# T1.5-COMBAT-01 Verification

## Baseline And Test Gate

Baseline preflight at HEAD `5e593448392e01617a0ca56fbe1db5bedb7d99fe`:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"
```

Result:

```text
Passed!  - Failed:     0, Passed:   133, Skipped:     0, Total:   133
```

Post-implementation command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-5-combat-01-stage2.trx" --results-directory "tests\evidence\T1.5-COMBAT-01"
```

Result:

```text
Passed!  - Failed:     0, Passed:   139, Skipped:     0, Total:   139
```

TRX counter: `tests/evidence/T1.5-COMBAT-01/t1-5-combat-01-stage2.trx:852`.

## QA Case Evidence

| QA Case | Status | Evidence |
| --- | --- | --- |
| `QA-01-01` Endurance actor state validates and clamps | PASS | Actor state fields and validation are at `src/gameplay/combat/CombatActorState.cs:421` through `src/gameplay/combat/CombatActorState.cs:607`; test method `tests/unit/gameplay/combat/combat_endurance_state_test.cs:14`; TRX row `tests/evidence/T1.5-COMBAT-01/t1-5-combat-01-stage2.trx:37`. |
| `QA-01-02` Combat persistence whitelist adds exactly Endurance | PASS | Projection adds `current_endurance` at `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:30`; existing field-count assertion now expects `5` at `tests/unit/gameplay/combat/combat_persistence_projection_test.cs:22`; QA test method `tests/unit/gameplay/combat/combat_endurance_state_test.cs:35`; TRX row `tests/evidence/T1.5-COMBAT-01/t1-5-combat-01-stage2.trx:54`. |
| `QA-01-03` Persistence still excludes transient combat state | PASS | Projection public fields remain declared-only and read-only at `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:24` through `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:32`; test method `tests/unit/gameplay/combat/combat_endurance_state_test.cs:56`; TRX row `tests/evidence/T1.5-COMBAT-01/t1-5-combat-01-stage2.trx:98`. |
| `QA-01-04` HUD projection exposes quiet Endurance | PASS | HUD signal is enum-typed at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:35` and `src/gameplay/combat/presentation/CombatHudStateProjection.cs:63`; test method `tests/unit/gameplay/combat/combat_endurance_state_test.cs:75`; TRX row `tests/evidence/T1.5-COMBAT-01/t1-5-combat-01-stage2.trx:63`. |
| `QA-01-05` No `src/ui/**` dependency introduced | PASS | Static grep for Unity/UI dependencies returned zero matches; test method `tests/unit/gameplay/combat/combat_endurance_state_test.cs:94`; TRX row `tests/evidence/T1.5-COMBAT-01/t1-5-combat-01-stage2.trx:33`. |
| `QA-01-06` ADR-0003 non-constraint preserved | PASS | `CombatProgressionBaselineSnapshot` remains Endurance-free at `src/gameplay/combat/CombatProgressionBaselineSnapshot.cs:37` through `src/gameplay/combat/CombatProgressionBaselineSnapshot.cs:42`; test method `tests/unit/gameplay/combat/combat_endurance_state_test.cs:115`; TRX row `tests/evidence/T1.5-COMBAT-01/t1-5-combat-01-stage2.trx:130`. |

## Hydration Source

`max_endurance` is parameterized for this story through
`CombatActorHydrationInput.MaxEndurance` at
`src/gameplay/combat/CombatActorHydrator.cs:26`. Current Endurance is carried by
`CombatResourceHydrationState.CurrentEndurance` at
`src/gameplay/combat/CombatActorHydrator.cs:16`. Fixture data remains untouched
and is deferred to `T1.5-COMBAT-02`.

ADR-0003 is preserved because `CombatProgressionBaselineSnapshot` has no
Endurance field and still carries only the combat actor level, permanent max
health, permanent max mana, and metadata in
`src/gameplay/combat/CombatProgressionBaselineSnapshot.cs:37` through
`src/gameplay/combat/CombatProgressionBaselineSnapshot.cs:42`.

## Frozen Contract Checks

Command:

```powershell
git diff --exit-code 5e59344 -- src/gameplay/combat/events/CombatDeathEvents.cs
```

Result: PASS, empty diff. `PlayerKillCreditEvent`, `PlayerDeathEvent`, and
`CombatActorDeathEvent` are unchanged.

Command:

```powershell
git diff --exit-code 5e59344 -- assets/
```

Result: PASS, empty diff. No fixture data changed.

Command:

```powershell
git diff --exit-code 5e59344 -- DECISIONS.md docs/architecture/
```

Result: PASS, empty diff. D013 and ADR-0006 remain Proposed.

## UI And Quietness Checks

Command:

```powershell
rg -n "using UnityEngine|MonoBehaviour|VisualElement|UnityEngine\.UI|UnityEngine\.UIElements" src\gameplay\combat\CombatActorState.cs src\gameplay\combat\CombatActorHydrator.cs src\gameplay\combat\CombatActorStateTransitions.cs src\gameplay\combat\melee\CombatMeleeResolution.cs src\gameplay\combat\death\CombatPlayerDeathResolver.cs src\gameplay\combat\persistence\CombatPersistenceProjection.cs src\gameplay\combat\presentation\CombatHudStateProjection.cs
```

Result: PASS, zero matches.

Command:

```powershell
rg -n -i "pulse|combo|rotation|priority" src\gameplay\combat\presentation\CombatHudStateProjection.cs
```

Result: PASS, zero matches.

Categorical type check:

- `CombatHudEnduranceCategory` is declared at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:35`.
- `CombatHudStateSnapshot.Endurance` is enum-typed at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:63`.
- `test_qa_01_04_hud_projection_exposes_categorical_endurance_signal` asserts the field is not numeric at `tests/unit/gameplay/combat/combat_endurance_state_test.cs:75`.

## Hardcoded Tuning Check

Command:

```powershell
git diff -U0 5e59344 -- src/gameplay/combat/CombatActorState.cs src/gameplay/combat/CombatActorHydrator.cs src/gameplay/combat/CombatActorStateTransitions.cs src/gameplay/combat/melee/CombatMeleeResolution.cs src/gameplay/combat/death/CombatPlayerDeathResolver.cs src/gameplay/combat/persistence/CombatPersistenceProjection.cs src/gameplay/combat/presentation/CombatHudStateProjection.cs | rg -n "^\+[^+].*\b[0-9]+(\.[0-9]+)?"
```

Result: PASS, new production numeric literals are only `0` defaults / validation
guards. No non-zero Endurance tuning value was introduced.

## T1 Scope Negative Pass

Command:

```powershell
rg -n -i "FishNet|\bnetworking\b|server authority|server-authority|\bPvP\b|duel|friendly fire|live LLM|OpenAI|Anthropic|companion|Sister Elara|\bWarrior\b|\bEnchanter\b|Time\.deltaTime|DateTime\.Now|DateTime\.UtcNow" src\gameplay\combat\CombatActorState.cs src\gameplay\combat\CombatActorHydrator.cs src\gameplay\combat\CombatActorStateTransitions.cs src\gameplay\combat\melee\CombatMeleeResolution.cs src\gameplay\combat\death\CombatPlayerDeathResolver.cs src\gameplay\combat\persistence\CombatPersistenceProjection.cs src\gameplay\combat\presentation\CombatHudStateProjection.cs tests\unit\gameplay\combat\combat_endurance_state_test.cs tests\unit\gameplay\combat\combat_persistence_projection_test.cs tests\integration\gameplay\combat\combat_hud_state_signal_test.cs
```

Result: PASS, zero matches.

## Existing Test Continuity

Command compared prior `T1-COMBAT-09c` 133-test TRX against the new
`T1.5-COMBAT-01` TRX:

```powershell
$oldPath = 'tests\evidence\T1-COMBAT-09c\t1-combat-09c-stage2.trx'; $newPath = 'tests\evidence\T1.5-COMBAT-01\t1-5-combat-01-stage2.trx'; [xml]$old = Get-Content -LiteralPath $oldPath; [xml]$new = Get-Content -LiteralPath $newPath; $oldPassed = @($old.TestRun.Results.UnitTestResult | Where-Object { $_.outcome -eq 'Passed' } | ForEach-Object { $_.testName } | Sort-Object -Unique); $newPassed = @($new.TestRun.Results.UnitTestResult | Where-Object { $_.outcome -eq 'Passed' } | ForEach-Object { $_.testName } | Sort-Object -Unique); $missing = @($oldPassed | Where-Object { $_ -notin $newPassed }); $newOnly = @($newPassed | Where-Object { $_ -notin $oldPassed }); "old_passed=$($oldPassed.Count) new_passed=$($newPassed.Count) missing_old_passed=$($missing.Count) new_only=$($newOnly.Count)"; if ($missing.Count -gt 0) { 'MISSING:'; $missing }; 'NEW_ONLY:'; $newOnly
```

Result:

```text
old_passed=133 new_passed=139 missing_old_passed=0 new_only=6
NEW_ONLY:
test_qa_01_01_endurance_actor_state_validates_clamps_and_round_trips
test_qa_01_02_persistence_projection_exposes_prior_four_fields_plus_current_endurance
test_qa_01_03_persistence_projection_still_excludes_transient_combat_state
test_qa_01_04_hud_projection_exposes_categorical_endurance_signal
test_qa_01_05_hud_projection_has_no_ui_or_unity_dependency
test_qa_01_06_baseline_snapshot_remains_endurance_free_while_persistence_adds_endurance
```

## Persistence Projection Field Count

Command:

```powershell
rg -n "Has\.Length\.EqualTo\(5\)|current_endurance|exactly_whitelisted_four_properties" tests\unit\gameplay\combat\combat_persistence_projection_test.cs
```

Result: PASS. The historical test method name remains for TRX continuity, and
the assertion now requires `5` read-only fields at
`tests/unit/gameplay/combat/combat_persistence_projection_test.cs:22`.

Diff review confirms the existing four fields keep the same names, getters, and
read sources; only `current_endurance` is added.

## Final Hygiene

Command:

```powershell
bash .githooks/pre-commit
```

Result:

```text
[pre-commit] OK
```

Command:

```powershell
git diff --check
```

Result: PASS, no whitespace or conflict-marker errors. Git emitted line-ending
normalization warnings for touched text files only.

Command:

```powershell
git diff --cached --name-only
```

Result: PASS, empty output. Nothing is staged.

Working tree note: tracked modifications are limited to the approved batch;
untracked standing deferrals remain `.claude/agent-memory/qa-lead/`,
`all-skills-claude-rebased.patch`, and `all-skills-claude.patch`. New approved
story/test/evidence files and the tool-generated TRX are untracked until the
user authorizes staging.
