# T1-COMBAT-07 Verification

## Targeted Test Command

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-07-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-07"
```

Result: PASS, 92 total, 92 passed, 0 failed.

TRX counter: `tests/evidence/T1-COMBAT-07/t1-combat-07-stage2.trx:570`.

## Acceptance Coverage Anchors

- `H-CCOM-MED-01`: Attack-off-before-regen ordering is implemented by `CombatPostureStateMachine.TrySit` calling the existing `CombatAttackStateMachine.ForceOff` path at `src/gameplay/combat/state/CombatPostureStateMachine.cs:49`; integration coverage starts at `tests/integration/gameplay/combat/combat_med_sit_regen_combat_exit_test.cs:16`.
- `H-CCOM-MED-02`: Fixture-owned out-of-combat sitting mana regen tuning is appended at `assets/data/combat/t1-combat-fixtures.json:511`; the formula applies the sitting multiplier only out of combat at `src/gameplay/combat/regen/CombatRegenFormulas.cs:74`; the fixture-loaded 8 mana/tick example starts at `tests/unit/gameplay/combat/combat_regen_formulas_test.cs:15`.
- `H-CCOM-MED-03`: Unsafe in-combat sit updates existing hostile `ThreatTable` entries at `src/gameplay/combat/threat/CombatThreatResolver.cs:42`; no-med-boost integration coverage starts at `tests/integration/gameplay/combat/combat_med_sit_regen_combat_exit_test.cs:66`.
- `H-CCOM-F5`: Combat exit evaluates Combat Simulation Tick elapsed time and zero valid hostile entries at `src/gameplay/combat/state/CombatExitStateMachine.cs:21`; the 30.0s false / 30.1s true / one-hostile false unit test starts at `tests/unit/gameplay/combat/combat_exit_timer_test.cs:14`.
- `H-CCOM-FEEL-04` prerequisite: regen interval conversion uses fixture seconds plus `combat_tick_rate_hz` at `src/gameplay/combat/regen/CombatRegenFormulas.cs:86`; tick interval coverage starts at `tests/unit/gameplay/combat/combat_regen_formulas_test.cs:53`.

## Hardcoded-Tuning Gate

Command:

```powershell
rg -n "[0-9]+(\.[0-9]+)?[dDfFlLmM]?" src\gameplay\combat\regen\CombatRegenFormulas.cs src\gameplay\combat\regen\CombatRegenResolver.cs src\gameplay\combat\state\CombatPostureStateMachine.cs src\gameplay\combat\state\CombatExitStateMachine.cs src\gameplay\combat\threat\CombatThreatResolver.cs
```

Result: PASS. Numeric literal hits are limited to zero/one guards, identity values, and clamp guards in:

- `src/gameplay/combat/threat/CombatThreatResolver.cs:27`
- `src/gameplay/combat/threat/CombatThreatResolver.cs:33`
- `src/gameplay/combat/threat/CombatThreatResolver.cs:48`
- `src/gameplay/combat/threat/CombatThreatResolver.cs:54`
- `src/gameplay/combat/threat/CombatThreatResolver.cs:60`
- `src/gameplay/combat/state/CombatPostureStateMachine.cs:29`
- `src/gameplay/combat/state/CombatPostureStateMachine.cs:41`
- `src/gameplay/combat/state/CombatExitStateMachine.cs:25`
- `src/gameplay/combat/state/CombatExitStateMachine.cs:30`
- `src/gameplay/combat/state/CombatExitStateMachine.cs:42`
- `src/gameplay/combat/state/CombatExitStateMachine.cs:43`
- `src/gameplay/combat/regen/CombatRegenResolver.cs:26`
- `src/gameplay/combat/regen/CombatRegenResolver.cs:37`
- `src/gameplay/combat/regen/CombatRegenFormulas.cs:61`
- `src/gameplay/combat/regen/CombatRegenFormulas.cs:63`
- `src/gameplay/combat/regen/CombatRegenFormulas.cs:83`
- `src/gameplay/combat/regen/CombatRegenFormulas.cs:91`
- `src/gameplay/combat/regen/CombatRegenFormulas.cs:96`
- `src/gameplay/combat/regen/CombatRegenFormulas.cs:101`

No gameplay tuning value appears in the new production logic. Regen rate, combat-exit duration, sitting threat bonus, and regen tick interval come from `assets/data/combat/t1-combat-fixtures.json:511`.

## Negative T1 Scope Grep

Command:

```powershell
rg -n -i "FishNet|networking|network|server authority|server|PvP|companion|Warrior|Enchanter|OpenAI|Anthropic|DateTime|UtcNow|DateTime\.Now|Time\.deltaTime|deltaTime|System\.Random" src\gameplay\combat\CombatActorState.cs src\gameplay\combat\fixtures\CombatFixtureModels.cs src\gameplay\combat\fixtures\CombatFixtureValidator.cs src\gameplay\combat\regen\CombatRegenFormulas.cs src\gameplay\combat\regen\CombatRegenResolver.cs src\gameplay\combat\state\CombatPostureStateMachine.cs src\gameplay\combat\state\CombatExitStateMachine.cs src\gameplay\combat\threat\CombatThreatResolver.cs tests\unit\gameplay\combat\combat_fixture_validation_test.cs tests\unit\gameplay\combat\combat_regen_formulas_test.cs tests\unit\gameplay\combat\combat_exit_timer_test.cs tests\integration\gameplay\combat\combat_med_sit_regen_combat_exit_test.cs
```

Result: PASS, zero matches.

## Composition Verification

- Attack-off-on-sit reuses the existing Attack state machine API: `CombatAttackStateMachine.ForceOff` at `src/gameplay/combat/state/CombatPostureStateMachine.cs:49`, with `CombatAttackTransitionPath.SuccessfulSitOrMed` at `src/gameplay/combat/state/CombatPostureStateMachine.cs:50`.
- Sitting threat composes with existing `CombatActorState.ThreatTable`, not a parallel threat store: read at `src/gameplay/combat/threat/CombatThreatResolver.cs:32` and `src/gameplay/combat/threat/CombatThreatResolver.cs:59`; update uses existing `AddThreat` at `src/gameplay/combat/threat/CombatThreatResolver.cs:62`.
- Timing uses `CombatTick` and `combat_tick_rate_hz`: exit request fields are `CombatTick` at `src/gameplay/combat/state/CombatExitStateMachine.cs:8` and `src/gameplay/combat/state/CombatExitStateMachine.cs:9`; regen tick request uses `CombatTick` at `src/gameplay/combat/regen/CombatRegenResolver.cs:10`.

## Additive-only Verification

Existing-file diff summary for the five existing-file modifications:

- `assets/data/combat/t1-combat-fixtures.json`: `24 insertions / 1 structural closing-bracket comma change`; unified diff shows only `regenAndCombatExitTuning` appended at the top level after `encounterFixtures` at `assets/data/combat/t1-combat-fixtures.json:511`; no actor, spell, tactical instant, pull, social assist, leash, or encounter row was renamed, retyped, reordered, or removed.
- `src/gameplay/combat/CombatActorState.cs`: `25 insertions / 0 deletions`; additive posture/regen/exit properties start at `src/gameplay/combat/CombatActorState.cs:512`; additive validation starts at `src/gameplay/combat/CombatActorState.cs:614`.
- `src/gameplay/combat/fixtures/CombatFixtureModels.cs`: `5 insertions / 0 deletions`; additive package property at `src/gameplay/combat/fixtures/CombatFixtureModels.cs:86`.
- `src/gameplay/combat/fixtures/CombatFixtureValidator.cs`: `41 insertions / 0 deletions`; additive validation call at `src/gameplay/combat/fixtures/CombatFixtureValidator.cs:103`; additive validation methods start at `src/gameplay/combat/fixtures/CombatFixtureValidator.cs:399`.
- `tests/unit/gameplay/combat/combat_fixture_validation_test.cs`: `41 insertions / 0 deletions`; additive fixture tests start at `tests/unit/gameplay/combat/combat_fixture_validation_test.cs:211` and `tests/unit/gameplay/combat/combat_fixture_validation_test.cs:227`.

## Cast-and-sit Policy

Manual sit requests during an active slow cast are rejected in this T1 story. Combat Core Rule 19 says the player can sit only while "not casting" and "not in recovery" at `design/gdd/combat-core.md:174`; Rule 14 also says sitting is a non-damage interrupt source at `design/gdd/combat-core.md:156`. This implementation applies the stricter manual sit eligibility guard now and leaves forced/external sitting-as-interrupt behavior to a later explicit interrupt-source story if Class Design or encounter content needs it. Coverage starts at `tests/integration/gameplay/combat/combat_med_sit_regen_combat_exit_test.cs:126`.

## Hygiene

- `git diff --check`: PASS. Git emitted CRLF working-copy warnings only; no whitespace/conflict-marker errors.
- `bash .githooks/pre-commit`: PASS, `[pre-commit] OK`.
