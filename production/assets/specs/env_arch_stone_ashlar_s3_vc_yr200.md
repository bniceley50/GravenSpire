# Asset Spec — env_arch_stone_ashlar_s3_vc_yr200

**Asset id:** `env_arch_stone_ashlar_s3_vc_yr200`
**Type:** Architecture tile (tileable surface material set)
**Tier:** Primary (S6.2 / S8.3)
**Faction:** Vampire Court
**Stratum:** 3 (Transition Period, ~200 years old; per S6.1 chronological strata table)
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director
**Source bible refs:** `design/art/art-bible.md` S6.1 (faction stratum), S6.2 (PBR philosophy + 80/20 rule), S6.3 (age via wear directionality), S8.3 (resolution table), S4.5 (Vampire Court palette: slate-violet, black marble accents)

## Purpose

Primary tileable stone facade material for Vampire Court-aligned walls in the Mournwall Cemetery District. Carries the 80% of the district's surface area per the S6.2 80/20 rule. Players must read the district's faction allegiance and 200-year-history through this surface alone — if a viewer at 30m cannot identify "Vampire Court has held this district on top of older construction" from this tile, the asset has failed.

## Visual Spec

- **Stratum identity:** Stratum 3 (Transition Period). Dressed stone with chamfered ashlar courses; gothic-period stone-cutting precision.
- **Court maintenance overlay:** ~150 years of subsequent Court maintenance visible. Mortar repointed at clean grout lines. **No biological colonization** (Court zones have no organic process per S6.1).
- **Wear language (every patch needs a physical cause):**
  - Foot-traffic polish at center of each ashlar where Court attendants pass repeatedly (S6.1 cue 4)
  - Tool-marks chiseled-but-not-polished at high reaches and concealed corners
  - Mortar lines hold faint differential staining where rain has wet against stone or shadow has held longer
  - **Forbidden:** spalled corners, biological greening, mineral staining from organic source, fresh mortar
- **Palette skew:** Pewter-weighted gray-stone base (S2 State 1: desaturated stone gray dominant) with faint slate-violet (`~#7A6B85` low-saturation) mineral tint consistent with Court accent vocabulary. Maintain S4.6 ≥3:1 luminance contrast against Bone Pale `#D4CCBC` for tritanopia.
- **Forbidden specifics:** spalled corners, biological greening, organic-source mineral staining, fresh mortar at any stratum below 5, faction insignia carved/applied, full-saturation Court violet.

## Technical Spec

| Channel | Resolution | Notes |
|---------|-----------|-------|
| Albedo | 1024² | PNG 8-bit; tileable seamlessly at 1:2m world scale |
| Normal | 1024² | PNG 8-bit; ashlar-course relief + tool-mark microdetail |
| Roughness | 512² | PNG 8-bit; smooth at high-traffic centers; rough at low-traffic |
| Metallic | 256² | PNG 8-bit; faint specular on polished centers only |

- **Mipmap bias:** `-0.5` starting value per S6.2 / S8.5 (UNVERIFIED at AD sign-off; validate against locked hardware target per F-09). Fall back to 2048² base if 30m faction-silhouette legibility (S3.1) fails.
- **URP material:** Standard PBR. SRP Batcher compatible. No emissive. No SSS (environment unaffected by SSS BLOCKING bound condition).
- **Naming:** Per S8.2 — `env_arch_stone_ashlar_s3_vc_yr200_[map-type].png` (e.g., `_alb.png`, `_nrm.png`, `_rgh.png`, `_mtl.png`).

## AI Generation Prompt

> Vampire Court gothic ashlar stone wall texture, 200 years old, transition-period construction with subsequent Court maintenance. Seamless tileable PBR material. Chamfered dressed stone blocks ~30cm tall, neat mortar lines with faint differential staining, faint desaturated slate-violet mineral tint in the stone. Foot-traffic polish at stone centers; tool-marks visible at low-traffic surfaces. No biological growth, no moss, no spalling, no faction insignia, no fresh mortar. Overcast 6000K diffuse lighting for albedo capture. Weight and age via surface history, not "old and damaged" overlay. Pre-Raphaelite restraint: specific, weighty, unhurried. Reference: Italian medieval city palazzo facade after 150 years of careful tenancy.

## Production Notes

- **First-pass path:** generate albedo + normal via Stable Diffusion + ControlNet (tileability) or Substance Sampler. Refine in Substance Painter to enforce the cause-test on each wear element.
- **Validation:** load tile into Unity at 1:2m scale; render at 30m distance under S2 State 1 overcast lighting; confirm faction silhouette legibility passes the S3.1 80px test.
- **Watch items:** mipmap bias `-0.5` may be insufficient at 30m on 1024² (F-08 UNVERIFIED). If 30m test fails, escalate to 2048² with memory budget impact tracked at S8.9 streaming-group analysis.
- **Dependencies:** F-09 hardware target lock for budget validation.

## Source citations

- `design/art/art-bible.md:920-924` (Stratum 3 Court characteristics, per-faction architectural expression table)
- `design/art/art-bible.md:881-883` (Court-dominant building maintenance phase)
- `design/art/art-bible.md:867-878` (age-legibility cues 1-5)
- `design/art/art-bible.md:1542-1559` (S8.3 consolidated resolution table)
- `design/art/art-bible-t1-scope.md` Environment section (T1 M3 environment scope)
- `design/gdd/game-concept.md:340` (T1 MVP faction = Vampire Court)
