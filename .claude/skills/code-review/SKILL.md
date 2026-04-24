---
name: code-review
description: "Performs an architectural and quality code review on a specified file or set of files. Checks for coding standard compliance, architectural pattern adherence, SOLID principles, testability, and performance concerns."
argument-hint: "[path-to-file-or-directory]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash, Task
agent: lead-programmer
---

# Code Review

Perform a repository-local code review for correctness, architecture conformance, maintainability, performance, testability, and implementation risk without modifying files.

## 0. Execution Contract

### 0.1 Invocation and autonomy

Supported modes:

- path-to-file: review one file
- path-to-directory: review bounded files under a directory
- blank: stop and request a path

This is a read-only skill. It may inspect files and run safe read-only diagnostics when Bash is allowed, but it must not create, edit, move, delete, rename, stage, commit, tag, deploy, publish, or update project state.

### 0.2 Path safety

All user-supplied paths must be repository-relative. Reject absolute paths, paths containing `..`, and paths outside the expected project roots for this skill. Normalize paths before reading or writing.

### 0.4 Bash safety

Bash is limited to diagnostics and read-only discovery unless the user explicitly approved a protected operation. Safe examples include `git status --short`, `git log`, `git diff --name-only`, existing test commands that do not update snapshots, and local grep/listing commands. Never run package installation, clean/reset, rm, deploy, publish, commit, tag, push, or build upload commands from this skill.

### 0.5 Task delegation

Use Task subagents only when they materially improve the result. Pass bounded context: the request, relevant source paths, current draft/report, and the exact verdict needed. Do not spawn duplicate reviewers. If review mode is available, use `solo` for no subagents, `lean` for only essential specialist review, and `full` for cross-functional or gate review.

### 0.8 Missing-file behavior

| Situation | Behavior |
|---|---|
| Primary source missing | Continue only if the skill can infer a narrower safe scope; otherwise stop with the exact missing file/folder. |
| Referenced artifact missing | Record as a gap or blocker instead of inventing content. |
| Existing target file present | Not applicable; this skill does not write. |
| Ambiguous scope | Choose the smallest evidence-backed scope; ask only if two or more scopes are equally plausible. |
| Contradictory sources | Prefer explicit status/source-of-truth documents over generated reports; list the contradiction in the output. |

## 1. Discover Context

Read only the sources needed for the requested scope. Start with indexes, manifests, registries, and status files before reading large documents.

Primary sources:

- requested path
- neighboring files needed to understand imports/contracts
- docs/architecture/control-manifest.md
- docs/architecture/adr-*.md
- design/gdd/**
- tests/**

Discovery rules:

1. Prefer canonical source-of-truth files over generated reports.
2. Use `Glob` and `Grep` before reading large files.
3. Keep a source list for the final report or artifact.
4. When many files match, read the most relevant 5 to 10 first and summarize the rest as candidates.
5. Treat missing or draft-status dependencies as blockers, not as approval to invent content.

## 2. Build the Working Model

Use the discovered evidence to build a concise working model before producing output.

1. Normalize the requested path and reject paths outside the repository.
2. Read the smallest necessary set of files to understand behavior and contracts.
3. Run safe diagnostics only when obvious and local, such as existing lint/test commands that do not modify files.
4. Check for contract violations, hidden coupling, untested branches, error handling gaps, performance traps, and maintainability issues.
5. Use Task only for specialist review when the code domain clearly benefits from a configured specialist.

Classification rules:

- **Blocking**: prevents safe implementation, review, release, or downstream skill execution.
- **High**: likely to cause rework, wrong implementation, invalid QA, or broken traceability.
- **Medium**: weakens handoff quality but can be resolved during normal follow-up.
- **Low**: cleanup, clarity, or optional improvement.

## 3. Produce the Read-Only Report

Return the report in chat. Do not write files. If a durable report would be useful, recommend the appropriate write-capable skill or command instead of creating it.

Required report sections:

- Verdict
- Blocking issues
- High-priority issues
- Medium/low issues
- Tests reviewed/run
- Architecture/GDD compliance
- Recommended patch direction

## 4. Validation

1. Check that every conclusion cites or names a repository source.
2. Check that all blockers have a concrete next action.
3. Check that proposed writes stay within the declared output paths.
4. Check that no writes or state changes were performed.
5. List every Bash command run and whether it was read-only or diagnostic.
6. Summarize any subagent verdicts and unresolved disagreements.

Stop conditions:

- No path was provided.

## 5. Final Response

End with a concise verdict, prioritized findings, evidence sources, and recommended next command. Do not imply that any files were changed.
