# NPC System

> **Status**: Designed (pending review)
> **Author**: Brian + Codex
> **Last Updated**: 2026-04-24
> **Last Verified**: 2026-04-24 - Phase 5 self-check readback
> **Implements Pillar**: Primary - **P1 The World Is Not Your Story** and **P4 Every Companion Is A Person**. Supports - **P2 The Silence Is Sacred**, **P3 Reputation Is The Progression**, and **P5 Stakes Are Honest**.

## Summary

NPC System is the Layer 2 Core framework for named and ambient inhabitants of Gravenspire. It owns NPC identity records, schedule evaluation, active-zone spawning, occupation postures, interaction availability, and the data-only bridge between unloaded zones and visible NPC behavior. It does not own faction-state calculation, combat rules, dialogue content, companion party AI, or UI presentation. In T1, it supports one haunt zone plus one city hub skeleton, one reactive faction baseline, a small set of named NPCs, and enough ambient NPCs for the city to read as inhabited without markers, nameplates, or convenience affordances.

> **Quick reference** - Layer: `Core` . Priority: `MVP` . Key deps: `World Structure; Save / Load & Persistence`

## Overview

NPC System makes Gravenspire's people behave like inhabitants rather than props. It defines how named NPCs and ambient NPCs are identified, scheduled, spawned into the active zone, removed from unloading zones, and persisted as data across sessions. The system consumes World Structure's `ZoneActiveEvent(zoneId, zoneType)`, `ZoneUnloadingEvent(zoneId)`, and `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)`. It must never retain scene-object references into a zone that is unloading or idle.

The T1 design answer to World Structure's `ADR-tba-5` pressure is **data-only delta catch-up plus active-zone ticks**. NPCs do not tick as live MonoBehaviours while their zone is `ZoneIdle`; their `NpcRecord` and schedule definitions are evaluated deterministically when `SessionResumeEvent` arrives and again when their zone becomes active. If the city clock says the lamplighter should be halfway through the gate route, the NPC spawns at the corresponding route anchor when CityHub becomes active. This preserves the "world kept moving" fantasy without keeping GameObjects, Renderers, Colliders, Materials, Textures, or live physics bodies resident in `ZoneIdle`.

T1 scope is deliberately narrow: a city hub skeleton with a handful of named inhabitants, ambient faction-correct civilians, a haunt with creature/person spawn support, and one faction reactive baseline. Sister Elara's mentor/onboarding needs are noted as downstream requirements, but full companion hire/dismiss logic, party role AI, relationship grudges, and inverse-population scaling belong to Named AI Companion Core and later systems. Dialogue remains templated in T1 per `DECISIONS.md` D004; live LLM dialogue is not part of this system.

## Player Fantasy

The player should feel that every person in Gravenspire had somewhere to be before the player arrived. The innkeeper is not standing at the counter because the player entered the inn; they are there because that is the hour when the inn must be kept. A Court courier passing near the gate is not a waypoint in disguise; they are part of a schedule the player can learn through attention. Named NPCs become recognizable through posture, material history, faction vocabulary, repeated behavior, and dialogue, not through overhead labels.

The core fantasy is social literacy. At first, the player cannot reliably tell a stranger from a future ally, a civilian from a hostile faction agent, or a named person from a particularly specific ambient figure. Over time, they learn who stands where at dawn, which faction writes on gray-blue vellum, who keeps their hand near an absent weapon, and which face is worth remembering. NPC System supports Pillar 1 by refusing to make inhabitants wait for the player, and Pillar 2 by refusing to explain them through markers.

### Anchor moments

- **Returning to the city gate at dawn.** A lamplighter is already partway through the route, and a Court courier passes without acknowledging the player. The player understands time has advanced because people are in different places, not because a UI recap says so.
- **Recognizing a named NPC by specificity, not emphasis.** Sister Elara or a Court chamberlain reads through garment history, posture, and repeated behavior. No glow, outline, overhead name, or special camera treatment identifies them.
- **Finding an absence.** An NPC who was usually at the inn is not there after a session resume. A chair remains pulled back, a ledger remains open, or another NPC references their errand through templated dialogue. The absence is diegetic evidence, not a banner.
- **Entering a haunt that does not perform.** Hostile and non-hostile NPC entities occupy routes and posts according to schedule/spawn data. Combat begins through behavior and the pivot, not through a warning marker.

### Anti-fantasy

- NPCs standing motionless forever until the player approaches.
- Quest markers, nameplates, exclamation points, minimap dots, auto-path targets, or proximity barks that identify importance.
- "Welcome back" NPC schedule summaries, elapsed-time recaps, or convenience logs generated by NPC System.
- NPC identity being reduced to utility slots such as "healer hireling" or "merchant vendor."
- Full companion/party systems smuggled into T1 before their GDDs exist.

## Detailed Design

### Core Rules

1. **Data-owned identity.** Every persistent NPC has an `NpcRecord` identified by stable `npcId`. The record may contain `npcId`, `npcArchetypeId`, `displayNameKey`, `knownNameState`, `factionId`, `zoneId`, `homeAnchorId`, `scheduleId`, `scheduleStateId`, `routeProgress`, `availabilityState`, `lastEvaluatedTimestampUtc`, `dialogueTemplateSetId`, and optional relationship/reaction flags. It must not contain GameObject references, Transform references, scene handles, Addressable handles, Colliders, Renderers, Materials, Textures, or runtime component pointers.

2. **NPC categories.**
   - **Named NPCs** are persistent people with stable identity, schedule, faction affiliation, known-name state, dialogue template set, and authored visual specificity.
   - **Ambient NPCs** are faction/material-correct inhabitants generated from zone/faction ambient pools. They may be deterministic within a session but do not receive unique long-term memory in T1.
   - **Creature/person spawn records** describe entities the downstream Creature / Enemy AI and Combat Core systems can activate. NPC System owns placement and inhabitant identity; combat systems own hostility, hate, damage, and ability behavior.
   - **Companion-capable named NPCs** are named NPCs with extra tags that downstream Named AI Companion Core can consume. NPC System does not implement hire/dismiss, party following, role competence, or companion relationships.

3. **T1 content envelope.** T1 supports one `CityHubZone`, one `HauntZone`, one reactive faction baseline, 3-6 named NPCs visible or discoverable across the city hub skeleton, 8-12 ambient city NPCs under normal hub conditions, and haunt spawn support for the Combat Core / Creature AI prototype. These numbers are initial caps, not content promises; the acceptance gate is that the world reads inhabited without violating art/performance constraints.

