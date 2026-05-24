# QA Plan - Sprint 3 Playable Vertical-Slice Assembly

**Date:** 2026-05-24
**Invocation:** `/qa-plan sprint`
**Scope:** Sprint 3 playable vertical-slice assembly: six stories, `S3-01` through `S3-06`.
**Input source:** `production/sprints/sprint-3.md` at Sprint 3 state head `90821f25163c367cda6218b4838dddc0b21da94e`.
**Sprint status source:** `production/sprint-status.yaml` at Sprint 3 state head `90821f25163c367cda6218b4838dddc0b21da94e`.
**Confidence:** High for test classification and evidence gates; medium for exact Unity runner implementation details until each story lands its runner.

## QA Scope

Sprint 3 builds no new game systems. It wires the runner-proven Sprint 2 M3/M2 systems behind player input inside a navigable greybox First District so the loop is human-playable end to end (`production/sprints/sprint-3.md:10` through `production/sprints/sprint-3.md:23`; `DECISIONS.md:575` through `DECISIONS.md:590`).

The QA plan covers all six Must Have stories:

| Story | Classification | Automated Test Required | Manual / Document Verification Required |
| --- | --- | --- | --- |
| `S3-01` Standalone player interaction harness | Integration | Unity Play Mode / batchmode runner T1-T6; M2 preservation reruns | No human-play sign-off; runner telemetry and feedback-deny scan |
| `S3-02` Player-driven NPC interaction | Integration | Unity runner T1-T5; M3 frame zero-diff proof; M2 preservation reruns | No human-play sign-off; no-dialogue-UI scene check |
| `S3-03` Player relic recovery + looting | Integration | Unity runner T1-T8; S3-02 regression; M3 objective/vendor zero-diff proof; M2 preservation reruns | No human-play sign-off; partial-success telemetry documented |
| `S3-04` Player-driven vendor | Integration | Unity runner T1-T6; vendor zero-diff proof; buy-side absence scan; M2 preservation reruns | No human-play sign-off; vendor feedback deny-pattern check |
| `S3-05` Navigable greybox First District | Integration primary + Visual/Feel secondary | Reachability runner; soft-lock scan; composite smoke; M2 preservation reruns | Blocking Pillar 2 wayfinding review; spawn screenshot; advisory walkthrough |
| `S3-06` Playable end-to-end + human-play feel check | Integration primary + Visual/Feel | End-to-end runner; M2 melee-RNG reset evidence; runner exception-guard evidence; chained M2 smoke | Blocking human-play verdict; optional second-playtester read |

## Source List

Verification method: live repository reads with `Get-Content`, `rg`, `git status`, subagent read-only QA extraction, and targeted source inspection on 2026-05-24.

