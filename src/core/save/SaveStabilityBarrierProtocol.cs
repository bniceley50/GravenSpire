#nullable enable

using System;
using System.Collections.Generic;

namespace Gravenspire.Core.Save;

public static class SaveBarrierNames
{
    public const string ProgressionSaveBarrier = "ProgressionSaveBarrier";
    public const string NpcSourceLifecycleSaveBarrier = "NpcSourceLifecycleSaveBarrier";
}

public static class SaveBarrierGroups
{
    public const string XpSourceLifecycleConsistency = "xp_source_lifecycle_consistency";
}

public enum SaveTriggerType
{
    TransitionSave,
    ManualSave,
    AutosaveTick,
    SessionExitSave
}

public enum SaveStabilityBarrierStatus
{
    Stable,
    Unresolved,
    Failed
}

public enum SaveStabilityBarrierReasonCode
{
    None,
    TransactionPending,
    DeadlineExceeded,
    ValidationFailed,
    OwnerUnavailable,
    Unknown
}

public enum SaveFailureClass
{
    DownstreamSaveBarrierUnresolved
}

public sealed record SaveStabilityBarrierRequest(
    long save_request_id,
    SaveTriggerType trigger_type,
    IReadOnlyList<string> requested_payloads,
    long? caller_deadline_monotonic_ms,
    int owner_budget_ms,
    long effective_deadline_monotonic_ms)
{
    public bool HasEffectiveDeadline => owner_budget_ms > 0 && effective_deadline_monotonic_ms >= 0;
}

public sealed record SaveStabilityBarrierResult(
    SaveStabilityBarrierStatus status,
    string owner_system,
    string barrier_name,
    long save_request_id,
    string? read_token,
    string? owner_state_revision,
    SaveStabilityBarrierReasonCode reason_code,
    string? diagnostics,
    object? read_view)
{
    public static SaveStabilityBarrierResult Stable(
        string ownerSystem,
        string barrierName,
        long saveRequestId,
        string readToken,
        string ownerStateRevision,
        object readView)
    {
        return new SaveStabilityBarrierResult(
            SaveStabilityBarrierStatus.Stable,
            ownerSystem,
            barrierName,
            saveRequestId,
            readToken,
            ownerStateRevision,
            SaveStabilityBarrierReasonCode.None,
            diagnostics: null,
            readView);
    }

    public static SaveStabilityBarrierResult Unresolved(
        string ownerSystem,
        string barrierName,
        long saveRequestId,
        SaveStabilityBarrierReasonCode reasonCode,
        string diagnostics)
    {
        return new SaveStabilityBarrierResult(
            SaveStabilityBarrierStatus.Unresolved,
            ownerSystem,
            barrierName,
            saveRequestId,
            read_token: null,
            owner_state_revision: null,
            reasonCode,
            diagnostics,
            read_view: null);
    }

    public static SaveStabilityBarrierResult Failed(
        string ownerSystem,
        string barrierName,
        long saveRequestId,
        SaveStabilityBarrierReasonCode reasonCode,
        string diagnostics)
    {
        return new SaveStabilityBarrierResult(
            SaveStabilityBarrierStatus.Failed,
            ownerSystem,
            barrierName,
            saveRequestId,
            read_token: null,
            owner_state_revision: null,
            reasonCode,
            diagnostics,
            read_view: null);
    }
}

public sealed record SaveFailedEvent(
    long save_request_id,
    SaveFailureClass failure_class,
    SaveStabilityBarrierReasonCode reason_code,
    string owner_system,
    string barrier_name,
    string diagnostics);

public interface ISaveStabilityBarrier
{
    string OwnerSystem { get; }

    string BarrierName { get; }

    string BarrierGroupId { get; }

    SaveStabilityBarrierResult Resolve(SaveStabilityBarrierRequest request);
}
