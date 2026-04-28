# Sprint 1 -- 2026-04-28 to 2026-06-05

## Sprint Goal

Implement the minimum production T1 Combat Core slice that reproduces the D012-validated combat loop for offline single-player Cleric play: fixture-hydrated combat actors, target/claim/pull, explicit Attack toggle, weapon-delay melee, slow casts, fixture-driven tactical instants, med-break recovery, HUD-facing Attack state, kill-credit handoff to Character Progression Path A, and smoke/profiled evidence against the approved H-CCOM criteria.

## Source Baseline

- Current HEAD: `2e268b4` (`main`, matching `origin/main` when this plan was drafted).
- Combat Core D012 gates are satisfied: pinned-engine prototype validation plus Combat Core D012 amendment approval.
- Authoritative next-work pointer: `production/session-state/active.md`.
- Core design sources:
  - `DECISIONS.md` D012
  - `design/gdd/combat-core.md`
  - `design/gdd/reviews/combat-core-review-log.md`
  - `design/gdd/systems-index.md`
  - `production/prototypes/combat-feel-report.md`
  - `prototypes/combat-feel/Logs/playtest-20260426-204721.log`
  - `prototypes/combat-feel/Logs/playtest-20260426-205508.log`
  - `design/gdd/character-progression.md`
  - `docs/architecture/adr-0001-xp-source-lifecycle-registry.md`
  - `docs/architecture/adr-0002-save-stability-barrier-protocol.md`
  - `docs/architecture/adr-0003-progression-baseline-snapshot-contract.md`
  - `docs/registry/architecture.yaml`

## Capacity

- Total working days: 29
- Buffer (20%): 6 days reserved for unplanned integration, engine setup, fixture repair, and review fixes
- Available planned work: 23 days
- Estimated story work: 20.25 days

This is the first production implementation sprint for the project. Estimates are planning estimates, not velocity evidence. Re-estimate after `/qa-plan sprint` and `/test-setup` expose the actual Unity test scaffold cost.

## Required Pre-Implementation Gates

1. Run `/qa-plan sprint`.
2. Run `/test-setup`.
3. Run `/dev-story T1-COMBAT-01-cleric-base-combat-actor-fixture-hydration`.

The sprint plan is intentionally written before the QA plan so QA can derive test cases from the story order and AC trace. Implementation should not begin until the QA plan exists.

## Parallelization Point

After `T1-COMBAT-03`, stories `T1-COMBAT-04` and `T1-COMBAT-05` are parallelizable because melee resolution and casting resolution share the actor/Attack substrate but do not depend on each other. If Codex parallel work is used under D006, these are the first clean candidates for separate branches because their likely write sets can remain mostly disjoint:

- `T1-COMBAT-04`: `src/gameplay/combat/melee/**`, `src/gameplay/combat/simulation/**`, formula tests
- `T1-COMBAT-05`: `src/gameplay/combat/casting/**`, cast lifecycle tests

Codex assignments must still obey EDIT_OK, branch/worktree rules, and no edits to forbidden governance/design zones without explicit approval.

## Must Have (Critical Path)

