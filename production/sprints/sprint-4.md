# Sprint 4 - Gravenspire T1: EQ-Readable Presentation Slice

> **Authority:** `DECISIONS.md` D020 (second trigger — the presentation-focused
> milestone inserted before M4/M5).
> **Tier:** T1, offline single-player. No multiplayer, no LLM, no Save/Load (M4),
> no Faction Consequence (M5), no second district, no extra class.
> **Status:** Planning. Slate proposed 2026-06-07; not yet opened (`/create-stories`
> + owner assignment pending — see Next Gate).

## Goal

The Sprint 3 mechanics are done: the Tier-1 objective loop (accept -> recover +
loot -> sell -> hand-in) is assembled and runner-proven (S3-01..S3-05 merged), and
the district is navigable. But the S3-06 N=1 human-play attempt (2026-05-30) FAILED
the feel gate for presentation-readability reasons: the slice read as Unity
greybox/debug scaffolding, not a playable classic-MMO-descended gothic slice. The
player could not reliably read their own combat state, their target, their cast, or
their interaction; and the district did not yet read as a place.

D020 routed the fix to (a) an art-bible revision — **DONE** 2026-06-07, §1/§5/§7
revised under the two-register model (Principle 4) — and (b) this milestone, which
**executes** that revision. Sprint 4 builds **no new game systems**. It adds the
presentation/legibility layer the revised bible now specifies: a recessive-but-legible
HUD, the three State-Report elements (target frame, cast bar, interaction prompt), the
relative-threat con indicator, an EQ-readable play camera, and a greybox district that
reads as a gothic place.

The line this milestone must hold is D020's: **EQ readability, not EQ cosplay.** The
world stays indifferent (P1 intact); the interface gets legible. No quest markers, no
minimap, no objective arrows, no loot glow, no world-routing — fenced by the art bible
§7.11 State-Report boundary and §7.12 acceptance criteria.

## Gate State

- **Entry gate:** D020 art-bible revision complete (commits `33587ce` §1, `3ede2c0`
  §5, `67fd8e6` §7; banner resolved 2026-06-07). Sprint 4 authoring is unblocked
  because its stories now have a settled art direction to execute against.
