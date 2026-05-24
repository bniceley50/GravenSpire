# S3-04: Player-Driven Vendor (Sell-Side Only)

> **Sprint**: Sprint 3 — Playable Vertical-Slice Assembly
> **Sprint Plan**: `production/sprints/sprint-3.md` (Story Ledger row, line 69)
> **Status**: Ready (depends on S3-03 being Done)
> **Layer**: Core
> **Type**: Integration
> **Estimate**: 1.0 day (HIGH confidence — thinnest wiring of the slate, per `sprint-3.md:158`; single M3 subsystem, single Try*, no state machine)
> **Manifest Version**: Unavailable (control-manifest absent project-wide per `production/qa/plans/qa-plan-sprint-2-20260509.md:54,60`; documented fallback applies)
> **Generated**: 2026-05-23
> **Owner**: Codex

## Context

**Sprint 3 plan**: `production/sprints/sprint-3.md`
**Quick-design source**: `design/quick/quick-design-m3-objective-npc-loot.md`
**Story Ledger row**: `production/sprints/sprint-3.md:69`
**Requirement IDs (story-local ACs)**: `S3-04-01` through `S3-04-08`

**Requirement Summary**: Wire the M3 fixed-profile vendor's salvage-sell path (`TrySellRecoveredSalvage`) behind player input via the S3-01 harness. Introduce a thin vendor adapter MonoBehaviour that implements `IPlayerInteractTarget` and, on dispatch, calls `vendor.TrySellRecoveredSalvage(out int creditedCopper)`. The adapter mounts on the `M3_CourtVendor` GameObject in `_DevEntry.unity:382`. `M3LootTableFixedProfileVendor.cs` has zero diff in this story; the S2-M3-03 closure (`25c94ee`) is preserved verbatim. The buy-side (`TryPurchaseFixedVendorGood` at :119) is **explicitly out of slate scope** — the player is not required to purchase anything to close the Tier-1 objective loop. Telemetry records two events per successful sale: `vendor_salvage_sold` (the sale happened) and `vendor_sell_copper_applied` (the currency credit happened), both with `player_driven` source attribution.

**Governing decisions** (DECISIONS.md):

| D-entry | Status | Usage |
|---|---|---|
| D001 (`DECISIONS.md:14`) | Locked | Unity 6.3 LTS + C# + URP |
| D003 (`DECISIONS.md:51`) | Locked | Tier 1 single-player offline — no Save/Load of currency (M4); session-local invariant from S2-M3-03 holds |
| D012 (`DECISIONS.md:342`) | Locked | T1 combat-feel validated |
| D016 (`DECISIONS.md:554`) | Locked | No new systems, reuse-not-rebuild — **D016 RED FLAG** for any rewrite of the F4 fixed-profile vendor formula |

**Sprint 3 feedback rule** (`sprint-3.md:85`): every feedback element passes trigger + direction tests. Vendor feedback acknowledges the sale (e.g., "+7 copper") — that is a *result* acknowledgement, not a *next-step* routing hint, so it passes the direction test. Reject any feedback that suggests "now go buy something" or "now go sell more" or surfaces the vendor's buy-side as an affordance the player should explore.

**Architecture Module**: Gameplay / Vendor Interaction Adapter (new thin adapter component on the M3 vendor anchor)
**Engine**: Unity 6.3 LTS
**Engine Risk**: LOW (only core MonoBehaviour + already-tested M3 API surface; no URP / UI Toolkit / DOTS / deprecated surface)

**Surfaces reused** (do not re-author):
- S3-01 harness dispatch interface: `IPlayerInteractTarget.TryInteract(playerActorId, distanceMeters, out InteractContext context)`
- M3 fixed-profile vendor: `Assets/Scripts/M3LootTableFixedProfileVendor.cs`
  - `TrySellRecoveredSalvage(out int creditedCopper)` at :101 — calls `Session.TrySellSalvage(M3LootTableFixedProfileVendorData.SalvageItemId, quantity: 1, out result, out rejection)` internally. Returns `false` with `LastRejectionReason` if the session rejects (e.g., no salvage carried). On success, `creditedCopper = result.CreditedCopper` and `LastRejectionReason = ""`.
  - State observables (read-only, used in test assertions):
    - `CarriedCurrencyCopper` at :26 — increases by `creditedCopper` after a successful sale
    - `CarriesSalvage` at :54 — becomes `false` when the last salvage is sold
    - `CarriedItemSlotsUsed` at :28 — decreases by 1 per sale
