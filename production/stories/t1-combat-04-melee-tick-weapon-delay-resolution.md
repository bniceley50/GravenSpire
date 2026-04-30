# T1-COMBAT-04 - Melee Tick / Weapon-Delay Resolution

**Status:** Implemented + Verified; awaiting `/story-done`
**Sprint:** 1
**Priority:** Must Have
**Layer:** Gameplay / Combat Core
**Type:** Logic + Integration
**Estimate:** 1.5 days
**Manifest Version:** Sprint 1, 2026-04-28
**GDD:** `design/gdd/combat-core.md`
**Governing ADR:** None new. T1 offline tier discipline remains governed by `DECISIONS.md` D003 and Combat Core's approved D012 contract.
**Evidence:** `tests/evidence/T1-COMBAT-04/verification.md`

## Scope

This story implements the production Combat Core domain slice for melee weapon-delay resolution:

- Add injected-tuning melee hit chance and melee damage formulas.
- Add deterministic RNG injection for hit and damage-roll sampling.
- Resolve eligible melee ticks from explicit Attack state and fixed Combat Simulation Tick ids.
- Revalidate target, range, facing tolerance, line of sight, actor alive state, and active combat zone gate on every eligible tick.
- Skip out-of-range swings without damage and without queued catch-up swings.
- Preserve same-tick priority by discarding swings against targets whose death resolved earlier on that tick.
- Add a caller-owned fixed-step simulation stepper that advances only explicit tick budgets and freezes while paused.

Source trace: `production/sprints/sprint-1.md:199-234`.

## Out Of Scope

- Cast lifecycle, tactical instant execution, med regen math, final HUD presentation, kill-credit emission, save barriers, player death payload emission, profiled feel harness, architecture scan tooling, sprint-status updates, session-state edits, ADR metadata edits, GDD edits, fixture-data edits, or existing story/evidence-tree edits.

## Dependencies

- `T1-COMBAT-03` complete: `production/stories/t1-combat-03-attack-toggle-state-machine.md:3`.
- Current pure C# test bridge compiles Combat Core implementation and flat unit/integration test files at `tests/Gravenspire.Combat.Tests.csproj:17-19`.

## Acceptance Criteria Coverage

