# T1-COMBAT-05 - Slow Cast Framework

**Status:** Implemented + Verified; awaiting `/story-done`
**Sprint:** 1
**Priority:** Must Have
**Layer:** Gameplay / Combat Core
**Type:** Logic + Integration
**Estimate:** 1.5 days
**Manifest Version:** Sprint 1, 2026-04-28
**GDD:** `design/gdd/combat-core.md`
**Governing ADR:** None new. T1 offline tier discipline remains governed by `DECISIONS.md` D003 and Combat Core's approved D012 contract.
**Evidence:** `tests/evidence/T1-COMBAT-05/verification.md`

## Scope

This story implements the production Combat Core domain slice for slow cast lifecycle:

- Add Combat-owned cast runtime state on the actor without owning spell slots.
- Validate slow cast requests against caster state, mana, selected target, active haunt zone, range, and line of sight.
- Enter `Casting` for valid non-zero cast-time requests and expose combat-clock cast progress.
- Resolve completion before same-tick incoming interrupt checks.
- Spend mana only on successful completion and route completion, cancel, and interrupt into recovery.
- Emit cast lifecycle events for downstream Spell Memorization and HUD consumers with transient runtime ids and Combat Simulation Tick ids.
- Use injected deterministic interrupt rolls and injected interrupt formula tuning.

Source trace: `production/sprints/sprint-1.md:239-280`.

## Out Of Scope

- Tactical instant execution, med regen math, final HUD presentation, kill-credit emission, save barriers, player death payload emission, profiled feel harness, architecture scan tooling, sprint-status updates, session-state edits, ADR metadata edits, GDD edits, fixture-data edits, or prior story/evidence-tree edits.
- Spellbook-slot ownership, memorized-slot availability, spell learning, or final Class Design spell-list content.

## Dependencies

- `T1-COMBAT-03` complete: `production/stories/t1-combat-03-attack-toggle-state-machine.md:3`.
- `T1-COMBAT-04` complete: `production/stories/t1-combat-04-melee-tick-weapon-delay-resolution.md:3`.
- Current pure C# test bridge compiles Combat Core implementation and flat unit/integration test files at `tests/Gravenspire.Combat.Tests.csproj:17-19`.

## Acceptance Criteria Coverage

