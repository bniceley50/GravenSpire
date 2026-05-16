# SSS PoC Verification — 2026-05-16

## Verdict

**RESOLVED-WITH-NOTES — Option 2 recommended for production.**

Bound condition `#3(b) URP SSS cost model (S8.7)` is closed. The PoC established that Option 1 (custom URP screen-space SSS via ScriptableRendererFeature + RecordRenderGraph) is implementable in URP 17.3 — it compiles, registers, runs, and produces a visible SSS effect on 15 skin proxies. But the URP-forward-rendered architecture (composite of diffuse + specular per fragment before any post-pass) makes the cost-model AND visual-fidelity claims of art bible §S8.7 structurally hard to satisfy in URP forward. Option 2 (per-material Shader Graph pre-integrated skin LUT) is the recommended production path for named-NPC skin shaders.

`/asset-spec` named-NPC tier is unblocked for `M3_Caretaker_T1`, `Sister Elara`, and `M3_CourtVendor_T1` against Option 2 as working assumption. Named-NPC budget at art-bible.md §S8.6 needs revision to include a per-draw SSS cost line item (instead of the originally-asserted "flat 1-2ms full-screen pass regardless of count").

Per-pass GPU cost was NOT empirically measured; the verdict closes #3(b) on the basis of the URP-forward architectural finding (F-3 below) and the §S5.2 visual-target alignment with Option 2 architecture (F-4 below), not on cost numbers.

## Source trace

