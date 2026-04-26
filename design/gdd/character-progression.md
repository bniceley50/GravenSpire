# Character Progression

> **Status**: In Design
> **Author**: Codex (session with brian, 2026-04-25)
> **Last Updated**: 2026-04-25
> **Implements Pillar**: Primary - **P3 Reputation Is The Progression**. Supports - **P2 The Silence Is Sacred** and **P5 Stakes Are Honest**.

## Overview

Character Progression is the mechanical ladder of Gravenspire's EQ-classic class progression: XP, levels, permanent stat growth, and spell-unlock eligibility for the T1 Cleric. It exists so camps, haunt depth, spell access, and survival margins can scale over time without turning the game into a gear treadmill or making class level the player's identity story. The game's three-track model remains intact: Character Progression owns the vertical class ladder; Faction Reputation owns the horizontal identity ladder; Server State owns the larger political backdrop. A higher-level Cleric is mechanically more capable, but not more socially meaningful by default.

At T1, Character Progression is deliberately narrow: offline single-player, one local Cleric, one haunt, one city hub, no networking, no account progression, no PvP, no companions, no live LLM, and no alternate classes. The system consumes Character Creation's `starting_class_id = Cleric` seed, persists its own XP/level/spell-eligibility state through Save/Load, and consumes Combat Core's approved kill/death hooks without redefining combat rules or requiring new Combat event fields. In particular, Combat Core owns runtime `current_health` and `current_mana`, while Character Progression owns level scaling and permanent maximum resource values that Save/Load must hydrate before Combat builds or hydrates the player combat actor.

This GDD defines the XP curve, level-up rules, permanent stat-growth contract, authored XP-source lookup contract, spell-unlock eligibility contract, persistence whitelist, and downstream interfaces. It does not define Cleric spell content, learned abilities, memorized spell slots, equipment stats, loot, faction reputation values, corpse-run recovery, XP-loss penalties, zone-control math, combat hit/damage rules, or any Tier 2+ class/account/server progression. Those systems consume Character Progression outputs through explicit interfaces rather than inheriting unstated assumptions.

## Player Fantasy

Character Progression should feel like the old EQ promise made gothic and restrained: the camp was dangerous, the pulls were slow, the XP bar moved by inches, and then the character dings. The moment matters because it was earned through patience, risk, and repeated returns to the same haunted rooms. Death and corpse-run penalties belong to Death & Corpse Recovery; this GDD supports that future fear through preview interfaces only, not by claiming the full death-stakes fantasy before the death system exists. A level is not a celebration that the world loves you; it is proof that your body and practice have hardened enough to stand a little deeper in the dark.

The player should feel mechanical growth directly. More maximum health, more maximum mana, improved combat-facing baselines, and new spell-unlock eligibility make the Cleric more capable over time. A hallway that demanded full mana and perfect pulling at level 3 may become manageable at level 5. A camp that was foolish alone remains foolish, but the margin for disciplined play widens. That is the fantasy: not domination, but earned steadiness.

Character Progression must not make the player feel socially promoted. Leveling does not grant faction trust, political title, NPC intimacy, patron access, reputation rank, or narrative importance. Those belong to Faction Reputation, Faction Events, Dialogue, and the city's server-state layer. Character level says, "you have survived and practiced." Reputation says, "the city knows what that survival means."

### Anchor Moments

- **The first ding after a hard camp**: the XP threshold crosses after a kill, the level changes, and the player immediately understands that the last hour counted.
- **The deeper pull**: a corridor that previously forced retreat becomes viable because permanent max health/mana and level-based baselines have improved.
- **The spell eligibility moment**: the player reaches the level where a new Cleric spell becomes learnable, but the actual spell content and memorization still belong to Class Design and Spell Memorization.
- **The humility check**: the player levels up and still cannot solo the named enemy, because class progression improves margins without erasing group-dependency design.

### Anti-fantasy

- Leveling should not feel like becoming the chosen one.
- Leveling should not replace faction reputation as the story of the character.
- Leveling should not shower the screen with spectacle, loot-tier language, or hero framing.
- Leveling should not unlock account-wide bonuses, server prestige, PvP power, companion authority, or live-service progression.
- Leveling should not redefine Combat Core rules; Combat reads progression outputs, but combat timing, death, damage, and runtime current resources stay in Combat-owned systems.

## Detailed Design

### Core Rules

1. **Cleric-only T1 progression.** T1 Character Progression supports exactly one local `Cleric` progression profile. The playable T1 band is levels `1-10`; the data model and formula ranges are shaped for future `1-60` expansion but levels above 10 are locked out in T1.

2. **Character Creation seed.** A new T1 character initializes at `class_id = Cleric`, `current_level = 1`, `visible_level = 1`, `total_xp = 0`, no XP debt, no level-up pending state, and default level-1 permanent baselines. Character Creation does not need to add a separate level/XP seed. Before first save, Save/Load invokes Character Progression to materialize `CharacterProgressionSaveState` from `starting_class_id = Cleric`; the first save must contain the progression state rather than synthesizing it on first load.

3. **Kill-credit XP only.** In T1, XP can be awarded only from Combat Core's approved `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)`. Character Progression must not require Combat Core to add `defeated_level`, `encounter_role`, XP values, spell data, or progression transaction ids to that event. Quest XP, exploration XP, discovery XP, dialogue XP, faction XP, crafting XP, account XP, rested XP, and companion XP are out of scope.

4. **XP source metadata lookup.** Character Progression resolves `defeated_level`, `encounter_role`, `encounter_role_multiplier`, `xp_eligible`, progression-owned `xp_weight_seed_t1`, expected T1 Combat `kill_weight_seed`, and `source_lifecycle_token` from its `XpSourceLifecycleRegistry`. Resolution is not a loose receive-time lookup against whatever source currently occupies the spawn anchor: the progression registration adapter must create a same-dispatch immutable `XpAwardResolutionSnapshot` before the defeated source can retire or respawn. This is the chosen contract for the Combat mismatch blocker: Combat Core remains approved and unchanged; progression-owned lookup/runtime registry data supplies XP-only metadata.

5. **Kill-credit dedupe identity.** Every XP award attempt requires a structured tuple `XpAwardDedupeKey(local_character_id, zoneId, defeated_source_ref, source_lifecycle_token)`. `local_character_id` comes from the hydrated active Character Progression profile; the source identity components come from the immutable award snapshot. If any component is missing or unresolved, no XP is awarded. Character Progression keeps a processed-key set for the current playable session and at least the source lifecycle retention window; duplicate events with the same key cannot award twice, while a respawn or new lifecycle token at the same spawn anchor is eligible for a new award only after the prior lifecycle is retained as a separate defeated tombstone.

6. **Combat event consumption is narrow.** Character Progression consumes `PlayerKillCreditEvent` only to evaluate XP eligibility and award XP. It reads the event fields listed in Rule 3, performs the lookup in Rule 4, and never inspects threat tables, damage rolls, combat actor ids, loot, corpse records, or runtime combat state.

7. **No faction mutation from XP events.** `faction_id` may appear on kill-credit events for downstream systems, but Character Progression must not mutate reputation, faction rank, faction standing, political access, NPC relationship, or faction visuals from it.

8. **Immediate ding.** When an XP transaction causes `total_xp` to meet or exceed the next level threshold, level-up resolves immediately in the same progression transaction. The event is felt, but restrained: data changes, one quiet per-level `LevelChangedEvent`, and downstream UI/audio hooks only.

9. **Sequential multi-level resolution.** If one XP transaction crosses multiple thresholds, levels resolve one at a time until `total_xp` falls below the next threshold or the T1 cap is reached. Each level increment refreshes permanent progression outputs before evaluating the next threshold and emits exactly one `LevelChangedEvent` for that single level step.

10. **T1 cap behavior.** At level 10, `current_level`, `visible_level`, and persisted `total_xp` are capped at `xp_threshold(t1_level_cap)`. Additional kill credit at cap is rejected as `AtCapNoXp` with a development diagnostic only. No banked over-cap XP exists in T1, and raising a future cap must require newly earned XP after a tier-transition decision.

11. **Level query vocabulary.** `current_level` is the mechanically active level after applying the T1 cap. `visible_level` is the value allowed for player-facing UI and equals `current_level` throughout T1. There is no hidden over-cap level. `current_level_xp_progress` and `next_level_xp_threshold` are derived query values defined in the Formulas section and are never separately persisted.

12. **Permanent progression outputs only.** Character Progression owns permanent max health, permanent max mana, XP state, and spell-unlock eligibility. Combat Core owns runtime `current_health`, `current_mana`, regen, damage, death, threat, casting, and med-break state; attack power, armor class, weapon delay, range, and skills remain Combat Core / Class Design fixture data until Class Design supersedes them.

13. **Hydration precedes Combat actor build.** On load, Save/Load must hydrate and validate Character Progression before Combat Core hydrates or builds the player combat actor. Progression publishes a narrow `ProgressionBaselineSnapshot(current_level, permanent_max_health, permanent_max_mana, spell_eligibility_tier)`. Combat uses only the health/mana maxima to validate or initialize runtime current resources; attack power, armor class, weapon delay, range, and skills remain Combat Core / Class Design fixture data until Class Design supersedes them. Progression never mutates Combat runtime values directly.

14. **No ding combat reset.** Level-up does not fully heal, fully restore mana, clear threat, cancel death, clear XP penalties, revive the player, reset med timers, interrupt enemy behavior, or change combat rules. If max health/mana increase, Combat decides how runtime current values respond.

15. **Spell eligibility only.** Character Progression may emit system-facing `SpellEligibilityChanged` for level-appropriate Cleric spell tiers. The authoritative query is the authored `spell_tier_unlock_levels_t1` list, not spell content and not a generated spell list. Tier 1 at level 1 is a baseline eligibility state, not an earned unlock. Player-facing use of levels 3/5/7/9 is blocked until Class Design and Spell Memorization map each eligibility beat to at least one authored Cleric gameplay beat. Progression does not define spell ids, spell effects, learned abilities, spell vendors, spell drops, memorized slots, spellbook UI, player-facing spell copy, or spell mana costs.

16. **Mechanical, not social.** Level never grants faction trust, social rank, patron access, NPC intimacy, city title, story status, political authority, companion authority, account prestige, or server recognition. Downstream systems may use level only as combat-safety context or encounter-readiness advice, never as the primary condition for reputation rank, trust, patron access, title, intimacy, or political authority.

17. **Named enemies remain gated.** Level growth widens survival and mana margins for ordinary camps; it does not make named enemies or linked camps default solo targets. Encounter design and Combat Core's soloability envelope remain authoritative.

18. **Progression persistence whitelist.** T1 progression save state contains only `progression_schema_version`, `class_id`, `current_level`, `total_xp`, and `spell_eligibility_tier`. It must not persist XP debt, Combat runtime current resources, threat, cast state, target selection, cooldowns, runtime ids, derived max-resource caches, spell content, spell ids, learned abilities, spellbook records, memorized slots, or vendor/drop availability.

19. **Hydration validation.** On load, Character Progression validates class, level range, XP bounds, cap behavior, schema version, and spell eligibility consistency before gameplay enablement. Invalid progression hydration returns failure to Save/Load; it does not silently repair or default.

20. **Death penalty preview only in T1.** Character Progression exposes `death_xp_debt_preview` as a policy preview and reserves a narrow XP-adjustment integration point for Death & Corpse Recovery. T1 rejects applied death penalty requests unless they use the future `ProgressionXpAdjustmentRequest` contract defined below and come from an approved Death & Corpse Recovery policy; Combat Core's `PlayerDeathEvent` never applies XP loss directly.

21. **Quiet presentation.** Character Progression emits data and presentation hooks; Layer 1 HUD, Audio, and later UI specs own how they appear. No hero banners, loot-tier language, glowing panels, battle-pass meters, fanfare, or center-screen reward treatment.

