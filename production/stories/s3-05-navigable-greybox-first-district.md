# S3-05: Navigable Greybox First District

> **Sprint**: Sprint 3 — Playable Vertical-Slice Assembly
> **Sprint Plan**: `production/sprints/sprint-3.md` (Story Ledger row, line 70)
> **Status**: Ready (depends on S3-01 being Done)
> **Layer**: Core / Presentation
> **Type**: Integration (primary) + Visual/Feel (secondary evidence per `sprint-3.md:140`)
> **Estimate**: 1.5 days (LOW confidence — scene-authoring volume uncertainty; the largest single scene change in the sprint, per `sprint-3.md:159`)
> **Manifest Version**: Unavailable (control-manifest absent project-wide per `production/qa/plans/qa-plan-sprint-2-20260509.md:54,60`; documented fallback applies)
> **Generated**: 2026-05-23
> **Owner**: (unassigned — assign before commit per D016; must be design-aware owner — Brian or Codex, NOT Qwen3-Coder, per `sprint-3.md:166` — spatial design + Pillar 2 review judgment + scene-authoring volume make this de facto design-aware-required)

> **Pattern-establishing notice**: this is the first Sprint 3 story without a clean Sprint 2 precedent for evidence methodology. The four wiring stories (S3-01 through S3-04) inherited the Try*-contract + telemetry assertion pattern from S2-M2/S2-M3 closures. S3-05 introduces three new evidence artifacts — mechanical reachability check, soft-lock scan, Pillar-2 wayfinding-cue review — that have no prior precedent in this project. The shape these artifacts take in this story becomes the precedent for future district/zone work. Recorded for Sprint 3 close-out promotion to `tasks/lessons.md` as the "greybox spatial-validation evidence pattern."

## Context

**Sprint 3 plan**: `production/sprints/sprint-3.md`
**Quick-design source**: `design/quick/quick-design-m3-objective-npc-loot.md`
**Design-level navigation reference**: `design/gdd/world-structure.md` (Pillar 2 anti-pattern list at line 34: "no `now entering: the Blood District` banners, no ambient NPC lines triggered by proximity, no skybox swaps"; "Zone-as-theme-park-ride" rejected)
**Story Ledger row**: `production/sprints/sprint-3.md:70`
**Requirement IDs (story-local ACs)**: `S3-05-01` through `S3-05-12`

**Requirement Summary**: Replace the `FirstDistrict_ShellOnly_NoGameplay` shell (`Assets/Scenes/_DevEntry.unity:1729`) with a navigable greybox First District that hosts the three M3 anchors — `M3_Caretaker` (:881), `M3_ObjectiveRelic` (:1556), `M3_CourtVendor` (:382) — and supports walked traversal between them. Greybox-grade only: no produced art, no textures beyond solid-color greybox materials, uniform ambient lighting, blocky landmark massings. The district must be navigable through spatial reading (sightlines, landmark massing, layout legibility) without any wayfinding cues (no markers, no glow, no outlines, no minimap, no compass, no banners, no ambient NPC lines). The spawn-to-`M3_Caretaker` discoverability is the loop's entry point and is an explicit AC: from spawn position, the Caretaker is visible via sightline as one of 2-3 plausible landmarks (player has agency in what to investigate first). The M2 combat camp continues to function — M2 preservation reruns gate closure.

**Governing decisions** (DECISIONS.md):

| D-entry | Status | Usage |
|---|---|---|
| D001 (`DECISIONS.md:14`) | Locked | Unity 6.3 LTS + C# + URP — district uses URP materials, baked NavMesh |
| D003 (`DECISIONS.md:51`) | Locked | Tier 1 single-player offline — no zone streaming, no Addressables for district yet (single-scene `_DevEntry.unity`) |
| D012 (`DECISIONS.md:342`) | Locked | T1 combat-feel validated — combat camp inside the district must preserve feel |
| D016 (`DECISIONS.md:554`) | Locked | "Greybox, not art" presentation minimum — navigable + readable blockout massing + legible interaction feedback. **Retires `m2_presentation_threshold_gap` carryover** |

