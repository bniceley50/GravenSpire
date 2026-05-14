#nullable enable

using System;
using System.IO;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Combat.Fixtures;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Gameplay.Combat;

public sealed class CombatRuntimeNamedBlockerBoundaryTest
{
    [Test]
    public void test_named_blocker_is_present_targetable_and_distinct_from_solo_trash_fixture()
    {
        var package = LoadPackage();
        var boundary = new NamedBlockerBoundaryHarness(package);

        boundary.StartNamedPull();

        Assert.That(boundary.NamedTargeted, Is.True);
        Assert.That(boundary.PlayerAttackAutoEnabled, Is.False);
        Assert.That(
            NamedBlockerBoundaryHarness.NamedEncounterFixtureId,
            Is.Not.EqualTo(NamedBlockerBoundaryHarness.SoloTrashEncounterFixtureId));
        Assert.That(
            boundary.NamedMaxHealth,
            Is.GreaterThan(boundary.BaselineTrashMaxHealth),
            "Named blocker must not be tuned as normal solo-trash farm content.");
    }

    [Test]
    public void test_named_blocker_solo_attempt_forces_loss_or_flee_threshold_consistent_with_feel02()
    {
        var package = LoadPackage();
        var boundary = new NamedBlockerBoundaryHarness(package);

        boundary.StartNamedPull();
        boundary.ResolveUntilBoundary();

        Assert.That(boundary.BoundaryOutcomeRecorded, Is.True);
        Assert.That(boundary.Outcome, Is.AnyOf("player_lost", "forced_flee_threshold"));
        Assert.That(boundary.TimeToDangerSeconds, Is.GreaterThan(0d));
        Assert.That(
            boundary.Player.CurrentHealth < boundary.Player.MaxHealth * 0.20d ||
            boundary.Player.CurrentMana < boundary.Player.MaxMana * 0.10d ||
            !boundary.Player.IsAlive,
            Is.True);
    }

    [Test]
    public void test_named_blocker_solo_attempt_leaves_named_alive_proving_no_farm_through()
    {
        var package = LoadPackage();
        var boundary = new NamedBlockerBoundaryHarness(package);

        boundary.StartNamedPull();
        boundary.ResolveUntilBoundary();

        Assert.That(boundary.Outcome, Is.Not.EqualTo("named_solo_killed"));
        Assert.That(boundary.Outcome, Is.Not.EqualTo("unresolved_tick_budget"));
        Assert.That(boundary.Named.IsAlive, Is.True);
        Assert.That(boundary.Named.CurrentHealth, Is.GreaterThan(0));
    }

    private sealed class NamedBlockerBoundaryHarness
    {
        public const string NamedEncounterFixtureId = "NamedSoloBlock_T1";
        public const string SoloTrashEncounterFixtureId = "SoloTrash_EvenCon_T1";

        private readonly CombatFixturePackage package;
        private readonly CombatMeleeResolver melee = new();
        private readonly CombatInstantAbilityResolver instant = new();
        private readonly CombatZoneGate zoneGate = new();
        private readonly FixedCombatClock clock;
        private readonly CombatTacticalAbilityProfile smiteProfile;
        private CombatActorState named;
        private CombatAttackStateSnapshot playerAttack = new(CombatAttackMode.Off, null, null, null);
        private CombatAttackStateSnapshot namedAttack = new(CombatAttackMode.Off, null, null, null);
        private long? smiteCooldownEndsTick;

