#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Combat.Fixtures;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Gameplay.Combat;

public sealed class CombatNpcDeathKillCreditTest
{
    [Test]
    public void test_npc_death_emits_combat_actor_death_event_once()
    {
        var phase = new CombatKillResolutionPhase();
        var player = CreatePlayer();
        var defeated = CreateDefeatedSpawnHostile()
            .SetThreat(player.CombatActorId, 25);

        var first = phase.Resolve(new CombatKillResolutionRequest(defeated, player, KillWeightSeed()));
        var second = phase.Resolve(new CombatKillResolutionRequest(defeated, player, KillWeightSeed()));

        Assert.That(first.Processed, Is.True);
        Assert.That(first.DeathEvent, Is.Not.Null);
        Assert.That(first.DeathEvent!.combat_actor_id, Is.EqualTo(defeated.CombatActorId));
        Assert.That(first.DeathEvent.defeated_source_ref, Is.EqualTo(defeated.StableSourceRef));
        Assert.That(first.DeathEvent.zoneId, Is.EqualTo(defeated.ZoneId));
        Assert.That(second.Processed, Is.False);
        Assert.That(second.DeathEvent, Is.Null);
        Assert.That(second.KillCreditEvent, Is.Null);
    }

    [Test]
    public void test_qualifying_player_threat_emits_one_player_kill_credit_event_with_stable_payload()
    {
        var phase = new CombatKillResolutionPhase();
        var player = CreatePlayer();
        var defeated = CreateDefeatedSpawnHostile()
            .SetThreat(player.CombatActorId, 25);

        var result = phase.Resolve(new CombatKillResolutionRequest(defeated, player, KillWeightSeed()));

        Assert.That(result.Processed, Is.True);
        Assert.That(result.KillCreditEvent, Is.Not.Null);
        Assert.That(result.KillCreditEvent!.defeated_source_ref, Is.EqualTo(defeated.StableSourceRef));
        Assert.That(result.KillCreditEvent.zoneId, Is.EqualTo(defeated.ZoneId));
        Assert.That(result.KillCreditEvent.faction_id, Is.EqualTo("VampireCourt_T1"));
        Assert.That(result.KillCreditEvent.kill_weight_seed, Is.EqualTo(1.25d).Within(0.000001d));
    }

    [Test]
    public void test_no_qualifying_player_contribution_emits_death_event_but_no_kill_credit_event()
    {
        var phase = new CombatKillResolutionPhase();
        var player = CreatePlayer();
        var defeated = CreateDefeatedSpawnHostile();

        var result = phase.Resolve(new CombatKillResolutionRequest(defeated, player, KillWeightSeed()));

        Assert.That(result.Processed, Is.True);
        Assert.That(result.DeathEvent, Is.Not.Null);
        Assert.That(result.KillCreditEvent, Is.Null);
    }

    [Test]
    public void test_repeat_processing_same_defeated_runtime_actor_does_not_double_emit()
    {
        var phase = new CombatKillResolutionPhase();
        var player = CreatePlayer();
        var defeated = CreateDefeatedSpawnHostile()
            .SetThreat(player.CombatActorId, 25);

        var first = phase.Resolve(new CombatKillResolutionRequest(defeated, player, KillWeightSeed()));
        var second = phase.Resolve(new CombatKillResolutionRequest(defeated, player, KillWeightSeed()));

        Assert.That(first.DeathEvent, Is.Not.Null);
        Assert.That(first.KillCreditEvent, Is.Not.Null);
        Assert.That(second.DeathEvent, Is.Null);
        Assert.That(second.KillCreditEvent, Is.Null);
    }

    [Test]
    public void test_player_damage_contribution_can_qualify_without_parallel_persistent_state()
    {
        var phase = new CombatKillResolutionPhase();
        var player = CreatePlayer();
        var defeated = CreateDefeatedSpawnHostile();

        var result = phase.Resolve(new CombatKillResolutionRequest(
            defeated,
            player,
            KillWeightSeed(),
            PlayerDamageContribution: 16));

        Assert.That(result.KillCreditEvent, Is.Not.Null);
        Assert.That(result.KillCreditEvent!.defeated_source_ref, Is.EqualTo(defeated.StableSourceRef));
    }

