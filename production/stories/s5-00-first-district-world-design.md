# S5-00: First District World/Level Design (Representative Area)

> **Sprint**: Sprint 5 — First District — Designed & Produced (First-Pass)
> **Sprint Plan**: `production/sprints/sprint-5.md` (Story Ledger, S5-00)
> **Status**: Ready
> **Layer**: Presentation
> **Type**: Design (spec/brief — no code, no assets)
> **Estimate**: 2.0 days
> **Manifest Version**: Unavailable (control-manifest absent project-wide; documented fallback applies)
> **Generated**: 2026-06-07
> **Owner**: level-designer + world-builder

## Context

**Authority**: `DECISIONS.md` D021 (First District produced-art pivot — the
non-skippable design-the-place gate); art bible `design/art/art-bible.md` §1
(weight-and-age), §2 (mood — practical light only), §6 (environment design language),
§6.4 (environmental-storytelling: every prop explainable by a person/activity).

**Requirement Summary**: The First District was blocked out for *mechanics* (S3-05),
never *designed as a place*. Per D021, producing art for an undesigned place is the
central trap and is forbidden — generic atmosphere fails both the demo bar and Pillar 1.
This story designs the **one representative area** (the spawn → Caretaker Morrvik path
the objective loop already walks) AS A PLACE: place identity, locked route, faction
identity, and a per-building occupation-history brief. Output is a design brief that
gates all downstream produced-art work (S5-01 selects materials *against* it; S5-03
produces *from* it). **No code, no assets.**

**Governing decisions**:

| D-entry | Status | Usage |
|---|---|---|
| D021 | Locked | The design-the-place gate this story IS; no asset before it |
| D016 | Locked (amended by D021) | Greybox remains the default outside this representative area |
| D003 | Locked | Tier-1 — the place is indifferent; no new systems |

**Engine**: Unity 6.3 LTS. **Engine Risk**: LOW (design/spec, no runtime surface).

## Acceptance Criteria

- [ ] **S5-00-01**: The representative area (spawn → Caretaker Morrvik) has a written **place identity** — what this district is, who lives/lived here, and what each space along the route is *for*. Not "generic gothic street": a specific place with a reason to exist.
- [ ] **S5-00-02**: The **primary route is locked** — the geometry/path the art will sit on is final (reuse the S3-05 district geometry). Any geometry that needs revision before art is explicitly flagged here, so produced materials are never applied to surfaces that will move.
- [ ] **S5-00-03**: The First District's **faction identity** is assigned — which faction's material vocabulary (art bible §4/§6) appears in the representative area. This is the input that determines which materials S5-01 selects (e.g. tarnished-silver vs oxidized rust-iron door hardware).
- [ ] **S5-00-04**: Each building receiving produced art carries a **2–3 sentence occupation-history brief** (art bible §6.4 — who occupied it, in what sequence, what activities) so every produced prop/decal in S5-03 is explainable by a person or activity, not placed for theater.
- [ ] **S5-00-05**: **Sightline/massing intent** is documented for *legibility* (the player can read and navigate the space) **without routing** — no composition that frames the objective, no "the vista points you at the hand-in door" (Pillar 1 / §7.11). Spatial readability ≠ objective signposting.
- [ ] **S5-00-06**: The brief holds the **D021 fences and Tier-1**: the place is indifferent (no atmosphere-as-warning, no guidance, no performing-for-the-player); no new systems; greybox stays the default everywhere outside the representative area.

## Implementation Notes

- This is the trap-breaker per D021. The §6.4 environmental-storytelling gate is the lock: if S5-01's manifest later contains an asset line that can't cite a purpose from this brief, that is a stop-and-return-to-S5-00 signal.
- Reuse S3-05 geometry; do not redesign the district. If the existing blockout cannot support the place identity, flag a bounded geometry revision here rather than papering over it with art.
- The brief covers only the primary route (spawn → Caretaker) — not the whole district. Everything off-route stays greybox.

## Out of Scope

- Material/lighting/prop selection (S5-01 art-direction) and the asset manifest.
- Any produced art (S5-03) or perf/asset-spec (S5-02).
- HUD/camera (S5-04). District geometry rebuild beyond a flagged bounded revision.

## QA Test Cases

**Manual check (S5-00-01/04 place identity + occupation history)**
- Setup: read the design brief as an artist about to produce the area.
- Verify: every space along the route has a stated purpose; every building to be produced has a 2–3 sentence occupation history.
- Pass: an artist could select materials and place props with each choice traceable to a person/activity; no "generic atmosphere" gaps.

**Manual check (S5-00-05 legibility-not-routing)**
- Setup: review the sightline/massing intent against §7.11.
- Verify: spatial readability is layout/massing/sightline — never composition that frames or points at the objective.
- Pass: zero routing/guidance elements; the space is navigable AND indifferent.

## Test Evidence

**Required evidence**: `production/qa/evidence/s5-00-first-district-design-brief.md`
(the design brief: place identity, locked route, faction identity, per-building
occupation histories, sightline/massing intent, fence compliance).

**Evidence status**: Not started

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| None | First story; the non-skippable design gate that unblocks all produced-art work | — |

## Blockers

None. D021 + the art bible are the authority. This story unblocks S5-01.
