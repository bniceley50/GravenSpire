# Active Session State

**Last Updated:** 2026-05-10
**Project Stage:** Pre-Production — Sprint 2 — M2 Stories Open / S2-M2-01 Next

## Current Task

Sprint 1.5 close-out gates are complete 3/3: `/smoke-check sprint` PASS WITH WARNINGS, `/team-qa sprint` APPROVED WITH CONDITIONS, and `/gate-check` PASS. Sprint 2 is now rolling forward to **Gravenspire T1: The First District**, a 20-30 minute offline playable slice.

S2-FOUNDATION-01 Unity launch verification is complete. Manual Play Mode verification and the Unity CLI runner both confirmed `_DevEntry.unity` loads, renders, and remains stable for 30 seconds with no captured errors.

M2 Combat Camp Loop quick design is complete, and the four-story M2 slate is open. Next action: `/story-readiness production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md`.

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
- ✅ Sprint 2 M2 story slate opened: `S2-M2-01` through `S2-M2-04`. `S2-M2-01` is ready for `/story-readiness`; `S2-M2-02` through `S2-M2-04` are blocked in dependency order.

## Files Being Worked On

- **Active:** Sprint 2 M2 Combat Camp Loop story slate is open. Next command is `/story-readiness production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md`.
- **Next story:** [production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md](../stories/s2-m2-01-unity-combat-core-runtime-bridge.md).
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

- `/story-readiness production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md`.
- After readiness passes, run `/dev-story production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md`.
- Then proceed in order: `S2-M2-02` -> `S2-M2-03` -> `S2-M2-04`.
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
