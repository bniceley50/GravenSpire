#nullable enable

using System;
using System.Linq;
using System.Reflection;
using Gravenspire.Gameplay.Combat;
using NUnit.Framework;

namespace Gravenspire.Tests.Unit.Gameplay.Combat;

public sealed class CombatPersistenceProjectionTest
{
    [Test]
    public void test_combat_persistence_projection_exposes_exactly_whitelisted_four_properties()
    {
        var properties = typeof(CombatPersistenceProjection)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
        var names = properties.Select(property => property.Name).ToArray();

        Assert.That(properties, Has.Length.EqualTo(5));
        Assert.That(names, Is.EqualTo(new[]
        {
            "combat_life_state",
            "current_endurance",
            "current_health",
            "current_mana",
            "pending_death_handoff_payload"
        }.OrderBy(name => name, StringComparer.Ordinal).ToArray()));
        Assert.That(properties, Has.All.Property(nameof(PropertyInfo.CanWrite)).False);

        Assert.That(names, Has.No.Member("combat_" + "actor_id"));
        Assert.That(names, Has.No.Member("ThreatTable"));
        Assert.That(names, Has.No.Member("TargetCombatActorId"));
        Assert.That(names, Has.No.Member("CastRuntimeState"));
        Assert.That(names, Has.No.Member("PostureState"));
        Assert.That(names, Has.No.Member("NextRegenTickIndex"));
        Assert.That(names, Has.No.Member("ActiveCastId"));
    }

    [Test]
    public void test_projection_reads_player_resources_life_state_and_optional_death_handoff()
    {
        var player = CreatePlayer(currentHealth: 0, currentMana: 42, currentEndurance: 23, lifeState: CombatActorLifeState.Dead);
        var payload = new PlayerDeathEvent(
            "DCTX-test",
            "local-character-1",
            "Haunt_Prototype_T1",
            new CombatPoint3(1.25d, 0.0d, -3.5d),
            CombatStableSourceRef.ForSpawn(new CombatSpawnSourceRef("VampireCourt_T1", "anchor-1", "VampireThrall_T1")),
            "npc_melee");

        var projection = CombatPersistenceProjection.FromPlayer(player, payload);

        Assert.That(projection.current_health, Is.EqualTo(0));
        Assert.That(projection.current_mana, Is.EqualTo(42));
        Assert.That(projection.current_endurance, Is.EqualTo(23));
        Assert.That(projection.combat_life_state, Is.EqualTo(CombatActorLifeState.Dead));
        Assert.That(projection.pending_death_handoff_payload, Is.SameAs(payload));
    }

    [Test]
    public void test_projection_has_no_public_constructor_or_restore_method()
    {
        var publicConstructors = typeof(CombatPersistenceProjection)
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public);
        var publicStaticMethods = typeof(CombatPersistenceProjection)
            .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(method => method.Name)
            .ToArray();

        Assert.That(publicConstructors, Is.Empty);
        Assert.That(publicStaticMethods, Is.EqualTo(new[] { nameof(CombatPersistenceProjection.FromPlayer) }));
        Assert.That(publicStaticMethods, Has.No.Member("Hydrate"));
        Assert.That(publicStaticMethods, Has.No.Member("Restore"));
        Assert.That(publicStaticMethods, Has.No.Member("Load"));
    }

    private static CombatActorState CreatePlayer(
        int currentHealth = 140,
        int currentMana = 180,
        int currentEndurance = 80,
        CombatActorLifeState lifeState = CombatActorLifeState.Alive)
    {
        return new CombatActorState(
            "combat-player-1",
            CombatActorKind.Player,
            CombatStableSourceRef.ForPlayer("local-character-1"),
            "PlayerLocal_T1",
            "Haunt_Prototype_T1",
            5,
            140,
            currentHealth,
            180,
            currentMana,
            35,
            25,
            8,
            30,
            30,
            2.8d,
            2.0d,
            30.0d,
            currentHealth > 0 ? CombatState.OutOfCombat : CombatState.Dead,
            lifeState,
            null,
            "player-local-character-1",
            maxEndurance: 80,
            currentEndurance: currentEndurance);
    }
}
