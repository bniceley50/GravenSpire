#nullable enable

using System;
using System.Collections.Generic;
using Gravenspire.Gameplay.Combat;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Gameplay.Combat;

public sealed class CombatPlayerDeathResolutionTest
{
    [Test]
    public void test_lethal_player_damage_clamps_health_to_zero_and_emits_one_death_event()
    {
        var resolver = new CombatPlayerDeathResolver();
        var player = CreatePlayer(currentHealth: 5)
            .WithTarget("combat-hostile-1")
            .SetThreat("combat-hostile-1", 45)
            .BeginCast("cast-001", "Smite_T1_Prototype", "combat-hostile-1")
            .WithCastProgress(1.5d);
        var request = DeathRequest(player, incomingDamage: 12);

        var first = resolver.Resolve(request);
        var second = resolver.Resolve(request);

        Assert.That(first.Processed, Is.True);
        Assert.That(first.PlayerAfterResolution.CurrentHealth, Is.EqualTo(0));
        Assert.That(first.PlayerAfterResolution.LifeState, Is.EqualTo(CombatActorLifeState.Dead));
        Assert.That(first.PlayerAfterResolution.CombatState, Is.EqualTo(CombatState.Dead));
        Assert.That(first.PlayerAfterResolution.IsAlive, Is.False);
        Assert.That(first.PlayerAfterResolution.TargetCombatActorId, Is.Null);
        Assert.That(first.PlayerAfterResolution.ThreatTable, Is.Empty);
        Assert.That(first.PlayerAfterResolution.CastRuntimeState, Is.EqualTo(CombatCastRuntimeState.None));
        Assert.That(first.PlayerAfterResolution.ActiveCastId, Is.Null);

        Assert.That(first.DeathEvent, Is.Not.Null);
        Assert.That(first.DeathEvent!.local_character_id, Is.EqualTo("local-character-1"));
        Assert.That(first.DeathEvent.zoneId, Is.EqualTo("Haunt_Prototype_T1"));
        Assert.That(first.DeathEvent.death_position, Is.EqualTo(new CombatPoint3(7.5d, 0.0d, -2.25d)));
        Assert.That(first.DeathEvent.killer_source_ref, Is.EqualTo(KillerSource()));
        Assert.That(first.DeathEvent.death_cause_type, Is.EqualTo("npc_melee"));
        Assert.That(first.DeathEvent.death_context_id, Is.EqualTo(ExpectedDeathContextId()));

        Assert.That(second.Processed, Is.False);
        Assert.That(second.DeathEvent, Is.Null);
    }

    [Test]
    public void test_nonlethal_player_damage_does_not_emit_death_event()
    {
        var resolver = new CombatPlayerDeathResolver();
        var player = CreatePlayer(currentHealth: 20);

        var result = resolver.Resolve(DeathRequest(player, incomingDamage: 7));

        Assert.That(result.Processed, Is.False);
        Assert.That(result.PlayerAfterResolution.CurrentHealth, Is.EqualTo(20));
        Assert.That(result.DeathEvent, Is.Null);
    }

    [Test]
    public void test_death_context_id_is_deterministic_from_stable_death_inputs()
    {
        var first = CombatPlayerDeathResolver.CreateDeathContextId(
            "local-character-1",
            "Haunt_Prototype_T1",
            new CombatPoint3(7.5d, 0.0d, -2.25d),
            KillerSource(),
            "npc_melee");
        var second = CombatPlayerDeathResolver.CreateDeathContextId(
            "local-character-1",
            "Haunt_Prototype_T1",
            new CombatPoint3(7.5d, 0.0d, -2.25d),
            KillerSource(),
            "npc_melee");
        var differentPosition = CombatPlayerDeathResolver.CreateDeathContextId(
            "local-character-1",
            "Haunt_Prototype_T1",
            new CombatPoint3(7.5d, 0.0d, -2.0d),
            KillerSource(),
            "npc_melee");

        Assert.That(first, Is.EqualTo(second));
        Assert.That(first, Does.StartWith("DCTX-"));
        Assert.That(first, Is.Not.EqualTo(differentPosition));
    }

    [Test]
    public void test_player_death_payload_uses_stable_local_identity_and_not_runtime_identity()
    {
        var resolver = new CombatPlayerDeathResolver();
        var player = CreatePlayer(currentHealth: 1, combatActorId: "runtime-player-session-999");

        var result = resolver.Resolve(DeathRequest(player, incomingDamage: 2));

        Assert.That(result.DeathEvent, Is.Not.Null);
        Assert.That(result.DeathEvent!.local_character_id, Is.EqualTo("local-character-1"));
        Assert.That(result.DeathEvent.death_context_id, Does.Not.Contain("runtime-player-session-999"));
    }

    private static CombatPlayerDeathResolutionRequest DeathRequest(
        CombatActorState player,
        int incomingDamage)
    {
        return new CombatPlayerDeathResolutionRequest(
            player,
            incomingDamage,
            new CombatPoint3(7.5d, 0.0d, -2.25d),
            KillerSource(),
            "npc_melee");
    }

    private static string ExpectedDeathContextId()
    {
        return CombatPlayerDeathResolver.CreateDeathContextId(
            "local-character-1",
            "Haunt_Prototype_T1",
            new CombatPoint3(7.5d, 0.0d, -2.25d),
            KillerSource(),
            "npc_melee");
    }

    private static CombatStableSourceRef KillerSource()
    {
        return CombatStableSourceRef.ForSpawn(new CombatSpawnSourceRef(
            "VampireCourt_T1",
            "solo-trash-anchor-1",
            "VampireThrall_T1"));
    }

    private static CombatActorState CreatePlayer(
        int currentHealth = 140,
        string combatActorId = "combat-player-1")
    {
        return new CombatActorState(
            combatActorId,
            CombatActorKind.Player,
            CombatStableSourceRef.ForPlayer("local-character-1"),
            "PlayerLocal_T1",
            "Haunt_Prototype_T1",
            5,
            140,
            currentHealth,
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
            currentHealth > 0 ? CombatState.InCombat : CombatState.Dead,
            currentHealth > 0 ? CombatActorLifeState.Alive : CombatActorLifeState.Dead,
            null,
            "player-local-character-1",
            new Dictionary<string, int>(StringComparer.Ordinal));
    }
}
