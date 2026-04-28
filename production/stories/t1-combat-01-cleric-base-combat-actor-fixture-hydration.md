# T1-COMBAT-01 - Cleric Base Combat Actor + Fixture Hydration

**Status:** Implemented / in-progress, awaiting `/story-done`
**Sprint:** 1
**Priority:** Must Have
**Layer:** Gameplay / Combat Core
**Type:** Logic + Config/Data + Integration
**Estimate:** 2.0 days
**Manifest Version:** Sprint 1, 2026-04-28
**GDD:** `design/gdd/combat-core.md`
**Governing ADR:** `docs/architecture/adr-0003-progression-baseline-snapshot-contract.md`
**Evidence:** `tests/evidence/T1-COMBAT-01/verification.md`

## Scope

This story implements the first production Combat Core domain slice for offline T1 Cleric combat actor construction:

- Create the production combat domain scaffold.
- Define fixed combat clock abstractions.
- Define the `CombatActorState` data shape.
- Implement fixture loading for Cleric, trash, named, spell, tactical instant, and encounter fixtures.
- Accept `CombatProgressionBaselineSnapshot` for player actor build/hydration.
- Keep all tunable values in data/config, not hardcoded production combat logic.

Source trace: `production/sprints/sprint-1.md:72-80`.

## Out Of Scope

- Targeting, pulling, social assist, leash, Attack toggle, melee formula resolution, casting, tactical instant execution, med/sit regen, HUD hookup, kill-credit emission, save-barrier integration, player death payloads, profiled combat-feel harness, and forbidden-pattern analyzer execution.
- `T1-COMBAT-02` or any later Sprint 1 story.
- `/story-done` closure, sprint-status updates, or session-state edits.
- FishNet, networking, server authority, account identity, PvP, live LLM, companions, Warrior, Enchanter, or any T2+ combat surface.

Scope guard evidence: `design/gdd/combat-core.md:604-605`; `design/gdd/combat-core.md:903-912`; `assets/data/combat/README.md:13-18`.

## Dependencies

- `/qa-plan sprint` complete: `production/qa/plans/qa-plan-sprint-20260428.md:1-8`.
- `/test-setup` complete enough for the narrow pure C# bridge: `tests/README.md:1-18`; `tests/README.md:91-99`.
- Sprint status still leaves this story to `/story-done`: `production/sprint-status.yaml:1-2`; `production/sprint-status.yaml:16-24`.
- ADR-0003 contract shape: `docs/architecture/adr-0003-progression-baseline-snapshot-contract.md:111-135`.

## Acceptance Criteria Coverage

