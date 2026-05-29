# S3-05 NavMesh Bake Scope Smoke

**Date:** 2026-05-29
**Story:** `production/stories/s3-05-navigable-greybox-first-district.md`
**Scene:** `Assets/Scenes/_DevEntry.unity`
**Runner:** `Assets/Editor/GravenspireS3DistrictNavMeshBakeScopeVerificationRunner.cs`
**Result:** PASS
**Negative Control Mode:** false
**Agent Radius (m):** 0.5

## Evidence Metadata

- negative_control_mode=false
- agent_radius_meters=0.5
- expected_result=PASS

## Methodology

- Static intent check: verify `FirstDistrict_NavMeshSurface` uses `CollectObjects.Children`, `PhysicsColliders`, matching layer mask, enabled colliders, and declared collection-scope inclusion for floor, 3 landmark massings, and 4 boundary walls.
- Static carve-mechanism check: verify all 3 landmark massings and 4 boundary walls carry an active `NavMeshModifierVolume` with area `1` (Not Walkable), local center `(0, 0, 0)`, and local size `(1, 1, 1)`.
- Runtime outcome check: at each obstacle ground center, `NavMesh.SamplePosition` with radius 0.3 m must fail, proving no NavMesh exists inside the obstacle footprint.
- Runtime perimeter check: a wider 3 m probe must resolve to a horizontally displaced point at least as far as the obstacle's minimum X/Z half-footprint, proving the nearest NavMesh sits at the obstacle perimeter rather than inside it.
- Runtime anchor-clearance check: at each gameplay anchor ground center, `NavMesh.SamplePosition` with radius 0.3 m must succeed with horizontal displacement no greater than 0.3 m, proving anchors remain on walkable NavMesh after obstacle carving.
- Runtime footprint-coverage check: across each obstacle footprint a grid of ground-plane probe points (corners, edge midpoints, and interior samples inset by the agent radius 0.5 m, stepped at 1 m and capped at 13 samples/axis) must each fail a 0.3 m `NavMesh.SamplePosition`, proving the entire footprint — not just its center — is carved. Footprint axes thinner than the agent radius collapse to their centerline with a recorded skip reason; long thin boundary walls are therefore covered along their full length.

## Surface Scope

- `surface_object`: `FirstDistrict_Greybox/FirstDistrict_NavMeshSurface`
- `collect_objects`: `Children`
- `use_geometry`: `PhysicsColliders`
- `layer_mask`: `-1`
- `surface_center`: `(0, 0, 0)`
- `surface_size`: `(30, 4, 30)`
- `world_volume_bounds`: `center=(0, 0, 0), size=(30, 4, 30), min=(-15, -2, -15), max=(15, 2, 15)` (informational; ignored by `CollectObjects.Children`)

## Checks

