# T1-COMBAT-03 - Attack Toggle State Machine

**Status:** Complete
**Sprint:** 1
**Priority:** Must Have
**Layer:** Gameplay / Combat Core
**Type:** Logic + Integration
**Estimate:** 1.0 day
**Manifest Version:** Sprint 1, 2026-04-28
**GDD:** `design/gdd/combat-core.md`
**Governing ADR:** None new. T1 offline tier discipline remains governed by `DECISIONS.md` D003 and Combat Core's approved D012 contract.
**Evidence:** `tests/evidence/T1-COMBAT-03/verification.md`

## Scope

This story implements the production Combat Core domain slice for explicit player-controlled Attack state:

- Add a standalone Attack ON/OFF state machine.
- Keep Attack separate from targeting, tab cycling, pull flow, social assist, and spell actions.
- Validate the explicit toggle-on command against a selected live hostile target in the active haunt gate and melee range.
- Implement approved Attack-off transitions for player toggle off, target death, successful sit/med, combat exit, player death, and zone transition.
- Expose a HUD-safe current-state accessor and state-change signal without owning visual treatment.

Source trace: `production/sprints/sprint-1.md:158-195`.

## Out Of Scope

- Melee hit or damage formulas, swing resolution, cast lifecycle, tactical instant execution, med regen math, final HUD presentation, kill-credit emission, save barriers, death payload emission, profiled feel harness, and architecture scan tooling.
- `/story-done` closure, sprint-status updates, session-state edits, ADR metadata edits, GDD edits, or fixture-data edits.

## Dependencies

- `T1-COMBAT-01` complete: `production/stories/t1-combat-01-cleric-base-combat-actor-fixture-hydration.md:1-13`.
- `T1-COMBAT-02` complete: `production/stories/t1-combat-02-targeting-and-hostile-actor-claim.md:1-13`.
- Current pure C# test bridge compiles Combat Core implementation and flat unit/integration test files at `tests/Gravenspire.Combat.Tests.csproj:17-19`.

## Acceptance Criteria Coverage

| AC | Status | Production Evidence | Test / Verification Evidence |
| --- | --- | --- | --- |
| `H-CCOM-AA-01` | Covered | `CombatAttackStateMachine.ToggleOn` validates the explicit player command at `src/gameplay/combat/attack/CombatAttackStateMachine.cs:103`; successful toggle-on schedules the next swing from `weapon_delay_seconds` at `src/gameplay/combat/attack/CombatAttackStateMachine.cs:121-129`. | Toggle-on behavior and tick scheduling are tested at `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:13`; suite PASS counter is `tests/evidence/T1-COMBAT-03/t1-combat-03-stage2.trx:276`. |
| `H-CCOM-AA-03` | Covered | Passive paths are modeled separately from state-changing transitions at `src/gameplay/combat/attack/CombatAttackStateMachine.cs:36` and no-op through `src/gameplay/combat/attack/CombatAttackStateMachine.cs:154-158`; approved forced-off paths are constrained at `src/gameplay/combat/attack/CombatAttackStateMachine.cs:217-225`. | Passive no-enable table starts at `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:54`; no-target no-op starts at `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:73`; forced-off table starts at `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:93`; suite PASS counter is `tests/evidence/T1-COMBAT-03/t1-combat-03-stage2.trx:276`. |
| `H-CCOM-MED-01` edge precondition | Covered | `SuccessfulSitOrMed` is an approved Attack-off transition at `src/gameplay/combat/attack/CombatAttackStateMachine.cs:222` and is routed through `ForceOff` at `src/gameplay/combat/attack/CombatAttackStateMachine.cs:144-151`. | Successful sit/med precondition is tested at `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:117`; forced-off table also covers it at `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:94`; suite PASS counter is `tests/evidence/T1-COMBAT-03/t1-combat-03-stage2.trx:276`. |
| `H-CCOM-HUD-04` edge precondition | Covered | HUD-safe current state is exposed at `src/gameplay/combat/attack/CombatAttackStateMachine.cs:97`; HUD-facing state-change signals are exposed at `src/gameplay/combat/attack/CombatAttackStateMachine.cs:100-101` and emitted at `src/gameplay/combat/attack/CombatAttackStateMachine.cs:235-237`. | Attack ON signal is asserted at `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:13`; player-toggle OFF signal is asserted at `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:39`; forced-off signal table starts at `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:99`; suite PASS counter is `tests/evidence/T1-COMBAT-03/t1-combat-03-stage2.trx:276`. |

## Runnable Evidence

Stage 2 command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-03-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-03"
```

Result: PASS, 43 total, 43 passed, 0 failed. Evidence: `tests/evidence/T1-COMBAT-03/t1-combat-03-stage2.trx:276`; verification summary at `tests/evidence/T1-COMBAT-03/verification.md:10-16`.

## Story Status

`T1-COMBAT-03` is implemented and verified. It is awaiting `/story-done` for closure, sprint status update, and active session-state update.

## Blockers / Carried Forward

- ADR-0003 / D009 status metadata remains unchanged by request.
- `H-CCOM-F2B` seeded melee formula execution remains owned by `T1-COMBAT-04`.
- Creature / Enemy AI movement implementation remains outside this story; this story changes only player Attack intent.
- Melee resolution, cast lifecycle, tactical instant execution, med regen math, HUD presentation, kill credit, save barriers, death payloads, profiled feel evidence, and architecture scan tooling remain owned by later Sprint 1 stories.

## Completion Notes

**Completed**: 2026-04-30
**Verdict**: COMPLETE WITH NOTES
**Implementation Baseline**: commit `57286cb51b576744c49f98a91753042118fbaf81` (`Implement T1-COMBAT-03: Attack Toggle State Machine`)
**Closure Baseline**: commit `57286cb51b576744c49f98a91753042118fbaf81` before this approved `/story-done` closure batch
**Criteria**: 4/4 passing. `H-CCOM-AA-01`, `H-CCOM-AA-03`, `H-CCOM-MED-01` edge precondition, and `H-CCOM-HUD-04` edge precondition all have file:line evidence in `## Acceptance Criteria Coverage`.
**Deferred/Untested Criteria**: None for this story boundary. Full melee tick damage, full med regen behavior, final HUD presentation, kill credit, save barriers, death payloads, profiled feel evidence, and architecture scan tooling remain downstream by sprint plan.
**Test Evidence**: Stage 2 TRX passed 43/43 at `tests/evidence/T1-COMBAT-03/t1-combat-03-stage2.trx:276`; verification summary is in `tests/evidence/T1-COMBAT-03/verification.md:10-16`; live closure rerun passed 43/43 with `dotnet test tests\Gravenspire.Combat.Tests.csproj --no-restore --logger "console;verbosity=minimal"`.
**GDD/ADR Deviations**: None blocking. No ADR metadata was changed. `docs/architecture/control-manifest.md` is absent and was treated as advisory per the `/story-done` prompt.
**Scope Notes**: Negative T1 scope grep passed on the changed story, implementation, test, and evidence files with zero matches; `src/networking` is absent. Closure edits are limited to the approved story, sprint-status, and active session-state files.
**Review Gates**: Lean `/story-done` closure; QA and lead-programmer subagents skipped by review mode. Story, sprint status, and session state updated in the approved closure batch.
**Forced Completion**: No.
