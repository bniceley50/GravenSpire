# T1-COMBAT-01 Verification

**Story:** `T1-COMBAT-01` - Cleric base combat actor + fixture hydration
**Stage:** 2 of 3 - runnable pure C# test harness plus passing verification
**Date:** 2026-04-28
**Status:** Implemented, not closed

## Scope

Stage 1 created pure C# Combat Core domain scaffolding, fixture loading/validation, authored T1 combat fixture data, and focused NUnit test intent.

Stage 2 adds the minimal runnable domain-test bridge and executes the Stage 1 unit/integration tests. This stage does not close the story. `/story-done` owns story closure and sprint-status updates after Stage 3 alignment.

## Harness Decision

Chosen path: **Option A - pure C# `.csproj` domain-test bridge**.

Rationale: the Stage 1 production combat code is pure C# under `src/gameplay/combat/**`, and the Stage 1 tests use NUnit assertions without Unity scene, MonoBehaviour, or Unity API dependencies. A Unity project shell would be required later for MonoBehaviour/runtime PlayMode surfaces, but bootstrapping it here would widen Stage 2 beyond "run the Stage 1 tests." The bridge at `tests/Gravenspire.Combat.Tests.csproj` compiles only the production Combat Core domain files and existing T1-COMBAT-01 unit/integration tests.

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
| `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-01-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-01"` | PASS | 15 tests passed, 0 failed, 0 skipped. TRX: `tests/evidence/T1-COMBAT-01/t1-combat-01-stage2.trx`. |
| Unity EditMode command from `tests/README.md` | NOT RUN | The repository still has no Unity `ProjectSettings/` or `Packages/manifest.json`; Stage 2 intentionally validates pure C# domain logic with `dotnet test`. |

## Passing Test Evidence

TRX counters from `tests/evidence/T1-COMBAT-01/t1-combat-01-stage2.trx`: `total=15`, `executed=15`, `passed=15`, `failed=0`, `notExecuted=0`.

Passed tests:

- `test_combat_actor_hydration_accepts_valid_cleric_mid_baseline`
- `test_combat_actor_hydration_rejects_dead_current_health_without_death_handoff`
- `test_combat_actor_hydration_rejects_missing_baseline_snapshot`
- `test_combat_actor_state_contains_required_runtime_and_stable_fields`
- `test_combat_actor_state_has_no_unity_scene_object_fields`
- `test_combat_actor_state_rejects_transient_threat_table_negative_values`
- `test_combat_clock_advance_ticks_uses_fixed_duration`
- `test_combat_clock_rejects_invalid_tick_rate`
- `test_combat_clock_reset_returns_known_tick`
- `test_combat_fixture_loader_exposes_source_ref_aliases_for_downstream_validation`
- `test_combat_fixture_loader_keeps_fixture_tuning_values_out_of_production_logic`
- `test_combat_fixture_loader_resolves_spells_tactical_instants_and_encounters`
- `test_combat_fixture_package_resolves_cleric_mid_t1_design_values`
- `test_combat_fixture_package_validates_required_t1_rows`
- `test_combat_fixture_validator_rejects_missing_required_rows`

```jsonl
{"story":"T1-COMBAT-01","stage":"2","timestamp":"2026-04-28T18:07:07-04:00","runner":"dotnet test","project":"tests/Gravenspire.Combat.Tests.csproj","result":"PASS","tests_total":15,"tests_passed":15,"tests_failed":0,"trx":"tests/evidence/T1-COMBAT-01/t1-combat-01-stage2.trx","fixture_set_version":"CombatPrototypeSpellProfileSet_T1@2026-04-28-stage1","scope":"pure-csharp-domain-bridge","story_status":"in-progress"}
```

## Stage 2 Requirement

Complete. Stage 2 added the minimal pure C# test harness and executed the Stage 1 tests.

## Stage 3 Requirement

Stage 3 should create or align the `production/stories/T1-COMBAT-01-*.md` story file if still required, then hand closure to `/story-done`. Do not update `production/sprint-status.yaml` in this stage; `/story-done` owns closure.
