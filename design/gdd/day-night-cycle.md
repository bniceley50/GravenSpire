# Day/Night Cycle

> **Status**: In Design
> **Author**: Codex session with Brian, 2026-04-24
> **Last Updated**: 2026-04-24
> **Implements Pillar**: Primary - **P1 The World Is Not Your Story**. Supports - **P2 The Silence Is Sacred** and **P4 Every Companion Is A Person**.

## Summary

Day/Night Cycle is the T1 offline single-player world-clock system for Gravenspire. It derives a deterministic local time-of-day from current UTC plus a fixed project epoch, exposes quiet phase data to active systems, applies zone-specific phase profiles when World Structure publishes `ZoneActiveEvent`, and re-derives its clock across session boundaries when World Structure publishes `SessionResumeEvent`. It never keeps unloaded zones alive, never owns save serialization, and never presents a clock UI, alert, recap, map marker, or non-diegetic guidance surface. Its job is to make the city and haunt keep their own hours without making the player feel serviced by a theme-park lighting system.

> **Quick reference** - Layer: Core. Priority: MVP. Direct dependency: World Structure only. T1 scope: offline single-player, one city hub skeleton, one haunt zone, local deterministic world clock.

## Overview

Day/Night Cycle owns Gravenspire's local world-clock behavior: how time-of-day is derived during play, how it re-derives after a session resume, and how active zones receive a phase profile such as City Hub "Inn Hours", City Hub "Court Hours", or Haunt Interior. It is a peer to World Structure, not a child of it. World Structure owns zone lifecycle and publishes `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` before any `ZoneActiveEvent`; Day/Night subscribes to that event to confirm ordering, derive the current clock from UTC, and then apply the correct zone phase when the active zone is announced.

The player experiences Day/Night indirectly. There is no clock widget, no "night has fallen" banner, no recap of time passed, and no quest-marker-like routing. The system is legible through light-source logic, NPC schedule consequences, inn/court availability, and the changing mood of already-authored spaces. In T1 it supports a small cycle with two CityHub social phases and a haunt profile that remains readable without adding special VFX, UI overlays, or live simulation in idle zones.

## Player Fantasy

The player should feel that Gravenspire keeps hours older than them. Day is not "safe mode" and night is not "danger mode"; each phase is a social arrangement the player learns by attention. During inn hours, the hub has working warmth and public transactions. During court hours, the same rooms seal themselves around candlelight, faction posture, and older social rules. The haunt does not become a different level at night; it receives time as a pressure on readability and presence, not as a theme-park transformation.

The fantasy is quiet continuity: the player leaves the city in one phase, returns after time has passed, and sees the world already partway through its own routines. A lamplighter is not spawned for the player. A Court chamberlain is not waiting because the player arrived. They are there because the clock and schedule data say this is the hour when they would be there.

### Anchor moments

- **Returning to the city gate near dawn after a night in the haunt**: the sky is overcast and the lamps are in a transitional state; the lamplighter route is visible if NPC System has scheduled it. No banner announces dawn.
- **Entering the inn during public hours**: working warmth competes with overcast daylight; the room reads as social but not celebratory.
- **Entering the same hall during Court hours**: exterior windows read as black mirrors; practical sources dominate; NPC posture and faction material become the signal.
- **Resuming after real-world time away**: time-of-day has advanced before the first active zone is enabled. Any visible effects are organic: changed light states, NPC positions, availability, and downstream Layer 2 surfaces, not UI recaps.

### Anti-fantasy

- The world should not feel like it flips between "day theme" and "night theme".
- The player should not receive non-diegetic guidance, warnings, alerts, or recaps from the clock.
- Idle zones should not feel alive because hidden GameObjects are still running; they should feel continuous because deterministic data evaluates when needed.
- Night should not become an excuse for spectacle, glow, particle density, or unreadable darkness.

## Detailed Design

### Core Rules

1. **Single local world clock.** Day/Night owns one deterministic T1 world clock expressed as `world_time_seconds` in the range `[0, day_length_seconds)`. It is local to the offline single-player session. It is not server-synchronized, network-authoritative, or multiplayer-aware.

2. **Direct dependency is World Structure only; clock state is derived, not persisted.** Day/Night consumes World Structure's `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` and `ZoneActiveEvent(zoneId, zoneType)`. It derives `world_time_seconds` from `now_utc_seconds`, fixed `PROJECT_WORLD_EPOCH_UTC_seconds`, `time_scale_world`, `world_clock_offset_seconds`, and `day_length_seconds`. It does not directly depend on Save / Load & Persistence in this GDD, and no T1 save schema may persist `world_time_seconds`, `world_time_seconds_at_last_exit`, or an equivalent Day/Night clock field.

