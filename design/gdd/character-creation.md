# Character Creation

> **Status**: Designed (pending review)
> **Author**: Codex (session with brian, 2026-04-24)
> **Last Updated**: 2026-04-24
> **Last Verified**: 2026-04-24 - Phase 5 self-check readback
> **Implements Pillar**: Primary — **P3 Reputation Is The Progression**. Supports — **P1 The World Is Not Your Story** and **P5 Stakes Are Honest**.

## Locked Inputs

These inputs are authoritative for the Character Creation GDD. This GDD reproduces them rather than redesigning them. If a later section appears to conflict with this block, the later section is wrong.

1. **T1 scope lock** — Per [DECISIONS.md](../../DECISIONS.md) D002-D004 and [systems-index.md](systems-index.md), Character Creation is T1 offline single-player only: one active local character, no FishNet/networking, no account identity, no server character list, no live LLM behavior, and no multi-slot management. T1 class scope is Cleric only.

2. **Direct dependency lock** — Character Creation has exactly one direct upstream dependency: [save-load-persistence.md](save-load-persistence.md). Menus & Settings invokes the flow, World Structure provides the eventual start-zone contract, and future systems consume the seed data, but they are boundary contracts or downstream consumers, not additional direct dependencies.

3. **Save/Load contract** — Save/Load Rule 1 persists Player State, including character identity, class, appearance tokens, and Player-Authored Strings. Rule 2 exposes one active local character record at T1. Rule 12 requires player-authored strings to be length-capped and sanitized before downstream consumption. Rule 14 distinguishes expected first-run no-save bootstrap from missing-file data loss. Save/Load owns persistence, integrity, versioning, and load rejection; Character Creation owns the initial character-record schema.

4. **Menus title-flow contract** — Menus & Settings Rule 16 owns title-menu gating. New Game with no existing record enters the first-run path. New Game with an existing record requires Menus' destructive-overwrite confirmation naming the existing character. Continue with no record must not emit `LoadRejected`. Character Creation owns the creation flow after Menus has routed the player into it.

5. **Approved T1 design choices** — Character name is required and must validate to 3-24 characters after sanitization, using ASCII letters, numbers, spaces, hyphen, and apostrophe only. T1 has no bio/backstory field. T1 shows Cleric as the only visible/selectable class. The starting location is fixed at `CityHub_InnRoom_StartAnchor`. Character Creation seeds onboarding eligibility only; Sister Elara Mentor owns any introduction beat. Flow exit hands the initial payload to Menus + Save/Load, then gameplay begins only after first save and zone activation complete. Starting equipment comes from an authored Cleric equipment template; carried inventory starts empty. Starting faction reputation is neutral across all factions.

6. **Art bible contract** — A newly created character reads as a pre-faction resident, not a protagonist: undyed linen / rough wool, Bone Pale / Render Umber / Wick Gray neutral band, no faction-primary color above 5% surface area, no hero lighting, no silhouette enhancement, and Cleric occupation baseline as layered mid-length vertical emphasis. Character Creation may expose only the minimum appearance tokens needed to select or confirm that baseline at T1.

## Summary

Character Creation is the T1 first-run flow that turns Menus & Settings' New Game route into a valid initial Player State payload for Save/Load. It is deliberately small: the player enters a required sanitized character name, confirms the single T1 class (`Cleric`), accepts the pre-faction visual baseline, and starts from the fixed `CityHub_InnRoom_StartAnchor`. The system does not manage save slots, accounts, portraits, backstory, class balance, faction reputation progression, inventory rules, or Sister Elara's onboarding scene. Its job is to produce one trustworthy initial character record: name, class, minimum appearance tokens, neutral faction baseline seed, authored Cleric starting-equipment template reference, empty carried inventory, onboarding eligibility flag, and start-zone membership data for the first save. Menus owns title routing and loading presentation; Save/Load owns persistence and integrity; Character Creation owns validation and the shape of the initial character record.

## Overview

Character Creation is the narrow bridge between "no local save exists" and "one valid T1 character can be saved, loaded, and played." In T1, there is exactly one active local character record, and that character is a Cleric. The flow begins only after Menus & Settings routes New Game into Save/Load's first-run path; it ends by handing an initial character payload to Save/Load and returning control to the Menus loading surface until the first save and zone activation complete.

The design is intentionally not a broad RPG creator. Players do not pick a backstory, origin, faction, portrait, server account, save slot, or class roster. They choose a name that can survive Save/Load's player-authored string rules, confirm the Cleric baseline, and enter the city as a pre-faction resident. That restraint supports Pillar 3: reputation is earned in play, not pre-authored on a biography screen. It also supports Pillar 1: the character arrives as one person in an older city, not as the protagonist of the world.

The initial payload is a cross-system seed, so its boundaries are strict. Character Creation defines the character-record schema fields it owns, including name validation, class identity, minimum appearance tokens, starting location id, neutral faction baseline seed, starting equipment template reference, empty carried inventory, and onboarding eligibility. Save/Load serializes and validates persistence integrity. Menus presents the route and loading surface. Future Inventory, Faction Reputation, Character Progression, and Sister Elara Mentor GDDs consume the seed data when authored; they do not become direct dependencies of Character Creation at T1.

## Player Fantasy

Character Creation is not where the player invents a legend. It is where the city receives a name.

The player fantasy is restrained: becoming recordable, not important. The player enters a valid name, confirms the Cleric baseline, and crosses from "no local record exists" to "this person can now be saved, loaded, harmed, remembered, and judged." The character begins in plain pre-faction dress, with no patron, no declared allegiance, no authored history, no carried goods beyond the Cleric starting-equipment template, and no faction debt. Gravenspire does not inherit the player's imagined past; it waits for evidence.

### Anchor Moment

The final confirmation before first save: the player has named someone who can now accumulate consequence. The neutral faction baseline is not emptiness; it is the honest starting condition for Pillar 3. Reputation begins after creation, because the city should attach meaning only to what the player does in play.

