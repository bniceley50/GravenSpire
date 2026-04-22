---
name: Gravenspire Project Context
description: Core UX-relevant facts about Gravenspire — game pillars, locked art decisions, active UX gaps, and design constraints
type: project
---

Gravenspire is a small persistent gothic MMO (10-50 concurrent), Unity 6.3 LTS + URP, stylized 3D. Classic EQ combat (tab-target, spell memorization, med breaks, group dependency). PC only (Windows + Mac), keyboard/mouse primary, gamepad partial. First-time solo dev, multi-year passion project.

**Why:** This context shapes every UX decision — the EQ pacing, the anti-tutorial pillars, and the two-layer UI architecture are load-bearing constraints, not preferences.

**How to apply:** Before proposing any UX pattern, check it against the three pillars and the locked art decisions. When proposing onboarding solutions, remember the anti-tutorial direction is intentional and must be respected through diegetic means.

## Three Game Pillars
- P1: The World Is Not Your Story — world is indifferent, does not explain itself, does not celebrate player events
- P2: The Silence Is Sacred — stillness is atmosphere; motion earns its place
- P3: Reputation Is The Progression — faction standing is the core progression system; visual power scaling is misleading and wrong

## Locked Art Decisions (cannot override)
- No floating nameplates differentiating players/NPCs/companions
- No rarity colors on loot
- No colored outlines on interactable objects
- No quest markers, no map arrows, no auto-path
- No glowing emphasis on significant entities
- Two-layer UI: Layer 1 abstract HUD (health/mana/hate/spells) + Layer 2 fully diegetic (faction boards, physical notices)
- HUD exempt from corpse-run -40% desaturation (URP camera stacking)
- Dialogue system: head-and-upper-body, 4-state posture vocabulary (not full-body acting, not stills)
- Low health drains LIGHTER to Bone Pale (#D4CCBC), not to warning red
- Max-aggro hate indicator pulses at 1.5Hz in combat only (colorblind accommodation for protanopia)

## Active UX Gaps (as of Section 7 UX alignment review, 2026-04-21)
- Dialogue text display layer is unresolved — neither pure Layer 1 nor Layer 2 fits
- Player personal faction standing has no diegetic display mechanism (board shows world events, not personal standing); personal-record object proposed but not decided
- Spell icon states (active/inactive/casting/cooldown) are unspecified
- Text chat legibility treatment is absent from art bible
- Group frame hate indicators are unspecified
- Font sizes are not specified anywhere in the art bible
- Onboarding mechanism for tab-target + spell memorization + med mechanics is unresolved under anti-tutorial direction

## HUD Color Palette (Layer 1)
- Health bar: Render Umber #7A6248
- Health low (<20%): Bone Pale #D4CCBC at 75-80% opacity (drains lighter, not redder)
- Mana bar: Pewter Rain #9EA4A8
- Mana depleted (<20%): Wick Gray #5C5650
- Hate indicator: Academic blue-black #2A3040 at 50% opacity
- Hate maximum: Rust Iron #7A4A38 (the HUD's loudest moment)
- Background panel: Iron Seam #3D3A38 at 45% opacity
- Panel borders: 1px, no drop shadows, no gradients, no rounded corners
- Bar height: 3px (solo HUD spec — group frame height TBD, must pass glance-scan test)
