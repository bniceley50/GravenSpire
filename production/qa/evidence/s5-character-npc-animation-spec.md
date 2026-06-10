# S5 Character / NPC Animation Spec

> **Date**: 2026-06-09
> **Status**: DRAFT SCOPING SPEC - not yet a sprint story, not in the S5-04 closure gate
> **Purpose**: Replace the current capsule placeholders with first-pass player, hostile, and civilian bodies whose animation carries combat/readability information without violating Gravenspire's stillness and no-marker rules.
> **Source image**: User-provided Play Mode screenshot, 2026-06-09, showing the produced Sexton's Court viewed through the S5-04 camera with capsule player/NPC placeholders still dominating the read.
> **Verification method**: Source-doc line scan plus visual inspection of the provided screenshot. No Unity scene, prefab, mesh, material, animation, or manifest file was changed by this spec.

---

## 1. Scope Lock

This is a production-input spec for the next character art / animation story. It is **not**
part of the S5-01/S5-02/S5-03 produced-art manifest and should not be silently folded into
S5-04.

| Scope Claim | Source | Verification |
|---|---|---|
| S5-01 explicitly excluded character art from the produced-area manifest. | `production/stories/s5-01-art-direction-and-asset-manifest.md:48` | `rg -n "character art" production/stories/s5-01-art-direction-and-asset-manifest.md` |
| S5-02 explicitly deferred character-fidelity. | `production/stories/s5-02-perf-budget-and-asset-spec.md:51` | `rg -n "character-fidelity" production/stories/s5-02-perf-budget-and-asset-spec.md` |
| S5-03 excluded character art and any asset beyond the manifest. | `production/stories/s5-03-produced-art-production.md:58` | `rg -n "Character art" production/stories/s5-03-produced-art-production.md` |
| S5-04 is camera/HUD isolation, not character production. | `production/stories/s5-04-land-s4-01-legibility-floor.md:45-47` | Read AC list after S5-04 rebase. |
| Art bible animation rule: every animation must carry information the static state does not. | `design/art/art-bible.md:1460` | `rg -n "What information does this motion carry" design/art/art-bible.md` |

**Decision for this spec:** design character/NPC bodies and animation requirements now, but
route implementation through a new story after S5-04 closes. S5-05 can still run as the
produced-place feel gate unless the product owner decides the capsule read invalidates that
test.

---

## 2. Canon Anchors

| Character Surface | Locked Canon Input |
|---|---|
| Player Cleric | T1 class scope is Cleric only; the player begins as a pre-faction resident, not a protagonist. `design/gdd/character-creation.md:13`, `design/gdd/character-creation.md:23`, `design/gdd/character-creation.md:72-79` |
| Caretaker Morrvik | Living human, Court-licensed, not Court-affiliated; civilian-institutional register. `production/qa/evidence/s5-00-first-district-design-brief.md:22-24`, `production/qa/evidence/s5-00-first-district-design-brief.md:43`, `production/qa/evidence/s5-00-first-district-design-brief.md:51-54` |
| Court Vendor | Separate NPC, thin Court affiliation, non-political. `production/qa/evidence/s5-00-first-district-design-brief.md:45` |
| Combat Trash | Feral creatures; no Syndicate garment vocabulary. `production/qa/evidence/s5-00-first-district-design-brief.md:46` |
| Named Blocker | Combat Core expects a named solo block profile that is not normally soloable. `design/gdd/combat-core.md:808-809` |
| NPC markers | Named NPCs must read through material, face, posture, and micro-behavior, not glow/outline/nameplate/camera emphasis. `design/gdd/npc-system.md:72`, `design/gdd/npc-system.md:388-401` |

---

## 3. First-Pass Asset List

These are the minimum bodies and rigs needed to remove the current capsule read from the
play path while preserving T1 scope.

