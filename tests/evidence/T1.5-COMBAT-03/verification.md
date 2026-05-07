# T1.5-COMBAT-03 Verification

**Date:** 2026-05-07
**Story:** `production/stories/t1-5-combat-03-feel-03-overpull-tuning.md`
**Verdict:** IMPLEMENTED + VERIFIED; awaiting `/story-done`

## Commands

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"
dotnet run --project prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj -- --evidence-story T1.5-COMBAT-03 --timestamp 2026-05-07T00:00:00-04:00
# Scoped pre-commit: set GIT_INDEX_FILE to a temp index, read HEAD,
# add only the intended T1.5-COMBAT-03 files, then run bash .githooks/pre-commit.
```

## Results

| Gate | Result | Evidence |
| --- | --- | --- |
| Baseline regression before edits | PASS | `148/148` tests passed before implementation. |
| Post-change regression | PASS | `149/149` tests passed after implementation with `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"`. |
| Scoped pre-commit gate | PASS | `.githooks/pre-commit` passed against a temporary index containing only the intended T1.5-COMBAT-03 files. |
| `QA-03-01` Two-trash overpull rerun | PASS | `profiled-combat-slice.jsonl:3` records `TwoTrash_Overpull_T1`, `result=pass`, `dangerous_outcomes=9`, `losses=5`, `wins=5`. |
| `QA-03-02` Named solo-block regression | PASS | `profiled-combat-slice.jsonl:2` records `NamedSoloBlock_T1`, `result=pass`, `losses=5`, `flees=3`, `dangerous_outcomes=8`. |
| `QA-03-03` Med-break pacing regression | PASS | `profiled-combat-slice.jsonl:4` records `MedBreak_Pacing_T1`, `result=pass`, `seconds_to_70_mana=72`, `regen_ticks=12`. |
| `QA-03-04` FEEL-01 untouched | PASS | `profiled-combat-slice.jsonl:1` still records `SoloTrash_EvenCon_T1`, `result=fail`, `wins=20`; unit test `test_qa_03_04_overpull_tuning_uses_dedicated_trash_fixture_without_retargeting_solo_trash` locks the fixture separation. |
| `QA-03-05` Tuning rationale captured | PASS | Story file lists legal knobs and records why a dedicated overpull fixture was selected after harness hydration/timing alone was insufficient. |

## Before / After

Prior `T1-COMBAT-10` evidence recorded `TwoTrash_Overpull_T1` as
`5/10` dangerous outcomes, `9` wins, `1` death, and `result=fail`.

This batch records `TwoTrash_Overpull_T1` as `9/10` dangerous outcomes,
`5` wins, `5` deaths, and `result=pass`.

## Tuning Rationale

The legal knob order was:

1. Correct harness Endurance hydration and resource affordability first so Bash
   is measured under the post-ADR-0006 physical resource model.
2. Tighten only the second-hostile timing in `TwoTrash_Overpull_T1`.
3. Because timing alone still left `TwoTrash_Overpull_T1` below target, add a
   dedicated `Trash_Mid_Overpull_T1` row rather than changing shared
   `Trash_Mid_T1`.

`Trash_Mid_Overpull_T1` keeps the same level, health, armor, weapon delay,
range, faction, and role as shared mid trash. It increases only attack-side
stats for the overpull fixture:

| Stat | `Trash_Mid_T1` | `Trash_Mid_Overpull_T1` |
| --- | ---: | ---: |
| `attackPower` | 25 | 38 |
| `weaponBaseDamage` | 8 | 10 |
| `attackSkill` | 30 | 36 |

## Carry Forward

`SoloTrash_EvenCon_T1` remains failed-as-measured in this run. That is
intentional: `T1.5-COMBAT-04` owns FEEL-01 target revalidation, and this story
does not tune or redefine FEEL-01.
