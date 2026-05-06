# T1.5-COMBAT-01 - Endurance State, HUD, Save Projection

**Status:** Implemented + Verified; awaiting `/story-done`
**Sprint:** 1.5
**Priority:** Must Have
**Layer:** Gameplay / Combat Core
**Type:** Integration
**Manifest Version:** Sprint 1.5, 2026-05-06
**GDD:** `design/gdd/combat-core.md`
**Governing ADR:** `docs/architecture/adr-0006-endurance-resource-model.md`
**Evidence:** `tests/evidence/T1.5-COMBAT-01/verification.md`

## Scope

This story implements the first production use of ADR-0006's quiet Endurance
contract:

- Add Combat-owned `MaxEndurance` and `CurrentEndurance` state.
- Preserve Endurance through Combat-owned state-copy and lethal/death paths.
- Expand `CombatPersistenceProjection` from the prior four read-only fields to
  five fields by adding `current_endurance`.
- Expose a quiet categorical Endurance signal through the gameplay-side HUD
  projection.
- Keep fixture schema, physical-instant resolver behavior, ADR status, and D013
  status untouched.

Source trace: `production/sprints/sprint-1-5.md:99` through
`production/sprints/sprint-1-5.md:137`; QA trace:
`production/qa/plans/qa-plan-sprint-1-5-20260506.md:89` through
`production/qa/plans/qa-plan-sprint-1-5-20260506.md:104`.

## Out Of Scope

- Any fixture data edit under `assets/data/combat/**`.
- Any `CombatProgressionBaselineSnapshot` change.
- Any `PlayerKillCreditEvent`, `PlayerDeathEvent`, or `CombatActorDeathEvent`
  schema change.
- Any ADR, D-entry, GDD, sprint plan, QA plan, sprint-status, session-state,
  `.claude/**`, or `.githooks/**` edit.
- Any `src/ui/**`, Unity API, UI styling, per-ability Endurance preview, or
  action-combat resource loop.
- Any Bash, fixture schema, instant resolver, FEEL-01, or FEEL-03 tuning work.

## Composition Trace

- Endurance actor state is additive: `MaxEndurance` and `CurrentEndurance` are
  appended to `CombatActorState` with validation at
  `src/gameplay/combat/CombatActorState.cs:337`,
  `src/gameplay/combat/CombatActorState.cs:421`, and
  `src/gameplay/combat/CombatActorState.cs:595`.
- Endurance hydration source is parameter-on-construction for this story:
  `CombatActorHydrationInput.MaxEndurance` supplies `max_endurance` at
  `src/gameplay/combat/CombatActorHydrator.cs:26`, and current resources carry
  `CurrentEndurance` at `src/gameplay/combat/CombatActorHydrator.cs:16`.
  Fixture data is deferred to `T1.5-COMBAT-02`; this preserves ADR-0003 because
  `CombatProgressionBaselineSnapshot` is not amended.
- Production copy paths preserve Endurance at
  `src/gameplay/combat/CombatActorStateTransitions.cs:289`,
  `src/gameplay/combat/CombatActorStateTransitions.cs:346`,
  `src/gameplay/combat/CombatActorStateTransitions.cs:397`,
  `src/gameplay/combat/melee/CombatMeleeResolution.cs:245`, and
  `src/gameplay/combat/death/CombatPlayerDeathResolver.cs:127`.
- `CombatPersistenceProjection` adds only `current_endurance` at
  `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:30`; the
  existing four fields remain read-only and semantically unchanged.
- HUD projection exposes enum-typed Endurance at
  `src/gameplay/combat/presentation/CombatHudStateProjection.cs:35` and
  `src/gameplay/combat/presentation/CombatHudStateProjection.cs:63`; it does
  not expose raw Endurance numbers.
- Existing source footprint from `git diff --stat`:
  - `src/gameplay/combat/CombatActorHydrator.cs`: 33 lines changed.
  - `src/gameplay/combat/CombatActorState.cs`: 33 lines changed.
  - `src/gameplay/combat/CombatActorStateTransitions.cs`: 36 lines changed.
  - `src/gameplay/combat/death/CombatPlayerDeathResolver.cs`: 4 lines changed.
  - `src/gameplay/combat/melee/CombatMeleeResolution.cs`: 4 lines changed.
  - `src/gameplay/combat/persistence/CombatPersistenceProjection.cs`: 5 lines changed.
  - `src/gameplay/combat/presentation/CombatHudStateProjection.cs`: 27 lines changed.

## Frozen Contracts

- `PlayerKillCreditEvent` remains unchanged at
  `src/gameplay/combat/events/CombatDeathEvents.cs:16`.
