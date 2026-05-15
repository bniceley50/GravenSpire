# Asset Spec — char_pc_neu_cleric_t1

**Asset id:** `char_pc_neu_cleric_t1`
**Type:** Player Character (body + onboarding garment; no face spec)
**Tier:** T1 onboarding (Rep Tier 0 — Unknown; Rep Tier 1 — Recognized at most for M3)
**Faction:** Neutral (pre-faction; T1 player starts before Court reputation accumulation)
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director (with open AD ratification pending on first-pass concept)
**Source bible refs:** S5.1 (Player Character Visual Archetype, Cleric class baseline, Onboarding Player constraints, Rep Tier table), S5.2 (Player vs Named NPC distinction), S5.4 (Player Character body budgets — S8.3 Player character — body row), S8.3 (resolution table)

## Purpose

The Cleric is the only T1 player archetype per `design/gdd/game-concept.md:340`. This spec covers the body and the **onboarding** garment vocabulary — the Cleric as a new resident in Gravenspire who has not yet accumulated Vampire Court reputation. Per S5.1 "Onboarding Player": "no faction-primary color present anywhere at >5% surface area; all materials in the Bone Pale / Render Umber / Wick Gray neutral band; no material quality token associated with any faction vocabulary."

The Cleric's faction-accumulation visual progression (Rep Tier 2-4) is a **T2 spec** per the open art decision in asset-manifest.md.

**Face spec deliberately excluded:** the player camera does not see the player's face in any moment relevant to T1 (third-person or first-person perspective; no mirror/dialogue camera framing the player's own face). Per bound condition #3(b), named-NPC face SSS is BLOCKED. Even if the player face needed to be rendered, the player character body uses the **Player character — body** row of S8.3, NOT the named-NPC face row.

## Visual Spec

- **Class silhouette:** Per S5.1 Cleric baseline — "layered mid-length vertical emphasis." A vertical garment composition (robe-like, not coat-like; not Warrior's padded-layer horizontal shoulder mass; not Enchanter's weighted-hood forward-lean). The Cleric carries a focus item (small staff or hand-held implement); this is the secondary carrier per S5.1 "carried objects accumulate material history."
- **Onboarding garment (Rep Tier 0):** undyed linen and rough wool. Materials in Bone Pale / Render Umber / Wick Gray ONLY. NO Court slate-violet anywhere. NO material quality tokens (polished, refined, embroidered) on any garment piece. The Cleric has not yet earned faction vocabulary; they are wearing what was available before knowing which faction to align with.
- **Garment construction:**
  - Mid-length robe or tunic over a base layer
  - A simple belt with practical fastening (no Court precision; no ornate buckle)
  - Functional footwear — sandals or basic boots in unstained leather
  - **No insignia of any kind.** No medallion, no inherited family crest, no faction marker. The bible's S5.1 rule applies — protagonist framing through visual prominence is forbidden.
- **Carried focus item:** a small wooden staff or focus implement, hand-held. ~80cm long. **Modest, not heroic.** No emission, no glow, no rune-marked surface. The bible's S4.8 forbids "Gold as reward signaling" and emissive material; the focus item is functional wood/bone with grip wear consistent with daily use.
- **Wear:**
  - Slight discoloration at garment hems where the Cleric has been walking through specific environments (per S5.1: "Garments show wear patterns corresponding to how the character has been spending their time")
  - For T1 starting state: minimal wear — the Cleric is *new* to Gravenspire; accumulation is at the very early stage
  - Hands and the focus item show the only meaningful wear (grip-polished thumb-area on the staff)
- **Forbidden specifics:** high-contrast hero silhouette (S1 explicit forbid), Court slate-violet at >5% surface area, gold/amber on any rendered surface (S4.8: gold is forbidden as reward signaling and reads as "valuable" wrongly), faction-primary color on body or garment, glowing focus item, emissive runes.

## Technical Spec — Per S5.4 / S8.3 Player character body

