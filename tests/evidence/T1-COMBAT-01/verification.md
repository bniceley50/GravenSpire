# T1-COMBAT-01 Verification

**Story:** `T1-COMBAT-01` - Cleric base combat actor + fixture hydration
**Stage:** 3 of 3 - story handoff alignment plus rerun verification
**Date:** 2026-04-28
**Status:** Implemented, not closed; awaiting `/story-done`

## Scope

Stage 1 created pure C# Combat Core domain scaffolding, fixture loading/validation, authored T1 combat fixture data, and focused NUnit test intent.

Stage 2 added the minimal runnable domain-test bridge and executed the Stage 1 unit/integration tests.

Stage 3 creates the `/dev-story` handoff story artifact, maps the T1-COMBAT-01 acceptance trace to Stage 1 implementation evidence plus Stage 2/3 runnable test evidence, and reruns the Stage 2 command. This stage does not close the story. `/story-done` owns story closure and sprint-status updates after Stage 3 alignment.

## Harness Decision

Chosen path: **Option A - pure C# `.csproj` domain-test bridge**.

Rationale: the Stage 1 production combat code is pure C# under `src/gameplay/combat/**`, and the Stage 1 tests use NUnit assertions without Unity scene, MonoBehaviour, or Unity API dependencies. A Unity project shell would be required later for MonoBehaviour/runtime PlayMode surfaces, but bootstrapping it here would widen Stage 2 beyond "run the Stage 1 tests." The bridge at `tests/Gravenspire.Combat.Tests.csproj` compiles only the production Combat Core domain files and existing T1-COMBAT-01 unit/integration tests.

## Acceptance Trace

| Requirement | Stage 1 + Stage 2/3 Evidence |
| --- | --- |
| `H-CCOM-SCOPE-01` | T1-only production write set is limited to `src/gameplay/combat/**` and `assets/data/combat/**`; fixture README bans FishNet/networking, server authority, PvP, live LLM, companions, Warrior, and Enchanter rows. Evidence: `assets/data/combat/README.md:13-18`; Stage 3 TRX counters pass 15/15 at `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:107-108`. |
| `H-CCOM-ACTOR-01` | `CombatActorState` defines transient runtime id, stable source ref, zone, level, resources, combat stats, ranges, state, target id, sort key, and transient threat table, with validation rejecting bad runtime/resource shape. Evidence: `src/gameplay/combat/CombatActorState.cs:272-326`, `src/gameplay/combat/CombatActorState.cs:328-441`, `src/gameplay/combat/CombatActorState.cs:451-523`; tests at `tests/unit/gameplay/combat/combat_actor_state_test.cs:13-59`; Stage 3 TRX rows `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:12`, `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:17-18`. |
| `H-CCOM-FIXTURE-01` | Fixture data includes low/mid/top Cleric, trash, named, slow spells, tactical instants, encounters, kill-weight seed values, and source-ref aliases; validator requires those rows and safe ranges. Evidence: `assets/data/combat/t1-combat-fixtures.json:1-6`, `assets/data/combat/t1-combat-fixtures.json:7-156`, `assets/data/combat/t1-combat-fixtures.json:157-341`, `assets/data/combat/t1-combat-fixtures.json:342-389`, `src/gameplay/combat/fixtures/CombatFixtureValidator.cs:34-98`; tests at `tests/unit/gameplay/combat/combat_fixture_validation_test.cs:13-52`, `tests/integration/gameplay/combat/combat_fixture_loading_test.cs:13-26`; Stage 3 TRX rows `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:8`, `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:20`. |
| `H-CCOM-F2B` | Stage 1 fixture rows define low/top Cleric and trash extremes and validate `Cleric_Mid_T1` = level 5, 140 HP, 180 mana. Full seeded melee damage formula execution remains intentionally deferred to `T1-COMBAT-04`. Evidence: `assets/data/combat/t1-combat-fixtures.json:8-70`, `assets/data/combat/t1-combat-fixtures.json:71-134`, `tests/unit/gameplay/combat/combat_fixture_validation_test.cs:23-36`; Stage 3 TRX row `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:21`. |
| `H-CCOM-SL-02` | Hydrator fails loud on missing `CombatProgressionBaselineSnapshot` and invalid current resources before producing a playable actor. Evidence: `src/gameplay/combat/CombatActorHydrator.cs:54-91`, `src/gameplay/combat/CombatActorHydrator.cs:191-215`; tests at `tests/integration/gameplay/combat/combat_actor_hydration_test.cs:46-86`; Stage 3 TRX rows `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:11`, `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:13`. |
| ADR-0003 | `CombatProgressionBaselineSnapshot` is the only T1 Character Progression baseline Combat consumes; implementation carries only combat actor level, max health/mana, class/id, schema/revision, and produced-for metadata, with T1 validation. Evidence: `docs/architecture/adr-0003-progression-baseline-snapshot-contract.md:111-135`, `src/gameplay/combat/CombatProgressionBaselineSnapshot.cs:34-91`, `src/gameplay/combat/CombatActorHydrator.cs:61-68`, `src/gameplay/combat/CombatActorHydrator.cs:104-126`; Stage 3 TRX row `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:9`. |

## Commands

| Command | Result | Notes |
| --- | --- | --- |
| `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-01-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-01"` | PASS | 15 tests passed, 0 failed, 0 skipped. TRX: `tests/evidence/T1-COMBAT-01/t1-combat-01-stage2.trx`. |
| `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-01-stage3-rerun.trx" --results-directory "tests\evidence\T1-COMBAT-01"` | PASS | 15 tests passed, 0 failed, 0 skipped. TRX: `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx`. |
| Unity EditMode command from `tests/README.md` | NOT RUN | The repository still has no Unity `ProjectSettings/` or `Packages/manifest.json`; Stage 2 intentionally validates pure C# domain logic with `dotnet test`. |

## Passing Test Evidence

TRX counters from `tests/evidence/T1-COMBAT-01/t1-combat-01-stage2.trx`: `total=15`, `executed=15`, `passed=15`, `failed=0`, `notExecuted=0`.

TRX counters from `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx`: `total=15`, `executed=15`, `passed=15`, `failed=0`, `notExecuted=0` (`tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:107-108`).

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
{"story":"T1-COMBAT-01","stage":"3","timestamp":"2026-04-28T18:16:28-04:00","runner":"dotnet test","project":"tests/Gravenspire.Combat.Tests.csproj","result":"PASS","tests_total":15,"tests_passed":15,"tests_failed":0,"trx":"tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx","fixture_set_version":"CombatPrototypeSpellProfileSet_T1@2026-04-28-stage1","scope":"pure-csharp-domain-bridge-rerun-plus-story-handoff","story_status":"implemented-in-progress-awaiting-story-done"}
```

## Stage 2 Requirement

Complete. Stage 2 added the minimal pure C# test harness and executed the Stage 1 tests.

## Stage 3 Handoff Requirement

Stage 3 should create or align the `production/stories/T1-COMBAT-01-*.md` story file if still required, then hand closure to `/story-done`. Do not update `production/sprint-status.yaml` in this stage; `/story-done` owns closure.

## Stage 3 Completion

Complete. Stage 3 created `production/stories/t1-combat-01-cleric-base-combat-actor-fixture-hydration.md`, reran the Stage 2 command, and preserved story closure for `/story-done`.

Status remains: implemented / in-progress, awaiting `/story-done`.

Do not advance to `T1-COMBAT-02` from this artifact. Do not update `production/sprint-status.yaml` outside `/story-done`.
