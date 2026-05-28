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
    public static class GravenspireS3DistrictNavMeshBakeScopeVerificationRunner
    {
        private const string StoryId = "S3-05";
        private const string StorySlug = "s3-05-navigable-greybox-first-district";
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const float TightProbeRadiusMeters = 0.3f;
        private const float WideProbeRadiusMeters = 3.0f;
        private const float AnchorProbeRadiusMeters = 0.3f;
        private const float AnchorMaxHorizontalDisplacementMeters = 0.3f;
        private const float BoundsToleranceMeters = 0.01f;
        private const float DisplacementEpsilonMeters = 0.001f;
        private const int NotWalkableArea = 1;

        private static readonly BakeObjectSpec[] RequiredBakeObjects =
        {
            new("DevEntry_DistrictBlockout_Floor", "walkable floor", requiresNotWalkableModifierVolume: false),
            new("Greybox_CaretakerHall_Massing", "landmark obstacle", requiresNotWalkableModifierVolume: true),
            new("Greybox_CourtVendorHall_Massing", "landmark obstacle", requiresNotWalkableModifierVolume: true),
            new("Greybox_RelicStorehouse_Massing", "landmark obstacle", requiresNotWalkableModifierVolume: true),
            new("GreyboxBoundary_North", "boundary obstacle", requiresNotWalkableModifierVolume: true),
            new("GreyboxBoundary_South", "boundary obstacle", requiresNotWalkableModifierVolume: true),
            new("GreyboxBoundary_East", "boundary obstacle", requiresNotWalkableModifierVolume: true),
            new("GreyboxBoundary_West", "boundary obstacle", requiresNotWalkableModifierVolume: true)
        };

        private static readonly string[] RuntimeObstacleNames =
        {
            "Greybox_CaretakerHall_Massing",
            "Greybox_CourtVendorHall_Massing",
            "Greybox_RelicStorehouse_Massing",
            "GreyboxBoundary_North",
            "GreyboxBoundary_South",
            "GreyboxBoundary_East",
            "GreyboxBoundary_West"
        };

        private static readonly string[] RuntimeAnchorNames =
        {
            "ClericShellMarker",
            "M3_Caretaker",
            "M3_CourtVendor",
            "M3_ObjectiveRelic",
            "M2_CampRestPoint",
            "M2_BaselineTrash",
            "M2_LinkedTrash",
            "M2_NamedBlocker",
            "M2_PullLane"
        };

        private static readonly List<string> Checks = new();
        private static readonly List<string> Warnings = new();
        private static readonly List<string> Errors = new();
        private static readonly List<StaticScopeResult> StaticScopeResults = new();
        private static readonly List<RuntimeProbeResult> RuntimeProbeResults = new();
        private static readonly List<AnchorProbeResult> AnchorProbeResults = new();
        private static string SurfaceScopeSummary = string.Empty;

        [MenuItem("Gravenspire/Verify S3 District NavMesh Bake Scope")]
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
            if (!scene.IsValid())
            {
                throw new IOException($"Failed to open scene at {ScenePath}.");
            }

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

            RecordSurfaceConfiguration(surfaceObject, surface);
            var volumeBounds = CalculateSurfaceVolumeBounds(surfaceObject.transform, surface);
            SurfaceScopeSummary = DescribeSurfaceScope(surfaceObject, surface, volumeBounds);

            foreach (var spec in RequiredBakeObjects)
            {
                StaticScopeResults.Add(CheckStaticBakeScope(spec, surfaceObject, surface, volumeBounds));
            }

            RecordCheck("all_required_objects_in_declared_bake_scope", StaticScopeResults.TrueForAll(result => result.InDeclaredScope));
            RecordCheck("all_required_objects_have_enabled_colliders", StaticScopeResults.TrueForAll(result => result.EnabledColliderCount > 0));

            RecordCheck("navmesh_surface_data_assigned", surface.navMeshData != null);
            if (surface.navMeshData == null)
            {
                throw new IOException("FirstDistrict_NavMeshSurface has no baked NavMeshData assigned.");
            }

            surface.AddData();

            foreach (var obstacleName in RuntimeObstacleNames)
            {
                RuntimeProbeResults.Add(CheckRuntimeObstacleCarve(obstacleName));
            }

            RecordCheck("all_obstacle_tight_probes_fail", RuntimeProbeResults.TrueForAll(result => !result.TightSampled));
            RecordCheck("all_obstacle_wide_probes_resolve_displaced", RuntimeProbeResults.TrueForAll(result => result.WideSampled && result.WideHorizontalDisplacementMeters + DisplacementEpsilonMeters >= result.RequiredDisplacementMeters));

            foreach (var anchorName in RuntimeAnchorNames)
            {
                AnchorProbeResults.Add(CheckRuntimeAnchorClearance(anchorName));
            }

            RecordCheck("all_anchor_tight_probes_resolve_on_navmesh", AnchorProbeResults.TrueForAll(result => result.Sampled));
            RecordCheck("all_anchor_tight_probes_low_displacement", AnchorProbeResults.TrueForAll(result => result.Sampled && result.HorizontalDisplacementMeters <= AnchorMaxHorizontalDisplacementMeters + DisplacementEpsilonMeters));
        }

        private static void RecordSurfaceConfiguration(GameObject surfaceObject, NavMeshSurface surface)
        {
            RecordCheck("surface_collects_children", surface.collectObjects == CollectObjects.Children);
            RecordCheck("surface_uses_physics_colliders", surface.useGeometry == NavMeshCollectGeometry.PhysicsColliders);
            RecordCheck("surface_transform_rotation_identity", Quaternion.Angle(surfaceObject.transform.rotation, Quaternion.identity) <= 0.01f);
            RecordCheck("surface_transform_scale_identity", VectorApproximately(surfaceObject.transform.lossyScale, Vector3.one, BoundsToleranceMeters));
        }

        private static StaticScopeResult CheckStaticBakeScope(
            BakeObjectSpec spec,
            GameObject surfaceObject,
            NavMeshSurface surface,
            Bounds volumeBounds)
        {
            var gameObject = FindSceneObjectIncludingInactive(spec.ObjectName);
            RecordCheck($"{spec.ObjectName}_exists", gameObject != null);
            if (gameObject == null)
            {
                return StaticScopeResult.Missing(spec);
            }

            var colliders = gameObject.GetComponentsInChildren<Collider>(includeInactive: true);
            var enabledColliders = new List<Collider>();
            foreach (var collider in colliders)
            {
                if (collider.enabled && collider.gameObject.activeInHierarchy)
                {
                    enabledColliders.Add(collider);
                }
            }

            RecordCheck($"{spec.ObjectName}_has_enabled_collider", enabledColliders.Count > 0);

            var layerIncluded = IsLayerIncluded(surface.layerMask, gameObject.layer);
            RecordCheck($"{spec.ObjectName}_layer_in_surface_mask", layerIncluded);

            var scopeDetails = "Skipped because the object had no enabled collider or its layer was outside the surface layer mask.";
            var inDeclaredScope = false;
            if (enabledColliders.Count > 0 && layerIncluded)
            {
                inDeclaredScope = IsInDeclaredCollectionScope(
                    gameObject,
                    surfaceObject,
                    surface,
                    enabledColliders,
                    volumeBounds,
                    out scopeDetails);
            }

            RecordCheck($"{spec.ObjectName}_included_in_declared_bake_scope", inDeclaredScope);
            var modifierStatus = ResolveNotWalkableModifierStatus(spec, gameObject, out var modifierDetails);
            if (spec.RequiresNotWalkableModifierVolume)
            {
                RecordCheck($"{spec.ObjectName}_has_not_walkable_modifier_volume", modifierStatus == "PASS");
            }

            return new StaticScopeResult(
                spec.ObjectName,
                spec.Role,
                gameObject.activeInHierarchy,
                gameObject.layer,
                enabledColliders.Count,
                layerIncluded,
                inDeclaredScope,
                modifierStatus,
                scopeDetails,
                modifierDetails);
        }

        private static string ResolveNotWalkableModifierStatus(
            BakeObjectSpec spec,
            GameObject gameObject,
            out string details)
        {
            if (!spec.RequiresNotWalkableModifierVolume)
            {
                details = "Not required for this bake object.";
                return "n/a";
            }

            if (!gameObject.TryGetComponent<NavMeshModifierVolume>(out var modifierVolume))
            {
                details = "NavMeshModifierVolume missing.";
                return "FAIL";
            }

            var centerMatches = VectorApproximately(modifierVolume.center, Vector3.zero, BoundsToleranceMeters);
            var sizeMatches = VectorApproximately(modifierVolume.size, Vector3.one, BoundsToleranceMeters);
            var areaMatches = modifierVolume.area == NotWalkableArea;
            details = $"area={modifierVolume.area}, center={FormatVector(modifierVolume.center)}, size={FormatVector(modifierVolume.size)}, enabled={modifierVolume.isActiveAndEnabled}";

            return modifierVolume.isActiveAndEnabled && areaMatches && centerMatches && sizeMatches
                ? "PASS"
                : "FAIL";
        }

        private static bool IsInDeclaredCollectionScope(
            GameObject gameObject,
            GameObject surfaceObject,
            NavMeshSurface surface,
            List<Collider> enabledColliders,
            Bounds volumeBounds,
            out string details)
        {
            if (surface.collectObjects == CollectObjects.All)
            {
                details = "CollectObjects.All includes all enabled scene colliders matching the layer mask.";
                return true;
            }

            if (surface.collectObjects == CollectObjects.Children)
            {
                var isChild = gameObject.transform.IsChildOf(surfaceObject.transform);
                details = isChild
                    ? "Object is a descendant of FirstDistrict_NavMeshSurface."
                    : "Object is not a descendant of FirstDistrict_NavMeshSurface.";
                return isChild;
            }

            if (surface.collectObjects == CollectObjects.Volume)
            {
                var allCollidersInside = true;
                var colliderSummaries = new List<string>();
                foreach (var collider in enabledColliders)
                {
                    var inside = BoundsContainsWithTolerance(volumeBounds, collider.bounds, BoundsToleranceMeters);
                    allCollidersInside &= inside;
                    colliderSummaries.Add($"{collider.GetType().Name} bounds={FormatBounds(collider.bounds)} insideVolume={inside}");
                }

                details = $"Object collider bounds within volume {FormatBounds(volumeBounds)}: {string.Join("; ", colliderSummaries)}";
                return allCollidersInside;
            }

            details = $"Unsupported NavMeshSurface collectObjects mode `{surface.collectObjects}`.";
            return false;
        }

        private static RuntimeProbeResult CheckRuntimeObstacleCarve(string obstacleName)
        {
            var gameObject = FindSceneObjectIncludingInactive(obstacleName);
            RecordCheck($"{obstacleName}_runtime_obstacle_exists", gameObject != null);
            if (gameObject == null)
            {
                return RuntimeProbeResult.Missing(obstacleName);
            }

            var colliderBounds = CalculateCombinedColliderBounds(gameObject);
            RecordCheck($"{obstacleName}_runtime_collider_bounds_resolved", colliderBounds.HasValue);
            if (!colliderBounds.HasValue)
            {
                return RuntimeProbeResult.MissingCollider(obstacleName, gameObject.transform.position);
            }

            var requiredDisplacement = Mathf.Min(colliderBounds.Value.extents.x, colliderBounds.Value.extents.z);
            var queryGroundCenter = new Vector3(gameObject.transform.position.x, 0.0f, gameObject.transform.position.z);

            var tightSampled = NavMesh.SamplePosition(queryGroundCenter, out var tightHit, TightProbeRadiusMeters, NavMesh.AllAreas);
            var wideSampled = NavMesh.SamplePosition(queryGroundCenter, out var wideHit, WideProbeRadiusMeters, NavMesh.AllAreas);
            var tightHorizontalDisplacement = tightSampled ? HorizontalDistance(queryGroundCenter, tightHit.position) : float.NaN;
            var wideHorizontalDisplacement = wideSampled ? HorizontalDistance(queryGroundCenter, wideHit.position) : float.NaN;

            RecordCheck($"{obstacleName}_tight_probe_fails_inside_obstacle", !tightSampled);
            RecordCheck($"{obstacleName}_wide_probe_resolves_near_perimeter", wideSampled);
            RecordCheck(
                $"{obstacleName}_wide_probe_displacement_matches_footprint",
                wideSampled && wideHorizontalDisplacement + DisplacementEpsilonMeters >= requiredDisplacement);

            return new RuntimeProbeResult(
                obstacleName,
                queryGroundCenter,
                TightProbeRadiusMeters,
                tightSampled,
                tightSampled ? tightHit.position : Vector3.zero,
                tightHorizontalDisplacement,
                WideProbeRadiusMeters,
                wideSampled,
                wideSampled ? wideHit.position : Vector3.zero,
                wideHorizontalDisplacement,
                requiredDisplacement);
        }

        private static AnchorProbeResult CheckRuntimeAnchorClearance(string anchorName)
        {
            var gameObject = FindSceneObjectIncludingInactive(anchorName);
            RecordCheck($"{anchorName}_anchor_exists", gameObject != null);
            if (gameObject == null)
            {
                return AnchorProbeResult.Missing(anchorName);
            }

            var queryGroundCenter = new Vector3(gameObject.transform.position.x, 0.0f, gameObject.transform.position.z);
            var sampled = NavMesh.SamplePosition(queryGroundCenter, out var hit, AnchorProbeRadiusMeters, NavMesh.AllAreas);
            var horizontalDisplacement = sampled ? HorizontalDistance(queryGroundCenter, hit.position) : float.NaN;
            var verticalDisplacement = sampled ? Mathf.Abs(hit.position.y - queryGroundCenter.y) : float.NaN;

            RecordCheck($"{anchorName}_anchor_probe_resolves_on_navmesh", sampled);
            RecordCheck(
                $"{anchorName}_anchor_probe_low_horizontal_displacement",
                sampled && horizontalDisplacement <= AnchorMaxHorizontalDisplacementMeters + DisplacementEpsilonMeters);

            return new AnchorProbeResult(
                anchorName,
                queryGroundCenter,
                AnchorProbeRadiusMeters,
                sampled,
                sampled ? hit.position : Vector3.zero,
                horizontalDisplacement,
                verticalDisplacement);
        }

        private static Bounds CalculateSurfaceVolumeBounds(Transform surfaceTransform, NavMeshSurface surface)
        {
            var worldCenter = surfaceTransform.TransformPoint(surface.center);
            var lossyScale = surfaceTransform.lossyScale;
            var scaledSize = new Vector3(
                Mathf.Abs(surface.size.x * lossyScale.x),
                Mathf.Abs(surface.size.y * lossyScale.y),
                Mathf.Abs(surface.size.z * lossyScale.z));
            return new Bounds(worldCenter, scaledSize);
        }

        private static Bounds? CalculateCombinedColliderBounds(GameObject gameObject)
        {
            var colliders = gameObject.GetComponentsInChildren<Collider>(includeInactive: true);
            Bounds? combined = null;
            foreach (var collider in colliders)
            {
                if (!collider.enabled || !collider.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!combined.HasValue)
                {
                    combined = collider.bounds;
                }
                else
                {
                    var bounds = combined.Value;
                    bounds.Encapsulate(collider.bounds);
                    combined = bounds;
                }
            }

            return combined;
        }

        private static bool BoundsContainsWithTolerance(Bounds outer, Bounds inner, float tolerance)
        {
            return inner.min.x >= outer.min.x - tolerance &&
                inner.min.y >= outer.min.y - tolerance &&
                inner.min.z >= outer.min.z - tolerance &&
                inner.max.x <= outer.max.x + tolerance &&
                inner.max.y <= outer.max.y + tolerance &&
                inner.max.z <= outer.max.z + tolerance;
        }

        private static bool IsLayerIncluded(LayerMask layerMask, int layer)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }

        private static float HorizontalDistance(Vector3 query, Vector3 sample)
        {
            query.y = 0.0f;
            sample.y = 0.0f;
            return Vector3.Distance(query, sample);
        }

        private static bool VectorApproximately(Vector3 actual, Vector3 expected, float tolerance)
        {
            return Mathf.Abs(actual.x - expected.x) <= tolerance &&
                Mathf.Abs(actual.y - expected.y) <= tolerance &&
                Mathf.Abs(actual.z - expected.z) <= tolerance;
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
            builder.AppendLine("# S3-05 NavMesh Bake Scope Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine($"**Story:** `production/stories/{StorySlug}.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireS3DistrictNavMeshBakeScopeVerificationRunner.cs`");
            builder.AppendLine($"**Result:** {(exitCode == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine();
            AppendMethodology(builder);
            builder.AppendLine();
            builder.AppendLine("## Surface Scope");
            builder.AppendLine();
            builder.AppendLine(SurfaceScopeSummary.Length == 0 ? "- Surface scope was not captured." : SurfaceScopeSummary);
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
            AppendStaticScopeResults(builder);
            builder.AppendLine();
            AppendRuntimeProbeResults(builder);
            builder.AppendLine();
            AppendAnchorProbeResults(builder);
            builder.AppendLine();
            builder.AppendLine("## Warnings");
            builder.AppendLine();
            AppendEvidenceLines(builder, Warnings);
            builder.AppendLine();
            builder.AppendLine("## Errors");
            builder.AppendLine();
            AppendEvidenceLines(builder, Errors);

            File.WriteAllText(evidencePath, builder.ToString());
            Debug.Log($"{StoryId} NavMesh bake-scope verification wrote {evidencePath} with exit code {exitCode}.");
            ClearState();
            EditorApplication.Exit(exitCode);
        }

        private static void AppendMethodology(StringBuilder builder)
        {
            builder.AppendLine("## Methodology");
            builder.AppendLine();
            builder.AppendLine("- Static intent check: verify `FirstDistrict_NavMeshSurface` uses `CollectObjects.Children`, `PhysicsColliders`, matching layer mask, enabled colliders, and declared collection-scope inclusion for floor, 3 landmark massings, and 4 boundary walls.");
            builder.AppendLine("- Static carve-mechanism check: verify all 3 landmark massings and 4 boundary walls carry an active `NavMeshModifierVolume` with area `1` (Not Walkable), local center `(0, 0, 0)`, and local size `(1, 1, 1)`.");
            builder.AppendLine($"- Runtime outcome check: at each obstacle ground center, `NavMesh.SamplePosition` with radius {TightProbeRadiusMeters.ToString("0.###", CultureInfo.InvariantCulture)} m must fail, proving no NavMesh exists inside the obstacle footprint.");
            builder.AppendLine($"- Runtime perimeter check: a wider {WideProbeRadiusMeters.ToString("0.###", CultureInfo.InvariantCulture)} m probe must resolve to a horizontally displaced point at least as far as the obstacle's minimum X/Z half-footprint, proving the nearest NavMesh sits at the obstacle perimeter rather than inside it.");
            builder.AppendLine($"- Runtime anchor-clearance check: at each gameplay anchor ground center, `NavMesh.SamplePosition` with radius {AnchorProbeRadiusMeters.ToString("0.###", CultureInfo.InvariantCulture)} m must succeed with horizontal displacement no greater than {AnchorMaxHorizontalDisplacementMeters.ToString("0.###", CultureInfo.InvariantCulture)} m, proving anchors remain on walkable NavMesh after obstacle carving.");
        }

        private static void AppendStaticScopeResults(StringBuilder builder)
        {
            builder.AppendLine("## Static Bake-Scope Assertions");
            builder.AppendLine();
            builder.AppendLine("| Object | Role | Active | Layer | Enabled colliders | Layer included | In declared scope | Not-walkable volume | Details |");
            builder.AppendLine("|---|---|---:|---:|---:|---:|---:|---:|---|");

            foreach (var result in StaticScopeResults)
            {
                builder.AppendLine($"| `{result.ObjectName}` | {result.Role} | {result.ActiveInHierarchy} | {result.Layer} | {result.EnabledColliderCount} | {result.LayerIncluded} | {result.InDeclaredScope} | {result.NotWalkableModifierStatus} | {EscapeTableCell(result.ScopeDetails)} {EscapeTableCell(result.ModifierDetails)} |");
            }
        }

        private static void AppendRuntimeProbeResults(StringBuilder builder)
        {
            builder.AppendLine("## Runtime Obstacle-Carve Assertions");
            builder.AppendLine();
            builder.AppendLine("| Obstacle | Query ground center | Tight probe | Wide resolved position | Wide horizontal displacement (m) | Required displacement (m) | Result |");
            builder.AppendLine("|---|---|---|---|---:|---:|---|");

            foreach (var result in RuntimeProbeResults)
            {
                var tight = result.TightSampled
                    ? $"SAMPLED {FormatVector(result.TightResolvedPosition)} / displacement {FormatFloat(result.TightHorizontalDisplacementMeters)} m"
                    : "not sampled (expected)";
                var wide = result.WideSampled ? FormatVector(result.WideResolvedPosition) : "not sampled";
                var passed = !result.TightSampled &&
                    result.WideSampled &&
                    result.WideHorizontalDisplacementMeters + DisplacementEpsilonMeters >= result.RequiredDisplacementMeters;

                builder.AppendLine($"| `{result.ObstacleName}` | {FormatVector(result.QueryGroundCenter)} | {tight} | {wide} | {FormatFloat(result.WideHorizontalDisplacementMeters)} | {FormatFloat(result.RequiredDisplacementMeters)} | {(passed ? "PASS" : "FAIL")} |");
            }
        }

        private static void AppendAnchorProbeResults(StringBuilder builder)
        {
            builder.AppendLine("## Runtime Anchor-Clearance Assertions");
            builder.AppendLine();
            builder.AppendLine("| Anchor | Query ground center | Probe radius (m) | Resolved position | Horizontal displacement (m) | Vertical displacement (m) | Result |");
            builder.AppendLine("|---|---|---:|---|---:|---:|---|");

            foreach (var result in AnchorProbeResults)
            {
                var resolved = result.Sampled ? FormatVector(result.ResolvedPosition) : "not sampled";
                var passed = result.Sampled &&
                    result.HorizontalDisplacementMeters <= AnchorMaxHorizontalDisplacementMeters + DisplacementEpsilonMeters;

                builder.AppendLine($"| `{result.AnchorName}` | {FormatVector(result.QueryGroundCenter)} | {FormatFloat(result.ProbeRadiusMeters)} | {resolved} | {FormatFloat(result.HorizontalDisplacementMeters)} | {FormatFloat(result.VerticalDisplacementMeters)} | {(passed ? "PASS" : "FAIL")} |");
            }
        }

        private static string DescribeSurfaceScope(GameObject surfaceObject, NavMeshSurface surface, Bounds volumeBounds)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"- `surface_object`: `{GetScenePath(surfaceObject)}`");
            builder.AppendLine($"- `collect_objects`: `{surface.collectObjects}`");
            builder.AppendLine($"- `use_geometry`: `{surface.useGeometry}`");
            builder.AppendLine($"- `layer_mask`: `{surface.layerMask.value}`");
            builder.AppendLine($"- `surface_center`: `{FormatVector(surface.center)}`");
            builder.AppendLine($"- `surface_size`: `{FormatVector(surface.size)}`");
            builder.AppendLine($"- `world_volume_bounds`: `{FormatBounds(volumeBounds)}` (informational; ignored by `CollectObjects.Children`)");
            return builder.ToString().TrimEnd();
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
                $"navmesh-bake-scope-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-smoke.md");
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

        private static string FormatBounds(Bounds bounds)
        {
            return $"center={FormatVector(bounds.center)}, size={FormatVector(bounds.size)}, min={FormatVector(bounds.min)}, max={FormatVector(bounds.max)}";
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

        private static string EscapeTableCell(string value)
        {
            return value.Replace("|", "\\|");
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
            StaticScopeResults.Clear();
            RuntimeProbeResults.Clear();
            AnchorProbeResults.Clear();
            SurfaceScopeSummary = string.Empty;
        }

        private readonly struct BakeObjectSpec
        {
            public BakeObjectSpec(string objectName, string role, bool requiresNotWalkableModifierVolume)
            {
                ObjectName = objectName;
                Role = role;
                RequiresNotWalkableModifierVolume = requiresNotWalkableModifierVolume;
            }

            public string ObjectName { get; }

            public string Role { get; }

            public bool RequiresNotWalkableModifierVolume { get; }
        }

        private readonly struct StaticScopeResult
        {
            public StaticScopeResult(
                string objectName,
                string role,
                bool activeInHierarchy,
                int layer,
                int enabledColliderCount,
                bool layerIncluded,
                bool inDeclaredScope,
                string notWalkableModifierStatus,
                string scopeDetails,
                string modifierDetails)
            {
                ObjectName = objectName;
                Role = role;
                ActiveInHierarchy = activeInHierarchy;
                Layer = layer;
                EnabledColliderCount = enabledColliderCount;
                LayerIncluded = layerIncluded;
                InDeclaredScope = inDeclaredScope;
                NotWalkableModifierStatus = notWalkableModifierStatus;
                ScopeDetails = scopeDetails;
                ModifierDetails = modifierDetails;
            }

            public string ObjectName { get; }

            public string Role { get; }

            public bool ActiveInHierarchy { get; }

            public int Layer { get; }

            public int EnabledColliderCount { get; }

            public bool LayerIncluded { get; }

            public bool InDeclaredScope { get; }

            public string NotWalkableModifierStatus { get; }

            public string ScopeDetails { get; }

            public string ModifierDetails { get; }

            public static StaticScopeResult Missing(BakeObjectSpec spec)
            {
                return new StaticScopeResult(spec.ObjectName, spec.Role, false, -1, 0, false, false, spec.RequiresNotWalkableModifierVolume ? "FAIL" : "n/a", "Object missing from scene.", "Modifier check skipped because object is missing.");
            }
        }

        private readonly struct RuntimeProbeResult
        {
            public RuntimeProbeResult(
                string obstacleName,
                Vector3 queryGroundCenter,
                float tightRadiusMeters,
                bool tightSampled,
                Vector3 tightResolvedPosition,
                float tightHorizontalDisplacementMeters,
                float wideRadiusMeters,
                bool wideSampled,
                Vector3 wideResolvedPosition,
                float wideHorizontalDisplacementMeters,
                float requiredDisplacementMeters)
            {
                ObstacleName = obstacleName;
                QueryGroundCenter = queryGroundCenter;
                TightRadiusMeters = tightRadiusMeters;
                TightSampled = tightSampled;
                TightResolvedPosition = tightResolvedPosition;
                TightHorizontalDisplacementMeters = tightHorizontalDisplacementMeters;
                WideRadiusMeters = wideRadiusMeters;
                WideSampled = wideSampled;
                WideResolvedPosition = wideResolvedPosition;
                WideHorizontalDisplacementMeters = wideHorizontalDisplacementMeters;
                RequiredDisplacementMeters = requiredDisplacementMeters;
            }

            public string ObstacleName { get; }

            public Vector3 QueryGroundCenter { get; }

            public float TightRadiusMeters { get; }

            public bool TightSampled { get; }

            public Vector3 TightResolvedPosition { get; }

            public float TightHorizontalDisplacementMeters { get; }

            public float WideRadiusMeters { get; }

            public bool WideSampled { get; }

            public Vector3 WideResolvedPosition { get; }

            public float WideHorizontalDisplacementMeters { get; }

            public float RequiredDisplacementMeters { get; }

            public static RuntimeProbeResult Missing(string obstacleName)
            {
                return new RuntimeProbeResult(obstacleName, Vector3.zero, TightProbeRadiusMeters, false, Vector3.zero, float.NaN, WideProbeRadiusMeters, false, Vector3.zero, float.NaN, float.NaN);
            }

            public static RuntimeProbeResult MissingCollider(string obstacleName, Vector3 queryGroundCenter)
            {
                return new RuntimeProbeResult(obstacleName, queryGroundCenter, TightProbeRadiusMeters, false, Vector3.zero, float.NaN, WideProbeRadiusMeters, false, Vector3.zero, float.NaN, float.NaN);
            }
        }

        private readonly struct AnchorProbeResult
        {
            public AnchorProbeResult(
                string anchorName,
                Vector3 queryGroundCenter,
                float probeRadiusMeters,
                bool sampled,
                Vector3 resolvedPosition,
                float horizontalDisplacementMeters,
                float verticalDisplacementMeters)
            {
                AnchorName = anchorName;
                QueryGroundCenter = queryGroundCenter;
                ProbeRadiusMeters = probeRadiusMeters;
                Sampled = sampled;
                ResolvedPosition = resolvedPosition;
                HorizontalDisplacementMeters = horizontalDisplacementMeters;
                VerticalDisplacementMeters = verticalDisplacementMeters;
            }

            public string AnchorName { get; }

            public Vector3 QueryGroundCenter { get; }

            public float ProbeRadiusMeters { get; }

            public bool Sampled { get; }

            public Vector3 ResolvedPosition { get; }

            public float HorizontalDisplacementMeters { get; }

            public float VerticalDisplacementMeters { get; }

            public static AnchorProbeResult Missing(string anchorName)
            {
                return new AnchorProbeResult(anchorName, Vector3.zero, AnchorProbeRadiusMeters, false, Vector3.zero, float.NaN, float.NaN);
            }
        }
    }
}
#endif
