#nullable enable

using System;
using System.IO;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Combat.Fixtures;
using NUnit.Framework;

namespace Gravenspire.Tests.Unit.Gameplay.Combat;

public sealed class CombatFixtureValidationTest
{
    [Test]
    public void test_combat_fixture_package_validates_required_t1_rows()
    {
        var package = LoadPackage();

        var validation = new CombatFixtureValidator().Validate(package);

        Assert.That(validation.IsValid, Is.True, string.Join(Environment.NewLine, validation.Errors));
    }

    [Test]
    public void test_combat_fixture_package_resolves_cleric_mid_t1_design_values()
    {
        var package = LoadPackage();

        var clericMid = package.ActorFixtures.Single(actor => actor.Id == "Cleric_Mid_T1");

        Assert.That(clericMid.Level, Is.EqualTo(5));
        Assert.That(clericMid.MaxHealth, Is.EqualTo(140));
        Assert.That(clericMid.MaxMana, Is.EqualTo(180));
        Assert.That(clericMid.ArmorClass, Is.EqualTo(35));
        Assert.That(clericMid.AttackPower, Is.EqualTo(25));
        Assert.That(clericMid.WeaponDelaySeconds, Is.EqualTo(2.8d).Within(0.000001d));
    }

    [Test]
    public void test_combat_fixture_package_resolves_targeting_pull_and_leash_tuning()
    {
        var package = LoadPackage();

        Assert.That(package.TargetingTuning.TargetAcquireRadiusMeters, Is.EqualTo(35d).Within(0.000001d));
        Assert.That(package.TargetingTuning.CombatQueryBufferSize, Is.EqualTo(64));
        Assert.That(package.TargetingTuning.LosOccluderLayerMaskT1, Is.EquivalentTo(T1CombatLineOfSight.BlockingLayers));
        Assert.That(package.TargetingTuning.NonBlockingLayersT1.Any(T1CombatLineOfSight.BlocksLineOfSight), Is.False);
        Assert.That(package.PullTuning.ProximityThreatInitial, Is.EqualTo(25));
        Assert.That(package.PullTuning.SocialAssistPulseSeconds, Is.EqualTo(2.0d).Within(0.000001d));
        Assert.That(package.PullTuning.SocialAssistRadiusMeters, Is.EqualTo(12d).Within(0.000001d));
        Assert.That(package.PullTuning.AssistThreatInitial, Is.EqualTo(25));
        Assert.That(package.LeashTuning.LeashDistanceMeters, Is.EqualTo(35d).Within(0.000001d));
        Assert.That(package.LeashTuning.PathFailureGraceSeconds, Is.EqualTo(1.0d).Within(0.000001d));
        Assert.That(package.LeashTuning.PathPendingGraceSeconds, Is.EqualTo(1.0d).Within(0.000001d));
        Assert.That(package.LeashTuning.PathStatusSampleSeconds, Is.EqualTo(0.25d).Within(0.000001d));
        Assert.That(package.LeashTuning.LeashThreatMemorySeconds, Is.EqualTo(30d).Within(0.000001d));
        Assert.That(package.LeashTuning.LeashReAggroDistanceMeters, Is.EqualTo(20d).Within(0.000001d));
    }

    [Test]
    public void test_combat_fixture_package_declares_default_social_assist_profile()
    {
        var package = LoadPackage();

        var profile = package.SocialAssistProfiles.Single(item => item.Id == "VampireCourt_T1_DefaultSocial");

        Assert.That(profile.AssistEnabled, Is.True);
        Assert.That(profile.SocialLinkGroupId, Is.EqualTo("VampireCourt_Haunt_Default_T1"));
        Assert.That(profile.AssistRadiusMeters, Is.EqualTo(package.PullTuning.SocialAssistRadiusMeters).Within(0.000001d));
        Assert.That(profile.AssistThreatInitial, Is.EqualTo(package.PullTuning.AssistThreatInitial));
        Assert.That(profile.AssistRequiresLosToPrimary, Is.True);
        Assert.That(profile.AssistRequiresLosToTarget, Is.True);
        Assert.That(profile.AssistFactionFilter, Is.EqualTo("SameFactionOrExplicitAlly"));
        Assert.That(profile.AssistEncounterFilter, Is.EqualTo("SameEncounterOrSharedSocialGroup"));
    }

