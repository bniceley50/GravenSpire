---
name: regression-suite
description: "Map test coverage to GDD critical paths, identify fixed bugs without regression tests, flag coverage drift from new features, and maintain tests/regression-suite.md. Run after implementing a bug fix or before a release gate."
argument-hint: "[update | audit | report] [--dry-run]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, AskUserQuestion
---

# Regression Suite Mapping

Map regression test coverage to GDD critical paths, fixed bugs, high-risk systems, and release gates, then produce or update a regression suite plan.

## 0. Execution Contract

### 0.1 Invocation and autonomy

Supported modes:

- full: full regression map
- bug:<id>: ensure regression coverage for one bug
- system:<name>: focus one system

The invocation authorizes routine repository-local work for this skill. Operate autonomously after resolving scope; do not ask the user to approve every normal file creation. Ask only for protected operations, destructive ambiguity, or missing source-of-truth decisions that cannot be inferred safely.

If `--dry-run` is present, perform discovery and produce the complete proposed result, but do not call `Write` or `Edit`.

### 0.2 Path safety

All user-supplied paths must be repository-relative. Reject absolute paths, paths containing `..`, and paths outside the expected project roots for this skill. Normalize paths before reading or writing.

### 0.3 Write policy

Routine writes allowed by invocation:

- production/qa/regression/regression-suite.md
- production/qa/regression/regression-gap-report-[YYYYMMDD].md

Protected operations require explicit confirmation through `AskUserQuestion`:

- Overwriting or deleting an existing file.
- Editing canonical source-of-truth documents outside the declared outputs.
- Changing statuses, gates, stage files, sprint state, story state, registry entries, or release readiness.
- Running commands that modify files, install dependencies, generate builds, publish artifacts, deploy, tag, commit, or push.
- Applying changes whose scope is broader than the user requested.

Use `Edit` for targeted changes to existing files. Use `Write` for new files or complete replacement only after the replacement scope is safe and approved.

### 0.8 Missing-file behavior

| Situation | Behavior |
|---|---|
| Primary source missing | Continue only if the skill can infer a narrower safe scope; otherwise stop with the exact missing file/folder. |
| Referenced artifact missing | Record as a gap or blocker instead of inventing content. |
| Existing target file present | Do not overwrite. Create a proposed dated file or ask for protected confirmation. |
| Ambiguous scope | Choose the smallest evidence-backed scope; ask only if two or more scopes are equally plausible. |
| Contradictory sources | Prefer explicit status/source-of-truth documents over generated reports; list the contradiction in the output. |

## 1. Discover Context

Read only the sources needed for the requested scope. Start with indexes, manifests, registries, and status files before reading large documents.

Primary sources:

- production/qa/bugs/**
- production/qa/evidence/**
- tests/**
- design/gdd/**
- docs/architecture/**
- production/releases/**

Discovery rules:

1. Prefer canonical source-of-truth files over generated reports.
2. Use `Glob` and `Grep` before reading large files.
3. Keep a source list for the final report or artifact.
4. When many files match, read the most relevant 5 to 10 first and summarize the rest as candidates.
5. Treat missing or draft-status dependencies as blockers, not as approval to invent content.

## 2. Build the Working Model

Use the discovered evidence to build a concise working model before producing output.

1. Identify fixed bugs, critical paths, release blockers, and high-risk systems.
2. Map each to existing tests or manual regression cases.
3. Classify coverage as Automated, Manual, Missing, Weak, or Not Applicable.
4. Write/append regression suite entries only for missing or weak coverage; do not edit test code.
5. Flag bugs that should not be closed until regression evidence exists.

Classification rules:

- **Blocking**: prevents safe implementation, review, release, or downstream skill execution.
- **High**: likely to cause rework, wrong implementation, invalid QA, or broken traceability.
- **Medium**: weakens handoff quality but can be resolved during normal follow-up.
- **Low**: cleanup, clarity, or optional improvement.

## 3. Produce the Artifact

Canonical outputs for this skill:

- production/qa/regression/regression-suite.md
- production/qa/regression/regression-gap-report-[YYYYMMDD].md

Artifact requirements:

1. Include scope, date, source list, assumptions, and confidence.
2. Separate facts from recommendations and inferred conclusions.
3. Include explicit next actions and the command that should be run next.
4. Preserve historical information; prefer additive notes over destructive rewrites.
5. When updating an index or manifest, append or update only the relevant row and preserve unrelated content.

Required report sections:

- Coverage summary
- Missing regressions
- Weak tests
- Suite updates
- Release risks

## 4. Validation

1. Check that every conclusion cites or names a repository source.
2. Check that all blockers have a concrete next action.
3. Check that proposed writes stay within the declared output paths.
4. Check that dry-run mode produced no writes.

Stop conditions:

- No blocking stop condition was encountered.

## 5. Final Response

End with a concise summary of what was written, what was skipped because it was protected or unsafe, validation results, and the recommended next command. Include file paths for every artifact created or proposed.
