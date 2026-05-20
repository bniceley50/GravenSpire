#nullable enable

using System.IO;
using Gravenspire.Gameplay.Npc.M3Objective;
using NUnit.Framework;

namespace Gravenspire.Tests.Unit.Gameplay.Npc;

public sealed class M3LootTableFixedProfileVendorTest
{
    private const string AuthoredDataRelativePath = "data/first-district/m3-objective-npc-loot.json";

    [Test]
    public void test_authored_loot_table_resolves_relic_and_salvage_rows()
    {
        var data = LoadAuthoredData();
        var session = new M3LootTableFixedProfileVendorSession(data);

        var resolved = session.TryResolveDefaultLoot(out var resolution, out var rejectionReason);

        Assert.That(resolved, Is.True, rejectionReason);
        Assert.That(resolution.LootTableId, Is.EqualTo(M3LootTableFixedProfileVendorData.DefaultLootTableId));
        Assert.That(resolution.Entries, Has.Count.EqualTo(2));
        Assert.That(session.HasCarriedItem(M3ObjectiveStateRelicHandInSession.RelicItemId), Is.True);
        Assert.That(session.HasCarriedItem(M3LootTableFixedProfileVendorData.SalvageItemId), Is.True);
    }

    [Test]
    public void test_loot_data_uses_stable_authored_ids_and_no_combat_runtime_fields()
    {
        var json = File.ReadAllText(AuthoredDataPath());
        var data = LoadAuthoredData();

        Assert.That(data.TryValidateForM3(out var errors), Is.True, string.Join("; ", errors));
        Assert.That(data.LootEntries, Has.All.Matches<M3LootEntryDefinition>(entry =>
            !string.IsNullOrWhiteSpace(entry.EntryId) &&
            !string.IsNullOrWhiteSpace(entry.ItemId)));

        Assert.That(json, Does.Not.Contain("combat_actor_id"));
        Assert.That(json, Does.Not.Contain("runtime_actor_handle"));
        Assert.That(json, Does.Not.Contain("threat_table"));
        Assert.That(json, Does.Not.Contain("damage_roll"));
        Assert.That(json, Does.Not.Contain("current_health"));
        Assert.That(json, Does.Not.Contain("current_mana"));
        Assert.That(json, Does.Not.Contain("current_endurance"));
    }

    [Test]
    public void test_loot_resolution_does_not_reuse_kill_weight_seed()
    {
        var json = File.ReadAllText(AuthoredDataPath());
        var data = LoadAuthoredData();
        var session = new M3LootTableFixedProfileVendorSession(data);

        Assert.That(json, Does.Not.Contain("kill_weight_seed"));
        Assert.That(session.UsesProgressionSeedAsLootRng, Is.False);
    }

    [Test]
    public void test_default_table_contains_no_currency_container_entry()
    {
        var json = File.ReadAllText(AuthoredDataPath());
        var data = LoadAuthoredData();

        Assert.That(json, Does.Not.Contain("CurrencyContainer"));
        Assert.That(data.MakesCoinFaucetProjectionClaim, Is.False);
    }

    [Test]
    public void test_fixed_profile_vendor_applies_f4_salvage_formula()
    {
        var session = ResolvedLootSession();

        var sold = session.TrySellSalvage(
            M3LootTableFixedProfileVendorData.SalvageItemId,
            quantity: 1,
            out var result,
            out var rejectionReason);

        Assert.That(sold, Is.True, rejectionReason);
        Assert.That(result.CreditedCopper, Is.EqualTo(7));
        Assert.That(result.CarriedCurrencyCopper, Is.EqualTo(7));
        Assert.That(session.GetCarriedQuantity(M3LootTableFixedProfileVendorData.SalvageItemId), Is.EqualTo(0));
    }

