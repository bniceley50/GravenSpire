# RECOVERY.md — Degraded-Capacity Operating Protocol

> Written 2026-05-30 while healthy, so it executes itself when not. The
> governance stack assumes a Brian who shows up; this file defines what
> Gravenspire is when he can't. Capacity dips are expected — medical (cervical
> spine), grief (kids), and consulting/day-job load are known forces. The
> failure mode this prevents: a bad week becoming a lost quarter by default,
> because the choice got made in the moment, badly.
>
> Source: 2026-05 premortem, death-stories #2 (body) and #10 (grief). Related
> decisions: D018 (LLM cost ceiling, referenced by the kill criterion below),
> D019 (minimum viable density).

## The three modes

### FULL — 8+ productive hours/week
Normal operation. New systems, the story slate, the works. This is the default
mode the rest of the governance stack assumes.

### MAINTENANCE — 2–4 hours/week
**Triggered by:** a bad medical stretch, a hard week with the kids, consulting
crunch, or any week the Full bar is clearly not happening.
- **No new systems.** Stabilization only.
- Exactly **one tiny shippable win per week** — a bug fix, an evidence backfill,
  one small green PR. Momentum over magnitude.
- The Codex / main-lane split still holds; just smaller batches.
- Do **not** open new stories. Do **not** start architecture work.
- Update `production/session-state/active.md` so the next Full week resumes cold.

### HIBERNATE — 0 hours
**Triggered by:** a medical or personal stretch where the project cannot be
touched.
- **One 10-minute check-in per week**, no more: `git status` clean, branches not
  rotting, no half-merged state, tooling still runs. Push nothing, build nothing.
- The repo is left in a known-good, walk-away-safe state (no uncommitted WIP on
  main, no open half-done merge).
- Explicitly **not guilt.** Hibernate is a sanctioned state, not a failure.

## Entering and leaving a mode
- Name the mode in `active.md`'s stage note when it changes. One line.
- Leave in order: Hibernate → Maintenance → Full. Don't jump Hibernate→Full in
  week one. One shippable win before any new system.

## What never degrades, even in Hibernate
- Secrets / PII rules (always-on, per `docs/brian-system-prompt-v4-6.md` §14).
- No commits/pushes to main without intent.
- The repo stays walk-away-safe.

## The kill criterion (write now, honor later)

Sunk cost eats years 3–4 of a life. To wind Gravenspire down *honorably* (and
consolidate effort on consulting / other projects) rather than let it rot,
pre-commit to triggers. **These are TODO placeholders — set the real numbers
before they are needed, while healthy enough to set them honestly:**

- [ ] No playable T2 alpha by **`[TODO — DATE, e.g. 18 months out]`**, OR
- [ ] Wishlist→sale conversion below **`[TODO — X%, e.g. 1.5%]`** at Steam Next
      Fest, OR
- [ ] LLM dialogue cost-per-CCU-hour still above **`[TODO — $Y]`** after three
      serious optimization passes (see `DECISIONS.md` D018), OR
- [ ] Sustained Hibernate for **`[TODO — N months]`** with no intent to return.

Hitting a kill trigger means a **deliberate wind-down decision, not auto-quit** —
but it forces the conversation instead of letting drift make the choice silently.
The kill criterion is the thing that prevents sunk cost from eating years 3 and 4.
Write it now, when you don't need it. Honor it later, when you do.
