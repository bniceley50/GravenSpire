# QA Plan - Sprint 4 EQ-Readable Presentation Slice

**Date:** 2026-06-07
**Invocation:** `/qa-plan sprint-4`
**Scope:** Sprint 4 EQ-Readable Presentation Slice: seven stories, `S4-00` through `S4-06`.
**Input source:** `production/sprints/sprint-4.md` and the S4-00..S4-06 story files at repository HEAD `e5428e0d94debcd9cb1550c75876a5a09f4442d5`.
**Sprint status source:** `production/sprint-status.yaml` (sprint `4`, head field `02de35d`; actual repo HEAD `e5428e0`).
**Authority:** `DECISIONS.md` D020 (EQ-readability pivot, Locked); revised `design/art/art-bible.md` §7.10 / §7.11 / §7.12.
**Confidence:** High for test classification, evidence-gate levels, and the §7.12 exit-criteria mapping; medium for exact Unity batchmode readability-runner implementation until each presentation story lands its evidence scaffold.

## QA Scope

Sprint 4 adds **no new game systems**. It executes the completed D020 art-bible revision — the presentation/legibility layer over the already-assembled, runner-proven Tier-1 objective loop (`production/sprints/sprint-4.md:20` through `:31`). The milestone exists because the S3-06 N=1 human-play attempt (2026-05-30) FAILED the feel gate for presentation-readability reasons: the slice read as Unity greybox/debug scaffolding, and the player could not reliably read their combat state, target, cast, or interaction.

The plan covers all seven stories:

| Story | Type | Gate Level | Automated Evidence Required | Manual / Human Verification Required |
| --- | --- | --- | --- | --- |
| `S4-00` UX HUD threshold + con-glyph pass | Config/Data (Design — spec only, no code) | ADVISORY | None (no code). Con-glyph colorblind-simulation discriminability artifact. | Spec review: every number concrete or a labelled open question; 5/5 con-glyph shape discriminability with color disabled. |
| `S4-01` Play camera + debug-HUD isolation | Integration (scene-touching) | BLOCKING | Scene-diff adapter-only proof; M2 preservation reruns; ProjectSettings-restore check. | Manual: player-steered camera with zero auto-pull-toward-POI; M2 debug HUD absent from objective-play view. |
| `S4-02` Layer 1 vitals HUD | UI | ADVISORY | Batchmode HUD-render/readability capture where automatable; §7.11 forbidden-treatment scan. | Manual walkthrough: combat-state read (§7.12-1); screenshots at full/low health. |
| `S4-03` Target frame + con indicator | UI | ADVISORY | Selection-gating telemetry where automatable; §7.11 scan. | Manual: frame on select / absent on deselect / absent on un-targeted entities; 5 con states color-disabled. |
| `S4-04` Cast bar + interaction prompt | UI | ADVISORY | Cast-bar placement (clears 40-60% band) check; §7.11 scan. | Manual: cast-state read (§7.12-4); interaction prompt in-range/facing vs out-of-range/across-room. |
| `S4-05` First District atmosphere + legibility | Visual/Feel (scene-touching) | ADVISORY (with one BLOCKING integration item) | **[F1]** artifact-identity tuple (scene + NavMesh + bake-scope SHAs) with reproduction commands; scene-diff adapter-only; ProjectSettings-restore; M2 preservation reruns. | Manual: reads-as-gothic-place at greybox fidelity; zero routing/guidance elements. |
| `S4-06` EQ-readable human-play gate | Integration + human-play | BLOCKING | **[F1]** tuple match, **[F2]** real walked traversal, **[F3]** real `Input.GetKeyDown` path, **[F7]** exact-sequence telemetry. | **Binding N=1 human-play feel verdict** against the six §7.12 criteria; "reads/plays" judged separately from "final-art pretty". |

## Source List

Verification method: live repository reads on 2026-06-07 (`Read`, `Grep`, `git rev-parse`) at HEAD `e5428e0`.

