#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Combat.Fixtures;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Gameplay.Combat;

public sealed class CombatRuntimeSingleTrashMedLoopTest
{
    [Test]
    public void test_single_trash_two_pull_med_loop_records_pull_attack_defeat_exit_and_mana_restoration()
    {
        var package = LoadPackage();
        var loop = new SingleTrashLoopHarness(package);

        loop.RunPull();
        loop.SitAndMed();
        loop.RespawnTrash();
        loop.RunPull();

        Assert.That(loop.PullStarts, Is.EqualTo(2));
        Assert.That(loop.PullDidNotAutoEnableAttack, Is.True);
        Assert.That(loop.AttackTransitions, Is.EqualTo(2));
        Assert.That(loop.HostileDefeats, Is.EqualTo(2));
        Assert.That(loop.CombatExits, Is.EqualTo(2));
        Assert.That(loop.SitMedStarts, Is.EqualTo(1));
        Assert.That(loop.ManaRestored, Is.GreaterThan(0));
        Assert.That(loop.ManaAfterMedBreak, Is.GreaterThan(loop.ManaAfterFirstPull));
        Assert.That(loop.Player.IsAlive, Is.True);
    }

    private sealed class SingleTrashLoopHarness
    {
        private readonly CombatFixturePackage package;
        private readonly CombatRuntimeEncounterHydrator hydrator = new();
        private readonly CombatMeleeResolver melee = new();
        private readonly CombatRegenResolver regen = new();
        private readonly CombatInstantAbilityResolver instant = new();
        private readonly CombatZoneGate zoneGate = new();
        private readonly FixedCombatClock clock;
        private readonly CombatTacticalAbilityProfile smiteProfile;
        private CombatActorState hostile;
        private CombatAttackStateSnapshot playerAttack = new(CombatAttackMode.Off, null, null, null);
        private CombatAttackStateSnapshot hostileAttack = new(CombatAttackMode.Off, null, null, null);

        public SingleTrashLoopHarness(CombatFixturePackage package)
        {
            this.package = package;
            clock = new FixedCombatClock(package.CombatTickRateHz);
            zoneGate.ActivateZone("Haunt_Prototype_T1", CombatZoneType.HauntZone);
            smiteProfile = CombatTacticalAbilityProfile.FromFixture(
                package.TacticalInstantAbilityProfiles.Single(profile => profile.Id == "SmiteOfAuthority_T1_Prototype"),
                "Mid");

            var hydration = Hydrate();
            Player = hydration.PlayerActor ?? throw new InvalidOperationException("Player actor did not hydrate.");
            hostile = hydration.HostileActors.Single();
        }

        public CombatActorState Player { get; private set; }

        public int PullStarts { get; private set; }

        public bool PullDidNotAutoEnableAttack { get; private set; }

        public int AttackTransitions { get; private set; }

        public int HostileDefeats { get; private set; }

        public int CombatExits { get; private set; }

        public int SitMedStarts { get; private set; }

        public int ManaRestored { get; private set; }

        public int ManaAfterFirstPull { get; private set; }

        public int ManaAfterMedBreak { get; private set; }

