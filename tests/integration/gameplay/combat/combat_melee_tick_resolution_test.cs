#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Gameplay.Combat;

public sealed class CombatMeleeTickResolutionTest
{
    [Test]
    public void test_fixed_tick_melee_resolution_at_fifty_hz_is_frame_rate_independent()
    {
        var oneFrame = RunScenario(new[] { 140 });
        var variableFrames = RunScenario(new[] { 13, 27, 50, 50 });

        Assert.That(oneFrame.EndTick.Index, Is.EqualTo(140));
        Assert.That(variableFrames.EndTick.Index, Is.EqualTo(140));
        Assert.That(oneFrame.MeleeResults.Select(result => result.Tick.Index), Is.EqualTo(new[] { 140L }));
        Assert.That(variableFrames.MeleeResults.Select(result => result.Tick.Index), Is.EqualTo(new[] { 140L }));
        Assert.That(oneFrame.MeleeResults.Single().Outcome, Is.EqualTo(CombatMeleeTickOutcome.Hit));
        Assert.That(variableFrames.MeleeResults.Single().Outcome, Is.EqualTo(CombatMeleeTickOutcome.Hit));
    }

    [Test]
    public void test_pause_does_not_advance_clock_or_wall_clock_catch_up_melee_ticks()
    {
        var clock = new FixedCombatClock(tickRateHz: 50);
        var resolver = new CombatMeleeResolver();
        var random = new ScriptedMeleeRandomSource(hitRolls: new[] { 0.0d }, damageRollScalars: new[] { 1.0d });
        var player = CreatePlayer().WithTarget("combat-hostile-1");
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var attackState = new CombatAttackStateSnapshot(
            CombatAttackMode.On,
            hostile.CombatActorId,
            NextSwingDueTick: 1,
            CombatAttackTransitionPath.PlayerToggleOn);
        var stepper = new CombatSimulationStepper();

        var paused = stepper.Step(new CombatSimulationStepRequest(
            clock,
            TickBudget: 500,
            IsPaused: true,
            ResolveMeleeTick: tick => Resolve(resolver, random, player, hostile, attackState, tick)));
        var resumed = stepper.Step(new CombatSimulationStepRequest(
            clock,
            TickBudget: 1,
            IsPaused: false,
            ResolveMeleeTick: tick => Resolve(resolver, random, player, hostile, attackState, tick)));

        Assert.That(paused.StartTick.Index, Is.EqualTo(0));
        Assert.That(paused.EndTick.Index, Is.EqualTo(0));
        Assert.That(paused.MeleeResults, Is.Empty);
        Assert.That(resumed.StartTick.Index, Is.EqualTo(0));
        Assert.That(resumed.EndTick.Index, Is.EqualTo(1));
        Assert.That(resumed.MeleeResults.Single().Tick.Index, Is.EqualTo(1));
        Assert.That(random.HitRollCalls, Is.EqualTo(1));
    }

    [Test]
    public void test_out_of_range_swing_skips_damage_and_schedules_only_next_weapon_delay()
    {
        var clock = new FixedCombatClock(tickRateHz: 50, initialTick: 9);
        var resolver = new CombatMeleeResolver();
        var random = new ScriptedMeleeRandomSource(hitRolls: Array.Empty<double>(), damageRollScalars: Array.Empty<double>());
        var player = CreatePlayer().WithTarget("combat-hostile-1");
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var attackState = new CombatAttackStateSnapshot(
            CombatAttackMode.On,
            hostile.CombatActorId,
            NextSwingDueTick: 10,
            CombatAttackTransitionPath.PlayerToggleOn);
        var stepper = new CombatSimulationStepper();

        var result = stepper.Step(new CombatSimulationStepRequest(
            clock,
            TickBudget: 200,
            IsPaused: false,
            ResolveMeleeTick: tick =>
            {
                var resolved = Resolve(
                    resolver,
                    random,
                    player,
                    hostile,
                    attackState,
                    tick,
                    distanceMetersToTarget: 3.0d);

                if (resolved.Outcome != CombatMeleeTickOutcome.NotDue)
                {
                    attackState = attackState with { NextSwingDueTick = resolved.NextSwingDueTick };
                    return resolved;
                }

                return null;
            }));

        Assert.That(result.MeleeResults.Select(item => item.Tick.Index), Is.EqualTo(new[] { 10L, 150L }));
        Assert.That(result.MeleeResults, Has.All.Property(nameof(CombatMeleeTickResult.Outcome)).EqualTo(CombatMeleeTickOutcome.OutOfRange));
        Assert.That(result.MeleeResults, Has.All.Property(nameof(CombatMeleeTickResult.Damage)).EqualTo(0));
        Assert.That(random.HitRollCalls, Is.EqualTo(0));
    }