3. **Session resume ordering.** Day/Night must process `SessionResumeEvent` before it applies the first active-zone phase after load. This mirrors World Structure Rule 13: `SessionResumeEvent` is published before any `ZoneActiveEvent`. If elapsed time is 0, Day/Night still records that its handler ran and re-derives the clock from UTC; the event's elapsed payload is not used as clock math.

4. **During-session derivation.** While the game is in active gameplay, `world_time_seconds` is re-derived from the local effective UTC sample using `time_scale_world`. T1 default is conservative: one full in-game day lasts `day_length_seconds = 7200` real seconds at `time_scale_world = 1.0`, or 2 real hours. This gives a normal 60-180 minute session enough time to feel phase movement without turning the city into a fast-cycle toy.

5. **Pause handling is explicit but owned elsewhere.** Day/Night honors a simulation-paused flag if the T1 pause semantics ADR or Menus & Settings implementation declares that local simulation time stops while the pause menu is open. Until that ADR resolves, Day/Night's GDD contract is: active gameplay samples effective UTC and re-derives the clock; non-playable loading/error/title states do not publish gameplay phase changes. If pause semantics later freeze local simulation, the implementation may hold the effective UTC sample while paused without introducing persisted Day/Night clock state. Menu-pause semantics remain an open integration question.

6. **No live idle-zone objects.** Day/Night may retain only data required to compute phase and zone profile selection: zone id, zone type, configured phase profile id, phase thresholds, scalar offsets, and last applied phase id. It must not retain or require idle-zone GameObjects, MonoBehaviours, Renderers, Lights, Materials, Textures, Animators, AudioSources, physics bodies, live scene handles, Addressable scene handles, or runtime light references.

7. **ZoneActive profile application.** On `ZoneActiveEvent(zoneId, zoneType)`, Day/Night resolves the zone's authored `DayNightZoneProfile` and applies phase data to systems in the active zone only. Application means publishing data or setting active-zone scene controls that already exist in the loaded zone. It does not load assets, instantiate hidden objects in idle zones, or force World Structure state changes.

8. **ZoneIdle data evaluation.** Day/Night continues to derive its world clock regardless of which zone is active, but inactive zones receive no live per-frame lighting, animation, audio, material, or physics updates. When an inactive zone later becomes active, Day/Night computes the correct current phase from data and applies it once after `ZoneActiveEvent`.

9. **Phase vocabulary is small at T1.** T1 uses four authored phase ids:
   - `DawnTransition`
   - `InnHours`
   - `DuskTransition`
   - `CourtHours`

   Haunt zones can map these same phase ids to a different profile, but they may not introduce a separate T1 night-combat ruleset.

10. **City Hub phase behavior.** In `CityHubZone`, `InnHours` and `CourtHours` are the primary readable phases. Dawn and dusk are short transition bands used for gradual light-source state changes and NPC schedule predicates. They are not dramatic events.

11. **Haunt phase behavior.** In `HauntZone`, Day/Night affects ambient readability, exterior light leakage through authored openings, and schedule predicates. It does not change enemy stats, spawn tables, loot, faction ownership, combat tuning, or player resources. Those belong to downstream systems.

12. **Lighting changes are material and light-source logical.** Day/Night may drive active-zone light rigs and sky/ambient parameters only within the art bible's light-source logic. It must not use LUT swaps, global post-process grade jumps, red/blue warning overlays, magical moonlight filters, or particle/VFX layers as primary phase signals.

13. **No clock UI.** Day/Night publishes no player-facing HUD element, toast, alert, recap, map marker, objective hint, or "current phase" panel. Any Layer 2 world information about time must be diegetic and owned by the appropriate downstream surface, such as NPC schedules, faction notices, dialogue, or world props.

14. **Organic discovery obligation.** Day/Night's `SessionResumeEvent` handler must produce player-noticeable effects only through diegetic active-zone state: light-source states, sky/ambient state, window darkness, candle/lantern states, and downstream system predicates. It must never produce a resume recap.

15. **Deterministic UTC derivation on resume.** Day/Night resume handling re-computes `world_time_seconds` from current UTC using the canonical derivation formula. The `SessionResumeEvent` handler's job is to confirm pre-`ZoneActiveEvent` ordering, trigger the derivation call, record derivation completion for tests, and publish current phase data for subscribers. It does not add clamped elapsed time, simulate every tick between sessions, run NPC movement, or evaluate faction events.

16. **Time authority is local in T1.** There is no FishNet time, no account time, no persistent server time, no multiplayer authority, and no live LLM behavior in this system. T2+ and T3+ time authority are future Network Architecture and server-persistence questions.

