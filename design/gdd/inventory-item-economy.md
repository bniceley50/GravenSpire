# Inventory & Item Economy

> **Status**: In Design
> **Author**: Codex (session with brian, 2026-04-26)
> **Last Updated**: 2026-04-26
> **Implements Pillar**: Primary - **P3 Reputation Is The Progression**. Supports - **P5 Stakes Are Honest**.

## Locked Inputs

These inputs are authoritative for the Inventory & Item Economy GDD. This GDD will reference them rather than redesigning them. If a later section appears to conflict with this block, the later section is wrong.

1. **T1 scope lock** - Per [DECISIONS.md](../../DECISIONS.md) D003, Tier 1 is offline single-player with local saves only. Inventory & Item Economy must not imply networking, account identity, server authority, auction houses, player-to-player trade, live economy services, PvP item transfer, or multi-character inventory.

2. **Save/Load contract** - [save-load-persistence.md](save-load-persistence.md) persists equipped inventory, carried inventory, currency, and faction tokens as Player State / downstream Inventory state. Save/Load owns serialization, HMAC/versioning, load rejection, first-run path, and hydration sequencing. Inventory & Item Economy owns item schema, currency/token schema, item validation, item materialization, and the current Inventory state exposed to Save/Load.

3. **Character Creation contract** - [character-creation.md](character-creation.md) emits `starting_equipment_template_id = ClericStartingEquipment_T1` and `carried_inventory = []` in the validated `InitialCharacterRecord`. Character Creation does not create item instances, define equipment slots, define stack rules, define item stats, or repair missing authored equipment templates. Inventory & Item Economy owns those rules.

4. **Progression and Combat boundaries** - [character-progression.md](character-progression.md) does not grant loot, define item requirements, define vendor/drop availability, or persist item data. [combat-core.md](combat-core.md) owns combat resolution and emits future loot eligibility context only; Inventory & Item Economy owns loot, drops, equipment legality, item stat schema, and currency/token outcomes.

5. **Pillar constraint** - Per [game-concept.md](game-concept.md), Gravenspire is not a gear-treadmill MMO. Gear plateaus early; reputation and relationships carry the long-term progression load. Inventory & Item Economy must support faction consequence, useful preparation, and honest persistence without becoming the primary vertical power ladder.

## Overview

**Pillar frame:** Primary - **P3 Reputation Is The Progression**: gear plateaus early, and items must not replace reputation as the long-term ladder. Supports - **P5 Stakes Are Honest**: what the player carries, spends, loses, recovers, and saves must remain true across the session boundary.

Inventory & Item Economy is the T1 system that turns item ownership into trustworthy, legible consequence without making gear the game's main progression ladder. It owns the schema and rules for materialized item records, equipped gear, carried inventory, currency fields, faction-token records, slot legality, stack behavior, item validation, and the Inventory-owned save payload exposed to Save/Load. Players feel it directly through what they carry, equip, loot, spend, and keep across sessions; they should understand that objects matter, but not feel pushed into a gear-score chase. Character Creation supplies only the `ClericStartingEquipment_T1` template reference and an empty carried inventory; Inventory materializes and validates the actual item state. Combat may later provide loot eligibility context. Save/Load Rule 1 persists Inventory state (items, currency, faction tokens), and Inventory declares its own ADR-0002 save-stability barrier if transient unsafe states are scoped during Detailed Rules; Inventory owns item schema, hydration validation, and rejection of invalid persisted payloads.

At T1, this system does not own crafting, economy simulation, auction houses, player trade, vendor reputation, faction reputation meaning, item degradation, repair loops, live pricing, LLM item descriptions, networking, accounts, PvP transfer, or multi-character storage. Currency may exist as a persisted schema field, but detailed money sources and sinks remain limited to what this GDD explicitly approves. Faction tokens may be represented in Inventory-owned schema for persistence and possession, but their social meaning, rank thresholds, and reputation effects belong to Faction Reputation. The design goal is preparation and consequence: equipment plateaus early, carried goods have constraints, and items support faction play without replacing reputation as the long-term ladder.

## Player Fantasy

Inventory & Item Economy should make the player feel prepared, constrained, and accountable. A carried object is not a prize shower; it is a commitment. The player should look at a small bag, a plain Cleric kit, a faction token, and a modest amount of currency and understand: this is what I chose to bring, this is what I spent, this is what I risk carrying back, and the record will remember it. Carrying matters because Death & Corpse Recovery means losing the run can mean losing the load: what you brought is what you stand to lose, and that knowledge is the felt stake.