| Source | Use |
| --- | --- |
| `production/sprints/sprint-3.md:10` through `production/sprints/sprint-3.md:23` | Sprint 3 goal, narrow target, and greybox-not-art presentation bar. |
| `production/sprints/sprint-3.md:66` through `production/sprints/sprint-3.md:71` | Six-story ledger, dependency order, and primary evidence summary. |
| `production/sprints/sprint-3.md:83` through `production/sprints/sprint-3.md:87` | Reuse-not-rebuild, feedback rule, and Sprint 3 review-subagent correction. |
| `production/sprints/sprint-3.md:109` through `production/sprints/sprint-3.md:112` | Runner-hygiene carryovers; only M2 RNG reset and new-runner exception guarding are in S3-06 scope. |
| `production/sprints/sprint-3.md:127` through `production/sprints/sprint-3.md:133` | Scene fragility, locomotion adequacy, greybox readability, owner, and Pillar 2 risk notes. |
| `production/sprints/sprint-3.md:140` through `production/sprints/sprint-3.md:145` | QA Plan Hooks: story types, human-play-on-S3-06-only, evidence baseline, M2 preservation, style gate. |
| `production/sprints/sprint-3.md:164` through `production/sprints/sprint-3.md:166` | Critical path, S3-05 parallelization, and owner assumptions. |
| `production/sprint-status.yaml:12` through `production/sprint-status.yaml:20` | Current QA plan pointer, implementation gate, and next command state. |
| `production/sprint-status.yaml:77` through `production/sprint-status.yaml:130` | Six story rows, status, dependency blockers, and owner fields. |
| `production/stories/s3-01-standalone-player-interaction-harness.md:44` through `production/stories/s3-01-standalone-player-interaction-harness.md:52` | S3-01 acceptance criteria for standalone harness, dispatch, feedback, and range-gated prompt. |
| `production/stories/s3-01-standalone-player-interaction-harness.md:85` through `production/stories/s3-01-standalone-player-interaction-harness.md:147` | S3-01 runner cases and evidence requirements. |
| `production/stories/s3-02-player-driven-npc-interaction.md:46` through `production/stories/s3-02-player-driven-npc-interaction.md:52` | S3-02 adapter, M3 zero-diff, telemetry, no UI, and blocked feedback criteria. |
| `production/stories/s3-02-player-driven-npc-interaction.md:81` through `production/stories/s3-02-player-driven-npc-interaction.md:139` | S3-02 runner cases and evidence requirements. |
| `production/stories/s3-03-player-relic-recovery-and-looting.md:57` through `production/stories/s3-03-player-relic-recovery-and-looting.md:78` | S3-03 state-routing, relic adapter, loot, telemetry, zero-diff, and full-loop criteria. |
| `production/stories/s3-03-player-relic-recovery-and-looting.md:112` through `production/stories/s3-03-player-relic-recovery-and-looting.md:194` | S3-03 runner cases and evidence requirements. |
| `production/stories/s3-04-player-driven-vendor.md:50` through `production/stories/s3-04-player-driven-vendor.md:61` | S3-04 adapter, vendor telemetry, post-sale state, blocked sale, and feedback criteria. |
| `production/stories/s3-04-player-driven-vendor.md:93` through `production/stories/s3-04-player-driven-vendor.md:161` | S3-04 runner cases, buy-side absence scan, and evidence requirements. |
| `production/stories/s3-05-navigable-greybox-first-district.md:59` through `production/stories/s3-05-navigable-greybox-first-district.md:102` | S3-05 district, reachability, soft-lock, Pillar 2, greybox-only, M2/M3 preservation criteria. |
| `production/stories/s3-05-navigable-greybox-first-district.md:142` through `production/stories/s3-05-navigable-greybox-first-district.md:225` | S3-05 runner, manual review, scan, composite smoke, and pattern-note requirements. |
| `production/stories/s3-06-playable-end-to-end-and-human-play.md:61` through `production/stories/s3-06-playable-end-to-end-and-human-play.md:104` | S3-06 end-to-end runner, M2 hygiene, and human-play criteria. |
| `production/stories/s3-06-playable-end-to-end-and-human-play.md:141` through `production/stories/s3-06-playable-end-to-end-and-human-play.md:213` | S3-06 runner, human-play protocol, evidence, and M2 preservation requirements. |
| `DECISIONS.md:55` through `DECISIONS.md:61` | D003 Tier 1 single-player offline scope. |
| `DECISIONS.md:74` through `DECISIONS.md:79` | D004 T1 templated dialogue and no live LLM dependency. |
| `DECISIONS.md:571` through `DECISIONS.md:595` | D016 Sprint 3 re-sequence, reuse-not-rebuild, greybox-not-art, and feel-gate definition-of-done rule. |
| `tasks/lessons.md:24` through `tasks/lessons.md:30` | Human-play feel-gates belong only on playable stories; telemetry is not a substitute for a playable loop. |
| `.claude/rules/game-dev-governance.md:63` through `.claude/rules/game-dev-governance.md:67` | Tier 1 style gate: `dotnet format --verify-no-changes` must pass locally before PR. |
| `.githooks/pre-commit:10` through `.githooks/pre-commit:17` | Current hook runs diff check, dotnet format, and staged T1 deny scan with IDE1006 excluded. |
| `.githooks/pre-commit:20` through `.githooks/pre-commit:31` | Hook deny scan currently scopes only staged `src/*.cs` and `tests/*.cs`, so Sprint 3 must run explicit scans over Unity files. |
| `.claude/docs/technical-preferences.md:12` through `.claude/docs/technical-preferences.md:24` | Unity 6.3 LTS, C#, URP, and keyboard/mouse baseline. |
| `docs/engine-reference/unity/VERSION.md:5` through `docs/engine-reference/unity/VERSION.md:18` | Unity 6.3 LTS version and post-cutoff API risk warning. |

