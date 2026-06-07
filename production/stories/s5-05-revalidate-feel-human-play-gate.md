# S5-05: Re-Validate Feel — Re-Targeted Human-Play Gate

> **Sprint**: Sprint 5 — First District — Designed & Produced (First-Pass)
> **Sprint Plan**: `production/sprints/sprint-5.md` (Story Ledger, S5-05)
> **Status**: Blocked (depends on S5-00..S5-04)
> **Layer**: Presentation
> **Type**: Integration + human-play
> **Estimate**: 1.0 day
> **Manifest Version**: Unavailable (control-manifest absent project-wide; documented fallback applies)
> **Generated**: 2026-06-07
> **Owner**: Brian (product-owner — the N=1 human-play feel verdict)

## Context

**Authority**: `DECISIONS.md` D021 (the re-targeted exit gate); art bible §7.12
(EQ-Legibility Acceptance Criteria) **+** a new place-read criterion added per D021.
This is the **milestone exit gate** — it re-runs the human-play check that S3-06 and the
S4-01 pass failed, now against the designed-and-produced representative area + the landed
legibility floor.

**Requirement Summary**: Prove the assembled Sprint 5 slice (S5-03 produced area + S5-04
legibility floor, on the S5-00-designed place) is human-playable and **resembles a
playable demo** — it reads as a gothic place AND plays legibly — and passes the
re-targeted gate. The N=1 product-owner verdict folds in the F1/F2/F3/F7 hollow-evidence
fences so the gate cannot pass on adapter/teleport/bypass evidence.

**Governing decisions**:

| D-entry | Status | Usage |
|---|---|---|
| D021 | Locked | The re-targeted exit gate proving the produced-art pivot landed |
| D016 | Locked (amended) | Greybox-elsewhere acceptable; the judged area is the produced one; "first-pass" not "final-art" |

**Surfaces composed**: S5-00..S5-04 (design, direction, spec, produced art, legibility floor).

**Engine**: Unity 6.3 LTS. **Engine Risk**: MEDIUM (end-to-end play; input-path verification).

## Acceptance Criteria

- [ ] **S5-05-01** *(new per D021)*: **Place-read** — a first-time viewer reads the spawn → Caretaker representative area as a **specific gothic place**, not a Unity prototype scene. (Greybox off-route is acceptable; the produced area is what's judged.)
- [ ] **S5-05-02**: The art-bible **§7.12 EQ-legibility criteria** hold to the extent the landed legibility floor (S5-04) covers them: combat/state read (camera + any HUD landed), interaction confirmation (the prompt named the target; result legible), and **no routing** (§7.12-6 — nothing tells the player where to go / what to value / what to feel).
- [ ] **S5-05-03**: The **F1/F2/F3/F7 hollow-evidence fences** are intact — artifact-identity match (the played build is the authored scene), real **walked** traversal (not marker teleport), real **input-path** firing (`Input.GetKeyDown`, not direct dispatch), exact-sequence main-path telemetry (not a subsequence). The gate cannot pass on adapter/teleport/bypass evidence.
- [ ] **S5-05-04**: The **N=1 product-owner human-play verdict** is recorded: place-read **and** state-read judged together; on a PASS the player answers "one more pull" and names a **world element** (the place / an NPC / the relic) as the reason — not mechanical reward, completionism, testing, or "the game told me to". The protocol separates "reads/plays + resembles a demo" from "is it *final-art* pretty" (first-pass is the bar; greybox-elsewhere acceptable).
- [ ] **S5-05-05**: **On FAIL, diagnose which axis** failed (world-read vs state-read) **before** re-scoping — do not blindly add more art (R4). A FAIL re-scopes within Sprint 5; it does not silently downgrade to a qualified supplement (the 2026-05-20 feel-gate lesson).

## Implementation Notes

- The runner/evidence scaffold (Codex may build it) covers F1/F2/F3/F7 mechanically; the **feel verdict (S5-05-04) is the product owner's human call** and cannot be a runner output.
- This is the third feel-checkpoint in the S3-06 → S4-01 → S5-05 sequence. A PASS retires the transferred human-play gate and validates the D021 pivot; a FAIL routes back to the owning story (world-read → S5-00/S5-03; state-read → S5-04) with an explicit axis diagnosis.
- Compare the played build's artifact tuple against the one S5-03 recorded ([F1]); fail if they don't match.

## Out of Scope

- Producing more art or fixing surfaced defects (route back to the owning story).
- Final-art quality as a pass condition (the bar is place-read + legibility + loop-pull, first-pass).
- New systems / Save-Load / faction consequence (Tier-1 holds).

## QA Test Cases

**Human-play (S5-05-01/02/04)**
- Setup: play the assembled slice end to end as a first-time player.
- Verify: the produced area reads as a gothic place; state is legible (S5-04 floor); nothing routes; after playing, would you do one more pull, and is the reason a world element?
- Pass: place-read + state-read + voluntary re-engagement for a world reason; "reads/plays" judged separately from "final-art pretty".

**Integration (S5-05-03 — the hollow-evidence fences)**
- Setup: run the end-to-end runner + record the played build's artifact tuple.
- Verify: F1 tuple matches S5-03; F2 traversal walked not teleported; F3 interactions via `Input.GetKeyDown`; F7 telemetry exact-sequence.
- Pass: all four fences hold.

## Test Evidence

**Required evidence**: `tests/evidence/S5-05/verification.md` +
`tests/evidence/S5-05/human-play-[YYYYMMDD].md` (the N=1 product-owner verdict), plus
the F1 tuple match, F2 traversal proof, F3 input-path proof, F7 exact-sequence telemetry.

**Evidence status**: Not started

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| `S5-00` | The designed place the produced area realizes | Done |
| `S5-01` | The art direction + manifest | Done |
| `S5-02` | The asset-spec + perf budgets | Done |
| `S5-03` | The produced representative area + the [F1] artifact tuple | Done |
| `S5-04` | The landed legibility floor (camera + debug-HUD isolation) | Done |

## Blockers

Blocked until S5-00..S5-04 are complete (this is the composition + exit gate). The
human-play feel verdict is the product owner's; it is not a runner output. A FAIL
re-scopes within Sprint 5 with an axis diagnosis; it does not downgrade.
