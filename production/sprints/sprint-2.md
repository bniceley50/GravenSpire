# Sprint 2 — MVP Foundation Scaffold

> **Status**: Scaffold
> **Generated**: 2026-05-09
> **Current Head**: a7269cb
> **Prior Sprint Gate**: Sprint 1.5 close-out gates pending

## Goal

Lock the next build lane to the actual T1 MVP: one hub, one faction slice, one combat loop, Save/Load, and minimal NPC behavior. Sprint 2 starts with the completed Gemini-review hotfix (`S2-COMBAT-01`) and does not authorize new implementation until Sprint 1.5 close-out gates and a Sprint 2 QA plan are complete.

## Required Gate Order

1. Run Sprint 1.5 close-out gates: `/smoke-check sprint`, `/team-qa sprint`, `/gate-check`.
2. Run `/qa-plan sprint` for Sprint 2 after those gates pass.
3. Select the next `/dev-story` only after the QA plan exists.

## Story Ledger

| Story | Status | Commits | Evidence |
|---|---|---:|---|
| `S2-COMBAT-01` Fix init-only property preservation in CombatActorState transitions | Complete | `5b8a017` -> `a7269cb` | `tests/evidence/S2-COMBAT-01/verification.md` |

## Operating Model Calibration

- Use lighter ceremony for bounded documentation, evidence, provenance, and closure fixes.
- Keep full rigor for cross-contract code, persistence/state transitions, frozen contracts, and fixture/harness logic.
- Chain tables contain only actual full SHAs and grow append-only; no pending placeholder rows.
- External whole-codebase review is valuable once per sprint or tier transition; findings enter the next sprint unless they are immediate ship-blockers.

## Known Findings

- `production/session-state/active.md` previously carried stale World Structure review wording from the first MAJOR REVISION round; corrected in this scaffold batch's active.md edit. Latest source is APPROVED in `design/gdd/reviews/world-structure-review-log.md`.
- `design/gdd/save-load-persistence.md` header still says `Status: In Design`; its review log says APPROVED. Do not silently rewrite the GDD header in this scaffold batch.
- Sprint 1.5 carryovers remain inputs to Sprint 2 QA planning, especially human death-moment playtest, QA-02-01 wording, and evidence-authoring norms.
