#nullable enable

using System;
using System.IO;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Combat.Fixtures;
using NUnit.Framework;

namespace Gravenspire.Tests.Unit.Gameplay.Combat;

public sealed class CombatRegenFormulasTest
{
    [Test]
    public void test_cleric_mid_t1_sitting_out_of_combat_regenerates_eight_mana_from_fixture_tuning()
    {
        var package = LoadPackage();
        var actor = CreateActorFromFixture(package, "Cleric_Mid_T1") with
        {
            PostureState = CombatPostureState.Sitting
        };

        var regen = CombatRegenFormulas.CalculateResourceRegenPerTick(new CombatResourceRegenRequest(
            actor,
            CombatResourceKind.Mana,
            package.RegenAndCombatExitTuning,
            actor.PostureState,
            CombatState.OutOfCombat));

        Assert.That(regen, Is.EqualTo(8));
    }

    [Test]
    public void test_sitting_in_combat_uses_combat_multiplier_without_med_boost()
    {
        var package = LoadPackage();
        var actor = CreateActorFromFixture(package, "Cleric_Mid_T1") with
        {
            PostureState = CombatPostureState.Sitting
        };

        var regen = CombatRegenFormulas.CalculateResourceRegenPerTick(new CombatResourceRegenRequest(
            actor,
            CombatResourceKind.Mana,
            package.RegenAndCombatExitTuning,
            actor.PostureState,
            CombatState.InCombat));

        Assert.That(regen, Is.EqualTo(0));
    }

    [Test]
    public void test_regen_tick_interval_uses_fixture_seconds_and_combat_tick_rate()
    {
        var package = LoadPackage();

        var intervalTicks = CombatRegenFormulas.CalculateRegenTickIntervalTicks(
            package.RegenAndCombatExitTuning,
            package.CombatTickRateHz);

        Assert.That(intervalTicks, Is.EqualTo(300));
    }

    private static CombatActorState CreateActorFromFixture(CombatFixturePackage package, string fixtureId)
    {
        var fixture = package.ActorFixtures.Single(actor => actor.Id == fixtureId);
        return new CombatActorState(
            "combat-player-1",
            fixture.ActorKind,
            CombatStableSourceRef.ForPlayer("local-character-1"),
            fixture.FactionId,
            "Haunt_Prototype_T1",
            fixture.Level,
            fixture.MaxHealth,
            fixture.MaxHealth,
            fixture.MaxMana,
            fixture.MaxMana,
            fixture.ArmorClass,
            fixture.AttackPower,
            fixture.WeaponBaseDamage,
            fixture.AttackSkill,
            fixture.DefenseSkill,
            fixture.WeaponDelaySeconds,
            fixture.MeleeRangeMeters,
            fixture.SpellRangeMeters,
            CombatState.OutOfCombat,
            CombatActorLifeState.Alive,
            null,
            "player-local-character-1");
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

        throw new DirectoryNotFoundException("Unable to locate repository root for combat regen tests.");
    }
}
