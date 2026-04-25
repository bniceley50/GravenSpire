# World Structure

> **Status**: APPROVED 2026-04-23 (re-entry #5 senior review). Prior rounds closed all blocker groups; the GDD is implementation-ready. Remaining work is explicit open ADR/prototype items declared in §Open Questions, not unresolved design defects.
> **Author**: Claude Code (session with brian, 2026-04-22; revised 2026-04-23 across rounds 1–4)
> **Last Updated**: 2026-04-23
> **Last Verified**: 2026-04-23
> **Design Review**: 2026-04-23 — five review rounds completed same day. **Round 1** (full `/design-review`) returned MAJOR REVISION NEEDED (6 structural blocker groups); resolved at document level via real Task subagents (game-designer, systems-designer, unity-specialist, unity-addressables-specialist, performance-analyst, qa-lead) + binding TD call on D1+D2 + binding CD call on D3. **Round 2** re-review returned NEEDS REVISION (scope M, 13 targeted-cleanup blockers); resolved via criterion-level AC rewrites, fantasy-language fixes, ADR-tba-4 prototype-scope expansion, and D3 organic-discovery amendment. **Round 3** re-review returned NEEDS REVISION (scope M, 3 blocker groups — zone/group contract coherence, Rule 13 subscriber completeness, T1 AC implementation safety); resolved via Rule 1 prototype-dependency framing, Interactions/Dependencies table completion (Day/Night + Faction Events + Dialogue System), and AC splits (H-F1 → steady+peak; H-F1.1 → local+global; H-F4b → schema+cadence; H-EC-A1 → state+addressables). **Round 4** re-review returned NEEDS REVISION (scope S, 3 residual items); resolved via zone/group wording normalization, systems-index.md reverse-map sync, and C2 shared-caveat rewrite with pre-specified load-side (Form L) and unload-side (Form U) fallback methodologies. **Round 5** re-review returned **APPROVED** — no additional GDD revisions required. See [reviews/world-structure-review-log.md](reviews/world-structure-review-log.md).
> **Implements Pillar**: Primary — **P2 The Silence Is Sacred** (spatial pacing, slow travel, no markers, tension through stillness). Supports — **P1 The World Is Not Your Story** (world persists and evolves across session boundaries via Rule 13 SessionResumeEvent at T1), **P3 Reputation Is The Progression** (via Zone Control — kills shift faction camp ownership), **P5 Stakes Are Honest** (corpse-run camera-stack boundary, timestamp-based cross-zone corpse recovery).

## Summary

World Structure is the spatial and memory-management infrastructure that defines Gravenspire's zones, transitions, and persistence boundaries. Players do not interact with it directly; every other system (combat, faction simulation, NPC behavior, save/load, audio) depends on it for the authoritative answers to "where am I?" and "what's loaded right now?" MVP scope is one haunt zone and one city hub, each represented by a `ZoneManifest` entry that resolves to a logical group-set of Addressable streaming resources (per Rule 1; the specific runtime binding shape — independent three-group split vs. collapsed — is prototype-dependent per ADR-tba-4(d)(g)).

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `None (Layer 1 — no upstream dependencies)`

## Overview

World Structure is the spatial architecture of the game world: it defines zones (each published as a stable `zoneId` by a `ZoneManifest` entry that resolves to a logical group-set of Addressable streaming resources — per Rule 1; the conceptual boundary aligns with [art bible §8.7](../art/art-bible.md), the runtime binding shape is prototype-dependent per ADR-tba-4(d)(g)), manages how zones are loaded, unloaded, and transitioned between, enforces the ≤350 MB texture-residency budget during normal play ([§8.9](../art/art-bible.md)), and holds the rule that zone-boundary color-temperature shifts are material-and-light-driven rather than post-process LUT overrides ([§4.5](../art/art-bible.md)). It owns the persistence boundary where "the player is in zone X at position P" becomes serializable save-file state. Every gameplay system downstream — combat, faction simulation, NPC behavior, save/load, audio — depends on it for authoritative answers to "where am I?", "what's loaded?", and "what changes when I cross this boundary?" The system is invisible in play: players never interact with it directly. They experience what it enables — the slow, marker-less traversal that supports [Pillar 2](game-concept.md) (Silence Is Sacred); a world that persists and evolves across session boundaries, supporting [Pillar 1](game-concept.md) (The World Is Not Your Story); and zone-scoped consequences of player action via the downstream Zone Control system. Tier 1 MVP scope is one haunt zone (Unrest-register mansion, 2–3 floors) plus one city hub skeleton (Gravenspire itself), each realised as a `ZoneManifest`-registered logical group-set. The architecture must accommodate 3–5 zones at Tier 4 without structural rework.

## Player Fantasy

Gravenspire's world structure is felt not as an architecture but as a *testimony*. The player feels they are a guest in a city that kept its own hours before they arrived and will keep them after they log out — lamplighters making their round at dawn, Court couriers crossing paths indifferently, shrines re-dressed between visits. Transitions are slow and material-driven (stone giving way to damp cobble, lantern-warm interiors cooling toward charcoal alleys), so *going somewhere is a thing that takes time and changes the player slightly*. The constrained texture residency budget is felt as **focus**, not as limitation — places become memorable the way streets in a real neighborhood do, through the returning eye finding the same worn stone, the same Court sigil bleeding through whitewash. Persistence makes [Pillar 1](game-concept.md) (The World Is Not Your Story) tactile: when the player returns after three days away, banners have changed, notice-board writs have accumulated, a body left in an alley has been *found*. The emotional register is what Susanna Clarke's *Piranesi* calls "the House" — a place whose rules predate the player, whose kindness, when it comes, is earned.

### Anchor moments

- **Return to the city gate at dawn after a night in the haunt.** The lamplighter is already on his round. A Court courier passes heading the other way. Neither acknowledges the player. This is arrival as threshold, not as event. *(Mid-route presence of the lamplighter — actor visibly on route, not result-only — is a **hard P2 requirement for this anchor**, which constrained ADR-tba-5's resolution space to delta-catch-up or real-time tick. NPC System resolves the T1 answer as data-only delta catch-up plus active-zone ticks. A pure discrete-event result-only fallback ("the lamp is already lit") remains a rejected degradation: the anti-fantasy "world-as-prepared-for-me" (see below) becomes harder to avoid because the player arrives to the *result* of the lamplighter's schedule rather than witnessing the schedule.)*
- **Walking from the cathedral district into the lower wards** (intra-zone at MVP — both districts are named locations inside the single `CityHubZone`; at T2+ if districts are split into separate streaming groups this becomes an inter-zone transition governed by the state machine). No loading screen, no skybox swap. The air gets colder *in the materials*. The player notices they've arrived somewhere different the way they notice the weather turning.
- **Returning to the inn after real-world days offline.** The innkeeper's schedule has advanced, the Faction Board shows events that resolved while you were gone, and — if Faction Events has authored rival-writ content — your rival's writ is on the board in their absence. **At T1, Rule 13 `SessionResumeEvent` lays the signal contract only**; the felt experience requires downstream systems (NPC System, Faction Sim, Day/Night, Faction Events, Dialogue) to implement their own deterministic `SessionResumeEvent` handlers. The event is a silent system-to-system signal with **no UI affordance** (no "welcome back" banner; inherits the anti-fantasy list below) and delivers the elapsed-time delta deterministically; the specific felt content (writs, schedules advanced, rival activity, expired corpse records) is entirely conditional on downstream GDDs authoring their handlers. Under the handler-absent default (§Detailed Design Rule 13), a downstream system without a handler treats load as t=0 — so at T1 the Pillar 1 anchor is real only to the extent downstream systems ship with handlers. The Rule 13 publisher itself runs during World Structure's session-load sequence, **before the first `ZoneActiveEvent`** — Unity-lifecycle placement is verified by H-CR-13b.
- **The commit lock at the threshold.** The ≤100 ms `T_activate` position-lock at the authored door prefab — the **commit** sub-phase of Rule 5 — is felt as a slight resistance at the threshold, the world taking note that you have decided to leave. Engineered, not theatrical. **Implementer note (Blocker 7 clarification, 2026-04-23 round-2):** this beat is the scene-activation commit lock (`T_activate` in F3, ≤100 ms), **not** the ≤150 ms `T_save` stream-ahead save — the save runs invisibly in parallel during stream-ahead while the player retains full locomotion (Rule 5 two-phase split). Do **NOT** implement this anchor as an artificial stream-ahead hold; the stream-ahead save is invisible by design. The felt beat is the commit phase only.

### Anti-fantasy — what the player should NOT feel

- **Loading-screen-as-time-wasted** — transitions are gradual and material, never a hard-cut grade override ([art bible §4.5](../art/art-bible.md)).
- **Zone-as-theme-park-ride** — no "now entering: the Blood District" banners, no ambient NPC lines triggered by proximity, no skybox swaps.
- **World-as-prepared-for-me** — if a candle is lit, someone lit it on a schedule the player doesn't know. The world is not performing.
- **Named-player-as-protagonist** — no "welcome back, hero" on return. No quest log suggesting the world paused while the player was offline.

### Reference register

Susanna Clarke (*Piranesi* — "the Beauty of the House is immeasurable"), Shirley Jackson (*Haunting of Hill House* — "not sane, stood by itself against its hills"), Caspar David Friedrich (figures small against out-scaling landscapes). Explicitly not: heroic fantasy power-player framing; theme-park MMO spatial design.

## Detailed Design

### Core Rules

1. **Zone Definition.** A zone is a stable `zoneId` published by a `ZoneManifest` entry (a ScriptableObject held in the `AlwaysLoaded` group). The manifest entry resolves, at runtime, to a **logical group-set** — the set of Addressable streaming resources whose loaded/unloaded state defines the zone's residency. Per [art bible §8.7](../art/art-bible.md) the conceptual boundary unit is the Addressable streaming-group boundary. Downstream systems reference `zoneId` via the manifest — they never reference Addressable internal keys, bundle labels, or group names directly; the manifest is the indirection layer. The smallest legal zone boundary is a group-set boundary; there is no sub-zone concept in World Structure.

   **Runtime binding shape — prototype-dependent, not yet settled (2026-04-23 round-3 clarification):** The conceptual contract above is stable. The **runtime binding shape** — (i) manifest field types for the group references (`AssetReference` vs string key vs `AssetLabelReference`), per ADR-tba-4(g); and (ii) whether the scene / texture / navmesh responsibilities can be realised as *independent coincident Addressable groups* (the three-group model assumed by F1, F1.1, ZoneIdle) or must collapse into fewer groups because of serialized cross-group dependencies, per ADR-tba-4(d) — is **not yet verified at Unity 6.3**. The conceptual one-or-more-groups binding holds either way; the authoring discipline (procedural-only texture loading vs. serialized material refs) depends on which outcome the prototype reaches. Rules that depend on independent three-group release (F1 peak form, H-F1.1, ZoneIdle metadata-only contract) carry their own ADR-gated caveats. *(Revised 2026-04-23 per D1 metadata-only ZoneIdle + TD binding — multi-group authoring is required for partial unload; round-3 expansion 2026-04-23 demotes the runtime binding shape to prototype-dependent per Blocker A0.)*

2. **Zone Types.** Two zone types exist at MVP: `HauntZone` and `CityHubZone`. `HauntZone` is session-scoped danger space: faction mob spawns are live, Zone Control is active, corpse recovery is in scope. `CityHubZone` is safe space: no combat AI spawns, Zone Control inactive, NPC schedules run on Day/Night Cycle ticks. The type is a behavioral tag consumed by downstream system subscriptions.

3. **Single Active Zone Invariant.** Exactly one zone may be in state `ZoneActive` at any moment. This enforces the ≤350 MB texture-residency budget ([art bible §8.9](../art/art-bible.md)), which assumes only one zone's texture resources (the active zone's logical group-set, per Rule 1) are fully resident at a time. The hub and haunt are never simultaneously active.

4. **Player Position Is Authoritative; zoneId Persisted as Cross-Check.** The player's world-space `Vector3` position is the single source of truth for zone membership. Zone membership is re-derived from position on save/load. The save also persists `zoneId` as a cross-check against the position-derived result — not as a cache. Mismatch on load fires `ZoneError` per Edge B1. *(Revised 2026-04-23 per D2 + TD binding — the original "no separately cached zone ID" framing is inverted: zoneId is persisted explicitly so Edge B1 missing-zone detection and H-CS-01 schema are both satisfied.)*

5. **Transition Initiation via Trigger Volumes — Two-Phase.** Zone transitions begin when the player enters a designated **stream-ahead trigger volume** (a door frame, archway, or staircase landing — authored geometry, not an invisible wall). There are no procedural seams. The transition has two sub-phases:
   - **Stream-ahead sub-phase**: the incoming zone's logical group-set begins loading via an asynchronous Addressables scene-load whose activation is suppressed until commit; the player retains full locomotion; `SaveCheckpointing` (Rule 8) fires and must complete here. **Spec-level contract**: the load API must support (a) an activation-hold state in which scene assets reach load completion but are not made the active scene, and (b) a flag or equivalent operation that releases the hold at commit time. *The specific Unity 6.3 Addressables API surface that realises this contract — the `LoadSceneAsync` + `allowSceneActivation` pairing assumed by the current design — is prototype-dependent per ADR-tba-4(a)(b). The conceptual two-phase split is stable; the API mapping is not yet verified at Unity 6.3.*
   - **Commit sub-phase**: the player crosses the authored commit threshold (door prefab or equivalent); the activation hold is released and the incoming scene becomes active; a sub-100 ms position-lock covers scene activation (`T_activate` in F3).

   This is the only moment player locomotion is constrained, and it is the door itself — not a loading screen. *(Revised 2026-04-23 per D1 Blocker 1 resolution — resolves the contradiction between the original Rule 5 "full locomotion during pre-load" and the ZoneLoading state-table "player locked." Both were half-right; the conceptual two-phase split is stable. Round-3 (2026-04-23) demotes the API mapping to prototype-dependent per Blocker C5 — the split is implementable pending ADR-tba-4(a)(b) verification of the Addressables async-scene-load API surface at Unity 6.3.)*

6. **Unload Policy.** The outgoing zone begins unloading after the incoming zone reaches `ZoneActive`. No simultaneous load/unload. Unload is triggered by successful active-confirm, not by trigger-volume exit — prevents crash-to-menu if the player turns back mid-transition.

7. **Cross-Zone Reference Policy.** Cross-zone reads of persistent state (faction standings, NPC states, world events) are permitted via the Save/Load service (always in memory, not zone-scoped). Direct scene-graph references across zone boundaries are forbidden. An NPC in Zone A may query faction state for Zone B via Save/Load, but may not hold a Unity object reference to an object in Zone B.

