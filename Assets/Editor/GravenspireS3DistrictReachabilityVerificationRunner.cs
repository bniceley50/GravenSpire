#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    public static class GravenspireS3DistrictReachabilityVerificationRunner
    {
        private const string StoryId = "S3-05";
        private const string StorySlug = "s3-05-navigable-greybox-first-district";
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const float SampleMaxDistanceMeters = 2.0f;

        private static readonly string[] AnchorNames =
        {
            "M3_Caretaker",
            "M3_ObjectiveRelic",
            "M3_CourtVendor"
        };

        private static readonly List<string> Checks = new();
        private static readonly List<string> Warnings = new();
        private static readonly List<string> Errors = new();
        private static readonly List<ReachabilityResult> Results = new();
        private static NavMeshBuildSettings? CapturedBuildSettings;
        private static int SurfaceAgentTypeId = -1;

        [MenuItem("Gravenspire/Verify S3 District Reachability")]
        public static void Run()
        {
            ClearState();
            Application.logMessageReceived += CaptureLog;
            var exitCode = 1;

            try
            {
                RunChecks();
                exitCode = AllChecksPassed() && Errors.Count == 0 ? 0 : 1;
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

            RecordCheck("first_district_greybox_root_exists", FindSceneObjectIncludingInactive("FirstDistrict_Greybox") != null);
            var surfaceObject = FindSceneObjectIncludingInactive("FirstDistrict_NavMeshSurface");
            RecordCheck("navmesh_surface_object_exists", surfaceObject != null);
            if (surfaceObject == null)
            {
                throw new IOException("FirstDistrict_NavMeshSurface is missing.");
            }

            RecordCheck("navmesh_surface_component_exists", surfaceObject.TryGetComponent<NavMeshSurface>(out var surface));
            if (surface == null)
            {
                throw new IOException("FirstDistrict_NavMeshSurface has no NavMeshSurface component.");
            }

            SurfaceAgentTypeId = surface.agentTypeID;
            RecordCheck("navmesh_surface_data_assigned", surface.navMeshData != null);
            if (surface.navMeshData == null)
            {
                throw new IOException("FirstDistrict_NavMeshSurface has no baked NavMeshData assigned.");
            }

            var buildSettings = NavMesh.GetSettingsByID(SurfaceAgentTypeId);
            CapturedBuildSettings = buildSettings;
            RecordCheck("navmesh_surface_agent_settings_resolved", buildSettings.agentTypeID == SurfaceAgentTypeId);
            if (buildSettings.agentTypeID != SurfaceAgentTypeId)
            {
                Warnings.Add($"NavMesh.GetSettingsByID({SurfaceAgentTypeId}) resolved agent type {buildSettings.agentTypeID}; evidence will record the resolved settings.");
            }

            surface.AddData();

            var spawn = SampleRequiredPosition("ClericShellMarker");
            RecordCheck("spawn_sampled_on_navmesh", spawn.Sampled);
            if (!spawn.Sampled)
            {
                throw new IOException("ClericShellMarker could not be sampled onto the baked NavMesh.");
            }

            foreach (var anchorName in AnchorNames)
            {
                Results.Add(CheckAnchor(spawn, anchorName));
            }

            RecordCheck("all_m3_anchors_reachable", Results.TrueForAll(result => result.Reachable));
            RecordCheck("m3_objective_relic_restored_inactive", FindSceneObjectIncludingInactive("M3_ObjectiveRelic")?.activeSelf == false);
        }

        private static ReachabilityResult CheckAnchor(SampledPosition spawn, string anchorName)
        {
            var anchor = FindSceneObjectIncludingInactive(anchorName);
            RecordCheck($"{anchorName}_exists", anchor != null);
            if (anchor == null)
            {
                return ReachabilityResult.Missing(anchorName);
            }

            var wasActiveSelf = anchor.activeSelf;
            if (!wasActiveSelf)
            {
                anchor.SetActive(true);
            }

            try
            {
                var target = SampleRequiredPosition(anchorName);
                RecordCheck($"{anchorName}_sampled_on_navmesh", target.Sampled);
                if (!target.Sampled)
                {
                    return ReachabilityResult.Unsampled(anchorName, wasActiveSelf, target.SampleDistanceMeters);
                }

                var path = new NavMeshPath();
                var calculated = NavMesh.CalculatePath(spawn.Position, target.Position, NavMesh.AllAreas, path);
                var reachable = calculated && path.status == NavMeshPathStatus.PathComplete;
                RecordCheck($"{anchorName}_path_complete", reachable);

                return new ReachabilityResult(
                    anchorName,
                    wasActiveSelf,
                    target.SampleDistanceMeters,
                    path.status.ToString(),
                    CalculatePathLength(path.corners),
                    CalculateMaxElevationDelta(path.corners),
                    reachable);
            }
            finally
            {
                anchor.SetActive(wasActiveSelf);
            }
        }

        private static SampledPosition SampleRequiredPosition(string objectName)
        {
            var gameObject = FindSceneObjectIncludingInactive(objectName);
            if (gameObject == null)
            {
                return SampledPosition.Missing;
            }

            if (!NavMesh.SamplePosition(gameObject.transform.position, out var hit, SampleMaxDistanceMeters, NavMesh.AllAreas))
            {
                Warnings.Add($"{objectName} could not be sampled onto the NavMesh within {SampleMaxDistanceMeters.ToString("0.0", CultureInfo.InvariantCulture)}m.");
                return new SampledPosition(false, gameObject.transform.position, float.NaN);
            }

            var sampleDistance = Vector3.Distance(gameObject.transform.position, hit.position);
            return new SampledPosition(true, hit.position, sampleDistance);
        }

        private static float CalculatePathLength(Vector3[] corners)
        {
            if (corners.Length < 2)
            {
                return 0.0f;
            }

            var length = 0.0f;
            for (var i = 1; i < corners.Length; i++)
            {
                length += Vector3.Distance(corners[i - 1], corners[i]);
            }

            return length;
        }

        private static float CalculateMaxElevationDelta(Vector3[] corners)
        {
            if (corners.Length == 0)
            {
                return 0.0f;
            }

            var minY = corners[0].y;
            var maxY = corners[0].y;
            for (var i = 1; i < corners.Length; i++)
            {
                minY = Mathf.Min(minY, corners[i].y);
                maxY = Mathf.Max(maxY, corners[i].y);
            }

            return maxY - minY;
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
            builder.AppendLine("# S3-05 First District Reachability Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine($"**Story:** `production/stories/{StorySlug}.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireS3DistrictReachabilityVerificationRunner.cs`");
            builder.AppendLine($"**Result:** {(exitCode == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine();
            AppendNavMeshProfile(builder);
            builder.AppendLine();
            builder.AppendLine("## Checks");
            builder.AppendLine();

            foreach (var check in Checks)
            {
                var parts = check.Split('=');
                var name = parts[0];
                var passed = parts.Length > 1 && parts[1] == "PASS";
                builder.AppendLine($"- {(passed ? "PASS" : "FAIL")} `{name}`");
            }

            builder.AppendLine();
            builder.AppendLine("## Anchor Reachability");
            builder.AppendLine();
            builder.AppendLine("| Anchor | Initial activeSelf | Sample distance (m) | Path status | Path length (m) | Max elevation delta (m) | Result |");
            builder.AppendLine("|---|---:|---:|---|---:|---:|---|");

            foreach (var result in Results)
            {
                builder.AppendLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "| `{0}` | {1} | {2} | {3} | {4} | {5} | {6} |",
                    result.AnchorName,
                    result.InitialActiveSelf,
                    FormatFloat(result.SampleDistanceMeters),
                    result.PathStatus,
                    FormatFloat(result.PathLengthMeters),
                    FormatFloat(result.MaxElevationDeltaMeters),
                    result.Reachable ? "PASS" : "FAIL"));
            }

            builder.AppendLine();
            builder.AppendLine("## Warnings");
            builder.AppendLine();
            AppendEvidenceLines(builder, Warnings);
            builder.AppendLine();
            builder.AppendLine("## Errors");
            builder.AppendLine();
            AppendEvidenceLines(builder, Errors);

            File.WriteAllText(evidencePath, builder.ToString());
            Debug.Log($"{StoryId} district reachability verification wrote {evidencePath} with exit code {exitCode}.");
            ClearState();
            EditorApplication.Exit(exitCode);
        }

        private static void AppendNavMeshProfile(StringBuilder builder)
        {
            builder.AppendLine("## NavMesh Profile");
            builder.AppendLine();
            builder.AppendLine($"- `surface_agent_type_id`: `{SurfaceAgentTypeId}`");

            if (CapturedBuildSettings.HasValue)
            {
                var settings = CapturedBuildSettings.Value;
                builder.AppendLine("- `settings_source`: `NavMesh.GetSettingsByID(surface_agent_type_id)`");
                builder.AppendLine($"- `resolved_agent_type_id`: `{settings.agentTypeID}`");
                builder.AppendLine($"- `agent_radius`: `{settings.agentRadius.ToString("0.###", CultureInfo.InvariantCulture)}`");
                builder.AppendLine($"- `agent_height`: `{settings.agentHeight.ToString("0.###", CultureInfo.InvariantCulture)}`");
                builder.AppendLine($"- `agent_slope`: `{settings.agentSlope.ToString("0.###", CultureInfo.InvariantCulture)}`");
                builder.AppendLine($"- `agent_climb`: `{settings.agentClimb.ToString("0.###", CultureInfo.InvariantCulture)}`");
            }
            else
            {
                builder.AppendLine("- NavMesh build settings were not captured.");
            }
        }

        private static string FormatFloat(float value)
        {
            return float.IsNaN(value)
                ? "n/a"
                : value.ToString("0.###", CultureInfo.InvariantCulture);
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
                $"reachability-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-smoke.md");
        }

        private static void AppendEvidenceLines(StringBuilder builder, List<string> lines)
        {
            if (lines.Count == 0)
            {
                builder.AppendLine("- None captured during runner execution.");
                return;
            }

            foreach (var line in lines)
            {
                builder.AppendLine($"- {line}");
            }
        }

        private static void ClearState()
        {
            Checks.Clear();
            Warnings.Clear();
            Errors.Clear();
            Results.Clear();
            CapturedBuildSettings = null;
            SurfaceAgentTypeId = -1;
        }

        private readonly struct SampledPosition
        {
            public static readonly SampledPosition Missing = new(false, Vector3.zero, float.NaN);

            public SampledPosition(bool sampled, Vector3 position, float sampleDistanceMeters)
            {
                Sampled = sampled;
                Position = position;
                SampleDistanceMeters = sampleDistanceMeters;
            }

            public bool Sampled { get; }

            public Vector3 Position { get; }

            public float SampleDistanceMeters { get; }
        }

        private readonly struct ReachabilityResult
        {
            public ReachabilityResult(
                string anchorName,
                bool initialActiveSelf,
                float sampleDistanceMeters,
                string pathStatus,
                float pathLengthMeters,
                float maxElevationDeltaMeters,
                bool reachable)
            {
                AnchorName = anchorName;
                InitialActiveSelf = initialActiveSelf;
                SampleDistanceMeters = sampleDistanceMeters;
                PathStatus = pathStatus;
                PathLengthMeters = pathLengthMeters;
                MaxElevationDeltaMeters = maxElevationDeltaMeters;
                Reachable = reachable;
            }

            public string AnchorName { get; }

            public bool InitialActiveSelf { get; }

            public float SampleDistanceMeters { get; }

            public string PathStatus { get; }

            public float PathLengthMeters { get; }

            public float MaxElevationDeltaMeters { get; }

            public bool Reachable { get; }

            public static ReachabilityResult Missing(string anchorName)
            {
                return new ReachabilityResult(anchorName, false, float.NaN, "Missing", 0.0f, 0.0f, false);
            }

            public static ReachabilityResult Unsampled(string anchorName, bool initialActiveSelf, float sampleDistanceMeters)
            {
                return new ReachabilityResult(anchorName, initialActiveSelf, sampleDistanceMeters, "Unsampled", 0.0f, 0.0f, false);
            }
        }
    }
}
#endif
