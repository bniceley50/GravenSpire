# S4-03: Target Frame + Relative-Threat (Con) Indicator

> **Sprint**: Sprint 4 — EQ-Readable Presentation Slice
> **Sprint Plan**: `production/sprints/sprint-4.md` (Story Ledger, S4-03)
> **Status**: Blocked (depends on S4-00, S4-02)
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: 1.0 day
> **Manifest Version**: Unavailable (control-manifest absent project-wide; documented fallback applies)
> **Generated**: 2026-06-07
> **Owner**: Codex

## Context

**Authority**: `DECISIONS.md` D020; revised art bible §7.1.1 (Target Frame;
Relative-Threat Indicator), §7.8 (faction icon-frame modifications), §4.6 (colorblind
accessibility).

**Requirement Summary**: Implement the selection-gated **target frame** (the single
most important EQ-legibility addition) and the 5-state **relative-threat "con"
indicator** inside it. Both are State Report — they appear because the player selected
a target, and report the state of that chosen relationship. Neither appears on
un-selected entities in the world (that would be World Performance). Serves §7.12
criterion 2 (target identification within ~1s of selection).

**Governing decisions**:

| D-entry | Status | Usage |
|---|---|---|
| D020 | Locked | EQ-readability pivot; the target frame + con this executes |
| D016 | Locked | Greybox; legibility floor |

**Art-bible authority**: §7.1.1 (Target Frame: selection-gated; Relative-Threat
Indicator: 5 shape-primary non-color-only glyphs); §7.8 (faction frame treatments —
Court recessed-surround, Syndicate ledger-notch, etc.); §4.3 (semantic color
vocabulary — palette colors confirm, never RPG danger-red/reward-gold); §4.6
(non-color-only); §7.11 (State-Report boundary).

**Surfaces consumed**: the S4-00 con-glyph shape set + discriminability validation;
the S4-02 Layer 1 vocabulary baseline.

**Engine**: Unity 6.3 LTS. **Engine Risk**: MEDIUM (UI Toolkit/UGUI; target-selection
input wiring).

## Acceptance Criteria

- [ ] **S4-03-01**: A **target frame appears when the player selects a target** (tab or click) and **disappears on deselect or target death**. It is placed in the upper-left periphery, never center-screen.
- [ ] **S4-03-02**: The frame shows target **name** (Layer 1 Medium typography), target **health bar** (same spec as the player health bar at the S4-00 floor), and the **faction frame treatment** per §7.8 (Court recessed-surround / Syndicate ledger-notch / etc.).
- [ ] **S4-03-03**: The **5-state con glyph** (Trivial / Below / Even / Above / Dangerous) renders inside the frame, left of the name, using the **S4-00-validated shape-primary line-glyphs**. It is **non-color-only** (§4.6): shape communicates, color (world-palette per §7.1.1) only confirms.
- [ ] **S4-03-04**: The target frame and con indicator **never appear on an entity the player has NOT selected** (§7.11 State-Report boundary). No floating health bar or con color over un-targeted world entities.
- [ ] **S4-03-05**: The con indicator is a **recessive line-glyph, not a loud color badge** (§7.11 fence; the §7.1.1 reconciliation of "fully specified" with "quiet/recessive").
- [ ] **S4-03-06**: §7.12 criterion 2: on selecting a target, its name and rough threat level are legible within ~1s. No forbidden Layer 1 treatments (§7.11: glow/gradient/rounded/red/green).

## Implementation Notes

- Build the con glyphs from the S4-00 shape set — do not invent shapes here.
- Selection model: tab/click target selection (EQ con model — the player chose to examine the target). If a selection system doesn't exist yet, the minimal version is in scope; a full targeting system is not.
- Verify Unity 6.3 UI Toolkit/UGUI APIs against `docs/engine-reference/unity/`.

## Out of Scope

- Cast bar + interaction prompt (S4-04).
- Player vitals (S4-02).
- A full combat-targeting system beyond what selection-gating the frame requires.
- Any con/threat display on un-selected entities (forbidden).

## QA Test Cases

**Manual check (S4-03-01/04 selection-gating)**
- Setup: in the district, select a target, then deselect.
- Verify: frame appears on select, vanishes on deselect; no frame/con on un-selected entities anywhere in view.
- Pass: strictly selection-gated; zero world-broadcast.

**Manual check (S4-03-03/05 con glyph)**
- Setup: select targets of varying relative threat; disable color.
- Verify: the 5 states are distinguishable by shape alone; the glyph reads as a quiet line-glyph, not a loud badge.
- Pass: §7.12 criterion 2 + §7.11 recessive-con held.

## Test Evidence

**Required evidence**: `production/qa/evidence/s4-03-target-frame-evidence.md`
(screenshots: frame on select / absent on deselect / absent on un-targeted entities;
the 5 con states color-disabled).

**Evidence status**: Not started

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| `S4-00` | The validated con-glyph shape set + discriminability proof | Done |
| `S4-02` | The Layer 1 vocabulary + health-bar spec the target frame reuses | Done |

## Blockers

Blocked on S4-00 (con-glyph shapes + numbers) and S4-02 (Layer 1 base). Kept separate
from S4-04 (cast bar/prompt) per plan decision — target-frame/con and cast/prompt are
distinct enough that merging bloats the story.