- `M3_CourtVendor` scene anchor: `Assets/Scenes/_DevEntry.unity:382` (added in S2-M3-03)
- S2-M3-03 closure baseline: commit `25c94ee`

## Acceptance Criteria

- [ ] **S3-04-01**: A new thin adapter MonoBehaviour (proposed name: `M3VendorInteractTarget`) is created under `Assets/Scripts/`, implements `IPlayerInteractTarget` (from S3-01), and is attached to the `M3_CourtVendor` GameObject in `_DevEntry.unity:382`.
- [ ] **S3-04-02**: The adapter holds a serialized reference to a `M3LootTableFixedProfileVendor` instance and, on `TryInteract(...)`, calls `vendor.TrySellRecoveredSalvage(out int creditedCopper)`. The adapter returns the boolean result. The harness sees a uniform `InteractContext`; the adapter populates it with the sale outcome data (`creditedCopper`, post-sale `CarriedCurrencyCopper`, and the `SalvageItemId`).
- [ ] **S3-04-03**: `M3LootTableFixedProfileVendor.cs` has **zero diff** in this story. S2-M3-03 closure (`25c94ee`) is preserved verbatim. Same for `M3LootTableFixedProfileVendor.cs.meta`.
- [ ] **S3-04-04**: On a successful sale (Try* returned `true` with `creditedCopper > 0`), telemetry records **two** events:
  - `vendor_salvage_sold` with payload `{ vendorId, salvageItemId, quantity: 1, source: "player_driven", playerActorId, distanceMeters }` — the sale-happened event
  - `vendor_sell_copper_applied` with payload `{ vendorId, creditedCopper, newCarriedCurrencyCopper, source: "player_driven" }` — the currency-credited event

  Both events are required by the plan row at `sprint-3.md:69`. Emit order: `vendor_salvage_sold` first, `vendor_sell_copper_applied` second (the credit is a consequence of the sale). Note: unlike S3-03 AC-03's ordering (forced by `TryAcceptObjectiveFromNpc`'s internal call sequence at `M3ObjectiveStateRelicHandIn.cs:65→71`), this ordering is an **adapter-side contract decision** — both events fire after `TrySellRecoveredSalvage` returns, so order is chosen, not forced. The adapter emits in the documented order; the test asserts the order; S3-06's fixture inherits it as established contract.
- [ ] **S3-04-05**: Post-sale state assertions: `vendor.CarriedCurrencyCopper` has increased by exactly `creditedCopper`; `vendor.CarriedItemSlotsUsed` has decreased by exactly 1; if the vendor held exactly one salvage pre-sale, `vendor.CarriesSalvage` is now `false`; if more than one, `CarriesSalvage` remains `true` and a second `TryInteract` call sells another unit.
- [ ] **S3-04-06**: On a failed sale (Try* returned `false`, e.g., no salvage carried), the adapter returns `false`; the harness's interact-blocked feedback fires (per S3-01 AC-08); no `vendor_salvage_sold` event fires; no `vendor_sell_copper_applied` event fires; `vendor.CarriedCurrencyCopper` is unchanged; `vendor.LastRejectionReason` is populated by the M3 layer. The blocked feedback does NOT explain why (e.g., "no salvage to sell") — that would be a routing/diagnostic hint forbidden by the Sprint 3 feedback rule. The player's "I tried and nothing happened" feedback is enough at greybox; the M3 rejection reason is captured in telemetry for the implementer/QA, not surfaced to the player.
- [ ] **S3-04-07**: Sale feedback at the harness layer acknowledges the action's *result* (e.g., a "+N copper" floating number, brief tone, or equivalent) — it does NOT surface buy-side affordances ("you have N copper, buy a fixed good!"), does NOT advertise the existence of `TryPurchaseFixedVendorGood`, does NOT show the buy-side inventory list, does NOT route the player anywhere. The sale acknowledgement is a *what just happened* answer, not a *what next* hint (Sprint 3 feedback rule direction test).
- [ ] **S3-04-08**: Player-driven path is end-to-end: harness keypress → harness raycast/distance-check finds the `M3_CourtVendor`-mounted adapter → harness invokes `adapter.TryInteract(...)` → adapter calls `vendor.TrySellRecoveredSalvage(...)` → on success, the two telemetry events fire and the harness's interact-fired feedback (per S3-01 AC-06) plays. No runner-side shortcut path that bypasses the harness.

