# T1-COMBAT-06 - Tactical Cleric Instants Using Fixture-Loaded Numeric Values

**Status:** Complete
**Sprint:** 1
**Priority:** Must Have
**Layer:** Gameplay / Combat Core
**Type:** Logic + Integration + Config/Data
**Estimate:** 1.5 days
**Manifest Version:** Sprint 1, 2026-04-28
**GDD:** `design/gdd/combat-core.md`
**Governing ADR:** None new. T1 offline tier discipline remains governed by `DECISIONS.md` D003 and Combat Core's approved D012 contract.
**Evidence:** `tests/evidence/T1-COMBAT-06/verification.md`

## Scope

This story implements the production Combat Core domain slice for fixture-loaded Cleric tactical instants:

- Add executable zero-cast-time tactical ability profiles loaded from combat fixture data.
- Resolve explicit tactical ability activations without entering the slow-cast state machine.
- Spend mana through the same Combat-owned resource transition used by successful casts.
- Start cooldowns as transient resolver runtime timers only.
- Apply declared `direct_damage`, `self_buff`, and `interrupt_current_channel` effects.
- Emit ability lifecycle/result events for upstream availability owners and HUD consumers without Combat Core owning ability availability.
- Preserve the prior slow-cast lifecycle and route Bash channel cancellation through the existing cast cancel/recovery event surface.

Source trace: `production/sprints/sprint-1.md:282-315`.

## Out Of Scope

- Sprint-status updates, session-state edits, ADR metadata edits, GDD edits, hook edits, project-file edits, prior story/evidence-tree edits, med regen math, final HUD presentation, kill-credit emission, save barriers, player death payload emission, profiled feel harness, or architecture scan tooling.
- Ability availability, ability learning, memorized-slot availability, or final Class Design spell-list content.
- Auto-rotation, queue-on-cooldown behavior, or manual melee command spam.

## Dependencies

- `T1-COMBAT-05` complete: `production/stories/t1-combat-05-slow-cast-framework.md:3`.
- Current pure C# test bridge compiles Combat Core implementation and flat unit/integration test files at `tests/Gravenspire.Combat.Tests.csproj:17`.

## Acceptance Criteria Coverage

| AC | Status | Production Evidence | Test / Verification Evidence |
| --- | --- | --- | --- |
| `H-CCOM-INST-01` instant ability resolves without a cast bar | Covered | `CombatInstantAbilityResolver.Resolve` resolves profile-driven instant activations at `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:55`; validation rejects non-zero cast time at `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:199`; resolved activations spend mana and never call `BeginCast` at `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:118`. | Same-tick no-cast-bar behavior is tested at `tests/integration/gameplay/combat/combat_tactical_cleric_instants_test.cs:16`; passing TRX counter is `tests/evidence/T1-COMBAT-06/t1-combat-06-stage2.trx:504`. |
| `H-CCOM-FIXTURE-01` fixture-driven cost/cooldown/effect declarations | Covered | Executable tactical profiles are loaded through `CombatFixturePackage.TacticalInstantAbilityProfiles` at `src/gameplay/combat/fixtures/CombatFixtureModels.cs:76`; the appended JSON profile block starts at `assets/data/combat/t1-combat-fixtures.json:386`; required `cost_mana` and `effect_type` JSON bindings are declared at `src/gameplay/combat/fixtures/CombatFixtureModels.cs:424` and `src/gameplay/combat/fixtures/CombatFixtureModels.cs:465`. | Required profile rows and effect declarations are tested at `tests/unit/gameplay/combat/combat_fixture_validation_test.cs:94`; missing required profile fields are rejected at `tests/unit/gameplay/combat/combat_fixture_validation_test.cs:122`; missing effect-specific data is rejected at `tests/unit/gameplay/combat/combat_fixture_validation_test.cs:146`. |
| Mana spend uses the same Rule 13 resource transition | Covered | Instant resolution uses `WithCurrentMana` for successful mana spend at `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:118`; `AbilityResolvedEvent` records the spent amount at `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:163`. | Mana spend for Smite of Authority is tested at `tests/integration/gameplay/combat/combat_tactical_cleric_instants_test.cs:16`; Defensive Prayer mana spend is tested at `tests/integration/gameplay/combat/combat_tactical_cleric_instants_test.cs:85`. |
| Cooldowns are transient runtime timers only | Covered | Cooldown state is held in the resolver-local `cooldownEndTicks` dictionary at `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:53`; cooldown start is computed from Combat Simulation Tick at `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:162`. | Resolver-local cooldown behavior and fresh-resolver non-persistence are tested at `tests/integration/gameplay/combat/combat_tactical_cleric_instants_test.cs:38`. |
| Bash cancels current channel only through declared `interrupt_current_channel` effect | Covered | Effect type parsing maps only the declared `interrupt_current_channel` row at `src/gameplay/combat/abilities/CombatAbilityProfiles.cs:97`; Bash cancellation emits `CastCancelledEvent` at `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:136`; the additive actor helper routes the channel into cast recovery at `src/gameplay/combat/CombatActorStateTransitions.cs:283`. | Bash channel cancellation and Smite non-cancellation are tested at `tests/integration/gameplay/combat/combat_tactical_cleric_instants_test.cs:58`. |
| Self-buff duration comes from fixture/profile data | Covered | Self-buff timing is loaded through `RequiredTiming` at `src/gameplay/combat/abilities/CombatAbilityProfiles.cs:91`; `DefensivePrayer_T1_Prototype` declares `duration_seconds` in fixture data at `assets/data/combat/t1-combat-fixtures.json:457`. | Defensive Prayer duration and damage-reduction payload are tested at `tests/integration/gameplay/combat/combat_tactical_cleric_instants_test.cs:85`. |
| Static check rejects hardcoded tactical instant tuning in Combat Core production code | Covered | Ability resolver/profile production code receives tuning from `CombatTacticalAbilityProfile` and fixture models; no ability damage, cost, cooldown, duration, or scaling literal appears in `src/gameplay/combat/abilities/**`. | Hardcoded-tuning grep over `src/gameplay/combat/abilities/*.cs` found only zero/count/identity guards: `tests/evidence/T1-COMBAT-06/verification.md:24`. |

