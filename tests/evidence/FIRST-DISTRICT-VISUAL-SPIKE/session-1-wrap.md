# First District Visual Spike — Session 1 Wrap Evidence

**Date:** 2026-05-16
**Branch:** `claude/first-district-visual-spike` @ `f7e5fcb` (HEAD when this wrap was authored)
**Plan ref:** [`tests/evidence/FIRST-DISTRICT-VISUAL-SPIKE/spike-plan.md`](spike-plan.md) §Engine-API Verification Status

## Purpose

Resolve the two UNVERIFIED / PARTIALLY VERIFIED engine-API gaps per the plan's Session 1 exit condition: "two UNVERIFIED engine-API gaps resolved or explicitly deferred."

Method: local sources first per user direction; external docs only as fallback (not needed this pass).

## Verification 1 — URP 17.3 fog visual behavior + Volume-stack composition

### Sources consulted (local-only)

1. `Packages/manifest.json:4` — `"com.unity.render-pipelines.universal": "17.3.0"` (declared dep)
2. `Packages/packages-lock.json:75-83` — URP 17.3.0 with transitive `com.unity.render-pipelines.core@17.3.0`, `com.unity.shadergraph@17.3.0`, `com.unity.render-pipelines.universal-config@17.0.3`
3. `Assets/Scripts/M2SingleTrashMedLoopController.cs:2083-2087` — production usage of `RenderSettings.fog = true; FogMode.Linear; start=10, end=30`
4. `Assets/DefaultVolumeProfile.asset:15` — `components: []` (existing Volume profile is empty; no current Volume overrides)
5. `docs/engine-reference/unity/modules/rendering.md` — does NOT cover fog (gap in engine reference)
6. `docs/engine-reference/unity/breaking-changes.md` — no Unity 2022 → 6.3 fog API breaking changes recorded
7. `Library/PackageCache/` — NOT PRESENT in this worktree (Unity hasn't been opened here), so URP package source not inspectable

### Findings

- **API status:** VERIFIED in use. `RenderSettings.fog` ships in M2 controller and is part of the M2-02 through M2-04 verified visual output chain.
- **Volume composition status:** Expected pipeline behavior is fog before post-process, but this was not locally source-verified in this worktree because `Library/PackageCache` is absent and engine-reference docs do not cover fog. Session 2 must empirically confirm fog + `gv_district_state1` composition before treating fog as directionally safe.
- **Empirical confirmation:** NOT YET DONE. Requires Session 2 scene experimentation — set up `gv_district_state1` + enable fog + observe composition in Game view.

### Verdict: PARTIALLY VERIFIED — API present and locally used; Volume composition explicitly deferred to Session 2 empirical check.

### Plan update needed

`spike-plan.md` API verification table row "URP 17.3 fog (visual behavior + Volume-stack interaction)" stays PARTIALLY VERIFIED; status cell updated to clarify Volume composition was NOT locally source-verified and Session 2 empirical confirmation is required.

## Verification 2 — Unity 6.3 world-space text for dev labels

### Sources consulted (local-only)

1. `Packages/manifest.json:1-13` — no `com.unity.textmeshpro` declared
2. `Packages/packages-lock.json:68` — `"com.unity.ugui": "2.0.0"` IS transitively available via `com.unity.render-pipelines.core@17.3.0` dependency chain (depth 2)
3. `Packages/packages-lock.json` (full) — no TextMeshPro anywhere
4. `Assets/**` grep for `TextMeshPro|TMPro|TextMesh\b` — zero usage (project has no existing text-mesh code)
5. `docs/engine-reference/unity/modules/ui.md:270` — covers Canvas Render Modes including World Space ("WorldSpace: UI in 3D world (e.g., floating health bars)"); UGUI scripting samples at `:212-243`; TextMeshPro pattern at `:247-259`
6. `docs/engine-reference/unity/modules/ui.md` — does NOT cover legacy `UnityEngine.TextMesh` 3D component (gap in engine reference)
7. `docs/engine-reference/unity/deprecated-apis.md:29` — UGUI `Text` component marked deprecated (replacement: TextMeshPro or UI Toolkit `Label`) but still functional in Unity 6.3
8. `docs/engine-reference/unity/breaking-changes.md:103-107` — UGUI deprecated-but-supported confirmed

### Findings — available options for world-space dev labels

| Option | Available? | New Dep? | Local source coverage | Verdict |
|---|---|---|---|---|
| **UGUI WorldSpace Canvas + UI `Text`** | Yes (UGUI transitive via `packages-lock.json:68`) | NO | YES — `modules/ui.md:270` documents WorldSpace render mode; UGUI scripting at `:212-243`; UI Text deprecated-but-supported per `deprecated-apis.md:29` | VERIFIED path |
| Legacy `UnityEngine.TextMesh` (3D component) | Built-in to UnityEngine | NO | NO local coverage (no engine-reference entry for legacy TextMesh) | Optional experiment only |
| UI Toolkit world-space via PanelSettings | Yes (`com.unity.modules.uielements` direct dep) | NO | Partial — UI Toolkit covered for screen-space; world-space PanelSettings not specifically documented in engine reference | Not chosen for spike |
| ~~TextMeshPro~~ | NOT installed | YES (`com.unity.textmeshpro`) | — | REJECTED (speculative dep per D001/governance) |

### Verified path

**UGUI WorldSpace Canvas + UI `Text`** — UGUI available transitively, WorldSpace render mode documented at `modules/ui.md:270`, UI Text component deprecated-but-supported per `deprecated-apis.md:29` and `breaking-changes.md:103-107`. Implementation = WorldSpace Canvas configured per-actor (or shared Canvas with positioning), UI Text child element, neutral gray `#808080`, hidden by default, visible only when dev toggle active.

### Optional experiment

Legacy `UnityEngine.TextMesh` may be tried first if the developer wants the lighter setup, but if it is **not source-verified** locally (no engine-reference coverage exists in this worktree) or **renders poorly** (e.g., magenta fallback from non-URP font shader), **fall back immediately to UGUI WorldSpace Canvas**. Do not invest more than one in-Editor attempt on the experiment path before falling back.

### Rejected

TextMeshPro (`com.unity.textmeshpro`) — NOT installed; would require speculative dependency per D001/governance. Must not be added for this spike.

### Session 3 implementation flow

1. Implement UGUI WorldSpace Canvas + UI Text dev-label MonoBehaviour (verified path).
2. (Optional) If developer wants to try legacy TextMesh first, single in-Editor attempt allowed; immediate fallback to step 1 if not source-verified or renders poorly.
3. Negative-scope check (per plan §Design Contract Conflict): grep `design/` for nameplate/marker policy diffs, confirm zero. Spike does not establish nameplate policy.

### Verdict: VERIFIED — dev labels can ship without new dependencies via UGUI WorldSpace Canvas + deprecated-but-supported UI Text.

### Plan update needed

`spike-plan.md` API verification table row "World-space text for dev labels (Unity 6.3)" status: UNVERIFIED → VERIFIED via UGUI WorldSpace Canvas + UI Text; TextMeshPro absent and rejected.

## Session 1 status at wrap

- ✅ Plan: written, approved (2026-05-16), committed `f7e5fcb`, pushed (`origin/claude/first-district-visual-spike` created, upstream remapped)
- ✅ Branch alignment: complete
- ✅ Spike inputs read: art-bible §S2/§S4.4/§S6/§S6.1, art-bible-t1-scope, asset-manifest, top specs, rendering.md, ui.md, breaking-changes, current-best-practices, deprecated-apis
- ✅ Design contract conflict: surfaced (nameplate prohibition), resolved (dev-only neutral labels)
- ✅ Engine-API verifications: fog PARTIALLY VERIFIED (Session 2 empirical pin); dev label VERIFIED via UGUI WorldSpace Canvas
- ⏸️ Memory saves (Git upstream gotcha + brief-is-not-design-source-of-truth): pending; out-of-tree to user's memory dir; explicit permission required before writing
- 🟢 **Session 2 unblocked** with one explicit pin: fog + Volume composition must be empirically confirmed at Session 2 lighting setup before fog can be treated as directionally safe

## Source provenance

This wrap note draws exclusively from local repository sources at this branch's `f7e5fcb` HEAD. No external doc lookups, no Context7 / WebFetch calls. If any local citation here is stale, that staleness is the same as the rest of the project state and will surface in future verification passes.
