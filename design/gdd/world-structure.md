# World Structure

> **Status**: In Design
> **Author**: Claude Code (session with brian, 2026-04-22)
> **Last Updated**: 2026-04-22
> **Last Verified**: 2026-04-22
> **Implements Pillar**: Primary — **P2 The Silence Is Sacred** (spatial pacing, slow travel, no markers, tension through stillness). Supports — **P1 The World Is Not Your Story** (world persists and streams regardless of player presence), **P3 Reputation Is The Progression** (via Zone Control — kills shift faction camp ownership), **P5 Stakes Are Honest** (corpse-run camera-stack boundary, cross-zone corpse recovery).

## Summary

World Structure is the spatial and memory-management infrastructure that defines Gravenspire's zones, transitions, and persistence boundaries. Players do not interact with it directly; every other system (combat, faction simulation, NPC behavior, save/load, audio) depends on it for the authoritative answers to "where am I?" and "what's loaded right now?" MVP scope is one haunt zone and one city hub, configured as separate Addressable streaming groups.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `None (Layer 1 — no upstream dependencies)`

## Overview

World Structure is the spatial architecture of the game world: it defines zones (as discrete Addressable streaming groups per [art bible §8.7](../art/art-bible.md)), manages how they are loaded, unloaded, and transitioned between, enforces the ≤350 MB texture-residency budget during normal play ([§8.9](../art/art-bible.md)), and holds the rule that zone-boundary color-temperature shifts are material-and-light-driven rather than post-process LUT overrides ([§4.4](../art/art-bible.md)). It owns the persistence boundary where "the player is in zone X at position P" becomes serializable save-file state. Every gameplay system downstream — combat, faction simulation, NPC behavior, save/load, audio — depends on it for authoritative answers to "where am I?", "what's loaded?", and "what changes when I cross this boundary?" The system is invisible in play: players never interact with it directly. They experience what it enables — the slow, marker-less traversal that supports [Pillar 2](game-concept.md) (Silence Is Sacred); a world that persists and evolves across session boundaries, supporting [Pillar 1](game-concept.md) (The World Is Not Your Story); and zone-scoped consequences of player action via the downstream Zone Control system. Tier 1 MVP scope is one haunt zone (Unrest-register mansion, 2–3 floors) plus one city hub skeleton (Gravenspire itself), configured as distinct streaming groups. The architecture must accommodate 3–5 zones at Tier 4 without structural rework.

## Player Fantasy

Gravenspire's world structure is felt not as an architecture but as a *testimony*. The player feels they are a guest in a city that kept its own hours before they arrived and will keep them after they log out — lamplighters making their round at dawn, Court couriers crossing paths indifferently, shrines re-dressed between visits. Transitions are slow and material-driven (stone giving way to damp cobble, lantern-warm interiors cooling toward charcoal alleys), so *going somewhere is a thing that takes time and changes the player slightly*. The constrained texture residency budget is felt as **focus**, not as limitation — places become memorable the way streets in a real neighborhood do, through the returning eye finding the same worn stone, the same Court sigil bleeding through whitewash. Persistence makes [Pillar 1](game-concept.md) (The World Is Not Your Story) tactile: when the player returns after three days away, banners have changed, notice-board writs have accumulated, a body left in an alley has been *found*. The emotional register is what Susanna Clarke's *Piranesi* calls "the House" — a place whose rules predate the player, whose kindness, when it comes, is earned.

### Anchor moments

- **Return to the city gate at dawn after a night in the haunt.** The lamplighter is already on his round. A Court courier passes heading the other way. Neither acknowledges the player. This is arrival as threshold, not as event.
- **Walking from the cathedral district into the lower wards.** No loading screen, no skybox swap. The air gets colder *in the materials*. The player notices they've arrived somewhere different the way they notice the weather turning.
- **Returning to the inn after three real-world days offline.** The innkeeper doesn't greet the player warmly. A rival they angered last session has posted their writ on the notice-board in their absence. Pillar 1 made legible.

### Anti-fantasy — what the player should NOT feel

- **Loading-screen-as-time-wasted** — transitions are gradual and material, never a hard-cut grade override ([art bible §4.4](../art/art-bible.md)).
- **Zone-as-theme-park-ride** — no "now entering: the Blood District" banners, no ambient NPC lines triggered by proximity, no skybox swaps.
- **World-as-prepared-for-me** — if a candle is lit, someone lit it on a schedule the player doesn't know. The world is not performing.
- **Named-player-as-protagonist** — no "welcome back, hero" on return. No quest log suggesting the world paused while the player was offline.

### Reference register

Susanna Clarke (*Piranesi* — "the Beauty of the House is immeasurable"), Shirley Jackson (*Haunting of Hill House* — "not sane, stood by itself against its hills"), Caspar David Friedrich (figures small against out-scaling landscapes). Explicitly not: heroic fantasy power-player framing; theme-park MMO spatial design.

## Detailed Design

### Core Rules

1. **Zone Definition.** A zone is exactly one Unity Addressable streaming group. "Zone" and "streaming group" are synonyms. The smallest legal zone boundary is a group boundary; there is no sub-zone concept in World Structure.

2. **Zone Types.** Two zone types exist at MVP: `HauntZone` and `CityHubZone`. `HauntZone` is session-scoped danger space: faction mob spawns are live, Zone Control is active, corpse recovery is in scope. `CityHubZone` is safe space: no combat AI spawns, Zone Control inactive, NPC schedules run on Day/Night Cycle ticks. The type is a behavioral tag consumed by downstream system subscriptions.

3. **Single Active Zone Invariant.** Exactly one zone may be in state `ZoneActive` at any moment. This enforces the ≤350 MB texture-residency budget ([art bible §8.9](../art/art-bible.md)), which assumes only one streaming group's texture set is fully resident at a time. The hub and haunt are never simultaneously active.

4. **Player Position Is Authoritative.** The player's world-space position is the single source of truth for "which zone the player is in." Zone membership is re-derived from position on save/load; no separately cached zone ID.

5. **Transition Initiation via Trigger Volumes.** Zone transitions begin when the player enters a designated transition trigger volume (a door frame, archway, or staircase landing — authored geometry, not an invisible wall). There are no procedural seams. The player keeps full locomotion control during the pre-load window (see Rule 8).

6. **Unload Policy.** The outgoing zone begins unloading after the incoming zone reaches `ZoneActive`. No simultaneous load/unload. Unload is triggered by successful active-confirm, not by trigger-volume exit — prevents crash-to-menu if the player turns back mid-transition.

7. **Cross-Zone Reference Policy.** Cross-zone reads of persistent state (faction standings, NPC states, world events) are permitted via the Save/Load service (always in memory, not zone-scoped). Direct scene-graph references across zone boundaries are forbidden. An NPC in Zone A may query faction state for Zone B via Save/Load, but may not hold a Unity object reference to an object in Zone B.

8. **Save-on-Transition Boundary.** Player position and zone membership are written to the save file at the moment the incoming zone reaches `ZoneLoading` — the last stable save point before the transition commits. If the app crashes during load, the save reflects the pre-transition state; the player resumes in the outgoing zone.

9. **In-Flight Effects on Zone Cross.** On zone boundary cross, World Structure fires `ZoneTransitionBeginEvent`. Combat Core owns the resolution: in-flight status effects on the player resolve to natural expiry or are stripped per its rules, mid-cast spells cancel. Prevents cross-zone spell exploits and simplifies combat cleanup.

