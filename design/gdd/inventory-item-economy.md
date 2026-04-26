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

### Defaults

| Knob | Default | Safe Range |
|---|---:|---:|
| `carried_slot_cap` | `18` slots | `16-22` |
| `carried_weight_cap` | `60` weight_units | `45-75` |
| `starting_carried_currency_copper` | `0` | `0-9` |
| `salvage_sell_multiplier` | `0.15` | `0.10-0.20` |
| `max_t1_item_weight_units` | `25` | Fixed unless fixture-proven |

`weight_units` is an abstract integer scale. One `weight_unit` corresponds roughly to the carry burden of a coin pouch, a small dagger, or a held trinket. Concrete physical weight in pounds/kilograms and audio/animation weight cues are deferred to art/animation pipeline work; this GDD's weight model is pickup-legality only, not physics, stamina, or movement speed.

### F1: Carry Acceptance

The `can_accept_item` formula is defined as:

`can_accept_item = projected_slot_count <= carried_slot_cap AND projected_weight_units <= carried_weight_cap`

When the pickup merges into an existing compatible stack with capacity, `projected_slot_count = current_slot_count`; otherwise `projected_slot_count = current_slot_count + ceil(pickup_quantity / max_stack)`. Weight always increases regardless of merge: `projected_weight_units = current_weight_units + (unit_weight_units * pickup_quantity)`.

`deterministic_slot_order = ascending carried slot index 0..carried_slot_cap-1`. Stack-fill scans existing compatible stacks in this order before allocating an empty slot.

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `current_slot_count` | `CSC0` | int | `0-18` default | Occupied carried slots before the pickup/merge. |
| `pickup_quantity` | `PQ` | int | `1-max_stack` | Quantity being picked up. |
| `max_stack` | `MS` | int | `1-20` default | Category-specific maximum stack size. |
| `projected_slot_count` | `PSC` | int | `0-18` default | Occupied carried slots after the pickup/merge. |
| `carried_slot_cap` | `CSC` | int | `16-22` | Maximum carried slots. |
| `current_weight_units` | `CWU0` | int | `0-60` default | Carried weight before pickup. |
| `unit_weight_units` | `UWU` | int | `0-25` | Authored unit weight for the pickup item. |
| `projected_weight_units` | `PWU` | int | `0-60` default | Total carried weight after pickup/merge. |
| `carried_weight_cap` | `CWC` | int | `45-75` | Hard pickup legality cap. |

**Output Range:** boolean.

**Example:** With 17 occupied slots and 58 weight, a 1-slot, 2-weight pickup passes. A 1-slot, 3-weight pickup fails weight. A 3-quantity salvage pickup that merges into an existing compatible stack keeps `projected_slot_count = current_slot_count` while adding `3 * unit_weight_units` to projected weight.

### F2: Stack Weight

The `stack_weight_units` formula is defined as:

`stack_weight_units = unit_weight_units * quantity`

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `unit_weight_units` | `UWU` | int | `0-25` | Authored item weight; negative values are illegal. |
| `quantity` | `Q` | int | `1-max_stack` | Stack quantity. |

**Output Range:** `0-150` under default salvage stack cap; most normal stacks should remain below `60`.

**Example:** A standard salvage stack with `unit_weight_units = 4` and `quantity = 6` weighs `24` weight_units.

Most items should weigh `1-8` weight_units. Items at or near `max_t1_item_weight_units = 25` represent intentionally committal heavy gear, such as two-handed weapons or heavy armor; their inclusion in starter or routine loot loadouts must be deliberate and fixture-validated against haunt-run carry pressure.

Typical authoring bands:

| Category | Typical Weight |
|---|---:|
| Charm equipment | `1-3` |
| OffHand equipment | `4-8` |
| MainHand equipment | `8-14` |
| Body equipment | `14-22` |
| Consumable | `1-2` |
| Light Salvage | `1-3` |
| Standard Salvage | `3-6` |
| Heavy Salvage | `8-14` |
| FactionToken | `1` each |
| CurrencyContainer | `1-4` |

### F3: Stack Cap Lookup

The `max_stack` formula is defined as:

`max_stack = category_stack_cap[item_category]`

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `item_category` | `IC` | enum | `Equipment, Consumable, Salvage, FactionToken, CurrencyContainer` | T1 legal item category. |
| `category_stack_cap` | `CSC` | map | See table | Authored stack cap by category. |

Default stack caps:

| Category | Default `max_stack` | Safe Range |
|---|---:|---:|
| `Equipment` | `1` | Fixed |
| `Consumable` | `5` | `3-10` |
| `Salvage` | `6` | `4-10` |
| `FactionToken` | `20` | `10-30` |
| `CurrencyContainer` | `1` | `1-3`; default stays `1` |

**Output Range:** `1-20` under T1 defaults.

**Example:** A `FactionToken` stack can hold `20`; a `CurrencyContainer` stack holds `1` because it is currency in transit, not currency at rest.

### F4: Vendor Salvage Sale

The `vendor_sell_copper` formula is defined as:

`vendor_sell_copper = max(1, floor(nominal_value_copper * salvage_sell_multiplier))`

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `nominal_value_copper` | `NVC` | int | `5-250` | Authored salvage value band. |
| `salvage_sell_multiplier` | `SSM` | float | `0.10-0.20` | Fixed CityHub vendor sale multiplier. |

**Output Range:** `1-50` copper under normal T1 authored ranges and safe multiplier bounds.

**Example:** `nominal_value_copper = 50` and `salvage_sell_multiplier = 0.15` gives `7` copper.

Vendor buy-from-vendor prices (player purchasing) are authored constants in `T1_CityHubVendorBuyTable_T1`, not formula-derived. No price formula exists for the buy side at T1; future dynamic pricing requires a tier-transition decision.

Recommended salvage value bands:

