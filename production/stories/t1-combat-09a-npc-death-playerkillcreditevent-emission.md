# T1-COMBAT-09a - NPC Death Resolution + Unchanged PlayerKillCreditEvent Emission

**Status:** Implemented + Verified; awaiting `/story-done`
**Sprint:** 1
**Priority:** Must Have
**Layer:** Gameplay / Combat Core
**Type:** Logic + Integration
**Estimate:** 1.0 day
**Manifest Version:** Sprint 1, 2026-04-28
**GDD:** `design/gdd/combat-core.md`
**Governing ADR:** ADR-0001 boundary only; no event amendment. T1 offline tier discipline remains governed by `DECISIONS.md` D003 and Combat Core's approved D012 contract.
**Evidence:** `tests/evidence/T1-COMBAT-09a/verification.md`

## Scope

This story implements the production Combat Core domain slice for NPC death and narrow player kill-credit emission:

- Add death/kill-credit event records under `src/gameplay/combat/events/`.
- Add a Combat-owned `CombatKillResolutionPhase` under `src/gameplay/combat/death/`.
- Emit `CombatActorDeathEvent(combat_actor_id, defeated_source_ref, zoneId)` once for a defeated NPC runtime actor.
- Emit unchanged `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)` once only when the player has qualifying threat or same-resolution damage contribution.
- Preserve stable defeated source refs from existing `CombatActorState.StableSourceRef`; transient runtime ids remain limited to `CombatActorDeathEvent`.

Source trace: `production/sprints/sprint-1.md:395-426`.

## Out Of Scope

- Sprint-status updates, session-state edits, ADR metadata edits, GDD edits, hook edits, project-file edits, fixture-data edits, prior story/evidence-tree edits, same-frame acknowledgement integration, source lifecycle persistence, downstream award processing, player death emission, Death & Corpse Recovery hooks, save-barrier behavior, profiled feel harness, or architecture scan tooling.
- Any edits to existing Combat Core source files. This story composes with existing public actor-state and fixture surfaces only.

## Dependencies

- `T1-COMBAT-04` complete: `production/stories/t1-combat-04-melee-tick-weapon-delay-resolution.md:3`.
- `T1-COMBAT-06` complete: `production/stories/t1-combat-06-tactical-cleric-instants-fixture-loaded-values.md:3`.
- Current pure C# test bridge compiles Combat Core implementation and flat integration test files at `tests/Gravenspire.Combat.Tests.csproj:17` and `tests/Gravenspire.Combat.Tests.csproj:19`.

## Acceptance Criteria Coverage

| AC | Status | Production Evidence | Test / Verification Evidence |
| --- | --- | --- | --- |
| `H-CCOM-KILL-01` NPC death emits runtime death event once | Covered | `CombatActorDeathEvent` is defined at `src/gameplay/combat/events/CombatDeathEvents.cs:8`; `CombatKillResolutionPhase.Resolve` emits it for defeated NPC actors at `src/gameplay/combat/death/CombatKillResolutionPhase.cs:56`. | One-shot death emission is tested at `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:16`; repeat-processing no-op coverage is at `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:69`; passing TRX counter is `tests/evidence/T1-COMBAT-09a/t1-combat-09a-stage2.trx:696`. |
| `H-CCOM-KILL-01` qualifying player contribution emits kill credit once | Covered | `HasQualifyingPlayerContribution` reads the defeated NPC threat table and optional request-local damage contribution at `src/gameplay/combat/death/CombatKillResolutionPhase.cs:72`; `PlayerKillCreditEvent` emission is at `src/gameplay/combat/death/CombatKillResolutionPhase.cs:61`. | Threat-qualified emission is tested at `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:37`; request-local damage contribution is tested at `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:86`; no-contribution rejection is tested at `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:55`. |
| Character Progression Rule 3 + Rule 6 payload preservation | Covered | `PlayerKillCreditEvent` contains exactly `defeated_source_ref`, `zoneId`, `faction_id`, and `kill_weight_seed` at `src/gameplay/combat/events/CombatDeathEvents.cs:16`. | Reflection schema guard is `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:103`; payload grep and full definition are recorded at `tests/evidence/T1-COMBAT-09a/verification.md:23`. |
| ADR-0001 no Combat event amendment boundary | Covered | The new production code imports no downstream gameplay namespaces and adds no downstream metadata fields; `CombatKillResolutionPhase` only emits Combat-owned records from actor state and the fixture-derived seed at `src/gameplay/combat/death/CombatKillResolutionPhase.cs:8`. | Banned payload and downstream-boundary greps are recorded at `tests/evidence/T1-COMBAT-09a/verification.md:38`, `tests/evidence/T1-COMBAT-09a/verification.md:56`, `tests/evidence/T1-COMBAT-09a/verification.md:66`, and `tests/evidence/T1-COMBAT-09a/verification.md:86`. |

