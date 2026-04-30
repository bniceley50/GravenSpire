# T1-COMBAT-04 Verification

## Command

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-04-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-04"
```

## Result

- Result: PASS
- Tests: 59 total, 59 passed, 0 failed
- TRX: `tests/evidence/T1-COMBAT-04/t1-combat-04-stage2.trx:372`

## Acceptance Evidence

| Check | Evidence |
| --- | --- |
| Fixed-tick melee resolution | `src/gameplay/combat/simulation/CombatSimulationStepper.cs:51`; `tests/integration/gameplay/combat/combat_melee_tick_resolution_test.cs:14` |
| Pause without catch-up | `src/gameplay/combat/simulation/CombatSimulationStepper.cs:38`; `tests/integration/gameplay/combat/combat_melee_tick_resolution_test.cs:28` |
| Out-of-range swing skip | `src/gameplay/combat/melee/CombatMeleeResolution.cs:135`; `tests/integration/gameplay/combat/combat_melee_tick_resolution_test.cs:63` |
| Hit chance formula | `src/gameplay/combat/melee/CombatMeleeFormulas.cs:67`; `tests/unit/gameplay/combat/combat_melee_formulas_test.cs:15`; `tests/unit/gameplay/combat/combat_melee_formulas_test.cs:28` |
| Damage formula | `src/gameplay/combat/melee/CombatMeleeFormulas.cs:87`; `tests/unit/gameplay/combat/combat_melee_formulas_test.cs:44`; `tests/unit/gameplay/combat/combat_melee_formulas_test.cs:68` |
| Seeded fixture formula execution | `src/gameplay/combat/melee/CombatMeleeFormulas.cs:87`; `tests/unit/gameplay/combat/combat_melee_formulas_test.cs:92` |
| Same-tick death priority | `src/gameplay/combat/melee/CombatMeleeResolution.cs:103`; `tests/integration/gameplay/combat/combat_melee_tick_resolution_test.cs:181` |

## Scope And Determinism Checks

- Negative T1 scope grep on the approved new-file batch: PASS, zero matches for the requested forbidden term set.
- Determinism check: PASS. Domain code uses caller-owned `ICombatClock` and injected `ICombatMeleeRandomSource`; no wall-clock API or engine frame-time API is used.
- Hardcoded tuning awareness scan: only ratio-bound checks, zero/minimum health checks, zero damage result construction, the minimum one-tick delay guard, and the explicit minimum successful-hit damage clamp appeared in `src/gameplay/combat/melee/*.cs`. Formula tuning coefficients are injected through `CombatMeleeHitChanceTuning` and `CombatMeleeDamageTuning`.
