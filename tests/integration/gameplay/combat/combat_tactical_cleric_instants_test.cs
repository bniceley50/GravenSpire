#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Combat.Fixtures;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Gameplay.Combat;

public sealed class CombatTacticalClericInstantsTest
{
    [Test]
    public void test_instant_direct_damage_resolves_same_tick_without_cast_bar_and_spends_mana()
    {
        var resolver = new CombatInstantAbilityResolver();
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var player = CreatePlayer().WithTarget(hostile.CombatActorId);
        var profile = Profile("SmiteOfAuthority_T1_Prototype");

        var result = resolver.Resolve(Request("ability-activation-1", player, hostile, profile, new CombatTick(10, 0.2d)));

        Assert.That(result.Outcome, Is.EqualTo(CombatInstantAbilityOutcome.Resolved));
        Assert.That(profile.ResourceKind, Is.EqualTo(CombatTacticalAbilityResourceKind.Magical));
        Assert.That(result.Caster.CurrentMana, Is.EqualTo(170));
        Assert.That(result.Caster.CurrentEndurance, Is.EqualTo(80));
        Assert.That(result.Caster.CombatState, Is.EqualTo(CombatState.OutOfCombat));
        Assert.That(result.Caster.CastRuntimeState, Is.EqualTo(CombatCastRuntimeState.None));
        Assert.That(result.TargetAfterResolution!.CurrentHealth, Is.EqualTo(104));
        Assert.That(result.CooldownEndsTick, Is.EqualTo(360));
        AssertPayload(result.AbilityEvents.OfType<AbilityActivatedEvent>().Single(), "ability-activation-1", "SmiteOfAuthority_T1_Prototype", player.CombatActorId, hostile.CombatActorId, 10);
        Assert.That(result.AbilityEvents.OfType<AbilityResolvedEvent>().Single().ManaSpent, Is.EqualTo(10));
        Assert.That(result.AbilityEvents.OfType<AbilityResolvedEvent>().Single().AppliedEffects.Single().Damage, Is.EqualTo(16));
        Assert.That(result.CastEvents, Is.Empty);
    }

    [Test]
    public void test_qa_02_01_bash_spends_endurance_and_leaves_mana_unchanged()
    {
        var resolver = new CombatInstantAbilityResolver();
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var player = CreatePlayer(currentEndurance: 80).WithTarget(hostile.CombatActorId);
        var bash = Profile("Bash_T1_Prototype");

        var result = resolver.Resolve(Request("ability-activation-1", player, hostile, bash, new CombatTick(20, 0.4d), distanceMetersToTarget: 1.5d));

        Assert.That(bash.ResourceKind, Is.EqualTo(CombatTacticalAbilityResourceKind.Physical));
        Assert.That(result.Outcome, Is.EqualTo(CombatInstantAbilityOutcome.Resolved));
        Assert.That(result.Caster.CurrentMana, Is.EqualTo(player.CurrentMana));
        Assert.That(result.Caster.CurrentEndurance, Is.EqualTo(70));
        Assert.That(result.TargetAfterResolution!.CurrentHealth, Is.EqualTo(109));
        Assert.That(result.CooldownEndsTick, Is.EqualTo(520));
        Assert.That(result.AbilityEvents.OfType<AbilityResolvedEvent>().Single().ManaSpent, Is.EqualTo(0));
    }

    [Test]
    public void test_qa_02_02_bash_rejects_insufficient_endurance_without_spend_or_cooldown()
    {
        var resolver = new CombatInstantAbilityResolver();
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var player = CreatePlayer(currentEndurance: 9).WithTarget(hostile.CombatActorId);
        var bash = Profile("Bash_T1_Prototype");

        var result = resolver.Resolve(Request("ability-activation-1", player, hostile, bash, new CombatTick(20, 0.4d), distanceMetersToTarget: 1.5d));

        Assert.That(result.Outcome, Is.EqualTo(CombatInstantAbilityOutcome.Rejected));
        Assert.That(result.Caster.CurrentMana, Is.EqualTo(player.CurrentMana));
        Assert.That(result.Caster.CurrentEndurance, Is.EqualTo(9));
        Assert.That(result.TargetAfterResolution, Is.SameAs(hostile));
        Assert.That(result.CooldownEndsTick, Is.Null);
        Assert.That(result.RejectionReasons, Has.Some.Contains("Caster does not have enough Endurance."));
        Assert.That(result.AbilityEvents.OfType<AbilityRejectedEvent>().Single().RejectionReasons, Has.Some.Contains("Caster does not have enough Endurance."));
    }

