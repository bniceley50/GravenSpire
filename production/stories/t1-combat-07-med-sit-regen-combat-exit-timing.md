# T1-COMBAT-07 - Med/sit Regen and Combat-exit Timing

**Status:** Complete
**Sprint:** 1
**Priority:** Must Have
**Layer:** Gameplay / Combat Core
**Type:** Logic + Integration + Config/Data
**Estimate:** 1.5 days
**Manifest Version:** Sprint 1, 2026-04-28
**GDD:** `design/gdd/combat-core.md`
**Governing ADR:** None new. T1 offline tier discipline remains governed by `DECISIONS.md` D003 and Combat Core's approved D012 contract.
**Evidence:** `tests/evidence/T1-COMBAT-07/verification.md`

## Scope

This story implements the production Combat Core domain slice for med/sit regen and combat-exit timing:

- Add fixture-owned med-break, regen, sitting-threat, and combat-exit tuning.
- Add deterministic regen formula and tick interval calculation using Combat Simulation Tick inputs.
- Add a sit posture transition that reuses the existing `CombatAttackStateMachine.ForceOff` path for `SuccessfulSitOrMed`.
- Add unsafe in-combat sit threat application against existing hostile threat tables.
- Add combat-exit timing that requires the authored timer to elapse and zero valid hostile threat entries.
- Preserve Combat Core's T1 offline, no-wall-clock, no-networking boundaries.

Source trace: `production/sprints/sprint-1.md:316-353`.

## Out Of Scope

- Sprint-status updates, session-state edits, ADR metadata edits, GDD edits, hook edits, project-file edits, prior story/evidence-tree edits, HUD presentation, kill-credit emission, save barriers, player death payload emission, profiled feel harness, architecture scan tooling, Class Design final ability names/values, spellbook/memorized slots, networking, PvP, companions, Warrior, Enchanter, OpenAI, or Anthropic.
- CombatActorState constructor changes, existing transition-helper rewrites, or changes to existing attack, pull, leash, melee, cast, ability, targeting, world, event, spatial, hydration, or progression-baseline code.

## Dependencies

- `T1-COMBAT-03` complete: `production/stories/t1-combat-03-attack-toggle-state-machine.md:3`.
- `T1-COMBAT-04` complete: `production/stories/t1-combat-04-melee-tick-weapon-delay-resolution.md:3`.
- `T1-COMBAT-05` complete: `production/stories/t1-combat-05-slow-cast-framework.md:3`.
- Current pure C# test bridge compiles Combat Core implementation and flat unit/integration test files at `tests/Gravenspire.Combat.Tests.csproj:17`.

## Acceptance Criteria Coverage

| AC | Status | Production Evidence | Test / Verification Evidence |
| --- | --- | --- | --- |
| `H-CCOM-MED-01` sitting disables auto-attack before regen/threat updates | Covered | `CombatPostureStateMachine.TrySit` calls `CombatAttackStateMachine.ForceOff` with `SuccessfulSitOrMed` before setting sitting posture at `src/gameplay/combat/state/CombatPostureStateMachine.cs:49`; the actor posture field is additive at `src/gameplay/combat/CombatActorState.cs:512`. | Ordering is tested at `tests/integration/gameplay/combat/combat_med_sit_regen_combat_exit_test.cs:16`; passing TRX counter is `tests/evidence/T1-COMBAT-07/t1-combat-07-stage2.trx:570`. |
| `H-CCOM-MED-02` sitting out of combat boosts mana regen | Covered | Fixture-owned regen tuning is appended at `assets/data/combat/t1-combat-fixtures.json:511`; formula selection applies the sitting multiplier only when `CombatState.OutOfCombat` at `src/gameplay/combat/regen/CombatRegenFormulas.cs:74`; mana is clamped through `WithCurrentMana` at `src/gameplay/combat/regen/CombatRegenResolver.cs:50`. | `Cleric_Mid_T1` fixture-loaded sitting regen equals 8 mana/tick at `tests/unit/gameplay/combat/combat_regen_formulas_test.cs:15`; max-mana clamp is tested at `tests/integration/gameplay/combat/combat_med_sit_regen_combat_exit_test.cs:46`. |
| `H-CCOM-MED-03` sitting in combat is unsafe and grants no med boost | Covered | `CombatThreatResolver.ApplySittingThreatBonus` updates existing hostile `ThreatTable` entries with fixture-owned `SittingThreatBonus` at `src/gameplay/combat/threat/CombatThreatResolver.cs:42`; in-combat mana regen receives the combat multiplier without the sitting med boost at `src/gameplay/combat/regen/CombatRegenFormulas.cs:74`. | Unsafe sit threat and no-mana-boost behavior are tested at `tests/integration/gameplay/combat/combat_med_sit_regen_combat_exit_test.cs:66`. |
| `H-CCOM-F5` combat-exit timer formula | Covered | `CombatExitStateMachine.Evaluate` derives elapsed seconds from Combat Simulation Tick indexes and requires `ValidHostileThreatEntries == 0` at `src/gameplay/combat/state/CombatExitStateMachine.cs:43`; valid hostile count is read from existing hostile threat tables at `src/gameplay/combat/threat/CombatThreatResolver.cs:20`. | The 30.0s false / 30.1s true / one-hostile false boundary is tested at `tests/unit/gameplay/combat/combat_exit_timer_test.cs:14`; integration with existing threat tables is tested at `tests/integration/gameplay/combat/combat_med_sit_regen_combat_exit_test.cs:95`. |
| `H-CCOM-FEEL-04` med-break pacing prerequisite | Covered | Fixture-owned `regenTickIntervalSeconds = 6.0`, `combatExitTimerSeconds = 30.0`, and mana regen parameters establish the deterministic prerequisite path at `assets/data/combat/t1-combat-fixtures.json:511`; interval conversion uses `CombatTickRateHz` at `src/gameplay/combat/regen/CombatRegenFormulas.cs:86`. | Tick interval conversion to 300 ticks at 50 Hz is tested at `tests/unit/gameplay/combat/combat_regen_formulas_test.cs:53`; future profiled med-break pacing remains owned by `T1-COMBAT-10`. |