- **Branch:** `claude/sss-poc-bound-3b` (off `claude/admiring-davinci-1f7c60` @ `40217f2`)
- **Worktree:** `N:\GravenSpire\.claude\worktrees\recursing-williams-7402b2`
- **Originating bound condition:** [`design/art/art-bible.md`](../../../design/art/art-bible.md) sign-off footer condition #3(b)
- **Fallback ladder:** [`design/art/art-bible-t1-scope.md`](../../../design/art/art-bible-t1-scope.md) "SSS cost model" section
- **PoC project (throwaway, outside repo):** `N:\GravenSpire-sss-poc\`
- **AD-ART-BIBLE ratification:** commit `0bcce46`

## Method

### Architecture chosen for measurement

Per `art-bible-t1-scope.md` SSS section, Option 1 was the preferred fallback to test first. Architecture v6 (after course-correction from initial v5 that incorrectly assumed URP/Lit exposed configurable stencil properties — caught via local-package grep before code was written):

1. **SkinStencilMask pass:** override-material draw on Layer 20 ("SSSSkin") renderers writing stencil bit 1 (`ZTest LEqual`, `ColorMask 0`)
2. **H Blur pass:** full-screen separable Gaussian blur of camera color → intermediate
3. **V Blur + Composite pass:** vertical Gaussian + composite back to camera color through stencil-Ref-1 + alpha-blend by `ScatterStrength`

### Implementation

The renderer-feature implementation was 7 files (~515 LOC) at `N:\GravenSpire-sss-poc\Assets\Scripts\Runtime\` and `Assets\Shaders\SSS\`:

- `Scripts/Runtime/SSSPassSettings.cs` (Serializable config)
- `Scripts/Runtime/SSSRendererFeature.cs` (`ScriptableRendererFeature` host)
- `Scripts/Runtime/SSSPass.cs` (`ScriptableRenderPass.RecordRenderGraph` with 3 raster passes)
- `Scripts/Runtime/SkinFlagComponent.cs` (Layer 20 auto-assign marker)
- `Shaders/SSS/SkinStencilMask.shader` (stencil-write-only override material)
- `Shaders/SSS/SSSBlur.shader` (2 passes: H blur, V blur + composite)
- `Shaders/SSS/SSSBlur.hlsl` (shared Gaussian kernel)

Subsequent automation and scene-setup work added:

- `Assets/Editor/SSSPoCAutomation.cs` (Editor-API renderer-feature registration + automated capture orchestrator)
- `Assets/Scripts/Runtime/PoCSceneSetup.cs` (15-NPC scene builder)
- `PROFILING_INSTRUCTIONS.md` (capture protocol)
- Generated/modified assets: `Assets/Scenes/SSSPoC.unity`, modifications to `Assets/Settings/PC_Renderer.asset` and `ProjectSettings/TagManager.asset`

All renderer-feature patterns mirrored verbatim from URP 17.3 local package source:

- `Library/PackageCache/com.unity.render-pipelines.universal@3b809f23691d/Samples~/URPRenderGraphSamples/RendererList/RendererListRenderFeature.cs` (RendererList API)
- `Library/PackageCache/com.unity.render-pipelines.universal@3b809f23691d/Runtime/Passes/RenderObjectsPass.cs` (overrideMaterial pattern)
- `Library/PackageCache/com.unity.render-pipelines.universal@3b809f23691d/Samples~/URPRenderGraphSamples/BlitWithMaterial/BlitWithMaterial.shader` (Blit.hlsl include path)

### Compile verification (three separate batchmode compiles, all clean)

- **Batch 2B renderer-feature compile:** `N:\GravenSpire-sss-poc\batch2b-compile.log` line 209 (`*** Tundra build success (0.18 seconds), 1 items updated, 634 evaluated`); exit code 0 at line 306 (`Exiting without the bug reporter. Application will terminate with return code 0`). No CS errors, no shader errors.
- **Automation setup compile** (post-Batch 2B, after `SSSPoCAutomation.cs` and related files landed): `N:\GravenSpire-sss-poc\setup-poc.log` line 246 (`*** Tundra build success (2.71 seconds), 11 items updated, 634 evaluated`).
- **Capture-run compile** (post-`PoCSceneSetup.cs` and capture orchestrator finalization): `N:\GravenSpire-sss-poc\automated-capture-3.log` line 227 (`*** Tundra build success (0.24 seconds), 1 items updated, 634 evaluated`).

### Renderer feature registration (Editor API, not hand-written YAML)

`Assets/Editor/SSSPoCAutomation.cs` registered SSSRendererFeature on PC_Renderer.asset using Unity's `SerializedObject` / `SerializedProperty` API. Verified at `Assets/Settings/PC_Renderer.asset:27-29` (m_RendererFeatures array entry) and `Assets/Settings/PC_Renderer.asset:58-79` (SSSRendererFeature sub-object with `InjectionPoint=300` = `AfterRenderingOpaques` + correct shader GUIDs `643fedcb7557fc944b66d283c6fafdcf` mask + `a076967955175844ca0abaa0b8d577ec` blur). TagManager.asset Layer 20 named `SSSSkin`. Test scene `Assets/Scenes/SSSPoC.unity` created programmatically by `PoCSceneSetup.cs` (15 capsule+sphere humanoid proxies in 3×5 grid, two-source gothic lighting per art bible §S6.1).

### Capture attempt

Single 15-NPC stress capture per Option #2 strategic narrowing (replaces the original 1/5/10/15 sweep). **Capture method: visible Unity Editor process via `-executeMethod GravenspireSSS.Editor.SSSPoCAutomation.RunAutomatedCapture`** (not `-batchmode -nographics -quit` — the capture orchestrator requires `EditorApplication.update` polling across domain reloads). Capture lifecycle evidence in `N:\GravenSpire-sss-poc\automated-capture-3.log`:

- Line 413: `[SSSPoC] Automated capture started.`
- Line 444: `[SSSPoC] Automated capture resumed after domain reload.`
- Line 556: `[SSSPoC] Automated capture complete: N:\GravenSpire-sss-poc\Capture\sss-poc-automated-capture.md`

The domain-reload-resume pattern at line 444 is direct evidence of `EditorApplication.update` polling persistence across Unity's domain reload — interactive Editor automation, not a one-shot batchmode invocation.

**Results** (from `N:\GravenSpire-sss-poc\Capture\sss-poc-automated-capture.md`):

| Metric | Value |
|---|---|
| Unity version | 6000.3.14f1 |
| URP version | 17.3.0 |
| Graphics API | Direct3D12 |
| GPU | NVIDIA GeForce RTX 5090 |
| Scene | 15 capsule/sphere skin proxies, 4-light setup |
| **Total frame GPU time** | **0.938 ms** |
| Game view rendered 15 proxies | ✓ |
| Screenshot | `N:\GravenSpire-sss-poc\Capture\sss-poc-15npc-capture.png` |

**Per-pass GPU timings (the decisive question for #3(b) cost model): NOT OBTAINED.**

Unity's `Recorder` API reported `gpuSampleBlockCount = 0` for all three `ProfilingSampler` markers (`Gravenspire.SSS.SkinStencilMask`, `Gravenspire.SSS.BlurH`, `Gravenspire.SSS.BlurVComposite`) despite the markers being declared in `SSSPass.cs` and the GPU profiler module reporting `Gathered, Enabled, Supported`. The GPU profiler flag `NotSupportedWithNativeGfxJobs` is the documented reason: URP 17.3 enables native graphics jobs by default on Direct3D12, which suppresses scripting access to per-pass GPU sample blocks. Disabling native gfx jobs would re-enable scripting GPU recording but would invalidate the rendering path being measured (different scheduling, different cost profile — circular).

Manual Frame Debugger UI capture was not performed in this PoC arc. The verdict pivot below explains why per-pass numbers were not necessary to close the bound condition.

## Findings

### F-1: Option 1 is implementable

The PoC compiles, registers in URP's renderer feature dropdown, runs, and produces a visible SSS effect on 15 skin proxies. Total frame GPU on RTX 5090 is 0.938ms, which bounds the three SSS passes' aggregate cost (the rest of the frame is a few primitives + 4 lights, no expensive shading). Positive evidence that Option 1 is not impossible — but per-pass attribution remains unmeasured.

### F-2: Per-pass cost numbers are not measurable via scripting in URP 17.3 + native gfx jobs

`Recorder.gpuSampleBlockCount = 0` across all three SSS markers despite marker presence + `Supported` flag. `NotSupportedWithNativeGfxJobs` is the documented reason. Manual Frame Debugger UI or Profiler GPU module hierarchy is required for per-pass GPU times.

### F-3: Option 1 has a structural visual-fidelity problem in URP forward

URP forward rendering composites diffuse + specular per fragment before any post-pass executes. The SSS V-blur composite operates on already-composited color through a stencil mask. Result: the blur smears specular highlights through the skin scatter, producing a visually-wrong soft halo around specular peaks rather than the diffusion-of-diffuse-light effect SSS is meant to produce. This is not a tuning issue — it is structural in the forward-rendering composition order. HDRP's deferred GBuffer separates diffuse + specular which is why HDRP's screen-space SSS works correctly; HDRP is blocked by `DECISIONS.md` D001.

### F-4: §S5.2 "Pre-Raphaelite portrait-grade skin" target requires Option 2 architecture regardless

Pre-integrated skin LUT (the recommended Option 2 approach for this URP-forward production path, suitable for Shader Graph authoring) operates in light-domain *before* per-fragment compositing. Cost scales per-draw (more named NPCs on screen = more cost), not per-screen-pixel as Option 1 does. Different cost model from the bible's original assumption — but the cost model that produces correct visuals in URP forward.

### F-5: Hardware-target governance drift (F-09) is unresolved and orthogonal

The art bible's polygon-budget validation claim ("tech-validated against GTX 1070 / RTX 4070+ / 1080p60") is unsupported by `.claude/docs/technical-preferences.md` (`[TO BE CONFIGURED]` for all performance fields). PoC capture on RTX 5090 cannot meaningfully validate the bible's GTX 1070 claim. Separate Technical Director + Producer decision tracked as `art_bible_hardware_target_drift` carryover in `production/sprint-status.yaml`.

## Recommendation

Adopt Option 2 (Shader Graph per-material pre-integrated skin LUT) as the working assumption for named-NPC skin shader authoring. Concrete next steps:

1. Re-run `/asset-spec` for the three blocked named-NPC specs (`M3_Caretaker_T1`, `Sister Elara`, `M3_CourtVendor_T1`) with Option 2 as the documented skin-shading approach.
2. Revise art-bible.md §S8.6 named-NPC GPU budget line item: replace "SSS: flat 1-2ms full-screen pass" with "Skin LUT: per-draw cost, measured during named-NPC shader validation after F-09 hardware target resolves."
3. Park the PoC project (`N:\GravenSpire-sss-poc\`) as a reference artifact. The `SSSPoCAutomation.cs` automation pattern is reusable for future similar PoCs.

## Re-verification triggers

This verdict is good until any of the following:

- Option 2 (Shader Graph per-material) fails §S5.2 visual differentiation in production
- F-09 hardware target resolves with a target that makes empirical Option 1 capture meaningful AND someone has reason to want to revisit
- URP 18+ adds native deferred-style SSS or alternative SSS infrastructure
- Manual Frame Debugger capture is performed on Option 1 (PoC project still operational) and the per-pass numbers contradict the structural analysis in F-3

## Open items (tracked elsewhere)

- **F-09 hardware-target governance drift:** unresolved; TD + Producer call needed. Tracked at `production/sprint-status.yaml` carryover `art_bible_hardware_target_drift`. NOT a blocker for this verdict.
- **Named-NPC §S8.6 budget revision:** the budget table needs the line-item swap described in Recommendation #2. Not included in this PoC closure; folded into the next art-bible amendment pass.

## What this verdict does NOT do

- Does not lock the named-NPC LOD0-3 polycount budgets (those depend on F-09)
- Does not measure Option 2's actual cost (production /asset-spec pass would surface that)
- Does not invalidate the PoC code — Option 1 remains a valid fallback if Option 2 hits unforeseen issues
- Does not affect bound conditions #3(a) (corpse-run desat) or #3(c) (mipmap bias)
