---
name: dev-story
description: "Implement one ready story. Loads the story, GDD requirement, governing ADRs, control manifest, engine preferences, and test evidence requirements; then writes scoped source/test changes and records session state."
argument-hint: "[story-path] [--dry-run] [--no-tests]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Bash, Task, AskUserQuestion
---

# Dev Story

Implement one story from `production/epics/`. This skill is the coding bridge between planning and completion. It must preserve story scope, follow accepted ADRs, respect the control manifest, create required test evidence, and leave closure to `/story-done`.

The user's invocation authorizes routine repository-local implementation writes. Do not ask before ordinary source, test, data, or evidence-file edits that are required by the story. Ask only for protected actions, dependency overrides, out-of-scope changes, or unresolved design choices.

Do not mark the story Done. `/story-done` performs closure.

---

## 0. Execution Contract

### 0.1 Parse invocation

Supported flags:

- `--dry-run`: load context and produce an implementation plan; do not write code, tests, evidence, or session state.
- `--no-tests`: allowed only for `Visual/Feel`, `UI`, or `Config/Data` stories. For `Logic` and `Integration`, this flag is ignored unless the user explicitly confirms the test deferral.

If a story path is provided, validate it. It must match:

```text
production/epics/*/story-*.md
```

If no path is provided:

1. Read `production/session-state/active.md` and look for the active story.
2. If exactly one active story is found, use it.
3. Otherwise glob `production/epics/**/story-*.md` and choose the first `Ready` story whose dependencies are complete.
4. If multiple equally valid choices remain, ask the user to choose.

### 0.2 Path safety

Reject absolute paths and paths containing `..`.

Routine write locations:

- Project source directories inferred from engine/project layout.
- `tests/`.
- `production/qa/evidence/`.
- `production/session-state/active.md`.

Protected locations requiring explicit confirmation:

- `.claude/`, `.claude/`, `.github/`, CI files, release/deploy config.
- `AGENTS.md`.
- `docs/architecture/`, `docs/registry/`, `design/gdd/`.
- Other stories or epics, except session-state references.
- Dependency story status updates.

Never delete, move, publish, deploy, install packages, or modify credentials.

### 0.3 Bash policy

Bash is permitted for:

- Listing files.
- Running tests.
- Reading test result artifacts.
- Non-destructive diagnostics.

Do not use Bash for destructive commands, package installation, network calls, git operations that mutate state, deployment, long-running watchers, or process killing.

### 0.4 Stop conditions

Stop before implementation if any of these are true:

- Story file missing or invalid.
- Story status is `Blocked`, `Needs Work`, `Draft`, or unknown.
- Governing ADR is missing or not `Accepted`.
- A dependency story is missing or not `Done`/`Complete`/`Closed`, unless the user explicitly accepts the dependency risk.
- Story contains unresolved design questions in acceptance criteria or implementation notes.
- The requested implementation would violate a forbidden control-manifest rule.
- The story requires an architectural decision not covered by an accepted ADR.

When stopping, write no source/test changes and provide the exact remediation command.

---

## 1. Load Story and Supporting Context

Read the story completely. Extract:

- Title and story number.
- Epic path and slug.
- Status.
- Layer.
- Type.
- Estimate.
- Manifest Version.
- GDD path.
- Requirement IDs.
- Governing ADRs.
- Acceptance criteria.
- Implementation notes.
- Out of scope.
- QA test cases.
- Test evidence path.
- Dependencies.
- Blockers.

Read supporting files:

| File | Purpose | Missing behavior |
|---|---|---|
| Epic `EPIC.md` | Scope and neighboring stories | Warn and continue if story is self-contained. |
| GDD path | Requirement context | Continue only if story criteria are self-contained; otherwise stop. |
| `docs/architecture/tr-registry.yaml` | Current requirement text | Warn and use story text if missing. |
| Governing ADRs | Binding implementation rules | Stop if any required ADR missing or not accepted. |
| `docs/architecture/control-manifest.md` | Required/forbidden patterns | Warn if missing; stop if story requires known manifest validation and manifest is absent. |
| `.claude/docs/technical-preferences.md` | Engine, naming, specialists, budgets | Warn and infer from project if missing. |
| Engine reference docs | Engine risk verification | Warn if missing; use ADR engine notes as source of truth. |

---

## 2. Validate Readiness Inline

Before coding, run a lightweight readiness check:

- Status must be `Ready` unless the only issue is `Needs Trace ID` and the story is otherwise implementable.
- ADRs must be accepted.
- Dependencies must be complete.
- Acceptance criteria must be testable.
- Out-of-scope boundaries must be clear.
- Evidence path must exist in the story.
- For Logic/Integration, test cases must be present or derivable from acceptance criteria.

If readiness fails, stop and recommend `/story-readiness [story-path]`.

### 2.1 Dependency handling

For each dependency:

1. Locate the dependency story.
2. Read its status.
3. If not complete, ask:

```text
[Current story] depends on [dependency] which is [status].

[A] Stop and complete the dependency first
[B] Proceed anyway and record a dependency-risk deviation
[C] The dependency is complete; update is needed elsewhere
```

If `[C]`, do not modify the dependency story unless the user separately confirms the protected status update. Prefer stopping and running `/story-done` on the dependency.

---

## 3. Select Implementation Route

Use the story type, layer, and engine to decide whether to implement directly or delegate with `Task`.

