# S3-05 First District Reachability Smoke

**Date:** 2026-05-29
**Story:** `production/stories/s3-05-navigable-greybox-first-district.md`
**Scene:** `Assets/Scenes/_DevEntry.unity`
**Runner:** `Assets/Editor/GravenspireS3DistrictReachabilityVerificationRunner.cs`
**Result:** PASS

## Composability Precondition

- `precondition_artifact_required`: `navmesh-bake-scope`
- `bake_scope_artifact_path`: `tests/evidence/S3-EVIDENCE-01/navmesh-bake-scope-footprint-20260528-smoke.md`
- `enforcement`: `visibility-only`
- This block names the bake-scope footprint artifact this reachability evidence depends on. It is a visibility stub for future composability: it does NOT verify artifact freshness, file hashes, scene hashes, or NavMesh asset identity. No Tier 2 composability framework is implied (D003 — Tier 1 single-player offline).

## NavMesh Profile

- `surface_agent_type_id`: `0`
- `settings_source`: `NavMesh.GetSettingsByID(surface_agent_type_id)`
- `resolved_agent_type_id`: `0`
- `agent_radius`: `0.5`
- `agent_height`: `2`
- `agent_slope`: `45`
- `agent_climb`: `0.75`

## Checks

- PASS `scene_loaded`
- PASS `first_district_greybox_root_exists`
- PASS `navmesh_surface_object_exists`
- PASS `navmesh_surface_component_exists`
- PASS `navmesh_surface_data_assigned`
- PASS `navmesh_surface_agent_settings_resolved`
- PASS `spawn_sampled_on_navmesh`
- PASS `M3_Caretaker_exists`
- PASS `M3_Caretaker_sampled_on_navmesh`
- PASS `M3_Caretaker_path_complete`
- PASS `M3_ObjectiveRelic_exists`
- PASS `M3_ObjectiveRelic_sampled_on_navmesh`
- PASS `M3_ObjectiveRelic_path_complete`
- PASS `M3_CourtVendor_exists`
- PASS `M3_CourtVendor_sampled_on_navmesh`
- PASS `M3_CourtVendor_path_complete`
- PASS `all_m3_anchors_reachable`
- PASS `m3_objective_relic_restored_inactive`

## Anchor Reachability

| Anchor | Initial activeSelf | Sample distance (m) | Path status | Path length (m) | Max elevation delta (m) | Result |
|---|---:|---:|---|---:|---:|---|
| `M3_Caretaker` | True | 0.917 | PathComplete | 4.652 | 0 | PASS |
| `M3_ObjectiveRelic` | False | 0.267 | PathComplete | 3.653 | 0 | PASS |
| `M3_CourtVendor` | True | 0.537 | PathComplete | 5.207 | 0 | PASS |

## Warnings

- None captured during runner execution.

## Errors

- None captured during runner execution.
