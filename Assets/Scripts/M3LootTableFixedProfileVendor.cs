#nullable enable

using System.IO;
using Gravenspire.Gameplay.Npc.M3Objective;
using UnityEngine;

namespace Gravenspire.UnityRuntime.Npc
{
    [DisallowMultipleComponent]
    public sealed class M3LootTableFixedProfileVendor : MonoBehaviour
    {
        public const string AuthoredDataRelativePath = "data/first-district/m3-objective-npc-loot.json";

        private M3LootTableFixedProfileVendorSession? _session;

        public bool LoadedAuthoredDataFile { get; private set; }

        public bool UsingFallbackData { get; private set; } = true;

        public string ResolvedAuthoredDataPath { get; private set; } = string.Empty;

        public string ConfiguredVendorId => Session.VendorId;

        public string ConfiguredLootTableId => Session.LootTableId;

        public int CarriedCurrencyCopper => Session.CarriedCurrencyCopper;

        public int CarriedItemSlotsUsed => Session.CarriedItemSlotsUsed;

        public bool SessionLocalOnly => Session.SessionLocalOnly;

        public bool UsesProgressionSeedAsLootRng => Session.UsesProgressionSeedAsLootRng;

        public bool HasDynamicPricingHook => Session.HasDynamicPricingHook;

        public bool HasStockSimulationHook => Session.HasStockSimulationHook;

        public bool HasReputationDiscountHook => Session.HasReputationDiscountHook;

        public bool HasLimitedTimeRotationHook => Session.HasLimitedTimeRotationHook;

        public bool HasTokenBuyingHook => Session.HasTokenBuyingHook;

        public bool HasFactionRankGoodsHook => Session.HasFactionRankGoodsHook;

        public bool HasArbitrageHook => Session.HasArbitrageHook;

        public bool MakesCoinFaucetProjectionClaim => Session.MakesCoinFaucetProjectionClaim;

        public bool PersistsCurrencyAtRest => Session.PersistsCurrencyAtRest;

        public bool CarriesCourtMarkedRelic => Session.HasCarriedItem(M3ObjectiveStateRelicHandInSession.RelicItemId);

        public bool CarriesSalvage => Session.HasCarriedItem(M3LootTableFixedProfileVendorData.SalvageItemId);

        public string LastRejectionReason { get; private set; } = string.Empty;

        private M3LootTableFixedProfileVendorSession Session
        {
            get
            {
                if (_session is null)
                {
                    ResetSessionVendor();
                }

                return _session!;
            }
        }

        private void Awake()
        {
            ResetSessionVendor();
        }

        public void ConfigureForM3LootTableFixedProfileVendor()
        {
            ResetSessionVendor();
        }

        public void ResetSessionVendor()
        {
            var data = LoadAuthoredDataForRuntime();
            _session = new M3LootTableFixedProfileVendorSession(
                data);
            LastRejectionReason = string.Empty;
        }

        public bool TryResolveObjectiveLoot()
        {
            if (!Session.TryResolveDefaultLoot(out _, out var rejectionReason))
            {
                LastRejectionReason = rejectionReason;
                return false;
            }

            LastRejectionReason = string.Empty;
            return true;
        }

        public bool TrySellRecoveredSalvage(out int creditedCopper)
        {
            creditedCopper = 0;
            if (!Session.TrySellSalvage(
                    M3LootTableFixedProfileVendorData.SalvageItemId,
                    quantity: 1,
                    out var result,
                    out var rejectionReason))
            {
                LastRejectionReason = rejectionReason;
                return false;
            }

            creditedCopper = result.CreditedCopper;
            LastRejectionReason = string.Empty;
            return true;
        }

        public bool TryPurchaseFixedVendorGood(out int debitedCopper)
        {
            debitedCopper = 0;
            if (!Session.TryPurchaseFixedVendorGood(
                    M3LootTableFixedProfileVendorData.FixedBuyOfferId,
                    quantity: 1,
                    out var result,
                    out var rejectionReason))
            {
                LastRejectionReason = rejectionReason;
                return false;
            }

            debitedCopper = result.DebitedCopper;
            LastRejectionReason = string.Empty;
            return true;
        }

        public int GetCarriedQuantity(string itemId)
        {
            return Session.GetCarriedQuantity(itemId);
        }

        private M3LootTableFixedProfileVendorData LoadAuthoredDataForRuntime()
        {
            ResolvedAuthoredDataPath = ResolveProjectRelativePath(AuthoredDataRelativePath);
            if (File.Exists(ResolvedAuthoredDataPath))
            {
                LoadedAuthoredDataFile = true;
                UsingFallbackData = false;
                return M3LootTableFixedProfileVendorData.LoadFromFile(ResolvedAuthoredDataPath);
            }

            Debug.LogWarning(
                $"M3 loot/vendor authored data was missing at {ResolvedAuthoredDataPath}; using documented fallback data.");
            LoadedAuthoredDataFile = false;
            UsingFallbackData = true;
            return M3LootTableFixedProfileVendorData.CreateAuthoredM3Default();
        }

        private static string ResolveProjectRelativePath(string relativePath)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }
    }
}
