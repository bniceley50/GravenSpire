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

### 2026-04-26 — [GLOBAL][SCOPE] Repair contract drift, park implementation detail

**Context:** Inventory & Item Economy full design review found six legitimate
blocker groups after authoring was complete. Only one blocker created active
cross-document drift: Inventory claimed Save/Load invoked
`InventoryFirstSaveMaterializer`, while Save/Load did not reverse-list it.
The remaining blockers were implementation-pre-spec work (schema identity,
partial-stack math, currency/vendor transaction closure, UI result handoff,
and future-system fixture gating). We repaired the Save/Load drift and parked
Inventory behind `INV-OQ-05` instead of running another large design round
before validating combat feel.

**Lesson:** When review uncovers many valid issues, separate false committed
claims from honest future work. Fix real cross-document drift immediately; do
not let broad implementation detail pull the project into over-design before
the current strategic risk is validated. A parked pre-spec entry is better
than pretending a system is approved, and better than burning a prototype
window on non-blocking precision.

**Evidence:** Commit `294a365` (Save/Load reverse-listing repair + Inventory
park); `design/gdd/inventory-item-economy.md` `INV-OQ-05`;
`production/session-state/active.md` prototype pivot entry.

**Promotion status:** open — promote to universal system prompt once repeated
across another project or another major Gravenspire review cycle.

---

### 2026-04-25 — [GLOBAL] Approved work hoarding without prompt protocol

**Context:** Across multiple sessions of the Pre-Production design pass,
~5,000 lines of `/design-review`-APPROVED design work (7 GDDs + 7 review
logs + systems-index updates + entities.yaml registry sync) accumulated
uncommitted in the working tree. The discipline "no commit without user
instruction" was honored, but no complementary discipline forced the agent
to *prompt* for commit at the moment of approval. The accumulation was
recovered by a single catch-up commit `f1df1c5` (2026-04-25), but a worktree
corruption or hard reset before the catch-up would have lost weeks of work.

**Lesson:** "No commits without user instruction" is necessary but
insufficient. It must be paired with an agent prompting obligation at every
approval checkpoint (`/design-review APPROVED`, `EDIT_OK` + verified batch,
test-passing implementation milestone, end-of-session). Approved work that
sits uncommitted across session boundaries is forgotten work and lost work
waiting to happen. The fix is structural protocol, not vigilance — quiet
competence reliably forgets the commit question; explicit prompts at
approval moments do not.

**Evidence:** `AGENTS.md` §14 (Commit & Push Cadence, added 2026-04-25);
`CLAUDE.md` Collaboration Protocol cross-reference; commit `f1df1c5`
(catch-up batch); commit `32e13a6` (the prior commit before the
accumulation, showing the gap window).

**Promotion status:** open — promote to universal system prompt
(`docs/brian-system-prompt-v4-6.md`) when that placeholder is populated;
this failure mode is universal across all Brian projects, not Gravenspire-
specific. Tag remains `[GLOBAL]` for that propagation pathway.

---

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
