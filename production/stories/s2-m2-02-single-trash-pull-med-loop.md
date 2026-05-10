# S2-M2-02 - Single Trash Pull + Med Loop

**Status:** Blocked
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

Blocked until `S2-M2-01` is complete. This story should not implement a parallel
bridge or duplicate runtime adapter work; it builds the first playable loop on
top of the bridge.

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

## Test Evidence

Required evidence:

- `tests/evidence/S2-M2-02/verification.md`
- Story-specific Unity Play Mode or batchmode runner output for the two-pull
  med-loop smoke.
- `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"`
- T1 negative-scope scan over changed files.
- `git diff --check`
- `.githooks/pre-commit`

## Dependencies

- Depends on: `S2-M2-01` Unity Combat Core Runtime Bridge complete.
- Unlocks: `S2-M2-03` Linked Trash Overpull.

## Next Gate

Blocked until `S2-M2-01` is closed.
