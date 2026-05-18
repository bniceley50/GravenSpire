# S2-M3-01 Unity Named NPC Objective Frame Smoke

**Date:** 2026-05-18
**Story:** `production/stories/s2-m3-01-named-npc-objective-frame.md`
**Scene:** `Assets/Scenes/_DevEntry.unity`
**Runner:** `Assets/Editor/GravenspireM3NamedNpcObjectiveFrameVerificationRunner.cs`
**Result:** PASS

## Checks

- PASS `scene_loaded`
- PASS `m3_named_npc_anchor_exists`
- PASS `exactly_one_m3_named_npc_component`
- PASS `npc_anchor_visible_renderer`
- PASS `npc_interaction_trigger_exists`
- PASS `no_marker_affordance_components`
- PASS `m2_camp_rest_point_exists`
- PASS `m2_baseline_trash_exists`
- PASS `m2_linked_trash_exists`
- PASS `m2_named_blocker_exists`
- PASS `m2_loop_controller_exists`
- PASS `npc_component_found_in_play_mode`
- PASS `cleric_marker_found_for_interaction`
- PASS `intentional_interaction_recorded`
- PASS `npc_interaction_context_shape_recorded`
- PASS `templated_dialogue_handle_recorded`
- PASS `interaction_state_is_interacting`
- PASS `session_local_no_persistence_claim`
- PASS `templated_dialogue_only`
- PASS `m2_controller_found_for_preservation`
- PASS `m2_controller_initialized_for_preservation`
- PASS `m2_linked_anchor_available`
- PASS `m2_named_blocker_anchor_available`

## NPC Interaction Context

- npc_id=M3_Caretaker_T1
- player_actor_id=m3-player-cleric
- active_zone_id=Haunt_Prototype_T1
- interaction_state=Interacting
- interaction_kind=IntentionalPlayerInteraction
- dialogue_template_set_id=dialogue.m3.caretaker.objective_frame_t1
- objective_frame_text_key=m3.objective.recover_marked_relic.frame
- distance_meters=1.25

## M2 Preservation Telemetry

- full_m2_preservation=external_runner_reruns_under_tests/evidence/S2-M3-01
- m2_controller_initialized=True
- m2_linked_anchor_available=True
- m2_named_blocker_anchor_available=True

## Warnings

- None captured during runner execution.

## Errors

- None captured during runner execution.
