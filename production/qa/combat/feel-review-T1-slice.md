# T1 Combat Slice Feel Review

**Date:** 2026-05-06
**Status:** Scaffolded evidence artifact; Brian verdict pending.

## Evidence Read Summary

Read inputs:

- `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl`
- `production/qa/combat/t1-combat-10-profiled-evidence-summary.md`
- `production/qa/combat/feel-review-09c-player-death.md`
- `production/stories/t1-combat-10-smoke-profiled-evidence-loop.md`

Quantitative combat-slice evidence is mixed. The headless profiled harness used build `6875672`, fixture set `CombatPrototypeSpellProfileSet_T1@2026-04-28-stage1`, and command `dotnet run --project prototypes/combat-slice-T1/Harness/CombatSliceHarness.csproj`, as recorded in `production/qa/combat/t1-combat-10-profiled-evidence-summary.md:3` through `production/qa/combat/t1-combat-10-profiled-evidence-summary.md:8`.

Direct JSONL failure citations:

- `H-CCOM-FEEL-01`: `SoloTrash_EvenCon_T1` recorded `20/20` solo-trash wins and `result=fail` against the `55-85%` target. Evidence: `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:1`; summary target/observed row at `production/qa/combat/t1-combat-10-profiled-evidence-summary.md:14`.
- `H-CCOM-FEEL-03`: `TwoTrash_Overpull_T1` recorded `5/10` dangerous two-trash outcomes and `result=fail` against the `>=8/10` target. Evidence: `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:3`; summary target/observed row at `production/qa/combat/t1-combat-10-profiled-evidence-summary.md:16`.

Supporting passes:

- `NamedSoloBlock_T1` passed with `9/10` loss/flee outcomes and one win. Evidence: `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:2`; summary row at `production/qa/combat/t1-combat-10-profiled-evidence-summary.md:15`.
- `MedBreak_Pacing_T1` passed with `72s` to recover to `70%` mana over `12` regen ticks. Evidence: `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:4`; summary row at `production/qa/combat/t1-combat-10-profiled-evidence-summary.md:17`.
- `DevBuild_StructuralSmoke_T1` passed with `0` structural matches for Combat-owned global visual state or audio playback objects. Evidence: `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:5`; summary row at `production/qa/combat/t1-combat-10-profiled-evidence-summary.md:18`.

Story scope confirms no production tuning was done to mask the quantitative failures, and the slice review owns the Green/Yellow/Red judgment. Evidence: `production/stories/t1-combat-10-smoke-profiled-evidence-loop.md:21`, `production/stories/t1-combat-10-smoke-profiled-evidence-loop.md:35` through `production/stories/t1-combat-10-smoke-profiled-evidence-loop.md:42`, and `production/stories/t1-combat-10-smoke-profiled-evidence-loop.md:86` through `production/stories/t1-combat-10-smoke-profiled-evidence-loop.md:90`.

## Human Playtest Notes

Brian played the combat-feel prototype on 2026-05-06. Rerun metrics were captured at `prototypes/combat-feel/Logs/playtest-20260506-093105.log`.

Prototype rerun metric summary:

- `final_state`: `Complete`
- `stopped_via`: `completion`
- `pulls_completed`: `5`
- `pulls_target`: `5`
- `total_combat_seconds`: `80.101`
- `total_downtime_seconds`: `76.908`
- `avg_pull_seconds`: `16.02`
- `med_breaks`: `5`
- `auto_swings`: `29`
- `smites_channeled`: `9`
- `heals_used`: `1`
- `smite_of_authority_uses`: `3`
- `bash_uses`: `3`
- `defensive_prayer_uses`: `1`
- `defensive_prayer_damage_prevented`: `6`
- `unsafe_pulls`: `0`
- `deaths`: `0`

Qualitative finding: Bash, and physical instants as a category, should not cost mana. Physical instants should consume a separate Endurance/Stamina resource and remain gated by cooldown/global recovery. This applies to Bash and future Warrior-style physical abilities, not to Cleric magical/holy abilities.

`Smite of Authority` and `Defensive Prayer` remain mana-based Cleric abilities. They are not part of the physical-instant Endurance finding.

T1 surface impact: Endurance is a real T1 combat resource and should exist in Combat Core state, Layer 1 HUD signaling, and save/load persistence. Brian indicated this resource-model finding affects the slice verdict.

### Sprint-1.5 / Sprint-2 Implementation Implications

The Endurance finding likely touches Combat Core actor state, the combat persistence whitelist, HUD projection, fixture schema/data, instant ability resolution, test coverage, and the profiled harness's resource tracking. In the current model, `CombatPersistenceProjection` would need to grow beyond the existing four-field shape to carry Endurance, physical instant fixture rows would need an Endurance cost rather than a mana cost, and physical instant gating would need to read the Endurance pool while magical/holy instants continue using mana. This is an implementation surface-area note only; no source, fixture, GDD, or acceptance-criteria change is made by this review artifact.

## Player-Death Moment Notes

The 09c player-death review currently provides implementation-perspective notes only. It records that lethal player damage now produces `combat_life_state == Dead`, clamps health to zero, clears transient combat interaction state, and emits one narrow `PlayerDeathEvent`. Evidence: `production/qa/combat/feel-review-09c-player-death.md:7` through `production/qa/combat/feel-review-09c-player-death.md:13`.

Death & Corpse Recovery remains intentionally absent from 09c: no respawn flow, corpse-run flow, resurrection, death narrative, penalty calculation, item-drop behavior, recovery interaction, or "you died" UI treatment. Evidence: `production/qa/combat/feel-review-09c-player-death.md:15` through `production/qa/combat/feel-review-09c-player-death.md:19`.

The human player-death prompts remain blank and explicitly pending. Evidence: `production/qa/combat/feel-review-09c-player-death.md:21` through `production/qa/combat/feel-review-09c-player-death.md:41`.

## Verdict

Yellow

No agent verdict replaces Brian's judgment. This artifact preserves the evidence and pending human sections so the Green/Yellow/Red call remains a human feel-review decision, not a derivation from the harness data.

## Rationale

Architecture is holding: the Combat Core correctness suite passed 133/133, the approved ADR boundaries held, and the kill-credit, save-barrier, and persistence seams are not the failure mode. This is not "production is broken"; the feel review surfaced real T1 work that should land before the next system story. The strongest tuning warning is FEEL-03 because two-trash overpull danger is core EQ pull discipline, while FEEL-01 is softer after the prototype rerun also produced clean solo-trash wins and suggests the 55-85% target needs revalidation against D012 feel. The harder design finding is Endurance: Bash and future physical instants should move off mana onto a quiet Endurance resource that supports physical pacing without becoming an action-rotation bar, while Smite of Authority and Defensive Prayer remain mana-based.

## Recommendation For Next Step

If Brian's verdict is **Green**: run `/dev-story T1-COMBAT-11`, then sprint-2 planning.

If Brian's verdict is **Yellow**: run `/dev-story T1-COMBAT-11` if still useful, then sprint-1.5 focused on data-driven combat-feel tuning.

If Brian's verdict is **Red**: skip `T1-COMBAT-11` and plan sprint-1.5 around combat-feel repair first.
