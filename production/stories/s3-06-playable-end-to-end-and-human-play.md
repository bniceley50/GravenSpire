# S3-06: Playable End-to-End + Human-Play Feel Check

> **Sprint**: Sprint 3 — Playable Vertical-Slice Assembly
> **Sprint Plan**: `production/sprints/sprint-3.md` (Story Ledger row, line 71)
> **Status**: Mechanically complete; human-play feel gate NOT PASSED (presentation-readability); Sprint 4 presentation re-plan pending
> **Layer**: Core / Integration
> **Type**: Integration (primary) + Visual/Feel (the human-play AC carries the feel evidence — see ADVISORY/BLOCKING split in QA Test Cases)
> **Estimate**: 1.0 day (MEDIUM confidence — per `sprint-3.md:160`; new runner reuses telemetry vocabulary, but the M2 melee-RNG fix touches the 2156-line M2 controller and the human-play protocol is non-trivial)
> **Manifest Version**: Unavailable (control-manifest absent project-wide per `production/qa/plans/qa-plan-sprint-2-20260509.md:54,60`; documented fallback applies)
> **Generated**: 2026-05-23
> **Owner**: Codex (design-aware owner per `sprint-3.md:166`; Qwen3-Coder not eligible)

> **Pattern-establishing notice**: this is the slate's emotional payload story and the second Sprint 3 story to establish a new evidence pattern. S3-05 established the greybox spatial-validation pattern; S3-06 establishes the **Tier-1 human-play feel-check pattern** — a pillar-anchored binary verdict with structured supporting evidence, N=1 self-test selection bias acknowledged, optional second-playtester both-reads-recorded. The shape S3-06 takes for the human-play protocol becomes the precedent for any future "is this loop worth playing" feel-check story. Inherits the structural template from `tests/evidence/S2-M3-04/human-play-20260520.md` (the originating deferral artifact). Recorded for Sprint 3 close-out promotion to `tasks/lessons.md` as the "T1 human-play feel-check evidence pattern" — bundled with the S3-05 spatial-validation pattern and the `feedback_external_review_verification` promotion already pending.

## Context

**Sprint 3 plan**: `production/sprints/sprint-3.md`
**Quick-design source**: `design/quick/quick-design-m3-objective-npc-loot.md`
**Story Ledger row**: `production/sprints/sprint-3.md:71`
**QA Plan Hook (pillar-anchored feel protocol)**: `production/sprints/sprint-3.md:141-142`
**Inherited deferral evidence**: `tests/evidence/S2-M3-04/human-play-20260520.md` (S2-M3-04 AC-06 deferred to S3-06)
**Requirement IDs (story-local ACs)**: `S3-06-01` through `S3-06-10`

**Requirement Summary**: Prove the Tier-1 objective loop is playable end-to-end as a single human-driven session, validated by both (a) a new end-to-end runner that asserts the full S3-02/03/04 telemetry vocabulary fires in correct order across the S3-05 district, and (b) a pillar-anchored human-play feel check answering: after completing the loop once, does the playtester voluntarily re-engage AND name the objective / NPC / relic as the reason (not raw XP, not "the game told me to"). The runner takes on two scoped runner-hygiene items that directly affect it — fixing the un-resettable M2 melee-RNG cursors in `M2SingleTrashMedLoopController.cs:76-79` so chained M2 smokes are deterministic, and exception-guarding the new runner's `RunSmokeChecks` per `m3_04_low_review_notes` item 1. No new game systems; this story is the slate's composition + emotional payload + bounded hygiene.

**Governing decisions** (DECISIONS.md):