### Anti-fantasy — what the player should NOT feel

- **"I am choosing my destiny"** — no origin, faction, biography, chosen-one framing, or declared future.
- **"I am customizing a hero"** — no protagonist silhouette, hero lighting, portrait spectacle, or broad cosmetic suite at T1.
- **"The world already owes me context"** — no hidden faction modifier, no prewritten relationship, no inherited patronage.
- **"This is the Sister Elara scene"** — Character Creation may seed onboarding eligibility, but Sister Elara Mentor owns her introduction beat.

### Reference Register

Municipal and ecclesiastical rather than heroic: the city ledger, the parish book, the admission record, the first mark beside a name. This shares Save/Load's archival register, but at an earlier moment: not the preservation of a lived record, but the creation of a person the record can begin to follow.

## Detailed Design

### Core Rules

1. **Entry is Menus-routed.** Character Creation may start only after Menus & Settings routes the player into New Game under Rule 16: no existing record first-run, or existing record after Menus has completed destructive-overwrite confirmation. Character Creation does not inspect, enumerate, delete, or overwrite save records itself.

2. **One T1 character record.** T1 produces exactly one active local character record. No save-slot selection, character list, account identity, server identity, import/export, or multi-character management is exposed or implied.

3. **Name is required and canonicalized before first save.** The player-authored name is sanitized before validation: control characters stripped, outer spaces trimmed, and repeated internal whitespace collapsed to a single space. The final canonical name must be 3-24 characters and match `^[A-Za-z0-9 '-]+$`. Invalid names block confirmation inline and emit no `LoadRejected`.

4. **No biography or origin fields.** Character Creation must not ask for, infer, display, save, or template a biography, origin, homeland, patron, vow, trauma, prior faction tie, or reason for waking in the inn. The name is an identity handle, not a backstory.

5. **Cleric is the only T1 class.** The only visible and selectable class id is `Cleric`. This is a mechanical class seed, not proof of ordination, sect membership, faith, faction, or moral position. Future classes remain absent from the T1 UI.

6. **Appearance is a fixed pre-faction baseline.** Character Creation may store only the minimum appearance tokens required to instantiate the approved T1 Cleric pre-faction resident baseline:
   - `appearance_profile_id = PreFactionResident_Cleric_T1`
   - `palette_id = Neutral_PreFaction_T1`
   - `class_visual_baseline_id = cleric_layered_midlength_vertical`

   These tokens are not cosmetic rewards, identity sliders, faction declarations, or protagonist markers. If scope pressure rises, the three-token set collapses to `appearance_profile_id` only.

7. **Resident test for appearance tokens.** Every T1 appearance token must pass the resident test: if the same token were applied to an ambient pre-faction NPC in the CityHub inn room, it would not make that NPC read as special, heroic, faction-aligned, wealthy, magical, or narratively selected. Faction-primary color coverage target is 0% at creation and must never exceed the art-bible 5% ceiling.

8. **Initial character record schema.** Character Creation owns this initial payload shape:

   ```yaml
   InitialCharacterRecord:
     local_character_id: generated opaque id, immutable, not player-authored
     creation_schema_version: 1
     character_name: sanitized canonical display name
     starting_class_id: Cleric
     appearance_tokens:
       appearance_profile_id: PreFactionResident_Cleric_T1
       palette_id: Neutral_PreFaction_T1
       class_visual_baseline_id: cleric_layered_midlength_vertical
     start_anchor_id: CityHub_InnRoom_StartAnchor
     player_zone_membership:
       zoneId: resolved from start_anchor_id
       position: resolved world-space Vector3 from start_anchor_id
       zoneType: CityHubZone
     onboarding_eligible: true
     onboarding_intro_state: pending
     starting_equipment_template_id: ClericStartingEquipment_T1
     carried_inventory: []
     starting_faction_reputation_baseline: NeutralAllFactions
   ```

   Raw name input, bio/backstory text, account id, save-slot label, faction choice, patron id, portrait id, cosmetic freeform fields, runtime handles, Addressable handles, and scene object references are not part of this schema.

9. **Start anchor is not an origin story.** `start_anchor_id = CityHub_InnRoom_StartAnchor` is a spawn/location seed only. It resolves to `PlayerZoneMembership` before the first save. Character Creation must not explain why the character is there.

10. **Starting equipment is a template reference, not item ownership rules.** Character Creation records `starting_equipment_template_id = ClericStartingEquipment_T1` and `carried_inventory = []`. Inventory & Item Economy later owns materializing item records, slot legality, item schema, stack rules, and equipment validation.

11. **Faction baseline is neutral seed data.** `starting_faction_reputation_baseline = NeutralAllFactions` means no earned reputation yet. It does not mean innocent, unknown to everyone, apolitical, protected, or newly arrived. Faction Reputation later owns numeric ranges, rank labels, mutation rules, and per-faction display.

12. **Onboarding eligibility is only a seed.** Character Creation sets `onboarding_eligible = true` and `onboarding_intro_state = pending`. It does not author Sister Elara's introduction, dialogue, staging, behavior, motivations, companion state, relationship state, or departure. Sister Elara Mentor owns those beats when authored.

13. **Validation precedes first save.** The payload must pass validation before handoff to Save/Load: name valid, class exactly `Cleric`, appearance tokens whitelisted, start anchor resolvable, zone membership resolved, equipment template id present, carried inventory empty, and faction baseline complete for all authored factions.

14. **Creation validation failures are local.** `NameTooShort`, `NameTooLong`, `NameDisallowedChars`, `ClassUnavailable`, `AppearanceTokenInvalid`, `StartAnchorUnresolved`, `StartingEquipmentTemplateMissing`, and `FactionBaselineIncomplete` block submission inside Character Creation. They are not Save/Load failures and must not emit `LoadRejected`.

