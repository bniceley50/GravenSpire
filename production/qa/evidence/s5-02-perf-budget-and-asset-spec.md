# S5-02 Perf Budget and Asset Spec Evidence

> **Story**: S5-02: Perf-Budget Framework + Asset-Spec
> **Sprint**: Sprint 5 - First District - Designed & Produced (First-Pass)
> **Status**: SPEC CHECKPOINT ONLY - no asset generation, import, scene edit, material edit, lighting edit, or prefab edit is authorized by this file.
> **Generated**: 2026-06-08
> **Scope**: First District representative area, spawn -> Caretaker beat only.
> **Consumed manifest**: `production/qa/evidence/s5-01-art-direction-and-manifest.md`, committed locally as `19193eb docs: add S5-01 art direction evidence manifest`.

---

## 1. Source Anchors

This evidence consumes existing Gravenspire sources instead of creating a parallel art bible.

| Source | Relevant lines | Verification method |
|---|---:|---|
| D021 produced-art pivot | `DECISIONS.md:831`, `DECISIONS.md:845`, `DECISIONS.md:857`, `DECISIONS.md:865`, `DECISIONS.md:881`, `DECISIONS.md:892` | Read with `Get-Content` line ranges. |
| S5-00 scope and fences | `production/qa/evidence/s5-00-first-district-design-brief.md:7`, `:67`, `:90`, `:97`, `:110`, `:164` | Read with `rg` and line ranges. |
| S5-01 manifest | `production/qa/evidence/s5-01-art-direction-and-manifest.md:123`, `:136`, `:147`, `:163`, `:170`, `:177`, `:183`, `:196` | Read with `Get-Content` line ranges. |
| Art bible texture standards | `design/art/art-bible.md:1672`, `:1680`, `:1683`, `:1686`, `:1699` | Read with `Get-Content` line ranges. |
| Art bible polygon standards | `design/art/art-bible.md:1701`, `:1716`, `:1718`, `:1720`, `:1730` | Read with `Get-Content` line ranges. |
| Art bible import and validation | `design/art/art-bible.md:1607`, `:1640`, `:1650`, `:1733`, `:1751`, `:1755`, `:1801` | Read with `Get-Content` line ranges. |
| Technical preferences budget fields | `.claude/docs/technical-preferences.md:40` | Edited in this batch after scoped clean-status check. |

---

## 2. S5-02 Requirement Summary

S5-02 is the production gate between the S5-01 taste gate and any S5-03 asset work.

It requires:

1. A named target-hardware tier for the first-pass produced area.
2. Provisional performance budgets before production scales.
3. Per-asset technical specs for every S5-01 material set, practical source, and hero prop.
4. A sequencing rule: author one sub-slice -> profile on target hardware -> lock measured budgets -> then scale.
5. Explicit deferrals: Addressables, character-fidelity, full faction material library, VFX, rear relic/vendor production, and interiors remain out.
6. Tier 1 and URP-only discipline.

This file does not authorize production. It is a budget/spec checkpoint only.

---

## 3. Non-Negotiable Scope Fences

| Fence | S5-02 rule |
|---|---|
| Area | Spawn -> Caretaker representative area only. |
| Interiors | No produced interiors, no interior plaster requirement, no interior prop set. |
| Routing | No routing cues, objective lighting, hero-lit doors, arrows, vista framing, or path-language prop clusters. |
| Court vocabulary | Court material and form vocabulary only on the Caretaker-facing ground-floor office face. |
| Lighting | Practical-source lighting only, tied to visible fixtures or visible architectural emitters. |
| Asset count | Max 4 material families, max 4 practical light fixtures, max 5 produced hero props. |
| Deferred areas | Rear relic/vendor zones remain greybox. |
| Deferred asset classes | No characters, no VFX, no full faction material library, no Addressables work in S5-02. |

---

## 4. Named Target Hardware And Provisional Budgets

### Target Hardware Tier

| Field | Provisional S5-02 value |
|---|---|
| Target platform | PC, Windows primary, macOS secondary. |
| Engine/render pipeline | Unity 6.3 LTS, URP only. |
| Minimum-spec budget anchor | GTX 1070-class GPU at 1080p / 60 fps. |
| Target comfort anchor | RTX 4070+ class GPU for later validation headroom. |
| Confirmation point | S5-03 profiles the first produced sub-slice; these values are ceilings until measured. |

