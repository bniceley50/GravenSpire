# T1-COMBAT-10 Profiled Combat Slice Evidence Summary

**Date:** 2026-05-05
**Measured build SHA:** `6875672`
**Harness engine:** `.NET 8 headless`
**Fixture set version:** `CombatPrototypeSpellProfileSet_T1@2026-04-28-stage1`
**JSONL:** `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl`
**Command:** `dotnet run --project prototypes/combat-slice-T1/Harness/CombatSliceHarness.csproj`

## Scenario Results

| Scenario | Target | Observed | Result |
| --- | --- | --- | --- |
| `SoloTrash_EvenCon_T1` | `55-85%` Cleric wins across 20 seeded trials; ending state below either `80%` health or `60%` mana. | `20/20` wins; mean ending health `0.819`; mean ending mana `0.486`. | Failed-As-Measured |
| `NamedSoloBlock_T1` | At least `8/10` losses or forced flees across 10 seeded trials. | `5` losses + `4` flees = `9/10`; `1` win. | Passed |
| `TwoTrash_Overpull_T1` | At least `8/10` loss/flee/below-threshold outcomes across 10 seeded trials. | `5/10` dangerous outcomes; `9` wins, `1` death, `0` flees. | Failed-As-Measured |
| `MedBreak_Pacing_T1` | Recover from below `35%` mana to `70%` mana in `60-120s`, only on regen ticks. | `72s`; `12` regen ticks. | Passed |
| `DevBuild_StructuralSmoke_T1` | No Combat Core global visual state or Combat-owned audio playback objects. | `0` structural matches. | Passed |

## Quantitative Failures

`H-CCOM-FEEL-01` failed as measured. The Cleric won every solo-trash trial (`20/20`) where the acceptance criterion expected a `55-85%` win range. This indicates the current production Combat Core plus fixture values make single-trash combat too safe under the harness assumptions.

`H-CCOM-FEEL-03` failed as measured. The two-trash overpull produced only `5/10` dangerous outcomes where the acceptance criterion expected at least `8/10`. This indicates the current production Combat Core plus fixture values do not yet create the intended "do not solo two enemies" pressure under the harness assumptions.

Production source and fixture data were not tuned in this story.

## Structural Passes

`H-CCOM-ART-02` passed structurally: the scan for Combat-owned global visual state found zero matches.

`H-CCOM-AUD-01` passed structurally: the scan for Combat-owned audio playback objects found zero matches.

`H-CCOM-SCOPE-01` passed for this story boundary: production source remained unchanged from `6875672`, and the headless harness grep found zero T1 deny-list matches.

## Handoff

This file is neutral evidence, not a verdict. The measured gap is carried into the slice review session without fixture tuning, production code changes, or acceptance-range revision.

Slice review session reads this file alongside qualitative human play to issue Green/Yellow/Red verdict.
