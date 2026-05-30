# S3-04 Unity Player-Driven Vendor Smoke

**Date:** 2026-05-30
**Story:** `production/stories/s3-04-player-driven-vendor.md`
**Scene:** `Assets/Scenes/_DevEntry.unity`
**Runner:** `Assets/Editor/GravenspireS3PlayerDrivenVendorVerificationRunner.cs`
**Result:** PASS

## Checks

- PASS `scene_loaded`
- PASS `harness_root_exists`
- PASS `m3_caretaker_anchor_exists`
- PASS `m3_objective_relic_exists`
- PASS `m3_court_vendor_exists`
- PASS `vendor_adapter_present_on_m3_court_vendor`
- PASS `buy_side_absent_from_vendor_adapter_source`
- PASS `no_vendor_buy_side_scene_affordances`
- PASS `harness_component_found_in_play_mode`
- PASS `cleric_marker_found_in_play_mode`
- PASS `caretaker_found_in_play_mode`
- PASS `objective_component_found_in_play_mode`
- PASS `relic_object_found_in_play_mode`
- PASS `vendor_component_found_in_play_mode`
- PASS `npc_adapter_found_in_play_mode`
- PASS `relic_adapter_found_in_play_mode`
- PASS `vendor_adapter_found_in_play_mode`
- PASS `frame_found_in_play_mode`
- PASS `vendor_adapter_reference_resolves`
- PASS `vendor_adapter_registered_with_harness`
- PASS `vendor_session_starts_without_salvage`
- PASS `vendor_session_is_t1_local_only`
- PASS `vendor_authority_annotation_present`
- PASS `t3_blocked_fresh_vendor_dispatch_returns_false`
- PASS `t3_blocked_fresh_vendor_adapter_called_once`
- PASS `t3_blocked_fresh_vendor_no_sale_event`
- PASS `t3_blocked_fresh_vendor_no_copper_event`
- PASS `t3_blocked_fresh_vendor_currency_unchanged`
- PASS `t3_blocked_fresh_vendor_rejection_reason_captured`
- PASS `t3_blocked_fresh_vendor_adapter_rejection_reason_captured`
- PASS `t3_blocked_fresh_vendor_harness_outcome_blocked`
- PASS `t3_blocked_fresh_vendor_feedback_has_no_diagnostic_hint`
- PASS `t2_success_setup_accept_returns_true`
- PASS `t2_success_setup_recover_returns_true`
- PASS `t2_success_setup_state_relic_recovered`
- PASS `t2_success_setup_vendor_carries_salvage`
- PASS `t2_success_setup_objective_loot_resolved_event_present`
- PASS `t2_sale_dispatch_returns_true`
- PASS `t2_sale_adapter_called_once`
- PASS `t2_sale_success_count_incremented`
- PASS `t2_sale_credited_copper_positive`
- PASS `t2_sale_currency_exact`
- PASS `t2_sale_slots_decrease_by_one`
- PASS `t2_sale_salvage_quantity_decrements`
- PASS `t2_sale_single_salvage_now_absent`
- PASS `t2_sale_event_order`
- PASS `t2_sale_copper_event_before_feedback`
- PASS `t2_sale_event_payload_vendor_id`
- PASS `t2_sale_event_payload_salvage_id`
- PASS `t2_sale_event_payload_quantity_one`
- PASS `t2_sale_event_payload_source`
- PASS `t2_copper_event_payload_vendor_id`
- PASS `t2_copper_event_payload_credited_amount`
- PASS `t2_copper_event_payload_new_currency`
- PASS `t2_sale_harness_outcome_fired`
- PASS `t2_sale_feedback_mentions_copper_result`
- PASS `t2_sale_feedback_has_no_buy_side_hint`
- PASS `t2_sale_no_buy_side_runtime_event`
- PASS `t3_blocked_after_sell_all_returns_false`
- PASS `t3_blocked_after_sell_all_no_sale_event`
- PASS `t3_blocked_after_sell_all_no_copper_event`
- PASS `t3_blocked_after_sell_all_currency_unchanged`
- PASS `t3_blocked_after_sell_all_feedback_has_no_diagnostic_hint`
- PASS `t6_end_to_end_accept_returns_true`
- PASS `t6_end_to_end_recover_returns_true`
- PASS `t6_end_to_end_sell_returns_true`
- PASS `t6_end_to_end_salvage_sold`
- PASS `t6_end_to_end_copper_applied`
- PASS `t6_end_to_end_harness_outcome_fired`
- PASS `t6_end_to_end_full_vendor_vocabulary_order`
- PASS `t6_end_to_end_no_route_hint_feedback`
- PASS `buy_side_absent_from_vendor_adapter_source_after_play`
- PASS `no_vendor_buy_side_scene_affordances_after_interactions`
- PASS `feedback_rule_forbidden_text_absent`

## Vendor Adapter Telemetry Shapes

- Sale success: `vendor_salvage_sold` then `vendor_sell_copper_applied`; harness outcome `Fired`; feedback is `+N copper`.
- Sale blocked: no vendor target events; harness outcome `Blocked`; rejection reason remains internal telemetry/debug data.

## Player-Driven Vendor Telemetry