| ID | Task | Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------|-----------|--------------|---------------------|
| T1-COMBAT-01 | Cleric base combat actor + fixture hydration | gameplay-programmer + systems-designer | 2.0 | `/qa-plan sprint`, `/test-setup` | H-CCOM-SCOPE-01, H-CCOM-ACTOR-01, H-CCOM-FIXTURE-01, H-CCOM-F2B, H-CCOM-SL-02; ADR-0003 |
| T1-COMBAT-02 | Targeting and hostile actor claim | gameplay-programmer + ai-programmer | 2.5 | T1-COMBAT-01 | H-CCOM-WS-01, H-CCOM-WS-02, H-CCOM-WS-03, H-CCOM-TGT-01, H-CCOM-PULL-01, H-CCOM-PULL-02, H-CCOM-PULL-03, H-CCOM-PULL-04, H-CCOM-LEASH-01, H-CCOM-LEASH-02, H-CCOM-ART-01 |
| T1-COMBAT-03 | Attack toggle state machine | gameplay-programmer | 1.0 | T1-COMBAT-01, T1-COMBAT-02 | H-CCOM-AA-01, H-CCOM-AA-03, H-CCOM-MED-01, H-CCOM-HUD-04 edge preconditions |
| T1-COMBAT-04 | Melee tick / weapon-delay resolution | gameplay-programmer | 1.5 | T1-COMBAT-03 | H-CCOM-TICK-01, H-CCOM-PAUSE-01, H-CCOM-AA-02, H-CCOM-F1, H-CCOM-F2 |
| T1-COMBAT-05 | Slow cast framework | gameplay-programmer | 1.5 | T1-COMBAT-03 | H-CCOM-CAST-01, H-CCOM-CAST-02, H-CCOM-CAST-03, H-CCOM-CAST-04, H-CCOM-CAST-05, H-CCOM-F4, H-CCOM-IF-01 |
| T1-COMBAT-06 | Tactical Cleric instants using fixture-loaded numeric values | gameplay-programmer + systems-designer | 1.5 | T1-COMBAT-05 | H-CCOM-INST-01, H-CCOM-FIXTURE-01 |
| T1-COMBAT-07 | Med/sit regen and combat-exit timing | gameplay-programmer | 1.5 | T1-COMBAT-03, T1-COMBAT-04, T1-COMBAT-05 | H-CCOM-MED-01, H-CCOM-MED-02, H-CCOM-MED-03, H-CCOM-F5, H-CCOM-FEEL-04 prereq |
| T1-COMBAT-08 | Attack ON HUD state signal hookup | gameplay-programmer + ui-programmer | 1.0 | T1-COMBAT-03 | H-CCOM-HUD-01, H-CCOM-HUD-02, H-CCOM-HUD-03, H-CCOM-HUD-04 |
| T1-COMBAT-09a | NPC death resolution + unchanged PlayerKillCreditEvent emission | gameplay-programmer | 1.0 | T1-COMBAT-04, T1-COMBAT-06 | H-CCOM-KILL-01; Character Progression Path A event payload preservation |
| T1-COMBAT-09b | Same-frame save barrier integration for kill-credit consistency | gameplay-programmer + engine-programmer | 2.0 | T1-COMBAT-09a, Character Progression lookup fixtures | H-CCOM-KILL-01 acknowledgement behavior; H-CPRO-XP-02, H-CPRO-XP-03, H-CPRO-XP-09, H-CPRO-XP-14, H-CPRO-SL-06, H-CPRO-CB-01; ADR-0001, ADR-0002 |
| T1-COMBAT-09c | Player death payload narrowing as stub-only reserved integration point | gameplay-programmer | 0.75 | T1-COMBAT-01, T1-COMBAT-04 | H-CCOM-DEATH-01, H-CCOM-DEATH-02, H-CCOM-DEATH-03, H-CCOM-SL-01, H-CCOM-SL-03 |
| T1-COMBAT-10 | Minimal smoke/profiled evidence loop for validated combat feel | gameplay-programmer + qa-tester | 2.0 | T1-COMBAT-01 through T1-COMBAT-09c | H-CCOM-FEEL-01, H-CCOM-FEEL-02, H-CCOM-FEEL-03, H-CCOM-FEEL-04, H-CCOM-ART-02, H-CCOM-AUD-01, H-CCOM-SCOPE-01 |
| T1-COMBAT-11 | Forbidden-pattern compliance scan/analyzer | gameplay-programmer + qa-tester | 1.0 | T1-COMBAT-09b | `docs/registry/architecture.yaml` forbidden patterns; H-CCOM-SCOPE-01; H-CPRO-CB-01; ADR-0001, ADR-0002, ADR-0003 |

## Story Details

### T1-COMBAT-01 - Cleric base combat actor + fixture hydration

Scope:
- Create the production combat domain scaffold.
- Define fixed combat clock abstractions.
- Define `CombatActorState` data shape.
- Implement fixture loading for Cleric, trash, named, spell, tactical instant, and encounter fixtures.
- Accept `CombatProgressionBaselineSnapshot` for player actor build/hydration.
- Keep all tunable values in data/config, not hardcoded production logic.

Likely files touched:
- `src/gameplay/combat/**`
- `assets/data/combat/**`
- `tests/unit/gameplay/combat/**`
- `tests/integration/gameplay/combat/**`

Acceptance criteria trace:
- H-CCOM-SCOPE-01
- H-CCOM-ACTOR-01
- H-CCOM-FIXTURE-01
- H-CCOM-F2B
- H-CCOM-SL-02
- ADR-0003 `CombatProgressionBaselineSnapshot`

