# First District Visible-Art Spike — Plan

**Status:** Active — plan approved 2026-05-16
**Branch:** `claude/first-district-visual-spike` (this worktree)
**Base:** `origin/claude/sss-poc-bound-3b` @ `5e57cc2` (SSS PoC arc closed 2026-05-16)
**Scene file:** `Assets/Scenes/FirstDistrict_VisualSpike.unity` (sibling; preserves `_DevEntry.unity` as M2 regression scene)
**Cap:** 3-5 sessions; pivot or stop if visible-art direction needs more
**Tier:** T1 (no Tier N+1 work introduced)
**Date:** 2026-05-16

## Goal

Make `_DevEntry` feel like a place. ONE concrete playable scene at greybox fidelity demonstrating Mournwall Cemetery District (Vampire Court visual register) + two-source gothic lighting + fog/exposure mood + **temporary dev-only enemy labels behind a development toggle** + §S4.4 architectural HUD framing.

The SSS PoC arc closed bound condition #3(b) but did not move the visible-game needle; current `_DevEntry.unity` is M2-04 named-blocker capsules on a flat floor with debug-IMGUI HUD. This spike answers: **can the bible's visual direction land at greybox fidelity in URP 17.3 inside 3-5 sessions?**

## Non-Goals (per brief, verbatim)

- Generate more `/asset-spec` files (use the 18 already at `production/assets/specs/`)
- Implement named-NPC skin shaders (Option 2 Shader Graph work — separate future story)
- Production-quality polish (greybox-grade is the right fidelity)
- Multi-zone scene work (Mournwall Cemetery District only)
- Resolve F-09 (TD + Producer decision; spike treats performance as provisional)
- Touch the SSS PoC project at `N:\GravenSpire-sss-poc\` (parked; do not import code)

## Design Contract Conflict: Brief vs. Canonical Design Docs

The original brief calls for "Basic enemy nameplates over the existing M2 hostile actors." A grep of the canonical design contracts shows production nameplates are **explicitly forbidden** in multiple load-bearing locations:

- `design/art/art-bible.md:390` — "**No** floating nameplate in differentiated color for named entities..."
- `design/art/art-bible.md:392` (production test) — "If any named NPC requires a UI marker to distinguish them from ambient NPCs at 15m, the model or material has failed and must be revised"
- `design/art/art-bible.md:416` — §3.6 FORBIDS "differentiated nameplates...on interactables, significant entities, or named NPCs"
- `design/gdd/combat-core.md:71` — "should not warn the player with...nameplate color changes"
- `design/gdd/combat-core.md:565` — "**The pivot is the signal.** Combat initiation is communicated by enemy facing/stance change, not by...nameplate color"
- `design/gdd/combat-core.md:592` — Combat Core FORBIDS "nameplate color changes, aggro outlines, target rings, quest markers..."
- `design/gdd/combat-core.md:823` (AC) — Tests assert no nameplate signal appears at combat start
- `design/gdd/combat-core.md:831` (AC) — Combat Core "does not instantiate...nameplates, floating combat text, or screen-space warning elements"
- `design/gdd/npc-system.md:39` — Anti-fantasy: "Quest markers, nameplates...that identify importance"
- `design/gdd/npc-system.md:140` — NPC System "must not render markers, nameplates, recaps, or convenience UI"
- `design/gdd/npc-system.md:273` — "**No markers.** No overhead icons, nameplates, outlines, glows, rings..."
- `design/gdd/npc-system.md:286` — "No nameplates, quest markers, exclamation points..."
- `design/gdd/npc-system.md:291` — Accessibility deferred to diegetic-only ("not through marker-like affordances")
- `design/gdd/character-progression.md:665` — XP "must not appear as...nameplate effects..."

**Spike resolution:** Session 3's "enemy nameplate" deliverable becomes **temporary dev-only neutral enemy labels** scoped to:

- **Hidden by default** at scene load; visible ONLY when a development toggle is active (runtime keybind or build flag, **NOT** a HUD settings option)
- **Neutral text only** — actor id or fixture id; never importance / aggro / named-vs-trash differentiation
- **Single muted color** (e.g. `#808080` neutral gray); no warmth/cool register; no opacity changes tied to combat state
- **Isolated implementation** — dev-tool MonoBehaviour only, **NOT** routed through HUD code, NPC System code, Combat Core surface, or any future production widget system
- **Visual-indistinguishability test:** with the dev toggle OFF, the runtime is visually identical to a no-labels build

