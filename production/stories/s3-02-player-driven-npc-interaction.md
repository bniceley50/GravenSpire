# S3-02: Player-Driven NPC Interaction

> **Sprint**: Sprint 3 — Playable Vertical-Slice Assembly
> **Sprint Plan**: `production/sprints/sprint-3.md` (Story Ledger row, line 67)
> **Status**: Complete
> **Layer**: Core
> **Type**: Integration
> **Estimate**: 1.0 day (HIGH confidence — thin wiring onto already-built M3 system, per `sprint-3.md:156`)
> **Manifest Version**: Unavailable (control-manifest absent project-wide per `production/qa/plans/qa-plan-sprint-2-20260509.md:54,60`; documented fallback applies — `control_manifest_absence_pre_existing` carryover)
> **Generated**: 2026-05-23
> **Owner**: Codex

## Context

**Sprint 3 plan**: `production/sprints/sprint-3.md`
**Quick-design source**: `design/quick/quick-design-m3-objective-npc-loot.md`
**Story Ledger row**: `production/sprints/sprint-3.md:67`
**Requirement IDs (story-local ACs)**: `S3-02-01` through `S3-02-07`

**Requirement Summary**: Wire the existing `M3NamedNpcObjectiveFrame` (the M3 named-NPC objective-frame system, closed in S2-M3-01 at commit `1166cae`) behind the S3-01 player interaction harness so the player can intentionally interact with the `M3_Caretaker` and receive the objective frame through real input. Introduce a thin adapter MonoBehaviour that implements the S3-01 `IPlayerInteractTarget` interface and, on dispatch, calls `M3NamedNpcObjectiveFrame.TryRecordIntentionalInteraction(playerActorId, distanceMeters, out NpcInteractionContext context)`. The adapter mounts on the `M3_Caretaker` GameObject in `_DevEntry.unity`. `M3NamedNpcObjectiveFrame.cs` has zero diff in this story; S2-M3-01 boundaries hold (no quest markers, no overhead names, no Dialogue System UI, no live LLM). Telemetry records the player-driven `npc_interaction_intentional` event with the full `NpcInteractionContext` shape, including the templated dialogue handle, surfaced as data only (not rendered).

**Governing decisions** (DECISIONS.md):

| D-entry | Status | Usage |
|---|---|---|
| D001 (`DECISIONS.md:14`) | Locked | Unity 6.3 LTS + C# + URP — engine/stack |
| D003 (`DECISIONS.md:51`) | Locked | Tier 1 single-player offline |
| D004 (`DECISIONS.md:73`) | Provisional (revisit at T3 gate) | Templated default dialogue, no live LLM at T1 — the templated `dialogueTemplateSetId` surfaces in telemetry as data, not rendered UI |
| D012 (`DECISIONS.md:342`) | Locked | T1 combat-feel validated — composes with combat-feel-validated systems |
| D016 (`DECISIONS.md:554`) | Locked | No new systems, reuse-not-rebuild, greybox-not-art |

**Sprint 3 feedback rule** (`sprint-3.md:85`): every feedback element passes the trigger and direction tests — player verb + "what is this / did that work." No advertising, locating, or routing.

**Architecture Module**: Gameplay / NPC Interaction Adapter (new thin adapter component; lives under `Assets/Scripts/` alongside the existing M3 files)
**Engine**: Unity 6.3 LTS
**Engine Risk**: LOW (uses only core MonoBehaviour + Transform + already-tested M3 API surface; no URP / UI Toolkit / DOTS / deprecated-API surface)

**Surfaces reused** (do not re-author):
- S3-01 harness dispatch interface: `IPlayerInteractTarget.TryInteract(string playerActorId, float distanceMeters, out InteractContext context)` (introduced in S3-01)
- M3 named-NPC objective frame: `Assets/Scripts/M3NamedNpcObjectiveFrame.cs:112` — `bool TryRecordIntentionalInteraction(string playerActorId, float distanceMeters, out NpcInteractionContext context)`. Returns `false` on whitespace `playerActorId` or `distanceMeters > ConfiguredInteractionRangeMeters` (M3 internal range-gate). On success, populates `NpcInteractionContext` with `_npcId`, `playerActorId`, `_activeZoneId`, `InteractionState`, `InteractionKind`, `_dialogueTemplateSetId`, `_objectiveFrameTextKey`, `wasIntentional: true`, `distanceMeters`.
- `M3_Caretaker` scene anchor: `Assets/Scenes/_DevEntry.unity:881` (added in S2-M3-01)
- S2-M3-01 closure baseline: commit `1166cae` (named NPC anchor + session-local intentional interaction)

