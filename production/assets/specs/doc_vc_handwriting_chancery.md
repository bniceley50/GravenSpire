# Asset Spec — doc_vc_handwriting_chancery

**Asset id:** `doc_vc_handwriting_chancery`
**Type:** Layer 2 typography (diegetic handwriting font for Vampire Court documents)
**Tier:** —
**Faction:** Vampire Court
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director (with localization-lead consultation on character coverage)
**Source bible refs:** `design/art/art-bible.md` S4.6 (Court chancery handwriting distinguishes from Syndicate cipher under greyscale), S7.7 (faction-specific handwriting × 6 — Vampire Court is the M3 target), S7.11 (Layer 2 typography is entirely separate from Layer 1 — see HUD typography)

## Purpose

The **handwriting that appears on top of** `doc_vc_paper_vellum_stock` (asset #14). Every Vampire Court Layer 2 document — posted dispatch, sealed letter, archived notice — uses this hand. Per S4.6 the Court's chancery hand must remain readable as "Court" against Syndicate's cramped cipher even when the paper colors fail (greyscale survival test). The hand is the Court's **graphic voice** — geometric precision applied to ink-on-vellum.

Per S7.11 Layer 2 forbidden: "Documents without faction attribution visible in material alone (if faction cannot be read from paper + handwriting before text content, the document fails)." This asset is half of that compliance.

## Visual Spec

- **Hand:** Italian chancery cursive (cancelleresca corsiva) of the gothic-late-medieval period (~13th-14th century reference). Specific, formal, geometrically-controlled. NOT cramped (that's Syndicate); NOT ornamental (that's Academy); NOT calligraphic-decorative (Pre-Raphaelite over-styling).
- **Letterform character:**
  - Upright posture with very slight forward slope (5-8° lean)
  - Long downward strokes on descenders (the Court's elongation reads as "controlled time" — the writer was not in a hurry)
  - Consistent baseline, consistent x-height
  - **Geometric precision in pen control** — no waver, no hesitation, no scratched corrections
- **Ink:** Iron-gall ink, **aged to brown-black** on the gray-blue vellum substrate. Modern iron-gall ages from black to dark brown over decades; the Court's chancery uses iron-gall by tradition.
- **Per S4.6 production note:** "both survive greyscale" — the handwriting must remain readable as Court chancery even when the paper's gray-blue cast is invisible. Achieved through letterform character itself, not through color.
- **Forbidden:** decorative flourishes beyond minimal ascender curl, illustration / illumination interleaved with text (that's Academy register), modern san-serif (HUD register), italic faux-Italian (Court hand is upright; the slight slope is angular, not italic-slope), gold accents (S4.8 forbidden + the Court doesn't gild paper).

## Authoring approach — Font asset, NOT image asset

The Vampire Court chancery hand is delivered as a **font file** (TTF or OTF), not as image assets. This is critical because:

- The font enables runtime text composition (so different documents have different content)
- The font carries character coverage for localization (future T2+ work)
- AI-generated handwriting on image cannot scale to runtime composition

**Working font candidates (open-source / web-fonts):**

- **EB Garamond** (italic variant) — too italic for the Court's upright posture; only useful as reference
- **Cardo** (Roman variant) — closer to the chancery register but still too book-typesetting
- **AD-direction needed:** the bible's exact target is a chancery face that doesn't exist as a commercial font at the level of stylization required. Three options:
  1. **License a chancery-style commercial font** (e.g., MyFonts Chancery Cursive variants) — AD reviews available faces and picks
  2. **Commission a custom font** from a type designer with chancery experience — multi-week timeline; produces the most authentic result
  3. **Use a "good enough" commercial face for T1 M3** (e.g., Pirata One or similar gothic-blackletter Google Fonts as placeholder) and commission the custom font for T2

**Recommended T1 path:** Option 3 (good-enough placeholder) for M3 visible-art milestone; queue Option 2 (custom commission) for T2 polish pass.

## Technical Spec

- **Format:** TTF or OTF font file
- **Character coverage (T1):** Basic Latin (ASCII printable). Localization expansion to extended Latin diacritics deferred to T2 per `design/quick/quick-design-m3-objective-npc-loot.md` T1 templated-dialogue scope.
- **Hinting:** standard TTF auto-hinting acceptable at 30-32px range
- **In-Unity application:** TextMeshPro asset compiled from the source font. Apply to Layer 2 documents in UI Toolkit panels.
- **Ink color:** runtime-applied in TMP material; brown-black on Court vellum — `#3D2E1A`-ish on the slate-violet-cast paper, darker than black-on-white because the paper is darker than white.
- **Naming:** Per S8.2 — `doc_vc_handwriting_chancery.ttf` (font); `doc_vc_handwriting_chancery_tmp.asset` (TMP signed-distance-field asset).

## AI Generation Prompt (for concept art only — not for the actual font)

> Vampire Court chancery handwriting sample, written on gray-blue vellum. Italian chancery cursive (cancelleresca corsiva) of the gothic-late-medieval period, upright posture with very slight forward slope (5-8°), long downward strokes on descenders, geometric precision in pen control. Iron-gall ink aged to dark brown. NO decorative flourishes, NO illumination, NO calligraphic over-styling. Reference: real 13th-14th century Italian chancery document. Sample text could be a short formal phrase like "By order of the Court" — render as a 2-3 line excerpt at high resolution suitable for concept reference.

## Production Notes

- **The font asset is the deliverable, not an image.** AI-generated handwriting samples are useful as **direction reference for the type designer** OR as a starting point for choosing a commercial face, but they cannot ship as Unity Layer 2 text.
- **T1 placeholder is acceptable.** If commissioning a custom chancery font is out of T1 scope, use a commercial gothic-script face with appropriate licensing (the Cinzel family from Google Fonts is open-license; another commercial chancery face under proper license is fine). Document the choice in the asset manifest and mark for T2 replacement.
- **Greyscale survival test (S4.6):** print the Court chancery and the Syndicate cipher (future asset) at 100% greyscale and confirm both remain readable AS faction signals. The Court reads as "formal, controlled, geometric"; the Syndicate (future) will read as "cramped, dense, functional."
- **Tritanopia path:** the handwriting itself is the tritanopia-safe channel — color is in the paper, character is in the form. This is exactly what S4.6 designed for.

## Source citations

- `design/art/art-bible.md:602` (S4.6: Court chancery vs Syndicate cipher — both survive greyscale)
- `design/art/art-bible.md:1239-1276` (S7.7 faction handwriting traditions; production note flagging tech-artist conversation)
- `design/art/art-bible.md:1425-1428` (S7.11: Layer 2 forbidden — documents without faction attribution in material alone)
- `design/art/art-bible-t1-scope.md` Layer 2 section (Vampire Court only at M3)
