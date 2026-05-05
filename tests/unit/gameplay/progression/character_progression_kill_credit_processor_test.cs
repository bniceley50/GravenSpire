#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Gravenspire.Core.Save;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Progression;
using NUnit.Framework;

namespace Gravenspire.Tests.Unit.Gameplay.Progression;

public sealed class CharacterProgressionKillCreditProcessorTest
{
    [Test]
    public void test_valid_kill_credit_snapshot_awards_xp_once_from_registry_snapshot()
    {
        var processor = CreateProcessor();
        processor.RegisterActiveSource(CreateLookupRow(), SourceLifecycleToken());

        var result = processor.ProcessKillCredit(CreateKillCreditEvent());

        Assert.That(result.Status, Is.EqualTo(XpAwardStatus.Awarded));
        Assert.That(result.XpAwarded, Is.EqualTo(150));
        Assert.That(processor.CurrentSaveState.total_xp, Is.EqualTo(150));
        Assert.That(result.Snapshot, Is.Not.Null);
        Assert.That(result.Snapshot!.lifecycle_state, Is.EqualTo(XpSourceLifecycleState.DefeatedTombstone));
        Assert.That(result.Snapshot.defeated_level, Is.EqualTo(5));
        Assert.That(result.Snapshot.encounter_role, Is.EqualTo(ProgressionEncounterRole.Trash));
    }

    [Test]
    public void test_duplicate_kill_credit_dedupes_by_stable_source_lifecycle_key()
    {
        var processor = CreateProcessor();
        processor.RegisterActiveSource(CreateLookupRow(), SourceLifecycleToken());

        var first = processor.ProcessKillCredit(CreateKillCreditEvent());
        var second = processor.ProcessKillCredit(CreateKillCreditEvent());

        Assert.That(first.Status, Is.EqualTo(XpAwardStatus.Awarded));
        Assert.That(second.Status, Is.EqualTo(XpAwardStatus.DuplicateRejected));
        Assert.That(second.XpAwarded, Is.EqualTo(0));
        Assert.That(processor.CurrentSaveState.total_xp, Is.EqualTo(150));
        Assert.That(second.DedupeKey, Is.Not.Null);
        Assert.That(second.DedupeKey!.local_character_id, Is.EqualTo(LocalCharacterId()));
        Assert.That(second.DedupeKey.zoneId, Is.EqualTo(ZoneId()));
        Assert.That(second.DedupeKey.defeated_source_ref, Is.EqualTo(SourceRef()));
        Assert.That(second.DedupeKey.source_lifecycle_token, Is.EqualTo(SourceLifecycleToken()));
    }

    [Test]
    public void test_missing_snapshot_rejects_xp_without_combat_fallback()
    {
        var processor = CreateProcessor();

        var result = processor.ProcessKillCredit(CreateKillCreditEvent());

        Assert.That(result.Status, Is.EqualTo(XpAwardStatus.RejectedMissingSnapshot));
        Assert.That(result.XpAwarded, Is.EqualTo(0));
        Assert.That(processor.CurrentSaveState.total_xp, Is.EqualTo(0));
        Assert.That(result.Diagnostic, Does.Contain("Missing XpAwardResolutionSnapshot"));
        Assert.That(processor.Diagnostics.Single(), Does.Contain("PlayerKillCreditEvent.defeated_source_ref"));
    }

    [Test]
    public void test_progression_save_barrier_drains_pending_kill_credit_before_stable_read_view()
    {
        var processor = CreateProcessor();
        processor.RegisterActiveSource(CreateLookupRow(), SourceLifecycleToken());
        processor.QueueKillCreditForBarrier(CreateKillCreditEvent());

        var result = processor.Resolve(CreateManualSaveRequest());

        Assert.That(result.status, Is.EqualTo(SaveStabilityBarrierStatus.Stable));
        Assert.That(result.barrier_name, Is.EqualTo(SaveBarrierNames.ProgressionSaveBarrier));
        Assert.That(processor.PendingKillCreditCount, Is.EqualTo(0));
        Assert.That(processor.CurrentSaveState.total_xp, Is.EqualTo(150));
        Assert.That(result.read_view, Is.TypeOf<CharacterProgressionSaveState>());
        Assert.That(((CharacterProgressionSaveState)result.read_view!).total_xp, Is.EqualTo(150));
    }

    [Test]
    public void test_processor_reads_only_approved_player_kill_credit_fields()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepoRoot(),
            "src",
            "gameplay",
            "progression",
            "CharacterProgressionKillCreditProcessor.cs"));
        var fields = Regex.Matches(source, @"killCreditEvent\.([A-Za-z_][A-Za-z0-9_]*)")
            .Cast<Match>()
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.That(fields, Is.EqualTo(new[]
        {
            "defeated_source_ref",
            "faction_id",
            "kill_weight_seed",
            "zoneId"
        }.OrderBy(name => name, StringComparer.Ordinal).ToArray()));
    }

    internal static CharacterProgressionKillCreditProcessor CreateProcessor()
    {
        return new CharacterProgressionKillCreditProcessor(
            LocalCharacterId(),
            new CharacterProgressionSaveState("cpro-save-t1", "Cleric", current_level: 5, total_xp: 0, "Tier1"),
            new XpAwardFormulaParameters(xp_per_defeated_level: 10, kill_weight_seed_tolerance: 0.000001d));
    }

    internal static ProgressionXpSourceLookupRow CreateLookupRow()
    {
        return new ProgressionXpSourceLookupRow(
            ZoneId(),
            SourceRef(),
            defeated_level: 5,
            ProgressionEncounterRole.Trash,
            encounter_role_multiplier: 2.0d,
            xp_weight_seed_t1: 1.5d,
            expected_kill_weight_seed_t1: KillWeightSeed(),
            XpRepeatabilityClass.Repeatable,
            ProgressionSourceLifecycleTokenPolicy.SpawnCycle,
            xp_eligible: true);
    }

    internal static PlayerKillCreditEvent CreateKillCreditEvent()
    {
        return new PlayerKillCreditEvent(SourceRef(), ZoneId(), "VampireCourt_T1", KillWeightSeed());
    }

    internal static SaveStabilityBarrierRequest CreateManualSaveRequest()
    {
        return new SaveStabilityBarrierRequest(
            save_request_id: 9001,
            SaveTriggerType.ManualSave,
            new[] { "CharacterProgressionSaveState", "NpcSourceLifecycleRecord" },
            caller_deadline_monotonic_ms: null,
            owner_budget_ms: 50,
            effective_deadline_monotonic_ms: 50);
    }

    internal static string LocalCharacterId()
    {
        return "local-character-1";
    }

    internal static string ZoneId()
    {
        return "Haunt_Prototype_T1";
    }

    internal static string SourceLifecycleToken()
    {
        return "spawn-cycle-token-1";
    }

    internal static double KillWeightSeed()
    {
        return 1.25d;
    }

    internal static CombatStableSourceRef SourceRef()
    {
        return CombatStableSourceRef.ForSpawn(new CombatSpawnSourceRef(
            "VampireCourt_T1",
            "solo-trash-anchor-1",
            "VampireThrall_T1"));
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
                    Directory.Exists(Path.Combine(directory.FullName, "src", "gameplay", "progression")))
                {
                    return directory.FullName;
                }
            }
        }

        throw new DirectoryNotFoundException("Unable to locate repository root for progression boundary scan.");
    }
}
