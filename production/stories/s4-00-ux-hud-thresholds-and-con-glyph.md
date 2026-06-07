# S4-00: UX HUD Threshold + Con-Glyph Pass

> **Sprint**: Sprint 4 — EQ-Readable Presentation Slice
> **Sprint Plan**: `production/sprints/sprint-4.md` (Story Ledger, S4-00)
> **Status**: Complete
> **Layer**: Presentation
> **Type**: Config/Data (Design — spec only, no code)
> **Estimate**: 0.5 day
> **Manifest Version**: Unavailable (control-manifest absent project-wide; documented fallback applies)
> **Generated**: 2026-06-07
> **Owner**: ux-designer

## Context

**Authority**: `DECISIONS.md` D020 (EQ-readability pivot, Locked); revised art bible
`design/art/art-bible.md` §7.10 (the explicit deferred-numbers home) and §7.1.1.

**Requirement Summary**: The D020 art-bible revision deliberately deferred the exact
HUD numbers to a ux-designer validation pass (§7.10) rather than inventing them in the
bible. This story IS that pass. It resolves: minimum bar height, panel-fill opacity
floor, cast-bar lower-center placement (clearing the 40-60% forbidden band), and the
5-state con-glyph shape discriminability. Output is a concrete spec the HUD
implementation stories (S4-02/03/04) build to. This story writes NO implementation
code — it produces validated numbers and a con-glyph shape set.

**Governing decisions**:

| D-entry | Status | Usage |
|---|---|---|
| D020 | Locked | EQ-readability pivot; this resolves its §7.10 deferred numbers |
| D016 | Locked | Greybox presentation minimum; HUD legibility floor, not maximum polish |

**Art-bible authority**: §7.10 (Testable Thresholds — the deferral home); §7.1.1
(the three State-Report elements whose numbers this sets); §4.6 (colorblind
accessibility — shape-primary, non-color-only); §4.4 (UI palette).

**Engine**: Unity 6.3 LTS. **Engine Risk**: LOW (spec/validation, no runtime API surface).

## Acceptance Criteria

- [ ] **S4-00-01**: A **minimum bar height** is set for Layer 1 vitals bars (health/mana/endurance/hate), with a stated rationale that it passes a combat-stress readability check. The prior 3px value is explicitly recorded as the replaced baseline (no longer accepted as proven readable after S3-06, per §7.10). The number replaces the §7.10 "deferred" marker.
- [ ] **S4-00-02**: A **panel-fill opacity floor** is set for Layer 1 panels, with rationale that bar content meets ≥3:1 contrast against the panel fill under the URP post-process stack. The prior 45% value is recorded as the replaced baseline.
- [ ] **S4-00-03**: The **cast-bar lower-center placement** is validated to sit clear of the 40-60% viewport-height forbidden band (§7.11), expressed as a concrete screen-space position/height a HUD story can implement.
- [ ] **S4-00-04**: The **5-state con glyph** set (Trivial / Below / Even / Above / Dangerous) is specified as compressed geometric line-glyphs (§7.8 instrument-plate register) and validated **discriminable by shape alone with color disabled** (§4.6 colorblind simulation). Color is confirm-only, drawn from the existing world palette per §7.1.1 (no RPG danger-red / reward-gold).
- [ ] **S4-00-05**: Every number is **prototype-validated or recorded as an explicit open question** — no value is handed to an implementation story as "TBD". (Evidence-honesty: a deferred number that gets guessed downstream is the failure this story exists to prevent.)
- [ ] **S4-00-06**: The output spec stays within the "recessive but legible" doctrine (§7.1): it sets a legibility *floor*, not a maximum-visibility target. Quiet aesthetic, readable under stress.

## Implementation Notes

- **No code.** Output is a spec document (the evidence file) the HUD stories consume.
- The con-glyph validation should use a colorblind simulation (deuteranopia/protanopia/tritanopia per §4.6) with color channels disabled to prove shape-primary discriminability.
- Where a number genuinely cannot be validated without an in-engine prototype, record it as an explicit open question with the validation method, not a guessed value.

## Out of Scope

- Any HUD implementation (S4-02 vitals, S4-03 target frame, S4-04 cast bar/prompt).
- Camera or scene work (S4-01, S4-05).
- Final-art palette or texture decisions (D016 greybox; §4 palette already set).

## QA Test Cases

**Manual check (S4-00-04 con-glyph discriminability)**
- Setup: render the 5 con glyphs at HUD scale with color disabled.
- Verify: a viewer can name all 5 states by shape alone.
- Pass: 5/5 distinguishable without color; no two glyphs ambiguous.

**Manual check (S4-00-01/02/03 numbers)**
- Setup: the output spec is read by an implementer.
- Verify: every number is concrete (or a labeled open question with a method), and each cites the §7.10 deferral it resolves.
- Pass: no "TBD" reaches S4-02/03/04; replaced baselines (3px, 45%) recorded.

## Test Evidence

**Required evidence**: `production/qa/evidence/s4-00-hud-thresholds-evidence.md`

The spec/validation document the HUD stories build to: the resolved numbers, the
con-glyph shape set + colorblind-discriminability proof, and any explicit open
questions with their validation methods.

**Evidence status**: Delivered — `production/qa/evidence/s4-00-hud-thresholds-evidence.md` (committed `e244460`). All four thresholds resolved as PROPOSED floors with named in-engine/render validation methods (OQ-1..7) routed to the consuming stories.

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| None | First story; gates the HUD implementation stories | — |

## Blockers

None. D020 + the revised art bible are the authority; §7.10 is the explicit home for
these numbers. This story unblocks S4-02/S4-03/S4-04.

## Completion Notes

**Completed**: 2026-06-07
**Verdict**: COMPLETE WITH NOTES
**Criteria**: 6/6 passing (S4-00-04 PASS with a forward-assigned empirical test, below)
**Deferred/Untested Criteria**: None at the spec level. In-engine + render-test validations are routed forward as open questions, not deferred ACs.
**Test Evidence**: `production/qa/evidence/s4-00-hud-thresholds-evidence.md` (committed `e244460`) — vitals bar-height floor 6px@1080p (3px retired), panel-fill opacity floor 65% (45% retired), cast-bar placement (top edge 70% viewport, clears the 40-60% band), and the 5-state shape-primary con-glyph set with world-palette confirm-colors and colorblind reasoning.
**GDD/ADR Deviations**: None — executes art-bible §7.10/§7.1.1/§4.3/§4.4/§4.6/§7.8 faithfully.
**Scope Notes**: Design/spec only, no code (Config/Data type). The deliverable is the threshold/glyph spec plus explicit open-question routing (OQ-1..7).
**Forward-assigned validations** (per evidence-honesty design + product-owner decision):
- OQ-1..5 (in-engine bar-height read, panel-opacity ≥3:1 measurement, cast-bar screenshot) → S4-02/S4-04 own them; those stories may not mark the corresponding claims evidence-linked until run.
- OQ-6/7 (con-glyph 5/5 shape-discriminability render + human naming test) → **assigned forward to S4-03** by explicit product-owner decision (non-strict closure). S4-03 must run it before its con-glyph claim is evidence-linked.
**Review Gates**: lean mode (no Task gates). Advisory close-out.
**Forced Completion**: No.
