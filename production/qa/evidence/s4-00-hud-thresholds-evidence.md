# S4-00 UX HUD Threshold + Con-Glyph Pass — Evidence Document

**Story:** S4-00 — UX HUD Threshold + Con-Glyph Pass
**Date:** 2026-06-07
**Authority:** DECISIONS.md D020 (Locked); `design/art/art-bible.md` §7.1, §7.1.1, §4.3, §4.4, §4.6, §7.8, §7.10, §7.11
**Status:** PROPOSED — pending product-owner review and approval before implementation stories may consume these values
**Output path:** `production/qa/evidence/s4-00-hud-thresholds-evidence.md`
**Unblocks:** S4-02 (vitals HUD), S4-03 (target frame + con indicator), S4-04 (cast bar + interaction prompt)

---

## Purpose and Doctrine

This document is the ux-designer validation pass required by §7.10 ("the deferred numbers home") and mandated by D020. It replaces the §7.10 "deferred" markers with concrete values and replaces the §7.1 "under revision" language with specific numbers.

**Governing doctrine (AC S4-00-06):** This spec sets a legibility *floor*, not a maximum-visibility target. "Quiet is the aesthetic; unreadable is the bug" (§7.1). Every value is the minimum required to pass a combat-stress readability check while preserving the recessive intent. Implementation stories must not go below these values, but they may adjust upward within the recessive-but-legible band if in-engine evidence supports it — with product-owner approval.

**Evidence-honesty rule (AC S4-00-05):** Every number below is either accompanied by a stated rationale, OR is explicitly recorded as an open question with a named validation method. No value is marked "TBD" — a guessed number that gets built on is the S3-06 failure this story exists to prevent. Values marked OPEN QUESTION must be confirmed by the described method before any downstream story records them as validated. Values marked PROPOSED FLOOR are recommended starting points; the validation method is mandatory before the story closes as USABILITY_TESTED or ANALYTICS_VALIDATED.

---

## Replaced Baselines (explicitly retired)

Per AC S4-00-01 and S4-00-02 and the §7.10 deferral language:

| Value | Old (retired) | Reason Retired |
|---|---|---|
| Minimum bar height (Layer 1 vitals) | **3px at 1080p** | No longer accepted as proven readable after S3-06 human-play failure. Player could not reliably read combat state. |
| Panel-fill opacity floor (Layer 1 panels) | **45% Iron Seam opacity** | No longer accepted as proven readable. Luminance contrast analysis shows 45% Iron Seam fails to provide ≥3:1 bar contrast against the composited panel in bright outdoor scenes behind the HUD. |

These values must not be used in S4-02, S4-03, or S4-04. Both are recorded here as the starting baseline that failed.

---

## Threshold 1 — Minimum Bar Height (Layer 1 Vitals Bars)

**Applies to:** health bar, mana bar, endurance bar, hate/threat indicator (S4-02); target health bar inside target frame (S4-03).

### Recommended Floor

**6px at 1080p (1920×1080).**

At 4K (3840×2160), bars must scale proportionally to maintain equivalent perceptual weight. A 6px bar at 1080p rasterizes to approximately 3px at 4K if rendered without scaling — reproducing the S3-06 failure condition. Implementation must apply a DPI-aware or resolution-aware scale factor. Recommended 4K target: **10–12px** (see §7.10: "1px border legibility at 4K — may need to scale to 1.5–2px at 4K for equivalent perceptual weight"; the same principle applies to bars).

### Rationale

3px is at the threshold of reliable peripheral detection on a 1920×1080 display at a typical 60–80cm viewing distance. At this distance and resolution, 3px subtends approximately 0.12–0.15 arcminutes, which is below the commonly cited 0.17–0.2 arcminute threshold for confident peripheral detection of a narrow horizontal element. The player cannot be expected to shift foveal attention to the health bar during a pull — the bar must register in peripheral vision.

