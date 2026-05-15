# Asset Spec — ui_l1_hud_layout

**Asset id:** `ui_l1_hud_layout`
**Type:** HUD (Layer 1 — abstract practical HUD per the project's two-layer UI architecture)
**Tier:** —
**Faction:** None (HUD is the player's own perceptual state per S4.4 rationale)
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director (with ux-designer + ui-programmer collaboration on interaction details)
**Source bible refs:** `design/art/art-bible.md` S4.4 (UI Palette — Layer 1 HUD; full palette table), S7 (UI / HUD Visual Direction — entire section), S7.6 (Typography — Primary Interface Font), S7.9 (mana-restore fill / anim register), S7.11 (FORBIDDEN list — particularly relevant to HUD)

## Purpose

Replace the current **debug-IMGUI placeholder HUD** (shipped through S2-M2-02; flagged as the worst-thing in the 2026-05-12 human-play notes) with the bible's S4.4-compliant production HUD. This is the most visible single asset for the M3 playable surface — every second of gameplay frames the player's view through this HUD. If the HUD reads as "debug" rather than "game," the rest of the M3 art lift fails the "looks like a game" bar regardless of how good the environment and characters are.

**Critical bound condition:** The HUD is EXEMPT from corpse-run desaturation per S4.4 — implementation is camera-stack isolation (Overlay camera with no Volume layer mask). Per the unity-shader-specialist verdict 2026-05-15: the URP Volume Layer Mask is the documented mechanism; proof-of-concept required before T1 commits.

## Visual Spec — Element-by-element per S4.4

The bible's S4.4 specifies the HUD palette explicitly. This spec is the **production translation** of that palette into a layout authored against Unity UI Toolkit (or UGUI Canvas with the equivalent layering).

### Element inventory

| Element | Color (hex from S4.4) | Position | State logic |
|---------|----------------------|----------|-------------|
| Health bar | Render Umber `#7A6248` | Lower-left | Drains LIGHTER (Bone Pale `#D4CCBC` at <20%) and slow-pulses in combat only |
| Mana bar | Pewter Rain `#9EA4A8` | Lower-left, below health | Depletes toward Wick Gray `#5C5650` at <20% |
| Hate / Threat indicator | Academic blue-black `#2A3040` at 50% opacity | Lower-right or above health | Rust Iron `#7A4A38` at maximum (pulling aggro) |
| Background panel | Iron Seam `#3D3A38` at 45% opacity | Behind bars; architectural framing | Static |
| Spell hotbar (Cleric T1: Smite + Heal + Attack toggle + Med) | Per-spell icons in S4.4-derived per-class subset | Lower-center | Cooldown overlays per S7.9 |
| Status effect indicators | Per S4.4 icon palette | Right side, vertical stack | Hide when count = 0 |

### Layout principles per S7

- **Layer 1 register:** architectural, structural, not decorative. The bible's S7.6 typography spec — "compressed, geometric, narrow proportions consistent with pointed-arch structural logic. Working inscriptional face." Layer 1 HUD type is a single faction-neutral compressed sans (medieval lapidary inscription register).
- **No element in vertical center of screen or center horizontal band (40-60% viewport height)** per S7.11. HUD elements occupy the bottom 25% and the right 5-10% of the screen.
- **Architectural framing:** 1px borders around the bars; **NO rounded corners** (S7.11 explicit forbid — "zero tolerance, not even 1px radius"); **NO drop shadows, NO gradient fills, NO glow, NO bloom on any HUD element** (all S7.11 forbidden).
- **Animation:** all bars **snap to value** — no smooth-tween (S7.11 forbidden). The only allowed animations are:
  - Slow-pulse on low health (in combat only) per S4.4 table
  - Cooldown sweep on spell icons (per S7.9 — literal readout, no ease, no satisfying ping)
  - Med-break mana-restore 1:1 fill per S7.9 (combat-core dependency; gated)

### Typography

Per S7.6:
- **Primary font:** compressed geometric inscriptional face. Reference: medieval lapidary inscription. **Working font candidate:** Cinzel (Google Fonts; closest commercially-available match to the lapidary register; AD approval required before locking).
- **Weight hierarchy:**
  - Regular (labels, descriptors): 30-32px at 1080p, compressed proportions
  - Medium (numeric values, critical state labels like "DEAD", "PULLING"): same face, one step heavier, 34-36px
- **Italic / oblique forbidden** (S7.11).
- **Font size range: 24-36px at 1080p** (S7.11). At 4K, scale to 1.5× perceptual equivalent.

## Technical Spec

- **Framework:** Unity UI Toolkit (UXML + USS). Per `.claude/docs/technical-preferences.md` — UI Toolkit is the unity-ui-specialist's primary surface for Unity 6.3 LTS.
- **Architecture:**
  - Root UIDocument on the Overlay camera (HUD camera in the camera stack)
  - Volume Layer Mask on the Overlay camera = `Nothing` or `UIVolumes` (no Volume override applied)
  - This camera-stack isolation IS the corpse-run desat exemption mechanism (S4.4)
- **Asset files:**
  - UXML layout: `Assets/Settings/ui/hud_layout.uxml`
  - USS stylesheet: `Assets/Settings/ui/hud_styles.uss`
  - Spell icons: `Assets/Settings/ui/icons/ui_icon_spell_[slug]_64.png` (per S8.2 naming)
  - Font asset: `Assets/Settings/ui/fonts/cinzel_compressed.asset` (or AD-approved equivalent)
- **NO emissive materials, NO bloom, NO post-process effects** on any HUD element (S7.11 forbidden + camera-stack isolation enforces this).

## AI Generation Prompt (concept reference only — not asset generation)

The HUD is implementation work, not AI-generation work. AI tools can produce **concept reference images** showing the bible's intended register:

> Layer 1 HUD for a gothic RPG, architectural minimal register, 1px borders, 40-60% opacity. Health bar in muted umber (#7A6248), mana bar in pewter gray (#9EA4A8), background panel in iron seam dark (#3D3A38 at 45% opacity). NO rounded corners, NO drop shadows, NO gradient, NO glow, NO bloom. Compressed inscriptional sans typography (Cinzel or similar lapidary face). Architectural framing, NOT decorative. Reference: real architectural drafting line work, not video game UI. Pre-Raphaelite restraint.

## Production Notes

- **First implementation pass priority:** the bars (health, mana, hate). Get those reading as architectural materials inside a stone frame (per S4.4 rationale: "All bars read as architectural materials under the compressed-arch framing from Section 3.3"). Spell hotbar and status indicators are stage-2 polish.
- **Replaces the M2-02 debug-IMGUI HUD.** The current placeholder lives at... whatever the M2 controller renders via OnGUI. This new HUD operates from a UIDocument on the Overlay camera; the OnGUI debug HUD can be disabled or kept for development-only via a toggle.
- **Camera-stack PoC dependency:** before this HUD is considered "production ready," the unity-shader-specialist PoC must confirm the Volume Layer Mask isolation actually keeps the HUD out of the world-camera's Volume stack. If the HUD ends up receiving the State 7 desat by accident, the corpse-run UX breaks.
- **Accessibility:** per S4.4 / S7.10:
  - Pewter Rain mana bar vs Iron Seam background ≥3:1 luminance contrast (tritanopia)
  - 1px borders may need to scale to 1.5-2px at 4K for equivalent perceptual weight (S4.4 caveat)
  - Slow-pulse animation must have a "reduce motion" accessibility option per S7.11 caveat

## Source citations

- `design/art/art-bible.md:479-495` (S4.4 UI Palette — Layer 1 HUD table)
- `design/art/art-bible.md:1208-1219` (S7.6 Typography Primary Interface Font)
- `design/art/art-bible.md:1407-1421` (S7.11 FORBIDDEN — Layer 1 / Screen-space HUD)
- `design/art/art-bible.md:1400-1405` (S7.10 accessibility validation list)
- `design/art/art-bible-t1-scope.md` HUD section (Section 4.4 palette implementation; replaces debug-IMGUI)
- Unity-shader-specialist verdict 2026-05-15 (Claim A: URP Volume Layer Mask is documented camera-stack isolation mechanism)
- `tests/evidence/S2-M2-02/human-play-20260512.md:30-37` (M2-02 worst-thing: HUD overlay too small to read in Game view; fixed in-loop but the IMGUI register itself is the larger problem)
