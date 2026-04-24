---
name: team-audio
description: "Orchestrate audio team: audio-director + sound-designer + technical-artist + gameplay-programmer for full audio pipeline from direction to implementation."
argument-hint: "[feature or area to design audio for] [--dry-run]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Bash, Task, AskUserQuestion, TodoWrite
---

# Team Audio

Orchestrate audio direction, sound design, implementation support, and QA evidence for a feature or area.

## 0. Execution Contract

### 0.1 Invocation and autonomy

Supported modes:

- feature or area description: orchestrate full team workflow
- --dry-run: produce plan and subagent summaries without writes

The invocation authorizes routine repository-local work for this skill. Operate autonomously after resolving scope; do not ask the user to approve every normal file creation. Ask only for protected operations, destructive ambiguity, or missing source-of-truth decisions that cannot be inferred safely.

If `--dry-run` is present, perform discovery and produce the complete proposed result, but do not call `Write` or `Edit`.

### 0.2 Path safety

All user-supplied paths must be repository-relative. Reject absolute paths, paths containing `..`, and paths outside the expected project roots for this skill. Normalize paths before reading or writing.

### 0.3 Write policy

Routine writes allowed by invocation:

- production/team/audio/[slug]-audio-plan.md
- production/team/audio/[slug]-implementation-notes.md
- production/qa/evidence/audio-[slug]-checklist.md

Protected operations require explicit confirmation through `AskUserQuestion`:

- Overwriting or deleting an existing file.
- Editing canonical source-of-truth documents outside the declared outputs.
- Changing statuses, gates, stage files, sprint state, story state, registry entries, or release readiness.
- Running commands that modify files, install dependencies, generate builds, publish artifacts, deploy, tag, commit, or push.
- Applying changes whose scope is broader than the user requested.

Use `Edit` for targeted changes to existing files. Use `Write` for new files or complete replacement only after the replacement scope is safe and approved.

### 0.4 Bash safety

Bash is limited to diagnostics and read-only discovery unless the user explicitly approved a protected operation. Safe examples include `git status --short`, `git log`, `git diff --name-only`, existing test commands that do not update snapshots, and local grep/listing commands. Never run package installation, clean/reset, rm, deploy, publish, commit, tag, push, or build upload commands from this skill.

### 0.5 Task delegation

Use Task subagents only when they materially improve the result. Pass bounded context: the request, relevant source paths, current draft/report, and the exact verdict needed. Do not spawn duplicate reviewers. If review mode is available, use `solo` for no subagents, `lean` for only essential specialist review, and `full` for cross-functional or gate review.

### 0.6 Todo tracking

Use `TodoWrite` for multi-phase orchestration. Keep todos tied to observable phases, not low-level file operations. Close todos only when evidence exists.

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

- design/gdd/**
- docs/architecture/**
- docs/registry/**
- production/stories/**
- production/qa/**
- design/art/**
- design/ux/**
- docs/engine-reference/**

Discovery rules:

1. Prefer canonical source-of-truth files over generated reports.
2. Use `Glob` and `Grep` before reading large files.
3. Keep a source list for the final report or artifact.
4. When many files match, read the most relevant 5 to 10 first and summarize the rest as candidates.
5. Treat missing or draft-status dependencies as blockers, not as approval to invent content.

## 2. Build the Working Model

Use the discovered evidence to build a concise working model before producing output.

1. Define the target feature/area and success criteria from existing docs before spawning specialists.
2. Use TodoWrite to track orchestration phases: context, specialist briefs, synthesis, artifact plan, QA/evidence handoff, and open blockers.
3. Spawn only the roles that materially contribute to this request: audio-director, sound-designer, technical-artist, gameplay-programmer, qa-tester.
4. Synthesize specialist output into one coherent plan focused on audio pillars, asset list, implementation hooks, mix/readability constraints, accessibility, and QA playback checks.
5. Write team artifacts and evidence checklists; do not modify canonical GDDs, ADRs, sprint status, or production code unless the invocation explicitly requests implementation and protected-write confirmation is obtained.

Classification rules:

- **Blocking**: prevents safe implementation, review, release, or downstream skill execution.
- **High**: likely to cause rework, wrong implementation, invalid QA, or broken traceability.
- **Medium**: weakens handoff quality but can be resolved during normal follow-up.
- **Low**: cleanup, clarity, or optional improvement.

## 3. Produce the Artifact

Canonical outputs for this skill:

- production/team/audio/[slug]-audio-plan.md
- production/team/audio/[slug]-implementation-notes.md
- production/qa/evidence/audio-[slug]-checklist.md

Artifact requirements:

1. Include scope, date, source list, assumptions, and confidence.
2. Separate facts from recommendations and inferred conclusions.
3. Include explicit next actions and the command that should be run next.
4. Preserve historical information; prefer additive notes over destructive rewrites.
5. When updating an index or manifest, append or update only the relevant row and preserve unrelated content.

Required report sections:

- Team verdict
- Specialist findings
- Integrated plan
- Artifacts/evidence created
- Open blockers
- Next command

## 4. Validation

1. Check that every conclusion cites or names a repository source.
2. Check that all blockers have a concrete next action.
3. Check that proposed writes stay within the declared output paths.
4. Check that dry-run mode produced no writes.
5. List every Bash command run and whether it was read-only or diagnostic.
6. Summarize any subagent verdicts and unresolved disagreements.

Stop conditions:

- No feature, area, or current story context can be inferred.

## 5. Final Response

End with a concise summary of what was written, what was skipped because it was protected or unsafe, validation results, and the recommended next command. Include file paths for every artifact created or proposed.
