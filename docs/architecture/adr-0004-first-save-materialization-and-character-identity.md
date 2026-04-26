# ADR-0004: First-Save Materialization and Character Identity

## Status
Proposed

## Date
2026-04-26

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Unity 6.3 LTS (6000.3.x) |
| **Domain** | Core persistence / player identity / gameplay-state bootstrap |
| **Knowledge Risk** | MEDIUM - Unity 6.3 is post-LLM-cutoff, but this ADR defines local identity ownership and first-save data-flow contracts rather than Unity-specific APIs. |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`; `docs/engine-reference/unity/breaking-changes.md`; `docs/engine-reference/unity/deprecated-apis.md`; `.claude/docs/technical-preferences.md` |
| **Post-Cutoff APIs Used** | None. |
| **Verification Required** | PlayMode first-save tests for identity generation, materialization ordering, no-bytes-written failure behavior, retry semantics, and subsequent-load no-re-materialization behavior. |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0001 XP Source Lifecycle Registry for XP dedupe identity use of `local_character_id`; ADR-0002 Save Stability Barrier Protocol for save-readiness vocabulary; ADR-0003 Progression Baseline Snapshot Contract for new-profile combat baseline publication. |
| **Enables** | ADR-0005 Progression Pacing Fixture Contracts; Character Progression GDD approval; Character Creation implementation readiness; Save/Load first-run implementation readiness. |
| **Blocks** | T1 Character Creation implementation; T1 Character Progression implementation; T1 Save/Load first-run implementation; any character-bound save payload implementation that requires `local_character_id`. |
| **Ordering Note** | This ADR must be accepted before revising Character Creation's open `local_character_id` question and Character Progression's first-save materialization prose. |

## Context

### Problem Statement

Character Creation already defines `InitialCharacterRecord.local_character_id` as an opaque immutable id, but leaves its generation location open. Character Progression uses `local_character_id` in XP dedupe keys and new-profile baselines, but its save whitelist intentionally excludes identity fields. Save/Load owns the first-run path and writes the first durable record, but must not synthesize progression state on first load or regenerate identity after a save exists.

Round-4 Character Progression review exposed that first-save materialization and `local_character_id` ownership need one architectural lock. Without it, three bugs remain likely: different systems could generate competing ids, the first save could omit materialized Character Progression state and rely on first-load synthesis, or later loads could silently repair missing state by re-running first-save defaults.

### Constraints

- T1 is offline single-player with exactly one active local character record.
- T1 has no account id, server character id, save-slot grid, multi-character management, network authority, or cloud identity.
- Character Creation owns the initial character-record schema and validation.
- Save/Load owns persistence, HMAC/versioning, atomic write behavior, first-run vs missing-file distinction, and write/load failure surfacing.
- Character Progression owns the progression save schema and materializes only its whitelisted progression state.
- Character Progression save state does not include `local_character_id`; it receives the active local identity as context from Player State / Save/Load.
- A failed first save must not leave a partially initialized record that later loads as valid.
- Subsequent saves and loads must use persisted state; they must not re-run first-save materialization to repair missing data.

### Requirements

- Assign one owner for generating and validating `local_character_id`.
- Define the first-save materialization protocol before Save/Load writes the first record.
- Guarantee the first successful save contains both the `InitialCharacterRecord` seed and materialized downstream save states required for T1.
- Keep first-save materialization separate from normal load hydration and normal save barriers.
- Preserve retry behavior after first-save failure without generating multiple durable identities for one creation attempt.
- Provide an identity context that Character Progression, Combat baselines, XP dedupe, and future character-bound systems can consume without owning identity.

## Decision

Character Creation owns `local_character_id` generation and validation as part of `InitialCharacterRecord`. Save/Load owns persistence and active-record context. Downstream systems consume `local_character_id` as read-only identity context and may not generate, replace, or infer it.

Before the first successful save, Save/Load must run a declared first-save materialization pass for every required downstream system whose T1 save payload cannot be copied directly from `InitialCharacterRecord`. In current T1 scope, Character Progression is required. The first save writes the validated seed plus materialized downstream payloads atomically, or writes no bytes.

### Architecture Diagram

```text
Menus routes New Game
      |
      v
