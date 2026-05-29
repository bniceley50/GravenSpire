#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Gravenspire.Gameplay.Npc.M3Objective;
using Gravenspire.UnityRuntime.Combat;
using Gravenspire.UnityRuntime.Npc;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravenspire.Editor
{
    [InitializeOnLoad]
    public static class GravenspireM3EndToEndObjectiveLoopVerificationRunner
    {
        private const string StoryId = "S2-M3-04";
        private const string StorySlug = "s2-m3-04-end-to-end-objective-loop";
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string RunKey = "GravenspireM3EndToEndObjectiveLoop.Run";
        private const string PhaseKey = "GravenspireM3EndToEndObjectiveLoop.Phase";
        private const string ChecksKey = "GravenspireM3EndToEndObjectiveLoop.Checks";
        private const string ErrorsKey = "GravenspireM3EndToEndObjectiveLoop.Errors";
        private const string WarningsKey = "GravenspireM3EndToEndObjectiveLoop.Warnings";
        private const string TelemetryKey = "GravenspireM3EndToEndObjectiveLoop.Telemetry";
        private const string EvidencePathKey = "GravenspireM3EndToEndObjectiveLoop.EvidencePath";
        private const string FakeFactionConsequenceEventsKey = "GravenspireM3EndToEndObjectiveLoop.FakeFactionConsequenceEvents";
        private const string FakeFactionConsequenceEnabledKey = "GravenspireM3EndToEndObjectiveLoop.FakeFactionConsequenceEnabled";
        private const string PlayStartedKey = "GravenspireM3EndToEndObjectiveLoop.PlayStartedTicks";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const string FakeFactionConsequenceArgumentName = "-gravenspireFakeFactionConsequence";
        private const string FakeFactionConsequenceObjectName = "RunnerLocalFakeFactionConsequenceProbe";
        private const double SmokeDelaySeconds = 1.0d;
        private static readonly string[] FactionConsequenceMarkers =
        {
            "FactionConsequence",
            "FactionReaction",
            "FactionStandingConsequence",
            "FactionSink"
        };

        static GravenspireM3EndToEndObjectiveLoopVerificationRunner()
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

        [MenuItem("Gravenspire/Verify M3 End-To-End Objective Loop")]
        public static void Run()
        {
            ClearSession();
            SessionState.SetBool(RunKey, true);
            SessionState.SetString(PhaseKey, "initial");
            Application.logMessageReceived += CaptureLog;

            try
            {
                var evidencePath = ResolveEvidencePathFromCommandLine(DefaultEvidencePath());
                SessionState.SetString(EvidencePathKey, evidencePath);
                SessionState.SetBool(FakeFactionConsequenceEnabledKey, HasCommandLineFlag(FakeFactionConsequenceArgumentName));
                Directory.CreateDirectory(Path.GetDirectoryName(evidencePath) ?? ".");

                GravenspireM3NamedNpcObjectiveFrameBuilder.Build();
                GravenspireM3ObjectiveStateRelicHandInBuilder.Build();
                GravenspireM3LootTableFixedProfileVendorBuilder.Build();

                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

                RecordCheck("scene_loaded", scene.IsValid() && scene.path == ScenePath);
                RecordCheck("m3_caretaker_anchor_exists", GameObject.Find(M3NamedNpcObjectiveFrame.AnchorObjectName) != null);
                RecordCheck(
                    "m3_objective_root_exists",
                    FindSceneObjectIncludingInactive(M3ObjectiveStateRelicHandInSession.RelicObjectName) != null ||
                    FindObjectiveComponentIncludingInactive() != null);
                RecordCheck("m3_objective_relic_exists", FindSceneObjectIncludingInactive(M3ObjectiveStateRelicHandInSession.RelicObjectName) != null);
                RecordCheck("m3_loot_vendor_root_exists", GameObject.Find(M3LootTableFixedProfileVendorData.VendorObjectName) != null);
                RecordCheck("m3_court_vendor_component_exists", FindVendorComponentIncludingInactive() != null);
                RecordCheck("m2_combat_camp_loop_root_exists", GameObject.Find("M2_CombatCampLoopRoot")?.GetComponent<M2SingleTrashMedLoopController>() != null);
                RecordCheck("cleric_shell_marker_exists", GameObject.Find("ClericShellMarker") != null);

                SessionState.SetString(PhaseKey, "entering_play");
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

            RunSmokeChecks();
            WriteEvidenceAndExit(AllChecksPassed() && GetSessionLines(ErrorsKey).Count == 0 ? 0 : 1);
        }

        private static void RunSmokeChecks()
        {
            // --- Component discovery ---
            var npc = UnityEngine.Object.FindFirstObjectByType<M3NamedNpcObjectiveFrame>();
            RecordCheck("npc_component_found_in_play_mode", npc != null);
            if (npc is null)
            {
                AppendSessionLine(ErrorsKey, "M3 named NPC objective frame component was not found.");
                return;
            }

            var objective = UnityEngine.Object.FindFirstObjectByType<M3ObjectiveStateRelicHandIn>();
            RecordCheck("objective_component_found_in_play_mode", objective != null);
            if (objective is null)
            {
                AppendSessionLine(ErrorsKey, "M3 objective state relic hand-in component was not found.");
                return;
            }

            var vendor = UnityEngine.Object.FindFirstObjectByType<M3LootTableFixedProfileVendor>();
            RecordCheck("vendor_component_found_in_play_mode", vendor != null);
            if (vendor is null)
            {
                AppendSessionLine(ErrorsKey, "M3 loot table fixed-profile vendor component was not found.");
                return;
            }

            var controller = UnityEngine.Object.FindFirstObjectByType<M2SingleTrashMedLoopController>();
            RecordCheck("m2_controller_found_in_play_mode", controller != null);
            if (controller is null)
            {
                AppendSessionLine(ErrorsKey, "M2 combat loop controller was not found.");
                return;
            }

            var playerMarker = GameObject.Find("ClericShellMarker");
            RecordCheck("cleric_marker_found_in_play_mode", playerMarker != null);
            if (playerMarker is null)
            {
                AppendSessionLine(ErrorsKey, "ClericShellMarker was not found.");
                return;
            }

            // Telemetry: npc_anchor_present (AC-02)
            AppendSessionLine(TelemetryKey, $"npc_anchor_present={npc != null}");

            // --- Session reset ---
            npc!.ClearSessionInteractions();
            objective!.ResetSessionObjective();
            vendor!.ResetSessionVendor();

            // --- Step 1: Position player and compute distance ---
            playerMarker.transform.position = npc.transform.position + new Vector3(0.0f, 0.0f, -1.25f);
            var markerPos = playerMarker.transform.position;
            var npcPos = npc.transform.position;
            var distance = Vector3.Distance(markerPos, npcPos);

            // --- Step 2: TryAcceptObjectiveFromNpc (NPC interaction + NotIntroduced -> Accepted) ---
            var accepted = objective.TryAcceptObjectiveFromNpc(npc, M3NamedNpcObjectiveFrame.PlayerActorId, distance);
            RecordCheck("objective_accepted_from_npc", accepted && objective.State == M3ObjectiveState.Accepted);

            // Telemetry: npc_interaction_intentional and dialogue_template_id (AC-02)
            AppendSessionLine(TelemetryKey, $"npc_interaction_intentional={npc.LastInteraction.WasIntentional}");
            AppendSessionLine(TelemetryKey, $"dialogue_template_id={npc.LastInteraction.DialogueTemplateSetId}");

            // Telemetry: relic_available right after accept (AC-02)
            AppendSessionLine(TelemetryKey, $"relic_available={objective.RelicAvailable}");

            // --- Step 3: TryRecoverRelic (Accepted -> RelicRecovered) ---
            var recovered = objective.TryRecoverRelic();
            RecordCheck("relic_recovered", recovered && objective.State == M3ObjectiveState.RelicRecovered);

            // --- Step 4: TryResolveObjectiveLoot (fills carried items: relic + salvage) ---
            var lootResolved = vendor.TryResolveObjectiveLoot();
            RecordCheck("objective_loot_resolved", lootResolved);
            RecordCheck("resolved_relic_carried", vendor.CarriesCourtMarkedRelic);
            RecordCheck("resolved_salvage_carried", vendor.CarriesSalvage);

            // Telemetry: loot_table_id and loot_result_item_ids (AC-02)
            AppendSessionLine(TelemetryKey, $"loot_table_id={vendor.ConfiguredLootTableId}");
            var relicItemId = M3ObjectiveStateRelicHandInSession.RelicItemId;
            var salvageItemId = M3LootTableFixedProfileVendorData.SalvageItemId;
            AppendSessionLine(TelemetryKey, $"loot_result_item_ids={relicItemId}|{salvageItemId}");

            // --- Step 5: TryReturnRelicToNpc (RelicRecovered -> Complete) ---
            var handedIn = objective.TryReturnRelicToNpc(npc);
            RecordCheck("relic_handed_in_to_npc", handedIn && objective.IsComplete && objective.State == M3ObjectiveState.Complete);

            // Telemetry: objective_state_sequence, relic_handed_in, objective_complete (AC-02)
            AppendSessionLine(TelemetryKey, $"objective_state_sequence={objective.StateSequence}");
            AppendSessionLine(TelemetryKey, $"relic_handed_in={handedIn}");
            AppendSessionLine(TelemetryKey, $"objective_complete={objective.IsComplete}");

            // --- Step 6: TrySellRecoveredSalvage (F4 salvage sale; creditedCopper == 7) ---
            var sold = vendor.TrySellRecoveredSalvage(out var creditedCopper);
            RecordCheck("vendor_salvage_sale_succeeds", sold);
            RecordCheck("vendor_f4_formula_credits_7_copper", creditedCopper == 7);
            RecordCheck("salvage_removed_after_sale", !vendor.CarriesSalvage);

            // Telemetry: vendor_salvage_sold and vendor_sell_copper_applied (AC-02)
            AppendSessionLine(TelemetryKey, $"vendor_salvage_sold={sold}");
            AppendSessionLine(TelemetryKey, $"vendor_sell_copper_applied={creditedCopper}");

            // --- M2 preservation: named-blocker boundary smoke ---
            // The M2 controller's smoke methods are single-invocation per Play
            // session. Calling RunAutomatedTwoPullSmoke() ahead of this advances
            // the shared, never-reset _playerMeleeRandom cursor, which makes the
            // named-blocker scenario solo-kill the blocker and fail its FEEL-02
            // guard. RunAutomatedNamedBlockerBoundarySmoke() already re-runs the
            // clean two-pull internally, so a single call yields both M2
            // preservation signals, matching GravenspireM2NamedBlockerVerificationRunner.
            RecordCheck("m2_controller_initialized_before_m2_smoke", controller.IsInitialized);

            var m2NamedBlocker = controller.RunAutomatedNamedBlockerBoundarySmoke();
            var m2CleanLoopPreserved = controller.CleanSingleTrashLoopPreservedAfterNamedBlocker;

            RecordCheck("m2_named_blocker_boundary_smoke_passes", m2NamedBlocker);
            RecordCheck("m2_clean_loop_preserved_after_named_blocker", m2CleanLoopPreserved);

            foreach (var error in controller.Errors)
            {
                AppendSessionLine(ErrorsKey, $"m2_controller: {error}");
            }

            RecordCheck("m2_controller_reported_no_errors", controller.Errors.Count == 0);

            // Telemetry: m2_clean_loop_preserved and m2_named_blocker_boundary_preserved (AC-03)
            AppendSessionLine(TelemetryKey, $"m2_clean_loop_preserved={m2CleanLoopPreserved}");
            AppendSessionLine(TelemetryKey, $"m2_named_blocker_boundary_preserved={m2NamedBlocker}");

            // --- Step 9: no_save_load_state_written (AC-04) ---
            // Derived from objective.SessionLocalOnly and vendor.SessionLocalOnly.
            // Neither component has a persistence path; session-local-only is the
            // architectural contract for M3. No save file exists anywhere in this runner.
            var noSaveLoadState = objective.SessionLocalOnly && vendor.SessionLocalOnly && npc.SessionLocalOnly;
            RecordCheck("no_save_load_state_written", noSaveLoadState);
            AppendSessionLine(TelemetryKey, $"no_save_load_state_written={noSaveLoadState}");

            // --- Step 10: no_faction_consequence_applied (AC-04 / S3-EVIDENCE-01 AC-1) ---
            if (SessionState.GetBool(FakeFactionConsequenceEnabledKey, false))
            {
                TriggerRunnerLocalFakeFactionConsequenceProbe();
            }

            var factionAbsence = EvaluateFactionConsequenceAbsence(vendor);
            RecordCheck("no_faction_consequence_scene_object_present", !factionAbsence.SceneObjectPresent);
            RecordCheck("no_faction_consequence_component_present", !factionAbsence.ComponentPresent);
            RecordCheck("no_faction_consequence_event_present", !factionAbsence.EventPresent);
            RecordCheck("no_faction_consequence_event_fired", !factionAbsence.EventFired);
            RecordCheck("m5_vendor_reputation_discount_hook_absent", !vendor.HasReputationDiscountHook);
            RecordCheck("m5_vendor_faction_rank_goods_hook_absent", !vendor.HasFactionRankGoodsHook);
            RecordCheck("no_faction_consequence_applied", factionAbsence.NoFactionConsequenceApplied);
            if (SessionState.GetBool(FakeFactionConsequenceEnabledKey, false))
            {
                RecordCheck(
                    "runner_local_fake_faction_consequence_negative_control_tripped",
                    factionAbsence.EventFired && !factionAbsence.NoFactionConsequenceApplied);
            }

            AppendSessionLine(TelemetryKey, $"runner_local_fake_faction_consequence_enabled={SessionState.GetBool(FakeFactionConsequenceEnabledKey, false)}");
            AppendSessionLine(TelemetryKey, $"faction_consequence_scene_objects={FormatMatches(factionAbsence.SceneObjectMatches)}");
            AppendSessionLine(TelemetryKey, $"faction_consequence_components={FormatMatches(factionAbsence.ComponentMatches)}");
            AppendSessionLine(TelemetryKey, $"faction_consequence_events={FormatMatches(factionAbsence.EventMatches)}");
            AppendSessionLine(TelemetryKey, $"runner_local_fake_faction_consequence_events={factionAbsence.FakeEventCount.ToString(CultureInfo.InvariantCulture)}");
            AppendSessionLine(TelemetryKey, $"vendor_reputation_discount_hook={vendor.HasReputationDiscountHook}");
            AppendSessionLine(TelemetryKey, $"vendor_faction_rank_goods_hook={vendor.HasFactionRankGoodsHook}");
            AppendSessionLine(TelemetryKey, $"no_faction_consequence_applied={factionAbsence.NoFactionConsequenceApplied}");

            // --- State sequence correctness check ---
            RecordCheck(
                "state_sequence_exact",
                objective.StateSequence == "NotIntroduced -> Accepted -> RelicRecovered -> Complete");
        }

        private static M3LootTableFixedProfileVendor? FindVendorComponentIncludingInactive()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<M3LootTableFixedProfileVendor>(includeInactive: true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static M3ObjectiveStateRelicHandIn? FindObjectiveComponentIncludingInactive()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<M3ObjectiveStateRelicHandIn>(includeInactive: true);
                if (component != null)
                {
                    return component;
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

        private static void TriggerRunnerLocalFakeFactionConsequenceProbe()
        {
            var probe = new RunnerLocalFakeFactionConsequenceProbe();
            probe.FactionConsequenceApplied += () =>
            {
                SessionState.SetInt(
                    FakeFactionConsequenceEventsKey,
                    SessionState.GetInt(FakeFactionConsequenceEventsKey, 0) + 1);
            };
            _ = new GameObject(FakeFactionConsequenceObjectName);
            probe.Fire();
        }

        private static FactionConsequenceAbsenceResult EvaluateFactionConsequenceAbsence(M3LootTableFixedProfileVendor vendor)
        {
            var sceneObjectMatches = new List<string>();
            var componentMatches = new List<string>();
            var eventMatches = new List<string>();
            var scene = SceneManager.GetActiveScene();

            if (scene.IsValid())
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var transform in root.GetComponentsInChildren<Transform>(includeInactive: true))
                    {
                        if (ContainsFactionConsequenceMarker(transform.name))
                        {
                            sceneObjectMatches.Add(GetScenePath(transform));
                        }
                    }
                }
            }

            foreach (var component in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (component == null || component.gameObject.scene != scene)
                {
                    continue;
                }

                var type = component.GetType();
                if (ContainsFactionConsequenceMarker(component.gameObject.name) ||
                    ContainsFactionConsequenceMarker(type.FullName ?? type.Name))
                {
                    componentMatches.Add($"{GetScenePath(component.transform)}:{type.FullName}");
                }

                foreach (var eventInfo in type.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (ContainsFactionConsequenceMarker(eventInfo.Name))
                    {
                        eventMatches.Add($"{type.FullName}.{eventInfo.Name}");
                    }
                }
            }

            var fakeEventCount = SessionState.GetInt(FakeFactionConsequenceEventsKey, 0);
            var noFactionConsequenceApplied =
                sceneObjectMatches.Count == 0 &&
                componentMatches.Count == 0 &&
                eventMatches.Count == 0 &&
                fakeEventCount == 0 &&
                !vendor.HasReputationDiscountHook &&
                !vendor.HasFactionRankGoodsHook;

            return new FactionConsequenceAbsenceResult(
                sceneObjectMatches,
                componentMatches,
                eventMatches,
                fakeEventCount,
                noFactionConsequenceApplied);
        }

        private static bool ContainsFactionConsequenceMarker(string value)
        {
            foreach (var marker in FactionConsequenceMarkers)
            {
                if (value.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetScenePath(Transform transform)
        {
            var segments = new Stack<string>();
            var cursor = transform;
            while (cursor != null)
            {
                segments.Push(cursor.name);
                cursor = cursor.parent;
            }

            return string.Join("/", segments);
        }

        private static string FormatMatches(IReadOnlyList<string> matches)
        {
            return matches.Count == 0 ? "none" : string.Join("|", matches);
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
            builder.AppendLine($"# {StoryId} Unity End-To-End Objective Loop Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine($"**Story:** `production/stories/{StorySlug}.md`");
            builder.AppendLine("**Evidence Patch:** `production/stories/s3-evidence-integrity-patch.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireM3EndToEndObjectiveLoopVerificationRunner.cs`");
            builder.AppendLine("**Scene Preparation:** Existing runner builder chain rebuilds before play; AC-1 verifies faction-consequence structural absence in runtime code/components, not authored-scene preservation.");
            if (SessionState.GetBool(FakeFactionConsequenceEnabledKey, false))
            {
                builder.AppendLine("**Negative Control:** runner-local fake faction consequence enabled; expected result FAIL");
            }

            builder.AppendLine($"**Result:** {(exitCode == 0 ? "PASS" : "FAIL")}");
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
            builder.AppendLine("## End-To-End Objective Loop Telemetry");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(TelemetryKey));
            builder.AppendLine();
            builder.AppendLine("## Warnings");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(WarningsKey));
            builder.AppendLine();
            builder.AppendLine("## Errors");
            builder.AppendLine();
            AppendEvidenceLines(builder, GetSessionLines(ErrorsKey));

            File.WriteAllText(evidencePath, builder.ToString());
            Debug.Log($"{StoryId} end-to-end objective loop verification wrote {evidencePath} with exit code {exitCode}.");
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
                $"unity-end-to-end-objective-loop-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-smoke.md");
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

        private static bool HasCommandLineFlag(string flagName)
        {
            var arguments = Environment.GetCommandLineArgs();
            foreach (var argument in arguments)
            {
                if (string.Equals(argument, flagName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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
            SessionState.EraseString(EvidencePathKey);
            SessionState.EraseInt(FakeFactionConsequenceEventsKey);
            SessionState.EraseBool(FakeFactionConsequenceEnabledKey);
            SessionState.EraseString(PlayStartedKey);
        }

        private sealed class RunnerLocalFakeFactionConsequenceProbe
        {
            public event Action? FactionConsequenceApplied;

            public void Fire()
            {
                FactionConsequenceApplied?.Invoke();
            }
        }

        private readonly struct FactionConsequenceAbsenceResult
        {
            public FactionConsequenceAbsenceResult(
                IReadOnlyList<string> sceneObjectMatches,
                IReadOnlyList<string> componentMatches,
                IReadOnlyList<string> eventMatches,
                int fakeEventCount,
                bool noFactionConsequenceApplied)
            {
                SceneObjectMatches = sceneObjectMatches;
                ComponentMatches = componentMatches;
                EventMatches = eventMatches;
                FakeEventCount = fakeEventCount;
                NoFactionConsequenceApplied = noFactionConsequenceApplied;
            }

            public IReadOnlyList<string> SceneObjectMatches { get; }

            public IReadOnlyList<string> ComponentMatches { get; }

            public IReadOnlyList<string> EventMatches { get; }

            public int FakeEventCount { get; }

            public bool SceneObjectPresent => SceneObjectMatches.Count > 0;

            public bool ComponentPresent => ComponentMatches.Count > 0;

            public bool EventPresent => EventMatches.Count > 0;

            public bool EventFired => FakeEventCount > 0;

            public bool NoFactionConsequenceApplied { get; }
        }
    }
}
#endif