10. **Color Temperature Is Not Owned Here.** World Structure does not modify post-process volumes, LUT references, or global lighting states at zone boundaries ([art bible §4.4](../art/art-bible.md) lock). Temperature shifts are handled by the material and light-source inventory of the incoming zone's scene assets. World Structure raises no visual event at zone crossing — the player notices the change through geometry and material, not a system signal.

11. **Tier-Scalability Contract.** All rules above must hold at Tier 4 (3–5 zones) without structural change. Zone list is data-driven (`ZoneManifest` ScriptableObject, not hardcoded scene names). Zone types may be extended (e.g., future `WildernessZone`) without modifying the state machine or transition rules.

12. **Save-on-Transition Hard Timeout.** Rule 8's save-on-transition must complete within `save_mutex_max_ms` (tuning knob, Section G; target <150 ms). If `SaveWriteConfirmed` is not received within that window, World Structure fires `SaveTimedOutEvent`, transitions to `ZoneError`, and logs elapsed time. Prevents the "game freezes at the door" failure mode on slow disks (Section E A4).

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior / Memory |
|-------|-----------------|----------------|-------------------|
| `ZoneIdle` | Zone's group loaded into memory; player not present | Player triggers transition toward this zone (→ `ZoneLoading`); or explicit unload (→ `ZoneUnloading`) | Full texture budget resident; NPC schedules ticking; Day/Night live. At MVP only the hub holds this state: the haunt cold-starts on first player enter and is fully released after exit. |
| `ZoneLoading` | Player enters incoming zone's trigger volume | Addressable load complete + player repositioned (→ `ZoneActive`); or load fails (→ `ZoneError`) | **Save-on-transition written HERE (Rule 8).** Texture budget temporarily overrun — capped by `zone_overrun_window_seconds` tuning knob (target <5s). Player locked to outgoing-zone position. |
| `ZoneActive` | Incoming zone load complete | Player exits toward another zone (→ `ZoneUnloading` for this zone); or death event (→ `CorpseRunActive`) | Full systems subscribed; combat AI live (`HauntZone` only); NPC schedules live; audio zone profile active. |
| `ZoneUnloading` | Previously-active zone, incoming zone has just reached `ZoneActive` | Addressable unload completes (→ `ZoneIdle` for hub; fully released for haunt) | Trigger volume disabled; player cannot re-enter mid-unload. |
| `CorpseRunActive` | Substate of `ZoneActive`. Player death event from Combat Core | Corpse recovery confirmed by Death & Corpse Recovery; or corpse-run timeout (Section G tuning knob) | Zone rules unchanged. If corpse is in a *different* zone than player's current location: that zone must hold at least `ZoneIdle` for the duration (corpse position stays live for recovery probe). |
| `SaveCheckpointing` | Short mutex. Fires on: zone-transition initiation (Rule 8), manual save, periodic autosave tick | Save/Load confirms `SaveWriteConfirmed` | Target duration <150 ms (tuning knob `save_mutex_max_ms`). World Structure pauses transition acknowledgment until confirm. |
| `ZoneError` | `ZoneLoading` fails (bundle corrupted, disk full, group missing from catalog) | Non-dismissible UI error; no automatic recovery. Player returns to title via manual action. | Terminal state for the session. Prevents further transitions. Surfaces to Menus & Settings; logs to session audit trail. |

`CrossZoneTransit` is a conceptual label for the sequenced pair (`ZoneLoading` incoming + `ZoneUnloading` outgoing), not a distinct enum value.

### Interactions with Other Systems

| System | Published (this system emits) | Subscribed (this system consumes) | Interface Owner | Hard/Soft |
|---|---|---|---|---|
| **Save / Load** (§2) | `PlayerZoneMembership` (zoneId + `Vector3` position + zoneType), `ZoneTransitionTimestamp` — written on `SaveCheckpointing`. | `SaveWriteConfirmed` (unblocks transition). On load-resume: player position + zone ID to restore world state. | Save/Load owns serialization format; World Structure owns the data contract. | **Hard** |
| **NPC System** (§4) | `ZoneActiveEvent(zoneId, zoneType)`, `ZoneUnloadingEvent(zoneId)` — gates schedule ticks and spawn enables. | None. | World Structure publishes; NPC subscribes. | **Hard** |
| **Combat Core** (§7) | `ZoneTransitionBeginEvent` (triggers Rule 9). `ZoneType` of active zone (Combat gates combat-enable to `HauntZone`). | `PlayerDeathEvent` (→ `CorpseRunActive`). | World Structure publishes zone events; Combat Core owns combat-enable gate. | **Hard** |
| **Faction State Simulation** (§15) | `ZoneType` + `ZoneId` of active zone. | `FactionControlChanged(zoneId)` — flags zone's NPC spawn config stale. | Faction Sim owns faction state; World Structure owns zone-scope filter. | **Soft at MVP** (reactive). **Hard at T3** (autonomous between sessions). |
| **Zone Control** (§17) | `ZoneActiveEvent(zoneId)` (activates kill-weight attribution). | `ZoneFactionOwnerChanged(zoneId, newFaction)` — persisted via Save/Load. | Zone Control owns ownership calc; World Structure persists result. | **Hard** |
| **Audio System** (§32) | `ZoneActiveEvent(zoneId, zoneType)`, `ZoneTransitionBeginEvent`. | None. | World Structure publishes; Audio subscribes. | **Soft** (audio degrades to silence if missing — art bible P2 "Stillness Is The Signal" tolerates). |

**Day/Night Cycle (§5) — peer, not child.** Day/Night maintains a world-clock that runs whether any zone is active (ticks during `ZoneIdle`, enabling "the lamplighter is already on his round when you return"). World Structure does NOT own Day/Night. **Day/Night subscribes to `ZoneActiveEvent`** (the same event NPC System, Combat Core, and Zone Control consume) to apply zone-specific schedule offsets (haunt schedules differ from hub). No dedicated Day/Night-only event exists; one shared event is sufficient for the offset lookup.

### Scope boundary — deferred to ADRs

These are implementation architecture decisions, not design rules — they will become ADRs after this GDD is approved:

1. **ADR-tba — Zone Scene Topology.** Persistent-hub + additive Addressable haunt scenes (provisional; recommended by `engine-programmer` specialist consult).
2. **ADR-tba — Corpse-Run Camera Stack Configuration.** URP camera-stack approach for per-camera desaturation ([art bible S2 State 7](../art/art-bible.md)). Medium-confidence feasible; **prototype required** before commit.
3. **ADR-tba — World State Serialization Contract.** `WorldStateRecord` POCO DTO shape; interaction with [save-integrity rule](../../.claude/rules/save-integrity.md).
4. **ADR-tba — Zone Transition Mechanism.** Trigger-volume stream-ahead + door-prefab commit point; GPU Resident Drawer registration-frame mitigation. **GRD × Addressables unload behavior in 6.3 is unknown — prototype required.**
5. **ADR-tba — Hub NPC Schedule Tick Semantics.** Real-time tick vs. delta-time catch-up on zone entry. Significant downstream implications for Faction State Simulation and NPC System. Not a design rule; belongs in ADR before T1 implementation begins.

