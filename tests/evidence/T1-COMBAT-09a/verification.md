# T1-COMBAT-09a Verification

## Targeted Test Command

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-09a-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-09a"
```

Result: PASS, 113 total, 113 passed, 0 failed.

TRX counter: `tests/evidence/T1-COMBAT-09a/t1-combat-09a-stage2.trx:696`.

## Acceptance Coverage Anchors

- `H-CCOM-KILL-01`: `CombatActorDeathEvent` is defined at `src/gameplay/combat/events/CombatDeathEvents.cs:8`; `CombatKillResolutionPhase.Resolve` emits it once per defeated runtime NPC through the processed set at `src/gameplay/combat/death/CombatKillResolutionPhase.cs:24` and `src/gameplay/combat/death/CombatKillResolutionPhase.cs:51`; one-shot death coverage is `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:16` and `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:69`.
- `H-CCOM-KILL-01` qualifying contribution: player threat-table contribution is checked at `src/gameplay/combat/death/CombatKillResolutionPhase.cs:85`; request-local damage contribution is checked at `src/gameplay/combat/death/CombatKillResolutionPhase.cs:80`; covered by `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:37`, `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:55`, and `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:86`.
- Character Progression Rule 3 + Rule 6 payload preservation: `PlayerKillCreditEvent` exact four-field record is at `src/gameplay/combat/events/CombatDeathEvents.cs:16`; reflection schema guard starts at `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:103`.
- ADR-0001 no Combat event amendment boundary: production code reads only actor stable source, zone, faction, threat, runtime actor id for death-event/dedupe, and fixture-derived seed request input at `src/gameplay/combat/death/CombatKillResolutionPhase.cs:8`, `src/gameplay/combat/death/CombatKillResolutionPhase.cs:51`, `src/gameplay/combat/death/CombatKillResolutionPhase.cs:57`, `src/gameplay/combat/death/CombatKillResolutionPhase.cs:58`, `src/gameplay/combat/death/CombatKillResolutionPhase.cs:59`, `src/gameplay/combat/death/CombatKillResolutionPhase.cs:63`, `src/gameplay/combat/death/CombatKillResolutionPhase.cs:64`, `src/gameplay/combat/death/CombatKillResolutionPhase.cs:65`, and `src/gameplay/combat/death/CombatKillResolutionPhase.cs:85`.

## Payload Contract Grep

Command:

```powershell
$i=0; Get-Content -LiteralPath src\gameplay\combat\events\CombatDeathEvents.cs | ForEach-Object { $i++; if ($i -ge 16 -and $i -le 20) { "{0,4}: {1}" -f $i,$_ } }
```

Result: PASS. Full `PlayerKillCreditEvent` definition:

```text
  16: public sealed record PlayerKillCreditEvent(
  17:     CombatStableSourceRef defeated_source_ref,
  18:     string zoneId,
  19:     string? faction_id,
  20:     double kill_weight_seed);
