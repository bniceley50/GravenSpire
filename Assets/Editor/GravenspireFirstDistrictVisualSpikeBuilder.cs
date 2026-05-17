using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Gravenspire.Editor
{
    // First District Visual Spike — Session 2 builder.
    // Creates Assets/Scenes/FirstDistrict_VisualSpike.unity with two-source gothic lighting
    // per design/art/art-bible.md §S2 State 1/2 + production/assets/specs/env_lighting_district_camp.md.
    // Reproducible programmatic scene construction; no hand-edited .unity YAML
    // (governance §Scene Discipline). Sibling scene; does NOT touch _DevEntry.unity.
    public static class GravenspireFirstDistrictVisualSpikeBuilder
    {
        private const string ScenePath = "Assets/Scenes/FirstDistrict_VisualSpike.unity";
        private const string VolumeProfileDir = "Assets/Settings/lighting";
        private const string VolumeProfilePath = "Assets/Settings/lighting/gv_district_state1.asset";
        private const string ScenesDir = "Assets/Scenes";

        // Courtyard dimensions per spike plan §Session 2 (~15x20m, 3 walls + open south).
        private const float CourtyardWidth = 15f;
        private const float CourtyardLength = 20f;
        private const float WallHeight = 4f;
        private const float WallThickness = 0.3f;

        // Lighting per env_lighting_district_camp.md:34, :51 (warm practical 2200K, ~6 lumens, 4m).
        private const float LanternColorTemp = 2200f;
        private const float LanternIntensity = 6f;
        private const float LanternRange = 4f;
        private const float LanternMountHeight = 2.4f;

        // Volume tuning per env_lighting_district_camp.md:28-32.
        private const float ColorAdjustmentsSaturation = -5f;
        private const float WhiteBalanceTemperature = 10f;

        // Fog per existing M2 controller pattern at
        // Assets/Scripts/M2SingleTrashMedLoopController.cs:2083-2087 (linear, 10-30m).
        // Session 1 pinned the empirical Volume+fog composition confirmation to Session 2.
        private const float FogStartDistance = 10f;
        private const float FogEndDistance = 30f;

        [MenuItem("Gravenspire/Visual Spike/Build First District Scene")]
        public static void BuildSceneMenu()
        {
            BuildScene();
        }

        // Public so batchmode -executeMethod can call this directly.
        public static void BuildScene()
        {
            EnsureDirectory(VolumeProfileDir);
            EnsureDirectory(ScenesDir);

            var profile = EnsureVolumeProfile();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var geometryRoot = new GameObject("Geometry");
            BuildGeometry(geometryRoot);

            var lightingRoot = new GameObject("Lighting");
            BuildLanterns(lightingRoot);

            var volumesRoot = new GameObject("Volumes");
            BuildVolume(volumesRoot, profile);

            var camerasRoot = new GameObject("Cameras");
            BuildCameras(camerasRoot);

            ConfigureRenderSettingsAndSkybox();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[VisualSpikeBuilder] Scene built and saved to " + ScenePath);
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static VolumeProfile EnsureVolumeProfile()
        {
            var existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            if (existing != null)
            {
                // Detect a broken profile from a prior buggy create flow (component refs
                // that did not survive serialization will load as nulls). Delete the asset
                // and recreate cleanly so AddObjectToAsset can run from a known-good base.
                bool hasBroken = existing.components.Exists(c => c == null);
                if (hasBroken)
                {
                    AssetDatabase.DeleteAsset(VolumeProfilePath);
                }
                else
                {
                    ConfigureVolumeProfile(existing);
                    return existing;
                }
            }

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            // Persist the profile asset BEFORE adding components so AddObjectToAsset can
            // attach each component as a subasset of the asset file. If components are added
            // to an in-memory profile and then CreateAsset runs, the component references
            // serialize as {fileID: 0} nulls (root cause of 2026-05-16 Session 2 smoke FAIL).
            AssetDatabase.CreateAsset(profile, VolumeProfilePath);
            ConfigureVolumeProfile(profile);
            return profile;
        }

        private static void ConfigureVolumeProfile(VolumeProfile profile)
        {
            var colorAdj = GetOrAdd<ColorAdjustments>(profile);
            colorAdj.active = true;
            colorAdj.saturation.value = ColorAdjustmentsSaturation;
            colorAdj.saturation.overrideState = true;

            var whiteBal = GetOrAdd<WhiteBalance>(profile);
            whiteBal.active = true;
            whiteBal.temperature.value = WhiteBalanceTemperature;
            whiteBal.temperature.overrideState = true;

            var tonemap = GetOrAdd<Tonemapping>(profile);
            tonemap.active = true;
            tonemap.mode.value = TonemappingMode.Neutral;
            tonemap.mode.overrideState = true;

            // Δ4: Bloom + Vignette explicitly present-and-disabled so the smoke runner
            // can assert active==false rather than rely on absence.
            var bloom = GetOrAdd<Bloom>(profile);
            bloom.active = false;

            var vignette = GetOrAdd<Vignette>(profile);
            vignette.active = false;
        }

        private static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet<T>(out var existing))
            {
                return existing;
            }
            var component = profile.Add<T>(overrides: true);
            // Persist the component as a subasset of the profile so the reference survives
            // serialization. Without this, the saved .asset has {fileID: 0} null refs.
            // The Editor UI path (VolumeProfileEditor in URP/Core) uses the same call.
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(profile)))
            {
                AssetDatabase.AddObjectToAsset(component, profile);
            }
            return component;
        }

        private static void BuildGeometry(GameObject root)
        {
            // Ground plane (Unity Plane primitive is 10x10m; scale to 15x20m).
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(root.transform, worldPositionStays: false);
            ground.transform.localScale = new Vector3(CourtyardWidth / 10f, 1f, CourtyardLength / 10f);
            ground.transform.localPosition = Vector3.zero;

            // 3 walls + 1 open south side (toward camp interaction).
            BuildWall(root.transform, "Wall_North",
                position: new Vector3(0f, WallHeight * 0.5f, CourtyardLength * 0.5f),
                size: new Vector3(CourtyardWidth, WallHeight, WallThickness));

            BuildWall(root.transform, "Wall_West",
                position: new Vector3(-CourtyardWidth * 0.5f, WallHeight * 0.5f, 0f),
                size: new Vector3(WallThickness, WallHeight, CourtyardLength));

            BuildWall(root.transform, "Wall_East",
                position: new Vector3(CourtyardWidth * 0.5f, WallHeight * 0.5f, 0f),
                size: new Vector3(WallThickness, WallHeight, CourtyardLength));
        }

        private static void BuildWall(Transform parent, string name, Vector3 position, Vector3 size)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent, worldPositionStays: false);
            wall.transform.localScale = size;
            wall.transform.localPosition = position;
        }

        private static void BuildLanterns(GameObject root)
        {
            // 3 practical sources per env_lighting_district_camp.md:34 ("2-3 instances per district zone").
            // Placement: NW, NE corners + center of closed end. Mount at lantern height.
            var positions = new Vector3[]
            {
                new Vector3(-CourtyardWidth * 0.45f, LanternMountHeight, CourtyardLength * 0.45f),
                new Vector3(CourtyardWidth * 0.45f, LanternMountHeight, CourtyardLength * 0.45f),
                new Vector3(0f, LanternMountHeight, 0f),
            };
            var names = new[] { "Lantern_NW", "Lantern_NE", "Lantern_Center" };

            for (int i = 0; i < positions.Length; i++)
            {
                var go = new GameObject(names[i]);
                go.transform.SetParent(root.transform, worldPositionStays: false);
                go.transform.localPosition = positions[i];

                var light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.useColorTemperature = true;
                light.colorTemperature = LanternColorTemp;
                light.color = Color.white;
                light.intensity = LanternIntensity;
                light.range = LanternRange;
                light.shadows = LightShadows.Soft;
            }
        }

        private static void BuildVolume(GameObject root, VolumeProfile profile)
        {
            var volumeGo = new GameObject("Volume_District_State1");
            volumeGo.transform.SetParent(root.transform, worldPositionStays: false);
            // Δ5: Volume on Default layer; Base camera includes Default, Overlay = Nothing.
            volumeGo.layer = LayerMask.NameToLayer("Default");

            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 0f;
            volume.weight = 1f;
            volume.sharedProfile = profile;
        }

        private static void BuildCameras(GameObject root)
        {
            // Base camera: positioned to read the courtyard at typical exploration framing.
            var baseGo = new GameObject("Camera_Base");
            baseGo.transform.SetParent(root.transform, worldPositionStays: false);
            baseGo.transform.localPosition = new Vector3(0f, 7f, -CourtyardLength * 0.5f);
            baseGo.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
            baseGo.tag = "MainCamera";

            var baseCamera = baseGo.AddComponent<Camera>();
            baseCamera.clearFlags = CameraClearFlags.Skybox;
            baseCamera.fieldOfView = 50f;

            var baseData = baseCamera.GetUniversalAdditionalCameraData();
            baseData.renderType = CameraRenderType.Base;
            // Δ5: Volume on Default; Base sees it (composition tested).
            baseData.volumeLayerMask = 1 << LayerMask.NameToLayer("Default");
            baseData.renderPostProcessing = true;

            // Overlay camera: empty for now; populated in Session 4 (HUD).
            var overlayGo = new GameObject("Camera_Overlay");
            overlayGo.transform.SetParent(root.transform, worldPositionStays: false);
            overlayGo.transform.localPosition = baseGo.transform.localPosition;
            overlayGo.transform.localRotation = baseGo.transform.localRotation;

            var overlayCamera = overlayGo.AddComponent<Camera>();
            overlayCamera.clearFlags = CameraClearFlags.Depth;
            overlayCamera.fieldOfView = 50f;

            var overlayData = overlayCamera.GetUniversalAdditionalCameraData();
            overlayData.renderType = CameraRenderType.Overlay;
            // Δ5: Overlay sees no Volume (HUD-case isolation pattern).
            overlayData.volumeLayerMask = 0;
            overlayData.renderPostProcessing = false;

            // Attach Overlay to Base camera stack.
            baseData.cameraStack.Add(overlayCamera);
        }

        private static void ConfigureRenderSettingsAndSkybox()
        {
            // Ambient: muted cool gray (visually corresponds to 4800K cool register per
            // env_lighting_district_camp.md:42 "ambient fill beyond is a cool 4800-5200K").
            // Unity ambient color does not have a Kelvin field directly (Δ2).
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.45f, 0.48f, 0.52f);
            RenderSettings.ambientIntensity = 1f;

            // NO directional sun per spike plan §Session 2 line 103 + lighting spec line 24.
            RenderSettings.sun = null;

            // Procedural skybox tuned toward 6000K overcast register (spec §State 1).
            // Sun size 0 + low atmosphere thickness gives flat overcast read.
            var skyboxMat = new Material(Shader.Find("Skybox/Procedural"));
            skyboxMat.name = "Skybox_District_Overcast";
            skyboxMat.SetFloat("_SunSize", 0f);
            skyboxMat.SetFloat("_SunSizeConvergence", 1f);
            skyboxMat.SetFloat("_AtmosphereThickness", 0.4f);
            skyboxMat.SetColor("_SkyTint", new Color(0.55f, 0.58f, 0.62f));
            skyboxMat.SetColor("_GroundColor", new Color(0.30f, 0.30f, 0.32f));
            skyboxMat.SetFloat("_Exposure", 0.8f);
            RenderSettings.skybox = skyboxMat;

            // Session 1 fog pin: empirical Volume+fog composition test runs in Session 2.
            // Linear fog from 10-30m mirrors M2 controller (10-30m). Color matches ambient register.
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.40f, 0.43f, 0.47f);
            RenderSettings.fogStartDistance = FogStartDistance;
            RenderSettings.fogEndDistance = FogEndDistance;

            DynamicGI.UpdateEnvironment();
        }
    }
}