17. **New game and resume share the same clock source.** A new T1 character does not receive a separate `new_game_initial_time_seconds` value. First impression is determined by the character-creation UTC sample, `PROJECT_WORLD_EPOCH_UTC_seconds`, and `world_clock_offset_seconds`. The offset is an authored tuning constant, not save state; changing it deliberately re-phases the whole world without changing the dependency graph.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `ClockUninitialized` | New process start before a new game or loaded session provides a valid UTC sample | Clock derived for new game or valid resume metadata delivered | Holds no gameplay phase. No zone application allowed. |
| `ClockReady` | Initial `world_time_seconds` has been derived | Active gameplay begins -> `ClockAdvancing`; session resume event received -> `ResumeDerivation`; non-playable state -> `ClockSuspended` | Data-only clock state. May answer phase queries. |
| `ResumeDerivation` | `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` received | Derivation completes -> `ClockReady` or `ClockAdvancing` depending on gameplay state | Re-derives current clock from UTC using F1. Publishes no UI. Must complete before first active-zone phase application. |
| `ClockAdvancing` | Playable session is active and local simulation is not paused | Pause/title/error/loading -> `ClockSuspended`; session end -> `ClockReady` | Re-derives the local clock from the effective UTC sample. Active-zone phase may change on threshold crossing. |
| `ClockSuspended` | Title/loading/error state, or a pause mode that explicitly stops local simulation time | Playable unpaused state -> `ClockAdvancing`; load metadata delivered -> `ClockReady` or `ResumeDerivation` | Clock phase changes are not published to gameplay. No idle-zone live updates. |
| `ActiveZonePhaseApplied` | `ZoneActiveEvent(zoneId, zoneType)` received after clock is ready | Zone unload begins or phase threshold changes | Active-zone profile has been applied from data. This is not a separate World Structure state. |

### Interactions with Other Systems

| System | Day/Night consumes | Day/Night provides | Dependency |
|---|---|---|---|
| **World Structure** | `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)`, `ZoneActiveEvent(zoneId, zoneType)`, active/idle zone lifecycle boundaries | No World Structure state writes. Optional diagnostics that Day/Night applied the active-zone profile. | **Hard direct dependency** |
| **NPC System** | None required for Day/Night core. NPC schedules later query phase/current time. | Current phase id, normalized time, schedule predicates such as `IsCourtHours`. | **Downstream consumer, not direct dependency** |
| **Menus & Settings** | Optional simulation-pause flag if pause semantics ADR resolves that pause stops local world time. | No UI. Menus must ignore Day/Night for recaps or clock displays at T1. | **Soft integration question** |
| **Audio System** | None at T1. | Current phase id can later select ambient beds or silence profiles. | **Future downstream consumer** |
| **Faction Events** | None at T1. | Current phase id may later gate event visibility or notice timing. | **Future downstream consumer** |
| **Dialogue System** | None at T1. | Current phase id may later gate dialogue templates like Court-hour availability. | **Future downstream consumer** |
| **Save / Load & Persistence** | No direct input in this GDD. World Structure provides resume ordering via `SessionResumeEvent`. | No persisted `world_time_seconds` field, no Day/Night hydration callback, no save-file schema ownership. | **Not a direct T1 dependency** |

## Formulas

The `world_time_seconds_derivation` formula is the canonical clock source:

`world_time_seconds = ((now_utc_seconds - PROJECT_WORLD_EPOCH_UTC_seconds) * time_scale_world + world_clock_offset_seconds) mod day_length_seconds`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| `now_utc_seconds` | `N` | int | UTC epoch seconds sampled from the local platform clock | Current effective UTC sample used by Day/Night. |
| `PROJECT_WORLD_EPOCH_UTC_seconds` | `E` | int | 1767225600 default | Fixed project epoch: 2026-01-01T00:00:00Z. |
| `time_scale_world` | `S` | float | 0.0 to 8.0; default 1.0 | Multiplier converting real UTC seconds to world-clock seconds. |
| `world_clock_offset_seconds` | `O` | int | -7200 to 7200; default 0 | Authored phase offset applied after scale; not persisted save state. |
| `day_length_seconds` | `L` | int | 1800 to 14400; default 7200 | Length of a full in-game day in real/world-clock seconds at scale 1.0. |

**Output Range:** 0 to `day_length_seconds` exclusive. The modulo wraps cleanly at day boundary.

**Example:** If `now_utc_seconds = 1767233100`, `PROJECT_WORLD_EPOCH_UTC_seconds = 1767225600`, `time_scale_world = 1.0`, `world_clock_offset_seconds = 0`, and `day_length_seconds = 7200`, then `((1767233100 - 1767225600) * 1.0 + 0) mod 7200 = 300`. The clock is in `DawnTransition`.

The `phase_from_world_time` formula is defined as:

`phase_id = first phase where phase_start_seconds <= world_time_seconds < phase_end_seconds`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| `world_time_seconds` | `W` | float | 0 to `day_length_seconds` exclusive | Current local in-game time within the day. |
| `phase_start_seconds` | `A` | int | 0 to `day_length_seconds` exclusive | Inclusive start bound for an authored phase. |
| `phase_end_seconds` | `B` | int | 1 to `day_length_seconds`, may wrap by authored split | Exclusive end bound for an authored phase. |
| `phase_id` | `P` | enum | `DawnTransition`, `InnHours`, `DuskTransition`, `CourtHours` | The current T1 Day/Night phase. |

**Output Range:** One valid phase id at all times. Authored profiles must cover the full day with no gaps and no overlaps.

**Example:** With default T1 thresholds, `DawnTransition = 0-600`, `InnHours = 600-3900`, `DuskTransition = 3900-4500`, and `CourtHours = 4500-7200`. A `world_time_seconds` value of `5000` resolves to `CourtHours`.

## Edge Cases

- **If `SessionResumeEvent` arrives with `real_elapsed_seconds = 0`**: Day/Night records that resume derivation ran, samples UTC, applies no elapsed-delta math, and still allows later `ZoneActiveEvent` profile application.
- **If system clock skew produced a negative raw delta**: World Structure clamps the event payload to 0 for its own Rule 13 contract. Day/Night does not use that payload for clock correctness; development telemetry may flag large backward UTC jumps, but no saved Day/Night clock state is repaired or rolled back.
- **If `ZoneActiveEvent` arrives before Day/Night processed resume derivation**: Day/Night rejects active-zone phase application, logs an ordering fault in development builds, and applies no partial phase. This is a World Structure ordering contract violation to be caught by Integration tests.
- **If no `DayNightZoneProfile` exists for the active zone**: Day/Night applies the neutral fallback profile for legibility, logs the missing `zoneId`, and marks the build invalid in Editor-validation. It does not block play in a development build.
- **If an authored phase profile has a gap or overlap**: Editor-validation fails the profile. At runtime in development builds, Day/Night uses the previous valid phase and logs the profile id.
- **If the player crosses a zone boundary exactly as a phase threshold is crossed**: Day/Night resolves the clock first, then applies the active-zone profile after `ZoneActiveEvent`. There is no duplicate phase event.
- **If the game is paused while a phase threshold would have passed**: behavior follows the pause semantics ADR. If local simulation is paused, the threshold waits; if only input is captured, the clock continues. This remains an open integration question until the pause ADR is resolved.
- **If the active zone is `ZoneIdle` or unloading**: Day/Night does not apply scene changes. It only keeps the data clock current.
- **If a light, renderer, material, animator, audio source, or scene handle from an idle zone is needed to express a phase**: the design is invalid. The phase must be represented as data and applied only when the zone becomes active.
- **If a designer wants night-only combat modifiers**: reject for T1. Combat tuning belongs to Combat Core or downstream haunt/event systems and would expand Day/Night beyond small MVP scope.
- **If the phase would make a required path unreadable**: the zone profile must raise practical-source legibility or material readability within art bible constraints. Do not add UI overlays, path arrows, glow strips, or warning VFX.

## Dependencies

### Direct dependency

| Dependency | Type | Contract |
|---|---|---|
| World Structure | Hard | Publishes `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` before any `ZoneActiveEvent`; publishes `ZoneActiveEvent(zoneId, zoneType)` when the active zone is ready; defines `ZoneIdle` as metadata/data-only with no live scene resources. |

### Downstream consumers and soft integrations

| System | Direction | Contract |
|---|---|---|
| NPC System | Consumes Day/Night | NPC schedules may use current phase and normalized time after Day/Night is authored. NPC System owns route, posture, availability, and data-only schedule evaluation. |
| Audio System | Consumes Day/Night later | May choose ambient profiles from phase data. Absence degrades to silence; Day/Night owns no audio playback. |
| Faction Events | Consumes Day/Night later | May use phase as an event visibility or scheduling predicate. Day/Night does not own event queues. |
| Dialogue System | Consumes Day/Night later | May gate templates such as Court-hour dialogue. Day/Night owns no dialogue content or LLM behavior. |
| Menus & Settings | Soft integration | May provide a simulation-pause flag after pause semantics are resolved. Must not expose clock UI or resume recaps. |
| Save / Load & Persistence | Not a direct T1 dependency | No direct T1 dependency in this GDD. Day/Night does not persist `world_time_seconds`, register a hydration callback, or own save-file schema. |

## Cross-References