The GTX 1070 / RTX 4070+ tier is already used by the art bible polygon and draw-call sections, so S5-02 adopts it rather than inventing a new hardware tier.

### Area Budget

| Budget | Provisional ceiling |
|---|---:|
| Produced scope | Spawn -> Caretaker representative area only. |
| Produced hero props | Max 5 from the S5-01 manifest. |
| Practical light fixtures | Max 4 authored practical sources. |
| New material families | Max 4. |
| Additional draw calls for this sub-slice | Target <= 120 additional draw calls before profiling. |
| New resident texture memory for this sub-slice | Target <= 64 MB before profiling. |
| Global draw-call guardrail | <= 1,500 draw calls/frame at 1080p / 60 fps. |
| Global resident texture-memory guardrail | <= 350 MB per active zone; S5-02 does not configure Addressables. |

### Frame Budget

| Budget | Provisional ceiling |
|---|---:|
| Target framerate | 60 fps. |
| Total frame budget | 16.67 ms/frame. |
| S5 produced sub-slice increment | <= 2.0 ms additional CPU + GPU over the current greybox baseline until S5-03 profiling replaces this estimate. |

The S5 sub-slice increment is intentionally conservative. If S5-03 profiles over this ceiling, the response is to cut dressing or simplify assets, not to lower the framerate target.

---

## 5. Per-Category Production Budgets

Repo standards are stricter than the initial S5-02 placeholder in a few places, so this checkpoint uses the stricter repo limits.

### Small Prop / Hero Prop Budget

| Field | S5-02 rule |
|---|---|
| LOD0 target | 500-1,200 tris for S5 hero props, using art bible hero-prop guidance. 2,000 tris is an absolute exception cap only with explicit later approval and profiling evidence. |
| LOD1 target | 40-60 percent of LOD0 where a LOD is required. |
| Materials | Max 1 material for minor/simple props; max 2 for S5 hero props only when the second material has a physical cause. |
| Texture target | 512 albedo / 256 normal / 256 roughness for major props unless specifically approved otherwise. |
| One approved hero surface | A single 2048 unique surface may be reserved for the Caretaker threshold/entry read if S5-03 selects it; it is not granted by default. |
| Collider | Primitive or simple convex only. No mesh collider for authored props in this pass. |

### Architecture / Facade Module Budget

| Field | S5-02 rule |
|---|---|
| Primary module LOD0 | 800-1,500 tris for modular facade pieces. |
| Assembled facade span LOD0 | 4,000-8,000 tris for a 10-15m assembled city facade span. |
| Materials | Max 2-3 material slots per assembled facade span; prefer shared tiling and trim materials. |
| Texture target | 1024 max for primary facade and street/courtyard tiling sets. 2048 only for the one approved unique hero surface, if selected. |
| History treatment | Exterior history is baked into tile albedo/roughness or cheap decal meshes. No runtime exterior URP Decal Projector budget is granted in S5-02. |
| Collider | Simplified block collision only. |

### Practical Lighting Budget

| Field | S5-02 rule |
|---|---|
| Authored practical sources | Max 4 in the representative area. |
| Source requirement | Every light must be tied to a visible fixture or visible architectural emitter. |
| Temperature | Use S5-01 ranges: 2200-2400K for civic lanterns and camp fire bowl; 2400-2600K for weak Caretaker hearth/window glow. |
| Intensity | Weak warm accents only. No hero spotlights. |
| Balance | CaretakerHall must not read warmer or brighter than CourtVendorHall from spawn. |
| Routing | No light cone, placement, contrast, color, or source arrangement may indicate where the player should go. |
| Forbidden | No magical glow, glowing runes, neon, magical fog, floating crystals, rarity glow, or objective-signposting lights. |

---

## 6. Material Set Specs

