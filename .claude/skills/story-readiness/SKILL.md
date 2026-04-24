---
name: story-readiness
description: "Read-only validation that a story is implementation-ready. Checks GDD traceability, ADR acceptance, manifest version, dependencies, acceptance criteria, test evidence requirements, and scope clarity; returns READY / NEEDS WORK / BLOCKED."
argument-hint: "[story-file-path | epic:<epic-slug> | sprint | all] [--review full|lean|solo]"
user-invocable: true
allowed-tools: Read, Glob, Grep, AskUserQuestion, Task
model: haiku
---

# Story Readiness

Validate whether one or more story files are safe to implement. This skill is read-only. It never edits story files, statuses, registries, sprint plans, or session state.

Output: a verdict per story plus concrete fix instructions for every failing check.

---

## 0. Execution Contract

### 0.1 Parse invocation

Supported scopes:

| Invocation | Scope |
|---|---|
| `/story-readiness production/epics/combat/story-001-basic-attack.md` | One story. |
| `/story-readiness epic:combat` | All stories under `production/epics/combat/`. |
| `/story-readiness sprint` | Stories referenced by the current sprint plan. |
| `/story-readiness all` | All `production/epics/**/story-*.md` files. |
| No argument | Ask for scope. |

Flags:

- `--review full|lean|solo`

Resolve review mode once:

1. Explicit `--review` value.
2. `production/review-mode.txt`, if present and valid.
3. `lean`.

Review semantics:

| Mode | QA lead review |
|---|---|
| `solo` | Skip Task review. |
| `lean` | Skip Task review. |
| `full` | Run `qa-lead` gate `QL-STORY-READY` on non-ready stories and a sample of ready stories. |

### 0.2 Path safety

All user-supplied paths must be repository-relative. Reject absolute paths and paths containing `..`.

Valid story paths must match:

```text
production/epics/*/story-*.md
```

### 0.3 Read-only policy

Do not call `Write`, `Edit`, `Bash`, or any mutation tool. If a story needs changes, provide exact recommended replacement text or a patch plan in the response only.

### 0.4 Missing-file behavior

| Missing input | Effect |
|---|---|
| Story file | `BLOCKED`. |
| GDD referenced by story | `NEEDS WORK`; if the story cannot be understood without it, `BLOCKED`. |
| Governing ADR file | `BLOCKED`. |
| TR registry | Do not fail all stories; mark TR validation `Not checked`. |
| Control manifest | Do not fail all stories; mark manifest validation `Not checked`. |
| Dependency story file | `BLOCKED`. |
| Evidence file | Depends on type; generally `NEEDS WORK` before closure, not before implementation. |

---

## 1. Resolve Story Set

For `sprint` scope:

1. Glob `production/sprints/*.md` and `production/sprints/*.yaml`.
2. Use the most recent sprint file unless the active sprint is named in `production/session-state/active.md`.
3. Extract story paths from the sprint file.
4. If none are found, fall back to stories with status `Ready` in `production/epics/**/story-*.md` and state the fallback.

For `all` scope, include only files matching `story-*.md`; exclude `EPIC.md`, `index.md`, and proposed revision files.

For no argument, ask:

```text
What should I validate?
[A] Current sprint
[B] All stories
[C] A specific epic
[D] A specific story path
```

---

## 2. Load Shared Context Once

Read these once and cache their findings:

- `docs/architecture/tr-registry.yaml`, if present.
- `docs/architecture/control-manifest.md`, if present.
- `design/gdd/systems-index.md`, if present.
- `production/session-state/active.md`, if present.
- Current sprint file, if applicable.
- All unique ADRs referenced by the selected stories.
- All dependency story files referenced by the selected stories.

For each ADR, extract:

- Path.
- ADR number and title.
- `Status`.
- `Engine Compatibility` risk.
- Any explicit implementation constraints.

For the control manifest, extract:

- Manifest version.
- Layer-specific required patterns.
- Layer-specific forbidden patterns.
- Performance guardrails.

---

## 3. Evaluate Each Story

Assign each check a result:

| Result | Meaning |
|---|---|
| `PASS` | Requirement satisfied. |
| `WARN` | Missing context prevents validation but does not block implementation. |
| `NEEDS WORK` | Story needs content changes before implementation. |
| `BLOCKED` | Story cannot safely be implemented. |
| `N/A` | Explicitly not applicable. |

### 3.1 Required header fields

A story should contain:

- Epic.
- Status.
- Layer.
- Type.
- Estimate.
- Manifest Version or explicit `Unavailable`.

Missing `Type` or `Status` is `NEEDS WORK` because downstream skills use those fields. Missing estimate is `NEEDS WORK` for sprint planning but not an implementation blocker.

### 3.2 GDD traceability

Pass only if:

- The story names a `design/gdd/*.md` file or an approved quick-design spec.
- It lists at least one specific requirement ID or criterion.
- Acceptance criteria are self-contained.

Fail patterns:

- Generic reference such as "see GDD" with no requirement.
- Acceptance criteria that require opening the GDD to understand Done.
- Requirement ID missing from the registry when the registry exists.
- Requirement ID exists but is `deprecated` or `superseded`.

### 3.3 ADR readiness

Pass only if every governing ADR is either:

- `Accepted`, or
- explicitly marked `N/A — no ADR applies` with a reason.

Block if:

- ADR file missing.
- ADR status is `Proposed`.
- ADR status is `Deprecated` or `Superseded` and no current ADR is referenced.
- Story instruction contradicts a binding ADR or registry stance.

### 3.4 Manifest compliance

If the story has a Manifest Version and the current manifest has a newer version/date:

- Mark `NEEDS WORK`.
- Include: `Fix: Re-read control-manifest.md, update affected rules, then update Manifest Version to [current].`

If the manifest is missing, mark `WARN`, not failure.

If the story violates a forbidden manifest pattern, mark `BLOCKED`.

### 3.5 Scope clarity

Check:

- At least three testable acceptance criteria unless the story is explicitly tiny and Config/Data.
- No `TBD`, `TODO`, `UNRESOLVED`, `???`, or open question marks in criteria or implementation notes.
- Clear `Out of Scope` section.
- Dependencies listed, or explicit `None`.
- Acceptance criteria are observable and not subjective.

Subjective terms such as `feels good`, `looks polished`, or `responsive` pass only if paired with a benchmark, timing threshold, example clip, reference asset, or manual QA protocol.

### 3.6 Dependency readiness

For each dependency:

- Find the dependency story file.
- Read its status.
- Pass if status is `Done`, `Complete`, or `Closed`.
- Block if status is `Draft`, `Ready`, `In Progress`, `Blocked`, `Needs Work`, missing, or unknown.

### 3.7 Evidence readiness

Evidence requirements by type:

| Type | Implementation-ready requirement |
|---|---|
| `Logic` | Test path declared. Test file need not exist yet. |
| `Integration` | Test or integration evidence path declared. Test file need not exist yet. |
| `Visual/Feel` | Evidence document path and manual pass condition declared. |
| `UI` | Evidence document path or interaction test path declared. |
| `Config/Data` | Smoke-check evidence requirement declared. |

Missing evidence path is `NEEDS WORK`.

### 3.8 Asset references

Scan for asset-like paths and extensions:

```text
assets/, .png, .jpg, .jpeg, .svg, .wav, .ogg, .mp3, .glb, .gltf, .tres, .tscn, .res
```

If referenced assets are missing:

- `NEEDS WORK` if the story can still be implemented with placeholders.
- `BLOCKED` if the asset is required for acceptance criteria.

---

## 4. Assign Verdict

Use the strongest applicable result:

| Verdict | Criteria |
|---|---|
| `READY` | No `BLOCKED` or `NEEDS WORK` checks. Warnings are acceptable and listed. |
| `NEEDS WORK` | One or more `NEEDS WORK` checks and no blockers. |
| `BLOCKED` | Any blocker exists. |

Do not produce `READY` for a story governed by a proposed, missing, deprecated, or superseded ADR.

---

## 5. Full-Mode QA Review

Run only in `full` mode.

For each non-ready story, and for up to five ready stories as a quality sample, spawn `qa-lead` with gate `QL-STORY-READY`. Pass:

- Story path.
- Story type.
- Acceptance criteria.
- Evidence requirement.
- Dependency status.
- Preliminary verdict and gaps.

Ask QA lead to verify:

1. Criteria are testable.
2. Evidence path matches story type.
3. Dependency verdict is correct.
4. No missing edge case would make implementation ambiguous.

If QA lead finds additional gaps, merge them into the final report. Do not edit files.

---

## 6. Output Format

### 6.1 Single story

```markdown
## Story Readiness: [Story title]

File: `[path]`
Verdict: [READY | NEEDS WORK | BLOCKED]

### Summary

| Category | Result | Notes |
|----------|--------|-------|
| GDD traceability | PASS/NEEDS WORK/BLOCKED/WARN | [notes] |
| ADR readiness | PASS/NEEDS WORK/BLOCKED/WARN | [notes] |
| Manifest compliance | PASS/NEEDS WORK/BLOCKED/WARN | [notes] |
| Scope clarity | PASS/NEEDS WORK/BLOCKED/WARN | [notes] |
| Dependencies | PASS/NEEDS WORK/BLOCKED/WARN | [notes] |
| Evidence | PASS/NEEDS WORK/BLOCKED/WARN | [notes] |

### Required Fixes

- [exact issue]
  - Fix: [specific text or action]

### Warnings

- [warning or None]

### Next Action

[Run /dev-story path, fix story, accept ADR, create missing dependency, etc.]
```

### 6.2 Multiple stories

```markdown
## Story Readiness Summary — [scope]

Ready: [N]
Needs Work: [N]
Blocked: [N]

### Ready Stories

- `[path]` — [title]

### Needs Work

- `[path]` — [primary fix]

### Blocked

- `[path]` — [blocker]

### Must-Have Sprint Risk

[Only for sprint scope. List Must Have stories that are not READY.]
```

For multiple scope, include detailed reports for all non-ready stories. For ready stories, list concise rows unless the user requested full detail.

---

## 7. Completion

End with:

```text
Verdict: [READY | NEEDS WORK | BLOCKED | MIXED]
Next best action: [command]
```

Do not offer to modify story files from this read-only skill. Recommend `/create-stories [epic]` for structural regeneration or manual patching for small fixes.
