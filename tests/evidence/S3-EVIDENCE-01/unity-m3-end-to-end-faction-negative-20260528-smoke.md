# S2-M3-04 Unity End-To-End Objective Loop Smoke

**Date:** 2026-05-29
**Story:** `production/stories/s2-m3-04-end-to-end-objective-loop.md`
**Evidence Patch:** `production/stories/s3-evidence-integrity-patch.md`
**Scene:** `Assets/Scenes/_DevEntry.unity`
**Runner:** `Assets/Editor/GravenspireM3EndToEndObjectiveLoopVerificationRunner.cs`
**Scene Preparation:** Existing runner builder chain rebuilds before play; AC-1 verifies faction-consequence structural absence in runtime code/components, not authored-scene preservation.
**Result:** PASS

## Checks

- PASS `scene_loaded`
- PASS `m3_caretaker_anchor_exists`
- PASS `m3_objective_root_exists`
- PASS `m3_objective_relic_exists`
- PASS `m3_loot_vendor_root_exists`
- PASS `m3_court_vendor_component_exists`
- PASS `m2_combat_camp_loop_root_exists`
- PASS `cleric_shell_marker_exists`
- PASS `npc_component_found_in_play_mode`
- PASS `objective_component_found_in_play_mode`
- PASS `vendor_component_found_in_play_mode`
- PASS `m2_controller_found_in_play_mode`
- PASS `cleric_marker_found_in_play_mode`
- PASS `objective_accepted_from_npc`
- PASS `relic_recovered`
- PASS `objective_loot_resolved`
- PASS `resolved_relic_carried`
- PASS `resolved_salvage_carried`
- PASS `relic_handed_in_to_npc`
- PASS `vendor_salvage_sale_succeeds`
- PASS `vendor_f4_formula_credits_7_copper`
- PASS `salvage_removed_after_sale`
- PASS `m2_controller_initialized_before_m2_smoke`
- PASS `m2_named_blocker_boundary_smoke_passes`
- PASS `m2_clean_loop_preserved_after_named_blocker`
- PASS `m2_controller_reported_no_errors`
- PASS `no_save_load_state_written`
- PASS `no_faction_consequence_scene_object_present`
- PASS `no_faction_consequence_component_present`
- PASS `no_faction_consequence_event_present`
- PASS `no_faction_consequence_event_fired`
- PASS `m5_vendor_reputation_discount_hook_absent`
- PASS `m5_vendor_faction_rank_goods_hook_absent`
- PASS `no_faction_consequence_applied`
- PASS `state_sequence_exact`

## End-To-End Objective Loop Telemetry

- npc_anchor_present=True
- npc_interaction_intentional=True
- dialogue_template_id=dialogue.m3.caretaker.objective_frame_t1
- relic_available=True
- loot_table_id=M3_ObjectiveNpcLoot_T1
- loot_result_item_ids=CourtMarkedRelic_T1|GraveDust_Salvage_T1
- objective_state_sequence=NotIntroduced -> Accepted -> RelicRecovered -> Complete
- relic_handed_in=True
- objective_complete=True
- vendor_salvage_sold=True
- vendor_sell_copper_applied=7
- m2_clean_loop_preserved=True
- m2_named_blocker_boundary_preserved=True
- no_save_load_state_written=True
- runner_local_fake_faction_consequence_enabled=False
- faction_consequence_scene_objects=none
- faction_consequence_components=none
- faction_consequence_events=none
- runner_local_fake_faction_consequence_events=0
- vendor_reputation_discount_hook=False
- vendor_faction_rank_goods_hook=False
- no_faction_consequence_applied=True

## Warnings

- None captured during runner execution.

## Errors

- None captured during runner execution.