**Out of spike scope:** any production-facing nameplate policy. If user later determines accessibility/onboarding requires persistent enemy identification, that's a design decision flowing through `art-bible.md:392` production-test revision + `npc-system.md:462` open question + a new D-entry — **NOT** a spike implementation.

**Negative-scope check at Session 3 closure:** grep `design/` for any nameplate/marker/identification-policy diffs; confirm zero design-doc changes. Mirror the S2-M2-02/03 negative-scope scan discipline (active.md line 73).

## Bonus Deliverable Opportunity (narrower claim)

Per `env_lighting_district_camp.md:15`, the camera-stack architecture this spike implements (Base camera `GameVolumes` + Overlay camera `Nothing` layer mask) shares its foundation with the death-state desat work — AD-ART-BIBLE bound condition #3(a).

**Session 4 can produce evidence toward bound 3(a)** by validating that Volume Layer Mask isolation works as documented for the HUD case (Overlay camera receives no Volume override).

**Closure of bound 3(a) requires more:** an explicit desat-isolation test where a temporary `-40` Saturation Color Adjustments override is applied to the Base camera's Global Volume while confirming the Overlay HUD camera remains visually unchanged.

**Default plan position:** spike documents evidence only; explicit desat-isolation test is NOT in core deliverables. **Opt-in expansion** available at Session 4 entry (~30 min added scope); user decides at that gate. If declined, bound 3(a) closure stays open for a future PoC task.

## Visual Anchors (load-bearing references)

- **Cohesion DNA** (`art-bible.md:258`): "light is always practical, localized, and earned"
- **State 1 District lighting** (`art-bible.md:152`): overcast 6000K + 2200-2600K warm practicals, tight 3-4m falloff, NO hard directional sun
- **State 2 Camp lighting** (`art-bible.md:163`): single practical 2000-2400K, tight ~3-5m radius, 4800-5200K cool ambient beyond
- **§S4.4 HUD palette** (`art-bible.md:542-558`): Render Umber `#7A6248` health, Pewter Rain `#9EA4A8` mana, Iron Seam `#3D3A38` @ 45% panel, 1px borders, NO rounded corners
- **§6.1 Architecture** (`art-bible.md:860-942`): VC Stratum 3-4 actively maintained; strata visible from street; "organic absence" rule in Court zones

## Engine-API Verification Status (per governance §Engine Version Awareness)

| API / Feature | Status | Source | Spike Action |
|---|---|---|---|
| URP Volume system (Color Adjustments, White Balance) | VERIFIED | `rendering.md:151-166` + `env_lighting_district_camp.md:27-35` | Use as documented |
| URP Camera Stack + Volume Layer Mask isolation | CONDITIONALLY VERIFIED | shader-specialist verdict 2026-05-15 (`art-bible-t1-scope.md:91-98`); PoC required | Session 4 HUD work validates HUD case; bound 3(a) full closure requires opt-in expansion |
| URP RenderGraph for custom passes | VERIFIED | `rendering.md:19-33` | Spike avoids custom passes (greybox) |
| UI Toolkit UXML/USS on Overlay camera | VERIFIED | `ui_l1_hud_layout.md:54-57`; tech-prefs UI specialist surface | Use as documented |
| URP 17.3 fog (visual behavior + Volume-stack interaction) | PARTIALLY VERIFIED | `RenderSettings.fog` already used at `Assets/Scripts/M2SingleTrashMedLoopController.cs:2083-2087` (linear, 10-30m); URP 17.3 visual fidelity + composition with `gv_district_state1` Color Adjustments remains UNVERIFIED | Session 1 wrap: verify visual behavior + Volume composition (not API existence) |
| World-space text for dev labels (Unity 6.3) | UNVERIFIED | Brief-driven (now narrowed to dev-tool scope) | Session 3 pre-impl verification |

