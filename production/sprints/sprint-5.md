# Sprint 5 - Gravenspire T1: First District — Designed & Produced (First-Pass)

> **Authority:** `DECISIONS.md` D021 (First District produced-art pivot — amends D016),
> D020 (presentation-legibility), D003 (Tier-1).
> **Tier:** T1, offline single-player. No multiplayer, LLM, Save/Load (M4), Faction
> Consequence (M5), second district, or extra class.
> **Status:** Planning. Opened 2026-06-07 after Sprint 4 partial-close.

## Goal

Sprint 4 closed partial: the HUD-threshold spec (S4-00) landed, but the second
consecutive human-play attempt (S4-01 camera/HUD pass, Brian N=1, 2026-06-07) failed
demo-readiness — the slice still "doesn't resemble a playable demo." Per D021, greybox
is no longer an accepted feel-validation surface for this product; the fix is a
first-pass PRODUCED-art treatment of ONE representative area of the First District,
**designed as a place before it is produced.**

Sprint 5 makes the spawn → Caretaker Morrvik path the objective loop already walks read
as a real gothic place AND play legibly: produced materials + practical lighting +
dressing, with the S4-01 legibility floor (player-steered camera, debug-HUD isolation)
landed so the district is not viewed through a debug overlay. Everything else stays
greybox. The line D021 holds: produced art lives in the world register, practical-source
light only, no routing/hero-lighting/advertising — Pillar 1 + D020 readability survive
the fidelity jump.

## Gate State

