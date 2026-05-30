# S3-03 S3-02 NPC Interaction Regression Smoke

**Date:** 2026-05-30
**Story:** `production/stories/s3-03-player-relic-recovery-and-looting.md`
**Regression Target:** `production/stories/s3-02-player-driven-npc-interaction.md` S3-02-T2, rerun in Accepted-state re-talk routing.
**Runner:** `Assets/Editor/GravenspireS3PlayerRelicRecoveryAndLootingVerificationRunner.cs`
**Result:** PASS

## Checks

- PASS `s3_02_t2_accepted_retalk_returns_true`
- PASS `s3_02_t2_npc_interaction_intentional_present`
- PASS `s3_02_t2_player_driven_source_preserved`
- PASS `s3_02_t2_feedback_fired`

## Telemetry

- accepted_retalk.telemetry_event=npc_interaction_intentional
- accepted_retalk.source=player_driven
- accepted_retalk.npc_id=M3_Caretaker_T1
- accepted_retalk.player_actor_id=m3-player-cleric
- accepted_retalk.feedback_event=interact_fired

## Notes

- Regression path intentionally starts from `Accepted`, matching S3-03 AC-02: the expanded adapter still records `npc_interaction_intentional` and `interact_fired` for S3-02-style re-talk.
