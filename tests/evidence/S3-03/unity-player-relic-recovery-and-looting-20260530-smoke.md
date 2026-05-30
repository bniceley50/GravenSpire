# S3-03 Unity Player Relic Recovery + Looting Smoke

**Date:** 2026-05-30
**Story:** `production/stories/s3-03-player-relic-recovery-and-looting.md`
**Scene:** `Assets/Scenes/_DevEntry.unity`
**Runner:** `Assets/Editor/GravenspireS3PlayerRelicRecoveryAndLootingVerificationRunner.cs`
**Result:** PASS

## Checks

- PASS `scene_loaded`
- PASS `harness_root_exists`
- PASS `m3_caretaker_anchor_exists`
- PASS `m3_objective_root_exists`
- PASS `m3_objective_relic_exists`
- PASS `m3_court_vendor_exists`
- PASS `npc_adapter_present_on_caretaker`
- PASS `relic_adapter_present_on_relic`
- PASS `no_dialogue_or_route_ui_scene_objects`
- PASS `harness_component_found_in_play_mode`
- PASS `cleric_marker_found_in_play_mode`
- PASS `caretaker_found_in_play_mode`
- PASS `objective_component_found_in_play_mode`
- PASS `relic_object_found_in_play_mode`
- PASS `vendor_component_found_in_play_mode`
- PASS `npc_adapter_found_in_play_mode`
- PASS `relic_adapter_found_in_play_mode`
- PASS `frame_found_in_play_mode`
- PASS `npc_adapter_frame_reference_resolves`
- PASS `npc_adapter_state_reference_resolves`
- PASS `relic_adapter_state_reference_resolves`
- PASS `relic_adapter_vendor_reference_resolves`
- PASS `fresh_state_not_introduced`
- PASS `fresh_relic_inactive`
- PASS `t1_accept_dispatch_returns_true`
- PASS `t1_accept_state_accepted`
- PASS `t1_accept_relic_became_active`
- PASS `t1_accept_harness_outcome_fired`
- PASS `t1_accept_npc_event_before_objective_accepted`
- PASS `t1_accept_accept_payload_from_state`
- PASS `t1_accept_accept_payload_to_state`
- PASS `t1_accept_accept_source_player_driven`
- PASS `t2_accepted_retalk_returns_true`
- PASS `t2_accepted_retalk_state_unchanged`
- PASS `t2_accepted_retalk_npc_event_present`
- PASS `t2_accepted_retalk_no_objective_accepted_refire`
- PASS `t2_accepted_retalk_player_driven_payload`
- PASS `t2_accepted_retalk_harness_outcome_fired`
- PASS `t3_blocked_relic_dispatch_returns_false`
- PASS `t3_blocked_state_still_not_introduced`
- PASS `t3_blocked_no_relic_recovered_event`
- PASS `t3_blocked_interact_blocked_feedback`
- PASS `t3_blocked_feedback_has_no_routing_hint`
- PASS `t3_partial_dispatch_returns_true`
- PASS `t3_partial_state_relic_recovered`
- PASS `t3_partial_relic_inactive_after_recovery`
- PASS `t3_partial_relic_recovered_event_present`
- PASS `t3_partial_loot_failed_event_present`
- PASS `t3_partial_no_loot_resolved_event`
- PASS `t3_partial_harness_outcome_fired`
- PASS `t3_success_accept_setup_dispatch_returns_true`
- PASS `t3_success_accept_setup_state_accepted`
- PASS `t3_success_accept_setup_relic_became_active`
- PASS `t3_success_accept_setup_harness_outcome_fired`
- PASS `t3_success_accept_setup_npc_event_before_objective_accepted`
- PASS `t3_success_accept_setup_accept_payload_from_state`
- PASS `t3_success_accept_setup_accept_payload_to_state`
- PASS `t3_success_accept_setup_accept_source_player_driven`
- PASS `t3_success_relic_dispatch_returns_true`
- PASS `t3_success_state_relic_recovered`
- PASS `t3_success_relic_inactive_after_recovery`
- PASS `t3_success_relic_carried_in_vendor_inventory`
- PASS `t3_success_salvage_carried_in_vendor_inventory`
- PASS `t3_success_currency_unchanged`
- PASS `t3_success_relic_before_loot_resolved`
- PASS `t3_success_harness_outcome_fired`
- PASS `t4_hand_in_dispatch_returns_true`
- PASS `t4_hand_in_state_complete`
- PASS `t4_hand_in_event_present`
- PASS `t4_hand_in_no_npc_interaction_event`
- PASS `t4_hand_in_harness_outcome_fired`
- PASS `t5_complete_retalk_returns_true`
- PASS `t5_complete_retalk_state_unchanged`
- PASS `t5_complete_retalk_npc_event_present`
- PASS `t5_complete_retalk_no_hand_in_refire`
- PASS `t7_end_to_end_accept_returns_true`
- PASS `t7_end_to_end_recover_returns_true`
- PASS `t7_end_to_end_hand_in_returns_true`
- PASS `t7_end_to_end_state_complete`
- PASS `t7_end_to_end_state_sequence_exact`
- PASS `t7_end_to_end_relic_carried`
- PASS `t7_end_to_end_salvage_carried`
- PASS `t7_end_to_end_full_target_vocabulary_order`
- PASS `t7_end_to_end_no_route_hint_feedback`
- PASS `no_dialogue_or_route_ui_after_interactions`
- PASS `feedback_rule_forbidden_text_absent`