## Composition Verification

- Defeated identity reuses existing `CombatActorState.StableSourceRef` at `src/gameplay/combat/CombatActorState.cs:377`; read sites are `src/gameplay/combat/death/CombatKillResolutionPhase.cs:58` and `src/gameplay/combat/death/CombatKillResolutionPhase.cs:63`.
- Runtime death identity reuses existing transient `CombatActorState.CombatActorId` at `src/gameplay/combat/CombatActorState.cs:367`; it is read only for `CombatActorDeathEvent` and the session-local one-shot processed set at `src/gameplay/combat/death/CombatKillResolutionPhase.cs:51` and `src/gameplay/combat/death/CombatKillResolutionPhase.cs:57`.
- Zone identity reuses existing `CombatActorState.ZoneId` at `src/gameplay/combat/CombatActorState.cs:387`; read sites are `src/gameplay/combat/death/CombatKillResolutionPhase.cs:59` and `src/gameplay/combat/death/CombatKillResolutionPhase.cs:64`.
- Faction identity reuses existing `CombatActorState.FactionId` at `src/gameplay/combat/CombatActorState.cs:382`; read site is `src/gameplay/combat/death/CombatKillResolutionPhase.cs:65`.
- Qualifying contribution defaults to existing `CombatActorState.ThreatTable` at `src/gameplay/combat/CombatActorState.cs:477`; read site is `src/gameplay/combat/death/CombatKillResolutionPhase.cs:85`.
- Same-resolution direct damage is accepted as request input only at `src/gameplay/combat/death/CombatKillResolutionPhase.cs:12` and checked at `src/gameplay/combat/death/CombatKillResolutionPhase.cs:80`; it is not stored, serialized, or made into a parallel long-lived tracker.
- `kill_weight_seed` is fixture-derived and supplied to the phase from existing encounter fixture data. The existing model property is `CombatEncounterFixture.KillWeightSeed` at `src/gameplay/combat/fixtures/CombatFixtureModels.cs:516`; fixture data declares `SoloTrash_EvenCon_T1.killWeightSeed = 1.25` at `assets/data/combat/t1-combat-fixtures.json:471`; test input loads that fixture row at `tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:150`.

## Runnable Evidence

Stage 2 command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-09a-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-09a"
```

Result: PASS, 113 total, 113 passed, 0 failed. Evidence: `tests/evidence/T1-COMBAT-09a/t1-combat-09a-stage2.trx:696`; verification summary at `tests/evidence/T1-COMBAT-09a/verification.md`.

## Story Status

`T1-COMBAT-09a` is implemented and verified, awaiting `/story-done`.

## Blockers / Carried Forward

- Same-frame acknowledgement, source lifecycle persistence, downstream award processing, and grouped save-barrier behavior remain owned by `T1-COMBAT-09b`.
- Player death event emission and player death payload narrowing remain owned by `T1-COMBAT-09c`.
- No downstream award assertions are made in 09a; this story verifies only event-emission shape, stable source identity, qualification, and one-shot behavior.