| D-entry | Status | Usage |
|---|---|---|
| D001 (`DECISIONS.md:14`) | Locked | Unity 6.3 LTS + C# + URP |
| D003 (`DECISIONS.md:51`) | Locked | Tier 1 single-player offline — solo human-play, no co-op feel-check, no remote playtester |
| D012 (`DECISIONS.md:342`) | Locked | T1 combat-feel validated — the combat camp inside the district is feel-validated; this story validates the *objective-loop* feel on top of that |
| D016 (`DECISIONS.md:554`) | Locked | "Greybox, not art" presentation minimum — human-play feel-check separates loop-feel from art-feel per R-P2-FEEL-MISATTRIBUTION |
| D020 (`DECISIONS.md` on main commit `46c6d1b`) | Locked | S3-06 is mechanically complete but the human-play feel gate did not pass; presentation-readability work moves to the art-bible revision and Sprint 4, not an in-story pass flip |

**Sprint 3 feedback rule** (`sprint-3.md:85`): applies to runner output as well as gameplay feedback. The human-play protocol's verdict text and limitations table follow the same trigger/direction discipline — describe what happened and what couldn't be tested, not what the player should do next time.

**Post-playtest outcome (2026-05-30)**: S3-06's runner evidence supports AC-01 through AC-06 mechanically, but the project lead's N=1 human-play attempt did **not** pass the AC-08 feel gate. The failure is recorded as presentation-readability: the slice read as Unity greybox/debug scaffolding rather than a playable classic-MMO-descended gothic slice. The legacy M2 combat debug HUD bleeding over the S3 objective loop was a real presentation bug; the `M2SingleTrashMedLoopController` HUD suppression is kept as a scoped S3-06 presentation-readiness bug fix, but it does **not** retroactively convert the feel gate into PASS.

**Pillar 1 risk (R-P1-PROTAGONIST-DRIFT, `sprint-3.md:134`)**: the "named the objective/NPC/relic as the reason" attribution test cuts against the "world built for you" failure — pass means the player engaged because they cared about the *world's problem*, not because the game prompted them to.

**Pillar 2 risk (R-P2-FEEL-MISATTRIBUTION, `sprint-3.md:131`)**: the human-play protocol must separate "does the loop pull me back" (the real S3-06 question) from "is it pretty" (out of scope per D016 greybox-not-art). A missing-art failure must not be misread as a loop/pacing failure. The limitations table classifies any presentation deficit explicitly so the verdict isn't contaminated.

**Architecture Module**: Gameplay / End-to-End Composition + M2 Controller Hygiene (new runner; targeted modification to M2 controller for melee-RNG reset)
**Engine**: Unity 6.3 LTS
**Engine Risk**: LOW (composes existing APIs; no new Unity feature surface)

**Surfaces composed** (do not re-author):
- S3-01 player interaction harness — drives all player input
- S3-02 NPC adapter (with S3-03 state-routing) on `M3_Caretaker` — drives objective accept (NotIntroduced→Accepted) and hand-in (RelicRecovered→Complete)
- S3-03 relic adapter on `M3_ObjectiveRelic` — drives relic recovery + objective loot resolution
- S3-04 vendor adapter on `M3_CourtVendor` — drives salvage sale + currency credit
- S3-05 navigable greybox First District — provides the walked-traversal space; AC-12 composite assertion rolls forward into this story
- M2 combat camp area (preserved within the district) — preservation reruns gate closure
- Telemetry vocabulary established in S3-02/03/04: `npc_interaction_intentional`, `objective_accepted`, `relic_recovered`, `objective_loot_resolved`, `objective_loot_resolution_failed` (partial-success path), `vendor_salvage_sold`, `vendor_sell_copper_applied`, `relic_handed_in`

**Surfaces modified** (bounded hygiene only):
- `Assets/Scripts/M2SingleTrashMedLoopController.cs:76-79` — the four `LoopingMeleeRandomSource` melee-RNG cursors. Modification: add reset hooks invoked by `ResetLoop` / `ResetOverpullMetrics` / `ResetNamedBlockerMetrics` so chained M2 smokes within a single Play session produce deterministic results matching independent invocations. Implementation choice (e.g., add `Reset()` method to `LoopingMeleeRandomSource` and invoke; or make cursors mutable and reassign) is deferred to dev-story.

## Acceptance Criteria

