---
name: story-done
description: "End-of-story completion review. Verifies acceptance criteria, test evidence, GDD/ADR compliance, QA/code-review gates, then updates the story and sprint state when completion is justified."
argument-hint: "[story-file-path] [--review full|lean|solo] [--dry-run] [--force-complete]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash, Write, Edit, AskUserQuestion, Task
---

# Story Done

Close an implementation story with evidence. This skill validates the story against its acceptance criteria, test evidence requirements, governing GDDs/ADRs, and implementation scope before marking it complete.

Invoking `/story-done` authorizes routine repo-local updates needed to close a story **only if** the final verdict is `COMPLETE` or `COMPLETE WITH NOTES`. It must never mark a `BLOCKED` story complete unless the user explicitly invokes `--force-complete` and a risk note is written.

---

## 0. Operating Contract

### Autonomy defaults

- Locate the active story automatically when no path is supplied.
- Run safe tests and grep checks automatically.
- Ask only for subjective/manual criteria that cannot be verified from files or tests.
- If verdict is `COMPLETE`, update the story, sprint status, and session state automatically unless `--dry-run` is set.
- If verdict is `COMPLETE WITH NOTES`, update automatically only when notes are advisory and not design/ADR contradictions.
- If verdict is `BLOCKED`, do not update status to Complete.

### Protected changes requiring confirmation

Ask before:

- Completing a story with accepted GDD/ADR deviations.
- Completing a story despite blocking test evidence gaps.
- Logging new tech debt items when the register already contains a conflicting entry.
- Editing any file outside the story, sprint status, session state, or tech-debt register.

### Path rules

A supplied story path must be repository-relative and must not be absolute or contain `..`.

Allowed story locations:

```text
production/epics/**/story-*.md
production/stories/**/*.md
production/sprints/**/*.md
```

If the path is outside these locations, read only after asking for confirmation that it is a story file.

---

## 1. Parse Invocation

Options:

- `--review full|lean|solo`
- `--dry-run`
- `--force-complete`

Resolve review mode once:

1. CLI `--review`.
2. `production/review-mode.txt`, if valid.
3. Default: `lean`.

Review mode behavior:

| Mode | Automated checks | QA coverage subagent | Lead programmer subagent |
|---|---|---|---|
| `solo` | Yes | No | No |
| `lean` | Yes | No | No |
| `full` | Yes | Yes | Yes |

---

## 2. Resolve Story File

If a path is supplied, validate and read it.

If no path is supplied, search in this order:

1. `production/session-state/active.md` for an active story path.
2. Most recent sprint file under `production/sprints/` for `IN PROGRESS` stories.
3. `production/epics/**/story-*.md` for `Status: In Progress`.
4. `production/epics/**/story-*.md` for `Status: Ready` if no in-progress story exists.

If exactly one likely story is found, proceed.

If multiple are found, ask the user to choose.

If none is found, ask for the story path.

---

## 3. Read Story and Dependency Context

Read the full story file and extract:

- Story title and ID.
- Status.
- Type: `Logic`, `Integration`, `Visual/Feel`, `UI`, `Config/Data`, or missing.
- Priority/MoSCoW tier.
- GDD requirement IDs / TR-IDs.
- Referenced GDD files and sections.
- ADR references.
- Control manifest version.
- Acceptance Criteria.
- QA Test Cases.
- Test Evidence section.
- Files to create/modify.
- Definition of Done.
- Estimate and actual time, if present.

Read supporting files when present:

```text
docs/architecture/tr-registry.yaml
docs/architecture/control-manifest.md
docs/registry/architecture.yaml
referenced ADR files
referenced GDD files
production/sprint-status.yaml
production/sprints/*
```

For each TR-ID, prefer `docs/architecture/tr-registry.yaml` as the current requirement text. Treat inline requirement text in the story as potentially stale.

For each ADR, read only the sections needed for validation:

- Status.
- Decision.
- Key Interfaces.
- Consequences.
- Registry Impact.
- Forbidden Patterns.

---

## 4. Determine Implementation Evidence

Build an implementation evidence set from:

1. Files listed in the story.
2. Files changed according to `git status --short` if git is available.
3. Test paths listed in the story.
4. Test files matching story/system keywords under `tests/unit/` and `tests/integration/`.
5. Evidence docs under `production/qa/evidence/` and `tests/evidence/`.

If git is available, run only:

```bash
git status --short
git diff --name-only HEAD
```

If there is no implementation evidence and the story is not Config/Data or documentation-only, mark BLOCKED.

---

## 5. Verify Acceptance Criteria

For each acceptance criterion, assign an ID `AC-1`, `AC-2`, etc.

### 5.1 Automatic verification

Use automatic checks for objective criteria:

| Criterion type | Verification |
|---|---|
| File exists | Glob target path |
| Test passes | Run specific test path or targeted test command |
| No hardcoded gameplay constants | Grep implementation files for suspicious numeric literals |
| No hardcoded player-facing strings | Grep implementation files for string literals when localization expected |
| Dependency exists | Read/Glob referenced artifact |
| API/interface exists | Grep implementation files for exact method/signal/class |
| Data/config changed | Read target data/config file |

