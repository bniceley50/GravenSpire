---
name: setup-engine
description: "Configure the project's game engine, version, technical preferences, specialist routing, and local engine-reference docs. Supports guided setup, refresh, and upgrade workflows with official-source version verification."
argument-hint: "[engine] [version] [--language gdscript|csharp|both] [--dry-run] | refresh [--dry-run] | upgrade [old-version] [new-version] [--dry-run] | no args for guided selection"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, WebSearch, WebFetch, Task, AskUserQuestion
---

# Setup Engine

Configure the repository's pinned game engine and the downstream agent metadata that depends on it.

This skill is allowed to make routine repo-local configuration writes needed for engine setup, because invoking `/setup-engine` is an explicit setup request. It must still stop before protected changes such as replacing a different existing engine, overwriting non-placeholder user-authored configuration, or changing source files.

## Operating Contract

### Autonomy defaults

- Proceed autonomously through discovery, version lookup, config generation, and routine writes.
- Ask only when information is missing, ambiguous, or a protected change is required.
- If `--dry-run` is present, do all discovery and produce the full write plan, but do not write files.
- Never invent current engine versions, migration facts, licensing facts, deprecated APIs, or breaking changes. Verify them from official sources or mark them unknown.
- Prefer official engine documentation, release pages, migration guides, changelogs, and package/release repositories. Ignore unofficial posts unless official sources are unavailable, and clearly label them as non-authoritative.

### Protected changes requiring explicit confirmation

Stop and ask before:

- Switching from one configured engine to another.
- Replacing a non-placeholder value in `AGENTS.md`, `.claude/docs/technical-preferences.md`, or existing engine-reference docs.
- Deleting files or directories.
- Editing source code.
- Creating speculative dependency allow-lists.
- Performing an engine upgrade that changes the pinned version.

### Repository path rules

All write targets must be repository-relative and must not contain absolute paths or `..` traversal.

Allowed write targets for this skill:

```text
AGENTS.md
.claude/docs/technical-preferences.md
.claude/agents/*.md
docs/engine-reference/<engine>/**
```

If any target falls outside these paths, stop.

---

## 1. Parse Invocation

Recognize these modes:

| Invocation | Mode |
|---|---|
| `/setup-engine` | Guided engine selection |
| `/setup-engine godot 4.6` | Configure explicit engine and version |
| `/setup-engine unity` | Configure explicit engine, resolve current stable version |
| `/setup-engine refresh` | Refresh docs for the existing pinned engine |
| `/setup-engine upgrade 4.4 4.6` | Audit and update pinned version |

Normalize aliases:

| Input | Canonical engine |
|---|---|
| `godot`, `godot4` | `godot` |
| `unity`, `unity6` | `unity` |
| `unreal`, `ue`, `ue5`, `unreal-engine` | `unreal` |

Recognize options:

- `--language gdscript|csharp|both` — Godot language choice.
- `--dry-run` — preview only.
- `--force-refresh` — refresh existing reference docs even if last verified recently.

If an unknown option is provided, continue if it does not affect behavior, but report it in the summary as ignored.

---

## 2. Discover Existing Repository State

Read or inspect:

```text
AGENTS.md
.claude/docs/technical-preferences.md
docs/engine-reference/
docs/engine-reference/*/VERSION.md
design/gdd/game-concept.md
production/stage.txt
```

Extract:

- Existing engine and version, if configured.
- Existing language choice.
- Existing specialist routing.
- Existing target platforms and input method preferences.
- Game concept signals: genre, 2D/3D, target platforms, team size, scope, art style, online/multiplayer, and any existing engine recommendation.
- Whether engine-reference docs already exist and when they were last verified.

If the repository already has a configured engine different from the requested engine, classify as an **engine switch** and require explicit confirmation before changing any file.

If `AGENTS.md` or `.claude/docs/technical-preferences.md` is missing, create it only as part of the approved setup write plan. Do not block solely because the template is absent.

---

## 3. Guided Engine Selection

Run this phase only when no engine argument was supplied.

### 3.1 Prefer existing concept data

If `design/gdd/game-concept.md` exists, infer likely engine fit from:

