# Asset Spec — prop_maj_neu_gravestone_set_01

**Asset id:** `prop_maj_neu_gravestone_set_01`
**Type:** Prop set (Major; multiple variants in one FBX)
**Tier:** —
**Faction:** Neutral (gravestones predate or transcend the Court's current control of the district; varied stratum)
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director
**Source bible refs:** `design/art/art-bible.md` S6.2 (PBR philosophy: cause test), S6.3 (texture & material), S8.3 (Props — Major row), S3.6 ("Skeletal / bone-motif decoration as generic 'undead' signaling" is FORBIDDEN per S1)

## Purpose

The cemetery's load-bearing prop set. Mournwall Cemetery District without gravestones isn't a cemetery. Without these, the district reads as "stone yard"; with them, it reads as "ancient burial ground with multiple periods of interment." Each variant carries a specific stratum + age tier so the cemetery's history (~400 years of interments under shifting factional control) reads through the prop layout, per S6.2 cause-test discipline.

**Critical bible compliance:** S3.6 forbids "Skeletal / bone-motif decoration as generic 'undead' signaling — skulls on the architecture, bones in the trim. This collapses factional distinction." Gravestones in Gravenspire's cemetery do NOT use skull motifs or bone iconography as decoration. They are markers of specific dead persons, carved in the iconographic vocabulary of the burying faction at the time of interment.

## Variant inventory (5-8 in one FBX)

| Variant | Stratum / Era | Iconography | Wear |
|---------|---------------|-------------|------|
| `headstone_s2_yr250_lancet` | Stratum 2 (Early City, gothic) | Carved pointed-arch frame; chiseled name no longer legible at distance | Heavy weathering; lichen at base only |
| `headstone_s3_yr150_court_marker` | Stratum 3, Court-era | Geometric Court precision; small Court marker (no skull) | Moderate weathering; Court-attendant maintenance evident |
| `headstone_s3_yr180_pre_court` | Stratum 3, Resistance-era (pre-Court control of cemetery) | Living Resistance iconography (sun, hand, wheat — pre-faction undead) | Heavier weathering; visible faction-transition |
| `tomb_marker_s2_yr300_recumbent` | Stratum 2 | Horizontal recumbent slab; carved figure in low relief | Significant erosion |
| `obelisk_s4_yr120_court_minor` | Stratum 4, Court | Small obelisk; Court-precision dressing | Light weathering; recently re-cut Court lettering at base |
| `funerary_urn_pedestal_s3` | Stratum 3 | Stone pedestal with urn (sealed; not bone iconography) | Surface chemistry darkening |
| `iron_fence_post_s4_yr80` | Stratum 4 | Wrought-iron fence post (Court hardware vocabulary) | Tarnished hardware per S6.1 Court rule |

## Visual Spec

- **No skull motifs on any stone surface.** This is bible-compliance load-bearing per S3.6.
- **Each variant has its history visible** — per S6.2 "production discipline: every building must have a documented occupation history of 2-3 sentences specifying which factions held it and when, before modeling begins." This applies to gravestones at miniature scale: each variant should have a 1-sentence history note in production documentation.
- **Iconography is faction-vocabulary specific to its era**, not generic "undead" iconography. Court markers use the Court's geometric register; Resistance markers use organic-symbolic (sun, hand, wheat); Stratum 2 markers use early gothic ecclesiastical (without explicit skulls).
- **Wear language:**
  - Stratum 2 stones show 250+ years of weathering; details have softened
  - Stratum 3 Court stones are partially maintained — Court attendants re-cut faction-significant lettering on a periodic schedule
  - Stratum 3 pre-Court stones show MORE wear because they're NOT maintained (the Court doesn't clean up the Resistance era's stones)
  - Lichen is allowed at ground contact (S6.1 hierarchy: ground-contact biological is allowed; the Court's "no biological" rule applies to upright wall surfaces, not horizontal ground or pre-Court legacy markers)
- **Palette:** Same desaturated stone-gray dominant as the architecture. Slight warm shifts at Resistance-era markers (older mineralization, more umber). NO faction-color on any stone — the iconography reads through *carving style*, not color.

## Technical Spec

| Channel | Resolution | Notes |
|---------|-----------|-------|
| Albedo | 512² per variant | PNG 8-bit; individual UV per variant |
| Normal | 256² per variant | PNG 8-bit |
| Roughness | 256² per variant | PNG 8-bit |
| Metallic | 128² | Only for the `iron_fence_post` variant |

- **Mesh:** FBX containing all 5-8 variants as separate meshes. Each variant ~800-1500 tri. Total kit ~6-10k tri.
- **LODs:** 2 LODs per variant (50% reduction at LOD1, 25m+ cull).
- **Naming:** `prop_maj_neu_gravestone_set_01.fbx` (master kit), individual variant textures `prop_maj_neu_gravestone_set_01_[variant-slug]_alb.png`.
- **Atlas option:** alternatively, atlas the albedos into a single 1024² atlas — saves draw calls at the cost of slightly more memory per variant. **Recommended: atlas approach for M3 to minimize draw call count in the cemetery scene.**

## AI Generation Prompt

> Italian gothic cemetery gravestones, multi-era, 400 years of interments under shifting religious authority. Concept art for 5-8 variants. NO skulls, NO bones, NO generic "undead" iconography. Each marker uses the iconographic vocabulary of its era: Stratum-2 early gothic (carved pointed-arch frame, ecclesiastical), Stratum-3 Court-era (geometric precision, no skull), Stratum-3 Resistance-era (organic symbolic: sun, hand, wheat — pre-vampire-rule). Lichen at ground contact only. Weathering varies by era and by whether the marker is maintained by current Court attendants. Palette: desaturated stone-gray dominant with slight umber-warm at older Resistance-era stones. Reference: real Italian medieval cemetery (Camposanto Monumentale Pisa sensibility), not fantasy graveyard.

## Production Notes

- **Iconography is THE compliance check.** AI generation tools default to skulls + bones for "gothic cemetery." Reject any output that includes skull/bone motifs on the stone faces. Re-prompt explicitly: "no skulls, no bones — the iconography is from the burying faction's vocabulary at time of death."
- **Stratum/faction diversity is the cemetery's storytelling.** A cemetery with all Stratum-3 Court stones reads as "Court mausoleum." A cemetery with a mix reads as "burial ground that has been here since before the Court took the district." The mix is required.
- **Atlas approach:** if going with atlas, lay out variants so that visually-distinct elements (lancet frames, obelisk tops) don't conflict at atlas seams.
- **Co-asset:** the `iron_fence_post_s4_yr80` variant pairs with the bracket hardware on the marble dado spec. Same metallic + tarnish vocabulary.

## Source citations

- `design/art/art-bible.md:76` (S1 FORBIDS skull/bone decoration as generic undead signaling)
- `design/art/art-bible.md:927` (S6.1 per-faction architectural expression for Resistance — organic vocabulary)
- `design/art/art-bible.md:951` (S6.2 "cause test")
- `design/art/art-bible.md:889` (S6.1 production discipline: 2-3 sentences of history before modeling)
- `design/art/art-bible.md:962` (S8.3 Props — Major row, 512² albedo)
- `design/art/art-bible-t1-scope.md` Environment section
