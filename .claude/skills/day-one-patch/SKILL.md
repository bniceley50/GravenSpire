---
name: day-one-patch
description: "Scope, prepare, implement, and QA-gate a day-one patch as a constrained mini-sprint with rollback plan, bug deferrals, patch record, and deploy-readiness evidence."
argument-hint: "[scope: known-bugs | cert-feedback | all] [--implement] [--dry-run] [--version vX.Y.Z]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Bash, Task, AskUserQuestion
---

# Day-One Patch

Prepare a day-one patch for a project approaching launch. A day-one patch is a bounded fix package, not a feature sprint and not a broad refactor. Its purpose is to address launch-critical issues found after the gold master or release candidate is locked.

Invoking this skill authorizes autonomous discovery, classification, planning, rollback planning, patch record creation, and QA scoping. Code/data implementation proceeds only for approved in-scope fixes or when `--implement` is supplied. Deployment, publishing, platform submission, or external communication are never performed by this skill.

---

## 0. Operating Contract

### Autonomy defaults

- Load release, bug, QA, sprint, and security context automatically.
- Classify bugs into Include / Defer / Exclude automatically using the policy below.
- Write the patch plan, rollback plan, and patch record automatically after scope approval unless `--dry-run` is set.
- Use subagents for implementation and QA where appropriate.
- Ask only when scope is risky, ambiguous, too large, or includes implementation without `--implement`.

### Hard boundaries

Never:

- Add new features.
- Refactor unrelated code.
- Change architecture.
- Ship or deploy the patch.
- Merge branches or tag releases.
- Publish patch notes externally.
- Include a fix estimated above four hours unless the user explicitly overrides and the risk is recorded.

### Protected operations requiring confirmation

Ask before:

- Implementing code fixes when `--implement` is absent.
- Including a fix that touches architecture, save data, networking, monetization, platform integration, or security.
- Proceeding when estimated patch effort exceeds one workday.
- Removing a previously included S1/S2 bug from patch scope.
- Changing release-stage files outside `production/releases/`, `production/qa/bugs/`, or approved config/data fix targets.

---

## 1. Parse Invocation

Scope argument:

| Argument | Meaning |
|---|---|
| `known-bugs` | Use open bug reports only. |
| `cert-feedback` | Use certification/platform feedback only. |
| `all` or omitted | Consider bugs, cert feedback, QA findings, and security items. |

Options:

- `--implement` — approved scope may be implemented after rollback plan exists.
- `--dry-run` — produce plan only; write nothing.
- `--version vX.Y.Z` — target release/patch version.

If no version is supplied, infer from release checklist, milestone, `AGENTS.md`, latest release record, or ask only if inference fails.

---

## 2. Load Release Context

Read or inspect:

```text
production/stage.txt
production/gate-checks/**
production/releases/**
production/qa/bugs/*.md
production/qa/**
production/sprints/**
production/security/security-audit-*.md
docs/architecture/control-manifest.md
AGENTS.md
```

Extract:

- Current stage.
- Release gate verdict.
- Gold master / release candidate tag or commit.
- Target version and launch date, if known.
- Open bugs and Fixed — Pending Verification bugs.
- Certification feedback.
- Open security items.
- Recently shipped scope.
- Existing rollback or release plan.

Appropriateness check:

- If stage is `Release` or `Polish`, continue.
- If stage is earlier, continue only as a dry planning exercise and mark the report `NOT READY FOR DAY-ONE PATCH` unless the user explicitly confirms.

---

## 3. Classify Candidate Fixes

For each candidate issue, extract:

- ID.
- Severity `S1`–`S4`.
- Priority `P1`–`P4`.
- Player impact.
- Reproduction reliability.
- Root cause known/unknown.
- Affected files/systems.
- Estimated fix effort.
- Risk class.
- Cert/security/platform relevance.

### 3.1 Inclusion policy

