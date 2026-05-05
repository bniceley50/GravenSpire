# T1-COMBAT-09b - Same-Frame Save Barrier Kill-Credit Consistency

**Status:** Complete
**Sprint:** 1
**Priority:** Must Have
**Layer:** Gameplay / Combat Core + Character Progression + NPC System + Save/Load
**Type:** Logic + Integration
**Estimate:** 2.0 days
**Manifest Version:** Sprint 1, 2026-04-28
**GDD:** `design/gdd/combat-core.md`; `design/gdd/character-progression.md`; `design/gdd/npc-system.md`; `design/gdd/save-load-persistence.md`
**Governing ADR:** ADR-0001 XP Source Lifecycle Registry; ADR-0002 Save Stability Barrier Protocol
**Evidence:** `tests/evidence/T1-COMBAT-09b/verification.md`

## Scope

This story implements the T1 same-frame kill-credit consistency seam without changing the approved Combat kill-credit payload:

- Add a Combat-owned acknowledgement hook beside the 09a `CombatKillResolutionPhase.Resolve` path.
- Add Character Progression kill-credit processing from `XpAwardResolutionSnapshot`, injected XP formula parameters, and `XpAwardDedupeKey`.
- Add minimal NPC source lifecycle records and acknowledgement/save barrier support.
- Add the Save/Load barrier protocol and grouped save-attempt coordinator with no file I/O or storage-backend choice.
- Verify grouped barrier semantics: all stable or fail, `SaveFailedEvent(DownstreamSaveBarrierUnresolved)`, and no bytes written.

Source trace: `production/sprints/sprint-1.md:429-466`; user-approved 12-file 09b batch.

## Out Of Scope

- Any edit to `src/gameplay/combat/events/CombatDeathEvents.cs` or any Combat file outside `src/gameplay/combat/death/CombatKillResolutionPhase.cs`.
- Levels, XP curves, level-up events, spell eligibility changes, character-creation hooks, progression UI, faction reputation, fixture data, or durable progression tombstones.
- NPC spawn, schedule, identity, behavior, dialogue, faction allegiance, ambient loops, or full NPC records beyond `NpcSourceLifecycleRecord`.
- Save file I/O, HMAC, version stamping, migrations, storage backend, autosave triggers, save slots, or atomic persistence.
- Sprint-status/session-state/GDD/ADR/hook edits and prior story/evidence trees.

## Dependencies

- `T1-COMBAT-09a` implemented the unchanged `PlayerKillCreditEvent` emission path and passed 113/113 tests at `tests/evidence/T1-COMBAT-09a/t1-combat-09a-stage2.trx`.
- ADR-0001 defines Character Progression ownership of XP lookup, snapshots, and dedupe, plus NPC ownership of durable source lifecycle state.
- ADR-0002 defines declared bounded barriers, the `xp_source_lifecycle_consistency` group, and no-bytes-written failure semantics.

## Acceptance Criteria Coverage