## Relic Adapter Telemetry Shapes

- Full success: `relic_recovered` then `objective_loot_resolved`; harness outcome `Fired`.
- Partial success: `relic_recovered` then `objective_loot_resolution_failed`; harness outcome `Fired`; objective remains `RelicRecovered`.
- Blocked: no relic/loot target events; harness outcome `Blocked`.

## Player-Driven Objective Telemetry

- t1_accept.last_outcome=Fired
- t1_accept.event_sequence=npc_interaction_intentional>objective_accepted>interact_fired
- t1_accept.npc_interaction_intentional=M3_Caretaker_T1|m3-player-cleric|npc_interaction_context:player_driven|dialogue.m3.caretaker.objective_frame_t1|m3.objective.recover_marked_relic.frame|1.25
- t1_accept.objective_accepted=M3_Caretaker_T1|m3-player-cleric|objective_transition:player_driven|NotIntroduced|Accepted|1.25
- t1_accept.interact_fired=M3_Caretaker_T1|m3-player-cleric|npc_interaction_context:player_driven|dialogue.m3.caretaker.objective_frame_t1|m3.objective.recover_marked_relic.frame|1.25
- t2_accepted_retalk.last_outcome=Fired
- t2_accepted_retalk.event_sequence=npc_interaction_intentional>interact_fired
- t2_accepted_retalk.npc_interaction_intentional=M3_Caretaker_T1|m3-player-cleric|npc_interaction_context:player_driven|dialogue.m3.caretaker.objective_frame_t1|m3.objective.recover_marked_relic.frame|1.25
- t2_accepted_retalk.interact_fired=M3_Caretaker_T1|m3-player-cleric|npc_interaction_context:player_driven|dialogue.m3.caretaker.objective_frame_t1|m3.objective.recover_marked_relic.frame|1.25
- t3_blocked.last_outcome=Blocked
- t3_blocked.event_sequence=interact_blocked
- t3_blocked.interact_blocked=M3_ObjectiveRelic|m3-player-cleric||||1.25
- t3_partial.last_outcome=Fired
- t3_partial.event_sequence=relic_recovered>objective_loot_resolution_failed>interact_fired
- t3_partial.relic_recovered=M3_ObjectiveRelic|m3-player-cleric|relic_recovery:player_driven|CourtMarkedRelic_T1|Accepted->RelicRecovered|1.25
- t3_partial.objective_loot_resolution_failed=M3_CourtVendor_T1|m3-player-cleric|objective_loot:player_driven|M3_ObjectiveNpcLoot_T1|M3 loot/vendor component is missing.|1.25
- t3_partial.interact_fired=M3_ObjectiveRelic|m3-player-cleric|relic_recovery:player_driven|CourtMarkedRelic_T1|Accepted->RelicRecovered|1.25
- t3_success_accept_setup.last_outcome=Fired
- t3_success_accept_setup.event_sequence=npc_interaction_intentional>objective_accepted>interact_fired
- t3_success_accept_setup.npc_interaction_intentional=M3_Caretaker_T1|m3-player-cleric|npc_interaction_context:player_driven|dialogue.m3.caretaker.objective_frame_t1|m3.objective.recover_marked_relic.frame|1.25
- t3_success_accept_setup.objective_accepted=M3_Caretaker_T1|m3-player-cleric|objective_transition:player_driven|NotIntroduced|Accepted|1.25
- t3_success_accept_setup.interact_fired=M3_Caretaker_T1|m3-player-cleric|npc_interaction_context:player_driven|dialogue.m3.caretaker.objective_frame_t1|m3.objective.recover_marked_relic.frame|1.25
- t3_success_recover.last_outcome=Fired
- t3_success_recover.event_sequence=relic_recovered>objective_loot_resolved>interact_fired
- t3_success_recover.relic_recovered=M3_ObjectiveRelic|m3-player-cleric|relic_recovery:player_driven|CourtMarkedRelic_T1|Accepted->RelicRecovered|1.25
- t3_success_recover.objective_loot_resolved=M3_CourtVendor_T1|m3-player-cleric|objective_loot:player_driven|M3_ObjectiveNpcLoot_T1|CourtMarkedRelic_T1|GraveDust_Salvage_T1|1.25
- t3_success_recover.interact_fired=M3_ObjectiveRelic|m3-player-cleric|relic_recovery:player_driven|CourtMarkedRelic_T1|Accepted->RelicRecovered|1.25
- t4_hand_in.last_outcome=Fired
- t4_hand_in.event_sequence=relic_handed_in>interact_fired
- t4_hand_in.relic_handed_in=M3_Caretaker_T1|m3-player-cleric|objective_transition:player_driven|RelicRecovered|Complete|1.25
- t4_hand_in.interact_fired=M3_Caretaker_T1|m3-player-cleric|objective_transition:player_driven|RelicRecovered|Complete|1.25
- t5_complete_retalk.last_outcome=Fired
- t5_complete_retalk.event_sequence=npc_interaction_intentional>interact_fired
- t5_complete_retalk.npc_interaction_intentional=M3_Caretaker_T1|m3-player-cleric|npc_interaction_context:player_driven|dialogue.m3.caretaker.objective_frame_t1|m3.objective.recover_marked_relic.frame|1.25
- t5_complete_retalk.interact_fired=M3_Caretaker_T1|m3-player-cleric|npc_interaction_context:player_driven|dialogue.m3.caretaker.objective_frame_t1|m3.objective.recover_marked_relic.frame|1.25
- t7_end_to_end.last_outcome=Fired
- t7_end_to_end.event_sequence=npc_interaction_intentional>objective_accepted>interact_fired>relic_recovered>objective_loot_resolved>interact_fired>relic_handed_in>interact_fired
- t7_end_to_end.npc_interaction_intentional=M3_Caretaker_T1|m3-player-cleric|npc_interaction_context:player_driven|dialogue.m3.caretaker.objective_frame_t1|m3.objective.recover_marked_relic.frame|1.25
- t7_end_to_end.objective_accepted=M3_Caretaker_T1|m3-player-cleric|objective_transition:player_driven|NotIntroduced|Accepted|1.25
- t7_end_to_end.interact_fired=M3_Caretaker_T1|m3-player-cleric|npc_interaction_context:player_driven|dialogue.m3.caretaker.objective_frame_t1|m3.objective.recover_marked_relic.frame|1.25
- t7_end_to_end.relic_recovered=M3_ObjectiveRelic|m3-player-cleric|relic_recovery:player_driven|CourtMarkedRelic_T1|Accepted->RelicRecovered|1.25
- t7_end_to_end.objective_loot_resolved=M3_CourtVendor_T1|m3-player-cleric|objective_loot:player_driven|M3_ObjectiveNpcLoot_T1|CourtMarkedRelic_T1|GraveDust_Salvage_T1|1.25
- t7_end_to_end.interact_fired=M3_ObjectiveRelic|m3-player-cleric|relic_recovery:player_driven|CourtMarkedRelic_T1|Accepted->RelicRecovered|1.25
- t7_end_to_end.relic_handed_in=M3_Caretaker_T1|m3-player-cleric|objective_transition:player_driven|RelicRecovered|Complete|1.25
- t7_end_to_end.interact_fired=M3_Caretaker_T1|m3-player-cleric|objective_transition:player_driven|RelicRecovered|Complete|1.25

## Warnings

- None captured during runner execution.

## Errors

- None captured during runner execution.