## Formulas

Section D specifies the constraint equations and time budgets that govern World Structure. These are infrastructure formulas, not scaling curves — damage, XP curves, and faction progression live in downstream GDDs. The formulas here produce budgets, derived constraints, and accountability decompositions.

### F1 — Peak Texture Residency (Constraint Equation)

The primary budget enforcement formula. Answers: "does this zone configuration fit in memory?"

`R_peak = R_always + R_active + R_transient`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|----------|--------|------|-------|-------------|
| Always-loaded bytes | `R_always` | float (MB) | 0–350 | Resident bytes in the `AlwaysLoaded` group at all times. MVP baseline: ~45 MB. |
| Active-zone bytes | `R_active` | float (MB) | 0–305 | Resident bytes for the single `ZoneActive` streaming group. |
| Transient-overlap bytes | `R_transient` | float (MB) | 0–`R_active` | Outgoing zone bytes still resident during `ZoneLoading`. Bounded by `R_active` because Rule 3 permits only one transition at a time. |
| Peak residency | `R_peak` | float (MB) | 0–unbounded | Peak resident bytes at the worst moment (mid-transition). |

**Output Range:** Must satisfy `R_peak ≤ 350 MB` ([art bible §8.9](../art/art-bible.md) hard cap). Violation is a **build-blocking error**, not a runtime clamp.

**Example (MVP CityHub → HauntMansion transition):** `R_always` = 45 MB + `R_active` = 140 MB + `R_transient` = 125 MB = **`R_peak` = 310 MB** — within 350 MB cap, 40 MB headroom.

**Extreme behavior:** Violation requires zone-group split, lower-res LODs, or trimming `R_always`. The `zone_overrun_window_seconds` tuning knob (Section G) bounds the *duration* of overrun, not magnitude — F1 must be satisfied at authoring time, not runtime.

### F1.1 — Zone Art Budget Ceiling (Corollary to F1)

The maximum budget any single zone can consume. Derived by rearranging F1 for the worst-case transition (outgoing zone = largest zone):

`R_zone_max = R_cap − R_always − R_transient_max`

Where `R_cap` = 350 MB and `R_transient_max` is the largest zone's texture footprint.

**Example (MVP, `R_transient_max` = HauntMansion at 140 MB):** `R_zone_max` = 350 − 45 − 140 = **165 MB per zone maximum**. CityHub (125 MB) has 40 MB headroom. HauntMansion (140 MB) is at the ceiling — no further density possible without trimming `R_always` or splitting the group. Art leads need this number before committing to zone density.

### F2 — Stream-Ahead Trigger Distance

Minimum distance from the zone boundary at which the trigger volume must be placed, so the incoming zone finishes loading before the player crosses the threshold at maximum speed.

`D_trigger = V_max × T_load + D_margin`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|----------|--------|------|-------|-------------|
| Max player speed | `V_max` | m/s (float) | 0–10 | Player's maximum sustained movement speed (running; sprint if added). Measured input, not tunable. |
| Zone load time | `T_load` | s (float) | 0–∞ | Addressable group load time, profiled on minimum-spec hardware. Measured, not tunable. |
| Safety margin | `D_margin` | m (float) | 1–5 | Buffer for frame-timing variance + save-on-transition write. **Tuning knob** (Section G). |
| Trigger distance | `D_trigger` | m (float) | 0–∞ | Minimum distance from zone boundary for trigger-volume placement. |

**Output Range:** Unbounded upward. Values <5 m are a design smell (trigger too close to boundary). Values >40 m indicate the zone takes too long to load — split the group instead.

**Example (MVP):** Running speed = 5 m/s, profiled `T_load` = 3 s, `D_margin` = 2 m.
`D_trigger` = 5 × 3 + 2 = **17 m**.

**Extreme behavior:** Teleport or a future mount at 2× `V_max` invalidates existing trigger placements. `T_load` must be re-profiled on minimum-spec hardware, not dev machines. If profiled `T_load` exceeds ~6 s the trigger distance becomes level-design-disruptive; split the zone.

### F3 — Zone Transition Total Time Budget (Time-Budget Identity)

Decomposes zone-transition latency into subsystem-owned slices. Not a scaling formula — an **accountability identity** assigning latency ownership across Save/Load, Addressables, and Unity scene activation.

`T_transition = T_save + T_load + T_activate + T_unload_async`

**Variables:**

| Variable | Symbol | Type | Range | Description | Owner |
|----------|--------|------|-------|-------------|-------|
| Save mutex | `T_save` | ms (int) | 0–200 | `SaveCheckpointing` state duration; target <150 ms (`save_mutex_max_ms`, Section G). | Save/Load |
| Addressable load | `T_load` | ms (int) | 0–∞ | Group load time for incoming zone. | Addressables + content |
| Scene activation | `T_activate` | ms (int) | 0–100 | `LoadSceneMode` switch + GRD registration frame + first-frame settle. | Engine integration |
| Async unload | `T_unload_async` | ms (int) | 0–∞ | Outgoing zone unload. Post-felt (player already in new zone). | Addressables |
| Total | `T_transition` | ms (int) | 0–∞ | From trigger entry to player control restored. | — |

**Felt latency** = `T_save + T_load + T_activate` (player position locked). **Post-felt** = `T_unload_async` (affects `R_peak` overrun duration, not UX).

**Output Range:** Felt latency target <5 s (corresponds to `zone_overrun_window_seconds`, Section G).

**Example (MVP):** `T_save` = 120 ms, `T_load` = 3000 ms, `T_activate` = 50 ms, `T_unload_async` = 800 ms.
Felt latency = **3,170 ms (~3.2 s)** — within target. `R_peak` overrun window = 800 ms.

**Extreme behavior:** If `T_save` spikes to 500 ms (slow disk, heavy save payload), it stacks perceptibly with `T_load`. The `save_mutex_max_ms` tuning knob is a target, not a guarantee — it bounds acceptable-miss thresholds. Save system benchmarking is a separate concern (Save/Load GDD).

### F4 — Corpse-Run Zone Retention Memory Cost

When the player dies and the corpse is in a zone different from their current zone, that zone must hold `ZoneIdle` for the recovery probe. This costs against the 350 MB cap.

`R_available_active = R_cap − R_always − R_corpse_zone`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|----------|--------|------|-------|-------------|
| Cap | `R_cap` | MB | 350 | Hard cap (art bible §8.9). |
| Always-loaded | `R_always` | MB | 0–350 | Same as F1. |
| Corpse zone bytes | `R_corpse_zone` | MB | 0–`R_cap` | Bytes resident for the `ZoneIdle` zone holding the corpse. Zero when corpse in current zone or retention timeout elapsed. |
| Available for active | `R_available_active` | MB | 0–305 | Bytes available for active zone's group while corpse zone is held. |

**Output Range:** Clamped [0, 305 MB] under normal play.

**Example (MVP):** Player dies in HauntMansion (140 MB), returns to CityHub as active.
`R_available_active` = 350 − 45 − 140 = **165 MB** — CityHub (125 MB) fits with 40 MB headroom.

**Tradeoff lever:** The `corpse_run_zone_retention_seconds` tuning knob (Section G) determines how long `R_corpse_zone` is non-zero. Shorter retention → more active-zone budget → denser art possible. Longer retention → richer corpse-run UX but tighter budget.