| Manifest id | Asset spec | Texture budget | Naming pattern | Validation |
|---|---|---|---|---|
| `mat_01_street_cobble` | Ground surface, civic-neutral street/courtyard cobble with Quarry Stone / Pewter Rain exterior read and Iron Seam cracks. | 1024 albedo, 1024 normal, 512 roughness. No metallic. | `env_ground_cobble_courtyard_neu_[map-type].png` | Power-of-two only; sRGB albedo; Linear normal/roughness; no objective-wear path stripe; wear follows traffic and drainage cause. |
| `mat_02_caretaker_face_court_ashlar` | Primary facade material for the Caretaker-facing ground-floor office face only: dressed ashlar precision, tarnished hardware, restrained Court maintenance. | 1024 albedo, 1024 normal, 512 roughness, optional 256 metallic for hardware. Optional one 2048 unique threshold surface only if selected as the single hero surface. | `env_arch_stone_ashlar_s3_vc_yr200_[map-type].png`; optional unique surface `env_arch_uniq_caretaker_threshold_[map-type].png` | Court vocabulary cannot appear on residential/civic surfaces; no glow; no polished marble route highlight. |
| `mat_03_residential_facade_stone` | Exterior-facing residential/civic stone with soot, oxidized iron, repaired bracket scars, moss in grout lines, and living occupation wear. | 1024 albedo, 1024 normal, 512 roughness. No metallic except separate hardware if needed. | `env_arch_stone_residential_s3_lr_yr50_[map-type].png` or neutral variant if not faction-tagged by S5-03. | Must read as exterior facade stone, not interior plaster; no Court hardware; history marks require physical cause. |
| `mat_04_timber_trim` | Old timber trim, lap joints, repaired brackets, hand-height wear, and mismatched repairs. | 1024 albedo, 1024 normal, 512 roughness for trim sheet; reduce to 512 if used only on small props. | `env_arch_timber_lap_s4_neu_yr50_[map-type].png` or `env_arch_timber_lap_s4_lr_yr50_[map-type].png` if living-resistance context is explicit. | No fresh decorative beams; material slots remain within facade caps. |

Map-type uses the art bible names already implied by Section 8: `alb`, `nrm`, `rgh`, `mtl`, or packed `msk` if S5-03 chooses the mask workflow.

---

## 7. Practical Light Specs

| Manifest id | Asset spec | Mesh/material budget | Light rule | Validation |
|---|---|---|---|---|
| `light_01_civic_lantern_west` | West civic lantern instance using the shared `prop_01` lantern-post asset. | No unique fixture beyond `prop_01`; one shared material set preferred. | 2200-2400K weak warm practical; not adjacent to Morrvik; not aimed at Caretaker door. | Screenshot from spawn must show no objective framing or special cone. |
| `light_02_civic_lantern_east` | East civic lantern instance using the same shared `prop_01` asset. | Same as `light_01`; duplicated instance, not a new prop. | 2200-2400K weak warm practical; balances `light_01` so visible landmarks read equivalently. | CaretakerHall cannot become warmer/brighter than CourtVendorHall. |
| `light_03_caretaker_office_hearth_glow` | Weak visible window/hearth emitter on the Caretaker-facing office face; no produced interior. | Embedded emitter surface or simple fixture only; no room dressing. | 2400-2600K weak occupancy warmth. Remove or mirror if it becomes destination-signaling. | Must read as occupancy, not route guidance. |
| `light_04_m2_camp_fire_bowl` | Low camp fire bowl tied to existing M2 camp rest context. | Simple bowl/fixture only if produced in S5-03; no VFX granted by S5-02. | 2200-2400K tight practical. Does not merge with Caretaker-facing props. | No magical flame, fog, route signal, or objective cluster. |

---

## 8. Hero Prop Specs

| Manifest id | Asset spec | Budget | Naming | Validation |
|---|---|---|---|---|
| `prop_01_civic_lantern_post` | Oxidized iron civic lantern post with dull glass and soot-darkened cap. Shared by `light_01` and `light_02`. | 500-1,200 tris target; max 2 materials; 512/256/256 prop textures; primitive/convex collider. | `prop_maj_neu_lantern_post_01.fbx`; textures `prop_maj_neu_lantern_post_[map-type].png` | No special cone, no glow-as-signal, no placement next to Morrvik. |
| `prop_02_caretaker_dispatch_box` | Court-licensed threshold paperwork object: controlled rectangular geometry, tarnished latch, gray-blue vellum scraps. | 500-1,200 tris target; max 2 materials; 512 prop textures plus reused document/paper texture where possible. | `prop_maj_vc_dispatch_box_01.fbx`; textures `prop_maj_vc_dispatch_box_[map-type].png` | Specificity at close range only; no interactable glow; Court vocabulary only on Caretaker-facing side. |
| `prop_03_threshold_deposit_tray` | Low-profile deposit tray / threshold wear object, marble edge over older stone wear-bowl, wax/deposit residue. | 500-1,200 tris target; max 2 materials; may be the one approved 2048 unique surface only if S5-03 explicitly chooses it. | `prop_maj_vc_deposit_tray_01.fbx`; optional unique threshold surface uses `env_arch_uniq_caretaker_threshold_[map-type].png` | Must not read from spawn as an objective marker; collider simple and non-blocking. |
| `prop_04_residential_repair_bundle` | Timber offcuts, iron bracket, tied cloth, aged tool marks, placed on residential/city-fabric side. | 500-1,200 tris target if authored as one hero bundle; max 1-2 materials; prefer atlas/reuse where feasible. | `prop_maj_lr_repair_bundle_01.fbx`; textures `prop_maj_lr_repair_bundle_[map-type].png` | No Court hardware, no door-framing placement, no route arrow composition. |
| `prop_05_maintenance_cart` | Dull timber maintenance cart with oxidized iron rims and uneven civic-material load. | 500-1,200 tris target; max 2 materials; 512 prop textures; primitive/convex collider only. | `prop_maj_neu_maintenance_cart_01.fbx`; textures `prop_maj_neu_maintenance_cart_[map-type].png` | Must not block route or form a visual arrow toward the objective. |