4. **World Structure event gating.** NPC schedule ticks, spawn enables, interaction enables, and route updates for scene instances are gated by `ZoneActiveEvent(zoneId, zoneType)`. No NPC scene instance may tick before the active-zone event for its zone has been received. `ZoneUnloadingEvent(zoneId)` disables interactions, cancels active scene-instance ticks, serializes any needed data deltas, and clears all outgoing-zone scene references before World Structure releases the zone.

5. **ZoneIdle is data-only.** While a zone is idle, NPC System may retain `NpcRecord` data, schedule definitions, route definitions as serialized data, and optional baked `NavMeshData` references if approved by World Structure's runtime binding ADR. It may not retain live NPC GameObjects, MonoBehaviours, Renderers, Colliders, Materials, Textures, Animator instances, NavMeshAgent components, AudioSources, or physics bodies for the idle zone.

6. **Schedule model.** A schedule is an authored, deterministic list of beats keyed by world time, zone, route anchors, and optional faction-state predicates. A beat may say "at dawn, lamplighter route A, 35% progress" or "during Court hours, chamberlain at hall anchor." Schedule evaluation produces data: target zone, anchor/route progress, posture, availability, and optional organic trace token. Scene movement occurs only after the relevant zone is active.

7. **Active-zone tick cadence.** In an active zone, NPC System evaluates schedule changes on `npc_schedule_tick_interval_seconds` and may update route targets, posture states, and availability. It must not call pathfinding destination updates every frame unless a downstream AI system owns a combat/movement behavior requiring it. Normal civilian schedule movement should update at low cadence and use route progress interpolation.

8. **Session resume catch-up handler.** NPC System ships a T1 `SessionResumeEvent` handler. On receipt, it clamps elapsed time through World Structure's `session_catchup_max_real_seconds_default`, quantizes it by `npc_catchup_step_seconds`, and evaluates schedule records deterministically in data. The handler must run before any NPC `ZoneActiveEvent` spawn enable path. If elapsed time is 0, the handler still records that it ran and produces no schedule delta.

9. **Organic discovery of catch-up.** NPC catch-up produces no UI banner, recap, toast, log pop-up, or welcome-back line. The player discovers catch-up through changed NPC positions, absences, posture/occupation changes, dialogue templates unlocked by schedule state, or Layer 2 world surfaces owned by other systems. NPC System may emit data tokens such as `NpcPresenceTraceRecorded`, but visual/UI presentation belongs to Faction Board, Personal Journal, Dialogue, or world props.

10. **Spawn enable contract.** When a zone becomes active, NPC System instantiates or enables only the named and ambient NPCs whose evaluated records belong in that zone at that time. It uses authored spawn anchors and route anchors, never arbitrary relocation near the player. If the player enters faster than a route can be made visible, NPCs appear at deterministic schedule-valid anchors, not at player-facing convenience positions.

11. **Named NPC specificity.** Named NPCs read through authored material history, posture, face/garment specificity, and low-amplitude signature behavior. The system must not identify named NPCs with glow, outline, overhead name, minimap marker, special lighting, proximity bark, or camera emphasis.

12. **Ambient NPC restraint.** Ambient NPCs communicate faction and occupation through silhouette/material rules, low-amplitude idle loops, and occupation anchors. They do not deliver random proximity chatter, tutorial lines, or repeated atmospheric barks. They are allowed to be quiet.

13. **No player convenience pathing.** NPC schedules may make NPC movement legible, but NPC System must not expose auto-pathing, follow arrows, quest-navigation hints, or "track this NPC" affordances. If the player wants to find someone, they learn schedule patterns through observation, dialogue, faction board/journal surfaces, or world behavior.

14. **Faction reactive baseline.** NPC System owns faction membership tags and reaction-profile hooks. Faction State Simulation owns the actual faction state, reputation values, and control changes. At T1, if Faction State Simulation is not present yet, NPCs use authored baseline faction behavior. Once Faction State Simulation is authored, NPC System consumes its outputs to choose spawn pools, availability, and reaction templates without owning the faction calculation.

15. **Dialogue boundary.** NPC System exposes `NpcInteractionContext` and `dialogueTemplateSetId` to Dialogue System. It does not author dialogue content, render dialogue UI, or call an LLM. T1 dialogue is templated; live LLM behavior is deferred to T3 per D004.

16. **Combat boundary.** NPC System can expose whether an NPC is eligible to become a combat actor in the current zone and state. Combat Core and Creature / Enemy AI own target selection, hate, damage, death, crowd control, and hostile state machines. A civilian can become hostile only through downstream combat/faction rules; NPC System does not silently convert social identity into combat behavior.

17. **Persistence boundary.** Save/Load persists `NpcRecord` data and any NPC-owned schedule state. NPC System owns the schema and hydration validation for that data. Save/Load owns serialization, integrity, versioning, and failure handling. NPC System must reject or fail hydration loudly if persisted NPC data contains invalid ids, out-of-range enum values, unsafe strings, or any runtime-handle field.

17a. **Progression source lifecycle durability.** NPC System owns durable source lifecycle state for any NPC or spawn that can be referenced by Character Progression XP lookup. The persisted `NpcSourceLifecycleRecord` contains only data fields: `zoneId`, `defeated_source_ref`, `source_lifecycle_token`, `source_lifecycle_state` (`Active`, `Defeated`, `RespawnEligible`), `source_lifecycle_token_policy`, and any authored respawn/availability timing key needed to avoid recreating a defeated source immediately after load. It does not contain XP values, progression dedupe keys, Combat runtime actor ids, GameObject references, or Character Progression tombstones. Character Progression owns XP math and transient award dedupe; NPC System owns whether the source lifecycle is alive, defeated, or eligible to respawn across saves.

18. **String safety.** T1 NPC-facing strings use authored localization/template keys. NPC System does not accept player-authored NPC names or free-text memories in T1. If future systems add player-authored labels or notes involving NPCs, those strings must be length-bounded and sanitized before NPC System consumes them.

19. **Sister Elara scope line.** NPC System provides the minimum substrate Sister Elara needs later: stable named identity, schedule, faction affiliation, known-name state, mentor-capable tags, occupation posture, and interaction context. Her first-hour behavior, party following, teaching logic, healing competence, departure timing, and companion relationship state belong to Sister Elara Mentor and Named AI Companion Core.