    [Test]
    public void test_qa_02_03_smite_remains_mana_based_and_ignores_endurance()
    {
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var smite = Profile("SmiteOfAuthority_T1_Prototype");
        var zeroEndurancePlayer = CreatePlayer(currentEndurance: 0).WithTarget(hostile.CombatActorId);

        var resolved = new CombatInstantAbilityResolver()
            .Resolve(Request("ability-activation-1", zeroEndurancePlayer, hostile, smite, new CombatTick(10, 0.2d)));

        Assert.That(smite.ResourceKind, Is.EqualTo(CombatTacticalAbilityResourceKind.Magical));
        Assert.That(resolved.Outcome, Is.EqualTo(CombatInstantAbilityOutcome.Resolved));
        Assert.That(resolved.Caster.CurrentMana, Is.EqualTo(170));
        Assert.That(resolved.Caster.CurrentEndurance, Is.EqualTo(0));
        Assert.That(resolved.TargetAfterResolution!.CurrentHealth, Is.EqualTo(104));
        Assert.That(resolved.CooldownEndsTick, Is.EqualTo(360));
        Assert.That(resolved.CastEvents, Is.Empty);
        Assert.That(resolved.AbilityEvents.OfType<AbilityResolvedEvent>().Single().ManaSpent, Is.EqualTo(10));

        var lowManaPlayer = CreatePlayer(currentMana: 9, currentEndurance: 80).WithTarget(hostile.CombatActorId);
        var rejected = new CombatInstantAbilityResolver()
            .Resolve(Request("ability-activation-2", lowManaPlayer, hostile, smite, new CombatTick(11, 0.22d)));

        Assert.That(rejected.Outcome, Is.EqualTo(CombatInstantAbilityOutcome.Rejected));
        Assert.That(rejected.Caster.CurrentMana, Is.EqualTo(9));
        Assert.That(rejected.Caster.CurrentEndurance, Is.EqualTo(80));
        Assert.That(rejected.TargetAfterResolution, Is.SameAs(hostile));
        Assert.That(rejected.CooldownEndsTick, Is.Null);
        Assert.That(rejected.RejectionReasons, Has.Some.Contains("Caster does not have enough mana."));
        Assert.That(rejected.AbilityEvents.OfType<AbilityRejectedEvent>().Single().RejectionReasons, Has.Some.Contains("Caster does not have enough mana."));
    }

    [Test]
    public void test_qa_02_04_defensive_prayer_remains_mana_based_and_ignores_endurance()
    {
        var prayer = Profile("DefensivePrayer_T1_Prototype");
        var zeroEndurancePlayer = CreatePlayer(currentEndurance: 0);

        var resolved = new CombatInstantAbilityResolver()
            .Resolve(Request("ability-activation-1", zeroEndurancePlayer, target: null, prayer, new CombatTick(40, 0.8d)));

        Assert.That(prayer.ResourceKind, Is.EqualTo(CombatTacticalAbilityResourceKind.Magical));
        Assert.That(resolved.Outcome, Is.EqualTo(CombatInstantAbilityOutcome.Resolved));
        Assert.That(resolved.Caster.CurrentMana, Is.EqualTo(155));
        Assert.That(resolved.Caster.CurrentEndurance, Is.EqualTo(0));
        Assert.That(resolved.TargetAfterResolution, Is.Null);
        Assert.That(resolved.AppliedEffects.Single().EffectType, Is.EqualTo(CombatTacticalAbilityEffectType.SelfBuff));
        Assert.That(resolved.CooldownEndsTick, Is.EqualTo(1540));
        Assert.That(resolved.CastEvents, Is.Empty);
        Assert.That(resolved.AbilityEvents.OfType<AbilityResolvedEvent>().Single().ManaSpent, Is.EqualTo(25));

        var lowManaPlayer = CreatePlayer(currentMana: 24, currentEndurance: 80);
        var rejected = new CombatInstantAbilityResolver()
            .Resolve(Request("ability-activation-2", lowManaPlayer, target: null, prayer, new CombatTick(41, 0.82d)));

        Assert.That(rejected.Outcome, Is.EqualTo(CombatInstantAbilityOutcome.Rejected));
        Assert.That(rejected.Caster.CurrentMana, Is.EqualTo(24));
        Assert.That(rejected.Caster.CurrentEndurance, Is.EqualTo(80));
        Assert.That(rejected.TargetAfterResolution, Is.Null);
        Assert.That(rejected.AppliedEffects, Is.Empty);
        Assert.That(rejected.CooldownEndsTick, Is.Null);
        Assert.That(rejected.RejectionReasons, Has.Some.Contains("Caster does not have enough mana."));
        Assert.That(rejected.AbilityEvents.OfType<AbilityRejectedEvent>().Single().RejectionReasons, Has.Some.Contains("Caster does not have enough mana."));
    }