| AC | Status | Production Evidence | Test / Verification Evidence |
| --- | --- | --- | --- |
| `H-CCOM-TICK-01` | Covered | `CombatSimulationStepper.Step` advances a caller-owned fixed clock by explicit tick budgets at `src/gameplay/combat/simulation/CombatSimulationStepper.cs:51` and records resolved tick ids at `src/gameplay/combat/simulation/CombatSimulationStepper.cs:53-59`; melee resolution consumes the scheduled tick id at `src/gameplay/combat/melee/CombatMeleeResolution.cs:91-93`. | Frame-rate-independent fixed-step melee resolution is tested at `tests/integration/gameplay/combat/combat_melee_tick_resolution_test.cs:14`; passing TRX counter is `tests/evidence/T1-COMBAT-04/t1-combat-04-stage2.trx:372`. |
| `H-CCOM-PAUSE-01` | Covered | Paused simulation returns without advancing the clock or dispatching melee results at `src/gameplay/combat/simulation/CombatSimulationStepper.cs:38-45`. | No wall-clock catch-up after pause is tested at `tests/integration/gameplay/combat/combat_melee_tick_resolution_test.cs:28`; passing TRX counter is `tests/evidence/T1-COMBAT-04/t1-combat-04-stage2.trx:372`. |
| `H-CCOM-AA-02` | Covered | Out-of-range eligible ticks return `OutOfRange` with zero damage and the next normal weapon-delay due tick at `src/gameplay/combat/melee/CombatMeleeResolution.cs:135-139`. | Out-of-range skip/no queue catch-up is tested at `tests/integration/gameplay/combat/combat_melee_tick_resolution_test.cs:63`; passing TRX counter is `tests/evidence/T1-COMBAT-04/t1-combat-04-stage2.trx:372`. |
| `H-CCOM-F1` | Covered | `CalculateHitChance` applies actor level, actor skill, injected tuning, and clamp bounds at `src/gameplay/combat/melee/CombatMeleeFormulas.cs:67-84`. | Equal-level example is tested at `tests/unit/gameplay/combat/combat_melee_formulas_test.cs:15`; clamp/boundary cases are tested at `tests/unit/gameplay/combat/combat_melee_formulas_test.cs:28`; passing TRX counter is `tests/evidence/T1-COMBAT-04/t1-combat-04-stage2.trx:372`. |
| `H-CCOM-F2` | Covered | `CalculateDamage` uses actor weapon damage, attack power, armor class, injected scalar tuning, injected damage-roll scalar, and the minimum successful-hit clamp at `src/gameplay/combat/melee/CombatMeleeFormulas.cs:87-108`. | Design example is tested at `tests/unit/gameplay/combat/combat_melee_formulas_test.cs:44`; minimum-damage clamp is tested at `tests/unit/gameplay/combat/combat_melee_formulas_test.cs:68`; passing TRX counter is `tests/evidence/T1-COMBAT-04/t1-combat-04-stage2.trx:372`. |
| `H-CCOM-F2B` seeded formula execution | Covered | Seeded/injected melee execution uses the same `CalculateDamage` production path at `src/gameplay/combat/melee/CombatMeleeFormulas.cs:87-108`; no fixture values are embedded in production formula code. | Low/top fixture extremes and seeded roll scalars are executed at `tests/unit/gameplay/combat/combat_melee_formulas_test.cs:92`; passing TRX counter is `tests/evidence/T1-COMBAT-04/t1-combat-04-stage2.trx:372`. |
| Same-tick death-before-swing priority | Covered | A target whose death resolved before the scheduled swing tick is discarded before random rolls or damage at `src/gameplay/combat/melee/CombatMeleeResolution.cs:103-111`. | Same-tick death priority is tested at `tests/integration/gameplay/combat/combat_melee_tick_resolution_test.cs:181`; passing TRX counter is `tests/evidence/T1-COMBAT-04/t1-combat-04-stage2.trx:372`. |
| Per-tick eligibility validation | Covered | Eligible melee ticks revalidate target identity, alive state, zone gate, range, facing, and line of sight at `src/gameplay/combat/melee/CombatMeleeResolution.cs:96-149`. | Range, facing, line of sight, alive state, and zone-gate invalidation are tested at `tests/integration/gameplay/combat/combat_melee_tick_resolution_test.cs:111` and `tests/integration/gameplay/combat/combat_melee_tick_resolution_test.cs:152`; passing TRX counter is `tests/evidence/T1-COMBAT-04/t1-combat-04-stage2.trx:372`. |

## Runnable Evidence

Stage 2 command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-04-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-04"
```

Result: PASS, 59 total, 59 passed, 0 failed. Evidence: `tests/evidence/T1-COMBAT-04/t1-combat-04-stage2.trx:372`; verification summary at `tests/evidence/T1-COMBAT-04/verification.md`.

## Story Status

`T1-COMBAT-04` is implemented and verified. It is awaiting `/story-done` for closure, sprint status update, and active session-state update.

## Blockers / Carried Forward

- ADR-0003 / D009 status metadata remains unchanged by request.
- Cast lifecycle, tactical instant execution, med regen math, HUD presentation, kill credit, save barriers, death payloads, profiled feel evidence, and architecture scan tooling remain owned by later Sprint 1 stories.
- Full NPC death event emission remains downstream; this story computes and returns melee damage results but does not emit death or kill-credit events.

## Completion Notes

**Implemented**: 2026-04-30
**Verdict**: Implemented + Verified; awaiting `/story-done`
**Implementation Baseline**: commit `aa85defc40fd4c91e19e4ffcbccdfd04524d231f` (`Close T1-COMBAT-03: Attack Toggle State Machine`)
**Criteria**: 7/7 story checks covered: `H-CCOM-TICK-01`, `H-CCOM-PAUSE-01`, `H-CCOM-AA-02`, `H-CCOM-F1`, `H-CCOM-F2`, seeded `H-CCOM-F2B` formula execution, and same-tick death-before-swing priority all have file:line evidence in `## Acceptance Criteria Coverage`.
**Deferred/Untested Criteria**: None for this story boundary.
**Test Evidence**: Stage 2 TRX passed 59/59 at `tests/evidence/T1-COMBAT-04/t1-combat-04-stage2.trx:372`; verification summary is in `tests/evidence/T1-COMBAT-04/verification.md`.
**GDD/ADR Deviations**: None blocking. No ADR metadata, GDD, fixture data, sprint status, or session-state files were changed.
**Scope Notes**: Implementation is additive under `src/gameplay/combat/melee/` and `src/gameplay/combat/simulation/`; existing Combat Core source files were not edited.
**Forced Completion**: No.