20. **Tier discipline.** No networking authority, replicated NPC state, shared-server persistence, inverse-population hiring hall, live LLM memory, or multiplayer ownership assumptions are part of T1 NPC System. All such behavior is deferred to the appropriate later-tier GDDs and DECISIONS entries.

## States and Transitions

NPC System has two related state machines: a data-record lifecycle and an active scene-instance lifecycle. The data record may persist while no scene instance exists.

| State | Entry Condition | Exit Condition | Behavior / Memory |
|-------|-----------------|----------------|-------------------|
| `RecordUnhydrated` | Save/Load has not delivered NPC state; new session bootstrapping | Valid NPC data hydrated -> `RecordScheduled`; hydration failure -> `NpcHydrationFailed` | No scene instances. NPC System cannot process `SessionResumeEvent` until required records are hydrated. |
| `RecordScheduled` | Valid `NpcRecord` exists with schedule id, zone id, and availability state | `SessionResumeEvent` -> `CatchupEvaluating`; matching `ZoneActiveEvent` -> `PendingZoneActivation` | Data-only. Holds no scene references. |
| `CatchupEvaluating` | `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` received after hydration | Deterministic schedule evaluation complete -> `RecordScheduled`; invalid schedule data -> `NpcHydrationFailed` | Applies quantized elapsed-time catch-up to NPC schedule records. Produces no UI. |
| `PendingZoneActivation` | `ZoneActiveEvent(zoneId, zoneType)` matches the record's evaluated zone | Spawn/enable succeeds -> `ActiveInZone`; spawn invalid -> `RecordScheduled` with logged unavailable reason | Resolves schedule anchor/route data to active-zone scene anchors. Scene references may be acquired only here. |
| `ActiveInZone` | NPC scene instance created or enabled in the active zone | Player initiates interaction -> `Interacting`; combat system claims actor -> `CombatDelegated`; `ZoneUnloadingEvent` -> `UnloadingPurge` | Active GameObject, Animator, NavMeshAgent/route controller, Collider, AudioSource, and interaction volume may exist. Ticks are gated by active zone. |
| `Interacting` | Player intentionally interacts within allowed range and Dialogue System accepts context | Dialogue ends -> `ActiveInZone`; `ZoneUnloadingEvent` -> `UnloadingPurge`; Combat claim -> `CombatDelegated` | NPC holds current interaction context only. Dialogue UI/content are external. No nameplate or marker is created. |
| `CombatDelegated` | Combat Core / Creature AI claims the NPC as a combat actor | Combat ends and actor survives -> `ActiveInZone`; actor death/despawn recorded -> `RecordScheduled`; `ZoneUnloadingEvent` -> `UnloadingPurge` | NPC System stops owning moment-to-moment behavior except identity/persistence hooks. On actor death/despawn, NPC System records `NpcSourceLifecycleRecord(source_lifecycle_state = Defeated)` and acknowledges Character Progression's same-frame snapshot phase before source cleanup may retire scene references. Combat systems own combat state. |
| `UnloadingPurge` | `ZoneUnloadingEvent(zoneId)` received for an active NPC's zone | All scene references cleared -> `RecordScheduled` | Interactions disabled immediately. Scene-instance state deltas written to `NpcRecord`; GameObject/component references cleared before zone unload completes. |
| `NpcHydrationFailed` | Invalid save data, missing archetype, missing schedule, unsafe string, or runtime-handle field detected during hydration | Load path rejects via Save/Load failure handling | No playable session may start from partial NPC state. Pairs with Save/Load `HydrationFailed`. |

### Transition priority

If multiple events arrive in the same frame, NPC System resolves them in this order:

1. `ZoneUnloadingEvent` for any active NPC zone.
2. Save/Load hydration failure or schema rejection.
3. `SessionResumeEvent` catch-up.
4. `ZoneActiveEvent` spawn enable.
5. Active-zone schedule tick.
6. Player interaction request.
7. Combat delegation request.
8. Combat death/despawn source-lifecycle recording.

This priority prevents outgoing-zone references from surviving because a lower-priority interaction or tick ran during unload.

## Interactions with Other Systems

| System | Inputs Consumed by NPC System | Outputs Published by NPC System | Ownership Boundary | Dependency |
|--------|-------------------------------|---------------------------------|--------------------|------------|
| **World Structure** | `ZoneActiveEvent(zoneId, zoneType)`, `ZoneUnloadingEvent(zoneId)`, `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` | NPC subscriber logs; optional `NpcZoneReady(zoneId)` after spawn enable completes | World Structure owns zone state and event ordering. NPC System owns schedule/spawn response and reference purge. | **Hard** |
| **Save / Load & Persistence** | Hydrated NPC records and NPC-owned progression source lifecycle records; save trigger requests from Foundation flow | `NpcSourceLifecycleSaveBarrier`; current `NpcRecord` and `NpcSourceLifecycleRecord` data; `NpcHydrationFailed` on invalid data | Save/Load owns serialization/integrity. NPC owns NPC schema, source lifecycle durability, and valid/invalid state. | **Hard** |
| **Day/Night Cycle** | Current world time / phase once authored; schedule phase predicates | None required at T1; may publish schedule-readiness diagnostics | Day/Night owns world clock. NPC owns interpreting clock into NPC schedules. | **Soft until Day/Night GDD; hard when authored** |
| **Faction State Simulation** | Faction control/reaction outputs once authored | `NpcPresenceTraceRecorded`, NPC availability/reaction hooks for faction systems | Faction Sim owns faction state and political calculation. NPC owns inhabitant presence and reaction-profile application. | **Soft before Faction Sim; hard at MVP once Faction Sim lands** |
| **Combat Core** | Combat eligibility request, death/despawn outcomes, active zone type from WS indirectly | NPC identity/combat-actor seed, targetable eligibility, social/civilian flags | Combat owns damage, hate, abilities, death. NPC owns identity and non-combat state. | **Hard downstream** |
| **Character Progression** | No XP values consumed; only progression source lifecycle registration requirements from approved Character Progression GDD | Stable `defeated_source_ref`, source lifecycle token, activation/death lifecycle hooks, and persisted `NpcSourceLifecycleRecord` for XP-eligible NPC/spawn sources | Character Progression owns XP lookup, XP math, and transient dedupe. NPC owns stable source refs and durable lifecycle state. | **Hard downstream data boundary** |
| **Creature / Enemy AI** | AI claim/release for hostile actors | Spawn anchors, archetype ids, route/posture seeds | Creature AI owns hostile state machine. NPC owns spawn container and non-combat identity. | **Hard downstream** |
| **Dialogue System** | Dialogue-ended callback; template availability queries once authored | `NpcInteractionContext`, `dialogueTemplateSetId`, `knownNameState`, schedule/faction context | Dialogue owns content, UI, template resolution, and any future LLM. NPC owns who is available to speak and in what state. | **Hard downstream** |
| **Faction Events** | Event-state predicates once authored | NPC presence/absence tokens, named-NPC availability state | Faction Events owns event narratives. NPC owns where people are and whether an event can use them. | **Soft at T1, hard when authored** |
| **Named AI Companion Core** | Companion claim/release once authored | Companion-capable named NPC records and current availability | Companion Core owns hire/dismiss, party following, role AI, and relationship state. NPC owns base named inhabitant identity. | **Hard downstream** |
| **Sister Elara Mentor** | Mentor behavior requests once authored | Sister Elara base record, schedule, posture, faction identity, interaction context | Mentor GDD owns onboarding behavior and teaching beats. NPC owns the person substrate. | **Hard downstream** |
| **Layer 1 HUD / Menus** | None | None | NPC System must not render markers, nameplates, recaps, or convenience UI. | **No direct dependency** |
| **Dialogue UI Panel / Journal / Faction Board** | None directly | Data may be consumed by their owning systems via Dialogue/Faction systems | These are Layer 2 diegetic surfaces. NPC System does not duplicate them. | **Indirect only** |
| **Audio System** | Audio availability once authored | Low-level event hooks such as footsteps/occupation foley requests from active NPCs | Audio owns playback/mix. NPC owns when an active NPC action occurs. | **Soft** |