Per AGENTS.md §3: every "done" claim in the spike's evidence report will cite file:line + verification method.

## Session Arc (cap-enforced)

### Session 1 — Plan + Branch Alignment + Engine-API Verification (current)
- ✅ Branch aligned to `origin/claude/sss-poc-bound-3b`
- ✅ Spike inputs read (art-bible §2/§4.4/§6.1, t1-scope, manifest, lighting spec, HUD spec, rendering.md)
- ✅ Design contract conflict surfaced + resolved (nameplates → dev-only labels)
- ✅ Plan doc written with EDIT_OK
- [ ] URP 17.3 fog **visual behavior + Volume-stack interaction** verification (API existence confirmed at `Assets/Scripts/M2SingleTrashMedLoopController.cs:2083-2087`; what's UNVERIFIED is fidelity + composition with `gv_district_state1`)
- [ ] Unity 6.3 world-space text verification (for dev labels at narrowed scope)
- **Exit:** plan approved, two UNVERIFIED engine-API gaps resolved or explicitly deferred

### Session 2 — Scene Creation + Two-Source Lighting
- Create `Assets/Scenes/FirstDistrict_VisualSpike.unity` (NEW scene; copy M2 hostile spawn pattern from `_DevEntry.unity`)
- Cemetery footprint blockout: ~15×20m courtyard, Stratum 3-4 walls on 3 sides, open on 1 side toward camp interaction point
- Skybox: temporary 6000K overcast procedural (proper EXR is production-pipeline scope)
- Lights: 2-3 practical lanterns (2200K Point Lights, 4m falloff) + 1 ambient/reflection probe at 4800K. **NO directional sun.**
- URP Global Volume `gv_district_state1` (per spec): Color Adjustments Sat `-5`, White Balance Temp `+10`, Bloom OFF, Vignette OFF, Tonemap Neutral
- Camera stack scaffold: Base camera `GameVolumes` layer + Overlay camera (empty for now; populated session 4)
- **Subagent gates** at session end: art-director (lighting direction) + technical-artist (URP impl correctness)
- **Evidence:** lighting screenshot + Frame Debugger capture
- **Commit checkpoint per AGENTS.md §14**

### Session 3 — Cemetery Dressing + Dev-Only Enemy Labels
- Placeholder primitives standing in for: `env_arch_stone_ashlar_s3_vc_yr200` (walls), `env_ground_cobble_street_neu_yr200` (ground), `prop_maj_neu_gravestone_set_01` (5-8 gravestones), `prop_maj_vc_lantern_practical_01` (lantern meshes attached to Session 2 Point Lights)
- Local Volume `lv_camp_state2` at one corner (per spec): Sat `-10`, Temp `-20`, Weight interpolation ~1s
- Fog: if URP 17.3 visual behavior + Volume composition verified in Session 1, configure low-density neutral-cool for "weight and age" register; else defer with documented note
- **Temporary dev-only enemy labels** (NOT production nameplates — see §Design Contract Conflict): world-space `TextMesh` or `TextMeshPro` above existing M2 hostile actors. Neutral gray `#808080`, actor/fixture id only, hidden by default, visible only when dev toggle active. Implementation isolated to a dev-tool MonoBehaviour outside HUD / NPC System / Combat Core code paths.
- **Negative-scope check (per S2-M2-02/03 pattern):** grep `design/` for nameplate/marker/identification-policy diffs; confirm zero design-doc changes. Negative-scope scan output captured in evidence.
- **Visual-indistinguishability test:** with dev toggle OFF, scene visually identical to no-labels build.
- **M3 objective/salvage props deliberately EXCLUDED** — they belong to the M3 mechanical story (`S2-M3-02`); including them would muddy the visual-direction question.
- **Subagent gates:** art-director (dressing density) + level-designer (blockout pacing + silhouette readability + traversal)
- **Evidence:** dressed-state screenshot + dev-label toggle-on/toggle-off screenshots + negative-scope scan output
- **Commit checkpoint**

### Session 4 — HUD Framing + Performance Toggles
- Replace M2-02 debug-IMGUI HUD with §S4.4-compliant UI Toolkit HUD
- `UIDocument` on Overlay camera; files at `Assets/Settings/ui/hud_layout.uxml` + `Assets/Settings/ui/hud_styles.uss`
- First-pass elements only (per `ui_l1_hud_layout.md:74`): health bar (Render Umber), mana bar (Pewter Rain), hate indicator (Academic blue-black @ 50%), Iron Seam panel @ 45%, 1px borders, NO rounded corners, snap-to-value (no tween)
- Performance toggles (ScriptableObject + runtime keybind):
  - Fog density slider (0.0 → 0.5)
  - Light count toggle (full → reduce additional lights)
  - Post-process intensity step (Color Adjustments Sat: 0 / -5 / -10 / -20)
- **Camera-stack isolation verification:** confirm Overlay camera Volume Layer Mask = `Nothing` correctly isolates HUD from world post-process (validates HUD case of bound 3(a))
- **Optional opt-in expansion (user decision at session entry):** explicit desat-isolation test — temporarily apply `-40` Saturation override to Base camera Global Volume, confirm HUD unaffected. If accepted, closes bound 3(a) in-spike. If declined, bound 3(a) remains open.
- **Subagent gates:** unity-ui-specialist (HUD camera-stack + UXML structure) + technical-artist (perf toggle impl + fog)
- **Evidence:** HUD screenshot + toggle list + camera-stack isolation verification note (contributes toward bound 3(a); closure status depends on opt-in expansion)
- **Commit checkpoint**

### Session 5 (optional buffer) — Polish / Direction Refinement / Verification Report
- React to user + AD feedback from sessions 2-4
- If direction is durable: write `tests/evidence/FIRST-DISTRICT-VISUAL-SPIKE/spike-2026-MM-DD-verification.md` (per brief)
- If direction needs work past session 5: surface scope problem; pivot per brief's "scope or pivot" clause
- **Closure subagent gate:** reviewer + qa-tester convergent pair on the full batch
- active.md sync at closure pointing at next strategic decision

## Subagent Routing (per project memory: standing pairings)

| Session | Subagent(s) | Purpose |
|---|---|---|
| 2 | art-director, technical-artist | Lighting direction + URP impl |
| 3 | art-director, level-designer | Dressing density + blockout pacing |
| 4 | unity-ui-specialist, technical-artist | HUD impl + perf toggles + fog |
| 5 (closure) | reviewer + qa-tester | Convergent-finding pair on full batch |

Subagent briefs use absolute paths + explicit operate-from-primary-checkout instructions per memory.

## Performance Posture (F-09 unresolved — claims are provisional)

- No frame-time targets locked (F-09 hardware target = `[TO BE CONFIGURED]` per tech-prefs)
- Performance toggles ARE the deliverable so future profiling can characterize cost without rebuilding the scene
- Frame Debugger captures (`Window > Analysis > Frame Debugger` per `rendering.md:226-228`) for each session
- Provisional ceiling: total Point Light count < 8 in spike scene; URP Asset Additional Lights Per Object = default 4
- All performance claims in evidence report labeled "provisional pending F-09 resolution"

## Evidence Plan (per brief)

`tests/evidence/FIRST-DISTRICT-VISUAL-SPIKE/spike-{date}-verification.md` at closure includes:
- Scene path
- Lighting setup (color temps, intensity, falloff, count)
- Fog/exposure values (or "deferred" note if URP fog visual-behavior verification blocks)
- Asset list (placeholder primitive → spec id mapping)
- Performance toggle inventory + Frame Debugger captures
- Dressed-state screenshot
- Dev-label toggle ON/OFF screenshots + negative-scope scan output (proves no design-policy change)
- HUD camera-stack isolation verification — **contributes evidence toward AD-ART-BIBLE bound 3(a); explicit desat-isolation test required for full closure** (see §Bonus Deliverable Opportunity)

Each claim cites file:line + verification method per AGENTS.md §3.

## Carryovers / Open Items (per brief)

- F-09 hardware target unresolved → all performance claims provisional
- §S8.7 body still asserts original SSS "flat 1-2ms" claim → follow signoff footer, not unrevised body
- §S8.6 named-NPC budget revision pending → does not block environment work
- `.claude/docs/directory-structure.md` doc drift (claims active.md is gitignored; reality is tracked) → flag for separate cleanup batch
- URP 17.3 fog visual behavior + Volume-stack composition: PARTIALLY VERIFIED at session 1 start; full verification or deferral at session 1 wrap

## Scope Discipline (per memory: process calibration by batch class)

Spike sits in "gameplay-feel iteration" band — lighter implement-play-fix loop is appropriate, NOT full rigor. **Snap rigor back if:**
- HUD camera-stack isolation verification fails (becomes a tier-routing question)
- Any URP API turns out UNVERIFIED in a way demanding an ADR
- Scene work touches save/load, persistence, or cross-contract state (none expected)
- 5-session cap nears without subjective "place not capsules" verdict
- Dev-label work surfaces unexpected design-policy pressure (must escalate to design decision, not implementation)

## Commit & Push Cadence (per AGENTS.md §14)

One commit + push per approval checkpoint:
1. Plan doc approved (this gate)
2. Session 2 lighting + scene scaffolding approved
3. Session 3 dressing + dev-labels approved (with negative-scope scan in evidence)
4. Session 4 HUD + toggles approved (with bound 3(a) verdict)
5. Session 5 closure / verification report

**Push operational note:** this branch was created via `git checkout -b claude/first-district-visual-spike origin/claude/sss-poc-bound-3b`, so the tracked upstream is `origin/claude/sss-poc-bound-3b` — NOT a matching remote spike branch. First push MUST use `git push -u origin claude/first-district-visual-spike` to set the correct upstream. A plain `git push` would target the wrong remote branch.

## Active.md Sync at Closure

Update Status block + Current Task section. Point Next Skill to Run at the next strategic decision: production-asset-pipeline / another spike iteration / pivot back to M3 mechanical loop (`S2-M3-01` is the next M3 mechanical gate after `S2-M3-00` closure in `479c74e`).

## Exit Conditions

**Spike succeeds and closes cleanly** if:
- Scene reads as "place not capsules" per user/AD subjective verdict
- Two-source lighting matches §S2 State 1-2 spec at greybox fidelity
- HUD framing replaces debug-IMGUI per §S4.4 palette
- Performance toggles work and produce Frame Debugger evidence
- Camera-stack isolation has documented verification verdict (contributes to bound 3(a); closure requires the opt-in desat-isolation test)
- Dev-label work passed negative-scope check (zero design-doc diffs)
- Visual direction judged durable enough to inform Option 2 named-NPC work and production-asset-pipeline planning

**Spike pivots / re-scopes** if:
- 5 sessions land without "place not capsules" verdict
- Camera-stack isolation fails (different problem than spike was scoped for)
- URP fog requires custom render pass (out-of-scope; defer to ADR)
- Dev-label work touches production HUD/NPC/Combat Core surfaces (scope discipline violation)
- Any scope drift trigger fires
