#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    public static class GravenspireS3DistrictGreyboxPresentationScanVerificationRunner
    {
        private const string StoryId = "S3-05";
        private const string StorySlug = "s3-05-navigable-greybox-first-district";
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const string GreyboxMaterialFolder = "Assets/Materials/Greybox";
        private const string AllMaterialFolder = "Assets/Materials";

        private static readonly HashSet<string> AllowedGreyboxMaterialNames = new(StringComparer.Ordinal)
        {
            "GreyboxFloor",
            "GreyboxBoundary",
            "GreyboxLandmarkWarm",
            "GreyboxLandmarkCool",
            "GreyboxLandmarkDark"
        };

        private static readonly HashSet<string> AllowedDistrictPrimitiveMeshNames = new(StringComparer.Ordinal)
        {
            "Cube",
            "Plane"
        };

        private static readonly HashSet<string> ProducedArtExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".fbx",
            ".obj",
            ".blend",
            ".ma",
            ".mb",
            ".psd",
            ".tga",
            ".png",
            ".jpg",
            ".jpeg",
            ".exr",
            ".hdr",
            ".wav",
            ".mp3",
            ".ogg",
            ".aiff",
            ".flac",
            ".mp4",
            ".mov"
        };

        private static readonly string[] RenderedObjectNames =
        {
            "FirstDistrict_Greybox",
            "M3_Caretaker",
            "M3_Caretaker_PostureStaff",
            "M3_CourtVendor",
            "M3_CourtVendor_SalvageCounter",
            "M3_ObjectiveRelic"
        };

        private static readonly List<string> Checks = new();
        private static readonly List<string> BlockingFindings = new();
        private static readonly List<string> ClassifiedObservations = new();
        private static readonly List<string> Warnings = new();
        private static readonly List<string> Errors = new();
        private static readonly List<string> MaterialPalette = new();

        [MenuItem("Gravenspire/Verify S3 District Greybox Presentation Scan")]
        public static void Run()
        {
            ClearState();
            Application.logMessageReceived += CaptureLog;
            var exitCode = 1;

            try
            {
                RunChecks();
                exitCode = AllChecksPassed() && BlockingFindings.Count == 0 && Errors.Count == 0 ? 0 : 1;
            }
            catch (Exception ex)
            {
                Errors.Add(ex.ToString());
                exitCode = 1;
            }
            finally
            {
                Application.logMessageReceived -= CaptureLog;
                WriteEvidenceAndExit(exitCode);
            }
        }

        private static void RunChecks()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RecordCheck("scene_loaded", scene.IsValid() && scene.path == ScenePath);
            if (!scene.IsValid())
            {
                throw new IOException($"Failed to open scene at {ScenePath}.");
            }

            var districtRoot = FindSceneObjectIncludingInactive("FirstDistrict_Greybox");
            RecordCheck("first_district_greybox_root_exists", districtRoot != null);
            if (districtRoot == null)
            {
                throw new IOException("FirstDistrict_Greybox is missing.");
            }

            RecordCheck("uniform_ambient_flat_mode", RenderSettings.ambientMode == AmbientMode.Flat);
            RecordCheck("scene_fog_disabled", !RenderSettings.fog);
            RecordCheck("no_light_components_in_scene", ScanSceneLightComponents() == 0);
            RecordCheck("no_audio_sources_in_scene", ScanSceneAudioSources() == 0);
            RecordCheck("district_uses_blocky_primitive_meshes", ScanDistrictMeshes(districtRoot));
            RecordCheck("rendered_objects_use_greybox_palette", ScanRenderedObjectMaterials());
            RecordCheck("greybox_material_assets_are_solid_color", ScanGreyboxMaterials());
            RecordCheck("no_non_greybox_material_assets", ScanMaterialFolder());
            RecordCheck("no_produced_art_asset_files_under_assets", ScanProducedArtAssetFiles());
            RecordCheck("scene_has_no_authored_skybox_assignment", ScanSceneSkyboxAssignment());
        }

        private static int ScanSceneLightComponents()
        {
            var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var light in lights)
            {
                BlockingFindings.Add($"Light component found: `{GetScenePath(light.gameObject)}` type `{light.type}`.");
            }

            return lights.Length;
        }

        private static int ScanSceneAudioSources()
        {
            var audioSources = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var audioSource in audioSources)
            {
                BlockingFindings.Add($"AudioSource component found: `{GetScenePath(audioSource.gameObject)}`.");
            }

            return audioSources.Length;
        }

        private static bool ScanDistrictMeshes(GameObject districtRoot)
        {
            var passed = true;
            foreach (var meshFilter in districtRoot.GetComponentsInChildren<MeshFilter>(includeInactive: true))
            {
                var mesh = meshFilter.sharedMesh;
                if (mesh == null)
                {
                    BlockingFindings.Add($"MeshFilter has no shared mesh: `{GetScenePath(meshFilter.gameObject)}`.");
                    passed = false;
                    continue;
                }

                var assetPath = AssetDatabase.GetAssetPath(mesh);
                if (IsUnityBuiltinResource(assetPath))
                {
                    ClassifiedObservations.Add($"District mesh `{GetScenePath(meshFilter.gameObject)}` uses Unity built-in primitive mesh `{mesh.name}`.");
                }
                else if (!string.IsNullOrEmpty(assetPath))
                {
                    BlockingFindings.Add($"District mesh uses imported asset `{assetPath}` on `{GetScenePath(meshFilter.gameObject)}`.");
                    passed = false;
                }

                if (!AllowedDistrictPrimitiveMeshNames.Contains(mesh.name))
                {
                    BlockingFindings.Add($"District mesh `{GetScenePath(meshFilter.gameObject)}` uses `{mesh.name}` instead of blocky primitive Cube/Plane geometry.");
                    passed = false;
                }
            }

            return passed;
        }

        private static bool ScanRenderedObjectMaterials()
        {
            var passed = true;
            foreach (var objectName in RenderedObjectNames)
            {
                var gameObject = FindSceneObjectIncludingInactive(objectName);
                if (gameObject == null)
                {
                    Warnings.Add($"Rendered object `{objectName}` was not present for palette scan.");
                    continue;
                }

                foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(includeInactive: true))
                {
                    var materials = renderer.sharedMaterials;
                    if (materials.Length == 0)
                    {
                        BlockingFindings.Add($"Renderer has no material: `{GetScenePath(renderer.gameObject)}`.");
                        passed = false;
                        continue;
                    }

                    foreach (var material in materials)
                    {
                        if (!ValidateGreyboxMaterialReference(renderer.gameObject, material))
                        {
                            passed = false;
                        }
                    }
                }
            }

            return passed;
        }

        private static bool ScanGreyboxMaterials()
        {
            var passed = true;
            var materialPaths = AssetDatabase.FindAssets("t:Material", new[] { GreyboxMaterialFolder });
            var foundNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var guid in materialPaths)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if (material == null)
                {
                    BlockingFindings.Add($"Material asset could not be loaded: `{assetPath}`.");
                    passed = false;
                    continue;
                }

                foundNames.Add(material.name);
                MaterialPalette.Add(DescribeMaterial(material, assetPath));

                if (!AllowedGreyboxMaterialNames.Contains(material.name))
                {
                    BlockingFindings.Add($"Unexpected greybox material asset `{assetPath}` with name `{material.name}`.");
                    passed = false;
                }

                if (!ValidateSolidColorMaterial(material, assetPath))
                {
                    passed = false;
                }
            }

            foreach (var expectedName in AllowedGreyboxMaterialNames)
            {
                if (!foundNames.Contains(expectedName))
                {
                    BlockingFindings.Add($"Expected greybox material `{expectedName}` is missing from `{GreyboxMaterialFolder}`.");
                    passed = false;
                }
            }

            return passed;
        }

        private static bool ScanMaterialFolder()
        {
            if (!Directory.Exists(AllMaterialFolder))
            {
                Warnings.Add($"Material folder `{AllMaterialFolder}` does not exist.");
                return true;
            }

            var passed = true;
            foreach (var path in Directory.GetFiles(AllMaterialFolder, "*.mat", SearchOption.AllDirectories))
            {
                var unityPath = NormalizePath(path);
                if (!unityPath.StartsWith(GreyboxMaterialFolder + "/", StringComparison.Ordinal))
                {
                    BlockingFindings.Add($"Non-greybox material asset found under Assets/Materials: `{unityPath}`.");
                    passed = false;
                }
            }

            return passed;
        }

        private static bool ScanProducedArtAssetFiles()
        {
            var passed = true;
            foreach (var path in Directory.GetFiles("Assets", "*", SearchOption.AllDirectories))
            {
                var extension = Path.GetExtension(path);
                if (!ProducedArtExtensions.Contains(extension))
                {
                    continue;
                }

                var unityPath = NormalizePath(path);
                BlockingFindings.Add($"Produced-art/audio asset extension `{extension}` found under Assets: `{unityPath}`.");
                passed = false;
            }

            return passed;
        }

        private static bool ScanSceneSkyboxAssignment()
        {
            var text = File.ReadAllText(ScenePath);
            var skyboxLine = FindLineContaining(text, "m_SkyboxMaterial:");
            if (skyboxLine == null)
            {
                ClassifiedObservations.Add("No `m_SkyboxMaterial` scene setting line found.");
                return true;
            }

            ClassifiedObservations.Add($"Scene skybox setting: `{skyboxLine.Trim()}`.");
            if (skyboxLine.Contains("fileID: 0", StringComparison.Ordinal))
            {
                return true;
            }

            if (skyboxLine.Contains("guid: 0000000000000000f000000000000000", StringComparison.Ordinal))
            {
                ClassifiedObservations.Add("The serialized skybox line points at Unity's built-in default material, not an authored scene skybox swap.");
                return true;
            }

            BlockingFindings.Add($"Authored skybox material assignment found in scene settings: `{skyboxLine.Trim()}`.");
            return false;
        }

        private static bool ValidateGreyboxMaterialReference(GameObject owner, Material? material)
        {
            if (material == null)
            {
                BlockingFindings.Add($"Renderer material slot is empty: `{GetScenePath(owner)}`.");
                return false;
            }

            var assetPath = AssetDatabase.GetAssetPath(material);
            if (!assetPath.StartsWith(GreyboxMaterialFolder + "/", StringComparison.Ordinal))
            {
                BlockingFindings.Add($"Renderer `{GetScenePath(owner)}` uses non-greybox material `{material.name}` at `{assetPath}`.");
                return false;
            }

            return ValidateSolidColorMaterial(material, assetPath);
        }

        private static bool ValidateSolidColorMaterial(Material material, string assetPath)
        {
            var passed = true;
            if (material.shader == null || material.shader.name != "Universal Render Pipeline/Lit")
            {
                BlockingFindings.Add($"Material `{assetPath}` uses shader `{(material.shader == null ? "null" : material.shader.name)}` instead of `Universal Render Pipeline/Lit`.");
                passed = false;
            }

            foreach (var propertyName in material.GetTexturePropertyNames())
            {
                if (material.GetTexture(propertyName) != null)
                {
                    BlockingFindings.Add($"Material `{assetPath}` has texture assigned to `{propertyName}`.");
                    passed = false;
                }
            }

            if (material.IsKeywordEnabled("_EMISSION"))
            {
                BlockingFindings.Add($"Material `{assetPath}` has `_EMISSION` keyword enabled.");
                passed = false;
            }

            if (material.HasProperty("_EmissionColor"))
            {
                var emission = material.GetColor("_EmissionColor");
                if (emission.maxColorComponent > 0.001f)
                {
                    BlockingFindings.Add($"Material `{assetPath}` has non-black emission color `{FormatColor(emission)}`.");
                    passed = false;
                }
            }

            return passed;
        }

        private static string DescribeMaterial(Material material, string assetPath)
        {
            var baseColor = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : material.color;
            return string.Format(
                CultureInfo.InvariantCulture,
                "`{0}` path=`{1}` shader=`{2}` baseColor=`{3}`",
                material.name,
                assetPath,
                material.shader == null ? "null" : material.shader.name,
                FormatColor(baseColor));
        }

        private static string? FindLineContaining(string text, string pattern)
        {
            using var reader = new StringReader(text);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains(pattern, StringComparison.Ordinal))
                {
                    return line;
                }
            }

            return null;
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

        private static void CaptureLog(string condition, string stackTrace, LogType type)
        {
            if (GravenspireScenarioSmokeRunnerHelpers.IsEditorStartupNoise(condition, stackTrace, type))
            {
                return;
            }

            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                Errors.Add(condition);
            }
            else if (type == LogType.Warning)
            {
                Warnings.Add(condition);
            }
        }

        private static void RecordCheck(string name, bool passed)
        {
            Checks.Add($"{name}={(passed ? "PASS" : "FAIL")}");
        }

        private static bool AllChecksPassed()
        {
            foreach (var check in Checks)
            {
                if (check.EndsWith("=FAIL", StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return Checks.Count > 0;
        }

        private static void WriteEvidenceAndExit(int exitCode)
        {
            var evidencePath = ResolveEvidencePathFromCommandLine(DefaultEvidencePath());
            Directory.CreateDirectory(Path.GetDirectoryName(evidencePath) ?? ".");

            var builder = new StringBuilder();
            builder.AppendLine("S3-05 Greybox Presentation Scan");
            builder.AppendLine();
            builder.AppendLine($"Date: {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine($"Story: production/stories/{StorySlug}.md");
            builder.AppendLine($"Scene: {ScenePath}");
            builder.AppendLine("Runner: Assets/Editor/GravenspireS3DistrictGreyboxPresentationScanVerificationRunner.cs");
            builder.AppendLine($"Result: {(exitCode == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine();
            builder.AppendLine("Scan Scope:");
            builder.AppendLine("- Scene object scan: Light components, AudioSource components, district primitive mesh geometry, rendered object material palette.");
            builder.AppendLine("- Scene source scan: serialized skybox material assignment.");
            builder.AppendLine("- Asset scan: Assets/Materials material location, greybox material texture/emission state, produced-art/audio file extensions under Assets.");
            builder.AppendLine();
            builder.AppendLine("Checks:");
            foreach (var check in Checks)
            {
                var parts = check.Split('=');
                var name = parts[0];
                var passed = parts.Length > 1 && parts[1] == "PASS";
                builder.AppendLine($"- {(passed ? "PASS" : "FAIL")} {name}");
            }

            builder.AppendLine();
            builder.AppendLine("Greybox Material Palette:");
            AppendEvidenceLines(builder, MaterialPalette);
            builder.AppendLine();
            builder.AppendLine("Classified Observations:");
            AppendEvidenceLines(builder, ClassifiedObservations);
            builder.AppendLine();
            builder.AppendLine("Blocking Findings:");
            AppendEvidenceLines(builder, BlockingFindings);
            builder.AppendLine();
            builder.AppendLine("Warnings:");
            AppendEvidenceLines(builder, Warnings);
            builder.AppendLine();
            builder.AppendLine("Errors:");
            AppendEvidenceLines(builder, Errors);

            File.WriteAllText(evidencePath, builder.ToString());
            Debug.Log($"{StoryId} greybox presentation scan wrote {evidencePath} with exit code {exitCode}.");
            ClearState();
            EditorApplication.Exit(exitCode);
        }

        private static void AppendEvidenceLines(StringBuilder builder, List<string> lines)
        {
            if (lines.Count == 0)
            {
                builder.AppendLine("- None.");
                return;
            }

            foreach (var line in lines)
            {
                builder.AppendLine($"- {line}");
            }
        }

        private static string ResolveEvidencePathFromCommandLine(string defaultEvidencePath)
        {
            var arguments = Environment.GetCommandLineArgs();
            for (var i = 0; i < arguments.Length - 1; i++)
            {
                if (string.Equals(arguments[i], EvidencePathArgumentName, StringComparison.OrdinalIgnoreCase))
                {
                    return arguments[i + 1];
                }
            }

            return defaultEvidencePath;
        }

        private static string DefaultEvidencePath()
        {
            return Path.Combine(
                "tests",
                "evidence",
                StoryId,
                $"greybox-presentation-scan-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.txt");
        }

        private static string GetScenePath(GameObject gameObject)
        {
            var names = new List<string>();
            var current = gameObject.transform;
            while (current != null)
            {
                names.Add(current.name);
                current = current.parent;
            }

            names.Reverse();
            return string.Join("/", names);
        }

        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }

        private static bool IsUnityBuiltinResource(string assetPath)
        {
            return assetPath.StartsWith("Library/unity default resources", StringComparison.Ordinal)
                || assetPath.StartsWith("Resources/unity_builtin_extra", StringComparison.Ordinal);
        }

        private static string FormatColor(Color color)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "rgba({0:0.###}, {1:0.###}, {2:0.###}, {3:0.###})",
                color.r,
                color.g,
                color.b,
                color.a);
        }

        private static void ClearState()
        {
            Checks.Clear();
            BlockingFindings.Clear();
            ClassifiedObservations.Clear();
            Warnings.Clear();
            Errors.Clear();
            MaterialPalette.Clear();
        }
    }
}
#endif
