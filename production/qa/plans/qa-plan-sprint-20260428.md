# QA Plan - Sprint 1 T1 Combat Core

**Date:** 2026-04-28
**Invocation:** `/qa-plan sprint`
**Scope:** Sprint 1 production Combat Core implementation through offline single-player T1 Cleric play.
**Confidence:** High for QA scope and risk model; medium for exact test commands until `/test-setup` creates the Unity test scaffold.

## QA Scope

Sprint 1 implements the minimum production T1 Combat Core slice that reproduces the D012-validated combat loop: fixture-hydrated Cleric combat actors, target/claim/pull, explicit Attack toggle, weapon-delay melee, slow casts, fixture-driven tactical instants, med-break recovery, HUD-facing Attack state, kill-credit handoff, save-barrier consistency, and profiled evidence (`production/sprints/sprint-1.md:3`, `production/sprints/sprint-1.md:5`).

The QA plan covers all Must Have stories `T1-COMBAT-01` through `T1-COMBAT-11` (`production/sprints/sprint-1.md:52`, `production/sprints/sprint-1.md:68`). It does not mark QA complete, change story status, or approve implementation start. The required next gate remains `/test-setup` before `/dev-story T1-COMBAT-01-cleric-base-combat-actor-fixture-hydration` (`production/sprints/sprint-1.md:35`, `production/sprint-status.yaml:13`).

T1 scope is strict: offline single-player, no networking/FishNet, no server authority, no PvP, no live LLM, no companions, and no Warrior/Enchanter implementation. Evidence: D003 locks T1 to single-player local saves with no netcode, account system, server backend, or live LLM calls (`DECISIONS.md:48`, `DECISIONS.md:55`, `DECISIONS.md:59`); Combat Core acceptance bans FishNet/networking combat authority, account identity, PvP, Warrior/Enchanter behavior, live LLM dependency, companion combat behavior, raid logic, and server combat state (`design/gdd/combat-core.md:604`, `design/gdd/combat-core.md:605`).

## Source List

Verification method: live repository reads with `rg`, `Get-Content`, `Test-Path`, and git inspection on 2026-04-28.

| Source | Use |
| --- | --- |
| `production/sprints/sprint-1.md:35` | Required pre-implementation gate order. |
| `production/sprints/sprint-1.md:52` | Sprint Must Have story table begins. |
| `production/sprints/sprint-1.md:634` | Explicit out-of-scope section. |
| `production/sprints/sprint-1.md:678` | Sprint Definition of Done. |
| `production/sprint-status.yaml:12` | QA plan pointer was empty before this artifact. |
| `production/sprint-status.yaml:13` | Implementation gate sequence. |
| `production/qa/agents-smoke-report-2026-04-28.md:303` | Historical smoke report says `/qa-plan sprint` is safe to proceed. |
| `design/gdd/combat-core.md:35` | D012 amendment scope. |
| `design/gdd/combat-core.md:598` | Combat Core acceptance criteria begin. |
| `design/gdd/combat-core.md:842` | Combat Core acceptance matrix begins. |
| `design/gdd/character-progression.md:20` | ADR-0001/0002/0003/0004 authority references. |
| `design/gdd/character-progression.md:703` | Character Progression acceptance criteria begin. |
| `design/gdd/save-load-persistence.md:85` | ADR-0002 downstream save-stability barriers. |
| `docs/architecture/adr-0001-xp-source-lifecycle-registry.md:33` | Combat kill-credit event enrichment must stay outside Combat. |
| `docs/architecture/adr-0002-save-stability-barrier-protocol.md:53` | Unresolved barriers fail loudly with no bytes written. |
| `docs/architecture/adr-0003-progression-baseline-snapshot-contract.md:111` | `CombatProgressionBaselineSnapshot` contract. |
| `docs/architecture/adr-0004-first-save-materialization-and-character-identity.md:59` | Character Creation owns `local_character_id`; Save/Load owns persistence and context. |
| `docs/registry/architecture.yaml:481` | Forbidden pattern registry begins. |
| `production/prototypes/combat-feel-report.md:198` | D012 pinned-engine validation headline pass section. |

