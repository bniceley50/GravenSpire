# Test Infrastructure

**Engine**: Unity 6.3 LTS (6000.3.x)
**Primary Language**: C# (.NET 8+)
**Test Framework**: Unity Test Framework (NUnit-based) plus Moq when mocking is needed
**CI Workflow**: None in T1. T1 uses local gates only; add CI only after a tier decision permits it.
**Setup Date**: 2026-04-28

## Scope

This scaffold exists for Sprint 1 T1 Combat Core implementation. It supports the QA plan in `production/qa/plans/qa-plan-sprint-20260428.md` and the Sprint 1 story order in `production/sprints/sprint-1.md`.

T1 tests must stay offline and local. Do not add FishNet, networking placeholders, account identity, server authority, PvP, live LLM calls, companions, Warrior, or Enchanter implementation while building the T1 Combat Core test surface.

## Directory Layout

```text
tests/
  EditMode/       # Unity Test Runner EditMode notes and future assembly definition
  PlayMode/       # Unity Test Runner PlayMode notes and future assembly definition
  unit/           # Isolated logic tests: formulas, state machines, pure systems
  integration/    # Cross-system tests, Unity runtime behavior, save/load round trips
  performance/    # Profiled playtest and deterministic scenario harnesses
  architecture/   # Forbidden-pattern static checks and architecture boundary scans
  fixtures/       # Versioned combat, progression, and save/load test fixtures
  smoke/          # Critical path smoke checklists and setup gates
  evidence/       # Test logs, manual notes, screenshots, and copied summaries
```

Production code and data should follow these conventions unless a later approved story narrows them:

```text
Assets/**                         # Unity project assets, scenes, render settings, and editor-only shell tools
src/gameplay/combat/**           # Combat domain code
src/gameplay/progression/**      # Character Progression code
src/gameplay/npc/**              # NPC lifecycle integration or test doubles
src/core/save/**                 # Save/Load code
data/combat/**                   # Combat actor, spell, ability, and encounter fixtures
data/progression/**              # XP source lookup and progression fixture data
production/qa/combat/**          # Durable combat QA evidence and sprint reports
production/playtests/combat/**   # Human/profiler combat playtest evidence
```

## Running Tests

The Unity editor is installed locally at:

```text
C:\Program Files\Unity\Hub\Editor\6000.3.14f1\Editor\Unity.exe
```

Run EditMode tests for unit-style logic, formulas, schema validation, fixture validation, and architecture scans:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.14f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "$PWD" -runTests -testPlatform EditMode -testResults "tests/evidence/test-results/editmode-results.xml" -logFile "tests/evidence/test-results/editmode.log"
```

Run PlayMode tests for runtime integration, event ordering, save/load barriers, hydration, scene-driven behavior, and smoke harnesses:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.14f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "$PWD" -runTests -testPlatform PlayMode -testResults "tests/evidence/test-results/playmode-results.xml" -logFile "tests/evidence/test-results/playmode.log"
```

Current setup note: `S2-FOUNDATION-01` created the production Unity shell at the repository root with canonical `Assets/`, `ProjectSettings/`, and `Packages/` directories. The shell has no real Unity Test Runner assemblies yet; until an approved story adds them, EditMode/PlayMode commands may launch and exit successfully without writing result XML.

No example test was created because no implemented production gameplay system was detected. The first implementation story should add a real failing-then-passing test for `T1-COMBAT-01`; do not add a fake passing assertion.

## Naming

- Test files: `[system]_[feature]_test.cs`
- Test methods: `test_[scenario]_[expected_result]`
- Evidence docs: `[story-slug]-evidence.md`
- Evidence logs: `tests/evidence/[story-id]/[YYYYMMDD-HHMM]-[suite]-[git-sha].log`
- Profile JSONL: `production/qa/combat/[story-id]/[YYYYMMDD-HHMM]-[scenario]-[git-sha].jsonl`

## Story Type to Required Evidence