22. **T1 exclusions.** No networking, accounts, server progression, PvP scaling, companion progression, alternate classes, account unlocks, live-service tracks, achievements, or LLM memory.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Uninitialized` | No progression state exists | Character Creation seed accepted -> `InitializedLevel1`; Save/Load hydrate -> `HydratedFromSave` | No XP events accepted. |
| `InitializedLevel1` | New Cleric profile created | Valid kill credit -> `XpAwarding`; save requested -> `ProgressionSaved` | Level 1, 0 XP, default baselines. |
| `HydratedFromSave` | Save/Load delivers valid progression payload | Validation success -> `Ready`; validation failure -> `InvalidProgressionState` | Rebuilds permanent outputs from saved state. No XP re-award. |
| `Ready` | Valid runtime progression state | Kill credit -> `XpAwarding`; death penalty request -> `XpAdjustmentPending`; save requested -> `ProgressionSaved` | Normal playable state. |
| `XpAwarding` | Valid `PlayerKillCreditEvent` accepted | Threshold crossed -> `LevelingUp`; no threshold -> `Ready`; invalid event -> `Ready` | Applies XP transaction once. |
| `LevelingUp` | XP meets next threshold | More thresholds -> `LevelingUp`; cap reached -> `LevelCapped`; done -> `Ready` | Sequentially increments level and refreshes outputs. |
| `LevelCapped` | Level 10 reached in T1 | Save/load or future tier expansion | Stores capped `total_xp = xp_threshold(10)`; later kill credit emits diagnostics only. |
| `XpAdjustmentPending` | Approved Death & Corpse Recovery request is accepted in a future authored integration | Adjustment applied -> `Ready`; invalid request -> `Ready` with diagnostic | Unreachable in T1 default implementation. Death system owns timing, death context, and policy. |
| `ProgressionSaved` | Save/Load serializes progression state | Save complete -> prior playable state | Writes only whitelist state. |
| `InvalidProgressionState` | Hydration or schema validation fails | Save/Load rejects load | No playable session enabled. |

### Interactions with Other Systems

| System | Character Progression Consumes | Character Progression Provides | Ownership Boundary | Dependency |
|---|---|---|---|---|
| **Character Creation** | `starting_class_id = Cleric` | No output | Character Creation owns first-run identity seed; Progression owns level/XP initialization. | Hard upstream |
| **Save / Load & Persistence** | Hydrated progression payload; `ProgressionSaveBarrier` call before save serialization; load-order call before Combat hydration | `CharacterProgressionSaveState` only after pending progression transactions and same-frame kill-credit dispatch settle; hydration validation result; `ProgressionBaselineSnapshot` for Combat hydration | Progression owns schema, validation, permanent baseline computation, and save-eligible stability. Save/Load owns serialization, HMAC, migration, load ordering, and failure surfacing. | Hard |
| **Combat Core** | `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)`; permanent baseline read requests | `ProgressionBaselineSnapshot`; `XPChangedEvent`; per-level `LevelChangedEvent` | Combat owns approved kill/death event emission, runtime current health/mana, active-zone/playability kill-credit filtering, and combat rules. Progression owns XP, level, lookup/snapshot of XP metadata, and permanent max values. | Hard event boundary |
| **NPC System / Spawn Authoring Data** | Stable source refs represented in `ProgressionXpSourceRefLookup` at build time; NPC source lifecycle activation/death hooks; persisted NPC-owned `NpcSourceLifecycleRecord` data | No progression-owned runtime output to NPC | NPC owns source refs and source lifecycle durability. Progression owns XP metadata, award snapshots, and dedupe; it must not mutate NPC records or infer XP metadata from NPC runtime state after death. | Hard data boundary |
| **Death & Corpse Recovery** | Approved XP adjustment request once authored | `death_xp_debt_preview`; future adjustment integration point | Death system owns death timing, corpse, XP-loss application trigger, resurrection, mitigation, and deleveling policy. Progression owns XP math target state when a future approved request is accepted. | Future hard downstream |
| **Class Design** | Cleric class progression table once authored | Current level and class progression band | Class Design owns class content and final stat tables; Progression owns applying approved tables. | Future hard downstream |
| **Spell Memorization** | None in T1 | `SpellEligibilityChanged`; level eligibility query | Spell system owns memorized slots, spellbook behavior, learned abilities, spell ids, and availability presentation. Progression owns eligibility tier by authored unlock level only. | Future hard downstream |
| **Inventory & Item Economy** | None in T1 | Current level if item requirements later need it | Inventory owns items, equipment legality, drops, currency. Progression does not grant loot. | Future downstream |
| **Faction Reputation** | None | No reputation output | Reputation owns identity ladder. Progression must not mutate faction values. | Boundary only |
| **Zone Control** | None | No zone-control output | Zone Control consumes Combat kill-weight data, not progression XP. | Boundary only |
| **Layer 1 HUD / Audio** | None | `XPChangedEvent`, `LevelChangedEvent`, `LevelCapReached`, `SpellEligibilityChanged` | UI/audio own presentation. Progression owns quiet event timing/data. | Future downstream |

### Progression XP Source Lookup

`ProgressionXpSourceRefLookup` is Character Progression's build-validated XP metadata table. It is generated or authored from NPC/spawn content data before runtime, keyed by the same stable source references Combat Core already emits. It is not a request for Combat Core to change `PlayerKillCreditEvent`.

```yaml
ProgressionXpSourceRefLookupRow:
  zoneId: stable World Structure zone id
  defeated_source_ref: stable Combat source ref
  defeated_level: int
  encounter_role: Trash | Named | Camp
  encounter_role_multiplier: float
  xp_weight_seed_t1: float
  expected_kill_weight_seed_t1: float
  repeatability_class: Repeatable | NonRepeatableFirstKill | RespawnLockout
  source_lifecycle_token_policy: PersistentNpcEpisode | SpawnCycle
  xp_eligible: bool
```

Character Progression owns the authored lookup asset `ProgressionXpSourceRefLookup_T1` and the Editor validator for it. NPC System owns the stable `defeated_source_ref` namespace, source lifecycle records, and the runtime lifecycle hooks that feed the progression registration adapter. This is no longer an open authoring question for T1: XP-awarding implementation is blocked until the authored lookup asset contains rows for every Combat fixture source and the NPC source lifecycle hooks below are present.

Minimum T1 fixture rows:

| Fixture source ref | zoneId | defeated_level | encounter_role | encounter_role_multiplier | xp_weight_seed_t1 | expected_kill_weight_seed_t1 | repeatability_class | lifecycle policy | xp_eligible |
|---|---|---:|---|---:|---:|---:|---|---|---|
| `SoloTrash_EvenCon_T1` | `T1_Haunt` | `5` | `Trash` | `1.0` | `1.25` | `1.25` | `Repeatable` | `SpawnCycle` | `true` |
| `SoloTrash_SoftUndercon_T1` | `T1_Haunt` | `4` | `Trash` | `1.0` | `1.25` | `1.25` | `Repeatable` | `SpawnCycle` | `true` |
| `SoloTrash_Trivial_T1` | `T1_Haunt` | `3` | `Trash` | `1.0` | `1.25` | `1.25` | `Repeatable` | `SpawnCycle` | `true` |
| `Named_XP_Smoke_T1` | `T1_Haunt` | `6` | `Named` | `3.0` | `1.25` | `1.25` | `RespawnLockout` | `PersistentNpcEpisode` | `true` |

At runtime, Character Progression owns an `XpSourceLifecycleRegistry` populated from these rows by its registration adapter. NPC/spawn activation and Combat actor-claim boundaries must be observable by that adapter before the source can award XP; this does not add fields to Combat Core events and does not make Combat Core responsible for XP metadata. The registry never resolves XP against a replaced current occupant at the same source ref. Award resolution uses an immutable snapshot captured on the same event-dispatch frame as Combat's kill-credit delivery, before source cleanup can remove the defeated lifecycle or a respawn can rotate the active token.

```yaml
XpSourceLifecycleRegistryEntry:
  zoneId: stable World Structure zone id
  defeated_source_ref: stable Combat source ref
  source_lifecycle_token: opaque local lifecycle token
  defeated_level: int
  encounter_role: Trash | Named | Camp
  encounter_role_multiplier: float
  xp_weight_seed_t1: float
  expected_kill_weight_seed_t1: float
  repeatability_class: Repeatable | NonRepeatableFirstKill | RespawnLockout
  xp_eligible: bool
  lifecycle_state: Active | DefeatedTombstone

XpAwardResolutionSnapshot:
  zoneId: stable World Structure zone id
  defeated_source_ref: stable Combat source ref
  source_lifecycle_token: opaque local lifecycle token from defeated lifecycle
  defeated_level: int
  encounter_role: Trash | Named | Camp
  encounter_role_multiplier: float
  xp_weight_seed_t1: float
  expected_kill_weight_seed_t1: float
  repeatability_class: Repeatable | NonRepeatableFirstKill | RespawnLockout
  xp_eligible: bool
  lifecycle_state: DefeatedTombstone
```

For persistent NPCs, `source_lifecycle_token` identifies the current defeat/availability episode for the `source_npc_id`. For non-persistent creature or ambient spawns, it identifies the active spawn cycle for the `source_spawn_ref` at its spawn anchor. The token is not a Combat runtime id and is not persisted as progression state. `xp_weight_seed_t1` is the progression-owned XP pacing weight used by the XP formula. Combat's `kill_weight_seed` is still consumed from `PlayerKillCreditEvent` and compared to `expected_kill_weight_seed_t1` in T1 fixtures, but it does not directly drive XP amount. `xp_eligible = false` is a hard zero-XP gate: the kill-credit event is consumed, a development diagnostic emits, no `XPChangedEvent` emits, and no threshold check occurs.

Lifecycle registry entries must follow this lifetime:

1. **Active registration before eligibility.** A source cannot award XP until the adapter has registered an `Active` entry with all required metadata and a lifecycle token.
2. **Defeat snapshot barrier.** When Combat's death resolution emits `PlayerKillCreditEvent`, Character Progression must synchronously resolve the active entry into one `XpAwardResolutionSnapshot` and convert the entry to `DefeatedTombstone` before NPC/spawn cleanup may retire the source lifecycle. NPC System's `NpcSourceLifecycleRecord` death update and Character Progression's award snapshot are captured in the same kill-resolution phase; NPC cleanup/despawn may run only after both systems have acknowledged the phase.
3. **Respawn isolation.** A respawn or new NPC availability episode may create a new `Active` entry only with a new `source_lifecycle_token`. It must not replace, mutate, or reuse the defeated tombstone for the prior lifecycle.
4. **Retention.** Defeated tombstones and processed dedupe keys remain queryable until `xp_source_lookup_retention_seconds` elapses or until all same-dispatch kill-credit subscribers have acknowledged source cleanup, whichever is later. They may be retained for the rest of the playable session.
5. **Stale duplicate handling.** A duplicate event for a defeated lifecycle resolves to the retained tombstone and processed dedupe key, so it cannot award twice. A late duplicate with no retained tombstone is rejected as stale; it must never resolve against a new active respawn token.

Registry entries, tombstones, and processed dedupe keys are not saved because Combat kill-credit events are not replayed by Save/Load. Durable source death/respawn state belongs to NPC System's persisted `NpcSourceLifecycleRecord`; Character Progression save state persists only XP/level/spell eligibility.

### Event Payloads and Public Queries

Character Progression locks these payloads for T1. Payloads must not include faction rank, reputation delta, patron id, political access, faction-visual fields, spell ids, learned ability ids, combat actor ids, threat tables, damage rolls, or runtime resource-current values.

```yaml
XPChangedEvent:
  local_character_id: stable T1 player save identity
  xp_transaction_id: local progression transaction id
  reason: KillCredit | DeathAdjustment
  previous_total_xp: int
  new_total_xp: int
  xp_delta: int
  current_level: int
  visible_level: int
  current_level_xp_progress: int
  current_level_xp_band: int
```

`XPChangedEvent.xp_delta` is the applied persisted XP delta after cap clamp, not the raw award request. `previous_total_xp + xp_delta == new_total_xp` must hold. `current_level`, `visible_level`, `current_level_xp_progress`, and `current_level_xp_band` are the final post-transaction values after all level steps have been computed, even though `XPChangedEvent` is the first notification emitted to subscribers. A cap-crossing award that would add `300` XP from `11,600` stores `11,710`, emits `xp_delta = 110`, and carries `current_level = 10`, `visible_level = 10`, `current_level_xp_progress = 0`, and `current_level_xp_band = 0`; an already-at-cap award emits no event. `XPChangedEvent` emits only when `xp_delta != 0`. T1 emits `reason = KillCredit` only; `DeathAdjustment` is reserved for the future approved Death & Corpse Recovery integration. Trivial kills, `xp_eligible = false`, malformed events, unresolved lookup rows, duplicate dedupe keys, at-cap kills, and rejected death-adjustment previews emit diagnostics in development builds but no `XPChangedEvent`. `xp_transaction_id` is a per-character, monotonic, local-session id; it is not persisted.

```yaml
LevelChangedEvent:
  local_character_id: stable T1 player save identity
  xp_transaction_id: local progression transaction id
  previous_level: int
  new_level: int
  visible_level: int
  total_xp_after: int
  permanent_max_health_after: int
  permanent_max_mana_after: int
  spell_eligibility_tier_before: int
  spell_eligibility_tier_after: int
