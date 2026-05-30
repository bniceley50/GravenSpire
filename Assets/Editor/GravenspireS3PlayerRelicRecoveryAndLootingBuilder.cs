#if UNITY_EDITOR
#nullable enable

using System.IO;
using Gravenspire.Gameplay.Npc.M3Objective;
using Gravenspire.UnityRuntime.Interaction;
using Gravenspire.UnityRuntime.Npc;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    public static class GravenspireS3PlayerRelicRecoveryAndLootingBuilder
    {
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";

        [MenuItem("Gravenspire/Build S3 Player Relic Recovery + Looting")]
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

            var caretaker = RequiredObject(M3NamedNpcObjectiveFrame.AnchorObjectName);
            var objectiveRoot = RequiredObject("M3_ObjectiveStateRoot");
            var relicObject = RequiredObject(M3ObjectiveStateRelicHandInSession.RelicObjectName);
            var vendorObject = RequiredObject(M3LootTableFixedProfileVendorData.VendorObjectName);
            var harnessRoot = RequiredObject(S3PlayerInteractionHarness.HarnessRootName);

            var frame = RequiredComponent<M3NamedNpcObjectiveFrame>(caretaker);
            var objectiveState = RequiredComponent<M3ObjectiveStateRelicHandIn>(objectiveRoot);
            var lootVendor = RequiredComponent<M3LootTableFixedProfileVendor>(vendorObject);
            var harness = RequiredComponent<S3PlayerInteractionHarness>(harnessRoot);

            if (!caretaker.TryGetComponent<M3NamedNpcInteractTarget>(out var npcAdapter))
            {
                npcAdapter = caretaker.AddComponent<M3NamedNpcInteractTarget>();
            }

            if (!relicObject.TryGetComponent<M3RelicInteractTarget>(out var relicAdapter))
            {
                relicAdapter = relicObject.AddComponent<M3RelicInteractTarget>();
            }

            npcAdapter.Configure(frame, objectiveState);
            relicAdapter.Configure(objectiveState, lootVendor);
            harness.RefreshRegisteredTargetsFromScene();

            EditorUtility.SetDirty(npcAdapter);
            EditorUtility.SetDirty(relicAdapter);
            EditorUtility.SetDirty(harness);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new IOException($"Failed to save S3 player relic recovery + looting scene at {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("S3-03 player relic recovery + looting adapters wired.");
        }

        private static GameObject RequiredObject(string objectName)
        {
            var found = FindSceneObjectIncludingInactive(objectName);
            if (found == null)
            {
                throw new IOException($"{objectName} is missing from {ScenePath}.");
            }

            return found;
        }

        private static T RequiredComponent<T>(GameObject owner)
            where T : Component
        {
            if (!owner.TryGetComponent<T>(out var component))
            {
                throw new IOException($"{owner.name} is missing {typeof(T).Name}.");
            }

            return component;
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