## Formulas

### NPC Schedule Catch-Up Steps

The `npc_schedule_catchup_steps` formula is defined as:

```text
npc_schedule_catchup_steps = floor(clamp(real_elapsed_seconds, 0, session_catchup_max_real_seconds_default) / npc_catchup_step_seconds)
```

**Variables:**

| Variable | Symbol | Type | Range | Description |
|----------|--------|------|-------|-------------|
| `real_elapsed_seconds` | `E` | int | 0-604800 after WS clamp at default | Payload from `SessionResumeEvent`. |
| `session_catchup_max_real_seconds_default` | `Cmax` | int | 604800 default | World Structure-owned max catch-up clamp. |
| `npc_catchup_step_seconds` | `Q` | int | 30-300; default 60 | NPC-owned quantization step for deterministic schedule catch-up. |
| `npc_schedule_catchup_steps` | `S` | int | 0-20160 depending on Q | Number of deterministic schedule steps to evaluate. |

**Output Range:** 0 to 20160 across the full safe-range quantum [30s-300s] at the 7-day clamp. Default 60s quantum produces 0 to 10080; safe-range minimum 30s quantum produces 0 to 20160 (matches registry `output_range`).
**Example:** If the player returns after 3 days, `floor(259200 / 60) = 4320` schedule steps. The handler evaluates final schedule state deterministically; it does not simulate 4320 visible movements.

### Active NPC Population Load

The `npc_active_population_load` formula is defined as:

```text
npc_active_population_load = named_visible_count + ambient_visible_count + hostile_visible_count
```

**Variables:**

| Variable | Symbol | Type | Range | Description |
|----------|--------|------|-------|-------------|
| `named_visible_count` | `N` | int | 0-8 at T1 | Named NPC scene instances enabled in the active zone. |
| `ambient_visible_count` | `A` | int | 0-16 at T1 | Ambient civilian scene instances enabled in the active zone. |
| `hostile_visible_count` | `H` | int | 0-20 at T1 | Hostile/creature/person spawn records currently active through downstream AI. |
| `npc_active_population_load` | `P` | int | 0-30 at T1 | Total active NPC-controlled scene instances before downstream combat summons or future systems. |

**Output Range:** 0 to 30 under T1 caps. The normal city hub target is 11-18; the normal haunt target is 6-18 depending on encounter density.
**Example:** CityHub with 4 named NPCs and 10 ambient NPCs has `P = 4 + 10 + 0 = 14`, within T1 city hub target.

### Ambient Spawn Count

The `ambient_spawn_count` formula is defined as:

```text
ambient_spawn_count = clamp(round(base_ambient_count * faction_presence_scalar * zone_activity_scalar), ambient_min_count, ambient_max_count)
```

**Variables:**

| Variable | Symbol | Type | Range | Description |
|----------|--------|------|-------|-------------|
| `base_ambient_count` | `B` | int | 0-16 | Authored zone baseline. |
| `faction_presence_scalar` | `F` | float | 0.0-1.5 | Faction State Simulation output when authored; default 1.0 if absent. |
| `zone_activity_scalar` | `Z` | float | 0.0-1.25 | Day/Night or schedule phase modifier; default 1.0 until Day/Night is authored. |
| `ambient_min_count` | `Amin` | int | 0-4 | Minimum ambient count for the zone/phase. |
| `ambient_max_count` | `Amax` | int | 4-16 | Maximum ambient count for the zone/phase. |
| `ambient_spawn_count` | `Aout` | int | 0-16 | Ambient NPCs enabled for this zone/phase. |

**Output Range:** 0 to 16 at T1.
**Example:** A hub baseline of 10 with `F = 1.0`, `Z = 0.75`, `Amin = 4`, and `Amax = 12` produces `round(7.5) = 8` ambient NPCs.

## Edge Cases