| Story context | Default route |
|---|---|
| `Config/Data` | Implement directly. |
| Small `Logic` story with localized files | Implement directly. |
| `Integration` story | Spawn primary programmer Task unless straightforward. |
| `UI` story | Spawn `ui-programmer` if available; otherwise implement directly. |
| `Visual/Feel` story | Spawn `gameplay-programmer` if available; otherwise implement directly. |
| Engine risk `HIGH` | Spawn engine specialist Task for review before or after implementation. |

Primary programmer routing:

| Context | Agent |
|---|---|
| Foundation/Core engine systems | `engine-programmer` |
| Gameplay mechanics | `gameplay-programmer` |
| AI/pathfinding | `ai-programmer` |
| Networking/replication | `network-programmer` |
| UI/HUD/menus | `ui-programmer` |

Engine specialist routing comes from `.claude/docs/technical-preferences.md` under `Engine Specialists`. If no specialist is configured, continue and record that specialist review was skipped.

When spawning a Task, pass bounded context only:

- Story content.
- Acceptance criteria.
- ADR Decision and Key Interfaces verbatim.
- Manifest rules for this layer.
- Engine notes and risk.
- Target evidence path.
- Out-of-scope list.
- Explicit instruction not to modify protected files.

---

## 4. Implement Within Scope

Create a concise internal implementation plan:

1. Files to create or modify.
2. Acceptance criteria covered by each change.
3. Test or evidence file to create.
4. Risks or deviations.

Do not ask the user to approve the plan unless it touches protected files or expands scope.

Implementation rules:

- Follow the ADR over personal preference.
- Do not introduce new architecture not covered by the ADR.
- Do not implement out-of-scope items.
- Do not change GDDs, ADRs, registry files, epics, or unrelated stories.
- Prefer the smallest code change that satisfies the acceptance criteria.
- Preserve existing public APIs unless the ADR explicitly changes them.
- Add comments only where they clarify non-obvious constraints or public API behavior.

If implementation requires a protected or out-of-scope change, stop and ask:

```text
Implementing [criterion] requires modifying [path], which is outside this story's scope.

[A] Proceed with this scoped exception
[B] Stop and create a follow-up story
[C] Revise the implementation approach
```

Record any approved exception in the final summary.

---

## 5. Create Required Evidence

### 5.1 Logic and Integration stories

A test is required unless the user explicitly confirms deferral.

Use the story's `## QA Test Cases` and acceptance criteria to create tests. Each acceptance criterion should map to at least one test assertion.

Test rules:

- Deterministic tests only.
- No random seeds unless fixed.
- No external network or uncontrolled time dependency.
- Cover formula bounds and edge cases from the GDD.
- Put tests at the evidence path declared in the story unless the project's test layout requires a close equivalent.

### 5.2 Visual/Feel and UI stories

If automation is impractical, create or update the evidence stub:

```text
production/qa/evidence/[story-slug]-evidence.md
```

Include:

- Story path.
- Acceptance criteria.
- Manual verification steps.
- Expected pass condition.
- Placeholder for tester notes.

### 5.3 Config/Data stories

Record changed values and smoke-check expectations. If the story declares a smoke evidence path, create a small evidence stub; otherwise record in the final summary that `/smoke-check quick` is the evidence mechanism.

---

## 6. Run Verification

Skip Bash only when `--dry-run` is active or the story type has no runnable automated evidence.

Select a test command from available project evidence:

1. Existing `package.json`, test scripts, or project test config.
2. Engine-specific test setup docs.
3. `.claude/docs/technical-preferences.md`.
4. Recently used commands in `production/qa/` or `test-results/`.

Run the narrowest relevant test first. If it passes, optionally run the broader suite if inexpensive.

Record:

- Command run.
- Pass/fail/not run.
- Test count, if available.
- Failure names, if any.
- Reason if not run.

Do not install missing test frameworks or modify test infrastructure. If tests cannot run, record `NOT RUN` and explain what the user should run locally.

---

## 7. Update Session State

Unless `--dry-run`, append a session extract to:

```text
production/session-state/active.md
```

Create the file if missing.

Use:

```markdown
## Session Extract — /dev-story — [YYYY-MM-DD]

- Story: `[story-path]` — [title]
- Status after implementation: Implemented, pending `/code-review` and `/story-done`
- Files changed:
  - `[path]` — [created/modified]
- Evidence:
  - `[path]` — [test/evidence status]
- Verification:
  - `[command]` — [PASS/FAIL/NOT RUN]
- Deviations:
  - [None or approved exception]
- Next:
  - `/code-review [files]`
  - `/story-done [story-path]`
```

Do not change the story's status to Done. Do not update sprint status.

---

## 8. Completion Output

End with:

```markdown
## Implementation Result: [Story title]

Verdict: [IMPLEMENTED | PARTIAL | BLOCKED | DRY RUN]

### Files Changed

- `[path]` — [created/modified, purpose]

### Acceptance Criteria Coverage

| AC | Status | Evidence |
|----|--------|----------|
| AC-1 | Covered | `[file or test]` |

### Verification

| Command | Result | Notes |
|---------|--------|-------|
| `[command]` | PASS/FAIL/NOT RUN | [notes] |

### Deviations

- [None or explicit exception]

### Next Best Action

Run `/code-review [changed files]`, then `/story-done [story-path]`.
```

If blocked, include only remediation commands and do not claim implementation was completed.
