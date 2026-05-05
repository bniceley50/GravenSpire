#nullable enable

using System;
using System.Collections.Generic;

namespace Gravenspire.Core.Save;

public enum SaveAttemptStatus
{
    Written,
    Failed
}

public sealed record SaveStableReadBundle(IReadOnlyList<SaveStabilityBarrierResult> BarrierResults);

public interface ISavePayloadWriter
{
    void Write(SaveStableReadBundle stableReadBundle);
}

public sealed record SaveAttemptResult(
    SaveAttemptStatus Status,
    IReadOnlyList<SaveStabilityBarrierResult> BarrierResults,
    SaveFailedEvent? FailureEvent);

public sealed class GroupedSaveAttemptCoordinator
{
    private readonly IReadOnlyList<ISaveStabilityBarrier> barriers;
    private readonly Action<SaveFailedEvent> saveFailedEmitter;

    public GroupedSaveAttemptCoordinator(
        IReadOnlyList<ISaveStabilityBarrier> barriers,
        Action<SaveFailedEvent> saveFailedEmitter)
    {
        this.barriers = barriers ?? throw new ArgumentNullException(nameof(barriers));
        this.saveFailedEmitter = saveFailedEmitter ?? throw new ArgumentNullException(nameof(saveFailedEmitter));
    }

    public SaveAttemptResult AttemptGroupedSave(SaveStabilityBarrierRequest request, ISavePayloadWriter writer)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(writer);

        if (!request.HasEffectiveDeadline)
        {
            throw new InvalidOperationException("Save stability barriers require an effective monotonic deadline.");
        }

        var results = new List<SaveStabilityBarrierResult>();
        foreach (var barrier in barriers)
        {
            ArgumentNullException.ThrowIfNull(barrier);
            results.Add(barrier.Resolve(request));
        }

        foreach (var result in results)
        {
            if (result.status == SaveStabilityBarrierStatus.Stable)
            {
                continue;
            }

            var failureEvent = new SaveFailedEvent(
                request.save_request_id,
                SaveFailureClass.DownstreamSaveBarrierUnresolved,
                result.reason_code,
                result.owner_system,
                result.barrier_name,
                result.diagnostics ?? "Grouped downstream save barrier did not return Stable.");

            saveFailedEmitter(failureEvent);
            return new SaveAttemptResult(SaveAttemptStatus.Failed, results, failureEvent);
        }

        writer.Write(new SaveStableReadBundle(results));
        return new SaveAttemptResult(SaveAttemptStatus.Written, results, FailureEvent: null);
    }
}
