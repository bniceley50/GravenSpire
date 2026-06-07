# S4-01: Play Camera + Debug-HUD Isolation

> **Sprint**: Sprint 4 — EQ-Readable Presentation Slice
> **Sprint Plan**: `production/sprints/sprint-4.md` (Story Ledger, S4-01)
> **Status**: Ready
> **Layer**: Presentation
> **Type**: Integration
> **Estimate**: 1.0 day
> **Manifest Version**: Unavailable (control-manifest absent project-wide; documented fallback applies)
> **Generated**: 2026-06-07
> **Owner**: Codex

## Context

**Authority**: `DECISIONS.md` D020; revised art bible `design/art/art-bible.md` §7
(recessive interface) and §1 Principle 1 (*Stillness Is The Signal* — the world does
not attend to the player).

**Requirement Summary**: A primary cause of the S3-06 feel-fail was that the slice
read as Unity greybox/debug scaffolding — and a concrete defect contributed: the
legacy M2 combat-debug HUD bled over the S3 objective-loop view. This story (a) gives
the slice an EQ-readable third-person **play camera** that the player steers (not a
camera that pulls toward points of interest — that would be World Performance), and
(b) formally isolates the legacy M2 combat-debug HUD from objective-play, formalizing
the kept S3-06 presentation-readiness bugfix (`M2SingleTrashMedLoopController.cs`,
currently on the unmerged S3-06 branch) onto main.

**Governing decisions**:

| D-entry | Status | Usage |
|---|---|---|
| D020 | Locked | EQ-readability pivot; the camera/overlay legibility this executes |
| D017 | Locked | Server-auth design discipline applies to any presentation-adjacent system touching a mandatory seam (camera is CLIENT-LOCAL; annotate) |
| D016 | Locked | Greybox; no produced art |

**Art-bible authority**: §7 (recessive interface, not invisible); §1 Principle 1
(player steers the camera; the camera does not perform for the player).

**Surfaces reused** (do not re-author): the S3-01 harness mover / follow-camera
(`M2SingleTrashMedLoopController.cs:564-583`, WASD + follow-camera). This story tunes
camera framing and isolates the debug HUD; it does not rebuild locomotion.

**Engine**: Unity 6.3 LTS. **Engine Risk**: MEDIUM (camera + URP overlay/camera-stack
isolation; verify any post-6.0 camera API against `docs/engine-reference/unity/`).

## Acceptance Criteria

- [ ] **S4-01-01**: An EQ-readable third-person **play camera** frames the Cleric for objective-loop play (sightlines to interactables, readable traversal). The **player steers** the camera; it does **not** auto-pull toward NPCs, the relic, the vendor, or any point of interest (camera-pull-toward-POI is World Performance, forbidden per §7 / §7.11).
- [ ] **S4-01-02**: The legacy M2 **combat-debug HUD is removed from the objective-play view**. During non-combat objective play, the S3 interaction prompt/feedback is visible and the M2 combat HUD is not. (Formalizes the kept S3-06 bugfix from `M2SingleTrashMedLoopController.cs` onto main.)
- [ ] **S4-01-03**: **No new locomotion system.** The S3-01 harness mover is reused; a structural mover rebuild is a red flag (escalate). Camera framing tuning (offset, FOV) within reuse scope is allowed; movement behavior is not re-authored.
- [ ] **S4-01-04**: The scene change to `_DevEntry.unity` is **adapter/additive only** — no legacy builder is chained over the authored district scene (2026-05-30 builder-chaining lesson). The post-edit scene diff contains only this story's camera/overlay delta; any Unity ProjectSettings drift is restored (2026-05-26 lesson).
- [ ] **S4-01-05**: The camera/overlay change is annotated `CLIENT-LOCAL` per D017 (camera and HUD-overlay are presentation, genuinely single-player-local — not a server-auth mandatory seam). A one-line ownership annotation, not a structural seam (D017 mandates seams only for state-mutating systems).

## Implementation Notes

- The debug-HUD isolation already exists as a kept bugfix on the unmerged S3-06 branch (`M2SingleTrashMedLoopController.cs` HUD-hide hook). Bring that approach onto main cleanly, scoped to this story, rather than re-inventing it.
- Verify any Unity 6.3 camera-stack / URP overlay API against `docs/engine-reference/unity/` before use (post-6.0 APIs are UNVERIFIED).
- Scene discipline: save-then-diff before staging; one scene edit per PR; never hand-edit YAML; Unity Smart Merge for conflicts. Sequence against S4-05 (also scene-touching) — do not run concurrently.

## Out of Scope

- HUD content — vitals (S4-02), target frame (S4-03), cast bar/prompt (S4-04).
- Locomotion rebuild (reuse only).
- District atmosphere/lighting (S4-05).
- Produced art.

## QA Test Cases

**Manual check (S4-01-01 camera)**
- Setup: enter Play Mode in `_DevEntry.unity`, walk the district.
- Verify: the camera frames the play space readably and is steered by the player; it never swings toward an NPC/relic/vendor on its own.
- Pass: readable third-person framing; zero auto-pull-toward-POI behavior.

**Manual check (S4-01-02 debug-HUD isolation)**
- Setup: enter objective-play (non-combat) in the district.
- Verify: the S3 interaction prompt/feedback shows; the M2 combat-debug HUD does not bleed over the view.
- Pass: no legacy combat HUD visible during objective play.

**Integration check (S4-01-04 scene delta)**
- Setup: post-implementation, inspect `git diff` of `_DevEntry.unity`.
- Verify: only the camera/overlay delta; no legacy-builder rebuild artifacts; no ProjectSettings drift shipped.
- Pass: adapter-only scene diff; diff-check clean.

## Test Evidence

**Required evidence**: `production/qa/evidence/s4-01-play-camera-evidence.md`
(screenshots of the framed play camera + debug-HUD-isolated objective view; scene-diff
confirmation that the change is adapter-only).

**Evidence status**: Not started

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| None | Independent of the HUD chain; can start immediately | — |

## Blockers

None. Independent of S4-00 (the HUD-numbers pass). Sequence against S4-05 for scene
safety (both touch `_DevEntry.unity`).