The fantasy is quiet usefulness. Gear should feel like field equipment that helps the Cleric survive the haunt, not like the true measure of the character. A better weapon, charm, token, or consumable may change a plan, open a faction-facing option, or improve a dangerous run, but it should not replace reputation, relationship, and local consequence as the reason to keep playing. Gravenspire items earn attention through scarcity, context, and persistence, not through rarity color, loot explosions, or gear-score pressure.

### Anchor Moment

The player returns from the haunt with limited space, a few useful finds, spent currency, and one faction-marked object whose meaning is not yet fully clear: the kind of small marked thing that turns into a question for the inn rumor table next session. They must decide what to keep, what to sell, what to equip, and what to risk carrying next time. The moment works because the inventory is small enough to matter, persistent enough to trust, and restrained enough that the item is a consequence of play rather than a replacement for play.

### Anti-fantasy - what the player should NOT feel

- **"My real level is my gear score"** - no item-power treadmill, raid-tier chase, or gear replacing reputation as progression.
- **"Loot is confetti"** - no constant rarity bursts, color-tier dopamine loop, or celebratory reward shower.
- **"The bag is infinite"** - no frictionless hoarding that makes carried choices meaningless.
- **"The economy is the game"** - no auction-house play, market speculation, live pricing, or vendor-arbitrage loop at T1.
- **"Items secretly solve faction identity"** - faction tokens may be possessed and persisted here, but their social meaning belongs to Faction Reputation.
- **"Items replace identity"** - no signature weapon, mythic artifact, or named gear that becomes the character's defining trait; identity belongs to faction standing, named NPC recognition, and what the player has done.

### Reference Register

Classic EQ restraint rather than ARPG loot spectacle: ordinary equipment, memorable camp drops, coin that matters because recovery and travel matter, and items that feel tied to place. The tonal register is ledger, satchel, parish chest, field kit, and marked token: useful, specific, and persistent.

## Detailed Rules

1. **Inventory owns item truth.** Inventory owns `InventorySaveState`, item definitions, materialized item instances, equipped slots, carried slots, stack records, currency, and faction-token possession. Save/Load owns serialization and active-record context; Inventory does not duplicate `local_character_id` inside item records.

2. **T1 first-save materialization is required.** Inventory declares ADR-0004 `InventoryFirstSaveMaterializer`. It consumes `starting_class_id = Cleric`, `starting_equipment_template_id = ClericStartingEquipment_T1`, `carried_inventory = []`, and active `local_character_id` context, then produces valid starting `InventorySaveState` before first save.

3. **Starter failures are loud.** Missing starter template, illegal slot assignment, illegal class, invalid item definition, or non-empty carried seed returns `FirstSaveMaterializationFailed`; Save/Load writes no bytes and does not initialize the record.

4. **Initialized saves are never repaired from creation defaults.** A later load missing Inventory state, starter item records, or required equipped-slot records fails hydration. Inventory must not synthesize fallback gear, delete invalid records, clamp values, or re-run first-save materialization.

5. **Persisted schema is narrow.** Inventory save state may contain `inventory_schema_version`, equipped slot bindings, carried slot contents, item instance records, stack records, `carried_currency_copper`, faction-token stack records, and an optional state revision for tests. It must not persist derived stat totals, rarity colors, runtime handles, scene refs, loot containers, drag state, vendor UI state, combat actor ids, or cached formulas.

6. **T1 item categories are small.** Legal T1 categories are `Equipment`, `Consumable`, `Salvage`, `FactionToken`, and `CurrencyContainer`. Crafting materials, key/progression items, cosmetics, durability parts, generated lore items, and set/affix items are deferred unless another approved GDD reverse-lists them.

7. **Equipment slots are explicit.** T1 equipment slots are `MainHand`, `OffHand`, `Body`, and `Charm`. Equippable items declare slot mask, Cleric eligibility, and any exclusive-slot group. Equipment is non-stackable; stackable items cannot be equipped.

8. **Equip and unequip are atomic.** If replacing gear would require moving the old item into carried inventory, Inventory validates the destination before mutation. No partial equip, hidden overflow, ground spill, mail, bank, or fallback deletion exists at T1.