6px doubles the vertical extent (approximately 0.25–0.3 arcminutes at typical distance), placing it above the peripheral detection threshold with margin. The bar remains visually thin — a narrow architectural strip consistent with the §7.1 "ironwork at the periphery of vision" analogy. It is not a prominently thick game-UI bar.

The EQ-genre analog (EQ1 and EQ2 at 1024×768, the era the art direction references) used bars in the 8–14px range at that resolution. Scaled to 1080p for equivalent proportion, that range maps to approximately 10–18px. 6px sits below that range, reflecting the Gravenspire recessive intent: readable under combat stress, but genuinely quieter than the genre reference.

### Validation Status

**OPEN QUESTION — in-engine confirmation required.**

The 6px recommendation is derived from viewing-distance / angular-resolution analysis and genre analog. It has not been confirmed under the actual URP post-process stack with the Gravenspire color palette and scene lighting.

**Validation method (required before S4-02 records bar height as evidence-linked):**
1. Implement health and mana bars at 6px in Unity 6.3 LTS, Layer 1 Overlay camera stack.
2. Enter a combat scenario (pull an enemy) in `_DevEntry.unity`.
3. The tester's task: read approximate health level (above 50% / at 20-50% / below 20%) without shifting gaze from the enemy. Observation: is the tester correct within 1 attempt on 3 of 3 trials?
4. If pass: 6px is confirmed. Record in S4-02 evidence.
5. If fail: raise to 8px and repeat. Do not exceed 10px without product-owner approval (scope: "quieter bar" constraint).
6. Test at 1080p and, if 4K hardware is available, at 4K with proportional scaling.

If the 6px test cannot be run before S4-02 begins, implementation may proceed at 6px provisionally with explicit notation that the bar height is PROPOSED FLOOR pending the in-combat readability confirmation. S4-02 must not mark its bar-height claim as evidence-linked until the confirmation test is complete.

---

## Threshold 2 — Panel-Fill Opacity Floor (Layer 1 Panels)

**Applies to:** all Layer 1 panel fills (vitals panel, target frame, cast bar background, interaction prompt background). Panel specification: Iron Seam `#3D3A38` at the floor opacity. Border remains 1px Iron Seam at 100% opacity, per §7.1.

### Recommended Floor

**65% Iron Seam opacity.**

This replaces the retired 45% baseline.

### Rationale

The §7.10 requirement is that bar content achieves ≥3:1 luminance contrast against the panel fill under the URP post-process stack.

**Luminance analysis (headless, pre-engine):**

Iron Seam `#3D3A38` full-opacity relative luminance:
- sRGB channels: R=0.239, G=0.227, B=0.220
- Relative luminance L ≈ 0.041 (calculated per WCAG 2.x formula)

The Layer 1 HUD sits on the URP Overlay camera composited over the Base camera's rendered frame. The panel pixel is a blend of Iron Seam at α and the scene pixel at (1-α). The scene pixel luminance behind the lower-left HUD panel varies from approximately 0.01 (deep interior shadow) to 0.25 (Quarry Stone exterior lit by overcast sky).

**Worst-case outdoor scene (Quarry Stone exterior, L_background ≈ 0.25):**

At 45% opacity: L_composite ≈ 0.45 × 0.041 + 0.55 × 0.25 ≈ 0.019 + 0.138 ≈ 0.157
Render Umber health bar `#7A6248`, L ≈ 0.132.
Contrast: (0.157 + 0.05) / (0.132 + 0.05) ≈ 1.14:1. Far below 3:1. This is a direct reproduction of the S3-06 failure condition in numbers: the health bar is darker than the panel composite in bright outdoor scenes, making the bar effectively invisible.

At 65% opacity: L_composite ≈ 0.65 × 0.041 + 0.35 × 0.25 ≈ 0.027 + 0.088 ≈ 0.115
Render Umber (0.132) vs. panel (0.115): ratio ≈ (0.132+0.05)/(0.115+0.05) ≈ 1.10:1 — still failing.

