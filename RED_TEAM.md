# RED_TEAM.md — Adversarial Review Protocol

Run when a PR touches a risky subsystem per the tier trigger rules below.
Output is a saved audit at `docs/audits/RED_TEAM_[YYYY-MM-DD]_[scope].md`.
Every finding cites file:line + verification method (see `AGENTS.md` §3).

---

## Tier Trigger Rules

| Tier | Trigger |
|---|---|
| **T1** | **Skip.** Use the 4-question PR check in the PR template only. |
| **T2** | Run on PRs touching `src/networking/**` or `Assets/Scripts/Networking/**`. Other PRs stay on the 4-question check. |
| **T3** | Run on PRs touching networking, `src/ai/**` (faction sim + LLM), or `src/core/save/**`. **One tier-transition RED_TEAM required before opening the server to real players.** |
| **T4** | Run on all PRs touching risky subsystems. Monthly full-surface audit. |

**Current tier: T1** (per `DECISIONS.md` D003) — this protocol is **inactive**
until T2. The file exists so the shape is ready when the tier advances.

---

## Sections (run all 9 when active)

### §1 — Scope and Claims

What does the PR/feature claim to do, in one paragraph? List the claims as
bullet points so they can be falsified individually.

### §2 — Reproducible Setup

Exact steps to reproduce the audited behavior locally. Include commit SHA,
scene name, test data path, seed values, and starting tier state.

### §3 — Positive Path Evidence

For each claim in §1, cite the file:line that implements it and the test
(unit, integration, or playtest note) that verifies it. **Missing evidence
is a finding**, not an acceptable gap.

### §4 — Negative Path Attempts

Try to make it fail. For each of the four categories below, record the
attempt and outcome:

- **Bad input** — out-of-range values, NaN, empty, absurdly large
- **Missing data** — required reference null, file not found, corrupted save
- **Ordering anomalies** — init order, save-during-action, frame-boundary edge
- **Interruptions** — process kill mid-save, network drop mid-sync, disconnected peer

### §5 — Resource and Performance

Frame time under load, memory growth over time, allocation churn, GC pauses.
Compare against the performance budgets in
`.claude/docs/technical-preferences.md`. Flag anything that regresses a budget.

### §6 — Save Integrity and Client Authority
*(was PHI/HIPAA in clinic-notes)*

- Can a modified save file grant unearned state? (reputation, inventory,
  unlocks, quest flags)
- Does the client ever compute a value the server should own? (T2+)
- Are save-format version numbers checked on load?
- Is an HMAC or signature present on the save file?

### §7 — Server-Authoritative Validation Gaps
*(was auth-bypass in clinic-notes)*

- Can a client craft a packet that grants state it shouldn't own? (T2+)
- Are hit-registration, item grants, and reputation changes all
  server-validated?
- Rate limits on destructive actions?
- Audit log for high-value actions (item grants, rep swings above threshold)?

### §8 — Faction Sim Determinism and Rep Change Auditing
*(was audit-log in clinic-notes)*

- Is the faction sim deterministic given `(seed, tick, input sequence)`?
- Can two clients or two replays diverge given the same inputs?
- Are rep-change events logged with: `actor, target, delta, cause, timestamp,
  server_tick`?

### §9 — Save Format Versioning and Migration
*(was schema / RLS / migrations in clinic-notes)*

- Does the new save format carry a version stamp as the **first** field?
- Is there a migration path from the previous version?
- Does loading an older save fail loud (error), not silent (corrupt-load)?
- Is the migration tested against **real** save files from prior versions, not
  synthetic fixtures?

---

## Output Template

For each section:

```
### §N — [Name]
**Status:** PASS / CONCERN / FAIL / N/A
**Findings:** bullet list; each with file:line + severity (LOW / MED / HIGH)
**Evidence:** paths to tests, playtest notes, or screenshots
```

Sign-off line at the end:

```
**RED_TEAM Verdict:** APPROVE / CONCERNS / REJECT
**Auditor:** [agent name or human]
**Date:** YYYY-MM-DD
**Scope:** [subsystem / PR number / feature name]
```

See `RED_TEAM_RUBRIC.md` for PASS/CONCERN/FAIL criteria per section.
