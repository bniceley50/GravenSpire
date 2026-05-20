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
    public static class GravenspireM3LootTableFixedProfileVendorBuilder
    {
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string VendorRootName = "M3_LootVendorRoot";

        [MenuItem("Gravenspire/Build M3 Loot Table Fixed-Profile Vendor")]
        public static void Build()
        {
            if (!File.Exists(ScenePath))
            {
                throw new FileNotFoundException($"Dev entry scene is missing: {ScenePath}");
            }

            GravenspireM3ObjectiveStateRelicHandInBuilder.Build();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                throw new IOException($"Failed to open scene at {ScenePath}.");
            }

            var root = EnsureEmpty(VendorRootName, Vector3.zero);
            var vendor = EnsurePrimitive(
                M3LootTableFixedProfileVendorData.VendorObjectName,
                PrimitiveType.Cylinder,
                new Vector3(4.0f, 0.55f, -3.6f),
                new Vector3(0.8f, 0.55f, 0.8f),
                root.transform);
            vendor.tag = "Untagged";
            vendor.layer = 0;

            if (!vendor.TryGetComponent<M3LootTableFixedProfileVendor>(out var vendorComponent))
            {
                vendorComponent = vendor.AddComponent<M3LootTableFixedProfileVendor>();
            }

            vendorComponent.ConfigureForM3LootTableFixedProfileVendor();

            if (vendor.TryGetComponent<Collider>(out var collider))
            {
                collider.isTrigger = true;
            }

            var counter = EnsurePrimitive(
                "M3_CourtVendor_SalvageCounter",
                PrimitiveType.Cube,
                new Vector3(4.0f, 0.35f, -2.95f),
                new Vector3(1.3f, 0.25f, 0.35f),
                vendor.transform);
            if (counter.TryGetComponent<Collider>(out var counterCollider))
            {
                Object.DestroyImmediate(counterCollider);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new IOException($"Failed to save M3 loot table fixed-profile vendor scene at {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("S2-M3-03 loot table fixed-profile vendor scene objects generated.");
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
            existing.SetActive(true);
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
