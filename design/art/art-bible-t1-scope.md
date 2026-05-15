# Gravenspire Art Bible — T1 M3 Production Scope

**Version:** 1.0
**Created:** 2026-05-15
**Owned by:** art-director
**Related:** [design/art/art-bible.md](art-bible.md) (visual identity source-of-truth);
[design/quick/quick-design-m3-objective-npc-loot.md](../quick/quick-design-m3-objective-npc-loot.md)
(M3 milestone scope)
**Status:** Draft pending AD-ART-BIBLE sign-off ratification; will be promoted
to Active when the sign-off footer at `art-bible.md:12` is applied.

Resolves the T1 / T2 / T3+ scope translation of `design/art/art-bible.md` for
the Sprint 2 M3 playable surface ("Gravenspire T1: The First District").
Addresses AD-ART-BIBLE sign-off findings F-04 (S5 T1 scope), F-06 (Layer 2
phasing), F-07 (S2 State 8 fallback).

## T1 M3 ships

### Characters

- **Cleric (player)** — faction-baseline silhouette per Section 5; named-NPC-tier
  facial rig (8-12 blend shapes per Section 5.3) is **not** required at T1, since
  player camera does not see the player face. Standard humanoid rig with the
  weight-and-age material treatment per Section 6.
- **Named NPC for M3 objective frame** (working id `M3_Caretaker_T1` per the M3
  quick design at line 128): named-NPC visual register; 8-12 blend shape facial
  rig; faction-baseline silhouette (Vampire Court visual language); ambient
  stillness micro-tells per Section 5.3. Animator coordination fallback applies
  per F-07: if state-machine coordination cannot ship, surface-resolution alone
  carries the named-NPC reading.
- **Sister Elara** (per Section 7.5, post-F-05 revision): **Tier 1 templated**
  mentor only. Named-NPC visual register + behavioral logic per Section 7.5
  paragraphs 1-3. **No LLM dependency, no autonomous decision-making, no
  persistent companion state across sessions.** Full AI-companion surface
  deferred to Tier 2+.
- **Three enemy types** (per Sprint 2 plan): solo trash, linked trash, named
  blocker. Capsule placeholders → humanoid silhouettes per Section 5 at
  faction-baseline tier (not named-NPC-tier facial rig).

### Environment

- **One district**: First District / Mournwall Cemetery District (working name).
  Vampire Court visual register only.
- **Wear patterns on stone** per Section 2 State 1's load-bearing visual element.
  Mipmap target per Section 6.2 (UNVERIFIED at sign-off; unity-shader-specialist
  verdict gates final 1024² vs 2048² call).
- **Practical light sources only** per Section 6.1 / Cohesion DNA. Two-source
  gothic lighting (warm practical 2200-2600K + cool ambient 4800-5200K
  consistent with Section 2 State 1/2 specs) for camp + district.

### Layer 2 / Typography

- **Vampire Court only** at T1 M3 (per F-06 / S7.6 production note). One paper
  stock, one handwriting tradition, one seal, one mounting protocol. Other 5
  factions (Ghoul Syndicate, Cult of the Pale King, Nameless Apprentices, Hand
  of the Compact, Lichguard Remnants) deferred to T2+ as their associated zones
  ship.

### HUD (Layer 1)

- Full Section 4.4 palette implementation. Architectural register; replaces the
  current debug-IMGUI placeholder shipped through S2-M2-02.
- Corpse-run desat exemption per Section 4.4 (UNVERIFIED at sign-off;
  unity-shader-specialist verdict gates the URP camera-stack implementation
  pattern itself).

## T2+ deferrals — explicitly NOT in M3

- **Postural compression rigging** (Section 5.1 tertiary carrier + Section 5.3
  hand positions). T3+ per Section 5.5 tension 2; Court-only when it lands.
- **Dynamic garment-wear-pattern transfer** (Section 5.1 garments paragraph).
  Static variants in T1 (2-3 wear states per garment per faction). Dynamic
  texture modification deferred to T2+.
- **Per-character micro-signature behaviors at full coverage** (Section 5.3
  last subsection): T1 produces 1-2 micro-signatures for Sister Elara only;
  other named NPCs at standard ambient stillness register.
- **Additional faction Layer 2 surfaces** (Sections 7.6 / 7.7): five other
  factions deferred to T2+, phased per zone introduction.
