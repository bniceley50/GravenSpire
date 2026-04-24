---
name: team-release
description: "Orchestrate release readiness, release candidate preparation, quality gates, go/no-go, protected deployment coordination, and post-release monitoring through specialist subagents."
argument-hint: "[version|next] [plan|candidate|go-no-go|deploy|monitor] [--dry-run] [--no-deploy]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Bash, Task, AskUserQuestion, TodoWrite
---

# Team Release

Coordinate the release team from release planning through go/no-go and deployment readiness. This skill orchestrates specialists and writes release records. It must not deploy externally unless the user explicitly invokes deployment mode and all gates are green.

---

## 0. Operating Contract

### Autonomy defaults

- Infer version when possible.
- Run release planning, evidence collection, and specialist sign-offs autonomously.
- Spawn independent specialists in parallel where possible.
- Write release reports/checklists automatically unless `--dry-run` is active.
- Ask only when version is ambiguous, a gate is blocked, or protected deployment/merge/tag actions are requested.

### Hard boundaries

Never without explicit deployment-mode approval:

- Deploy to production.
- Publish store/community posts.
- Merge branches.
- Create release tags.
- Upload builds to platform stores.
- Change production infrastructure.
- Mark release Complete after a NO-GO.

### Protected deployment rule

Deployment actions require all of the following:

1. Invocation includes `deploy` and does not include `--no-deploy`.
2. Producer verdict is GO.
3. QA verdict is APPROVED or APPROVED WITH CONDITIONS.
4. Release-manager and devops-engineer sign off.
5. Security/network sign-off is complete when applicable.
6. The exact deployment commands or sub-skill actions are displayed.
7. User confirms deployment.

If any condition is missing, produce a deploy-readiness report only.

---

## 1. Parse Invocation

Arguments:

- Version: explicit version such as `v1.0.0`, or `next`.
- Mode:
  - `plan` — release plan only.
  - `candidate` — prepare/check release candidate.
  - `go-no-go` — run gates and make release decision.
  - `deploy` — protected deployment coordination after GO.
  - `monitor` — post-release monitoring summary.
  - omitted — run through go/no-go, but do not deploy.
- `--dry-run` — no writes, no branch/tag/deploy commands.
- `--no-deploy` — do not deploy even if mode is deploy.

If no version is supplied:

1. Read `production/session-state/active.md`.
2. Read latest `production/milestones/` and `production/releases/` files.
3. Read `AGENTS.md` and release checklist files.
4. Infer version if exactly one candidate is clear.
5. If ambiguous, ask with `AskUserQuestion`.

Use `TodoWrite` to track the release phases and mark them complete/blocked as the workflow proceeds.

---

## 2. Load Release Context

Read or inspect:

```text
production/stage.txt
production/milestones/**
production/releases/**
production/gate-checks/**
production/qa/**
production/qa/bugs/*.md
production/sprints/**
design/gdd/**
docs/architecture/**
.claude/docs/technical-preferences.md
AGENTS.md
.github/workflows/**
```

Extract:

- Current stage.
- Target version.
- Target platforms.
- Milestone scope.
- Deferred scope.
- Release checklist status.
- QA sign-off status.
- Open bugs by severity.
- Test and CI status.
- Performance/profile reports.
- Localization status.
- Security requirements.
- Multiplayer/online/player-data presence.
- Analytics/telemetry requirements.
- Known issues planned for release notes.

If current stage is earlier than Polish, report that release is premature and continue only as planning unless the user explicitly confirms.

---

## 3. Team Composition and Delegation

Spawn specialists with full context, not isolated prompts.

Core specialists:

| Specialist | Responsibility |
|---|---|
| `producer` | Scope, schedule, go/no-go, stakeholder decision. |
| `release-manager` | Branching, versioning, changelog, release checklist, release record. |
| `qa-lead` | Regression suite, critical path sign-off, bug severity review. |
| `devops-engineer` | Build reproducibility, CI, artifacts, deployment mechanics. |
| `technical-director` | Technical risk, architecture stability, blocker assessment. |
| `community-manager` | Patch notes, launch communication, known issues post. |
| `analytics-engineer` | Telemetry events, dashboards, critical funnels. |

Conditional specialists:

| Condition | Specialist |
|---|---|
| Online, multiplayer, accounts, player data, anti-cheat, payments | `security-engineer` |
| Multiplayer/networked systems | `network-programmer` |
| Localization targets exist | `localization-lead` if available; otherwise handle locally |
| Performance risk or platform-specific budgets | `performance-analyst` if available; otherwise handle locally |

