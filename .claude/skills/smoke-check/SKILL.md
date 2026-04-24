---
name: smoke-check
description: "Run the critical-path smoke test gate before QA hand-off. Executes safe automated tests where available, collects manual smoke confirmations, checks evidence coverage, and writes a PASS / PASS WITH WARNINGS / FAIL report."
argument-hint: "[sprint | quick] [--platform pc|console|mobile|all] [--dry-run]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash, Write, AskUserQuestion
---

# Smoke Check

Run the gate between implementation and QA hand-off. A failed smoke check means the build is not ready for QA. This skill may run tests and write a smoke report; it must not edit source code, tests, stories, QA plans, stage files, or release files.

Output:

```text
production/qa/smoke-[scope]-[YYYYMMDD].md
```

---

## 0. Execution Contract

### 0.1 Parse invocation

Base modes:

| Mode | Behavior |
|---|---|
| `sprint` or blank | Full smoke check for current sprint. |
| `quick` | Skip coverage scan and long manual batches; use for re-checking after a fix. |

Platform flag:

| Flag | Adds |
|---|---|
| `--platform pc` | PC controls, windowing, resolution checks. |
| `--platform console` | Gamepad, safe-zone, cold boot, platform-prompt checks. |
| `--platform mobile` | Touch, orientation, background/foreground, mobile performance checks. |
| `--platform all` | All platform batches. |

`--dry-run` means run discovery and tests, collect manual confirmations if needed, but do not write a report file.

### 0.2 Path safety and write policy

Allowed write location:

```text
production/qa/smoke-[scope]-[YYYYMMDD].md
```

If the target exists, create a numbered variant. Do not overwrite.

Do not write anywhere else.

### 0.3 Bash policy

Bash may be used only for:

- Running test commands.
- Listing test result artifacts.
- Reading logs/results.
- Non-destructive environment checks.

Do not install packages, launch watchers, delete files, modify git state, deploy, publish, call network tools, or run destructive shell commands.

If a test command is unavailable, record `NOT RUN`; do not attempt installation.

---

## 1. Detect Scope and Environment

Read or infer:

- Engine from `.claude/docs/technical-preferences.md`.
- Test framework from `tests/`, project config, and test setup docs.
- Current sprint from `production/session-state/active.md` or most recent `production/sprints/*` file.
- Latest QA plan from `production/qa/qa-plan-*.md`.
- Existing smoke checklist from `production/qa/smoke-tests.md` or `tests/smoke/`.
- Recent test result artifacts from `test-results/`, `coverage/`, engine-specific logs, or CI artifact directories.

Stop if no `tests/`, no QA plan, and no smoke checklist exist only when the project has no observable QA structure at all. Otherwise proceed and record missing items.

Environment summary:

```text
Engine: [engine or Unknown]
Tests directory: [found/missing]
QA plan: [path/missing]
Smoke checklist: [path/generated fallback]
Current sprint: [name/unknown]
```

---

## 2. Run Automated Tests

Select the narrowest safe command from project evidence.

Command selection priority:

1. Explicit test command in `production/qa/qa-plan-*.md`.
2. `package.json` scripts, if present.
3. Engine-specific test setup docs.
4. Common engine defaults below.
5. Recent test result artifacts if no runner is available.

Common defaults:

| Engine | Safe attempt |
|---|---|
| Godot | `godot --headless -s addons/gdunit4/GdUnitRunner.gd` if runner exists. |
| Unity | Read latest test result artifact; do not assume Unity Editor is available. |
| Unreal | Read latest automation/test log; do not assume editor automation is available. |
| Unknown | Do not guess. Record `NOT RUN`. |

For any Bash command:

- Prefer commands that already exist in project config.
- Capture output.
- Stop after one failing runner command unless an obvious fallback runner path exists.
- Do not run commands expected to be long-running.

Classify automated tests:

| Status | Meaning |
|---|---|
| `PASS` | Runner or artifact reports no failures. |
| `FAIL` | Runner or artifact reports one or more failures, crash, or fatal error. |
| `NOT RUN` | No runner/artifact available or environment lacks engine binary. |
| `UNKNOWN` | Output exists but cannot be parsed. |

`NOT RUN` is not automatic failure. It contributes to `PASS WITH WARNINGS` unless manual checks fail.

---

## 3. Check Evidence Coverage

Skip this phase in `quick` mode.

Build story list from:

1. Latest QA plan.
2. Current sprint file.
3. Story paths found in `production/session-state/active.md`.
4. If none, glob `production/epics/**/story-*.md` and include Ready/In Progress/Implemented stories only.

For each story:

- Read story type.
- Read required evidence path.
- Check whether the evidence file exists.
- For Logic/Integration, check whether test path exists.
- For Visual/Feel/UI, check whether manual evidence doc exists.
- For Config/Data, mark smoke evidence expected.

Coverage statuses:

