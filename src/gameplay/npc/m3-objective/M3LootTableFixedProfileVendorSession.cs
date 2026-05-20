#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
#if UNITY_5_3_OR_NEWER
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#else
using System.Text.Json;
#endif

namespace Gravenspire.Gameplay.Npc.M3Objective;

public enum M3LootEntryKind
{
    ObjectiveRelic,
    Salvage,
    FixedVendorGood
}

public readonly struct M3LootEntryDefinition
{
    public M3LootEntryDefinition(
        string entryId,
        string itemId,
        M3LootEntryKind kind,
        int quantity,
        int nominalValueCopper)
    {
        EntryId = entryId;
        ItemId = itemId;
        Kind = kind;
        Quantity = quantity;
        NominalValueCopper = nominalValueCopper;
    }

    public string EntryId { get; }

    public string ItemId { get; }

    public M3LootEntryKind Kind { get; }

    public int Quantity { get; }

    public int NominalValueCopper { get; }
}

public readonly struct M3VendorBuyOfferDefinition
{
    public M3VendorBuyOfferDefinition(
        string offerId,
        string itemId,
        int priceCopper,
        int carriedSlotCost)
    {
        OfferId = offerId;
        ItemId = itemId;
        PriceCopper = priceCopper;
        CarriedSlotCost = carriedSlotCost;
    }

    public string OfferId { get; }

    public string ItemId { get; }

    public int PriceCopper { get; }

    public int CarriedSlotCost { get; }
}

public readonly struct M3LootResolution
{
    public M3LootResolution(string lootTableId, IReadOnlyList<M3LootEntryDefinition> entries)
    {
        LootTableId = lootTableId;
        Entries = entries;
    }

    public string LootTableId { get; }

    public IReadOnlyList<M3LootEntryDefinition> Entries { get; }
}

public readonly struct M3VendorSaleResult
{
    public M3VendorSaleResult(
        string vendorId,
        string soldItemId,
        int quantity,
        int creditedCopper,
        int carriedCurrencyCopper)
    {
        VendorId = vendorId;
        SoldItemId = soldItemId;
        Quantity = quantity;
        CreditedCopper = creditedCopper;
        CarriedCurrencyCopper = carriedCurrencyCopper;
    }

    public string VendorId { get; }

    public string SoldItemId { get; }

    public int Quantity { get; }

    public int CreditedCopper { get; }

    public int CarriedCurrencyCopper { get; }
}

public readonly struct M3VendorPurchaseResult
{
    public M3VendorPurchaseResult(
        string vendorId,
        string purchasedItemId,
        int quantity,
        int debitedCopper,
        int carriedCurrencyCopper)
    {
        VendorId = vendorId;
        PurchasedItemId = purchasedItemId;
        Quantity = quantity;
        DebitedCopper = debitedCopper;
        CarriedCurrencyCopper = carriedCurrencyCopper;
    }

    public string VendorId { get; }

    public string PurchasedItemId { get; }

    public int Quantity { get; }

    public int DebitedCopper { get; }

    public int CarriedCurrencyCopper { get; }
}

public sealed class M3LootTableFixedProfileVendorData
{
    public const string DefaultLootTableId = "M3_ObjectiveNpcLoot_T1";
    public const string DefaultVendorId = "M3_CourtVendor_T1";
    public const string VendorObjectName = "M3_CourtVendor";
    public const string RelicEntryId = "m3.court_marked_relic.t1";
    public const string SalvageEntryId = "m3.grave_dust_salvage.t1";
    public const string SalvageItemId = "GraveDust_Salvage_T1";
    public const string FixedBuyOfferId = "m3.court_vendor.votive_candle.t1";
    public const string FixedBuyOfferItemId = "VotiveCandle_T1";
    public const double DefaultSalvageSellMultiplier = 0.15d;

#if UNITY_5_3_OR_NEWER
    private static readonly JsonSerializerSettings JsonSettings = CreateSettings();
#else
    private static readonly JsonSerializerOptions JsonOptions = CreateOptions();
#endif

    private readonly ReadOnlyCollection<M3LootEntryDefinition> _lootEntries;
    private readonly ReadOnlyCollection<M3VendorBuyOfferDefinition> _vendorBuyOffers;

