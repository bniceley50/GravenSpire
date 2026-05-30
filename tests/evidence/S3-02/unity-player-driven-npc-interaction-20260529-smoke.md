# S3-02 Unity Player-Driven NPC Interaction Smoke

**Date:** 2026-05-29
**Story:** `production/stories/s3-02-player-driven-npc-interaction.md`
**Scene:** `Assets/Scenes/_DevEntry.unity`
**Runner:** `Assets/Editor/GravenspireS3PlayerDrivenNpcInteractionVerificationRunner.cs`
**Result:** PASS

## Checks

- PASS `scene_loaded`
- PASS `harness_root_exists`
- PASS `m3_caretaker_anchor_exists`
- PASS `adapter_present_on_m3_caretaker`
- PASS `m3_objective_frame_present_on_m3_caretaker`
- PASS `no_dialogue_ui_scene_objects`
- PASS `no_forbidden_caretaker_affordances`
- PASS `harness_component_found_in_play_mode`
- PASS `cleric_marker_found_in_play_mode`
- PASS `caretaker_found_in_play_mode`
- PASS `adapter_found_in_play_mode`
- PASS `frame_found_in_play_mode`
- PASS `adapter_registered_with_harness`
- PASS `adapter_frame_reference_resolves`
- PASS `in_range_dispatch_returns_true`
- PASS `adapter_called_once_for_in_range`
- PASS `adapter_success_count_incremented`
- PASS `frame_recorded_one_interaction`
- PASS `harness_last_outcome_fired`
- PASS `harness_records_target_event_before_feedback`
- PASS `npc_interaction_context_shape_recorded`
- PASS `target_telemetry_maps_npc_identity`
- PASS `target_telemetry_maps_player_actor`
- PASS `target_telemetry_maps_dialogue_handle`
- PASS `target_telemetry_maps_objective_text_key`
- PASS `target_telemetry_marks_player_driven_source`
- PASS `target_telemetry_marks_intentional_amount`
- PASS `equal_to_m3_range_is_in_range`
- PASS `adapter_called_once_for_boundary`
- PASS `boundary_records_target_event`
- PASS `out_of_m3_range_dispatch_returns_false`
- PASS `adapter_called_once_for_out_of_range`
- PASS `adapter_success_count_not_incremented_for_out_of_range`
- PASS `frame_records_no_out_of_range_interaction`
- PASS `harness_last_outcome_blocked`
- PASS `blocked_records_no_npc_target_event`
- PASS `blocked_records_interact_blocked`
- PASS `blocked_feedback_has_no_routing_hint`
- PASS `no_dialogue_ui_after_interactions`
- PASS `no_forbidden_caretaker_affordances_after_interactions`

## Player-Driven NPC Telemetry

- in_range.telemetry_event=npc_interaction_intentional
- in_range.source=player_driven
- in_range.npc_id=M3_Caretaker_T1
- in_range.player_actor_id=m3-player-cleric
- in_range.active_zone_id=Haunt_Prototype_T1
- in_range.interaction_state=Interacting
- in_range.interaction_kind=IntentionalPlayerInteraction
- in_range.dialogue_template_set_id=dialogue.m3.caretaker.objective_frame_t1
- in_range.objective_frame_text_key=m3.objective.recover_marked_relic.frame
- in_range.was_intentional=true
- in_range.distance_meters=1.25
- in_range.feedback_event_follows_target_event=interact_fired
- boundary_distance_meters=2
- out_of_m3_range_distance_meters=2.25
- blocked_feedback=Interact blocked

## Warnings

- None captured during runner execution.

## Errors

- None captured during runner execution.
