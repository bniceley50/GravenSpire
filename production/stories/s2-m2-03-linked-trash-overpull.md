# S2-M2-03 - Linked Trash Overpull

**Status:** In Progress
**Sprint:** 2
**Priority:** Must Have
**Layer:** Gameplay / Unity Runtime
**Type:** Integration / Visual-Feel
**Estimate:** 1.0 days
**Manifest Version:** Sprint 2, 2026-05-10
**GDD:** `design/gdd/combat-core.md`; `design/gdd/game-concept.md`
**Quick Design:** `design/quick/quick-design-m2-combat-camp-loop.md`
**Governing Decisions:** `DECISIONS.md` D001, D002, D003, D012, D014
**Evidence:** `tests/evidence/S2-M2-03/verification.md`

## Routing Status

Blocked until `S2-M2-02` is complete. This story adds overpull pressure to the
existing single-trash loop without broadening M2 into a full Creature / Enemy AI
implementation.

## Source Trace

- `production/sprints/sprint-2.md:46` defines M2's proof target: three enemy
  types support pull, fight, med-break, and recovery pacing.
- `design/quick/quick-design-m2-combat-camp-loop.md:108` through
  `design/quick/quick-design-m2-combat-camp-loop.md:128` define linked/patrol
  trash as the overpull pressure enemy role.
- `design/quick/quick-design-m2-combat-camp-loop.md:213` through
  `design/quick/quick-design-m2-combat-camp-loop.md:218` define the linked trash
  overpull acceptance-candidate shape.
- `design/gdd/combat-core.md:57` defines the Cleric discipline boundary: one
  enemy at a time, named enemies and camps expose absence of a group.
- `design/gdd/combat-core.md:119` defines explicit pulling and social-link
  response without auto-enabling Attack.
- `design/gdd/combat-core.md:164` through `design/gdd/combat-core.md:168`
  define `encounter_role = Trash`, `Named`, and `Camp` metadata.
- `design/gdd/combat-core.md:430` defines `TwoTrash_Overpull_T1`.
- `design/gdd/combat-core.md:812` through `design/gdd/combat-core.md:814`
  define FEEL-03 two-trash overpull danger.
- `data/combat/t1-combat-fixtures.json:514` through
  `data/combat/t1-combat-fixtures.json:526` define the approved two-trash
  overpull encounter fixture.

## Scope

Add a second trash role to the M2 camp: linked placement or a simple patrol that
can turn a bad pull into a two-trash overpull. The story proves that careful
pulling matters and that the already-playable single-trash loop is not a modern
solo-cleave arena.

Planned implementation surface:

- A linked or patrol trash arrangement in `_DevEntry.unity`.
- Minimal anchor, assist, or simple movement behavior needed to create a
  two-trash overpull within the approved FEEL-03 window.
- Story-specific smoke evidence for both a bad pull and the preserved clean
  single-trash loop.
- No broad behavior-tree, NavMesh, or Creature / Enemy AI contract expansion
  unless readiness explicitly reclassifies the story.

## Out Of Scope

- No named blocker, named tuning, camp boss, or future named encounter proof;
  `S2-M2-04` owns that boundary.
- No objective, quest, named friendly NPC, loot, item pickup, vendor, stash,
  Save/Load flow, or visible faction consequence.
- No networking, FishNet, server authority, PvP, accounts, cloud saves, live LLM,
  extra playable classes, broad companion behavior, second district, or deep
  economy.
- No full Creature / Enemy AI, pathfinding contract, faction simulation,
  respawn system, final HUD, audio pass, or opportunistic cleanup of known
  non-M2 findings.

## Acceptance Criteria

| ID | Criterion | Evidence |
| --- | --- | --- |
| `S2-M2-03-01` | `_DevEntry.unity` contains a linked or patrol trash arrangement that can create a two-trash overpull. | `tests/evidence/S2-M2-03/verification.md` |
| `S2-M2-03-02` | Bad-pull smoke records two same-band trash enemies entering hate within the FEEL-03 window. | `tests/evidence/S2-M2-03/verification.md` |
| `S2-M2-03-03` | Overpull outcome is dangerous per Combat Core intent: player loses, flees, or survives below the approved health/mana danger threshold. | `tests/evidence/S2-M2-03/verification.md` |
| `S2-M2-03-04` | Clean single-trash loop from `S2-M2-02` still passes after the linked/patrol trash addition. | `tests/evidence/S2-M2-03/verification.md` |
| `S2-M2-03-05` | Dotnet combat regression, T1 negative-scope scan, `git diff --check`, and `.githooks/pre-commit` pass before closure. | `tests/evidence/S2-M2-03/verification.md` |

## QA Test Cases

- **S2-M2-03-01**: Overpull arrangement exists
  - Given: M2-02 is complete and `_DevEntry.unity` has the single-trash loop.
  - When: scene objects are inspected.
  - Then: a linked or patrol trash arrangement exists and can plausibly create a two-trash bad pull.
  - Edge cases: static linked placement is acceptable if it proves the overpull; broad AI is out of scope.
- **S2-M2-03-02**: FEEL-03 timing
  - Given: the player performs a bad pull.
  - When: the story-specific smoke runs.
  - Then: two same-band trash enemies enter hate within the approved FEEL-03 window.
  - Edge cases: if only one enemy enters hate, the overpull proof fails.
- **S2-M2-03-03**: Dangerous outcome
  - Given: two same-band trash enemies are in hate.
  - When: the fight resolves or the player flees.
  - Then: evidence records loss, flee, or survival below the approved danger threshold.
  - Edge cases: repeatable comfortable two-trash wins block closure as a tuning or implementation defect.
- **S2-M2-03-04**: Clean loop preserved
  - Given: the linked/patrol enemy exists.
  - When: the player performs the M2-02 intended single-trash pull path.
  - Then: the clean single-trash loop still passes.
  - Edge cases: if every pull becomes an overpull, the layout fails the camp-loop teaching goal.
- **S2-M2-03-05**: Regression and hygiene gates
  - Given: implementation is complete.
  - When: the required local gates run.
  - Then: all gates pass or exact blockers are recorded before closure.
  - Edge cases: runtime forbidden-scope hits block closure.

## Test Evidence

Required evidence:

- `tests/evidence/S2-M2-03/verification.md`
- Story-specific Unity Play Mode or batchmode runner output for bad-pull and
  preserved clean-loop smoke.
- `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"`
- T1 negative-scope scan over changed files.
- `git diff --check`
- `.githooks/pre-commit`

## Performance Budget

This story adds one linked or patrol trash pressure path on top of the existing
single-trash loop. Runtime cost must grow linearly with the small authored enemy
set used by M2; no quadratic enemy-pair scans or broad scene-wide polling should
be introduced for assist, patrol, or overpull checks. Verification should record
that the clean single-trash loop still runs after the second-trash addition and
that authored overpull code has no obvious steady-state per-frame allocation
loop.

## Dependencies

- Depends on: `S2-M2-02` Single Trash Pull + Med Loop complete.
- Unlocks: `S2-M2-04` Named Blocker + Camp Boundary.

## Next Gate

Blocked until `S2-M2-02` is closed.