- **Entry gate:** D021 locked (`6aaf128`, pushed to origin/main); Sprint 4 closed partial.
- **Exit gate:** re-targeted human-play (S5-05) — art-bible §7.12 EQ-legibility criteria
  + a new place-read criterion ("a first-time viewer reads the area as a gothic place,
  not a Unity prototype"); F1/F2/F3/F7 hollow-evidence fences intact; N=1 product-owner
  verdict, place-read AND state-read judged together, "first-pass" not "final-art."

## Milestone Structure

| Milestone | Name | Purpose | First Proof |
|---|---|---|---|
| Sprint 5 | First District — Designed & Produced (first-pass) | Make one representative area of the First District read as a real gothic place AND play legibly, designed before produced. | A first-time player launches the build and the spawn → Caretaker area reads as a specific cursed gothic place (not debug scaffolding), they can read their own state (S4-01 legibility floor), nothing routes them, and they answer "one more pull" for a world reason (re-targeted §7.12). |

### Deferred behind Sprint 5

| Milestone | Name | Status |
|---|---|---|
| HUD chain | S4-02/03/04 (vitals / target frame / cast bar) | Deferred (D021). S4-00 spec banked; built when the demo bar needs them. |
| M4 | Save/Load Flow | Deferred (D016). |
| M5 | Faction Consequence | Deferred (D016). |

## Story Ledger (dependency-ordered — design-first per D021)

| ID | Story | Type | Owner | Status | Depends |
|---|---|---|---|---|---|
| S5-00 | First District world/level design (representative area) | Design | level-designer + world-builder | Ready | — |
| S5-01 | Art-direction pass + bounded asset manifest | Design | art-director | Blocked (S5-00) | S5-00 |
| S5-02 | Perf-budget framework + asset-spec | Config/Data + Tech | technical-director + art-director | Blocked (S5-01) | S5-01 |
| S5-03 | Produced-art production (4 material sets + practical lighting + 3–5 hero props) | Visual/Feel | Codex / art | Blocked (S5-02) | S5-02 |
| S5-04 | Land S4-01 legibility floor (camera + debug-HUD isolation) | Integration | Codex | Ready | — (sequence vs S5-03) |
| S5-05 | Re-validate feel — re-targeted human-play gate | Integration + human-play | Brian | Blocked (S5-00..04) | S5-00..S5-04 |

Per-story scope boundaries cite D021's conditions + the art-bible section each executes.
`/create-stories` sets precise acceptance criteria.

- **S5-00 is the non-skippable trap-breaker:** the area gets place identity, route lock,
  faction identity, and a 2–3 sentence occupation-history per produced building (art-bible
  §6.4) before any asset is authored. Producing art for an undesigned place is forbidden.
- **S5-01 produces the asset manifest** — the hard scope cap.
- **S5-02 sets the four `TO BE CONFIGURED` perf budgets** against a named target-hardware
  tier; sequence author-one-sub-slice → profile → lock → scale.
- **S5-04 (S4-01 disposition):** carry the parked `codex/s4-01-play-camera-debug-hud-isolation`
  work, re-validate, land it **as-is** unless code-review surfaces changes. Minimum vitals
  readout + interaction prompt are optional levers decided here — not separate stories
  (avoid HUD scope creep in a world-art sprint).

## Bounded Scope (D021 anti-inflation lock)

- ONE representative area (spawn → Caretaker), first-pass fidelity, **not** final polish.
- 4 material sets (street cobble, primary facade stone, interior plaster, timber trim) +
  practical-source lighting (3–6 warms) + 3–5 hero props, capped by the S5-01 manifest.
- Anything beyond the manifest = `[SCOPE]` lesson + stop. `/scope-check` before close.
- Scope lever if still too big: shrink to the single first-encounter beat (spawn +
  Caretaker), fade the rest to greybox.

## Fences (binding — D021 / Pillar 1 / D020)

- **Practical-source light only** (§6.6): no light placed for the player's benefit without
  an in-world emitter; to darken, remove a source; to warm, add a fire.
- **No** guidance lighting, hero-lit objective doors, emissive/glowing interactables,
  rarity color, atmosphere-as-warning, or composition that frames the objective.
- §6.4 gate: every prop explainable by a person/activity; no asset before S5-00.
- Per-element State-Report-vs-World-Performance test at the **art** PR; creative-director
  gate on the produced-art PR.
- Tier 1 holds: produced art adds no systems.

## Risks

| ID | Risk | Owner | Mitigation |
|---|---|---|---|
| R1 | Undesigned-place trap (art for a place with no design intent → generic atmosphere, Pillar-1 violation, third fail) | Producer + CD + level-design | S5-00 non-skippable; §6.4 review gate; any manifest line without a design purpose → stop, return to S5-00 |
| R2 | Scope inflation ("produced art" goes open-ended) | Producer | Manifest cap; `/scope-check`; `[SCOPE]` lesson on creep; character art + full faction library explicitly OUT |
| R3 | Perf with unset budgets | Technical Director | S5-02 sets the four budgets vs target hardware; profile one sub-slice before scaling; over-budget → cut dressing, not framerate |
| R4 | Third consecutive feel-fail | Producer + Brian | S5-04 legibility rides along; S5-05 diagnoses which axis (world-read vs state-read) failed before re-scoping — don't blindly add more art |
| R5 | Scene corruption on `_DevEntry.unity` (S5-03 art + S5-04 camera both touch it) | Producer + implementers | Sequence, never concurrent; one scene edit/PR; no legacy-builder chaining (2026-05-30 lesson); Unity Smart Merge |
| R6 | D021 over-reversal (read as "greybox discipline is dead everywhere") | Producer | D021 scopes the reversal to the First District representative area only; greybox stays the project default elsewhere until explicitly promoted |

## Next Gate

1. **`/create-stories`** for the Sprint 5 slate (S5-00..S5-05) — precise ACs, owners,
   per-story D021/art-bible authority.
2. **`sprint-status.yaml` Sprint 4→5 reconciliation** — applied with the close.
3. **S5-00 (design-the-place) starts** — level-designer + world-builder (the non-skippable
   design gate before any produced asset).

## Definition Of Done For Sprint 5 Planning

- [x] Goal, authority (D021), exit gate (re-targeted §7.12 + place-read) stated.
- [x] Design-first dependency sequence (S5-00..S5-05) with per-story owner + scope shape.
- [x] Scope bounded (one area; manifest cap; scope lever).
- [x] S4-01 carried (S5-04); fences + risks named (R-COSPLAY-DRIFT lives in the art PR now).
- [ ] `/create-stories` run; story files written. *(Next gate.)*
- [ ] `sprint-status.yaml` Sprint 4→5 reconciliation applied. *(With the close.)*

---

*Sources: `DECISIONS.md` D021/D020/D016/D003; the four 2026-06-07 lead assessments
(creative-director, art-director, producer, technical-director); `design/art/art-bible.md`
§1/§2/§6/§8; `production/sprints/sprint-4.md`; `production/qa/evidence/s4-01-play-camera-evidence.md`
(the failed human-play verdict). Plan proposed 2026-06-07.*
