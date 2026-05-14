# S2-M3-04 - End-To-End Objective Loop

**Status:** Blocked
**Sprint:** 2
**Priority:** Must Have
**Layer:** Gameplay / Unity Runtime
**Type:** Integration / Visual-Feel
**Estimate:** 1.0 days
**Manifest Version:** Sprint 2, 2026-05-14
**GDD:** `design/gdd/game-concept.md`; `design/gdd/npc-system.md`; `design/gdd/inventory-item-economy.md`
**Quick Design:** `design/quick/quick-design-m3-objective-npc-loot.md`
**Governing Decisions:** `DECISIONS.md` D001, D002, D003, D004, D012, D014
**Evidence:** `tests/evidence/S2-M3-04/verification.md`

## Routing Status

Blocked until `S2-M3-03` is complete. This story proves the full M3 loop:
named NPC frame -> objective accepted -> M2 combat loop preserved -> relic
recovered -> relic returned to NPC -> objective complete -> salvage sold at
fixed-profile vendor.

## Source Trace

- `design/quick/quick-design-m3-objective-npc-loot.md:281` through `:289`
  define M3-04 end-to-end acceptance candidates.
- `design/quick/quick-design-m3-objective-npc-loot.md:303` through `:331`
  define the M3 evidence plan and minimum telemetry.
- `design/quick/quick-design-m3-objective-npc-loot.md:291` through `:299`
  define the proposed M3 story split and this final proof story.
- `design/quick/quick-design-m3-objective-npc-loot.md:333` through `:346`
  preserve Save/Load, faction, Inventory, vendor, and future-system non-goals.
- `tests/evidence/S2-M2-04/verification.md:22` through `:26` provide the M2
  proof pattern: mechanical telemetry plus prior-loop preservation.

## Scope

Create story-specific Unity Play Mode or batchmode evidence proving the whole
M3 loop and preserving M2 behavior. This is a proof/integration story, not a
new system expansion story.

Planned implementation surface:

- M3 end-to-end runner or scenario entry.
- Mechanical telemetry for NPC, objective, relic, loot, vendor, and M2 preservation.
- Human-play note for whether the objective gave a real reason to do one more pull.

## Out Of Scope

- No new feature surface beyond wiring/proving M3-01 through M3-03 together.
- No Save/Load, M5 faction consequence, tuned economy, full Inventory schema,
  CurrencyContainer loot, kill-credited drops, final quest UI, Dialogue System
  UI, live LLM, companion, extra class, networking, FishNet, server authority,
  PvP, accounts, cloud saves, or multiplayer behavior.

## Acceptance Criteria

| ID | Criterion | Evidence |
| --- | --- | --- |
| `S2-M3-04-01` | A story-specific Unity Play Mode or batchmode runner proves the full M3 sequence from NPC frame through vendor salvage sale. | `tests/evidence/S2-M3-04/verification.md` |
| `S2-M3-04-02` | Runner telemetry includes `npc_anchor_present`, `npc_interaction_intentional`, `dialogue_template_id`, `objective_state_sequence`, `relic_available`, `loot_table_id`, `loot_result_item_ids`, `relic_handed_in`, `objective_complete`, `vendor_salvage_sold`, and `vendor_sell_copper_applied`. | `tests/evidence/S2-M3-04/verification.md` |
| `S2-M3-04-03` | Runner telemetry includes `m2_clean_loop_preserved` and `m2_named_blocker_boundary_preserved`. | `tests/evidence/S2-M3-04/verification.md` |
| `S2-M3-04-04` | Runner telemetry includes `no_save_load_state_written` and `no_faction_consequence_applied`. | `tests/evidence/S2-M3-04/verification.md` |
| `S2-M3-04-05` | Dotnet regression, T1 negative-scope scan, `git diff --check`, and `.githooks/pre-commit` pass before closure. | `tests/evidence/S2-M3-04/verification.md` |
| `S2-M3-04-06` | Human-play notes are captured for "did the objective give a real reason to do one more pull?", with presentation limitations classified rather than hidden. | `tests/evidence/S2-M3-04/human-play-20260514.md` |

## QA Test Cases

- **S2-M3-04-01**: Full sequence
  - Given: M3-01 through M3-03 are complete.
  - When: the M3 end-to-end runner executes.
  - Then: evidence records NPC frame, accepted objective, preserved M2 loop, relic recovery, NPC hand-in, objective complete, and vendor sale.
- **S2-M3-04-02**: Required telemetry
  - Given: runner output exists.
  - When: telemetry labels are inspected.
  - Then: all minimum M3 telemetry labels are present.
- **S2-M3-04-03**: M2 preservation
  - Given: M3 loop is wired.
  - When: preservation checks run.
  - Then: M2 clean-loop and named-blocker boundary remain valid.
- **S2-M3-04-04**: No M4/M5 leakage
  - Given: full M3 run completes.
  - When: telemetry and changed files are inspected.
  - Then: no Save/Load state or faction consequence is written.
- **S2-M3-04-05**: Local gates
  - Given: implementation is complete.
  - When: required local gates run.
  - Then: all pass or exact blockers are recorded.
- **S2-M3-04-06**: Human-play note
  - Given: the blockout M3 loop is playable.
  - When: human play evidence is recorded.
  - Then: it answers the one-more-pull question and classifies presentation limits honestly.

## Test Evidence

Required evidence:

- `tests/evidence/S2-M3-04/verification.md`
- Story-specific Unity Play Mode or batchmode runner output.
- `tests/evidence/S2-M3-04/human-play-20260514.md`
- `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"`
- T1 negative-scope scan over changed files.
- `git diff --check`
- `.githooks/pre-commit`

## Performance Budget

This story may compose completed M3 surfaces but must not add broad polling,
Save/Load hooks, faction simulation, dynamic economy, or new steady-state loops.
The end-to-end runner should prove behavior through deterministic telemetry, not
through long-running scene sweeps.

## Dependencies

- Depends on: `S2-M3-03` complete.
- Unlocks: M4 Save/Load Flow story-breaking after M3 closure.

## Next Gate

Blocked until `S2-M3-03` is closed.
