# S3-EVIDENCE-01: S3 Evidence Integrity Patch

> **Sprint**: Sprint 3 — Playable Vertical-Slice Assembly
> **Sprint Plan**: `production/sprints/sprint-3.md` (out-of-band evidence integrity patch before S3-06)
> **Status**: Ready for Story Readiness
> **Layer**: Test Infrastructure / Evidence Integrity
> **Type**: Integration / test-infrastructure
> **Estimate**: 1.0 day (MEDIUM confidence — bounded runner/evidence work, with two negative controls)
> **Manifest Version**: Unavailable (control-manifest absent project-wide per `production/qa/plans/qa-plan-sprint-2-20260509.md:54,60`; documented fallback applies)
> **Generated**: 2026-05-28
> **Owner**: Codex (test-infrastructure story; Qwen3-Coder not eligible because this spans multiple evidence runners and Unity scene/evidence semantics)

> **Patch notice**: this story is a pre-S3-06 evidence-integrity patch, not a playable-loop payload story. It repairs confirmed hollow or under-specified evidence so S3-06 can consume trustworthy preservation, negative, and NavMesh evidence. It must branch from post-PR #5 `main` after `S3-05.1: NavMesh obstacle inclusion correction` merges.

## Context

**Sprint 3 plan**: `production/sprints/sprint-3.md`
**Quick-design source**: `design/quick/quick-design-m3-objective-npc-loot.md`
**GDD anchors**: `design/gdd/game-concept.md:163` through `design/gdd/game-concept.md:201` (Pillars 1-5; evidence here protects the Sprint 3 playable-loop claim), `design/gdd/world-structure.md:34` (Pillar 2 navigation anti-patterns inherited by S3-05 spatial evidence)
**Story dependency**: PR #5, `S3-05.1: NavMesh obstacle inclusion correction`, merged to `main` before this patch branches
**Requirement IDs (story-local ACs)**: `S3-EVIDENCE-01-01` through `S3-EVIDENCE-01-04`

**Requirement Summary**: Patch four evidence-integrity gaps before S3-06. First, replace the M3 end-to-end runner's unconditional faction-negative check with a structural-absence assertion and a runner-local negative control. Second, make M2 preservation evidence self-describing by recording whether builders were skipped, and fail preservation-mode runs unless `-gravenspireSkipBuilder` is explicit. Third, harden PR #5's NavMesh bake-scope runner from center-only obstacle probing to footprint coverage plus a flat-floor negative control. Fourth, add a lightweight composability stub to reachability and soft-lock evidence so the bake-scope precondition is visible in downstream artifacts.

**Known hollow evidence being patched**:
- `Assets/Editor/GravenspireM3EndToEndObjectiveLoopVerificationRunner.cs:271` through `:277` currently records `no_faction_consequence_applied` from `var noFactionConsequence = true` instead of observed runtime state.
- T1 has faction identity but no implemented M5 faction-consequence machinery to observe: `src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs:358` discards the carried `factionId`, and `src/gameplay/npc/m3-objective/M3LootTableFixedProfileVendorSession.cs:189` through `:199` keeps reputation/faction-rank/future economy hooks false.
- PR #5's bake-scope runner center-probes obstacle footprints at `Assets/Editor/GravenspireS3DistrictNavMeshBakeScopeVerificationRunner.cs:314` through `:326`; this proves obstacle-center carving but not footprint coverage for corners, edges, long thin boundary walls, or larger footprints.

**Governing decisions** (DECISIONS.md):

| D-entry | Status | Usage |
|---|---|---|
| D001 (`DECISIONS.md:14`) | Locked | Unity 6.3 LTS + C# + URP; runners must stay compatible with the pinned Unity path |
| D003 (`DECISIONS.md:51`) | Locked | Tier 1 single-player offline; no netcode, no server evidence, no Tier 2 composability framework |
| D016 (`DECISIONS.md:554`) | Locked | Sprint 3 is playable assembly; this patch is infrastructure so S3-06's playable-loop verdict consumes clean evidence |

**Architecture / ADRs**: N/A — no new architecture decision applies. This story patches runner behavior and evidence fields only. It explicitly does not introduce the future composability framework described in Out of Scope.

**Engine Risk**: LOW-MEDIUM. Work touches Unity Editor runners, NavMesh queries, and batchmode evidence files, but does not add runtime gameplay systems.

**Performance Impact**: No gameplay performance impact expected. Added footprint probes and negative controls run only inside Editor/batchmode verification runners. Evidence must state probe counts and scan density so slower runner behavior is visible if it becomes material.

## Acceptance Criteria

- [ ] **S3-EVIDENCE-01-01 — Faction negative check is derived from structural absence.** `no_faction_consequence_applied` in `GravenspireM3EndToEndObjectiveLoopVerificationRunner.cs` is no longer derived from an unconditional `true`. The runner asserts structural absence of faction-consequence machinery in the M3/S3 runtime path: no faction-consequence sink/event/component is present or wired, and future-M5 hooks that are intentionally false remain false. The story does not build a faction sink or faction-consequence implementation. A runner-local fake faction-consequence probe/event negative control must trip the check to FAIL, proving the check can detect the failure it guards against.

