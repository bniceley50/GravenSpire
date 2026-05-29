# Active Session State

**Last Updated:** 2026-05-29
**Current Stage Note:** S3-05/S3-05.1 slate-row reconciliation APPLIED and pushed to origin/main at `265615b` (S3-05 completed 2026-05-26; completed_stories→2; S3-05.1 NavMesh correction PR #5/7cade4b recorded as a parent-row note). S3-02 cleared `/story-readiness` = READY (2026-05-29). Main checkout = origin/main at `265615b`. Next: `/dev-story` for S3-02 in the Codex lane (`N:/GravenSpire-codex`).
**Project Stage:** Pre-Production — Sprint 3 (Playable Vertical-Slice Assembly)

## Session Extract — main-lane reconciliation + S3-02 readiness 2026-05-29

- `/story-readiness production/stories/s3-02-player-driven-npc-interaction.md` = **READY** (lean). S3-01 dependency Complete; D001/D003/D012/D016 Locked, D004 Provisional but T1 boundary respected as telemetry-data-only. Warnings non-blocking (D004 provisional, control-manifest-absent fallback, scene-discipline reminder).
- S3-05/S3-05.1 reconciliation APPLIED (resolves the S3-EVIDENCE-01 extract's open follow-up #1): S3-05 `completed`→`2026-05-26` (sourced from carryover `s3_05_ac12_partial_rollforward_to_s3_06`); `completed_stories`→2; S3-05.1 (PR #5 / `7cade4b`) recorded as a note on the S3-05 row, not a slate story. `implementation_gate`/`progress`/`next_active_command` refreshed to `/dev-story` S3-02.
- Bookkeeping commit `265615b` (sprint-status.yaml only) pushed (a2dfee6..265615b); `main` = `origin/main`, no drift.
- Pre-existing main-checkout WIP still preserved untouched (6 modified tracked files: art-director.md, two M3 session files, three combat/M3 tests).
- Next recommended: `/dev-story production/stories/s3-02-player-driven-npc-interaction.md` (Codex lane).

## Session Extract — main-lane merge + bookkeeping 2026-05-29 (S3-EVIDENCE-01)

- PR #6 (`codex/s3-evidence-integrity-patch`) merged to `main` at `cfbfc02`; main checkout fast-forwarded from `05b25a3` (clean ancestor).
- Story `production/stories/s3-evidence-integrity-patch.md` closed COMPLETE WITH NOTES (4/4 ACs; both AC-01 and AC-03 negative controls FAIL as designed). Evidence: `tests/evidence/S3-EVIDENCE-01/verification.md` + 8 companion artifacts.
- Phases committed: AC-1 `5f8f8b6`, AC-2 `11f898c`, AC-3/AC-4 `94229fc`, story-close `1dd23dc`.
- Code review: PASS_WITH_NOTES (1 Medium cross-runner duplication; 2 Low). `/code-review` + `/story-done` run in the Codex worktree; merge + bookkeeping in the main-checkout lane per D006.
- Code Style Gate: BLOCKED by Unity-Editor-only tooling scope (no tracked csproj covers `Assets/Editor/`); compensating evidence = clean Unity 6.3 batchmode compilation. Documented in verification.md. No formal waiver written (user accepted documented rationale).
- Raw Unity `.log` files were intentionally excluded from the commit (local/licensing identifiers); a stale untracked pre-completion copy of the story file in the main checkout was removed (strict subset of the merged version, zero unique content) to allow the fast-forward.
- Pre-existing main-checkout WIP preserved untouched (6 modified tracked files: art-director.md, two M3 session files, three combat/M3 tests).
- bookkeeping applied to sprint-status.yaml: `updated`→2026-05-29, `head`→cfbfc02, progress rewritten, S3-EVIDENCE-01 added as an out-of-band patch row (slate counts unchanged).
- Bookkeeping commit `1f3e115` (sprint-status.yaml only) pushed to `origin/main`.
- Open follow-ups: (1) S3-05/S3-05.1 slate-row reconciliation in sprint-status.yaml (likely S3-05 completed date 2026-05-26 per sprint-status.yaml carryover, pending confirmation); (2) runner-hygiene tech-debt (shared CLI resolver / precondition emit across the verification runners).
- Next recommended: `/story-readiness production/stories/s3-02-player-driven-npc-interaction.md`.

## Session Extract - /story-done 2026-05-25 (S3-01)

- Verdict: COMPLETE WITH NOTES (9/9 AC passing).
- Story: `production/stories/s3-01-standalone-player-interaction-harness.md` — S3-01 Standalone Player Interaction Harness.
- Merge/evidence chain: implementation `fecd121`, runner fix `45459cb`, Phase 2 evidence `d7cde93`, PR #3 merge `9072bcb`, doc/state follow-up base `325d47c`.
- Evidence: `tests/evidence/S3-01/verification.md:8` records PASS; `tests/evidence/S3-01/verification.md:14-22` maps all 9 ACs to PASS; `tests/evidence/S3-01/unity-player-interaction-harness-20260524-smoke.md:7` records the S3 runner PASS; `tests/evidence/S3-01/unity-player-interaction-harness-20260524-smoke.md:11-44` lists the passing story checks.
- M2 preservation: `tests/evidence/S3-01/verification.md:28-30` records M2 single-trash, linked-trash, and named-blocker preservation smokes as PASS, each run as a separate Unity invocation per `m2_melee_rng_not_reset`.
- Closure gate rerun: `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` PASS 189/189 on 2026-05-25.
- Scope: no `Assets/Scripts/M2SingleTrashMedLoopController.cs` path in `git diff --name-only df5fdec..9072bcb -- Assets/Scripts/M2SingleTrashMedLoopController.cs`; negative-scope scan PASS WITH CLASSIFIED HITS (story out-of-scope/GDD references, `ClericShellMarker` references, and runner deny-list term only).
- Code review: PR #3 review/body verification completed before merge; `/story-done` lead-programmer gate skipped under lean mode.
- Tech debt logged: none in `production/sprint-status.yaml` this turn. Known follow-ups remain C2 (replace IMGUI prompt/feedback before S3-06 feel validation) and C3 (turn auto-discovery into S3-02 executable assertion if separately approved).
- Routing: `production/sprint-status.yaml` now marks S3-01 done, S3-02 ready, and S3-05 ready. S3-02 remains the next active story in dependency order.
- Next recommended: `/story-readiness production/stories/s3-02-player-driven-npc-interaction.md`.

## Session Extract - /story-done 2026-05-25 (S3-01 blocked before PR #3 merge)

- Attempted `/story-done production/stories/s3-01-standalone-player-interaction-harness.md` in lean mode (default; `production/review-mode.txt` absent).
- Verdict at the time: BLOCKED for main-branch closure. PR #3 was OPEN, `draft=false`, head `d7cde93c1860ff3230be1d77d9cb49566e89b20d`, and `mergeCommit=null` by `gh pr view 3 --json state,isDraft,headRefOid,mergeCommit`; `origin/main` remained at `df5fdec` by `git log origin/main`, while `d7cde93` was present only on `origin/codex/s3-01-standalone-player-interaction-harness` by `git branch -r --contains d7cde93`.
- Branch evidence: S3-01 verification on the PR branch reports `PASS` at `tests/evidence/S3-01/verification.md:8` (read via `git show origin/codex/s3-01-standalone-player-interaction-harness:tests/evidence/S3-01/verification.md`); all 9 AC rows are PASS at `tests/evidence/S3-01/verification.md:14-22`; M2 preservation rows are PASS at `tests/evidence/S3-01/verification.md:28-30`; the story smoke result is PASS at `tests/evidence/S3-01/unity-player-interaction-harness-20260524-smoke.md:7`, with checks listed at `:11-44`.
- No closure writes were made to story status or `production/sprint-status.yaml`, because main did not contain the S3-01 implementation/evidence files yet.
- Update: PR #3 later merged to `origin/main` as `9072bcb`, so the next run can close S3-01 on updated `main`.

## Session Extract — /dev-story 2026-05-24 (S3-01 Phase 1)

- Story: `production/stories/s3-01-standalone-player-interaction-harness.md` — S3-01 Standalone Player Interaction Harness.
- Worktree / branch: `N:\GravenSpire-codex` on `codex/s3-01-standalone-player-interaction-harness`, created from `origin/main` at `df5fdec`.
- Files changed:
  - `Assets/Scripts/S3PlayerInteractionHarness.cs` + `.meta` — standalone distance-check interaction harness, `IPlayerInteractTarget`, `InteractContext`, single `KeyCode.E` verb, range-gated prompt state, and fired/missed/blocked telemetry/feedback acknowledgements.
  - `Assets/Editor/GravenspireS3PlayerInteractionHarnessBuilder.cs` + `.meta` — Unity builder that wires a distinct `S3_PlayerInteractionHarnessRoot` to `ClericShellMarker` without editing `M2SingleTrashMedLoopController`.
  - `Assets/Editor/GravenspireS3PlayerInteractionHarnessVerificationRunner.cs` + `.meta` — story-specific Unity runner for S3-01 T1-T6 using a mock registered target and `DateTimeOffset.UtcNow` / `EditorApplication.timeSinceStartup`.
- Verification run in Phase 1:
  - `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — PASS 189/189.
  - `dotnet format tests\Gravenspire.Combat.Tests.csproj --verify-no-changes --exclude-diagnostics IDE1006` — PASS.
  - `dotnet format prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj --verify-no-changes --exclude-diagnostics IDE1006` — PASS.
  - Explicit Phase 1 source scan over the new S3 files — PASS for no `DateTime.UtcNow`, no Unity Input System package, no `CharacterController`, no Tier-2/network/LLM terms, and no direct M3 `Try*` calls from the harness.
  - Explicit trailing-whitespace scan over the new S3 files — PASS.
- Scope notes:
  - No `Assets/Scripts/M2SingleTrashMedLoopController.cs` change.
  - No existing M3 runtime file change.
  - No hand-authored `_DevEntry.unity` scene change and no hand-authored S3-01 evidence artifact.
- Blockers / next:
  - Phase 2 remains mandatory before S3-01 can close: run Unity builder to generate/save the `_DevEntry.unity` scene delta, run `GravenspireS3PlayerInteractionHarnessVerificationRunner`, run the three M2 preservation runners in separate Unity invocations with `tests/evidence/S3-01/` output paths, then create `tests/evidence/S3-01/verification.md`.
  - After Phase 2 evidence: `/code-review` with Sprint 3 review-subagent layer, then `/story-done production/stories/s3-01-standalone-player-interaction-harness.md`.

## Current Task

Current as of 2026-05-25: S3-01 is closed COMPLETE WITH NOTES. S3-02 and S3-05 are unblocked; S3-02 is the next active story in dependency order. The paragraphs below are retained as older session context and no longer describe the live gate state.

Sprint 3 ("Playable Vertical-Slice Assembly") is open per `DECISIONS.md` D016, and the slate is fully opened on 2026-05-23. Six-commit arc on `origin/main`: `d7acfbf` (close Sprint 2) → `62c0f66` (Sprint 3 plan + active.md refresh) → `9bf60e1` (six story files as slate commit) → `ecbea5e` (Story Ledger back-fill linking ledger to slate) → `4ea9e2d` (sprint-status.yaml regen) → `90821f2` (format-gate PR #2 merge 2026-05-24, three commits: policy `f040493` + baseline `a0785d8` + hook `cf1c204`). Six story files at `production/stories/s3-01..s3-06-*.md` on disk, ledger-linked, status synchronized; sprint-status.yaml carryover stays at 21 live (`gitattributes_crlf_normalization_unconfigured` retired with format-gate merge, `ide1006_naming_debt_excluded_from_format_gate` added in its place), 8 retired with visible-trace comment block, 3 RESOLUTION CONDITIONs threading carryover→AC retirement causally.

Sprint 3 preconditions are resolved: format-gate PR #2 merged at `90821f2`, owner assignment and Sprint 3 `/qa-plan` landed at `df5fdec`, and S3-01 merged through PR #3 at `9072bcb`. The current routing gate is S3-02 story readiness.

## Status

- ✓ Brainstorm complete → [design/gdd/game-concept.md](../../design/gdd/game-concept.md); Engine configured → Unity 6.3 LTS + C# + URP (D001).
- ✓ Art bible complete → [design/art/art-bible.md](../../design/art/art-bible.md); Systems mapped → [design/gdd/systems-index.md](../../design/gdd/systems-index.md) (33 systems, 26 MVP).
- ✓ Core GDDs authored — Combat Core, Character Progression, World Structure, NPC System; Inventory & Item Economy parked behind the `INV-OQ-05` pre-spec.
- ✓ Sprint 1 + Sprint 1.5 closed — Combat Core domain built; T1 combat-feel validated (D012); Endurance resource model (D013 / ADR-0006); FEEL-01 revalidated (D014).
- ✓ Sprint 2 CLOSED 2026-05-20 (D016) — M1-M3 delivered as runner-proven systems, 11/11 stories complete. The Tier-1 combat loop and the objective/NPC/loot systems exist and are verified by automated Unity runners.
- ✓ D016 locked — Sprint 2→3 re-sequence: the M3 systems were runner-proven but never assembled into a human-playable loop; Sprint 3 is the playability assembly pass.
- ✓ Sprint 3 plan authored + approved + committed → [production/sprints/sprint-3.md](../sprints/sprint-3.md) at `62c0f66`. Single milestone; 6-story slate `S3-01`–`S3-06`, dependency-ordered.
- ✓ Sprint 3 slate opened 2026-05-23 — six story files at `production/stories/s3-NN-*.md` committed as slate commit `9bf60e1` (one logical slate-opening event per §14 coordinated-cross-doc exception). Story Ledger in `sprint-3.md` back-filled at `ecbea5e` with markdown links to each story file; intro sentence updated past-tense to reflect the slate-opened state with slate-commit SHA anchor.
- ✓ Sprint 3 `sprint-status.yaml` regenerated at `4ea9e2d` — schema preserved verbatim from Sprint 2; 21 live carryovers, 7 retired with visible-trace YAML comment block at top of `carryover:`, 3 new entries (`feedback_external_review_verification`, `sprint_3_pattern_promotion_bundle`, `gitattributes_crlf_normalization_unconfigured`), 3 RESOLUTION CONDITIONs appended (`s2_m3_04_ac06_transfer` retires on S3-06 AC-08 PASS, `m2_melee_rng_not_reset` on S3-06 AC-04 PASS, `m3_04_low_review_notes` item 1 partial-retires on S3-06 AC-05 PASS). Schema-managed-by-skill convention paused for the regen, not violated; future updates go through `/story-done`.
- ✓ Format-gate pre-condition resolved 2026-05-24 by PR #2 merge commit `90821f2` (policy `f040493`, baseline `a0785d8`, hook `cf1c204`). `.githooks/pre-commit` runs `dotnet format --verify-no-changes --exclude-diagnostics IDE1006` on the test + harness csproj; pre-existing IDE1006 naming debt excluded via the new `ide1006_naming_debt_excluded_from_format_gate` carryover (NOT Sprint 3 scope).
- ✓ S3-01 closed COMPLETE WITH NOTES on 2026-05-25 — standalone player interaction harness merged through PR #3 (`9072bcb`), story/evidence/routing closure updated by `/story-done`; S3-02 and S3-05 unblocked.
- Pending (Sprint 3 close-out): six-item lessons promotion to `tasks/lessons.md` per `sprint_3_pattern_promotion_bundle` carryover — (1) S3-05 greybox spatial-validation evidence pattern; (2) S3-05 NavMesh agent profile as canonical T1 reference; (3) S3-06 T1 human-play feel-check evidence pattern; (4) `feedback_external_review_verification` (occurrence #2 logged); (5) skill-vs-project-convention deviation (the `/create-stories` §0.4 "stop, run /create-epics first" trip-wire); (6) D006 implementer-worktree-boundary discipline. Plus uncommitted-meta-rationale chain from this session: hybrid-skill/project-convention deliberation, two-adapter-per-GameObject S3-03 reasoning, binary-pillar-anchored S3-06 verdict choice — captured implicitly in story Out-of-Scope + Implementation Notes sections but not in source-of-truth lesson form.

## Files Being Worked On

- **Active:** Sprint 3 implementation. S3-01 is closed; S3-02 is next active story and S3-05 is also ready.
- Sprint 3 plan + ledger: [production/sprints/sprint-3.md](../sprints/sprint-3.md) (ledger IDs link to story files)
- Sprint 3 stories (six, all `production/stories/s3-NN-*.md`): [s3-01](../stories/s3-01-standalone-player-interaction-harness.md) `done`; [s3-02](../stories/s3-02-player-driven-npc-interaction.md) and [s3-05](../stories/s3-05-navigable-greybox-first-district.md) `ready`; [s3-03](../stories/s3-03-player-relic-recovery-and-looting.md), [s3-04](../stories/s3-04-player-driven-vendor.md), and [s3-06](../stories/s3-06-playable-end-to-end-and-human-play.md) remain `blocked` per dependency chain.
- Sprint 3 status: [production/sprint-status.yaml](../sprint-status.yaml) — 6 stories, 1 complete; `head: 325d47c`; carryover-triaged (21 live, 8 retired with visible-trace block; includes `ide1006_naming_debt_excluded_from_format_gate` post-merge).
- M3 quick-design (Sprint 3's design source for the systems being wired): [design/quick/quick-design-m3-objective-npc-loot.md](../../design/quick/quick-design-m3-objective-npc-loot.md)
- M3/M2 systems Sprint 3 wires (reuse, not rebuild; zero-diff invariants on the M3 files per S3-02/03/04 ACs): `Assets/Scripts/M3NamedNpcObjectiveFrame.cs`, `Assets/Scripts/M3ObjectiveStateRelicHandIn.cs`, `Assets/Scripts/M3LootTableFixedProfileVendor.cs`, `src/gameplay/npc/m3-objective/**`, `Assets/Scenes/_DevEntry.unity` (M3 anchors), `Assets/Scripts/M2SingleTrashMedLoopController.cs` (player locomotion + combat loop; bounded modification at `:76-79` reset hooks per S3-06 AC-04).
- Game concept / pillars: [design/gdd/game-concept.md](../../design/gdd/game-concept.md); DECISIONS.md (D001–D016).

## Key Decisions Made

- **D016** (2026-05-20): Sprint 2→3 re-sequence. Sprint 2 closes with M1-M3 as runner-proven systems; Sprint 3 = "Playable Vertical-Slice Assembly" — wire the M3/M2 systems behind player input in a navigable greybox First District. No new systems; reuse the M3 systems. Greybox, not produced art. M4/M5 defer behind Sprint 3.
- Sprint 3 USER DECISIONS (2026-05-21): the `S3-01` player interaction harness is a NEW standalone component (not bolted onto the 2156-line M2 controller); the slate is 6 stories — no 7th runner-hygiene story (runner-hygiene debt carries as tracked carryovers).
- TD feasibility consult: D016's open question answered YES — the M3 session-state APIs are drivable from player input without rework; player locomotion already exists at `M2SingleTrashMedLoopController.cs:564-583`.
- CD pillar consult: the Sprint 3 feedback rule — feedback acknowledges player action, never advertises/locates/routes (Pillar 2, *The Silence Is Sacred*).
- Engine Unity 6.3 LTS + C# + URP (D001); Tier 1 single-player offline (D003); Codex parallel implementer (D006); Qwen3-Coder scoped local implementer (D015).
- **Slate-opening decisions (2026-05-23)**:
  - **Skill/project-convention hybrid** — the `/create-stories` skill prescribes `production/epics/[slug]/EPIC.md` + `story-NNN-[slug].md` layout and instructs "stop, run /create-epics first" at §0.4 if no EPIC.md exists; this project has never had `production/epics/` and uses `production/stories/[id]-[slug].md` flat with sprint-plan Story Ledger as the epic-substitute. The Sprint 3 slate followed the hybrid (skill's content discipline + project's path/ID convention). Tracked in `sprint_3_pattern_promotion_bundle` item 5 for lessons-promotion at close-out so the next `/create-stories` invocation doesn't re-trip on §0.4.
  - **S3-03 two-adapter design** — one adapter per GameObject (NPC adapter on `M3_Caretaker` state-routes across four objective states for Accept / re-talk / Hand-In; relic adapter on `M3_ObjectiveRelic` atomically calls `TryRecoverRelic` + `TryResolveObjectiveLoot`), not one-adapter-dispatching-to-both-subsystems. M3 contract shapes confirmed via source reads of `M3ObjectiveStateRelicHandIn.cs:54/82/110` + `M3LootTableFixedProfileVendor.cs:89/101/119`. Plan row at `sprint-3.md:68` omitted `TryReturnRelicToNpc` (hand-in); the state machine's geometry made the scope unambiguous and hand-in is in-scope for S3-03 with the row-text-gap noted inline.
  - **S3-06 binary pillar-anchored human-play verdict** — pass = playtester voluntarily re-engages AND names objective/NPC/relic as the reason; fail = re-engagement only for mechanical reward or no re-engagement. Inherits structural template from `tests/evidence/S2-M3-04/human-play-20260520.md` (the originating deferral artifact). Structured per-question scorecard explicitly rejected per `sprint-3.md:142`; binary attribution test with N=1 self-test methodological-limit acknowledgment is the committed shape.
  - **Format-gate routing (2026-05-24)** — handled by Codex per D006 in `N:\GravenSpire-codex` on `codex/sprint-3-format-gate-policy`; merged via PR #2 at `90821f2`. Three commits: policy (`f040493`), one-file format baseline (`a0785d8`), hook wiring (`cf1c204`). Hook excludes IDE1006 because naming debt predates the branch (verified at baseline `f040493` — IDE1006 already failed on origin/main, not introduced by this batch). Tracked as separate `ide1006_naming_debt_excluded_from_format_gate` carryover, NOT Sprint 3 implementation scope.
- §14 commit cadence: slate opening produced three commits in sequence (slate commit `9bf60e1`, back-fill commit `ecbea5e`, status regen commit `4ea9e2d`) — each prompting for commit per §14 default cadence; slate commit consolidated the six story-file writes under the coordinated-cross-doc exception (one logical slate-opening event). §14 drift was surfaced mid-session (six story-file writes had accumulated without per-file commit prompts) and resolved by the slate-as-one-commit pattern, with the durable slate-commit SHA serving as the anchor for the back-fill commit's updated ledger intro sentence.

## Next Skill to Run

No single skill — two pre-conditions parallelizable in any order before `/story-readiness production/stories/s3-01-standalone-player-interaction-harness.md`:

1. **Owner assignment per story** (cheapest, one-pass decision): assign one owner per `S3-01..S3-06` row in `production/sprint-status.yaml` per D016 pre-commit gate. Design-aware required for S3-01/S3-05/S3-06; wiring-OK for S3-02/03/04; Qwen3-Coder NOT eligible per D015.
2. **Sprint 3 `/qa-plan`** (most substantive): ratify the human-play-AC-on-S3-06-only rule + M2 preservation discipline + the pattern-establishing story shapes for S3-05 (spatial validation) and S3-06 (human-play feel check) as enforceable QA gates rather than plan-narrative.
(Format-gate pre-condition resolved 2026-05-24 by PR #2 / `90821f2`; no further setup action needed before the first S3-01 implementation PR.)

After the two pre-conditions land: `/story-readiness production/stories/s3-01-standalone-player-interaction-harness.md` → `/dev-story s3-01` → six-story implementation chain.

## Session Extract — Sprint 3 Slate Open 2026-05-23

- Five-commit arc on `origin/main`:
  - `d7acfbf` Record D016 Sprint 2-to-3 re-sequence; close Sprint 2 (2026-05-20)
  - `62c0f66` Open Sprint 3 playable vertical-slice assembly plan (2026-05-21 plan + active.md refresh)
  - `9bf60e1` Open Sprint 3 story slate: S3-01 through S3-06 (six story files as one logical slate-opening event per §14 coordinated-cross-doc exception)
  - `ecbea5e` Wire Sprint 3 Story Ledger to slate story files (back-fill linking ledger to slate; minimal-disturbance — turned each `S3-NN` ID cell into markdown link, updated stale "are a proposal" sentence to past tense with slate-commit SHA anchor)
  - `4ea9e2d` Regenerate sprint-status.yaml for Sprint 3 slate-open (full-file replacement; schema preserved from Sprint 2; 21 live carryovers + 7 retired-with-trace + 3 new + 3 RESOLUTION CONDITIONs)
- Six story files written, each via per-file EDIT_OK gate, all self-contained per `/create-stories` skill §5:
  - [s3-01](../stories/s3-01-standalone-player-interaction-harness.md) Standalone player interaction harness (1.5d MEDIUM; foundation; greenfield composition layer)
  - [s3-02](../stories/s3-02-player-driven-npc-interaction.md) Player-driven NPC interaction (1.0d HIGH; thin wiring; M3 frame zero-diff invariant)
  - [s3-03](../stories/s3-03-player-relic-recovery-and-looting.md) Player relic recovery + looting — Accept, Recover, Hand-In, Loot (1.0d MEDIUM; densest wiring; two-adapter design; three terminal telemetry shapes including partial-success)
  - [s3-04](../stories/s3-04-player-driven-vendor.md) Player-driven vendor sell-side (1.0d HIGH; thinnest wiring; buy-side absence guarded by code-scan invariant T6)
  - [s3-05](../stories/s3-05-navigable-greybox-first-district.md) Navigable greybox First District (1.5d LOW; pattern-establishing spatial-validation evidence; four reject / four allowed wayfinding criteria; AC-12 closure semantics roll forward to S3-06)
  - [s3-06](../stories/s3-06-playable-end-to-end-and-human-play.md) Playable end-to-end + human-play feel check (1.0d MEDIUM; pattern-establishing T1 human-play feel-check; binary pillar-anchored verdict; bounded M2 controller hygiene at `:76-79`)
- Carryover triage at sprint-status.yaml regen: 21 live preserved verbatim; 7 retired with visible-trace YAML comment block at top of `carryover:` (`ability_resolved_event_payload`, `qa_02_01_wording`, `unity_shell_absent`, `unity_launch_human_verification`, `m2_02_human_play_gate`, `m2_presentation_threshold_gap` per D016, `m2_runner_editor_noise_capture` per S2-M3-00); 3 new added (`feedback_external_review_verification`, `sprint_3_pattern_promotion_bundle` with five-item enumeration including skill-vs-convention §0.4 trip-wire, `gitattributes_crlf_normalization_unconfigured` with observed-on-commits citations); 3 existing appended with RESOLUTION CONDITIONs threading retirement to S3-06 ACs.
- §14 drift surfaced and corrected mid-session: six story-file writes accumulated without per-file commit prompts (drift named at the gate 1 inflection point — agent's prompting obligation per §14, second-pair-of-eyes lapse acknowledged shared); resolved via Option A — slate-commit consolidation of all six story files as one logical event per the coordinated-cross-doc exception. Slate-then-back-fill ordering produced the durable slate-commit SHA anchor used in the back-fill commit's updated ledger intro sentence.
- Three pre-conditions surface as the next gate (parallelizable; format-setup is the only hard PR-blocker — Style Gate; owner-assignment and `/qa-plan` are governance gates). See Current Task and Next Skill to Run for routing options.

## Session Extract — Sprint 2 Story Open 2026-05-09 (S2-FOUNDATION-01)

- Story: [production/stories/s2-foundation-01-unity-project-shell.md](../stories/s2-foundation-01-unity-project-shell.md) - S2-FOUNDATION-01 Unity Project Shell.
- Status at creation: BLOCKED - Sprint 2 `/qa-plan sprint` not yet run.
- Source trace: `production/sprints/sprint-2.md:45` anchors M1 Player In World; `production/sprints/sprint-2.md:94-98` anchors the minimum shell shape.
- Routing: `production/sprint-status.yaml` now carries 2 total stories, 1 complete, and `S2-FOUNDATION-01` blocked.
- Superseded by the Sprint 2 M2 story-slate extract below; current next gate is `/story-readiness production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md`.

## Session Extract — Sprint 1.5 Close-Out Chain 2026-05-09

- Smoke: `production/qa/smoke-sprint-20260509.md` records **PASS WITH WARNINGS**; accepted warning is the missing production Unity shell.
- QA sign-off: `production/qa/qa-signoff-sprint-1-5-20260509.md` records **APPROVED WITH CONDITIONS**; no Sprint 1.5 bugs found.
- Gate-check: `production/gate-checks/gate-check-2026-05-09-sprint-1-5-closeout.md` records **PASS** and unblocks Sprint 2 rollover.
- Routing: `production/sprint-status.yaml` intentionally held Sprint 1.5 at `caea662` until gate-check landed; current live head is `aa785a0`.
- Carried forward: human death-moment playtest, QA-02-01 wording, `AbilityResolvedEvent.ManaSpent`-only semantics, evidence provenance conventions, Save/Load metadata drift, README template-facing drift, and game-concept engine wording drift.
- Next recommended: open `S2-FOUNDATION-01`, then run Sprint 2 `/qa-plan sprint`; do not implement new Sprint 2 feature work before that.

## Session Extract — /story-done 2026-05-09 (T1.5-COMBAT-05)

- Story: [production/stories/t1-5-combat-05-profiled-rerun-evidence-summary.md](../stories/t1-5-combat-05-profiled-rerun-evidence-summary.md) - T1.5-COMBAT-05 Profiled Rerun + Slice Evidence Summary.
- Verdict: COMPLETE.
- Criteria: 6/6 passing; `QA-05-01` through `QA-05-06` all have file:line evidence in the story completion notes and verification summary.
- Evidence: [tests/evidence/T1.5-COMBAT-05/verification.md](../../tests/evidence/T1.5-COMBAT-05/verification.md) records the harness rerun, fixture update, regression pass, hygiene gates, and provenance chain. [tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl](../../tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl) records all five required scenarios.
- Implementation commits: `960a148` is the implementation and metric-capture commit. `caea662` is the provenance-fix follow-up and current `origin/main` head.
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1.5-COMBAT-05` done, records 7/7 Sprint 1.5 stories done, and updates `head` to `caea662`.
- Tech debt logged: None.
- Carried forward: human qualitative death-moment playtest pending for the slice review; `AbilityResolvedEvent.ManaSpent`-only payload semantics; QA-02-01 cooldown/global-recovery wording.
- Next recommended: `/smoke-check sprint`.

## Session Extract — /story-done 2026-05-09 (S2-COMBAT-01)

- Story: [production/stories/s2-combat-01-fix-init-only-property-preservation.md](../stories/s2-combat-01-fix-init-only-property-preservation.md) - S2-COMBAT-01 Fix Init-Only Property Preservation in CombatActorState Transitions.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 5/5 passing; `S2-01-01` through `S2-01-05` all covered by unit tests and verification evidence.
- Evidence: [tests/evidence/S2-COMBAT-01/verification.md](../../tests/evidence/S2-COMBAT-01/verification.md) records implementation provenance, local gates, T1 negative-scope scan, and deferred Gemini-review items.
- Implementation commit: `5b8a017` fixes the three manual transition-copy paths and adds four regression tests.
- Code review: `/code-review 5b8a017` returned APPROVED WITH SUGGESTIONS from code and QA agents; no blocking findings.
- State updates: story status set to Complete; `production/sprint-status.yaml` intentionally unchanged because Sprint 1.5 is closed and no Sprint 2 status file exists yet.
- Tech debt logged: None.
- Carried forward: include `S2-COMBAT-01` retrospectively as story #1 when Sprint 2 plan/status scaffolding is drafted; optional future test polish can make the init-only preservation assertion reflection-driven from the allowlist.
- Next recommended: draft Sprint 2 plan/status scaffold, or run Sprint 1.5 close-out gates if Brian chooses that sequencing.

## Session Extract — Sprint 2 QA Plan 2026-05-09

- QA plan: [production/qa/plans/qa-plan-sprint-2-20260509.md](../qa/plans/qa-plan-sprint-2-20260509.md).
- Scope: Sprint 2 First District foundation routing, covering completed `S2-COMBAT-01` as regression-only and `S2-FOUNDATION-01` as the next implementation story.
- Classification: `S2-COMBAT-01` is Hotfix / Logic and remains closed; `S2-FOUNDATION-01` is Foundation / Integration and is now closed. This extract is superseded by completed launch verification and the M2 story-slate opening.
- Required evidence for next story: `tests/evidence/S2-FOUNDATION-01/verification.md`.
- Superseded next gate: launch verification is complete; current next command is `/story-readiness production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md`.

## Session Extract — /dev-story 2026-05-09 (S2-FOUNDATION-01)

- Story: [production/stories/s2-foundation-01-unity-project-shell.md](../stories/s2-foundation-01-unity-project-shell.md) - S2-FOUNDATION-01 Unity Project Shell.
- Files changed: Unity shell under `Assets/**`, `ProjectSettings/**`, `Packages/**`; production fixture data moved from `assets/data/**` to `data/**`; test bridge/docs/evidence updated.
- Dev entry: `Assets/Scenes/_DevEntry.unity`.
- Test written: None - Foundation / Integration shell story; Unity batchmode builder and EditMode smoke were run instead.
- Verification: [tests/evidence/S2-FOUNDATION-01/verification.md](../../tests/evidence/S2-FOUNDATION-01/verification.md) records source trace, footprint, Unity smoke, combat regression, negative-scope scan, and hygiene gates.
- Gate results: `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` passed 164/164; Unity batchmode shell generation returned code 0; Unity EditMode smoke returned code 0 but produced no results XML because no Unity Test Runner assemblies exist yet; `git diff --check` and `.githooks/pre-commit` passed.
- Blockers: None for implementation. Next: `/code-review` on changed files, then `/story-done production/stories/s2-foundation-01-unity-project-shell.md`.

## Session Extract — /story-done 2026-05-09 (S2-FOUNDATION-01)

- Story: [production/stories/s2-foundation-01-unity-project-shell.md](../stories/s2-foundation-01-unity-project-shell.md) - S2-FOUNDATION-01 Unity Project Shell.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 6/6 passing; `S2-FND-01` through `S2-FND-06` are covered by verification evidence.
- Evidence: [tests/evidence/S2-FOUNDATION-01/verification.md](../../tests/evidence/S2-FOUNDATION-01/verification.md) records source trace, Unity config, dev-entry scene, Unity batchmode smoke, combat regression, negative-scope scan, hygiene gates, and code-review notes.
- Implementation commit: `f5f74dc` creates the production Unity shell.
- State updates: story status set to Complete; `production/sprint-status.yaml` recorded 2/2 Sprint 2 tracked stories closed, updated `head` to `f5f74dc`, and left `next_active_command` empty pending launch verification. This is superseded by the M2 story-slate routing below.
- Watch items: build-settings GUID parity, Unity Test Runner results XML absence, and test-data bridge/scanner alignment.
- Superseded by Unity launch verification and M2 story-slate opening; current next recommended command is `/story-readiness production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md`.

## Session Extract — Unity Launch Verification 2026-05-10 (S2-FOUNDATION-01)

- Verification methods: manual Play Mode check plus Unity CLI runner.
- Manual result: `_DevEntry.unity` loaded in Unity `6000.3.14f1`; Game view rendered; floor, Cleric marker, and `FirstDistrict_ShellOnly_NoGameplay` were visible; Console had no red errors; Play Mode stayed stable for 30 seconds.
- CLI result: [tests/evidence/S2-FOUNDATION-01/unity-cli-launch-verification-20260510.md](../../tests/evidence/S2-FOUNDATION-01/unity-cli-launch-verification-20260510.md) records PASS for scene load, required objects, nonblank camera render, 30-second Play Mode stability, and no captured errors or warnings.
- Tooling added: [Assets/Editor/GravenspireLaunchVerificationRunner.cs](../../Assets/Editor/GravenspireLaunchVerificationRunner.cs) provides reusable Editor-only Unity launch verification for later Sprint 2 Unity smoke gates.
- Hardening note: the runner persists state through Unity Play Mode editor-domain reload using `SessionState` plus `[InitializeOnLoad]`; the first runner version hung because static callbacks were erased during Play Mode entry.
- ProjectSettings noise: Unity-open-and-close generated settings drift was restored and not carried forward.
- Superseded by the M2 story-slate opening below. Next gate: `/story-readiness production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md`.

## Session Extract — M2 Combat Camp Loop Story Slate 2026-05-10

- Quick design: [design/quick/quick-design-m2-combat-camp-loop.md](../../design/quick/quick-design-m2-combat-camp-loop.md).
- Open stories:
  - [production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md](../stories/s2-m2-01-unity-combat-core-runtime-bridge.md) - Ready for story-readiness.
  - [production/stories/s2-m2-02-single-trash-pull-med-loop.md](../stories/s2-m2-02-single-trash-pull-med-loop.md) - Blocked on `S2-M2-01`.
  - [production/stories/s2-m2-03-linked-trash-overpull.md](../stories/s2-m2-03-linked-trash-overpull.md) - Blocked on `S2-M2-02`.
  - [production/stories/s2-m2-04-named-blocker-camp-boundary.md](../stories/s2-m2-04-named-blocker-camp-boundary.md) - Blocked on `S2-M2-03`.
- Routing: `production/sprint-status.yaml` now tracks 6 Sprint 2 stories, 2 closed, 1 ready-for-dev, and 3 blocked in M2 dependency order.
- Sprint plan: `production/sprints/sprint-2.md` Story Ledger now lists the M2 slate while preserving the quick-design citation to `production/sprints/sprint-2.md:60` through `production/sprints/sprint-2.md:78`.
- Next command: `/story-readiness production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md`.

## Session Extract — /story-done 2026-05-11 (S2-M2-01)

- Story: [production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md](../stories/s2-m2-01-unity-combat-core-runtime-bridge.md) - S2-M2-01 Unity Combat Core Runtime Bridge.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 5/5 passing; `S2-M2-01-01` through `S2-M2-01-05` are covered by committed verification evidence and live cheap-gate reruns.
- Evidence: [tests/evidence/S2-M2-01/verification.md](../../tests/evidence/S2-M2-01/verification.md) records AC pass coverage, local gates, code-review fix evidence, and watch items. [tests/evidence/S2-M2-01/unity-combat-bridge-smoke-20260510.md](../../tests/evidence/S2-M2-01/unity-combat-bridge-smoke-20260510.md) records Play Mode PASS with bridge hydration, fixture ids, player/hostile actors, active zone id, and no captured errors.
- Implementation commit: `b4cb377` implements the Unity Combat Core runtime bridge.
- Gate results: `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` passed 169/169 on 2026-05-11; S2-M2-01 negative-scope scan had only classified story/test/comment hits; `git diff --check` passed; `.githooks/pre-commit` returned `[pre-commit] OK`.
- Watch items: Unity ProjectSettings hygiene is deferred. `ProjectSettings/TagManager.asset` emits a non-fatal parse warning in S2-M2-01 Unity smoke logs; `ProjectSettings/URPProjectSettings.asset` is currently untracked after Unity generation.
- Routing: `production/sprint-status.yaml` marks `S2-M2-01` done, updates Sprint 2 progress to 3/6 closed, unblocks `S2-M2-02`, and routes next to `/story-readiness production/stories/s2-m2-02-single-trash-pull-med-loop.md`.
- Human-play gate: S2-M2-01 was a bridge/infrastructure story; CLI/runner evidence was sufficient. `S2-M2-02` and later playable-loop stories should require human play/feel evidence at closure.
- Tech debt logged: None.
- Next recommended: small Unity settings hygiene batch, then `/story-readiness production/stories/s2-m2-02-single-trash-pull-med-loop.md`.

## Session Extract — /story-done 2026-05-12 (S2-M2-02)

- Story: [production/stories/s2-m2-02-single-trash-pull-med-loop.md](../stories/s2-m2-02-single-trash-pull-med-loop.md) - S2-M2-02 Single Trash Pull + Med Loop.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 6/6 passing; `S2-M2-02-01` through `S2-M2-02-06` are covered by committed verification evidence and the post-patch Unity smoke evidence.
- Evidence: [tests/evidence/S2-M2-02/verification.md](../../tests/evidence/S2-M2-02/verification.md) records AC pass coverage, gate evidence, and review-gate provenance. [tests/evidence/S2-M2-02/human-play-20260512.md](../../tests/evidence/S2-M2-02/human-play-20260512.md) records the qualified-no answer to "did you want one more pull?" and the worst-thing presentation-threshold finding carried forward.
- Implementation commits: `93b460e` (implementation) -> `946c9d1` (harden / closes /code-review P0/P1) -> `350b06e` (evidence-integrity patch / closes qa-tester re-review).
- Code review: `/code-review` pass 1 on `946c9d1` returned STILL BLOCKED from qa-tester on evidence integrity; lead-programmer + gameplay-programmer returned APPROVED WITH NOTES on the runtime. Focused `/code-review` pass 2 on `350b06e` returned APPROVED from qa-tester; convergent WATCH item `renderer.material` on Update path retained.
- Gate results: `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` passed 170/170 on 2026-05-12; `git diff --check` clean; `.githooks/pre-commit` returned `[pre-commit] OK` on the evidence-integrity commit.
- Routing: `production/sprint-status.yaml` marks `S2-M2-02` done, updates Sprint 2 progress to 4/6 closed, unblocks `S2-M2-03`, and routes next to `/story-readiness production/stories/s2-m2-03-linked-trash-overpull.md`. M2-04 revisit trigger embedded in the `m2_presentation_threshold_gap` carryover.
- WATCH items: `m2_renderer_material_property_access` (convergent finding on renderer.material in Update path); `s2_bridge_runner_evidence_path_hardcoded` (runner internal literal cleanup); `m2_presentation_threshold_gap` (human-play presentation-threshold; affects M2-03/M2-04 routing — see active carryover).
- Tech debt logged: None new. Three carryover keys above were added to `sprint-status.yaml`. One sprint-level WATCH on integration-test private-field naming convention is noted in the story Completion Notes; not a separate carryover key.
- Next recommended: `/story-readiness production/stories/s2-m2-03-linked-trash-overpull.md`.

## Session Extract — /story-done 2026-05-13 (S2-M2-03)

- Story: `production/stories/s2-m2-03-linked-trash-overpull.md` — S2-M2-03 Linked Trash Overpull
- Verdict: COMPLETE WITH NOTES
- Criteria: 5/5 passing (zero deferred/untested)
- Evidence: bb6deab commit (10 files, 1965 insertions, 6 deletions); `tests/evidence/S2-M2-03/verification.md`; dotnet 172/172; smoke evidence with mechanical AC-03 proof
- Code review: lean mode (subagent gates skipped); pre-commit four-condition verification covered renderer-material discipline, runner hygiene, manifest absence recording, and story-status flip
- Routing forward: S2-M2-04 is the next active routing target for `/story-readiness`; its story header and sprint-status row remain blocked until that gate flips them; M2-02 presentation-threshold revisit trigger remains active for M2-04
- Tech debt logged in this batch: 0; S2-M2-03-cycle lessons captured 2026-05-14 in `fc20a86` — 4 entries (telemetry-backed feel acceptance, classified negative-scope doc hits, Unity log redaction, MaterialPropertyBlock lifecycle-safe allocation). The PowerShell heredoc whitespace-collapse lesson was dropped: real but lacking durable repo evidence; recapture if it recurs or when a shell/git workflow doc is opened.
- Sprint-status carryover added: `control_manifest_absence_pre_existing` (pre-existing project-wide governance gap, non-blocking)
- Next recommended: `/story-readiness production/stories/s2-m2-04-named-blocker-camp-boundary.md`

## Session Extract — /dev-story 2026-05-14 (S2-M2-04)

- Story: `production/stories/s2-m2-04-named-blocker-camp-boundary.md` — S2-M2-04 Named Blocker + Camp Boundary.
- Status after implementation: Implemented, pending `/code-review` and `/story-done`. Story header flipped `Blocked` → `In Progress`; `production/sprint-status.yaml` intentionally untouched (dev-story skill §7 — `/story-done` owns the sprint-status row).
- Route: implemented directly — a straightforward additive extension of the S2-M2-01/02/03 three-layer pattern (scene builder + controller + Editor runner + dotnet integration test). No implementation subagent. An Explore subagent dispatched for read-only code mapping misread its brief; it made no edits and was abandoned in favor of direct reads.
- Files changed:
  - `Assets/Editor/GravenspireM2SingleTrashLoopBuilder.cs` — modified: adds the `M2_NamedBlocker` capsule anchor at `(-2.8, 1.4, 5.6)`.
  - `Assets/Scripts/M2SingleTrashMedLoopController.cs` — modified: `NamedSoloBlock_T1` fixture handoff, named-blocker telemetry properties, `RunAutomatedNamedBlockerBoundarySmoke` + boundary-smoke helpers, `HydrateNamedBlockerPresence`, scene binding, and the violet `MaterialPropertyBlock` visual.
  - `Assets/Editor/GravenspireM2NamedBlockerVerificationRunner.cs` — created: story-specific Unity smoke runner modeled on the S2-M2-03 runner; `CaptureLog` hardened to filter the diagnosed `UnityEditor.Search.SearchInit` editor-startup noise.
  - `Assets/Editor/GravenspireM2NamedBlockerVerificationRunner.cs.meta` — created by the Unity import.
  - `Assets/Scenes/_DevEntry.unity` — modified: Unity-serialized `M2_NamedBlocker` GameObject (`:706`) + Transform (`:721`); recurring Unity `m_Name:` trailing-whitespace artifact trimmed at `:232`.
  - `tests/integration/gameplay/combat/combat_runtime_named_blocker_boundary_test.cs` — created: 3 dotnet integration tests proving the FEEL-02 boundary, named-vs-trash discovery, and no-farm-through.
  - `tests/evidence/S2-M2-04/verification.md`, `unity-named-blocker-runner-20260514-smoke.md`, `unity-named-blocker-runner-20260514.log` — created (Unity log redacted for local-machine identifiers).
- Evidence: `tests/evidence/S2-M2-04/verification.md` — Result PASS; 5/5 AC traced to file:line.
- Verification:
  - `dotnet test tests/Gravenspire.Combat.Tests.csproj` — PASS, 175/175 (172 baseline + 3 new named-blocker tests).
  - T1 negative-scope scan over the 4 changed code files — PASS, zero matches.
  - `git diff --check` — PASS (exit 0; `_DevEntry.unity:232` Unity trailing-whitespace artifact trimmed).
  - `.githooks/pre-commit` — PASS (`[pre-commit] OK`, temporary index).
  - Unity `6000.3.14f1` batchmode smoke — PASS, 17/17 checks, exit 0; telemetry `forced_flee_threshold`, `time_to_danger_seconds=26.00`, `ending_health=40/220`, `named_ending_health=330/520`, `clean_loop_preserved=True`.
- Carry-in constraint: satisfied. The named blocker is mechanically proven (Unity smoke telemetry + dotnet integration test) to change camp behavior across all five dimensions — discovery, time-to-danger, boundary pressure, clean-loop preservation, no farm-through. Human-play remains a qualified supplement; mechanical AC does not rest on presentation feel.
- Unity smoke note: the first two batchmode runs captured a Unity-editor `UnityEditor.Search.SearchInit.IndexationOnStartup` `ArgumentOutOfRangeException` (no default Search database asset in a fresh worktree — fires on any batchmode launch, outside the named-blocker runtime/controller/runner). The runner's `CaptureLog` was hardened to filter that diagnosed editor-startup noise; the final run is `Result: PASS` exit 0.
- Deviations: None. `ProjectSettings/ShaderGraphSettings.asset` was incidentally re-serialized by the Unity cold import (zero semantic change) and restored to its committed content + canonical CRLF — not part of the changeset.
- Carryover candidate for `/story-done`: `m2_runner_editor_noise_capture` — the sibling S2-M2-01/02/03 verification runners' `CaptureLog` would benefit from the same `UnityEditor.Search.SearchInit` editor-noise filter in a sprint-level runner-hardening batch.
- Next recommended: `/code-review` on the changed files, then `/story-done production/stories/s2-m2-04-named-blocker-camp-boundary.md`.

## Session Extract — /story-done 2026-05-14 (S2-M2-04)

- Story: `production/stories/s2-m2-04-named-blocker-camp-boundary.md` — S2-M2-04 Named Blocker + Camp Boundary.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 5/5 passing (0 deferred, 0 untested); all five ACs traced to file:line evidence in `tests/evidence/S2-M2-04/verification.md`.
- Review mode: lean — §9 subagent gates skipped; the code-review gate was satisfied separately by `/code-review ccb0c03` (2026-05-14, PASS_WITH_NOTES from main reviewer + `unity-specialist`; no blocking/high findings, 1 MEDIUM + LOW advisories).
- Evidence: `tests/evidence/S2-M2-04/verification.md` (Result PASS); Unity smoke `unity-named-blocker-runner-20260514-smoke.md` (17/17 checks, exit 0); dotnet 175/175; integration test `combat_runtime_named_blocker_boundary_test.cs` (3 tests).
- Implementation commit: `ccb0c03` — held unpushed pending the closure commit and its review.
- State updates: story status set to Complete + Completion Notes appended; `production/sprint-status.yaml` marks `S2-M2-04` done, sets `completed_stories: 6/6`, records `head: ccb0c03`, routes `next_active_command` to `/quick-design M3-objective-npc-loot`. Sprint 2 Milestone M2 (Combat Camp Loop) is complete; M3/M4/M5 remain un-story-broken.
- Carryover added (2): `m2_controller_scenario_smoke_abstraction` (pre-M3: extract a shared scenario-smoke abstraction before a 4th scenario grows the controller past ~2156 lines); `m2_runner_editor_noise_capture` (sprint-level batch: backport the `UnityEditor.Search.SearchInit` `CaptureLog` filter to the sibling S2-M2-01/02/03 runners). `control_manifest_absence_pre_existing` updated to include S2-M2-04.
- Tech debt logged: 0 to `docs/tech-debt-register.md` (file absent); 2 carryover entries added to `sprint-status.yaml` per the established project mechanism.
- Watch item: `production/sprints/sprint-2.md` Story Ledger row for `S2-M2-04` is stale (still "Blocked on S2-M2-03 / Not started") — outside `/story-done`'s standard write set; pending user confirmation to refresh.
- Next recommended: M3 story-breaking — `/quick-design M3-objective-npc-loot` (following the M2 quick-design then `/create-stories` pattern).

## Session Extract — M3 Objective + NPC + Loot Story Slate 2026-05-14

- Quick design: [design/quick/quick-design-m3-objective-npc-loot.md](../../design/quick/quick-design-m3-objective-npc-loot.md) (committed and pushed at `6704203`).
- Open stories:
  - [production/stories/s2-m3-00-scenario-smoke-handoff-cleanup.md](../stories/s2-m3-00-scenario-smoke-handoff-cleanup.md) - Ready for dev.
  - [production/stories/s2-m3-01-named-npc-objective-frame.md](../stories/s2-m3-01-named-npc-objective-frame.md) - Blocked on `S2-M3-00`.
  - [production/stories/s2-m3-02-objective-state-relic-hand-in.md](../stories/s2-m3-02-objective-state-relic-hand-in.md) - Blocked on `S2-M3-01`.
  - [production/stories/s2-m3-03-loot-table-fixed-profile-vendor.md](../stories/s2-m3-03-loot-table-fixed-profile-vendor.md) - Blocked on `S2-M3-02`.
  - [production/stories/s2-m3-04-end-to-end-objective-loop.md](../stories/s2-m3-04-end-to-end-objective-loop.md) - Blocked on `S2-M3-03`.
- Routing: `production/sprint-status.yaml` now tracks 11 Sprint 2 stories, 6 closed, 1 ready-for-dev, and 4 blocked in M3 dependency order.
- Sprint plan: `production/sprints/sprint-2.md` Story Ledger now lists the M3 slate after the M2 rows.
- Vendor scope: `S2-M3-03` uses the Inventory GDD's fixed-profile vendor rules blockout-grade (mechanism, not tuned economy); promotion triggers carried in the quick-design.
- Next command: `/story-readiness production/stories/s2-m3-00-scenario-smoke-handoff-cleanup.md`.

## Session Extract — /story-readiness 2026-05-17 (S2-M3-00)

- Story: `production/stories/s2-m3-00-scenario-smoke-handoff-cleanup.md` — Scenario Smoke Handoff Cleanup.
- Verdict: READY (convergent: Codex parallel run + this verification both READY).
- Review mode: Lean (no `production/review-mode.txt`; QL-STORY-READY skipped).
- Checks: header fields PASS, GDD traceability PASS (quick-design anchors `:216-223` and `:246-252` verified), ADR readiness PASS (D001/D002/D003/D012/D014 all Locked at `DECISIONS.md:15,35,51,342,418`), scope clarity PASS (6 ACs + Out Of Scope + Performance Budget), dependencies PASS (S2-M2-04 done at `production/sprint-status.yaml:96`), evidence PASS (`tests/evidence/S2-M3-00/verification.md` declared at story `:13`).
- Warnings (non-blocking): `production/sprint-status.yaml:11` `head: 6704203` lags actual `HEAD dc0f306` (expected sprint-status rollover convention drift); control manifest absent per pre-existing carryover `control_manifest_absence_pre_existing`.
- Next gate: `/dev-story production/stories/s2-m3-00-scenario-smoke-handoff-cleanup.md`.

## Session Extract — /dev-story 2026-05-17 (S2-M3-00)

- Story: `production/stories/s2-m3-00-scenario-smoke-handoff-cleanup.md` — S2-M3-00 Scenario Smoke Handoff Cleanup.
- Status after implementation: Implemented, pending `/code-review` and `/story-done`. Story-file header status remains `Ready for Story Readiness` (read-only readiness skill does not auto-update); sprint-status.yaml already routes it as `ready-for-dev` at `production/sprint-status.yaml:105`.
- Files changed:
  - `Assets/Editor/GravenspireScenarioSmokeRunnerHelpers.cs` — created. New shared static class hosting `IsEditorStartupNoise(condition, stackTrace, type)` as the single source of truth for the diagnosed `UnityEditor.Search.SearchInit` editor-startup noise filter. Designed to grow as further cross-runner helpers are needed by M3 and beyond.
  - `Assets/Editor/GravenspireScenarioSmokeRunnerHelpers.cs.meta` — created. Unity asset import companion for the new Editor script; matches the minimal two-line `fileFormatVersion: 2` + `guid` format used by every other Editor `.cs.meta` in the project. Persistent GUID `c471cec5ceec4d498fe462fcc0ea70a3` (verified unique by repo grep before write).
  - `Assets/Editor/GravenspireM2NamedBlockerVerificationRunner.cs` — modified. `CaptureLog` refactored to delegate the SearchInit suppression to the shared helper. Bit-equivalent behavior because the helper internally gates on `LogType.Error / Exception / Assert` (matches the prior inline gate).
  - `Assets/Editor/GravenspireM2CombatBridgeVerificationRunner.cs` — modified. `CaptureLog` backported to call the shared filter at the top. Additive suppression of a single diagnosed editor-startup noise condition (the same condition that was already being suppressed in the named-blocker runner). Resolves the `m2_runner_editor_noise_capture` carryover for this runner.
  - `Assets/Editor/GravenspireM2LinkedTrashOverpullVerificationRunner.cs` — modified. Same backport as above.
  - `Assets/Editor/GravenspireM2SingleTrashLoopVerificationRunner.cs` — modified. Same backport as above.
  - `Assets/Editor/GravenspireLaunchVerificationRunner.cs` — modified. Same backport as above.
  - `tests/evidence/S2-M3-00/verification.md` — created. AC trace, local gates, refactor verification note, deferred closure evidence protocol, carryover status.
- Evidence:
  - `tests/evidence/S2-M3-00/verification.md` — Result PARTIAL — PASS PENDING CLOSURE (Unity batchmode reruns + `.githooks/pre-commit` execution + implementation HEAD lock all finalized at `/story-done`); refactor proof in evidence file covers AC-03 at implementation gate. Top-level label updated per /code-review convergent finding on 2026-05-17.
- Verification:
  - `dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — PASS `175/175` (Duration 729 ms).
  - `git diff --check` — PASS (clean).
  - T1 negative-scope scan over 6 changed files — PASS WITH CLASSIFIED PRE-EXISTING HITS (only pre-existing `DateTime.UtcNow` calls in editor-evidence timestamping; none introduced by this story; the new helper file is clean).
  - `.githooks/pre-commit` — PARTIAL: implementation-gate scope analysis PASS (changes in `Assets/Editor/**` are outside the hook's `src/*.cs|tests/*.cs` deny-pattern scope by design; working-tree `git diff --check` equivalent to the hook's whitespace check passed). Actual hook execution against the staged closure index is DEFERRED to `/story-done`, which will stage the changeset and record `[pre-commit] OK` or the exact failure in the closure evidence row.
  - Unity batchmode reruns of four affected runners (M2-01 CombatBridge, M2-02 SingleTrashLoop, M2-03 LinkedTrashOverpull, M2-04 NamedBlocker) — DEFERRED to `/story-done` closure phase. The fifth modified runner (Launch) is exempt from the batchmode rerun because its evidence path is hardcoded to S2-FOUNDATION-01 (`Assets/Editor/GravenspireLaunchVerificationRunner.cs:18`) and lacks `-gravenspireEvidencePath` argument parsing; running it under S2-M3-00 would overwrite the foundation evidence rather than produce an S2-M3-00 artifact. Launch's backport is covered by the logical proof in the Refactor Verification Note. Protocol documented at `tests/evidence/S2-M3-00/verification.md` under **Closure Evidence**. AC-03's named set (S2-M2-02/03/04) maps to M2-02/M2-03/M2-04 in this list.
- Deviations:
  - Scope decision: extracted only the SearchInit log filter as the first concrete shared helper; did not extract the three parallel scenario-smoke subsystems from `Assets/Scripts/M2SingleTrashMedLoopController.cs` (would risk gameplay behavior change; out of scope per `production/stories/s2-m3-00-scenario-smoke-handoff-cleanup.md:50` through `:57`). M2 controller has zero diff in this story. Carryover `m2_controller_scenario_smoke_abstraction` is therefore PARTIALLY ADDRESSED (shared helper class established; further controller-side extraction remains a follow-up only if M3 ever requires it).
  - Backported SearchInit filter to four sibling runners (M2-01/M2-02/M2-03/Launch) in addition to the strict AC-04 requirement of "shared OR explicitly available". Justification: directly resolves the `m2_runner_editor_noise_capture` carryover (`production/sprint-status.yaml:44`), addresses the false-fail risk on fresh-worktree batchmode, and is logically AC-03-preserving (one-way suppression of a single diagnosed editor-startup noise condition; any pre-change PASS remains a post-change PASS).
- Carryover status:
  - `m2_runner_editor_noise_capture` (`production/sprint-status.yaml:44`) — RESOLVED by this story. Single shared filter consumed by all five existing runners; future runners can opt in with a one-line call.
  - `m2_controller_scenario_smoke_abstraction` (`production/sprint-status.yaml:43`) — PARTIALLY ADDRESSED. Shared helper class is the first piece of cross-runner abstraction; controller-side extraction explicitly deferred to keep gameplay behavior frozen.
  - `launch_runner_evidence_path_hardcoded` (NEW, surfaced during /code-review convergent verification on 2026-05-17) — Launch runner hardcodes its evidence path to S2-FOUNDATION-01 (`Assets/Editor/GravenspireLaunchVerificationRunner.cs:18`) and has no `-gravenspireEvidencePath` argument parsing. Follow-up story should harmonize Launch's evidence-path argument handling with the four M2 runners (~12 LOC: add `EvidencePathArgumentName`, `EvidencePathKey`, `DefaultEvidencePath`, `ResolveEvidencePathFromCommandLine`, and `CurrentEvidencePath` per the M2 pattern). Out of scope for S2-M3-00 per the story's behavior-preserving constraint.
- Next:
  - `/code-review` on the six changed files. Standing pair: `reviewer` + `qa-tester` per `feedback_review_subagent_pairings`; substitute `reviewer` → `unity-specialist` if the diff is flagged as Unity-runner-specific (it is — editor batchmode runners and log-capture lifecycle).
  - `/story-done production/stories/s2-m3-00-scenario-smoke-handoff-cleanup.md` to run the deferred Unity batchmode reruns, set status to Complete, append the closure extract, and update `production/sprint-status.yaml`.

## Session Extract — /story-done 2026-05-17 (S2-M3-00)

- Verdict: COMPLETE WITH NOTES.
- Story: `production/stories/s2-m3-00-scenario-smoke-handoff-cleanup.md` — Scenario Smoke Handoff Cleanup.
- Criteria: 6/6 passing. No fourth M3 scenario block added to `Assets/Scripts/M2SingleTrashMedLoopController.cs`; shared SearchInit helper exists; M2 preservation reruns passed; no gameplay code changed; local gates passed.
- Closure evidence:
  - `tests/evidence/S2-M3-00/m2-01-rerun-20260517-smoke.md` — PASS, exit code 0, no captured errors.
  - `tests/evidence/S2-M3-00/m2-02-rerun-20260517-smoke.md` — PASS, exit code 0, no captured errors.
  - `tests/evidence/S2-M3-00/m2-03-rerun-20260517-smoke.md` — PASS, exit code 0, no captured errors.
  - `tests/evidence/S2-M3-00/m2-04-rerun-20260517-smoke.md` — PASS, exit code 0, no captured errors.
  - `dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — PASS 175/175.
  - `.githooks/pre-commit` — PASS (`[pre-commit] OK`) against the staged closure index after closure documentation edits were complete.
- Code review: complete. Initial `/code-review` BLOCKED on Launch evidence-path and pre-commit evidence-label overclaims; both were fixed in evidence/session-state docs and third Codex verification returned clean.
- Tech debt / carryovers:
  - `m2_controller_scenario_smoke_abstraction` — PARTIALLY ADDRESSED; shared runner helper exists, controller-side extraction remains follow-up only if M3 telemetry needs it.
  - `m2_runner_editor_noise_capture` — RESOLVED; all five existing scenario-smoke/launch runners consume `GravenspireScenarioSmokeRunnerHelpers.IsEditorStartupNoise`.
  - `launch_runner_evidence_path_hardcoded` — NEW; Launch runner needs evidence-path argument parity before future story-local Launch reruns.
- Sprint routing: `S2-M3-00` done; `S2-M3-01` moved to ready-for-story-readiness; `S2-M3-02` through `S2-M3-04` remain blocked in dependency order.
- Next recommended: `/story-readiness production/stories/s2-m3-01-named-npc-objective-frame.md`.

## Session Extract — /story-readiness 2026-05-17 (S2-M3-01)

- Story: `production/stories/s2-m3-01-named-npc-objective-frame.md` — Named NPC Objective Frame.
- Verdict: READY (Codex parallel run + this verification both READY).
- Review mode: Lean.
- Checks: header fields PASS, GDD traceability PASS (quick-design `:126-140` and `:254-259`; npc-system `:104-105` lifecycle and `:388-407` no-marker / DLG-01 templated dialogue), ADR readiness PASS (D001/D002/D003 Locked; D004 Provisional with explicit T1 templated-dialogue boundary), scope clarity PASS (5 ACs + Out Of Scope + Performance Budget), dependencies PASS (S2-M3-00 done), evidence PASS (declared at story `:13`).
- Warnings (non-blocking): control manifest absent (pre-existing); D004 Provisional with T3 revisit trigger but T1 boundary explicit.
- Next gate: `/dev-story production/stories/s2-m3-01-named-npc-objective-frame.md`.

## Session Extract — /dev-story 2026-05-18 (S2-M3-01)

- Story: `production/stories/s2-m3-01-named-npc-objective-frame.md` — S2-M3-01 Named NPC Objective Frame.
- Status after implementation: Implemented, pending `/code-review` and `/story-done`. Story-file header status remained `Ready for Story Readiness` until `/story-done`.
- Files changed:
  - `Assets/Scripts/M3NamedNpcObjectiveFrame.cs` — created. Runtime MonoBehaviour component with `NpcInteractionContext` shape, `M3_Caretaker_T1` id, `dialogue.m3.caretaker.objective_frame_t1` template handle, `m3.objective.recover_marked_relic.frame` objective text key. `SessionLocalOnly => true` invariant at `:85`; `TryRecordIntentionalInteraction` at `:112`.
  - `Assets/Scripts/M3NamedNpcObjectiveFrame.cs.meta` — created.
  - `Assets/Editor/GravenspireM3NamedNpcObjectiveFrameBuilder.cs` — created. Editor builder that layers `M3_ObjectiveFrameRoot` / `M3_Caretaker` anchor into `_DevEntry.unity` after the M2 builder; does not mutate `Assets/Scripts/M2SingleTrashMedLoopController.cs`.
  - `Assets/Editor/GravenspireM3NamedNpcObjectiveFrameBuilder.cs.meta` — created.
  - `Assets/Editor/GravenspireM3NamedNpcObjectiveFrameVerificationRunner.cs` — created. Story-specific Unity smoke runner. Builds anchor, enters Play Mode, records intentional interaction, checks no marker affordances, verifies M2 handoff anchors remain available. Reuses `GravenspireScenarioSmokeRunnerHelpers.IsEditorStartupNoise` from S2-M3-00. Editor timing uses `EditorApplication.timeSinceStartup` + `DateTimeOffset.UtcNow` (not `DateTime.UtcNow`).
  - `Assets/Editor/GravenspireM3NamedNpcObjectiveFrameVerificationRunner.cs.meta` — created.
  - `Assets/Scenes/_DevEntry.unity` — modified: `M3_ObjectiveFrameRoot` + `M3_Caretaker` capsule with posture-staff child for blockout specificity. +256 line scene delta.
  - `tests/evidence/S2-M3-01/verification.md` — created. 5/5 AC trace, local gates, implementation notes, scope check.
  - `tests/evidence/S2-M3-01/unity-named-npc-objective-frame-20260518-smoke.md` — created. Story-specific Unity smoke evidence.
  - `tests/evidence/S2-M3-01/m2-02-preservation-20260518-smoke.md`, `m2-03-preservation-20260518-smoke.md`, `m2-04-preservation-20260518-smoke.md` — created. M2 preservation reruns produced by the established M2 runners with `-gravenspireEvidencePath` overrides (separate runs, not in-process re-driving of the M2 controller).
- Course-corrections during the run:
  - First runner attempted to drive all three M2 smoke methods in one mutated `M2SingleTrashMedLoopController` instance; controller is not designed for that sequence and produced invalid preservation failures. Pivoted to separate M2 runner invocations under S2-M3-01 evidence paths (matches S2-M3-00 closure protocol).
  - `FindObjectsByType` unqualified in editor static context — fixed to `UnityEngine.Object.FindObjectsByType`.
  - New `DateTime.UtcNow` deny-pattern hit in the editor runner — replaced with `EditorApplication.timeSinceStartup` + `DateTimeOffset.UtcNow`.
- Verification:
  - `dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — PASS 175/175 (558ms).
  - `git diff --check` — PASS (clean after Unity `m_Name:` trailing-space trim in `_DevEntry.unity`).
  - T1 negative-scope scan over the four implementation-surface files — PASS, zero `DateTime.UtcNow` matches on Assets/Scripts/M3*.cs, Assets/Editor/GravenspireM3*.cs, Assets/Scenes/_DevEntry.unity.
  - Unity M3 named NPC smoke — PASS, exit 0.
  - Unity M2-02/M2-03/M2-04 preservation reruns — PASS, exit 0 each.
- Scope: `Assets/Scripts/M2SingleTrashMedLoopController.cs` zero diff; no `src/gameplay/combat/**` changes; no `Packages/manifest.json` changes; no Dialogue UI, quest log, minimap, marker affordance, schedule ticking, persistence, faction reaction, loot, vendor, Save/Load, or live LLM.
- Next: `/code-review` on the changed files, then `/story-done`.

## Session Extract — /code-review 2026-05-18 (S2-M3-01)

- Target: `Assets/Scripts/M3NamedNpcObjectiveFrame.cs`, `Assets/Editor/GravenspireM3NamedNpcObjectiveFrameBuilder.cs`, `Assets/Editor/GravenspireM3NamedNpcObjectiveFrameVerificationRunner.cs`, `Assets/Scenes/_DevEntry.unity`, `tests/evidence/S2-M3-01/**`.
- Verdict: PASS_WITH_NOTES after two evidence-only fixes (both originally classified BLOCKING by Codex; both resolved without code-side changes).
- Findings:
  - P1 (BLOCKING, resolved in-evidence): `tests/evidence/S2-M3-01/verification.md:44` originally claimed "no matches" while the same row contained the literal `DateTime.UtcNow` deny term. Resolved by changing the row verdict to PASS WITH CLASSIFIED DOC HIT and explicitly classifying the hit as documentation-only (no runtime, scene, or runner code hits). Convention matches `tests/evidence/S2-M2-03/verification.md:43`. Edit included Brian's wording tweak ("evidence-only hit(s) are confined to this row's documented deny-term text").
  - P2 (HIGH, routed to carryover): `tests/evidence/S2-M3-01/m2-02-preservation-20260518-smoke.md:3` carries `**Date:** 2026-05-12` because `Assets/Editor/GravenspireM2SingleTrashLoopVerificationRunner.cs:192` hardcodes that literal. Pre-existing runner-hygiene gap; routed to new sprint-status carryover `m2_02_runner_date_hardcoded` and documented in verification.md Implementation Notes.
- Implementation surface clean: no blocking findings on the Unity runtime component, builder, runner, or scene delta. Anchor, trigger collider, templated context shape, markerless checks, session-local invariant, and M2 preservation shape all line up with the story ACs.
- Verification (Claude): `dotnet test tests/Gravenspire.Combat.Tests.csproj` PASS 175/175; `git diff --check` clean.
- Next: `/story-done production/stories/s2-m3-01-named-npc-objective-frame.md`.

## Session Extract — /story-done 2026-05-18 (S2-M3-01)

- Verdict: COMPLETE WITH NOTES.
- Story: `production/stories/s2-m3-01-named-npc-objective-frame.md` — S2-M3-01 Named NPC Objective Frame.
- Criteria: 5/5 passing. `M3_Caretaker` anchor exists in `_DevEntry.unity`; intentional interaction records `NpcInteractionContext`-shaped event + templated dialogue handle; no marker affordances or live LLM; session-local surface preserved; M2 preservation proven via separate established runners.
- Closure evidence:
  - `tests/evidence/S2-M3-01/verification.md` — PASS.
  - `tests/evidence/S2-M3-01/unity-named-npc-objective-frame-20260518-smoke.md` — PASS, exit 0, no captured errors.
  - `tests/evidence/S2-M3-01/m2-02-preservation-20260518-smoke.md` — PASS, exit 0 (embedded date is stale per `m2_02_runner_date_hardcoded` carryover; runner output is sound).
  - `tests/evidence/S2-M3-01/m2-03-preservation-20260518-smoke.md` — PASS, exit 0.
  - `tests/evidence/S2-M3-01/m2-04-preservation-20260518-smoke.md` — PASS, exit 0.
  - `dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — PASS 175/175.
  - `.githooks/pre-commit` — PASS (`[pre-commit] OK`) against the staged closure index.
- Code review: complete. Two evidence-only fixes applied during /code-review (P1 doc-hit reclassification + P2 runner-hygiene carryover routing). Unity runtime surface clean.
- Tech debt / carryovers added: `m2_02_runner_date_hardcoded` — NEW; M2-02 runner hardcodes embedded date; follow-up runner-hygiene story should parameterize the embedded date (and ideally the story header) alongside the `launch_runner_evidence_path_hardcoded` parity fix.
- Sprint routing: `S2-M3-01` done; `S2-M3-02` moved to ready-for-story-readiness; `S2-M3-03` through `S2-M3-04` remain blocked in dependency order.
- Next recommended: `/story-readiness production/stories/s2-m3-02-objective-state-relic-hand-in.md`.

## Session Extract — /dev-story 2026-05-18 (S2-M3-02)

- Story: `production/stories/s2-m3-02-objective-state-relic-hand-in.md` — S2-M3-02 Objective State + Relic Hand-In.
- Status after implementation: Implemented, pending `/code-review` and `/story-done`. Story-file header status remains `Ready for Story Readiness` until closure.
- Files changed:
  - `src/gameplay/npc/m3-objective/M3ObjectiveStateRelicHandInSession.cs` — pure C# four-state session-local objective model: `NotIntroduced -> Accepted -> RelicRecovered -> Complete`.
  - `tests/unit/gameplay/npc/m3_objective_state_relic_hand_in_test.cs` — five NUnit tests for accept, relic availability, recovery, hand-in, and invalid ordering.
  - `src/gameplay/npc/m3-objective/*` package files — narrow Unity local package for the M3 objective model only.
  - `Packages/manifest.json` and `Packages/packages-lock.json` — local package wiring for `com.gravenspire.gameplay.npc.m3objective`.
  - `tests/Gravenspire.Combat.Tests.csproj` — includes `tests/unit/gameplay/npc/*.cs`.
  - `Assets/Scripts/M3ObjectiveStateRelicHandIn.cs` — Unity wrapper around the tested session model.
  - `Assets/Editor/GravenspireM3ObjectiveStateRelicHandInBuilder.cs` — adds `M3_ObjectiveStateRoot` and inactive authored `M3_ObjectiveRelic` to `_DevEntry.unity`.
  - `Assets/Editor/GravenspireM3ObjectiveStateRelicHandInVerificationRunner.cs` — story-specific Unity smoke runner for accept/recover/hand-in telemetry.
  - `Assets/Scenes/_DevEntry.unity` — scene object references for the objective root and relic marker.
  - `tests/evidence/S2-M3-02/verification.md` plus four smoke files — implementation evidence and M2 preservation reruns.
- Verification:
  - `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — PASS 180/180.
  - Unity S2-M3-02 objective smoke — PASS, exit 0, `tests/evidence/S2-M3-02/unity-objective-state-relic-hand-in-20260518-smoke.md`.
  - Unity M2 preservation reruns — PASS, exit 0 each for M2-02, M2-03, and M2-04 under `tests/evidence/S2-M3-02/`.
  - T1 negative-scope scan — PASS WITH CLASSIFIED DOC HITS; only this verification summary's excluded-scope wording matched.
  - `git diff --check` — PASS on a temporary staged index containing only the approved S2-M3-02 file batch.
  - `.githooks/pre-commit` — PASS (`[pre-commit] OK`) on the same temporary staged index.
- Scope:
  - `Assets/Scripts/M2SingleTrashMedLoopController.cs` has zero diff.
  - No Save/Load persistence, repair-by-load path, faction consequence, full Inventory implementation, vendor sale, currency path, quest journal, minimap marker, broad polling, live LLM behavior, or S2-M3-03 loot finalization was added.
  - Root `src/gameplay/npc` Unity packaging was deliberately avoided because it would import existing NPC lifecycle/save-barrier code into Unity; this story uses only `src/gameplay/npc/m3-objective`.
- Deferred closure notes:
  - `/code-review` should review the pure model, Unity wrapper, package boundary, runner, scene delta, and evidence.
  - `/story-done` should update story status/routing after review, rerun final local gates after any review fixes, and preserve the existing `m2_02_runner_date_hardcoded` carryover note for the stale embedded M2-02 smoke date.

## Session Extract — /story-done 2026-05-18 (S2-M3-02)

- Verdict: COMPLETE WITH NOTES.
- Story: `production/stories/s2-m3-02-objective-state-relic-hand-in.md` — S2-M3-02 Objective State + Relic Hand-In.
- Criteria: 6/6 passing. Accept, authored relic availability, relic recovery, NPC hand-in, session-local/no-leakage, and M2 preservation are all covered by unit tests and story-local Unity/evidence artifacts.
- Closure evidence:
  - `tests/evidence/S2-M3-02/verification.md` — COMPLETE WITH NOTES.
  - `tests/unit/gameplay/npc/m3_objective_state_relic_hand_in_test.cs` — five NUnit tests covering accept, relic availability, recovery, hand-in, and invalid ordering.
  - `tests/evidence/S2-M3-02/unity-objective-state-relic-hand-in-20260518-smoke.md` — PASS, exit 0, state sequence `NotIntroduced -> Accepted -> RelicRecovered -> Complete`.
  - `tests/evidence/S2-M3-02/m2-02-preservation-20260518-smoke.md` — PASS, exit 0 (embedded date remains stale per `m2_02_runner_date_hardcoded`; runner output is sound).
  - `tests/evidence/S2-M3-02/m2-03-preservation-20260518-smoke.md` — PASS, exit 0.
  - `tests/evidence/S2-M3-02/m2-04-preservation-20260518-smoke.md` — PASS, exit 0.
  - `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — PASS 180/180.
  - T1 negative-scope scan over implementation, Unity, package, scene, and evidence surfaces — PASS, zero matches.
- Code review: complete. Evidence-label overclaim fixed; CodeRabbit `-langversion:10.0` concern verified as project-context false positive and documented in `tests/evidence/S2-M3-02/verification.md`.
- Tech debt / carryovers: none new. Existing `m2_02_runner_date_hardcoded` remains the runner-hygiene carryover for stale M2-02 embedded dates.
- Sprint routing: `S2-M3-02` done at closure commit `fb77f83`; `S2-M3-03` moved to ready-for-story-readiness; `S2-M3-04` remains blocked on `S2-M3-03`.
- Next recommended: `/story-readiness production/stories/s2-m3-03-loot-table-fixed-profile-vendor.md`.

## Session Extract — /dev-story 2026-05-19 (S2-M3-03)

- Story: `production/stories/s2-m3-03-loot-table-fixed-profile-vendor.md` — S2-M3-03 Loot Table + Fixed-Profile Vendor.
- Status after implementation: Implemented locally, pending `/code-review` and `/story-done`. Story-file header and `production/sprint-status.yaml` are not closure-updated yet; `/story-done` owns final status/routing changes.
- Files changed:
  - `data/first-district/m3-objective-npc-loot.json` — authored M3 loot/vendor data with `CourtMarkedRelic_T1`, `GraveDust_Salvage_T1`, F4 multiplier `0.15`, carried-slot cap, and one fixed-price vendor offer.
  - `src/gameplay/npc/m3-objective/M3LootTableFixedProfileVendorSession.cs` and `.meta` — pure session model for authored loot resolution, salvage sale, capacity-prevalidated fixed purchase, atomicity, no dynamic hooks, and session-local currency.
  - `tests/unit/gameplay/npc/m3_loot_table_fixed_profile_vendor_test.cs` — nine NUnit tests covering the eight S2-M3-03 ACs.
  - `tests/Gravenspire.Combat.Tests.csproj` — copies `data/first-district/**` into the test output so tests validate the authored JSON.
  - `Assets/Scripts/M3LootTableFixedProfileVendor.cs` and `.meta` — Unity runtime bridge for the session model.
  - `Assets/Editor/GravenspireM3LootTableFixedProfileVendorBuilder.cs` and `.meta` — layers `M3_LootVendorRoot`, `M3_CourtVendor`, and `M3_CourtVendor_SalvageCounter` onto `_DevEntry.unity`.
  - `Assets/Editor/GravenspireM3LootTableFixedProfileVendorVerificationRunner.cs` and `.meta` — story-specific Unity smoke runner for objective recovery, loot resolution, salvage sale, and fixed-profile checks.
  - `Assets/Scenes/_DevEntry.unity` — adds the M3 vendor marker/counter only.
  - `tests/evidence/S2-M3-03/verification.md` and `tests/evidence/S2-M3-03/unity-loot-table-fixed-profile-vendor-20260519-smoke.md` — implementation evidence.
- Verification:
  - `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — PASS 189/189.
  - Unity `6000.3.14f1` S2-M3-03 smoke — PASS, 37/37 checks, no warnings/errors.
  - T1 negative-scope scan over the new S2-M3-03 `.cs` implementation/test files — PASS, zero matches.
  - Story-specific forbidden loot-scope scan over production code and authored JSON — PASS, zero matches.
  - `git diff --check` — PASS after normalizing Unity blank `m_Name` serialization whitespace.
- Scope:
  - No full Inventory persistence, Save/Load hook, tuned economy claim, coin pacing claim, dynamic vendor profile, stock ticking, reputation discount, combat-owned loot identity, or reused progression seed path was added.
  - Existing pre-existing untracked `.claude/**` and `all-skills-claude*.patch` items remain unrelated and untouched.
- Next recommended: `/code-review` the S2-M3-03 implementation/evidence files, then `/story-done production/stories/s2-m3-03-loot-table-fixed-profile-vendor.md`.

## Session Extract — /code-review 2026-05-20 (S2-M3-03)

- Target: S2-M3-03 working-tree implementation + evidence (loot table + fixed-profile vendor).
- Initial verdict: PASS_WITH_NOTES — **incorrect**. The first pass saw the runtime-hardcoded-data pattern, recorded it as a LOW note, and rationalized it as "likely intentional" instead of holding it against AC-01 ("resolves... from authored data") and `gameplay-code.md:32`.
- Corrected verdict: NEEDS_FIX. Codex's independent reconciliation caught the miss. The runtime used `CreateAuthoredM3Default()` (hardcoded factory) and never consumed the authored JSON — AC-01 was met only by the test, not the runtime.
- Fix implemented and re-reviewed: runtime now loads `data/first-district/m3-objective-npc-loot.json` as primary data; `CreateAuthoredM3Default()` demoted to documented missing-file fallback; dual-JSON-path (`#if UNITY_5_3_OR_NEWER` — Newtonsoft / System.Text.Json) matches the `CombatFixtureLoader` precedent; the test converged on the shared loader (private parser removed); ProjectSettings editor drift reverted.
- Re-review verdict: PASS_WITH_NOTES. HIGH finding resolved with mechanical evidence (smoke checks `vendor_loaded_authored_data_file`, `vendor_not_using_fallback_data`; telemetry `authored_data_file_loaded=True`). Three non-blocking LOW notes deferred to the runner-hygiene cleanup story.
- Process lesson (candidate for `tasks/lessons.md`): a passing test proves the test's path works, not that the acceptance criterion is met — verify the runtime path satisfies the AC, not just the test fixture.

## Session Extract — /story-done 2026-05-20 (S2-M3-03)

- Verdict: COMPLETE WITH NOTES (8/8 AC passing).
- Story: `production/stories/s2-m3-03-loot-table-fixed-profile-vendor.md` — S2-M3-03 Loot Table + Fixed-Profile Vendor.
- Closure evidence:
  - `tests/evidence/S2-M3-03/verification.md` — COMPLETE WITH NOTES.
  - `tests/unit/gameplay/npc/m3_loot_table_fixed_profile_vendor_test.cs` — 9 NUnit tests covering the 8 ACs.
  - `tests/evidence/S2-M3-03/unity-loot-table-fixed-profile-vendor-20260519-smoke.md` — PASS, 39/39 checks, exit 0; telemetry `salvage_sale_credited_copper=7`, `authored_data_file_loaded=True`.
  - `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — PASS 189/189.
  - T1 negative-scope scan — PASS, zero hits.
  - `git diff --check` — PASS; `.githooks/pre-commit` — PASS against the staged closure index.
- Code review: complete. Initial `/code-review` verdict corrected NEEDS_FIX -> fix -> re-review PASS_WITH_NOTES (see `/code-review` extract above).
- Carryover added: `m3_03_low_review_notes` — three non-blocking LOW review notes (smoke-filename date drift, `Enum.Parse` outside JSON catch, Editor-context path resolution) deferred to the runner-hygiene cleanup story.
- Sprint routing: `S2-M3-03` done at closure commit `25c94ee`; `S2-M3-04` moved to ready-for-story-readiness; M3 slate is now 4/5 complete with `S2-M3-04` the final proof story.
- Next recommended: `/story-readiness production/stories/s2-m3-04-end-to-end-objective-loop.md`.

## Session Extract — /dev-story 2026-05-20 (S2-M3-04)

- Story: `production/stories/s2-m3-04-end-to-end-objective-loop.md` — S2-M3-04 End-To-End Objective Loop (proof/integration story; Type Integration / Visual-Feel).
- Status after implementation: Implemented locally, pending `/code-review`, the Unity batchmode smoke, and `/story-done`. Story-file `Status:` header still reads "Ready for Story Readiness" (stale — `/story-readiness` is read-only and never writes the file back); `/story-done` owns final status/routing changes.
- Files changed:
  - `Assets/Editor/GravenspireM3EndToEndObjectiveLoopVerificationRunner.cs` — created. M3 end-to-end batchmode verification runner; mirrors the M3-03 runner's `[InitializeOnLoad]` + `SessionState` domain-reload pattern. Invokes the three M3 builders, drives accept -> recover -> loot resolve -> hand-in -> salvage sale, runs the M2 clean-loop and named-blocker smokes, and emits all 15 required telemetry labels plus PASS/FAIL checks.
  - `tests/evidence/S2-M3-04/human-play-20260520.md` — created. AC-06 human-play evidence stub (the "one more pull" question plus honest presentation-limitation classification).
  - `production/stories/s2-m3-04-end-to-end-objective-loop.md` — modified. Readiness Warning 3 fix: human-play evidence filename `human-play-20260514.md` -> `human-play-20260520.md` (AC-06 row + Test Evidence list).
- Verification:
  - `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — PASS 189/189 (regression; the new runner is an Editor script outside the test csproj's compile set).
  - API surface of every component the runner calls verified by grep against the real M3/M2 sources — no compile-breakers; all three M3 builders confirmed (`public static void Build()`, namespace `Gravenspire.Editor`).
  - Deny-pattern scan of the new runner — PASS, zero hits (`DateTimeOffset.UtcNow` for the evidence date, `EditorApplication.timeSinceStartup` for timing).
  - Unity batchmode smoke — NOT RUN. The runner only compiles/executes inside Unity; the smoke is the post-`/code-review` verification step.
- Scope:
  - One new file only (the Editor runner). No new session model, Unity bridge, scene object, or `.unity` edit; no `src/` or `tests/` change. The proof story composes existing M3-01/02/03 + M2 surfaces.
  - One in-implementation correctness tightening: telemetry `npc_interaction_intentional` sourced from `LastInteraction.WasIntentional` rather than `HasRecordedInteraction`.
  - Pre-existing untracked `.claude/**` and `all-skills-claude*.patch` items remain unrelated and untouched.
- Next recommended: `/code-review` the S2-M3-04 runner, run the Unity batchmode smoke, then `/story-done production/stories/s2-m3-04-end-to-end-objective-loop.md`.

## Session Extract — /story-done 2026-05-20 (S2-M3-04)

- Verdict: COMPLETE WITH NOTES (5/6 AC; AC-01 through AC-05 PASS, AC-06 DEFERRED — feel-validation transferred to Sprint 3).
- Story: `production/stories/s2-m3-04-end-to-end-objective-loop.md` — S2-M3-04 End-To-End Objective Loop.
- Closure evidence:
  - `tests/evidence/S2-M3-04/verification.md` — COMPLETE WITH NOTES.
  - `tests/evidence/S2-M3-04/unity-end-to-end-objective-loop-20260520-smoke.md` — Unity batchmode smoke PASS, 29/29 checks, no warnings, no errors, exit 0.
  - `tests/evidence/S2-M3-04/human-play-20260520.md` — AC-06 human-play: the M3 objective loop was found not player-interactive in blockout; feel-validation transferred to Sprint 3.
  - `dotnet test tests\Gravenspire.Combat.Tests.csproj` — PASS 189/189.
  - T1 negative-scope deny-scan over the new runner — zero hits; `git diff --check` + `.githooks/pre-commit` — PASS at the staged closure index.
- Code review and debugging: `/code-review` (lean, with a `unity-specialist` pass) — PASS_WITH_NOTES. The Unity smoke initially failed on the M2 named-blocker checks; root-caused via systematic debugging (M2SingleTrashMedLoopController's shared, never-reset `_playerMeleeRandom` cursor — chaining M2 smokes contaminated it) and fixed (the runner now calls only `RunAutomatedNamedBlockerBoundarySmoke()`); re-run smoke 29/29 PASS.
- Carryovers added: `s2_m3_04_ac06_transfer` (AC-06 feel-validation -> Sprint 3 assembly milestone); `m2_melee_rng_not_reset` (M2 controller melee-RNG cursors never reset between smokes; M2 smokes cannot be chained); `m3_04_low_review_notes` (S2-M3-04 runner `/code-review` LOW notes).
- Re-plan context: per lead decision (2026-05-20), Sprint 2 closes with M1-M3 delivered as runner-proven systems; Sprint 3 opens as the "Playable Vertical-Slice Assembly" milestone (wires the M3 systems behind player input in a navigable greybox); M4/M5 deferred behind it. Unity 6.4 drift (6 files) reverted before closure.
- Sprint routing: `S2-M3-04` done at closure commit `ee7c450`; M3 slate 5/5 complete; Sprint 2 ready for close-out.
- Next recommended: continue the Sprint 2 -> Sprint 3 re-plan (close Sprint 2; open Sprint 3; record D016 + the definition-of-done lesson).

## Session Extract — Re-Plan Governance Lock 2026-05-20

- Per the producer + creative-director reassessment and lead decision (2026-05-20), the Sprint 2 -> Sprint 3 re-sequence is locked:
  - `DECISIONS.md` D016 — the decision: Sprint 2 closes with M1-M3 delivered as runner-proven systems; Sprint 3 is a new "Playable Vertical-Slice Assembly" milestone (wires the proven M3/M2 systems behind player input in a navigable greybox); M4/M5 defer behind it; S2-M3-04's AC-06 feel-validation transferred to Sprint 3.
  - `tasks/lessons.md` 2026-05-20 entry: feel-gates belong only on human-playable stories; playability is a milestone entry condition (bounds the 2026-05-14 telemetry lesson).
  - `production/sprint-status.yaml`: Sprint 2 marked CLOSED (11/11); `m2_presentation_threshold_gap` carryover RETIRED with the greybox-not-art presentation-minimum answer.
- Re-plan status: DONE — 6.4-drift revert, S2-M3-04 closure (`ee7c450` + sync `c66654b`), Sprint 2 close, D016 + the definition-of-done lesson. REMAINING — open Sprint 3: author the sprint plan + ~6 assembly story files (a focused next effort).
- Sprint 3 brief (in D016 + the 2026-05-20 producer/creative-director reassessments): ~6 dependency-ordered stories — player interaction harness -> player-driven NPC interaction -> player relic recovery + looting -> player-driven vendor -> navigable greybox district -> playable end-to-end + human-play. A short technical-director feasibility consult precedes the slate. Greybox, not produced art. Reuse the M3 systems, do not rebuild.
- Next recommended: open Sprint 3 (`/sprint-plan` then `/create-stories` for the Playable Vertical-Slice Assembly milestone) per DECISIONS.md D016.
