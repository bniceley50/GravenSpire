#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Gravenspire.UnityRuntime.Combat;
using Gravenspire.UnityRuntime.Npc;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    [InitializeOnLoad]
    public static class GravenspireM3NamedNpcObjectiveFrameVerificationRunner
    {
        private const string StoryId = "S2-M3-01";
        private const string StorySlug = "s2-m3-01-named-npc-objective-frame";
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string RunKey = "GravenspireM3NamedNpcObjectiveFrame.Run";
        private const string PhaseKey = "GravenspireM3NamedNpcObjectiveFrame.Phase";
        private const string ChecksKey = "GravenspireM3NamedNpcObjectiveFrame.Checks";
        private const string ErrorsKey = "GravenspireM3NamedNpcObjectiveFrame.Errors";
        private const string EventsKey = "GravenspireM3NamedNpcObjectiveFrame.Events";
        private const string M2TelemetryKey = "GravenspireM3NamedNpcObjectiveFrame.M2Telemetry";
        private const string WarningsKey = "GravenspireM3NamedNpcObjectiveFrame.Warnings";
        private const string EvidencePathKey = "GravenspireM3NamedNpcObjectiveFrame.EvidencePath";
        private const string PlayStartedKey = "GravenspireM3NamedNpcObjectiveFrame.PlayStartedTicks";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const double SmokeDelaySeconds = 1.0;

        static GravenspireM3NamedNpcObjectiveFrameVerificationRunner()
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

        [MenuItem("Gravenspire/Verify M3 Named NPC Objective Frame")]
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
                GravenspireM3NamedNpcObjectiveFrameBuilder.Build();
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                RecordCheck("scene_loaded", scene.IsValid() && scene.path == ScenePath);
                RecordCheck("m3_named_npc_anchor_exists", GameObject.Find(M3NamedNpcObjectiveFrame.AnchorObjectName) != null);
                RecordCheck(
                    "exactly_one_m3_named_npc_component",
                    UnityEngine.Object.FindObjectsByType<M3NamedNpcObjectiveFrame>(FindObjectsSortMode.None).Length == 1);
                RecordCheck("npc_anchor_visible_renderer", HasVisibleNpcRenderer());
                RecordCheck("npc_interaction_trigger_exists", HasNpcTriggerCollider());
                RecordCheck("no_marker_affordance_components", HasNoForbiddenNpcAffordances());
                RecordCheck("m2_camp_rest_point_exists", GameObject.Find("M2_CampRestPoint") != null);
                RecordCheck("m2_baseline_trash_exists", GameObject.Find("M2_BaselineTrash") != null);
                RecordCheck("m2_linked_trash_exists", GameObject.Find("M2_LinkedTrash") != null);
                RecordCheck("m2_named_blocker_exists", GameObject.Find("M2_NamedBlocker") != null);
                RecordCheck("m2_loop_controller_exists", GameObject.Find("M2_CombatCampLoopRoot")?.GetComponent<M2SingleTrashMedLoopController>() != null);

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
            var npc = UnityEngine.Object.FindFirstObjectByType<M3NamedNpcObjectiveFrame>();
            RecordCheck("npc_component_found_in_play_mode", npc != null);
            if (npc is null)
            {
                AppendSessionLine(ErrorsKey, "M3 named NPC objective frame component was not found.");
                return;
            }

            npc.ClearSessionInteractions();
            var playerMarker = GameObject.Find("ClericShellMarker");
            RecordCheck("cleric_marker_found_for_interaction", playerMarker != null);
            if (playerMarker is null)
            {
                AppendSessionLine(ErrorsKey, "ClericShellMarker was not found for M3 NPC interaction smoke.");
                return;
            }

            playerMarker.transform.position = npc.transform.position + new Vector3(0.0f, 0.0f, -1.25f);
            var distance = Vector3.Distance(playerMarker.transform.position, npc.transform.position);
            var interactionRecorded = npc.TryRecordIntentionalInteraction(M3NamedNpcObjectiveFrame.PlayerActorId, distance, out var context);
            RecordCheck("intentional_interaction_recorded", interactionRecorded && npc.HasRecordedInteraction);
            RecordCheck("npc_interaction_context_shape_recorded", IsExpectedContext(context));
            RecordCheck("templated_dialogue_handle_recorded", IsExpectedDialogueHandle(context));
            RecordCheck("interaction_state_is_interacting", context.WasIntentional && context.InteractionState == M3NamedNpcObjectiveFrame.InteractionState);
            RecordCheck("session_local_no_persistence_claim", npc.SessionLocalOnly);
            RecordCheck("templated_dialogue_only", npc.UsesTemplatedDialogueOnly);
            AppendSessionLine(EventsKey, $"npc_id={context.NpcId}");
            AppendSessionLine(EventsKey, $"player_actor_id={context.PlayerActorId}");
            AppendSessionLine(EventsKey, $"active_zone_id={context.ActiveZoneId}");
            AppendSessionLine(EventsKey, $"interaction_state={context.InteractionState}");
            AppendSessionLine(EventsKey, $"interaction_kind={context.InteractionKind}");
            AppendSessionLine(EventsKey, $"dialogue_template_set_id={context.DialogueTemplateSetId}");
            AppendSessionLine(EventsKey, $"objective_frame_text_key={context.ObjectiveFrameTextKey}");
            AppendSessionLine(EventsKey, $"distance_meters={context.DistanceMeters:0.00}");

            var controller = UnityEngine.Object.FindFirstObjectByType<M2SingleTrashMedLoopController>();
            RecordCheck("m2_controller_found_for_preservation", controller != null);
            if (controller is null)
            {
                AppendSessionLine(ErrorsKey, "M2 combat loop controller was not found for preservation checks.");
                return;
            }

            RecordCheck("m2_controller_initialized_for_preservation", controller.IsInitialized);
            RecordCheck("m2_linked_anchor_available", controller.LinkedTrashArrangementPresent);
            RecordCheck("m2_named_blocker_anchor_available", controller.NamedBlockerAnchorPresent);
            AppendSessionLine(M2TelemetryKey, "full_m2_preservation=external_runner_reruns_under_tests/evidence/S2-M3-01");
            AppendSessionLine(M2TelemetryKey, $"m2_controller_initialized={controller.IsInitialized}");
            AppendSessionLine(M2TelemetryKey, $"m2_linked_anchor_available={controller.LinkedTrashArrangementPresent}");
            AppendSessionLine(M2TelemetryKey, $"m2_named_blocker_anchor_available={controller.NamedBlockerAnchorPresent}");

            foreach (var error in controller.Errors)
            {
                AppendSessionLine(ErrorsKey, error);
            }
        }

        private static bool IsExpectedContext(NpcInteractionContext context)
        {
            return context.NpcId == M3NamedNpcObjectiveFrame.NpcId &&
                context.PlayerActorId == M3NamedNpcObjectiveFrame.PlayerActorId &&
                context.ActiveZoneId == M3NamedNpcObjectiveFrame.ActiveZoneId &&
                context.InteractionKind == M3NamedNpcObjectiveFrame.InteractionKind &&
                context.DistanceMeters <= M3NamedNpcObjectiveFrame.InteractionRangeMeters;
        }

        private static bool IsExpectedDialogueHandle(NpcInteractionContext context)
        {
            return context.DialogueTemplateSetId == M3NamedNpcObjectiveFrame.DialogueTemplateSetId &&
                context.ObjectiveFrameTextKey == M3NamedNpcObjectiveFrame.ObjectiveFrameTextKey;
        }

        private static bool HasVisibleNpcRenderer()
        {
            var npc = GameObject.Find(M3NamedNpcObjectiveFrame.AnchorObjectName);
            if (npc == null)
            {
                return false;
            }

            return npc.activeInHierarchy &&
                npc.TryGetComponent<Renderer>(out var renderer) &&
                renderer.enabled;
        }

        private static bool HasNpcTriggerCollider()
        {
            var npc = GameObject.Find(M3NamedNpcObjectiveFrame.AnchorObjectName);
            if (npc == null)
            {
                return false;
            }

            return npc.TryGetComponent<Collider>(out var collider) && collider.isTrigger;
        }

        private static bool HasNoForbiddenNpcAffordances()
        {
            var npc = GameObject.Find(M3NamedNpcObjectiveFrame.AnchorObjectName);
            if (npc == null)
            {
                return false;
            }

            foreach (var transform in npc.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                var lowerName = transform.name.ToLowerInvariant();
                if (lowerName.IndexOf("quest", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("marker", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("minimap", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("nameplate", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("overhead", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("glow", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("outline", StringComparison.Ordinal) >= 0)
                {
                    return false;
                }
            }

            return npc.GetComponentInChildren<Canvas>(includeInactive: true) == null &&
                npc.GetComponentInChildren<TextMesh>(includeInactive: true) == null &&
                npc.GetComponentInChildren<AudioSource>(includeInactive: true) == null &&
                npc.GetComponentInChildren<Light>(includeInactive: true) == null &&
                npc.GetComponentInChildren<LineRenderer>(includeInactive: true) == null &&
                npc.GetComponentInChildren<ParticleSystem>(includeInactive: true) == null;
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
            builder.AppendLine($"# {StoryId} Unity Named NPC Objective Frame Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine($"**Story:** `production/stories/{StorySlug}.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireM3NamedNpcObjectiveFrameVerificationRunner.cs`");
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
            builder.AppendLine("## NPC Interaction Context");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(EventsKey));
            builder.AppendLine();
            builder.AppendLine("## M2 Preservation Telemetry");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(M2TelemetryKey));
            builder.AppendLine();
            builder.AppendLine("## Warnings");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(WarningsKey));
            builder.AppendLine();
            builder.AppendLine("## Errors");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(ErrorsKey));

            File.WriteAllText(evidencePath, builder.ToString());
            Debug.Log($"{StoryId} named NPC objective frame verification wrote {evidencePath} with exit code {exitCode}.");
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
                $"unity-named-npc-objective-frame-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-smoke.md");
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
            SessionState.EraseString(EventsKey);
            SessionState.EraseString(M2TelemetryKey);
            SessionState.EraseString(WarningsKey);
            SessionState.EraseString(EvidencePathKey);
            SessionState.EraseString(PlayStartedKey);
        }
    }
}
#endif
