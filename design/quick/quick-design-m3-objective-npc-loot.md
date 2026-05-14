# Quick Design: M3 Objective + NPC + Loot

**Type:** Addition
**System:** First District authored objective bridge
**Date:** 2026-05-14
**Spec path:** `design/quick/quick-design-m3-objective-npc-loot.md`
**Sprint:** 2
**Milestone:** M3 - Objective + NPC + Loot
**Confidence:** Medium-high for milestone shape; medium for exact implementation split because the M3 fixed-profile vendor leans on the Inventory & Item Economy GDD's vendor rules while the wider Inventory implementation stays parked behind a pre-spec.

## Change Summary

M3 adds the minimum authored reason to play the First District slice after the M2 combat camp loop: one named NPC frames a small objective, the player resolves it through the existing M2 pull/fight/med loop, one restrained loot table produces a recoverable objective item plus sellable salvage, the player returns the objective item to the named NPC, and one fixed-profile vendor closes the loop by buying that salvage.

M3 does not add Save/Load, a visible faction consequence, a tuned vendor economy, full Inventory implementation, full NPC scheduling, Dialogue System UI, faction reputation mutation, companion behavior, networking, or live LLM dialogue. The M3 vendor is blockout-grade: it proves the vendor mechanism, not a balanced economy. M4 owns persistence. M5 owns visible faction/world-state consequence.

## Source List

Verification method: live repository reads in `N:\GravenSpire\.claude\worktrees\sharp-chebyshev-ad16e1` at `9367d10` using `git status --short --branch`, `git log --oneline -8`, `Get-Content`, `rg`, `Test-Path`, and targeted source reads on 2026-05-14.