## Acceptance Criteria

- [x] **S3-02-01**: A new thin adapter MonoBehaviour (proposed name: `M3NamedNpcInteractTarget`) is created under `Assets/Scripts/`, implements `IPlayerInteractTarget` (from S3-01), and is attached to the `M3_Caretaker` GameObject in `Assets/Scenes/_DevEntry.unity`.
- [x] **S3-02-02**: The adapter holds a serialized reference to a `M3NamedNpcObjectiveFrame` instance and, on `TryInteract(...)`, calls `frame.TryRecordIntentionalInteraction(playerActorId, distanceMeters, out NpcInteractionContext npcContext)`. The adapter returns the boolean result directly and maps `npcContext` into the harness's `InteractContext` (the adapter is the mapping point; the harness sees a uniform `InteractContext`).
- [x] **S3-02-03**: `M3NamedNpcObjectiveFrame.cs` has **zero diff** in this story. S2-M3-01 closure (`1166cae`) is preserved. No additional public methods, no signature changes, no field changes on the M3 frame component.
- [x] **S3-02-04**: On a successful adapter dispatch (Try* returned `true`), telemetry records a `npc_interaction_intentional` event with all `NpcInteractionContext` fields: `npcId`, `playerActorId`, `activeZoneId`, `InteractionState`, `InteractionKind`, `dialogueTemplateSetId`, `objectiveFrameTextKey`, `wasIntentional` (must be `true`), `distanceMeters`. The event source attribution is `player_driven` (not `runner_driven`), distinguishing this from the S2-M3-01 runner-only path.
- [x] **S3-02-05**: The templated `dialogueTemplateSetId` and `objectiveFrameTextKey` are surfaced **as data** in the telemetry payload only. No Dialogue System UI / dialogue window / dialogue box renders. No live LLM call. No overhead NPC name plate, no quest marker, no minimap pin, no objective signpost (S2-M3-01 boundary and Sprint 3 feedback rule both hold).
- [x] **S3-02-06**: Player-driven path is end-to-end: harness keypress → harness raycast/distance-check finds the `M3_Caretaker`-mounted adapter → harness invokes `adapter.TryInteract(...)` → adapter calls `frame.TryRecordIntentionalInteraction(...)` → telemetry records the event → harness's interact-fired feedback (per S3-01 AC-06) plays. No runner-side shortcut path that bypasses the harness; the runner exercises the same player-input path as a real player session would.
- [x] **S3-02-07**: When the player keypress fires from outside the M3 frame's `ConfiguredInteractionRangeMeters` (M3 returns `false`), the harness's interact-blocked feedback (per S3-01 AC-08) plays. No diagnostic text explaining the M3 internal reason; no routing hint to "get closer."

## Implementation Notes