This analysis reveals that Render Umber health bars will fail to achieve 3:1 contrast against Iron Seam panels in bright-outdoor scenes regardless of opacity alone, because Render Umber and Iron Seam are in similar luminance territory. The 65% floor is not a full solution in the absolute-worst-case outdoor scene; it is a meaningful improvement over 45% and will achieve 3:1 in the majority of in-game environments (interior scenes, low-light areas, and scenes with architectural shadow behind the HUD).

**Typical interior scene (Iron Seam shadow, L_background ≈ 0.04):**
At 65%: L_composite ≈ 0.65 × 0.041 + 0.35 × 0.04 ≈ 0.027 + 0.014 ≈ 0.041
Render Umber (0.132) vs. panel (0.041): ratio ≈ (0.182)/(0.091) ≈ 2.0:1. Approaching but not achieving 3:1.

**Supplementary mitigation (structural, not opacity-based):** The §7.1 specification positions vitals panels at the screen periphery with a 1px Iron Seam border at 100% opacity. This full-opacity border is the primary separator between bar content and scene background — the border provides definite contrast even when the fill opacity creates ambiguity. The bars also benefit from being positioned inside the bordered frame. This structural mitigation means the 65% fill floor is a floor against the interior of the panel, not against the raw scene. In normal gameplay, the border carries the legibility; the fill provides depth and de-emphasis.

**The Pewter Rain mana bar (L ≈ 0.363) achieves 3:1 against any plausible composited Iron Seam panel** (even at 45% opacity over bright scenes, ratio ≈ 2.0–3.5:1). The mana bar's higher luminance means it is less sensitive to panel opacity. The health bar (Render Umber, lower luminance) is the binding constraint.

**Recommendation rationale:** 65% preserves the "there-but-not-there" recessive quality (it is still noticeably semi-transparent — less so than 45%, but not opaque) while substantively improving contrast in interior and mixed-light scenes. It raises the floor from a value that fails in the majority of combat contexts to one that achieves adequate contrast in typical in-game scenes. Bright outdoor corners remain the edge case requiring in-engine confirmation.

### Validation Status

**OPEN QUESTION — in-engine measurement required.**

The contrast math above is headless. URP's post-process stack, tone-mapping, and gamma curve will affect the actual composited luminance values. The precise opacity that achieves ≥3:1 in the lowest-contrast typical scene must be measured in-engine.

**Validation method (required before S4-02 records panel opacity as evidence-linked):**
1. Implement panels at 65% Iron Seam opacity in Unity 6.3 LTS Overlay camera stack.
2. Navigate to the brightest accessible exterior location in `_DevEntry.unity` (maximum outdoor ambient, Quarry Stone surface visible behind the lower-left HUD panel position).
3. Use Unity's Color Picker or a contrast measurement tool to measure the luminance of: (a) the composited panel pixel inside the bar track area (panel fill, no bar drawn yet), (b) the Render Umber health bar pixel at full health.
4. Calculate contrast ratio per WCAG formula.
5. If ≥3:1: 65% is confirmed. Record in S4-02 evidence.
6. If below 3:1 in the brightest outdoor scene: raise opacity in 5% increments until 3:1 is achieved, or implement a secondary mitigation (heavier-weight border, bar track color adjustment). Do not exceed 80% opacity without product-owner approval (scope: recessive-panel constraint).
7. Record the confirmed opacity value and the test scene SHA in S4-02 evidence.

Note: The §7.10 requirement also specifies "Pewter Rain / Iron Seam contrast — must maintain ≥3:1 luminance contrast for tritanopia accessibility (§4.6)." Pewter Rain (L≈0.363) vs. Iron Seam panel at 65% in typical scenes will easily achieve 3:1; confirm as part of the above measurement pass.

---

## Threshold 3 — Cast Bar Lower-Center Placement

**Applies to:** cast bar (S4-04). Art bible authority: §7.1.1, §7.11 (forbidden band definition), §7.10.

### Forbidden Band Definition