### End-to-end runner (composition)

- [ ] **S3-06-01**: A new Editor batchmode runner `GravenspireS3PlayableEndToEndRunner` is created under `Assets/Editor/`. It composes the S3-01 harness, S3-02/03 NPC adapter, S3-03 relic adapter, S3-04 vendor adapter, and the S3-05 district. No new dispatch logic, no new adapters, no new M3 system code — the runner is composition only.
- [ ] **S3-06-02**: The runner drives a scripted player-input sequence through the full Tier-1 objective loop in the S3-05 district: spawn → walk to `M3_Caretaker` → interact (objective accept, NotIntroduced→Accepted) → walk to `M3_ObjectiveRelic` → interact (relic recovery + objective loot resolution, Accepted→RelicRecovered) → walk to `M3_CourtVendor` → interact (salvage sell + currency credit) → walk back to `M3_Caretaker` → interact (hand-in, RelicRecovered→Complete). Each walk segment uses harness locomotion (no debug teleport).
- [ ] **S3-06-03**: The runner asserts the full S3-02/03/04 telemetry vocabulary fires in correct order across the sequence:
  - From the Caretaker accept dispatch: `npc_interaction_intentional` then `objective_accepted` (order forced by `TryAcceptObjectiveFromNpc` internal call sequence at `M3ObjectiveStateRelicHandIn.cs:65→71`, per S3-03 AC-03)
  - From the relic dispatch: `relic_recovered` then `objective_loot_resolved` (full-success shape, per S3-03 AC-08)
  - From the vendor dispatch: `vendor_salvage_sold` then `vendor_sell_copper_applied` (order is adapter-side contract per S3-04 AC-04)
  - From the hand-in dispatch: `relic_handed_in` (no `npc_interaction_intentional` — hand-in does not internally record, per S3-03 AC-04)

  Final state: `objectiveState.State == Complete`, `vendor.CarriedCurrencyCopper > 0`, `vendor.CarriesCourtMarkedRelic == true` (still carried — relic wasn't sold; the hand-in transition completed the state machine but didn't transfer the relic from vendor inventory in T1 scope).

  **This AC subsumes S3-05's AC-12 composite assertion** — S3-05's AC-12 closure semantics rolled the full-chain assertion forward into this canonical place. If S3-05 closed "Done with Notes" on AC-12 (carryover `s3_05_ac12_partial_rollforward_to_s3_06`), this AC's pass resolves it.

### Runner-hygiene (scoped to this runner only)

