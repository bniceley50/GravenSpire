---
name: create-epics
description: "Translate approved GDDs and architecture into implementation epics. Creates one bounded epic per architectural module, records governing ADRs, requirement traceability, engine risk, dependency order, and story-creation handoff. Does not create stories."
argument-hint: "[system-name | layer:foundation|core|feature|presentation | all] [--review full|lean|solo] [--dry-run] [--force]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Task, AskUserQuestion
agent: technical-director
---

# Create Epics

Create implementation epics from approved design and architecture artifacts. Each epic must be small enough to become a set of stories, traceable to GDD requirements, governed by accepted or explicitly blocking ADRs, and ordered by architectural dependency.

This skill is autonomous by default. The user's invocation authorizes creation of repo-local epic artifacts. Ask only when no scope can be inferred, when an existing epic would be overwritten, or when producer review exposes a materially different scope choice.

Output:

```text
production/epics/[epic-slug]/EPIC.md
production/epics/index.md
```

Do not create story files. Run `/create-stories [epic-slug]` after this skill.

---

## 0. Execution Contract

### 0.1 Parse invocation

Supported scopes:

| Invocation | Scope |
|---|---|
| `/create-epics all` | Every eligible system in layer order. |
| `/create-epics layer:foundation` | Foundation systems only. |
| `/create-epics layer:core` | Core systems only. |
| `/create-epics layer:feature` | Feature systems only. |
| `/create-epics layer:presentation` | Presentation systems only. |
| `/create-epics combat` | One named system. |
| No argument | Infer from unfinished GDDs and ask only if multiple scopes are equally plausible. |

Normalize layer arguments by removing spaces after `layer:` and lowercasing the layer name.

Flags:

- `--review full|lean|solo`
- `--dry-run`
- `--force`

Resolve review mode once:

1. Explicit `--review` value.
2. `production/review-mode.txt`, if present and valid.
3. `lean`.

Review semantics:

| Mode | Producer gate |
|---|---|
| `solo` | Skip Task reviews. |
| `lean` | Skip producer gate. |
| `full` | Run `producer` gate `PR-EPIC`. |

### 0.2 Path safety

All paths are repository-relative. Reject absolute paths and any path containing `..`.

Allowed write locations:

```text
production/epics/[epic-slug]/EPIC.md
production/epics/index.md
```

Do not write outside `production/epics/`.

### 0.3 Write policy

Routine writes are authorized by invocation:

- Creating a missing epic directory.
- Writing a new `EPIC.md` for a system that does not already have one.
- Creating or updating `production/epics/index.md` with the generated epic rows.

Protected writes require confirmation through `AskUserQuestion`:

- Replacing an existing `EPIC.md`.
- Changing the status of an existing epic.
- Removing rows from `production/epics/index.md`.
- Modifying files outside `production/epics/`.

If `--force` is not present and `EPIC.md` already exists, do not overwrite it. Either leave it unchanged or create a proposed revision at:

```text
production/epics/[epic-slug]/EPIC.proposed-[YYYYMMDD].md
```

In `--dry-run`, do not call `Write` or `Edit`. Present all proposed paths and file contents in the final report.

### 0.4 Missing-file behavior

| File | Behavior if missing |
|---|---|
| `design/gdd/systems-index.md` | For `all` or `layer:` scope, stop. For single-system scope, continue by locating a matching GDD. |
| In-scope GDD | Stop for that system; no epic can be created without a GDD. |
| `docs/architecture/architecture.md` | Continue with `Architecture Module: Unknown`; mark epic `Blocked: architecture map missing`. |
| `docs/architecture/control-manifest.md` | Continue; mark manifest rules unavailable. |
| `docs/architecture/tr-registry.yaml` | Continue; use inferred temporary requirement IDs and warn. |
| Governing ADR files | Continue only if the ADR is referenced but missing; mark affected requirements blocked. |
| `docs/engine-reference/*/VERSION.md` | Continue with `Engine: Unknown`; mark engine risk `UNKNOWN`. |

