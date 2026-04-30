# T1-COMBAT-02 - Targeting and Hostile Actor Claim

**Status:** Implemented + Verified; awaiting `/story-done`
**Sprint:** 1
**Priority:** Must Have
**Layer:** Gameplay / Combat Core
**Type:** Logic + Config/Data + Integration
**Estimate:** 2.5 days
**Manifest Version:** Sprint 1, 2026-04-28
**GDD:** `design/gdd/combat-core.md`
**Governing ADR:** None new. T1 offline tier discipline remains governed by `DECISIONS.md` D003 and Combat Core's approved D012 contract.
**Evidence:** `tests/evidence/T1-COMBAT-02/verification.md`

## Scope

This story implements the production Combat Core domain slice for targeting, hostile actor claim, pull, social assist, and leash hooks:

- Implement HauntZone / CityHubZone combat gate, including CityHub no-combat boundary.
- Implement target acquisition by radius and T1 line-of-sight contract.
- Implement NPC hostile claim/release boundaries.
- Implement body/LoS pull, initial threat, social assist, bounded social pulses, and deterministic query ordering.
- Implement path/leash hooks with pure-domain path-probe fakes because Creature / Enemy AI movement is not authored yet.
- Keep LoS layers, pull tuning, social assist tuning, and leash tuning in authored fixture data.

Source trace: `production/sprints/sprint-1.md:121-158`.

## Out Of Scope

- Attack toggle state machine, explicit Attack ON/OFF events, melee ticks, melee formulas, slow casting, tactical instant execution, med/sit regen, HUD hookup, kill-credit emission, save-barrier integration, player death payloads, profiled combat-feel harness, and forbidden-pattern analyzer execution.
- `T1-COMBAT-03` or any later Sprint 1 story.
- Creature / Enemy AI movement implementation, NavMeshAgent return-to-anchor behavior, patrol authoring, animation, VFX, audio playback, UI warning markers, or final HUD presentation.
- `/story-done` closure, sprint-status updates, or session-state edits.
- FishNet, networking, server authority, account identity, PvP, live LLM, companions, Warrior, Enchanter, or any T2+ combat surface.

Scope guard evidence: `design/gdd/combat-core.md:608-689`; `design/gdd/combat-core.md:818-819`; `assets/data/combat/README.md:12-22`.

## Dependencies

- `T1-COMBAT-01` complete: `production/stories/t1-combat-01-cleric-base-combat-actor-fixture-hydration.md:1-13`.
- `/test-setup` complete enough for the narrow pure C# bridge: `tests/README.md:1-18`; `tests/README.md:91-99`.
- Stage 1+2 implementation baseline landed in commit `f88571795ab2b8eabb30aa395a08bb7536e46eb2`.

## Acceptance Criteria Coverage

| AC | Status | Production / Data Evidence | Test / Verification Evidence |
| --- | --- | --- | --- |
| `H-CCOM-WS-01` | Covered | HauntZone permits hostile claim, targeting, threat creation, damage, and kill-credit hooks through `CombatZoneGate` at `src/gameplay/combat/world/CombatZoneGate.cs:89-121`. | Integration coverage starts at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:14`; Stage 2 and Stage 3 counters are `tests/evidence/T1-COMBAT-02/t1-combat-02-stage2.trx:179-180` and `tests/evidence/T1-COMBAT-02/t1-combat-02-stage3-rerun.trx:179-180`. |
| `H-CCOM-WS-02` | Covered | CityHubZone returns false for hostile targeting, claim, threat, damage, and kill-credit hooks at `src/gameplay/combat/world/CombatZoneGate.cs:89-121`. | Integration coverage starts at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:47`; Stage 3 rerun passed 27/27 at `tests/evidence/T1-COMBAT-02/t1-combat-02-stage3-rerun.trx:179-180`. |
| `H-CCOM-WS-03` | Covered | Zone transition cleanup clears transient target/threat and exposes cancellation / incoming-zone block flags at `src/gameplay/combat/world/CombatZoneGate.cs:129-163`. | Integration coverage starts at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:80`; verification trace is `tests/evidence/T1-COMBAT-02/verification.md:24`. |
| `H-CCOM-TGT-01` | Covered | Target selection filters by active HauntZone, alive hostile actor, radius, LoS, and deterministic ordering at `src/gameplay/combat/targeting/CombatTargetSelector.cs:53-152`. | Integration coverage starts at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:108`; verification trace is `tests/evidence/T1-COMBAT-02/verification.md:25`. |
| `H-CCOM-PULL-01` | Covered | Body/LoS pull claims a hostile, applies `proximity_threat_initial`, leaves Attack disabled, and exposes only pivot/stance-shift presentation at `src/gameplay/combat/CombatActorStateTransitions.cs:78`, `src/gameplay/combat/pull/CombatPullCoordinator.cs:151-198`. | Integration coverage starts at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:139`; verification trace is `tests/evidence/T1-COMBAT-02/verification.md:26`. |
| `H-CCOM-PULL-02` | Covered | Social-link assist predicates and deterministic ordering live in `CombatPullCoordinator` at `src/gameplay/combat/pull/CombatPullCoordinator.cs:181`, `src/gameplay/combat/pull/CombatPullCoordinator.cs:292-357`; authored defaults live in `assets/data/combat/t1-combat-fixtures.json:36-52`. | Integration coverage starts at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:163`; fixture profile coverage starts at `tests/unit/gameplay/combat/combat_fixture_validation_test.cs:61`. |
| `H-CCOM-PULL-03` | Covered | T1 LoS blockers, non-blocking layers, query overflow diagnostic, and deterministic query sorting are implemented at `src/gameplay/combat/spatial/CombatSpatialTypes.cs:109-142` and `src/gameplay/combat/targeting/CombatTargetSelector.cs:82`; fixture data starts at `assets/data/combat/t1-combat-fixtures.json:7`. | Integration coverage starts at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:203`; fixture tuning coverage starts at `tests/unit/gameplay/combat/combat_fixture_validation_test.cs:40`. |
| `H-CCOM-PULL-04` | Covered | Social assist pulse gating and one-join-per-pull episode state are implemented at `src/gameplay/combat/pull/CombatPullCoordinator.cs:103-128`, `src/gameplay/combat/pull/CombatPullCoordinator.cs:205-253`. | Integration coverage starts at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:233`; verification trace is `tests/evidence/T1-COMBAT-02/verification.md:29`. |
| `H-CCOM-LEASH-01` | Covered | Path partial/invalid and path-pending grace hooks enter Leashing, stop new attacks/casts, clear active attack intent, request return-to-anchor, and preserve threat memory at `src/gameplay/combat/leash/CombatLeashCoordinator.cs:99-163`. | Integration coverage starts at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:293`; verification trace is `tests/evidence/T1-COMBAT-02/verification.md:30`. |
| `H-CCOM-LEASH-02` | Covered | Re-aggro requires active memory, distance, LoS, and no anchor return; expiry or anchor return clears threat at `src/gameplay/combat/leash/CombatLeashCoordinator.cs:169-208`. | Integration coverage starts at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:324`; verification trace is `tests/evidence/T1-COMBAT-02/verification.md:31`. |
| `H-CCOM-ART-01` | Covered | Pull result exposes `EnemyPivotOrStanceShift`, empty warning-signal list, and no scripted encounter trigger at `src/gameplay/combat/pull/CombatPullCoordinator.cs:196-198`. | Integration coverage starts at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:139`; verification trace is `tests/evidence/T1-COMBAT-02/verification.md:32`. |

## Runnable Evidence

Stage 2 command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-02-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-02"
```