| ID | Asset | Category | Role | Budget Tier | Status |
|---|---|---|---|---|---|
| CHR-001 | `char_pc_neu_cleric_lod0.fbx` | 3D rigged character | Player Cleric body | Close-character / named-tier ceiling | Needed |
| RIG-001 | `rig_humanoid_neu_t1.blend` | Rig source | Player, Morrvik, vendor shared humanoid base | Humanoid | Needed |
| CHR-002 | `char_npc_amb_feral_ghoul_lod0.fbx` | 3D rigged character | Trash hostile | Ambient NPC tier | Needed |
| RIG-002 | `rig_feral_ghoul_neu_t1.blend` | Rig source | Trash + named blocker shared hostile base | Creature/person hostile | Needed |
| CHR-003 | `char_npc_named_blocker_ghoul_lod0.fbx` | 3D rigged character | Named blocker / boundary threat | Named NPC tier | Needed |
| CHR-004 | `char_npc_named_morrvik_lod0.fbx` | 3D rigged character | Caretaker Morrvik | Named NPC tier | Needed |
| CHR-005 | `char_npc_named_court_vendor_lod0.fbx` | 3D rigged character | Court vendor | Named-functional NPC tier | Needed |

**Reuse rule:** CHR-002 and CHR-003 should share RIG-002 unless a rig test proves the named
blocker's silhouette or attack timing cannot be produced from the trash rig. The named
blocker is a stronger read, not a boss pipeline.

---

## 4. Visual Direction

### CHR-001 - Player Cleric

The Cleric must read as a tired local resident with a job, not a chosen hero. Use undyed
linen, rough wool, worn leather, Bone Pale / Render Umber / Wick Gray neutral range, and
layered mid-length vertical emphasis. Any clerical focus, satchel, staff, or blunt implement
must be plain and occupational; no faction-primary color above the art-bible creation ceiling,
no halo, no heroic cloak, no brighter material response than nearby NPCs.

**Silhouette:** narrow vertical layers, practical hood/shoulder cloth, unheroic posture,
readable from the S5-04 camera without broadening shoulders or making a protagonist outline.

### CHR-002 - Feral Ghoul Trash

The trash hostile should read as once-human but socially erased: collapsed posture, high
shoulders, low head, long reach, damaged cloth or exposed material that does **not** imply a
faction uniform. It should not be a zombie caricature, not a Syndicate worker, and not a
glowing monster. Threat is carried by the pivot and lurch, not by color coding.

**Silhouette:** forward-hung spine, long forearms, asymmetrical gait, slack jaw or broken
neck line; 80px read should be "feral body" before "enemy type."

### CHR-003 - Named Blocker

The named blocker uses the ghoul vocabulary with more mass, less hurry, and heavier timing.
It should feel like a boundary in the space, not a miniboss presentation. No special light,
no VFX, no trophy materials. The player learns "not yet" through failed combat pressure and
the heavier combat hold, not through visual rarity.

**Silhouette:** same family as trash, but broader rib/shoulder mass, slower head tracking,
more grounded feet, and a longer windup readable enough to support flee/interrupt-pressure
learning.

### CHR-004 - Caretaker Morrvik

Morrvik is a living human under Court license, not Court faction nobility. His clothing
should be civilian-institutional: repaired dark wool, ledger strap, key cord, sleeve wear,
cheap tarnished metal, and paper/ink stains. Court precision may appear in tools and habits,
not factional costume. He must not look combat-ready.

**Silhouette:** slightly bent administrative posture; one hand often near ledger/satchel;
head-and-upper-body specificity for dialogue, no doorway-framed hero stance.

### CHR-005 - Court Vendor

The vendor is thinly Court-affiliated and non-political. Treat them as a working market
function under administrative pressure: weighing tools, folded cloth, coin pouch, practical
outer layer, small Court-licensed signifier on an object rather than garment dominance.
No combat rig requirements for this pass.

**Silhouette:** compact working posture, one hand near goods/scale, low-amplitude idle
that reads as occupational rest rather than an interactable marker.

---

## 5. Animation Set

Animation names follow art bible Section 8.2: `anim_[character-slug]_[state]_[variant].fbx`.
All animations must have a mechanical information reason. Idle motion must pass the stillness
test: if it draws attention while seen peripherally, reduce amplitude or cut it.

### Player Cleric

