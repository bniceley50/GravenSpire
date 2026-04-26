# Save / Load & Persistence

> **Status**: In Design
> **Author**: Claude Code (session with brian, 2026-04-23)
> **Last Updated**: 2026-04-23
> **Implements Pillar**: Primary — **P5 Stakes Are Honest** (save integrity is not negotiable; save failure has legible, non-silent consequences). Supports — **P1 The World Is Not Your Story** (Rule 13 between-session catch-up depends on persisted `last_exit_timestamp_utc`).

## Locked Inputs (non-negotiable — copied exactly from upstream sources)

These three inputs are authoritative upstream contracts. This GDD reproduces them (in §Detailed Design, §Formulas, §Edge Cases, §Dependencies, §Acceptance Criteria) rather than redesigning them. Any apparent conflict is a bug in this GDD, not a design choice.

1. **Save-integrity rule** — [.claude/rules/save-integrity.md](../../.claude/rules/save-integrity.md) is the authoritative contract: HMAC with per-install derived key, verify before gameplay-state deserialization, version stamp as the first field, fail-loud on version mismatch (never partial-load), forward-only migrations with real prior-version fixture saves in `tests/fixtures/saves/v[N]/`, player-authored strings bounded-length and sanitized on load. Reproduced — not redesigned — below.

2. **World Structure interface** — [world-structure.md](world-structure.md) §Interactions + §Dependencies already lock the Save/Load contract:
   - **WS emits to Save/Load** during `SaveCheckpointing`: `PlayerZoneMembership` (zoneId + `Vector3` position + zoneType), `ZoneTransitionTimestamp`, `last_exit_timestamp_utc` (Rule 13), `CorpseRecord` (zoneId + `Vector3` position + `expiry_timestamp_utc`).
   - **Save/Load emits to WS**: `SaveWriteConfirmed` / `SaveFailedEvent`.
   - **`save_mutex_max_ms`** (150 ms target, 100–500 ms safe range) is the hard timeout for save-on-transition (world-structure Rule 12).
   - **`SessionResumeEvent`** fires before any `ZoneActiveEvent` on session load (world-structure Rule 13). `last_exit_timestamp_utc` is the persistence field that drives the elapsed-delta computation.
   This interface is reproduced (not redesigned) in §Dependencies + §Edge Cases + §Acceptance Criteria.

3. **Storage backend is deferred to ADR** — the systems-index lean note ("SQLite schema + character/world state serialization") is **not locked**. Backend selection (SQLite vs. JSON-on-disk vs. binary format vs. hybrid) remains an open **ADR-tba** item. This GDD specifies behavior, guarantees, and data contracts only; the concrete storage mechanism is determined by the ADR in the architecture phase.

## Summary

Save / Load & Persistence is Gravenspire's Foundation-layer data infrastructure: it owns the serialisation, integrity, and versioning contracts that let every gameplay system treat the save file as an authoritative source of truth across sessions. Players never interact with it directly; they experience what it enables — stats, inventory, faction standing, and zone membership surviving session boundaries; a world whose events can be computed deterministically from `last_exit_timestamp_utc` on resume (via [world-structure.md](world-structure.md) Rule 13 `SessionResumeEvent`). Three non-negotiables anchor the design: HMAC verification before any gameplay-state deserialisation, version stamp as the first field with fail-loud mismatch handling, and the World Structure save-on-transition contract (`SaveCheckpointing` → `SaveWriteConfirmed` / `SaveFailedEvent` within `save_mutex_max_ms`, with `SaveTimedOutEvent` owned by WS when confirmation is missing). The storage backend itself is deferred to an ADR — the GDD specifies what saves, when, with what guarantees, and what the failure modes look like, independent of whether the realisation is SQLite, JSON-on-disk, binary, or hybrid. T1 scope is single-player offline local save; the architecture accommodates T2 co-op coordination and T3 persistent-server saves without structural rework.

## Overview

Save / Load & Persistence is the data-layer infrastructure that makes Gravenspire's world durable. It owns the serialization, integrity, and versioning contracts that allow every gameplay system — combat, inventory, character progression, faction state, corpse recovery, faction reputation — to treat the save file as an authoritative source of truth across sessions. Players never interact with it directly; they experience what it enables: a character whose stats, inventory, faction standing, and zone membership survive session boundaries; a world whose faction events and NPC schedule advances can be computed from the elapsed delta derived from `last_exit_timestamp_utc` on resume (via [world-structure.md](world-structure.md) Rule 13's `SessionResumeEvent`); and the assurance that **no incomplete save is ever treated as valid gameplay state, no tamper is silent, no version drift is quietly accepted**.

Three non-negotiables are architecturally prior to every design choice in this GDD and are reproduced — not redesigned — from upstream locks:

1. **HMAC verification happens before any gameplay-state field is deserialized.** The save file carries an HMAC signature computed with a per-install derived key ([.claude/rules/save-integrity.md](../../.claude/rules/save-integrity.md)). The loader verifies the signature first; a mismatch rejects the save with a **loud error** — never a silent fall-through to default state. Tampered or corrupted saves never reach the gameplay deserializer.

2. **The version stamp is the first field, checked before any subsequent read.** A newer loader reading an older save attempts a forward-only migration against real prior-version test fixtures; if no migration path exists, the loader fails loud. An older loader reading a newer save is rejected outright. Partial loads are never attempted.

3. **The World Structure save-on-transition contract is locked.** `SaveCheckpointing` fires on `ZoneLoading` entry ([world-structure.md](world-structure.md) Rule 8). Save/Load's normal outcomes are `SaveWriteConfirmed` (write succeeded) and `SaveFailedEvent` (write failed — e.g., disk full per world-structure.md Edge C4). World Structure waits for one of these two events; if `SaveWriteConfirmed` specifically is not received within the `save_mutex_max_ms` window (150 ms target, 100–500 ms safe range per world-structure.md Rule 12), **World Structure itself** emits `SaveTimedOutEvent`, aborts the transition before commit, and routes to `ZoneError`. The timeout is therefore a World-Structure-side deadline on missing confirmation — not a Save/Load latency SLA — though Save/Load must be engineered to complete within the window under normal conditions so the timeout isn't tripped routinely.

The storage backend itself — SQLite, JSON-on-disk, binary format, or a hybrid — is **not decided in this GDD** and remains an open ADR item. This GDD specifies *what* saves, *when* it saves, *what guarantees* the save provides, and *what the failure modes look like*; the concrete storage mechanism is the responsibility of an ADR in the architecture phase, constrained to the Unity 6.3 LTS + C# .NET 8+ stack pinned in [.claude/docs/technical-preferences.md](../../.claude/docs/technical-preferences.md). Tier 1 MVP scope is a single-player offline local save (per [DECISIONS.md](../../DECISIONS.md) D003); the architecture must accommodate Tier 2 co-op save coordination and Tier 3 persistent-server saves without structural rework.

## Player Fantasy

Somewhere beneath the city, a clerk keeps your record, and the clerk is meticulous. Your two-hour expedition into the Undercroft, the rival who finally crossed you, the patron whose trust you spent — all of it is filed before you stand up from the chair. You do not think about the clerk, the same way you do not think about the floor holding you up; the work is invisible because it is done correctly. But if the clerk ever drops a page, you will hear about it before you take another step. The silence you experience is the silence of competence, not of loss. **The stakes you lived through are the stakes that remain.**

### Anchor moment

Returning three days later to find your corpse precisely where it fell, with the same recovery stakes still in force, in a world that has moved on around it — evidence that the record held while you were away. `CorpseRecord` (`zoneId` + world-space position + `expiry_timestamp_utc`) and `last_exit_timestamp_utc` ([world-structure.md](world-structure.md) Rule 13 + Corpse Retention Model) are what Save/Load carries across the session boundary; Death & Corpse Recovery owns the recovery-stakes resolution itself. The player feels the persistence as *continuity of their own actions*, not as a technical feature.

### Anti-fantasy — what the player should NOT feel

- **Silent loss** — a play session ending and the next session not reflecting it, with no error shown. If a save fails to write or an event fails to persist, the system surfaces the failure explicitly (per Overview non-negotiable #3 and the `SaveFailedEvent` / `SaveTimedOutEvent` contracts); **silence always means success, never a hidden gap**.
- **Mysterious rollback** — the loader quietly reverting to an older save without telling the player. If state cannot be trusted — crash mid-write, HMAC mismatch, version mismatch with no migration path — the failure is **surfaced explicitly** per the locked save-integrity contract; the player is never silently placed in an older scene as if nothing happened.
- **"Did my last hour count?"** — the anxiety of distrusting the record. Because integrity failures are **loud** rather than silent and because trusted state is continued forward rather than silently replaced, the player's relationship to the record is one of *felt continuity* — they do not have to perform their own audit to believe their time was real.

### Reference register

Municipal / archival rather than haunted-architectural: the ledger, the clerk's book, the filed record. Explicitly not heroic-fantasy "chosen one" framing and not theme-park-MMO save-point theater. Matches the tonal lock of [world-structure.md](world-structure.md) Player Fantasy (Clarke's *Piranesi*, Jackson's *Haunting of Hill House*, Friedrich's small figures in out-scaling landscapes) but shifts emphasis from architectural memory to bookkeeping integrity — the same world that remembers in stone also remembers in ink.

## Detailed Design

### Core Rules

1. **Save state categories.** Four logical categories are persisted: **(a) Player State** — character identity (name, class, appearance tokens), Character Progression's whitelisted state (`progression_schema_version`, `class_id`, `current_level`, `total_xp`, `spell_eligibility_tier`), class-owned abilities if a future Class Design GDD authors them, equipped and carried inventory, and currency. Character Creation's T1 first-save payload adds an **initial character seed** subrecord containing `local_character_id`, `starting_class_id`, appearance token ids, `onboarding_eligible`, `onboarding_intro_state`, `starting_equipment_template_id`, empty carried-inventory state, and `starting_faction_reputation_baseline`. These seed-consumed fields are persisted in the initial save for auditability; downstream GDDs may later materialize or migrate them into evolving Inventory, Character Progression, and Faction Reputation state. `creation_schema_version` is a Character Creation schema marker in the first-save payload, but its final placement (Player State vs. Session Metadata vs. a dedicated initial-record category) remains open in [character-creation.md](character-creation.md) §Open Questions before implementation; **(b) World State** — `PlayerZoneMembership` (zoneId + `Vector3` position + zoneType), `CorpseRecord` (zoneId + `Vector3` position + `expiry_timestamp_utc`) if one exists, NPC records (`npcId`, `scheduleStateId`, `routeProgress`, `availabilityState`, `knownNameState`, `lastEvaluatedTimestampUtc`; NPC System owns the schema), faction state (reputation, faction control of zones), event-log entries already committed; **(c) Session Metadata** — `last_exit_timestamp_utc` (UTC epoch seconds per [world-structure.md](world-structure.md) Rule 13), save-format version stamp, HMAC signature; **(d) Player-Authored Strings** — character name and any other player-typed strings, length-bounded and sanitised on load. Explicitly NOT saved (per [.claude/rules/save-integrity.md](../../.claude/rules/save-integrity.md) §What Saves): engine internals, derived/cached values, runtime handles.

2. **Single active local character record at T1.** T1 exposes one active local character record at a time (per [DECISIONS.md](../../DECISIONS.md) D003 — single-player offline local save). Multi-slot UI, multi-character management, and the specific save-identity model (single file per character, save index, slot enumeration) are deferred to T2+ as ADR-tba items — not locked by this GDD.

3. **Version stamp is the first field, read before any payload byte.** Per [.claude/rules/save-integrity.md](../../.claude/rules/save-integrity.md), the version stamp occupies the leading bytes of the persisted record. The loader reads and validates it before any other field is touched.

4. **HMAC verification happens before any gameplay-state deserialisation.** The save file carries an HMAC signature computed with a per-install *derived* key (derivation strategy is ADR-tba; [.claude/rules/save-integrity.md](../../.claude/rules/save-integrity.md)). The loader verifies the signature over the full payload before deserialising; mismatch = loud rejection, never silent fall-through to default state. Tampered or corrupted saves never reach the gameplay deserialiser.

5. **Save triggers at T1.** Four save triggers exist at T1, matching the three that enter World Structure's `SaveCheckpointing` state (per [world-structure.md](world-structure.md) §States and Transitions — `SaveCheckpointing` row) plus Session-Exit Save:
   - **Transition Save** — WS fires `SaveCheckpointing` on `ZoneLoading` entry ([world-structure.md](world-structure.md) Rule 8).
   - **Manual Save** — player-invoked from the pause menu; routed through Menus & Settings into WS's `SaveCheckpointing` mutex.
   - **Autosave Tick** — periodic, at `save_autosave_interval` — tuning knob **owned by Save/Load** per [world-structure.md](world-structure.md) §Tuning Knobs — Cross-reference (knobs owned elsewhere); registered in §Tuning Knobs of this GDD. Routes through WS's `SaveCheckpointing` mutex.
   - **Session-Exit Save** — fires on clean in-app quit or graceful shutdown signal before the application exits; uses the Save/Load mutex (Rule 6).

   Each trigger fires a write request to Save/Load; callers do not manipulate save state directly. **The `save_mutex_max_ms` timeout ([world-structure.md](world-structure.md) Rule 12) applies to the transition-save path specifically** (during `ZoneLoading` stream-ahead). Other save paths are bounded by their callers' UX needs, not by Rule 12.