## Live-State Corrections

- Format-gate setup is resolved by PR #2 / merge commit `90821f2`, and `.githooks/pre-commit` now runs `dotnet format` with IDE1006 excluded. Story-local notes that call format setup "unconfirmed" are stale; treat style gate as active evidence required, not a setup blocker.
- `docs/architecture/control-manifest.md` is still absent. Sprint 3 stories carry `Manifest Version: Unavailable`; QA uses the documented architecture-registry fallback pattern from Sprint 2 and does not block on manifest creation.
- Owner assignment was the other remaining precondition. This QA plan assumes the owner fields are updated in the same approved batch that records this plan.
- The working tree has unrelated dirty files. This QA plan does not classify, approve, or reject those changes; story verification must inspect only the story implementation diff under review.

## Classification Summary

All six stories are Integration stories because each crosses Unity scene, player input, existing M2/M3 runtime, and story-local evidence boundaries. `S3-05` carries Visual/Feel secondary evidence for spatial readability. `S3-06` carries Visual/Feel evidence through the only binding human-play feel check.

No story before `S3-06` may add a binding human-play feel AC. `S3-01` through `S3-05` may be played during implementation, but their closure gates are mechanical/integration evidence.

## Regression Gates

| Gate | Timing | Command / Method | Pass Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| RG-00 Combat/NPC baseline | Before each story closure | `dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` | Test suite passes; current count is expected to begin near 189, but verification records legitimate discovery-count changes | Story `tests/evidence/S3-NN/verification.md` |
| RG-01 Unity story runner | During each story | Story-specific Unity batchmode / Play Mode runner | Story AC runner checks pass, exit 0, no unclassified warnings/errors | Story-specific smoke file under `tests/evidence/S3-NN/` |
| RG-02 M2 preservation | Any story touching `_DevEntry.unity`, shared M2 surfaces, or scene anchors | Run `M2SingleTrashLoop`, `M2LinkedTrashOverpull`, and `M2NamedBlockerBoundary` in separate batchmode invocations until S3-06 proves RNG reset | All three pass, exit 0, with story-local evidence path overrides | `tests/evidence/S3-NN/m2-0X-preservation-[YYYYMMDD]-smoke.md` |
| RG-03 M3 reuse / zero-diff | S3-02 through S3-04 | `git diff --stat` over protected M3 files | Required M3 system files and `.meta` files have zero diff where story ACs require it | `tests/evidence/S3-NN/*zero-diff-[YYYYMMDD].txt` |
| RG-04 T1 negative-scope scan | Before story closure | Explicit scan over changed files, including `Assets/Scripts`, `Assets/Editor`, `Assets/Scenes/_DevEntry.unity`, `Packages`, `src`, `tests`, and story/evidence files as applicable | No real implementation hits for Tier 2+ or out-of-scope surfaces; documentation self-hits are classified | Story `verification.md` |
| RG-05 Style/local gate | Before PR / commit | `git diff --check`; `.githooks/pre-commit`; `dotnet format --verify-no-changes` as invoked by hook | Diff hygiene clean; hook reports `[pre-commit] OK`; format gate passes with documented IDE1006 exclusion | Story `verification.md` |
| RG-06 Review-subagent gate | Each `/code-review` | Standing reviewer + `qa-tester`; add `unity-specialist` for S3-01/S3-05/S3-06 and `gameplay-programmer` for harness/dispatch logic | Review findings addressed or explicitly deferred with severity/rationale | Story review notes / verification |

## Story Test Plans

### S3-01 - Standalone Player Interaction Harness