- PASS `scene_loaded`
- PASS `first_district_greybox_root_exists`
- PASS `navmesh_surface_object_exists`
- PASS `navmesh_surface_component_exists`
- PASS `surface_collects_children`
- PASS `surface_uses_physics_colliders`
- PASS `surface_transform_rotation_identity`
- PASS `surface_transform_scale_identity`
- PASS `DevEntry_DistrictBlockout_Floor_exists`
- PASS `DevEntry_DistrictBlockout_Floor_has_enabled_collider`
- PASS `DevEntry_DistrictBlockout_Floor_layer_in_surface_mask`
- PASS `DevEntry_DistrictBlockout_Floor_included_in_declared_bake_scope`
- PASS `Greybox_CaretakerHall_Massing_exists`
- PASS `Greybox_CaretakerHall_Massing_has_enabled_collider`
- PASS `Greybox_CaretakerHall_Massing_layer_in_surface_mask`
- PASS `Greybox_CaretakerHall_Massing_included_in_declared_bake_scope`
- PASS `Greybox_CaretakerHall_Massing_has_not_walkable_modifier_volume`
- PASS `Greybox_CourtVendorHall_Massing_exists`
- PASS `Greybox_CourtVendorHall_Massing_has_enabled_collider`
- PASS `Greybox_CourtVendorHall_Massing_layer_in_surface_mask`
- PASS `Greybox_CourtVendorHall_Massing_included_in_declared_bake_scope`
- PASS `Greybox_CourtVendorHall_Massing_has_not_walkable_modifier_volume`
- PASS `Greybox_RelicStorehouse_Massing_exists`
- PASS `Greybox_RelicStorehouse_Massing_has_enabled_collider`
- PASS `Greybox_RelicStorehouse_Massing_layer_in_surface_mask`
- PASS `Greybox_RelicStorehouse_Massing_included_in_declared_bake_scope`
- PASS `Greybox_RelicStorehouse_Massing_has_not_walkable_modifier_volume`
- PASS `GreyboxBoundary_North_exists`
- PASS `GreyboxBoundary_North_has_enabled_collider`
- PASS `GreyboxBoundary_North_layer_in_surface_mask`
- PASS `GreyboxBoundary_North_included_in_declared_bake_scope`
- PASS `GreyboxBoundary_North_has_not_walkable_modifier_volume`
- PASS `GreyboxBoundary_South_exists`
- PASS `GreyboxBoundary_South_has_enabled_collider`
- PASS `GreyboxBoundary_South_layer_in_surface_mask`
- PASS `GreyboxBoundary_South_included_in_declared_bake_scope`
- PASS `GreyboxBoundary_South_has_not_walkable_modifier_volume`
- PASS `GreyboxBoundary_East_exists`
- PASS `GreyboxBoundary_East_has_enabled_collider`
- PASS `GreyboxBoundary_East_layer_in_surface_mask`
- PASS `GreyboxBoundary_East_included_in_declared_bake_scope`
- PASS `GreyboxBoundary_East_has_not_walkable_modifier_volume`
- PASS `GreyboxBoundary_West_exists`
- PASS `GreyboxBoundary_West_has_enabled_collider`
- PASS `GreyboxBoundary_West_layer_in_surface_mask`
- PASS `GreyboxBoundary_West_included_in_declared_bake_scope`
- PASS `GreyboxBoundary_West_has_not_walkable_modifier_volume`
- PASS `all_required_objects_in_declared_bake_scope`
- PASS `all_required_objects_have_enabled_colliders`
- PASS `navmesh_surface_data_assigned`
- PASS `agent_radius_resolved`
- PASS `Greybox_CaretakerHall_Massing_runtime_obstacle_exists`
- PASS `Greybox_CaretakerHall_Massing_runtime_collider_bounds_resolved`
- PASS `Greybox_CaretakerHall_Massing_tight_probe_fails_inside_obstacle`
- PASS `Greybox_CaretakerHall_Massing_wide_probe_resolves_near_perimeter`
- PASS `Greybox_CaretakerHall_Massing_wide_probe_displacement_matches_footprint`
- PASS `Greybox_CourtVendorHall_Massing_runtime_obstacle_exists`
- PASS `Greybox_CourtVendorHall_Massing_runtime_collider_bounds_resolved`
- PASS `Greybox_CourtVendorHall_Massing_tight_probe_fails_inside_obstacle`
- PASS `Greybox_CourtVendorHall_Massing_wide_probe_resolves_near_perimeter`
- PASS `Greybox_CourtVendorHall_Massing_wide_probe_displacement_matches_footprint`
- PASS `Greybox_RelicStorehouse_Massing_runtime_obstacle_exists`
- PASS `Greybox_RelicStorehouse_Massing_runtime_collider_bounds_resolved`
- PASS `Greybox_RelicStorehouse_Massing_tight_probe_fails_inside_obstacle`
- PASS `Greybox_RelicStorehouse_Massing_wide_probe_resolves_near_perimeter`
- PASS `Greybox_RelicStorehouse_Massing_wide_probe_displacement_matches_footprint`
- PASS `GreyboxBoundary_North_runtime_obstacle_exists`
- PASS `GreyboxBoundary_North_runtime_collider_bounds_resolved`
- PASS `GreyboxBoundary_North_tight_probe_fails_inside_obstacle`
- PASS `GreyboxBoundary_North_wide_probe_resolves_near_perimeter`
- PASS `GreyboxBoundary_North_wide_probe_displacement_matches_footprint`
- PASS `GreyboxBoundary_South_runtime_obstacle_exists`
- PASS `GreyboxBoundary_South_runtime_collider_bounds_resolved`
- PASS `GreyboxBoundary_South_tight_probe_fails_inside_obstacle`
- PASS `GreyboxBoundary_South_wide_probe_resolves_near_perimeter`
- PASS `GreyboxBoundary_South_wide_probe_displacement_matches_footprint`
- PASS `GreyboxBoundary_East_runtime_obstacle_exists`
- PASS `GreyboxBoundary_East_runtime_collider_bounds_resolved`
- PASS `GreyboxBoundary_East_tight_probe_fails_inside_obstacle`
- PASS `GreyboxBoundary_East_wide_probe_resolves_near_perimeter`
- PASS `GreyboxBoundary_East_wide_probe_displacement_matches_footprint`
- PASS `GreyboxBoundary_West_runtime_obstacle_exists`
- PASS `GreyboxBoundary_West_runtime_collider_bounds_resolved`
- PASS `GreyboxBoundary_West_tight_probe_fails_inside_obstacle`
- PASS `GreyboxBoundary_West_wide_probe_resolves_near_perimeter`
- PASS `GreyboxBoundary_West_wide_probe_displacement_matches_footprint`
- PASS `all_obstacle_tight_probes_fail`
- PASS `all_obstacle_wide_probes_resolve_displaced`
- PASS `Greybox_CaretakerHall_Massing_footprint_fully_carved`
- PASS `Greybox_CourtVendorHall_Massing_footprint_fully_carved`
- PASS `Greybox_RelicStorehouse_Massing_footprint_fully_carved`
- PASS `GreyboxBoundary_North_footprint_fully_carved`
- PASS `GreyboxBoundary_South_footprint_fully_carved`
- PASS `GreyboxBoundary_East_footprint_fully_carved`
- PASS `GreyboxBoundary_West_footprint_fully_carved`
- PASS `all_obstacle_footprints_fully_carved`
- PASS `ClericShellMarker_anchor_exists`
- PASS `ClericShellMarker_anchor_probe_resolves_on_navmesh`
- PASS `ClericShellMarker_anchor_probe_low_horizontal_displacement`
- PASS `M3_Caretaker_anchor_exists`
- PASS `M3_Caretaker_anchor_probe_resolves_on_navmesh`
- PASS `M3_Caretaker_anchor_probe_low_horizontal_displacement`
- PASS `M3_CourtVendor_anchor_exists`
- PASS `M3_CourtVendor_anchor_probe_resolves_on_navmesh`
- PASS `M3_CourtVendor_anchor_probe_low_horizontal_displacement`
- PASS `M3_ObjectiveRelic_anchor_exists`
- PASS `M3_ObjectiveRelic_anchor_probe_resolves_on_navmesh`
- PASS `M3_ObjectiveRelic_anchor_probe_low_horizontal_displacement`
- PASS `M2_CampRestPoint_anchor_exists`
- PASS `M2_CampRestPoint_anchor_probe_resolves_on_navmesh`
- PASS `M2_CampRestPoint_anchor_probe_low_horizontal_displacement`
- PASS `M2_BaselineTrash_anchor_exists`
- PASS `M2_BaselineTrash_anchor_probe_resolves_on_navmesh`
- PASS `M2_BaselineTrash_anchor_probe_low_horizontal_displacement`
- PASS `M2_LinkedTrash_anchor_exists`
- PASS `M2_LinkedTrash_anchor_probe_resolves_on_navmesh`
- PASS `M2_LinkedTrash_anchor_probe_low_horizontal_displacement`
- PASS `M2_NamedBlocker_anchor_exists`
- PASS `M2_NamedBlocker_anchor_probe_resolves_on_navmesh`
- PASS `M2_NamedBlocker_anchor_probe_low_horizontal_displacement`
- PASS `M2_PullLane_anchor_exists`
- PASS `M2_PullLane_anchor_probe_resolves_on_navmesh`
- PASS `M2_PullLane_anchor_probe_low_horizontal_displacement`
- PASS `all_anchor_tight_probes_resolve_on_navmesh`
- PASS `all_anchor_tight_probes_low_displacement`

