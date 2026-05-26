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
    public static class GravenspireS3DistrictSoftLockScanVerificationRunner
    {
        private const string StoryId = "S3-05";
        private const string StorySlug = "s3-05-navigable-greybox-first-district";
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const float GridMinMeters = -15.0f;
        private const float GridSpacingMeters = 1.0f;
        private const int GridCellsPerAxis = 30;
        private const float SampleMaxDistanceMeters = 1.0f;
        private const int MaxTrappedSamplesInEvidence = 25;

        private static readonly List<string> Checks = new();
        private static readonly List<string> Warnings = new();
        private static readonly List<string> Errors = new();
        private static readonly List<ScanSampleResult> TrappedSamples = new();
        private static readonly List<ScanSampleResult> IncompleteSamples = new();
        private static NavMeshBuildSettings? CapturedBuildSettings;
        private static ScanSummary Summary;
        private static int SurfaceAgentTypeId = -1;

        [MenuItem("Gravenspire/Verify S3 District Soft-Lock Scan")]
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

            ScanGrid(spawn.Position);
            RecordCheck("scan_found_on_mesh_samples", Summary.OnMeshSamples > 0);
            RecordCheck("zero_soft_lock_zones_detected", Summary.TrappedSamples == 0);
        }

        private static void ScanGrid(Vector3 sampledSpawn)
        {
            for (var xIndex = 0; xIndex < GridCellsPerAxis; xIndex++)
            {
                for (var zIndex = 0; zIndex < GridCellsPerAxis; zIndex++)
                {
                    var samplePoint = new Vector3(
                        GridMinMeters + (xIndex + 0.5f) * GridSpacingMeters,
                        0.0f,
                        GridMinMeters + (zIndex + 0.5f) * GridSpacingMeters);

                    Summary.TotalSamples++;
                    if (!NavMesh.SamplePosition(samplePoint, out var hit, SampleMaxDistanceMeters, NavMesh.AllAreas))
                    {
                        Summary.OffMeshSamples++;
                        continue;
                    }

                    Summary.OnMeshSamples++;

                    var path = new NavMeshPath();
                    var calculated = NavMesh.CalculatePath(hit.position, sampledSpawn, NavMesh.AllAreas, path);
                    var pathLength = CalculatePathLength(path.corners);
                    var result = new ScanSampleResult(
                        samplePoint,
                        hit.position,
                        Vector3.Distance(samplePoint, hit.position),
                        calculated,
                        path.status.ToString(),
                        pathLength);

                    if (calculated && path.status == NavMeshPathStatus.PathComplete)
                    {
                        Summary.ReachableSamples++;
                        continue;
                    }

                    Summary.TrappedSamples++;
                    TrappedSamples.Add(result);
                    if (path.status != NavMeshPathStatus.PathInvalid)
                    {
                        IncompleteSamples.Add(result);
                    }
                }
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

            return new SampledPosition(true, hit.position, Vector3.Distance(gameObject.transform.position, hit.position));
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
            builder.AppendLine("# S3-05 First District Soft-Lock Scan Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine($"**Story:** `production/stories/{StorySlug}.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireS3DistrictSoftLockScanVerificationRunner.cs`");
            builder.AppendLine($"**Result:** {(exitCode == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine();
            AppendMethodology(builder);
            builder.AppendLine();
            AppendNavMeshProfile(builder);
            builder.AppendLine();
            AppendGridSummary(builder);
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
            builder.AppendLine("## Trapped Samples");
            builder.AppendLine();
            AppendTrappedSamples(builder);
            builder.AppendLine();
            builder.AppendLine("## Warnings");
            builder.AppendLine();
            AppendEvidenceLines(builder, Warnings);
            builder.AppendLine();
            builder.AppendLine("## Errors");
            builder.AppendLine();
            AppendEvidenceLines(builder, Errors);

            File.WriteAllText(evidencePath, builder.ToString());
            Debug.Log($"{StoryId} district soft-lock scan wrote {evidencePath} with exit code {exitCode}.");
            ClearState();
            EditorApplication.Exit(exitCode);
        }

        private static void AppendMethodology(StringBuilder builder)
        {
            builder.AppendLine("## Methodology");
            builder.AppendLine();
            builder.AppendLine("- Claim framing: best-effort high-confidence scan, not exhaustive proof that no soft-lock can exist.");
            builder.AppendLine($"- Sample grid: {GridCellsPerAxis.ToString(CultureInfo.InvariantCulture)} x {GridCellsPerAxis.ToString(CultureInfo.InvariantCulture)} cell centers over the 30 m x 30 m district footprint.");
            builder.AppendLine($"- Grid spacing: {GridSpacingMeters.ToString("0.###", CultureInfo.InvariantCulture)} m.");
            builder.AppendLine($"- Sample query: `NavMesh.SamplePosition(sample, maxDistance={SampleMaxDistanceMeters.ToString("0.###", CultureInfo.InvariantCulture)} m, areaMask=NavMesh.AllAreas)`.");
            builder.AppendLine("- Path query: `NavMesh.CalculatePath(sampled_position, sampled_spawn, NavMesh.AllAreas, path)`.");
            builder.AppendLine("- Pass condition: every sample that resolves onto the NavMesh returns `PathComplete` back to `ClericShellMarker`.");
            builder.AppendLine("- Known gaps: a 1 m grid can miss geometric traps that require sub-meter alignment, narrow squeeze ledges with no grid point landing on them, and mesh-gap pockets between samples.");
            builder.AppendLine("- Complementary coverage: S3-05 AC-11 walkthrough evidence must cover human navigation and geometric edge cases this scan cannot prove.");
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

        private static void AppendGridSummary(StringBuilder builder)
        {
            builder.AppendLine("## Grid Summary");
            builder.AppendLine();
            builder.AppendLine($"- `total_samples`: `{Summary.TotalSamples.ToString(CultureInfo.InvariantCulture)}`");
            builder.AppendLine($"- `on_mesh_samples`: `{Summary.OnMeshSamples.ToString(CultureInfo.InvariantCulture)}`");
            builder.AppendLine($"- `off_mesh_samples`: `{Summary.OffMeshSamples.ToString(CultureInfo.InvariantCulture)}`");
            builder.AppendLine($"- `reachable_samples`: `{Summary.ReachableSamples.ToString(CultureInfo.InvariantCulture)}`");
            builder.AppendLine($"- `trapped_samples`: `{Summary.TrappedSamples.ToString(CultureInfo.InvariantCulture)}`");
            builder.AppendLine($"- `incomplete_non_invalid_samples`: `{IncompleteSamples.Count.ToString(CultureInfo.InvariantCulture)}`");
        }

        private static void AppendTrappedSamples(StringBuilder builder)
        {
            if (TrappedSamples.Count == 0)
            {
                builder.AppendLine("- None detected. Best-effort high-confidence result: zero soft-lock zones detected at 1 m grid sampling density.");
                return;
            }

            builder.AppendLine("| Grid sample | NavMesh hit | Sample distance (m) | CalculatePath | Path status | Path length (m) |");
            builder.AppendLine("|---|---|---:|---|---|---:|");

            var rows = Mathf.Min(TrappedSamples.Count, MaxTrappedSamplesInEvidence);
            for (var i = 0; i < rows; i++)
            {
                var sample = TrappedSamples[i];
                builder.AppendLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "| {0} | {1} | {2} | {3} | {4} | {5} |",
                    FormatVector(sample.GridSample),
                    FormatVector(sample.NavMeshHit),
                    FormatFloat(sample.SampleDistanceMeters),
                    sample.CalculatePathReturned,
                    sample.PathStatus,
                    FormatFloat(sample.PathLengthMeters)));
            }

            if (TrappedSamples.Count > MaxTrappedSamplesInEvidence)
            {
                builder.AppendLine($"- Evidence truncated to first {MaxTrappedSamplesInEvidence.ToString(CultureInfo.InvariantCulture)} trapped samples.");
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
                $"soft-lock-scan-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-smoke.md");
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

        private static string FormatVector(Vector3 value)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "({0}, {1}, {2})",
                FormatFloat(value.x),
                FormatFloat(value.y),
                FormatFloat(value.z));
        }

        private static string FormatFloat(float value)
        {
            return float.IsNaN(value)
                ? "n/a"
                : value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static void ClearState()
        {
            Checks.Clear();
            Warnings.Clear();
            Errors.Clear();
            TrappedSamples.Clear();
            IncompleteSamples.Clear();
            CapturedBuildSettings = null;
            Summary = default;
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

        private readonly struct ScanSampleResult
        {
            public ScanSampleResult(
                Vector3 gridSample,
                Vector3 navMeshHit,
                float sampleDistanceMeters,
                bool calculatePathReturned,
                string pathStatus,
                float pathLengthMeters)
            {
                GridSample = gridSample;
                NavMeshHit = navMeshHit;
                SampleDistanceMeters = sampleDistanceMeters;
                CalculatePathReturned = calculatePathReturned;
                PathStatus = pathStatus;
                PathLengthMeters = pathLengthMeters;
            }

            public Vector3 GridSample { get; }

            public Vector3 NavMeshHit { get; }

            public float SampleDistanceMeters { get; }

            public bool CalculatePathReturned { get; }

            public string PathStatus { get; }

            public float PathLengthMeters { get; }
        }

        private struct ScanSummary
        {
            public int TotalSamples;
            public int OnMeshSamples;
            public int OffMeshSamples;
            public int ReachableSamples;
            public int TrappedSamples;
        }
    }
}
#endif
