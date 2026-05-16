# First District Visual Spike — Session 2 Evidence

**Date:** 2026-05-16 (session date; session may span calendar days — record actual run date in fields below)
**Branch:** `claude/first-district-visual-spike` @ TBD (HEAD SHA after Session 2 closing commit)
**Builder:** `Assets/Editor/GravenspireFirstDistrictVisualSpikeBuilder.cs`
**Runner:** `Assets/Editor/GravenspireFirstDistrictVisualSpikeRunner.cs`
**Scene:** `Assets/Scenes/FirstDistrict_VisualSpike.unity`
**Volume Profile:** `Assets/Settings/lighting/gv_district_state1.asset`

**Plan ref:** [`spike-plan.md`](spike-plan.md) §Session 2 (lines 99-108)
**Session 1 wrap ref:** [`session-1-wrap.md`](session-1-wrap.md) — fog Volume composition pin lives here

## Builder run

- **Method:** TBD (Unity Editor menu `Gravenspire > Visual Spike > Build First District Scene`, OR batchmode `-executeMethod Gravenspire.Editor.GravenspireFirstDistrictVisualSpikeBuilder.BuildScene`)
- **Run date / time (UTC):** TBD
- **Unity version:** TBD (expect `6000.3.14f1` per project pin)
- **Result:** TBD
- **Console output / log path:** TBD
- **Files generated:**
  - [ ] `Assets/Scenes/FirstDistrict_VisualSpike.unity`
  - [ ] `Assets/Scenes/FirstDistrict_VisualSpike.unity.meta`
  - [ ] `Assets/Settings/lighting/gv_district_state1.asset`
  - [ ] `Assets/Settings/lighting/gv_district_state1.asset.meta`
  - [ ] `Assets/Settings/lighting.meta` (if dir is new)
  - [ ] `Assets/Settings.meta` (if dir is new)

## Smoke runner

- **Method:** TBD (Unity Editor menu `Gravenspire > Visual Spike > Run Verification`, OR batchmode `-executeMethod Gravenspire.Editor.GravenspireFirstDistrictVisualSpikeRunner.RunMenu`)
- **Result:** TBD (PASS / FAIL)
- **Exit code:** TBD
- **Log path:** `tests/evidence/FIRST-DISTRICT-VISUAL-SPIKE/unity-session-2-builder-smoke-20260516.log`
- **Checks asserted:**
  - [ ] Scene loads at expected path
  - [ ] 0 Directional Lights; 3 Point Lights with `useColorTemperature=true`, `colorTemperature=2200`, `range=4`
  - [ ] 1 global Volume with `gv_district_state1.asset` profile
  - [ ] Bloom present-and-disabled OR absent
  - [ ] Vignette present-and-disabled OR absent
  - [ ] ColorAdjustments active, Saturation = -5
  - [ ] WhiteBalance active, Temperature = +10
  - [ ] Tonemapping active, mode = Neutral
  - [ ] 2 cameras; Base on Default-layer mask + Overlay on Nothing-layer mask; Overlay in Base cameraStack
  - [ ] RenderSettings.fog = true, Linear, 10-30m; RenderSettings.sun = null; Skybox assigned

## Lighting verdict — subjective (user)

- **First impression:** TBD
- **Reads as bible's "weight and age, not spectacle"?** TBD (yes / partial / no — with one-sentence reason)
- **Practical-light dominance felt (per Cohesion DNA `art-bible.md:258`)?** TBD
- **Two-source register visible (warm practical 2200K + cool ambient 4800K register)?** TBD
- **Hard directional sun read absent?** TBD (must be yes per plan §Session 2 line 103)
- **Any adjustments needed before art-director gate?** TBD

## Fog + Volume composition — empirical (RESOLVES Session 1 pin)

Per `session-1-wrap.md` Verification 1: standard URP forward order should apply fog per-fragment in opaque pass BEFORE Volume Color Adjustments post-process. Expected outcome: fog color desaturated (-5) and temp-shifted (+10) by the Volume.