15. **First save gates gameplay.** After payload validation, Character Creation hands the initial record to Save/Load and returns control to Menus' loading surface. Gameplay cannot begin until the first save succeeds and the start zone reaches playable activation. If the first save fails, Save/Load emits `SaveFailedEvent` and Menus owns the fail-loud surface.

16. **Save/Load ownership boundary.** Character Creation owns schema and validation of the initial record. Save/Load owns serialization, HMAC/versioning, first-run vs missing-file distinction, write failure handling, and later hydration of Player State. Character Creation never writes storage directly.

17. **Tier discipline.** T1 Character Creation contains no networking authority, replicated identity, account binding, server character list, multi-slot flow, live LLM prompt seed, generated backstory, character deletion, portrait system, or cosmetic unlock system.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Unentered` | Title menu active; Character Creation not routed | Menus routes New Game first-run or confirmed overwrite -> `AwaitingName` | No payload exists. Character Creation does not query or mutate save records. |
| `AwaitingName` | Creation flow opens | Valid canonical name accepted -> `NameValid`; cancel -> `CancelledToTitle`; invalid input -> `NameRejected` | Captures raw player text in transient UI state only. No save payload write. |
| `NameRejected` | Name fails sanitization or validation | Player edits input -> `AwaitingName` | Shows inline rejection. Emits no `LoadRejected`; does not call Save/Load. |
| `NameValid` | Canonical `character_name` passes Rule 3 | Cleric confirmation accepted -> `ClericConfirmed`; player edits name -> `AwaitingName` | Holds sanitized canonical name in transient creation state. |
| `ClericConfirmed` | `starting_class_id = Cleric` confirmed | Appearance baseline confirmed -> `AppearanceBaselineConfirmed` | No alternate class ids are visible or stored. |
| `AppearanceBaselineConfirmed` | T1 pre-faction Cleric tokens accepted | Payload validation requested -> `PayloadValidated` | Uses whitelisted appearance tokens only. No cosmetic sliders or faction choices. |
| `PayloadValidated` | All schema fields pass validation | Submit -> `FirstSavePending`; validation failure -> relevant local rejection state | Builds `InitialCharacterRecord`; resolves `CityHub_InnRoom_StartAnchor` to `PlayerZoneMembership`. |
| `FirstSavePending` | Valid payload handed to Save/Load | `SaveWriteConfirmed` -> `ZoneActivationPending`; `SaveFailedEvent` -> `FirstSaveFailed` | Menus loading surface owns presentation. Character Creation does not enter gameplay. |
| `FirstSaveFailed` | Save/Load reports first save failure | Menus handles failure acknowledgement / retry policy | No playable session starts from an unsaved initial payload. |
| `ZoneActivationPending` | First save confirmed; start zone activation in progress | Start zone playable -> `CompletePlayable`; zone failure -> Menus / World Structure failure surface | Waits for normal start-zone activation ordering. |
| `CompletePlayable` | First save confirmed and start zone playable | Gameplay owns control | Character Creation is complete for this record. |
| `CancelledToTitle` | Player cancels before first save submission | Menus title surface resumes | No save mutation; no partial character record persists. |

### Interactions with Other Systems

| System | Inputs Consumed by Character Creation | Outputs Published by Character Creation | Ownership Boundary | Dependency |
|---|---|---|---|---|
| **Save / Load & Persistence** | First-run availability via Menus route; `SaveWriteConfirmed`; `SaveFailedEvent`; later hydrated Player State is Save/Load-owned | Valid `InitialCharacterRecord` for first save | Character Creation owns initial schema and validation. Save/Load owns persistence, integrity, versioning, first-run/missing-file distinction, and failure classes. | **Hard direct dependency** |
| **Menus & Settings** | New Game routing; existing-record overwrite confirmation already completed; loading/failure surfaces | Creation-complete handoff; local validation status for presentation | Menus owns title gating, destructive-overwrite confirmation, loading surface, and player-facing failure modals. Character Creation owns the flow after route entry. | **Boundary contract; not direct dependency** |
| **World Structure** | No direct event dependency in this GDD | `player_zone_membership` resolved from `CityHub_InnRoom_StartAnchor` for Save/Load payload | World Structure owns zone ids, zone type, anchor validity, and zone activation. Character Creation owns selecting the fixed start anchor id. | **Boundary contract via Save/Load payload** |
| **Inventory & Item Economy** | None in T1 | `starting_equipment_template_id = ClericStartingEquipment_T1`; `carried_inventory = []` | Inventory owns item schema, equipment slots, item materialization, and validation. Character Creation only seeds the template reference. | **Future downstream consumer** |
| **Faction Reputation** | None in T1 | `starting_faction_reputation_baseline = NeutralAllFactions` | Faction Reputation owns numeric values, tiers, labels, and reputation mutation. Character Creation only seeds neutral baseline. | **Future downstream consumer** |
| **Character Progression** | None in T1 | `starting_class_id = Cleric`; optional future level/XP seed only if Character Progression requires it | Character Progression owns XP, level, spells, and progression rules. Character Creation owns starting class identity. | **Future downstream consumer** |
| **Sister Elara Mentor** | None in T1 | `onboarding_eligible = true`; `onboarding_intro_state = pending` | Sister Elara Mentor owns intro scene, behavior teaching, dialogue, companion state, and departure timing. Character Creation only seeds eligibility. | **Future downstream consumer** |
| **Dialogue System** | None | No dialogue seed, LLM prompt seed, origin text, or backstory field | Dialogue owns templated dialogue and any future LLM behavior. Character Creation must not create dialogue memory or prompt material. | **Future downstream consumer** |
| **Art Bible / Visual Implementation** | T1 pre-faction Cleric constraints | Appearance token ids only | Art bible owns visual rules; implementation owns models/materials. Character Creation owns only token selection/validation. | **Source-of-truth contract** |

## Formulas

Character Creation has no progression, economy, combat, randomization, weighting, or scaling formulas in T1. Its only deterministic predicate is canonical character-name validation.

The `valid_character_name` predicate is defined as:

`valid_character_name = length_ok AND allowed_chars_ok AND contains_alphanumeric`

**Variables:**

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| `raw_name` | `R` | string | Player input | Transient unsaved input from the name field. |
| `canonical_name` | `C` | string | 0-24+ chars before validation | `R` after control-character stripping, outer-space trimming, and repeated internal whitespace collapse. |
| `character_name_min_length` | `L_min` | int | 3 | Minimum canonical name length. Registry candidate. |
| `character_name_max_length` | `L_max` | int | 24 | Maximum canonical name length. Registry candidate. |
| `character_name_allowed_chars_regex` | `Rx` | regex | `^[A-Za-z0-9 '-]+$` | Allowed ASCII letters, numbers, spaces, hyphen, and apostrophe. Registry candidate. |
| `length_ok` | — | bool | true / false | True when `character_name_min_length <= length(canonical_name) <= character_name_max_length`. |
| `allowed_chars_ok` | — | bool | true / false | True when `canonical_name` matches `character_name_allowed_chars_regex`. |
| `contains_alphanumeric` | `A` | bool | true / false | True when `C` contains at least one ASCII letter or digit. Prevents punctuation-only names. |

**Output Range:** boolean `true` or `false`. A `false` result blocks Character Creation submission locally and emits no `LoadRejected`.

**Example:** Raw input `"  Sister   O'Har  "` canonicalizes to `"Sister O'Har"`. Length is 12, allowed characters match, and alphanumeric content exists, so `valid_character_name = true`.

## Edge Cases

- **If raw name is empty, whitespace-only, or control-character-only**: sanitize first, then reject as `NameTooShort`. The failure is local to Character Creation and emits no `LoadRejected`.
- **If raw name contains leading/trailing spaces or repeated internal whitespace**: canonicalize before display and save. The canonical name is the only name passed to Save/Load.
- **If canonical name is shorter than 3 characters**: reject as `NameTooShort`; do not pad, default, or auto-generate a name.
- **If canonical name is longer than 24 characters**: reject as `NameTooLong`; do not silently truncate.
- **If canonical name contains emoji, accents, underscores, periods, smart quotes, tabs, newlines, or symbols outside `^[A-Za-z0-9 '-]+$`**: reject as `NameDisallowedChars`; do not transliterate or replace characters.
- **If canonical name contains only spaces, hyphens, or apostrophes after sanitization**: reject as `NameDisallowedChars`; the name must contain at least one ASCII letter or digit.
- **If New Game is selected while an existing save exists but Menus has not completed destructive-overwrite confirmation**: Character Creation must not open. Menus owns the confirmation.
- **If an overwrite was confirmed but the player cancels before first save submission**: return to title and persist no partial record. The prior save remains intact unless/until Save/Load successfully commits the replacement record.
- **If no existing save exists on first-run New Game**: Menus routes into Character Creation; Character Creation does not inspect storage and does not emit `LoadRejected`.
- **If hidden UI state or debug tooling submits `starting_class_id` other than `Cleric`**: reject as `ClassUnavailable`; no future-class id is persisted in T1.
- **If hidden UI state submits unknown or non-whitelisted appearance tokens**: reject as `AppearanceTokenInvalid`; do not fall back to a random or default cosmetic set after validation has failed.
- **If hidden UI state submits non-empty `carried_inventory`**: reject with the closest local validation failure and do not pass the payload to Save/Load.
- **If `CityHub_InnRoom_StartAnchor` is missing, disabled, belongs to the wrong zone, or cannot resolve to `zoneId`, world-space `Vector3`, and `zoneType`**: reject as `StartAnchorUnresolved`; block first save.
- **If `ClericStartingEquipment_T1` is missing from authored data**: reject as `StartingEquipmentTemplateMissing`; do not create fallback items.
- **If `NeutralAllFactions` does not cover every currently authored faction baseline**: reject as `FactionBaselineIncomplete`; do not infer missing faction rows.
- **If payload validation succeeds but the first save write fails**: Save/Load emits `SaveFailedEvent`; Menus owns the fail-loud surface; gameplay must not start.
- **If first save succeeds but start-zone activation fails**: remain non-playable and route to the Menus / World Structure failure surface. Character Creation does not implement zone retry or relocation.
- **If the player cancels or backs out before first save submission**: discard transient raw name, canonical name, and hidden creation state; return to title with no save mutation.
- **If downstream systems later consume Character Creation seed data**: they must treat it as seed data only. Inventory must not assume materialized item instances; Faction Reputation must not infer allegiance; Character Progression must not infer XP/level schema beyond `Cleric`; Sister Elara Mentor must not assume relationship state; Dialogue must not infer backstory or LLM memory.

## Dependencies

Character Creation's dependency graph is intentionally narrow. The only direct upstream system dependency is Save / Load & Persistence. Menus, World Structure, and the art bible provide boundary contracts; future gameplay systems consume Character Creation seed data after their own GDDs are authored.

### Direct upstream dependency

| System | Direction | Nature / Data Interface | Hard/Soft | Interface Owner |
|---|---|---|---|---|
| **Save / Load & Persistence** | Character Creation depends on | Save/Load provides the first-run path, one-active-local-record constraint, player-authored string sanitation contract, first-save write outcome, and later Player State hydration. Character Creation provides the validated `InitialCharacterRecord` for first save. | **Hard** | Character Creation owns initial schema + validation. Save/Load owns persistence, integrity, versioning, first-run/missing-file distinction, write failures, and load rejection. |

### Boundary contracts, not direct dependencies

| Source | Contract Used | Why It Is Not a Direct Dependency |
|---|---|---|
| **Menus & Settings** | Title-menu New Game routing, overwrite confirmation, loading/failure presentation. | Menus invokes the flow and owns presentation; Character Creation does not depend on Menus state beyond being routed in. |
| **World Structure** | `CityHub_InnRoom_StartAnchor` resolves to `PlayerZoneMembership` (`zoneId`, world-space `Vector3`, `zoneType = CityHubZone`) and later zone activation. | Character Creation writes the resolved start membership into the initial payload; it does not subscribe to World Structure events in this GDD. |
| **Art Bible** | Pre-faction Cleric visual baseline, no protagonist framing, faction color ceiling, resident test. | Art Bible is a source-of-truth constraint, not a runtime system dependency. |

### Future downstream consumers

| Future System | Character Creation Seed Consumed | Reverse-Listing Obligation |
|---|---|---|
| **Character Progression** | `starting_class_id = Cleric`; optional future level/XP seed if required. | Must list Character Creation as seed-provider when authored. |
| **Inventory & Item Economy** | `starting_equipment_template_id = ClericStartingEquipment_T1`; `carried_inventory = []`. | Must own item materialization, equipment slots, item schema, and starting-template validation. |
| **Faction Reputation** | `starting_faction_reputation_baseline = NeutralAllFactions`. | Must own numeric baseline values, tiers, labels, mutation rules, and reverse-list Character Creation as baseline seed-provider. |
| **Sister Elara Mentor** | `onboarding_eligible = true`; `onboarding_intro_state = pending`. | Must own introduction beat, behavior teaching, dialogue, companion state, relationship state, and departure timing. |
| **Dialogue System** | No seed data. Explicit absence of bio/backstory/LLM prompt seed. | Must not infer backstory or LLM memory from Character Creation seed fields. |

### Scoped follow-up amendments

Character Creation introduces seed fields that should be reflected in Save/Load Rule 1 before implementation: `local_character_id`, nested `creation_schema_version`, `starting_class_id`, appearance token ids, `onboarding_eligible`, `onboarding_intro_state`, `starting_equipment_template_id`, empty carried-inventory state, and `starting_faction_reputation_baseline`. `PlayerZoneMembership` remains under Save/Load's World State category, resolved from `CityHub_InnRoom_StartAnchor` before first save. This is a scope-guarded Save/Load amendment, not an expansion of Character Creation's direct dependency graph.

## Cross-References

| Contract | Source | Character Creation Usage |
|---|---|---|
| T1 is offline single-player; no networking, accounts, server backend, or live LLM calls | [DECISIONS.md](../../DECISIONS.md) D002-D004 | Rules 2 and 17 prohibit netcode/account/server/LLM/multi-slot scope. |
| Character Creation is design-order #6, MVP Core, `S`, direct dependency Save/Load only | [systems-index.md](systems-index.md) | Locks this GDD's size and dependency graph. |
| Player State includes character identity and Player-Authored Strings; T1 has one active local character; player-authored strings are sanitized; first-run is distinct from missing-file failure | [save-load-persistence.md](save-load-persistence.md) Rules 1, 2, 12, 14 | Rules 3, 8, 14, 15, and 16 reproduce these contracts. |
| Title-menu save-record gating and New Game overwrite confirmation | [menus-settings.md](menus-settings.md) Rule 16, H-MS-SL-10, H-MS-SL-11 | Rule 1 and Edge Cases keep title routing Menus-owned. |
| `PlayerZoneMembership` shape and `CityHubZone` / `HauntZone` zone types | [world-structure.md](world-structure.md) | Rule 8 and Rule 9 resolve `CityHub_InnRoom_StartAnchor` into start-zone payload fields. |
| New player reads as pre-faction resident; Cleric silhouette is layered mid-length vertical; no player protagonist visual priority | [art-bible.md](../art/art-bible.md) §3.1 and §5.1 | Rules 6 and 7 constrain appearance tokens and resident test. |
| Sister Elara onboarding is a later authored beat, not Character Creation content | [art-bible.md](../art/art-bible.md) §7.5; [npc-system.md](npc-system.md) Rule 19 | Rule 12 seeds eligibility only and defers the scene to Sister Elara Mentor. |
| NPC-facing string safety convention: future player-authored strings must be bounded and sanitized before NPC consumption | [npc-system.md](npc-system.md) Rule 18 | Character Creation creates no NPC-facing player-authored string beyond character name. |
| Unity runtime UI substrate | [ui.md](../../docs/engine-reference/unity/modules/ui.md); [menus-settings.md](menus-settings.md) Rule 15 | UI Requirements use UI Toolkit unless a later UI ADR overrides it. |
| Unity input substrate | [breaking-changes.md](../../docs/engine-reference/unity/breaking-changes.md); [menus-settings.md](menus-settings.md) Rule 11 | UI Requirements use Input System actions and avoid legacy input APIs. |

## Tuning Knobs

Character Creation has no live balance knobs, random weights, or progression tuning in T1. Its editable values are authored constants that affect validation, seed payloads, or content routing; changes require cross-system review because Save/Load fixtures and downstream GDDs consume them.

| Knob / Constant | Default | Safe Range / Allowed Values | Too Low / Narrow | Too High / Broad |
|---|---|---|---|---|
| `character_name_min_length` | 3 | 1-8 | One-character names reduce identity weight and increase accidental entries. | Overly strict names block ordinary short names. |
| `character_name_max_length` | 24 | 12-32 | Blocks ordinary multi-part names. | Risks UI overflow and save-fixture drift. |
| `character_name_allowed_chars_regex` | `^[A-Za-z0-9 '-]+$` plus at least one alphanumeric | ASCII letters, numbers, spaces, hyphen, apostrophe at T1 | Rejects too many ordinary names. | Introduces localization/input complexity before T2+ i18n. |
| `appearance_profile_id` | `PreFactionResident_Cleric_T1` | T1 whitelisted pre-faction Cleric profiles only | Too little visual confirmation of the Cleric baseline. | Becomes a cosmetic creator or faction declaration. |
| `starting_equipment_template_id` | `ClericStartingEquipment_T1` | Authored Cleric templates only | Missing or under-specified equipment blocks first save. | Starts to define inventory/economy rules outside Inventory & Item Economy. |
| `start_anchor_id` | `CityHub_InnRoom_StartAnchor` | Fixed at T1 | Invalid or absent anchor blocks first save. | Multiple starts imply origin/location choice outside T1 scope. |
| `starting_faction_reputation_baseline` | `NeutralAllFactions` | Neutral authored baseline only at T1 | Hidden penalty violates honest starting state. | Hidden advantage undermines reputation as earned progression. |

## Visual/Audio Requirements

Character Creation uses the art bible's pre-faction resident register. The created character appears in plain Cleric baseline dress: undyed linen / rough wool, Bone Pale / Render Umber / Wick Gray neutral band, layered mid-length vertical emphasis, and no faction-primary color target above 0% at creation. The 5% art-bible ceiling remains a hard maximum for incidental contamination only, not a design target.

The character preview, if shown, is modest and non-heroic. It must not use hero lighting, rim lighting, silhouette enhancement, class VFX, faction color accents, rarity colors, dramatic pose changes, portrait framing, or camera treatment that makes the player read louder than an equivalent pre-faction NPC.

No T1 visual surface may expose face sculpting, portraits, cosmetics, makeup, jewelry, tattoos, faction garments, faction insignia, class carousel art, disabled future-class silhouettes, or backstory/origin imagery. Faction-earned visual progression belongs to Faction Reputation and future visual progression work, not Character Creation.

Audio is restrained. Name acceptance and Create confirmation may use at most a low UI tick in Menus' quiet register. No heroic sting, class fanfare, faction motif, choir swell, mentor voice line, or onboarding narration plays from Character Creation.

## UI Requirements

Character Creation is a Unity UI Toolkit runtime screen unless a later UI ADR explicitly overrides it. Input handling uses Unity Input System actions; legacy `Input.GetKey`, direct mouse polling, and deprecated `VisualElement.transform` usage are out of scope for implementation.

The T1 layout is one restrained record-style creation panel plus a modest pre-faction character preview. It presents only:
- `Name` text field, required.
- `Class` single selected option: `Cleric`.
- Fixed appearance preview using the T1 pre-faction Cleric baseline.
- Primary action: `Create`.
- Secondary action: `Back`.

The UI must not present a bio field, origin selector, faction selector, patron selector, portrait selector, class carousel, disabled future-class list, cosmetic grid, sliders, swatches, or explanatory tutorial text.

Validation feedback is local and inline under `Name`. `Create` stays disabled until the canonical name is valid. Allowed messages are terse and specific: `3-24 characters`, `Letters, numbers, spaces, apostrophes, and hyphens only`, or `Use at least one letter or number`. Local validation never emits `LoadRejected` and never displays Save/Load failure UI.

Initial focus lands in `Name`. `Tab` / `Shift+Tab` cycles through interactive controls. `Enter` submits only when `Create` is enabled. `Esc` or `Back` returns to Menus title surface before first-save submission. Once submitted, creation controls lock and no further edits/back navigation are available from Character Creation.

On valid `Create`, Character Creation emits only the validated initial payload. Menus owns the loading surface immediately after submission. Gameplay begins only after first save succeeds and `CityHub_InnRoom_StartAnchor` zone activation is playable. First-save failure, zone activation failure, overwrite confirmation, title routing, and return-to-title failure surfaces belong to Menus / Save/Load / World Structure, not Character Creation.

## Acceptance Criteria

All Character Creation acceptance criteria use the project QA taxonomy: Unit, Integration, Editor-validation, Dev-build smoke, or Profiled playtest. All are T1-blocking unless explicitly marked advisory-at-T1.

**H-CC-01 - Menus-routed entry and T1 scope**
**GIVEN** the title menu is active, **WHEN** Character Creation opens, **THEN** the route source is Menus' New Game first-run path or Menus' completed destructive-overwrite confirmation path; Character Creation does not inspect storage directly, delete saves, enumerate slots, bind accounts, contact servers, or create network identity.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-CC-02 - Existing-record overwrite remains Menus-owned**
**GIVEN** Save/Load reports an existing T1 active local character record, **WHEN** New Game is selected and destructive overwrite is cancelled, **THEN** Character Creation never opens, no gameplay save data mutates, and control returns to the title menu.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-CC-03 - Name canonicalization**
**GIVEN** raw name input containing control characters, leading spaces, trailing spaces, and repeated internal whitespace, **WHEN** Character Creation canonicalizes the input, **THEN** control characters are stripped, outer spaces are trimmed, repeated internal whitespace collapses to one space, and only the canonical value can advance to validation.
*Unit | gameplay-programmer | T1-blocking*

**H-CC-04 - Name length bounds**
**GIVEN** canonical names of length 2, 3, 24, and 25, **WHEN** `valid_character_name` evaluates length, **THEN** lengths 3 and 24 pass the length check; lengths 2 and 25 fail with `NameTooShort` and `NameTooLong` respectively.
*Unit | gameplay-programmer | T1-blocking*

**H-CC-05 - Name allowed-character set**
**GIVEN** canonical names containing ASCII letters, numbers, spaces, hyphen, apostrophe, emoji, accents, underscores, periods, smart quotes, tabs, newlines, and other symbols, **WHEN** `valid_character_name` evaluates allowed characters, **THEN** only names matching `^[A-Za-z0-9 '-]+$` pass; invalid names fail locally as `NameDisallowedChars` and emit no `LoadRejected`.
*Unit | gameplay-programmer | T1-blocking*

**H-CC-06 - Name requires alphanumeric content**
**GIVEN** canonical names made only of spaces, hyphens, apostrophes, or combinations of those characters, **WHEN** `valid_character_name` evaluates `contains_alphanumeric`, **THEN** the names fail as `NameDisallowedChars`; at least one ASCII letter or digit is required.
*Unit | gameplay-programmer | T1-blocking*

**H-CC-07 - Invalid names are not repaired silently**
**GIVEN** a canonical name that is too long, too short, or contains disallowed characters, **WHEN** validation fails, **THEN** Character Creation does not truncate, pad, transliterate, replace characters, auto-generate a name, or fall back to a default.
*Unit | gameplay-programmer | T1-blocking*

**H-CC-08 - Invalid-name UI blocks creation locally**
**GIVEN** the player enters an invalid name, **WHEN** the Character Creation UI updates, **THEN** `Create` remains disabled or submission is rejected inline with one of the allowed local messages; no Save/Load write request is sent and no `LoadRejected` event is emitted.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-CC-09 - InitialCharacterRecord required fields**
**GIVEN** a valid name and default T1 creation state, **WHEN** Character Creation builds `InitialCharacterRecord`, **THEN** the payload includes non-default or explicitly defined values for `local_character_id`, `creation_schema_version = 1`, canonical `character_name`, `starting_class_id = Cleric`, appearance tokens, `start_anchor_id`, resolved `player_zone_membership`, `onboarding_eligible = true`, `onboarding_intro_state = pending`, `starting_equipment_template_id = ClericStartingEquipment_T1`, `carried_inventory = []`, and `starting_faction_reputation_baseline = NeutralAllFactions`.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CC-10 - InitialCharacterRecord excludes forbidden fields**
**GIVEN** a valid `InitialCharacterRecord`, **WHEN** the outgoing first-save payload is inspected before persistence, **THEN** raw name input, bio/backstory text, origin, homeland, patron id, faction choice, account id, save-slot label, portrait id, cosmetic freeform fields, runtime handles, Addressable handles, scene object references, dialogue memory, and live LLM prompt seed are absent.
*Unit | gameplay-programmer | T1-blocking*

**H-CC-11 - Cleric-only class validation**
**GIVEN** hidden UI state, debug tooling, or a test harness submits any `starting_class_id` other than `Cleric`, **WHEN** payload validation runs, **THEN** submission fails locally as `ClassUnavailable`; no future-class id is persisted and no Save/Load write request is sent.
*Unit | gameplay-programmer | T1-blocking*

**H-CC-12 - No future-class UI promise**
**GIVEN** the T1 Character Creation UI tree is enumerated, **WHEN** visible and disabled controls are inspected, **THEN** `Cleric` is the only visible/selectable class and there are no disabled Warrior/Enchanter options, future-class silhouettes, class carousel, or future-class explanatory text.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-CC-13 - Pre-faction resident appearance validator**
**GIVEN** the authored T1 Character Creation appearance profile, palette, and class visual baseline assets, **WHEN** the Editor validator runs, **THEN** only `PreFactionResident_Cleric_T1`, `Neutral_PreFaction_T1`, and `cleric_layered_midlength_vertical` are whitelisted; no portrait, cosmetic grid, slider, faction garment, faction insignia, hero-lighting flag, class VFX, or faction-primary color target above the art-bible ceiling is present.
*Editor-validation | technical-artist + qa-tester | T1-blocking*

**H-CC-14 - Start anchor validator**
**GIVEN** authored World Structure start-anchor data, **WHEN** the Editor validator resolves `CityHub_InnRoom_StartAnchor`, **THEN** the anchor exists, is enabled, resolves to a stable `zoneId`, world-space `Vector3` position, and `zoneType = CityHubZone`, and is treated only as a spawn/location seed rather than an origin-story field.
*Editor-validation | engine-programmer + qa-tester | T1-blocking*

**H-CC-15 - Equipment template and faction baseline validator**
**GIVEN** authored Character Creation seed references, **WHEN** the Editor validator runs, **THEN** `ClericStartingEquipment_T1` exists as a template reference, `carried_inventory` is empty, and `NeutralAllFactions` covers every currently authored faction baseline without inferring allegiance, patronage, protection, advantage, or penalty.
*Editor-validation | gameplay-programmer + qa-tester | T1-blocking*

**H-CC-16 - Validation precedes first save**
**GIVEN** any invalid field among name, class, appearance tokens, start anchor, zone membership, equipment template, carried inventory, or faction baseline, **WHEN** Character Creation attempts to submit, **THEN** validation blocks before Save/Load handoff and returns the relevant local failure class (`NameTooShort`, `NameTooLong`, `NameDisallowedChars`, `ClassUnavailable`, `AppearanceTokenInvalid`, `StartAnchorUnresolved`, `StartingEquipmentTemplateMissing`, or `FactionBaselineIncomplete`).
*Unit | gameplay-programmer | T1-blocking*

**H-CC-17 - Valid Create locks editing and hands off presentation**
**GIVEN** a valid creation payload, **WHEN** the player selects `Create`, **THEN** Character Creation emits only the validated payload, locks name/class/appearance/back navigation, and Menus immediately owns the loading surface.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-CC-18 - Save/Load owns persistence and first-save failure**
**GIVEN** a valid payload has been handed to Save/Load, **WHEN** Save/Load emits `SaveWriteConfirmed` or `SaveFailedEvent`, **THEN** Character Creation does not write storage, compute HMAC/version fields, classify load failures, or show first-save failure UI; `SaveFailedEvent` leaves the player non-playable and routes to the Menus / Save/Load fail-loud surface.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CC-19 - Gameplay waits for first save and zone activation**
**GIVEN** `Create` has been submitted, **WHEN** `SaveWriteConfirmed` has not emitted or `CityHub_InnRoom_StartAnchor` has not reached playable zone activation, **THEN** gameplay control is not granted; only after both conditions are true may the flow enter playable control.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-CC-20 - Required controls and keyboard path**
**GIVEN** Character Creation opens, **WHEN** UI focus and keyboard navigation are exercised, **THEN** initial focus lands in `Name`; `Tab` / `Shift+Tab` cycles through `Name`, `Create`, and `Back`; `Enter` submits only when `Create` is enabled; `Esc` or `Back` returns to Menus before first-save submission.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-CC-21 - End-to-end first-save hydration smoke**
**GIVEN** a valid character is created in a dev build, **WHEN** the first save succeeds, the app returns to title, and Continue loads the saved record, **THEN** the hydrated Player State matches the submitted seed for `character_name`, `starting_class_id`, appearance tokens, `PlayerZoneMembership`, `starting_equipment_template_id`, empty carried inventory, neutral faction baseline, and onboarding seed fields.
*Dev-build smoke | gameplay-programmer + qa-tester | T1-blocking*

**H-CC-22 - Creation input/navigation profiling**
**GIVEN** the first playable T1 build on Min-Spec Profile hardware, **WHEN** QA repeatedly enters valid and invalid names, tabs through controls, backs out, and submits a valid character, **THEN** input/navigation latency and visible UI hitches are recorded for review. This is advisory-at-T1 because no project-wide UI frame-budget threshold is locked yet; it promotes to T1-blocking if input/navigation regressions appear in the first playable build.
*Profiled playtest | ui-programmer + qa-tester | advisory-at-T1*

### Summary Table

| ID | Covers | Test Type | Owner | T1-Blocking |
|---|---|---|---|---|
| H-CC-01 | Rule 1 routed entry + Rule 17 T1 scope | Integration | ui-programmer, qa-tester | Yes |
| H-CC-02 | Menus overwrite boundary | Integration | ui-programmer, qa-tester | Yes |
| H-CC-03 | Rule 3 canonicalization | Unit | gameplay-programmer | Yes |
| H-CC-04 | Rule 3 length bounds | Unit | gameplay-programmer | Yes |
| H-CC-05 | Rule 3 allowed characters + Rule 14 local failure | Unit | gameplay-programmer | Yes |
| H-CC-06 | Rule 3 alphanumeric requirement | Unit | gameplay-programmer | Yes |
| H-CC-07 | Rule 3 no silent repair | Unit | gameplay-programmer | Yes |
| H-CC-08 | Rule 14 invalid-name UI block | Integration | ui-programmer, qa-tester | Yes |
| H-CC-09 | Rule 8 required schema fields | Integration | gameplay-programmer, qa-tester | Yes |
| H-CC-10 | Rule 4 + Rule 8 forbidden schema fields | Unit | gameplay-programmer | Yes |
| H-CC-11 | Rule 5 Cleric-only validation | Unit | gameplay-programmer | Yes |
| H-CC-12 | Rule 5 no future-class UI promise | Integration | ui-programmer, qa-tester | Yes |
| H-CC-13 | Rules 6-7 appearance / resident constraints | Editor-validation | technical-artist, qa-tester | Yes |
| H-CC-14 | Rule 9 start anchor validity | Editor-validation | engine-programmer, qa-tester | Yes |
| H-CC-15 | Rules 10-11 seed reference validity | Editor-validation | gameplay-programmer, qa-tester | Yes |
| H-CC-16 | Rule 13 validation-before-save | Unit | gameplay-programmer | Yes |
| H-CC-17 | Rule 15 valid Create lockout + Menus handoff | Integration | ui-programmer, qa-tester | Yes |
| H-CC-18 | Rule 16 Save/Load ownership + first-save failure | Integration | gameplay-programmer, qa-tester | Yes |
| H-CC-19 | Rule 15 first save + zone activation gate | Integration | gameplay-programmer, qa-tester | Yes |
| H-CC-20 | UI Requirements keyboard path | Integration | ui-programmer, qa-tester | Yes |
| H-CC-21 | End-to-end first-save hydration smoke | Dev-build smoke | gameplay-programmer, qa-tester | Yes |
| H-CC-22 | Input/navigation profiling | Profiled playtest | ui-programmer, qa-tester | advisory-at-T1 |

**Total: 22 criteria. 21 T1-blocking, 1 advisory-at-T1 (H-CC-22 - promotes to T1-blocking if input/navigation regressions appear in the first playable build).**

## Non-Goals

Character Creation does not own save slots, character lists, account identity, overwrite mechanics, class balance, future classes, inventory rules, faction reputation rules, Sister Elara's introduction, portraits, cosmetic customization, biography/origin/backstory, first-save failure UI, zone activation recovery, networking, live LLM behavior, or any T2+ multi-character scope.

It records only the approved T1 seed data: canonical name, `Cleric`, pre-faction appearance tokens, fixed start-anchor membership, equipment template id, empty carried inventory, neutral faction baseline, and onboarding eligibility fields.

## Open Questions

| Question | Owner | Deadline | Status |
|---|---|---|---|
| Where is opaque `local_character_id` generation specified? It must be immutable, non-player-authored, and stable after first save. | `gameplay-programmer` + `engine-programmer` | Before T1 Character Creation implementation | Open |
| Do appearance tokens stay split into `appearance_profile_id`, `palette_id`, and `class_visual_baseline_id`, or collapse to `appearance_profile_id` only if the visual pipeline does not need separate ids? | `technical-artist` + `gameplay-programmer` | Before T1 Character Creation implementation | Open |
| What authored-data file owns `ClericStartingEquipment_T1` until Inventory & Item Economy is designed? Character Creation can reference the id, but Inventory must later own item materialization and validation. | `game-designer` + `economy-designer` | Before first playable T1 creation flow | Open |
| What authored-data file owns `NeutralAllFactions` until Faction Reputation is designed? Character Creation can require complete neutral coverage for currently authored factions, but Faction Reputation owns numeric values and labels. | `systems-designer` + `game-designer` | Before first playable T1 creation flow | Open |
| What authored-data file owns `CityHub_InnRoom_StartAnchor`, and how is anchor validity checked before first save? World Structure owns the anchor and zone membership contract; Character Creation needs a validation hook. | `engine-programmer` + `gameplay-programmer` | Before first playable T1 creation flow | Open |
| Is `creation_schema_version` nested inside Player State, or parallel to the save-format version stamp in Save/Load's Session Metadata category? This affects Save/Load forward-only migration scope and whether the Character Creation schema version requires a Save/Load Rule 10 amendment. | `gameplay-programmer` + `engine-programmer` | Before T1 Character Creation implementation | Open |

