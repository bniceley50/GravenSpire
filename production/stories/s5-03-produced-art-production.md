# S5-03: Produced-Art Production (Representative Area)

> **Sprint**: Sprint 5 — First District — Designed & Produced (First-Pass)
> **Sprint Plan**: `production/sprints/sprint-5.md` (Story Ledger, S5-03)
> **Status**: Complete
> **Layer**: Presentation
> **Type**: Visual/Feel
> **Estimate**: 3.0 days
> **Manifest Version**: Unavailable (control-manifest absent project-wide; documented fallback applies)
> **Generated**: 2026-06-07
> **Owner**: Codex / art

## Context

**Authority**: `DECISIONS.md` D021; art bible §1 (weight-and-age), §2 (practical light),
§4 (palette), §6 (environment language), §6.4 (occupation-explainable props), §8 (asset
standards); the `.claude/rules/game-dev-governance.md` Scene Discipline.

**Requirement Summary**: Build the S5-01 manifest — 4 material sets + a practical-source
lighting pass + 3–5 hero props — on the spawn → Caretaker representative area, to
**first-pass produced fidelity** (real materials/lighting/dressing that read as a gothic
place; **not** shipping-final polish). This is the produced-art surface the re-targeted
feel gate (S5-05) judges for place-read. Greybox stays everywhere else.

**Governing decisions**:

| D-entry | Status | Usage |
|---|---|---|
| D021 | Locked | First-pass produced area; manifest cap; fences; perf-budget ceiling |
| D016 | Locked (amended) | Greybox default outside this area |
| D001 | Locked | URP only |

**Surfaces consumed**: the S5-00 design brief, the S5-01 manifest + direction, the S5-02
asset-spec + perf budgets. The S3-05 district geometry (reused, not rebuilt).

**Engine**: Unity 6.3 LTS, URP. **Engine Risk**: MEDIUM-HIGH (first produced-art pass on
an unprofiled project; URP materials/lighting/GI — verify against
`docs/engine-reference/unity/`; batchmode reserializes ProjectSettings — 2026-05-26 lesson).

## Acceptance Criteria

- [ ] **S5-03-01**: The **4 material sets** are authored to the S5-02 asset-spec and applied to the representative-area geometry (street cobble, Caretaker-face Court ashlar, residential facade stone, timber trim) per art bible Section 4 / Section 6 and the S5-01 direction. **First deliverable + mid-pass review**: the street + facade material in-engine on the S3-05 geometry, evaluated by the art-director before the rest is committed.
- [ ] **S5-03-02**: The **practical-lighting pass** is implemented — the 3–6 source-motivated warm practicals at in-world emitter positions; default/greybox lighting removed. **Practical-source only** (§6.6): no light placed for the player's benefit without an emitter.
- [ ] **S5-03-03**: The **3–5 hero props** are placed with material treatment, **each tied to its S5-00 occupation cause** (§6.4) — no prop exists without a person/activity explaining it.
- [ ] **S5-03-04**: The **fences hold** (D021 / §7.11): no guidance lighting, hero-lit objective doors, emissive/glowing interactables, rarity color, atmosphere-as-warning, or composition that frames the objective. The per-element State-Report-vs-World-Performance test is applied; **creative-director gate on this PR**.
- [ ] **S5-03-05**: **Scope = the S5-01 manifest only**; anything beyond it is a `[SCOPE]` lesson and a stop. The produced sub-slice is **profiled against the S5-02 budget**; over-budget → cut dressing, not framerate (and the measured values lock the provisional budget framework).
- [ ] **S5-03-06**: **Scene discipline** (RG-S): adapter/additive scene edit; one scene edit per PR; save-then-diff before staging; no legacy-builder chaining (2026-05-30 lesson); ProjectSettings/Packages drift restored (2026-05-26 lesson); **sequence vs S5-04** (both may touch `_DevEntry.unity`), never concurrent; Unity Smart Merge.

## Implementation Notes

