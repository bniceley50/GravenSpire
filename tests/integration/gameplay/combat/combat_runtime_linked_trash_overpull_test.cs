#nullable enable

using System;
using System.IO;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Combat.Fixtures;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Gameplay.Combat;

public sealed class CombatRuntimeLinkedTrashOverpullTest
{
    [Test]
    public void test_linked_trash_bad_pull_claims_two_hostiles_within_feel03_window()
    {
        var package = LoadPackage();
        var overpull = new LinkedTrashOverpullHarness(package);

        overpull.StartBadPull();

        Assert.That(overpull.HostilesInHate, Is.EqualTo(2));
        Assert.That(overpull.HateWindowSeconds, Is.LessThanOrEqualTo(5d));
        Assert.That(overpull.PlayerAttackAutoEnabled, Is.False);
    }

    [Test]
    public void test_two_trash_overpull_forces_loss_or_flee_threshold_before_clean_farming_state()
    {
        var package = LoadPackage();
        var overpull = new LinkedTrashOverpullHarness(package);

        overpull.StartBadPull();
        overpull.ResolveUntilDangerous();

        Assert.That(overpull.DangerousOutcomeRecorded, Is.True);
        Assert.That(overpull.Outcome, Is.AnyOf("player_lost", "forced_flee_threshold"));
        Assert.That(
            overpull.Player.CurrentHealth < overpull.Player.MaxHealth * 0.20d ||
            overpull.Player.CurrentMana < overpull.Player.MaxMana * 0.10d ||
            !overpull.Player.IsAlive,
            Is.True);
    }

    private sealed class LinkedTrashOverpullHarness
    {
        private readonly CombatFixturePackage package;
        private readonly CombatMeleeResolver melee = new();
        private readonly CombatInstantAbilityResolver instant = new();
        private readonly CombatZoneGate zoneGate = new();
        private readonly FixedCombatClock clock;
        private readonly CombatTacticalAbilityProfile smiteProfile;
        private CombatActorState primary;
        private CombatActorState linked;
        private CombatAttackStateSnapshot playerAttack = new(CombatAttackMode.Off, null, null, null);
        private CombatAttackStateSnapshot primaryAttack = new(CombatAttackMode.Off, null, null, null);
        private CombatAttackStateSnapshot linkedAttack = new(CombatAttackMode.Off, null, null, null);
        private long? smiteCooldownEndsTick;

        public LinkedTrashOverpullHarness(CombatFixturePackage package)
        {
            this.package = package;
            clock = new FixedCombatClock(package.CombatTickRateHz);
            zoneGate.ActivateZone("Haunt_Prototype_T1", CombatZoneType.HauntZone);
            smiteProfile = CombatTacticalAbilityProfile.FromFixture(
                package.TacticalInstantAbilityProfiles.Single(profile => profile.Id == "SmiteOfAuthority_T1_Prototype"),
                "Mid");

            var hydration = new CombatRuntimeEncounterHydrator().HydrateFromFile(
                FixturePath(),
                new CombatRuntimeEncounterHydrationRequest
                {
                    EncounterFixtureId = "TwoTrash_Overpull_T1",
                    ActiveZoneId = "Haunt_Prototype_T1",
                    PlayerCombatActorId = "m2-player-cleric",
                    PlayerLocalCharacterId = "local-character-m2-dev",
                    HostileCombatActorIdPrefix = "m2-linked-hostile"
                });

            Assert.That(hydration.Succeeded, Is.True, string.Join(Environment.NewLine, hydration.Errors));
            Player = hydration.PlayerActor ?? throw new InvalidOperationException("Player actor did not hydrate.");
            Assert.That(hydration.HostileActors.Count, Is.EqualTo(2));
            primary = hydration.HostileActors[0];
            linked = hydration.HostileActors[1];
        }

        public CombatActorState Player { get; private set; }

        public int HostilesInHate { get; private set; }

        public double HateWindowSeconds { get; private set; } = -1d;

        public bool PlayerAttackAutoEnabled { get; private set; }

        public bool DangerousOutcomeRecorded { get; private set; }

        public string Outcome { get; private set; } = "not_run";

        public void StartBadPull()
        {
            var pull = new CombatPullCoordinator().ResolveBodyPull(
                Player,
                new CombatPoint3(0d, 0d, 2.5d),
                Candidate(primary, new CombatPoint3(0d, 0d, 4d), "M2_BaselineTrash", 0),
                new[] { Candidate(linked, new CombatPoint3(2.3d, 0d, 4.8d), "M2_LinkedTrash", 1) },
                zoneGate,
                clock.Snapshot());

            Assert.That(pull.Succeeded, Is.True, string.Join(Environment.NewLine, pull.Errors));
            Assert.That(pull.PrimaryHostile, Is.Not.Null);
            Assert.That(pull.AssistingHostiles.Count, Is.EqualTo(1));

            PlayerAttackAutoEnabled = pull.PlayerAttackEnabled;
            HostilesInHate = 1 + pull.AssistingHostiles.Count;
            HateWindowSeconds = 0d;
            primary = pull.PrimaryHostile!.WithCombatState(CombatState.InCombat);
            linked = pull.AssistingHostiles[0].WithCombatState(CombatState.InCombat);
            Player = Player.WithTarget(primary.CombatActorId).WithCombatState(CombatState.InCombat);
            primaryAttack = new CombatAttackStateSnapshot(CombatAttackMode.On, Player.CombatActorId, NextWeaponTick(primary), CombatAttackTransitionPath.PlayerToggleOn);
            linkedAttack = new CombatAttackStateSnapshot(CombatAttackMode.On, Player.CombatActorId, NextWeaponTick(linked), CombatAttackTransitionPath.PlayerToggleOn);

            var attack = new CombatAttackStateMachine().ToggleOn(new CombatAttackToggleOnRequest(
                Player,
                primary,
                zoneGate,
                DistanceMetersToTarget: 1.5d,
                clock.Snapshot(),
                package.CombatTickRateHz));
            Assert.That(attack.Succeeded, Is.True, string.Join(Environment.NewLine, attack.RejectionReasons));
            playerAttack = attack.Snapshot;
            ResolveSmite(ref primary);
        }