| AC | Status | Production Evidence | Test / Verification Evidence |
| --- | --- | --- | --- |
| `H-CCOM-CAST-01` | Covered | `CombatCastStateMachine.StartCast` accepts valid slow casts at `src/gameplay/combat/casting/CombatCastStateMachine.cs:103`; `BeginCast` puts the actor in `Casting` and records active cast ids at `src/gameplay/combat/CombatActorStateTransitions.cs:116`; HUD-safe progress comes from `ProgressAt` at `src/gameplay/combat/casting/CombatCastStateMachine.cs:475`. | Valid 6s cast and normalized progress are tested at `tests/integration/gameplay/combat/combat_slow_cast_framework_test.cs:14`; passing TRX counter is `tests/evidence/T1-COMBAT-05/t1-combat-05-stage2.trx:444`. |
| `H-CCOM-CAST-02` | Covered | `ResolveCompletion` resolves completion at `src/gameplay/combat/casting/CombatCastStateMachine.cs:135`; mana spend and recovery transition occur at `src/gameplay/combat/casting/CombatCastStateMachine.cs:154`; completion and recovery-start events emit at `src/gameplay/combat/casting/CombatCastStateMachine.cs:162`. | Completion mana spend and recovery are tested at `tests/integration/gameplay/combat/combat_slow_cast_framework_test.cs:39`; passing TRX counter is `tests/evidence/T1-COMBAT-05/t1-combat-05-stage2.trx:444`. |
| `H-CCOM-CAST-03` | Covered | `CancelCast` handles manual cancellation at `src/gameplay/combat/casting/CombatCastStateMachine.cs:178`; cancellation and recovery-start events emit without mana spend at `src/gameplay/combat/casting/CombatCastStateMachine.cs:198`. | Manual cancel no-mana behavior is tested at `tests/integration/gameplay/combat/combat_slow_cast_framework_test.cs:58`; passing TRX counter is `tests/evidence/T1-COMBAT-05/t1-combat-05-stage2.trx:444`. |
| `H-CCOM-CAST-04` | Covered | `InterruptFromDamage` handles eligible damage interrupts at `src/gameplay/combat/casting/CombatCastStateMachine.cs:214`; injected interrupt formula input and random roll resolve at `src/gameplay/combat/casting/CombatCastStateMachine.cs:241`; `CastInterruptedEvent` emits at `src/gameplay/combat/casting/CombatCastStateMachine.cs:261`. | Damage interrupt no-mana behavior is tested at `tests/integration/gameplay/combat/combat_slow_cast_framework_test.cs:75`; passing TRX counter is `tests/evidence/T1-COMBAT-05/t1-combat-05-stage2.trx:444`. |
| `H-CCOM-CAST-05` | Covered | Zero or blocked damage returns `NoInterruptRoll` before RNG at `src/gameplay/combat/casting/CombatCastStateMachine.cs:238`; same-tick completion resolves before interrupt checks at `src/gameplay/combat/casting/CombatCastStateMachine.cs:229`. | Zero/blocked no-roll behavior is tested at `tests/integration/gameplay/combat/combat_slow_cast_framework_test.cs:104`; same-tick completion priority is tested at `tests/integration/gameplay/combat/combat_slow_cast_framework_test.cs:138`; passing TRX counter is `tests/evidence/T1-COMBAT-05/t1-combat-05-stage2.trx:444`. |
| `H-CCOM-F4` | Covered | `CalculateInterruptChance` implements the injected formula at `src/gameplay/combat/casting/CombatCastFormulas.cs:25`; pressure, remaining-fraction, and clamp math resolve at `src/gameplay/combat/casting/CombatCastFormulas.cs:62`. | The worked 0.65 example is tested at `tests/unit/gameplay/combat/combat_cast_interrupt_formulas_test.cs:12`; clamp coverage starts at `tests/unit/gameplay/combat/combat_cast_interrupt_formulas_test.cs:27`; passing TRX counter is `tests/evidence/T1-COMBAT-05/t1-combat-05-stage2.trx:444`. |
| `H-CCOM-IF-01` | Covered | Lifecycle event payload contracts live in `src/gameplay/combat/events/CombatCastLifecycleEvents.cs:8`; start, complete, interrupt, cancel, recovery-start, and recovery-end event records begin at `src/gameplay/combat/events/CombatCastLifecycleEvents.cs:24`. Combat Core emits events only and does not own spell slots. | Lifecycle payload ids and Combat Tick ids are tested at `tests/integration/gameplay/combat/combat_slow_cast_framework_test.cs:163`; payload assertions start at `tests/integration/gameplay/combat/combat_slow_cast_framework_test.cs:292`; passing TRX counter is `tests/evidence/T1-COMBAT-05/t1-combat-05-stage2.trx:444`. |
| Same-tick completion-before-interrupt priority | Covered | `InterruptFromDamage` routes due casts to completion before reading interrupt RNG at `src/gameplay/combat/casting/CombatCastStateMachine.cs:229`. | Same-tick priority test proves zero interrupt-roll calls at `tests/integration/gameplay/combat/combat_slow_cast_framework_test.cs:138`; passing TRX counter is `tests/evidence/T1-COMBAT-05/t1-combat-05-stage2.trx:444`. |

## Runnable Evidence

Stage 2 command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-05-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-05"
```

Result: PASS, 71 total, 71 passed, 0 failed. Evidence: `tests/evidence/T1-COMBAT-05/t1-combat-05-stage2.trx:444`; verification summary at `tests/evidence/T1-COMBAT-05/verification.md`.

## Story Status

`T1-COMBAT-05` is implemented and verified. It is awaiting `/story-done` for closure, sprint status update, and active session-state update.

## Blockers / Carried Forward

- ADR-0003 / D009 status metadata remains unchanged by request.
- Tactical Cleric instant execution remains owned by `T1-COMBAT-06`.
- Med/sit regen and combat-exit timing remain owned by `T1-COMBAT-07`.
- HUD presentation, kill credit, save barriers, death payloads, profiled feel evidence, and architecture scan tooling remain owned by later Sprint 1 stories.
- `.claude/skills/dev-story/SKILL.md:75` now appears stale relative to three Sprint 1 precedents where `/dev-story` creates the story handoff artifact from the sprint plan. Treat as a non-blocking lesson candidate for a later `/skill-improve` task, not as part of this implementation batch.