| Source | Use |
| --- | --- |
| `production/sprints/sprint-4.md:10` through `:31` | Sprint 4 goal, the "EQ readability, not EQ cosplay" line, and the no-routing fence. |
| `production/sprints/sprint-4.md:58` through `:87` | Story Ledger S4-00..S4-06: type, owner, status, per-story art-bible authority, scope boundary, dependency shape. |
| `production/sprints/sprint-4.md:89` through `:107` | Operating-model calibration: two registers, State-Report vs World-Performance test, recessive-but-legible. |
| `production/sprints/sprint-4.md:134` through `:142` | Risk table — R-COSPLAY-DRIFT (central), scene fragility, deferred-numbers slip, R-P2-FEEL-MISATTRIBUTION, scope inflation, Tier-1 discipline. |
| `production/sprints/sprint-4.md:144` through `:160` | QA Plan Hooks: story-type split, pre-written §7.12 exit criteria, greybox-honest protocol, con-glyph accessibility. |
| `production/stories/s4-00-ux-hud-thresholds-and-con-glyph.md:39` through `:46` | S4-00 acceptance criteria: bar height, panel-fill opacity floor, cast-bar placement, 5-state con-glyph, evidence-honesty, recessive floor. |
| `production/stories/s4-01-play-camera-and-debug-hud-isolation.md:47` through `:53` | S4-01 AC: player-steered camera (no POI pull), debug-HUD isolation, no-locomotion-rebuild, adapter-only scene diff, CLIENT-LOCAL annotation. |
| `production/stories/s4-02-layer-1-vitals-hud.md:46` through `:53` | S4-02 AC: Iron Seam vitals at S4-00 floor, recessive-but-legible, §7.11 forbidden list, snap-to-value, HUD coherence, State-Report-only. |
| `production/stories/s4-03-target-frame-and-con-indicator.md:46` through `:53` | S4-03 AC: selection-gated frame, name/health/faction-frame, 5-state shape-primary con, never-on-unselected, recessive line-glyph, §7.12-2. |
| `production/stories/s4-04-cast-bar-and-interaction-prompt.md:47` through `:54` | S4-04 AC: linear cast bar clearing the band, no flourish, range+facing interaction prompt, screen-space single-target, §7.12-3/4, no routing. |
| `production/stories/s4-05-first-district-atmosphere-and-legibility.md:48` through `:54` | S4-05 AC: reads-as-place via practical light/massing/material, greybox bound, no-routing fence, **[F1]** artifact-identity tuple, adapter-only diff. |
| `production/stories/s4-06-eq-readable-human-play-gate.md:46` through `:53` | S4-06 AC: six §7.12 criteria, **[F2]** walked traversal, **[F3]** real input path, **[F7]** exact telemetry, **[F1]** artifact match, N=1 feel verdict. |
| `design/art/art-bible.md:1490` through `:1503` | §7.10 Testable Thresholds — the deferred HUD numbers S4-00 resolves (combat-readability floor, target/con/cast/prompt readability). |
| `design/art/art-bible.md:1505` through `:1560` | §7.11 Forbidden list + the State-Report boundary (the enforceable R-COSPLAY-DRIFT fence). |
| `design/art/art-bible.md:1562` through `:1581` | §7.12 EQ-Legibility Acceptance Criteria — the six pre-written exit tests. |
| `.claude/docs/coding-standards.md` (Test Evidence by Story Type) | Gate levels: Logic/Integration BLOCKING; Visual/Feel + UI ADVISORY; Config/Data smoke ADVISORY. |
| `.claude/rules/game-dev-governance.md` (Scene Discipline; Code Style Gate; Tier Discipline) | One scene edit per PR; save-then-diff; no hand-edit YAML; Smart Merge; `dotnet format --verify-no-changes`; no Tier N+1. |
| `production/qa/plans/qa-plan-sprint-3-20260524.md` | Format precedent + regression-gate shape (RG-00..RG-06) carried forward. |
| `production/qa/plans/qa-plan-sprint-2-20260509.md:54,:60` | Control-manifest-absent fallback: forbidden-pattern QA uses `docs/registry/architecture.yaml`. |
| `docs/engine-reference/unity/VERSION.md` | Unity 6.3 LTS pin; post-6.0 UI Toolkit / URP / camera APIs UNVERIFIED until referenced. |

## Live-State Corrections

