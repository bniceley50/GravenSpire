---
name: team-qa
description: "Orchestrate a full QA cycle for a sprint or feature. Produces a QA plan, manual test cases, bug reports, and QA sign-off using qa-lead and qa-tester subagents where useful."
argument-hint: "[sprint | sprint:<name> | feature:<system-name>] [--dry-run] [--manual-only]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Task, AskUserQuestion
agent: qa-lead
---

# Team QA

Run a structured QA cycle for a sprint or feature. This skill coordinates QA planning, test case preparation, manual result collection, bug report creation, and sign-off. It does not advance project stage, close stories, deploy builds, or modify source code.

The user's invocation authorizes routine QA artifact writes. Ask only for missing scope, manual QA results, bug severity confirmation, or protected actions.

Routine outputs:

```text
production/qa/qa-plan-[scope]-[date].md
production/qa/test-cases/[scope]-[date].md
production/qa/bugs/BUG-[NNN]-[slug].md
production/qa/qa-signoff-[scope]-[date].md
```

---

## 0. Execution Contract

### 0.1 Parse invocation

Supported scopes:

| Invocation | Scope |
|---|---|
| `/team-qa sprint` | Current sprint. |
| `/team-qa sprint:sprint-03` | Named sprint. |
| `/team-qa feature:combat` | Stories for one feature/system. |
| No argument | Infer current sprint; ask only if no current sprint can be found. |

Flags:

- `--dry-run`: generate plan and report in conversation only; do not write QA artifacts or bug reports.
- `--manual-only`: skip QA plan regeneration and use the latest existing QA plan/test cases.

### 0.2 Path safety

Reject absolute paths and paths containing `..`.

Allowed write locations:

```text
production/qa/
```

Do not modify story files, source files, test files, release files, stage files, or registry files.

### 0.3 Write policy

Routine QA writes are authorized by invocation:

- QA plan.
- Test case document.
- Bug reports.
- QA sign-off report.

Protected writes requiring explicit confirmation:

- Changing story status.
- Changing sprint status.
- Editing release/stage files.
- Overwriting an existing QA sign-off report.
- Any write outside `production/qa/`.

If an output path exists, create a numbered alternative rather than overwriting unless the user confirms.

### 0.4 Task policy

Use Task subagents for bounded QA analysis:

| Agent | Use |
|---|---|
| `qa-lead` | Strategy, risk assessment, sign-off verdict. |
| `qa-tester` | Test case drafting and bug report drafting. |

Do not delegate open-ended repository exploration. Pass story paths, relevant excerpts, required output format, and verdict rules.

---

## 1. Resolve Scope

Determine the in-scope stories.

For current sprint:

1. Read `production/session-state/active.md`, if present.
2. Glob `production/sprints/*.md` and `production/sprints/*.yaml`.
3. Use the active or most recently modified sprint file.
4. Extract story paths and priority tiers.

For named sprint:

- Read matching file under `production/sprints/`.

For feature:

- Glob `production/epics/[feature]/story-*.md`.
- If no exact epic slug exists, grep `production/epics/**/*.md` for the feature name.

Read each story and extract:

- Path and title.
- Status.
- Type.
- Acceptance criteria.
- Test evidence requirement.
- Existing evidence path.
- Priority, if from sprint.
- Governing ADRs and blockers.

Also read if present:

- Latest `production/qa/qa-plan-*.md`.
- Latest `production/qa/smoke-*.md`.
- Existing `production/qa/bugs/*.md`.
- `production/stage.txt`.

If no stories are found, stop with the exact scope searched.

---

## 2. Entry Gate

Before QA execution, classify entry readiness:

| Check | Pass condition |
|---|---|
| Stories exist | At least one in-scope story. |
| Story closure state | Stories are implemented or ready for QA; not Draft/Blocked. |
| Smoke check | Latest smoke report is `PASS` or `PASS WITH WARNINGS`, or user accepts manual fallback. |
| Evidence paths | Each story has a declared evidence path. |
| Critical blockers | No unresolved S1/S2 open bugs in current scope unless retest is specifically requested. |

If latest smoke report is `FAIL`, stop. Do not proceed to manual QA until `/smoke-check` passes.

If no smoke report exists, continue only after asking:

```text
No smoke check report was found for this scope.

[A] Continue with QA and record smoke check as missing
[B] Stop and run /smoke-check first
```

---

## 3. QA Strategy

Spawn `qa-lead` unless the scope is very small and the strategy is obvious.

Provide:

- Story list and statuses.
- Story types.
- Acceptance criteria counts.
- Evidence requirements.
- Existing smoke result.
- Open bugs.
- Stage and sprint context.

Ask for:

1. QA risk classification per story.
2. Automated versus manual coverage needs.
3. Retest needs for open bugs.
4. Recommended manual test batches.
5. Sign-off risk.

Produce a strategy table:

```markdown
| Story | Type | Priority | Auto Evidence | Manual QA | Risk | Notes |
|-------|------|----------|---------------|-----------|------|-------|
```

If the strategy identifies a critical blocker, stop and report it unless the user explicitly chose a retest-only QA run.

---

## 4. Generate QA Plan

Skip this phase only when `--manual-only` is set and a recent QA plan exists.

QA plan format:

```markdown
# QA Plan: [scope]

Generated: [YYYY-MM-DD]
Stage: [stage or Unknown]
Smoke Check: [PASS | PASS WITH WARNINGS | FAIL | Missing]

## Scope

- [story path] — [title]

## Entry Criteria

- [criterion]

## Story Coverage

| Story | Type | Acceptance Criteria | Required Evidence | Manual QA Needed |
|-------|------|---------------------|-------------------|------------------|

## Manual Test Batches

### Batch 1: Critical Path

- [story/check]

### Batch 2: Regression and Edge Cases

- [story/check]

## Out of Scope

- [excluded story/system and reason]

## Exit Criteria

- All critical-path checks pass.
- No open S1/S2 bugs.
- Any S3/S4 deferrals are documented.
```

Write to `production/qa/qa-plan-[scope]-[YYYYMMDD].md` unless `--dry-run`.

---

## 5. Generate Test Cases

For each story requiring manual QA, spawn `qa-tester` in bounded batches. Pass:

- Story path and content.
- Acceptance criteria.
- Required evidence path.
- Relevant QA strategy notes.

Require output:

```markdown
## [Story title]

### Test Case [N]: [criterion]

- Preconditions:
- Steps:
  1. [step]
  2. [step]
- Expected Result:
- Actual Result: [blank]
- Result: [PASS / FAIL / BLOCKED]
- Notes:
```

For automated-only Logic stories, include an evidence verification row instead of manual steps:

```markdown
## Automated Evidence Verification

| Story | Expected Test | Current Status | Retest Needed |
|-------|---------------|----------------|---------------|
```

Write test cases to `production/qa/test-cases/[scope]-[YYYYMMDD].md` unless `--dry-run`.

---

## 6. Collect Manual QA Results

Manual execution requires user input. Batch checks in groups of no more than four stories.

For each batch, ask:

```text
Manual QA batch: [batch name]

For each story, choose result:
[A] PASS all checks
[B] PASS WITH NOTES
[C] FAIL
[D] BLOCKED / could not test
```

When a story fails or is blocked, collect:

- Failure description.
- Repro steps.
- Expected result.
- Actual result.
- Severity estimate: S1, S2, S3, S4.
- Whether the bug blocks sign-off.

If the user gives incomplete bug detail, ask only for the missing fields necessary to write a useful bug report.

---

## 7. Write Bug Reports

For every FAIL result, create a bug report unless `--dry-run`.

Numbering:

1. Glob `production/qa/bugs/BUG-*.md`.
2. Use the next available `BUG-[NNN]`.
3. Slug from story title or failure summary.

Template:

```markdown
# BUG-[NNN]: [Short title]

> **Severity**: [S1 | S2 | S3 | S4]
> **Status**: Open
> **Found In**: [scope]
> **Story**: `[story path]`
> **Reported**: [YYYY-MM-DD]

## Summary

[one paragraph]

## Reproduction Steps

1. [step]

## Expected Result

[expected]

## Actual Result

[actual]

## Evidence

- [screenshot/video/log path or "Not provided"]

## Sign-Off Impact

[Blocks sign-off / Does not block sign-off / Unknown]
```

Do not change story status or sprint status when filing bugs.

---

## 8. Produce Sign-Off Verdict

Spawn `qa-lead` with final results if available. Otherwise compute directly using the rules below.

Verdict rules:

| Verdict | Conditions |
|---|---|
| `APPROVED` | All required manual checks pass, automated evidence is present or confirmed, and no open S1/S2 bugs. |
| `APPROVED WITH CONDITIONS` | Only S3/S4 bugs, PASS WITH NOTES items, missing non-critical evidence, or accepted smoke warnings remain. |
| `NOT APPROVED` | Any open S1/S2 bug, failed critical-path story, blocked manual QA item, or failed smoke check. |

Sign-off report:

```markdown
# QA Sign-Off Report: [scope]

Generated: [YYYY-MM-DD]
Verdict: [APPROVED | APPROVED WITH CONDITIONS | NOT APPROVED]

## Scope Summary

| Story | Type | Result | Evidence | Bugs |
|-------|------|--------|----------|------|

## Smoke Check

[latest smoke result]

## Bugs Found

| Bug | Severity | Story | Status | Sign-Off Impact |
|-----|----------|-------|--------|-----------------|

## Conditions

- [condition or None]

## Rationale

[why the verdict was assigned]

## Next Step

[gate-check, bug fixing, targeted retest, or rerun team-qa]
```

Write to `production/qa/qa-signoff-[scope]-[YYYYMMDD].md` unless `--dry-run`.

---

## 9. Completion Output

End with:

```text
Verdict: [APPROVED | APPROVED WITH CONDITIONS | NOT APPROVED | BLOCKED | DRY RUN]

QA artifacts:
- [path]

Bugs filed:
- BUG-[NNN] — [severity] — [summary]

Blocking issues:
- [issue or None]

Next best action:
- [command]
```

Recommended next actions:

| Verdict | Next action |
|---|---|
| `APPROVED` | `/gate-check` for formal stage or release gate validation. |
| `APPROVED WITH CONDITIONS` | Resolve listed conditions or document deferrals before gate-check. |
| `NOT APPROVED` | Fix S1/S2 or critical-path failures, then rerun `/smoke-check` and `/team-qa`. |
| `BLOCKED` | Resolve entry gate blocker first. |