9. **Carry pressure uses slots plus weight.** T1 has fixed carried slots and a hard `weight_units` cap. Pickup fails if it would exceed either cap. Weight never grants movement penalties in T1; it is a pickup/carry legality rule only. Negative, hidden, or runtime-calculated weight is illegal.

10. **Stacks merge only by exact key.** Stackable items merge only when item definition, faction marker fields, bind state, condition fields, and any token metadata match exactly. Stack quantity must be an integer from `1` through that item's `max_stack`.

11. **Currency is not an item stack.** Inventory stores currency as integer `carried_currency_copper`. UI may format denominations later, but save state does not store floats or separate coin piles. `CurrencyContainer` items per Rule 6 are item records in transit only: when consumed, they credit `carried_currency_copper` and are destroyed. They do not represent currency at rest; rest-state currency is the integer balance only. Negative currency, overflow, unknown denominations, and malformed balances reject hydration.

12. **T1 money loops are bounded.** Ordinary mob kills do not produce currency in T1. Coin enters only through (a) vendor sale of `Salvage` items, (b) authored `CurrencyContainer` pickups from world placements or approved loot tables, or (c) explicit fixture rewards from an approved owning GDD. The Combat-to-Inventory loot eligibility hook in Rule 15 may produce a `CurrencyContainer` item but never directly increments the currency balance.

13. **Vendors are fixed-profile only.** T1 may have one CityHub vendor profile with authored buy/sell tables. Prices are constants. Vendors have no dynamic pricing, stock simulation, reputation discounts, faction-rank goods, limited-time rotation, token buying, or arbitrage loop.

14. **Faction tokens are possession, not reputation.** Inventory may persist `token_def_id`, `faction_id`, `source_ref`, quantity, bind state, and deposit state. It never mutates reputation, rank, access, title, NPC trust, or faction meaning. Token turn-in is deferred until Faction Reputation owns the meaning.

15. **Combat does not own loot.** Inventory may consume future Combat loot context only through stable refs such as `defeated_source_ref`, `zoneId`, and optional `faction_id`; never `combat_actor_id`. XP `kill_weight_seed` must not be reused as loot RNG without an explicit approved contract.

16. **Gear plateaus early and does not modify T1 Combat stats.** T1 equipment bands are `Starting`, `Field`, and `Sidegrade`. No item level, rarity color, gear score, random affixes, upgrade ranks, sockets, set bonuses, or scaling stats. T1 starter equipment does not modify Combat stats; the player Cleric's combat capability is fully determined by ADR-0003 `CombatProgressionBaselineSnapshot` (level plus permanent max health/mana). `EquipmentCombatStatSnapshot_T1` is reserved as a future contract for a T2+ ADR introducing stat-bearing equipment, at which point Save/Load hydration order would become Character Progression -> Inventory -> Combat. T1 hydration order remains Character Progression -> Combat unchanged.

17. **Death and corpse rules are boundary-safe.** Inventory does not move, delete, or transfer items on death by itself. It may expose a future `CorpseInventorySnapshot(death_context_id)` containing equipped items, carried items, carried currency, and carried faction tokens. Death & Corpse Recovery owns when that snapshot is created, recovered, expired, or partially restored.

18. **No T1 Inventory save barrier yet.** All T1 Inventory mutations are synchronous and atomic. Therefore this GDD does not declare `InventorySaveBarrier`. If later rules add multi-frame loot pickup, vendor confirmation, faction-token turn-in, corpse transfer, async materialization, or ownership-changing drag/drop, this GDD must be amended to declare an ADR-0002 barrier.

19. **Hydration rejects invalid payloads.** Reject missing Inventory state, unknown item definitions, duplicate item instance ids, one instance in multiple locations, illegal equipped slots, class-illegal equipment, invalid stack quantities, currency underflow/overflow, unknown faction token definitions, persisted derived stats, runtime handles, scene refs, and malformed schema versions.

20. **Explicit non-goals.** T1 has no crafting, durability, repair, auction house, player trade, mail, account bank, shared stash, vendor reputation, dynamic pricing, faction-token reputation math, PvP transfer, companion inventory, LLM item text, rare affix tables, set bonuses, transmogs, cosmetics, or multi-character inventory.

## Formulas

[To be designed]

## Edge Cases

[To be designed]

## Dependencies

[To be designed]

## Tuning Knobs

[To be designed]

## Acceptance Criteria

[To be designed]