- **S4 slate not yet reconciled into `sprint-status.yaml`.** The `stories:` array still lists the S3 slate (`production/sprint-status.yaml:79-148`); `total_stories`/`completed_stories` are `0`. The Sprint 3→4 story reconciliation is a **separate explicit governance write** (`production/sprints/sprint-4.md:191`, Next Gate item 2) and is **out of this plan's scope**. This QA plan records only the `qa_plan:` pointer; it does not author or modify story rows.
- **S4-00 is a hard precondition for the HUD numbers.** S4-02/03/04 are Blocked on S4-00 and must build to its validated values; a guessed HUD number is the S3-06 failure repeating (`production/stories/s4-02-layer-1-vitals-hud.md:56`). QA treats any "TBD reached an implementation story" as a blocking evidence-honesty failure.
- **Control manifest still absent.** S4 stories carry `Manifest Version: Unavailable`; architecture forbidden-pattern QA uses the documented `docs/registry/architecture.yaml` fallback (Sprint 2 precedent). Not a Sprint 4 blocker.
- **Format gate active.** `.githooks/pre-commit` runs `dotnet format --verify-no-changes` with the documented IDE1006 exclusion (pre-existing naming debt). Treat the style gate as required evidence, not a setup blocker.
- **Working tree carries unrelated dirty files** (`M3*Session.cs`, combat/npc tests, `art-director.md`, untracked patches/worktrees). This plan neither classifies nor approves them; each story's verification must inspect only its own implementation diff.

## Classification Summary

| Story | Primary Type | Why | Closure-Gate Evidence |
| --- | --- | --- | --- |
| S4-00 | Config/Data (Design) | Spec/validation only, no runtime code. | Spec doc + con-glyph colorblind discriminability proof (ADVISORY). |
| S4-01 | Integration | Crosses Unity scene, camera, URP overlay, and existing M2 debug HUD. | Adapter-only scene diff + M2 preservation + manual camera/HUD checks (BLOCKING). |
| S4-02 / S4-03 / S4-04 | UI | Layer 1 HUD rendering + selection/cast/interaction presentation. | Manual walkthrough + §7.11 forbidden-treatment scan + §7.12 read tests (ADVISORY). |
| S4-05 | Visual/Feel | Greybox lighting/massing/material — a "reads as a place" judgment. | Walkthrough + screenshots + **[F1]** tuple (the tuple item is BLOCKING integration evidence). |
| S4-06 | Integration + human-play | The composition + milestone exit gate. | Four hollow-evidence fences (BLOCKING) + the binding N=1 human-play feel verdict. |

**Human-play rule (inherited from S2-M3-04 / S3-06).** Only **S4-06** carries a binding human-play feel AC. S4-00..S4-05 may be played during implementation, but their closure gates are spec / integration / manual-walkthrough evidence. The S4-06 protocol MUST separate "does it read/play as the game" (the real question) from "is it final-art pretty" (out of scope; greybox acceptable per D016) — the R-P2-FEEL-MISATTRIBUTION rule.

## Risk Matrix

