# S3-05.1 Verification Correction - NavMesh Obstacle Inclusion

**Result:** PASS (correction complete)
**Date:** 2026-05-28
**Correction branch:** `codex/s3-05-1-navmesh-obstacle-inclusion-correction`
**Base commit:** `0d3ea58` (PR #4 merge of original S3-05)
**Correction commit range:** pending commit at time of this artifact
**Story corrected:** `production/stories/s3-05-navigable-greybox-first-district.md`

## Why This Correction Exists

Post-merge code review found that the original S3-05 NavMesh evidence did not
prove obstacle-aware navigation. The original scene used a NavMeshSurface whose
bake model excluded the obstacle geometry: the surface collected only its
children, while the landmark massings and boundary walls were authored as
siblings of the surface root. The original reachability and soft-lock evidence
therefore tested a flat floor with gameplay anchors on it, not a district whose
obstacles constrained navigation.

The original soft-lock scan showed `900` on-mesh samples, `0` off-mesh samples,
and `0` trapped samples in
`tests/evidence/S3-05/soft-lock-scan-20260526-smoke.md:32` through
`tests/evidence/S3-05/soft-lock-scan-20260526-smoke.md:36`. That was internally
consistent, but it was flat-plane-trivial.

## Four-Layer Defect Chain

These were one cascade, each layer masked by the prior layer:

| Layer | Defect | Why it was masked | Correction |
|---|---|---|---|
| 1 | Bake scope excluded obstacles. | `CollectObjects.Children` collected only descendants of the NavMeshSurface, while obstacles were siblings, so reachability and soft-lock queries never saw obstacle holes. | Keep `CollectObjects.Children`, but re-parent the floor, 3 landmark massings, and 4 boundary walls under `FirstDistrict_NavMeshSurface`; the corrected bake-scope runner reports `surface_collects_children` PASS at `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:33`, aggregate `all_required_objects_in_declared_bake_scope` PASS at `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:76`, and the per-object scope rows at `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:150` through `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:157`. |
| 2 | `M3_ObjectiveRelic` sat inside the Relic Storehouse footprint once the storehouse could affect the bake. | The flat-plane bake let the relic sample onto walkable floor even though the visible layout overlapped it. | Move the Relic Storehouse from `z=5` to `z=9`, keeping the relic anchor clear. The anchor-clearance runner reports `M3_ObjectiveRelic` at `(-1.85, 0, 3.15)` resolving with `0` horizontal displacement and PASS at `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:178`. |
| 3 | `M2_NamedBlocker` then sat inside the Storehouse footprint after the first storehouse move. | The first correction exposed the next anchor overlap only after the obstacle was no longer excluded. | The same Relic Storehouse rearward move, final `z=9`, also clears `M2_NamedBlocker`. The blocker's coordinates are unchanged; only the landmark moved. The anchor-clearance runner reports `M2_NamedBlocker` at `(-2.8, 0, 5.6)` resolving with `0` horizontal displacement and PASS at `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:182`. |
| 4 | Collider inclusion did not by itself carve obstacle holes. | Including colliders in the bake scope made the obstacles visible to the bake, but did not guarantee a not-walkable carve. | Add active `NavMeshModifierVolume` components with area `1` (Not Walkable) to all 3 landmark massings and 4 boundary walls. The corrected runner reports the modifier-volume checks PASS at `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:45`, `:50`, `:55`, `:60`, `:65`, `:70`, and `:75`, and obstacle-center tight probes fail as expected at `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:81` through `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:114`. |

The anchor-clearance half of the bake-scope runner was added during the
correction because layers 2 and 3 proved that obstacle correctness and gameplay
anchor correctness must be asserted together. The runner now proves both halves:
obstacles carve holes and gameplay destinations remain reachable.

## Systemic Framing

This was not four unrelated bugs. It was one foundational defect: the NavMesh
was not doing useful obstacle work. The flat-plane bake masked the two layout
overlaps because every anchor still sampled onto a floor with no carved holes.
It also masked the carve-mechanism gap because no obstacle had to prove it
removed walkable area. Once the bake included the obstacles, the latent layout
and carve-mechanism defects became visible in sequence.

## Corrected Evidence

| Evidence | Result | Verification method |
|---|---|---|
| Soft-lock divergence | PASS: `855` on-mesh, `45` off-mesh, `0` trapped. | Corrected soft-lock scan records `900` total samples, `855` on-mesh samples, `45` off-mesh samples, `855` reachable samples, and `0` trapped samples at `tests/evidence/S3-05/soft-lock-scan-20260527-correction-smoke.md:32` through `tests/evidence/S3-05/soft-lock-scan-20260527-correction-smoke.md:36`; `zero_soft_lock_zones_detected` PASS is recorded at `tests/evidence/S3-05/soft-lock-scan-20260527-correction-smoke.md:49`. |
| Bake-scope intent | PASS. | `FirstDistrict_NavMeshSurface` uses `CollectObjects.Children` at `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:33`; aggregate required-object inclusion passes at `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:76`; floor, 3 landmark massings, and 4 boundary walls are included in declared bake scope at `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:150` through `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:157`. |
| Bake-scope carve mechanism | PASS. | All 7 obstacles carry active Not Walkable modifier volumes, reported at `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:45`, `:50`, `:55`, `:60`, `:65`, `:70`, and `:75`. |
| Bake outcome at obstacles | PASS. | Obstacle tight probes fail inside obstacle footprints, and aggregate `all_obstacle_tight_probes_fail` PASS is recorded at `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:81` through `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:114`. |
| Anchor clearance | PASS. | All 9 gameplay anchors resolve on NavMesh with low horizontal displacement; aggregate checks PASS at `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:143` through `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:144`, with relic and named-blocker rows at `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:178` and `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md:182`. |
| M3 reachability | PASS. | Corrected reachability smoke records `M3_Caretaker_path_complete`, `M3_ObjectiveRelic_path_complete`, `M3_CourtVendor_path_complete`, and `all_m3_anchors_reachable` PASS at `tests/evidence/S3-05/reachability-20260527-correction-smoke.md:30` through `tests/evidence/S3-05/reachability-20260527-correction-smoke.md:37`. |
| Phase 1 builder verification | PASS. | `GravenspireS3FirstDistrictGreyboxBuilder.VerifyPhase1` now verifies paths to `M3_ObjectiveRelic` and `M2_NamedBlocker` in the corrected layout at `Assets/Editor/GravenspireS3FirstDistrictGreyboxBuilder.cs:440` and `Assets/Editor/GravenspireS3FirstDistrictGreyboxBuilder.cs:444`. |
| Combat regression canary | PASS: 189/189. | `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` returned 189/189 PASS during Phase 2 verification. |

The `45` off-mesh samples are the important divergence proof: they correspond
to grid cells whose centers fall inside carved obstacle footprints. The `0`
trapped result means every corrected on-mesh sample still reaches
`ClericShellMarker`.

## What Changed Structurally

- `Assets/Editor/GravenspireS3FirstDistrictGreyboxBuilder.cs` keeps the district
  surface on `CollectObjects.Children` and reparents required bake geometry under
  the surface root, so the bake scope is explicit and deterministic.
- `Greybox_RelicStorehouse_Massing` moved from `z=5` to `z=9`, clearing
  `M3_ObjectiveRelic`.
- The named-blocker overlap was cleared by the same Storehouse move to `z=9`,
  not by moving the blocker.
- No M2 or M3 anchor coordinates were changed in S3-05.1. The only positional
  change was the Relic Storehouse landmark (`z=5` -> `z=9`). `M3_ObjectiveRelic`
  and `M2_NamedBlocker` were cleared by moving the obstacle away from them,
  preserving the anchor-coordinate contract.
- All 7 obstacles now carry active `NavMeshModifierVolume` components with area
  `1` (Not Walkable), so collider inclusion is paired with an explicit carve
  mechanism.
- `Assets/Editor/GravenspireS3DistrictNavMeshBakeScopeVerificationRunner.cs`
  was added as the new bake-scope precondition runner. It asserts static intent,
  carve mechanism, runtime obstacle outcome, and gameplay-anchor clearance.
- The bake-scope runner's wide perimeter probe uses a 3 m radius, not 5 m, to
  avoid false-positive samples from vertical NavMesh islands on cube tops while
  still proving ground-level perimeter displacement.

## Superseded Evidence

The original S3-05 Phase 2 and Phase 3 NavMesh evidence is superseded for
NavMesh-functionality claims:

- `tests/evidence/S3-05/reachability-20260526-smoke.md`
- `tests/evidence/S3-05/soft-lock-scan-20260526-smoke.md`

Those files remain retained for audit. They correctly record what the original
runners observed, but they no longer support the claim that the First District
NavMesh accounted for obstacle geometry. The corrected evidence files are:

- `tests/evidence/S3-05/navmesh-bake-scope-20260527-smoke.md`
- `tests/evidence/S3-05/reachability-20260527-correction-smoke.md`
- `tests/evidence/S3-05/soft-lock-scan-20260527-correction-smoke.md`

## Pattern Note

`GravenspireS3DistrictNavMeshBakeScopeVerificationRunner` is now the district
and zone precedent. Any future NavMesh evidence runner must assert its own
preconditions before claiming path completeness: the bake/query model includes
the relevant obstacles, those obstacles actually carve not-walkable space, and
the gameplay destinations remain reachable after carving. The corrected runner
proves both intent and outcome; future district evidence should not rely on
`PathComplete` alone.
