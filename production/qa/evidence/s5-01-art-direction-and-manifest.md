# S5-01 Art Direction and Bounded Asset Manifest

> **Story:** S5-01 - Art-Direction Pass + Bounded Asset Manifest
> **Date:** 2026-06-08
> **Evidence status:** PASS WITH NOTES
> **Mode:** Design evidence only. No code, no scene edits, no generated assets.
> **Purpose:** First seed of the "AI game artist" system: a text-only Art Judge v0 plus a capped S5 manifest. The judge is the taste gate before any generation pipeline exists.

---

## 1. Source Evidence

| Claim | Source | Verification method |
|---|---|---|
| S5-01 must select four material sets, a practical-lighting approach, 3-5 hero props, a capped manifest, and fence compliance. | `production/stories/s5-01-art-direction-and-asset-manifest.md:38-43` | Read story acceptance criteria. |
| S5-01 evidence belongs in this file. | `production/stories/s5-01-art-direction-and-asset-manifest.md:68-72` | Read story Test Evidence section. |
| Produced-art scope is spawn -> Caretaker only; rear relic/vendor zones stay greybox. | `production/qa/evidence/s5-00-first-district-design-brief.md:5-7` | Read approved S5-00 design brief header. |
| Court vocabulary is only on the Caretaker office ground-floor face; everything else is Resistance/residential substrate or civic-neutral. | `production/qa/evidence/s5-00-first-district-design-brief.md:67-75` | Read S5-00 R1. |
| The route lock uses the existing 30x30m court; produced art covers the forward half, boundary walls, and ground plane. | `production/qa/evidence/s5-00-first-district-design-brief.md:79-88` | Read S5-00 spatial ground truth. |
| The produced pass must avoid routing: no hero-lit CaretakerHall, no warmer/brighter CaretakerHall, no lantern next to Morrvik, and no composition framing the objective. | `production/qa/evidence/s5-00-first-district-design-brief.md:97-115` | Read S5-00 legibility and forbidden routing section. |
| The binding S5-01 handoff is street cobble, Caretaker-face Court ashlar, residential facade stone, timber trim, practical lighting, and 3-5 hero props. | `production/qa/evidence/s5-00-first-district-design-brief.md:161-168` | Read S5-00 handoff. |
| Art must earn its place through weight and age, not spectacle. | `design/art/art-bible.md:41-45` | Read art-bible Section 1. |
| Generic spectacle shortcuts are forbidden: glowing rarity, particle-dense mood VFX, clean old surfaces, jump-cut lighting, and generic bone decoration. | `design/art/art-bible.md:132-139` | Read art-bible teeth check. |
| Light must be practical, localized, and earned by in-world sources. | `design/art/art-bible.md:256-258` | Read art-bible cohesion DNA. |
| The interface/world boundary forbids routing or advertising values through HUD/world-performance signals. | `design/art/art-bible.md:1553-1560` | Read State-Report boundary. |
| Asset production must obey Section 8 file formats, naming, resolution, import, and validation constraints. | `design/art/art-bible.md:1585-1813` and `design/art/art-bible.md:1832-1846` | Read Section 8 standards and forbids. |
| D021 caps this pass at four material sets, 3-6 practicals, 3-5 hero props, and no silent additions. | `DECISIONS.md:845-851` | Read D021 scope cap. |
| D021 keeps practical-source-only and no-routing fences active for art PRs. | `DECISIONS.md:857-864` | Read D021 produced-art fences. |

---

## 2. Source Conflict / Resolution Note

Older S5-01 / D021 wording includes one interior-facing material slot (`production/stories/s5-01-art-direction-and-asset-manifest.md:38`; `DECISIONS.md:845-848`). The approved S5-00 brief narrows this pass to spawn -> Caretaker only and says no produced interiors this pass (`production/qa/evidence/s5-00-first-district-design-brief.md:7`, `:90-95`). Its S5-01 handoff replaces that slot with residential facade stone (`production/qa/evidence/s5-00-first-district-design-brief.md:163-168`).

**Resolution for this evidence:** follow the approved S5-00 handoff and use **residential facade stone** as material set 3. The active manifest contains no interior material requirement. The older story wording should be accepted as superseded at `/story-done` or reconciled in the story file in a later approved edit.

---

## 3. Art Judge v0

The first step in the super-agent is not generation. It is this gate:

> **Art Judge v0 decides whether an asset request belongs in Gravenspire before any image, mesh, texture, or prefab is generated.**

### Input Contract

The judge receives:

1. `design/art/art-bible.md`
2. `production/qa/evidence/s5-00-first-district-design-brief.md`
3. This S5-01 manifest
4. One structured asset request

Example request shape:

```json
{
  "asset_name": "prop_maj_neu_civic_lantern_post_01",
  "asset_type": "major_prop",
  "game_area": "the_sextons_court_spawn_to_caretaker",
  "purpose": "civic-neutral practical light source that helps the court read as inhabited city fabric",
  "camera_distance": "third_person_medium_distance",
  "style_requirements": [
    "weight and age",
    "oxidized iron",
    "weak warm oil light",
    "civic neutral",
    "not adjacent to Morrvik"
  ],
  "forbidden": [
    "guidance lighting",
    "blue magical flame",
    "skulls",
    "rarity color",
    "clean shiny metal",
    "objective framing"
  ],
  "technical_requirements": {
    "unity_pipeline": "URP",
    "max_materials": 2,
    "requires_collider": true,
    "pivot": "ground_center",
    "lod_required": true
  }
}
```

### Score Rubric

| Category | Weight | Pass bar | What the judge checks |
|---|---:|---:|---|
| Scope compliance | 20 | 18 | Spawn -> Caretaker only; no rear relic/vendor produced art; no interiors; no characters; no VFX. |
| Material cause | 20 | 16 | Every color, surface, wear mark, and prop has a physical/occupation cause. |
| World-register restraint | 20 | 18 | No routing, hero lighting, emissive interactables, rarity color, or atmosphere-as-warning. |
| Place readability | 15 | 12 | The court reads as a gothic civic place, not a test chamber, without centering the objective. |
| Faction/substrate correctness | 15 | 12 | Court vocabulary appears only on the Caretaker face; Resistance/residential/civic-neutral surfaces stay distinct. |
| Technical production risk | 10 | 8 | Naming, texture sizes, material count, collision, LOD, and URP assumptions stay within Section 8. |

**Verdict rules:**

- `APPROVE`: total score >= 86 and no hard-fail.
- `REVISE`: total score 70-85 or one fixable hard-risk.
- `REJECT`: total score < 70 or any hard-fail.

**Hard-fails:**

- Any generated request that needs a new art-bible rule.
- Any visual element that makes CaretakerHall brighter, warmer, larger, centered, or easier to follow than CourtVendorHall.
- Any lantern or prop cluster placed next to Morrvik to guide the player.
- Any skull/bone motif, glow, rarity color, magical fog, objective pin, waypoint logic, or atmospheric warning effect.
- Any new asset beyond the manifest below.
- Any use of "detail texture" or "variation texture" without a physical cause slot.

---

## 4. First-Pass Direction

### Material Vocabulary

| Manifest id | Material set | Palette / material read | Age tier / history | Faction context | Cause test |
|---|---|---|---|---|---|
| `mat_01` | Street cobble | Quarry Stone / Pewter Rain exterior stone, with Iron Seam in cracks and low points. | Stratum 2-3 court paving, 200+ years of foot traffic. | Civic-neutral. | Centerline polish from daily crossing; moss and dirt only at low-traffic edges; drainage staining follows slope/wear, not decoration. |
| `mat_02` | Caretaker-face Court ashlar | Dressed ashlar precision, tarnished silver hardware, restrained cool Court read. | Stratum 2 guild hall with ~200-year Court ground-floor modification. | Vampire Court only on Caretaker office ground-floor face. | Court power reads as precision of maintenance: squared entry surround, marble threshold over original wear-bowl, tarnish from time rather than neglect. |
| `mat_03` | Residential facade stone | Worn civic/residential stone, oxidized iron hardware, domestic soot, moss at grout lines. | Resistance/residential occupation over 50-80 years. | Living Resistance substrate / civic residential, not formal faction display. | Misaligned partition tells, soot from occupancy, repaired bracket scars, no Court hardware. |
| `mat_04` | Timber trim | Dark, old, unrefined timber with lap joints, repaired brackets, and edge wear. | 50-200 years depending on repair layer. | Civic-neutral / Resistance substrate. | Wear at hand-height and hinge contact; species mismatch where repairs happened; not fresh decorative beams. |

**AC note:** This satisfies S5-01-01 with the S5-00-scoped substitution described in Section 2.

### Practical Lighting Plan

Use four source-motivated warm practicals, all treated as world objects first and light sources second.

| Manifest id | Source | Approximate placement intent | Temperature | Fence check |
|---|---|---|---:|---|
| `light_01` | Civic-neutral lantern post | West / left court edge, not adjacent to Morrvik and not pointing at the Caretaker door. | 2200-2400K | Reads as street infrastructure, not objective guidance. |
| `light_02` | Civic-neutral lantern post | East / right court edge, visually balancing `light_01` so CaretakerHall is not warmer/brighter than CourtVendorHall. | 2200-2400K | Equivalent practical lighting across visible landmarks. |
| `light_03` | Weak Caretaker office hearth/window glow | From the recessed Caretaker office face, visible as occupancy warmth but not placed over Morrvik or the door. | 2400-2600K | Must be weaker than route-guidance threshold; remove or mirror if it makes CaretakerHall read as destination. |
| `light_04` | Low M2 camp fire bowl | Near the existing camp rest zone, separated from Caretaker-facing props so it does not merge into a guidance cluster. | 2200-2400K | Explains camp use; does not point the player to Morrvik. |

**AC note:** This satisfies S5-01-02 as a 3-6 practical-source plan. Final Unity positions and photometric values are S5-03/S5-05 evidence, not claimed here.

### Hero Props

