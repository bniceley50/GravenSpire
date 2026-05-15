# Asset Spec — char_npc_amb_neu_feral_undead_trash

**Asset id:** `char_npc_amb_neu_feral_undead_trash`
**Type:** Ambient NPC (creature register, not civilian)
**Tier:** Ambient (S5.2 / S5.4)
**Faction:** Neutral (creature-register; garments destroyed past faction-legibility per S5.2 hard rule)
**Maps to runtime fixture:** `SoloTrash_EvenCon_T1` (per `design/quick/quick-design-m3-objective-npc-loot.md`)
**Status:** SPEC drafted 2026-05-15
**Owned by:** art-director
**Source bible refs:** `design/art/art-bible.md` S5.2 (Creature vs Civilian rule: "Garments are civilization"), S5.4 (Ambient NPC budgets), S8.3 (Creature enemy row), S3.1-3.3 (shape language)

## Purpose

The solo-trash enemy is the **mechanically simplest** combat target — a target the Cleric pulls, fights, defeats, and resets the loop on. **Visually it must read as a creature, not a person.** Per S5.2 "Garments Are Civilization": below the faction-recognition threshold, the character is in creature-read territory regardless of undead biology. The solo-trash exists outside Court hygiene, beyond any faction's social embrace — possibly an early-Court-era undead that lost its placement, or a feral haunt-creature with no political affiliation it can remember.

**Critical bible compliance:** This is NOT a named-NPC. The S5.2 distinction matters — named NPCs are blocked behind the SSS POC; ambient creatures are not. Make sure the silhouette reads as creature, not as faction-dressed undead person.

## Visual Spec

- **Anatomy:** Humanoid undead, biological forms partially exposed. Skin in late-decomposition state but not skeletal (per S3.6: no bone iconography as primary signaling).
- **Garments:** **Below faction-legibility threshold.** Whatever clothing exists is shredded, soiled, or absent. NO visible Court vocabulary; NO Resistance hand-of-the-compact iconography; NO faction-indicating cut or material. **The lack of garments IS the visual statement.**
- **Posture:** No social posture. No "ready stance," no theatrical menace. The bible's S5.3 idle rule applies inverted — instead of "what does a person look like in non-activity," this is **what does a creature look like when nothing has its attention?** Slack, slightly hunched, weight unevenly distributed. The opposite of occupational poise.
- **Silhouette:** Baseline humanoid (~1.7m tall, ~0.5m shoulder). The S3.6 silhouette rule applies (silhouettes ≤1.8× shoulder-width of equivalent-height human in idle pose). NO apex landmark elements (no spikes, no horns, no isolated extreme features).
- **The pivot (S2 State 3):** "The moment an enemy mesh rotates from facing-away (set dressing) to facing-toward (encounter). No particle effect signals this. The mesh rotation is the signal." When the player triggers a pull, the solo-trash rotates 180° to face the player. This must read as the encounter-begin moment without any VFX accompanying it.
- **Wear language:**
  - Mottled skin discoloration (the cause-test applies: each patch has a physical cause — bruise-source, decomposition pattern, etc.)
  - Hair matted, not stylized
  - Hands and fingers carry the most visible decay (most-exposed, most-handled)
- **Palette:** Desaturated. Skin tone shifts toward Bone Pale `#D4CCBC` / Wick Gray `#5C5650` per S4 palette. Slight Court slate-violet undertone in deep tissue (because this creature was likely Court-aligned once, biologically retains that mineral chemistry).
- **Forbidden:** prominent skull/bone display (S3.6), full-saturation biological color (red blood at biological accuracy only — see S4.8: "Red is biological. Blood, wound, tissue at biological accuracy. Not enemy highlights"), faction-color clothing.

## Technical Spec — Per S5.4 / S8.3 Ambient NPC budget

| Attribute | Value |
|-----------|-------|
| Polygon budget | 6,000-8,000 tri (body + remnant clothing) |
| Albedo | 512² PNG 8-bit |
| Normal | 256² PNG 8-bit |
| Roughness | 256² PNG 8-bit |
| Metallic | — (not used) |
| Material count | 1-2 materials (skin + clothing-remnant or skin-only) |
| Shader passes | 1: Standard PBR (NO SSS — named-NPC SSS pass is BLOCKED; creature-enemy row in S8.3 confirms standard PBR only) |
| Rigging | Standard biped. 1 idle blend, 1 walk, 1 combat. **Jaw only facial rig** (no expression blend shapes for ambient tier) |
| LODs | 3 LODs: LOD0 <10m (full), LOD1 (50% tri) 10-25m, LOD2 (25% tri) 25-50m, cull beyond 50m |
| Naming | Per S8.2: `char_npc_amb_neu_feral_undead_trash_[lod].fbx`, textures `char_npc_amb_neu_feral_undead_trash_[map].png` |

## AI Generation Prompt

> Feral undead humanoid creature, no faction affiliation, garments destroyed past faction-legibility. Late-decomposition biological state but NOT skeletal. Shredded remnants of unidentifiable clothing — no Court vocabulary, no symbol, no insignia. Slack posture, weight unevenly distributed, no readiness stance. Hands and fingers carry the most visible decay. Skin tone in desaturated Bone Pale to Wick Gray range with slight slate-violet undertone in deep tissue. Concept art for game enemy, character sheet view (front + side + back). Reference: Italian Renaissance painting of bodily mortification (Mantegna anatomy sensibility), NOT fantasy zombie horror.

## Production Notes

- **AI generation default failure mode:** "feral undead" prompts default to fantasy-zombie tropes (rotting flesh in saturated greens, lurching pose, exposed bone, snarling face). Reject all of these. Re-prompt explicitly: "Pre-Raphaelite restraint, desaturated, no bone display, no snarl, no green decay."
- **The pivot moment (S2 State 3):** during runtime, the solo-trash idle animation is facing-away; the encounter-begin trigger rotates the mesh 180° to face the player. The model and rig must support a clean turn-around with weight-shift consistent with "biological agent reorienting toward perceived threat."
- **Co-spec note:** `char_npc_amb_neu_feral_undead_linked` (asset #10) reuses 80%+ of this mesh and texture set — the linked variant is differentiated through animation coupling and possibly a simple binding-chain prop, not through unique geometry.
- **Bible-compliance critical check:** before locking the concept, run the test from S5.2: "is this character's garment vocabulary at or above the lowest faction recognition threshold?" If yes, the design has accidentally made the trash mob read as a civilian and needs revision. The answer must be NO.

## Source citations

- `design/art/art-bible.md:714-718` (S5.2 creature vs civilian — "Garments are civilization")
- `design/art/art-bible.md:117` (S2 State 3 — the pivot)
- `design/art/art-bible.md:692-704` (S5.4 ambient NPC budget table)
- `design/art/art-bible.md:1550` (S8.3 Creature enemy row)
- `design/art/art-bible.md:76` (S1 forbids skull/bone signaling)
- `design/quick/quick-design-m3-objective-npc-loot.md` enemy types
- `design/art/art-bible-t1-scope.md` Characters section