---

## 1. Discover In-Scope Systems

Use a bounded discovery sequence:

1. Read `design/gdd/systems-index.md` if present.
2. Grep `design/gdd/*.md` for `## Summary`, `Status:`, `Layer:`, `System:`, and `GDD:`.
3. Select only systems in the requested scope.
4. Read full GDD files only for selected systems.
5. Ignore Draft or Deprecated GDDs unless the user explicitly requested that system.

Eligibility rules:

| GDD status | Action |
|---|---|
| `Approved` | Eligible. |
| `Designed` | Eligible, but mark epic `Status: Ready with Design Caveat`. |
| `Draft` | Skip in `all` and `layer:` scope; block in single-system scope unless user confirms. |
| `Deprecated` | Skip. |
| Missing status | Include only in single-system scope; mark assumption. |

If no systems are eligible, stop with:

```text
No eligible GDD systems found for [scope]. Run /design-system or approve the relevant GDDs first.
```

---

## 2. Load Architecture and Traceability Context

Read these once where available:

- `docs/architecture/architecture.md`
- `docs/architecture/control-manifest.md`
- `docs/architecture/tr-registry.yaml`
- `docs/registry/architecture.yaml`
- `docs/engine-reference/*/VERSION.md`
- Existing `production/epics/*/EPIC.md`

For each in-scope GDD, read only relevant ADRs:

1. ADRs explicitly referenced by the GDD.
2. ADRs whose `GDD Requirements Addressed` section names the GDD or its system.
3. ADRs in `docs/registry/architecture.yaml` relevant to the system domain.

For each relevant ADR, extract:

- ADR number and title.
- `Status`.
- Decision summary.
- Engine compatibility risk.
- GDD requirements addressed.
- Registry impact, if present.

Do not read unrelated ADRs unless no relevant ADR can be found.

---

## 3. Build the Epic Model

For each selected system, derive:

| Field | Source |
|---|---|
| Epic name | GDD title or system name. |
| Slug | Lowercase system slug, kebab-case. |
| Layer | `systems-index.md`, GDD header, or architecture map. |
| GDD path | Selected GDD file. |
| Architecture module | `architecture.md` ownership section. |
| Governing ADRs | Accepted ADRs covering the system, plus proposed/missing ADR blockers. |
| Requirement rows | `tr-registry.yaml` active entries for the system, or GDD acceptance criteria. |
| Engine risk | Highest risk among governing ADRs and engine reference. |
| Status | `Ready`, `Blocked`, or `Ready with Design Caveat`. |

### 3.1 Requirement coverage rules

Classify every requirement:

| Coverage | Meaning |
|---|---|
| `Covered` | Active TR-ID maps to an accepted ADR or manifest rule. |
| `Blocked: ADR Proposed` | Governing ADR exists but status is `Proposed`. |
| `Blocked: ADR Missing` | GDD requires an architectural decision but no ADR exists. |
| `Untraced` | Requirement exists in GDD but no TR-ID was found. |
| `N/A` | Requirement is content/design only and does not need an ADR. |

Do not invent accepted ADR coverage. If uncertain, mark `Untraced` and include a recommended ADR title.

### 3.2 Epic status rules

| Condition | Epic status |
|---|---|
| All implementation requirements covered by accepted ADRs or manifest rules | `Ready` |
| Any governing ADR is `Proposed` or missing | `Blocked` |
| GDD is `Designed` but not `Approved` | `Ready with Design Caveat` |
| Architecture map missing | `Blocked` unless single-system prototype scope is explicit |

Blocked epics may still be written. Their story creation handoff must state which ADRs must be accepted before stories become implementable.

---

## 4. Producer Gate

Run only in `full` review mode.

Spawn `producer` with gate `PR-EPIC` from `.claude/docs/director-gates.md`. Pass:

- In-scope epic models.
- Requirement counts.
- Blocked requirements.
- Layer order.
- Existing epics.
- Current milestone or sprint context, if found.

Ask the producer for:

1. Scope realism.
2. Dependency ordering problems.
3. Oversized epics that should be split.
4. Undersized epics that should be merged.
5. Critical missing prerequisites.

Handle the result:

| Producer result | Action |
|---|---|
| `APPROVE` | Continue. |
| `CONCERNS` | Apply clear non-controversial fixes, record concerns in epic notes. |
| `UNREALISTIC` or `REJECT` | Revise epic boundaries once if the corrective action is obvious; otherwise ask the user to choose split, merge, or stop. |

Do not run more than one producer re-review.

---

## 5. Generate Epic Files

Use this exact `EPIC.md` structure.

```markdown
# Epic: [System Name]

> **Layer**: [Foundation | Core | Feature | Presentation | Unknown]
> **GDD**: `design/gdd/[filename].md`
> **Architecture Module**: [module name or Unknown]
> **Status**: [Ready | Blocked | Ready with Design Caveat]
> **Stories**: Not yet created — run `/create-stories [epic-slug]`
> **Generated**: [YYYY-MM-DD]

## Overview

[One concise paragraph derived from the GDD overview and architecture module responsibility.]

## Scope

### In Scope

- [system capability]
- [system capability]

### Out of Scope

- [neighboring system responsibility]
- [future/polish work]

## Dependency Order

| Depends On | Reason | Status |
|------------|--------|--------|
| [epic/ADR/system or None] | [why] | [Ready/Blocked/Unknown] |

## Governing ADRs

| ADR | Status | Decision Summary | Engine Risk |
|-----|--------|------------------|-------------|
| ADR-NNNN: [title] | Accepted | [summary] | LOW/MEDIUM/HIGH |

## GDD Requirements

| TR-ID | Requirement | Coverage | Story Guidance |
|-------|-------------|----------|----------------|
| TR-[system]-001 | [requirement text] | Covered by ADR-NNNN | Implement as [Logic/Integration/UI/etc.] story |

## Blockers

- [blocking ADR, missing architecture, or "None"]

## Definition of Done

This epic is complete when:

- Every requirement row has a story or is explicitly out of scope.
- Every story is closed through `/story-done`.
- Logic and Integration stories have passing test evidence.
- Visual/Feel and UI stories have manual QA evidence.
- Any blocking ADRs are accepted before implementation begins.

## Story Creation Handoff

Run:

```text
/create-stories [epic-slug]
```

Story generation must preserve the requirement IDs, governing ADRs, layer, and blocker notes recorded above.
```

### 5.1 Index format

Create or update `production/epics/index.md`:

```markdown
# Epics Index

Last Updated: [YYYY-MM-DD]
Engine: [engine name + version or Unknown]

| Epic | Layer | System | GDD | Stories | Status | Next Command |
|------|-------|--------|-----|---------|--------|--------------|
| [Epic name] | [Layer] | [slug] | `design/gdd/[file].md` | Not yet created | [Ready/Blocked] | `/create-stories [slug]` |
```

When updating the index, preserve existing rows for unrelated epics. Add or replace only rows for epics in this run.

---

## 6. Write Outputs

For each epic:

1. Validate the target path: `production/epics/[slug]/EPIC.md`.
2. If `--dry-run`, do not write; include the full proposed file in the report.
3. If the file does not exist, create it.
4. If it exists and `--force` is absent, do not overwrite. Create `EPIC.proposed-[YYYYMMDD].md` and report that the existing epic was preserved.
5. If it exists and `--force` is present, ask for confirmation before overwriting.

Then update `production/epics/index.md` unless `--dry-run`.

---

## 7. Completion Output

End with:

```text
Verdict: [COMPLETE | PARTIAL | BLOCKED]

Epics generated:
- [path] — [Ready/Blocked]

Blocked requirements:
- [TR-ID] — [reason]

Files changed:
- [path]

Assumptions:
- [assumption or "None"]

Next best action:
- /create-stories [first-ready-epic]
```

If all Foundation and Core epics are written, also suggest `/gate-check production`. Do not run it automatically.
