# S2-M3-02 - Objective State + Relic Hand-In

**Status:** Complete
**Sprint:** 2
**Priority:** Must Have
**Layer:** Gameplay / Unity Runtime
**Type:** Integration
**Estimate:** 1.0 days
**Manifest Version:** Sprint 2, 2026-05-14
**GDD:** `design/gdd/npc-system.md`; `design/gdd/game-concept.md`
**Quick Design:** `design/quick/quick-design-m3-objective-npc-loot.md`
**Governing Decisions:** `DECISIONS.md` D001, D002, D003, D004
**Evidence:** `tests/evidence/S2-M3-02/verification.md`

## Routing Status

Complete as of 2026-05-18. This story adds the four-state session-local objective
tracker and NPC relic hand-in, reusing the `S2-M3-01` `M3_Caretaker` anchor. It
uses an authored placeholder relic marker; `S2-M3-03` owns the final authored
loot table that resolves the relic and salvage together.

## Source Trace

- `design/quick/quick-design-m3-objective-npc-loot.md:146` through `:151`
  define the four objective states.
- `design/quick/quick-design-m3-objective-npc-loot.md:261` through `:268`
  define M3-02 objective state and relic hand-in criteria.
- `design/quick/quick-design-m3-objective-npc-loot.md:93` through `:104`
  define the M3 loop: NPC frame, relic recovery, return to NPC, then vendor.
- `design/quick/quick-design-m3-objective-npc-loot.md:333` through `:346`
  preserve Save/Load, faction consequence, and future-system non-goals.

## Scope

Add a minimal session-local objective tracker:

`NotIntroduced -> Accepted -> RelicRecovered -> Complete`

The objective becomes accepted through the NPC interaction from `S2-M3-01`, makes
one relic pickup/chest marker available through authored state, records recovery
into session-local carried state, and completes when the relic is returned to
the named NPC.

Planned implementation surface:

- Session-local M3 objective state.
- `M3_ObjectiveRelic` or equivalent marker availability.
- NPC hand-in path for `CourtMarkedRelic_T1` or equivalent.
- Story-specific state-transition evidence under `tests/evidence/S2-M3-02/`.

## Out Of Scope

- No global quest system, quest journal, final objective UI, map marker, minimap,
  auto-pathing, proximity bark, Save/Load persistence, repair-by-load, faction
  reputation, Zone Control, visible consequence, loot table finalization, vendor
  sale, currency, full Inventory implementation, or live LLM behavior.

## Acceptance Criteria

| ID | Criterion | Evidence |
| --- | --- | --- |
| `S2-M3-02-01` | Accepting the objective transitions state deterministically from `NotIntroduced` to `Accepted`. | `tests/evidence/S2-M3-02/verification.md` |
| `S2-M3-02-02` | The objective makes one relic/pickup/chest marker available through authored state, not global quest polling or map markers. | `tests/evidence/S2-M3-02/verification.md` |
| `S2-M3-02-03` | Recovering the relic transitions objective state to `RelicRecovered` in session-local carried state. | `tests/evidence/S2-M3-02/verification.md` |
| `S2-M3-02-04` | Returning the relic to the named NPC transitions objective state to `Complete`. | `tests/evidence/S2-M3-02/verification.md` |
| `S2-M3-02-05` | Objective state remains session-local and synchronous; no Save/Load persistence, faction consequence, or repair-by-load path is added. | `tests/evidence/S2-M3-02/verification.md` |
| `S2-M3-02-06` | Re-entering the M2 combat loop still works after objective state changes. | `tests/evidence/S2-M3-02/verification.md` |

## QA Test Cases

- **S2-M3-02-01**: Accept transition
  - Given: the player intentionally interacts with the M3 NPC.
  - When: the objective is accepted or framed.
  - Then: state sequence records `NotIntroduced -> Accepted`.
- **S2-M3-02-02**: Relic availability
  - Given: objective state is `Accepted`.
  - When: the relic area is inspected.
  - Then: exactly one authored relic marker is available without markers or polling.
- **S2-M3-02-03**: Recovery transition
  - Given: the relic marker is available.
  - When: the player recovers it.
  - Then: state becomes `RelicRecovered` and session-local carried state records the item.
- **S2-M3-02-04**: NPC hand-in
  - Given: state is `RelicRecovered`.
  - When: the player returns to the NPC and hands in the relic.
  - Then: state becomes `Complete`.
- **S2-M3-02-05**: No persistence or consequence leakage
  - Given: changed files are inspected.
  - When: objective code is reviewed.
  - Then: no Save/Load, faction consequence, or repair-by-load path exists.
- **S2-M3-02-06**: M2 preservation
  - Given: objective state changes have occurred.
  - When: M2 loop preservation smoke runs.
  - Then: prior loop checks still pass.

## Test Evidence

Required evidence:

- `tests/evidence/S2-M3-02/verification.md`
- Unit or integration tests for objective state transitions.
- Unity runner evidence for relic availability and NPC hand-in.
- M2 preservation evidence.
- T1 negative-scope scan over changed files.
- `git diff --check`
- `.githooks/pre-commit`

## Performance Budget

This story must keep objective state synchronous and event-driven. It must not
add global quest polling, per-frame objective scans, Save/Load hooks, faction
updates, or broad scene searches.

## Dependencies

- Depends on: `S2-M3-01` complete.
- Unlocks: `S2-M3-03` Loot Table + Fixed-Profile Vendor.
- Note: `S2-M3-02` uses a placeholder relic marker; `S2-M3-03` owns the authored
  loot table that resolves the relic and salvage.

## Next Gate

Complete. Next gate: `/story-readiness production/stories/s2-m3-03-loot-table-fixed-profile-vendor.md`.

## Completion Notes

**Completed:** 2026-05-18
**Criteria:** 6/6 passing.
**Verdict:** COMPLETE WITH NOTES.
**Test Evidence:** `tests/evidence/S2-M3-02/verification.md`; `tests/unit/gameplay/npc/m3_objective_state_relic_hand_in_test.cs`; `tests/evidence/S2-M3-02/unity-objective-state-relic-hand-in-20260518-smoke.md`; `tests/evidence/S2-M3-02/m2-02-preservation-20260518-smoke.md`; `tests/evidence/S2-M3-02/m2-03-preservation-20260518-smoke.md`; `tests/evidence/S2-M3-02/m2-04-preservation-20260518-smoke.md`.
**Code Review:** Complete on 2026-05-18. Evidence-label overclaim fixed; CodeRabbit `-langversion:10.0` finding verified as project-context false positive and documented in `tests/evidence/S2-M3-02/verification.md`.
**Deviations / Notes:** `docs/architecture/control-manifest.md` remains absent as pre-existing Sprint 2 carryover; local Unity package wiring was required for the pure C# objective model and is valid scope; M2-02 preservation evidence still carries the known stale embedded `2026-05-12` date from `m2_02_runner_date_hardcoded`.
**Closure Commit:** `fb77f83`.
