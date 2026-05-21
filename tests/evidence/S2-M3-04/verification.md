# S2-M3-04 Verification - End-To-End Objective Loop

**Story:** `production/stories/s2-m3-04-end-to-end-objective-loop.md`
**Result:** COMPLETE WITH NOTES — `/code-review` PASS_WITH_NOTES; `/story-done` closure passed locally; AC-06 feel-validation transferred to Sprint 3 per lead decision.
**Date:** 2026-05-20
**Implementation Branch:** `claude/s2-m3-04-end-to-end-objective-loop`
**Implementation HEAD:** `ee7c450c45fd3dcd25324bbe1e80f1f6c49b33f9`

## Evidence Index

| Artifact | Purpose |
| --- | --- |
| `Assets/Editor/GravenspireM3EndToEndObjectiveLoopVerificationRunner.cs` | Unity batchmode end-to-end runner: drives the full M3 sequence (NPC frame -> objective accept -> relic recovery -> loot resolution -> hand-in -> salvage sale) and the M2 named-blocker preservation smoke; emits 15 telemetry labels and 29 PASS/FAIL checks. |
| `tests/evidence/S2-M3-04/unity-end-to-end-objective-loop-20260520-smoke.md` | Unity batchmode smoke result: PASS, 29/29 checks, no warnings, no errors, exit 0. (The raw Unity `.log` is retained locally; not committed — its connection lines carry trailing whitespace that fails the commit whitespace gate.) |
| `tests/evidence/S2-M3-04/human-play-20260520.md` | AC-06 human-play notes — the M3 objective loop was found not player-interactive in blockout; feel-validation transferred to Sprint 3. |

## Acceptance Criteria

| AC | Result | Evidence |
| --- | --- | --- |
| `S2-M3-04-01` | PASS | The end-to-end runner proves the full M3 sequence; smoke records `objective_accepted_from_npc`, `relic_recovered`, `objective_loot_resolved`, `relic_handed_in_to_npc`, `vendor_salvage_sale_succeeds`, and `state_sequence_exact` as PASS (29/29 overall). |
| `S2-M3-04-02` | PASS | Smoke telemetry includes all 11 mechanical labels: `npc_anchor_present`, `npc_interaction_intentional`, `dialogue_template_id`, `objective_state_sequence`, `relic_available`, `loot_table_id`, `loot_result_item_ids`, `relic_handed_in`, `objective_complete`, `vendor_salvage_sold`, `vendor_sell_copper_applied`. |
| `S2-M3-04-03` | PASS | Smoke telemetry records `m2_clean_loop_preserved=True` and `m2_named_blocker_boundary_preserved=True`. |
| `S2-M3-04-04` | PASS | Smoke telemetry records `no_save_load_state_written=True` and `no_faction_consequence_applied=True`. |
| `S2-M3-04-05` | PASS | `dotnet test` 189/189 at closure; T1 negative-scope deny-scan over the new runner returned zero matches; `git diff --check` and `.githooks/pre-commit` passed against the staged closure index. |
| `S2-M3-04-06` | DEFERRED — transferred to Sprint 3 | Human-play (`human-play-20260520.md`) found the M3 objective loop not player-interactive in blockout; the one-more-pull question could not be validated. Per lead decision (2026-05-20) the feel-validation transfers to the Sprint 3 "Playable Vertical-Slice Assembly" milestone. AC-06's documentation requirement — notes captured, presentation limitations classified — is satisfied. |

## Local Gates

| Gate | Result | Evidence |
| --- | --- | --- |
| `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` | PASS | 189/189 passed at closure. The new runner is an `Assets/Editor` script outside the test csproj's compile set; the regression is unchanged. |
| Unity batchmode smoke | PASS | `tests/evidence/S2-M3-04/unity-end-to-end-objective-loop-20260520-smoke.md` records 29/29 checks PASS, no warnings, no errors, exit 0. |
| T1 negative-scope scan | PASS | Project deny-pattern scan over `Assets/Editor/GravenspireM3EndToEndObjectiveLoopVerificationRunner.cs` returned zero matches. |
| `git diff --check` | PASS | Final hygiene gate returned exit 0. |
| `.githooks/pre-commit` | PASS | Staged closure index returned `[pre-commit] OK`. |

## Scope Notes

- This is a proof/integration story: one new file (the Editor runner). No new session model, Unity bridge, scene object, or `.unity` edit; no `src/` or `tests/` change. The runner composes the already-built, runner-proven M3-01/02/03 + M2 systems.
- `/code-review` (lean mode, with a `unity-specialist` pass) returned PASS_WITH_NOTES — no blocking or high issues; non-blocking LOW notes deferred (carryover `m3_04_low_review_notes`).
- The Unity batchmode smoke initially failed (exit 1) on the M2 named-blocker checks. Root cause (found via systematic debugging): the runner chained `RunAutomatedTwoPullSmoke()` before `RunAutomatedNamedBlockerBoundarySmoke()`, advancing the shared, never-reset `_playerMeleeRandom` cursor in `M2SingleTrashMedLoopController` and making the named-blocker scenario solo-kill the blocker. Fix: the runner now calls only `RunAutomatedNamedBlockerBoundarySmoke()` (which runs the clean two-pull internally), matching the proven `GravenspireM2NamedBlockerVerificationRunner`. Verified by the re-run smoke (29/29 PASS). The M2-controller melee-RNG-reset fragility is tracked as carryover `m2_melee_rng_not_reset`.
- AC-06 (human-play) could not validate the one-more-pull feel because the M3 objective loop is not player-interactive in the blockout build. Per lead decision the feel-validation transfers to the Sprint 3 "Playable Vertical-Slice Assembly" milestone; see `human-play-20260520.md` and carryover `s2_m3_04_ac06_transfer`.
- Unity 6.4 drift incident: the project was briefly opened in Unity 6.4 during the human-play attempt, which upgraded 6 tracked files (`ProjectVersion.txt`, `Packages/manifest.json`, `Packages/packages-lock.json`, `UniversalRenderPipelineGlobalSettings.asset`, `ShaderGraphSettings.asset`, `_DevEntry.unity`). All 6 were reverted, restoring the Unity 6.3.14f1 (D001) pin before this closure.
- `docs/architecture/control-manifest.md` remains absent (carryover `control_manifest_absence_pre_existing`); closure proceeds under the documented architecture-registry fallback, consistent with the S2-M2 and S2-M3 closures.