        public void ResolveUntilDangerous()
        {
            var targetingPrimary = true;
            for (var tickIndex = 0; tickIndex < 5000; tickIndex++)
            {
                var tick = clock.AdvanceTicks(1);
                if (targetingPrimary && !primary.IsAlive && linked.IsAlive)
                {
                    targetingPrimary = false;
                    Player = Player.WithTarget(linked.CombatActorId);
                    playerAttack = new CombatAttackStateSnapshot(
                        CombatAttackMode.On,
                        linked.CombatActorId,
                        NextWeaponTick(Player),
                        CombatAttackTransitionPath.PlayerToggleOn);
                }

                if (smiteCooldownEndsTick is not null &&
                    tick.Index >= smiteCooldownEndsTick.Value &&
                    (targetingPrimary ? primary.IsAlive : linked.IsAlive))
                {
                    if (targetingPrimary)
                    {
                        ResolveSmite(ref primary);
                    }
                    else
                    {
                        ResolveSmite(ref linked);
                    }
                }

                ResolvePlayerMelee(tick, ref primary, targetingPrimary);
                ResolvePlayerMelee(tick, ref linked, !targetingPrimary);
                ResolveHostileMelee(tick, ref primary, ref primaryAttack, new LoopingRandomSource(0.38d, 0.82d));
                ResolveHostileMelee(tick, ref linked, ref linkedAttack, new LoopingRandomSource(0.34d, 0.84d));

                if (!Player.IsAlive)
                {
                    DangerousOutcomeRecorded = true;
                    Outcome = "player_lost";
                    return;
                }

                if ((primary.IsAlive || linked.IsAlive) &&
                    (Player.CurrentHealth < Player.MaxHealth * 0.20d ||
                     Player.CurrentMana < Player.MaxMana * 0.10d))
                {
                    DangerousOutcomeRecorded = true;
                    Outcome = "forced_flee_threshold";
                    return;
                }

                if (!primary.IsAlive && !linked.IsAlive)
                {
                    Outcome = "comfortable_two_trash_win";
                    return;
                }
            }

            Outcome = "unresolved_tick_budget";
        }

        private CombatPullCandidate Candidate(CombatActorState actor, CombatPoint3 position, string anchorId, int order)
        {
            return new CombatPullCandidate(
                actor,
                position,
                new CombatSpatialAnchorSet(position, new CombatPoint3(0d, 0d, 2.5d), anchorId, "ClericShellMarker"),
                AggroRadiusMeters: 4d,
                CombatSocialAssistProfile.T1Default("m2-linked-trash", "m2-linked-trash-overpull", order),
                Array.Empty<CombatLosLayer>(),
                Array.Empty<CombatLosLayer>(),
                AuthoredColliderIndex: order);
        }

        private void ResolveSmite(ref CombatActorState target)
        {
            var result = instant.Resolve(new CombatInstantAbilityRequest(
                $"overpull-smite-{clock.CurrentTick}",
                Player,
                target,
                zoneGate,
                DistanceMetersToTarget: 1.5d,
                Array.Empty<CombatLosLayer>(),
                clock.Snapshot(),
                package.CombatTickRateHz,
                smiteProfile));

            Assert.That(result.Outcome, Is.EqualTo(CombatInstantAbilityOutcome.Resolved));
            Player = result.Caster;
            target = result.TargetAfterResolution ?? target;
            smiteCooldownEndsTick = result.CooldownEndsTick;
        }

        private void ResolvePlayerMelee(CombatTick tick, ref CombatActorState target, bool isCurrentTarget)
        {
            if (!isCurrentTarget || !target.IsAlive || !playerAttack.IsAttackOn)
            {
                return;
            }

            var result = melee.ResolveTick(Request(Player, target, playerAttack, tick, new LoopingRandomSource(0.12d, 1.0d)));
            if (result.Outcome == CombatMeleeTickOutcome.NotDue)
            {
                return;
            }

            playerAttack = playerAttack with { NextSwingDueTick = result.NextSwingDueTick };
            target = result.TargetAfterResolution ?? target;
        }

        private void ResolveHostileMelee(
            CombatTick tick,
            ref CombatActorState hostile,
            ref CombatAttackStateSnapshot attackState,
            ICombatMeleeRandomSource random)
        {
            if (!hostile.IsAlive || !Player.IsAlive)
            {
                return;
            }

            var result = melee.ResolveTick(Request(hostile, Player, attackState, tick, random));
            if (result.Outcome == CombatMeleeTickOutcome.NotDue)
            {
                return;
            }

            attackState = attackState with { NextSwingDueTick = result.NextSwingDueTick };
            Player = result.TargetAfterResolution ?? Player;
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

        throw new DirectoryNotFoundException("Unable to locate repository root for S2-M2-03 linked-trash tests.");
    }
}
