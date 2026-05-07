# T1.5-COMBAT-02 Verification

## Baseline And Test Gate

Implementation target baseline:

```powershell
git rev-parse HEAD
```

Result: `f00b2d1f051902a1b84003dbbd6c0f44f9816383`.

Post-implementation command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-5-combat-02-stage2.trx" --results-directory "tests\evidence\T1.5-COMBAT-02"
```

Expected result: prior `139` tests plus seven `QA-02` checks pass.

Result: PASS, `148/148` on 2026-05-07. TRX:
`tests/evidence/T1.5-COMBAT-02/t1-5-combat-02-stage2.trx`.

The additional two tests are code-review blocker regressions:
`test_combat_actor_hydration_uses_fixture_endurance_for_all_cleric_bands` and
`test_combat_fixture_package_declares_legacy_tactical_instant_resource_split`.

## QA Case Evidence

| QA Case | Status | Evidence |
| --- | --- | --- |
| `QA-02-01` Bash uses Endurance, not mana | PASS | Bash fixture declares `resource_kind: "physical"` and `cost_endurance`; resolver spends Endurance for physical profiles; integration test `test_qa_02_01_bash_spends_endurance_and_leaves_mana_unchanged`. |
| `QA-02-02` Bash fails on insufficient Endurance | PASS | Resolver validates `CurrentEndurance` before cooldown creation; integration test `test_qa_02_02_bash_rejects_insufficient_endurance_without_spend_or_cooldown`. |
| `QA-02-03` Smite of Authority remains mana-based | PASS | Smite fixture declares `resource_kind: "magical"` and `cost_mana`; integration test `test_qa_02_03_smite_remains_mana_based_and_ignores_endurance` asserts zero-Endurance success, mana failure, cooldown behavior, and Endurance unchanged. |
| `QA-02-04` Defensive Prayer remains mana-based | PASS | Defensive Prayer fixture declares `resource_kind: "magical"` and `cost_mana`; integration test `test_qa_02_04_defensive_prayer_remains_mana_based_and_ignores_endurance` asserts zero-Endurance success, mana failure, cooldown behavior, and Endurance unchanged. |
| `QA-02-05` Fixture validator rejects physical instant with `cost_mana` | PASS | Unit test `test_qa_02_05_validator_rejects_physical_instant_with_mana_cost`. |
| `QA-02-06` Fixture validator rejects magical instant without legal mana cost | PASS | Unit test `test_qa_02_06_validator_rejects_magical_instant_without_mana_cost`. |
| `QA-02-07` No `combat_actor_id` leak into Endurance events | PASS | Unit test `test_qa_02_07_resource_split_adds_no_durable_combat_actor_id_surface`; no Endurance-specific event DTO is introduced. |

## Frozen Contract Checks

Command:

```powershell
git diff --exit-code f00b2d1 -- src/gameplay/combat/events/CombatDeathEvents.cs src/gameplay/combat/CombatProgressionBaselineSnapshot.cs DECISIONS.md docs/architecture/adr-0006-endurance-resource-model.md
```

Expected: PASS, empty diff. `PlayerKillCreditEvent`, `PlayerDeathEvent`,
`CombatActorDeathEvent`, `CombatProgressionBaselineSnapshot`, D013, and ADR-0006
remain unchanged during implementation.

Result: PASS, empty diff.

## T1 Scope Negative Pass

Command:

```powershell
rg -n -i "FishNet|\bnetworking\b|server authority|server-authority|\bPvP\b|duel|friendly fire|live LLM|OpenAI|Anthropic|companion|Sister Elara|\bWarrior\b|\bEnchanter\b|Time\.deltaTime|DateTime\.Now|DateTime\.UtcNow" assets\data\combat\t1-combat-fixtures.json src\gameplay\combat\abilities\CombatAbilityProfiles.cs src\gameplay\combat\abilities\CombatInstantAbilityResolver.cs src\gameplay\combat\fixtures\CombatFixtureModels.cs src\gameplay\combat\fixtures\CombatFixtureValidator.cs src\gameplay\combat\CombatActorHydrator.cs tests\unit\gameplay\combat\combat_tactical_ability_profile_test.cs tests\unit\gameplay\combat\combat_fixture_validation_test.cs tests\integration\gameplay\combat\combat_tactical_cleric_instants_test.cs tests\integration\gameplay\combat\combat_actor_hydration_test.cs
```

Expected: PASS, zero matches.

Result: PASS, zero matches.

## Code Review Blocker Fixes

Review finding 1: legacy `tacticalInstantFixtures` Bash retained
`manaCostByBand`.

Result: PASS. The legacy Bash row now declares `resource_kind: "physical"` and
`enduranceCostByBand`, with no `manaCostByBand`. Validator coverage rejects a
physical legacy tactical instant row that reintroduces `manaCostByBand`.

Review finding 2: only `Cleric_Mid_T1` hydrated with fixture Endurance.

Result: PASS. `Cleric_Low_T1`, `Cleric_Mid_T1`, and `Cleric_Top_T1` all
declare `max_endurance: 80`; hydration coverage verifies all three bands
hydrate with current/max Endurance.

Focused re-review result: PASS. Code review and QA review subagents reported
no remaining blocking findings after the two blocker fixes. Residual risk is
limited to the deferrable `AbilityResolvedEvent.ManaSpent`-only payload and
future `T1-COMBAT-11` static guards over both tactical fixture surfaces.

## Final Hygiene

Commands:

```powershell
bash .githooks/pre-commit
git diff --check
git diff --cached --name-only
```

Expected: hook OK, whitespace clean, staging empty until commit approval.

Result: PASS. `bash .githooks/pre-commit` returned `[pre-commit] OK`;
`git diff --check` returned no whitespace errors, with Git CRLF warnings only;
`git diff --cached --name-only` returned empty.
