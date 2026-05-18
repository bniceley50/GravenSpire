#if UNITY_EDITOR
#nullable enable

using System.IO;
using Gravenspire.UnityRuntime.Npc;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    public static class GravenspireM3NamedNpcObjectiveFrameBuilder
    {
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";

        [MenuItem("Gravenspire/Build M3 Named NPC Objective Frame")]
        public static void Build()
        {
            if (!File.Exists(ScenePath))
            {
                throw new FileNotFoundException($"Dev entry scene is missing: {ScenePath}");
            }

            GravenspireM2SingleTrashLoopBuilder.Build();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                throw new IOException($"Failed to open scene at {ScenePath}.");
            }

            var root = EnsureEmpty("M3_ObjectiveFrameRoot", Vector3.zero);
            var caretaker = EnsurePrimitive(
                M3NamedNpcObjectiveFrame.AnchorObjectName,
                PrimitiveType.Capsule,
                new Vector3(2.0f, 1.0f, -4.2f),
                new Vector3(0.75f, 1.1f, 0.75f),
                root.transform);
            caretaker.tag = "Untagged";
            caretaker.layer = 0;

            if (!caretaker.TryGetComponent<M3NamedNpcObjectiveFrame>(out var objectiveFrame))
            {
                objectiveFrame = caretaker.AddComponent<M3NamedNpcObjectiveFrame>();
            }

            objectiveFrame.ConfigureForM3ObjectiveFrame();

            if (caretaker.TryGetComponent<Collider>(out var collider))
            {
                collider.isTrigger = true;
            }

            var staff = EnsurePrimitive(
                "M3_Caretaker_PostureStaff",
                PrimitiveType.Cube,
                new Vector3(2.45f, 1.0f, -4.2f),
                new Vector3(0.08f, 1.65f, 0.08f),
                caretaker.transform);
            if (staff.TryGetComponent<Collider>(out var staffCollider))
            {
                Object.DestroyImmediate(staffCollider);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new IOException($"Failed to save M3 named NPC objective frame scene at {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("S2-M3-01 named NPC objective frame scene objects generated.");
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