**Extreme behavior:** At T4 with multiple haunt zones, only one corpse zone held at a time (Rule 3 still holds for active; corpse-retention is a separate `ZoneIdle` hold). If future design permits simultaneous deaths across zones (not at T1), F4 needs a summation term `Σ R_corpse_zone_i`. Flag for T4 design pass.

### Formula vs. Tuning-Knob boundary (cross-reference to Section G)

| Value | Owner | Why |
|-------|-------|-----|
| `R_cap` (350 MB) | Section D (input from [art bible §8.9](../art/art-bible.md)) | Hard constraint; not tunable without amending the art bible |
| `R_always`, `R_active`, `R_corpse_zone` | Section D (measured) | Derived from group configuration, not configurable targets |
| `V_max`, `T_load`, `T_save`, `T_activate`, `T_unload_async` | Section D (measured) | Profiled inputs; change with content and hardware |
| `D_margin` (1–5 m) | Section G | Authoring constant with a safe range |
| `zone_overrun_window_seconds` (<5 s target) | Section G | Configurable target for felt latency |
| `save_mutex_max_ms` (<150 ms target) | Section G | Configurable target for save subsystem |
| `corpse_run_zone_retention_seconds` (TBD) | Section G | Policy decision; memory cost quantified by F4 |

## Edge Cases

### A — Transition-boundary edges

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| **A1. Player dies inside a trigger volume during `ZoneLoading`** | Load aborted; incoming group unloaded immediately. Transition to `CorpseRunActive` as substate of outgoing zone (which never left `ZoneActive`). Corpse position = player's world-space death coords. Save/Load writes `CorpseRecord` with outgoing zone ID. | Rule 8 save already captured pre-transition state; corpse is geometrically in the known outgoing zone; no ambiguous ownership. |
| **A2a. Manual save during `ZoneLoading`** | Request queued; no second `SaveCheckpointing` fires. On exit (to `ZoneActive` or `ZoneError`), queued request discarded — Rule 8 already captured state. | Prevents mid-transition save inconsistency. |
| **A2b. Manual save during `ZoneUnloading`** | Request honored immediately. `SaveCheckpointing` fires; save captures just-entered zone as active; unload continues in parallel. | `ZoneUnloading` does not modify player position or zone membership. |
| **A3. Crash during `SaveCheckpointing`** | Save/Load owns recovery (per [save-integrity rule](../../.claude/rules/save-integrity.md)). Previous complete save reflects pre-transition state; player resumes in outgoing zone. If Save/Load detects unrecoverable corruption, fires `SaveFailedEvent`; new session opens in `ZoneError`. | Rule 8 save is fallback; Save/Load owns HMAC and write-integrity. |
| **A4. Save hangs — `SaveWriteConfirmed` never received** | Per **Rule 12**: if `save_mutex_max_ms` elapses, fire `SaveTimedOutEvent`, transition to `ZoneError`, log elapsed time. | Hard timeout prevents solo-dev "freezes at the door" failure on slow HDD. |

### B — Persistence / version-migration edges

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| **B1. Save file names a zone ID absent from current Addressable catalog** | Transition to `ZoneError` on load-resume. Surface non-dismissible UI: *"Save references a location that no longer exists — your progress may be incompatible with this version."* Log missing ID. **No automatic fallback to a "safe" zone.** | Silent relocation hides authoring bugs. Zone IDs are stable identifiers; removal requires migration script — flag for future ADR (Zone ID Lifecycle Management). |
| **B2. Corpse coordinates fall inside moved/removed geometry after update** | Death & Corpse Recovery owns probe; fires `CorpseUnreachableEvent` if blocked. World Structure releases corpse-retention hold (`R_corpse_zone` → 0); corpse penalty resolved by Death & Corpse Recovery. | World Structure has no raycasting authority; downstream system owns traversability. |
| **B3. `R_always` manifest disagrees between save and current build** | Not a runtime problem. `R_always` is derived at runtime, not persisted. If a new shared asset pushes `R_always` past F1 ceiling, that's a build-time violation caught by authoring check. | Save records only `PlayerZoneMembership`, not memory baselines. |

### C — Memory / streaming edges

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| **C1. Addressable load exceeds `zone_overrun_window_seconds`** | Log warning to session audit trail with elapsed time + zone ID. No automatic abort. Fire `ZoneLoadOverrunEvent(elapsed_ms)` for Menus & Settings (non-blocking UI indicator). | Mid-stream termination risks bundle corruption. Overrun is advisory; outright failure → `ZoneError` via C3. |
| **C2. `R_peak` exceeds cap at runtime (handle leak, etc.)** | **Not a design edge — a defect.** World Structure cannot detect directly. **Required CI smoke check:** memory profiler snapshot after each zone unload in headless test, asserting `R_active ≤ R_zone_max` per F1. Flag for Section H acceptance criteria. | Rule 7 forbids cross-zone refs; leaks are engine-programmer defects. Prevention lives in CI, not runtime. |
| **C3. Bundle corruption detected at runtime** | Addressables throws load-failure exception. `ZoneLoading` → `ZoneError`. Menus & Settings receives `ZoneErrorEvent(reason: BundleCorrupt, zoneId)`. Session cannot continue. Log bundle ID + checksum state. **No retry.** | Retry against corrupt cache fails identically and stalls the player. Reinstall/re-download required. Any future retry policy is an ADR, not a design rule. |
| **C4. Disk full during `SaveCheckpointing`** | Save/Load fires `SaveFailedEvent` back to World Structure → transition to `ZoneError`. Session blocked — cannot proceed with unconfirmed save. | Pillar 5 (Stakes Are Honest): save integrity is not negotiable. |

### D — Policy edges

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| **D1. Hub NPC tick semantics during `ZoneIdle` with player in haunt** | Deferred to **ADR-tba-5**. World Structure contract: `ZoneIdle` emits no `ZoneActiveEvent`. NPC System must distinguish "loaded + player-present" from "loaded + player-absent" and must not assume `ZoneIdle` = NPC-quiescent. | Resolution depends on NPC System + Faction Sim GDDs (not yet written). |
| **D2. Player teleports into unloaded zone (debug menu, future feature)** | Teleport MUST go through the normal state machine: fire `ZoneLoading`, complete load, reposition. Save-on-transition (Rule 8) fires. Bypassing the state machine is forbidden in all build configs. | Shortcuts bypass save-on-transition, stranding players on crash. |
| **D3. Player position ends up outside any zone trigger** | Fire `ZonePositionInvalidEvent(playerPosition)` → `ZoneError`. No automatic relocation. In-editor: log invalid coords visibly. Shipping build: `ZoneError` terminal. **Pre-ship CI check: scan all navmesh surface points for full zone-trigger coverage.** | Silent repositioning masks authoring bugs. Prevention in CI. |

### E — Cross-system edges

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| **E1. Faction State Simulation polls zone state during `ZoneLoading`** | Returns previous zone's membership (pre-transition state). No additional handling. | `ZoneActiveEvent` fires only on `ZoneActive` entry. Faction Sim must subscribe to events, not poll. Rule 7 already forbids direct cross-zone object refs. |
| **E2. Combat Core damage event fires during `ZoneTransitionBegin`** | Rule 9 governs: Combat Core resolves in-flight effects per its rules, strips persistent, cancels mid-cast. Combat Core GDD must specify exact discard rule for outgoing-zone projectiles/DOTs. | World Structure's contract complete at firing `ZoneTransitionBeginEvent`. Combat Core owns resolution. |