## Runnable Evidence

Stage 2 command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-06-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-06"
```

Result: PASS, 81 total, 81 passed, 0 failed. Evidence: `tests/evidence/T1-COMBAT-06/t1-combat-06-stage2.trx:504`; verification summary at `tests/evidence/T1-COMBAT-06/verification.md`.

## Story Status

`T1-COMBAT-06` is complete via `/story-done` with verdict `COMPLETE WITH NOTES`.

## Blockers / Carried Forward

- Med/sit regen and combat-exit timing remain owned by `T1-COMBAT-07`.
- HUD presentation, kill credit, save barriers, death payloads, profiled feel evidence, and architecture scan tooling remain owned by later Sprint 1 stories.
- Class Design still owns final tactical ability names/values beyond these prototype fixture rows.

## Completion Notes

**Completed**: 2026-04-30
**Verdict**: COMPLETE WITH NOTES

**Criteria**: 8/8 story checks covered. `H-CCOM-INST-01`, `H-CCOM-FIXTURE-01`, instant ability resolution without a cast bar, Rule 13 mana spend path, transient cooldown timer behavior, Bash cancellation only through declared `interrupt_current_channel`, fixture-driven self-buff duration, and the static/grep rejection of hardcoded tactical instant tuning all have file:line evidence in `## Acceptance Criteria Coverage`.
**Deferred/Untested Criteria**: None for this story boundary. Med/sit regen and combat-exit timing remain owned by `T1-COMBAT-07`; HUD presentation, kill credit, save barriers, death payloads, profiled feel evidence, and architecture scan tooling remain downstream by sprint plan. Class Design still owns final tactical ability names/values beyond these prototype fixture rows.
**Test Evidence**: Stage 2 TRX passed 81/81 at `tests/evidence/T1-COMBAT-06/t1-combat-06-stage2.trx:504`; verification summary is in `tests/evidence/T1-COMBAT-06/verification.md:3-100`; the hardcoded-tuning gate passed at `tests/evidence/T1-COMBAT-06/verification.md:16-24`.
**GDD/ADR Deviations**: None blocking. No ADR, GDD, D-entry, source, test, evidence, fixture, hook, or `.claude/**` edit was part of this closure batch.
**Scope Notes**: Negative T1 scope grep passed on the changed implementation and test files with zero matches for the requested forbidden terms; broader Combat Core grep found only existing non-ownership comments and the fixture README ban line. Closure edits are limited to the approved story, sprint-status, and active session-state files.
**Review Gates**: Lean `/story-done` closure; QA and lead-programmer subagents skipped by review mode. Story, sprint status, and active session state updated in the approved closure batch.
**Forced Completion**: No.
**Fixture Version**: `fixtureSetVersion` remains `CombatPrototypeSpellProfileSet_T1@2026-04-28-stage1`; no existing schema or row was changed, and the executable profile list is additive-only.
