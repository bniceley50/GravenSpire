# S2-M3-03 - Loot Table + Fixed-Profile Vendor

**Status:** Blocked
**Sprint:** 2
**Priority:** Must Have
**Layer:** Gameplay / Unity Runtime
**Type:** Integration
**Estimate:** 1.0 days
**Manifest Version:** Sprint 2, 2026-05-14
**GDD:** `design/gdd/inventory-item-economy.md`; `design/gdd/game-concept.md`
**Quick Design:** `design/quick/quick-design-m3-objective-npc-loot.md`
**Governing Decisions:** `DECISIONS.md` D001, D002, D003
**Evidence:** `tests/evidence/S2-M3-03/verification.md`

## Routing Status

Blocked until `S2-M3-02` is complete. This story adds the restrained authored
M3 loot table and the blockout-grade fixed-profile vendor salvage sale. It proves
the mechanism only; it does not claim a tuned economy or implement full Inventory
persistence.

## Source Trace

- `design/quick/quick-design-m3-objective-npc-loot.md:270` through `:279`
  define M3-03 loot table and fixed-profile vendor acceptance candidates.
- `design/quick/quick-design-m3-objective-npc-loot.md:183` through `:196`
  define the fixed-profile vendor sketch.
- `design/gdd/inventory-item-economy.md:80` defines fixed-profile T1 vendors.
- `design/gdd/inventory-item-economy.md:193` through `:217` define F4 vendor
  salvage sale and value bands.
- `design/gdd/inventory-item-economy.md:542` through `:547` define H-INV-VEN-01
  and H-INV-VEN-02.
- `design/gdd/inventory-item-economy.md:626` through `:627` require
  CurrencyContainer loot-table entries to have coin-faucet projection proof.
- `design/gdd/inventory-item-economy.md:652` through `:655` park wider
  Inventory implementation pre-spec blockers.

## Scope

Add one small authored M3 data surface for objective loot and vendor sale. The
loot table resolves `CourtMarkedRelic_T1` or equivalent and sellable `Salvage`.
The vendor buys salvage using F4:

`vendor_sell_copper = max(1, floor(nominal_value_copper * 0.15))`

Vendor/currency state remains session-local.

Planned implementation surface:

- `data/first-district/m3-objective-npc-loot.json` or equivalent authored data.
- Data validation or unit tests for item ids, salvage nominal values, and F4.
- `M3_CourtVendor` or equivalent blockout vendor marker.
- Unity smoke proving salvage can be sold after recovery.

## Out Of Scope

- No full Inventory persisted schema, currency-at-rest persistence, Save/Load,
  repair-by-load, tuned vendor economy, coin pacing, copper-per-hour claim,
  `CoinFaucetProjection_T1`, buy-side price formula, CurrencyContainer loot
  entry, token buying, faction-rank goods, dynamic pricing, stock simulation,
  reputation discount, rotation, arbitrage, kill-credited loot drops, item stats,
  gear score, rarity color, affixes, set bonuses, or equipment progression.

## Acceptance Criteria

| ID | Criterion | Evidence |
| --- | --- | --- |
| `S2-M3-03-01` | One M3 loot table resolves `CourtMarkedRelic_T1` and sellable `Salvage` from authored data. | `tests/evidence/S2-M3-03/verification.md` |
| `S2-M3-03-02` | Loot lookup uses stable authored ids and contains no `combat_actor_id`, runtime actor handle, threat table, damage roll, or Combat current-resource field. | `tests/evidence/S2-M3-03/verification.md` |
| `S2-M3-03-03` | The loot table does not reuse `kill_weight_seed` as loot RNG. | `tests/evidence/S2-M3-03/verification.md` |
| `S2-M3-03-04` | The default M3 table contains no CurrencyContainer entry unless the story also supplies coin-faucet projection proof or classifies the entry as fixed world placement. | `tests/evidence/S2-M3-03/verification.md` |
| `S2-M3-03-05` | The fixed-profile vendor buys `Salvage` and applies the F4 formula `vendor_sell_copper = max(1, floor(nominal_value_copper * 0.15))` to produce copper. | `tests/evidence/S2-M3-03/verification.md` |
| `S2-M3-03-06` | The vendor prevalidates carried capacity before any currency debit and exposes no dynamic pricing, stock simulation, reputation discount, rotation, or arbitrage hook. | `tests/evidence/S2-M3-03/verification.md` |
| `S2-M3-03-07` | The vendor transaction is synchronous and atomic; no partially debited, partially removed, or partially credited state is observable. | `tests/evidence/S2-M3-03/verification.md` |
| `S2-M3-03-08` | Vendor and currency state are session-local; M3 makes no `CoinFaucetProjection_T1`, copper-per-hour, or tuned-economy claim, and does not persist currency at rest. | `tests/evidence/S2-M3-03/verification.md` |

## QA Test Cases

- **S2-M3-03-01**: Authored loot table
  - Given: M3 authored data exists.
  - When: validation runs.
  - Then: relic and salvage rows resolve by stable authored ids.
- **S2-M3-03-02**: Loot boundary
  - Given: loot data and lookup code are inspected.
  - When: forbidden runtime/combat fields are searched.
  - Then: no runtime actor handles or Combat-owned loot logic appear.
- **S2-M3-03-03**: No seed reuse
  - Given: loot resolution code is inspected.
  - When: RNG seeding is reviewed.
  - Then: `kill_weight_seed` is not reused as loot RNG.
- **S2-M3-03-04**: No CurrencyContainer
  - Given: the default M3 loot table is inspected.
  - When: entries are classified.
  - Then: no CurrencyContainer appears.
- **S2-M3-03-05**: F4 formula
  - Given: salvage has authored nominal copper value.
  - When: the vendor sale resolves.
  - Then: copper equals `max(1, floor(NVC * 0.15))`.
- **S2-M3-03-06**: Vendor fixed profile
  - Given: vendor config is inspected.
  - When: validation runs.
  - Then: prices are constants and no dynamic economy hooks exist.
- **S2-M3-03-07**: Atomic transaction
  - Given: sale is attempted.
  - When: sale succeeds or fails.
  - Then: no partial remove/credit/debit state is observable.
- **S2-M3-03-08**: No persistence/economy claim
  - Given: changed files and evidence are inspected.
  - When: M3 vendor behavior is reviewed.
  - Then: state is session-local and evidence avoids tuning claims.

## Test Evidence

Required evidence:

- `tests/evidence/S2-M3-03/verification.md`
- Data validation or unit tests for M3 data and F4.
- Unity runner evidence for vendor marker and salvage sale.
- T1 negative-scope scan over changed files.
- `git diff --check`
- `.githooks/pre-commit`

## Performance Budget

This story must keep loot/vendor resolution synchronous and small. It must not
add per-frame inventory polling, async transactions, dynamic pricing simulation,
stock ticking, Save/Load hooks, or broad item/economy scans.

## Dependencies

- Depends on: `S2-M3-02` complete.
- Unlocks: `S2-M3-04` End-To-End Objective Loop.

## Next Gate

Blocked until `S2-M3-02` is closed.
