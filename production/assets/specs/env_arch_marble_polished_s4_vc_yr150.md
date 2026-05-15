# Asset Spec — env_arch_marble_polished_s4_vc_yr150

**Asset id:** `env_arch_marble_polished_s4_vc_yr150`
**Type:** Architecture tile (tileable, but **Unique-tier resolution per S8.3** — see Production Notes)
**Tier:** Unique (capped at 3-4 instances per Addressable streaming group; see S8.9)
**Faction:** Vampire Court (signature material)
**Stratum:** 4 (Factional Consolidation, ~150 years; per S6.1)
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director
**Source bible refs:** `design/art/art-bible.md` S4.5 (Vampire Court palette: black marble), S6.1 (Court modification phase: "black marble dado panels applied"), S6.2 (Court interior floor — polished marble), S8.3 (Architecture — Unique Surface row)

## Purpose

The **single most identifying Court surface** in the district. Per S6.1 Court-dominant building characterization: "Court modification phase (~180 years ago) — **black marble dado panels applied**, silver-then-tarnished hardware replacement." A player at 30m identifies a Court-controlled building partly because they can see this material on the lower wall surface or floor. This is the visual fingerprint of Court occupation; without it, the district reads as undifferentiated stone.

**Budget caveat:** Per S8.3, Unique-tier surfaces are capped at 3-4 per Addressable streaming group. The cemetery district plus the camp's named NPC interaction point should use this material at no more than 3-4 placements. Other Court surfaces use asset #1 (`stone_ashlar`).

## Visual Spec

- **Material:** Black marble — natural geological variant, NOT pure black. Surface shows subtle veining (gray-violet/slate-violet) consistent with Court accent vocabulary.
- **Polish:** **Polished annually for 150 years** per S6.1 ("marble polished annually, no vegetation encroachment on lower register"). Surface specularity is the highest of any Court-zone material.
- **Wear language:**
  - Foot-traffic polish at center of each panel where Court formal procession passes
  - Edge wear visible at panel corners where adjacent stone has shifted slightly over 150 years
  - **Surface chemistry darkening only** — per S6.1, Court "decay visible only in surface chemistry." No spalling, no cracks, no biological accumulation
  - Silver fittings (where panels are bracketed) are **tarnished** — never bright; never pure black
- **Palette skew:** Iron-Seam-anchored shadow within the marble (~`#3D3A38` shadow tone) + slate-violet vein (~`#7A6B85` low-saturation) + Bone-Pale highlight at polish reflections. Maintain Court's "warmth inversion" rule per S4.7 — this surface reads cold despite its high specularity.
- **Forbidden:** pure black `#000000` anywhere (S4.8); cracks, spalling, biological growth (Court hygiene); aged silver beyond the tarnish range (no green oxide); polishing inconsistency (Court maintains uniformly).

## Technical Spec

| Channel | Resolution | Notes |
|---------|-----------|-------|
| Albedo | 2048² | PNG 8-bit; **non-tileable per S8.3** Unique tier; authored at fixed panel-set scale |
| Normal | 2048² | PNG 8-bit; subtle veining + polish-direction texture |
| Roughness | 1024² | PNG 8-bit; very low (polished) at centers; slightly higher at edges |
| Metallic | 512² | PNG 8-bit; metallic channel for specular response — marble is dielectric, but the Court's polish creates near-metallic specular at glancing angles. **NOT a metal**; metallic value stays low (~0.1) |

- **Mipmap bias:** Inherits the `-0.5` starting value (UNVERIFIED, F-08).
- **URP material:** Standard PBR. SRP Batcher compatible. **Specular response carries the asset** — Court "geometric precision of doorway surrounds" is sold by clean specular highlights, not by albedo detail.
- **Naming:** Per S8.2 — `env_arch_uniq_marble_dado_vc_panel_01_[map-type].png`. **Note the `uniq` insertion** per S8.2 (`env_arch_uniq_[location-slug]_[map-type]` for non-tileable).
- **Memory budget impact:** 2048² × 4 channels ≈ 5MB per instance uncompressed; with BC7 ≈ 2.6MB. At 3 instances, ~8MB resident — within the 350MB streaming group target (S8.9), but a meaningful slice.

## AI Generation Prompt

> Polished black marble dado panel surface for Vampire Court gothic architecture, 150 years old, polished annually. PBR material, non-tileable, single panel render. Subtle slate-violet veining in natural geological pattern. High specular polish at panel centers; slightly higher roughness at edges where adjacent stone has shifted. Surface chemistry darkening with age but NO cracks, NO spalling, NO biological growth. Silver-then-tarnished bracket fittings at panel corners (tarnished gray-brown, never green-oxide, never bright). Overcast 6000K ambient lighting with one practical warm lantern at 2400K visible in the specular highlight. Reference: 19th-century Italian palazzo interior — preserved-in-use, not museum-cleaned.

## Production Notes

- **Placement discipline:** Cap at 3-4 panel placements per zone (S8.3 Unique-tier rule). Recommended placements for Mournwall Cemetery District: (1) the named NPC's interaction-point alcove wall, (2) the cemetery gate's lower panel, (3) possibly the vendor's display backdrop.
- **Maintenance read:** The Court's "polished annually" rule means the surface must read clean. **If the dirt/grime layer reads too heavily, the asset has failed** — the Court does not let surfaces dim.
- **Co-asset note:** the "silver-then-tarnished" bracket hardware is mentioned but is a separate prop (atlas-packed under minor-prop budget). This spec covers the **panel** only.
- **Specular validation:** test under S2 State 9 (City Hub Bell-Tolled Night) where Court formal lighting comes up — the panel should hold its specular character without going from "polished" to "wet-looking" or "plastic."

## Source citations

- `design/art/art-bible.md:921` (Vampire Court "black marble dado panels applied" — Court architectural expression)
- `design/art/art-bible.md:881-883` (Court-dominant building maintenance phase, ongoing polishing)
- `design/art/art-bible.md:1542-1559` (S8.3 Architecture — Unique Surface row, budget cap)
- `design/art/art-bible.md:619` (S4.8 forbidden: true black `#000000`)
- `design/art/art-bible.md:626-627` (S4.7 Court warmth-inversion rule)
- `design/art/art-bible-t1-scope.md` Environment section
