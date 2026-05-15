# Asset Spec — prop_maj_vc_court_marked_relic_01

**Asset id:** `prop_maj_vc_court_marked_relic_01`
**Type:** Prop (Major; M3 objective item — single instance)
**Tier:** —
**Faction:** Vampire Court (marked artifact, faction-token category per Inventory GDD)
**Maps to runtime fixture:** `CourtMarkedRelic_T1` (per `design/quick/quick-design-m3-objective-npc-loot.md`; the M3 objective item the player recovers + returns to the named NPC)
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director
**Source bible refs:** S4.5 (Vampire Court palette), S6.1 (Court hardware: silver-then-tarnished), S4.8 (FORBIDDEN: glowing loot, rarity colors, gold as reward signaling), S6.2 (cause test for surface authoring)

## Purpose

The single most ceremonially-significant object in M3 — the player picks it up at the cemetery relic point, returns it to the named NPC at the camp, and triggers the objective complete state. **It is FactionToken category per `design/gdd/inventory-item-economy.md`**: possession-only in T1, no reputation mutation, no progression token, returned to the NPC for objective hand-in.

**Critical bible-compliance load-bearing test:** Per S4.8 explicit FORBIDS list — "Rarity colors on any object. No golden glow, no purple shimmer, no tier color system." The relic must communicate "this is the objective item" through **specificity of design** (it looks like something a specific Court ritualist would have owned), NOT through visual prominence (glow, particle effect, gold material). If the player can tell this is the objective item only because it's glowing, the asset has failed.

## Visual Spec

- **Object type:** A Court-aligned ritual or ceremonial implement of specific origin. Working candidate (AD ratification required): **a small hand-held reliquary or chalice fragment**, ~15-20cm in its longest dimension. Specific to a known individual — the design implies "this belonged to someone who held a Court position 100-150 years ago," not "a generic relic of the Court."
- **Material composition:**
  - Primary: dark-stained wood OR aged silver (Court hardware vocabulary), depending on AD direction
  - Secondary: small inset elements (single small stone in slate-violet wax-seal range, or a tiny stretched-skin inscription panel)
  - Inscription: a name, a date, or a brief Court chancery text fragment using the asset #15 handwriting hand
- **Wear language:**
  - 100-150 year wear consistent with active use during the original owner's tenure, then **archive condition** since their departure (museum-quality preservation per Court hygiene)
  - Specific grip-polish patterns on handles or handle-adjacent surfaces (cause test compliance — someone held this here, frequently)
  - Surface chemistry has darkened slightly with archive age
- **Palette:**
  - Iron Seam / Pewter Rain neutral band for the primary material
  - Slate-violet accent at a single small inset only (sub-saturation)
  - **NO gold, NO bright silver, NO red, NO green** (S4.8 forbids each)
- **Forbidden specifics (LOAD-BEARING):**
  - **NO emissive material** — not even subtle. The relic does not glow.
  - **NO particle effect on pickup** — no shimmer, no soul-light, no dust motes rising from it. Per S4.8 and S1 Principle 1.
  - **NO floating** — when the player approaches, the relic is sitting on the cemetery surface in physical contact, not suspended.
  - **NO rarity color halo, outline glow, or interact-prompt visual** beyond the project's standard interact affordance (which should itself be restrained per S7.11).
  - NO obvious "quest item" framing that violates the bible's "no markers" principle.

## How the player identifies this as the objective item

Through the design grammar, NOT visual prominence:

1. **Placement:** the relic sits at a deliberate location (the cemetery's relic point per the M3 quick-design). The player has been told by the named NPC that it is at this place.
2. **Specificity:** unlike other props in the cemetery (gravestones, fallen leaves), this object is a specific Court ritual item. Its faction-vocabulary visual register is recognizable.
3. **Context:** the named NPC's framing of the objective ("the marked relic at the cemetery") gives the player the verbal handle that disambiguates this object from background.

**No UI marker. No floating "Q" prompt. No outline highlight.** The bible's "world is not your story" rule applies — the player learns to read this object through attention, not through visual labeling.

## Technical Spec — Per S8.3 Props — Major

| Channel | Resolution | Notes |
|---------|-----------|-------|
| Albedo | 512² | PNG 8-bit; individual UV |
| Normal | 256² | PNG 8-bit |
| Roughness | 256² | PNG 8-bit |
| Metallic | 128² | PNG 8-bit (silver hardware areas only) |

- **Mesh:** FBX, ~2,000 tri (Major-prop budget). LOD0 + LOD1 (50%).
- **Naming:** Per S8.2 — `prop_maj_vc_court_marked_relic_01.fbx` + texture suite.

## AI Generation Prompt

> Small hand-held ceremonial relic from the Vampire Court, 100-150 years old. ~15-20cm in longest dimension. Aged dark-stained wood OR tarnished silver primary material (NOT bright silver, NOT gold). Single small slate-violet inset (sub-saturation, NOT bright purple). Inscription panel with chancery handwriting text fragment in iron-gall brown-black ink. Grip-polish wear at handle-adjacent surfaces (cause-test compliant). Archive-condition preservation — Court hygiene means clean and intact, but the wear from original active use is preserved. NO emission, NO glow, NO shimmer, NO floating, NO outline, NO rarity-color halo, NO particles. Concept art on neutral background, three-quarter view. Pre-Raphaelite restraint. Reference: Renaissance-period Italian chancery seal box or small reliquary, museum preservation, not theatrical "quest item."

## Production Notes

- **AD ratification on the specific object choice.** The "hand-held reliquary or chalice fragment" working candidate is a placeholder; AD reviews the first-pass concept and approves the final form. **Whatever object is chosen must be specific enough that 'this belonged to a particular Court ritualist' reads** from the design alone.
- **Pickup affordance discipline:** per the bible's anti-quest-marker stance, the relic should NOT have a glow/outline/highlight that signals "interactable." The interaction affordance is environmental: the player approaches the cemetery's relic point, sees the object, and the standard interaction prompt (subtle, per the project's UI design) appears. This is also why the spec specifically prohibits emission/particle/outline — those would break the design grammar.
- **Co-asset dependency:** the inscription panel uses asset #15's chancery handwriting font. If the inscription is rendered as baked-into-texture, it can use a static text render in the chancery hand at authoring time.
- **Validation:** show the asset in-engine at the cemetery relic point under S2 State 1 lighting. Confirm the player can identify the object as "Court-faction relic" through silhouette + material at 5m camera distance, without any glow/highlight. Then confirm at 30m the object is **invisible** (faction-objective items are not visible from distance — players walk to them).

## Source citations

- `design/art/art-bible.md:615-625` (S4.8 FORBIDDEN list — particularly: rarity colors, glowing loot, gold as reward signaling)
- `design/art/art-bible.md:71-72` (S1 forbids "Glowing loot and rarity colors — the immediate-readability shortcut every RPG uses")
- `design/art/art-bible.md:921` (S6.1 Court hardware vocabulary)
- `design/gdd/inventory-item-economy.md:66-90` (FactionToken category; T1 possession-only)
- `design/quick/quick-design-m3-objective-npc-loot.md` Loot Table section (CourtMarkedRelic_T1)
- `design/art/art-bible-t1-scope.md` Characters / Layer 2 sections
