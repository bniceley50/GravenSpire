# Asset Spec — env_lighting_district_camp

**Asset id:** `env_lighting_district_camp`
**Type:** Lighting setup (URP Volume profile + scene lights + post-process configuration)
**Tier:** —
**Faction:** Mixed (district = Court-aligned exterior; camp = neutral interior/intimate)
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director (with technical-artist + unity-shader-specialist collaboration on URP volume implementation)
**Source bible refs:** `design/art/art-bible.md` S2 State 1 (Exploration: overcast 6000K), S2 State 2 (Camp: 2000-2400K single practical source), S4.5 (zone color-temperature rules), S6.1 (Court light-management vocabulary)

## Purpose

This is not a single mesh or texture — it's the **scene lighting configuration** that makes every other environment asset read correctly. The bible's per-state lighting specs (S2's 9 game states) are extremely specific (color temperature in Kelvin, contrast ratios, falloff specifics); the asset bundles a URP Volume profile + Light component setup + post-process configuration that implements two of those states for M3: State 1 (Exploration — the district at large) and State 2 (Camp — the named NPC's interaction point).

**Critical bound condition:** This spec assumes the AD-ART-BIBLE bound condition 3(a) PoC succeeds (camera-stack desat). The desat itself is for State 7 (Death), NOT for State 1 or 2 — but the camera-stack architecture is shared, so this spec is partially gated on the same PoC. Camera-stack architecture decisions here are the foundation for the State 7 work later.

## State 1 — District Exploration (exterior)

Per S2 State 1 spec:

- **Ambient sky / overcast:** 6000K diffuse sky dome. Skybox material with EXR 32-bit float panoramic at 1024×512 per S8.3.
- **Shadow contrast:** ~1:2 ratio. URP shadow cascade configured for compressed contrast (not the default high-contrast).
- **Practical sources (the lantern prop #7):** 2200-2600K. Asserted weakly but specifically. Tight falloff radius (3-4m per practical instance).
- **No hard directional sun.** Directional light is OFF or set to negligible intensity. The world is lit "from everywhere and nowhere; shadows exist without specific owners."

**URP setup:**
- One Global Volume in the scene with profile `gv_district_state1`
  - Color Adjustments: Saturation `-5` (very slight desaturation toward the pewter-weighted range; NOT the death-state's -40)
  - White Balance: Temperature `+10` (slight cool shift toward 6000K register)
  - Bloom: **OFF** (the bible's S2 State 1 specifies no bloom in the ambient register; bloom is acceptable only in S2 State 7 night-light readings, and even there it's clamped)
  - Vignette: **OFF** (bible-forbidden as zone-grade override per S4.8)
  - Tonemapping: Neutral (NOT cinematic / filmic — Pre-Raphaelite restraint per S1)
- One ambient light source (Light Probe network or Reflection Probe), 6000K, low intensity
- Practical light prefabs (`prop_maj_vc_lantern_practical_01`) placed deliberately at 2-3 instances per district zone

## State 2 — Camp Med-Break (named NPC interaction point)

Per S2 State 2 spec:

- **Single practical source:** 2000-2400K, owned by the camp group (lantern, candle stub, or campfire). Lighting radius is tight; faces of the party are lit; zone beyond is indeterminate.
- **Contrast ~1:3 inside the lit circle.** Higher contrast than State 1.
- **Ambient fill beyond:** 4800-5200K cool scattered light. Suggests "the larger world" without revealing it.
- **Total illuminated area:** "radius of a careful throw" (per S2 State 2: ~3-5m).

**URP setup:**
- Local Volume (trigger volume) at the camp interaction point with profile `lv_camp_state2`
  - Color Adjustments: Saturation `-10` (further desaturation; warmth dominant in the lit center)
  - White Balance: Temperature `-20` (warm shift)
  - Bloom: **OFF** (still forbidden in this register)
  - Vignette: **OFF**
- One Point Light child of the lantern/candle prop at 2200K, intensity ~6 lumens, 4m falloff
- One Ambient color override (`#4A5658` cool) for "beyond" reading
- **No party-fire spark VFX** — bible-forbidden per S1 Principle 1 (Stillness Is The Signal)

## Post-process / camera-stack architecture

This is the load-bearing technical decision per AD-ART-BIBLE bound condition 3(a):

- **Base camera:** Volume Layer Mask = `GameVolumes` layer. Receives `gv_district_state1` global volume and `lv_camp_state2` local volume when player enters trigger.
- **Overlay camera (HUD):** Volume Layer Mask = `Nothing` (or `UIVolumes` layer with no Volume placed). **HUD is exempt from all post-process per S4.4.**
- This architecture also gates the future Death state (S2 State 7) desat — the Volume Layer Mask isolation must be validated in the PoC before T1 commits.
- **Implementation note:** if the corpse-run desat PoC (separate task) lands successfully, the State 7 Volume override (Color Adjustments Saturation `-40`) lives on the same `GameVolumes` layer and toggles via script on death state entry. The Overlay HUD camera is unaffected by design.

## Technical Spec

- **Volume profile files:** Unity Volume Profile assets at `Assets/Settings/lighting/gv_district_state1.asset` and `Assets/Settings/lighting/lv_camp_state2.asset`. **Asset path note:** these live under `Assets/Settings/` in the Unity tree, not under `assets/` (which holds non-Unity assets). The spec file documents the design; the Volume Profile is authored in Unity Editor directly.
- **Light prefabs:** `prop_maj_vc_lantern_practical_01` (already specified separately) provides the Point Light component bundled with the prop.
- **Skybox / sky dome:** 1024×512 EXR panoramic. Authored offline in Substance or Blender. Even illumination at 6000K; no dramatic clouds; no sun. Per S2 State 1: "perpetual overcast is the city's weather state."
- **Forbidden:** HDRP-specific features (D001 blocks HDRP). Volumetric Light. Bloom. Vignette. Tonemap cinematic mode.

## AI Generation Prompt (sky dome only)

> Overcast Italian gothic city sky dome, 6000K diffuse, perpetual overcast. Panoramic equirectangular projection 2:1 aspect ratio. Even illumination from above (no sun direction). Pewter-weighted gray clouds; muted umber-brown at horizon. NO sun, NO stars, NO dramatic cloud formations, NO lightning. Reference: dim mid-day in northern Italy, late autumn, before rain. EXR 32-bit float for use as Unity skybox.

## Production Notes

- **Camera-stack PoC dependency:** the corpse-run desat work (separate task, bound condition 3(a)) shares this asset's camera-stack architecture. Both must be developed in coordination — the State 7 desat is just an additional Volume override on top of the State 1/2 baseline. Tech-artist + unity-shader-specialist subagent verdicts (2026-05-15) confirm the Volume Layer Mask pattern is the correct path.
- **State 2 trigger placement:** the Local Volume at the camp interaction point is a Trigger Volume — entering it activates the warm-narrow lighting register; leaving it returns to State 1 ambient. The transition should be slow (Volume `Weight` interpolation over ~1s) to avoid the bible-forbidden "jump-cut lighting between zones" (S4.8).
- **Validation:** view the district at State 1 from 30m camera distance — confirm desaturation reads as "pewter-weighted" not "drab." View the camp from 5m camera distance with the Local Volume active — confirm the named NPC's face is lit, the ambient stone beyond is indeterminate.
- **No directional sun.** This is a constant temptation to "make the scene pop." Resist it. The bible's "world is lit from everywhere and nowhere" rule is load-bearing.

## Source citations

- `design/art/art-bible.md:90-97` (S2 State 1 — Exploration full spec)
- `design/art/art-bible.md:99-108` (S2 State 2 — Camp full spec)
- `design/art/art-bible.md:481-483` (S4.4 HUD exempt from corpse-run desat — same camera-stack architecture)
- `design/art/art-bible.md:617` (S4.8 forbids jump-cut zone grading)
- `design/art/art-bible.md:617-618` (S4.8 forbids vignette / true-black ambient)
- `design/art/art-bible-t1-scope.md` HUD + Environment sections
- Unity-shader-specialist verdict 2026-05-15 (Claim A camera-stack desat): URP Volume Layer Mask is the documented mechanism