**Classification:** Integration
**Status after this QA plan:** Ready for `/story-readiness` once owner/status fields are synced.
**Story:** `production/stories/s3-01-standalone-player-interaction-harness.md`
**Evidence target:** `tests/evidence/S3-01/verification.md`

| Case ID | Scenario | Method | Pass / Fail Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| QA-S3-01-01 | Harness is standalone | Unity runner scene-tree check | `S3PlayerInteractionHarness` exists in `_DevEntry.unity`; it is distinct from the M2 controller; M2 locomotion code has zero diff | `unity-player-interaction-harness-[YYYYMMDD]-smoke.md`; verification diff note |
| QA-S3-01-02 | One interact verb dispatches | Unity runner with mock `IPlayerInteractTarget` | Keypress/test entry invokes target exactly once with actor id and measured distance; harness does not mutate objective/loot/vendor state | Same smoke |
| QA-S3-01-03 | Fired/missed/blocked feedback contract | Unity runner telemetry plus deny-pattern check | `interact_fired`, `interact_missed`, and `interact_blocked` all acknowledge the player action without routing or diagnostic hints | Same smoke |
| QA-S3-01-04 | Prompt is range-gated | Unity runner distance sweep | Prompt appears only within threshold and never acts as far-distance locator | Same smoke |
| QA-S3-01-05 | M2 preservation | Three independent M2 smokes | Clean loop, overpull, and named-blocker boundary all pass in story-local evidence directory | `m2-02/03/04-preservation-[YYYYMMDD]-smoke.md` |
| QA-S3-01-06 | T1 and style gates | Static scan + hook | No no-netcode/no-LLM/no-marker scope violations; `git diff --check`, hook, and format gate pass | `verification.md` |

### S3-02 - Player-Driven NPC Interaction

**Classification:** Integration
**Dependency:** `S3-01` Done
**Story:** `production/stories/s3-02-player-driven-npc-interaction.md`
**Evidence target:** `tests/evidence/S3-02/verification.md`

| Case ID | Scenario | Method | Pass / Fail Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| QA-S3-02-01 | Adapter presence and binding | Unity runner scene-tree check | `M3NamedNpcInteractTarget` is on `M3_Caretaker`, registered with harness, and points to the existing `M3NamedNpcObjectiveFrame` | `unity-player-driven-npc-interaction-[YYYYMMDD]-smoke.md` |
| QA-S3-02-02 | In-range player interaction | Unity runner through harness path | Harness -> adapter -> `TryRecordIntentionalInteraction` fires once, records full `NpcInteractionContext`, `wasIntentional=true`, and `source=player_driven` | Same smoke |
| QA-S3-02-03 | M3 frame range reject | Unity runner boundary test | Out-of-M3-range attempt returns false, records no intentional NPC event, and uses harness blocked feedback without "get closer" text | Same smoke |
| QA-S3-02-04 | M3 frame zero-diff | Diff stat artifact | `M3NamedNpcObjectiveFrame.cs` and `.meta` have zero diff | `m3-frame-zero-diff-[YYYYMMDD].txt` |
| QA-S3-02-05 | No dialogue UI or route affordance | Scene-tree query / screen capture / deny scan | Dialogue handles are telemetry data only; no dialogue window, overhead name, minimap, marker, signpost, glow, or route hint | Smoke + verification |
| QA-S3-02-06 | M2 preservation and local gates | Independent smokes + hook | M2 preservation, negative-scope scan, diff check, pre-commit, and format gate pass | `verification.md` |

### S3-03 - Player Relic Recovery + Looting

**Classification:** Integration
**Dependencies:** `S3-01` Done; `S3-02` Done
**Story:** `production/stories/s3-03-player-relic-recovery-and-looting.md`
**Evidence target:** `tests/evidence/S3-03/verification.md`