| AC | Status | Production / Data Evidence | Test / Verification Evidence |
| --- | --- | --- | --- |
| `H-CCOM-SCOPE-01` | Covered for T1-COMBAT-01 surface | T1 fixture rules ban FishNet/networking, server authority, PvP, live LLM, companions, Warrior, and Enchanter rows at `assets/data/combat/README.md:13-18`; production bridge compiles only Combat Core domain code at `tests/Gravenspire.Combat.Tests.csproj:16-25`. | Scope is included in verification trace at `tests/evidence/T1-COMBAT-01/verification.md:26`; Stage 3 rerun passed 15/15 at `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:107-108`. |
| `H-CCOM-ACTOR-01` | Covered | `CombatActorState` defines runtime id, stable source ref, zone, level, resources, combat stats, ranges, state, target id, sort key, and threat table at `src/gameplay/combat/CombatActorState.cs:272-326` and exposes those fields at `src/gameplay/combat/CombatActorState.cs:328-441`; validation is at `src/gameplay/combat/CombatActorState.cs:451-523`. | Unit tests cover required fields, no Unity scene object fields, and threat-table validation at `tests/unit/gameplay/combat/combat_actor_state_test.cs:13-59`; Stage 3 TRX rows are `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:12`, `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:17-18`. |
| `H-CCOM-FIXTURE-01` | Covered | Fixture package metadata and tick rate are at `assets/data/combat/t1-combat-fixtures.json:1-6`; actor rows at `assets/data/combat/t1-combat-fixtures.json:7-156`; spell and tactical instant rows at `assets/data/combat/t1-combat-fixtures.json:157-341`; encounter rows at `assets/data/combat/t1-combat-fixtures.json:342-389`; validator required rows at `src/gameplay/combat/fixtures/CombatFixtureValidator.cs:34-98`. | Fixture validation and loading tests are at `tests/unit/gameplay/combat/combat_fixture_validation_test.cs:13-52` and `tests/integration/gameplay/combat/combat_fixture_loading_test.cs:13-26`; Stage 3 TRX rows are `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:8`, `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:20`. |
| `H-CCOM-F2B` | Partially covered by fixture rows; formula execution deferred to `T1-COMBAT-04` | Low/mid/top Cleric fixture rows are `assets/data/combat/t1-combat-fixtures.json:8-70`; low/mid/top trash fixture rows are `assets/data/combat/t1-combat-fixtures.json:71-134`. | `Cleric_Mid_T1` = level 5, 140 HP, 180 mana is asserted at `tests/unit/gameplay/combat/combat_fixture_validation_test.cs:23-36`; Stage 3 TRX row is `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:21`. |
| `H-CCOM-SL-02` | Covered for Combat hydration validation path | Hydrator requires baseline, fixture, and input before actor creation at `src/gameplay/combat/CombatActorHydrator.cs:54-91`; current-resource fail-loud validation is at `src/gameplay/combat/CombatActorHydrator.cs:191-215`. | Hydration tests cover valid `Cleric_Mid_T1`, missing baseline, and dead current health without death handoff at `tests/integration/gameplay/combat/combat_actor_hydration_test.cs:14-86`; Stage 3 TRX rows are `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:9`, `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:11`, `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:13`. |
| ADR-0003 `CombatProgressionBaselineSnapshot` | Covered for T1 Combat actor hydration | ADR-0003 defines `CombatProgressionBaselineSnapshot` as the only T1 baseline Combat may consume at `docs/architecture/adr-0003-progression-baseline-snapshot-contract.md:111-135`; implementation shape and validation are at `src/gameplay/combat/CombatProgressionBaselineSnapshot.cs:34-91`; hydrator consumes the baseline at `src/gameplay/combat/CombatActorHydrator.cs:61-68` and builds from its legal fields at `src/gameplay/combat/CombatActorHydrator.cs:104-126`. | Hydration success proves level/max-resource handoff at `tests/integration/gameplay/combat/combat_actor_hydration_test.cs:17-44`; Stage 3 TRX row is `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:9`. |

## Runnable Evidence

Stage 2 command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-01-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-01"
```

Result: PASS, 15 total, 15 passed, 0 failed. Evidence: `tests/evidence/T1-COMBAT-01/t1-combat-01-stage2.trx:107-108`; verification command and counters at `tests/evidence/T1-COMBAT-01/verification.md:37` and `tests/evidence/T1-COMBAT-01/verification.md:43`.

Stage 3 rerun command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-01-stage3-rerun.trx" --results-directory "tests\evidence\T1-COMBAT-01"
```

Result: PASS, 15 total, 15 passed, 0 failed. Evidence: `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx:107-108`; verification command and counters at `tests/evidence/T1-COMBAT-01/verification.md:38` and `tests/evidence/T1-COMBAT-01/verification.md:45`.

## Story Status

`T1-COMBAT-01` is implemented and remains in progress until `/story-done` performs closure. This file intentionally does not mark the story Done, does not update `production/sprint-status.yaml`, and does not advance to `T1-COMBAT-02`.

## Blockers / Carried Forward

- `/story-done` must close `T1-COMBAT-01` and update `production/sprint-status.yaml`; Stage 3 intentionally left that file untouched because its header says it is updated by `/story-done` (`production/sprint-status.yaml:1-2`).
- `production/session-state/active.md` still points at older sprint-planning work and was intentionally left untouched per Stage 3 instructions.
- Formula execution for `H-CCOM-F2B` belongs to `T1-COMBAT-04`; this story supplies and validates fixture extremes only.
