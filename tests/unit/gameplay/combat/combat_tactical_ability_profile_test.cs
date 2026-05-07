#nullable enable

using System;
using System.IO;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Combat.Fixtures;
using NUnit.Framework;

namespace Gravenspire.Tests.Unit.Gameplay.Combat;

public sealed class CombatTacticalAbilityProfileTest
{
    [Test]
    public void test_tactical_ability_profile_resolves_fixture_loaded_mid_band_values()
    {
        var package = LoadPackage();
        var smiteFixture = package.TacticalInstantAbilityProfiles.Single(profile => profile.Id == "SmiteOfAuthority_T1_Prototype");

        var profile = CombatTacticalAbilityProfile.FromFixture(smiteFixture, "Mid");

        Assert.That(profile.AbilityId, Is.EqualTo("SmiteOfAuthority_T1_Prototype"));
        Assert.That(profile.CastTimeSeconds, Is.EqualTo(0d));
        Assert.That(profile.ResourceKind, Is.EqualTo(CombatTacticalAbilityResourceKind.Magical));
        Assert.That(profile.CostMana, Is.EqualTo(10));
        Assert.That(profile.CostEndurance, Is.EqualTo(0));
        Assert.That(profile.CooldownSeconds, Is.EqualTo(7.0d).Within(0.000001d));
        Assert.That(profile.RangeMeters, Is.EqualTo(30.0d).Within(0.000001d));
        Assert.That(profile.RequiresTarget, Is.True);
        Assert.That(profile.RequiresLineOfSight, Is.True);
        Assert.That(profile.Effects.Single().EffectType, Is.EqualTo(CombatTacticalAbilityEffectType.DirectDamage));
        Assert.That(profile.Effects.Single().Damage, Is.EqualTo(16));
    }

    [Test]
    public void test_tactical_ability_profile_preserves_declared_multi_effect_order()
    {
        var package = LoadPackage();
        var bashFixture = package.TacticalInstantAbilityProfiles.Single(profile => profile.Id == "Bash_T1_Prototype");

        var profile = CombatTacticalAbilityProfile.FromFixture(bashFixture, "Mid");

        Assert.That(profile.ResourceKind, Is.EqualTo(CombatTacticalAbilityResourceKind.Physical));
        Assert.That(profile.CostMana, Is.EqualTo(0));
        Assert.That(profile.CostEndurance, Is.EqualTo(10));
        Assert.That(profile.CooldownSeconds, Is.EqualTo(10.0d).Within(0.000001d));
        Assert.That(profile.RangeMeters, Is.EqualTo(2.0d).Within(0.000001d));
        Assert.That(profile.Effects.Select(effect => effect.EffectType), Is.EqualTo(new[]
        {
            CombatTacticalAbilityEffectType.DirectDamage,
            CombatTacticalAbilityEffectType.InterruptCurrentChannel
        }));
        Assert.That(profile.Effects.First().Damage, Is.EqualTo(11));
        Assert.That(profile.Effects.Last().InterruptSeconds, Is.EqualTo(1.0d).Within(0.000001d));
    }

    [Test]
    public void test_tactical_ability_profile_rejects_missing_requested_band()
    {
        var package = LoadPackage();
        var smiteFixture = package.TacticalInstantAbilityProfiles.Single(profile => profile.Id == "SmiteOfAuthority_T1_Prototype");

        var error = Assert.Throws<ArgumentException>(() =>
            CombatTacticalAbilityProfile.FromFixture(smiteFixture, "MissingBand"));

        Assert.That(error!.Message, Does.Contain("direct_damage is missing a value for fixture band MissingBand"));
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

        throw new DirectoryNotFoundException("Unable to locate repository root for tactical ability profile tests.");
    }
}