**ADR candidates surfaced by these edges:**
- **ADR-tba — Zone ID Lifecycle Management.** Stable-identifier contract for zone IDs; migration-script requirement when zones are deprecated (from B1).
- Existing ADR-tba-5 (Hub NPC Schedule Tick Semantics) confirmed as blocker for NPC System GDD (from D1).

## Dependencies

World Structure is Layer 1 Foundation — **no upstream dependencies.** Nine downstream systems depend on it; listed below with direction, data-interface summary, hard/soft classification, and interface ownership. For interface specifics (event payloads, data contracts), see Section C §Interactions.

| System | Direction | Nature / Data Interface | Hard/Soft | Interface Owner |
|---|---|---|---|---|
| **Save / Load** (§2) | WS emits for | `PlayerZoneMembership` (zoneId + `Vector3` position + zoneType) + `ZoneTransitionTimestamp` via `SaveCheckpointing`; receives `SaveWriteConfirmed` / `SaveFailedEvent` | **Hard** | Save/Load owns serialization format; WS owns data contract |
| **Menus & Settings** (§3) | WS depended on by | Publishes `ZoneErrorEvent`, `ZoneLoadOverrunEvent`, `ZoneTransitionBeginEvent` for UI affordance; receives manual-save triggers (routed via Save/Load) | **Soft** | WS publishes; Menus owns UI presentation |
| **NPC System** (§4) | WS depended on by | Publishes `ZoneActiveEvent(zoneId, zoneType)`, `ZoneUnloadingEvent(zoneId)` to gate schedules/spawns | **Hard** | WS publishes; NPC subscribes |
| **Day/Night Cycle** (§5) | Peer (not child) | Day/Night subscribes to `ZoneActiveEvent` (shared event; same emission as NPC/Combat/Zone Control) for zone-specific schedule offsets; Day/Night's world-clock ticks independently, including during `ZoneIdle` | **Hard** | Peer systems — neither owns the other |
| **Combat Core** (§7) | WS depended on by | Publishes `ZoneType`, `ZoneTransitionBeginEvent` (Rule 9); receives `PlayerDeathEvent` | **Hard** | WS publishes zone events; Combat owns combat-enable gate and in-flight resolution |
| **Death & Corpse Recovery** (§14) | Bidirectional | WS holds corpse-retention `ZoneIdle` hold (F4 memory cost); receives `CorpseUnreachableEvent` | **Hard** | Death/Corpse owns probe; WS owns retention hold duration |
| **Faction State Simulation** (§15) | WS depended on by | Publishes `ZoneType` + `ZoneId`; receives `FactionControlChanged(zoneId)` | **Soft at MVP** (reactive) → **Hard at T3** (autonomous between sessions) | Faction Sim owns state; WS owns zone-scope filter |
| **Zone Control** (§17) | WS depended on by | Publishes `ZoneActiveEvent(zoneId)` (activates kill-weight attribution); receives `ZoneFactionOwnerChanged` for persistence via Save/Load | **Hard** | Zone Control owns ownership calc; WS owns persistence of result |
| **Audio System** (§32) | WS depended on by | Publishes `ZoneActiveEvent`, `ZoneTransitionBeginEvent`; Audio selects ambient profile from zone ID | **Soft** — degrades to silence gracefully ([art bible P2](../art/art-bible.md) "Stillness Is The Signal" tolerates missing audio) | WS publishes; Audio subscribes |

### Bidirectional consistency contract

Each downstream GDD, when authored, must declare World Structure in its own §Dependencies with the reverse listing (`depends on: World Structure` — hard/soft matching this table). `/consistency-check` and `/review-all-gdds` verify bidirectional agreement. Any mismatch = one GDD is wrong and needs amending.

### Upstream

**None.** World Structure is Layer 1 in the dependency graph per [systems-index.md](systems-index.md) §Dependency Map — it depends on nothing and is the root of every gameplay feature.

## Tuning Knobs

Four designer-adjustable values drive World Structure's runtime behavior. Per the Section D vs. Section G boundary: profiled inputs (`V_max`, `T_load`, `T_save`, etc.) and the art-bible-locked `R_cap` are **not** knobs — they're inputs to formulas in Section D.

| Parameter | Current Value | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|--------------|------------|-------------------|-------------------|
| `D_margin` — F2 safety buffer for stream-ahead trigger distance | 2 m | 1 – 5 m | More loading slack; trigger volume further from zone boundary. Authored geometry must accommodate the longer approach corridor. | Tighter trigger placement; risk of late-loads (player crosses boundary before load completes) if `T_load` spikes. |
| `zone_overrun_window_seconds` — F1 `R_peak` transient-overlap duration bound | 5 s | 3 – 10 s | More tolerance for slow disks and large zone groups; longer window where both zones' textures are resident simultaneously. | Stricter budget; more warnings in session audit log on transient load hiccups. Below 3 s risks false-triggering `ZoneError` on minimum-spec hardware. |
| `save_mutex_max_ms` — Rule 12 hard timeout for `SaveCheckpointing` | 150 ms | 100 – 500 ms | Tolerates slower Save/Load (slow disk, large save payload); delays player transition perceptibly. Above 500 ms becomes noticeable as a hitch. | Stricter save deadline; may trigger spurious `SaveTimedOutEvent` → `ZoneError` on minimum-spec HDDs. Below 100 ms risks failing on any disk contention. |
| `corpse_run_zone_retention_seconds` — F4 tradeoff lever for how long `R_corpse_zone` is held | **300 s (5 min) — revisit at T1 playtest** | 30 s – 10 min | More forgiving corpse-recovery UX (player can take time to return); tighter F4 `R_available_active` budget for the current zone while corpse-zone is held. | Less forgiving corpse UX; more F4 budget for denser art in the current zone. At extreme, retention expires before player reaches recovery point → effective death penalty increase. |

> **`corpse_run_zone_retention_seconds` initial value rationale:** 5 minutes is enough for a player to travel back from nearly anywhere in a T1 haunt + hub, short enough that it doesn't dominate player time when they're stuck, and long enough that a phone-call interruption doesn't compound the death penalty. Revisit during T1 playtest: measure actual corpse-run durations and tune to the 75th percentile of observed recovery times.

### Tuning-knob interactions

- **`zone_overrun_window_seconds` × `save_mutex_max_ms`** — Both contribute to `T_transition` felt latency in F3. If `save_mutex_max_ms` is set near its max (500 ms), the effective budget for `T_load` shrinks. Authoring constraint: `T_save_target + T_load_profiled + T_activate_max < zone_overrun_window_seconds × 1000`.
- **`D_margin` × `zone_overrun_window_seconds`** — Spatial and temporal buffers absorbing the same risk (slow load). Don't set both to max simultaneously — level-design pain without additional safety.
- **`corpse_run_zone_retention_seconds` × F1.1 Zone Art Budget Ceiling** — Longer retention shrinks `R_available_active` for the player's current zone during the corpse-run window. At retention values >5 min, re-evaluate F1.1 for any zone a player might occupy post-death; art density in those zones must drop.

### Cross-reference — knobs owned elsewhere that affect this system

