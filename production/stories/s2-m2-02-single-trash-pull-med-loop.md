# S2-M2-02 - Single Trash Pull + Med Loop

**Status:** Complete
**Sprint:** 2
**Priority:** Must Have
**Layer:** Gameplay / Unity Runtime
**Type:** Integration / Visual-Feel
**Estimate:** 1.0 days
**Manifest Version:** Sprint 2, 2026-05-10
**GDD:** `design/gdd/combat-core.md`; `design/gdd/game-concept.md`
**Quick Design:** `design/quick/quick-design-m2-combat-camp-loop.md`
**Governing Decisions:** `DECISIONS.md` D001, D002, D003, D012, D014
**Evidence:** `tests/evidence/S2-M2-02/verification.md`

## Routing Status

This story is ready for dev. `S2-M2-01` (Unity Combat Core runtime bridge) is
complete at commit `b4cb377`, closed at `87c9dcd`, with Unity ProjectSettings
hygiene landed at `da4e177`. M2-02 builds the first playable loop on the
existing bridge; no parallel bridge or duplicate runtime adapter work is needed.

## Source Trace

- `production/sprints/sprint-2.md:46` defines M2's proof target: three enemy
  types support pull, fight, med-break, and recovery pacing.
- `design/quick/quick-design-m2-combat-camp-loop.md:70` through
  `design/quick/quick-design-m2-combat-camp-loop.md:82` define the M2 loop:
  start at camp, pull, fight, sit/med, repeat, then test overpull/named
  boundaries.
- `design/quick/quick-design-m2-combat-camp-loop.md:84` through
  `design/quick/quick-design-m2-combat-camp-loop.md:107` define the baseline
  trash role and evidence anchors.
- `design/quick/quick-design-m2-combat-camp-loop.md:205` through
  `design/quick/quick-design-m2-combat-camp-loop.md:211` define the single
  trash pull and med-loop acceptance-candidate shape.
- `design/gdd/combat-core.md:111` through `design/gdd/combat-core.md:119`
  define targeting, explicit Attack toggle, and pull behavior.
- `design/gdd/combat-core.md:174` through `design/gdd/combat-core.md:176`
  define sitting, unsafe sitting, and med-break regeneration.
- `design/gdd/combat-core.md:429` defines `SoloTrash_EvenCon_T1`.
- `design/gdd/combat-core.md:800` through `design/gdd/combat-core.md:817`
  define FEEL-01 single-trash pressure and FEEL-04 med-break pacing.
- `data/combat/t1-combat-fixtures.json:496` through
  `data/combat/t1-combat-fixtures.json:512` define the approved solo-trash
  encounter fixture.

## Scope

Add the first playable combat-camp loop to `_DevEntry.unity`: a safe camp/rest
point, a short pull lane, a baseline trash anchor, and runtime input/state
plumbing that lets the player pull one hostile, explicitly toggle Attack, fight,
exit combat, sit, recover mana, and repeat a second pull.

Planned implementation surface:

- `M2_CombatCampLoopRoot`, `M2_CampRestPoint`, `M2_PullLane`, and
  `M2_BaselineTrash` scene objects or equivalents.
- Runtime input for target, Attack toggle, cast/instant if needed by the
  approved fixture, sit, and stand.
- Story-specific smoke proving two sequential clean pulls with a med break.
- Evidence that the pull initializes hostile intent without auto-enabling
  Attack.

## Out Of Scope

- No linked/patrol trash overpull setup; `S2-M2-03` owns the second-enemy role.
- No named blocker, named tuning, or camp-boundary proof; `S2-M2-04` owns it.
- No objective, quest, named friendly NPC, loot, item pickup, vendor, stash,
  Save/Load flow, or visible faction consequence.
- No networking, FishNet, server authority, PvP, accounts, cloud saves, live LLM,
  extra playable classes, broad companion behavior, second district, or deep
  economy.
- No Combat Core formula rewrite, fixture retuning, final HUD, audio system,
  animation pass, broad Creature / Enemy AI, or opportunistic cleanup of known
  non-M2 findings.

## Acceptance Criteria

