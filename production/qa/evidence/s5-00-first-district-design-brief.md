# S5-00 — First District Design Brief: The Sexton's Court (Representative Area)

> **Story**: S5-00 — First District World/Level Design (Representative Area)
> **Date**: 2026-06-07
> **Status**: APPROVED / LOCKED 2026-06-07 (product owner). PROVISIONAL canon items (district name, Morrvik's 19 years, relic significance) remain revisitable per the §2 classification table; the LOCKED claims are now canon.
> **Authority**: DECISIONS.md D021 (design-the-place gate); art bible §1/§2/§3/§4/§5.2/§6/§6.4; D003 (Tier-1)
> **Produced-art scope this pass (product-owner decision)**: **spawn → Caretaker beat only** — the Threshold Court + Caretaker face. The relic/vendor rear zones stay greybox this pass (D021 scope lever).
> **Authored by**: world-builder + level-designer (S5-00 execution); canon ratified by narrative-director (RATIFY WITH REVISIONS); synthesized for review.

---

## 1. District Identity (S5-00-01)

**The Sexton's Court** — the lower administrative edge of Gravenspire's former
civic-ecclesiastical precinct. In the living city this quarter was run by a **sexton's
guild**: lay administrators of the city's dead (burial permits, catacomb upkeep,
interment records). When the undead factions took political power (~250–150 yrs ago,
art-bible Stratum 3), the guild dissolved — there is no burial economy when no one dies
permanently. Its buildings were **absorbed, not demolished**.

"Caretaker" is the surviving guild title, **repurposed**: under the current city a
Caretaker is a **Vampire-Court-licensed intermediary** who administers the physical
estate of the living and recently-dead. **Caretaker Morrvik is a living human** —
Court-sanctioned, *not* Court-affiliated — 19 years in post.

The Court maintains this space because property records require physical administration
by someone who can meet the living by day. The space exists, and is indifferent to the
player (Pillar 1): Morrvik is here because the Caretaker's office has been here for 150
years; the relic is in the unclaimed corner because an estate dispute put it there.

*(Name reconciliation: the world-builder proposed "Sexton's Passage"; the geometry is a
flat open court (not a through-route), and the narrative-director flagged "Passage"
implies transit. Locked as "The Sexton's Court.")*

## 2. Canon (narrative-director ratification — RATIFY WITH REVISIONS)

No prior APPROVED canon pinned this district. The following is established here:

| Claim | Classification |
|---|---|
| Sexton's guild — dissolved Stratum-3 lay administrative institution | **LOCKED** |
| "Caretaker" — Court-licensed intermediary title repurposed from the guild | **LOCKED** |
| Morrvik — living human, Court-licensed, not Court-affiliated | **LOCKED** |
| Faction read — Court administrative surface over Resistance residential substrate | **LOCKED** |
| Vendor (`M3_CourtVendor`) — separate NPC, thin Court affiliation, non-political | **LOCKED** |
| Combat-camp ghouls — feral creatures, no Syndicate garment vocabulary (§5.2/§5.6) | **LOCKED** |
| District name "The Sexton's Court" | **PROVISIONAL** (confirm vs final layout) |
| Morrvik 19 years in post | **PROVISIONAL** (timeline anchor; revisit at M5/Tier-3) |
| Relic as displaced estate property-title evidence, not magically significant | **PROVISIONAL** (M5 may add political layering) |

**R3 continuity footnote (for future writers):** the Caretaker title is Court-*licensed*
but **not** a Court faction role; Caretakers are neither Court members nor Resistance
members — a licensed intermediary class. Morrvik's dialogue/visual vocabulary is a
specific **civilian-institutional register**, neither Court-formal nor Resistance-improvised.

**Forward watch-items (left UNRESOLVED, by design):**
- M5: the property-title relic may carry political leverage between Court and Resistance —
  coordinate with game-designer at M5 entry; the "not magically significant" framing holds.
- Tier 3: Morrvik (a living human under Court license in a Resistance-substrate area) is a
  ready point of tension/trust for the Living Resistance — reserved, do not resolve here.

## 3. Faction Identity / Material Read (S5-00-03) — input for S5-01

**Vampire Court (administrative) over a Living Resistance residential substrate** — a
two-stratum palimpsest, not a single-faction space.

**R1 — Court vocabulary is scoped to the Caretaker's office facade ONLY:**
- **Court (Caretaker office ground-floor face only):** dressed ashlar precision, Court
  window-surround treatment, **tarnished-silver** entry hardware, marble threshold panel
  over the original wear-bowl. Court power reads as *precision of maintenance*, not abundance.
- **Resistance substrate (everything else):** worn civic/residential stone, **oxidized
  iron** hardware, moss at grout lines, domestic soot, un-refined timber. No Court surface
  modification on residential faces or upper floors.
- Street lantern posts: civic-neutral iron (the Court maintains them because the ledger
  requires it, not because it cares about the street).

## 4. Spatial Design — The Threshold Court (S5-00-01 spatial / S5-00-02 / S5-00-05)

**Ground truth (level-designer, from the real S3-05.1 coordinates):** a **30×30 m flat
open court**, zero elevation change. Spawn `ClericShellMarker` (0,0,0); Caretaker (2,0,-4.2)
standing *in front of* `Greybox_CaretakerHall_Massing` (~3,0,-7); `M3_CourtVendor` (4,0,-3.6)
before its hall (~6,0,-6); relic (-1.85,0,3.15) + storehouse (~-3,0,9) *behind* spawn;
the M2 camp co-located in the same forward court (`M2_CampRestPoint` 0,0,-5). The objective
loop is a figure-eight crossing spawn twice.

