# Asset Spec — prop_maj_neu_grave_dust_salvage_01

**Asset id:** `prop_maj_neu_grave_dust_salvage_01`
**Type:** Prop (Major; M3 sellable salvage item)
**Tier:** —
**Faction:** Neutral (Salvage category per Inventory GDD; no faction allegiance — incidental material from the cemetery)
**Maps to runtime fixture:** `GraveDust_Salvage_T1` (per `design/quick/quick-design-m3-objective-npc-loot.md`; the M3 sellable salvage the player sells to the fixed-profile vendor)
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director
**Source bible refs:** S4.8 (FORBIDDEN: gold-as-reward; rarity colors; satisfied/cool magic VFX), S6.2 (cause test), S6.1 (Court hygiene rule — grave dust is the absence-of-Court-hygiene by definition)

## Purpose

The Salvage-category item that closes the M3 economy mini-loop. Per `design/quick/quick-design-m3-objective-npc-loot.md` — the GraveDust drops at the cemetery encounter point (or via authored placement), the player carries it back to the fixed-profile vendor, the vendor applies the F4 formula (`vendor_sell_copper = max(1, floor(nominal_value_copper * 0.15))`), and the player gets copper. This is M3's vendor-mechanism proof.

**Critical bible-compliance:** Like the relic, the GraveDust does NOT glow, does NOT shimmer, does NOT have a rarity color. Per S4.8: "No golden glow, no purple shimmer, no tier color system." The player picks up the salvage because they see it (a small container of dust at the encounter point), recognizes it as salvageable, and brings it to the vendor.

## Visual Spec

- **Object type:** A small container of grave-dust — a sealed earthenware vessel or wax-stoppered glass vial, ~10-15cm tall. The container's shape is **utilitarian, not ceremonial** — this is salvage, not a treasure. Working candidates (AD ratification needed):
  - Small earthenware jar with cloth-and-wax stopper (~12cm)
  - Wax-stoppered glass vial (~10cm)
  - Small leather pouch (~10cm; least preferred — least clearly "dust" to a viewer)
- **Material composition:**
  - Earthenware: terracotta-brown matte ceramic, fired centuries ago, surface chemistry darkening
  - OR Glass: greenish-tinted hand-blown glass (cause-test: pre-industrial glass has impurities → tint); wax stopper in beige-amber range
  - Internal content visible (dust): muted gray-brown — Wick Gray to Bone Pale range, NO bright color
- **Wear language:**
  - The container itself shows handling wear (this is the kind of object that has been used multiple times — collected, sold, refilled)
  - Where the dust contacts the container interior wall, faint staining is visible through translucent glass (if glass variant)
  - The wax stopper has been re-pressed; the press is recent (player encountered the item recently sealed)
- **Palette:** Pewter Rain / Render Umber neutral band. NO faction color (this is neutral salvage). NO gold/silver/bright accent.
- **Forbidden:**
  - Emission, glow, shimmer (S4.8 + S1)
  - Bright color in the dust itself (it's grave-dust, not "magical essence" — desaturated by definition)
  - Particle effect on pickup or carry
  - Outline / highlight beyond standard interact prompt

## Quantity / stack representation

Per `design/gdd/inventory-item-economy.md` — Salvage is stackable. For M3, the player collects 1-3 instances; the spec covers a **single instance**'s visual. Multiple in inventory may show as count text per the HUD/inventory UI; the visual asset itself is the single-instance container.

## Technical Spec — Per S8.3 Props — Major

| Channel | Resolution | Notes |
|---------|-----------|-------|
| Albedo | 512² | PNG 8-bit; individual UV |
| Normal | 256² | PNG 8-bit |
| Roughness | 256² | PNG 8-bit (smoother on glass; rougher on earthenware) |
| Metallic | — | Not used |

- **Mesh:** FBX, ~800 tri (small Major-prop). Single LOD0; the asset is rarely viewed at >5m distance so LOD1 is optional.
- **Naming:** Per S8.2 — `prop_maj_neu_grave_dust_salvage_01.fbx` + texture suite.

## AI Generation Prompt

> Small container of grave dust, ~10-15cm tall. Working candidate: small earthenware jar with cloth-and-wax stopper, OR wax-stoppered hand-blown glass vial with greenish tint from pre-industrial glass impurities. Internal content (dust) visible through container: muted gray-brown, Wick Gray to Bone Pale palette, desaturated. Wax stopper in beige-amber range, recently re-pressed. Container shows handling wear from multiple use cycles (collected, sold, refilled). NO emission, NO glow, NO shimmer, NO rarity halo, NO floating, NO outline. Utilitarian object register — this is salvage, not treasure. Pre-Raphaelite restraint. Reference: Roman / medieval Italian pharmacy vial or small alchemist's specimen jar.

## Production Notes

- **AD ratification on the container choice.** Earthenware jar vs glass vial vs leather pouch — AD reviews first-pass concept. **Glass vial preferred** (least preferred fallback: leather pouch) because it allows the muted dust contents to be visible through the container, reinforcing "this is what you sell to the vendor; this is grave dust."
- **No glow on pickup is a recurring discipline check.** Both the relic (asset #17) and the salvage (this asset) face the same temptation from default Unity / asset-store conventions. The bible's S4.8 explicit FORBIDDEN list applies to BOTH: no rarity color, no halo, no shimmer.
- **Vendor display spec note:** when the player brings the salvage to the M3 fixed-profile vendor, the vendor UI shows the salvage with its F4 sell price. The vendor UI is part of `ui_l1_hud_layout` (asset #13) extensions; this asset spec covers the in-world container only.
- **Co-asset dependency:** the GraveDust salvage's drop or placement at the cemetery encounter point is choreographed alongside the relic's placement (the relic and salvage drop / appear at the same encounter location per the M3 quick design). The two assets must visually coexist at that location without crowding each other.

## Source citations

- `design/art/art-bible.md:615-625` (S4.8 FORBIDDEN — rarity colors, glowing loot, gold-as-reward, satisfied magic VFX)
- `design/art/art-bible.md:71-72` (S1 forbids glowing loot)
- `design/gdd/inventory-item-economy.md:66-90` (Salvage category; T1 categories; F4 vendor sell formula)
- `design/quick/quick-design-m3-objective-npc-loot.md` Loot Table section (GraveDust_Salvage_T1; F4 formula = vendor_sell_copper = max(1, floor(nominal_value_copper * 0.15)))
- `design/art/art-bible-t1-scope.md` (T1 M3 scope — props at faction-baseline ambient tier)