    [TestCase("out of range", 3.0d, 0.0d, false, false, CombatMeleeTickOutcome.OutOfRange)]
    [TestCase("bad facing", 1.5d, 120.0d, false, false, CombatMeleeTickOutcome.FacingBlocked)]
    [TestCase("blocked sight", 1.5d, 0.0d, true, false, CombatMeleeTickOutcome.LineOfSightBlocked)]
    [TestCase("same tick death", 1.5d, 0.0d, false, true, CombatMeleeTickOutcome.TargetAlreadyDead)]
    public void test_eligible_weapon_tick_revalidates_range_facing_los_and_alive_state(
        string caseName,
        double distance,
        double facingDegrees,
        bool blockedSight,
        bool targetDeathResolved,
        CombatMeleeTickOutcome expectedOutcome)
    {
        _ = caseName;
        var resolver = new CombatMeleeResolver();
        var random = new ScriptedMeleeRandomSource(hitRolls: Array.Empty<double>(), damageRollScalars: Array.Empty<double>());
        var player = CreatePlayer().WithTarget("combat-hostile-1");
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var attackState = new CombatAttackStateSnapshot(
            CombatAttackMode.On,
            hostile.CombatActorId,
            NextSwingDueTick: 140,
            CombatAttackTransitionPath.PlayerToggleOn);

        var result = Resolve(
            resolver,
            random,
            player,
            hostile,
            attackState,
            new CombatTick(140, 2.8d),
            distance,
            facingDegrees,
            blockedSight ? new[] { CombatLosLayer.WorldSolid } : Array.Empty<CombatLosLayer>(),
            targetDeathResolved);

        Assert.That(result.Outcome, Is.EqualTo(expectedOutcome));
        Assert.That(result.Damage, Is.EqualTo(0));
        if (targetDeathResolved)
        {
            Assert.That(result.ShouldForceAttackOff, Is.True);
            Assert.That(result.AttackOffPath, Is.EqualTo(CombatAttackTransitionPath.TargetDeath));
        }
    }

    [Test]
    public void test_city_zone_gate_blocks_damage_on_each_eligible_tick()
    {
        var resolver = new CombatMeleeResolver();
        var random = new ScriptedMeleeRandomSource(hitRolls: Array.Empty<double>(), damageRollScalars: Array.Empty<double>());
        var player = CreatePlayer(zoneId: "CityHub_T1").WithTarget("combat-hostile-1");
        var hostile = CreateHostile("combat-hostile-1", "hostile-001", zoneId: "CityHub_T1");
        var gate = new CombatZoneGate();
        gate.ActivateZone("CityHub_T1", CombatZoneType.CityHubZone);
        var attackState = new CombatAttackStateSnapshot(
            CombatAttackMode.On,
            hostile.CombatActorId,
            NextSwingDueTick: 1,
            CombatAttackTransitionPath.PlayerToggleOn);

        var result = resolver.ResolveTick(Request(
            player,
            hostile,
            attackState,
            new CombatTick(1, 0.02d),
            random,
            gate));

        Assert.That(result.Outcome, Is.EqualTo(CombatMeleeTickOutcome.ZoneBlocked));
        Assert.That(result.Damage, Is.EqualTo(0));
        Assert.That(result.ShouldForceAttackOff, Is.True);
        Assert.That(result.AttackOffPath, Is.EqualTo(CombatAttackTransitionPath.ZoneTransition));
    }

    [Test]
    public void test_same_tick_death_priority_discards_scheduled_swing_before_random_roll()
    {
        var resolver = new CombatMeleeResolver();
        var random = new ScriptedMeleeRandomSource(hitRolls: new[] { 0.0d }, damageRollScalars: new[] { 1.0d });
        var player = CreatePlayer().WithTarget("combat-hostile-1");
        var hostile = CreateHostile("combat-hostile-1", "hostile-001", currentHealth: 0, lifeState: CombatActorLifeState.Dead);
        var attackState = new CombatAttackStateSnapshot(
            CombatAttackMode.On,
            hostile.CombatActorId,
            NextSwingDueTick: 140,
            CombatAttackTransitionPath.PlayerToggleOn);

        var result = Resolve(
            resolver,
            random,
            player,
            hostile,
            attackState,
            new CombatTick(140, 2.8d),
            targetDeathResolvedBeforeSwing: true);

        Assert.That(result.Outcome, Is.EqualTo(CombatMeleeTickOutcome.TargetAlreadyDead));
        Assert.That(result.Damage, Is.EqualTo(0));
        Assert.That(result.ShouldForceAttackOff, Is.True);
        Assert.That(result.AttackOffPath, Is.EqualTo(CombatAttackTransitionPath.TargetDeath));
        Assert.That(random.HitRollCalls, Is.EqualTo(0));
        Assert.That(random.DamageRollCalls, Is.EqualTo(0));
    }

