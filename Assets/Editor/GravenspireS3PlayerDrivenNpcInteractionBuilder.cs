#if UNITY_EDITOR
#nullable enable

using System.IO;
using Gravenspire.UnityRuntime.Interaction;
using Gravenspire.UnityRuntime.Npc;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    public static class GravenspireS3PlayerDrivenNpcInteractionBuilder
    {
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";

        [MenuItem("Gravenspire/Build S3 Player-Driven NPC Interaction")]
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

            var caretaker = FindSceneObjectIncludingInactive(M3NamedNpcObjectiveFrame.AnchorObjectName);
            if (caretaker == null)
            {
                throw new IOException($"{M3NamedNpcObjectiveFrame.AnchorObjectName} is missing from {ScenePath}.");
            }

            if (!caretaker.TryGetComponent<M3NamedNpcObjectiveFrame>(out var objectiveFrame))
            {
                throw new IOException($"{M3NamedNpcObjectiveFrame.AnchorObjectName} is missing {nameof(M3NamedNpcObjectiveFrame)}.");
            }

            if (!caretaker.TryGetComponent<M3NamedNpcInteractTarget>(out var interactTarget))
            {
                interactTarget = caretaker.AddComponent<M3NamedNpcInteractTarget>();
            }

            interactTarget.Configure(objectiveFrame);

            var harnessRoot = FindSceneObjectIncludingInactive(S3PlayerInteractionHarness.HarnessRootName);
            if (harnessRoot == null || !harnessRoot.TryGetComponent<S3PlayerInteractionHarness>(out _))
            {
                throw new IOException($"{S3PlayerInteractionHarness.HarnessRootName} with {nameof(S3PlayerInteractionHarness)} is missing from {ScenePath}.");
            }

            EditorUtility.SetDirty(interactTarget);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new IOException($"Failed to save S3 player-driven NPC interaction scene at {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("S3-02 player-driven NPC interaction adapter attached to M3_Caretaker.");
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
