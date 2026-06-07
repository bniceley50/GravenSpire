# S4-04: Cast Bar + Interaction Prompt

> **Sprint**: Sprint 4 — EQ-Readable Presentation Slice
> **Sprint Plan**: `production/sprints/sprint-4.md` (Story Ledger, S4-04)
> **Status**: Blocked (depends on S4-00, S4-02)
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: 1.0 day
> **Manifest Version**: Unavailable (control-manifest absent project-wide; documented fallback applies)
> **Generated**: 2026-06-07
> **Owner**: Codex

## Context

**Authority**: `DECISIONS.md` D020; revised art bible §7.1.1 (Cast Bar; Interaction
Prompt).

**Requirement Summary**: Implement the two remaining Layer 1 State-Report elements: the
**cast bar** (a linear readout of an ongoing cast) and the **interaction prompt** (a
proximity/focus-gated label naming the single focused entity, e.g. "Speak — Caretaker
Morrvik"). Both directly fix S3-06 readability failures: the player could not read their
cast or their interaction. Serves §7.12 criteria 3 (interaction confirmation) and 4
(cast-state read). Input-display wording (which key/glyph names the verb) is settled in
implementation against ux; the art direction fixes only that it is a quiet,
single-target, range+facing-gated prompt.

**Governing decisions**:

| D-entry | Status | Usage |
|---|---|---|
| D020 | Locked | EQ-readability pivot; the cast bar + prompt this executes |
| D016 | Locked | Greybox; legibility floor |

**Art-bible authority**: §7.1.1 (Cast Bar: linear readout, lower-center, clears the
forbidden band, no completion flourish; Interaction Prompt: proximity/focus-gated,
single focused target, screen-space not world-floating); §7.11 (FORBIDS — including
the cast-bar exceptions to the center-band and snap-to-value rules); §5.2 (the focused
interaction prompt's world/interface-register boundary).

**Surfaces consumed**: the S4-00 cast-bar placement validation; the S4-02 Layer 1
vocabulary. The S3-01 harness already owns the raw interact dispatch + a basic
range-gated prompt — this story presents it per the bible, it does not re-author dispatch.

**Engine**: Unity 6.3 LTS. **Engine Risk**: MEDIUM (UI Toolkit/UGUI; cast-timing readout).

## Acceptance Criteria

- [ ] **S4-04-01**: A **cast bar appears when the player initiates a cast with cast-time > 0**, fills **left-to-right linearly** over the cast duration (a literal readout, not animation-for-feel; §7.9-permitted linear), and **disappears on completion / interrupt / fizzle** with **no flourish** (the bar reaches full and disappears; the spell fires).
- [ ] **S4-04-02**: The cast bar is placed **lower-center, clearing the 40-60% forbidden band** at the S4-00-validated placement (the sole element permitted in the lower-center zone, present only during an active cast — §7.11 exception).
- [ ] **S4-04-03**: An **interaction prompt** appears when the player is within interaction range and oriented (~60° forward cone) toward an interactable entity, naming the **single focused entity** and its verb (e.g. "Speak — Caretaker Morrvik"). It disappears on out-of-range or turn-away.
- [ ] **S4-04-04**: The interaction prompt is **screen-space Layer 1 text** (Iron Seam panel, quiet, no glow/pulse/excited state) — **NOT floating world-space text over the NPC**, and **never a persistent across-room indicator** (§7.1.1 / §5.2). It names only the single focused target.
- [ ] **S4-04-05**: §7.12 criteria 3+4: interaction confirmation (the prompt named the target before the player triggered it; the result is legible) and cast-state read (the cast bar is readable from peripheral vision while monitoring health).
- [ ] **S4-04-06**: Neither element routes (§7.11 boundary): no "go here" hint, no across-room prompt, no objective-advertising. The interaction prompt does not appear in combat with an active target (interaction is combat-blocked).

## Implementation Notes

- The interaction prompt presents the existing S3-01 harness dispatch — it does not re-author the interact path. Input-display wording is a ux detail resolved in implementation; the prompt is quiet, single-target, range+facing gated.
- Build the cast-bar placement to the S4-00 validation (clears the forbidden band); if S4-00 left it open, escalate rather than guess.
- Verify Unity 6.3 UI Toolkit/UGUI APIs against `docs/engine-reference/unity/`.

## Out of Scope

- Target frame + con (S4-03).
- Player vitals (S4-02).
- A combat/spell system — the cast bar presents cast state; it does not implement casting.
- Floating world-space prompt text (forbidden).

## QA Test Cases

**Manual check (S4-04-01/02 cast bar)**
- Setup: initiate a timed cast; let it complete, then interrupt one.
- Verify: linear fill, no completion flourish, disappears on interrupt; sits clear of the 40-60% band; readable peripherally.
- Pass: §7.12 criterion 4; §7.11 cast-bar exception honored.

**Manual check (S4-04-03/04 interaction prompt)**
- Setup: approach `M3_Caretaker` within range and facing; then turn away / leave range; then approach across the room.
- Verify: prompt names "Caretaker" only when in range + facing; vanishes on turn-away; never visible across the room; screen-space not floating over the NPC.
- Pass: §7.12 criterion 3; proximity/focus-gated, single-target.

## Test Evidence

**Required evidence**: `production/qa/evidence/s4-04-cast-prompt-evidence.md`
(screenshots: cast bar mid-cast / interrupt; interaction prompt in-range vs.
out-of-range/across-room).

**Evidence status**: Not started

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| `S4-00` | The cast-bar placement validation (clears the forbidden band) | Done |
| `S4-02` | The Layer 1 vocabulary the cast bar + prompt reuse | Done |

## Blockers

Blocked on S4-00 (cast-bar placement) and S4-02 (Layer 1 base). Kept separate from
S4-03 per plan decision.
