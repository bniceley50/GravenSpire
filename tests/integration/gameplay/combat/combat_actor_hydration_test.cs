#nullable enable

using System;
using System.IO;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Combat.Fixtures;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Gameplay.Combat;

public sealed class CombatActorHydrationTest
{
    [Test]
    public void test_combat_actor_hydration_accepts_valid_cleric_mid_baseline()
    {
        var package = LoadPackage();
        var clericMid = package.ActorFixtures.Single(actor => actor.Id == "Cleric_Mid_T1");
        var snapshot = new CombatProgressionBaselineSnapshot(
            "local-character-1",
            "Cleric",
            clericMid.Level,
            clericMid.MaxHealth,
            clericMid.MaxMana,
            1,
            10,
            CombatProgressionBaselineProducedFor.InitialHydration);
        var input = new CombatActorHydrationInput(
            "combat-player-1",
            "Haunt_Prototype_T1",
            "player-local-character-1");

        var result = new CombatActorHydrator().HydratePlayerActor(snapshot, clericMid, input);

        Assert.That(result.Succeeded, Is.True, string.Join(Environment.NewLine, result.Errors));
        Assert.That(result.Actor, Is.Not.Null);
        Assert.That(result.Actor!.StableSourceRef.LocalCharacterId, Is.EqualTo("local-character-1"));
        Assert.That(result.Actor.Level, Is.EqualTo(5));
        Assert.That(result.Actor.MaxHealth, Is.EqualTo(140));
        Assert.That(result.Actor.CurrentHealth, Is.EqualTo(140));
        Assert.That(result.Actor.MaxMana, Is.EqualTo(180));
        Assert.That(result.Actor.CurrentMana, Is.EqualTo(180));
        Assert.That(result.Actor.MaxEndurance, Is.EqualTo(80));
        Assert.That(result.Actor.CurrentEndurance, Is.EqualTo(80));
        Assert.That(result.Actor.CombatState, Is.EqualTo(CombatState.OutOfCombat));
    }

    [Test]
    public void test_combat_actor_hydration_uses_fixture_endurance_for_all_cleric_bands()
    {
        var package = LoadPackage();
        var clericFixtures = package.ActorFixtures
            .Where(actor => string.Equals(actor.ClassId, "Cleric", StringComparison.Ordinal))
            .ToArray();

        Assert.That(clericFixtures.Select(actor => actor.Id), Is.EquivalentTo(new[]
        {
            "Cleric_Low_T1",
            "Cleric_Mid_T1",
            "Cleric_Top_T1"
        }));

        foreach (var fixture in clericFixtures)
        {
            var snapshot = new CombatProgressionBaselineSnapshot(
                "local-character-1",
                "Cleric",
                fixture.Level,
                fixture.MaxHealth,
                fixture.MaxMana,
                1,
                10,
                CombatProgressionBaselineProducedFor.InitialHydration);
            var input = new CombatActorHydrationInput(
                $"combat-player-{fixture.Id}",
                "Haunt_Prototype_T1",
                fixture.Id);

            var result = new CombatActorHydrator().HydratePlayerActor(snapshot, fixture, input);

            Assert.That(result.Succeeded, Is.True, string.Join(Environment.NewLine, result.Errors));
            Assert.That(result.Actor!.MaxEndurance, Is.EqualTo(80), fixture.Id);
            Assert.That(result.Actor.CurrentEndurance, Is.EqualTo(80), fixture.Id);
        }
    }

    [Test]
    public void test_combat_actor_hydration_rejects_missing_baseline_snapshot()
    {
        var package = LoadPackage();
        var clericMid = package.ActorFixtures.Single(actor => actor.Id == "Cleric_Mid_T1");
        var input = new CombatActorHydrationInput(
            "combat-player-1",
            "Haunt_Prototype_T1",
            "player-local-character-1");

        var result = new CombatActorHydrator().HydratePlayerActor(null, clericMid, input);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("CombatProgressionBaselineSnapshot is required"));
    }

    [Test]
    public void test_combat_actor_hydration_rejects_dead_current_health_without_death_handoff()
    {
        var package = LoadPackage();
        var clericMid = package.ActorFixtures.Single(actor => actor.Id == "Cleric_Mid_T1");
        var snapshot = new CombatProgressionBaselineSnapshot(
            "local-character-1",
            "Cleric",
            clericMid.Level,
            clericMid.MaxHealth,
            clericMid.MaxMana,
            1,
            10,
            CombatProgressionBaselineProducedFor.InitialHydration);
        var input = new CombatActorHydrationInput(
            "combat-player-1",
            "Haunt_Prototype_T1",
            "player-local-character-1",
            new CombatResourceHydrationState(0, clericMid.MaxMana, clericMid.MaxEndurance));

        var result = new CombatActorHydrator().HydratePlayerActor(snapshot, clericMid, input);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("current_health <= 0 without death handoff is invalid combat hydration"));
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

        throw new DirectoryNotFoundException("Unable to locate repository root for combat hydration tests.");
    }
}