- **`save_autosave_interval`** — owned by Save/Load GDD. Affects frequency of `SaveCheckpointing` invocations.
- **`PlayerDeathEvent` trigger conditions** — owned by Combat Core GDD. Determines when `CorpseRunActive` transitions fire.
- **Player movement speed `V_max`** — owned by Character Progression / Combat Core. Not a knob here; profiled input to F2.

## Visual/Audio Requirements

Minimal. World Structure is infrastructure that **enables** visual/audio behavior but does not own it directly.

| Event | Visual | Audio | Owner |
|---|---|---|---|
| `ZoneLoadOverrunEvent` (load > `zone_overrun_window_seconds`) | Non-blocking UI indicator | None | Menus & Settings |
| `ZoneErrorEvent` (BundleCorrupt / MissingID / SaveTimedOut / ZonePositionInvalid) | Non-dismissible error modal with reason text | Optional low-urgency warning tone | Menus & Settings |
| Zone transition | No World-Structure-owned visual — color shifts are material-and-light-driven per [art bible §4.4](../art/art-bible.md) | Audio zone profile swap on `ZoneActiveEvent` (Audio System selects) | Audio System |
| Corpse-run desaturation | Per-camera -40% desaturation on dead player's Overlay Camera — **ADR-tba-2** owns the camera-stack architecture | None from this GDD | engine-programmer + unity-shader-specialist |
| `SaveCheckpointing` | Optional save-in-progress indicator (small, bottom-right) — may be invisible given 150 ms target | None | Menus & Settings |

> **📌 Asset Spec flag** — Once per-zone content exists, run `/asset-spec system:world-structure` for any World-Structure-owned asset. Likely minimal: loading-indicator icons, error-modal backing, and the corpse-run camera-stack prefab (if one emerges from ADR-tba-2). The vast majority of zone visuals are owned by per-zone content, not by this GDD.

## Game Feel

