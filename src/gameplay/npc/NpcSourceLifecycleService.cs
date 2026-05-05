#nullable enable

using System;
using System.Collections.Generic;
using Gravenspire.Core.Save;
using Gravenspire.Gameplay.Combat;

namespace Gravenspire.Gameplay.Npc;

public enum NpcSourceLifecycleState
{
    Active,
    Defeated,
    RespawnEligible
}

public enum NpcSourceLifecycleTokenPolicy
{
    PersistentNpcEpisode,
    SpawnCycle
}

public sealed record NpcSourceLifecycleRecord(
    string zoneId,
    CombatStableSourceRef defeated_source_ref,
    string source_lifecycle_token,
    NpcSourceLifecycleState source_lifecycle_state,
    NpcSourceLifecycleTokenPolicy source_lifecycle_token_policy,
    string respawn_or_availability_timing_key);

public sealed class NpcSourceLifecycleService :
    ICombatKillCreditAcknowledgementSink,
    ISaveStabilityBarrier
{
    private readonly Dictionary<NpcSourceKey, NpcSourceLifecycleRecord> records = new();
    private readonly Queue<PlayerKillCreditEvent> pendingDefeats = new();
    private bool holdSaveBarrier;

    public string ConsumerName => "NpcSourceLifecycle";

    public string OwnerSystem => "NPC System";

    public string BarrierName => SaveBarrierNames.NpcSourceLifecycleSaveBarrier;

    public string BarrierGroupId => SaveBarrierGroups.XpSourceLifecycleConsistency;

    public int BarrierInvocationCount { get; private set; }

    public int PendingDefeatCount => pendingDefeats.Count;

    public void RegisterActiveSource(NpcSourceLifecycleRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (record.source_lifecycle_state != NpcSourceLifecycleState.Active)
        {
            throw new ArgumentException("NPC source lifecycle registration requires an Active record.", nameof(record));
        }

        if (string.IsNullOrWhiteSpace(record.source_lifecycle_token))
        {
            throw new ArgumentException("source_lifecycle_token is required.", nameof(record));
        }

        records[NpcSourceKey.From(record.zoneId, record.defeated_source_ref)] = record;
    }

    public CombatKillCreditAcknowledgement Acknowledge(PlayerKillCreditEvent killCreditEvent)
    {
        ArgumentNullException.ThrowIfNull(killCreditEvent);

        if (holdSaveBarrier)
        {
            pendingDefeats.Enqueue(killCreditEvent);
            return CombatKillCreditAcknowledgement.Pending(ConsumerName, "NpcSourceLifecycleSaveBarrier is held by test latch.");
        }

        var recorded = RecordDefeat(killCreditEvent);
        return recorded
            ? CombatKillCreditAcknowledgement.Acknowledged(ConsumerName)
            : CombatKillCreditAcknowledgement.Rejected(ConsumerName, "NPC source lifecycle record missing for defeated source.");
    }

    public void QueueDefeatForBarrier(PlayerKillCreditEvent killCreditEvent)
    {
        ArgumentNullException.ThrowIfNull(killCreditEvent);
        pendingDefeats.Enqueue(killCreditEvent);
    }

    public void SetSaveBarrierHeld(bool isHeld)
    {
        holdSaveBarrier = isHeld;
    }

    public bool TryGetRecord(string zoneId, CombatStableSourceRef defeatedSourceRef, out NpcSourceLifecycleRecord record)
    {
        return records.TryGetValue(NpcSourceKey.From(zoneId, defeatedSourceRef), out record!);
    }

    public SaveStabilityBarrierResult Resolve(SaveStabilityBarrierRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        BarrierInvocationCount++;

        if (holdSaveBarrier)
        {
            return SaveStabilityBarrierResult.Unresolved(
                OwnerSystem,
                BarrierName,
                request.save_request_id,
                SaveStabilityBarrierReasonCode.TransactionPending,
                "NPC source lifecycle defeat outcome has not acknowledged.");
        }

        DrainPendingDefeats();

        return SaveStabilityBarrierResult.Stable(
            OwnerSystem,
            BarrierName,
            request.save_request_id,
            readToken: $"{BarrierName}:{request.save_request_id}",
            ownerStateRevision: records.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            readView: new List<NpcSourceLifecycleRecord>(records.Values));
    }

    private void DrainPendingDefeats()
    {
        while (pendingDefeats.Count > 0)
        {
            RecordDefeat(pendingDefeats.Dequeue());
        }
    }

    private bool RecordDefeat(PlayerKillCreditEvent killCreditEvent)
    {
        var key = NpcSourceKey.From(killCreditEvent.zoneId, killCreditEvent.defeated_source_ref);
        if (!records.TryGetValue(key, out var record))
        {
            return false;
        }

        records[key] = record with { source_lifecycle_state = NpcSourceLifecycleState.Defeated };
        return true;
    }

    private sealed record NpcSourceKey(string zoneId, CombatStableSourceRef defeated_source_ref)
    {
        public static NpcSourceKey From(string zoneId, CombatStableSourceRef defeatedSourceRef)
        {
            return new NpcSourceKey(zoneId, defeatedSourceRef);
        }
    }
}
