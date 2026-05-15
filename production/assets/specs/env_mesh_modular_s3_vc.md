# Asset Spec — env_mesh_modular_s3_vc

**Asset id:** `env_mesh_modular_s3_vc`
**Type:** Mesh kit (modular architectural pieces)
**Tier:** Primary
**Faction:** Vampire Court
**Stratum:** 3 (Transition Period; same era as the ashlar surface tile #1)
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director
**Source bible refs:** `design/art/art-bible.md` S3.1-3.3 (shape language, pointed-arch structural logic), S6.1 (modular architecture kit + strata tagging), S6.1 cue 1 (threshold geometry contradiction)

## Purpose

The structural geometry that wears the `env_arch_stone_ashlar_s3_vc_yr200` and `env_arch_marble_polished_s4_vc_yr150` surfaces. The bible's S6.1 explicitly mandates "modular architecture kit requires strata tagging" so the Mournwall Cemetery District is buildable from these pieces + the surface tiles. Without this mesh set, the district cannot stand up; the bible's "every modular piece carries metadata indicating stratum and faction attribution" rule is fundamental to scene assembly.

## Mesh inventory (one FBX, one Blender source)

| Mesh | Purpose | Approx tri count |
|------|---------|------------------|
| `wall_straight_3m` | Standard wall segment, 3m wide × 4m tall × 0.4m thick | ~600 tri |
| `wall_corner_in` | Inside corner 90° | ~400 tri |
| `wall_corner_out` | Outside corner 90° | ~400 tri |
| `arch_pointed_3m` | Pointed gothic arch over 3m opening | ~900 tri |
| `doorframe_3m` | Door surround with **modified threshold** (S6.1 cue 1: pointed arch with flat lintel inserted at mid-height) | ~1200 tri |
| `window_court_lancet` | Lancet window with Court geometric precision (S6.1 Court: "largest window reveals; geometric precision that has not degraded in 150 years") | ~800 tri |
| `cornice_band_3m` | Horizontal cornice for vertical-accretion boundary between Stratum 1 base and Stratum 3 upper | ~500 tri |
| `floor_panel_3x3` | Floor / paving panel 3m × 3m × 0.2m | ~200 tri |

**Total kit:** ~5,000 tri across 8 mesh pieces. Each piece UV'd to the trim-sheet for `env_arch_stone_ashlar_s3_vc_yr200`; the doorframe + window pieces have a secondary UV for marble accents (`env_arch_marble_polished_s4_vc_yr150`).

## Visual Spec

- **Shape language:** Pointed-arch structural logic per S3.1. Compressed verticality. No round arches at this stratum (Stratum 1 is round-arch; this is Stratum 2-3 gothic dressed).
- **Threshold geometry contradiction (load-bearing):** the `doorframe_3m` shows the S6.1 cue 1 — pointed gothic arch with flat lintel inserted at mid-height. The hinge-pintle relocation evidence (filled hole + new hole) is on the trim sheet, not on the mesh.
- **Court geometric precision:** Per S6.1 Vampire Court row — "geometric precision of doorway surrounds that has not degraded in 150 years." The doorframe and window pieces must read CLEAN — no sag, no settle, no warp. The wear is in the surface (handled by the tile material), not in the geometry.
- **Wall thickness:** 0.4m visible at openings. Per S6.1 Pre-City Stratum 1 thickness is 0.9-1.2m; the Stratum 3 upper walls are thinner at 0.4m. Visible at any opening reveal.

## Technical Spec

- **Format:** FBX (Y-up, transforms applied, normals from mesh per S8.1).
- **UV:** Primary UV → ashlar trim sheet (1024² at 1:2m scale). Secondary UV → marble dado trim (for doorframe + window only).
- **LODs:** 2 LODs per piece. LOD0 at the tri counts listed; LOD1 at 50% (for 25m+ distance).
- **Naming:** Per S8.2 — `env_mesh_wall_s3_vc_modular.fbx`, `env_mesh_arch_s3_vc_modular.fbx`, `env_mesh_doorframe_s3_vc_modular.fbx`, etc.
- **Metadata (Unity)**: each prefab carries `Stratum` and `Faction` string tags (per S6.1 modular-kit rule). Suggested approach: custom ScriptableObject component on the prefab root.
- **Collider strategy:** simple box colliders matching mesh footprint; no per-vertex collision.

## AI Generation Prompt (for concept reference)

> Modular gothic architectural kit, Italian medieval city, 200 years old. Vampire Court maintained. Pointed-arch openings; dressed stone; geometric precision preserved. Wall pieces with 0.4m thickness. Doorframe with **a modified threshold** — pointed gothic arch above, flat lintel inserted at mid-height where someone added a shorter functional door 200 years later. Hinge-pintle evidence (filled hole + new hole) visible. Window: tall lancet style with deep reveal. No ornament without function. Reference: Italian gothic palazzo entry (Palazzo Vecchio sensibility — not Venetian baroque). Concept art for game asset; isolated pieces on neutral background.

## Production Notes

- **Build order recommendation:** start with `wall_straight_3m` + `doorframe_3m` — those are the workhorse pieces. Validate the trim-sheet UV mapping works cleanly on those before authoring the rest.
- **Threshold contradiction is the storytelling piece.** The `doorframe_3m` is where 2-3 sentences of building-history (per S6.1 production discipline) accumulate visually. Get this one right and the rest of the kit reads correctly.
- **Adjacency test:** all pieces must abut cleanly at any 90° rotation. The seam between adjacent walls must not require additional cleanup geometry.
- **Combined-strata test:** when placed above the `env_arch_stone_rough_s1_neu_yr400` base course tile (using `cornice_band_3m` as the visual seam), the vertical-accretion read should be unmistakable.

## Source citations

- `design/art/art-bible.md:271-273` (S3.1 shape language: "pointed-arch structural logic")
- `design/art/art-bible.md:869-870` (S6.1 cue 1: threshold geometry contradiction)
- `design/art/art-bible.md:921` (S6.1 Court: "largest window reveals; geometric precision...")
- `design/art/art-bible.md:930` (S6.1 production guidance: modular architecture kit + strata tagging)
- `design/art/art-bible.md:889` (S6.1 production discipline: 2-3 sentences of building history before modeling)
- `design/art/art-bible-t1-scope.md` Environment section