    [Test]
    public void test_instant_cooldown_is_transient_runtime_timer_only()
    {
        var resolver = new CombatInstantAbilityResolver();
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var player = CreatePlayer().WithTarget(hostile.CombatActorId);
        var profile = Profile("SmiteOfAuthority_T1_Prototype");

        var first = resolver.Resolve(Request("ability-activation-1", player, hostile, profile, new CombatTick(0, 0d)));
        var second = resolver.Resolve(Request("ability-activation-2", first.Caster, first.TargetAfterResolution, profile, new CombatTick(1, 0.02d)));
        var freshResolverResult = new CombatInstantAbilityResolver()
            .Resolve(Request("ability-activation-3", first.Caster, first.TargetAfterResolution, profile, new CombatTick(1, 0.02d)));

        Assert.That(first.Outcome, Is.EqualTo(CombatInstantAbilityOutcome.Resolved));
        Assert.That(second.Outcome, Is.EqualTo(CombatInstantAbilityOutcome.OnCooldown));
        Assert.That(second.CooldownEndsTick, Is.EqualTo(350));
        Assert.That(second.CooldownRemainingTicks, Is.EqualTo(349));
        Assert.That(freshResolverResult.Outcome, Is.EqualTo(CombatInstantAbilityOutcome.Resolved));
    }

    [Test]
    public void test_bash_cancels_current_channel_only_through_declared_interrupt_effect()
    {
        var resolver = new CombatInstantAbilityResolver();
        var channelingHostile = CreateHostile("combat-hostile-1", "hostile-001")
            .BeginCast("hostile-cast-1", "Hostile_Channel_T1_Test", "combat-player-1");
        var player = CreatePlayer().WithTarget(channelingHostile.CombatActorId);
        var bash = Profile("Bash_T1_Prototype");

        var result = resolver.Resolve(Request("ability-activation-1", player, channelingHostile, bash, new CombatTick(25, 0.5d), distanceMetersToTarget: 1.5d));

        Assert.That(result.Outcome, Is.EqualTo(CombatInstantAbilityOutcome.Resolved));
        Assert.That(result.Caster.CurrentMana, Is.EqualTo(player.CurrentMana));
        Assert.That(result.Caster.CurrentEndurance, Is.EqualTo(70));
        Assert.That(result.TargetAfterResolution!.CastRuntimeState, Is.EqualTo(CombatCastRuntimeState.Recovery));
        Assert.That(result.TargetAfterResolution.CastRecoveryRemainingSeconds, Is.EqualTo(1.0d).Within(0.000001d));
        Assert.That(result.CastEvents.OfType<CastCancelledEvent>().Single().CancelSource, Is.EqualTo("Bash_T1_Prototype"));
        Assert.That(result.CastEvents.OfType<CastRecoveryStartedEvent>().Single().RecoverySeconds, Is.EqualTo(1.0d).Within(0.000001d));

        var smiteOnly = new CombatInstantAbilityResolver().Resolve(Request(
            "ability-activation-2",
            player,
            channelingHostile,
            Profile("SmiteOfAuthority_T1_Prototype"),
            new CombatTick(25, 0.5d)));
        Assert.That(smiteOnly.TargetAfterResolution!.CastRuntimeState, Is.EqualTo(CombatCastRuntimeState.Casting));
        Assert.That(smiteOnly.CastEvents, Is.Empty);
    }

