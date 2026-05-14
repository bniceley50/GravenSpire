#if UNITY_EDITOR
#nullable enable

using System.IO;
using Gravenspire.UnityRuntime.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    public static class GravenspireM2SingleTrashLoopBuilder
    {
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";

        [MenuItem("Gravenspire/Build M2 Single Trash Loop")]
        public static void Build()
        {
            if (!File.Exists(ScenePath))
            {
                throw new FileNotFoundException($"Dev entry scene is missing: {ScenePath}");
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                throw new IOException($"Failed to open scene at {ScenePath}.");
            }

            EnsureBaseShellObjects();
            var root = EnsureEmpty("M2_CombatCampLoopRoot", Vector3.zero);
            EnsurePrimitive("M2_CampRestPoint", PrimitiveType.Cylinder, new Vector3(0.0f, 0.05f, -5.0f), new Vector3(2.6f, 0.10f, 2.6f), root.transform);
            EnsurePrimitive("M2_PullLane", PrimitiveType.Cube, new Vector3(0.0f, 0.03f, -0.5f), new Vector3(2.0f, 0.06f, 8.0f), root.transform);
            EnsurePrimitive("M2_BaselineTrash", PrimitiveType.Capsule, new Vector3(0.0f, 1.0f, 4.0f), new Vector3(0.8f, 1.0f, 0.8f), root.transform);
            EnsurePrimitive("M2_LinkedTrash", PrimitiveType.Capsule, new Vector3(2.3f, 1.0f, 4.8f), new Vector3(0.8f, 1.0f, 0.8f), root.transform);
            EnsurePrimitive("M2_NamedBlocker", PrimitiveType.Capsule, new Vector3(-2.8f, 1.4f, 5.6f), new Vector3(1.25f, 1.4f, 1.25f), root.transform);

            if (!root.TryGetComponent<M2SingleTrashMedLoopController>(out _))
            {
                root.AddComponent<M2SingleTrashMedLoopController>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new IOException($"Failed to save M2 single-trash loop scene at {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("S2-M2-02/S2-M2-03/S2-M2-04 combat camp loop scene objects generated.");
        }

        private static void EnsureBaseShellObjects()
        {
            EnsurePrimitiveIfMissing("DevEntry_DistrictBlockout_Floor", PrimitiveType.Plane, Vector3.zero, new Vector3(3.0f, 1.0f, 3.0f));
            EnsurePrimitiveIfMissing("ClericShellMarker", PrimitiveType.Capsule, new Vector3(0.0f, 1.0f, -5.0f), Vector3.one);

            if (GameObject.Find("FirstDistrict_ShellOnly_NoGameplay") == null)
            {
                new GameObject("FirstDistrict_ShellOnly_NoGameplay");
            }

            var cameraObject = GameObject.Find("DevEntryCamera");
            if (cameraObject == null)
            {
                cameraObject = new GameObject("DevEntryCamera");
                cameraObject.tag = "MainCamera";
                cameraObject.AddComponent<Camera>();
                cameraObject.transform.SetPositionAndRotation(
                    new Vector3(0.0f, 8.0f, -13.0f),
                    Quaternion.Euler(42.0f, 0.0f, 0.0f));
            }

            if (!cameraObject.TryGetComponent<Camera>(out var camera))
            {
                camera = cameraObject.AddComponent<Camera>();
            }
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.03f, 0.035f, 0.04f);

            var lightObject = GameObject.Find("DevEntryDirectionalLight");
            if (lightObject == null)
            {
                lightObject = new GameObject("DevEntryDirectionalLight");
                lightObject.AddComponent<Light>();
                lightObject.transform.rotation = Quaternion.Euler(45.0f, -30.0f, 0.0f);
            }

            if (!lightObject.TryGetComponent<Light>(out var light))
            {
                light = lightObject.AddComponent<Light>();
            }
            light.type = LightType.Directional;
            light.intensity = 1.0f;
        }

        private static GameObject EnsurePrimitiveIfMissing(
            string objectName,
            PrimitiveType primitiveType,
            Vector3 position,
            Vector3 scale)
        {
            var existing = GameObject.Find(objectName);
            if (existing != null)
            {
                return existing;
            }

            var created = GameObject.CreatePrimitive(primitiveType);
            created.name = objectName;
            created.transform.position = position;
            created.transform.localScale = scale;
            return created;
        }

        private static GameObject EnsureEmpty(string objectName, Vector3 position)
        {
            var existing = GameObject.Find(objectName);
            if (existing != null)
            {
                existing.transform.position = position;
                return existing;
            }

            var created = new GameObject(objectName);
            created.transform.position = position;
            return created;
        }

        private static GameObject EnsurePrimitive(
            string objectName,
            PrimitiveType primitiveType,
            Vector3 position,
            Vector3 scale,
            Transform? parent)
        {
            var existing = GameObject.Find(objectName);
            if (existing == null)
            {
                existing = GameObject.CreatePrimitive(primitiveType);
                existing.name = objectName;
            }

            existing.transform.SetParent(parent, worldPositionStays: true);
            existing.transform.position = position;
            existing.transform.localScale = scale;
            return existing;
        }
    }
}
#endif
