# T1.5-COMBAT-02 - Physical Instant Conversion

**Status:** Complete
**Sprint:** 1.5
**Priority:** Must Have
**Layer:** Gameplay / Combat Core
**Type:** Logic + Integration
**Manifest Version:** Sprint 1.5, 2026-05-07
**GDD:** `design/gdd/combat-core.md`
**Governing ADR:** `docs/architecture/adr-0006-endurance-resource-model.md`
**Evidence:** `tests/evidence/T1.5-COMBAT-02/verification.md`

## Scope

This story validates ADR-0006's physical-instant resource split. During
implementation, ADR-0006 and D013 remained Proposed until `/story-done`.

- Convert Bash from mana spend to Endurance spend.
- Add fixture schema support for `resource_kind` and `cost_endurance`.
- Keep `SmiteOfAuthority_T1_Prototype` and `DefensivePrayer_T1_Prototype`
  mana-based.
- Wire Cleric mid-tier fixture Endurance into actor hydration.
- Preserve existing cooldown behavior: insufficient resource rejects before
  spend and does not start cooldown.

Source trace: `production/sprints/sprint-1-5.md:139` through
`production/sprints/sprint-1-5.md:178`; QA trace:
`production/qa/plans/qa-plan-sprint-1-5-20260506.md:106` through
`production/qa/plans/qa-plan-sprint-1-5-20260506.md:122`.

## Locked Implementation Decisions

- Endurance fixture field: `cost_endurance`.
- Resource discriminator: `resource_kind` with `physical` and `magical`.
- Insufficient-resource behavior: no resource spend and no cooldown start.
- ADR-0006 remains Proposed during implementation. D013 remains Proposed.
  Their status ride-along remains owned by `T1.5-COMBAT-02` closure.

## Out Of Scope

- Any DECISIONS.md or ADR status flip during implementation.
- Any Combat Core GDD revision.
- Any `src/ui/**`, Unity API, HUD styling, pulse/combo/rotation Endurance work,
  FEEL-01 target work, or FEEL-03 tuning.
- Any `PlayerKillCreditEvent`, `PlayerDeathEvent`, `CombatActorDeathEvent`, or
  `CombatProgressionBaselineSnapshot` schema change.
- Any sprint-status or session-state closure routing update.

## Composition Trace

- `assets/data/combat/t1-combat-fixtures.json` adds `max_endurance` to all
  Cleric player fixture bands, adds `resource_kind: "magical"` to Smite of
  Authority and Defensive Prayer, and converts Bash to `resource_kind:
  "physical"` with Endurance cost data on both legacy banded tactical instant
  rows and executable tactical ability profiles.
- `src/gameplay/combat/fixtures/CombatFixtureModels.cs` adds fixture fields for
  `max_endurance`, `resource_kind`, legacy banded Endurance costs, and
  executable `cost_endurance`.
- `src/gameplay/combat/fixtures/CombatFixtureValidator.cs` validates the split:
  physical tactical rows/profiles require Endurance cost data and reject mana
  costs; magical tactical rows/profiles require mana cost data and reject
  Endurance costs.
- `src/gameplay/combat/abilities/CombatAbilityProfiles.cs` exposes
  `CombatTacticalAbilityResourceKind`, `ResourceKind`, and `CostEndurance`.
- `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs` branches
  validation and spend by profile resource kind.
- `src/gameplay/combat/CombatActorHydrator.cs` uses fixture `MaxEndurance` when
  caller input does not override it.

## Frozen Contracts

- `PlayerKillCreditEvent` remains unchanged.
- `PlayerDeathEvent` remains unchanged.
- `CombatActorDeathEvent` remains unchanged.
- `CombatProgressionBaselineSnapshot` remains Endurance-free.
- D013 is Locked; ADR-0006 is Accepted through the `/story-done` ride-along.

## Acceptance Criteria Coverage

