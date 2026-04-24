---
name: gate-check
description: "Validate readiness to advance between development phases. Produces a PASS/CONCERNS/FAIL verdict with blockers, evidence, director review, and optional stage advancement."
argument-hint: "[target-phase: systems-design | technical-setup | pre-production | production | polish | release] [--review full|lean|solo] [--advance] [--dry-run]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash, Write, Task, AskUserQuestion
model: opus
---

# Phase Gate Validation

Validate whether the project is ready to move from the current production stage to a target stage.

This skill is prescriptive, not merely diagnostic. It must produce a formal verdict and a written evidence trail. It may write the gate report automatically unless `--dry-run` is set. It must not advance `production/stage.txt` unless the verdict is PASS and either `--advance` was provided or the user explicitly approves advancement.

---

## 0. Operating Contract

### Autonomy defaults

- Run the gate check without asking when the target phase is supplied.
- If target phase is omitted, auto-detect the current stage and target the next stage when confidence is high.
- Ask only when target stage is ambiguous, manual evidence is required, or protected stage advancement is requested.
- Write the gate report automatically unless `--dry-run` is active.
- Never mark unverifiable checks as PASS without file evidence, test output, or user confirmation.

### Protected operations

Require explicit confirmation before:

- Updating `production/stage.txt`, unless `--advance` was supplied and verdict is PASS.
- Creating missing project artifacts unrelated to the report.
- Running long or destructive shell commands.
- Treating a FAIL gate as advanced.

### Bash rules

Bash may be used for:

- Test commands detected from project configuration.
- `git status --short`, `git rev-parse`, and similar diagnostics.
- Non-mutating build/test discovery.

Bash must not be used to write files; use `Write` for reports and stage updates.

---

## 1. Parse Invocation

Arguments:

- Target phase: `systems-design`, `technical-setup`, `pre-production`, `production`, `polish`, or `release`.
- `--review full|lean|solo`.
- `--advance` to update `production/stage.txt` automatically on PASS.
- `--dry-run` to avoid all writes.

Resolve review mode once:

1. CLI `--review` value.
2. `production/review-mode.txt`, if valid.
3. Default: `lean`.

Review mode behavior for this skill:

| Mode | Artifact checks | Test/probe checks | Director panel |
|---|---|---|---|
| `solo` | Yes | Yes | No |
| `lean` | Yes | Yes | Yes, concise |
| `full` | Yes | Yes | Yes, detailed |

For phase gates, `lean` still runs the director panel because director judgment is central to phase advancement.

---

## 2. Determine Current and Target Stage

Read:

```text
production/stage.txt
design/gdd/game-concept.md
design/gdd/systems-index.md
docs/architecture/
production/sprints/
production/qa/
production/releases/
```

Stage order:

1. Concept
2. Systems Design
3. Technical Setup
4. Pre-Production
5. Production
6. Polish
7. Release
8. Live

If a target phase argument is present, map it to the destination stage and infer the source stage as the previous stage in the order unless `production/stage.txt` says otherwise.

If no target argument is present:

- Use `production/stage.txt` if valid.
- Else infer from artifact presence.
- If exactly one transition is likely, proceed and state the assumption.
- If multiple transitions are plausible, ask the user to choose.

Never run a gate into a stage lower than or equal to the current stage unless the user explicitly asks for a retrospective check.

---

## 3. Gate Definitions

Use the following definitions as the source of truth. A missing required artifact is normally FAIL unless the item is explicitly marked advisory.

### 3.1 Concept → Systems Design

Required artifacts:

- `design/gdd/game-concept.md` exists and has substantive content.
- Game pillars are defined in the concept doc or `design/gdd/game-pillars.md`.
- `design/gdd/game-concept.md` includes a Visual Identity Anchor.

Quality checks:

- Core loop is described.
- Target audience is identified.
- Visual Identity Anchor includes one visual rule and at least two supporting principles.
- Design review, if present, is not `MAJOR REVISION NEEDED`.

### 3.2 Systems Design → Technical Setup

Required artifacts:

- `design/gdd/systems-index.md` exists and lists at least MVP systems.
- MVP-tier GDDs exist under `design/gdd/`.
- Cross-GDD review report exists: `design/gdd/gdd-cross-review-*.md`.

