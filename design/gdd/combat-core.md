# Combat Core

> **Status**: Designed (APPROVED after D012 combat-feel amendment re-review)
> **Author**: Codex (session with Brian, 2026-04-24)
> **Last Updated**: 2026-04-27
> **Last Verified**: 2026-04-27 - D012 amendment full design-review approved; prior full approval 2026-04-25
> **Implements Pillar**: Primary - **P2 The Silence Is Sacred**. Supports - **P5 Stakes Are Honest**, plus future-facing hooks for **P4 Every Companion Is A Person** and **P3 Reputation Is The Progression**.

## Locked Inputs

These inputs are authoritative for Combat Core. This GDD reproduces them rather than redesigning them. If a later section conflicts with this block, the later section is wrong.

1. **Combat hypothesis lock** - Combat Core is system #7, MVP priority, and the load-bearing prototype target. `systems-index.md:35` lists Combat Core as MVP and Not Started. `systems-index.md:177` marks it as "**THE core hypothesis. Prototype this earliest.**" `systems-index.md:234` identifies the risk: EQ-classic tab-target combat may not feel good to modern players in 2026.

2. **Strict EQ-classic fidelity where feasible in T1** - `game-concept.md:51` says the combat and pacing are "fully Classic EQ" and "Not 'inspired by.' Is." `game-concept.md:81` defines the combat core as auto-attack, spell memorization, hate management, med breaks, and mandatory group composition. `game-concept.md:121` names auto-attack ticking, hate management, 6-10 second spell cadence, med breaks, regen, and re-buffing.

3. **T1 tier discipline** - `DECISIONS.md:38-42` defers FishNet and forbids T1 networking placeholders. `DECISIONS.md:55-61` locks T1 to single-player offline local saves with no netcode, account system, server backend, or live LLM calls. `systems-index.md:87` scopes MVP to one class, one haunt, one faction, and offline play.

4. **T1 class scope** - Character Creation locks T1 to Cleric only. `character-creation.md:21` says T1 shows Cleric as the only visible/selectable class. `character-creation.md:70` says the only visible and selectable class id is `Cleric`. Combat Core therefore supports a Cleric player in T1 and defines extensible combat-actor contracts for later Warrior, Enchanter, and companions without implementing those classes here.

5. **World Structure boundary** - `world-structure.md:50` defines `HauntZone` as the danger space where combat AI is live and `CityHubZone` as safe space where no combat AI spawns. `world-structure.md:68` says World Structure fires `ZoneTransitionBeginEvent` and Combat Core owns in-flight effect resolution. `world-structure.md:98` says Combat Core consumes zone events and owns the combat-enable gate. `world-structure.md:615` expects `PlayerDeathEvent` to move World Structure to `CorpseRunActive`.

6. **NPC System boundary** - `npc-system.md:53` says NPC System owns placement and inhabitant identity while combat systems own hostility, hate, damage, and ability behavior. `npc-system.md:82` says Combat Core and Creature / Enemy AI own target selection, hate, damage, death, crowd control, and hostile state machines. `npc-system.md:403` verifies that when Combat Core claims a combat actor, NPC System stops owning combat decisions.

7. **Save/Load boundary** - `save-load-persistence.md:130` leaves Combat Core's direct persistence surface to this GDD. Combat state may persist only as explicit gameplay state, never as engine internals, runtime handles, or cached/derived values per `save-load-persistence.md:63` and `save-load-persistence.md:361-362`.

8. **Art/UI/audio combat posture** - `art-bible.md:113` forbids combat-state post-process, desaturation, red vignette, and global lighting shifts. `art-bible.md:116-117` says the player watches hate list/spell queue rather than spectacle, and the combat signal is the pivot. `art-bible.md:1125-1126` defines Layer 1 HUD health, mana, hate, and spell queue as peripheral practical UI. `art-bible.md:1314` flags mana-restore fill as dependent on med-break mechanics, resolved here.

9. **D012 pinned combat-feel validation** - D012 (`DECISIONS.md:339`) locks the pinned-engine combat-feel result and requires this Combat Core amendment before `/sprint-plan new`. The underlying evidence is the combat-feel report's pinned validation section (`production/prototypes/combat-feel-report.md:196`) and JSONL playtest records: `prototypes/combat-feel/Logs/playtest-20260426-204721.log:1` (`engine_version = 6000.3.14f1`, `5/5` pulls, `24.507s` average pull, `5` med breaks) and `prototypes/combat-feel/Logs/playtest-20260426-205508.log:1` (`engine_version = 6000.3.14f1`, `5/5` pulls, `18.734s` average pull, `5` med breaks). Verification method: JSONL parsed with PowerShell `ConvertFrom-Json`; all six README success criteria passed after the Attack ON highlight fix.

## Second-Revision Scope

The 2026-04-25 second revision resolves Combat Core's local contract blockers for T1 implementation: runtime-vs-stable identity, death handoff payload semantics, combat-owned pause behavior, social assist data contracts, threat edge cases, fixture coherence, and pull/LoS/leash testability. It does not edit Menus & Settings or Day/Night Cycle; those GDDs still need a coordinated follow-up if their open pause-semantics rows are to be closed on disk.

## D012 Amendment Scope

The 2026-04-27 D012 amendment incorporates pinned combat-feel validation into Combat Core without reopening the approved death, kill-credit, save, threat, or pull architecture. It makes three implicit/prototype-validated contracts explicit: Attack toggle does not auto-enable on pull, T1 Cleric tactical instants are a first-class ability surface owned by the spell-profile contract, and Layer 1 HUD must receive enough Attack state to render an explicit Attack ON visual state. `PlayerKillCreditEvent` remains unchanged.

## Summary

Combat Core defines the T1 offline tab-target combat loop for a single Cleric player in one haunt zone. It owns combat actor state, target acquisition, auto-attack, melee/spell range checks, slow casting framework, mana spend, interruption, recovery, health and mana resources, hate/threat tables, combat entry and exit, med-break regeneration, kill/death events, and the data outputs consumed by downstream HUD, Spell Memorization, Class Design, Creature / Enemy AI, Death & Corpse Recovery, Zone Control, Character Progression, Inventory, Audio, and future companion systems.

T1 reproduces strict EQ-classic combat feel where feasible: player-controlled toggle auto-attack, weapon-delay ticks, slow spell casts, tactical Cleric instants, meaningful interruptions, explicit hate, careful body/line-of-sight pulls, sitting to recover mana, and trash encounters that a careful Cleric can solo. It does not implement PvP, networking, raids, future classes, live companions, full spellbook memorization, loot economy, XP curves, corpse-run penalties, or named/boss soloability. Combat Core validates the combat-feel half of the MVP hypothesis; Zone Control must still validate the "kills shift faction control" bridge alongside the combat prototype.

## Overview

Combat Core is the gameplay layer that answers "what happens once an actor is fighting?" It sits between World Structure's zone lifecycle, NPC System's inhabitant identity, and the downstream systems that make combat meaningful over time. World Structure tells Combat Core when the active zone is a `HauntZone` or `CityHubZone`; NPC System provides combat-eligible actor seeds; Combat Core claims eligible actors into combat, runs the target/auto-attack/cast/hate/resource loop, emits death and kill events, and exposes practical state for Layer 1 HUD.

The design intentionally tests strict EQ-classic fidelity rather than a modernized derivative. T1 supports one playable class (`Cleric`) and no party classes. To keep the vertical slice playable without erasing group dependency, T1 tuning allows a Cleric to solo carefully selected trash pulls, but not named enemies, camps, or multi-pull encounters. Group dependency remains a foundation contract for T2 when Warrior and Enchanter arrive; T1 proves that the combat machinery, timing, pulls, and recovery cadence can carry attention on their own.

The system's scope is behavioral, not content-complete. Combat Core defines the shared combat-actor interface, state machine, formulas, events, and acceptance gates. Class Design later defines Cleric spell lists and future Warrior/Enchanter roles. Spell Memorization later defines spellbook slots and memorized spell management. Status Effects later defines buffs/debuffs and crowd control. Creature / Enemy AI later defines hostile decision behavior. Death & Corpse Recovery later defines XP loss, corpse interaction, and recovery penalties. Combat Core owns only the combat substrate these systems attach to.

## Player Fantasy

The player fantasy is careful competence under slow pressure. Combat should feel like committing to a pull, not reacting to a fireworks display. A target pivots, auto-attack begins its measured tick, a spell takes long enough to make damage matter, mana drains into a real future cost, and the decision to sit after the fight is part of the fight's consequence. The silence between pulls is not downtime; it is the space where the player reads the room, reviews mana, watches patrols, and decides whether greed is about to become a corpse run.

The Cleric is not a solo power fantasy. In T1, the Cleric survives trash by discipline: one enemy at a time, range respected, casts protected, med breaks accepted. Named enemies and camps expose the absence of a group. That absence is useful information, not a design failure. It teaches the player that the combat system was built for dependency even before the full party roster exists.

### Anchor moments

- **The first careful body pull.** The player edges close enough for one haunt inhabitant to pivot without pulling the room. No alert marker appears. The mesh rotation and closing movement are the signal; if the player chooses `Attack`, the measured auto-attack tick begins only from that explicit toggle.
- **The interrupted heal.** A slow Cleric cast is nearly complete when a hit lands inside the interruption window. The cast drops, mana is not refunded beyond the specified rule, and the player learns to create space or time spells between enemy swings.
- **The med break that matters.** After a narrow trash fight, the player sits near a practical light source, mana rises faster, auto-attack is disabled, and the next pull waits because the player chooses patience.
- **The overpull.** Two enemies enter the hate table. The Cleric can delay death but not erase the mistake. The lesson is legible: the pull was wrong, not the math hidden.

### Anti-fantasy

- Combat should not feel like a modern action rotation, spammy cooldown puzzle, or animation-cancel skill check.
- The Cleric should not solo named enemies, full camps, or multiple same-level enemies as a normal expectation in T1.
- Threat should not be a visible numeric minigame. Combat Core uses explicit internal values, but the player reads threat through target behavior and categorical HUD state.
- The game should not warn the player with red screens, alert stingers, aggro outlines, danger arrows, nameplate color changes, or global post-process shifts.
- Sister Elara, companions, Warrior tanking, Enchanter control, PvP, raids, and networking should not be smuggled into this GDD.

## Detailed Design

### Core Rules

1. **T1 combat zone gate.** Combat Core enables hostile combat only when World Structure reports the active zone as `HauntZone`. In `CityHubZone`, hostile combat AI, auto-attack against NPCs, kill-credit emission, and Zone Control kill-weight events are disabled. Debug/test harnesses may simulate combat in controlled scenes, but shipped T1 gameplay honors the gate.

2. **Combat actor contract.** Any participant in combat implements or supplies a data-only combat actor record:

   ```yaml
   CombatActorState:
     combat_actor_id: transient runtime/session id; never persisted or used as save identity
     actor_kind: Player | NPC | EnvironmentalCombatSource
     stable_source_ref:
       local_character_id: required for Player actors
       source_npc_id: required for persistent NPC actors claimed from NPC System
       source_spawn_ref: required for non-persistent creature/ambient spawns
       source_hazard_id: required for environmental combat sources
     faction_id: optional faction identity for downstream systems
     zone_id: active zone id
     level: int
     max_health: int
     current_health: int
     max_mana: int
     current_mana: int
     armor_class: int
     attack_power: int
     weapon_delay_seconds: float
     melee_range_meters: float
     spell_range_meters: float
     combat_state: enum
     target_combat_actor_id: optional transient combat_actor_id
     combat_sort_key: stable test/order key derived from stable_source_ref + authored spawn/anchor id
     threat_table: transient table keyed by combat_actor_id
   ```

   Combat Core owns the runtime combat state and the interface. `combat_actor_id` is legal only inside the active runtime/session and may appear in transient combat events where the receiver resolves it immediately. It must not be serialized, persisted, or used as a death/corpse-run identity. Stable handoff identity uses `stable_source_ref`: player death handoff uses the player's `local_character_id` plus the killer's stable source reference; NPC deaths and kill credit use `source_npc_id` when the actor came from an `NpcRecord`, otherwise `source_spawn_ref` (`spawn_table_id`, `spawn_anchor_id`, `npc_archetype_id`) or `source_hazard_id` for environmental sources. NPC System owns `npcId`, identity, schedule, spawn, and non-combat state. Future Class Design and later companion systems consume this interface rather than redefining combat participation.

3. **Player target selection.** The player may target one combat-eligible actor at a time using tab-target selection or direct click/selection. Targeting does not start combat by itself. Targets are valid only when alive, enabled by the active `HauntZone`, within `target_acquire_radius_meters`, and line-of-sight valid according to the T1 LoS query. Invalid targets clear or fail selection silently with no UI error beyond HUD target loss.

4. **Attack toggle / auto-attack state.** Auto-attack is an explicit player-controlled `Attack` on/off state, separate from targeting and pulling. Target selection, tab cycling, body-pull aggro, hostile social assist, and spell casts never turn `Attack` on by themselves. A toggle-on request with no valid hostile target silently no-ops and leaves `Attack` off. When enabled against a valid hostile target in melee range, the player swings on `weapon_delay_seconds` ticks until toggled off, target invalidates, the player successfully sits/meditates, casts a spell that forbids melee overlap, dies, leaves combat, or crosses a zone transition boundary. Auto-attack does not queue multiple swings while out of range.

5. **Melee tick resolution.** On each eligible weapon tick, Combat Core validates target, range, facing tolerance, line-of-sight, actor alive state, and combat zone gate. If valid, it computes hit/miss and damage through the melee formulas. If invalid, the swing is skipped without resetting the weapon timer unless the invalid condition is target death, auto-attack off, or combat exit.

6. **T1 range scope.** T1 supports melee and spell range only. There is no bow, thrown weapon, wand auto-shot, ammunition, ranged weapon delay, ranged ammo economy, or future-class ranged attack in Combat Core. Future Class Design may add ranged capability through this actor interface in T2+.

