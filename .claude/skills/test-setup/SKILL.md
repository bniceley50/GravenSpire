---
name: test-setup
description: "Scaffold or repair test infrastructure for the configured game engine, including directory layout, engine-specific runner docs, smoke-test seed, and CI workflow, without overwriting existing tests."
argument-hint: "[--force] [--repair] [--dry-run]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash, Write, Edit, AskUserQuestion
---

# Test Setup

Create the repository's baseline automated and manual test infrastructure. This skill is intended for Technical Setup before implementation begins, but it can also repair missing test scaffolding later.

Invoking `/test-setup` authorizes creation of missing test infrastructure files. It must never overwrite existing tests or CI workflows without explicit approval.

---

## 0. Operating Contract

### Autonomy defaults

- Detect engine and language automatically from repository configuration.
- Create missing directories and scaffold files automatically unless `--dry-run` is active.
- Skip existing files by default.
- Ask only when engine detection is ambiguous, existing CI conflicts with the intended workflow, or `--repair` would edit an existing file.

### Safety rules

Never:

- Overwrite existing test files.
- Delete test files.
- Modify source code.
- Commit, push, or run external CI.
- Manage Unity license secrets or platform credentials.
- Install plugins/packages automatically.

### Protected edits

Require explicit confirmation before:

- Editing an existing `.github/workflows/*.yml`.
- Editing an existing `tests/README.md` that has user-authored content.
- Replacing an existing engine-specific runner.
- Changing test framework choice.

### Path rules

Allowed write targets:

```text
tests/**
.github/workflows/tests.yml
Source/Tests/**
```

All paths must be repository-relative and must not contain `..`.

---

## 1. Parse Invocation

Options:

- `--force` — do not early-exit when infrastructure exists; still never overwrite existing files.
- `--repair` — propose targeted edits for incomplete existing scaffold.
- `--dry-run` — show the plan but write nothing.

If both `--force` and `--repair` are present:

- Create missing files.
- Propose repairs for existing files.
- Ask before applying repairs.

---

## 2. Detect Engine and Language

Read in order:

```text
.claude/docs/technical-preferences.md
AGENTS.md
docs/engine-reference/*/VERSION.md
project.godot
*.sln
*.csproj
*.uproject
```

Determine:

- Engine: Godot, Unity, Unreal, or unknown.
- Engine version.
- Primary language.
- Project name.
- Target platforms if available.

If engine is unknown:

1. If exactly one engine marker exists, use it.
2. If multiple markers exist, ask which engine to scaffold.
3. If no markers exist, stop and recommend `/setup-engine`.

Do not guess an engine from directory names alone when configuration conflicts.

---

## 3. Inspect Existing Test State

Check:

```text
tests/
tests/unit/
tests/integration/
tests/smoke/
tests/evidence/
tests/README.md
.github/workflows/tests.yml
.github/workflows/*.yml
```

Engine-specific checks:

| Engine | Check |
|---|---|
| Godot | `tests/gdunit4_runner.gd`, `addons/gdunit4/`, `.gd` tests |
| Unity | `tests/EditMode/`, `tests/PlayMode/`, `.asmdef`, Unity Test Framework references |
| Unreal | `Source/Tests/`, `.uproject`, Automation test docs |

Classify each target file:

| Status | Meaning |
|---|---|
| CREATE | Missing and safe to create. |
| EXISTS_OK | Exists and appears compatible. |
| EXISTS_CONFLICT | Exists but differs materially. |
| REPAIRABLE | Exists but missing useful section and `--repair` may patch. |
| SKIP | Not applicable to the detected engine. |

If everything required exists and neither `--force` nor `--repair` is present, output a summary and stop.

---

## 4. Build Scaffold Plan

Produce a plan before writing:

```markdown
## Test Setup Plan — [Engine]

**Engine**: [engine] [version]
**Language**: [language]
**Mode**: [create missing / force / repair / dry-run]

| Path | Action | Reason |
|---|---|---|
| tests/README.md | CREATE | Missing test docs |
| tests/unit/.keep | CREATE | Preserve empty unit test directory |
| .github/workflows/tests.yml | EXISTS_CONFLICT | Existing workflow uses different framework |
```

In normal mode, proceed with all CREATE actions automatically.

For EXISTS_CONFLICT or REPAIRABLE actions, ask before editing.

---

## 5. Standard Directory Layout

Create missing directories:

```text
tests/
tests/unit/
tests/integration/
tests/smoke/
tests/evidence/
```

Use safe directory creation such as:

```bash
mkdir -p tests/unit tests/integration tests/smoke tests/evidence .github/workflows
```

