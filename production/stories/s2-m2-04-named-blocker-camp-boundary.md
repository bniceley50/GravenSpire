# S2-M2-04 - Named Blocker + Camp Boundary

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
**Evidence:** `tests/evidence/S2-M2-04/verification.md`

## Routing Status

Blocked until `S2-M2-03` is complete. This story adds the third M2 enemy role:
a visible named blocker that proves camp boundary and group-dependency pressure
without adding loot, objectives, or future party systems.

## Source Trace

- `production/sprints/sprint-2.md:46` defines M2's proof target: three enemy
  types support pull, fight, med-break, and recovery pacing.
- `design/quick/quick-design-m2-combat-camp-loop.md:129` through
  `design/quick/quick-design-m2-combat-camp-loop.md:149` define the named
  blocker enemy role and evidence anchors.
- `design/quick/quick-design-m2-combat-camp-loop.md:220` through
  `design/quick/quick-design-m2-combat-camp-loop.md:225` define the named
  blocker acceptance-candidate shape.
- `design/gdd/combat-core.md:57` defines named enemies and camps as the useful
  absence-of-group signal for the T1 Cleric.
- `design/gdd/combat-core.md:164` through `design/gdd/combat-core.md:168`
  define `encounter_role = Named` and camp metadata.
- `design/gdd/combat-core.md:431` defines `NamedSoloBlock_T1`.
- `design/gdd/combat-core.md:808` through `design/gdd/combat-core.md:810`
  define FEEL-02 named enemy not soloable.
- `data/combat/t1-combat-fixtures.json:528` through
  `data/combat/t1-combat-fixtures.json:538` define the approved named solo-block
  encounter fixture.

## Scope

Add a visible, targetable named blocker anchor to the M2 camp and prove it is
not normal solo-trash farm content. The story closes M2's three-enemy-type proof:
baseline trash for the loop, linked/patrol trash for overpull danger, and named
blocker for camp boundary.

Planned implementation surface:

- `M2_NamedBlocker` scene anchor or equivalent visible targetable named marker.
- Runtime fixture handoff for the existing named solo-block fixture family.
- Story-specific smoke that records named presence/targetability and a blocked,
  failed, loss, or flee outcome consistent with FEEL-02.
- Evidence that no loot, objective, faction consequence, companion, or extra
  class behavior is added to make the named killable.

## Out Of Scope

- No loot table, item drop, named reward, quest objective, named friendly NPC,
  vendor, stash, Save/Load flow, or visible faction consequence.
- No Warrior, Enchanter, companion, party support, resurrection, corpse-run,
  XP-loss, or named/boss soloability tuning.
- No networking, FishNet, server authority, PvP, accounts, cloud saves, live LLM,
  extra playable classes, broad companion behavior, second district, or deep
  economy.
- No full boss script, encounter reset UI, instancing, faction simulation,
  final HUD/audio/animation pass, or opportunistic cleanup of known non-M2
  findings.

## Acceptance Criteria

| ID | Criterion | Evidence |
| --- | --- | --- |
| `S2-M2-04-01` | `_DevEntry.unity` contains a visible named blocker anchor using the existing named fixture family. | `tests/evidence/S2-M2-04/verification.md` |
| `S2-M2-04-02` | Runtime smoke verifies the named blocker is present and targetable but not treated as normal solo-trash farm content. | `tests/evidence/S2-M2-04/verification.md` |
| `S2-M2-04-03` | Named attempt evidence records loss, flee, failed solo attempt, or blocked attempt consistent with FEEL-02. | `tests/evidence/S2-M2-04/verification.md` |
| `S2-M2-04-04` | M2 still contains no loot, objective, faction consequence, Save/Load, companion, or extra-class behavior. | `tests/evidence/S2-M2-04/verification.md` |
| `S2-M2-04-05` | M2-02 clean loop, M2-03 overpull proof, dotnet combat regression, T1 negative-scope scan, `git diff --check`, and `.githooks/pre-commit` pass before closure. | `tests/evidence/S2-M2-04/verification.md` |

## QA Test Cases

- **S2-M2-04-01**: Named anchor exists
  - Given: M2-03 is complete.
  - When: `_DevEntry.unity` is inspected.
  - Then: a visible named blocker anchor using the named fixture family is present.
  - Edge cases: placeholder visuals are acceptable; missing named fixture handoff blocks closure.