| Case ID | Scenario | Method | Pass / Fail Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| QA-S3-03-01 | NPC adapter state routing | Unity runner | NotIntroduced -> Accept; Accepted -> re-talk; RelicRecovered -> hand-in; Complete -> re-talk; no double-recording on accept | `unity-player-relic-recovery-and-looting-[YYYYMMDD]-smoke.md` |
| QA-S3-03-02 | Accept telemetry order | Unity runner telemetry assertion | `npc_interaction_intentional` records before `objective_accepted`, matching M3 internal call order | Same smoke |
| QA-S3-03-03 | Relic adapter full success | Unity runner through harness path | `TryRecoverRelic` succeeds, `TryResolveObjectiveLoot` succeeds, relic deactivates, `relic_recovered` and `objective_loot_resolved` fire | Same smoke |
| QA-S3-03-04 | Relic adapter partial success | Synthesized session-init failure path | Recovery success plus loot-resolution failure records `objective_loot_resolution_failed` honestly; no rollback is attempted | Same smoke + verification telemetry-shape note |
| QA-S3-03-05 | Hand-in completion | Unity runner | RelicRecovered -> Complete via NPC; `relic_handed_in` fires; no NPC intentional event for hand-in dispatch | Same smoke |
| QA-S3-03-06 | M3 objective/vendor zero-diff | Diff stat artifacts | `M3ObjectiveStateRelicHandIn.cs` and `M3LootTableFixedProfileVendor.cs` plus `.meta` files have zero diff | `m3-objective-state-zero-diff-[YYYYMMDD].txt`; `m3-loot-vendor-zero-diff-[YYYYMMDD].txt` |
| QA-S3-03-07 | S3-02 regression | Rerun S3-02 player-driven NPC path | In-range NPC interaction still records `player_driven` intentional event after adapter state-routing expansion | `s3-02-regression-[YYYYMMDD]-smoke.md` |
| QA-S3-03-08 | Feedback rule and scope gates | Deny scan + local gates | No route text such as "now go" or "return to"; M2 preservation, T1 negative-scope, diff, pre-commit, and format gates pass | `verification.md` |

### S3-04 - Player-Driven Vendor

**Classification:** Integration
**Dependencies:** `S3-01` Done; `S3-03` Done
**Story:** `production/stories/s3-04-player-driven-vendor.md`
**Evidence target:** `tests/evidence/S3-04/verification.md`

| Case ID | Scenario | Method | Pass / Fail Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| QA-S3-04-01 | Vendor adapter presence and binding | Unity runner scene-tree check | `M3VendorInteractTarget` is on `M3_CourtVendor`, registered with harness, and references the existing M3 vendor component | `unity-player-driven-vendor-[YYYYMMDD]-smoke.md` |
| QA-S3-04-02 | Sale success | Unity runner through harness path | One interact calls `TrySellRecoveredSalvage`, credits copper, decreases carried slots by one, and emits `vendor_salvage_sold` then `vendor_sell_copper_applied` | Same smoke |
| QA-S3-04-03 | Sale blocked | Unity runner blocked path | No salvage -> no sale events, currency unchanged, rejection telemetry/debug data only, harness blocked feedback without player-facing explanation | Same smoke |
| QA-S3-04-04 | Feedback rule compliance | Deny-pattern scan / capture | Sale acknowledgement reports result only; no buy-side affordance or "now go" prompt; blocked path does not explain "no salvage" to player | Same smoke |
| QA-S3-04-05 | M3 vendor zero-diff | Diff stat artifact | `M3LootTableFixedProfileVendor.cs` and `.meta` have zero diff | `m3-loot-vendor-zero-diff-[YYYYMMDD].txt` |
| QA-S3-04-06 | Buy-side absence | Source scan + runtime probe | New adapter contains zero references to `TryPurchaseFixedVendorGood`; runtime has no second vendor dispatch path | `buy-side-absence-scan-[YYYYMMDD].txt` |
| QA-S3-04-07 | M2 preservation and local gates | Independent smokes + hook | M2 preservation, T1 negative-scope, diff, pre-commit, and format gates pass | `verification.md` |

### S3-05 - Navigable Greybox First District