- **If `SessionResumeEvent` arrives before NPC records are hydrated**: NPC System enters `NpcHydrationFailed`; Save/Load must reject the load as `HydrationFailed`; no `ZoneActiveEvent` may produce playable NPC state.
- **If `SessionResumeEvent.real_elapsed_seconds` is 0**: the NPC catch-up handler still runs and records no schedule delta; this preserves firing-order testability.
- **If elapsed time exceeds `session_catchup_max_real_seconds_default`**: NPC System uses the clamped payload and does not apply additional hidden catch-up.
- **If system clock skew produces negative elapsed time**: World Structure clamps to 0; NPC System treats the handler as a no-op and emits no organic trace.
- **If `ZoneUnloadingEvent` fires during interaction**: the interaction ends immediately, Dialogue System receives cancellation context if present, and NPC System clears outgoing-zone scene references before unload continues.
- **If `ZoneUnloadingEvent` and an active-zone tick arrive in the same frame**: unload wins; the tick is skipped for that zone.
- **If an NPC route anchor is missing in the active zone**: the NPC is not spawned; the record remains data-valid with `availabilityState = Unavailable_MissingAnchor`; a dev-build error identifies `npcId`, `zoneId`, and `anchorId`.
- **If an ambient spawn anchor is occupied**: the system chooses the next deterministic anchor from the zone's ordered anchor list; if none are free, it reduces ambient count for that tick and logs a non-blocking warning.
- **If pathfinding cannot produce a complete route for a civilian schedule**: the NPC remains at the last valid schedule anchor or spawns at the deterministic fallback anchor for that beat; it does not teleport visibly while on camera.
- **If Faction State Simulation is absent**: NPC System uses authored baseline faction behavior with `faction_presence_scalar = 1.0`; no faction-reactive changes are invented locally.
- **If Faction State Simulation reports a faction state for an unknown faction id**: NPC System ignores the unknown state, logs it, and continues with baseline behavior for known NPCs.
- **If a named NPC is scheduled in an unloaded zone**: only data changes; no GameObject, Animator, NavMeshAgent, AudioSource, Collider, Material, Texture, or physics body is retained.
- **If player proximity would reveal a name the character has not learned**: NPC System exposes generic interaction context only; Dialogue System may reveal the name through content. No overhead name appears.
- **If a player tries to follow an NPC's route as a navigation aid**: NPC movement remains diegetic; the system exposes no auto-follow, path line, marker, or schedule UI.
- **If a hostile NPC is visually identical to a civilian faction member**: behavior state, combat pivot, and downstream combat rules determine hostility; NPC System does not add visual warning affordances.
- **If live LLM dialogue is requested in T1**: NPC System returns only `dialogueTemplateSetId` and static context; no LLM call is made.
- **If ambient NPC count exceeds active population caps after a faction/day-night modifier**: clamp to the cap and prefer named NPC visibility over ambient population.
- **If NPC save data includes a runtime handle field**: hydration fails loudly; the field is treated as schema corruption, not ignored.

## Dependencies

### Upstream

| Dependency | Type | Specific Contract |
|------------|------|-------------------|
| **World Structure** | Hard systems-index dependency | Publishes `ZoneActiveEvent(zoneId, zoneType)`, `ZoneUnloadingEvent(zoneId)`, and `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)`. Defines `ZoneIdle` as metadata-only and forbids retained scene references. |

### Foundation service contracts

| Service | Type | Specific Contract |
|---------|------|-------------------|
| **Save / Load & Persistence** | Hard persistence service | Persists and hydrates `NpcRecord` data only. NPC System owns its schema; Save/Load owns integrity, versioning, and failure handling. |

### Downstream dependents

| Dependent | Needs From NPC System |
|-----------|----------------------|
| **Combat Core** | Targetable/combat-eligible actor identity, zone-valid actor presence, non-combat social flags. |
| **Creature / Enemy AI** | Spawn anchors, archetype ids, initial route/posture state, identity container for hostile actors. |
| **Faction State Simulation** | Faction membership tags, NPC availability/presence hooks, inhabitant container for reactive state. |
| **Faction Events** | Named NPC availability, presence/absence traces, event-eligible participant ids. |
| **Named AI Companion Core** | Companion-capable named NPC substrate, availability state, identity persistence. |
| **Sister Elara Mentor** | Sister Elara as a named NPC with schedule, faction identity, posture, and interaction context. |
| **Dialogue System** | `NpcInteractionContext`, `dialogueTemplateSetId`, `knownNameState`, schedule/faction context. |

## Tuning Knobs

| Knob | Default | Safe Range | Higher Means | Lower Means |
|------|---------|------------|--------------|-------------|
| `npc_schedule_tick_interval_seconds` | 10 seconds | 5-30 seconds | Less CPU/path churn; schedules feel less responsive while active. | More responsive visible schedules; higher CPU/path churn. |
| `npc_catchup_step_seconds` | 60 seconds | 30-300 seconds | Fewer catch-up steps; coarser schedule resolution. | Finer catch-up; more data evaluation work. |
| `cityhub_named_visible_cap_t1` | 6 | 2-8 | More named density; higher art/perf cost. | Quieter hub; weaker named-person fantasy. |
| `cityhub_ambient_visible_cap_t1` | 12 | 4-16 | More inhabited hub; higher draw/animation cost. | Sparser hub; lower social texture. |
| `haunt_active_spawn_cap_t1` | 18 | 6-24 | Denser haunt encounters; higher combat/AI cost. | Sparser haunt; less EQ camp texture. |
| `npc_interaction_range_meters` | 2.0 m | 1.2-3.0 m | Easier interaction; risks convenience feel. | More intentional proximity; may feel fiddly. |
| `npc_route_repath_min_seconds` | 2 seconds | 1-5 seconds | Less pathfinding churn; slower route correction. | Faster correction; more CPU. |
| `npc_unload_purge_frame_budget` | 1 frame | 1-2 frames | More time to clean references; risk against unload ordering. | Stricter unload discipline; harder implementation. |
| `ambient_spawn_seed_salt` | project constant | fixed per build | Different deterministic ambient distribution. | Same; should not change casually because it affects test fixtures. |

## Visual/Audio Requirements

NPC System is governed by the art bible's character rules.

- **No markers.** No overhead icons, nameplates, outlines, glows, rings, shader highlights, minimap dots, or faction-colored convenience indicators.
- **Faction before fantasy.** NPCs communicate social location through material, color, and silhouette before power or importance. A Vampire Court attendant reads Court before they read "important."
- **Named NPCs through specificity.** Named NPCs use higher material/face resolution, posture specificity, and documented micro-signatures. They do not receive special lighting, VFX, or camera priority.
- **Ambient NPC hard limits.** Ambient NPCs use faction-shared bodies, 2 material slots maximum, standard PBR, and 3 LODs per art bible Section 8. Ambient bodies must remain GPU-instancing friendly.
- **Idle amplitude restraint.** Idle loops must pass the stillness test: motion may carry occupation and breath, but it must not pull attention from the scene.
- **Combat pivot support.** NPC System must expose posture/state changes needed for Creature AI and Combat Core to perform the pivot. The pivot is the combat signal; no additional warning VFX is allowed.
- **Sister Elara visual implication.** If Sister Elara appears in T1 onboarding, she must be named-NPC-tier and visually specific. Her mentor affordance is learned through behavior and recognition, not a companion marker.
- **Audio restraint.** NPC System may trigger footsteps, cloth, tool, and occupation foley for active NPCs. It must not trigger proximity barks, rare-spawn stingers, schedule-complete stings, or "important NPC nearby" audio cues. Dialogue audio, if any, is owned by Dialogue/Audio systems.