7. **Pull model.** Combat starts through explicit pulling: body proximity, line-of-sight body-pull, or hostile actor social-link response. Pulling initializes threat and hostile intent, but does not enable the player's `Attack` toggle; the player must choose `Attack` separately if they want melee auto-swings. The body-pull predicate is `hostile faction targetable AND player within aggro_radius_meters AND unobstructed LoS from hostile aggro origin to player target point`. Facing is not required for the initial body-pull unless Creature / Enemy AI later authors a narrower sentry-cone profile; once initial aggro is acquired, breaking LoS by backing around a corner does not erase threat.

   T1 LoS and pull queries use these implementation contracts:

   - `hostile_aggro_origin`: authored `aggro_eye_anchor` if present; otherwise the hostile actor capsule center plus 80% capsule height.
   - `player_target_point`: authored `player_los_anchor` if present; otherwise the player capsule center plus 70% capsule height.
   - `los_occluder_layer_mask_t1`: `WorldSolid`, `ClosedDoor`, and `LargeProp`. `CombatActor`, `TriggerOnly`, `InteractableSoft`, and VFX layers do not block LoS.
   - Target acquisition and aggro-radius scans use non-allocating physics queries with `combat_query_buffer_size = 64`. Non-alloc query result order is not trusted; Combat Core sorts returned hits before use by `distance_millimeters_from_query_origin`, then `combat_sort_key`, then authored collider index. A full buffer is a development-build validation failure for the fixture and logs `CombatQueryBufferOverflow`; shipping behavior uses the returned buffer after the same deterministic sort and truncates without allocating. Overflow is still a content/test defect because unseen colliders cannot be recovered deterministically.
   - Social-link assist runs once at aggro acquisition and then on `social_assist_pulse_seconds = 2.0` pulses while the primary hostile remains in `Pulling` or `InCombat`. T1 default `social_assist_radius_meters = 12`. Assist data lives in a `SocialAssistProfile`: `social_link_group_id`, optional `encounter_group_id`, `assist_enabled`, `assist_radius_meters`, `assist_threat_initial`, `assist_requires_los_to_primary`, `assist_requires_los_to_target`, `assist_faction_filter`, `assist_encounter_filter`, and authored `assist_order_index`. T1 defaults are `assist_threat_initial = 25`, `assist_requires_los_to_primary = true`, `assist_requires_los_to_target = true`, `assist_faction_filter = SameFactionOrExplicitAlly`, and `assist_encounter_filter = SameEncounterOrSharedSocialGroup`.
   - A linked hostile assists only if all predicates pass in this order: alive/combat-eligible, not already on the same threat table, `assist_enabled`, shared `social_link_group_id`, faction filter, encounter filter, distance from primary hostile <= `assist_radius_meters`, LoS from candidate aggro origin to primary hostile aggro origin when required, and LoS from candidate aggro origin to `player_target_point` when required. Eligible assisters are processed deterministically by distance from primary hostile, then `assist_order_index`, then `combat_sort_key`. Each assister receives exactly `assist_threat_initial` threat toward the player unless a later explicit profile overrides the numeric value.

   T1 does not use scripted encounter starts, warning volumes, alert icons, or proximity barks to announce the pull.

8. **Aggro source types.** Threat can be generated by proximity aggro, melee damage, spell damage, healing, buffs, debuffs, sitting aggro, and future taunt-like hooks. T1 implements the numeric threat table now even when some source types have no authored T1 ability yet, so downstream Cleric spells and future Warrior/Enchanter roles attach to the same model.

9. **Threat is internal, not numeric UI.** Threat values are explicit numbers for implementation and testing. Combat Core exposes neutral categorical HUD-facing enums only; Layer 1 HUD owns presentation language and must not display raw threat values in shipping player UI. Dev-build diagnostics may show raw threat values behind debug tooling only.

   Threat categories use a ratio model against the current highest valid threat entry on each hostile table. Invalid, dead, out-of-zone, negative, or zero-value entries are ignored for target selection and category math; negative threat in hydrated/test data is invalid and fails validation rather than clamping silently.

   - `NoThreat`: the actor has no valid entry, or the table's top threat is 0.
   - `ThreatListed`: the actor has a valid non-top entry and `entry_threat / top_threat < threat_close_ratio`.
   - `ThreatClose`: the actor has a valid non-top entry and `entry_threat / top_threat >= threat_close_ratio`.
   - `HasAggroStable`: the actor is the hostile's current target and `second_highest_valid_threat / top_threat < aggro_contested_ratio`. If only one valid positive entry exists, `second_highest_valid_threat` is defined as 0 for this ratio and the current target is stable.
   - `HasAggroContested`: the actor is the hostile's current target and `second_highest_valid_threat / top_threat >= aggro_contested_ratio`.

   T1 defaults: `threat_close_ratio = 0.85`, `aggro_contested_ratio = 0.90`, and `threat_entry_cap = 100000`. HUD presentation may label `HasAggroContested` in player-facing language later; Combat Core exposes only the enum.

10. **Target-of-target behavior.** Each hostile actor attacks the highest-threat valid target on its threat table. Ties resolve by earliest threat entry timestamp, then lowest `combat_sort_key` for deterministic tests. If the top target becomes invalid, the actor retargets to the next valid positive entry. If the table has exactly one valid positive entry, that entry is both top target and uncontested aggro. If no valid positive target remains, the hostile actor leashes or exits combat per Rule 18. This is the future "tank holds aggro" foundation: a tank keeps control by remaining highest accumulated threat, while a healer can overtake through effective healing or unsafe sitting. T1 has no tank class, so the Cleric player is normally the top valid target unless a test actor is present.

11. **Casting and tactical instant framework.** Combat Core owns spell/ability execution state, not spellbook content. A cast or ability request includes `caster_combat_actor_id`, `spell_id`, optional `target_combat_actor_id`, `cast_time_seconds`, `mana_cost`, `spell_range_meters`, `interrupt_profile_id`, `recovery_seconds`, `cooldown_seconds` or `cooldown_profile_id`, and effect declarations. These ids are transient runtime lookup ids only. `cast_time_seconds = 0` is an instant ability; non-zero values enter the normal cast bar/channel state. Combat Core validates actor state, target state, range, line-of-sight, mana, cooldown availability, and combat locks; enters `Casting` when cast time is non-zero; resolves success, interrupt, cancel, or fizzled validation failure; spends mana according to Rule 13; starts cooldowns as transient runtime timers; and emits lifecycle events for Spell Memorization and HUD.

   Tactical instant effect declarations are contract shape only. Numeric damage, cooldown, mana-cost, duration, and scaling values live in fixture data owned by Class Design / Spell Memorization. T1 effect types include hostile direct damage, self-buff with authored duration, and `interrupt_current_channel` for abilities such as Bash. `interrupt_current_channel` cancels the target's current channel if the target has one and the ability profile passes range/validity checks; interrupt duration or recovery pressure is fixture data, not a Combat Core constant.

12. **Slow spell cadence.** T1 default channeled cast times should live primarily in the 3-8 second band, with rare faster utility exceptions and later 6-10 second class-defining casts allowed by Class Design. Global recovery and cooldowns prevent spell spam even when individual spells are fast. D012 authorizes a narrow T1 tactical instant surface for Cleric agency, using `cast_time_seconds = 0` profiles with cooldown and mana-cost fields; this does not authorize an action-combat rotation or spammed manual melee.

13. **Mana spend timing.** The default T1 rule is mana commits when the cast successfully completes. If a cast is interrupted, mana is not spent. If the player cancels manually before completion, mana is not spent. If a cast passes completion but the target becomes invalid at the final resolution frame, spell-specific behavior is deferred to Class Design / Spell Memorization; Combat Core emits `CastResolvedInvalidTarget`.

14. **Interrupt model.** Eligible damage received during `Casting` rolls an interrupt check. Eligible damage means post-mitigation `damage_taken > 0`; absorbed, blocked, or zero-damage hits do not roll the damage interrupt formula. Movement, sitting, zone transition, death, and hard control effects interrupt automatically. The interrupt formula uses damage pressure, remaining cast fraction, and actor interrupt resistance. T1 should make long Cleric casts meaningfully vulnerable without making casting impossible against one trash enemy.

15. **Recovery state.** After a successful cast or interrupted cast, the caster enters `Recovery` for `recovery_seconds`. During recovery, new casts are blocked, auto-attack may resume only if the spell permits melee overlap, and movement is allowed unless the spell profile says otherwise. T1 uses recovery to maintain slow spell cadence until Spell Memorization owns spell-slot behavior.

16. **Resource model.** Combat Core owns current health/mana, runtime resource validation, regeneration, spend, clamp/reject behavior, and all combat formula use of actor level. ADR-0003 names Character Progression as the owner of `CombatProgressionBaselineSnapshot`, which provides `combat_actor_level = current_level`, `permanent_max_health`, and `permanent_max_mana` for the player actor. Class Design later owns class-specific content tables. Combat Core provides formulas and default T1 prototype baselines sufficient for prototype fixtures until hydrated progression baselines are available.

17. **T1 Cleric soloability envelope.** A same-level Cleric with baseline T1 equipment can defeat one even-con trash enemy when starting above 80% health and 60% mana, using appropriate casting and med breaks. The same Cleric should usually lose or be forced to flee against two even-con trash enemies, one named enemy, or a sustained camp without recovery. Encounter content enforces this envelope through enemy stats, social-link placement, and explicit encounter-role metadata:

   - `encounter_role = Trash` means a non-named hostile actor with no rare/boss/camp-anchor flag, no required social-link assist under the intended pull, and a level delta of -1 to +1 against the baseline T1 Cleric fixture.
   - `encounter_role = Named` means a named or rare hostile actor with `solo_block_profile_id` present. A named solo block must use at least two mechanical blockers from: health/mana wall, incoming DPS above sustainable Cleric healing, interrupt pressure, social-link/assist pressure, leash/space pressure, or authored ability pressure. "Designer intent says not soloable" is not sufficient.
   - `encounter_role = Camp` means two or more linked spawn anchors, a pathing cluster that reliably produces multi-pulls, or sustained respawn/patrol pressure that denies full med-break recovery between pulls.

   These roles are implementation/test metadata, not player-facing labels.

18. **Leash and combat exit.** Combat does not end the instant no swing occurs. An actor remains in combat until all valid hostile threat entries are gone or invalid and `combat_exit_timer_seconds` has elapsed without hostile action. Combat Core owns leash state and threat cleanup; Creature / Enemy AI owns NavMeshAgent movement execution and path probes. Creature / Enemy AI publishes `PathProbeResult(combat_actor_id, path_status, path_pending, sampled_tick)` at `path_status_sample_seconds = 0.25` or slower. Combat Core treats `PathPartial` / `PathInvalid` as failure only after continuous failure for `path_failure_grace_seconds = 1.0`; `path_pending = true` is neutral until it exceeds `path_pending_grace_seconds = 1.0`, after which it counts as failure. Hostile actors that exceed `leash_distance_meters` from their anchor, or remain in path failure beyond grace, enter `Leashing`. While leashing, the hostile stops new attacks and casts, clears active attack intent, asks Creature / Enemy AI to return toward its anchor, and retains limited threat memory for `leash_threat_memory_seconds = 30` or until combat exit, whichever resolves first.

   Re-aggro is allowed only while threat memory is active, the target re-enters `leash_reaggro_distance_meters = 20`, LoS is valid, and the hostile has not reached its anchor. Once the hostile reaches anchor or threat memory expires, the table clears and future aggro starts as a new pull. T1 uses explicit timer state rather than implicit "no aggro equals out of combat."

19. **Meditation / sitting.** The player can sit only while alive, grounded, not casting, not in recovery, not moving, and not in `ZoneLoading` commit lock. Sitting disables auto-attack and increases mana regeneration significantly when out of combat. Sitting during active combat is allowed only as an unsafe state: it increases incoming threat and does not grant the out-of-combat mana boost until combat exits.

20. **Regeneration model.** In-combat health and mana regeneration are zero or near-zero in T1. Standing out-of-combat regeneration is slow. Sitting out-of-combat regeneration is the med-break state and is significantly faster, especially for mana. This rule resolves the art-bible dependency that mana-restore fill needs med-break mechanics (`art-bible.md:1314`).

21. **Death event ownership and payload.** Combat Core owns the transition from `current_health > 0` to `current_health <= 0`. When the player reaches zero health, Combat Core clamps health to 0, stops auto-attack and casting, clears hostile action queues, creates a `death_context_id`, emits `PlayerDeathEvent(death_payload)`, and stops player combat input.

   `PlayerDeathEvent.death_payload` is:

   ```yaml
   PlayerDeathPayload:
     death_context_id: opaque local id for this death episode
     local_character_id: stable T1 player save identity
     zoneId: stable World Structure zone id where lethal damage resolved
     death_position: world-space Vector3 at lethal resolution
     killer_source_ref: stable source ref for NPC/spawn/environment; never combat_actor_id
     death_cause_type: NPC | EnvironmentalCombat | UnknownCombatSource
   ```

   `death_context_id` is created by Combat Core exactly once at the lethal transition, before event emission, using a locally unique ID provider. Tests must inject a deterministic provider so `PlayerDeathEvent` and corpse-run handoff are fixture-verifiable. The id is a correlation key, not actor identity: it exists to dedupe "death emitted once" handling across Combat Core, World Structure, Save/Load, and later Death & Corpse Recovery. It persists only while `combat_life_state = DeadPendingCorpseRunHandoff` or while downstream corpse-run state still needs to correlate this death episode; after World Structure has entered `CorpseRunActive` and Death & Corpse Recovery owns the corpse-run state, Combat Core drops it. World Structure consumes the payload in the same Combat Simulation Tick, transitions to `CorpseRunActive`, and uses `death_context_id` to reject duplicate death events for the same episode while creating the normal `CorpseRecord` (`zoneId`, position, expiry timestamp) that World Structure already owns. For T1, `killer_source_ref` identifies an NPC stable `source_npc_id`, stable spawn reference, or environmental source only; it never names a runtime combat actor id, player, account, server identity, or PvP source. Death & Corpse Recovery owns XP loss, corpse probe, resurrection, and recovery penalties later.