| ID | Risk | Prob | Impact | QA Control |
| --- | --- | --- | --- | --- |
| Q-R1 | **R-COSPLAY-DRIFT — interface crosses into World Performance** (minimap "to help the tester", objective arrow, loot glow, con rendered as a loud color badge, across-room interaction prompt, glowing objective door). The central risk of the milestone. | High | High | The §7.11 State-Report boundary is the enforceable fence; §7.12 criterion 6 (no routing) is the exit test. Every HUD/scene element runs the two-question test (`sprint-4.md:100-102`): *who initiated this?* / *what must the world do for this signal to appear?* If either answer points at the world, REJECT. Per-story State-Report scan (QA-SCAN-1 below). |
| Q-R2 | **Deferred-numbers slip** — S4-02/03/04 start before S4-00 returns validated numbers and guess. | Med | High | S4-00 is the gating first story. QA blocks any HUD story whose evidence cites a number not traceable to the S4-00 spec, or that records a number as "TBD". An S4-00 open question must carry an explicit validation method, not a guessed value. |
| Q-R3 | **Scene fragility on `_DevEntry.unity`** — S4-01 and S4-05 both touch the shared scene; concurrent edits risk merge corruption. | Med | High | Sequence S4-01 and S4-05 (do not run concurrently). Scene Discipline gate (RG-S): save-then-diff, one scene edit per PR, no hand-edit YAML, Unity Smart Merge, adapter-only/additive, ProjectSettings drift restored (2026-05-26 lesson), never chain a legacy builder over the authored scene (2026-05-30 lesson). |
| Q-R4 | **Missing-art misread as feel-fail (R-P2-FEEL-MISATTRIBUTION)** — S4-06 fails because the district lacks produced art when the real question is "does it read/play". | Med | Med | S4-06 protocol explicitly splits loop/legibility feel from art fidelity. A greybox-but-readable slice that pulls the player back is a PASS; a polished-but-unreadable slice is not. |
| Q-R5 | **Hollow / adapter-chain evidence at the exit gate** — S4-06 passes on teleport-not-traversal, direct-dispatch-not-input, subsequence-not-exact telemetry, or a drifted scene. | Med | High | The four fences are hard, BLOCKING ACs: **[F1]** artifact-identity match, **[F2]** real walked traversal, **[F3]** `Input.GetKeyDown` path, **[F7]** exact-sequence telemetry. The gate cannot pass on any bypassed fence. |
| Q-R6 | **Scope inflation** — "make it read as the game" expands S4-05 and the HUD stories toward unbounded polish. | Med | High | S4-05 is BOUNDED (greybox lighting + massing + placeholder material only). HUD stories build to the S4-00 floor, not "as nice as possible". `/scope-check` before each story closes; creep recorded as a `[SCOPE]` lesson. The exit gate is §7.12 readability, not visual fidelity. |
| Q-R7 | **Tier-1 / no-new-systems slip** — a "small" new system (settings menu, minimap toggle, HUD-layout save). | Med | High | T1 negative-scope scan per story (RG-04). A minimap is doubly forbidden (Tier-creep AND §7.11 World-Performance). No Save/Load hook, no faction wiring. |
| Q-R8 | **Unverified post-6.0 Unity API** — UI Toolkit (`VisualElement.transform` deprecated), URP overlay/camera-stack, URP lighting used without verification. | Med | Med | Verify every post-6.0 UI Toolkit / URP / camera API against `docs/engine-reference/unity/` before use; mark UNVERIFIED otherwise (game-dev-governance Engine Version Awareness). |
| Q-R9 | **Con-glyph fails colorblind discriminability** — the 5 states rely on color and collapse under §4.6 simulation. | Low | Med | S4-00-04 validates shape-alone discriminability with color disabled (deuteranopia/protanopia/tritanopia). A colorblind player must lose nothing. Blocking for S4-03 (which consumes the shape set). |

## Regression Gates

| Gate | Timing | Command / Method | Pass Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| RG-00 Combat/NPC baseline | Before each closure of any story touching shared runtime | `dotnet test` on the Combat test project | Suite passes (count expected ~189; legitimate discovery-count changes recorded, not assumed) | Story `tests/evidence/S4-NN/verification.md` |
| RG-S Scene Discipline | Any story touching `_DevEntry.unity` (S4-01, S4-05; S4-02/03/04 if HUD lands in-scene) | Save in Unity → `git diff` inspect → stage; Smart Merge for conflicts | Adapter-only/additive delta; no hand-edited YAML; no legacy-builder rebuild artifacts; ProjectSettings/Packages drift restored | Story evidence + `verification.md` Scene Change Review record |
| RG-02 M2 preservation | Any story touching `_DevEntry.unity` or shared M2 surfaces | Run `M2SingleTrashLoop`, `M2LinkedTrashOverpull`, `M2NamedBlockerBoundary` in separate batchmode invocations | All three pass, exit 0, story-local evidence-path overrides; `Builder Invoked: false` where asserted | `tests/evidence/S4-NN/m2-0X-preservation-[YYYYMMDD]-smoke.md` |
| RG-04 T1 negative-scope scan | Before story closure | Explicit scan over changed files (`Assets/Scripts`, `Assets/Editor`, `_DevEntry.unity`, `Packages`, `src`, `tests`, evidence) | No real implementation hits for Tier 2+ / new-system / Save-Load / faction surfaces; documentation self-hits classified | Story `verification.md` |
| RG-05 Style / local gate | Before PR / commit | `git diff --check`; `.githooks/pre-commit`; `dotnet format --verify-no-changes` (IDE1006 excluded, documented) | Diff hygiene clean; hook `[pre-commit] OK`; format gate passes | Story `verification.md` |
| RG-06 Review-subagent gate | Each `/code-review` | Standing reviewer + `qa-tester`; add `unity-specialist` for S4-01/S4-05/S4-06; `unity-ui-specialist` for S4-02/03/04; `ux-designer` consulted for S4-00 | Findings addressed or explicitly deferred with severity/rationale | Story review notes / `verification.md` |

