---
name: ux-review
description: "Validates a UX spec, HUD design, or interaction pattern library for completeness, accessibility compliance, GDD alignment, and implementation readiness. Produces APPROVED / NEEDS REVISION / MAJOR REVISION NEEDED verdict with specific gaps."
argument-hint: "[file-path or 'all' or 'hud' or 'patterns']"
user-invocable: true
allowed-tools: Read, Glob, Grep
agent: ux-designer
---

# UX Review

Validate UX specs, HUD designs, and interaction pattern documents for completeness, accessibility, GDD alignment, player clarity, and implementation readiness without modifying files.

## 0. Execution Contract

### 0.1 Invocation and autonomy

Supported modes:

- file-path: review one UX file
- all: review all UX docs
- hud: review HUD design
- patterns: review interaction patterns

This is a read-only skill. It may inspect files and run safe read-only diagnostics when Bash is allowed, but it must not create, edit, move, delete, rename, stage, commit, tag, deploy, publish, or update project state.

### 0.2 Path safety

All user-supplied paths must be repository-relative. Reject absolute paths, paths containing `..`, and paths outside the expected project roots for this skill. Normalize paths before reading or writing.

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

- design/ux/**
- design/gdd/**
- design/art/art-bible.md
- docs/architecture/**
- production/playtests/**

Discovery rules:

1. Prefer canonical source-of-truth files over generated reports.
2. Use `Glob` and `Grep` before reading large files.
3. Keep a source list for the final report or artifact.
4. When many files match, read the most relevant 5 to 10 first and summarize the rest as candidates.
5. Treat missing or draft-status dependencies as blockers, not as approval to invent content.

## 2. Build the Working Model

Use the discovered evidence to build a concise working model before producing output.

1. Resolve target UX documents.
2. Check user goals, flow completeness, states, accessibility, input/platform support, visual hierarchy, feedback, error handling, copy, analytics, QA criteria, and implementation handoff.
3. Compare UI terms and states against GDD systems and art direction.
4. Classify verdict as APPROVED, NEEDS REVISION, or MAJOR REVISION NEEDED.
5. Return precise revision instructions; do not edit UX files.

Classification rules:

- **Blocking**: prevents safe implementation, review, release, or downstream skill execution.
- **High**: likely to cause rework, wrong implementation, invalid QA, or broken traceability.
- **Medium**: weakens handoff quality but can be resolved during normal follow-up.
- **Low**: cleanup, clarity, or optional improvement.

## 3. Produce the Read-Only Report

Return the report in chat. Do not write files. If a durable report would be useful, recommend the appropriate write-capable skill or command instead of creating it.

Required report sections:

- Verdict
- Blocking UX gaps
- Accessibility issues
- GDD alignment
- Implementation readiness
- Recommended revisions

## 4. Validation

1. Check that every conclusion cites or names a repository source.
2. Check that all blockers have a concrete next action.
3. Check that proposed writes stay within the declared output paths.
4. Check that no writes or state changes were performed.

Stop conditions:

- No blocking stop condition was encountered.

## 5. Final Response

End with a concise verdict, prioritized findings, evidence sources, and recommended next command. Do not imply that any files were changed.