## UI Requirements

NPC System owns no direct UI.

- No nameplates, quest markers, exclamation points, minimap indicators, auto-pathing, overhead interaction icons, or NPC-tracking UI.
- No `SessionResumeEvent` recap, "while you were away" NPC summary, or schedule digest.
- Interaction availability may be exposed as data to Dialogue System only after intentional player proximity/selection. The presentation belongs to Dialogue UI Panel.
- Known-name state is data, not display permission for overhead labels. The player learns names through dialogue, journal/board content, or repeated observation.
- Faction Board, Personal Journal, and Dialogue UI Panel remain Layer 2 diegetic surfaces owned by their GDDs. NPC System may provide data that those systems transform into world objects, but it must not duplicate their surfaces.
- Accessibility concerns around no-nameplate identification should be handled later through diegetic readability, camera/interaction tuning, audio mix clarity, and optional text scaling on downstream dialogue/journal surfaces, not through marker-like affordances.

## Cross-References

| This Document References | Target Source | Specific Element Referenced | Nature |
|--------------------------|---------------|-----------------------------|--------|
| T1 offline scope | `DECISIONS.md` | D003 Single-Player Offline Through Tier 1 | Tier gate |
| FishNet deferred | `DECISIONS.md` | D002 FishNet Deferred to Tier 2 | Tier gate |
| Templated dialogue | `DECISIONS.md` | D004 LLM Dialogue Scope | Tier gate |
| Zone event contract | `design/gdd/world-structure.md` | `ZoneActiveEvent`, `ZoneUnloadingEvent`, `SessionResumeEvent`, `ZoneIdle` metadata-only | Hard dependency |
| WS ADR-tba-5 resolution | `design/gdd/world-structure.md` | Open Questions ADR-tba-5 resolved by this GDD's Overview, Core Rule 5, and H-NPC-WS-03 | Resolution pointer |
| Session catch-up clamp | `design/registry/entities.yaml` | `session_catchup_max_real_seconds_default` | Registered constant |
| Save data restrictions | `design/gdd/save-load-persistence.md` | Save state categories, no runtime handles, hydration failure handling | Persistence contract |
| Silent resume / no recap | `design/gdd/menus-settings.md` | `SessionResumeEvent` must not surface UI | UI boundary |
| No convenience UI | `design/gdd/game-concept.md` | Pillar 2 and anti-pillars | Pillar rule |
| NPC visual specificity | `design/art/art-bible.md` | Sections 5.2, 5.3, 5.4, 8.6 | Art/perf rule |
| Layer 2 information | `design/art/art-bible.md` | Sections 7.2-7.7 | UI boundary |
| Unity navigation | `docs/engine-reference/unity/modules/navigation.md` | NavMeshAgent, path status, update-frequency guidance | Feasibility reference |
| Unity animation | `docs/engine-reference/unity/modules/animation.md` | Animator Controller, blend trees, culling | Feasibility reference |
| Unity 6.3 caveats | `docs/engine-reference/unity/VERSION.md` | Post-cutoff risk and API verification requirement | Feasibility reference |

## Acceptance Criteria

### World Structure Contract

**H-NPC-WS-01 - ZoneActive gates NPC ticks**  
**GIVEN** a session transition into `CityHubZone`, **WHEN** NPC subscriber logs are inspected, **THEN** `ZoneActiveEvent(zoneId, zoneType)` is received before any NPC schedule tick, spawn enable, route update, or interaction enable occurs in that zone.  
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-NPC-WS-02 - ZoneUnloading purges scene references**  
**GIVEN** active NPCs in an outgoing zone, **WHEN** `ZoneUnloadingEvent(zoneId)` fires, **THEN** NPC interactions are disabled immediately and all outgoing-zone GameObject, Transform, Animator, NavMeshAgent, Collider, Renderer, Material, Texture, AudioSource, and physics-body references are cleared before unload completes.  
*Integration + Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

**H-NPC-WS-03 - ZoneIdle data-only residency**  
**GIVEN** a zone has reached `ZoneIdle`, **WHEN** memory/reference validation runs, **THEN** NPC System retains only serialized NPC records, schedule definitions, route data, and approved metadata references; no live NPC scene instance or render/physics resource from the idle zone remains.  
*Dev-build smoke + Editor-validation | gameplay-programmer | T1-blocking*

### Session Resume Catch-Up

**H-NPC-R13-01 - Handler implementation**  
**GIVEN** a valid save with hydrated NPC records and `last_exit_timestamp_utc`, **WHEN** `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` fires, **THEN** NPC System runs its catch-up handler, clamps/quantizes elapsed time through `npc_schedule_catchup_steps`, and updates schedule records deterministically before any NPC spawn enable path.  
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-NPC-R13-02 - Firing order observed by NPC subscriber**  
**GIVEN** a session load from a valid save fixture, **WHEN** NPC System records its event log, **THEN** NPC System observes `SessionResumeEvent` before any `ZoneActiveEvent` and before any active-zone NPC tick.  
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-NPC-R13-03 - Organic discovery without banner**  
**GIVEN** NPC catch-up changes at least one city NPC's schedule state while the player was offline, **WHEN** the player resumes and reaches the relevant active zone, **THEN** the change is visible only through diegetic state such as changed position, absence, occupation posture, or downstream dialogue/Layer 2 content; NPC System produces no UI banner, recap, toast, elapsed-time message, or marker.  
*Integration + Dev-build smoke | game-designer + qa-tester | T1-blocking*

**H-NPC-R13-04 - Determinism across identical saves**  
**GIVEN** two identical save fixtures with the same `last_exit_timestamp_utc`, NPC records, schedule definitions, and active date/time, **WHEN** both sessions process the same `SessionResumeEvent`, **THEN** all resulting `scheduleStateId`, `routeProgress`, `availabilityState`, and organic trace tokens match exactly.  
*Unit | gameplay-programmer | T1-blocking*

### Persistence Contract