Character Creation validates InitialCharacterRecord
      |
      | owns generated local_character_id
      v
Save/Load first-save request
      |
      v
FirstSaveMaterialization transaction
      |
      +--> Character Progression materializes CharacterProgressionSaveState
      |      from local_character_id + starting_class_id = Cleric
      |
      +--> Future declared materializers as their GDDs become T1-required
      |
      v
All required materializers succeed?
      |
      +-- yes --> atomic first save writes:
      |            InitialCharacterRecord seed
      |            CharacterProgressionSaveState
      |            PlayerZoneMembership and other approved seed payloads
      |
      +-- no  --> SaveFailedEvent(FirstSaveMaterializationFailed)
                  no bytes written; no slot initialized
```

After a successful first save:

```text
Continue / load existing record
      |
      v
Save/Load verifies version + HMAC
      |
      v
Hydrates persisted Player State and downstream payloads
      |
      v
No first-save materializer may run
```

### Key Interfaces

#### Local Character Identity

`local_character_id` is the stable T1 player-save identity.

```yaml
LocalCharacterIdentity:
  local_character_id: opaque id generated by Character Creation
  scope: local install / local save record in T1
  mutable: false
  player_authored: false
  network_authoritative: false
  account_bound: false
```

Rules:

- Character Creation generates `local_character_id` exactly once per submitted `InitialCharacterRecord`.
- Character Creation validates that the id is present, opaque, non-empty, non-player-authored, and not derived from character name, class id, save path, slot label, account id, device username, or Combat runtime ids.
- Save/Load persists the id as part of Player State / `InitialCharacterRecord` and exposes it as active-record context.
- Save/Load must never regenerate, repair, or replace an existing persisted `local_character_id`.
- Character Progression, Combat Core, XP dedupe, UI read models, and future character-bound systems consume the id as read-only context.
- `local_character_id` is not a multiplayer, server, account, entitlement, or analytics identity. T2+ identity authority requires a later ADR.

#### InitialCharacterRecord Ownership

Character Creation owns the seed schema:

```yaml
InitialCharacterRecord:
  local_character_id: generated opaque id, immutable, not player-authored
  creation_schema_version: 1
  character_name: sanitized canonical display name
  starting_class_id: Cleric
  appearance_tokens: approved T1 appearance token ids
  start_anchor_id: CityHub_InnRoom_StartAnchor
  player_zone_membership: resolved start zone membership
  onboarding_eligible: true
  onboarding_intro_state: pending
  starting_equipment_template_id: ClericStartingEquipment_T1
  carried_inventory: []
  starting_faction_reputation_baseline: NeutralAllFactions
```

Rules:

- Character Creation validates the record before Save/Load receives it.
- Save/Load may perform defensive schema checks before writing, but it does not own seed meaning.
- The initial seed remains persisted for auditability until a later migration explicitly removes or transforms it.
- Seed fields are not proof that downstream systems have materialized their own runtime records. Each downstream system must declare a materializer or hydration contract for its owned state.

#### First-Save Materialization Declaration

Any downstream system that must transform an `InitialCharacterRecord` seed into its own save payload before the first save declares:

```yaml
FirstSaveMaterializationDeclaration:
  owner_system: system name
  materializer_name: stable hook name
  consumes_seed_fields: list of InitialCharacterRecord fields
  produces_payloads: list of downstream-owned save payloads
  required_for_t1_first_save: bool
  owner_budget_ms: max owner-local materialization time
  failure_class: FirstSaveMaterializationFailed
```

T1 declaration:

```yaml
owner_system: Character Progression
materializer_name: CharacterProgressionFirstSaveMaterializer
consumes_seed_fields:
  - local_character_id
  - starting_class_id
produces_payloads:
  - CharacterProgressionSaveState