    private static CombatSimulationStepResult RunScenario(IReadOnlyList<int> tickBudgets)
    {
        var clock = new FixedCombatClock(tickRateHz: 50);
        var resolver = new CombatMeleeResolver();
        var random = new ScriptedMeleeRandomSource(hitRolls: new[] { 0.0d }, damageRollScalars: new[] { 1.0d });
        var player = CreatePlayer().WithTarget("combat-hostile-1");
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var attackState = new CombatAttackStateSnapshot(
            CombatAttackMode.On,
            hostile.CombatActorId,
            NextSwingDueTick: 140,
            CombatAttackTransitionPath.PlayerToggleOn);
        var stepper = new CombatSimulationStepper();
        CombatSimulationStepResult? finalResult = null;

        foreach (var budget in tickBudgets)
        {
            finalResult = stepper.Step(new CombatSimulationStepRequest(
                clock,
                budget,
                IsPaused: false,
                ResolveMeleeTick: tick =>
                {
                    var resolved = Resolve(resolver, random, player, hostile, attackState, tick);
                    if (resolved.Outcome == CombatMeleeTickOutcome.NotDue)
                    {
                        return null;
                    }

                    attackState = attackState with { NextSwingDueTick = resolved.NextSwingDueTick };
                    return resolved;
                }));
        }

        return finalResult ?? throw new InvalidOperationException("Scenario did not run.");
    }

    private static CombatMeleeTickResult Resolve(
        CombatMeleeResolver resolver,
        ICombatMeleeRandomSource random,
        CombatActorState player,
        CombatActorState hostile,
        CombatAttackStateSnapshot attackState,
        CombatTick tick,
        double distanceMetersToTarget = 1.5d,
        double facingDegreesToTarget = 0.0d,
        IReadOnlyList<CombatLosLayer>? losBlockingLayers = null,
        bool targetDeathResolvedBeforeSwing = false)
    {
        return resolver.ResolveTick(Request(
            player,
            hostile,
            attackState,
            tick,
            random,
            ActiveHauntGate(player.ZoneId),
            distanceMetersToTarget,
            facingDegreesToTarget,
            losBlockingLayers ?? Array.Empty<CombatLosLayer>(),
            targetDeathResolvedBeforeSwing));
    }

    private static CombatMeleeTickRequest Request(
        CombatActorState player,
        CombatActorState hostile,
        CombatAttackStateSnapshot attackState,
        CombatTick tick,
        ICombatMeleeRandomSource random,
        CombatZoneGate gate,
        double distanceMetersToTarget = 1.5d,
        double facingDegreesToTarget = 0.0d,
        IReadOnlyList<CombatLosLayer>? losBlockingLayers = null,
        bool targetDeathResolvedBeforeSwing = false)
    {
        return new CombatMeleeTickRequest(
            player,
            hostile,
            attackState,
            gate,
            distanceMetersToTarget,
            facingDegreesToTarget,
            FacingToleranceDegrees: 90.0d,
            losBlockingLayers ?? Array.Empty<CombatLosLayer>(),
            tick,
            TickRateHz: 50,
            DefaultHitTuning(),
            DefaultDamageTuning(),
            random,
            targetDeathResolvedBeforeSwing);
    }

    private static CombatMeleeHitChanceTuning DefaultHitTuning()
    {
        return new CombatMeleeHitChanceTuning(
            BaseHitChance: 0.72d,
            LevelHitDelta: 0.03d,
            SkillHitDelta: 0.001d,
            HitChanceMin: 0.10d,
            HitChanceMax: 0.92d);
    }

    private static CombatMeleeDamageTuning DefaultDamageTuning()
    {
        return new CombatMeleeDamageTuning(
            AttackPowerScalar: 0.20d,
            ArmorMitigationScalar: 0.10d);
    }

    private static CombatZoneGate ActiveHauntGate(string zoneId = "Haunt_Prototype_T1")
    {
        var gate = new CombatZoneGate();
        gate.ActivateZone(zoneId, CombatZoneType.HauntZone);
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

    private static CombatActorState CreateHostile(
        string combatActorId,
        string sortKey,
        string zoneId = "Haunt_Prototype_T1",
        int currentHealth = 120,
        CombatActorLifeState lifeState = CombatActorLifeState.Alive)
    {
        return new CombatActorState(
            combatActorId,
            CombatActorKind.NPC,
            CombatStableSourceRef.ForSpawn(new CombatSpawnSourceRef("VampireCourt_T1", $"{combatActorId}-anchor", "VampireThrall_T1")),
            "VampireCourt_T1",
            zoneId,
            5,
            120,
            currentHealth,
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
            lifeState == CombatActorLifeState.Alive ? CombatState.OutOfCombat : CombatState.Dead,
            lifeState,
            null,
            sortKey);
    }

    private sealed class ScriptedMeleeRandomSource : ICombatMeleeRandomSource
    {
        private readonly Queue<double> hitRolls;
        private readonly Queue<double> damageRollScalars;

        public ScriptedMeleeRandomSource(IEnumerable<double> hitRolls, IEnumerable<double> damageRollScalars)
        {
            this.hitRolls = new Queue<double>(hitRolls);
            this.damageRollScalars = new Queue<double>(damageRollScalars);
        }

        public int HitRollCalls { get; private set; }

        public int DamageRollCalls { get; private set; }

        public double NextHitRoll()
        {
            HitRollCalls++;
            return hitRolls.Dequeue();
        }

        public double NextDamageRollScalar()
        {
            DamageRollCalls++;
            return damageRollScalars.Dequeue();
        }
    }
}