Result: PASS, 27 total, 27 passed, 0 failed. Evidence: `tests/evidence/T1-COMBAT-02/t1-combat-02-stage2.trx:179-180`; verification summary at `tests/evidence/T1-COMBAT-02/verification.md:10-16`.

Stage 3 rerun command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-02-stage3-rerun.trx" --results-directory "tests\evidence\T1-COMBAT-02"
```

Result: PASS, 27 total, 27 passed, 0 failed. Evidence: `tests/evidence/T1-COMBAT-02/t1-combat-02-stage3-rerun.trx:179-180`.

## Story Status

`T1-COMBAT-02` is implemented and verified. It is ready for `/story-done` to perform closure review, sprint-status update, any approved session-state update, and the final closure commit.

## Blockers / Carried Forward

- ADR-0003 / D009 status metadata still says `Proposed` (`docs/architecture/adr-0003-progression-baseline-snapshot-contract.md:3-4`; `DECISIONS.md:222-225`). This remains carried forward from `T1-COMBAT-01` and is not closed by targeting/pull/leash work.
- Creature / Enemy AI still owns actual return-to-anchor movement and NavMeshAgent behavior. This story supplies Combat Core leash hooks and test doubles only.
- Attack toggle remains off-by-contract on pull, but the actual Attack toggle state machine belongs to `T1-COMBAT-03`.
- Melee, casting, tactical instants, med/sit regen, HUD state, kill credit, save barriers, death payloads, profiled combat-feel evidence, and forbidden-pattern analyzer execution remain owned by later Sprint 1 stories.

## Completion Notes

**Implemented**: 2026-04-30
**Stage 1+2 Baseline**: commit `f88571795ab2b8eabb30aa395a08bb7536e46eb2` (`Implement T1-COMBAT-02 Stage 1+2: targeting + pull + leash + zone gate`)
**Criteria**: 11/11 covered for this story surface. `H-CCOM-WS-01`, `H-CCOM-WS-02`, `H-CCOM-WS-03`, `H-CCOM-TGT-01`, `H-CCOM-PULL-01`, `H-CCOM-PULL-02`, `H-CCOM-PULL-03`, `H-CCOM-PULL-04`, `H-CCOM-LEASH-01`, `H-CCOM-LEASH-02`, and `H-CCOM-ART-01` all have file:line evidence in `## Acceptance Criteria Coverage`.
**Deferred/Untested Criteria**: None for this story. Creature / Enemy AI movement implementation is outside this story and represented with path-probe fakes.
**Test Evidence**: Stage 2 TRX passed 27/27 at `tests/evidence/T1-COMBAT-02/t1-combat-02-stage2.trx:179-180`; Stage 3 rerun TRX passed 27/27 at `tests/evidence/T1-COMBAT-02/t1-combat-02-stage3-rerun.trx:179-180`.
**GDD/ADR Deviations**: None blocking. No ADR metadata was changed.
**Scope Notes**: `production/sprint-status.yaml` and `production/session-state/active.md` were not edited. `/story-done` owns closure status.
**Review Gates**: `/dev-story` implementation + evidence boundary only; `/story-done` closure still pending.
**Forced Completion**: No.
