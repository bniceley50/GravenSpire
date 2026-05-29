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
    public static class GravenspireM2NamedBlockerVerificationRunner
    {
        private const string StoryId = "S2-M2-04";
        private const string StorySlug = "s2-m2-04-named-blocker-camp-boundary";
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string RunKey = "GravenspireM2NamedBlocker.Run";
        private const string PhaseKey = "GravenspireM2NamedBlocker.Phase";
        private const string ChecksKey = "GravenspireM2NamedBlocker.Checks";
        private const string ErrorsKey = "GravenspireM2NamedBlocker.Errors";
        private const string EventsKey = "GravenspireM2NamedBlocker.Events";
        private const string NamedBlockerEventsKey = "GravenspireM2NamedBlocker.NamedBlockerEvents";
        private const string WarningsKey = "GravenspireM2NamedBlocker.Warnings";
        private const string TelemetryKey = "GravenspireM2NamedBlocker.Telemetry";
        private const string EvidencePathKey = "GravenspireM2NamedBlocker.EvidencePath";
        private const string PlayStartedKey = "GravenspireM2NamedBlocker.PlayStartedTicks";
        private const string PreservationModeKey = "GravenspireM2NamedBlocker.PreservationMode";
        private const string BuilderSkippedKey = "GravenspireM2NamedBlocker.BuilderSkipped";
        private const string BuilderInvokedKey = "GravenspireM2NamedBlocker.BuilderInvoked";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const string PreservationModeArgumentName = "-gravenspirePreservationMode";
        private const string SkipBuilderArgumentName = "-gravenspireSkipBuilder";
        private const double SmokeDelaySeconds = 1.0;

        static GravenspireM2NamedBlockerVerificationRunner()
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

        [MenuItem("Gravenspire/Verify M2 Named Blocker Camp Boundary")]
        public static void Run()
        {
            ClearSession();
            SessionState.SetBool(RunKey, true);
            SessionState.SetString(PhaseKey, "initial");
            Application.logMessageReceived += CaptureLog;

            try
            {
                var evidencePath = ResolveEvidencePathFromCommandLine(DefaultEvidencePath());
                var preservationMode = IsCommandLineFlagPresent(PreservationModeArgumentName);
                var builderSkipped = IsCommandLineFlagPresent(SkipBuilderArgumentName);
                SessionState.SetString(EvidencePathKey, evidencePath);
                SessionState.SetBool(PreservationModeKey, preservationMode);
                SessionState.SetBool(BuilderSkippedKey, builderSkipped);
                SessionState.SetBool(BuilderInvokedKey, false);
                Directory.CreateDirectory(Path.GetDirectoryName(evidencePath) ?? ".");
                if (preservationMode && !builderSkipped)
                {
                    RecordCheck("preservation_mode_requires_skip_builder", false);
                    AppendSessionLine(ErrorsKey, "Preservation mode requires -gravenspireSkipBuilder; no builder call was executed.");
                    WriteEvidenceAndExit(1);
                    return;
                }

                RecordCheck("preservation_mode_requires_skip_builder", true);
                if (!builderSkipped)
                {
                    SessionState.SetBool(BuilderInvokedKey, true);
                    GravenspireM2SingleTrashLoopBuilder.Build();
                }

                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                RecordCheck("scene_loaded", scene.IsValid() && scene.path == ScenePath);
                RecordCheck("camp_rest_point_exists", GameObject.Find("M2_CampRestPoint") != null);
                RecordCheck("pull_lane_exists", GameObject.Find("M2_PullLane") != null);
                RecordCheck("cleric_marker_exists", GameObject.Find("ClericShellMarker") != null);
                RecordCheck("baseline_trash_anchor_exists", GameObject.Find("M2_BaselineTrash") != null);
                RecordCheck("linked_trash_anchor_exists", GameObject.Find("M2_LinkedTrash") != null);
                RecordCheck("named_blocker_anchor_exists", GameObject.Find("M2_NamedBlocker") != null);
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
                AppendSessionLine(ErrorsKey, "M2 named-blocker loop controller was not found.");
                return;
            }

            var smokeResult = controller.RunAutomatedNamedBlockerBoundarySmoke();
            RecordCheck("named_blocker_boundary_smoke_completed", smokeResult);
            RecordCheck("named_blocker_anchor_present", controller.NamedBlockerAnchorPresent);
            RecordCheck("named_blocker_present_and_targetable", controller.NamedBlockerPresentAndTargetable);
            RecordCheck("named_blocker_distinct_from_trash_fixture", controller.NamedBlockerDistinctFromTrashFixture);
            RecordCheck("named_blocker_boundary_outcome_recorded", controller.NamedBlockerBoundaryOutcomeRecorded);
            RecordCheck("named_blocker_not_farmable_trash", controller.NamedBlockerNotFarmableTrash);
            RecordCheck("clean_single_trash_loop_preserved", controller.CleanSingleTrashLoopPreservedAfterNamedBlocker);
            RecordCheck("no_controller_errors", controller.Errors.Count == 0);

            AppendSessionLine(TelemetryKey, $"named_blocker_outcome={controller.NamedBlockerOutcome}");
            AppendSessionLine(TelemetryKey, $"named_hostile_fixture={controller.NamedBlockerHostileFixtureId}");
            AppendSessionLine(TelemetryKey, $"named_max_health={controller.NamedBlockerMaxNamedHealth}");
            AppendSessionLine(TelemetryKey, $"baseline_trash_max_health={controller.NamedBlockerBaselineTrashMaxHealth}");
            AppendSessionLine(TelemetryKey, $"time_to_danger_seconds={controller.NamedBlockerTimeToDangerSeconds:0.00}");
            AppendSessionLine(TelemetryKey, $"ending_health={controller.NamedBlockerEndingHealth}/{controller.NamedBlockerMaxHealth}");
            AppendSessionLine(TelemetryKey, $"ending_mana={controller.NamedBlockerEndingMana}/{controller.NamedBlockerMaxMana}");
            AppendSessionLine(TelemetryKey, $"named_ending_health={controller.NamedBlockerEndingNamedHealth}/{controller.NamedBlockerMaxNamedHealth}");
            AppendSessionLine(TelemetryKey, $"clean_loop_preserved={controller.CleanSingleTrashLoopPreservedAfterNamedBlocker}");

            foreach (var namedBlockerEvent in controller.NamedBlockerEvents)
            {
                AppendSessionLine(NamedBlockerEventsKey, namedBlockerEvent);
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
            builder.AppendLine($"# {StoryId} Unity Named-Blocker Camp-Boundary Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine($"**Story:** `production/stories/{StorySlug}.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireM2NamedBlockerVerificationRunner.cs`");
            builder.AppendLine($"**Result:** {(exitCode == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine($"**Preservation Mode:** {EvidenceBool(CurrentPreservationMode())}");
            builder.AppendLine($"**Builder Skipped:** {EvidenceBool(CurrentBuilderSkipped())}");
            builder.AppendLine($"**Builder Invoked:** {EvidenceBool(CurrentBuilderInvoked())}");
            builder.AppendLine();
            builder.AppendLine("## Evidence Metadata");
            builder.AppendLine();
            builder.AppendLine($"- preservation_mode={EvidenceBool(CurrentPreservationMode())}");
            builder.AppendLine($"- builder_skipped={EvidenceBool(CurrentBuilderSkipped())}");
            builder.AppendLine($"- builder_invoked={EvidenceBool(CurrentBuilderInvoked())}");
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
            builder.AppendLine("## Named Blocker Telemetry");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(TelemetryKey));
            builder.AppendLine();
            builder.AppendLine("## Named Blocker Events");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(NamedBlockerEventsKey));
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
            Debug.Log($"{StoryId} named-blocker camp-boundary verification wrote {evidencePath} with exit code {exitCode}.");
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
                $"unity-named-blocker-runner-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-smoke.md");
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

        private static bool IsCommandLineFlagPresent(string argumentName)
        {
            var arguments = Environment.GetCommandLineArgs();
            for (var i = 0; i < arguments.Length; i++)
            {
                if (string.Equals(arguments[i], argumentName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CurrentPreservationMode()
        {
            return SessionState.GetBool(PreservationModeKey, false);
        }

        private static bool CurrentBuilderSkipped()
        {
            return SessionState.GetBool(BuilderSkippedKey, false);
        }

        private static bool CurrentBuilderInvoked()
        {
            return SessionState.GetBool(BuilderInvokedKey, false);
        }

        private static string EvidenceBool(bool value)
        {
            return value ? "true" : "false";
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
            SessionState.EraseString(NamedBlockerEventsKey);
            SessionState.EraseString(WarningsKey);
            SessionState.EraseString(TelemetryKey);
            SessionState.EraseString(EvidencePathKey);
            SessionState.EraseString(PlayStartedKey);
            SessionState.EraseBool(PreservationModeKey);
            SessionState.EraseBool(BuilderSkippedKey);
            SessionState.EraseBool(BuilderInvokedKey);
        }
    }
}
#endif