**Route lock (S5-00-02): no structural geometry revision required.** The blockout supports
the route. The produced-art beat is the forward half: spawn → Caretaker, plus the city-fabric
boundary walls and ground plane that frame it.

**Fiction-onto-geometry reconciliation (no interiors this pass):** the Sexton's Court is a
small civic *court* bounded by the CaretakerHall face, the CourtVendorHall face, and
city-fabric boundary walls. Morrvik's "administrative threshold" is delivered as a **recessed
-entrance facade treatment on the CaretakerHall face — an art-layer treatment, NOT a geometry
change and NOT a produced interior.** The "alley/courtyard" of the fiction maps to the M2
corner of the same court.

**Legibility WITHOUT routing (the central D021/§7.11 constraint) — mechanisms:**
1. Two landmarks (CaretakerHall, CourtVendorHall) are both visible from spawn; **neither is
   center-framed, hero-lit, brighter, or warmer than the other** — equivalent practical
   lighting on both.
2. The court reads as a *place with a shape* (two building faces + city-fabric boundary walls)
   before it reads as "go to the left building." Place-read first, then investigation.
3. Significance via **specificity/resolution, not prominence** (§3.4): the Caretaker face is
   *more historically resolved*, not brighter/bigger/decorated.
4. Morrvik stands in open court, **not doorway-framed**; the recessed entrance must **not
   center-frame him from the spawn camera angle** (level-designer art-constraint; CD-gate item).
5. Boundary walls **must read as city fabric** (building backs / district walls), not engine
   limits — this is what makes the court a *place* and not a test chamber.

**Explicit routing rejections (FORBIDDEN, §7.11 / Pillar 1):** no vista that frames the
Caretaker/hall door as a centered terminus; no sightline from the Caretaker toward the relic
pre-quest; no material/lighting making CaretakerHall read brighter/warmer than CourtVendorHall
from spawn; no lantern positioned over/next to Morrvik; no prop cluster at the relic position
visible-as-a-point-of-interest from spawn; the rear/South boundary wall must not become a
sightline attractor.

## 5. Per-Building Occupation Histories (S5-00-04) — beat-scoped

**CaretakerHall (the primary produced building):** originally the sexton's guild hall
(~350 yrs, Stratum 2). Court took administrative control in the Transition (~200 yrs ago)
and commissioned a **ground-floor surface modification only** (Court doorway surround, marble
threshold over the original wear-bowl, silver hardware now tarnished). Upper two floors were
*not* touched — they continue as residential (document-runner tenants, ~40 yrs continuous).
The two-register split is visible from the street: formal Court ground floor vs residential
upper floors (original window proportion, domestic soot, a bowed second-floor wall, unrepaired
stone where a downpipe bracket was removed). Morrvik's 19 years have worn an oval in the floor
before his desk (interior detail — reserved, not produced this pass).

**The spawn-street residential faces (Zone A):** former ground-floor guild-supplier trade
(scribes, wax-seal vendors), subdivided into unauthorized residential by successive Resistance-
affiliated tenants over 50–80 yrs. Ground floors show partition tells (doorways misaligned with
windows, half-height walls); upper floors show normal residential wear. **No Court hardware
anywhere** — iron, civic-neutral.

**Boundary walls (city fabric):** read as building backs / district enclosure, Stratum 2–3, no
Court modification — backs are always the first to accumulate deferred work.

**Deferred to the rear-zone follow (greybox this pass):** CourtVendorHall and the Relic
Storehouse occupation histories, and the **R2 Stratum-2 tell** in the relic courtyard.

## 6. Spatial Risks the Produced-Art Pass Must Respect (level-designer)

- **M2 camp shares the Caretaker court** — produced M2-camp dressing (rest point, fire bowl,
  cart) and the Caretaker zone are visually adjacent; both must be §6.4-explicable and must not
  merge into a "prop cluster" that reads as performance.
- **Boundary walls** must be produced as city fabric (non-negotiable for place-read).
- **Flat ground plane** needs surface treatment implying drainage/wear history (§3.2/§6.4) —
  achievable via material, no geometry change.
- **The CaretakerHall entrance** must not center-frame Morrvik (CD-gate on the S5-03 art PR).
- New collision-bearing art geometry → requires a fresh NavMesh bake + soft-lock scan before
  the art is "art-ready" (the S3-05 soft-lock runner exists for this).

## 7. Fence Compliance (S5-00-06)

No atmosphere-as-warning, no routing fiction, no prop placed to direct the player, no
composition framing the objective, no guidance lighting. The world is indifferent and predates
the player. No new systems, no new factions, no faction-consequence mechanics — world fiction +
occupation history + spatial design only. Tier-1 holds; greybox remains the default outside this
produced beat.

## 8. Handoff to S5-01 (art-direction + manifest)

The art-director builds the asset manifest against this brief: the **4 material sets** (street
cobble, Caretaker-face Court ashlar, residential facade stone, timber trim) + the **practical
lighting** (civic-neutral lanterns + hearth glow, no light-for-the-player) + the **3–5 hero
props** (each tied to an occupation cause above), scoped to the **spawn → Caretaker beat**, with
Court vocabulary on the Caretaker office face only (R1). The CaretakerHall-entrance-not-framing
-Morrvik constraint and the boundary-walls-as-city-fabric requirement are binding inputs.

---

*Sources: the S5-00 world-builder + level-designer execution (2026-06-07), grounded in S3-05.1
verified geometry and the art-bible §1/§2/§3/§4/§5.2/§6 vocabulary; narrative-director canon
ratification (2026-06-07, RATIFY WITH REVISIONS); DECISIONS.md D021/D003. Generated 2026-06-07
for product-owner review.*