| Source Band | Nominal Default Range | Sale at `0.15` |
|---|---:|---:|
| Common haunt refuse | `5-20` | `1-3` copper |
| Standard useful salvage | `21-50` | `3-7` copper |
| Heavy or awkward salvage | `51-100` | `7-15` copper |
| Named or locked-placement salvage | `100-180` | `15-27` copper |
| Explicit fixture reward salvage | `150-250` | `22-37` copper |

These are source/economy bands only and must not become player-facing rarity tiers.

### F5: CurrencyContainer Consume

The `currency_container_consume` formula is defined as:

`new_carried_currency_copper = checked_add(carried_currency_copper, container_value_copper); destroy CurrencyContainer`

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `carried_currency_copper` | `CCC` | int | `0+` | Persisted carried currency balance. |
| `container_value_copper` | `CVC` | int | `1-75` | Resolved value on the item record. |

**Output Range:** non-negative integer copper balance; overflow rejects the consume transaction.

**Example:** `carried_currency_copper = 4` and a `CurrencyContainer` with `container_value_copper = 12` produces `new_carried_currency_copper = 16`, then destroys the container item record.

Consumption never rolls. If a future container uses randomization, the roll occurs at materialization from an approved fixture seed and persists `resolved_value_copper`; consumption reads the resolved value only.

Recommended `CurrencyContainer` value bands:

| Container Source | Default Range |
|---|---:|
| Small authored purse/cache | `1-5` |
| Haunt placement cache | `6-15` |
| Named/locked cache | `16-40` |
| Explicit fixture reward | `25-75` |

### F6: Gear Plateau Validator

The `t1_equipment_combat_delta_valid` formula is defined as:

`t1_equipment_combat_delta_valid = all(combat_stat_delta(item) == 0 for item in T1 equipment)`

For fixture loadouts:

`CombatStats(level, loadout) == CombatProgressionBaselineSnapshot(level) + CombatCoreClassFixture(level)`

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `level` | `L` | int | `1, 5, 10` for T1 fixture validation | Character level under validation. |
| `loadout` | `LO` | enum | `StartingOnly, FieldFullSet, SidegradeMixedSet, IllegalStatBearingEquipment` | Authored validation loadout. |
| `combat_stat_delta(item)` | `CSD` | stat map | `0` required | Combat stat delta supplied by an item. |
| `CombatProgressionBaselineSnapshot(level)` | `CPBS` | read model | Approved ADR-0003 fields | Level and permanent max-resource baseline. |
| `CombatCoreClassFixture(level)` | `CCF` | authored fixture | Cleric T1 fixtures | Combat Core class fixture values. |

**Output Range:** boolean validator result.

**Example:** `FieldFullSet` at level 5 must produce the same Combat stats as the level-5 Combat fixture after ADR-0003 progression baseline is applied. `IllegalStatBearingEquipment` must fail because its item definition includes non-zero Combat stat delta.

F6 consumes `CombatCoreClassFixture(level)` from authored Combat Core Cleric fixture data, including the Cleric fixture rows referenced by Character Progression's level 1/5/10 resource checks. If Combat Core does not expose this as a runtime query, the validator runs as Editor-time validation against authored fixture YAML. The runtime-vs-Editor execution model is `INV-OQ-01` in §Open Questions.

Any T1 equipment with item level, rarity, affix, upgrade rank, socket, set bonus, scaling stat, or non-zero Combat stat delta fails validation.

### Coupled Tuning Rules

Coupled tuning rules are consolidated in §Tuning Knobs so formula math and tuning policy do not drift.

### Fixture Gates

Before pacing acceptance criteria can claim tuned carry pressure or modest economy, they need:

- `InventoryHauntRunProfile_T1` or `LegalPickupRoute_T1` for slot/weight pressure over 60, 120, and 180 minutes.
- `CoinFaucetProjection_T1` for expected sell value, slots used, weight used, and copper/hour.
- `EquipmentPlateauFixtureSet_T1` for no-combat-stat gear at levels 1, 5, and 10.
- A fixture gate for any `CurrencyContainer` that appears in loot tables rather than fixed world placement.

## Edge Cases

### Category A: Materialization and First-Save

- **If first-save write or HMAC commit fails after `InventoryFirstSaveMaterializer` succeeds**: the materialized payload is discarded and `InventoryFirstSaveMaterializer` re-runs deterministically on retry from the same pending `InitialCharacterRecord`. The materializer is idempotent: identical inputs (`local_character_id` + `ClericStartingEquipment_T1` + empty carry seed) produce identical `InventorySaveState`. ADR-0004's `rematerializing_existing_record_on_load` ban applies only to post-first-success loads, not failed-first-save retries; retry reuses the pending `local_character_id` per ADR-0004 retry semantics.
- **If `ClericStartingEquipment_T1` materializes into a state that violates F1 slot or weight caps**: first-save rejects as `FirstSaveMaterializationFailed(StarterLoadoutCapacityInvalid)` before Save/Load serializes bytes. No splitting, deletion, replacement, or fallback loadout is allowed.
- **If starter equipment contains any non-zero Combat stat delta**: first-save rejects as `FirstSaveMaterializationFailed(T1CombatStatDeltaForbidden)` at authored-data validation or materialization time, not at later hydration.
- **If a later load is missing `InventorySaveState`**: Save/Load rejects as `LoadRejected(HydrationFailed)` with Inventory reason `InventoryStateMissing`.
- **If a later load is missing an equipped-slot binding for a slot whose schema marks it required**: Save/Load rejects as `LoadRejected(HydrationFailed)` with Inventory reason `RequiredEquippedSlotMissing`. Slots whose schema permits empty (for example, Charm if Cleric equip rules allow it) are valid as missing; schema-permitted absence is not a hydration failure.
- **If a later load contains corrupt stack records**: Save/Load rejects as `LoadRejected(HydrationFailed)` with Inventory reason `StackRecordInvalid`; no clamp, deletion, split, or synthetic replacement stack is allowed.

### Category B: Pickup, Equip, Vendor, and Currency Atomicity

