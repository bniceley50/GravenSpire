# S3-03 Verification

**Story:** `production/stories/s3-03-player-relic-recovery-and-looting.md`
**Branch:** `codex/s3-03-player-relic-recovery-and-looting`
**Date:** 2026-05-30
**Verdict:** PASS

S3-03 wires the existing M3 objective and loot/vendor systems behind the S3 player interaction harness without modifying either protected M3 runtime file. The player-driven sequence is Caretaker accept -> relic recover + loot resolution -> Caretaker hand-in, with the partial-success relic loot edge case recorded explicitly.

## Implementation Surface

| File | Purpose |
|---|---|
| `Assets/Scripts/S3PlayerInteractionHarness.cs` | Adds multi-event target telemetry support and refreshes active scene targets so the relic adapter can register after the M3 state system activates the relic. |
| `Assets/Scripts/M3NamedNpcInteractTarget.cs` | Expands the S3-02 NPC adapter into objective-state routing: accept, re-talk, hand-in, post-complete re-talk. |
| `Assets/Scripts/M3RelicInteractTarget.cs` | New player relic adapter. Calls `TryRecoverRelic`, then resolves M3 objective loot or records `objective_loot_resolution_failed`. |
| `Assets/Editor/GravenspireS3PlayerRelicRecoveryAndLootingBuilder.cs` | Wires the S3-03 adapters onto existing scene anchors only; it does not invoke legacy M2/M3 builders. |
| `Assets/Editor/GravenspireS3PlayerRelicRecoveryAndLootingVerificationRunner.cs` | Unity batchmode runner covering S3-03 T1-T8 plus S3-02 Accepted-state regression evidence. |
| `Assets/Scenes/_DevEntry.unity` | Scene wiring only: NPC adapter gets `_objectiveState`; `M3_ObjectiveRelic` gets `M3RelicInteractTarget`. |

## Acceptance Criteria

| AC | Result | Evidence |
|---|---|---|
| S3-03-01 | PASS | `M3NamedNpcInteractTarget` routes by `M3ObjectiveState` in `Assets/Scripts/M3NamedNpcInteractTarget.cs`; smoke checks `t1_accept_*`, `t2_accepted_retalk_*`, `t4_hand_in_*`, and `t5_complete_retalk_*` PASS in `unity-player-relic-recovery-and-looting-20260530-smoke.md`. |
| S3-03-02 | PASS | `s3-02-regression-20260530-smoke.md` records the Accepted-state S3-02 re-talk regression PASS. |
| S3-03-03 | PASS | `t1_accept_npc_event_before_objective_accepted`, `t1_accept_accept_payload_from_state`, `t1_accept_accept_payload_to_state`, and `t1_accept_accept_source_player_driven` all PASS. Telemetry sequence: `npc_interaction_intentional>objective_accepted>interact_fired`. |
| S3-03-04 | PASS | `t4_hand_in_event_present` and `t4_hand_in_no_npc_interaction_event` PASS; telemetry sequence is `relic_handed_in>interact_fired`. |
| S3-03-05 | PASS | `relic_adapter_present_on_relic` PASS and `_DevEntry.unity` wires `M3RelicInteractTarget` on `M3_ObjectiveRelic`. |
| S3-03-06 | PASS | `M3RelicInteractTarget.TryInteract` calls `TryRecoverRelic` first; blocked dispatch checks `t3_blocked_*` PASS and record `interact_blocked`. |
| S3-03-07 | PASS | Partial-success checks `t3_partial_*` PASS. The runner records `relic_recovered>objective_loot_resolution_failed>interact_fired`, with state left at `RelicRecovered`. |
| S3-03-08 | PASS | Full-success relic checks `t3_success_relic_*` PASS. Telemetry records `relic_recovered>objective_loot_resolved>interact_fired`, relic inactive, carried relic/salvage present, and currency unchanged. |
| S3-03-09 | PASS | `m3-objective-state-zero-diff-20260530.txt` records zero diff for `M3ObjectiveStateRelicHandIn.cs` and `.meta`. |
| S3-03-10 | PASS | `m3-loot-vendor-zero-diff-20260530.txt` records zero diff for `M3LootTableFixedProfileVendor.cs` and `.meta`. |
| S3-03-11 | PASS | `no_dialogue_or_route_ui_scene_objects`, `no_dialogue_or_route_ui_after_interactions`, and `feedback_rule_forbidden_text_absent` PASS. No route-hint feedback is emitted. |
| S3-03-12 | PASS | `t7_end_to_end_*` checks PASS: accept, recover, hand-in all return true; final state is Complete; state sequence is exact; full target vocabulary order is preserved. |

## Companion Evidence

| Artifact | Result |
|---|---|
| `unity-player-relic-recovery-and-looting-20260530-smoke.md` | PASS; no warnings; no errors. |
| `s3-02-regression-20260530-smoke.md` | PASS. |
| `s3-01-harness-regression-20260530-smoke.md` | PASS; added because S3-03 widens harness telemetry behavior. |
| `m2-02-preservation-20260530-smoke.md` | PASS; `Builder Invoked: false`. |
| `m2-03-preservation-20260530-smoke.md` | PASS; `Builder Invoked: false`. |
| `m2-04-preservation-20260530-smoke.md` | PASS; `Builder Invoked: false`. |
| `m3-objective-state-zero-diff-20260530.txt` | PASS. |
| `m3-loot-vendor-zero-diff-20260530.txt` | PASS. |

## Local Gates

| Gate | Result |
|---|---|
| `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` | PASS: 189/189. |
| `dotnet format tests\Gravenspire.Combat.Tests.csproj --verify-no-changes --exclude-diagnostics IDE1006` | PASS. |
| `dotnet format prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj --verify-no-changes --exclude-diagnostics IDE1006` | PASS. |
| `git diff --check` | PASS. |
| Trailing-whitespace scan over changed/untracked S3-03 files and evidence | PASS: zero matches. |
| T1 negative-scope scan over changed code/scene files | PASS: zero matches for FishNet/networking/server authority/PvP/companions/future classes/LLM/wall-clock-time deny terms. |
| `.githooks/pre-commit` via `C:\Program Files\Git\bin\bash.exe` | PASS: `[pre-commit] OK`. |

## Notes

- The S3-03 builder was corrected after an initial local builder pass showed that chaining legacy builders would re-author unrelated scene surfaces. Final builder behavior is adapter-only wiring; final `_DevEntry.unity` diff is limited to S3-03 adapter references.
- Raw Unity `.log` files were not committed because they can contain local licensing and machine identifiers. Evidence is recorded in the generated Markdown smoke artifacts instead.
