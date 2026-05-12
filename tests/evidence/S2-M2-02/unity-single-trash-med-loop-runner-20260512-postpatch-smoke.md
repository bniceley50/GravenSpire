# S2-M2-02 Unity Single-Trash Med-Loop Smoke (Post-Patch Guard Evidence)

**Date:** 2026-05-12
**Story:** `production/stories/s2-m2-02-single-trash-pull-med-loop.md`
**Scene:** `Assets/Scenes/_DevEntry.unity`
**Runner:** `Assets/Editor/GravenspireM2SingleTrashLoopVerificationRunner.cs`
**Result:** PASS

## Checks

- PASS `scene_loaded`
- PASS `camp_rest_point_exists`
- PASS `pull_lane_exists`
- PASS `cleric_marker_exists`
- PASS `baseline_trash_anchor_exists`
- PASS `loop_controller_exists`
- PASS `controller_found_in_play_mode`
- PASS `two_pull_med_loop_smoke_completed`
- PASS `pull_start_recorded`
- PASS `pull_did_not_auto_enable_attack`
- PASS `attack_transition_recorded`
- PASS `hostile_defeat_recorded`
- PASS `combat_exit_recorded`
- PASS `sit_med_start_recorded`
- PASS `mana_restoration_recorded`
- PASS `no_controller_errors`

## Loop Events

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
