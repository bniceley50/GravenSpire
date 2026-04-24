---
name: create-stories
description: "Break one epic into implementation-ready story files. Reads the epic, GDD, governing ADRs, control manifest, and TR registry; emits traceable stories with type, dependencies, acceptance criteria, and required test evidence."
argument-hint: "[epic-slug | epic-path] [--review full|lean|solo] [--dry-run] [--force]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Task, AskUserQuestion
agent: lead-programmer
---

# Create Stories

Break a single epic into implementable story files. A story is one bounded unit of behavior that a developer can implement in one focused session, with clear acceptance criteria and evidence requirements.

This skill is autonomous by default. The user's invocation authorizes creation of missing story files and safe update of the epic's story table. Ask only when no epic can be inferred, existing story files would be overwritten, or review feedback creates a material scope decision.

Output:

```text
production/epics/[epic-slug]/story-NNN-[story-slug].md
production/epics/[epic-slug]/EPIC.md  # story table update only
```

Do not implement code. Run `/story-readiness [story-path]` and then `/dev-story [story-path]` after stories are created.

---

## 0. Execution Contract

### 0.1 Parse invocation

Supported inputs:

| Invocation | Behavior |
|---|---|
| `/create-stories combat` | Use `production/epics/combat/EPIC.md`. |
| `/create-stories production/epics/combat/EPIC.md` | Use the explicit epic file. |
| No argument | List epics missing story files and ask the user to choose. |

Flags:

- `--review full|lean|solo`
- `--dry-run`
- `--force`

Resolve review mode once:

1. Explicit `--review` value.
2. `production/review-mode.txt`, if present and valid.
3. `lean`.

Review semantics:

| Mode | QA story gate |
|---|---|
| `solo` | Skip Task reviews; self-generate test cases. |
| `lean` | Skip QA lead gate; self-generate test cases. |
| `full` | Run `qa-lead` gate `QL-STORY-READY`. |

### 0.2 Path safety

All paths are repository-relative. Reject absolute paths and paths containing `..`.

Accepted epic paths must match:

```text
production/epics/*/EPIC.md
```

Allowed write locations:

```text
production/epics/[epic-slug]/story-[NNN]-*.md
production/epics/[epic-slug]/EPIC.md
```

Do not write outside the selected epic directory.

### 0.3 Write policy

Routine writes are authorized by invocation:

- Creating missing story files for the selected epic.
- Adding or replacing the `## Stories` table in the selected `EPIC.md`.

Protected writes require confirmation:

- Overwriting an existing story file.
- Changing existing story statuses.
- Deleting story files.
- Editing any GDD, ADR, manifest, registry, or source code file.

If story files already exist and `--force` is absent, preserve them and create only missing stories using the next available story number. If `--force` is present, ask before replacing existing story files.

In `--dry-run`, do not call `Write` or `Edit`.

### 0.4 Required inputs

| Input | Behavior if missing |
|---|---|
| Selected `EPIC.md` | Stop; run `/create-epics` first. |
| Epic GDD file | Stop; stories cannot be traceable without the GDD. |
| Governing ADR file | Stop for accepted ADR references; create stories only when the epic explicitly marks the ADR as missing/proposed blocker. |
| `docs/architecture/tr-registry.yaml` | Continue with temporary `TR-[system]-???` IDs and mark stories `Needs Trace ID`. |
| `docs/architecture/control-manifest.md` | Continue; mark manifest rules unavailable. |
| Engine reference | Continue with engine risk `UNKNOWN`. |

---

## 1. Load Epic Context

Read the selected `EPIC.md` completely and extract:

- Epic title, slug, layer, status, architecture module.
- GDD path.
- Governing ADR rows and statuses.
- GDD requirement table.
- Blockers.
- Existing `## Stories` table, if any.

Read the GDD completely. Extract:

- Acceptance criteria.
- Formulas and thresholds.
- Edge cases.
- Performance constraints.
- UI, visual, audio, and feel requirements.
- Explicit out-of-scope notes.

