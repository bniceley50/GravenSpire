# ADR-0001: XP Source Lifecycle Registry

## Status
Proposed

## Date
2026-04-26

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Unity 6.3 LTS (6000.3.x) |
| **Domain** | Core gameplay state / progression integration |
| **Knowledge Risk** | MEDIUM - Unity 6.3 is post-LLM-cutoff, but this ADR defines data ownership and event contracts rather than Unity-specific APIs. |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`; `docs/engine-reference/unity/breaking-changes.md`; `docs/engine-reference/unity/deprecated-apis.md`; `.claude/docs/technical-preferences.md` |
| **Post-Cutoff APIs Used** | None. |
| **Verification Required** | Unity EditMode validators for lookup data; PlayMode integration tests for kill-credit snapshot ordering, duplicate kill-credit rejection, respawn token isolation, save/load lifecycle reconstruction, and T1 repeatability policy enforcement. |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | None. |
| **Enables** | ADR-0002 Save Stability Barrier Protocol; ADR-0005 Progression Pacing Fixture Contracts; Character Progression GDD approval. |
| **Blocks** | T1 Character Progression implementation; Character Progression GDD approval. |
| **Ordering Note** | This ADR must be accepted before revising Character Progression's XP source lookup, NPC lifecycle, and pacing acceptance criteria. |

## Context

### Problem Statement

Character Progression needs defeated level, encounter role, XP eligibility, XP weighting, repeatability class, and source lifecycle identity to award XP from Combat Core kill-credit events. Combat Core is already approved and emits the narrow `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)` contract, so Character Progression must enrich that event without demanding new Combat fields or reading mutable Combat runtime state.

Round-4 review found that the current GDD prose introduced the right parts but had not locked the architecture: `repeatability_class` was present without durable enforcement semantics, lookup uniqueness was underspecified, and post-load source lifecycle behavior was not testable.

### Constraints

- T1 is offline single-player with one local active character.
- Combat Core's approved kill-credit payload remains unchanged.
- `combat_actor_id`, GameObject references, threat tables, damage rolls, and Combat runtime state are never legal XP identity inputs.
- Character Progression owns XP math, authored XP metadata, award snapshots, and transient processed-award dedupe.
- NPC System owns stable NPC/spawn source refs and durable source lifecycle records.
- Save/Load serializes NPC-owned source lifecycle state, not Character Progression tombstones or processed dedupe keys.
- Character Progression's T1 save state remains limited to `progression_schema_version`, `class_id`, `current_level`, `total_xp`, and `spell_eligibility_tier` unless a later ADR explicitly changes that contract.

### Requirements

- Resolve Combat kill-credit events into XP award inputs deterministically.
- Prevent duplicate kill-credit events for the same defeated lifecycle from awarding XP twice in the same playable session.
- Prevent a late duplicate for an old lifecycle from binding to a newly respawned source.
- Preserve durable source death / availability across save/load through NPC-owned records.
- Make repeatability semantics testable and enforceable without hidden per-character claim state in T1.
- Let pacing tests reason about repeatable trash, named lockouts, and future first-kill rewards without conflating them.

## Decision

Character Progression will use a progression-owned authored lookup asset and transient runtime registry to enrich Combat kill-credit events. NPC System will own durable source lifecycle state. T1 will not persist Character Progression tombstones, processed dedupe keys, or per-character first-kill claim records.

### Architecture Diagram

```text
NPC authored source refs + lifecycle hooks
        |
        v
NPC System ------------------------------+
owns durable NpcSourceLifecycleRecord     |
        |                                 |
        | activation/death lifecycle hook |
        v                                 |
Character Progression                    |
owns ProgressionXpSourceRefLookup_T1      |
owns XpSourceLifecycleRegistry            |
owns XpAwardResolutionSnapshot            |
        ^                                 |
        | PlayerKillCreditEvent           |
        | defeated_source_ref, zoneId,    |
        | faction_id, kill_weight_seed    |
        |                                 |
Combat Core ------------------------------+
approved narrow event; no XP metadata fields added

Save/Load persists:
  - CharacterProgressionSaveState: XP/level/spell eligibility only
  - NpcSourceLifecycleRecord: durable source lifecycle only
```

### Key Interfaces

#### Authored Lookup Row

`ProgressionXpSourceRefLookup_T1` is authored and validated by Character Progression. It contains one row per XP-eligible or explicitly XP-ineligible source ref that Character Progression may resolve in T1.

```yaml
ProgressionXpSourceRefLookupRow:
  zoneId: stable zone id
  defeated_source_ref: Combat-stable NPC/spawn source ref
  defeated_level: int
  encounter_role: Trash | Named | Camp
  encounter_role_multiplier: float
  xp_weight_seed_t1: float
  expected_kill_weight_seed_t1: float
  repeatability_class: Repeatable | RespawnLockout | NonRepeatableFirstKill
  source_lifecycle_token_policy: PersistentNpcEpisode | SpawnCycle
  xp_eligible: bool
