#nullable enable

using System.Collections.Generic;
using System.Linq;
using Gravenspire.Core.Save;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Core.Save;

public sealed class SaveGroupedBarrierConsistencyTest
{
    [Test]
    public void test_one_stable_and_one_unresolved_group_member_fails_whole_save_attempt()
    {
        var stable = new TestSaveBarrier("Character Progression", SaveBarrierNames.ProgressionSaveBarrier, SaveStabilityBarrierStatus.Stable);
        var unresolved = new TestSaveBarrier("NPC System", SaveBarrierNames.NpcSourceLifecycleSaveBarrier, SaveStabilityBarrierStatus.Unresolved);
        var failures = new List<SaveFailedEvent>();
        var writer = new RecordingSavePayloadWriter();
        var coordinator = new GroupedSaveAttemptCoordinator(new ISaveStabilityBarrier[] { stable, unresolved }, failures.Add);

        var result = coordinator.AttemptGroupedSave(CreateRequest(), writer);

        Assert.That(result.Status, Is.EqualTo(SaveAttemptStatus.Failed));
        Assert.That(result.BarrierResults.Count, Is.EqualTo(2));
        Assert.That(stable.InvocationCount, Is.EqualTo(1));
        Assert.That(unresolved.InvocationCount, Is.EqualTo(1));
        Assert.That(failures.Single().failure_class, Is.EqualTo(SaveFailureClass.DownstreamSaveBarrierUnresolved));
        Assert.That(failures.Single().barrier_name, Is.EqualTo(SaveBarrierNames.NpcSourceLifecycleSaveBarrier));
        Assert.That(writer.WriteCount, Is.EqualTo(0));
    }

    [Test]
    public void test_grouped_barrier_writer_is_not_called_when_any_member_is_unresolved()
    {
        var progression = new TestSaveBarrier("Character Progression", SaveBarrierNames.ProgressionSaveBarrier, SaveStabilityBarrierStatus.Unresolved);
        var npc = new TestSaveBarrier("NPC System", SaveBarrierNames.NpcSourceLifecycleSaveBarrier, SaveStabilityBarrierStatus.Stable);
        var failures = new List<SaveFailedEvent>();
        var writer = new RecordingSavePayloadWriter();
        var coordinator = new GroupedSaveAttemptCoordinator(new ISaveStabilityBarrier[] { progression, npc }, failures.Add);

        var result = coordinator.AttemptGroupedSave(CreateRequest(), writer);

        Assert.That(result.Status, Is.EqualTo(SaveAttemptStatus.Failed));
        Assert.That(result.FailureEvent, Is.Not.Null);
        Assert.That(result.FailureEvent!.reason_code, Is.EqualTo(SaveStabilityBarrierReasonCode.TransactionPending));
        Assert.That(failures.Count, Is.EqualTo(1));
        Assert.That(writer.WriteCount, Is.EqualTo(0));
        Assert.That(result.BarrierResults.Select(barrier => barrier.status), Has.Member(SaveStabilityBarrierStatus.Stable));
        Assert.That(result.BarrierResults.Select(barrier => barrier.status), Has.Member(SaveStabilityBarrierStatus.Unresolved));
    }

    private static SaveStabilityBarrierRequest CreateRequest()
    {
        return new SaveStabilityBarrierRequest(
            save_request_id: 42,
            SaveTriggerType.ManualSave,
            new[] { "CharacterProgressionSaveState", "NpcSourceLifecycleRecord" },
            caller_deadline_monotonic_ms: null,
            owner_budget_ms: 50,
            effective_deadline_monotonic_ms: 50);
    }

    private sealed class TestSaveBarrier : ISaveStabilityBarrier
    {
        private readonly SaveStabilityBarrierStatus status;

        public TestSaveBarrier(string ownerSystem, string barrierName, SaveStabilityBarrierStatus status)
        {
            OwnerSystem = ownerSystem;
            BarrierName = barrierName;
            this.status = status;
        }

        public string OwnerSystem { get; }

        public string BarrierName { get; }

        public string BarrierGroupId => SaveBarrierGroups.XpSourceLifecycleConsistency;

        public int InvocationCount { get; private set; }

        public SaveStabilityBarrierResult Resolve(SaveStabilityBarrierRequest request)
        {
            InvocationCount++;
            return status == SaveStabilityBarrierStatus.Stable
                ? SaveStabilityBarrierResult.Stable(
                    OwnerSystem,
                    BarrierName,
                    request.save_request_id,
                    $"{BarrierName}:{request.save_request_id}",
                    InvocationCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    new object())
                : SaveStabilityBarrierResult.Unresolved(
                    OwnerSystem,
                    BarrierName,
                    request.save_request_id,
                    SaveStabilityBarrierReasonCode.TransactionPending,
                    "test barrier held");
        }
    }

    private sealed class RecordingSavePayloadWriter : ISavePayloadWriter
    {
        public int WriteCount { get; private set; }

        public void Write(SaveStableReadBundle stableReadBundle)
        {
            WriteCount++;
        }
    }
}