| Attribute | Value |
|-----------|-------|
| Polygon budget | 14,000-18,000 tri (player character — body row, same poly target as named NPC body) |
| Albedo (body) | 1024² PNG 8-bit (S8.3 player body row) |
| Normal (body) | 512² PNG 8-bit |
| Roughness (body) | 512² PNG 8-bit |
| Metallic (body) | 256² PNG 8-bit (minimal — focus item hardware only) |
| Albedo (garment) | 512² (treated as separate material for swapping at progression tiers) |
| Normal (garment) | 256² |
| Roughness (garment) | 256² |
| Face textures | **NOT SPEC'd in T1.** Player face is camera-occluded; named-NPC face SSS pass is BLOCKED. If T2+ design requires player face rendering, that's a new spec at named-NPC face budget. |
| Material count | 2-3 materials (body skin + onboarding garment + focus item) |
| Shader passes | 1: Standard PBR for body skin (NO SSS — SSS is BLOCKED + player face isn't rendered anyway) |
| Rigging | Standard biped, full body rig. Jaw + minimal facial rig (the bible's S5.4 references "jaw + minimal" for player body but full facial rig is a named-NPC budget item) |
| LODs | 4 LODs per the player character standard: LOD0 <5m, LOD1 (75%) 5-10m, LOD2 (50%) 10-25m, LOD3 (25%) 25-50m, cull beyond 50m |
| Naming | Per S8.2: `char_pc_neu_cleric_lod0.fbx` (LOD0), `_lod1.fbx`, etc. Textures: `char_pc_neu_cleric_[map].png` |

## AI Generation Prompt

> Cleric player character, Gravenspire game, new arrival in the city, no faction allegiance yet. Mid-length layered robe or tunic with vertical emphasis. Undyed linen and rough wool in desaturated band — Bone Pale, Render Umber, Wick Gray. NO faction colors, NO Court slate-violet, NO gold, NO emission, NO insignia, NO ornate buckle. Functional belt, simple footwear (sandals or basic boots in unstained leather). Carries a small wooden staff or focus implement (~80cm), modest, no glow, no runes. The character reads as someone wearing what they could afford before knowing which faction they'd be spending time with — NOT as a protagonist, NOT as a heroic figure. Slight discoloration at garment hems (minimal — character is new to the city). Pre-Raphaelite restraint. Reference: Italian medieval cleric or itinerant scholar, pre-modern, weighty fabric.

## Production Notes

- **AD ratification required before PROD.** Per Open art decision #1 (asset-manifest.md): "Cleric class baseline silhouette specifics ... need AD ratification on the first-pass concept before spec #12 moves SPEC → PROD." Generate first-pass concept; AD reviews; spec gets ratified or revised.
- **Body + onboarding garment authored together** as one mesh source (LOD0 includes both). Later T2 specs swap the garment slot for accumulated faction-rep materials; the body is shared.
- **The focus item is a separate prop** (`prop_maj_neu_cleric_focus_01`, future spec) carried in the rig's hand bone. Not authored in the body mesh.
- **Camera framing test:** during scene assembly, validate that the player's face is never the framed subject in T1. If the game requires a face-camera moment (dialogue with self in mirror, character creation portrait), the spec needs a face budget pass — which would be BLOCKED by the SSS POC.
- **Mixamo path (for fast first iteration):** Mixamo offers humanoid characters with Cleric-adjacent silhouettes (monk, scholar, hooded figure). Use as a starting mesh; retexture per this spec's wear/palette rules. Avoids needing to author the biped rig from scratch. Validation: confirm the retextured Mixamo character passes the bible compliance tests (no insignia, no hero silhouette, etc.).

## Source citations

- `design/art/art-bible.md:640-648` (S5.1 Onboarding Player constraints)
- `design/art/art-bible.md:640` (S5.1 Cleric baseline: "layered mid-length vertical emphasis")
- `design/art/art-bible.md:651-657` (S5.1 Rep Tier visual table — T1 starts at Tier 0-1)
- `design/art/art-bible.md:683-691` (S5.2 Player vs Named NPC — no marker system)
- `design/art/art-bible.md:1544-1546` (S8.3 Player character body + face rows)
- `design/gdd/game-concept.md:340` (T1 MVP: Cleric class)
- `design/art/art-bible-t1-scope.md` Characters section (Cleric T1 onboarding scope)