8. **Save-on-Transition Boundary.** Player position and zone membership are written to the save file at the moment the incoming zone reaches `ZoneLoading` — the last stable save point before the transition commits. If the app crashes during load, the save reflects the pre-transition state; the player resumes in the outgoing zone.

9. **In-Flight Effects on Zone Cross.** On zone boundary cross, World Structure fires `ZoneTransitionBeginEvent`. Combat Core owns the resolution (cancel / expire / strip per its rules) — World Structure's contract completes on firing the event. Outgoing-zone projectile/DOT discard specifics belong to Combat Core (see Edge E2). Prevents cross-zone spell exploits and simplifies combat cleanup.

10. **Color Temperature Is Not Owned Here.** World Structure does not modify post-process volumes, LUT references, or global lighting states at zone boundaries ([art bible §4.5](../art/art-bible.md) lock). Temperature shifts are handled by the material and light-source inventory of the incoming zone's scene assets. World Structure raises no visual event at zone crossing — the player notices the change through geometry and material, not a system signal.

11. **Tier-Scalability Contract.** All rules above must hold at Tier 4 (3–5 zones) without structural change. Zone list is data-driven (`ZoneManifest` ScriptableObject, not hardcoded scene names). Zone types may be extended (e.g., future `WildernessZone`) without modifying the state machine or transition rules.

12. **Save-on-Transition Hard Timeout.** Rule 8's save-on-transition must complete within `save_mutex_max_ms` (tuning knob, Section G; target <150 ms, safe range 100–500 ms). **Scope: this timeout applies to the `SaveCheckpointing` state during the stream-ahead sub-phase of Rule 5 only.** It does not bound `T_load_ms` or `T_activate`. If `SaveWriteConfirmed` is not received within the window, World Structure fires `SaveTimedOutEvent`, aborts the transition before commit, transitions to `ZoneError`, and logs elapsed time. **The in-flight Addressables load is aborted via the Edge A1 flag-and-async-unload pattern** — whose API realisation is per ADR-tba-4(b)(c): set the `_transitionCancelled` flag; allow the async load operation to reach its activation-hold state (synchronous cancellation semantics at Unity 6.3 are prototype-dependent per ADR-tba-4(b)); suppress activation-release in the completion callback; then release the held handle asynchronously (the release API surface for an unactivated scene handle is prototype-dependent per ADR-tba-4(c) — Rule 12 does not name a specific call here). Prevents the "game freezes at the door" failure mode on slow disks (Section E A4).

13. **Session Resume Catch-Up (T1 Offline World-Kept-Moving Bridge).** On session load, World Structure fires `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` **before** any `ZoneActiveEvent` is published. The elapsed value is clamped to `[0, session_catchup_max_real_seconds]` (tuning knob, Section G; default 7 real days, matching [game-concept.md](game-concept.md) unique-hook language "log off for a week"); negative deltas (clock skew) clamp to 0. This is a **silent system-to-system signal** — no UI banner, no toast, no "welcome back" affordance (inherits the anti-fantasy list in §Player Fantasy). Downstream systems (NPC System §4, Faction State Simulation §15, Day/Night Cycle §5, Faction Events §18, Dialogue System §23) own their own deterministic catch-up handlers; World Structure owns only the timestamp delta computation and event publication. **Handler-absent default is no-catch-up, not partial catch-up** — a downstream system that ships without a `SessionResumeEvent` handler treats load as t=0 rather than producing asymmetric state. **Organic-discovery obligation (D3 amendment, 2026-04-23 round-2 per CD binding reaffirmation):** each Rule-13 subscriber GDD must specify *how the player would notice its handler's effect without a banner* — advanced NPC schedules leaving visible traces, the Faction Board displaying newly-resolved events, rival-writ content appearing in the player's absence, corpse records expiring to unreachable status, etc. Organic-discovery design is a **downstream-subscriber obligation** under the silent-signal contract, not World Structure's responsibility; this GDD owns only the mechanism. Supports Pillar 1 "The World Is Not Your Story" felt experience at T1 without requiring T3 autonomous between-session simulation. *(Added 2026-04-23 per D3 + CD binding; D3 organic-discovery amendment added 2026-04-23 round-2 per CD binding reaffirmation. Downstream contract to be captured as DECISIONS.md D007 after GDD re-review per D6.)*

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior / Memory |
|-------|-----------------|----------------|-------------------|
| `ZoneIdle` | Zone's Addressable groups released; only serialized data references retained in memory | Player triggers transition toward this zone (→ `ZoneLoading`); session end (all ZoneIdle entries cleared) | **Lightweight serialized data only.** Retains: `ZoneManifest` entry (in-memory ScriptableObject reference), baked `NavMeshData` asset reference (if NPC System's catch-up requires path queries during ZoneIdle — otherwise null), spawn-point/boundary-trigger coordinates. Total ≤ 2 MB per zone. **NO retained Unity scene GameObjects, MonoBehaviours, Renderers, Colliders (no live physics bodies), Materials, or Textures.** Addressable scene/texture/navmesh groups are released on entry via `Addressables.UnloadSceneAsync` / `Addressables.Release`. NPC schedule simulation is owned by NPC System's data layer (per Edge D1 + ADR-tba-5); Day/Night world-clock runs independently as a peer (see §Interactions). Re-entry follows the standard `ZoneLoading` → `ZoneActive` path — no special warm-start. |
| `ZoneLoading` | Player enters incoming zone's stream-ahead trigger volume | Player crosses commit threshold (prefab door) and the activation hold is released → `ZoneActive`; or load fails (→ `ZoneError`); or Rule 12 timeout during stream-ahead (→ `ZoneError`) | **Two sub-phases per Rule 5.** *Stream-ahead*: async load in progress in its activation-hold state (the `LoadSceneAsync` + `allowSceneActivation = false` pairing is the intended API mapping — prototype-dependent per ADR-tba-4(a)); `SaveCheckpointing` (Rule 8) fires and must complete here; **player retains full locomotion.** *Commit*: activation hold released and scene made active (the `allowSceneActivation = true` release call is the intended API mapping — prototype-dependent per ADR-tba-4(a)); ≤100 ms player position-lock (`T_activate`). Peak residency during this state bounded by F1's peak form; stream-ahead duration bounded by `zone_overrun_window_seconds` (target <5s). |
| `ZoneActive` | Incoming zone load complete (commit succeeded) | Player exits toward another zone (→ `ZoneUnloading` for this zone); or death event (→ `CorpseRunActive`) | Full systems subscribed; combat AI live (`HauntZone` only); NPC schedules live; audio zone profile active. |
| `ZoneUnloading` | Previously-active zone, incoming zone has just reached `ZoneActive` | Addressable unload completes (→ `ZoneIdle` — metadata-only; all texture/scene/navmesh groups released) | Trigger volume disabled; player cannot re-enter mid-unload. Released groups freed via `Addressables.Release` / `UnloadSceneAsync`. |
| `CorpseRunActive` | Substate of `ZoneActive`. Player death event from Combat Core | Corpse recovery confirmed by Death & Corpse Recovery; or `corpse_run_zone_retention_seconds` expiry (Section G tuning knob) | Zone rules unchanged. Cross-zone corpse data — `zoneId`, world-space `Vector3` position, `expiry_timestamp_utc` — is retained in active session state (`CorpseRecord`), **not** via zone texture retention. Dead zone's Addressable groups are released normally per the `ZoneIdle` contract (metadata-only). On re-entry to the dead zone within the retention window, the standard F2/F3 reload path runs; corpse recovery resumes by matching `CorpseRecord.position` after the zone's assets reload. |
| `SaveCheckpointing` | Short mutex. Fires on: zone-transition initiation (Rule 8), manual save, periodic autosave tick | Save/Load confirms `SaveWriteConfirmed` | Target duration <150 ms (tuning knob `save_mutex_max_ms`). World Structure pauses transition acknowledgment until confirm. |
| `ZoneError` | `ZoneLoading` fails (bundle corrupted, disk full, group missing from catalog) | Non-dismissible UI error; no automatic recovery. Player returns to title via manual action. | Terminal state for the session. Prevents further transitions. Surfaces to Menus & Settings; logs to session audit trail. |

`CrossZoneTransit` is a conceptual label for the sequenced pair (`ZoneLoading` incoming + `ZoneUnloading` outgoing), not a distinct enum value.

### Interactions with Other Systems

| System | Published (this system emits) | Subscribed (this system consumes) | Interface Owner | Hard/Soft |
|---|---|---|---|---|
| **Save / Load** (§2) | `PlayerZoneMembership` (zoneId + `Vector3` position + zoneType), `ZoneTransitionTimestamp`, `last_exit_timestamp_utc` (Rule 13), `CorpseRecord` (zoneId + position + expiry_timestamp_utc) — written on `SaveCheckpointing`. | `SaveWriteConfirmed` (unblocks commit sub-phase). On load-resume: player position + zone ID to restore world state + `last_exit_timestamp_utc` to compute Rule 13 elapsed delta. | Save/Load owns serialization format; World Structure owns the data contract. | **Hard** |
| **NPC System** (§4) | `ZoneActiveEvent(zoneId, zoneType)`, `ZoneUnloadingEvent(zoneId)` — gates schedule ticks and spawn enables. `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` — Rule 13 between-session catch-up. | None. | World Structure publishes; NPC subscribes. NPC System owns handler contract; handler-absent default is no-catch-up. | **Hard** |
| **Combat Core** (§7) | `ZoneTransitionBeginEvent` (triggers Rule 9). `ZoneType` of active zone (Combat gates combat-enable to `HauntZone`). | `PlayerDeathEvent` (→ `CorpseRunActive`). | World Structure publishes zone events; Combat Core owns combat-enable gate. | **Hard** |
| **Day/Night Cycle** (§5) | `ZoneActiveEvent(zoneId, zoneType)` — Day/Night applies zone-specific schedule offsets. `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` — Rule 13 between-session clock advance. | None. | World Structure publishes shared events; Day/Night subscribes. Peer-not-child relationship — neither owns the other; the world-clock runs whether any zone is active. | **Hard** |
| **Faction State Simulation** (§15) | `ZoneType` + `ZoneId` of active zone. `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` — Rule 13 between-session catch-up. | `FactionControlChanged(zoneId)` — flags zone's NPC spawn config stale. | Faction Sim owns faction state + Rule 13 catch-up handler; World Structure owns zone-scope filter + event publication. | **Hard at MVP** (Rule 13 handler required for Pillar 1 T1). **Hard at T3** (autonomous between sessions). |
| **Faction Events** (§18) | `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` — Rule 13 between-session event-queue catch-up. | None (Faction Events polls Save/Load for persisted event state; no WS-consumed events). | World Structure publishes; Faction Events subscribes when its GDD is authored. Handler-absent default is no-catch-up. | **Soft at T1** (handler-absent default — Faction Events GDD not yet authored, no T1 event content with between-session semantics). **Hard at T2+** (when Faction Events GDD lands and rival-writ / between-session events have deterministic catch-up logic). |
| **Zone Control** (§17) | `ZoneActiveEvent(zoneId)` (activates kill-weight attribution). | `ZoneFactionOwnerChanged(zoneId, newFaction)` — persisted via Save/Load. | Zone Control owns ownership calc; World Structure persists result. | **Hard** |
| **Dialogue System** (§23) | `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` — Rule 13 between-session dialogue-memory advance (e.g. NPCs acknowledging elapsed time, scheduled dialogue becoming available). | None. | World Structure publishes; Dialogue subscribes when its GDD authors stateful dialogue. Handler-absent default is no-catch-up. | **Soft at T1–T2** (templated dialogue per DECISIONS.md D004 has no between-session state to advance; handler-absent default). **Hard at T3** (LLM dialogue with NPC memory — per D004 T3 entry gate). |
| **Audio System** (§32) | `ZoneActiveEvent(zoneId, zoneType)`, `ZoneTransitionBeginEvent`. | None. | World Structure publishes; Audio subscribes. | **Soft** (audio degrades to silence if missing — art bible P2 "Stillness Is The Signal" tolerates). |

**Day/Night Cycle (§5) — peer, not child.** Day/Night maintains a world-clock that runs whether any zone is active. Rule 13's `SessionResumeEvent` provides the ordering signal before `ZoneActiveEvent`; Day/Night subscribes to that event to trigger its UTC + fixed project epoch derivation, not to add the clamped elapsed delta to clock state. During-session ticking (player online) is Day/Night's own cadence, independent of zone state. World Structure does NOT own Day/Night. **Day/Night subscribes to `ZoneActiveEvent`** (the same event NPC System, Combat Core, and Zone Control consume) to apply zone-specific schedule offsets (haunt schedules differ from hub), and to `SessionResumeEvent` for between-session derivation ordering. No dedicated Day/Night-only event exists.

### Scope boundary — deferred to ADRs

These are implementation architecture decisions, not design rules — they will become ADRs after this GDD is approved:

1. **ADR-tba — Zone Scene Topology.** Persistent-hub + additive Addressable haunt scenes (provisional; recommended by `engine-programmer` specialist consult).
2. **ADR-tba — Corpse-Run Camera Stack Configuration.** URP camera-stack approach for per-camera desaturation ([art bible S2 State 7](../art/art-bible.md)). Medium-confidence feasible; **prototype required** before commit.
3. **ADR-tba — World State Serialization Contract.** `WorldStateRecord` POCO DTO shape; interaction with [save-integrity rule](../../.claude/rules/save-integrity.md).
4. **ADR-tba — Zone Transition Mechanism.** Stream-ahead via `allowSceneActivation = false` (Rule 5 phase 1) + door-prefab commit via `allowSceneActivation = true` (phase 2); GPU Resident Drawer registration-frame mitigation. **GRD × Addressables unload behavior in 6.3 is unknown — prototype required.** All of the following must be verified as **prototype-dependent assumptions** at Unity 6.3 Addressables package version; the engine reference (`docs/engine-reference/unity/plugins/addressables.md`) reflects ≤6.0 knowledge per VERSION.md, and Rules 1, 5, 12, F1, F1.1, Edge A1, and H-F1.1 all rely on 6.3-specific behaviors that are not settled. Prototype scope expanded 2026-04-23 round-2 per Blockers 8, 9, and 10:
   - **(a) `allowSceneActivation` semantics under the Addressables `AsyncOperationHandle<SceneInstance>` wrapper** (not raw Unity `AsyncOperation`). Verify stream-ahead hold at ~90% with activation suppressed, commit via flag flip, and the stream-ahead/commit latency split assumed by Rule 5 + F3. Failure here invalidates the Rule 5 two-phase split.
   - **(b) `AsyncOperationHandle` cancellation API at Unity 6.3** — specifically whether synchronous cancellation of an in-flight scene load is available. Current design (Edge A1, Rule 12) assumes synchronous cancellation is NOT available (true at Unity 6.0). If it becomes available at 6.3, the flag-and-async-unload pattern simplifies.
   - **(c) `UnloadSceneAsync(handle)` vs `Release(handle)` for a held-but-unactivated scene handle** — which is correct at Unity 6.3. At Unity 6.0 the documented pattern was `Release(handle)` for unactivated handles; `UnloadSceneAsync` applied to loaded-and-activated scenes. Resolution affects Rule 12 abort path, Edge A1 cancellation flow, and ZoneIdle entry release semantics.
   - **(d) Cross-group serialized-dependency release behavior (Blocker 9, HIGH).** The three-group-per-zone split (scene + texture + navmesh) is currently presented as settled in Rules 1, F1, and H-F1.1, but this is **prototype-dependent**: if scene assets carry serialized material/texture references, Addressables may either duplicate textures into the SceneGroup or auto-load TextureGroup as a bundle dependency when the scene loads — either outcome defeats independent release of the TextureGroup on `ZoneIdle` entry, and the H-F1.1 per-zone `Texture2D` label filter would not decompose cleanly. **Procedural-only texture loading may be required as an authoring discipline** (textures referenced by `AssetReference<Texture2D>` + runtime assignment, not serialized material slots) to preserve the three-group contract. Prototype must confirm whether three-group independent release is achievable at 6.3, and if not, document the authoring discipline required — this becomes an art-pipeline constraint flowing back to the art bible.
   - **(e) GPU Resident Drawer byte-count instrumentation** — API (Unity 6.3) for reading GRD-resident byte totals per bundle label, needed by H-EC-C2 (R_peak leak detection) and any GRD-leak assertion. Without this instrumentation H-EC-C2 falls back to coarse Memory Profiler totals and loses bundle-label precision.
   - **(f) Pack Mode authoring constraint** — confirm each zone's Addressable groups must be configured with `Pack Mode = Pack Together` (not Pack Separately) so the zone's bundle boundary is coherent with the release/unload semantics assumed above. If Pack Mode is incorrect, a single zone's assets may end up spread across bundles shared with other zones, violating Rule 1's `ZoneManifest` group-set contract.
   - **(g) `ZoneManifest` field types** — `AssetReference<SceneAsset>` vs string key vs `AssetLabelReference` for the scene, texture, and navmesh group entries. Affects catalog lookup path, load-time behavior, whether the manifest can be populated without Addressables runtime dependency during Editor validation, and whether H-CR-06's Roslyn/field-scan validator sees cross-zone references through the manifest.
   - **(h) Three-group independent release on `ZoneIdle` entry** (original scope item, retained): verify complete release of all three Addressable groups (scene + texture + navmesh) on `ZoneIdle` entry with no GRD leak and no live scene/navmesh references remaining — consistent with the D1 metadata-only ZoneIdle contract (only serialized data references persist). The prototype must NOT retain live scene or navmesh Addressable groups in `ZoneIdle`. Depends on (d) above — if cross-group serialization forces co-resident groups, (h) can only be partially satisfied.
   - **(i) Transitional/commit-phase residency semantics during `ZoneLoading`** (original scope item, retained): stream-ahead + commit, including any case where incoming groups enter residency at staggered times. Feeds F1 peak-form validation.
   
   Unity 6.3 API knowledge gap per [engine-reference VERSION.md](../../docs/engine-reference/unity/VERSION.md); all nine items above are **prototype-required**, not desk-analysable.
5. **ADR-tba — Hub NPC Schedule Tick Semantics.** Real-time tick vs. delta-catch-up vs. discrete-event during `ZoneIdle` in T1 offline (during-session only; between-session ticking is closed by Rule 13 `SessionResumeEvent`). **Elevated to blocker** for Rule 1 revision close-out and for Anchor-moment #1 fidelity — because D1 metadata-only ZoneIdle forbids retained MonoBehaviour Update ticks, NPC schedule simulation must live in NPC System's data layer, not the scene hierarchy. Significant downstream implications for Faction State Simulation and NPC System. Belongs in ADR before T1 implementation begins.

## Formulas

Section D specifies the constraint equations and time budgets that govern World Structure. These are infrastructure formulas, not scaling curves — damage, XP curves, and faction progression live in downstream GDDs. The formulas here produce budgets, derived constraints, and accountability decompositions.

### F1 — Texture Residency (Steady-State + Peak Constraint Equations)

Two-part budget enforcement formula. Answers: "does this zone configuration fit in memory at both steady-state and during transitions?"