| QA Case | Status | Implementation Evidence | Test Evidence |
| --- | --- | --- | --- |
| `QA-02-01` Bash uses Endurance, not mana | Covered | Bash fixture uses `resource_kind: "physical"` and `cost_endurance`; resolver spends Endurance for physical profiles. | `tests/integration/gameplay/combat/combat_tactical_cleric_instants_test.cs` test `test_qa_02_01_bash_spends_endurance_and_leaves_mana_unchanged`. |
| `QA-02-02` Bash fails on insufficient Endurance | Covered | Resolver validates physical profiles against `CurrentEndurance` before cooldown creation. | `tests/integration/gameplay/combat/combat_tactical_cleric_instants_test.cs` test `test_qa_02_02_bash_rejects_insufficient_endurance_without_spend_or_cooldown`. |
| `QA-02-03` Smite of Authority remains mana-based | Covered | Smite fixture uses `resource_kind: "magical"` and `cost_mana`; resolver spends mana for magical profiles. | `tests/integration/gameplay/combat/combat_tactical_cleric_instants_test.cs` test `test_qa_02_03_smite_remains_mana_based_and_ignores_endurance`. |
| `QA-02-04` Defensive Prayer remains mana-based | Covered | Defensive Prayer fixture uses `resource_kind: "magical"` and `cost_mana`; resolver spends mana for magical profiles. | `tests/integration/gameplay/combat/combat_tactical_cleric_instants_test.cs` test `test_qa_02_04_defensive_prayer_remains_mana_based_and_ignores_endurance`. |
| `QA-02-05` Fixture validator rejects physical instant with `cost_mana` | Covered | Validator branches physical profile checks by `ResourceKind`. | `tests/unit/gameplay/combat/combat_fixture_validation_test.cs` test `test_qa_02_05_validator_rejects_physical_instant_with_mana_cost`. |
| `QA-02-06` Fixture validator rejects magical instant without legal mana cost | Covered | Validator branches magical profile checks by `ResourceKind`. | `tests/unit/gameplay/combat/combat_fixture_validation_test.cs` test `test_qa_02_06_validator_rejects_magical_instant_without_mana_cost`. |
| `QA-02-07` No `combat_actor_id` leak into Endurance events | Covered | No Endurance-specific event DTO is introduced; resource split lives on fixture/profile/resolver state only. | `tests/unit/gameplay/combat/combat_fixture_validation_test.cs` test `test_qa_02_07_resource_split_adds_no_durable_combat_actor_id_surface`. |

## Runnable Evidence

- `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-5-combat-02-stage2.trx" --results-directory "tests\evidence\T1.5-COMBAT-02"` passed with `148/148`.
- TRX counters are at
  `tests/evidence/T1.5-COMBAT-02/t1-5-combat-02-stage2.trx`.
- Prior `139` passed tests are still present and passing; the seven
  `QA-02` checks are added in this story, plus two code-review blocker
  regression tests for legacy tactical instant fixture data and all-band
  Cleric Endurance hydration.

## Story Status

`T1.5-COMBAT-02` is complete.

## Blockers / Carried Forward

- ADR-0006 Proposed -> Accepted and D013 Proposed -> Locked completed in this
  `/story-done` closure.
- `AbilityResolvedEvent` remains `ManaSpent`-only; Bash proves Endurance spend
  through state/result behavior while resolved-event payload semantics stay a
  `T1-COMBAT-11` scan input.
- `fixtureSetVersion` bump is deferred to `T1.5-COMBAT-03`, the next
  fixture-touching story.
- QA plan wording that says "cooldown/global recovery" for `QA-02-01` is
  deferred to the next QA plan iteration; implementation currently exposes
  resolver-local cooldown.
- `T1.5-COMBAT-03` owns FEEL-03 overpull tuning.
- `T1-COMBAT-11` owns the forbidden-pattern scan including ADR-0006 Endurance
  banned patterns.

## Completion Notes

**Completed:** 2026-05-07
**Verdict:** COMPLETE WITH NOTES
**Criteria:** 7/7 QA-02 cases covered, plus 2 blocker regression tests added
for legacy tactical instant fixture data and all-band Cleric Endurance
hydration.
**Test Evidence:** `tests/evidence/T1.5-COMBAT-02/verification.md:32` through
`tests/evidence/T1.5-COMBAT-02/verification.md:38` record all QA-02 PASS rows;
`tests/evidence/T1.5-COMBAT-02/t1-5-combat-02-stage2.trx:906` records
`148/148` passing. Direct test anchors: `QA-02-01` through `QA-02-04` at
`tests/integration/gameplay/combat/combat_tactical_cleric_instants_test.cs:40`,
`:59`, `:78`, and `:110`; `QA-02-05` through `QA-02-07` at
`tests/unit/gameplay/combat/combat_fixture_validation_test.cs:249`, `:289`,
and `:310`; legacy fixture regression at
`tests/unit/gameplay/combat/combat_fixture_validation_test.cs:135`; all-band
hydration regression at
`tests/integration/gameplay/combat/combat_actor_hydration_test.cs:52`.
**Code Review:** Complete. `/code-review 9aacee0` approved with P3 suggestions
only; both P3s are intentionally deferred outside this closure batch.
**ADR Ride-Along:** ADR-0006 moved from Proposed to Accepted, and D013 moved
from Proposed to Locked. The ADR pre-authorizes this closure transition at
`docs/architecture/adr-0006-endurance-resource-model.md:123` through
`docs/architecture/adr-0006-endurance-resource-model.md:127`, contingent on
`T1.5-COMBAT-01` and `T1.5-COMBAT-02` holding.
**Deviations:** Non-blocking carryovers remain: `AbilityResolvedEvent.ManaSpent`
payload semantics for `T1-COMBAT-11`, fixture package version bump for
`T1.5-COMBAT-03`, and QA plan cooldown/global-recovery wording for the next QA
plan iteration.