```

## Banned Payload Field Grep

Command:

```powershell
rg -n "defeated_level|encounter_role|xp_value|xp_metadata|progression_transaction_id|loot|corpse_record|spell_data|tick_id|threat_table_snapshot" src\gameplay\combat\events\CombatDeathEvents.cs src\gameplay\combat\death\CombatKillResolutionPhase.cs
```

Result: PASS, zero matches.

Targeted `combat_actor_id` payload check:

```powershell
$content = Get-Content -Raw -LiteralPath src\gameplay\combat\events\CombatDeathEvents.cs; $content -match "PlayerKillCreditEvent\([\s\S]*combat_actor_id"
```

Result: PASS, `False`. `combat_actor_id` appears only in `CombatActorDeathEvent`, where it is allowed.

## Save-Barrier Scope Grep

Command:

```powershell
rg -n "ProgressionSaveBarrier|NpcSourceLifecycleSaveBarrier|XpAwardDedupeKey|XpAwardResolutionSnapshot|NpcSourceLifecycleRecord|SaveFailedEvent|DownstreamSaveBarrierUnresolved" src\gameplay\combat\events\CombatDeathEvents.cs src\gameplay\combat\death\CombatKillResolutionPhase.cs tests\integration\gameplay\combat\combat_npc_death_kill_credit_test.cs
```

Result: PASS, zero matches.

## Player-Death Scope Grep

Command:

```powershell
rg -n "PlayerDeathEvent|death_context_id|corpse_run|Death & Corpse Recovery" src\gameplay\combat\events\CombatDeathEvents.cs src\gameplay\combat\death\CombatKillResolutionPhase.cs tests\integration\gameplay\combat\combat_npc_death_kill_credit_test.cs
```

Result: PASS, zero matches.

## Hardcoded-Tuning Gate

Command:

```powershell
rg -n "[0-9]+(\.[0-9]+)?[dDfFlLmM]?" src\gameplay\combat\events\CombatDeathEvents.cs src\gameplay\combat\death\CombatKillResolutionPhase.cs
```

Result: PASS. Numeric literal hits are limited to zero guards for defeated health, positive finite seed, non-negative contribution, and positive contribution qualification; no fixture balance value is hardcoded in production.

## Negative T1 Scope Grep

Command:

```powershell
rg -n -i "FishNet|networking|network|server authority|server|PvP|companion|Warrior|Enchanter|OpenAI|Anthropic|DateTime|UtcNow|DateTime\.Now|Time\.deltaTime|deltaTime|System\.Random" src\gameplay\combat\events\CombatDeathEvents.cs src\gameplay\combat\death\CombatKillResolutionPhase.cs tests\integration\gameplay\combat\combat_npc_death_kill_credit_test.cs
```

Result: PASS, zero matches.

## Composition Verification

- Defeated source identity reuses existing actor stable source: `CombatActorState.StableSourceRef` at `src/gameplay/combat/CombatActorState.cs:377`; read sites are `src/gameplay/combat/death/CombatKillResolutionPhase.cs:58` and `src/gameplay/combat/death/CombatKillResolutionPhase.cs:63`.
- Runtime actor id remains transient: `CombatActorState.CombatActorId` is defined at `src/gameplay/combat/CombatActorState.cs:367`; read sites are `src/gameplay/combat/death/CombatKillResolutionPhase.cs:51` and `src/gameplay/combat/death/CombatKillResolutionPhase.cs:57`, both limited to one-shot processing and the runtime death event.
- Zone identity reuses `CombatActorState.ZoneId` at `src/gameplay/combat/CombatActorState.cs:387`; read sites are `src/gameplay/combat/death/CombatKillResolutionPhase.cs:59` and `src/gameplay/combat/death/CombatKillResolutionPhase.cs:64`.
- Faction identity reuses `CombatActorState.FactionId` at `src/gameplay/combat/CombatActorState.cs:382`; read site is `src/gameplay/combat/death/CombatKillResolutionPhase.cs:65`.
- Player contribution reuses `CombatActorState.ThreatTable` at `src/gameplay/combat/CombatActorState.cs:477`; read site is `src/gameplay/combat/death/CombatKillResolutionPhase.cs:85`.
- Same-resolution direct damage input is request-local only: `src/gameplay/combat/death/CombatKillResolutionPhase.cs:12` and `src/gameplay/combat/death/CombatKillResolutionPhase.cs:80`. It is not retained by the phase processed set and is not a long-lived tracker.
- Fixture-derived seed source is existing encounter fixture data: model property at `src/gameplay/combat/fixtures/CombatFixtureModels.cs:516`; data row at `assets/data/combat/t1-combat-fixtures.json:471`; test load site at `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:150`.

## Focused Test Anchors

- No double emit: `test_repeat_processing_same_defeated_runtime_actor_does_not_double_emit` starts at `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:69`.
- No contribution produces no kill credit: `test_no_qualifying_player_contribution_emits_death_event_but_no_kill_credit_event` starts at `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:55`.
- Exact four-field payload: `test_player_kill_credit_event_payload_schema_contains_exactly_approved_four_fields` starts at `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:103`.

## Hygiene

- `bash .githooks/pre-commit`: PASS, `[pre-commit] OK`.
- `git diff --check`: PASS.
- Staging area: empty during final verification.
