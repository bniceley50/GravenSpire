# S3-01 Unity Player Interaction Harness Smoke

**Date:** 2026-05-25
**Story:** `production/stories/s3-01-standalone-player-interaction-harness.md`
**Scene:** `Assets/Scenes/_DevEntry.unity`
**Runner:** `Assets/Editor/GravenspireS3PlayerInteractionHarnessVerificationRunner.cs`
**Result:** PASS

## Checks

- PASS `scene_loaded`
- PASS `harness_component_exists`
- PASS `exactly_one_harness_component`
- PASS `harness_root_exists`
- PASS `m2_loop_controller_exists`
- PASS `harness_distinct_from_m2_controller`
- PASS `cleric_marker_exists`
- PASS `harness_component_found_in_play_mode`
- PASS `cleric_marker_found_in_play_mode`
- PASS `m2_controller_found_for_preservation_signal`
- PASS `m2_controller_initialized_for_preservation_signal`
- PASS `prompt_hidden_with_no_targets`
- PASS `missed_path_returns_false`
- PASS `missed_path_records_telemetry`
- PASS `missed_feedback_is_acknowledgement_only`
- PASS `fired_path_returns_true`
- PASS `fired_target_called_once`
- PASS `fired_target_receives_player_actor`
- PASS `fired_target_receives_measured_distance`
- PASS `fired_path_records_telemetry`
- PASS `fired_feedback_is_acknowledgement_only`
- PASS `nearest_dispatch_returns_true`
- PASS `nearest_target_called_once`
- PASS `farther_target_not_called`
- PASS `blocked_path_returns_false`
- PASS `blocked_target_called_once`
- PASS `blocked_path_records_telemetry`
- PASS `blocked_feedback_is_acknowledgement_only`
- PASS `target_just_past_threshold_is_missed`
- PASS `prompt_visible_at_zero_distance`
- PASS `prompt_visible_at_threshold`
- PASS `prompt_hidden_beyond_threshold`
- PASS `prompt_text_has_no_locator_terms`
- PASS `harness_did_not_record_m3_npc_interaction`

## Harness Telemetry

- m2_controller_initialized=True
- missed=event:interact_missed|target:|distance:0.00|feedback:Interact missed
- fired=event:interact_fired|target:mock-fired-target|distance:1.25|feedback:Interacted
- nearest=event:interact_fired|target:mock-near-target|distance:0.80|feedback:Interacted
- blocked=event:interact_blocked|target:S3_MockBlockedTarget|distance:1.00|feedback:Interact blocked
- just_past_threshold=event:interact_missed|target:|distance:0.00|feedback:Interact missed
- configured_interact_range_meters=2.00
- m2_preservation_external_reruns_required=true

## Warnings

- None captured during runner execution.

## Errors

- None captured during runner execution.
