#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    [InitializeOnLoad]
    public static class GravenspireLaunchVerificationRunner
    {
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string EvidencePath = "tests/evidence/S2-FOUNDATION-01/unity-cli-launch-verification-20260510.md";
        private const string RunKey = "GravenspireLaunchVerification.Run";
        private const string PhaseKey = "GravenspireLaunchVerification.Phase";
        private const string ChecksKey = "GravenspireLaunchVerification.Checks";
        private const string ErrorsKey = "GravenspireLaunchVerification.Errors";
        private const string WarningsKey = "GravenspireLaunchVerification.Warnings";
        private const string RenderSummaryKey = "GravenspireLaunchVerification.RenderSummary";
        private const string PlayStartedKey = "GravenspireLaunchVerification.PlayStartedTicks";
        private const double StabilitySeconds = 30.0;

        static GravenspireLaunchVerificationRunner()
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

        [MenuItem("Gravenspire/Verify Dev Entry Launch")]
        public static void Run()
        {
            ClearSession();
            SessionState.SetBool(RunKey, true);
            SessionState.SetString(PhaseKey, "initial");
            Application.logMessageReceived += CaptureLog;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(EvidencePath) ?? ".");
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                RecordCheck("scene_loaded", scene.IsValid() && scene.path == ScenePath);
                RecordCheck("floor_exists", GameObject.Find("DevEntry_DistrictBlockout_Floor") != null);
                RecordCheck("cleric_marker_exists", GameObject.Find("ClericShellMarker") != null);
                RecordCheck("district_marker_exists", GameObject.Find("FirstDistrict_ShellOnly_NoGameplay") != null);
                RecordCheck("camera_exists", GameObject.Find("DevEntryCamera")?.GetComponent<Camera>() != null);
                RecordCheck("light_exists", GameObject.Find("DevEntryDirectionalLight")?.GetComponent<Light>() != null);
                RecordCheck("camera_render_nonblank", TryRenderCamera());

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

            var elapsed = (DateTime.UtcNow - new DateTime(startedTicks, DateTimeKind.Utc)).TotalSeconds;
            if (elapsed < StabilitySeconds)
            {
                return;
            }

            RecordCheck("play_mode_stable_30s", true);
            RecordCheck("floor_exists_after_play", GameObject.Find("DevEntry_DistrictBlockout_Floor") != null);
            RecordCheck("cleric_marker_exists_after_play", GameObject.Find("ClericShellMarker") != null);
            RecordCheck("district_marker_exists_after_play", GameObject.Find("FirstDistrict_ShellOnly_NoGameplay") != null);
            WriteEvidenceAndExit(AllChecksPassed() && GetSessionLines(ErrorsKey).Count == 0 ? 0 : 1);
        }

        private static bool TryRenderCamera()
        {
            var camera = GameObject.Find("DevEntryCamera")?.GetComponent<Camera>();
            if (camera == null)
            {
                SessionState.SetString(RenderSummaryKey, "DevEntryCamera was missing.");
                return false;
            }

            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(128, 72, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(128, 72, TextureFormat.RGBA32, false);

            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                texture.Apply();

                var pixels = texture.GetPixels32();
                var first = pixels[0];
                var differingPixels = 0;
                var litPixels = 0;

                foreach (var pixel in pixels)
                {
                    if (Math.Abs(pixel.r - first.r) > 2 ||
                        Math.Abs(pixel.g - first.g) > 2 ||
                        Math.Abs(pixel.b - first.b) > 2)
                    {
                        differingPixels++;
                    }

                    if (pixel.r + pixel.g + pixel.b > 24)
                    {
                        litPixels++;
                    }
                }

                SessionState.SetString(
                    RenderSummaryKey,
                    $"Differing pixels: {differingPixels}/{pixels.Length}; lit pixels: {litPixels}/{pixels.Length}.");
                return differingPixels > 50 && litPixels > 50;
            }
            catch (Exception ex)
            {
                SessionState.SetString(RenderSummaryKey, ex.ToString());
                return false;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(texture);
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static void CaptureLog(string condition, string stackTrace, LogType type)
        {
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

            var builder = new StringBuilder();
            builder.AppendLine("# S2-FOUNDATION-01 Unity CLI Launch Verification");
            builder.AppendLine();
            builder.AppendLine("**Date:** 2026-05-10");
            builder.AppendLine("**Story:** `production/stories/s2-foundation-01-unity-project-shell.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireLaunchVerificationRunner.cs`");
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
            builder.AppendLine("## Render Check");
            builder.AppendLine();
            builder.AppendLine(SessionState.GetString(RenderSummaryKey, "Render check was not recorded."));
            builder.AppendLine();
            builder.AppendLine("## Warnings");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(WarningsKey));
            builder.AppendLine();
            builder.AppendLine("## Errors");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(ErrorsKey));

            File.WriteAllText(EvidencePath, builder.ToString());
            Debug.Log($"S2-FOUNDATION-01 CLI launch verification wrote {EvidencePath} with exit code {exitCode}.");
            ClearSession();
            EditorApplication.Exit(exitCode);
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
            SessionState.EraseString(RenderSummaryKey);
            SessionState.EraseString(PlayStartedKey);
        }
    }
}
#endif