| Manifest id | Prop | Material treatment | Occupation-history cause | Fence check |
|---|---|---|---|---|
| `prop_01` | Civic lantern post | Oxidized iron, dull glass, soot-darkened cap. | The city maintains lanterns because the ledger requires street function, not because the Court cares about the player. | No special light cone, glow, or placement next to Morrvik. |
| `prop_02` | Caretaker dispatch box | Tarnished silver latch, gray-blue vellum scraps, controlled rectangular geometry. | Court-licensed administration leaves physical paperwork at the threshold; protocol is old, maintained, and impersonal. | Specificity, not prominence; must not become an interactable glow. |
| `prop_03` | Threshold wear/deposit tray | Marble threshold edge over older stone wear-bowl, small wax/deposit residue. | Estate claims and death-administration happen at the threshold; objects accumulate where people wait, not where the player should go. | Low profile; readable close-up, not from spawn as objective marker. |
| `prop_04` | Residential repair bundle | Timber offcuts, iron bracket, tied cloth, aged tool marks. | Resistance/residential tenants make practical repairs to old civic buildings. | Placed on residential/city-fabric side, not at the Caretaker door. |
| `prop_05` | Maintenance cart | Dull timber, oxidized iron rims, uneven load of civic materials. | Boundary walls and court surfaces are maintained just enough to function; deferred work remains visible. | Must not block route or form a visual arrow toward the objective. |

**AC note:** This satisfies S5-01-03 with five hero props, each tied to an S5-00 occupation cause.

---

## 5. Capped Asset Manifest

This is the S5-01 scope cap. Anything beyond this list is a `[SCOPE]` lesson and a stop.

### Material Sets

1. `mat_01_street_cobble`
2. `mat_02_caretaker_face_court_ashlar`
3. `mat_03_residential_facade_stone`
4. `mat_04_timber_trim`

### Practical Lighting Rig

1. `light_01_civic_lantern_west`
2. `light_02_civic_lantern_east`
3. `light_03_caretaker_office_hearth_glow`
4. `light_04_m2_camp_fire_bowl`

### Hero Props

1. `prop_01_civic_lantern_post`
2. `prop_02_caretaker_dispatch_box`
3. `prop_03_threshold_deposit_tray`
4. `prop_04_residential_repair_bundle`
5. `prop_05_maintenance_cart`

### Explicit Exclusions

- No character art.
- No full faction material library.
- No VFX.
- No produced art for the rear relic/vendor zones.
- No produced interiors.
- No Save/Load, faction consequence, networking, LLM, or extra class work.
- No extra dressing "as needed."
- No Addressables work in S5-01; S5-02/S5-03 may make technical implementation calls if approved.

---

## 6. Fence Compliance

| Fence | Result | Evidence |
|---|---|---|
| No guidance lighting | PASS WITH NOTES | Lighting is balanced across visible landmarks and uses in-world sources only; final scene proof remains S5-03/S5-05. |
| No hero-lit objective doors | PASS WITH NOTES | `light_03` is explicitly weak and removable if it makes CaretakerHall read as destination. |
| No emissive/glowing interactables | PASS | No manifest item uses emission as gameplay signal. Practical flame/glass is source-motivated only. |
| No rarity color / loot glow | PASS | No item-value color treatment exists in the manifest. |
| No atmosphere-as-warning | PASS | No mist, pulse, particle mood, or combat-warning treatment is requested. |
| No composition framing objective | PASS WITH NOTES | Prop and lighting instructions forbid centering Morrvik/CaretakerHall; S5-03 must screenshot-check spawn camera framing. |
| No invented occupation history | PASS | Each prop/material uses S5-00 causes; unresolved rear-zone histories remain greybox. |

---

## 7. Acceptance Criteria Mapping

| AC | Verdict | Evidence |
|---|---|---|
| S5-01-01 | PASS WITH NOTES | Section 4 selects four material sets. Note the S5-00-scoped substitution in Section 2. |
| S5-01-02 | PASS | Section 4 defines four practical-source lights, within the required 3-6 range. |
| S5-01-03 | PASS | Section 4 defines five hero props with occupation-history causes. |
| S5-01-04 | PASS | Section 5 is an explicit, countable cap with no open-ended dressing clause. |
| S5-01-05 | PASS WITH NOTES | Section 6 applies the D021 / art-bible fences; runtime screenshot proof remains future evidence. |
| S5-01-06 | PASS WITH NOTES | No art-bible extension is required. The only note is older interior-facing wording versus the S5-00 no-interiors scope. |

---

## 8. What The Future Super-Agent Gets From This

This evidence file is the first seed of an AI art department:

1. The Art Judge has a source-backed taste rubric.
2. Asset requests have a structured input contract.
3. The manifest defines exactly what a generator may attempt.
4. Hard-fails prevent generic AI fantasy before it reaches Unity.
5. Future screenshot judging has concrete expectations to test: balanced practicals, no objective framing, place-read before route-read, and Section 8 technical compliance.

The next build step would be a text-only CLI or agent prompt that reads this file plus the art bible and returns an `APPROVE`, `REVISE`, or `REJECT` report for one asset request. No image generation should be added until that judge can reliably reject attractive-but-wrong requests.