Quality checks:

- MVP GDDs contain required sections and no unresolved major revision verdicts.
- Cross-GDD review verdict is not FAIL.
- System dependencies are mapped and bidirectionally consistent.
- MVP priority tier is defined.
- No stale GDD references remain unresolved.

If no cross-GDD review report exists, mark FAIL and recommend `/review-all-gdds`.

### 3.3 Technical Setup → Pre-Production

Required artifacts:

- Engine configured in `AGENTS.md` and `.claude/docs/technical-preferences.md`.
- Engine reference docs exist under `docs/engine-reference/<engine>/`.
- Art bible exists at `design/art/art-bible.md` with Visual Identity Foundation sections.
- Master architecture document exists at `docs/architecture/architecture.md`.
- Architecture traceability exists at `docs/architecture/architecture-traceability.md`.
- At least three Foundation ADRs exist under `docs/architecture/`.
- Architecture review report exists under `docs/architecture/`.
- Test framework exists: `tests/unit/`, `tests/integration/`, and `.github/workflows/tests.yml` or equivalent.
- At least one example test exists.
- `design/accessibility-requirements.md` exists.
- `design/ux/interaction-patterns.md` exists.

Quality checks:

- ADRs include `## Status`, `## Engine Compatibility`, `## ADR Dependencies`, and `## GDD Requirements Addressed`.
- All ADRs agree on the same engine version.
- No ADR references APIs listed in `deprecated-apis.md`.
- High-risk engine domains from `VERSION.md` are covered by architecture docs or listed as open questions.
- Architecture traceability has zero Foundation-layer gaps.
- Technical preferences include naming conventions and performance budgets or an explicit reason they are deferred.
- Accessibility tier is defined.

Special check: build ADR dependency graph from each ADR's `Depends On`. Any cycle is FAIL.

### 3.4 Pre-Production → Production

Required artifacts:

- At least one prototype exists under `prototypes/` with README or equivalent notes.
- First sprint plan exists under `production/sprints/`.
- Art bible is complete and sign-off recorded.
- Key character visual profiles exist when referenced by narrative docs.
- MVP-tier GDDs are complete.
- Control manifest exists at `docs/architecture/control-manifest.md`.
- Foundation and Core epics exist under `production/epics/`.
- Vertical Slice build or prototype is playable.
- At least three playtest sessions are documented.
- Vertical Slice playtest report exists.
- UX specs exist for main menu, core gameplay HUD, and pause menu where applicable.
- Key UX specs have passed or accepted `/ux-review`.

Quality checks:

- Core loop fun is validated by playtest data.
- Vertical Slice demonstrates one complete start → challenge → resolution cycle.
- The first two minutes communicate what to do.
- No critical fun blocker bugs remain.
- Core fantasy is independently observed in playtest notes.
- Sprint plan references real story file paths, not only GDDs.
- Stories embed GDD requirement IDs and ADR references.
- Architecture has no unresolved Foundation/Core open questions.

If Vertical Slice validation fails, verdict is FAIL regardless of other checks.

### 3.5 Production → Polish

Required artifacts:

- `src/` or equivalent active implementation exists.
- Core mechanics from MVP GDDs are implemented.
- Main gameplay path is playable end-to-end.
- Logic and Integration stories have unit/integration tests.
- Smoke check report exists and is PASS or PASS WITH WARNINGS.
- QA plan exists under `production/qa/`.
- QA sign-off report exists and is APPROVED or APPROVED WITH CONDITIONS.
- At least three playtest sessions cover new player experience, mid-game systems, and difficulty curve.

Quality checks:

- Test suite passes.
- No critical/blocker bugs remain.
- Performance is within budget.
- Critical fun issues from playtests are addressed.
- No unresolved confusion loops where more than half of playtesters got stuck without understanding why.
- Implemented screens have UX specs.
- Interaction pattern library is current.
- Accessibility tier is implemented or exceptions documented.

### 3.6 Polish → Release

Required artifacts:

- Milestone features are implemented or formally deferred.
- Content complete: levels, assets, dialogue, and data referenced by design docs exist.
- Localization strings are externalized.
- QA plan and QA sign-off exist.
- Must Have story evidence is complete.
- Smoke check PASS exists for release candidate.
- Regression suite passes.
- Balance check has been run if balance-sensitive systems exist.
- Release or launch checklist exists.
- Store metadata, changelog, and patch notes are drafted where applicable.

Quality checks:

- QA sign-off is APPROVED or APPROVED WITH CONDITIONS.
- No known critical, high, or medium bugs remain unless explicitly accepted in known issues.
- Build compiles and packages cleanly.
- Performance targets are met across target platforms.
- Accessibility basics are covered.
- Localization is verified for target languages.
- Legal/platform requirements are documented where applicable.

---

## 4. Run Evidence Collection

### 4.1 Artifact checks

For each required artifact:

- Use `Glob` to find candidate files.
- Use `Read` to verify meaningful content, not only existence.
- Mark as:
  - `PASS` — present and substantive.
  - `CONCERNS` — present but incomplete or stale.
  - `FAIL` — missing or unusable.
  - `MANUAL` — cannot be verified automatically.

### 4.2 Quality checks

Use direct evidence where possible:

- Tests: run configured test command with Bash when a runner exists.
- Design review: read review reports and verdicts.
- Architecture: read ADRs, architecture docs, registry, and traceability.
- Performance: read `technical-preferences.md`, perf reports, and test output.
- Localization: grep for likely hardcoded player-facing strings in implementation paths.
- Bugs: read `production/qa/bugs/` and recent QA reports.
- Playtests: read `production/playtests/` reports and extract observed validation.

If test command is unknown or likely long-running, ask before running. Otherwise run targeted tests.

### 4.3 Consistency failures

If `docs/consistency-failures.md` exists, read entries relevant to the target gate's domain. Recurring issues increase severity:

- Repeated unresolved issue in a gate-critical domain: escalate one level.
- Resolved issue with evidence: mention but do not penalize.

### 4.4 Manual evidence

Batch manual questions with `AskUserQuestion` only after all automatic checks are complete.

Do not ask the user to confirm facts that are visible in files. Ask only for experiential or off-repo evidence, such as:

- Whether a human played the Vertical Slice without guidance.
- Whether core mechanic feel was validated.
- Whether informal QA occurred but was not documented.
- Whether deployment/store/legal materials exist outside the repository.

Unanswered manual checks remain `MANUAL CHECK NEEDED` and cannot contribute to PASS.

---

## 5. Director Panel

Skip entirely in `solo` mode.

In `lean` and `full`, spawn director reviewers using `.claude/docs/director-gates.md` when available.

Spawn these subagents in parallel:

1. `creative-director` — `CD-PHASE-GATE`
2. `technical-director` — `TD-PHASE-GATE`
3. `producer` — `PR-PHASE-GATE`
4. `art-director` — `AD-PHASE-GATE`

Pass each:

- Source and target stage.
- Artifact check summary.
- Quality check summary.
- Manual evidence status.
- Known blockers.
- Relevant docs and paths.
- Current draft verdict.

If `.claude/docs/director-gates.md` is missing:

- In `lean`: continue and mark director panel unavailable.
- In `full`: ask whether to continue without director gates or stop.

Director verdict mapping:

| Director response | Gate impact |
|---|---|
| READY | No downgrade |
| CONCERNS | Minimum final verdict CONCERNS |
| NOT READY / REJECT | Minimum final verdict FAIL |

A user may override a director FAIL only after explicit acknowledgement. The report must record the override rationale.

---

## 6. Determine Verdict

Initial verdict rules:

- Any hard FAIL artifact or quality check: `FAIL`.
- Any unresolved manual check required for the gate: `CONCERNS`, unless the check is a hard gate item, then `FAIL`.
- Any director NOT READY: `FAIL`.
- Any director CONCERNS: at least `CONCERNS`.
- All required artifacts and quality checks PASS with no director concerns: `PASS`.

Verdict definitions:

| Verdict | Meaning |
|---|---|
| `PASS` | Ready to advance. No gate-critical blockers remain. |
| `CONCERNS` | Advancement is possible but risks are documented and should be handled soon. |
| `FAIL` | Advancement is not recommended; blockers must be resolved first. |

---

## 7. Chain-of-Verification

Before finalizing the verdict, challenge it.