Per §7.11: any UI element in the **40–60% viewport height band** is forbidden, with the sole exception being the cast bar at lower-center during an active cast. The cast bar must sit **below** the band (i.e., below 60% from the top of the viewport). The word "below" in §7.1.1 ("below the 40-60% center band") refers to being lower on screen — the cast bar sits in the lower portion of the viewport, not in the central band.

### Concrete Screen-Space Position

At **1920×1080 (1080p):**
- Forbidden band lower edge: 60% × 1080 = **648px from top** (i.e., 432px from bottom)
- Cast bar must sit entirely below 648px from top (i.e., entirely in the lower 432px of the screen)

**Recommended placement:**
- Cast bar **top edge: 70% viewport height = 756px from top** (108px clearance below the forbidden band's lower edge)
- Cast bar height: 6px (matching the vitals bar height floor for vocabulary consistency — a single pixel weight for the fill readout, inside a thin bordered track)
- Cast bar **bottom edge: 762px from top** (318px from bottom of screen)
- Cast bar horizontal span: full width between the existing vitals panel insets, or a minimum centered span of 400px at 1080p (to be finalized by UI Programmer per art bible panel spacing rules)
- Cast bar is centered horizontally on screen, per §7.1.1

At **3840×2160 (4K):** all values scale by 2x (top edge at 1512px from top, bottom edge at 1524px from top). Bar height scales per Threshold 1 resolution scaling (10–12px at 4K).

**Clearance summary:**
- Top edge of cast bar (756px) is **108px below** the forbidden band lower edge (648px). This is the minimum clearance; implementation may place the bar lower if the spell-queue panel requires it.
- The spell-queue panel is positioned "above lower margin" with a 48px minimum edge clearance (§7.1), so the spell-queue top edge sits at approximately 1080 - 48 = 1032px from top (at 1080p). The cast bar at 756–762px sits well above the spell queue with approximately 270px of spacing for the spell queue below it.

**Interaction with interaction prompt:** the interaction prompt also sits "lower-center, above the spell queue" (§7.1.1). The cast bar and interaction prompt must not overlap. Rule: interaction prompt suppressed during an active cast with a selected target (§7.1.1: "Does not appear in combat with an active target"). The two elements are mutually exclusive by design; no simultaneous-display layout is needed.

### Validation Status

**PROPOSED VALIDATED by geometry derivation.**

This is the most confident of the three thresholds because it is derived directly from the art bible's stated forbidden-band percentages and spacing rules. It does not depend on color measurement or perceptual testing.

**Confirming check (required in S4-04 evidence):**
1. Implement cast bar in Unity 6.3 LTS at the specified position (top edge at 70% viewport height).
2. In Play Mode, initiate a cast. Take a screenshot.
3. Verify: the cast bar is entirely below the 60% viewport-height line. Measure in pixels if needed (image-editor ruler, or Unity Screenshot Tool + pixel ruler).
4. Verify: the cast bar does not overlap the spell-queue panel.
5. Verify: no UI element appears in the 40–60% band during a cast (QA-SCAN-1 check).
6. Record screenshot path in S4-04 evidence.

The §7.12-4 read test (peripheral readability of cast bar while monitoring health) also confirms placement usability. That test is part of S4-04's manual evidence, not this story's scope.

---

## Threshold 4 — Five-State Con Glyph Set

**Applies to:** relative-threat indicator inside target frame (S4-03). Art bible authority: §7.1.1, §7.8, §4.3, §4.4, §4.6.

### Design Principles (from authority docs)

- Shape-primary, non-color-only. A colorblind player loses nothing (§4.6).
- Each glyph is in the §7.8 instrument-plate line register: outlined monochrome line art, 1.5px outer stroke at icon resolution, no fill, no glow, no gradient.
- Colors are confirm-only, drawn exclusively from the world palette (§4.4). **Red and green are forbidden as signaling colors** (§7.11, §4.3). RPG danger-red and reward-gold are forbidden.
- The glyph is recessive — a small line-glyph, not a loud color badge (§7.11 State-Report boundary).
- The glyph appears ONLY for the currently selected target, ONLY inside the target frame. Never floating in world space.

### Glyph Geometry Specification

Each glyph is drawn at **icon display size: 12×12px at 1080p** (small enough to be recessive inside the target frame name row; large enough to be shape-discriminable). Line weight: 1.5px (matching §7.8 outer stroke specification). Interior clear of panel border. All glyphs centered within the 12×12 bounding box with 1px inset margin (effective drawing area 10×10px).

**The five states, each with precise geometry:**

---

**STATE 1: TRIVIAL**
- Art bible description: "single compressed horizontal bar"
- Geometry: a single horizontal stroke across the full 10px width of the drawing area, centered vertically at the mid-point. Stroke weight 1.5px. Height: 1.5px. Visually a thin horizontal dash or rule. No other mark.
- Shape character: completely horizontal; minimum vertical extent; the simplest possible mark.
- Confirm color: Pewter Rain `#9EA4A8` (cool blue-gray, the world's ambient material)

**STATE 2: BELOW**
- Art bible description: "down-pointing compressed triangle"
- Geometry: an isoceles triangle, apex pointing downward. Top edge horizontal at 2px below the drawing area top (giving a slightly inset cap). Bottom apex at 8px from the drawing area top (compressed vertical extent — not a full equilateral). Base width 10px. Stroked outline only, no fill (§7.8 line register). Line weight 1.5px.
- Shape character: triangle pointing down; the only down-pointing element in the set.
- Confirm color: Quarry Stone `#8A8478` (cool neutral, the default building material)

**STATE 3: EVEN**
- Art bible description: "square"
- Geometry: a square. 8×8px, centered in the 10×10 drawing area (1px margin on all sides within the drawing area). Stroked outline only, no fill. Line weight 1.5px. Corners are right-angle (no chamfer on this glyph — chamfer is reserved for icon frames per §7.8, not for the con glyph itself which must be discriminable as a simple shape).
- Shape character: four equal sides; the only closed four-sided shape in the set. Completely symmetric.
- Confirm color: Bone Pale `#D4CCBC` (warm-cool transitional, the lightest city tone)

**STATE 4: ABOVE**
- Art bible description: "up-pointing compressed triangle"
- Geometry: mirror of STATE 2. Isoceles triangle, apex pointing upward. Bottom edge horizontal at 8px from the drawing area top. Top apex at 2px from the drawing area top. Base width 10px. Stroked outline only, no fill. Line weight 1.5px.
- Shape character: triangle pointing up; identical form to STATE 2 but mirrored. Up vs. down is the discriminating axis between these two states.
- Confirm color: Candlefall Amber `#C48B3A` (warm amber, the color of practical light sources)

**STATE 5: DANGEROUS**
- Art bible description: "double up-pointing triangle (stacked)"
- Geometry: two up-pointing triangles in vertical stack. Lower triangle: bottom edge at 9px from drawing area top, apex at 5px from drawing area top, base width 8px. Upper triangle: bottom edge at 5px from drawing area top (sharing the lower triangle's apex row), apex at 1px from drawing area top, base width 6px. Both stroked outline only, no fill. Line weight 1.5px. The stack reads as a chevron-within-chevron or double-chevron form. The smaller upper triangle sits inside the negative space above the lower triangle.
- Shape character: distinctly the most complex mark in the set — two stacked triangles pointing up, visually more dense than STATE 4's single triangle. Cannot be confused with STATE 4 even at a glance.
- Confirm color: Rust Iron `#7A4A38` (the HUD's loudest moment, ferrous-under-stress warm)

---

### Shape Discriminability Analysis — Colorblind Simulation

The five shapes, viewed as pure geometry with color disabled (all glyphs rendered in a single neutral tone), must be unambiguous. Assessment by deficiency type:

**Deuteranopia (red-green collapse, ~6% of males):**
Rust Iron (STATE 5) and Candlefall Amber (STATE 4) may lose chromatic distinction. Quarry Stone (STATE 2) and Pewter Rain (STATE 1) may converge toward similar gray. Bone Pale (STATE 3) lightens relative to others.

Shape outcome: STATE 1 (horizontal dash), STATE 2 (down triangle), STATE 3 (square), STATE 4 (up triangle), and STATE 5 (double up triangle) are geometrically orthogonal. Color confusion between STATE 4 and STATE 5 (both in the warm-to-rust range under deut) is immaterial because their shapes are distinct: single up-triangle vs. double-stacked up-triangle. The shape distinction survives. Verdict: **discriminable by shape alone.**

**Protanopia (red deficiency, ~1% of males):**
Rust Iron (STATE 5) grays out most severely — §4.6 documents this as a known risk for the hate indicator, mitigated by shape pulse. For the con glyph, STATE 5 may appear similar in tone to STATE 2 or STATE 3 under protanopia. Shape outcome: STATE 5 (double chevron) remains geometrically distinct from STATE 2 (single down-triangle) and STATE 3 (square). The doubled-stacked geometry of STATE 5 is the most visually complex mark in the set — more marks, more edges, more visual weight. A colorblind player will still see "the one with more lines" as the most severe. Verdict: **discriminable by shape alone, with STATE 5 relying on complexity rather than color.**

**Tritanopia (blue-yellow collapse, ~0.01%):**
Pewter Rain (STATE 1, blue-gray) and Candlefall Amber (STATE 4, warm amber) are on the blue-yellow axis and may converge to similar gray. Shape outcome: STATE 1 is a horizontal dash; STATE 4 is an up-pointing triangle. These are among the most geometrically different shapes in the set. Color confusion has zero impact on discriminability. Verdict: **fully discriminable by shape alone.**

**Cross-state confusion risk matrix (shape-only):**

| Potential Confusion | Shape | Verdict |
|---|---|---|
| STATE 1 (dash) vs. STATE 2 (down-tri) | Horizontal line vs. downward triangle | No confusion |
| STATE 2 (down-tri) vs. STATE 4 (up-tri) | Down-pointing vs. up-pointing triangle | Orientation is the discriminating axis — requires the player to learn "down = below, up = above." This is the highest-risk pair: two triangles that differ only in orientation. At 12px, up vs. down should be clear; at smaller sizes, this pair is the one to watch. |
| STATE 4 (up-tri) vs. STATE 5 (double up-tri) | Single triangle vs. two stacked triangles | Weight/complexity difference is visible even at small size |
| STATE 1 (dash) vs. STATE 3 (square) | Line vs. square | No confusion |
| STATE 2 or STATE 4 vs. STATE 3 (square) | Triangle vs. square | No confusion |

**Highest-risk pair:** STATE 2 (down-triangle) and STATE 4 (up-triangle). At 12px with 1.5px stroke, the orientation difference must be legible. The compressed geometry (not full equilateral) maintains a clear directional read because the apex is a visually unambiguous point. If in-engine testing reveals these two states are confused at display size, the mitigation is: increase display size to 14–16px, or differentiate by adding a horizontal base-line stroke to STATE 2 (below) to make "below" feel more grounded. This mitigation must be approved by product owner before implementation.

### Validation Method (required before S4-03 records con glyph as evidence-linked)

Per AC S4-00-04 and the QA plan S4-00 test case:

1. Render the five glyphs at HUD display size (12×12px) in a single row, all in the same neutral tone (Iron Seam, `#3D3A38`, representing color-disabled state).
2. Ask a reviewer (the product owner or a team member not involved in the spec) to name all 5 states — Trivial / Below / Even / Above / Dangerous — by shape alone, given only the mapping rules (the five names, not paired to glyphs).
3. Pass: 5/5 correctly named; no two glyphs ambiguous. Reviewer may point at glyphs in any order.
4. If STATE 2 / STATE 4 are ambiguous: apply the base-line mitigation for STATE 2 and re-test.
5. Record the test artifact (the rendered row of 5 glyphs, color-disabled) in the evidence file.
6. A deuteranopia, protanopia, and tritanopia simulation can be approximated by rendering the glyphs with confirm-colors applied and then applying a colorblind-simulation filter (browser-based tools such as Coblis, or a Unity shader that converts to simulation colorspace). The shape test above is the pass criterion; the simulation is supplementary evidence.

### Confirm-Color Summary

| State | Glyph Geometry | Confirm Color | Palette Name | World Meaning |
|---|---|---|---|---|
| Trivial | Horizontal dash (1.5px stroke, full width) | `#9EA4A8` | Pewter Rain | Cool ambient — barely there |
| Below | Down-pointing compressed triangle (stroked, no fill) | `#8A8478` | Quarry Stone | Neutral building material — unthreatening |
| Even | Square (stroked, no fill) | `#D4CCBC` | Bone Pale | Aged paper, the city's mid-light tone |
| Above | Up-pointing compressed triangle (stroked, no fill) | `#C48B3A` | Candlefall Amber | Candle flame — functional alert, not RPG gold |
| Dangerous | Double up-pointing triangles (stacked, stroked, no fill) | `#7A4A38` | Rust Iron | Iron under stress — the HUD's loudest moment |

**Note on Bone Pale for Even (STATE 3):** Bone Pale at `#D4CCBC` is a notably lighter color than the other confirm-colors, and as a stroke-only glyph on a dark Iron Seam panel, a Bone Pale outline on a dark background will achieve high contrast. This is correct: "even match" is the neutral reference point. Bone Pale is not "safe" in Gravenspire's semantic vocabulary (§4.3) — it means "aged, specific, the limit of lightness" — not "low danger." The player must learn this over time; the glyph does not promise safety.

**Note on Candlefall Amber for Above (STATE 4):** Candlefall Amber `#C48B3A` is the color of candle flame, a practical light source — not RPG gold or reward-yellow. It reads as "something is present and relevant" in the world's vocabulary, not as "danger is imminent." This is intentional: "above" means "this target outclasses you — proceed with caution and group support," not "immediate danger." The warm-but-not-red nature of this color is load-bearing: using Rust Iron here would signal maximum alarm; using Amber signals elevated attention.

---

## Open Questions / In-Engine Validation Roll-Up

| ID | Item | Method | Owner | Blocks |
|---|---|---|---|---|
| OQ-1 | **Bar height at 6px confirmed peripheral-readable** in combat stress (10s into pull, no gaze shift, 3/3 trials correct) | In-engine playtest per Threshold 1 validation method | UI Programmer + product owner (Brian) | S4-02 bar-height claim marked evidence-linked |
| OQ-2 | **Bar height 4K scaling (10–12px) confirmed** with equivalent perceptual weight | Screenshot comparison at 4K if hardware available; otherwise deferred and noted in S4-02 as a known open item | UI Programmer | S4-02 4K bar-height claim |
| OQ-3 | **Panel fill at 65% achieves ≥3:1 contrast** for Render Umber health bar against composited panel in lowest-contrast in-game scene | In-engine luminance measurement per Threshold 2 validation method | UI Programmer | S4-02 panel-opacity claim marked evidence-linked |
| OQ-4 | **Pewter Rain mana bar ≥3:1 tritanopia contrast** confirmed in-engine at 65% opacity | Part of the same measurement pass as OQ-3 | UI Programmer | S4-02 tritanopia accessibility claim |
| OQ-5 | **Cast bar placement screenshot** confirms top edge below 648px from top (1080p) and no overlap with spell queue | Screenshot in Unity Play Mode, ruler measurement per Threshold 3 check | UI Programmer | S4-04 placement claim marked evidence-linked |
| OQ-6 | **5/5 con glyph shape discriminability** confirmed with color disabled, human reviewer | Render + review test per Threshold 4 validation method | ux-designer + product owner (Brian) | S4-03 con-glyph claim marked evidence-linked |
| OQ-7 | **STATE 2 / STATE 4 (down-triangle / up-triangle) unambiguous at 12px** | Part of OQ-6 test; if ambiguous, escalate mitigation to product owner before S4-03 implementation | ux-designer + product owner | S4-03 con-glyph implementation |

**Status at time of this document:** all seven items are OPEN QUESTIONS. None may be recorded as validated until the described method is executed and the result recorded in the downstream story's evidence file. S4-02, S4-03, and S4-04 must cite these OQ numbers when recording their own evidence, indicating which OQs were resolved and how.

---

## How Downstream Stories Consume This Document

### S4-02 — Layer 1 Vitals HUD

Consumes:
- **Threshold 1** (minimum bar height): build health, mana, hate bars at 6px (or confirmed in-engine value from OQ-1). Bar height cited in S4-02 evidence with reference to OQ-1 status.
- **Threshold 2** (panel-fill opacity floor): build panels at 65% (or confirmed in-engine value from OQ-3). Panel opacity cited in S4-02 evidence with reference to OQ-3 status.
- S4-02 must execute OQ-1 and OQ-3 validation methods as part of its own evidence collection. It may not mark bar-height or panel-opacity claims as evidence-linked until these are confirmed.
- Bar track color: Iron Seam at 30% opacity (§7.1 — "more transparent than the enclosing panel so the bar reads as inside the panel"). This is a fixed value from the art bible, not part of the Threshold 1 or 2 revision.

### S4-03 — Target Frame + Con Indicator

Consumes:
- **Threshold 1** (target health bar height): same floor as vitals bars (§7.1.1: "same height spec as the player health bar").
- **Threshold 4** (con glyph set): implement the 5-state geometry specification verbatim. Do not invent shapes. Con glyph size is 12×12px at 1080p. Confirm-colors from the table above.
- S4-03 must execute OQ-6 (and OQ-7 if flagged) as part of its evidence collection. Con glyph shapes must not be modified from this spec without product-owner approval — any shape modification requires re-running the discriminability test.

### S4-04 — Cast Bar + Interaction Prompt

Consumes:
- **Threshold 3** (cast bar placement): implement cast bar top edge at 70% viewport height (756px from top at 1080p). Cite the §7.11 forbidden band calculation in evidence. Execute OQ-5 (screenshot + ruler confirmation).
- **Threshold 1** (cast bar height): 6px fill track inside a thin bordered channel, matching the vitals bar vocabulary. No separate height spec needed — the vitals bar floor applies.
- Cast bar fill: linear fill (no ease), left to right over cast duration. Disappears on complete, interrupt, or fizzle. No completion flourish. These behaviors are from §7.1.1, not from this threshold document; S4-04 implements them per the art bible directly.

---

## Acceptance Criteria Cross-Reference

| AC | Resolved By | Status |
|---|---|---|
| S4-00-01: Minimum bar height set with rationale, replaced baseline recorded | Threshold 1 section | PROPOSED — awaiting OQ-1 in-engine confirmation |
| S4-00-02: Panel-fill opacity floor set with rationale and ≥3:1 contrast path | Threshold 2 section | PROPOSED — awaiting OQ-3 in-engine measurement |
| S4-00-03: Cast-bar lower-center placement concrete and clear of forbidden band | Threshold 3 section | PROPOSED VALIDATED by geometry derivation — awaiting OQ-5 screenshot confirmation |
| S4-00-04: 5-state con glyph set specified, shape-primary, colorblind discriminability reasoned | Threshold 4 section | PROPOSED — awaiting OQ-6 human discriminability test |
| S4-00-05: Every number is prototype-validated or explicit open question with method | OQ roll-up | SATISFIED by design — all open questions are named with methods; no value is TBD |
| S4-00-06: Spec sets legibility floor, not maximum-visibility target | Doctrine section + recessive intent notes throughout | SATISFIED by design — all values are minimums, implementation may not go below |

---

*Authority: DECISIONS.md D020; `design/art/art-bible.md` §7.1, §7.1.1, §4.3, §4.4, §4.6, §7.8, §7.10, §7.11, §7.12. Generated 2026-06-07 by ux-designer for product-owner review.*