---

## 9. Sequencing Rule

S5-03 must not attempt the full manifest at once.

Required sequence:

1. Author one sub-slice: street cobble plus one facade material on existing representative-area geometry.
2. Profile on the S5-02 target hardware tier or the closest available local proxy.
3. Capture draw calls, frame timing, texture memory, material count, and screenshot evidence.
4. Lock measured budgets or revise the ceilings downward.
5. Only then scale to the remaining material sets, practicals, and hero props.

If a measured sub-slice misses budget, the remedy is to reduce asset complexity, instance shared materials, reduce unique surfaces, or cut dressing. The remedy is not to lower target framerate, add hidden routing light, or expand scope.

---

## 10. Generator Guardrails

These guardrails are for the future AI game-art agent pipeline. They do not authorize generation now.

1. The agent may draft prompts/specs only after the request passes the S5-02 budgets.
2. The agent may not add uncapped props, variants, decals, materials, lights, VFX, or "ambient dressing."
3. The agent may not create or cite a parallel art bible.
4. The agent may not generate Court vocabulary outside the Caretaker-facing side.
5. The agent may not use glowing runes, skull spam, neon, magical fog, floating crystals, rarity glow, magical glow, or objective-signposting lights.
6. The agent may not generate interiors, rear relic/vendor assets, characters, or VFX for this sprint.
7. The agent must produce receipts for generated/imported assets in later stories.

Required future receipts:

- Source manifest id.
- Prompt and negative prompt.
- Tool/model/version and date.
- Seed or deterministic generation id when available.
- Raw output path and final import path.
- Triangle count, material count, texture dimensions, collider type, and LOD info.
- Validation result against art bible Section 8 and S5-02 budgets.
- Screenshot evidence showing no routing cue or Court-vocabulary leak.

---

## 11. Acceptance Criteria Mapping

| AC | Verdict | Evidence |
|---|---|---|
| S5-02-01 | PASS | Section 4 names the GTX 1070-class 1080p/60 minimum-spec tier with RTX 4070+ comfort anchor, inherited from art bible budget language. |
| S5-02-02 | PASS WITH NOTES | `.claude/docs/technical-preferences.md` is edited in this batch with provisional target framerate, frame budget, draw-call, and memory-ceiling values. These are explicitly pending S5-03 profiling. |
| S5-02-03 | PASS | Sections 6-8 spec every S5-01 material set, practical light, and hero prop with texture, poly, naming, import, collider, and validation rules. |
| S5-02-04 | PASS | Section 9 records the one sub-slice -> profile -> lock measured budgets -> scale sequencing rule. |
| S5-02-05 | PASS | Sections 3 and 10 explicitly defer Addressables, character fidelity, full faction material library, VFX, rear relic/vendor production, and interiors. |
| S5-02-06 | PASS | Sections 3-5 keep Tier 1, Unity 6.3 LTS, URP-only scope and practical-source lighting only. |

---

## 12. Final Gate Statement

S5-02 is complete only as a text/spec gate. It allows the project to review and approve a bounded S5-03 production batch. It does not authorize generation, import, scene application, lighting placement, material authoring, prefab work, or new asset directories.

Recommended S5-03 first batch:

1. Street cobble material on existing representative-area geometry.
2. One exterior facade material pass, preferring Caretaker-face ashlar only if the Court-vocabulary side is isolated.
3. Profile and screenshot before any hero prop or additional light fixture is produced.
