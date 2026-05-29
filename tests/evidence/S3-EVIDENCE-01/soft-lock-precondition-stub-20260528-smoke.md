# S3-05 First District Soft-Lock Scan Smoke

**Date:** 2026-05-29
**Story:** `production/stories/s3-05-navigable-greybox-first-district.md`
**Scene:** `Assets/Scenes/_DevEntry.unity`
**Runner:** `Assets/Editor/GravenspireS3DistrictSoftLockScanVerificationRunner.cs`
**Result:** PASS

## Composability Precondition

- `precondition_artifact_required`: `navmesh-bake-scope`
- `bake_scope_artifact_path`: `tests/evidence/S3-EVIDENCE-01/navmesh-bake-scope-footprint-20260528-smoke.md`
- `enforcement`: `visibility-only`
- This block names the bake-scope footprint artifact this soft-lock scan depends on. It is a visibility stub for future composability: it does NOT verify artifact freshness, file hashes, scene hashes, or NavMesh asset identity. No Tier 2 composability framework is implied (D003 — Tier 1 single-player offline).

## Methodology

- Claim framing: best-effort high-confidence scan, not exhaustive proof that no soft-lock can exist.
- Sample grid: 30 x 30 cell centers over the 30 m x 30 m district footprint.
- Grid spacing: 1 m.
- Sample query: `NavMesh.SamplePosition(sample, maxDistance=1 m, areaMask=NavMesh.AllAreas)`.
- Path query: `NavMesh.CalculatePath(sampled_position, sampled_spawn, NavMesh.AllAreas, path)`.
- Pass condition: every sample that resolves onto the NavMesh returns `PathComplete` back to `ClericShellMarker`.
- Known gaps: a 1 m grid can miss geometric traps that require sub-meter alignment, narrow squeeze ledges with no grid point landing on them, and mesh-gap pockets between samples.
- Complementary coverage: S3-05 AC-11 walkthrough evidence must cover human navigation and geometric edge cases this scan cannot prove.

## NavMesh Profile

- `surface_agent_type_id`: `0`
- `settings_source`: `NavMesh.GetSettingsByID(surface_agent_type_id)`
- `resolved_agent_type_id`: `0`
- `agent_radius`: `0.5`
- `agent_height`: `2`
- `agent_slope`: `45`
- `agent_climb`: `0.75`

## Grid Summary

- `total_samples`: `900`
- `on_mesh_samples`: `855`
- `off_mesh_samples`: `45`
- `reachable_samples`: `855`
- `trapped_samples`: `0`
- `incomplete_non_invalid_samples`: `0`

## Checks

- PASS `scene_loaded`
- PASS `first_district_greybox_root_exists`
- PASS `navmesh_surface_object_exists`
- PASS `navmesh_surface_component_exists`
- PASS `navmesh_surface_data_assigned`
- PASS `navmesh_surface_agent_settings_resolved`
- PASS `spawn_sampled_on_navmesh`
- PASS `scan_found_on_mesh_samples`
- PASS `zero_soft_lock_zones_detected`

## Trapped Samples

- None detected. Best-effort high-confidence result: zero soft-lock zones detected at 1 m grid sampling density.

## Warnings

- None captured during runner execution.

## Errors

- None captured during runner execution.
