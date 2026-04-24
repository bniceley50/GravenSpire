---
name: hotfix
description: "Emergency fix workflow for S1/S2 issues with severity triage, hotfix record, optional branch creation, minimal implementation, QA re-entry, approval collection, and post-deploy verification plan."
argument-hint: "[bug-id or description] [--branch] [--implement] [--dry-run] [--base release|main|tag]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Bash, Task, AskUserQuestion
---

# Hotfix

Emergency workflow for an S1/S2 defect that must bypass normal sprint scheduling while preserving an audit trail.

This skill must only run on explicit `/hotfix` invocation. It may prepare records and coordinate implementation. It must not deploy, merge, tag a release, or publish external communication. It prepares a hotfix for deployment through the release process.

---

## 0. Operating Contract

### Autonomy defaults

- Triage severity automatically from the supplied bug ID or description.
- Create the hotfix record automatically for S1/S2 unless `--dry-run` is set.
- Create a branch only if `--branch` is supplied, the repository is clean, and the base is unambiguous.
- Implement only if `--implement` is supplied or the user explicitly approves implementation.
- Run targeted tests automatically when a safe command or test file is known.
- Ask only when severity is below S2, base branch is ambiguous, repository is dirty, implementation is not explicitly authorized, or deployment-like actions are requested.

### Hard boundaries

Never:

- Deploy to production.
- Merge to release or development branches.
- Create release tags.
- Rewrite history.
- Delete files.
- Expand scope beyond the emergency fix.
- Add unrelated cleanup/refactoring.
- Close the original bug before deployed verification.

### Protected operations requiring confirmation

Ask before:

- Proceeding with S3/S4 as a hotfix.
- Creating a branch when uncommitted changes exist.
- Implementing without `--implement`.
- Touching save data, platform certification, authentication, anti-cheat, monetization, or security-sensitive files.
- Continuing if the fix exceeds four hours or requires architectural change.

---

## 1. Parse Invocation

Arguments:

- Bug ID, bug file path, or free-text description.
- `--branch` — create a local hotfix branch if safe.
- `--implement` — perform or delegate the minimum viable fix.
- `--dry-run` — produce triage, plan, and record preview only.
- `--base release|main|tag` — branch base hint.

If no bug or description is provided, ask for it.

---

## 2. Load Bug and Release Context

If the argument looks like a bug ID, search:

```text
production/qa/bugs/*.md
production/bugs/*.md
production/issues/*.md
```

Read matching bug report(s). If multiple match, ask the user to select one.

Also read when present:

```text
production/stage.txt
production/releases/**
production/gate-checks/**
production/qa/**
production/security/security-audit-*.md
production/session-state/active.md
AGENTS.md
.claude/docs/technical-preferences.md
```

Extract:

- Severity and priority.
- Reproduction steps.
- Player impact.
- Affected systems/files.
- Known root cause.
- Current release branch/tag if available.
- Target platforms.
- Existing QA evidence.

If the bug report is missing severity, infer severity conservatively and ask only if S2 vs S3 is ambiguous.

---

## 3. Severity Triage

Severity definitions:

| Severity | Meaning | Hotfix default |
|---|---|---|
| S1 Critical | Game unplayable, data loss, exploit/security issue, crash on critical path | Hotfix immediately |
| S2 Major | Significant feature broken, severe player impact, workaround exists | Hotfix within 24h or next urgent patch |
| S3 Moderate | Noticeable issue, workaround available, non-critical | Normal bug workflow |
| S4 Minor | Cosmetic/minor annoyance | Normal backlog |

If severity is S3/S4, recommend normal workflow and stop unless the user explicitly chooses to continue as an exception.

Escalate to `security-engineer` immediately if the issue involves:

- Player data.
- Authentication.
- Payment/monetization.
- Anti-cheat.
- Multiplayer exploit.
- Remote code execution, injection, or data corruption.

---

## 4. Create Hotfix Record

Target path:

```text
production/hotfixes/hotfix-[YYYY-MM-DD]-[slug].md
```

Create directory if needed.

Record template:

```markdown
# Hotfix: [Short Description]

**Date opened**: [date]
**Severity**: [S1/S2]
**Priority**: [P1/P2/unknown]
**Reporter**: [source]
**Status**: In Progress
**Bug reference**: [bug ID/path or inline description]
**Base build/branch**: [release tag/branch/main/unknown]
**Hotfix branch**: [branch or pending]

## Problem

[player impact and reproduction summary]

## Scope Constraint

Minimum viable fix only. No feature work, no cleanup, no refactor.

## Root Cause

[known / to be determined]

## Fix Plan

[planned minimal change]

## Changed Files

[pending]

## Testing

[pending]

## QA Re-Entry

[pending]

## Approvals

- [ ] lead-programmer: fix reviewed
- [ ] qa-tester: targeted regression passed
- [ ] qa-lead: QA re-entry scope approved
- [ ] producer: deployment timing/communication approved
- [ ] security-engineer: required only if security-sensitive

## Rollback Plan

[how to revert if hotfix causes a regression]

## Deployment Notes

This skill does not deploy. Merge/deploy through release process after approval.
```

Write automatically unless `--dry-run`.

---

## 5. Optional Branch Creation

Run only if `--branch` is supplied.