- **Per-zone Addressables streaming groups** (Section 8.9, flag #9): T1 ships
  one playable surface; streaming groups not yet at scale.
- **Full AI-companion surface for Sister Elara** (Section 7.5 corollary about
  her departure after 3-5 sessions): T1 ships the templated mentor; the
  "she leaves" behavior + persistent state across sessions is T2+.

## T1 fallbacks — UPDATED 2026-05-15 with subagent verdicts

`technical-artist` and `unity-shader-specialist` passes ran 2026-05-15. Their
findings are folded into each fallback below; speculative "if X" language is
replaced with concrete recommendations.

- **Corpse-run desat (Sections 2 State 7 / 4.4) — CONDITIONALLY VERIFIED:**
  URP Volume Layer Mask is the documented camera-stack isolation mechanism.
  Implementation pattern: two-camera setup with Base = `GameVolumes` layer
  (carries Color Adjustments Saturation = -40); Overlay = `Nothing` /
  `UIVolumes` (no Volume override received). Run a proof-of-concept in Unity
  6.3 LTS before T1 commits to it. **If PoC fails**, ship M3 without desat
  death state; add to T2 polish backlog. Multiplayer per-player isolation
  deferred to T2 ADR per the bible's own S2 State 7 production note.

- **Ambient NPC stilling (Section 2 State 8) — T1-RISKY-NEEDS-FALLBACK:**
  Tech-artist confirms T1 mechanics are achievable via `Physics.OverlapSphere`
  proximity broadcast + Animator parameter to a "Stilled" sub-state on each
  ambient NPC's Animator Controller. The bible's lack of fallback is the risk
  vector. **Recommended fallback (requires AD approval before it can land in
  `/asset-spec`):** ambient NPCs facing away from the named entity with very
  slow procedural head turn via Animation Rigging's `MultiAimConstraint` at
  low weight. Preserves the "world noticed without celebrating" read if full
  animator coordination slips T1.

- **Mipmap bias (Sections 6.2 / 8.5) — UNVERIFIED, hardware-dependent:**
  `-0.5` bias is a reasonable starting value. **Implementation:** set via
  `TextureImporter.mipMapBias` at import time (not runtime). Enforce via
  AssetPostprocessor rule for primary facade tiles. **If `-0.5` is
  insufficient** at the locked hardware target (currently `[TO BE CONFIGURED]`
  per the F-09 governance drift), fall back to 2048² base resolution. Memory
  budget cost ~4× per affected texture set; track for Section 8.9
  streaming-group implications.

- **SSS cost model (Section 8.7) — UNVERIFIED, BLOCKING for skin shader
  authoring:** No project engine-reference file documents URP 6.3 SSS. The
  "1-2ms flat full-screen-pass" claim may be HDRP behavior misattributed to
  URP (HDRP is blocked by D001). **Three options before skin shader work
  begins** (ordered by recommended preference):
  1. Author a custom URP screen-space SSS renderer feature using
     `AddRenderPasses` + `RecordRenderGraph` (the render-graph path required
     by Unity 6.2+ deprecation of `SetupRenderPasses`). Achieves the
     flat-pass cost model the bible assumes. Non-trivial shader engineering
     investment.
  2. Use per-material SSS approximation in Shader Graph (pre-integrated skin
     LUT, separable scatter approximation). Cost scales with draw calls and
     screen-area coverage. The 15-named-NPC city-hub budget at S8.6 needs a
     new GPU time line item to absorb this.
  3. No SSS on named NPCs (Standard PBR + careful subsurface stencil only).
     Violates the named-NPC differentiation in S5.2 and the portrait-quality
     skin goal in S8.4.

  **Required action before `/asset-spec` runs skin shaders:** a URP 6.3 LTS
  SSS proof-of-concept in the actual project, profiled on the target
  hardware (or a representative GTX 1070-tier GPU), with Frame Debugger
  evidence of pass count and GPU timing. Until that evidence exists, treat
  the "flat 1-2ms" claim as UNVERIFIED and the named-NPC budget as
  provisional.

- **F-09 hardware-target governance drift (NEW 2026-05-15):**
  Both subagents flag that the bible's "tech-validated against GTX 1070 /
  RTX 4070+ / 1080p60" claim (S8.4 line 1510, S8.6 line 1539) is unsupported
  by `.claude/docs/technical-preferences.md` (which says `[TO BE CONFIGURED]`
  for all performance fields). Technical Director + producer decision needed:
  lock the hardware spec (and profile to back it) or soften the bible's
  language. Tracked separately as `art_bible_hardware_target_drift` carryover
  in `production/sprint-status.yaml`.

## Review triggers — this companion expires when

- M3 milestone closes (sprint-2 ends, M4 begins)
- Bible v2 revision lands (any structural rewrite)
- A tier-transition D-entry in `DECISIONS.md` promotes T2+ items to T1
- Tech-artist or unity-shader-specialist subagent verdicts update the fallback
  list (in flight as of 2026-05-15)
