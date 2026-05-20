# S2-M3-03 Unity Loot Table Fixed-Profile Vendor Smoke

**Date:** 2026-05-20
**Story:** `production/stories/s2-m3-03-loot-table-fixed-profile-vendor.md`
**Scene:** `Assets/Scenes/_DevEntry.unity`
**Runner:** `Assets/Editor/GravenspireM3LootTableFixedProfileVendorVerificationRunner.cs`
**Result:** PASS

## Checks

- PASS `scene_loaded`
- PASS `authored_data_file_exists`
- PASS `authored_data_file_valid`
- PASS `m3_vendor_marker_exists`
- PASS `m3_vendor_component_exists`
- PASS `exactly_one_m3_vendor_component`
- PASS `m3_objective_component_exists`
- PASS `m3_caretaker_anchor_exists`
- PASS `m2_loop_controller_exists`
- PASS `vendor_component_found_in_play_mode`
- PASS `vendor_loaded_authored_data_file`
- PASS `vendor_not_using_fallback_data`
- PASS `objective_component_found_in_play_mode`
- PASS `m3_caretaker_component_found_in_play_mode`
- PASS `cleric_marker_found_for_vendor_smoke`
- PASS `objective_accepts_before_loot_resolution`
- PASS `relic_recovered_before_loot_resolution`
- PASS `authored_loot_table_resolved`
- PASS `resolved_relic_carried`
- PASS `resolved_salvage_carried`
- PASS `loot_rng_seed_boundary_preserved`
- PASS `vendor_salvage_sale_succeeds`
- PASS `vendor_f4_formula_credits_7_copper`
- PASS `salvage_removed_after_sale`
- PASS `fixed_profile_purchase_succeeds_after_capacity_prevalidation`
- PASS `fixed_profile_purchase_debits_constant_price`
- PASS `fixed_vendor_good_recorded_in_session`
- PASS `vendor_session_local_only`
- PASS `vendor_no_dynamic_pricing_hook`
- PASS `vendor_no_stock_simulation_hook`
- PASS `vendor_no_reputation_discount_hook`
- PASS `vendor_no_rotation_hook`
- PASS `vendor_no_token_or_rank_goods_hook`
- PASS `vendor_no_arbitrage_hook`
- PASS `vendor_no_coin_faucet_projection_claim`
- PASS `vendor_no_currency_at_rest_persistence`
- PASS `no_vendor_rejection_reason`
- PASS `m2_controller_found_after_vendor_changes`
- PASS `m2_controller_initialized_after_vendor_changes`

## Vendor Telemetry

- loot_table_id=M3_ObjectiveNpcLoot_T1
- vendor_id=M3_CourtVendor_T1
- authored_data_path=N:\GravenSpire\data\first-district\m3-objective-npc-loot.json
- authored_data_file_loaded=True
- objective_state_before_vendor=RelicRecovered
- resolved_relic_carried=True
- salvage_sale_credited_copper=7
- purchase_debited_copper=3
- ending_carried_currency_copper=4
- ending_carried_slots=2

## Warnings

- None captured during runner execution.

## Errors

- None captured during runner execution.