| Referenced contract | Source | Day/Night usage |
|---|---|---|
| Current tier is T1 offline single-player; no networking or server authority | [DECISIONS.md](../../DECISIONS.md) D002-D003 | Rule 16 and Non-Goals keep Day/Night local-only and forbid FishNet/server-synchronized time. |
| LLM behavior deferred to T3 | [DECISIONS.md](../../DECISIONS.md) D004 | Non-Goals forbid live LLM behavior or generated dialogue in this system. |
| World Structure `SessionResumeEvent` ordering | [world-structure.md](world-structure.md) Rule 13 and H-CR-13b | Rules 2-3 and H-DN-WS-01 require Day/Night derivation before first active-zone profile application. |
| World Structure `ZoneIdle` metadata/data-only boundary | [world-structure.md](world-structure.md) `ZoneIdle` state | Rule 6 and H-DN-WS-03 enumerate the forbidden idle-zone runtime references. |
| World Structure peer contract for Day/Night | [world-structure.md](world-structure.md) Interactions with Other Systems | Overview, Rules 2 and 7, and Dependencies keep Day/Night as a peer subscriber, not a World Structure child. |
| Save/Load indirect resume path | [save-load-persistence.md](save-load-persistence.md) Rule 9 and indirect interactions | Rules 2, 15, and H-DN-RS-05 keep Day/Night out of Save/Load schema and hydration. |
| NPC schedule data-only catch-up and active-zone ticks | [npc-system.md](npc-system.md) Rules 5-10 | Day/Night provides phase predicates; NPC System owns schedule interpretation and scene instances. |
| Menus silent resume rule | [menus-settings.md](menus-settings.md) Rule 10 and H-MS-WS-06 | UI Requirements and H-DN-RS-03 forbid resume recaps, toasts, and phase announcements. |
| `PROJECT_WORLD_EPOCH_UTC_seconds` and `world_clock_offset_seconds` | [entities.yaml](../registry/entities.yaml) constants | `world_time_seconds_derivation` uses fixed epoch plus authored offset; neither is save state. |
| Art bible light-source logic and no zone-grade post-process | [art-bible.md](../art/art-bible.md) Sections 1, 2, 4, and 8.9 | Visual/Audio Requirements and H-DN-VA-01 require practical-source logic, readable atmosphere, and no LUT/overlay phase signaling. |
| Unity 6.3 LTS + URP / Addressables constraints | [VERSION.md](../../docs/engine-reference/unity/VERSION.md), [rendering.md](../../docs/engine-reference/unity/modules/rendering.md), [addressables.md](../../docs/engine-reference/unity/plugins/addressables.md) | Open Questions defer light-rig substrate; H-DN-PERF-02 forbids phase application from triggering Addressables loads or new texture residency. |

## Tuning Knobs

| Knob | Default | Safe Range | Higher Means | Lower Means |
|---|---:|---:|---|---|
| `day_length_seconds` | 7200 s | 1800-14400 s | Slower visible phase turnover; fewer phase changes per session. | Faster phase turnover; risk of theme-park cycling. |
| `time_scale_world` | 1.0 | 0.0-8.0 | More world-clock movement per real second. | Less movement; 0 freezes clock for test harnesses or paused states only. |
| `world_clock_offset_seconds` | 0 s | -7200 to 7200 s | Later derived world-clock phase at the same UTC sample. | Earlier derived world-clock phase at the same UTC sample. |
| `dawn_transition_seconds` | 600 s | 120-1200 s | Longer dawn band; more gradual light-source changes. | Shorter transition; risk of abrupt phase read. |
| `dusk_transition_seconds` | 600 s | 120-1200 s | Longer dusk band; more gradual light-source changes. | Shorter transition; risk of abrupt phase read. |
| `phase_application_max_ms` | 16 ms | 1-33 ms | More tolerance for applying active-zone profile in one frame. | Stricter; may require splitting scene-control application over frames. |
| `daynight_resume_max_ms` | 5 ms | 1-20 ms | More tolerance for resume derivation and event logging. | Stricter; catches inefficient handler work early. |

### Cross-knob interactions

- `day_length_seconds` and NPC schedule authoring must stay aligned. If a route assumes dawn lasts 10 in-game minutes, changing dawn duration changes where NPCs appear after UTC derivation.
- `time_scale_world` multiplies UTC distance from the project epoch. Raising it can make a one-week absence wrap many times; downstream systems must not infer event count from Day/Night wraps.
- `world_clock_offset_seconds` re-phases the entire world at the same UTC sample. Changing it after content authoring invalidates phase-specific screenshots, route expectations, and smoke-test fixtures.
- `phase_application_max_ms` must stay small because it runs near `ZoneActiveEvent`. If active-zone application becomes heavy, the zone profile is doing asset or scene work that belongs elsewhere.

## Visual/Audio Requirements

Day/Night is a presentation-facing world system, so this section is required for the GDD.

### Visual rules

