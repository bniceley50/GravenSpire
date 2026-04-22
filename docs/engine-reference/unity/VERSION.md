# Unity Engine — Version Reference

| Field | Value |
|-------|-------|
| **Engine Version** | Unity 6.3 LTS (6000.3.x) |
| **Release Date** | December 2025 |
| **Project Pinned** | 2026-04-21 |
| **Last Docs Verified** | 2026-04-21 |
| **LLM Knowledge Cutoff** | May 2025 |
| **Risk Level** | MEDIUM — beyond training data; incremental LTS diffs but includes real breaking changes (URP render passes, URP Compatibility Mode, UI Toolkit) |

## Knowledge Gap Warning

The LLM's training data likely covers Unity up to **~6.0 LTS** (October 2024)
and possibly early 6.1. Versions 6.1, 6.2, and 6.3 introduced changes — some
incremental, some breaking — that the model does NOT reliably know about.
Always cross-reference this directory before suggesting Unity API calls,
especially for URP, UI Toolkit, and rendering-pipeline code.

## Post-Cutoff Version Timeline

| Version | Release | Risk Level | Key Theme |
|---------|---------|------------|-----------|
| 6.0 LTS | Oct 2024 | LOW | First Unity 6 LTS — render graph in URP, Entities 1.3, UI Toolkit runtime. Likely in training data. |
| 6.1 | Apr 2025 | MEDIUM | Forward+ foveated rendering, camera history API, incremental performance |
| 6.2 | ~Jul 2025 | MEDIUM | `VisualElement.transform` deprecated; URP `SetupRenderPasses` deprecated (use `AddRenderPasses` + render graph); URP Compatibility Mode soft-deprecated |
| 6.3 LTS | Dec 2025 | MEDIUM | URP Compatibility Mode hidden by default; Box2D v3 API; Shader Graph template browser; SVG native in UI Toolkit; Platform Toolkit (cross-platform account/achievement API); Kawase/Dual filtering Bloom; Profiler Captures List |

Unity 6.3 LTS is supported through **December 2027**.

## Key Post-Cutoff Breaking Changes (summary — see breaking-changes.md for detail)

### URP / Rendering
- **`SetupRenderPasses` deprecated (6.2)** → use `AddRenderPasses` + render graph
- **URP Compatibility Mode hidden by default (6.3)** → migrate custom passes to render graph
- **BIRP deprecation process starts in Unity 6.5** → all new projects must use URP (or HDRP for photoreal)

### UI Toolkit
- **`VisualElement.transform` deprecated (6.2)** → use `style.translate` / `.rotate` / `.scale`
- **Vector Graphics package integrated (6.3)** → SVG import is native in UI Toolkit, no separate package

### Physics / 2D
- **Box2D v3 API added (6.3)** — runs alongside existing API, will replace it in future version

## 2026 Render Pipeline Strategy

- **URP is the recommended pipeline** for all new projects.
- **HDRP is maintenance-only** — only adding Nintendo Switch 2 support, no new features.
- **BIRP deprecation starts in Unity 6.5** — do NOT start new projects in BIRP.
- URP and HDRP now share the same underlying compiler and API — moving toward a unified renderer.

## Project-Specific Notes

- **This project uses URP** (Universal Render Pipeline). See `.claude/docs/technical-preferences.md`.
- All custom render-pass code must use the **render graph system** + `AddRenderPasses`, never the deprecated `SetupRenderPasses`.
- The faction simulation system is a **candidate for DOTS/ECS** — see `unity-dots-specialist` agent.
- Zone streaming should use **Addressables**, not `Resources` folders — see `unity-addressables-specialist`.
- **FishNet** is planned for Tier 2+ netcode (not installed yet; will be added to Allowed Libraries when Tier 2 work begins).

## Verified Sources

- Official Unity 6.3 what's new: https://docs.unity3d.com/6000.3/Documentation/Manual/WhatsNewUnity63.html
- Unity 6.3 LTS announcement: https://unity.com/blog/unity-6-3-lts-is-now-available
- Unity 6.2 upgrade guide: https://docs.unity3d.com/6000.2/Documentation/Manual/UpgradeGuideUnity62.html
- Unity 6.0 upgrade guide: https://docs.unity3d.com/6000.1/Documentation/Manual/UpgradeGuideUnity6.html
- Unity 6.3 planned breaking changes: https://discussions.unity.com/t/planned-breaking-changes-in-unity-6-3/1646418
- URP 17 what's new: https://docs.unity3d.com/6000.0/Documentation/Manual/urp/whats-new/urp-whats-new.html
- Render Pipelines strategy 2026: https://unity.com/topics/render-pipelines-strategy-for-2026
