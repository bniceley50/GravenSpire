#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Gravenspire.UnityRuntime.Interaction;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    [InitializeOnLoad]
    public static class GravenspireS3DistrictCompositeObjectiveLoopVerificationRunner
    {
        private const string StoryId = "S3-05";
        private const string StorySlug = "s3-05-navigable-greybox-first-district";
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string RunKey = "GravenspireS3DistrictCompositeObjectiveLoop.Run";
        private const string PhaseKey = "GravenspireS3DistrictCompositeObjectiveLoop.Phase";
        private const string ChecksKey = "GravenspireS3DistrictCompositeObjectiveLoop.Checks";
        private const string ErrorsKey = "GravenspireS3DistrictCompositeObjectiveLoop.Errors";
        private const string WarningsKey = "GravenspireS3DistrictCompositeObjectiveLoop.Warnings";
        private const string TelemetryKey = "GravenspireS3DistrictCompositeObjectiveLoop.Telemetry";
        private const string DownstreamKey = "GravenspireS3DistrictCompositeObjectiveLoop.Downstream";
        private const string EvidencePathKey = "GravenspireS3DistrictCompositeObjectiveLoop.EvidencePath";
        private const string PlayStartedKey = "GravenspireS3DistrictCompositeObjectiveLoop.PlayStartedSeconds";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const double SmokeDelaySeconds = 1.0d;
        private const float SampleMaxDistanceMeters = 2.0f;
        private const float ProbeApproachDistanceMeters = 1.0f;
        private const string FullChainVocabulary =
            "npc_interaction_intentional|objective_accepted|relic_recovered|objective_loot_resolved|vendor_salvage_sold|vendor_sell_copper_applied|relic_handed_in";

        private static readonly string[] DownstreamAnchorNames =
        {
            "M3_Caretaker",
            "M3_ObjectiveRelic",
            "M3_CourtVendor"
        };

        static GravenspireS3DistrictCompositeObjectiveLoopVerificationRunner()
        {
            if (!SessionState.GetBool(RunKey, false))
            {
                return;
            }

            Application.logMessageReceived -= CaptureLog;
            Application.logMessageReceived += CaptureLog;
            EditorApplication.update -= ContinueAfterDomainReload;
            EditorApplication.update += ContinueAfterDomainReload;
        }

        [MenuItem("Gravenspire/Verify S3 District Composite Objective Loop")]
        public static void Run()
        {
            ClearSession();
            SessionState.SetBool(RunKey, true);
            SessionState.SetString(PhaseKey, "initial");
            Application.logMessageReceived -= CaptureLog;
            Application.logMessageReceived += CaptureLog;

            try
            {
                var evidencePath = ResolveEvidencePathFromCommandLine(DefaultEvidencePath());
                SessionState.SetString(EvidencePathKey, evidencePath);
                Directory.CreateDirectory(Path.GetDirectoryName(evidencePath) ?? ".");

                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                RecordCheck("scene_loaded", scene.IsValid() && scene.path == ScenePath);
                RecordCheck("first_district_greybox_root_exists", FindSceneObjectIncludingInactive("FirstDistrict_Greybox") != null);
                RecordCheck("harness_root_exists", FindSceneObjectIncludingInactive(S3PlayerInteractionHarness.HarnessRootName) != null);
                RecordCheck("cleric_shell_marker_exists", FindSceneObjectIncludingInactive(S3PlayerInteractionHarness.ClericMarkerObjectName) != null);
                RecordCheck("m3_caretaker_anchor_exists", FindSceneObjectIncludingInactive("M3_Caretaker") != null);
                RecordCheck("m3_objective_relic_anchor_exists", FindSceneObjectIncludingInactive("M3_ObjectiveRelic") != null);
                RecordCheck("m3_court_vendor_anchor_exists", FindSceneObjectIncludingInactive("M3_CourtVendor") != null);
                RecordCheck("navmesh_surface_ready", FindNavMeshSurface() is { navMeshData: not null });

                AppendSessionLine(DownstreamKey, "S3-01 status: closed; S3 player interaction harness is present and asserted in this runner.");
                AppendSessionLine(DownstreamKey, "S3-02 status at S3-05 Phase 6: ready, not closed; NPC adapter vocabulary is catalogued but not asserted.");
                AppendSessionLine(DownstreamKey, "S3-03 status at S3-05 Phase 6: blocked on S3-02; relic/objective adapter vocabulary is catalogued but not asserted.");
                AppendSessionLine(DownstreamKey, "S3-04 status at S3-05 Phase 6: blocked on S3-03; vendor adapter vocabulary is catalogued but not asserted.");
                AppendSessionLine(DownstreamKey, "AC-12 closure semantics: partial-pass rolls full-chain assertion forward to S3-06; production/sprint-status.yaml carryover is a Phase 8 closure artifact.");

                SessionState.SetString(PhaseKey, "entering_play");
                EditorApplication.update -= ContinueAfterDomainReload;
                EditorApplication.update += ContinueAfterDomainReload;
                EditorApplication.isPlaying = true;
            }
            catch (Exception ex)
            {
                AppendSessionLine(ErrorsKey, ex.ToString());
                WriteEvidenceAndExit(1);
            }
        }

        private static void ContinueAfterDomainReload()
        {
            if (!SessionState.GetBool(RunKey, false))
            {
                EditorApplication.update -= ContinueAfterDomainReload;
                Application.logMessageReceived -= CaptureLog;
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                return;
            }

            var phase = SessionState.GetString(PhaseKey, string.Empty);
            if (phase == "entering_play")
            {
                SessionState.SetString(PhaseKey, "playing");
                SessionState.SetString(PlayStartedKey, EditorApplication.timeSinceStartup.ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (phase != "playing")
            {
                return;
            }

            if (!double.TryParse(
                    SessionState.GetString(PlayStartedKey, "0"),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var startedSeconds) ||
                EditorApplication.timeSinceStartup - startedSeconds < SmokeDelaySeconds)
            {
                return;
            }

            try
            {
                RunSmokeChecks();
                WriteEvidenceAndExit(AllChecksPassed() && GetSessionLines(ErrorsKey).Count == 0 ? 0 : 1);
            }
            catch (Exception ex)
            {
                AppendSessionLine(ErrorsKey, ex.ToString());
                WriteEvidenceAndExit(1);
            }
        }

        private static void RunSmokeChecks()
        {
            var harness = UnityEngine.Object.FindFirstObjectByType<S3PlayerInteractionHarness>();
            RecordCheck("harness_component_found_in_play_mode", harness != null);
            if (harness == null)
            {
                AppendSessionLine(ErrorsKey, "S3 player interaction harness component was not found in Play Mode.");
                return;
            }

            var playerMarker = FindSceneObjectIncludingInactive(S3PlayerInteractionHarness.ClericMarkerObjectName);
            RecordCheck("cleric_marker_found_in_play_mode", playerMarker != null);
            if (playerMarker == null)
            {
                AppendSessionLine(ErrorsKey, "ClericShellMarker was not found in Play Mode.");
                return;
            }

            var surface = FindNavMeshSurface();
            RecordCheck("navmesh_surface_found_in_play_mode", surface != null);
            RecordCheck("navmesh_surface_data_assigned_in_play_mode", surface?.navMeshData != null);
            if (surface == null || surface.navMeshData == null)
            {
                AppendSessionLine(ErrorsKey, "FirstDistrict_NavMeshSurface with baked NavMeshData was not found in Play Mode.");
                return;
            }

            surface.AddData();
            AppendDownstreamAdapterSnapshot();

            var spawn = SampleRequiredPosition(playerMarker, S3PlayerInteractionHarness.ClericMarkerObjectName);
            RecordCheck("spawn_sampled_on_navmesh", spawn.Sampled);
            if (!spawn.Sampled)
            {
                AppendSessionLine(ErrorsKey, "ClericShellMarker could not be sampled onto the baked NavMesh.");
                return;
            }

            var caretaker = FindSceneObjectIncludingInactive("M3_Caretaker");
            RecordCheck("caretaker_anchor_found_in_play_mode", caretaker != null);
            if (caretaker == null)
            {
                AppendSessionLine(ErrorsKey, "M3_Caretaker was not found in Play Mode.");
                return;
            }

            var target = SampleRequiredPosition(caretaker, "M3_Caretaker");
            RecordCheck("caretaker_sampled_on_navmesh", target.Sampled);
            if (!target.Sampled)
            {
                AppendSessionLine(ErrorsKey, "M3_Caretaker could not be sampled onto the baked NavMesh.");
                return;
            }

            var path = new NavMeshPath();
            var calculated = NavMesh.CalculatePath(spawn.Position, target.Position, NavMesh.AllAreas, path);
            var pathComplete = calculated && path.status == NavMeshPathStatus.PathComplete;
            RecordCheck("spawn_to_caretaker_path_complete", pathComplete);
            RecordCheck("spawn_to_caretaker_has_walked_distance", CalculatePathLength(path.corners) > 0.1f);
            if (!pathComplete)
            {
                AppendSessionLine(ErrorsKey, $"NavMesh path from spawn to M3_Caretaker returned status {path.status}.");
                return;
            }

            var probeObject = new GameObject("S3_05_AC12_PartialHarnessProbeTarget");
            probeObject.transform.position = target.Position;
            var probeTarget = new ProbeInteractTarget();

            harness.Configure(playerMarker.transform, S3PlayerInteractionHarness.DefaultInteractRangeMeters);
            harness.ClearRegisteredTargets();
            harness.ClearTelemetry();
            harness.RegisterTarget(probeTarget, probeObject.transform);

            MoveMarkerAlongPath(playerMarker.transform, path.corners);
            playerMarker.transform.position = ChooseProbeApproachPosition(path.corners, target.Position);

            var distance = Vector3.Distance(playerMarker.transform.position, probeObject.transform.position);
            AppendSessionLine(TelemetryKey, $"scripted_on_foot_path_status={path.status}");
            AppendSessionLine(TelemetryKey, $"scripted_on_foot_path_length_meters={CalculatePathLength(path.corners).ToString("0.###", CultureInfo.InvariantCulture)}");
            AppendSessionLine(TelemetryKey, $"scripted_on_foot_path_corners={path.corners.Length}");
            AppendSessionLine(TelemetryKey, $"partial_probe_distance_meters={distance.ToString("0.###", CultureInfo.InvariantCulture)}");

            var promptVisible = harness.RefreshPromptState();
            RecordCheck("harness_prompt_visible_at_partial_probe", promptVisible && harness.PromptVisible);
            var dispatched = harness.TryDispatchInteract();
            RecordCheck("s3_01_harness_dispatch_returns_true_inside_district", dispatched);
            RecordCheck("partial_probe_target_called_once", probeTarget.CallCount == 1);
            RecordCheck("partial_probe_receives_player_actor", probeTarget.LastPlayerActorId == S3PlayerInteractionHarness.DefaultPlayerActorId);
            RecordCheck("partial_probe_distance_within_harness_range", distance <= harness.ConfiguredInteractRangeMeters);
            RecordCheck("harness_records_interact_fired", HasLastEvent(harness, S3PlayerInteractionHarness.FiredTelemetryEvent));

            AppendSessionLine(TelemetryKey, $"asserted_partial_vocabulary={S3PlayerInteractionHarness.FiredTelemetryEvent}|s3_05_partial_probe_dispatched");
            AppendSessionLine(TelemetryKey, $"full_chain_vocabulary_deferred={FullChainVocabulary}");
            AppendSessionLine(TelemetryKey, $"harness_last_event={LastTelemetryEvent(harness)}");
            AppendSessionLine(TelemetryKey, $"harness_last_feedback={harness.LastFeedbackText}");
            AppendSessionLine(TelemetryKey, $"probe_target_event=s3_05_partial_probe_dispatched");
            AppendSessionLine(TelemetryKey, $"probe_target_payload_source=runner_only_partial_ac12_probe");

            harness.ClearRegisteredTargets();
            UnityEngine.Object.Destroy(probeObject);
        }

        private static void AppendDownstreamAdapterSnapshot()
        {
            var adapterCount = 0;
            foreach (var behaviour in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (behaviour is S3PlayerInteractionHarness)
                {
                    continue;
                }

                if (behaviour is IPlayerInteractTarget)
                {
                    adapterCount++;
                    AppendSessionLine(DownstreamKey, $"Observed interact target component: `{GetScenePath(behaviour.gameObject)}` component `{behaviour.GetType().Name}`.");
                }
            }

            AppendSessionLine(DownstreamKey, $"Observed downstream adapter count before runner probe: {adapterCount}.");

            foreach (var anchorName in DownstreamAnchorNames)
            {
                var anchor = FindSceneObjectIncludingInactive(anchorName);
                var hasInteractTarget = anchor != null && HasInteractTargetComponent(anchor);
                AppendSessionLine(DownstreamKey, $"`{anchorName}` has closed adapter component: {hasInteractTarget}.");
            }
        }

        private static bool HasInteractTargetComponent(GameObject gameObject)
        {
            foreach (var behaviour in gameObject.GetComponents<MonoBehaviour>())
            {
                if (behaviour is IPlayerInteractTarget)
                {
                    return true;
                }
            }

            return false;
        }

        private static void MoveMarkerAlongPath(Transform marker, Vector3[] corners)
        {
            foreach (var corner in corners)
            {
                marker.position = corner;
            }
        }

        private static Vector3 ChooseProbeApproachPosition(Vector3[] corners, Vector3 targetPosition)
        {
            var direction = Vector3.back;
            if (corners.Length >= 2)
            {
                direction = targetPosition - corners[^2];
                direction.y = 0.0f;
                if (direction.sqrMagnitude < 0.001f)
                {
                    direction = Vector3.back;
                }
            }

            return targetPosition - direction.normalized * ProbeApproachDistanceMeters;
        }

        private static SampledPosition SampleRequiredPosition(GameObject gameObject, string label)
        {
            if (!NavMesh.SamplePosition(gameObject.transform.position, out var hit, SampleMaxDistanceMeters, NavMesh.AllAreas))
            {
                AppendSessionLine(
                    WarningsKey,
                    $"{label} could not be sampled onto the NavMesh within {SampleMaxDistanceMeters.ToString("0.0", CultureInfo.InvariantCulture)}m.");
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

        private static NavMeshSurface? FindNavMeshSurface()
        {
            var surfaceObject = FindSceneObjectIncludingInactive("FirstDistrict_NavMeshSurface");
            return surfaceObject == null ? null : surfaceObject.GetComponent<NavMeshSurface>();
        }

        private static bool HasLastEvent(S3PlayerInteractionHarness harness, string telemetryEvent)
        {
            return harness.TelemetryEvents.Count > 0 && harness.TelemetryEvents[^1].TelemetryEvent == telemetryEvent;
        }

        private static string LastTelemetryEvent(S3PlayerInteractionHarness harness)
        {
            return harness.TelemetryEvents.Count == 0 ? "none" : harness.TelemetryEvents[^1].TelemetryEvent;
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

        private static void CaptureLog(string condition, string stackTrace, LogType type)
        {
            if (GravenspireScenarioSmokeRunnerHelpers.IsEditorStartupNoise(condition, stackTrace, type))
            {
                return;
            }

            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                AppendSessionLine(ErrorsKey, condition);
            }
            else if (type == LogType.Warning)
            {
                AppendSessionLine(WarningsKey, condition);
            }
        }

        private static void RecordCheck(string name, bool passed)
        {
            AppendSessionLine(ChecksKey, $"{name}={(passed ? "PASS" : "FAIL")}");
        }

        private static bool AllChecksPassed()
        {
            foreach (var check in GetSessionLines(ChecksKey))
            {
                if (check.EndsWith("=FAIL", StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return GetSessionLines(ChecksKey).Count > 0;
        }

        private static void WriteEvidenceAndExit(int exitCode)
        {
            EditorApplication.update -= ContinueAfterDomainReload;
            Application.logMessageReceived -= CaptureLog;
            var evidencePath = CurrentEvidencePath();

            var builder = new StringBuilder();
            builder.AppendLine("# S3-05 Unity End-To-End In-District Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine($"**Story:** `production/stories/{StorySlug}.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireS3DistrictCompositeObjectiveLoopVerificationRunner.cs`");
            builder.AppendLine($"**Result:** {(exitCode == 0 ? "PASS_WITH_NOTES" : "FAIL")}");
            builder.AppendLine("**AC-12 status:** Partial pass by story-defined graceful-degradation semantics.");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Asserted: S3-01 harness dispatch works inside the S3-05 district after a NavMesh-complete walked path from spawn toward `M3_Caretaker`.");
            builder.AppendLine("- Deferred: S3-02/03/04 player-driven adapters and full objective-loop telemetry, because those stories were not closed at S3-05 Phase 6 implementation time.");
            builder.AppendLine("- Rollforward: full AC-12 assertion belongs to S3-06; the `s3_05_ac12_partial_rollforward_to_s3_06` carryover is deferred to Phase 8 closure artifacts.");
            builder.AppendLine();
            builder.AppendLine("## Checks");
            builder.AppendLine();

            foreach (var check in GetSessionLines(ChecksKey))
            {
                var parts = check.Split('=');
                var name = parts[0];
                var passed = parts.Length > 1 && parts[1] == "PASS";
                builder.AppendLine($"- {(passed ? "PASS" : "FAIL")} `{name}`");
            }

            builder.AppendLine();
            builder.AppendLine("## Partial Dispatch Telemetry");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(TelemetryKey));
            builder.AppendLine();
            builder.AppendLine("## Downstream Adapter Envelope");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(DownstreamKey));
            builder.AppendLine();
            builder.AppendLine("## Warnings");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(WarningsKey));
            builder.AppendLine();
            builder.AppendLine("## Errors");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(ErrorsKey));

            File.WriteAllText(evidencePath, builder.ToString());
            ClearSession();
            EditorApplication.Exit(exitCode);
        }

        private static string CurrentEvidencePath()
        {
            var configuredPath = SessionState.GetString(EvidencePathKey, string.Empty);
            return string.IsNullOrWhiteSpace(configuredPath) ? DefaultEvidencePath() : configuredPath;
        }

        private static string DefaultEvidencePath()
        {
            return Path.Combine(
                "tests",
                "evidence",
                StoryId,
                $"unity-end-to-end-in-district-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-smoke.md");
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

        private static void AppendSessionLine(string key, string value)
        {
            var current = SessionState.GetString(key, string.Empty);
            SessionState.SetString(key, string.IsNullOrEmpty(current) ? value : current + "\n" + value);
        }

        private static List<string> GetSessionLines(string key)
        {
            var value = SessionState.GetString(key, string.Empty);
            return string.IsNullOrWhiteSpace(value)
                ? new List<string>()
                : new List<string>(value.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static void ClearSession()
        {
            SessionState.EraseBool(RunKey);
            SessionState.EraseString(PhaseKey);
            SessionState.EraseString(ChecksKey);
            SessionState.EraseString(ErrorsKey);
            SessionState.EraseString(WarningsKey);
            SessionState.EraseString(TelemetryKey);
            SessionState.EraseString(DownstreamKey);
            SessionState.EraseString(EvidencePathKey);
            SessionState.EraseString(PlayStartedKey);
        }

        private readonly struct SampledPosition
        {
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

        private sealed class ProbeInteractTarget : IPlayerInteractTarget
        {
            public int CallCount { get; private set; }

            public string LastPlayerActorId { get; private set; } = string.Empty;

            public bool TryInteract(string playerActorId, float distanceMeters, out InteractContext context)
            {
                CallCount++;
                LastPlayerActorId = playerActorId;
                context = new InteractContext(
                    "s3_05_partial_probe_dispatched",
                    playerActorId,
                    "S3_05_AC12_PartialHarnessProbeTarget",
                    "interacted",
                    "interacted",
                    distanceMeters,
                    "partial_ac12_probe",
                    "s3_01_harness_dispatch",
                    "s3_06_rollforward");
                return true;
            }
        }
    }
}
#endif