Test plan:
- Unit tests for actor schema and fixture rows.
- Data-validation tests for fixture completeness, safe ranges, and missing required values.
- Hydration tests for valid baseline snapshot and invalid/missing snapshot fail-loud behavior.

Dependencies:
- `/qa-plan sprint`
- `/test-setup`
- Approved Combat Core and Character Progression ADR-0003 contract.

Done definition:
- Combat fixture package exists and validates.
- `Cleric_Mid_T1` resolves as level 5, 140 HP, 180 mana.
- No production Combat Core tuning value is hardcoded outside approved config/data.
- Tests pass and cite H-CCOM ids.

### T1-COMBAT-02 - Targeting and hostile actor claim

Scope:
- Implement HauntZone/CityHubZone combat gate.
- Implement target acquisition by radius and LoS.
- Implement NPC hostile claim/release boundary.
- Implement body/LoS pull, initial threat, social assist, bounded social pulses, deterministic query ordering, and path/leash hooks.
- Use fakes/test doubles where Creature / Enemy AI is not authored.

Likely files touched:
- `src/gameplay/combat/targeting/**`
- `src/gameplay/combat/threat/**`
- `src/gameplay/combat/pull/**`
- `src/gameplay/combat/leash/**`
- `tests/integration/gameplay/combat/**`

Acceptance criteria trace:
- H-CCOM-WS-01
- H-CCOM-WS-02
- H-CCOM-WS-03
- H-CCOM-TGT-01
- H-CCOM-PULL-01
- H-CCOM-PULL-02
- H-CCOM-PULL-03
- H-CCOM-PULL-04
- H-CCOM-LEASH-01
- H-CCOM-LEASH-02
- H-CCOM-ART-01

Test plan:
- Integration tests for valid/invalid target selection.
- CityHub combat-disable test.
- Body-pull test with no marker, bark, UI alert, or scripted trigger.
- Social-assist ordering and one-join-per-pull tests.
- LoS layer-mask and query-buffer overflow tests.
- Leash/re-aggro tests using path-probe fakes.

Dependencies:
- T1-COMBAT-01.

Done definition:
- Combat can claim only eligible haunt actors.
- CityHub hostile combat cannot start.
- Pulling initializes threat but never toggles Attack on.
- All integration tests are deterministic and clean up event subscriptions/test actors.

### T1-COMBAT-03 - Attack toggle state machine

Scope:
- Implement explicit player-controlled `Attack` ON/OFF state.
- Keep Attack separate from targeting, tab cycling, pulling, social assist, and spell casts.
- Implement all forced-off conditions.
- Preserve no-target no-op behavior.

Likely files touched:
- `src/gameplay/combat/attack/**`
- `src/gameplay/combat/state/**`
- `tests/integration/gameplay/combat/attack/**`

Acceptance criteria trace:
- H-CCOM-AA-01
- H-CCOM-AA-03
- H-CCOM-MED-01
- H-CCOM-HUD-04 edge preconditions

Test plan:
- Table-driven H-CCOM-AA-03 cases:
  - target selection does not enable Attack
  - tab cycling does not enable Attack
  - body pull does not enable Attack
  - spell pull does not enable Attack
  - no valid hostile target no-ops and leaves Attack off
  - target death forces Attack off
  - successful sit/med forces Attack off
  - combat exit forces Attack off
  - death forces Attack off
  - zone transition forces Attack off

Dependencies:
- T1-COMBAT-01
- T1-COMBAT-02

Done definition:
- Attack state changes only through approved command/transition paths.
- Forced-off conditions are centralized and table-tested.
- This story produces the implementation-time table-driven tests required by `production/session-state/active.md` and Combat Core review follow-up.

### T1-COMBAT-04 - Melee tick / weapon-delay resolution

Scope:
- Implement fixed-tick weapon timer.
- Implement melee hit chance and damage formulas.
- Validate target, range, facing tolerance, LoS, alive state, and zone gate on each eligible tick.
- Implement deterministic RNG injection and same-tick priority behavior.

Likely files touched:
- `src/gameplay/combat/melee/**`
- `src/gameplay/combat/simulation/**`
- `tests/unit/gameplay/combat/formulas/**`
- `tests/integration/gameplay/combat/melee/**`

