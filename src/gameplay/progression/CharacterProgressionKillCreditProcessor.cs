#nullable enable

using System;
using System.Collections.Generic;
using Gravenspire.Core.Save;
using Gravenspire.Gameplay.Combat;

namespace Gravenspire.Gameplay.Progression;

public enum ProgressionEncounterRole
{
    Trash,
    Named,
    Camp
}

public enum XpRepeatabilityClass
{
    Repeatable,
    RespawnLockout
}

public enum ProgressionSourceLifecycleTokenPolicy
{
    PersistentNpcEpisode,
    SpawnCycle
}

public enum XpSourceLifecycleState
{
    Active,
    DefeatedTombstone
}

public enum XpAwardStatus
{
    Awarded,
    DuplicateRejected,
    RejectedMissingSnapshot,
    RejectedInvalidSnapshot,
    RejectedIneligible
}

public sealed record CharacterProgressionSaveState(
    string progression_schema_version,
    string class_id,
    int current_level,
    int total_xp,
    string spell_eligibility_tier);

public sealed record XpAwardFormulaParameters(
    int xp_per_defeated_level,
    double kill_weight_seed_tolerance);

public sealed record ProgressionXpSourceLookupRow(
    string zoneId,
    CombatStableSourceRef defeated_source_ref,
    int defeated_level,
    ProgressionEncounterRole encounter_role,
    double encounter_role_multiplier,
    double xp_weight_seed_t1,
    double expected_kill_weight_seed_t1,
    XpRepeatabilityClass repeatability_class,
    ProgressionSourceLifecycleTokenPolicy source_lifecycle_token_policy,
    bool xp_eligible);

public sealed record XpSourceLifecycleRegistryEntry(
    string zoneId,
    CombatStableSourceRef defeated_source_ref,
    string source_lifecycle_token,
    int defeated_level,
    ProgressionEncounterRole encounter_role,
    double encounter_role_multiplier,
    double xp_weight_seed_t1,
    double expected_kill_weight_seed_t1,
    XpRepeatabilityClass repeatability_class,
    ProgressionSourceLifecycleTokenPolicy source_lifecycle_token_policy,
    bool xp_eligible,
    XpSourceLifecycleState lifecycle_state);

public sealed record XpAwardResolutionSnapshot(
    string zoneId,
    CombatStableSourceRef defeated_source_ref,
    string source_lifecycle_token,
    int defeated_level,
    ProgressionEncounterRole encounter_role,
    double encounter_role_multiplier,
    double xp_weight_seed_t1,
    double expected_kill_weight_seed_t1,
    XpRepeatabilityClass repeatability_class,
    bool xp_eligible,
    XpSourceLifecycleState lifecycle_state);

public sealed record XpAwardDedupeKey(
    string local_character_id,
    string zoneId,
    CombatStableSourceRef defeated_source_ref,
    string source_lifecycle_token);

public sealed record XpAwardResult(
    XpAwardStatus Status,
    int XpAwarded,
    CharacterProgressionSaveState SaveState,
    XpAwardDedupeKey? DedupeKey,
    XpAwardResolutionSnapshot? Snapshot,
    string? Diagnostic);