- Day/Night must follow the art bible's light-source logic: exterior overcast daylight around 6000K, warm practicals around 2200-3200K where physical sources exist, and no global LUT/post-process zone-grade changes.
- `InnHours` in CityHub uses working warmth: practical interior sources compete with overcast exterior light. It is public, readable, and politically textured.
- `CourtHours` in CityHub seals exterior windows into dark mirrors and lets practical interior sources own the room. It is not a danger filter.
- `DawnTransition` and `DuskTransition` are short, gradual changes in practical-source states and sky/ambient balance, not events.
- Haunt profiles preserve the haunt's authored light inventory. Night may reduce exterior leakage or alter sky contribution, but it must not add magical moonlight, glowing breadcrumbs, or warning overlays.
- Phase changes must be readable at the level of mood and legibility, not through special VFX.
- Active-zone lighting changes must not allocate or load new texture groups outside World Structure's active-zone residency contract.

### Audio rules

- Day/Night owns no audio playback at T1.
- Future Audio System may consume phase data for ambient profile selection, but no stinger, alarm, chime, or "night falls" cue is permitted as a primary information channel.
- Silence is an acceptable fallback if Audio System is absent.

## UI Requirements

Day/Night has no Layer 1 HUD and no abstract UI at T1.

Allowed:

- No direct UI.
- Diegetic downstream expression through NPC availability, faction notices, dialogue availability, world props, or authored light-source states.

Forbidden:

- Clock widget.
- Phase label.
- "Dawn", "Night", or "Court Hours" toast.
- Resume recap.
- Map marker, quest arrow, path highlight, or schedule overlay.
- Full-screen tint or UI overlay used as a time-of-day signal.

## Acceptance Criteria

### Test-Type Taxonomy

Use only these labels:

- Unit
- Integration
- Editor-validation
- Dev-build smoke
- Profiled playtest

### Core clock behavior

**H-DN-CR-01 - UTC-derived local clock**

**GIVEN** `now_utc_seconds = 1767233100`, `PROJECT_WORLD_EPOCH_UTC_seconds = 1767225600`, `time_scale_world = 1.0`, `world_clock_offset_seconds = 0`, and `day_length_seconds = 7200`, **WHEN** `world_time_seconds_derivation` is evaluated, **THEN** the resulting `world_time_seconds` is `300` and remains inside `[0, day_length_seconds)`.

*Unit | gameplay-programmer | T1-blocking*

**H-DN-CR-02 - Phase coverage validation**

**GIVEN** the default T1 phase thresholds, **WHEN** the Day/Night profile validator scans all phase ranges, **THEN** the ranges cover `[0, day_length_seconds)` exactly once with no gaps, overlaps, invalid ids, or unreachable phase intervals.

*Editor-validation | gameplay-programmer | T1-blocking*

**H-DN-CR-03 - Phase resolution**

**GIVEN** default thresholds and `world_time_seconds` values of `0`, `599`, `600`, `3899`, `3900`, `4499`, `4500`, and `7199`, **WHEN** `phase_from_world_time` is evaluated, **THEN** outputs are respectively `DawnTransition`, `DawnTransition`, `InnHours`, `InnHours`, `DuskTransition`, `DuskTransition`, `CourtHours`, and `CourtHours`.

*Unit | gameplay-programmer | T1-blocking*

**H-DN-CR-04 - New-game clock bootstrap**

**GIVEN** a fresh install with no gameplay save and character creation begins at a known UTC fixture, **WHEN** Day/Night transitions from `ClockUninitialized` to `ClockReady`, **THEN** `world_time_seconds` is derived from `now_utc_seconds`, `PROJECT_WORLD_EPOCH_UTC_seconds`, `time_scale_world`, `world_clock_offset_seconds`, and `day_length_seconds`; no `new_game_initial_time_seconds` constant or persisted Day/Night clock field is consulted.

*Integration | gameplay-programmer + qa-tester | T1-blocking*

### World Structure contract

**H-DN-WS-01 - SessionResumeEvent handler runs before first zone phase application**

**GIVEN** a session load from a valid save fixture where World Structure publishes `SessionResumeEvent` before `ZoneActiveEvent`, **WHEN** Day/Night's event log is inspected, **THEN** Day/Night records resume derivation completion before any active-zone profile application.

*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-DN-WS-02 - ZoneActive profile application**

**GIVEN** Day/Night is `ClockReady` and World Structure publishes `ZoneActiveEvent(zoneId, zoneType)`, **WHEN** the active zone has a valid `DayNightZoneProfile`, **THEN** Day/Night applies exactly one current phase profile for that active zone within `phase_application_max_ms`, and does not apply any profile to idle zones.

*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-DN-WS-03 - ZoneIdle resource prohibition**

**GIVEN** a zone has reached `ZoneIdle`, **WHEN** memory/reference validation runs, **THEN** Day/Night retains no GameObjects, MonoBehaviours, Renderers, Lights, Materials, Textures, Animators, AudioSources, physics bodies, live scene handles, Addressable scene handles, or runtime light references from that idle zone.