| AC | Status | Production Evidence | Test / Verification Evidence |
| --- | --- | --- | --- |
| `H-CCOM-KILL-01` acknowledgement behavior | Covered | `ICombatKillCreditAcknowledgementSink` is defined at `src/gameplay/combat/death/CombatKillResolutionPhase.cs:49`; held emissions are tracked at `src/gameplay/combat/death/CombatKillResolutionPhase.cs:94`; `ResolveWithAcknowledgements` calls sinks and holds pending acknowledgements at `src/gameplay/combat/death/CombatKillResolutionPhase.cs:142`; release is `src/gameplay/combat/death/CombatKillResolutionPhase.cs:191`. | Hold/release coverage starts at `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:149`; idempotent re-emission rejection starts at `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:183`. |
| `H-CPRO-XP-02` XP award calculation from snapshot + registry | Covered | Lookup row, registry entry, and snapshot records are in `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:48`, `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:62`, and `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:81`; XP calculation uses only the snapshot plus injected formula parameters at `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:383`. | Award correctness test starts at `tests/unit/gameplay/progression/character_progression_kill_credit_processor_test.cs:17`. |
| `H-CPRO-XP-03` snapshot-only award path; no live NPC state read | Covered | `ProcessKillCredit` reads only the four Combat event fields at `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:245`; `TryCaptureSnapshot` resolves the progression-owned registry snapshot at `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:329`; the processor imports no NPC namespace. | Boundary/source-scan test starts at `tests/unit/gameplay/progression/character_progression_kill_credit_processor_test.cs:85`; no-fallback grep result is recorded in `tests/evidence/T1-COMBAT-09b/verification.md`. |
| `H-CPRO-XP-09` `XpAwardDedupeKey` session dedupe | Covered | `XpAwardDedupeKey` is defined at `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:94`; processed keys are session-local at `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:115`; duplicate rejection is at `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:270`. | Dedupe test starts at `tests/unit/gameplay/progression/character_progression_kill_credit_processor_test.cs:34`. |
| `H-CPRO-XP-14` missing lookup/snapshot rejects loudly with no Combat fallback | Covered | Missing snapshot rejection is in `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:250`; diagnostic emission is in `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:253`. | Missing snapshot test starts at `tests/unit/gameplay/progression/character_progression_kill_credit_processor_test.cs:54`; no-fallback grep result is recorded in `tests/evidence/T1-COMBAT-09b/verification.md`. |
| `H-CPRO-SL-06` ProgressionSaveBarrier stable read view or unresolved | Covered | `ProgressionSaveBarrier` name is exposed at `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:153`; stable/unresolved barrier resolution is `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:216`; pending kill credits drain before stable view at `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:325`. | Stable read-view test starts at `tests/unit/gameplay/progression/character_progression_kill_credit_processor_test.cs:68`; unresolved same-frame save test starts at `tests/integration/gameplay/progression/progression_save_barrier_kill_credit_consistency_test.cs:58`. |
| `H-CPRO-CB-01` Progression consumes approved Combat event only | Covered | `PlayerKillCreditEvent` remains exactly four fields at `src/gameplay/combat/events/CombatDeathEvents.cs:16`; processor field reads are only `defeated_source_ref`, `zoneId`, `faction_id`, and `kill_weight_seed` at `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:245`. | Frozen-event diff is zero against `b2fe66f`; source-scan test starts at `tests/unit/gameplay/progression/character_progression_kill_credit_processor_test.cs:85`; TRX counter is `tests/evidence/T1-COMBAT-09b/t1-combat-09b-stage2.trx:762`. |
| ADR-0001 source lifecycle ownership | Covered | Progression owns lookup/snapshot/dedupe records at `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:48`, `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:81`, and `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:94`; NPC owns `NpcSourceLifecycleRecord` at `src/gameplay/npc/NpcSourceLifecycleService.cs:23`. | Same-frame kill-credit/save integration test starts at `tests/integration/gameplay/progression/progression_save_barrier_kill_credit_consistency_test.cs:17`. |
| ADR-0002 grouped save barrier protocol | Covered | Barrier request/result/event/interface are in `src/core/save/SaveStabilityBarrierProtocol.cs:49`, `src/core/save/SaveStabilityBarrierProtocol.cs:60`, `src/core/save/SaveStabilityBarrierProtocol.cs:130`, and `src/core/save/SaveStabilityBarrierProtocol.cs:138`; grouped coordinator fails before writer call at `src/core/save/GroupedSaveAttemptCoordinator.cs:39`, `src/core/save/GroupedSaveAttemptCoordinator.cs:63`, and `src/core/save/GroupedSaveAttemptCoordinator.cs:75`. | Grouped barrier tests start at `tests/integration/core/save/save_grouped_barrier_consistency_test.cs:13` and `tests/integration/core/save/save_grouped_barrier_consistency_test.cs:33`. |

## Composition Trace