**QA scan specific to this milestone:**

- **QA-SCAN-1 (State-Report boundary, §7.11).** For every HUD/scene element in S4-01..S4-05, confirm against the §7.11 forbidden list: no frame/health/con on an un-selected entity; no interaction prompt before range+facing or naming more than one target; no persistent overhead nameplates; no loot glow / rarity color; no minimap / quest arrow / objective pin / waypoint / "go here" routing; con is a recessive shape-primary glyph, not a loud color badge; no glow/bloom/gradient/rounded-corner/drop-shadow/true-black/true-white/red/green; no UI in the 40-60% viewport-height band except the cast bar during an active cast. This scan is BLOCKING for the §7.12-6 (no routing) exit criterion.

## §7.12 Exit-Criteria Traceability

The exit criteria are pre-written in `design/art/art-bible.md:1562-1581`. S4-06 verifies all six; each maps to the story that implements it:

| §7.12 Criterion | Implementing Story | Verified-At-Exit By |
| --- | --- | --- |
| 1. Combat-state read (health legible ~10s into a pull without studying) | S4-02 | S4-02-02 manual + S4-06-01 human-play |
| 2. Target identification (name + rough threat ≤1s of selection) | S4-03 | S4-03-06 manual + S4-06-01 |
| 3. Interaction confirmation (prompt named target before trigger; result legible) | S4-04 | S4-04-05 manual + S4-06-01 |
| 4. Cast-state read (cast bar peripheral-readable while monitoring health) | S4-04 | S4-04-05 manual + S4-06-01 |
| 5. HUD coherence (Layer 1 shares the city material vocabulary) | S4-02 / S4-03 / S4-04 | S4-02-05 + S4-06-01 |
| 6. No routing (nothing tells the player where to go / what to value / what to feel) | S4-01 (camera), S4-03 (con), S4-04 (prompt), S4-05 (scene) | QA-SCAN-1 + S4-06-01 |

## Story Test Plans

### S4-00 — UX HUD Threshold + Con-Glyph Pass (Design / ADVISORY)

- **Automated:** none (no code).
- **Manual / spec validation:**
  - Con-glyph discriminability: render the 5 states (Trivial/Below/Even/Above/Dangerous) at HUD scale with color disabled (deuteranopia/protanopia/tritanopia simulation, §4.6). **Pass: 5/5 nameable by shape alone; no two ambiguous.**
  - Numbers concreteness: minimum bar height, panel-fill opacity floor, and cast-bar lower-center placement (clears the 40-60% band) are each a concrete value or a labelled open question **with a validation method** — never "TBD". The replaced baselines (3px bar, 45% fill) are recorded as no-longer-accepted.
  - Recessive-floor check: the spec sets a legibility *floor*, not a maximum-visibility target (§7.1 doctrine).
- **Evidence:** `production/qa/evidence/s4-00-hud-thresholds-evidence.md` (resolved numbers + con-glyph shape set + colorblind proof + any open questions with methods).
- **Blocks:** S4-02, S4-03, S4-04.

### S4-01 — Play Camera + Debug-HUD Isolation (Integration / BLOCKING)

- **Manual (camera):** enter Play Mode in `_DevEntry.unity`, walk the district. **Pass: readable third-person framing, player-steered; zero auto-pull toward NPC/relic/vendor/POI** (camera-pull-toward-POI is World Performance — REJECT).
- **Manual (debug-HUD isolation):** enter non-combat objective-play. **Pass: S3 interaction prompt/feedback visible; the legacy M2 combat-debug HUD does not bleed over the view.**
- **Integration (scene delta):** inspect `git diff` of `_DevEntry.unity`. **Pass: camera/overlay delta only; no legacy-builder rebuild artifacts; no ProjectSettings drift shipped.** (RG-S, RG-02.)
- **No-locomotion-rebuild:** the S3-01 harness mover is reused; a structural mover rebuild is a red flag → escalate (S4-01-03).
- **Annotation:** camera/overlay change carries the one-line `CLIENT-LOCAL` annotation per D017 (presentation, single-player-local, not a state-mutating seam).
- **Evidence:** `production/qa/evidence/s4-01-play-camera-evidence.md`.
- **Sequence:** before/after S4-05, never concurrent (shared scene).

