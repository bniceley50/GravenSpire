#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Gameplay.Combat;

public sealed class CombatSlowCastFrameworkTest
{
    [Test]
    public void test_valid_six_second_cast_enters_casting_and_reports_progress()
    {
        var machine = new CombatCastStateMachine();
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var player = CreatePlayer().WithTarget(hostile.CombatActorId);

        var result = machine.StartCast(StartRequest(player, hostile, new CombatTick(0, 0d)));
        var progress = machine.GetProgress(new CombatTick(150, 3.0d));

        Assert.That(result.Succeeded, Is.True, string.Join(Environment.NewLine, result.RejectionReasons));
        Assert.That(result.Outcome, Is.EqualTo(CombatCastTransitionOutcome.Accepted));
        Assert.That(result.Caster.CombatState, Is.EqualTo(CombatState.Casting));
        Assert.That(result.Caster.CastRuntimeState, Is.EqualTo(CombatCastRuntimeState.Casting));
        Assert.That(result.Caster.ActiveCastId, Is.EqualTo("cast-t1-0001"));
        Assert.That(result.Caster.ActiveCastSpellId, Is.EqualTo("Smite_T1_Prototype"));
        Assert.That(result.Caster.ActiveCastTargetCombatActorId, Is.EqualTo(hostile.CombatActorId));
        Assert.That(machine.CurrentCast!.CompletionTick, Is.EqualTo(300));
        Assert.That(machine.LifecycleEvents.Single(), Is.TypeOf<CastStartedEvent>());
        AssertPayload(machine.LifecycleEvents.Single(), "cast-t1-0001", "Smite_T1_Prototype", player.CombatActorId, hostile.CombatActorId, 0);
        Assert.That(progress!.ProgressSeconds, Is.EqualTo(3.0d).Within(0.000001d));
        Assert.That(progress.NormalizedProgress, Is.EqualTo(0.5d).Within(0.000001d));
        Assert.That(progress.IsComplete, Is.False);
    }

    [Test]
    public void test_cast_completion_spends_mana_and_enters_recovery()
    {
        var machine = new CombatCastStateMachine();
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var started = machine.StartCast(StartRequest(CreatePlayer().WithTarget(hostile.CombatActorId), hostile, new CombatTick(0, 0d)));

        var completed = machine.ResolveCompletion(started.Caster, hostile, new CombatTick(300, 6.0d));

        Assert.That(completed.Outcome, Is.EqualTo(CombatCastTransitionOutcome.Completed));
        Assert.That(completed.Caster.CurrentMana, Is.EqualTo(150));
        Assert.That(completed.Caster.CombatState, Is.EqualTo(CombatState.Recovery));
        Assert.That(completed.Caster.CastRuntimeState, Is.EqualTo(CombatCastRuntimeState.Recovery));
        Assert.That(completed.Caster.CastRecoveryRemainingSeconds, Is.EqualTo(1.5d).Within(0.000001d));
        Assert.That(completed.Events.OfType<CastCompletedEvent>().Single().ManaSpent, Is.EqualTo(30));
        Assert.That(completed.Events.OfType<CastRecoveryStartedEvent>().Single().RecoverySeconds, Is.EqualTo(1.5d).Within(0.000001d));
        Assert.That(machine.CurrentCast!.RecoveryEndTick, Is.EqualTo(375));
    }

    [Test]
    public void test_manual_cancel_emits_event_and_spends_no_mana()
    {
        var machine = new CombatCastStateMachine();
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var started = machine.StartCast(StartRequest(CreatePlayer().WithTarget(hostile.CombatActorId), hostile, new CombatTick(0, 0d)));

        var cancelled = machine.CancelCast(started.Caster, new CombatTick(100, 2.0d));

        Assert.That(cancelled.Outcome, Is.EqualTo(CombatCastTransitionOutcome.Cancelled));
        Assert.That(cancelled.Caster.CurrentMana, Is.EqualTo(180));
        Assert.That(cancelled.Caster.CombatState, Is.EqualTo(CombatState.Recovery));
        Assert.That(cancelled.Events.OfType<CastCancelledEvent>().Single().CancelSource, Is.EqualTo("manual_cancel"));
        Assert.That(cancelled.Events.OfType<CastRecoveryStartedEvent>().Single().Tick.Index, Is.EqualTo(100));
        Assert.That(machine.LifecycleEvents.OfType<CastCompletedEvent>(), Is.Empty);
    }