    [Test]
    public void test_combat_fixture_validator_rejects_missing_required_rows()
    {
        var package = LoadPackage() with
        {
            ActorFixtures = LoadPackage().ActorFixtures
                .Where(actor => actor.Id != "Cleric_Mid_T1")
                .ToList()
        };

        var validation = new CombatFixtureValidator().Validate(package);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors, Has.Some.Contains("Missing required actor fixture: Cleric_Mid_T1"));
    }

    [Test]
    public void test_combat_fixture_package_declares_tactical_instant_ability_profiles()
    {
        var package = LoadPackage();

        var smite = package.TacticalInstantAbilityProfiles.Single(profile => profile.Id == "SmiteOfAuthority_T1_Prototype");
        var bash = package.TacticalInstantAbilityProfiles.Single(profile => profile.Id == "Bash_T1_Prototype");
        var prayer = package.TacticalInstantAbilityProfiles.Single(profile => profile.Id == "DefensivePrayer_T1_Prototype");

        Assert.That(smite.CastTimeSeconds, Is.EqualTo(0d));
        Assert.That(smite.CostMana, Is.EqualTo(10));
        Assert.That(smite.CooldownSeconds, Is.EqualTo(7.0d).Within(0.000001d));
        Assert.That(smite.Effects.Single().EffectType, Is.EqualTo(CombatTacticalAbilityEffectTypes.DirectDamage));
        Assert.That(smite.Effects.Single().DamageByBand.Single(value => value.Band == "Mid").Value, Is.EqualTo(16));

        Assert.That(bash.Effects.Select(effect => effect.EffectType), Is.EqualTo(new[]
        {
            CombatTacticalAbilityEffectTypes.DirectDamage,
            CombatTacticalAbilityEffectTypes.InterruptCurrentChannel
        }));
        Assert.That(bash.Effects.Single(effect => effect.EffectType == CombatTacticalAbilityEffectTypes.InterruptCurrentChannel).InterruptSeconds, Is.EqualTo(1.0d).Within(0.000001d));

        Assert.That(prayer.RequiresTarget, Is.False);
        Assert.That(prayer.RequiresLineOfSight, Is.False);
        Assert.That(prayer.Effects.Single().EffectType, Is.EqualTo(CombatTacticalAbilityEffectTypes.SelfBuff));
        Assert.That(prayer.Effects.Single().DurationSeconds, Is.EqualTo(8.0d).Within(0.000001d));
    }

    [Test]
    public void test_combat_fixture_validator_rejects_missing_tactical_instant_ability_profile_fields()
    {
        var package = LoadPackage() with
        {
            TacticalInstantAbilityProfiles = new()
            {
                new CombatTacticalInstantAbilityProfileFixture
                {
                    Id = "SmiteOfAuthority_T1_Prototype"
                }
            }
        };

        var validation = new CombatFixtureValidator().Validate(package);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors, Has.Some.Contains("SmiteOfAuthority_T1_Prototype: cost_mana must be positive."));
        Assert.That(validation.Errors, Has.Some.Contains("SmiteOfAuthority_T1_Prototype: cooldown_seconds must be positive."));
        Assert.That(validation.Errors, Has.Some.Contains("SmiteOfAuthority_T1_Prototype: at least one declared ability effect is required."));
        Assert.That(validation.Errors, Has.Some.Contains("Missing required tactical instant ability profile: Bash_T1_Prototype"));
        Assert.That(validation.Errors, Has.Some.Contains("Missing required tactical instant ability profile: DefensivePrayer_T1_Prototype"));
    }

    [Test]
    public void test_combat_fixture_validator_rejects_missing_tactical_instant_effect_specific_data()
    {
        var package = LoadPackage() with
        {
            TacticalInstantAbilityProfiles = new()
            {
                new CombatTacticalInstantAbilityProfileFixture
                {
                    Id = "SmiteOfAuthority_T1_Prototype",
                    CastTimeSeconds = 0d,
                    CostMana = 10,
                    CooldownSeconds = 7.0d,
                    RangeMeters = 30.0d,
                    Effects = new()
                    {
                        new CombatTacticalInstantAbilityEffectFixture
                        {
                            EffectType = CombatTacticalAbilityEffectTypes.DirectDamage
                        }
                    }
                },
                new CombatTacticalInstantAbilityProfileFixture
                {
                    Id = "Bash_T1_Prototype",
                    CastTimeSeconds = 0d,
                    CostMana = 10,
                    CooldownSeconds = 10.0d,
                    RangeMeters = 2.0d,
                    Effects = new()
                    {
                        new CombatTacticalInstantAbilityEffectFixture
                        {
                            EffectType = CombatTacticalAbilityEffectTypes.InterruptCurrentChannel
                        }
                    }
                },
                new CombatTacticalInstantAbilityProfileFixture
                {
                    Id = "DefensivePrayer_T1_Prototype",
                    CastTimeSeconds = 0d,
                    CostMana = 25,
                    CooldownSeconds = 30.0d,
                    RangeMeters = 0d,
                    RequiresTarget = false,
                    RequiresLineOfSight = false,
                    Effects = new()
                    {
                        new CombatTacticalInstantAbilityEffectFixture
                        {
                            EffectType = CombatTacticalAbilityEffectTypes.SelfBuff
                        }
                    }
                }
            }
        };

        var validation = new CombatFixtureValidator().Validate(package);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors, Has.Some.Contains("SmiteOfAuthority_T1_Prototype: direct damage values are required."));
        Assert.That(validation.Errors, Has.Some.Contains("Bash_T1_Prototype: interrupt_current_channel requires positive interrupt_seconds."));
        Assert.That(validation.Errors, Has.Some.Contains("DefensivePrayer_T1_Prototype: self_buff requires positive duration_seconds."));
    }

    [Test]
    public void test_combat_fixture_package_declares_regen_and_combat_exit_tuning()
    {
        var package = LoadPackage();
        var tuning = package.RegenAndCombatExitTuning;

        Assert.That(tuning.RegenTickIntervalSeconds, Is.EqualTo(6.0d).Within(0.000001d));
        Assert.That(tuning.CombatExitTimerSeconds, Is.EqualTo(30.0d).Within(0.000001d));
        Assert.That(tuning.SittingThreatBonus, Is.EqualTo(50));
        Assert.That(tuning.ManaRegen.BaseRegen, Is.EqualTo(1));
        Assert.That(tuning.ManaRegen.LevelRegenScalar, Is.EqualTo(0.10d).Within(0.000001d));
        Assert.That(tuning.ManaRegen.PercentRegenScalar, Is.EqualTo(0.005d).Within(0.000001d));
        Assert.That(tuning.ManaRegen.SittingPostureMultiplier, Is.EqualTo(4.0d).Within(0.000001d));
        Assert.That(tuning.ManaRegen.InCombatMultiplier, Is.EqualTo(0.0d).Within(0.000001d));
    }

    [Test]
    public void test_combat_fixture_validator_rejects_invalid_regen_and_combat_exit_tuning()
    {
        var package = LoadPackage() with
        {
            RegenAndCombatExitTuning = new CombatRegenAndCombatExitTuning
            {
                RegenTickIntervalSeconds = 0d,
                CombatExitTimerSeconds = 0d,
                SittingThreatBonus = -1,
                HealthRegen = new CombatResourceRegenTuning(),
                ManaRegen = new CombatResourceRegenTuning()
            }
        };

        var validation = new CombatFixtureValidator().Validate(package);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors, Has.Some.Contains("regen_tick_interval_seconds must be positive."));
        Assert.That(validation.Errors, Has.Some.Contains("combat_exit_timer_seconds must be positive."));
        Assert.That(validation.Errors, Has.Some.Contains("sitting_threat_bonus must not be negative."));
        Assert.That(validation.Errors, Has.Some.Contains("health_regen values must be non-negative"));
        Assert.That(validation.Errors, Has.Some.Contains("mana_regen values must be non-negative"));
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

        throw new DirectoryNotFoundException("Unable to locate repository root for combat fixture tests.");
    }
}