Acceptance criteria trace:
- H-CCOM-TICK-01
- H-CCOM-PAUSE-01
- H-CCOM-AA-02
- H-CCOM-F1
- H-CCOM-F2

Test plan:
- Unit tests for hit chance formula example and clamps.
- Unit tests for melee damage formula example and minimum 1 damage.
- Integration tests for out-of-range swing skip.
- Fixed-tick test at `combat_tick_rate_hz = 50`.
- Pause test proving no wall-clock catch-up.
- Same-tick death-before-swing test.

Dependencies:
- T1-COMBAT-03.

Done definition:
- Melee ticks are fixed-step and frame-rate independent.
- Weapon timer does not queue multiple swings while out of range.
- Formula tests cover normal, boundary, and clamp cases.

Parallelization:
- Can run in parallel with T1-COMBAT-05 after T1-COMBAT-03.

### T1-COMBAT-05 - Slow cast framework

Scope:
- Implement cast request validation.
- Implement `Casting`, `Interrupted`, and `Recovery` transitions.
- Implement cast progress, completion, cancel, interrupt, recovery, and mana-spend timing.
- Emit Spell Memorization/HUD lifecycle events without owning spellbook slots.

Likely files touched:
- `src/gameplay/combat/casting/**`
- `src/gameplay/combat/events/**`
- `tests/unit/gameplay/combat/casting/**`
- `tests/integration/gameplay/combat/casting/**`

Acceptance criteria trace:
- H-CCOM-CAST-01
- H-CCOM-CAST-02
- H-CCOM-CAST-03
- H-CCOM-CAST-04
- H-CCOM-CAST-05
- H-CCOM-F4
- H-CCOM-IF-01

Test plan:
- Valid 6s cast enters `Casting`.
- Completion spends mana and enters recovery.
- Manual cancel emits event and spends no mana.
- Damage interrupt emits event and spends no mana.
- Zero/blocked damage does not roll interrupt.
- Same-tick completion resolves before new interrupt check.
- Lifecycle event payload tests include spell id, caster transient id, target transient id where applicable, and Combat Simulation Tick id.

Dependencies:
- T1-COMBAT-03.

Done definition:
- Cast lifecycle is event-driven and testable without UI.
- Combat Core does not own memorized slots or spellbook availability.
- Time-dependent behavior uses the combat simulation clock.

Parallelization:
- Can run in parallel with T1-COMBAT-04 after T1-COMBAT-03.

### T1-COMBAT-06 - Tactical Cleric instants using fixture-loaded numeric values

Scope:
- Implement zero-cast-time ability execution path.
- Support fixture/profile-loaded `SmiteOfAuthority_T1_Prototype`, `Bash_T1_Prototype`, and `DefensivePrayer_T1_Prototype` or Class Design-approved equivalents.
- Support declared effect types: direct damage, self-buff with authored duration, and `interrupt_current_channel`.
- Do not hardcode damage, mana cost, cooldown, duration, or scaling in Combat Core production logic.

Likely files touched:
- `src/gameplay/combat/abilities/**`
- `assets/data/combat/abilities/**`
- `tests/unit/gameplay/combat/abilities/**`
- `tests/integration/gameplay/combat/abilities/**`

Acceptance criteria trace:
- H-CCOM-INST-01
- H-CCOM-FIXTURE-01

Test plan:
- Instant ability resolves without a cast bar.
- Mana spend uses Rule 13 path.
- Cooldown starts as transient runtime timer.
- Bash cancels current channel only through declared effect profile.
- Self-buff duration comes from fixture data.
- Static/grep check rejects hardcoded tactical instant tuning in Combat Core code.

Dependencies:
- T1-COMBAT-05.

Done definition:
- Tactical instant values are loaded from fixture/profile data.
- Tests fail if ability profile data is missing required cost/cooldown/effect declarations.
- No action-combat rotation or manual melee spam is introduced.

### T1-COMBAT-07 - Med/sit regen and combat-exit timing

Scope:
- Implement sitting posture guards.
- Implement sitting out-of-combat mana boost.
- Implement sitting in combat as unsafe state with threat penalty and no med boost.
- Implement regen tick interval and combat-exit timer.

Likely files touched:
- `src/gameplay/combat/regen/**`
- `src/gameplay/combat/state/**`
- `src/gameplay/combat/threat/**`
- `tests/unit/gameplay/combat/regen/**`
- `tests/integration/gameplay/combat/regen/**`