```

Multi-level transactions emit one `LevelChangedEvent` per level step in ascending order. A transaction from level 2 to level 5 emits 2->3, 3->4, and 4->5 events, each after the permanent outputs for that new level are computed.

```yaml
LevelCapReached:
  local_character_id: stable T1 player save identity
  xp_transaction_id: local progression transaction id
  cap_level: int
  total_xp_after: int
```

`LevelCapReached` emits only on the first transaction that reaches `t1_level_cap`. Later at-cap kill credit emits development diagnostics only.

```yaml
SpellEligibilityChanged:
  local_character_id: stable T1 player save identity
  xp_transaction_id: local progression transaction id
  previous_spell_eligibility_tier: int
  new_spell_eligibility_tier: int
  unlock_level: int
  unlock_source: AuthoredUnlockList
```

Spell eligibility authority is the authored `spell_tier_unlock_levels_t1` list. The query returns the highest eligible tier whose unlock level is less than or equal to `current_level`; it never creates spell content, learned abilities, spellbook records, vendors, drops, memorized slots, UI buttons, VFX, or spell effects.

Within one XP transaction, event ordering is fixed: preflight the transaction guard; reject the whole transaction before mutation if it would exceed `max_levels_per_xp_transaction`; compute `applied_xp_delta` after cap clamp; precompute the final level chain and permanent outputs; atomically commit the final XP/level/spell-eligibility state; emit `XPChangedEvent` first if `applied_xp_delta != 0`; then emit one `LevelChangedEvent` per level step in ascending order; emit `SpellEligibilityChanged` immediately after the `LevelChangedEvent` for the step that changed eligibility; after the final level step, emit `LevelCapReached` if the transaction reached the cap for the first time. Subscribers must treat event payloads as authoritative for the transaction and must not infer mid-transaction state by reading live progression during event dispatch. UI/audio subscribers must treat `SpellEligibilityChanged` and `LevelCapReached` as system-facing notifications until their owning downstream GDDs define player-facing presentation.

`ProgressionSaveBarrier` is the synchronous readiness hook Save/Load must call before serializing Character Progression. It returns only after same-frame Combat kill-credit dispatch has either produced a valid XP transaction, rejected the event, or confirmed no progression-relevant event is pending. Save/Load receives `CharacterProgressionSaveState` only from `InitializedLevel1`, `Ready`, or `LevelCapped`; pending `XpAwarding`, `LevelingUp`, `XpAdjustmentPending`, or unacknowledged kill-credit dispatch is not save-eligible. If the barrier cannot settle by `progression_save_barrier_max_ms` or before the caller's stricter transition-save deadline, it returns `ProgressionSaveBarrierUnresolved`; Save/Load must fail the write loudly rather than serializing stale or partial XP.

## Formulas

Formula rounding conventions:

- `RoundTo10(x) = 10 * round(x / 10, halves up)`
- `RoundTo5(x) = 5 * round(x / 5, halves up)`
- `clamp(value, min, max)` returns `min` if below range, `max` if above range, otherwise `value`.

The `xp_threshold` formula is defined as:

`xp_threshold(level) = level == 1 ? 0 : RoundTo10((xp_base * (level - 1)^xp_exponent) + (xp_linear * (level - 1)))`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `level` | `L` | int | 1-60; T1 playable 1-10 | Character level being evaluated. |
| `xp_base` | `B` | int | 80-140; default 100 | Main XP curve scale. |
| `xp_exponent` | `E` | float | 1.90-2.35; default 2.15 | Curve steepness. |
| `xp_linear` | `X` | int | 20-80; default 50 | Early-level smoothing term. |

**Output Range:** T1 cumulative threshold range is `0-11,710` XP. Future 1-60 output is formula-supported but not T1-balanced.

**Default T1 Thresholds:**

| Level | Total XP Required |
|---:|---:|
| 1 | 0 |
| 2 | 150 |
| 3 | 540 |
| 4 | 1,210 |
| 5 | 2,170 |
| 6 | 3,430 |
| 7 | 5,010 |
| 8 | 6,910 |
| 9 | 9,140 |
| 10 | 11,710 |

**Example:** `xp_threshold(10) = RoundTo10((100 * 9^2.15) + (50 * 9)) = 11,710`.

The level-state query formulas are defined as:

`uncapped_level_candidate = highest_level_where(xp_threshold(level) <= total_xp)`

`current_level = min(uncapped_level_candidate, t1_level_cap)`

`visible_level = current_level`

If `current_level < t1_level_cap`:

`current_level_xp_progress = total_xp - xp_threshold(current_level)`

`current_level_xp_band = xp_threshold(current_level + 1) - xp_threshold(current_level)`

`next_level_xp_threshold = current_level_xp_band` as a compatibility alias; new UI code should use `current_level_xp_band` to avoid confusing band size with a cumulative threshold.

If `current_level == t1_level_cap`:

`current_level_xp_progress = 0`

`current_level_xp_band = 0`

`next_level_xp_threshold = 0`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `total_xp` | `TXP` | int | 0 to `xp_threshold(t1_level_cap)` in T1 | Persisted total XP. T1 never stores over-cap XP. |
| `current_level` | `CL` | int | 1-10 T1 | Mechanically active level after cap. |
| `visible_level` | `VL` | int | 1-10 T1 | Player-facing level; equals `current_level` in T1. |
| `current_level_xp_progress` | `CLXP` | int | 0 to current band size; 0 at cap | XP earned inside the current visible level band. |
| `current_level_xp_band` | `CLB` | int | 0 to current band size; 0 at cap | XP required inside the current band before the next visible level. |
| `next_level_xp_threshold` | `NLXT` | int | 0 to current band size; 0 at cap | Compatibility alias for `current_level_xp_band`; not a cumulative threshold. |

**Example:** At level 5 with `total_xp = 2,500`, `current_level_xp_progress = 2,500 - 2,170 = 330` and `current_level_xp_band = 3,430 - 2,170 = 1,260`.

The `level_difference_modifier` formula is defined as:

`level_delta = defeated_level - player_level`

`level_difference_modifier = level_delta <= trivial_cutoff ? 0 : clamp(1 + (diff_step * level_delta), min_nontrivial_modifier, max_level_difference_modifier)`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `player_level` | `PL` | int | 1-10 T1; 1-60 future | Current player level. |
| `defeated_level` | `DL` | int | 1-10 T1; 1-60 future | Defeated source level from authored NPC/spawn data. |
| `trivial_cutoff` | `TC` | int | -6 to -4; default -4 | Level delta at or below which a kill awards 0 XP. |
| `diff_step` | `DS` | float | 0.10-0.20; default 0.15 | Modifier change per level difference. |
| `min_nontrivial_modifier` | `MIN` | float | 0.10-0.50; default 0.25 | Lowest modifier before trivial cutoff. |
| `max_level_difference_modifier` | `MAX` | float | 1.25-2.00; default 1.60 | Highest modifier for above-level kills. |

**Output Range:** `0.00-1.60` in T1 defaults.

**Example:** Level 5 player defeats level 3 source: `level_delta = -2`; modifier is `clamp(1 + (0.15 * -2), 0.25, 1.60) = 0.70`.

The `xp_award` formula is defined as:

`raw_xp_award = floor(base_xp_per_weight * xp_weight_seed_t1 * level_difference_modifier * encounter_role_multiplier)`

`effective_max_xp_per_kill = min(max_xp_per_kill, floor(current_level_xp_band * max_xp_per_kill_pct_of_level_band))`

`xp_award_uncapped = level_difference_modifier == 0 ? 0 : max(xp_award_minimum_nontrivial, raw_xp_award)`

`xp_award = current_level == t1_level_cap ? 0 : min(effective_max_xp_per_kill, xp_award_uncapped)`

`applied_xp_delta = min(xp_award, xp_threshold(t1_level_cap) - stored_total_xp_before)`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `base_xp_per_weight` | `BW` | int | 35-75; default 50 | Conversion scale from progression XP weight to XP. |
| `xp_weight_seed_t1` | `XW` | float | Expected T1 0.25-6.0 | Progression-owned XP pacing weight from `ProgressionXpSourceRefLookup`; decoupled from Combat's political/combat kill-weight semantics. |
| `kill_weight_seed` | `KW` | float | Expected T1 0.25-6.0 | Combat Core kill-weight seed from `PlayerKillCreditEvent`; consumed for fixture validation against `expected_kill_weight_seed_t1`, not used as the XP formula multiplier. |
| `level_difference_modifier` | `LDM` | float | 0.00-1.60 | Output of `level_difference_modifier`. |
| `encounter_role_multiplier` | `ERM` | float | Trash 1.0; Named 3.0; Camp 1.0 defaults | Authored encounter role multiplier; named safe range 2.0-4.0. |
| `xp_award_minimum_nontrivial` | `XMIN` | int | 1-10; default 1 | Minimum XP for a valid non-trivial kill after flooring. Ignored when `LDM == 0`. |
| `max_xp_per_kill` | `MXK` | int | 300-900; default 600 | Per-kill XP ceiling. |
| `max_xp_per_kill_pct_of_level_band` | `MXPCT` | float | 0.15-0.35; default 0.25 | Secondary cap preventing one kill from skipping too much of the current level band. |
| `current_level_xp_band` | `CLB` | int | 0+ | Current level band size from the level-state query. |
| `stored_total_xp_before` | `SXP0` | int | 0 to cap | Persisted XP before this transaction; used to compute the cap-clamped applied delta. |

**Output Range:** `0-600` XP per kill with T1 defaults.

**Example:** Level 5 Cleric defeats a level 6 trash source with `xp_weight_seed_t1 = 1.25`: `raw_xp_award = floor(50 * 1.25 * 1.15 * 1.0) = 71`, `effective_max_xp_per_kill = min(600, floor(1,260 * 0.25)) = 315`, so `xp_award = 71 XP`.

The `permanent_max_resources` formula is defined as:

`n = level - 1`

`permanent_max_health = RoundTo5(base_health + health_linear * n + health_quadratic * n^2)`

`permanent_max_mana = RoundTo5(base_mana + mana_linear * n + mana_quadratic * n^2)`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `level` | `L` | int | 1-10 T1; 1-60 future | Character level. |
| `base_health` | `BH` | int | 70-90; default 80 | Level 1 Cleric permanent max health. |
| `base_mana` | `BM` | int | 90-110; default 100 | Level 1 Cleric permanent max mana. |
| `health_linear` | `HL` | float | default `131 / 9` | Linear health growth term derived from Combat fixtures. |
| `health_quadratic` | `HQ` | float | default `1 / 9` | Health curve term derived from Combat fixtures. |
| `mana_linear` | `ML` | float | default `164 / 9` | Linear mana growth term derived from Combat fixtures. |
| `mana_quadratic` | `MQ` | float | default `4 / 9` | Mana curve term derived from Combat fixtures. |

**Output Range:** T1 health `80-220`; T1 mana `100-300`.

**Example:** Level 5 gives `n = 4`; health `RoundTo5(80 + (131/9 * 4) + (1/9 * 16)) = 140`; mana `RoundTo5(100 + (164/9 * 4) + (4/9 * 16)) = 180`, matching Combat Core's level-5 Cleric fixture.

The `spell_eligibility_tier` query is defined against the authored unlock list:

`spell_eligibility_tier = highest_tier_where(spell_tier_unlock_levels_t1[tier] <= current_level)`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `current_level` | `CL` | int | 1-10 T1 | Current mechanically active Cleric level. |
| `spell_tier_unlock_levels_t1` | `STUL` | int list | default `[1, 3, 5, 7, 9]` | Authoritative 1-indexed tier-to-level map. |
| `max_t1_spell_tier` | `MST` | int | 3-5; default 5 | Highest T1 eligibility tier. |

**Output Range:** T1 eligibility tier `1-5`. This is eligibility only, not spell content or memorization.

**Example:** Level 7 Cleric with `[1, 3, 5, 7, 9]` returns tier `4`, because tier 4 unlocks at level 7 and tier 5 unlocks at level 9.

The `death_xp_debt_preview` formula is defined as:

`band_xp = level < t1_level_cap ? xp_threshold(level + 1) - xp_threshold(level) : xp_threshold(level) - xp_threshold(level - 1)`

`death_xp_debt_preview = level < death_penalty_min_level ? 0 : RoundTo10(band_xp * death_debt_pct)`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `level` | `L` | int | 1-10 T1 | Level at death or penalty preview time. |
| `t1_level_cap` | `CAP` | int | fixed 10 in T1 | Visible T1 level cap. |
| `death_penalty_min_level` | `DML` | int | 4-6; default 4 | Below this level, previewed XP debt is 0. |
| `death_debt_pct` | `DDP` | float | 0.04-0.12; default 0.08 | Percent of current level band used as debt preview. |
| `xp_threshold` | `XT` | formula | see above | XP threshold function. |

**Output Range:** T1 default preview range `0-210` XP debt. Death & Corpse Recovery owns whether and when this preview becomes an applied penalty.

**Example:** Level 7 band is `xp_threshold(8) - xp_threshold(7) = 6,910 - 5,010 = 1,900`; debt preview is `RoundTo10(1,900 * 0.08) = 150`.

The reserved death XP adjustment contract is:

```yaml
ProgressionXpAdjustmentRequest:
  local_character_id: stable T1 player save identity
  death_context_id: globally unique death/corpse context id from Death & Corpse Recovery
  requested_xp_delta: negative int
  adjustment_policy_ref: approved Death & Corpse Recovery policy id
  allow_delevel: bool
  reversal_context_id: optional unique mitigation/reversal id
