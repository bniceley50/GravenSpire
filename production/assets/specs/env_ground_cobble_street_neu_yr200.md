# Asset Spec — env_ground_cobble_street_neu_yr200

**Asset id:** `env_ground_cobble_street_neu_yr200`
**Type:** Ground tile (tileable surface material set)
**Tier:** Primary (S6.2 / S8.3)
**Faction:** Neutral (street infrastructure precedes current factional control; wear pattern encodes social use)
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director
**Source bible refs:** `design/art/art-bible.md` S2 State 1 (THE ONE visual element: "wear patterns on stone — high-traffic cobblestone polished smooth at the center of each stone"), S6.2 (PBR philosophy + texture budgets), S6.3 (wear directionality as social history), S8.3 (Ground — Street/courtyard row), S4.6 (color-accessibility constraints)

## Purpose

**Carries the "wear patterns on stone" cue that the bible names as THE ONE visual element of S2 State 1 (Exploration).** Per the bible: "high-traffic cobblestone polished smooth at the center of each stone by centuries of foot traffic; low-traffic alleys show moss encroachment at the grout lines. The city's social geography is written in surfaces, not signage." Without this surface telling its 200-year wear story, the district's age reads as flat and the player loses the bible's core "weight and age" register at ground level — which is where they spend most camera attention.

## Visual Spec

- **Cobble layout:** Irregular hand-cut cobbles, 12-20cm per stone. NOT uniform machine-cut — Stratum 2-3 era handwork.
- **Wear pattern (load-bearing):** Center of each stone polished smooth by foot traffic; outer 1-2cm of each stone retains original rough cutting. The wear gradient per stone IS the readable history. **Vertex-painted overlay** authored separately handles street-vs-alley variation (S6.2: "Wear variation via vertex-painted overlay" for Primary ground surface).
- **Grout lines:**
  - **Court-adjacent / cemetery main path:** clean grout lines; no biological colonization (S6.1 Court rule)
  - **Side alleys (if rendered):** moss encroachment at grout (the bible's explicit "low-traffic alleys show moss encroachment" example)
- **Palette skew:** Pewter-weighted gray dominant. Faint umber-brown patina at low spots from rain pooling history. Stays well within S2 State 1's stone-gray range.
- **Forbidden:** uniform stone size (modern paving), perfectly straight courses (Stratum 5+ vibe), full-saturation greens at moss areas (low-saturation only), faction-insignia-shaped wear patterns (the wear is incidental social geography, not design).

## Technical Spec

| Channel | Resolution | Notes |
|---------|-----------|-------|
| Albedo | 1024² | PNG 8-bit; tileable at 1:1.5m scale (per S8.3 Ground tile note) |
| Normal | 1024² | PNG 8-bit; cobble relief + center-polish microvariation |
| Roughness | 512² | PNG 8-bit; **load-bearing** — smooth at center of each stone, rough at edges, rougher at grout |
| Metallic | — | Not used |

- **Mipmap bias:** `-0.5` starting value (UNVERIFIED, F-08). Ground tile is viewed at very close range (camera distance 1-3m typically), so mipmap behavior here is less risk-coupled than facade tiles.
- **URP material:** Standard PBR. SRP Batcher compatible. Vertex-painted overlay channel for per-section wear variation.
- **Naming:** Per S8.2 — `env_ground_cobble_street_neu_yr200_[map-type].png`.

## AI Generation Prompt

> Hand-cut cobblestone street texture, 200 years old, neutral pre-faction construction. Seamless tileable PBR material at ~1.5m scale. Irregular cobbles 12-20cm per stone. **Foot-traffic polish at the center of each stone** (the load-bearing detail — smooth center, rough outer edge per stone). Pewter-weighted gray dominant, faint umber-brown patina from rain history at low spots. Clean grout lines (Court-maintained district). NO biological growth at this version (alley version is separate). NO uniform machine-cut paving, NO straight courses. Overcast 6000K diffuse lighting. Reference: 17th-century Italian piazza paving still in use, polished by 200 years of foot traffic.

## Production Notes

- **Generate the worn variant first.** The center-polish wear pattern is the asset's load-bearing detail; an unworn cobblestone tile is wrong-fidelity. If using AI generation, multiple passes may be needed: first to get the cobble layout, then to enforce the per-stone center-polish gradient.
- **Vertex-painted overlay (separate task):** Author a mask channel that handles street-vs-alley variation (alley = moss at grout). This is a separate Substance Painter or Blender workflow, not part of the tile albedo itself.
- **Validation:** test the tile under S2 State 1 overcast (6000K diffuse) lighting at ground-level camera angle. The center-polish gradient must read at close range; if it doesn't, the asset has lost its load-bearing detail.
- **Co-asset:** The "low-traffic alleys show moss encroachment at the grout lines" variant is a future spec — for M3 cemetery district main path, the clean-grout Court-adjacent version is what ships.

## Source citations

- `design/art/art-bible.md:95` (S2 State 1 — THE ONE visual element: wear patterns on stone)
- `design/art/art-bible.md:874-875` (wear directionality tells social history — S6.1 cue 4)
- `design/art/art-bible.md:962-965` (S6.2 ground surface tile budgets)
- `design/art/art-bible.md:1554-1556` (S8.3 Ground tile resolution)
- `design/art/art-bible-t1-scope.md` Environment section