- **If pickup would overfill an existing compatible stack**: Inventory fills compatible stacks in `deterministic_slot_order`, allocates any remainder into new slots only if F1 and F3 pass, and rejects the whole pickup before mutation if the full quantity cannot fit. No over-cap stack and no partial pickup are allowed.
- **If an equip-swap cannot return the old item to a legal carried slot**: the equip rejects as `EquipSwapCarryDestinationInvalid` before mutation (per F1); equipped and carried state remain unchanged.
- **If equipping a two-handed or exclusive item would clear OffHand and the OffHand item has no legal carried destination**: the equip rejects as `ExclusiveEquipBlockedByCarryCapacity` before mutation (per F1). No ground spill, hidden overflow, or deletion occurs.
- **If a `CurrencyContainer` pickup fits as an item but later consume would overflow `carried_currency_copper`**: pickup may succeed; consume rejects as `CurrencyOverflowOnConsume` with the container intact and the currency balance unchanged. Intentional consequence: a player at currency cap can carry an unconsumable container indefinitely, and the container occupies a slot until the player spends, vendors, or destroys it. No silent destruction, automatic consume-on-overflow-clear, value clamp, or spillover mechanic exists. Future UI must surface `CurrencyOverflowOnConsume` so the player understands the carrying pressure rather than experiencing it as a bug.
- **If multiple pickup requests arrive in the same simulation frame**: Inventory processes them in the order their source events were received by the Inventory event handler, using FIFO receive-queue order. Each pickup is a synchronous atomic mutation that recomputes F1 against the post-previous-mutation state before committing. Fixtures can scaffold same-frame pickups by controlling receive-queue order; no parallel merge or batched application exists.
- **If selling Salvage would overflow `carried_currency_copper`**: the vendor sale rejects as `CurrencyOverflowOnVendorSale` before item removal or currency credit.
- **If buying from a vendor would exceed carried slot or weight capacity**: Inventory prevalidates F1 before currency debit and rejects as `VendorBuyCarryCapacityExceeded`; currency remains untouched.

### Category C: Cross-System Boundaries

- **If a loot table references a defeated source whose Progression row has `xp_eligible = false`**: loot may still materialize if Inventory's loot table allows it; XP eligibility does not gate loot eligibility.
- **If Death & Corpse Recovery requests `InventoryCorpseSnapshot(death_context_id)` for an unknown death context**: Inventory rejects as `CorpseSnapshotUnknownDeathContext` before snapshot creation; no item move, delete, or transfer occurs.
- **If Inventory hydration succeeds but Combat hydration later fails**: the overall load rejects as `LoadRejected(HydrationFailed)`, no playable session starts, and Inventory's staged hydrated state must not become active or write back to disk.
- **If Manual Save is requested during a vendor transaction**: the transaction must already be fully complete or not yet begun when Save/Load reads. Any implementation that exposes mid-transaction Inventory state violates Rule 18 and requires a future `InventorySaveBarrier` decision.
- **If a `FactionToken` references an unknown `faction_id`**: hydration rejects as `LoadRejected(HydrationFailed)` with Inventory reason `FactionTokenFactionUnknown`; no token deletion, remap, or reputation inference occurs.
- **If starter equipment excludes `Cleric` in `class_eligibility`**: first-save rejects as `FirstSaveMaterializationFailed(ClassIllegalEquipment)`. If class-illegal equipment appears in a later save, hydration rejects instead.

## Dependencies

Inventory & Item Economy has two hard direct upstream dependencies in T1: Save / Load & Persistence and Character Creation. Other systems below are build-time validation boundaries, future event boundaries, or explicit non-dependencies. This dependency narrowness directly enforces P3 (items do not replace reputation as the ladder) and P5 (carry pressure must be honest, not laundered through hidden cross-system effects). This system owns item truth; it must not become a hidden bridge from combat, progression, faction reputation, UI, or networking into item state.

Each downstream GDD listed below must reverse-list Inventory & Item Economy when authored if it consumes an Inventory-owned interface. `/consistency-check` and `/review-all-gdds` should verify bidirectional agreement.

### Hard Direct Upstream

| System | Direction | Data Interface | Contract |
|---|---|---|---|
| **Save / Load & Persistence** | Inventory is a hard persistence client | `InventorySaveState`, equipped slot bindings, carried slot contents, item instance records, stack records, `carried_currency_copper`, faction-token records, `InventoryFirstSaveMaterializer`, hydration validation result, Inventory reason codes under `LoadRejected(HydrationFailed)` | Inventory owns schema, materialization, validation, atomic mutation state, and rejection of invalid payloads. Save/Load owns serialization, HMAC, versioning, active-record identity context, first-save orchestration, write failure, load rejection, and save timing. T1 Inventory declares no ADR-0002 `InventorySaveBarrier` because all mutations are synchronous and atomic. |
| **Character Creation** | Inventory consumes first-save seed data | ADR-0004 `InitialCharacterRecord` context: active `local_character_id`, `starting_class_id = Cleric`, `starting_equipment_template_id = ClericStartingEquipment_T1`, and `carried_inventory = []` | Character Creation owns local character identity and seed validation. Inventory owns item instance materialization, equipment-slot legality, stack rules, currency defaults, starter-template validation, and all Inventory save-state output. Character Creation never creates item instances or repairs missing starter templates. |

### Build-Time Data and Validation Boundaries

| Source | Direction | Data Interface | Contract |
|---|---|---|---|
| **Combat Core** | Inventory may consume future loot eligibility context; Inventory validates against Combat fixture data | Stable `defeated_source_ref`, `zoneId`, optional `faction_id`; authored `CombatCoreClassFixture(level)` for F6 validation | Combat owns combat resolution, kill/death events, runtime resources, actor ids, and combat formulas. Inventory owns loot tables, item drops, equipment legality, and item schema. `combat_actor_id` is never legal in Inventory persistence or loot lookup. |
| **NPC System / Spawn Authoring Data** | Inventory loot tables may key against stable authored source refs | Stable NPC/spawn refs used by Combat as `defeated_source_ref` | NPC System owns source identity and lifecycle durability. Inventory owns loot eligibility and item materialization keyed to stable refs. Inventory does not read or mutate NPC source lifecycle state. |
| **Character Progression** | Inventory uses only build-time validation context in T1 | ADR-0003 `CombatProgressionBaselineSnapshot(level)` for F6 gear plateau fixture checks | Progression owns XP, level, permanent max health/mana baselines, spell eligibility, and XP-source lookup. Inventory does not grant XP, read XP progress, gate T1 items by level, or use progression source metadata as loot metadata. |

