# ADR-0003: Progression Baseline Snapshot Contract

## Status
Proposed

## Date
2026-04-26

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Unity 6.3 LTS (6000.3.x) |
| **Domain** | Core gameplay hydration / progression integration |
| **Knowledge Risk** | MEDIUM - Unity 6.3 is post-LLM-cutoff, but this ADR defines data ownership, hydration ordering, and immutable read-model contracts rather than Unity-specific APIs. |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`; `docs/engine-reference/unity/breaking-changes.md`; `docs/engine-reference/unity/deprecated-apis.md`; `.claude/docs/technical-preferences.md` |
| **Post-Cutoff APIs Used** | None. |
| **Verification Required** | PlayMode load-order tests for Character Progression before Combat hydration; schema tests for consumer-scoped snapshots; integration tests for level-change baseline refresh without Combat current-resource refill. |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0001 XP Source Lifecycle Registry for Character Progression ownership of level/XP state; ADR-0002 Save Stability Barrier Protocol for stable save/read-view vocabulary. |
| **Enables** | ADR-0004 First-Save Materialization and Character Identity; Character Progression GDD approval; Save/Load hydration sequencing cleanup; Combat Core integration story readiness. |
| **Blocks** | T1 Character Progression implementation; T1 Save/Load resume implementation; T1 Combat player-actor hydration implementation. |
| **Ordering Note** | This ADR must be accepted before revising Character Progression and Save/Load references to the old generic `ProgressionBaselineSnapshot` handoff. |

## Context

### Problem Statement

Character Progression round-4 review exposed an internal contradiction in the baseline handoff contract. The GDD named `ProgressionBaselineSnapshot(current_level, permanent_max_health, permanent_max_mana, spell_eligibility_tier)` before Combat actor hydration, while also saying Combat uses only the health/mana maxima. Combat Core, however, defines level/stat input points and combat formulas that require an actor level. The result is an ambiguous fat snapshot: either Combat is reading fields the GDD says it does not use, or Combat has no explicit source for the player actor level it needs.

The project needs an architecture-level contract that says which system owns progression-derived baselines, which consumers may read which fields, how load hydration is ordered, and what happens when a level changes while Combat is already active.

### Constraints

- T1 is offline single-player with one active local character.
- Character Progression owns XP, level, spell eligibility tier, and permanent level-derived health/mana maxima.
- Combat Core owns combat formulas, runtime current resources, threat, damage, casting, regen, death, and actor hydration validation.
- Save/Load owns serialization, load ordering, rejection surfacing, and no-playable-partial-session behavior.
- Character Progression must not persist derived max-resource caches, Combat current resources, spell content, spell ids, learned abilities, spellbook state, or memorized slots.
- Combat Core's approved kill/death/XP hook boundaries remain unchanged.
- Layer 1 HUD, Spell Memorization, Menus, and Class Design are downstream or future consumers; this ADR must not implement their UI, spell content, or class progression surfaces.

### Requirements

- Give Combat an explicit, narrow source for player actor level and permanent max health/mana.
- Prevent Combat from consuming spell eligibility, total XP, UI progress, or spell unlock data through a fat progression snapshot.
- Preserve Save/Load ordering: Character Progression validates before Combat actor hydration, and no `ZoneActiveEvent` fires on failed hydration.
- Preserve Combat ownership of current health/mana, resource clamping/migration behavior, and combat formulas.
- Support level-up refreshes while active combat continues without implicit heals, mana refills, threat clears, or combat resets.
- Provide separate future-facing read models for UI and spell eligibility consumers without broadening the Combat handoff.

## Decision

Character Progression will expose consumer-scoped immutable snapshots/read models. There is no legal generic cross-system payload named only `ProgressionBaselineSnapshot` for all consumers. Each consumer must use the narrow contract registered for its purpose.

For T1, the load-critical contract is `CombatProgressionBaselineSnapshot`. Character Progression produces it after a valid progression state exists. Save/Load passes it to Combat Core before Combat hydrates or builds the player combat actor.

### Architecture Diagram

```text
Save/Load Resuming
      |
      v
Hydrate + validate Character Progression
      |
      v
Character Progression produces CombatProgressionBaselineSnapshot
      |      fields:
      |        local_character_id
      |        class_id
      |        combat_actor_level = current_level
      |        permanent_max_health
      |        permanent_max_mana
      |        progression_state_revision
      v
Combat Core hydrates/builds player actor
      |
      v
Validate current resources against Combat-owned rules
      |
      v
