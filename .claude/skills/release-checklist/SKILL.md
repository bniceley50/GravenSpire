---
name: release-checklist
description: "Generate a pre-release validation checklist from repository evidence. Covers build verification, QA gates, platform readiness, store/distribution prep, support readiness, and go/no-go sign-offs."
argument-hint: "[platform: pc|console|mobile|all] [version:<version>] [--dry-run]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write
---

# Release Checklist

Generate a repository-local release checklist. This skill does not build, tag, publish, upload, submit, deploy, notify users, or alter release state. It only reads project evidence and writes a checklist artifact.

Output:

```text
production/releases/release-checklist-[version-or-date]-[platform].md
```

The user's invocation authorizes writing the checklist. Do not ask for confirmation before routine checklist creation.

---

## 0. Execution Contract

### 0.1 Parse invocation

Supported platform arguments:

| Argument | Scope |
|---|---|
| `pc` | PC/desktop release. |
| `console` | Console release. |
| `mobile` | Mobile release. |
| `all` or blank | All relevant platform sections. |

Optional version argument:

```text
version:1.0.0
```

If no version is supplied, infer it from repository evidence in this order:

1. `AGENTS.md` release/version field.
2. `production/milestones/` current release milestone.
3. Project config files with version fields.
4. Latest changelog or patch-notes artifact.
5. `unknown-version-[YYYYMMDD]`.

`--dry-run` means produce the checklist in conversation only; do not write.

### 0.2 Path safety and writes

Allowed write location:

```text
production/releases/
```

Reject absolute paths and paths containing `..`. If the target file already exists, create a numbered variant instead of overwriting.

Do not modify:

- Source code.
- Tests.
- QA reports.
- Stage files.
- Release status files.
- CI/deploy configuration.
- Git tags or branches.
- External store/platform accounts.

### 0.3 Evidence policy

Use repository evidence when available. If evidence is missing, create an unchecked checklist item and mark the source as missing. Do not claim compliance with external certification, store, legal, or privacy requirements unless the repository contains evidence.

For platform certification requirements, write generic validation items and require the team to verify against the current official platform holder documentation.

---

## 1. Load Release Context

Read if present:

- `AGENTS.md`.
- `production/stage.txt`.
- Current milestone in `production/milestones/`.
- Latest sprint status or sprint plan in `production/sprints/`.
- Latest `production/qa/smoke-*.md`.
- Latest `production/qa/qa-signoff-*.md`.
- `production/qa/bugs/*.md`.
- `production/releases/*`.
- `CHANGELOG.md`, `changelog.md`, or `production/changelog*.md`.
- `production/patch-notes*.md`.
- `docs/engine-reference/*/VERSION.md`.
- `.claude/docs/technical-preferences.md`.

Extract:

- Project name.
- Version.
- Engine and version.
- Target platforms.
- Current milestone/release scope.
- QA verdicts.
- Open bugs by severity.
- Known issues.
- Existing release artifacts.

---

## 2. Scan Repository Health

Use `Grep`/`Glob` to inspect the repository for release risk signals:

- `TODO`.
- `FIXME`.
- `HACK`.
- `TEMP`.
- `PLACEHOLDER`.
- `WIP`.
- `XXX`.

Exclude obviously irrelevant directories when possible:

```text
.git/
node_modules/
Library/
Temp/
Build/
build/
dist/
.import/
```

Classify findings:

| Marker | Release impact |
|---|---|
| `FIXME` | Potential blocker. |
| `HACK` | Needs review. |
| `TODO` | Advisory unless in release-critical path. |
| `PLACEHOLDER` | Content-completeness risk. |
| `WIP` / `TEMP` / `XXX` | Potential blocker depending on location. |

Also inspect known bug reports:

| Severity | Release rule |
|---|---|
| S1 | Blocker. |
| S2 | Blocker unless formal exception exists. |
| S3 | Condition or deferral candidate. |
| S4 | Advisory. |

---

## 3. Determine Readiness Inputs

Summarize evidence:

| Area | Evidence source | Status |
|---|---|---|
| Smoke check | Latest `production/qa/smoke-*.md` | PASS/PASS WITH WARNINGS/FAIL/Missing |
| QA sign-off | Latest `qa-signoff` | APPROVED/APPROVED WITH CONDITIONS/NOT APPROVED/Missing |
| Open bugs | `production/qa/bugs/` | counts by severity |
| Changelog | `CHANGELOG.md` or production changelog | Present/Missing |
| Patch notes | `production/patch-notes*` | Present/Missing |
| Release artifacts | `production/releases/` | Present/Missing |
| Engine reference | `docs/engine-reference/*/VERSION.md` | Present/Missing |

Use this to set preliminary go/no-go:

| Result | Conditions |
|---|---|
| `READY CANDIDATE` | Smoke PASS, QA APPROVED, no open S1/S2 bugs, no known release blockers. |
| `READY WITH CONDITIONS` | Smoke PASS WITH WARNINGS or QA APPROVED WITH CONDITIONS and no S1/S2 blockers. |
| `NOT READY` | Smoke FAIL, QA NOT APPROVED, open S1 bug, open S2 bug without exception, or missing critical evidence. |

---

## 4. Generate Checklist

Use this structure.