```

Validator requirements:

- `(zoneId, defeated_source_ref)` is globally unique within `ProgressionXpSourceRefLookup_T1`.
- Duplicate rows, conflicting lifecycle policies, or multiple XP metadata rows for one Combat source are build-blocking errors.
- `defeated_source_ref` must resolve to an NPC System stable source namespace used by Combat Core kill credit.
- `source_lifecycle_token_policy = PersistentNpcEpisode` requires NPC-owned source lifecycle persistence.
- `source_lifecycle_token_policy = SpawnCycle` requires an NPC/spawn lifecycle hook that creates a new token for each spawn cycle.
- `repeatability_class = RespawnLockout` requires `PersistentNpcEpisode` and an NPC-authored respawn or availability timing key.
- `repeatability_class = NonRepeatableFirstKill` is future-reserved and invalid for T1 shipping rows until a later ADR defines durable per-character claim persistence.
- Combat fixture sources named in Combat Core acceptance tests must have matching rows or explicit `xp_eligible = false` rows.
- For T1 Combat fixture rows, `expected_kill_weight_seed_t1` must match the Combat fixture's emitted `kill_weight_seed`.

#### Runtime Registry Entry

`XpSourceLifecycleRegistry` is transient runtime state owned by Character Progression. It is populated from validated lookup rows and NPC lifecycle hooks.

```yaml
XpSourceLifecycleRegistryEntry:
  zoneId: stable zone id
  defeated_source_ref: Combat-stable NPC/spawn source ref
  source_lifecycle_token: opaque local lifecycle token
  defeated_level: int
  encounter_role: Trash | Named | Camp
  encounter_role_multiplier: float
  xp_weight_seed_t1: float
  expected_kill_weight_seed_t1: float
  repeatability_class: Repeatable | RespawnLockout
  xp_eligible: bool
  lifecycle_state: Active | DefeatedTombstone
```

Rules:

- A source cannot award XP until an `Active` entry exists with all required metadata and a lifecycle token.
- Registry entries and tombstones are not serialized by Character Progression.
- A registry lookup must never resolve XP against whatever source currently occupies a spawn anchor unless that source has the same lifecycle token as the kill-credit event's defeated lifecycle.
- A missing, malformed, or ambiguous registry entry rejects the XP award and emits a development diagnostic.

#### Award Resolution Snapshot

`XpAwardResolutionSnapshot` is a same-dispatch immutable copy of XP metadata for one defeated lifecycle.

```yaml
XpAwardResolutionSnapshot:
  zoneId: stable zone id
  defeated_source_ref: Combat-stable NPC/spawn source ref
  source_lifecycle_token: opaque local lifecycle token from defeated lifecycle
  defeated_level: int
  encounter_role: Trash | Named | Camp
  encounter_role_multiplier: float
  xp_weight_seed_t1: float
  expected_kill_weight_seed_t1: float
  repeatability_class: Repeatable | RespawnLockout
  xp_eligible: bool
  lifecycle_state: DefeatedTombstone
```

Snapshot rules:

- Combat Core emits kill credit exactly as approved; it does not add XP metadata fields.
- NPC System records the defeated `NpcSourceLifecycleRecord` and Character Progression captures the `XpAwardResolutionSnapshot` in the same kill-resolution phase.
- NPC cleanup/despawn cannot retire scene references or rotate the source lifecycle token until NPC System and Character Progression have acknowledged the phase.
- The snapshot is the only source of XP metadata for the award attempt; Character Progression does not ask Combat Core for missing metadata and does not infer from live NPC state after death.

#### Dedupe Key

```yaml
XpAwardDedupeKey:
  local_character_id: active local character id
  zoneId: stable zone id
  defeated_source_ref: Combat-stable NPC/spawn source ref
  source_lifecycle_token: opaque local lifecycle token
```

Rules:

- All four fields are required.
- `local_character_id` comes from the active hydrated character/progression profile.
- Source identity fields come from the immutable award snapshot.
- `combat_actor_id` is never legal in this key.
- Processed keys are transient and retained for the current playable session plus the source lifecycle retention window.
- Duplicate events with the same key cannot award twice.
- Late duplicates for old lifecycle tokens may resolve only to the retained tombstone/processed key or be rejected as stale; they must never bind to a newer active token at the same source ref.

#### Durable NPC Source Lifecycle

NPC System owns the durable source lifecycle record:

```yaml
NpcSourceLifecycleRecord:
  zoneId: stable zone id
  defeated_source_ref: Combat-stable NPC/spawn source ref
  source_lifecycle_token: opaque local lifecycle token
  source_lifecycle_state: Active | Defeated | RespawnEligible
  source_lifecycle_token_policy: PersistentNpcEpisode | SpawnCycle
  respawn_or_availability_timing_key: authored timing reference