Only use Bash for directory creation and non-mutating diagnostics. Use `Write` for file content.

Create placeholder files only when the directory would otherwise be empty:

```text
tests/unit/.keep
tests/integration/.keep
tests/smoke/.keep
tests/evidence/.keep
```

Placeholder content:

```text
# Placeholder. Replace with real test evidence when this directory receives test files.
```

---

## 6. `tests/README.md`

Create if missing.

```markdown
# Test Infrastructure

**Engine**: [engine name + version]
**Primary Language**: [language]
**Test Framework**: [framework]
**CI Workflow**: `.github/workflows/tests.yml`
**Setup Date**: [date]

## Directory Layout

```text
tests/
  unit/           # Isolated logic tests: formulas, state machines, pure systems
  integration/    # Cross-system tests and save/load round trips
  smoke/          # Critical path manual/automated smoke checks
  evidence/       # Screenshots, walkthroughs, manual sign-off records
```

## Running Tests

[engine-specific command]

## Naming

- Test files: `[system]_[feature]_test.[ext]`
- Test functions: `test_[scenario]_[expected_result]`
- Evidence docs: `[story-slug]-evidence.md`

## Story Type → Required Evidence

| Story Type | Required Evidence | Location |
|---|---|---|
| Logic | Passing automated unit test | `tests/unit/[system]/` |
| Integration | Passing integration test or documented playtest | `tests/integration/[system]/` |
| Visual/Feel | Screenshot/video + sign-off | `tests/evidence/` or `production/qa/evidence/` |
| UI | Manual walkthrough or interaction test | `tests/evidence/` or `production/qa/evidence/` |
| Config/Data | Smoke check or config validation | `tests/smoke/` or `production/qa/smoke-*.md` |

## CI

Tests should run on pull requests and pushes to `main`. A failed required test suite should block merge.
```

---

## 7. Engine-Specific Scaffold

### 7.1 Godot 4

Framework recommendation: GdUnit4 for automated tests plus smoke/evidence docs.

Create `tests/gdunit4_runner.gd` if missing:

```gdscript
# GdUnit4 test runner — invoked by CI and smoke/test workflows.
# Usage: godot --headless --script tests/gdunit4_runner.gd
extends SceneTree

func _init() -> void:
    var runner_script := load("res://addons/gdunit4/GdUnitRunner.gd")
    if runner_script == null:
        push_error("GdUnit4 not found. Install and enable the addon before running tests.")
        quit(1)
        return

    var runner = runner_script.new()
    runner.run_tests()
    quit(0)
```

Godot README running command:

```text
godot --headless --script tests/gdunit4_runner.gd
```

Manual install note:

```markdown
## Installing GdUnit4

1. Open Godot.
2. Use AssetLib or the official GdUnit4 installation instructions.
3. Enable the plugin in Project Settings → Plugins.
4. Verify `res://addons/gdunit4/` exists.
```

Do not install the addon automatically.

### 7.2 Unity

Framework recommendation: Unity Test Framework.

Create if missing:

```text
tests/EditMode/README.md
tests/PlayMode/README.md
```

`tests/EditMode/README.md`:

```markdown
# Edit Mode Tests

Use for pure logic, formulas, data validation, and editor-independent systems.

Recommended assembly definition: `EditModeTests.asmdef`.
Run via Unity Test Runner in Edit Mode or CI.
```

`tests/PlayMode/README.md`:

```markdown
# Play Mode Tests

Use for integration behavior that requires scenes, physics, coroutines, or runtime GameObjects.

Recommended assembly definition: `PlayModeTests.asmdef`.
Run via Unity Test Runner in Play Mode or CI.
```

Unity README running command:

```text
Use Window → General → Test Runner, or run Unity batchmode test commands in CI.
```

Do not create Unity license secrets. Note that `UNITY_LICENSE` must be configured manually if using GitHub Actions.

### 7.3 Unreal Engine

Framework recommendation: Unreal Automation Testing Framework.

Create `Source/Tests/README.md` if missing:

```markdown
# Unreal Automation Tests

Tests use the Unreal Automation Testing Framework.

Run in editor:
Session Frontend → Automation → select project tests.

Run headlessly:
`UnrealEditor <Project>.uproject -nullrhi -nosound -ExecCmds="Automation RunTests <Project>.; Quit" -log -unattended`