| Source | Use |
| --- | --- |
| `production/sprints/sprint-2.md:10` through `production/sprints/sprint-2.md:25` | Sprint 2 First District target and working example. |
| `production/sprints/sprint-2.md:47` through `production/sprints/sprint-2.md:49` | M3 proof statement and M4/M5 sequencing. |
| `production/sprints/sprint-2.md:72` through `production/sprints/sprint-2.md:78` | Sprint 2 cuts: no extra classes, huge world, deep economy, broad companion system, or broad faction simulation. |
| `production/sprint-status.yaml:5` | Current Sprint 2 goal. |
| `production/sprint-status.yaml:15` through `production/sprint-status.yaml:20` | M2 complete; next active command is `/quick-design M3-objective-npc-loot`. |
| `production/sprint-status.yaml:38` through `production/sprint-status.yaml:44` | Carryovers affecting M3: human-play evidence, presentation threshold, scenario-smoke abstraction, runner noise filter, and control-manifest absence. |
| `production/session-state/active.md:4` through `production/session-state/active.md:10` | Current-task header confirms M3 quick-design next. |
| `production/session-state/active.md:77` through `production/session-state/active.md:120` | Contradictory stale inner routing sections; do not use these as stronger routing truth. |
| `production/session-state/active.md:291` through `production/session-state/active.md:295` | `/story-done` extract confirms M2 complete and M3 quick-design next. |
| `design/quick/quick-design-m2-combat-camp-loop.md:13` | M2 explicitly did not add objective, NPC, loot, vendor/stash, save/load, or faction-consequence work. |
| `design/quick/quick-design-m2-combat-camp-loop.md:188` through `design/quick/quick-design-m2-combat-camp-loop.md:189` | Those surfaces were M2 non-goals and are the M3/M4/M5 boundary. |
| `design/gdd/game-concept.md:125` through `design/gdd/game-concept.md:136` | Camp and return-to-city loop: named spawn, loot, sell/bank/deposit tokens. |
| `design/gdd/game-concept.md:315` through `design/gdd/game-concept.md:321` | T1 MVP hypothesis and one-faction/templated-dialogue target. |
| `DECISIONS.md:32` through `DECISIONS.md:42` | FishNet deferred; T1 is strictly single-player offline. |
| `DECISIONS.md:48` through `DECISIONS.md:61` | D003 T1 offline/local-save/no-netcode/no-live-LLM boundary. |
| `DECISIONS.md:68` through `DECISIONS.md:79` | D004: T1 dialogue is templated; live LLM is T3. |
| `design/gdd/npc-system.md:21` | T1 NPC scope is narrow and templated; live LLM is not part of NPC System. |
| `design/gdd/npc-system.md:50` through `design/gdd/npc-system.md:56` | Named NPC identity and T1 content envelope. |
| `design/gdd/npc-system.md:72` through `design/gdd/npc-system.md:80` | Named NPC specificity and Dialogue boundary. |
| `design/gdd/npc-system.md:104` through `design/gdd/npc-system.md:105` | NPC `ActiveInZone` -> `Interacting` state model for intentional player interaction. |
| `design/gdd/npc-system.md:388` through `design/gdd/npc-system.md:407` | No marker affordances; T1 templated dialogue context only. |
| `design/gdd/npc-system.md:459` | Initial named NPC roster is still open before Dialogue System GDD. |
| `design/gdd/inventory-item-economy.md:7` | Inventory full review disposition: parked behind implementation pre-spec. |
| `design/gdd/inventory-item-economy.md:66` through `design/gdd/inventory-item-economy.md:90` | Legal T1 item categories, currency/vendor/token/loot boundaries, and no T1 Inventory save barrier (synchronous/atomic mutations). |
| `design/gdd/inventory-item-economy.md:80` | Rule 13: T1 vendors are fixed-profile only - one CityHub vendor profile, authored buy/sell tables, constant prices. |
| `design/gdd/inventory-item-economy.md:193` through `design/gdd/inventory-item-economy.md:217` | F4 vendor salvage sale formula and authored salvage value bands. |
| `design/gdd/inventory-item-economy.md:530` through `design/gdd/inventory-item-economy.md:547` | Currency and fixed-profile vendor acceptance hypotheses (H-INV-CUR / H-INV-VEN). |
| `design/gdd/inventory-item-economy.md:626` through `design/gdd/inventory-item-economy.md:627` | CurrencyContainer loot-table entries require coin-faucet projection proof. |
| `design/gdd/inventory-item-economy.md:652` through `design/gdd/inventory-item-economy.md:655` | Combat-to-Inventory loot eligibility and Inventory implementation pre-spec blockers. |
| `design/gdd/combat-core.md:41` through `design/gdd/combat-core.md:43` | Combat Core owns the existing loop but not loot economy or named/boss soloability. |
| `design/gdd/combat-core.md:194` | Combat kill-credit emits stable refs; Inventory consumes loot hooks later. |
| `design/gdd/combat-core.md:267` through `design/gdd/combat-core.md:268` | Zone Control and Inventory are downstream consumers, not Combat-owned implementation surfaces. |
| `design/gdd/combat-core.md:431` | Existing named blocker fixture source reference. |
| `design/gdd/combat-core.md:912` | Combat Core non-goal: no loot tables, item drops, item stat schema, currency economy, or equipment slot legality. |
| `design/gdd/systems-index.md:29` through `design/gdd/systems-index.md:55` | System status map: NPC approved; Inventory parked; Faction/Dialogue/HUD not started. |
| `design/gdd/systems-index.md:108` through `design/gdd/systems-index.md:145` | Dependency layering for NPC, Combat, Inventory, Faction, Dialogue, and UI surfaces. |
| `Assets/Scripts/M2SingleTrashMedLoopController.cs:315` through `Assets/Scripts/M2SingleTrashMedLoopController.cs:451` | Current M2 controller already owns three scenario smoke paths. |
| `Assets/Scripts/M2SingleTrashMedLoopController.cs:1543` through `Assets/Scripts/M2SingleTrashMedLoopController.cs:1856` | Named blocker path shows why M3 should not add a fourth parallel block directly. |
| `Assets/Editor/GravenspireM2SingleTrashLoopBuilder.cs:32` through `Assets/Editor/GravenspireM2SingleTrashLoopBuilder.cs:52` | Current `_DevEntry.unity` M2 object builder and anchor naming. |
| `tests/evidence/S2-M2-04/verification.md:22` through `tests/evidence/S2-M2-04/verification.md:26` | M2 final evidence and reusable proof pattern: mechanical telemetry plus prior-loop preservation. |