        public void RunPull()
        {
            var pull = new CombatPullCoordinator().ResolveBodyPull(
                Player,
                new CombatPoint3(0, 0, 0),
                new CombatPullCandidate(
                    hostile,
                    new CombatPoint3(0, 0, 1.5d),
                    new CombatSpatialAnchorSet(new CombatPoint3(0, 0, 1.5d), new CombatPoint3(0, 0, 0), "M2_BaselineTrash", "ClericShellMarker"),
                    AggroRadiusMeters: 4d,
                    CombatSocialAssistProfile.T1Default("m2-single-trash"),
                    Array.Empty<CombatLosLayer>(),
                    Array.Empty<CombatLosLayer>(),
                    AuthoredColliderIndex: 0),
                Array.Empty<CombatPullCandidate>(),
                zoneGate,
                clock.Snapshot());

            Assert.That(pull.Succeeded, Is.True, string.Join(Environment.NewLine, pull.Errors));
            PullStarts++;
            PullDidNotAutoEnableAttack = PullDidNotAutoEnableAttack || !pull.PlayerAttackEnabled;
            Player = Player.WithTarget(hostile.CombatActorId).WithCombatState(CombatState.InCombat);
            hostile = pull.PrimaryHostile!.WithCombatState(CombatState.InCombat);
            hostileAttack = new CombatAttackStateSnapshot(CombatAttackMode.On, Player.CombatActorId, NextWeaponTick(hostile), CombatAttackTransitionPath.PlayerToggleOn);

            var attack = new CombatAttackStateMachine().ToggleOn(new CombatAttackToggleOnRequest(
                Player,
                hostile,
                zoneGate,
                DistanceMetersToTarget: 1.5d,
                clock.Snapshot(),
                package.CombatTickRateHz));
            Assert.That(attack.Succeeded, Is.True, string.Join(Environment.NewLine, attack.RejectionReasons));
            playerAttack = attack.Snapshot;
            AttackTransitions++;

            ResolveSmite();
            ResolveUntilHostileDefeated();
            if (PullStarts == 1)
            {
                ManaAfterFirstPull = Player.CurrentMana;
            }
        }

        public void SitAndMed()
        {
            var sit = new CombatPostureStateMachine().TrySit(new CombatSitRequest(
                Player,
                new CombatAttackStateMachine(),
                clock.Snapshot(),
                IsGrounded: true,
                IsMoving: false,
                IsZoneLoadingCommitLocked: false));
            Assert.That(sit.Succeeded, Is.True, string.Join(Environment.NewLine, sit.RejectionReasons));
            Player = sit.Player.WithCombatState(CombatState.OutOfCombat);
            SitMedStarts++;

            for (var tick = 0; tick < 500 && ManaRestored == 0; tick++)
            {
                var result = regen.ResolveTick(new CombatRegenTickRequest(
                    Player,
                    package.RegenAndCombatExitTuning,
                    clock.AdvanceTicks(1),
                    package.CombatTickRateHz));
                Player = result.Actor;
                ManaRestored += result.ManaRestored;
            }

            ManaAfterMedBreak = Player.CurrentMana;
        }

        public void RespawnTrash()
        {
            var hydration = Hydrate();
            hostile = hydration.HostileActors.Single();
            playerAttack = new CombatAttackStateSnapshot(CombatAttackMode.Off, null, null, CombatAttackTransitionPath.TargetDeath);
            hostileAttack = new CombatAttackStateSnapshot(CombatAttackMode.Off, null, null, null);
            Player = Player.WithCombatState(CombatState.OutOfCombat).WithTarget(null) with
            {
                PostureState = CombatPostureState.Standing
            };
        }

        private void ResolveSmite()
        {
            var result = instant.Resolve(new CombatInstantAbilityRequest(
                $"smite-{clock.CurrentTick}",
                Player,
                hostile,
                zoneGate,
                DistanceMetersToTarget: 1.5d,
                Array.Empty<CombatLosLayer>(),
                clock.Snapshot(),
                package.CombatTickRateHz,
                smiteProfile));

            Assert.That(result.Outcome, Is.EqualTo(CombatInstantAbilityOutcome.Resolved));
            Player = result.Caster;
            hostile = result.TargetAfterResolution ?? hostile;
        }