- [ ] **S3-EVIDENCE-01-02 — Preservation evidence is self-describing.** M2 preservation runners emit `builder_skipped=true|false` into their evidence. Preservation-mode runs fail unless `-gravenspireSkipBuilder` is set, so a reviewer can distinguish "preserve the existing scene and test it" from "rebuild the scene and test the rebuilt result." Existing normal builder-backed runs remain allowed and record `builder_skipped=false`.

- [ ] **S3-EVIDENCE-01-03 — Bake-scope footprint coverage and negative control.** The post-PR #5 bake-scope runner extends obstacle carve probes beyond center-only checks to footprint coverage: corners inset by agent radius, edge midpoints, and an interior grid for large footprints. Long thin boundary walls are priority coverage. The runner emits per-obstacle footprint probe results and aggregate pass/fail fields. A flat-floor negative-control fixture or mode, with obstacles intentionally not carved, must be flagged FAILING by the runner; this proves the runner detects the flat-bake disease it guards against.

- [ ] **S3-EVIDENCE-01-04 — Lightweight composability stub is present.** Reachability and soft-lock evidence emit `precondition_artifact_required: navmesh-bake-scope`, naming the bake-scope artifact as a required precondition. This is only a visibility stub: it does not enforce fresh artifact hashes, scene hashes, or NavMesh asset identity. Evidence consumers can no longer silently cite reachability or soft-lock output alone as if the bake-scope precondition were implicit.

## Implementation Notes

- **Branch sequencing**: do not start implementation until PR #5 merges. This patch branches from post-merge `main`, because AC-03 hardens the bake-scope runner introduced by PR #5 rather than re-opening that reviewed correction.
- **AC-1 scope lock**: do not create real M5 faction-consequence infrastructure. The live T1 surface carries faction identity but discards or defers consequence behavior. The fix is structural absence plus a runner-local negative control, not a sink abstraction.
- **AC-2 preservation-mode flag**: add a `-gravenspireSkipBuilder` command-line argument to the M2 preservation runners that currently call `GravenspireM2SingleTrashLoopBuilder.Build()` before running. In preservation mode, skip the builder and fail if the flag is absent. Evidence must record `builder_skipped=true` only when the flag was present and honored.
- **AC-3 footprint geometry**: calculate obstacle footprint from collider bounds or the runner's resolved obstacle bounds. Probe corners inset by the NavMesh agent radius, edge midpoints, and enough interior samples to catch large footprints. The output should make failures local: name the obstacle, probe kind, probe position, sample radius, expected fail/pass, and actual result.
- **AC-3 negative control**: the flat-floor negative control can be a runner mode, fixture scene setup, or controlled in-run fixture mutation, as long as it cannot pass accidentally by reusing the corrected obstacle-aware bake. The evidence must show the negative-control result as expected FAIL, not as a passing main artifact.
- **AC-4 stub discipline**: add the precondition field to the reachability and soft-lock runner evidence output. Do not attempt the full Tier 2 composability contract here.
- **Evidence timestamps**: no hardcoded dates; use `DateTimeOffset.UtcNow` for evidence timestamps and `EditorApplication.timeSinceStartup` for editor timing.
- **Scene discipline**: if a negative-control fixture requires a Unity scene or asset, keep it isolated and explicitly named as test fixture evidence. Do not alter gameplay scene layout beyond what PR #5 already established unless the implementation proves it is necessary and story-local.

## Out of Scope

- Full Tier 2 composability contract. Future design-story definition, recorded verbatim: "a runner claim is invalid unless its preconditions are asserted in the same artifact or linked by fresh artifact hash / scene hash / NavMesh asset identity."
- Negative controls for all runners. This story adds the faction fake-event negative control and the flat-floor NavMesh negative control only.
- Semantic data-driven scan rewrite for brittle substring checks.
- S3-06 AC-08 route-plurality instrumentation.
- XML doc comments on combat public API.
- Sprint-status reconciliation or main-lane bookkeeping.
- M5 faction consequence, faction reputation, or visible world-state consequence implementation.
- Save/Load, Inventory persistence, netcode, live LLM, or any Tier 2+ surface.

## QA Test Cases

**Test S3-EVIDENCE-01-T1: faction structural absence and negative control (AC-01)**
- Given: `_DevEntry.unity` loaded with the M3 end-to-end runner's normal scene configuration.
- When: `GravenspireM3EndToEndObjectiveLoopVerificationRunner` runs.
- Then: `no_faction_consequence_applied` is computed from structural absence checks, not an unconditional constant; evidence records the inspected absence surfaces and PASS.
- Negative control: the runner-local fake faction-consequence probe/event is enabled in a controlled test path; the same check records FAIL and exits non-zero or records expected-failing negative-control evidence.

