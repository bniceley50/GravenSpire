# T1-COMBAT-03 Verification

**Story:** `T1-COMBAT-03` - Attack toggle state machine
**Stage:** Stage 2 implementation evidence
**Date:** 2026-04-30
**Git baseline:** `99a26a0`
**Status:** PASS; awaiting `/story-done` closure

## Test Command

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-03-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-03"
```

Result: PASS, 43 total, 43 passed, 0 failed. Machine-readable evidence is in `tests/evidence/T1-COMBAT-03/t1-combat-03-stage2.trx:276`.

## Acceptance Coverage

| AC | Coverage | Evidence |
| --- | --- | --- |
| `H-CCOM-AA-01` | Explicit player toggle-on validates selected hostile target and schedules the next weapon-delay tick. | `src/gameplay/combat/attack/CombatAttackStateMachine.cs:103`, `src/gameplay/combat/attack/CombatAttackStateMachine.cs:121`, `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:13`, `tests/evidence/T1-COMBAT-03/t1-combat-03-stage2.trx:276`. |
| `H-CCOM-AA-03` | Passive target/pull/spell paths do not enable Attack; no-target toggle no-ops; forced-off table covers target death, sit/med, combat exit, death, and zone transition. | `src/gameplay/combat/attack/CombatAttackStateMachine.cs:154`, `src/gameplay/combat/attack/CombatAttackStateMachine.cs:217`, `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:54`, `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:73`, `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:93`, `tests/evidence/T1-COMBAT-03/t1-combat-03-stage2.trx:276`. |
| `H-CCOM-MED-01` edge precondition | Successful sit/med is an approved forced-off transition and is tested before later regen/threat work. | `src/gameplay/combat/attack/CombatAttackStateMachine.cs:222`, `src/gameplay/combat/attack/CombatAttackStateMachine.cs:144`, `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:117`, `tests/evidence/T1-COMBAT-03/t1-combat-03-stage2.trx:276`. |
| `H-CCOM-HUD-04` edge precondition | Current-state accessor and state-change signals expose Attack ON/OFF without final visual ownership. | `src/gameplay/combat/attack/CombatAttackStateMachine.cs:97`, `src/gameplay/combat/attack/CombatAttackStateMachine.cs:100`, `src/gameplay/combat/attack/CombatAttackStateMachine.cs:235`, `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:13`, `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:39`, `tests/integration/gameplay/combat/combat_attack_toggle_state_machine_test.cs:99`, `tests/evidence/T1-COMBAT-03/t1-combat-03-stage2.trx:276`. |

## Boundary Notes

- Story file: `production/stories/t1-combat-03-attack-toggle-state-machine.md`.
- Sprint status and active session-state remain untouched by this `/dev-story` boundary.
- Negative scope scan on the approved new files passed with zero matches for the requested guard terms.
- `git diff --check` passed after the approved evidence files were written.