    [Test]
    public void test_vendor_prevalidates_capacity_before_any_currency_debit()
    {
        var data = LoadAuthoredData();
        var session = new M3LootTableFixedProfileVendorSession(data, carriedCurrencyCopper: 7);
        Assert.That(session.TryResolveDefaultLoot(out _, out _), Is.True);
        Assert.That(session.TrySellSalvage(M3LootTableFixedProfileVendorData.SalvageItemId, 1, out _, out _), Is.True);
        Assert.That(session.TryPurchaseFixedVendorGood(
            M3LootTableFixedProfileVendorData.FixedBuyOfferId,
            quantity: 4,
            out _,
            out var rejectionReason), Is.False);

        Assert.That(rejectionReason, Does.Contain("prevalidated carried capacity"));
        Assert.That(session.CarriedCurrencyCopper, Is.EqualTo(14));
        Assert.That(session.GetCarriedQuantity(M3LootTableFixedProfileVendorData.FixedBuyOfferItemId), Is.EqualTo(0));
    }

    [Test]
    public void test_vendor_profile_exposes_no_dynamic_economy_hooks()
    {
        var data = LoadAuthoredData();
        var session = new M3LootTableFixedProfileVendorSession(data);

        Assert.That(session.HasDynamicPricingHook, Is.False);
        Assert.That(session.HasStockSimulationHook, Is.False);
        Assert.That(session.HasReputationDiscountHook, Is.False);
        Assert.That(session.HasLimitedTimeRotationHook, Is.False);
        Assert.That(session.HasTokenBuyingHook, Is.False);
        Assert.That(session.HasFactionRankGoodsHook, Is.False);
        Assert.That(session.HasArbitrageHook, Is.False);
    }

    [Test]
    public void test_vendor_transactions_are_synchronous_and_atomic()
    {
        var session = ResolvedLootSession();
        var currencyBeforeFailedSale = session.CarriedCurrencyCopper;
        var relicBeforeFailedSale = session.GetCarriedQuantity(M3ObjectiveStateRelicHandInSession.RelicItemId);

        var failedSale = session.TrySellSalvage(
            M3ObjectiveStateRelicHandInSession.RelicItemId,
            quantity: 1,
            out _,
            out _);

        Assert.That(failedSale, Is.False);
        Assert.That(session.CarriedCurrencyCopper, Is.EqualTo(currencyBeforeFailedSale));
        Assert.That(session.GetCarriedQuantity(M3ObjectiveStateRelicHandInSession.RelicItemId), Is.EqualTo(relicBeforeFailedSale));

        Assert.That(session.TrySellSalvage(M3LootTableFixedProfileVendorData.SalvageItemId, 1, out _, out _), Is.True);
        Assert.That(session.TryPurchaseFixedVendorGood(
            M3LootTableFixedProfileVendorData.FixedBuyOfferId,
            quantity: 1,
            out var purchase,
            out var rejectionReason), Is.True, rejectionReason);
        Assert.That(purchase.DebitedCopper, Is.EqualTo(3));
        Assert.That(session.CarriedCurrencyCopper, Is.EqualTo(4));
        Assert.That(session.GetCarriedQuantity(M3LootTableFixedProfileVendorData.FixedBuyOfferItemId), Is.EqualTo(1));
    }

    [Test]
    public void test_vendor_state_is_session_local_and_makes_no_tuned_economy_or_persistence_claim()
    {
        var data = LoadAuthoredData();
        var session = new M3LootTableFixedProfileVendorSession(data);

        Assert.That(session.SessionLocalOnly, Is.True);
        Assert.That(session.MakesCoinFaucetProjectionClaim, Is.False);
        Assert.That(session.PersistsCurrencyAtRest, Is.False);
    }

    private static M3LootTableFixedProfileVendorSession ResolvedLootSession()
    {
        var session = new M3LootTableFixedProfileVendorSession(LoadAuthoredData());
        Assert.That(session.TryResolveDefaultLoot(out _, out var rejectionReason), Is.True, rejectionReason);
        return session;
    }

    private static M3LootTableFixedProfileVendorData LoadAuthoredData()
    {
        return M3LootTableFixedProfileVendorData.LoadFromFile(AuthoredDataPath());
    }

    private static string AuthoredDataPath()
    {
        return Path.Combine(TestContext.CurrentContext.TestDirectory, AuthoredDataRelativePath);
    }

}
