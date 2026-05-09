#nullable enable

using System;
using System.IO;
using System.Linq;
using Gravenspire.Gameplay.Combat.Fixtures;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Gameplay.Combat;

public sealed class CombatFixtureLoadingTest
{
    [Test]
    public void test_combat_fixture_loader_resolves_spells_tactical_instants_and_encounters()
    {
        var package = LoadPackage();

        Assert.That(package.SpellFixtures.Select(spell => spell.Id), Does.Contain("Smite_T1_Prototype"));
        Assert.That(package.SpellFixtures.Select(spell => spell.Id), Does.Contain("LesserHeal_T1_Prototype"));
        Assert.That(package.TacticalInstantFixtures.Select(spell => spell.Id), Does.Contain("SmiteOfAuthority_T1_Prototype"));
        Assert.That(package.TacticalInstantFixtures.Select(spell => spell.Id), Does.Contain("Bash_T1_Prototype"));
        Assert.That(package.TacticalInstantFixtures.Select(spell => spell.Id), Does.Contain("DefensivePrayer_T1_Prototype"));
        Assert.That(package.EncounterFixtures.Select(encounter => encounter.Id), Does.Contain("SoloTrash_EvenCon_T1"));
        Assert.That(package.EncounterFixtures.Select(encounter => encounter.Id), Does.Contain("TwoTrash_Overpull_T1"));
        Assert.That(package.EncounterFixtures.Select(encounter => encounter.Id), Does.Contain("NamedSoloBlock_T1"));
    }

    [Test]
    public void test_combat_fixture_loader_keeps_fixture_tuning_values_out_of_production_logic()
    {
        var repoRoot = FindRepoRoot();
        var sourceDirectory = Path.Combine(repoRoot, "src", "gameplay", "combat");
        var productionSource = string.Join(
            Environment.NewLine,
            Directory.GetFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories)
                .Select(File.ReadAllText));

        Assert.That(productionSource, Does.Not.Contain("maxHealth = 140"));
        Assert.That(productionSource, Does.Not.Contain("MaxHealth = 140"));
        Assert.That(productionSource, Does.Not.Contain("maxMana = 180"));
        Assert.That(productionSource, Does.Not.Contain("MaxMana = 180"));
        Assert.That(productionSource, Does.Not.Contain("1.25"));
        Assert.That(productionSource, Does.Not.Contain("2.8"));
    }

    [Test]
    public void test_combat_fixture_loader_exposes_source_ref_aliases_for_downstream_validation()
    {
        var package = LoadPackage();

        var soloTrash = package.EncounterFixtures.Single(encounter => encounter.Id == "SoloTrash_EvenCon_T1");
        var named = package.EncounterFixtures.Single(encounter => encounter.Id == "NamedSoloBlock_T1");

        Assert.That(soloTrash.KillWeightSeed, Is.EqualTo(1.25d).Within(0.000001d));
        Assert.That(soloTrash.SourceRefAliases, Does.Contain("source_spawn_ref:SoloTrash_EvenCon_T1"));
        Assert.That(named.SourceRefAliases, Does.Contain("source_npc_id:Named_XP_Smoke_T1"));
    }

    [Test]
    public void test_solo_trash_required_outcome_tracks_d014_clean_state_target()
    {
        var package = LoadPackage();

        var soloTrash = package.EncounterFixtures.Single(encounter => encounter.Id == "SoloTrash_EvenCon_T1");

        Assert.That(soloTrash.RequiredOutcome, Does.Contain("90-100%"));
        Assert.That(soloTrash.RequiredOutcome, Does.Contain("clean-state seeded trials"));
        Assert.That(soloTrash.RequiredOutcome, Does.Contain("mean ending pressure"));
        Assert.That(soloTrash.RequiredOutcome, Does.Not.Contain("55-85%"));
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

        throw new DirectoryNotFoundException("Unable to locate repository root for combat fixture loading tests.");
    }
}