| Animation | Information Carried | Notes |
|---|---|---|
| `anim_pc_cleric_idle_01.fbx` | Out of combat, controllable body at rest. | Tiny breathing/weight only. |
| `anim_pc_cleric_walk_01.fbx` | Player locomotion direction and speed. | No heroic stride; grounded local resident. |
| `anim_pc_cleric_combat_pivot_01.fbx` | Player has entered/accepted combat orientation. | Fast enough to read, not flashy. |
| `anim_pc_cleric_combat_hold_01.fbx` | Attack ON / combat-ready posture. | Distinct from idle without broadening silhouette. |
| `anim_pc_cleric_auto_attack_blunt_01.fbx` | Auto-attack swing timing. | One plain blunt/staff swing; hit timing must sync to Combat Core event. |
| `anim_pc_cleric_smite_cast_01.fbx` | Smite cast request / spell action. | Quiet hand/focus action; no VFX requirement here. |
| `anim_pc_cleric_hit_react_01.fbx` | Damage landed on player. | Small interruption, no stagger theater unless mechanics require loss of action. |
| `anim_pc_cleric_medbreak_sit_01.fbx` | Sitting / med-break regen state. | Must align with the existing Sit/Stand control. |
| `anim_pc_cleric_medbreak_rise_01.fbx` | Leaving med-break. | No flourish; state transition only. |
| `anim_pc_cleric_death_01.fbx` | Player combat life state reaches dead. | Collapse is functional; no defeat pose. |

### Feral Ghoul Trash

| Animation | Information Carried | Notes |
|---|---|---|
| `anim_ghoul_trash_idle_01.fbx` | Ambient pre-pull occupation/rest. | No combat anticipation. |
| `anim_ghoul_trash_combat_pivot_01.fbx` | Body pull / hostile claim. | This is the main encounter-read moment. |
| `anim_ghoul_trash_lurch_walk_01.fbx` | Hostile movement toward target. | Unsteady but path-readable. |
| `anim_ghoul_trash_combat_hold_01.fbx` | Hostile has active threat. | Differentiates from idle. |
| `anim_ghoul_trash_melee_swipe_01.fbx` | Melee attack timing. | Contact frame must be clear to tune swing readability. |
| `anim_ghoul_trash_hit_react_01.fbx` | Hit landed on hostile. | Brief, not celebratory. |
| `anim_ghoul_trash_death_collapse_01.fbx` | Source lifecycle defeated. | Collapse clears target read without victory pose. |

### Named Blocker

| Animation | Information Carried | Notes |
|---|---|---|
| `anim_ghoul_named_idle_01.fbx` | Boundary hostile at rest. | Same family as trash, heavier stillness. |
| `anim_ghoul_named_combat_pivot_01.fbx` | Named hostile claims combat. | Slower and more deliberate than trash. |
| `anim_ghoul_named_combat_hold_01.fbx` | "This is a wall" combat posture. | Behavioral pressure, not visual rarity. |
| `anim_ghoul_named_heavy_windup_01.fbx` | Heavy attack / interrupt-pressure warning if profile supports it. | Readable enough to teach flee/interrupt without becoming a telegraph UI. |
| `anim_ghoul_named_heavy_strike_01.fbx` | Heavy attack timing. | Sync to combat result event. |
| `anim_ghoul_named_hit_react_01.fbx` | Hit landed. | Heavier recovery than trash. |
| `anim_ghoul_named_death_collapse_01.fbx` | Defeated source. | Needed even if tuning says player should usually lose. |

### Morrvik

| Animation | Information Carried | Notes |
|---|---|---|
| `anim_morrvik_idle_ledger_01.fbx` | Caretaker at work before player engagement. | Ledger/satchel micro-behavior, low amplitude. |
| `anim_morrvik_dialogue_engaged_01.fbx` | Intentional interaction is active. | Head-and-upper-body only. |
| `anim_morrvik_dialogue_cautious_01.fbx` | Civilian tension / careful answer. | Dialogue system can choose later; no gameplay logic now. |
| `anim_morrvik_dialogue_dismissive_01.fbx` | Conversation refusal/closure. | No bark/proximity trigger. |

Morrvik gets **no combat animations** in this pass. If later faction rules allow civilian
hostility, that is a new story because it changes his systemic role.

### Court Vendor

| Animation | Information Carried | Notes |
|---|---|---|
| `anim_court_vendor_idle_weighing_01.fbx` | Vendor occupation at rest. | Scale/goods hand-check; no marker loop. |
| `anim_court_vendor_trade_attention_01.fbx` | Intentional vendor interaction is active. | Small orientation shift. |
| `anim_court_vendor_dialogue_engaged_01.fbx` | Templated dialogue/vendor exchange. | Head-and-upper-body only. |

