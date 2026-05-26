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
    public static class GravenspireM2SingleTrashLoopVerificationRunner
    {
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string DefaultEvidencePath = "tests/evidence/S2-M2-02/unity-single-trash-med-loop-smoke-20260512.md";
        private const string RunKey = "GravenspireM2SingleTrashLoop.Run";
        private const string PhaseKey = "GravenspireM2SingleTrashLoop.Phase";
        private const string ChecksKey = "GravenspireM2SingleTrashLoop.Checks";
        private const string ErrorsKey = "GravenspireM2SingleTrashLoop.Errors";
        private const string EventsKey = "GravenspireM2SingleTrashLoop.Events";
        private const string WarningsKey = "GravenspireM2SingleTrashLoop.Warnings";
        private const string EvidencePathKey = "GravenspireM2SingleTrashLoop.EvidencePath";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const string SkipBuilderArgumentName = "-gravenspireSkipBuilder";
        private const double SmokeDelaySeconds = 1.0;
        private static readonly DateTime RunDate = new DateTime(2026, 5, 12, 0, 0, 0, DateTimeKind.Utc);

        static GravenspireM2SingleTrashLoopVerificationRunner()
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

        [MenuItem("Gravenspire/Verify M2 Single Trash Loop")]
        public static void Run()
        {
            ClearSession();
            SessionState.SetBool(RunKey, true);
            SessionState.SetString(PhaseKey, "initial");
            Application.logMessageReceived += CaptureLog;

            try
            {
                var evidencePath = ResolveEvidencePathFromCommandLine(DefaultEvidencePath);
                SessionState.SetString(EvidencePathKey, evidencePath);
                Directory.CreateDirectory(Path.GetDirectoryName(evidencePath) ?? ".");
                if (!ShouldSkipBuilderFromCommandLine())
                {
                    GravenspireM2SingleTrashLoopBuilder.Build();
                }

                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                RecordCheck("scene_loaded", scene.IsValid() && scene.path == ScenePath);
                RecordCheck("camp_rest_point_exists", GameObject.Find("M2_CampRestPoint") != null);
                RecordCheck("pull_lane_exists", GameObject.Find("M2_PullLane") != null);
                RecordCheck("cleric_marker_exists", GameObject.Find("ClericShellMarker") != null);
                RecordCheck("baseline_trash_anchor_exists", GameObject.Find("M2_BaselineTrash") != null);
                RecordCheck("loop_controller_exists", GameObject.Find("M2_CombatCampLoopRoot")?.GetComponent<M2SingleTrashMedLoopController>() != null);

                SessionState.SetString(PhaseKey, "entering_play");
                SessionState.SetString("GravenspireM2SingleTrashLoop.StartTicks", RunDate.Ticks.ToString());
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
                SessionState.SetString("GravenspireM2SingleTrashLoop.PlayStartedTicks", DateTime.UtcNow.Ticks.ToString());
                return;
            }

            if (phase != "playing")
            {
                return;
            }

            if (!long.TryParse(SessionState.GetString("GravenspireM2SingleTrashLoop.PlayStartedTicks", "0"), out var startedTicks) ||
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
                AppendSessionLine(ErrorsKey, "M2 single-trash loop controller was not found.");
                return;
            }

            var smokeResult = controller.RunAutomatedTwoPullSmoke();
            RecordCheck("two_pull_med_loop_smoke_completed", smokeResult);
            RecordCheck("pull_start_recorded", controller.PullStarted);
            RecordCheck("pull_did_not_auto_enable_attack", controller.PullDidNotAutoEnableAttack);
            RecordCheck("attack_transition_recorded", controller.AttackTransitionRecorded);
            RecordCheck("hostile_defeat_recorded", controller.HostileDefeatRecorded);
            RecordCheck("combat_exit_recorded", controller.CombatExitRecorded);
            RecordCheck("sit_med_start_recorded", controller.SitMedStartRecorded);
            RecordCheck("mana_restoration_recorded", controller.ManaRestorationRecorded);
            RecordCheck("no_controller_errors", controller.Errors.Count == 0);

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
            builder.AppendLine("# S2-M2-02 Unity Single-Trash Med-Loop Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine("**Story:** `production/stories/s2-m2-02-single-trash-pull-med-loop.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireM2SingleTrashLoopVerificationRunner.cs`");
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
            builder.AppendLine("## Loop Events");
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
            Debug.Log($"S2-M2-02 single-trash med-loop verification wrote {evidencePath} with exit code {exitCode}.");
            ClearSession();
            EditorApplication.Exit(exitCode);
        }

        private static string CurrentEvidencePath()
        {
            var configuredPath = SessionState.GetString(EvidencePathKey, string.Empty);
            return string.IsNullOrWhiteSpace(configuredPath) ? DefaultEvidencePath : configuredPath;
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

        private static bool ShouldSkipBuilderFromCommandLine()
        {
            var arguments = Environment.GetCommandLineArgs();
            for (var i = 0; i < arguments.Length; i++)
            {
                if (string.Equals(arguments[i], SkipBuilderArgumentName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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
            SessionState.EraseString(WarningsKey);
            SessionState.EraseString(EvidencePathKey);
            SessionState.EraseString("GravenspireM2SingleTrashLoop.StartTicks");
            SessionState.EraseString("GravenspireM2SingleTrashLoop.PlayStartedTicks");
        }
    }
}
#endif
