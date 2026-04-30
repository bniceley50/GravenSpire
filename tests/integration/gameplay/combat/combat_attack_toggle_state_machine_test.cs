#nullable enable

using System;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Gameplay.Combat;

public sealed class CombatAttackToggleStateMachineTest
{
    [Test]
    public void test_player_toggle_on_with_valid_hostile_target_turns_attack_on_and_schedules_weapon_tick()
    {
        var gate = ActiveHauntGate();
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var player = CreatePlayer().WithTarget(hostile.CombatActorId);
        var machine = new CombatAttackStateMachine();

        var result = machine.ToggleOn(new CombatAttackToggleOnRequest(
            player,
            hostile,
            gate,
            DistanceMetersToTarget: 1.5d,
            new CombatTick(10, 0.2d),
            TickRateHz: 50));

        Assert.That(result.Succeeded, Is.True, string.Join(Environment.NewLine, result.RejectionReasons));
        Assert.That(result.Changed, Is.True);
        Assert.That(result.Snapshot.IsAttackOn, Is.True);
        Assert.That(result.Snapshot.TargetCombatActorId, Is.EqualTo(hostile.CombatActorId));
        Assert.That(result.Snapshot.NextSwingDueTick, Is.EqualTo(150));
        Assert.That(machine.CurrentState.IsAttackOn, Is.True);
        Assert.That(machine.StateChangedSignals.Single().AttackOn, Is.True);
        Assert.That(machine.StateChangedSignals.Single().TransitionPath, Is.EqualTo(CombatAttackTransitionPath.PlayerToggleOn));
    }

    [Test]
    public void test_player_toggle_off_is_an_approved_command_path_and_emits_hud_safe_off_signal()
    {
        var machine = EnabledMachine(out _);

        var result = machine.ToggleOff(CombatAttackTransitionPath.PlayerToggleOff, new CombatTick(151, 3.02d));

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Changed, Is.True);
        Assert.That(result.Snapshot.IsAttackOn, Is.False);
        Assert.That(result.StateChangedSignal, Is.Not.Null);
        Assert.That(result.StateChangedSignal!.AttackOn, Is.False);
        Assert.That(result.StateChangedSignal.TransitionPath, Is.EqualTo(CombatAttackTransitionPath.PlayerToggleOff));
        Assert.That(machine.CurrentState.IsAttackOn, Is.False);
    }

    [TestCase(CombatAttackPassivePath.TargetSelection)]
    [TestCase(CombatAttackPassivePath.TabCycle)]
    [TestCase(CombatAttackPassivePath.BodyPull)]
    [TestCase(CombatAttackPassivePath.SocialAssist)]
    [TestCase(CombatAttackPassivePath.SpellPull)]
    [TestCase(CombatAttackPassivePath.SpellCast)]
    public void test_passive_combat_paths_do_not_enable_attack(CombatAttackPassivePath passivePath)
    {
        var machine = new CombatAttackStateMachine();

        var result = machine.ObservePassivePath(passivePath, CombatTick.Zero);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Changed, Is.False);
        Assert.That(machine.CurrentState.IsAttackOn, Is.False);
        Assert.That(machine.StateChangedSignals, Is.Empty);
    }

    [Test]
    public void test_toggle_on_with_no_valid_hostile_target_noops_and_leaves_attack_off()
    {
        var gate = ActiveHauntGate();
        var player = CreatePlayer();
        var machine = new CombatAttackStateMachine();

        var result = machine.ToggleOn(new CombatAttackToggleOnRequest(
            player,
            Target: null,
            gate,
            DistanceMetersToTarget: 0d,
            CombatTick.Zero,
            TickRateHz: 50));

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Changed, Is.False);
        Assert.That(result.RejectionReasons, Has.Some.Contains("valid hostile target"));
        Assert.That(machine.CurrentState.IsAttackOn, Is.False);
        Assert.That(machine.StateChangedSignals, Is.Empty);
    }

    [TestCase(CombatAttackTransitionPath.TargetDeath)]
    [TestCase(CombatAttackTransitionPath.SuccessfulSitOrMed)]
    [TestCase(CombatAttackTransitionPath.CombatExit)]
    [TestCase(CombatAttackTransitionPath.PlayerDeath)]
    [TestCase(CombatAttackTransitionPath.ZoneTransition)]
    public void test_forced_off_conditions_turn_attack_off_and_emit_hud_safe_signal(CombatAttackTransitionPath transitionPath)
    {
        var machine = EnabledMachine(out var hostile);

        var result = machine.ForceOff(transitionPath, new CombatTick(160, 3.2d));

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Changed, Is.True);
        Assert.That(result.Snapshot.IsAttackOn, Is.False);
        Assert.That(result.Snapshot.TargetCombatActorId, Is.Null);
        Assert.That(result.StateChangedSignal, Is.Not.Null);
        Assert.That(result.StateChangedSignal!.AttackOn, Is.False);
        Assert.That(result.StateChangedSignal.TargetCombatActorId, Is.Null);
        Assert.That(result.StateChangedSignal.TransitionPath, Is.EqualTo(transitionPath));
        Assert.That(machine.StateChangedSignals.First().TargetCombatActorId, Is.EqualTo(hostile.CombatActorId));
    }

    [Test]
    public void test_successful_sit_forces_attack_off_before_later_regen_or_threat_updates()
    {
        var machine = EnabledMachine(out _);

        var result = machine.ForceOff(CombatAttackTransitionPath.SuccessfulSitOrMed, new CombatTick(160, 3.2d));

        Assert.That(result.Changed, Is.True);
        Assert.That(result.Snapshot.IsAttackOn, Is.False);
        Assert.That(result.StateChangedSignal!.TransitionPath, Is.EqualTo(CombatAttackTransitionPath.SuccessfulSitOrMed));
        Assert.That(machine.StateChangedSignals.Last().AttackOn, Is.False);
    }

    [Test]
    public void test_unapproved_transition_path_cannot_change_attack_state()
    {
        var machine = EnabledMachine(out _);

        Assert.Throws<ArgumentException>(() =>
            machine.ToggleOff(CombatAttackTransitionPath.PlayerToggleOn, new CombatTick(160, 3.2d)));

        Assert.That(machine.CurrentState.IsAttackOn, Is.True);
        Assert.That(machine.StateChangedSignals, Has.Count.EqualTo(1));
    }

    private static CombatAttackStateMachine EnabledMachine(out CombatActorState hostile)
    {
        hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var player = CreatePlayer().WithTarget(hostile.CombatActorId);
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

    private static CombatZoneGate ActiveHauntGate()
    {
        var gate = new CombatZoneGate();
        gate.ActivateZone("Haunt_Prototype_T1", CombatZoneType.HauntZone);
        return gate;
    }

    private static CombatActorState CreatePlayer(string zoneId = "Haunt_Prototype_T1")
    {
        return new CombatActorState(
            "combat-player-1",
            CombatActorKind.Player,
            CombatStableSourceRef.ForPlayer("local-character-1"),
            "PlayerLocal_T1",
            zoneId,
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
            "player-local-character-1");
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
            CombatState.OutOfCombat,
            CombatActorLifeState.Alive,
            null,
            sortKey);
    }
}
