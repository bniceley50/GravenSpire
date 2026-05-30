#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Gravenspire.Gameplay.Npc.M3Objective;
using Gravenspire.UnityRuntime.Interaction;
using Gravenspire.UnityRuntime.Npc;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    [InitializeOnLoad]
    public static class GravenspireS3PlayerRelicRecoveryAndLootingVerificationRunner
    {
        private const string StoryId = "S3-03";
        private const string StorySlug = "s3-03-player-relic-recovery-and-looting";
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string RunKey = "GravenspireS3PlayerRelicRecoveryAndLooting.Run";
        private const string PhaseKey = "GravenspireS3PlayerRelicRecoveryAndLooting.Phase";
        private const string ChecksKey = "GravenspireS3PlayerRelicRecoveryAndLooting.Checks";
        private const string ErrorsKey = "GravenspireS3PlayerRelicRecoveryAndLooting.Errors";
        private const string WarningsKey = "GravenspireS3PlayerRelicRecoveryAndLooting.Warnings";
        private const string TelemetryKey = "GravenspireS3PlayerRelicRecoveryAndLooting.Telemetry";
        private const string RegressionChecksKey = "GravenspireS3PlayerRelicRecoveryAndLooting.RegressionChecks";
        private const string RegressionTelemetryKey = "GravenspireS3PlayerRelicRecoveryAndLooting.RegressionTelemetry";
        private const string EvidencePathKey = "GravenspireS3PlayerRelicRecoveryAndLooting.EvidencePath";
        private const string RegressionEvidencePathKey = "GravenspireS3PlayerRelicRecoveryAndLooting.RegressionEvidencePath";
        private const string PlayStartedKey = "GravenspireS3PlayerRelicRecoveryAndLooting.PlayStartedSeconds";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const string RegressionEvidencePathArgumentName = "-gravenspireRegressionEvidencePath";
        private const double SmokeDelaySeconds = 1.0d;
        private const float InRangeOffsetMeters = 1.25f;

        static GravenspireS3PlayerRelicRecoveryAndLootingVerificationRunner()
        {
            if (!SessionState.GetBool(RunKey, false))
            {
                return;
            }

            Application.logMessageReceived -= CaptureLog;
            Application.logMessageReceived += CaptureLog;
            EditorApplication.update -= ContinueAfterDomainReload;
            EditorApplication.update += ContinueAfterDomainReload;
        }

        [MenuItem("Gravenspire/Verify S3 Player Relic Recovery + Looting")]
        public static void Run()
        {
            ClearSession();
            SessionState.SetBool(RunKey, true);
            SessionState.SetString(PhaseKey, "initial");
            Application.logMessageReceived -= CaptureLog;
            Application.logMessageReceived += CaptureLog;

            try
            {
                var evidencePath = ResolveEvidencePathFromCommandLine(EvidencePathArgumentName, DefaultEvidencePath());
                var regressionEvidencePath = ResolveEvidencePathFromCommandLine(
                    RegressionEvidencePathArgumentName,
                    DefaultRegressionEvidencePath());
                SessionState.SetString(EvidencePathKey, evidencePath);
                SessionState.SetString(RegressionEvidencePathKey, regressionEvidencePath);
                Directory.CreateDirectory(Path.GetDirectoryName(evidencePath) ?? ".");
                Directory.CreateDirectory(Path.GetDirectoryName(regressionEvidencePath) ?? ".");

                GravenspireS3PlayerRelicRecoveryAndLootingBuilder.Build();
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                RecordCheck("scene_loaded", scene.IsValid() && scene.path == ScenePath);
                RecordCheck("harness_root_exists", FindSceneObjectIncludingInactive(S3PlayerInteractionHarness.HarnessRootName) != null);
                RecordCheck("m3_caretaker_anchor_exists", FindSceneObjectIncludingInactive(M3NamedNpcObjectiveFrame.AnchorObjectName) != null);
                RecordCheck("m3_objective_root_exists", FindSceneObjectIncludingInactive("M3_ObjectiveStateRoot") != null);
                RecordCheck("m3_objective_relic_exists", FindSceneObjectIncludingInactive(M3ObjectiveStateRelicHandInSession.RelicObjectName) != null);
                RecordCheck("m3_court_vendor_exists", FindSceneObjectIncludingInactive(M3LootTableFixedProfileVendorData.VendorObjectName) != null);
                RecordCheck("npc_adapter_present_on_caretaker", FindCaretakerAdapter() != null);
                RecordCheck("relic_adapter_present_on_relic", FindRelicAdapterIncludingInactive() != null);
                RecordCheck("no_dialogue_or_route_ui_scene_objects", HasNoForbiddenSceneAffordances());

                SessionState.SetString(PhaseKey, "entering_play");
                EditorApplication.update -= ContinueAfterDomainReload;
                EditorApplication.update += ContinueAfterDomainReload;
                EditorApplication.isPlaying = true;
            }
            catch (Exception ex)
            {
                AppendSessionLine(ErrorsKey, ex.ToString());
                WriteEvidenceAndExit(1);
            }
        }

        private static void ContinueAfterDomainReload()
        {
            if (!SessionState.GetBool(RunKey, false))
            {
                EditorApplication.update -= ContinueAfterDomainReload;
                Application.logMessageReceived -= CaptureLog;
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                return;
            }

            var phase = SessionState.GetString(PhaseKey, string.Empty);
            if (phase == "entering_play")
            {
                SessionState.SetString(PhaseKey, "playing");
                SessionState.SetString(PlayStartedKey, EditorApplication.timeSinceStartup.ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (phase != "playing")
            {
                return;
            }

            if (!double.TryParse(
                    SessionState.GetString(PlayStartedKey, "0"),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var startedSeconds) ||
                EditorApplication.timeSinceStartup - startedSeconds < SmokeDelaySeconds)
            {
                return;
            }

            try
            {
                RunSmokeChecks();
                WriteEvidenceAndExit(AllChecksPassed() && GetSessionLines(ErrorsKey).Count == 0 ? 0 : 1);
            }
            catch (Exception ex)
            {
                AppendSessionLine(ErrorsKey, ex.ToString());
                WriteEvidenceAndExit(1);
            }
        }

        private static void RunSmokeChecks()
        {
            var sceneObjects = RequiredSceneObjects();
            if (!sceneObjects.Valid)
            {
                AppendSessionLine(ErrorsKey, "Required S3-03 scene component was missing in Play Mode.");
                return;
            }

            ConfigureFreshSession(sceneObjects);
            RecordCheck("npc_adapter_frame_reference_resolves", sceneObjects.NpcAdapter.ObjectiveFrame == sceneObjects.Frame);
            RecordCheck("npc_adapter_state_reference_resolves", sceneObjects.NpcAdapter.ObjectiveState == sceneObjects.Objective);
            RecordCheck("relic_adapter_state_reference_resolves", sceneObjects.RelicAdapter.ObjectiveState == sceneObjects.Objective);
            RecordCheck("relic_adapter_vendor_reference_resolves", sceneObjects.RelicAdapter.LootVendor == sceneObjects.Vendor);
            RecordCheck("fresh_state_not_introduced", sceneObjects.Objective.State == M3ObjectiveState.NotIntroduced);
            RecordCheck("fresh_relic_inactive", !sceneObjects.RelicObject.activeSelf);

            RunAcceptDispatch(sceneObjects, "t1_accept");
            RunAcceptedRetalkRegression(sceneObjects);
            RunRelicBlockedDispatch(sceneObjects);
            RunRelicPartialSuccessDispatch(sceneObjects);
            RunRelicSuccessAndHandInDispatches(sceneObjects);
            RunCompleteRetalk(sceneObjects);
            RunEndToEndDispatch(sceneObjects);

            RecordCheck("no_dialogue_or_route_ui_after_interactions", HasNoForbiddenSceneAffordances());
            RecordCheck("feedback_rule_forbidden_text_absent", !ContainsForbiddenFeedbackText(sceneObjects.Harness.LastFeedbackText));
        }

        private static void RunAcceptDispatch(SceneObjects sceneObjects, string label)
        {
            sceneObjects.Frame.ClearSessionInteractions();
            sceneObjects.Harness.ClearTelemetry();
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.Caretaker.transform);
            var accepted = sceneObjects.Harness.TryDispatchInteract();
            RecordCheck($"{label}_dispatch_returns_true", accepted);
            RecordCheck($"{label}_state_accepted", sceneObjects.Objective.State == M3ObjectiveState.Accepted);
            RecordCheck($"{label}_relic_became_active", sceneObjects.RelicObject.activeSelf);
            RecordCheck($"{label}_harness_outcome_fired", sceneObjects.Harness.LastOutcome == InteractFeedbackOutcome.Fired);
            RecordCheck($"{label}_npc_event_before_objective_accepted", HasEventOrder(
                sceneObjects.Harness,
                M3NamedNpcInteractTarget.TelemetryEvent,
                M3NamedNpcInteractTarget.ObjectiveAcceptedTelemetryEvent));
            RecordCheck($"{label}_accept_payload_from_state", FirstEvent(
                sceneObjects.Harness,
                M3NamedNpcInteractTarget.ObjectiveAcceptedTelemetryEvent).PrimaryPayload == M3ObjectiveState.NotIntroduced.ToString());
            RecordCheck($"{label}_accept_payload_to_state", FirstEvent(
                sceneObjects.Harness,
                M3NamedNpcInteractTarget.ObjectiveAcceptedTelemetryEvent).SecondaryPayload == M3ObjectiveState.Accepted.ToString());
            RecordCheck($"{label}_accept_source_player_driven", FirstEvent(
                sceneObjects.Harness,
                M3NamedNpcInteractTarget.ObjectiveAcceptedTelemetryEvent).PayloadKind == M3NamedNpcInteractTarget.ObjectiveTransitionPayloadKind);
            AppendTelemetrySnapshot(label, sceneObjects.Harness);
        }

        private static void RunAcceptedRetalkRegression(SceneObjects sceneObjects)
        {
            sceneObjects.Frame.ClearSessionInteractions();
            sceneObjects.Harness.ClearTelemetry();
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.Caretaker.transform);
            var result = sceneObjects.Harness.TryDispatchInteract();
            var npcEvent = FirstEvent(sceneObjects.Harness, M3NamedNpcInteractTarget.TelemetryEvent);

            RecordCheck("t2_accepted_retalk_returns_true", result);
            RecordCheck("t2_accepted_retalk_state_unchanged", sceneObjects.Objective.State == M3ObjectiveState.Accepted);
            RecordCheck("t2_accepted_retalk_npc_event_present", npcEvent.TelemetryEvent == M3NamedNpcInteractTarget.TelemetryEvent);
            RecordCheck("t2_accepted_retalk_no_objective_accepted_refire", CountEvent(
                sceneObjects.Harness,
                M3NamedNpcInteractTarget.ObjectiveAcceptedTelemetryEvent) == 0);
            RecordCheck("t2_accepted_retalk_player_driven_payload", npcEvent.PayloadKind == M3NamedNpcInteractTarget.PayloadKind);
            RecordCheck("t2_accepted_retalk_harness_outcome_fired", sceneObjects.Harness.LastOutcome == InteractFeedbackOutcome.Fired);

            AppendRegressionCheck("s3_02_t2_accepted_retalk_returns_true", result);
            AppendRegressionCheck("s3_02_t2_npc_interaction_intentional_present", npcEvent.TelemetryEvent == M3NamedNpcInteractTarget.TelemetryEvent);
            AppendRegressionCheck("s3_02_t2_player_driven_source_preserved", npcEvent.PayloadKind == M3NamedNpcInteractTarget.PayloadKind);
            AppendRegressionCheck("s3_02_t2_feedback_fired", sceneObjects.Harness.LastOutcome == InteractFeedbackOutcome.Fired);
            AppendRegressionTelemetry($"accepted_retalk.telemetry_event={npcEvent.TelemetryEvent}");
            AppendRegressionTelemetry($"accepted_retalk.source={M3NamedNpcInteractTarget.SourceAttribution}");
            AppendRegressionTelemetry($"accepted_retalk.npc_id={npcEvent.TargetId}");
            AppendRegressionTelemetry($"accepted_retalk.player_actor_id={npcEvent.PlayerActorId}");
            AppendRegressionTelemetry($"accepted_retalk.feedback_event={S3PlayerInteractionHarness.FiredTelemetryEvent}");
            AppendTelemetrySnapshot("t2_accepted_retalk", sceneObjects.Harness);
        }

        private static void RunRelicBlockedDispatch(SceneObjects sceneObjects)
        {
            ConfigureFreshSession(sceneObjects);
            sceneObjects.RelicObject.SetActive(true);
            sceneObjects.Harness.RefreshRegisteredTargetsFromScene();
            sceneObjects.Harness.ClearTelemetry();
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.RelicObject.transform);

            var blocked = sceneObjects.Harness.TryDispatchInteract();
            RecordCheck("t3_blocked_relic_dispatch_returns_false", !blocked);
            RecordCheck("t3_blocked_state_still_not_introduced", sceneObjects.Objective.State == M3ObjectiveState.NotIntroduced);
            RecordCheck("t3_blocked_no_relic_recovered_event", CountEvent(
                sceneObjects.Harness,
                M3RelicInteractTarget.RelicRecoveredTelemetryEvent) == 0);
            RecordCheck("t3_blocked_interact_blocked_feedback", sceneObjects.Harness.LastOutcome == InteractFeedbackOutcome.Blocked);
            RecordCheck("t3_blocked_feedback_has_no_routing_hint", !ContainsForbiddenFeedbackText(sceneObjects.Harness.LastFeedbackText));
            AppendTelemetrySnapshot("t3_blocked", sceneObjects.Harness);
        }

        private static void RunRelicPartialSuccessDispatch(SceneObjects sceneObjects)
        {
            ConfigureFreshSession(sceneObjects);
            _ = sceneObjects.Objective.TryAcceptObjectiveFromNpc(
                sceneObjects.Frame,
                S3PlayerInteractionHarness.DefaultPlayerActorId,
                InRangeOffsetMeters);
            sceneObjects.Vendor.gameObject.SetActive(false);
            sceneObjects.RelicAdapter.Configure(sceneObjects.Objective, lootVendor: null);
            sceneObjects.Harness.RefreshRegisteredTargetsFromScene();
            sceneObjects.Harness.ClearTelemetry();
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.RelicObject.transform);

            var partial = sceneObjects.Harness.TryDispatchInteract();
            RecordCheck("t3_partial_dispatch_returns_true", partial);
            RecordCheck("t3_partial_state_relic_recovered", sceneObjects.Objective.State == M3ObjectiveState.RelicRecovered);
            RecordCheck("t3_partial_relic_inactive_after_recovery", !sceneObjects.RelicObject.activeSelf);
            RecordCheck("t3_partial_relic_recovered_event_present", CountEvent(
                sceneObjects.Harness,
                M3RelicInteractTarget.RelicRecoveredTelemetryEvent) == 1);
            RecordCheck("t3_partial_loot_failed_event_present", CountEvent(
                sceneObjects.Harness,
                M3RelicInteractTarget.ObjectiveLootResolutionFailedTelemetryEvent) == 1);
            RecordCheck("t3_partial_no_loot_resolved_event", CountEvent(
                sceneObjects.Harness,
                M3RelicInteractTarget.ObjectiveLootResolvedTelemetryEvent) == 0);
            RecordCheck("t3_partial_harness_outcome_fired", sceneObjects.Harness.LastOutcome == InteractFeedbackOutcome.Fired);
            AppendTelemetrySnapshot("t3_partial", sceneObjects.Harness);

            sceneObjects.Vendor.gameObject.SetActive(true);
            sceneObjects.Vendor.ResetSessionVendor();
            sceneObjects.RelicAdapter.Configure(sceneObjects.Objective, sceneObjects.Vendor);
        }

        private static void RunRelicSuccessAndHandInDispatches(SceneObjects sceneObjects)
        {
            ConfigureFreshSession(sceneObjects);
            RunAcceptDispatch(sceneObjects, "t3_success_accept_setup");
            sceneObjects.Harness.RefreshRegisteredTargetsFromScene();
            sceneObjects.Harness.ClearTelemetry();
            var currencyBeforeRecovery = sceneObjects.Vendor.CarriedCurrencyCopper;
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.RelicObject.transform);

            var recovered = sceneObjects.Harness.TryDispatchInteract();
            RecordCheck("t3_success_relic_dispatch_returns_true", recovered);
            RecordCheck("t3_success_state_relic_recovered", sceneObjects.Objective.State == M3ObjectiveState.RelicRecovered);
            RecordCheck("t3_success_relic_inactive_after_recovery", !sceneObjects.RelicObject.activeSelf);
            RecordCheck("t3_success_relic_carried_in_vendor_inventory", sceneObjects.Vendor.CarriesCourtMarkedRelic);
            RecordCheck("t3_success_salvage_carried_in_vendor_inventory", sceneObjects.Vendor.CarriesSalvage);
            RecordCheck("t3_success_currency_unchanged", sceneObjects.Vendor.CarriedCurrencyCopper == currencyBeforeRecovery);
            RecordCheck("t3_success_relic_before_loot_resolved", HasEventOrder(
                sceneObjects.Harness,
                M3RelicInteractTarget.RelicRecoveredTelemetryEvent,
                M3RelicInteractTarget.ObjectiveLootResolvedTelemetryEvent));
            RecordCheck("t3_success_harness_outcome_fired", sceneObjects.Harness.LastOutcome == InteractFeedbackOutcome.Fired);
            AppendTelemetrySnapshot("t3_success_recover", sceneObjects.Harness);

            sceneObjects.Frame.ClearSessionInteractions();
            sceneObjects.Harness.ClearTelemetry();
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.Caretaker.transform);
            var handedIn = sceneObjects.Harness.TryDispatchInteract();
            RecordCheck("t4_hand_in_dispatch_returns_true", handedIn);
            RecordCheck("t4_hand_in_state_complete", sceneObjects.Objective.State == M3ObjectiveState.Complete);
            RecordCheck("t4_hand_in_event_present", CountEvent(
                sceneObjects.Harness,
                M3NamedNpcInteractTarget.RelicHandedInTelemetryEvent) == 1);
            RecordCheck("t4_hand_in_no_npc_interaction_event", CountEvent(
                sceneObjects.Harness,
                M3NamedNpcInteractTarget.TelemetryEvent) == 0);
            RecordCheck("t4_hand_in_harness_outcome_fired", sceneObjects.Harness.LastOutcome == InteractFeedbackOutcome.Fired);
            AppendTelemetrySnapshot("t4_hand_in", sceneObjects.Harness);
        }

        private static void RunCompleteRetalk(SceneObjects sceneObjects)
        {
            sceneObjects.Harness.ClearTelemetry();
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.Caretaker.transform);
            var result = sceneObjects.Harness.TryDispatchInteract();
            RecordCheck("t5_complete_retalk_returns_true", result);
            RecordCheck("t5_complete_retalk_state_unchanged", sceneObjects.Objective.State == M3ObjectiveState.Complete);
            RecordCheck("t5_complete_retalk_npc_event_present", CountEvent(
                sceneObjects.Harness,
                M3NamedNpcInteractTarget.TelemetryEvent) == 1);
            RecordCheck("t5_complete_retalk_no_hand_in_refire", CountEvent(
                sceneObjects.Harness,
                M3NamedNpcInteractTarget.RelicHandedInTelemetryEvent) == 0);
            AppendTelemetrySnapshot("t5_complete_retalk", sceneObjects.Harness);
        }

        private static void RunEndToEndDispatch(SceneObjects sceneObjects)
        {
            ConfigureFreshSession(sceneObjects);
            sceneObjects.Harness.ClearTelemetry();
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.Caretaker.transform);
            var accept = sceneObjects.Harness.TryDispatchInteract();
            sceneObjects.Harness.RefreshRegisteredTargetsFromScene();
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.RelicObject.transform);
            var recover = sceneObjects.Harness.TryDispatchInteract();
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.Caretaker.transform);
            var handIn = sceneObjects.Harness.TryDispatchInteract();

            RecordCheck("t7_end_to_end_accept_returns_true", accept);
            RecordCheck("t7_end_to_end_recover_returns_true", recover);
            RecordCheck("t7_end_to_end_hand_in_returns_true", handIn);
            RecordCheck("t7_end_to_end_state_complete", sceneObjects.Objective.State == M3ObjectiveState.Complete);
            RecordCheck("t7_end_to_end_state_sequence_exact", sceneObjects.Objective.StateSequence == "NotIntroduced -> Accepted -> RelicRecovered -> Complete");
            RecordCheck("t7_end_to_end_relic_carried", sceneObjects.Vendor.CarriesCourtMarkedRelic);
            RecordCheck("t7_end_to_end_salvage_carried", sceneObjects.Vendor.CarriesSalvage);
            RecordCheck("t7_end_to_end_full_target_vocabulary_order", HasTargetVocabularyOrder(
                sceneObjects.Harness,
                M3NamedNpcInteractTarget.TelemetryEvent,
                M3NamedNpcInteractTarget.ObjectiveAcceptedTelemetryEvent,
                M3RelicInteractTarget.RelicRecoveredTelemetryEvent,
                M3RelicInteractTarget.ObjectiveLootResolvedTelemetryEvent,
                M3NamedNpcInteractTarget.RelicHandedInTelemetryEvent));
            RecordCheck("t7_end_to_end_no_route_hint_feedback", !ContainsForbiddenFeedbackText(sceneObjects.Harness.LastFeedbackText));
            AppendTelemetrySnapshot("t7_end_to_end", sceneObjects.Harness);
        }

        private static void ConfigureFreshSession(SceneObjects sceneObjects)
        {
            sceneObjects.Vendor.gameObject.SetActive(true);
            sceneObjects.Frame.ClearSessionInteractions();
            sceneObjects.Objective.ResetSessionObjective();
            sceneObjects.Vendor.ResetSessionVendor();
            sceneObjects.NpcAdapter.Configure(sceneObjects.Frame, sceneObjects.Objective);
            sceneObjects.RelicAdapter.Configure(sceneObjects.Objective, sceneObjects.Vendor);
            sceneObjects.Harness.Configure(sceneObjects.PlayerMarker.transform, S3PlayerInteractionHarness.DefaultInteractRangeMeters);
            sceneObjects.Harness.ClearRegisteredTargets();
            sceneObjects.Harness.ClearTelemetry();
            sceneObjects.Harness.RefreshRegisteredTargetsFromScene();
        }

        private static SceneObjects RequiredSceneObjects()
        {
            var harness = UnityEngine.Object.FindFirstObjectByType<S3PlayerInteractionHarness>();
            var playerMarker = FindSceneObjectIncludingInactive(S3PlayerInteractionHarness.ClericMarkerObjectName);
            var caretaker = FindSceneObjectIncludingInactive(M3NamedNpcObjectiveFrame.AnchorObjectName);
            var objectiveRoot = FindSceneObjectIncludingInactive("M3_ObjectiveStateRoot");
            var relicObject = FindSceneObjectIncludingInactive(M3ObjectiveStateRelicHandInSession.RelicObjectName);
            var vendorObject = FindSceneObjectIncludingInactive(M3LootTableFixedProfileVendorData.VendorObjectName);
            var frame = caretaker == null ? null : caretaker.GetComponent<M3NamedNpcObjectiveFrame>();
            var npcAdapter = caretaker == null ? null : caretaker.GetComponent<M3NamedNpcInteractTarget>();
            var objective = objectiveRoot == null ? null : objectiveRoot.GetComponent<M3ObjectiveStateRelicHandIn>();
            var relicAdapter = relicObject == null ? null : relicObject.GetComponent<M3RelicInteractTarget>();
            var vendor = vendorObject == null ? null : vendorObject.GetComponent<M3LootTableFixedProfileVendor>();

            RecordCheck("harness_component_found_in_play_mode", harness != null);
            RecordCheck("cleric_marker_found_in_play_mode", playerMarker != null);
            RecordCheck("caretaker_found_in_play_mode", caretaker != null);
            RecordCheck("objective_component_found_in_play_mode", objective != null);
            RecordCheck("relic_object_found_in_play_mode", relicObject != null);
            RecordCheck("vendor_component_found_in_play_mode", vendor != null);
            RecordCheck("npc_adapter_found_in_play_mode", npcAdapter != null);
            RecordCheck("relic_adapter_found_in_play_mode", relicAdapter != null);
            RecordCheck("frame_found_in_play_mode", frame != null);

            return new SceneObjects(
                harness,
                playerMarker,
                caretaker,
                frame,
                npcAdapter,
                objective,
                relicObject,
                relicAdapter,
                vendor);
        }

        private static void PositionAt(Transform playerMarker, Transform target)
        {
            playerMarker.position = target.position + new Vector3(0.0f, 0.0f, -InRangeOffsetMeters);
        }

        private static bool HasEventOrder(S3PlayerInteractionHarness harness, string firstEvent, string secondEvent)
        {
            return EventIndex(harness, firstEvent) >= 0 &&
                EventIndex(harness, secondEvent) > EventIndex(harness, firstEvent);
        }

        private static bool HasTargetVocabularyOrder(S3PlayerInteractionHarness harness, params string[] events)
        {
            var cursor = -1;
            foreach (var eventName in events)
            {
                var found = EventIndexAfter(harness, eventName, cursor);
                if (found < 0)
                {
                    return false;
                }

                cursor = found;
            }

            return true;
        }

        private static int EventIndex(S3PlayerInteractionHarness harness, string telemetryEvent)
        {
            return EventIndexAfter(harness, telemetryEvent, -1);
        }

        private static int EventIndexAfter(S3PlayerInteractionHarness harness, string telemetryEvent, int afterIndex)
        {
            for (var i = afterIndex + 1; i < harness.TelemetryEvents.Count; i++)
            {
                if (harness.TelemetryEvents[i].TelemetryEvent == telemetryEvent)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int CountEvent(S3PlayerInteractionHarness harness, string telemetryEvent)
        {
            var count = 0;
            foreach (var context in harness.TelemetryEvents)
            {
                if (context.TelemetryEvent == telemetryEvent)
                {
                    count++;
                }
            }

            return count;
        }

        private static InteractContext FirstEvent(S3PlayerInteractionHarness harness, string telemetryEvent)
        {
            foreach (var context in harness.TelemetryEvents)
            {
                if (context.TelemetryEvent == telemetryEvent)
                {
                    return context;
                }
            }

            return default;
        }

        private static void AppendTelemetrySnapshot(string label, S3PlayerInteractionHarness harness)
        {
            AppendSessionLine(TelemetryKey, $"{label}.last_outcome={harness.LastOutcome}");
            AppendSessionLine(TelemetryKey, $"{label}.event_sequence={FormatEventSequence(harness)}");
            foreach (var context in harness.TelemetryEvents)
            {
                AppendSessionLine(
                    TelemetryKey,
                    $"{label}.{context.TelemetryEvent}={context.TargetId}|{context.PlayerActorId}|{context.PayloadKind}|{context.PrimaryPayload}|{context.SecondaryPayload}|{context.DistanceMeters.ToString("0.###", CultureInfo.InvariantCulture)}");
            }
        }

        private static string FormatEventSequence(S3PlayerInteractionHarness harness)
        {
            var events = new List<string>();
            foreach (var context in harness.TelemetryEvents)
            {
                events.Add(context.TelemetryEvent);
            }

            return events.Count == 0 ? "none" : string.Join(">", events);
        }

        private static bool HasNoForbiddenSceneAffordances()
        {
            foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (canvas != null)
                {
                    return false;
                }
            }

            foreach (var textMesh in UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (textMesh != null)
                {
                    return false;
                }
            }

            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                return false;
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(includeInactive: true))
                {
                    if (ContainsForbiddenFeedbackText(transform.name))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool ContainsForbiddenFeedbackText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var lower = value.ToLowerInvariant();
            return lower.IndexOf("quest", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("go to", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("objective located", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("nearest", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("track", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("next step", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("return to", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("now go", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("head to", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("minimap", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("arrow", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("glow", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("outline", StringComparison.Ordinal) >= 0;
        }

        private static M3NamedNpcInteractTarget? FindCaretakerAdapter()
        {
            var caretaker = FindSceneObjectIncludingInactive(M3NamedNpcObjectiveFrame.AnchorObjectName);
            return caretaker == null ? null : caretaker.GetComponent<M3NamedNpcInteractTarget>();
        }

        private static M3RelicInteractTarget? FindRelicAdapterIncludingInactive()
        {
            var relicObject = FindSceneObjectIncludingInactive(M3ObjectiveStateRelicHandInSession.RelicObjectName);
            return relicObject == null ? null : relicObject.GetComponent<M3RelicInteractTarget>();
        }

        private static GameObject? FindSceneObjectIncludingInactive(string objectName)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                return null;
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(includeInactive: true))
                {
                    if (transform.name == objectName)
                    {
                        return transform.gameObject;
                    }
                }
            }

            return null;
        }

        private static void CaptureLog(string condition, string stackTrace, LogType type)
        {
            if (GravenspireScenarioSmokeRunnerHelpers.IsEditorStartupNoise(condition, stackTrace, type))
            {
                return;
            }

            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                AppendSessionLine(ErrorsKey, condition);
            }
            else if (type == LogType.Warning)
            {
                AppendSessionLine(WarningsKey, condition);
            }
        }

        private static void RecordCheck(string name, bool passed)
        {
            AppendSessionLine(ChecksKey, $"{name}={(passed ? "PASS" : "FAIL")}");
        }

        private static void AppendRegressionCheck(string name, bool passed)
        {
            AppendSessionLine(RegressionChecksKey, $"{name}={(passed ? "PASS" : "FAIL")}");
        }

        private static void AppendRegressionTelemetry(string value)
        {
            AppendSessionLine(RegressionTelemetryKey, value);
        }

        private static bool AllChecksPassed()
        {
            foreach (var check in GetSessionLines(ChecksKey))
            {
                if (check.EndsWith("=FAIL", StringComparison.Ordinal))
                {
                    return false;
                }
            }

            foreach (var check in GetSessionLines(RegressionChecksKey))
            {
                if (check.EndsWith("=FAIL", StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return GetSessionLines(ChecksKey).Count > 0;
        }

        private static void WriteEvidenceAndExit(int exitCode)
        {
            EditorApplication.update -= ContinueAfterDomainReload;
            Application.logMessageReceived -= CaptureLog;
            WriteMainEvidence(exitCode);
            WriteRegressionEvidence(exitCode);
            Debug.Log($"{StoryId} player relic recovery + looting verification wrote evidence with exit code {exitCode}.");
            ClearSession();
            EditorApplication.Exit(exitCode);
        }

        private static void WriteMainEvidence(int exitCode)
        {
            var evidencePath = CurrentEvidencePath(EvidencePathKey, DefaultEvidencePath());
            var builder = new StringBuilder();
            builder.AppendLine("# S3-03 Unity Player Relic Recovery + Looting Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine($"**Story:** `production/stories/{StorySlug}.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireS3PlayerRelicRecoveryAndLootingVerificationRunner.cs`");
            builder.AppendLine($"**Result:** {(exitCode == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine();
            builder.AppendLine("## Checks");
            builder.AppendLine();
            AppendCheckLines(builder, GetSessionLines(ChecksKey));
            builder.AppendLine();
            builder.AppendLine("## Relic Adapter Telemetry Shapes");
            builder.AppendLine();
            builder.AppendLine("- Full success: `relic_recovered` then `objective_loot_resolved`; harness outcome `Fired`.");
            builder.AppendLine("- Partial success: `relic_recovered` then `objective_loot_resolution_failed`; harness outcome `Fired`; objective remains `RelicRecovered`.");
            builder.AppendLine("- Blocked: no relic/loot target events; harness outcome `Blocked`.");
            builder.AppendLine();
            builder.AppendLine("## Player-Driven Objective Telemetry");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(TelemetryKey));
            builder.AppendLine();
            builder.AppendLine("## Warnings");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(WarningsKey));
            builder.AppendLine();
            builder.AppendLine("## Errors");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(ErrorsKey));
            File.WriteAllText(evidencePath, builder.ToString());
        }

        private static void WriteRegressionEvidence(int exitCode)
        {
            var evidencePath = CurrentEvidencePath(RegressionEvidencePathKey, DefaultRegressionEvidencePath());
            var builder = new StringBuilder();
            builder.AppendLine("# S3-03 S3-02 NPC Interaction Regression Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine("**Story:** `production/stories/s3-03-player-relic-recovery-and-looting.md`");
            builder.AppendLine("**Regression Target:** `production/stories/s3-02-player-driven-npc-interaction.md` S3-02-T2, rerun in Accepted-state re-talk routing.");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireS3PlayerRelicRecoveryAndLootingVerificationRunner.cs`");
            builder.AppendLine($"**Result:** {(exitCode == 0 && GetSessionLines(RegressionChecksKey).Count > 0 ? "PASS" : "FAIL")}");
            builder.AppendLine();
            builder.AppendLine("## Checks");
            builder.AppendLine();
            AppendCheckLines(builder, GetSessionLines(RegressionChecksKey));
            builder.AppendLine();
            builder.AppendLine("## Telemetry");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(RegressionTelemetryKey));
            builder.AppendLine();
            builder.AppendLine("## Notes");
            builder.AppendLine();
            builder.AppendLine("- Regression path intentionally starts from `Accepted`, matching S3-03 AC-02: the expanded adapter still records `npc_interaction_intentional` and `interact_fired` for S3-02-style re-talk.");
            File.WriteAllText(evidencePath, builder.ToString());
        }

        private static string CurrentEvidencePath(string key, string defaultPath)
        {
            var configuredPath = SessionState.GetString(key, string.Empty);
            return string.IsNullOrWhiteSpace(configuredPath) ? defaultPath : configuredPath;
        }

        private static string DefaultEvidencePath()
        {
            return Path.Combine(
                "tests",
                "evidence",
                StoryId,
                $"unity-player-relic-recovery-and-looting-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-smoke.md");
        }

        private static string DefaultRegressionEvidencePath()
        {
            return Path.Combine(
                "tests",
                "evidence",
                StoryId,
                $"s3-02-regression-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-smoke.md");
        }

        private static string ResolveEvidencePathFromCommandLine(string argumentName, string defaultEvidencePath)
        {
            var arguments = Environment.GetCommandLineArgs();
            for (var i = 0; i < arguments.Length - 1; i++)
            {
                if (string.Equals(arguments[i], argumentName, StringComparison.OrdinalIgnoreCase))
                {
                    return arguments[i + 1];
                }
            }

            return defaultEvidencePath;
        }

        private static void AppendCheckLines(StringBuilder builder, List<string> lines)
        {
            if (lines.Count == 0)
            {
                builder.AppendLine("- FAIL `no_checks_recorded`");
                return;
            }

            foreach (var check in lines)
            {
                var parts = check.Split('=');
                var name = parts[0];
                var passed = parts.Length > 1 && parts[1] == "PASS";
                builder.AppendLine($"- {(passed ? "PASS" : "FAIL")} `{name}`");
            }
        }

        private static void AppendEvidenceLines(StringBuilder builder, List<string> lines)
        {
            if (lines.Count == 0)
            {
                builder.AppendLine("- None captured during runner execution.");
                return;
            }

            foreach (var line in lines)
            {
                builder.AppendLine($"- {line}");
            }
        }

        private static void AppendSessionLine(string key, string value)
        {
            var current = SessionState.GetString(key, string.Empty);
            SessionState.SetString(key, string.IsNullOrEmpty(current) ? value : current + "\n" + value);
        }

        private static List<string> GetSessionLines(string key)
        {
            var value = SessionState.GetString(key, string.Empty);
            return string.IsNullOrWhiteSpace(value)
                ? new List<string>()
                : new List<string>(value.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static void ClearSession()
        {
            SessionState.EraseBool(RunKey);
            SessionState.EraseString(PhaseKey);
            SessionState.EraseString(ChecksKey);
            SessionState.EraseString(ErrorsKey);
            SessionState.EraseString(WarningsKey);
            SessionState.EraseString(TelemetryKey);
            SessionState.EraseString(RegressionChecksKey);
            SessionState.EraseString(RegressionTelemetryKey);
            SessionState.EraseString(EvidencePathKey);
            SessionState.EraseString(RegressionEvidencePathKey);
            SessionState.EraseString(PlayStartedKey);
        }

        private readonly struct SceneObjects
        {
            public SceneObjects(
                S3PlayerInteractionHarness? harness,
                GameObject? playerMarker,
                GameObject? caretaker,
                M3NamedNpcObjectiveFrame? frame,
                M3NamedNpcInteractTarget? npcAdapter,
                M3ObjectiveStateRelicHandIn? objective,
                GameObject? relicObject,
                M3RelicInteractTarget? relicAdapter,
                M3LootTableFixedProfileVendor? vendor)
            {
                Harness = harness!;
                PlayerMarker = playerMarker!;
                Caretaker = caretaker!;
                Frame = frame!;
                NpcAdapter = npcAdapter!;
                Objective = objective!;
                RelicObject = relicObject!;
                RelicAdapter = relicAdapter!;
                Vendor = vendor!;
                Valid = harness != null &&
                    playerMarker != null &&
                    caretaker != null &&
                    frame != null &&
                    npcAdapter != null &&
                    objective != null &&
                    relicObject != null &&
                    relicAdapter != null &&
                    vendor != null;
            }

            public bool Valid { get; }

            public S3PlayerInteractionHarness Harness { get; }

            public GameObject PlayerMarker { get; }

            public GameObject Caretaker { get; }

            public M3NamedNpcObjectiveFrame Frame { get; }

            public M3NamedNpcInteractTarget NpcAdapter { get; }

            public M3ObjectiveStateRelicHandIn Objective { get; }

            public GameObject RelicObject { get; }

            public M3RelicInteractTarget RelicAdapter { get; }

            public M3LootTableFixedProfileVendor Vendor { get; }
        }
    }
}
#endif