- **S3-01 dependency**: this story cannot start until S3-01 lands the harness component and the `IPlayerInteractTarget` interface. The adapter's job is to satisfy that interface against the existing M3 frame.
- **Adapter mapping**: `NpcInteractionContext` → `InteractContext` is the adapter's responsibility. S3-01 defined the `IPlayerInteractTarget` interface shape; S3-02 is the first consumer that actually carries payload through `InteractContext`. For S3-02, the adapter copies relevant fields — `npcId`, `playerActorId`, `distanceMeters`, `wasIntentional`, plus a payload pointer or copy of `dialogueTemplateSetId` and `objectiveFrameTextKey` for telemetry. **If `InteractContext` as defined by S3-01 cannot carry the `dialogueTemplateSetId` + `objectiveFrameTextKey` payload without widening, the widening is in-scope for this story** — S3-02 is the first consumer beyond the harness itself, so the gap surfaces here. The test for AC-04 implicitly verifies the payload roundtrip. Downstream (S3-03, S3-04) each map their own M3 context types into the (possibly widened) `InteractContext`.
- **Frame instance source**: the adapter's `M3NamedNpcObjectiveFrame` reference can be a serialized inspector field set in `_DevEntry.unity`, a `GetComponent` lookup on the same GameObject, or a runtime registration call. The choice is implementation-side; the test verifies the dispatch chain works, not the wiring mechanism. Prefer the same pattern used by the existing M3 verification runner (`Assets/Editor/GravenspireM3NamedNpcObjectiveFrameVerificationRunner.cs`) to minimize scene-edit surface.
- **No dialogue UI rendering**: even though the templated dialogue handle is part of the M3 contract surface, this story renders nothing on screen for dialogue. The handle is data; rendering is a future-story concern (and currently out of T1 scope per D004 templated-default boundary).
- **Telemetry source attribution**: `npc_interaction_intentional` already exists from S2-M3-01 (`1166cae`) as a runner-driven path. This story adds the player-driven path. Distinguish the two paths in telemetry (e.g., `source: "player_driven" | "runner_driven"`) so historical runner-only evidence remains identifiable. The exact field name is implementation-side; the test asserts the distinction exists.
- **No `DateTime.UtcNow`** in editor/runner code (project-wide deny pattern). Use `DateTimeOffset.UtcNow` for evidence timestamps, `EditorApplication.timeSinceStartup` for editor timing.
- **Scene Discipline**: the adapter wiring touches `_DevEntry.unity` (attaching the adapter to the `M3_Caretaker` GameObject). Save in Unity before staging, inspect the diff, do not hand-edit YAML. One scene edit per PR.
- **Style Gate**: `dotnet format --verify-no-changes` must pass locally before this PR. Inherits the same format-setup pre-condition as S3-01.

## Out of Scope

- No rewrite of `M3NamedNpcObjectiveFrame.cs` (S2-M3-01 closure preserved; D016 red flag)
- No Dialogue System UI / dialogue window / dialogue box / scrolling text (S2-M3-01 boundary)
- No live LLM dialogue or templated dialogue rendering (D004 templated-default boundary at T1)
- No overhead NPC name plate, quest marker, minimap pin, objective signpost, glow, outline, or any feedback element advertising/locating/routing (Pillar 2, Sprint 3 feedback rule)
- No multiple-NPC support in this story (S3-02 wires one NPC: `M3_Caretaker`)
- No NPC schedule, daily routine, or temporal behavior (S2-M3-01 boundary; the NPC is statically placed)
- No faction reaction or consequence on NPC interaction (M5 deferred behind Sprint 3)
- No Save/Load of the interaction state (M4 deferred behind Sprint 3; the existing session-local invariant from S2-M3-01 holds)
- No objective state mutation in this story (that's S3-03, via the separate `M3ObjectiveStateRelicHandIn` system)
- No loot or vendor logic (S3-03, S3-04)
- No human-play feel acceptance criterion on this story (sits only on S3-06 per `sprint-3.md:141`)

## QA Test Cases

### Integration test (Unity Play Mode / batchmode runner)

**Test setup** (shared): Unity 6.3 LTS (`6000.3.14f1`) editor batchmode; `_DevEntry.unity` loaded; S3-01 harness wired; `M3NamedNpcInteractTarget` adapter mounted on `M3_Caretaker`; player marker positioned at known world coordinates relative to the `M3_Caretaker` anchor.

**Test S3-02-T1: adapter presence and binding (AC-01, AC-02, AC-03)**
- Given: `_DevEntry.unity` is loaded.
- When: Play Mode enters.
- Then: the `M3NamedNpcInteractTarget` MonoBehaviour is present on the `M3_Caretaker` GameObject and is registered with the S3-01 harness as an `IPlayerInteractTarget`. Its `M3NamedNpcObjectiveFrame` reference resolves to the same frame instance S2-M3-01 added. The frame's own state is unchanged from a fresh `_DevEntry.unity` load.
- Edge cases: adapter component disabled → not registered; frame reference null → adapter logs a clear setup error and remains harmless (no NullReferenceException on dispatch).

**Test S3-02-T2: player-driven intentional interaction at in-range distance (AC-04, AC-05, AC-06)**
- Given: player marker positioned at a distance from `M3_Caretaker` that is within the M3 frame's `ConfiguredInteractionRangeMeters` (and within the harness's own interact threshold).
- When: the harness's interact keycode fires.
- Then: the harness raycast/distance-check finds the adapter; the harness invokes `adapter.TryInteract(playerActorId, actualDistance, out interactContext)`; the adapter calls `frame.TryRecordIntentionalInteraction(playerActorId, actualDistance, out npcContext)` exactly once; the frame returns `true`; a `npc_interaction_intentional` telemetry event records with all `NpcInteractionContext` fields including `wasIntentional: true`, `dialogueTemplateSetId: "dialogue.m3.caretaker.objective_frame_t1"`, `objectiveFrameTextKey: "m3.objective.recover_marked_relic.frame"`, and a `source: "player_driven"` attribution; the harness's interact-fired feedback fires (per S3-01 AC-06).
- Edge cases: `playerActorId` whitespace/null → frame returns false at line 119–122; this manifests as interact-blocked (see T3), not interact-fired; rapid repeat key → multiple events record, no debounce-induced silent skip.