### S4-02 — Layer 1 Vitals HUD (UI / ADVISORY)

- **Manual (combat-state read):** enter a pull; take damage to ~50% then <20%. **Pass: health readable at a peripheral glance throughout (§7.12-1); the <20% death-approach pulse is low-amplitude 0.7Hz, felt-not-distracting.**
- **Manual (§7.11 forbidden treatments):** inspect rendered HUD + implementation. **Pass: zero glow/bloom/emission, gradient, rounded corners, drop shadow, true-black/true-white, red/green signaling.** (QA-SCAN-1.)
- **Snap-to-value:** bars snap (§7.9); only permitted animation is the death-approach pulse and the linear med-break mana fill. No tween-for-feel.
- **Builds to S4-00 floor:** the bar-height and panel-fill values cited in evidence trace to the S4-00 spec; a guessed value is a blocking evidence-honesty failure (Q-R2).
- **Evidence:** `production/qa/evidence/s4-02-vitals-hud-evidence.md` (full/low-health screenshots + §7.11 compliance + S4-00 values used).

### S4-03 — Target Frame + Con Indicator (UI / ADVISORY)

- **Manual (selection-gating):** select a target, then deselect. **Pass: frame appears on select, vanishes on deselect/target-death; no frame/con on any un-selected entity anywhere in view** (§7.11 boundary — strictly no world-broadcast). (QA-SCAN-1.)
- **Manual (con glyph):** select targets of varying relative threat; disable color. **Pass: 5 states distinguishable by shape alone; the glyph reads as a quiet line-glyph, not a loud color badge** (§7.12-2; §7.11 recessive-con fence).
- **Placement:** frame in the upper-left periphery, never center-screen.
- **Consumes:** the S4-00 con-glyph shape set (do not invent shapes here) + the S4-02 Layer 1 vocabulary.
- **Evidence:** `production/qa/evidence/s4-03-target-frame-evidence.md` (frame on-select/off-deselect/absent-on-untargeted; 5 con states color-disabled).

### S4-04 — Cast Bar + Interaction Prompt (UI / ADVISORY)

- **Manual (cast bar):** initiate a timed cast, let it complete; interrupt another. **Pass: linear left-to-right fill, no completion flourish, disappears on interrupt; sits clear of the 40-60% band; peripherally readable** (§7.12-4; §7.11 cast-bar exception).
- **Manual (interaction prompt):** approach `M3_Caretaker` within range + facing (~60° cone); then turn away / leave range; then approach across the room. **Pass: prompt names "Caretaker Morrvik" only when in range+facing; vanishes on turn-away; never across the room; screen-space, not floating over the NPC; single-target** (§7.12-3).
- **No routing:** no "go here" hint, no across-room prompt, no objective-advertising; interaction prompt suppressed in combat with an active target. (QA-SCAN-1.)
- **Consumes:** the S4-00 cast-bar placement validation + S4-02 vocabulary; presents the existing S3-01 dispatch (does not re-author the interact path).
- **Evidence:** `production/qa/evidence/s4-04-cast-prompt-evidence.md` (cast bar mid-cast/interrupt; prompt in-range vs out-of-range/across-room).

### S4-05 — First District Atmosphere + Legibility (Visual/Feel / ADVISORY; F1 item BLOCKING)