| Condition | Default decision |
|---|---|
| S1 or S2 and safe minimal fix | Include |
| P1 and safe minimal fix | Include |
| Certification blocker | Include |
| Security issue affecting player data, auth, integrity, or online safety | Include, but require security review |
| Data/config-only fix | Include if low risk |
| Fix estimated under 4 hours | Eligible |
| S3/S4 trivial config fix | Eligible |
| Requires architecture change | Defer to 1.1 or hotfix plan |
| Adds new code path | Defer unless S1 and explicitly approved |
| Requires broad refactor | Defer |
| Root cause unknown | Investigate only; do not include until understood |
| Estimate over 4 hours | Defer unless explicit override |

### 3.2 Risk classes

| Risk | Meaning | Day-one default |
|---|---|---|
| LOW | Config/data/test-only or isolated bug fix | Include if important |
| MEDIUM | Code fix in local subsystem with targeted tests | Include if S1/S2/P1 |
| HIGH | Cross-system, save/load, networking, platform, monetization, security | Require explicit approval and broader QA |
| EXTREME | Architecture, schema migration, release pipeline, platform submission logic | Defer unless release is impossible without it |

---

## 4. Produce Scope Proposal

Output a scope table:

```markdown
## Proposed Day-One Patch Scope

### Included
| ID | Severity | Priority | System | Summary | Effort | Risk | Reason |
|---|---|---|---|---|---:|---|---|

### Deferred to 1.1
| ID | Severity | Priority | Summary | Reason Deferred |
|---|---|---|---|---|

### Excluded / Already Resolved
| ID | Status | Reason |
|---|---|---|
```

Compute:

- Total estimated effort.
- Number of S1/S2 issues included and deferred.
- Highest risk class.
- Systems touched.
- QA breadth required.

If total effort exceeds one workday, or any HIGH/EXTREME item is included, ask the user to approve or reduce scope.

If scope is safe and `--implement` was supplied, proceed after showing the scope.

If `--implement` was not supplied, ask:

```text
Proceed with this day-one patch scope?
```

Options:

- `Approve scope and prepare records only`
- `Approve scope and implement fixes`
- `Adjust scope`
- `No day-one patch needed`

Stop if no day-one patch is needed.

---

## 5. Create Rollback Plan First

A rollback plan is required before implementation.

Spawn `release-manager` with:

- Target version.
- Base build / gold master.
- Included fixes and risk classes.
- Target platforms.
- Release pipeline facts from repository.

Ask for rollback plan covering:

- How to revert to the gold master or prior release.
- Platform-specific rollback constraints.
- Who triggers rollback.
- Communication required.
- Data/save compatibility considerations.
- Conditions that trigger rollback.

If online, multiplayer, player data, anti-cheat, or security-sensitive systems are touched, also spawn `security-engineer` to review rollback and exploit/player-data implications.

Write rollback plan unless `--dry-run`:

```text
production/releases/rollback-plan-[version].md
```

Do not proceed to implementation without a rollback plan, unless the user explicitly chooses records-only mode.

---

## 6. Implement Approved Fixes

Run only when implementation is approved and not `--dry-run`.

### 6.1 Per-fix implementation loop

For each included issue:

1. Read the bug report and affected files.
2. Confirm root cause is known or investigate with minimal file reads.
3. Classify fix type:
   - Config/data-only.
   - Local code fix.
   - Cross-system code fix.
   - Security/platform fix.
4. Apply the minimum viable fix.
5. Run targeted tests.
6. Record changed files and test output.

For local code fixes, spawn `lead-programmer` with:

- Bug report.
- Affected files.
- Constraint: minimum viable fix only.
- No cleanup, no refactor, no feature work.
- Required targeted tests.

For config/data-only fixes, apply directly if the target path is listed in the bug report or release context. If target path is unclear, ask.

For security-sensitive fixes, spawn `security-engineer` before implementation and again for review.

### 6.2 Scope control during implementation

If a fix expands beyond estimated effort, touches unexpected files, or requires architecture change:

- Stop that fix.
- Mark it deferred to 1.1.
- Continue with other included fixes if safe.
- Record the reason.

---

## 7. Verification and QA Gate

