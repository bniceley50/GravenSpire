# S3-02 Verification

**Date:** 2026-05-29
**Story:** `production/stories/s3-02-player-driven-npc-interaction.md`
**Verdict:** PASS

## Summary

S3-02 adds a thin `M3NamedNpcInteractTarget` adapter on `M3_Caretaker`, maps the existing M3 NPC context into the S3-01 harness telemetry envelope, and preserves the S3-01 feedback event by recording the NPC target event before `interact_fired`. The scene delta is limited to attaching the adapter component to the existing `M3_Caretaker` object.

Raw Unity `.log` files were not committed because they contain local licensing and machine-environment lines. Each committed Unity evidence artifact below records the runner result, checks, warnings, and errors. The raw local Unity logs also show a Unity Search startup `ArgumentOutOfRangeException` before runner execution; the story runners exited 0 and captured no runner errors.

## Acceptance Criteria

| AC | Result | Evidence |
|---|---|---|
| S3-02-01 | PASS | `Assets/Scripts/M3NamedNpcInteractTarget.cs` implements the adapter; `_DevEntry.unity` attaches it to `M3_Caretaker`. Unity smoke checks `adapter_present_on_m3_caretaker` and `adapter_found_in_play_mode` PASS in `unity-player-driven-npc-interaction-20260529-smoke.md`. |
| S3-02-02 | PASS | Adapter calls `M3NamedNpcObjectiveFrame.TryRecordIntentionalInteraction(...)` and maps the result into `InteractContext`; Unity smoke checks `adapter_called_once_for_in_range`, `adapter_success_count_incremented`, `adapter_frame_reference_resolves`, and payload mapping checks PASS. |
| S3-02-03 | PASS | `m3-frame-zero-diff-20260529.txt` records `git diff --stat -- Assets/Scripts/M3NamedNpcObjectiveFrame.cs Assets/Scripts/M3NamedNpcObjectiveFrame.cs.meta` produced no output. |
| S3-02-04 | PASS | `unity-player-driven-npc-interaction-20260529-smoke.md` records `npc_interaction_intentional`, `source=player_driven`, `npc_id`, `player_actor_id`, `active_zone_id`, `interaction_state`, `interaction_kind`, `dialogue_template_set_id`, `objective_frame_text_key`, `was_intentional=true`, and `distance_meters=1.25`. |
| S3-02-05 | PASS | Unity smoke checks `no_dialogue_ui_scene_objects`, `no_forbidden_caretaker_affordances`, `no_dialogue_ui_after_interactions`, and `no_forbidden_caretaker_affordances_after_interactions` all PASS. Dialogue handles remain telemetry data only. |
| S3-02-06 | PASS | Unity smoke drives the harness dispatch path and checks `adapter_registered_with_harness`, `in_range_dispatch_returns_true`, `harness_records_target_event_before_feedback`, and `harness_last_outcome_fired` all PASS. |
| S3-02-07 | PASS | Unity smoke widens the harness test range, positions the player outside the M3 range, and checks `out_of_m3_range_dispatch_returns_false`, `harness_last_outcome_blocked`, `blocked_records_no_npc_target_event`, `blocked_records_interact_blocked`, and `blocked_feedback_has_no_routing_hint` all PASS. |

## Unity Evidence

| Gate | Result | Artifact |
|---|---|---|
| S3-02 player-driven NPC interaction smoke | PASS | `tests/evidence/S3-02/unity-player-driven-npc-interaction-20260529-smoke.md` |
| S3-01 harness regression smoke | PASS | `tests/evidence/S3-02/s3-01-harness-regression-20260529-smoke.md` |
| M2 single-trash preservation | PASS | `tests/evidence/S3-02/m2-02-preservation-20260529-smoke.md` |
| M2 linked-trash preservation | PASS | `tests/evidence/S3-02/m2-03-preservation-20260529-smoke.md` |
| M2 named-blocker preservation | PASS | `tests/evidence/S3-02/m2-04-preservation-20260529-smoke.md` |

The three M2 preservation smokes were run in separate Unity invocations with `-gravenspirePreservationMode -gravenspireSkipBuilder`; each artifact records `Builder Skipped: true` and `Builder Invoked: false`.

## Local Gates

| Gate | Result | Evidence |
|---|---|---|
| Combat regression | PASS | `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` passed 189/189. |
| Format gate - tests project | PASS | `dotnet format tests\Gravenspire.Combat.Tests.csproj --verify-no-changes --exclude-diagnostics IDE1006` exited 0. |
| Format gate - prototype harness | PASS | `dotnet format prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj --verify-no-changes --exclude-diagnostics IDE1006` exited 0. |
| Diff whitespace | PASS | `git diff --check` exited 0 after normalizing Unity-generated empty-`m_Name` trailing whitespace in `_DevEntry.unity`. |
| T1 negative-scope scan | PASS WITH CLASSIFIED RUNNER GUARD HITS | Scan over changed S3-02 implementation and evidence files found only deny-list terms inside the S3-02 verification runner's forbidden-affordance guard strings (`minimap`, `nameplate`, `glow`, `outline`, `get closer`). No runtime implementation, scene, or evidence payload introduced networking, server authority, live LLM, Dialogue UI, quest markers, minimap, objective signposts, or routing feedback. |

## Notes

- The S3-01 harness edit is intentionally narrow: successful dispatch now records the target telemetry context first, then records the `interact_fired` feedback context. The S3-01 regression smoke passed after this change.
- `Assets/Scripts/M3NamedNpcObjectiveFrame.cs` remains zero-diff for S3-02.
- The S3-01 regression runner mutates `_DevEntry.unity` through its historical builder; the scene was snapshotted before the regression and restored afterward so the committed scene delta remains the S3-02 adapter attachment only.
