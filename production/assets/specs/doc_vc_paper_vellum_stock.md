# Asset Spec — doc_vc_paper_vellum_stock

**Asset id:** `doc_vc_paper_vellum_stock`
**Type:** Layer 2 paper stock (diegetic document substrate)
**Tier:** —
**Faction:** Vampire Court
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director
**Source bible refs:** `design/art/art-bible.md` S2 State 9 (line 186: "Vampire Court dispatches are written on gray-blue vellum"), S4.6 (Court vellum vs Syndicate parchment color-blind distinction), S7 (UI/HUD Visual Direction — Layer 2 diegetic documents), S7.11 (Layer 2 forbidden: consistent white/neutral paper stock)

## Purpose

The **substrate** on which every Vampire Court diegetic document appears in M3 — posted dispatches, sealed letters, notice-board postings, the objective hand-in's accompanying ledger entry. Per S7.11 Layer 2 forbidden list: "Documents on consistent white/neutral paper stock — faction stock is mandatory." Without this asset, Layer 2 documents cannot be authored faithfully. Per S2 State 9: "Vampire Court dispatches are written on gray-blue vellum" — this is the Court's stock by canonical definition.

**This is the foundational Layer 2 asset.** The handwriting (asset #15) and seal (asset #16) sit on top of this paper.

## Visual Spec

- **Material:** Gray-blue vellum — calf-skin parchment with the bluish-gray cast specific to Court production. Per S4.6, the Court vellum's distinguishing feature against Syndicate parchment (old-wax) is **texture pattern**: Court is smooth/thin/formal; Syndicate is rough/thick/fibrous.
- **Color:** Faint slate-violet cast in the highlight; Pewter Rain (`#9EA4A8`-ish) in the midtone; Iron Seam (`#3D3A38`) in the shadow / fold lines. **NOT pure gray** — the slate-violet tint is the Court signal.
- **Surface treatment:**
  - Smooth on the recto (writing surface); slightly more textured on the verso (skin side)
  - Visible fiber direction (subtle — vellum is processed enough to be smooth, but the original animal-tissue grain is still readable at close range)
  - **Aged appropriately to the document's inferred age** — see Production Notes for age-tier variants
- **Edges:**
  - Court documents are **trimmed precisely** — geometric edge per S6.1 Court precision rule. NOT torn, NOT deckled.
  - Slight foxing at corners where document has been handled
- **Forbidden:** White paper (S7.11), torn edges on a Court document (Syndicate vocabulary), uniform color (must have visible material variation), crisp-bright (must be aged), gold leaf or metallic accents (the Court has them on hardware, not on paper — paper is humble substrate).

## Age-tier variants

Per S7.7 "every seal shows age consistent with document's inferred age" — the paper has the same rule. Author 3 age-tier variants of the base stock:

| Variant | Inferred age | Visual difference |
|---------|--------------|-------------------|
| `yr0_fresh` | Document issued recently (within 1 year) | Crisp; minimal foxing; clean folds if any |
| `yr20_handled` | Document in active use (1-20 years) | Light foxing at corners; one fold line visible; faint hand-oil at handling zones |
| `yr80_archived` | Document old (20-80 years; archived but not stratum-2 ancient) | Significant foxing; multiple fold lines; surface darkening; possible water stain at one edge |

## Technical Spec

| Channel | Resolution | Notes |
|---------|-----------|-------|
| Albedo | 1024² per variant | PNG 8-bit; uses S8.3 Layer-2 / document budget (closest match: Architecture — Secondary or Props — Major, depending on render context) |
| Normal | 512² per variant | PNG 8-bit; vellum surface texture (subtle) |
| Roughness | 512² per variant | PNG 8-bit |
| Metallic | — | Not used (paper is not metallic) |

- **Authoring approach:** Single base material with per-variant texture sets. The age progression is in the textures, not in the shader.
- **Naming:** Per S8.2 — `doc_vc_paper_vellum_stock_yr0_fresh_alb.png`, `doc_vc_paper_vellum_stock_yr20_handled_alb.png`, `doc_vc_paper_vellum_stock_yr80_archived_alb.png`.
- **In-Unity application:**
  - For UI Toolkit Layer 2 panels (in-world documents that the player reads from a held-paper-in-hand framing per S7.4): apply as the panel's background image
  - For diegetic placed-in-world documents (posted dispatches, sealed letters on tables): apply as a mesh material on a flat quad with the document's content as a separate texture overlay

## AI Generation Prompt

> Gray-blue vellum parchment, Vampire Court production, smooth/thin/formal — NOT old-wax/rough/fibrous (that would be Syndicate). Faint slate-violet cast in highlights; pewter-gray midtones; iron-seam dark in fold shadows. Geometric trimmed edges (Court precision; NOT torn or deckled). Subtle visible fiber direction on the smooth recto side. Three age variants: fresh (within 1 year, minimal foxing); handled (1-20 years, light foxing + one fold + hand-oil); archived (20-80 years, significant foxing + multiple folds + surface darkening + edge water stain). Pre-Raphaelite restraint; subtle, weighty, specific. Reference: real Italian medieval Court chancery vellum, preserved but used.

## Production Notes

- **The 3 age-tier variants are the most important deliverable.** A single "default vellum" reads as fantasy paper; three age-graded versions readable side-by-side communicate the Court's archival depth — the bible's "preserved in use, not museum" rule applied to paper.
- **Tritanopia check:** Court vellum vs Syndicate parchment must remain distinguishable for tritanopia per S4.6. The Court's gray-blue + Syndicate's old-wax-yellow sit on opposite ends of the blue-yellow axis — naturally distinct. Validation requires both authored before the test is meaningful.
- **Watermark / chancery imprint:** Court documents typically have a faint chancery imprint pressed into the paper (sub-saturation; texture-only — the bible doesn't specify but the Court's geometric precision implies it). Optional Stage-2 detail: add a subtle pressed pattern to the paper's normal map (e.g., a small Court geometric mark in the upper margin).
- **Co-asset dependency:** asset #15 (`doc_vc_handwriting_chancery`) is authored AS INK ON THIS PAPER. The handwriting spec assumes this paper stock and its slate-violet cast. Do not author the handwriting spec independently of the paper's color values.

## Source citations

- `design/art/art-bible.md:186` (S2 State 9: "Vampire Court dispatches are written on gray-blue vellum")
- `design/art/art-bible.md:602` (S4.6: Court vellum vs Syndicate parchment, color-blind distinction)
- `design/art/art-bible.md:1425-1427` (S7.11: Layer 2 forbidden — consistent white/neutral paper stock; faction stock is mandatory)
- `design/art/art-bible-t1-scope.md` Layer 2 section (Vampire Court only at M3)