### 5.1 Safety checks

Use Bash diagnostics:

```bash
git rev-parse --is-inside-work-tree
git status --short
git branch --show-current
```

If not a git repository, skip branch creation and record that no branch was created.

If working tree is dirty, ask before branching.

Resolve base:

1. `--base` value.
2. Release branch from release records.
3. Current branch if it appears to be release/main and user confirms if ambiguous.

### 5.2 Create branch

Branch name:

```text
hotfix/[slug]
```

If branch exists, use it and do not recreate.

Command:

```bash
git checkout -b hotfix/[slug] [base]
```

After branch creation, update hotfix record with branch name.

---

## 6. Investigate and Implement

Run implementation only when `--implement` is supplied or user approves.

### 6.1 Investigation

Read affected files and search callers/dependencies with `Grep`.

Classify the fix:

| Fix type | Handling |
|---|---|
| Config/data-only | Apply directly if target file is clear. |
| Isolated code | Delegate or implement minimal patch. |
| Cross-system code | Spawn lead-programmer and qa-lead before changes. |
| Security-sensitive | Spawn security-engineer before changes. |
| Architecture/schema | Stop and recommend emergency ADR or day-one/patch release process. |

### 6.2 Implementation rules

- Change the fewest files possible.
- Keep behavior change limited to the reproduction case and direct cause.
- Add or update targeted regression test when feasible.
- Do not perform cleanup, formatting-only changes, or unrelated test rewrites.
- If the fix grows beyond four hours, stop and escalate to `technical-director`.

Use `lead-programmer` for code changes unless the change is a trivial config/data edit.

Pass to `lead-programmer`:

- Bug report.
- Hotfix record path.
- Affected files.
- Minimal-change constraint.
- Target tests.
- Rollback requirement.

---

## 7. Targeted Tests

Identify tests from:

- Bug report.
- Changed system.
- `tests/unit/` and `tests/integration/` paths.
- Existing smoke check definitions.

Run targeted tests automatically when command is clear and bounded.

If no automated test exists, require one of:

- Manual reproduction verification by `qa-tester`.
- Evidence doc in `production/qa/evidence/`.
- Targeted smoke check result.

Record all test commands and outcomes in the hotfix record.

---

## 8. Approval Collection

Spawn in parallel when implementation is complete or when a deploy-ready plan is needed:

- `lead-programmer` — correctness, side effects, minimality.
- `qa-tester` — reproduction no longer occurs and adjacent regression checks pass.
- `producer` — deployment timing and player communication.
- `security-engineer` — only if security-sensitive.

Approval mapping:

| Response | Effect |
|---|---|
| APPROVE | Continue. |
| CONCERNS | Stop and present concern with options. |
| REJECT | Do not proceed to deploy-ready summary. |

Do not suppress concerns. A hotfix with unresolved approval concerns is not deploy-ready.

---

## 9. QA Re-Entry Gate

Spawn `qa-lead` with:

- Hotfix description.
- Changed files.
- Systems touching changed files.
- Test results.
- Approval results.
- Severity.

Ask for required re-entry scope:

| QA scope | Meaning |
|---|---|
| Targeted smoke | Only affected critical path. |
| Targeted team QA | Affected system requires broader manual/automated pass. |
| Full QA | Core, save, platform, security, or cross-system risk. |

Run feasible targeted checks. If qa-lead requires a full QA pass, do not simulate it; state the required command/workflow.

Hotfix is deploy-ready only if QA re-entry returns PASS/APPROVED or APPROVED WITH CONDITIONS and conditions are recorded.

---

## 10. Update Bug Report

If a bug file exists and hotfix reached deploy-ready state:

- Set status to `Fixed — Pending Verification`.
- Append `## Fix Record` if absent.
- Record hotfix branch, changed files, tests, approval status, and rollback plan.

Do not close the bug.

If implementation did not occur, write a plan-only note instead.

---

## 11. Deploy-Ready Summary

Output:

```markdown
## Hotfix Deploy-Ready Summary: [slug]

**Severity**: [S1/S2]
**Status**: [Deploy-ready / Blocked / Plan only]
**Hotfix Record**: [path]
**Branch**: [branch or not created]
**Root Cause**: [one line]
**Fix**: [one line]
**Changed Files**: [list]
**Tests**: [summary]
**QA Re-Entry**: [scope + verdict]
**Approvals**: lead-programmer [✓/✗], qa-tester [✓/✗], qa-lead [✓/✗], producer [✓/✗], security [✓/not required]
**Rollback Plan**: [summary]

### Not performed by this skill

- Deployment
- Merge to release branch
- Merge/backport to development branch
- Release tag creation
- External communication

### Required next steps

1. Merge through the approved release process.
2. Deploy through `/team-release` or the established release pipeline.
3. After live deployment, run `/bug-report verify [BUG-ID]`.
4. If verified, run `/bug-report close [BUG-ID]`.
5. Run `/retrospective hotfix` within 48 hours for S1 or high-impact S2 fixes.
```

Verdicts:

- `DEPLOY-READY` — implementation, tests, approvals, and QA re-entry are complete.
- `PLAN-ONLY` — record/plan created but implementation not performed.
- `BLOCKED` — severity inappropriate, root cause unknown, tests failed, approval rejected, or QA re-entry failed.
