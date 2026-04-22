# RED_TEAM_RUBRIC.md — Scoring Guide

Companion to `RED_TEAM.md`. Defines what PASS / CONCERN / FAIL means per
section so reviewers converge on verdicts instead of flipping coins.

---

## Severity Levels

| Severity | Definition | Example |
|---|---|---|
| **LOW** | Cosmetic, non-blocking, fix opportunistically | Inconsistent log format in non-production code path |
| **MED** | Should fix before merge but doesn't block a tier gate | Missing rate limit on a rep-change endpoint |
| **HIGH** | **Blocks merge.** Blocks tier advancement if tier-gated. | Client-authoritative hit detection in T2+, tamperable save in T3+, LLM call without moderation in T3+ |

A single HIGH → section **FAIL**. Multiple MED → section **CONCERN**. Any FAIL
→ overall **REJECT**.

---

## Section Rubrics

### §1 — Scope and Claims

- **PASS:** claims are specific and falsifiable
- **CONCERN:** vague claims (e.g., "improves performance" without a number)
- **FAIL:** scope creep vs linked design doc / GDD / DECISION

### §2 — Reproducible Setup

- **PASS:** I can reproduce from the doc alone, no ambient knowledge needed
- **CONCERN:** missing one step but the gap is guessable
- **FAIL:** can't reproduce from the doc

### §3 — Positive Path Evidence

- **PASS:** every claim has file:line + passing test
- **CONCERN:** claim backed by playtest note only where an automated test was feasible
- **FAIL:** a claim has no evidence at all

### §4 — Negative Path Attempts

- **PASS:** all four categories (bad input / missing data / ordering / interruption) were exercised
- **CONCERN:** 2–3 of the four exercised
- **FAIL:** 0–1 exercised

### §5 — Resource and Performance

- **PASS:** within all budgets
- **CONCERN:** regressed a soft budget (e.g., allocation churn up but frame-rate holds)
- **FAIL:** regressed a hard budget (frame-rate, memory ceiling, draw-call cap)

### §6 — Save Integrity and Client Authority

- **PASS:** save tamper detected + rejected; no client compute of server-owned state in T2+
- **CONCERN:** tamper detected but logged only, not rejected
- **FAIL:** save can be edited to grant state, or client computes server-owned state

### §7 — Server-Authoritative Validation

- **PASS:** all destructive actions server-validated + rate-limited + audit-logged
- **CONCERN:** validated but not audit-logged
- **FAIL:** client can send a packet that mutates server state without validation (T2+)

### §8 — Faction Sim Determinism and Rep Change Auditing

- **PASS:** sim is seed-deterministic; rep changes fully audited (all 6 fields)
- **CONCERN:** sim deterministic but replay test missing
- **FAIL:** sim diverges across replays, OR rep changes missing audit fields (actor / target / delta / cause / timestamp / server_tick)

### §9 — Save Format Versioning

- **PASS:** version stamp + forward migration + backward load-as-error all tested
- **CONCERN:** version stamp present, no migration test
- **FAIL:** no version stamp, or silent data loss on load

---

## Verdict Table

| Section Verdicts | Overall Verdict |
|---|---|
| All PASS | **APPROVE** |
| ≥1 CONCERN, 0 FAIL | **CONCERNS** (reviewer discretion; often merge with a follow-up ticket) |
| ≥1 FAIL | **REJECT** (block merge until fixed) |

---

## Calibration Notes

- When in doubt between CONCERN and FAIL, ask: **"would I be comfortable
  shipping this to real players?"** If no, it's FAIL.
- Playtest notes count as evidence ONLY for §6–§9 if an automated test is
  genuinely infeasible. Otherwise require the test.
- "I'll add a test in a follow-up" is not evidence. Either the test is here
  or the section is CONCERN/FAIL.