```markdown
# Release Checklist: [Project] [Version] — [Platform]

Generated: [YYYY-MM-DD]
Preliminary Go/No-Go: [READY CANDIDATE | READY WITH CONDITIONS | NOT READY]

## Evidence Summary

| Area | Evidence | Status | Notes |
|------|----------|--------|-------|
| Smoke Check | [path/missing] | [status] | [notes] |
| QA Sign-Off | [path/missing] | [status] | [notes] |
| Open Bugs | [count] | [status] | [S1/S2/S3/S4 counts] |
| Changelog | [path/missing] | [status] | [notes] |
| Patch Notes | [path/missing] | [status] | [notes] |

## Codebase Health

- [ ] Review TODO/FIXME/HACK/PLACEHOLDER findings.
- [ ] Resolve or formally defer release-critical findings.
- [ ] Confirm no debug-only paths remain in player-facing builds.
- [ ] Confirm logging level is appropriate for release.

### Marker Findings

| Marker | Count | Highest Risk Locations |
|--------|-------|------------------------|
| TODO | [N] | [top paths] |
| FIXME | [N] | [top paths] |
| HACK | [N] | [top paths] |
| PLACEHOLDER | [N] | [top paths] |

## Build Verification

- [ ] Clean build succeeds for every target platform.
- [ ] Build version matches `[version]`.
- [ ] Build number / bundle identifier / package name are correct.
- [ ] Build is reproducible from a known commit.
- [ ] No release-blocking compiler or packaging warnings.
- [ ] Build size is within target budget or exception is documented.
- [ ] Required runtime dependencies are packaged.
- [ ] Debug/dev-only flags are disabled unless intentionally shipping.

## Quality Gates

- [ ] Latest smoke check is PASS or accepted PASS WITH WARNINGS.
- [ ] Latest QA sign-off is APPROVED or accepted APPROVED WITH CONDITIONS.
- [ ] No open S1 bugs.
- [ ] No open S2 bugs unless formal release exception exists.
- [ ] All release-critical stories are Done/Closed.
- [ ] Regression suite has been run or exception is documented.
- [ ] Soak test completed or explicitly waived.
- [ ] Performance budgets verified on target hardware/profile.

## Content Complete

- [ ] Placeholder assets replaced or explicitly accepted.
- [ ] Player-facing text proofread.
- [ ] Localization readiness confirmed for supported languages.
- [ ] Credits complete and accurate.
- [ ] Third-party license notices complete.
- [ ] Audio mix and visual polish sign-offs completed.

## Store and Distribution

- [ ] Store metadata complete.
- [ ] Screenshots and trailers current for this build.
- [ ] Key art/capsule art current.
- [ ] Age rating process complete or scheduled.
- [ ] EULA, privacy policy, and legal notices available.
- [ ] Pricing and regional availability configured.
- [ ] Support contact and known-issues page prepared.

## Launch Operations

- [ ] Crash reporting configured and monitored.
- [ ] Analytics/telemetry verified where applicable.
- [ ] Rollback or hotfix plan documented.
- [ ] On-call/support coverage defined for launch window.
- [ ] Community/support FAQ prepared.
- [ ] Day-one patch plan documented if needed.
```

### 4.1 PC section

Include for `pc` or `all`:

```markdown
## Platform Requirements — PC

- [ ] Minimum and recommended specs verified.
- [ ] Keyboard and mouse path tested.
- [ ] Controller path tested if supported.
- [ ] Windowed, borderless, fullscreen modes tested.
- [ ] Common resolutions and ultrawide behavior tested.
- [ ] Graphics/audio/input settings persist correctly.
- [ ] PC storefront SDK features tested if integrated.
- [ ] Steam Deck or handheld-PC behavior tested if targeted.
```

### 4.2 Console section

Include for `console` or `all`:

```markdown
## Platform Requirements — Console

- [ ] Current first-party certification checklist reviewed against official docs.
- [ ] Controller prompts and platform terminology correct.
- [ ] Suspend/resume behavior tested.
- [ ] User/profile switching handled.
- [ ] Storage-full and network-loss scenarios handled.
- [ ] Safe-zone and TV readability verified.
- [ ] Achievement/trophy integration tested if applicable.
- [ ] Certification package prepared and internally reviewed.
```

### 4.3 Mobile section

Include for `mobile` or `all`:

```markdown
## Platform Requirements — Mobile

- [ ] Current app-store policy checklist reviewed against official docs.
- [ ] Required permissions justified and documented.
- [ ] Privacy/data-safety labels complete.
- [ ] Touch controls verified across supported screen sizes.
- [ ] Orientation and background/foreground behavior tested.
- [ ] Battery, thermal, and memory behavior acceptable.
- [ ] In-app purchase or ad flows tested if applicable.
- [ ] App size and asset delivery constraints checked.
```

### 4.4 Sign-offs

```markdown
## Go / No-Go

Preliminary verdict: [READY CANDIDATE | READY WITH CONDITIONS | NOT READY]

### Blockers

- [blocker or None]

### Conditions

- [condition or None]

### Sign-Offs Required

- [ ] QA Lead
- [ ] Technical Director
- [ ] Producer
- [ ] Creative Director / Product Owner
- [ ] Release Manager

### Final Decision

- [ ] GO
- [ ] NO-GO
- [ ] GO WITH CONDITIONS: [conditions]
```

---

## 5. Write Checklist

If `--dry-run`, present the checklist and do not write.

Otherwise:

1. Create `production/releases/` if missing.
2. Write to `production/releases/release-checklist-[version]-[platform].md`.
3. If the target exists, write a numbered variant.

Do not update release status or stage files.

---

## 6. Completion Output

End with:

```text
Verdict: [READY CANDIDATE | READY WITH CONDITIONS | NOT READY | DRY RUN]
Checklist: [path or not written]
Blockers: [N]
Conditions: [N]
Next best action: [command]
```

Recommended next actions:

| Preliminary verdict | Next action |
|---|---|
| `READY CANDIDATE` | `/team-release` for coordinated sign-off. |
| `READY WITH CONDITIONS` | Resolve or formally defer listed conditions, then `/team-release`. |
| `NOT READY` | Resolve blockers, rerun `/smoke-check` and `/team-qa`, then regenerate this checklist. |