- **Fog visible at expected distances (10-30m)?** TBD
- **Fog color reads desaturated/temp-shifted by Volume Color Adjustments?** TBD (expected: yes — fog tint appears slightly cooler/desaturated vs raw `fogColor`)
- **Composition unexpected or broken in any way?** TBD
- **Volume Profile contribution visible in Game view vs Volume disabled comparison test?** TBD
- **Verdict:**
  - [ ] CONFIRMED — Session 1 fog pin resolves; `spike-plan.md` fog row can move to VERIFIED at Session 2 closing sync
  - [ ] BROKEN — escalation needed; see §Scope Discipline trigger ("Snap rigor back if HUD camera-stack PoC fails")
  - [ ] NEEDS-RETUNE — values were wrong but composition logic OK; proceed with revised values at Session 3

## Screenshots

- [ ] Game view screenshot — full scene, default camera position (path: TBD)
- [ ] Game view screenshot — fog visible at distance (path: TBD)
- [ ] Frame Debugger capture per `rendering.md:226-228` — opaque pass + post-process pass order (path: TBD)
- [ ] (Optional) Volume-enabled vs Volume-disabled comparison shot for fog composition empirical proof (path: TBD)

## Subagent gates

Per spike-plan §Subagent Routing, Session 2 end gate = art-director + technical-artist.

### art-director — lighting direction review

- **Verdict:** TBD (APPROVED / NEEDS REVISION / BLOCKED)
- **Findings:** TBD
- **Direction validation against `art-bible.md` §S2 State 1/2:** TBD

### technical-artist — URP impl correctness review

- **Verdict:** TBD (APPROVED / NEEDS REVISION / BLOCKED)
- **Findings:** TBD
- **Verification of Volume Layer Mask isolation pattern:** TBD
- **Verification of fog + Volume composition:** TBD (cross-references this evidence's empirical section)

## Negative scope check

Confirm Session 2 work does not introduce design-doc changes or touch out-of-scope code surfaces.

- [ ] `git diff main --name-only design/` returns empty (no design-doc diffs)
- [ ] No changes to combat-core / NPC system / HUD code paths (`git diff main --name-only Assets/Scripts/`)
- [ ] No changes to M2 controller (`Assets/Scripts/M2SingleTrashMedLoopController.cs`)
- [ ] No changes to `_DevEntry.unity` (preserves M2 regression scene)
- [ ] No new dependencies added (`Packages/manifest.json` unchanged)
- [ ] Editor scripts contain no T1 deny-pattern terms (manual grep — pre-commit hook does NOT scan `Assets/Editor/`)

## Spike-plan updates needed (after Session 2 closure)

Based on this evidence, the following spike-plan changes should be applied at session closure:

- [ ] Fog row in API verification table: PARTIALLY VERIFIED → VERIFIED (if empirical CONFIRMED above) or ADR-escalation (if BROKEN)
- [ ] Session 2 checklist line `- [ ] Session 2 lighting + scene scaffolding approved` (line TBD) → ✅
- [ ] Reference this evidence file from spike-plan §Evidence Plan once complete

## Conclusion

- **Session 2 verdict:** TBD (COMPLETE / COMPLETE WITH NOTES / BLOCKED / PIVOT)
- **Session 3 unblocked?** TBD (yes only if fog composition CONFIRMED + art-director APPROVED + technical-artist APPROVED)
- **Carry-forwards:** TBD
- **Next session entry point:** Session 3 — Cemetery Dressing + Dev-Only Enemy Labels, OR remediation iteration if Session 2 needs revision

## Source provenance

This evidence file is generated by Session 2 of the First District Visual Spike. Builder + runner source live at the paths cited above. Fixture-of-truth for visual direction is `design/art/art-bible.md` (§S2 State 1/2, §6.1) at this branch's HEAD; the spec at `production/assets/specs/env_lighting_district_camp.md` is the implementation source-of-truth for lighting values.
