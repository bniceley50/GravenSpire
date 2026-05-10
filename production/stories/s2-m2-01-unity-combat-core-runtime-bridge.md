# S2-M2-01 - Unity Combat Core Runtime Bridge

**Status:** Ready for Story Readiness
**Sprint:** 2
**Priority:** Must Have
**Layer:** Gameplay / Unity Runtime
**Type:** Integration
**Estimate:** 1.0 days
**Manifest Version:** Sprint 2, 2026-05-10
**GDD:** `design/gdd/combat-core.md`; `design/gdd/game-concept.md`
**Quick Design:** `design/quick/quick-design-m2-combat-camp-loop.md`
**Governing Decisions:** `DECISIONS.md` D001, D002, D003, D012, D014
**Evidence:** `tests/evidence/S2-M2-01/verification.md`

## Routing Status

This is the first M2 Combat Camp Loop story. It is ready for `/story-readiness`.
Do not start implementation until readiness confirms the runtime bridge surface,
test approach, and evidence target.

## Source Trace

- `production/sprints/sprint-2.md:46` defines M2: put existing Combat Core into
  a playable encounter loop where three enemy types support pull, fight,
  med-break, and recovery pacing.
- `design/quick/quick-design-m2-combat-camp-loop.md:151` through
  `design/quick/quick-design-m2-combat-camp-loop.md:169` define the Unity
  integration approach: Unity wraps Combat Core with runtime adapters, scene
  objects, input plumbing, and story-specific smoke evidence.
- `design/quick/quick-design-m2-combat-camp-loop.md:197` through
  `design/quick/quick-design-m2-combat-camp-loop.md:203` list the runtime
  bridge acceptance-candidate shape.
- `design/quick/quick-design-m2-combat-camp-loop.md:236` through
  `design/quick/quick-design-m2-combat-camp-loop.md:257` define the M2 evidence
  plan and runner successor requirement.
- `design/gdd/combat-core.md:41` through `design/gdd/combat-core.md:49` define
  Combat Core ownership and the T1 Cleric solo-trash boundary.
- `design/gdd/combat-core.md:111` through `design/gdd/combat-core.md:119`
  define target selection, Attack toggle separation, and pull behavior.
- `design/gdd/combat-core.md:904` through `design/gdd/combat-core.md:917`
  preserve Combat Core non-goals for PvP, networking, future classes,
  companions, loot, XP, corpse runs, live LLM, and ranged weapons.
- `data/combat/t1-combat-fixtures.json:496` through
  `data/combat/t1-combat-fixtures.json:538` provide the existing encounter
  fixture ids M2 must consume rather than duplicate.

## Scope

Create the minimum Unity runtime bridge that lets `_DevEntry.unity` consume or
hydrate existing Combat Core actors and fixtures without reimplementing combat
math in Unity scripts. The bridge should prove that authored Unity runtime code
can call into the existing engine-agnostic Combat Core and produce story-specific
smoke evidence.

Planned implementation surface:

- A narrow Unity runtime adapter/coordinator under `Assets/**`.
- Fixture load or handoff from `data/combat/t1-combat-fixtures.json`.
- A story-specific Unity smoke runner successor or refactor that writes to
  `tests/evidence/S2-M2-01/`.
- Minimal Play Mode proof that a player actor and at least one hostile actor
  hydrate into the verified `_DevEntry.unity` scene.
- Dotnet combat regression evidence proving the bridge did not alter Combat
  Core behavior.

## Out Of Scope

- No full pull lane, baseline trash loop, med-break repeat loop, linked overpull,
  or named blocker encounter completion; M2-02 through M2-04 own those behaviors.
- No Unity-only duplicate combat formulas, tuning math, fixture interpretation,
  threat model, Attack toggle model, regen model, or death/kill event semantics.
- No objective, quest, named friendly NPC, loot, item pickup, vendor, stash,
  Save/Load flow, or visible faction consequence.
- No networking, FishNet, server authority, PvP, accounts, cloud saves, live LLM,
  extra playable classes, broad companion behavior, second district, or deep
  economy.