```

Rules:

- The record may not contain XP values, progression dedupe keys, Combat runtime actor ids, GameObject references, or Character Progression tombstones.
- Save/Load persists and hydrates this record as NPC-owned World State.
- On load, Character Progression rebuilds only valid `Active` registry entries from hydrated NPC lifecycle state and validated lookup rows.
- A `Defeated` source does not produce an active XP-awarding registry entry after load.
- A `RespawnEligible` source may create a new token only through NPC System's normal activation/respawn path.

#### Repeatability Semantics

`Repeatable`:

- Legal for T1 trash/spawn-cycle rows.
- Each distinct lifecycle token can award once.
- A later spawn cycle with a new token can award again.
- Same-session duplicate events for the same token are deduped by `XpAwardDedupeKey`.

`RespawnLockout`:

- Legal for T1 persistent NPC rows only when `source_lifecycle_token_policy = PersistentNpcEpisode`.
- Each lifecycle token can award once.
- The durable lockout/availability state is owned by NPC System through `NpcSourceLifecycleRecord`.
- The source cannot create a new XP-awarding token until NPC System transitions it through authored lockout/availability timing.
- Pacing tests may not project `RespawnLockout` rows as continuous repeatable XP/hour routes unless the lockout timing is explicitly included in the fixture.

`NonRepeatableFirstKill`:

- Future-reserved.
- Invalid for T1 shipping rows.
- Requires a later ADR/GDD amendment defining durable per-character first-kill claim ownership before it can become legal.
- If encountered in T1 content validation, it fails validation rather than silently behaving as `RespawnLockout`.

## Alternatives Considered

### Alternative 1: Add XP Metadata to Combat Core Events

- **Description**: Amend `PlayerKillCreditEvent` to include `defeated_level`, `encounter_role`, `encounter_role_multiplier`, repeatability, and source lifecycle metadata.
- **Pros**: Simple for Character Progression receive-side logic.
- **Cons**: Reopens approved Combat Core boundaries and pushes progression/economy metadata into the combat subsystem.
- **Rejection Reason**: Combat Core is approved and should own combat rules, not XP metadata. Consumers should enrich narrow combat events from their own authored data.

### Alternative 2: Resolve XP Metadata From Live NPC State at Receive Time

- **Description**: On kill-credit receipt, Character Progression queries the current NPC/spawn object at the source ref for level, role, repeatability, and XP eligibility.
- **Pros**: Avoids a separate progression lookup asset.
- **Cons**: Races cleanup/despawn and respawn, can bind old kill credit to a new occupant, and makes XP depend on mutable runtime state after death.
- **Rejection Reason**: The design requires same-dispatch immutable snapshots and stale duplicate protection.

### Alternative 3: Persist Character Progression Tombstones and Processed Dedupe Keys

- **Description**: Character Progression saves tombstones and processed `XpAwardDedupeKey` records directly.
- **Pros**: Makes repeatability and dedupe self-contained inside Character Progression.
- **Cons**: Expands the Character Progression save whitelist, duplicates NPC lifecycle ownership, and stores event-processing artifacts that are not replayed after load.
- **Rejection Reason**: T1 persistence should keep XP/level/spell eligibility in Character Progression and durable source lifecycle in NPC System. Per-character first-kill claim state is deferred until a real design needs it.

### Alternative 4: Progression Lookup + NPC Durable Lifecycle

- **Description**: Character Progression owns XP metadata and transient dedupe; NPC System owns durable source lifecycle; Save/Load persists each owner's state separately.
- **Pros**: Preserves Combat Core approval, keeps state ownership clean, prevents same-session duplicate awards, and makes save/load lifecycle behavior testable.
- **Cons**: Requires cross-system validators and PlayMode integration tests.
- **Decision**: Selected as the proposed architecture in this ADR.

## Consequences

### Positive

- Combat Core remains a narrow producer of kill-credit facts.
- Character Progression owns XP metadata and does not depend on live Combat/NPC runtime mutation after death.
- NPC System owns source availability and persistence, matching its existing identity/lifecycle role.
- Save/Load can serialize XP state and source lifecycle state through distinct owners.
- The T1 validator can reject ambiguous lookup rows before gameplay or pacing tests run.

### Negative

- Character Progression implementation now needs an Editor validator for lookup rows.
- NPC System implementation must expose lifecycle hooks and durable records for XP-relevant sources.
- T1 cannot use true per-character `NonRepeatableFirstKill` rewards until a later persistence decision defines claim ownership.
- Pacing fixtures must include repeatability class and lifecycle timing data.

### Risks

- **Risk**: `RespawnLockout` could still become a disguised repeatable route if lockout timing is too short.
  **Mitigation**: Pacing fixtures must include lockout timing and may not project lockout rows as continuous trash-like XP/hour routes.
- **Risk**: Lookup rows drift from NPC source refs or Combat fixture refs.
  **Mitigation**: Editor validation fails missing, duplicate, conflicting, or fixture-mismatched rows.
- **Risk**: Save/load restores XP from a kill but not source lifecycle state.
  **Mitigation**: Save/Load must serialize post-barrier XP only with post-barrier NPC lifecycle state; post-load integration tests must verify defeated sources remain non-active until NPC-authored availability permits.
- **Risk**: Future systems may need first-kill rewards.
  **Mitigation**: `NonRepeatableFirstKill` remains reserved and invalid until a future ADR defines durable per-character claim persistence.

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|------------|-------------|--------------------------|
| `design/gdd/character-progression.md` | Rule 4 requires Character Progression to resolve XP metadata from `XpSourceLifecycleRegistry` while Combat Core remains unchanged. | Defines the lookup asset, runtime registry, immutable snapshot, and no-Combat-amendment boundary. |
| `design/gdd/character-progression.md` | Rule 5 requires `XpAwardDedupeKey(local_character_id, zoneId, defeated_source_ref, source_lifecycle_token)` and no fallback identity. | Defines required key fields, source of each field, session retention, and stale duplicate behavior. |
| `design/gdd/character-progression.md` | Runtime registry rows include `repeatability_class` and `source_lifecycle_token_policy`. | Defines T1 legal semantics for `Repeatable` and `RespawnLockout`, and reserves `NonRepeatableFirstKill` until durable claim ownership exists. |
| `design/gdd/npc-system.md` | NPC System owns durable `NpcSourceLifecycleRecord` for XP-relevant NPC/spawn sources. | Defines fields, forbidden fields, and how hydrated lifecycle state rebuilds active progression registry entries. |
| `design/gdd/save-load-persistence.md` | Save/Load persists Character Progression XP state and NPC source lifecycle state through separate owners. | Locks that Character Progression tombstones/dedupe keys are not saved and NPC lifecycle records are the durable source state. |

## Performance Implications

- **CPU**: One dictionary lookup by `(zoneId, defeated_source_ref, source_lifecycle_token)` per kill-credit event; Editor validation is offline.
- **Memory**: One active or tombstone registry entry per active/retained XP-relevant source in the loaded zone/session; processed dedupe keys retained only for the playable session and lifecycle retention window.
- **Load Time**: On load, Character Progression rebuilds active registry entries from hydrated NPC lifecycle state and validated lookup rows for loaded content only.
- **Network**: None in T1. A future networked authority model must supersede or extend this ADR before replicated XP awards exist.

## Migration Plan

1. Revise `design/gdd/character-progression.md` to reference ADR-0001 for XP source lookup, duplicate/collision validation, repeatability semantics, and post-load lifecycle integrity.
2. Revise `design/gdd/npc-system.md` to reference ADR-0001 for NPC-owned source lifecycle records and same-frame death/snapshot ordering.
3. Revise `design/gdd/save-load-persistence.md` to reference ADR-0001 for NPC lifecycle durability and Character Progression tombstone non-persistence.
4. Update Character Progression acceptance criteria to add:
   - duplicate lookup row rejection,
   - conflicting lifecycle policy rejection,
   - T1 rejection of `NonRepeatableFirstKill`,
   - kill -> save -> load -> no duplicate XP / no immediate respawn integrity,
   - `RespawnLockout` pacing projection with explicit lockout timing.
5. Update `docs/registry/architecture.yaml` after this ADR is accepted, registering state ownership and interface contracts.

## Validation Criteria

- Editor validator fails duplicate `(zoneId, defeated_source_ref)` rows.
- Editor validator fails conflicting lifecycle policies or repeatability classes for one source ref.
- Editor validator fails any T1 shipping row using `NonRepeatableFirstKill`.
- PlayMode test proves a valid kill credit produces exactly one XP transaction from an immutable snapshot.
- PlayMode test proves duplicate kill credit for the same lifecycle token awards once.
- PlayMode test proves a stale duplicate cannot bind to a new active respawn token.
- PlayMode save/load test proves a defeated source remains non-active after load until NPC-authored availability permits a new lifecycle token.
- PlayMode save/load test proves XP is not serialized unless matching NPC source lifecycle state is also save-stable.
- Pacing fixture validation proves `RespawnLockout` rows are not projected as continuous repeatable XP/hour routes without explicit lockout timing.

## Related Decisions

- `DECISIONS.md` D003 - Single-player offline through Tier 1.
- `DECISIONS.md` D006 - Codex Added as Parallel Implementer.
- `design/gdd/character-progression.md`
- `design/gdd/npc-system.md`
- `design/gdd/save-load-persistence.md`
- `design/gdd/combat-core.md`
