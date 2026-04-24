---
name: scope-check
description: "Analyze a feature or sprint for scope creep by comparing current scope against the original plan. Flags additions, quantifies bloat, and recommends cuts. Use when user says 'any scope creep', 'scope review', 'are we staying in scope'."
argument-hint: "[feature-name or sprint-N]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash
model: haiku
---

# Scope Check

Compare current work, feature requests, or sprint contents against original scope to detect scope creep, hidden dependencies, and schedule risk.

## 0. Execution Contract

### 0.1 Invocation and autonomy

Supported modes:

- feature/story/sprint identifier
- blank: infer current sprint scope

This is a read-only skill. It may inspect files and run safe read-only diagnostics when Bash is allowed, but it must not create, edit, move, delete, rename, stage, commit, tag, deploy, publish, or update project state.

### 0.2 Path safety

All user-supplied paths must be repository-relative. Reject absolute paths, paths containing `..`, and paths outside the expected project roots for this skill. Normalize paths before reading or writing.

### 0.4 Bash safety

Bash is limited to diagnostics and read-only discovery unless the user explicitly approved a protected operation. Safe examples include `git status --short`, `git log`, `git diff --name-only`, existing test commands that do not update snapshots, and local grep/listing commands. Never run package installation, clean/reset, rm, deploy, publish, commit, tag, push, or build upload commands from this skill.

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

- production/sprints/**
- production/stories/**
- production/epics/**
- design/gdd/**
- docs/architecture/**
- git diff/status when useful

Discovery rules:

1. Prefer canonical source-of-truth files over generated reports.
2. Use `Glob` and `Grep` before reading large files.
3. Keep a source list for the final report or artifact.
4. When many files match, read the most relevant 5 to 10 first and summarize the rest as candidates.
5. Treat missing or draft-status dependencies as blockers, not as approval to invent content.

## 2. Build the Working Model

Use the discovered evidence to build a concise working model before producing output.

1. Identify original scope baseline and current requested/implemented scope.
2. Use safe git status/diff-name-only commands when implementation drift matters.
3. Classify additions as in-scope clarification, scope expansion, dependency discovery, bug fix, or unrelated work.
4. Estimate impact on effort, QA, design, and release risk.
5. Return recommended cut/defer/approve decisions without changing plans.

Classification rules:

- **Blocking**: prevents safe implementation, review, release, or downstream skill execution.
- **High**: likely to cause rework, wrong implementation, invalid QA, or broken traceability.
- **Medium**: weakens handoff quality but can be resolved during normal follow-up.
- **Low**: cleanup, clarity, or optional improvement.

## 3. Produce the Read-Only Report

Return the report in chat. Do not write files. If a durable report would be useful, recommend the appropriate write-capable skill or command instead of creating it.

Required report sections:

- Scope verdict
- Original scope
- New/expanded work
- Impact
- Recommended cuts/deferments
- Risks

## 4. Validation

1. Check that every conclusion cites or names a repository source.
2. Check that all blockers have a concrete next action.
3. Check that proposed writes stay within the declared output paths.
4. Check that no writes or state changes were performed.
5. List every Bash command run and whether it was read-only or diagnostic.

Stop conditions:

- No blocking stop condition was encountered.

## 5. Final Response

End with a concise verdict, prioritized findings, evidence sources, and recommended next command. Do not imply that any files were changed.