## Static Bake-Scope Assertions

| Object | Role | Active | Layer | Enabled colliders | Layer included | In declared scope | Not-walkable volume | Details |
|---|---|---:|---:|---:|---:|---:|---:|---|
| `DevEntry_DistrictBlockout_Floor` | walkable floor | True | 0 | 1 | True | True | n/a | Object is a descendant of FirstDistrict_NavMeshSurface. Not required for this bake object. |
| `Greybox_CaretakerHall_Massing` | landmark obstacle | True | 0 | 1 | True | True | PASS | Object is a descendant of FirstDistrict_NavMeshSurface. area=1, center=(0, 0, 0), size=(1, 1, 1), enabled=True |
| `Greybox_CourtVendorHall_Massing` | landmark obstacle | True | 0 | 1 | True | True | PASS | Object is a descendant of FirstDistrict_NavMeshSurface. area=1, center=(0, 0, 0), size=(1, 1, 1), enabled=True |
| `Greybox_RelicStorehouse_Massing` | landmark obstacle | True | 0 | 1 | True | True | PASS | Object is a descendant of FirstDistrict_NavMeshSurface. area=1, center=(0, 0, 0), size=(1, 1, 1), enabled=True |
| `GreyboxBoundary_North` | boundary obstacle | True | 0 | 1 | True | True | PASS | Object is a descendant of FirstDistrict_NavMeshSurface. area=1, center=(0, 0, 0), size=(1, 1, 1), enabled=True |
| `GreyboxBoundary_South` | boundary obstacle | True | 0 | 1 | True | True | PASS | Object is a descendant of FirstDistrict_NavMeshSurface. area=1, center=(0, 0, 0), size=(1, 1, 1), enabled=True |
| `GreyboxBoundary_East` | boundary obstacle | True | 0 | 1 | True | True | PASS | Object is a descendant of FirstDistrict_NavMeshSurface. area=1, center=(0, 0, 0), size=(1, 1, 1), enabled=True |
| `GreyboxBoundary_West` | boundary obstacle | True | 0 | 1 | True | True | PASS | Object is a descendant of FirstDistrict_NavMeshSurface. area=1, center=(0, 0, 0), size=(1, 1, 1), enabled=True |

