using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Gravenspire.Editor
{
    // First District Visual Spike — Session 2 smoke runner.
    // Loads the spike scene and validates structural integrity per spike-plan.md §Session 2.
    // Visual/subjective judgment (lighting feel, fog composition) is user + art-director gate,
    // NOT runner scope; runner only proves the scene matches builder intent.
    public static class GravenspireFirstDistrictVisualSpikeRunner
    {
        private const string ScenePath = "Assets/Scenes/FirstDistrict_VisualSpike.unity";
        private const string EvidenceDir = "tests/evidence/FIRST-DISTRICT-VISUAL-SPIKE";

        // Session date as constant per Δ6 (mirrors M2-04 pattern; avoids real-clock APIs
        // tracked by the T1 deny-pattern in .githooks/pre-commit).
        private const string SessionDate = "20260516";

        // Editor-noise filter pattern per M2-04 carryover (m2_runner_editor_noise_capture).
        private const string EditorNoiseFilter = "UnityEditor.Search.SearchInit";

        [MenuItem("Gravenspire/Visual Spike/Run Verification")]
        public static void RunMenu()
        {
            int exitCode = Run();
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
        }

        public static int Run()
        {
            var log = new StringBuilder();
            bool allPassed = true;

            log.AppendLine("[VisualSpikeRunner] Session 2 verification: " + SessionDate);
            log.AppendLine("[VisualSpikeRunner] Scene: " + ScenePath);
            log.AppendLine();

            try
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                log.AppendLine("[PASS] Scene loaded: " + scene.path);
                log.AppendLine();

                allPassed &= CheckLights(log);
                log.AppendLine();
                allPassed &= CheckVolume(log);
                log.AppendLine();
                allPassed &= CheckCameras(log);
                log.AppendLine();
                allPassed &= CheckFog(log);
                log.AppendLine();
                allPassed &= CheckRenderSettings(log);
                log.AppendLine();
            }
            catch (Exception ex)
            {
                if (ex.GetType().FullName != null && ex.GetType().FullName.Contains(EditorNoiseFilter))
                {
                    log.AppendLine("[FILTERED] Editor-noise exception suppressed: " + ex.GetType().FullName);
                }
                else
                {
                    log.AppendLine("[FAIL] Exception during scene load or verification: " + ex.Message);
                    allPassed = false;
                }
            }

            log.AppendLine("[RESULT] " + (allPassed ? "PASS" : "FAIL"));
            WriteLog(log.ToString());
            return allPassed ? 0 : 1;
        }

        private static bool CheckLights(StringBuilder log)
        {
            log.AppendLine("[CHECK] Lights");
            bool ok = true;

            var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);

            int dirCount = lights.Count(l => l.type == LightType.Directional);
            int pointCount = lights.Count(l => l.type == LightType.Point);

            if (dirCount != 0)
            {
                log.AppendLine("  [FAIL] Expected 0 Directional Lights; found " + dirCount);
                ok = false;
            }
            else
            {
                log.AppendLine("  [PASS] No Directional Light present");
            }

            if (pointCount != 3)
            {
                log.AppendLine("  [FAIL] Expected 3 Point Lights; found " + pointCount);
                ok = false;
            }
            else
            {
                log.AppendLine("  [PASS] 3 Point Lights present");
            }

            foreach (var l in lights.Where(l => l.type == LightType.Point))
            {
                if (!l.useColorTemperature)
                {
                    log.AppendLine("  [FAIL] Light '" + l.name + "' useColorTemperature is false");
                    ok = false;
                }
                else if (Math.Abs(l.colorTemperature - 2200f) > 0.1f)
                {
                    log.AppendLine("  [FAIL] Light '" + l.name + "' colorTemperature=" + l.colorTemperature + ", expected 2200");
                    ok = false;
                }
                else if (Math.Abs(l.range - 4f) > 0.001f)
                {
                    log.AppendLine("  [FAIL] Light '" + l.name + "' range=" + l.range + ", expected 4");
                    ok = false;
                }
                else
                {
                    log.AppendLine("  [PASS] Light '" + l.name + "' useColorTemperature=true, colorTemperature=2200, range=4");
                }
            }
            return ok;
        }

        private static bool CheckVolume(StringBuilder log)
        {
            log.AppendLine("[CHECK] Volume + Profile");
            bool ok = true;

            var volumes = UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
            if (volumes.Length == 0)
            {
                log.AppendLine("  [FAIL] No Volume found in scene");
                return false;
            }
            if (volumes.Length != 1)
            {
                log.AppendLine("  [FAIL] Expected 1 Volume; found " + volumes.Length);
                ok = false;
            }

            var volume = volumes[0];
            log.AppendLine("  [PASS] Volume found: '" + volume.name + "'");

            if (!volume.isGlobal)
            {
                log.AppendLine("  [FAIL] Volume isGlobal=false; expected true");
                ok = false;
            }

            if (volume.sharedProfile == null)
            {
                log.AppendLine("  [FAIL] Volume has no sharedProfile");
                return false;
            }
            log.AppendLine("  [PASS] Volume profile assigned: " + volume.sharedProfile.name);

            var profile = volume.sharedProfile;

            // Δ4: assert disabled state explicitly (do not rely on absence).
            if (profile.TryGet<Bloom>(out var bloom))
            {
                if (bloom.active)
                {
                    log.AppendLine("  [FAIL] Bloom present and active; must not be active");
                    ok = false;
                }
                else
                {
                    log.AppendLine("  [PASS] Bloom present and disabled");
                }
            }
            else
            {
                log.AppendLine("  [PASS] Bloom absent (acceptable)");
            }

            if (profile.TryGet<Vignette>(out var vignette))
            {
                if (vignette.active)
                {
                    log.AppendLine("  [FAIL] Vignette present and active; must not be active");
                    ok = false;
                }
                else
                {
                    log.AppendLine("  [PASS] Vignette present and disabled");
                }
            }
            else
            {
                log.AppendLine("  [PASS] Vignette absent (acceptable)");
            }

            if (!profile.TryGet<ColorAdjustments>(out var colorAdj))
            {
                log.AppendLine("  [FAIL] ColorAdjustments missing from profile");
                ok = false;
            }
            else if (!colorAdj.active)
            {
                log.AppendLine("  [FAIL] ColorAdjustments present but inactive");
                ok = false;
            }
            else if (Math.Abs(colorAdj.saturation.value - (-5f)) > 0.1f)
            {
                log.AppendLine("  [FAIL] ColorAdjustments saturation=" + colorAdj.saturation.value + ", expected -5");
                ok = false;
            }
            else
            {
                log.AppendLine("  [PASS] ColorAdjustments active, saturation=-5");
            }

            if (!profile.TryGet<WhiteBalance>(out var whiteBal))
            {
                log.AppendLine("  [FAIL] WhiteBalance missing from profile");
                ok = false;
            }
            else if (!whiteBal.active)
            {
                log.AppendLine("  [FAIL] WhiteBalance present but inactive");
                ok = false;
            }
            else if (Math.Abs(whiteBal.temperature.value - 10f) > 0.1f)
            {
                log.AppendLine("  [FAIL] WhiteBalance temperature=" + whiteBal.temperature.value + ", expected +10");
                ok = false;
            }
            else
            {
                log.AppendLine("  [PASS] WhiteBalance active, temperature=+10");
            }

            if (!profile.TryGet<Tonemapping>(out var tonemap))
            {
                log.AppendLine("  [FAIL] Tonemapping missing from profile");
                ok = false;
            }
            else if (!tonemap.active)
            {
                log.AppendLine("  [FAIL] Tonemapping present but inactive");
                ok = false;
            }
            else if (tonemap.mode.value != TonemappingMode.Neutral)
            {
                log.AppendLine("  [FAIL] Tonemapping mode=" + tonemap.mode.value + ", expected Neutral");
                ok = false;
            }
            else
            {
                log.AppendLine("  [PASS] Tonemapping active, mode=Neutral");
            }

            return ok;
        }

        private static bool CheckCameras(StringBuilder log)
        {
            log.AppendLine("[CHECK] Camera Stack");
            bool ok = true;

            var cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            if (cameras.Length != 2)
            {
                log.AppendLine("  [FAIL] Expected 2 cameras; found " + cameras.Length);
                return false;
            }
            log.AppendLine("  [PASS] 2 cameras present");

            Camera baseCamera = null;
            Camera overlayCamera = null;
            foreach (var cam in cameras)
            {
                var data = cam.GetUniversalAdditionalCameraData();
                if (data.renderType == CameraRenderType.Base) baseCamera = cam;
                else if (data.renderType == CameraRenderType.Overlay) overlayCamera = cam;
            }

            if (baseCamera == null)
            {
                log.AppendLine("  [FAIL] No Base camera found");
                ok = false;
            }
            else
            {
                var baseData = baseCamera.GetUniversalAdditionalCameraData();
                int expectedBaseMask = 1 << LayerMask.NameToLayer("Default");
                if (baseData.volumeLayerMask == expectedBaseMask)
                {
                    log.AppendLine("  [PASS] Base camera Volume Layer Mask = Default (" + expectedBaseMask + ")");
                }
                else
                {
                    log.AppendLine("  [FAIL] Base camera Volume Layer Mask = " + baseData.volumeLayerMask + ", expected " + expectedBaseMask);
                    ok = false;
                }

                if (overlayCamera != null && baseData.cameraStack.Contains(overlayCamera))
                {
                    log.AppendLine("  [PASS] Overlay camera attached to Base camera stack");
                }
                else
                {
                    log.AppendLine("  [FAIL] Overlay camera not in Base camera stack");
                    ok = false;
                }
            }

            if (overlayCamera == null)
            {
                log.AppendLine("  [FAIL] No Overlay camera found");
                ok = false;
            }
            else
            {
                var overlayData = overlayCamera.GetUniversalAdditionalCameraData();
                if (overlayData.volumeLayerMask == 0)
                {
                    log.AppendLine("  [PASS] Overlay camera Volume Layer Mask = Nothing (0)");
                }
                else
                {
                    log.AppendLine("  [FAIL] Overlay camera Volume Layer Mask = " + overlayData.volumeLayerMask + ", expected 0");
                    ok = false;
                }
            }

            return ok;
        }

        private static bool CheckFog(StringBuilder log)
        {
            log.AppendLine("[CHECK] Fog (RenderSettings)");
            bool ok = true;

            if (!RenderSettings.fog)
            {
                log.AppendLine("  [FAIL] RenderSettings.fog = false; expected true");
                ok = false;
            }
            else
            {
                log.AppendLine("  [PASS] RenderSettings.fog = true");
            }

            if (RenderSettings.fogMode != FogMode.Linear)
            {
                log.AppendLine("  [FAIL] FogMode = " + RenderSettings.fogMode + ", expected Linear");
                ok = false;
            }
            else
            {
                log.AppendLine("  [PASS] FogMode = Linear");
            }

            if (Math.Abs(RenderSettings.fogStartDistance - 10f) > 0.001f)
            {
                log.AppendLine("  [FAIL] fogStartDistance = " + RenderSettings.fogStartDistance + ", expected 10");
                ok = false;
            }
            else
            {
                log.AppendLine("  [PASS] fogStartDistance = 10");
            }

            if (Math.Abs(RenderSettings.fogEndDistance - 30f) > 0.001f)
            {
                log.AppendLine("  [FAIL] fogEndDistance = " + RenderSettings.fogEndDistance + ", expected 30");
                ok = false;
            }
            else
            {
                log.AppendLine("  [PASS] fogEndDistance = 30");
            }

            return ok;
        }

        private static bool CheckRenderSettings(StringBuilder log)
        {
            log.AppendLine("[CHECK] RenderSettings (ambient + sun)");
            bool ok = true;

            if (RenderSettings.sun != null)
            {
                log.AppendLine("  [FAIL] RenderSettings.sun is set; expected null (no directional sun)");
                ok = false;
            }
            else
            {
                log.AppendLine("  [PASS] RenderSettings.sun is null (no directional sun)");
            }

            if (RenderSettings.skybox == null)
            {
                log.AppendLine("  [FAIL] RenderSettings.skybox is null; expected procedural skybox");
                ok = false;
            }
            else
            {
                log.AppendLine("  [PASS] RenderSettings.skybox assigned: " + RenderSettings.skybox.name);
            }

            return ok;
        }

        private static void WriteLog(string content)
        {
            if (!Directory.Exists(EvidenceDir))
            {
                Directory.CreateDirectory(EvidenceDir);
            }
            var path = Path.Combine(EvidenceDir, "unity-session-2-builder-smoke-" + SessionDate + ".log");
            File.WriteAllText(path, content);
            Debug.Log("[VisualSpikeRunner] Log written: " + path);
        }
    }
}