    [Test]
    public void test_damage_interrupt_emits_event_and_spends_no_mana()
    {
        var machine = new CombatCastStateMachine();
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var started = machine.StartCast(StartRequest(CreatePlayer().WithTarget(hostile.CombatActorId), hostile, new CombatTick(0, 0d)));
        var random = new ScriptedInterruptRandomSource(new[] { 0.10d });

        var interrupted = machine.InterruptFromDamage(
            started.Caster,
            hostile,
            new CombatDamageInterruptRequest(
                DamageTaken: 10,
                DamageWasBlockedOrAbsorbed: false,
                new CombatTick(200, 4.0d),
                InterruptTuning(),
                random));
        var recovery = machine.BeginRecoveryAfterInterrupt(interrupted.Caster, new CombatTick(200, 4.0d));

        Assert.That(interrupted.Outcome, Is.EqualTo(CombatCastTransitionOutcome.Interrupted));
        Assert.That(interrupted.Caster.CurrentMana, Is.EqualTo(180));
        Assert.That(interrupted.Caster.CombatState, Is.EqualTo(CombatState.Interrupted));
        Assert.That(interrupted.Caster.CastRuntimeState, Is.EqualTo(CombatCastRuntimeState.Interrupted));
        Assert.That(interrupted.Events.OfType<CastInterruptedEvent>().Single().InterruptRoll, Is.EqualTo(0.10d).Within(0.000001d));
        Assert.That(random.RollCalls, Is.EqualTo(1));
        Assert.That(recovery.Outcome, Is.EqualTo(CombatCastTransitionOutcome.RecoveryStarted));
        Assert.That(recovery.Caster.CombatState, Is.EqualTo(CombatState.Recovery));
    }

    [Test]
    public void test_zero_and_blocked_damage_do_not_roll_interrupt()
    {
        var machine = new CombatCastStateMachine();
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var started = machine.StartCast(StartRequest(CreatePlayer().WithTarget(hostile.CombatActorId), hostile, new CombatTick(0, 0d)));
        var random = new ScriptedInterruptRandomSource(new[] { 0.0d });

        var zeroDamage = machine.InterruptFromDamage(
            started.Caster,
            hostile,
            new CombatDamageInterruptRequest(
                DamageTaken: 0,
                DamageWasBlockedOrAbsorbed: false,
                new CombatTick(50, 1.0d),
                InterruptTuning(),
                random));
        var blockedDamage = machine.InterruptFromDamage(
            zeroDamage.Caster,
            hostile,
            new CombatDamageInterruptRequest(
                DamageTaken: 10,
                DamageWasBlockedOrAbsorbed: true,
                new CombatTick(100, 2.0d),
                InterruptTuning(),
                random));

        Assert.That(zeroDamage.Outcome, Is.EqualTo(CombatCastTransitionOutcome.NoInterruptRoll));
        Assert.That(blockedDamage.Outcome, Is.EqualTo(CombatCastTransitionOutcome.NoInterruptRoll));
        Assert.That(random.RollCalls, Is.EqualTo(0));
        Assert.That(machine.LifecycleEvents.OfType<CastInterruptedEvent>(), Is.Empty);
        Assert.That(blockedDamage.Caster.CurrentMana, Is.EqualTo(180));
    }

    [Test]
    public void test_same_tick_completion_resolves_before_new_interrupt_check()
    {
        var machine = new CombatCastStateMachine();
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var started = machine.StartCast(StartRequest(CreatePlayer().WithTarget(hostile.CombatActorId), hostile, new CombatTick(0, 0d)));
        var random = new ScriptedInterruptRandomSource(new[] { 0.0d });

        var result = machine.InterruptFromDamage(
            started.Caster,
            hostile,
            new CombatDamageInterruptRequest(
                DamageTaken: 90,
                DamageWasBlockedOrAbsorbed: false,
                new CombatTick(300, 6.0d),
                InterruptTuning(),
                random));

        Assert.That(result.Outcome, Is.EqualTo(CombatCastTransitionOutcome.Completed));
        Assert.That(result.Caster.CurrentMana, Is.EqualTo(150));
        Assert.That(random.RollCalls, Is.EqualTo(0));
        Assert.That(result.Events.OfType<CastCompletedEvent>().Single().Tick.Index, Is.EqualTo(300));
        Assert.That(machine.LifecycleEvents.OfType<CastInterruptedEvent>(), Is.Empty);
    }

