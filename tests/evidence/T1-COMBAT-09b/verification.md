# T1-COMBAT-09b Verification

## Targeted Test Command

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-09b-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-09b"
```

Result: PASS, 124 total, 124 passed, 0 failed.

TRX counter: `tests/evidence/T1-COMBAT-09b/t1-combat-09b-stage2.trx:762`.

## Boundary Test

Chosen approach: grep/source-scan test. `test_processor_reads_only_approved_player_kill_credit_fields` starts at `tests/unit/gameplay/progression/character_progression_kill_credit_processor_test.cs:85` and scans `CharacterProgressionKillCreditProcessor.cs` for `killCreditEvent.<field>` references.

Result: PASS. The only reads are:

```text
src\gameplay\progression\CharacterProgressionKillCreditProcessor.cs:245:        var defeatedSourceRef = killCreditEvent.defeated_source_ref;
src\gameplay\progression\CharacterProgressionKillCreditProcessor.cs:246:        var zoneId = killCreditEvent.zoneId;
src\gameplay\progression\CharacterProgressionKillCreditProcessor.cs:247:        var factionId = killCreditEvent.faction_id;
src\gameplay\progression\CharacterProgressionKillCreditProcessor.cs:248:        var killWeightSeed = killCreditEvent.kill_weight_seed;
```

## Frozen Event Contract

Command:

```powershell
git diff b2fe66f -- src\gameplay\combat\events\CombatDeathEvents.cs
```

Result: PASS, zero diff. `PlayerKillCreditEvent` remains exactly four fields at `src/gameplay/combat/events/CombatDeathEvents.cs:16`.

## No Combat Fallback / Live NPC Read

Command:

```powershell
rg -n "NpcRecord|CurrentHealth|CurrentMana|ThreatTable|CombatActorId|CombatActorState|retry|synthesize|fallback|live NPC|live_npc" src\gameplay\progression\CharacterProgressionKillCreditProcessor.cs
```

Result: PASS, zero matches. Progression consumes `PlayerKillCreditEvent` fields and its own registry/snapshot types only.

## No-Bytes-Written Assertion

The no-writer-call assertion is covered by:

- `tests/integration/core/save/save_grouped_barrier_consistency_test.cs:13` - one stable group member plus one unresolved member fails the save.
- `tests/integration/core/save/save_grouped_barrier_consistency_test.cs:33` - writer call count remains zero when any member is unresolved.
- `tests/integration/gameplay/progression/progression_save_barrier_kill_credit_consistency_test.cs:58` - held Progression barrier rejects same-frame save with zero writer calls and unchanged XP.

Production enforcement: `GroupedSaveAttemptCoordinator.AttemptGroupedSave` returns failure and emits `SaveFailedEvent` before `writer.Write` at `src/core/save/GroupedSaveAttemptCoordinator.cs:63`; the only writer call is after all barriers are stable at `src/core/save/GroupedSaveAttemptCoordinator.cs:75`.

## Hardcoded Tuning Grep

Command:

```powershell
rg -n "\b[1-9][0-9]*(\.[0-9]+)?[dDfFlLmM]?\b" src\gameplay\progression\CharacterProgressionKillCreditProcessor.cs src\gameplay\npc\NpcSourceLifecycleService.cs src\core\save\SaveStabilityBarrierProtocol.cs src\core\save\GroupedSaveAttemptCoordinator.cs
```

Result: PASS, zero matches. Non-zero XP values, dedupe-key lengths, barrier budgets, and timeouts are injected by tests/requests, not hardcoded in production.

## Negative T1 Scope Grep

Command:

```powershell
rg -n -i "FishNet|networking|network|server authority|server|PvP|companion|Warrior|Enchanter|OpenAI|Anthropic|DateTime|UtcNow|DateTime\.Now|Time\.deltaTime|deltaTime|System\.Random" src\gameplay\combat\death\CombatKillResolutionPhase.cs src\gameplay\progression\CharacterProgressionKillCreditProcessor.cs src\gameplay\npc\NpcSourceLifecycleService.cs src\core\save\SaveStabilityBarrierProtocol.cs src\core\save\GroupedSaveAttemptCoordinator.cs tests\unit\gameplay\progression\character_progression_kill_credit_processor_test.cs tests\integration\gameplay\combat\combat_npc_death_kill_credit_test.cs tests\integration\gameplay\progression\progression_save_barrier_kill_credit_consistency_test.cs tests\integration\core\save\save_grouped_barrier_consistency_test.cs
```

Result: PASS, zero matches.

## Existing 113-Test Regression Check

Command:

```powershell
$old = [xml](Get-Content -Raw -LiteralPath 'tests\evidence\T1-COMBAT-09a\t1-combat-09a-stage2.trx'); $new = [xml](Get-Content -Raw -LiteralPath 'tests\evidence\T1-COMBAT-09b\t1-combat-09b-stage2.trx'); ...
```

Result:

```text
old_total=113 old_passed=113 new_total=124 missing_old_passed=0
```

The prior 113 passing tests from 09a are still present and passed in the new 124-test TRX.

## Csproj Diff Inspection

Command:

```powershell
git diff -- tests\Gravenspire.Combat.Tests.csproj
```

Result: PASS. Diff is strictly additive compile includes; no package references, project name, target framework, or existing includes changed.

```diff
@@ -15,8 +15,14 @@

   <ItemGroup>
     <Compile Include="..\src\gameplay\combat\**\*.cs" Link="src\gameplay\combat\%(RecursiveDir)%(Filename)%(Extension)" />