Read all referenced ADRs that exist. For each ADR extract:

- `Status`.
- `Decision`.
- `Key Interfaces` or implementation guidelines.
- `Engine Compatibility`.
- `Registry Impact`.
- `GDD Requirements Addressed`.

Read:

- `docs/architecture/control-manifest.md`, if present.
- `docs/architecture/tr-registry.yaml`, if present.
- `.claude/docs/technical-preferences.md`, if present.

Report internally:

```text
Loaded epic [name], GDD [path], [N] ADRs, [M] requirements, manifest [present/missing].
```

---

## 2. Validate Story-Creation Preconditions

Before decomposing:

1. If the epic status is `Blocked`, identify each blocker.
2. If blockers are only proposed or missing ADRs, continue but create affected stories as `Blocked`.
3. If the GDD is missing or unreadable, stop.
4. If an accepted governing ADR is referenced but the file is missing, stop.
5. If existing story files are present, classify this as incremental mode.

Incremental mode:

- Preserve existing story files.
- Read their TR-IDs and acceptance criteria.
- Generate only missing stories for uncovered requirement rows.
- Use the next available `story-NNN` number.

Full mode:

- Generate stories for all requirement rows in the epic.

---

## 3. Decompose Requirements Into Stories

For each requirement row:

1. Map it to one or more acceptance criteria from the GDD.
2. Group criteria that require the same implementation unit.
3. Split groups larger than one focused session.
4. Order stories by dependency:
   - foundation/config
   - core logic
   - integration
   - edge cases
   - UI/presentation
   - visual/audio/feel polish
5. Assign a governing ADR.
6. Assign a story type.
7. Assign required evidence.

### 3.1 Story type classification

| Type | Use when |
|---|---|
| `Logic` | Formula, threshold, state transition, rule, AI decision, calculation. |
| `Integration` | Two or more systems communicate, save/load roundtrip, event/signal boundary. |
| `Visual/Feel` | Animation timing, VFX, audio sync, game feel, screen shake, responsiveness. |
| `UI` | HUD, menus, buttons, settings, tooltips, dialogue screens. |
| `Config/Data` | Data-only change, tuning table, localization row, content config. |

For mixed stories, choose the highest-risk type in this order:

```text
Integration > Logic > UI > Visual/Feel > Config/Data
```

### 3.2 Status rules

| Condition | Story status |
|---|---|
| Governing ADR is `Accepted`, dependencies clear, no unresolved design questions | `Ready` |
| Governing ADR is `Proposed` | `Blocked` |
| Governing ADR is missing | `Blocked` |
| TR-ID is temporary or missing | `Needs Trace ID` |
| Acceptance criteria contain `TBD`, `TODO`, `UNRESOLVED`, or `?` | `Needs Work` |

Do not set a story to `Ready` when an ADR blocker exists.

### 3.3 Evidence rules

| Type | Required evidence |
|---|---|
| `Logic` | `tests/unit/[system]/[story-slug]_test.[ext]` |
| `Integration` | `tests/integration/[system]/[story-slug]_test.[ext]` or documented integration playtest when automation is impossible. |
| `Visual/Feel` | `production/qa/evidence/[story-slug]-evidence.md` |
| `UI` | `production/qa/evidence/[story-slug]-evidence.md` or interaction test path. |
| `Config/Data` | Smoke-check evidence in `production/qa/smoke-[date].md`. |

---

## 4. Generate QA Test Cases

For every story, produce test guidance before writing the file.

For `Logic` and `Integration` stories:

```text
Test: [acceptance criterion]
Given: [precondition]
When: [action]
Then: [assertion]
Edge cases: [boundary values and failure states]
```

For `Visual/Feel`, `UI`, and `Config/Data` stories:

```text
Manual check: [acceptance criterion]
Setup: [how to reach the state]
Verify: [what to look for]
Pass condition: [unambiguous pass description]
```