- t3_blocked_fresh_vendor.last_outcome=Blocked
- t3_blocked_fresh_vendor.feedback=Interact blocked
- t3_blocked_fresh_vendor.event_sequence=interact_blocked
- t3_blocked_fresh_vendor.interact_blocked=M3_CourtVendor|m3-player-cleric||||amount:0|feedback:|1.25
- t2_success_setup.last_outcome=Fired
- t2_success_setup.feedback=Interacted
- t2_success_setup.event_sequence=npc_interaction_intentional>objective_accepted>interact_fired>relic_recovered>objective_loot_resolved>interact_fired
- t2_success_setup.npc_interaction_intentional=M3_Caretaker_T1|m3-player-cleric|npc_interaction_context:player_driven|dialogue.m3.caretaker.objective_frame_t1|m3.objective.recover_marked_relic.frame|amount:1|feedback:|1.25
- t2_success_setup.objective_accepted=M3_Caretaker_T1|m3-player-cleric|objective_transition:player_driven|NotIntroduced|Accepted|amount:0|feedback:|1.25
- t2_success_setup.interact_fired=M3_Caretaker_T1|m3-player-cleric|npc_interaction_context:player_driven|dialogue.m3.caretaker.objective_frame_t1|m3.objective.recover_marked_relic.frame|amount:1|feedback:|1.25
- t2_success_setup.relic_recovered=M3_ObjectiveRelic|m3-player-cleric|relic_recovery:player_driven|CourtMarkedRelic_T1|Accepted->RelicRecovered|amount:0|feedback:|1.25
- t2_success_setup.objective_loot_resolved=M3_CourtVendor_T1|m3-player-cleric|objective_loot:player_driven|M3_ObjectiveNpcLoot_T1|CourtMarkedRelic_T1|GraveDust_Salvage_T1|amount:0|feedback:|1.25
- t2_success_setup.interact_fired=M3_ObjectiveRelic|m3-player-cleric|relic_recovery:player_driven|CourtMarkedRelic_T1|Accepted->RelicRecovered|amount:0|feedback:|1.25
- t2_sale_success.last_outcome=Fired
- t2_sale_success.feedback=+7 copper
- t2_sale_success.event_sequence=vendor_salvage_sold>vendor_sell_copper_applied>interact_fired
- t2_sale_success.vendor_salvage_sold=M3_CourtVendor_T1|m3-player-cleric|vendor_salvage_sale:player_driven|GraveDust_Salvage_T1|player_driven|amount:1|feedback:+7 copper|1.25
- t2_sale_success.vendor_sell_copper_applied=M3_CourtVendor_T1|m3-player-cleric|vendor_sell_copper:player_driven|7|7|amount:7|feedback:+7 copper|1.25
- t2_sale_success.interact_fired=M3_CourtVendor_T1|m3-player-cleric|vendor_salvage_sale:player_driven|GraveDust_Salvage_T1|player_driven|amount:1|feedback:+7 copper|1.25
- t3_blocked_after_sell_all.last_outcome=Blocked
- t3_blocked_after_sell_all.feedback=Interact blocked
- t3_blocked_after_sell_all.event_sequence=interact_blocked
- t3_blocked_after_sell_all.interact_blocked=M3_CourtVendor|m3-player-cleric||||amount:0|feedback:|1.25
- t6_end_to_end.last_outcome=Fired
- t6_end_to_end.feedback=+7 copper
- t6_end_to_end.event_sequence=npc_interaction_intentional>objective_accepted>interact_fired>relic_recovered>objective_loot_resolved>interact_fired>vendor_salvage_sold>vendor_sell_copper_applied>interact_fired
- t6_end_to_end.npc_interaction_intentional=M3_Caretaker_T1|m3-player-cleric|npc_interaction_context:player_driven|dialogue.m3.caretaker.objective_frame_t1|m3.objective.recover_marked_relic.frame|amount:1|feedback:|1.25
- t6_end_to_end.objective_accepted=M3_Caretaker_T1|m3-player-cleric|objective_transition:player_driven|NotIntroduced|Accepted|amount:0|feedback:|1.25
- t6_end_to_end.interact_fired=M3_Caretaker_T1|m3-player-cleric|npc_interaction_context:player_driven|dialogue.m3.caretaker.objective_frame_t1|m3.objective.recover_marked_relic.frame|amount:1|feedback:|1.25
- t6_end_to_end.relic_recovered=M3_ObjectiveRelic|m3-player-cleric|relic_recovery:player_driven|CourtMarkedRelic_T1|Accepted->RelicRecovered|amount:0|feedback:|1.25
- t6_end_to_end.objective_loot_resolved=M3_CourtVendor_T1|m3-player-cleric|objective_loot:player_driven|M3_ObjectiveNpcLoot_T1|CourtMarkedRelic_T1|GraveDust_Salvage_T1|amount:0|feedback:|1.25
- t6_end_to_end.interact_fired=M3_ObjectiveRelic|m3-player-cleric|relic_recovery:player_driven|CourtMarkedRelic_T1|Accepted->RelicRecovered|amount:0|feedback:|1.25
- t6_end_to_end.vendor_salvage_sold=M3_CourtVendor_T1|m3-player-cleric|vendor_salvage_sale:player_driven|GraveDust_Salvage_T1|player_driven|amount:1|feedback:+7 copper|1.25
- t6_end_to_end.vendor_sell_copper_applied=M3_CourtVendor_T1|m3-player-cleric|vendor_sell_copper:player_driven|7|7|amount:7|feedback:+7 copper|1.25
- t6_end_to_end.interact_fired=M3_CourtVendor_T1|m3-player-cleric|vendor_salvage_sale:player_driven|GraveDust_Salvage_T1|player_driven|amount:1|feedback:+7 copper|1.25

## Warnings

- None captured during runner execution.

## Errors

- None captured during runner execution.
