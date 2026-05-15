# Asset Spec — env_arch_stone_rough_s1_neu_yr400

**Asset id:** `env_arch_stone_rough_s1_neu_yr400`
**Type:** Architecture tile (tileable surface material set)
**Tier:** Primary (S6.2 / S8.3)
**Faction:** Neutral (pre-city; Stratum 1 predates faction control)
**Stratum:** 1 (Pre-City Foundation, ~400+ years; per S6.1 chronological strata table)
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director
**Source bible refs:** `design/art/art-bible.md` S6.1 (Stratum 1 character, vertical accretion principle), S6.2 (PBR philosophy), S6.3 (texture & material)

## Purpose

Pre-city base-course material visible at the bottom register of Court-dominant walls (per S6.1: "buildings grew up, not out. Base course is rough-cut massive stone"). Provides the **stratigraphy** the bible names as a primary first-read cue — without a visible Stratum-1 base under the Stratum-3 Court ashlar (asset #1), the district's age reads as flat. **Co-dependent with asset #1** — they form the vertical strata story together.

## Visual Spec

- **Stratum identity:** Stratum 1. Massive rough-cut stone (sandstone or limestone, not yet dressed). Crude lime mortar. Round arches if visible. Walls 0.9-1.2m thick (this tile reads the surface of that thickness).
- **Faction overlay:** **None.** Pre-city is pre-faction. The Pale King Cult theology associates with this stratum (S6.1) but does not modify the surface — the Cult's discipline is anti-maintenance, not stratum-revision.
- **Wear language:**
  - Coarse chisel marks at original cutting (never dressed)
  - Mineral accretion in joints (lime-mortar age, calcium carbonate deposition)
  - Slight biological allowance at base where ground contact occurs — **only at the very bottom courses**, not the upper register (the Court zone above keeps biological process absent)
  - Differential surface darkening from 400 years of weathering
- **Palette skew:** Heavier umber-brown than asset #1 (older stone, slightly different mineral content). Stays within S2 State 1's stone-gray-dominant range; oxide-brown tint per "muted umber/oxide brown in trim" guidance.
- **Forbidden:** dressed stone (that's Stratum 2+), regular ashlar courses (Stratum 2+), faction tint (Stratum 1 is pre-faction), polished surfaces.

## Technical Spec

| Channel | Resolution | Notes |
|---------|-----------|-------|
| Albedo | 1024² | PNG 8-bit; tileable at 1:2m scale |
| Normal | 1024² | PNG 8-bit; rough-cut block relief, deeper than #1 |
| Roughness | 512² | PNG 8-bit; uniformly rough (no polished centers) |
| Metallic | — | Not used (no metallic content in pre-city stone) |

- **Mipmap bias:** `-0.5` starting value per S6.2 / S8.5 (UNVERIFIED, hardware-dependent).
- **URP material:** Standard PBR. SRP Batcher compatible.
- **Tiling note:** verticality should read at 1:2m scale; the base-course strata is typically 0.5-1m tall in world units, so the tile may be used at a stretched aspect ratio for vertical accretion.

## AI Generation Prompt

> Pre-historic rough-cut massive stone wall texture, 400+ years old, no faction maintenance. Seamless tileable PBR. Massive uncut/crude blocks of sandstone-limestone with crude lime mortar. Visible coarse chisel marks from original quarrying, never refined. Mineral accretion in joints (calcium deposits, age-darkened). Differential surface darkening from centuries of weathering. Heavier umber-brown tone than dressed gothic stone. No dressed faces, no ashlar courses, no faction insignia, no biological greening on the upper surface (some allowed at the very base). Overcast 6000K diffuse lighting. Reference: Italian Romanesque crypt foundation, undisturbed since construction.

## Production Notes

- **Use in scene:** appears as a 0.5-1m tall horizontal strip at the base of Vampire Court buildings and the cemetery wall. Visible vertical accretion above it should use asset #1 (`env_arch_stone_ashlar_s3_vc_yr200`).
- **Combined-strata test:** when the two tiles abut vertically, the seam should read as a "vertical accretion in exterior walls" cue per S6.1 cue 2. Test by rendering a full-height wall with the two tiles stacked and confirm the seam reads as visible geological layering, not as a texture transition.
- **AI generation refinement:** Stable Diffusion tends to produce "fantasy ruined stone" by default; explicit prompt language about *crude original construction* and *no decay* is required to avoid the spalled-ruin failure mode.
- **Dependencies:** F-09 hardware target.

## Source citations

- `design/art/art-bible.md:907-915` (Stratum 1 — Pre-City Foundation row)
- `design/art/art-bible.md:865-871` (vertical accretion principle; S6.1 cue 2)
- `design/art/art-bible.md:1542-1559` (S8.3 resolution table — Architecture Primary row)
- `design/art/art-bible-t1-scope.md` Environment section
