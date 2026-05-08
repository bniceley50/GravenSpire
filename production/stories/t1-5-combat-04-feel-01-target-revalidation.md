# T1.5-COMBAT-04 - FEEL-01 Target Revalidation

**Status:** Implemented + Verified; awaiting `/story-done`
**Sprint:** 1.5
**Priority:** Must Have
**Layer:** Gameplay / Combat Feel Contract
**Type:** Design/Contract
**Manifest Version:** Sprint 1.5, 2026-05-08
**GDD:** `design/gdd/combat-core.md`
**Governing Decision:** `DECISIONS.md` D014
**Evidence:** `tests/evidence/T1.5-COMBAT-04/verification.md`

## Scope

This story revalidates `H-CCOM-FEEL-01` after the T1 slice review and the
Sprint 1.5 Endurance split. It produces a design/contract decision, not tuning.

Source trace: `production/sprints/sprint-1-5.md:216` through
`production/sprints/sprint-1-5.md:252`; QA trace:
`production/qa/plans/qa-plan-sprint-1-5-20260506.md:140` through
`production/qa/plans/qa-plan-sprint-1-5-20260506.md:152`.

## Decision

D014 moves `H-CCOM-FEEL-01` from the original `55-85%` solo-trash target to
`90-100%` Cleric wins for clean-state `SoloTrash_EvenCon_T1` trials.

The ending-state pressure clause remains part of the acceptance criterion:
mean ending state must still fall below either 80% health or 60% mana. This
keeps med-break pressure separate from the win-rate target.

FEEL-01 now covers clean single-trash reliability. FEEL-03 remains the
overpull danger surface and is not redefined by this story.

## Rationale

- D012 prototype feel was positive at high solo reliability, including the
  direct playtest finding preserved in
  `production/prototypes/combat-feel-report.md:142`.
- The 2026-05-06 pinned-engine prototype rerun recorded `5/5` pulls, `5` med
  breaks, `0` unsafe pulls, and `0` deaths at
  `prototypes/combat-feel/Logs/playtest-20260506-093105.log:1`.
- T1-COMBAT-10 measured `20/20` solo-trash wins against the old target at
  `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:1` and summarized
  the failed-as-measured result in
  `tests/evidence/T1-COMBAT-10/verification.md:64`.
- That measurement predated D013/ADR-0006 implementation. T1.5-COMBAT-02 later
  moved Bash to Endurance while preserving Smite of Authority and Defensive
  Prayer as mana-based abilities, recorded at
  `tests/evidence/T1.5-COMBAT-02/verification.md:32` through
  `tests/evidence/T1.5-COMBAT-02/verification.md:35`.
- T1.5-COMBAT-03 restored FEEL-03 overpull danger without retargeting solo
  trash, leaving FEEL-01 as a clean target decision instead of a tuning task;
  see `tests/evidence/T1.5-COMBAT-03/verification.md:24` and
  `tests/evidence/T1.5-COMBAT-03/verification.md:61` through
  `tests/evidence/T1.5-COMBAT-03/verification.md:63`.

## Files Changed

- `DECISIONS.md` appends D014.
- `design/gdd/combat-core.md` updates the FEEL-01 target and clean-solo tuning
  knob.
- `tests/performance/README.md` updates the current profiled target reference.
- `production/stories/t1-5-combat-04-feel-01-target-revalidation.md` records
  this story.
- `tests/evidence/T1.5-COMBAT-04/verification.md` records verification and old
  target reference classification.

## Out Of Scope

- No fixture data edits.
- No harness behavior or JSONL schema edits.
- No `production/qa/combat/feel-review-T1-slice.md` rewrite.
- No T1.5-COMBAT-05 profiled evidence rerun.
- No Green/Yellow/Red verdict.
- No sprint-status or active-session closure routing updates during the
  implementation batch.

## Acceptance Criteria Coverage

| QA Case | Status | Evidence |
| --- | --- | --- |
| `QA-04-01` D014 decision exists | Covered | D014 is appended to `DECISIONS.md` and names the FEEL-01 decision as Move. |
| `QA-04-02` Rationale cites required evidence | Covered | D014 and this story cite D012 prototype feedback, the 2026-05-06 prototype rerun log, and T1-COMBAT-10 JSONL evidence. |
| `QA-04-03` No fixture data modified | Covered | Verification records that no `assets/data/combat/**` files are modified. |
| `QA-04-04` Old target references handled | Covered | Verification includes a grep classification table for remaining `55-85` and `0.55-0.85` references. |
| `QA-04-05` FEEL-01 remains distinct from FEEL-03 tuning | Covered | D014 states FEEL-01 owns clean solo-trash reliability while FEEL-03 owns overpull danger. |

## Story Status

`T1.5-COMBAT-04` is implemented and verified, awaiting `/story-done`.

## Blockers / Carried Forward

- `T1.5-COMBAT-05` owns the next profiled rerun and any harness/output-label
  updates needed for the new FEEL-01 target.
- Disadvantage-state solo-trash vulnerability remains a future acceptance-
  criteria candidate, not a Sprint 1.5 implementation requirement.
- `T1-COMBAT-11` remains blocked on story-file recovery.