If a named specialist is unavailable, continue with local checks and record the gap.

---

## 4. Phase 1 — Release Planning

Spawn `producer` and `release-manager` in parallel.

Producer checks:

- Milestone acceptance criteria complete.
- Deferred scope is explicit.
- Target release date and platform timing known.
- Stakeholder risks identified.
- Release is appropriate for current stage.

Release-manager checks:

- Version number validity.
- Existing release branch/tag status.
- Release checklist presence.
- Changelog source data.
- Known release process.

Output:

```markdown
## Release Planning

| Field | Value |
|---|---|
| Version | [version] |
| Scope | [summary] |
| Deferred Items | [summary] |
| Target Date | [date/unknown] |
| Release Branch | [branch/pending] |
| Planning Verdict | [READY/CONCERNS/BLOCKED] |
```

If planning is BLOCKED, stop after writing a partial report.

---

## 5. Phase 2 — Release Candidate Readiness

Do not create branches/tags automatically unless explicitly authorized by the sub-skill or deployment mode rules. This orchestrator primarily verifies readiness.

Checks:

- Release candidate build exists or can be produced.
- Version numbers are consistent across config files.
- CI workflow exists and latest run evidence is available if recorded.
- Release checklist exists or is generated by `/release-checklist` through the release-manager.
- Branch freeze rules are documented.
- No feature changes remain scheduled for the candidate branch.

Spawn `devops-engineer` for build/reproducibility review and `release-manager` for branch/version review.

If mode is `candidate`, stop after candidate report unless the user wants go/no-go.

---

## 6. Phase 3 — Quality, Security, Build, and Technical Gates

Spawn independent gates in parallel:

- `qa-lead`
- `devops-engineer`
- `technical-director`
- `security-engineer` if applicable
- `network-programmer` if applicable

Pass each:

- Target version.
- Release scope.
- Known open bugs.
- QA reports and smoke/regression reports.
- Build/CI evidence.
- Architecture and high-risk systems.
- Target platforms.

### 6.1 QA gate

qa-lead must verify:

- Full regression or required release test suite status.
- Critical path smoke test PASS.
- No unresolved S1/S2 bugs.
- S3+ known issues are documented.
- Must Have story evidence is present.

### 6.2 DevOps gate

DevOps must verify:

- Build artifacts are reproducible.
- CI/test pipeline is green or exceptions recorded.
- Target platform builds exist.
- Deployment pipeline is known and reversible.
- Secrets/credentials are not exposed.

### 6.3 Technical gate

Technical Director must verify:

- No unresolved architectural blockers.
- Engine/version risk is understood.
- Hotfix/day-one-patch dependencies are not blocking release.
- Performance and stability risks are acceptable.

### 6.4 Security/network gates

Security/network gates are blocking when applicable. Missing security sign-off for online/player-data features is at least CONCERNS and may be FAIL depending on risk.

---

## 7. Phase 4 — Localization, Performance, Analytics, Communication

Run these in parallel after or alongside Phase 3 if sufficient evidence exists.

### 7.1 Localization

Check:

- Player-facing strings externalized.
- Target language files exist.
- Build includes fallback behavior.
- No obvious hardcoded strings in implementation paths.

### 7.2 Performance

Check:

- Perf budgets from `.claude/docs/technical-preferences.md`.
- Latest `/perf-profile` report if available.
- Platform-specific performance risks.

### 7.3 Analytics

Spawn `analytics-engineer` if telemetry exists or is required.

Verify:

- Critical events fire.
- Dashboards are configured.
- Launch-health alerts exist or are planned.
- Privacy/legal constraints are respected.

### 7.4 Community communication

Spawn `community-manager` to draft or verify:

- Patch notes.
- Known issues post.
- Launch announcement.
- Player support escalation route.

Do not publish externally.

---

## 8. Phase 5 — Go/No-Go Decision

Spawn `producer` with all gate results.

Required sign-offs for GO:

- Producer: GO.
- QA: APPROVED or APPROVED WITH CONDITIONS.
- Release-manager: READY.
- DevOps: READY.
- Technical Director: READY or CONCERNS accepted.
- Security: READY if applicable.
- Network: READY if applicable.

Automatic NO-GO conditions:

- Any unresolved S1 bug.
- Any unresolved S2 bug without explicit known-issue acceptance and workaround.
- QA FAIL.
- Build cannot be produced for a target platform.
- Security blocker for online/player-data features.
- No rollback or deployment recovery plan.
- Producer NO-GO.

If NO-GO:

1. Surface the blocker immediately.
2. Write partial report.
3. Do not run deployment phase.
4. Offer options:
   - Fix blocker and rerun affected phase.
   - Defer release.
   - Document override rationale.

Overrides require user-written rationale and remain `RELEASE OVERRIDE`, not clean GO.

---

## 9. Phase 6 — Protected Deployment Coordination

Run only in `deploy` mode and only if protected deployment rule conditions are met.

Before any deployment-like Bash command or sub-skill call:

1. Show exact command/action.
2. Show target environment.
3. Show rollback command/procedure.
4. Show last GO verdict timestamp.
5. Ask for confirmation.

Deployment tasks may include:

- Release tag creation.
- Staging deployment.
- Final smoke test on staging.
- Production deployment or platform upload.
- Publishing release communication.

If the project uses dedicated sub-skills such as `/changelog`, `/patch-notes`, or `/launch-checklist`, call those workflows conceptually by delegating to the appropriate subagent; do not silently write external-facing content.

If any deployment step fails, stop and run Error Recovery Protocol.

---

## 10. Phase 7 — Post-Release Monitoring

Run in `monitor` mode or after protected deployment completes.

Coordinate:

- `qa-lead`: incoming bugs and regressions.
- `analytics-engineer`: dashboard health and missing events.
- `community-manager`: community sentiment and known issues.
- `producer`: stakeholder update.
- `release-manager`: release report.

Write or preview:

```text
production/releases/release-report-[version].md
```

Monitoring window default: 48 hours.

---

## 11. Error Recovery Protocol

If any specialist returns BLOCKED, errors, or cannot complete:

1. Surface immediately: `[Specialist]: BLOCKED — [reason]`.
2. Identify dependent phases.
3. Stop before dependent phase.
4. Offer options:
   - Retry with narrower scope.
   - Skip and record gap.
   - Stop and resolve blocker.
5. Always produce a partial report.

Common blockers and redirects:

| Blocker | Redirect |
|---|---|
| Missing release checklist | `/release-checklist` |
| QA not approved | `/team-qa` |
| Smoke check missing | `/smoke-check` |
| Open S1/S2 bugs | `/hotfix` or `/bug-triage` |
| Performance unknown | `/perf-profile` |
| Security not reviewed | `/security-audit` |
| Patch notes missing | `/patch-notes` |
| Launch checklist missing | `/launch-checklist` |

---

## 12. Release Report

Unless `--dry-run`, write:

```text
production/releases/release-orchestration-[version]-[YYYY-MM-DD].md
```

Template:

```markdown
# Release Orchestration: [version]

**Date**: [date]
**Mode**: [plan/candidate/go-no-go/deploy/monitor]
**Verdict**: [GO / NO-GO / GO WITH CONDITIONS / PLAN ONLY / DEPLOYED / BLOCKED]
**Dry Run**: [yes/no]

## Scope

[summary]

## Planning

| Check | Result | Notes |
|---|---|---|

## Release Candidate

| Check | Result | Notes |
|---|---|---|

## Quality Gates

| Gate | Verdict | Evidence | Notes |
|---|---|---|---|

## Localization / Performance / Analytics / Communication

| Area | Verdict | Evidence | Notes |
|---|---|---|---|

## Go/No-Go

**Decision**: [GO/NO-GO/GO WITH CONDITIONS]
**Producer rationale**: [rationale]
**Conditions**: [list or None]

## Deployment

**Deployment attempted**: [yes/no]
**Deployment status**: [not requested / blocked / staging / production / failed]
**Commands/actions performed**: [list or None]

## Open Risks

[list or None]

## Required Follow-Up

[numbered list]
```

---

## 13. Stage Update

After a successful deployment, updating `production/stage.txt` to `Live` is protected.

If deployment completed and user confirms:

- Write `Live` to `production/stage.txt`.

If deployment did not occur, do not update stage to Live.

---

## 14. Closing Output

End with:

```markdown
## Release Orchestration Summary

**Version**: [version]
**Mode**: [mode]
**Verdict**: [verdict]
**Report**: [path or dry-run]
**Deployment Performed**: [yes/no]
**Stage Updated**: [yes/no]

### Next action

[one specific command or action]
```