Gameplay may enable only after all required hydration succeeds
```

Future non-combat consumers use separate read models:

```text
Layer 1 HUD / Menus ----> ProgressionPresentationReadModel_T1
Spell Memorization ----> SpellEligibilityReadModel_T1
Combat Core -----------> CombatProgressionBaselineSnapshot
```

### Key Interfaces

#### Shared Snapshot Rules

All Character Progression snapshots/read models obey these rules:

- They are immutable views over one validated progression state revision.
- They include `local_character_id` and `progression_state_revision` for correlation and tests.
- They contain no Unity object references, Combat runtime actor ids, scene handles, threat tables, damage rolls, current health, current mana, cast state, cooldowns, spell content, learned ability ids, spellbook records, memorized slots, vendor/drop availability, or UI widget state.
- They are produced only after Character Progression has validated a new profile, hydrated a save payload, or completed a stable progression transaction.
- Consumers may not mutate Character Progression state through a snapshot.
- Consumers may cache a snapshot only until they observe a newer `progression_state_revision` or their owning GDD defines a stricter lifecycle.
- Consumers must not read a broader snapshot and ignore most fields when a narrow contract exists for their purpose.

#### CombatProgressionBaselineSnapshot

`CombatProgressionBaselineSnapshot` is the only Character Progression baseline Combat Core may consume in T1.

```yaml
CombatProgressionBaselineSnapshot:
  local_character_id: active local character id
  class_id: Cleric in T1
  combat_actor_level: int 1-10; equals Character Progression current_level
  permanent_max_health: int
  permanent_max_mana: int
  progression_schema_version: int
  progression_state_revision: monotonic local revision id
  produced_for: InitialHydration | NewProfileMaterialization | LevelChanged | DebugValidation
```

Rules:

- `combat_actor_level` is the explicit progression-derived level input for Combat's player actor formulas.
- `combat_actor_level` equals Character Progression `current_level` in T1. `visible_level` is not a combat input.
- `permanent_max_health` and `permanent_max_mana` are derived from Character Progression's approved permanent baseline formulas.
- Combat Core may read only `combat_actor_level`, `permanent_max_health`, `permanent_max_mana`, `class_id`, ids, and revision metadata from this snapshot.
- Combat Core owns all formula use of `combat_actor_level`. Character Progression does not define hit chance, damage, regen, threat, cast timing, target selection, or death rules.
- Combat Core owns runtime `current_health` and `current_mana`. Character Progression does not heal, refill, clamp, repair, or persist current resources.
- On load, if the snapshot is missing, malformed, for the wrong character, for a non-T1-supported class, below level 1, above level 10, or missing max-resource values, Combat hydration fails and Save/Load rejects the load before gameplay enables.
- If saved Combat current resources exceed max values, Combat Core's approved hydration rules decide clamp-vs-reject behavior. This ADR does not change Combat Core's resource hydration policy.

#### ProgressionPresentationReadModel_T1

`ProgressionPresentationReadModel_T1` is the future UI/Menu read model for progression display surfaces. It is not a Combat input.

```yaml
ProgressionPresentationReadModel_T1:
  local_character_id: active local character id
  current_level: int 1-10
  visible_level: int 1-10
  total_xp: int
  current_level_xp_progress: int
  current_level_xp_band: int
  next_level_xp_threshold: int compatibility alias for current_level_xp_band
  progression_state_revision: monotonic local revision id
```

Rules:

- Layer 1 HUD and Menus may use this read model only after their GDDs approve the player-facing presentation.
- It contains no permanent max-resource values, Combat runtime state, spell content, or spell ids.
- `visible_level` is the player-facing display level. `current_level` remains the mechanical level.
- At T1 cap, `current_level_xp_progress`, `current_level_xp_band`, and `next_level_xp_threshold` are zero.

#### SpellEligibilityReadModel_T1

`SpellEligibilityReadModel_T1` is the future spell-system read model for eligibility gating. It is not a Combat input and not a spell-content source.

```yaml
SpellEligibilityReadModel_T1:
  local_character_id: active local character id
  current_level: int 1-10
  spell_eligibility_tier: int
  unlock_list_id: authored Cleric T1 unlock-list id
  progression_state_revision: monotonic local revision id
