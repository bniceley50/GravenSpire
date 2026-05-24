# Sprint 3 - Gravenspire T1: Playable Vertical-Slice Assembly

> **Status**: Planning draft / pre-`/create-stories`
> **Generated**: 2026-05-21
> **Current Head**: d7acfbf
> **Prior Sprint Gate**: Sprint 2 closed 2026-05-20 (DECISIONS.md D016); M1-M3 delivered as runner-proven systems, 11/11 stories complete

## Goal

Build **Gravenspire T1: Playable Vertical-Slice Assembly**: take the runner-proven Sprint 2 M3 (and M2) systems — named-NPC objective frame, objective state, relic recovery, loot resolution, fixed-profile vendor, and the combat camp loop — and wire them behind **player input** inside a navigable greybox First District, so the Tier-1 objective loop is genuinely human-playable and feel-testable end to end.

Sprint 3 builds **no new systems**. Its deliverable is the orchestration/composition layer that drives the existing M3 `Try*` entry points from a player marker and interact verb, plus a navigable district to drive them in. The honest scope statement: Sprint 2 proved the systems with runners; Sprint 3 proves a *human can play them*.

The Sprint 3 target is deliberately narrow:

- one standalone player interaction harness (player marker + interact raycast/distance-check + one interact verb)
- the player intentionally interacting with the M3 named NPC to receive an objective
- the player recovering and looting the M3 objective relic
- the player selling salvage at the M3 fixed-profile vendor
- one navigable greybox First District replacing the `FirstDistrict_ShellOnly_NoGameplay` shell
- a playable end-to-end objective loop, validated by a human "feel" check

What carries the feel is **interactivity, spatial readability, and legible feedback** — not produced art. Presentation minimum (D016): navigable + readable blockout massing + legible interaction feedback. Greybox, not art.

This is single-milestone re-sequencing within Tier 1, not new scope. M4 (Save/Load Flow) and M5 (Faction Consequence) defer behind this assembly milestone — both presuppose a playable loop, and M5, the Tier-1 emotional payoff, only lands if the player drove the change.

## Gate State

Sprint 2 is closed (DECISIONS.md D016):

1. M1-M3 delivered as runner-proven systems; `S2-M3-04` passed its batchmode smoke 29/29 and closed COMPLETE WITH NOTES (closure commit `ee7c450`).
2. `S2-M3-04` acceptance criterion AC-06 (mandatory human-play feel check) could not be validated — the M3 objective loop has no player input path — and transferred to this Sprint 3 milestone.
3. A producer + creative-director reassessment (2026-05-20) confirmed pillars and systems are sound; the failure was sequencing and the definition of "done".

A technical-director feasibility consult is complete (2026-05-20 session). Findings are incorporated below under Risks and the Story Ledger. Headline result: D016's open question is answered **YES** — the M3 session-state APIs are drivable from player input without rework.

Before story-breaking the Sprint 3 slate:

1. Assign one accountable `owner` per story (story files carry empty `owner` fields until `/create-stories`).
2. Run `/create-stories` for the 6-story slate using this plan and `design/quick/quick-design-m3-objective-npc-loot.md` as input.
3. A Sprint 3 `/qa-plan` follows plan approval and precedes implementation.

## Milestone Structure

Sprint 3 is a **single milestone**: Playable Vertical-Slice Assembly.

| Milestone | Name | Purpose | First Proof |
|---|---|---|---|
| Sprint 3 | Playable Vertical-Slice Assembly | Wire the runner-proven M3/M2 systems behind player input inside a navigable greybox First District. | A human can launch the build, walk the district as the Cleric, interact with the named NPC, recover and hand in the relic, and sell salvage — the full Tier-1 objective loop, playable end to end. |

Deferred behind this milestone (unchanged from the Sprint 2 M1-M5 structure):

| Milestone | Name | Status |
|---|---|---|
| M4 | Save/Load Flow | Deferred behind Sprint 3 (DECISIONS.md D016). Presupposes a playable loop. |
| M5 | Faction Consequence | Deferred behind Sprint 3 (DECISIONS.md D016). The Tier-1 emotional payoff; only lands if the player drove the change. |

## Story Ledger

Proposed 6-story slate, dependency-ordered. Story IDs use the `S3-NN` scheme — Sprint 3 is single-milestone, so the Sprint 2 `S2-M2-NN` / `S2-M3-NN` per-milestone prefix is not needed. IDs were a proposal for `/create-stories`; the slate was opened on 2026-05-23 (slate commit `9bf60e1`) and each `S3-NN` ID in the table below links to its story file at `production/stories/s3-NN-*.md`. The ledger row provides design context; the linked file provides implementable spec.

