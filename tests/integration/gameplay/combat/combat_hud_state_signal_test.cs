#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Gameplay.Combat;

public sealed class CombatHudStateSignalTest
{
    [Test]
    public void test_snapshot_exposes_hud_safe_combat_state_without_presentation_ownership()
    {
        var player = CreatePlayer().WithTarget("combat-hostile-1");
        var hostile = CreateHostile("combat-hostile-1", "hostile-001")
            .WithTarget(player.CombatActorId)
            .SetThreat(player.CombatActorId, 100);
        var attack = EnabledAttackMachine(player, hostile);
        var cast = new CombatCastStateMachine();
        var started = cast.StartCast(StartRequest(player, hostile, CombatTick.Zero));
        var progress = cast.GetProgress(new CombatTick(150, 3.0d));

        var snapshot = CombatHudStateProjection.Project(new CombatHudProjectionRequest(
            started.Caster,
            hostile,
            attack.CurrentState,
            new CombatTick(150, 3.0d),
            new CombatHudThreatCategoryRequest(
                started.Caster,
                hostile,
                new[] { started.Caster },
                ThreatTuning()),
            progress));

        Assert.That(snapshot.Health.Current, Is.EqualTo(140));
        Assert.That(snapshot.Health.Max, Is.EqualTo(140));
        Assert.That(snapshot.Mana.Current, Is.EqualTo(180));
        Assert.That(snapshot.Mana.Max, Is.EqualTo(180));
        Assert.That(snapshot.Endurance, Is.EqualTo(CombatHudEnduranceCategory.Full));
        Assert.That(snapshot.Target!.TargetCombatActorId, Is.EqualTo(hostile.CombatActorId));
        Assert.That(snapshot.Target.IsHostile, Is.True);
        Assert.That(snapshot.Cast.Category, Is.EqualTo(CombatHudCastCategory.Casting));
        Assert.That(snapshot.Cast.NormalizedProgress, Is.EqualTo(0.5d).Within(0.000001d));
        Assert.That(snapshot.AttackOn, Is.True);
        Assert.That(snapshot.NextSwingReadiness, Is.EqualTo(CombatHudSwingReadinessCategory.Ready));
        Assert.That(snapshot.ThreatCategory, Is.EqualTo(CombatHudThreatCategory.HasAggroStable));
        Assert.That(snapshot.CombatState, Is.EqualTo(CombatState.Casting));
    }

    [Test]
    public void test_attack_on_and_off_signals_project_to_hud_event_stream()
    {
        var player = CreatePlayer().WithTarget("combat-hostile-1");
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var machine = new CombatAttackStateMachine();

        var on = machine.ToggleOn(new CombatAttackToggleOnRequest(
            player,
            hostile,
            ActiveHauntGate(),
            DistanceMetersToTarget: 1.5d,
            new CombatTick(10, 0.2d),
            TickRateHz: 50));
        var off = machine.ToggleOff(CombatAttackTransitionPath.PlayerToggleOff, new CombatTick(151, 3.02d));
        var projectedOn = CombatHudStateProjection.ProjectAttackSignal(on);
        var projectedOff = CombatHudStateProjection.ProjectAttackSignal(off);
        var history = CombatHudStateProjection.ProjectAttackSignals(machine.StateChangedSignals);

        Assert.That(projectedOn, Is.Not.Null);
        Assert.That(projectedOn!.AttackOn, Is.True);
        Assert.That(projectedOn.TargetCombatActorId, Is.EqualTo(hostile.CombatActorId));
        Assert.That(projectedOff, Is.Not.Null);
        Assert.That(projectedOff!.AttackOn, Is.False);
        Assert.That(history, Has.Count.EqualTo(2));
        Assert.That(history.Last().AttackOn, Is.EqualTo(machine.CurrentState.IsAttackOn));
    }

    [TestCase(CombatAttackTransitionPath.TargetDeath)]
    [TestCase(CombatAttackTransitionPath.SuccessfulSitOrMed)]
    [TestCase(CombatAttackTransitionPath.CombatExit)]
    [TestCase(CombatAttackTransitionPath.PlayerDeath)]
    [TestCase(CombatAttackTransitionPath.ZoneTransition)]
    public void test_forced_off_transitions_project_hud_attack_off_signal(CombatAttackTransitionPath transitionPath)
    {
        var machine = EnabledAttackMachine(out _);

        var off = machine.ForceOff(transitionPath, new CombatTick(160, 3.2d));
        var projected = CombatHudStateProjection.ProjectAttackSignal(off);

        Assert.That(projected, Is.Not.Null);
        Assert.That(projected!.AttackOn, Is.False);
        Assert.That(projected.TargetCombatActorId, Is.Null);
        Assert.That(projected.TransitionPath, Is.EqualTo(transitionPath));
    }

