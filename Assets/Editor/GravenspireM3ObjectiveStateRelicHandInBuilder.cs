#if UNITY_EDITOR
#nullable enable

using System.IO;
using Gravenspire.Gameplay.Npc.M3Objective;
using Gravenspire.UnityRuntime.Npc;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    public static class GravenspireM3ObjectiveStateRelicHandInBuilder
    {
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string ObjectiveRootName = "M3_ObjectiveStateRoot";

        [MenuItem("Gravenspire/Build M3 Objective State Relic Hand-In")]
        public static void Build()
        {
            if (!File.Exists(ScenePath))
            {
                throw new FileNotFoundException($"Dev entry scene is missing: {ScenePath}");
            }

            GravenspireM3NamedNpcObjectiveFrameBuilder.Build();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                throw new IOException($"Failed to open scene at {ScenePath}.");
            }

            var root = EnsureEmpty(ObjectiveRootName, Vector3.zero);
            var relicObject = EnsurePrimitive(
                M3ObjectiveStateRelicHandInSession.RelicObjectName,
                PrimitiveType.Cube,
                new Vector3(-1.85f, 0.35f, 3.15f),
                new Vector3(0.75f, 0.35f, 0.55f),
                root.transform);
            relicObject.tag = "Untagged";
            relicObject.layer = 0;

            if (relicObject.TryGetComponent<Collider>(out var relicCollider))
            {
                relicCollider.isTrigger = true;
            }

            if (!root.TryGetComponent<M3ObjectiveStateRelicHandIn>(out var objective))
            {
                objective = root.AddComponent<M3ObjectiveStateRelicHandIn>();
            }

            objective.ConfigureForM3ObjectiveStateRelicHandIn(relicObject);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new IOException($"Failed to save M3 objective state relic hand-in scene at {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("S2-M3-02 objective state relic hand-in scene objects generated.");
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

        private static GameObject EnsurePrimitive(
            string objectName,
            PrimitiveType primitiveType,
            Vector3 position,
            Vector3 scale,
            Transform? parent)
        {
            var existing = FindSceneObjectIncludingInactive(objectName);
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
