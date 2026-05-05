# T1-COMBAT-09c - Player Death Payload Stub Reserved Integration

**Status:** Complete
**Sprint:** 1
**Priority:** Must Have
**Layer:** Gameplay / Combat Core
**Type:** Logic + Integration
**Estimate:** 0.75 days
**Manifest Version:** Sprint 1, 2026-04-28
**GDD:** `design/gdd/combat-core.md`
**Governing ADR:** None newly introduced; T1 offline tier discipline remains governed by `DECISIONS.md` D003 and Combat Core's approved D012 contract.
**Evidence:** `tests/evidence/T1-COMBAT-09c/verification.md`; `production/qa/combat/feel-review-09c-player-death.md`

## Scope

This story implements Combat-owned local-player death as a narrow reserved handoff:

- Add `PlayerDeathEvent(death_context_id, local_character_id, zoneId, death_position, killer_source_ref, death_cause_type)` as a sibling event beside the frozen `PlayerKillCreditEvent`.
- Add `CombatPlayerDeathResolver` to clamp lethal player damage to zero, mark Combat-owned life state dead, clear transient combat interaction state, and emit the player death event once.
- Add `CombatPersistenceProjection` as a save-safe typed read surface with exactly `current_health`, `current_mana`, `combat_life_state`, and optional `pending_death_handoff_payload`.
- Add the story-required implementation-perspective feel-review scaffold at `production/qa/combat/feel-review-09c-player-death.md`.

Source trace: `production/sprints/sprint-1.md:473-507`.

## Out Of Scope

- Death & Corpse Recovery behavior: no penalty, no XP loss calculation, no resurrection mechanism, no corpse probe, no recovery interaction, no corpse persistence, and no respawn-point logic.
- Save-side consumption of `CombatPersistenceProjection`, storage serialization, save coordinator participation, or load-side restoration.
- Any edit to `production/sprint-status.yaml`, `production/session-state/active.md`, ADR/D-entries, GDDs, fixture data, `.claude/**`, `.githooks/**`, prior story/evidence trees, or `tests/Gravenspire.Combat.Tests.csproj`.
- Any change to `CombatActorState.cs`, `CombatActorStateTransitions.cs`, `CombatKillResolutionPhase.cs`, `SaveStabilityBarrierProtocol.cs`, or `GroupedSaveAttemptCoordinator.cs`.

## Dependencies

- `T1-COMBAT-01` established `CombatActorState`, stable player identity, and hydration fail-loud behavior. Evidence: `tests/evidence/T1-COMBAT-01/verification.md`.
- `T1-COMBAT-04` established the lethal-health clamp pattern and same-tick death priority in melee resolution. Evidence: `tests/evidence/T1-COMBAT-04/verification.md`.
- `T1-COMBAT-09b` left `PlayerKillCreditEvent` frozen and passed 124/124 tests. Evidence: `tests/evidence/T1-COMBAT-09b/verification.md`.

## Composition Trace

- `PlayerKillCreditEvent` remains the approved four-field event at `src/gameplay/combat/events/CombatDeathEvents.cs:16`; `PlayerDeathEvent` is an additive sibling at `src/gameplay/combat/events/CombatDeathEvents.cs:25`.
- `CombatPlayerDeathResolver.Resolve` starts at `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:29` and only composes with `CombatActorState`, `CombatStableSourceRef`, and `CombatPoint3`.
- Lethal transition uses the existing health-clamp convention at `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:49`, then creates a dead Combat-owned actor state at `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:101`.
- `death_context_id` derivation is deterministic: `CreateDeathContextId` starts at `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:82` and encodes only `local_character_id`, `zoneId`, canonical `death_position`, canonical `killer_source_ref`, and `death_cause_type` at `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:90`. It excludes transient runtime id and combat-clock state by construction.
- One-shot emission is scoped to the derived context id at `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:63`.
- `CombatPersistenceProjection` is the typed save-safe read seam at `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:10`; its four whitelisted properties are at `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:24`, `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:26`, `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:28`, and `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:30`.

## Acceptance Criteria Coverage