        public NamedBlockerBoundaryHarness(CombatFixturePackage package)
        {
            this.package = package;
            clock = new FixedCombatClock(package.CombatTickRateHz);
            zoneGate.ActivateZone("Haunt_Prototype_T1", CombatZoneType.HauntZone);
            smiteProfile = CombatTacticalAbilityProfile.FromFixture(
                package.TacticalInstantAbilityProfiles.Single(profile => profile.Id == "SmiteOfAuthority_T1_Prototype"),
                "Mid");

            var baselineTrashHydration = new CombatRuntimeEncounterHydrator().HydrateFromFile(
                FixturePath(),
                new CombatRuntimeEncounterHydrationRequest
                {
                    EncounterFixtureId = SoloTrashEncounterFixtureId,
                    ActiveZoneId = "Haunt_Prototype_T1",
                    PlayerCombatActorId = "m2-player-cleric",
                    PlayerLocalCharacterId = "local-character-m2-dev",
                    HostileCombatActorIdPrefix = "m2-baseline-trash"
                });
            Assert.That(
                baselineTrashHydration.Succeeded,
                Is.True,
                string.Join(Environment.NewLine, baselineTrashHydration.Errors));
            Assert.That(baselineTrashHydration.HostileActors.Count, Is.GreaterThan(0));
            BaselineTrashMaxHealth = baselineTrashHydration.HostileActors[0].MaxHealth;

            var hydration = new CombatRuntimeEncounterHydrator().HydrateFromFile(
                FixturePath(),
                new CombatRuntimeEncounterHydrationRequest
                {
                    EncounterFixtureId = NamedEncounterFixtureId,
                    ActiveZoneId = "Haunt_Prototype_T1",
                    PlayerCombatActorId = "m2-player-cleric",
                    PlayerLocalCharacterId = "local-character-m2-dev",
                    HostileCombatActorIdPrefix = "m2-named-hostile"
                });
            Assert.That(hydration.Succeeded, Is.True, string.Join(Environment.NewLine, hydration.Errors));
            Player = hydration.PlayerActor ?? throw new InvalidOperationException("Player actor did not hydrate.");
            Assert.That(hydration.HostileActors.Count, Is.EqualTo(1));
            named = hydration.HostileActors[0];
            NamedMaxHealth = named.MaxHealth;
        }

        public CombatActorState Player { get; private set; }

        public CombatActorState Named => named;

        public int BaselineTrashMaxHealth { get; }

        public int NamedMaxHealth { get; }

        public bool NamedTargeted { get; private set; }

        public bool PlayerAttackAutoEnabled { get; private set; }

        public bool BoundaryOutcomeRecorded { get; private set; }

        public string Outcome { get; private set; } = "not_run";

        public double TimeToDangerSeconds { get; private set; } = -1d;

        public void StartNamedPull()
        {
            var pull = new CombatPullCoordinator().ResolveBodyPull(
                Player,
                new CombatPoint3(-2.8d, 0d, 4.1d),
                Candidate(named, new CombatPoint3(-2.8d, 0d, 5.6d), "M2_NamedBlocker"),
                Array.Empty<CombatPullCandidate>(),
                zoneGate,
                clock.Snapshot());

            Assert.That(pull.Succeeded, Is.True, string.Join(Environment.NewLine, pull.Errors));
            Assert.That(pull.PrimaryHostile, Is.Not.Null);
            Assert.That(pull.AssistingHostiles.Count, Is.EqualTo(0));

            PlayerAttackAutoEnabled = pull.PlayerAttackEnabled;
            named = pull.PrimaryHostile!.WithCombatState(CombatState.InCombat);
            Player = Player.WithTarget(named.CombatActorId).WithCombatState(CombatState.InCombat);
            NamedTargeted = true;
            namedAttack = new CombatAttackStateSnapshot(
                CombatAttackMode.On,
                Player.CombatActorId,
                NextWeaponTick(named),
                CombatAttackTransitionPath.PlayerToggleOn);

            var attack = new CombatAttackStateMachine().ToggleOn(new CombatAttackToggleOnRequest(
                Player,
                named,
                zoneGate,
                DistanceMetersToTarget: 1.5d,
                clock.Snapshot(),
                package.CombatTickRateHz));
            Assert.That(attack.Succeeded, Is.True, string.Join(Environment.NewLine, attack.RejectionReasons));
            playerAttack = attack.Snapshot;

            Assert.That(
                ResolveSmite(),
                Is.True,
                "Initial named-blocker Smite did not resolve at full Cleric mana.");
        }