[N/A — World Structure is infrastructure. Player-facing feel targets live in downstream systems: Combat Core (combat feel), Character Progression / Character Creation (traversal), Camera system, Death & Corpse Recovery (corpse-run pacing). Revisit if zone-transition animations or camera behavior get pulled into this GDD's scope during section C.]

## UI Requirements

World Structure does not own any primary UI. It triggers the following UI states via events published to Menus & Settings.

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| "Loading..." indicator (low-opacity) | Bottom-right HUD corner | Shown on `ZoneLoadOverrunEvent`; hidden on `ZoneActiveEvent` | Only when `T_load > zone_overrun_window_seconds` — a normal transition does not show this |
| `ZoneError` modal | Screen-center, non-dismissible | Once, on entry | `ZoneError` state entered (Edge B1 missing-zone, C3 bundle corruption, A4 save timeout, D3 invalid position) |
| Save-in-progress indicator | Bottom-right HUD corner (optional — may be invisible given 150 ms target) | During `SaveCheckpointing`, max `save_mutex_max_ms` | Optional affordance; Menus decides whether to render |

> **📌 UX Flag — World Structure**: In pre-production, run `/ux-design` for the `zone-error-modal` and `loading-indicator` UI elements. This GDD specifies triggers and events; the visual/interaction design of the error modal and indicator goes in the UX spec, not here.

## Cross-References

Consolidated declared dependencies on other project artifacts. Machine-checkable by `/review-all-gdds` Phase 2c.

| This Document References | Target | Specific Element | Nature |
|---|---|---|---|
| "zone = Addressable streaming group" (Rule 1, F1, F1.1) | `design/art/art-bible.md` §8.7 | Zone definition lock | Rule dependency |
| "≤350 MB texture residency" (F1, Section G `R_cap`) | `design/art/art-bible.md` §8.9 | `R_cap` = 350 MB | Data dependency |
| "≤3–4 unique 2K surfaces per group" (Section C context) | `design/art/art-bible.md` §6.2 | Per-group art budget | Rule dependency |
| "no post-process LUT swap at zone boundary" (Rule 10) | `design/art/art-bible.md` §4.4 | Zone-transition visual constraint | Rule dependency |
| "exterior decal projectors forbidden at city density" (Section C context) | `design/art/art-bible.md` §8.7 | Rendering constraint | Rule dependency |
| "corpse-run per-camera desaturation" (ADR-tba-2) | `design/art/art-bible.md` §2 State 7 | Camera-stack requirement | State trigger |
| Pillar 1 "The World Is Not Your Story" | `design/gdd/game-concept.md` | Persistence pillar | Rule dependency |
| Pillar 2 "The Silence Is Sacred" | `design/gdd/game-concept.md` | Pacing pillar | Rule dependency |
| Pillar 5 "Stakes Are Honest" | `design/gdd/game-concept.md` | Save integrity / failure legibility | Rule dependency |
| "Unity 6.3 LTS + URP + Addressables" | `DECISIONS.md` D001 | Engine lock | Rule dependency |
| "T1 single-player offline" | `DECISIONS.md` D003 | Tier scope | Rule dependency |
| "Codex parallel worktree governance" | `DECISIONS.md` D006 | Agent governance | Rule dependency |
| "HMAC-signed local saves" | `.claude/rules/save-integrity.md` | Save integrity rule | Rule dependency |
| "Unity 6.3 render graph; `SetupRenderPasses` deprecated" | `docs/engine-reference/unity/VERSION.md` | Engine API gap | Rule dependency |

## Acceptance Criteria

Testable conditions that prove World Structure works as designed. Organized by Core Rule coverage (10), Formula coverage (5), High-Risk Edge Case coverage (5), and Cross-System interface integrity (6). Every criterion uses **Given-When-Then** format; each is tagged with test type, owner, and T1-blocking status. Summary table at end.

### Core Rules coverage (C1–C12)

**H-CR-01 — Rules 1 + 11 (Zone Definition + Tier Scalability)**
**GIVEN** the `ZoneManifest` ScriptableObject is the sole zone registry, **WHEN** a gameplay-programmer adds a new zone entry at T4 (up to 5 zones), **THEN** the zone loads, activates, and unloads without any code change to the state machine or transition logic — only manifest data changes.
*Integration | gameplay-programmer | T1-blocking*

**H-CR-02 — Rule 2 (Zone Types)**
**GIVEN** the player enters a `HauntZone`, **WHEN** `ZoneActiveEvent` is published, **THEN** Combat Core enables combat AI and Zone Control enables kill-weight attribution; both are disabled when the active zone is `CityHubZone`.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CR-03 — Rules 3 + 6 (Single Active Zone + Unload Policy)**
**GIVEN** a zone transition is initiated, **WHEN** the state-machine log is captured, **THEN** no frame reports two zones simultaneously in `ZoneActive`; the outgoing zone does not begin unloading until the incoming zone has confirmed `ZoneActive`.
*Integration (automated state-log assertion) | engine-programmer | T1-blocking*

**H-CR-04 — Rule 4 (Player Position Is Authoritative)**
**GIVEN** a save file written during `SaveCheckpointing`, **WHEN** the session reloads, **THEN** zone membership is re-derived from the persisted `Vector3` position — no separate cached zone-ID field exists in the save schema.
*Unit | gameplay-programmer | T1-blocking*

**H-CR-05 — Rule 5 (Trigger Volumes)**
**GIVEN** the player approaches a zone boundary at `V_max`, **WHEN** they enter the trigger volume, **THEN** the incoming zone finishes loading before the player reaches the zone-boundary threshold (verified via `D_trigger` placement per F2).
*Integration (profiled playtest on minimum-spec) | engine-programmer + qa-tester | T1-blocking*

**H-CR-06 — Rule 7 (Cross-Zone Reference Policy)**
**GIVEN** any two zones are simultaneously resident (mid-transition), **WHEN** a static-analysis scan + runtime reference check run, **THEN** no Unity scene-graph object reference crosses zone boundaries; all cross-zone persistent-state reads go through the Save/Load service.
*CI-smoke (Unity reference validator + grep) | engine-programmer | T1-blocking*

**H-CR-07 — Rule 8 (Save-on-Transition Boundary)**
**GIVEN** the player initiates a zone transition, **WHEN** the app is force-quit at any point during `ZoneLoading`, **THEN** the next session resumes in the outgoing zone at the pre-transition player position, with no data loss.
*Integration (manual force-kill test) | qa-tester | T1-blocking*

**H-CR-08 — Rule 9 (In-Flight Effects on Zone Cross)**
**GIVEN** the player has an active DOT and a mid-cast spell, **WHEN** `ZoneTransitionBeginEvent` fires, **THEN** the spell is cancelled and the DOT is resolved per Combat Core rules before the transition completes; no status effects persist into the new zone.
*Integration | gameplay-programmer (Combat Core) + qa-tester | T1-blocking*

**H-CR-09 — Rule 10 (Color Temperature Not Owned Here)**
**GIVEN** the player crosses a zone boundary, **WHEN** reviewed in Unity Frame Debugger, **THEN** no post-process LUT swap, URP Volume blend, or global lighting state change is triggered by World Structure; temperature shift is entirely material- and light-source-driven.
*Visual (screenshot + art-lead sign-off) | engine-programmer + art-lead | advisory*

**H-CR-10 — Rule 12 (Save-on-Transition Hard Timeout)**
**GIVEN** `SaveCheckpointing` is entered and `SaveWriteConfirmed` is withheld (test harness), **WHEN** `save_mutex_max_ms` elapses, **THEN** `SaveTimedOutEvent` fires, the state machine transitions to `ZoneError`, and elapsed time is written to the session audit trail — no hang or freeze.
*Unit | gameplay-programmer | T1-blocking*

### Formula coverage (F1–F4)

**H-F1 — Peak Texture Residency (F1)**
**GIVEN** any authored zone configuration, **WHEN** Unity Memory Profiler snapshots at the worst transition midpoint (both zones resident), **THEN** `R_peak = R_always + R_active + R_transient ≤ 350 MB`; violation is a build-blocking CI error.
*CI-smoke (automated Memory Profiler snapshot in headless build) | engine-programmer | T1-blocking*

**H-F1.1 — Zone Art Budget Ceiling (F1.1)**
**GIVEN** the current `R_always` and the largest zone's texture footprint, **WHEN** art delivers any zone asset bundle, **THEN** the bundle's texture bytes ≤ `R_zone_max` (165 MB at MVP); CI rejects bundles exceeding the ceiling.
*CI-smoke (Addressable bundle size check) | engine-programmer + art-lead | T1-blocking*

**H-F2 — Stream-Ahead Trigger Distance (F2)**
**GIVEN** `V_max` and profiled `T_load` on minimum-spec, **WHEN** trigger volumes are placed, **THEN** every trigger satisfies `D_trigger ≥ V_max × T_load + D_margin`; values <5 m or >40 m produce a level-validation warning.
*CI-smoke (level-validator script) | engine-programmer | T1-blocking*

**H-F3 — Transition Time Budget (F3)**
**GIVEN** a CityHub→HauntMansion transition on minimum-spec, **WHEN** timed from trigger-volume entry to player control restored, **THEN** felt latency `T_save + T_load + T_activate < 5,000 ms`; each sub-component is logged separately so ownership is unambiguous.
*Performance-profile (manual profiled playtest) | engine-programmer + qa-tester | T1-blocking*

**H-F4 — Corpse-Run Zone Retention Memory Cost (F4)**
**GIVEN** the player dies in HauntMansion and active zone becomes CityHub, **WHEN** measured at any point during `corpse_run_zone_retention_seconds`, **THEN** `R_available_active = R_cap − R_always − R_corpse_zone ≥ 0` and the active zone's texture bytes fit within `R_available_active`.
*Integration (profiled playtest) | engine-programmer + qa-tester | T1-blocking*

### High-risk edge case coverage

**H-EC-A1 — Death in Transition Volume (Edge A1)**
**GIVEN** the player enters a `ZoneLoading` trigger volume, **WHEN** `PlayerDeathEvent` fires before `ZoneActive` is confirmed, **THEN** the incoming load aborts, the outgoing zone remains `ZoneActive`, `CorpseRunActive` substates into the outgoing zone, and the corpse record names the outgoing zone ID.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-EC-B1 — Missing Zone ID on Load-Resume (Edge B1)**
**GIVEN** a save file referencing a zone ID absent from the current Addressable catalog, **WHEN** the session loads, **THEN** `ZoneError` is entered, a non-dismissible UI message surfaces the missing-ID string, the session audit logs the missing ID, and no silent zone relocation occurs.
*Integration (authored invalid test save) | gameplay-programmer + qa-tester | T1-blocking*

**H-EC-C2 — R_peak Runtime Violation via Leak (Edge C2)**
**GIVEN** a zone unload completes in headless CI, **WHEN** Unity Memory Profiler snapshots `R_active` post-unload, **THEN** `R_active ≤ R_zone_max`; failure blocks the build and is filed as an engine-programmer defect, not a design edge.
*CI-smoke | engine-programmer | T1-blocking*

**H-EC-C3 — Bundle Corruption (Edge C3)**
**GIVEN** Addressables throws a load-failure exception on a deliberately corrupted test bundle, **WHEN** `ZoneLoading` handles the exception, **THEN** `ZoneError` is entered, no retry is attempted, bundle ID + checksum state are logged, and `ZoneErrorEvent(reason: BundleCorrupt)` is delivered to Menus & Settings.
*Integration | engine-programmer + qa-tester | T1-blocking*

**H-EC-D3 — Player Outside All Zone Triggers (Edge D3)**
**GIVEN** a pre-ship CI scan of all navmesh surface points, **WHEN** any point falls outside every zone trigger volume's coverage, **THEN** the level-validator fails the build with out-of-bounds coordinates logged; in a shipping session, such a position fires `ZonePositionInvalidEvent` and enters `ZoneError`.
*CI-smoke (navmesh coverage scan) | engine-programmer | T1-blocking*

### Cross-system interface integrity

**H-CS-01 — Save/Load (hard)**
**GIVEN** a zone transition reaches `ZoneActive`, **WHEN** the save file is inspected post-session, **THEN** `PlayerZoneMembership` contains `zoneId`, `Vector3` position, `zoneType`, and `ZoneTransitionTimestamp`; all four fields present and non-default.
*Unit | gameplay-programmer | T1-blocking*

**H-CS-02 — NPC System (hard)**
**GIVEN** the incoming zone reaches `ZoneActive`, **WHEN** NPC subscriber logs are checked, **THEN** `ZoneActiveEvent(zoneId, zoneType)` was received by all NPC subscribers before any NPC tick fires in the new zone; `ZoneUnloadingEvent` is received before any NPC references the outgoing zone's scene objects.
*Integration | gameplay-programmer (NPC) + qa-tester | T1-blocking*

**H-CS-03 — Combat Core (hard)**
**GIVEN** the player dies in a `HauntZone`, **WHEN** `PlayerDeathEvent` is received, **THEN** the state machine transitions to `CorpseRunActive` within the same frame; `ZoneType` remains `HauntZone` (no zone flip on death).
*Unit | gameplay-programmer | T1-blocking*

**H-CS-04 — Audio System (soft)**
**GIVEN** `ZoneActiveEvent` is not received by Audio (subscriber missing or disabled), **WHEN** the player completes a transition, **THEN** the game remains playable — audio falls back to silence, no exception thrown, no other system affected.
*Integration (audio subscriber removed in test) | engine-programmer + qa-tester | advisory*

**H-CS-05 — Zone Control (hard)**
**GIVEN** the player kills an enemy in a `HauntZone`, **WHEN** kill-weight attribution is computed by Zone Control, **THEN** `ZoneActiveEvent(zoneId)` was the trigger; no kill-weight attribution fires when active zone is `CityHubZone`.
*Integration | gameplay-programmer (Zone Control) | T1-blocking*

**H-CS-06 — Day/Night Cycle (hard, peer)**
**GIVEN** a zone transition reaches `ZoneActive`, **WHEN** Day/Night Cycle logs are inspected, **THEN** Day/Night received `ZoneActiveEvent` and applied the zone-specific schedule offset within one frame; Day/Night continues ticking during `ZoneIdle` without requiring re-subscription.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

### Summary Table

| ID | Covers | Test Type | Owner | T1-Blocking |
|---|---|---|---|---|
| H-CR-01 | C1, C11 | Integration | gameplay-programmer | Yes |
| H-CR-02 | C2 | Integration | gameplay-programmer, qa-tester | Yes |
| H-CR-03 | C3, C6 | Integration | engine-programmer | Yes |
| H-CR-04 | C4 | Unit | gameplay-programmer | Yes |
| H-CR-05 | C5 | Integration/profile | engine-programmer, qa-tester | Yes |
| H-CR-06 | C7 | CI-smoke | engine-programmer | Yes |
| H-CR-07 | C8 | Integration | qa-tester | Yes |
| H-CR-08 | C9 | Integration | gameplay-programmer, qa-tester | Yes |
| H-CR-09 | C10 | Visual | engine-programmer, art-lead | advisory |
| H-CR-10 | C12 | Unit | gameplay-programmer | Yes |
| H-F1 | F1 | CI-smoke | engine-programmer | Yes |
| H-F1.1 | F1.1 | CI-smoke | engine-programmer, art-lead | Yes |
| H-F2 | F2 | CI-smoke | engine-programmer | Yes |
| H-F3 | F3 | Performance-profile | engine-programmer, qa-tester | Yes |
| H-F4 | F4 | Integration/profile | engine-programmer, qa-tester | Yes |
| H-EC-A1 | Edge A1 | Integration | gameplay-programmer, qa-tester | Yes |
| H-EC-B1 | Edge B1 | Integration | gameplay-programmer, qa-tester | Yes |
| H-EC-C2 | Edge C2 | CI-smoke | engine-programmer | Yes |
| H-EC-C3 | Edge C3 | Integration | engine-programmer, qa-tester | Yes |
| H-EC-D3 | Edge D3 | CI-smoke | engine-programmer | Yes |
| H-CS-01 | Save/Load | Unit | gameplay-programmer | Yes |
| H-CS-02 | NPC System | Integration | gameplay-programmer, qa-tester | Yes |
| H-CS-03 | Combat Core | Unit | gameplay-programmer | Yes |
| H-CS-04 | Audio System | Integration | engine-programmer, qa-tester | advisory |
| H-CS-05 | Zone Control | Integration | gameplay-programmer | Yes |
| H-CS-06 | Day/Night Cycle | Integration | gameplay-programmer, qa-tester | Yes |

**Total: 26 criteria. 23 T1-blocking, 3 advisory.**

### Explicit non-criteria (out of this GDD's scope)

- **Edge E2 (Combat damage during `ZoneTransitionBegin`)** — ownership belongs to Combat Core GDD. World Structure's contract complete at firing the event; Combat Core's Section H will cover in-flight resolution specifics.
- **Faction Sim at T3** — `FactionControlChanged` persistence smoke check becomes T3-blocking when autonomous sim lands. Flag for Faction State Simulation GDD Section H.
- **Hub NPC tick semantics (Edge D1)** — ADR-tba-5 gate. Acceptance criterion for the chosen tick mode belongs to whatever GDD/ADR resolves it.

## Open Questions

| Question | Owner | Deadline | Status |
|---|---|---|---|
| **ADR-tba-1: Zone Scene Topology** — persistent-hub + additive Addressable scenes (provisional recommendation from `engine-programmer`) | `engine-programmer` + `unity-specialist` | Before T1 zone implementation | Open |
| **ADR-tba-2: Corpse-Run Camera Stack Configuration** — URP Overlay-camera post-process isolation for per-camera -40% desaturation | `engine-programmer` + `unity-shader-specialist` | Before Combat Core implementation (corpse-run visible) | Open — **prototype required** (art bible TA-flagged) |
| **ADR-tba-3: World State Serialization Contract** — `WorldStateRecord` POCO DTO shape, HMAC integrity, version-migration strategy | `gameplay-programmer` + `security-engineer` | Before T1 save implementation | Open |
| **ADR-tba-4: Zone Transition Mechanism** — trigger-volume stream-ahead + door-prefab commit point; GPU Resident Drawer registration-frame mitigation | `engine-programmer` | Before first zone implementation | Open — **prototype required** (GRD × Addressables unload behavior unknown in 6.3) |
| **ADR-tba-5: Hub NPC Schedule Tick Semantics** — real-time tick vs. delta-catch-up vs. discrete-event during `ZoneIdle` in T1 offline | `game-designer` + `ai-programmer` + `systems-designer` | Before NPC System GDD authoring | Open — **blocker for NPC System GDD** |
| **ADR-tba-6: Zone ID Lifecycle Management** — stable-identifier contract; migration-script requirement when zones are deprecated | `release-manager` + `gameplay-programmer` | Before any zone deletion in a shipped build | Open |
| `corpse_run_zone_retention_seconds` — initial 300 s pin, validate against T1 playtest observed corpse-run durations | `qa-tester` + `game-designer` | T1 playtest | Pinned with revisit flag |
| Per-zone art-budget validation at T3+ (zones may diverge from MVP 165 MB profile) | `art-director` + `engine-programmer` | T3 zone design | Open — flagged for T3 |
| Faction Sim T3 upgrade (reactive → autonomous): `FactionControlChanged` persistence smoke check gating T3 release | `gameplay-programmer` (Faction Sim) + `qa-lead` | T3 entry gate | Open — flagged for T3 |

**T1 blockers (ADR-tba-1 through ADR-tba-5)** must be resolved before their respective implementation work begins. ADR-tba-6 is not T1-blocking (zone deletion won't happen at MVP). ADR-tba-2 and ADR-tba-4 require prototyping, not just desk analysis.
