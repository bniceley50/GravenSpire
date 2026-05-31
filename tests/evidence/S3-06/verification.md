# S3-06 Verification

**Story:** `production/stories/s3-06-playable-end-to-end-and-human-play.md`
**Branch:** `codex/s3-06-playable-end-to-end-and-human-play`
**Base SHA at implementation start:** `cc7da60`
**Date:** 2026-05-30
**Status:** MECHANICAL PASS / HUMAN FEEL GATE NOT PASSED

## Summary

S3-06 AC-01 through AC-06 are mechanically supported by Unity batchmode evidence. The 2026-05-30 project-lead human-play attempt did not pass the feel gate: the assembled slice read as Unity greybox/debug scaffolding rather than a playable classic-MMO-descended gothic slice. A legacy M2 combat debug HUD bleed was identified and kept as a scoped S3-06 presentation-readiness bug fix, but that fix does not retroactively convert AC-08 into PASS.

## Acceptance Criteria

| AC | Result | Evidence |
|---|---|---|
| S3-06-01 | PASS | `Assets/Editor/GravenspireS3PlayableEndToEndRunner.cs` exists and composes existing S3-01 through S3-05 scene/runtime surfaces. No M3 runtime files were modified. |
| S3-06-02 | PASS | `tests/evidence/S3-06/unity-playable-end-to-end-20260530-smoke.md` records walked NavMesh path segments for spawn to Caretaker, Caretaker to relic, relic to vendor, and vendor to Caretaker. |
| S3-06-03 | PASS | `unity-playable-end-to-end-20260530-smoke.md` records the exact target sequence `npc_interaction_intentional>objective_accepted>relic_recovered>objective_loot_resolved>vendor_salvage_sold>vendor_sell_copper_applied>relic_handed_in`, final objective state `Complete`, vendor currency `7`, and final relic carried `True`. This resolves S3-05 AC-12 rollforward mechanically. |
| S3-06-04 | PASS WITH NOTES | `Assets/Scripts/M2SingleTrashMedLoopController.cs` now resets scenario-boundary transient state in the three reset paths. `tests/evidence/S3-06/m2-regression-comparison-20260530.md` compares independent M2 reruns against S3-05 baseline telemetry. Note: the melee random source is fixed-roll/stateless today; the active chain failure was Smite cooldown resolver state after the smoke clock reset. |
| S3-06-05 | PASS | `tests/evidence/S3-06/unity-runner-exception-guard-20260530-smoke.md` intentionally injects `InvalidOperationException`; the runner records `synthetic_exception_guard_probe=FAIL`, continues to later checks, and records only the synthetic error. |
| S3-06-06 | PASS | `tests/evidence/S3-06/unity-end-to-end-chained-m2-20260530-smoke.md` records all three chained M2 smokes PASS after the S3 objective loop, `m2_scenario_boundary_reset_hook_invoked=PASS`, no controller errors, and reset count `0 -> 5`. |
| S3-06-07 | FAIL WITH NOTES | A project-lead human-play attempt happened on 2026-05-30, but it did not produce a completed-loop feel PASS; `tests/evidence/S3-06/human-play-20260530.md` records the failed attempt and why the gate stayed closed. |
| S3-06-08 | FAIL | The binary re-engagement verdict is NOT PASSED. The playtest reaction did not support immediate re-engagement for the objective/NPC/relic; the blocking reason was presentation-readability, not missing mechanical runner proof. |
| S3-06-09 | PASS | `tests/evidence/S3-06/human-play-20260530.md` now records the attempted playtest, verdict computation, methodological limit, presentation limitations, and second-playtester absence. |
| S3-06-10 | PASS | `tests/evidence/S3-06/human-play-20260530.md` classifies the presentation-readability gap and the M2 debug-HUD bleed separately; the HUD fix is logged as a bug fix, not as AC-08 pass evidence. |

## Evidence Artifacts

- `tests/evidence/S3-06/unity-playable-end-to-end-20260530-smoke.md` - PASS, AC-01 through AC-03 plus chained M2 checks.
- `tests/evidence/S3-06/unity-end-to-end-chained-m2-20260530-smoke.md` - PASS, AC-06 focused companion artifact.
- `tests/evidence/S3-06/unity-runner-exception-guard-20260530-smoke.md` - expected FAIL negative control for AC-05; synthetic exception is caught and subsequent real checks continue.
- `tests/evidence/S3-06/m2-02-preservation-20260530-smoke.md` - PASS, independent M2-02 preservation, builder skipped.
- `tests/evidence/S3-06/m2-03-preservation-20260530-smoke.md` - PASS, independent M2-03 preservation, builder skipped.
- `tests/evidence/S3-06/m2-04-preservation-20260530-smoke.md` - PASS, independent M2-04 preservation, builder skipped.
- `tests/evidence/S3-06/m2-regression-comparison-20260530.md` - side-by-side comparison against S3-05 preservation artifacts.
- `tests/evidence/S3-06/human-play-20260530.md` - NOT PASSED human-play evidence for AC-07 through AC-10; records the presentation-readability failure and scoped M2 HUD bug fix.