All six stories are FEASIBLE BY REUSE per the technical-director feasibility consult. Story `S3-05` is feasible with noted risk — the risk is scene-authoring volume, not code. The estimate basis is Sprint 2 actuals (M3 stories landed at ~1.0 day each); confidence and risk multipliers are noted per story below and in Capacity Assumptions.

| ID | Name | Purpose | M3/M2 Surface Reused | Scope Boundary | Primary Evidence | Depends On |
|---|---|---|---|---|---|---|
| [`S3-01`](../stories/s3-01-standalone-player-interaction-harness.md) | Standalone player interaction harness | A NEW standalone lightweight component owns the player marker and an interact raycast/distance-check, and dispatches one interact verb to the M3 systems. | M2 player locomotion: `M2SingleTrashMedLoopController.cs:564-583` (WASD transform-driven movement + follow-camera, drives `ClericShellMarker`). The harness extends/reuses the navigable marker; it does not re-author locomotion. | NEW standalone component — NOT a verb bolted onto `M2SingleTrashMedLoopController` (USER DECISION; the 2156-line combat-only controller is already flagged too large by `m2_controller_scenario_smoke_abstraction`). The harness dispatches to existing `Try*` methods only; it implements no objective/loot/vendor logic. Interaction feedback obeys the Sprint 3 feedback rule (Operating Model Calibration): an interact prompt is range-gated, never a far-distance locator. `S3-01` also locks the harness's player-feedback contract — what the player perceives on interact-fired, interact-missed (raycast hit nothing), and interact-blocked (hit an anchor but the M3 `Try*` returned `false`). All three are acknowledgements of a player action and obey the feedback rule; `/create-stories` details the per-outcome feedback. | Integration: Unity Play Mode / batchmode runner proving the marker + interact raycast/distance-check fire one interact verb. | — |
| [`S3-02`](../stories/s3-02-player-driven-npc-interaction.md) | Player-driven NPC interaction | The player intentionally interacts with the M3 named NPC to receive the objective frame. | `M3NamedNpcObjectiveFrame.TryRecordIntentionalInteraction(...)` (`Assets/Scripts/M3NamedNpcObjectiveFrame.cs:112`); `M3_Caretaker` scene anchor (`_DevEntry.unity:881`). | Drives the existing `M3_Caretaker` quest-give through player input. No rewrite of the named-NPC objective frame; no quest markers / overhead names / Dialogue System UI / live LLM (S2-M3-01 boundaries hold). | Integration: runner telemetry proving player-driven `npc_interaction_intentional` and the templated dialogue handle. | `S3-01` |
| [`S3-03`](../stories/s3-03-player-relic-recovery-and-looting.md) | Player relic recovery + looting | The player recovers the marked relic and resolves objective loot through player input. | `M3ObjectiveStateRelicHandIn.TryRecoverRelic()` (`Assets/Scripts/M3ObjectiveStateRelicHandIn.cs:82`) and `TryAcceptObjectiveFromNpc(...)` (`:54`); `M3LootTableFixedProfileVendor.TryResolveObjectiveLoot()` (`Assets/Scripts/M3LootTableFixedProfileVendor.cs:89`); `M3_ObjectiveRelic` anchor (`_DevEntry.unity:1556`). | Drives M3 objective-state transitions + the authored M3 loot table. Rewriting objective state or loot resolution is a red flag (D016). Loot stays authored-id, no `kill_weight_seed` reuse, no CurrencyContainer (S2-M3-03 boundaries hold). | Integration: runner telemetry proving player-driven `objective_state_sequence`, `relic_available`, `relic_recovered`, `loot_table_id`, `loot_result_item_ids`. | `S3-02` |
| [`S3-04`](../stories/s3-04-player-driven-vendor.md) | Player-driven vendor | The player sells recovered salvage at the M3 fixed-profile vendor through player input. | `M3LootTableFixedProfileVendor.TrySellRecoveredSalvage(out int creditedCopper)` (`Assets/Scripts/M3LootTableFixedProfileVendor.cs:101`); `M3_CourtVendor` anchor (`_DevEntry.unity:382`). | Drives the F4 fixed-profile vendor. Rewriting the F4 vendor formula is a red flag (D016). No tuned economy / coin-pacing claim / `CoinFaucetProjection_T1` / buy-side formula (S2-M3-03 boundaries hold). `TryPurchaseFixedVendorGood(...)` (`:119`) exists but the player is not required to buy to close the loop. | Integration: runner telemetry proving player-driven `vendor_salvage_sold`, `vendor_sell_copper_applied`. | `S3-03` |
| [`S3-05`](../stories/s3-05-navigable-greybox-first-district.md) | Navigable greybox First District | Replace the `FirstDistrict_ShellOnly_NoGameplay` shell with a navigable greybox district that hosts the M3 NPC, relic, and vendor anchors and supports walked traversal between them. | `_DevEntry.unity`: `DevEntry_DistrictBlockout_Floor` (`:1331`), the `FirstDistrict_ShellOnly_NoGameplay` empty marker (`:1729`), the three placed M3 anchors `M3_Caretaker` (`:881`), `M3_ObjectiveRelic` (`:1556`), `M3_CourtVendor` (`:382`). | Greybox, NOT produced art. Presentation minimum: navigable + readable blockout massing + legible interaction feedback. Feasible-with-noted-risk: the risk is scene-authoring volume, not code. Scene governance applies (see Risks). Greybox readability is spatial clarity (sightlines, landmark massing) — never objective signposting; see the Sprint 3 feedback rule. `S3-05` must also establish first-time discoverability: from the player spawn position, the `M3_Caretaker` (the loop's entry point) is reachable through spatial readability alone — sightline, massing, layout legibility — with no marker. `/create-stories` sets the precise acceptance criterion; the plan-level requirement is that spawn-to-first-anchor discoverability is an explicit `S3-05` criterion, not left implicit. | Integration: runner / manual walkthrough proving the district is navigable and the three M3 anchors are reachable on foot; greybox readability check. | `S3-01` |
| [`S3-06`](../stories/s3-06-playable-end-to-end-and-human-play.md) | Playable end-to-end + human-play feel check | Prove the whole loop is human-playable end to end; carry the AC-06 human-play "feel" check transferred from `S2-M3-04`. | All of `S3-01`..`S3-05` composed. A NEW end-to-end runner; reuses the M3 telemetry-label vocabulary from `S2-M3-04`. | The human-play / "feel" acceptance criterion sits ONLY on this story (D016 + 2026-05-20 `tasks/lessons.md`). The new runner addresses only the runner-hygiene items that directly affect it: the un-resettable M2 melee-RNG (`m2_melee_rng_not_reset`) and runner exception-guarding. It does NOT take on the broader carried runner-hygiene debt. The human-play AC is pillar-anchored — see QA Plan Hooks. | Integration: end-to-end runner telemetry; PLUS human-play "feel" evidence answering the one-more-pull question. | `S3-02`, `S3-03`, `S3-04`, `S3-05` |

USER DECISION (recorded): the slate is 6 stories. There is no 7th runner-hygiene story. Sprint 2 runner-hygiene debt is carried forward as tracked carryovers (see Known Findings); `S3-06`'s new runner addresses only the hygiene items that directly affect it.

USER DECISION (recorded): the player interaction harness (`S3-01`) is a NEW standalone lightweight component — not a verb added to `M2SingleTrashMedLoopController`.

The technical-director consult also confirmed: player locomotion ALREADY EXISTS (`M2SingleTrashMedLoopController.cs:564-583`, WASD transform-driven movement + follow-camera driving `ClericShellMarker`). "Navigable" is therefore a property to *extend*, and there is no separate "add locomotion" story. The existing locomotion is greybox-grade (legacy `UnityEngine.Input`; no Input System package; no `CharacterController`) — acceptable for a Tier-1 greybox slice.

## Operating Model Calibration

- Prioritization test: **Does this make the Tier-1 objective loop genuinely human-playable end to end?** If no, defer it. (Sprint 3's milestone *is* playability, so this sharpens the Sprint 2 "first 10 minutes" test for this milestone.)
- Development loop: implement one small wiring increment, play it immediately, write down what felt bad, fix the worst thing, commit, repeat. Sprint 3 is assembly work — the play-immediately loop is the point.
- Reuse-not-rebuild discipline: the M3 systems are wired, not rewritten. Rewriting objective state, loot resolution, or the F4 vendor formula is a red flag (D016). The harness dispatches to existing `Try*` methods; it never reimplements objective/loot/vendor logic.
- No-new-systems discipline: Sprint 3 builds no new systems. New code is the orchestration/composition layer plus the player interaction harness; everything else is reuse.
- Feedback discipline (Pillar 2, *The Silence Is Sacred*): Sprint 3 feedback **acknowledges player action; it never advertises, locates, or routes**. If a feedback element is visible or audible before the player chooses to engage the thing, it is cut. Two-part test for any feedback element — (a) *trigger*: does it fire from a player verb, or ambiently/unprompted? (ambient-unprompted is cut); (b) *direction*: does it answer "what is this / did that work" (allowed) or "where do I go / what's next" (cut). An interact prompt ("Press [E]") is allowed only range-gated — never as a far-distance locator. Greybox readability is spatial clarity (sightlines, landmark massing), never objective signposting. Source: creative-director pillar-fit consult, 2026-05-21.
- Process calibration by batch class (per the established lesson): full rigor for the orchestration/composition layer, scene edits, evidence, and cross-contract wiring; the lighter implement-play-fix loop for greybox feel iteration. Human play is first-class acceptance for the end-to-end story (`S3-06`).
- **Review-subagent layer (Sprint 3 correction).** Sprint 3 does not Lean-skip the review gates. Every story's `/code-review` runs the standing review-subagent pair (`reviewer` + `qa-tester`), adding `unity-specialist` for the Unity-runtime-heavy stories (`S3-01`, `S3-05`, `S3-06`) and `gameplay-programmer` where harness/dispatch logic is under review. This reverses the Sprint 2 Lean-mode habit that quietly skipped specialist review (standing lesson `feedback_subagent_invocation`). "Process calibration by batch class" governs ceremony *depth*, not whether the subagent review runs — it always runs.
- Cleric-only lock holds: Tier 1 proves one playable archetype.
- Chain tables contain only actual full SHAs and grow append-only; no pending placeholder rows.
- External whole-codebase review is valuable once per sprint or tier transition; findings enter the next sprint unless they are immediate ship-blockers.

## Tier 2+ Cuts Preserved

Confirmed Tier-2+ cuts stand (DECISIONS.md D002, D003, D016):

- No multiplayer, FishNet, server authority, PvP, accounts, or cloud saves.
- No live LLM dialogue.
- No extra playable classes (no Warrior, no Enchanter).
- No second district, no huge world.
- No deep economy — the M3 vendor remains blockout-grade fixed-profile; no tuned economy, coin-pacing claim, or `CoinFaucetProjection_T1`.
- No broad AI companion system.
- No faction simulation beyond what M5 will later need; M5 (visible faction consequence) is deferred behind Sprint 3.
- No Save/Load; M4 is deferred behind Sprint 3.

## Known Findings

Live carryovers and findings that remain inputs to Sprint 3:

- **Carried runner-hygiene debt (USER DECISION: carried as tracked carryovers, not a 7th story).** Four runner-hygiene items remain open from Sprint 2; `S3-06`'s new runner addresses only the two that directly affect it (`m2_melee_rng_not_reset` and runner exception-guarding):
  - `m2_melee_rng_not_reset` — `M2SingleTrashMedLoopController`'s four `LoopingMeleeRandomSource` melee-RNG cursors (lines 76-79) are `readonly`, created once, never reset by `ResetLoop`/`ResetOverpullMetrics`/`ResetNamedBlockerMetrics`. Determinism holds for one smoke per Play session but breaks when smokes are chained. **Directly affects `S3-06`** if the end-to-end runner must chain M2 smokes; harden by resetting the `LoopingMeleeRandomSource` instances in `ResetLoop`.
  - `m3_04_low_review_notes` — non-blocking LOW notes on the `S2-M3-04` runner; item (1), `RunSmokeChecks` is not exception-guarded so an unexpected throw would hang the batchmode runner — a template-wide hardening candidate. The exception-guarding portion **directly affects `S3-06`**'s new runner.
  - Hardcoded evidence paths/dates: `s2_bridge_runner_evidence_path_hardcoded`, `launch_runner_evidence_path_hardcoded`, `m2_02_runner_date_hardcoded`, and `m3_03_low_review_notes` item (1) — runners with hardcoded dates/paths. Carried as tracked carryovers; out of scope for the 6-story slate.
- **Stale `production/sprints/sprint-2.md` Story Ledger rows.** The Sprint 2 plan's Story Ledger (lines 52-64) still shows `S2-M3-01` "Ready for Story Readiness", `S2-M3-02`/`-03`/`-04` "Blocked", and `S2-M3-00` with a "Pending closure commit". `production/sprint-status.yaml` correctly records all 11 Sprint 2 stories as `done`. The `sprint-status.yaml` is the source of truth; the `sprint-2.md` ledger rows are stale and were not reconciled at Sprint 2 close. Non-blocking for Sprint 3; flag for cleanup when `sprint-2.md` is next touched.
- **`game-concept.md` engine drift (`game_concept_engine_drift`).** `design/gdd/game-concept.md` still says engine TBD / Godot 4.6 pinned (`game-concept.md:262`) and lists `/setup-engine` as an open Next Step (`:353`); D001 locks Unity 6.3 LTS + C# + URP. Known finding; non-blocking for Sprint 3.
- **`control_manifest_absence_pre_existing`.** `docs/architecture/control-manifest.md` is absent project-wide. Sprint 2 M2/M3 stories cite Manifest Version headers and closed under this condition with absence recorded in evidence; the documented project handling is architecture-registry fallback (`production/qa/plans/qa-plan-sprint-2-20260509.md:54,60`). Sprint 3 stories will carry Manifest Version headers under the same documented fallback unless a sprint-close decision authors the manifest via `/create-control-manifest`. Non-blocking for Sprint 3 implementation.
- `m2_presentation_threshold_gap` is RETIRED (DECISIONS.md D016) and replaced by the Sprint 3 presentation-minimum bar: navigable greybox + readable blockout massing + legible interaction feedback.
- `m2_renderer_material_property_access` — `S2-M2-02` finding: `M2SingleTrashMedLoopController.cs:1168` reads `renderer.material` on the Update hot path. T1-acceptable; convert to `.sharedMaterial` + `MaterialPropertyBlock` before higher entity counts or presentation polish. If `S3-05` greybox work or the harness touches renderer state, watch this; otherwise non-blocking.
- `design/gdd/save-load-persistence.md` header says `Status: In Design` while its review log says APPROVED (`save_load_metadata_drift`); defer cleanup until M4 Save/Load story-breaking. Non-blocking for Sprint 3.
- README remains template-facing (`readme_template_facing`); should later become a Gravenspire landing page. Non-blocking.
- **`dotnet format` setup unconfirmed.** Codex Assignment #1 (`dotnet format` setup, branch `codex/dotnet-format-setup`) was drafted but its completion/merge status is not confirmed in current session state. The QA Plan Hooks **Style Gate** depends on `dotnet format --verify-no-changes` running cleanly — confirm the format configuration is in place and passing before the first Sprint 3 PR.

## Risks

| Risk | Probability | Impact | Owner | Mitigation |
|---|---|---|---|---|
| **Rebuild-temptation / orchestration-layer unowned.** The player marker and the M3 wrappers both exist but are NOT wired together — the M2 controller owns movement, the M3 wrappers are driven only by separate runners. Sprint 3's real deliverable is an orchestration/composition layer that is unowned today. Scope can quietly inflate from "wiring" into "rewriting objective/loot/vendor logic." | High | High | Producer (escalate to Technical Director on any rewrite signal) | Per-story scope boundary in the Story Ledger explicitly names the `Try*` method to dispatch to. Reviewers reject any PR that reimplements objective state, loot resolution, or the F4 vendor formula. The harness is a thin dispatch component (USER DECISION + D016 red-flag rule). `/scope-check` before each story closes. |
| **Scene fragility on `_DevEntry.unity`.** `S3-05` (greybox district) and the anchor-wiring stories all touch the shared `_DevEntry.unity` scene; the technical-director consult flagged `S3-05` as feasible-with-noted-risk specifically because of scene-authoring volume. Concurrent scene edits across implementers (Brian / Codex / Qwen) risk merge corruption. | Medium | High | Producer + implementer of each scene-touching story | Governance (`.claude/rules/game-dev-governance.md` Scene Discipline): one scene edit per PR; save-then-diff before staging; never hand-edit Unity scene YAML; Unity Smart Merge for conflicts. Sequence scene-touching stories rather than running them concurrently across worktrees. `S3-05` is the largest scene change — give it a clean window. |
| **Locomotion adequacy at district scale (untested).** The player mover (`M2SingleTrashMedLoopController.cs:564-583`, WASD + follow-camera, `PlayerMoveSpeedMeters = 4.0f`) was built for a small-radius M2 combat camp, not district-scale traversal. The TD consult confirmed the mover *exists* and is *drivable*; it did not assess whether it *feels* right across the `S3-05` district. | Medium | Medium | `S3-01` + `S3-05` implementers (escalate to Technical Director) | `S3-01` validates the mover feels acceptable as the harness wraps it; `S3-05` re-checks traversal feel at district scale in its implement-play-fix loop. A locomotion-feel problem here is a tuning fix (move speed, camera framing) within reuse scope; a *structural* mover rebuild is a red flag and escalates. Greybox bar: "navigable and not annoying," not polished movement. Stays inside the 6-story slate — no separate locomotion task. |
| **Carried runner-hygiene debt accumulates.** Four runner-hygiene carryovers (hardcoded paths/dates, un-resettable M2 melee-RNG, exception-guarding) ride forward unaddressed except where they touch `S3-06`. Stale dates/paths in evidence can mislead future readers; chained-smoke non-determinism can produce a false runner result. | Medium | Medium | Producer (tracked); `S3-06` implementer for the two in-scope items | `S3-06`'s new runner hardens the un-resettable M2 melee-RNG and adds exception-guarding (the two items that directly affect it). The remaining items stay tracked carryovers for a future hygiene effort. Evidence reviewers verify actual run dates against filenames rather than trusting embedded headers (the `index-lagging-canonical` pattern). |
| **Tier-1 / no-new-systems discipline slip.** Sprint 3 is assembly; any new system, Save/Load hook, faction-consequence wiring, tuned economy, second district, or extra class is Tier-creep or M4/M5 leakage. The objective loop is emotionally close to M5 (faction consequence), making leakage tempting. | Medium | High | Producer (escalate to Technical Director / Creative Director) | No-new-systems is a milestone constraint (D016). Per-story scope boundaries hold the Sprint 2 M3 non-goals. T1 negative-scope scan runs over changed files per story. Any creep is recorded as a `[SCOPE]` lesson in `tasks/lessons.md` per governance. |
| **Greybox readability under-delivers feel.** D016's presentation minimum is navigable + readable blockout massing + legible interaction feedback. If massing is unreadable or interaction feedback is illegible, the `S3-06` human-play check fails even though the systems work — repeating the `S2-M3-04` failure shape one layer up. | Medium | Medium | Producer + Creative Director (presentation-minimum owner) | `S3-05` acceptance includes a greybox readability check; interaction feedback legibility is an explicit `S3-01` concern. The `S3-06` human-play note classifies presentation limits honestly rather than hiding them. Feel is carried by interactivity and spatial readability, not texture fidelity. The `S3-06` human-play protocol must separate "does the loop pull me back" (the real Story 6 question) from "is it pretty" (out of scope per D016's presentation minimum) so a missing-art failure is not misread as a loop/pacing failure (R-P2-FEEL-MISATTRIBUTION). |
| **Owner assignment outstanding.** All Sprint 3 story files will carry empty `owner` fields until `/create-stories`. An unowned story has no accountable completion path; the orchestration layer being "unowned today" is the central risk above. | High (until resolved) | Medium | Producer | Assign exactly one accountable owner per story before the slate commits (D016). Implementer mix is Brian (lead), Codex (own worktree, D006), Qwen3-Coder (scoped mechanical edits only, D015 — not a `/dev-story` implementer). The orchestration/harness stories (`S3-01`, `S3-06`) need a design-aware owner (Brian or Codex), not Qwen. |
| **Pillar-2 convenience creep (R-P2-CONVENIENCE-CREEP).** Making the loop player-driven invites convenience affordances that violate Pillar 2 (*The Silence Is Sacred* — the anti-pillar list rejects map markers, quest arrows, auto-path, quest logs). Greybox navigation legibility or interaction feedback quietly drifts into *guidance*: colored path floors, a glowing objective door, an outlined NPC, an at-distance objective marker — added because they "help the tester." The interact prompt becoming a far-distance locator is this risk materializing in `S3-01`, the first story. The other concrete pressure point: if `S3-05` does not solve spawn-to-`M3_Caretaker` discoverability through spatial readability, the temptation to add a marker/arrow/glow to "help the tester" is exactly where this risk lands. | Medium | High | Creative Director + Producer | The Sprint 3 feedback rule (Operating Model Calibration): every navigation and feedback element passes the trigger test (fires from a player verb, not ambiently) and the direction test (answers "what is this / did that work", not "where do I go"). Greybox readability is spatial clarity — sightlines, landmark massing, layout legibility — never objective signposting. Reviewers reject any feedback element visible or audible before the player chooses to engage the thing. Per-story pillar checks route through a creative-director gate at `/create-stories`. |
| **Pillar-1 protagonist drift (R-P1-PROTAGONIST-DRIFT).** Wiring the loop behind player input can make the First District read as existing *to be solved by the player*, and the `M3_Caretaker` read as a quest-dispenser servicing the player rather than a person with a problem the player chose to help (anti-pillar: *NOT Skyrim* — no chosen-one fantasy; Pillar 1 — *The World Is Not Your Story*). | Low-Medium | Medium | Creative Director | `S3-02`'s interaction framing keeps the NPC's posture "I have a problem", not "I have a task for the chosen one." Cheap to honor at greybox/templated-text stage; expensive to retrofit after the loop is wired. Creative-director review of `S3-02` acceptance criteria at `/create-stories`. |

## QA Plan Hooks

A Sprint 3 `/qa-plan` follows this plan's approval and precedes implementation. Hooks for that plan:

- **Story types.** All six stories are Integration type (multi-system wiring), Gameplay / Unity Runtime layer — matching the `S2-M3-*` headers. `S3-05` additionally carries scene-authoring (Visual/Feel) characteristics for its greybox readability check. Per `coding-standards.md`, Integration stories require an integration test OR documented playtest (BLOCKING gate); Visual/Feel evidence is screenshot + lead sign-off (ADVISORY).
- **Human-play AC on `S3-06` only.** The human-play / "feel" acceptance criterion may sit ONLY on a story that is genuinely human-playable end to end — i.e., `S3-06` only (D016 + the 2026-05-20 `tasks/lessons.md` definition-of-done lesson). Stories `S3-01`-`S3-05` feel-test continuously during the implement-play-fix loop, but their **binding acceptance gates are mechanical / integration** (runner telemetry, navigability checks). Never bolt a human-play AC onto an earlier, still-partial story. Playability is a milestone *entry* condition for the `S3-06` feel check, not a final-story afterthought.
- **AC-06 transfer (pillar-anchored).** `S3-06`'s human-play AC is the `S2-M3-04` AC-06 feel-validation transferred forward (DECISIONS.md D016; `tests/evidence/S2-M3-04/human-play-20260520.md` is the originating record) — now genuinely answerable because the loop is player-driven. Per the creative-director pillar consult, the feel question is anchored to the core fantasy, not raw mechanics: **pass = after completing the loop once, the playtester voluntarily re-engages and, asked why, names the objective / NPC / relic as the reason — not raw XP, not "the game told me to"; fail = re-engagement only for mechanical reward, or not at all.** This measures whether the *loop is worth playing*, not merely whether it *functions*. Methodological limit, named honestly: on a solo project the `S3-06` playtester is also the designer and implementer — an N=1 self-test with known selection bias. The `S3-06` human-play evidence must state this limitation explicitly (as `tests/evidence/S2-M3-04/human-play-20260520.md` classified its presentation limits), not hide it; if a second playtester is available, both reads are recorded. The plan does not assume one exists.
- **Evidence baseline.** Each Sprint 3 story produces a `tests/evidence/S3-NN/verification.md`, story-specific Unity Play Mode / batchmode runner output, a T1 negative-scope scan over changed files, `git diff --check`, and `.githooks/pre-commit`. The end-to-end story adds the human-play note. `dotnet test` Combat regression runs where Combat Core or M2 regression coverage is in scope.
- **M2 preservation.** Each story that touches shared M2 surfaces (the controller, `_DevEntry.unity`, the scene anchors) re-verifies M2 clean-loop / overpull / named-blocker preservation, consistent with the `S2-M3-*` pattern.
- **Style gate.** Tier 1: `dotnet format --verify-no-changes` must pass locally before each PR (`.claude/rules/game-dev-governance.md` Code Style Gate).

## Capacity Assumptions

- **Sprint start:** 2026-05-21.
- **Estimate basis:** Sprint 2 M3 stories landed at ~1.0 day each (`production/sprint-status.yaml` `estimate_days`: `S2-M3-00` through `S2-M3-04` all 1.0). Sprint 3 stories are sized against that actual, with confidence and risk multipliers below. Story files carry no estimates until `/create-stories`; these are planning estimates, not committed dates.
- **Per-story estimate and confidence:**

  | ID | Estimate | Confidence | Basis / risk multiplier |
  |---|---:|---|---|
  | `S3-01` | 1.5 days | MEDIUM | NEW standalone component (not pure reuse); the orchestration layer is unowned today. Higher than the 1.0 M3 baseline because it is greenfield composition code. |
  | `S3-02` | 1.0 day | HIGH | Thin player-input wiring onto `TryRecordIntentionalInteraction` (`:112`); the NPC frame and anchor already exist. Depends on `S3-01`. |
  | `S3-03` | 1.0 day | MEDIUM | Wires `TryRecoverRelic`/`TryAcceptObjectiveFromNpc` + `TryResolveObjectiveLoot`; objective-state + loot are two surfaces, slightly more wiring than `S3-02`. |
  | `S3-04` | 1.0 day | HIGH | Thin player-input wiring onto `TrySellRecoveredSalvage` (`:101`); the F4 vendor already exists. |
  | `S3-05` | 1.5 days | LOW | Feasible-with-noted-risk per the technical-director consult — scene-authoring volume on `_DevEntry.unity`, not code. LOW confidence reflects scene-volume uncertainty; the largest single scene change in the sprint. |
  | `S3-06` | 1.0 day | MEDIUM | New end-to-end runner + human-play check; reuses the `S2-M3-04` telemetry vocabulary. Carries two in-scope runner-hygiene fixes (M2 melee-RNG reset, exception-guarding). |

- **Raw planned total:** ~7.0 implementer-days across 6 stories.
- **Buffer:** reserve ~20% sprint capacity for unplanned work — integration friction on the unowned orchestration layer, scene-merge coordination across worktrees, bug fixing, review turnaround, and the two `S3-06` runner-hygiene fixes. Planned work is held at ~7.0 days; buffer is not spent in this plan.
- **Proposed end date:** with a 5-day implementer week, ~7.0 planned days + ~20% buffer ≈ 9 working days of capacity → a realistic end date of **2026-06-03** (single-implementer-equivalent). The implementer mix (Brian lead; Codex parallel per D006; Qwen3-Coder scoped mechanical edits only per D015) can compress the calendar via Codex parallelism — but parallelism is bounded by the dependency chain (`S3-01` gates everything; `S3-02`-`S3-04` are a linear NPC→relic→vendor sequence; `S3-05` parallels off `S3-01`; `S3-06` needs all of `S3-02`-`S3-05`). The realistic parallel opportunity is `S3-05` running alongside `S3-02`-`S3-04`; the linear vendor chain is the critical path.
- **Critical path:** `S3-01` → `S3-02` → `S3-03` → `S3-04` → `S3-06`. `S3-05` is not on the critical path (it parallels off `S3-01`) but it is the LOW-confidence story and the largest scene change — if it slips, `S3-06` slips, so it needs early scheduling and a clean scene window.
- **Owner assumptions:** owner fields are empty until `/create-stories`; assignment is a pre-commit gate (D016). The greenfield orchestration/harness stories (`S3-01`, `S3-06`) require a design-aware owner — Brian or Codex, NOT Qwen3-Coder, which is scoped to small mechanical edits only and is not a `/dev-story` implementer (D015). Qwen3-Coder may assist with isolated mechanical sub-edits under its mandatory `/code-review` gate, but no Sprint 3 story should be owned by it.
- **Capacity confidence:** MEDIUM overall. The estimate basis (Sprint 2 actuals) is sound, but `S3-01` is greenfield composition and `S3-05` is scene-volume-uncertain — both carry real estimate risk. This plan does not mark the sprint REALISTIC; that verdict belongs to a `/qa-plan` and a `PR-SPRINT` gate once owners and the `/create-stories` slate exist.

## Next Gate

Sprint 3 planning is at the pre-`/create-stories` stage. The next command is:

`/create-stories` for the 6-story slate (`S3-01` through `S3-06`), using this plan and `design/quick/quick-design-m3-objective-npc-loot.md` as input.

Before the slate commits, assign one accountable `owner` per story. A Sprint 3 `/qa-plan` follows plan approval and precedes implementation. The Sprint 3 `sprint-status.yaml` is generated when `/create-stories` opens the slate; until then `production/sprint-status.yaml` records Sprint 2 as closed.

## Definition Of Done For Sprint 3 Planning

- [ ] Sprint 3 plan (`production/sprints/sprint-3.md`) exists and is approved.
- [ ] The 6-story slate (`S3-01`-`S3-06`) is opened via `/create-stories` with the dependency order in this plan.
- [ ] Every Sprint 3 story has exactly one accountable `owner` assigned.
- [ ] Sprint 3 `/qa-plan` exists and names the human-play-AC-on-`S3-06`-only rule.
- [ ] No Sprint 3 implementation starts before the QA plan.
- [ ] The slate carries no 7th runner-hygiene story; the four runner-hygiene carryovers are tracked, with the two in-scope items assigned to `S3-06`.
- [ ] Every story's scope boundary names the specific M3/M2 `Try*` surface it reuses and forbids rewriting objective/loot/vendor logic.