**Test S3-02-T3: out-of-range player attempt (AC-07)**
- Given: player marker positioned at a distance from `M3_Caretaker` that exceeds the M3 frame's `ConfiguredInteractionRangeMeters` but is still within the harness's own raycast/distance-check threshold (i.e., the harness finds the adapter, but the M3 layer rejects).
- When: the harness's interact keycode fires.
- Then: the harness invokes `adapter.TryInteract(...)`; the adapter calls `frame.TryRecordIntentionalInteraction(...)`; the frame returns `false` at line 124–127; no `npc_interaction_intentional` event records; the harness's interact-blocked feedback fires (per S3-01 AC-08). The blocked feedback does NOT explain that the player was out of M3's range; it does NOT say "get closer."
- Edge cases: distance exactly equal to the M3 threshold (boundary inclusive vs exclusive) — verify against the M3 frame's behavior at line 124 (`distanceMeters > ConfiguredInteractionRangeMeters`, strict greater-than, so equal-to is in-range) and assert the boundary behavior in the test.

**Test S3-02-T4: M3 frame zero-diff invariant (AC-03)**
- Given: pre- and post-implementation source-tree snapshots of `Assets/Scripts/M3NamedNpcObjectiveFrame.cs` and `Assets/Scripts/M3NamedNpcObjectiveFrame.cs.meta`.
- When: `git diff --stat` is run between the snapshots.
- Then: zero lines changed in either file. The story's pre-commit evidence includes the diff stat output proving this.
- Edge cases: none (binary invariant).

**Test S3-02-T5: no UI rendering invariant (AC-05)**
- Given: any test path that produces a successful or blocked interaction (T2 or T3).
- When: Play Mode runs.
- Then: no Dialogue System / UGUI / UI Toolkit panel renders for dialogue; no overhead text mesh appears on `M3_Caretaker`; no minimap overlay; no objective signpost. The runner's screen-capture or scene-tree query confirms no new UI elements exist that weren't there before the harness fired.
- Edge cases: prompts from S3-01 AC-09 (range-gated "Press [E]") are NOT a violation of this AC — they're the S3-01 harness's allowed range-gated prompt, not a dialogue UI.

### M2 preservation reruns (additional required evidence, not standalone ACs)

Per the Sprint 2 M3 pattern (`sprint-3.md:144`), re-verify M2 clean-loop / overpull / named-blocker preservation since this story modifies `_DevEntry.unity`:

- `M2SingleTrashLoop` smoke — must PASS, exit 0, no captured errors
- `M2LinkedTrashOverpull` smoke — must PASS, exit 0
- `M2NamedBlockerBoundary` smoke — must PASS, exit 0

Run each via its existing scenario-smoke runner with `-gravenspireEvidencePath` redirected to `tests/evidence/S3-02/`. See "M2 preservation rerun execution note" below.

## Test Evidence

**Required evidence**: `tests/evidence/S3-02/verification.md`

