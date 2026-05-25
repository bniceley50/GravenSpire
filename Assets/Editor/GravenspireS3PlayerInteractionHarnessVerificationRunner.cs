#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Gravenspire.UnityRuntime.Combat;
using Gravenspire.UnityRuntime.Interaction;
using Gravenspire.UnityRuntime.Npc;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    [InitializeOnLoad]
    public static class GravenspireS3PlayerInteractionHarnessVerificationRunner
    {
        private const string StoryId = "S3-01";
        private const string StorySlug = "s3-01-standalone-player-interaction-harness";
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string RunKey = "GravenspireS3PlayerInteractionHarness.Run";
        private const string PhaseKey = "GravenspireS3PlayerInteractionHarness.Phase";
        private const string ChecksKey = "GravenspireS3PlayerInteractionHarness.Checks";
        private const string ErrorsKey = "GravenspireS3PlayerInteractionHarness.Errors";
        private const string WarningsKey = "GravenspireS3PlayerInteractionHarness.Warnings";
        private const string TelemetryKey = "GravenspireS3PlayerInteractionHarness.Telemetry";
        private const string EvidencePathKey = "GravenspireS3PlayerInteractionHarness.EvidencePath";
        private const string PlayStartedKey = "GravenspireS3PlayerInteractionHarness.PlayStartedSeconds";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const double SmokeDelaySeconds = 1.0d;

        private static readonly string[] ForbiddenFeedbackFragments =
        {
            "quest",
            "go to",
            "objective located",
            "nearest",
            "track",
            "marker",
            "minimap",
            "route"
        };

        static GravenspireS3PlayerInteractionHarnessVerificationRunner()
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

        [MenuItem("Gravenspire/Verify S3 Player Interaction Harness")]
        public static void Run()
        {
            ClearSession();
            SessionState.SetBool(RunKey, true);
            SessionState.SetString(PhaseKey, "initial");
            Application.logMessageReceived += CaptureLog;

            try
            {
                var evidencePath = ResolveEvidencePathFromCommandLine(DefaultEvidencePath());
                SessionState.SetString(EvidencePathKey, evidencePath);
                Directory.CreateDirectory(Path.GetDirectoryName(evidencePath) ?? ".");

                GravenspireS3PlayerInteractionHarnessBuilder.Build();
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

                var harness = UnityEngine.Object.FindFirstObjectByType<S3PlayerInteractionHarness>();
                var m2Controller = UnityEngine.Object.FindFirstObjectByType<M2SingleTrashMedLoopController>();
                RecordCheck("scene_loaded", scene.IsValid() && scene.path == ScenePath);
                RecordCheck("harness_component_exists", harness != null);
                RecordCheck("exactly_one_harness_component", UnityEngine.Object.FindObjectsByType<S3PlayerInteractionHarness>(FindObjectsSortMode.None).Length == 1);
                RecordCheck("harness_root_exists", GameObject.Find(S3PlayerInteractionHarness.HarnessRootName) != null);
                RecordCheck("m2_loop_controller_exists", m2Controller != null);
                RecordCheck("harness_distinct_from_m2_controller", harness != null && m2Controller != null && harness.gameObject != m2Controller.gameObject);
                RecordCheck("cleric_marker_exists", GameObject.Find(S3PlayerInteractionHarness.ClericMarkerObjectName) != null);

                SessionState.SetString(PhaseKey, "entering_play");
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

            RunSmokeChecks();
            WriteEvidenceAndExit(AllChecksPassed() && GetSessionLines(ErrorsKey).Count == 0 ? 0 : 1);
        }

        private static void RunSmokeChecks()
        {
            var harness = UnityEngine.Object.FindFirstObjectByType<S3PlayerInteractionHarness>();
            RecordCheck("harness_component_found_in_play_mode", harness != null);
            if (harness is null)
            {
                AppendSessionLine(ErrorsKey, "S3 player interaction harness component was not found in Play Mode.");
                return;
            }

            var playerMarker = GameObject.Find(S3PlayerInteractionHarness.ClericMarkerObjectName);
            RecordCheck("cleric_marker_found_in_play_mode", playerMarker != null);
            if (playerMarker is null)
            {
                AppendSessionLine(ErrorsKey, "ClericShellMarker was not found in Play Mode.");
                return;
            }

            var m2Controller = UnityEngine.Object.FindFirstObjectByType<M2SingleTrashMedLoopController>();
            RecordCheck("m2_controller_found_for_preservation_signal", m2Controller != null);
            if (m2Controller != null)
            {
                RecordCheck("m2_controller_initialized_for_preservation_signal", m2Controller.IsInitialized);
                AppendSessionLine(TelemetryKey, $"m2_controller_initialized={m2Controller.IsInitialized}");
            }

            var initialNpcInteractions = CountNpcInteractions();
            playerMarker.transform.position = Vector3.zero;
            harness.Configure(playerMarker.transform, S3PlayerInteractionHarness.DefaultInteractRangeMeters);
            harness.ClearRegisteredTargets();
            harness.ClearTelemetry();

            RecordCheck("prompt_hidden_with_no_targets", !harness.RefreshPromptState() && !harness.PromptVisible);
            var missedResult = harness.TryDispatchInteract();
            RecordCheck("missed_path_returns_false", !missedResult);
            RecordCheck("missed_path_records_telemetry", HasLastEvent(harness, S3PlayerInteractionHarness.MissedTelemetryEvent));
            RecordCheck("missed_feedback_is_acknowledgement_only", !ContainsForbiddenFeedback(harness.LastFeedbackText));
            AppendHarnessTelemetry("missed", harness);

            harness.ClearRegisteredTargets();
            harness.ClearTelemetry();
            var firedTargetObject = CreateTargetObject("S3_MockFiredTarget", new Vector3(0.0f, 0.0f, 1.25f));
            var firedTarget = new MockInteractTarget("mock-fired-target", shouldSucceed: true);
            harness.RegisterTarget(firedTarget, firedTargetObject.transform);
            var firedResult = harness.TryDispatchInteract();
            RecordCheck("fired_path_returns_true", firedResult);
            RecordCheck("fired_target_called_once", firedTarget.CallCount == 1);
            RecordCheck("fired_target_receives_player_actor", firedTarget.LastPlayerActorId == S3PlayerInteractionHarness.DefaultPlayerActorId);
            RecordCheck("fired_target_receives_measured_distance", Mathf.Abs(firedTarget.LastDistanceMeters - 1.25f) <= 0.01f);
            RecordCheck("fired_path_records_telemetry", HasLastEvent(harness, S3PlayerInteractionHarness.FiredTelemetryEvent));
            RecordCheck("fired_feedback_is_acknowledgement_only", !ContainsForbiddenFeedback(harness.LastFeedbackText));
            AppendHarnessTelemetry("fired", harness);

            harness.ClearRegisteredTargets();
            harness.ClearTelemetry();
            var farTargetObject = CreateTargetObject("S3_MockFarTarget", new Vector3(0.0f, 0.0f, 1.9f));
            var nearTargetObject = CreateTargetObject("S3_MockNearTarget", new Vector3(0.0f, 0.0f, 0.8f));
            var farTarget = new MockInteractTarget("mock-far-target", shouldSucceed: true);
            var nearTarget = new MockInteractTarget("mock-near-target", shouldSucceed: true);
            harness.RegisterTarget(farTarget, farTargetObject.transform);
            harness.RegisterTarget(nearTarget, nearTargetObject.transform);
            var nearestResult = harness.TryDispatchInteract();
            RecordCheck("nearest_dispatch_returns_true", nearestResult);
            RecordCheck("nearest_target_called_once", nearTarget.CallCount == 1);
            RecordCheck("farther_target_not_called", farTarget.CallCount == 0);
            AppendHarnessTelemetry("nearest", harness);

            harness.ClearRegisteredTargets();
            harness.ClearTelemetry();
            var blockedTargetObject = CreateTargetObject("S3_MockBlockedTarget", new Vector3(0.0f, 0.0f, 1.0f));
            var blockedTarget = new MockInteractTarget("mock-blocked-target", shouldSucceed: false);
            harness.RegisterTarget(blockedTarget, blockedTargetObject.transform);
            var blockedResult = harness.TryDispatchInteract();
            RecordCheck("blocked_path_returns_false", !blockedResult);
            RecordCheck("blocked_target_called_once", blockedTarget.CallCount == 1);
            RecordCheck("blocked_path_records_telemetry", HasLastEvent(harness, S3PlayerInteractionHarness.BlockedTelemetryEvent));
            RecordCheck("blocked_feedback_is_acknowledgement_only", !ContainsForbiddenFeedback(harness.LastFeedbackText));
            AppendHarnessTelemetry("blocked", harness);

            harness.ClearRegisteredTargets();
            harness.ClearTelemetry();
            blockedTargetObject.transform.position = new Vector3(0.0f, 0.0f, S3PlayerInteractionHarness.DefaultInteractRangeMeters + 0.05f);
            harness.RegisterTarget(blockedTarget, blockedTargetObject.transform);
            var justPastThresholdResult = harness.TryDispatchInteract();
            RecordCheck("target_just_past_threshold_is_missed", !justPastThresholdResult && blockedTarget.CallCount == 1 && HasLastEvent(harness, S3PlayerInteractionHarness.MissedTelemetryEvent));
            AppendHarnessTelemetry("just_past_threshold", harness);

            harness.ClearRegisteredTargets();
            harness.ClearTelemetry();
            firedTargetObject.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            harness.RegisterTarget(firedTarget, firedTargetObject.transform);
            RecordCheck("prompt_visible_at_zero_distance", harness.RefreshPromptState() && harness.PromptVisible);
            firedTargetObject.transform.position = new Vector3(0.0f, 0.0f, S3PlayerInteractionHarness.DefaultInteractRangeMeters);
            RecordCheck("prompt_visible_at_threshold", harness.RefreshPromptState() && harness.PromptVisible);
            var promptTextAtThreshold = harness.CurrentPromptText;
            firedTargetObject.transform.position = new Vector3(0.0f, 0.0f, S3PlayerInteractionHarness.DefaultInteractRangeMeters + 0.05f);
            RecordCheck("prompt_hidden_beyond_threshold", !harness.RefreshPromptState() && !harness.PromptVisible);
            RecordCheck("prompt_text_has_no_locator_terms", !ContainsForbiddenFeedback(promptTextAtThreshold));
            AppendSessionLine(TelemetryKey, $"configured_interact_range_meters={harness.ConfiguredInteractRangeMeters:0.00}");

            RecordCheck("harness_did_not_record_m3_npc_interaction", CountNpcInteractions() == initialNpcInteractions);
            RecordCheck("harness_telemetry_available", harness.TelemetryEvents.Count > 0);
            AppendSessionLine(TelemetryKey, "m2_preservation_external_reruns_required=true");
        }

        private static GameObject CreateTargetObject(string objectName, Vector3 position)
        {
            var targetObject = new GameObject(objectName);
            targetObject.transform.position = position;
            return targetObject;
        }

        private static int CountNpcInteractions()
        {
            var npc = UnityEngine.Object.FindFirstObjectByType<M3NamedNpcObjectiveFrame>();
            return npc == null ? 0 : npc.RecordedInteractions.Count;
        }

        private static bool HasLastEvent(S3PlayerInteractionHarness harness, string telemetryEvent)
        {
            if (harness.TelemetryEvents.Count == 0)
            {
                return false;
            }

            return harness.TelemetryEvents[^1].TelemetryEvent == telemetryEvent;
        }

        private static void AppendHarnessTelemetry(string label, S3PlayerInteractionHarness harness)
        {
            if (harness.TelemetryEvents.Count == 0)
            {
                AppendSessionLine(TelemetryKey, $"{label}=no_event");
                return;
            }

            var context = harness.TelemetryEvents[^1];
            AppendSessionLine(
                TelemetryKey,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}=event:{1}|target:{2}|distance:{3:0.00}|feedback:{4}",
                    label,
                    context.TelemetryEvent,
                    context.TargetId,
                    context.DistanceMeters,
                    harness.LastFeedbackText));
        }

        private static bool ContainsForbiddenFeedback(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            for (var i = 0; i < ForbiddenFeedbackFragments.Length; i++)
            {
                if (value.IndexOf(ForbiddenFeedbackFragments[i], StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
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
            builder.AppendLine("# S3-01 Unity Player Interaction Harness Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine($"**Story:** `production/stories/{StorySlug}.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireS3PlayerInteractionHarnessVerificationRunner.cs`");
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
            builder.AppendLine("## Harness Telemetry");
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
            Debug.Log($"{StoryId} player interaction harness verification wrote {evidencePath} with exit code {exitCode}.");
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
                $"unity-player-interaction-harness-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-smoke.md");
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

        private sealed class MockInteractTarget : IPlayerInteractTarget
        {
            private readonly string _targetId;
            private readonly bool _shouldSucceed;

            public MockInteractTarget(string targetId, bool shouldSucceed)
            {
                _targetId = targetId;
                _shouldSucceed = shouldSucceed;
            }

            public int CallCount { get; private set; }

            public string LastPlayerActorId { get; private set; } = string.Empty;

            public float LastDistanceMeters { get; private set; }

            public bool TryInteract(string playerActorId, float distanceMeters, out InteractContext context)
            {
                CallCount++;
                LastPlayerActorId = playerActorId;
                LastDistanceMeters = distanceMeters;

                if (!_shouldSucceed)
                {
                    context = default;
                    return false;
                }

                context = new InteractContext(
                    "mock_interact_success",
                    playerActorId,
                    _targetId,
                    "interacted",
                    "interacted",
                    distanceMeters,
                    "mock",
                    _targetId);
                return true;
            }
        }
    }
}
#endif
