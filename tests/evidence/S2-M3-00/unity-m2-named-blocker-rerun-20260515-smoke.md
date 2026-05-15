# S2-M2-04 Unity Named-Blocker Camp-Boundary Smoke

**Date:** 2026-05-15
**Story:** `production/stories/s2-m2-04-named-blocker-camp-boundary.md`
**Scene:** `Assets/Scenes/_DevEntry.unity`
**Runner:** `Assets/Editor/GravenspireM2NamedBlockerVerificationRunner.cs`
**Result:** PASS

## Checks

- PASS `scene_loaded`
- PASS `camp_rest_point_exists`
- PASS `pull_lane_exists`
- PASS `cleric_marker_exists`
- PASS `baseline_trash_anchor_exists`
- PASS `linked_trash_anchor_exists`
- PASS `named_blocker_anchor_exists`
- PASS `loop_controller_exists`
- PASS `controller_found_in_play_mode`
- PASS `named_blocker_boundary_smoke_completed`
- PASS `named_blocker_anchor_present`
- PASS `named_blocker_present_and_targetable`
- PASS `named_blocker_distinct_from_trash_fixture`
- PASS `named_blocker_boundary_outcome_recorded`
- PASS `named_blocker_not_farmable_trash`
- PASS `clean_single_trash_loop_preserved`
- PASS `no_controller_errors`

## Named Blocker Telemetry

- named_blocker_outcome=forced_flee_threshold
- named_hostile_fixture=NamedSoloBlock_T1
- named_max_health=520
- baseline_trash_max_health=120
- time_to_danger_seconds=26.00
- ending_health=40/220
- ending_mana=260/300
- named_ending_health=330/520
- clean_loop_preserved=True

## Named Blocker Events

- 0:named_fixture:NamedSoloBlock_T1
- 0:named_max_health:520 baseline_trash_max_health:120
- 0:named_present_targetable:m2-named-hostile-1
- 0:smite_resolved_named
- 130:named_melee_hit:18
- 140:player_melee_hit_named:14
- 260:named_melee_hit:18
- 280:player_melee_hit_named:14
- 350:smite_resolved_named
- 390:named_melee_hit:18
- 420:player_melee_hit_named:14
- 520:named_melee_hit:18
- 560:player_melee_hit_named:14
- 650:named_melee_hit:18
- 700:smite_resolved_named
- 700:player_melee_hit_named:14
- 780:named_melee_hit:18
- 840:player_melee_hit_named:14
- 910:named_melee_hit:18
- 980:player_melee_hit_named:14
- 1040:named_melee_hit:18
- 1050:smite_resolved_named
- 1120:player_melee_hit_named:14
- 1170:named_melee_hit:18
- 1260:player_melee_hit_named:14
- 1300:named_melee_hit:18
- 1300:outcome:forced_flee_threshold
- 1300:time_to_danger_seconds:26.00
- 1300:ending_health:40/220
- 1300:ending_mana:260/300
- 1300:named_ending_health:330/520

## Clean Loop Events

- 0:loop_initialized
- 0:loop_reset
- 0:pull_blocked_while_sitting
- 0:pull_start
- 0:attack_blocked_without_target
- 0:target_selected
- 0:smite_blocked_without_attack
- 0:attack_on
- 0:smite_resolved
- 140:player_melee_hit:10
- 150:hostile_melee_hit:7
- 280:player_melee_hit:10
- 300:hostile_melee_hit:7
- 400:smite_resolved
- 420:player_melee_hit:10
- 450:hostile_melee_hit:7
- 560:player_melee_hit:10
- 600:hostile_melee_hit:7
- 700:player_melee_hit:10
- 750:hostile_melee_hit:7
- 840:player_melee_hit:10
- 900:hostile_melee_hit:7
- 980:player_melee_hit:10
- 1050:hostile_melee_hit:7
- 1120:player_melee_hit:10
- 1200:hostile_melee_hit:7
- 1260:player_melee_hit:10
- 1260:attack_off_target_death
- 1260:hostile_defeat
- 1260:combat_exit
- 1260:sit_med_start
- 1261:mana_restored:8
- 1561:mana_restored:8
- 1660:stand
- 1660:baseline_trash_respawned
- 1660:manual_repeat_ready
- 1660:pull_start
- 1660:target_selected
- 1660:attack_on
- 1660:smite_resolved
- 1800:player_melee_hit:10
- 1810:hostile_melee_hit:7
- 1940:player_melee_hit:10
- 1960:hostile_melee_hit:7
- 2060:smite_resolved
- 2080:player_melee_hit:10
- 2110:hostile_melee_hit:7
- 2220:player_melee_hit:10
- 2260:hostile_melee_hit:7
- 2360:player_melee_hit:10
- 2410:hostile_melee_hit:7
- 2500:player_melee_hit:10
- 2560:hostile_melee_hit:7
- 2640:player_melee_hit:10
- 2710:hostile_melee_hit:7
- 2780:player_melee_hit:10
- 2860:hostile_melee_hit:7
- 2920:player_melee_hit:10
- 2920:attack_off_target_death
- 2920:hostile_defeat
- 2920:combat_exit
- 2920:sit_med_start
- 2921:mana_restored:8
- 3221:mana_restored:8
- 3320:stand

## Warnings

- None captured during runner execution.

## Errors

- None captured during runner execution.