## Implementation Notes

- **S3-03 dependency**: this story cannot start until S3-03 closes. The vendor's session has no salvage until `TryResolveObjectiveLoot` runs (which S3-03's relic adapter triggers). Without salvage, every sale dispatch is the AC-06 blocked path — useful for testing that path, but not for testing the success path.
- **Hardcoded quantity:1 per sale**: the M3 vendor's `TrySellRecoveredSalvage` internally hardcodes `quantity: 1` (line 106 of `M3LootTableFixedProfileVendor.cs`). One interact = one salvage sold = one currency credit. If the player has multiple salvage, multiple interacts are required. This is M3-system behavior, not adapter design — do not work around it by looping the Try* in a single dispatch. The adapter dispatches once per player interact.
- **Buy-side is explicitly out**: `TryPurchaseFixedVendorGood` exists at line 119 and is fully functional, but the plan (`sprint-3.md:69`) says "the player is not required to buy to close the loop" and the slate has no buy-side story. The adapter must NOT implement a second dispatch path for purchasing, must NOT expose a buy verb, and must NOT surface buy-side affordances in feedback. A future Sprint-3+ story can wire the buy side; this story does not.
- **No currency persistence**: `vendor.PersistsCurrencyAtRest` (line 50) is exposed as a property for the S2-M3-03 boundary check and remains `false` at T1. Currency resets on Play Mode restart. This is M4 (Save/Load) territory and intentionally out of scope.
- **Telemetry vocabulary**: this story introduces two new event names (`vendor_salvage_sold`, `vendor_sell_copper_applied`). Keep names stable for S3-06's end-to-end assertion vocabulary.
- **Vendor has two terminal telemetry shapes per dispatch** (matters for S3-06's fixture):
  1. **Sale success** — `vendor_salvage_sold` + `vendor_sell_copper_applied` both fire (AC-04, AC-05); harness interact-fired feedback plays.
  2. **Sale blocked** — no events fire (no salvage carried); only harness interact-blocked feedback plays (AC-06).

  Simpler than S3-03's relic adapter (no partial-success branch — there's no second internal call that can fail after the first succeeds; `TrySellRecoveredSalvage` is atomic at the M3 layer).
- **No DateTime.UtcNow**, scene discipline (touches `_DevEntry.unity` for adapter attachment), style gate — same as S3-01/S3-02/S3-03.

## Out of Scope

- No rewrite of `M3LootTableFixedProfileVendor.cs` (S2-M3-03 zero-diff invariant; **D016 RED FLAG**)
- No buy-side wiring — `TryPurchaseFixedVendorGood` is not dispatched, not exposed via the adapter, not surfaced in feedback
- No tuned economy / coin-pacing claim / `CoinFaucetProjection_T1` / faction-rank goods / token buying / arbitrage / reputation discount / limited-time rotation / stock simulation / dynamic pricing — all S2-M3-03 boundary flags (`Has*Hook` properties at lines 34–48) remain `false`
- No currency persistence at rest (`PersistsCurrencyAtRest` stays `false`; M4 deferred behind Sprint 3)
- No multi-vendor support (one vendor, `M3_CourtVendor`)
- No vendor UI panel, inventory list, price display, or "what's for sale" screen (greybox; the sale-result acknowledgement is the only player-facing surface)
- No quest log entry on sale; no minimap pin on vendor; no glow/outline on vendor; no advertising affordance (Pillar 2)
- No Save/Load of currency or vendor state (M4 deferred)
- No faction reaction to selling salvage (M5 deferred)
- No progression seed routing through loot RNG (`UsesProgressionSeedAsLootRng` stays `false`)
- No human-play feel acceptance criterion on this story (sits only on S3-06 per `sprint-3.md:141`)

## QA Test Cases

### Integration test (Unity Play Mode / batchmode runner)