## Source Arbitration Note

`production/session-state/active.md` has a current header that points to M3, but its "Files Being Worked On" and "Next Skill to Run" sections still contain stale S2-M2-04 routing. This spec treats `production/sprint-status.yaml:15` through `:20` and the `/story-done` extract at `production/session-state/active.md:291` through `:295` as the stronger current routing evidence.

## Assumptions

- M3 may extend `_DevEntry.unity` because Sprint 2 is still using that scene as the controlled playable-slice proving ground.
- Working labels such as `Mournwall`, `Caretaker`, `Relic`, and `CourtVendor` are implementation handles, not final narrative names.
- M3 closes the loop with a fixed-profile vendor, per the explicit milestone decision. The M3 vendor is blockout-grade: it implements the vendor mechanism against the Inventory GDD's existing fixed-profile vendor rules (`design/gdd/inventory-item-economy.md:80`, F4 at `:193` through `:217`, H-INV-VEN at `:542` through `:547`). It does not implement, claim, or tune a full economy; coin pacing / `CoinFaucetProjection_T1` and the full Inventory persisted schema remain parked and are explicit promotion triggers.
- M3 can use session-only runtime state for objective, carried objective item, carried salvage, and carried copper evidence. M4 will decide persistence.
- M3 evidence should keep the M2 pattern: mechanical telemetry proves design intent, while human-play notes are treated as qualified supplements when blockout presentation limits feel judgment.
- Story files and routing state are not updated by this quick design. Those belong to the next `/create-stories` pass.

## Facts

- Sprint 2 requires one named NPC, one objective, one loot table, and one vendor or stash as part of the First District target.
- M3 specifically proves: "One named NPC gives or frames an objective; one loot table and one vendor or stash close the loop."
- M2 deliberately excluded those surfaces, and S2-M2-04 closed without adding loot, objectives, NPC, faction consequence, Save/Load, companion, or extra-class behavior.
- T1 remains offline, single-player, Cleric-only, no netcode, no account system, no server backend, and no live LLM calls.
- NPC System is approved as a framework for named NPC identity and templated interaction context, but the final T1 named NPC roster is still open.
- Inventory & Item Economy is still draft-parked: full Inventory implementation requires `INV-OQ-05` pre-spec closure and fresh full review.
- The Inventory GDD already defines T1 vendors as fixed-profile only - one CityHub vendor profile, authored buy/sell tables, constant prices (`design/gdd/inventory-item-economy.md:80`) - with the F4 vendor salvage sale formula and authored value bands at `:193` through `:217`. M3's vendor uses those rules; what stays parked is the wider Inventory implementation, not the vendor contract itself.
- Inventory GDD allows T1 categories `Equipment`, `Consumable`, `Salvage`, `FactionToken`, and `CurrencyContainer`. Ordinary mob kills do not produce currency in T1; coin enters only through vendor sale of `Salvage`, authored `CurrencyContainer` pickups, or approved fixture rewards. CurrencyContainer loot-table entries require coin-faucet projection proof.
- All T1 Inventory mutations are synchronous and atomic; the Inventory GDD declares no T1 save barrier. An M3 vendor transaction or loot pickup that cannot stay synchronous/atomic triggers an ADR-0002 `InventorySaveBarrier` amendment.
- Combat Core can provide stable defeated source refs and kill context, but it does not own loot tables, item drops, currency economy, or equipment legality.
- The M2 controller is already too large for a fourth scenario added by direct copy. M3 story-breaking should include a pre-M3 scenario-smoke abstraction or a new M3 controller composed around shared M2 loop helpers.

## M3 Loop Definition

The M3 player loop is:

1. Start in the current First District blockout with the M2 combat camp loop intact.
2. Find and intentionally interact with one named NPC near the safe side of the district.
3. The NPC frames a concrete objective through templated text only: recover a marked relic from the camp and bring it back to the named NPC.
4. Objective state moves from `NotIntroduced` to `Accepted` with no quest marker, minimap target, auto-pathing, overhead icon, or proximity bark.
5. The player uses the existing M2 pull/fight/med loop to reach a fixed pickup/chest/relic point.
6. A restrained M3 loot table resolves the objective item plus sellable salvage through stable authored ids, not through Combat-owned loot logic.
7. The player returns to the named NPC and hands in the marked relic; the objective transitions to `Complete` through templated acknowledgement text.
8. The player visits the fixed-profile vendor and sells the salvage for copper through the F4 salvage-sale formula, closing the return-to-town loop.

The minimum complete target is not "make a quest system" or "build an economy." It is: named person -> reason to enter the camp -> recover one authored thing -> return it to that person -> sell the salvage at a fixed-profile vendor, with M2 combat still preserved and no M4/M5 leakage.

## Design Choice: Fixed-Profile Vendor

M3 closes the milestone loop with a fixed-profile vendor, per the explicit decision recorded for this quick design.

Scope of the M3 vendor:

- It implements the vendor mechanism against the Inventory GDD's existing fixed-profile vendor rules: one authored vendor profile, authored buy/sell tables, constant prices (`design/gdd/inventory-item-economy.md:80`), the F4 salvage-sale formula `vendor_sell_copper = max(1, floor(nominal_value_copper * salvage_sell_multiplier))` with `salvage_sell_multiplier = 0.15` (`:193` through `:208`), buy prevalidation before currency debit (H-INV-VEN-01), and no dynamic pricing / stock simulation / reputation discount / rotation / arbitrage (H-INV-VEN-02).
- The vendor transaction is synchronous and atomic, consistent with the Inventory GDD's no-T1-save-barrier rule.
- Currency enters M3 only through vendor sale of `Salvage`; ordinary mob kills do not produce currency.

What the M3 vendor explicitly does not do:

- It does not claim or tune a balanced economy. Any copper-per-hour, vendor-affordability, or coin-pacing claim needs `CoinFaucetProjection_T1`, which stays parked. M3 proves the vendor mechanism, not the economy.
- It does not implement the full Inventory persisted schema, currency-at-rest persistence, or save/load of vendor state. M3 vendor and inventory state is session-local.
- It does not add a `CurrencyContainer` loot-table entry, a buy-side price formula, faction-rank goods, or token buying.

Promotion trigger: if the M3 vendor implementation requires the full Inventory persisted schema, currency-at-rest persistence, a coin-pacing claim, or anything beyond the Inventory GDD's existing fixed-profile vendor rules, M3 has exceeded quick-design scope and must stop for the Inventory pre-spec / `/design-system` / `/architecture-decision` path before any further vendor code.

## Content Sketch

### Named NPC

Working id: `M3_Caretaker_T1`

Working presentation: a cemetery caretaker or local Court-aligned custodian near the safe side of the district. The sprint plan's "Mournwall Cemetery District" example is useful tone, but not a final naming lock.

Rules:

- Reads as a named NPC through specificity: posture, material treatment, position, and repeatable behavior.
- No overhead name, quest icon, exclamation point, minimap marker, glow, outline, special camera, or proximity bark.
- Interaction requires intentional proximity/selection, consistent with the NPC System `ActiveInZone` -> `Interacting` model.
- Provides `NpcInteractionContext`-shaped data and a `dialogueTemplateSetId`-shaped handle if implemented, but does not require the full Dialogue System.
- Uses templated text only. No LLM call, no LLM memory, no moderation dependency.
- Frames the objective and accepts the relic hand-in that completes it; both use templated text.
- Does not implement full schedule catch-up, persistence, faction reaction, companion capability, or party behavior in M3.

### Objective

Working id: `M3_RecoverMarkedRelic_T1`

Objective states (kept minimal - a tracker, not a quest framework):

- `NotIntroduced` - the NPC has not framed the objective yet.
- `Accepted` - the player has intentionally received the framing from the NPC; the marked relic pickup is authored-available.
- `RelicRecovered` - the player has recovered the marked relic into session-local carried state.
- `Complete` - the player has returned the marked relic to the named NPC.

Rules:

- The objective is session-local for M3.
- The objective must be legible without markers: NPC text, prop placement, and return-to-NPC behavior carry the information.
- The objective does not mutate faction reputation, Zone Control, NPC trust, or world state. M5 owns visible consequence.
- The objective does not persist through save/load. M4 owns persistence and repair-by-load rejection.
- Abandon/restart behavior may reset session-local M3 state, but it must not silently grant duplicate objective items.

### Loot Table

Working id: `M3_MournwallRelicTable_T1`

Default route: fixed placed pickup or chest, not kill-credited random drops.

Allowed default entries:

| Entry | Category | Required? | Notes |
| --- | --- | --- | --- |
| `CourtMarkedRelic_T1` | `FactionToken` | Yes | Objective item. Possession only; no reputation mutation. Returned to the named NPC to complete the objective. |
| `GraveDust_Salvage_T1` or equivalent | `Salvage` | Yes | Sellable salvage; the fixed-profile vendor's input. Authored `nominal_value_copper` band per F4. Not gear progression. |

Rules:

- Loot table resolution uses authored ids and deterministic test seeds.
- No `combat_actor_id` in loot lookup or item records.
- No reuse of `kill_weight_seed` as loot RNG.
- No CurrencyContainer entry by default. If a CurrencyContainer appears in M3, the story must include coin-faucet projection proof or explicitly classify it as a fixed world placement with authored value.
- No item level, rarity color, gear score, random affixes, set bonuses, stat-bearing equipment, or loot spectacle.
- No kill-credited loot drops until the Combat -> Inventory loot eligibility contract is explicitly designed or narrowly pre-specified.

### Fixed-Profile Vendor

Working id: `M3_CourtVendor_T1`

Rules:

- Buys `Salvage` (for example `GraveDust_Salvage_T1`) from the player, applying the F4 formula `vendor_sell_copper = max(1, floor(nominal_value_copper * salvage_sell_multiplier))` with `salvage_sell_multiplier = 0.15`.
- May expose a small authored buy table (`T1_CityHubVendorBuyTable_T1`-shaped) with constant prices, but M3 does not require the player to buy anything to close the loop.
- Prevalidates carried-slot/weight capacity before any currency debit (H-INV-VEN-01).
- No dynamic pricing, stock simulation, reputation discount, faction-rank goods, limited-time rotation, token buying, or arbitrage loop (H-INV-VEN-02).
- The transaction is synchronous and atomic; no partial debit, partial removal, or partial credit is observable.
- Credits `carried_currency_copper` in session-local state only; M3 does not persist currency at rest.
- Does not grant reputation, rank, access, title, NPC trust, or faction state.
- Does not act as account storage, bank, mail, player trade, or multi-character inventory.

## Data And Runtime Approach

M3 should prefer one small authored data file for the milestone, for example:

```text
data/first-district/m3-objective-npc-loot.json
```

Suggested top-level sections:

- `namedNpc`
- `objective`
- `lootTable`
- `vendor`
- `telemetryLabels`

This path is a proposal for the story split, not a GDD lock. If implementation chooses ScriptableObjects or a different data location, the story must cite the reason and preserve the same narrow data contract. The vendor's authored salvage value bands and buy table are blockout-grade authored constants; the Inventory GDD notes the authored data format/location is not yet locked (`INV-OQ-04`), so the M3 data file is explicitly a blockout proposal that the Inventory pre-spec may later relocate.

Runtime shape:

- Do not add a fourth parallel scenario-smoke block directly to `M2SingleTrashMedLoopController`.
- Either extract shared M2 scenario-smoke helpers first or create a small M3 controller that composes the existing M2 loop and records M3-specific objective/loot/vendor telemetry.
- Keep Combat Core formulas untouched.
- Keep item/objective/vendor state session-local and synchronous.
- Use story-specific Unity runner evidence under `tests/evidence/S2-M3-*/`.
- Keep M2 clean-loop, overpull, and named-blocker preservation checks in the M3 end-to-end runner.

## Allowed `_DevEntry.unity` Changes

Allowed for M3:

- Add `M3_ObjectiveLoopRoot`.
- Add `M3_Caretaker` named NPC anchor/marker.
- Add `M3_ObjectiveRelic` pickup/chest/relic marker.
- Add `M3_CourtVendor` vendor anchor/marker.
- Add small, restrained material/shape differences so the NPC, relic, and vendor are distinguishable in blockout.
- Add runtime-only scripts/components needed to frame the objective, resolve the pickup, complete the hand-in at the NPC, run the vendor salvage sale, and record evidence.

Not allowed for M3:

- Final quest log, minimap markers, exclamation points, overhead names, auto-pathing, "track objective" UI, or proximity bark instructions.
- A tuned vendor economy, coin-pacing claims, `CoinFaucetProjection_T1`-backed balance assertions, buy-side price formulas, or `CurrencyContainer` loot entries.
- Full Inventory implementation, the full Inventory persisted schema, currency-at-rest persistence, Save/Load persistence, or inventory repair-by-load behavior.
- Faction reputation mutation, Zone Control mutation, faction board changes, changed patrols, opened access, or other M5 visible consequence.
- Live LLM dialogue, LLM memory, moderation dependency, companion hire/follow/combat behavior, Warrior, Enchanter, networking, FishNet, server authority, accounts, cloud saves, PvP, or multiplayer.

## Acceptance Criteria Candidates

### M3-00 Scenario Smoke Handoff Cleanup

- [ ] M3 does not add a fourth parallel 300-400 line scenario-smoke block directly to `M2SingleTrashMedLoopController`.
- [ ] Shared scenario-smoke setup, telemetry capture, or runner helpers exist where needed before M3 adds objective-specific smoke.
- [ ] Existing M2 S2-M2-02, S2-M2-03, and S2-M2-04 scenario checks still pass after the cleanup.
- [ ] The `UnityEditor.Search.SearchInit` editor-startup noise filter is either shared or explicitly copied into any M3 runner that captures Unity logs.
- [ ] No gameplay behavior changes are introduced by the cleanup story.

### M3-01 Named NPC Objective Frame

- [ ] `_DevEntry.unity` contains one visible named NPC anchor/marker for the M3 objective frame.
- [ ] Intentional interaction with the NPC records an `NpcInteractionContext`-shaped event and a templated dialogue id or text key.
- [ ] The NPC frames the objective without quest markers, overhead names, glow, outline, minimap dots, auto-pathing, proximity barks, or live LLM calls.
- [ ] The NPC implementation is session-local and does not claim full NPC schedule, persistence, faction reaction, companion, or Dialogue System ownership.

### M3-02 Objective State And Relic Hand-In

- [ ] Accepting the objective transitions objective state deterministically from `NotIntroduced` to `Accepted`.
- [ ] The objective makes one relic/pickup/chest marker available through authored state, not through global quest polling or map markers.
- [ ] Recovering the relic transitions objective state to `RelicRecovered` in session-local carried state.
- [ ] Returning the relic to the named NPC transitions objective state to `Complete`.
- [ ] Re-entering the M2 combat loop still works after objective state changes.
- [ ] Objective state remains session-local and synchronous; no Save/Load persistence, faction consequence, or repair-by-load path is added.

### M3-03 Loot Table And Fixed-Profile Vendor

- [ ] One M3 loot table resolves `CourtMarkedRelic_T1` and sellable `Salvage` from authored data.
- [ ] Loot lookup uses stable authored ids and contains no `combat_actor_id`, runtime actor handle, threat table, damage roll, or Combat current-resource field.
- [ ] The loot table does not reuse `kill_weight_seed` as loot RNG.
- [ ] The default M3 table contains no CurrencyContainer entry unless the story also supplies coin-faucet projection proof or classifies the entry as fixed world placement.
- [ ] The fixed-profile vendor buys `Salvage` and applies the F4 formula `vendor_sell_copper = max(1, floor(nominal_value_copper * 0.15))` to produce copper.
- [ ] The vendor prevalidates carried capacity before any currency debit and exposes no dynamic pricing, stock simulation, reputation discount, rotation, or arbitrage hook.
- [ ] The vendor transaction is synchronous and atomic; no partially debited, partially removed, or partially credited state is observable.
- [ ] Vendor and currency state are session-local; M3 makes no `CoinFaucetProjection_T1`, copper-per-hour, or tuned-economy claim, and does not persist currency at rest.