    [Test]
    public void test_current_state_accessor_matches_latest_projected_attack_event()
    {
        var machine = EnabledAttackMachine(out _);

        var off = machine.ForceOff(CombatAttackTransitionPath.CombatExit, new CombatTick(160, 3.2d));
        var projected = CombatHudStateProjection.ProjectAttackSignal(off);

        Assert.That(projected, Is.Not.Null);
        Assert.That(projected!.AttackOn, Is.EqualTo(machine.CurrentState.IsAttackOn));
    }

    [Test]
    public void test_no_target_toggle_noop_emits_no_misleading_attack_on_pulse()
    {
        var player = CreatePlayer();
        var machine = new CombatAttackStateMachine();

        var noTarget = machine.ToggleOn(new CombatAttackToggleOnRequest(
            player,
            Target: null,
            ActiveHauntGate(),
            DistanceMetersToTarget: 0d,
            CombatTick.Zero,
            TickRateHz: 50));
        var projected = CombatHudStateProjection.ProjectAttackSignal(noTarget);
        var history = CombatHudStateProjection.ProjectAttackSignals(machine.StateChangedSignals);

        Assert.That(noTarget.Changed, Is.False);
        Assert.That(projected, Is.Null);
        Assert.That(history, Is.Empty);
        Assert.That(machine.CurrentState.IsAttackOn, Is.False);
    }

    private static CombatAttackStateMachine EnabledAttackMachine(out CombatActorState hostile)
    {
        var player = CreatePlayer().WithTarget("combat-hostile-1");
        hostile = CreateHostile("combat-hostile-1", "hostile-001");
        return EnabledAttackMachine(player, hostile);
    }

    private static CombatAttackStateMachine EnabledAttackMachine(
        CombatActorState player,
        CombatActorState hostile)
    {
        var machine = new CombatAttackStateMachine();
        var enabled = machine.ToggleOn(new CombatAttackToggleOnRequest(
            player,
            hostile,
            ActiveHauntGate(),
            DistanceMetersToTarget: 1.5d,
            new CombatTick(10, 0.2d),
            TickRateHz: 50));

        Assert.That(enabled.Succeeded, Is.True, string.Join(Environment.NewLine, enabled.RejectionReasons));
        return machine;
    }

    private static CombatCastStartRequest StartRequest(
        CombatActorState player,
        CombatActorState hostile,
        CombatTick tick)
    {
        return new CombatCastStartRequest(
            "cast-001",
            player,
            hostile,
            ActiveHauntGate(),
            DistanceMetersToTarget: 15.0d,
            Array.Empty<CombatLosLayer>(),
            tick,
            TickRateHz: 50,
            new CombatCastProfile(
                "Smite_T1_Prototype",
                CastTimeSeconds: 6.0d,
                ManaCost: 20,
                SpellRangeMeters: 30.0d,
                RecoverySeconds: 1.5d));
    }

    private static CombatHudThreatCategoryTuning ThreatTuning()
    {
        return new CombatHudThreatCategoryTuning(0.85d, 0.90d);
    }

    private static CombatZoneGate ActiveHauntGate()
    {
        var gate = new CombatZoneGate();
        gate.ActivateZone("Haunt_Prototype_T1", CombatZoneType.HauntZone);
        return gate;
    }

    private static CombatActorState CreatePlayer()
    {
        return new CombatActorState(
            "combat-player-1",
            CombatActorKind.Player,
            CombatStableSourceRef.ForPlayer("local-character-1"),
            "PlayerLocal_T1",
            "Haunt_Prototype_T1",
            5,
            140,
            140,
            180,
            180,
            35,
            25,
            8,
            30,
            30,
            2.8d,
            2.0d,
            30.0d,
            CombatState.OutOfCombat,
            CombatActorLifeState.Alive,
            null,
            "player-local-character-1",
            maxEndurance: 80,
            currentEndurance: 80);
    }

    private static CombatActorState CreateHostile(string combatActorId, string sortKey)
    {
        return new CombatActorState(
            combatActorId,
            CombatActorKind.NPC,
            CombatStableSourceRef.ForSpawn(new CombatSpawnSourceRef("VampireCourt_T1", $"{combatActorId}-anchor", "VampireThrall_T1")),
            "VampireCourt_T1",
            "Haunt_Prototype_T1",
            5,
            120,
            120,
            0,
            0,
            30,
            25,
            8,
            30,
            30,
            3.0d,
            2.0d,
            0.0d,
            CombatState.InCombat,
            CombatActorLifeState.Alive,
            null,
            sortKey);
    }
}