## Runtime Obstacle-Carve Assertions

| Obstacle | Query ground center | Tight probe | Wide resolved position | Wide horizontal displacement (m) | Required displacement (m) | Result |
|---|---|---|---|---:|---:|---|
| `Greybox_CaretakerHall_Massing` | (3, 0, -7) | not sampled (expected) | (1, 0.083, -7) | 2 | 1.5 | PASS |
| `Greybox_CourtVendorHall_Massing` | (6, 0, -6) | not sampled (expected) | (6, 0.083, -8.5) | 2.5 | 2 | PASS |
| `Greybox_RelicStorehouse_Massing` | (-3, 0, 9) | not sampled (expected) | (-5.5, 0.083, 9) | 2.5 | 2 | PASS |
| `GreyboxBoundary_North` | (0, 0, 15) | not sampled (expected) | (0, 0.083, 14.167) | 0.833 | 0.25 | PASS |
| `GreyboxBoundary_South` | (0, 0, -15) | not sampled (expected) | (0, 0.083, -14.167) | 0.833 | 0.25 | PASS |
| `GreyboxBoundary_East` | (15, 0, 0) | not sampled (expected) | (14.167, 0.083, 0) | 0.833 | 0.25 | PASS |
| `GreyboxBoundary_West` | (-15, 0, 0) | not sampled (expected) | (-14.167, 0.083, 0) | 0.833 | 0.25 | PASS |

## Runtime Obstacle-Footprint Coverage Assertions