Companion artifacts:
- `tests/evidence/S3-02/unity-player-driven-npc-interaction-[YYYYMMDD]-smoke.md` (story-specific Unity batchmode runner output covering T1–T5)
- `tests/evidence/S3-02/m2-02-preservation-[YYYYMMDD]-smoke.md`
- `tests/evidence/S3-02/m2-03-preservation-[YYYYMMDD]-smoke.md`
- `tests/evidence/S3-02/m2-04-preservation-[YYYYMMDD]-smoke.md`
- `tests/evidence/S3-02/m3-frame-zero-diff-[YYYYMMDD].txt` (git diff stat output proving `M3NamedNpcObjectiveFrame.cs` has zero diff)
- `dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — Combat regression baseline must hold (current: 189/189)
- T1 negative-scope scan over changed files — zero matches expected
- `git diff --check` — clean
- `.githooks/pre-commit` — `[pre-commit] OK`
- `dotnet format --verify-no-changes` — PASS

**Evidence status**: Complete

**M2 preservation rerun execution note** (per `m2_melee_rng_not_reset` carryover): the three M2 preservation smokes cannot be chained in a single Unity batchmode invocation. The M2 controller's four `LoopingMeleeRandomSource` melee-RNG cursors at `M2SingleTrashMedLoopController.cs:76-79` are `readonly`, created once, and never reset between smokes within a Play session — chained runs yield stale RNG and bogus results. Run each preservation smoke in its own `Unity.exe -batchmode -executeMethod ...` invocation, each with its own `-gravenspireEvidencePath` override. Pattern established in S2-M3-04 closure.

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| `S3-01` | The adapter implements `IPlayerInteractTarget`, defined by the S3-01 harness. The end-to-end dispatch chain (harness → adapter → M3 Try*) requires both stories to be present. | Done |

**Sprint-level pre-condition (tracked):** `dotnet format` setup — same as S3-01, must be resolved before this PR.

## Blockers

S3-01 must close before this story enters `/dev-story`. D004 is Provisional but its T1 boundary (templated default, no LLM) is explicit and this story respects it strictly (templated handle surfaces as telemetry data only, no rendering). No other blockers; D001/D003/D012/D016 all Locked.

Watch items (not blockers):
- `m2_melee_rng_not_reset` — see Test Evidence execution note (three M2 smokes require separate invocations)
- `m2_02_runner_date_hardcoded` — do not introduce a new hardcoded date in any story-specific runner; use `DateTimeOffset.UtcNow`
- `m2_renderer_material_property_access` — if adapter feedback touches renderer state, follow the `MaterialPropertyBlock` lesson
- `control_manifest_absence_pre_existing` — Manifest Version header carries `Unavailable` per documented fallback
- Format Gate — see Dependencies
- **D004 templated-dialogue boundary**: if T3 ever lands LLM dialogue, the adapter is the natural seam to gate live LLM behind a config switch — this story does not implement that switch, but the adapter shape should not preclude it

## Completion Notes

**Completed**: 2026-05-30
**Verdict**: COMPLETE
**Criteria**: 7/7 passing (S3-02-01 … S3-02-07)
**Deferred/Untested Criteria**: None
**Test Evidence**: `tests/evidence/S3-02/verification.md` (PASS) + companions — S3-02 player-driven smoke (T1–T5), S3-01 harness regression smoke (PASS after the cross-story harness edit), 3× M2 preservation smokes (separate Unity invocations, `Builder Invoked: false`), `m3-frame-zero-diff-20260529.txt` (zero output), Combat regression 189/189, both `dotnet format` targets exit 0, `git diff --check` clean, T1 negative-scope scan (classified runner-guard hits only).
**GDD/ADR Deviations**: None. D004 T1 boundary respected (templated dialogue handle surfaced as telemetry data only, no render). `M3NamedNpcObjectiveFrame.cs` zero-diff (AC-03).
**Scope Notes**: One intentional cross-story touch — `Assets/Scripts/S3PlayerInteractionHarness.cs` (S3-01 deliverable) edited additively so the NPC target telemetry event is recorded before the appended `interact_fired` feedback event (anticipated by AC-02). S3-01 regression smoke PASS confirms no S3-01 behavior break. Also added a `!isActiveAndEnabled` registration guard (covers the T1 "disabled adapter → not registered" edge case).
**Review Gates**: lean (Task subagents skipped). Codex pre-PR review (no blocking findings) + main-lane read-only staged-set verification + /story-done evidence review.
**Forced Completion**: No
**Merge**: PR #7 (`codex/s3-02-player-driven-npc-interaction`) merged to `main` at `8acb53b` on 2026-05-30; implementation commit `e1ed954`.
