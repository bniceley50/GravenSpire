#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
    public static class GravenspireM3LootTableFixedProfileVendorVerificationRunner
    {
        private const string StoryId = "S2-M3-03";
        private const string StorySlug = "s2-m3-03-loot-table-fixed-profile-vendor";
        private const string ScenePath = "Assets/Scenes/_DevEntry.unity";
        private const string DataPath = M3LootTableFixedProfileVendor.AuthoredDataRelativePath;
        private const string RunKey = "GravenspireM3LootTableFixedProfileVendor.Run";
        private const string PhaseKey = "GravenspireM3LootTableFixedProfileVendor.Phase";
        private const string ChecksKey = "GravenspireM3LootTableFixedProfileVendor.Checks";
        private const string ErrorsKey = "GravenspireM3LootTableFixedProfileVendor.Errors";
        private const string WarningsKey = "GravenspireM3LootTableFixedProfileVendor.Warnings";
        private const string TelemetryKey = "GravenspireM3LootTableFixedProfileVendor.Telemetry";
        private const string EvidencePathKey = "GravenspireM3LootTableFixedProfileVendor.EvidencePath";
        private const string PlayStartedKey = "GravenspireM3LootTableFixedProfileVendor.PlayStartedTicks";
        private const string EvidencePathArgumentName = "-gravenspireEvidencePath";
        private const double SmokeDelaySeconds = 1.0d;

        static GravenspireM3LootTableFixedProfileVendorVerificationRunner()
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

        [MenuItem("Gravenspire/Verify M3 Loot Table Fixed-Profile Vendor")]
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
                Directory.CreateDirectory(Path.GetDirectoryName(evidencePath) ?? ".");
                GravenspireM3LootTableFixedProfileVendorBuilder.Build();
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                var authoredDataPath = ResolveProjectRelativePath(DataPath);

                RecordCheck("scene_loaded", scene.IsValid() && scene.path == ScenePath);
                RecordCheck("authored_data_file_exists", File.Exists(authoredDataPath));
                RecordCheck("authored_data_file_valid", M3LootTableFixedProfileVendorData.LoadFromFile(authoredDataPath).TryValidateForM3(out _));
                RecordCheck("m3_vendor_marker_exists", GameObject.Find(M3LootTableFixedProfileVendorData.VendorObjectName) != null);
                RecordCheck("m3_vendor_component_exists", FindVendorComponentIncludingInactive() != null);
                RecordCheck("exactly_one_m3_vendor_component", CountVendorComponentsIncludingInactive() == 1);
                RecordCheck("m3_objective_component_exists", FindObjectiveComponentIncludingInactive() != null);
                RecordCheck("m3_caretaker_anchor_exists", GameObject.Find(M3NamedNpcObjectiveFrame.AnchorObjectName) != null);
                RecordCheck("m2_loop_controller_exists", GameObject.Find("M2_CombatCampLoopRoot")?.GetComponent<M2SingleTrashMedLoopController>() != null);

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
            var vendor = UnityEngine.Object.FindFirstObjectByType<M3LootTableFixedProfileVendor>();
            RecordCheck("vendor_component_found_in_play_mode", vendor != null);
            if (vendor is null)
            {
                AppendSessionLine(ErrorsKey, "M3 loot table fixed-profile vendor component was not found.");
                return;
            }

            RecordCheck("vendor_loaded_authored_data_file", vendor.LoadedAuthoredDataFile);
            RecordCheck("vendor_not_using_fallback_data", !vendor.UsingFallbackData);

            var objective = UnityEngine.Object.FindFirstObjectByType<M3ObjectiveStateRelicHandIn>();
            RecordCheck("objective_component_found_in_play_mode", objective != null);
            if (objective is null)
            {
                AppendSessionLine(ErrorsKey, "M3 objective state component was not found.");
                return;
            }

            var npc = UnityEngine.Object.FindFirstObjectByType<M3NamedNpcObjectiveFrame>();
            RecordCheck("m3_caretaker_component_found_in_play_mode", npc != null);
            if (npc is null)
            {
                AppendSessionLine(ErrorsKey, "M3 caretaker component was not found.");
                return;
            }

            var playerMarker = GameObject.Find("ClericShellMarker");
            RecordCheck("cleric_marker_found_for_vendor_smoke", playerMarker != null);
            if (playerMarker is null)
            {
                AppendSessionLine(ErrorsKey, "ClericShellMarker was not found for M3 vendor smoke.");
                return;
            }

            npc.ClearSessionInteractions();
            objective.ResetSessionObjective();
            vendor.ResetSessionVendor();

            playerMarker.transform.position = npc.transform.position + new Vector3(0.0f, 0.0f, -1.25f);
            var distance = Vector3.Distance(playerMarker.transform.position, npc.transform.position);
            var accepted = objective.TryAcceptObjectiveFromNpc(npc, M3NamedNpcObjectiveFrame.PlayerActorId, distance);
            RecordCheck("objective_accepts_before_loot_resolution", accepted && objective.State == M3ObjectiveState.Accepted);

            var recovered = objective.TryRecoverRelic();
            RecordCheck("relic_recovered_before_loot_resolution", recovered && objective.State == M3ObjectiveState.RelicRecovered);

            var lootResolved = vendor.TryResolveObjectiveLoot();
            RecordCheck("authored_loot_table_resolved", lootResolved);
            RecordCheck("resolved_relic_carried", vendor.CarriesCourtMarkedRelic);
            RecordCheck("resolved_salvage_carried", vendor.CarriesSalvage);
            RecordCheck("loot_rng_seed_boundary_preserved", !vendor.UsesProgressionSeedAsLootRng);

            var sold = vendor.TrySellRecoveredSalvage(out var creditedCopper);
            RecordCheck("vendor_salvage_sale_succeeds", sold);
            RecordCheck("vendor_f4_formula_credits_7_copper", creditedCopper == 7 && vendor.CarriedCurrencyCopper == 7);
            RecordCheck("salvage_removed_after_sale", !vendor.CarriesSalvage);

            var purchased = vendor.TryPurchaseFixedVendorGood(out var debitedCopper);
            RecordCheck("fixed_profile_purchase_succeeds_after_capacity_prevalidation", purchased);
            RecordCheck("fixed_profile_purchase_debits_constant_price", debitedCopper == 3 && vendor.CarriedCurrencyCopper == 4);
            RecordCheck("fixed_vendor_good_recorded_in_session", vendor.GetCarriedQuantity(M3LootTableFixedProfileVendorData.FixedBuyOfferItemId) == 1);
            RecordCheck("vendor_session_local_only", vendor.SessionLocalOnly);
            RecordCheck("vendor_no_dynamic_pricing_hook", !vendor.HasDynamicPricingHook);
            RecordCheck("vendor_no_stock_simulation_hook", !vendor.HasStockSimulationHook);
            RecordCheck("vendor_no_reputation_discount_hook", !vendor.HasReputationDiscountHook);
            RecordCheck("vendor_no_rotation_hook", !vendor.HasLimitedTimeRotationHook);
            RecordCheck("vendor_no_token_or_rank_goods_hook", !vendor.HasTokenBuyingHook && !vendor.HasFactionRankGoodsHook);
            RecordCheck("vendor_no_arbitrage_hook", !vendor.HasArbitrageHook);
            RecordCheck("vendor_no_coin_faucet_projection_claim", !vendor.MakesCoinFaucetProjectionClaim);
            RecordCheck("vendor_no_currency_at_rest_persistence", !vendor.PersistsCurrencyAtRest);
            RecordCheck("no_vendor_rejection_reason", string.IsNullOrWhiteSpace(vendor.LastRejectionReason));

            AppendSessionLine(TelemetryKey, $"loot_table_id={vendor.ConfiguredLootTableId}");
            AppendSessionLine(TelemetryKey, $"vendor_id={vendor.ConfiguredVendorId}");
            AppendSessionLine(TelemetryKey, $"authored_data_path={vendor.ResolvedAuthoredDataPath}");
            AppendSessionLine(TelemetryKey, $"authored_data_file_loaded={vendor.LoadedAuthoredDataFile}");
            AppendSessionLine(TelemetryKey, $"objective_state_before_vendor={objective.State}");
            AppendSessionLine(TelemetryKey, $"resolved_relic_carried={vendor.CarriesCourtMarkedRelic}");
            AppendSessionLine(TelemetryKey, $"salvage_sale_credited_copper={creditedCopper}");
            AppendSessionLine(TelemetryKey, $"purchase_debited_copper={debitedCopper}");
            AppendSessionLine(TelemetryKey, $"ending_carried_currency_copper={vendor.CarriedCurrencyCopper}");
            AppendSessionLine(TelemetryKey, $"ending_carried_slots={vendor.CarriedItemSlotsUsed}");

            var controller = UnityEngine.Object.FindFirstObjectByType<M2SingleTrashMedLoopController>();
            RecordCheck("m2_controller_found_after_vendor_changes", controller != null);
            if (controller is null)
            {
                AppendSessionLine(ErrorsKey, "M2 combat loop controller was not found after vendor changes.");
                return;
            }

            RecordCheck("m2_controller_initialized_after_vendor_changes", controller.IsInitialized);
            foreach (var error in controller.Errors)
            {
                AppendSessionLine(ErrorsKey, error);
            }
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

        private static int CountVendorComponentsIncludingInactive()
        {
            var count = 0;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                count += root.GetComponentsInChildren<M3LootTableFixedProfileVendor>(includeInactive: true).Length;
            }

            return count;
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
            builder.AppendLine($"# {StoryId} Unity Loot Table Fixed-Profile Vendor Smoke");
            builder.AppendLine();
            builder.AppendLine($"**Date:** {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            builder.AppendLine($"**Story:** `production/stories/{StorySlug}.md`");
            builder.AppendLine("**Scene:** `Assets/Scenes/_DevEntry.unity`");
            builder.AppendLine("**Runner:** `Assets/Editor/GravenspireM3LootTableFixedProfileVendorVerificationRunner.cs`");
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
            builder.AppendLine("## Vendor Telemetry");
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
            Debug.Log($"{StoryId} loot table fixed-profile vendor verification wrote {evidencePath} with exit code {exitCode}.");
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
                $"unity-loot-table-fixed-profile-vendor-{DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-smoke.md");
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

        private static string ResolveProjectRelativePath(string relativePath)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
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
            SessionState.EraseString(PlayStartedKey);
        }
    }
}
#endif