    public M3LootTableFixedProfileVendorData(
        string lootTableId,
        string vendorId,
        double salvageSellMultiplier,
        int maxCarriedItemSlots,
        IReadOnlyList<M3LootEntryDefinition> lootEntries,
        IReadOnlyList<M3VendorBuyOfferDefinition> vendorBuyOffers)
    {
        LootTableId = lootTableId;
        VendorId = vendorId;
        SalvageSellMultiplier = salvageSellMultiplier;
        MaxCarriedItemSlots = maxCarriedItemSlots;
        _lootEntries = new ReadOnlyCollection<M3LootEntryDefinition>(new List<M3LootEntryDefinition>(lootEntries));
        _vendorBuyOffers = new ReadOnlyCollection<M3VendorBuyOfferDefinition>(new List<M3VendorBuyOfferDefinition>(vendorBuyOffers));
    }

    public string LootTableId { get; }

    public string VendorId { get; }

    public double SalvageSellMultiplier { get; }

    public int MaxCarriedItemSlots { get; }

    public IReadOnlyList<M3LootEntryDefinition> LootEntries => _lootEntries;

    public IReadOnlyList<M3VendorBuyOfferDefinition> VendorBuyOffers => _vendorBuyOffers;

    public bool UsesDynamicPricing => false;

    public bool UsesStockSimulation => false;

    public bool UsesReputationDiscount => false;

    public bool UsesLimitedTimeRotation => false;

    public bool UsesTokenBuying => false;

    public bool UsesFactionRankGoods => false;

    public bool UsesArbitrageHook => false;

    public bool MakesCoinFaucetProjectionClaim => false;

    public bool PersistsCurrencyAtRest => false;

    /// <summary>
    /// Creates the documented missing-file fallback. Runtime paths must prefer
    /// LoadFromFile so authored JSON remains the primary data authority.
    /// </summary>
    public static M3LootTableFixedProfileVendorData CreateAuthoredM3Default()
    {
        return new M3LootTableFixedProfileVendorData(
            DefaultLootTableId,
            DefaultVendorId,
            DefaultSalvageSellMultiplier,
            maxCarriedItemSlots: 4,
            new[]
            {
                new M3LootEntryDefinition(
                    RelicEntryId,
                    M3ObjectiveStateRelicHandInSession.RelicItemId,
                    M3LootEntryKind.ObjectiveRelic,
                    quantity: 1,
                    nominalValueCopper: 0),
                new M3LootEntryDefinition(
                    SalvageEntryId,
                    SalvageItemId,
                    M3LootEntryKind.Salvage,
                    quantity: 1,
                    nominalValueCopper: 50)
            },
            new[]
            {
                new M3VendorBuyOfferDefinition(
                    FixedBuyOfferId,
                    FixedBuyOfferItemId,
                    priceCopper: 3,
                    carriedSlotCost: 1)
            });
    }