Generate five challenge questions appropriate to the draft verdict.

For PASS, challenge:

1. Which checks were inferred instead of directly verified?
2. Are any manual checks unresolved?
3. Did any artifact exist but contain only template content?
4. Are director concerns being underweighted?
5. What single check has the weakest evidence?

For CONCERNS, challenge:

1. Could any concern become a blocker in the next phase?
2. Are multiple minor concerns collectively blocking?
3. Did a FAIL condition get softened without evidence?
4. Are there missing artifacts outside the checklist that are implied by the phase?
5. Can each concern be resolved within the next phase?

For FAIL, challenge:

1. Are blockers separated from recommendations?
2. Is any blocker based on missing evidence rather than actual failure?
3. Is there a minimal path to PASS?
4. Are any additional blockers missing?
5. Is the failure about readiness or about unavailable documentation?

Answer each question using evidence. Revise the verdict if needed.

---

## 8. Write Gate Report

Unless `--dry-run`, write:

```text
production/gate-checks/gate-check-[YYYY-MM-DD]-[source-to-target-slug].md
```

Create the directory if needed.

Report template:

```markdown
# Gate Check: [Source Stage] → [Target Stage]

**Date**: [date]
**Review Mode**: [solo/lean/full]
**Dry Run**: [yes/no]
**Verdict**: [PASS/CONCERNS/FAIL]

## Summary

[one-paragraph readiness assessment]

## Required Artifacts

| Check | Status | Evidence | Notes |
|---|---|---|---|

## Quality Checks

| Check | Status | Evidence | Notes |
|---|---|---|---|

## Manual Evidence

| Check | Status | User Response / Evidence | Notes |
|---|---|---|---|

## Director Panel

| Director | Verdict | Notes |
|---|---|---|

## Blockers

[numbered list or `None`]

## Concerns

[numbered list or `None`]

## Recommendations

[numbered list]

## Minimal Path to PASS

1. [specific action]
2. [specific action]
3. [specific action]

## Chain of Verification

- Questions checked: 5
- Verdict change: [unchanged | changed from X to Y]
- Weakest evidence area: [area]
```

In `--dry-run`, print the report content in conversation and do not write.

---

## 9. Stage Advancement

Only eligible when verdict is `PASS`.

If verdict is PASS and `--advance` was provided:

- Write target stage name to `production/stage.txt` unless `--dry-run`.
- Report that the stage was advanced.

If verdict is PASS and `--advance` was not provided:

Use `AskUserQuestion`:

```text
Gate passed. Advance production/stage.txt to [Target Stage]?
```

Options:

- `Advance stage now`
- `Leave stage unchanged`

If verdict is CONCERNS or FAIL:

- Do not update `production/stage.txt`.
- If the user asks to advance anyway, record an explicit override report and write stage only after confirmation.

---

## 10. Follow-Up Actions

Suggest actions tied to actual gaps found. Do not list generic commands unrelated to the verdict.

Common mappings:

| Gap | Suggested action |
|---|---|
| No concept | `/brainstorm` |
| No systems index | `/map-systems` |
| Missing GDDs | `/design-system [system]` |
| GDDs not cross-reviewed | `/review-all-gdds` |
| Missing engine setup | `/setup-engine` |
| Missing tests | `/test-setup` |
| Missing architecture blueprint | `/create-architecture` |
| Missing ADRs | `/architecture-decision [topic]` |
| ADRs missing required sections | `/architecture-decision retrofit [path]` |
| Missing control manifest | `/create-control-manifest` |
| Missing epics/stories | `/create-epics` then `/create-stories` |
| Stories not ready | `/story-readiness [path]` |
| QA missing | `/qa-plan`, `/team-qa`, `/smoke-check` |
| Performance unknown | `/perf-profile` |
| Playtest evidence missing | `/playtest-report` |
| Release prep missing | `/launch-checklist` or `/release-checklist` |

---

## 11. Closing Output

End with:

```markdown
## Gate Check Result

**Transition**: [Source] → [Target]
**Verdict**: [PASS/CONCERNS/FAIL]
**Report**: [path or "dry-run only"]
**Stage Updated**: [yes/no]

### Next recommended action

[one command or action based on highest-priority blocker]
```