- **S2-M2-04-02**: Present and targetable, not farmable trash
  - Given: the player can target M2 enemies.
  - When: the named blocker is selected or approached.
  - Then: runtime evidence records the named as present/targetable and distinguishes it from baseline trash.
  - Edge cases: named should not use the solo-trash fixture id or easy-trash tuning.
- **S2-M2-04-03**: FEEL-02 boundary proof
  - Given: the player attempts the named solo without companion/party support.
  - When: the attempt resolves or is blocked/fled.
  - Then: evidence records a loss, flee, failed solo attempt, or blocked attempt consistent with FEEL-02.
  - Edge cases: a comfortable named solo kill is a tuning defect unless marked exploit-under-investigation.
- **S2-M2-04-04**: No future-content leakage
  - Given: the named blocker exists.
  - When: changed files are inspected.
  - Then: there is no loot, objective, faction consequence, Save/Load, companion, or extra-class behavior.
  - Edge cases: debug labels are acceptable if they do not become gameplay systems.
- **S2-M2-04-05**: Prior loop preservation and gates
  - Given: M2-04 changes are complete.
  - When: M2-02, M2-03, dotnet, negative-scope, diff, and hook gates run.
  - Then: all pass or exact blockers are recorded before closure.
  - Edge cases: regressions in earlier M2 proofs block story closure.

## Test Evidence

Required evidence:

- `tests/evidence/S2-M2-04/verification.md`
- Story-specific Unity Play Mode or batchmode runner output for named
  presence/targetability and boundary proof.
- Prior M2 clean-loop and overpull smoke rechecks.
- `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"`
- T1 negative-scope scan over changed files.
- `git diff --check`
- `.githooks/pre-commit`

## Performance Budget

This story adds a visible named blocker anchor and named-fixture handoff only.
It must not introduce boss AI, perception sweeps, animation-state complexity,
loot evaluation, objective polling, or faction/save processing. Verification
should confirm the named blocker is present and targetable without adding a new
steady-state gameplay loop beyond the existing M2 combat checks; broader named
encounter performance budgets belong to later grouped-content or boss stories.

## Dependencies

- Depends on: `S2-M2-03` Linked Trash Overpull complete.
- Unlocks: M3 Objective + NPC + Loot story-breaking.

## Next Gate

Blocked until `S2-M2-03` is closed.

## Completion Notes

**Completed**: 2026-05-14
**Verdict**: COMPLETE WITH NOTES
**Criteria**: 5/5 passing
**Deferred/Untested Criteria**: None
**Test Evidence**: `tests/evidence/S2-M2-04/verification.md`, `tests/evidence/S2-M2-04/unity-named-blocker-runner-20260514-smoke.md`, `tests/evidence/S2-M2-04/unity-named-blocker-runner-20260514.log`, `tests/integration/gameplay/combat/combat_runtime_named_blocker_boundary_test.cs`
**GDD/ADR Deviations**: None — implementation aligns with `combat-core.md` FEEL-02 (H-CCOM-FEEL-02) and the `NamedSoloBlock_T1` fixture family; governing DECISIONS D001, D002, D003, D012, D014 remain Locked. No `Proposed` ADR implemented against.
**Scope Notes**: Implementation committed at `ccb0c03` (11 files, 2083 insertions, 6 deletions). The M2-02 presentation-threshold revisit trigger (`m2_presentation_threshold_gap`) was resolved at `/story-readiness` with routing option (a): the named blocker is mechanically proven through telemetry (discovery, time-to-danger, boundary pressure, clean-loop preservation, no farm-through); human-play remains a qualified supplement, not a blocking closure gate. Unity log redacted for local-machine identifiers (username, licensing handshake IDs, network interface IPs/ports). `ProjectSettings/ShaderGraphSettings.asset` cold-import re-serialization was restored to committed state — not part of the changeset. The runner's `CaptureLog` was hardened to filter a diagnosed `UnityEditor.Search.SearchInit` editor-startup exception (Unity editor noise, outside the named-blocker runtime).
**Review Gates**: `/story-done` ran in lean mode (§9 subagent gates skipped). The code-review gate was satisfied separately by `/code-review ccb0c03` on 2026-05-14 — verdict PASS_WITH_NOTES from the main reviewer plus a `unity-specialist` subagent; no blocking or high-priority findings. One MEDIUM (controller god-object growth → carryover `m2_controller_scenario_smoke_abstraction`) and LOW advisories. Local gates: dotnet 175/175, T1 negative-scope scan PASS, `git diff --check` PASS, `.githooks/pre-commit` PASS, Unity batchmode smoke 17/17 checks PASS.
**Forced Completion**: No