### Future Hard Downstream

| Future System | Inventory Interface Consumed | Contract |
|---|---|---|
| **Death & Corpse Recovery** | `InventoryCorpseSnapshot(death_context_id)` containing equipped items, carried items, carried currency, and carried faction tokens | Death & Corpse Recovery owns death timing, corpse creation, recovery, expiry, restoration, and any XP/currency/item penalties. Inventory only provides a snapshot when called with a valid death context. |
| **Faction Reputation** | Faction-token possession, deposit state, and any future token turn-in request | Faction Reputation owns social meaning, reputation values, tiers, titles, access, NPC trust, and token effects. Inventory owns token possession and persistence only. |
| **Layer 1 HUD / Inventory UI** | Item/currency/token read models, capacity state, transaction failure reasons such as `CurrencyOverflowOnConsume` | UI owns presentation, affordances, accessibility, and player-facing messaging. Inventory owns the data and failure reasons. |
| **Combat Core** | Future `EquipmentCombatStatSnapshot` only after a T2+ ADR introduces stat-bearing equipment | T1 equipment has zero Combat stat delta. Any future stat-bearing equipment requires ADR-level hydration-order revision and Combat reverse-listing. |

### Future Optional Consumers

| System | Possible Interface | Boundary |
|---|---|---|
| **Class Design** | Future class eligibility expansion beyond Cleric | T1 eligibility is Cleric-only and Inventory-owned. Future classes may consume or extend eligibility rules after Class Design is authored. |
| **Vendor / Dialogue / NPC-facing surfaces** | Vendor profile presentation, item names, sale/buy prompts | Inventory owns transaction legality and authored price tables. Dialogue/NPC/UI systems own presentation and interaction framing. |
| **Audio System** | Transaction, pickup, reject, equip, and inventory-full events | Audio owns playback and mix. Inventory emits restrained event hooks only if later UI/audio work requests them. |

### Boundary-Only / Non-Dependencies

| System | Relationship | Explicit Non-Contract |
|---|---|---|
| **World Structure** | Boundary only | Inventory may reference `CityHub` and `Haunt` placement context in authored content, but it does not own zone transitions, zone activation, save triggers, or corpse records. |
| **Character Progression** | Boundary only at runtime in T1; Editor-time fixture validation only | Inventory does not award XP, consume XP events, set level gates, grant spells, or inspect XP-source lifecycle at runtime. The only legal Character Progression touch is Editor-time consumption of `CombatProgressionBaselineSnapshot` for the F6 gear plateau validator, per §Build-Time Data and Validation Boundaries above. |
| **Faction State Simulation / Zone Control** | Boundary only | Inventory does not mutate faction control, faction state, zone ownership, political outcomes, or kill-weight math. |
| **Network Architecture / Authentication & Accounts** | Boundary only in T1 | No account inventory, server authority, replicated item ownership, trading, shared stash, or online identity exists in T1. |
| **Crafting / Durability / Auction / Trade Systems** | Out of scope | These systems are not T1 dependencies and cannot be implied by Inventory schema fields. |

### Reverse-Listing Obligations

- Save / Load already lists Inventory & Item Economy as a hard persistence client. When Save/Load is revised, it should name `InventoryFirstSaveMaterializer`, `InventorySaveState`, Inventory-owned hydration reason codes, and the T1 no-`InventorySaveBarrier` constraint.
- Character Creation already lists Inventory as the downstream owner of `ClericStartingEquipment_T1` materialization and empty carried-inventory seed consumption. That boundary must remain: Character Creation seeds, Inventory materializes.
- Combat Core should reverse-list Inventory only for future loot eligibility or any T2+ stat-bearing equipment contract. T1 Combat stats remain independent of equipment.
- Death & Corpse Recovery must reverse-list `InventoryCorpseSnapshot(death_context_id)` when authored.
- Faction Reputation must reverse-list any faction-token turn-in, deposit, or meaning contract when authored.
- Layer 1 HUD / Inventory UI must reverse-list Inventory read models and failure reasons when authored.

## Tuning Knobs

Tuning Knobs consolidates values already defined in §Formulas. This section does not introduce new economy surfaces; it records defaults, safe ranges, and failure modes for implementation and fixture review.

