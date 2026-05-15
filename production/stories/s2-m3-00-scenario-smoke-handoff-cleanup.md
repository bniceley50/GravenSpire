# S2-M3-00 - Scenario Smoke Handoff Cleanup

**Status:** Complete
**Sprint:** 2
**Priority:** Must Have
**Layer:** Gameplay / Unity Runtime
**Type:** Integration
**Estimate:** 1.0 days
**Manifest Version:** Sprint 2, 2026-05-14
**GDD:** `design/gdd/combat-core.md`; `design/gdd/game-concept.md`
**Quick Design:** `design/quick/quick-design-m3-objective-npc-loot.md`
**Governing Decisions:** `DECISIONS.md` D001, D002, D003, D012, D014
**Evidence:** `tests/evidence/S2-M3-00/verification.md`

## Routing Status

This is the first M3 story. It is ready for `/story-readiness`. Do not start
M3 NPC, objective, loot, or vendor implementation until this cleanup either
extracts shared scenario-smoke support or proves a new M3 controller can compose
the M2 loop without adding a fourth parallel smoke block to
`M2SingleTrashMedLoopController`.

## Source Trace

- `design/quick/quick-design-m3-objective-npc-loot.md:246` through `:252`
  define M3-00 as scenario-smoke handoff cleanup.
- `design/quick/quick-design-m3-objective-npc-loot.md:216` through `:223`
  require M3 to avoid a fourth parallel scenario-smoke block and preserve M2
  clean-loop, overpull, and named-blocker checks.
- `production/sprint-status.yaml:43` records the pre-M3 carryover:
  `m2_controller_scenario_smoke_abstraction`.
- `production/sprint-status.yaml:44` records the runner noise-filter carryover.
- `Assets/Scripts/M2SingleTrashMedLoopController.cs:315` through `:451` and
  `:1543` through `:1856` show the existing scenario-smoke growth surface.

## Scope

Create a narrow handoff layer for scenario smoke setup, telemetry capture, or
runner support before M3 adds objective-specific checks. The implementation may
extract shared helper methods/classes from the current M2 controller or create a
small M3 composition point, but it must not change player-facing behavior.

Planned implementation surface:

- Shared scenario-smoke setup, telemetry, or runner helper code under `Assets/**`.
- Focused rerun path proving existing M2 checks still pass.
- SearchInit editor-startup noise filtering available to M3 runner capture.
- Verification evidence under `tests/evidence/S2-M3-00/`.

## Out Of Scope

- No NPC, objective, loot, vendor, Save/Load, faction consequence, Dialogue UI,
  companion, extra class, networking, FishNet, server authority, PvP, accounts,
  cloud saves, multiplayer, or live LLM behavior.
- No combat tuning, fixture retuning, FEEL target change, or named-blocker
  behavior change.
- No broad architecture rewrite outside the M2/M3 smoke handoff surface.

## Acceptance Criteria

| ID | Criterion | Evidence |
| --- | --- | --- |
| `S2-M3-00-01` | M3 does not add a fourth parallel 300-400 line scenario-smoke block directly to `M2SingleTrashMedLoopController`. | `tests/evidence/S2-M3-00/verification.md` |
| `S2-M3-00-02` | Shared scenario-smoke setup, telemetry capture, or runner helpers exist where needed before objective-specific smoke lands. | `tests/evidence/S2-M3-00/verification.md` |
| `S2-M3-00-03` | Existing M2 S2-M2-02, S2-M2-03, and S2-M2-04 scenario checks still pass after cleanup. | `tests/evidence/S2-M3-00/verification.md` |
| `S2-M3-00-04` | The `UnityEditor.Search.SearchInit` editor-startup noise filter is shared or explicitly available to M3 runner log capture. | `tests/evidence/S2-M3-00/verification.md` |
| `S2-M3-00-05` | No gameplay behavior changes are introduced by the cleanup story; the extract/compose change is behavior-preserving. | `tests/evidence/S2-M3-00/verification.md` |
| `S2-M3-00-06` | Dotnet regression, T1 negative-scope scan, `git diff --check`, and `.githooks/pre-commit` pass before closure. | `tests/evidence/S2-M3-00/verification.md` |

## QA Test Cases