| Status | Meaning |
|---|---|
| `COVERED` | Required test/evidence file exists. |
| `MANUAL PENDING` | Manual evidence path declared but not yet filled. |
| `MISSING` | Required evidence path missing. |
| `EXPECTED` | Config/Data or smoke-only evidence. |
| `UNKNOWN` | Story unreadable or evidence requirement absent. |

Coverage gaps do not automatically fail smoke check unless they affect Batch 1 or Batch 2. They are warnings that must be resolved before `/story-done`.

---

## 4. Manual Smoke Checks

Manual checks require user confirmation. Use at most three core batches plus platform batches.

If a project-specific smoke checklist exists, use it. Otherwise generate fallback checks from the current sprint's stories and the standard batches below.

### 4.1 Batch 1 — Core stability

Ask:

```text
Smoke check — Core stability

[A] PASS — game launches to main menu without crash
[B] FAIL — launch/main menu crash or hang
[C] NOT CHECKED — cannot verify in this environment
```

Then ask for session start/input if not covered by the first response:

```text
Smoke check — Basic interaction

[A] PASS — new session starts and primary input responds
[B] FAIL — session start or primary input broken
[C] NOT CHECKED
```

### 4.2 Batch 2 — Sprint critical path

Derive the top one to three sprint mechanics from the QA plan or story list.

Ask a compact batch question:

```text
Smoke check — Sprint critical path

For each item, report PASS, FAIL, or NOT CHECKED:
- [mechanic/story 1]
- [mechanic/story 2]
- Regression check: previous critical path still works
```

Any explicit FAIL in this batch contributes to overall `FAIL`.

### 4.3 Batch 3 — Persistence/performance

Skip in `quick` mode.

Ask:

```text
Smoke check — Persistence and performance

[A] PASS — save/load and basic performance acceptable
[B] FAIL — save/load or obvious performance regression found
[C] PARTIAL — one item not implemented or not checked
[D] NOT APPLICABLE — no save/performance-sensitive systems yet
```

### 4.4 Platform batches

Run only for requested platform(s).

PC:

- Keyboard/mouse controls.
- Controller support if target includes controller.
- Windowed/fullscreen/resolution handling.
- Graphics settings persistence, if implemented.

Console:

- Gamepad control path.
- Safe-zone/UI clipping.
- Cold boot.
- Platform prompt consistency.

Mobile:

- Touch controls.
- Orientation handling.
- Background/foreground behavior.
- Obvious battery/thermal/performance issue.

Collect PASS/FAIL/NOT CHECKED per platform.

---

## 5. Determine Verdict

Use first matching rule:

| Verdict | Rule |
|---|---|
| `FAIL` | Automated tests fail, Batch 1 fails, Batch 2 fails, or any requested platform has a critical FAIL. |
| `PASS WITH WARNINGS` | No failing test/check, but automated tests are NOT RUN/UNKNOWN, coverage is missing, Batch 3 is partial, or manual checks are NOT CHECKED. |
| `PASS` | Automated tests pass, Batch 1 and Batch 2 pass, requested platform checks pass or N/A, and no required evidence is missing. |

Do not downgrade to `FAIL` solely because automated tests could not run in this environment.

---

## 6. Write Smoke Report

Report template:

```markdown
# Smoke Check Report: [scope]

Generated: [YYYY-MM-DD]
Mode: [sprint | quick]
Platform: [none | pc | console | mobile | all]
Verdict: [PASS | PASS WITH WARNINGS | FAIL]

## Environment

| Item | Value |
|------|-------|
| Engine | [engine] |
| QA Plan | [path or Missing] |
| Current Sprint | [name or Unknown] |
| Test Setup | [summary] |

## Automated Tests

| Status | Command/Artifact | Summary |
|--------|------------------|---------|
| PASS/FAIL/NOT RUN/UNKNOWN | `[command or artifact]` | [counts/failures/reason] |

## Evidence Coverage

| Story | Type | Required Evidence | Status |
|-------|------|-------------------|--------|

## Manual Smoke Checks

| Check | Result | Notes |
|-------|--------|-------|

## Platform Checks

[Only if platform flag was supplied.]

## Warnings

- [warning or None]

## Failures

- [failure or None]

## Gate Decision

[Plain-language explanation of verdict and what must happen next.]
```

If `--dry-run`, show the report in conversation and do not write.

Otherwise write to `production/qa/smoke-[scope]-[YYYYMMDD].md`, using a numbered suffix if needed.

---

## 7. Completion Output

End with:

```text
Verdict: [PASS | PASS WITH WARNINGS | FAIL | DRY RUN]
Report: [path or not written]
Failures: [N]
Warnings: [N]
Next best action: [command]
```

Next actions:

| Verdict | Next action |
|---|---|
| `PASS` | `/team-qa sprint` |
| `PASS WITH WARNINGS` | `/team-qa sprint`, with warnings carried forward. |
| `FAIL` | Fix listed failures, then rerun `/smoke-check`. |