### M3-04 End-To-End Objective Loop

- [ ] A story-specific Unity Play Mode or batchmode runner proves: named NPC frame -> objective accepted -> M2 combat loop preserved -> relic recovered -> relic returned to the NPC -> objective complete -> salvage sold at the fixed-profile vendor.
- [ ] The runner records mechanical telemetry for discovery, objective state transitions, loot resolution, relic hand-in, vendor salvage sale, and M2 loop preservation.
- [ ] Dotnet regression for Combat Core still passes.
- [ ] T1 negative-scope scan passes with classified doc hits only, if any.
- [ ] `git diff --check` passes.
- [ ] `.githooks/pre-commit` passes against the approved file batch.
- [ ] Human-play notes are captured for "did the objective give a real reason to do one more pull?", with presentation limitations classified rather than hidden.

## Proposed Story Split

| Story | Purpose | Primary evidence |
| --- | --- | --- |
| M3-00 Scenario Smoke Handoff Cleanup | Prevent M3 from growing the M2 controller by another parallel scenario block; share runner/log-noise handling before new objective smoke lands. | Existing M2 runners or focused smoke reruns; dotnet regression; no behavior-change diff review. |
| M3-01 Named NPC Objective Frame | Add one named NPC anchor and templated interaction frame. | Unity runner proving NPC anchor, intentional interaction, no marker affordances, no LLM. |
| M3-02 Objective State + Relic Hand-In | Add session-local objective state, make the relic available through authored state, and complete the objective on return to the NPC. | Unit or integration tests for state transitions plus Unity smoke for marker availability and hand-in. |
| M3-03 Loot Table + Fixed-Profile Vendor | Add restrained authored loot table and the fixed-profile vendor salvage-sale closure. | Data validation, loot/vendor unit tests (including F4), Unity smoke proving the salvage sale. |
| M3-04 End-To-End M3 Loop | Prove the whole M3 loop without breaking M2. | Story-specific Unity runner, dotnet regression, negative-scope scan, human-play note. |

`/create-stories` may merge M3-02 through M3-04 if the implementation stays small, but it should not merge M3-00 into player-facing feature work if the cleanup has a distinct risk profile.

## Evidence Plan

Each M3 implementation story should produce:

- `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` when Combat Core or M2 regression coverage is in scope.
- Story-specific Unity Play Mode or batchmode runner evidence under `tests/evidence/S2-M3-*/`.
- Data validation or unit tests for any authored M3 data file, including the F4 vendor salvage-sale formula.
- T1 negative-scope scan for FishNet, networking, server authority, PvP, accounts, cloud saves, live LLM, multiplayer, Warrior, Enchanter, broad companion behavior, Save/Load mutation, and faction consequence.
- `git diff --check`.
- `.githooks/pre-commit`.
- Human-play note for the end-to-end story, with blockout-presentation limitations called out explicitly.

Minimum end-to-end telemetry:

- `npc_anchor_present`
- `npc_interaction_intentional`
- `dialogue_template_id`
- `objective_state_sequence`
- `relic_available`
- `loot_table_id`
- `loot_result_item_ids`
- `relic_handed_in`
- `objective_complete`
- `vendor_salvage_sold`
- `vendor_sell_copper_applied`
- `m2_clean_loop_preserved`
- `m2_named_blocker_boundary_preserved`
- `no_save_load_state_written`
- `no_faction_consequence_applied`

## Deferred / Non-Goals

