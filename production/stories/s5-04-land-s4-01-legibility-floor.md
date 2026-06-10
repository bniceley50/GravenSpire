# S5-04: Land S4-01 Legibility Floor (Camera + Debug-HUD Isolation)

> **Sprint**: Sprint 5 — First District — Designed & Produced (First-Pass)
> **Sprint Plan**: `production/sprints/sprint-5.md` (Story Ledger, S5-04)
> **Status**: Complete
> **Layer**: Presentation
> **Type**: Integration
> **Estimate**: 0.5 day
> **Manifest Version**: Unavailable (control-manifest absent project-wide; documented fallback applies)
> **Generated**: 2026-06-07
> **Owner**: Codex

## Context

**Authority**: `DECISIONS.md` D021 (legibility rides the demo bar), D020 (EQ-readability);
art bible §7 (recessive interface); the parked S4-01 implementation on
`codex/s4-01-play-camera-debug-hud-isolation`.

**Requirement Summary**: Per D021, the two feel-fails had a *legibility* component that
produced art will not fix — a produced district viewed through a debug overlay still
fails. This story carries the already-implemented S4-01 work (player-steered camera +
legacy M2 debug-HUD isolation), re-validates it on the current base, and lands it as the
**legibility floor** for the Sprint 5 demo. It does not rebuild anything — S4-01 is
implemented; this re-validates and lands it.

**Governing decisions**:

| D-entry | Status | Usage |
|---|---|---|
| D021 | Locked | Legibility rides the demo bar; S4-01 is the carried floor |
| D020 | Locked | EQ-readability — the camera/overlay legibility this lands |
| D017 | Locked | Camera/HUD are CLIENT-LOCAL presentation (annotated, not a seam) |

**Surfaces consumed**: the parked S4-01 work — `Assets/Scripts/M2SingleTrashMedLoopController.cs`
(camera framing + Q/E orbit + debug-HUD-hide) and its evidence
`production/qa/evidence/s4-01-play-camera-evidence.md` (incl. the recorded failed
demo-readiness verdict and the AC-04 no-scene-delta acceptance).

**Engine**: Unity 6.3 LTS, URP. **Engine Risk**: MEDIUM (camera/overlay; verify any
post-6.0 camera API against `docs/engine-reference/unity/`).

## Acceptance Criteria

- [ ] **S5-04-01**: The parked S4-01 implementation is **rebased onto current `origin/main`** and **re-validated**: `dotnet test` green (≈189/189), `dotnet format --verify-no-changes` green, the three M2 preservation smokes green (builder skipped/not invoked) — as the S4-01 evidence already recorded; re-confirmed on the new base.
- [ ] **S5-04-02**: The S4-01 acceptance behaviors hold: **player-steered camera, zero auto-pull-toward-POI** (S4-01-01); **S3 interaction prompt visible / M2 debug HUD hidden** in objective play (S4-01-02); **no locomotion rebuild** (S4-01-03); the **AC-04 no-scene-delta acceptance** recorded (camera lives in the reused harness controller, no `_DevEntry.unity` edit) — the stronger form of the adapter-only requirement; **CLIENT-LOCAL** annotation present (S4-01-05).
- [ ] **S5-04-03**: **Manual Play Mode behavior confirmation** — camera frames the play space readably and is player-steered; the interaction prompt shows while the M2 debug HUD does not bleed over. (This confirms the legibility-floor *behavior*; the demo-readiness *verdict* is S5-05's job, judged against the produced area — not this story's.)
- [ ] **S5-04-04**: **Optional legibility levers decided here** (not separate stories): a minimum vitals readout and/or the focused interaction prompt ("Speak — Caretaker Morrvik"). Include only if cheap and demo-relevant; otherwise defer with a recorded note. Full vitals/target/cast HUD (S4-02/03/04) stays deferred.
- [ ] **S5-04-05**: **Scene discipline**: the S4-01 change is code-only (no scene delta), which keeps this clean; if any scene edit becomes necessary, **sequence vs S5-03** (never concurrent on `_DevEntry.unity`), one scene edit per PR, no builder chaining.