| Knob | Default | Safe Range | Too Low / Narrow | Too High / Broad |
|---|---:|---|---|---|
| `carried_slot_cap` | `18` slots | `16-22` | Carry pressure becomes cramped before the player can make meaningful keep/sell/equip choices. | Hoarding becomes easy and return-to-city decisions lose weight. |
| `carried_weight_cap` | `60` weight_units | `45-75` | Heavy items become soft traps and normal haunt pickups overtax 60-minute runs. | Weight stops mattering as a pickup-legality constraint. |
| `max_t1_item_weight_units` | `25` | Fixed unless fixture-proven | Heavy gear cannot feel committal. | One item can consume too much of the bag and create avoidable starter/loadout traps. |
| `equipment_max_stack` | `1` | Fixed | Equipment stacking becomes undefined. | Equipment becomes item-stack economy instead of gear choice. |
| `consumable_max_stack` | `5` | `3-10` | Consumables tax slots so hard that preparation feels punitive. | Consumable hoarding weakens carry-pressure decisions. |
| `salvage_max_stack` | `6` | `4-10` | Salvage floods slots and makes vendor return loops too frequent. | Salvage becomes too compact and coin faucet pressure becomes too soft. |
| `faction_token_max_stack` | `20` | `10-30` | Token possession crowds out normal loot before Faction Reputation exists. | Tokens become frictionless counters and lose object weight. |
| `currency_container_max_stack` | `1` | `1-3`; default stays `1` | `< 1` is invalid; `max_stack >= 1` by F3 contract. The `1` lock exists because containers are currency in transit; stacking would conflate transit with rest. | Currency-in-transit starts acting like currency at rest. |
| `starting_carried_currency_copper` | `0` | `0-9` | New Cleric starts with no spending buffer; this is intentional at default. | New Cleric feels pre-funded and early coin stops mattering. |
| `salvage_sell_multiplier` | `0.15` | `0.10-0.20` | Salvage feels worthless and vendor visits lose purpose. | Salvage becomes the dominant reward loop before economy feel is proven. |
| `salvage_nominal_value_bands` | See F4 bands | F4 ranges only until `CoinFaucetProjection_T1` passes | Haunt returns feel empty and spending cannot recover naturally. | Coin/hour inflates and vendor prices lose bite. |
| `currency_container_value_bands` | See F5 bands | F5 ranges only until `CoinFaucetProjection_T1` passes | Authored purses feel irrelevant. | Fixed pickups bypass the salvage loop and overfund the player. |
| `T1_CityHubVendorBuyTable_T1` | TBA authored constants | Fixture-owned; must pass `CoinFaucetProjection_T1` | Purchases are unreachable and coin becomes cosmetic. | Purchases are trivial and spending stops feeling like a choice. |
| `t1_equipment_bands` | `Starting`, `Field`, `Sidegrade` | Fixed for T1 | Gear distinctions become too flat to support preparation. | Gear begins to imply vertical progression. |
| `equipment_plateau_fixture_lock_levels` | `[1, 5, 10]` | Fixed for T1 | Gear plateau validation becomes implicit and easy to break. | More lock points make validation brittle before playable combat data exists. |
| `pickup_frequency_per_haunt_run` | TBA fixture value | Owned by `InventoryHauntRunProfile_T1` / `LegalPickupRoute_T1` | Carry pressure never appears in normal sessions. | Bag pressure becomes constant friction instead of meaningful choice. |
| `return_to_city_pacing` | TBA fixture value | Owned by `InventoryHauntRunProfile_T1` | Vendor/sell loops feel optional and stakes soften. | Runs feel interrupted by inventory cleanup rather than danger or planning. |

### Coupled Tuning Rules

- Capacity tuning must move as a group: `carried_slot_cap`, `carried_weight_cap`, category stack caps, pickup frequency, item weight bands, and return-to-city pacing. Changing one value alone can erase carry pressure or overtax normal haunt runs.
- Currency tuning must move as a group: `salvage_sell_multiplier`, salvage nominal value bands, `CurrencyContainer` values, vendor buy prices, and expected pickups/hour. No acceptance criterion may claim a modest economy without `CoinFaucetProjection_T1`.
- Gear plateau tuning is not a T1 knob. Any stat-bearing equipment, item level, rarity, affix, upgrade rank, socket, set bonus, or scaling stat requires a T2+ ADR and hydration-order revision.
- `CurrencyContainer` entries from loot tables require fixture proof before supporting pacing claims. Fixed world placements can use authored values directly, but still count against the overall coin-faucet projection.
- `InventoryHauntRunProfile_T1`, `LegalPickupRoute_T1`, `CoinFaucetProjection_T1`, and `EquipmentPlateauFixtureSet_T1` are required before review can treat carry pressure, coin pacing, or gear plateau claims as tuned rather than provisional.

## Acceptance Criteria

All criteria use the project QA taxonomy: Unit, Integration, Editor-validation, Dev-build smoke, or Profiled playtest. All are T1-blocking unless marked fixture-gated.

### Scope / Initialization

**H-INV-SCOPE-01 - T1 scope exclusions**
**GIVEN** authored Inventory feature flags and item categories, **WHEN** the Editor validator runs, **THEN** only `Equipment`, `Consumable`, `Salvage`, `FactionToken`, and `CurrencyContainer` are legal; no crafting, durability, auction, player trade, mail, account bank, shared stash, PvP transfer, companion inventory, LLM item text, rare affix, set bonus, transmog, cosmetic, or multi-character inventory feature is enabled.
*Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-INIT-01 - First-save materializer produces Inventory state**
**GIVEN** Save/Load invokes `InventoryFirstSaveMaterializer` with ADR-0004 active `local_character_id`, `starting_class_id = Cleric`, `starting_equipment_template_id = ClericStartingEquipment_T1`, and `carried_inventory = []`, **WHEN** materialization succeeds, **THEN** Inventory produces valid `InventorySaveState` with equipped starter items, empty carried inventory, `carried_currency_copper = 0`, and no duplicated `local_character_id` inside item records.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-INIT-02 - Character Creation seed does not create items**
**GIVEN** a valid `InitialCharacterRecord`, **WHEN** Character Creation completes, **THEN** no item instances, equipped slots, stack records, currency balances, or faction-token records are created outside `InventoryFirstSaveMaterializer`.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

### Persistence Schema / Hydration

**H-INV-SL-01 - Persistence whitelist**
**GIVEN** an Inventory save payload, **WHEN** serialized fields are inspected, **THEN** it contains only `inventory_schema_version`, equipped slot bindings, carried slot contents, item instance records, stack records, `carried_currency_copper`, faction-token stack records, and optional test state revision.
*Unit | gameplay-programmer | T1-blocking*

