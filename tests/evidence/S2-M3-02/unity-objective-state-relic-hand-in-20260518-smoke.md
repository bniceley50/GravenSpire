# S2-M3-02 Unity Objective State Relic Hand-In Smoke

**Date:** 2026-05-18
**Story:** `production/stories/s2-m3-02-objective-state-relic-hand-in.md`
**Scene:** `Assets/Scenes/_DevEntry.unity`
**Runner:** `Assets/Editor/GravenspireM3ObjectiveStateRelicHandInVerificationRunner.cs`
**Result:** PASS

## Checks

- PASS `scene_loaded`
- PASS `m3_caretaker_anchor_exists`
- PASS `m3_objective_component_exists`
- PASS `exactly_one_m3_objective_component`
- PASS `m3_relic_object_authored`
- PASS `m3_relic_starts_unavailable`
- PASS `m3_relic_has_trigger_collider`
- PASS `no_marker_affordance_components`
- PASS `m2_camp_rest_point_exists`
- PASS `m2_baseline_trash_exists`
- PASS `m2_linked_trash_exists`
- PASS `m2_named_blocker_exists`
- PASS `m2_loop_controller_exists`
- PASS `objective_component_found_in_play_mode`
- PASS `m3_caretaker_component_found_in_play_mode`
- PASS `initial_state_not_introduced`
- PASS `initial_relic_unavailable`
- PASS `cleric_marker_found_for_objective_interaction`
- PASS `objective_accept_transition_recorded`
- PASS `relic_available_after_accept`
- PASS `relic_recovery_transition_recorded`
- PASS `session_carried_relic_recorded`
- PASS `relic_unavailable_after_recovery`
- PASS `npc_relic_hand_in_recorded`
- PASS `objective_complete_after_hand_in`
- PASS `state_sequence_exact`
- PASS `transition_count_exact`
- PASS `objective_session_local_only`
- PASS `no_objective_rejection_reason`
- PASS `m2_controller_found_after_objective_changes`
- PASS `m2_controller_initialized_after_objective_changes`
- PASS `m2_linked_anchor_available_after_objective_changes`
- PASS `m2_named_blocker_anchor_available_after_objective_changes`

## Objective Telemetry

- objective_state_sequence=NotIntroduced -> Accepted -> RelicRecovered -> Complete
- relic_available_after_accept=True
- session_carried_relic_recorded=True
- relic_handed_in=True
- objective_complete=True
- session_local_only=True

## M2 Preservation Telemetry

- full_m2_preservation=external_runner_reruns_under_tests/evidence/S2-M3-02
- m2_controller_initialized=True
- m2_linked_anchor_available=True
- m2_named_blocker_anchor_available=True

## Warnings

- None captured during runner execution.

## Errors

- None captured during runner execution.
