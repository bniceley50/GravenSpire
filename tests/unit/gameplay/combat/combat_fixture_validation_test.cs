#nullable enable

using System;
using System.IO;
using System.Linq;
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