Acceptance criteria trace:
- H-CCOM-MED-01
- H-CCOM-MED-02
- H-CCOM-MED-03
- H-CCOM-F5
- H-CCOM-FEEL-04 prereq

Test plan:
- `Cleric_Mid_T1` sitting out of combat gains 8 mana/tick.
- Mana never exceeds max.
- Sitting while on hostile threat table applies no med multiplier and adds `sitting_threat_bonus`.
- Combat exit formula returns true only after 30.1s and zero valid hostile entries.
- Attack is forced off before regen/threat updates on successful sit.

Dependencies:
- T1-COMBAT-03
- T1-COMBAT-04
- T1-COMBAT-05

Done definition:
- Med-break timing is fixed-tick and frame-rate independent.
- In-combat sit is dangerous, not a regen exploit.
- Tests establish the production path needed by later profiled med-break evidence.

### T1-COMBAT-08 - Attack ON HUD state signal hookup

Scope:
- Expose HUD-safe combat state snapshots/events.
- Include health, mana, target, cast, recovery, Attack ON/OFF, next swing readiness category, categorical threat, and combat state.
- Do not implement final HUD visual styling unless a separate Layer 1 HUD story explicitly owns it.

Likely files touched:
- `src/gameplay/combat/presentation/**`
- `src/gameplay/combat/events/**`
- `src/ui/hud/combat/**` only if a thin observer/view-model seam is required
- `tests/integration/gameplay/combat/hud/**`

Acceptance criteria trace:
- H-CCOM-HUD-01
- H-CCOM-HUD-02
- H-CCOM-HUD-03
- H-CCOM-HUD-04

Test plan:
- Threat state category tests cover all threshold cases.
- Table-driven H-CCOM-HUD-04 cases:
  - Attack on signal emits
  - Attack off signal emits
  - target death emits/reflects off
  - successful sit emits/reflects off
  - combat exit emits/reflects off
  - death emits/reflects off
  - zone transition emits/reflects off
  - current-state accessor matches event history
  - no misleading transient Attack ON pulse occurs when no-target no-op happens

Dependencies:
- T1-COMBAT-03.

Done definition:
- Gameplay code has no direct UI dependency.
- Combat Core exposes an explicit state surface, not a color/layout/animation choice.
- Raw numeric threat is not exposed as shipping HUD output.

### T1-COMBAT-09a - NPC death resolution + unchanged PlayerKillCreditEvent emission

Scope:
- Implement NPC lethal transition and one-shot actor death event.
- Implement Combat-owned `CombatKillResolutionPhase` coordinator on the Combat side.
- Emit unchanged `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)` when player contribution qualifies.
- Keep XP metadata out of Combat Core.

Likely files touched:
- `src/gameplay/combat/death/**`
- `src/gameplay/combat/events/**`
- `tests/integration/gameplay/combat/death/**`

Acceptance criteria trace:
- H-CCOM-KILL-01
- Character Progression Rule 3 and Rule 6 payload preservation
- ADR-0001 no Combat event amendment boundary

Test plan:
- NPC death emits `CombatActorDeathEvent` once.
- Qualifying player contribution emits `PlayerKillCreditEvent` once.
- Payload schema contains exactly `defeated_source_ref`, `zoneId`, `faction_id`, and `kill_weight_seed`.
- Payload does not contain `combat_actor_id`, defeated level, encounter role, XP value, spell data, progression transaction id, threat table, loot, or corpse record.

Dependencies:
- T1-COMBAT-04
- T1-COMBAT-06

Done definition:
- Combat side can produce the legal Path A event without any Character Progression mutation.
- Test proves the payload remains unchanged from the approved contract.

### T1-COMBAT-09b - Same-frame save barrier integration for kill-credit consistency

Scope:
- Integrate Combat kill-credit phase with Character Progression same-dispatch snapshot acknowledgement.
- Integrate grouped Save/Load barrier behavior for `ProgressionSaveBarrier` and `NpcSourceLifecycleSaveBarrier`.
- Preserve ADR-0001 source lifecycle token ownership and ADR-0002 no-bytes-written failure behavior.

Likely files touched:
- `src/gameplay/combat/death/**`
- `src/gameplay/progression/**`
- `src/gameplay/npc/**` or test doubles until NPC implementation exists
- `src/core/save/**`
- `assets/data/progression/**`
- `tests/integration/gameplay/progression/**`
- `tests/integration/core/save/**`

