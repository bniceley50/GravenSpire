# Asset Spec — prop_maj_vc_lantern_practical_01

**Asset id:** `prop_maj_vc_lantern_practical_01`
**Type:** Prop (Major) + bundled URP light source
**Tier:** —
**Faction:** Vampire Court
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director
**Source bible refs:** `design/art/art-bible.md` S2 Cohesion DNA (line 195: "light is always practical, localized, and earned"), S6.1 (Court hardware vocabulary), S2 State 1 (2200-2600K practical warm sources), S4.5 (Court palette — slate-violet accents; tarnished silver hardware)

## Purpose

The wrought-iron lantern is the **canonical practical light source** for Vampire Court zones at night and in shadowed cemetery areas. Per the bible's Cohesion DNA: "Light comes from objects that exist in the world for reasons the world has, not for reasons the player has." This lantern IS that object — its geometry AND its emitted URP light are coupled. Remove the lantern from the scene and the light goes with it. This is the bible's anti-fantasy-lighting discipline made literal.

**Why a single canonical lantern matters for T1:** the district's practical-light grammar must be consistent. One canonical Court lantern, placed deliberately, teaches the player to read Court's light-management vocabulary (S6.1 "Light management for predatory function — upper-quarter windows shuttered on a schedule that has nothing to do with weather or sleep"). Multiple competing lantern designs would dilute that grammar.

## Visual Spec

- **Material:** Wrought iron, **tarnished silver hardware** per S6.1 Court row ("silver-then-tarnished hardware replacement"). Aged 150 years. Not bright; not rusted-orange; the specific tarnished gray-brown of long-maintained but never-polished silver.
- **Form:** Court geometric precision. Lantern body is a slim vertical lancet shape echoing the Stratum-3 Court window register. Hangs from a bracket arm 60-80cm extension from wall.
- **Glass panes:** Four sides, slightly amber-tinted (the glass itself, not the light through it). Visible candle or oil-wick inside; flickering animation handled at runtime by URP Light component.
- **Wear language:**
  - Tarnish on hardware (gray-brown, not green oxide per S6.1)
  - Smoke staining at upper interior of glass panes from 150 years of candle use (S6.2 cause-test compliance)
  - Slight sag in one bracket joint where age has compressed the iron
  - Wax accumulation at base of lantern interior — solidified, multi-layered, dated
- **Forbidden:** glowing emissive material (S6.1: "Particle-dense ambient VFX" forbidden), polished silver (must be tarnished), green-oxide (wrong age vocabulary), faction-insignia engraving (faction reads through form, not logo per S6.1).

## Technical Spec — Mesh + Materials

| Channel | Resolution | Notes |
|---------|-----------|-------|
| Albedo (iron body) | 512² | PNG 8-bit; individual UV |
| Normal (iron body) | 256² | PNG 8-bit |
| Roughness (iron body) | 256² | PNG 8-bit |
| Metallic (iron body) | 128² | PNG 8-bit; wrought iron metallic value (~0.7) |
| Albedo (glass panes) | 256² | Separate material; thin film on a glass shader |
| Roughness (glass) | 128² | PNG 8-bit; mostly smooth, some surface accumulation |

- **Mesh:** FBX, ~1500 tri (body + bracket + 4 panes). LOD0 at full; LOD1 at 750 tri.
- **Naming:** Per S8.2 — `prop_maj_vc_lantern_practical_01.fbx` + texture suite.

## Technical Spec — URP Light Component

This prop is bundled with a Unity prefab containing a child Light component:

- **Type:** Point Light (radial, attenuates with distance)
- **Color:** 2400-2500K (Bible S2 State 1 practical warm range, lower end where the source is small — a single candle/wick). Use Unity Light component's color temperature mode for precision.
- **Intensity:** 4-6 lumens equivalent in URP. Tight radius (3-4m falloff). The bible's S2 State 2 specifies "tight light radius; faces are lit; the zone beyond is indeterminate" — this lantern hits that profile.
- **Shadow casting:** ON. Shadow softness: medium (for the gothic interior register).
- **Cookie / projection pattern:** optional URP light cookie depicting the lantern's pane shadow on adjacent walls — adds the Pre-Raphaelite specificity per S2 State 6 "shadow of the cathedral railing falling across the candle the cleric carries."
- **Volumetric / glow:** **OFF.** The bible explicitly forbids ambient glow/emission. The lantern's light reads through what it lights, not through a halo.
- **Flicker:** very subtle, very slow (0.5-1 Hz, 5-10% intensity range). Per S1 Principle 1 ("Stillness Is The Signal"): "A torch that flickers on a physics simulation ... whispers 'this world is attending to you.'" Restraint required. Default-OFF and only enabled if a playtest specifically requires it.

## AI Generation Prompt

> Italian gothic wrought-iron lantern, 150 years old, Vampire Court hardware. Wrought-iron body with tarnished silver bracket fittings. Slim vertical lancet form echoing gothic window register. Four amber-tinted glass panes with smoke staining at upper interior. Bracket arm extending 60-80cm from wall. Candle visible inside. NO glow effect, NO emission halo, NO sparkle. Reference: real wrought-iron palazzo wall lantern, weathered but maintained. Tarnish is gray-brown, never green-oxide, never bright silver. Smoke stain is cause-test compliant (150 years of candle use). Concept art for game prop; isolated on neutral background.

## Production Notes

- **Build the prefab as a single unit.** Mesh + materials + bundled URP Light child. When the lantern is removed from the scene, the light goes with it. This enforces the bible's practical-light rule at the engineering level, not just the art-direction level.
- **No emissive material.** The candle/wick visible inside is small enough to read as detail at distance; the URP Light handles the actual illumination. Emissive materials are S4.8-forbidden.
- **Cookie texture (optional Stage 2):** author a light cookie projecting the four-pane shadow pattern. Adds gothic specificity. Defer to Stage 2 if the M3 art baseline doesn't require it.
- **Placement guidance:** for M3 Mournwall Cemetery District: 1 at the named NPC's interaction point (camp light per S2 State 2); 1 at the cemetery gate; optional 1 at the vendor's interaction point if M3-03 needs it. **Total: 2-3 instances.** Practical-light placement is deliberate, not decorative.

## Source citations

- `design/art/art-bible.md:195` (S2 Cohesion DNA: practical-localized-earned)
- `design/art/art-bible.md:91-92` (S2 State 1: 2200-2600K practical warm sources)
- `design/art/art-bible.md:101-103` (S2 State 2: tight light radius)
- `design/art/art-bible.md:921` (S6.1 Court: silver-then-tarnished hardware)
- `design/art/art-bible.md:32-34` (S1 Principle 1: Stillness Is The Signal — restraint on flicker)
- `design/art/art-bible.md:618` (S4.8 forbidden: emissive halos)
- `design/art/art-bible-t1-scope.md` Environment section