        public void ResolveUntilBoundary()
        {
            for (var tickIndex = 0; tickIndex < 5000; tickIndex++)
            {
                var tick = clock.AdvanceTicks(1);

                if (smiteCooldownEndsTick is not null &&
                    tick.Index >= smiteCooldownEndsTick.Value &&
                    named.IsAlive)
                {
                    ResolveSmite();
                }

                ResolvePlayerMelee(tick);
                ResolveNamedMelee(tick);

                if (!Player.IsAlive)
                {
                    BoundaryOutcomeRecorded = true;
                    Outcome = "player_lost";
                    TimeToDangerSeconds = tick.Index / (double)package.CombatTickRateHz;
                    return;
                }

                if (named.IsAlive &&
                    (Player.CurrentHealth < Player.MaxHealth * 0.20d ||
                     Player.CurrentMana < Player.MaxMana * 0.10d))
                {
                    BoundaryOutcomeRecorded = true;
                    Outcome = "forced_flee_threshold";
                    TimeToDangerSeconds = tick.Index / (double)package.CombatTickRateHz;
                    return;
                }

                if (!named.IsAlive)
                {
                    Outcome = "named_solo_killed";
                    TimeToDangerSeconds = tick.Index / (double)package.CombatTickRateHz;
                    return;
                }
            }

            Outcome = "unresolved_tick_budget";
        }

        private CombatPullCandidate Candidate(CombatActorState actor, CombatPoint3 position, string anchorId)
        {
            return new CombatPullCandidate(
                actor,
                position,
                new CombatSpatialAnchorSet(position, new CombatPoint3(-2.8d, 0d, 4.1d), anchorId, "ClericShellMarker"),
                AggroRadiusMeters: 4d,
                CombatSocialAssistProfile.T1Default("m2-named-blocker"),
                Array.Empty<CombatLosLayer>(),
                Array.Empty<CombatLosLayer>(),
                AuthoredColliderIndex: 0);
        }

        private bool ResolveSmite()
        {
            var result = instant.Resolve(new CombatInstantAbilityRequest(
                $"named-blocker-smite-{clock.CurrentTick}",
                Player,
                named,
                zoneGate,
                DistanceMetersToTarget: 1.5d,
                Array.Empty<CombatLosLayer>(),
                clock.Snapshot(),
                package.CombatTickRateHz,
                smiteProfile));

            if (result.Outcome != CombatInstantAbilityOutcome.Resolved)
            {
                // Smite unavailable this attempt (for example insufficient mana under sustained
                // named pressure). The Cleric falls back to auto-attack; this is itself part of
                // the FEEL-02 block and must not abort the boundary run.
                return false;
            }

            Player = result.Caster;
            named = result.TargetAfterResolution ?? named;
            smiteCooldownEndsTick = result.CooldownEndsTick;
            return true;
        }

        private void ResolvePlayerMelee(CombatTick tick)
        {
            if (!named.IsAlive || !playerAttack.IsAttackOn)
            {
                return;
            }

            var result = melee.ResolveTick(Request(Player, named, playerAttack, tick, new LoopingRandomSource(0.12d, 1.0d)));
            if (result.Outcome == CombatMeleeTickOutcome.NotDue)
            {
                return;
            }

            playerAttack = playerAttack with { NextSwingDueTick = result.NextSwingDueTick };
            named = result.TargetAfterResolution ?? named;
        }

        private void ResolveNamedMelee(CombatTick tick)
        {
            if (!named.IsAlive || !Player.IsAlive)
            {
                return;
            }

            var result = melee.ResolveTick(Request(named, Player, namedAttack, tick, new LoopingRandomSource(0.36d, 0.86d)));
            if (result.Outcome == CombatMeleeTickOutcome.NotDue)
            {
                return;
            }

            namedAttack = namedAttack with { NextSwingDueTick = result.NextSwingDueTick };
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

        throw new DirectoryNotFoundException("Unable to locate repository root for S2-M2-04 named-blocker tests.");
    }
}