    [Test]
    public void test_self_buff_duration_comes_from_fixture_profile_data()
    {
        var resolver = new CombatInstantAbilityResolver();
        var player = CreatePlayer();
        var prayer = Profile("DefensivePrayer_T1_Prototype");

        var result = resolver.Resolve(Request("ability-activation-1", player, target: null, prayer, new CombatTick(40, 0.8d)));

        Assert.That(result.Outcome, Is.EqualTo(CombatInstantAbilityOutcome.Resolved));
        Assert.That(prayer.ResourceKind, Is.EqualTo(CombatTacticalAbilityResourceKind.Magical));
        Assert.That(result.Caster.CurrentMana, Is.EqualTo(155));
        Assert.That(result.Caster.CurrentEndurance, Is.EqualTo(80));
        Assert.That(result.Caster.CastRuntimeState, Is.EqualTo(CombatCastRuntimeState.None));
        Assert.That(result.TargetAfterResolution, Is.Null);
        var selfBuff = result.AppliedEffects.Single();
        Assert.That(selfBuff.EffectType, Is.EqualTo(CombatTacticalAbilityEffectType.SelfBuff));
        Assert.That(selfBuff.DurationSeconds, Is.EqualTo(8.0d).Within(0.000001d));
        Assert.That(selfBuff.DamageReduction, Is.EqualTo(0.2d).Within(0.000001d));
        Assert.That(result.CooldownEndsTick, Is.EqualTo(1540));
    }

    private static CombatInstantAbilityRequest Request(
        string activationId,
        CombatActorState player,
        CombatActorState? target,
        CombatTacticalAbilityProfile profile,
        CombatTick tick,
        double distanceMetersToTarget = 20.0d)
    {
        return new CombatInstantAbilityRequest(
            activationId,
            player,
            target,
            ActiveHauntGate(),
            distanceMetersToTarget,
            Array.Empty<CombatLosLayer>(),
            tick,
            TickRateHz: 50,
            profile);
    }

    private static CombatTacticalAbilityProfile Profile(string abilityId)
    {
        var fixture = LoadPackage().TacticalInstantAbilityProfiles.Single(profile => profile.Id == abilityId);
        return CombatTacticalAbilityProfile.FromFixture(fixture, "Mid");
    }

    private static CombatFixturePackage LoadPackage()
    {
        var path = Path.Combine(FindRepoRoot(), "assets", "data", "combat", "t1-combat-fixtures.json");
        return new CombatFixtureLoader().LoadFromFile(path);
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
                    Directory.Exists(Path.Combine(directory.FullName, "assets")))
                {
                    return directory.FullName;
                }
            }
        }

        throw new DirectoryNotFoundException("Unable to locate repository root for tactical instant integration tests.");
    }

    private static CombatZoneGate ActiveHauntGate()
    {
        var gate = new CombatZoneGate();
        gate.ActivateZone("Haunt_Prototype_T1", CombatZoneType.HauntZone);
        return gate;
    }

    private static CombatActorState CreatePlayer(
        string zoneId = "Haunt_Prototype_T1",
        int maxMana = 180,
        int currentMana = 180,
        int maxEndurance = 80,
        int currentEndurance = 80)
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
            maxMana,
            currentMana,
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
            maxEndurance: maxEndurance,
            currentEndurance: currentEndurance);
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
        ICombatAbilityLifecycleEvent lifecycleEvent,
        string activationId,
        string abilityId,
        string casterCombatActorId,
        string? targetCombatActorId,
        long tick)
    {
        Assert.That(lifecycleEvent.ActivationId, Is.EqualTo(activationId));
        Assert.That(lifecycleEvent.AbilityId, Is.EqualTo(abilityId));
        Assert.That(lifecycleEvent.CasterCombatActorId, Is.EqualTo(casterCombatActorId));
        Assert.That(lifecycleEvent.TargetCombatActorId, Is.EqualTo(targetCombatActorId));
        Assert.That(lifecycleEvent.Tick.Index, Is.EqualTo(tick));
    }
}
