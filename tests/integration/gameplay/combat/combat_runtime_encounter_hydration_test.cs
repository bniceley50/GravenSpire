#nullable enable

using System;
using System.IO;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Combat.Fixtures;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Gameplay.Combat;

public sealed class CombatRuntimeEncounterHydrationTest
{
    [Test]
    public void test_runtime_encounter_hydrator_loads_solo_trash_bridge_actor_set()
    {
        var result = Hydrate("SoloTrash_EvenCon_T1");

        Assert.That(result.Succeeded, Is.True, string.Join(Environment.NewLine, result.Errors));
        Assert.That(result.FixtureFilePath.Replace('\\', '/'), Does.EndWith("data/combat/t1-combat-fixtures.json"));
        Assert.That(result.FixtureSetVersion, Is.Not.Empty);
        Assert.That(result.ActiveZoneId, Is.EqualTo("Haunt_Prototype_T1"));
        Assert.That(result.EncounterFixtureIds, Is.EquivalentTo(new[] { "SoloTrash_EvenCon_T1" }));
        Assert.That(result.ActorFixtureIds, Is.EquivalentTo(new[] { "Cleric_Mid_T1", "Trash_Mid_T1" }));
        Assert.That(result.PlayerActor, Is.Not.Null);
        Assert.That(result.PlayerActor!.CombatActorId, Is.EqualTo("m2-player-cleric"));
        Assert.That(result.PlayerActor.ActorKind, Is.EqualTo(CombatActorKind.Player));
        Assert.That(result.PlayerActor.ZoneId, Is.EqualTo("Haunt_Prototype_T1"));
        Assert.That(result.HostileActors.Select(actor => actor.CombatActorId), Is.EquivalentTo(new[] { "m2-hostile-1" }));
        Assert.That(result.HostileActors.Single().ActorKind, Is.EqualTo(CombatActorKind.NPC));
        Assert.That(result.HostileActors.Single().ZoneId, Is.EqualTo("Haunt_Prototype_T1"));
    }

    [Test]
    public void test_runtime_encounter_hydrator_uses_existing_fixture_values_without_reauthoring_tuning()
    {
        var package = LoadPackage();
        var cleric = package.ActorFixtures.Single(actor => actor.Id == "Cleric_Mid_T1");
        var trash = package.ActorFixtures.Single(actor => actor.Id == "Trash_Mid_T1");
        var result = Hydrate("SoloTrash_EvenCon_T1");

        Assert.That(result.Succeeded, Is.True, string.Join(Environment.NewLine, result.Errors));
        Assert.That(result.PlayerActor!.MaxHealth, Is.EqualTo(cleric.MaxHealth));
        Assert.That(result.PlayerActor.MaxMana, Is.EqualTo(cleric.MaxMana));
        Assert.That(result.PlayerActor.MaxEndurance, Is.EqualTo(cleric.MaxEndurance));
        Assert.That(result.HostileActors.Single().MaxHealth, Is.EqualTo(trash.MaxHealth));
        Assert.That(result.HostileActors.Single().AttackPower, Is.EqualTo(trash.AttackPower));
        Assert.That(result.HostileActors.Single().WeaponBaseDamage, Is.EqualTo(trash.WeaponBaseDamage));
        Assert.That(result.HostileActors.Single().WeaponDelaySeconds, Is.EqualTo(trash.WeaponDelaySeconds));
    }

    [Test]
    public void test_runtime_encounter_hydrator_fails_loud_on_missing_encounter_fixture()
    {
        var result = Hydrate("Missing_M2_Encounter");

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("encounter fixture 'Missing_M2_Encounter' was not found"));
        Assert.That(result.PlayerActor, Is.Null);
        Assert.That(result.HostileActors, Is.Empty);
    }

    [Test]
    public void test_runtime_encounter_hydrator_fails_loud_on_missing_fixture_file()
    {
        var result = HydrateFromFile(Path.Combine(TestContext.CurrentContext.WorkDirectory, "missing-m2-fixture.json"));

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("Could not find file"));
        Assert.That(result.PlayerActor, Is.Null);
        Assert.That(result.HostileActors, Is.Empty);
    }

    [Test]
    public void test_runtime_encounter_hydrator_fails_loud_on_malformed_fixture_json()
    {
        var malformedPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "malformed-m2-fixture.json");
        File.WriteAllText(malformedPath, "{ malformed fixture json");

        var exception = Assert.Throws<InvalidDataException>(() => new CombatFixtureLoader().LoadFromJson("{ malformed fixture json"));
        Assert.That(exception!.Message, Does.Contain("Combat fixture JSON could not be parsed."));
        Assert.That(exception.InnerException, Is.Not.Null);
        Assert.That(exception.InnerException!.GetType().FullName, Does.Contain("Json"));

        var result = HydrateFromFile(malformedPath);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("Combat fixture JSON could not be parsed."));
        Assert.That(result.PlayerActor, Is.Null);
        Assert.That(result.HostileActors, Is.Empty);
    }

    private static CombatRuntimeEncounterHydrationResult Hydrate(string encounterFixtureId)
    {
        return HydrateFromFile(FixturePath(), encounterFixtureId);
    }

    private static CombatRuntimeEncounterHydrationResult HydrateFromFile(
        string fixturePath,
        string encounterFixtureId = "SoloTrash_EvenCon_T1")
    {
        return new CombatRuntimeEncounterHydrator().HydrateFromFile(
            fixturePath,
            new CombatRuntimeEncounterHydrationRequest
            {
                EncounterFixtureId = encounterFixtureId,
                ActiveZoneId = "Haunt_Prototype_T1",
                PlayerCombatActorId = "m2-player-cleric",
                PlayerLocalCharacterId = "local-character-m2-dev",
                HostileCombatActorIdPrefix = "m2-hostile"
            });
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

        throw new DirectoryNotFoundException("Unable to locate repository root for runtime encounter hydration tests.");
    }
}
