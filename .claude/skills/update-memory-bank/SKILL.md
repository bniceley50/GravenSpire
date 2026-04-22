---
name: update-memory-bank
description: Promote repeating lessons from tasks/lessons.md into CLAUDE-patterns.md (cross-cutting) or .claude/rules/*.md (path-scoped). Scans for qualifying candidates, proposes a promotion batch, gets EDIT_OK, updates source entries with [PROMOTED] markers. Run at sprint end, after any outage/incident, or on demand.
---

# /update-memory-bank

Port of the clinic-notes-ai lessons-promotion ritual. Keeps
`tasks/lessons.md` from becoming a write-only log.

## When to run

- Sprint end (before `/retrospective`)
- After any outage, near-miss, or incident
- When `tasks/lessons.md` exceeds ~30 un-promoted entries
- On demand when the user says "update memory bank" or `/update-memory-bank`

## Procedure

### 1. Read `tasks/lessons.md` in full

### 2. Scan for promotion candidates

A lesson qualifies if **any** of:

- Tagged `[GLOBAL]` — always a candidate (may escalate to system prompt)
- Appears ≥2 times with different specifics (same underlying lesson)
- Matches an existing rule in `.claude/rules/*.md` and could be folded in

### 3. Classify each candidate into exactly one target

- `CLAUDE-patterns.md` — cross-cutting (touches ≥3 systems or ≥2 tiers)
- `.claude/rules/<specific-rule>.md` — path-scoped (touches one directory tree)
- `AGENTS.md` — universal behavioral rule (rare; prefer CLAUDE-patterns)
- `docs/brian-system-prompt-v4-6.md` — only if `[GLOBAL]` AND applies to all
  Brian projects. **Escalate to user; do not self-edit the system prompt.**

### 4. Propose the promotion set to the user as a single batch

```
Proposed promotions (N candidates):
- Lesson [YYYY-MM-DD tag]: "title" → <target file>
- …
May I apply all N? (EDIT_OK / reject list / reject all)
```

### 5. On EDIT_OK

- Append each promoted lesson to its target file (respecting target format)
- In `tasks/lessons.md`, mark the source entry with
  `[PROMOTED to <target> YYYY-MM-DD]`
- **Do not delete the source entry** — promotion is additive,
  `tasks/lessons.md` is archival

### 6. Report

- List of promotions made (path → section header)
- List rejected (if any) with user's stated reason
- Next-candidate threshold: which open lessons are close to qualifying but
  not yet there (1 occurrence so far, watch for a second)

## Output Contract

End the response with:

- **Files modified** (full paths)
- **Promotion count:** X of Y candidates applied
- **Open lessons still under threshold:** Z
- **Suggested next run date** (default: next sprint end)

## See Also

- `AGENTS.md` §9 — lesson tag taxonomy and promotion philosophy
- `tasks/lessons.md` — the source log
- `CLAUDE-patterns.md` — cross-cutting promotion target
- `.claude/rules/` — path-scoped promotion targets