### 7.1 Per-fix verification

For every implemented fix:

- Run the bug's reproduction steps where possible.
- Run targeted tests for affected systems.
- Spawn `qa-tester` for verification if the fix is not purely data/config.
- Update bug status to `Fixed — Pending Verification` only after QA verification passes.

### 7.2 Patch-level QA scope

Spawn `qa-lead` with:

- Included issues.
- Deferred issues.
- Changed files.
- Test results.
- Systems touched.
- Risk classes.
- Rollback plan path.

Ask whether QA scope is:

| Scope | Use when |
|---|---|
| Targeted smoke | LOW risk, isolated fixes |
| Targeted regression | MEDIUM risk or multiple related fixes |
| Broad regression | HIGH risk or cross-system changes |
| Full QA re-entry | EXTREME risk, save/platform/security/core loop |

Run the required tests if available. If QA returns FAIL, remove the failing fix from the day-one patch and defer it unless the user explicitly chooses to continue with documented risk.

Proceed only if patch QA verdict is PASS or PASS WITH WARNINGS.

---

## 8. Generate Patch Record

Create or preview:

```text
production/releases/day-one-patch-[version].md
```

Template:

```markdown
# Day-One Patch: [Game Name] [version]

**Date prepared**: [date]
**Target release**: [launch date or day of launch]
**Base build**: [gold master tag/commit]
**Patch build**: [patch tag/commit or pending]
**Scope mode**: [known-bugs/cert-feedback/all]
**Implementation mode**: [records only / implemented]

## Summary

[one paragraph]

## Bugs Fixed

| Bug ID | Severity | Priority | System | Fix Summary | Changed Files | Verification |
|---|---|---|---|---|---|---|

## Deferred to 1.1

| Bug ID | Severity | Priority | Reason Deferred | Follow-Up |
|---|---|---|---|---|

## QA Sign-Off

| Field | Value |
|---|---|
| QA Scope | [targeted smoke / regression / full] |
| Verdict | [PASS / PASS WITH WARNINGS / FAIL] |
| Evidence | [paths / command output summary] |
| Warnings | [list or None] |

## Rollback Plan

See: `production/releases/rollback-plan-[version].md`

**Rollback trigger**: [trigger]
**Rollback owner**: [owner]

## Approvals Required Before Deploy

- [ ] lead-programmer: fixes reviewed
- [ ] qa-lead: QA gate PASS confirmed
- [ ] producer: deployment timing approved
- [ ] release-manager: platform submission confirmed
- [ ] security-engineer: security sign-off, if applicable

## Player-Facing Patch Notes Draft

[plain-language summary for `/patch-notes` or community-manager review]

## Risks Accepted

[list or None]
```

Write automatically unless `--dry-run`.

---

## 9. Update Bug Reports

For each implemented and verified bug:

- Append `## Fix Record` if absent.
- Set status to `Fixed — Pending Verification`.
- Record patch version and verification evidence.

Do not close bugs. Closure requires post-launch verification through `/bug-report verify` and `/bug-report close`.

For deferred bugs:

- Append or update `Deferred to 1.1` note.
- Preserve original severity.
- Do not lower severity solely because the issue was deferred.

---

## 10. Closing Output

End with:

```markdown
## Day-One Patch Summary

**Version**: [version]
**Mode**: [records only / implemented]
**Patch Record**: [path or dry-run]
**Rollback Plan**: [path or dry-run]
**Included Fixes**: [N]
**Deferred Fixes**: [N]
**QA Verdict**: [PASS / PASS WITH WARNINGS / FAIL / not run]
**Deploy Status**: Not deployed by this skill

### Required next steps

1. `/patch-notes [version]` — finalize player-facing notes.
2. Final producer/release-manager approval.
3. Deploy through the release pipeline, not through this skill.
4. After live deployment, run `/bug-report verify [BUG-ID]` for each fixed bug.
5. Schedule `/retrospective launch` 48–72 hours after launch.
```

If any S1 bugs remain deferred, include a warning and add them to rollback trigger considerations.