    public static M3LootTableFixedProfileVendorData LoadFromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("M3 loot/vendor authored data path is required.", nameof(filePath));
        }

        return LoadFromJson(File.ReadAllText(filePath));
    }

    public static M3LootTableFixedProfileVendorData LoadFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidDataException("M3 loot/vendor authored data JSON is empty.");
        }

        AuthoredDataFile? dto;
        try
        {
#if UNITY_5_3_OR_NEWER
            dto = JsonConvert.DeserializeObject<AuthoredDataFile>(json, JsonSettings);
#else
            dto = JsonSerializer.Deserialize<AuthoredDataFile>(json, JsonOptions);
#endif
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("M3 loot/vendor authored data JSON could not be parsed.", ex);
        }

        if (dto is null)
        {
            throw new InvalidDataException("M3 loot/vendor authored data JSON did not produce a data file.");
        }

        if (dto.LootEntries is null)
        {
            throw new InvalidDataException("M3 loot/vendor authored data is missing lootEntries.");
        }

        if (dto.VendorBuyOffers is null)
        {
            throw new InvalidDataException("M3 loot/vendor authored data is missing vendorBuyOffers.");
        }

        var lootEntries = new List<M3LootEntryDefinition>();
        foreach (var entry in dto.LootEntries)
        {
            lootEntries.Add(new M3LootEntryDefinition(
                entry.EntryId ?? string.Empty,
                entry.ItemId ?? string.Empty,
                Enum.Parse<M3LootEntryKind>(entry.Kind ?? string.Empty, ignoreCase: false),
                entry.Quantity,
                entry.NominalValueCopper));
        }

        var vendorBuyOffers = new List<M3VendorBuyOfferDefinition>();
        foreach (var offer in dto.VendorBuyOffers)
        {
            vendorBuyOffers.Add(new M3VendorBuyOfferDefinition(
                offer.OfferId ?? string.Empty,
                offer.ItemId ?? string.Empty,
                offer.PriceCopper,
                offer.CarriedSlotCost));
        }

        return new M3LootTableFixedProfileVendorData(
            dto.TableId ?? string.Empty,
            dto.VendorId ?? string.Empty,
            dto.SalvageSellMultiplier,
            dto.MaxCarriedItemSlots,
            lootEntries,
            vendorBuyOffers);
    }

    public int CalculateVendorSellCopper(int nominalValueCopper)
    {
        return Math.Max(1, (int)Math.Floor(nominalValueCopper * SalvageSellMultiplier));
    }

    public bool TryValidateForM3(out IReadOnlyList<string> errors)
    {
        var validationErrors = new List<string>();

        if (!string.Equals(LootTableId, DefaultLootTableId, StringComparison.Ordinal))
        {
            validationErrors.Add("M3 loot table id does not match the authored default.");
        }

        if (!string.Equals(VendorId, DefaultVendorId, StringComparison.Ordinal))
        {
            validationErrors.Add("M3 vendor id does not match the authored default.");
        }

        if (Math.Abs(SalvageSellMultiplier - DefaultSalvageSellMultiplier) > 0.0001d)
        {
            validationErrors.Add("M3 salvage sell multiplier must be 0.15.");
        }

        if (MaxCarriedItemSlots <= 0)
        {
            validationErrors.Add("M3 carried item slot capacity must be positive.");
        }

        var relicCount = 0;
        var salvageCount = 0;
        foreach (var entry in LootEntries)
        {
            ValidateEntry(entry, validationErrors);

            if (entry.Kind == M3LootEntryKind.ObjectiveRelic &&
                string.Equals(entry.ItemId, M3ObjectiveStateRelicHandInSession.RelicItemId, StringComparison.Ordinal))
            {
                relicCount++;
            }

            if (entry.Kind == M3LootEntryKind.Salvage &&
                string.Equals(entry.ItemId, SalvageItemId, StringComparison.Ordinal))
            {
                salvageCount++;
            }
        }

        if (relicCount != 1)
        {
            validationErrors.Add("M3 loot table must contain exactly one CourtMarkedRelic_T1 objective relic row.");
        }

        if (salvageCount != 1)
        {
            validationErrors.Add("M3 loot table must contain exactly one GraveDust_Salvage_T1 salvage row.");
        }

        foreach (var offer in VendorBuyOffers)
        {
            if (string.IsNullOrWhiteSpace(offer.OfferId) ||
                string.IsNullOrWhiteSpace(offer.ItemId) ||
                offer.PriceCopper <= 0 ||
                offer.CarriedSlotCost <= 0)
            {
                validationErrors.Add("M3 vendor buy offers must use stable ids, positive prices, and positive carried-slot costs.");
            }
        }

        if (UsesDynamicPricing ||
            UsesStockSimulation ||
            UsesReputationDiscount ||
            UsesLimitedTimeRotation ||
            UsesTokenBuying ||
            UsesFactionRankGoods ||
            UsesArbitrageHook ||
            MakesCoinFaucetProjectionClaim ||
            PersistsCurrencyAtRest)
        {
            validationErrors.Add("M3 vendor profile must remain fixed, session-local, and mechanism-only.");
        }

        errors = validationErrors;
        return validationErrors.Count == 0;
    }

    public bool TryFindEntryByItemId(string itemId, out M3LootEntryDefinition entry)
    {
        foreach (var candidate in LootEntries)
        {
            if (string.Equals(candidate.ItemId, itemId, StringComparison.Ordinal))
            {
                entry = candidate;
                return true;
            }
        }

        entry = default;
        return false;
    }

    public bool TryFindBuyOffer(string offerId, out M3VendorBuyOfferDefinition offer)
    {
        foreach (var candidate in VendorBuyOffers)
        {
            if (string.Equals(candidate.OfferId, offerId, StringComparison.Ordinal))
            {
                offer = candidate;
                return true;
            }
        }

        offer = default;
        return false;
    }

    private static void ValidateEntry(M3LootEntryDefinition entry, List<string> validationErrors)
    {
        if (string.IsNullOrWhiteSpace(entry.EntryId))
        {
            validationErrors.Add("M3 loot entries must use stable authored entry ids.");
        }

        if (string.IsNullOrWhiteSpace(entry.ItemId))
        {
            validationErrors.Add("M3 loot entries must use stable authored item ids.");
        }

        if (entry.Quantity <= 0)
        {
            validationErrors.Add($"M3 loot entry {entry.EntryId} must have a positive quantity.");
        }

        if (entry.Kind == M3LootEntryKind.Salvage && entry.NominalValueCopper <= 0)
        {
            validationErrors.Add($"M3 salvage entry {entry.EntryId} must have a positive nominal copper value.");
        }

    }

