# Post-S2-M3-00 Worktree Playtest (M2 Loop)

**Date:** 2026-05-15
**Context:** Post-`/dev-story` worktree playtest of the M2 combat camp loop in `_DevEntry.unity` under Unity 6000.3.14f1.
**Player:** Brian
**Session duration:** _TBD_
**S2-M3-00 status at playtest:** Implemented (pending `/code-review` + `/story-done`). The S2-M3-00 changeset is editor-only infrastructure (shared SearchInit filter helper + 4 runner `CaptureLog` modifications); runtime / scene / controller are byte-identical to commit `dc0f306` (S2-M2-04 closure).

**Verification artifact:** _Not applicable._ S2-M3-00 has no human-play acceptance criterion (`production/stories/s2-m3-00-scenario-smoke-handoff-cleanup.md:60` through `:68`). This playtest is an open exploration of the M2 loop's current state, intentionally framed against the open `m2_presentation_threshold_gap` carryover from S2-M2-02 (`production/session-state/active.md:240`) and as informal input for M3-01 readiness scoping.

## The Question

> Did you want one more pull?

(Same central feel question as `tests/evidence/S2-M2-02/human-play-20260512.md:11`. Mechanical loop has not changed since S2-M2-04 — what's tested here is whether the loop still reads the same way on a fresh playthrough, and whether anything noticed earlier feels different now.)

**Answer:**
_TBD — fill in after play._

## Worst-Thing Finding

_One specific worst-thing observation. Keep it concrete: a single sentence describing what felt wrong and why. Mirror the S2-M2-02 worst-thing framing._

## In-Loop Findings

_Things noticed during the session. Categorize each as_:
- **fix-in-loop** _(small enough to patch right away if motivated)_
- **carry-forward** _(real issue, but bigger than this session; goes to `production/sprint-status.yaml` carryover or `tasks/lessons.md`)_
- **out-of-scope** _(real, but belongs to M3/M4/M5 design)_

- _TBD_
- _TBD_
- _TBD_

## Worst-Thing Disposition

_One of:_
- **fix-in-this-batch** _(patch S2-M3-00 worktree before `/code-review`)_
- **carry-forward** _(add to `production/sprint-status.yaml` under `/story-done`)_
- **out-of-scope** _(capture as M3+ design input; do not patch)_

## Notes for M3-01 Readiness

_Anything that should shape the next dev-story (Named NPC Objective Frame):_

- _Named NPC presentation should be distinguishable from the named-blocker capsule presentation. What would help?_
- _Objective framing without a quest marker — does the current scene have enough visual hierarchy for a hand-off NPC to feel intentionally placed?_
- _The HUD currently centers on combat state; does it have room to also show an "objective accepted / relic carried" state without overcrowding?_

_TBD — fill in based on play._

## Routing

- If a **fix-in-this-batch** worst-thing emerges that is in-scope for S2-M3-00 (editor-tooling / evidence / runner hygiene), patch before `/code-review` lands.
- If a **carry-forward** emerges, candidate for `production/sprint-status.yaml` carryover at `/story-done` time, or a durable lesson in `tasks/lessons.md`.
- If an **out-of-scope** finding emerges, capture as M3+ design input — feed into the M3-01 readiness gate or, for M4/M5, into the corresponding `/quick-design`.

## Framework Notes

- **Batch shape:** Standalone playtest invited mid-`/dev-story` between implementation and `/code-review`, not a story-AC playtest. Treats playtest as cheap exploration, consistent with the "play-immediately" pattern (`production/session-state/active.md:115`).
- **Presentation-threshold check:** This is the explicit S2-M2-02 carryover trigger (`m2_presentation_threshold_gap`). Re-evaluating whether anything in the M2 loop's mechanical-coherence proof reads differently on a fresh play, now that the M2 milestone is closed.
- **Memory cross-ref:** `feedback_play_immediately_dev_loop.md` — implement one small thing → play it → note what felt bad → fix worst → commit → repeat. This file captures the "play it / note what felt bad" step for the current cycle.