**Steady-state** (only one zone's Addressable groups fully loaded; all other zones at `ZoneIdle` metadata-only):

`R_steady = R_always + R_active_current`

**Peak** (mid-transition, stream-ahead sub-phase, both outgoing and incoming zones' texture groups resident simultaneously):

`R_peak = R_always + R_active_outgoing + R_active_incoming`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|----------|--------|------|-------|-------------|
| Always-loaded bytes | `R_always` | float (MB) | 0–350 | Resident bytes in the `AlwaysLoaded` group at all times. MVP baseline: ~45 MB. ZoneIdle metadata (manifest + optional NavMeshData ref, ≤2 MB per zone) is counted here. |
| Current active zone bytes | `R_active_current` | float (MB) | 0–305 | Resident bytes for the single `ZoneActive` zone's texture group during steady-state. |
| Outgoing zone bytes during transition | `R_active_outgoing` | float (MB) | 0–305 | Outgoing zone's texture group during stream-ahead sub-phase (not yet released). |
| Incoming zone bytes during transition | `R_active_incoming` | float (MB) | 0–305 | Incoming zone's texture group loading during stream-ahead sub-phase. |
| Steady residency | `R_steady` | float (MB) | 0–350 | Resident bytes between transitions. |
| Peak residency | `R_peak` | float (MB) | 0–350 | Peak resident bytes at the worst moment (mid-transition). |

**Output Range:** Both `R_steady ≤ 350 MB` AND `R_peak ≤ 350 MB` must hold ([art bible §8.9](../art/art-bible.md) hard cap). Either violation is a **build-blocking authoring error**, not a runtime clamp.

**Example 1 (MVP steady-state, player in HauntMansion):** `R_always` = 45 + `R_active_current` = 140 = **`R_steady` = 185 MB** — 165 MB headroom. CityHub is at `ZoneIdle` metadata-only (~2 MB, counted in `R_always` baseline).

**Example 2 (MVP mid-transition, HauntMansion → CityHub):** `R_always` = 45 + `R_active_outgoing` (HauntMansion) = 140 + `R_active_incoming` (CityHub) = 125 = **`R_peak` = 310 MB** — 40 MB headroom under cap.

**Extreme behavior:** Violation requires zone-group split, lower-res LODs, or trimming `R_always`. The `zone_overrun_window_seconds` tuning knob (Section G) bounds the *duration* of peak residency during the stream-ahead sub-phase, not its magnitude — F1 must be satisfied at authoring time, not runtime. Per D1 metadata-only ZoneIdle, idle zones hold no texture bytes; there is no separate `R_idle` term. *(Rewritten 2026-04-23 per Blocker 4 + performance-analyst math — original F1's `R_transient ≤ R_active` bound was wrong; peak is bounded by the sum of outgoing+incoming.)*

### F1.1 — Zone Art Budget Ceiling (Corollary to F1 Peak Form)

The maximum budget any single zone can consume. Derived by rearranging F1's peak form: the sum of any two zones' texture footprints (the worst-case outgoing+incoming pair) plus `R_always` must fit under the cap.

`R_zone_max = R_cap − R_always − R_zone_peer_max`

Where `R_cap` = 350 MB and `R_zone_peer_max` is the largest OTHER zone in the manifest (not the zone being ceiling-computed). **General constraint across all zones: the sum of the two largest zone texture footprints ≤ 305 MB** (R_cap − R_always).

**Example (MVP, two zones):** For HauntMansion (peer = CityHub at 125 MB): `R_zone_max` = 350 − 45 − 125 = **180 MB** — HauntMansion at 140 MB has 40 MB headroom. For CityHub (peer = HauntMansion at 140 MB): `R_zone_max` = 350 − 45 − 140 = **165 MB** — CityHub at 125 MB has 40 MB headroom. Binding MVP constraint: 125 + 140 = 265 ≤ 305 MB. Art leads need this pairwise constraint before committing to zone density.

**T4 scalability note — R_always zone-count drift (added 2026-04-23 round-2 per Blocker 11, performance-analyst P1 HIGH):** `R_always` is **not fixed** across tiers. Under D1 metadata-only ZoneIdle, each ZoneIdle entry adds ≤2 MB of retained serialized data (manifest + optional NavMeshData ref) to the `AlwaysLoaded` group baseline. At MVP (one ZoneIdle when the other is active), R_always ≈ 45 + 2 ≈ 47 MB. At T4 (four ZoneIdle when one of five is active), R_always ≈ 45 + 4×2 = **~53 MB**, and the peer-sum ceiling tightens from 305 MB to **~297 MB**. Art-team zone budgets inherited from T1 will silently produce **build-blocking F1 violations** at T4 if this drift is not accounted for when new zones are authored. **Authoring discipline:** re-evaluate the pairwise constraint (`two-largest-zones sum ≤ R_cap − R_always_at_that_tier`) each time a new zone is added to the manifest. `R_zone_peer_max` in the F1.1 formula must use the `R_always` value current to the full zone set at that tier, not the MVP baseline. The `zone_art_budget_ceiling` registry entry carries a matching drift note.

**N > 2 general form:** For N zones at the same tier, F1 peak evaluates only the worst outgoing+incoming *pair* at any single transition — the Single Active Zone Invariant (Rule 3) forbids more than one zone in `ZoneActive` at a time, and stream-ahead only overlaps one outgoing + one incoming. The T4 tightening therefore comes entirely from R_always growth (≤2 MB × count of ZoneIdle zones), not from a sum across all N zones. F1's peak form remains correct at any N; only the R_always term drifts. *(Rewritten 2026-04-23 — original F1.1's R_transient_max was self-referential; revised formulation pins `R_zone_peer_max` as the largest OTHER zone; T4 drift note + N > 2 clarification added 2026-04-23 round-2 per Blocker 11.)*

### F2 — Stream-Ahead Trigger Distance

Minimum distance from the zone boundary at which the trigger volume must be placed, so the incoming zone finishes loading before the player crosses the threshold at maximum speed.

`D_trigger = V_max × T_load_s + D_margin`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|----------|--------|------|-------|-------------|
| Max player speed | `V_max` | m/s (float) | 0–10 | Player's maximum sustained movement speed (running; sprint if added). Measured input, not tunable. |
| Zone load time (seconds) | `T_load_s` | s (float) | 0–∞ | Addressable group load time for the stream-ahead sub-phase, profiled on [Min-Spec Profile](#min-spec-profile-profiling-target-for-measured-variables). Measured, not tunable. Cross-references F3's `T_load_ms` — same underlying measurement, different unit. |
| Safety margin | `D_margin` | m (float) | 1–5 | Buffer for frame-timing variance + save-on-transition write. **Tuning knob** (Section G). |
| Trigger distance | `D_trigger` | m (float) | 0–∞ | Minimum distance from zone boundary for stream-ahead trigger-volume placement. |

**Output Range:** Unbounded upward. Values <5 m are a design smell (trigger too close to boundary). Values >40 m are **advisory only, not enforced** — they indicate the zone takes too long to load and should be split.

**Example (MVP):** Running speed = 5 m/s, profiled `T_load_s` = 3 s, `D_margin` = 2 m.
`D_trigger` = 5 × 3 + 2 = **17 m**.

**Extreme behavior:** Teleport or a future mount at 2× `V_max` invalidates existing trigger placements. `T_load_s` must be re-profiled on minimum-spec hardware, not dev machines. If profiled `T_load_s` exceeds ~6 s the trigger distance becomes level-design-disruptive; split the zone. *(Variable renamed 2026-04-23 — `T_load` previously collided across F2 (seconds) and F3 (ms); now `T_load_s` and `T_load_ms` are distinct registry entries.)*

### F3 — Zone Transition Total Time Budget (Time-Budget Identity)

Decomposes zone-transition latency into subsystem-owned slices. Not a scaling formula — an **accountability identity** assigning latency ownership across Save/Load, Addressables, and Unity scene activation. Per Rule 5, only `T_activate` is felt as a player position-lock; `T_save` and `T_load_ms` occur during the stream-ahead sub-phase when the player retains full locomotion.

`T_transition = T_save + T_load_ms + T_activate + T_unload_async`

**Variables:**

| Variable | Symbol | Type | Range | Description | Owner |
|----------|--------|------|-------|-------------|-------|
| Save mutex | `T_save` | ms (int) | 0–`save_mutex_max_ms` | `SaveCheckpointing` state duration during stream-ahead sub-phase; target <150 ms, hard ceiling per Rule 12 tuning knob (Section G; safe range 100–500 ms). | Save/Load |
| Addressable load (ms) | `T_load_ms` | ms (int) | 0–∞ | Group load time for incoming zone during stream-ahead sub-phase. Cross-references F2's `T_load_s` — same measurement, different unit. | Addressables + content |
| Scene activation | `T_activate` | ms (int) | 0–100 | `allowSceneActivation = true` + GRD registration frame + first-frame settle (commit sub-phase). **Only felt component** — player briefly position-locked at threshold. | Engine integration |
| Async unload | `T_unload_async` | ms (int) | 0–∞ | Outgoing zone unload via `Addressables.Release` / `UnloadSceneAsync`. Post-felt (player already in new zone). Unbounded (filesystem/driver-dependent). | Addressables |
| Total | `T_transition` | ms (int) | 0–∞ | From stream-ahead trigger entry to player control restored post-commit. | — |

**Felt latency** = `T_activate` only (≤100 ms; player position-locked at threshold during commit). **Stream-ahead latency** = `T_save + T_load_ms` (player retains locomotion throughout). **Post-felt** = `T_unload_async` (affects `R_peak` overrun duration, not UX).

**Output Range:** Stream-ahead latency target <5,000 ms (corresponds to `zone_overrun_window_seconds`, Section G). Felt commit latency ≤ 100 ms.

**Example (MVP):** `T_save` = 120 ms, `T_load_ms` = 3,000 ms, `T_activate` = 50 ms, `T_unload_async` = 800 ms.
Stream-ahead = 120 + 3,000 = **3,120 ms (~3.1 s)** — within target, player free. Felt commit = **50 ms**. `R_peak` overrun window = 800 ms post-commit.

**Extreme behavior:** If `T_save` spikes toward its 500 ms ceiling (slow disk, heavy save payload), it stacks with `T_load_ms` within stream-ahead but the player remains free. Rule 12's hard timeout fires on `save_mutex_max_ms` — exceeding it fires `SaveTimedOutEvent` and aborts before commit. Derived authoring constraint: `T_save_max + T_load_ms_profiled + T_activate_max < zone_overrun_window_seconds × 1000`. Save system benchmarking is a separate concern (Save/Load GDD). *(Rewritten 2026-04-23 — variable renamed to `T_load_ms`; `T_save` range made parametric on the knob; felt-latency semantics clarified per Rule 5 stream-ahead/commit split.)*

### Corpse Retention Model (replaces F4 — Memory Formula Deprecated Under D1)

Under D1 metadata-only ZoneIdle, the memory-cost formula F4 is removed. Cross-zone corpse retention is a **timestamp/save-state model**, not a texture-retention model. No new formula is required at T1.

When the player dies in zone A and transitions to zone B as active:

- A `CorpseRecord` is written to active session state and persisted on save, containing: `zoneId` (zone A), world-space `Vector3` position, `expiry_timestamp_utc` computed as `now + corpse_run_zone_retention_seconds`.
- Zone A's Addressable groups are released normally per the `ZoneIdle` / `ZoneUnloading` contract (metadata-only retention). Zone A holds **no texture bytes** during the corpse-retention window.
- On re-entry to zone A within the retention window, the standard F2/F3 reload path runs; corpse recovery resumes by matching `CorpseRecord.position` to world geometry after zone A's assets are reloaded.
- If `expiry_timestamp_utc` elapses before re-entry, Death & Corpse Recovery resolves the corpse penalty per its rules and clears the `CorpseRecord`.

**Memory consequence:** The original F4 `R_corpse_zone` term is always 0 under this model. F1's steady and peak forms are sufficient to bound memory; no `R_corpse_zone` term is added.

**T2+ composition flag (preserved from deleted F4 for traceability):** At T2+ with multiple haunt zones, multiple `CorpseRecord` entries may coexist (player dies in HauntA with a prior corpse in HauntB). Each `CorpseRecord` is data-only — no texture retention — so F1 is unaffected regardless of count. The T2+ concern the deleted F4 raised (simultaneous cross-zone memory accumulation pushing `R_peak` over cap) **is closed by this model**, not deferred. *(F4 formula removed 2026-04-23 per D1 metadata-only ZoneIdle + TD binding + Brian's D7 — acceptance criterion H-F4 replaced one-for-one with a corpse-retention contract criterion that validates metadata/timestamp behavior rather than the deleted memory formula.)*

### Formula vs. Tuning-Knob boundary (cross-reference to Section G)

| Value | Owner | Why |
|-------|-------|-----|
| `R_cap` (350 MB) | Section D (input from [art bible §8.9](../art/art-bible.md)) | Hard constraint; not tunable without amending the art bible |
| `R_always`, `R_active_current`, `R_active_outgoing`, `R_active_incoming` | Section D (measured) | Derived from group configuration, not configurable targets |
| `V_max`, `T_load_s`, `T_load_ms`, `T_save`, `T_activate`, `T_unload_async` | Section D (measured) | Profiled inputs; change with content and hardware. See [Min-Spec Profile](#min-spec-profile-profiling-target-for-measured-variables) |
| `D_margin` (1–5 m) | Section G | Authoring constant with a safe range |
| `zone_overrun_window_seconds` (<5 s target) | Section G | Configurable target for stream-ahead latency bound |
| `save_mutex_max_ms` (100–500 ms safe range; 150 ms target) | Section G | Configurable hard ceiling for save subsystem (Rule 12) |
| `corpse_run_zone_retention_seconds` (300 s default) | Section G | Player-time-to-recover policy. **No memory cost under D1 metadata-only ZoneIdle** — the knob now controls only the `CorpseRecord.expiry_timestamp_utc` window. |
| `session_catchup_max_real_seconds` (604,800 s / 7 days default) | Section G | Rule 13 elapsed-time clamp; default aligned with [game-concept.md](game-concept.md) unique-hook "log off for a week" |

### Min-Spec Profile (Profiling Target for Measured Variables)

All measured variables in Section D (`T_load_s`, `T_load_ms`, `T_save`, `T_activate`, `T_unload_async`, `R_active_*`) must be profiled on this minimum-spec hardware configuration, not on dev machines. Dev machines with SSDs mask the `T_load_ms` and `T_save` spikes that drive Rule 12's primary failure mode.

| Component | Spec |
|-----------|------|
| CPU | AMD Ryzen 5 1600 (6c/12t) — Intel i5-7600K (4c/4t) acceptable alternate |
| GPU | NVIDIA GTX 1070 (8 GB VRAM) |
| System RAM | 16 GB DDR4-2400 |
| Storage | 7200 rpm SATA HDD (no SSD caching) — deliberately used to stress `T_load_ms` and `T_save` |
| OS | Windows 10 64-bit 21H2 |

Rationale: representative 2016-era budget platform still well-represented in Steam survey data for indie titles in Gravenspire's target audience. Ryzen 5 1600's 12-thread SMT is a worse case for Unity's job system than the i5's 4 threads, so Ryzen is the primary pin; i5 is acceptable as alternate. Acquisition note: secondary-market Ryzen 5 1600 systems are inexpensive; a dedicated min-spec profiling box is the recommended T1 investment. *(Added 2026-04-23 per performance-analyst recommendation + Blocker 4.)*

## Edge Cases

### A — Transition-boundary edges

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| **A1. Player dies inside a trigger volume during `ZoneLoading` stream-ahead sub-phase** | State machine sets `_transitionCancelled` flag. **Addressables cancellation and release semantics at Unity 6.3 are prototype-dependent — see ADR-tba-4(b) and (c).** Current design assumes: `AsyncOperationHandle` synchronous cancellation is NOT available (true at Unity 6.0; 6.3 unverified) — the operation runs to its ~90% activation hold; the completion callback then suppresses `allowSceneActivation = true` and releases the handle. **The exact release API is also prototype-dependent**: at Unity 6.0 the documented path for a held-but-unactivated scene handle was `Addressables.Release(handle)`; `Addressables.UnloadSceneAsync(handle)` was for loaded-and-activated scenes. Which API is correct for this unactivated-scene path at Unity 6.3 is **unverified** — the engine reference (`docs/engine-reference/unity/plugins/addressables.md`) reflects ≤6.0 knowledge per VERSION.md. ADR-tba-4's prototype must resolve this. Async unload runs in background (bounded by `zone_overrun_window_seconds`). Outgoing zone never leaves `ZoneActive`. Transition to `CorpseRunActive` as substate of outgoing zone. `CorpseRecord` is written with outgoing zone ID + world-space death coords + `expiry_timestamp_utc`. | Rule 8 save already captured pre-transition state; corpse is geometrically in the known outgoing zone; abort is asynchronous at the Addressables API level — it cannot be made synchronous without engine-level cancellation support whose availability at 6.3 is unverified (per unity-addressables-specialist binding, demoted to prototype-dependent 2026-04-23 round-2 per Blocker 8). |
| **A2a. Manual save during `ZoneLoading`** | Request queued; no second `SaveCheckpointing` fires. On exit (to `ZoneActive` or `ZoneError`), queued request discarded — Rule 8 already captured state. | Prevents mid-transition save inconsistency. |
| **A2b. Manual save during `ZoneUnloading`** | Request honored immediately. `SaveCheckpointing` fires; save captures just-entered zone as active; unload continues in parallel. | `ZoneUnloading` does not modify player position or zone membership. |
| **A3. Crash during `SaveCheckpointing`** | Save/Load owns recovery (per [save-integrity rule](../../.claude/rules/save-integrity.md)). Previous complete save reflects pre-transition state; player resumes in outgoing zone. If Save/Load detects unrecoverable corruption, fires `SaveFailedEvent`; new session opens in `ZoneError`. | Rule 8 save is fallback; Save/Load owns HMAC and write-integrity. |
| **A4. Save hangs — `SaveWriteConfirmed` never received** | Per **Rule 12**: if `save_mutex_max_ms` elapses, fire `SaveTimedOutEvent`, transition to `ZoneError`, log elapsed time. | Hard timeout prevents solo-dev "freezes at the door" failure on slow HDD. |

### B — Persistence / version-migration edges

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| **B1. Save file names a zone ID absent from current Addressable catalog** | Transition to `ZoneError` on load-resume. Surface non-dismissible UI: *"Save references a location that no longer exists — your progress may be incompatible with this version."* Log missing ID. **No automatic fallback to a "safe" zone.** | Silent relocation hides authoring bugs. Zone IDs are stable identifiers; removal requires migration script — flag for future ADR (Zone ID Lifecycle Management). |
| **B2. Corpse coordinates fall inside moved/removed geometry after update** | Death & Corpse Recovery owns probe; fires `CorpseUnreachableEvent` if blocked. World Structure clears the `CorpseRecord` on unreachable-confirmation (no memory hold to release under D1 metadata-only ZoneIdle — the record was timestamp/position data only); corpse penalty resolved by Death & Corpse Recovery. | World Structure has no raycasting authority; downstream system owns traversability. |
| **B3. `R_always` manifest disagrees between save and current build** | Not a runtime problem. `R_always` is derived at runtime, not persisted. If a new shared asset pushes `R_always` past F1 ceiling, that's a build-time violation caught by authoring check. | Save records only `PlayerZoneMembership`, not memory baselines. |

### C — Memory / streaming edges

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| **C1. Addressable load exceeds `zone_overrun_window_seconds`** | Log warning to session audit trail with elapsed time + zone ID. No automatic abort. Fire `ZoneLoadOverrunEvent(elapsed_ms)` for Menus & Settings (non-blocking UI indicator). | Mid-stream termination risks bundle corruption. Overrun is advisory; outright failure → `ZoneError` via C3. |
| **C2. `R_peak` exceeds cap at runtime (handle leak, etc.)** | **Not a design edge — a defect.** World Structure cannot detect directly. **Required Dev-build smoke check** (T1 local gate): Memory Profiler snapshot in a PlayMode session after each zone unload, asserting `R_active ≤ R_zone_max` per F1. See H-EC-C2 in Acceptance Criteria. | Rule 7 forbids cross-zone refs; leaks are engine-programmer defects. Prevention lives in validation tooling, not runtime. (CI promotion deferred to T2 per AGENTS.md §6 local-gate policy.) |
| **C3. Bundle corruption detected at runtime** | Addressables throws load-failure exception. `ZoneLoading` → `ZoneError`. Menus & Settings receives `ZoneErrorEvent(reason: BundleCorrupt, zoneId)`. Session cannot continue. Log bundle ID + checksum state. **No retry.** | Retry against corrupt cache fails identically and stalls the player. Reinstall/re-download required. Any future retry policy is an ADR, not a design rule. |
| **C4. Disk full during `SaveCheckpointing`** | Save/Load fires `SaveFailedEvent` back to World Structure → transition to `ZoneError`. Session blocked — cannot proceed with unconfirmed save. | Pillar 5 (Stakes Are Honest): save integrity is not negotiable. |

### D — Policy edges

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| **D1. Hub NPC tick semantics during `ZoneIdle` with player in haunt** | **Resolved 2026-04-24 by NPC System GDD.** Between-session ticking is handled by Rule 13's `SessionResumeEvent`: elapsed-time delta is published to NPC System / Faction Sim at load; each elapsed-time consumer owns its deterministic catch-up handler. Day/Night receives the same ordering signal but derives its clock from current UTC + fixed project epoch, not from elapsed-delta math. During-session answer for NPCs: no live NPC ticks run while the hub is `ZoneIdle`; NPC schedule state remains data-only and is evaluated when the zone becomes active. Under D1 metadata-only ZoneIdle, NPC simulation cannot rely on retained MonoBehaviour Update ticks — NPC System owns the data layer. | Rule 13 closes between-session ambiguity; NPC System closes the during-session NPC answer through data-only delta catch-up plus active-zone ticks, with H-NPC-WS-03 as the residency acceptance gate. |
| **D2. Player teleports into unloaded zone (debug menu, future feature)** | Teleport MUST go through the normal state machine: fire `ZoneLoading`, complete load, reposition. Save-on-transition (Rule 8) fires. Bypassing the state machine is forbidden in all build configs. | Shortcuts bypass save-on-transition, stranding players on crash. |
| **D3. Player position ends up outside any zone trigger** | Fire `ZonePositionInvalidEvent(playerPosition)` → `ZoneError`. No automatic relocation. In-editor: log invalid coords visibly. Shipping build: `ZoneError` terminal. **Pre-ship Editor-validation check** (T1 local gate): Editor menu script scans all navmesh surface points against trigger-volume coverage; out-of-bounds coordinates logged to `production/qa/evidence/world-structure/`. | Silent repositioning masks authoring bugs. Prevention in validation tooling. (CI promotion deferred to T2 per AGENTS.md §6 local-gate policy.) |

### E — Cross-system edges

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| **E1. Faction State Simulation polls zone state during `ZoneLoading`** | Returns previous zone's membership (pre-transition state). No additional handling. | `ZoneActiveEvent` fires only on `ZoneActive` entry. Faction Sim must subscribe to events, not poll. Rule 7 already forbids direct cross-zone object refs. |
| **E2. Combat Core damage event fires during `ZoneTransitionBegin`** | Rule 9 governs: Combat Core resolves in-flight effects per its rules, strips persistent, cancels mid-cast. Combat Core GDD must specify exact discard rule for outgoing-zone projectiles/DOTs. | World Structure's contract complete at firing `ZoneTransitionBeginEvent`. Combat Core owns resolution. |

**ADR candidates surfaced by these edges:**
- **ADR-tba — Zone ID Lifecycle Management.** Stable-identifier contract for zone IDs; migration-script requirement when zones are deprecated (from B1).
- Existing ADR-tba-5 (Hub NPC Schedule Tick Semantics) resolved 2026-04-24 by NPC System GDD (from D1).

## Dependencies

World Structure is Layer 1 Foundation — **no upstream dependencies.** **Eleven** downstream systems depend on it (*expanded from nine in 2026-04-23 round-3 per Blocker B — Faction Events §18 and Dialogue System §23 added as Rule-13 subscribers to match Rule 13 and Anchor-3 prose*); listed below with direction, data-interface summary, hard/soft classification, and interface ownership. For interface specifics (event payloads, data contracts), see Section C §Interactions.

| System | Direction | Nature / Data Interface | Hard/Soft | Interface Owner |
|---|---|---|---|---|
| **Save / Load** (§2) | WS emits for | `PlayerZoneMembership` (zoneId + `Vector3` position + zoneType), `ZoneTransitionTimestamp`, `last_exit_timestamp_utc` (Rule 13), `CorpseRecord` (zoneId + position + expiry_timestamp_utc) via `SaveCheckpointing`; receives `SaveWriteConfirmed` / `SaveFailedEvent` | **Hard** | Save/Load owns serialization format; WS owns data contract |
| **Menus & Settings** (§3) | WS depended on by | Publishes `ZoneErrorEvent`, `ZoneLoadOverrunEvent`, `ZoneTransitionBeginEvent` for UI affordance; receives manual-save triggers (routed via Save/Load) | **Soft** | WS publishes; Menus owns UI presentation |
| **NPC System** (§4) | WS depended on by | Publishes `ZoneActiveEvent(zoneId, zoneType)`, `ZoneUnloadingEvent(zoneId)` to gate schedules/spawns; `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` for Rule 13 between-session catch-up | **Hard** | WS publishes; NPC subscribes. Rule 13 handler required at T1 (handler-absent default = no-catch-up) |
| **Day/Night Cycle** (§5) | Peer (not child) | Day/Night subscribes to `ZoneActiveEvent` (shared event; same emission as NPC/Combat/Zone Control) for zone-specific schedule offsets AND to `SessionResumeEvent` for Rule 13 between-session clock advance; Day/Night's world-clock ticks independently, including during `ZoneIdle` | **Hard** | Peer systems — neither owns the other |
| **Combat Core** (§7) | WS depended on by | Publishes `ZoneType`, `ZoneTransitionBeginEvent` (Rule 9); receives `PlayerDeathEvent` | **Hard** | WS publishes zone events; Combat owns combat-enable gate and in-flight resolution |
| **Death & Corpse Recovery** (§14) | Bidirectional | WS persists `CorpseRecord` (`zoneId` + world-space `Vector3` position + `expiry_timestamp_utc`) in save state; releases all Addressable groups (scene, texture, navmesh) of the dead zone normally per the `ZoneIdle` metadata-only contract (no memory hold); receives `CorpseUnreachableEvent` and clears `CorpseRecord` on unreachable-confirmation or `expiry_timestamp_utc` elapse | **Hard** | Death & Corpse owns probe, recovery, and unreachable resolution; WS owns `CorpseRecord` persistence + `corpse_run_zone_retention_seconds` knob (timestamp-only, no memory cost under D1 metadata-only ZoneIdle) |
| **Faction State Simulation** (§15) | WS depended on by | Publishes `ZoneType` + `ZoneId`; `SessionResumeEvent` for Rule 13 between-session catch-up; receives `FactionControlChanged(zoneId)` | **Hard at MVP** (Rule 13 handler required for Pillar 1 at T1) → **Hard at T3** (autonomous between sessions) | Faction Sim owns state + Rule 13 handler; WS owns zone-scope filter + event publication |
| **Faction Events** (§18) | WS depended on by | Publishes `SessionResumeEvent` for Rule 13 between-session event-queue catch-up | **Soft at T1** (handler-absent default — GDD not yet authored) → **Hard at T2+** (when Faction Events GDD lands) | Faction Events owns event-queue semantics + Rule 13 handler; WS owns event publication |
| **Zone Control** (§17) | WS depended on by | Publishes `ZoneActiveEvent(zoneId)` (activates kill-weight attribution); receives `ZoneFactionOwnerChanged` for persistence via Save/Load | **Hard** | Zone Control owns ownership calc; WS owns persistence of result |
| **Dialogue System** (§23) | WS depended on by | Publishes `SessionResumeEvent` for Rule 13 between-session dialogue-memory advance | **Soft at T1–T2** (templated dialogue per D004 has no between-session state) → **Hard at T3** (LLM dialogue with NPC memory per D004 T3 entry gate) | Dialogue owns dialogue-memory semantics + Rule 13 handler when LLM dialogue lands; WS owns event publication |
| **Audio System** (§32) | WS depended on by | Publishes `ZoneActiveEvent`, `ZoneTransitionBeginEvent`; Audio selects ambient profile from zone ID | **Soft** — degrades to silence gracefully ([art bible P2](../art/art-bible.md) "Stillness Is The Signal" tolerates missing audio) | WS publishes; Audio subscribes |

### Bidirectional consistency contract

Each downstream GDD, when authored, must declare World Structure in its own §Dependencies with the reverse listing (`depends on: World Structure` — hard/soft matching this table). `/consistency-check` and `/review-all-gdds` verify bidirectional agreement. Any mismatch = one GDD is wrong and needs amending.

> **Reverse dependency map sync — follow-up batch (added 2026-04-23 round-3 per Blocker B4):** the `design/gdd/systems-index.md` reverse dependency map (§Dependency Map) currently reflects the round-2 nine-subscriber table. Now that Faction Events (§18) and Dialogue System (§23) are canonical Rule-13 subscribers here, `systems-index.md` requires matching reverse-map entries for both, and the count of World Structure downstream subscribers must be updated in the index. **This sync is out of scope for the current approved batch** — it belongs to a separate follow-up batch that also touches `systems-index.md`. Flag reflected in the active session state and/or the review log by the reviewer.

### Upstream

**None.** World Structure is Layer 1 in the dependency graph per [systems-index.md](systems-index.md) §Dependency Map — it depends on nothing and is the root of every gameplay feature.

## Tuning Knobs

Five designer-adjustable values drive World Structure's runtime behavior (updated 2026-04-23: `session_catchup_max_real_seconds` added per Rule 13). Per the Section D vs. Section G boundary: profiled inputs (`V_max`, `T_load_s`, `T_load_ms`, `T_save`, `T_activate`, `T_unload_async`) and the art-bible-locked `R_cap` are **not** knobs — they're inputs to formulas in Section D.

| Parameter | Current Value | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|--------------|------------|-------------------|-------------------|
| `D_margin` — F2 safety buffer for stream-ahead trigger distance | 2 m | 1 – 5 m | More loading slack; trigger volume further from zone boundary. Authored geometry must accommodate the longer approach corridor. | Tighter trigger placement; risk of late-loads (player crosses commit threshold before stream-ahead load completes) if `T_load_s` spikes. |
| `zone_overrun_window_seconds` — F1 `R_peak` transient-overlap duration bound | 5 s | 3 – 10 s | More tolerance for slow disks and large zone groups; longer window where both zones' textures are resident simultaneously. | Stricter budget; more warnings in session audit log on transient load hiccups. Below 3 s risks false-triggering `ZoneError` on minimum-spec hardware. |
| `save_mutex_max_ms` — Rule 12 hard timeout for `SaveCheckpointing` | 150 ms | 100 – 500 ms | Tolerates slower Save/Load (slow disk, large save payload); delays player transition perceptibly. Above 500 ms becomes noticeable as a hitch. | Stricter save deadline; may trigger spurious `SaveTimedOutEvent` → `ZoneError` on minimum-spec HDDs. Below 100 ms risks failing on any disk contention. |
| `corpse_run_zone_retention_seconds` — Player time window to recover a cross-zone corpse (timestamp-based under D1 metadata-only ZoneIdle; no memory cost) | **300 s (5 min) — revisit at T1 playtest** | 2 min – 10 min | More forgiving corpse-recovery UX; player can take time to return before `CorpseRecord.expiry_timestamp_utc` elapses. | Less forgiving corpse UX; phone-call interruptions may miss the window → effective death penalty increase. |
| `session_catchup_max_real_seconds` — Rule 13 elapsed-time clamp for `SessionResumeEvent` | **604,800 s (7 days)** — aligned with [game-concept.md](game-concept.md) unique-hook "log off for a week" | 3,600 s – 2,592,000 s (1 hour – 30 days) | Wider catch-up window; richer offline-change fantasy; more deterministic simulation burden on NPC / Faction Sim / Day-Night handlers at session resume. | Narrower window; shorter offline-change fantasy; lighter catch-up computation. Below 1 hour risks breaking the Pillar 1 "world kept moving" experience for normal absences (work, sleep). Above 30 days implies time-based story arcs that may not be designed yet. |

> **`corpse_run_zone_retention_seconds` initial value rationale:** 5 minutes is enough for a player to travel back from nearly anywhere in a T1 haunt + hub, short enough that it doesn't dominate player time when they're stuck, and long enough that a phone-call interruption doesn't compound the death penalty. Revisit during T1 playtest: measure actual corpse-run durations and tune to the 75th percentile of observed recovery times.
>
> **Safe-range floor raised (2026-04-22 design review):** original 30 s floor was punitive against real-world interruptions (phone call, bathroom break, doorbell) and risked compounding an already-legible death penalty with an arbitrary clock penalty. 2 min preserves a reasonable under-pressure floor while keeping T1 playtest-tuned values (likely 3–7 min) well above it.

### Tuning-knob interactions

- **`zone_overrun_window_seconds` × `save_mutex_max_ms`** — Both contribute to the F3 stream-ahead latency budget. If `save_mutex_max_ms` is set near its 500 ms ceiling, the effective budget for `T_load_ms` shrinks. Authoring constraint: `T_save_max + T_load_ms_profiled + T_activate_max < zone_overrun_window_seconds × 1000`.
- **`D_margin` × `zone_overrun_window_seconds`** — Spatial and temporal buffers absorbing the same risk (slow load). Don't set both to max simultaneously — level-design pain without additional safety.
- **`session_catchup_max_real_seconds` × downstream handler tolerance** — Rule 13 publishes the clamped elapsed delta; downstream handlers that consume elapsed time must bound catch-up computation so that a 7-day (default) or 30-day (max) delta does not stall session load. NPC System / Faction Sim handlers own this performance budget independently per Rule 13's handler-absent-default-is-no-catch-up contract. Day/Night is exempt from the elapsed-delta budget because its approved clock model derives from current UTC + fixed project epoch in constant time. *(Previous corpse-retention × F1.1 interaction removed 2026-04-23 — under D1 metadata-only ZoneIdle, corpse retention has no memory cost.)*

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
| Zone transition | No World-Structure-owned visual — color shifts are material-and-light-driven per [art bible §4.5](../art/art-bible.md) | Audio zone profile swap on `ZoneActiveEvent` (Audio System selects) | Audio System |
| Corpse-run desaturation | Per-camera -40% desaturation on dead player's Overlay Camera — **ADR-tba-2** owns the camera-stack architecture | None from this GDD | engine-programmer + unity-shader-specialist |
| `SaveCheckpointing` | Optional save-in-progress indicator (small, bottom-right) — may be invisible given 150 ms target | None | Menus & Settings |

> **📌 Asset Spec flag** — Once per-zone content exists, run `/asset-spec system:world-structure` for any World-Structure-owned asset. Likely minimal: loading-indicator icons, error-modal backing, and the corpse-run camera-stack prefab (if one emerges from ADR-tba-2). The vast majority of zone visuals are owned by per-zone content, not by this GDD.

## Game Feel

[N/A — World Structure is infrastructure. Player-facing feel targets live in downstream systems: Combat Core (combat feel), Character Progression / Character Creation (traversal), Camera system, Death & Corpse Recovery (corpse-run pacing). Revisit if zone-transition animations or camera behavior get pulled into this GDD's scope during section C.]

## UI Requirements

World Structure does not own any primary UI. It triggers the following UI states via events published to Menus & Settings.

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| "Loading..." indicator (low-opacity) | Bottom-right HUD corner | Shown on `ZoneLoadOverrunEvent`; hidden on `ZoneActiveEvent` | Only when `T_load_s > zone_overrun_window_seconds` — a normal transition does not show this |
| `ZoneError` modal | Screen-center, non-dismissible | Once, on entry | `ZoneError` state entered (Edge B1 missing-zone, C3 bundle corruption, A4 save timeout, D3 invalid position) |
| Save-in-progress indicator | Bottom-right HUD corner (optional — may be invisible given 150 ms target) | During `SaveCheckpointing`, max `save_mutex_max_ms` | Optional affordance; Menus decides whether to render |

> **📌 UX Flag — World Structure**: In pre-production, run `/ux-design` for the `zone-error-modal` and `loading-indicator` UI elements. This GDD specifies triggers and events; the visual/interaction design of the error modal and indicator goes in the UX spec, not here.

## Cross-References

Consolidated declared dependencies on other project artifacts. Machine-checkable by `/review-all-gdds` Phase 2c.

| This Document References | Target | Specific Element | Nature |
|---|---|---|---|
| "zone = `ZoneManifest` entry publishing a stable `zoneId`, resolving to a logical group-set whose conceptual boundary aligns with §8.7; runtime binding shape prototype-dependent per ADR-tba-4(d)(g)" (Rule 1, F1, F1.1) | `design/art/art-bible.md` §8.7 | Zone-boundary unit (conceptual alignment) | Rule dependency |
| "≤350 MB texture residency" (F1, Section G `R_cap`) | `design/art/art-bible.md` §8.9 | `R_cap` = 350 MB | Data dependency |
| "≤3–4 unique 2K surfaces per group" (Section C context) | `design/art/art-bible.md` §6.2 | Per-group art budget | Rule dependency |
| "no post-process LUT swap at zone boundary" (Rule 10) | `design/art/art-bible.md` §4.5 | Zone-transition visual constraint | Rule dependency |
| "exterior decal projectors forbidden at city density" (Section C context) | `design/art/art-bible.md` §8.7 | Rendering constraint | Rule dependency |
| "corpse-run per-camera desaturation" (ADR-tba-2) | `design/art/art-bible.md` §2 State 7 | Camera-stack requirement | State trigger |
| Pillar 1 "The World Is Not Your Story" | `design/gdd/game-concept.md` | Persistence pillar | Rule dependency |
| Pillar 2 "The Silence Is Sacred" | `design/gdd/game-concept.md` | Pacing pillar | Rule dependency |
| Pillar 5 "Stakes Are Honest" | `design/gdd/game-concept.md` | Save integrity / failure legibility | Rule dependency |
| "Unity 6.3 LTS + URP + Addressables" | `DECISIONS.md` D001 | Engine lock | Rule dependency |
| "T1 single-player offline" | `DECISIONS.md` D003 | Tier scope | Rule dependency |
| "HMAC-signed local saves" | `.claude/rules/save-integrity.md` | Save integrity rule | Rule dependency |
| "Unity 6.3 render graph; `SetupRenderPasses` deprecated" | `docs/engine-reference/unity/VERSION.md` | Engine API gap | Rule dependency |
| "Rule 13 SessionResumeEvent supports Pillar 1 at T1" | `design/gdd/game-concept.md` | Pillar 1 "The World Is Not Your Story" / Retention Hook #1 / Unique-hook "log off for a week" language | Rule dependency |
| "ZoneManifest authoritative registry binds ≥1 Addressable groups sharing §8.7 boundary" | `design/art/art-bible.md` §8.7 | Boundary-unit definition (coincident with Addressable streaming-group boundary) | Data dependency (reinforces Rule 1 reframe) |
| "`allowSceneActivation` stream-ahead + commit pattern (Rule 5)" | `docs/engine-reference/unity/VERSION.md` | Unity 6.3 SceneManager / Addressables API | Engine API reference |
| "`AsyncOperationHandle` not synchronously cancellable (Edge A1)" | `docs/engine-reference/unity/VERSION.md` | Unity 6.3 Addressables cancellation semantics | Engine API reference |

## Acceptance Criteria

Testable conditions that prove World Structure works as designed. Organized by Core Rule coverage (Rules 1–13), Formula coverage (F1, F1.1, F2, F3, Corpse Retention Model), High-Risk Edge Case coverage, and Cross-System interface integrity. Every criterion uses **Given-When-Then** format; each is tagged with test type, owner, and T1-blocking status. Summary table at end.

#### T1 Test-Type Taxonomy (WS-local; project-wide promotion flagged as follow-up per D5)

Per AGENTS.md §6, T1 is "None — local gate only" — no CI. Acceptance criteria use one of five categories:

| Category | Definition | Evidence destination |
|---|---|---|
| **Editor-validation** | Unity Editor menu script, import-time check, Roslyn analyzer, or `IPreprocessBuildWithReport` callback. Runs in Editor without PlayMode. | Console log → `production/qa/evidence/world-structure/` |
| **Dev-build smoke** | PlayMode session against a Development build. Manual run. | Memory Profiler snapshot / screenshot / log file → `production/qa/evidence/world-structure/` |
| **Profiled playtest** | Manual run on [Min-Spec Profile](#min-spec-profile-profiling-target-for-measured-variables) hardware with Unity Profiler. | `.data` file + screenshot |
| **Unit** | Unity Test Framework NUnit test. Deterministic, isolated, no scene dependency. | UTF green/red |
| **Integration** | Unity Test Framework PlayMode test. Runs in scene context. | UTF green/red |

CI-smoke (GameCI) applies at T2+, not T1. This taxonomy is local to World Structure for now; promotion to `.claude/rules/test-standards.md` is deferred until Save/Load GDD adopts it consistently (flagged follow-up per D5, 2026-04-23 revision).

### Core Rules coverage (C1–C12)

**H-CR-01 — Rules 1 + 11 (Zone Definition + Tier Scalability)** — **T4-DEFERRED**
**GIVEN** the `ZoneManifest` ScriptableObject is the sole zone registry, **WHEN** a gameplay-programmer adds a new zone entry at T4 (up to 5 zones), **THEN** the zone loads, activates, and unloads without any code change to the state machine or transition logic — only manifest data changes.
*Integration | gameplay-programmer | **T4-deferred** (demoted 2026-04-23 per D4 — a T1 synthetic proxy would pass for reasons unrelated to the Tier Scalability Contract; validate this when T4 zone-addition work actually begins)*

**H-CR-02 — Rule 2 (Zone Types)**
**GIVEN** the player enters a `HauntZone`, **WHEN** `ZoneActiveEvent` is published, **THEN** Combat Core enables combat AI and Zone Control enables kill-weight attribution; both are disabled when the active zone is `CityHubZone`.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CR-03 — Rules 3 + 6 (Single Active Zone + Unload Policy)**
**GIVEN** a zone transition is initiated, **WHEN** the state-machine log is captured, **THEN** no frame reports two zones simultaneously in `ZoneActive`; the outgoing zone does not begin unloading until the incoming zone has confirmed `ZoneActive`.
*Integration (automated state-log assertion) | engine-programmer | T1-blocking*

**H-CR-04 — Rule 4 (Player Position Is Authoritative; zoneId Persisted as Cross-Check)**
**GIVEN** a save file written during `SaveCheckpointing`, **WHEN** the session reloads, **THEN** zone membership is re-derived from the persisted `Vector3` position AND cross-checked against the persisted `zoneId` field; mismatch fires `ZoneError` per Edge B1; the persisted `zoneId` is a cross-check, not a cache (position remains authoritative).
*Unit | gameplay-programmer | T1-blocking*

**H-CR-05 — Rule 5 (Two-Phase Transition — Stream-Ahead + Commit)**
**GIVEN** the player approaches a zone boundary at `V_max`, **WHEN** they enter the stream-ahead trigger volume, **THEN** (a) the player retains full locomotion during the stream-ahead sub-phase; (b) the incoming zone's Addressables load reaches its `allowSceneActivation = false` hold before the player reaches the authored commit threshold (verified via `D_trigger` placement per F2); (c) the commit phase imposes a ≤100 ms position-lock (`T_activate`) and no longer.
*Profiled playtest (minimum-spec hardware; see §Min-Spec Profile) | engine-programmer + qa-tester | T1-blocking*

**H-CR-06 — Rule 7 (Cross-Zone Reference Policy)**
**GIVEN** any two zones are simultaneously resident (mid-transition stream-ahead sub-phase), **WHEN** a Unity Editor reference-validator script (walking `AssetDatabase` + scanning serialized fields) plus a Roslyn analyzer / text grep over source executes, **THEN** no Unity scene-graph object reference crosses zone boundaries; all cross-zone persistent-state reads go through the Save/Load service. Validator runs as Editor menu item plus `IPreprocessBuildWithReport` callback. Evidence log in `production/qa/evidence/world-structure/`.
*Editor-validation | engine-programmer | T1-blocking*

**H-CR-07 — Rule 8 (Save-on-Transition Boundary)**
**GIVEN** the player initiates a zone transition, **WHEN** the app is force-quit at any point during `ZoneLoading`, **THEN** the next session resumes in the outgoing zone at the pre-transition player position, with no data loss.
*Integration (manual force-kill test) | qa-tester | T1-blocking*

**H-CR-08 — Rule 9 (WS-Owned Event Publication Contract)**
**GIVEN** the player initiates a zone transition (enters the stream-ahead trigger volume), **WHEN** state enters `ZoneLoading`, **THEN** `ZoneTransitionBeginEvent(outgoingZoneId, incomingZoneId)` is published to all subscribers within the same frame and appears in the event log with correct payload. **Combat Core's in-flight effect discard logic is tested in Combat Core GDD §H — not here.** World Structure's contract completes at event publication.
*Integration (event-log assertion in PlayMode test) | engine-programmer | T1-blocking*

**H-CR-09a — Rule 10 (Color Temperature Static Assertion)**
**GIVEN** the source codebase, **WHEN** a static-analysis script inspects all call sites in the zone-transition code path (`ZoneStateMachine`, `ZoneTransitionController`, `ZoneLoader`, Rule 13 publisher), **THEN** no call site modifies a `VolumeProfile` blend weight, LUT reference, `RenderSettings`, or `LightingSettings` global state. Evidence: script output log in `production/qa/evidence/world-structure/`.
*Editor-validation | engine-programmer | T1-blocking*

**H-CR-09b — Rule 10 (Color Temperature Runtime Instrumentation)**
**GIVEN** a PlayMode test that triggers a `CityHub → HauntMansion` transition, **WHEN** `ZoneTransitionBeginEvent` fires, **THEN** an instrumentation listener confirms no `VolumeProfile.weight`, `RenderSettings`, or `LightingSettings` property was set during the transition frame sequence. Evidence: UTF test green.
*Integration | engine-programmer | T1-blocking*

**H-CR-09c — Rule 10 (Color Temperature Art-Lead Visual Check)**
**GIVEN** an in-Editor Dev-build Frame Debugger session, **WHEN** the tester manually captures frames N-1 and N at the zone-boundary crossing, **THEN** no URP post-process Volume blend or LUT swap appears in the render pass list; art-lead signs off on the frame capture. Evidence: annotated screenshot in `production/qa/evidence/world-structure/`.
*Dev-build smoke (manual Frame Debugger capture + art-lead sign-off) | engine-programmer + art-lead | advisory*

**H-CR-10 — Rule 12 (Save-on-Transition Hard Timeout)**
**GIVEN** `SaveCheckpointing` is entered during the stream-ahead sub-phase and `SaveWriteConfirmed` is withheld (test harness), **WHEN** `save_mutex_max_ms` elapses, **THEN** `SaveTimedOutEvent` fires, the transition aborts before commit, the state machine transitions to `ZoneError`, and elapsed time is written to the session audit trail — no hang or freeze.
*Unit | gameplay-programmer | T1-blocking*

**H-CR-13a — Rule 13 (Last-Exit Timestamp Persisted)**
**GIVEN** the player exits a session normally (or the session ends via any clean path), **WHEN** the save file is inspected, **THEN** `last_exit_timestamp_utc` is present and non-default, encoded as UTC epoch seconds.
*Unit | gameplay-programmer | T1-blocking*

**H-CR-13b — Rule 13 (SessionResumeEvent Firing Order — T1 scope)**
**GIVEN** a session load from a save file containing `last_exit_timestamp_utc`, **WHEN** a synthetic test fixture that subscribes to `SessionResumeEvent` via the project's standard event-bus subscription API (the same API any production Rule-13 subscriber uses — no bespoke interface required, the event-bus dispatch pattern *is* the subscriber contract) inspects its received-event log, **THEN** the fixture observes `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` BEFORE any `ZoneActiveEvent` fires in the session.

*T1 scope narrowed 2026-04-23 round-2 per Blocker 2:* the original multi-subscriber assertion ("received by all Rule-13 subscribers — NPC System, Faction Sim, Day/Night, Faction Events, Dialogue") is **vacuously true at T1** because 4 of 5 named subscribers do not yet have GDDs, and the handler-absent default is no-catch-up. Narrowed to "at least one synthetic test fixture observes correct ordering." Multi-subscriber firing-order verification migrates to each Rule-13 subscriber GDD as a downstream-GDD obligation (see Explicit non-criteria + Rule 13 D3 organic-discovery amendment).

*Round-3 (2026-04-23) per Blocker C1.5:* prior reference to an `ISessionResumeSubscriber` interface was undefined in this document and has been removed — this GDD does not define any subscriber interface beyond the project's standard event-bus subscription API. The "test fixture" above is simply a component that subscribes to `SessionResumeEvent` through that same API; the test registers such a fixture before session load and reads its event log after. If the project's event-bus substrate is later specified to require a typed subscriber interface, that specification is an architecture concern captured elsewhere — it is not a contract World Structure originates.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CR-13c — Rule 13 (Elapsed Clamp — with boundary cases)**
**GIVEN** save files spanning the full elapsed-delta range, **WHEN** `SessionResumeEvent` fires, **THEN** the `real_elapsed_seconds` payload respects the clamp in all four cases:
(a) **Above ceiling** — `now − last_exit_timestamp_utc > session_catchup_max_real_seconds` → payload equals `session_catchup_max_real_seconds` (hard-clamped, not the raw delta);
(b) **Boundary equality** — `now − last_exit_timestamp_utc == session_catchup_max_real_seconds` → payload equals the raw delta exactly; the clamp fires only on strict *exceed*, not on equality (passes through unclamped);
(c) **Zero delta (same-second resume)** — `now − last_exit_timestamp_utc == 0` → payload equals 0 and `SessionResumeEvent` still fires with zero delta (downstream handlers are responsible for treating 0 as no-op; the event must still publish to preserve firing-order guarantees for synthetic subscribers per H-CR-13b);
(d) **Negative delta (clock skew)** — `now − last_exit_timestamp_utc < 0` → payload clamps to 0; event still fires.
*Unit | gameplay-programmer | T1-blocking (boundary cases (b) and (c) added 2026-04-23 round-2 per Blocker 4)*

### Formula coverage (F1–F4)

*H-F1 (compound steady + peak criterion) was split into H-F1-steady and H-F1-peak on 2026-04-23 round-3 per Blocker C1. The steady-state form is a stable, reproducible gate. The peak form depends on a **prototype-defined peak observable seam** per ADR-tba-4(i) and is therefore ADR-gated, not T1-blocking, until that seam is named. Midpoint timing arithmetic (the round-2 approach) is removed as it is not a reproducible observable — the prototype must name the specific moment during `ZoneLoading` at which outgoing + incoming texture groups can be reliably observed resident together.*

**H-F1-steady — Steady-State Texture Residency (F1, steady form)**
**GIVEN** any authored zone configuration, **WHEN** a Unity Memory Profiler snapshot is taken on [Min-Spec Profile](#min-spec-profile-profiling-target-for-measured-variables) hardware while the player is in `ZoneActive` with no transition in progress, **THEN** `R_steady = R_always + R_active_current ≤ 350 MB`. Violation is authoring-time build-blocking. This form uses only declared Section D variables and a state (`ZoneActive`) that is itself a stable contract; no prototype-dependent seam is required to reach it.
*Dev-build smoke (PlayMode Memory Profiler, steady-state snapshot — not timing-dependent) | engine-programmer | T1-blocking*

**H-F1-peak — Peak Texture Residency (F1, peak form) — ADR-gated**
**GIVEN** any authored zone configuration, **WHEN** a Unity Memory Profiler snapshot is taken on [Min-Spec Profile](#min-spec-profile-profiling-target-for-measured-variables) hardware at **the peak observable seam named by ADR-tba-4(i)** during `ZoneLoading` (the specific, prototype-verified moment at which outgoing + incoming texture groups can be reliably observed resident together), **THEN** `R_peak = R_always + R_active_outgoing + R_active_incoming ≤ 350 MB`. Violation is authoring-time build-blocking.

**ADR-gating (2026-04-23 round-3 per Blocker C1):** this AC **does not gate T1** until ADR-tba-4(i) resolves and names the peak observable. The prototype is required to define both *how* the stream-ahead mid-transition snapshot is captured deterministically (harness-triggered from a state-machine transition or an Addressables async-load callback — not operator-timed, not midpoint arithmetic) and *at what state-machine moment* the snapshot is taken. Once the observable is named, H-F1-peak becomes T1-blocking with that observable as its verification instrument, and the Summary Table is updated accordingly. Prior round-2 text that recommended `T_save_target + T_load_ms_p95_profiled / 2` midpoint timing is withdrawn — midpoint arithmetic across a variable-length async load is not a reproducible seam and those symbols were not declared in Section D anyway.
*Dev-build smoke (PlayMode Memory Profiler, peak-seam snapshot harness-triggered per ADR-tba-4(i) resolution) | engine-programmer | **ADR-gated** — unlocks T1-blocking status when ADR-tba-4(i) resolves*

*H-F1.1 (compound local + global criterion) was split into H-F1.1a and H-F1.1b on 2026-04-23 round-3 per Blocker C2.5. The local per-zone budget and the global pairwise invariant are independently verifiable assertions and should not fail or pass together. Both are Dev-build smoke (Memory Profiler PlayMode) and both carry the bundle-label attribution caveat per Blocker C2.*

**Shared caveat for H-F1.1a, H-F1.1b, H-F4a, H-EC-C2 — per-zone bundle-label attribution (authored 2026-04-23 round-3 per Blocker C2; fallback forms pre-specified 2026-04-23 round-4 per Blocker 3 — these ACs are T1-runnable on day one regardless of ADR-tba-4(d) outcome):** These ACs assume that `Texture2D` resident bytes can be attributed per-zone. **Primary attribution method:** Addressable bundle-label filter of resident `Texture2D` objects grouped by the zone's bundle label. **Pre-specified fallback (authored here — no ADR-naming step required for T1 gate execution):** if the prototype shows cross-group serialized dependencies prevent clean per-label filtering at Unity 6.3 Addressables (per ADR-tba-4(d)), each AC switches to one of two criterion-specific delta methodologies, chosen by AC type.

- **Fallback Form L (load-side delta) — used by H-F1.1a / H-F1.1b (active-zone budget gates).** Baseline snapshot = Memory Profiler `Texture2D` total taken immediately *before* the zone's load begins (pre-`ZoneLoading` entry for that zone). Post-load snapshot = `Texture2D` total taken immediately *after* the zone reaches `ZoneActive` (steady-state, no transition in progress). Zone-attributable bytes = (post-load total) − (baseline total). **H-F1.1a** asserts this load-side delta ≤ `R_zone_max`. **H-F1.1b** asserts the sum of any two zones' load-side deltas ≤ `R_cap − R_always_at_current_tier`.
- **Fallback Form U (unload-side delta) — used by H-F4a / H-EC-C2 (post-unload leak gates).** Baseline snapshot = Memory Profiler `Texture2D` total taken immediately *before* the zone's unload begins (pre-`ZoneUnloading` entry for that zone). Post-unload snapshot = `Texture2D` total taken immediately *after* the zone reaches `ZoneIdle` (unload complete). Zone-released bytes = (baseline total) − (post-unload total). **H-F4a / H-EC-C2** assert that this unload-side released-delta equals the zone's live residency from the baseline snapshot — i.e., no bytes retained for this zone after unload. The "= 0 bytes for this zone after unload" assertion is expressed as a delta-equality when per-label isolation is unavailable (released = baseline means nothing from this zone was retained).

Both fallback forms are T1-runnable on day one. ADR-tba-4(d) may later refine attribution (if bundle-label isolation turns out achievable at Unity 6.3, the primary method holds and these fallbacks become unnecessary), but no ADR-naming step is required for T1 gate execution. ADR-tba-4(e) GRD byte-count instrumentation remains an independent refinement path for H-EC-C2 leak precision.

**H-F1.1a — Zone Art Budget Ceiling: Local Per-Zone Budget (F1.1, local form)**
**GIVEN** the currently active zone, **WHEN** a PlayMode Memory Profiler snapshot is taken immediately after `ZoneActive` is confirmed (zone fully loaded, unload not in progress), **THEN** resident `Texture2D` bytes attributed to that zone ≤ `R_zone_max` (where `R_zone_max = R_cap − R_always_at_current_tier − R_zone_peer_max` per F1.1; `R_always_at_current_tier` per F1.1 T4 scalability note). Attribution per the shared bundle-label caveat above: **primary** method is bundle-label filter; **fallback is Form L (load-side delta)** if per-label isolation is unavailable at Unity 6.3. Note: **bundle size on disk ≠ resident texture memory** — the gate is resident memory, not bundle size.
*Dev-build smoke (PlayMode Memory Profiler, primary per-zone bundle-label filter OR Fallback Form L per shared caveat) | engine-programmer + art-lead | T1-blocking*

**H-F1.1b — Zone Art Budget Ceiling: Global Pairwise Invariant (F1.1, global form)**
**GIVEN** the full set of authored zones at the current tier, **WHEN** PlayMode Memory Profiler snapshots are taken for each zone while that zone is active, **THEN** the sum of any two zones' resident texture footprints ≤ `R_cap − R_always_at_current_tier` (305 MB at MVP; ~297 MB at T4 per F1.1 T4 scalability note) — i.e. the worst-case outgoing+incoming pair at any transition fits under the peak cap. Authoring discipline: re-evaluate this pairwise constraint each time a new zone is added to the manifest. Attribution per the shared bundle-label caveat above: **primary** method is bundle-label filter; **fallback is Form L (load-side delta)** applied per zone, then summed across the pair, if per-label isolation is unavailable at Unity 6.3.
*Dev-build smoke (PlayMode Memory Profiler, primary per-zone bundle-label filter OR Fallback Form L summed pairwise per shared caveat) | engine-programmer + art-lead | T1-blocking*

*(Split from round-2 H-F1.1 on 2026-04-23 round-3 per Blocker C2.5 — the local budget and global invariant are independent assertions, each independently Dev-build-smoke-verifiable; bundling them obscured which gate failed when one violated. Both remain reclassified Editor-validation → Dev-build smoke per Blocker 1.)*

**H-F2 — Stream-Ahead Trigger Distance (F2)**
**GIVEN** `V_max` and profiled `T_load_s` on [Min-Spec Profile](#min-spec-profile-profiling-target-for-measured-variables), **WHEN** an Editor level-validator script scans all stream-ahead trigger placements, **THEN** every trigger satisfies `D_trigger ≥ V_max × T_load_s + D_margin`; values <5 m produce a design-smell warning and values >40 m produce an advisory (split recommended).
*Editor-validation (level-validator Editor menu script) | engine-programmer | T1-blocking*

**H-F3 — Transition Time Budget (F3)**
**GIVEN** a `CityHub → HauntMansion` transition on [Min-Spec Profile](#min-spec-profile-profiling-target-for-measured-variables), **WHEN** timed from stream-ahead trigger entry through commit to player control restored post-commit, **THEN** stream-ahead latency `T_save + T_load_ms < zone_overrun_window_seconds × 1000` (default 5,000 ms) AND felt commit latency `T_activate ≤ 100 ms`; each sub-component is logged separately so ownership is unambiguous. Player retains full locomotion during stream-ahead; only commit imposes the position-lock.
*Profiled playtest (minimum-spec manual run with Unity Profiler) | engine-programmer + qa-tester | T1-blocking*

*H-F4 (compound 4-condition criterion) was split into H-F4a / H-F4b / H-F4c on 2026-04-23 round-2 per Blocker 3, then further split on round-3 per Blocker C3: H-F4b became H-F4b-schema (Unit, save-file fixture) + H-F4b-cadence (Integration PlayMode, programmatic service-field access). Round-2's "debug-watch" wording for condition (c) has been replaced with programmatic `CorpseRecord` service-field access inside the declared Integration taxonomy (debug-watch was outside the declared taxonomy). Each sub-criterion now names its own verification instrument.*

**H-F4a — Corpse Retention Contract: Texture Release (was H-F4 condition (a))**
**GIVEN** the player dies in HauntMansion and the active zone transitions to CityHub, **WHEN** a PlayMode Memory Profiler snapshot is taken after CityHub reaches `ZoneActive` and HauntMansion has completed `ZoneUnloading → ZoneIdle`, **THEN** HauntMansion's Addressable texture group is fully released — resident `Texture2D` bytes attributed to HauntMansion = 0. No texture/scene-group reference is retained on the `CorpseRecord`. Attribution per the shared bundle-label caveat noted with H-F1.1a / H-F1.1b: **primary** method is bundle-label filter; **fallback is Form U (unload-side delta)** — released-delta (baseline − post-unload) equals HauntMansion's live residency from the baseline snapshot, i.e. no bytes retained after unload.
*Dev-build smoke (PlayMode Memory Profiler post-unload snapshot via primary per-label filter OR Fallback Form U per shared caveat; overlaps H-EC-C2 methodology) | engine-programmer | T1-blocking*

*H-F4b (compound schema + cadence criterion) was further split into H-F4b-schema and H-F4b-cadence on 2026-04-23 round-3 per Blocker C3. The schema assertion and the runtime countdown-cadence assertion are independent and verified by different test types. Debug-watch (used in round-2 text) is outside the declared T1 Test-Type Taxonomy and has been replaced with a PlayMode Integration test that reads the `CorpseRecord` service field programmatically via the component's public accessor — inside the declared Integration category.*

**H-F4b-schema — Corpse Retention Contract: CorpseRecord Schema (was H-F4b schema portion)**
**GIVEN** the player dies in HauntMansion and the save file is written during `SaveCheckpointing`, **WHEN** the serialized save fixture is inspected (test harness deserialises the save file and asserts on the decoded `CorpseRecord` structure), **THEN** the `CorpseRecord` contains `zoneId` + world-space `Vector3` position + `expiry_timestamp_utc` — all three fields present and non-default; **no `Texture2D` reference, scene handle, or Addressable bundle label is serialized on the record** (records-only schema — no engine-resource references).
*Unit (save-file fixture deserialise + schema assertion — no PlayMode required) | gameplay-programmer | T1-blocking*

**H-F4b-cadence — Corpse Retention Contract: Independent Countdown Cadence (was H-F4b cadence portion)**
**GIVEN** the player dies in HauntMansion, the save file is written during `SaveCheckpointing`, and the player subsequently transitions between CityHub and HauntMansion (both directions) within the retention window, **WHEN** a PlayMode Integration test reads `CorpseRecord.expiry_timestamp_utc` programmatically via the `CorpseRecord` service's public accessor at three sampling points — (i) before the dead zone transitions to `ZoneIdle`, (ii) while the dead zone is `ZoneIdle`, (iii) after re-entry to the dead zone — and compares each sample against wall-clock elapsed, **THEN** the measured countdown rate is uniform on wall-clock cadence with no jumps or stalls correlated to zone state transitions (i.e. the countdown is demonstrably independent of zone residency). This is the named instrument — *Integration PlayMode with programmatic service-field access* — for the "countdown independent of zone residency" condition from original H-F4(c). Debug-watch is not used; the service field is read by test code.
*Integration (PlayMode, three-point sample of `CorpseRecord` public accessor across zone transitions) | gameplay-programmer + qa-tester | T1-blocking*

**H-F4c — Corpse Retention Contract: Re-Entry Path Integrity (was H-F4 condition (d))**
**GIVEN** the player is in CityHub with an active `CorpseRecord` pointing into HauntMansion, **WHEN** the player initiates a zone transition back to HauntMansion within the `corpse_run_zone_retention_seconds` window, **THEN** the state-machine event log shows the standard F2/F3 reload path (`ZoneLoading → ZoneActive` for HauntMansion with no "corpse-zone held" special-case branch; `T_activate ≤ 100 ms` at commit as normal); after HauntMansion reaches `ZoneActive`, `CorpseRecord.position` is matched to world-space geometry and corpse recovery resumes via Death & Corpse Recovery's probe (per Edge B2 ownership boundary). Event log assertion: only the seven baseline transition events fire (stream-ahead trigger, `SaveCheckpointing`, `ZoneLoading`, `SaveWriteConfirmed`, commit, `ZoneActiveEvent`, `ZoneUnloading`→`ZoneIdle` for CityHub); no corpse-specific branch.
*Integration PlayMode (event-log assertion on zone state transitions + recovery-resume assertion) | engine-programmer + qa-tester | T1-blocking*

### High-risk edge case coverage

*H-EC-A1 (compound 6-condition criterion) was split into H-EC-A1-state and H-EC-A1-addressables on 2026-04-23 round-3 per Blocker C4. Conditions (d)(e)(f) are state-machine / save-state assertions that are stable contracts independent of ADR-tba-4 resolution. Conditions (a)(b)(c) are prototype-dependent Addressables-API behaviors per ADR-tba-4(b)(c) and are ADR-gated — they do not gate T1 until the prototype resolves.*

**H-EC-A1-state — Death in Transition Volume: State-Machine + Save-State Contract (was H-EC-A1 conditions (d)(e)(f))**
**GIVEN** the player enters a `ZoneLoading` stream-ahead trigger volume, **WHEN** `PlayerDeathEvent` fires before commit, **THEN** (d) the outgoing zone remains `ZoneActive` (no zone flip on death during stream-ahead); (e) `CorpseRunActive` substates into the outgoing zone (not the incoming one); (f) the written `CorpseRecord` names the outgoing zone's `zoneId` + world-space death coords + `expiry_timestamp_utc`. This criterion is independent of the Addressables-API resolution — it asserts only state-machine routing and save-state schema.
*Integration (PlayMode event-log + save-state fixture) | gameplay-programmer + qa-tester | T1-blocking*

**H-EC-A1-addressables — Death in Transition Volume: Load Abort Semantics (was H-EC-A1 conditions (a)(b)(c)) — ADR-gated**
**GIVEN** the player enters a `ZoneLoading` stream-ahead trigger volume, **WHEN** `PlayerDeathEvent` fires before commit, **THEN** the in-flight async scene load is aborted via the Edge A1 flag-and-async-unload pattern: (a) the load is allowed to reach its activation-hold state (synchronous cancellation semantics at Unity 6.3 are prototype-dependent per ADR-tba-4(b); current design assumes no synchronous cancellation, as was true at Unity 6.0); (b) the activation-release call is suppressed in the completion callback; (c) the held scene handle is released asynchronously via the API validated by ADR-tba-4(c). The test asserts that *some* async release call occurs against the handle and the handle is no longer resident after `zone_overrun_window_seconds`; **the specific release API MUST NOT be hardcoded in the test expectation** until ADR-tba-4(c) resolves.

**ADR-gating (2026-04-23 round-3 per Blocker C4):** this AC **does not gate T1** until ADR-tba-4(b)(c) resolves and the Edge A1 abort pattern's API realisation is verified at Unity 6.3. Once resolved, H-EC-A1-addressables becomes T1-blocking with the ADR-named release API (and cancellation semantics) as its verification instrument. The state-machine contract (H-EC-A1-state) is T1-blocking independently of this gate.
*Integration (PlayMode, API-agnostic assertion on handle residency + async release call occurrence) | gameplay-programmer + qa-tester | **ADR-gated** — unlocks T1-blocking status when ADR-tba-4(b)(c) resolves*

**H-EC-B1 — Missing Zone ID on Load-Resume (Edge B1)**
**GIVEN** a save file referencing a zone ID absent from the current Addressable catalog, **WHEN** the session loads, **THEN** `ZoneError` is entered, a non-dismissible UI message surfaces the missing-ID string, the session audit logs the missing ID, and no silent zone relocation occurs.
*Integration (authored invalid test save) | gameplay-programmer + qa-tester | T1-blocking*

**H-EC-C2 — R_peak Runtime Violation via Leak (Edge C2)**
**GIVEN** a zone transition completes in a PlayMode session, **WHEN** Unity Memory Profiler snapshots `Texture2D` bytes attributed to the outgoing zone after `ZoneUnloading → ZoneIdle` completion, **THEN** outgoing zone's resident texture bytes = 0 (fully released); non-zero bytes are an engine-programmer defect, not a design edge. Attribution per the shared bundle-label caveat noted with H-F1.1a / H-F1.1b / H-F4a: **primary** method is bundle-label filter; **fallback is Form U (unload-side delta)** — released-delta equals the outgoing zone's live residency from the baseline snapshot, i.e. no bytes retained after unload.
*Dev-build smoke (post-unload PlayMode Memory Profiler snapshot via primary per-label filter OR Fallback Form U per shared caveat) | engine-programmer | T1-blocking*

**H-EC-C3 — Bundle Corruption (Edge C3)**
**GIVEN** Addressables throws a load-failure exception on a deliberately corrupted test bundle, **WHEN** `ZoneLoading` handles the exception, **THEN** `ZoneError` is entered, no retry is attempted, bundle ID + checksum state are logged, and `ZoneErrorEvent(reason: BundleCorrupt)` is delivered to Menus & Settings.
*Integration | engine-programmer + qa-tester | T1-blocking*

**H-EC-D3 — Player Outside All Zone Triggers (Edge D3)**
**GIVEN** a pre-ship Editor-validation scan of all navmesh surface points, **WHEN** any point falls outside every zone trigger volume's coverage, **THEN** the level-validator reports out-of-bounds coordinates to the console and to `production/qa/evidence/world-structure/`; in a shipping session, such a position fires `ZonePositionInvalidEvent` and enters `ZoneError`.
*Editor-validation (navmesh coverage scan as Editor menu script + pre-build callback) | engine-programmer | T1-blocking*

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
| H-CR-01 | Rules 1 + 11 (Tier Scalability) | Integration | gameplay-programmer | **T4-deferred** (demoted 2026-04-23) |
| H-CR-02 | Rule 2 | Integration | gameplay-programmer, qa-tester | Yes |
| H-CR-03 | Rules 3 + 6 | Integration | engine-programmer | Yes |
| H-CR-04 | Rule 4 | Unit | gameplay-programmer | Yes |
| H-CR-05 | Rule 5 (two-phase) | Profiled playtest | engine-programmer, qa-tester | Yes |
| H-CR-06 | Rule 7 | Editor-validation | engine-programmer | Yes |
| H-CR-07 | Rule 8 | Integration | qa-tester | Yes |
| H-CR-08 | Rule 9 (WS-owned portion) | Integration | engine-programmer | Yes |
| H-CR-09a | Rule 10 (static) | Editor-validation | engine-programmer | Yes |
| H-CR-09b | Rule 10 (runtime) | Integration | engine-programmer | Yes |
| H-CR-09c | Rule 10 (visual) | Dev-build smoke | engine-programmer, art-lead | advisory |
| H-CR-10 | Rule 12 | Unit | gameplay-programmer | Yes |
| H-CR-13a | Rule 13 (timestamp persisted) | Unit | gameplay-programmer | Yes |
| H-CR-13b | Rule 13 (firing order) | Integration | gameplay-programmer, qa-tester | Yes |
| H-CR-13c | Rule 13 (elapsed clamp) | Unit | gameplay-programmer | Yes |
| H-F1-steady | F1 steady-state form | Dev-build smoke | engine-programmer | Yes |
| H-F1-peak | F1 peak form | Dev-build smoke | engine-programmer | **ADR-gated** (unlocks T1 on ADR-tba-4(i)) |
| H-F1.1a | F1.1 local per-zone budget | Dev-build smoke | engine-programmer, art-lead | Yes |
| H-F1.1b | F1.1 global pairwise invariant | Dev-build smoke | engine-programmer, art-lead | Yes |
| H-F2 | F2 | Editor-validation | engine-programmer | Yes |
| H-F3 | F3 | Profiled playtest | engine-programmer, qa-tester | Yes |
| H-F4a | Corpse Retention Model — texture release | Dev-build smoke | engine-programmer | Yes |
| H-F4b-schema | Corpse Retention Model — CorpseRecord schema | Unit | gameplay-programmer | Yes |
| H-F4b-cadence | Corpse Retention Model — independent countdown cadence | Integration | gameplay-programmer, qa-tester | Yes |
| H-F4c | Corpse Retention Model — re-entry path integrity | Integration | engine-programmer, qa-tester | Yes |
| H-EC-A1-state | Edge A1 state-machine + save-state contract | Integration | gameplay-programmer, qa-tester | Yes |
| H-EC-A1-addressables | Edge A1 Addressables abort semantics | Integration | gameplay-programmer, qa-tester | **ADR-gated** (unlocks T1 on ADR-tba-4(b)(c)) |
| H-EC-B1 | Edge B1 | Integration | gameplay-programmer, qa-tester | Yes |
| H-EC-C2 | Edge C2 | Dev-build smoke | engine-programmer | Yes |
| H-EC-C3 | Edge C3 | Integration | engine-programmer, qa-tester | Yes |
| H-EC-D3 | Edge D3 | Editor-validation | engine-programmer | Yes |
| H-CS-01 | Save/Load | Unit | gameplay-programmer | Yes |
| H-CS-02 | NPC System | Integration | gameplay-programmer, qa-tester | Yes |
| H-CS-03 | Combat Core | Unit | gameplay-programmer | Yes |
| H-CS-04 | Audio System | Integration | engine-programmer, qa-tester | advisory |
| H-CS-05 | Zone Control | Integration | gameplay-programmer | Yes |
| H-CS-06 | Day/Night Cycle | Integration | gameplay-programmer, qa-tester | Yes |

**Total: 37 criteria. 32 T1-blocking, 2 ADR-gated (H-F1-peak, H-EC-A1-addressables), 2 advisory (H-CR-09c + H-CS-04), 1 T4-deferred (H-CR-01).** *(Delta from round-2: H-F1 split → H-F1-steady + H-F1-peak (ADR-gated); H-F1.1 split → H-F1.1a + H-F1.1b; H-F4b split → H-F4b-schema + H-F4b-cadence; H-EC-A1 split → H-EC-A1-state + H-EC-A1-addressables (ADR-gated). Net: +4 criteria, +2 T1-blocking, +2 ADR-gated. Cumulative delta from round-1: H-F4 split + round-3 splits = +6 criteria.)*

> **ADR-gated category (new 2026-04-23 round-3):** Criteria marked ADR-gated are T1-blocking *after* the named ADR resolves and defines the required verification instrument. Until then they are neither T1-blocking nor deferred — they are specification-complete but awaiting prototype-defined observable seam or API realisation. **Two ACs are ADR-gated:** H-F1-peak unlocks on ADR-tba-4(i) resolution (peak observable seam); H-EC-A1-addressables unlocks on ADR-tba-4(b)(c) resolution (cancellation + release API). The H-F1.1a / H-F1.1b / H-F4a / H-EC-C2 ACs are **T1-blocking on day one** — they carry a pre-specified fallback (Form L for load-side active-zone budgets, Form U for unload-side leak gates; see the shared C2 caveat) that is runnable regardless of ADR-tba-4(d) outcome. No ADR-naming step is required for those four ACs to execute at T1 (2026-04-23 round-4 Blocker 3 resolution).

### Explicit non-criteria (out of this GDD's scope)

- **Edge E2 (Combat damage during `ZoneTransitionBegin`)** — ownership belongs to Combat Core GDD. World Structure's contract complete at firing the event; Combat Core's Section H will cover in-flight resolution specifics.
- **Faction Sim at T3** — `FactionControlChanged` persistence smoke check becomes T3-blocking when autonomous sim lands. Flag for Faction State Simulation GDD Section H.
- **Hub NPC tick semantics (Edge D1) — during-session portion** — resolved by NPC System GDD. Acceptance criterion for the chosen NPC mode belongs to NPC System H-NPC-WS-03. Between-session publication remains covered here (H-CR-13a/b/c via Rule 13).
- **Downstream `SessionResumeEvent` handlers — implementation, multi-subscriber firing order, AND organic-discovery design** — Rule 13's binding contract requires NPC System, Faction Sim, Day/Night, Faction Events, and Dialogue System each, when its GDD is authored, to include three T1-blocking acceptance criteria of its own: (1) a handler-implementation AC (the handler exists and produces the designed catch-up effect deterministically); (2) a firing-order AC (this specific subscriber observes `SessionResumeEvent` before any `ZoneActiveEvent`) — this is the per-subscriber migration target for H-CR-13b, which at T1 has been narrowed to a single synthetic subscriber per Blocker 2; (3) an **organic-discovery AC** specifying how the player notices the handler's effect *without a banner* (per the Rule 13 D3 organic-discovery amendment) — e.g., advanced NPC schedules leaving visible traces, Faction Board reflecting newly-resolved events, rival-writ content appearing, expired corpse records becoming unreachable. World Structure owns only the event publication criteria (H-CR-13a/b/c); all three downstream-obligation criteria above are out of this GDD's scope.
- **QA test taxonomy promotion** — the WS-local 5-category taxonomy (Editor-validation / Dev-build smoke / Profiled playtest / Unit / Integration) is a candidate for project-wide promotion to `.claude/rules/test-standards.md` after Save/Load GDD adopts it consistently. Not in this GDD's scope; flagged as follow-up per D5 (2026-04-23 revision).
- **DECISIONS.md D007 for Rule 13** — Rule 13's offline-bridge contract will be captured as a new DECISIONS.md entry after this GDD passes re-review (per D6, 2026-04-23 revision). Not in this revision batch — DECISIONS.md is append-only and will not be amended while the GDD is still unstable.

## Open Questions

| Question | Owner | Deadline | Status |
|---|---|---|---|
| **ADR-tba-1: Zone Scene Topology** — persistent-hub + additive Addressable scenes (provisional recommendation from `engine-programmer`) | `engine-programmer` + `unity-specialist` | Before T1 zone implementation | Open |
| **ADR-tba-2: Corpse-Run Camera Stack Configuration** — URP Overlay-camera post-process isolation for per-camera -40% desaturation | `engine-programmer` + `unity-shader-specialist` | Before Combat Core implementation (corpse-run visible) | Open — **prototype required** (art bible TA-flagged) |
| **ADR-tba-3: World State Serialization Contract** — `WorldStateRecord` POCO DTO shape, HMAC integrity, version-migration strategy | `gameplay-programmer` + `security-engineer` | Before T1 save implementation | Open |
| **ADR-tba-4: Zone Transition Mechanism** — stream-ahead via `allowSceneActivation = false` (Rule 5 phase 1) + door-prefab commit via `allowSceneActivation = true` (phase 2); GPU Resident Drawer registration-frame mitigation. Prototype scope **expanded 2026-04-23 round-2 per Blockers 8, 9, 10** — see §Scope boundary item 4 for full scope: (a) `allowSceneActivation` under `AsyncOperationHandle<SceneInstance>`; (b) `AsyncOperationHandle` cancellation API at 6.3; (c) `UnloadSceneAsync` vs `Release` for unactivated handles; (d) cross-group serialized-dependency release (three-group model now prototype-dependent); (e) GRD byte-count instrumentation API; (f) Pack Mode authoring constraint; (g) `ZoneManifest` field types; (h) three-group independent release on `ZoneIdle` entry; (i) transitional/commit-phase staggered residency. | `engine-programmer` | Before first zone implementation | Open — **prototype required** with 9-item expanded scope. All nine items are prototype-required, not desk-analysable. |
| **ADR-tba-5: Hub NPC Schedule Tick Semantics** — real-time tick vs. delta-catch-up vs. discrete-event during `ZoneIdle` in T1 offline (during-session only; Rule 13 closes between-session ambiguity) | `game-designer` + `ai-programmer` + `systems-designer` | Before NPC implementation | Resolved 2026-04-24 via NPC System GDD Overview + Core Rule 5 + H-NPC-WS-03: data-only delta catch-up plus active-zone ticks; no live NPC ticks during `ZoneIdle`. |
| **ADR-tba-6: Zone ID Lifecycle Management** — stable-identifier contract; migration-script requirement when zones are deprecated | `release-manager` + `gameplay-programmer` | Before any zone deletion in a shipped build | Open |
| `corpse_run_zone_retention_seconds` — initial 300 s pin, validate against T1 playtest observed corpse-run durations | `qa-tester` + `game-designer` | T1 playtest | Pinned with revisit flag |
| Per-zone art-budget validation at T3+ (zones may diverge from MVP 165 MB profile) | `art-director` + `engine-programmer` | T3 zone design | Open — flagged for T3 |
| Faction Sim T3 upgrade (reactive → autonomous): `FactionControlChanged` persistence smoke check gating T3 release | `gameplay-programmer` (Faction Sim) + `qa-lead` | T3 entry gate | Open — flagged for T3 |
| **Capture Rule 13 contract as new DECISIONS.md D007 entry** | `creative-director` + `technical-director` + Brian | After revised WS GDD passes re-review | Open — follow-up per D6 (2026-04-23 revision) |
| **Promote WS-local QA test taxonomy to `.claude/rules/test-standards.md`** | `qa-lead` | After Save/Load GDD adopts the taxonomy | Open — follow-up per D5 (2026-04-23 revision) |

**T1 blockers (ADR-tba-1 through ADR-tba-4)** must be resolved before their respective implementation work begins. ADR-tba-5 is resolved at GDD level by NPC System; ADR-tba-6 is not T1-blocking (zone deletion won't happen at MVP). ADR-tba-2 and ADR-tba-4 require prototyping, not just desk analysis.
