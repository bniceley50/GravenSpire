#nullable enable

using System.Collections.Generic;
using System.Globalization;
using Gravenspire.Gameplay.Npc.M3Objective;
using Gravenspire.UnityRuntime.Interaction;
using UnityEngine;

namespace Gravenspire.UnityRuntime.Npc
{
    /// <summary>
    /// SERVER-AUTH-INTENT: this T1 adapter emits a local sell intent and delegates
    /// the economy mutation to the existing M3 vendor API, keeping the intent
    /// boundary visible for a later remote-authority move.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class M3VendorInteractTarget : MonoBehaviour, IPlayerInteractTarget, IPlayerInteractTelemetryTarget
    {
        public const string SalvageSoldTelemetryEvent = "vendor_salvage_sold";
        public const string SellCopperAppliedTelemetryEvent = "vendor_sell_copper_applied";
        public const string SalvageSalePayloadKind = "vendor_salvage_sale:player_driven";
        public const string CopperAppliedPayloadKind = "vendor_sell_copper:player_driven";
        public const string SourceAttribution = "player_driven";

        private readonly List<InteractContext> _lastInteractTelemetryEvents = new();

        [SerializeField] private M3LootTableFixedProfileVendor? _vendor;

        private bool _missingVendorLogged;

        public IReadOnlyList<InteractContext> LastInteractTelemetryEvents => _lastInteractTelemetryEvents;

        public M3LootTableFixedProfileVendor? Vendor => ResolveVendor(logIfMissing: false);

        public int InteractionAttemptCount { get; private set; }

        public int SuccessfulSaleCount { get; private set; }

        public int LastCreditedCopper { get; private set; }

        public int LastPostSaleCurrencyCopper { get; private set; }

        public int LastPostSaleSlotsUsed { get; private set; }

        public string LastSaleRejectionReason { get; private set; } = string.Empty;

        private void Awake()
        {
            ResolveVendor(logIfMissing: true);
        }

        public void Configure(M3LootTableFixedProfileVendor vendor)
        {
            _vendor = vendor;
            _missingVendorLogged = false;
        }

        public bool TryInteract(string playerActorId, float distanceMeters, out InteractContext context)
        {
            context = default;
            InteractionAttemptCount++;
            LastCreditedCopper = 0;
            LastPostSaleCurrencyCopper = 0;
            LastPostSaleSlotsUsed = 0;
            LastSaleRejectionReason = string.Empty;
            _lastInteractTelemetryEvents.Clear();

            var vendor = ResolveVendor(logIfMissing: true);
            if (vendor == null)
            {
                return false;
            }

            if (!vendor.TrySellRecoveredSalvage(out var creditedCopper))
            {
                LastSaleRejectionReason = vendor.LastRejectionReason;
                return false;
            }

            SuccessfulSaleCount++;
            LastCreditedCopper = creditedCopper;
            LastPostSaleCurrencyCopper = vendor.CarriedCurrencyCopper;
            LastPostSaleSlotsUsed = vendor.CarriedItemSlotsUsed;

            context = MapSalvageSoldContext(playerActorId, vendor, creditedCopper, distanceMeters);
            _lastInteractTelemetryEvents.Add(context);
            _lastInteractTelemetryEvents.Add(MapCopperAppliedContext(playerActorId, vendor, creditedCopper, distanceMeters));
            return true;
        }

        private M3LootTableFixedProfileVendor? ResolveVendor(bool logIfMissing)
        {
            if (_vendor != null)
            {
                return _vendor;
            }

            _vendor = GetComponent<M3LootTableFixedProfileVendor>();
            if (_vendor != null)
            {
                return _vendor;
            }

            if (logIfMissing && !_missingVendorLogged)
            {
                Debug.LogError(
                    $"{nameof(M3VendorInteractTarget)} on {name} is missing its {nameof(M3LootTableFixedProfileVendor)} reference.");
                _missingVendorLogged = true;
            }

            return null;
        }

        private static InteractContext MapSalvageSoldContext(
            string playerActorId,
            M3LootTableFixedProfileVendor vendor,
            int creditedCopper,
            float distanceMeters)
        {
            return new InteractContext(
                SalvageSoldTelemetryEvent,
                playerActorId,
                vendor.ConfiguredVendorId,
                "vendor_salvage_sell",
                FormatCopperFeedback(creditedCopper),
                distanceMeters,
                SalvageSalePayloadKind,
                M3LootTableFixedProfileVendorData.SalvageItemId,
                SourceAttribution,
                1,
                FormatCopperFeedback(creditedCopper));
        }

        private static InteractContext MapCopperAppliedContext(
            string playerActorId,
            M3LootTableFixedProfileVendor vendor,
            int creditedCopper,
            float distanceMeters)
        {
            return new InteractContext(
                SellCopperAppliedTelemetryEvent,
                playerActorId,
                vendor.ConfiguredVendorId,
                "vendor_sell_copper_apply",
                FormatCopperFeedback(creditedCopper),
                distanceMeters,
                CopperAppliedPayloadKind,
                creditedCopper.ToString(CultureInfo.InvariantCulture),
                vendor.CarriedCurrencyCopper.ToString(CultureInfo.InvariantCulture),
                creditedCopper,
                FormatCopperFeedback(creditedCopper));
        }

        private static string FormatCopperFeedback(int creditedCopper)
        {
            return $"+{creditedCopper.ToString(CultureInfo.InvariantCulture)} copper";
        }
    }
}