**H-INV-SL-02 - Forbidden persisted fields absent**
**GIVEN** an Inventory save payload, **WHEN** serialized fields are inspected, **THEN** no derived stat totals, rarity colors, runtime handles, scene refs, loot containers, drag state, vendor UI state, combat actor ids, or cached formulas are present.
*Unit + Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-SL-03 - Missing Inventory state rejects load**
**GIVEN** an initialized save is missing `InventorySaveState`, **WHEN** Save/Load hydrates Inventory, **THEN** load rejects as `LoadRejected(HydrationFailed)` with Inventory reason `InventoryStateMissing`, and no playable session starts.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-SL-04 - Required equipped-slot absence rejects load**
**GIVEN** a save is missing an equipped-slot binding whose schema marks it required, **WHEN** Inventory hydrates, **THEN** load rejects with Inventory reason `RequiredEquippedSlotMissing`; schema-permitted empty slots remain valid.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-SL-05 - Corrupt stack records reject load**
**GIVEN** stack quantity is zero, negative, above `max_stack`, non-integer, or attached to an illegal item category, **WHEN** Inventory hydrates, **THEN** load rejects with Inventory reason `StackRecordInvalid`; no clamp, split, deletion, or replacement occurs.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-SL-06 - Invalid payload classes reject loudly**
**GIVEN** unknown item definitions, duplicate item instance ids, one instance in multiple locations, illegal equipped slots, class-illegal equipment, currency underflow/overflow, unknown faction token definitions, runtime handles, scene refs, or malformed schema versions, **WHEN** Inventory hydrates, **THEN** hydration fails loudly and no playable session starts.
*Integration + Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-SL-07 - No load-time repair from creation defaults**
**GIVEN** an initialized save is missing Inventory state, starter records, or required equipped-slot records, **WHEN** Inventory hydrates, **THEN** it does not synthesize fallback gear, delete invalid records, clamp values, or re-run first-save materialization.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

### First-Save Materialization

**H-INV-FS-01 - Failed first-save retry is deterministic**
**GIVEN** first-save write or HMAC commit fails after materialization, **WHEN** Save/Load retries with the same pending `InitialCharacterRecord`, **THEN** `InventoryFirstSaveMaterializer` re-runs idempotently and produces identical `InventorySaveState` from identical inputs.
*Integration | gameplay-programmer + engine-programmer + qa-tester | T1-blocking*

**H-INV-FS-02 - Starter loadout capacity violation rejects first save**
**GIVEN** `ClericStartingEquipment_T1` would exceed F1 slot or weight caps, **WHEN** first-save materialization runs, **THEN** it rejects as `FirstSaveMaterializationFailed(StarterLoadoutCapacityInvalid)` and Save/Load writes no bytes.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-FS-03 - Starter combat stat delta rejects first save**
**GIVEN** starter equipment contains non-zero Combat stat delta, **WHEN** authored data validates or first-save materialization runs, **THEN** it rejects as `FirstSaveMaterializationFailed(T1CombatStatDeltaForbidden)`.
*Editor-validation + Unit | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-FS-04 - Class-illegal starter equipment rejects first save**
**GIVEN** starter equipment excludes `Cleric` in `class_eligibility`, **WHEN** first-save materialization runs, **THEN** it rejects as `FirstSaveMaterializationFailed(ClassIllegalEquipment)` and no initialized record exists.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

### Equip / Slot / Stack Atomicity

**H-INV-EQ-01 - T1 equipment slots are exact**
**GIVEN** T1 item definitions, **WHEN** the Editor validator inspects equippable items, **THEN** every equippable item targets only `MainHand`, `OffHand`, `Body`, or `Charm`, declares Cleric eligibility, and is non-stackable.
*Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-EQ-02 - Equip-swap is atomic**
**GIVEN** replacing equipped gear requires moving the old item into carried inventory, **WHEN** the carried destination fails F1, **THEN** equip rejects as `EquipSwapCarryDestinationInvalid` and equipped/carried state remains unchanged.
*Unit | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-EQ-03 - Exclusive OffHand clear must have carry destination**
**GIVEN** equipping a two-handed or exclusive item would clear OffHand, **WHEN** the OffHand item has no legal carried destination, **THEN** equip rejects as `ExclusiveEquipBlockedByCarryCapacity`; no ground spill, hidden overflow, or deletion occurs.
*Unit | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-STK-01 - Stacks merge only by exact key**
**GIVEN** two stackable items differ in definition, faction marker, bind state, condition field, or token metadata, **WHEN** pickup or merge is attempted, **THEN** they do not merge and must allocate separate legal slots or reject.
*Unit | gameplay-programmer | T1-blocking*

**H-INV-STK-02 - Over-cap stack pickup rejects whole pickup if remainder cannot fit**
**GIVEN** pickup would overfill an existing compatible stack, **WHEN** deterministic fill plus new-slot allocation cannot fit the full quantity under F1 and F3, **THEN** the whole pickup rejects before mutation.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-STK-03 - Stack-fill order is deterministic**
**GIVEN** multiple compatible carried stacks with capacity, **WHEN** pickup merges, **THEN** Inventory fills stacks in ascending carried slot index before allocating an empty slot.
*Unit | gameplay-programmer | T1-blocking*

### Carry Pressure / Formulas

**H-INV-F1-01 - Slot and weight caps both gate pickup**
**GIVEN** 17 occupied slots and 58 weight, **WHEN** a 1-slot, 2-weight pickup is tested, **THEN** F1 passes; **WHEN** a 1-slot, 3-weight pickup is tested, **THEN** F1 rejects on weight.
*Unit | gameplay-programmer | T1-blocking*

**H-INV-F1-02 - Merge accounting keeps slot count stable**
**GIVEN** a pickup merges into an existing compatible stack with capacity, **WHEN** F1 evaluates, **THEN** `projected_slot_count = current_slot_count` and `projected_weight_units` still increases by `unit_weight_units * pickup_quantity`.
*Unit | gameplay-programmer | T1-blocking*

**H-INV-F2-01 - Stack weight formula matches example**
**GIVEN** `unit_weight_units = 4` and `quantity = 6`, **WHEN** `stack_weight_units` evaluates, **THEN** output is `24` weight_units.
*Unit | gameplay-programmer | T1-blocking*