## Evidence Rule v2 Notes

### End-to-End Runner

- Claim: the closed S3-02/03/04 adapter vocabulary fires in order while walking through the S3-05 district.
- Subject: existing `_DevEntry.unity` scene state plus existing S3 adapters.
- Preconditions: required scene anchors and adapter components are found before Play Mode checks; NavMeshSurface has assigned data; each path segment returns `PathComplete`.
- Exercise: the runner moves the harness player marker along NavMesh path corners, dispatches interaction through `S3PlayerInteractionHarness`, and reads resulting target telemetry.
- Observation: full target event order, final objective state, vendor currency, and relic carry state.
- Negative case: missing adapters, path failure, wrong event order, or final state mismatch would fail named checks and the runner result.

### M2 Scenario Boundary Reset

- Claim: S3-06 resets the scenario-boundary state that made chained M2 smokes non-deterministic.
- Subject: `M2SingleTrashMedLoopController` reset paths and new scenario-boundary reset count.
- Preconditions: independent M2 preservation smokes still match S3-05 baseline telemetry; chained runner invokes all three M2 smokes in one Play session.
- Exercise: independent M2 batchmode reruns plus S3-06 chained M2 runner after the end-to-end objective loop.
- Observation: independent telemetry matches the S3-05 baseline; chained single, linked, and named M2 smokes all pass with no controller errors.
- Negative case: the first S3-06 negative run exposed linked/named chain failures before the resolver reset; this was corrected before the committed PASS artifacts.

### M2 HUD Suppression Bug Fix

- Claim: the legacy M2 combat debug HUD should not bleed over the S3 objective-play view when no combat scenario is active.
- Subject: `Assets/Scripts/M2SingleTrashMedLoopController.cs` `ShouldHideCombatHudForObjectivePlay()` guard.
- Preconditions: the S3 interaction harness exists in the scene; no pull is active; no target is selected; player attack is off; player and hostile combat states are out of combat.
- Exercise: code inspection of the approved S3-06 M2 controller delta after the 2026-05-30 human-play failure.
- Observation: `OnGUI()` returns before drawing the M2 debug HUD under the S3 objective-play preconditions, while preserving the HUD when combat/pull state is active.
- Negative case: if the guard hid the HUD during an active M2 combat scenario, M2 preservation evidence would be invalid; if the guard were absent, the debug HUD would keep contaminating S3 objective-play presentation.
- Evidence limit: this edit records the bug fix and code-inspection basis. It does not rerun the human-play protocol and does not convert AC-08 into PASS.

## Pattern Notes

S3-06 establishes the T1 human-play feel-check evidence shape:

- Binary, pillar-anchored verdict.
- Verbatim re-engagement answer.
- Explicit distinction between loop-feel and greybox presentation deficits.
- N=1 self-test limitation named honestly.
- Optional second-playtester read recorded separately if available.

The human-play attempt is recorded as NOT PASSED. Future Tier-1 feel-check stories should preserve this distinction: a runner can prove a loop functions, while a failed human read must stay failed even when one contributor is later fixed.

## Local Gates

| Gate | Result | Notes |
|---|---|---|
| `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` | PASS | 189/189 tests passed. |
| `dotnet format tests\Gravenspire.Combat.Tests.csproj --verify-no-changes --exclude-diagnostics IDE1006` | PASS | Exit 0. |
| `dotnet format prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj --verify-no-changes --exclude-diagnostics IDE1006` | PASS | Exit 0. |
| `git diff --check` | PASS | Exit 0; PowerShell printed the existing line-ending warning for `M2SingleTrashMedLoopController.cs`, not a diff-check error. |
| Trailing-whitespace scan over changed files | PASS | Exit 1 from `rg` because no matches were found after normalizing the generated `.meta` blank fields. |
| T1 negative-scope scan over changed code and S3-06 evidence | PASS | Exit 1 from `rg` because no matches were found. |
| `.githooks/pre-commit` | PASS | `[pre-commit] OK`. |