**Classification:** Integration primary + Visual/Feel secondary evidence
**Dependency:** `S3-01` Done. `S3-02` through `S3-04` are not hard dependencies, but full AC-12 rolls forward to S3-06 if unavailable.
**Story:** `production/stories/s3-05-navigable-greybox-first-district.md`
**Evidence target:** `tests/evidence/S3-05/verification.md`

| Case ID | Scenario | Method | Pass / Fail Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| QA-S3-05-01 | District replaces shell | Unity runner scene-tree check | `FirstDistrict_ShellOnly_NoGameplay` is removed/replaced; `FirstDistrict_Greybox` exists; M3 anchors retain bindings | `verification.md`; runner output |
| QA-S3-05-02 | Reachability | `GravenspireS3FirstDistrictReachabilityRunner` | NavMesh path from spawn to all three M3 anchors; path length and max elevation delta recorded; relic activation restored after check | `unity-first-district-reachability-[YYYYMMDD]-smoke.md` |
| QA-S3-05-03 | Soft-lock scan | `GravenspireS3FirstDistrictSoftLockScanRunner` | Zero soft-lock zones detected at 1m grid sampling; evidence wording says "best-effort high-confidence", not exhaustive | `unity-first-district-soft-lock-scan-[YYYYMMDD]-smoke.md` |
| QA-S3-05-04 | Pillar 2 wayfinding review | Blocking design-aware manual review | Pass/fail per four reject criteria: single-path layout, focal-point lighting, geometric focal framing, anchor visual distinction; spawn-to-Caretaker has sightline and 2-3 plausible landmarks | `pillar-2-wayfinding-review-[YYYYMMDD].md`; `spawn-to-caretaker-discoverability-[YYYYMMDD].png` |
| QA-S3-05-05 | Walkthrough log | Advisory human walkthrough | Lead walks spawn to anchors with no debug teleport; logs time-to-arrival, disorientation, and geometric edge cases complementary to soft-lock scan | `walkthrough-log-[YYYYMMDD].md` |
| QA-S3-05-06 | Greybox-only scan | Source/asset scan | No produced art, imported mesh/texture scope, focal lights, audio additions, second district, marker, minimap, glow, outline, or route affordance | `greybox-presentation-scan-[YYYYMMDD].txt` |
| QA-S3-05-07 | Composite district smoke | Unity runner / scripted or human input path | Full chain passes if S3-02..04 are closed; otherwise partial pass is recorded and `s3_05_ac12_partial_rollforward_to_s3_06` is added at story close | `unity-end-to-end-in-district-[YYYYMMDD]-smoke.md` |
| QA-S3-05-08 | M2 preservation and local gates | Independent smokes + hook | M2 preservation, T1 negative-scope, diff, pre-commit, and format gates pass | `verification.md` |

QA correction: S3-05 story text labels the walkthrough as "AC-11" in its test case heading, but AC-11 is M2 preservation. This plan treats the walkthrough as advisory/complementary evidence for AC-07 and spatial readability, while AC-11 remains the M2 preservation gate.

### S3-06 - Playable End-to-End + Human-Play Feel Check

**Classification:** Integration primary + Visual/Feel human-play evidence
**Dependencies:** `S3-02`, `S3-03`, `S3-04`, and `S3-05` Done
**Story:** `production/stories/s3-06-playable-end-to-end-and-human-play.md`
**Evidence target:** `tests/evidence/S3-06/verification.md`