- No Save/Load. M4 owns persistence, reload, and no repair-by-load behavior.
- No visible faction consequence. M5 owns changed patrols, opened access, altered signage, board change, or faction presence shift.
- No full NPC schedule, session-resume catch-up, or persistent NPC record implementation beyond the minimal M3 named interaction surface.
- No production Dialogue System UI. Templated text or evidence-only dialogue output is enough for M3.
- No live LLM or LLM memory.
- No production Inventory implementation, full Inventory persisted schema, or currency-at-rest persistence unless `INV-OQ-05` is closed first.
- No tuned vendor economy. The M3 vendor proves the mechanism only; coin pacing, copper-per-hour, vendor affordability, and any `CoinFaucetProjection_T1`-backed claim are out of scope.
- No buy-side price formula, `CurrencyContainer` loot-table entry, faction-rank goods, or token buying at the M3 vendor.
- No kill-credited loot drops until the Combat -> Inventory loot eligibility contract is designed or narrowly pre-specified.
- No faction-token reputation meaning. Tokens are possession/hand-in only in M3.
- No gear progression, item stat schema, rarity colors, loot spectacle, item level, affixes, set bonuses, or gear score.
- No companion, Warrior, Enchanter, networking, FishNet, server authority, PvP, accounts, cloud saves, or multiplayer behavior.

## Affected Systems

| System | Impact | Action required |
| --- | --- | --- |
| M2 Unity runtime wrapper | Provides existing combat camp loop substrate. | Preserve clean-loop, overpull, and named-blocker checks. Do not grow controller by direct copy. |
| NPC System | Supplies named NPC identity/interact boundaries. | Implement only a narrow M3 anchor/context surface, including the relic hand-in; no full schedule/persistence. |
| Dialogue System | Future owner of dialogue content/UI. | Use templated text/key only; no full system, no LLM. |
| Inventory & Item Economy | Future owner of item truth, loot, and vendor legality; full implementation parked. | Use the existing fixed-profile vendor rules (Rule 13, F4, H-INV-VEN) blockout-grade; keep loot/vendor/currency session-local; promote if M3 needs the full Inventory schema or a tuned economy. |
| Combat Core | Existing loop and kill context source. | No formula changes; no Combat-owned loot. |
| World Structure | `_DevEntry.unity` remains the temporary scene. | No zone transition or save-on-transition work. |
| Save / Load | M4 owner. | No M3 persistence or repair-by-load. |
| Faction State / Reputation / Zone Control | M5 and later owners. | No consequence, reputation, or zone-control mutation in M3. |
| Layer 1 HUD / Dialogue UI | Future presentation owners. | Debug/evidence output allowed; no final UI promises. |

## QA Checks

- Verify this quick-design source remains at `design/quick/quick-design-m3-objective-npc-loot.md`.
- Before `/create-stories`, ensure every M3 story cites this spec and the relevant source lines above.
- During implementation, require M2 preservation evidence in the M3 end-to-end runner.
- Keep the M3 vendor blockout-grade: any production Inventory schema work, currency-at-rest persistence, or tuned-economy / coin-pacing claim is blocked until the Inventory pre-spec path is explicitly resolved.
- Preserve Sprint 2 sequencing: M3 objective/NPC/loot now, M4 Save/Load next, M5 faction consequence last.

## Promotion Warning

M3 remains quick-design scope only if it is a narrow playable-slice bridge.

Promote to `/design-system`, `/architecture-decision`, or an Inventory pre-spec/review path if implementation requires any of the following:

- full quest/objective framework;
- production Inventory save schema or currency-at-rest persistence;
- a tuned vendor economy, coin-pacing proof, or `CoinFaucetProjection_T1`-backed balance claim;
- a buy-side price formula or `CurrencyContainer` loot-table entry;
- kill-credited loot drops;
- item stat/equipment progression;
- full NPC scheduling/persistence;
- Dialogue System UI;
- faction reputation or Zone Control mutation;
- Save/Load integration;
- a vendor transaction or loot pickup that cannot stay synchronous/atomic (would require an ADR-0002 `InventorySaveBarrier` amendment);
- live LLM behavior.

## Next Action

Run:

```text
/create-stories M3-objective-npc-loot
```

Use this quick design as the input for M3 story-breaking. Do not update `production/sprint-status.yaml`, `production/session-state/active.md`, `production/sprints/sprint-2.md`, GDDs, or routing state until stories are actually opened under an approved `/create-stories` batch.
