#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Gravenspire.UnityRuntime.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    [InitializeOnLoad]
    public static class GravenspireM2LinkedTrashOverpullVerificationRunner
    {
        private const string StoryId = "S2-M2-03";
        private const string StorySlug = "s2-m2-03-linked-trash-overpull";
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string RunKey = "GravenspireM2LinkedTrashOverpull.Run";
        private const string PhaseKey = "GravenspireM2LinkedTrashOverpull.Phase";
        private const string ChecksKey = "GravenspireM2LinkedTrashOverpull.Checks";
        private const string ErrorsKey = "GravenspireM2LinkedTrashOverpull.Errors";
        private const string EventsKey = "GravenspireM2LinkedTrashOverpull.Events";
        private const string OverpullEventsKey = "GravenspireM2LinkedTrashOverpull.OverpullEvents";
        private const string WarningsKey = "GravenspireM2LinkedTrashOverpull.Warnings";
        private const string TelemetryKey = "GravenspireM2LinkedTrashOverpull.Telemetry";
        private const string EvidencePathKey = "GravenspireM2LinkedTrashOverpull.EvidencePath";
        private const string PlayStartedKey = "GravenspireM2LinkedTrashOverpull.PlayStartedTicks";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const double SmokeDelaySeconds = 1.0;

        static GravenspireM2LinkedTrashOverpullVerificationRunner()
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

        [MenuItem("Gravenspire/Verify M2 Linked Trash Overpull")]
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
                GravenspireM2SingleTrashLoopBuilder.Build();
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                RecordCheck("scene_loaded", scene.IsValid() && scene.path == ScenePath);
                RecordCheck("camp_rest_point_exists", GameObject.Find("M2_CampRestPoint") != null);
                RecordCheck("pull_lane_exists", GameObject.Find("M2_PullLane") != null);
                RecordCheck("cleric_marker_exists", GameObject.Find("ClericShellMarker") != null);
                RecordCheck("baseline_trash_anchor_exists", GameObject.Find("M2_BaselineTrash") != null);
                RecordCheck("linked_trash_anchor_exists", GameObject.Find("M2_LinkedTrash") != null);
                RecordCheck("loop_controller_exists", GameObject.Find("M2_CombatCampLoopRoot")?.GetComponent<M2SingleTrashMedLoopController>() != null);

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
                SessionState.SetString(PlayStartedKey, DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (phase != "playing")
            {
                return;
            }

            if (!long.TryParse(SessionState.GetString(PlayStartedKey, "0"), out var startedTicks) ||
                (DateTime.UtcNow - new DateTime(startedTicks, DateTimeKind.Utc)).TotalSeconds < SmokeDelaySeconds)
            {
                return;
            }

            RunSmokeChecks();
            WriteEvidenceAndExit(AllChecksPassed() && GetSessionLines(ErrorsKey).Count == 0 ? 0 : 1);
        }

        private static void RunSmokeChecks()
        {
            var controller = UnityEngine.Object.FindFirstObjectByType<M2SingleTrashMedLoopController>();
            RecordCheck("controller_found_in_play_mode", controller != null);
            if (controller is null)
            {
                AppendSessionLine(ErrorsKey, "M2 linked-trash loop controller was not found.");
                return;
            }

            var smokeResult = controller.RunAutomatedLinkedTrashOverpullSmoke();
            RecordCheck("linked_overpull_smoke_completed", smokeResult);
            RecordCheck("linked_trash_arrangement_present", controller.LinkedTrashArrangementPresent);
            RecordCheck("two_hostiles_entered_hate", controller.OverpullHostilesInHate >= 2);
            RecordCheck("feel03_hate_window_met", controller.LinkedTrashEnteredHateWithinFeelWindow);
            RecordCheck("dangerous_outcome_recorded", controller.OverpullDangerousOutcomeRecorded);
            RecordCheck("clean_single_trash_loop_preserved", controller.CleanSingleTrashLoopPreservedAfterOverpull);
            RecordCheck("no_controller_errors", controller.Errors.Count == 0);

            AppendSessionLine(TelemetryKey, $"hostiles_in_hate={controller.OverpullHostilesInHate}");
            AppendSessionLine(TelemetryKey, $"hate_window_seconds={controller.LinkedTrashHateWindowSeconds:0.0}");
            AppendSessionLine(TelemetryKey, $"overpull_outcome={controller.OverpullOutcome}");
            AppendSessionLine(TelemetryKey, $"ending_health={controller.OverpullEndingHealth}/{controller.OverpullMaxHealth}");
            AppendSessionLine(TelemetryKey, $"ending_mana={controller.OverpullEndingMana}/{controller.OverpullMaxMana}");
            AppendSessionLine(TelemetryKey, $"clean_loop_preserved={controller.CleanSingleTrashLoopPreservedAfterOverpull}");

            foreach (var overpullEvent in controller.OverpullEvents)
            {
                AppendSessionLine(OverpullEventsKey, overpullEvent);
            }

            foreach (var loopEvent in controller.Events)
            {
                AppendSessionLine(EventsKey, loopEvent);
            }

            foreach (var error in controller.Errors)
            {
                AppendSessionLine(ErrorsKey, error);
            }
        }

        private static void CaptureLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                if (GravenspireScenarioSmokeRunnerSupport.IsDiagnosedEditorStartupNoise(stackTrace))
                {
                    return;
                }

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
            builder.AppendLine($"# {StoryId} Unity Linked-Trash Overpull Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine($"**Story:** `production/stories/{StorySlug}.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireM2LinkedTrashOverpullVerificationRunner.cs`");
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
            builder.AppendLine("## Overpull Telemetry");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(TelemetryKey));
            builder.AppendLine();
            builder.AppendLine("## Overpull Events");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(OverpullEventsKey));
            builder.AppendLine();
            builder.AppendLine("## Clean Loop Events");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(EventsKey));
            builder.AppendLine();
            builder.AppendLine("## Warnings");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(WarningsKey));
            builder.AppendLine();
            builder.AppendLine("## Errors");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(ErrorsKey));

            File.WriteAllText(evidencePath, builder.ToString());
            Debug.Log($"{StoryId} linked-trash overpull verification wrote {evidencePath} with exit code {exitCode}.");
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
                $"unity-linked-trash-overpull-runner-{DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-smoke.md");
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
            SessionState.EraseString(OverpullEventsKey);
            SessionState.EraseString(WarningsKey);
            SessionState.EraseString(TelemetryKey);
            SessionState.EraseString(EvidencePathKey);
            SessionState.EraseString(PlayStartedKey);
        }
    }
}
#endif