22. **NPC death and kill event ownership.** When an NPC combat actor reaches zero health, Combat Core owns a same-tick `CombatKillResolutionPhase` for that defeated source. It emits `CombatActorDeathEvent(combat_actor_id, defeated_source_ref, zoneId)` for immediate runtime subscribers and, if the player contributed qualifying threat or damage, `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)` exactly once. `combat_actor_id` is transient and cannot be persisted; `defeated_source_ref` is `source_npc_id` for persistent NPC records or `source_spawn_ref` for non-persistent creature/ambient spawns. Combat Core does not add XP metadata or inspect progression state, but it must hold source cleanup/despawn and respawn-token rotation until NPC System has recorded the source lifecycle outcome and Character Progression has either captured or rejected its same-dispatch award snapshot. Missing same-tick acknowledgements log `KillResolutionPhaseAckMissing`, keep the source in a kill-resolution hold, and surface through the ADR-0002 save-barrier failure path if Save/Load requests serialization before the phase resolves. Character Progression consumes XP-relevant data, Inventory consumes loot hooks later, Zone Control consumes kill-weight data, and NPC System receives release/death outcome for identity persistence.

23. **Zone transition cleanup.** On `ZoneTransitionBeginEvent`, Combat Core cancels player casting, disables auto-attack, clears transient projectiles/hit windows owned by Combat Core, and resolves hostile actors according to active zone rules. No damage-over-time, projectile, cast, or melee tick may cross from outgoing zone into incoming zone unless a future GDD explicitly defines cross-zone persistence. T1 default is strip/cancel at transition boundary.

24. **Save participation.** Combat Core persists only explicit current gameplay values that must survive a save/load to preserve honest stakes and fit Save/Load's state categories: player `current_health`, `current_mana`, `combat_life_state`, and optional pending death handoff payload. `current_health`, `current_mana`, and `combat_life_state` are Player State. The optional death handoff payload is Player/World bridge state that exists only while `combat_life_state = DeadPendingCorpseRunHandoff`; once World Structure / Death & Corpse Recovery owns corpse-run state, persistence flows through World Structure's `CorpseRecord`, not Combat Core. `combat_life_state` is a closed enum: `Alive`, `DeadPendingCorpseRunHandoff`, or `CorpseRunOwnedByDeathSystem`. The pending death handoff payload may contain only `death_context_id`, `local_character_id`, `zoneId`, `death_position`, `killer_source_ref`, and `death_cause_type`.

   Combat Core does not persist threat tables, cast progress, recovery timers, swing timers, transient hit rolls, target selection, `combat_actor_id`, runtime actor handles, animation state, projectiles, cooldown timers, regen rates, or cached formulas. Memorized spell slots belong to Spell Memorization persistence, not Combat Core. Regen rates are formula-derived from hydrated current/max resources and tuning data. Loading a save resumes out of active combat unless the pending death handoff or Death & Corpse Recovery says the player is in corpse-run state.

25. **Pause/menu interaction.** T1 Combat Core owns a hard-stop local combat pause for Combat-owned simulation only. Menus & Settings may request that pause and owns focus/input presentation; Combat Core owns what happens to combat state after the request. While combat-paused, Combat-owned timers and dispatch stop: combat simulation ticks, cast timers, recovery timers, weapon ticks, regen ticks, leash timers, assist pulses, threat expiry, hostile intent resolution, and combat event emission. On resume, casts and timers continue from the paused combat tick, not from real-world elapsed time. Blocking save/load/zone-error modals always prevent new player combat input even if they do not enter the named `PauseMenu` state.

   Pause behavior by surface:

   - **Combat timers/cooldowns/casts/regen/AI pulses/leash**: frozen by Combat Core; no catch-up on resume.
   - **NavMesh movement execution**: Combat Core stops issuing combat movement intents while paused. Creature / Enemy AI must pause or hold NavMeshAgent combat pursuit/return execution for actors under Combat claim; it resumes from the same pathing state and does not advance attacks while paused.
   - **Animation**: Combat Core exposes paused combat state. Animation implementation should hold combat state-machine progression for attack, cast, hit-react, pivot, death, and med-break transitions tied to Combat Core events; ambient non-combat animation remains outside Combat Core.
   - **Audio**: Combat Core emits no new combat audio hooks while paused. Already-playing audio is owned by Audio System; Combat Core requires no `AudioSource` or mixer control.
   - **Day/Night/world clock**: Combat Core does not decide whether global world time pauses. This GDD closes only the combat-simulation contract. Menus & Settings and Day/Night currently keep global pause semantics open and must be amended separately if a single project-wide pause policy is required.

26. **PvP forbidden in T1.** Combat Core has no player-vs-player damage, friendly-fire, duel flag, faction PvP, replicated hit validation, or PvP threat model in T1. Any PvP work requires a later tier decision and GDD update.

27. **No companion behavior in T1 Combat Core.** Sister Elara, Named AI Companion Core, companion hire/dismiss, tanking behavior, healing behavior, relationship state, and companion combat AI are not authored here. Combat Core provides the generic actor contract those later systems must implement.

28. **Deterministic testability.** Combat calculations use deterministic inputs and injectable random seeds in tests. T1 does not require deterministic network replay, but unit and integration tests must be able to reproduce hit/miss, damage, interrupt, and threat outcomes.

29. **Combat simulation tick.** T1 Combat Core resolves all combat state transitions on a fixed deterministic Combat Simulation Tick at `combat_tick_rate_hz = 50` (`combat_tick_seconds = 0.02`), matching Unity's default fixed timestep. Unity physics, input, and animation observations are sampled into combat tick inputs; rendering observes already-resolved combat state. "Same frame" in this GDD means the same Combat Simulation Tick for combat logic. If the project later changes Unity `Time.fixedDeltaTime`, Combat Core must either match it or record an ADR explaining the divergence.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `CombatDisabled` | Active zone is not a `HauntZone`; actor not hydrated; actor dead; session not playable | `ZoneActiveEvent` for `HauntZone` and actor ready -> `OutOfCombat` | No hostile combat, auto-attack, casts, threat, or kill credit. |
| `OutOfCombat` | Actor alive and combat-enabled with no active hostile threat | Pull, hostile action, or valid auto-attack -> `InCombat`; sit command -> `SittingOutOfCombat` | Standing regen applies. Targeting allowed. `Attack` does not pre-queue without a valid hostile target. |
| `SittingOutOfCombat` | Player sits while alive and out of combat | Stand/move/cast/pull/damage -> `OutOfCombat` or `InCombat` | Med-break mana regen applies. `Attack` is forced off on successful sit. |
| `Pulling` | Actor violates hostile proximity/LoS/social-link rule | Threat table initialized -> `InCombat`; invalid pull -> `OutOfCombat` | Initial aggro source recorded. Pivot/hostile claim begins through Creature AI. |
| `InCombat` | Threat table has at least one valid hostile entry or recent hostile action | No hostile entries/action for `combat_exit_timer_seconds` -> `ExitingCombat`; player death -> `Dead`; cast request -> `Casting` | Auto-attack ticks, threat updates, damage, and hostile retargeting run. In-combat regen minimal/zero. |
| `Casting` | Valid cast request begins | Cast completes -> `Recovery`; interrupt/cancel -> `Interrupted`; death -> `Dead`; zone transition -> `TransitionCleanup` | Cast timer runs. Damage may trigger interrupt. Movement/sit/transition interrupts. |
| `Interrupted` | Cast interrupted by damage, movement, transition, death, or hard control | Interrupt recovery elapsed -> previous legal state | Emits `CastInterruptedEvent`; mana spend follows Rule 13. |
| `Recovery` | Cast completed or interrupt recovery begins | Recovery elapsed -> `InCombat` or `OutOfCombat` based on threat | Blocks new casts. Auto-attack overlap follows spell profile. |
| `Leashing` | Hostile actor exceeds leash limit or loses valid path within allowed bounds | Anchor reached and exit timer elapsed -> `OutOfCombat`; valid target re-enters leash band -> `InCombat` | Hostile returns toward anchor. Threat memory retained only until combat exit. |
| `ExitingCombat` | No valid hostile entries/action but combat timer still active | Timer elapsed -> `OutOfCombat`; hostile action resumes -> `InCombat` | Prevents instant regen/med exploit after brief line-of-sight breaks. |
| `TransitionCleanup` | `ZoneTransitionBeginEvent` received while actor has combat state | Cleanup complete -> `CombatDisabled` or `OutOfCombat` in incoming zone | Cancels casts, auto-attack, transient hit windows, and threat as needed. |
| `Dead` | `current_health <= 0` | Death & Corpse Recovery later restores/revives actor | Player input blocked. `PlayerDeathEvent` or actor death event emitted once. |

### Transition priority

If multiple combat events occur in the same frame, resolve in this order:

1. Zone unload / `ZoneTransitionBeginEvent`.
2. Death from already-applied damage.
3. Save/load hard failure or session disable.
4. Cast completion at exact timer boundary.
5. Interrupt checks from damage received this frame.
6. Melee auto-attack tick.
7. Threat retargeting.
8. Combat exit timer update.
9. Player input requests.

This order prevents post-death casts, cross-zone hits, and swing/cast double-resolution ambiguity.

### Interactions with Other Systems

| System | Combat Core Consumes | Combat Core Provides | Ownership Boundary | Dependency |
|---|---|---|---|---|
| **World Structure** | `ZoneActiveEvent(zoneId, zoneType)`, `ZoneTransitionBeginEvent(outgoingZoneId, incomingZoneId)`, active zone type | `PlayerDeathEvent`; combat-enable/disable diagnostics | WS owns zone lifecycle and corpse-run state. Combat owns combat gate and death event emission. | **Hard upstream** |
| **NPC System** | Combat-eligible actor seeds, `npcId`, faction/social flags, spawn/active state | Combat claim/release, actor death outcome, hostile targetability state | NPC owns identity, schedule, spawn, and non-combat state. Combat owns hostility, hate, damage, death, and combat actor state. | **Hard upstream/downstream boundary** |
| **Save / Load & Persistence** | Hydrated player current health/mana if present; load/session state | Explicit combat-persisted values only: player current health, current mana, alive/dead flag, death handoff state if required | Save/Load owns serialization/integrity. Combat owns what combat values are real state. | **Hard** |
| **Character Creation** | `starting_class_id = Cleric`, starting equipment template id as later materialized by Inventory | No direct output | Character Creation seeds class identity; Combat does not validate creator UI. | **Seed consumer** |
| **Character Progression** | ADR-0003 `CombatProgressionBaselineSnapshot` with `combat_actor_level`, `permanent_max_health`, and `permanent_max_mana` for player actor hydration/build | XP/kill-credit event context, death context | Progression owns XP curves, levels, permanent max-resource baselines, spell eligibility, and XP-source lookup. Combat owns runtime current resources, combat formulas, damage, threat, casting, regen, and death. | **Hard downstream/same-tier boundary** |
| **Class Design** | Class-authored base stats, weapon delays, spell profiles, role rules once authored | Combat actor interface, cast framework, auto-attack and threat hooks | Class Design owns Cleric/Warrior/Enchanter content. Combat owns shared mechanics. | **Future hard downstream** |
| **Spell Memorization** | Memorized spell availability once authored | Cast request/resolve/interrupt/recovery event framework | Spell Memorization owns spellbook slots and memorized spell management. Combat owns cast execution state. | **Future hard downstream** |
| **Status Effects & Buffs** | Buff/debuff modifiers once authored | Damage, interrupt, threat, regen, and state hooks | Status Effects owns effect definitions and stacking. Combat owns base hooks and application timing. | **Future hard downstream** |
| **Creature / Enemy AI** | Hostile decision requests, path validity, attack intent | Threat table, current target, combat actor state, damage results | Creature AI owns enemy behavior choices. Combat owns resolution and state. | **Future hard downstream** |
| **Death & Corpse Recovery** | Revive/recovery outcome later | `PlayerDeathEvent(death_payload)` with death position, stable killer source ref, and death context | Death system owns corpse, XP loss, resurrection, recovery. Combat owns death detection/event. | **Future hard downstream** |
| **Zone Control** | None in T1 core loop | `PlayerKillCreditEvent` with stable defeated source ref, zone/faction/kill-weight seed | Zone Control owns ownership math. Combat owns kill event emission. | **Future hard downstream** |
| **Inventory & Item Economy** | Equipped weapon/armor stats once authored | Loot eligibility hook and kill context | Inventory owns items, gear legality, drops. Combat owns combat resolution. | **Future hard downstream** |
| **Layer 1 HUD** | None | Health, mana, target, cast, recovery, auto-attack, and categorical hate state | HUD owns visual presentation. Combat owns state values. | **Future hard downstream** |
| **Audio System** | None | Combat event hooks: swing, hit, miss, cast start, interrupt, death, med start/stop | Audio owns playback and mix. Combat owns event timing. | **Soft downstream** |
| **Named AI Companion Core / Sister Elara Mentor** | No T1 companion behavior | Generic combat actor interface and future companion actor kind | Companion systems own companion AI, role, relationship, and onboarding behavior. | **T2+/future downstream** |

## Formulas

The `melee_hit_chance` formula is defined as:

`melee_hit_chance = clamp(base_hit_chance + ((attacker_level - defender_level) * level_hit_delta) + ((attacker_attack_skill - defender_defense_skill) * skill_hit_delta), hit_chance_min, hit_chance_max)`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| `base_hit_chance` | `B` | float | 0.50-0.90; default 0.72 | Equal-level baseline chance for a normal melee swing to hit. |
| `attacker_level` | `AL` | int | 1-60 future; T1 fixture 1-10 | Attacker class/progression level. |
| `defender_level` | `DL` | int | 1-60 future; T1 fixture 1-10 | Defender level. |
| `level_hit_delta` | `LD` | float | 0.01-0.05; default 0.03 | Hit chance shift per level difference. |
| `attacker_attack_skill` | `AS` | int | 0-300 future; T1 fixture 1-60 | Attacker weapon/combat skill, source later owned by Class/Progression. |
| `defender_defense_skill` | `DS` | int | 0-300 future; T1 fixture 1-60 | Defender avoidance/defense skill. |
| `skill_hit_delta` | `SD` | float | 0.0005-0.003; default 0.001 | Hit chance shift per skill difference. |
| `hit_chance_min` | `Min` | float | 0.05-0.25; default 0.10 | Minimum non-special melee hit chance. |
| `hit_chance_max` | `Max` | float | 0.75-0.98; default 0.92 | Maximum non-special melee hit chance. |

**Output Range:** 0.10 to 0.92 with defaults under normal play; never below `hit_chance_min` or above `hit_chance_max`.
**Example:** Equal-level actors with equal skills: `clamp(0.72 + 0 + 0, 0.10, 0.92) = 0.72`.

The `melee_damage` formula is defined as:

`melee_damage = max(1, floor(((weapon_base_damage + attack_power * attack_power_scalar) * damage_roll_scalar) - (defender_armor_class * armor_mitigation_scalar)))`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| `weapon_base_damage` | `W` | int | 1-200 future; T1 fixture 1-20 | Base weapon or natural attack damage. |
| `attack_power` | `AP` | int | 0-500 future; T1 fixture 0-80 | Attacker offensive stat after class/item modifiers. |
| `attack_power_scalar` | `APS` | float | 0.05-0.50; default 0.20 | Converts attack power to damage. |
| `damage_roll_scalar` | `R` | float | 0.70-1.30; default sampled 0.85-1.15 | Deterministic seeded roll for hit variation. |
| `defender_armor_class` | `AC` | int | 0-500 future; T1 fixture 0-80 | Defender mitigation stat. |
| `armor_mitigation_scalar` | `AMS` | float | 0.02-0.30; default 0.10 | Converts armor class to flat mitigation. |

**Output Range:** Minimum 1 on a successful non-special hit; normal T1 trash range target 2-20 damage per player/enemy hit before class/item tuning.
**Example:** `W=8`, `AP=20`, `APS=0.20`, `R=1.0`, `AC=30`, `AMS=0.10` gives `max(1, floor((8 + 4) - 3)) = 9`.
**Fixture requirement:** T1 damage validation must exercise both level-1 and level-10 stat packages. Extreme armor or low weapon inputs may clamp to 1 damage on successful hits, but the approved T1 trash fixture must stay inside the 2-20 normal-hit target band unless Class Design later changes the baseline.

The `threat_delta` formula is defined as:

`threat_delta = damage_done * damage_threat_scalar + healing_done * healing_threat_scalar + buff_value * buff_threat_scalar + proximity_threat + sitting_threat_bonus + taunt_threat_future`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| `damage_done` | `D` | int | 0-9999 | Damage applied by actor to hostile target. |
| `damage_threat_scalar` | `DTS` | float | 0.5-2.0; default 1.0 | Threat per damage. |
| `healing_done` | `H` | int | 0-9999 | Effective healing, excluding overheal, applied to a target on a hostile threat table. |
| `healing_threat_scalar` | `HTS` | float | 0.25-2.0; default 0.75 | Threat per effective healing distributed to hostile actors. |
| `buff_value` | `B` | int | 0-9999 | Class/Status-defined estimate for useful buff/debuff actions. |
| `buff_threat_scalar` | `BTS` | float | 0.1-2.0; default 0.50 | Threat per buff value. |
| `proximity_threat` | `P` | int | 0-500; default initial 25 | Threat applied by body/LoS pull. |
| `sitting_threat_bonus` | `S` | int | 0-1000; default 50 when sitting on hostile table | Threat penalty for sitting while on a hostile table. |
| `taunt_threat_future` | `T` | int | 0-9999; default 0 at T1 | Future Warrior/AI hook; not authored as a player ability in T1. |

**Output Range:** 0 to `threat_entry_cap` after accumulation clamps. T1 expected event deltas are 0-500. Threat table stores accumulated values per hostile actor as non-negative integers. `proximity_threat` is one-shot at initial body/LoS/social aggro. `sitting_threat_bonus` is one-shot when the actor enters sitting while already on that hostile table; standing and sitting again can apply it again. Healing threat is applied to hostile actors whose threat tables include the healed target; overheal does not generate threat unless a later Status Effects or Class Design rule explicitly says otherwise.
**Example:** A Cleric heals 20 effective HP while sitting after proximity aggro: `0 + 20*0.75 + 0 + 25 + 50 + 0 = 90`.

The `interrupt_chance` formula is defined as:

`interrupt_chance = clamp(base_interrupt_chance + (damage_taken / max_health * damage_interrupt_scalar) + ((cast_time_remaining_seconds / cast_time_total_seconds) * early_cast_interrupt_scalar) - interrupt_resistance, interrupt_chance_min, interrupt_chance_max)`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| `base_interrupt_chance` | `B` | float | 0.05-0.50; default 0.20 | Baseline chance that a damaging hit interrupts a cast. |
| `damage_taken` | `D` | int | 0-current health | Damage from the hit being evaluated. |
| `max_health` | `HPmax` | int | 1-99999 | Caster max health. |
| `damage_interrupt_scalar` | `DIS` | float | 1.0-8.0; default 4.0 | Converts hit size as percent max HP into interrupt chance. |
| `cast_time_remaining_seconds` | `R` | float | 0-cast total | Remaining cast time when hit lands. |
| `cast_time_total_seconds` | `T` | float | 0.1-10.0 T1 | Total cast duration. |
| `early_cast_interrupt_scalar` | `EIS` | float | 0.0-0.30; default 0.10 | Makes earlier cast segments slightly more vulnerable. |
| `interrupt_resistance` | `IR` | float | 0.0-0.75; default 0.0 T1 | Future stat/buff hook reducing interrupts. |
| `interrupt_chance_min` | `Min` | float | 0.0-0.25; default 0.05 | Minimum chance on eligible damaging hit. |
| `interrupt_chance_max` | `Max` | float | 0.50-1.0; default 0.85 | Maximum chance on eligible damaging hit. |

**Output Range:** 0.05 to 0.85 with defaults.
**Example:** A caster with 100 max HP takes 10 damage with 2s remaining on a 4s cast: `clamp(0.20 + (10/100*4.0) + (2/4*0.10) - 0, 0.05, 0.85) = 0.65`.

The `regen_tick` formula is defined as:

`resource_regen_per_tick = floor(floor(base_regen + level * level_regen_scalar + max_resource * percent_regen_scalar) * posture_multiplier * combat_multiplier)`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| `base_regen` | `B` | int | 0-20; default HP 1, mana 1 | Flat resource returned per tick. |
| `level` | `L` | int | 1-60 future; T1 fixture 1-10 | Actor level. |
| `level_regen_scalar` | `LS` | float | 0.0-1.0; default 0.10 | Level contribution. |
| `max_resource` | `MR` | int | 1-99999 | Max health or mana. |
| `percent_regen_scalar` | `PS` | float | 0.0-0.05; default HP 0.005, mana 0.005 | Percent max resource contribution. |
| `posture_multiplier` | `PM` | float | 1.0 standing, default 4.0 sitting/medding for mana, 1.5 sitting for health | Posture-based med-break multiplier. |
| `combat_multiplier` | `CM` | float | 0.0-1.0; default 0.0 in combat for mana, 0.25 in combat for health | Combat-state regen suppression. |

**Output Range:** 0 to resource cap. T1 med-break mana should refill from empty to useful pull-readiness in roughly 60-120 seconds depending on max mana and tuning. Approved out-of-combat mana-med fixtures must produce at least 1 mana per tick unless current mana is already capped; in-combat mana regen may be 0 by design.
**Example:** `Cleric_Mid_T1` is level 5 with 180 max mana. Sitting out of combat: `floor(1 + 5*0.1 + 180*0.005) * 4.0 * 1.0 = floor(2.4) * 4 = 8 mana/tick`.

The `combat_exit_timer` formula is defined as:

`can_exit_combat = seconds_since_last_hostile_action >= combat_exit_timer_seconds AND valid_hostile_threat_entries == 0`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| `seconds_since_last_hostile_action` | `S` | float | >=0 | Time since damage, hostile cast, threat-producing action, or hostile target update. |
| `combat_exit_timer_seconds` | `T` | float | 10-60; default 30 | Time required before out-of-combat state. |
| `valid_hostile_threat_entries` | `N` | int | 0-encounter cap | Current valid hostile actors on threat table. |

**Output Range:** boolean.
**Example:** If no valid hostiles remain and 30.1 seconds have elapsed with default `T=30`, combat exits. If one hostile entry remains, combat does not exit.

## T1 Prototype Fixtures

These fixtures unblock Combat Core tests before Class Design, Level Design, Encounter Design, Spell Memorization, and Creature / Enemy AI are authored. They are implementation/test data, not final balance. When the T1 haunt level band is locked, these fixture ids must remap to the lowest, middle, and top levels of that band. Until then, `PrototypeHauntBand_T1 = 1-10` and uses levels 1, 5, and 10.

### Actor fixtures

| Fixture | Level | Max HP | Max Mana | AC | Attack Power | Weapon Base Damage | Attack Skill | Defense Skill | Weapon Delay |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| `Cleric_Low_T1` | 1 | 80 | 100 | 20 | 10 | 4 | 10 | 10 | 2.8s |
| `Cleric_Mid_T1` | 5 | 140 | 180 | 35 | 25 | 8 | 30 | 30 | 2.8s |
| `Cleric_Top_T1` | 10 | 220 | 300 | 55 | 45 | 12 | 60 | 60 | 2.8s |
| `Trash_Low_T1` | 1 | 70 | 0 | 15 | 12 | 5 | 10 | 10 | 3.0s |
| `Trash_Mid_T1` | 5 | 120 | 0 | 30 | 25 | 8 | 30 | 30 | 3.0s |
| `Trash_Top_T1` | 10 | 200 | 0 | 50 | 45 | 12 | 60 | 60 | 3.0s |
| `Named_Top_T1` | 10 | 520 | 0 | 65 | 60 | 16 | 70 | 70 | 2.6s |

### Spell profile fixtures

Combat Core owns this temporary fixture set only for combat-feel prototyping. Class Design and Spell Memorization must replace it when authored.

| Spell Fixture | Cast Time | Mana Cost Low/Mid/Top | Effect Low/Mid/Top | Recovery | Notes |
|---|---:|---:|---:|---:|---|
| `Smite_T1_Prototype` | 6.0s | 18 / 30 / 48 | 24 / 48 / 82 damage | 1.5s | Hostile spell; eligible for interruption. |
| `LesserHeal_T1_Prototype` | 6.0s | 20 / 34 / 52 | 34 / 68 / 110 healing | 1.5s | Self-heal fixture; effective healing generates threat if hostile tables include the healed actor. |

D012 tactical instant fixtures use the same profile contract. They are prototype-derived contract fixtures, not final Class Design spell-list commitments.

| Tactical Instant Fixture | Cast-Time Contract | Cost/Cooldown Ownership | Effect Contract | Notes |
|---|---|---|---|---|
| `SmiteOfAuthority_T1_Prototype` | `cast_time_seconds = 0` | Fixture data | Hostile direct damage | Aggressive filler button; final name/value may change in Class Design. |
| `Bash_T1_Prototype` | `cast_time_seconds = 0` | Fixture data | Melee-range direct damage plus `interrupt_current_channel` | Interrupt value/duration is fixture data; Combat Core owns channel-cancel effect resolution. |
| `DefensivePrayer_T1_Prototype` | `cast_time_seconds = 0` | Fixture data | Self-buff with authored duration | Defensive preservation button; final name/value may change in Class Design. |

### Encounter fixtures

| Fixture | Composition | T1 `kill_weight_seed` | Source-ref aliases available to downstream fixtures | Required Outcome |
|---|---|---:|---|---|
| `SoloTrash_EvenCon_T1` | Cleric fixture vs. same-level Trash fixture | `1.25` | `source_spawn_ref:Trash_Early_L2_T1`, `source_spawn_ref:SoloTrash_EvenCon_T1`, `source_spawn_ref:Trash_Level7_T1`, `source_spawn_ref:Trash_Late_L9_T1`, `source_spawn_ref:SoloTrash_SoftUndercon_T1`, `source_spawn_ref:SoloTrash_Trivial_T1`, `source_spawn_ref:DenseCamp_Trash_T1` | Cleric wins 90-100% of clean-state seeded trials from >80% HP and >60% mana, with mean ending pressure still below either 80% HP or 60% mana. |
| `TwoTrash_Overpull_T1` | Cleric fixture vs. two same-level Trash fixtures entering hate within 5s | `1.25` per defeated trash source | `source_spawn_ref:TwoTrash_A_T1`, `source_spawn_ref:TwoTrash_B_T1` | Normal two-trash farming is not viable. |
| `NamedSoloBlock_T1` | `Cleric_Top_T1` vs. `Named_Top_T1` | `1.25` | `source_npc_id:Named_XP_Smoke_T1` | Named has `solo_block_profile_id` with at least health/mana wall plus interrupt pressure. Any baseline solo kill is a tuning defect unless marked exploit-under-investigation. |

## Edge Cases