**H-NPC-SL-01 - NPC schema contains data only**  
**GIVEN** a save fixture containing named and ambient NPC state, **WHEN** the serialized NPC payload is inspected, **THEN** it contains only data-owned fields and contains no scene handles, Addressable handles, GameObject references, component references, Materials, Textures, Colliders, Renderers, or runtime pointers.  
*Unit | gameplay-programmer | T1-blocking*

**H-NPC-SL-02 - Invalid NPC hydration fails loud**  
**GIVEN** a save fixture with an invalid `npcId`, missing `npcArchetypeId`, missing `scheduleId`, out-of-range enum, or runtime-handle field, **WHEN** Save/Load hydrates NPC System, **THEN** NPC System returns `NpcHydrationFailed`; Save/Load emits `LoadRejected(HydrationFailed)`; no playable session reaches `ZoneActiveEvent`.  
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-NPC-SL-03 - Safe string handling**  
**GIVEN** a malformed NPC-facing string key or future player-authored NPC string containing control characters or exceeding bounds, **WHEN** NPC System validates the payload, **THEN** the value is rejected or sanitized before any downstream Dialogue/UI system receives it.  
*Unit | gameplay-programmer | T1-blocking*

**H-NPC-SL-04 - Valid NPC hydration and pre-SessionResume readiness**  
**GIVEN** a valid save fixture containing NPC-owned `NpcRecord` data for named and ambient NPCs, **WHEN** Save/Load enters `Resuming`, **THEN** NPC System hydrates the records before it observes `SessionResumeEvent`; programmatic accessors report post-hydration `npcId`, `scheduleStateId`, `routeProgress`, `availabilityState`, `knownNameState`, and `lastEvaluatedTimestampUtc` matching the fixture; NPC catch-up runs on those hydrated records; and no `ZoneActiveEvent` enables NPC gameplay until NPC System reports readiness.  
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-NPC-SL-05 - Progression source lifecycle persists with XP-relevant deaths**
**GIVEN** a Combat-delegated NPC or spawn dies and is eligible for Character Progression kill-credit lookup, **WHEN** Save/Load requests NPC state in the same frame or later, **THEN** `NpcSourceLifecycleSaveBarrier` settles the death/despawn outcome before serialization; the saved `NpcSourceLifecycleRecord` contains the defeated `zoneId`, `defeated_source_ref`, `source_lifecycle_token`, lifecycle state, lifecycle policy, and respawn/availability timing key; and no runtime actor id, XP value, or progression dedupe key is persisted by NPC System.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

### Population And Scheduling

**H-NPC-F1 - Catch-up formula bounds at default and safe-range-minimum quantum**  
**GIVEN** elapsed values at 0, 1 second, 60 seconds, 3 days, 7 days, and above 7 days, **WHEN** `npc_schedule_catchup_steps` is evaluated at default `npc_catchup_step_seconds = 60` and safe-range minimum `npc_catchup_step_seconds = 30`, **THEN** the 60s quantum outputs are respectively 0, 0, 1, 4320, 10080, and 10080 after World Structure's default clamp, and the 30s quantum outputs are respectively 0, 0, 2, 8640, 20160, and 20160.  
*Unit | gameplay-programmer | T1-blocking*

**H-NPC-F2 - Active population cap**  
**GIVEN** a T1 zone spawn evaluation, **WHEN** `npc_active_population_load` would exceed the zone cap, **THEN** ambient NPC count is reduced before named NPCs are removed, and the final active count is within the configured cap.  
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-NPC-F3 - Ambient spawn formula clamps**  
**GIVEN** authored ambient inputs with faction/day-night scalars below and above normal ranges, **WHEN** `ambient_spawn_count` is evaluated, **THEN** the output is rounded and clamped between `ambient_min_count` and `ambient_max_count`.  
*Unit | gameplay-programmer | T1-blocking*

**H-NPC-SCHED-01 - No visible teleport on active route failure**  
**GIVEN** an active visible NPC whose next route path is invalid, **WHEN** schedule evaluation attempts to move them, **THEN** they remain at the last valid schedule anchor or transition off-camera through a deterministic fallback; they do not visibly teleport in front of the player.  
*Integration + Dev-build smoke | gameplay-programmer + qa-tester | T1-blocking*

### Pillar And Presentation Compliance

**H-NPC-P2-01 - No marker affordances**  
**GIVEN** any named NPC, ambient NPC, hostile person, or companion-capable named NPC is visible, **WHEN** the scene is inspected in gameplay, **THEN** there are no overhead names, quest icons, exclamation points, minimap dots, outlines, glows, rings, shader highlights, auto-path lines, or "track NPC" affordances.  
*Dev-build smoke | game-designer + qa-tester | T1-blocking*

**H-NPC-P2-02 - No proximity barks for importance**  
**GIVEN** the player walks within interaction range of named and ambient NPCs, **WHEN** no intentional interaction is initiated, **THEN** NPC System does not trigger proximity tutorial barks, importance barks, or marker-like audio cues.  
*Integration + Dev-build smoke | qa-tester | T1-blocking*

**H-NPC-ART-01 - Ambient NPC art budget**  
**GIVEN** an ambient NPC prefab, **WHEN** asset validation runs, **THEN** it has no more than 2 materials, uses faction-shared body material strategy, includes required LODs, and stays within the art bible ambient NPC budget.  
*Editor-validation | technical-artist + qa-tester | T1-blocking*

**H-NPC-ART-02 - Named NPC specificity without emphasis**  
**GIVEN** a named NPC prefab and scene placement, **WHEN** reviewed against the art bible, **THEN** the NPC reads through material/face/posture specificity and documented micro-behavior, not through lighting, VFX, UI, or camera emphasis.  
*Dev-build smoke | art-lead + game-designer | T1-blocking*

### Cross-System Boundaries

