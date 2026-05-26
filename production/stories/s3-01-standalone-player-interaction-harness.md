# S3-01: Standalone Player Interaction Harness

> **Sprint**: Sprint 3 — Playable Vertical-Slice Assembly
> **Sprint Plan**: `production/sprints/sprint-3.md` (Story Ledger row, line 66)
> **Status**: Complete
> **Layer**: Core
> **Type**: Integration
> **Estimate**: 1.5 days (MEDIUM confidence — greenfield composition layer, per `sprint-3.md:155`)
> **Manifest Version**: Unavailable (control-manifest absent project-wide per `production/qa/plans/qa-plan-sprint-2-20260509.md:54,60`; documented fallback applies — `control_manifest_absence_pre_existing` carryover)
> **Generated**: 2026-05-23
> **Owner**: Codex

## Context

**Sprint 3 plan**: `production/sprints/sprint-3.md`
**Quick-design source**: `design/quick/quick-design-m3-objective-npc-loot.md`
**GDD anchor**: `design/gdd/npc-system.md:125` (Interactions with Other Systems — defines the `NpcInteractionContext` / Dialogue boundary the harness dispatches into) and `design/gdd/npc-system.md:264` (`npc_interaction_range_meters` tuning knob — the canonical interaction-range value the harness's range-gate must respect at the M3 layer)
**Story Ledger row**: `production/sprints/sprint-3.md:66`
**Requirement IDs (story-local ACs)**: `S3-01-01` through `S3-01-09`

**Requirement Summary**: Introduce a NEW standalone Unity component that owns the player marker, fires a single interact verb on player input via raycast or distance-check, and dispatches the verb to a registered M3 target through a defined interface. The component reuses (extends) the existing M2 player locomotion and marker without re-authoring locomotion. It implements no objective, loot, or vendor logic. It locks the player-feedback contract for three outcomes — interact-fired, interact-missed, interact-blocked — all of which are acknowledgements of a player action and obey the Sprint 3 feedback rule (acknowledge action; never advertise, locate, or route). The interact prompt is range-gated to within the interact threshold and never serves as a far-distance locator.

**Governing decisions** (DECISIONS.md):

| D-entry | Status | Usage |
|---|---|---|
| D001 (`DECISIONS.md:14`) | Locked | Unity 6.3 LTS + C# + URP — engine/stack constraint for harness implementation |
| D003 (`DECISIONS.md:51`) | Locked | Tier 1 single-player offline — no netcode, no live LLM, no Save/Load |
| D012 (`DECISIONS.md:342`) | Locked | T1 combat-feel validated — harness composes with combat-feel-validated systems; no re-authoring |
| D016 (`DECISIONS.md:554`) | Locked | Sprint 2→3 re-sequence — no new systems, reuse-not-rebuild, greybox-not-art |

**Sprint 3 feedback rule** (creative-director pillar consult, recorded at `sprint-3.md:85`): every feedback element passes a two-part test — (a) trigger: does it fire from a player verb (allowed) or ambiently/unprompted (cut); (b) direction: does it answer "what is this / did that work" (allowed) or "where do I go / what's next" (cut). An interact prompt is allowed only range-gated; never as a far-distance locator. Pillar 2, *The Silence Is Sacred*.

**Architecture Module**: Gameplay / Player Interaction (new standalone component; lives alongside `Assets/Scripts/M3*.cs` per Sprint 2 placement convention)
**Engine**: Unity 6.3 LTS
**Engine Risk**: LOW (uses core MonoBehaviour, Transform, legacy `UnityEngine.Input`, and raycast/distance-check APIs that pre-date Unity 6.1; no URP render-pass code, no UI Toolkit, no DOTS, no deprecated-API surface)
**Performance Impact**: No-impact at story scope. The harness runs a single Update with one keycode poll plus at most one raycast or O(n) distance-check against the registered target set (n ≤ small constant in Sprint 3 — one NPC, one relic pickup, one vendor); no per-frame allocations, no renderer hot-path access, no GC churn. Project-wide performance budgets are still `[TO BE CONFIGURED]` per `.claude/docs/technical-preferences.md` (set during Tier 1 prototype), so this story claims no absolute frame-time number; the relative claim is "additive load is negligible vs. the existing M2 controller's Update loop." Evidence path on closure: Unity batchmode runner T1 confirms harness present and active with no log-warn/log-error from a per-frame budget assertion.

**Surfaces reused** (do not re-author):
- M2 player locomotion: `Assets/Scripts/M2SingleTrashMedLoopController.cs:564-583` (`HandlePlayerMovement` — WASD transform-driven movement + `FollowCamera`, drives `_playerMarker` / `ClericShellMarker`)
- M2 player marker: `ClericShellMarker` GameObject in `Assets/Scenes/_DevEntry.unity`
- M3 dispatch contract shape (informational; harness does not call this directly in S3-01): `Assets/Scripts/M3NamedNpcObjectiveFrame.cs:112` — `bool TryRecordIntentionalInteraction(string playerActorId, float distanceMeters, out NpcInteractionContext context)`. Returns `false` on invalid `playerActorId` or `distanceMeters > ConfiguredInteractionRangeMeters`. The M3 layer already enforces its own range-gate; the harness passes through distance and respects the boolean return.

## Acceptance Criteria

- [x] **S3-01-01**: A new standalone Unity MonoBehaviour component (proposed name: `S3PlayerInteractionHarness`) is created under `Assets/Scripts/`, wired into `Assets/Scenes/_DevEntry.unity`, and is NOT bolted onto `M2SingleTrashMedLoopController` (USER DECISION recorded at `sprint-3.md:75`).
- [x] **S3-01-02**: The harness extends/reuses the existing player marker (`ClericShellMarker`) and the M2 locomotion at `M2SingleTrashMedLoopController.cs:564-583`. The harness contains no locomotion code; movement code in `M2SingleTrashMedLoopController` has zero diff in this story.
- [x] **S3-01-03**: A single interact verb fires from one player-input keycode (proposed: `KeyCode.E`, matching M2's legacy `UnityEngine.Input` pattern). The verb triggers either a raycast from the player's forward axis or a distance-check against registered targets — implementation choice is the developer's, but the chosen mechanism is documented in the harness header XML doc.
- [x] **S3-01-04**: The harness exposes a dispatch interface (proposed shape: `IPlayerInteractTarget { bool TryInteract(string playerActorId, float distanceMeters, out InteractContext context); }`) that S3-02, S3-03, and S3-04 will route their M3 `Try*` methods through. The interface design must accommodate the M3 dispatch shape at `M3NamedNpcObjectiveFrame.cs:112`. If no target is in range or no target is hit, no dispatch occurs.
- [x] **S3-01-05**: The harness implements zero objective, loot, or vendor logic. It only dispatches. A code-review reject signal: any rewrite of `M3*` state transitions, loot resolution, or the F4 vendor formula in this story (D016 red flag, `sprint-3.md:83`).
- [x] **S3-01-06**: **Interact-fired** (dispatch returned `true`): the player perceives an acknowledgement of their action (visual flash, brief tone, or equivalent). The acknowledgement names the action (e.g., "interacted") or its result (e.g., "objective accepted" when S3-02 lands), never advertises a quest, locator, or routing hint.
- [x] **S3-01-07**: **Interact-missed** (raycast/distance-check found nothing): the player perceives an acknowledgement that the verb fired and missed (subtle audio cue or equivalent). No proximity hint, no "look over there" arrow, no nearest-target highlight.
- [x] **S3-01-08**: **Interact-blocked** (target found, but its `Try*` returned `false` — e.g., out of M3's own range, or invalid state): the player perceives an acknowledgement that the action did not succeed. No diagnostic text explaining why, no routing hint, no quest-marker overlay.
- [x] **S3-01-09**: Any visible interact prompt ("Press [E]" or equivalent) is **range-gated**: it appears only when a valid target is within the interact threshold. It must never display as a far-distance locator (Pillar 2 violation; reject in review). The threshold is a configurable serialized field on the harness component, default value to be set during implementation and documented in the verification evidence.

## Implementation Notes

- **No-new-systems discipline (D016)**: Sprint 3 builds the orchestration layer; the harness is the orchestration layer's player-input front-end. Reuse the M2 marker, reuse the legacy `UnityEngine.Input` pattern, reuse the M3 dispatch shape. No new locomotion, no new input system, no new dialogue UI.
- **Legacy Input is acceptable** (TD consult, `sprint-3.md:77`): M2 already uses legacy `UnityEngine.Input`; the harness follows suit. Do not introduce the Unity Input System package — that's Tier-2+ scope and out of D003.
- **Naming convention**: PascalCase class (`S3PlayerInteractionHarness`), file matches class (`S3PlayerInteractionHarness.cs`), `_camelCase` private fields, PascalCase public/serialized fields per `.claude/docs/technical-preferences.md`.
- **No `DateTime.UtcNow` in editor/runner code** (project-wide deny pattern per Sprint 2 hygiene lessons — `m2_02_runner_date_hardcoded` etc.). If a verification runner is added under `Assets/Editor/`, use `DateTimeOffset.UtcNow` for evidence timestamps and `EditorApplication.timeSinceStartup` for editor timing.
- **Renderer hot-path discipline**: if any feedback uses a renderer material, follow the `m2_renderer_material_property_access` lesson — prefer `.sharedMaterial` + `MaterialPropertyBlock` over `renderer.material` reads on Update.
- **Scene Discipline** (governance): `_DevEntry.unity` modification is a scene edit — save in Unity before staging, inspect the diff, do not hand-edit YAML. One scene edit per PR per `.claude/rules/game-dev-governance.md` Scene Discipline.
- **Feedback rule trigger/direction test**: every feedback element passes (a) trigger = player verb (not ambient) AND (b) direction = "what is this / did that work" (not "where do I go"). Reviewer rejects any element visible or audible before the player chooses to engage the thing (`sprint-3.md:85`).
- **Style Gate**: `dotnet format --verify-no-changes` must pass locally before this story's PR (`.claude/rules/game-dev-governance.md` Code Style Gate). The Sprint 3 format-gate pre-condition is RESOLVED — PR #2 merge commit `90821f2` wired the policy + baseline + pre-commit hook (`.editorconfig:8-18`, `.gitattributes:1-82`, `.githooks/pre-commit:12-15`); the hook runs `dotnet format --verify-no-changes --exclude-diagnostics IDE1006` on `tests/Gravenspire.Combat.Tests.csproj` + `prototypes/combat-slice-T1/Harness/CombatSliceHarness.csproj`. This story's PR must pass the gate locally; the IDE1006 naming-debt exclusion is carryover `ide1006_naming_debt_excluded_from_format_gate`, not Sprint 3 scope.

## Out of Scope

Explicit non-goals (drawn from `sprint-3.md:66` Scope Boundary column and Sprint 2 M3 boundaries that hold forward):

- No verb bolted onto `M2SingleTrashMedLoopController` (USER DECISION)
- No objective state, loot resolution, or vendor formula logic in the harness (D016 red flag)
- No dialogue system UI / dialogue window / dialogue box (S2-M3-01 boundary)
- No live LLM dialogue (D004; templated dialogue only at T1)
- No quest log, quest marker, minimap, overhead NPC name plate, objective signposting, or any feedback element that advertises/locates/routes (Pillar 2, Sprint 3 feedback rule)
- No far-distance interact prompt (range-gated only)
- No Unity Input System package (legacy `UnityEngine.Input` matches M2)
- No `CharacterController` (greybox-grade transform-driven locomotion reuses M2)
- No Save/Load hook (M4 deferred behind Sprint 3)
- No faction reaction or consequence (M5 deferred behind Sprint 3)
- No tuned economy, currency persistence, coin pacing claim (S2-M3-03 boundary)
- No second interact verb in this story (one verb only; multi-verb expansion is a future scope decision, not Sprint 3)
- No human-play feel acceptance criterion on this story (sits only on S3-06 per `sprint-3.md:141` and the 2026-05-20 definition-of-done lesson)

## QA Test Cases

### Integration test (Unity Play Mode / batchmode runner)

**Test setup** (shared across cases): Unity 6.3 LTS (`6000.3.14f1`) editor batchmode; `_DevEntry.unity` loaded; harness wired with `ClericShellMarker` and a mock `IPlayerInteractTarget` registered at a known world position with a known M3 Try* return profile.

**Test S3-01-T1: harness presence and standalone separation (AC-01, AC-02)**
- Given: `_DevEntry.unity` is loaded and the M2 controller's `_playerMarker` is positioned at world origin.
- When: the scene enters Play Mode.
- Then: the `S3PlayerInteractionHarness` MonoBehaviour is present and active on a GameObject distinct from the M2 controller's GameObject; `M2SingleTrashMedLoopController.HandlePlayerMovement` continues to drive `_playerMarker.position` on WASD input (M2 locomotion preserved).
- Edge cases: harness disabled → M2 locomotion unaffected; harness present but no targets registered → no dispatch on interact, no error log.

**Test S3-01-T2: interact verb dispatch on player input (AC-03, AC-04, AC-05)**
- Given: a mock `IPlayerInteractTarget` is registered, positioned within the harness's interact threshold from the player marker, and primed to return `true` from `TryInteract`.
- When: the harness's input keycode (`KeyCode.E`) is simulated via `Input` synthesis or the harness's testable dispatch entry point.
- Then: the harness invokes the mock's `TryInteract` exactly once with the player's actor id and the actual distance; the mock's `TryInteract` returns `true`; no objective/loot/vendor state is mutated by the harness itself.
- Edge cases: no target hit → no dispatch, no error; multiple targets in range → dispatch to nearest only, documented and tested.

**Test S3-01-T3: interact-fired feedback (AC-06)**
- Given: the mock target returned `true` from `TryInteract` (from T2).
- When: the harness emits its interact-fired feedback.
- Then: an observable acknowledgement fires (telemetry event `interact_fired` recorded with the action name; or visible/audible cue verified via runner capture). The feedback names the action or result; the runner asserts no advertising/locating/routing text (no presence of strings matching a deny-pattern list: `"quest"`, `"go to"`, `"objective located"`, `"nearest"`, `"track"`, or equivalent — finalized in the story's verification fixture).
- Edge cases: rapid repeat input → repeat firing acknowledged each time; no debounce-induced silent skip.

**Test S3-01-T4: interact-missed feedback (AC-07)**
- Given: no target is registered or the player marker is positioned with no targets in raycast/distance range.
- When: the player input keycode fires.
- Then: a `interact_missed` telemetry event records; an observable acknowledgement fires (per the per-outcome feedback contract); no proximity hint, no nearest-target name, no direction indicator.
- Edge cases: target just past the threshold → missed (not blocked) feedback path; rapid repeat → repeated acknowledgement, no drift to a "locator" cue.

**Test S3-01-T5: interact-blocked feedback (AC-08)**
- Given: a mock target is registered in range, but primed to return `false` from `TryInteract` (simulating M3 internal range/state policing).
- When: the player input keycode fires.
- Then: a `interact_blocked` telemetry event records; an observable acknowledgement fires; no diagnostic text explaining the M3 internal reason; no routing/locator hint.
- Edge cases: target returns `false` due to its own range-gate (M3 layer) vs due to state — both produce the same blocked acknowledgement at the harness layer; runner asserts identical feedback shape.

**Test S3-01-T6: range-gated interact prompt (AC-09)**
- Given: a mock target is registered; the player marker is moved through three distances — within threshold, at threshold, and well beyond threshold.
- When: at each distance, the harness's prompt-display state is queried.
- Then: prompt is displayed only when within threshold; not at the threshold boundary's far side; not beyond. The prompt never displays a target position, name, or direction indicator at any distance.
- Edge cases: zero-distance overlap → prompt displays; player marker not yet initialized → prompt does not display, no error.

### M2 preservation reruns (additional required evidence, not standalone ACs)

Per the established Sprint 2 M3 pattern (`sprint-3.md:144`), re-verify M2 clean-loop / overpull / named-blocker preservation since this story modifies `_DevEntry.unity`:

- `M2SingleTrashLoop` smoke (single trash pull + med break) — must PASS, exit 0, no captured errors
- `M2LinkedTrashOverpull` smoke — must PASS, exit 0
- `M2NamedBlockerBoundary` smoke — must PASS, exit 0

Run each via its existing scenario-smoke runner under `Assets/Editor/`, with `-gravenspireEvidencePath` redirected to `tests/evidence/S3-01/`. Smoke files dated 2026-05-XX with the actual run date in the body (not hardcoded — the `m2_02_runner_date_hardcoded` carryover is still open, evidence reviewer verifies actual date against filename).

## Test Evidence

**Required evidence**: `tests/evidence/S3-01/verification.md`

Companion artifacts:
- `tests/evidence/S3-01/unity-player-interaction-harness-[YYYYMMDD]-smoke.md` (story-specific Unity batchmode runner output covering T1–T6)
- `tests/evidence/S3-01/m2-02-preservation-[YYYYMMDD]-smoke.md` (M2 single-trash preservation)
- `tests/evidence/S3-01/m2-03-preservation-[YYYYMMDD]-smoke.md` (M2 linked-trash preservation)
- `tests/evidence/S3-01/m2-04-preservation-[YYYYMMDD]-smoke.md` (M2 named-blocker preservation)
- `dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — Combat regression baseline must hold (current: 189/189 per S2-M3-03 closure)
- T1 negative-scope scan over changed files — zero matches expected
- `git diff --check` — clean
- `.githooks/pre-commit` — `[pre-commit] OK`
- `dotnet format --verify-no-changes` — PASS (resolves Style Gate; format-setup pre-condition already landed via PR #2 / `90821f2`, so the hook is wired before this PR opens — no longer an open finding)

**Evidence status**: Complete — `tests/evidence/S3-01/verification.md`

**M2 preservation rerun execution note** (per `m2_melee_rng_not_reset` carryover): the three M2 preservation smokes above **cannot be chained in a single Unity batchmode invocation**. The M2 controller's four `LoopingMeleeRandomSource` melee-RNG cursors at `M2SingleTrashMedLoopController.cs:76-79` are `readonly`, created once, and never reset by `ResetLoop` / `ResetOverpullMetrics` / `ResetNamedBlockerMetrics`. Determinism holds for one smoke per Play session but breaks when smokes are chained — chained runs yield stale RNG state and bogus results. Run each preservation smoke in its own `Unity.exe -batchmode -executeMethod ...` invocation, each with its own `-gravenspireEvidencePath` override. This is the pattern S2-M3-04 ultimately landed on after the chained-smoke failure that surfaced the carryover (see the active.md S2-M3-04 closure extract).

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| None at story level | S3-01 is the foundation story for the Sprint 3 slate; the M2 surfaces it reuses (locomotion, marker, `_DevEntry.unity`) and the M3 dispatch contract shape it accommodates are all already-built (Sprint 2 M2/M3 closures `b4cb377`, `1166cae`, `fb77f83`, `25c94ee`, `ee7c450`). | N/A |

**Sprint-level pre-condition (RESOLVED, no longer blocking this PR):** `dotnet format` setup. Per `sprint-3.md:120` the format gate had to pass locally before any Sprint 3 PR; that pre-condition is now resolved by PR #2 merge commit `90821f2` (2026-05-24), which landed the policy commit `f040493`, the baseline commit `a0785d8`, and the hook-wiring commit `cf1c204` together. The pre-commit hook at `.githooks/pre-commit:12-15` now runs `dotnet format --verify-no-changes --exclude-diagnostics IDE1006` on the two .NET project files, so the gate is enforceable on every Sprint 3 PR including this one. Story author still runs the gate locally before opening the PR (see Test Evidence).

## Blockers

None. All four governing D-entries (D001, D003, D012, D016) are Locked. **ADRs: N/A for this story** — no new architecture decision is required and no Proposed ADRs are in scope; the harness composes existing M2/M3 surfaces under already-Locked D-entries, and the interface shape it introduces (`IPlayerInteractTarget`) is a story-local contract, not an architecture-level decision. No unresolved design questions — the plan's Story Ledger row, the TD feasibility consult result, and the CD pillar consult together close the design surface for the harness contract.

Watch items (not blockers):
- `m2_renderer_material_property_access` — if harness feedback touches renderer state, follow the `MaterialPropertyBlock` lesson
- `m2_02_runner_date_hardcoded` — if a story-specific runner is added, do not introduce a new hardcoded date; use `DateTimeOffset.UtcNow`
- `m2_melee_rng_not_reset` — the M2 controller's four `LoopingMeleeRandomSource` melee-RNG cursors at `M2SingleTrashMedLoopController.cs:76-79` are `readonly` and never reset by `ResetLoop` / `ResetOverpullMetrics` / `ResetNamedBlockerMetrics`. Operational impact for this story: the three M2 preservation smokes (single-trash, overpull, named-blocker) cannot be chained in one batchmode invocation; each requires a separate `Unity.exe -batchmode` invocation. See the Test Evidence "M2 preservation rerun execution note" above.
- `control_manifest_absence_pre_existing` — Manifest Version header carries `Unavailable` per documented fallback
- Format Gate — see Dependencies section

## Completion Notes

- Closed via `/story-done` 2026-05-25 with verdict **COMPLETE WITH NOTES** (9/9 AC passing).
- Implementation landed through PR #3: `fecd121` created the standalone harness, `45459cb` fixed the runner's redundant telemetry check, `d7cde93` added the Unity scene/evidence pass, and merge commit `9072bcb` brought S3-01 to `main`.
- Evidence: `tests/evidence/S3-01/verification.md:8` records PASS; `tests/evidence/S3-01/verification.md:14-22` maps all 9 ACs to passing evidence; `tests/evidence/S3-01/unity-player-interaction-harness-20260524-smoke.md:7` records the S3 runner PASS; `tests/evidence/S3-01/unity-player-interaction-harness-20260524-smoke.md:11-44` lists the passing story-specific checks.
- M2 preservation: `tests/evidence/S3-01/verification.md:28-30` records the three M2 preservation smokes as PASS, and each smoke file records `**Result:** PASS` at line 7. The smokes were run as separate Unity invocations per `m2_melee_rng_not_reset`.
- Local gates rerun at closure: `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` PASS 189/189; `git diff --name-only df5fdec..9072bcb -- Assets/Scripts/M2SingleTrashMedLoopController.cs` returned no path, confirming no M2 controller modification in S3-01.
- Scope scan: PASS WITH CLASSIFIED HITS. Hits were story out-of-scope/GDD references, `ClericShellMarker` references, and the runner's player-facing deny-list term (`minimap`); no runtime implementation of netcode, Save/Load, live LLM, quest log, minimap, `CharacterController`, or Unity Input System was added.
- Code review: PR #3 review and body verification completed before merge; `/story-done` lead-programmer gate skipped under lean mode.
- Known follow-ups, not closed here: C2 (`IMGUI` prompt/feedback is acceptable for S3-01 wiring but not final S3-06 feel UI) and C3 (`_autoDiscoverTargetsOnStart = true` should become an executable S3-02 assertion if separately approved).
- Next gate: `/story-readiness production/stories/s3-02-player-driven-npc-interaction.md`. S3-05 is also unblocked by S3-01, but S3-02 remains the dependency-chain next active story.
