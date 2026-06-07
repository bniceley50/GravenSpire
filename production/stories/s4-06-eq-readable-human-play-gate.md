# S4-06: EQ-Readable Human-Play Gate

> **Sprint**: Sprint 4 — EQ-Readable Presentation Slice
> **Sprint Plan**: `production/sprints/sprint-4.md` (Story Ledger, S4-06)
> **Status**: Blocked (depends on S4-01..S4-05)
> **Layer**: Presentation
> **Type**: Integration + human-play
> **Estimate**: 1.0 day
> **Manifest Version**: Unavailable (control-manifest absent project-wide; documented fallback applies)
> **Generated**: 2026-06-07
> **Owner**: Brian (product-owner — the N=1 human-play feel verdict)

## Context

**Authority**: `DECISIONS.md` D020; revised art bible §7.12 (EQ-Legibility Acceptance
Criteria — the six exit tests). This is the **milestone exit gate**: it re-runs the
human-play feel check that S3-06 failed, now against the executed presentation slice.

**Requirement Summary**: Prove the assembled EQ-readable presentation slice (S4-01
camera, S4-02 vitals, S4-03 target frame + con, S4-04 cast bar + prompt, S4-05 district)
is human-playable and reads as a playable classic-MMO-descended gothic slice — and
passes the §7.12 criteria. The human-play feel verdict is an **N=1 self-test** (the
product owner's call per the recorded S3-06 feel-gate plan; a runner/evidence scaffold
may be built, but the feel verdict is human, not runner output). This story folds in the
2026-06-07 code-review findings as hard requirements so the gate cannot pass on hollow
adapter-chain evidence: **[F1]** artifact-identity, **[F2]** real traversal, **[F3]**
real input path, **[F7]** exact telemetry sequence.

**Governing decisions**:

| D-entry | Status | Usage |
|---|---|---|
| D020 | Locked | EQ-readability pivot; this is the exit gate proving it landed |
| D016 | Locked | Greybox acceptable; the feel verdict separates "reads/plays" from "final-art pretty" |

**Art-bible authority**: §7.12 (the six EQ-Legibility Acceptance Criteria —
combat-state read, target identification, interaction confirmation, cast-state read,
HUD coherence, no routing).

**Surfaces composed**: all of S4-01..S4-05.

**Engine**: Unity 6.3 LTS. **Engine Risk**: MEDIUM (end-to-end play; Unity input-path
verification).

## Acceptance Criteria

- [ ] **S4-06-01**: The **six §7.12 EQ-Legibility Acceptance Criteria each pass**: (1) combat-state read; (2) target identification ≤1s; (3) interaction confirmation; (4) cast-state read; (5) HUD coherence with the city material vocabulary; (6) no routing (nothing tells the player where to go / what to value / what to feel).
- [ ] **S4-06-02 [F2]**: **Real player traversal proof — NOT marker teleport.** The player physically walks spawn -> `M3_Caretaker` -> relic -> vendor -> `M3_Caretaker` through movement (and NavMesh where relevant). The S3-03/S3-04 runners that *teleported* `ClericShellMarker` to each target prove adapter dispatch, NOT playable traversal; this gate requires the real walked route. (Closes review finding F2.)
- [ ] **S4-06-03 [F3]**: **Real input-path proof.** Interactions fire through the actual player input path — `Input.GetKeyDown(_interactKey)` in the harness `Update()` — **not** a direct `TryDispatchInteract()` method call. The evidence confirms the same telemetry sequence is produced by a real keypress, not a bypass. (Closes review finding F3.)
- [ ] **S4-06-04 [F7]**: **Exact main-path telemetry sequence**, not subsequence. The full accept -> recover -> loot -> sell -> hand-in telemetry is asserted as the exact ordered sequence with no hidden interleaved/duplicate/diagnostic events masking. (Closes review finding F7.)
- [ ] **S4-06-05 [F1]**: **Artifact-identity match.** The scene/NavMesh/bake-scope SHAs of the *played* build match the tuple S4-05 recorded — proving the build that was played is the authored scene, not a drifted one. (Closes review finding F1.)
- [ ] **S4-06-06**: **N=1 human-play feel verdict** recorded: the player answers the one-more-pull question, and on a PASS names a world element (objective / NPC / relic / the district) as the reason — not mechanical reward, completionism, testing, or "the game told me to". The protocol **separates "does it read/play as the game" from "is it final-art pretty"** (greybox is acceptable per D016; R-P2-FEEL-MISATTRIBUTION) so a missing-art deficit is not misread as a loop/legibility failure.

## Implementation Notes

- The runner/evidence scaffold (Codex may build it) covers F2/F3/F4/F7 mechanically; the **feel verdict (S4-06-06) is the product owner's human call** and cannot be a runner output.
- F1 tuple comparison: re-run the exact commands S4-05 recorded and compare; fail the gate if they don't match (the played scene drifted from the authored one).
- F3 input-path: prefer a one-frame Play Mode input simulation if feasible, OR human-play evidence that pressing the interact key fires the same telemetry sequence as the direct-dispatch tests.
- This is the milestone exit. A PASS retires the S2-M3-04 / S3-06 transferred human-play gate; a FAIL re-scopes within Sprint 4 (it does not silently downgrade).

## Out of Scope

- Produced-art quality as a pass condition (greybox acceptable; the gate is readability + loop-pull, not fidelity).
- New systems / Save/Load / faction consequence (T1 boundary holds).
- Fixing any S4-01..05 defect this gate surfaces — that routes back to the owning story.

## QA Test Cases

**Human-play (S4-06-01/06)**
- Setup: play the assembled slice end to end as a real player.
- Verify: the six §7.12 criteria hold; after playing, would you do one more pull, and is the reason a world element?
- Pass: 6/6 §7.12 criteria + voluntary re-engagement for a world reason; "reads/plays" judged separately from "final-art pretty".

**Integration (S4-06-02/03/04/05 — the hollow-evidence fences)**
- Setup: run the end-to-end runner + record the played build's artifact tuple.
- Verify: traversal is walked not teleported (F2); interactions fire via `Input.GetKeyDown` (F3); telemetry is exact-sequence (F7); artifact tuple matches S4-05 (F1).
- Pass: all four fences hold — the gate cannot pass on adapter-chain/teleport/bypass evidence.

## Test Evidence

**Required evidence**: `tests/evidence/S4-06/verification.md` +
`tests/evidence/S4-06/human-play-[YYYYMMDD].md` (the N=1 feel verdict — the product
owner's), plus the F1 tuple match, F2 traversal proof, F3 input-path proof, F7
exact-sequence telemetry.

**Evidence status**: Not started

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| `S4-01` | Play camera + debug-HUD isolation | Done |
| `S4-02` | Layer 1 vitals HUD | Done |
| `S4-03` | Target frame + con indicator | Done |
| `S4-04` | Cast bar + interaction prompt | Done |
| `S4-05` | District atmosphere + the F1 artifact-identity tuple | Done |

## Blockers

Blocked until all of S4-01..S4-05 are complete (this is the composition + exit gate).
The human-play feel verdict is the product owner's; it is not a runner output. A FAIL
re-scopes within Sprint 4; it does not downgrade to a qualified supplement (the
2026-05-20 feel-gate lesson).