*Dev-build smoke + Editor-validation | gameplay-programmer + engine-programmer | T1-blocking*

**H-DN-WS-04 - Missing zone profile validation**

**GIVEN** every `ZoneManifest` entry in the T1 build, **WHEN** Editor-validation scans Day/Night profile bindings, **THEN** each zone has exactly one valid `DayNightZoneProfile`, and missing or duplicate bindings fail validation.

*Editor-validation | gameplay-programmer | T1-blocking*

### Resume and organic discovery

**H-DN-RS-01 - Resume UTC derivation**

**GIVEN** a session resumes at `now_utc_seconds = 1767484800` (three real days after `PROJECT_WORLD_EPOCH_UTC_seconds = 1767225600`), with `time_scale_world = 1.0`, `world_clock_offset_seconds = 0`, and `day_length_seconds = 7200`, **WHEN** Day/Night processes `SessionResumeEvent`, **THEN** `world_time_seconds = 0` and the current phase resolves to `DawnTransition` before active-zone profile application.

*Unit | gameplay-programmer | T1-blocking*

**H-DN-RS-02 - Zero elapsed resume still records handler completion**

**GIVEN** `SessionResumeEvent(real_elapsed_seconds = 0)`, **WHEN** Day/Night handles the event, **THEN** no elapsed-delta clock math is applied, but the event log records derivation completion so ordering against `ZoneActiveEvent` remains testable.

*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-DN-RS-03 - Silent resume**

**GIVEN** Day/Night processes `SessionResumeEvent`, **WHEN** Menus & Settings, Layer 1 HUD, and any Day/Night-authored surfaces are inspected, **THEN** no player-facing UI, audio cue, recap, toast, elapsed-time message, phase label, marker, or notification is produced by Day/Night.

*Integration | ui-programmer + qa-tester | T1-blocking*

**H-DN-RS-04 - Organic active-zone result after resume**

**GIVEN** a valid resume changes the current phase from `InnHours` to `CourtHours`, **WHEN** the player reaches CityHub and `ZoneActiveEvent` is processed, **THEN** the active-zone phase profile reflects `CourtHours` through authored light-source states and phase predicates only; no resume recap or phase announcement appears.

*Dev-build smoke | gameplay-programmer + art-lead + qa-tester | T1-blocking*

**H-DN-RS-05 - Resume does not read persisted Day/Night clock state**

**GIVEN** the T1 Save/Load schema and Day/Night resume boot sequence, **WHEN** schema validation and event-subscription inspection run, **THEN** there is no `world_time_seconds_at_last_exit`, `world_time_seconds`, or equivalent persisted Day/Night clock field; Day/Night registers no Save/Load hydration callback; and resume clock state is derived only after `SessionResumeEvent` from UTC plus project epoch and authored offset.

*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

### Presentation constraints

**H-DN-VA-01 - No post-process/LUT phase signaling**

**GIVEN** all T1 Day/Night profiles, **WHEN** the runtime scene and profile assets are inspected, **THEN** no phase transition is implemented through global LUT swaps, full-screen color-grade jumps, red/blue warning overlays, or non-diegetic UI tint layers.

*Editor-validation + Dev-build smoke | technical-artist + qa-tester | T1-blocking*

**H-DN-VA-02 - CityHub phase legibility**

**GIVEN** CityHub is active in `InnHours` and then `CourtHours`, **WHEN** a dev-build smoke run captures both states from the same authored camera anchors, **THEN** `InnHours` reads as working warmth competing with overcast exterior light, `CourtHours` reads as practical-source-dominant interior light, and required navigation remains readable without path highlights or UI overlays.

*Dev-build smoke | art-lead + qa-tester | T1-blocking*

**H-DN-VA-03 - Haunt readability**

**GIVEN** HauntMansion is active across all four T1 phases, **WHEN** a profiled playtest traverses the critical path on minimum-spec display settings, **THEN** required traversal geometry and interactable silhouettes remain readable, and no phase requires glow strips, markers, warning overlays, or special VFX to navigate.

*Profiled playtest | art-lead + qa-tester | T1-blocking*

**H-DN-PERF-01 - Resume handler budget**

**GIVEN** any valid World Structure `SessionResumeEvent` payload, including the maximum default elapsed payload of 604800 seconds, **WHEN** Day/Night processes the event, **THEN** the UTC derivation handler completes within `daynight_resume_max_ms` on the Min-Spec Profile and does not allocate or load assets proportional to elapsed time.

*Profiled playtest | gameplay-programmer + engine-programmer | T1-blocking*

**H-DN-PERF-02 - Active-zone phase application budget**

**GIVEN** a zone reaches `ZoneActive`, **WHEN** Day/Night applies the current active-zone phase profile, **THEN** the phase application completes within `phase_application_max_ms` and does not trigger Addressables loads, new texture residency, or idle-zone object activation.