        private void ResolveUntilHostileDefeated()
        {
            for (var tickIndex = 0; tickIndex < 5000; tickIndex++)
            {
                var tick = clock.AdvanceTicks(1);
                if (tickIndex == 400)
                {
                    ResolveSmite();
                }

                var playerResult = melee.ResolveTick(Request(Player, hostile, playerAttack, tick, new LoopingRandomSource(0.12d, 1.0d)));
                if (playerResult.Outcome != CombatMeleeTickOutcome.NotDue)
                {
                    playerAttack = playerAttack with { NextSwingDueTick = playerResult.NextSwingDueTick };
                    hostile = playerResult.TargetAfterResolution ?? hostile;
                }

                if (!hostile.IsAlive)
                {
                    HostileDefeats++;
                    CombatExits++;
                    Player = Player.ClearTargetAndThreat().WithCombatState(CombatState.OutOfCombat);
                    playerAttack = new CombatAttackStateSnapshot(CombatAttackMode.Off, null, null, CombatAttackTransitionPath.TargetDeath);
                    return;
                }

                var hostileResult = melee.ResolveTick(Request(hostile, Player, hostileAttack, tick, new LoopingRandomSource(0.38d, 0.82d)));
                if (hostileResult.Outcome != CombatMeleeTickOutcome.NotDue)
                {
                    hostileAttack = hostileAttack with { NextSwingDueTick = hostileResult.NextSwingDueTick };
                    Player = hostileResult.TargetAfterResolution ?? Player;
                    Assert.That(Player.IsAlive, Is.True, "Player died during the single-trash loop.");
                }
            }

            Assert.Fail("Hostile was not defeated within the fixed tick budget.");
        }

        private CombatMeleeTickRequest Request(
            CombatActorState attacker,
            CombatActorState target,
            CombatAttackStateSnapshot attackState,
            CombatTick tick,
            ICombatMeleeRandomSource random)
        {
            return new CombatMeleeTickRequest(
                attacker,
                target,
                attackState,
                zoneGate,
                DistanceMetersToTarget: 1.5d,
                FacingDegreesToTarget: 0.0d,
                FacingToleranceDegrees: 90.0d,
                Array.Empty<CombatLosLayer>(),
                tick,
                package.CombatTickRateHz,
                new CombatMeleeHitChanceTuning(0.72d, 0.03d, 0.001d, 0.10d, 0.92d),
                new CombatMeleeDamageTuning(0.20d, 0.10d),
                random);
        }

        private long NextWeaponTick(CombatActorState actor)
        {
            return checked(clock.Snapshot().Index + (long)Math.Ceiling(actor.WeaponDelaySeconds * package.CombatTickRateHz));
        }

        private CombatRuntimeEncounterHydrationResult Hydrate()
        {
            return hydrator.HydrateFromFile(
                FixturePath(),
                new CombatRuntimeEncounterHydrationRequest
                {
                    EncounterFixtureId = "SoloTrash_EvenCon_T1",
                    ActiveZoneId = "Haunt_Prototype_T1",
                    PlayerCombatActorId = "m2-player-cleric",
                    PlayerLocalCharacterId = "local-character-m2-dev",
                    HostileCombatActorIdPrefix = "m2-hostile"
                });
        }
    }

    private sealed class LoopingRandomSource : ICombatMeleeRandomSource
    {
        private readonly double hitRoll;
        private readonly double damageRollScalar;

        public LoopingRandomSource(double hitRoll, double damageRollScalar)
        {
            this.hitRoll = hitRoll;
            this.damageRollScalar = damageRollScalar;
        }

        public double NextHitRoll()
        {
            return hitRoll;
        }

        public double NextDamageRollScalar()
        {
            return damageRollScalar;
        }
    }

    private static CombatFixturePackage LoadPackage()
    {
        return new CombatFixtureLoader().LoadFromFile(FixturePath());
    }

    private static string FixturePath()
    {
        return Path.Combine(FindRepoRoot(), "data", "combat", "t1-combat-fixtures.json");
    }

    private static string FindRepoRoot()
    {
        var candidates = new[]
        {
            new DirectoryInfo(TestContext.CurrentContext.TestDirectory),
            new DirectoryInfo(Directory.GetCurrentDirectory())
        };

        foreach (var candidate in candidates)
        {
            for (var directory = candidate; directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")) &&
                    File.Exists(Path.Combine(directory.FullName, "data", "combat", "t1-combat-fixtures.json")))
                {
                    return directory.FullName;
                }
            }
        }

        throw new DirectoryNotFoundException("Unable to locate repository root for S2-M2-02 loop tests.");
    }
}