- Dimension: 2D, 2.5D, contained 3D, open-world 3D, photorealistic 3D.
- Platform targets: PC, mobile, console, web, multi-platform.
- Team size and programming experience.
- Language preference.
- Visual ambition.
- Multiplayer or online requirements.
- Asset-store dependency.

If no concept exists, ask a compact set of questions. Do not run a long interview.

### 3.2 Ask only missing high-impact questions

Use `AskUserQuestion` for missing facts, with these questions in priority order:

1. Prior engine experience.
2. Target platform.
3. 2D/3D scope and visual ambition.
4. Team size and programming comfort.
5. Language preference.

If prior engine experience is strong and the engine is viable for the concept, recommend that engine. Prior experience can outweigh marginal technical fit.

### 3.3 Recommendation policy

Give one primary recommendation and one fallback. Do not present a fake numeric scoring matrix.

Use these stable tradeoffs:

- **Godot**: strongest for 2D, stylized/contained indie projects, rapid iteration, open source, and solo teams. Weaker for large open-world 3D, asset-store depth, and first-party console workflow.
- **Unity**: strongest for mobile, mid-scope 3D, broad tutorial/asset ecosystem, and multi-platform indie production. Requires awareness of licensing and ecosystem changes, which must be verified from current official terms if relevant.
- **Unreal Engine**: strongest for high-fidelity 3D, large-scale 3D worlds, advanced rendering, Blueprint-heavy prototyping, and high-end PC/console projects. Heavier tooling and steeper learning curve.

Ask for confirmation only if the user has not already provided a clear engine preference.

---

## 4. Resolve Version From Official Sources

If the user provided a version, verify that it exists using official sources when possible.

If no version was provided:

1. Use `WebSearch` to locate the official latest stable release source.
2. Use `WebFetch` on official pages only when possible.
3. Record the source URL and date verified.
4. If results conflict, ask the user which version to pin.

Search patterns:

```text
<engine> official latest stable release
<engine> releases official
<engine> changelog official <year>
<engine> download official stable
```

Official source examples:

| Engine | Preferred source type |
|---|---|
| Godot | `godotengine.org`, official docs, official GitHub releases |
| Unity | Unity download archive, Unity manual/release notes, official Unity blog/docs |
| Unreal | Epic documentation, Unreal release notes, Epic launcher/version docs |

Do not use stale known-version tables as source of truth.

---

## 5. Resolve Language and Platform Defaults

### 5.1 Godot language

If engine is Godot, determine language:

1. Use `--language` if provided.
2. Else use existing technical preferences if configured.
3. Else infer from prior engine/language experience.
4. Else ask once with `AskUserQuestion`.

Options:

| Choice | Use when |
|---|---|
| `gdscript` | Beginners, solo projects, fast iteration, most gameplay/UI scripting |
| `csharp` | Existing C#/Unity experience, stronger IDE tooling, larger codebases |
| `both` | Advanced projects with clear boundaries between GDScript gameplay/UI and C# performance-critical systems |

Guardrail: For `gdscript`, the project language field must be exactly `GDScript`. Do not append `C++ via GDExtension` as a primary language.

### 5.2 Platform and input defaults

Infer from game concept where possible. Ask only if unclear.

Mapping:

| Target platform | Gamepad support | Touch support | Primary input default |
|---|---|---|---|
| PC | Partial | None | Keyboard/Mouse unless action game |
| Console | Full | None | Gamepad |
| Mobile | None | Full | Touch |
| PC + Console | Full | None | Gamepad for action, Keyboard/Mouse for strategy |
| PC + Mobile | Partial | Full | Ask if unclear |
| Web | Partial | Partial | Keyboard/Mouse or Touch by genre |

---

## 6. Determine Knowledge Risk

Compare the pinned engine version to local and fetched reference data. Do not rely on a hardcoded model cutoff as an absolute fact.

Classify:

| Risk | Meaning |
|---|---|
| LOW | Version is well-covered by local docs and no recent breaking changes were found. |
| MEDIUM | Version is near recent releases, docs are incomplete, or domain-specific behavior changed. |
| HIGH | Version is newer than local reference coverage, migration docs show breaking changes, or critical APIs changed. |

