# T1.5-COMBAT-05 Profiled Combat Slice Evidence Summary

**Date:** 2026-05-09
**Measured build SHA:** `be1c3ed`
**Harness engine:** `.NET 8 headless`
**Fixture set version:** `CombatPrototypeSpellProfileSet_T1@2026-05-07-t1-5-combat-03`
**JSONL:** `tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl`
**Command:** `dotnet run --project prototypes/combat-slice-T1/Harness/CombatSliceHarness.csproj`

## Scenario Results

| Scenario | Target | Observed | Result |
| --- | --- | --- | --- |
| `SoloTrash_EvenCon_T1` | `90-100%` Cleric wins across 20 clean-state seeded trials; ending state below either `80%` health or `60%` mana. | `20/20` wins; mean ending health `0.819`; mean ending mana `0.544`. | Pass |
| `NamedSoloBlock_T1` | At least `8/10` losses or forced flees across 10 seeded trials. | `5` losses + `3` flees = `8/10`; `2` wins. | Pass |
| `TwoTrash_Overpull_T1` | At least `8/10` loss/flee/below-threshold outcomes across 10 seeded trials. | `9/10` dangerous outcomes; `5` wins, `5` losses/deaths, `0` flees. | Pass |
| `MedBreak_Pacing_T1` | Recover from below `35%` mana to `70%` mana in `60-120s`, only on regen ticks. | `72s`; `12` regen ticks. | Pass |
| `DevBuild_StructuralSmoke_T1` | No Combat Core global visual state or Combat-owned audio playback objects. | `0` structural matches. | Structural-Pass |

## Quantitative Notes

`H-CCOM-FEEL-01` passes against D014's current clean-state target. The Cleric won all solo-trash trials (`20/20`), and the pressure clause remains active because mean ending mana was `0.544`, below the `0.60` threshold.

`H-CCOM-FEEL-03` remains separated from FEEL-01. The two-trash overpull produced `9/10` dangerous outcomes without changing fixture stats or production Combat Core logic in this story.

`NamedSoloBlock_T1` met the blocking threshold exactly: `5` losses plus `3` flees equals `8/10` non-win outcomes.

`MedBreak_Pacing_T1` stayed inside the pacing band at `72s` to `70%` mana, with `12` regen ticks.

`DevBuild_StructuralSmoke_T1` found zero structural matches for the Combat-owned presentation patterns it scans.

## Handoff

This file is neutral quantitative evidence. Any future human qualitative rating remains outside this implementation batch.
