# T1-COMBAT-08 - Attack ON HUD State Signal Hookup

**Status:** Implemented + Verified; awaiting `/story-done`
**Sprint:** 1
**Priority:** Must Have
**Layer:** Gameplay / Combat Core
**Type:** Logic + Integration
**Estimate:** 1.0 day
**Manifest Version:** Sprint 1, 2026-04-28
**GDD:** `design/gdd/combat-core.md`
**Governing ADR:** None new. T1 offline tier discipline remains governed by `DECISIONS.md` D003 and Combat Core's approved D012 contract.
**Evidence:** `tests/evidence/T1-COMBAT-08/verification.md`

## Scope

This story implements the production Combat Core domain slice for HUD-safe combat state projection:

- Add a gameplay-side presentation seam under `src/gameplay/combat/presentation/`.
- Expose a HUD-safe snapshot containing health, mana, target, cast, recovery, Attack ON/OFF, next swing readiness, categorical threat, and combat state.
- Expose Attack ON/OFF state-change signals from the existing `CombatAttackStateMachine` signal surface.
- Project threat as a categorical enum only; raw threat numbers remain internal implementation/test inputs.
- Preserve Layer 1 HUD ownership of visual treatment, styling, layout, animation, and rendering.

Source trace: `production/sprints/sprint-1.md:355-390`.

## Out Of Scope

- Sprint-status updates, session-state edits, ADR metadata edits, GDD edits, hook edits, project-file edits, fixture-data edits, prior story/evidence-tree edits, final HUD presentation, UI view-models under `src/ui/**`, Unity UI Toolkit/UGUI/MonoBehaviour integration, nameplates, floating combat text, colors, layouts, animations, kill-credit emission, save barriers, player death payload emission, profiled feel harness, or architecture scan tooling.
- Any edits to existing Combat Core source outside the new `presentation/` seam. The projection reads existing state surfaces only.

## Dependencies

- `T1-COMBAT-03` complete: `production/stories/t1-combat-03-attack-toggle-state-machine.md:3`.
- Current pure C# test bridge compiles Combat Core implementation and flat unit/integration test files at `tests/Gravenspire.Combat.Tests.csproj:17`.

## Acceptance Criteria Coverage

| AC | Status | Production Evidence | Test / Verification Evidence |
| --- | --- | --- | --- |
| `H-CCOM-HUD-01` combat state surface exposed | Covered | `CombatHudStateSnapshot` includes health, mana, target, cast, Attack ON/OFF, next swing readiness, categorical threat, and combat state at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:52`; `Project` fills those fields from existing Combat Core state at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:91`. | Snapshot coverage starts at `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:14`; passing TRX counter is `tests/evidence/T1-COMBAT-08/t1-combat-08-stage2.trx:654`. |
| `H-CCOM-HUD-02` HUD output only, no presentation ownership | Covered | The projection file lives in `src/gameplay/combat/presentation/CombatHudStateProjection.cs` and imports only `System` namespaces at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:3`; it exposes records/enums only and no UI/rendering types. | UI seam grep returned zero matches for Unity/UI/styling primitives; verification is recorded at `tests/evidence/T1-COMBAT-08/verification.md:28`. Snapshot/event tests prove consumable accessors and event stream at `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:14` and `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:52`. |
| `H-CCOM-HUD-03` categorical threat output, not raw numeric threat | Covered | `CombatHudThreatCategory` is the only HUD threat output type at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:10`; snapshot output carries `CombatHudThreatCategory` at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:59`; raw threat table values are consumed only inside `EvaluateThreatCategory` at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:126` and are not exposed on the snapshot. | Category tests cover absent, zero, listed, close, stable, contested, ignored invalid actors, and negative-entry failure beginning at `tests/unit/gameplay/combat/combat_hud_threat_category_test.cs:13`; sample categorical assertion is `CombatHudThreatCategory.ThreatClose` at `tests/unit/gameplay/combat/combat_hud_threat_category_test.cs:50`; raw-threat grep passed in `tests/evidence/T1-COMBAT-08/verification.md:38`. |
| `H-CCOM-HUD-04` Attack ON state exposed for HUD feedback | Covered | `ProjectAttackSignal` adapts existing `CombatAttackStateChangedSignal` into `CombatHudAttackStateSignal` at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:110`; `ProjectAttackSignals` exposes the event stream at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:118`; snapshot Attack ON and swing readiness are projected at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:102` and `src/gameplay/combat/presentation/CombatHudStateProjection.cs:235`. | Table-driven coverage includes Attack on/off, target death, successful sit, combat exit, player death, zone transition, current-state accessor matching event history, and no no-target pulse at `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:52`, `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:84`, `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:98`, and `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:110`. |

## Composition Verification

- `CombatHudStateProjection` reads `CombatActorState.CurrentHealth`, `MaxHealth`, `CurrentMana`, `MaxMana`, `TargetCombatActorId`, `ThreatTable`, `CombatState`, `CastRuntimeState`, `ActiveCastSpellId`, and `CastRecoveryRemainingSeconds` from the existing actor state surface at `src/gameplay/combat/CombatActorState.cs:397`, `src/gameplay/combat/CombatActorState.cs:402`, `src/gameplay/combat/CombatActorState.cs:407`, `src/gameplay/combat/CombatActorState.cs:412`, `src/gameplay/combat/CombatActorState.cs:457`, `src/gameplay/combat/CombatActorState.cs:467`, `src/gameplay/combat/CombatActorState.cs:477`, `src/gameplay/combat/CombatActorState.cs:482`, `src/gameplay/combat/CombatActorState.cs:492`, and `src/gameplay/combat/CombatActorState.cs:507`.
- Attack state and event history reuse `CombatAttackStateSnapshot`, `CombatAttackStateChangedSignal`, `CurrentState`, and `StateChangedSignals` at `src/gameplay/combat/attack/CombatAttackStateMachine.cs:49`, `src/gameplay/combat/attack/CombatAttackStateMachine.cs:61`, `src/gameplay/combat/attack/CombatAttackStateMachine.cs:97`, and `src/gameplay/combat/attack/CombatAttackStateMachine.cs:100`.
- Cast HUD data composes with `CombatCastProgressSnapshot` and `GetProgress` at `src/gameplay/combat/casting/CombatCastStateMachine.cs:57` and `src/gameplay/combat/casting/CombatCastStateMachine.cs:130`.

## Runnable Evidence

Stage 2 command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-08-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-08"
```

Result: PASS, 106 total, 106 passed, 0 failed. Evidence: `tests/evidence/T1-COMBAT-08/t1-combat-08-stage2.trx:654`; verification summary at `tests/evidence/T1-COMBAT-08/verification.md`.

## Story Status

`T1-COMBAT-08` is implemented and awaits `/story-done` closure.

## Blockers / Carried Forward

- Kill-credit chain, save barriers, death payloads, profiled feel evidence, and architecture scan tooling remain owned by later Sprint 1 stories.
- Layer 1 HUD still owns final Attack ON visual treatment, categorical threat presentation language, layout, animation, color, and rendering.
