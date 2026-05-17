#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    public static class GravenspireDevEntryBuilder
    {
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string RendererPath = "Assets/Settings/GravenspireUniversalRenderer.asset";
        private const string PipelinePath = "Assets/Settings/GravenspireUniversalRenderPipeline.asset";
        private const string DefaultPostProcessDataPath =
            "Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset";

        [MenuItem("Gravenspire/Build Dev Entry Shell")]
        public static void Build()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Settings");

            var pipelineAsset = EnsureUniversalRenderPipelineAsset();
            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            QualitySettings.renderPipeline = pipelineAsset;

            PlayerSettings.companyName = "Gravenspire";
            PlayerSettings.productName = "Gravenspire";
            PlayerSettings.bundleVersion = "0.2.0-dev";
            PlayerSettings.colorSpace = ColorSpace.Linear;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "_DevEntry";

            BuildSceneObjects();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new IOException($"Failed to save dev entry scene at {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"S2-FOUNDATION-01 dev entry shell generated at {ScenePath}.");
        }

        private static UniversalRenderPipelineAsset EnsureUniversalRenderPipelineAsset()
        {
            var rendererData = EnsureUniversalRendererData();
            var existingPipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (existingPipeline != null)
            {
                return existingPipeline;
            }

            var pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
            AssetDatabase.CreateAsset(pipelineAsset, PipelinePath);
            return pipelineAsset;
        }

        private static UniversalRendererData EnsureUniversalRendererData()
        {
            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, RendererPath);
            }

            EnsureRendererPostProcessData(rendererData);
            return rendererData;
        }

        private static void EnsureRendererPostProcessData(UniversalRendererData rendererData)
        {
            if (rendererData.postProcessData != null)
            {
                return;
            }

            var postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>(DefaultPostProcessDataPath);
            if (postProcessData == null)
            {
                throw new FileNotFoundException(
                    "Failed to load URP default PostProcessData at " + DefaultPostProcessDataPath + ".");
            }

            rendererData.postProcessData = postProcessData;
            EditorUtility.SetDirty(rendererData);
        }

        private static void BuildSceneObjects()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.12f, 0.12f, 0.14f);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "DevEntry_DistrictBlockout_Floor";
            floor.transform.localScale = new Vector3(3.0f, 1.0f, 3.0f);

            var playerMarker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerMarker.name = "ClericShellMarker";
            playerMarker.transform.position = new Vector3(0.0f, 1.0f, 0.0f);

            var districtMarker = new GameObject("FirstDistrict_ShellOnly_NoGameplay");
            districtMarker.transform.position = Vector3.zero;

            var cameraObject = new GameObject("DevEntryCamera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetPositionAndRotation(
                new Vector3(0.0f, 6.0f, -8.0f),
                Quaternion.Euler(35.0f, 0.0f, 0.0f));
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.03f, 0.035f, 0.04f);

            var lightObject = new GameObject("DevEntryDirectionalLight");
            lightObject.transform.rotation = Quaternion.Euler(45.0f, -30.0f, 0.0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
        }
    }
}
#endif
