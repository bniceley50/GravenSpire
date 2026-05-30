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
    public static class GravenspireS3PlayerDrivenVendorVerificationRunner
    {
        private const string StoryId = "S3-04";
        private const string StorySlug = "s3-04-player-driven-vendor";
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string VendorAdapterSourcePath = "Assets/Scripts/M3VendorInteractTarget.cs";
        private const string RunKey = "GravenspireS3PlayerDrivenVendor.Run";
        private const string PhaseKey = "GravenspireS3PlayerDrivenVendor.Phase";
        private const string ChecksKey = "GravenspireS3PlayerDrivenVendor.Checks";
        private const string ErrorsKey = "GravenspireS3PlayerDrivenVendor.Errors";
        private const string WarningsKey = "GravenspireS3PlayerDrivenVendor.Warnings";
        private const string TelemetryKey = "GravenspireS3PlayerDrivenVendor.Telemetry";
        private const string EvidencePathKey = "GravenspireS3PlayerDrivenVendor.EvidencePath";
        private const string PlayStartedKey = "GravenspireS3PlayerDrivenVendor.PlayStartedSeconds";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const double SmokeDelaySeconds = 1.0d;
        private const float InRangeOffsetMeters = 1.25f;

        static GravenspireS3PlayerDrivenVendorVerificationRunner()
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

        [MenuItem("Gravenspire/Verify S3 Player-Driven Vendor")]
        public static void Run()
        {
            ClearSession();
            SessionState.SetBool(RunKey, true);
            SessionState.SetString(PhaseKey, "initial");
            Application.logMessageReceived -= CaptureLog;
            Application.logMessageReceived += CaptureLog;

            try
            {
                var evidencePath = ResolveEvidencePathFromCommandLine(DefaultEvidencePath());
                SessionState.SetString(EvidencePathKey, evidencePath);
                Directory.CreateDirectory(Path.GetDirectoryName(evidencePath) ?? ".");

                GravenspireS3PlayerDrivenVendorBuilder.Build();
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                RecordCheck("scene_loaded", scene.IsValid() && scene.path == ScenePath);
                RecordCheck("harness_root_exists", FindSceneObjectIncludingInactive(S3PlayerInteractionHarness.HarnessRootName) != null);
                RecordCheck("m3_caretaker_anchor_exists", FindSceneObjectIncludingInactive(M3NamedNpcObjectiveFrame.AnchorObjectName) != null);
                RecordCheck("m3_objective_relic_exists", FindSceneObjectIncludingInactive(M3ObjectiveStateRelicHandInSession.RelicObjectName) != null);
                RecordCheck("m3_court_vendor_exists", FindSceneObjectIncludingInactive(M3LootTableFixedProfileVendorData.VendorObjectName) != null);
                RecordCheck("vendor_adapter_present_on_m3_court_vendor", FindVendorAdapterIncludingInactive() != null);
                RecordCheck("buy_side_absent_from_vendor_adapter_source", HasNoBuySideSourceReference());
                RecordCheck("no_vendor_buy_side_scene_affordances", HasNoForbiddenSceneAffordances());

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
                AppendSessionLine(ErrorsKey, "Required S3-04 scene component was missing in Play Mode.");
                return;
            }

            ConfigureFreshSession(sceneObjects);
            RecordCheck("vendor_adapter_reference_resolves", sceneObjects.VendorAdapter.Vendor == sceneObjects.Vendor);
            RecordCheck("vendor_adapter_registered_with_harness", sceneObjects.Harness.RegisteredTargetCount >= 2);
            RecordCheck("vendor_session_starts_without_salvage", !sceneObjects.Vendor.CarriesSalvage);
            RecordCheck("vendor_session_is_t1_local_only", sceneObjects.Vendor.SessionLocalOnly && !sceneObjects.Vendor.PersistsCurrencyAtRest);
            RecordCheck("vendor_authority_annotation_present", VendorAdapterSourceContains("SERVER-AUTH-INTENT"));

            RunBlockedSaleDispatch(sceneObjects, "t3_blocked_fresh_vendor");
            RunSaleSuccessDispatch(sceneObjects);
            RunEndToEndVendorDispatch(sceneObjects);

            RecordCheck("buy_side_absent_from_vendor_adapter_source_after_play", HasNoBuySideSourceReference());
            RecordCheck("no_vendor_buy_side_scene_affordances_after_interactions", HasNoForbiddenSceneAffordances());
            RecordCheck("feedback_rule_forbidden_text_absent", !ContainsForbiddenFeedbackText(sceneObjects.Harness.LastFeedbackText));
        }

        private static void RunBlockedSaleDispatch(SceneObjects sceneObjects, string label)
        {
            ConfigureFreshSession(sceneObjects);
            var currencyBefore = sceneObjects.Vendor.CarriedCurrencyCopper;
            var attemptsBefore = sceneObjects.VendorAdapter.InteractionAttemptCount;
            sceneObjects.Harness.ClearTelemetry();
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.Vendor.gameObject.transform);

            var blocked = sceneObjects.Harness.TryDispatchInteract();
            RecordCheck($"{label}_dispatch_returns_false", !blocked);
            RecordCheck($"{label}_adapter_called_once", sceneObjects.VendorAdapter.InteractionAttemptCount == attemptsBefore + 1);
            RecordCheck($"{label}_no_sale_event", CountEvent(sceneObjects.Harness, M3VendorInteractTarget.SalvageSoldTelemetryEvent) == 0);
            RecordCheck($"{label}_no_copper_event", CountEvent(sceneObjects.Harness, M3VendorInteractTarget.SellCopperAppliedTelemetryEvent) == 0);
            RecordCheck($"{label}_currency_unchanged", sceneObjects.Vendor.CarriedCurrencyCopper == currencyBefore);
            RecordCheck($"{label}_rejection_reason_captured", !string.IsNullOrWhiteSpace(sceneObjects.Vendor.LastRejectionReason));
            RecordCheck($"{label}_adapter_rejection_reason_captured", !string.IsNullOrWhiteSpace(sceneObjects.VendorAdapter.LastSaleRejectionReason));
            RecordCheck($"{label}_harness_outcome_blocked", sceneObjects.Harness.LastOutcome == InteractFeedbackOutcome.Blocked);
            RecordCheck($"{label}_feedback_has_no_diagnostic_hint", !ContainsForbiddenFeedbackText(sceneObjects.Harness.LastFeedbackText));
            AppendTelemetrySnapshot(label, sceneObjects.Harness);
        }

        private static void RunSaleSuccessDispatch(SceneObjects sceneObjects)
        {
            ConfigureFreshSession(sceneObjects);
            ResolveSalvageThroughPlayerDrivenRelic(sceneObjects, "t2_success_setup");
            sceneObjects.Harness.ClearTelemetry();
            sceneObjects.Harness.RefreshRegisteredTargetsFromScene();

            var currencyBefore = sceneObjects.Vendor.CarriedCurrencyCopper;
            var slotsBefore = sceneObjects.Vendor.CarriedItemSlotsUsed;
            var salvageBefore = sceneObjects.Vendor.GetCarriedQuantity(M3LootTableFixedProfileVendorData.SalvageItemId);
            var attemptsBefore = sceneObjects.VendorAdapter.InteractionAttemptCount;
            var successesBefore = sceneObjects.VendorAdapter.SuccessfulSaleCount;
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.Vendor.gameObject.transform);

            var sold = sceneObjects.Harness.TryDispatchInteract();
            var creditedCopper = sceneObjects.VendorAdapter.LastCreditedCopper;
            var saleEvent = FirstEvent(sceneObjects.Harness, M3VendorInteractTarget.SalvageSoldTelemetryEvent);
            var copperEvent = FirstEvent(sceneObjects.Harness, M3VendorInteractTarget.SellCopperAppliedTelemetryEvent);

            RecordCheck("t2_sale_dispatch_returns_true", sold);
            RecordCheck("t2_sale_adapter_called_once", sceneObjects.VendorAdapter.InteractionAttemptCount == attemptsBefore + 1);
            RecordCheck("t2_sale_success_count_incremented", sceneObjects.VendorAdapter.SuccessfulSaleCount == successesBefore + 1);
            RecordCheck("t2_sale_credited_copper_positive", creditedCopper > 0);
            RecordCheck("t2_sale_currency_exact", sceneObjects.Vendor.CarriedCurrencyCopper == currencyBefore + creditedCopper);
            RecordCheck("t2_sale_slots_decrease_by_one", sceneObjects.Vendor.CarriedItemSlotsUsed == slotsBefore - 1);
            RecordCheck("t2_sale_salvage_quantity_decrements", sceneObjects.Vendor.GetCarriedQuantity(M3LootTableFixedProfileVendorData.SalvageItemId) == salvageBefore - 1);
            RecordCheck("t2_sale_single_salvage_now_absent", salvageBefore != 1 || !sceneObjects.Vendor.CarriesSalvage);
            RecordCheck("t2_sale_event_order", HasEventOrder(
                sceneObjects.Harness,
                M3VendorInteractTarget.SalvageSoldTelemetryEvent,
                M3VendorInteractTarget.SellCopperAppliedTelemetryEvent));
            RecordCheck("t2_sale_copper_event_before_feedback", HasEventOrder(
                sceneObjects.Harness,
                M3VendorInteractTarget.SellCopperAppliedTelemetryEvent,
                S3PlayerInteractionHarness.FiredTelemetryEvent));
            RecordCheck("t2_sale_event_payload_vendor_id", saleEvent.TargetId == sceneObjects.Vendor.ConfiguredVendorId);
            RecordCheck("t2_sale_event_payload_salvage_id", saleEvent.PrimaryPayload == M3LootTableFixedProfileVendorData.SalvageItemId);
            RecordCheck("t2_sale_event_payload_quantity_one", saleEvent.Amount == 1);
            RecordCheck("t2_sale_event_payload_source", saleEvent.SecondaryPayload == M3VendorInteractTarget.SourceAttribution);
            RecordCheck("t2_copper_event_payload_vendor_id", copperEvent.TargetId == sceneObjects.Vendor.ConfiguredVendorId);
            RecordCheck("t2_copper_event_payload_credited_amount", copperEvent.Amount == creditedCopper);
            RecordCheck("t2_copper_event_payload_new_currency", copperEvent.SecondaryPayload == sceneObjects.Vendor.CarriedCurrencyCopper.ToString(CultureInfo.InvariantCulture));
            RecordCheck("t2_sale_harness_outcome_fired", sceneObjects.Harness.LastOutcome == InteractFeedbackOutcome.Fired);
            RecordCheck("t2_sale_feedback_mentions_copper_result", sceneObjects.Harness.LastFeedbackText == $"+{creditedCopper.ToString(CultureInfo.InvariantCulture)} copper");
            RecordCheck("t2_sale_feedback_has_no_buy_side_hint", !ContainsForbiddenFeedbackText(sceneObjects.Harness.LastFeedbackText));
            RecordCheck("t2_sale_no_buy_side_runtime_event", CountEvent(sceneObjects.Harness, "vendor_purchase_fixed_good") == 0);
            AppendTelemetrySnapshot("t2_sale_success", sceneObjects.Harness);

            var currencyAfterSale = sceneObjects.Vendor.CarriedCurrencyCopper;
            sceneObjects.Harness.ClearTelemetry();
            var blockedAfterSellAll = sceneObjects.Harness.TryDispatchInteract();
            RecordCheck("t3_blocked_after_sell_all_returns_false", !blockedAfterSellAll);
            RecordCheck("t3_blocked_after_sell_all_no_sale_event", CountEvent(sceneObjects.Harness, M3VendorInteractTarget.SalvageSoldTelemetryEvent) == 0);
            RecordCheck("t3_blocked_after_sell_all_no_copper_event", CountEvent(sceneObjects.Harness, M3VendorInteractTarget.SellCopperAppliedTelemetryEvent) == 0);
            RecordCheck("t3_blocked_after_sell_all_currency_unchanged", sceneObjects.Vendor.CarriedCurrencyCopper == currencyAfterSale);
            RecordCheck("t3_blocked_after_sell_all_feedback_has_no_diagnostic_hint", !ContainsForbiddenFeedbackText(sceneObjects.Harness.LastFeedbackText));
            AppendTelemetrySnapshot("t3_blocked_after_sell_all", sceneObjects.Harness);
        }

        private static void RunEndToEndVendorDispatch(SceneObjects sceneObjects)
        {
            ConfigureFreshSession(sceneObjects);
            sceneObjects.Harness.ClearTelemetry();
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.Caretaker.transform);
            var accept = sceneObjects.Harness.TryDispatchInteract();
            sceneObjects.Harness.RefreshRegisteredTargetsFromScene();
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.RelicObject.transform);
            var recover = sceneObjects.Harness.TryDispatchInteract();
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.Vendor.gameObject.transform);
            var sell = sceneObjects.Harness.TryDispatchInteract();

            RecordCheck("t6_end_to_end_accept_returns_true", accept);
            RecordCheck("t6_end_to_end_recover_returns_true", recover);
            RecordCheck("t6_end_to_end_sell_returns_true", sell);
            RecordCheck("t6_end_to_end_salvage_sold", !sceneObjects.Vendor.CarriesSalvage);
            RecordCheck("t6_end_to_end_copper_applied", sceneObjects.Vendor.CarriedCurrencyCopper == sceneObjects.VendorAdapter.LastPostSaleCurrencyCopper);
            RecordCheck("t6_end_to_end_harness_outcome_fired", sceneObjects.Harness.LastOutcome == InteractFeedbackOutcome.Fired);
            RecordCheck("t6_end_to_end_full_vendor_vocabulary_order", HasTargetVocabularyOrder(
                sceneObjects.Harness,
                M3NamedNpcInteractTarget.TelemetryEvent,
                M3NamedNpcInteractTarget.ObjectiveAcceptedTelemetryEvent,
                M3RelicInteractTarget.RelicRecoveredTelemetryEvent,
                M3RelicInteractTarget.ObjectiveLootResolvedTelemetryEvent,
                M3VendorInteractTarget.SalvageSoldTelemetryEvent,
                M3VendorInteractTarget.SellCopperAppliedTelemetryEvent));
            RecordCheck("t6_end_to_end_no_route_hint_feedback", !ContainsForbiddenFeedbackText(sceneObjects.Harness.LastFeedbackText));
            AppendTelemetrySnapshot("t6_end_to_end", sceneObjects.Harness);
        }

        private static void ResolveSalvageThroughPlayerDrivenRelic(SceneObjects sceneObjects, string label)
        {
            sceneObjects.Harness.ClearTelemetry();
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.Caretaker.transform);
            var accept = sceneObjects.Harness.TryDispatchInteract();
            sceneObjects.Harness.RefreshRegisteredTargetsFromScene();
            PositionAt(sceneObjects.PlayerMarker.transform, sceneObjects.RelicObject.transform);
            var recover = sceneObjects.Harness.TryDispatchInteract();

            RecordCheck($"{label}_accept_returns_true", accept);
            RecordCheck($"{label}_recover_returns_true", recover);
            RecordCheck($"{label}_state_relic_recovered", sceneObjects.Objective.State == M3ObjectiveState.RelicRecovered);
            RecordCheck($"{label}_vendor_carries_salvage", sceneObjects.Vendor.CarriesSalvage);
            RecordCheck($"{label}_objective_loot_resolved_event_present", CountEvent(
                sceneObjects.Harness,
                M3RelicInteractTarget.ObjectiveLootResolvedTelemetryEvent) == 1);
            AppendTelemetrySnapshot(label, sceneObjects.Harness);
        }

        private static void ConfigureFreshSession(SceneObjects sceneObjects)
        {
            sceneObjects.Vendor.gameObject.SetActive(true);
            sceneObjects.Frame.ClearSessionInteractions();
            sceneObjects.Objective.ResetSessionObjective();
            sceneObjects.Vendor.ResetSessionVendor();
            sceneObjects.NpcAdapter.Configure(sceneObjects.Frame, sceneObjects.Objective);
            sceneObjects.RelicAdapter.Configure(sceneObjects.Objective, sceneObjects.Vendor);
            sceneObjects.VendorAdapter.Configure(sceneObjects.Vendor);
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
            var vendorAdapter = vendorObject == null ? null : vendorObject.GetComponent<M3VendorInteractTarget>();

            RecordCheck("harness_component_found_in_play_mode", harness != null);
            RecordCheck("cleric_marker_found_in_play_mode", playerMarker != null);
            RecordCheck("caretaker_found_in_play_mode", caretaker != null);
            RecordCheck("objective_component_found_in_play_mode", objective != null);
            RecordCheck("relic_object_found_in_play_mode", relicObject != null);
            RecordCheck("vendor_component_found_in_play_mode", vendor != null);
            RecordCheck("npc_adapter_found_in_play_mode", npcAdapter != null);
            RecordCheck("relic_adapter_found_in_play_mode", relicAdapter != null);
            RecordCheck("vendor_adapter_found_in_play_mode", vendorAdapter != null);
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
                vendor,
                vendorAdapter);
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
            AppendSessionLine(TelemetryKey, $"{label}.feedback={harness.LastFeedbackText}");
            AppendSessionLine(TelemetryKey, $"{label}.event_sequence={FormatEventSequence(harness)}");
            foreach (var context in harness.TelemetryEvents)
            {
                AppendSessionLine(
                    TelemetryKey,
                    $"{label}.{context.TelemetryEvent}={context.TargetId}|{context.PlayerActorId}|{context.PayloadKind}|{context.PrimaryPayload}|{context.SecondaryPayload}|amount:{context.Amount}|feedback:{context.FeedbackText}|{context.DistanceMeters.ToString("0.###", CultureInfo.InvariantCulture)}");
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
            return lower.IndexOf("buy", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("purchase", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("for sale", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("in stock", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("shop", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("merchant has", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("now you can", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("go check", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("no salvage", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("nothing to sell", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("come back when", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("now go", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("return to", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("go to", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("quest", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("minimap", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("arrow", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("track", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("route", StringComparison.Ordinal) >= 0;
        }

        private static bool HasNoBuySideSourceReference()
        {
            return !VendorAdapterSourceContains("TryPurchaseFixedVendorGood");
        }

        private static bool VendorAdapterSourceContains(string value)
        {
            if (!File.Exists(VendorAdapterSourcePath))
            {
                return false;
            }

            return File.ReadAllText(VendorAdapterSourcePath).IndexOf(value, StringComparison.Ordinal) >= 0;
        }

        private static M3VendorInteractTarget? FindVendorAdapterIncludingInactive()
        {
            var vendorObject = FindSceneObjectIncludingInactive(M3LootTableFixedProfileVendorData.VendorObjectName);
            return vendorObject == null ? null : vendorObject.GetComponent<M3VendorInteractTarget>();
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

        private static bool AllChecksPassed()
        {
            foreach (var check in GetSessionLines(ChecksKey))
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
            var evidencePath = CurrentEvidencePath();

            var builder = new StringBuilder();
            builder.AppendLine("# S3-04 Unity Player-Driven Vendor Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine($"**Story:** `production/stories/{StorySlug}.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireS3PlayerDrivenVendorVerificationRunner.cs`");
            builder.AppendLine($"**Result:** {(exitCode == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine();
            builder.AppendLine("## Checks");
            builder.AppendLine();
            AppendCheckLines(builder, GetSessionLines(ChecksKey));
            builder.AppendLine();
            builder.AppendLine("## Vendor Adapter Telemetry Shapes");
            builder.AppendLine();
            builder.AppendLine("- Sale success: `vendor_salvage_sold` then `vendor_sell_copper_applied`; harness outcome `Fired`; feedback is `+N copper`.");
            builder.AppendLine("- Sale blocked: no vendor target events; harness outcome `Blocked`; rejection reason remains internal telemetry/debug data.");
            builder.AppendLine();
            builder.AppendLine("## Player-Driven Vendor Telemetry");
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
            Debug.Log($"{StoryId} player-driven vendor verification wrote {evidencePath} with exit code {exitCode}.");
            ClearSession();
            EditorApplication.Exit(exitCode);
        }

        private static string CurrentEvidencePath()
        {
            var configuredPath = SessionState.GetString(EvidencePathKey, string.Empty);
            return string.IsNullOrWhiteSpace(configuredPath) ? DefaultEvidencePath() : configuredPath;
        }

        private static string DefaultEvidencePath()
        {
            return Path.Combine(
                "tests",
                "evidence",
                StoryId,
                $"unity-player-driven-vendor-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-smoke.md");
        }

        private static string ResolveEvidencePathFromCommandLine(string defaultEvidencePath)
        {
            var arguments = Environment.GetCommandLineArgs();
            for (var i = 0; i < arguments.Length - 1; i++)
            {
                if (string.Equals(arguments[i], EvidencePathArgumentName, StringComparison.OrdinalIgnoreCase))
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
            SessionState.EraseString(EvidencePathKey);
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
                M3LootTableFixedProfileVendor? vendor,
                M3VendorInteractTarget? vendorAdapter)
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
                VendorAdapter = vendorAdapter!;
                Valid = harness != null &&
                    playerMarker != null &&
                    caretaker != null &&
                    frame != null &&
                    npcAdapter != null &&
                    objective != null &&
                    relicObject != null &&
                    relicAdapter != null &&
                    vendor != null &&
                    vendorAdapter != null;
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

            public M3VendorInteractTarget VendorAdapter { get; }
        }
    }
}
#endif
