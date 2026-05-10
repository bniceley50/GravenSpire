# Active Session State

**Last Updated:** 2026-05-09
**Project Stage:** Pre-Production — Sprint 2 — Active Routing / QA Planning

## Current Task

Sprint 1.5 close-out gates are complete 3/3: `/smoke-check sprint` PASS WITH WARNINGS, `/team-qa sprint` APPROVED WITH CONDITIONS, and `/gate-check` PASS. Sprint 2 is now rolling forward to **Gravenspire T1: The First District**, a 20-30 minute offline playable slice.

Next recommended: open `S2-FOUNDATION-01` as the Unity project shell story, then run Sprint 2 `/qa-plan sprint` before any new feature implementation.

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
- ✅ Sprint 2 first implementation constraint: production Unity shell is absent, so `S2-FOUNDATION-01` must open before launch/menu/session smoke can pass.

## Files Being Worked On

- **Active:** Sprint 2 First District alignment landed in [production/sprints/sprint-2.md](../sprints/sprint-2.md) (5-milestone structure) and [production/sprint-status.yaml](../sprint-status.yaml) (rolled to Sprint 2 at `aa785a0`). Next milestone gate: open `S2-FOUNDATION-01`.
- **Next story to open:** `S2-FOUNDATION-01` Unity project shell.
- **Closed:** [production/stories/s2-combat-01-fix-init-only-property-preservation.md](../stories/s2-combat-01-fix-init-only-property-preservation.md) is complete.
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

- **Open story:** `S2-FOUNDATION-01` Unity project shell.
- Then run Sprint 2 `/qa-plan sprint`.
- Later: run Inventory implementation pre-spec to close `INV-OQ-05`, then rerun `/design-review design/gdd/inventory-item-economy.md --depth full`.

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
