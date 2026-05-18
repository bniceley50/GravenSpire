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

This first M3 story is complete. It established shared editor-runner support for
scenario-smoke log filtering, proved the M2 preservation runners still pass, and
kept M3 NPC/objective/loot/vendor work blocked until the handoff cleanup was
closed.

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

Complete. Next Sprint 2 M3 gate:

`/story-readiness production/stories/s2-m3-01-named-npc-objective-frame.md`

## Completion Notes

**Completed:** 2026-05-17
**Verdict:** COMPLETE WITH NOTES
**Criteria:** 6/6 passing.
**Deviations / Notes:**
- Launch runner evidence-path parity is a follow-up carryover: `Assets/Editor/GravenspireLaunchVerificationRunner.cs:18` hardcodes S2-FOUNDATION-01 evidence and cannot safely participate in S2-M3-00 `-gravenspireEvidencePath` closure reruns without a small runner hygiene patch.
- `m2_controller_scenario_smoke_abstraction` is partially addressed: this story intentionally extracted shared runner support only and did not refactor `Assets/Scripts/M2SingleTrashMedLoopController.cs`, preserving gameplay behavior.
**Test Evidence:**
- `tests/evidence/S2-M3-00/verification.md`
- `tests/evidence/S2-M3-00/m2-01-rerun-20260517-smoke.md`
- `tests/evidence/S2-M3-00/m2-02-rerun-20260517-smoke.md`
- `tests/evidence/S2-M3-00/m2-03-rerun-20260517-smoke.md`
- `tests/evidence/S2-M3-00/m2-04-rerun-20260517-smoke.md`
**Code Review:** Complete — `/code-review` initially BLOCKED on Launch evidence-path and pre-commit evidence labeling; follow-up evidence/session-state fixes passed the third convergent verification.
