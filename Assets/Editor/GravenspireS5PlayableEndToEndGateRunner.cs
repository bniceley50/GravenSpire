#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Gravenspire.Gameplay.Npc.M3Objective;
using Gravenspire.UnityRuntime.Interaction;
using Gravenspire.UnityRuntime.Npc;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    /// <summary>
    /// S5-05 gate runner: batchmode end-to-end objective-loop verification on the produced
    /// Sexton's Court, covering the mechanical hollow-evidence fences. [F1] artifact identity:
    /// the played scene/NavMesh hashes must match the tuple recorded in
    /// production/qa/evidence/s5-03-produced-art-evidence.md section 2. [F2] walked traversal:
    /// the player marker moves time-stepped along the NavMesh path at walk speed and is never
    /// corner-teleported. [F7] exact main-path telemetry sequence: accept, recover, loot, sell,
    /// hand-in events asserted as the exact ordered sequence. [F3] (real Input.GetKeyDown input
    /// path) and the S5-05-04 feel verdict are explicitly NOT claimed by this runner; both
    /// belong to the product-owner human-play session. Derived from the S3-06 scaffold
    /// GravenspireS3PlayableEndToEndRunner (Codex stash, untracked-files commit e218dc9);
    /// the S3-06 chained-M2 and exception-injection machinery is out of S5-05 scope and removed.
    /// </summary>
    [InitializeOnLoad]
    public static class GravenspireS5PlayableEndToEndGateRunner
    {
        private const string StoryId = "S5-05";
        private const string StorySlug = "s5-05-revalidate-feel-human-play-gate";
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string NavMeshAssetPath = "Assets/Scenes/_DevEntry/FirstDistrict_Greybox_NavMesh.asset";
        private const string TupleSourcePath = "production/qa/evidence/s5-03-produced-art-evidence.md";

        // [F1] expected tuple values, recorded by S5-03 (see TupleSourcePath section 2).
        private const string ExpectedSceneSha = "bae11334e62cc72b39ffdd20d6cb10836891dda9";
        private const string ExpectedNavMeshSha = "5c20605e530996245a7061c01e82243063ee8dda";
        private const long ExpectedNavMeshSizeBytes = 9704L;

        private const string RunKey = "GravenspireS5PlayableEndToEndGate.Run";
        private const string PhaseKey = "GravenspireS5PlayableEndToEndGate.Phase";
        private const string ChecksKey = "GravenspireS5PlayableEndToEndGate.Checks";
        private const string ErrorsKey = "GravenspireS5PlayableEndToEndGate.Errors";
        private const string WarningsKey = "GravenspireS5PlayableEndToEndGate.Warnings";
        private const string TelemetryKey = "GravenspireS5PlayableEndToEndGate.Telemetry";
        private const string WalkTelemetryKey = "GravenspireS5PlayableEndToEndGate.WalkTelemetry";
        private const string F1TelemetryKey = "GravenspireS5PlayableEndToEndGate.F1Telemetry";
        private const string F3TelemetryKey = "GravenspireS5PlayableEndToEndGate.F3Telemetry";
        private const string TargetEventSequenceKey = "GravenspireS5PlayableEndToEndGate.TargetEventSequence";
        private const string EvidencePathKey = "GravenspireS5PlayableEndToEndGate.EvidencePath";
        private const string PlayStartedKey = "GravenspireS5PlayableEndToEndGate.PlayStartedSeconds";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";

        private const float SampleMaxDistanceMeters = 2.0f;
        private const float ApproachDistanceMeters = 1.0f;
        private const double SmokeDelaySeconds = 1.0d;

        // [F2] walk pacing: the marker advances at most WalkSpeed * tick-delta per editor
        // update tick, so traversal consumes real wall time proportional to path length.
        private const float WalkSpeedMetersPerSecond = 3.0f;
        private const double MaxTickDeltaSeconds = 0.1d;
        private const int MinimumWalkTicksPerLeg = 5;
        private const double WatchdogSeconds = 300.0d;

        private static readonly string[] ExpectedTargetEventOrder =
        {
            M3NamedNpcInteractTarget.TelemetryEvent,
            M3NamedNpcInteractTarget.ObjectiveAcceptedTelemetryEvent,
            M3RelicInteractTarget.RelicRecoveredTelemetryEvent,
            M3RelicInteractTarget.ObjectiveLootResolvedTelemetryEvent,
            M3VendorInteractTarget.SalvageSoldTelemetryEvent,
            M3VendorInteractTarget.SellCopperAppliedTelemetryEvent,
            M3NamedNpcInteractTarget.RelicHandedInTelemetryEvent
        };

        private static readonly string[] LegLabels =
        {
            "spawn_to_caretaker_accept",
            "caretaker_to_relic",
            "relic_to_vendor",
            "vendor_to_caretaker_hand_in"
        };

        private static GateState? _gate;

        static GravenspireS5PlayableEndToEndGateRunner()
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

        [MenuItem("Gravenspire/Verify S5 Playable End-To-End Gate")]
        public static void Run()
        {
            ClearSession();
            _gate = null;
            SessionState.SetBool(RunKey, true);
            SessionState.SetString(PhaseKey, "initial");
            Application.logMessageReceived -= CaptureLog;
            Application.logMessageReceived += CaptureLog;

            try
            {
                var evidencePath = ResolveEvidencePathFromCommandLine(DefaultEvidencePath());
                SessionState.SetString(EvidencePathKey, evidencePath);
                Directory.CreateDirectory(Path.GetDirectoryName(evidencePath) ?? ".");

                RunF1ArtifactIdentityChecks();

                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                RecordCheck("scene_loaded", scene.IsValid() && scene.path == ScenePath);
                RecordCheck("first_district_greybox_root_exists", FindSceneObjectIncludingInactive("FirstDistrict_Greybox") != null);
                RecordCheck("harness_root_exists", FindSceneObjectIncludingInactive(S3PlayerInteractionHarness.HarnessRootName) != null);
                RecordCheck("cleric_shell_marker_exists", FindSceneObjectIncludingInactive(S3PlayerInteractionHarness.ClericMarkerObjectName) != null);
                RecordCheck("m3_caretaker_anchor_exists", FindSceneObjectIncludingInactive(M3NamedNpcObjectiveFrame.AnchorObjectName) != null);
                RecordCheck("m3_objective_relic_anchor_exists", FindSceneObjectIncludingInactive(M3ObjectiveStateRelicHandInSession.RelicObjectName) != null);
                RecordCheck("m3_court_vendor_anchor_exists", FindSceneObjectIncludingInactive(M3LootTableFixedProfileVendorData.VendorObjectName) != null);
                RecordCheck("navmesh_surface_ready", FindNavMeshSurface() is { navMeshData: not null });

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
                if (EditorApplication.timeSinceStartup - startedSeconds > WatchdogSeconds)
                {
                    RecordCheck("gate_watchdog_within_limit", false);
                    AppendSessionLine(ErrorsKey, $"Gate watchdog exceeded {WatchdogSeconds.ToString("0", CultureInfo.InvariantCulture)} seconds in Play Mode.");
                    WriteEvidenceAndExit(1);
                    return;
                }

                if (_gate == null)
                {
                    InitializeGate();
                    return;
                }

                StepGate(_gate);
            }
            catch (Exception ex)
            {
                AppendSessionLine(ErrorsKey, ex.ToString());
                WriteEvidenceAndExit(1);
            }
        }

        private static void RunF1ArtifactIdentityChecks()
        {
            AppendSessionLine(F1TelemetryKey, $"f1.tuple_source={TupleSourcePath} (section 2, recorded by S5-03)");

            var sceneHashed = TryGitHashObject(ScenePath, out var sceneSha, out var sceneDetail);
            RecordCheck("f1_scene_hash_computed", sceneHashed);
            RecordCheck("f1_scene_sha_matches_s5_03_tuple", sceneHashed && string.Equals(sceneSha, ExpectedSceneSha, StringComparison.OrdinalIgnoreCase));
            AppendSessionLine(F1TelemetryKey, $"f1.scene_sha_measured={(sceneHashed ? sceneSha : "unavailable: " + sceneDetail)}");
            AppendSessionLine(F1TelemetryKey, $"f1.scene_sha_expected={ExpectedSceneSha}");

            var navMeshHashed = TryGitHashObject(NavMeshAssetPath, out var navMeshSha, out var navMeshDetail);
            RecordCheck("f1_navmesh_hash_computed", navMeshHashed);
            RecordCheck("f1_navmesh_sha_matches_s5_03_tuple", navMeshHashed && string.Equals(navMeshSha, ExpectedNavMeshSha, StringComparison.OrdinalIgnoreCase));
            AppendSessionLine(F1TelemetryKey, $"f1.navmesh_sha_measured={(navMeshHashed ? navMeshSha : "unavailable: " + navMeshDetail)}");
            AppendSessionLine(F1TelemetryKey, $"f1.navmesh_sha_expected={ExpectedNavMeshSha}");

            var navMeshSize = File.Exists(NavMeshAssetPath) ? new FileInfo(NavMeshAssetPath).Length : -1L;
            RecordCheck("f1_navmesh_size_matches_s5_03_tuple", navMeshSize == ExpectedNavMeshSizeBytes);
            AppendSessionLine(F1TelemetryKey, $"f1.navmesh_size_measured_bytes={navMeshSize.ToString(CultureInfo.InvariantCulture)}");
            AppendSessionLine(F1TelemetryKey, $"f1.navmesh_size_expected_bytes={ExpectedNavMeshSizeBytes.ToString(CultureInfo.InvariantCulture)}");
        }

        private static bool TryGitHashObject(string relativePath, out string sha, out string detail)
        {
            sha = string.Empty;
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"hash-object -- \"{relativePath}\"",
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(startInfo);
                if (process == null)
                {
                    detail = "git process failed to start";
                    return false;
                }

                var stdout = process.StandardOutput.ReadToEnd();
                var stderr = process.StandardError.ReadToEnd();
                if (!process.WaitForExit(30000))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                        // Best-effort kill on timeout; the timeout itself is the failure.
                    }

                    detail = "git hash-object timed out after 30s";
                    return false;
                }

                if (process.ExitCode != 0)
                {
                    detail = $"git exited {process.ExitCode.ToString(CultureInfo.InvariantCulture)}: {stderr.Trim()}";
                    return false;
                }

                sha = stdout.Trim();
                detail = "ok";
                return sha.Length == 40;
            }
            catch (Exception ex)
            {
                detail = ex.Message;
                return false;
            }
        }

        private static void InitializeGate()
        {
            var sceneObjects = RequiredSceneObjects();
            if (!sceneObjects.Valid)
            {
                AppendSessionLine(ErrorsKey, "Required S5-05 scene component was missing in Play Mode.");
                WriteEvidenceAndExit(1);
                return;
            }

            ConfigureFreshSession(sceneObjects);
            RecordGuardedCheck("s5_05_scene_references_resolve", () => SceneReferencesResolve(sceneObjects));
            RecordCheck("fresh_state_not_introduced", sceneObjects.Objective.State == M3ObjectiveState.NotIntroduced);
            RecordCheck("fresh_vendor_has_no_currency", sceneObjects.Vendor.CarriedCurrencyCopper == 0);
            RecordCheck("fresh_relic_inactive", !sceneObjects.RelicObject.activeSelf);

            AppendSessionLine(TelemetryKey, "legacy_m2_suppression_note=batchmode_keeps_legacy_m2_active_per_s5_04_scenario_discrimination");
            AppendSessionLine(F3TelemetryKey, "f3.input_path_claim=NOT_PROVEN_BY_THIS_RUNNER");
            AppendSessionLine(F3TelemetryKey, "f3.runner_dispatch_mechanism=S3PlayerInteractionHarness.TryDispatchInteract (direct method call)");
            AppendSessionLine(F3TelemetryKey, $"f3.harness_interact_key={sceneObjects.Harness.InteractKey}");
            AppendSessionLine(F3TelemetryKey, "f3.required_proof=product-owner human-play session firing the same telemetry sequence via Input.GetKeyDown");

            _gate = new GateState(sceneObjects);
        }

        private static void StepGate(GateState gate)
        {
            if (!gate.LegInitialized)
            {
                InitializeLeg(gate);
                return;
            }

            var now = EditorApplication.timeSinceStartup;
            var delta = Math.Clamp(now - gate.LastTickSeconds, 0.0d, MaxTickDeltaSeconds);
            gate.LastTickSeconds = now;
            gate.TickCount++;

            var remaining = WalkSpeedMetersPerSecond * (float)delta;
            var marker = gate.Scene.PlayerMarker.transform;
            while (remaining > 0f && gate.CornerIndex < gate.Corners.Length)
            {
                var corner = gate.Corners[gate.CornerIndex];
                var distance = Vector3.Distance(marker.position, corner);
                if (distance <= remaining)
                {
                    marker.position = corner;
                    gate.WalkedMeters += distance;
                    remaining -= distance;
                    gate.CornerIndex++;
                }
                else
                {
                    marker.position = Vector3.MoveTowards(marker.position, corner, remaining);
                    gate.WalkedMeters += remaining;
                    remaining = 0f;
                }
            }

            if (gate.CornerIndex >= gate.Corners.Length)
            {
                ArriveAtLegTarget(gate);
            }
        }

        private static void InitializeLeg(GateState gate)
        {
            var label = LegLabels[gate.LegIndex];
            var target = ResolveLegTarget(gate);
            if (target == null)
            {
                AppendSessionLine(ErrorsKey, $"{label} target object could not be resolved.");
                WriteEvidenceAndExit(1);
                return;
            }

            var marker = gate.Scene.PlayerMarker.transform;
            var start = SamplePosition(marker.position, label + "_start");
            var end = SamplePosition(target.position, label + "_target");
            if (!start.Sampled || !end.Sampled)
            {
                AppendSessionLine(ErrorsKey, $"{label} start or target could not be sampled onto the NavMesh.");
                WriteEvidenceAndExit(1);
                return;
            }

            var path = new NavMeshPath();
            var calculated = NavMesh.CalculatePath(start.Position, end.Position, NavMesh.AllAreas, path);
            var complete = calculated && path.status == NavMeshPathStatus.PathComplete;
            RecordCheck(label + "_navmesh_path_complete", complete);
            AppendSessionLine(WalkTelemetryKey, $"{label}.path_status={path.status}");
            AppendSessionLine(WalkTelemetryKey, $"{label}.path_length_meters={CalculatePathLength(path.corners).ToString("0.###", CultureInfo.InvariantCulture)}");
            AppendSessionLine(WalkTelemetryKey, $"{label}.path_corners={path.corners.Length.ToString(CultureInfo.InvariantCulture)}");
            if (!complete)
            {
                AppendSessionLine(ErrorsKey, $"NavMesh path for {label} returned status {path.status}.");
                WriteEvidenceAndExit(1);
                return;
            }

            var corners = (Vector3[])path.corners.Clone();
            if (corners.Length >= 2)
            {
                corners[^1] = ChooseApproachPosition(path.corners, end.Position);
            }

            marker.position = corners[0];
            gate.Corners = corners;
            gate.CornerIndex = 1;
            gate.ExpectedWalkMeters = CalculatePathLength(corners);
            gate.WalkedMeters = 0f;
            gate.TickCount = 0;
            gate.LegStartedSeconds = EditorApplication.timeSinceStartup;
            gate.LastTickSeconds = gate.LegStartedSeconds;
            gate.LegInitialized = true;
        }

        private static Transform? ResolveLegTarget(GateState gate)
        {
            return gate.LegIndex switch
            {
                0 => gate.Scene.Caretaker.transform,
                1 => gate.Scene.RelicObject.transform,
                2 => gate.Scene.VendorObject.transform,
                3 => gate.Scene.Caretaker.transform,
                _ => null
            };
        }

        private static void ArriveAtLegTarget(GateState gate)
        {
            var label = LegLabels[gate.LegIndex];
            var now = EditorApplication.timeSinceStartup;
            var elapsedSeconds = now - gate.LegStartedSeconds;
            var target = ResolveLegTarget(gate);
            var finalDistance = target == null
                ? float.NaN
                : Vector3.Distance(gate.Scene.PlayerMarker.transform.position, target.position);

            AppendSessionLine(WalkTelemetryKey, $"{label}.walked_meters={gate.WalkedMeters.ToString("0.###", CultureInfo.InvariantCulture)}");
            AppendSessionLine(WalkTelemetryKey, $"{label}.expected_walk_meters={gate.ExpectedWalkMeters.ToString("0.###", CultureInfo.InvariantCulture)}");
            AppendSessionLine(WalkTelemetryKey, $"{label}.walk_ticks={gate.TickCount.ToString(CultureInfo.InvariantCulture)}");
            AppendSessionLine(WalkTelemetryKey, $"{label}.walk_elapsed_seconds={elapsedSeconds.ToString("0.###", CultureInfo.InvariantCulture)}");
            AppendSessionLine(WalkTelemetryKey, $"{label}.final_distance_meters={finalDistance.ToString("0.###", CultureInfo.InvariantCulture)}");

            var distanceTolerance = Math.Max(0.5f, gate.ExpectedWalkMeters * 0.05f);
            RecordCheck(label + "_walked_distance_matches_path", Math.Abs(gate.WalkedMeters - gate.ExpectedWalkMeters) <= distanceTolerance);

            var minimumElapsedSeconds = gate.ExpectedWalkMeters / WalkSpeedMetersPerSecond * 0.8d;
            RecordCheck(
                label + "_walk_time_stepped_not_teleported",
                gate.TickCount >= MinimumWalkTicksPerLeg && elapsedSeconds >= minimumElapsedSeconds);

            gate.Scene.Harness.RefreshRegisteredTargetsFromScene();
            var promptVisible = gate.Scene.Harness.RefreshPromptState();
            RecordCheck(label + "_prompt_visible_at_target", promptVisible && gate.Scene.Harness.PromptVisible);
            AppendSessionLine(WalkTelemetryKey, $"{label}.prompt_text={gate.Scene.Harness.CurrentPromptText}");

            var legPassed = RecordGuardedCheck("s5_05_leg_" + label + "_completed", () => RunLegAssertions(gate));
            if (!legPassed)
            {
                AppendSessionLine(ErrorsKey, $"{label} leg assertions failed; aborting remaining legs.");
                WriteEvidenceAndExit(1);
                return;
            }

            if (gate.LegIndex >= LegLabels.Length - 1)
            {
                FinishGate(gate);
                return;
            }

            gate.LegIndex++;
            gate.LegInitialized = false;
        }

        private static bool RunLegAssertions(GateState gate)
        {
            switch (gate.LegIndex)
            {
                case 0:
                {
                    var dispatched = Dispatch(gate.Scene, "accept");
                    RecordCheck("accept_dispatch_returns_true", dispatched);
                    RecordCheck("accept_state_accepted", gate.Scene.Objective.State == M3ObjectiveState.Accepted);
                    RecordCheck("accept_event_order", TargetEventsMatchOrder(
                        M3NamedNpcInteractTarget.TelemetryEvent,
                        M3NamedNpcInteractTarget.ObjectiveAcceptedTelemetryEvent));
                    RecordCheck("relic_active_after_accept", gate.Scene.RelicObject.activeSelf);
                    return dispatched &&
                        gate.Scene.Objective.State == M3ObjectiveState.Accepted &&
                        gate.Scene.RelicObject.activeSelf;
                }

                case 1:
                {
                    var dispatched = Dispatch(gate.Scene, "recover_relic");
                    RecordCheck("relic_dispatch_returns_true", dispatched);
                    RecordCheck("relic_state_recovered", gate.Scene.Objective.State == M3ObjectiveState.RelicRecovered);
                    RecordCheck("relic_event_order", TargetEventsMatchOrder(
                        M3NamedNpcInteractTarget.TelemetryEvent,
                        M3NamedNpcInteractTarget.ObjectiveAcceptedTelemetryEvent,
                        M3RelicInteractTarget.RelicRecoveredTelemetryEvent,
                        M3RelicInteractTarget.ObjectiveLootResolvedTelemetryEvent));
                    RecordCheck("vendor_carries_relic_after_recovery", gate.Scene.Vendor.CarriesCourtMarkedRelic);
                    RecordCheck("vendor_carries_salvage_after_recovery", gate.Scene.Vendor.CarriesSalvage);
                    return dispatched &&
                        gate.Scene.Objective.State == M3ObjectiveState.RelicRecovered &&
                        gate.Scene.Vendor.CarriesCourtMarkedRelic &&
                        gate.Scene.Vendor.CarriesSalvage;
                }

                case 2:
                {
                    var dispatched = Dispatch(gate.Scene, "sell_salvage");
                    RecordCheck("vendor_dispatch_returns_true", dispatched);
                    RecordCheck("vendor_currency_positive", gate.Scene.Vendor.CarriedCurrencyCopper > 0);
                    RecordCheck("vendor_salvage_removed_after_sale", !gate.Scene.Vendor.CarriesSalvage);
                    RecordCheck("vendor_event_order", TargetEventsMatchOrder(
                        M3RelicInteractTarget.RelicRecoveredTelemetryEvent,
                        M3RelicInteractTarget.ObjectiveLootResolvedTelemetryEvent,
                        M3VendorInteractTarget.SalvageSoldTelemetryEvent,
                        M3VendorInteractTarget.SellCopperAppliedTelemetryEvent));
                    return dispatched &&
                        gate.Scene.Vendor.CarriedCurrencyCopper > 0 &&
                        !gate.Scene.Vendor.CarriesSalvage;
                }

                case 3:
                {
                    var dispatched = Dispatch(gate.Scene, "hand_in");
                    RecordCheck("hand_in_dispatch_returns_true", dispatched);
                    RecordCheck("hand_in_state_complete", gate.Scene.Objective.State == M3ObjectiveState.Complete);
                    RecordCheck("hand_in_event_present", CountTargetEvent(M3NamedNpcInteractTarget.RelicHandedInTelemetryEvent) == 1);
                    RecordCheck("hand_in_no_extra_npc_interaction_event", CountTargetEvent(M3NamedNpcInteractTarget.TelemetryEvent) == 1);
                    return dispatched && gate.Scene.Objective.State == M3ObjectiveState.Complete;
                }

                default:
                    return false;
            }
        }

        private static void FinishGate(GateState gate)
        {
            RecordCheck("full_target_event_order_exact", TargetEventsEqual(ExpectedTargetEventOrder));
            RecordCheck("final_objective_complete", gate.Scene.Objective.IsComplete);
            RecordCheck("final_vendor_currency_positive", gate.Scene.Vendor.CarriedCurrencyCopper > 0);
            RecordCheck("final_vendor_still_carries_relic", gate.Scene.Vendor.CarriesCourtMarkedRelic);

            AppendSessionLine(TelemetryKey, $"final_objective_state={gate.Scene.Objective.State}");
            AppendSessionLine(TelemetryKey, $"final_objective_state_sequence={gate.Scene.Objective.StateSequence}");
            AppendSessionLine(TelemetryKey, $"final_vendor_currency_copper={gate.Scene.Vendor.CarriedCurrencyCopper.ToString(CultureInfo.InvariantCulture)}");
            AppendSessionLine(TelemetryKey, $"final_vendor_carries_relic={gate.Scene.Vendor.CarriesCourtMarkedRelic}");
            AppendSessionLine(TelemetryKey, $"target_event_sequence={FormatTargetEventSequence()}");

            WriteEvidenceAndExit(AllChecksPassed() && GetSessionLines(ErrorsKey).Count == 0 ? 0 : 1);
        }

        private static bool SceneReferencesResolve(SceneObjects sceneObjects)
        {
            var surface = FindNavMeshSurface();
            if (surface != null)
            {
                surface.AddData();
            }

            return sceneObjects.Harness.PlayerMarker == sceneObjects.PlayerMarker.transform &&
                sceneObjects.NpcAdapter.ObjectiveFrame == sceneObjects.Frame &&
                sceneObjects.NpcAdapter.ObjectiveState == sceneObjects.Objective &&
                sceneObjects.RelicAdapter.ObjectiveState == sceneObjects.Objective &&
                sceneObjects.RelicAdapter.LootVendor == sceneObjects.Vendor &&
                sceneObjects.VendorAdapter.Vendor == sceneObjects.Vendor &&
                surface is { navMeshData: not null };
        }

        private static void ConfigureFreshSession(SceneObjects sceneObjects)
        {
            sceneObjects.Frame.ClearSessionInteractions();
            sceneObjects.Objective.ResetSessionObjective();
            sceneObjects.Vendor.ResetSessionVendor();
            sceneObjects.NpcAdapter.Configure(sceneObjects.Frame, sceneObjects.Objective);
            sceneObjects.RelicAdapter.Configure(sceneObjects.Objective, sceneObjects.Vendor);
            sceneObjects.VendorAdapter.Configure(sceneObjects.Vendor);
            sceneObjects.Harness.Configure(sceneObjects.PlayerMarker.transform, S3PlayerInteractionHarness.DefaultInteractRangeMeters);
            sceneObjects.Harness.ClearRegisteredTargets();
            sceneObjects.Harness.ClearTelemetry();
            sceneObjects.Harness.RefreshRegisteredTargetsFromScene();
            SessionState.EraseString(TargetEventSequenceKey);
        }

        private static bool Dispatch(SceneObjects sceneObjects, string label)
        {
            sceneObjects.Harness.RefreshRegisteredTargetsFromScene();
            sceneObjects.Harness.ClearTelemetry();
            var result = sceneObjects.Harness.TryDispatchInteract();
            AppendHarnessTelemetry(label, sceneObjects.Harness);
            return result;
        }

        private static SampledPosition SamplePosition(Vector3 position, string label)
        {
            if (!NavMesh.SamplePosition(position, out var hit, SampleMaxDistanceMeters, NavMesh.AllAreas))
            {
                AppendSessionLine(WarningsKey, $"{label} could not be sampled onto NavMesh.");
                return new SampledPosition(false, position);
            }

            return new SampledPosition(true, hit.position);
        }

        private static Vector3 ChooseApproachPosition(Vector3[] corners, Vector3 targetPosition)
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

            return targetPosition - direction.normalized * ApproachDistanceMeters;
        }

        private static void AppendHarnessTelemetry(string label, S3PlayerInteractionHarness harness)
        {
            AppendSessionLine(TelemetryKey, $"{label}.last_outcome={harness.LastOutcome}");
            AppendSessionLine(TelemetryKey, $"{label}.last_feedback={harness.LastFeedbackText}");
            AppendSessionLine(TelemetryKey, $"{label}.event_sequence={FormatEventSequence(harness.TelemetryEvents)}");

            foreach (var context in harness.TelemetryEvents)
            {
                AppendSessionLine(
                    TelemetryKey,
                    $"{label}.{context.TelemetryEvent}={context.TargetId}|{context.PlayerActorId}|{context.PayloadKind}|{context.PrimaryPayload}|{context.SecondaryPayload}|amount:{context.Amount}|{context.DistanceMeters.ToString("0.###", CultureInfo.InvariantCulture)}");

                if (context.TelemetryEvent != S3PlayerInteractionHarness.FiredTelemetryEvent &&
                    context.TelemetryEvent != S3PlayerInteractionHarness.MissedTelemetryEvent &&
                    context.TelemetryEvent != S3PlayerInteractionHarness.BlockedTelemetryEvent)
                {
                    AppendSessionLine(TargetEventSequenceKey, context.TelemetryEvent);
                }
            }
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

        private static bool TargetEventsMatchOrder(params string[] expectedEvents)
        {
            var events = GetSessionLines(TargetEventSequenceKey);
            var cursor = -1;
            foreach (var expected in expectedEvents)
            {
                var index = IndexOfAfter(events, expected, cursor);
                if (index < 0)
                {
                    return false;
                }

                cursor = index;
            }

            return true;
        }

        private static bool TargetEventsEqual(IReadOnlyList<string> expectedEvents)
        {
            var events = GetSessionLines(TargetEventSequenceKey);
            if (events.Count != expectedEvents.Count)
            {
                return false;
            }

            for (var i = 0; i < expectedEvents.Count; i++)
            {
                if (!string.Equals(events[i], expectedEvents[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private static int CountTargetEvent(string telemetryEvent)
        {
            var count = 0;
            foreach (var candidate in GetSessionLines(TargetEventSequenceKey))
            {
                if (string.Equals(candidate, telemetryEvent, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        private static int IndexOfAfter(IReadOnlyList<string> values, string expected, int afterIndex)
        {
            for (var i = afterIndex + 1; i < values.Count; i++)
            {
                if (string.Equals(values[i], expected, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        private static string FormatTargetEventSequence()
        {
            var events = GetSessionLines(TargetEventSequenceKey);
            return events.Count == 0 ? "none" : string.Join(">", events);
        }

        private static string FormatEventSequence(IReadOnlyList<InteractContext> contexts)
        {
            var events = new List<string>();
            foreach (var context in contexts)
            {
                events.Add(context.TelemetryEvent);
            }

            return events.Count == 0 ? "none" : string.Join(">", events);
        }

        private static bool RecordGuardedCheck(string name, Func<bool> check)
        {
            try
            {
                var passed = check();
                RecordCheck(name, passed);
                return passed;
            }
            catch (Exception ex)
            {
                RecordCheck(name, false);
                AppendSessionLine(ErrorsKey, $"{name} threw {ex.GetType().Name}: {ex.Message}");
                return false;
            }
        }

        private static SceneObjects RequiredSceneObjects()
        {
            var harness = UnityEngine.Object.FindFirstObjectByType<S3PlayerInteractionHarness>();
            var playerMarker = FindSceneObjectIncludingInactive(S3PlayerInteractionHarness.ClericMarkerObjectName);
            var caretaker = FindSceneObjectIncludingInactive(M3NamedNpcObjectiveFrame.AnchorObjectName);
            var objectiveRoot = FindSceneObjectIncludingInactive("M3_ObjectiveStateRoot");
            var relicObject = FindSceneObjectIncludingInactive(M3ObjectiveStateRelicHandInSession.RelicObjectName);
            var vendorObject = FindSceneObjectIncludingInactive(M3LootTableFixedProfileVendorData.VendorObjectName);
            var frame = caretaker == null ? null : caretaker.GetComponent<M3NamedNpcObjectiveFrame>();
            var npcAdapter = caretaker == null ? null : caretaker.GetComponent<M3NamedNpcInteractTarget>();
            var objective = objectiveRoot == null ? null : objectiveRoot.GetComponent<M3ObjectiveStateRelicHandIn>();
            var relicAdapter = relicObject == null ? null : relicObject.GetComponent<M3RelicInteractTarget>();
            var vendor = vendorObject == null ? null : vendorObject.GetComponent<M3LootTableFixedProfileVendor>();
            var vendorAdapter = vendorObject == null ? null : vendorObject.GetComponent<M3VendorInteractTarget>();

            RecordCheck("harness_component_found_in_play_mode", harness != null);
            RecordCheck("cleric_marker_found_in_play_mode", playerMarker != null);
            RecordCheck("caretaker_found_in_play_mode", caretaker != null);
            RecordCheck("frame_found_in_play_mode", frame != null);
            RecordCheck("npc_adapter_found_in_play_mode", npcAdapter != null);
            RecordCheck("objective_component_found_in_play_mode", objective != null);
            RecordCheck("relic_object_found_in_play_mode", relicObject != null);
            RecordCheck("relic_adapter_found_in_play_mode", relicAdapter != null);
            RecordCheck("vendor_component_found_in_play_mode", vendor != null);
            RecordCheck("vendor_adapter_found_in_play_mode", vendorAdapter != null);

            return new SceneObjects(
                harness,
                playerMarker,
                caretaker,
                frame,
                npcAdapter,
                objective,
                relicObject,
                relicAdapter,
                vendor,
                vendorObject,
                vendorAdapter);
        }

        private static NavMeshSurface? FindNavMeshSurface()
        {
            var surfaceObject = FindSceneObjectIncludingInactive("FirstDistrict_NavMeshSurface");
            return surfaceObject == null ? null : surfaceObject.GetComponent<NavMeshSurface>();
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
            builder.AppendLine("# S5-05 Unity Playable End-To-End Gate Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine($"**Story:** `production/stories/{StorySlug}.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireS5PlayableEndToEndGateRunner.cs`");
            builder.AppendLine("**Fence coverage:** mechanical [F1]/[F2]/[F7] only; [F3] and the S5-05-04 feel verdict are human-play scope");
            builder.AppendLine($"**Result:** {(exitCode == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine();
            builder.AppendLine("## [F1] Artifact Identity");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(F1TelemetryKey));
            builder.AppendLine();
            builder.AppendLine("## Checks");
            builder.AppendLine();
            AppendCheckLines(builder, GetSessionLines(ChecksKey));
            builder.AppendLine();
            builder.AppendLine("## [F2] Walked Segment Telemetry");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(WalkTelemetryKey));
            builder.AppendLine();
            builder.AppendLine("## Objective Loop Telemetry ([F7])");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(TelemetryKey));
            builder.AppendLine();
            builder.AppendLine("## [F3] Input-Path Note");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(F3TelemetryKey));
            builder.AppendLine();
            builder.AppendLine("## Warnings");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(WarningsKey));
            builder.AppendLine();
            builder.AppendLine("## Errors");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(ErrorsKey));

            File.WriteAllText(evidencePath, builder.ToString());
            Debug.Log($"{StoryId} playable end-to-end gate verification wrote {evidencePath} with exit code {exitCode}.");
            ClearSession();
            _gate = null;
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
                $"unity-playable-end-to-end-gate-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-smoke.md");
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

        private static void AppendCheckLines(StringBuilder builder, List<string> lines)
        {
            if (lines.Count == 0)
            {
                builder.AppendLine("- FAIL `no_checks_recorded`");
                return;
            }

            foreach (var check in lines)
            {
                var parts = check.Split('=');
                var name = parts[0];
                var passed = parts.Length > 1 && parts[1] == "PASS";
                builder.AppendLine($"- {(passed ? "PASS" : "FAIL")} `{name}`");
            }
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
            SessionState.EraseString(WalkTelemetryKey);
            SessionState.EraseString(F1TelemetryKey);
            SessionState.EraseString(F3TelemetryKey);
            SessionState.EraseString(TargetEventSequenceKey);
            SessionState.EraseString(EvidencePathKey);
            SessionState.EraseString(PlayStartedKey);
        }

        private sealed class GateState
        {
            public GateState(SceneObjects scene)
            {
                Scene = scene;
            }

            public SceneObjects Scene { get; }

            public int LegIndex { get; set; }

            public bool LegInitialized { get; set; }

            public Vector3[] Corners { get; set; } = Array.Empty<Vector3>();

            public int CornerIndex { get; set; }

            public float ExpectedWalkMeters { get; set; }

            public float WalkedMeters { get; set; }

            public int TickCount { get; set; }

            public double LegStartedSeconds { get; set; }

            public double LastTickSeconds { get; set; }
        }

        private readonly struct SampledPosition
        {
            public SampledPosition(bool sampled, Vector3 position)
            {
                Sampled = sampled;
                Position = position;
            }

            public bool Sampled { get; }

            public Vector3 Position { get; }
        }

        private readonly struct SceneObjects
        {
            public SceneObjects(
                S3PlayerInteractionHarness? harness,
                GameObject? playerMarker,
                GameObject? caretaker,
                M3NamedNpcObjectiveFrame? frame,
                M3NamedNpcInteractTarget? npcAdapter,
                M3ObjectiveStateRelicHandIn? objective,
                GameObject? relicObject,
                M3RelicInteractTarget? relicAdapter,
                M3LootTableFixedProfileVendor? vendor,
                GameObject? vendorObject,
                M3VendorInteractTarget? vendorAdapter)
            {
                Harness = harness!;
                PlayerMarker = playerMarker!;
                Caretaker = caretaker!;
                Frame = frame!;
                NpcAdapter = npcAdapter!;
                Objective = objective!;
                RelicObject = relicObject!;
                RelicAdapter = relicAdapter!;
                Vendor = vendor!;
                VendorObject = vendorObject!;
                VendorAdapter = vendorAdapter!;
                Valid = harness != null &&
                    playerMarker != null &&
                    caretaker != null &&
                    frame != null &&
                    npcAdapter != null &&
                    objective != null &&
                    relicObject != null &&
                    relicAdapter != null &&
                    vendor != null &&
                    vendorObject != null &&
                    vendorAdapter != null;
            }

            public bool Valid { get; }

            public S3PlayerInteractionHarness Harness { get; }

            public GameObject PlayerMarker { get; }

            public GameObject Caretaker { get; }

            public M3NamedNpcObjectiveFrame Frame { get; }

            public M3NamedNpcInteractTarget NpcAdapter { get; }

            public M3ObjectiveStateRelicHandIn Objective { get; }

            public GameObject RelicObject { get; }

            public M3RelicInteractTarget RelicAdapter { get; }

            public M3LootTableFixedProfileVendor Vendor { get; }

            public GameObject VendorObject { get; }

            public M3VendorInteractTarget VendorAdapter { get; }
        }
    }
}
#endif
