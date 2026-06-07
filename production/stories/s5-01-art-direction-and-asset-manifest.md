# S5-01: Art-Direction Pass + Bounded Asset Manifest

> **Sprint**: Sprint 5 — First District — Designed & Produced (First-Pass)
> **Sprint Plan**: `production/sprints/sprint-5.md` (Story Ledger, S5-01)
> **Status**: Blocked (depends on S5-00)
> **Layer**: Presentation
> **Type**: Design (direction + manifest — no code, no assets)
> **Estimate**: 1.0 day
> **Manifest Version**: Unavailable (control-manifest absent project-wide; documented fallback applies)
> **Generated**: 2026-06-07
> **Owner**: art-director

## Context

**Authority**: `DECISIONS.md` D021; art bible §1 (weight-and-age), §2 (practical light),
§4 (palette + per-faction colors), §6 (environment language), §8 (asset standards).

**Requirement Summary**: Translate the S5-00 design brief into **first-pass produced-art
direction** for the representative area, and produce the **asset manifest** — the
explicit, capped list of everything to be authored. The manifest IS the scope lock: it
is the difference between "first-pass produced area" and an open-ended art build. No
asset is selected that the S5-00 brief can't justify (§6.4). **No code, no assets.**

**Governing decisions**:

| D-entry | Status | Usage |
|---|---|---|
| D021 | Locked | First-pass produced direction; the manifest = the scope cap |
| D016 | Locked (amended) | Greybox default outside the representative area |

**Surfaces consumed**: the S5-00 design brief (place identity, faction identity,
occupation histories). This story does NOT design the place — it directs art for it.

**Engine**: Unity 6.3 LTS, URP. **Engine Risk**: LOW (direction/spec).

## Acceptance Criteria

- [ ] **S5-01-01**: A first-pass **material vocabulary** is selected for the representative area against the S5-00 brief + art bible §4/§6 — the **4 material sets** (street cobble, primary facade stone, interior plaster, timber trim), each with palette, age tier, and faction context (per the S5-00 faction identity).
- [ ] **S5-01-02**: The **practical-lighting approach** is specified: 3–6 source-motivated warm practicals (~2200–2600K) at actual in-world emitter positions, replacing default/greybox lighting. **Practical-source only** (§2 / §6.6) — no light placed for the player's benefit without an emitter.
- [ ] **S5-01-03**: **3–5 hero props** are identified (e.g. lantern post, faction-board physical object, doorstep threshold deposit) with material treatment, **each tied to an S5-00 occupation-history cause** (§6.4).
- [ ] **S5-01-04**: The **asset manifest** is produced — the explicit, capped list of every asset to author (the 4 material sets + the practical-lighting rig + the 3–5 hero props). **This manifest is the scope cap**: anything beyond it is a `[SCOPE]` lesson and a stop, not a silent add.
- [ ] **S5-01-05**: The direction holds the **fences** (D021 / §7.11): no guidance lighting, hero-lit objective doors, emissive/glowing interactables, rarity color, atmosphere-as-warning, or composition that frames the objective. Produced art lives entirely in the **world register**.
- [ ] **S5-01-06**: **No art-bible extension** is required (§1/§2/§4/§6/§8 govern). Any occupation-history gap that blocks a material/prop choice is flagged back to S5-00, **not invented** here.

## Implementation Notes

- Recommended first deliverable (de-risk, art-director's call): the primary street-surface + primary facade material as the vocabulary test, before the rest of the manifest is committed.
- Explicitly OUT of the manifest (scope traps): character art, the full multi-faction material library, VFX, produced art for off-route/secondary areas, interior produced art beyond the single hub space the player uses.
- The manifest feeds S5-02 (perf budgets + asset-spec). Keep it concrete enough to budget.

## Out of Scope

- Designing the place (S5-00). Producing the assets (S5-03). Perf budgets / asset-spec (S5-02).
- Final-polish art direction (this is first-pass). HUD/camera (S5-04).

## QA Test Cases

**Manual check (S5-01-04 manifest as scope cap)**
- Setup: review the manifest as the producer.
- Verify: it is an explicit, finite list (4 material sets + lighting + 3–5 props); every line cites an S5-00 purpose; character art / full faction library are explicitly excluded.
- Pass: a reader can count the assets and the cap is unambiguous; no open-ended "and dressing as needed".

**Manual check (S5-01-05 fences)**
- Setup: run each direction element through the State-Report-vs-World-Performance test.
- Verify: no element routes/hero-lights/advertises; all light has an in-world source.
- Pass: zero §7.11 violations in the direction.

## Test Evidence

**Required evidence**: `production/qa/evidence/s5-01-art-direction-and-manifest.md`
(the art-direction brief: material vocabulary, practical-lighting plan, hero-prop list
with §6.4 causes, the capped asset manifest, fence compliance).

**Evidence status**: Not started

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| `S5-00` | The design brief (place/faction identity + occupation histories) the direction is built against | Done |

## Blockers

Blocked on S5-00 (the design-the-place gate). Cannot select materials/props for an
undesigned place (D021).