Run targeted tests automatically when a test file or command is clear. Avoid full-project test suites unless they are the project's normal fast test command or the user approved them earlier in the session.

### 5.2 Manual verification

For criteria involving feel, subjective quality, visual appearance, animation correctness, playability, or full-build behavior, batch questions with `AskUserQuestion`.

Use options:

- `Passes`
- `Fails`
- `Not tested yet`

Do not mark `Not tested yet` as PASS.

### 5.3 Deferred verification

Mark a criterion `DEFERRED` only when it genuinely requires a later playtest/build session and is not required by the story's Definition of Done.

Deferred criteria prevent `COMPLETE` but may allow `COMPLETE WITH NOTES` if the story type permits advisory manual evidence.

### 5.4 Acceptance criterion status

Each criterion status must be one of:

| Status | Meaning |
|---|---|
| PASS | Verified by file, test, or manual confirmation. |
| FAIL | Verified not true. |
| UNTESTED | No evidence. |
| DEFERRED | Requires later build/playtest and is not immediately testable. |

Any FAIL criterion normally produces `BLOCKED`.

---

## 6. Test-Criterion Traceability

For every acceptance criterion, identify direct coverage:

- Unit test.
- Integration test.
- Manual confirmation.
- Evidence document.
- Smoke report.

Output table:

```markdown
| Criterion | Verification | Coverage Status |
|---|---|---|
| AC-1: [text] | tests/unit/combat/damage_test.gd::test_base_damage | COVERED |
| AC-2: [text] | Manual confirmation | COVERED |
| AC-3: [text] | — | UNTESTED |
```

Escalation:

- More than 50% UNTESTED: BLOCKED.
- Any Logic story with no unit test evidence: BLOCKED.
- Any Integration story with no integration or playtest evidence: BLOCKED.
- Visual/Feel, UI, and Config/Data evidence gaps are advisory unless the story explicitly requires them.

---

## 7. Story-Type Evidence Gate

Use the story's `Type:` field.

| Story Type | Required Evidence | Gate Level |
|---|---|---|
| Logic | Passing automated unit test under `tests/unit/[system]/` | BLOCKING |
| Integration | Passing integration test under `tests/integration/[system]/` or playtest/session log | BLOCKING |
| Visual/Feel | Screenshot/video/sign-off doc under evidence path | ADVISORY unless explicitly required |
| UI | Manual walkthrough, interaction test, or sign-off doc | ADVISORY unless explicitly required |
| Config/Data | Smoke report or config validation evidence | ADVISORY unless explicitly required |
| Missing | Add Type field recommendation | ADVISORY |

Read the story's `Test Evidence` section first and check exact paths. Then search broadly if the exact path is missing.

For blocking evidence gaps, do not allow `COMPLETE`.

---

## 8. Deviation Checks

Compare implementation evidence against current design constraints.

### 8.1 GDD/TR compliance

For each TR-ID:

- Read current registry requirement text.
- Read referenced GDD snippet if available.
- Check implementation files for expected functions, data, components, values, or interfaces.
- Flag contradictions.

### 8.2 ADR compliance

For each referenced ADR:

- Verify ADR status is `Accepted` or explicitly allowed for implementation.
- Check implementation against Decision and Key Interfaces.
- Grep for forbidden patterns from ADRs and control manifest.
- Check state ownership and interface contracts from registry if available.

If a story implements against a `Proposed` ADR, mark BLOCKED unless the story explicitly says prototype-only or the user approves an override.

### 8.3 Manifest staleness

Compare story manifest version to `docs/architecture/control-manifest.md` header.

- Match: pass.
- Story older than manifest: advisory or blocking depending on whether the changed manifest affects this story's domain.
- Manifest missing: advisory.

### 8.4 Scope check

Compare files changed or implemented against the story's declared file list.

Classify extra files:

| Class | Meaning |
|---|---|
| IN SCOPE | Listed or necessary dependency. |
| OUT OF SCOPE | Touched but not obviously related. |
| SCOPE CREEP | Adds feature/work beyond story. |

Scope creep is advisory unless it violates GDD/ADR or creates unreviewed feature behavior.

---

## 9. Subagent Review Gates

Run only in `full` mode.

### 9.1 QA coverage gate

Skip for Config/Data unless tests were explicitly required.

Spawn `qa-lead` using `QL-TEST-COVERAGE` from `.claude/docs/director-gates.md` if available.

Pass:

- Story path.
- Story type.
- Acceptance criteria.
- QA Test Cases.
- Test files found.
- Evidence docs found.
- Draft traceability table.

Verdict mapping:

| QA verdict | Effect |
|---|---|
| ADEQUATE | Continue. |
| GAPS | Advisory unless Logic/Integration coverage is missing. |
| INADEQUATE | BLOCKED. |