## Assumptions

- Current live HEAD is `600f779` on `main`, with `origin/main` matching; verified by `git log -1`, `git rev-parse --short HEAD`, and `git rev-parse --short origin/main`.
- `production/sprints/sprint-1.md` and `production/sprint-status.yaml` still record draft HEAD `2e268b4` (`production/sprints/sprint-1.md:9`, `production/sprint-status.yaml:11`). This is stale metadata, not the live checkout. This plan records the discrepancy but does not update it.
- `production/session-state/active.md` is stale and still points to `/sprint-plan new` (`production/session-state/active.md:8`, `production/session-state/active.md:101`). It is not used as the routing source for this QA plan and is not edited.
- `production/stories/` is absent in the live checkout, while `production/sprint-status.yaml` references story files under that folder (`production/sprint-status.yaml:18`, `production/sprint-status.yaml:27`). This is a downstream handoff gap to resolve before `/dev-story` if that skill requires per-story files.

## Facts, Recommendations, and Inferences

Facts:

- Sprint 1 cannot start implementation until `/qa-plan sprint` and `/test-setup` have run (`production/sprints/sprint-1.md:35`, `production/sprints/sprint-1.md:37`, `production/sprints/sprint-1.md:41`).
- Combat Core D012 makes Attack toggle, tactical Cleric instants, and explicit Attack ON HUD state first-class sprint requirements while preserving `PlayerKillCreditEvent` (`design/gdd/combat-core.md:35`, `design/gdd/combat-core.md:37`).
- Character Progression must consume only `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)` and must not require Combat event expansion (`design/gdd/character-progression.md:57`, `design/gdd/character-progression.md:63`).
- Save/Load must invoke `ProgressionSaveBarrier` and `NpcSourceLifecycleSaveBarrier` before reading guarded payloads; unresolved barriers write no bytes and emit `SaveFailedEvent(DownstreamSaveBarrierUnresolved)` (`design/gdd/save-load-persistence.md:85`, `docs/architecture/adr-0002-save-stability-barrier-protocol.md:153`, `docs/architecture/adr-0002-save-stability-barrier-protocol.md:155`).

Recommendations:

- Run `/test-setup` next and make it produce the exact Unity Test Framework command surface, fixture folder conventions, and evidence-log paths required by this plan.
- Keep `T1-COMBAT-01` pure actor/fixture work. Do not absorb targeting, Attack toggle, casting, kill credit, or test scaffold work into the first dev story (`production/sprints/sprint-1.md:72`, `production/sprints/sprint-1.md:96`, `production/sprints/sprint-1.md:704`).
- Treat `T1-COMBAT-09b` as the highest integration-risk story because it crosses Combat, Character Progression, NPC source lifecycle, and Save/Load barrier behavior (`production/sprints/sprint-1.md:427`, `production/sprints/sprint-1.md:434`, `production/sprints/sprint-1.md:454`).

Inferred conclusions:

- The missing `production/stories/` folder does not block this QA plan because Sprint 1 embeds story details and acceptance traces directly in `production/sprints/sprint-1.md`, but it may block `/dev-story` if that skill requires the referenced story files.
- The historical smoke report still records the old `NEEDS_FIX` state (`production/qa/agents-smoke-report-2026-04-28.md:93`, `production/qa/agents-smoke-report-2026-04-28.md:127`), but live HEAD `600f779` repaired the two truncated agent specs. This plan relies on the live repo verification and user-provided baseline while leaving the historical report untouched.

## Risk Matrix