If `MEDIUM` or `HIGH`, engine reference docs are required before dependent architecture/code-generation skills should rely on engine-specific APIs.

---

## 7. Generate Configuration Content

Generate a single write plan before writing. The plan must list each target file, action, and whether the edit is placeholder fill, append, create, or protected replacement.

### 7.1 `AGENTS.md` Technology Stack

Ensure `AGENTS.md` contains a Technology Stack section. Fill or update engine fields.

Godot templates:

```markdown
- **Engine**: Godot [version]
- **Language**: GDScript
- **Build System**: SCons (engine), Godot Export Templates
- **Asset Pipeline**: Godot Import System + custom resource pipeline
```

```markdown
- **Engine**: Godot [version]
- **Language**: C# (.NET, primary), C++ via GDExtension (native plugins only)
- **Build System**: .NET SDK + Godot Export Templates
- **Asset Pipeline**: Godot Import System + custom resource pipeline
```

```markdown
- **Engine**: Godot [version]
- **Language**: GDScript (gameplay/UI scripting), C# (performance-critical systems), C++ via GDExtension (native plugins only)
- **Build System**: .NET SDK + Godot Export Templates
- **Asset Pipeline**: Godot Import System + custom resource pipeline
```

Unity template:

```markdown
- **Engine**: Unity [version]
- **Language**: C#
- **Build System**: Unity Build Pipeline
- **Asset Pipeline**: Unity Asset Import Pipeline + Addressables when explicitly adopted
```

Unreal template:

```markdown
- **Engine**: Unreal Engine [version]
- **Language**: C++ (primary), Blueprint (gameplay prototyping)
- **Build System**: Unreal Build Tool (UBT)
- **Asset Pipeline**: Unreal Content Pipeline
```

Do not add libraries such as Steam, Addressables, GAS, DOTS, or GDExtension as adopted dependencies unless the project is actively integrating them. Mention them only as available specialists or future options.

### 7.2 `.claude/docs/technical-preferences.md`

Ensure this file contains:

```markdown
# Technical Preferences

## Engine & Language
- **Engine**: [engine]
- **Version**: [version]
- **Primary Language**: [language]
- **Knowledge Risk**: [LOW/MEDIUM/HIGH]
- **Last Engine Docs Verified**: [date]

## Input & Platform
- **Target Platforms**: [platforms]
- **Input Methods**: [methods]
- **Primary Input**: [primary]
- **Gamepad Support**: [None/Partial/Full]
- **Touch Support**: [None/Partial/Full]
- **Platform Notes**: [notes]

## Naming Conventions
[engine/language-specific conventions]

## Performance Budgets
[configured defaults or TO BE CONFIGURED]

## Testing
[engine-specific test framework recommendation]

## Forbidden Patterns
[TO BE CONFIGURED]

## Allowed Libraries
[TO BE CONFIGURED]

## Engine Specialists
[routing table]

## File Extension Routing
[routing table]
```

Performance budgets may use conservative defaults only if target platform is known. Otherwise leave as `[TO BE CONFIGURED]`.

### 7.3 Naming conventions

Godot GDScript:

```markdown
- Classes: PascalCase (`PlayerController`)
- Variables/functions: snake_case (`move_speed`)
- Signals: snake_case past tense or event form (`health_changed`)
- Files: snake_case (`player_controller.gd`)
- Scenes: PascalCase (`PlayerController.tscn`)
- Constants: UPPER_SNAKE_CASE (`MAX_HEALTH`)
```

Godot C#:

```markdown
- Classes: PascalCase and `partial` (`public partial class PlayerController`)
- Public members: PascalCase
- Private fields: `_camelCase`
- Methods: PascalCase
- Signal delegates: PascalCase + `EventHandler`
- Files: PascalCase matching class
```

Unity:

```markdown
- Classes/files/methods/properties: PascalCase
- Private fields: `_camelCase`
- Serialized private fields: `[SerializeField] private ...`
- Coroutines: PascalCase with action-oriented names
```

Unreal:

```markdown
- Actor classes: `A` prefix
- UObject classes: `U` prefix
- Structs: `F` prefix
- Booleans: `b` prefix
- Functions and variables: PascalCase
- Files: class name without Unreal prefix when appropriate
```

