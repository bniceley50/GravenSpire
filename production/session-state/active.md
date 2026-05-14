# Active Session State

**Last Updated:** 2026-05-14
**Project Stage:** Pre-Production — Sprint 2 — S2-M2-04 Complete / M3 Quick-Design Next

## Current Task

Sprint 2 M2 progress is 5/6: `S2-M2-03 Linked trash overpull` is closed (`/story-done` 2026-05-13, **COMPLETE WITH NOTES**, 5/5 AC; implementation `bb6deab`, closure `3ce334c`). Linked-trash overpull pressure is mechanically validated: smoke telemetry recorded `overpull_outcome=forced_flee_threshold` and `ending_health=14/140`, with the clean M2-02 single-trash loop preserved. All four local gates passed (dotnet 172/172, T1 negative-scope scan, git diff --check, .githooks/pre-commit). The S2-M2-03-cycle process lessons were captured 2026-05-14 in `fc20a86` (pushed to `origin/main`).

`S2-M2-04 Named blocker + camp boundary` is implemented via `/dev-story` on 2026-05-14; the story header is flipped to **In Progress** (sprint-status row left for `/story-done` per the dev-story skill). The binding telemetry carry-in is satisfied: the Unity `6000.3.14f1` smoke recorded `named_blocker_outcome=forced_flee_threshold`, `time_to_danger_seconds=26.00`, `ending_health=40/220`, `named_ending_health=330/520`, `clean_loop_preserved=True` (17/17 checks PASS, exit 0), and the new dotnet integration test proves the same FEEL-02 boundary mechanically. All local gates passed (dotnet 175/175, T1 negative-scope scan, git diff --check, .githooks/pre-commit). Next is `/code-review` on the changed files, then `/story-done`.

## Status