6. **Save mutex and queue semantics.** Only one write may be in-flight. If a second trigger fires while a write is in progress, the incoming request is *queued* (not dropped, not coalesced). Queue depth at T1 is one — a newer request replaces an older queued request (later state is always more current). **Queued requests may be externally cancelled by upstream systems whose context renders them redundant.** Specifically, per [world-structure.md](world-structure.md) Edge A2a: a **Manual Save queued during WS's `ZoneLoading` state is discarded when WS exits `ZoneLoading`** (to `ZoneActive` or `ZoneError`), because the Transition Save that ran during `ZoneLoading` already captured the relevant state. On write completion, any **non-cancelled** queued request begins immediately. An Autosave Tick that fires during an active Transition Save is implicitly redundant under the same rationale and is **suppressed** (the tick does not enqueue a separate write).

7. **Atomic write guarantee.** A write is either fully committed or absent from the player's perspective — a partial or in-progress write is never observable as a valid save. If any failure occurs mid-write, the pre-existing save record remains intact and loadable. The *mechanism* that delivers this guarantee (temp-file-rename, write-ahead log, two-slot rotation, transactional write) is **ADR-tba** — see §Open Questions entry *"Save atomicity mechanism + power-loss model"*; the ADR must satisfy this behavioural guarantee on Windows NTFS and macOS APFS.

8. **Write outcome events.** On successful commit Save/Load emits `SaveWriteConfirmed`; on any detected write failure it emits `SaveFailedEvent`. **`SaveWriteConfirmed` semantics**: the event means *"write submitted and OS/filesystem-acknowledged at the application boundary"* — it does **NOT** imply physical-media durability (fsync-to-platter may complete after the event). Full durability is implementation-level and belongs to the atomicity ADR. **Dispatch semantics**: `SaveWriteConfirmed` must dispatch synchronously on the same call frame as the commit decision (not queued for end-of-frame) — this is an ADR-tba constraint on the event bus chosen for Save/Load ↔ WS communication (see §Open Questions entry *"Save event dispatch semantics"*), because WS's `save_mutex_max_ms` deadline clock is running in real time. Cross-refs: [world-structure.md](world-structure.md) Rule 12 for the timeout contract; Edge C4 for disk-full mapping to `SaveFailedEvent`.

8a. **Downstream save-stability barriers.** Before `Writing` reads a downstream-owned payload, Save/Load must call that system's declared save-stability hook when one exists. Character Progression's hook is `ProgressionSaveBarrier`: it must settle same-frame Combat kill-credit dispatch, XP transactions, level-up chains, and future XP adjustments before Save/Load reads `CharacterProgressionSaveState`. Save/Load may serialize only the post-barrier progression payload; it never writes pre-award XP when the same frame's gameplay state has already earned post-award XP.

9. **Session resume sequencing.** The hard upstream ordering rule from [world-structure.md](world-structure.md) Rule 13 + H-CR-13b remains: **`SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` must fire before any `ZoneActiveEvent`** in the session-load sequence. Save/Load's load path must therefore deliver `last_exit_timestamp_utc` to WS early enough that WS can publish `SessionResumeEvent` before publishing the first `ZoneActiveEvent`. Character Progression adds one downstream hydration-order lock: during `Resuming`, Save/Load hydrates and validates Character Progression first, obtains `ProgressionBaselineSnapshot(current_level, permanent_max_health, permanent_max_mana, spell_eligibility_tier)`, and only then may Combat Core hydrate or build the player combat actor using that snapshot's health/mana maxima. Other gameplay-state ordering remains downstream-owned: each subscriber's Rule 13 catch-up handler specifies state-readiness requirements in its own GDD, and Save/Load's load path must satisfy those declared requirements before gameplay is enabled.

10. **Forward-only migration; no write-back at load.** Per [.claude/rules/save-integrity.md](../../.claude/rules/save-integrity.md), migrations are forward-only and sequential (v1→v2→v3, never v1→v3 directly). Real prior-version test fixtures are maintained in `tests/fixtures/saves/v[N]/`. **A successful migration at load produces an in-memory current-version payload but does NOT write back to disk** — keeps the load path read-first, avoids a second write/failure branch inside Loading, preserves the pre-migration file as a recovery option. The migrated form is persisted on the *next* successful normal save trigger (Transition / Manual / Autosave / Session-Exit).

11. **Failure is loud, never silent.** Every failure class (see §Edge Cases for the full failure-mode matrix) maps to either a `SaveFailedEvent` emission, a named loader state (`WriteFailed`, `LoadRejected`), or a surfaced loud error. No failure class resolves to "load anyway with degraded or default state." The fail-loud requirement is stated behaviourally: the integrity-verification step in the load path must be wrapped so that *any* exception or detected mismatch unconditionally produces a `SaveFailedEvent` or `LoadRejected` — never a silent partial load. The implementation entry point for the load path must not rely on exception semantics that can swallow failures (a concern flagged for the storage-backend ADR).

12. **Player-authored strings are sanitised on load.** Character name and any player-typed strings are length-capped and control-character-stripped during deserialisation, before the value is passed to any downstream system ([.claude/rules/save-integrity.md](../../.claude/rules/save-integrity.md) §What Saves + [AGENTS.md](../../AGENTS.md) §11).

13. **Session-Exit Save failure blocks controlled shutdown.** If the Session-Exit Save emits `SaveFailedEvent`, the **in-app quit flow is blocked pending player acknowledgement**: a modal presents the failure and offers **Retry** or **Quit Without Saving**. Silent fallback to the last successful save is forbidden — it would violate the fail-loud posture of [.claude/rules/save-integrity.md](../../.claude/rules/save-integrity.md) and [world-structure.md](world-structure.md) Edge C4. Uncontrolled termination (OS kill, power loss, process crash) remains outside this contract — recovery on next session-load follows the normal loader path (last intact save loaded; version + HMAC checks apply).

14. **First-run vs. file-missing distinction.** Save/Load distinguishes two cases that both present as "no save file at the expected location":
    - **First-run path** — a character's save slot has never been initialised (new game / new character). The load request resolves to *"no-save-to-load"*; the system initialises fresh state; the first save trigger (any of Rule 5's four) writes the initial record. **Not an error; no `LoadRejected` event emitted.**
    - **Missing-file failure** — the slot was previously initialised (the system has a record that this character had a successful save) but the expected file is absent at load time. This IS a loud failure: the loader enters `LoadRejected` with a distinct `SaveMissing` failure class. Preserves the fail-loud contract and avoids masking data-loss symptoms as expected bootstrap.

    The *mechanism* used to track "slot initialised" status (separate marker file, dedicated metadata field, save index entry) is implementation-level and belongs to the storage-backend ADR.

15. **Autosave clock reset on any confirmed save.** The autosave clock resets on any `SaveWriteConfirmed`, regardless of trigger type — Transition Save, Manual Save, Autosave Tick, or Session-Exit Save. `next_autosave_tick = t_last_confirmed_save + save_autosave_interval`. If a Transition Save, Manual Save, or Autosave Tick occurs before the pending autosave fires, the pending autosave is cancelled and the clock restarts from the confirmed-save timestamp.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior / Memory |
|-------|-----------------|----------------|-------------------|
| `Idle` | System initialised; or `Writing` / `WriteFailed` / `Loading` / `Migrating` / `LoadRejected` / `Resuming` reach their terminal transition into Idle | Write request received (→ `Writing`); or Load request received (→ `Loading`) | No I/O in progress. The mutex queue (depth 1, Rule 6) accumulates here. |
| `Writing` | Write request dequeued from `Idle` (or from the mutex queue as a follow-on) | `SaveWriteConfirmed` emitted → `Idle`; or fatal write error → `WriteFailed` | Invoke declared downstream save-stability hooks before reading payloads, including Character Progression's `ProgressionSaveBarrier`; then read current in-memory Player State, World State, Session Metadata (including WS-provided `last_exit_timestamp_utc`, `PlayerZoneMembership`, `CorpseRecord` if present); serialise in version-stamp-first order; compute HMAC over full payload; commit atomically per Rule 7. Write-attempt context (trigger type, timestamp) held in transient memory for the outcome event. |
| `WriteFailed` | Fatal error during `Writing`: disk full ([world-structure.md](world-structure.md) Edge C4), key derivation failure, I/O error, atomicity-check failure | Outcome event handled by caller → `Idle` | Emit `SaveFailedEvent`. Prior committed save is untouched (atomicity guarantee, Rule 7). If the failed trigger was Session-Exit Save, the quit-flow is blocked per Rule 13 until player acknowledges Retry or Quit Without Saving. Terminal for the failed attempt; system returns to `Idle` after handling. |
| `Loading` | Load request received from `Idle` (session start, or explicit load action) | Full deserialisation complete → `Resuming`; or hard rejection → `LoadRejected`; or version < current with migration path available → `Migrating` | Read raw record from storage; **read and validate version stamp before touching any payload byte** (Rule 3); **verify HMAC over full payload before deserialising any gameplay state** (Rule 4). If either check fails, transition to `LoadRejected`. Holds raw buffer in transient memory until verification passes. |
| `Migrating` | `Loading` detected a save version < current version AND a migration path exists (Rule 10) | Migration chain succeeds → re-enter `Loading` with migrated in-memory payload; migration step fails → `LoadRejected` | Substate of `Loading`. Steps through sequential version migrations (v1→v2→...→current). Each step produces an in-memory migrated payload; **does NOT write back to disk** (Rule 10). For long migrations at future versions, this state should surface a loading-screen hint to the UI layer — not a T1 concern. |
| `LoadRejected` | Any hard rejection during `Loading`, `Migrating`, **or `Resuming`** — including (non-exhaustive; see §Edge Cases failure-mode matrix for the complete class list): HMAC mismatch; version mismatch with no migration path; older loader reading newer save; file corrupt / truncated; migration step failure; save-missing (Rule 14 — expected file absent); key derivation failure during verify; **unexpected exception during integrity verification (`IntegrityException`, §Edge Cases B10)**; **synchronous hydration failure during `Resuming` (`HydrationFailed`, §Edge Cases D3)** | Rejection acknowledged by caller → `Idle` | Emit `LoadRejected` event carrying the distinct failure class (see §Edge Cases failure-mode matrix). **Load aborts before gameplay is enabled — no partially hydrated session becomes playable.** If the rejection occurred during `Resuming` (e.g., `HydrationFailed` per §Edge Cases D3), some downstream systems may have received deserialised state during the failed hydration sequence; Save/Load does **not** lock transactional rollback of hydration calls at the GDD level. What is guaranteed: `ZoneActiveEvent` is never produced, so the session does not reach playable state. Cleanup semantics for any received-but-unused state are owned by the affected downstream systems (or by implementation-level decisions in the storage-backend ADR). Surfaces to Menus & Settings for player-facing presentation. Terminal for the rejected load; system returns to `Idle`. |
| `Resuming` | `Loading` (or `Migrating` → `Loading`) completes successfully with full deserialised state | Session Metadata delivered to WS + downstream systems notified they may proceed → `Idle`; or **synchronous hydration failure in a downstream system during delivery → `LoadRejected(HydrationFailed)` per §Edge Cases D3** | **Rule 9 hard ordering**: delivers Session Metadata (`last_exit_timestamp_utc`) to World Structure **before any `ZoneActiveEvent` fires**. Delivers deserialised Player State and World State to their owning systems. Character Progression hydrates and validates before Combat Core hydrates or builds the player combat actor; Combat receives only `ProgressionBaselineSnapshot` health/mana maxima for its runtime current-resource validation. Other gameplay-state ordering is locked only when downstream GDDs declare state-readiness requirements. |

### Interactions with Other Systems