### 7.4 Specialist routing

Godot GDScript:

```markdown
## Engine Specialists
- **Primary**: godot-specialist
- **Language/Code Specialist**: godot-gdscript-specialist
- **Shader Specialist**: godot-shader-specialist
- **UI Specialist**: godot-specialist
- **Additional Specialists**: godot-gdextension-specialist only for native extensions

## File Extension Routing
| File Extension / Type | Specialist |
|---|---|
| `.gd` | godot-gdscript-specialist |
| `.gdshader`, VisualShader | godot-shader-specialist |
| `.tscn`, `.tres` | godot-specialist |
| `.gdextension`, native C++ | godot-gdextension-specialist |
| Architecture review | godot-specialist |
```

Godot C# and mixed-language projects should route `.cs` to `godot-csharp-specialist` and `.gd` to `godot-gdscript-specialist` when present.

Unity:

```markdown
## Engine Specialists
- **Primary**: unity-specialist
- **Shader Specialist**: unity-shader-specialist
- **UI Specialist**: unity-ui-specialist
- **Additional Specialists**: unity-dots-specialist, unity-addressables-specialist only when those systems are actively adopted

## File Extension Routing
| File Extension / Type | Specialist |
|---|---|
| `.cs` | unity-specialist |
| `.shader`, `.shadergraph`, `.mat` | unity-shader-specialist |
| `.uxml`, `.uss`, Canvas/UI prefabs | unity-ui-specialist |
| `.unity`, `.prefab` | unity-specialist |
```

Unreal:

```markdown
## Engine Specialists
- **Primary**: unreal-specialist
- **Blueprint Specialist**: ue-blueprint-specialist
- **UI Specialist**: ue-umg-specialist
- **Additional Specialists**: ue-gas-specialist, ue-replication-specialist only when those systems are actively adopted

## File Extension Routing
| File Extension / Type | Specialist |
|---|---|
| `.cpp`, `.h` | unreal-specialist |
| Blueprint assets | ue-blueprint-specialist |
| UMG/CommonUI | ue-umg-specialist |
| GAS systems | ue-gas-specialist |
| Multiplayer/replication | ue-replication-specialist |
```

---

## 8. Generate Engine Reference Docs

Target directory:

```text
docs/engine-reference/<engine>/
```

### 8.1 Minimum docs for LOW risk

Create or update:

```text
docs/engine-reference/<engine>/VERSION.md
```

Template:

```markdown
# [Engine] — Version Reference

| Field | Value |
|---|---|
| **Engine Version** | [version] |
| **Project Pinned** | [date] |
| **Last Docs Verified** | [date] |
| **Knowledge Risk** | LOW |
| **Version Source** | [official URL or source description] |
| **Release Notes Source** | [official URL or "Not required"] |

## Notes

This version has low project-specific engine-reference risk. If future ADRs or implementation stories rely on engine APIs that changed recently, run `/setup-engine refresh` before generating code.
```

### 8.2 Full docs for MEDIUM/HIGH risk

Create or update:

```text
VERSION.md
breaking-changes.md
deprecated-apis.md
current-best-practices.md
modules/<domain>.md    # only for changed or high-risk domains
```

Every file must include:

```markdown
**Last verified**: [date]
**Sources**:
- [official source title] — [URL]
```

`breaking-changes.md` must group changes by version and domain.

`deprecated-apis.md` must use this table:

```markdown
| API / Pattern | Status | Replacement | Source | Notes |
|---|---|---|---|---|
```

`current-best-practices.md` must separate verified current practices from project conventions.

Do not create empty module files. If no domain-specific changes were verified, omit `modules/`.

---

## 9. Apply Write Plan

If `--dry-run`, print the write plan and stop.

Otherwise:

1. Create missing parent directories.
2. Apply placeholder fills and file creations.
3. For protected replacements, stop and ask for confirmation before editing.
4. After each write, re-read the file if the next edit depends on its current content.
5. Do not overwrite unrelated sections.

Edit strategy:

