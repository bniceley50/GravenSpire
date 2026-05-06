# T1.5-COMBAT-00 - Endurance Contract Lock

**Status:** Implemented + Verified; awaiting /story-done
**Sprint:** 1.5
**Priority:** Must Have
**Layer:** Gameplay / Combat Core Architecture
**Type:** Design/Contract
**Estimate:** 1.0 day
**Manifest Version:** Sprint 1.5, 2026-05-06
**GDD:** `design/gdd/combat-core.md`
**Governing ADR:** `docs/architecture/adr-0006-endurance-resource-model.md`
**Evidence:** `tests/evidence/T1.5-COMBAT-00/verification.md`

## Scope

This story locks the proposed Endurance resource contract before any Sprint 1.5
implementation begins:

- Append D013 to `DECISIONS.md` as a Proposed decision entry for ADR-0006.
- Create `docs/architecture/adr-0006-endurance-resource-model.md` with the
  Sprint 1.5 Endurance contract.
- Preserve the physical-only Endurance scope and quiet HUD/save discipline from
  Brian's slice-review verdict.
- Name `Smite of Authority` and `Defensive Prayer` as mana-based carveouts.
- Record verification for the docs-only story in
  `tests/evidence/T1.5-COMBAT-00/verification.md`.

Source trace: `production/sprints/sprint-1-5.md:67` through
`production/sprints/sprint-1-5.md:97`; QA trace:
`production/qa/plans/qa-plan-sprint-1-5-20260506.md:73` through
`production/qa/plans/qa-plan-sprint-1-5-20260506.md:87`.

## Out Of Scope

- Any source code, production test, fixture data, GDD, `.claude/**`,
  `.githooks/**`, sprint-status, or session-state edit.
- Endurance actor-state implementation.
- Combat persistence projection implementation.
- Layer 1 HUD projection implementation.
- Instant resolver or fixture schema changes.
- FEEL-01 target revalidation.
- FEEL-03 overpull tuning.
- Any story file for T1.5-COMBAT-01 through T1.5-COMBAT-05 or T1-COMBAT-11.

## Dependencies And Inputs

- Baseline regression passed at HEAD `baec9a6`: `dotnet test
  tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"`
  returned 133 total / 133 passed / 0 failed / 0 skipped.
- Slice review verdict committed at `4edf2f9` with Brian's Yellow verdict.
- Sprint 1.5 plan committed at `8885d2e`.
- Sprint 1.5 QA plan committed at `b6297b4`.

## Composition Trace

- D013 is appended to `DECISIONS.md` and remains Proposed until implementation
  validates the Endurance contract.
- ADR-0006 is a new proposed architecture document.
- No downstream story files are created by this batch; Sprint 1.5 story files
  remain forward-looking until their `/dev-story` runs.
- No implementation surface is touched.

## Acceptance Criteria Coverage

| QA Case | Status | Evidence |
| --- | --- | --- |
| `QA-00-01` D013 Endurance decision exists | Covered | `DECISIONS.md:374` and `DECISIONS.md:377`; verification at `tests/evidence/T1.5-COMBAT-00/verification.md:26`. |
| `QA-00-02` D013 cites slice review | Covered | `DECISIONS.md:382` through `DECISIONS.md:385`; verification at `tests/evidence/T1.5-COMBAT-00/verification.md:27`. |
| `QA-00-03` Specific carveout artifacts are named | Covered | `docs/architecture/adr-0006-endurance-resource-model.md:55` through `docs/architecture/adr-0006-endurance-resource-model.md:56`; verification at `tests/evidence/T1.5-COMBAT-00/verification.md:28`. |
| `QA-00-04` Quiet Endurance banned patterns are explicit | Covered | `docs/architecture/adr-0006-endurance-resource-model.md:81` through `docs/architecture/adr-0006-endurance-resource-model.md:90`; verification at `tests/evidence/T1.5-COMBAT-00/verification.md:29`. |
| `QA-00-05` Docs-only story boundary holds | Covered | `tests/evidence/T1.5-COMBAT-00/verification.md:62` through `tests/evidence/T1.5-COMBAT-00/verification.md:71`. |

## Runnable Evidence

This is a docs-only Design/Contract story. No TRX file is expected. Baseline
regression before the story passed 133/133 at HEAD `baec9a6`.

## Story Status

`T1.5-COMBAT-00` is implemented and verified, awaiting `/story-done`.

## Blockers / Carried Forward

- T1.5-COMBAT-01 validates Endurance state, persistence, and HUD projection.
- T1.5-COMBAT-02 validates the physical-instant resource split.
- ADR-0006 and D013 should remain Proposed until implementation validation.
- If the implementation holds, ADR-0006 can move to Accepted and D013 can move
  to Locked during the appropriate closure batch.