| AC | Status | Production Evidence | Test / Verification Evidence |
| --- | --- | --- | --- |
| `H-CCOM-DEATH-01` lethal transition clamps health to zero | Covered | Lethal damage clamps via `Math.Max` at `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:49`; the dead actor state writes `currentHealth: default`, `CombatState.Dead`, and `CombatActorLifeState.Dead` at `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:101`. | Integration coverage starts at `tests/integration/gameplay/combat/combat_player_death_resolution_test.cs:13`; TRX counter is `tests/evidence/T1-COMBAT-09c/t1-combat-09c-stage2.trx:816`. |
| `H-CCOM-DEATH-02` event emits once with deterministic context id | Covered | `PlayerDeathEvent` construction is at `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:68`; duplicate derived ids are suppressed at `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:63`; deterministic derivation is at `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:82`. | One-shot coverage starts at `tests/integration/gameplay/combat/combat_player_death_resolution_test.cs:13`; deterministic id coverage starts at `tests/integration/gameplay/combat/combat_player_death_resolution_test.cs:62`. |
| `H-CCOM-DEATH-03` death payload schema is narrow | Covered | `PlayerDeathEvent` contains only the six approved fields at `src/gameplay/combat/events/CombatDeathEvents.cs:25`; the resolver emits only those values at `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:68`. | Reflection schema guard starts at `tests/unit/gameplay/combat/combat_player_death_payload_schema_test.cs:14`; stable-local-identity coverage starts at `tests/integration/gameplay/combat/combat_player_death_resolution_test.cs:89`; banned-field scan is recorded in `tests/evidence/T1-COMBAT-09c/verification.md`. |
| `H-CCOM-SL-01` combat persistence whitelist | Covered | `CombatPersistenceProjection` exposes only `current_health`, `current_mana`, `combat_life_state`, and `pending_death_handoff_payload` at `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:24`. | Whitelist reflection guard starts at `tests/unit/gameplay/combat/combat_persistence_projection_test.cs:14`; optional payload projection coverage starts at `tests/unit/gameplay/combat/combat_persistence_projection_test.cs:41`. |
| `H-CCOM-SL-03` no spell, cooldown, regen, or load-side synthesis surface | Covered | `CombatPersistenceProjection.FromPlayer` reads only `CurrentHealth`, `CurrentMana`, `LifeState`, and the optional pending payload at `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:32`; it has no public constructor. | No-constructor/no-restore guard starts at `tests/unit/gameplay/combat/combat_persistence_projection_test.cs:61`; projection reflection guard excludes transient combat fields at `tests/unit/gameplay/combat/combat_persistence_projection_test.cs:14`. |

## Runnable Evidence

Stage 2 command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-09c-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-09c"
```

Result: PASS, 133 total, 133 passed, 0 failed. Evidence: `tests/evidence/T1-COMBAT-09c/t1-combat-09c-stage2.trx:816`; verification summary at `tests/evidence/T1-COMBAT-09c/verification.md`.

## Story Status

`T1-COMBAT-09c` is complete.

## Blockers / Carried Forward

- Death & Corpse Recovery remains unauthored and unimplemented.
- Human qualitative playtest notes for the death moment remain pending in `production/qa/combat/feel-review-09c-player-death.md`.
- Profiled combat-feel evidence remains owned by `T1-COMBAT-10`.
- Forbidden-pattern compliance scan/analyzer remains owned by `T1-COMBAT-11`.

## Completion Notes

**Completed:** 2026-04-30
**Verdict:** COMPLETE WITH NOTES
**Criteria:** 5/5 covered: `H-CCOM-DEATH-01`, `H-CCOM-DEATH-02`, `H-CCOM-DEATH-03`, `H-CCOM-SL-01`, and `H-CCOM-SL-03`.
**Test Evidence:** `tests/evidence/T1-COMBAT-09c/t1-combat-09c-stage2.trx:816` records 133 total / 133 passed / 0 failed; `tests/evidence/T1-COMBAT-09c/verification.md` records the verification summary.
**Frozen Event Invariant:** `PlayerKillCreditEvent` remained unchanged from the `b2fe66f` baseline; the event remains four fields at `src/gameplay/combat/events/CombatDeathEvents.cs:16`, and `git diff b2fe66f -- src\gameplay\combat\events\CombatDeathEvents.cs` showed only the additive `PlayerDeathEvent` block.
**PlayerDeathEvent Shape:** `PlayerDeathEvent` is exactly six fields at `src/gameplay/combat/events/CombatDeathEvents.cs:25`: `death_context_id`, `local_character_id`, `zoneId`, `death_position`, `killer_source_ref`, and `death_cause_type`.
**Persistence Projection Shape:** `CombatPersistenceProjection` exposes exactly four read-only fields at `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:24`: `current_health`, `current_mana`, `combat_life_state`, and optional `pending_death_handoff_payload`.
**Death & Corpse Recovery Scope:** Stub-only discipline held. Scope scan found zero matches for `corpse`, `respawn`, `resurrection`, `xp_loss`, `corpse_run`, or `corpse_recovery` in the 09c production files.
**09b Save Coordinator:** `SaveStabilityBarrierProtocol.cs` and `GroupedSaveAttemptCoordinator.cs` remained unchanged from commit `617a431` by `git diff --exit-code 617a431 -- src\core\save\SaveStabilityBarrierProtocol.cs src\core\save\GroupedSaveAttemptCoordinator.cs`.
**Held-Policy Artifact:** `production/qa/combat/feel-review-09c-player-death.md` is durable story evidence with implementation-perspective sections, human playtest prompts, and `<!-- HUMAN PLAYTEST PENDING -->`; no agent-issued slice verdict.
**Code Review:** Lean-mode gates skipped per `/story-done` rules.
**Tech Debt Logged:** None.
**Blockers Carried Forward:** Death & Corpse Recovery remains unauthored; human qualitative death-moment playtest remains pending for the slice review; `T1-COMBAT-10` owns profiled feel evidence and the Unity harness per held policy; `T1-COMBAT-11` owns forbidden-pattern scan/analyzer and triggers the hard-stop before slice review.