- **Manual (reads-as-place + no-routing):** walk the district from spawn through the loop path. **Pass: reads as a gothic place via practical-source lighting + massing/sightlines + placeholder material vocabulary; spawn-to-`M3_Caretaker` discoverable by spatial readability; zero glowing doors / markers / minimap pins / atmosphere-as-warning / guidance lighting.** (QA-SCAN-1; Pillar 2.)
- **Bound (BLOCKING scope check):** greybox-grade only — no produced art palette, no final textures (D016). If work trends toward produced art, it is out of scope and stops (Q-R6).
- **Integration [F1] (BLOCKING):** record the artifact-identity tuple — `_DevEntry.unity` content SHA, First District NavMesh asset SHA + size, and (if present) bake-scope SHA — **with the exact commands to reproduce it** (e.g. `git hash-object`). **Pass: a reader re-runs the commands and gets the same tuple; S4-06 can compare.** This is the freshness anchor that closes the F1 "artifact exists but model-under-test drifted" gap.
- **Scene:** adapter/additive; ProjectSettings drift restored; no builder chaining (RG-S, RG-02).
- **Evidence:** `production/qa/evidence/s4-05-district-atmosphere-evidence.md`.
- **Sequence:** before/after S4-01, never concurrent.

### S4-06 — EQ-Readable Human-Play Gate (Integration + human-play / BLOCKING)

The milestone exit. Composes S4-01..S4-05. Four hollow-evidence fences are hard BLOCKING ACs; the feel verdict is the binding human gate.

- **[F1] Artifact-identity match:** re-run S4-05's tuple commands; the played build's scene/NavMesh/bake SHAs **must match**. Mismatch → FAIL (played scene drifted from authored).
- **[F2] Real walked traversal — NOT marker teleport:** the player physically walks spawn → `M3_Caretaker` → relic → vendor → `M3_Caretaker` through movement (and NavMesh where relevant). The S3-03/S3-04 runners that *teleported* `ClericShellMarker` prove adapter dispatch, not playable traversal. **Pass: the real walked route.**
- **[F3] Real input path:** interactions fire through `Input.GetKeyDown(_interactKey)` in the harness `Update()`, **not** a direct `TryDispatchInteract()` call. **Pass: a real keypress produces the same telemetry sequence as the dispatch tests.**
- **[F7] Exact main-path telemetry sequence:** the full accept → recover → loot → sell → hand-in telemetry asserted as the **exact ordered sequence**, no hidden interleaved/duplicate/diagnostic masking. (Subsequence ≠ pass.)
- **Six §7.12 criteria (S4-06-01):** all six pass (combat-state read, target ID ≤1s, interaction confirmation, cast-state read, HUD coherence, no routing).
- **N=1 human-play feel verdict (S4-06-06, BINDING):** the product owner (Brian) answers the one-more-pull question; on PASS, names a **world element** (objective / NPC / relic / the district) as the reason — not mechanical reward, completionism, testing, or "the game told me to". The protocol **separates "reads/plays as the game" from "is it final-art pretty"** (greybox acceptable per D016; R-P2-FEEL-MISATTRIBUTION).
- **Evidence:** `tests/evidence/S4-06/verification.md` + `tests/evidence/S4-06/human-play-[YYYYMMDD].md` (the feel verdict), plus the F1 tuple match, F2 traversal proof, F3 input-path proof, F7 exact-sequence telemetry.
- **On FAIL:** re-scopes within Sprint 4; it does **not** silently downgrade to a qualified supplement (2026-05-20 feel-gate lesson). A surfaced defect routes back to the owning S4-01..05 story.

## Automated Coverage Expectations

Sprint 4 is presentation work; most evidence is screenshots + manual walkthrough + the §7.12 read tests (ADVISORY gates per the coding-standards evidence table). Automation applies where it is honest:

- **S4-00:** colorblind-simulation render of the con-glyph set (artifact, not a pass/fail runner).
- **S4-01 / S4-05:** scene-diff adapter-only proof, ProjectSettings-restore confirmation, and the three M2-preservation batchmode reruns are automatable and required (RG-S, RG-02). S4-05's **[F1]** tuple is a reproducible command set (BLOCKING).
- **S4-02 / S4-03 / S4-04:** a Unity batchmode HUD-render/readability capture and the §7.11 forbidden-treatment scan where automatable; selection-gating and cast-placement assertions where a runner can observe them. These supplement, never replace, the manual §7.12 read tests.
- **S4-06:** the four fences (**F1** hash compare, **F2** traversal telemetry, **F3** input-path telemetry, **F7** exact-sequence assertion) are automatable and BLOCKING. The feel verdict is explicitly **not** automatable — it is the human gate.
- **No test is disabled or skipped to make a gate pass.** A number that cannot be validated is recorded as an open question with its method, not guessed (the evidence-honesty discipline this milestone inherits).