- `PlayerDeathEvent` remains unchanged at
  `src/gameplay/combat/events/CombatDeathEvents.cs:25`.
- `CombatActorDeathEvent` remains unchanged at
  `src/gameplay/combat/events/CombatDeathEvents.cs:8`.
- `CombatProgressionBaselineSnapshot` remains Endurance-free at
  `src/gameplay/combat/CombatProgressionBaselineSnapshot.cs:37` through
  `src/gameplay/combat/CombatProgressionBaselineSnapshot.cs:42`.
- D013 remains Proposed; ADR-0006 remains Proposed. Their status ride-along is
  still owned by `T1.5-COMBAT-02` closure if the physical-instant conversion
  validates the contract.

## Acceptance Criteria Coverage

| QA Case | Status | Implementation Evidence | Test Evidence |
| --- | --- | --- | --- |
| `QA-01-01` Endurance actor state validates and clamps | Covered | `MaxEndurance` / `CurrentEndurance` state and validation at `src/gameplay/combat/CombatActorState.cs:421` through `src/gameplay/combat/CombatActorState.cs:607`; transition helper at `src/gameplay/combat/CombatActorStateTransitions.cs:217`. | `tests/unit/gameplay/combat/combat_endurance_state_test.cs:14`; TRX row `tests/evidence/T1.5-COMBAT-01/t1-5-combat-01-stage2.trx:37`. |
| `QA-01-02` Combat persistence whitelist adds exactly Endurance | Covered | `current_endurance` is read from the player at `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:49`; property is read-only at `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:30`. | New QA test at `tests/unit/gameplay/combat/combat_endurance_state_test.cs:35`; existing whitelist assertion updated at `tests/unit/gameplay/combat/combat_persistence_projection_test.cs:22`; TRX row `tests/evidence/T1.5-COMBAT-01/t1-5-combat-01-stage2.trx:54`. |
| `QA-01-03` Persistence still excludes transient combat state | Covered | Projection public shape is still limited to declared read-only fields at `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:24` through `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:32`; `max_endurance` is intentionally absent. | `tests/unit/gameplay/combat/combat_endurance_state_test.cs:56`; TRX row `tests/evidence/T1.5-COMBAT-01/t1-5-combat-01-stage2.trx:98`. |
| `QA-01-04` HUD projection exposes quiet Endurance | Covered | HUD signal is `CombatHudEnduranceCategory`, not a numeric resource snapshot, at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:35` and `src/gameplay/combat/presentation/CombatHudStateProjection.cs:63`; projection logic is at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:226`. | `tests/unit/gameplay/combat/combat_endurance_state_test.cs:75`; existing HUD snapshot assertion at `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:41`; TRX row `tests/evidence/T1.5-COMBAT-01/t1-5-combat-01-stage2.trx:63`. |
| `QA-01-05` No `src/ui/**` dependency introduced | Covered | HUD projection remains in `src/gameplay/combat/presentation/CombatHudStateProjection.cs` and exposes primitives/enums only; no Unity/UI dependency grep returned zero matches. | `tests/unit/gameplay/combat/combat_endurance_state_test.cs:94`; static verification in `tests/evidence/T1.5-COMBAT-01/verification.md`; TRX row `tests/evidence/T1.5-COMBAT-01/t1-5-combat-01-stage2.trx:33`. |
| `QA-01-06` ADR-0003 non-constraint preserved | Covered | `CombatProgressionBaselineSnapshot` remains scoped to level, health, and mana baseline fields at `src/gameplay/combat/CombatProgressionBaselineSnapshot.cs:37` through `src/gameplay/combat/CombatProgressionBaselineSnapshot.cs:42`; sprint plan states ADR-0003 does not constrain persistence shape at `production/sprints/sprint-1-5.md:14`. | `tests/unit/gameplay/combat/combat_endurance_state_test.cs:115`; TRX row `tests/evidence/T1.5-COMBAT-01/t1-5-combat-01-stage2.trx:130`. |

## Runnable Evidence

- `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-5-combat-01-stage2.trx" --results-directory "tests\evidence\T1.5-COMBAT-01"` passed with `139/139`.
- TRX counters are at
  `tests/evidence/T1.5-COMBAT-01/t1-5-combat-01-stage2.trx:852`.
- Prior `133` passed tests are still present and passing; only the six
  `test_qa_01_*` methods are new.

## Story Status

`T1.5-COMBAT-01` is implemented and verified, awaiting `/story-done`.

## Blockers / Carried Forward

- `T1.5-COMBAT-02` still owns fixture schema/data, Bash Endurance spend, and
  Smite of Authority / Defensive Prayer mana carveout tests.
- ADR-0006 and D013 remain Proposed until the physical-instant conversion
  validates the remaining resource-split contract.