*Profiled playtest | gameplay-programmer + engine-programmer | T1-blocking*

### Summary Table

| ID | Covers | Test Type | Owner | T1-Blocking |
|---|---|---|---|---|
| H-DN-CR-01 | UTC-derived local clock | Unit | gameplay-programmer | Yes |
| H-DN-CR-02 | Phase coverage validation | Editor-validation | gameplay-programmer | Yes |
| H-DN-CR-03 | Phase resolution | Unit | gameplay-programmer | Yes |
| H-DN-CR-04 | New-game clock bootstrap | Integration | gameplay-programmer, qa-tester | Yes |
| H-DN-WS-01 | SessionResumeEvent before active-zone phase application | Integration | gameplay-programmer, qa-tester | Yes |
| H-DN-WS-02 | ZoneActive profile application | Integration | gameplay-programmer, qa-tester | Yes |
| H-DN-WS-03 | ZoneIdle resource prohibition | Dev-build smoke + Editor-validation | gameplay-programmer, engine-programmer | Yes |
| H-DN-WS-04 | Missing zone profile validation | Editor-validation | gameplay-programmer | Yes |
| H-DN-RS-01 | Resume UTC derivation | Unit | gameplay-programmer | Yes |
| H-DN-RS-02 | Zero elapsed resume handler completion | Integration | gameplay-programmer, qa-tester | Yes |
| H-DN-RS-03 | Silent resume | Integration | ui-programmer, qa-tester | Yes |
| H-DN-RS-04 | Organic active-zone result after resume | Dev-build smoke | gameplay-programmer, art-lead, qa-tester | Yes |
| H-DN-RS-05 | No persisted Day/Night clock state | Unit + Integration | gameplay-programmer, qa-tester | Yes |
| H-DN-VA-01 | No post-process/LUT phase signaling | Editor-validation + Dev-build smoke | technical-artist, qa-tester | Yes |
| H-DN-VA-02 | CityHub phase legibility | Dev-build smoke | art-lead, qa-tester | Yes |
| H-DN-VA-03 | Haunt readability | Profiled playtest | art-lead, qa-tester | Yes |
| H-DN-PERF-01 | Resume handler budget | Profiled playtest | gameplay-programmer, engine-programmer | Yes |
| H-DN-PERF-02 | Active-zone phase application budget | Profiled playtest | gameplay-programmer, engine-programmer | Yes |

**Total: 18 criteria. 18 T1-blocking, 0 advisory.**

## Non-Goals

- No networking, FishNet, server-synchronized clock, account-linked time, or multiplayer authority.
- No persistent-server time or T3 always-on simulation.
- No LLM behavior, generated dialogue, or moderation surface.
- No combat stat changes, spawn-table changes, loot changes, XP modifiers, or enemy buffs based on time-of-day at T1.
- No player-facing clock, calendar, quest marker, schedule overlay, or phase alert.
- No save-file schema ownership, persisted Day/Night clock field, or Day/Night Save/Load hydration callback in this GDD.
- No live ticking, rendering, audio, animation, physics, or scene handles for `ZoneIdle`.
- No weather system, moon-phase system, seasons, holidays, or calendar events.
- No cutscenes or scripted time skips.

## Open Questions

| Question | Owner | Deadline | Status |
|---|---|---|---|
| **Pause semantics integration** - Does T1 pause stop local simulation time or only capture input? Menus & Settings leaves this open. Day/Night will honor the resolved policy. | `game-designer` + `engine-programmer` | Before Pause Menu implementation | Open - T1-blocking integration question |
| **Exact CityHub phase thresholds** - Defaults are supplied for implementation, but T1 playtest should validate whether 2-hour days and 10-minute dawn/dusk bands support session pacing. | `game-designer` + `qa-tester` | T1 playtest | Pinned with revisit flag |
| **Light rig implementation substrate** - Does active-zone phase application use scene-authored light rigs, ScriptableObject profile data, Timeline-free animator curves, or another Unity 6.3-safe pattern? | `technical-artist` + `unity-specialist` | Before first Day/Night prototype | Open - implementation ADR candidate |
| **Audio consumption timing** - Should Audio System consume phase changes immediately at threshold crossing or only on `ZoneActiveEvent` and ambient loop boundaries? | `audio-director` + `gameplay-programmer` | Audio System GDD | Deferred |
| **Layer 2 time literacy** - Should any diegetic objects communicate rough time, such as chapel bells, candle maintenance, or posted court hours? If yes, ownership belongs to downstream world prop, NPC, Audio, or Layer 2 UI specs, not Day/Night HUD. | `game-designer` + `art-director` | Before Faction Board / Dialogue UI GDDs | Open |
