# Asset Spec — char_npc_amb_neu_feral_undead_linked

**Asset id:** `char_npc_amb_neu_feral_undead_linked`
**Type:** Ambient NPC (creature register; pair-coupled variant)
**Tier:** Ambient (S5.2 / S5.4)
**Faction:** Neutral
**Maps to runtime fixture:** `LinkedTrash_T1` (per `design/quick/quick-design-m3-objective-npc-loot.md`; M2-03 mechanically validated linked-pull overpull)
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director
**Source bible refs:** Same as `char_npc_amb_neu_feral_undead_trash` (asset #9). This spec covers the **variant differences** only.

## Purpose

The linked-trash variant is the mechanical pair-pull risk surface from S2-M2-03. Two LinkedTrash instances spawn together and aggro as a pair (an "overpull" if the Cleric mis-times). **Visually, the linkage must be readable from 30m** so the player can identify a linked-pair pull before triggering — the bible's S3.1 silhouette legibility target at faction-recognition range applies here as combat-recognition range.

## Variant differences from asset #9 (`feral_undead_trash`)

The vast majority of geometry, texturing, rigging, and animation is reused from asset #9. **What differs:**

### 1. Binding-chain prop (separate atlas)

The pair is visually coupled by a length of iron chain between them — a tether 2-3m long from one creature's waist or wrist to the other's. This is a separate prop authored in the minor-prop atlas (`prop_min_neu_atlas_combat_01`, future spec):

- **Material:** Wrought iron, oxidized; same vocabulary as the Court lantern's bracket hardware but heavier gauge
- **Wear:** Cause-test compliant — the chain has visibly dragged through soil and stone; links show wear at the contact points
- **Length / sway:** Authored at ~2m at rest; physics-driven sway during movement. URP physics or simple cloth/rope simulation.
- **Naming:** `prop_min_neu_chain_link_01` within the minor-prop atlas

### 2. Per-individual posture differentiation

The two linked-trash share the chain but each has a slightly different posture:

- **One** carries the chain at the wrist; arm pulled slightly forward of natural rest
- **Other** carries the chain at the waist or shoulder; weight shifted to compensate for the off-balance load
- The pair's posture asymmetry is the visual proof of the binding (you can read that they're tethered before seeing the chain itself)

### 3. Optional skin discoloration at chain contact

If production budget allows, add a localized skin-darkening texture variant at the wrist/waist where the chain contacts. Cause-test compliant (iron oxidation transfer). LOW priority — drop if mesh/texture budget is tight.

## Re-used from asset #9

- **Anatomy, silhouette baseline:** identical humanoid creature register
- **Garments / lack thereof:** below faction-legibility threshold; remnant fabric only
- **Palette skew:** Bone Pale / Wick Gray desaturated band
- **Forbidden patterns:** identical (no bone display, no snarl, no green decay, no faction color)
- **Polygon budget:** 6,000-8,000 tri per instance (same as #9)
- **Textures, materials, shader:** Standard PBR, 512² albedo, 256² normal/roughness
- **Rigging:** Standard biped + jaw-only facial rig
- **LODs:** Same 3-LOD schedule

## Visual Spec — variant-specific only

- **The pair as a single visual unit:** when both linked-trash are visible, the silhouette at 30m must read as **two-creatures-tethered**, not as two-creatures-incidentally-near-each-other. The chain's curve between them is the legibility carrier.
- **Animation coupling:** when one linked-trash moves, the chain pulls on the other with a 0.2-0.5s delay (physical coupling). This animation behavior is the bible's "the pivot" applied to a coupled pair: when one aggros, the other follows by physics, not by independent decision.
- **Combat behavior visual (handoff to programming):** if the player kills one of the pair, the surviving instance's posture should shift visibly — the chain now drags freely from one side. This is the visual proof of partial success; the loop reads as "half-overpulled" rather than "two simultaneous fights."

## AI Generation Prompt

> Two feral undead humanoid creatures linked by a length of iron chain, 2-3m long. NO faction garments, NO bone display, NO snarl. Both creatures in slack posture; one carries chain at wrist, the other at waist or shoulder, weight shifted asymmetrically. Chain is wrought iron, oxidized, dragged-through wear pattern. Same biological state as solo feral undead (late decomposition, desaturated Bone Pale to Wick Gray, slate-violet undertone in deep tissue). Concept art showing the pair from 10m camera distance — the linkage must read at this range. Pre-Raphaelite restraint; no fantasy zombie tropes.

## Production Notes

- **Mesh reuse path:** import asset #9's mesh; duplicate; pose-vary the two instances at the rig level (not at mesh level). Save instance differentiation as animation state, not as separate FBX. **The two linked-trash share one mesh source.**
- **Chain prop is in the minor-prop atlas** — atlas-packed in sets of 8-16 similar props per S8.3 minor-prop budget. Authoring the atlas is a separate task; this spec just names the chain as an asset dependency.
- **Physics validation:** the chain's sway must not introduce per-frame instability or bone glitches at the rig coupling points. Test under M2-03 overpull scenario.
- **30m legibility test:** render the pair at 30m camera distance under State 1 lighting. Confirm the player can read "two creatures linked" before seeing detail.

## Source citations

- `design/art/art-bible.md:714-718` (S5.2 creature vs civilian rule — same as #9)
- `design/art/art-bible.md:117` (S2 State 3 — the pivot)
- `design/art/art-bible.md:280-285` (S3.1 silhouette legibility at 30m)
- `design/quick/quick-design-m3-objective-npc-loot.md` enemy types (LinkedTrash_T1)
- `design/art/art-bible-t1-scope.md` Characters section