- **If the player targets a valid NPC in `CityHubZone` and presses auto-attack**: auto-attack does not start; no threat table is created; no damage event fires. T1 city combat remains disabled.
- **If the player presses `Attack` with no valid hostile target**: the toggle-on request no-ops, `Attack` remains off, and no auto-swing timer is queued.
- **If auto-attack is on and the target steps out of melee range before the weapon tick**: the tick validates range, deals no damage, and the next tick waits for the normal weapon delay.
- **If a target dies on the same frame as the player's swing tick**: death resolves before the swing by transition priority; the swing is discarded, `Attack` is forced off, and no overkill second death event fires.
- **If the player successfully sits/meditates while `Attack` is on**: the sit transition forces `Attack` off before med-break regen begins; no melee ticks occur while seated.
- **If the player tab-cycles targets while `Attack` is on**: `Attack` remains on, but the next swing tick validates the new target, range, LoS, hostility, and zone gate before dealing damage. If the new target is invalid, the tick is skipped or the toggle is forced off per invalid-target rules.
- **If two actors reach lethal health in the same frame**: resolve player death first for `PlayerDeathEvent`, then NPC deaths for kill/death events; tests must assert single event emission per actor.
- **If a cast completes on the same frame as incoming damage**: cast completion resolves before the new interrupt check. Damage may still apply after the spell resolves.
- **If damage interrupts a cast**: emit `CastInterruptedEvent`, enter `Interrupted`, do not spend mana under default T1 mana-spend rule, and apply interrupt recovery.
- **If a hit applies 0 post-mitigation damage during a cast**: do not roll the damage interrupt formula. Non-damage interrupt sources such as movement, sitting, transition, death, or hard control still interrupt automatically.
- **If the player manually cancels a cast**: emit `CastCancelledEvent`, do not spend mana, and apply the configured cancel recovery if any.
- **If the player begins sitting while on any hostile threat table**: sitting applies no med-break mana multiplier and adds `sitting_threat_bonus` to hostile threat.
- **If the player sits out of combat and is body-pulled**: med-break stops immediately, combat begins, sitting threat applies, and the player must stand before auto-attacking or casting if the cast profile forbids sitting.
- **If a target/aggro query fills `combat_query_buffer_size`**: development builds log `CombatQueryBufferOverflow` and the fixture fails validation; shipping builds sort returned hits by distance, `combat_sort_key`, and authored collider index, then use that bounded set without allocation. Overflow remains a content defect because non-returned colliders cannot be recovered.
- **If a hostile actor loses path to the player**: Creature / Enemy AI reports `PathProbeResult`; Combat Core starts leash/exit handling only after path pending/partial/invalid grace windows expire and does not teleport the hostile actor to the player.
- **If a leashing hostile reaches anchor or `leash_threat_memory_seconds` expires**: clear the hostile threat table, clear active target, stop attack/cast intent, and treat any later aggro as a new pull.
- **If the player re-enters range before leash memory expires**: resume combat only if the player is within `leash_reaggro_distance_meters`, LoS is valid, and the hostile has not reached anchor.
- **If Combat Core is paused during a cast, swing, regen tick, assist pulse, leash timer, combat pursuit, combat animation event, or combat audio-hook window**: all Combat-owned timers and event dispatch remain at their current tick; on resume they continue from that tick without applying wall-clock elapsed time.
- **If `ZoneTransitionBeginEvent` fires during a cast or swing windup**: Combat Core cancels the cast/swing before any completion event can affect the incoming zone.
- **If `ZoneTransitionBeginEvent` and `PlayerDeathEvent` are possible in the same frame**: transition cleanup resolves first. If lethal damage was already applied before transition begin, death resolves in the outgoing zone; otherwise no cross-zone death is synthesized.
- **If Save/Load hydrates `current_health <= 0` without a Death & Corpse Recovery state**: Combat Core treats this as invalid combat hydration and returns a hydration failure to Save/Load. A dead player must be represented through the proper corpse-run/death state contract.
- **If loaded health/mana exceed max values from current Class/Progression data**: apply Combat-owned hydration clamp/reject behavior against the ADR-0003 `CombatProgressionBaselineSnapshot` maxima and log a migration warning when clamping is the selected policy. If the Combat-scoped progression baseline or max data is missing, fail hydration rather than inventing values.
- **If a future system requests PvP damage in T1**: reject the request; emit a development warning in non-shipping builds; no damage or threat event fires.
- **If a future companion actor uses the interface before companion GDDs are authored**: test harnesses may instantiate a combat actor through the generic interface only; shipped T1 content must not spawn companion combat actors.
- **If hate values tie exactly**: earliest threat-entry timestamp wins; if still tied, lowest `combat_sort_key` wins. This keeps tests deterministic without using runtime ids as stable identity.
- **If a hostile threat table has exactly one valid positive entry**: that entry is selected as current target and its HUD-facing category is `HasAggroStable`; `second_highest_valid_threat` is defined as 0 for the contested ratio.
- **If a threat table contains zero, negative, dead, or out-of-zone entries**: those entries are invalid for target/category evaluation. Negative threat in authored or hydrated data fails validation.
- **If a target is valid for targeting but not line-of-sight valid for spell completion**: cast fails as `CastResolvedInvalidTarget`; mana rule follows Rule 13 default unless spell profile overrides later.

## Dependencies

### Direct upstream dependencies

| Dependency | Type | Contract |
|---|---|---|
| World Structure | Hard | Provides active `zoneId`, `zoneType`, `ZoneActiveEvent`, and `ZoneTransitionBeginEvent`; consumes `PlayerDeathEvent`. |
| NPC System | Hard | Provides combat-eligible actor identity/seed data; receives combat claim/release and death outcome. |
| Save / Load & Persistence | Hard | Persists explicit combat state fields selected by this GDD; rejects invalid hydration before gameplay enables. |

### Direct downstream / same-tier dependents

| Dependent | Contract Combat Core Must Provide |
|---|---|
| Character Progression | Approved `PlayerKillCreditEvent` hook plus ADR-0003 `CombatProgressionBaselineSnapshot` input for player actor level and permanent health/mana maxima; Combat Core must not consume generic progression snapshots, `visible_level`, XP progress fields, or spell eligibility. |
| Class Design | Shared actor, Attack toggle, tactical instant profile contract, spell profile, threat, and resource hooks for Cleric T1 and Warrior/Enchanter T2; when authored, Class Design must reverse-list Combat Core's tactical instant fixture contract and decide final Cleric names/values. |
| Spell Memorization | Cast request, zero-cast-time instant profile support, cast start, cast complete, interrupt, cancel, cooldown, and recovery framework. |
| Status Effects & Buffs | Hook points for modifiers to damage, threat, interrupt, regen, movement, casting, and actor state. |
| Creature / Enemy AI | Threat table, current target, damage results, death state, and leash state. |
| Death & Corpse Recovery | Player death payload with `death_context_id`, stable source refs, death position, and zone id. |
| Zone Control | Kill-credit event with stable defeated source ref, `zoneId`, `faction_id`, and kill-weight seed. |
| Layer 1 HUD | Practical state outputs: health, mana, target, cast, recovery, Attack on/off plus explicit Attack ON visual-state signal, and categorical hate. |
| Audio System | Combat timing events for restrained SFX and no-stinger death/interruption policy. |

### Forward-looking T2+ dependents

- **Class Design - Warrior + Enchanter** must consume the Combat Core actor interface and add tank-specific and crowd-control-specific class rules without redefining base threat or death mechanics.
- **Named AI Companion Core** must implement companion combat actors through the same interface rather than creating a parallel companion combat model.
- **Sister Elara Mentor** may use the interface for authored onboarding beats later, but Combat Core does not author her behavior, tanking, healing, or relationship logic.
- **Network Architecture** must not be anticipated in T1. When FishNet arrives at T2, it owns authority/replication around this already-authored local combat model.

### Scope-guarded amendment candidates

- **NPC System Rule 16 verification** - Combat Core should be cross-checked against `npc-system.md:82` and `npc-system.md:403` during review. Any divergence in hostility/hate/damage/death ownership must be resolved explicitly.
- **World Structure death-event fields** - `world-structure.md:615` only requires `PlayerDeathEvent`; this GDD now requires `PlayerDeathPayload` with `death_context_id`, `local_character_id`, `zoneId`, `death_position`, stable `killer_source_ref`, and `death_cause_type`. World Structure and Death & Corpse Recovery must mirror or explicitly supersede that payload when their implementations/GDDs are updated.
- **Save/Load Rule 1 amendment** - Save/Load may need to list Combat Core's direct persisted fields (`current_health`, `current_mana`, `combat_life_state`, and optional pending death handoff payload) before implementation. Threat tables, cast progress, swing timers, target selection, cooldown timers, regen rates, `combat_actor_id`, runtime handles, XP, item drops, and corpse records remain outside Combat Core persistence.
- **Class Design tactical instant reverse-listing** - Class Design must reverse-list Combat Core's D012 tactical instant profile contract (`cast_time_seconds = 0`, mana cost, cooldown, and declared effects) before final Cleric T1 spell/ability data is accepted.

## Cross-References

These references do not override Locked Inputs. They bind implementation constraints that Combat Core must respect when translated into stories or code.

| Source | Lines | Combat Core Use |
|---|---:|---|
| `.claude/docs/technical-preferences.md` | 12-15 | Unity 6.3 LTS, C#/.NET, URP, and standard MonoBehaviour PhysX are the implementation baseline. |
| `.claude/docs/technical-preferences.md` | 22-27 | T1 input is keyboard/mouse first; tab-target, auto-attack, sit/stand, and cast commands must be keybindable. |
| `.claude/docs/technical-preferences.md` | 49-50 | Formula-heavy combat code targets Unity Test Framework coverage, with 90% expected on combat formulas. |
| `.claude/docs/technical-preferences.md` | 61-66 | No speculative dependencies; FishNet remains deferred until T2+ work begins. |
| `docs/engine-reference/unity/VERSION.md` | 5, 58 | Unity 6.3 LTS is the engine target; FishNet is planned but not installed for T1. |
| `docs/engine-reference/unity/modules/physics.md` | 63-87, 94-99 | Targeting, line-of-sight, and aggro-radius checks should use non-allocating physics queries and LayerMasks where practical. |
| `docs/engine-reference/unity/modules/physics.md` | 205-211 | Combat collision/query behavior should respect the Physics Layer Collision Matrix and fixed-timestep settings. |
| `docs/engine-reference/unity/modules/navigation.md` | 89-124, 321-323 | Creature / Enemy AI path validity and leashing should consume NavMesh path status and avoid per-frame `SetDestination()` churn. |
| `docs/engine-reference/unity/modules/animation.md` | 10-11, 34-40 | Combat posture, pivot, medbreak, and death animations should use Animator Controller state machines rather than legacy animation. |
| `docs/engine-reference/unity/modules/audio.md` | 10-12, 256-260 | Combat Core emits audio timing hooks only; Audio System owns AudioSource/Audio Mixer playback and the single-listener rule. |
| `DECISIONS.md` | 339-358 | D012 locks pinned-engine combat-feel validation and requires Combat Core revision before `/sprint-plan new`. |
| `production/prototypes/combat-feel-report.md` | 196-251 | Pinned Unity 6000.3.14f1 validation passes all six combat-feel criteria and identifies v2 Attack toggle plus tactical instants as the preferred T1 combat baseline. |

## Tuning Knobs

| Knob | Default | Safe Range | Higher Means | Lower Means |
|---|---:|---:|---|---|
| `combat_tick_rate_hz` | 50 Hz | 20-60 Hz | More frequent deterministic combat resolution; closer to rendering/physics cadence. | More EQ-like chunky cadence; larger per-tick state jumps. |
| `combat_tick_seconds` | 0.02 s | Derived | Derived from `combat_tick_rate_hz`. | Derived from `combat_tick_rate_hz`. |
| `target_acquire_radius_meters` | 35 m | 15-60 m | Easier target cycling; more risk of selecting through clutter. | More local targeting; harder pull setup. |
| `combat_query_buffer_size` | 64 | 32-128 | More hostile/query hits before fixture overflow. | Smaller memory footprint; more likely overflow validation failure. |
| `melee_range_meters` | 2.25 m | 1.5-3.5 m | More forgiving melee contact. | More positional friction. |
| `spell_range_meters_default` | 30 m | 15-45 m | Safer pulls/casts; may trivialize trash. | More dangerous casting; may frustrate Cleric. |
| `weapon_delay_seconds_player_default` | 2.8 s | 1.8-4.5 s | Slower auto-attack cadence, more EQ-like patience. | Faster ticks, more modern/action feel. |
| `combat_exit_timer_seconds` | 30 s | 10-60 s | Longer commitment, delays med-break. | Easier regen/leash exploits. |
| `leash_distance_meters` | 35 m | 15-80 m | Longer chases, more dangerous overpulls. | Easier disengage. |
| `path_failure_grace_seconds` | 1.0 s | 0.25-3.0 s | More tolerance for brief NavMesh/path churn. | Faster leash on path failure. |
| `path_pending_grace_seconds` | 1.0 s | 0.25-3.0 s | More tolerance for pending NavMesh path probes. | Faster leash on stalled path probes. |
| `path_status_sample_seconds` | 0.25 s | 0.10-1.0 s | Less path-status churn. | Faster path-failure detection; more CPU. |
| `leash_reaggro_distance_meters` | 20 m | 5-40 m | Easier re-aggro before anchor return. | Cleaner disengage once fleeing. |
| `leash_threat_memory_seconds` | 30 s | 5-60 s | Longer hostile memory after leash starts. | Faster threat cleanup and safer reset. |
| `social_assist_pulse_seconds` | 2.0 s | 1.0-5.0 s | Slower assist refresh; fewer query checks. | Faster social adds; higher query cadence. |
| `social_assist_radius_meters` | 12 m | 4-25 m | Wider social pull chains. | More isolated pulls. |
| `assist_threat_initial` | 25 | 1-200 | Assisting enemies stick to the initial player target longer. | Assists can be peeled by damage/heal events sooner. |
| `base_hit_chance` | 0.72 | 0.50-0.90 | More consistent melee damage. | More misses, slower fights. |
| `hit_chance_min` | 0.10 | 0.05-0.25 | More chance against hard targets. | Hard walls by level/skill. |
| `hit_chance_max` | 0.92 | 0.75-0.98 | Fewer misses when advantaged. | More uncertainty even against easy trash. |
| `damage_threat_scalar` | 1.0 | 0.5-2.0 | Damage dominates threat. | Healing/proximity relatively stronger. |
| `healing_threat_scalar` | 0.75 | 0.25-2.0 | Heals more dangerous, group-dependency pressure rises. | Healer threat less relevant. |
| `proximity_threat_initial` | 25 | 1-200 | Body-pulls stick longer. | Heals/damage override initial aggro faster. |
| `sitting_threat_bonus` | 50 | 0-300 | Sitting in combat is more dangerous. | Less penalty for bad med timing. |
| `threat_close_ratio` | 0.85 | 0.70-0.95 | Non-top actors become "close" only near the top. | More actors count as close to aggro. |
| `aggro_contested_ratio` | 0.90 | 0.75-0.98 | Current target remains stable until challengers nearly tie. | More current-target states become contested. |
| `threat_entry_cap` | 100000 | 10000-1000000 | Longer fights before threat clamps. | Earlier cap pressure; easier cap-edge testing. |
| `base_interrupt_chance` | 0.20 | 0.05-0.50 | Casts are more fragile. | Cleric can face-tank casts more easily. |
| `interrupt_chance_max` | 0.85 | 0.50-1.0 | Heavy hits nearly always interrupt. | Interrupts stay softer. |
| `cast_time_default_seconds` | 6.0 s | 2.0-8.0 s | Slower cadence, more EQ pressure. | Faster, more modern feel. |
| `recovery_default_seconds` | 1.5 s | 0.5-4.0 s | Less spell spam, more downtime. | More chain-casting. |
| `regen_tick_interval_seconds` | 6 s | 2-10 s | Chunkier EQ-like med ticks. | Smoother resource bars. |
| `sitting_mana_regen_multiplier` | 4.0 | 2.0-8.0 | Shorter med breaks. | Longer med breaks. |
| `in_combat_mana_regen_multiplier` | 0.0 | 0.0-0.25 | More forgiving long fights. | Stricter med-break dependency. |
| `trash_clean_solo_target_success_rate` | 0.95 | 0.90-1.00 | Clean same-band solo trash is more reliable. | Clean same-band solo trash becomes more punishing; disadvantage-start vulnerability needs its own fixture. |