**H-INV-F3-01 - Default stack caps validate**
**GIVEN** T1 stack cap config, **WHEN** F3 validates defaults, **THEN** `Equipment = 1`, `Consumable = 5`, `Salvage = 6`, `FactionToken = 20`, and `CurrencyContainer = 1`.
*Unit + Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-CARRY-01 - Weight is pickup legality only**
**GIVEN** carried weight ranges from `0` to `carried_weight_cap`, **WHEN** player movement, stamina, encumbrance, animation, regen, combat, casting, or any non-pickup runtime behavior is inspected, **THEN** weight has zero effect; the only legal weight consequence is pickup rejection via F1.
*Integration + Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

### Currency / Vendor

**H-INV-CUR-01 - Currency at rest is integer copper only**
**GIVEN** Inventory save state, **WHEN** currency fields are inspected, **THEN** only non-negative integer `carried_currency_copper` represents currency at rest; floats, denominations, coin piles, negative balances, and malformed balances reject.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-CUR-02 - Vendor salvage sale formula matches examples**
**GIVEN** `nominal_value_copper = 50` and `salvage_sell_multiplier = 0.15`, **WHEN** F4 evaluates, **THEN** `vendor_sell_copper = 7`; **GIVEN** the product floors below `1`, **THEN** output is `1`.
*Unit | gameplay-programmer | T1-blocking*

**H-INV-CUR-03 - Ordinary kills do not directly grant coin**
**GIVEN** Combat loot context from an ordinary T1 kill, **WHEN** Inventory evaluates currency outcomes, **THEN** it never directly increments `carried_currency_copper`; only an approved `CurrencyContainer` item can later credit currency through F5.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-CUR-04 - CurrencyContainer consume is deterministic**
**GIVEN** `carried_currency_copper = 4` and a `CurrencyContainer` with `container_value_copper = 12`, **WHEN** F5 consume succeeds, **THEN** new balance is `16` and the container item record is destroyed.
*Unit | gameplay-programmer | T1-blocking*

**H-INV-CUR-05 - Currency overflow on consume preserves container**
**GIVEN** consuming a `CurrencyContainer` would overflow `carried_currency_copper`, **WHEN** consume is attempted, **THEN** it rejects as `CurrencyOverflowOnConsume`, leaves the container intact, and leaves currency unchanged.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-VEN-01 - Vendor buy prevalidates before debit**
**GIVEN** a CityHub vendor purchase would exceed carried slot or weight capacity, **WHEN** buy is attempted, **THEN** Inventory rejects as `VendorBuyCarryCapacityExceeded` before currency debit.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-VEN-02 - Vendor profile is fixed-constant only**
**GIVEN** the T1 CityHub vendor profile and authored buy/sell tables, **WHEN** the Editor validator inspects vendor configuration, **THEN** buy/sell prices are authored constants only; no dynamic pricing, stock simulation, reputation discount, faction-rank goods, limited-time rotation, token buying, or arbitrage hook is configured. Future dynamic pricing requires a tier-transition decision.
*Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

### Faction Tokens

**H-INV-TOK-01 - Token possession schema is narrow**
**GIVEN** faction-token records, **WHEN** persisted fields are inspected, **THEN** only Inventory-owned possession fields such as `token_def_id`, `faction_id`, `source_ref`, quantity, bind state, and deposit state are present.
*Unit + Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-TOK-02 - Tokens do not mutate reputation**
**GIVEN** a faction token is picked up, carried, stacked, saved, loaded, or deposited as Inventory state, **WHEN** downstream faction systems are absent, **THEN** no reputation, rank, access, title, NPC trust, political state, or faction meaning changes.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-TOK-03 - Unknown faction id rejects hydration**
**GIVEN** a persisted `FactionToken` references an unknown `faction_id`, **WHEN** Inventory hydrates, **THEN** load rejects as `LoadRejected(HydrationFailed)` with Inventory reason `FactionTokenFactionUnknown`.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

### Combat Boundary / Gear Plateau

**H-INV-CB-01 - Combat actor ids are illegal**
**GIVEN** Inventory persistence, loot lookup, or item records are schema-scanned, **WHEN** fields are inspected, **THEN** no `combat_actor_id`, runtime Combat handle, threat table, damage roll, or Combat current-resource field is present.
*Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-CB-02 - XP eligibility does not gate loot**
**GIVEN** a defeated source has `xp_eligible = false` in Progression metadata but an Inventory loot table permits a drop, **WHEN** Inventory evaluates loot, **THEN** XP eligibility does not block the Inventory-owned loot decision.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-GEAR-01 - Gear plateau fixture preserves Combat envelope**
**GIVEN** `EquipmentPlateauFixtureSet_T1` validates `StartingOnly`, `FieldFullSet`, and `SidegradeMixedSet` at levels `1`, `5`, and `10`, **WHEN** F6 evaluates, **THEN** Combat stats match ADR-0003 `CombatProgressionBaselineSnapshot(level)` plus `CombatCoreClassFixture(level)` with zero item Combat stat delta.
*Editor-validation + Unit | gameplay-programmer + systems-designer + qa-tester | fixture-gated T1-blocking*

**H-INV-GEAR-02 - Stat-bearing equipment is illegal in T1**
**GIVEN** any T1 equipment contains item level, rarity, affix, upgrade rank, socket, set bonus, scaling stat, or non-zero Combat stat delta, **WHEN** item definitions validate, **THEN** validation fails and `EquipmentCombatStatSnapshot_T1` remains future-reserved only.
*Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

### Death / Corpse Boundary

**H-INV-DCR-01 - Death alone does not mutate Inventory**
**GIVEN** Combat emits `PlayerDeathEvent`, **WHEN** Death & Corpse Recovery has not called an approved Inventory snapshot/recovery interface, **THEN** Inventory does not move, delete, transfer, expire, or restore items, currency, or faction tokens.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-INV-DCR-02 - Unknown death context rejects snapshot**
**GIVEN** Death & Corpse Recovery requests `InventoryCorpseSnapshot(death_context_id)` for an unknown death context, **WHEN** Inventory evaluates the request, **THEN** it rejects as `CorpseSnapshotUnknownDeathContext` before creating a snapshot or mutating state.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

### Atomicity / Save Boundary

