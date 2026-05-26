# Technical Preferences

> **Behavioral contract:** `AGENTS.md` (project root). This file governs tech
> choices; `AGENTS.md` governs workflow and behavior. See `AGENTS.md` §4 for
> the source-of-truth table.

<!-- Populated by /setup-engine. Updated as the user makes decisions throughout development. -->
<!-- All agents reference this file for project-specific standards and conventions. -->

## Engine & Language

- **Engine**: Unity 6.3 LTS
- **Language**: C# (.NET 8+, primary)
- **Rendering**: URP (Universal Render Pipeline) — fits stylized gothic 3D; switch to HDRP only if photoreal is ever required (not planned)
- **Physics**: Unity Physics (PhysX) — standard MonoBehaviour physics; no DOTS physics planned

## Input & Platform

<!-- Written by /setup-engine. Read by /ux-design, /ux-review, /test-setup, /team-ui, and /dev-story -->
<!-- to scope interaction specs, test helpers, and implementation to the correct input methods. -->

- **Target Platforms**: PC (Windows primary, macOS secondary)
- **Input Methods**: Keyboard/Mouse
- **Primary Input**: Keyboard/Mouse
- **Gamepad Support**: Partial (controllers should be bindable for accessibility, but keyboard/mouse is the expected primary)
- **Touch Support**: None
- **Platform Notes**: MMO-genre conventions — every action must be keybindable; a hotkey/hotbar system is required; mouse-over tooltips acceptable; chat input must never be swallowed by hotkey bindings when typing

## Naming Conventions

- **Classes**: PascalCase (e.g., `PlayerController`)
- **Public fields/properties**: PascalCase (e.g., `MoveSpeed`)
- **Private fields**: `_camelCase` (e.g., `_moveSpeed`)
- **Methods**: PascalCase (e.g., `TakeDamage()`)
- **Signals/Events**: PascalCase + `Changed` or `ed` past tense suffix (e.g., `HealthChanged`, `DamageTaken`)
- **Files**: PascalCase matching class (e.g., `PlayerController.cs`)
- **Scenes/Prefabs**: PascalCase matching root object (e.g., `GravenspireCity.unity`, `Cleric.prefab`)
- **Constants**: PascalCase for instance constants (e.g., `MaxHealth`); UPPER_SNAKE_CASE for true static compile-time constants (e.g., `MAX_PARTY_SIZE`)

## Performance Budgets

- **Target Framerate**: [TO BE CONFIGURED — set during Tier 1 prototype once target hardware is known]
- **Frame Budget**: [TO BE CONFIGURED]
- **Draw Calls**: [TO BE CONFIGURED]
- **Memory Ceiling**: [TO BE CONFIGURED]

## Testing

- **Framework**: Unity Test Framework (NUnit-based) for unit + integration tests; Moq for mocking
- **Minimum Coverage**: 70% on gameplay systems; 90% on combat formulas and faction simulation logic
- **Required Tests**: Balance formulas, gameplay systems, networking (once Tier 2 begins), faction simulation state transitions

## Forbidden Patterns

<!-- Add patterns that should never appear in this project's codebase -->
- [None configured yet — add as architectural decisions are made]

## Allowed Libraries / Addons

<!-- Add approved third-party dependencies here -->
<!-- Guardrail: Do NOT add speculative dependencies. A library is added here ONLY when work actively begins on the system that requires it. -->
- `com.unity.nuget.newtonsoft-json` — Unity package dependency for local
  Combat Core fixture deserialization in `S2-M2-01`; .NET/headless Combat Core
  tests continue to use `System.Text.Json`.
- `com.unity.ai.navigation` — Unity package dependency for NavMesh authoring
  and runtime path queries in `S3-05` and downstream district/zone stories.
  Pinned to `2.0.12` for Unity 6.3 LTS.

<!--
Deferred libraries (planned but NOT yet approved for use):
- FishNet — identified in /brainstorm as the planned netcode library for Tier 2+. Add to Allowed Libraries when Tier 2 (multiplayer co-op) work actively begins, not before.
-->

## Architecture Decisions Log

<!-- Quick reference linking to full ADRs in docs/architecture/ -->
- [No ADRs yet — use /architecture-decision to create one]

## Engine Specialists

<!-- Written by /setup-engine when engine is configured. -->
<!-- Read by /code-review, /architecture-decision, /architecture-review, and team skills -->
<!-- to know which specialist to spawn for engine-specific validation. -->

- **Primary**: unity-specialist
- **Language/Code Specialist**: unity-specialist (C# review — primary covers it)
- **Shader Specialist**: unity-shader-specialist (Shader Graph, HLSL, URP/HDRP materials)
- **UI Specialist**: unity-ui-specialist (UI Toolkit UXML/USS, UGUI Canvas, runtime UI)
- **Additional Specialists**: unity-dots-specialist (ECS, Jobs system, Burst compiler), unity-addressables-specialist (asset loading, memory management, content catalogs)
- **Routing Notes**: Invoke primary for architecture and general C# code review. Invoke DOTS specialist for any ECS/Jobs/Burst code (faction simulation is a candidate for DOTS once scale demands it). Invoke shader specialist for rendering and visual effects. Invoke UI specialist for all interface implementation. Invoke Addressables specialist for asset management systems and zone streaming.

### File Extension Routing

<!-- Skills use this table to select the right specialist per file type. -->
<!-- If a row says [TO BE CONFIGURED], fall back to Primary for that file type. -->

| File Extension / Type | Specialist to Spawn |
|-----------------------|---------------------|
| Game code (.cs files) | unity-specialist |
| Shader / material files (.shader, .shadergraph, .mat) | unity-shader-specialist |
| UI / screen files (.uxml, .uss, Canvas prefabs) | unity-ui-specialist |
| Scene / prefab / level files (.unity, .prefab) | unity-specialist |
| Native extension / plugin files (.dll, native plugins) | unity-specialist |
| General architecture review | unity-specialist |
