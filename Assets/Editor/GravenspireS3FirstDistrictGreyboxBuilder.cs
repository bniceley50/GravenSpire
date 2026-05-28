#if UNITY_EDITOR
#nullable enable

using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    public static class GravenspireS3FirstDistrictGreyboxBuilder
    {
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string MaterialFolder = "Assets/Materials/Greybox";
        private const string NavMeshFolder = "Assets/Scenes/_DevEntry";
        private const string NavMeshAssetPath = NavMeshFolder + "/FirstDistrict_Greybox_NavMesh.asset";
        private const string RootName = "FirstDistrict_Greybox";
        private const string SurfaceRootName = "FirstDistrict_NavMeshSurface";
        private const string ShellMarkerName = "FirstDistrict_ShellOnly_NoGameplay";
        private const string FloorName = "DevEntry_DistrictBlockout_Floor";

        private static readonly Vector3 ExpectedSpawnPosition = new(0.0f, 1.0f, 0.0f);
        private static readonly Vector3 ExpectedCaretakerPosition = new(2.0f, 1.0f, -4.2f);
        private static readonly Vector3 ExpectedVendorPosition = new(4.0f, 0.55f, -3.6f);
        private static readonly Vector3 ExpectedRelicPosition = new(-1.85f, 0.35f, 3.15f);

        [MenuItem("Gravenspire/Build S3 First District Greybox")]
        public static void Build()
        {
            BuildInternal(buildNavMesh: false);
        }

        [MenuItem("Gravenspire/Build+Bake S3 First District Greybox")]
        public static void BuildAndBake()
        {
            BuildInternal(buildNavMesh: true);
        }

        [MenuItem("Gravenspire/Verify S3 First District Greybox Phase 1")]
        public static void VerifyPhase1()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                throw new IOException($"Failed to open scene at {ScenePath}.");
            }

            VerifyRequiredSceneObject(RootName);
            VerifyRequiredSceneObject(SurfaceRootName);
            VerifyRequiredSceneObject(FloorName);
            VerifyRequiredSceneObject("Greybox_CaretakerHall_Massing");
            VerifyRequiredSceneObject("Greybox_CourtVendorHall_Massing");
            VerifyRequiredSceneObject("Greybox_RelicStorehouse_Massing");
            VerifyRequiredSceneObject("GreyboxBoundary_North");
            VerifyRequiredSceneObject("GreyboxBoundary_South");
            VerifyRequiredSceneObject("GreyboxBoundary_East");
            VerifyRequiredSceneObject("GreyboxBoundary_West");
            VerifyAbsentSceneObject(ShellMarkerName);
            VerifyAnchorPositionsUnchanged();
            VerifyNoSceneLights();
            VerifyNavMeshPaths();
            VerifySpawnSightlineSet();

            Debug.Log("S3-05 First District greybox Phase 1 verification PASS.");
        }

        private static void BuildInternal(bool buildNavMesh)
        {
            if (!File.Exists(ScenePath))
            {
                throw new FileNotFoundException($"Dev entry scene is missing: {ScenePath}");
            }

            GravenspireS3PlayerInteractionHarnessBuilder.Build();

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                throw new IOException($"Failed to open scene at {ScenePath}.");
            }

            var materials = EnsureMaterials();
            RemoveObjectIfPresent(ShellMarkerName);
            RemoveObjectIfPresent("DevEntryDirectionalLight");

            var root = EnsureEmpty(RootName, Vector3.zero, parent: null);
            ClearChildren(root.transform);

            var surfaceRoot = EnsureEmpty(SurfaceRootName, Vector3.zero, root.transform);
            EnsureFloor(surfaceRoot.transform, materials.Floor);

            EnsurePrimitive(
                "Greybox_CaretakerHall_Massing",
                PrimitiveType.Cube,
                new Vector3(3.0f, 4.0f, -7.0f),
                new Vector3(3.0f, 8.0f, 4.0f),
                materials.LandmarkWarm,
                surfaceRoot.transform);
            EnsurePrimitive(
                "Greybox_CourtVendorHall_Massing",
                PrimitiveType.Cube,
                new Vector3(6.0f, 2.0f, -6.0f),
                new Vector3(6.0f, 4.0f, 4.0f),
                materials.LandmarkCool,
                surfaceRoot.transform);
            EnsurePrimitive(
                "Greybox_RelicStorehouse_Massing",
                PrimitiveType.Cube,
                new Vector3(-3.0f, 3.0f, 9.0f),
                new Vector3(4.0f, 6.0f, 4.0f),
                materials.LandmarkDark,
                surfaceRoot.transform);

            EnsureBoundaryWalls(surfaceRoot.transform, materials.Boundary);
            AssignAnchorGreyboxMaterials(materials);
            EnsureNavMeshSurface(surfaceRoot);
            ConfigureCamera();
            ConfigureAmbientOnlyLighting();
            VerifyAnchorPositionsUnchanged();

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new IOException($"Failed to save S3 First District greybox scene at {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (buildNavMesh)
            {
                BakeNavMesh();
                scene = SceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                {
                    throw new IOException($"Failed to save baked NavMesh scene at {ScenePath}.");
                }
            }

            Debug.Log($"S3-05 First District greybox generated. Floor={FloorName}, NavMeshBake={buildNavMesh}.");
        }

        private static GreyboxMaterials EnsureMaterials()
        {
            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets/Materials", "Greybox");

            return new GreyboxMaterials(
                EnsureMaterial("GreyboxFloor", new Color32(0x7A, 0x7A, 0x7A, 0xFF)),
                EnsureMaterial("GreyboxLandmarkWarm", new Color32(0xA8, 0x98, 0x78, 0xFF)),
                EnsureMaterial("GreyboxLandmarkCool", new Color32(0x78, 0x88, 0x98, 0xFF)),
                EnsureMaterial("GreyboxLandmarkDark", new Color32(0x5A, 0x5A, 0x5A, 0xFF)),
                EnsureMaterial("GreyboxBoundary", new Color32(0x90, 0x90, 0x90, 0xFF)));
        }

        private static Material EnsureMaterial(string name, Color color)
        {
            var assetPath = $"{MaterialFolder}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (existing != null)
            {
                ConfigureMaterial(existing, color);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null)
            {
                throw new IOException("Unable to find a compatible greybox material shader.");
            }

            var material = new Material(shader)
            {
                name = name
            };
            ConfigureMaterial(material, color);
            AssetDatabase.CreateAsset(material, assetPath);
            return material;
        }

        private static void ConfigureMaterial(Material material, Color color)
        {
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0.0f);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.3f);
            }
        }

        private static GameObject EnsureFloor(Transform parent, Material material)
        {
            var floor = FindSceneObjectIncludingInactive(FloorName);
            if (floor == null)
            {
                floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = FloorName;
            }

            floor.SetActive(true);
            floor.transform.SetParent(parent, worldPositionStays: true);
            floor.transform.position = Vector3.zero;
            floor.transform.rotation = Quaternion.identity;
            floor.transform.localScale = new Vector3(3.0f, 1.0f, 3.0f);
            AssignMaterial(floor, material);
            return floor;
        }

        private static void EnsureBoundaryWalls(Transform parent, Material material)
        {
            EnsurePrimitive("GreyboxBoundary_North", PrimitiveType.Cube, new Vector3(0.0f, 0.5f, 15.0f), new Vector3(30.0f, 1.0f, 0.5f), material, parent);
            EnsurePrimitive("GreyboxBoundary_South", PrimitiveType.Cube, new Vector3(0.0f, 0.5f, -15.0f), new Vector3(30.0f, 1.0f, 0.5f), material, parent);
            EnsurePrimitive("GreyboxBoundary_East", PrimitiveType.Cube, new Vector3(15.0f, 0.5f, 0.0f), new Vector3(0.5f, 1.0f, 30.0f), material, parent);
            EnsurePrimitive("GreyboxBoundary_West", PrimitiveType.Cube, new Vector3(-15.0f, 0.5f, 0.0f), new Vector3(0.5f, 1.0f, 30.0f), material, parent);
        }

        private static GameObject EnsurePrimitive(
            string objectName,
            PrimitiveType primitiveType,
            Vector3 position,
            Vector3 scale,
            Material material,
            Transform parent)
        {
            var existing = FindSceneObjectIncludingInactive(objectName);
            if (existing == null)
            {
                existing = GameObject.CreatePrimitive(primitiveType);
                existing.name = objectName;
            }

            existing.SetActive(true);
            existing.transform.SetParent(parent, worldPositionStays: true);
            existing.transform.position = position;
            existing.transform.rotation = Quaternion.identity;
            existing.transform.localScale = scale;
            AssignMaterial(existing, material);
            EnsureNotWalkableModifierVolume(existing);
            return existing;
        }

        private static void EnsureNotWalkableModifierVolume(GameObject gameObject)
        {
            const int notWalkableArea = 1;

            if (!gameObject.TryGetComponent<NavMeshModifierVolume>(out var modifierVolume))
            {
                modifierVolume = gameObject.AddComponent<NavMeshModifierVolume>();
            }

            modifierVolume.area = notWalkableArea;
            modifierVolume.center = Vector3.zero;
            modifierVolume.size = Vector3.one;
        }

        private static void AssignMaterial(GameObject gameObject, Material material)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static void EnsureNavMeshSurface(GameObject surfaceRoot)
        {
            if (!surfaceRoot.TryGetComponent<NavMeshSurface>(out var surface))
            {
                surface = surfaceRoot.AddComponent<NavMeshSurface>();
            }

            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.defaultArea = 0;
            surface.agentTypeID = 0;
            surface.center = Vector3.zero;
            surface.size = new Vector3(30.0f, 4.0f, 30.0f);
        }

        private static void BakeNavMesh()
        {
            var surfaceRoot = FindSceneObjectIncludingInactive(SurfaceRootName);
            if (surfaceRoot == null || !surfaceRoot.TryGetComponent<NavMeshSurface>(out var surface))
            {
                throw new IOException($"{SurfaceRootName} with NavMeshSurface is missing.");
            }

            surface.RemoveData();
            surface.BuildNavMesh();
            if (surface.navMeshData == null)
            {
                throw new IOException("S3-05 NavMeshSurface did not produce NavMeshData.");
            }

            var builtData = surface.navMeshData;
            EnsureFolder("Assets/Scenes", "_DevEntry");
            var existingAsset = AssetDatabase.LoadAssetAtPath<NavMeshData>(NavMeshAssetPath);
            if (existingAsset != null)
            {
                AssetDatabase.DeleteAsset(NavMeshAssetPath);
            }

            if (!AssetDatabase.Contains(builtData))
            {
                AssetDatabase.CreateAsset(builtData, NavMeshAssetPath);
            }
            else if (AssetDatabase.GetAssetPath(builtData) != NavMeshAssetPath)
            {
                builtData = Object.Instantiate(builtData);
                AssetDatabase.CreateAsset(builtData, NavMeshAssetPath);
            }

            var savedData = AssetDatabase.LoadAssetAtPath<NavMeshData>(NavMeshAssetPath);
            if (savedData == null)
            {
                throw new IOException($"Failed to save NavMeshData asset at {NavMeshAssetPath}.");
            }

            surface.navMeshData = savedData;
            var serializedSurface = new SerializedObject(surface);
            serializedSurface.FindProperty("m_NavMeshData").objectReferenceValue = savedData;
            serializedSurface.ApplyModifiedPropertiesWithoutUndo();
            surface.AddData();
            EditorUtility.SetDirty(surface);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ConfigureCamera()
        {
            var cameraObject = FindSceneObjectIncludingInactive("DevEntryCamera");
            if (cameraObject == null)
            {
                cameraObject = new GameObject("DevEntryCamera")
                {
                    tag = "MainCamera"
                };
                cameraObject.AddComponent<Camera>();
            }

            cameraObject.transform.SetPositionAndRotation(
                new Vector3(0.0f, 7.0f, -12.0f),
                Quaternion.Euler(34.0f, 0.0f, 0.0f));

            if (!cameraObject.TryGetComponent<Camera>(out var camera))
            {
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(0x90, 0x90, 0x90, 0xFF);
        }

        private static void ConfigureAmbientOnlyLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color32(0x80, 0x80, 0x80, 0xFF);
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflection = null;
            RenderSettings.fog = false;

            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(light.gameObject);
            }
        }

        private static void AssignAnchorGreyboxMaterials(GreyboxMaterials materials)
        {
            AssignMaterialToObjectTree("M3_Caretaker", materials.LandmarkWarm);
            AssignMaterialToObjectTree("M3_Caretaker_PostureStaff", materials.LandmarkWarm);
            AssignMaterialToObjectTree("M3_CourtVendor", materials.LandmarkCool);
            AssignMaterialToObjectTree("M3_CourtVendor_SalvageCounter", materials.LandmarkCool);
            AssignMaterialToObjectTree("M3_ObjectiveRelic", materials.LandmarkDark);
        }

        private static void AssignMaterialToObjectTree(string objectName, Material material)
        {
            var gameObject = FindSceneObjectIncludingInactive(objectName);
            if (gameObject == null)
            {
                return;
            }

            foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                renderer.sharedMaterial = material;
            }
        }

        private static void VerifyAnchorPositionsUnchanged()
        {
            VerifyPosition("ClericShellMarker", ExpectedSpawnPosition);
            VerifyPosition("M3_Caretaker", ExpectedCaretakerPosition);
            VerifyPosition("M3_CourtVendor", ExpectedVendorPosition);
            VerifyPosition("M3_ObjectiveRelic", ExpectedRelicPosition);
        }

        private static void VerifyNoSceneLights()
        {
            var lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (lights.Length > 0)
            {
                throw new IOException($"S3-05 greybox must use ambient lighting only, but found {lights.Length} Light component(s).");
            }
        }

        private static void VerifyNavMeshPaths()
        {
            var surfaceObject = VerifyRequiredSceneObject(SurfaceRootName);
            if (!surfaceObject.TryGetComponent<NavMeshSurface>(out var surface))
            {
                throw new IOException($"{SurfaceRootName} is missing NavMeshSurface.");
            }

            if (surface.navMeshData == null)
            {
                throw new IOException($"{SurfaceRootName} has no baked NavMeshData assigned.");
            }

            surface.AddData();
            var spawn = GetSampledNavMeshPosition("ClericShellMarker");
            VerifyPathTo(spawn, "M3_Caretaker");
            VerifyPathTo(spawn, "M3_CourtVendor");
            VerifyPathTo(spawn, "M3_ObjectiveRelic");
            VerifyPathTo(spawn, "M2_CampRestPoint");
            VerifyPathTo(spawn, "M2_BaselineTrash");
            VerifyPathTo(spawn, "M2_LinkedTrash");
            VerifyPathTo(spawn, "M2_NamedBlocker");
        }

        private static void VerifyPathTo(Vector3 sampledSpawn, string objectName)
        {
            var sampledTarget = GetSampledNavMeshPosition(objectName);
            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(sampledSpawn, sampledTarget, NavMesh.AllAreas, path))
            {
                throw new IOException($"NavMesh.CalculatePath failed from ClericShellMarker to {objectName}.");
            }

            if (path.status != NavMeshPathStatus.PathComplete)
            {
                throw new IOException($"NavMesh path from ClericShellMarker to {objectName} is {path.status}, not PathComplete.");
            }
        }

        private static Vector3 GetSampledNavMeshPosition(string objectName)
        {
            var gameObject = VerifyRequiredSceneObject(objectName);
            if (!NavMesh.SamplePosition(gameObject.transform.position, out var hit, 2.0f, NavMesh.AllAreas))
            {
                throw new IOException($"{objectName} at {gameObject.transform.position} is not within 2m of the baked NavMesh.");
            }

            return hit.position;
        }

        private static void VerifySpawnSightlineSet()
        {
            var spawn = VerifyRequiredSceneObject("ClericShellMarker").transform.position;
            VerifyInForwardSightline(spawn, "Greybox_CaretakerHall_Massing", 60.0f);
            VerifyInForwardSightline(spawn, "Greybox_CourtVendorHall_Massing", 60.0f);
        }

        private static void VerifyInForwardSightline(Vector3 spawn, string objectName, float maxHalfAngleDegrees)
        {
            var landmark = VerifyRequiredSceneObject(objectName);
            var offset = landmark.transform.position - spawn;
            offset.y = 0.0f;
            var angle = Vector3.Angle(Vector3.back, offset.normalized);
            if (angle > maxHalfAngleDegrees)
            {
                throw new IOException($"{objectName} is {angle:0.0} degrees from spawn forward, outside the {maxHalfAngleDegrees:0.0}-degree Phase 1 sightline check.");
            }
        }

        private static void VerifyPosition(string objectName, Vector3 expected)
        {
            var gameObject = FindSceneObjectIncludingInactive(objectName);
            if (gameObject == null)
            {
                throw new IOException($"{objectName} is missing after S3-05 greybox build.");
            }

            var actual = gameObject.transform.position;
            if (Vector3.Distance(actual, expected) > 0.001f)
            {
                throw new IOException($"{objectName} moved unexpectedly. Expected {expected}, actual {actual}.");
            }
        }

        private static GameObject VerifyRequiredSceneObject(string objectName)
        {
            var gameObject = FindSceneObjectIncludingInactive(objectName);
            if (gameObject == null)
            {
                throw new IOException($"{objectName} is missing from {ScenePath}.");
            }

            return gameObject;
        }

        private static void VerifyAbsentSceneObject(string objectName)
        {
            var gameObject = FindSceneObjectIncludingInactive(objectName);
            if (gameObject != null)
            {
                throw new IOException($"{objectName} should have been replaced, but is still present in {ScenePath}.");
            }
        }

        private static GameObject EnsureEmpty(string objectName, Vector3 position, Transform? parent)
        {
            var existing = FindSceneObjectIncludingInactive(objectName);
            if (existing != null)
            {
                existing.SetActive(true);
                existing.transform.SetParent(parent, worldPositionStays: true);
                existing.transform.position = position;
                return existing;
            }

            var created = new GameObject(objectName);
            created.transform.SetParent(parent, worldPositionStays: true);
            created.transform.position = position;
            return created;
        }

        private static void ClearChildren(Transform transform)
        {
            for (var index = transform.childCount - 1; index >= 0; index--)
            {
                Object.DestroyImmediate(transform.GetChild(index).gameObject);
            }
        }

        private static void RemoveObjectIfPresent(string objectName)
        {
            var existing = FindSceneObjectIncludingInactive(objectName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
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

        private static void EnsureFolder(string parent, string name)
        {
            var path = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private sealed class GreyboxMaterials
        {
            public GreyboxMaterials(
                Material floor,
                Material landmarkWarm,
                Material landmarkCool,
                Material landmarkDark,
                Material boundary)
            {
                Floor = floor;
                LandmarkWarm = landmarkWarm;
                LandmarkCool = landmarkCool;
                LandmarkDark = landmarkDark;
                Boundary = boundary;
            }

            public Material Floor { get; }

            public Material LandmarkWarm { get; }

            public Material LandmarkCool { get; }

            public Material LandmarkDark { get; }

            public Material Boundary { get; }
        }
    }
}
#endif