| Obstacle | Footprint bounds | Agent radius (m) | Probe points | Carved probes | Uncarved probes | Axis notes | Result |
|---|---|---:|---:|---:|---:|---|---|
| `Greybox_CaretakerHall_Massing` | center=(3, 4, -7), size=(3, 8, 4), min=(1.5, 0, -9), max=(4.5, 8, -5) | 0.5 | 12 | 12 | 0 | Both footprint axes exceeded the agent radius; full inset grid probed. | PASS |
| `Greybox_CourtVendorHall_Massing` | center=(6, 2, -6), size=(6, 4, 4), min=(3, 0, -8), max=(9, 4, -4) | 0.5 | 24 | 24 | 0 | Both footprint axes exceeded the agent radius; full inset grid probed. | PASS |
| `Greybox_RelicStorehouse_Massing` | center=(-3, 3, 9), size=(4, 6, 4), min=(-5, 0, 7), max=(-1, 6, 11) | 0.5 | 16 | 16 | 0 | Both footprint axes exceeded the agent radius; full inset grid probed. | PASS |
| `GreyboxBoundary_North` | center=(0, 0.5, 15), size=(30, 1, 0.5), min=(-15, 0, 14.75), max=(15, 1, 15.25) | 0.5 | 13 | 13 | 0 | Footprint z-axis half-extent 0.25 m <= agent radius 0.5 m; probed along centerline only. | PASS |
| `GreyboxBoundary_South` | center=(0, 0.5, -15), size=(30, 1, 0.5), min=(-15, 0, -15.25), max=(15, 1, -14.75) | 0.5 | 13 | 13 | 0 | Footprint z-axis half-extent 0.25 m <= agent radius 0.5 m; probed along centerline only. | PASS |
| `GreyboxBoundary_East` | center=(15, 0.5, 0), size=(0.5, 1, 30), min=(14.75, 0, -15), max=(15.25, 1, 15) | 0.5 | 13 | 13 | 0 | Footprint x-axis half-extent 0.25 m <= agent radius 0.5 m; probed along centerline only. | PASS |
| `GreyboxBoundary_West` | center=(-15, 0.5, 0), size=(0.5, 1, 30), min=(-15.25, 0, -15), max=(-14.75, 1, 15) | 0.5 | 13 | 13 | 0 | Footprint x-axis half-extent 0.25 m <= agent radius 0.5 m; probed along centerline only. | PASS |

## Runtime Anchor-Clearance Assertions

| Anchor | Query ground center | Probe radius (m) | Resolved position | Horizontal displacement (m) | Vertical displacement (m) | Result |
|---|---|---:|---|---:|---:|---|
| `ClericShellMarker` | (0, 0, 0) | 0.3 | (0, 0.083, 0) | 0 | 0.083 | PASS |
| `M3_Caretaker` | (2, 0, -4.2) | 0.3 | (2, 0.083, -4.2) | 0 | 0.083 | PASS |
| `M3_CourtVendor` | (4, 0, -3.6) | 0.3 | (4, 0.083, -3.333) | 0.267 | 0.083 | PASS |
| `M3_ObjectiveRelic` | (-1.85, 0, 3.15) | 0.3 | (-1.85, 0.083, 3.15) | 0 | 0.083 | PASS |
| `M2_CampRestPoint` | (0, 0, -5) | 0.3 | (0, 0.083, -5) | 0 | 0.083 | PASS |
| `M2_BaselineTrash` | (0, 0, 4) | 0.3 | (0, 0.083, 4) | 0 | 0.083 | PASS |
| `M2_LinkedTrash` | (2.3, 0, 4.8) | 0.3 | (2.3, 0.083, 4.8) | 0 | 0.083 | PASS |
| `M2_NamedBlocker` | (-2.8, 0, 5.6) | 0.3 | (-2.8, 0.083, 5.6) | 0 | 0.083 | PASS |
| `M2_PullLane` | (0, 0, -0.5) | 0.3 | (0, 0.083, -0.5) | 0 | 0.083 | PASS |

## Warnings

- None captured during runner execution.

## Errors

- None captured during runner execution.