```

Rules:

- Character Progression is the authority for `spell_eligibility_tier` by authored unlock list.
- Spell Memorization, Class Design, vendors, drops, and UI own spell ids, spell records, learned abilities, memorized slots, availability presentation, and spell effects.
- A `SpellEligibilityChanged` event may point consumers to this read model, but the event/read model must not create spell content.

#### Load Hydration Handoff

Save/Load must execute this order before gameplay is enabled:

1. Deserialize Character Progression's whitelisted save fields.
2. Call Character Progression hydration validation.
3. If validation fails, emit `LoadRejected(HydrationFailed)` and do not enable gameplay.
4. Obtain `CombatProgressionBaselineSnapshot` for the active local character.
5. Pass the snapshot to Combat Core actor hydration/build.
6. If Combat hydration fails, emit `LoadRejected(HydrationFailed)` and do not enable gameplay.
7. Continue remaining downstream hydration ordering declared by each GDD.
8. `ZoneActiveEvent` may fire only after required hydration succeeds.

#### Active Level-Change Refresh

When Character Progression changes `current_level` during active gameplay:

- It increments `progression_state_revision`.
- It refreshes permanent max health/mana outputs before emitting the per-level `LevelChangedEvent`.
- It makes a new `CombatProgressionBaselineSnapshot` available to Combat Core for the new revision.
- Combat Core applies the updated actor level and max-resource baseline according to Combat-owned rules.
- Character Progression must not heal, refill mana, clear threat, reset enemies, reset timers, revive the player, or otherwise modify Combat runtime state as part of the level change.

## Alternatives Considered

### Alternative 1: Keep One Fat ProgressionBaselineSnapshot

- **Description**: Continue passing `ProgressionBaselineSnapshot(current_level, permanent_max_health, permanent_max_mana, spell_eligibility_tier)` to Combat and let consumers ignore fields they do not need.
- **Pros**: Minimal prose churn in the current GDDs.
- **Cons**: Keeps the original contradiction, leaks spell eligibility to Combat, and encourages future consumers to depend on unrelated fields.
- **Rejection Reason**: The project needs explicit consumer contracts, not a broad payload with informal field discipline.

### Alternative 2: Remove Level From Combat Handoff

- **Description**: Pass only permanent max health/mana to Combat and require Combat to derive or own player actor level elsewhere.
- **Pros**: Matches the narrowest reading of current Save/Load prose.
- **Cons**: Combat Core already declares level/stat input points and formulas that need actor level; no other authoritative player level source is specified.
- **Rejection Reason**: This would preserve ambiguity instead of resolving it. Character Progression owns current mechanical level, so it must provide Combat's player actor level input explicitly.

### Alternative 3: Let Combat Query Character Progression Directly Whenever Needed

- **Description**: Combat calls Character Progression for current level and max resources during hydration and formula execution.
- **Pros**: Avoids snapshot DTOs.
- **Cons**: Couples Combat runtime formula paths to live progression state and creates timing ambiguity during XP transactions, load hydration, and future save barriers.
- **Rejection Reason**: Immutable revisioned snapshots make integration testable and keep ownership boundaries clearer.

### Alternative 4: Consumer-Scoped Progression Snapshots

- **Description**: Character Progression exposes narrow immutable read models for each approved consumer: Combat, UI/Menu presentation, and spell eligibility.
- **Pros**: Gives each consumer exactly the fields it needs, preserves Combat ownership of combat rules, and prevents spell/UI data from leaking through the combat handoff.
- **Cons**: Requires GDD sync and registry entries for each cross-system contract.
- **Decision**: Selected as the proposed architecture in this ADR.

## Consequences

### Positive

- Combat's player actor level source is explicit: `combat_actor_level = current_level`.
- The old fat `ProgressionBaselineSnapshot` ambiguity is removed.
- Character Progression remains the owner of progression-derived permanent baselines without redefining Combat rules.
- Spell eligibility and UI progress data stay out of the Combat hydration path.
- Save/Load has a precise hydration-order contract to enforce and test.
- Future consumers can add narrow read models instead of expanding the Combat snapshot.

### Negative

- Character Progression GDD and Save/Load GDD must be revised to replace old generic snapshot wording.
- Combat Core dependency prose should be synced to name `CombatProgressionBaselineSnapshot`.
- Test fixtures must inspect snapshot schemas, not just event ordering.
- Future systems cannot shortcut by reading the Combat snapshot when they need UI or spell data.

### Risks

- **Risk**: Implementers treat `CombatProgressionBaselineSnapshot` as permission for Character Progression to define Combat formulas.
  **Mitigation**: This ADR states that Combat owns formula use, current resources, and all combat rules.
- **Risk**: UI or spell systems consume Combat's baseline because it is already available on load.
  **Mitigation**: Register forbidden broad/fat-snapshot patterns and require each GDD to declare its own read model.
- **Risk**: A level-up during active combat creates an implicit heal or refill.
  **Mitigation**: Validation must assert current health/mana are not refilled by Character Progression; Combat owns any clamp/current-resource behavior.
- **Risk**: Snapshot revisions drift from persisted progression state after load.
  **Mitigation**: Save/Load tests must verify the snapshot revision is produced after validated hydration and before Combat actor hydration.

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|------------|-------------|--------------------------|
| `design/gdd/character-progression.md` | Character Progression must publish progression-derived baselines before Combat actor hydration without redefining Combat rules. | Defines `CombatProgressionBaselineSnapshot` and limits Combat's legal fields to actor level and permanent max resources. |
| `design/gdd/character-progression.md` | `current_level` and `visible_level` must remain distinct. | Maps `combat_actor_level` to `current_level` and keeps `visible_level` only in the presentation read model. |
| `design/gdd/character-progression.md` | Spell eligibility is formula/authored-list authority, not spell content or UI presentation. | Defines `SpellEligibilityReadModel_T1` with tier-only eligibility and bans spell ids/content from the read model. |
| `design/gdd/save-load-persistence.md` | Save/Load must hydrate Character Progression before Combat actor hydration and reject failed hydration before gameplay enables. | Locks the eight-step load handoff and the `LoadRejected(HydrationFailed)` outcome for missing or invalid baselines. |
| `design/gdd/combat-core.md` | Combat needs level/stat input points but owns combat formulas and runtime current resources. | Provides `combat_actor_level`, max health, and max mana while preserving Combat ownership of formulas, current resources, and hydration clamp/reject behavior. |
| `design/gdd/systems-index.md` | Systems graph must reflect real dependencies without broad hidden coupling. | Turns the Character Progression -> Combat edge into a named narrow interface instead of a generic cross-system snapshot. |

## Performance Implications

- **CPU**: O(1) read-model creation per hydration, new profile materialization, and stable level-change transaction. Combat formula costs remain owned by Combat Core.
- **Memory**: One small immutable snapshot/read model per active local character per consumer contract, retained only while the consumer needs that revision.
- **Load Time**: One additional narrow snapshot handoff during `Resuming`; no disk read or asset load is introduced by this ADR.
- **Network**: None in T1. A future network authority ADR must define replicated progression baselines before multiplayer consumes these contracts.

## Migration Plan

1. Revise `design/gdd/character-progression.md` to replace generic `ProgressionBaselineSnapshot` handoff prose with `CombatProgressionBaselineSnapshot`.
2. Revise `design/gdd/character-progression.md` acceptance criteria so Combat hydration expects `combat_actor_level = current_level`, `permanent_max_health`, and `permanent_max_mana`, and so spell/UI read models are distinct.
3. Revise `design/gdd/save-load-persistence.md` to name the Combat-specific snapshot in `Resuming`, dependency, reverse-listing, and Combat indirect-reference sections.
4. Revise `design/gdd/combat-core.md` dependency prose only if needed to name the explicit level/max-resource input contract; do not reopen Combat kill/death/XP hooks or formulas.
5. Update `docs/registry/architecture.yaml` after this ADR draft is reviewed, registering the snapshot state/interface contracts and forbidden fat-snapshot patterns.
6. Add or revise tests proving:
   - Character Progression validates before Combat hydration,
   - Combat receives `combat_actor_level = current_level`,
   - Combat does not receive spell eligibility or UI progress through its baseline snapshot,
   - level-up refreshes max-resource baselines without Character Progression changing Combat current resources,
   - `visible_level` is not used as a combat input.

## Validation Criteria

- Schema test proves `CombatProgressionBaselineSnapshot` contains exactly the allowed combat fields and omits `visible_level`, `total_xp`, XP progress fields, `spell_eligibility_tier`, spell ids, spell content, Combat current resources, threat, casts, targets, cooldowns, and runtime actor ids.
- Load-order integration test proves Save/Load hydrates and validates Character Progression, obtains `CombatProgressionBaselineSnapshot`, then hydrates Combat before any `ZoneActiveEvent`.
- Hydration failure test proves missing, malformed, wrong-character, wrong-class, or out-of-range combat baseline causes `LoadRejected(HydrationFailed)` before gameplay enables.
- Combat integration test proves `combat_actor_level` equals Character Progression `current_level` and is the only progression level field Combat consumes.
- Level-up integration test proves a `LevelChangedEvent` refreshes `CombatProgressionBaselineSnapshot` for the new revision without Character Progression healing, refilling mana, clearing threat, reviving, or resetting combat.
- UI/schema test proves `ProgressionPresentationReadModel_T1` exposes `visible_level` and XP band fields but no Combat current-resource or spell-content fields.
- Spell/schema test proves `SpellEligibilityReadModel_T1` exposes eligibility tier by authored unlock list but no spell ids, learned abilities, spellbook entries, memorized slots, vendors, drops, UI buttons, VFX, or spell effects.
- Architecture review proves no GDD still describes a generic all-consumer `ProgressionBaselineSnapshot` as the Combat handoff.

## Related Decisions

- ADR-0001: XP Source Lifecycle Registry
- ADR-0002: Save Stability Barrier Protocol
- `DECISIONS.md` D003 - Single-player offline through Tier 1.
- `DECISIONS.md` D007 - ADR-0001 XP Source Lifecycle Registry.
- `DECISIONS.md` D008 - ADR-0002 Save Stability Barrier Protocol.
- `design/gdd/character-progression.md`
- `design/gdd/save-load-persistence.md`
- `design/gdd/combat-core.md`
- `design/gdd/systems-index.md`