#if UNITY_5_3_OR_NEWER
    private static JsonSerializerSettings CreateSettings()
    {
        return new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };
    }
#else
    private static JsonSerializerOptions CreateOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Disallow,
            AllowTrailingCommas = false
        };
    }
#endif

    private sealed class AuthoredDataFile
    {
        public string? TableId { get; set; }

        public string? VendorId { get; set; }

        public double SalvageSellMultiplier { get; set; }

        public int MaxCarriedItemSlots { get; set; }

        public List<AuthoredLootEntry>? LootEntries { get; set; }

        public List<AuthoredBuyOffer>? VendorBuyOffers { get; set; }
    }

    private sealed class AuthoredLootEntry
    {
        public string? EntryId { get; set; }

        public string? ItemId { get; set; }

        public string? Kind { get; set; }

        public int Quantity { get; set; }

        public int NominalValueCopper { get; set; }
    }

    private sealed class AuthoredBuyOffer
    {
        public string? OfferId { get; set; }

        public string? ItemId { get; set; }

        public int PriceCopper { get; set; }

        public int CarriedSlotCost { get; set; }
    }
}

public sealed class M3LootTableFixedProfileVendorSession
{
    private readonly M3LootTableFixedProfileVendorData _data;
    private readonly Dictionary<string, int> _carriedItems = new(StringComparer.Ordinal);

    public M3LootTableFixedProfileVendorSession(M3LootTableFixedProfileVendorData data, int carriedCurrencyCopper = 0)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        if (!_data.TryValidateForM3(out var errors))
        {
            throw new ArgumentException(string.Join("; ", errors), nameof(data));
        }