## Evidence Requirements (summary)

| Story | Evidence File | Gate |
| --- | --- | --- |
| S4-00 | `production/qa/evidence/s4-00-hud-thresholds-evidence.md` | ADVISORY |
| S4-01 | `production/qa/evidence/s4-01-play-camera-evidence.md` | BLOCKING |
| S4-02 | `production/qa/evidence/s4-02-vitals-hud-evidence.md` | ADVISORY |
| S4-03 | `production/qa/evidence/s4-03-target-frame-evidence.md` | ADVISORY |
| S4-04 | `production/qa/evidence/s4-04-cast-prompt-evidence.md` | ADVISORY |
| S4-05 | `production/qa/evidence/s4-05-district-atmosphere-evidence.md` (incl. [F1] tuple) | ADVISORY (+[F1] BLOCKING) |
| S4-06 | `tests/evidence/S4-06/verification.md` + `tests/evidence/S4-06/human-play-[YYYYMMDD].md` | BLOCKING |

Every "done" claim requires test-pass or file:line evidence; "configured" is not evidence (game-dev-governance Evidence Discipline). Screenshots are the primary evidence for Visual/Feel + UI stories and require lead/owner sign-off (ADVISORY gate per coding-standards).

## Open Blockers

| ID | Blocker | Owner | Next Action |
| --- | --- | --- | --- |
| B-1 | **S4 slate not opened / owners-in-status not reconciled.** `sprint-status.yaml` `stories:` still lists the S3 slate; `total_stories`/`completed_stories` = 0. Story files exist (owners set in-file) but the status reconciliation is a separate governance write. | Producer | Sprint 3→4 `sprint-status.yaml` reconciliation (`sprint-4.md:191`, Next Gate item 2) — out of this plan's scope. This plan writes only the `qa_plan:` pointer. |
| B-2 | **S4-00 must deliver validated numbers before S4-02/03/04 start.** | ux-designer | Run S4-00; deliver concrete bar-height / panel-fill / cast-placement values (or labelled open questions with methods). HUD stories blocked until then. |
| B-3 | **Scene-touching stories (S4-01, S4-05) must be sequenced, not concurrent.** | Producer + scene-touching implementers | Schedule S4-01 and S4-05 in series; apply RG-S Scene Discipline each. |
| B-4 | **Control manifest absent (project-wide).** | (pre-existing) | Continue the documented `docs/registry/architecture.yaml` fallback for forbidden-pattern QA; non-blocking. |

## Validation

- Every conclusion cites a repository source (sprint-4 plan, the seven story files, art-bible §7.10/§7.11/§7.12, coding-standards, governance rules, the Sprint 2/3 QA-plan precedents). ✓
- Every blocker has a concrete next action. ✓
- Proposed writes stay within the declared output path (`production/qa/plans/qa-plan-sprint-4-20260607.md`) plus the explicitly-authorized one-line `qa_plan:` pointer in `production/sprint-status.yaml`. ✓
- No story statuses, story rows, or sprint counts modified. ✓

## Next Actions

1. Record this plan's pointer in `production/sprint-status.yaml` (`qa_plan: "production/qa/plans/qa-plan-sprint-4-20260607.md"`) — the one authorized field update, same commit.
2. **`/create-stories` reconciliation already done in-file** (S4-00..S4-06 exist); the remaining governance write is the **Sprint 3→4 `sprint-status.yaml` story reconciliation** (separate, out of scope here).
3. Begin implementation at **S4-00** (gates the HUD chain) and **S4-01** (independent; sequence against S4-05 for scene safety).
4. Run `/story-readiness` on each story before `/dev-story`, and `/scope-check` before each story closes (Q-R6).

---

*Sources: `production/sprints/sprint-4.md`; `production/stories/s4-00..s4-06`; `design/art/art-bible.md` §7.10/§7.11/§7.12; `.claude/docs/coding-standards.md`; `.claude/rules/game-dev-governance.md`; `production/qa/plans/qa-plan-sprint-3-20260524.md` + `qa-plan-sprint-2-20260509.md`; `DECISIONS.md` D016/D017/D020. Generated 2026-06-07 at HEAD `e5428e0`.*