The vendor gets **no combat animations** in this pass.

---

## 6. Technical Constraints

| Constraint | Requirement |
|---|---|
| Source/export | Character mesh sources are Blender `.blend`; runtime delivery is FBX. Animations are Blender actions exported as FBX with baked keyframes. No embedded textures. See `design/art/art-bible.md:1597-1605`. |
| Naming | Characters use `char_[type]_[faction]_[role]_[state].[ext]`; animations use `anim_[character-slug]_[state]_[variant].[ext]`. See `design/art/art-bible.md:1636`, `design/art/art-bible.md:1656`. |
| Texture resolution | Named NPC body 1024^2, named face 2048^2; ambient NPC full 512^2. See `design/art/art-bible.md:1676-1678`. |
| Poly budgets | Named NPC LOD0 ceiling 16k tris; ambient NPC LOD0 ceiling 6k tris. See `design/art/art-bible.md:1708-1709`. |
| Material slots | Named NPC 4 slots, 5 max with art-director approval; ambient NPC hard limit 2. See `design/art/art-bible.md:1762-1763`. |
| LODs | Character/major prop prefabs require LODGroup; characters 4 LODs, ambient NPCs 3. See `design/art/art-bible.md:1813`. |
| Performance envelope | Keep S5 produced-area discipline: 60 fps target, 16.67 ms frame budget, <=120 additional draw calls for the sub-slice before profiling, <=64 MB new resident texture memory before profiling. See `.claude/docs/technical-preferences.md:40-45`. |
| System boundary | Combat Core owns combat state/damage/death; NPC System owns identity/persistence hooks and delegates combat decisions. See `design/gdd/npc-system.md:82`, `design/gdd/npc-system.md:410-415`. |

---

## 7. Integration Acceptance Criteria

1. **Capsule read removed**: in Play Mode, the player, trash hostile, named blocker, Morrvik,
   and vendor are no longer primitive capsules on the spawn -> Caretaker path.
2. **No new routing markers**: no glows, outlines, overhead names, target rings, rarity colors,
   or hero lighting are introduced by any character asset.
3. **Combat state is readable by motion**: body pull is readable through `combat_pivot`;
   active hostile state through `combat_hold`; swing timing through attack animation; death
   through collapse.
4. **Player remains unheroic**: the Cleric reads as a pre-faction resident at the S5-04 camera
   distance, not a protagonist silhouette.
5. **Civilian NPCs stay civilian**: Morrvik and vendor receive occupation/dialogue animation only,
   no combat-ready idles or weapon poses.
6. **No Combat Core ownership leak**: animation controllers consume Combat/NPC state; they do
   not alter combat formulas, target selection, threat, XP, persistence, or save barriers.
7. **No scene mutation during spec**: implementation story may instantiate prefabs through a
   controlled scene-edit lane; this draft spec does not serialize `_DevEntry.unity`.
8. **Preservation evidence required**: after implementation, rerun M2 preservation smokes with
   builder skipped and record perf against S5-02/S5-03 budget anchors.

---

## 8. Explicit Deferrals

- Full character creator, appearance sliders, cosmetic unlocks, portraits, or faction-choice
  visuals.
- Full faction material library for all six factions.
- Facial rig expansion beyond named-NPC budget.
- VFX for Smite, hit sparks, death, threat, rare spawn, or quest significance.
- Audio/foley hooks for attacks, ghouls, cloth, ledgers, or vendor tools.
- Companion AI, civilian hostility, vendor combat behavior, or Morrvik combat behavior.
- Addressables or streaming pipeline changes.

---

## 9. Open Questions For Story Intake

1. Does the T1 Cleric visual baseline carry a staff, a mace, both, or a neutral focus prop until
   Class Design locks equipment visuals?
2. Should the Court Vendor be produced at named-tier fidelity because the player may trade at
   close range, or ambient-tier fidelity because the vendor remains non-political and thinly
   characterized in S5?
3. Can the named blocker share the feral ghoul mesh/rig plus scale/material/timing variants, or
   does it need a distinct silhouette mesh to pass the named-blocker play read?
4. Should this land before S5-05, making the feel gate judge the court with bodies, or after
   S5-05 so the current sprint closes on the already-approved produced-place scope?
