# S2-M2-03 Unity Linked-Trash Overpull Smoke

**Date:** 2026-05-29
**Story:** `production/stories/s2-m2-03-linked-trash-overpull.md`
**Scene:** `Assets/Scenes/_DevEntry.unity`
**Runner:** `Assets/Editor/GravenspireM2LinkedTrashOverpullVerificationRunner.cs`
**Result:** PASS
**Preservation Mode:** true
**Builder Skipped:** true
**Builder Invoked:** false

## Evidence Metadata

- preservation_mode=true
- builder_skipped=true
- builder_invoked=false

## AC-2 Guard Control Evidence

These control runs were executed before the final preservation PASS artifact overwrote this file.

- Negative control: same runner invoked with `-gravenspirePreservationMode` and without `-gravenspireSkipBuilder`; observed artifact result `FAIL`, `preservation_mode=true`, `builder_skipped=false`, `builder_invoked=false`, failing check `preservation_mode_requires_skip_builder`, and error `Preservation mode requires -gravenspireSkipBuilder; no builder call was executed.`
- Normal control: same runner invoked without either preservation flag; observed artifact result `PASS`, `preservation_mode=false`, `builder_skipped=false`, `builder_invoked=true`.
- Final preservation run: current artifact was invoked with `-gravenspirePreservationMode -gravenspireSkipBuilder`; observed artifact result `PASS`, `preservation_mode=true`, `builder_skipped=true`, `builder_invoked=false`.

## Checks

- PASS `preservation_mode_requires_skip_builder`
- PASS `scene_loaded`
- PASS `camp_rest_point_exists`
- PASS `pull_lane_exists`
- PASS `cleric_marker_exists`
- PASS `baseline_trash_anchor_exists`
- PASS `linked_trash_anchor_exists`
- PASS `loop_controller_exists`
- PASS `controller_found_in_play_mode`
- PASS `linked_overpull_smoke_completed`
- PASS `linked_trash_arrangement_present`
- PASS `two_hostiles_entered_hate`
- PASS `feel03_hate_window_met`
- PASS `dangerous_outcome_recorded`
- PASS `clean_single_trash_loop_preserved`
- PASS `no_controller_errors`

## Overpull Telemetry

- hostiles_in_hate=2
- hate_window_seconds=0.0
- overpull_outcome=forced_flee_threshold
- ending_health=14/140
- ending_mana=150/180
- clean_loop_preserved=True

## Overpull Events

- 0:bad_pull_primary_hate:m2-linked-hostile-1
- 0:bad_pull_linked_hate:m2-linked-hostile-2
- 0:hate_window_seconds:0.0
- 0:smite_resolved_primary
- 140:player_melee_hit_primary:10
- 150:primary_trash_melee_hit:10
- 150:linked_trash_melee_hit:11
- 280:player_melee_hit_primary:10
- 300:primary_trash_melee_hit:10
- 300:linked_trash_melee_hit:11
- 350:smite_resolved_primary
- 420:player_melee_hit_primary:10
- 450:primary_trash_melee_hit:10
- 450:linked_trash_melee_hit:11
- 560:player_melee_hit_primary:10
- 600:primary_trash_melee_hit:10
- 600:linked_trash_melee_hit:11
- 700:smite_resolved_primary
- 700:player_melee_hit_primary:10
- 750:primary_trash_melee_hit:10
- 750:linked_trash_melee_hit:11
- 840:player_melee_hit_primary:10
- 900:primary_trash_melee_hit:10
- 900:linked_trash_melee_hit:11
- 900:outcome:forced_flee_threshold
- 900:ending_health:14/140
- 900:ending_mana:150/180

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
