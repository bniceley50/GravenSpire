#nullable enable

using System.Collections.Generic;
using System.Linq;
using Gravenspire.Core.Save;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Npc;
using Gravenspire.Gameplay.Progression;
using Gravenspire.Tests.Unit.Gameplay.Progression;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Gameplay.Progression;

public sealed class ProgressionSaveBarrierKillCreditConsistencyTest
{
    [Test]
    public void test_same_frame_manual_save_invokes_progression_and_npc_barriers_after_kill_credit_acknowledgement()
    {
        var progression = CharacterProgressionKillCreditProcessorTest.CreateProcessor();
        progression.RegisterActiveSource(
            CharacterProgressionKillCreditProcessorTest.CreateLookupRow(),
            CharacterProgressionKillCreditProcessorTest.SourceLifecycleToken());
        var npc = CreateNpcSourceLifecycleService();
        var phase = new CombatKillResolutionPhase();
        var saveFailures = new List<SaveFailedEvent>();
        var writer = new RecordingSavePayloadWriter();
        var saveCoordinator = new GroupedSaveAttemptCoordinator(
            new ISaveStabilityBarrier[] { progression, npc },
            saveFailures.Add);

        var killResolution = phase.ResolveWithAcknowledgements(
            new CombatKillResolutionRequest(CreateDefeatedNpc(), CreatePlayer(), CharacterProgressionKillCreditProcessorTest.KillWeightSeed()),
            new ICombatKillCreditAcknowledgementSink[] { progression, npc });
        var saveResult = saveCoordinator.AttemptGroupedSave(
            CharacterProgressionKillCreditProcessorTest.CreateManualSaveRequest(),
            writer);

        Assert.That(killResolution.Resolution.KillCreditEvent, Is.Not.Null);
        Assert.That(killResolution.HoldStatus.IsAcknowledged, Is.True);
        Assert.That(saveResult.Status, Is.EqualTo(SaveAttemptStatus.Written));
        Assert.That(saveResult.BarrierResults.Select(result => result.barrier_name), Is.EquivalentTo(new[]
        {
            SaveBarrierNames.ProgressionSaveBarrier,
            SaveBarrierNames.NpcSourceLifecycleSaveBarrier
        }));
        Assert.That(saveFailures, Is.Empty);
        Assert.That(writer.WriteCount, Is.EqualTo(1));
        Assert.That(progression.CurrentSaveState.total_xp, Is.EqualTo(150));
        Assert.That(npc.BarrierInvocationCount, Is.EqualTo(1));
        Assert.That(npc.TryGetRecord(
            CharacterProgressionKillCreditProcessorTest.ZoneId(),
            CharacterProgressionKillCreditProcessorTest.SourceRef(),
            out var record), Is.True);
        Assert.That(record.source_lifecycle_state, Is.EqualTo(NpcSourceLifecycleState.Defeated));
    }

    [Test]
    public void test_unresolved_progression_barrier_rejects_same_frame_save_without_bytes()
    {
        var progression = CharacterProgressionKillCreditProcessorTest.CreateProcessor();
        progression.RegisterActiveSource(
            CharacterProgressionKillCreditProcessorTest.CreateLookupRow(),
            CharacterProgressionKillCreditProcessorTest.SourceLifecycleToken());
        progression.SetSaveBarrierHeld(true);
        progression.QueueKillCreditForBarrier(CharacterProgressionKillCreditProcessorTest.CreateKillCreditEvent());
        var npc = CreateNpcSourceLifecycleService();
        var saveFailures = new List<SaveFailedEvent>();
        var writer = new RecordingSavePayloadWriter();
        var saveCoordinator = new GroupedSaveAttemptCoordinator(
            new ISaveStabilityBarrier[] { progression, npc },
            saveFailures.Add);

        var saveResult = saveCoordinator.AttemptGroupedSave(
            CharacterProgressionKillCreditProcessorTest.CreateManualSaveRequest(),
            writer);

        Assert.That(saveResult.Status, Is.EqualTo(SaveAttemptStatus.Failed));
        Assert.That(saveFailures.Single().failure_class, Is.EqualTo(SaveFailureClass.DownstreamSaveBarrierUnresolved));
        Assert.That(saveFailures.Single().barrier_name, Is.EqualTo(SaveBarrierNames.ProgressionSaveBarrier));
        Assert.That(writer.WriteCount, Is.EqualTo(0));
        Assert.That(progression.CurrentSaveState.total_xp, Is.EqualTo(0));
        Assert.That(progression.PendingKillCreditCount, Is.EqualTo(1));
    }

    private static NpcSourceLifecycleService CreateNpcSourceLifecycleService()
    {
        var service = new NpcSourceLifecycleService();
        service.RegisterActiveSource(new NpcSourceLifecycleRecord(
            CharacterProgressionKillCreditProcessorTest.ZoneId(),
            CharacterProgressionKillCreditProcessorTest.SourceRef(),
            CharacterProgressionKillCreditProcessorTest.SourceLifecycleToken(),
            NpcSourceLifecycleState.Active,
            NpcSourceLifecycleTokenPolicy.SpawnCycle,
            "trash-respawn-window-t1"));
        return service;
    }

    private static CombatActorState CreatePlayer()
    {
        return new CombatActorState(
            "combat-player-1",
            CombatActorKind.Player,
            CombatStableSourceRef.ForPlayer(CharacterProgressionKillCreditProcessorTest.LocalCharacterId()),
            "PlayerLocal_T1",
            CharacterProgressionKillCreditProcessorTest.ZoneId(),
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

    private static CombatActorState CreateDefeatedNpc()
    {
        return new CombatActorState(
            "combat-hostile-1",
            CombatActorKind.NPC,
            CharacterProgressionKillCreditProcessorTest.SourceRef(),
            "VampireCourt_T1",
            CharacterProgressionKillCreditProcessorTest.ZoneId(),
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
            "combat-hostile-1-sort")
            .SetThreat("combat-player-1", 25);
    }

    private sealed class RecordingSavePayloadWriter : ISavePayloadWriter
    {
        public int WriteCount { get; private set; }

        public void Write(SaveStableReadBundle stableReadBundle)
        {
            Assert.That(stableReadBundle.BarrierResults, Is.Not.Empty);
            WriteCount++;
        }
    }
}