| Risk | Severity | Evidence | QA Response |
| --- | --- | --- | --- |
| Test scaffold absent at sprint start | Blocking | `/test-setup` is a required gate (`production/sprints/sprint-1.md:35`); live `Test-Path tests` returned false. | `/test-setup` must define test commands, folders, fixtures, and log output before T1-COMBAT-01 starts. |
| T1 scope creep into networking, PvP, companions, future classes, or LLM | Blocking | D003 T1 scope (`DECISIONS.md:55`, `DECISIONS.md:59`); H-CCOM-SCOPE-01 (`design/gdd/combat-core.md:604`). | Add negative static scan in T1-COMBAT-11 and repeat scope scan for every PR. |
| Combat fixture values become hardcoded in production logic | High | Fixture package requirement (`design/gdd/combat-core.md:658`); tactical instant no-hardcode requirement (`design/gdd/combat-core.md:746`). | Data-validation tests plus static scan for tactical instant numeric literals outside approved fixture/config files. |
| Attack turns on implicitly through targeting, pull, or spell flow | High | Attack rule separates toggle from targeting/pulling/casts (`design/gdd/combat-core.md:113`); H-CCOM-AA-03 (`design/gdd/combat-core.md:646`). | Table-driven integration tests for every non-toggle pathway and every forced-off condition. |
| HUD shows misleading Attack ON state | High | D012 requires explicit Attack ON feedback (`design/gdd/combat-core.md:596`); H-CCOM-HUD-04 (`design/gdd/combat-core.md:830`). | HUD-state event/accessor tests plus no-pulse no-target no-op case. |
| Kill-credit event payload expands or Combat starts carrying XP metadata | Blocking | Combat event contract (`design/gdd/combat-core.md:194`); Progression narrow consumption (`design/gdd/character-progression.md:863`); ADR-0001 rejects adding XP metadata to Combat events (`docs/architecture/adr-0001-xp-source-lifecycle-registry.md:240`). | Schema tests requiring exact event fields and forbidden-field static scans. |
| Same-frame kill/save serializes mismatched XP and source lifecycle state | Blocking | Combat kill-resolution hold (`design/gdd/combat-core.md:194`); Save barrier rule (`design/gdd/save-load-persistence.md:85`); ADR-0002 grouped barriers (`docs/architecture/adr-0002-save-stability-barrier-protocol.md:177`). | Integration tests for stable/stable, stable/unresolved, unresolved/stable, and deadline-expired barrier cases. |
| `CombatProgressionBaselineSnapshot` becomes a generic progression payload | High | ADR-0003 bans generic all-consumer snapshots (`docs/architecture/adr-0003-progression-baseline-snapshot-contract.md:58`); forbidden registry records the pattern (`docs/registry/architecture.yaml:550`). | Snapshot schema tests plus architecture forbidden-pattern scan. |
| First-save materialization is confused with normal save barriers | High | ADR-0004 separates first-save materialization from later save barriers (`docs/architecture/adr-0004-first-save-materialization-and-character-identity.md:193`, `docs/architecture/adr-0004-first-save-materialization-and-character-identity.md:281`). | Integration tests for first-save success/failure and subsequent-load missing-payload failure. |
| Profiled evidence drifts from D012 prototype schema | Medium | Sprint JSONL schema is defined (`production/sprints/sprint-1.md:532`); pinned validation passed on Unity 6000.3.14f1 (`production/prototypes/combat-feel-report.md:198`, `production/prototypes/combat-feel-report.md:254`). | T1-COMBAT-10 JSONL schema validation, build SHA capture, fixture-set version capture, and scenario labels. |

## Test Suites

### Suite 0 - Setup Gate

Purpose: establish the local Unity test surface before implementation.

Required output from `/test-setup`:

- Exact command for unit tests.
- Exact command for integration/PlayMode tests.
- Folder conventions for `src/**`, `assets/data/**`, `tests/unit/**`, `tests/integration/**`, `tests/performance/**`, and `production/qa/**`.
- Fixture validation convention for combat and progression data.
- Evidence-log location and filename pattern.

Pass condition: T1-COMBAT-01 can cite a working command and an initially failing or scaffold smoke test before production combat code lands.

### Suite 1 - Scope and Forbidden Pattern Static Checks

Coverage:

- `H-CCOM-SCOPE-01`.
- Architecture forbidden patterns from `docs/registry/architecture.yaml:481`.
- Sprint negative scope exclusions (`production/sprints/sprint-1.md:634`, `production/sprints/sprint-1.md:649`).

Minimum checks:

- No `FishNet`, `NetworkObject`, `server authority`, prediction, replication, or networking combat placeholders in T1 Combat Core code.
- No PvP, duels, friendly fire, account identity, server identity, live LLM, companions, Sister Elara combat behavior, Warrior, or Enchanter implementation.
- No `combat_actor_id` as XP identity or persistence identity.
- No generic `ProgressionBaselineSnapshot` handoff to Combat.
- No `visible_level`, XP progress fields, `spell_eligibility_tier`, spell ids, or UI read-model fields inside `CombatProgressionBaselineSnapshot`.
- No Save/Load direct read of guarded downstream state before declared barriers.

### Suite 2 - Actor Fixture and Hydration

Stories: `T1-COMBAT-01`.

Automated coverage:

- Actor schema unit tests for fields required by `H-CCOM-ACTOR-01` (`design/gdd/combat-core.md:630`).
- Fixture validator tests for lowest/mid/top T1 Cleric, trash, named, spell, tactical instant, and encounter fixtures (`design/gdd/combat-core.md:658`).
- Hydration integration tests that accept valid `CombatProgressionBaselineSnapshot` and reject missing/malformed/max-resource-invalid snapshots (`docs/architecture/adr-0003-progression-baseline-snapshot-contract.md:111`, `docs/architecture/adr-0003-progression-baseline-snapshot-contract.md:135`).
- Fixture assertion: `Cleric_Mid_T1` resolves as level 5, 140 HP, 180 mana (`production/sprints/sprint-1.md:106`).

### Suite 3 - Targeting, Claim, Pull, Social Assist, and Leash

Stories: `T1-COMBAT-02`.

Automated coverage:

- HauntZone enables combat and CityHub disables hostile combat (`design/gdd/combat-core.md:608`, `design/gdd/combat-core.md:612`).
- Target acquisition radius and LoS tests (`design/gdd/combat-core.md:634`).
- Body/LoS pull initializes threat but does not enable Attack (`design/gdd/combat-core.md:668`, `production/sprints/sprint-1.md:153`).
- Social assist bounded pulse tests (`design/gdd/combat-core.md:672`, `design/gdd/combat-core.md:680`).
- Leash path failure and re-aggro/memory expiry tests (`design/gdd/combat-core.md:684`, `design/gdd/combat-core.md:688`).

### Suite 4 - Attack Toggle and HUD State

Stories: `T1-COMBAT-03`, `T1-COMBAT-08`.

Automated coverage:

- Table-driven `H-CCOM-AA-03` cases listed in Sprint 1 (`production/sprints/sprint-1.md:177`, `production/sprints/sprint-1.md:188`).
- `H-CCOM-HUD-04` event/accessor cases listed in Sprint 1 (`production/sprints/sprint-1.md:374`, `production/sprints/sprint-1.md:385`).
- No-target toggle request no-ops and leaves Attack off (`design/gdd/combat-core.md:113`).
- Successful sit, combat exit, death, target death, and zone transition force Attack off (`design/gdd/combat-core.md:113`, `design/gdd/combat-core.md:439`).

Manual coverage:

- Attack ON state is visually unmistakable in the temporary dev HUD or debug overlay without prescribing final Layer 1 HUD styling.

### Suite 5 - Melee Tick, Formula, and Simulation Clock

Stories: `T1-COMBAT-04`.

Automated coverage:

- Fixed tick at `combat_tick_rate_hz = 50` (`design/gdd/combat-core.md:620`, `production/sprints/sprint-1.md:224`).
- Hard-stop pause resumes from state tick with no wall-clock catch-up (`design/gdd/combat-core.md:624`).
- Melee hit chance and damage formula examples, clamp cases, and minimum 1 damage (`design/gdd/combat-core.md:650`, `design/gdd/combat-core.md:654`).
- Out-of-range swing skips damage without queuing multiple swings (`design/gdd/combat-core.md:642`, `design/gdd/combat-core.md:113`).
- Same-tick death-before-swing priority (`production/sprints/sprint-1.md:226`).

### Suite 6 - Casting and Tactical Instants

Stories: `T1-COMBAT-05`, `T1-COMBAT-06`.

Automated coverage:

- Valid slow cast enters `Casting`; completion spends mana and enters recovery (`design/gdd/combat-core.md:718`, `design/gdd/combat-core.md:722`).
- Manual cancel and damage interrupt spend no mana (`design/gdd/combat-core.md:726`, `design/gdd/combat-core.md:734`).
- Same-frame cast completion priority (`design/gdd/combat-core.md:738`).
- Spell Memorization lifecycle event payloads without Combat owning slots (`design/gdd/combat-core.md:742`).
- Tactical instant profiles resolve without cast bar, spend mana through the normal path, start cooldown, and use fixture-declared effects without hardcoded numeric values (`design/gdd/combat-core.md:746`, `production/sprints/sprint-1.md:300`).

### Suite 7 - Med/Sit Regen and Combat Exit

Stories: `T1-COMBAT-07`.

Automated coverage:

- Sitting disables auto-attack before med-break regen begins (`design/gdd/combat-core.md:752`, `design/gdd/combat-core.md:439`).
- `Cleric_Mid_T1` sitting out of combat gains 8 mana/tick and never exceeds max (`production/sprints/sprint-1.md:338`, `production/sprints/sprint-1.md:340`).
- Sitting while on hostile threat table grants no med multiplier and adds `sitting_threat_bonus` (`design/gdd/combat-core.md:446`).
- Combat exit requires no valid hostile entries and timer threshold (`design/gdd/combat-core.md:764`, `production/sprints/sprint-1.md:342`).

### Suite 8 - Death, Kill Credit, Progression, and Save Barrier Integration

Stories: `T1-COMBAT-09a`, `T1-COMBAT-09b`, `T1-COMBAT-09c`.

Automated coverage:

- NPC death emits `CombatActorDeathEvent` once and qualifying player contribution emits `PlayerKillCreditEvent` once (`design/gdd/combat-core.md:782`).
- `PlayerKillCreditEvent` schema contains exactly `defeated_source_ref`, `zoneId`, `faction_id`, and `kill_weight_seed`; it excludes Combat runtime ids, XP values, threat, loot, corpse, and progression transaction fields (`production/sprints/sprint-1.md:413`, `production/sprints/sprint-1.md:417`).
- Valid kill credit plus `XpAwardResolutionSnapshot` awards XP once (`design/gdd/character-progression.md:727`).
- Duplicate kill credit dedupes by `XpAwardDedupeKey` (`design/gdd/character-progression.md:731`).
- Missing lookup/snapshot rejects XP without Combat fallback (`design/gdd/character-progression.md:755`).
- Same-frame Manual Save and Transition Save invoke `ProgressionSaveBarrier` and `NpcSourceLifecycleSaveBarrier`; any unresolved member fails the whole save and writes no bytes (`design/gdd/character-progression.md:775`, `design/gdd/save-load-persistence.md:515`, `docs/architecture/adr-0002-save-stability-barrier-protocol.md:197`).
- Player death payload is narrow and excludes `combat_actor_id`, account id, PvP source, server authority, raw threat table, corpse record, XP penalty, item drop, and LLM/narrative context (`design/gdd/combat-core.md:778`, `production/sprints/sprint-1.md:493`).

### Suite 9 - First-Save and Load/Hydration Boundaries

Stories: cross-system coverage for `T1-COMBAT-01`, `T1-COMBAT-09b`, and `T1-COMBAT-09c`.

Automated coverage:

- Save/Load hydrates and validates Character Progression before Combat actor hydration/build (`design/gdd/save-load-persistence.md:87`, `design/gdd/save-load-persistence.md:511`).
- `CombatProgressionBaselineSnapshot` contains allowed Combat fields and excludes `visible_level`, XP progress, `spell_eligibility_tier`, spell content, Combat current resources, threat, casts, targets, cooldowns, and runtime actor ids (`docs/architecture/adr-0003-progression-baseline-snapshot-contract.md:296`).
- First-save invokes `CharacterProgressionFirstSaveMaterializer` before bytes are written and writes no bytes on `FirstSaveMaterializationFailed` (`docs/architecture/adr-0004-first-save-materialization-and-character-identity.md:241`, `design/gdd/save-load-persistence.md:485`).
- Subsequent loads never re-run first-save materializers to repair missing state (`docs/architecture/adr-0004-first-save-materialization-and-character-identity.md:278`).

