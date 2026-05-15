# Asset Spec — doc_vc_seal_wax_01

**Asset id:** `doc_vc_seal_wax_01`
**Type:** Layer 2 seal (diegetic wax seal applied to Vampire Court documents)
**Tier:** —
**Faction:** Vampire Court
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director
**Source bible refs:** `design/art/art-bible.md` S7.7 (faction seals — Court vocabulary), S7.11 (Layer 2 forbidden: "Crisp wax seals or fresh-pressed stamps — every seal shows age consistent with document's inferred age"), S6.1 (Court hardware: silver-then-tarnished)

## Purpose

The Vampire Court's wax seal is the **third visual layer** completing Layer 2 alongside the vellum paper (asset #14) and chancery handwriting (asset #15). A Court document without a seal is unauthenticated; a Court document WITH a seal carries the Court's authority into the player's hand. The bible's S7.11 forbidden item — "Crisp wax seals or fresh-pressed stamps" — applies here as a positive constraint: every Court seal must show age consistent with the document's inferred age.

## Visual Spec

- **Wax color:** Court's vocabulary uses a **deep slate-violet** wax — `#5B4A60` to `#6E5A75` range. NOT bright purple (S4.8 forbids saturated faction colors); NOT red (red is biological per S4.8); NOT gold (forbidden as reward signaling). This is the Court's accent color in physical material form.
- **Press impression:** Geometric Court precision per S6.1. The seal's design is **NOT a representational image** (no skull, no bone, no portrait); it is an architectural / heraldic geometric mark. Working candidate: a stylized pointed arch within an inner circle, with two opposing geometric flanks suggesting either pillars or wings. Final design pending AD ratification.
- **Age treatment (load-bearing per S7.11):**
  - **fresh variant:** seal pressed within the last year; press impression sharp; wax has not yet darkened at the edges
  - **handled variant:** seal 1-20 years old; press impression has slightly settled; faint surface darkening at the high points where handling has accumulated oil
  - **archived variant:** seal 20-80 years old; the impression has lost some crispness; the wax has shifted slightly under its own weight (gravitational creep over decades); surface chemistry has darkened
- **Adhesion to vellum:** The wax bonds to the vellum with a slight visible spread at the contact edge. Where the seal sits over a fold or edge of the document, the wax shape distorts naturally.
- **Forbidden:** crisp pressed surfaces (S7.11), bright saturated wax color, red sealing wax (S4.8 + signals "blood" wrongly), gold-embossed designs, decorative tassels or ribbons under the seal (Court is restrained), skull / bone iconography in the press design (S3.6).

## Variant inventory (3 wax age variants × 1 press design)

| Variant id | Age tier | Visual state |
|-----------|----------|--------------|
| `seal_vc_wax_yr0_fresh` | Fresh (within 1 year) | Sharp impression, full wax color, no darkening |
| `seal_vc_wax_yr20_handled` | Handled (1-20 years) | Slightly settled impression; faint surface darkening at high points |
| `seal_vc_wax_yr80_archived` | Archived (20-80 years) | Soft impression; gravitational creep; surface chemistry darkening |

## Technical Spec

- **Format:** PNG 8-bit albedo + normal + roughness for each variant. The seal is a **2D asset rendered as if photographed**, applied as a sprite/decal over the document's paper texture in UI Toolkit.

| Channel | Resolution | Notes |
|---------|-----------|-------|
| Albedo | 512² per variant | PNG 8-bit |
| Normal | 256² per variant | PNG 8-bit; surface relief of the press impression |
| Roughness | 256² per variant | PNG 8-bit; smooth where wax is most-recent (fresh); rougher where age has affected surface |
| Alpha | Included in albedo | PNG 8-bit; cuts the seal shape against transparent background for compositing onto paper |

- **Authoring approach:**
  - Create one master 3D model of the seal with press impression
  - Render under controlled studio lighting (single 4000K key + soft fill) at 3 wax-age states
  - Capture the rendered output as 2D sprite with normal/roughness baked
  - Alternatively, author entirely in Substance Painter as a 2D-but-PBR texture set
- **Naming:** Per S8.2 — `doc_vc_seal_wax_yr0_fresh_alb.png`, `doc_vc_seal_wax_yr20_handled_alb.png`, `doc_vc_seal_wax_yr80_archived_alb.png`.

## AI Generation Prompt

> Vampire Court wax seal, deep slate-violet wax (#5B4A60 to #6E5A75 range — NOT bright purple, NOT red, NOT gold), pressed onto gray-blue Court vellum. The press impression is a geometric architectural mark — a stylized pointed arch within an inner circle with two opposing geometric flanks (NOT a skull, NOT bones, NOT a portrait, NOT heraldic figures). Three age variants: fresh (sharp impression, no darkening), handled (slightly settled, faint darkening at high points), archived (soft impression, gravitational creep, surface darkening). The wax adhesion to vellum shows a slight visible spread at the contact edge. Pre-Raphaelite restraint; architectural precision. Reference: real 13th-14th century Italian chancery seals.

## Production Notes

- **Three age variants are mandatory per S7.11.** Authoring only the "fresh" version and re-using it on all documents violates the bible's forbidden list. The age variation IS the asset's bible-compliance.
- **Press design AD ratification:** the working "pointed arch within inner circle with geometric flanks" is a starting point. AD reviews the first-pass concept and either ratifies or revises. The design must NOT contain skull/bone iconography (S3.6) and must NOT be a literal heraldic figure (over-stylization).
- **Color discipline:** the slate-violet wax range is **load-bearing for tritanopia** — confirms Court material identity against Syndicate's old-wax (yellow-brown) seal vocabulary. Test in greyscale: the Court seal's deep slate-violet reads as a darker mid-gray; the Syndicate's old-wax reads as a lighter warm-gray. Both must remain distinguishable.
- **Composition workflow:** at runtime, the seal sprite is composited over the vellum + handwriting layers in Unity UI Toolkit. The normal/roughness channels on the seal are physically integrated with the paper's material (so the seal reads as 3D wax on flat paper, not as a sticker).
- **Co-asset dependency:** depends on assets #14 (vellum) + #15 (chancery handwriting). The seal sits ON TOP of those; they must exist first or the composition test is meaningless.

## Source citations

- `design/art/art-bible.md:1239-1276` (S7.7 faction seals; production note)
- `design/art/art-bible.md:1425-1426` (S7.11 forbidden: crisp wax seals / fresh-pressed stamps)
- `design/art/art-bible.md:921` (S6.1 Court vocabulary: silver-then-tarnished hardware; precision)
- `design/art/art-bible.md:619-620` (S4.8 forbidden: gold-as-reward, red-as-stylized-danger)
- `design/art/art-bible.md:76` (S1 forbids skull/bone iconography)
- `design/art/art-bible-t1-scope.md` Layer 2 section
