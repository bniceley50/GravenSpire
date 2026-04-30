# T1-COMBAT-02 Verification

**Story:** `T1-COMBAT-02` - Targeting and hostile actor claim
**Stage:** Stage 2 initial evidence for the Option B dev-story cadence
**Date:** 2026-04-30
**Git baseline:** `565ee2682ca681b47401c66a6f98a7c0780687c6`
**Fixture set:** `CombatPrototypeSpellProfileSet_T1@2026-04-28-stage1`
**Status:** Stage 2 PASS; Stage 3 story handoff and rerun pending a separate commit boundary

## Stage 2 Command

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-02-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-02"
```

Result: PASS, 27 total, 27 passed, 0 failed. Machine-readable evidence is in `tests/evidence/T1-COMBAT-02/t1-combat-02-stage2.trx:179-180`.

## Stage 2 Acceptance Coverage

| AC | Stage 2 coverage | Evidence |
| --- | --- | --- |
| `H-CCOM-WS-01` | HauntZone activates hostile claim, targeting, threat creation, and kill-credit eligibility hooks. | `src/gameplay/combat/world/CombatZoneGate.cs:89`, `src/gameplay/combat/world/CombatZoneGate.cs:97`, `src/gameplay/combat/world/CombatZoneGate.cs:105`, `src/gameplay/combat/world/CombatZoneGate.cs:121`; tested at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:14`. |
| `H-CCOM-WS-02` | CityHubZone disables hostile targeting, claim, threat, damage, and kill-credit hooks. | `src/gameplay/combat/world/CombatZoneGate.cs:89`, `src/gameplay/combat/world/CombatZoneGate.cs:97`, `src/gameplay/combat/world/CombatZoneGate.cs:105`, `src/gameplay/combat/world/CombatZoneGate.cs:113`, `src/gameplay/combat/world/CombatZoneGate.cs:121`; tested at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:47`. |
| `H-CCOM-WS-03` | Zone transition cleanup clears transient target/threat and exposes cancel/blocked-result hooks. | `src/gameplay/combat/world/CombatZoneGate.cs:129`, `src/gameplay/combat/world/CombatZoneGate.cs:163`; tested at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:80`. |
| `H-CCOM-TGT-01` | Target selection filters by alive state, active HauntZone, radius, LoS, and deterministic ordering. | `src/gameplay/combat/targeting/CombatTargetSelector.cs:53`, `src/gameplay/combat/targeting/CombatTargetSelector.cs:82`, `src/gameplay/combat/targeting/CombatTargetSelector.cs:114`, `src/gameplay/combat/targeting/CombatTargetSelector.cs:146`, `src/gameplay/combat/targeting/CombatTargetSelector.cs:152`; tested at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:108`. |
| `H-CCOM-PULL-01` | Body/LoS pull claims the hostile, initializes `proximity_threat_initial`, keeps Attack off, and emits only pivot/stance-shift presentation. | `src/gameplay/combat/CombatActorStateTransitions.cs:78`, `src/gameplay/combat/pull/CombatPullCoordinator.cs:151`, `src/gameplay/combat/pull/CombatPullCoordinator.cs:172`, `src/gameplay/combat/pull/CombatPullCoordinator.cs:196`; tested at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:139`. |
| `H-CCOM-PULL-02` | Social-link assist predicates and deterministic assist ordering are implemented with default assist threat. | `src/gameplay/combat/pull/CombatPullCoordinator.cs:181`, `src/gameplay/combat/pull/CombatPullCoordinator.cs:292`, `src/gameplay/combat/pull/CombatPullCoordinator.cs:326`; tested at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:163`. |
| `H-CCOM-PULL-03` | LoS blocker layers, non-blocking layers, deterministic query sort, and query-buffer overflow diagnostic are covered. | `src/gameplay/combat/spatial/CombatSpatialTypes.cs:132`, `src/gameplay/combat/spatial/CombatSpatialTypes.cs:140`, `src/gameplay/combat/spatial/CombatSpatialTypes.cs:142`, `src/gameplay/combat/targeting/CombatTargetSelector.cs:82`; tested at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:203`. |
| `H-CCOM-PULL-04` | Social assist pulse timing is bounded and joined actors cannot join more than once per pull episode. | `src/gameplay/combat/pull/CombatPullCoordinator.cs:205`, `src/gameplay/combat/pull/CombatPullCoordinator.cs:221`, `src/gameplay/combat/pull/CombatPullCoordinator.cs:235`; tested at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:233`. |
| `H-CCOM-LEASH-01` | Path partial/invalid and path-pending grace hooks enter Leashing, stop new attacks/casts, clear active attack intent, and request return-to-anchor. | `src/gameplay/combat/leash/CombatLeashCoordinator.cs:99`, `src/gameplay/combat/leash/CombatLeashCoordinator.cs:125`, `src/gameplay/combat/leash/CombatLeashCoordinator.cs:127`, `src/gameplay/combat/leash/CombatLeashCoordinator.cs:129`, `src/gameplay/combat/leash/CombatLeashCoordinator.cs:161`; tested at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:293`. |
| `H-CCOM-LEASH-02` | Re-aggro requires active memory, distance, LoS, and no anchor return; expiry clears threat. | `src/gameplay/combat/leash/CombatLeashCoordinator.cs:169`, `src/gameplay/combat/leash/CombatLeashCoordinator.cs:188`, `src/gameplay/combat/leash/CombatLeashCoordinator.cs:197`, `src/gameplay/combat/leash/CombatLeashCoordinator.cs:201`; tested at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:324`. |
| `H-CCOM-ART-01` | Pull result exposes pivot/stance shift and no warning marker/bark/UI/stinger/scripted trigger affordance. | `src/gameplay/combat/pull/CombatPullCoordinator.cs:196`, `src/gameplay/combat/pull/CombatPullCoordinator.cs:197`, `src/gameplay/combat/pull/CombatPullCoordinator.cs:198`; tested at `tests/integration/gameplay/combat/combat_targeting_pull_leash_test.cs:139`. |

## Data Validation

- Targeting, LoS, pull, social-assist, and leash tuning fixture models are defined at `src/gameplay/combat/fixtures/CombatFixtureModels.cs:178`, `src/gameplay/combat/fixtures/CombatFixtureModels.cs:204`, `src/gameplay/combat/fixtures/CombatFixtureModels.cs:230`, and `src/gameplay/combat/fixtures/CombatFixtureModels.cs:266`.
- Fixture validators enforce the new authored data contract at `src/gameplay/combat/fixtures/CombatFixtureValidator.cs:87-90`, with specific checks at `src/gameplay/combat/fixtures/CombatFixtureValidator.cs:104`, `src/gameplay/combat/fixtures/CombatFixtureValidator.cs:131`, `src/gameplay/combat/fixtures/CombatFixtureValidator.cs:154`, and `src/gameplay/combat/fixtures/CombatFixtureValidator.cs:167`.
- Authored fixture data starts at `assets/data/combat/t1-combat-fixtures.json:7`, `assets/data/combat/t1-combat-fixtures.json:22`, `assets/data/combat/t1-combat-fixtures.json:28`, and `assets/data/combat/t1-combat-fixtures.json:36`.
- Unit coverage for fixture tuning starts at `tests/unit/gameplay/combat/combat_fixture_validation_test.cs:40` and `tests/unit/gameplay/combat/combat_fixture_validation_test.cs:61`.

## Negative Scope Scan

Command:

```powershell
rg -n "FishNet|NetworkObject|networking|server authority|server-authority|PvP|duel|friendly fire|live LLM|LLM|companion|Sister Elara|Warrior|Enchanter" "src/gameplay/combat" "assets/data/combat" "tests/unit/gameplay/combat" "tests/integration/gameplay/combat"
```

Result: PASS. The only hit is the existing fixture README exclusion line at `assets/data/combat/README.md:22`.

## Diff Check

Command:

```powershell
git diff --check
```

Result: PASS with line-ending normalization warnings only for touched text files; no whitespace errors.

## Stage 3 Pending

Stage 3 still needs:

- `production/stories/t1-combat-02-targeting-and-hostile-actor-claim.md`
- `tests/evidence/T1-COMBAT-02/t1-combat-02-stage3-rerun.trx`
- A Stage 3 section appended to this file

`production/sprint-status.yaml`, `production/session-state/active.md`, and ADR-0003/D009 metadata remain untouched by this `/dev-story` boundary.