    [Test]
    public void test_player_kill_credit_event_payload_schema_contains_exactly_approved_four_fields()
    {
        var properties = typeof(PlayerKillCreditEvent)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.That(properties, Is.EqualTo(new[]
        {
            "defeated_source_ref",
            "faction_id",
            "kill_weight_seed",
            "zoneId"
        }.OrderBy(name => name, StringComparer.Ordinal).ToArray()));

        Assert.That(properties, Has.No.Member("combat_actor_id"));
        Assert.That(properties, Has.No.Member("defeated_level"));
        Assert.That(properties, Has.No.Member("encounter_role"));
        Assert.That(properties, Has.No.Member("xp_value"));
        Assert.That(properties, Has.No.Member("xp_metadata"));
        Assert.That(properties, Has.No.Member("spell_data"));
        Assert.That(properties, Has.No.Member("progression_transaction_id"));
        Assert.That(properties, Has.No.Member("threat_table_snapshot"));
        Assert.That(properties, Has.No.Member("loot"));
        Assert.That(properties, Has.No.Member("corpse_record"));
        Assert.That(properties, Has.No.Member("tick_id"));
    }

    [Test]
    public void test_player_kill_credit_payload_uses_persistent_npc_source_ref_when_available()
    {
        var phase = new CombatKillResolutionPhase();
        var player = CreatePlayer();
        var defeated = CreateDefeatedPersistentNpc()
            .SetThreat(player.CombatActorId, 25);

        var result = phase.Resolve(new CombatKillResolutionRequest(defeated, player, KillWeightSeed()));

        Assert.That(result.KillCreditEvent, Is.Not.Null);
        Assert.That(result.KillCreditEvent!.defeated_source_ref.SourceNpcId, Is.EqualTo("Named_XP_Smoke_T1"));
        Assert.That(result.KillCreditEvent.defeated_source_ref.SourceSpawnRef, Is.Null);
        Assert.That(result.KillCreditEvent.zoneId, Is.EqualTo("Haunt_Prototype_T1"));
    }

    [Test]
    public void test_kill_resolution_holds_kill_credit_until_progression_acknowledges()
    {
        var phase = new CombatKillResolutionPhase();
        var player = CreatePlayer();
        var defeated = CreateDefeatedSpawnHostile()
            .SetThreat(player.CombatActorId, 25);
        var progressionSink = new TestAcknowledgementSink(
            "CharacterProgressionAwardSnapshot",
            CombatKillCreditAcknowledgementStatus.Pending);
        var npcSink = new TestAcknowledgementSink(
            "NpcSourceLifecycle",
            CombatKillCreditAcknowledgementStatus.Acknowledged);

        var result = phase.ResolveWithAcknowledgements(
            new CombatKillResolutionRequest(defeated, player, KillWeightSeed()),
            new ICombatKillCreditAcknowledgementSink[] { progressionSink, npcSink });

        Assert.That(result.Resolution.Processed, Is.True);
        Assert.That(result.Resolution.KillCreditEvent, Is.Not.Null);
        Assert.That(result.HoldStatus.HasHeldKillCreditEvent, Is.True);
        Assert.That(result.HoldStatus.IsAcknowledged, Is.False);
        Assert.That(result.HoldStatus.PendingAcknowledgements, Is.EqualTo(new[] { "CharacterProgressionAwardSnapshot" }));
        Assert.That(result.HoldStatus.HeldKillCreditEvent!.defeated_source_ref, Is.EqualTo(defeated.StableSourceRef));

        var holdId = result.HoldStatus.HoldId!;
        Assert.That(phase.AcknowledgeHeldKillCredit(holdId, "CharacterProgressionAwardSnapshot"), Is.True);

        var released = phase.GetHeldKillCreditStatus(holdId);
        Assert.That(released.HasHeldKillCreditEvent, Is.False);
        Assert.That(released.IsAcknowledged, Is.True);
        Assert.That(released.PendingAcknowledgements, Is.Empty);
    }