**Test setup** (shared): Unity 6.3 LTS (`6000.3.14f1`) editor batchmode; `_DevEntry.unity` loaded; S3-01 harness, S3-02 NPC adapter, S3-03 relic adapter + state-routing NPC adapter, and S3-04 vendor adapter all wired; player marker positioned at known coordinates relative to `M3_CourtVendor`.

**Test S3-04-T1: vendor adapter presence and binding (AC-01, AC-02)**
- Given: `_DevEntry.unity` is loaded.
- When: Play Mode enters.
- Then: the `M3VendorInteractTarget` MonoBehaviour is present on the `M3_CourtVendor` GameObject and is registered with the S3-01 harness as an `IPlayerInteractTarget`. Its `M3LootTableFixedProfileVendor` reference resolves to the vendor instance from S2-M3-03.
- Edge cases: adapter component disabled → not registered; vendor reference null → adapter logs a clear setup error and remains harmless (no NullReferenceException on dispatch).

**Test S3-04-T2: sale success path (AC-04, AC-05, AC-08)**
- Given: pre-state where `vendor.CarriesSalvage == true` and `vendor.CarriedCurrencyCopper == C0` (some baseline value; in practice, salvage gets carried via S3-03's `TryResolveObjectiveLoot` resolving the M3 objective loot table). Record `slotsBefore = vendor.CarriedItemSlotsUsed`.
- When: player marker positioned at `M3_CourtVendor` within harness threshold; harness keycode fires.
- Then: adapter calls `vendor.TrySellRecoveredSalvage(out int creditedCopper)`; returns `true`; `creditedCopper > 0` (S2-M3-03's smoke recorded `salvage_sale_credited_copper=7` as a reference value; test asserts `> 0`, not a specific number, to remain robust to authored-data tuning); two telemetry events fire in order — `vendor_salvage_sold` then `vendor_sell_copper_applied`; post-sale: `vendor.CarriedCurrencyCopper == C0 + creditedCopper`; `vendor.CarriedItemSlotsUsed == slotsBefore - 1`; harness interact-fired feedback plays.
- Edge cases: player has multiple salvage units (loot resolution yielded > 1) → first interact sells one (`CarriesSalvage` stays true if more remain); second interact sells another; third interact (after all salvage sold) follows the AC-06 blocked path.

**Test S3-04-T3: sale blocked — no salvage carried (AC-06)**
- Given: pre-state where `vendor.CarriesSalvage == false` (no salvage in inventory — either pre-S3-03 state or post-sell-all state). Record `currencyBefore = vendor.CarriedCurrencyCopper`.
- When: player interacts with vendor.
- Then: adapter calls `vendor.TrySellRecoveredSalvage(...)`; returns `false`; `creditedCopper == 0`; no `vendor_salvage_sold` event; no `vendor_sell_copper_applied` event; `vendor.CarriedCurrencyCopper == currencyBefore` (unchanged); `vendor.LastRejectionReason` is populated (M3 layer sets it; the test verifies the rejection reason is non-empty but does NOT assert specific text — that's M3-internal behavior, not contract); harness interact-blocked feedback plays.
- Edge cases: rapid repeated interact when blocked → each fires blocked feedback, no events accumulate, `LastRejectionReason` may be overwritten on each call (M3 behavior).

**Test S3-04-T4: feedback rule compliance (AC-07)**
- Given: any of T2 (success) or T3 (blocked) dispatch.
- When: telemetry and runner screen-capture are inspected.
- Then: success path acknowledgement contains the *result* (creditedCopper amount or sale confirmation) but NOT buy-side text — no rendered string matches the deny-pattern list extended for vendor scope: `"buy"`, `"purchase"`, `"for sale"`, `"in stock"`, `"shop"`, `"merchant has"`, `"now you can"`, `"go check"` (finalized in the story's verification fixture, building on S3-01-T3 / S3-03-T8 deny patterns). Blocked path acknowledgement does NOT contain explanatory text about why ("no salvage", "nothing to sell", "come back when..."); the player gets a simple "tried and nothing" cue.

**Test S3-04-T5: M3 vendor zero-diff invariant (AC-03)**
- Given: pre- and post-implementation source-tree snapshots of `Assets/Scripts/M3LootTableFixedProfileVendor.cs` and `Assets/Scripts/M3LootTableFixedProfileVendor.cs.meta`.
- When: `git diff --stat` is run.
- Then: zero lines changed in either file. Diff stat output committed as evidence.
- Edge cases: none (binary invariant).

**Test S3-04-T6: buy-side absence invariant (Out of Scope)**
- Given: the new vendor adapter source and the harness's registered targets.
- When: source scan + runtime probe.
- Then: source scan over the adapter file finds zero references to `TryPurchaseFixedVendorGood`; runtime probe confirms no second dispatch path or second interact verb on the vendor adapter; harness's interact dispatch with the vendor target produces exactly one Try* call (the sell call), not two.
- Edge cases: a future story may add a buy adapter — this test guards specifically against the buy path appearing in S3-04's scope; a separately-wired buy adapter in a later story would not break this test because the test scans the S3-04 vendor adapter file, not the harness registry.

### M2 preservation reruns (additional required evidence)

- `M2SingleTrashLoop` smoke — PASS, exit 0
- `M2LinkedTrashOverpull` smoke — PASS, exit 0
- `M2NamedBlockerBoundary` smoke — PASS, exit 0

See "M2 preservation rerun execution note" below.

## Test Evidence

**Required evidence**: `tests/evidence/S3-04/verification.md`

Companion artifacts:
- `tests/evidence/S3-04/unity-player-driven-vendor-[YYYYMMDD]-smoke.md` (story-specific Unity batchmode runner output covering T1–T6)
- `tests/evidence/S3-04/m2-02-preservation-[YYYYMMDD]-smoke.md`
- `tests/evidence/S3-04/m2-03-preservation-[YYYYMMDD]-smoke.md`
- `tests/evidence/S3-04/m2-04-preservation-[YYYYMMDD]-smoke.md`
- `tests/evidence/S3-04/m3-loot-vendor-zero-diff-[YYYYMMDD].txt` (proves `M3LootTableFixedProfileVendor.cs` + .meta zero diff)
- `tests/evidence/S3-04/buy-side-absence-scan-[YYYYMMDD].txt` (proves zero `TryPurchaseFixedVendorGood` references in the new vendor adapter file)
- `dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — Combat regression baseline must hold (current: 189/189)
- T1 negative-scope scan over changed files — zero matches expected
- `git diff --check` — clean
- `.githooks/pre-commit` — `[pre-commit] OK`
- `dotnet format --verify-no-changes` — PASS

**Evidence status**: Not started

**Vendor adapter telemetry-shape summary in verification.md**: enumerate the two terminal telemetry shapes (sale success / sale blocked) with example payloads, so the S3-06 end-to-end fixture author has the vocabulary catalog without re-deriving it. Simpler than S3-03's three shapes — no partial-success branch because `TrySellRecoveredSalvage` is atomic at the M3 layer.

**M2 preservation rerun execution note** (per `m2_melee_rng_not_reset`): three M2 preservation smokes cannot be chained in a single Unity batchmode invocation. Run each in its own `Unity.exe -batchmode -executeMethod ...` invocation with its own `-gravenspireEvidencePath` override. Pattern established in S2-M3-04 closure.

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| `S3-01` | Harness + `IPlayerInteractTarget` interface | Done |
| `S3-03` | Vendor's session has no salvage until `TryResolveObjectiveLoot` runs (triggered by S3-03's relic adapter). Without salvage, only the AC-06 blocked path can be tested. | Done |

**Sprint-level pre-condition (tracked):** `dotnet format` setup — same as S3-01/S3-02/S3-03.

## Blockers

S3-01 and S3-03 must close before this story enters `/dev-story`. No design blockers; all governing D-entries Locked. The buy-side scope question is unambiguously settled by the plan row ("the player is not required to buy to close the loop") and is enforced by AC-07 + T6.

Watch items (not blockers):
- `m2_melee_rng_not_reset` — three M2 smokes require separate invocations
- `m2_02_runner_date_hardcoded` — no new hardcoded dates in runners
- `m2_renderer_material_property_access` — adapter feedback renderer hot-path discipline
- `control_manifest_absence_pre_existing` — Manifest Version `Unavailable` per fallback
- Format Gate — see Dependencies
- **Buy-side temptation watch**: the vendor exposes a fully-functional `TryPurchaseFixedVendorGood` (line 119). Reviewer rejects any PR that wires it in this story. No future story should backdoor it via S3-04's adapter.