- **S2-M3-00-01**: No fourth parallel block
  - Given: the current M2 controller has three scenario-smoke paths.
  - When: the cleanup lands.
  - Then: no new M3 objective-specific scenario block is added directly to the controller.
  - Edge cases: small call-site edits are allowed if behavior remains unchanged.
- **S2-M3-00-02**: Shared helper exists
  - Given: M3 needs NPC/objective/loot/vendor telemetry later.
  - When: smoke setup or telemetry is inspected.
  - Then: the reusable surface is available without duplicating an entire scenario body.
- **S2-M3-00-03**: M2 preservation
  - Given: S2-M2-02 through S2-M2-04 are complete.
  - When: focused M2 smoke reruns or equivalent verification runs.
  - Then: clean-loop, overpull, and named-blocker proofs still pass.
- **S2-M3-00-04**: Runner log noise filter
  - Given: fresh-worktree Unity batchmode can emit `SearchInit` editor noise.
  - When: M3 runner log capture is configured.
  - Then: diagnosed editor-startup noise is filtered without suppressing runtime errors.
- **S2-M3-00-05**: No behavior change
  - Given: the cleanup extracts or composes scenario-smoke support.
  - When: changed files are reviewed and M2 smoke is rerun.
  - Then: no player-facing or scenario behavior differs from before the cleanup.
  - Edge cases: small call-site edits are allowed only if behavior is identical.
- **S2-M3-00-06**: Local gates
  - Given: the cleanup is complete.
  - When: required local gates run.
  - Then: all pass or exact blockers are recorded.

## Test Evidence

Required evidence:

- `tests/evidence/S2-M3-00/verification.md`
- Focused Unity Play Mode or batchmode runner evidence for M2 preservation.
- `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"`
- T1 negative-scope scan over changed files.
- `git diff --check`
- `.githooks/pre-commit`

## Performance Budget

This story should reduce or isolate future scenario-smoke growth. It must not
add a new steady-state gameplay loop, per-frame allocation path, objective
poller, loot evaluator, vendor evaluator, or broad scene scan.

## Dependencies

- Depends on: `S2-M2-04` complete and `design/quick/quick-design-m3-objective-npc-loot.md` committed.
- Unlocks: `S2-M3-01` Named NPC Objective Frame.

## Next Gate

Closed via `/story-done` 2026-05-15. M3-01 dependency on S2-M3-00 is now satisfied. User chose (C) Full art-director path before resuming M3-01+; next gate is AD-ART-BIBLE sign-off via the `art-director` agent, not `/story-readiness` on M3-01.

## Completion Notes

**Completed**: 2026-05-15
**Verdict**: COMPLETE WITH NOTES
**Criteria**: 6/6 passing (0 deferred, 0 untested)
**Deferred/Untested Criteria**: None
**Test Evidence**: `tests/evidence/S2-M3-00/verification.md` (Result PASS); 4 Unity 6000.3.14f1 batchmode rerun smoke files (15+16+15+17 = 63 checks all PASS, all exit 0); 4 redacted Unity logs; informal `human-play-20260515.md` carryover scaffold (non-AC).
**GDD/ADR Deviations**: None.
**Scope Notes**: All 16 files in scope. `human-play-20260515.md` added as informal user-approved carryover documentation, explicitly disclaimed as non-AC.
**Review Gates**: Lean mode skipped §9 subagent gates. Out-of-band review pair (`reviewer` + `unity-specialist`) both returned PASS_WITH_NOTES. Manual review pass caught and patched 3 P1/P2/P3 findings (license-Id leak, multicast-port leak, evidence-count miscount). Unity-spec F1 (unused `using UnityEngine;`) applied in-session. F2-F5 advisory/INFO deferred as new carryovers (see `production/sprint-status.yaml`).
**Forced Completion**: No.
**Carryovers closed**: `m2_runner_editor_noise_capture` — 3 older M2 runners now share the SearchInit filter via `Assets/Editor/GravenspireScenarioSmokeRunnerSupport.cs:36`.
**Carryovers added (4)**: `m2_runner_capturelog_dedup`, `m2_runner_helper_allowlist_grow`, `runner_baked_date_strings`, `launch_verification_runner_searchinit_backport`. See `production/sprint-status.yaml`.