**Test S3-EVIDENCE-01-T2: M2 preservation builder-skip evidence (AC-02)**
- Given: each M2 preservation runner is invoked with `-gravenspireEvidencePath` and `-gravenspireSkipBuilder`.
- When: the runner writes evidence.
- Then: the evidence includes `builder_skipped=true`; no builder call rebuilds the scene before the preservation assertion.
- Failure path: invoke preservation mode without `-gravenspireSkipBuilder`; runner fails with a clear message that preservation evidence requires explicit builder skip.
- Control path: invoke the existing normal runner path without preservation-mode intent; evidence records `builder_skipped=false`.

**Test S3-EVIDENCE-01-T3: bake-scope footprint probes (AC-03)**
- Given: post-PR #5 `_DevEntry.unity` with obstacle-aware NavMesh bake and the bake-scope runner loaded.
- When: the bake-scope runner executes.
- Then: each obstacle reports center, inset corners, edge midpoints, and interior-grid probes as appropriate for its footprint. Long thin boundary walls have enough edge/interior coverage to prove their full length is not silently walkable.
- Edge cases: tiny obstacles may have fewer interior samples if the inset footprint is smaller than the agent radius; evidence must say why a probe class was skipped.

**Test S3-EVIDENCE-01-T4: flat-floor negative control fails (AC-03)**
- Given: a controlled flat-floor negative-control fixture where obstacles are intentionally not carved or not included in the NavMesh bake.
- When: the bake-scope runner executes against that fixture/mode.
- Then: the runner flags the fixture FAILING, with at least one footprint probe resolving incorrectly on walkable NavMesh inside an obstacle footprint. Evidence records this as expected negative-control failure.

**Test S3-EVIDENCE-01-T5: precondition stub appears downstream (AC-04)**
- Given: reachability and soft-lock runners execute against the corrected scene.
- When: each evidence artifact is written.
- Then: each artifact includes `precondition_artifact_required: navmesh-bake-scope` and names the bake-scope artifact path used by the verification batch. No hash/scene-identity enforcement is required in this story.

## Test Evidence

**Required evidence**: `tests/evidence/S3-EVIDENCE-01/verification.md`

Companion artifacts:
- `tests/evidence/S3-EVIDENCE-01/unity-m3-end-to-end-faction-negative-[YYYYMMDD]-smoke.md` (AC-01 structural absence)
- `tests/evidence/S3-EVIDENCE-01/unity-m3-end-to-end-faction-negative-control-[YYYYMMDD]-smoke.md` (AC-01 fake faction-consequence negative control)
- `tests/evidence/S3-EVIDENCE-01/m2-02-preservation-skip-builder-[YYYYMMDD]-smoke.md` (AC-02)
- `tests/evidence/S3-EVIDENCE-01/m2-03-preservation-skip-builder-[YYYYMMDD]-smoke.md` (AC-02)
- `tests/evidence/S3-EVIDENCE-01/m2-04-preservation-skip-builder-[YYYYMMDD]-smoke.md` (AC-02)
- `tests/evidence/S3-EVIDENCE-01/navmesh-bake-scope-footprint-[YYYYMMDD]-smoke.md` (AC-03 main footprint coverage)
- `tests/evidence/S3-EVIDENCE-01/navmesh-bake-scope-flat-floor-negative-control-[YYYYMMDD]-smoke.md` (AC-03 expected-failing negative control)
- `tests/evidence/S3-EVIDENCE-01/reachability-precondition-stub-[YYYYMMDD]-smoke.md` (AC-04)
- `tests/evidence/S3-EVIDENCE-01/soft-lock-precondition-stub-[YYYYMMDD]-smoke.md` (AC-04)
- `dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — Combat regression baseline must hold (current expected: 189/189)
- T1 negative-scope scan over changed files — zero real implementation hits expected; classified documentation hits allowed only if explicitly recorded
- `git diff --check` — clean
- `.githooks/pre-commit` — `[pre-commit] OK`
- `dotnet format --verify-no-changes` — PASS

**Evidence status**: Not started

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| PR #5 — `S3-05.1: NavMesh obstacle inclusion correction` | AC-03 hardens the bake-scope runner introduced there; branch must include the corrected obstacle-aware bake and runner | Merged to `main` |
| S3-05 | Reachability, soft-lock, and NavMesh evidence artifacts exist only after the navigable greybox story | Done / Done with Notes |

S3-06 must not consume the current faction, preservation, reachability, soft-lock, or NavMesh evidence until this patch closes.

## Blockers

PR #5 merge is the only implementation blocker. Story creation and readiness review can happen before the merge, but `/dev-story` starts only after post-PR #5 `main` exists.

Watch items (not blockers):
- `s3_05_ac12_partial_rollforward_to_s3_06` — remains S3-06's responsibility; this story only makes evidence dependencies visible and trustworthy.
- `m2_melee_rng_not_reset` — remains S3-06 AC-04 scope; this story adds preservation builder-skip evidence but does not reset M2 RNG cursors.
- `m2_02_runner_date_hardcoded` — do not introduce new hardcoded dates while touching preservation runners.
- `control_manifest_absence_pre_existing` — Manifest Version `Unavailable` per fallback.