- ✓ Brainstorm complete → [design/gdd/game-concept.md](../../design/gdd/game-concept.md)
- ✓ Engine configured → Unity 6.3 LTS + C# + URP (FishNet planned for Tier 2+)
- ✓ Art bible complete → [design/art/art-bible.md](../../design/art/art-bible.md) (9/9 sections, Lean mode, AD-ART-BIBLE skipped)
- ✓ Systems mapped → [design/gdd/systems-index.md](../../design/gdd/systems-index.md) (33 systems, 26 MVP)
- ✓ Spelling normalized Gravespire → Gravenspire, AGENTS.md §0 canonical path fixed (commit `486f0a0`)
- ✓ D006 committed — Codex onboarded as parallel implementer (commit `fae8c8c`)
- ✓ Codex onboarding brief delivered; Codex has completed read-only onboarding and surfaced 4 clarifying questions (all answered)
- ✓ Codex Assignment #1 drafted — `dotnet format` setup on branch `codex/dotnet-format-setup`
- ✅ World Structure GDD — latest review log verdict is **APPROVED** at `design/gdd/reviews/world-structure-review-log.md`. Earlier MAJOR REVISION text was historical and is no longer the current verdict; remaining World Structure work is explicit ADR/prototype gating, not unresolved GDD revision.
- ✅ Commit `4801d8a` on `main` (GDD + registry + systems-index + active.md bundle; rebased onto PR #1 merge).
- ✅ Character Progression GDD skeleton created 2026-04-25 at [design/gdd/character-progression.md](../../design/gdd/character-progression.md).
- ✅ Character Progression Overview section written 2026-04-25.
- ✅ Character Progression Player Fantasy section written 2026-04-25.
- ✅ Character Progression Detailed Design section written 2026-04-25.
- ✅ Character Progression Formulas section written 2026-04-25.
- ✅ Character Progression Edge Cases section written 2026-04-25.
- ✅ Character Progression Dependencies section written 2026-04-25.
- ✅ Character Progression Tuning Knobs section written 2026-04-25.
- ✅ Character Progression Visual/Audio Requirements section written 2026-04-25.
- ✅ Character Progression UI Requirements section written 2026-04-25.
- ✅ Character Progression Acceptance Criteria section written and count-verified 2026-04-25 (`44` criteria; `42` ordinary T1-blocking, `2` fixture-gated T1-blocking, `0` advisory-at-T1).
- ✅ Character Progression full review reached **APPROVED** after ADR-alignment and specialist blocker fixes; landed in commit `6459ceb`.
- ✅ Character Progression metadata/review-log follow-up landed in commit `557fa3e`.
- ✅ Inventory & Item Economy GDD skeleton created 2026-04-26 at [design/gdd/inventory-item-economy.md](../../design/gdd/inventory-item-economy.md).
- ✅ Inventory & Item Economy Overview section written 2026-04-26.
- ✅ Inventory & Item Economy Player Fantasy section written 2026-04-26.
- ✅ Inventory & Item Economy Detailed Rules section written 2026-04-26.
- ✅ Inventory & Item Economy Formulas section written 2026-04-26.
- ✅ Inventory & Item Economy Edge Cases section written 2026-04-26.
- ✅ Inventory & Item Economy Dependencies section written 2026-04-26.
- ✅ Inventory & Item Economy Tuning Knobs section written 2026-04-26.
- ✅ Inventory & Item Economy Acceptance Criteria section written and count-verified 2026-04-26 (`51` criteria; `47` ordinary T1-blocking, `4` fixture-gated T1-blocking, `0` advisory-at-T1).
- ✅ Inventory & Item Economy Open Questions section written 2026-04-26.
- ⚠️ Inventory & Item Economy full design review completed 2026-04-26 with verdict **NEEDS REVISION** and scope signal **XL**. Six blocker groups: (1) Save/Load first-save reverse-listing drift, (2) Inventory schema/id/hydration pre-spec gap, (3) F1/currency/vendor math gaps, (4) vendor transaction economy not closed, (5) future Combat / Death & Corpse Recovery / Faction Reputation criteria leaking into T1, (6) Layer 1 HUD / Inventory UI receiver not registered.
- ✅ Inventory blocker 1 repaired 2026-04-26: Save/Load now reverse-lists `InventoryFirstSaveMaterializer`; remaining blockers parked in [design/gdd/inventory-item-economy.md](../../design/gdd/inventory-item-economy.md) `INV-OQ-05` for future Inventory implementation pre-spec.
- 🧪 `/prototype combat-feel` started 2026-04-26. Tech choice locked to Unity 6.3 LTS standalone prototype under `prototypes/combat-feel/`, with 3-5 sequential pulls, at least one default med break between pulls 2 and 3, explicit success/failure criteria, and durable findings target `production/prototypes/combat-feel-report.md`.
- ✅ Combat-feel scripted mechanics smoke passed under locally installed Unity `6000.4.1f1` batchmode (not pinned `6000.3.x`): 5/5 scripted pulls, 97.1s combat, 41.9s downtime, 19.4s average pull, 1 med break, 9 Smites, 4 Heals, 0 unsafe pulls. Human feel playtest still required before verdict.
- ✅ Sprint 1.5 plan committed 2026-05-06 at `8885d2e`: [production/sprints/sprint-1-5.md](../sprints/sprint-1-5.md).
- ✅ Sprint 1.5 QA plan committed 2026-05-06 at `b6297b4`: [production/qa/plans/qa-plan-sprint-1-5-20260506.md](../qa/plans/qa-plan-sprint-1-5-20260506.md).
- ✅ Sprint 1.5 story `T1.5-COMBAT-00` closed via `/story-done` 2026-05-06 with verdict **COMPLETE WITH NOTES**. Commit `c2487fc` lands D013 and ADR-0006 as Proposed.
- ✅ Sprint 1.5 story `T1.5-COMBAT-01` closed via `/story-done` 2026-05-07 with verdict **COMPLETE WITH NOTES**. Commit `d6c8e08` lands Endurance state.
- ✅ Sprint 1.5 story `T1.5-COMBAT-02` closed via `/story-done` 2026-05-07 with verdict **COMPLETE WITH NOTES**. Commit `9aacee0` lands physical conversions; ADR-0006 Accepted / D013 Locked.
- ✅ Sprint 1.5 story `T1.5-COMBAT-03` closed via `/story-done` 2026-05-08 with verdict **COMPLETE WITH NOTES**. Commit `e7233b5` lands overpull tuning.
- ✅ Sprint 1.5 story `T1.5-COMBAT-04` closed via `/story-done` 2026-05-08 with verdict **COMPLETE WITH NOTES**. Commit `bd6c81b` lands D014 Locked.
- ✅ Sprint 1.5 carryover story `T1-COMBAT-11` closed via `/story-done` 2026-05-08 with verdict **COMPLETE WITH NOTES**. Commit `496ebc6` provenance restructure.
- ✅ Sprint 1.5 story `T1.5-COMBAT-05` closed via `/story-done` 2026-05-09 with verdict **COMPLETE**. Commit `960a148` lands profiled rerun evidence; `caea662` fixes verification provenance.
- ✅ Sprint 1.5 close-out smoke recorded **PASS WITH WARNINGS** at `production/qa/smoke-sprint-20260509.md`.
- ✅ Sprint 1.5 QA sign-off recorded **APPROVED WITH CONDITIONS** at `production/qa/qa-signoff-sprint-1-5-20260509.md`.
- ✅ Sprint 1.5 gate-check recorded **PASS** at `production/gate-checks/gate-check-2026-05-09-sprint-1-5-closeout.md`; rollover to Sprint 2 is unblocked.
- ✅ Sprint 2 target refined to **Gravenspire T1: The First District**: one Cleric, one cursed-city district, three enemy types, one named NPC, one faction presence, one objective, one loot table, one vendor or stash, one save/load flow, and one visible world-state change.
- ✅ Sprint 2 first implementation constraint resolved: production Unity shell now exists under `Assets/**`, `ProjectSettings/**`, and `Packages/**`.
- ✅ Sprint 2 story `S2-FOUNDATION-01` opened as a blocked routing story for the Unity project shell.
- ✅ Sprint 2 QA plan recorded at [production/qa/plans/qa-plan-sprint-2-20260509.md](../qa/plans/qa-plan-sprint-2-20260509.md).
- ✅ Sprint 2 story `S2-FOUNDATION-01` closed via `/story-done` 2026-05-09 with verdict **COMPLETE WITH NOTES**. Commit `f5f74dc` lands the Unity project shell.
- ✅ Sprint 2 M1 Player In World foundation and launch verification are complete; M2 story slate is now open.
- ✅ Sprint 2 M1 Unity launch verification complete (manual Play Mode + Unity CLI runner) at [tests/evidence/S2-FOUNDATION-01/unity-cli-launch-verification-20260510.md](../../tests/evidence/S2-FOUNDATION-01/unity-cli-launch-verification-20260510.md); M2 story slate is now open.
- ✅ Sprint 2 M2 Combat Camp Loop quick design recorded at [design/quick/quick-design-m2-combat-camp-loop.md](../../design/quick/quick-design-m2-combat-camp-loop.md).
- ✅ Sprint 2 M2 story slate opened: `S2-M2-01` through `S2-M2-04`. `S2-M2-01` is complete; `S2-M2-02` is ready for `/story-readiness`; `S2-M2-03` through `S2-M2-04` remain blocked in dependency order.
- ✅ Sprint 2 story `S2-M2-01` closed via `/story-done` 2026-05-11 with verdict **COMPLETE WITH NOTES**. Commit `b4cb377` lands the Unity Combat Core runtime bridge.
- ✅ Sprint 2 story `S2-M2-02` closed via `/story-done` 2026-05-12 with verdict **COMPLETE WITH NOTES**. Commits `93b460e` (implementation) -> `946c9d1` (harden / closes /code-review P0/P1) -> `350b06e` (evidence-integrity patch / closes qa-tester re-review). Three carryover keys logged: `m2_renderer_material_property_access`, `s2_bridge_runner_evidence_path_hardcoded`, `m2_presentation_threshold_gap`.
- ✅ Sprint 2 story `S2-M2-03` closed via `/story-done` 2026-05-13 with verdict **COMPLETE WITH NOTES** (5/5 AC); implementation `bb6deab`, closure `3ce334c`; `control_manifest_absence_pre_existing` carryover added to sprint-status; S2-M2-03-cycle lessons captured 2026-05-14 in `fc20a86` (4 entries — telemetry-backed feel acceptance, classified negative-scope doc hits, Unity log redaction, MaterialPropertyBlock lifecycle-safe allocation; PowerShell heredoc whitespace-collapse lesson dropped, no durable repo evidence).
- ⚠️ Unity ProjectSettings hygiene is deferred to a separate batch: `ProjectSettings/TagManager.asset` emits a non-fatal parse warning in committed S2-M2-01 smoke logs, and `ProjectSettings/URPProjectSettings.asset` is currently untracked after Unity generation.

## Files Being Worked On

- **Active:** `S2-M2-03` is complete. Next command is `/dev-story production/stories/s2-m2-04-named-blocker-camp-boundary.md`.
- **Next story:** [production/stories/s2-m2-04-named-blocker-camp-boundary.md](../stories/s2-m2-04-named-blocker-camp-boundary.md).
- **Closed:** [production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md](../stories/s2-m2-01-unity-combat-core-runtime-bridge.md) is complete.
- **Closed:** [production/stories/s2-combat-01-fix-init-only-property-preservation.md](../stories/s2-combat-01-fix-init-only-property-preservation.md) is complete.
- **Closed:** [production/stories/s2-foundation-01-unity-project-shell.md](../stories/s2-foundation-01-unity-project-shell.md) is complete.
- Combat Feel Prototype: [prototypes/combat-feel/](../../prototypes/combat-feel/) — pinned-engine headline pass complete; prototype code remains throwaway evidence artifact.
- Combat Feel Prototype README: [prototypes/combat-feel/README.md](../../prototypes/combat-feel/README.md)
- Inventory & Item Economy: [design/gdd/inventory-item-economy.md](../../design/gdd/inventory-item-economy.md)
- Character Progression: [design/gdd/character-progression.md](../../design/gdd/character-progression.md)
- World Structure: [design/gdd/world-structure.md](../../design/gdd/world-structure.md)
- Game concept: [design/gdd/game-concept.md](../../design/gdd/game-concept.md)
- Art bible: [design/art/art-bible.md](../../design/art/art-bible.md)
- Systems index: [design/gdd/systems-index.md](../../design/gdd/systems-index.md)
- CLAUDE.md (Technology Stack + Engine Version Reference)
- .claude/docs/technical-preferences.md (Unity 6.3 LTS + C# + URP + full specialist routing)
- docs/engine-reference/unity/VERSION.md (updated with post-May-2025 API gaps)
- .claude/agents/unity-specialist.md (Version Awareness section added)
- DECISIONS.md (D006 appended)

## Key Decisions Made

- **Engine**: Unity 6.3 LTS with C# (decided during /setup-engine) — FishNet planned for Tier 2+ netcode
- **Review mode**: Lean — director-gates skipped this session; targeted `/code-review 960a148` ran before closure and its provenance finding was fixed by `caea662`.
- **Visual identity locked**: "Every visual element earns its place through weight and age, not spectacle" + 3 supporting principles
- **Two-layer UI**: Layer 1 abstract practical HUD + Layer 2 fully diegetic world information
- T1-COMBAT-09c closed the player death payload stub and first held-policy feel-review artifact at `production/qa/combat/feel-review-09c-player-death.md`; human qualitative death-moment playtest remains pending as a Sprint 1.5 carryover.
- T1-COMBAT-10 closed the profiled evidence loop and surfaced quantitative combat-feel gaps.
- T1-COMBAT-11 is closed; Sprint 1.5 Must Have stories are 7/7 done.
- `T1.5-COMBAT-05` is closed; Sprint 1.5 Must Have stories are 7/7 complete.
- `AbilityResolvedEvent` remains `ManaSpent`-only after the physical resource split.
- ADR-0003 / D009 status metadata cleanup is closed.
- `H-CCOM-F2B` fixture extremes are validated by `T1-COMBAT-01`; seeded melee formula execution is validated by `T1-COMBAT-04`.
- Creature / Enemy AI still owns actual return-to-anchor movement and NavMeshAgent behavior.
- Inventory & Item Economy is intentionally parked after review.
- Sprint 2 / T1 playable anchor: **Gravenspire T1: The First District** - a 20-30 minute offline slice that makes the first 10 minutes of playable Gravenspire better.
- Sprint 2 lock: Cleric-only playable archetype; multiplayer, live LLM dialogue, extra classes, huge world, and deep economy remain cut.
- Sprint 2 development loop: implement one small feature, play it immediately, write down what felt bad, fix the worst thing, commit, repeat.
- Save/Load metadata drift is known: `save-load-persistence.md` header says `In Design`, while its review log says APPROVED. Do not silently correct outside an approved metadata cleanup batch.

## Next Skill to Run

- `/code-review` on the S2-M2-04 changed files, then `/story-done production/stories/s2-m2-04-named-blocker-camp-boundary.md`. `/dev-story` ran 2026-05-14 and implemented S2-M2-04; the story header is flipped to In Progress and `production/sprint-status.yaml` stays blocked until `/story-done`. Changed files: `Assets/Editor/GravenspireM2SingleTrashLoopBuilder.cs`, `Assets/Scripts/M2SingleTrashMedLoopController.cs`, `Assets/Editor/GravenspireM2NamedBlockerVerificationRunner.cs` (+`.meta`), `Assets/Scenes/_DevEntry.unity`, `tests/integration/gameplay/combat/combat_runtime_named_blocker_boundary_test.cs`, `tests/evidence/S2-M2-04/**`.
- Routing decision (2026-05-14) on the M2-02 presentation-threshold revisit trigger (`m2_presentation_threshold_gap`): option **(a) — accept qualified human-play findings** — with a binding constraint. S2-M2-04 verification must mechanically prove the named blocker changes camp behavior through telemetry (discovery, time-to-danger, boundary pressure, clean-loop preservation, no farm-through), the way S2-M2-03 proved danger via `ending_health=14/140`. **Status: satisfied by `/dev-story` 2026-05-14** — telemetry + the dotnet integration test mechanically prove all five dimensions; human-play stays a qualified supplement. Human-play notes may be qualified by blockout visuals; mechanical AC cannot rest on presentation feel. Captured durably as lesson A in `tasks/lessons.md` (`fc20a86`).
- Later: run Inventory implementation pre-spec to close `INV-OQ-05`, then rerun `/design-review design/gdd/inventory-item-economy.md --depth full`.

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