| Case ID | Scenario | Method | Pass / Fail Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| QA-S3-06-01 | Full end-to-end runner | `GravenspireS3PlayableEndToEndRunner` | Scripted input drives spawn -> Caretaker -> relic -> vendor -> Caretaker; telemetry sequence and final state match AC-03 | `unity-playable-end-to-end-[YYYYMMDD]-smoke.md` |
| QA-S3-06-02 | M2 melee-RNG reset additive | Targeted M2 controller change + independent M2 reruns | Independent M2 smoke telemetry matches pre-S3-06 baseline; reset changes only inter-scenario determinism, not scenario behavior | `m2-regression-comparison-[YYYYMMDD].md`; independent M2 smoke files |
| QA-S3-06-03 | New-runner exception guarding | Safe runner-only injected failure path | A deliberately injected check failure records a clear FAIL with exception type, check name, and message; runner continues to later checks and exits non-zero | `unity-runner-exception-guard-[YYYYMMDD]-smoke.md` |
| QA-S3-06-04 | Chained M2 preservation | Same Play session after end-to-end sequence | M2 named-blocker boundary, or all three M2 smokes if implemented, pass with telemetry matching baseline | `unity-end-to-end-chained-m2-[YYYYMMDD]-smoke.md` |
| QA-S3-06-05 | Human-play primary verdict | Lead playtest protocol | Lead completes one full loop, answers "Would you do that again right now?" and "Why?" or "What would change that?"; PASS requires yes plus objective/NPC/relic attribution | `human-play-[YYYYMMDD].md` |
| QA-S3-06-06 | Presentation-limit classification | Evidence review | Any greybox aesthetic deficit is classified Tolerable under R-P2-FEEL-MISATTRIBUTION; loop-mechanical deficits are Blocking and affect verdict | `human-play-[YYYYMMDD].md` |
| QA-S3-06-07 | Optional second playtester | Repeat protocol if available | Second read is recorded separately; disagreement is preserved without overwriting lead verdict convention | `human-play-[YYYYMMDD].md` |
| QA-S3-06-08 | Local gates and carryover retirement | Static scan + hook + sprint status closure | Negative-scope, diff, pre-commit, format pass; `m2_melee_rng_not_reset` retires only if AC-04 passes; `m3_04_low_review_notes` item 1 partially retires only if AC-05 passes | `verification.md`; `production/sprint-status.yaml` after `/story-done` |

QA requirement: the injected exception test must be runner-only and safe. Do not add a runtime gameplay failure mode just to test runner guarding.

## Cross-Story Required Checks

### Human-Play Gate Placement

Human-play feel acceptance is binding only on `S3-06`. This follows D016 (`DECISIONS.md:591` through `DECISIONS.md:595`) and the 2026-05-20 lesson (`tasks/lessons.md:28`). `S3-01` through `S3-05` may be played during development, but closure relies on mechanical, integration, spatial, and documented manual-review gates.

### M2 Preservation Discipline

Until `S3-06` lands the additive M2 melee-RNG reset, each M2 preservation smoke runs in its own Unity batchmode invocation. This is required for `S3-01` through `S3-05`. After `S3-06`, independent reruns become the regression baseline and chained-session proof becomes the positive reset demonstration.

### Explicit Negative-Scope Scans

Do not rely on `.githooks/pre-commit` as the only T1 negative-scope scan for Sprint 3. The hook's deny scan currently reads staged `src/*.cs|tests/*.cs` only, while Sprint 3 mostly touches `Assets/Scripts`, `Assets/Editor`, `.unity`, package, and evidence surfaces. Every story verification must include a changed-file scan over all changed story files and classify doc self-hits.

Suggested deny-pattern families:

- Tier 2+ tech: `FishNet`, `networking`, `server authority`, `PvP`, account/cloud-save surfaces
- T1 content cuts: `Warrior`, `Enchanter`, broad companion system, second district
- T1 AI/dialogue cuts: `OpenAI`, `Anthropic`, live LLM calls, moderation runtime
- Convenience creep: quest marker, minimap, compass, route hint, glow/outline, objective HUD pin, auto-path, overhead name plate
- Runner hygiene: `DateTime.UtcNow`, hardcoded evidence paths/dates in new runners

### Reuse-Not-Rebuild Invariants

- `S3-02`: `M3NamedNpcObjectiveFrame.cs` has zero diff.
- `S3-03`: `M3ObjectiveStateRelicHandIn.cs` and `M3LootTableFixedProfileVendor.cs` have zero diff.
- `S3-04`: `M3LootTableFixedProfileVendor.cs` has zero diff, and buy-side dispatch stays absent.
- `S3-06`: only the M2 RNG cursors/reset hooks are touched in `M2SingleTrashMedLoopController.cs`; broader controller refactor remains out of scope.

