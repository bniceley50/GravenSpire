# S3-05 Unity End-To-End In-District Smoke

**Date:** 2026-05-26
**Story:** `production/stories/s3-05-navigable-greybox-first-district.md`
**Scene:** `Assets/Scenes/_DevEntry.unity`
**Runner:** `Assets/Editor/GravenspireS3DistrictCompositeObjectiveLoopVerificationRunner.cs`
**Result:** PASS_WITH_NOTES
**AC-12 status:** Partial pass by story-defined graceful-degradation semantics.

## Scope

- Asserted: S3-01 harness dispatch works inside the S3-05 district after a NavMesh-complete walked path from spawn toward `M3_Caretaker`.
- Deferred: S3-02/03/04 player-driven adapters and full objective-loop telemetry, because those stories were not closed at S3-05 Phase 6 implementation time.
- Rollforward: full AC-12 assertion belongs to S3-06; the `s3_05_ac12_partial_rollforward_to_s3_06` carryover is deferred to Phase 8 closure artifacts.

## Checks

- PASS `scene_loaded`
- PASS `first_district_greybox_root_exists`
- PASS `harness_root_exists`
- PASS `cleric_shell_marker_exists`
- PASS `m3_caretaker_anchor_exists`
- PASS `m3_objective_relic_anchor_exists`
- PASS `m3_court_vendor_anchor_exists`
- PASS `navmesh_surface_ready`
- PASS `harness_component_found_in_play_mode`
- PASS `cleric_marker_found_in_play_mode`
- PASS `navmesh_surface_found_in_play_mode`
- PASS `navmesh_surface_data_assigned_in_play_mode`
- PASS `spawn_sampled_on_navmesh`
- PASS `caretaker_anchor_found_in_play_mode`
- PASS `caretaker_sampled_on_navmesh`
- PASS `spawn_to_caretaker_path_complete`
- PASS `spawn_to_caretaker_has_walked_distance`
- PASS `harness_prompt_visible_at_partial_probe`
- PASS `s3_01_harness_dispatch_returns_true_inside_district`
- PASS `partial_probe_target_called_once`
- PASS `partial_probe_receives_player_actor`
- PASS `partial_probe_distance_within_harness_range`
- PASS `harness_records_interact_fired`

## Partial Dispatch Telemetry

- scripted_on_foot_path_status=PathComplete
- scripted_on_foot_path_length_meters=2.154
- scripted_on_foot_path_corners=2
- partial_probe_distance_meters=1
- asserted_partial_vocabulary=interact_fired|s3_05_partial_probe_dispatched
- full_chain_vocabulary_deferred=npc_interaction_intentional|objective_accepted|relic_recovered|objective_loot_resolved|vendor_salvage_sold|vendor_sell_copper_applied|relic_handed_in
- harness_last_event=interact_fired
- harness_last_feedback=Interacted
- probe_target_event=s3_05_partial_probe_dispatched
- probe_target_payload_source=runner_only_partial_ac12_probe

## Downstream Adapter Envelope

- S3-01 status: closed; S3 player interaction harness is present and asserted in this runner.
- S3-02 status at S3-05 Phase 6: ready, not closed; NPC adapter vocabulary is catalogued but not asserted.
- S3-03 status at S3-05 Phase 6: blocked on S3-02; relic/objective adapter vocabulary is catalogued but not asserted.
- S3-04 status at S3-05 Phase 6: blocked on S3-03; vendor adapter vocabulary is catalogued but not asserted.
- AC-12 closure semantics: partial-pass rolls full-chain assertion forward to S3-06; production/sprint-status.yaml carryover is a Phase 8 closure artifact.
- Observed downstream adapter count before runner probe: 0.
- `M3_Caretaker` has closed adapter component: False.
- `M3_ObjectiveRelic` has closed adapter component: False.
- `M3_CourtVendor` has closed adapter component: False.

## Warnings

- None captured during runner execution.

## Errors

- None captured during runner execution.
