# S2-M2-02 Human Play Evidence

**Date:** 2026-05-12
**Story:** `production/stories/s2-m2-02-single-trash-pull-med-loop.md`
**Player:** Brian
**Session:** ~45 minutes, iterative play + fix cycles
**Verification artifact:** `tests/evidence/S2-M2-02/verification.md`

## The Question

> Did you want one more pull?

**Qualified no.** Mechanically the two-pull loop now works: Pull -> Target ->
Attack -> Smite -> kill -> Sit -> mana recovery -> Stand -> trash reset ->
Pull 2. The presentation surface is too thin to honestly evaluate "want one
more pull?" as a feel question. The play experience reads as "did the test
harness work?" rather than "did the game pull me back in?" The question
requires enough visible game-state for the player to separate "the systems
function" from "the world wants me to stay."

## Worst-Thing Finding

**Visual presentation surface fails the "looks like a game" bar.** Blockout
capsules, debug-overlay HUD, and flat-color floor are sufficient to validate
that the mechanical loop runs, but insufficient to validate that the loop feels
like Gravenspire. The First District scope filter implicitly assumed
blockout-quality assets would be enough to evaluate feel; this session
disproves that assumption.

Secondary in-loop findings, all fixed during the session rather than deferred:

- HUD overlay too small to read in Game view; fixed by enlarging and
  structuring it.
- Sit/Stand allowed during active combat; fixed with phase-aware buttons.
- Smite cooldown not legible; fixed with cooldown timer in HUD.
- Player input and clarity were too thin; fixed with clickable buttons and
  step-ordered HUD.

## Worst-Thing Disposition

**Carried forward as sprint-level finding**, not an in-story fix. Visual polish
inside M2-02 would explode scope: player model, enemy model, environment, HUD
that does not read as debug overlay, hit feedback, and audio. That would
violate the play-immediately principle of small cycles. The mechanical loop is
the M2-02 acceptance bar; presentation is M3+ scope.

Specific carry-forward candidate for `production/sprint-status.yaml` during
`/story-done`, not this evidence batch:

> `m2_presentation_threshold_gap`: "Sprint 2 M2-02 human-play validation
> revealed that blockout-quality presentation (capsule actors, flat floor,
> debug HUD) is insufficient to validate gameplay feel via the 'did you want
> one more pull?' bar. Future M2/M3 stories need minimum-visible-art baseline
> OR explicit acknowledgment that human-play AC for blockout-stage stories
> tests mechanical coherence only, not feel. Affects M2-03, M2-04 readiness
> scoping and M3 design."

## Framework Metrics

- **Batch shape:** Lighter loop with multiple in-loop fixes; framework
  operating as designed.
- **Time-to-implement:** ~3 hours from `/dev-story` start to
  mechanical-functional state, versus M2-01's ~3-4 days with compatibility-fix
  cascade; framework velocity dividend observed.
- **Finding character:** Mixed. Procedural findings were fixed in-loop; the
  structural presentation-threshold finding surfaced and was carried forward.
- **Worst-thing decision:** Carry-forward. Presentation polish is outside
  M2-02 scope per the First District filter; in-loop polish fixes shipped with
  the implementation.

## Routing

- Verification artifact: `tests/evidence/S2-M2-02/verification.md`.
- Human-play AC: `S2-M2-02-06` is satisfied with a qualified-no answer and the
  structural worst-thing carried forward.
- Next gate: `/code-review` on the M2-02 implementation, then `/story-done`
  with the carry-forward documented.
