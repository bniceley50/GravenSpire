#nullable enable

using System;
using System.IO;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Combat.Fixtures;
using NUnit.Framework;

namespace Gravenspire.Tests.Unit.Gameplay.Combat;

public sealed class CombatExitTimerTest
{
    [Test]
    public void test_combat_exit_returns_true_only_after_fixture_boundary_and_zero_valid_hostiles()
    {
        var package = LoadPackage();
        var machine = new CombatExitStateMachine();

        var beforeBoundary = machine.Evaluate(new CombatExitTimerRequest(
            new CombatTick(1500, 30.0d),
            CombatTick.Zero,
            package.CombatTickRateHz,
            ValidHostileThreatEntries: 0,
            package.RegenAndCombatExitTuning));
        var afterBoundary = machine.Evaluate(new CombatExitTimerRequest(
            new CombatTick(1505, 30.1d),
            CombatTick.Zero,
            package.CombatTickRateHz,
            ValidHostileThreatEntries: 0,
            package.RegenAndCombatExitTuning));
        var hostileStillValid = machine.Evaluate(new CombatExitTimerRequest(
            new CombatTick(1505, 30.1d),
            CombatTick.Zero,
            package.CombatTickRateHz,
            ValidHostileThreatEntries: 1,
            package.RegenAndCombatExitTuning));

        Assert.That(beforeBoundary.CanExitCombat, Is.False);
        Assert.That(afterBoundary.CanExitCombat, Is.True);
        Assert.That(hostileStillValid.CanExitCombat, Is.False);
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

        throw new DirectoryNotFoundException("Unable to locate repository root for combat exit tests.");
    }
}
