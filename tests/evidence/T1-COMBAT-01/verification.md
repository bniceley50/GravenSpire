# T1-COMBAT-01 Stage 1 Verification

**Story:** `T1-COMBAT-01` - Cleric base combat actor + fixture hydration
**Stage:** 1 of 3 - implementation plus test intent
**Date:** 2026-04-28
**Status:** Implemented, not closed

## Scope

Stage 1 creates pure C# Combat Core domain scaffolding, fixture loading/validation, authored T1 combat fixture data, and focused NUnit test intent.

This stage does not close the story. `/story-done` owns story closure and sprint-status updates after runnable verification exists.

## Acceptance Trace

| Requirement | Stage 1 Evidence |
| --- | --- |
| `H-CCOM-SCOPE-01` | T1-only combat domain and fixture data; no networking, FishNet, server authority, PvP, live LLM, companions, Warrior, or Enchanter implementation. |
| `H-CCOM-ACTOR-01` | `src/gameplay/combat/CombatActorState.cs`; `tests/unit/gameplay/combat/combat_actor_state_test.cs`. |
| `H-CCOM-FIXTURE-01` | `assets/data/combat/t1-combat-fixtures.json`; `src/gameplay/combat/fixtures/CombatFixtureValidator.cs`; `tests/unit/gameplay/combat/combat_fixture_validation_test.cs`. |
| `H-CCOM-F2B` | Low/mid/top Cleric and trash fixture rows exist in `assets/data/combat/t1-combat-fixtures.json`; damage formula execution belongs to later story `T1-COMBAT-04`. |
| `H-CCOM-SL-02` | `src/gameplay/combat/CombatActorHydrator.cs`; `tests/integration/gameplay/combat/combat_actor_hydration_test.cs`. |
| ADR-0003 | `src/gameplay/combat/CombatProgressionBaselineSnapshot.cs`; player hydration consumes only the combat-scoped baseline fields. |

## Commands

| Command | Result | Notes |
| --- | --- | --- |
| Unity EditMode command from `tests/README.md` | NOT RUN | The repository still has no `ProjectSettings/` or `Packages/manifest.json`, so Unity Test Runner cannot execute yet. |
| `dotnet test` | NOT RUN | Stage 2 will decide whether to add a pure C# domain-test bridge or a Unity project shell. No project file exists in Stage 1. |

## Stage 2 Requirement

Stage 2 must add the minimal runnable test harness and then execute these tests. The `.csproj` bridge vs. Unity project shell decision is intentionally deferred to that stage.

## Stage 3 Requirement

Stage 3 should update this evidence file with passing test output, create or align the `production/stories/T1-COMBAT-01-*.md` story file if still required, and then hand closure to `/story-done`.