### Scene Governance

Scene-touching stories must save through Unity, inspect diffs, and keep one scene edit per PR where possible. `S3-05` is the largest scene change and needs a clean scene window. Concurrent scene edits across worktrees should be avoided.

## Smoke Test Scope

Critical paths to verify before Sprint 3 QA hand-off:

1. `dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` passes.
2. Story-specific Unity runner for each closed story passes with story-local evidence.
3. M2 preservation smokes pass under the correct invocation model for that story.
4. Protected M3 files have zero diff where required.
5. `_DevEntry.unity` loads and contains the expected harness/adapters/anchors/district hierarchy.
6. Player input path, not runner-only shortcuts, drives NPC, relic, vendor, and end-to-end flows.
7. T1 negative-scope scans cover all changed files, including Unity files.
8. Greybox-only and no-wayfinding checks pass for S3-05.
9. S3-06 human-play evidence records the binary pillar-anchored verdict and N=1 limitation.
10. `git diff --check`, `.githooks/pre-commit`, and format gate pass.

## Playtest Requirements

| Story | Playtest Goal | Minimum Sessions | Target Player Type | Required Evidence |
| --- | --- | ---:| --- | --- |
| `S3-01` through `S3-04` | None as binding closure; implement-play-fix allowed during development | 0 | N/A | Runner/evidence artifacts only |
| `S3-05` | Advisory spatial walkthrough, complementary to soft-lock scan | 1 advisory walkthrough | Lead / design-aware reviewer | `tests/evidence/S3-05/walkthrough-log-[YYYYMMDD].md` |
| `S3-06` | Determine whether the full loop is worth playing | 1 blocking primary session | Project lead | `tests/evidence/S3-06/human-play-[YYYYMMDD].md` |
| `S3-06` optional | Additional read without replacing lead verdict | 0 or 1 optional session | Second playtester if available | Separate section in same human-play evidence |

## Needs Clarification Before /dev-story

| Story | Clarification Needed | Why It Matters |
| --- | --- | --- |
| `S3-01` | Final harness input-simulation strategy for Unity runner | Legacy `UnityEngine.Input` may require a testable dispatch entry point rather than synthetic key injection |
| `S3-02` | Whether `InteractContext` must widen for dialogue/objective keys | Widening is in scope, but test must lock the payload contract |
| `S3-03` | Partial-success injection mechanism for loot resolution failure | Must prove recovery-success/loot-fail telemetry without corrupting normal runtime path |
| `S3-05` | Unity AI Navigation package/version and NavMesh agent profile | Package addition is active-work-tied; agent profile becomes future T1 reference |
| `S3-06` | Safe runner-only injected-failure mechanism | Exception-guard proof must not add a gameplay failure path |

## Definition of Done - Sprint 3 QA

A Sprint 3 story is DONE only when all applicable items are true:

- [ ] Story owner is assigned in the story header and `production/sprint-status.yaml`.
- [ ] All acceptance criteria are verified by automated runner, static/document review, manual design-aware evidence, or human-play evidence where applicable.
- [ ] Every Integration story has a story-specific Unity runner or explicit documented reason why an existing runner fully covers it.
- [ ] `S3-05` includes reachability, soft-lock, Pillar 2 review, greybox-only scan, and pattern notes.
- [ ] `S3-06` includes full end-to-end runner, M2 RNG reset evidence, exception-guard evidence, human-play evidence, and pattern notes.
- [ ] M2 preservation evidence exists for every scene/shared-surface story.
- [ ] T1 negative-scope scan covers all changed files and classifies documentation self-hits.
- [ ] `dotnet test`, `git diff --check`, `.githooks/pre-commit`, and format gate pass.
- [ ] `/code-review` runs with the Sprint 3 standing review-subagent layer.
- [ ] `/story-done` updates story status, `production/sprint-status.yaml`, and any carryover retirement only after evidence is complete.

## Next Gate

Run `/story-readiness production/stories/s3-01-standalone-player-interaction-harness.md` after this QA plan and the Sprint 3 owner fields are written.