| Story Type | Required Evidence | Location |
| --- | --- | --- |
| Logic | Passing EditMode unit test | `tests/unit/[system]/` |
| Integration | Passing PlayMode integration test or documented blocked status | `tests/integration/[system]/` |
| Visual/Feel | Screenshot, video, JSONL profile, or signed manual note | `tests/evidence/`, `production/qa/combat/`, or `production/playtests/combat/` |
| UI | Manual walkthrough or interaction test | `tests/evidence/` or `production/qa/evidence/` |
| Config/Data | Fixture validation test or smoke check | `tests/unit/[system]/`, `tests/fixtures/`, or `tests/smoke/` |
| Architecture Boundary | Static scan or analyzer result | `tests/architecture/` and `production/qa/combat/` |

## Fixture Validation

Combat fixture tests must validate:

- Required actor fixture fields for player and hostile actors.
- `Cleric_Mid_T1` resolves as level 5, 140 HP, and 180 mana.
- Lowest, mid, and top T1 Cleric fixtures exist.
- Lowest, mid, and top T1 trash fixtures exist.
- Top-band named fixture exists.
- Spell fixtures exist for `Smite_T1_Prototype` and `LesserHeal_T1_Prototype`.
- D012 tactical instant fixtures exist for `SmiteOfAuthority_T1_Prototype`, `Bash_T1_Prototype`, and `DefensivePrayer_T1_Prototype` or approved T1 equivalents.
- Encounter fixtures exist for `SoloTrash_EvenCon_T1`, `TwoTrash_Overpull_T1`, and `NamedSoloBlock_T1`.
- T1 kill-weight seed values and source-ref aliases are explicit.
- Tunable numeric values resolve from fixture/config data, not hardcoded production combat logic.

Progression fixture tests must validate:

- `PlayerKillCreditEvent` stays narrow: `defeated_source_ref`, `zoneId`, `faction_id`, and `kill_weight_seed`.
- XP metadata comes from progression-owned lookup/snapshot data, not Combat event expansion.
- `XpAwardResolutionSnapshot` contains the fields required for valid award, duplicate, missing lookup, and stale lifecycle cases.
- T1 shipping lookup rows do not use `NonRepeatableFirstKill`.

Save/load fixture tests must validate:

- `CombatProgressionBaselineSnapshot` contains only Combat-allowed fields.
- Character Progression hydration precedes Combat hydration.
- `ProgressionSaveBarrier` and `NpcSourceLifecycleSaveBarrier` settle together before serialization.
- Any unresolved grouped barrier writes no bytes and emits `SaveFailedEvent(DownstreamSaveBarrierUnresolved)`.
- First save runs required materializers before bytes are written.

## Sprint 1 Minimum Suites

| Suite | Location | First Story |
| --- | --- | --- |
| Actor schema and fixture hydration | `tests/unit/gameplay/combat/`, `tests/integration/gameplay/combat/` | T1-COMBAT-01 |
| Targeting, pull, social assist, leash | `tests/integration/gameplay/combat/` | T1-COMBAT-02 |
| Attack toggle and forced-off table | `tests/integration/gameplay/combat/` | T1-COMBAT-03 |
| Melee formulas and fixed tick | `tests/unit/gameplay/combat/`, `tests/integration/gameplay/combat/` | T1-COMBAT-04 |
| Cast lifecycle and tactical instants | `tests/unit/gameplay/combat/`, `tests/integration/gameplay/combat/` | T1-COMBAT-05/T1-COMBAT-06 |
| Med/sit regen and combat exit | `tests/unit/gameplay/combat/`, `tests/integration/gameplay/combat/` | T1-COMBAT-07 |
| HUD-safe Attack state signal | `tests/integration/gameplay/combat/` | T1-COMBAT-08 |
| Death, kill credit, progression, save barriers | `tests/integration/gameplay/combat/`, `tests/integration/gameplay/progression/`, `tests/integration/core/save/` | T1-COMBAT-09a/b/c |
| Profiled combat-feel evidence | `tests/performance/gameplay/combat/`, `production/qa/combat/` | T1-COMBAT-10 |
| Forbidden-pattern compliance scan | `tests/architecture/` | T1-COMBAT-11 |

## Local Gate

Before a story can be marked done:

1. Run the relevant EditMode and/or PlayMode command.
2. Save terminal summaries or Unity logs under `tests/evidence/[story-id]/`.
3. Cite changed production files and changed test files with file:line evidence.
4. Include engine version, git SHA, fixture-set version, and acceptance criteria ids.
5. Include a negative-scope scan when the story touches Combat, Save/Load, Progression, NPC lifecycle, HUD, audio, or architecture boundaries.