**H-NPC-DLG-01 - Templated dialogue context only at T1**  
**GIVEN** the player intentionally interacts with an NPC in T1, **WHEN** NPC System provides dialogue data, **THEN** it provides `NpcInteractionContext` and `dialogueTemplateSetId` only; no live LLM call, LLM memory write, or moderation dependency is invoked.  
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-NPC-COMBAT-01 - Combat delegation boundary**  
**GIVEN** an NPC becomes combat-eligible in a `HauntZone`, **WHEN** Combat Core / Creature AI claims the actor, **THEN** NPC System stops owning combat decisions and retains only identity/persistence hooks; damage, hate, death, and hostile AI state are owned by downstream systems.  
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-NPC-CPRO-01 - Progression snapshot precedes source cleanup**
**GIVEN** Combat emits kill credit for a defeated NPC/spawn source that has an active NPC source lifecycle token, **WHEN** the same-frame kill-resolution event log is inspected, **THEN** NPC System records the defeated `NpcSourceLifecycleRecord`, Character Progression captures its `XpAwardResolutionSnapshot`, and only then may NPC cleanup/despawn retire scene references or allow a respawn with a new lifecycle token.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-NPC-FACTION-01 - Faction fallback baseline**  
**GIVEN** Faction State Simulation is absent or not yet authored, **WHEN** NPC System evaluates faction reactive hooks, **THEN** it uses authored baseline behavior with `faction_presence_scalar = 1.0` and does not invent faction-state changes locally.  
*Unit | gameplay-programmer | T1-blocking until Faction State Simulation lands*

### Summary Table

| ID | Covers | Test Type | Owner | T1-Blocking |
|----|--------|-----------|-------|-------------|
| H-NPC-WS-01 | ZoneActive gating | Integration | gameplay-programmer, qa-tester | Yes |
| H-NPC-WS-02 | ZoneUnloading purge | Integration + Editor-validation | gameplay-programmer, qa-tester | Yes |
| H-NPC-WS-03 | ZoneIdle data-only | Dev-build smoke + Editor-validation | gameplay-programmer | Yes |
| H-NPC-R13-01 | SessionResume handler | Integration | gameplay-programmer, qa-tester | Yes |
| H-NPC-R13-02 | NPC firing order | Integration | gameplay-programmer, qa-tester | Yes |
| H-NPC-R13-03 | Organic discovery | Integration + Dev-build smoke | game-designer, qa-tester | Yes |
| H-NPC-R13-04 | Catch-up determinism | Unit | gameplay-programmer | Yes |
| H-NPC-SL-01 | Data-only schema | Unit | gameplay-programmer | Yes |
| H-NPC-SL-02 | Hydration failure | Integration | gameplay-programmer, qa-tester | Yes |
| H-NPC-SL-03 | String safety | Unit | gameplay-programmer | Yes |
| H-NPC-SL-04 | Valid NPC hydration and pre-SessionResume readiness | Integration | gameplay-programmer, qa-tester | Yes |
| H-NPC-SL-05 | Progression source lifecycle persistence | Integration | gameplay-programmer, qa-tester | Yes |
| H-NPC-F1 | Catch-up formula at both quanta | Unit | gameplay-programmer | Yes |
| H-NPC-F2 | Active population cap | Integration | gameplay-programmer, qa-tester | Yes |
| H-NPC-F3 | Ambient spawn formula | Unit | gameplay-programmer | Yes |
| H-NPC-SCHED-01 | Route failure | Integration + Dev-build smoke | gameplay-programmer, qa-tester | Yes |
| H-NPC-P2-01 | No marker affordances | Dev-build smoke | game-designer, qa-tester | Yes |
| H-NPC-P2-02 | No proximity barks | Integration + Dev-build smoke | qa-tester | Yes |
| H-NPC-ART-01 | Ambient budget | Editor-validation | technical-artist, qa-tester | Yes |
| H-NPC-ART-02 | Named specificity | Dev-build smoke | art-lead, game-designer | Yes |
| H-NPC-DLG-01 | T1 templated dialogue boundary | Integration | gameplay-programmer, qa-tester | Yes |
| H-NPC-COMBAT-01 | Combat boundary | Integration | gameplay-programmer, qa-tester | Yes |
| H-NPC-CPRO-01 | Character Progression source snapshot ordering | Integration | gameplay-programmer, qa-tester | Yes |
| H-NPC-FACTION-01 | Faction fallback baseline | Unit | gameplay-programmer | Yes |

**Total: 24 criteria. 24 T1-blocking.**

## Open Questions

| Question | Owner | Deadline | Status |
|----------|-------|----------|--------|
| **ADR-tba-5 - Hub NPC Schedule Tick Semantics resolution.** This GDD is the resolution authority for World Structure ADR-tba-5. During-session answer: NPC System performs no live ticks while a zone is `ZoneIdle`; schedule state remains data-only and is evaluated when the zone becomes active. Between-session answer: Rule 13 `SessionResumeEvent` delta catch-up updates `NpcRecord` data deterministically before any `ZoneActiveEvent` spawn path. Acceptance criterion for ADR-tba-5: H-NPC-WS-03. Concrete Unity substrate (plain C# service, ScriptableObject schedule assets, ECS later, or hybrid) remains an implementation decision before NPC implementation. | `gameplay-programmer` + `ai-programmer` | Before NPC implementation | Resolved at GDD level 2026-04-24; implementation substrate still open |
| **ADR-tba - Route interpolation for unloaded zones.** Decide whether route progress uses authored route percentages only, baked NavMeshData queries, or a custom route graph while `ZoneIdle` remains data-only. | `ai-programmer` + `unity-specialist` | Before city schedule implementation | Open - T1-blocking |
| **T1 named NPC roster.** Finalize the initial named set: Sister Elara implications, innkeeper, Court contact, faction-board handler, haunt named entity, and any minimum dialogue-critical NPCs. | `game-designer` + `narrative-director` | Before Dialogue System GDD | Open |
| **Sister Elara scope split.** Confirm exactly which fields live in NPC System versus Named AI Companion Core and Sister Elara Mentor, especially first-hour guided movement and post-onboarding absence. | `game-designer` + `ai-programmer` | Before Named AI Companion Core GDD | Open |
| **Faction reaction payload.** Faction State Simulation must define the exact faction-state outputs NPC System consumes for `faction_presence_scalar`, reaction profile changes, and availability shifts. | `systems-designer` + `ai-programmer` | During Faction State Simulation GDD | Open |
| **No-nameplate accessibility validation.** Playtest whether players can identify important NPCs through diegetic signals alone; if not, solve with stronger material/posture/dialogue literacy before considering any marker-like fallback. | `ux-designer` + `accessibility-specialist` + `game-designer` | T1 playtest | Open |
| **Ambient population perf.** Validate T1 hub caps against Unity 6.3 URP character animation/render cost and the art bible draw-call assumptions. | `technical-artist` + `performance-analyst` | T1 city hub prototype | Open |
| **Audio event scope.** Decide which occupation foley hooks NPC System emits at T1 and which wait for Audio System. | `audio-director` + `sound-designer` | Before Audio System GDD | Open |