- **Exit gate:** S4-06 human-play feel check passes against the art bible §7.12
  EQ-Legibility Acceptance Criteria (N=1 self-test per the recorded S3-06 feel-gate
  plan; the human-play verdict is the product owner's, not a runner output).
- **Sprint 3 closure:** Sprint 3 closes mechanically-successful / feel-unvalidated.
  The `sprint-status.yaml` Sprint 3->4 reconciliation is a separate explicit write
  after this plan lands (see Next Gate).

## Milestone Structure

| Milestone | Name | Purpose | First Proof |
|---|---|---|---|
| Sprint 4 | EQ-Readable Presentation Slice | Execute the D020 art-bible revision so the assembled Tier-1 loop reads as a playable classic-MMO-descended gothic slice. | A human launches the build and, within the first pull, can read their health, their selected target + its threat, their cast, and their interaction prompt — in a district that reads as a gothic place, not debug scaffolding — and answers "yes" to one-more-pull for a world reason (art bible §7.12). |

### Deferred behind Sprint 4

| Milestone | Name | Status |
|---|---|---|
| M4 | Save/Load Flow | Deferred (D016). Presupposes a playable, feel-validated loop. |
| M5 | Faction Consequence | Deferred (D016). The Tier-1 emotional payoff; only lands after the slice is playable and reads as the game. |

## Story Ledger

Dependency-ordered. Per-story scope boundaries cite the art bible section each story
executes. `/create-stories` sets precise acceptance criteria; this ledger sets the
plan-level scope and dependency shape.

**Story files created 2026-06-07** (`/create-stories`; flat convention
`production/stories/s4-NN-[slug].md`, the F1/F2/F3/F7 code-review findings folded in):

| ID | Story File | Type | Owner | Status |
|---|---|---|---|---|
| S4-00 | [`s4-00-ux-hud-thresholds-and-con-glyph`](../stories/s4-00-ux-hud-thresholds-and-con-glyph.md) | Config/Data (Design) | ux-designer | Ready |
| S4-01 | [`s4-01-play-camera-and-debug-hud-isolation`](../stories/s4-01-play-camera-and-debug-hud-isolation.md) | Integration | Codex | Ready |
| S4-02 | [`s4-02-layer-1-vitals-hud`](../stories/s4-02-layer-1-vitals-hud.md) | UI | Codex | Blocked (S4-00) |
| S4-03 | [`s4-03-target-frame-and-con-indicator`](../stories/s4-03-target-frame-and-con-indicator.md) | UI | Codex | Blocked (S4-00, S4-02) |
| S4-04 | [`s4-04-cast-bar-and-interaction-prompt`](../stories/s4-04-cast-bar-and-interaction-prompt.md) | UI | Codex | Blocked (S4-00, S4-02) |
| S4-05 | [`s4-05-first-district-atmosphere-and-legibility`](../stories/s4-05-first-district-atmosphere-and-legibility.md) | Visual/Feel | Codex | Ready |
| S4-06 | [`s4-06-eq-readable-human-play-gate`](../stories/s4-06-eq-readable-human-play-gate.md) | Integration + human-play | Brian | Blocked (S4-01..05) |

Ready to start now: **S4-00** (gates the HUD stories) and **S4-01** (independent).

| ID | Name | Purpose | Bible Authority | Scope Boundary | Depends On |
|---|---|---|---|---|---|
| `S4-00` | UX HUD threshold + con-glyph pass *(ux-designer; Design type)* | Resolve the art bible §7.10 **deferred** HUD numbers the revision intentionally left to ux: minimum bar height, panel-fill opacity floor, cast-bar lower-center placement clearing the 40-60% band, and 5-state con-glyph shape discriminability. Output: concrete values the HUD stories build to. | §7.10 (the explicit deferral home); §7.1.1 | Design/spec only — NO implementation. Sets numbers + validates con-glyph shapes (color-disabled discriminability per §4.6). This is the ux-designer pass that owns the deferred thresholds. | — |
| `S4-01` | Play camera + debug-HUD isolation | An EQ-readable third-person play camera (player steers; the camera does NOT pull toward points of interest — that is World Performance, §7) and removal of the legacy M2 combat-debug HUD from the objective-play view (formalizes the kept S3-06 presentation-readiness bugfix). | §7 (recessive interface); Principle 1 (the world does not attend to the player — the player steers the camera) | Camera framing + debug-overlay isolation only. No new locomotion system (the S3-01 harness mover is reused; a structural mover rebuild is a red flag). Kills the "debug scaffolding" read at the camera/overlay level. | — |
| `S4-02` | Layer 1 vitals HUD | Implement the recessive-but-legible player vitals (health / mana / endurance / hate) in the Iron Seam architectural vocabulary at the S4-00 legibility floor. | §7.1 (recessive but legible); §7.1.1; §4.4 (UI palette) | Vitals only — target frame is S4-03, cast bar/prompt are S4-04. No glow / gradient / rounded corners / true black / true white (§7.11). Snap-to-value bars except where §7.9 permits linear. | `S4-00` |
| `S4-03` | Target frame + relative-threat (con) indicator | Selection-gated target frame: target name, target health bar, faction frame treatment (§7.8), and the 5-state shape-primary non-color-only con glyph. | §7.1.1 (Target Frame; Relative-Threat Indicator) | State Report ONLY — appears on the *selected* target, never floating over un-targeted entities. Con glyph is a recessive line-glyph, NOT a loud color badge (§7.11 fence). Palette colors only confirm shape (§4.3). | `S4-00`, `S4-02` |
| `S4-04` | Cast bar + interaction prompt | The two remaining State-Report elements: cast bar (linear readout of an ongoing cast, lower-center, clears the forbidden band) and the proximity/focus-gated interaction prompt naming the single focused entity ("Speak — Caretaker Morrvik"). Directly fixes the S3-06 "couldn't read interaction" failure. | §7.1.1 (Cast Bar; Interaction Prompt) | Quiet labels, no routing. Cast bar disappears on completion/interrupt (no flourish). Interaction prompt is screen-space, single-target, range + facing gated; input-display wording is a ux detail, not floating world-space text. | `S4-00`, `S4-02` |
| `S4-05` | First District atmosphere + legibility pass *(bounded)* | Make the navigable greybox district read as a gothic *place* through practical-source lighting, massing readability, and placeholder material language. | §1 (weight-and-age); §2 (mood/atmosphere, practical light only); §6 (environment) | **BOUNDED (USER DECISION):** greybox practical lighting + massing/sightline readability + placeholder material language ONLY. NOT a produced art palette, NOT final textures (D016 greybox discipline holds). Holds the no-routing fence: no glowing doors, objective markers, atmosphere-as-warning, or guidance lighting (§7.11; Pillar 2). | `S3-05` (done) |
| `S4-06` | EQ-readable human-play gate | Re-run the human-play feel check against the art bible §7.12 acceptance criteria. Composes S4-01..S4-05. The milestone exit gate. | §7.12 (the six EQ-Legibility Acceptance Criteria) | Human-play verdict (N=1 self-test; the product owner's call per the recorded S3-06 feel-gate plan — Codex builds any runner/evidence scaffold, the feel verdict is human). The protocol MUST separate "does it read/play" from "is it final-art pretty" (greybox still acceptable; R-P2-FEEL-MISATTRIBUTION). | `S4-01`, `S4-02`, `S4-03`, `S4-04`, `S4-05` |

## Operating Model Calibration

Sprint 4 inherits the Sprint 3 feedback rule and the D020 two-register model as its
governing calibration:

- **Two registers (art bible Principle 4).** The **world register** obeys P1
  absolutely — it does not perform for or route the player. The **interface register**
  reports the player's own state clearly. Every Sprint 4 element is classified into one
  register and obeys that register's rules. A HUD element that reports player state is
  State Report (allowed); a HUD element that directs the player toward unchosen content
  is World Performance (forbidden).
- **State Report vs. World Performance test** (art bible §7.1.1 / §7.11). For any
  proposed element: *Who initiated this relationship?* (the player) and *What must the
  world do for this signal to appear?* (nothing — the player's own action triggers it).
  If either answer points at the world, the element is World Performance and is rejected.
- **Recessive but legible** (§7.1). Quiet is the aesthetic; unreadable is a bug. The HUD
  stays in the city's material vocabulary and out of the player's aesthetic attention —
  but it must be readable under combat stress. The S4-00 legibility floor is the
  enforceable form of this.

## Tier 2+ Cuts Preserved

The confirmed Tier-2+ cuts stand. Sprint 4 adds none of: multiplayer / netcode, LLM
dialogue, Save/Load, faction consequence, tuned economy, second district, extra class.
It is presentation only, still offline single-player. D017 (server-authoritative design
discipline) continues to apply to any new presentation-adjacent system that touches a
mandatory seam.

## Known Findings

- **Art-bible revision is the entry artifact.** §1/§5/§7 are revised and consistent
  under the two-register model (commits `33587ce`/`3ede2c0`/`67fd8e6`). Sprint 4 stories
  execute it; they do not re-litigate it.
- **Deferred HUD numbers are real, not a gap.** The bible intentionally deferred exact
  px/opacity to ux (§7.10). S4-00 is that pass; S4-02/03/04 depend on it. This is by
  design, not an omission.
- **The kept S3-06 M2-HUD bugfix** lives in the Codex worktree
  (`M2SingleTrashMedLoopController.cs`), recorded in the S3-06 evidence (commit
  `900f296`). S4-01 formalizes the debug-HUD isolation on main.
- **S3-06 branch is unmerged.** The S3-06 mechanical pass + feel-fail evidence sits on
  `codex/s3-06-playable-end-to-end-and-human-play` (not merged; feel-failed). Whether it
  merges or stands as a recorded artifact is a Sprint 3 closure question, tracked.

## Risks

| Risk | Probability | Impact | Owner | Mitigation |
|---|---|---|---|---|
| **R-COSPLAY-DRIFT — interface crosses into World Performance.** The entire D020 pivot is "readability, not cosplay." Under presentation pressure, a HUD element drifts into routing: a minimap "to help the tester", an objective arrow, loot glow, the con indicator rendered as a loud color badge, an across-room interaction prompt, a glowing objective door in S4-05. | High | High | Creative Director + Producer | The art bible §7.11 State-Report boundary is the enforceable fence; §7.12 criterion 6 ("no routing") is the exit test. Reviewers reject any HUD/scene element that tells the player where to go, what to value, or what to feel. Per-story creative-director gate at `/create-stories`. This is the central risk of the milestone. |
| **Scene fragility on `_DevEntry.unity`.** S4-01 (camera/overlay) and S4-05 (atmosphere/lighting) both touch the shared scene; concurrent scene edits across implementers (Brian / Codex) risk merge corruption. | Medium | High | Producer + each scene-touching implementer | Scene Discipline (`.claude/rules/game-dev-governance.md`): one scene edit per PR; save-then-diff before staging; never hand-edit YAML; Unity Smart Merge. Sequence the scene-touching stories (S4-01, S4-05) rather than running them concurrently. Adapter-only / additive scene edits where possible — never chain a legacy builder over the authored scene (the 2026-05-30 builder-chaining lesson; recurred across S3-02/03/04/05). |
| **Deferred-numbers dependency slip.** S4-02/03/04 are blocked on the S4-00 ux pass. If S4-00 slips or returns unvalidated numbers, the HUD stories cannot start with real values and may guess. | Medium | High | Producer + ux-designer | S4-00 is the first story and gates the HUD stories. It must deliver concrete, prototype-validated numbers (not "TBD"). If S4-00 cannot validate a number, it records the open question explicitly rather than letting an HUD story invent one — the same evidence-honesty discipline the bible used to defer them. |
| **Missing-art misread as feel-fail (R-P2-FEEL-MISATTRIBUTION).** S4-06's human-play check could fail because the district lacks produced art, when the real question is "does it read and play." This would repeat the S3-06 confusion one layer up — and greybox is still acceptable per D016. | Medium | Medium | Producer + Creative Director | S4-06's protocol explicitly separates "does it read/play as the game" (the real question — camera, HUD, legibility, place-read) from "is it final-art pretty" (out of scope). A greybox-but-readable slice that pulls the player back is a PASS; a polished-but-unreadable slice is not. |
| **Presentation milestone scope inflation.** "Make it read as the game" is open-ended; S4-05 (atmosphere) and the HUD stories can each expand indefinitely toward "more polish". | Medium | High | Producer | S4-05 is explicitly BOUNDED (greybox lighting + massing + placeholder material only; no produced palette). HUD stories build to the S4-00 floor, not to "as nice as possible". `/scope-check` before each story closes. Any creep recorded as a `[SCOPE]` lesson. The exit gate is §7.12 readability, not visual fidelity. |
| **Tier-1 / no-new-systems discipline slip.** A presentation milestone can tempt a "small" new system (a settings menu, a minimap toggle, a save of HUD layout). Any new game system, Save/Load hook, or faction wiring is Tier-creep / M4-M5 leakage. | Medium | High | Producer (escalate to Technical Director / Creative Director) | No-new-systems is the milestone constraint. Sprint 4 adds presentation, not systems. T1 negative-scope scan per story. A minimap is doubly forbidden — both Tier-creep AND a §7.11 World-Performance violation. |
| **Owner assignment outstanding.** Sprint 4 story files will carry empty `owner` fields until `/create-stories`. | High (until resolved) | Medium | Producer | Assign exactly one accountable owner per story before the slate commits. Implementer mix: Brian (lead), Codex (own worktree, D006 — NOT art-bible/DECISIONS/sprint-plan), Qwen3-Coder (scoped mechanical edits only, D015 — not a `/dev-story` implementer). S4-00 (ux design) and S4-06 (human-play verdict) are design-aware / product-owner work, not Codex/Qwen. |

## QA Plan Hooks

- **Story-type split.** S4-00 is **Design** (spec/validation, no code). S4-01..S4-05 are
  **Visual/Feel + UI** (presentation work — evidence is screenshots + manual walkthrough
  + the §7.12 read tests, ADVISORY gates per the coding-standards evidence table, plus
  any Unity batchmode readability runner where automatable). S4-06 is **Integration +
  human-play** (the feel gate).
- **The exit criteria are pre-written.** Art bible §7.12 is the acceptance-criteria
  source for S4-06: combat-state read, target identification, interaction confirmation,
  cast-state read, HUD coherence, no-routing. `/create-stories` maps each S4 story to
  the §7.12 criteria it serves.
- **Greybox-honest human-play protocol.** S4-06 inherits the S2-M3-04 / S3-06 human-play
  shape and the R-P2-FEEL-MISATTRIBUTION rule: separate loop/legibility feel from art
  fidelity. N=1 self-test acknowledged with its selection bias.
- **Con-glyph accessibility.** S4-00 must validate the 5 con glyphs are discriminable by
  shape alone (color disabled) per §4.6 — a colorblind player loses nothing.
- **`/qa-plan sprint`** is run after the slate opens, before implementation, as in Sprint 3.

## Capacity Assumptions

- **1 UX design pass (S4-00) + 6 stories.** Honestly larger than Sprint 3's assembly
  work — "make it read as the game" is real presentation work, not wiring. Not trimmed to
  look smaller.
- **Dependency shape:** S4-00 gates S4-02/03/04 (HUD numbers). S4-01 and S4-05 (scene)
  are independent of the HUD chain but must be sequenced against each other for scene
  safety. S4-06 composes everything.
- **Implementer mix:** Brian (lead, design-aware stories), Codex (own worktree, scoped
  implementation), ux-designer agent (S4-00). Qwen3-Coder only for scoped mechanical
  edits if any. Owner per story assigned at `/create-stories`.
- **Scope levers (if the slate proves too large):** S4-05 is the most bounded-already and
  the most deferrable; the HUD stories build to a floor, not to maximum polish.

## Tightening Levers Considered (and the decisions taken)

- **Merge S4-03 + S4-04?** REJECTED (USER DECISION) — target-frame/con and
  cast-bar/interaction-prompt are distinct enough that merging invites a bloated HUD
  story. Kept separate.
- **Bound S4-05?** APPLIED (USER DECISION) — S4-05 is scoped to greybox practical
  lighting + massing readability + placeholder material language only; no produced
  palette, no final textures.

## Next Gate

1. **`/create-stories`** for the Sprint 4 slate (S4-00..S4-06) — sets precise acceptance
   criteria (mapping each story to its art bible §-authority and the §7.12 exit criteria),
   assigns one owner per story, and follows the project flat-path convention
   (`production/stories/s4-NN-[slug].md`).
2. **Sprint 3 -> Sprint 4 `sprint-status.yaml` reconciliation** — a separate explicit
   write (Sprint 3 closes feel-unvalidated; Sprint 4 opens). Kept distinct from this
   planning artifact for clean reviewability.
3. **`/qa-plan sprint`** once the slate is opened, before implementation.

## Definition Of Done For Sprint 4 Planning

- [x] Sprint 4 goal, authority (D020), and exit gate (§7.12) stated.
- [x] Story Ledger (S4-00..S4-06) dependency-ordered with per-story art-bible authority
      and scope boundary.
- [x] S4-05 bounded; S4-03/S4-04 kept separate (user decisions recorded).
- [x] Risks named with owners and mitigations; R-COSPLAY-DRIFT flagged as central.
- [x] QA plan hooks tied to the pre-written §7.12 acceptance criteria.
- [x] `/create-stories` run; owners assigned; story files written. *(Done 2026-06-07.)*
- [x] `sprint-status.yaml` Sprint 3->4 reconciliation applied. *(Done 2026-06-07; S4-01 assigned.)*

---

*Sources: `DECISIONS.md` D016/D017/D020; `design/art/art-bible.md` §1/§5/§7 (revised
2026-06-07, commits `33587ce`/`3ede2c0`/`67fd8e6`); `production/sprints/sprint-3.md`
(template + carried risks); `production/sprint-status.yaml` (Sprint 3 state);
`tests/evidence/S3-06/human-play-20260530.md` (the feel-fail evidence, Codex worktree).
Plan proposed 2026-06-07.*