Naming:
- Test class: `F[SystemName]Test`
- Category: `<Project>.[System].[Feature]`
```

Unreal CI requires a self-hosted runner with Unreal Editor installed. Do not assume hosted GitHub runners have Unreal available.

---

## 8. CI Workflow

Create `.github/workflows/tests.yml` only if no compatible workflow exists.

If another test workflow exists, classify as EXISTS_OK, REPAIRABLE, or EXISTS_CONFLICT. Do not overwrite.

### 8.1 Godot workflow

```yaml
name: Automated Tests

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  test:
    name: Run Godot Tests
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4
        with:
          lfs: true

      - name: Run GdUnit4 Tests
        uses: MikeSchulze/gdUnit4-action@v1
        with:
          godot-version: '[engine-version]'
          paths: |
            tests/unit
            tests/integration
          report-name: test-results

      - name: Upload Test Results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results
          path: reports/
```

### 8.2 Unity workflow

```yaml
name: Automated Tests

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  test:
    name: Run Unity Tests
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4
        with:
          lfs: true

      - name: Run Edit Mode Tests
        uses: game-ci/unity-test-runner@v4
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
        with:
          testMode: editmode
          artifactsPath: test-results/editmode

      - name: Run Play Mode Tests
        uses: game-ci/unity-test-runner@v4
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
        with:
          testMode: playmode
          artifactsPath: test-results/playmode

      - name: Upload Test Results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results
          path: test-results/
```

### 8.3 Unreal workflow

```yaml
name: Automated Tests

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  test:
    name: Run Unreal Automation Tests
    runs-on: self-hosted

    steps:
      - name: Checkout
        uses: actions/checkout@v4
        with:
          lfs: true

      - name: Run Automation Tests
        run: |
          "$UE_EDITOR_PATH" "${{ github.workspace }}/[ProjectName].uproject" \
            -nullrhi -nosound \
            -ExecCmds="Automation RunTests [ProjectName].; Quit" \
            -log -unattended
        shell: bash

      - name: Upload Logs
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-logs
          path: Saved/Logs/
```

---

## 9. Smoke Test Seed

Create `tests/smoke/critical-paths.md` if missing:

```markdown
# Smoke Test: Critical Paths

**Purpose**: Run these checks before QA hand-off, release candidate approval, or hotfix deployment.
**Run via**: `/smoke-check`
**Expected Duration**: 10–15 minutes
**Update Rule**: Add entries when new core systems become release-critical.

## Core Stability

1. Game launches without crash.
2. Main menu loads.
3. New session can start.
4. Input responds without freezing.

## Core Mechanic

5. [Primary mechanic — update when the first core system is implemented.]

## Data Integrity

6. Save completes without error once save system exists.
7. Load restores correct state once load system exists.

## Performance

8. No obvious frame hitching on target hardware.
9. No visible memory growth during a short play session.

## Exit / Recovery

10. Player can quit to menu or desktop without crash.
```

---

## 10. Optional Example Test

Do not create a fake passing test that asserts nothing.

If a real first system exists, create a clearly marked starter test only when the target framework and system are known.

If no system exists yet, report:

```text
No example test created because no implemented system was detected. /gate-check requires at least one real example test before Pre-Production.
```

---

## 11. Apply Writes

If `--dry-run`, print the plan and stop.

Otherwise:

1. Create missing directories with `mkdir -p`.
2. Write CREATE files only when they do not exist.
3. Skip EXISTS_OK files.
4. For REPAIRABLE files, ask before using `Edit`.
5. For EXISTS_CONFLICT files, do not edit unless the user approves a specific patch.

After writing, re-read created files that are critical to the summary:

- `tests/README.md`
- `.github/workflows/tests.yml`
- Engine-specific runner/doc file
- `tests/smoke/critical-paths.md`

---

## 12. Post-Setup Summary

Output:

```markdown
## Test Setup Summary

| Field | Value |
|---|---|
| Engine | [engine] [version] |
| Framework | [framework] |
| Mode | [normal/force/repair/dry-run] |
| Files Created | [N] |
| Files Skipped | [N] |
| Conflicts | [N] |

### Created
- [path]

### Skipped Existing
- [path]

### Conflicts / Repairs Needed
- [path] — [reason]

### Manual Setup Required
- [engine-specific plugin/license/runner notes]

### Next Steps
1. Install/enable the engine test framework if required.
2. Add one real example test for the first implemented system.
3. Run `/smoke-check` after the first playable path exists.
4. Run `/qa-plan sprint` before the first implementation sprint.
```

Verdict:

- `COMPLETE` — missing scaffold created and no blocking conflicts remain.
- `PARTIAL` — scaffold created but manual framework setup or CI secrets are required.
- `BLOCKED` — engine unknown or conflicting existing CI requires user decision.