- Build to the S5-02 asset-spec and perf budgets; if a spec value is missing, escalate rather than guess.
- The mid-pass street+facade review (S5-03-01) is the cheap checkpoint: if the material vocabulary doesn't achieve the weight-and-age read on this geometry, that is revealed before the full pass is committed (and may indicate a geometry/layout issue, not a material one — route back to S5-00, do not deepen the material pass).
- Verify URP material/lighting/GI APIs against `docs/engine-reference/unity/` (post-6.0 UNVERIFIED).

## Out of Scope

- Design (S5-00), direction/manifest (S5-01), perf-spec (S5-02). HUD/camera (S5-04).
- Final-polish art. The rest of the district (stays greybox). Character art. Any asset beyond the manifest.

## QA Test Cases

**Manual check (S5-03-01/02/03 reads-as-place)**
- Setup: walk the produced spawn → Caretaker area in Play Mode.
- Verify: the area reads as a specific gothic place (material vocabulary, practical light, occupation-explainable props); off-route greybox is acceptable.
- Pass: place-read achieved at first-pass produced fidelity; every produced prop has a §6.4 cause.

**Manual check (S5-03-04 fences) + profile (S5-03-05)**
- Setup: inspect each produced element against §7.11; profile the sub-slice against the S5-02 budget.
- Verify: zero routing/hero-lighting/emissive/atmosphere-as-warning; within budget (or dressing cut to fit).
- Pass: fences clean; profile within the (now lockable) budget.

## Test Evidence

**Required evidence**: `production/qa/evidence/s5-03-produced-art-evidence.md`
(walkthrough screenshots; manifest-complete confirmation; practical-light + §6.4 cause
confirmation; the perf profile vs budget; fence + scene-discipline confirmation; the
[F1] artifact-identity tuple for the S5-05 gate).

**Evidence status**: Not started

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| `S5-02` | The asset-spec + perf budgets the production builds to | Done |

## Blockers

Blocked on S5-02 (asset-spec + budgets), which is blocked on S5-01 (manifest), which is
blocked on S5-00 (design). Sequence vs S5-04 for scene safety.

## Completion Notes

**Completed**: 2026-06-09
**Verdict**: COMPLETE WITH NOTES
**Criteria**: 6/6 PASS (S5-03-05 with the by-design note that the provisional budget
framework locks finally at the S5-05 play profile).
**Test Evidence**: `production/qa/evidence/s5-03-produced-art-evidence.md` — manifest
completion (4 material sets + lighting + 5 props at cap), [F1] tuple at final scene
state (`bae11334` @ `67fe9c4`), perf snapshot all-PASS, fence register, scene-discipline
record, 8 walkthrough screenshots in `tests/evidence/S5-03/`, and M2 preservation
**3/3 PASS** (`m2-0{2,3,4}-preservation-20260609-smoke.md`).
**Review Gates**: product-owner mid-pass GO; **creative-director gate PASS WITH
ADJUSTMENTS** — caught the Caretaker-corner lantern co-lighting Morrvik + the door niche
(soft routing) and an unevidenced parity claim; both adjustments applied and re-captured
(`1d92daf`). The Evidence Rule working as intended: the register was corrected to match
pixels.
**GDD/ADR Deviations**: None. D021 fences held; Tier-1 intact; URP-only.
**Scope Notes**: NPC placeholder palette added mid-story by product-owner direction
(design pass + tints; commit `443db2f`) — recorded, bounded, no manifest breach. Cobble
intake briefly deviated to 2048; caught by Codex-lane review, corrected to the 1024 spec
(`32f6451`). ProjectSettings linear-intensity/color-temperature adopted deliberately
(the gated lighting depends on it; `95a0bb9`).
**Carried forward**: CD tone-deepening note ("cursed gothic" sky/grime — next art pass);
NPC body production (Hyper3D experiment or rigged-mesh path) routed separately; final
budget lock at S5-05.
**Forced Completion**: No.