**Sprint 3 feedback rule** (`sprint-3.md:85`): every feedback element passes trigger + direction tests. For S3-05, the substantive design tension is the line between *spatial readability* (allowed — the player must be able to see and reason about the district) and *incidental wayfinding* (forbidden — geometry/lighting/framing that routes the player even without explicit markers). Four specific rejection criteria are locked in AC-08.

**Pillar 1 risk (R-P1-PROTAGONIST-DRIFT, `sprint-3.md:134`)**: the district must read as a place that *exists*, not a place built *for the player*. Multiple landmarks (not one focal-point), world-rules-not-player-service massing, world persists whether or not the player engages with it.

**Architecture Module**: World / First District (greybox scene authoring; new NavMesh; uses existing M3 anchor GameObjects)
**Engine**: Unity 6.3 LTS
**Engine Risk**: LOW-MEDIUM (NavMesh + AI Navigation package — well-established Unity APIs that pre-date 6.1; no URP custom passes, no UI Toolkit. MEDIUM bump for the unknown of whether the project's NavMesh setup exists — first navigable scene in the project)

**Surfaces reused** (do not re-author):
- Existing M3 anchors in `Assets/Scenes/_DevEntry.unity`: `M3_Caretaker` (:881), `M3_ObjectiveRelic` (:1556), `M3_CourtVendor` (:382). The anchors' GameObject references and components are preserved; their world positions may be moved to fit the district layout. M3 scripts reference them by GameObject reference (not world position), so moving them does not break the M3 dispatch chain.
- Existing M2 combat camp area in `_DevEntry.unity` — the M2 spawn, `M2_NamedBlocker`, trash spawn anchors must continue to function for M2 preservation reruns.
- Existing player marker (`ClericShellMarker`) — its spawn position becomes the district's "player spawn." The position may be moved; M2 locomotion at `M2SingleTrashMedLoopController.cs:564-583` operates relative to the marker, not absolute coordinates.
- S3-01 harness, S3-02/03/04 adapters — must continue to dispatch correctly after the district authoring. Each adapter's GameObject reference to its M3 component is preserved.

**Surfaces newly authored**:
- Greybox district geometry (blocky landmark massings + walkable floor) under a new `FirstDistrict_Greybox` GameObject (replacing the `FirstDistrict_ShellOnly_NoGameplay` marker at `:1729`)
- Uniform-ambient lighting setup
- Baked NavMesh covering the walkable district surface
- One or more solid-color greybox materials (e.g., `M_Greybox_Floor`, `M_Greybox_Wall`)
- Verification runners (Editor scripts) for mechanical reachability + soft-lock scan

## Acceptance Criteria

### District authoring

- [ ] **S3-05-01**: The `FirstDistrict_ShellOnly_NoGameplay` marker at `Assets/Scenes/_DevEntry.unity:1729` is replaced with a `FirstDistrict_Greybox` GameObject hierarchy containing the district geometry (blocky landmark massings + walkable floor + ambient light setup). The shell marker is removed; no orphaned references remain.
- [ ] **S3-05-02**: The three M3 anchors (`M3_Caretaker`, `M3_ObjectiveRelic`, `M3_CourtVendor`) remain in `_DevEntry.unity` with their existing GameObject IDs and component bindings preserved. Their world positions may be moved to fit the district layout; their script references and adapter bindings continue to function.
- [ ] **S3-05-03**: The M2 combat camp area (spawn, `M2_NamedBlocker`, trash spawn anchors) is preserved in a walkable region of the district — either alongside the M3 anchors (one shared space) or in a connected sub-area reachable from M3 anchor positions. M2 preservation reruns (T7) gate closure.
- [ ] **S3-05-04**: Player spawn position (`ClericShellMarker`) is set to a deliberate vantage point from which the player can begin spatial reading of the district — see S3-05-09 for the spawn-to-Caretaker discoverability constraint.

### Mechanical reachability

- [ ] **S3-05-05**: A baked NavMesh covers the walkable district surface, including walked paths between the player spawn and each M3 anchor and between the spawn and the M2 combat camp area. The NavMesh agent settings (radius, height, max slope, step height) are documented in the story's verification.md.
- [ ] **S3-05-06**: A new Editor batchmode runner `GravenspireS3FirstDistrictReachabilityRunner` proves: from player spawn position, NavMesh agent path-finds to each M3 anchor's world position. The runner records boolean reachable + computed path length + max elevation delta per anchor. Pass: all three anchors reachable. Fail: any anchor unreachable. (Relic `activeSelf` state: the runner force-activates the relic for the duration of the check so its position is queryable; restores prior state on exit.)
- [ ] **S3-05-07**: A new Editor batchmode runner `GravenspireS3FirstDistrictSoftLockScanRunner` grid-samples the walkable district surface at 1-meter spacing. For each sampled position, the runner places a NavMesh agent and attempts to path back to player spawn. Soft-lock zones = positions where placement succeeds but pathing back fails. Pass: zero soft-lock zones. Fail: any soft-lock zones → district has trap geometry that must be removed. **Methodology note (honest-coverage acknowledgement — §3 evidence rule applies)**: this scan is high-confidence for soft-locks accessible from grid-sampled positions but is **not exhaustive** — geometric edge cases requiring specific alignment to enter (narrow ledges the player can squeeze onto with no sampled grid point landing on them, mesh-gap pockets between meshes) can be missed by 1m sampling regardless of density. The scan catches the common case; the AC-11 walkthrough log provides complementary human-coverage of geometric edge cases the grid misses. Evidence claims in verification.md must frame this scan as **"best-effort high-confidence"**, not "exhaustive." Reporting it as a binary "no soft-locks exist" would overclaim what the methodology actually delivers.

### Pillar 2 — no wayfinding cues

- [ ] **S3-05-08**: The district contains zero wayfinding cues, with explicit rejection criteria for incidental wayfinding:

  **Reject (incidental wayfinding):**
  - Single-path layout from spawn to any M3 anchor (one plausible path = guided)
  - Focal-point lighting (any anchor in a localized "pool of light" while surroundings are darker)
  - Geometric focal-point framing (camera-from-spawn naturally frames any anchor as the visual center — e.g., a courtyard with the anchor dead-center, symmetric walls)
  - Anchor visual distinction (any anchor in a different color, scale, glow, animation, or particle than surrounding greybox geometry)

  **Allowed (spatial readability):**
  - Multiple plausible paths from spawn to each anchor (player chooses route)
  - Uniform-ambient lighting district-wide (no spotlights, no focal pools)
  - Anchors using the same greybox material palette as surrounding district geometry
  - Anchors visible via sightline from spawn as part of a broader landmark set (player chooses which to investigate first)

  Explicit absences: no quest markers, no minimap, no compass, no objective HUD pin, no overhead NPC name plate, no glowing relic, no spotlit vendor stall, no "now entering" banner, no ambient NPC line triggered by proximity, no skybox swap (`design/gdd/world-structure.md:34` Pillar 2 anti-pattern list applies).
- [ ] **S3-05-09**: **Spawn-to-Caretaker discoverability** (the loop's entry point — `sprint-3.md:70`): from player spawn position, `M3_Caretaker` is visible via at least one sightline AND is one of **2 to 3 plausible landmarks** visible from spawn, such that the player has agency in which to investigate first. NOT the only visible landmark (would be a guided tour without a marker); NOT zero visual cue (would be a greybox failure where the player wanders aimlessly). The reviewer verifies via screen capture from the spawn vantage point.

### Greybox-only presentation

- [ ] **S3-05-10**: District presentation is greybox-grade only:
  - Solid-color greybox materials only (no textures beyond uniform-color base maps)
  - Uniform ambient lighting only (no directional spotlights, no point lights, no light cones, no light cookies, no emissive surfaces beyond ambient)
  - Blocky geometric massings only (no rounded organic shapes, no detailed props, no foliage, no architectural decoration)
  - No produced art assets imported (no FBX meshes from external authoring tools, no PSD textures, no audio clips beyond what already exists)

  A source/asset scan (T6) enforces these as gates. The greybox material palette is documented in verification.md.

### Cross-cutting invariants

- [ ] **S3-05-11**: M2 combat camp preservation: all three M2 smokes (`M2SingleTrashLoop`, `M2LinkedTrashOverpull`, `M2NamedBlockerBoundary`) PASS at exit 0 after district authoring. Per `m2_melee_rng_not_reset`, each smoke runs in its own batchmode invocation.
- [ ] **S3-05-12**: M3 dispatch preservation: S3-01 harness, S3-02 NPC adapter (with S3-03 state-routing if S3-03 has landed), S3-03 relic adapter, and S3-04 vendor adapter all continue to dispatch correctly after the district authoring. A composite smoke runs the end-to-end objective loop (player walks spawn → Caretaker → interact → walk to relic → interact → walk to vendor → sell salvage → walk back to Caretaker → hand-in) and asserts all telemetry events fire as established in S3-02/03/04 vocabulary.

  **Closure semantics** (prevents orphaned partial-evidence): if S3-02/03/04 are not all closed at S3-05 implementation time, AC-12 partial-pass marks S3-05 as **"Done with Notes"** (precedent: S2-M3-04 closed "COMPLETE WITH NOTES" for deferred AC-06 human-play). Full AC-12 closure rolls forward into S3-06 as a pre-condition gate — S3-06's end-to-end runner subsumes the composite assertion and is the canonical place where the full chain is asserted. This is intentional: it preserves S3-06 as the slate's emotional payload and prevents AC-12 from becoming an orphaned partial-evidence artifact between S3-05 close and S3-06 implementation. The carryover key for this transfer (e.g., `s3_05_ac12_partial_rollforward_to_s3_06`) goes in `production/sprint-status.yaml` at S3-05 close-out if partial-pass applies.

## Implementation Notes

- **S3-01 dependency**: this story cannot start until S3-01 lands the harness — the AC-12 composite smoke requires the harness to dispatch. The district can be authored before S3-02/03/04 land (those wire adapters that hook into the harness); but the AC-12 composite smoke fully exercises only after S3-02/03/04 have all closed. Practical sequencing: implement district authoring + AC-05–10 first; defer AC-12's full composite assertion to the last possible point, or run AC-12 against whatever dispatch chain exists at the moment of S3-05 implementation (per AC-12 closure semantics — graceful degradation with explicit rollforward to S3-06).
- **NavMesh package**: the project does not currently have a baked NavMesh (no prior navigable scene). Adding the Unity AI Navigation package is part of this story's setup — verify package version against `docs/engine-reference/unity/` before adding; no speculative dependency (this is concrete, active-work-tied). If the package addition crosses into the Allowed Libraries list in `.claude/docs/technical-preferences.md`, that update is in-scope for this story.
- **Anchor positions**: the three M3 anchors' current positions in `_DevEntry.unity` are inside the `FirstDistrict_ShellOnly_NoGameplay` shell. Moving them to fit the new district layout is in-scope; the M3 scripts and adapters reference them by GameObject (not world coordinates), so moves are non-breaking. Document the new positions in verification.md for future reference.
- **Greybox palette**: keep to 2-3 distinct greybox material colors (e.g., a floor color, a wall/massing color, optionally a roof/ceiling color). This is enough for spatial reading without becoming a "color = wayfinding cue" trap. All anchors use the same greybox material as surrounding geometry (AC-08 reject criterion 4).
- **Soft-lock scan is high-confidence-but-not-exhaustive** (matches AC-07 methodology note): 1-meter spacing is a reasonable default and catches the common case. The runner documents its sampling density in the output for reproducibility. Finer sampling catches smaller traps but takes longer. **The methodology has known coverage gaps**: geometric edge cases requiring specific alignment to enter (narrow ledges the player can squeeze onto with no sampled grid point landing on them, mesh-gap pockets) can be missed by grid sampling regardless of density. The right granularity is "small enough to catch most traps a player can body-volume into" — not "exhaustive coverage." The AC-11 walkthrough log provides complementary human-coverage of these edge cases. Evidence claims must frame this scan as **"best-effort high-confidence"**, not "exhaustive" (§3 evidence rule honesty). This honest framing is itself part of the pattern-establishing precedent: future district/zone stories should inherit the same framing, not regress to overclaiming.
- **Reachability runner — relic activation**: `M3_ObjectiveRelic.activeSelf` is false until objective is Accepted (M3 system's `ApplyRelicAvailability` behavior). The reachability runner force-activates the relic for the duration of the check (so its position is queryable) and restores prior state on exit. This is a runner-only side effect, not a runtime change.
- **Telemetry-shape composite (AC-12)**: the composite end-to-end smoke must assert the full S3-02/03/04 telemetry vocabulary fires in the correct order (per S3-03-T7 and the established adapter contracts). If S3-02/03/04 have NOT closed at S3-05 implementation time, AC-12 closure semantics apply (partial-pass → "Done with Notes" → rollforward to S3-06); full AC-12 pass requires all downstream stories closed and is canonically asserted in S3-06's end-to-end runner.
- **Pattern-establishing precedent**: capture the verification.md shape, the reachability runner shape, the soft-lock scan runner shape (with honest-coverage framing), and the Pillar-2 wayfinding-cue review shape carefully. These become the template for any future district/zone story. A Sprint 3 close-out lesson promotion to `tasks/lessons.md` is planned ("greybox spatial-validation evidence pattern, see s3-05-*.md"). The NavMesh agent profile documented under AC-05 is a forward-thinking inheritance candidate for the same close-out promotion (canonical T1 agent profile until a future story revisits it).
- **No DateTime.UtcNow** in runner code; **scene discipline** (largest scene change in the sprint — save in Unity, inspect diff, no hand-edited YAML, one scene edit per PR per `.claude/rules/game-dev-governance.md`); **style gate** — same as all wiring stories.

## Out of Scope

- No produced art assets (no FBX meshes, no PSD textures, no models from external authoring; greybox-only per D016)
- No directional lighting, point lighting, light cones, light cookies, or emissive surfaces beyond uniform ambient (focal-point lighting is an AC-08 reject criterion)
- No NPCs beyond the existing `M3_Caretaker` (no ambient pedestrians, no faction-presence NPCs at greybox stage)
- No ambient NPC lines triggered by proximity (Pillar 2 anti-pattern, `world-structure.md:34`)
- No skybox swaps, no "now entering" banners, no zone transition VFX (Pillar 2 anti-pattern)
- No quest markers, minimap, compass, objective HUD pin, glow/outline on anchors, or any element advertising/locating/routing (Pillar 2, Sprint 3 feedback rule, AC-08)
- No second district, no extra-area expansion, no Addressables-based streaming (D003 single-scene; zone streaming is later/Tier-2+ work)
- No procedural generation; the district is hand-authored
- No day-night cycle, weather, or temporal variation (uniform ambient only)
- No combat encounters or hostile spawns inside the district beyond the existing M2 combat camp (which is preserved as a sub-area)
- No Save/Load of player position (M4 deferred; the player respawns at `ClericShellMarker` on Play Mode restart)
- No faction reaction to the player's presence in the district (M5 deferred)
- No human-play feel acceptance criterion on this story (sits only on S3-06 per `sprint-3.md:141`)

## QA Test Cases

### Test setup (shared)

Unity 6.3 LTS (`6000.3.14f1`) editor batchmode; `_DevEntry.unity` loaded; district authoring complete; baked NavMesh present; M3 anchors at their post-authoring positions; S3-01 harness wired; S3-02/03/04 adapters wired (or partial, per AC-12 graceful degradation).

### Integration tests (Unity batchmode runners)

**Test S3-05-T1: district replaces shell (AC-01, AC-02)**
- Given: `_DevEntry.unity` is loaded.
- When: scene-tree query for `FirstDistrict_ShellOnly_NoGameplay` and `FirstDistrict_Greybox`.
- Then: `FirstDistrict_ShellOnly_NoGameplay` is absent (or its marker GameObject is removed/renamed); `FirstDistrict_Greybox` is present with district geometry children; the three M3 anchor GameObjects (`M3_Caretaker`, `M3_ObjectiveRelic`, `M3_CourtVendor`) are present with their existing GameObject IDs.
- Edge cases: orphaned scene references that point to the removed shell → must be cleaned (no dangling Inspector references).

**Test S3-05-T2: mechanical reachability (AC-05, AC-06)**
- Given: baked NavMesh present; player spawn position recorded; M3 anchor positions recorded (with relic force-activated for the duration of the check).
- When: `GravenspireS3FirstDistrictReachabilityRunner` runs in batchmode.
- Then: for each of the three M3 anchors, NavMesh agent path-finds from spawn to anchor position; runner records `{anchorId, reachable: bool, pathLength: float, maxElevationDelta: float}`; all three `reachable == true`; relic state restored to prior on runner exit.
- Edge cases: NavMesh not baked → runner fails fast with clear error (don't pass silently); anchor outside NavMesh bounds → marked unreachable, fails the gate.

**Test S3-05-T3: soft-lock scan (AC-07)**
- Given: baked NavMesh present; district walkable surface bounds recorded.
- When: `GravenspireS3FirstDistrictSoftLockScanRunner` runs in batchmode; grid-samples walkable surface at 1m spacing.
- Then: for each sampled position, runner places NavMesh agent and attempts path back to spawn; soft-lock count = positions where placement succeeds but pathing back fails; pass = soft-lock count == 0. **Evidence claim must read as "best-effort high-confidence: zero soft-lock zones detected at 1m grid sampling density,"** not as "zero soft-lock zones exist" (the methodology has known coverage gaps per AC-07).
- Edge cases: sampled positions outside the walkable surface (above/below floor) → runner skips them (placement fails, not counted); sampled positions on a no-NavMesh area → skip.

### Manual evidence (BLOCKING for AC-08, AC-09; ADVISORY for AC-11 walkthrough)

**Test S3-05-T4: Pillar 2 wayfinding-cue review (AC-08, AC-09)** — BLOCKING manual review
- Reviewer (Brian or design-aware Codex; NOT auto-passable) inspects the district against the four reject criteria and four allowed patterns documented in AC-08. Verdict per criterion (PASS/FAIL with evidence note):
  - Single-path layout check: count plausible paths from spawn to each M3 anchor — pass if ≥ 2 per anchor; fail if exactly 1
  - Focal-point lighting check: visual inspection at runtime — pass if uniform ambient throughout; fail if any localized brighter region around an anchor
  - Geometric focal-point framing check: screen capture from spawn camera position — pass if no single anchor dominates visual center; fail if an anchor is the obvious visual focus
  - Anchor visual distinction check: visual inspection — pass if all anchors use the same greybox material palette as surrounding geometry; fail if any anchor is visually distinguished by color, scale, glow, animation, or particle
- AC-09 spawn-to-Caretaker discoverability: screen capture from spawn vantage point; reviewer confirms `M3_Caretaker` is visible via sightline AND is one of 2-3 plausible landmarks (not the only one, not invisible).
- Output: a reviewer checklist with PASS/FAIL per criterion + screen-capture artifacts, committed to verification.md.

**Test S3-05-T5: walkthrough log (AC-11)** — ADVISORY qualitative + complementary coverage for AC-07
- Playtester (lead) opens Play Mode; walks from spawn to each M3 anchor without using debug teleport, scene navigation, or any UI hint; logs:
  - Time-to-arrival per anchor
  - Whether the intended path was taken or the player got disoriented
  - Subjective notes on spatial readability
  - Any geometric edge cases encountered that the soft-lock scan didn't flag (narrow ledges, mesh-gap pockets, body-volume traps the player squeezed into) — this is the **complementary human-coverage** for AC-07's known scan coverage gaps
- Format: extends `tests/evidence/S2-M3-04/human-play-20260520.md` shape (what was attempted / what was found / classified limitations table / verbatim feedback). ADVISORY for the qualitative spatial-readability dimension; the geometric-edge-case observations feed back into the AC-07 evidence claim's honesty.

### Source/asset scan (BLOCKING for AC-10)

**Test S3-05-T6: greybox-only presentation scan (AC-10)** — source/asset gate
- Source scan over `Assets/Scenes/_DevEntry.unity` for any of: imported FBX mesh references, imported PSD/TGA/PNG texture references beyond greybox palette, light components other than ambient, emissive material references, audio source references beyond what already existed pre-S3-05.
- Asset scan over `Assets/Materials/` (or wherever the new greybox materials land) for any non-greybox material introduced by this story.
- Output: list of any matches (expected: zero) committed to verification.md.

### Composite smoke (AC-12)

**Test S3-05-T7: end-to-end objective loop in the district (AC-12)**
- Given: full S3-01/02/03/04 dispatch chain (or whatever portion is closed at S3-05 implementation time, per AC-12 closure semantics); fresh Play Mode session.
- When: scripted or human player input sequence drives spawn → Caretaker (interact) → relic (interact) → vendor (interact) → Caretaker (interact); each player movement is on-foot through the district (no teleport).
- Then: all telemetry events from S3-02/03/04 vocabulary fire in correct order (`npc_interaction_intentional`, `objective_accepted`, `relic_recovered`, `objective_loot_resolved`, `vendor_salvage_sold`, `vendor_sell_copper_applied`, `relic_handed_in`); objective state transitions NotIntroduced → Accepted → RelicRecovered → Complete; M2 combat camp adjacent and unaffected; final `state.State == Complete`, `vendor.CarriedCurrencyCopper > 0`.
- Graceful degradation: if S3-02/03/04 are not all closed at this story's implementation time, the composite smoke asserts only the portion of the chain that exists; AC-12 full pass requires all downstream stories closed (canonical assertion in S3-06; rollforward per AC-12 closure semantics).

### M2 preservation reruns (additional required evidence)

- `M2SingleTrashLoop` smoke — PASS, exit 0
- `M2LinkedTrashOverpull` smoke — PASS, exit 0
- `M2NamedBlockerBoundary` smoke — PASS, exit 0

District authoring is the largest scene change in the sprint; M2 preservation is the most important regression check. See "M2 preservation rerun execution note" below.

## Test Evidence

**Required evidence**: `tests/evidence/S3-05/verification.md`

Companion artifacts:
- `tests/evidence/S3-05/unity-first-district-reachability-[YYYYMMDD]-smoke.md` (S3-05-T2 batchmode runner output)
- `tests/evidence/S3-05/unity-first-district-soft-lock-scan-[YYYYMMDD]-smoke.md` (S3-05-T3 batchmode runner output; **evidence-claim language must use "best-effort high-confidence" framing per AC-07**)
- `tests/evidence/S3-05/pillar-2-wayfinding-review-[YYYYMMDD].md` (S3-05-T4 reviewer checklist with screen captures)
- `tests/evidence/S3-05/spawn-to-caretaker-discoverability-[YYYYMMDD].png` (S3-05-T4/AC-09 screen capture from spawn vantage)
- `tests/evidence/S3-05/walkthrough-log-[YYYYMMDD].md` (S3-05-T5 advisory walkthrough — also records any geometric edge cases the soft-lock scan missed)
- `tests/evidence/S3-05/greybox-presentation-scan-[YYYYMMDD].txt` (S3-05-T6 source/asset scan output)
- `tests/evidence/S3-05/unity-end-to-end-in-district-[YYYYMMDD]-smoke.md` (S3-05-T7 composite smoke — full pass or graceful-degradation partial per AC-12 closure semantics)
- `tests/evidence/S3-05/m2-02-preservation-[YYYYMMDD]-smoke.md`
- `tests/evidence/S3-05/m2-03-preservation-[YYYYMMDD]-smoke.md`
- `tests/evidence/S3-05/m2-04-preservation-[YYYYMMDD]-smoke.md`
- `dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — Combat regression baseline must hold (current: 189/189)
- T1 negative-scope scan over changed files — zero matches expected
- `git diff --check` — clean (scene file may carry Unity-serialized whitespace; trim before staging)
- `.githooks/pre-commit` — `[pre-commit] OK`
- `dotnet format --verify-no-changes` — PASS

**Evidence status**: Not started

**Greybox spatial-validation pattern documentation**: verification.md must include a `## Pattern Notes` section documenting the new evidence shape (reachability runner + soft-lock scan with best-effort-high-confidence framing + Pillar-2 review checklist + walkthrough log with geometric-edge-case coverage) for future district/zone stories. This is the pattern-establishing precedent flagged in the header.

**M2 preservation rerun execution note** (per `m2_melee_rng_not_reset`): three M2 preservation smokes cannot be chained in a single Unity batchmode invocation. Run each in its own `Unity.exe -batchmode -executeMethod ...` invocation with its own `-gravenspireEvidencePath` override. Pattern established in S2-M3-04 closure.

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| `S3-01` | AC-12 composite smoke requires the harness; harness's `ClericShellMarker` is the spawn position whose vantage drives AC-09 | Done |

S3-02, S3-03, S3-04 are NOT hard dependencies — district authoring can proceed before they close. AC-12 closure semantics define graceful degradation with rollforward to S3-06 for any partial pass at S3-05 close-out.

**Sprint-level pre-condition (tracked):** `dotnet format` setup — same as S3-01/S3-02/S3-03/S3-04.

## Blockers

S3-01 must close before this story enters `/dev-story` (AC-12 dependency). No design blockers; all governing D-entries Locked. The pattern-establishing nature means the shape of the verification artifacts is itself part of the deliverable — reviewers should evaluate the artifact shapes for reusability in addition to the per-AC verdicts.

Watch items (not blockers):
- `m2_melee_rng_not_reset` — three M2 smokes require separate invocations
- `m2_02_runner_date_hardcoded` — no new hardcoded dates in any of the two new runners (reachability + soft-lock scan)
- `m2_renderer_material_property_access` — if greybox materials touch renderer state on Update, follow the `MaterialPropertyBlock` lesson (unlikely for static district geometry)
- `control_manifest_absence_pre_existing` — Manifest Version `Unavailable` per fallback
- Format Gate — see Dependencies
- **NavMesh package addition**: if Unity AI Navigation package is added to `Packages/manifest.json`, that's a dependency change requiring `.claude/rules/game-dev-governance.md` Dependency Discipline. Tie active work to this story explicitly in any dependency-request record.
- **AC-12 rollforward**: if S3-05 closes "Done with Notes" on AC-12 (per closure semantics), the carryover key (e.g., `s3_05_ac12_partial_rollforward_to_s3_06`) must be added to `production/sprint-status.yaml` so S3-06 inherits the full-chain assertion responsibility explicitly.
- **Pattern-establishing closure follow-up**: at Sprint 3 close-out, promote (a) the greybox spatial-validation evidence pattern and (b) the NavMesh agent profile as canonical T1 reference to `tasks/lessons.md` so future district/zone stories inherit the shape rather than re-deriving them. Bundle with the existing `feedback_external_review_verification` promotion already pending.
- **Pillar 1 framing watch**: the district must read as a place that exists, not a place built for the player. Reviewer evaluates AC-08's four reject criteria with R-P1-PROTAGONIST-DRIFT in mind — focal-point framing is both a Pillar 2 wayfinding hazard AND a Pillar 1 "world-built-for-you" hazard.
