# QA Sign-Off Report: Sprint 1.5

**Date**: 2026-05-09
**Scope**: Sprint 1.5 close-out for T1 Combat Feel Correction
**QA Lead sign-off**: pending Brian approval

## Verdict: APPROVED WITH CONDITIONS

Sprint 1.5 is approved to proceed to `/gate-check` with accepted smoke warnings. Do not roll `production/sprint-status.yaml` to Sprint 2 until `/gate-check` also passes.

## Scope Summary

| Story | Type | Result | Evidence | Bugs |
|---|---|---|---|---|
| `T1.5-COMBAT-00` Endurance contract lock | Design/Contract | PASS WITH NOTES | Story status complete at `production/stories/t1-5-combat-00-endurance-contract-lock.md:3`; QA rows pass at `tests/evidence/T1.5-COMBAT-00/verification.md:26` through `tests/evidence/T1.5-COMBAT-00/verification.md:30`. | None |
| `T1.5-COMBAT-01` Endurance state, persistence, HUD signal | Integration | PASS WITH NOTES | Story status complete at `production/stories/t1-5-combat-01-endurance-state-hud-save-projection.md:3`; QA rows pass at `tests/evidence/T1.5-COMBAT-01/verification.md:35` through `tests/evidence/T1.5-COMBAT-01/verification.md:40`. | None |
| `T1.5-COMBAT-02` Physical instant conversion | Logic + Integration | PASS WITH NOTES | Story status complete at `production/stories/t1-5-combat-02-physical-instant-conversion.md:3`; QA rows pass at `tests/evidence/T1.5-COMBAT-02/verification.md:32` through `tests/evidence/T1.5-COMBAT-02/verification.md:38`. | None |
| `T1.5-COMBAT-03` FEEL-03 overpull tuning | Config/Data + Profiled Feel | PASS WITH NOTES | Story status complete at `production/stories/t1-5-combat-03-feel-03-overpull-tuning.md:3`; profiled and regression rows pass at `tests/evidence/T1.5-COMBAT-03/verification.md:20` through `tests/evidence/T1.5-COMBAT-03/verification.md:28`. | None |
| `T1.5-COMBAT-04` FEEL-01 target revalidation | Design/Contract | PASS WITH NOTES | Story status complete at `production/stories/t1-5-combat-04-feel-01-target-revalidation.md:3`; QA rows pass at `tests/evidence/T1.5-COMBAT-04/verification.md:18` through `tests/evidence/T1.5-COMBAT-04/verification.md:22`. | None |
| `T1-COMBAT-11` Forbidden-pattern compliance scan/analyzer | Static/Integration | PASS WITH KNOWN-CARRYOVER | Story status complete at `production/stories/t1-combat-11-forbidden-pattern-compliance-scan-analyzer.md:3`; scanner gate passed `159/159` at `tests/evidence/T1-COMBAT-11/verification.md:28`; AC evidence starts at `tests/evidence/T1-COMBAT-11/verification.md:86`. | None |
| `T1.5-COMBAT-05` Profiled rerun + slice evidence summary | Profiled QA Evidence | PASS | Story status complete at `production/stories/t1-5-combat-05-profiled-rerun-evidence-summary.md:3`; QA rows pass at `tests/evidence/T1.5-COMBAT-05/verification.md:59` through `tests/evidence/T1.5-COMBAT-05/verification.md:64`; story verdict complete at `production/stories/t1-5-combat-05-profiled-rerun-evidence-summary.md:87`. | None |

## Smoke Check

Sprint smoke verdict is `PASS WITH WARNINGS` at `production/qa/smoke-sprint-20260509.md:74`; smoke report says Sprint 1.5 is ready for `/team-qa sprint` at `production/qa/smoke-sprint-20260509.md:76`.

Coverage is complete: `7 covered, 0 missing` at `production/qa/smoke-sprint-20260509.md:45`.

Live close-out reruns during `/team-qa sprint`:
- `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"`: PASS, `164/164`.
- `bash .githooks/pre-commit`: PASS, `[pre-commit] OK`.
- T1 negative-scope scan over Sprint 1.5 profiled surfaces: PASS, no matches.

The profiled harness was not rerun during this sign-off because it writes `tests/evidence/<story>/profiled-combat-slice.jsonl` directly at `prototypes/combat-slice-T1/Harness/CombatSliceHarness.cs:57` through `prototypes/combat-slice-T1/Harness/CombatSliceHarness.cs:66`. The smoke report already records that rerunning at a later SHA would dirty the tracked JSONL and was intentionally restored under the established provenance convention at `production/qa/smoke-sprint-20260509.md:65`.

Existing profiled JSONL covers all five required scenarios at `tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl:1` through `tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl:5`. The human-readable summary is `production/qa/combat/t1-5-combat-profiled-evidence-summary.md`; scenario results are recorded at `production/qa/combat/t1-5-combat-profiled-evidence-summary.md:14` through `production/qa/combat/t1-5-combat-profiled-evidence-summary.md:18`.

## Bugs Found

| ID | Story | Severity | Status | Sign-Off Impact |
|---|---|---|---|---|
| None | - | - | - | None |

## Conditions

1. Production Unity launch/menu/session smoke remains **NOT CHECKED** because the Unity project shell is absent; the smoke warning is recorded at `production/qa/smoke-sprint-20260509.md:63`.
2. `production/stage.txt` is absent; project stage for this pass was inferred from `production/session-state/active.md:4`.
3. Future profiled harness reruns must either preserve the established JSONL provenance convention or run under an approved evidence-update batch.

## Rationale

Sprint 1.5 met its close-out standard for isolated Combat Core and combat-feel correction: all seven stories are complete, story evidence exists, smoke coverage is complete, the local Combat test suite passes, pre-commit passes, and the T1 negative-scope scan remains clean. The remaining warnings are accepted foundation/process warnings, not Sprint 1.5 combat regressions.

Sprint 2 planning should include a Unity project shell foundation story before hub, faction, Save/Load, NPC, or playable-loop feature stories depend on launch smoke.

## Next Step

Run `/gate-check`. If it passes, preserve the Sprint 1.5 close-out evidence, then roll sprint state forward and run Sprint 2 `/qa-plan sprint`.