## Implementation Notes

- This is a carry-and-land, not a rebuild. Bring the parked branch current and confirm it still behaves; do not re-author the camera or movement.
- The S4-01 evidence already records the failed *demo-readiness* verdict — that verdict belongs to the world/art layer (now Sprint 5), not to the camera/HUD behavior, which is sound. Keep that distinction explicit in the evidence.
- Code-review checkpoints carried from S4-01: confirm the Q/E camera-orbit is camera *presentation*, not a new locomotion/input system; no POI-pull; CLIENT-LOCAL annotation.

## Out of Scope

- Full HUD — vitals (S4-02), target frame (S4-03), cast bar (S4-04) stay deferred.
- Produced art (S5-03). Any camera/movement rebuild.

## QA Test Cases

**Manual check (S5-04-02/03 camera + HUD isolation)**
- Setup: rebased branch, Play Mode in `_DevEntry.unity`, walk the area.
- Verify: camera readable + player-steered, zero auto-pull-toward-POI; interaction prompt visible, M2 debug HUD hidden.
- Pass: legibility-floor behavior confirmed; no debug overlay over objective play.

**Integration check (S5-04-01 re-validation)**
- Setup: post-rebase, run the gates.
- Verify: dotnet test green; format clean; 3× M2 preservation green (builder skipped).
- Pass: all gates green on the current base; diff is the S4-01 code delta only.

## Test Evidence

**Required evidence**: `production/qa/evidence/s5-04-legibility-floor-evidence.md`
(carries forward the S4-01 evidence + the post-rebase re-validation + the Play Mode
behavior confirmation + the optional-lever decision).

**Evidence status**: Complete — `production/qa/evidence/s5-04-legibility-floor-evidence.md`
(PASS WITH NOTES) + `tests/evidence/S5-04/` (3× post-fix M2 preservation PASS + the
objective-freewalk fix artifact). Merged via PR #10 (`ab031b4`).

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| None | The S4-01 work is implemented; this re-validates and lands it. Sequence vs S5-03 for scene safety. | — |

## Blockers

None. Independent of the design/art chain (the S4-01 code exists). Sequence against
S5-03 if any scene edit arises.

## Completion Notes

**Completed**: 2026-06-09 (merged 2026-06-10 UTC via PR #10, merge `ab031b4`)
**Verdict**: COMPLETE WITH NOTES
**Criteria**: 5/5 — rebase + re-validation (S5-04-01), S4-01 behaviors held incl. the
AC-04 no-scene-delta acceptance (S5-04-02), manual Play Mode confirmation (S5-04-03),
optional-lever decision (S5-04-04: prompt scaled; vitals/full prompt deferred), scene
discipline (S5-04-05: code-only, no scene delta).
**The story grew a real fix mid-flight**: the first manual check FAILED (1 PASS / 2 FAIL
/ 1 indeterminate), which surfaced three diagnoses — (F-A) the legacy M2 controller
stomped Play Mode presentation every frame (near-black fog, ambient override, floor
repaint over the produced cobble — meaning every prior Play Mode session, including both
feel-fails, was judged through the stomp); (F-B) M2 proximity body-pull yanked the
player next to Morrvik; (F-C) the S3 prompt was a tiny unscaled label. Fixed via the
`ShouldSuppressLegacyM2DuringObjectiveFreeWalk` guard (explicit batchmode/_smokeRunning
scenario discrimination — suppression is human-free-walk only; combat re-enables
everything) + prompt scaling. Re-checks: manual PASS WITH NOTES; post-fix M2
preservation 3/3 PASS; dotnet 189/189; format x2; no scene/settings/package drift.
**Notes (routed forward, not S5-04 scope)**: player avatar/orientation read → the
character/NPC body story (spec drafted, uncommitted); ability surface (harness exposes
auto-attack + Smite only; combat-core names Lesser Heal / Bash / Defensive Prayer) →
a combat ability-surface story.
**Review Gates**: Codex four-question PR body; main-lane review APPROVE (guard condition
+ surface verified post-merge); product-owner merge.
**Forced Completion**: No.