| System | Published (this system emits) | Subscribed (this system consumes) | Interface Owner | Hard/Soft |
|---|---|---|---|---|
| **World Structure** (§1) | `SaveWriteConfirmed` (synchronous same-frame dispatch per Rule 8), `SaveFailedEvent`, deserialised Session Metadata including `last_exit_timestamp_utc` delivered pre-`ZoneActiveEvent` per Rule 9. | `SaveCheckpointing` state entry (Transition Save trigger, [world-structure.md](world-structure.md) Rule 8); `PlayerZoneMembership` (zoneId + `Vector3` position + zoneType), `ZoneTransitionTimestamp`, `last_exit_timestamp_utc`, `CorpseRecord` (zoneId + `Vector3` position + `expiry_timestamp_utc`) as save payload. | Bidirectional — WS owns the state-machine trigger and the data shape; Save/Load owns serialisation, HMAC, versioning, atomicity, and the outcome-event contract. | **Hard** |
| **Menus & Settings** (§3) | Status events for UI: `SaveInProgress` (during `Writing`), `SaveWriteConfirmed`, `SaveFailedEvent`, and **`LoadRejected` events carrying a distinct failure class per §Edge Cases failure-mode matrix** (the matrix is the authoritative class list — illustrative examples: HMAC mismatch / version mismatch no migration / older-loader-newer-save / file corrupt / migration failed / save-missing / key derivation failure / integrity exception / hydration failed) for UI presentation. Session-Exit Save failure requires the acknowledgement flow per Rule 13 (Retry / Quit Without Saving). | Manual Save trigger from the pause menu (Rule 5); player acknowledgement input for the Session-Exit Save failure flow. | Menus owns UI presentation and quit-flow dialog; Save/Load owns the event contract and the quit-flow gating per Rule 13. | **Soft** (UI degrades gracefully if missing; if Menus is absent the Session-Exit Save failure still emits `SaveFailedEvent` and the caller of the shutdown request handles acknowledgement) |
| **Character Creation** (§6) | Deserialised Player State on load (initial character record hydration). | Initial character record on first save (triggers first `Writing`). | Character Creation owns the character-record schema; Save/Load owns persistence. | **Hard** |
| **Character Progression** (§8) | Deserialised Character Progression state on load: `progression_schema_version`, `class_id`, `current_level`, `total_xp`, and `spell_eligibility_tier`; hydration validation result and `ProgressionBaselineSnapshot` before Combat actor hydration. | `ProgressionSaveBarrier` and current whitelisted Character Progression state on each save trigger. | Character Progression owns schema, validation, permanent baseline computation, and save-eligible stability; Save/Load owns serialisation, HMAC, versioning, load ordering, and failure surfacing. | **Hard** |
| **Inventory & Item Economy** (§9) | Deserialised Inventory state on load (items, currency, faction tokens). | Current Inventory state on each save trigger. | Inventory owns the item schema; Save/Load owns serialisation. | **Hard** |
| **NPC System** (§4) | Deserialised NPC record payload during `Resuming`; `LoadRejected(HydrationFailed)` routing if synchronous NPC hydration returns `NpcHydrationFailed`. | Current NPC-owned `NpcRecord` data on each save trigger; synchronous NPC hydration validation result on load. | NPC System owns the NPC schema and validation; Save/Load owns serialisation, HMAC, versioning, and failure-class routing. | **Hard** |
| **Faction State Simulation** (§15) | Deserialised Faction State on load (reputations, faction control of zones, event-log entries committed). | Current Faction State on each save trigger. | Faction Sim owns the faction-state schema; Save/Load owns serialisation. | **Hard at MVP** (reactive sim) → **Hard at T3** (autonomous between sessions; pairs with WS Rule 13 `SessionResumeEvent` handler in Faction Sim's own GDD) |
| **Faction Reputation** (§16) | Deserialised Faction Reputation per-player-per-faction state on load. | Current Faction Reputation state on each save trigger. | Faction Reputation owns the reputation schema; Save/Load owns serialisation. | **Hard** |

#### Indirect interactions (not a direct persistence client)

- **Combat Core** (§7) participates in saves *indirectly* — XP updates flow through Character Progression after Character Progression's `ProgressionSaveBarrier` settles pending kill-credit dispatch; deaths flow through Death & Corpse Recovery → WS's `CorpseRecord`. During load, Combat Core's player actor hydration/build is sequenced after Character Progression validation so Combat can validate or initialize runtime `current_health` and `current_mana` against `ProgressionBaselineSnapshot` health/mana maxima. Combat Core's own direct persistence surface, if any, remains defined by the Combat Core GDD; Character Progression does not persist Combat runtime current resources.
- **Death & Corpse Recovery** (§14) interacts with Save/Load *through* World Structure — WS owns the `CorpseRecord` data contract and delivers it to Save/Load during `SaveCheckpointing`; Death & Corpse Recovery reads `CorpseRecord` back from WS on load, not directly from Save/Load. Corpse penalty resolution is Death & Corpse Recovery's domain.
- **Day/Night Cycle** (§5), **Zone Control** (§17) participate in session resume via WS's Rule 13 `SessionResumeEvent`, not as direct Save/Load clients.

#### Forward-looking (T2+)

- **Companion Relationships** (§21, T2+) — Hard when authored. Relationship state (grudges, preferences, faction allegiances) persists through Save/Load.
- **Network Architecture** (§29, T2+) — Hard when authored. Multi-client save coordination, authority boundaries, and conflict resolution become net-new architecture concerns at T2+ and require ADR-level decisions (save authority client vs. server; conflict resolution on client-server drift).
- **Authentication & Accounts** (§30, T2+) — Hard when authored. Account-linked save identity becomes a persistence-identity concern at T2+.

#### Bidirectional consistency contract

Each downstream GDD, when authored, must declare Save / Load & Persistence in its own §Dependencies with the reverse listing (`depends on: Save / Load & Persistence` — hard/soft matching this table). `/consistency-check` and `/review-all-gdds` verify bidirectional agreement.

## Formulas

Save / Load & Persistence has **no Save/Load-owned formulas** at T1. The operative values governing its behavior are named constants registered in §Tuning Knobs and the entity registry:

- `save_mutex_max_ms` — **owned by** [world-structure.md](world-structure.md) Rule 12; referenced here for the Transition Save timeout contract.
- `save_autosave_interval` — **owned by Save/Load**; value and safe range defined in §Tuning Knobs. Governed behaviorally by Rule 15 (autosave clock reset on any confirmed save), not by a formula.
- `session_catchup_max_real_seconds_default` — owned by [world-structure.md](world-structure.md) Rule 13; Save/Load persists `last_exit_timestamp_utc`, World Structure computes the clamped elapsed delta against this bound.
- `corpse_run_zone_retention_seconds_default` — owned by [world-structure.md](world-structure.md); Save/Load persists `CorpseRecord.expiry_timestamp_utc`.

The `T_save` time budget is **inherited** from [world-structure.md](world-structure.md) **F3** (*Zone Transition Total Time Budget*), where `T_save` is Save/Load's owned slice in the accountability identity `T_transition = T_save + T_load_ms + T_activate + T_unload_async`, bounded as a single measured term by `save_mutex_max_ms`. A sub-decomposition of `T_save` into components (`T_hmac_compute`, `T_serialize`, `T_commit`) is **not warranted at T1** because the storage backend is deferred to ADR and component magnitudes are not yet estimable; revisit if the storage-backend ADR produces reliable per-component budgets.

Migration-chain timing is **deferred to T2+** — at T1 exactly one schema version (v1) exists, so a migration-time formula would sum over an empty set. Revisit when the first v1→v2 migration is authored; the save-integrity rule's per-step fixture test in `tests/fixtures/saves/v[N]/` is the correct enforcement mechanism at that point.

HMAC verification, version-stamp comparison, and queue-depth management are pass/fail checks and state rules, not formulas with numeric outputs. The Rule 13 elapsed-delta computation is owned by World Structure (see [world-structure.md](world-structure.md) Rule 13 + Section G knob entry), not Save/Load.

## Edge Cases

Edge cases are organized by the state space they arise in: save-time (Category A), load-time (Category B), session-boundary (Category C), and cross-system (Category D). Each edge names an exact condition and an exact resolution per Rule 11's fail-loud posture. A Failure-Mode Matrix at the end catalogues every named failure class that the state machine may emit, serving as the authoritative list referenced from Section C's `LoadRejected` entry condition and Menus & Settings row Published column.

### Category A — Save-time edges

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| **A1. Concurrent save triggers — write in flight when new trigger arrives** | Incoming request enters the depth-1 queue, replacing any prior queued request (later state is more current). The in-flight write completes; on `SaveWriteConfirmed` the queued request begins immediately per Rule 6. Queue never grows beyond depth 1. | Coalescing to the latest state is strictly safer than preserving an earlier queued request. Depth-1 cap prevents unbounded deferral. |
| **A2. Write fails mid-operation — disk full, I/O error, or HMAC key-derivation failure during `Writing`** | Save/Load transitions to `WriteFailed`. `SaveFailedEvent` emitted. The pre-existing committed save record is untouched (atomicity guarantee, Rule 7). If the failed trigger was Session-Exit Save, the quit-flow blocks per Rule 13 (Retry / Quit Without Saving dialog). For Transition Save, WS receives `SaveFailedEvent` and routes to `ZoneError` per [world-structure.md](world-structure.md) Edge C4. No silent fallback to prior state. | Atomicity (Rule 7) guarantees the prior intact save is the recovery baseline. Fail-loud (Rule 11) forbids silent continuation. |
| **A3. Power loss mid-write** | On next boot the storage backend finds no valid committed record from the interrupted write. The atomicity mechanism (temp-file-rename or equivalent — ADR-tba) ensures the pre-loss committed save is the last valid record. Load path proceeds with the prior save; HMAC + version checks apply normally. No partial write is observable as a valid save. | The atomicity ADR is the enforcement point. This edge confirms the behavioural guarantee: power loss mid-write = prior intact save survives. NTFS without `FILE_FLAG_WRITE_THROUGH` is a storage-backend-level concern the atomicity ADR must address. |
| **A4. `SaveWriteConfirmed` dispatched after `save_mutex_max_ms` deadline has elapsed** | WS has already fired `SaveTimedOutEvent` and routed to `ZoneError` ([world-structure.md](world-structure.md) Rule 12 + Edge A4). Save/Load's write may have legitimately succeeded — the write is not rolled back. The late `SaveWriteConfirmed` event is dropped by WS (it is already past the mutex window). The save record on disk reflects the completed write; it is valid for the next load. The failed transition is the user-visible outcome, not data loss. | The `save_mutex_max_ms` deadline is a WS-side UX gate, not a Save/Load validity rule. A completed-but-late write is still a good write; the transition failing is the correct consequence of missing the deadline. |
| **A5. Manual Save queued during `ZoneLoading`, then WS exits `ZoneLoading`** | The queued Manual Save is discarded (cancelled by WS). No second `SaveCheckpointing` fires. The Transition Save that ran at `ZoneLoading` entry already captured state. Per [world-structure.md](world-structure.md) Edge A2a and Rule 6 queue-cancellation clause. | Running the queued Manual Save after `ZoneActive` would re-save identical state at best; at worst it would re-save mid-transition state no longer consistent. |
| **A6. Autosave tick fires during active Transition Save in `Writing`** | The autosave tick is suppressed — it does not enqueue a write. Rule 6 treats the tick as implicitly redundant: the Transition Save captures the same state the autosave would. The autosave clock resets from the Transition Save's `SaveWriteConfirmed` per Rule 15. | Two nearly-simultaneous writes of identical state produce no player benefit and risk queue contention. Suppression plus Rule 15 clock-reset achieves the autosave intent without redundancy. |

### Category B — Load-time edges

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| **B1. HMAC mismatch (tampered or corrupted file)** | Load path transitions to `LoadRejected` with failure class `HMACMismatch`. `LoadRejected` event emitted to Menus & Settings. No gameplay-state field is deserialised. System returns to `Idle`. | Rule 4: HMAC verification precedes all deserialisation. Tampered saves never reach the gameplay deserialiser. |
| **B2. Version stamp unreadable or corrupt — integrity check fails before version comparison** | Load path transitions to `LoadRejected` with failure class `VersionStampCorrupt`. Treated identically to HMAC failure: hard rejection, no deserialisation. Rule 3 mandates version stamp as first field; unreadability means even the prefix is suspect — the file cannot be trusted. | If the version stamp cannot be read, migration path selection is impossible; no safe interpretation exists. |
| **B3. Version older than current, migration path available** | Load path enters `Migrating`. Steps execute sequentially (v1→v2→...→current). On success, re-enters `Loading` with the in-memory migrated payload. Migrated payload is NOT written back to disk (Rule 10); it is persisted on the next normal save trigger. | Keeping the load path read-only avoids a second write/failure branch inside `Loading` and preserves the pre-migration file as a fallback until the first successful post-load save. |
| **B4. Version older than current, NO migration path** | Load path transitions to `LoadRejected` with failure class `NoMigrationPath`. Hard rejection; no deserialisation. Rule 10 + Rule 11. | Forward-only migration contract ([.claude/rules/save-integrity.md](../../.claude/rules/save-integrity.md)) requires every version transition to be explicitly specified. An absent path is a developer omission, not an acceptable runtime fallback. |
| **B5. Version newer than current (older loader + newer save)** | Load path transitions to `LoadRejected` with failure class `LoaderTooOld`. Hard rejection; no deserialisation. Older loaders never attempt to partially read newer schemas. | Partial reads of unknown fields produce unpredictable state. Fail-loud is the only safe posture. |
| **B6. File truncated or corrupt (HMAC catches)** | HMAC verification covers the full payload; truncation means the payload does not match the stored HMAC. Outcome: same as B1 — `LoadRejected` with failure class `HMACMismatch`. No separate truncation check is required because HMAC is comprehensive. | B6 resolves as a special case of B1. No extra branch needed provided HMAC is computed over the entire payload. |
| **B7. Migration step fails mid-chain (v1→v2 succeeds, v2→v3 fails)** | `Migrating` state transitions to `LoadRejected` with failure class `MigrationStepFailed`. The in-memory intermediate payload (v2) is discarded; no partial state reaches gameplay systems. The on-disk save file (v1, unmigrated) is untouched — it remains available for recovery. | No partial migrated state is ever delivered downstream. The on-disk file is read-only during load (Rule 10), so the source is always recoverable. |
| **B8. First-run: no save exists (new game / new character)** | Not an error. Load request resolves to the *no-save-to-load* first-run path per Rule 14. System initialises fresh state; the first save trigger (Transition / Manual / Autosave / Session-Exit) writes the initial record. **No event emitted.** | First-run is an expected state, not a failure. Emitting any event for this path would conflate normal first-session bootstrap with a data-loss symptom (which is precisely what B9 `SaveMissing` preserves the distinction for). |
| **B9. File expected but missing (slot previously initialised, file now absent)** | Load path transitions to `LoadRejected` with failure class `SaveMissing`. Emits `LoadRejected` event to Menus & Settings. Hard failure; the system does not fall back to fresh-state initialisation. Rule 14 missing-file failure path. | A missing file in an initialised slot is a data-loss symptom (accidental deletion, platform sync failure). Silent reinitialisation would mask the loss. |
| **B10. Unexpected exception during integrity verification** | `LoadRejected(IntegrityException)` is emitted; no partial load, no silent continuation. The *implementation pattern* that achieves this (try/catch, result-type, typed-error return, etc.) is not locked at the GDD level — Rule 11 locks the behavior, not the mechanism. | The fail-loud contract (Rule 11) requires integrity failures to always produce a surfaced `LoadRejected` outcome. The concrete implementation belongs to the storage-backend ADR and implementation guidance, not this GDD. |
| **B11. Key derivation fails during verify** | Load path transitions to `LoadRejected` with failure class `KeyDerivationFailureVerify`. No HMAC comparison is attempted; the file is not deserialised. Emits `LoadRejected` to Menus & Settings. | Key derivation failure is indistinguishable from a corrupted environment; proceeding without a valid key would produce a spurious HMAC mismatch. Fail-loud is the correct posture. |

### Category C — Session-boundary edges

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| **C1. Session-Exit Save fails — quit-flow blocked** | `WriteFailed` state emits `SaveFailedEvent`. A modal is surfaced (via Menus & Settings if present, directly via the engine's quit-flow hook if Menus is absent) offering **Retry** or **Quit Without Saving**. Retry requeues the Session-Exit Save write. Quit Without Saving exits the application without further save attempt; the last successful save is the recovery baseline. Silent fallback is forbidden. Rule 13. | Stakes Are Honest (P5): the player must never be silently placed in an older session without acknowledgement. The two-option modal preserves agency without bypassing the fail-loud contract. |
| **C2. Uncontrolled termination (OS kill, power loss, process crash)** | No Session-Exit Save runs. On next session start, load path proceeds normally with the last intact committed save. HMAC + version checks apply. If the last committed write was itself partial (power loss mid-write), the atomicity mechanism (Rule 7 + ADR-tba) ensures the pre-loss record is intact. No special recovery mode exists — the normal loader path is the recovery path. | A separate "crash recovery" mode adds a code path that is rarely exercised and easily diverges from the normal load path. Normal-load-as-recovery is simpler and always tested. |
| **C3. `last_exit_timestamp_utc` not yet delivered when `ZoneActiveEvent` attempts to fire** | This is a Rule 9 ordering violation. WS must not publish `ZoneActiveEvent` before receiving `SessionResumeEvent` from `last_exit_timestamp_utc` delivery. Detection: Rule 9 is an ordering *assertion*; verification that the ordering holds at runtime belongs to Section H (Acceptance Criteria). The GDD does not lock a specific handshake mechanism — the implementation must ensure by construction that Save/Load's `Resuming` delivery precedes any `ZoneActiveEvent`. | Rule 9's value is defeated entirely if the ordering can be violated at runtime. A runtime verification AC (in Section H) is the correct enforcement surface; a state-machine handshake is one implementation strategy, not a design-level contract. |
| **C4. Save/Load still in `Loading` or `Migrating` when WS attempts to publish `ZoneActiveEvent`** | WS defers `ZoneActiveEvent` publication until `SessionResumeEvent` arrives (WS-side ordering contract per [world-structure.md](world-structure.md) Rule 13 + H-CR-13b). Save/Load obligation: complete `Loading` / `Migrating` → `Resuming` → deliver `last_exit_timestamp_utc` without blocking indefinitely. **If load fails (any `LoadRejected` class), Save/Load emits `LoadRejected` to Menus & Settings and the session does NOT enter gameplay** — WS never receives `SessionResumeEvent` and therefore does not publish `ZoneActiveEvent`. The rejection surfaces in the Save/Load rejection path (`LoadRejected` → Menus); it is **NOT** routed to WS's `ZoneError`, which is reserved for WS-side zone/transition failures (per [world-structure.md](world-structure.md) Edge B1 / Edge C3). | Any `ZoneActiveEvent` before `SessionResumeEvent` guarantees incorrect catch-up across Rule 13 subscribers. Session-start load failures are a distinct failure mode from WS zone/transition failures; routing them through `ZoneError` would bleed Save/Load's error surface into WS's domain. |
| **C5. Clean shutdown requested while a Transition Save is in flight** | The Session-Exit Save request enters the depth-1 queue behind the in-flight Transition Save. The Transition Save completes first. On `SaveWriteConfirmed`, the queued Session-Exit Save begins immediately. If the Transition Save itself fails to `WriteFailed`, the Session-Exit Save is never queued (the system is already in the Rule 13 blocked-quit flow). The application does not exit until one of: (a) the queued Session-Exit Save confirms, (b) the player selects Quit Without Saving from the modal. | A clean shutdown must not discard in-flight state. The queue semantics of Rule 6 already handle this ordering; Session-Exit Save behaves as a normal queued trigger. |

### Category D — Cross-system edges

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| **D1. WS emits `SaveCheckpointing` while Save/Load is already in `Writing` for a Manual Save** | The Transition Save request enters the depth-1 queue, replacing any prior queued entry. The in-flight Manual Save completes; on `SaveWriteConfirmed` the queued Transition Save begins. Both saves operate within the same WS `SaveCheckpointing` mutex window; the Transition Save's `save_mutex_max_ms` clock is running against the combined latency. If the total combined latency exceeds `save_mutex_max_ms`, WS fires `SaveTimedOutEvent` and routes to `ZoneError` (WS Rule 12) — the Transition Save's queued write may still complete and will produce a valid on-disk record. | The `save_mutex_max_ms` deadline is WS's UX gate. Rule 6's queue semantics remain correct even within the deadline window. Engineers must size Manual Save write latency so back-to-back write + write fits within the window under normal conditions (storage-backend ADR concern). |
| **D2. Menus & Settings absent when `SaveFailedEvent` or `LoadRejected` fires** | `SaveFailedEvent` and `LoadRejected` events are emitted regardless of whether Menus & Settings has a subscriber. If Menus is absent, the events are unhandled by the UI layer. For `SaveFailedEvent` during a normal save (non-exit): the failure is logged; WS handles the consequence (`ZoneError`). For `SaveFailedEvent` during Session-Exit Save: the in-app quit-flow hook (engine-level, not Menus-dependent) must surface the Retry / Quit Without Saving dialog. Save/Load's Interactions table marks Menus as **Soft** — the event contract survives Menus being absent; the UI degrades. | Soft classification means the save system's correctness does not depend on any single UI subscriber being present. The Session-Exit Save modal is a special case requiring an engine-level fallback that does not route through Menus. |
| **D3. Downstream system fails to receive deserialised state during `Resuming`** | Save/Load's `Resuming` delivers deserialised state to each downstream system's hydration entry point. **If a synchronous hydration call throws or returns an error during `Resuming` — detected before gameplay is enabled** — Save/Load transitions from `Resuming` to `LoadRejected(HydrationFailed)`. **Load aborts before gameplay is enabled — no partially hydrated session becomes playable; `ZoneActiveEvent` is never produced.** Save/Load does **not** claim transactional rollback of hydration calls that completed before the failure was detected — some downstream systems may hold deserialised state from the aborted sequence at the moment of rejection. Cleanup semantics for any received-but-unused state are owned by the affected downstream systems (or by implementation-level decisions in the storage-backend ADR), not by Save/Load at the GDD level. Arbitrary runtime faults that occur AFTER gameplay enable are outside this edge's scope — each downstream system owns its own runtime-fault handling once play has started. | Save/Load still owns the load chain during `Resuming`; Rule 11 forbids gameplay enablement on partial state. Preventing the session from becoming playable is sufficient to satisfy Rule 11; full hydration rollback would require transactional semantics that are implementation/ADR territory, not a GDD-level lock. |
| **D4. Faction Sim's Rule 13 catch-up handler requires Faction State hydrated before it can run, but Faction State has not yet hydrated when `SessionResumeEvent` fires** | Save/Load guarantees only one ordering: `last_exit_timestamp_utc` delivery (enabling `SessionResumeEvent`) fires before `ZoneActiveEvent`. The ordering between `SessionResumeEvent` dispatch and Faction State hydration is **not** locked by Save/Load — Faction Sim's own GDD must declare the dependency and Save/Load's `Resuming` state must satisfy it when that GDD is authored. At T1 MVP, Faction Sim's catch-up handler is reactive-only (no between-session autonomy); if Faction State hydration is required before the handler runs, Faction Sim declares that requirement and `Resuming` sequences accordingly. Open resolution: deferred to Faction Sim GDD. | Rule 9 (narrowed) deliberately does not over-specify subscriber ordering. Downstream GDDs are the correct specification point for their own hydration prerequisites. |

### Failure-Mode Matrix

The matrix is the authoritative catalogue of named failure classes Save/Load may emit. Section C's `LoadRejected` state entry condition and the Menus & Settings row Published column reference this matrix non-exhaustively; when a new class is added, only the matrix needs updating — the Section C surfaces point at this table rather than re-enumerating it.

| Failure Class | Detection Point | Action | Event / Outcome | Recoverable? |
|---|---|---|---|---|
| `HMACMismatch` | `Loading` — HMAC verify step, before any field deserialisation | Transition to `LoadRejected` | `LoadRejected(HMACMismatch)` → Menus & Settings | No — file is untrusted; player must delete or restore from backup |
| `VersionStampCorrupt` | `Loading` — version stamp read, before HMAC | Transition to `LoadRejected` | `LoadRejected(VersionStampCorrupt)` → Menus & Settings | No — prefix unreadable; equivalent to corrupt file |
| `LoaderTooOld` | `Loading` — version stamp > current loader version | Transition to `LoadRejected` | `LoadRejected(LoaderTooOld)` → Menus & Settings | Yes — player must use the newer game version that wrote the save |
| `NoMigrationPath` | `Loading` — version stamp < current, no migration chain registered | Transition to `LoadRejected` | `LoadRejected(NoMigrationPath)` → Menus & Settings | No — developer must ship migration; no runtime recovery |
| `MigrationStepFailed` | `Migrating` — a sequential migration step returns failure | Transition to `LoadRejected`; on-disk file untouched | `LoadRejected(MigrationStepFailed)` → Menus & Settings | Partial — source file intact; recovery requires developer fix + re-run |
| `SaveMissing` | `Loading` — file absent at expected path, slot previously initialised (Rule 14) | Transition to `LoadRejected` | `LoadRejected(SaveMissing)` → Menus & Settings | Situational — if file is recoverable from backup or platform sync |
| `IntegrityException` | `Loading` — unexpected exception thrown inside integrity-verification step | Transition to `LoadRejected` unconditionally (implementation pattern is not GDD-locked; see B10) | `LoadRejected(IntegrityException)` → Menus & Settings | Unknown — exception source must be diagnosed; do not retry without investigation |
| `KeyDerivationFailureVerify` | `Loading` — key derivation call fails before HMAC comparison | Transition to `LoadRejected` | `LoadRejected(KeyDerivationFailureVerify)` → Menus & Settings | Situational — environment may be recoverable (reinstall, platform keystore repair) |
| `WriteIOError` | `Writing` — I/O error during serialize / commit step | Transition to `WriteFailed`; prior save intact | `SaveFailedEvent` → caller; if Session-Exit Save: Rule 13 modal | Yes — prior intact save is recovery baseline |
| `DiskFull` | `Writing` — OS reports insufficient disk space ([world-structure.md](world-structure.md) Edge C4) | Transition to `WriteFailed`; prior save intact | `SaveFailedEvent` → WS (`ZoneError`); or Rule 13 modal if Session-Exit | Yes — free disk space; retry |
| `KeyDerivationFailureWrite` | `Writing` — key derivation call fails before HMAC computation | Transition to `WriteFailed`; no bytes written | `SaveFailedEvent`; prior save intact | Situational — environment issue; same recovery as `KeyDerivationFailureVerify` |
| `AtomicityFailure` | `Writing` — atomicity mechanism detects partial commit (on next read of tmp record) | Treated as `WriteIOError`; prior committed save is intact | `SaveFailedEvent`; prior save intact | Yes — prior intact save is baseline; atomicity ADR must guarantee this |
| `PowerLossMidWrite` | Detected on next boot by atomicity mechanism (no in-process detection possible) | Normal load path; prior committed save used; no special recovery state | Normal `Loading` sequence against prior intact save | Yes — prior intact save is baseline; atomicity ADR must guarantee this for NTFS + APFS |
| `HydrationFailed` | `Resuming` — synchronous downstream hydration call throws or returns error before gameplay is enabled | Transition to `LoadRejected` | `LoadRejected(HydrationFailed)` → Menus & Settings; no playable session produced (load aborts before gameplay enable; some downstream systems may hold partial deserialised state from the aborted sequence — their obligation to discard, not Save/Load's) | Situational — likely save-schema / downstream-system schema drift; needs developer investigation |

### ADR candidates surfaced by §Edge Cases

No new ADR-tba items are surfaced beyond the three already flagged in §Overview and §Detailed Design:

1. **Storage backend selection** — SQLite / JSON-on-disk / binary / hybrid.
2. **Atomicity mechanism + power-loss model** — A3 / C2 / B6 / B7 confirm the behavioural requirement; mechanism is ADR-tba. Must address NTFS without `FILE_FLAG_WRITE_THROUGH` and APFS.
3. **Save event dispatch semantics** — A4 confirms `SaveWriteConfirmed` must dispatch synchronously same-frame with commit decision; the event-bus choice must satisfy this.

Edge D2 surfaces an implementation constraint (Session-Exit Save modal must have an engine-level fallback independent of Menus & Settings) but this is a detail of the storage-backend or application-lifecycle ADR rather than a new top-level ADR topic.

## Dependencies

### Upstream

**None.** Save / Load & Persistence is Layer 1 Foundation per [systems-index.md](systems-index.md) §Dependency Map — it has no upstream game-system dependencies. The canonical row at [systems-index.md](systems-index.md) line 30 lists *"Depends On = —"*, and the Foundation layer notes that Layer 1 systems have no dependencies on other game systems.

**Contract inputs are distinct from system-graph dependencies.** This GDD reproduces three locked upstream *contracts* (see §Locked Inputs): the save-integrity rule ([.claude/rules/save-integrity.md](../../.claude/rules/save-integrity.md)) and two items from [world-structure.md](world-structure.md) — the save-on-transition interface (Rule 8 / Rule 12) and the Rule 13 `SessionResumeEvent` ordering (Rule 9 ordering lock). Those contracts **shape** Save/Load's interface surface but do **not** make World Structure or the save-integrity rule upstream graph dependencies. Save/Load and World Structure are peer Layer-1 Foundation systems that interface **bidirectionally**; the save-integrity rule is a project-level policy, not a game system. Both Save/Load and World Structure can be designed and implemented independently as long as each honors the other's published interface.

### Downstream — direct persistence clients

Systems that read/write save data through Save/Load. This table mirrors the §Interactions with Other Systems table in §Detailed Design — interface specifics (event payloads, data contracts) live there. This section records direction, nature, and hard/soft classification for `/consistency-check` and `/review-all-gdds`.

| System | Direction | Nature / Data Interface | Hard/Soft | Interface Owner |
|---|---|---|---|---|
| **World Structure** (§1) | Peer (bidirectional) | WS drives Transition Save via `SaveCheckpointing`; WS provides `PlayerZoneMembership`, `ZoneTransitionTimestamp`, `last_exit_timestamp_utc`, `CorpseRecord` for persistence; Save/Load returns `SaveWriteConfirmed` / `SaveFailedEvent` and delivers `last_exit_timestamp_utc` pre-`ZoneActiveEvent` per Rule 9. | **Hard** | WS owns state-machine trigger + data shape; Save/Load owns serialisation, HMAC, versioning, atomicity, outcome events. |
| **Menus & Settings** (§3) | S/L depended on by | Manual Save trigger routing; status events for UI (`SaveInProgress`, `SaveWriteConfirmed`, `SaveFailedEvent`, `LoadRejected` carrying failure class per §Edge Cases failure-mode matrix); Session-Exit Save failure acknowledgement flow per Rule 13. | **Soft** (UI degrades gracefully if missing; engine-level fallback required for the Session-Exit Save modal per §Edge Cases D2) | Menus owns UI + quit-flow dialog; Save/Load owns event contract and Rule 13 quit-flow gating. |
| **Character Creation** (§6) | S/L depended on by | Initial character record on first save; deserialised Player State on load. | **Hard** | Character Creation owns character-record schema; Save/Load owns persistence. |
| **Character Progression** (§8) | S/L depended on by | `ProgressionSaveBarrier`; whitelisted progression state (`progression_schema_version`, `class_id`, `current_level`, `total_xp`, `spell_eligibility_tier`) persisted and hydrated; `ProgressionBaselineSnapshot` produced before Combat actor hydration. | **Hard** | Character Progression owns schema, validation, permanent baseline computation, and save-eligible stability; Save/Load owns serialisation, HMAC, versioning, load ordering, and failure surfacing. |
| **Inventory & Item Economy** (§9) | S/L depended on by | Items, currency, faction tokens — persisted and hydrated. | **Hard** | Inventory owns item schema; Save/Load owns serialisation. |
| **NPC System** (§4) | S/L depended on by | NPC-owned `NpcRecord` data — persisted and hydrated; invalid NPC hydration maps to `LoadRejected(HydrationFailed)`. | **Hard** | NPC System owns NPC schema and validation; Save/Load owns serialisation, HMAC, versioning, and failure-class routing. |
| **Faction State Simulation** (§15) | S/L depended on by | Reputations, faction control of zones, committed event-log entries. | **Hard at MVP** (reactive sim) → **Hard at T3** (autonomous between sessions; pairs with WS Rule 13 `SessionResumeEvent` handler in Faction Sim's GDD) | Faction Sim owns schema; Save/Load owns serialisation. |
| **Faction Reputation** (§16) | S/L depended on by | Per-player per-faction rep state. | **Hard** | Faction Reputation owns schema; Save/Load owns serialisation. |

### Indirect dependents — graph-listed (covered by bidirectional consistency)

The following systems are named in [systems-index.md](systems-index.md) as graph dependents of Save / Load & Persistence, but their persistence interaction is *indirect* (via other systems' state rather than directly through Save/Load's event contract). They **are** covered by the bidirectional-consistency contract below.

- **Combat Core** (§7, graph-listed at [systems-index.md:35](systems-index.md)) — **Hard (graph).** Participates indirectly via Character Progression (settled XP updates and progression-derived permanent health/mana maxima for load hydration) and Death & Corpse Recovery (→ WS's `CorpseRecord`). Save writes consult Character Progression's `ProgressionSaveBarrier` before serializing XP state; Combat actor hydration/build occurs after Character Progression publishes `ProgressionBaselineSnapshot`; Combat Core's own direct persistence surface, if any, remains defined in its own GDD.
- **Death & Corpse Recovery** (§14, graph-listed at [systems-index.md:42](systems-index.md)) — **Hard (graph).** Interacts through World Structure (WS owns the `CorpseRecord` data contract; D&CR reads it back via WS, not directly from Save/Load).

### Indirect interactions — not graph dependents (reference only)

The following systems interact with Save/Load only via World Structure's Rule 13 `SessionResumeEvent`. They are **not** systems-graph dependents of Save/Load (they depend on World Structure in the graph), and therefore **do not** fall under the bidirectional-consistency contract for this GDD. Their reverse-listing obligation is against [world-structure.md](world-structure.md), not Save/Load.

- **Day/Night Cycle** (§5), **Zone Control** (§17) — participate in session resume via WS's Rule 13 `SessionResumeEvent`.

### Forward-looking dependents (T2+, not yet authored)

- **Companion Relationships** (§21, T2+) — **Hard when authored.** Relationship state (grudges, preferences, faction allegiances) persists through Save/Load.
- **Network Architecture** (§29, T2+) — **Hard when authored.** Multi-client save coordination, authority boundaries, and conflict resolution become net-new architecture concerns at T2+ requiring ADR-level decisions (save authority client vs. server; client-server drift resolution).
- **Authentication & Accounts** (§30, T2+) — **Hard when authored.** Account-linked save identity becomes a persistence-identity concern at T2+.

### Bidirectional consistency contract

The reverse-listing obligation applies to every system named as a dependent in this §Dependencies section. It covers three groups:

1. **Direct persistence clients** (the 7-row table above, including the peer bidirectional relationship with World Structure).
2. **Indirect graph-listed dependents**: Combat Core and Death & Corpse Recovery, listed as Save/Load graph dependents in [systems-index.md:35](systems-index.md) and [systems-index.md:42](systems-index.md).
3. **Forward-looking T2+ dependents**: Companion Relationships, Network Architecture, Authentication & Accounts.

Each such GDD, when authored, must declare Save / Load & Persistence in its own §Dependencies with the reverse listing (`depends on: Save / Load & Persistence` — hard/soft matching this section's classification). `/consistency-check` and `/review-all-gdds` verify bidirectional agreement. Any mismatch = one GDD is wrong and needs amending.

**World Structure's reverse listing is already satisfied:** [world-structure.md](world-structure.md) §Dependencies already lists Save / Load & Persistence as Hard.

**Not covered by this obligation:** Day/Night Cycle, Zone Control (see §Indirect interactions — not graph dependents above).

**Pre-existing reverse-sync debts (follow-up batches, not blocking this GDD):**

- [systems-index.md](systems-index.md) line 30 lists Save/Load's "Depends On" as "—" — correct per §Upstream above; no sync required.
- [systems-index.md](systems-index.md) line 172 (Recommended Design Order) describes Save/Load as *"SQLite schema + character/world state serialization"*. Per §Overview non-negotiable #3, storage backend is deferred to an ADR and is **not** locked to SQLite. Stale description; update in a separate follow-up batch.

## Tuning Knobs

One designer-adjustable value drives Save/Load's runtime behavior. Per the §Formulas boundary: `save_autosave_interval` is Save/Load-owned; other values referenced in this GDD (`save_mutex_max_ms`, `session_catchup_max_real_seconds`, `corpse_run_zone_retention_seconds`) are owned elsewhere and appear only as cross-references below.

| Parameter | Current Value | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|--------------|------------|-------------------|-------------------|
| `save_autosave_interval` — Rule 5 Autosave Tick cadence; governed by Rule 15 clock-reset on any confirmed save | **300 s (5 min)** | 60 s – 600 s | Weaker safety-net coverage between manual / transition / session-exit saves — longer windows where a crash loses progress not captured by a natural save. Above 600 s risks meaningful progress loss in typical crash scenarios. | More frequent autosave writes. Rule 15 clock-reset + Rule 6 suppression-during-Transition-Save mitigate redundant overlap, but tighter intervals increase the chance autosave + transition-save back-to-back fill the depth-1 mutex queue in sessions with frequent zone transitions. Below 60 s adds I/O pressure with diminishing player-safety benefit and risks contention with the `save_mutex_max_ms` window on min-spec HDD. |

> **`save_autosave_interval` initial value rationale:** Gravenspire sessions are modelled on EQ-classic pacing with 60–180 minute session length (per [game-concept.md](game-concept.md)). The autosave serves crash protection, not pacing — natural save points defined by Rule 5 (Transition Save on zone entry, Session-Exit Save on clean quit, Manual Save at player's discretion) already cover intentional save moments. 300 s provides a ≤5 minute crash window at worst and splits the 60–600 s safe range cleanly. Revisit during T1 playtest once session cadence and crash-window tolerance are observed.

### Tuning-knob interactions

This system exposes only one Save/Load-owned knob; there are no knob-to-knob interactions internal to Save/Load. Interactions with rules and state are:

- **`save_autosave_interval` × Rule 15 clock reset** — The autosave clock resets on any `SaveWriteConfirmed`. In a session with frequent zone transitions (interval ≤ `save_autosave_interval` between Transition Saves), the autosave may rarely fire; this is intended behavior — the autosave is a safety-net for sessions with long intervals between natural saves, not a pacing mechanism.
- **`save_autosave_interval` × Rule 6 mutex queue** — The depth-1 mutex queue + Rule 6 autosave-suppression-during-active-Transition-Save means autosave ticks that fire during transitions are discarded (not queued). This protects against saturating the queue during high-transition-density play but means the autosave's effective cadence is conditional on session rhythm.
- **`save_autosave_interval` × `save_mutex_max_ms`** — Not directly interacting. Per Rule 5, only the Transition-Save path is bounded by `save_mutex_max_ms` ([world-structure.md](world-structure.md) Rule 12). Autosave latency is bounded by caller UX needs, not Rule 12.

### Cross-reference — knobs owned elsewhere that affect this system

- **`save_mutex_max_ms`** — owned by [world-structure.md](world-structure.md) Rule 12 + §Tuning Knobs (value 150 ms, safe range 100–500 ms). Bounds Transition Save latency from WS's perspective. Not a Save/Load knob — Save/Load must be engineered to complete within this window under normal conditions (Rule 8 + §Edge Cases A4).
- **`session_catchup_max_real_seconds`** — owned by [world-structure.md](world-structure.md) Rule 13 + §Tuning Knobs (value 604 800 s / 7 days; registry constant `session_catchup_max_real_seconds_default`). Governs the Rule 13 elapsed-delta clamp; Save/Load persists `last_exit_timestamp_utc` and WS computes the clamped delta.
- **`corpse_run_zone_retention_seconds`** — owned by [world-structure.md](world-structure.md) §Tuning Knobs (value 300 s; registry constant `corpse_run_zone_retention_seconds_default`). Governs `CorpseRecord.expiry_timestamp_utc` window; Save/Load persists the timestamp, Death & Corpse Recovery resolves expiry.

## Visual/Audio Requirements

Save / Load & Persistence is infrastructure and owns **no primary audiovisual surface**. Player-facing presentation for save/load events is owned by Menus & Settings (in-game UI) and the engine-level quit-flow hook (when Menus is absent per §Edge Cases D2). Any audio cues for save success/failure are UX-design concerns belonging to Menus & Settings' audio spec, not this GDD. The save file itself has no visual representation in gameplay; corpse-run visuals (which depend on the persisted `CorpseRecord`) are owned by Death & Corpse Recovery + the Camera system per [world-structure.md](world-structure.md) §Visual/Audio Requirements. **No `/asset-spec` is required for Save/Load at T1.**

## UI Requirements

Save / Load & Persistence owns **no primary UI**. Three UI affordances trigger via events published to Menus & Settings (with engine-level quit-flow fallback when Menus is absent):

| Information | Display Location | Update Frequency | Condition | Owner |
|---|---|---|---|---|
| Save-in-progress indicator (optional; often invisible given `save_mutex_max_ms` target) | Menus & Settings UI surface (low-opacity, non-interrupting) | During `Writing` state | Any `SaveInProgress` event; hidden on `SaveWriteConfirmed` / `SaveFailedEvent` | Menus & Settings |
| `LoadRejected` error presentation carrying a distinct failure class | Screen-centre modal (or equivalent attention-grabbing UI) | Once, on entry to `LoadRejected` state | Loud rejection per §Edge Cases failure-mode matrix | Menus & Settings |
| Session-Exit Save failure modal — Retry / Quit Without Saving | Modal blocking the quit flow | Once, on Session-Exit Save `SaveFailedEvent` | Rule 13 blocked-shutdown contract; **engine-level quit-flow hook provides fallback when Menus is absent** per §Edge Cases D2 | Menus & Settings (+ engine fallback) |

> **📌 UX Flag — Save / Load & Persistence**: these three UI surfaces belong in Menus & Settings' UX spec when it is authored. Run `/ux-design` for each surface **before** writing epics for Menus & Settings. Stories referencing these elements should cite `design/ux/[screen].md`, not this GDD directly.

## Acceptance Criteria

Testable conditions that prove Save / Load & Persistence works as designed. Every criterion uses **Given-When-Then** format and is independently verifiable by a QA tester with no GDD context. Organised by Rule coverage (Rules 1–15, minus Rule 11 which is satisfied indirectly by every failure-mode AC), Failure-Mode Matrix coverage (§Edge Cases), Cross-system interface integrity, and Performance + profiling. Summary table at the end.

### T1 Test-Type Taxonomy (adopted from world-structure.md)

This GDD adopts the 5-category test taxonomy authored in [world-structure.md](world-structure.md) §Acceptance Criteria (T1 Test-Type Taxonomy, WS-local per the WS round-2 follow-up binding — the promotion-deferral note in world-structure.md §Acceptance Criteria flags that the taxonomy promotes to `.claude/rules/test-standards.md` once Save/Load adopts it). Save/Load's adoption completes that "Save/Load GDD adopts consistently" condition; a follow-up batch will promote the taxonomy project-wide.

| Category | Definition | Evidence destination |
|---|---|---|
| **Editor-validation** | Unity Editor menu script, import-time check, Roslyn analyzer, or `IPreprocessBuildWithReport` callback. Runs in Editor without PlayMode. | Console log → `production/qa/evidence/save-load/` |
| **Dev-build smoke** | PlayMode session against a Development build. Manual or scripted run. | Screenshot / log file / fixture output → `production/qa/evidence/save-load/` |
| **Profiled playtest** | Manual run on [Min-Spec Profile](world-structure.md#min-spec-profile-profiling-target-for-measured-variables) hardware with Unity Profiler. | `.data` file + screenshot |
| **Unit** | Unity Test Framework NUnit test. Deterministic, isolated, no scene dependency. | UTF green/red |
| **Integration** | Unity Test Framework PlayMode test. Runs in scene context. | UTF green/red |

**T1 is local-gate only** per [AGENTS.md](../../AGENTS.md) §6 — no CI. Where an AC would promote to CI-level at T2+, the row is annotated inline.

### Core Rule coverage

**H-SL-R1 — Save state categories (Rule 1)**
**GIVEN** an in-memory game state populated with all four categories, **WHEN** a Transition Save fires and the serialised fixture is inspected, **THEN** the fixture contains exactly Player State / World State / Session Metadata / Player-Authored Strings; engine internals, derived/cached values, and runtime handles (per [.claude/rules/save-integrity.md](../../.claude/rules/save-integrity.md) §What Saves exclusion list) are absent.
*Unit (fixture schema assertion) | gameplay-programmer | T1-blocking*

**H-SL-R2 — Single active local character record at T1 (Rule 2)**
**GIVEN** the T1 single-player offline build, **WHEN** the player progresses through normal gameplay flows (character creation, active play, session exit/resume), **THEN** exactly one active local character record exists at any moment and no T1 player-facing flow exposes multi-slot or multi-character selection. The specific save-identity model — including whether a latent/backend slot-enumeration API exists — is deferred to the storage-backend ADR per Rule 2 and is **not** asserted by this test.
*Integration (PlayMode player-facing flow traversal + UI surface inspection) | gameplay-programmer | T1-blocking*

**H-SL-R3 — Version stamp first (Rule 3) — covers FM-VersionStampCorrupt**
**GIVEN** an authored fixture save whose version-stamp prefix is deliberately corrupted, **WHEN** the loader begins a load attempt, **THEN** `LoadRejected(VersionStampCorrupt)` emits with no gameplay-state field deserialised; verification that the version stamp was read before any other field.
*Unit (authored fixture) | gameplay-programmer | T1-blocking*

**H-SL-R4 — HMAC verify before deserialize (Rule 4) — covers FM-HMACMismatch**
**GIVEN** an authored fixture save with a valid version stamp but a tampered payload (HMAC mismatch), **WHEN** the loader begins a load attempt, **THEN** `LoadRejected(HMACMismatch)` emits before any gameplay-state field is deserialised; verification took place between version-check and deserialisation.
*Unit (authored fixture) | gameplay-programmer | T1-blocking*

**H-SL-R5a — Transition Save trigger (Rule 5)**
**GIVEN** the player approaches a zone boundary in gameplay, **WHEN** WS enters `ZoneLoading` and fires `SaveCheckpointing`, **THEN** Save/Load writes within the `save_mutex_max_ms` window and returns `SaveWriteConfirmed`; WS proceeds to `ZoneActive` commit; the save record on disk reflects the pre-transition state.
*Integration (PlayMode with event-log assertion) | engine-programmer + qa-tester | T1-blocking*

**H-SL-R5b — Manual Save trigger (Rule 5)**
**GIVEN** the player is in active gameplay (not in a zone transition), **WHEN** the player invokes Manual Save from the pause menu, **THEN** Menus & Settings routes the trigger; WS enters `SaveCheckpointing`; Save/Load writes; `SaveWriteConfirmed` is returned and the pause-menu UI reflects the successful save.
*Integration (PlayMode with Menus subscriber active) | gameplay-programmer + qa-tester | T1-blocking*

**H-SL-R5c — Autosave Tick trigger (Rule 5)**
**GIVEN** the player is in active gameplay and `save_autosave_interval` elapses since the last successful save, **WHEN** the autosave tick fires (Rule 5 Autosave Tick), **THEN** Save/Load enters `Writing`; on success, `SaveWriteConfirmed` emits and the autosave clock resets per Rule 15.
*Integration (PlayMode with advanced-clock test harness) | gameplay-programmer + qa-tester | T1-blocking*

**H-SL-R5d — Session-Exit Save trigger (Rule 5)**
**GIVEN** the player invokes a clean in-app quit, **WHEN** the application receives the shutdown signal, **THEN** Session-Exit Save fires; Save/Load writes; on `SaveWriteConfirmed` the application proceeds with shutdown; the save record on disk reflects the final gameplay state.
*Integration (PlayMode with scripted clean-quit) | gameplay-programmer + qa-tester | T1-blocking*

**H-SL-R6 — Mutex queue semantics (Rule 6)**
**GIVEN** a write is in progress in `Writing` state, **WHEN** additional triggers arrive during the write, **THEN** (a) exactly one queued request is retained (depth-1); (b) a newer queued request replaces an older queued request (latest-state wins); (c) a Manual Save queued during WS's `ZoneLoading` state is discarded when WS exits `ZoneLoading` to `ZoneActive` or `ZoneError` per Edge A2a; (d) an Autosave Tick that fires during an active Transition Save is suppressed (does not enqueue).
*Integration (PlayMode with scripted concurrent-trigger harness) | gameplay-programmer + qa-tester | T1-blocking*

**H-SL-R7 — Atomic write guarantee (Rule 7)**
**GIVEN** the deterministic write-failure injection seam approved by the storage-backend ADR (see §Open Questions — "Save atomicity mechanism + power-loss model"), **WHEN** the write is interrupted mid-commit during `Writing`, **THEN** on next session start the loader reads the prior-committed save intact; no partial or invalid save is observable at the filesystem level; the atomicity mechanism (ADR-defined) restored the prior state.
*Dev-build smoke (ADR-approved fault-injection seam) | engine-programmer | T1-blocking (atomicity ADR is T1-blocking before save-system implementation)*

**H-SL-R8 — SaveWriteConfirmed semantics + dispatch (Rule 8)**
**GIVEN** a write completes successfully in `Writing`, **WHEN** the event-bus dispatch timing is instrumented, **THEN** `SaveWriteConfirmed` is dispatched synchronously on the same call frame as the commit decision (not queued for end-of-frame); the event signals "write submitted and OS/filesystem-acknowledged at the application boundary" per Rule 8 semantics, not physical-media durability.
*Integration (PlayMode with frame-level event-bus instrumentation) | engine-programmer + qa-tester | T1-blocking*

**H-SL-R9 — Session resume sequencing (Rule 9)**
**GIVEN** a session-load sequence from a valid save fixture with `last_exit_timestamp_utc` set, **WHEN** the load path completes through `Loading` → `Resuming` and Save/Load delivers `last_exit_timestamp_utc` to WS, **THEN** event-log assertion confirms that Save/Load's delivery to WS occurs before any `ZoneActiveEvent` is published; this is the Save/Load-side obligation — pairs with [world-structure.md](world-structure.md) **H-CR-13b** which verifies the WS-side observable (a synthetic subscriber sees `SessionResumeEvent` before `ZoneActiveEvent`). Together the two ACs close the bilateral ordering contract.
*Integration (PlayMode with event-log assertion) | engine-programmer + qa-tester | T1-blocking*

**H-SL-R10a — Migration behaviour at load (Rule 10)**
**GIVEN** an authored v1-era fixture save in `tests/fixtures/saves/v1/` (assumes a v1→v2 migration registered), **WHEN** the loader processes the fixture and enters `Migrating`, **THEN** (a) the in-memory payload hydrates as v2-current; (b) the on-disk fixture is unchanged (v1); (c) gameplay begins normally.
*Integration (PlayMode with v1 fixture) | gameplay-programmer + qa-tester | **advisory at T1** (vacuous at single schema version; promotes to T1-blocking at first v1→v2 schema bump)*

**H-SL-R10b — Migrated form persists on next save (Rule 10)**
**GIVEN** a successfully loaded v1 fixture that migrated to v2 in memory per H-SL-R10a, **WHEN** the next normal save trigger fires (any of Transition / Manual / Autosave / Session-Exit) and writes successfully, **THEN** the on-disk save is now at v2 — the next load does not re-enter `Migrating`.
*Integration (PlayMode with v1 fixture + forced save trigger) | gameplay-programmer + qa-tester | **advisory at T1** (same rationale as H-SL-R10a)*

**H-SL-R12 — Player-authored string sanitisation (Rule 12)**
**GIVEN** a fixture save whose character-name field contains control characters and exceeds the length cap, **WHEN** the loader deserialises the Player-Authored Strings category, **THEN** the name is sanitised (control characters stripped, length-capped) before being passed to any downstream system; raw control characters never reach Character Creation.
*Unit (authored fixture) | gameplay-programmer | T1-blocking*

**H-SL-R13 — Session-Exit Save failure blocks shutdown (Rule 13)**
**GIVEN** the player invokes a clean in-app quit and the Session-Exit Save is forced to fail via fault-injection seam, **WHEN** `SaveFailedEvent` emits, **THEN** (a) the in-app quit flow is blocked; (b) a modal surfaces offering **Retry** and **Quit Without Saving**; (c) Retry re-queues the Session-Exit Save; (d) Quit Without Saving exits without further save attempt. No silent fallback to last successful save.
*Integration (PlayMode with forced Session-Exit write failure) | gameplay-programmer + qa-tester | T1-blocking*

**H-SL-R14a — First-run path (Rule 14)**
**GIVEN** no save file at the expected location AND the slot is not initialised per the backend's status mechanism (the specific mechanism — marker file, metadata field, save index entry — is implementation-level per Rule 14 + storage-backend ADR), **WHEN** the player starts a new character, **THEN** the load request resolves to the *no-save-to-load* first-run path; no `LoadRejected` event emitted; the system initialises fresh state; the first save trigger (any of Rule 5's four) writes the initial record.
*Integration (clean install on ADR-chosen backend) | qa-tester | T1-blocking*

**H-SL-R14b — Missing-file failure (Rule 14) — covers FM-SaveMissing**
**GIVEN** a slot previously initialised (the backend's status mechanism marks the slot as initialised AND a prior successful save was recorded) whose save file is now absent at the expected location, **WHEN** the loader attempts to load, **THEN** `LoadRejected(SaveMissing)` emits; the system does NOT fall back to fresh-state initialisation; Menus & Settings surfaces the distinct `SaveMissing` failure class.
*Integration (PlayMode with tooled save-file removal) | gameplay-programmer + qa-tester | T1-blocking*

**H-SL-R15 — Autosave clock reset on any confirmed save (Rule 15)**
**GIVEN** the autosave clock is running at time `T`, **WHEN** any of the four save triggers (Transition Save, Manual Save, Autosave Tick, Session-Exit Save) produces `SaveWriteConfirmed` at time `T + n`, **THEN** the next autosave tick is scheduled at `T + n + save_autosave_interval`, not at `T + save_autosave_interval`; any pending autosave that had not yet fired is cancelled. Covers all four trigger types.
*Integration (PlayMode with advanced-clock harness, 4 trigger variants) | gameplay-programmer + qa-tester | T1-blocking*

### Failure-Mode Matrix coverage

Cross-references to Rule ACs above: `HMACMismatch` → H-SL-R4; `VersionStampCorrupt` → H-SL-R3; `SaveMissing` → H-SL-R14b; `PowerLossMidWrite` → H-SL-FM-AtomicityFailure (same harness, same observable — process-kill vs. interrupt trigger but identical post-recovery test).

**H-SL-FM-LoaderTooOld**
**GIVEN** an authored fixture save whose version stamp is greater than the current loader's known-max version, **WHEN** the loader reads the version stamp, **THEN** `LoadRejected(LoaderTooOld)` emits; no payload deserialised; Menus & Settings surfaces the class.
*Unit (authored fixture) | gameplay-programmer | T1-blocking*

**H-SL-FM-NoMigrationPath**
**GIVEN** an authored fixture save at an older version for which no migration path is registered, **WHEN** the loader encounters the version gap, **THEN** `LoadRejected(NoMigrationPath)` emits; no partial load attempted.
*Unit (authored fixture + migration-registry mock) | gameplay-programmer | T1-blocking*

**H-SL-FM-MigrationStepFailed**
**GIVEN** an authored v1 fixture and a migration step registered to throw/error on execution, **WHEN** the loader enters `Migrating` and the failing step runs, **THEN** `LoadRejected(MigrationStepFailed)` emits; intermediate in-memory payload discarded; on-disk v1 file untouched.
*Integration (PlayMode with fault-injected migration step) | gameplay-programmer + qa-tester | **advisory at T1** (same rationale as H-SL-R10a/b)*

**H-SL-FM-IntegrityException**
**GIVEN** a test-double integrity verifier configured to throw an unexpected exception during load, **WHEN** the loader invokes the verifier, **THEN** `LoadRejected(IntegrityException)` emits (not swallowed); no partial load; Menus & Settings surfaces the class. Rule 11 fail-loud behaviour preserved.
*Integration (PlayMode with integrity-verifier test double) | engine-programmer + qa-tester | T1-blocking*

**H-SL-FM-KeyDerivationFailureVerify**
**GIVEN** a test-double key-derivation service configured to fail on verify, **WHEN** the loader attempts HMAC verification, **THEN** `LoadRejected(KeyDerivationFailureVerify)` emits; HMAC comparison not attempted; no deserialisation.
*Unit (key-derivation test double) | gameplay-programmer | T1-blocking*

**H-SL-FM-WriteIOError**
**GIVEN** the deterministic write-failure injection seam (approved by the storage-backend ADR) configured to return an I/O error mid-write, **WHEN** Save/Load is in `Writing` and the I/O error fires, **THEN** state transitions to `WriteFailed`; `SaveFailedEvent` emits; prior committed save intact; caller (WS or Menus or engine-level quit hook) handles consequence.
*Dev-build smoke (ADR-approved fault-injection seam) | engine-programmer | T1-blocking*

**H-SL-FM-DiskFull**
**GIVEN** the deterministic write-failure injection seam (approved by the storage-backend ADR) configured to return an ENOSPC-equivalent failure, **WHEN** Save/Load enters `Writing`, **THEN** state transitions to `WriteFailed`; `SaveFailedEvent` emits; for Transition Save: WS routes to `ZoneError` per [world-structure.md](world-structure.md) Edge C4; for Session-Exit Save: Rule 13 modal surfaces; prior save intact.
*Dev-build smoke (ADR-approved fault-injection seam) | engine-programmer | T1-blocking*

**H-SL-FM-KeyDerivationFailureWrite**
**GIVEN** a test-double key-derivation service configured to fail on write, **WHEN** Save/Load enters `Writing` and attempts HMAC compute, **THEN** `WriteFailed` + `SaveFailedEvent`; no bytes written; prior save intact.
*Unit (key-derivation test double) | gameplay-programmer | T1-blocking*

**H-SL-FM-AtomicityFailure (covers FM-PowerLossMidWrite)**
**GIVEN** the atomicity ADR's approved mechanism and a test scenario that interrupts write commit (process-kill or simulated power-loss), **WHEN** the atomicity mechanism detects a partial commit on the next read (post-recovery), **THEN** the prior committed save is observable; no partial state is observable as valid; the load path proceeds normally against the prior intact save.
*Dev-build smoke (ADR-defined atomicity-verification scenario) | engine-programmer | T1-blocking (atomicity ADR is T1-blocking before save-system implementation)*

**H-SL-FM-HydrationFailed**
**GIVEN** a test-harness downstream system configured to throw on its hydration entry point during `Resuming`, **WHEN** Save/Load invokes the hydration call during `Resuming`, **THEN** state transitions to `LoadRejected` with failure class `HydrationFailed`; no `ZoneActiveEvent` is produced; no playable session enabled per §Edge Cases D3; Menus & Settings surfaces the class.
*Integration (PlayMode with downstream test-harness mock) | gameplay-programmer + qa-tester | T1-blocking*

### Cross-system interface integrity

**H-SL-CS-WS-SavePath — Bidirectional save contract (Hard)**
**GIVEN** a zone transition in active gameplay on [Min-Spec Profile](world-structure.md#min-spec-profile-profiling-target-for-measured-variables) hardware, **WHEN** WS enters `SaveCheckpointing` on `ZoneLoading`, **THEN** Save/Load writes and returns `SaveWriteConfirmed` within `save_mutex_max_ms` (150 ms target on Min-Spec HDD); WS exits the mutex and proceeds to the commit sub-phase. Event-log assertion confirms the bidirectional event sequence (`SaveCheckpointing` → `SaveWriteConfirmed`) without `SaveTimedOutEvent` firing under normal conditions.
*Profiled playtest (Min-Spec Profile) | engine-programmer + qa-tester | T1-blocking*

**H-SL-CS-WS-ResumePath — Bidirectional resume contract (Hard)**
**GIVEN** a session load from a valid save fixture, **WHEN** Save/Load's `Resuming` state delivers `last_exit_timestamp_utc` to WS, **THEN** WS publishes `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` before any `ZoneActiveEvent` fires. Pair with H-SL-R9 (Save/Load-side obligation) and [world-structure.md](world-structure.md) H-CR-13b (WS-side synthetic-subscriber observable).
*Integration (PlayMode with event-log assertion across the WS + Save/Load boundary) | engine-programmer + qa-tester | T1-blocking*

**H-SL-CS-Menus-Absent — Soft degradation (Menus subscriber absent)**
**GIVEN** a test configuration where the Menus & Settings subscriber is removed from the event bus, **WHEN** `SaveFailedEvent` or `LoadRejected` emits during gameplay, **THEN** (a) the event is logged; (b) WS handles the consequence for Transition-Save failures; (c) **for Session-Exit Save failures**, the engine-level quit-flow hook (not Menus-dependent) surfaces the Retry / Quit Without Saving dialog per §Edge Cases D2.
*Integration (PlayMode with Menus subscriber disabled) | engine-programmer + qa-tester | T1-blocking*

**H-SL-CS-Downstream-Hydration — Full-load hydration smoke**
**GIVEN** a successfully loaded save fixture containing Character Progression, Inventory, NPC records, Faction State, and Faction Reputation state, **WHEN** `Resuming` delivers deserialised state to each downstream system, **THEN** each system's post-hydration runtime state matches the pre-save fixture values (programmatic assertion via each downstream system's public accessor); all downstream systems report successful hydration.
*Integration (PlayMode full-load smoke) | gameplay-programmer + qa-tester | T1-blocking*

**H-SL-CS-CharacterProgression-BeforeCombat — Progression baseline precedes Combat hydration**
**GIVEN** a valid save fixture containing Character Progression and Combat player resource state, **WHEN** Save/Load runs `Resuming`, **THEN** Character Progression hydrates and validates first, publishes `ProgressionBaselineSnapshot(current_level, permanent_max_health, permanent_max_mana, spell_eligibility_tier)`, and Combat Core hydrates or builds the player combat actor only after receiving that snapshot's health/mana maxima.
*Integration (PlayMode event-log assertion across Save/Load + Character Progression + Combat Core) | gameplay-programmer + engine-programmer + qa-tester | T1-blocking*

**H-SL-CS-CharacterProgression-SaveBarrier — Pending kill credit settles before serialization**
**GIVEN** a Combat `PlayerKillCreditEvent` and a Manual Save trigger are delivered on the same frame, **WHEN** Save/Load enters `Writing`, **THEN** it invokes Character Progression's `ProgressionSaveBarrier` before reading progression state; the serialized `total_xp`, `current_level`, and `spell_eligibility_tier` match the post-barrier Character Progression state, and no save fixture can contain pre-award XP while runtime gameplay contains post-award XP from that event.
*Integration (PlayMode event-log assertion across Save/Load + Character Progression + Combat Core) | gameplay-programmer + engine-programmer + qa-tester | T1-blocking*

### Performance + profiling

**H-SL-P1-MutexWindow — Transition Save completes within save_mutex_max_ms on Min-Spec Profile**
**GIVEN** a zone transition on Min-Spec Profile (7200 rpm SATA HDD per [world-structure.md Min-Spec Profile](world-structure.md#min-spec-profile-profiling-target-for-measured-variables)), **WHEN** Transition Save runs under normal conditions (no fault injection; no artificial disk load), **THEN** `SaveWriteConfirmed` is dispatched within `save_mutex_max_ms` (150 ms target); the event semantics are "OS/filesystem-acknowledged at the application boundary" per Rule 8, not physical-media durability (fsync-to-platter may complete after the event — see [world-structure.md](world-structure.md) Edge C4 discussion).
*Profiled playtest (Min-Spec Profile, Unity Profiler) | engine-programmer + qa-tester | T1-blocking*

**H-SL-P2-AutosaveLatency — Autosave Tick produces no player-visible frame hitch**
**GIVEN** active gameplay on Min-Spec Profile, **WHEN** the Autosave Tick fires, **THEN** the Unity Profiler frame-time trace shows no autosave-attributable player-visible hitch; the frame-time delta between the autosave-firing frame and adjacent frames is recorded in profiling evidence for engine-programmer review. The AC measures the observable (whether the player experiences a hitch), not a specific numeric frame-budget target — no project-level frame budget is locked in Gravenspire design sources at this point; a future frame-budget decision in a performance ADR or UX spec may tighten this AC.
*Profiled playtest (Unity Profiler frame-time trace) | engine-programmer + qa-tester | T1-blocking*

**H-SL-P3-HMACComputeBudget — HMAC compute latency profiled and recorded**
**GIVEN** the expected T1 save payload size (as profiled), **WHEN** HMAC is computed over the full payload on Min-Spec Profile CPU, **THEN** HMAC compute time is recorded as a Profiled playtest data point and surfaced in the profiling report. The measured value informs engine-programmer review against the inherited `T_save ≤ save_mutex_max_ms` bound (world-structure.md F3 + Rule 12), but no specific numeric alerting threshold is locked at this GDD level — the GDD commits to measurement and reporting, not to a specific ratio.
*Profiled playtest (Min-Spec Profile, Unity Profiler) | engine-programmer + qa-tester | T1-blocking*

### Summary Table

| ID | Covers | Test Type | Owner | T1-Blocking |
|---|---|---|---|---|
| H-SL-R1 | Rule 1 (save state categories) | Unit | gameplay-programmer | Yes |
| H-SL-R2 | Rule 2 (single active record at T1) | Integration | gameplay-programmer | Yes |
| H-SL-R3 | Rule 3 + FM-VersionStampCorrupt | Unit | gameplay-programmer | Yes |
| H-SL-R4 | Rule 4 + FM-HMACMismatch | Unit | gameplay-programmer | Yes |
| H-SL-R5a | Rule 5 Transition Save | Integration | engine-programmer, qa-tester | Yes |
| H-SL-R5b | Rule 5 Manual Save | Integration | gameplay-programmer, qa-tester | Yes |
| H-SL-R5c | Rule 5 Autosave Tick | Integration | gameplay-programmer, qa-tester | Yes |
| H-SL-R5d | Rule 5 Session-Exit Save | Integration | gameplay-programmer, qa-tester | Yes |
| H-SL-R6 | Rule 6 (mutex queue semantics) | Integration | gameplay-programmer, qa-tester | Yes |
| H-SL-R7 | Rule 7 (atomic write) | Dev-build smoke | engine-programmer | Yes (atomicity ADR T1-blocking) |
| H-SL-R8 | Rule 8 (SaveWriteConfirmed semantics + dispatch) | Integration | engine-programmer, qa-tester | Yes |
| H-SL-R9 | Rule 9 (session resume ordering) | Integration | engine-programmer, qa-tester | Yes |
| H-SL-R10a | Rule 10 (migration behaviour) | Integration | gameplay-programmer, qa-tester | **advisory at T1** |
| H-SL-R10b | Rule 10 (migrated form persists on next save) | Integration | gameplay-programmer, qa-tester | **advisory at T1** |
| H-SL-R12 | Rule 12 (player-authored string sanitisation) | Unit | gameplay-programmer | Yes |
| H-SL-R13 | Rule 13 (Session-Exit Save failure blocks shutdown) | Integration | gameplay-programmer, qa-tester | Yes |
| H-SL-R14a | Rule 14 first-run path | Integration | qa-tester | Yes |
| H-SL-R14b | Rule 14 missing-file + FM-SaveMissing | Integration | gameplay-programmer, qa-tester | Yes |
| H-SL-R15 | Rule 15 (autosave clock reset) | Integration | gameplay-programmer, qa-tester | Yes |
| H-SL-FM-LoaderTooOld | Failure class | Unit | gameplay-programmer | Yes |
| H-SL-FM-NoMigrationPath | Failure class | Unit | gameplay-programmer | Yes |
| H-SL-FM-MigrationStepFailed | Failure class | Integration | gameplay-programmer, qa-tester | **advisory at T1** |
| H-SL-FM-IntegrityException | Failure class | Integration | engine-programmer, qa-tester | Yes |
| H-SL-FM-KeyDerivationFailureVerify | Failure class | Unit | gameplay-programmer | Yes |
| H-SL-FM-WriteIOError | Failure class (ADR-approved seam) | Dev-build smoke | engine-programmer | Yes |
| H-SL-FM-DiskFull | Failure class (ADR-approved seam) | Dev-build smoke | engine-programmer | Yes |
| H-SL-FM-KeyDerivationFailureWrite | Failure class | Unit | gameplay-programmer | Yes |
| H-SL-FM-AtomicityFailure (+ PowerLossMidWrite) | Failure class (atomicity ADR) | Dev-build smoke | engine-programmer | Yes |
| H-SL-FM-HydrationFailed | Failure class | Integration | gameplay-programmer, qa-tester | Yes |
| H-SL-CS-WS-SavePath | WS bidirectional save contract | Profiled playtest | engine-programmer, qa-tester | Yes |
| H-SL-CS-WS-ResumePath | WS bidirectional resume contract | Integration | engine-programmer, qa-tester | Yes |
| H-SL-CS-Menus-Absent | Menus degradation (Soft) | Integration | engine-programmer, qa-tester | Yes |
| H-SL-CS-Downstream-Hydration | Full-load hydration smoke | Integration | gameplay-programmer, qa-tester | Yes |
| H-SL-CS-CharacterProgression-BeforeCombat | Character Progression baseline before Combat hydration | Integration | gameplay-programmer, engine-programmer, qa-tester | Yes |
| H-SL-CS-CharacterProgression-SaveBarrier | Character Progression pending XP save barrier | Integration | gameplay-programmer, engine-programmer, qa-tester | Yes |
| H-SL-P1-MutexWindow | Transition Save latency on HDD | Profiled playtest | engine-programmer, qa-tester | Yes |
| H-SL-P2-AutosaveLatency | Autosave frame-hitch check | Profiled playtest | engine-programmer, qa-tester | Yes |
| H-SL-P3-HMACComputeBudget | HMAC compute latency | Profiled playtest | engine-programmer, qa-tester | Yes |

**Total: 38 criteria. 35 T1-blocking, 3 advisory-at-T1 (H-SL-R10a, H-SL-R10b, H-SL-FM-MigrationStepFailed — all promote to T1-blocking at first v1→v2 schema bump).**

### Explicit non-criteria (out of this GDD's scope)

- **Storage-backend-specific ACs** (file format, on-disk schema, transaction semantics) — belong in the storage-backend ADR-tba, not this GDD.
- **Atomicity mechanism ACs** (temp-rename vs. WAL vs. two-slot rotation; NTFS-specific `FILE_FLAG_WRITE_THROUGH` behaviour) — belong in the atomicity ADR-tba. **The atomicity ADR is T1-blocking before save-system implementation** (see §Open Questions).
- **HMAC key derivation strategy verification** — key-derivation ADR-tba (per [.claude/rules/save-integrity.md](../../.claude/rules/save-integrity.md)); GDD locks only that the key is per-install derived, not the specific derivation mechanism.
- **Rule 11 (fail-loud) dedicated meta-AC** — deliberately omitted. Rule 11 is a meta-rule satisfied by every failure-mode AC in aggregate; a dedicated Roslyn/code-scan AC would drag implementation-pattern policy into the GDD.
- **Downstream-system schema ACs** — each downstream GDD owns its own schema verification (Character Creation, Character Progression, Inventory, Faction State Simulation, Faction Reputation — each authors its own ACs covering the save-round-trip of its schema).
- **T2+ networked save coordination** — deferred to Network Architecture GDD when authored at T2+.
- **T3+ LLM dialogue memory persistence** — deferred per [DECISIONS.md](../../DECISIONS.md) D004.

## Open Questions

Consolidates ADR-tba items surfaced across this GDD, grouped by T1-readiness.

### T1-blocking before save-system implementation

These ADRs must resolve before any Save/Load implementation work begins. Section H's T1-blocking ACs depend on the resolutions below.

| Question | Owner | Deadline | Status |
|---|---|---|---|
| **ADR-tba — Storage backend selection.** SQLite / JSON-on-disk / binary / hybrid. The GDD specifies what/when/guarantees/failure modes; the ADR selects the mechanism that delivers them at Unity 6.3 LTS + C# .NET 8+. Affects Rule 2 save-identity model, Rule 7 atomicity implementation, Rule 8 dispatch semantics, Rule 14 slot-initialised status mechanism, every failure-mode class implementation. | `engine-programmer` + `gameplay-programmer` | **Before T1 save-system implementation begins** | Open — **T1-blocking** |
| **ADR-tba — Save atomicity mechanism + power-loss model.** Temp-rename vs. write-ahead log vs. two-slot rotation. Must satisfy Rule 7 behavioural guarantee on Windows NTFS (with or without `FILE_FLAG_WRITE_THROUGH`) and macOS APFS. H-SL-R7 + H-SL-FM-AtomicityFailure (covers PowerLossMidWrite) depend on this resolution. | `engine-programmer` | **Before T1 save-system implementation begins** | Open — **T1-blocking** |
| **ADR-tba — Deterministic write-failure injection seam.** A narrow test-harness primitive for deterministic simulation of write failures (disk full, I/O error, key-derivation failure) during `Writing`. Scope narrower than a general mock-filesystem — just enough to make H-SL-FM-DiskFull, H-SL-FM-WriteIOError, H-SL-R7, and H-SL-R13 deterministically reproducible. Scope approved as part of the storage-backend ADR. | `engine-programmer` | **Before T1 save-system test harness lands** | Open — **T1-blocking** |
| **ADR-tba — Save event dispatch semantics.** Event-bus chosen for Save/Load ↔ WS communication must support **synchronous same-frame dispatch** for `SaveWriteConfirmed` (Rule 8 + §Edge Cases A4). Queued/end-of-frame dispatch eats the `save_mutex_max_ms` budget and breaks the WS timeout contract. H-SL-R8 verifies the behaviour. | `engine-programmer` | **Before T1 save-system implementation begins** | Open — **T1-blocking** |
| **ADR-tba — HMAC key-derivation strategy.** Per-install derivation required by [.claude/rules/save-integrity.md](../../.claude/rules/save-integrity.md) §Tamper Resistance: *"derivation strategy is a T1 design decision; document in the save system's GDD when authored."* Specific mechanism (device-ID-based, OS-keystore, file-derived, etc.) must be chosen for Rule 4 HMAC verification, `KeyDerivationFailureWrite` / `KeyDerivationFailureVerify` test doubles (§Edge Cases failure-mode matrix), and the save-integrity contract. ADR resolution must be reflected back into this GDD (inline documentation or appendix) per the save-integrity rule's "document in the save system's GDD" clause. | `engine-programmer` + `security-engineer` | **Before T1 save-system implementation begins** | Open — **T1-blocking** |

### T1 follow-up / advisory (not blocking T1 save-system implementation start)

| Question | Owner | Deadline | Status |
|---|---|---|---|
| **ADR-tba — Save fixture test-assembly scoping.** Per-version fixture saves in `tests/fixtures/saves/v[N]/` are raw binary/JSON consumed by edit-mode tests via `System.IO`, NOT Unity Addressables or Resources. Needs Editor-only assembly definition and fixture file format scoping. | `engine-programmer` | Before first migration fixture ships | Open — advisory at T1; **becomes T1-blocking at first schema bump** |
| **`save_autosave_interval` initial value** — 300 s default, 60–600 s safe range. Per §Tuning Knobs, revisit during T1 playtest once session cadence and crash-window tolerance are observed. | `qa-tester` + `game-designer` | T1 playtest | Pinned with revisit flag |

### T2+ deferred (not blocking T1)

| Question | Owner | Deadline | Status |
|---|---|---|---|
| **ADR-tba — Multi-slot / multi-character save identity model** (Rule 2's deferred save-identity model). | `game-designer` + `gameplay-programmer` | T2+ entry | Open — deferred |
| **ADR-tba — Networked save coordination (T2+).** Multi-client save coordination, authority boundaries, conflict resolution on client-server drift. | `gameplay-programmer` + `engine-programmer` | T2 entry gate | Open — deferred |
| **ADR-tba — Account-linked save identity (T2+).** Account-to-save binding, save-portability across accounts. | `gameplay-programmer` + `security-engineer` | T2 entry gate | Open — deferred |
| **ADR-tba — LLM dialogue memory persistence (T3+).** Per [DECISIONS.md](../../DECISIONS.md) D004, LLM dialogue with memory activates at T3; persistence semantics for LLM state/memory become a design concern then. | `gameplay-programmer` (dialogue) + `game-designer` | T3 entry gate | Open — deferred per D004 |

### Follow-up promotions (not ADR topics)

- **Promote WS-local QA test taxonomy** (Editor-validation / Dev-build smoke / Profiled playtest / Unit / Integration) from WS + Save/Load-local usage to `.claude/rules/test-standards.md` project-wide rule. The original deferral + "adopt in Save/Load first" condition is documented in [world-structure.md](world-structure.md) §Acceptance Criteria (promotion-deferral note) + §Open Questions (follow-up entry). Save/Load's adoption in §Acceptance Criteria now closes that adoption condition; promotion to the project-wide rule is the follow-up batch.
- **[systems-index.md](systems-index.md) line 172 stale description** — *"SQLite schema + character/world state serialization"* should be revised to match this GDD's ADR-deferred backend stance per §Overview non-negotiable #3. Separate follow-up batch.
