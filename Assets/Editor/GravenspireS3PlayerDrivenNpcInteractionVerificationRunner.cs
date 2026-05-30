#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Gravenspire.UnityRuntime.Interaction;
using Gravenspire.UnityRuntime.Npc;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    [InitializeOnLoad]
    public static class GravenspireS3PlayerDrivenNpcInteractionVerificationRunner
    {
        private const string StoryId = "S3-02";
        private const string StorySlug = "s3-02-player-driven-npc-interaction";
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string RunKey = "GravenspireS3PlayerDrivenNpcInteraction.Run";
        private const string PhaseKey = "GravenspireS3PlayerDrivenNpcInteraction.Phase";
        private const string ChecksKey = "GravenspireS3PlayerDrivenNpcInteraction.Checks";
        private const string ErrorsKey = "GravenspireS3PlayerDrivenNpcInteraction.Errors";
        private const string WarningsKey = "GravenspireS3PlayerDrivenNpcInteraction.Warnings";
        private const string TelemetryKey = "GravenspireS3PlayerDrivenNpcInteraction.Telemetry";
        private const string EvidencePathKey = "GravenspireS3PlayerDrivenNpcInteraction.EvidencePath";
        private const string PlayStartedKey = "GravenspireS3PlayerDrivenNpcInteraction.PlayStartedSeconds";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const double SmokeDelaySeconds = 1.0d;
        private const float InRangeOffsetMeters = 1.25f;
        private const float OutOfM3RangeOffsetMeters = 2.25f;

        static GravenspireS3PlayerDrivenNpcInteractionVerificationRunner()
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

        [MenuItem("Gravenspire/Verify S3 Player-Driven NPC Interaction")]
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

                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                RecordCheck("scene_loaded", scene.IsValid() && scene.path == ScenePath);
                RecordCheck("harness_root_exists", FindSceneObjectIncludingInactive(S3PlayerInteractionHarness.HarnessRootName) != null);
                RecordCheck("m3_caretaker_anchor_exists", FindSceneObjectIncludingInactive(M3NamedNpcObjectiveFrame.AnchorObjectName) != null);
                RecordCheck("adapter_present_on_m3_caretaker", FindCaretakerAdapter() != null);
                RecordCheck("m3_objective_frame_present_on_m3_caretaker", FindCaretakerFrame() != null);
                RecordCheck("no_dialogue_ui_scene_objects", HasNoDialogueUiSceneObjects());
                RecordCheck("no_forbidden_caretaker_affordances", HasNoForbiddenCaretakerAffordances());

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
            var harness = UnityEngine.Object.FindFirstObjectByType<S3PlayerInteractionHarness>();
            var caretaker = FindSceneObjectIncludingInactive(M3NamedNpcObjectiveFrame.AnchorObjectName);
            var playerMarker = FindSceneObjectIncludingInactive(S3PlayerInteractionHarness.ClericMarkerObjectName);
            var adapter = caretaker == null ? null : caretaker.GetComponent<M3NamedNpcInteractTarget>();
            var frame = caretaker == null ? null : caretaker.GetComponent<M3NamedNpcObjectiveFrame>();

            RecordCheck("harness_component_found_in_play_mode", harness != null);
            RecordCheck("cleric_marker_found_in_play_mode", playerMarker != null);
            RecordCheck("caretaker_found_in_play_mode", caretaker != null);
            RecordCheck("adapter_found_in_play_mode", adapter != null);
            RecordCheck("frame_found_in_play_mode", frame != null);
            if (harness == null || playerMarker == null || caretaker == null || adapter == null || frame == null)
            {
                AppendSessionLine(ErrorsKey, "Required S3-02 scene component was missing in Play Mode.");
                return;
            }

            frame.ClearSessionInteractions();
            harness.Configure(playerMarker.transform, S3PlayerInteractionHarness.DefaultInteractRangeMeters);
            harness.ClearRegisteredTargets();
            harness.ClearTelemetry();
            harness.RefreshRegisteredTargetsFromScene();
            RecordCheck("adapter_registered_with_harness", harness.RegisteredTargetCount == 1);
            RecordCheck("adapter_frame_reference_resolves", adapter.ObjectiveFrame == frame);

            var attemptsBeforeInRange = adapter.InteractionAttemptCount;
            var successesBeforeInRange = adapter.SuccessfulInteractionCount;
            playerMarker.transform.position = caretaker.transform.position + new Vector3(0.0f, 0.0f, -InRangeOffsetMeters);
            var inRangeResult = harness.TryDispatchInteract();
            RecordCheck("in_range_dispatch_returns_true", inRangeResult);
            RecordCheck("adapter_called_once_for_in_range", adapter.InteractionAttemptCount == attemptsBeforeInRange + 1);
            RecordCheck("adapter_success_count_incremented", adapter.SuccessfulInteractionCount == successesBeforeInRange + 1);
            RecordCheck("frame_recorded_one_interaction", frame.RecordedInteractions.Count == 1);
            RecordCheck("harness_last_outcome_fired", harness.LastOutcome == InteractFeedbackOutcome.Fired);
            RecordCheck("harness_records_target_event_before_feedback", HasEventSequence(harness, M3NamedNpcInteractTarget.TelemetryEvent, S3PlayerInteractionHarness.FiredTelemetryEvent));

            var npcContext = frame.LastInteraction;
            var targetTelemetry = FirstEvent(harness, M3NamedNpcInteractTarget.TelemetryEvent);
            RecordCheck("npc_interaction_context_shape_recorded", IsExpectedNpcContext(npcContext));
            RecordCheck("target_telemetry_maps_npc_identity", targetTelemetry.TargetId == M3NamedNpcObjectiveFrame.NpcId);
            RecordCheck("target_telemetry_maps_player_actor", targetTelemetry.PlayerActorId == M3NamedNpcObjectiveFrame.PlayerActorId);
            RecordCheck("target_telemetry_maps_dialogue_handle", targetTelemetry.PrimaryPayload == M3NamedNpcObjectiveFrame.DialogueTemplateSetId);
            RecordCheck("target_telemetry_maps_objective_text_key", targetTelemetry.SecondaryPayload == M3NamedNpcObjectiveFrame.ObjectiveFrameTextKey);
            RecordCheck("target_telemetry_marks_player_driven_source", targetTelemetry.PayloadKind == M3NamedNpcInteractTarget.PayloadKind);
            RecordCheck("target_telemetry_marks_intentional_amount", targetTelemetry.Amount == 1);
            AppendNpcTelemetry("in_range", targetTelemetry, npcContext);

            harness.ClearTelemetry();
            frame.ClearSessionInteractions();
            var attemptsBeforeBoundary = adapter.InteractionAttemptCount;
            playerMarker.transform.position = caretaker.transform.position + new Vector3(0.0f, 0.0f, -frame.ConfiguredInteractionRangeMeters);
            var boundaryResult = harness.TryDispatchInteract();
            RecordCheck("equal_to_m3_range_is_in_range", boundaryResult);
            RecordCheck("adapter_called_once_for_boundary", adapter.InteractionAttemptCount == attemptsBeforeBoundary + 1);
            RecordCheck("boundary_records_target_event", HasEvent(harness, M3NamedNpcInteractTarget.TelemetryEvent));
            AppendSessionLine(TelemetryKey, $"boundary_distance_meters={frame.ConfiguredInteractionRangeMeters.ToString("0.###", CultureInfo.InvariantCulture)}");

            harness.Configure(
                playerMarker.transform,
                frame.ConfiguredInteractionRangeMeters + 0.75f,
                S3PlayerInteractionHarness.DefaultPlayerActorId);
            harness.ClearTelemetry();
            frame.ClearSessionInteractions();
            var attemptsBeforeOutOfRange = adapter.InteractionAttemptCount;
            var successesBeforeOutOfRange = adapter.SuccessfulInteractionCount;
            playerMarker.transform.position = caretaker.transform.position + new Vector3(0.0f, 0.0f, -OutOfM3RangeOffsetMeters);
            var outOfRangeResult = harness.TryDispatchInteract();
            RecordCheck("out_of_m3_range_dispatch_returns_false", !outOfRangeResult);
            RecordCheck("adapter_called_once_for_out_of_range", adapter.InteractionAttemptCount == attemptsBeforeOutOfRange + 1);
            RecordCheck("adapter_success_count_not_incremented_for_out_of_range", adapter.SuccessfulInteractionCount == successesBeforeOutOfRange);
            RecordCheck("frame_records_no_out_of_range_interaction", frame.RecordedInteractions.Count == 0);
            RecordCheck("harness_last_outcome_blocked", harness.LastOutcome == InteractFeedbackOutcome.Blocked);
            RecordCheck("blocked_records_no_npc_target_event", !HasEvent(harness, M3NamedNpcInteractTarget.TelemetryEvent));
            RecordCheck("blocked_records_interact_blocked", HasEvent(harness, S3PlayerInteractionHarness.BlockedTelemetryEvent));
            RecordCheck("blocked_feedback_has_no_routing_hint", !ContainsForbiddenFeedback(harness.LastFeedbackText));
            AppendSessionLine(TelemetryKey, $"out_of_m3_range_distance_meters={OutOfM3RangeOffsetMeters.ToString("0.###", CultureInfo.InvariantCulture)}");
            AppendSessionLine(TelemetryKey, $"blocked_feedback={harness.LastFeedbackText}");

            RecordCheck("no_dialogue_ui_after_interactions", HasNoDialogueUiSceneObjects());
            RecordCheck("no_forbidden_caretaker_affordances_after_interactions", HasNoForbiddenCaretakerAffordances());
        }

        private static bool IsExpectedNpcContext(NpcInteractionContext context)
        {
            return context.NpcId == M3NamedNpcObjectiveFrame.NpcId &&
                context.PlayerActorId == M3NamedNpcObjectiveFrame.PlayerActorId &&
                context.ActiveZoneId == M3NamedNpcObjectiveFrame.ActiveZoneId &&
                context.InteractionState == M3NamedNpcObjectiveFrame.InteractionState &&
                context.InteractionKind == M3NamedNpcObjectiveFrame.InteractionKind &&
                context.DialogueTemplateSetId == M3NamedNpcObjectiveFrame.DialogueTemplateSetId &&
                context.ObjectiveFrameTextKey == M3NamedNpcObjectiveFrame.ObjectiveFrameTextKey &&
                context.WasIntentional &&
                context.DistanceMeters <= M3NamedNpcObjectiveFrame.InteractionRangeMeters;
        }

        private static void AppendNpcTelemetry(string label, InteractContext targetTelemetry, NpcInteractionContext npcContext)
        {
            AppendSessionLine(TelemetryKey, $"{label}.telemetry_event={targetTelemetry.TelemetryEvent}");
            AppendSessionLine(TelemetryKey, $"{label}.source={M3NamedNpcInteractTarget.SourceAttribution}");
            AppendSessionLine(TelemetryKey, $"{label}.npc_id={npcContext.NpcId}");
            AppendSessionLine(TelemetryKey, $"{label}.player_actor_id={npcContext.PlayerActorId}");
            AppendSessionLine(TelemetryKey, $"{label}.active_zone_id={npcContext.ActiveZoneId}");
            AppendSessionLine(TelemetryKey, $"{label}.interaction_state={npcContext.InteractionState}");
            AppendSessionLine(TelemetryKey, $"{label}.interaction_kind={npcContext.InteractionKind}");
            AppendSessionLine(TelemetryKey, $"{label}.dialogue_template_set_id={npcContext.DialogueTemplateSetId}");
            AppendSessionLine(TelemetryKey, $"{label}.objective_frame_text_key={npcContext.ObjectiveFrameTextKey}");
            AppendSessionLine(TelemetryKey, $"{label}.was_intentional={(npcContext.WasIntentional ? "true" : "false")}");
            AppendSessionLine(TelemetryKey, $"{label}.distance_meters={npcContext.DistanceMeters.ToString("0.###", CultureInfo.InvariantCulture)}");
            AppendSessionLine(TelemetryKey, $"{label}.feedback_event_follows_target_event={S3PlayerInteractionHarness.FiredTelemetryEvent}");
        }

        private static bool HasEventSequence(S3PlayerInteractionHarness harness, string firstEvent, string secondEvent)
        {
            for (var i = 0; i < harness.TelemetryEvents.Count - 1; i++)
            {
                if (harness.TelemetryEvents[i].TelemetryEvent == firstEvent &&
                    harness.TelemetryEvents[i + 1].TelemetryEvent == secondEvent)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasEvent(S3PlayerInteractionHarness harness, string telemetryEvent)
        {
            foreach (var context in harness.TelemetryEvents)
            {
                if (context.TelemetryEvent == telemetryEvent)
                {
                    return true;
                }
            }

            return false;
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

        private static M3NamedNpcInteractTarget? FindCaretakerAdapter()
        {
            var caretaker = FindSceneObjectIncludingInactive(M3NamedNpcObjectiveFrame.AnchorObjectName);
            return caretaker == null ? null : caretaker.GetComponent<M3NamedNpcInteractTarget>();
        }

        private static M3NamedNpcObjectiveFrame? FindCaretakerFrame()
        {
            var caretaker = FindSceneObjectIncludingInactive(M3NamedNpcObjectiveFrame.AnchorObjectName);
            return caretaker == null ? null : caretaker.GetComponent<M3NamedNpcObjectiveFrame>();
        }

        private static bool HasNoDialogueUiSceneObjects()
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

            return true;
        }

        private static bool HasNoForbiddenCaretakerAffordances()
        {
            var caretaker = FindSceneObjectIncludingInactive(M3NamedNpcObjectiveFrame.AnchorObjectName);
            if (caretaker == null)
            {
                return false;
            }

            foreach (var transform in caretaker.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                var lowerName = transform.name.ToLowerInvariant();
                if (lowerName.IndexOf("quest", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("marker", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("minimap", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("nameplate", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("overhead", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("glow", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("outline", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("signpost", StringComparison.Ordinal) >= 0)
                {
                    return false;
                }
            }

            return caretaker.GetComponentInChildren<Canvas>(includeInactive: true) == null &&
                caretaker.GetComponentInChildren<TextMesh>(includeInactive: true) == null &&
                caretaker.GetComponentInChildren<AudioSource>(includeInactive: true) == null &&
                caretaker.GetComponentInChildren<Light>(includeInactive: true) == null &&
                caretaker.GetComponentInChildren<LineRenderer>(includeInactive: true) == null &&
                caretaker.GetComponentInChildren<ParticleSystem>(includeInactive: true) == null;
        }

        private static bool ContainsForbiddenFeedback(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var lower = value.ToLowerInvariant();
            return lower.IndexOf("get closer", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("go to", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("route", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("marker", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("minimap", StringComparison.Ordinal) >= 0 ||
                lower.IndexOf("quest", StringComparison.Ordinal) >= 0;
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
            builder.AppendLine("# S3-02 Unity Player-Driven NPC Interaction Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine($"**Story:** `production/stories/{StorySlug}.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireS3PlayerDrivenNpcInteractionVerificationRunner.cs`");
            builder.AppendLine($"**Result:** {(exitCode == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine();
            builder.AppendLine("## Checks");
            builder.AppendLine();

            foreach (var check in GetSessionLines(ChecksKey))
            {
                var parts = check.Split('=');
                var name = parts[0];
                var passed = parts.Length > 1 && parts[1] == "PASS";
                builder.AppendLine($"- {(passed ? "PASS" : "FAIL")} `{name}`");
            }

            builder.AppendLine();
            builder.AppendLine("## Player-Driven NPC Telemetry");
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
            Debug.Log($"{StoryId} player-driven NPC interaction verification wrote {evidencePath} with exit code {exitCode}.");
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
                $"unity-player-driven-npc-interaction-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-smoke.md");
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
    }
}
#endif