public sealed class CharacterProgressionKillCreditProcessor :
    ICombatKillCreditAcknowledgementSink,
    ISaveStabilityBarrier
{
    private readonly string localCharacterId;
    private readonly XpAwardFormulaParameters formulaParameters;
    private readonly Dictionary<ProgressionSourceKey, XpSourceLifecycleRegistryEntry> registry = new();
    private readonly HashSet<XpAwardDedupeKey> processedAwardKeys = new();
    private readonly Queue<PlayerKillCreditEvent> pendingKillCredits = new();
    private readonly List<string> diagnostics = new();
    private bool holdSaveBarrier;
    private CharacterProgressionSaveState saveState;

    public CharacterProgressionKillCreditProcessor(
        string localCharacterId,
        CharacterProgressionSaveState initialSaveState,
        XpAwardFormulaParameters formulaParameters)
    {
        if (string.IsNullOrWhiteSpace(localCharacterId))
        {
            throw new ArgumentException("local_character_id is required.", nameof(localCharacterId));
        }

        ArgumentNullException.ThrowIfNull(initialSaveState);
        ArgumentNullException.ThrowIfNull(formulaParameters);

        if (formulaParameters.xp_per_defeated_level <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(formulaParameters), "xp_per_defeated_level must be positive injected tuning.");
        }

        if (formulaParameters.kill_weight_seed_tolerance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(formulaParameters), "kill_weight_seed_tolerance must not be negative.");
        }

        this.localCharacterId = localCharacterId;
        saveState = initialSaveState;
        this.formulaParameters = formulaParameters;
    }

    public string ConsumerName => "CharacterProgressionAwardSnapshot";

    public string OwnerSystem => "Character Progression";

    public string BarrierName => SaveBarrierNames.ProgressionSaveBarrier;

    public string BarrierGroupId => SaveBarrierGroups.XpSourceLifecycleConsistency;

    public CharacterProgressionSaveState CurrentSaveState => saveState;

    public IReadOnlyList<string> Diagnostics => diagnostics;

    public int PendingKillCreditCount => pendingKillCredits.Count;

    public void RegisterActiveSource(ProgressionXpSourceLookupRow lookupRow, string sourceLifecycleToken)
    {
        ArgumentNullException.ThrowIfNull(lookupRow);

        if (string.IsNullOrWhiteSpace(sourceLifecycleToken))
        {
            throw new ArgumentException("source_lifecycle_token is required.", nameof(sourceLifecycleToken));
        }

        var entry = new XpSourceLifecycleRegistryEntry(
            lookupRow.zoneId,
            lookupRow.defeated_source_ref,
            sourceLifecycleToken,
            lookupRow.defeated_level,
            lookupRow.encounter_role,
            lookupRow.encounter_role_multiplier,
            lookupRow.xp_weight_seed_t1,
            lookupRow.expected_kill_weight_seed_t1,
            lookupRow.repeatability_class,
            lookupRow.source_lifecycle_token_policy,
            lookupRow.xp_eligible,
            XpSourceLifecycleState.Active);

        registry[ProgressionSourceKey.From(lookupRow.zoneId, lookupRow.defeated_source_ref)] = entry;
    }

    public CombatKillCreditAcknowledgement Acknowledge(PlayerKillCreditEvent killCreditEvent)
    {
        ArgumentNullException.ThrowIfNull(killCreditEvent);

        if (holdSaveBarrier)
        {
            pendingKillCredits.Enqueue(killCreditEvent);
            return CombatKillCreditAcknowledgement.Pending(ConsumerName, "ProgressionSaveBarrier is held by test latch.");
        }

        var result = ProcessKillCredit(killCreditEvent);
        return result.Status == XpAwardStatus.Awarded || result.Status == XpAwardStatus.DuplicateRejected
            ? CombatKillCreditAcknowledgement.Acknowledged(ConsumerName, result.Diagnostic)
            : CombatKillCreditAcknowledgement.Rejected(ConsumerName, result.Diagnostic ?? "Kill credit rejected by Character Progression.");
    }

    public void QueueKillCreditForBarrier(PlayerKillCreditEvent killCreditEvent)
    {
        ArgumentNullException.ThrowIfNull(killCreditEvent);
        pendingKillCredits.Enqueue(killCreditEvent);
    }

    public void SetSaveBarrierHeld(bool isHeld)
    {
        holdSaveBarrier = isHeld;
    }

    public SaveStabilityBarrierResult Resolve(SaveStabilityBarrierRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (holdSaveBarrier)
        {
            return SaveStabilityBarrierResult.Unresolved(
                OwnerSystem,
                BarrierName,
                request.save_request_id,
                SaveStabilityBarrierReasonCode.TransactionPending,
                "Character Progression kill-credit dispatch has not acknowledged.");
        }

        DrainPendingKillCredits();

        return SaveStabilityBarrierResult.Stable(
            OwnerSystem,
            BarrierName,
            request.save_request_id,
            readToken: $"{BarrierName}:{request.save_request_id}",
            ownerStateRevision: saveState.total_xp.ToString(System.Globalization.CultureInfo.InvariantCulture),
            readView: saveState);
    }

    public XpAwardResult ProcessKillCredit(PlayerKillCreditEvent killCreditEvent)
    {
        ArgumentNullException.ThrowIfNull(killCreditEvent);

        var defeatedSourceRef = killCreditEvent.defeated_source_ref;
        var zoneId = killCreditEvent.zoneId;
        var factionId = killCreditEvent.faction_id;
        var killWeightSeed = killCreditEvent.kill_weight_seed;

        if (!TryCaptureSnapshot(zoneId, defeatedSourceRef, killWeightSeed, factionId, out var snapshot, out var diagnostic))
        {
            diagnostics.Add(diagnostic);
            return new XpAwardResult(
                XpAwardStatus.RejectedMissingSnapshot,
                XpAwarded: 0,
                saveState,
                DedupeKey: null,
                Snapshot: null,
                Diagnostic: diagnostic);
        }

        var dedupeKey = new XpAwardDedupeKey(
            localCharacterId,
            snapshot.zoneId,
            snapshot.defeated_source_ref,
            snapshot.source_lifecycle_token);

        if (processedAwardKeys.Contains(dedupeKey))
        {
            var duplicateDiagnostic = "Duplicate PlayerKillCreditEvent rejected by XpAwardDedupeKey.";
            diagnostics.Add(duplicateDiagnostic);
            return new XpAwardResult(
                XpAwardStatus.DuplicateRejected,
                XpAwarded: 0,
                saveState,
                dedupeKey,
                snapshot,
                duplicateDiagnostic);
        }

        if (!snapshot.xp_eligible)
        {
            var ineligibleDiagnostic = "XP source snapshot is explicitly ineligible.";
            diagnostics.Add(ineligibleDiagnostic);
            processedAwardKeys.Add(dedupeKey);
            return new XpAwardResult(
                XpAwardStatus.RejectedIneligible,
                XpAwarded: 0,
                saveState,
                dedupeKey,
                snapshot,
                ineligibleDiagnostic);
        }

        if (!KillWeightSeedMatches(snapshot, killWeightSeed))
        {
            var invalidDiagnostic = "PlayerKillCreditEvent.kill_weight_seed does not match progression lookup expectation.";
            diagnostics.Add(invalidDiagnostic);
            return new XpAwardResult(
                XpAwardStatus.RejectedInvalidSnapshot,
                XpAwarded: 0,
                saveState,
                dedupeKey,
                snapshot,
                invalidDiagnostic);
        }

        var xpAwarded = CalculateXpAward(snapshot);
        saveState = saveState with { total_xp = checked(saveState.total_xp + xpAwarded) };
        processedAwardKeys.Add(dedupeKey);

        return new XpAwardResult(
            XpAwardStatus.Awarded,
            xpAwarded,
            saveState,
            dedupeKey,
            snapshot,
            null);
    }

    private void DrainPendingKillCredits()
    {
        while (pendingKillCredits.Count > 0)
        {
            ProcessKillCredit(pendingKillCredits.Dequeue());
        }
    }

    private bool TryCaptureSnapshot(
        string zoneId,
        CombatStableSourceRef defeatedSourceRef,
        double killWeightSeed,
        string? factionId,
        out XpAwardResolutionSnapshot snapshot,
        out string diagnostic)
    {
        var key = ProgressionSourceKey.From(zoneId, defeatedSourceRef);
        if (!registry.TryGetValue(key, out var entry))
        {
            snapshot = EmptySnapshot();
            diagnostic = "Missing XpAwardResolutionSnapshot for PlayerKillCreditEvent.defeated_source_ref.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(zoneId) ||
            string.IsNullOrWhiteSpace(entry.source_lifecycle_token) ||
            entry.defeated_level <= 0 ||
            entry.encounter_role_multiplier <= 0 ||
            entry.xp_weight_seed_t1 <= 0 ||
            double.IsNaN(killWeightSeed) ||
            double.IsInfinity(killWeightSeed))
        {
            snapshot = EmptySnapshot();
            diagnostic = "Invalid XpAwardResolutionSnapshot required field.";
            return false;
        }

        _ = factionId;

        var tombstone = entry with { lifecycle_state = XpSourceLifecycleState.DefeatedTombstone };
        registry[key] = tombstone;
        snapshot = new XpAwardResolutionSnapshot(
            tombstone.zoneId,
            tombstone.defeated_source_ref,
            tombstone.source_lifecycle_token,
            tombstone.defeated_level,
            tombstone.encounter_role,
            tombstone.encounter_role_multiplier,
            tombstone.xp_weight_seed_t1,
            tombstone.expected_kill_weight_seed_t1,
            tombstone.repeatability_class,
            tombstone.xp_eligible,
            tombstone.lifecycle_state);
        diagnostic = string.Empty;
        return true;
    }

    private bool KillWeightSeedMatches(XpAwardResolutionSnapshot snapshot, double killWeightSeed)
    {
        return Math.Abs(snapshot.expected_kill_weight_seed_t1 - killWeightSeed) <= formulaParameters.kill_weight_seed_tolerance;
    }

    private int CalculateXpAward(XpAwardResolutionSnapshot snapshot)
    {
        var rawAward = snapshot.defeated_level *
            formulaParameters.xp_per_defeated_level *
            snapshot.encounter_role_multiplier *
            snapshot.xp_weight_seed_t1;

        return checked((int)Math.Round(rawAward, MidpointRounding.AwayFromZero));
    }

    private static XpAwardResolutionSnapshot EmptySnapshot()
    {
        return new XpAwardResolutionSnapshot(
            string.Empty,
            CombatStableSourceRef.ForPersistentNpc("empty-snapshot"),
            string.Empty,
            defeated_level: 0,
            ProgressionEncounterRole.Trash,
            encounter_role_multiplier: 0,
            xp_weight_seed_t1: 0,
            expected_kill_weight_seed_t1: 0,
            XpRepeatabilityClass.Repeatable,
            xp_eligible: false,
            XpSourceLifecycleState.DefeatedTombstone);
    }

    private sealed record ProgressionSourceKey(string zoneId, CombatStableSourceRef defeated_source_ref)
    {
        public static ProgressionSourceKey From(string zoneId, CombatStableSourceRef defeatedSourceRef)
        {
            return new ProgressionSourceKey(zoneId, defeatedSourceRef);
        }
    }
}