- [ ] **S3-06-04**: The M2 melee-RNG reset (`m2_melee_rng_not_reset` carryover) is implemented in `M2SingleTrashMedLoopController.cs`: the four `LoopingMeleeRandomSource` melee-RNG cursors at `:76-79` are reset by `ResetLoop` / `ResetOverpullMetrics` / `ResetNamedBlockerMetrics` such that chained M2 smokes within a single Play session produce deterministic results matching independent invocations. **Regression check**: each M2 smoke run independently (one per Play session) must continue to produce the same telemetry it produced pre-S3-06. The reset is additive — behavior between scenarios changes, behavior within a single scenario does not.
- [ ] **S3-06-05**: The new `GravenspireS3PlayableEndToEndRunner`'s `RunSmokeChecks` (or equivalent) is **exception-guarded** per `m3_04_low_review_notes` item 1 — an unexpected throw within a check produces a clear FAIL entry in the runner output with the exception type and message, and the runner proceeds to subsequent checks rather than hanging the batchmode invocation. Pattern: try/catch around each named check; runner exits with non-zero status code if any check FAILED or threw.
- [ ] **S3-06-06**: M2 preservation in the same Play session: after the M3 end-to-end sequence completes, the runner invokes the M2 named-blocker boundary smoke (or all three M2 smokes if AC-04's reset implementation enables chaining). M2 PASS in the same Play session is the proof that the AC-04 reset works. Without the reset, this assertion would be the bogus-RNG failure mode that originated `m2_melee_rng_not_reset`; with the reset, it's a positive demonstration. The runner output documents which M2 smokes ran chained and confirms determinism.

### Human-play feel check (the slate's emotional payload)

- [ ] **S3-06-07**: A human-play session is run by the project lead with the build at the head of this story's implementation branch (post-AC-01–06 mechanical pass). The playtester:
  - Completes the full Tier-1 objective loop one time as a real player (spawn, find Caretaker, accept objective, find relic, recover, find vendor, sell salvage, return to Caretaker, hand in)
  - **The test protocol pauses the session at loop completion and asks** (regardless of whether the playtester would otherwise re-engage immediately): "Would you do that again right now?" — yes/no recorded
  - If yes: is asked "Why?" — answer recorded verbatim
  - If no: is asked "What would change that?" — answer recorded verbatim (this informs polish-vs-loop-deficit attribution)

- [ ] **S3-06-08**: The human-play verdict is **binary, pillar-anchored** per `sprint-3.md:142`:
  - **PASS**: playtester answers "yes" to re-engagement AND names the **objective / NPC / relic** (the world's elements) as the reason — not raw XP, not "the game told me to," not "I wanted to test the next thing"
  - **FAIL**: re-engagement only for mechanical reward (e.g., "to get more copper", "to level"), OR no re-engagement, OR re-engagement for meta reasons (testing, completionism)

  This measures whether the loop is **worth playing**, not whether it functions. AC-01–06 prove it functions; AC-08 proves it pulls.

- [ ] **S3-06-09**: The human-play evidence (`tests/evidence/S3-06/human-play-[YYYYMMDD].md`) inherits the structural shape from `tests/evidence/S2-M3-04/human-play-20260520.md`:
  - Header: story, AC ref, date, played by, build SHA
  - **What was attempted** (the loop walk-through)
  - **What was found** (re-engagement answer + verbatim "why" attribution)
  - **The re-engagement question and attribution test** (verdict computation per AC-08)
  - **Presentation limitations (classified, not hidden)** table with severity column (BLOCKING vs Tolerable) — applies the R-P2-FEEL-MISATTRIBUTION carve-out (greybox aesthetic deficits are Tolerable for the loop-feel verdict; only loop-mechanical deficits are BLOCKING)
  - **Methodological limit, named honestly**: "On a solo project, the playtester is the designer and implementer — an N=1 self-test with known selection bias. The verdict is recorded under this constraint." Verbatim from plan `sprint-3.md:142`.
  - **Playtester's verbatim feedback** (full transcription of the "why" / "what would change that" answer)
  - **Verdict** (PASS or FAIL, with the AC-08 binary attribution test computation)
  - **Second-playtester read if available** (separate section, both reads recorded; absent section if no second playtester — the plan does not assume one exists)

- [ ] **S3-06-10**: The human-play evidence explicitly classifies any presentation-deficit feedback against the R-P2-FEEL-MISATTRIBUTION carve-out — if the playtester says "the relic looked bad" or "the vendor was visually boring," those are Tolerable per D016 greybox-not-art and do NOT factor into the AC-08 verdict. Only feedback about the *loop* (pacing, anticipation, reward, agency, world-investment) factors into the binary attribution test. The limitations table makes this explicit per item, so a future reviewer can re-derive the verdict from the structured evidence.

## Implementation Notes

- **Full-slate dependency**: this story cannot start until S3-02, S3-03, S3-04, and S3-05 are all closed. Partial implementation against an incomplete slate is not useful — the human-play AC requires the full loop, and the end-to-end runner asserts the full telemetry vocabulary. If S3-05 closed "Done with Notes" on its AC-12, this story's AC-03 resolves that carryover.
- **M2 controller modification scope (AC-04)**: target only lines `:76-79` and the three reset methods (`ResetLoop`, `ResetOverpullMetrics`, `ResetNamedBlockerMetrics`). Do NOT take on broader M2 controller refactoring; the `m2_controller_scenario_smoke_abstraction` carryover (controller is 2156 lines, partial S2-M3-00 progress) is OUT of scope for this story. Reviewer rejects any unrelated change to the controller.
- **M2 controller modification approach (deferred to dev-story)**: the cursors are `readonly` (created once, never reassigned). Two implementation paths:
  1. Add a `Reset()` method to `LoopingMeleeRandomSource` that re-initializes its internal state; call it from the three M2 reset methods
  2. Make the cursors mutable (`private LoopingMeleeRandomSource _cursor;`) and reassign in the reset methods

  Choice depends on `LoopingMeleeRandomSource`'s shape (immutable seed vs mutable state). Implementation reviews preference (1) since it preserves the `readonly` field convention; (2) is acceptable if (1) is harder.
- **Pre-existing M2 smoke regression evidence**: AC-04 requires each M2 smoke run independently (one per Play session) to produce the same telemetry post-change as pre-change. The dev-story's evidence must include independent-invocation re-runs of `M2SingleTrashLoop`, `M2LinkedTrashOverpull`, and `M2NamedBlockerBoundary` and compare telemetry against the latest pre-S3-06 smoke evidence (from S3-05's preservation reruns). Identical telemetry = additive reset confirmed.
- **Runner-hygiene scope is BOUNDED**: only `m2_melee_rng_not_reset` (AC-04) and exception-guarding in the new runner (AC-05). The other carryovers (`s2_bridge_runner_evidence_path_hardcoded`, `launch_runner_evidence_path_hardcoded`, `m2_02_runner_date_hardcoded`, `m3_03_low_review_notes` item 1) are explicitly NOT in scope per `sprint-3.md:112`. Reviewer rejects any drift into those items.
- **Human-play protocol — solo-tester N=1 honesty**: the lead is both the designer and the playtester. This is genuinely a known selection bias. The AC-09 evidence must state this explicitly (not as a disclaimer, as a methodological context). The plan does not assume a second playtester exists; if one is available (e.g., a friend or external playtester), both reads go into the evidence file as separate sections. Disagreement between reads is recorded without resolution — the human-play AC's verdict goes with the lead's read by convention, with the second read as supporting evidence (or counter-evidence).
- **R-P2-FEEL-MISATTRIBUTION discipline (AC-10)**: this is the substantive design discipline for the human-play protocol. Greybox aesthetic deficits ("the relic is just a capsule") are Tolerable per D016 — they do NOT factor into the loop-feel verdict. Loop-mechanical deficits ("recovering the relic felt anticlimactic") DO factor. The limitations table forces per-item classification; a reviewer reading the evidence later can see the line and re-derive the verdict.
- **Pattern-establishing precedent**: capture the human-play protocol shape carefully — the question phrasing ("Would you do that again right now?" and "Why?"), the binary attribution test, the limitations table with R-P2-FEEL-MISATTRIBUTION classification, the N=1 acknowledgment, the second-playtester optional both-reads. These become the template for any future Tier-1 feel-check story. Sprint 3 close-out promotion candidate to `tasks/lessons.md`.
- **No DateTime.UtcNow** in runner code; **scene discipline** (this story does NOT modify `_DevEntry.unity` — the runner composes existing scene state); **style gate** — same as all preceding stories.

## Out of Scope

- No new game systems (composition only)
- No new dispatch logic, no new adapters, no new M3 system code
- No second human-playtester *required* (optional; both reads recorded if available)
- No remote/multi-player feel-check (D003 single-player offline; co-op feel is Tier 2+)
- No A/B testing of feel variations (one verdict per playtester, one build)
- No subjective scorecards beyond the AC-08 binary (e.g., "rate the relic recovery 1-5" — explicitly NOT this shape; the plan committed to binary pillar-anchored verdict)
- No broader M2 controller refactoring beyond AC-04's targeted reset (the `m2_controller_scenario_smoke_abstraction` carryover stays open)
- No backporting exception-guarding to other runners (only the new S3-06 runner per AC-05)
- No address of unrelated runner-hygiene carryovers (`s2_bridge_runner_evidence_path_hardcoded`, `launch_runner_evidence_path_hardcoded`, `m2_02_runner_date_hardcoded`, `m3_03_low_review_notes`)
- No M4 (Save/Load) hook (deferred behind Sprint 3 per D016)
- No M5 (faction consequence) wiring (deferred behind Sprint 3 per D016)
- No polish work — if the human-play verdict is FAIL, the next step is a Sprint 3+ polish story or a milestone re-plan, NOT in-this-story polish iteration

## QA Test Cases

### Mechanical: end-to-end runner + hygiene

**Test S3-06-T1: end-to-end runner full chain (AC-01, AC-02, AC-03)**
- Given: full S3-01..S3-05 chain closed; fresh `_DevEntry.unity` Play Mode session via `Unity.exe -batchmode -executeMethod GravenspireS3PlayableEndToEndRunner.Run`.
- When: the runner drives the scripted player-input sequence through the loop.
- Then: telemetry sequence is exactly — `npc_interaction_intentional` → `objective_accepted` (from Caretaker accept); `relic_recovered` → `objective_loot_resolved` (from relic); `vendor_salvage_sold` → `vendor_sell_copper_applied` (from vendor); `relic_handed_in` (from hand-in). State sequence: NotIntroduced → Accepted → RelicRecovered → Complete. Final assertions: `objectiveState.State == Complete`, `vendor.CarriedCurrencyCopper > 0`. Runner exits 0.
- Edge cases: runner invoked against a build where S3-02/03/04 partially regressed → runner FAILs with the specific missing telemetry event named in the output; runner exits non-zero.

**Test S3-06-T2: M2 melee-RNG reset additive (AC-04)**
- Given: M2 controller modified per AC-04; pre-S3-06 baseline telemetry recorded for each of the three M2 smokes run independently.
- When: each M2 smoke is re-run independently (one per Play session) against the modified controller.
- Then: telemetry from each independent rerun matches the pre-S3-06 baseline (the reset is additive — behavior within a single scenario is unchanged). Evidence: side-by-side telemetry comparison committed to `tests/evidence/S3-06/m2-regression-comparison-[YYYYMMDD].md`.
- Edge cases: if telemetry diverges, AC-04 FAILs — the reset was not additive; implementer reviews whether the reset is firing at the wrong point (during the scenario instead of between scenarios) or whether the cursors needed different reset semantics.

**Test S3-06-T3: runner exception-guarding (AC-05)**
- Given: a synthesized failure mode injected into one of the runner's checks (e.g., null reference, throw on assertion).
- When: the runner executes that check.
- Then: a clear FAIL entry appears in the runner output naming the exception type, the check name, and the message; the runner proceeds to subsequent checks; the runner exits with non-zero status code (because at least one check FAILED).
- Edge cases: if the runner hangs on an injected exception, AC-05 FAILs — the exception-guard didn't wrap that check; implementer adds the missing guard.

**Test S3-06-T4: M2 preservation in chained Play session (AC-06)**
- Given: AC-04 reset implemented and AC-T2 passed.
- When: the new runner drives the M3 end-to-end sequence AND immediately chains the M2 named-blocker boundary smoke (or all three M2 smokes if the implementation supports chaining) within the same Play session.
- Then: M2 smoke PASSes with telemetry matching its independent-invocation baseline. The runner output documents which M2 smokes ran chained and confirms determinism.
- Edge cases: if M2 telemetry diverges from baseline, the reset is not firing between scenarios → AC-06 FAILs; root-cause via the same path as T2.

### Human-play feel check

**Test S3-06-T5: human-play protocol — primary playtester (AC-07, AC-08, AC-09, AC-10)** — BLOCKING manual
- Playtester: project lead (Brian or designated).
- Setup: build at head of S3-06 implementation branch with AC-01–06 mechanical passes confirmed.
- Protocol: open Unity, Play Mode, complete one full loop per AC-07; the test protocol pauses the session at loop completion and asks "Would you do that again right now?" (yes/no); if yes, "Why?" (verbatim); if no, "What would change that?" (verbatim).
- Verdict: AC-08 binary attribution test applied to the verbatim answer.
- Evidence: written to `tests/evidence/S3-06/human-play-[YYYYMMDD].md` per AC-09 structural template with AC-10 R-P2-FEEL-MISATTRIBUTION classification.
- Edge cases: playtester answers "yes" but for meta reasons (testing, completionism) → FAIL per AC-08; playtester would re-engage immediately on their own — the test protocol still pauses and asks "why" post-loop, attribution applies; playtester gets stuck mid-loop (e.g., can't find an anchor) → AC-T5 records the friction in the limitations table; if the friction is loop-mechanical (not aesthetic), it's a BLOCKING limitation for the verdict.

**Test S3-06-T6: human-play protocol — second playtester (optional)** — ADVISORY
- If a second playtester is available, repeat T5 protocol with that playtester; record their reads as a separate section in the same `human-play-[YYYYMMDD].md` evidence file.
- Both reads are evidence; disagreement is recorded without resolution; the lead's read is the verdict by convention, the second read is supporting evidence (or counter-evidence).
- Edge cases: no second playtester available → AC-T6 absent (not failed). Plan does not assume one exists.

### M2 preservation reruns (additional required evidence — independent invocations)

Per the established pattern AND now as the AC-04 additive-reset evidence baseline:

- `M2SingleTrashLoop` smoke independent rerun — PASS, exit 0, telemetry matches pre-S3-06 baseline
- `M2LinkedTrashOverpull` smoke independent rerun — PASS, exit 0, telemetry matches pre-S3-06 baseline
- `M2NamedBlockerBoundary` smoke independent rerun — PASS, exit 0, telemetry matches pre-S3-06 baseline

(Each runs in its own batchmode invocation per `m2_melee_rng_not_reset` — but ALSO note that post-AC-04 this is no longer strictly required; the AC-06 chained-session test demonstrates the reset works. The independent-invocation baseline reruns are the regression-evidence for AC-04.)

## Test Evidence

**Required evidence**: `tests/evidence/S3-06/verification.md`

Companion artifacts:
- `tests/evidence/S3-06/unity-playable-end-to-end-[YYYYMMDD]-smoke.md` (T1 batchmode runner output covering AC-01–03)
- `tests/evidence/S3-06/m2-regression-comparison-[YYYYMMDD].md` (T2 side-by-side telemetry comparison proving AC-04 additive-reset)
- `tests/evidence/S3-06/unity-runner-exception-guard-[YYYYMMDD]-smoke.md` (T3 synthesized-failure runner output covering AC-05)
- `tests/evidence/S3-06/unity-end-to-end-chained-m2-[YYYYMMDD]-smoke.md` (T4 chained-session runner output covering AC-06)
- `tests/evidence/S3-06/human-play-[YYYYMMDD].md` (T5 primary playtester evidence with AC-09 structure + AC-10 R-P2-FEEL-MISATTRIBUTION classification)
- `tests/evidence/S3-06/m2-02-preservation-[YYYYMMDD]-smoke.md` (independent invocation; baseline for AC-04 regression)
- `tests/evidence/S3-06/m2-03-preservation-[YYYYMMDD]-smoke.md` (independent invocation)
- `tests/evidence/S3-06/m2-04-preservation-[YYYYMMDD]-smoke.md` (independent invocation)
- `dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — Combat regression baseline must hold (current: 189/189)
- T1 negative-scope scan over changed files (M2 controller, new runner) — zero matches expected
- `git diff --check` — clean
- `.githooks/pre-commit` — `[pre-commit] OK`
- `dotnet format --verify-no-changes` — PASS

**Evidence status**: Mechanical evidence complete; human-play evidence recorded as NOT PASSED. The M2 HUD suppression is recorded as a scoped S3-06 presentation-readiness bug fix, not as grounds to pass AC-08.

**Human-play feel-check pattern documentation**: verification.md must include a `## Pattern Notes` section documenting the new evidence shape (binary pillar-anchored verdict + structural template per AC-09 + R-P2-FEEL-MISATTRIBUTION classification per AC-10 + N=1 acknowledgement + optional second-playtester both-reads) for future Tier-1 human-play stories. This is the pattern-establishing precedent flagged in the header.

**M2 preservation rerun execution note** (per `m2_melee_rng_not_reset` carryover, **now resolving via AC-04**): pre-AC-04, three M2 preservation smokes cannot be chained in a single Unity batchmode invocation. Post-AC-04 (this story), the chained-session test (AC-06) proves the reset works. The independent-invocation reruns here are the baseline regression evidence for AC-04's additive-reset claim, not a chaining-avoidance workaround.

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| `S3-02` | NPC adapter dispatch must work for the end-to-end runner to assert NPC telemetry | Done |
| `S3-03` | Relic adapter + state-routing must work for relic recovery + objective loot resolution | Done |
| `S3-04` | Vendor adapter must work for salvage sale + currency credit | Done |
| `S3-05` | Navigable greybox district must exist for walked traversal between anchors; AC-12 closure semantics roll into this story | Done |

**Sprint-level pre-condition (tracked):** `dotnet format` setup — same as S3-01..S3-05.

## Blockers

All of S3-02, S3-03, S3-04, S3-05 must close before this story enters `/dev-story`. No design blockers; all governing D-entries Locked. The pattern-establishing nature of the human-play protocol means the shape of the verification artifacts is itself part of the deliverable.

Watch items (not blockers):
- `m2_melee_rng_not_reset` — **RESOLVED by AC-04** at S3-06 close (carryover should be retired in `sprint-status.yaml`)
- `m3_04_low_review_notes` item 1 (exception-guarding) — **PARTIALLY RESOLVED by AC-05** (new runner only; broader backport remains open)
- `s2_bridge_runner_evidence_path_hardcoded`, `launch_runner_evidence_path_hardcoded`, `m2_02_runner_date_hardcoded`, `m3_03_low_review_notes` — explicitly NOT in scope per `sprint-3.md:112`; remain open as runner-hygiene carryovers for a future story
- `s3_05_ac12_partial_rollforward_to_s3_06` (if S3-05 closed "Done with Notes" on AC-12) — **RESOLVED by AC-03** at S3-06 close
- `m2_controller_scenario_smoke_abstraction` — explicitly NOT in scope; only the targeted reset hooks are touched
- `control_manifest_absence_pre_existing` — Manifest Version `Unavailable` per fallback
- Format Gate — see Dependencies
- **Solo-N=1 selection bias** — explicitly named in AC-09 evidence per `sprint-3.md:142`; not a flaw to fix, a constraint to honor
- **M2 HUD suppression bug fix** — kept and recorded as a scoped S3-06 presentation-readiness bug fix after the human-play attempt exposed legacy M2 combat debug HUD bleed over the objective loop. This fixes a contributor to the bad read, but the AC-08 feel verdict remains NOT PASSED per the recorded playtest and D020.
- **Pattern-establishing closure follow-up**: at Sprint 3 close-out, promote the T1 human-play feel-check evidence pattern to `tasks/lessons.md`. Bundle with (a) the S3-05 greybox spatial-validation pattern, (b) the S3-05 NavMesh agent profile as canonical T1 reference, and (c) the existing `feedback_external_review_verification` promotion — single Sprint 3 lessons batch.
- **FAIL verdict downstream impact**: if AC-08 returns FAIL, the next step is NOT in-story polish iteration. It is either (a) a Sprint 3+ polish story scoped to the specific loop-mechanical deficit named in the verbatim feedback, or (b) a milestone re-plan if the failure is structural. Polish-during-this-story would conflate slate completion with loop satisfaction; both decisions are post-Sprint-3 per D016.
