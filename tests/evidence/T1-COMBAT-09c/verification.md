# T1-COMBAT-09c Verification

## Targeted Test Command

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-09c-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-09c"
```

Result: PASS, 133 total, 133 passed, 0 failed.

TRX counter: `tests/evidence/T1-COMBAT-09c/t1-combat-09c-stage2.trx:816`.

## Acceptance Evidence

| Check | Evidence |
| --- | --- |
| Lethal transition clamps health to zero | `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:49`; `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:101`; `tests/integration/gameplay/combat/combat_player_death_resolution_test.cs:13` |
| `PlayerDeathEvent` emits exactly once | `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:63`; `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:68`; `tests/integration/gameplay/combat/combat_player_death_resolution_test.cs:13` |
| Deterministic `death_context_id` | Derived from `local_character_id`, `zoneId`, canonical `death_position`, canonical `killer_source_ref`, and `death_cause_type` at `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:82`; covered by `tests/integration/gameplay/combat/combat_player_death_resolution_test.cs:62` |
| Six-field player death payload | `src/gameplay/combat/events/CombatDeathEvents.cs:25`; reflection guard at `tests/unit/gameplay/combat/combat_player_death_payload_schema_test.cs:14` |
| Stable local identity, not transient runtime identity | Resolver reads `StableSourceRef.LocalCharacterId` at `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:141`; test starts at `tests/integration/gameplay/combat/combat_player_death_resolution_test.cs:89` |
| Four-field combat persistence projection | `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:24`; reflection guard at `tests/unit/gameplay/combat/combat_persistence_projection_test.cs:14` |
| No load-side projection surface | Private constructor plus `FromPlayer` only at `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:12` and `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:32`; test starts at `tests/unit/gameplay/combat/combat_persistence_projection_test.cs:61` |

## Frozen Event Diff

Command:

```powershell
git diff b2fe66f -- src\gameplay\combat\events\CombatDeathEvents.cs
```

Result: PASS. The diff is additive only. `PlayerKillCreditEvent` remains unchanged at `src/gameplay/combat/events/CombatDeathEvents.cs:16`; the only new block is `PlayerDeathEvent` at `src/gameplay/combat/events/CombatDeathEvents.cs:25`.

Key diff:

```diff
+/// <summary>
+/// Narrow local-player death signal for a future downstream handoff.
+/// </summary>
+public sealed record PlayerDeathEvent(
+    string death_context_id,
+    string local_character_id,
+    string zoneId,
+    CombatPoint3 death_position,
+    CombatStableSourceRef killer_source_ref,
+    string death_cause_type);
```

## Payload Boundary Scans

PlayerDeathEvent context scan:

```powershell
$lines = Get-Content -LiteralPath "src\gameplay\combat\events\CombatDeathEvents.cs"; $block = $lines[21..30] -join "`n"; $pattern = "combat_actor_id|account_id|pvp|server_authority|raw_threat|corpse_record|xp_penalty|xp_loss|item_drop|llm|tick_id"; if ($block -match $pattern) { $block | Select-String -Pattern $pattern -AllMatches }
```

Result: PASS, zero matches.

Resolver scan:

```powershell
rg -n -i "combat_actor_id|account_id|pvp|server_authority|raw_threat|corpse_record|xp_penalty|xp_loss|item_drop|llm|tick_id" src\gameplay\combat\death\CombatPlayerDeathResolver.cs
```

Result: PASS, zero matches.

## Persistence Boundary Scan

Reflection guard `test_combat_persistence_projection_exposes_exactly_whitelisted_four_properties` starts at `tests/unit/gameplay/combat/combat_persistence_projection_test.cs:14`.

The approved exposed fields are:

- `current_health`
- `current_mana`
- `combat_life_state`
- `pending_death_handoff_payload`

The test also rejects transient fields such as runtime id, threat table, target id, cast runtime, posture, regen tick state, and active cast id.

## Death-and-Recovery Scope Scan

Command:

```powershell
rg -n -i "corpse|respawn|resurrection|xp_loss|corpse_run|corpse_recovery" src\gameplay\combat\death\CombatPlayerDeathResolver.cs src\gameplay\combat\persistence\CombatPersistenceProjection.cs src\gameplay\combat\events\CombatDeathEvents.cs
```

Result: PASS, zero matches.

## No Save Coordinator Modification

Command:

```powershell
git diff --exit-code 617a431 -- src\core\save\SaveStabilityBarrierProtocol.cs src\core\save\GroupedSaveAttemptCoordinator.cs
```

Result: PASS, zero diff. `T1-COMBAT-09c` did not modify the 09b save coordinator or barrier protocol.

## Hardcoded Tuning Scan

Command:

```powershell
rg -n "BaseHitChance|LevelHitDelta|SkillHitDelta|HitChance|AttackPowerScalar|ArmorMitigationScalar|DamageReduction|Cooldown|Duration|ManaCost|Regen|Rate|Multiplier|Scalar" src\gameplay\combat\death\CombatPlayerDeathResolver.cs src\gameplay\combat\persistence\CombatPersistenceProjection.cs
```

Result: PASS, zero matches. The fixed event/projection field shapes are contract, not tuning.

## Negative T1 Scope Scan

Command:

```powershell
rg -n -i "FishNet|networking|network|server authority|server|PvP|companion|Warrior|Enchanter|OpenAI|Anthropic|DateTime|UtcNow|DateTime\.Now|Time\.deltaTime|deltaTime|System\.Random" src\gameplay\combat\events\CombatDeathEvents.cs src\gameplay\combat\death\CombatPlayerDeathResolver.cs src\gameplay\combat\persistence\CombatPersistenceProjection.cs tests\unit\gameplay\combat\combat_player_death_payload_schema_test.cs tests\unit\gameplay\combat\combat_persistence_projection_test.cs tests\integration\gameplay\combat\combat_player_death_resolution_test.cs
```

Result: PASS, zero matches.

## Existing 124-Test Regression Check

Command:

```powershell
$old = [xml](Get-Content -Raw -LiteralPath "tests\evidence\T1-COMBAT-09b\t1-combat-09b-stage2.trx"); $new = [xml](Get-Content -Raw -LiteralPath "tests\evidence\T1-COMBAT-09c\t1-combat-09c-stage2.trx"); ...
```

Result:

```text
old_total=124 old_passed=124 new_total=133 new_passed=133 missing_old_passed=0
```

The prior 124 passing tests from `T1-COMBAT-09b` remain present and passing in the new 133-test TRX.

## Feel-Review Artifact Check

`production/qa/combat/feel-review-09c-player-death.md` exists as a story-required evidence artifact. Agent-filled sections:

- Implementation perspective.
- What is intentionally absent.

Human-pending sections:

- What the death moment felt like.
- What the player read from the moment without UI text.
- What's missing.
- Death-cause clarity.
- Pre-fix-required vs defer.

The human-pending marker is `<!-- HUMAN PLAYTEST PENDING -->`; no slice verdict is issued in this file.

## Hygiene

- `git diff --check`: PASS. Git emitted the expected LF-to-CRLF normalization warning for `src/gameplay/combat/events/CombatDeathEvents.cs`; no whitespace errors.
- `bash .githooks/pre-commit`: PASS, `[pre-commit] OK`.
- Staging area: verified empty with `git diff --cached --name-only`, zero output.
