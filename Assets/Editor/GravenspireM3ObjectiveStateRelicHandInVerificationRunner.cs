#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Gravenspire.Gameplay.Npc.M3Objective;
using Gravenspire.UnityRuntime.Combat;
using Gravenspire.UnityRuntime.Npc;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    [InitializeOnLoad]
    public static class GravenspireM3ObjectiveStateRelicHandInVerificationRunner
    {
        private const string StoryId = "S2-M3-02";
        private const string StorySlug = "s2-m3-02-objective-state-relic-hand-in";
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string RunKey = "GravenspireM3ObjectiveStateRelicHandIn.Run";
        private const string PhaseKey = "GravenspireM3ObjectiveStateRelicHandIn.Phase";
        private const string ChecksKey = "GravenspireM3ObjectiveStateRelicHandIn.Checks";
        private const string ErrorsKey = "GravenspireM3ObjectiveStateRelicHandIn.Errors";
        private const string WarningsKey = "GravenspireM3ObjectiveStateRelicHandIn.Warnings";
        private const string TelemetryKey = "GravenspireM3ObjectiveStateRelicHandIn.Telemetry";
        private const string M2TelemetryKey = "GravenspireM3ObjectiveStateRelicHandIn.M2Telemetry";
        private const string EvidencePathKey = "GravenspireM3ObjectiveStateRelicHandIn.EvidencePath";
        private const string PlayStartedKey = "GravenspireM3ObjectiveStateRelicHandIn.PlayStartedTicks";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const double SmokeDelaySeconds = 1.0d;

        static GravenspireM3ObjectiveStateRelicHandInVerificationRunner()
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

        [MenuItem("Gravenspire/Verify M3 Objective State Relic Hand-In")]
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
                GravenspireM3ObjectiveStateRelicHandInBuilder.Build();
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

                RecordCheck("scene_loaded", scene.IsValid() && scene.path == ScenePath);
                RecordCheck("m3_caretaker_anchor_exists", GameObject.Find(M3NamedNpcObjectiveFrame.AnchorObjectName) != null);
                RecordCheck("m3_objective_component_exists", FindObjectiveComponentIncludingInactive() != null);
                RecordCheck("exactly_one_m3_objective_component", CountObjectiveComponentsIncludingInactive() == 1);
                RecordCheck("m3_relic_object_authored", FindSceneObjectIncludingInactive(M3ObjectiveStateRelicHandInSession.RelicObjectName) != null);
                RecordCheck("m3_relic_starts_unavailable", IsRelicInactive());
                RecordCheck("m3_relic_has_trigger_collider", HasRelicTriggerCollider());
                RecordCheck("no_marker_affordance_components", HasNoForbiddenRelicAffordances());
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
            var objective = UnityEngine.Object.FindFirstObjectByType<M3ObjectiveStateRelicHandIn>();
            RecordCheck("objective_component_found_in_play_mode", objective != null);
            if (objective is null)
            {
                AppendSessionLine(ErrorsKey, "M3 objective state relic hand-in component was not found.");
                return;
            }

            var npc = UnityEngine.Object.FindFirstObjectByType<M3NamedNpcObjectiveFrame>();
            RecordCheck("m3_caretaker_component_found_in_play_mode", npc != null);
            if (npc is null)
            {
                AppendSessionLine(ErrorsKey, "M3 caretaker component was not found.");
                return;
            }

            npc.ClearSessionInteractions();
            objective.ResetSessionObjective();
            RecordCheck("initial_state_not_introduced", objective.State == M3ObjectiveState.NotIntroduced);
            RecordCheck("initial_relic_unavailable", !objective.RelicAvailable && !objective.RelicObjectActive);

            var playerMarker = GameObject.Find("ClericShellMarker");
            RecordCheck("cleric_marker_found_for_objective_interaction", playerMarker != null);
            if (playerMarker is null)
            {
                AppendSessionLine(ErrorsKey, "ClericShellMarker was not found for M3 objective interaction smoke.");
                return;
            }

            playerMarker.transform.position = npc.transform.position + new Vector3(0.0f, 0.0f, -1.25f);
            var distance = Vector3.Distance(playerMarker.transform.position, npc.transform.position);
            var accepted = objective.TryAcceptObjectiveFromNpc(npc, M3NamedNpcObjectiveFrame.PlayerActorId, distance);
            RecordCheck("objective_accept_transition_recorded", accepted && objective.State == M3ObjectiveState.Accepted);
            RecordCheck("relic_available_after_accept", objective.RelicAvailable && objective.RelicObjectActive);

            var recovered = objective.TryRecoverRelic();
            RecordCheck("relic_recovery_transition_recorded", recovered && objective.State == M3ObjectiveState.RelicRecovered);
            RecordCheck("session_carried_relic_recorded", objective.CarriesRelic);
            RecordCheck("relic_unavailable_after_recovery", !objective.RelicAvailable && !objective.RelicObjectActive);

            var handedIn = objective.TryReturnRelicToNpc(npc);
            RecordCheck("npc_relic_hand_in_recorded", handedIn);
            RecordCheck("objective_complete_after_hand_in", objective.IsComplete && objective.State == M3ObjectiveState.Complete);
            RecordCheck("state_sequence_exact", objective.StateSequence == "NotIntroduced -> Accepted -> RelicRecovered -> Complete");
            RecordCheck("transition_count_exact", objective.TransitionCount == 3);
            RecordCheck("objective_session_local_only", objective.SessionLocalOnly);
            RecordCheck("no_objective_rejection_reason", string.IsNullOrWhiteSpace(objective.LastRejectionReason));

            AppendSessionLine(TelemetryKey, $"objective_state_sequence={objective.StateSequence}");
            AppendSessionLine(TelemetryKey, $"relic_available_after_accept={accepted && objective.TransitionCount >= 1}");
            AppendSessionLine(TelemetryKey, $"session_carried_relic_recorded={recovered}");
            AppendSessionLine(TelemetryKey, $"relic_handed_in={handedIn}");
            AppendSessionLine(TelemetryKey, $"objective_complete={objective.IsComplete}");
            AppendSessionLine(TelemetryKey, $"session_local_only={objective.SessionLocalOnly}");

            var controller = UnityEngine.Object.FindFirstObjectByType<M2SingleTrashMedLoopController>();
            RecordCheck("m2_controller_found_after_objective_changes", controller != null);
            if (controller is null)
            {
                AppendSessionLine(ErrorsKey, "M2 combat loop controller was not found after objective state changes.");
                return;
            }

            RecordCheck("m2_controller_initialized_after_objective_changes", controller.IsInitialized);
            RecordCheck("m2_linked_anchor_available_after_objective_changes", controller.LinkedTrashArrangementPresent);
            RecordCheck("m2_named_blocker_anchor_available_after_objective_changes", controller.NamedBlockerAnchorPresent);
            AppendSessionLine(M2TelemetryKey, "full_m2_preservation=external_runner_reruns_under_tests/evidence/S2-M3-02");
            AppendSessionLine(M2TelemetryKey, $"m2_controller_initialized={controller.IsInitialized}");
            AppendSessionLine(M2TelemetryKey, $"m2_linked_anchor_available={controller.LinkedTrashArrangementPresent}");
            AppendSessionLine(M2TelemetryKey, $"m2_named_blocker_anchor_available={controller.NamedBlockerAnchorPresent}");

            foreach (var error in controller.Errors)
            {
                AppendSessionLine(ErrorsKey, error);
            }
        }

        private static M3ObjectiveStateRelicHandIn? FindObjectiveComponentIncludingInactive()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<M3ObjectiveStateRelicHandIn>(includeInactive: true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static int CountObjectiveComponentsIncludingInactive()
        {
            var count = 0;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                count += root.GetComponentsInChildren<M3ObjectiveStateRelicHandIn>(includeInactive: true).Length;
            }

            return count;
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

        private static bool IsRelicInactive()
        {
            var relic = FindSceneObjectIncludingInactive(M3ObjectiveStateRelicHandInSession.RelicObjectName);
            return relic != null && !relic.activeSelf;
        }

        private static bool HasRelicTriggerCollider()
        {
            var relic = FindSceneObjectIncludingInactive(M3ObjectiveStateRelicHandInSession.RelicObjectName);
            return relic != null && relic.TryGetComponent<Collider>(out var collider) && collider.isTrigger;
        }

        private static bool HasNoForbiddenRelicAffordances()
        {
            var relic = FindSceneObjectIncludingInactive(M3ObjectiveStateRelicHandInSession.RelicObjectName);
            if (relic == null)
            {
                return false;
            }

            foreach (var transform in relic.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                var lowerName = transform.name.ToLowerInvariant();
                if (lowerName.IndexOf("quest", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("nameplate", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("overhead", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("glow", StringComparison.Ordinal) >= 0 ||
                    lowerName.IndexOf("outline", StringComparison.Ordinal) >= 0)
                {
                    return false;
                }
            }

            return relic.GetComponentInChildren<Canvas>(includeInactive: true) == null &&
                relic.GetComponentInChildren<TextMesh>(includeInactive: true) == null &&
                relic.GetComponentInChildren<AudioSource>(includeInactive: true) == null &&
                relic.GetComponentInChildren<Light>(includeInactive: true) == null &&
                relic.GetComponentInChildren<LineRenderer>(includeInactive: true) == null &&
                relic.GetComponentInChildren<ParticleSystem>(includeInactive: true) == null;
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
            builder.AppendLine($"# {StoryId} Unity Objective State Relic Hand-In Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine($"**Story:** `production/stories/{StorySlug}.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireM3ObjectiveStateRelicHandInVerificationRunner.cs`");
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
            builder.AppendLine("## Objective Telemetry");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(TelemetryKey));
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
            Debug.Log($"{StoryId} objective state relic hand-in verification wrote {evidencePath} with exit code {exitCode}.");
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
                $"unity-objective-state-relic-hand-in-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-smoke.md");
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
            SessionState.EraseString(M2TelemetryKey);
            SessionState.EraseString(EvidencePathKey);
            SessionState.EraseString(PlayStartedKey);
        }
    }
}
#endif
