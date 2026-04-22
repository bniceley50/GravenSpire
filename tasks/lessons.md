# tasks/lessons.md — Accumulated Lessons

Append-only log of lessons learned. Tag with the taxonomy in `AGENTS.md` §9.
Promote repeating lessons to `CLAUDE-patterns.md` (cross-cutting) or a
`.claude/rules/*.md` file (path-scoped). Mark promoted entries with
`[PROMOTED to <target> YYYY-MM-DD]`.

---

## Format

```
### YYYY-MM-DD — [TAG][TAG2] one-line title
**Context:** what happened, one paragraph
**Lesson:** the generalized rule this teaches
**Evidence:** file path(s) or commit SHA
**Promotion status:** open / promoted to <target> on YYYY-MM-DD
```

---

## Entries (newest first)

### 2026-04-21 — [GLOBAL][CI] Pre-build/pre-deploy config verification is not automatic

**Context:** Inherited lesson from clinic-notes-ai 2026-04-10. A production
outage occurred because `UPSTASH_REDIS_REST_TOKEN` was missing from the Vercel
Production env. The governance had no pre-deploy env-vars check — the team
assumed "configured" meant configured, with no verification step that
produced evidence. Same class of failure will hit Gravenspire when a Steam
build goes out with a broken Addressables group, a stale version stamp, or
(T3+) a missing server config.

**Lesson:** Every deploy/build surface needs a verification checklist that
produces evidence (file:line, screenshot, console output), **not self-report**.
"I checked" is not evidence. "Here's the line" is evidence. For Gravenspire,
the checklist is `production/pre-build-checklist.md`.

**Evidence:** `production/pre-build-checklist.md` (created 2026-04-21 as part
of the D005 governance migration); `AGENTS.md` §12.

**Promotion status:** open — promote to `.claude/rules/game-dev-governance.md`
if the same class of near-miss recurs during T1.

---

<!-- Add new lessons above this line (newest first). -->