## Visual/Audio Requirements

Combat Core is governed by the art bible's combat state.

- **The pivot is the signal.** Combat initiation is communicated by enemy facing/stance change, not by VFX, outline, nameplate color, alert icon, or audio sting.
- **No global combat treatment.** Combat must not trigger combat-state post-process, red vignette, global desaturation, sky/lighting changes, camera shake, or full-screen warning overlays.
- **Spell VFX are brief and local.** Cast effects may create short localized light events, with full-intensity duration under 0.4 seconds unless a later Class Design spell explicitly earns an exception.
- **Magic stays cool/desaturated.** Warm VFX are reserved for physically motivated fire or biological heat, not generic spell power.
- **Med break reads through posture.** Sitting/medding uses `medbreak_sit` and `medbreak_rise` animation states from the art-bible naming grammar; it should feel like setting down weight, not activating a buff.
- **Death has no spectacle.** Combat Core emits death events. Death & Corpse Recovery owns corpse-run visuals later. No death music sting, red screen, celebratory kill flash, or slow-motion effect is authored here.
- **Audio hooks are restrained.** Combat Core may emit swing, hit, miss, cast start, interrupt, fizzled/cancelled cast, actor death, and med start/stop hooks. Audio System owns playback. No aggro sting, rare-spawn sting, low-health alarm, or kill fanfare.

## UI Requirements

Combat Core owns no UI presentation, but it must expose state required by Layer 1 HUD.

### Combat Core exposes

- Player current/max health.
- Player current/max mana.
- Current target id, target current/max health, target alive/dead state, target hostility state.
- Attack on/off, explicit Attack ON visual-state signal, and next swing readiness category.
- Cast state: not casting, casting, interrupted, recovery, cast complete/fail.
- Cast progress normalized value for HUD bar/timer.
- Categorical threat state: `NoThreat`, `ThreatListed`, `ThreatClose`, `HasAggroStable`, or `HasAggroContested`.
- Combat state: out of combat, in combat, exiting combat, dead.

### Combat Core forbids

- Raw numeric threat display in shipping player UI; dev-build diagnostics may expose raw values behind debug tools.
- Floating damage numbers as a Combat Core requirement.
- Nameplate color changes, aggro outlines, target rings, quest markers, minimap enemy dots, or path arrows.
- Center-screen combat warnings except future accessibility work explicitly approved by HUD/Accessibility GDDs.
- Any HUD element participating in world desaturation; HUD presentation follows Layer 1 GDD/art-bible rules.

Layer 1 HUD owns the final visual treatment for Attack ON, but Combat Core must expose an unambiguous state/event so the HUD can render it distinctly. The combat-feel prototype's pinned-engine highlight fix showed that unclear Attack feedback made the otherwise validated toggle feel clunky; this GDD requires the state surface, not the specific gold-highlight prototype presentation.

## Acceptance Criteria

All Combat Core acceptance criteria use the project QA taxonomy: Unit, Integration, Editor-validation, Dev-build smoke, or Profiled playtest. All are T1-blocking. Criteria marked `fixture-gated T1-blocking` cannot pass until the prototype fixture data in this GDD is materialized in test data, but they still block implementation closeout.

### Scope and zone gate

**H-CCOM-SCOPE-01 - T1 strict scope**
**GIVEN** the T1 Combat Core scene/content set, **WHEN** code/assets/config are inspected, **THEN** there is no FishNet/networking combat authority, account identity, PvP damage, Warrior/Enchanter player class behavior, live LLM dependency, companion combat behavior, raid logic, or server combat state.
*Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-WS-01 - HauntZone enables combat**
**GIVEN** World Structure publishes `ZoneActiveEvent(zoneId, zoneType = HauntZone)`, **WHEN** Combat Core receives the event and actors are ready, **THEN** hostile combat actor claiming, targeting, auto-attack, casts, threat, and kill-credit emission are enabled for eligible haunt actors.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-WS-02 - CityHubZone disables hostile combat**
**GIVEN** World Structure publishes `ZoneActiveEvent(zoneId, zoneType = CityHubZone)`, **WHEN** the player targets an NPC and toggles auto-attack, **THEN** no hostile threat table is created, no damage event fires, no kill credit can emit, and combat remains disabled.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-WS-03 - Zone transition cleanup**
**GIVEN** the player is auto-attacking or casting in a `HauntZone`, **WHEN** `ZoneTransitionBeginEvent(outgoingZoneId, incomingZoneId)` fires, **THEN** Combat Core cancels casting, disables auto-attack, clears transient hit windows/projectiles, emits cancellation events, and no damage/threat/cast result applies to the incoming zone.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-TICK-01 - Fixed combat simulation tick**
**GIVEN** Unity render frame rate varies while `Time.fixedDeltaTime` remains at the project default, **WHEN** Combat Core resolves swing, cast, recovery, regen, leash, assist, and threat timers, **THEN** all Combat-owned transitions resolve on `combat_tick_rate_hz = 50` / `combat_tick_seconds = 0.02` and test logs identify same-frame cases by Combat Simulation Tick id.
*Unit + Integration | gameplay-programmer + engine-programmer + qa-tester | T1-blocking*

**H-CCOM-PAUSE-01 - Combat hard-stop pause resumes from state tick**
**GIVEN** the player pauses during a cast, swing delay, regen timer, social-assist pulse, leash timer, hostile pursuit, combat animation transition, or combat audio hook window, **WHEN** the pause is held for real-world time and then resumed, **THEN** Combat-owned timers resume from the paused combat tick with no wall-clock catch-up, no hostile intent advances, combat-owned NavMesh intents are held, combat animation event progression tied to Combat Core is held, no new combat audio hook emits while paused, and Day/Night world-clock behavior remains outside Combat Core.
*Integration | gameplay-programmer + engine-programmer + qa-tester | T1-blocking*

### Actor, target, and auto-attack

**H-CCOM-ACTOR-01 - Combat actor interface fields**
**GIVEN** a player actor and a hostile NPC actor are created for a T1 combat fixture, **WHEN** their `CombatActorState` records are inspected, **THEN** each contains transient `combat_actor_id`, actor kind, `stable_source_ref`, zone id, level, current/max health, current/max mana where applicable, armor class, attack power, weapon delay, melee range, spell range, combat state, optional target `combat_actor_id`, `combat_sort_key`, and transient threat-table support; no GameObject, Transform, Animator, Material, Texture, Addressable handle, runtime scene handle, or persisted runtime id is serialized in the actor record.
*Unit + Editor-validation | gameplay-programmer | T1-blocking*

**H-CCOM-TGT-01 - Target acquisition radius and LoS**
**GIVEN** hostile actors inside and outside `target_acquire_radius_meters`, with valid and blocked LoS fixtures, **WHEN** the player cycles targets, **THEN** only alive combat-eligible actors inside radius and with valid LoS can become the current target.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-AA-01 - Auto-attack toggle starts weapon-delay ticks**
**GIVEN** the player has a valid hostile target in melee range in a `HauntZone`, **WHEN** auto-attack is toggled on, **THEN** Combat Core schedules melee swing checks on `weapon_delay_seconds` ticks until a stop condition occurs.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-AA-02 - Out-of-range swing skips damage**
**GIVEN** auto-attack is on and the hostile target moves outside `melee_range_meters` before the next weapon tick, **WHEN** the tick resolves, **THEN** no damage or threat from that swing is applied and the actor remains in combat if threat remains valid.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-AA-03 - Pull does not auto-enable Attack**
**GIVEN** the player targets, tab-cycles to, body-pulls, or spell-pulls a valid hostile actor in a `HauntZone`, **WHEN** threat initializes or the hostile enters combat, **THEN** the player's `Attack` state remains off until the player explicitly toggles it on; pressing `Attack` with no valid hostile target no-ops; target death, successful sit/med, combat exit, death, and zone transition force `Attack` off.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-F1 - Melee hit chance formula**
**GIVEN** equal-level attacker/defender with equal skills and default hit parameters, **WHEN** `melee_hit_chance` is evaluated, **THEN** output is `0.72`; with extreme unfavorable/favorable inputs, output clamps to `hit_chance_min` and `hit_chance_max` respectively.
*Unit | gameplay-programmer | T1-blocking*

**H-CCOM-F2 - Melee damage formula**
**GIVEN** `weapon_base_damage = 8`, `attack_power = 20`, `attack_power_scalar = 0.20`, `damage_roll_scalar = 1.0`, `defender_armor_class = 30`, and `armor_mitigation_scalar = 0.10`, **WHEN** `melee_damage` is evaluated, **THEN** output is `9`; successful hits never output below `1`.
*Unit | gameplay-programmer | T1-blocking*

**H-CCOM-FIXTURE-01 - Prototype fixture package exists**
**GIVEN** the Combat Core prototype fixture data is loaded, **WHEN** QA inspects the fixture package, **THEN** it includes lowest/mid/top T1 Cleric fixtures, lowest/mid/top T1 trash fixtures, a top-band named fixture, `Smite_T1_Prototype`, `LesserHeal_T1_Prototype`, D012 tactical instant fixtures (`SmiteOfAuthority_T1_Prototype`, `Bash_T1_Prototype`, and `DefensivePrayer_T1_Prototype` or Class Design-approved T1 equivalents), and the `SoloTrash_EvenCon_T1`, `TwoTrash_Overpull_T1`, and `NamedSoloBlock_T1` encounter fixtures with explicit T1 `kill_weight_seed` values and source-ref aliases for downstream fixture validation; `Cleric_Mid_T1` is level 5 with 140 HP and 180 max mana; the package documents that final levels remap to the T1 haunt band when Level / Encounter Design locks that band.
*Editor-validation + Unit | game-designer + systems-designer + qa-tester | fixture-gated T1-blocking*

**H-CCOM-F2B - Damage fixture extremes**
**GIVEN** `Cleric_Low_T1`, `Cleric_Top_T1`, `Trash_Low_T1`, and `Trash_Top_T1` fixture stats exactly as listed in the actor fixture table, **WHEN** seeded melee damage samples are evaluated at low, default, and high armor values, **THEN** extreme armor cases clamp to at least `1` on successful hits and approved trash fixtures remain inside the 2-20 normal-hit target band unless Class Design amends the baseline.
*Unit | gameplay-programmer + systems-designer | fixture-gated T1-blocking*

### Pulling and threat

**H-CCOM-PULL-01 - Body/LoS pull initializes threat**
**GIVEN** a hostile actor with an authored aggro radius and LoS to the player, **WHEN** the player enters the body-pull threshold, **THEN** the hostile actor pivots/claims combat, initializes a threat table with `proximity_threat_initial`, and enters combat without a marker, bark, UI alert, or scripted encounter trigger.
*Integration + Dev-build smoke | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-PULL-02 - Social-link assist**
**GIVEN** two hostile actors with an authored `SocialAssistProfile` sharing `social_link_group_id`, passing faction and encounter filters, within `assist_radius_meters`, and meeting required LoS to the primary hostile and player target point, **WHEN** the player body-pulls one and the assist predicate resolves, **THEN** the linked hostile actor enters the threat table with exactly `assist_threat_initial = 25` by default, and eligible assisters are processed by distance, `assist_order_index`, then `combat_sort_key`.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-PULL-03 - LoS query contract uses anchors, LayerMask, and non-alloc buffer**
**GIVEN** hostile/player fixtures with authored and fallback query anchors plus occluders on `WorldSolid`, `ClosedDoor`, `LargeProp`, `CombatActor`, `TriggerOnly`, `InteractableSoft`, and VFX layers, **WHEN** target acquisition, body-pull, and assist LoS queries run, **THEN** Combat Core uses the configured anchor points, blocks only on `los_occluder_layer_mask_t1`, uses non-allocating query buffers sized by `combat_query_buffer_size`, sorts returned hits by distance, `combat_sort_key`, and authored collider index, logs `CombatQueryBufferOverflow` on fixture overflow, and never allocates fallback query arrays.
*Integration + Dev-build smoke | gameplay-programmer + engine-programmer + qa-tester | T1-blocking*

