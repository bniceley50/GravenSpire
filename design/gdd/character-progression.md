# Character Progression

> **Status**: In Design
> **Author**: Codex (session with brian, 2026-04-25)
> **Last Updated**: 2026-04-25
> **Implements Pillar**: Primary - **P3 Reputation Is The Progression**. Supports - **P2 The Silence Is Sacred** and **P5 Stakes Are Honest**.

## Overview

Character Progression is the mechanical ladder of Gravenspire's EQ-classic class progression: XP, levels, permanent stat growth, and spell-unlock eligibility for the T1 Cleric. It exists so camps, haunt depth, spell access, and survival margins can scale over time without turning the game into a gear treadmill or making class level the player's identity story. The game's three-track model remains intact: Character Progression owns the vertical class ladder; Faction Reputation owns the horizontal identity ladder; Server State owns the larger political backdrop. A higher-level Cleric is mechanically more capable, but not more socially meaningful by default.

At T1, Character Progression is deliberately narrow: offline single-player, one local Cleric, one haunt, one city hub, no networking, no account progression, no PvP, no companions, no live LLM, and no alternate classes. The system consumes Character Creation's `starting_class_id = Cleric` seed, persists its own XP/level/progression state through Save/Load, and consumes Combat Core's kill/death hooks without redefining combat rules. In particular, Combat Core owns runtime `current_health` and `current_mana`, while Character Progression owns level scaling and permanent maximum resource values that Combat reads when building or hydrating combat actor state.

This GDD defines the XP curve, level-up rules, permanent stat-growth contract, spell-unlock eligibility contract, persistence whitelist, and downstream interfaces. It does not define Cleric spell content, memorized spell slots, equipment stats, loot, faction reputation values, corpse-run recovery, XP-loss penalties, zone-control math, combat hit/damage rules, or any Tier 2+ class/account/server progression. Those systems consume Character Progression outputs through explicit interfaces rather than inheriting unstated assumptions.

## Player Fantasy

Character Progression should feel like the old EQ promise made gothic and restrained: the camp was dangerous, the pulls were slow, the XP bar moved by inches, and then the character dings. The moment matters because it was earned through patience, risk, corpse-run fear, and repeated returns to the same haunted rooms. A level is not a celebration that the world loves you; it is proof that your body and practice have hardened enough to stand a little deeper in the dark.

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

2. **Character Creation seed.** A new T1 character initializes at `class_id = Cleric`, `level = 1`, `total_xp = 0`, no XP debt, no level-up pending state, and default level-1 permanent baselines. Character Creation does not need to add a separate level/XP seed.

3. **Kill-credit XP only.** In T1, XP can be awarded only from Combat Core's confirmed `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)`. Quest XP, exploration XP, discovery XP, dialogue XP, faction XP, crafting XP, account XP, rested XP, and companion XP are out of scope.

4. **Combat event consumption is narrow.** Character Progression consumes `PlayerKillCreditEvent` only to evaluate XP eligibility and award XP. It does not inspect threat tables, damage rolls, combat actor ids, loot, corpse records, or runtime combat state.

5. **No faction mutation from XP events.** `faction_id` may appear on kill-credit events for downstream systems, but Character Progression must not mutate reputation, faction rank, faction standing, political access, NPC relationship, or faction visuals from it.

6. **Immediate ding.** When an XP transaction causes `total_xp` to meet or exceed the next level threshold, level-up resolves immediately in the same progression transaction. The event is felt, but restrained: data changes, a quiet `LevelChangedEvent`, and downstream UI/audio hooks only.

7. **Sequential multi-level resolution.** If one XP transaction crosses multiple thresholds, levels resolve one at a time until `total_xp` falls below the next threshold or the T1 cap is reached. Each level increment refreshes permanent progression outputs before evaluating the next threshold.

8. **T1 cap behavior.** At level 10, the visible character level is capped. Additional XP is stored in `total_xp` only for future compatibility and save continuity; it grants no level, stat, spell eligibility, title, reputation, or hidden benefit during T1.

9. **Permanent values only.** Character Progression owns permanent level-derived baselines: max health, max mana, level-facing combat baselines, XP state, and spell-unlock eligibility. Combat Core owns runtime `current_health`, `current_mana`, regen, damage, death, threat, casting, and med-break state.

10. **No ding combat reset.** Level-up does not fully heal, fully restore mana, clear threat, cancel death, clear XP penalties, revive the player, reset med timers, interrupt enemy behavior, or change combat rules. If max health/mana increase, Combat decides how runtime current values respond.

11. **Spell eligibility only.** Character Progression may emit `SpellEligibilityChanged` for level-appropriate Cleric spell tiers. It does not define spell lists, spell effects, spell vendors, spell drops, memorized slots, spellbook UI, or spell mana costs.

12. **Mechanical, not social.** Level never grants faction trust, social rank, patron access, NPC intimacy, city title, story status, political authority, companion authority, account prestige, or server recognition.

13. **Named enemies remain gated.** Level growth widens survival and mana margins for ordinary camps; it does not make named enemies or linked camps default solo targets. Encounter design and Combat Core's soloability envelope remain authoritative.