If director-gates config is missing, record the gate as unavailable and continue with local evidence rules.

### 9.2 Lead programmer code review gate

Spawn `lead-programmer` using `LP-CODE-REVIEW` if implementation files exist.

Pass:

- Implementation file paths.
- Story file path.
- Relevant GDD requirement text.
- Governing ADR decisions.
- Test results.
- Scope/deviation findings.

Verdict mapping:

| Code review verdict | Effect |
|---|---|
| APPROVE | Continue. |
| CONCERNS | COMPLETE WITH NOTES unless concern is correctness or ADR violation. |
| REJECT | BLOCKED. |

---

## 10. Determine Completion Verdict

Verdicts:

| Verdict | Conditions |
|---|---|
| COMPLETE | All criteria PASS, required evidence present, no blocking deviations, review gates approve or are skipped by mode. |
| COMPLETE WITH NOTES | All required criteria PASS, no blocking deviations, but advisory evidence gaps, scope notes, or minor review concerns exist. |
| BLOCKED | Any criterion FAIL, required evidence missing, GDD/ADR contradiction, rejected code review, or >50% criteria untested. |

`--force-complete` may override BLOCKED only after the report records:

- The exact blocked items.
- User intent to force completion.
- Risk accepted.
- Follow-up action required.

If `--force-complete` is present but the story contradicts an Accepted ADR, still ask before completing.

---

## 11. Completion Report

Before writing updates, present the report in conversation.

```markdown
## Story Done: [Story Name]

**Story**: [path]
**Date**: [date]
**Review Mode**: [solo/lean/full]
**Verdict**: [COMPLETE / COMPLETE WITH NOTES / BLOCKED]

### Acceptance Criteria

| ID | Criterion | Status | Evidence |
|---|---|---|---|

### Test-Criterion Traceability

| Criterion | Verification | Coverage Status |
|---|---|---|

### Test Evidence

| Field | Value |
|---|---|
| Story Type | [type] |
| Required Evidence | [requirement] |
| Evidence Found | [paths or none] |
| Gate Result | [PASS/ADVISORY/BLOCKING] |

### Deviations

| Severity | Source | Finding | Action |
|---|---|---|---|

### Scope

| File | Status | Notes |
|---|---|---|

### Review Gates

| Gate | Result | Notes |
|---|---|---|

### Required Follow-Up

[numbered list or `None`]
```

If BLOCKED and `--force-complete` is not active, stop after the report and recommend the shortest path to COMPLETE.

---

## 12. Apply Story Updates

Skip all writes in `--dry-run`.

If verdict is COMPLETE or COMPLETE WITH NOTES:

1. Edit the story status to `Status: Complete`.
2. Append or update `## Completion Notes`.
3. Update `production/sprint-status.yaml` if it exists.
4. Append session extract to `production/session-state/active.md`.
5. Optionally log advisory deviations to `docs/tech-debt-register.md` if appropriate.

Completion Notes template:

```markdown
## Completion Notes

**Completed**: [date]
**Verdict**: [COMPLETE / COMPLETE WITH NOTES]
**Criteria**: [X/Y passing]
**Deferred/Untested Criteria**: [list or None]
**Test Evidence**: [paths or summary]
**GDD/ADR Deviations**: [None or list]
**Scope Notes**: [None or list]
**Review Gates**: [summary]
**Forced Completion**: [No / Yes — risk accepted]
```

Sprint status update rules:

- Match by story path first, then story ID.
- Set `status: done`.
- Set `completed: [date]`.
- Update top-level `updated: [date]` if present.
- Do not rewrite unrelated YAML entries.

Session state extract:

```markdown
## Session Extract — /story-done [date]

- Story: [path] — [title]
- Verdict: [verdict]
- Criteria: [X/Y passing]
- Evidence: [summary]
- Tech debt logged: [N or None]
- Next recommended: [next story path or sprint close-out]
```

If `active.md` does not exist, create it.

---

## 13. Surface Next Work

After completion, inspect:

```text
production/sprints/
production/epics/**/story-*.md
production/sprint-status.yaml
```

Find next stories that are:

- Ready or Not Started.
- Not blocked by incomplete prerequisites.
- Must Have or Should Have.
- In the current sprint if a sprint is active.

Output:

```markdown
## Next Up

1. [Story] — [path] — [estimate] — [why ready]
2. [Story] — [path] — [estimate] — [why ready]

Recommended: `/story-readiness [path]`
```

If all Must Have stories are complete:

```markdown
## Sprint Close-Out Sequence

1. `/smoke-check sprint`
2. `/team-qa sprint`
3. `/gate-check`
```

Do not run those commands automatically.

---

## 14. Closing Output

End with:

```markdown
## Story Completion Summary

**Story**: [path]
**Verdict**: [verdict]
**Story Updated**: [yes/no/dry-run]
**Sprint Status Updated**: [yes/no/not found/dry-run]
**Session State Updated**: [yes/no/dry-run]
**Next Recommended Action**: [command]
```