Acceptance criteria trace:
- H-CCOM-KILL-01 acknowledgement behavior
- H-CPRO-XP-02
- H-CPRO-XP-03
- H-CPRO-XP-09
- H-CPRO-XP-14
- H-CPRO-SL-06
- H-CPRO-CB-01
- ADR-0001
- ADR-0002

Test plan:
- Valid kill credit plus `XpAwardResolutionSnapshot` awards XP once.
- Duplicate kill credit dedupes by `XpAwardDedupeKey`.
- Missing lookup/snapshot rejects XP without Combat fallback.
- Same-frame Manual Save invokes `ProgressionSaveBarrier` and `NpcSourceLifecycleSaveBarrier`.
- One stable barrier plus one unresolved barrier fails whole save attempt.
- `SaveFailedEvent(DownstreamSaveBarrierUnresolved)` emits and no bytes are written.
- Static/API boundary scan proves Character Progression reads only the approved Combat event fields plus progression-owned registry metadata.

Dependencies:
- T1-COMBAT-09a
- Character Progression lookup fixture data
- Save/Load barrier seam or test double

Done definition:
- Save cannot serialize pre-award XP with post-award gameplay, or post-death source lifecycle with pre-award progression.
- Combat still does not carry XP metadata.
- Integration tests cover success, duplicate, missing snapshot, and unresolved barrier paths.

### T1-COMBAT-09c - Player death payload narrowing as stub-only reserved integration point

Scope:
- Implement Combat-owned player lethal transition and narrow `PlayerDeathEvent(death_payload)`.
- Keep Death & Corpse Recovery integration stub-only because Death & Corpse Recovery is not authored.
- Reserve the integration point without creating corpse-run penalty, XP loss, resurrection, corpse probe, or corpse recovery interaction.

Likely files touched:
- `src/gameplay/combat/death/**`
- `src/gameplay/combat/persistence/**`
- `tests/unit/gameplay/combat/death/**`
- `tests/integration/gameplay/combat/death/**`

Acceptance criteria trace:
- H-CCOM-DEATH-01
- H-CCOM-DEATH-02
- H-CCOM-DEATH-03
- H-CCOM-SL-01
- H-CCOM-SL-03

Test plan:
- Lethal transition clamps health to zero.
- `PlayerDeathEvent` emits exactly once with deterministic `death_context_id`.
- Payload contains only `death_context_id`, `local_character_id`, `zoneId`, `death_position`, `killer_source_ref`, and `death_cause_type`.
- Payload excludes `combat_actor_id`, account id, PvP source, server authority, raw threat table, corpse record, XP penalty, item drop, and LLM/narrative context.
- Persistence whitelist permits only current health, current mana, `combat_life_state`, and optional pending death handoff payload.

Dependencies:
- T1-COMBAT-01
- T1-COMBAT-04

Done definition:
- Combat death event is narrow and testable.
- Death & Corpse Recovery is not implemented or spoofed.
- Integration point is explicit and blocked for future D&CR work.

### T1-COMBAT-10 - Minimal smoke/profiled evidence loop for validated combat feel

Scope:
- Build seeded smoke/profile harness for production Combat Core.
- Run solo trash, named block, two-trash overpull, and med-break pacing scenarios.
- Emit durable JSONL evidence using the prototype-compatible schema plus production-specific fields.
- Produce a short markdown evidence summary suitable for PR and session handoff.

Likely files touched:
- `tests/performance/gameplay/combat/**`
- `tests/integration/gameplay/combat/**`
- `production/qa/combat/**`
- `production/playtests/combat/**`

Acceptance criteria trace:
- H-CCOM-FEEL-01
- H-CCOM-FEEL-02
- H-CCOM-FEEL-03
- H-CCOM-FEEL-04
- H-CCOM-ART-02
- H-CCOM-AUD-01
- H-CCOM-SCOPE-01

Required JSONL schema:

```json
{
  "timestamp": "2026-04-28T00:00:00-04:00",
  "engine_version": "6000.3.x",
  "fixture_set_version": "CombatPrototypeSpellProfileSet_T1@<revision>",
  "build_sha": "<git-sha>",
  "test_scenario": "SoloTrash_EvenCon_T1",
  "final_state": "Complete",
  "stopped_via": "completion",
  "pulls_completed": 5,
  "pulls_target": 5,
  "total_combat_seconds": 0.0,
  "total_downtime_seconds": 0.0,
  "avg_pull_seconds": 0.0,
  "med_breaks": 0,
  "auto_swings": 0,
  "hostile_swings": 0,
  "smites_channeled": 0,
  "heals_used": 0,
  "smite_of_authority_uses": 0,
  "bash_uses": 0,
  "defensive_prayer_uses": 0,
  "defensive_prayer_damage_prevented": 0,
  "unsafe_pulls": 0,
  "deaths": 0
}
```

Test plan:
- Profiled solo-trash run: 20 seeded trials, Cleric wins 55-85%.
- Named solo-block run: 10 seeded trials, Cleric loses or must flee at least 8/10.
- Two-trash overpull run: 10 seeded trials, Cleric loses, flees, or survives below threshold at least 8/10.
- Med-break pacing run: below 35% mana to 70% mana within 60-120 seconds after combat exit.
- Dev-build smoke verifies no Combat Core global combat visual state and no Combat-owned audio playback objects.

Dependencies:
- T1-COMBAT-01 through T1-COMBAT-09c.

Done definition:
- Evidence files include command, commit/build SHA, engine version, fixture set version, scenario, and result.
- JSONL remains comparable with `playtest-20260426-204721.log` and `playtest-20260426-205508.log`.
- Profiled evidence is durable enough for PR 4-question review.

### T1-COMBAT-11 - Forbidden-pattern compliance scan/analyzer

Scope:
- Add implementation-time compliance checks for architectural forbidden patterns from `docs/registry/architecture.yaml`.
- Start grep-based if fastest; promote to Roslyn analyzer only if grep cannot reliably catch the patterns in the current codebase.
- Include scope-creep negative checks from T1 and architectural boundary checks for Combat/Progression identity and snapshot contracts.

Likely files touched:
- `tests/architecture/**`
- `tools/architecture/**`
- `docs/registry/architecture.yaml` read-only source
- `production/qa/combat/**` evidence output

Acceptance criteria trace:
- `docs/registry/architecture.yaml` forbidden patterns
- H-CCOM-SCOPE-01
- H-CPRO-CB-01
- ADR-0001
- ADR-0002
- ADR-0003

Minimum forbidden-pattern coverage:
- `combat_actor_id` must not be used as XP identity or persistence identity.
- XP lookup must not read live NPC state after death.
- Character Progression must not demand expanded Combat kill-credit payload fields.
- Generic all-consumer progression baseline snapshot must not appear.
- `visible_level`, XP progress, or `spell_eligibility_tier` must not enter Combat hydration snapshot.
- Save barrier unresolved paths must not write bytes.
- FishNet/networking combat authority, PvP, companions, Warrior, Enchanter, and live LLM must not appear in production T1 combat code.

Test plan:
- Static scan test for banned identifiers and pattern pairs.
- DTO/schema scan for event and snapshot shapes.
- Failure fixture proving a deliberate forbidden-pattern sample is caught by the scanner.

Dependencies:
- T1-COMBAT-09b.

Done definition:
- Compliance check runs with the test suite or a clearly documented local gate command.
- Output names each forbidden pattern checked and whether it passed.
- Any failure blocks sprint closeout.

## Should Have

None. The sprint is intentionally all critical-path because it is the first production Combat Core implementation slice.

## Nice to Have

None. Any extra polish, final HUD styling, audio playback, or visual treatment belongs in later system-specific stories.

## Carryover from Previous Sprint

| Task | Reason | New Estimate |
|------|--------|--------------|
| None | This is the first sprint plan in the project. | N/A |

## Explicit Out Of Scope

- FishNet, networking placeholder, replicated combat authority, lag compensation, account identity, server validation, or multiplayer prediction.
- PvP, duels, faction PvP, friendly fire, or player-vs-player threat.
- Warrior, Enchanter, or future player-class implementation.
- Sister Elara combat behavior, full companion AI, hiring hall, companion relationships, or inverse-population scaling.
- Full spellbook, spell memorization slots, spell learning, spell unlocks, or final class spell list content.
- Final Class Design numeric balancing beyond fixture/profile data required for Combat Core tests.
- Complete Status Effects matrix, buff stacking, crowd-control durations, or dispel rules.
- Loot tables, item drops, item stat schema, currency economy, or equipment slot legality.
- XP curve redesign, level-up redesign, skill-up rules, or spell unlock progression beyond Character Progression Path A integration.
- Corpse-run penalty, XP loss, resurrection, corpse probe, or corpse recovery interaction beyond emitting/stubbing player death context.
- Zone Control ownership math or faction-control bridge validation.
- Live LLM, generated dialogue, moderation dependency, or dialogue memory.
- Production Layer 1 HUD visual styling beyond exposing Attack state and HUD-safe state signals.
- Ranged weapon model in T1.

## Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| First production code/test scaffold cost is underestimated | Medium | High | Run `/test-setup` before T1-COMBAT-01; keep T1-COMBAT-01 pure actor/fixture work after scaffold exists. |
| Kill-credit integration crosses too many systems | High | High | Split into T1-COMBAT-09a/09b/09c so Combat-only payload emission, save barriers, and player death stub are reviewable separately. |
| Tactical instant values get hardcoded from the prototype | Medium | High | T1-COMBAT-06 requires fixture/profile loading and tests that reject hardcoded tuning. |
| Implementer accidentally uses `combat_actor_id` for XP identity | Medium | High | T1-COMBAT-11 adds forbidden-pattern compliance scan/analyzer before sprint closeout. |
| Death & Corpse Recovery absence leads to fake implementation | Medium | Medium | T1-COMBAT-09c is stub-only and reserves the integration point without implementing D&CR behavior. |
| Smoke evidence drifts from prototype evidence schema | Medium | Medium | T1-COMBAT-10 reuses prototype JSONL fields and adds production-specific fields. |
| No QA plan exists yet | High | High | Run `/qa-plan sprint` before implementation begins. |
| Inventory remains parked and HUD dependency touches inventory surfaces | Low | Medium | Keep Combat HUD state narrow; do not implement inventory transaction UI or item/equipment schema. |

## Dependencies on External Factors

- Pinned Unity 6.3 LTS local installation for production test execution.
- Unity Test Framework setup from `/test-setup`.
- QA plan for sprint-level test case ownership.
- Character Progression lookup fixture data and ADR-0001/0002/0003 contracts.
- Existing Save/Load and NPC System interfaces or temporary test doubles that do not violate ownership boundaries.

## QA Plan Status

No QA plan was found under `production/qa/` when this sprint plan was drafted.

Run `/qa-plan sprint` before implementation begins. A sprint plan without a QA plan means test requirements are undefined from QA's perspective, and the sprint cannot pass a future Production to Polish gate without QA sign-off evidence.

## Definition of Done for this Sprint

- [ ] `/qa-plan sprint` completed and linked from this plan or sprint status.
- [ ] `/test-setup` completed and Unity Test Framework path verified.
- [ ] All Must Have stories completed or explicitly removed by approved sprint update.
- [ ] Every completed story cites its H-CCOM acceptance criteria and test evidence.
- [ ] All formula and state-machine stories have passing unit tests.
- [ ] All cross-system stories have passing integration tests or documented blocked status with owner.
- [ ] Tactical instant numeric values are loaded from fixture/profile data, not hardcoded in Combat Core.
- [ ] `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)` remains unchanged.
- [ ] H-CCOM-AA-03 and H-CCOM-HUD-04 are covered by table-driven tests.
- [ ] Smoke/profiled evidence loop produces prototype-compatible JSONL plus production fields.
- [ ] Forbidden-pattern compliance scan/analyzer passes.
- [ ] Negative scope checks find no FishNet, networking placeholder, PvP, live LLM, companions, Warrior, or Enchanter implementation in T1 combat code.
- [ ] No S1/S2 bugs remain open against delivered Combat Core scope.
- [ ] Design documents are updated only for approved deviations.
- [ ] Code is reviewed and merged through the approved workflow.

## Recommended First Dev Story

After `/qa-plan sprint` and `/test-setup`, run:

```text
/dev-story T1-COMBAT-01-cleric-base-combat-actor-fixture-hydration
```

This first dev story should be pure actor + fixture work. It should not absorb test framework setup, QA planning, targeting, Attack toggle, melee, casting, or kill-credit integration.

## Scope Check

If this sprint later adds stories beyond the Combat Core D012 implementation surface, run `/scope-check combat-core` before implementation begins on the added scope.