    [Test]
    public void test_lifecycle_payloads_include_ids_targets_and_combat_ticks()
    {
        var completionMachine = new CombatCastStateMachine();
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var started = completionMachine.StartCast(StartRequest(CreatePlayer().WithTarget(hostile.CombatActorId), hostile, new CombatTick(10, 0.2d)));
        var completed = completionMachine.ResolveCompletion(started.Caster, hostile, new CombatTick(310, 6.2d));
        var ended = completionMachine.EndRecovery(completed.Caster, new CombatTick(385, 7.7d));

        Assert.That(ended.Outcome, Is.EqualTo(CombatCastTransitionOutcome.RecoveryEnded));
        AssertPayload(completionMachine.LifecycleEvents.OfType<CastStartedEvent>().Single(), "cast-t1-0001", "Smite_T1_Prototype", "combat-player-1", hostile.CombatActorId, 10);
        AssertPayload(completionMachine.LifecycleEvents.OfType<CastCompletedEvent>().Single(), "cast-t1-0001", "Smite_T1_Prototype", "combat-player-1", hostile.CombatActorId, 310);
        AssertPayload(completionMachine.LifecycleEvents.OfType<CastRecoveryStartedEvent>().Single(), "cast-t1-0001", "Smite_T1_Prototype", "combat-player-1", hostile.CombatActorId, 310);
        AssertPayload(completionMachine.LifecycleEvents.OfType<CastRecoveryEndedEvent>().Single(), "cast-t1-0001", "Smite_T1_Prototype", "combat-player-1", hostile.CombatActorId, 385);

        var cancelMachine = new CombatCastStateMachine();
        var cancelStarted = cancelMachine.StartCast(StartRequest(CreatePlayer().WithTarget(hostile.CombatActorId), hostile, new CombatTick(0, 0d), castId: "cast-t1-0002"));
        _ = cancelMachine.CancelCast(cancelStarted.Caster, new CombatTick(40, 0.8d));
        AssertPayload(cancelMachine.LifecycleEvents.OfType<CastCancelledEvent>().Single(), "cast-t1-0002", "Smite_T1_Prototype", "combat-player-1", hostile.CombatActorId, 40);

        var interruptMachine = new CombatCastStateMachine();
        var interruptStarted = interruptMachine.StartCast(StartRequest(CreatePlayer().WithTarget(hostile.CombatActorId), hostile, new CombatTick(0, 0d), castId: "cast-t1-0003"));
        var interrupted = interruptMachine.InterruptFromDamage(
            interruptStarted.Caster,
            hostile,
            new CombatDamageInterruptRequest(10, false, new CombatTick(200, 4.0d), InterruptTuning(), new ScriptedInterruptRandomSource(new[] { 0.10d })));
        _ = interruptMachine.BeginRecoveryAfterInterrupt(interrupted.Caster, new CombatTick(200, 4.0d));
        AssertPayload(interruptMachine.LifecycleEvents.OfType<CastInterruptedEvent>().Single(), "cast-t1-0003", "Smite_T1_Prototype", "combat-player-1", hostile.CombatActorId, 200);
        AssertPayload(interruptMachine.LifecycleEvents.OfType<CastRecoveryStartedEvent>().Single(), "cast-t1-0003", "Smite_T1_Prototype", "combat-player-1", hostile.CombatActorId, 200);
    }

    private static CombatCastStartRequest StartRequest(
        CombatActorState player,
        CombatActorState hostile,
        CombatTick tick,
        string castId = "cast-t1-0001")
    {
        return new CombatCastStartRequest(
            castId,
            player,
            hostile,
            ActiveHauntGate(),
            DistanceMetersToTarget: 20.0d,
            Array.Empty<CombatLosLayer>(),
            tick,
            TickRateHz: 50,
            SlowSmiteProfile());
    }

    private static CombatCastProfile SlowSmiteProfile()
    {
        return new CombatCastProfile(
            "Smite_T1_Prototype",
            CastTimeSeconds: 6.0d,
            ManaCost: 30,
            SpellRangeMeters: 30.0d,
            RecoverySeconds: 1.5d);
    }

    private static CombatInterruptFormulaTuning InterruptTuning()
    {
        return new CombatInterruptFormulaTuning(
            BaseInterruptChance: 0.20d,
            DamageInterruptScalar: 4.0d,
            EarlyCastInterruptScalar: 0.10d,
            InterruptChanceMin: 0.05d,
            InterruptChanceMax: 0.85d);
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

    private static void AssertPayload(
        ICombatCastLifecycleEvent lifecycleEvent,
        string castId,
        string spellId,
        string casterCombatActorId,
        string? targetCombatActorId,
        long tick)
    {
        Assert.That(lifecycleEvent.CastId, Is.EqualTo(castId));
        Assert.That(lifecycleEvent.SpellId, Is.EqualTo(spellId));
        Assert.That(lifecycleEvent.CasterCombatActorId, Is.EqualTo(casterCombatActorId));
        Assert.That(lifecycleEvent.TargetCombatActorId, Is.EqualTo(targetCombatActorId));
        Assert.That(lifecycleEvent.Tick.Index, Is.EqualTo(tick));
    }

    private sealed class ScriptedInterruptRandomSource : ICombatCastInterruptRandomSource
    {
        private readonly Queue<double> rolls;

        public ScriptedInterruptRandomSource(IEnumerable<double> rolls)
        {
            this.rolls = new Queue<double>(rolls);
        }

        public int RollCalls { get; private set; }

        public double NextInterruptRoll()
        {
            RollCalls++;
            return rolls.Dequeue();
        }
    }
}