### Suite 10 - Profiled Combat-Feel Evidence

Stories: `T1-COMBAT-10`.

Automated/profiled coverage:

- Solo trash: 20 seeded trials, Cleric wins 55-85% and ends below either 80% health or 60% mana on mean result (`design/gdd/combat-core.md:800`).
- Named solo block: 10 seeded trials, Cleric loses or must flee at least 8/10 (`design/gdd/combat-core.md:804`).
- Two-trash overpull: 10 seeded trials, Cleric loses, flees, or survives below threshold at least 8/10 (`design/gdd/combat-core.md:808`, `production/sprints/sprint-1.md:564`).
- Med-break pacing: below 35% mana to 70% mana within 60-120 seconds after combat exit (`design/gdd/combat-core.md:812`, `production/sprints/sprint-1.md:566`).
- JSONL evidence must include engine version, fixture-set version, build SHA, scenario, completion state, pull counts, combat/downtime seconds, med breaks, tactical instant usage, unsafe pulls, and deaths (`production/sprints/sprint-1.md:532`, `production/sprints/sprint-1.md:560`).

Manual coverage:

- A short human feel read after automated profile runs checks whether Attack toggle, tactical instants, and med breaks still match the pinned-engine D012 direction (`production/prototypes/combat-feel-report.md:254`, `production/prototypes/combat-feel-report.md:262`).

### Suite 11 - Visual, HUD, and Audio Boundaries

Stories: `T1-COMBAT-08`, `T1-COMBAT-10`.

Automated/manual coverage:

- Pivot/no warning marker smoke (`design/gdd/combat-core.md:818`).
- No global combat visual state, post-process, or combat-owned visual treatment (`design/gdd/combat-core.md:822`).
- HUD output boundary: Combat exposes state, not final presentation (`design/gdd/combat-core.md:826`).
- Audio hooks only, no Combat-owned playback object (`design/gdd/combat-core.md:834`).

## Manual Test Cases

| ID | Scenario | Steps | Pass Evidence |
| --- | --- | --- | --- |
| M-01 | Solo trash baseline | Run `SoloTrash_EvenCon_T1` as Cleric with intended Attack toggle, Smite, Lesser Heal, med breaks. | JSONL run plus short note: win rate, health/mana ending state, med breaks, no unsafe pulls. |
| M-02 | Attack toggle clarity | Toggle Attack on/off, pull without toggling, target dies, sit, combat exits, die, zone transition. | Screenshot or dev HUD capture proving Attack state is explicit and current-state accessor matches events. |
| M-03 | Tactical instant feel | Use Smite of Authority, Bash, and Defensive Prayer or approved equivalents during trash and named runs. | JSONL counts plus note that instants feel intentional and do not erase med-break requirement. |
| M-04 | Named solo block | Run `NamedSoloBlock_T1` for 10 seeded trials. | JSONL summary: at least 8/10 losses or forced flees; any win logged as tuning defect unless tied to known exploit. |
| M-05 | Two-trash overpull | Run `TwoTrash_Overpull_T1` for 10 seeded trials. | JSONL summary: at least 8/10 loss/flee/below-threshold outcomes. |
| M-06 | Med-break recovery | Exit combat below 35% mana, sit, measure recovery to 70%. | Evidence line with elapsed seconds, regen ticks, and Attack forced off. |
| M-07 | Scope negative pass | Inspect delivered code/assets/config after T1-COMBAT-11. | Static scan report: no FishNet/networking/PvP/live LLM/companions/Warrior/Enchanter/server authority. |
| M-08 | Save race smoke | Trigger same-frame kill credit and Manual Save or Transition Save with barrier test doubles. | Event log showing barriers invoked before serialization, or `SaveFailedEvent(DownstreamSaveBarrierUnresolved)` with no bytes written. |

## Automated Coverage Expectations