**H-INV-ATM-01 - No InventorySaveBarrier in T1**
**GIVEN** T1 Inventory runtime mutation APIs, **WHEN** implementation is scanned, **THEN** all mutations are synchronous and atomic, no ADR-0002 `InventorySaveBarrier` is declared, and no multi-frame ownership-changing transaction exists.
*Editor-validation + Integration | gameplay-programmer + engine-programmer + qa-tester | T1-blocking*

**H-INV-ATM-02 - Manual Save cannot observe mid-vendor state**
**GIVEN** Manual Save is requested during a vendor transaction test latch, **WHEN** Save/Load reads Inventory state, **THEN** the transaction is either fully complete or not yet begun; no partially debited, partially removed, or partially credited state is visible.
*Integration | gameplay-programmer + engine-programmer + qa-tester | T1-blocking*

### Same-Frame Pickup Determinism

**H-INV-ATM-03 - Same-frame pickups are FIFO and recompute F1**
**GIVEN** two pickup requests arrive in one simulation frame, **WHEN** fixtures enqueue them in a known receive-queue order, **THEN** Inventory commits them FIFO, recomputes F1 after the first mutation, and never applies a parallel merge or batched state.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

### Tuning / Fixture Validation

**H-INV-TUNE-01 - Tuning safe-range validator**
**GIVEN** authored Inventory tuning data, **WHEN** the Editor validator runs, **THEN** every knob in §Tuning Knobs is within its safe range or explicitly marked fixed, fixture-owned, or TBA fixture value.
*Editor-validation | systems-designer + qa-tester | T1-blocking*

**H-INV-TUNE-02 - Capacity tuning changes warn as a group**
**GIVEN** `carried_slot_cap`, `carried_weight_cap`, category stack caps, pickup frequency, item weight bands, or return-to-city pacing changes alone, **WHEN** validation runs, **THEN** it emits a designer-facing warning requiring carry-pressure revalidation.
*Editor-validation | systems-designer | T1-blocking*

**H-INV-TUNE-03 - Coin pacing requires projection fixture**
**GIVEN** any claim about modest economy, copper/hour, vendor affordability, or coin pacing, **WHEN** validation checks evidence, **THEN** it must cite passing `CoinFaucetProjection_T1`; formula-only examples do not count as pacing proof.
*Editor-validation | systems-designer + qa-tester | fixture-gated T1-blocking*

**H-INV-TUNE-04 - Carry pressure requires haunt-run fixture**
**GIVEN** any claim about tuned slot/weight pressure over 60, 120, or 180 minutes, **WHEN** validation checks evidence, **THEN** it must cite `InventoryHauntRunProfile_T1` or `LegalPickupRoute_T1` with passing projection.
*Editor-validation + Profiled playtest | systems-designer + qa-tester | fixture-gated T1-blocking*

**H-INV-TUNE-05 - CurrencyContainer loot-table entries require fixture proof**
**GIVEN** a `CurrencyContainer` appears in a loot table rather than fixed world placement, **WHEN** tuning validation runs, **THEN** the loot entry must be included in the coin-faucet projection before it can support pacing claims.
*Editor-validation | systems-designer + qa-tester | fixture-gated T1-blocking*

### Presentation / Non-Scope

**H-INV-VUI-01 - No loot spectacle or gear-score presentation**
**GIVEN** T1 item definitions and Inventory-facing presentation hooks, **WHEN** they are inspected, **THEN** no rarity color, item level, gear score, affix tier, loot-confetti event, hero reward banner, or item-power ladder marker exists.
*Editor-validation + Dev-build smoke | ui-programmer + qa-tester | T1-blocking*

**H-INV-VUI-02 - Currency overflow reason is available to UI**
**GIVEN** `CurrencyOverflowOnConsume` occurs, **WHEN** Inventory returns the transaction result, **THEN** the reason code is available to future UI consumers without exposing formula internals, stack internals, or raw debug state in shipping surfaces.
*Unit + Integration | gameplay-programmer + ui-programmer + qa-tester | T1-blocking*

### Summary

Total: 51 criteria.
Ordinary T1-blocking: 47.
Fixture-gated T1-blocking: 4.
Advisory-at-T1: 0.

## Open Questions

| ID | Question | Owner | Required Before |
|---|---|---|---|
| `INV-OQ-01` | If T2+ introduces stat-bearing equipment, Save/Load hydration order must change to Character Progression -> Inventory -> Combat, and a new ADR must define the `EquipmentCombatStatSnapshot` contract with Combat reverse-listing. T1 equipment remains stat-flat. | Combat Core + Inventory + ADR | Stat-bearing equipment implementation (T2+) |
| `INV-OQ-02` | The Combat -> Inventory loot eligibility contract must be designed before any kill-credited loot drops. The likely shape is `InventoryLootEligibilityRequest(local_character_id, defeated_source_ref, zoneId, loot_context_id)` plus Inventory-owned `LootSourceRefLookup_T1`, separate from `ProgressionXpSourceRefLookup_T1`. T1 can ship authored placements such as fixed pickups/chests before this is resolved. | Combat Core + Inventory | Kill-credited loot drops |
| `INV-OQ-03` | Death & Corpse Recovery must own death timing and the call into `InventoryCorpseSnapshot(death_context_id)` before corpse-run item recovery can ship. If D&CR introduces multi-frame inventory transfer, this GDD must be amended to declare an ADR-0002 `InventorySaveBarrier` grouped with corpse state under `inventory_corpse_consistency`. | Death & Corpse Recovery + Inventory + ADR-0002 | Corpse-run item recovery implementation |
| `INV-OQ-04` | Authored data location and asset format are not locked for `ClericStartingEquipment_T1`, item definitions, `T1_CityHubVendorBuyTable_T1`, salvage value bands, or `CurrencyContainer` value bands. Likely path is ScriptableObjects under `assets/data/inventory/` per project structure, but asset format and Addressables grouping are implementation decisions. | gameplay-programmer + unity-addressables-specialist | Inventory implementation start |