- Use `Edit` for targeted changes in existing files.
- Use `Write` only for new files or complete generated files that do not already exist.
- If an existing file's structure is unrecognizable, append a clearly labeled generated section rather than rewriting the whole file.

---

## 10. Update Specialist Agent Instructions

For the chosen engine's specialist agents under `.claude/agents/`, verify they include a `Version Awareness` section.

If missing, append:

```markdown
## Version Awareness

Before giving engine-specific API guidance:

1. Read `docs/engine-reference/<engine>/VERSION.md`.
2. Check `deprecated-apis.md` when present.
3. Check `breaking-changes.md` when present.
4. Use official documentation if an API is uncertain or version-sensitive.
5. Prefer project-pinned engine version behavior over general engine memory.
```

Only edit specialist files that already exist. Do not create speculative agents.

---

## 11. Refresh Mode

Run when invoked as `/setup-engine refresh`.

1. Read the existing pinned engine from `docs/engine-reference/*/VERSION.md` or `.claude/docs/technical-preferences.md`.
2. Locate official current release notes, migration guides, and deprecated API lists.
3. Compare current docs against official sources.
4. Update only changed reference docs.
5. Preserve project-specific notes.
6. Update `Last Docs Verified` / `Last verified` dates on modified docs.

If the latest stable version differs from the pinned version, do not upgrade automatically. Report:

```text
Pinned version: [version]
Latest stable verified: [version]
Recommendation: [stay pinned | consider upgrade]
Run `/setup-engine upgrade [old] [new]` to change the project pin.
```

---

## 12. Upgrade Mode

Run when invoked as `/setup-engine upgrade [old-version] [new-version]`.

### 12.1 Read current pin

Read:

```text
docs/engine-reference/<engine>/VERSION.md
.claude/docs/technical-preferences.md
AGENTS.md
```

If `old-version` is omitted, use the pinned version.

If pinned version and supplied `old-version` conflict, ask before proceeding.

### 12.2 Fetch official migration data

Use official sources to collect:

- Migration guide.
- Release notes.
- Breaking changes.
- Deprecated and removed APIs.
- Changed defaults.

### 12.3 Pre-upgrade audit

Search project files for affected APIs. Prioritize:

```text
src/**
addons/**
Assets/**
Source/**
tests/**
```

Report:

```markdown
## Pre-Upgrade Audit: [engine] [old] → [new]

| File | API / Pattern Found | Source Change | Effort | Notes |
|---|---|---|---|---|

### Migration Order
1. [lowest dependency system]
2. [next]

### Risk
[LOW/MEDIUM/HIGH]
```

This audit must not modify source files.

### 12.4 Protected version update

Changing the pinned engine version is protected. Ask:

```text
Proceed with updating repository configuration from [old] to [new]?
This updates engine metadata and reference docs only. It does not migrate source files.
```

If approved:

- Update `AGENTS.md` engine line.
- Update `.claude/docs/technical-preferences.md` version and risk fields.
- Update `VERSION.md` and append `## Migration Notes — [old] → [new]`.
- Append relevant changes to `breaking-changes.md` and `deprecated-apis.md`.

---

## 13. Output Summary

Always finish with:

```markdown
## Engine Setup Summary

| Field | Value |
|---|---|
| Engine | [engine] |
| Version | [version] |
| Language | [language] |
| Knowledge Risk | [LOW/MEDIUM/HIGH] |
| Official Version Source | [source] |
| Reference Docs | [created/updated/skipped] |
| AGENTS.md | [created/updated/unchanged] |
| Technical Preferences | [created/updated/unchanged] |
| Specialist Instructions | [updated/unchanged/skipped] |
| Dry Run | [yes/no] |

### Files changed
- [path]

### Follow-up
1. Review `docs/engine-reference/<engine>/VERSION.md`.
2. Run `/test-setup` after engine setup if tests are not initialized.
3. Run `/create-architecture` or `/architecture-decision` only after the engine reference is available for MEDIUM/HIGH risk versions.
```

Verdict:

- `COMPLETE` — engine configured and reference docs available.
- `PARTIAL` — configuration written but some official docs could not be verified.
- `BLOCKED` — missing required user choice or protected change was not approved.