| Story | Required automated coverage |
| --- | --- |
| T1-COMBAT-01 | Unit actor schema, fixture validation, ADR-0003 hydration success/failure. |
| T1-COMBAT-02 | Integration targeting, zone gates, pull, social assist, leash, deterministic cleanup. |
| T1-COMBAT-03 | Table-driven Attack toggle state machine and forced-off cases. |
| T1-COMBAT-04 | Unit formula tests, fixed-tick simulation, pause, out-of-range and same-tick priority integration. |
| T1-COMBAT-05 | Cast lifecycle integration, mana spend timing, cancel/interrupt, same-frame completion priority. |
| T1-COMBAT-06 | Tactical instant profile validation, no-hardcoded-tuning scan, declared-effect behavior. |
| T1-COMBAT-07 | Regen formula/unit tests and sit/combat-exit integration. |
| T1-COMBAT-08 | HUD-safe state snapshot/event tests, Attack ON signal table, no presentation ownership scan. |
| T1-COMBAT-09a | Death event one-shot, exact kill-credit schema, no Combat XP metadata. |
| T1-COMBAT-09b | Progression XP award/dedupe/rejection and Save/Load grouped barrier integration. |
| T1-COMBAT-09c | Player death payload schema, persistence whitelist, no corpse-run/XP-loss implementation. |
| T1-COMBAT-10 | Profile harness, JSONL schema validation, seeded scenarios, evidence summary generation. |
| T1-COMBAT-11 | Architecture forbidden-pattern scanner with at least one deliberate failing fixture/sample. |

## Evidence Requirements

Every story PR or handoff must include:

- Test command used.
- Passing test log path or copied terminal summary.
- Git commit/build SHA.
- Engine version.
- Fixture-set version for any combat/progression data.
- Acceptance criteria ids covered.
- File:line evidence for changed production code and changed tests.
- Explicit negative-scope scan result when the story touches Combat, Save/Load, Progression, NPC lifecycle, HUD, audio, or architecture boundaries.

Profiled evidence for T1-COMBAT-10 must write durable JSONL under `production/qa/combat/**` or `production/playtests/combat/**` and must preserve the Sprint 1 schema (`production/sprints/sprint-1.md:532`, `production/sprints/sprint-1.md:560`).

Sprint closeout requires all Definition of Done checks under `production/sprints/sprint-1.md:678` through `production/sprints/sprint-1.md:694`, including passing formula/state-machine tests, cross-system integration evidence, tactical instant fixture loading, unchanged `PlayerKillCreditEvent`, table-driven `H-CCOM-AA-03` and `H-CCOM-HUD-04`, JSONL profile evidence, forbidden-pattern scan, and negative scope checks.

## Open Blockers

1. **`/test-setup` has not run.**
   Evidence: Sprint 1 requires it before implementation (`production/sprints/sprint-1.md:35`); live `tests/` path is absent.
   Next action: run `/test-setup`.

2. **Per-story files referenced by `production/sprint-status.yaml` are absent.**
   Evidence: `production/sprint-status.yaml` references `production/stories/t1-combat-01-cleric-base-combat-actor-fixture-hydration.md` (`production/sprint-status.yaml:18`) and similar paths, while live `production/stories/` does not exist.
   Next action: before `/dev-story`, either confirm that `/dev-story` consumes `production/sprints/sprint-1.md` directly or create the referenced story files under a separate approved batch.

3. **Sprint metadata records stale draft HEAD.**
   Evidence: sprint plan and sprint status still record `2e268b4` (`production/sprints/sprint-1.md:9`, `production/sprint-status.yaml:11`), while live HEAD is `600f779` by git verification.
   Next action: update sprint metadata only if a protected sprint-state cleanup batch is explicitly approved.

4. **`production/session-state/active.md` is stale.**
   Evidence: it still points to `/sprint-plan new` (`production/session-state/active.md:8`, `production/session-state/active.md:101`).
   Next action: leave untouched for this QA plan; clean up later only under explicit approval.

## Next Actions

Run next:

```text
/test-setup
```

Then run:

```text
/dev-story T1-COMBAT-01-cleric-base-combat-actor-fixture-hydration
```

Only run the dev story after `/test-setup` produces a working local test command and either resolves or explicitly accepts the missing `production/stories/` handoff gap.
