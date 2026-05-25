#if UNITY_EDITOR
#nullable enable

using System.IO;
using Gravenspire.UnityRuntime.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    public static class GravenspireS3PlayerInteractionHarnessBuilder
    {
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";

        [MenuItem("Gravenspire/Build S3 Player Interaction Harness")]
        public static void Build()
        {
            if (!File.Exists(ScenePath))
            {
                throw new FileNotFoundException($"Dev entry scene is missing: {ScenePath}");
            }

            // Rebuild the baseline M2 dev-entry scene before layering the S3-01 harness root.
            GravenspireM2SingleTrashLoopBuilder.Build();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                throw new IOException($"Failed to open scene at {ScenePath}.");
            }

            var root = EnsureEmpty(S3PlayerInteractionHarness.HarnessRootName, Vector3.zero);
            if (!root.TryGetComponent<S3PlayerInteractionHarness>(out var harness))
            {
                harness = root.AddComponent<S3PlayerInteractionHarness>();
            }

            var playerMarker = FindSceneObjectIncludingInactive(S3PlayerInteractionHarness.ClericMarkerObjectName);
            if (playerMarker == null)
            {
                throw new IOException($"{S3PlayerInteractionHarness.ClericMarkerObjectName} is missing from {ScenePath}.");
            }

            harness.Configure(playerMarker.transform, S3PlayerInteractionHarness.DefaultInteractRangeMeters);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new IOException($"Failed to save S3 player interaction harness scene at {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("S3-01 player interaction harness scene objects generated.");
        }

        private static GameObject EnsureEmpty(string objectName, Vector3 position)
        {
            var existing = FindSceneObjectIncludingInactive(objectName);
            if (existing != null)
            {
                existing.transform.position = position;
                existing.SetActive(true);
                return existing;
            }

            var created = new GameObject(objectName);
            created.transform.position = position;
            return created;
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
    }
}
#endif