- Combat event contract consumed: `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)` at `src/gameplay/combat/events/CombatDeathEvents.cs:16`.
- Existing stable identity type consumed: `CombatStableSourceRef` at `src/gameplay/combat/CombatActorState.cs:135`, `SourceNpcId` at `src/gameplay/combat/CombatActorState.cs:157`, and `SourceSpawnRef` at `src/gameplay/combat/CombatActorState.cs:162`.
- Combat reads no Progression or NPC XP metadata. The acknowledgement hook imports no downstream namespace and carries only the unchanged `PlayerKillCreditEvent`.
- Progression imports Combat only for `PlayerKillCreditEvent` and `CombatStableSourceRef`; it internalizes `ProgressionXpSourceLookupRow`, `XpAwardResolutionSnapshot`, and `XpAwardDedupeKey`.
- NPC imports Combat only for `PlayerKillCreditEvent` and `CombatStableSourceRef`; it internalizes `NpcSourceLifecycleRecord`.
- Save imports no gameplay namespace; Progression and NPC implement the save barrier interface from `src/core/save/SaveStabilityBarrierProtocol.cs:138`.

## Runnable Evidence

Stage 2 command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-09b-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-09b"
```

Result: PASS, 124 total, 124 passed, 0 failed. Evidence: `tests/evidence/T1-COMBAT-09b/t1-combat-09b-stage2.trx:762`; verification summary at `tests/evidence/T1-COMBAT-09b/verification.md`.

## Story Status

`T1-COMBAT-09b` is complete.

## Blockers / Carried Forward

- Player death event payload narrowing remains owned by `T1-COMBAT-09c`.
- Profiled combat-feel evidence remains owned by `T1-COMBAT-10`.
- Forbidden-pattern compliance scan/analyzer remains owned by `T1-COMBAT-11`.

## Completion Notes

**Completed:** 2026-04-30
**Verdict:** COMPLETE WITH NOTES
**Criteria:** 9/9 covered: `H-CCOM-KILL-01` acknowledgement behavior; `H-CPRO-XP-02`; `H-CPRO-XP-03`; `H-CPRO-XP-09`; `H-CPRO-XP-14`; `H-CPRO-SL-06`; `H-CPRO-CB-01`; ADR-0001; ADR-0002.
**Test Evidence:** `tests/evidence/T1-COMBAT-09b/t1-combat-09b-stage2.trx:762` records 124 total / 124 passed / 0 failed; `tests/evidence/T1-COMBAT-09b/verification.md` records the verification summary.
**Frozen Event Invariant:** `PlayerKillCreditEvent` remained unchanged from 09a baseline `b2fe66f`; `git diff --exit-code b2fe66f -- src/gameplay/combat/events/CombatDeathEvents.cs` returned zero diff, and the event remains four fields at `src/gameplay/combat/events/CombatDeathEvents.cs:16`.
**Boundary Scan:** Character Progression reads only `defeated_source_ref`, `zoneId`, `faction_id`, and `kill_weight_seed` from the Combat kill-credit event, with source-scan coverage at `tests/unit/gameplay/progression/character_progression_kill_credit_processor_test.cs:85` and recorded reads in `tests/evidence/T1-COMBAT-09b/verification.md:17`.
**No-Bytes Assertion:** unresolved grouped save barriers keep writer call count at zero, anchored by `tests/integration/core/save/save_grouped_barrier_consistency_test.cs:33` and `tests/integration/gameplay/progression/progression_save_barrier_kill_credit_consistency_test.cs:58`; production writer call is after all barriers are stable at `src/core/save/GroupedSaveAttemptCoordinator.cs:75`.
**Regression Check:** prior 09a TRX regression check passed with `old_total=113 old_passed=113 new_total=124 missing_old_passed=0`, recorded at `tests/evidence/T1-COMBAT-09b/verification.md:76`.
**ADR Ride-Along:** ADR-0001 and ADR-0002 moved to `Accepted`; DECISIONS.md D007 and D008 moved to `Locked`. This is metadata-only and validated by T1-COMBAT-09b implementation commit `617a431`.
**Code Review:** Lean-mode gates skipped per `/story-done` rules.
**Tech Debt Logged:** None.
**Blockers Carried Forward:** `T1-COMBAT-09c` player death event payload narrowing, `T1-COMBAT-10` profiled combat-feel evidence, and `T1-COMBAT-11` forbidden-pattern compliance scan/analyzer.
