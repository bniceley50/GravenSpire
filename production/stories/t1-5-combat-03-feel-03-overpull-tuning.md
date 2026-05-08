# T1.5-COMBAT-03 - FEEL-03 Overpull Tuning

**Status:** Complete
**Sprint:** 1.5
**Priority:** Must Have
**Layer:** Gameplay / Combat Feel Fixtures
**Type:** Config/Data + Profiled Feel
**Manifest Version:** Sprint 1.5, 2026-05-07
**GDD:** `design/gdd/combat-core.md`
**Governing ADR:** `docs/architecture/adr-0006-endurance-resource-model.md`
**Evidence:** `tests/evidence/T1.5-COMBAT-03/verification.md`

## Scope

This story restores `H-CCOM-FEEL-03` two-trash overpull danger after the Sprint
1 slice review found only `5/10` dangerous outcomes. It treats FEEL-03 as the
stronger tuning warning while leaving FEEL-01 target revalidation to
`T1.5-COMBAT-04`.

Source trace: `production/sprints/sprint-1-5.md:180` through
`production/sprints/sprint-1-5.md:214`; QA trace:
`production/qa/plans/qa-plan-sprint-1-5-20260506.md:132` through
`production/qa/plans/qa-plan-sprint-1-5-20260506.md:144`.

## Legal Tuning Knobs

Legal knobs for this story were constrained before tuning:

- Harness Endurance hydration had to be corrected first, because
  `T1.5-COMBAT-02` made Bash physical and the profiled harness must measure the
  post-ADR-0006 resource model.
- Two-trash timing was legal because `TwoTrash_Overpull_T1` enters two same-band
  hostiles within the GDD's 5-second window; simultaneous aggro is the narrowest
  scenario-specific pressure increase.
- A dedicated overpull trash row was legal only after timing alone failed,
  because changing shared `Trash_Mid_T1` would silently retune
  `SoloTrash_EvenCon_T1`.

Forbidden knobs:

- No `SoloTrash_EvenCon_T1` fixture retargeting.
- No Cleric health, mana, Endurance, spell, instant, regen, FEEL threshold, or
  global melee formula retuning.
- No new human Green/Yellow/Red verdict.

## Implementation Trace

- `prototypes/combat-slice-T1/Harness/CombatSliceHarness.cs` now accepts
  story-specific evidence arguments so T1.5 profiled evidence does not overwrite
  T1-COMBAT-10 evidence.
- The harness hydrates fixture Endurance into player and NPC actor states, uses
  resource-aware instant affordability, and records ending Endurance ratios.
- `TwoTrash_Overpull_T1` now pulls both enemies simultaneously in the harness.
- `assets/data/combat/t1-combat-fixtures.json` bumps `fixtureSetVersion` and
  adds `Trash_Mid_Overpull_T1`, a two-trash-specific hostile fixture with the
  same health/armor as shared mid trash and modestly higher attack stats.
- `TwoTrash_Overpull_T1` uses the dedicated overpull fixture. `SoloTrash_EvenCon_T1`
  still uses shared `Trash_Mid_T1`.

## Acceptance Criteria Coverage

| QA Case | Status | Evidence |
| --- | --- | --- |
| `QA-03-01` Two-trash overpull rerun | Covered | `tests/evidence/T1.5-COMBAT-03/profiled-combat-slice.jsonl` row `TwoTrash_Overpull_T1` records `result=pass`, `dangerous_outcomes=9`, `losses=5`. |
| `QA-03-02` Named solo-block regression | Covered | JSONL row `NamedSoloBlock_T1` records `result=pass`, `losses=5`, `flees=3`, `dangerous_outcomes=8`. |
| `QA-03-03` Med-break pacing regression | Covered | JSONL row `MedBreak_Pacing_T1` records `result=pass`, `seconds_to_70_mana=72`, `regen_ticks=12`. |
| `QA-03-04` FEEL-01 untouched by this story | Covered | JSONL row `SoloTrash_EvenCon_T1` remains `result=fail`; unit test `test_qa_03_04_overpull_tuning_uses_dedicated_trash_fixture_without_retargeting_solo_trash` proves solo trash still uses `Trash_Mid_T1`. |
| `QA-03-05` Tuning rationale captured | Covered | Legal knob list and implementation trace above; verification summary records before/after inputs. |

## Runnable Evidence

- Baseline before edits:
  `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"`
  passed `148/148`.
- Post-change regression:
  `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"`
  passed `149/149`.
- Profiled run:
  `dotnet run --project prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj -- --evidence-story T1.5-COMBAT-03 --timestamp 2026-05-08T00:00:00-04:00`
  exited `0` at `HEAD=e7233b5` and wrote
  `tests/evidence/T1.5-COMBAT-03/profiled-combat-slice.jsonl` with
  `build_sha=e7233b5`.
- Scoped pre-commit gate:
  `.githooks/pre-commit` passed against a temporary index containing only the
  intended T1.5-COMBAT-03 files.

## Story Status

`T1.5-COMBAT-03` is complete.

## Blockers / Carried Forward

- `T1.5-COMBAT-04` still owns FEEL-01 target revalidation. This story records
  the current FEEL-01 row as failed, but does not solve or redefine it.
- `T1-COMBAT-11` still owns the forbidden-pattern compliance scan/analyzer.

## Completion Notes

**Completed:** 2026-05-08
**Verdict:** COMPLETE WITH NOTES
**Criteria:** 5/5 QA-03 cases covered.
**Test Evidence:** `tests/evidence/T1.5-COMBAT-03/verification.md:20` through
`tests/evidence/T1.5-COMBAT-03/verification.md:28` record the `148/148`
baseline, `149/149` post-change regression, scoped pre-commit pass,
post-commit evidence-origin pass, and all QA-03 PASS rows. JSONL ground truth
is `tests/evidence/T1.5-COMBAT-03/profiled-combat-slice.jsonl:3`, which
records `TwoTrash_Overpull_T1`, `build_sha=e7233b5`, `result=pass`,
`dangerous_outcomes=9`, `losses=5`, and `wins=5`; line `:2` records
`NamedSoloBlock_T1` passing with `dangerous_outcomes=8`, and line `:4` records
`MedBreak_Pacing_T1` passing with `seconds_to_70_mana=72`.
**Implementation Provenance:** `e7233b5` is the implementation and
metric-capture commit for the JSONL rows. `1935515` is the follow-up
SHA-drift fix on `origin/main` that restored evidence reproducibility after
the pre-commit harness run captured the parent SHA. Verified on 2026-05-08 via
`git rev-parse HEAD`, `git rev-parse origin/main`, and
`git show --format=fuller --no-patch 1935515`.
**Code Review:** Complete. `/code-review` approved `1935515`; the SHA-drift
follow-up is treated as part of the closure-eligible implementation chain.
**Deviations:** FEEL-01 target revalidation remains owned by
`T1.5-COMBAT-04`. `AbilityResolvedEvent.ManaSpent` payload semantics and
Endurance forbidden-pattern scan coverage remain `T1-COMBAT-11` inputs.
QA-02-01 cooldown/global-recovery wording remains deferred to the next QA plan
iteration. `T1-COMBAT-11` story-file recovery and `09c` human playtest remain
open carryovers. The pre-commit SHA-drift workflow lesson should be captured
before `T1.5-COMBAT-05`, the next harness-touching story.