    [Test]
    public void test_repeat_kill_resolution_does_not_reemit_held_kill_credit()
    {
        var phase = new CombatKillResolutionPhase();
        var player = CreatePlayer();
        var defeated = CreateDefeatedSpawnHostile()
            .SetThreat(player.CombatActorId, 25);
        var progressionSink = new TestAcknowledgementSink(
            "CharacterProgressionAwardSnapshot",
            CombatKillCreditAcknowledgementStatus.Pending);
        var npcSink = new TestAcknowledgementSink(
            "NpcSourceLifecycle",
            CombatKillCreditAcknowledgementStatus.Acknowledged);

        var first = phase.ResolveWithAcknowledgements(
            new CombatKillResolutionRequest(defeated, player, KillWeightSeed()),
            new ICombatKillCreditAcknowledgementSink[] { progressionSink, npcSink });
        var second = phase.ResolveWithAcknowledgements(
            new CombatKillResolutionRequest(defeated, player, KillWeightSeed()),
            new ICombatKillCreditAcknowledgementSink[] { progressionSink, npcSink });

        Assert.That(first.Resolution.KillCreditEvent, Is.Not.Null);
        Assert.That(first.HoldStatus.HasHeldKillCreditEvent, Is.True);
        Assert.That(second.Resolution.Processed, Is.False);
        Assert.That(second.Resolution.KillCreditEvent, Is.Null);

        var held = phase.GetHeldKillCreditStatus(first.HoldStatus.HoldId!);
        Assert.That(held.HasHeldKillCreditEvent, Is.True);
        Assert.That(held.PendingAcknowledgements, Is.EqualTo(new[] { "CharacterProgressionAwardSnapshot" }));
    }

    private static double KillWeightSeed()
    {
        return LoadPackage().EncounterFixtures.Single(encounter => encounter.Id == "SoloTrash_EvenCon_T1").KillWeightSeed;
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

        throw new DirectoryNotFoundException("Unable to locate repository root for kill-credit integration tests.");
    }

    private static CombatActorState CreatePlayer()
    {
        return new CombatActorState(
            "combat-player-1",
            CombatActorKind.Player,
            CombatStableSourceRef.ForPlayer("local-character-1"),
            "PlayerLocal_T1",
            "Haunt_Prototype_T1",
            5,
            140,
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
            CombatActorLifeState.Alive,
            null,
            "player-local-character-1");
    }

    private static CombatActorState CreateDefeatedSpawnHostile()
    {
        return CreateHostile(
            "combat-hostile-1",
            CombatStableSourceRef.ForSpawn(new CombatSpawnSourceRef("VampireCourt_T1", "solo-trash-anchor-1", "VampireThrall_T1")));
    }

    private static CombatActorState CreateDefeatedPersistentNpc()
    {
        return CreateHostile(
            "combat-named-1",
            CombatStableSourceRef.ForPersistentNpc("Named_XP_Smoke_T1"));
    }

    private static CombatActorState CreateHostile(string combatActorId, CombatStableSourceRef sourceRef)
    {
        return new CombatActorState(
            combatActorId,
            CombatActorKind.NPC,
            sourceRef,
            "VampireCourt_T1",
            "Haunt_Prototype_T1",
            5,
            120,
            0,
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
            CombatState.Dead,
            CombatActorLifeState.Dead,
            null,
            $"{combatActorId}-sort");
    }

    private sealed class TestAcknowledgementSink : ICombatKillCreditAcknowledgementSink
    {
        private readonly CombatKillCreditAcknowledgementStatus status;

        public TestAcknowledgementSink(string consumerName, CombatKillCreditAcknowledgementStatus status)
        {
            ConsumerName = consumerName;
            this.status = status;
        }

        public string ConsumerName { get; }

        public CombatKillCreditAcknowledgement Acknowledge(PlayerKillCreditEvent killCreditEvent)
        {
            Assert.That(killCreditEvent.defeated_source_ref, Is.Not.Null);
            return status switch
            {
                CombatKillCreditAcknowledgementStatus.Acknowledged => CombatKillCreditAcknowledgement.Acknowledged(ConsumerName),
                CombatKillCreditAcknowledgementStatus.Rejected => CombatKillCreditAcknowledgement.Rejected(ConsumerName, "test rejection"),
                _ => CombatKillCreditAcknowledgement.Pending(ConsumerName)
            };
        }
    }
}