+    <Compile Include="..\src\gameplay\progression\**\*.cs" Link="src\gameplay\progression\%(RecursiveDir)%(Filename)%(Extension)" />
+    <Compile Include="..\src\gameplay\npc\**\*.cs" Link="src\gameplay\npc\%(RecursiveDir)%(Filename)%(Extension)" />
+    <Compile Include="..\src\core\save\**\*.cs" Link="src\core\save\%(RecursiveDir)%(Filename)%(Extension)" />
     <Compile Include="unit\gameplay\combat\*.cs" Link="tests\unit\gameplay\combat\%(Filename)%(Extension)" />
+    <Compile Include="unit\gameplay\progression\*.cs" Link="tests\unit\gameplay\progression\%(Filename)%(Extension)" />
     <Compile Include="integration\gameplay\combat\*.cs" Link="tests\integration\gameplay\combat\%(Filename)%(Extension)" />
+    <Compile Include="integration\gameplay\progression\*.cs" Link="tests\integration\gameplay\progression\%(Filename)%(Extension)" />
+    <Compile Include="integration\core\save\*.cs" Link="tests\integration\core\save\%(Filename)%(Extension)" />
   </ItemGroup>
```

## CombatKillResolutionPhase Diff Inspection

Command:

```powershell
git diff -- src\gameplay\combat\death\CombatKillResolutionPhase.cs
```

Result: PASS. Diff is additive around the unchanged 09a `Resolve` path: acknowledgement status/result types, acknowledgement sink interface, held-emission tracking, `ResolveWithAcknowledgements`, held-ack release/status methods, and stable hold id construction.

Key hunk anchors:

- `src/gameplay/combat/death/CombatKillResolutionPhase.cs:49` - acknowledgement sink interface.
- `src/gameplay/combat/death/CombatKillResolutionPhase.cs:94` - held kill-credit dictionary.
- `src/gameplay/combat/death/CombatKillResolutionPhase.cs:132` - unchanged `PlayerKillCreditEvent` emission shape still constructed from the 09a inputs.
- `src/gameplay/combat/death/CombatKillResolutionPhase.cs:142` - new acknowledgement wrapper.
- `src/gameplay/combat/death/CombatKillResolutionPhase.cs:191` - release acknowledgement.

## Composition Trace

- Existing Combat event field surface read by Progression: `PlayerKillCreditEvent` at `src/gameplay/combat/events/CombatDeathEvents.cs:16`; field reads at `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:245`.
- Existing Combat stable source type read by Progression and NPC: `CombatStableSourceRef` at `src/gameplay/combat/CombatActorState.cs:135`; persistent NPC source accessor at `src/gameplay/combat/CombatActorState.cs:157`; spawn source accessor at `src/gameplay/combat/CombatActorState.cs:162`.
- Progression-owned internalized types: `ProgressionXpSourceLookupRow` at `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:48`; `XpAwardResolutionSnapshot` at `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:81`; `XpAwardDedupeKey` at `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:94`.
- NPC-owned internalized type: `NpcSourceLifecycleRecord` at `src/gameplay/npc/NpcSourceLifecycleService.cs:23`.
- Save-owned protocol types: `SaveStabilityBarrierRequest` at `src/core/save/SaveStabilityBarrierProtocol.cs:49`; `SaveStabilityBarrierResult` at `src/core/save/SaveStabilityBarrierProtocol.cs:60`; `SaveFailedEvent` at `src/core/save/SaveStabilityBarrierProtocol.cs:130`; `ISaveStabilityBarrier` at `src/core/save/SaveStabilityBarrierProtocol.cs:138`.
- Cross-system imports are one-way and narrow: Progression/NPC import Combat for `PlayerKillCreditEvent` and source refs; Save imports no gameplay namespace; Combat imports no Progression, NPC, or Save namespace.

## ADR Evidence

- ADR-0001 Progression ownership of lookup/snapshot/dedupe: `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:48`, `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:81`, and `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:94`.
- ADR-0001 NPC ownership of source lifecycle durability: `src/gameplay/npc/NpcSourceLifecycleService.cs:23` and defeat recording at `src/gameplay/npc/NpcSourceLifecycleService.cs:134`.
- ADR-0002 declared barrier request/result/event shape: `src/core/save/SaveStabilityBarrierProtocol.cs:49`, `src/core/save/SaveStabilityBarrierProtocol.cs:60`, and `src/core/save/SaveStabilityBarrierProtocol.cs:130`.
- ADR-0002 all-stable-or-fail grouping and no bytes on unresolved: `src/core/save/GroupedSaveAttemptCoordinator.cs:39`, `src/core/save/GroupedSaveAttemptCoordinator.cs:63`, and `src/core/save/GroupedSaveAttemptCoordinator.cs:75`.

## Hygiene

- `bash .githooks/pre-commit`: PASS, `[pre-commit] OK`.
- `git diff --check`: PASS. Git emitted LF-to-CRLF normalization warnings for touched text files, but no whitespace errors.
- Staging area: verified empty before final report.