**H-CCOM-PULL-04 - Social assist pulses are bounded**
**GIVEN** a hostile is in combat and eligible linked hostiles exist inside and outside `social_assist_radius_meters`, with passing/failing faction filters, encounter filters, and LoS fixtures, **WHEN** immediate assist and later `social_assist_pulse_seconds` pulses resolve, **THEN** only alive eligible actors in the shared `social_link_group_id`, inside radius, passing filters, and meeting required LoS join the threat table, each no more than once per pull episode.
*Integration | gameplay-programmer + ai-programmer + qa-tester | T1-blocking*

**H-CCOM-LEASH-01 - Path failure enters Leashing**
**GIVEN** a hostile actor has active threat and Creature / Enemy AI publishes `PathProbeResult` at or above `path_status_sample_seconds`, **WHEN** `PathPartial` or `PathInvalid` remains continuous longer than `path_failure_grace_seconds`, or `path_pending` remains true longer than `path_pending_grace_seconds`, **THEN** Combat Core changes the actor to `Leashing`, stops new attacks/casts, clears active attack intent, requests return-to-anchor behavior through Creature / Enemy AI, and preserves threat memory only for `leash_threat_memory_seconds`.
*Integration | gameplay-programmer + ai-programmer + qa-tester | T1-blocking*

**H-CCOM-LEASH-02 - Re-aggro and threat memory expiry**
**GIVEN** a hostile actor is leashing with active threat memory, **WHEN** the player re-enters before memory expiry, **THEN** re-aggro occurs only if the player is inside `leash_reaggro_distance_meters`, LoS is valid, and the hostile has not reached its anchor; after anchor return or `leash_threat_memory_seconds` expiry, the threat table clears and the same player contact starts a fresh pull.
*Integration | gameplay-programmer + ai-programmer + qa-tester | T1-blocking*

**H-CCOM-F3 - Threat delta formula**
**GIVEN** `healing_done = 20`, `healing_threat_scalar = 0.75`, `proximity_threat = 25`, `sitting_threat_bonus = 50`, and all other threat inputs 0, **WHEN** `threat_delta` is evaluated, **THEN** output is `90`.
*Unit | gameplay-programmer | T1-blocking*

**H-CCOM-HATE-01 - Highest threat target selected**
**GIVEN** a hostile actor's threat table contains two valid player-side actors with different accumulated threat values, **WHEN** target selection updates, **THEN** the hostile actor targets the actor with the highest threat.
*Unit | gameplay-programmer | T1-blocking*

**H-CCOM-HATE-02 - Threat tie deterministic**
**GIVEN** two valid threat entries have equal threat, **WHEN** target selection updates, **THEN** the earliest threat-entry timestamp wins; if timestamps are also equal, the lowest `combat_sort_key` wins.
*Unit | gameplay-programmer | T1-blocking*

**H-CCOM-HATE-03 - Healing can overtake damage threat**
**GIVEN** a hostile actor threat table contains a future tank/test actor ahead of the Cleric by less than the configured effective-healing threat delta, **WHEN** the Cleric lands enough effective healing or sits while on the table, **THEN** accumulated Cleric threat can become highest and the hostile retargets to the Cleric on the next target update.
*Unit + Integration | gameplay-programmer + systems-designer | T1-blocking*

**H-CCOM-HUD-01 - Threat exposed categorically**
**GIVEN** the player is absent from threat, listed below threshold, listed above `threat_close_ratio`, holding stable aggro, and holding contested aggro in separate fixtures, **WHEN** HUD-facing combat state is inspected, **THEN** Combat Core exposes `NoThreat`, `ThreatListed`, `ThreatClose`, `HasAggroStable`, or `HasAggroContested` and does not expose raw threat values as required shipping HUD output.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-HUD-03 - Threat category thresholds are deterministic**
**GIVEN** empty, zero-threat, negative-threat, single-entry, tied-entry, and threshold-boundary threat fixtures around `threat_close_ratio = 0.85` and `aggro_contested_ratio = 0.90`, **WHEN** category evaluation runs, **THEN** empty or zero-top tables map to `NoThreat`, negative entries fail validation, a single valid positive current target maps to `HasAggroStable`, non-top ratios below 0.85 map to `ThreatListed`, non-top ratios at or above 0.85 map to `ThreatClose`, top-target second-place ratios below 0.90 map to `HasAggroStable`, and top-target second-place ratios at or above 0.90 map to `HasAggroContested`.
*Unit | gameplay-programmer + systems-designer | T1-blocking*

### Casting, interrupts, and recovery

**H-CCOM-CAST-01 - Valid slow cast enters Casting**
**GIVEN** a Cleric player with enough mana, valid hostile target in spell range and LoS, and a spell profile with `cast_time_seconds = 6.0`, **WHEN** the cast request is issued, **THEN** Combat Core enters `Casting`, emits `CastStartedEvent`, and reports normalized cast progress until completion/interruption/cancel.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-CAST-02 - Cast completion spends mana**
**GIVEN** a valid cast reaches completion with target still valid, **WHEN** Combat Core resolves the cast, **THEN** mana decreases by `mana_cost`, the spell result event emits, and the caster enters `Recovery` for `recovery_seconds`.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-CAST-03 - Manual cancel does not spend mana**
**GIVEN** the player is casting and cancels before completion, **WHEN** cancellation resolves, **THEN** `CastCancelledEvent` emits, mana is unchanged, and configured cancel/recovery behavior applies.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-F4 - Interrupt chance formula**
**GIVEN** `base_interrupt_chance = 0.20`, `damage_taken = 10`, `max_health = 100`, `damage_interrupt_scalar = 4.0`, `cast_time_remaining_seconds = 2`, `cast_time_total_seconds = 4`, `early_cast_interrupt_scalar = 0.10`, `interrupt_resistance = 0`, `interrupt_chance_min = 0.05`, and `interrupt_chance_max = 0.85`, **WHEN** `interrupt_chance` evaluates, **THEN** output is `0.65`.
*Unit | gameplay-programmer | T1-blocking*

**H-CCOM-CAST-04 - Damage interrupt result**
**GIVEN** a deterministic interrupt roll below computed `interrupt_chance`, **WHEN** the player takes post-mitigation `damage_taken > 0` during `Casting`, **THEN** the cast is interrupted, `CastInterruptedEvent` emits, mana is not spent under T1 default rule, and the actor enters `Interrupted` / recovery; zero, absorbed, or blocked damage does not roll an interrupt.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-CAST-05 - Same-frame cast completion priority**
**GIVEN** a cast completion and incoming damage occur in the same Combat Simulation Tick, **WHEN** transition priority resolves, **THEN** cast completion resolves before the new interrupt check, and test logs show no double result.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-IF-01 - Spell Memorization cast lifecycle interface**
**GIVEN** a test spell profile with cast time, mana cost, range, interrupt profile, and recovery seconds, **WHEN** Combat Core resolves start, completion, interruption, cancellation, recovery start, and recovery end cases, **THEN** it emits `CastStartedEvent`, `CastCompletedEvent`, `CastInterruptedEvent`, `CastCancelledEvent`, `CastRecoveryStartedEvent`, and `CastRecoveryEndedEvent` with spell id, caster transient id, target transient id where applicable, and Combat Simulation Tick id; Combat Core does not own spellbook slots or memorized-spell availability.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-INST-01 - Tactical instant profile contract**
**GIVEN** a Cleric tactical instant profile with `cast_time_seconds = 0`, authored mana cost, authored cooldown, valid range, and declared effect type (`direct_damage`, `self_buff`, or `interrupt_current_channel`), **WHEN** the player activates the ability with sufficient mana and a valid target/self target, **THEN** Combat Core resolves it without a cast bar, spends mana through the same Rule 13 path, starts the transient cooldown, emits ability lifecycle/result events, and applies the declared effect without hard-coded numeric values in Combat Core prose or code. If `interrupt_current_channel` is declared and the target is channeling, the current channel is cancelled through the normal interrupt/cancel event surface.
*Integration + Unit | gameplay-programmer + systems-designer + qa-tester | T1-blocking*

### Regen and med breaks

**H-CCOM-MED-01 - Sitting disables auto-attack**
**GIVEN** auto-attack is on and the player successfully enters any sitting/med posture, **WHEN** the sitting state begins, **THEN** auto-attack is disabled before regen/threat updates and no melee ticks occur while seated.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-MED-02 - Sitting out of combat boosts mana regen**
**GIVEN** `Cleric_Mid_T1` at level 5 with 180 max mana, default mana regen parameters, sitting out of combat, **WHEN** `regen_tick` resolves, **THEN** mana increases by `8` per tick in the example fixture and never exceeds max mana.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-MED-03 - Sitting in combat does not grant med boost**
**GIVEN** the player is on a hostile threat table and sits, **WHEN** the next regen tick and threat update resolve, **THEN** out-of-combat sitting mana multiplier is not applied and `sitting_threat_bonus` is added to relevant hostile threat tables.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-F5 - Combat exit timer formula**
**GIVEN** `seconds_since_last_hostile_action = 30.1`, `combat_exit_timer_seconds = 30`, and `valid_hostile_threat_entries = 0`, **WHEN** `combat_exit_timer` evaluates, **THEN** `can_exit_combat = true`; if one valid hostile entry exists, output is false.
*Unit | gameplay-programmer | T1-blocking*

### Death, kill credit, and persistence

**H-CCOM-DEATH-01 - PlayerDeathEvent fires once**
**GIVEN** the player has `current_health > 0` and the deterministic death-context id provider returns `death_context_id = DCTX-T1-0001`, **WHEN** damage reduces `current_health <= 0`, **THEN** Combat Core clamps health to 0, stops casting/auto-attack, emits exactly one `PlayerDeathEvent(death_payload)` containing that `death_context_id`, `local_character_id`, `zoneId`, `death_position`, stable `killer_source_ref`, and `death_cause_type`, and blocks further player combat input.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-DEATH-02 - World Structure consumes death**
**GIVEN** the player dies in a `HauntZone`, **WHEN** `PlayerDeathEvent(death_payload)` emits, **THEN** World Structure receives the payload in the same Combat Simulation Tick, records `death_context_id` as the active handoff correlation key, transitions to `CorpseRunActive` within the same frame per its contract, creates or updates the normal World Structure-owned `CorpseRecord`, and ignores any duplicate event with the same `death_context_id`; Combat Core does not create `CorpseRecord` directly.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-DEATH-03 - Death payload schema is narrow**
**GIVEN** a T1 player death caused by an NPC, non-persistent spawn, or environmental combat source, **WHEN** the emitted death payload and optional pending save handoff are inspected, **THEN** they contain only `death_context_id`, `local_character_id`, `zoneId`, `death_position`, stable `killer_source_ref`, and `death_cause_type`; they contain no `combat_actor_id`, account id, player-vs-player source, server authority field, raw threat table, corpse record, XP penalty, item drop, or LLM/narrative context.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-KILL-01 - NPC death emits kill credit hook**
**GIVEN** a hostile NPC combat actor reaches zero health and the player has qualifying contribution, **WHEN** death resolves, **THEN** Combat Core opens one same-tick `CombatKillResolutionPhase`, emits `CombatActorDeathEvent(combat_actor_id, defeated_source_ref, zoneId)` for immediate runtime subscribers and `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)` exactly once, waits for same-tick NPC source-lifecycle and Character Progression award-snapshot acknowledgements before source cleanup/despawn or respawn-token rotation, and treats only `defeated_source_ref` as legal for persistence or downstream long-lived records.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-SL-01 - Combat persistence whitelist**
**GIVEN** a save fixture after combat, **WHEN** serialized combat-related state is inspected, **THEN** it may contain player `current_health`, `current_mana`, `combat_life_state`, and optional pending death handoff payload (`death_context_id`, `local_character_id`, `zoneId`, `death_position`, `killer_source_ref`, `death_cause_type`) only; it does not contain threat tables, cast progress, recovery timers, swing timers, target selection, projectiles, `combat_actor_id`, runtime handles, animation state, hit rolls, cooldown timers, regen rates, or cached formula outputs.
*Unit | gameplay-programmer | T1-blocking*

**H-CCOM-SL-02 - Invalid combat hydration fails loud**
**GIVEN** Save/Load hydrates Combat Core with `current_health <= 0` and no valid death/corpse-run handoff state, **WHEN** Combat Core validates hydration before gameplay enablement, **THEN** it reports hydration failure; Save/Load emits `LoadRejected(HydrationFailed)`; no playable `ZoneActiveEvent` state begins.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-SL-03 - Spell, cooldown, and regen persistence ownership**
**GIVEN** a save fixture with memorized spells, active cooldown timers, and current regen state, **WHEN** Combat Core persistence is inspected, **THEN** memorized spell slots are absent because Spell Memorization owns them, cooldown timers are absent because they are transient combat runtime, and regen rates are absent because they are derived from formulas and actor state on load.
*Unit | gameplay-programmer + systems-designer | T1-blocking*

### T1 feel and soloability

**H-CCOM-FEEL-01 - Cleric single-trash success envelope**
**GIVEN** `SoloTrash_EvenCon_T1` starts the same-band Cleric fixture above 80% health and above 60% mana against one same-band `encounter_role = Trash` fixture (`Cleric_Mid_T1` uses 140 HP / 180 mana), **WHEN** QA runs 20 seeded combat trials using intended casts, auto-attack, tactical instants, and med breaks, **THEN** the Cleric wins 90-100% of clean-state trials and the mean ending state is below either 80% health or 60% mana so that immediately pulling the same fixture again is measurably riskier than sitting/regen first.
*Profiled playtest | game-designer + qa-tester | fixture-gated T1-blocking*

**D014 note:** This criterion covers clean-state solo trash only. Low-resource,
surprise-pull, interrupted-med-break, or other disadvantage-start solo-trash
vulnerability requires a separate future fixture and acceptance criterion.

