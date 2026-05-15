# Asset Spec — char_npc_amb_neu_feral_undead_block

**Asset id:** `char_npc_amb_neu_feral_undead_block`
**Type:** Ambient NPC (creature register, "named-in-combat-sense" — bigger / older silhouette)
**Tier:** Ambient (NOT named-NPC tier per S5.2 — see CRITICAL section below)
**Faction:** Neutral
**Maps to runtime fixture:** `NamedSoloBlock_T1` (per `design/quick/quick-design-m3-objective-npc-loot.md` and S2-M2-04 closure evidence)
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director
**Source bible refs:** S5.2 (creature vs civilian + named-NPC tier distinction), S3.1 (silhouette legibility), S2 State 3 (combat pivot)

## CRITICAL — Naming-collision warning

The runtime fixture is named `NamedSoloBlock_T1` and the M2-04 evidence calls it "the named blocker." **This name does NOT promote the asset to the bible's named-NPC tier (S5.2).** The "named" here means *combat-tier named* (a boss-like single creature with distinctive silhouette and combat behavior), not *socially-named NPC* (a character with dialogue, faction identity, schedule).

The AD-ART-BIBLE bound condition #3(b) BLOCKS named-NPC tier (S5.2) skin shader / material slot work pending the SSS POC. **This asset is at the AMBIENT tier**, uses standard PBR (no SSS), and is unblocked. If the design later promotes this entity to socially-named (e.g., revealing it as a faction-affiliated character), the spec gets re-tiered and becomes BLOCKED until SSS POC resolves.

Per the Asset Manifest: "Open art decision #2 — NamedSoloBlock narrative identity."

## Purpose

The block-tier creature is the camp's **boundary signaller**: a creature visually too large/heavy for the Cleric to solo-defeat at current power level (per S2-M2-04's FEEL-02 boundary validation — the named blocker should produce a forced-flee threshold). **The silhouette IS the warning system.** A player at 30m must read "this one is different — don't try to solo it" before committing to a pull.

## Visual Spec

- **Anatomy:** Humanoid creature, but visibly **larger and heavier** than the trash variants. ~2.1m tall (vs ~1.7m for trash), ~0.7m shoulder width (vs ~0.5m). The S3.6 rule applies — silhouettes ≤1.8× shoulder-width — and this variant approaches that ceiling without exceeding it.
- **Mass and weight:** Heavier mass distribution. Stance reads as load-bearing — the creature carries its own weight as a constant fact, where the trash variants moved with the slack of recent biological collapse.
- **Decomposition state:** **Older than the trash variants.** This creature has been undead longer; the biological surfaces show further deterioration but, critically, the structure has *settled* into its current state. The trash variants read as "recently became something terrible"; this variant reads as "has been this thing for a long time."
- **Garments:** Still below faction-legibility threshold (creature register), but the remnants are more substantial — a heavy fabric drape that has bonded with the body over decades. The remnant is unidentifiable as faction vocabulary but is recognizably *something that once was clothing.*
- **Posture:** Slow, deliberate weight-shift. The bible's S2 State 3 "pivot" applies but in slow-motion register — the named-block creature's pivot toward the player is slower than the trash variants', communicating mass.
- **Silhouette:** The 30m legibility test is the load-bearing test. At 30m, this creature must read as **larger and more dangerous than the trash variants** without any UI marker, glow, or particle effect. The bible's S2 State 3 "no particle effect signals this; the mesh rotation is the signal" applies — the silhouette and pivot pace ARE the warning.
- **Palette:** Same Bone Pale / Wick Gray desaturated band as trash. Possibly slightly more umber-warm in the deep tissue (signaling older age / more mineralization). **No faction color overlay.**
- **Forbidden:** named-NPC tier facial rig (8-12 blend shapes — blocked), skin shader SSS pass (blocked), any faction-color clothing, prominent skull/bone display, theatrical menacing pose, glowing eyes.

## Technical Spec — Ambient tier (S5.4 / S8.3 Creature enemy row)

| Attribute | Value |
|-----------|-------|
| Polygon budget | 8,000-12,000 tri (larger mesh; **slightly above** the standard ambient ceiling because larger silhouette needs more geometric detail, but **still ambient-tier**, not named-NPC) |
| Albedo | 512² PNG 8-bit (Creature enemy row — NOT the 1024² named-NPC body row) |
| Normal | 256² |
| Roughness | 256² |
| Metallic | — |
| Material count | 1-2 materials |
| Shader passes | 1: Standard PBR (NO SSS — creature row per S8.3) |
| Rigging | Standard biped + jaw-only facial rig. **No expressive face blend shapes** (named-NPC-tier facial rig is BLOCKED) |
| LODs | 3 LODs: LOD0 <10m, LOD1 (50%) 10-25m, LOD2 (25%) 25-50m, cull beyond 50m |
| Naming | Per S8.2: `char_npc_amb_neu_feral_undead_block_[lod].fbx` |

## AI Generation Prompt

> Large feral undead humanoid creature, 2.1m tall, heavy mass. Has been undead a long time — biological deterioration has stabilized, weight has settled. Body wears a heavy fabric drape that has fused with the body over decades — unidentifiable as faction clothing but recognizably "once was clothing." Posture is slow, weight-bearing, deliberate. NO bone display, NO snarl, NO theatrical menace, NO glowing eyes, NO faction colors. Skin in desaturated Bone Pale to Wick Gray band, slightly more umber-warm in deep tissue (mineralization with age). Concept art for game enemy — character sheet view (front, side, back) at 10m and at 30m camera distance to validate silhouette legibility at both ranges.

## Production Notes

- **Silhouette is the asset's load-bearing detail.** Geometry, materials, and surface authoring all serve the 30m read. If the player at 30m cannot distinguish this creature from the trash variants based on silhouette + pivot pace alone, the asset has failed.
- **Pivot pace differentiation:** the animation set must include a slower pivot variant for this creature (vs the standard trash pivot). Slower pivot is part of "this creature has been here for a long time and knows you can't hurt it quickly."
- **Mesh reuse path:** start from asset #9's mesh as a base; scale up; re-pose. The shared rig is preserved; the bulked-up silhouette is geometric work on top.
- **Promotion risk:** if M3 design later assigns this creature a faction identity / dialogue / schedule, the spec must move to named-NPC tier (S5.2 named NPC row of S8.3) and become BLOCKED. Track in asset-manifest.md "Open art decision #2."
- **30m legibility test:** the critical validation. Render both `feral_undead_trash` (asset #9) and `feral_undead_block` (this asset) side-by-side at 30m camera distance under S2 State 3 combat lighting. The player must be able to identify which is which without UI.

## Source citations

- `design/art/art-bible.md:693-704` (S5.2 named-NPC vs ambient-NPC material resolution table)
- `design/art/art-bible.md:1550` (S8.3 Creature enemy row — standard PBR only)
- `design/art/art-bible.md:117` (S2 State 3 — the pivot as encounter signal)
- `design/art/art-bible.md:346` (S3.6 silhouette ≤1.8× shoulder-width)
- `design/quick/quick-design-m3-objective-npc-loot.md` enemy types (NamedSoloBlock_T1)
- `tests/evidence/S2-M2-04/verification.md:22-24` (M2-04 named-blocker mechanical proof)
- `design/art/art-bible-t1-scope.md` Characters section (M3 enemy types at faction-baseline tier)