| ID | Criterion | Evidence |
| --- | --- | --- |
| `S2-M2-02-01` | `_DevEntry.unity` contains the M2 camp rest point, pull lane, Cleric/player marker, and baseline trash anchor. | `tests/evidence/S2-M2-02/verification.md` |
| `S2-M2-02-02` | Player can body/LoS pull one baseline trash enemy, and the pull does not automatically enable Attack. | `tests/evidence/S2-M2-02/verification.md` |
| `S2-M2-02-03` | Player can target, toggle Attack, resolve a clean single-trash fight, exit combat, sit, recover mana, and repeat a second pull. | `tests/evidence/S2-M2-02/verification.md` |
| `S2-M2-02-04` | Runtime smoke records pull start, Attack on/off transitions, hostile defeat, combat exit, sit/med start, mana restoration, and no captured errors or exceptions. | `tests/evidence/S2-M2-02/verification.md` |
| `S2-M2-02-05` | `S2-M2-01` bridge smoke and dotnet combat regression still pass after the loop objects are added. | `tests/evidence/S2-M2-02/verification.md` |
| `S2-M2-02-06` | Human play session is recorded with the player's answer to "did you want one more pull?" and at least one worst-thing finding; the worst-thing is either fixed in-story or explicitly carried forward in closure notes. | `tests/evidence/S2-M2-02/human-play-YYYYMMDD.md` |

## QA Test Cases

- **S2-M2-02-01**: Scene loop shape
  - Given: `_DevEntry.unity` is opened after M2-01 is complete.
  - When: scene objects are inspected through the runner or editor.
  - Then: camp rest point, pull lane, player marker, and baseline trash anchor are present.
  - Edge cases: placeholder visuals are acceptable; missing anchors block closure.
- **S2-M2-02-02**: Pull without Attack auto-enable
  - Given: the player approaches baseline trash in the pull lane.
  - When: body/LoS pull begins.
  - Then: hostile intent/threat starts and Attack remains off until player input toggles it on.
  - Edge cases: target selection, tab cycling, or spell cast must not secretly toggle Attack on.
- **S2-M2-02-03**: Two clean pulls with med break
  - Given: the player starts above the approved clean-state fixture thresholds.
  - When: the player completes a baseline trash fight, exits combat, sits, regens mana, and pulls again.
  - Then: both pulls complete through Combat Core behavior and the med break measurably restores mana.
  - Edge cases: sitting during active combat must not grant the out-of-combat med boost.
- **S2-M2-02-04**: Runtime evidence log
  - Given: the story-specific smoke runner executes the loop.
  - When: it records state transitions.
  - Then: the verification artifact includes pull start, Attack transitions, hostile defeat, combat exit, sit/med start, mana restoration, and no errors/exceptions.
  - Edge cases: captured exceptions or missing transition records fail closure.
- **S2-M2-02-05**: Bridge regression
  - Given: M2-02 scene/runtime changes are complete.
  - When: M2-01 bridge smoke and dotnet combat regression are rerun.
  - Then: both still pass or exact blockers are recorded before closure.
  - Edge cases: bridge regressions are fixed in this story only if caused by M2-02 changes.
- **S2-M2-02-06**: Human play session
  - Given: M2-02 implementation is functional under Unity Play Mode.
  - When: a human runs the two-pull med-loop in-editor for at least one short session.
  - Then: written evidence records the answer to "did you want one more pull?" with at least one worst-thing finding; either fixed in this story or carried forward in closure notes.
  - Edge cases: feel is subjective; session need only be one short run, not a polish pass. CLI evidence alone is insufficient for closure.

## Test Evidence

Required evidence:

- `tests/evidence/S2-M2-02/verification.md`
- Story-specific Unity Play Mode or batchmode runner output for the two-pull
  med-loop smoke.
- `tests/evidence/S2-M2-02/human-play-YYYYMMDD.md` (player feel session note
  with answer to "did you want one more pull?" and at least one worst-thing
  finding)
- `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"`
- T1 negative-scope scan over changed files.
- `git diff --check`
- `.githooks/pre-commit`

## Performance Budget

This story introduces the first playable single-trash loop and should remain
bounded to one player actor, one baseline hostile actor, and the minimum camp
objects needed for pull, fight, sit, med, and repeat evidence. Authored runtime
code must not allocate every frame in the target, Attack toggle, combat-exit,
sit/stand, or mana-restoration paths. Any Unity smoke or profiler evidence
should focus on errors/exceptions, repeated-loop stability, and obvious
steady-state allocation regressions rather than full frame-time tuning.

## Dependencies

