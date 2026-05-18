#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gravenspire.UnityRuntime.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    [InitializeOnLoad]
    public static class GravenspireM2CombatBridgeVerificationRunner
    {
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string DefaultEvidencePath = "tests/evidence/S2-M2-01/unity-combat-bridge-smoke-20260510.md";
        private const string RunKey = "GravenspireM2CombatBridge.Run";
        private const string PhaseKey = "GravenspireM2CombatBridge.Phase";
        private const string ChecksKey = "GravenspireM2CombatBridge.Checks";
        private const string ErrorsKey = "GravenspireM2CombatBridge.Errors";
        private const string WarningsKey = "GravenspireM2CombatBridge.Warnings";
        private const string BridgeSummaryKey = "GravenspireM2CombatBridge.Summary";
        private const string PlayStartedKey = "GravenspireM2CombatBridge.PlayStartedTicks";
        private const string EvidencePathKey = "GravenspireM2CombatBridge.EvidencePath";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const double SmokeDelaySeconds = 2.0;

        static GravenspireM2CombatBridgeVerificationRunner()
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

        [MenuItem("Gravenspire/Verify M2 Combat Bridge")]
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
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                RecordCheck("scene_loaded", scene.IsValid() && scene.path == ScenePath);
                RecordCheck("dev_entry_scene_unchanged_entrypoint", scene.path == ScenePath);
                RecordCheck("floor_exists", GameObject.Find("DevEntry_DistrictBlockout_Floor") != null);
                RecordCheck("cleric_marker_exists", GameObject.Find("ClericShellMarker") != null);

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
                SessionState.SetString(PlayStartedKey, DateTime.UtcNow.Ticks.ToString());
                return;
            }

            if (phase != "playing")
            {
                return;
            }

            if (!long.TryParse(SessionState.GetString(PlayStartedKey, "0"), out var startedTicks) || startedTicks <= 0)
            {
                AppendSessionLine(ErrorsKey, "Play Mode start time was not recorded.");
                WriteEvidenceAndExit(1);
                return;
            }

            if ((DateTime.UtcNow - new DateTime(startedTicks, DateTimeKind.Utc)).TotalSeconds < SmokeDelaySeconds)
            {
                return;
            }

            RecordBridgeChecks();
            WriteEvidenceAndExit(AllChecksPassed() && GetSessionLines(ErrorsKey).Count == 0 ? 0 : 1);
        }

        private static void RecordBridgeChecks()
        {
            var bridge = UnityEngine.Object.FindFirstObjectByType<M2CombatCoreRuntimeBridge>();
            RecordCheck("bridge_object_exists", bridge != null);
            if (bridge is null)
            {
                SessionState.SetString(BridgeSummaryKey, "M2 combat bridge object was not found.");
                return;
            }

            bridge.HydrateBridge();

            RecordCheck("bridge_component_enabled", bridge.enabled && bridge.gameObject.activeInHierarchy);
            RecordCheck("bridge_hydrated", bridge.IsHydrated);
            RecordCheck("active_scene_recorded", string.Equals(SceneManager.GetActiveScene().name, "_DevEntry", StringComparison.Ordinal));
            RecordCheck("active_zone_id_recorded", string.Equals(bridge.ActiveZoneId, "Haunt_Prototype_T1", StringComparison.Ordinal));
            RecordCheck("fixture_set_version_recorded", !string.IsNullOrWhiteSpace(bridge.FixtureSetVersion));
            RecordCheck("solo_trash_fixture_recorded", bridge.EncounterFixtureIds.Contains("SoloTrash_EvenCon_T1"));
            RecordCheck("player_actor_recorded", !string.IsNullOrWhiteSpace(bridge.PlayerActorId));
            RecordCheck("hostile_actor_recorded", bridge.HostileActorIds.Count > 0);
            RecordCheck("fixture_file_from_data_directory", bridge.FixtureFilePath.Replace('\\', '/').EndsWith("data/combat/t1-combat-fixtures.json", StringComparison.Ordinal));

            foreach (var error in bridge.Errors)
            {
                AppendSessionLine(ErrorsKey, error);
            }

            SessionState.SetString(
                BridgeSummaryKey,
                "Object: " + bridge.gameObject.name + Environment.NewLine +
                "Scene: " + SceneManager.GetActiveScene().path + Environment.NewLine +
                "Active zone: " + bridge.ActiveZoneId + Environment.NewLine +
                "Fixture file: " + bridge.FixtureFilePath + Environment.NewLine +
                "Fixture set: " + bridge.FixtureSetVersion + Environment.NewLine +
                "Encounter fixtures: " + string.Join(", ", bridge.EncounterFixtureIds) + Environment.NewLine +
                "Actor fixtures: " + string.Join(", ", bridge.ActorFixtureIds) + Environment.NewLine +
                "Player actor: " + bridge.PlayerActorId + Environment.NewLine +
                "Hostile actors: " + string.Join(", ", bridge.HostileActorIds));
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
            builder.AppendLine("# S2-M2-01 Unity Combat Bridge Smoke");
            builder.AppendLine();
            builder.AppendLine("**Date:** 2026-05-10");
            builder.AppendLine("**Story:** `production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireM2CombatBridgeVerificationRunner.cs`");
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

            builder.AppendLine($"- {(GetSessionLines(ErrorsKey).Count == 0 ? "PASS" : "FAIL")} `no_errors_or_exceptions`");
            builder.AppendLine();
            builder.AppendLine("## Bridge Summary");
            builder.AppendLine();
            builder.AppendLine(SessionState.GetString(BridgeSummaryKey, "Bridge summary was not recorded."));
            builder.AppendLine();
            builder.AppendLine("## Bootstrap Note");
            builder.AppendLine();
            builder.AppendLine("The bridge uses `RuntimeInitializeOnLoadMethod` and intentionally does not modify `Assets/Scenes/_DevEntry.unity`.");
            builder.AppendLine("Future multi-scene work may need a narrower activation predicate; S2-M2-01 keeps the bridge global for the single-scene T1 shell.");
            builder.AppendLine();
            builder.AppendLine("## Warnings");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(WarningsKey));
            builder.AppendLine();
            builder.AppendLine("## Errors");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(ErrorsKey));

            File.WriteAllText(evidencePath, builder.ToString());
            Debug.Log($"S2-M2-01 combat bridge verification wrote {evidencePath} with exit code {exitCode}.");
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
            SessionState.EraseString(BridgeSummaryKey);
            SessionState.EraseString(PlayStartedKey);
            SessionState.EraseString(EvidencePathKey);
        }
    }
}
#endif