In `full` review mode, spawn `qa-lead` using gate `QL-STORY-READY`. Pass:

- Proposed story list.
- Acceptance criteria.
- TR-IDs.
- Types.
- Required evidence paths.
- Blocked/Needs Work stories.

Ask the QA lead for:

1. Untestable criteria.
2. Missing edge cases.
3. Story splits needed for testability.
4. Evidence-path problems.
5. Test case improvements.

Apply clear fixes automatically. If the QA lead recommends a material scope split or merge, revise once; if still ambiguous, ask the user to choose.

Do not run more than one QA re-review.

---

## 5. Story File Template

Write each story as:

```markdown
# Story [NNN]: [Title]

> **Epic**: [Epic name]
> **Epic Path**: `production/epics/[epic-slug]/EPIC.md`
> **Status**: [Ready | Blocked | Needs Work | Needs Trace ID]
> **Layer**: [Foundation | Core | Feature | Presentation | Unknown]
> **Type**: [Logic | Integration | Visual/Feel | UI | Config/Data]
> **Estimate**: [2h | 4h | 1 day]
> **Manifest Version**: [version/date or Unavailable]
> **Generated**: [YYYY-MM-DD]

## Context

**GDD**: `design/gdd/[filename].md`
**Requirement IDs**: [TR-ID list]
**Requirement Summary**: [self-contained requirement statement]

**Governing ADRs**:

| ADR | Status | Usage |
|-----|--------|-------|
| ADR-NNNN: [title] | Accepted | [what this story must follow] |

**Architecture Module**: [module]
**Engine**: [engine + version or Unknown]
**Engine Risk**: [LOW | MEDIUM | HIGH | UNKNOWN]

## Acceptance Criteria

- [ ] AC-1: [specific, observable criterion from the GDD]
- [ ] AC-2: [criterion]
- [ ] AC-3: [criterion]

## Implementation Notes

- [ADR-derived implementation requirement]
- [control-manifest required pattern]
- [engine-specific note]

## Out of Scope

- [neighboring behavior not to implement]
- [future/polish work]

## QA Test Cases

[Generated test cases or manual checks.]

## Test Evidence

**Required evidence**: `[path]`
**Evidence status**: Not started

## Dependencies

| Depends On | Reason | Required Status |
|------------|--------|-----------------|
| [story or None] | [why] | Done |

## Blockers

- [ADR Proposed/Missing, unresolved design question, or "None"]
```

Story files must be self-contained. A developer should not need to open the GDD to understand the acceptance criteria.

---

## 6. Write Story Files and Epic Table

Before writing, build a summary:

```text
Stories for [epic]:
- story-001-[slug].md — [Type] — [Status] — [TR-IDs]
- story-002-[slug].md — [Type] — [Status] — [TR-IDs]
```

Then:

1. Validate every target path.
2. In `--dry-run`, show proposed files and stop without writing.
3. Write new story files.
4. Preserve existing story files unless `--force` is present and confirmed.
5. Update only the `## Stories` section of `EPIC.md`.

`EPIC.md` story table format:

```markdown
## Stories

| # | Story | Type | Status | Requirement IDs | ADRs | Evidence |
|---|-------|------|--------|-----------------|------|----------|
| 001 | [title] | Logic | Ready | TR-[system]-001 | ADR-NNNN | `tests/unit/...` |
```

If `EPIC.md` has no `## Stories` section, append one. If it has an old story placeholder, replace only that section.

---

## 7. Completion Output

End with:

```text
Verdict: [COMPLETE | PARTIAL | BLOCKED]

Story files written:
- [path] — [status]

Blocked stories:
- [story] — [reason]

Files changed:
- [story paths]
- production/epics/[epic-slug]/EPIC.md

Assumptions:
- [assumption or None]

Next best action:
- /story-readiness [first-ready-story-path]
```

If no story is `Ready`, recommend the blocking action, such as `/architecture-decision` for proposed/missing ADRs or `/architecture-review` for missing traceability.