required_for_t1_first_save: true
failure_class: FirstSaveMaterializationFailed
```

Rules:

- Save/Load owns the registry of declarations it invokes during the first-save path.
- Required materializers must complete before Save/Load serializes the first record.
- Optional or future materializers may not be silently skipped once their GDD declares them T1-required.
- Materializers must be deterministic from validated seed data and approved authored defaults.
- Materializers do not write storage directly.

#### First-Save Materialization Request

```yaml
FirstSaveMaterializationRequest:
  first_save_request_id: local monotonic first-save attempt id
  local_character_id: from InitialCharacterRecord
  initial_character_record_revision: immutable local revision id for this creation attempt
  trigger_type: FirstSave
  requested_payloads: list of payload names owned by the downstream system
  owner_budget_ms: max owner-local materialization budget
  effective_deadline_monotonic_ms: monotonic deadline derived from caller context and owner budget
```

Rules:

- Save/Load must not invoke a materializer without a deadline.
- The request includes only the seed fields the downstream owner declared it consumes.
- Materializers may validate referenced authored defaults but may not inspect UI state, Menus state, storage paths, or Combat runtime state.

#### First-Save Materialization Result

```yaml
FirstSaveMaterializationResult:
  status: Materialized | Rejected | Failed
  owner_system: system name
  materializer_name: stable hook name
  first_save_request_id: matching request id
  produced_payloads: downstream-owned immutable first-save payloads
  active_identity_context: local_character_id correlation metadata
  owner_state_revision: debug/integration-test revision id
  reason_code: None | InvalidSeed | MissingAuthoredDefault | DeadlineExceeded | OwnerUnavailable | ValidationFailed | Unknown
  diagnostics: development-build diagnostic string
```

Rules:

- `Materialized` means the downstream owner produced a valid first-save payload and a matching runtime state or read model for the same local character.
- `Rejected` means the seed is invalid for that owner, such as Character Progression receiving `starting_class_id != Cleric` in T1.
- `Failed` means the owner could not complete because of missing authored data, deadline expiry, or internal validation failure.
- Any non-`Materialized` required result makes Save/Load emit `SaveFailedEvent(FirstSaveMaterializationFailed)` and write no bytes.
- Save/Load must not serialize seed-only placeholders for required downstream payloads.

#### CharacterProgressionFirstSaveMaterializer

Character Progression's T1 materializer consumes only `local_character_id` and `starting_class_id`.

It produces:

```yaml
CharacterProgressionSaveState:
  progression_schema_version: current T1 progression schema version
  class_id: Cleric
  current_level: 1
  total_xp: 0
  spell_eligibility_tier: tier for level 1 by authored unlock list