- Depends on: `S2-M2-01` Unity Combat Core Runtime Bridge complete.
- Unlocks: `S2-M2-03` Linked Trash Overpull.

## Next Gate

`/dev-story production/stories/s2-m2-02-single-trash-pull-med-loop.md`.

## Completion Notes

**Completed:** 2026-05-12
**Verdict:** COMPLETE WITH NOTES
**Criteria:** 6/6 passing (`S2-M2-02-01` through `S2-M2-02-06`)
**Deferred/Untested Criteria:** None
**Test Evidence:**
- `tests/evidence/S2-M2-02/verification.md` (gate evidence index)
- `tests/evidence/S2-M2-02/human-play-20260512.md` (AC-6 qualified-no answer + worst-thing finding carried forward)
- `tests/evidence/S2-M2-02/unity-single-trash-med-loop-runner-20260512-postpatch-smoke.md` (post-patch loop smoke with phase guards)
- `tests/evidence/S2-M2-02/unity-m2-01-bridge-regression-20260512-postpatch-smoke.md` (post-patch bridge regression)
- `tests/integration/gameplay/combat/combat_runtime_single_trash_med_loop_test.cs` (integration test asserting zero auto-enabled Attack pulls across both pulls)
- `dotnet test tests\Gravenspire.Combat.Tests.csproj` 170/170 PASS on 2026-05-12

**GDD/ADR Deviations:** None. All Governing Decisions (D001, D002, D003, D012, D014) remain Locked per `DECISIONS.md`.

**Scope Notes:** All file changes within declared story scope (scene loop objects, runtime controller, story-specific smoke, integration test, story-specific evidence). No SCOPE CREEP.

**Review Gates:**
- `/code-review 946c9d1` pass 1 (lead-programmer + qa-tester + gameplay-programmer): qa-tester returned STILL BLOCKED on evidence integrity (K1, K2a, K2b, N1, N2); lead-programmer + gameplay-programmer returned APPROVED WITH NOTES on the runtime patch.
- Evidence-only patch landed at commit `350b06e`.
- `/code-review 350b06e` pass 2 (focused qa-tester): APPROVED — all 5 prior findings CLOSED, clean residual-drift sweep.
- Aggregate: APPROVED WITH NOTES.

**Forced Completion:** No.

### Carryover Items

Three keys recorded in `production/sprint-status.yaml`:

- `m2_renderer_material_property_access`: Convergent finding from lead-programmer + gameplay-programmer code review. `Assets/Scripts/M2SingleTrashMedLoopController.cs:1168` reads `renderer.material` on the `Update` hot path via `ApplySceneVisualState` (line :123). Unity's `.material` property instantiates a per-renderer copy on first access (one-time per renderer, not per-frame), below the P1-2 blocking bar that targeted explicit `new Material()` in `Update`. T1 prototype acceptable; convert to `.sharedMaterial` + `MaterialPropertyBlock` before M3+ presentation polish or higher entity counts.
- `s2_bridge_runner_evidence_path_hardcoded`: Lead-programmer note. `Assets/Editor/GravenspireM2CombatBridgeVerificationRunner.cs:21,:199` hard-codes the date `2026-05-10` and writes to an S2-M2-01 evidence path. Pre-patch behavior wrote there; harden patch (`946c9d1`) routed the post-patch run to a distinct S2-M2-02 path, resolving the immediate overwrite-collision risk. Cosmetic literal cleanup remains.
- `m2_presentation_threshold_gap`: Human-play finding from AC-6 evidence at `tests/evidence/S2-M2-02/human-play-20260512.md:42-56`. Blockout-quality presentation (capsule actors, flat floor, debug HUD) is insufficient to validate gameplay feel via the "did you want one more pull?" bar. Routing decision: `S2-M2-03` accepts qualified human-play findings because linked-trash overpull is primarily a mechanical risk/stakes validation. Explicit revisit trigger before `S2-M2-04`, where presentation/discovery may matter more.

### Sprint-level WATCH (not a carryover key)

- Integration test inner harness uses `camelCase` private fields (e.g. `hitRoll`, `damageRollScalar`, `package`) rather than the project's `_camelCase` convention per [technical-preferences.md](.claude/docs/technical-preferences.md). Pre-existing in `93b460e`, outside the harden/evidence rubric. Defer to a future style-pass batch.

### Closure Next Gate

`/story-readiness production/stories/s2-m2-03-linked-trash-overpull.md`.
