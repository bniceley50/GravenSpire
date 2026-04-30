# T1-COMBAT-06 Verification

## Test Command

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-06-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-06"
```

## Result

- PASS: 81 total, 81 passed, 0 failed.
- TRX evidence: `tests/evidence/T1-COMBAT-06/t1-combat-06-stage2.trx:504`.
- Counters line: `<Counters total="81" executed="81" passed="81" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />`.
- Console result: `Passed! - Failed: 0, Passed: 81, Skipped: 0, Total: 81`.

## Hardcoded-Tuning Gate

Command:

```powershell
rg -n "\b\d+(?:\.\d+)?\b" src/gameplay/combat/abilities -g "*.cs"
```

Result: PASS. Every numeric literal hit in `src/gameplay/combat/abilities/**` is a zero/count/identity guard; no ability damage, mana cost, cooldown, duration, or scaling literal appears in production ability logic.

Hits reviewed:

- `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:64` count guard.
- `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:194`, `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:199`, `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:204`, `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:205`, `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:206`, and `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:237` zero validation guards.
- `src/gameplay/combat/abilities/CombatAbilityProfiles.cs:53` declared-effect count guard.

## Negative T1 Scope Grep

Command:

```powershell
rg -n -S "FishNet|networking|server authority|PvP|companion|Warrior|Enchanter|OpenAI|Anthropic|Time\.deltaTime|DateTime\.Now|DateTime\.UtcNow" src/gameplay/combat/abilities/CombatAbilityProfiles.cs src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs src/gameplay/combat/events/CombatAbilityLifecycleEvents.cs src/gameplay/combat/CombatActorStateTransitions.cs src/gameplay/combat/fixtures/CombatFixtureModels.cs src/gameplay/combat/fixtures/CombatFixtureValidator.cs tests/unit/gameplay/combat/combat_fixture_validation_test.cs tests/unit/gameplay/combat/combat_tactical_ability_profile_test.cs tests/integration/gameplay/combat/combat_tactical_cleric_instants_test.cs
```

Result: PASS. The command returned zero matches across the nine changed `.cs` files.

## Existing-File Additive Summary

`src/gameplay/combat/CombatActorStateTransitions.cs`:

- Added `WithCurrentHealthAfterAbilityDamage` and `CancelActiveChannelByAbility` after the T1-COMBAT-05 cast helpers at `src/gameplay/combat/CombatActorStateTransitions.cs:234` and `src/gameplay/combat/CombatActorStateTransitions.cs:283`.
- Unified diff summary: one hunk, `+65` lines, no removed lines.
- Existing Attack, melee, pull, threat, target, zone, and cast methods were not renamed, reordered, or removed.

`src/gameplay/combat/fixtures/CombatFixtureModels.cs`:

- Added `TacticalInstantAbilityProfiles` to the root package at `src/gameplay/combat/fixtures/CombatFixtureModels.cs:76`.
- Added `CombatTacticalInstantAbilityProfileFixture` and `CombatTacticalInstantAbilityEffectFixture` at `src/gameplay/combat/fixtures/CombatFixtureModels.cs:409` and `src/gameplay/combat/fixtures/CombatFixtureModels.cs:460`.
- Unified diff summary: two hunks, `+92` lines, no removed lines.
- Existing actor, spell, tactical instant, and encounter fixture records were not renamed, retyped, reordered, or removed.

`src/gameplay/combat/fixtures/CombatFixtureValidator.cs`:

- Added required tactical instant ability profile ids at `src/gameplay/combat/fixtures/CombatFixtureValidator.cs:58`.
- Added profile validation dispatch at `src/gameplay/combat/fixtures/CombatFixtureValidator.cs:101` and required-id validation at `src/gameplay/combat/fixtures/CombatFixtureValidator.cs:107`.
- Added ability profile/effect validators at `src/gameplay/combat/fixtures/CombatFixtureValidator.cs:310` and `src/gameplay/combat/fixtures/CombatFixtureValidator.cs:356`.
- Unified diff summary: four hunks, `+97` lines, no removed lines.
- Existing validation helper paths remain present and unremoved.

`tests/unit/gameplay/combat/combat_fixture_validation_test.cs`:

- Added tactical instant ability profile coverage at `tests/unit/gameplay/combat/combat_fixture_validation_test.cs:94`.
- Added required-field rejection tests at `tests/unit/gameplay/combat/combat_fixture_validation_test.cs:122` and effect-specific rejection tests at `tests/unit/gameplay/combat/combat_fixture_validation_test.cs:146`.
- Unified diff summary: one hunk, `+117` lines, no removed lines.
- Existing H-CCOM-FIXTURE-01 and pull/social-assist tests were not modified.

## JSON Diff Summary

`assets/data/combat/t1-combat-fixtures.json`:

- Appended `tacticalInstantAbilityProfiles` after the existing `tacticalInstantFixtures` block at `assets/data/combat/t1-combat-fixtures.json:386`.
- Added executable profiles for `SmiteOfAuthority_T1_Prototype`, `Bash_T1_Prototype`, and `DefensivePrayer_T1_Prototype` at `assets/data/combat/t1-combat-fixtures.json:388`, `assets/data/combat/t1-combat-fixtures.json:416`, and `assets/data/combat/t1-combat-fixtures.json:448`.
- Unified diff summary: one hunk, `+78` lines, no removed lines.
- Existing pull, social-assist, LoS, leash, actor, spell, tactical instant, and encounter entries were not renamed, retyped, reordered, or removed.
- `fixtureSetVersion` remains `CombatPrototypeSpellProfileSet_T1@2026-04-28-stage1`; the executable ability-profile list is additive-only and does not alter existing fixture rows.

## Hook Smoke

Command:

```powershell
bash .githooks/pre-commit
```

Result: PASS. Output: `[pre-commit] OK`.

## Diff Hygiene

Command:

```powershell
git diff --check
```

Result: PASS.