## Cast-and-sit Interaction Policy

Manual sit requests during an active slow cast are rejected in this T1 story. Combat Core Rule 19 says the player can sit only while "not casting" and "not in recovery" at `design/gdd/combat-core.md:174`; Rule 14 also says sitting is a non-damage interrupt source at `design/gdd/combat-core.md:156`. This implementation applies the stricter manual sit eligibility guard now and leaves forced/external sitting-as-interrupt behavior to a later explicit interrupt-source story if Class Design or encounter content needs it. The rejection is tested at `tests/integration/gameplay/combat/combat_med_sit_regen_combat_exit_test.cs:126`.

## Runnable Evidence

Stage 2 command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-07-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-07"
```

Result: PASS, 92 total, 92 passed, 0 failed. Evidence: `tests/evidence/T1-COMBAT-07/t1-combat-07-stage2.trx:570`; verification summary at `tests/evidence/T1-COMBAT-07/verification.md`.

## Story Status

`T1-COMBAT-07` is complete via `/story-done` with verdict `COMPLETE WITH NOTES`.

## Blockers / Carried Forward

- HUD presentation, kill credit, save barriers, death payloads, profiled feel evidence, and architecture scan tooling remain owned by later Sprint 1 stories.
- Future forced/external sitting-as-interrupt behavior is not implemented here; manual sit during active cast is rejected by Rule 19 eligibility.

## Completion Notes

**Completed**: 2026-04-30
**Verdict**: COMPLETE WITH NOTES
**Implementation Baseline**: commit `b8ef6cbb283ee655838eaab1f0e35dc788a9e32d` (`Implement T1-COMBAT-07 med sit regen timing`)
**Closure Baseline**: commit `b8ef6cbb283ee655838eaab1f0e35dc788a9e32d` before this approved `/story-done` closure batch
**Criteria**: 6/6 story checks covered. `H-CCOM-MED-01`, `H-CCOM-MED-02`, `H-CCOM-MED-03`, `H-CCOM-F5`, the `H-CCOM-FEEL-04` prerequisite, and attack-off-before-regen sequencing all have file:line evidence in `## Acceptance Criteria Coverage`.
**Deferred/Untested Criteria**: None for this story boundary. `H-CCOM-FEEL-04` full profiled med-break pacing evidence remains owned by `T1-COMBAT-10`; HUD presentation, kill credit, save barriers, death payloads, profiled feel evidence, and architecture scan tooling remain downstream by sprint plan.
**Test Evidence**: Stage 2 TRX passed 92/92 at `tests/evidence/T1-COMBAT-07/t1-combat-07-stage2.trx:570`; verification summary is in `tests/evidence/T1-COMBAT-07/verification.md:3-86`.
**Composition Evidence**: Sit-forces-Attack-off reuses `CombatAttackStateMachine.ForceOff` at `src/gameplay/combat/state/CombatPostureStateMachine.cs:49` with no parallel attack state; sitting threat reuses existing `ThreatTable` / `AddThreat` at `src/gameplay/combat/threat/CombatThreatResolver.cs:42` and `src/gameplay/combat/threat/CombatThreatResolver.cs:62`; timing uses `CombatTick` inputs at `src/gameplay/combat/state/CombatExitStateMachine.cs:8-9` and `src/gameplay/combat/regen/CombatRegenResolver.cs:10`.
**Cast-and-Sit Policy**: Manual sit during an active slow cast is rejected per Combat Core Rule 19 (`design/gdd/combat-core.md:174`), documented at `tests/evidence/T1-COMBAT-07/verification.md:81`, and tested at `tests/integration/gameplay/combat/combat_med_sit_regen_combat_exit_test.cs:126`.
**GDD/ADR Deviations**: None blocking. No ADR, GDD, D-entry, source, test, evidence, fixture, hook, or `.claude/**` edit was part of this closure batch.
**Scope Notes**: Hardcoded-tuning gate passed: regen rate, combat-exit duration, sitting threat bonus, and regen tick interval come from `assets/data/combat/t1-combat-fixtures.json:511`; negative T1 scope grep passed on the T1-COMBAT-07 changed implementation and test files with zero forbidden-scope matches.
**Review Gates**: Lean `/story-done` closure; QA and lead-programmer subagents skipped by review mode. Story, sprint status, and active session state updated in the approved closure batch.
**Forced Completion**: No.
