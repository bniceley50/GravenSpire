# T1.5-COMBAT-03 - FEEL-03 Overpull Tuning

**Status:** Implemented + Verified; awaiting `/story-done`
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
  `dotnet run --project prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj -- --evidence-story T1.5-COMBAT-03 --timestamp 2026-05-07T00:00:00-04:00`
  exited `0` and wrote
  `tests/evidence/T1.5-COMBAT-03/profiled-combat-slice.jsonl`.
- Scoped pre-commit gate:
  `.githooks/pre-commit` passed against a temporary index containing only the
  intended T1.5-COMBAT-03 files.

## Story Status

`T1.5-COMBAT-03` is implemented and verified, awaiting `/story-done`.

## Blockers / Carried Forward

- `T1.5-COMBAT-04` still owns FEEL-01 target revalidation. This story records
  the current FEEL-01 row as failed, but does not solve or redefine it.
- `T1-COMBAT-11` still owns the forbidden-pattern compliance scan/analyzer.