- No opportunistic cleanup of known documentation drift: Save/Load metadata,
  README template wording, game-concept engine wording, Unity build-settings
  GUID parity, or Unity Test Runner XML absence.

## Acceptance Criteria

| ID | Criterion | Evidence |
| --- | --- | --- |
| `S2-M2-01-01` | Unity runtime compiles with a thin adapter over `src/gameplay/combat/**`; no Unity-only duplicate combat formulas are introduced. | `tests/evidence/S2-M2-01/verification.md` |
| `S2-M2-01-02` | Runtime can load or receive the existing T1 combat fixtures from `data/combat/t1-combat-fixtures.json`. | `tests/evidence/S2-M2-01/verification.md` |
| `S2-M2-01-03` | `_DevEntry.unity` can enter Play Mode with the bridge enabled, a player actor, at least one hostile actor, fixture ids, and active zone id recorded, with no captured errors or exceptions. | `tests/evidence/S2-M2-01/verification.md` |
| `S2-M2-01-04` | A story-specific Unity smoke runner successor or refactor exists; it does not write to `tests/evidence/S2-FOUNDATION-01/`, and it records M2 bridge checks under `tests/evidence/S2-M2-01/`. | `tests/evidence/S2-M2-01/verification.md` |
| `S2-M2-01-05` | Dotnet combat regression, T1 negative-scope scan, `git diff --check`, and `.githooks/pre-commit` pass before closure. | `tests/evidence/S2-M2-01/verification.md` |

## QA Test Cases

- **S2-M2-01-01**: Thin bridge compile
  - Given: the Unity project and Combat Core source are present on the current branch.
  - When: the implementation compiles in Unity or batchmode.
  - Then: runtime adapter code references Combat Core APIs and does not contain duplicated combat formulas.
  - Edge cases: formula-like constants in Unity scripts must be justified as display or scene config, not combat math.
- **S2-M2-01-02**: Fixture handoff
  - Given: `data/combat/t1-combat-fixtures.json` contains the approved T1 encounter fixtures.
  - When: Play Mode bridge smoke runs.
  - Then: the evidence records the loaded or received fixture ids and fails loudly if fixture loading/handoff fails.
  - Edge cases: missing fixture file, malformed fixture data, or wrong fixture id must fail the smoke.
- **S2-M2-01-03**: Runtime actor smoke
  - Given: `_DevEntry.unity` is opened with the M2 bridge enabled.
  - When: the story-specific smoke runner enters Play Mode.
  - Then: the scene records a player actor, at least one hostile actor, active zone id, and no captured errors or exceptions.
  - Edge cases: shell objects from S2-FOUNDATION-01 must still exist unless intentionally superseded and documented.
- **S2-M2-01-04**: Runner successor evidence isolation
  - Given: `GravenspireLaunchVerificationRunner` writes S2-FOUNDATION-01 evidence.
  - When: M2 bridge verification runs.
  - Then: evidence is written under `tests/evidence/S2-M2-01/`, and no S2-FOUNDATION-01 evidence artifact is modified.
  - Edge cases: if the foundation runner is refactored, S2-FOUNDATION-01 launch checks must remain reproducible.
- **S2-M2-01-05**: Regression and hygiene gates
  - Given: implementation is complete.
  - When: the required local gates run.
  - Then: combat regression, negative-scope scan, diff hygiene, and pre-commit gate all pass or record exact blockers.
  - Edge cases: documentation-only T1 forbidden terms are classified; runtime forbidden-scope hits block closure.

## Test Evidence

Required evidence:

- `tests/evidence/S2-M2-01/verification.md`
- `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"`
- Unity Play Mode or batchmode runner output for the story-specific bridge smoke.
- T1 negative-scope scan over changed story/runtime files.
- `git diff --check`
- `.githooks/pre-commit`

## Dependencies

- Depends on: `S2-FOUNDATION-01` complete and Unity launch verification complete.
- Unlocks: `S2-M2-02` Single Trash Pull + Med Loop.

## Next Gate

Run:

```text
/story-readiness production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md
```