**H-CCOM-FEEL-02 - Named enemy not soloable**
**GIVEN** `NamedSoloBlock_T1` pits `Cleric_Top_T1` (220 HP / 300 mana) against `Named_Top_T1` with `encounter_role = Named`, `solo_block_profile_id`, and no companion/party support, **WHEN** QA runs 10 seeded combat trials, **THEN** the Cleric loses or must flee in at least 8/10 trials and the log attributes the block to health/mana wall plus interrupt pressure; any victory is logged as a tuning defect unless caused by known exploit under investigation.
*Profiled playtest | game-designer + qa-tester | fixture-gated T1-blocking*

**H-CCOM-FEEL-03 - Two-trash overpull is dangerous**
**GIVEN** `TwoTrash_Overpull_T1` body-pulls two same-band even-con `encounter_role = Trash` enemies within 5 seconds using the actor fixture table values for the selected band, **WHEN** QA runs 10 seeded combat trials, **THEN** the Cleric loses, flees, or survives below 20% health or below 10% mana in at least 8/10 trials; normal two-trash farming is not viable.
*Profiled playtest | game-designer + qa-tester | fixture-gated T1-blocking*

**H-CCOM-FEEL-04 - Med break pacing**
**GIVEN** the player exits `SoloTrash_EvenCon_T1` as `Cleric_Mid_T1` below 35% of 180 mana and has no valid hostile threat entries after the 30-second combat-exit timer, **WHEN** the player sits to med using default regen parameters, **THEN** the time to return to 70% of 180 mana is within the 60-120 second target band and mana changes only on `regen_tick_interval_seconds` ticks.
*Profiled playtest + Integration | game-designer + qa-tester | fixture-gated T1-blocking*

### Presentation compliance

**H-CCOM-ART-01 - Pivot without warning marker**
**GIVEN** a body-pull begins in the haunt, **WHEN** the encounter starts, **THEN** the visible signal is the enemy pivot/stance shift; no aggro outline, overhead icon, nameplate color change, alert VFX, minimap marker, proximity bark, or warning sting appears.
*Dev-build smoke | game-designer + art-lead + qa-tester | T1-blocking*

**H-CCOM-ART-02 - No global combat visual state**
**GIVEN** combat starts and ends in the haunt, **WHEN** runtime rendering state is inspected, **THEN** no combat-state post-process, red vignette, global desaturation, LUT swap, camera shake, or global lighting change is applied by Combat Core.
*Editor-validation + Dev-build smoke | technical-artist + qa-tester | T1-blocking*

**H-CCOM-HUD-02 - HUD output only, no presentation ownership**
**GIVEN** Combat Core is running without Layer 1 HUD implementation, **WHEN** health, mana, cast, target, auto-attack, and hate states change, **THEN** Combat Core exposes data/events through testable accessors and does not instantiate HUD UI, nameplates, floating combat text, or screen-space warning elements.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CCOM-HUD-04 - Attack ON state exposed for HUD feedback**
**GIVEN** the player toggles `Attack` on, off, loses target to death, successfully sits/meds, exits combat, dies, or transitions zones, **WHEN** HUD-facing combat state is inspected, **THEN** Combat Core exposes an explicit Attack ON/OFF state-change signal and current-state accessor so Layer 1 HUD can render a distinct Attack ON visual state; Combat Core does not prescribe the final color, shape, animation, or layout.
*Integration | gameplay-programmer + ui-programmer + qa-tester | T1-blocking*

**H-CCOM-AUD-01 - Audio hooks only, no playback ownership**
**GIVEN** Combat Core emits swing, hit, miss, cast start, interrupt, death, or med start/stop timing events, **WHEN** the runtime scene is inspected, **THEN** Combat Core does not create or control `AudioSource`, `AudioMixer`, music state, warning stingers, low-health alarms, or kill fanfare; Audio System owns playback.
*Editor-validation + Integration | gameplay-programmer + audio-lead + qa-tester | T1-blocking*

### Summary Table

| ID | Covers | Test Type | Owner | Gate |
|---|---|---|---|---|
| H-CCOM-SCOPE-01 | T1 scope exclusions | Editor-validation | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-WS-01 | HauntZone combat enable | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-WS-02 | CityHub combat disable | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-WS-03 | Zone transition cleanup | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-TICK-01 | Fixed combat simulation tick | Unit + Integration | gameplay-programmer, engine-programmer, qa-tester | T1-blocking |
| H-CCOM-PAUSE-01 | Combat hard-stop pause | Integration | gameplay-programmer, engine-programmer, qa-tester | T1-blocking |
| H-CCOM-ACTOR-01 | Actor interface | Unit + Editor-validation | gameplay-programmer | T1-blocking |
| H-CCOM-TGT-01 | Target acquisition | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-AA-01 | Auto-attack toggle | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-AA-02 | Out-of-range swing | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-AA-03 | Pull does not auto-enable Attack | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-F1 | Hit chance formula | Unit | gameplay-programmer | T1-blocking |
| H-CCOM-F2 | Damage formula | Unit | gameplay-programmer | T1-blocking |
| H-CCOM-FIXTURE-01 | Prototype fixture package | Editor-validation + Unit | game-designer, systems-designer, qa-tester | fixture-gated T1-blocking |
| H-CCOM-F2B | Damage fixture extremes | Unit | gameplay-programmer, systems-designer | fixture-gated T1-blocking |
| H-CCOM-PULL-01 | Body/LoS pull | Integration + Dev-build smoke | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-PULL-02 | Social-link assist | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-PULL-03 | LoS query contract | Integration + Dev-build smoke | gameplay-programmer, engine-programmer, qa-tester | T1-blocking |
| H-CCOM-PULL-04 | Social assist pulses | Integration | gameplay-programmer, ai-programmer, qa-tester | T1-blocking |
| H-CCOM-LEASH-01 | Path failure leashing | Integration | gameplay-programmer, ai-programmer, qa-tester | T1-blocking |
| H-CCOM-LEASH-02 | Re-aggro and memory expiry | Integration | gameplay-programmer, ai-programmer, qa-tester | T1-blocking |
| H-CCOM-F3 | Threat formula | Unit | gameplay-programmer | T1-blocking |
| H-CCOM-HATE-01 | Highest threat target | Unit | gameplay-programmer | T1-blocking |
| H-CCOM-HATE-02 | Threat tie deterministic | Unit | gameplay-programmer | T1-blocking |
| H-CCOM-HATE-03 | Healing overtakes damage threat | Unit + Integration | gameplay-programmer, systems-designer | T1-blocking |
| H-CCOM-HUD-01 | Categorical threat output | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-HUD-03 | Threat category thresholds | Unit | gameplay-programmer, systems-designer | T1-blocking |
| H-CCOM-CAST-01 | Valid cast starts | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-CAST-02 | Cast completion spends mana | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-CAST-03 | Cast cancel no mana | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-F4 | Interrupt formula | Unit | gameplay-programmer | T1-blocking |
| H-CCOM-CAST-04 | Damage interrupt | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-CAST-05 | Same-tick cast priority | Unit + Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-IF-01 | Spell Memorization cast lifecycle interface | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-INST-01 | Tactical instant profile contract | Integration + Unit | gameplay-programmer, systems-designer, qa-tester | T1-blocking |
| H-CCOM-MED-01 | Sitting disables auto-attack | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-MED-02 | Sitting mana regen | Unit + Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-MED-03 | Sitting in combat unsafe | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-F5 | Combat exit formula | Unit | gameplay-programmer | T1-blocking |
| H-CCOM-DEATH-01 | PlayerDeathEvent once | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-DEATH-02 | WS corpse-run handoff | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-DEATH-03 | Death payload schema | Unit + Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-KILL-01 | NPC death / kill credit | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-SL-01 | Combat persistence whitelist | Unit | gameplay-programmer | T1-blocking |
| H-CCOM-SL-02 | Invalid hydration fail-loud | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-SL-03 | Spell/cooldown/regen persistence ownership | Unit | gameplay-programmer, systems-designer | T1-blocking |
| H-CCOM-FEEL-01 | Cleric trash solo envelope | Profiled playtest | game-designer, qa-tester | fixture-gated T1-blocking |
| H-CCOM-FEEL-02 | Named not soloable | Profiled playtest | game-designer, qa-tester | fixture-gated T1-blocking |
| H-CCOM-FEEL-03 | Two-trash overpull | Profiled playtest | game-designer, qa-tester | fixture-gated T1-blocking |
| H-CCOM-FEEL-04 | Med-break pacing | Profiled playtest + Integration | game-designer, qa-tester | fixture-gated T1-blocking |
| H-CCOM-ART-01 | Pivot/no marker | Dev-build smoke | game-designer, art-lead, qa-tester | T1-blocking |
| H-CCOM-ART-02 | No global combat visual state | Editor-validation + Dev-build smoke | technical-artist, qa-tester | T1-blocking |
| H-CCOM-HUD-02 | HUD output boundary | Integration | gameplay-programmer, qa-tester | T1-blocking |
| H-CCOM-HUD-04 | Attack ON HUD state | Integration | gameplay-programmer, ui-programmer, qa-tester | T1-blocking |
| H-CCOM-AUD-01 | Audio hook boundary | Editor-validation + Integration | gameplay-programmer, audio-lead, qa-tester | T1-blocking |

**Total: 55 criteria. 49 ordinary T1-blocking. 6 fixture-gated T1-blocking. 0 advisory-at-T1.**

## Non-Goals

- No PvP, duels, faction PvP, friendly fire, or player-vs-player threat.
- No networking, FishNet, replicated combat authority, lag compensation, account identity, server validation, or multiplayer prediction.
- No Warrior, Enchanter, or future player-class implementation.
- No Sister Elara combat behavior, full companion AI, hiring hall, companion relationships, or inverse-population scaling.
- No full spellbook, spell memorization slots, spell learning, spell unlocks, or class spell list content.
- No complete Status Effects matrix, buff stacking, crowd-control durations, or dispel rules.
- No loot tables, item drops, item stat schema, currency economy, or equipment slot legality.
- No XP curve, level-up rules, skill-up rules, or spell unlock progression.
- No corpse-run penalty, XP loss, resurrection, corpse probe, or corpse recovery interaction beyond emitting player death context.
- No raids, boss scripts, scripted encounter starts, instancing, dungeon finder, encounter reset UI, or map/marker affordances.
- No live LLM, generated dialogue, moderation dependency, or dialogue memory.
- No ranged weapon model in T1.

## Open Questions and Handoffs

| Question | Owner | Deadline | Status |
|---|---|---|---|
| **T1 baseline fixture finalization.** Combat Core now supplies prototype low/mid/top Cleric, trash, named, spell, and encounter fixtures. Level / Encounter Design must remap those fixtures to the final T1 haunt level band when that band locks. | `game-designer` + `systems-designer` + `level-designer` | Before tuning signoff | Closed for Combat Core; downstream remap required |
| **Pause semantics integration.** Combat Core now hard-stops Combat-owned timers, movement intents, combat-tied animation progression, and combat audio hook emission. Menus & Settings owns pause entry/input flow. Day/Night and Menus still have open global world-simulation pause rows outside this file. | `game-designer` + `engine-programmer` | Before Pause Menu + Combat implementation | Combat slice closed; cross-doc global pause policy still open outside approved edit scope |
| **Save/Load amendment for combat persisted fields.** Combat Core now defines `current_health`, `current_mana`, `combat_life_state`, optional death handoff payload, and transient exclusions. Save/Load must mirror the whitelist before implementation. | `gameplay-programmer` + `engine-programmer` | Before Save/Load implementation | Closed for Combat Core; downstream amendment required |
| **World Structure / Death payload confirmation.** Combat Core now requires `PlayerDeathEvent(death_payload)` with `death_context_id`, `local_character_id`, `zoneId`, `death_position`, stable `killer_source_ref`, and `death_cause_type`, with T1 sources restricted to NPC/spawn/environmental combat. World Structure and Death & Corpse Recovery must mirror or explicitly supersede this payload when those contracts are implemented. | `gameplay-programmer` + `game-designer` | Before Death & Corpse Recovery GDD | Closed for Combat Core; downstream confirmation required |
| **Temporary spell profile data ownership.** Combat Core now defines `CombatPrototypeSpellProfileSet_T1` and D012 tactical instant fixture contracts for the combat-feel prototype. Class Design and Spell Memorization own final spell lists, memorized slots, spell acquisition, and final numeric cooldown/mana/effect values; Class Design must reverse-list the tactical instant contract when authored. | `game-designer` + `systems-designer` | Before Class Design / Spell Memorization GDDs | Closed for Combat Core; downstream ownership noted |
| **Attack ON HUD treatment.** Combat Core exposes explicit Attack ON/OFF state and state-change signals. Layer 1 HUD owns final visual treatment consistent with art bible restraint and must make Attack ON unmistakable enough to avoid the pinned-prototype "clunky feedback" failure. | `ui-programmer` + `game-designer` | Before Layer 1 HUD implementation | Closed for Combat Core; downstream presentation required |
| **Threat HUD categories.** Combat Core now exposes `NoThreat`, `ThreatListed`, `ThreatClose`, `HasAggroStable`, and `HasAggroContested` using ratio thresholds. Layer 1 HUD owns visual treatment without raw numbers. | `ux-designer` + `ui-programmer` | During Layer 1 HUD GDD | Closed for Combat Core; downstream presentation deferred |
| **Leash/path failure details.** Combat Core now defines T1 path-failure grace, path-pending grace, path-status sample cadence, re-aggro distance, threat-memory expiry, query anchors, LayerMasks, and social-assist pulse rules. Creature / Enemy AI owns return-to-anchor movement implementation. | `ai-programmer` + `gameplay-programmer` | During Creature / Enemy AI GDD | Closed for Combat Core; downstream movement detail deferred |
| **Ranged combat addition.** Future classes may require ranged attacks. This is explicitly out of T1 and should be added through Class Design / Combat Core amendment only when needed. | `game-designer` + `systems-designer` | T2+ class expansion | Deferred |