```

Character Progression accepts this request only after Death & Corpse Recovery has an approved GDD policy. It must reject duplicate `death_context_id`, missing policy refs, non-negative deltas, requests for the wrong character, requests that would delevel when `allow_delevel = false`, and any request before the approved downstream policy exists. Accepted future adjustments use the same atomic preflight/event/save barrier as kill-credit XP; T1 persists no XP debt and applies no death XP loss by default.

The `cap_clamp` formula is defined as:

`stored_total_xp = stored_total_xp_before + applied_xp_delta`

`current_level = min(highest_level_where(xp_threshold(level) <= stored_total_xp), t1_level_cap)`

`visible_level = current_level`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `stored_total_xp_before` | `SXP0` | int | 0 to `xp_threshold(t1_level_cap)` in T1 | Persisted total XP before the transaction. T1 never stores over-cap XP. |
| `stored_total_xp` | `SXP` | int | 0 to `xp_threshold(t1_level_cap)` in T1 | Persisted total XP after the transaction. T1 never stores over-cap XP. |
| `xp_award` | `XA` | int | 0-600 T1 default | Output of `xp_award`. |
| `applied_xp_delta` | `AXD` | int | 0-600 T1 default | Actual persisted XP change after cap clamp; this is `XPChangedEvent.xp_delta`. |
| `current_level` | `CL` | int | 1-10 T1 | Mechanically active level after cap. |
| `visible_level` | `VL` | int | 1-10 T1 | Displayed level; equals `current_level` in T1. |
| `t1_level_cap` | `CAP` | int | fixed 10 in T1 | T1 cap. |

**Output Range:** `visible_level` remains `1-10`; persisted `stored_total_xp` never exceeds `xp_threshold(10) = 11,710` in T1.

**Example:** Level 10 character at `11,710` XP earns `300` XP. `applied_xp_delta = 0`; stored total remains `11,710`; visible level remains `10`; no `XPChangedEvent` emits. Level 9 at `11,600` earning `300` XP applies `110` XP, stores `11,710`, emits `XPChangedEvent.xp_delta = 110`, and then emits the level/cap events.

## Edge Cases

- **If duplicate `PlayerKillCreditEvent` arrives for the same defeated source lifecycle**: resolve it against the retained `DefeatedTombstone`, find the same `XpAwardDedupeKey`, award XP once, emit a duplicate-event diagnostic, and ignore subsequent duplicates. The required dedupe key is `local_character_id + zoneId + defeated_source_ref + source_lifecycle_token`; there is no "if available" fallback and `combat_actor_id` is never legal. If the same spawn anchor later respawns, the lookup must expose a new `source_lifecycle_token`, while the prior tombstone remains separately retained so an old duplicate cannot bind to the new respawn token.

- **If `XpSourceLifecycleRegistry` cannot resolve an immutable `XpAwardResolutionSnapshot` with `defeated_level`, `encounter_role`, `encounter_role_multiplier`, `xp_weight_seed_t1`, `xp_eligible`, expected T1 `kill_weight_seed`, and `source_lifecycle_token` for a kill-credit event**: reject the XP award with a development diagnostic and leave state unchanged. Progression does not ask Combat Core for missing fields, does not guess from runtime combat state, and does not resolve against a newer active respawn entry.

- **If the lookup row has `xp_eligible = false`**: award 0 XP, emit no `XPChangedEvent`, emit a development diagnostic with reason `XpSourceIneligible`, and perform no threshold check.

- **If Combat Core's event `kill_weight_seed` does not match the award snapshot's `expected_kill_weight_seed_t1` for an authored fixture source**: reject the XP award with a development diagnostic. This keeps T1 fixture data drift-verifiable while preserving Combat Core's approved event shape.

- **If `PlayerKillCreditEvent` arrives near player death timing**: Character Progression does not inspect death state or infer XP from corpse-run state. It processes only valid Combat Core kill-credit events delivered while progression is initialized and the source lifecycle is active; Death & Corpse Recovery may later request an approved XP adjustment once its GDD defines the required death context.

- **If Combat Core does not emit kill credit because a defeated source is outside its playable active-zone/death-resolution boundary**: Character Progression does nothing. Character Progression does not subscribe to World Structure transition events, zone residency, or transition state. If Combat Core emits a valid `PlayerKillCreditEvent`, Character Progression treats Combat as the authority that the event passed Combat's own playability and kill-credit filters, then applies only the progression lookup/dedupe rules in this GDD.

- **If `PlayerKillCreditEvent` arrives before Character Progression has initialized or hydrated**: ignore the event, emit a diagnostic, and award no XP. XP cannot be queued against an unvalidated progression state.

- **If a save trigger arrives while Character Progression is in `XpAwarding`, `LevelingUp`, `XpAdjustmentPending`, or while a same-frame Combat kill-credit dispatch is unresolved**: finish the transient progression transaction or reject the pending event first, then expose the post-resolution stable state through `ProgressionSaveBarrier`. Save serialization is valid only from `InitializedLevel1`, `Ready`, or `LevelCapped`; no pending kill credit, mid-award, mid-level-chain, or partially applied XP adjustment state may be persisted. If the barrier cannot settle by `progression_save_barrier_max_ms`, Save/Load receives `ProgressionSaveBarrierUnresolved` and fails the write. Tests use `progression_transaction_test_latch` and a `kill_credit_dispatch_test_latch` to hold transient states open deterministically.

- **If a save trigger arrives during the same kill-resolution phase that awarded XP**: Save/Load must wait for both Character Progression's `ProgressionSaveBarrier` and NPC System's `NpcSourceLifecycleSaveBarrier`. XP cannot be serialized unless the NPC-owned `NpcSourceLifecycleRecord` for the defeated source is also save-stable. Character Progression does not persist its own tombstone to solve this race.

- **If `PlayerKillCreditEvent.defeated_source_ref` is missing, empty, or an unrecognized stable-source type**: reject the event receive-side, emit a diagnostic, do not transition state, and award no XP. T1 legal XP source refs are the stable forms Combat Core uses for NPC death kill credit: persistent NPC source or non-persistent spawn source.

- **If `kill_weight_seed` is missing, NaN, negative, zero, or outside the expected T1 authored range**: reject the XP award, emit a diagnostic, and leave state unchanged. Do not clamp invalid upstream payloads silently.

- **If a kill is trivial by `level_difference_modifier`**: award 0 XP, emit no `XPChangedEvent`, emit a development diagnostic with reason `TrivialKill`, and do not advance thresholds. Reputation and Zone Control may still consume their own Combat events independently.

- **If one XP award crosses multiple level thresholds**: resolve each level sequentially in one transaction, emit one `LevelChangedEvent` per level gained in ascending order, refresh permanent progression outputs after each level, and persist only the final stable state. Batched level-change events are not used in T1.

- **If preflight detects that one XP transaction would advance more than `max_levels_per_xp_transaction`**: reject the entire transaction before mutating XP, level, spell eligibility, or processed dedupe state. Emit a development diagnostic and no progression events. This guard is fail-closed; it never serializes a partially applied oversized transaction.

- **If an XP award crosses the T1 cap**: clamp `total_xp` to `xp_threshold(10)`, set `visible_level = 10`, emit `LevelCapReached` if this is the first cap crossing, and grant no additional health, mana, spell eligibility, title, faction value, banked XP, or hidden benefit. Later kills at cap award 0 XP and emit diagnostics only.

- **If level-up increases permanent max health or max mana during combat**: publish updated permanent maxima for Combat Core to consume, but do not mutate `current_health`, `current_mana`, threat, cast, regen, med-break, or death state. Combat owns runtime current-resource handling.

- **If Save/Load hydrates Combat before Character Progression**: the load sequence is invalid. Character Progression must validate first and publish `ProgressionBaselineSnapshot`; Combat actor hydration then validates `current_health` and `current_mana` against the snapshot. Progression never persists or repairs Combat current resources.

- **If Save/Load hydrates class id other than `Cleric`, level below 1, level above 10 in T1, negative `total_xp`, XP outside the persisted level band, `total_xp` above `xp_threshold(10)`, inconsistent spell eligibility tier, unknown `progression_schema_version`, missing authored formula config for that schema version, or missing authored spell unlock data**: return progression hydration failure to Save/Load. `progression_schema_version` is the only persisted progression version field and selects the compatible formula/config contract; no separate formula-version field is saved. For non-cap saves, valid XP must satisfy `xp_threshold(current_level) <= total_xp < xp_threshold(current_level + 1)`; at cap, valid XP must equal `xp_threshold(10)`. Do not silently clamp, downgrade, recalculate from defaults, or enter playable state.

- **If saved `total_xp` exceeds the level-10 threshold in T1**: reject hydration. T1 has no banked over-cap XP.

- **If Death & Corpse Recovery requests an XP penalty before it is authored or without a valid death context**: reject the request with a diagnostic and leave XP unchanged. Character Progression exposes the penalty formula and adjustment interface only; it does not invent death timing. Durable idempotency for applied death contexts belongs to Death & Corpse Recovery when that system is authored; Character Progression persists no applied death-context ids in T1.

- **If a death XP adjustment would reduce `total_xp` below the current level threshold**: do not apply it in this GDD. Until Death & Corpse Recovery is authored, Character Progression exposes preview values only and persists no XP debt. Deleveling is a Death & Corpse Recovery policy decision, not a Character Progression rule.

- **If `SpellEligibilityChanged` fires before Class Design or Spell Memorization exists**: persist only the eligibility tier and emit the event for future subscribers, but do not create spell records, spell ids, learned abilities, spellbook entries, memorized slots, vendors, drops, UI buttons, or spell effects.

- **If a kill-credit event contains `faction_id`**: ignore it for progression mutation. Character Progression may pass through the original event identity in diagnostics, but it must not award reputation, faction rank, title, patron access, political state, or faction visual changes.

## Dependencies

Character Progression's dependency surface is intentionally narrow. It owns the mechanical class ladder, but it must not become a hidden dependency bridge into faction, world-state, combat-runtime, or account systems. Dependency direction matters: most systems below consume Character Progression outputs later; Character Progression does not mutate them.

Each downstream GDD listed below must declare Character Progression in its own Dependencies section when authored, with hard/soft classification matching this section unless that later GDD explicitly supersedes the relationship and surfaces the conflict. `/consistency-check` and `/review-all-gdds` should verify bidirectional agreement.

### Hard Direct Upstream

| System | Direction | Data Interface | Contract |
|---|---|---|---|
| **Character Creation** | Character Progression consumes | `starting_class_id = Cleric`; no separate T1 level/XP seed required | Character Creation owns first-run identity/class seed. Character Progression owns level 1 / 0 XP initialization. |
| **Save / Load & Persistence** | Bidirectional persistence client | `ProgressionSaveBarrier`; `CharacterProgressionSaveState`; hydration validation result; save serialization from stable progression states only; `ProgressionBaselineSnapshot` before Combat hydration | Character Progression owns schema, validation, permanent baseline computation, and save-eligible stability. Save/Load owns serialization, HMAC, versioning, migration, write timing, load ordering, and failure surfacing. |

### Hard Event Boundary

| System | Direction | Data Interface | Contract |
|---|---|---|---|
| **Combat Core** | Combat emits; Character Progression consumes and provides read-only baselines | Consumes approved `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)`; provides current level, permanent max health/mana, and level-facing baseline snapshot | Combat owns kill/death event emission, active-zone/playability kill-credit filtering, runtime current resources, damage, threat, casting, regen, and death. Progression owns XP, level, XP-source lookup/snapshot, spell eligibility, and permanent level-derived outputs. |

### Build-Time Data Boundary

| Source | Direction | Data Interface | Contract |
|---|---|---|---|
| **NPC System / Spawn Authoring Data** | Character Progression validates against NPC-authored source refs and consumes NPC source lifecycle hooks | `ProgressionXpSourceRefLookup(zoneId, defeated_source_ref) -> defeated_level, encounter_role, encounter_role_multiplier, xp_weight_seed_t1, expected_kill_weight_seed_t1, repeatability_class, source_lifecycle_token_policy, xp_eligible`; NPC-owned `NpcSourceLifecycleRecord` durability | Lookup rows are progression-owned XP metadata keyed to Combat-stable refs. NPC System owns source refs, lifecycle hooks, and durable source state. Runtime Progression owns the `XpSourceLifecycleRegistry` registration/snapshot/tombstone seam and does not require new Combat event fields or NPC runtime mutation. |

### Future Hard Downstream

| System | Direction | Data Interface | Contract |
|---|---|---|---|
| **Class Design** | Class Design consumes Character Progression | Current level, class progression band, permanent baseline tables once authored | Class Design owns class content and final Cleric/Warrior/Enchanter class tables. Character Progression applies approved tables. |
| **Spell Memorization** | Spell Memorization consumes Character Progression | `SpellEligibilityChanged`; level eligibility query | Spell Memorization owns memorized slots, spellbook behavior, learned abilities, spell ids, and availability presentation. Progression owns eligibility tier by authored unlock list only. |
| **Death & Corpse Recovery** | Death system consumes Character Progression policy and later requests adjustment | `death_xp_debt_preview`; reserved `ProgressionXpAdjustmentRequest` | Death system owns death timing, corpse recovery, resurrection, mitigation, deleveling policy, and penalty application. Progression owns XP target math only when called through a future approved interface. |
| **Faction Reputation** | Faction Reputation may consume Character Progression | Current level only as combat-safety advisory context if later approved | Faction Reputation owns reputation values, identity ladder, rank labels, faction trust, political access, and display. Character level must never be the primary gate for rank, trust, patron access, title, intimacy, or political authority. |

### Future Optional Consumers

| System | Direction | Data Interface | Contract |
|---|---|---|---|
| **Inventory & Item Economy** | Optional future consumer | Current level if item requirements later need it | Inventory owns items, equipment legality, drops, currency, and loot. Progression does not grant items. |
| **Layer 1 HUD** | UI consumes progression events and queries | `visible_level`, current-band XP progress queries, `XPChangedEvent`, `LevelChangedEvent`, `LevelCapReached`, system-facing `SpellEligibilityChanged` | HUD owns visual presentation and accessibility; Progression owns quiet event timing and data payloads. |
| **Audio System** | Audio consumes progression events | `LevelChangedEvent`, optionally `LevelCapReached` | Audio owns playback and mix. Progression emits only restrained hooks. |

### Boundary-Only / Non-Dependencies

| System | Relationship | Explicit Non-Contract |
|---|---|---|
| **World Structure** | Boundary only | Character Progression does not consume `ZoneActiveEvent`, `SessionResumeEvent`, `CorpseRecord`, zone transition state, or zone residency data. XP arrives through Combat Core kill-credit events only. |
| **Zone Control** | Boundary only | Zone Control consumes Combat kill-weight data for ownership math. Character Progression does not produce zone-control values and does not read zone-control output. |
| **Faction State Simulation** | Boundary only | Character Progression does not advance faction state, event logs, between-session simulation, or political outcomes. |
| **Dialogue System / LLM Dialogue** | Boundary only | Character Progression does not create dialogue memory, NPC recognition, LLM prompt context, or templated dialogue variants. |
| **Named AI Companion Core / Sister Elara Mentor** | Boundary only in T1 | Character Progression does not grant companion authority, companion XP, companion relationship state, or onboarding progression. |
| **Network Architecture / Authentication & Accounts** | Boundary only in T1 | No account progression, server authority, replicated XP, online identity, or networking placeholder exists in T1. |
| **Social Systems** | Boundary only in T1 | Character Progression does not grant guild/cabal status, chat identity, party authority, or social ranking. |

### Reverse-Listing Obligations

- Save / Load already lists Character Progression as a hard persistence client and must remain consistent with this GDD's persistence whitelist: `progression_schema_version`, `class_id`, `current_level`, `total_xp`, and `spell_eligibility_tier` only. Save/Load must call `ProgressionSaveBarrier` before serialization, hydrate Character Progression before Combat actor hydration, and must not treat Character Progression as the owner of XP debt, spell content, learned abilities, memorized slots, Combat current resources, or derived max-resource caches.
- Combat Core owns the approved kill/death hooks and permanent-baseline handoff. If Combat Core is revised again, it should continue naming Character Progression as the owner of XP, level, permanent max health/mana, XP-source lookup, and spell eligibility without adding progression-only fields to `PlayerKillCreditEvent`.
- Character Creation already lists Character Progression as a downstream seed consumer. If Character Creation is revised, it should keep the boundary that it seeds `Cleric` only and does not own XP/level schema.
- Future Class Design, Spell Memorization, Death & Corpse Recovery, Faction Reputation, Layer 1 HUD, and Audio GDDs must reverse-list the relevant Character Progression interface when authored.

## Tuning Knobs

| Knob | Default | Safe Range | Too Low / Narrow | Too High / Broad |
|---|---:|---|---|---|
| `t1_level_cap` | `10` | Fixed at 10 for T1 | Undercuts the approved T1 progression band and Combat fixture spread. | Expands T1 scope and invalidates T1 haunt balance. |
| `xp_base` | `100` | `80-140` | Levels arrive too quickly unless XP awards are also lowered. | T1 becomes grind-heavy before camp feel is proven. |
| `xp_exponent` | `2.15` | `1.90-2.35` | Late T1 levels flatten and lose EQ-style pacing. | Level 8-10 may become punitive for the vertical slice. |
| `xp_linear` | `50` | `20-80` | Early levels feel abrupt and threshold spacing may spike. | Early levels become too smooth and less meaningful. |
| `base_xp_per_weight` | `50` | `35-75` | Kill-credit pacing slows and levels may require too many repeated pulls. | XP awards overtake the threshold curve and compress T1 leveling. |
| `max_xp_per_kill` | `600` | `300-900` | Named kills may feel unrewarding relative to risk. | Single kills can skip too much of a level band. |
| `max_xp_per_kill_pct_of_level_band` | `0.25` | `0.15-0.35` | Named and above-level kills feel under-rewarded near a fresh level. | Single kills can skip too much of the current level band and break EQ-slow pacing. |
| `xp_award_minimum_nontrivial` | `1` | `1-10` | Valid non-trivial kills can floor to 0 after modifiers, making the formula feel broken. | Low-value farming stays relevant too long. Trivial kills still award 0 regardless of this knob. |
| `trivial_cutoff` | `-4` | `-6` to `-4` | Low-level kills remain farmable too long. | Slightly under-level enemies become unrewarding too quickly. |
| `diff_step` | `0.15` | `0.10-0.20` | Level difference barely matters. | Above-level kills become too efficient; below-level kills collapse too fast. |
| `min_nontrivial_modifier` | `0.25` | `0.10-0.50` | Non-trivial low-level kills feel indistinguishable from trivial kills. | Low-level farming remains too efficient. |
| `max_level_difference_modifier` | `1.60` | `1.25-2.00` | Above-level risk is under-rewarded. | Players may optimize reckless above-level kills if Combat tuning permits. |
| `encounter_role_multiplier_trash` | `1.0` | `0.8-1.2` | Normal camp XP feels weak. | Trash farming compresses the level curve. |
| `encounter_role_multiplier_named` | `3.0` | `2.0-4.0` | Named kills feel unrewarding relative to danger. | Named farming becomes the dominant XP strategy if respawn rules allow it. |
| `encounter_role_multiplier_camp` | `1.0` | `0.8-1.2` | Camp-tagged individual kills feel weak relative to pull risk. | Camp-tagged individual kills become the dominant XP route instead of a warning about pull danger. |
| `xp_source_lookup_retention_seconds` | `10` | `5-30` | Duplicate late subscribers may lose metadata and reject valid same-tick events. | Stale lookup rows linger longer, increasing debugging ambiguity. Respawn safety still depends on lifecycle token change. |
| `progression_save_barrier_max_ms` | `10` | `5-25` | Save/Load may fail writes during legitimate same-frame XP transactions. | Transition Save may consume too much of WS's `save_mutex_max_ms` budget. |
| `base_health` | `80` | `70-90` | Level 1 Cleric becomes too fragile for the approved Combat fixture. | Level 1 Cleric may trivialize early trash. |
| `base_mana` | `100` | `90-110` | Level 1 Cleric cannot sustain intended spell cadence. | Level 1 Cleric can over-cast before med-break rhythm matters. |
| `health_linear` | `131 / 9` | Fixture-derived; change only with Combat fixture revision | Breaks `Cleric_Mid_T1` / `Cleric_Top_T1` alignment. | Breaks Combat fixture alignment and soloability tests. |
| `health_quadratic` | `1 / 9` | Fixture-derived; change only with Combat fixture revision | Late T1 health growth under-runs fixture. | Late T1 health growth over-runs fixture. |
| `mana_linear` | `164 / 9` | Fixture-derived; change only with Combat fixture revision | Breaks med-break and spell-sustain fixture expectations. | Over-expands Cleric casting margin. |
| `mana_quadratic` | `4 / 9` | Fixture-derived; change only with Combat fixture revision | Late T1 mana growth under-runs fixture. | Late T1 mana growth over-runs fixture. |
| `resource_fixture_lock_levels` | `[1, 5, 10]` | T1 lock | Fixture alignment becomes implicit and easy to break. | More lock points make the formula brittle before real playtest data exists. |
| `max_t1_spell_tier` | `5` | `3-5` | T1 spell eligibility has too few beats. | Implies more spell content than Class Design / Spell Memorization may support. |
| `spell_tier_unlock_levels_t1` | `[1, 3, 5, 7, 9]` | Authored monotonic list inside levels 1-10 | Eligibility is too sparse or misses early onboarding beats. | Too many unlock beats imply spell content outside T1 capacity. |
| `death_penalty_min_level` | `4` | `4-6` | Death debt can punish onboarding before players understand stakes. | Death XP loss arrives too late to teach honest stakes. |
| `death_debt_pct` | `0.08` | `0.04-0.12` | Death debt preview is too soft to matter. | Death debt preview becomes punitive before Death & Corpse Recovery is validated. |
| `max_levels_per_xp_transaction` | `3` | `1-3` for T1 | Corrupt or oversized awards can run unchecked if no guard exists. | Oversized awards can cross too much of T1 before the guard proves useful. |

### Coupled Tuning Rules

- `xp_base`, `xp_exponent`, `xp_linear`, `base_xp_per_weight`, `xp_award_minimum_nontrivial`, `max_xp_per_kill`, and `max_xp_per_kill_pct_of_level_band` must be tuned together. Changing only one side can halve or double time-to-level.
- `trivial_cutoff`, `diff_step`, `min_nontrivial_modifier`, and `max_level_difference_modifier` define anti-farming behavior as a group.
- `encounter_role_multiplier_named` must be tuned against `repeatability_class`, named respawn cadence, and Combat Core's named soloability envelope. Named enemies should feel valuable, not become the dominant XP route.
- `ProgressionXpSourceRefLookup` rows must be validated against NPC/spawn source refs, legal `Trash | Named | Camp` roles, progression-owned `xp_weight_seed_t1`, expected T1 Combat `kill_weight_seed`, `repeatability_class`, and XP eligibility before pacing tests. Missing `source_lifecycle_token_policy` is a blocker, not a warning.
- `health_linear`, `health_quadratic`, `mana_linear`, and `mana_quadratic` are fixture-alignment knobs. Changing them requires re-validating Combat Core's Cleric fixtures at `resource_fixture_lock_levels`.
- `death_debt_pct` is preview-only until Death & Corpse Recovery owns application timing. Character Progression persists no XP debt in T1.
- `max_t1_spell_tier` and `spell_tier_unlock_levels_t1` must be reconciled with Class Design and Spell Memorization before implementation. The authored unlock list is the authority.

## Visual/Audio Requirements

Character Progression is player-facing, but its audiovisual treatment must be restrained. It reports earned mechanical growth; it does not stage triumph, social promotion, or chosen-one status.

1. **No hero spectacle.** Level-up must not trigger hero glow, screen shake, camera punch, slow motion, bloom pulse, global lighting change, post-process change, particle burst, VFX ring, floating text explosion, center-screen banner, rarity color, or celebratory animation.

2. **Restrained ding only.** `LevelChangedEvent` may produce a quiet audio cue and a small HUD/UI state update when those systems are authored. Character Progression emits the event; Audio and Layer 1 HUD own playback and presentation. The cue should read as acknowledgement, not fanfare.

3. **XP movement is UI-only.** XP gain and threshold progress must not appear as world VFX, character VFX, floating numbers, overhead icons, nameplate effects, minimap pings, or enemy death effects. If shown, XP progress belongs to a restrained Layer 1 UI treatment.

4. **No character visual growth from level.** Level, XP, and permanent stat growth must not change player silhouette, scale, posture, garment quality, faction colors, material age, class ornamentation, lighting priority, facial treatment, portrait treatment, or animation status. These visual progressions belong elsewhere.

5. **Spell eligibility is not spell spectacle.** `SpellEligibilityChanged` may notify downstream systems that a new Cleric spell tier is eligible, but it must not play spell VFX, create spell icons, add spellbook entries, reveal vendors, or preview spell effects. Class Design, Spell Memorization, UI, and Audio own those later surfaces.

6. **Faction visual progression belongs to Faction Reputation.** Level-up must not add faction accents, faction materials, faction seals, rank marks, NPC recognition marks, political titles, or faction-specific posture. Reputation and faction participation own the identity ladder and its visual expression.

7. **Level cap is not a celebration.** Reaching `t1_level_cap` (`visible_level = 10`) uses the same restrained level-up treatment as any other ding. No completion banner, "T1 cap reached" celebration, badge, account marker, special sound, over-cap XP effect, or milestone visual is allowed. The cap is a quiet mechanical ceiling, not a heroic achievement.

## UI Requirements

Character Progression exposes progression data and events for future UI systems; it does not own the final HUD, character sheet, journal, or spellbook presentation. UI must preserve the same mechanical-versus-identity boundary as the rest of this GDD.

1. **Data surfaces only.** Character Progression may expose `current_level`, `visible_level`, `total_xp`, `current_level_xp_progress`, `current_level_xp_band`, `next_level_xp_threshold`, `XPChangedEvent`, per-level `LevelChangedEvent`, first-crossing `LevelCapReached`, and system-facing `SpellEligibilityChanged`. `current_level_xp_progress` is the numerator inside the current level band; `current_level_xp_band` is the band size; `next_level_xp_threshold` is a compatibility alias for the band size and is `0` at cap. It does not instantiate UI widgets.

2. **Layer 1 HUD owns presentation.** T1 shipping HUD must expose `visible_level` and the current XP band progress (`current_level_xp_progress` / `current_level_xp_band`, or an equivalent readable numeric treatment) once Layer 1 HUD is authored. The presentation must be peripheral, restrained, scalable/readable, not color-only, and never a 3px-only progress cue that fails accessibility review. Character Progression only provides data; Layer 1 HUD / UX owns the widget.

3. **No reward modal vocabulary.** UI must not present center-screen popups, quest-complete panels, level-up reward modals, loot-tier language, badge unlocks, achievement panels, account-level meters, battle-pass framing, over-cap celebration, or "T1 cap reached" completion treatment.

4. **XP progress is required but restrained.** The concept promise of earned slow progression requires players to understand progress toward the next level. The required T1 surface may be an XP band bar, numeric `progress / band`, or equivalent Layer 1 HUD treatment, but it must not become a faction reputation display, social rank display, server-status display, or achievement tracker.

5. **No raw hidden internals.** Shipping UI must not display `kill_weight_seed`, formula coefficients, encounter-role multipliers, trivial cutoff math, death-debt percent, Combat source refs, faction mutation assumptions, or raw diagnostic payloads. Debug builds may expose these behind explicit developer tooling only.

6. **Character sheet deferred.** Character Progression may provide data for a future character sheet or journal surface, but this GDD does not define that screen. Character-sheet layout, stat grouping, spell eligibility display, learned ability display, spellbook presentation, and journal integration require a future UX spec.

7. **Cap representation is quiet.** At `visible_level = t1_level_cap`, UI may show the current level as `10` and should hide the XP progress track rather than implying progress beyond cap. It must not show progress past cap, "progress toward Tier 2," percentage-to-future-cap, cap badge, cap celebration, or any account/achievement marker. T1 stores no over-cap XP.

8. **Legal surface matrix.** Until Layer 1 HUD and the relevant UX specs supersede this table, these are the only legal progression surfaces:

| Surface | Shipping T1 | Dev Build | Owner |
|---|---|---|---|
| Layer 1 HUD | Must show `visible_level` and restrained XP band progress once Layer 1 HUD exists; no spell-tier copy and no cap celebration. | May show the same fields plus transaction timing during UI smoke tests. | Layer 1 HUD / UX |
| Character sheet or journal | Deferred; no screen is created by this GDD. | May inspect read-only level and permanent max resource queries in test harnesses. | Future UX spec |
| Spellbook / spell memorization | No abstract "Tier N unlocked" message, no spell icons, and no learned spell presentation from Character Progression. | May log `SpellEligibilityChanged` for integration tests only. | Spell Memorization + UX |
| T1 title / Continue active-record summary | May show `visible_level` only if Menus & Settings later requests it for the single active local record; no save-slot grid, XP progress, or cap badge from this GDD. | May show schema-validation diagnostics. | Menus & Settings |
| Developer diagnostics | Not visible. | May show kill-weight, lookup, dedupe, rejection, and formula details behind explicit debug tooling. | Engineering |

## Acceptance Criteria

All criteria use the project QA taxonomy: Unit, Integration, Editor-validation, Dev-build smoke, or Profiled playtest. All are T1-blocking unless marked fixture-gated.

### Scope / Initialization

**H-CPRO-SCOPE-01 - T1 scope exclusions**
**GIVEN** the Character Progression authored config, **WHEN** the Editor validator inspects enabled features, **THEN** only local Cleric level `1-10` progression is enabled; no networking, account progression, PvP scaling, companion XP, alternate class, achievement, live-service, or LLM progression flags exist.
*Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-INIT-01 - Character Creation seed initializes progression**
**GIVEN** Character Creation emits `starting_class_id = Cleric`, **WHEN** Character Progression initializes a new T1 profile, **THEN** state is `current_level = 1`, `visible_level = 1`, `total_xp = 0`, no XP debt, no level-up pending state, and level-1 permanent baselines.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-INIT-02 - No separate level seed required**
**GIVEN** a valid Character Creation payload without level or XP fields, **WHEN** Character Progression initializes, **THEN** initialization succeeds from `starting_class_id = Cleric` alone.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

### XP Events

**H-CPRO-XP-01 - Kill-credit XP only**
**GIVEN** non-combat reward requests for quest, exploration, discovery, dialogue, faction, crafting, account, rested, or companion XP, **WHEN** they reach Character Progression, **THEN** no XP is awarded and a diagnostic is emitted in development builds.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-XP-02 - Valid kill credit awards XP once**
**GIVEN** a valid active progression profile and `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)` plus a same-dispatch `XpAwardResolutionSnapshot` with `defeated_level`, `encounter_role`, `encounter_role_multiplier`, `xp_weight_seed_t1`, `expected_kill_weight_seed_t1` matching the event, `repeatability_class`, `xp_eligible = true`, and `source_lifecycle_token`, **WHEN** Character Progression processes it, **THEN** exactly one XP transaction is applied using `xp_award` and the structured dedupe tuple is `XpAwardDedupeKey(active_profile.local_character_id, zoneId, defeated_source_ref, source_lifecycle_token)`.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-XP-03 - Duplicate kill credit dedupes**
**GIVEN** duplicate kill-credit events resolving to the same retained `DefeatedTombstone` and `xp_award_dedupe_key`, **WHEN** both are processed, **THEN** XP is awarded once, duplicate diagnostics emit, and no second threshold check occurs.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-XP-04 - Malformed source ref rejected**
**GIVEN** `PlayerKillCreditEvent.defeated_source_ref` is missing, empty, or not a legal persistent NPC or non-persistent spawn source ref, **WHEN** the event is received, **THEN** state does not transition and XP remains unchanged.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-XP-05 - Invalid kill weight rejected**
**GIVEN** `kill_weight_seed` is missing, NaN, negative, zero, or outside the expected T1 authored range, **WHEN** XP is evaluated, **THEN** no clamping occurs, no XP is awarded, and a diagnostic emits.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-XP-06 - Trivial kill awards zero**
**GIVEN** `level_difference_modifier` returns `0`, **WHEN** XP is evaluated, **THEN** `xp_award = 0`, no `XPChangedEvent` emits, no threshold advances, and Faction Reputation / Zone Control remain independent consumers of their own events.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-XP-07 - Sequential multi-level resolution**
**GIVEN** a valid XP transaction crosses multiple thresholds, **WHEN** `LevelingUp` resolves, **THEN** levels apply sequentially, permanent outputs refresh after each level, and only the final stable state is save-eligible.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-XP-08 - Save waits for stable progression state**
**GIVEN** Save/Load requests serialization while Character Progression is held by `progression_transaction_test_latch` in `XpAwarding`, `LevelingUp`, or `XpAdjustmentPending`, **WHEN** the request is handled, **THEN** `ProgressionSaveBarrier` completes the transaction first and Save/Load receives only a stable state.
*Integration | gameplay-programmer + engine-programmer + qa-tester | T1-blocking*

**H-CPRO-XP-09 - Lookup missing rejects XP without Combat fallback**
**GIVEN** Combat Core emits an otherwise valid `PlayerKillCreditEvent` whose `defeated_source_ref` has no matching `XpAwardResolutionSnapshot` or whose snapshot lacks any required field, **WHEN** Character Progression evaluates XP, **THEN** no XP is awarded, no Combat runtime state is inspected, no Combat event-field amendment is required, and a development diagnostic names the missing registry field.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-XP-10 - Respawn lifecycle is not deduped**
**GIVEN** two kills at the same spawn anchor with the same `defeated_source_ref` but different `source_lifecycle_token` values, **WHEN** both kill-credit events are processed in the same playable session, **THEN** each receives its own XP award attempt and the second kill is not rejected as a duplicate of the first.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-XP-11 - XP-ineligible sources award zero**
**GIVEN** a valid kill-credit event and matching award snapshot with `xp_eligible = false`, **WHEN** Character Progression evaluates XP, **THEN** no XP is awarded, no `XPChangedEvent` emits, no threshold check occurs, and the development diagnostic reason is `XpSourceIneligible`.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-XP-12 - Authored kill-weight mismatch rejects XP**
**GIVEN** a T1 fixture kill-credit event whose `kill_weight_seed` does not match the snapshot's `expected_kill_weight_seed_t1`, **WHEN** strict fixture validation is enabled, **THEN** no XP is awarded, no threshold check occurs, and a development diagnostic identifies the source ref and expected kill weight; non-fixture shipping lookup rows validate this mismatch at authoring time instead of silently zeroing runtime XP.
*Unit + Integration | gameplay-programmer + systems-designer + qa-tester | T1-blocking*

**H-CPRO-XP-13 - Stale duplicate cannot bind to respawn**
**GIVEN** a killed spawn has a retained `DefeatedTombstone` and the same spawn anchor has already registered a new `Active` entry with a different `source_lifecycle_token`, **WHEN** a late duplicate kill-credit event for the old lifecycle arrives, **THEN** Character Progression resolves it only against the old tombstone/processed key or rejects it as stale; it never resolves the duplicate against the new active token and never awards second XP for the old kill.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-XP-14 - Save barrier drains pending kill credit**
**GIVEN** Combat emits `PlayerKillCreditEvent` on the same frame a Manual Save or Transition Save trigger enters Save/Load, **WHEN** Save/Load invokes `ProgressionSaveBarrier`, **THEN** the kill-credit event is either fully awarded or fully rejected before the progression save state is serialized; if the barrier exceeds `progression_save_barrier_max_ms`, Save/Load fails the write loudly; no valid save can contain pre-award XP while post-save gameplay contains the awarded XP from that same event.
*Integration | gameplay-programmer + engine-programmer + qa-tester | T1-blocking*

### Event Schemas

**H-CPRO-EVT-01 - Progression event schemas are exact**
**GIVEN** `XPChangedEvent`, `LevelChangedEvent`, `LevelCapReached`, and `SpellEligibilityChanged` are emitted in fixture transactions, **WHEN** payload DTOs are schema-inspected, **THEN** they contain exactly the fields defined in this GDD's Event Payloads section and omit faction rank, reputation delta, patron id, political access, faction-visual fields, spell ids, learned ability ids, combat actor ids, threat tables, damage rolls, and runtime current resources.
*Unit + Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-EVT-02 - Multi-level emits per-level ordered events**
**GIVEN** a single XP transaction advances level 2 to level 5, **WHEN** event logs are inspected, **THEN** `LevelChangedEvent` emits exactly three times in order (`2->3`, `3->4`, `4->5`), permanent outputs refresh before each event, and no batched multi-level event emits.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-EVT-03 - Zero-XP no-op emits no XPChangedEvent**
**GIVEN** a trivial kill, XP-ineligible source, malformed kill-credit event, duplicate dedupe key, at-cap kill, or unresolved XP award snapshot, **WHEN** Character Progression rejects the award with `xp_delta = 0`, **THEN** no `XPChangedEvent` is emitted and the reason is available only as a development diagnostic.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-EVT-04 - Transaction event order is stable**
**GIVEN** one fixture XP transaction changes XP and advances into a spell-eligibility level, and a separate cap-crossing fixture reaches level 10, **WHEN** event logs are inspected, **THEN** subscribers observe `XPChangedEvent` first with final post-transaction `current_level`, `visible_level`, `current_level_xp_progress`, and `current_level_xp_band`, then one ordered `LevelChangedEvent` per level step, then any `SpellEligibilityChanged` for the corresponding level step, and `LevelCapReached` last only in the cap-crossing transaction.
*Integration | gameplay-programmer + ui-programmer + qa-tester | T1-blocking*

### Formulas

**H-CPRO-F1 - XP threshold table**
**GIVEN** default XP threshold parameters, **WHEN** `xp_threshold(level)` evaluates levels `1-10`, **THEN** outputs are exactly `0, 150, 540, 1210, 2170, 3430, 5010, 6910, 9140, 11710`.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-F2 - Level difference modifier**
**GIVEN** player level `5`, **WHEN** defeated level is `3`, **THEN** modifier is `0.70`; **GIVEN** player level `7`, **WHEN** defeated level is `1`, **THEN** level delta is `-6` and modifier is `0`.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-F3 - XP award example**
**GIVEN** level 5 Cleric defeats level 6 trash with `xp_weight_seed_t1 = 1.25`, **WHEN** defaults evaluate, **THEN** `xp_award = 71`.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-F3a - Minimum nontrivial XP award**
**GIVEN** a non-trivial kill where `level_difference_modifier > 0` but `raw_xp_award` floors below `xp_award_minimum_nontrivial`, **WHEN** `xp_award` evaluates, **THEN** the result is exactly `xp_award_minimum_nontrivial`; **WHEN** `level_difference_modifier = 0`, **THEN** the result remains `0` regardless of the minimum knob.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-F3b - Level query mapping**
**GIVEN** `total_xp = 2,500` and default thresholds, **WHEN** level-state queries evaluate, **THEN** `current_level = 5`, `visible_level = 5`, `current_level_xp_progress = 330`, `current_level_xp_band = 1,260`, and `next_level_xp_threshold = 1,260`; **WHEN** `total_xp = 11,710`, **THEN** `current_level = 10`, `visible_level = 10`, `current_level_xp_progress = 0`, `current_level_xp_band = 0`, and `next_level_xp_threshold = 0`.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-F4 - Permanent resource fixture locks**
**GIVEN** default resource formulas, **WHEN** levels `1`, `5`, and `10` evaluate, **THEN** health/mana outputs are `80/100`, `140/180`, and `220/300`, matching Combat Core fixtures.
*Unit + Editor-validation | gameplay-programmer + systems-designer | fixture-gated T1-blocking*

**H-CPRO-F5 - Spell eligibility tier**
**GIVEN** default `spell_tier_unlock_levels_t1 = [1, 3, 5, 7, 9]`, **WHEN** level `7` evaluates, **THEN** `spell_eligibility_tier = 4`, and no spell ids, learned abilities, spell records, or memorized slots are created.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-F6 - Death debt preview**
**GIVEN** level `7` and default death-debt parameters, **WHEN** `death_xp_debt_preview` evaluates, **THEN** output is `150` XP debt preview and no penalty is applied unless Death & Corpse Recovery calls the approved interface.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-F7 - Cap clamp**
**GIVEN** a level 9 character at `11,600` XP earns `300` raw XP, **WHEN** cap clamp evaluates, **THEN** `applied_xp_delta = 110`, stored XP is `11,710`, `XPChangedEvent.xp_delta = 110`, `XPChangedEvent.current_level = 10`, `XPChangedEvent.visible_level = 10`, `LevelCapReached` emits once, and no over-cap XP or T1 benefit is granted.
*Unit | gameplay-programmer | T1-blocking*

### Save / Load Hydration

**H-CPRO-SL-01 - Persistence whitelist**
**GIVEN** a progression save payload, **WHEN** serialized fields are inspected, **THEN** it contains only `progression_schema_version`, `class_id`, `current_level`, `total_xp`, and `spell_eligibility_tier`; it excludes XP debt, Combat runtime current resources, threat, casts, targets, cooldowns, runtime ids, derived max-resource caches, spell content, spell ids, learned abilities, spellbook records, memorized slots, and vendor/drop availability.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-SL-02 - Invalid hydration fails loud**
**GIVEN** Save/Load hydrates invalid class id, level, unknown `progression_schema_version`, missing authored formula config for that schema version, missing authored spell unlock data, spell eligibility mismatch, negative XP, non-cap XP outside `xp_threshold(current_level) <= total_xp < xp_threshold(current_level + 1)`, or cap XP not equal to `xp_threshold(10)`, **WHEN** Character Progression validates, **THEN** it returns hydration failure and no playable session is enabled.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-SL-03 - Over-cap XP is invalid save data**
**GIVEN** saved `total_xp` exceeds the level-10 threshold, **WHEN** Character Progression hydrates, **THEN** hydration fails loudly, no clamp occurs, and no playable session is enabled.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-SL-04 - Stable-state serialization only**
**GIVEN** a save trigger during transient progression state, **WHEN** serialization occurs, **THEN** no partial XP award, partial multi-level chain, or partial XP adjustment is written.
*Integration | gameplay-programmer + engine-programmer | T1-blocking*

**H-CPRO-SL-05 - Progression hydrates before Combat actor**
**GIVEN** a valid save fixture containing Character Progression and Combat player resource state, **WHEN** Save/Load runs `Resuming`, **THEN** Character Progression validates first, publishes `ProgressionBaselineSnapshot(current_level, permanent_max_health, permanent_max_mana, spell_eligibility_tier)`, and only then may Combat Core hydrate or build the player combat actor using that snapshot's health/mana maxima.
*Integration | gameplay-programmer + engine-programmer + qa-tester | T1-blocking*

**H-CPRO-SL-06 - Pending kill credit cannot serialize stale XP**
**GIVEN** a pending same-frame Combat kill-credit dispatch exists when Save/Load requests Character Progression state, **WHEN** `ProgressionSaveBarrier` is invoked, **THEN** Character Progression returns no save payload until the event has been awarded, rejected, or diagnosed as stale; Save/Load must also obtain NPC System's matching source-lifecycle save barrier before serialization; and the serialized `total_xp` equals the post-barrier runtime value. If either barrier fails to settle within its bounded save-barrier budget, the write fails loudly rather than writing inconsistent XP/source state.
*Integration | gameplay-programmer + engine-programmer + qa-tester | T1-blocking*

### Combat Boundaries

**H-CPRO-CB-01 - Combat event is consumed narrowly**
**GIVEN** a valid Combat kill-credit event, **WHEN** Character Progression processes it, **THEN** only `defeated_source_ref`, `zoneId`, `faction_id`, `kill_weight_seed`, and `XpSourceLifecycleRegistry` XP metadata are read; threat tables, damage rolls, loot, corpse records, runtime current resources, and `combat_actor_id` are not read or persisted.
*Unit + Integration + Editor-validation (DTO/static API boundary scan) | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-CB-02 - Permanent max update does not mutate current resources**
**GIVEN** level-up increases permanent max health or mana during combat, **WHEN** outputs refresh, **THEN** Combat receives new maxima, but `current_health`, `current_mana`, threat, casts, regen, med-break, and death state are unchanged by Character Progression.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-CB-03 - Level-up does not reset combat**
**GIVEN** the player levels during active combat, **WHEN** `LevelChangedEvent` emits, **THEN** combat continues with no threat clear, heal, mana refill, revive, enemy reset, timer reset, or combat rule change from Character Progression.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

### Death / Cap / Spell Boundaries

**H-CPRO-DCS-01 - Death penalty interface only**
**GIVEN** Combat Core emits `PlayerDeathEvent`, **WHEN** Character Progression receives no Death & Corpse Recovery penalty request, **THEN** it applies no death XP loss.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-DCS-02 - Invalid death penalty request rejected**
**GIVEN** Death & Corpse Recovery is absent or sends a request missing `death_context_id`, `local_character_id`, `requested_xp_delta`, approved `adjustment_policy_ref`, explicit `allow_delevel`, or sends a non-negative `requested_xp_delta`, **WHEN** XP adjustment is requested, **THEN** Character Progression rejects the request, emits a diagnostic, and leaves XP unchanged.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-DCS-03 - Death penalties are preview-only in T1**
**GIVEN** a previewed XP penalty would reduce XP below the current level threshold, **WHEN** Death & Corpse Recovery is not yet authored and approved, **THEN** Character Progression exposes the preview value only, persists no XP debt, applies no XP loss, and does not delevel.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-DCS-03a - Death adjustment contract is idempotent**
**GIVEN** any `ProgressionXpAdjustmentRequest` reaches Character Progression before Death & Corpse Recovery is authored and approved, including requests with duplicate-looking `death_context_id`, wrong `local_character_id`, non-negative XP delta, missing policy ref, or `allow_delevel = false`, **WHEN** Character Progression evaluates the request, **THEN** it rejects before mutation, emits no `XPChangedEvent`, persists no death-context id, and leaves saved XP unchanged.
*Unit | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-DCS-04 - Spell eligibility does not create spell content**
**GIVEN** `SpellEligibilityChanged` fires, **WHEN** downstream spell systems are absent, **THEN** no spell ids, learned abilities, spell records, spellbook entries, memorized slots, vendors, drops, UI buttons, VFX, or spell effects are created.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-DCS-05 - Cap grants no hidden benefit**
**GIVEN** visible level is capped at 10 and additional kill credit is received, **WHEN** progression outputs are inspected, **THEN** XP remains capped at `11,710`, no over-cap value exists, and no additional health, mana, spell eligibility, title, faction value, account marker, or hidden benefit is produced.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-DCS-06 - Spell eligibility remains system-facing until mapped**
**GIVEN** `spell_tier_unlock_levels_t1` contains tiers above baseline tier 1 and Class Design / Spell Memorization have not approved player-facing Cleric beats for those tiers, **WHEN** `SpellEligibilityChanged` emits, **THEN** the event remains system-facing only, no player-facing spell unlock copy or UI is shown, and downstream player-facing spell presentation stays blocked until the owning specs reverse-list the mapping.
*Integration + Editor-validation | game-designer + gameplay-programmer + qa-tester | T1-blocking*

### Faction Reputation Boundary

**H-CPRO-FRB-01 - XP never mutates faction state**
**GIVEN** XP is awarded from a kill-credit event containing `faction_id`, **WHEN** Character Progression processes it, **THEN** no faction reputation value, rank, standing, event log, or political state changes.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-FRB-02 - Faction fields absent from progression events**
**GIVEN** `XPChangedEvent`, `LevelChangedEvent`, `LevelCapReached`, or `SpellEligibilityChanged` emits, **WHEN** payload schema is inspected, **THEN** no faction rank, reputation delta, patron id, political access, or faction-visual field is present.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-FRB-03 - No faction API surface**
**GIVEN** Character Progression public API is scanned, **WHEN** method/event/property names and DTOs are inspected, **THEN** no API exists to mutate faction reputation, faction state, political access, NPC trust, or faction visuals.
*Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-FRB-04 - Faction visuals not touched by level-up**
**GIVEN** the player levels up in a dev build, **WHEN** character render state and appearance tokens are inspected, **THEN** no faction color, material, seal, posture, silhouette, portrait, lighting priority, or garment-quality change is applied.
*Dev-build smoke | technical-artist + qa-tester | T1-blocking*

**H-CPRO-FRB-05 - Level is not a social gate**
**GIVEN** Character Progression public APIs, event payloads, and authored progression conditions are scanned, **WHEN** level accessors and query contracts are inspected, **THEN** no Character Progression-owned condition grants reputation rank, faction trust, patron access, NPC intimacy, title, city access, or political authority; future Faction Reputation, Dialogue, and city-access systems must reverse-list any advisory level read in their own GDDs.
*Editor-validation | gameplay-programmer + narrative-designer + qa-tester | T1-blocking*

### Visual / Audio + UI Prohibitions

**H-CPRO-VUI-01 - No level-up spectacle**
**GIVEN** a level-up occurs in a dev build, **WHEN** rendering and camera state are inspected, **THEN** no hero glow, screen shake, camera punch, slow motion, bloom pulse, post-process change, particle burst, VFX ring, floating text explosion, or center-screen banner appears.
*Dev-build smoke | technical-artist + qa-tester | T1-blocking*

**H-CPRO-VUI-02 - Audio hook only**
**GIVEN** `LevelChangedEvent` emits, **WHEN** runtime audio objects are inspected before Audio System implementation, **THEN** Character Progression creates no `AudioSource`, mixer state, fanfare, special cap cue, or playback object.
*Integration + Editor-validation | gameplay-programmer + audio-lead | T1-blocking*

**H-CPRO-VUI-03 - UI hook only**
**GIVEN** XP or level state changes with no Layer 1 HUD implementation, **WHEN** the scene is inspected, **THEN** Character Progression instantiates no HUD widget, modal, stat grid, journal screen, reward panel, badge, or spellbook UI.
*Integration | gameplay-programmer + ui-programmer | T1-blocking*

**H-CPRO-VUI-04 - Shipping UI hides internals**
**GIVEN** a shipping UI build, **WHEN** progression UI surfaces are inspected, **THEN** `kill_weight_seed`, formula coefficients, encounter multipliers, trivial cutoff math, death-debt percent, Combat source refs, and diagnostic payloads are absent.
*Dev-build smoke + Editor-validation | ui-programmer + qa-tester | T1-blocking*

**H-CPRO-VUI-05 - Cap representation is quiet**
**GIVEN** visible level reaches 10, **WHEN** UI/presentation events fire, **THEN** there is no "T1 cap reached" celebration, over-cap XP display, progress-to-Tier-2 indicator, cap badge, account marker, special sound, or milestone visual.
*Integration + Dev-build smoke | ui-programmer + audio-lead + qa-tester | T1-blocking*

**H-CPRO-VUI-06 - UI surface matrix enforced**
**GIVEN** shipping and dev-build progression surfaces are inspected, **WHEN** UI code subscribes to Character Progression data or events, **THEN** each surface matches the legal surface matrix in this GDD, shipping UI never displays raw diagnostics or abstract spell tiers, and dev-only fields are behind explicit debug tooling.
*Editor-validation + Dev-build smoke | ui-programmer + qa-tester | T1-blocking*

**H-CPRO-VUI-07 - Shipping HUD exposes readable XP progress**
**GIVEN** the Character Progression UI handoff contract is validated before Layer 1 HUD implementation, **WHEN** Editor validation scans progression UI requirements and public queries, **THEN** the contract exposes `visible_level`, `current_level_xp_progress`, and `current_level_xp_band`, requires downstream HUD to define readable/non-color-only progress treatment before `shipping_t1_progression_hud_ready = true`, and keeps cap presentation quiet (`visible_level = 10`, no progress track or cap celebration).
*Editor-validation + Dev-build smoke | ui-programmer + qa-tester | T1-blocking*

### Tuning / Fixture Validation

**H-CPRO-TUNE-01 - Tuning safe-range validator**
**GIVEN** authored Character Progression tuning data, **WHEN** the Editor validator runs, **THEN** every knob is within its safe range or explicitly marked fixture-derived/T1-fixed.
*Editor-validation | systems-designer + qa-tester | T1-blocking*

**H-CPRO-TUNE-02 - Coupled XP tuning warning**
**GIVEN** one XP curve or XP award knob changes without the coupled group changing or a tuning-note override, **WHEN** the validator runs, **THEN** it emits a designer-facing warning for time-to-level revalidation.
*Editor-validation | systems-designer | T1-blocking*

**H-CPRO-TUNE-03 - Fixture-derived resource coefficients lock**
**GIVEN** any resource coefficient or `resource_fixture_lock_levels` changes, **WHEN** validation runs, **THEN** fixture outputs at levels 1, 5, and 10 must still match Combat Core Cleric fixtures or validation fails.
*Editor-validation + Unit | gameplay-programmer + systems-designer | fixture-gated T1-blocking*

**H-CPRO-TUNE-04 - Max levels per transaction guard**
**GIVEN** a corrupted or oversized XP transaction with the test override `max_levels_per_xp_transaction = 1` would advance more than one level, **WHEN** Character Progression preflights it, **THEN** the whole transaction is rejected before XP, level, spell eligibility, processed dedupe state, or events mutate; a diagnostic emits and serialized progression state remains byte-for-byte equivalent to the pre-transaction state.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-TUNE-05 - Spell tier unlock list validator**
**GIVEN** `spell_tier_unlock_levels_t1` is authored, **WHEN** validation runs, **THEN** the list length equals `max_t1_spell_tier`, tier 1 unlocks at level 1, all tiers are monotonic, all unlock levels are inside levels 1-10, and no tier exceeds `max_t1_spell_tier`.
*Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-TUNE-06 - XP source lookup validator**
**GIVEN** authored `ProgressionXpSourceRefLookup` data, **WHEN** the Editor validator runs, **THEN** every row has legal `zoneId`, legal `defeated_source_ref`, `defeated_level` inside T1 authored range, legal `encounter_role` (`Trash`, `Named`, or `Camp`), role multiplier inside the configured safe range, `xp_weight_seed_t1` and `expected_kill_weight_seed_t1` inside the progression-authored T1 fixture range, legal `repeatability_class`, explicit `xp_eligible`, and a non-empty `source_lifecycle_token_policy`; missing rows for Combat fixture sources fail validation.
*Editor-validation | gameplay-programmer + systems-designer + qa-tester | T1-blocking*

### Pacing Fantasy

**H-CPRO-PACE-01 - EQ-slow camp-session ding cadence**
**GIVEN** the profiled `CampLoop_Mid_T1` fixture uses a level 5 Cleric, Combat Core's `SoloTrash_EvenCon_T1` trash source at defeated level 5, `xp_weight_seed_t1 = 1.25`, `kill_weight_seed = expected_kill_weight_seed_t1`, `encounter_role = Trash`, `encounter_role_multiplier = 1.0`, `xp_eligible = true`, a 120-second pull/kill cadence, and a 90-second med-break after every four kills, **WHEN** QA runs a 60-120 minute camp loop from level 5 toward level 6 with no death penalties, **THEN** each qualifying kill awards `62` XP, the level band requires `21` kills, observed time-to-ding is between 45 and 120 minutes, and the report logs XP/hour, kills/level, pull cadence, med cadence, and time-to-ding.
*Profiled playtest | game-designer + systems-designer + qa-tester | fixture-gated T1-blocking*

**H-CPRO-PACE-02 - Undercon farming is nonviable**
**GIVEN** a level 7 Cleric repeatedly kills level 3 or lower trash using valid Combat fixture kill credit, **WHEN** default `trivial_cutoff = -4` and authored lookup rows evaluate for a 30-minute farm loop, **THEN** each kill is trivial, awards `0` XP, emits no `XPChangedEvent`, and cannot advance toward level 8.
*Profiled playtest + Unit | systems-designer + qa-tester | fixture-gated T1-blocking*

**H-CPRO-PACE-03 - Soft-undercon farming is slower than even-con**
**GIVEN** paired 30-minute fixtures for a level 7 Cleric killing level 7 trash and level 4 trash with identical `xp_weight_seed_t1 = 1.25`, `encounter_role = Trash`, `encounter_role_multiplier = 1.0`, `repeatability_class = Repeatable`, and `xp_eligible = true`, while the level 4 route uses its fastest profiled safe pull cadence and lowest observed med-break cadence, **WHEN** XP/hour and projected kills-to-level are compared, **THEN** the level 4 route produces no more than 60% of the even-con XP/hour and cannot project a faster time-to-ding than the even-con route.
*Profiled playtest + Unit | systems-designer + qa-tester | fixture-gated T1-blocking*

**H-CPRO-PACE-04 - Named and camp density do not bypass slow progression**
**GIVEN** named and dense-camp T1 fixtures use legal `Named` or `Camp` lookup rows with explicit `repeatability_class`, **WHEN** QA profiles a 60-minute loop with authored respawn/lockout/med cadence, **THEN** any `Repeatable` route produces no more than 100% of the even-con trash fixture XP/hour from `H-CPRO-PACE-01`; any `RespawnLockout` or `NonRepeatableFirstKill` route records its lockout or one-time status and cannot be projected as a repeatable XP/hour route.
*Profiled playtest | game-designer + systems-designer + qa-tester | fixture-gated T1-blocking*

**H-CPRO-PACE-05 - Early, mid, and late level bands preserve earned cadence**
**GIVEN** level-band fixtures from 2->3, 5->6, and 9->10 use valid Combat kill credit and progression lookup rows, **WHEN** QA profiles time-to-ding, **THEN** each band reports XP/hour, kills/level, and med cadence, with 2->3 between 20-60 minutes, 5->6 between 45-120 minutes, and 9->10 between 90-180 minutes under default T1 camp cadence.
*Profiled playtest | game-designer + systems-designer + qa-tester | fixture-gated T1-blocking*

### Summary

Total: 69 criteria.
Ordinary T1-blocking: 62.
Fixture-gated T1-blocking: 7 (`H-CPRO-F4`, `H-CPRO-TUNE-03`, `H-CPRO-PACE-01`, `H-CPRO-PACE-02`, `H-CPRO-PACE-03`, `H-CPRO-PACE-04`, `H-CPRO-PACE-05`).
Advisory-at-T1: 0.

## Open Questions

| ID | Question | Owner | Required Before |
|---|---|---|---|
| `CPRO-OQ-01` | Class Design and Spell Memorization must map tiers 3/5/7/9 to authored Cleric gameplay beats before spell eligibility becomes player-facing. | Class Design + Spell Memorization | Player-facing spell unlocks |
| `CPRO-OQ-02` | Death & Corpse Recovery must define approved XP-loss timing, corpse recovery reversal/mitigation, and deleveling policy before Character Progression accepts `ProgressionXpAdjustmentRequest` beyond preview rejection. | Death & Corpse Recovery | Death penalty implementation |
| `CPRO-OQ-03` | Layer 1 HUD / UX must approve the shipping treatment for required `visible_level` + current-band XP progress, T1 title/Continue active-record summary, accessibility, and any character-sheet surface before UI exposes progression data. | Layer 1 HUD / UX | Shipping progression UI |
