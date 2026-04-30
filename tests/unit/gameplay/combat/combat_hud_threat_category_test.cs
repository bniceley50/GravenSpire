#nullable enable

using System;
using System.Collections.Generic;
using Gravenspire.Gameplay.Combat;
using NUnit.Framework;

namespace Gravenspire.Tests.Unit.Gameplay.Combat;

public sealed class CombatHudThreatCategoryTest
{
    [Test]
    public void test_absent_and_zero_threat_project_to_no_threat()
    {
        var player = CreatePlayer("combat-player-1");
        var tank = CreatePlayer("combat-tank-1");
        var hostileAbsent = CreateHostile()
            .WithTarget(tank.CombatActorId)
            .SetThreat(tank.CombatActorId, 100);
        var hostileZero = CreateHostile()
            .WithTarget(tank.CombatActorId)
            .SetThreat(player.CombatActorId, 0)
            .SetThreat(tank.CombatActorId, 100);

        var absent = CombatHudStateProjection.EvaluateThreatCategory(Request(player, hostileAbsent, tank));
        var zero = CombatHudStateProjection.EvaluateThreatCategory(Request(player, hostileZero, tank));

        Assert.That(absent, Is.EqualTo(CombatHudThreatCategory.NoThreat));
        Assert.That(zero, Is.EqualTo(CombatHudThreatCategory.NoThreat));
    }

    [Test]
    public void test_non_top_threat_projects_listed_or_close_without_raw_numeric_output()
    {
        var player = CreatePlayer("combat-player-1");
        var tank = CreatePlayer("combat-tank-1");
        var listedHostile = CreateHostile()
            .WithTarget(tank.CombatActorId)
            .SetThreat(player.CombatActorId, 84)
            .SetThreat(tank.CombatActorId, 100);
        var closeHostile = CreateHostile()
            .WithTarget(tank.CombatActorId)
            .SetThreat(player.CombatActorId, 85)
            .SetThreat(tank.CombatActorId, 100);

        var listed = CombatHudStateProjection.EvaluateThreatCategory(Request(player, listedHostile, tank));
        var close = CombatHudStateProjection.EvaluateThreatCategory(Request(player, closeHostile, tank));

        Assert.That(listed, Is.EqualTo(CombatHudThreatCategory.ThreatListed));
        Assert.That(close, Is.EqualTo(CombatHudThreatCategory.ThreatClose));
    }

    [Test]
    public void test_current_target_threat_projects_stable_or_contested_without_raw_numeric_output()
    {
        var player = CreatePlayer("combat-player-1");
        var tank = CreatePlayer("combat-tank-1");
        var stableHostile = CreateHostile()
            .WithTarget(player.CombatActorId)
            .SetThreat(player.CombatActorId, 100)
            .SetThreat(tank.CombatActorId, 89);
        var contestedHostile = CreateHostile()
            .WithTarget(player.CombatActorId)
            .SetThreat(player.CombatActorId, 100)
            .SetThreat(tank.CombatActorId, 90);

        var stable = CombatHudStateProjection.EvaluateThreatCategory(Request(player, stableHostile, tank));
        var contested = CombatHudStateProjection.EvaluateThreatCategory(Request(player, contestedHostile, tank));

        Assert.That(stable, Is.EqualTo(CombatHudThreatCategory.HasAggroStable));
        Assert.That(contested, Is.EqualTo(CombatHudThreatCategory.HasAggroContested));
    }

    [Test]
    public void test_dead_or_out_of_zone_threat_entries_are_ignored()
    {
        var player = CreatePlayer("combat-player-1");
        var deadTank = CreatePlayer(
            "combat-dead-tank-1",
            currentHealth: 0,
            lifeState: CombatActorLifeState.Dead);
        var otherZoneTank = CreatePlayer("combat-other-zone-tank-1", zoneId: "OtherZone_T1");
        var hostile = CreateHostile()
            .WithTarget(player.CombatActorId)
            .SetThreat(player.CombatActorId, 100)
            .SetThreat(deadTank.CombatActorId, 100)
            .SetThreat(otherZoneTank.CombatActorId, 100);

        var category = CombatHudStateProjection.EvaluateThreatCategory(
            Request(player, hostile, deadTank, otherZoneTank));

        Assert.That(category, Is.EqualTo(CombatHudThreatCategory.HasAggroStable));
    }

    [Test]
    public void test_negative_threat_entry_fails_loudly_before_category_projection()
    {
        var player = CreatePlayer("combat-player-1");
        var hostile = CreateHostile(new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [player.CombatActorId] = -1
        });

        Assert.Throws<InvalidOperationException>(() =>
            CombatHudStateProjection.EvaluateThreatCategory(Request(player, hostile)));
    }

    private static CombatHudThreatCategoryRequest Request(
        CombatActorState player,
        CombatActorState hostile,
        params CombatActorState[] extraActors)
    {
        var actors = new List<CombatActorState> { player };
        actors.AddRange(extraActors);
        return new CombatHudThreatCategoryRequest(
            player,
            hostile,
            actors,
            new CombatHudThreatCategoryTuning(0.85d, 0.90d));
    }

    private static CombatActorState CreatePlayer(
        string combatActorId,
        string zoneId = "Haunt_Prototype_T1",
        int currentHealth = 140,
        CombatActorLifeState lifeState = CombatActorLifeState.Alive)
    {
        return new CombatActorState(
            combatActorId,
            CombatActorKind.Player,
            CombatStableSourceRef.ForPlayer($"{combatActorId}-local"),
            "PlayerLocal_T1",
            zoneId,
            5,
            currentHealth,
            140,
            180,
            180,
            35,
            25,
            8,
            30,
            30,
            2.8d,
            2.0d,
            30.0d,
            CombatState.OutOfCombat,
            lifeState,
            null,
            combatActorId);
    }

    private static CombatActorState CreateHostile(IReadOnlyDictionary<string, int>? threatTable = null)
    {
        return new CombatActorState(
            "combat-hostile-1",
            CombatActorKind.NPC,
            CombatStableSourceRef.ForSpawn(new CombatSpawnSourceRef("VampireCourt_T1", "hostile-anchor-1", "VampireThrall_T1")),
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
            CombatState.InCombat,
            CombatActorLifeState.Alive,
            null,
            "hostile-001",
            threatTable);
    }
}