14. **Progression persistence whitelist.** T1 progression save state contains `progression_schema_version`, `class_id`, `level`, `total_xp`, `xp_debt` if active, and permanent progression baseline identifiers or values required to hydrate consistently. It must not persist Combat runtime current resources, threat, cast state, target selection, cooldowns, runtime ids, or derived caches that can be rebuilt from level.

15. **Hydration validation.** On load, Character Progression validates class, level range, XP bounds, cap behavior, schema version, and permanent baseline availability before gameplay enablement. Invalid progression hydration returns failure to Save/Load; it does not silently repair or default.

16. **Death penalty interface only.** Character Progression defines XP adjustment/debt interfaces for Death & Corpse Recovery, but does not apply death penalties directly from Combat Core's `PlayerDeathEvent`. Death & Corpse Recovery owns when death loss applies, corpse recovery, resurrection, and penalty reversal or mitigation.

17. **Quiet presentation.** Character Progression emits data and presentation hooks; Layer 1 HUD, Audio, and later UI specs own how they appear. No hero banners, loot-tier language, glowing panels, battle-pass meters, fanfare, or center-screen reward treatment.

18. **T1 exclusions.** No networking, accounts, server progression, PvP scaling, companion progression, alternate classes, account unlocks, live-service tracks, achievements, or LLM memory.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Uninitialized` | No progression state exists | Character Creation seed accepted -> `InitializedLevel1`; Save/Load hydrate -> `HydratedFromSave` | No XP events accepted. |
| `InitializedLevel1` | New Cleric profile created | Valid kill credit -> `XpAwarding`; save requested -> `ProgressionSaved` | Level 1, 0 XP, default baselines. |
| `HydratedFromSave` | Save/Load delivers valid progression payload | Validation success -> `Ready`; validation failure -> `InvalidProgressionState` | Rebuilds permanent outputs from saved state. No XP re-award. |
| `Ready` | Valid runtime progression state | Kill credit -> `XpAwarding`; death penalty request -> `XpAdjustmentPending`; save requested -> `ProgressionSaved` | Normal playable state. |
| `XpAwarding` | Valid `PlayerKillCreditEvent` accepted | Threshold crossed -> `LevelingUp`; no threshold -> `Ready`; invalid event -> `Ready` | Applies XP transaction once. |
| `LevelingUp` | XP meets next threshold | More thresholds -> `LevelingUp`; cap reached -> `LevelCapped`; done -> `Ready` | Sequentially increments level and refreshes outputs. |
| `LevelCapped` | Level 10 reached in T1 | Save/load or future tier expansion | Stores excess `total_xp` without T1 benefits. |
| `XpAdjustmentPending` | Death & Corpse Recovery requests XP adjustment/debt | Adjustment applied -> `Ready`; invalid request -> `Ready` with diagnostic | Interface state only; death system owns timing. |
| `ProgressionSaved` | Save/Load serializes progression state | Save complete -> prior playable state | Writes only whitelist state. |
| `InvalidProgressionState` | Hydration or schema validation fails | Save/Load rejects load | No playable session enabled. |

### Interactions with Other Systems

| System | Character Progression Consumes | Character Progression Provides | Ownership Boundary | Dependency |
|---|---|---|---|---|
| **Character Creation** | `starting_class_id = Cleric` | No output | Character Creation owns first-run identity seed; Progression owns level/XP initialization. | Hard upstream |
| **Save / Load & Persistence** | Hydrated progression payload; save trigger serialization | `CharacterProgressionSaveState`; hydration validation result | Progression owns schema and validation; Save/Load owns serialization, HMAC, migration, failure surfacing. | Hard |
| **Combat Core** | `PlayerKillCreditEvent`; permanent baseline read requests | Level/max-resource/stat baseline snapshot; `LevelChangedEvent` | Combat owns runtime current health/mana and combat rules; Progression owns permanent max values and XP. | Hard event boundary |
| **Death & Corpse Recovery** | XP penalty/debt request once authored | XP adjustment policy and apply/preview interface | Death system owns death timing, corpse, XP-loss application trigger, resurrection. Progression owns XP math target state. | Future hard downstream |
| **Class Design** | Cleric class progression table once authored | Current level and class progression band | Class Design owns class content and final stat tables; Progression owns applying approved tables. | Future hard downstream |
| **Spell Memorization** | None in T1 | `SpellEligibilityChanged`; level eligibility query | Spell system owns memorized slots/spellbook behavior. Progression owns eligibility by level only. | Future hard downstream |
| **Inventory & Item Economy** | None in T1 | Current level if item requirements later need it | Inventory owns items, equipment legality, drops, currency. Progression does not grant loot. | Future downstream |
| **Faction Reputation** | None | No reputation output | Reputation owns identity ladder. Progression must not mutate faction values. | Boundary only |
| **Zone Control** | None | No zone-control output | Zone Control consumes Combat kill-weight data, not progression XP. | Boundary only |
| **Layer 1 HUD / Audio** | None | `XPChangedEvent`, `LevelChangedEvent`, `LevelCapReached`, `SpellEligibilityChanged` | UI/audio own presentation. Progression owns quiet event timing/data. | Future downstream |

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

The `level_difference_modifier` formula is defined as:

`level_delta = defeated_level - player_level`

`level_difference_modifier = level_delta <= trivial_cutoff ? 0 : clamp(1 + (diff_step * level_delta), min_nontrivial_modifier, max_level_difference_modifier)`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `player_level` | `PL` | int | 1-10 T1; 1-60 future | Current player level. |
| `defeated_level` | `DL` | int | 1-10 T1; 1-60 future | Defeated source level from authored NPC/spawn data. |
| `trivial_cutoff` | `TC` | int | -8 to -4; default -6 | Level delta at or below which a kill awards 0 XP. |
| `diff_step` | `DS` | float | 0.10-0.20; default 0.15 | Modifier change per level difference. |
| `min_nontrivial_modifier` | `MIN` | float | 0.10-0.50; default 0.25 | Lowest modifier before trivial cutoff. |
| `max_level_difference_modifier` | `MAX` | float | 1.25-2.00; default 1.60 | Highest modifier for above-level kills. |

**Output Range:** `0.00-1.60` in T1 defaults.

**Example:** Level 5 player defeats level 3 source: `level_delta = -2`; modifier is `clamp(1 + (0.15 * -2), 0.25, 1.60) = 0.70`.

The `xp_award` formula is defined as:

`xp_award = min(max_xp_per_kill, floor(base_xp_per_weight * kill_weight_seed * level_difference_modifier * encounter_role_multiplier))`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `base_xp_per_weight` | `BW` | int | 35-75; default 50 | Conversion scale from Combat kill weight to XP. |
| `kill_weight_seed` | `KW` | float | Expected T1 0.25-6.0 | Combat Core kill-weight seed from `PlayerKillCreditEvent`; Progression consumes, does not redefine. |
| `level_difference_modifier` | `LDM` | float | 0.00-1.60 | Output of `level_difference_modifier`. |
| `encounter_role_multiplier` | `ERM` | float | Trash 1.0; Elite 1.5; Named 3.0 default | Authored encounter role multiplier; named safe range 2.0-4.0. |
| `max_xp_per_kill` | `MXK` | int | 300-900; default 600 | Per-kill XP ceiling. |

**Output Range:** `0-600` XP per kill with T1 defaults.

**Example:** Level 5 Cleric defeats a level 6 trash source with `kill_weight_seed = 1.25`: `floor(50 * 1.25 * 1.15 * 1.0) = 71 XP`.

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

The `spell_eligibility_tier` formula is defined as:

`spell_eligibility_tier = clamp(1 + floor((level - 1) / levels_per_spell_tier), 1, max_t1_spell_tier)`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `level` | `L` | int | 1-10 T1 | Current Cleric level. |
| `levels_per_spell_tier` | `LPT` | int | 2-4; default 2 | Levels per eligibility tier. |
| `max_t1_spell_tier` | `MST` | int | 3-5; default 5 | Highest T1 eligibility tier. |

**Output Range:** T1 eligibility tier `1-5`. This is eligibility only, not spell content or memorization.

**Example:** Level 7 Cleric: `1 + floor((7 - 1) / 2) = 4`.

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

The `cap_overflow` formula is defined as:

`stored_total_xp = stored_total_xp + xp_award`

`visible_level = min(highest_level_where(xp_threshold(level) <= stored_total_xp), t1_level_cap)`

`overcap_xp = max(0, stored_total_xp - xp_threshold(t1_level_cap))`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---:|---|---|---|
| `stored_total_xp` | `SXP` | int | 0+ | Persisted total XP, including over-cap XP. |
| `xp_award` | `XA` | int | 0-600 T1 default | Output of `xp_award`. |
| `visible_level` | `VL` | int | 1-10 T1 | Displayed and mechanically active level. |
| `t1_level_cap` | `CAP` | int | fixed 10 in T1 | T1 cap. |
| `overcap_xp` | `OCX` | int | 0+ | Stored XP above level-10 threshold with no T1 benefit. |

**Output Range:** `visible_level` remains `1-10`; `overcap_xp` is stored but grants no T1 benefit.

**Example:** Level 10 character at `11,710` XP earns `300` XP. Stored total becomes `12,010`; visible level remains `10`; `overcap_xp = 300`.

## Edge Cases

- **If duplicate `PlayerKillCreditEvent` arrives for the same defeated source in the same death transaction**: award XP once, emit a duplicate-event diagnostic, and ignore subsequent duplicates. The dedupe key is the defeated stable source reference plus the Combat Core death/kill transaction identity if available; never use transient `combat_actor_id` for long-lived dedupe.

- **If `PlayerKillCreditEvent` arrives after the player has died but before Death & Corpse Recovery has taken over**: process the kill credit only if Combat Core emitted it before or during the same lethal combat resolution transaction and the player had qualifying contribution. Do not infer XP from corpse-run state, and do not apply death penalty here.

- **If `PlayerKillCreditEvent` arrives during `ZoneTransitionBeginEvent` cleanup or after the active zone has become non-playable**: accept only kill-credit events already emitted by Combat Core before transition cleanup completed. Late events from an inactive outgoing zone are ignored with a diagnostic; no cross-zone XP synthesis occurs.

- **If `PlayerKillCreditEvent` arrives before Character Progression has initialized or hydrated**: ignore the event, emit a diagnostic, and award no XP. XP cannot be queued against an unvalidated progression state.

- **If a save trigger arrives while Character Progression is in `XpAwarding`, `LevelingUp`, or `XpAdjustmentPending`**: finish the transient progression transaction first, then expose the post-resolution stable state to Save/Load. Save serialization is valid only from `InitializedLevel1`, `HydratedFromSave`, `Ready`, or `LevelCapped`; no mid-award, mid-level-chain, or partially applied XP adjustment state may be persisted.

- **If `PlayerKillCreditEvent.defeated_source_ref` is missing, empty, or an unrecognized stable-source type**: reject the event receive-side, emit a diagnostic, do not transition state, and award no XP. T1 legal source refs are the stable forms already defined by Combat Core: persistent NPC source, non-persistent spawn source, or environmental combat source.

- **If `kill_weight_seed` is missing, NaN, negative, zero, or outside the expected T1 authored range**: reject the XP award, emit a diagnostic, and leave state unchanged. Do not clamp invalid upstream payloads silently.

- **If a kill is trivial by `level_difference_modifier`**: award 0 XP, emit `XPChangedEvent` only if UI/debug subscribers need an explicit 0-XP reason, and do not advance thresholds. Reputation and Zone Control may still consume their own Combat events independently.

- **If one XP award crosses multiple level thresholds**: resolve each level sequentially in one transaction, emit one `LevelChangedEvent` per level gained or one batched event carrying all gained levels, refresh permanent progression outputs after each level, and persist only the final stable state.

- **If an XP award crosses the T1 cap**: set `visible_level = 10`, store excess `total_xp` as over-cap XP, emit `LevelCapReached` if this is the first cap crossing, and grant no additional health, mana, spell eligibility, title, faction value, or hidden benefit.

- **If level-up increases permanent max health or max mana during combat**: publish updated permanent maxima for Combat Core to consume, but do not mutate `current_health`, `current_mana`, threat, cast, regen, med-break, or death state. Combat owns runtime current-resource handling.

- **If Save/Load hydrates class id other than `Cleric`, level below 1, level above 10 in T1, negative `total_xp`, XP below the current level threshold, missing formula data, or missing permanent baseline data**: return progression hydration failure to Save/Load. Do not silently clamp, downgrade, recalculate from defaults, or enter playable state.

- **If saved `total_xp` exceeds the level-10 threshold in T1**: hydrate as `visible_level = 10`, preserve `stored_total_xp`, recompute `overcap_xp`, and grant no over-cap T1 benefit.

- **If Death & Corpse Recovery requests an XP penalty before it is authored or without a valid death context**: reject the request with a diagnostic and leave XP unchanged. Character Progression exposes the penalty formula and adjustment interface only; it does not invent death timing.

- **If a death XP adjustment would reduce `total_xp` below the current level threshold**: apply the approved Death & Corpse Recovery policy when authored; until then, expose preview/debt values only. Character Progression does not decide whether deleveling exists in T1.

- **If `SpellEligibilityChanged` fires before Class Design or Spell Memorization exists**: persist the eligibility state and emit the event for future subscribers, but do not create spell records, spellbook entries, memorized slots, vendors, drops, UI buttons, or spell effects.

- **If a kill-credit event contains `faction_id`**: ignore it for progression mutation. Character Progression may pass through the original event identity in diagnostics, but it must not award reputation, faction rank, title, patron access, political state, or faction visual changes.

## Dependencies

Character Progression's dependency surface is intentionally narrow. It owns the mechanical class ladder, but it must not become a hidden dependency bridge into faction, world-state, combat-runtime, or account systems. Dependency direction matters: most systems below consume Character Progression outputs later; Character Progression does not mutate them.

Each downstream GDD listed below must declare Character Progression in its own Dependencies section when authored, with hard/soft classification matching this section unless that later GDD explicitly supersedes the relationship and surfaces the conflict. `/consistency-check` and `/review-all-gdds` should verify bidirectional agreement.

### Hard Direct Upstream

| System | Direction | Data Interface | Contract |
|---|---|---|---|
| **Character Creation** | Character Progression consumes | `starting_class_id = Cleric`; no separate T1 level/XP seed required | Character Creation owns first-run identity/class seed. Character Progression owns level 1 / 0 XP initialization. |
| **Save / Load & Persistence** | Bidirectional persistence client | `CharacterProgressionSaveState`; hydration validation result; save serialization from stable progression states only | Character Progression owns schema and validation. Save/Load owns serialization, HMAC, versioning, migration, write timing, and failure surfacing. |

### Hard Event Boundary

| System | Direction | Data Interface | Contract |
|---|---|---|---|
| **Combat Core** | Combat emits; Character Progression consumes and provides read-only baselines | Consumes `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)`; provides level, permanent max health/mana, and level-facing baseline snapshot | Combat owns kill/death event emission, runtime current resources, damage, threat, casting, regen, and death. Progression owns XP, level, and permanent level-derived outputs. |

### Future Hard Downstream

| System | Direction | Data Interface | Contract |
|---|---|---|---|
| **Class Design** | Class Design consumes Character Progression | Current level, class progression band, permanent baseline tables once authored | Class Design owns class content and final Cleric/Warrior/Enchanter class tables. Character Progression applies approved tables. |
| **Spell Memorization** | Spell Memorization consumes Character Progression | `SpellEligibilityChanged`; level eligibility query | Spell Memorization owns memorized slots, spellbook behavior, and availability presentation. Progression owns eligibility by level only. |
| **Death & Corpse Recovery** | Death system consumes Character Progression policy and requests adjustment | `death_xp_debt_preview`; XP adjustment/debt interface | Death system owns death timing, corpse recovery, resurrection, and penalty application. Progression owns XP target math and state mutation when called through approved interface. |
| **Faction Reputation** | Faction Reputation may consume Character Progression | Current level only if later approved for gating/context | Faction Reputation owns reputation values, identity ladder, rank labels, faction trust, political access, and display. Character Progression must not mutate reputation or social meaning. |

### Future Optional Consumers

| System | Direction | Data Interface | Contract |
|---|---|---|---|
| **Inventory & Item Economy** | Optional future consumer | Current level if item requirements later need it | Inventory owns items, equipment legality, drops, currency, and loot. Progression does not grant items. |
| **Layer 1 HUD** | UI consumes progression events | `XPChangedEvent`, `LevelChangedEvent`, `LevelCapReached`, `SpellEligibilityChanged` | HUD owns visual presentation. Progression owns quiet event timing and data payloads. |
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

- Save / Load already lists Character Progression as a hard persistence client and must remain consistent with this GDD's persistence whitelist.
- Combat Core owns the approved kill/death hooks and permanent-baseline handoff. If Combat Core is revised again, it should continue naming Character Progression as the owner of XP, level, permanent max resources, and spell eligibility.
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
| `max_xp_per_kill` | `600` | `300-900` | Named/elite kills may feel unrewarding relative to risk. | Single kills can skip too much of a level band. |
| `xp_award_minimum_nontrivial` | `1` | `1-10` | Valid non-trivial kills can floor to 0 after modifiers, making the formula feel broken. | Low-value farming stays relevant too long. Trivial kills still award 0 regardless of this knob. |
| `trivial_cutoff` | `-6` | `-8` to `-4` | Low-level kills remain farmable too long. | Slightly under-level enemies become unrewarding too quickly. |
| `diff_step` | `0.15` | `0.10-0.20` | Level difference barely matters. | Above-level kills become too efficient; below-level kills collapse too fast. |
| `min_nontrivial_modifier` | `0.25` | `0.10-0.50` | Non-trivial low-level kills feel indistinguishable from trivial kills. | Low-level farming remains too efficient. |
| `max_level_difference_modifier` | `1.60` | `1.25-2.00` | Above-level risk is under-rewarded. | Players may optimize reckless above-level kills if Combat tuning permits. |
| `encounter_role_multiplier_trash` | `1.0` | `0.8-1.2` | Normal camp XP feels weak. | Trash farming compresses the level curve. |
| `encounter_role_multiplier_elite` | `1.5` | `1.2-2.0` | Elite risk feels unrewarded. | Elite enemies become optimal farming targets. |
| `encounter_role_multiplier_named` | `3.0` | `2.0-4.0` | Named kills feel unrewarding relative to danger. | Named farming becomes the dominant XP strategy if respawn rules allow it. |
| `base_health` | `80` | `70-90` | Level 1 Cleric becomes too fragile for the approved Combat fixture. | Level 1 Cleric may trivialize early trash. |
| `base_mana` | `100` | `90-110` | Level 1 Cleric cannot sustain intended spell cadence. | Level 1 Cleric can over-cast before med-break rhythm matters. |
| `health_linear` | `131 / 9` | Fixture-derived; change only with Combat fixture revision | Breaks `Cleric_Mid_T1` / `Cleric_Top_T1` alignment. | Breaks Combat fixture alignment and soloability tests. |
| `health_quadratic` | `1 / 9` | Fixture-derived; change only with Combat fixture revision | Late T1 health growth under-runs fixture. | Late T1 health growth over-runs fixture. |
| `mana_linear` | `164 / 9` | Fixture-derived; change only with Combat fixture revision | Breaks med-break and spell-sustain fixture expectations. | Over-expands Cleric casting margin. |
| `mana_quadratic` | `4 / 9` | Fixture-derived; change only with Combat fixture revision | Late T1 mana growth under-runs fixture. | Late T1 mana growth over-runs fixture. |
| `resource_fixture_lock_levels` | `[1, 5, 10]` | T1 lock | Fixture alignment becomes implicit and easy to break. | More lock points make the formula brittle before real playtest data exists. |
| `levels_per_spell_tier` | `2` | `2-4` | Spell eligibility changes too often and risks content churn. | Spell eligibility feels sparse across 10 levels. |
| `max_t1_spell_tier` | `5` | `3-5` | T1 spell eligibility has too few beats. | Implies more spell content than Class Design / Spell Memorization may support. |
| `spell_tier_unlock_levels_t1` | `[1, 3, 5, 7, 9]` | Authored monotonic list inside levels 1-10 | Eligibility is too sparse or misses early onboarding beats. | Too many unlock beats imply spell content outside T1 capacity. |
| `death_penalty_min_level` | `4` | `4-6` | Death debt can punish onboarding before players understand stakes. | Death XP loss arrives too late to teach honest stakes. |
| `death_debt_pct` | `0.08` | `0.04-0.12` | Death debt is too soft to matter. | Death debt becomes punitive before Death & Corpse Recovery is validated. |
| `xp_debt_max_pct_of_level_band` | `0.12` | `0.08-0.20` | Death system has too little room to tune meaningful penalties. | Death debt can exceed the intended conservative T1 envelope. |
| `max_levels_per_xp_transaction` | `10` | `1-10` for T1 | Corrupt or oversized awards can run unchecked if no guard exists. | Higher than T1 cap is meaningless and weakens corruption safeguards. |

### Coupled Tuning Rules

- `xp_base`, `xp_exponent`, `xp_linear`, `base_xp_per_weight`, and `max_xp_per_kill` must be tuned together. Changing only one side can halve or double time-to-level.
- `trivial_cutoff`, `diff_step`, `min_nontrivial_modifier`, and `max_level_difference_modifier` define anti-farming behavior as a group.
- `encounter_role_multiplier_named` must be tuned against named respawn cadence and Combat Core's named soloability envelope. Named enemies should feel valuable, not become the dominant XP route.
- `health_linear`, `health_quadratic`, `mana_linear`, and `mana_quadratic` are fixture-alignment knobs. Changing them requires re-validating Combat Core's Cleric fixtures at `resource_fixture_lock_levels`.
- `death_debt_pct` and `xp_debt_max_pct_of_level_band` are preview-policy knobs only until Death & Corpse Recovery owns application timing.
- `levels_per_spell_tier`, `max_t1_spell_tier`, and `spell_tier_unlock_levels_t1` must be reconciled with Class Design and Spell Memorization before implementation.

## Visual/Audio Requirements

Character Progression is player-facing, but its audiovisual treatment must be restrained. It reports earned mechanical growth; it does not stage triumph, social promotion, or chosen-one status.

1. **No hero spectacle.** Level-up must not trigger hero glow, screen shake, camera punch, slow motion, bloom pulse, global lighting change, post-process change, particle burst, VFX ring, floating text explosion, center-screen banner, rarity color, or celebratory animation.

2. **Restrained ding only.** `LevelChangedEvent` may produce a quiet audio cue and a small HUD/UI state update when those systems are authored. Character Progression emits the event; Audio and Layer 1 HUD own playback and presentation. The cue should read as acknowledgement, not fanfare.

3. **XP movement is UI-only.** XP gain and threshold progress must not appear as world VFX, character VFX, floating numbers, overhead icons, nameplate effects, minimap pings, or enemy death effects. If shown, XP progress belongs to a restrained Layer 1 UI treatment.

4. **No character visual growth from level.** Level, XP, and permanent stat growth must not change player silhouette, scale, posture, garment quality, faction colors, material age, class ornamentation, lighting priority, facial treatment, portrait treatment, or animation status. These visual progressions belong elsewhere.

5. **Spell eligibility is not spell spectacle.** `SpellEligibilityChanged` may notify downstream systems that a new Cleric spell tier is eligible, but it must not play spell VFX, create spell icons, add spellbook entries, reveal vendors, or preview spell effects. Class Design, Spell Memorization, UI, and Audio own those later surfaces.

6. **Faction visual progression belongs to Faction Reputation.** Level-up must not add faction accents, faction materials, faction seals, rank marks, NPC recognition marks, political titles, or faction-specific posture. Reputation and faction participation own the identity ladder and its visual expression.

7. **Level cap is not a celebration.** Reaching `t1_level_cap` (`visible_level = 10`) uses the same restrained level-up treatment as any other ding. No completion banner, "T1 cap reached" celebration, badge, account marker, special sound, over-cap XP effect, or milestone visual is allowed. The cap is a soft ceiling, not a heroic achievement.

## UI Requirements

Character Progression exposes progression data and events for future UI systems; it does not own the final HUD, character sheet, journal, or spellbook presentation. UI must preserve the same mechanical-versus-identity boundary as the rest of this GDD.

1. **Data surfaces only.** Character Progression may expose `current_level`, `visible_level`, `total_xp`, `current_level_xp_progress`, `next_level_xp_threshold`, `overcap_xp` as data, `XPChangedEvent`, `LevelChangedEvent`, `LevelCapReached`, and `SpellEligibilityChanged`. It does not instantiate UI widgets.

2. **Layer 1 HUD owns presentation.** If XP, level, or spell eligibility appears in the HUD, it must be authored by the Layer 1 HUD / UX spec and obey the art bible's practical HUD constraints: peripheral, restrained, no glow, no gradient, no drop shadow, no center-screen reward treatment.

3. **No reward modal vocabulary.** UI must not present center-screen popups, quest-complete panels, level-up reward modals, loot-tier language, badge unlocks, achievement panels, account-level meters, battle-pass framing, over-cap celebration, or "T1 cap reached" completion treatment.

4. **XP progress is optional and restrained.** An XP bar or numeric XP text is not required by this GDD. If shown, it belongs to restrained Layer 1 UI. It must not become a faction reputation display, social rank display, server-status display, or achievement tracker.

5. **No raw hidden internals.** Shipping UI must not display `kill_weight_seed`, formula coefficients, encounter-role multipliers, trivial cutoff math, death-debt percent, Combat source refs, faction mutation assumptions, or raw diagnostic payloads. Debug builds may expose these behind explicit developer tooling only.

6. **Character sheet deferred.** Character Progression may provide data for a future character sheet or journal surface, but this GDD does not define that screen. Character-sheet layout, stat grouping, spell eligibility display, and journal integration require a future UX spec.

7. **Cap representation is quiet.** At `visible_level = t1_level_cap`, UI may show the current level as `10`, but it must not show progress past cap, stored over-cap XP, "progress toward Tier 2," percentage-to-future-cap, cap badge, cap celebration, or any account/achievement marker. Stored over-cap XP is data-only in T1.

## Acceptance Criteria

All criteria use the project QA taxonomy: Unit, Integration, Editor-validation, Dev-build smoke, or Profiled playtest. All are T1-blocking unless marked fixture-gated.

### Scope / Initialization

**H-CPRO-SCOPE-01 - T1 scope exclusions**
**GIVEN** the Character Progression authored config, **WHEN** the Editor validator inspects enabled features, **THEN** only local Cleric level `1-10` progression is enabled; no networking, account progression, PvP scaling, companion XP, alternate class, achievement, live-service, or LLM progression flags exist.
*Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-INIT-01 - Character Creation seed initializes progression**
**GIVEN** Character Creation emits `starting_class_id = Cleric`, **WHEN** Character Progression initializes a new T1 profile, **THEN** state is `level = 1`, `total_xp = 0`, no XP debt, no level-up pending state, and level-1 permanent baselines.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-INIT-02 - No separate level seed required**
**GIVEN** a valid Character Creation payload without level or XP fields, **WHEN** Character Progression initializes, **THEN** initialization succeeds from `starting_class_id = Cleric` alone.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

### XP Events

**H-CPRO-XP-01 - Kill-credit XP only**
**GIVEN** non-combat reward requests for quest, exploration, discovery, dialogue, faction, crafting, account, rested, or companion XP, **WHEN** they reach Character Progression, **THEN** no XP is awarded and a diagnostic is emitted in development builds.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-XP-02 - Valid kill credit awards XP once**
**GIVEN** a valid `PlayerKillCreditEvent` with legal `defeated_source_ref`, `kill_weight_seed`, level data, and encounter role, **WHEN** Character Progression processes it, **THEN** exactly one XP transaction is applied using `xp_award`.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-XP-03 - Duplicate kill credit dedupes**
**GIVEN** duplicate kill-credit events for the same defeated source and kill transaction, **WHEN** both are processed, **THEN** XP is awarded once, duplicate diagnostics emit, and no second threshold check occurs.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-XP-04 - Malformed source ref rejected**
**GIVEN** `PlayerKillCreditEvent.defeated_source_ref` is missing, empty, or not a legal persistent NPC, non-persistent spawn, or environmental source ref, **WHEN** the event is received, **THEN** state does not transition and XP remains unchanged.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-XP-05 - Invalid kill weight rejected**
**GIVEN** `kill_weight_seed` is missing, NaN, negative, zero, or outside the expected T1 authored range, **WHEN** XP is evaluated, **THEN** no clamping occurs, no XP is awarded, and a diagnostic emits.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-XP-06 - Trivial kill awards zero**
**GIVEN** `level_difference_modifier` returns `0`, **WHEN** XP is evaluated, **THEN** `xp_award = 0`, no threshold advances, and Faction Reputation / Zone Control remain independent consumers of their own events.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-XP-07 - Sequential multi-level resolution**
**GIVEN** a valid XP transaction crosses multiple thresholds, **WHEN** `LevelingUp` resolves, **THEN** levels apply sequentially, permanent outputs refresh after each level, and only the final stable state is save-eligible.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-XP-08 - Save waits for stable progression state**
**GIVEN** Save/Load requests serialization while Character Progression is in `XpAwarding`, `LevelingUp`, or `XpAdjustmentPending`, **WHEN** the request is handled, **THEN** the transaction completes first and Save/Load receives only a stable state.
*Integration | gameplay-programmer + engine-programmer + qa-tester | T1-blocking*

### Formulas

**H-CPRO-F1 - XP threshold table**
**GIVEN** default XP threshold parameters, **WHEN** `xp_threshold(level)` evaluates levels `1-10`, **THEN** outputs are exactly `0, 150, 540, 1210, 2170, 3430, 5010, 6910, 9140, 11710`.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-F2 - Level difference modifier**
**GIVEN** player level `5`, **WHEN** defeated level is `3`, **THEN** modifier is `0.70`; **WHEN** level delta is `-6`, **THEN** modifier is `0`.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-F3 - XP award example**
**GIVEN** level 5 Cleric defeats level 6 trash with `kill_weight_seed = 1.25`, **WHEN** defaults evaluate, **THEN** `xp_award = 71`.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-F4 - Permanent resource fixture locks**
**GIVEN** default resource formulas, **WHEN** levels `1`, `5`, and `10` evaluate, **THEN** health/mana outputs are `80/100`, `140/180`, and `220/300`, matching Combat Core fixtures.
*Unit + Editor-validation | gameplay-programmer + systems-designer | fixture-gated T1-blocking*

**H-CPRO-F5 - Spell eligibility tier**
**GIVEN** default spell eligibility parameters, **WHEN** level `7` evaluates, **THEN** `spell_eligibility_tier = 4`, and no spell records or memorized slots are created.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-F6 - Death debt preview**
**GIVEN** level `7` and default death-debt parameters, **WHEN** `death_xp_debt_preview` evaluates, **THEN** output is `150` XP debt preview and no penalty is applied unless Death & Corpse Recovery calls the approved interface.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-F7 - Cap overflow**
**GIVEN** level 10 at `11,710` XP earns `300` XP, **WHEN** cap overflow evaluates, **THEN** stored XP is `12,010`, visible level remains `10`, overcap XP is `300`, and no T1 benefit is granted.
*Unit | gameplay-programmer | T1-blocking*

### Save / Load Hydration

**H-CPRO-SL-01 - Persistence whitelist**
**GIVEN** a progression save payload, **WHEN** serialized fields are inspected, **THEN** it contains only approved progression state and excludes Combat runtime current resources, threat, casts, targets, cooldowns, runtime ids, and derived caches.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-SL-02 - Invalid hydration fails loud**
**GIVEN** Save/Load hydrates invalid class id, level, XP bounds, schema version, or missing baseline data, **WHEN** Character Progression validates, **THEN** it returns hydration failure and no playable session is enabled.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-SL-03 - Over-cap XP hydrates without benefit**
**GIVEN** saved `total_xp` exceeds the level-10 threshold, **WHEN** hydration completes, **THEN** visible level is 10, overcap XP is recomputed, and no over-cap stat/spell/social benefit appears.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-SL-04 - Stable-state serialization only**
**GIVEN** a save trigger during transient progression state, **WHEN** serialization occurs, **THEN** no partial XP award, partial multi-level chain, or partial XP adjustment is written.
*Integration | gameplay-programmer + engine-programmer | T1-blocking*

### Combat Boundaries

**H-CPRO-CB-01 - Combat event is consumed narrowly**
**GIVEN** a valid Combat kill-credit event, **WHEN** Character Progression processes it, **THEN** only legal XP inputs are read; threat tables, damage rolls, loot, corpse records, runtime current resources, and `combat_actor_id` are not read or persisted.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

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
**GIVEN** Death & Corpse Recovery is absent or sends no valid death context, **WHEN** XP adjustment is requested, **THEN** Character Progression rejects the request, emits a diagnostic, and leaves XP unchanged.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-DCS-03 - Deleveling remains undecided**
**GIVEN** a previewed XP penalty would reduce XP below the current level threshold, **WHEN** Death & Corpse Recovery is not yet authored, **THEN** Character Progression exposes preview/debt only and does not delevel.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-DCS-04 - Spell eligibility does not create spell content**
**GIVEN** `SpellEligibilityChanged` fires, **WHEN** downstream spell systems are absent, **THEN** no spell records, spellbook entries, memorized slots, vendors, drops, UI buttons, VFX, or spell effects are created.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CPRO-DCS-05 - Cap grants no hidden benefit**
**GIVEN** visible level is capped at 10 and XP continues accumulating, **WHEN** progression outputs are inspected, **THEN** no additional health, mana, spell eligibility, title, faction value, account marker, or hidden benefit is produced.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

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
**GIVEN** a corrupted or oversized XP transaction would advance more than `max_levels_per_xp_transaction`, **WHEN** Character Progression processes it, **THEN** processing stops at the guard, emits a diagnostic, and no unstable state is serialized.
*Unit | gameplay-programmer | T1-blocking*

**H-CPRO-TUNE-05 - Spell tier unlock list validator**
**GIVEN** `spell_tier_unlock_levels_t1` is authored, **WHEN** validation runs, **THEN** the list is monotonic, inside levels 1-10, and does not exceed `max_t1_spell_tier`.
*Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

### Summary

Total: 44 criteria.
Ordinary T1-blocking: 42.
Fixture-gated T1-blocking: 2 (`H-CPRO-F4`, `H-CPRO-TUNE-03`).
Advisory-at-T1: 0.

## Open Questions

[To be designed]
