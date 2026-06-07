# S4-05: First District Atmosphere + Legibility Pass (Bounded)

> **Sprint**: Sprint 4 — EQ-Readable Presentation Slice
> **Sprint Plan**: `production/sprints/sprint-4.md` (Story Ledger, S4-05)
> **Status**: Ready
> **Layer**: Presentation
> **Type**: Visual/Feel
> **Estimate**: 1.5 days
> **Manifest Version**: Unavailable (control-manifest absent project-wide; documented fallback applies)
> **Generated**: 2026-06-07
> **Owner**: Codex

## Context

**Authority**: `DECISIONS.md` D020 + D016 (greybox presentation minimum); revised art
bible §1 (weight-and-age), §2 (mood/atmosphere — practical light only), §6
(environment design language).

**Requirement Summary**: Make the navigable greybox First District (built in S3-05)
**read as a gothic place**, not Unity greybox/debug scaffolding — through practical-source
lighting, massing/sightline readability, and placeholder material language. This story
is **BOUNDED** (user decision): greybox-grade only — no produced art palette, no final
textures. It also records the **[F1] artifact-identity tuple** (scene + NavMesh +
bake-scope SHAs) so the S4-06 human-play gate can prove the played build is the
authored scene.

**Governing decisions**:

| D-entry | Status | Usage |
|---|---|---|
| D020 | Locked | EQ-readability pivot; the "reads as a place" this executes |
| D016 | Locked | Greybox, NOT produced art — the bound on this story |

**Art-bible authority**: §1 (weight-and-age, not spectacle — every element earns its
place); §2 (mood/atmosphere; practical light only — no atmospheric mood-light placed
for player benefit, §6.6); §6 (environment language); §7.11 (no World Performance — the
scene must not route or warn the player).

**Surfaces reused** (do not re-author): the S3-05 navigable district geometry +
NavMesh in `_DevEntry.unity`. This story lights and dresses it to greybox-place
fidelity; it does not rebuild the district.

**Engine**: Unity 6.3 LTS, URP. **Engine Risk**: MEDIUM (URP lighting; Unity batchmode
reserializes ProjectSettings — the 2026-05-26 lesson; verify URP lighting APIs against
`docs/engine-reference/unity/`).

## Acceptance Criteria

- [ ] **S4-05-01**: The district reads as a **gothic place** through **practical-source lighting** (no atmospheric mood-light placed for player benefit without an in-world source, §6.6), **massing/sightline readability**, and **placeholder material language** (material *vocabulary* per faction/zone, not finished textures).
- [ ] **S4-05-02 (BOUND)**: Greybox-grade only. **No produced art palette, no final textures** (D016). Material language is placeholder/vocabulary-level. If the work starts trending toward produced art, it is out of scope and stops.
- [ ] **S4-05-03**: The no-routing fence holds (§7.11, Pillar 2): **no glowing doors, objective markers, minimap pins, atmosphere-as-warning, or guidance lighting**. Spatial readability is sightlines + massing + layout legibility — never objective signposting. Spawn-to-`M3_Caretaker` discoverability (inherited from S3-05) is by spatial readability, not a marker.
- [ ] **S4-05-04 [F1]**: The **artifact-identity tuple** is recorded as evidence: `_DevEntry.unity` content SHA, the First District NavMesh asset SHA + size, and (if a bake-scope artifact exists) its SHA. This tuple is the freshness anchor the S4-06 gate compares against, so downstream evidence cannot cite a stale scene/NavMesh. (Closes the 2026-06-07 review finding F1 — the "artifact exists but the model under test drifted" gap.)
- [ ] **S4-05-05**: The scene edit is **adapter/additive**; no legacy builder is chained over the authored district (2026-05-30 builder-chaining lesson). Unity ProjectSettings/Packages drift from batchmode lighting work is **restored** unless explicitly in scope (2026-05-26 lesson); the diff ships only the authored lighting/material delta.

## Implementation Notes

- Lighting is practical-source: a torch lights because there is a torch, not because the player needs the space brighter (§2 / §6.6). To darken, remove sources; to warm, add a fire.
- Record the F1 tuple with concrete commands (e.g. `git hash-object _DevEntry.unity`, the NavMesh asset SHA/size) so S4-06 can re-run the same commands and compare.
- Scene discipline: save-then-diff; one scene edit per PR; never hand-edit YAML; sequence against S4-01 (also scene-touching) — do not run concurrently. Restore any ProjectSettings drift.
- Verify URP lighting APIs against `docs/engine-reference/unity/` (post-6.0 URP UNVERIFIED).

## Out of Scope

- Produced art / final textures / finished palette (the bound).
- New district geometry or NavMesh rebake beyond what lighting/dressing requires (reuse S3-05).
- HUD work (S4-01..04).
- Any guidance/routing/marker element (forbidden).

## QA Test Cases

**Manual check (S4-05-01/03 reads-as-place + no-routing)**
- Setup: walk the district from spawn through the loop path.
- Verify: it reads as a gothic place (lighting, massing, material vocabulary); spawn-to-Caretaker is discoverable by sightline/massing; no glowing doors / markers / guidance lighting anywhere.
- Pass: place-read achieved at greybox fidelity; zero routing elements.

**Integration check (S4-05-04 [F1] tuple)**
- Setup: post-implementation, record the scene/NavMesh/bake-scope SHAs.
- Verify: the tuple is in the evidence file with the exact commands to reproduce it.
- Pass: a reader can re-run the commands and get the same tuple; S4-06 can compare.

## Test Evidence

**Required evidence**: `production/qa/evidence/s4-05-district-atmosphere-evidence.md`
(walkthrough screenshots; practical-lighting confirmation; the F1 artifact-identity
tuple with reproduction commands; adapter-only scene-diff + ProjectSettings-restore
confirmation).

**Evidence status**: Not started

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| `S3-05` | The navigable greybox district + NavMesh this story lights and dresses | Done (with notes) |

## Blockers

None. S3-05 (the district) is done. Sequence against S4-01 for scene safety (both
touch `_DevEntry.unity`).