```

Rules:

- `local_character_id` is used as active identity context but is not stored inside `CharacterProgressionSaveState`.
- `starting_class_id` must be `Cleric` in T1.
- Character Creation does not provide level, XP, spell eligibility, or max-resource values.
- Character Progression computes level-1 permanent baselines and can publish `CombatProgressionBaselineSnapshot` after materialization per ADR-0003.
- The first save must persist `CharacterProgressionSaveState`; later loads hydrate it from disk and do not synthesize it from `starting_class_id`.

#### Retry and Overwrite Semantics

Rules:

- While the creation flow remains locked after a first-save write failure, retrying the same first-save request uses the same pending `InitialCharacterRecord.local_character_id`.
- If the player cancels or returns to title before any successful first save, the pending id is discarded and no initialized record exists.
- If Menus confirmed destructive overwrite of an existing record, the old record remains intact until the new first save commits atomically.
- The new `local_character_id` replaces the old active record identity only after the first save commit succeeds.
- Save/Load marks a slot/record initialized only after the first save commits. The exact backend marker is deferred to the storage-backend ADR.

#### Subsequent Load and Save Rules

Rules:

- A subsequent load must hydrate persisted `local_character_id` and persisted downstream payloads.
- If a previously initialized record is missing `local_character_id`, Character Progression state, or another required persisted payload, load fails loudly through Save/Load's normal failure path.
- Save/Load must not repair an existing record by generating a new id or by invoking first-save materializers.
- Normal saves after first save use ADR-0002 save-stability barriers where declared; they do not use first-save materializers.

## Alternatives Considered

### Alternative 1: Save/Load Generates local_character_id

- **Description**: Save/Load assigns `local_character_id` when writing the first record.
- **Pros**: Keeps id creation close to storage.
- **Cons**: Makes Character Creation's `InitialCharacterRecord` incomplete, blurs seed-schema ownership, and can create different ids across failed write retries if not carefully constrained.
- **Rejection Reason**: Character Creation owns the initial record schema and validation. Save/Load should persist the id, not invent seed content.

### Alternative 2: Character Progression Generates local_character_id

- **Description**: Character Progression creates the id when materializing its first save state.
- **Pros**: Puts the id near XP dedupe's first known consumer.
- **Cons**: Identity is broader than progression; Inventory, Faction Reputation, Menus, saves, and future systems also need the same id.
- **Rejection Reason**: Character Progression consumes identity context. It must not own the identity authority for the whole character record.

### Alternative 3: Synthesize Progression Defaults on First Load

- **Description**: The first save writes only Character Creation seed data; on first Continue, Character Progression synthesizes level 1 / 0 XP from `starting_class_id`.
- **Pros**: Reduces first-save coordination.
- **Cons**: Makes the first save incomplete, creates a different code path for first load, and risks silently repairing missing progression state after the record should be durable.
- **Rejection Reason**: The first successful save must be complete for T1-required downstream states.

### Alternative 4: Put local_character_id in Every Downstream Payload

- **Description**: Each downstream payload stores its own copy of `local_character_id`.
- **Pros**: Self-contained payloads.
- **Cons**: Creates duplicate identity storage and mismatch risk across subsystems.
- **Rejection Reason**: Save/Load provides active-record identity context; downstream payloads should reference it, not persist duplicate identity unless a later ADR requires denormalization.

### Alternative 5: Character Creation Identity + Save/Load Materialization Transaction

- **Description**: Character Creation generates the id and seed; Save/Load invokes required materializers before the first atomic write; downstream systems consume identity context.
- **Pros**: Keeps ownership local, guarantees first-save completeness, and separates first-save bootstrap from subsequent load repair.
- **Cons**: Requires a declaration registry and integration tests for first-save materializers.
- **Decision**: Selected as the proposed architecture in this ADR.

## Consequences

### Positive

- `local_character_id` has one T1 owner: Character Creation.
- Save/Load has a deterministic first-save transaction before any bytes are written.
- Character Progression first-save state is durable immediately after the first successful save.
- XP dedupe keys can rely on a stable active local identity from the beginning of play.
- Failed first saves cannot create half-initialized records.
- Later corrupted/missing persisted data cannot be hidden by re-running first-save defaults.

### Negative

- Character Creation, Save/Load, and Character Progression GDDs must be synced to name this protocol.
- Save/Load implementation needs a first-save materialization registry distinct from save-stability barriers.
- First-save write can fail before I/O if a required materializer rejects the seed or authored defaults are missing.
- Future systems that consume seed data must declare whether they are T1-required materializers or later migration/hydration consumers.

### Risks

- **Risk**: Implementers confuse first-save materializers with ADR-0002 save-stability barriers.
  **Mitigation**: First-save materializers run only before the first successful save; save-stability barriers run before normal save reads of runtime-owned state.
- **Risk**: A retry after first-save failure generates a second id for the same locked creation attempt.
  **Mitigation**: Retry rules require reusing the pending `InitialCharacterRecord` while the creation flow remains locked.
- **Risk**: Existing-record loads silently repair missing downstream payloads by reusing Character Creation seed defaults.
  **Mitigation**: This ADR bans re-materialization after first successful save; missing required persisted state is a loud load failure.
- **Risk**: `local_character_id` gets mistaken for a server/account identity.
  **Mitigation**: The identity scope explicitly excludes networking, accounts, entitlements, analytics, and T2+ authority.

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|------------|-------------|--------------------------|
| `design/gdd/character-creation.md` | `InitialCharacterRecord` includes `local_character_id`, but generation was open. | Assigns generation and validation to Character Creation and defines retry/discard semantics. |
| `design/gdd/character-creation.md` | Gameplay cannot begin until first save succeeds and zone activation completes. | Defines a first-save materialization transaction that must succeed before `SaveWriteConfirmed`. |
| `design/gdd/character-progression.md` | Before first save, Save/Load invokes Character Progression to materialize `CharacterProgressionSaveState` from `starting_class_id = Cleric`. | Defines `CharacterProgressionFirstSaveMaterializer`, request/result shape, produced fields, and no first-load synthesis. |
| `design/gdd/character-progression.md` | XP dedupe requires `local_character_id` from the active profile. | Defines Save/Load active-record identity context and Character Progression's read-only consumption of it. |
| `design/gdd/save-load-persistence.md` | Save/Load distinguishes first-run no-save bootstrap from missing-file failure. | Locks that first-run materialization occurs only before first successful save; later missing required state fails loud. |
| `design/gdd/save-load-persistence.md` | T1 exposes one active local character record. | Defines `local_character_id` as local T1 save identity without account/server/multi-slot semantics. |

## Performance Implications

- **CPU**: One bounded materializer call per T1-required downstream system during first save only; Character Progression's T1 materializer is constant-time aside from authored-data validation.
- **Memory**: One pending immutable `InitialCharacterRecord` and materialized payload set held until first-save commit or failure.
- **Load Time**: No additional subsequent-load work; this ADR forbids re-running first-save materializers during load.
- **Network**: None in T1. T2+ account/server identity requires a new ADR before local identity maps to network authority.

## Migration Plan

1. Revise `design/gdd/character-creation.md` to close the open `local_character_id` generation question by referencing ADR-0004.
2. Revise `design/gdd/character-creation.md` dependency prose so Character Progression is no longer only a future downstream consumer for T1 first-save progression state.
3. Revise `design/gdd/character-progression.md` to reference ADR-0004 for first-save materialization and active local identity context.
4. Revise `design/gdd/save-load-persistence.md` to define first-save materialization before the initial atomic write, separate from ADR-0002 save-stability barriers.
5. Revise acceptance criteria to prove:
   - Character Creation generates one opaque id per submitted initial record,
   - first-save retry reuses the pending id,
   - Character Progression materializes its whitelisted save state before first write,
   - first materialization failure writes no bytes and initializes no slot,
   - subsequent loads never invoke first-save materializers.
6. Update `docs/registry/architecture.yaml` after this ADR draft is reviewed, registering identity ownership, first-save materialization interfaces, and forbidden repair/generation patterns.

## Validation Criteria

- Unit test proves Character Creation-generated `local_character_id` is present, opaque, non-player-authored, not derived from `character_name`, and stable across a locked first-save retry.
- Integration test proves a valid `InitialCharacterRecord` invokes `CharacterProgressionFirstSaveMaterializer` before Save/Load writes the first record.
- Integration test proves the first successful save contains both the `InitialCharacterRecord` seed and `CharacterProgressionSaveState`.
- Failure test proves `starting_class_id != Cleric`, missing `local_character_id`, missing progression authored defaults, or materializer deadline expiry emits `SaveFailedEvent(FirstSaveMaterializationFailed)`, writes no bytes, and does not mark the record initialized.
- Overwrite test proves an existing record remains intact until the replacement first save commits successfully.
- Subsequent-load test proves a previously initialized record missing `local_character_id` or required `CharacterProgressionSaveState` fails loud instead of invoking first-save materialization.
- Identity-context test proves Character Progression uses Save/Load active-record `local_character_id` for `XpAwardDedupeKey` without persisting the id inside `CharacterProgressionSaveState`.
- ADR-0003 integration test proves a newly materialized Character Progression profile can publish `CombatProgressionBaselineSnapshot` for the same `local_character_id`.

## Related Decisions

- ADR-0001: XP Source Lifecycle Registry
- ADR-0002: Save Stability Barrier Protocol
- ADR-0003: Progression Baseline Snapshot Contract
- `DECISIONS.md` D003 - Single-player offline through Tier 1.
- `DECISIONS.md` D007 - ADR-0001 XP Source Lifecycle Registry.
- `DECISIONS.md` D008 - ADR-0002 Save Stability Barrier Protocol.
- `DECISIONS.md` D009 - ADR-0003 Progression Baseline Snapshot Contract.
- `design/gdd/character-creation.md`
- `design/gdd/character-progression.md`
- `design/gdd/save-load-persistence.md`
- `design/gdd/systems-index.md`