        if (carriedCurrencyCopper < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(carriedCurrencyCopper), "M3 carried currency cannot be negative.");
        }

        CarriedCurrencyCopper = carriedCurrencyCopper;
    }

    public string LootTableId => _data.LootTableId;

    public string VendorId => _data.VendorId;

    public int CarriedCurrencyCopper { get; private set; }

    public bool SessionLocalOnly => true;

    public bool UsesProgressionSeedAsLootRng => false;

    public bool HasDynamicPricingHook => _data.UsesDynamicPricing;

    public bool HasStockSimulationHook => _data.UsesStockSimulation;

    public bool HasReputationDiscountHook => _data.UsesReputationDiscount;

    public bool HasLimitedTimeRotationHook => _data.UsesLimitedTimeRotation;

    public bool HasTokenBuyingHook => _data.UsesTokenBuying;

    public bool HasFactionRankGoodsHook => _data.UsesFactionRankGoods;

    public bool HasArbitrageHook => _data.UsesArbitrageHook;

    public bool MakesCoinFaucetProjectionClaim => _data.MakesCoinFaucetProjectionClaim;

    public bool PersistsCurrencyAtRest => _data.PersistsCurrencyAtRest;

    public int CarriedItemSlotsUsed
    {
        get
        {
            var count = 0;
            foreach (var item in _carriedItems)
            {
                count += item.Value;
            }

            return count;
        }
    }

    public IReadOnlyDictionary<string, int> CarriedItems => _carriedItems;

    public bool HasCarriedItem(string itemId)
    {
        return _carriedItems.TryGetValue(itemId, out var quantity) && quantity > 0;
    }

    public int GetCarriedQuantity(string itemId)
    {
        return _carriedItems.TryGetValue(itemId, out var quantity) ? quantity : 0;
    }

    public bool TryResolveDefaultLoot(out M3LootResolution resolution, out string rejectionReason)
    {
        resolution = default;
        rejectionReason = string.Empty;

        var slotsRequired = 0;
        foreach (var entry in _data.LootEntries)
        {
            slotsRequired += entry.Quantity;
        }

        if (!HasItemCapacity(slotsRequired))
        {
            rejectionReason = "M3 loot resolution would exceed session-local carried capacity.";
            return false;
        }

        foreach (var entry in _data.LootEntries)
        {
            AddCarriedItem(entry.ItemId, entry.Quantity);
        }

        resolution = new M3LootResolution(_data.LootTableId, _data.LootEntries);
        return true;
    }

    public bool TrySellSalvage(
        string itemId,
        int quantity,
        out M3VendorSaleResult result,
        out string rejectionReason)
    {
        result = default;
        rejectionReason = string.Empty;

        if (quantity <= 0)
        {
            rejectionReason = "M3 salvage sale quantity must be positive.";
            return false;
        }

        if (!_data.TryFindEntryByItemId(itemId, out var entry) || entry.Kind != M3LootEntryKind.Salvage)
        {
            rejectionReason = "M3 fixed-profile vendor only buys authored Salvage rows.";
            return false;
        }

        if (GetCarriedQuantity(itemId) < quantity)
        {
            rejectionReason = "M3 fixed-profile vendor cannot buy salvage the player is not carrying.";
            return false;
        }

        var creditedCopper = _data.CalculateVendorSellCopper(entry.NominalValueCopper) * quantity;
        RemoveCarriedItem(itemId, quantity);
        CarriedCurrencyCopper += creditedCopper;
        result = new M3VendorSaleResult(_data.VendorId, itemId, quantity, creditedCopper, CarriedCurrencyCopper);
        return true;
    }

    public bool TryPurchaseFixedVendorGood(
        string offerId,
        int quantity,
        out M3VendorPurchaseResult result,
        out string rejectionReason)
    {
        result = default;
        rejectionReason = string.Empty;

        if (quantity <= 0)
        {
            rejectionReason = "M3 vendor purchase quantity must be positive.";
            return false;
        }

        if (!_data.TryFindBuyOffer(offerId, out var offer))
        {
            rejectionReason = "M3 fixed-profile vendor offer was not authored.";
            return false;
        }

        var carriedSlotsRequired = checked(offer.CarriedSlotCost * quantity);
        if (!HasItemCapacity(carriedSlotsRequired))
        {
            rejectionReason = "M3 fixed-profile vendor prevalidated carried capacity before currency debit.";
            return false;
        }

        var priceCopper = checked(offer.PriceCopper * quantity);
        if (CarriedCurrencyCopper < priceCopper)
        {
            rejectionReason = "M3 fixed-profile vendor purchase requires more carried copper.";
            return false;
        }

        CarriedCurrencyCopper -= priceCopper;
        AddCarriedItem(offer.ItemId, quantity);
        result = new M3VendorPurchaseResult(_data.VendorId, offer.ItemId, quantity, priceCopper, CarriedCurrencyCopper);
        return true;
    }

    private bool HasItemCapacity(int slotsRequired)
    {
        return CarriedItemSlotsUsed + slotsRequired <= _data.MaxCarriedItemSlots;
    }

    private void AddCarriedItem(string itemId, int quantity)
    {
        _carriedItems.TryGetValue(itemId, out var existingQuantity);
        _carriedItems[itemId] = existingQuantity + quantity;
    }

    private void RemoveCarriedItem(string itemId, int quantity)
    {
        var remaining = GetCarriedQuantity(itemId) - quantity;
        if (remaining > 0)
        {
            _carriedItems[itemId] = remaining;
        }
        else
        {
            _carriedItems.Remove(itemId);
        }
    }
}
