# Active Session State

**Last Updated:** 2026-05-07
**Project Stage:** Pre-Production — Sprint 1.5 — Implementation

## Current Task

Sprint 1.5 is active in implementation. `T1.5-COMBAT-02` closed via `/story-done` with verdict **COMPLETE WITH NOTES**, landing physical-instant Endurance conversion and validating ADR-0006's resource split.

The next critical-path story is `/dev-story T1.5-COMBAT-03-feel-03-overpull-tuning`. `T1.5-COMBAT-04` remains unblocked for optional parallel design work.

## Status

- ✓ Brainstorm complete → [design/gdd/game-concept.md](../../design/gdd/game-concept.md)
- ✓ Engine configured → Unity 6.3 LTS + C# + URP (FishNet planned for Tier 2+)
- ✓ Art bible complete → [design/art/art-bible.md](../../design/art/art-bible.md) (9/9 sections, Lean mode, AD-ART-BIBLE skipped)
- ✓ Systems mapped → [design/gdd/systems-index.md](../../design/gdd/systems-index.md) (33 systems, 26 MVP)
- ✓ Spelling normalized Gravespire → Gravenspire, AGENTS.md §0 canonical path fixed (commit `486f0a0`)
- ✓ D006 committed — Codex onboarded as parallel implementer (commit `fae8c8c`)
- ✓ Codex onboarding brief delivered; Codex has completed read-only onboarding and surfaced 4 clarifying questions (all answered)
- ✓ Codex Assignment #1 drafted — `dotnet format` setup on branch `codex/dotnet-format-setup`
- ⚠️ World Structure GDD — **NEEDS REVISION.** Full `/design-review design/gdd/world-structure.md` completed 2026-04-23 with verdict **MAJOR REVISION NEEDED** and scope signal **XL**. Six blocker groups: (1) transition contract contradiction, (2) memory residency model contradiction, (3) save/zone identity contradiction, (4) formula/registry validator-safety issues, (5) acceptance criteria not T1-gateable, (6) T1 offline "world kept moving" bridge underspecified. Review log created at [design/gdd/reviews/world-structure-review-log.md](../../design/gdd/reviews/world-structure-review-log.md).
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
- ✅ Combat-feel advisory playtest completed 2026-04-26 under Unity `6000.4.1f1` standalone build: player reported the loop felt "pretty smooth" and saw casting time as tuneable, not a fundamental pacing rejection. Heal usability bug and standalone black-screen HUD issue were fixed; post-fix smoke passed 5/5 pulls with 22.7s average pull and 5 Heals. Report written to [production/prototypes/combat-feel-report.md](../prototypes/combat-feel-report.md). Verdict: **ADVISORY POSITIVE**, pending pinned Unity `6000.3.x` validation.
- 🧪 Combat-feel v2 tactical-instants iteration started 2026-04-26 after Read A confirmation. Scope: preserve auto-attack baseline and med breaks while testing Smite of Authority, Bash, and Defensive Prayer as non-spammable agency buttons. Manual melee spam remains out of scope unless a later pillar-level reframe is explicitly approved.
- ✅ Combat-feel v2 scripted smoke passed under Unity `6000.4.1f1`: 5/5 pulls, 80.9s combat, 37.9s downtime, 16.2s average pull, 1 med break, 5 Smites, 2 Heals, 8 Smite of Authority uses, 7 Bashes, 2 Defensive Prayers, 0 unsafe pulls. Added agency did not erase the med-break requirement in smoke.
- 🧪 Attack-toggle adjustment added after v2 playtest feedback: Pull starts combat but does not enable auto-swinging; player toggles Attack with `A`; Attack turns off on target death; Bash remains independent of Attack. This preserves auto-attack as an EQ-style toggle rather than spammed manual melee.
- ✅ Attack-toggle smoke passed under Unity `6000.4.1f1`: 5/5 pulls, 81.1s combat, 40.8s downtime, 16.2s average pull, 1 med break, 32 auto swings, 6 Smites, 2 Heals, 7 Smite of Authority uses, 6 Bashes, 2 Defensive Prayers, 0 unsafe pulls.
- ✅ Combat-feel v2 full advisory playtest completed 2026-04-26: player reported "you nailed it! felt really smooth"; med breaks still felt "very" necessary; Attack toggle solved the manual-melee instinct without spam. Verdict: **ADVISORY POSITIVE — stronger than v1 baseline; preferred T1 combat-feel direction**, pending pinned Unity `6000.3.x` validation.
- ✅ Combat-feel pinned Unity `6000.3.14f1` validation completed 2026-04-26. Disk-captured run `prototypes/combat-feel/Logs/playtest-20260426-204721.log`: 5/5 pulls, 24.507s average pull, 5 med breaks, 0 unsafe pulls, 0 deaths. Six criteria passed except Attack toggle feedback clarity; mechanic passed but ON state was not visually explicit enough.
- ✅ Attack ON highlight fix added in `PrototypeBootstrap.cs` and rerun on pinned Unity `6000.3.14f1`. Disk-captured run `prototypes/combat-feel/Logs/playtest-20260426-205508.log`: 5/5 pulls, 18.734s average pull, 5 med breaks, 0 unsafe pulls, 0 deaths. Player confirmed the highlight fixed clunky feel, Attack toggle now feels right, and everything else still felt smooth.
- ✅ **HEADLINE PASS:** T1 combat-feel is validated on pinned Unity 6.3 LTS. The game-concept risk "Does EQ-Classic combat still feel good in 2026?" is answered affirmatively at prototype-grade evidence level. Preferred T1 baseline: Classic-EQ tab-target discipline with tactical Cleric instants, player-controlled Attack toggle, and explicit Attack ON visual feedback.
- ✅ Combat Core D012 amendment APPROVED 2026-04-27 after full `/design-review design/gdd/combat-core.md --depth full`: six specialist passes, zero blockers, three non-blocking recommendations. D012's two gates are now satisfied: pinned-engine validation plus Combat Core revision/re-review. `/sprint-plan new` is unblocked for the T1 combat sprint.
- ✅ Codex PR #1 (`codex/dotnet-format-setup` → `main`) merged into `main`; follow-up cleanup commit `ce634c3` recorded.
- ✅ Sprint 1 story `T1-COMBAT-01` closed via `/story-done` 2026-04-28 with verdict **COMPLETE WITH NOTES**. Evidence: Stage 2 TRX 15/15 PASS, Stage 3 rerun TRX 15/15 PASS, and AC trace in [production/stories/t1-combat-01-cleric-base-combat-actor-fixture-hydration.md](../stories/t1-combat-01-cleric-base-combat-actor-fixture-hydration.md).
- ✅ Sprint 1 story `T1-COMBAT-02` closed via `/story-done` 2026-04-30 with verdict **COMPLETE WITH NOTES**. Evidence: Stage 2 TRX 27/27 PASS, Stage 3 rerun TRX 27/27 PASS, live closure rerun 27/27 PASS, negative T1 scope grep clean except the known README ban line, and AC trace in [production/stories/t1-combat-02-targeting-and-hostile-actor-claim.md](../stories/t1-combat-02-targeting-and-hostile-actor-claim.md). `production/sprint-status.yaml` now records 2/13 Sprint 1 stories done.
- ✅ Sprint 1 story `T1-COMBAT-03` closed via `/story-done` 2026-04-30 with verdict **COMPLETE WITH NOTES**. Evidence: Stage 2 TRX 43/43 PASS, live closure rerun 43/43 PASS, negative T1 scope grep clean with `src/networking` absent, and AC trace in [production/stories/t1-combat-03-attack-toggle-state-machine.md](../stories/t1-combat-03-attack-toggle-state-machine.md). `production/sprint-status.yaml` now records 3/13 Sprint 1 stories done.
- ✅ Sprint 1 story `T1-COMBAT-04` closed via `/story-done` 2026-04-30 with verdict **COMPLETE WITH NOTES**. Evidence: Stage 2 TRX 59/59 PASS, live closure rerun 59/59 PASS, negative T1 scope grep clean, and AC trace in [production/stories/t1-combat-04-melee-tick-weapon-delay-resolution.md](../stories/t1-combat-04-melee-tick-weapon-delay-resolution.md). `production/sprint-status.yaml` now records 4/13 Sprint 1 stories done.
- ✅ Sprint 1 story `T1-COMBAT-05` closed via `/story-done` 2026-04-30 with verdict **COMPLETE WITH NOTES**. Evidence: Stage 2 TRX 71/71 PASS, live closure rerun 71/71 PASS, negative T1 scope grep clean, and AC trace in [production/stories/t1-combat-05-slow-cast-framework.md](../stories/t1-combat-05-slow-cast-framework.md). ADR-0003/D009 metadata cleanup was closed in the same approved ride-along. `production/sprint-status.yaml` now records 5/13 Sprint 1 stories done.
- ✅ Sprint 1 story `T1-COMBAT-06` closed via `/story-done` 2026-04-30 with verdict **COMPLETE WITH NOTES**. Evidence: Stage 2 TRX 81/81 PASS, hardcoded-tuning gate PASS, negative T1 scope grep clean, and AC trace in [production/stories/t1-combat-06-tactical-cleric-instants-fixture-loaded-values.md](../stories/t1-combat-06-tactical-cleric-instants-fixture-loaded-values.md). `production/sprint-status.yaml` now records 6/13 Sprint 1 stories done.
- ✅ Sprint 1 story `T1-COMBAT-07` closed via `/story-done` 2026-04-30 with verdict **COMPLETE WITH NOTES**. Evidence: Stage 2 TRX 92/92 PASS, hardcoded-tuning gate PASS, negative T1 scope grep clean, composition verification clean, cast-and-sit policy documented, and AC trace in [production/stories/t1-combat-07-med-sit-regen-combat-exit-timing.md](../stories/t1-combat-07-med-sit-regen-combat-exit-timing.md). `production/sprint-status.yaml` now records 7/13 Sprint 1 stories done.
- ✅ Sprint 1 story `T1-COMBAT-08` closed via `/story-done` 2026-04-30 with verdict **COMPLETE WITH NOTES**. Evidence: Stage 2 TRX 106/106 PASS, UI seam guards held, raw numeric threat excluded from shipping HUD output, no misleading no-target Attack ON pulse, composition verification clean, and AC trace in [production/stories/t1-combat-08-attack-on-hud-state-signal-hookup.md](../stories/t1-combat-08-attack-on-hud-state-signal-hookup.md). `production/sprint-status.yaml` now records 8/13 Sprint 1 stories done.
- ✅ Sprint 1 story `T1-COMBAT-09b` closed via `/story-done` 2026-04-30 with verdict **COMPLETE WITH NOTES**. Evidence: Stage 2 TRX 124/124 PASS, frozen `PlayerKillCreditEvent` invariant held, Character Progression boundary scan clean, no-byte unresolved-barrier assertion held, prior 113-test regression check passed, and AC trace in [production/stories/t1-combat-09b-same-frame-save-barrier-kill-credit-consistency.md](../stories/t1-combat-09b-same-frame-save-barrier-kill-credit-consistency.md). ADR-0001/D007 and ADR-0002/D008 metadata cleanup was closed in the same approved ride-along. `production/sprint-status.yaml` now records 9/13 Sprint 1 stories done.
- ✅ Sprint 1 story `T1-COMBAT-09c` closed via `/story-done` 2026-04-30 with verdict **COMPLETE WITH NOTES**. Evidence: Stage 2 TRX 133/133 PASS, `PlayerKillCreditEvent` frozen-event invariant held, six-field `PlayerDeathEvent` schema verified, four-field `CombatPersistenceProjection` whitelist verified, Death & Corpse Recovery scope guards held, 09b save coordinator unchanged, and AC trace in [production/stories/t1-combat-09c-player-death-payload-stub-reserved-integration.md](../stories/t1-combat-09c-player-death-payload-stub-reserved-integration.md). The held-policy first artifact is now durable at [production/qa/combat/feel-review-09c-player-death.md](../qa/combat/feel-review-09c-player-death.md). `production/sprint-status.yaml` now records 10/13 Sprint 1 stories done.
- ✅ Sprint 1 story `T1-COMBAT-10` closed via `/story-done` 2026-05-05 with verdict **COMPLETE WITH NOTES**. Evidence: headless JSONL harness at [prototypes/combat-slice-T1/Harness/CombatSliceHarness.cs](../../prototypes/combat-slice-T1/Harness/CombatSliceHarness.cs), JSONL evidence at [tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl](../../tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl), verification summary at [tests/evidence/T1-COMBAT-10/verification.md](../../tests/evidence/T1-COMBAT-10/verification.md), and slice-review input at [production/qa/combat/t1-combat-10-profiled-evidence-summary.md](../qa/combat/t1-combat-10-profiled-evidence-summary.md). `H-CCOM-FEEL-01` failed-as-measured with `20/20` solo-trash wins vs. the `55-85%` target; `H-CCOM-FEEL-03` failed-as-measured with `5/10` dangerous two-trash outcomes vs. the `>=8/10` target. Production was not tuned. `production/sprint-status.yaml` now records 11/13 Sprint 1 stories done and surfaces slice review as the next gate.
- ✅ T1 combat slice review verdict committed 2026-05-06 at `4edf2f9`: **Yellow**. Architecture held; sprint-1.5 scope is combat-feel correction, including quiet Endurance, FEEL-03 overpull danger, and FEEL-01 target revalidation.
- ✅ Sprint 1.5 plan committed 2026-05-06 at `8885d2e`: [production/sprints/sprint-1-5.md](../sprints/sprint-1-5.md).
- ✅ Sprint 1.5 QA plan committed 2026-05-06 at `b6297b4`: [production/qa/plans/qa-plan-sprint-1-5-20260506.md](../qa/plans/qa-plan-sprint-1-5-20260506.md). Required next gate is baseline regression before `/dev-story T1.5-COMBAT-00-endurance-contract-lock`.
- ✅ Sprint 1.5 story `T1.5-COMBAT-00` closed via `/story-done` 2026-05-06 with verdict **COMPLETE WITH NOTES**. Commit `c2487fc` lands D013 and ADR-0006 as Proposed; the status ride-along to D013 Locked / ADR-0006 Accepted is scheduled for `T1.5-COMBAT-02` closure after physical instant conversion validates the contract.
- ✅ Sprint 1.5 story `T1.5-COMBAT-01` closed via `/story-done` 2026-05-07 with verdict **COMPLETE WITH NOTES**. Commit `d6c8e08` lands Endurance state, combat persistence projection expanded to five fields, and categorical `CombatHudEnduranceCategory` HUD signaling; production source is unfrozen for this story and `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` passes 139/139.
- ✅ Sprint 1.5 story `T1.5-COMBAT-02` closed via `/story-done` 2026-05-07 with verdict **COMPLETE WITH NOTES**. Commit `9aacee0` lands Bash Endurance spend, Smite/Defensive Prayer mana carveouts, resource_kind/cost_endurance fixture schema support across both tactical instant surfaces, all-band Cleric Endurance hydration, 7 QA-02 cases, and 2 blocker regression tests; `dotnet test tests\Gravenspire.Combat.Tests.csproj` passes 148/148. ADR-0006 is now Accepted and D013 is now Locked.

## Files Being Worked On

- **Active:** Sprint 1.5 implementation. Next critical path: `/dev-story T1.5-COMBAT-03-feel-03-overpull-tuning`. Optional parallel design work: `/dev-story T1.5-COMBAT-04-feel-01-target-revalidation`.
- Combat Feel Prototype: [prototypes/combat-feel/](../../prototypes/combat-feel/) — pinned-engine headline pass complete; prototype code remains throwaway evidence artifact.
- Combat Feel Prototype README: [prototypes/combat-feel/README.md](../../prototypes/combat-feel/README.md) — prototype question, success/failure criteria, loop spec, controls, run notes
- Inventory & Item Economy: [design/gdd/inventory-item-economy.md](../../design/gdd/inventory-item-economy.md) — design draft, **NEEDS REVISION**, blocker 1 repaired, remaining blockers tracked in `INV-OQ-05`
- Character Progression: [design/gdd/character-progression.md](../../design/gdd/character-progression.md) — APPROVED 2026-04-26; commits `6459ceb` and `557fa3e`
- World Structure: [design/gdd/world-structure.md](../../design/gdd/world-structure.md)
- Game concept: [design/gdd/game-concept.md](../../design/gdd/game-concept.md)
- Art bible: [design/art/art-bible.md](../../design/art/art-bible.md) — hard constraints for this GDD (§8.7 zone=Addressable group, §8.9 ≤350MB, §4.5 no zone-boundary LUT, §6.2 ≤3-4 unique 2K surfaces/group)
- Systems index: [design/gdd/systems-index.md](../../design/gdd/systems-index.md)
- CLAUDE.md (Technology Stack + Engine Version Reference)
- .claude/docs/technical-preferences.md (Unity 6.3 LTS + C# + URP + full specialist routing)
- docs/engine-reference/unity/VERSION.md (updated with post-May-2025 API gaps)
- .claude/agents/unity-specialist.md (Version Awareness section added)
- DECISIONS.md (D006 appended)

## Key Decisions Made

- **Engine**: Unity 6.3 LTS with C# (decided during /setup-engine) — FishNet planned for Tier 2+ netcode
- **Review mode**: Lean — director-gates (CD-PILLARS, AD-CONCEPT-VISUAL, AD-ART-BIBLE, TD-FEASIBILITY, TD-SYSTEM-BOUNDARY, PR-SCOPE, CD-SYSTEMS) skipped this session
- **Visual identity locked**: "Every visual element earns its place through weight and age, not spectacle" + 3 supporting principles (Stillness Is The Signal / Faction Before Fantasy / The Beautiful And The Wrong)
- **Two-layer UI**: Layer 1 abstract practical HUD + Layer 2 fully diegetic world information (faction board, dialogue panels on faction-specific paper, personal journal)
- **Onboarding**: Sister Elara AI companion mentors new players by behavior — no tutorial text. MVP-critical system.
- **Scope staging**: 4 tiers. Tier 1 MVP = single-player vertical slice (Cleric + 1 haunt + 1 faction + 1 city hub). Tier 2 = co-op alpha. Tier 3 = persistent small-server beta. Tier 4 = full vision.
- **Codex onboarded** (D006, 2026-04-22) — parallel implementer, own worktree `N:\GravenSpire-codex`, branch-scoped write authority, PR flow to `main`. Forbidden zones: `design/gdd/**`, art bible, DECISIONS.md, AGENTS.md, CLAUDE.md, engine-reference, `.claude/**`.

## Open Questions / Flags

- Technical-artist validation items accumulated in art bible (URP SSS cost model, decal projector perf, camera-stack isolation for corpse-run desat, GPU instancing behavior, etc.) — consolidated in art-bible.md Document Status header
- T1-COMBAT-07 now provides the Combat Core med-break regen tick contract; Layer 1 HUD must consume Combat Core state rather than inventing separate fill timing.
- T1-COMBAT-08 now provides the Combat Core HUD-safe projection seam; Layer 1 HUD must consume categorical threat and explicit Attack ON/OFF state rather than exposing raw threat or inventing parallel combat state.
- T1-COMBAT-09c closed the player death payload stub and first held-policy feel-review artifact at `production/qa/combat/feel-review-09c-player-death.md`; human qualitative death-moment playtest remains pending as a Sprint 1.5 carryover.
- T1-COMBAT-10 closed the profiled evidence loop and surfaced quantitative combat-feel gaps: solo trash is too safe (`20/20` wins) and two-trash overpull is insufficiently punishing (`5/10` dangerous outcomes). Treat these as slice-review inputs, not as bugs fixed in T1-COMBAT-10.
- T1.5-COMBAT-02 is closed; `production/sprint-status.yaml` now records Sprint 1.5 progress as 3/7 done and surfaces `T1.5-COMBAT-03` as the next active story.
- `AbilityResolvedEvent` remains `ManaSpent`-only after the physical resource split; carry this semantic gap to `T1-COMBAT-11` as a forbidden-pattern/static-scan input.
- T1.5-COMBAT-02 `/code-review` P3s are intentionally deferred: `fixtureSetVersion` bump belongs in `T1.5-COMBAT-03`, and QA-02-01 "cooldown/global recovery" wording belongs in the next QA plan iteration.
- `production/stories/t1-combat-11-forbidden-pattern-compliance-scan-analyzer.md` is referenced by Sprint 1 and Sprint 1.5 planning but is currently absent; create or recover it before T1-COMBAT-11 implementation.
- ADR-0003 / D009 status metadata cleanup is closed as of the T1-COMBAT-05 `/story-done` ride-along: ADR-0003 is `Accepted`, and DECISIONS.md D009 is `Locked`. Justification: T1-COMBAT-01 closure commit `565ee26` has been on `main` since 2026-04-28; `CombatProgressionBaselineSnapshot` is consumed by production `CombatActorHydrator` at `src/gameplay/combat/CombatActorHydrator.cs:55`, `src/gameplay/combat/CombatActorHydrator.cs:61-68`, and `src/gameplay/combat/CombatActorHydrator.cs:104-126`; T1-COMBAT-01 verification cites ADR-0003 coverage at `tests/evidence/T1-COMBAT-01/verification.md:31`.
- `H-CCOM-F2B` fixture extremes are validated by `T1-COMBAT-01`; seeded melee formula execution is validated by `T1-COMBAT-04`.
- Creature / Enemy AI still owns actual return-to-anchor movement and NavMeshAgent behavior; `T1-COMBAT-02` supplies Combat Core leash hooks and test doubles only.
- Optional deferred lesson capture remains open for the `[GLOBAL][CI]` `git diff --check` line-ending warning pattern if it repeats outside `T1-COMBAT-02`.
- Remaining game-concept project-killer risk: `design/gdd/game-concept.md:304` asks whether kills shifting faction control feel meaningful at the five-minute scale. Needs a later separate `/prototype faction-feel`.
- Zone definition = Addressable streaming group boundary; `unity-addressables-specialist` must configure streaming groups
- 26 MVP systems is large scope for a solo first-time dev — every design decision should keep asking "is this actually needed for Tier 1?"
- Inventory & Item Economy is intentionally parked after review; do not implement it until an Inventory implementation pre-spec closes `INV-OQ-05` and a fresh full review passes.

## Next Skill to Run

- **Run `/dev-story T1.5-COMBAT-03-feel-03-overpull-tuning` next.**
- Optional parallel work: `/dev-story T1.5-COMBAT-04-feel-01-target-revalidation`.
- Later: run Inventory implementation pre-spec to close `INV-OQ-05`, then rerun `/design-review design/gdd/inventory-item-economy.md --depth full`.
- Codex PR #1 is merged; no Codex follow-up pending in this active state file.

## Session Extract - Sprint 1 Closeout / Sprint 1.5 Planning 2026-05-06

- Slice review: [production/qa/combat/feel-review-T1-slice.md](../qa/combat/feel-review-T1-slice.md) committed at `4edf2f9` with Brian's verdict **Yellow**.
- Sprint 1.5 plan: [production/sprints/sprint-1-5.md](../sprints/sprint-1-5.md) committed at `8885d2e`.
- Sprint 1.5 QA plan: [production/qa/plans/qa-plan-sprint-1-5-20260506.md](../qa/plans/qa-plan-sprint-1-5-20260506.md) committed at `b6297b4`.
- Sprint 1.5 scope: quiet Endurance resource-model addition, Bash/physical instant conversion, FEEL-03 overpull tuning, FEEL-01 target revalidation, T1-COMBAT-11 carryover, and profiled rerun evidence.
- Carryover: `production/stories/t1-combat-11-forbidden-pattern-compliance-scan-analyzer.md` is currently absent and must be created or recovered before T1-COMBAT-11 implementation; [production/qa/combat/feel-review-09c-player-death.md](../qa/combat/feel-review-09c-player-death.md) still has HUMAN PLAYTEST PENDING.
- State update: `production/sprint-status.yaml` and this active state now point at baseline regression as the required next gate, then `/dev-story T1.5-COMBAT-00-endurance-contract-lock`.

## Session Extract - /story-done 2026-05-06 (T1.5-COMBAT-00)

- Story: [production/stories/t1-5-combat-00-endurance-contract-lock.md](../stories/t1-5-combat-00-endurance-contract-lock.md) - T1.5-COMBAT-00 Endurance Contract Lock.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 5/5 covered; `QA-00-01`, `QA-00-02`, `QA-00-03`, `QA-00-04`, and `QA-00-05` all have file:line evidence in the story AC trace.
- Evidence: [tests/evidence/T1.5-COMBAT-00/verification.md](../../tests/evidence/T1.5-COMBAT-00/verification.md) records D013 Proposed, ADR-0006 Proposed, carveout naming, banned-pattern enumeration, empty no-code diff, 133/133 baseline regression, and hook smoke.
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1.5-COMBAT-00` done, records 1/7 Sprint 1.5 stories done, and surfaces `T1.5-COMBAT-01` as next active. `T1.5-COMBAT-04` is also unblocked for optional parallel design work.
- ADR ride-along marker: ADR-0006 / D013 status ride-along is scheduled at `T1.5-COMBAT-02` closure after physical instant conversion validates the contract. That closure batch should include both metadata flips as narrow status-only changes: ADR-0006 Proposed -> Accepted and D013 Proposed -> Locked, mirroring the T1-COMBAT-09b ride-along precedent for D007/D008.
- Tech debt logged: None.
- Carried forward: `T1-COMBAT-11` story file is still absent and must be created or recovered before that story's `/dev-story`; [production/qa/combat/feel-review-09c-player-death.md](../qa/combat/feel-review-09c-player-death.md) remains HUMAN PLAYTEST PENDING.
- Next recommended: `/dev-story T1.5-COMBAT-01-endurance-state-hud-save-projection`.

## Session Extract - /story-done 2026-05-07 (T1.5-COMBAT-01)

- Story: [production/stories/t1-5-combat-01-endurance-state-hud-save-projection.md](../stories/t1-5-combat-01-endurance-state-hud-save-projection.md) - T1.5-COMBAT-01 Endurance State, HUD, Save Projection.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 6/6 covered; `QA-01-01`, `QA-01-02`, `QA-01-03`, `QA-01-04`, `QA-01-05`, and `QA-01-06` all have file:line evidence in the story AC trace and completion notes.
- Evidence: [tests/evidence/T1.5-COMBAT-01/verification.md](../../tests/evidence/T1.5-COMBAT-01/verification.md) records the 139/139 regression pass, six QA PASS rows, frozen event/baseline checks, quiet HUD/UI checks, T1 negative-scope pass, prior 133-test continuity, and final hygiene proof. TRX counter is at [tests/evidence/T1.5-COMBAT-01/t1-5-combat-01-stage2.trx](../../tests/evidence/T1.5-COMBAT-01/t1-5-combat-01-stage2.trx).
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1.5-COMBAT-01` done, records 2/7 Sprint 1.5 stories done, corrects the `T1.5-COMBAT-01` row file path to the actual story artifact, and surfaces `T1.5-COMBAT-02` as next active. `T1.5-COMBAT-04` remains unblocked for optional parallel design work.
- ADR ride-along marker: ADR-0006 / D013 status ride-along remains scheduled at `T1.5-COMBAT-02` closure after physical instant conversion validates the resource split. The chain is recorded in the prior `T1.5-COMBAT-00` closure extract from commit `5e59344` and this story's Completion Notes.
- Tech debt logged: sprint-status routing drift was corrected for `T1.5-COMBAT-01`; no other story-row path drift was fixed in this batch.
- Carried forward: `T1-COMBAT-11` story file is still absent and must be created or recovered before that story's `/dev-story`; [production/qa/combat/feel-review-09c-player-death.md](../qa/combat/feel-review-09c-player-death.md) remains HUMAN PLAYTEST PENDING.
- Next recommended: `/dev-story T1.5-COMBAT-02-physical-instant-conversion`.

## Session Extract - /story-done 2026-05-07 (T1.5-COMBAT-02)

- Story: [production/stories/t1-5-combat-02-physical-instant-conversion.md](../stories/t1-5-combat-02-physical-instant-conversion.md) - T1.5-COMBAT-02 Physical Instant Conversion.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 7/7 QA-02 cases covered; `QA-02-01` through `QA-02-07` all have file:line evidence in the story AC trace and verification summary. Two additional blocker regression tests cover the legacy `tacticalInstantFixtures` resource split and all-band Cleric Endurance hydration.
- Evidence: [tests/evidence/T1.5-COMBAT-02/verification.md](../../tests/evidence/T1.5-COMBAT-02/verification.md) records the 148/148 regression pass, seven QA PASS rows, frozen contract checks, T1 negative-scope pass, pre-commit pass, and subagent review notes. TRX counter is at [tests/evidence/T1.5-COMBAT-02/t1-5-combat-02-stage2.trx](../../tests/evidence/T1.5-COMBAT-02/t1-5-combat-02-stage2.trx).
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1.5-COMBAT-02` done, records 3/7 Sprint 1.5 stories done, and surfaces `T1.5-COMBAT-03` as next active. `T1.5-COMBAT-04` remains unblocked for optional parallel design work.
- ADR ride-along: ADR-0006 status changed from Proposed to Accepted; DECISIONS.md D013 status changed from Proposed to Locked. This is metadata-only and validated by T1.5-COMBAT-01 closure commit `d6c8e08`, T1.5-COMBAT-02 implementation commit `9aacee0`, ADR-0006's validation-state authorization, and the T1.5-COMBAT-02 verification artifact.
- Tech debt logged: None in `docs/tech-debt-register.md`; P3 review findings are deferred in sprint/session carryover instead of smuggled into closure.
- Carried forward: `AbilityResolvedEvent.ManaSpent`-only payload semantics for `T1-COMBAT-11`; `fixtureSetVersion` bump for `T1.5-COMBAT-03`; QA-02-01 cooldown/global-recovery wording for the next QA plan iteration; `T1-COMBAT-11` story file remains absent and must be created or recovered before that story's `/dev-story`; [production/qa/combat/feel-review-09c-player-death.md](../qa/combat/feel-review-09c-player-death.md) remains HUMAN PLAYTEST PENDING.
- Next recommended: `/dev-story T1.5-COMBAT-03-feel-03-overpull-tuning`.

## Session Extract - /story-done 2026-04-28

- Story: [production/stories/t1-combat-01-cleric-base-combat-actor-fixture-hydration.md](../stories/t1-combat-01-cleric-base-combat-actor-fixture-hydration.md) - T1-COMBAT-01 Cleric Base Combat Actor + Fixture Hydration.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 6/6 passing; `H-CCOM-F2B` passes for fixture coverage with formula execution deferred to `T1-COMBAT-04`; ADR-0003 snapshot contract passes with metadata status cleanup carried forward.
- Evidence: `tests/evidence/T1-COMBAT-01/t1-combat-01-stage2.trx` 15/15 PASS; `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx` 15/15 PASS; verification summary at [tests/evidence/T1-COMBAT-01/verification.md](../../tests/evidence/T1-COMBAT-01/verification.md).
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1-COMBAT-01` done and surfaces `T1-COMBAT-02` as next active.
- Tech debt logged: None.
- Carried forward at the time: ADR-0003 / D009 status metadata cleanup (closed 2026-04-30 in the T1-COMBAT-05 closure ride-along); `H-CCOM-F2B` seeded melee formula execution in `T1-COMBAT-04`.
- Next recommended: `/dev-story T1-COMBAT-02-targeting-and-hostile-actor-claim`.

## Session Extract - /story-done 2026-04-30

- Story: [production/stories/t1-combat-02-targeting-and-hostile-actor-claim.md](../stories/t1-combat-02-targeting-and-hostile-actor-claim.md) - T1-COMBAT-02 Targeting and Hostile Actor Claim.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 11/11 passing; `H-CCOM-WS-01`, `H-CCOM-WS-02`, `H-CCOM-WS-03`, `H-CCOM-TGT-01`, `H-CCOM-PULL-01`, `H-CCOM-PULL-02`, `H-CCOM-PULL-03`, `H-CCOM-PULL-04`, `H-CCOM-LEASH-01`, `H-CCOM-LEASH-02`, and `H-CCOM-ART-01` all have file:line evidence in the story AC trace.
- Evidence: `tests/evidence/T1-COMBAT-02/t1-combat-02-stage2.trx` 27/27 PASS; `tests/evidence/T1-COMBAT-02/t1-combat-02-stage3-rerun.trx` 27/27 PASS; verification summary at [tests/evidence/T1-COMBAT-02/verification.md](../../tests/evidence/T1-COMBAT-02/verification.md); live closure rerun passed 27/27 with `dotnet test tests\Gravenspire.Combat.Tests.csproj --no-restore --logger "console;verbosity=minimal"`.
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1-COMBAT-02` done, records 2/13 Sprint 1 stories done, and surfaces `T1-COMBAT-03` as next active.
- Tech debt logged: None.
- Carried forward at the time: ADR-0003 / D009 status metadata cleanup (closed 2026-04-30 in the T1-COMBAT-05 closure ride-along); `H-CCOM-F2B` seeded melee formula execution in `T1-COMBAT-04`; Creature / Enemy AI return-to-anchor/NavMeshAgent movement implementation; optional deferred `[GLOBAL][CI] git diff --check` lesson capture.
- Next recommended: `/dev-story T1-COMBAT-03-attack-toggle-state-machine`.

## Session Extract - /story-done 2026-04-30 (T1-COMBAT-03)

- Story: [production/stories/t1-combat-03-attack-toggle-state-machine.md](../stories/t1-combat-03-attack-toggle-state-machine.md) - T1-COMBAT-03 Attack Toggle State Machine.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 4/4 passing; `H-CCOM-AA-01`, `H-CCOM-AA-03`, `H-CCOM-MED-01` edge precondition, and `H-CCOM-HUD-04` edge precondition all have file:line evidence in the story AC trace.
- Evidence: `tests/evidence/T1-COMBAT-03/t1-combat-03-stage2.trx:276` 43/43 PASS; verification summary at [tests/evidence/T1-COMBAT-03/verification.md](../../tests/evidence/T1-COMBAT-03/verification.md); live closure rerun passed 43/43 with `dotnet test tests\Gravenspire.Combat.Tests.csproj --no-restore --logger "console;verbosity=minimal"`.
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1-COMBAT-03` done, records 3/13 Sprint 1 stories done, and surfaces `T1-COMBAT-04` as next active.
- Tech debt logged: None.
- Carried forward at the time: ADR-0003 / D009 status metadata cleanup (closed 2026-04-30 in the T1-COMBAT-05 closure ride-along); `H-CCOM-F2B` seeded melee formula execution in `T1-COMBAT-04`; melee resolution, cast lifecycle, tactical instant execution, med regen math, HUD presentation, kill credit, save barriers, death payloads, profiled feel evidence, and architecture scan tooling remain owned by later Sprint 1 stories.
- Next recommended: `/dev-story T1-COMBAT-04-melee-tick-weapon-delay-resolution`.

## Session Extract - /story-done 2026-04-30 (T1-COMBAT-04)

- Story: [production/stories/t1-combat-04-melee-tick-weapon-delay-resolution.md](../stories/t1-combat-04-melee-tick-weapon-delay-resolution.md) - T1-COMBAT-04 Melee Tick / Weapon-Delay Resolution.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 8/8 passing; `H-CCOM-TICK-01`, `H-CCOM-PAUSE-01`, `H-CCOM-AA-02`, `H-CCOM-F1`, `H-CCOM-F2`, seeded `H-CCOM-F2B` formula execution, same-tick death-before-swing priority, and per-tick eligibility validation all have file:line evidence in the story AC trace.
- Evidence: `tests/evidence/T1-COMBAT-04/t1-combat-04-stage2.trx:372` 59/59 PASS; verification summary at [tests/evidence/T1-COMBAT-04/verification.md](../../tests/evidence/T1-COMBAT-04/verification.md); live closure rerun passed 59/59 with `dotnet test tests\Gravenspire.Combat.Tests.csproj --no-restore --logger "console;verbosity=minimal"`.
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1-COMBAT-04` done, records 4/13 Sprint 1 stories done, and surfaces `T1-COMBAT-05` as next active.
- Tech debt logged: None.
- Carried forward at the time: ADR-0003 / D009 status metadata cleanup (closed 2026-04-30 in the T1-COMBAT-05 closure ride-along); cast lifecycle, tactical instant execution, med regen math, HUD presentation, kill credit, save barriers, death payloads, profiled feel evidence, and architecture scan tooling remain owned by later Sprint 1 stories.
- Next recommended: `/dev-story T1-COMBAT-05-slow-cast-framework`.

## Session Extract - /story-done 2026-04-30 (T1-COMBAT-05)

- Story: [production/stories/t1-combat-05-slow-cast-framework.md](../stories/t1-combat-05-slow-cast-framework.md) - T1-COMBAT-05 Slow Cast Framework.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 8/8 passing; `H-CCOM-CAST-01`, `H-CCOM-CAST-02`, `H-CCOM-CAST-03`, `H-CCOM-CAST-04`, `H-CCOM-CAST-05`, `H-CCOM-F4`, `H-CCOM-IF-01`, and same-tick completion-before-interrupt priority all have file:line evidence in the story AC trace.
- Evidence: `tests/evidence/T1-COMBAT-05/t1-combat-05-stage2.trx:444` 71/71 PASS; verification summary at [tests/evidence/T1-COMBAT-05/verification.md](../../tests/evidence/T1-COMBAT-05/verification.md); live closure rerun passed 71/71 with `dotnet test tests\Gravenspire.Combat.Tests.csproj --no-restore --logger "console;verbosity=minimal"`.
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1-COMBAT-05` done, records 5/13 Sprint 1 stories done, and surfaces `T1-COMBAT-06` as next active.
- ADR ride-along: ADR-0003 status changed from Proposed to Accepted; DECISIONS.md D009 status changed from Proposed to Locked. This is metadata-only and justified by T1-COMBAT-01 closure commit `565ee26`, `tests/evidence/T1-COMBAT-01/verification.md:31`, and production consumption in `src/gameplay/combat/CombatActorHydrator.cs:55`, `src/gameplay/combat/CombatActorHydrator.cs:61-68`, and `src/gameplay/combat/CombatActorHydrator.cs:104-126`. D007, D008, D010, and D011 remain Proposed pending their own validation gates.
- Tech debt logged: None.
- Carried forward: tactical instant execution (`T1-COMBAT-06`), med/sit regen and combat-exit timing, HUD presentation, kill credit, save barriers, death payloads, profiled feel evidence, architecture scan tooling, and the `.claude/skills/dev-story/SKILL.md:75` story-file-creation drift lesson candidate. ADR-0003/D009 metadata cleanup is no longer carried forward.
- Next recommended: `/dev-story T1-COMBAT-06-tactical-cleric-instants-fixture-loaded-values`.

## Session Extract - /story-done 2026-04-30 (T1-COMBAT-06)

- Story: [production/stories/t1-combat-06-tactical-cleric-instants-fixture-loaded-values.md](../stories/t1-combat-06-tactical-cleric-instants-fixture-loaded-values.md) - T1-COMBAT-06 Tactical Cleric Instants Using Fixture-Loaded Numeric Values.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 8/8 passing; `H-CCOM-INST-01`, `H-CCOM-FIXTURE-01`, instant ability resolution without a cast bar, Rule 13 mana spend path, transient cooldown timer behavior, Bash cancellation only through declared `interrupt_current_channel`, fixture-driven self-buff duration, and the static/grep rejection of hardcoded tactical instant tuning all have file:line evidence in the story AC trace.
- Evidence: `tests/evidence/T1-COMBAT-06/t1-combat-06-stage2.trx:504` 81/81 PASS; verification summary at [tests/evidence/T1-COMBAT-06/verification.md](../../tests/evidence/T1-COMBAT-06/verification.md); hardcoded-tuning gate passed at `tests/evidence/T1-COMBAT-06/verification.md:16-24` against the Sprint 1 T1-COMBAT-06 done-definition gate.
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1-COMBAT-06` done, records 6/13 Sprint 1 stories done, and surfaces `T1-COMBAT-07` as next active.
- Tech debt logged: None.
- Carried forward: med/sit regen and combat-exit timing (`T1-COMBAT-07`), HUD presentation, kill credit, save barriers, death payloads, profiled feel evidence, architecture scan tooling, and the `.claude/skills/dev-story/SKILL.md:75` story-file-creation drift lesson candidate remain owned by later Sprint 1 stories. Class Design still owns final tactical ability names/values beyond these prototype fixture rows.
- Next recommended: `/dev-story T1-COMBAT-07-med-sit-regen-combat-exit-timing`.

## Session Extract - /story-done 2026-04-30 (T1-COMBAT-07)

- Story: [production/stories/t1-combat-07-med-sit-regen-combat-exit-timing.md](../stories/t1-combat-07-med-sit-regen-combat-exit-timing.md) - T1-COMBAT-07 Med/sit Regen and Combat-exit Timing.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 6/6 story checks covered; `H-CCOM-MED-01`, `H-CCOM-MED-02`, `H-CCOM-MED-03`, `H-CCOM-F5`, the `H-CCOM-FEEL-04` prerequisite, and attack-off-before-regen sequencing all have file:line evidence in the story AC trace.
- Evidence: `tests/evidence/T1-COMBAT-07/t1-combat-07-stage2.trx:570` 92/92 PASS; verification summary at [tests/evidence/T1-COMBAT-07/verification.md](../../tests/evidence/T1-COMBAT-07/verification.md).
- Composition verification: sit-forces-Attack-off reuses existing `CombatAttackStateMachine.ForceOff` with no parallel attack state; sitting threat reuses existing `ThreatTable` / `AddThreat` with no parallel threat structure; timing uses `CombatTick` and `combat_tick_rate_hz` with no parallel clock.
- Cast-and-sit policy: manual sit during an active slow cast is rejected per Combat Core Rule 19 (`design/gdd/combat-core.md:174`), documented at `tests/evidence/T1-COMBAT-07/verification.md:81`, and tested at `tests/integration/gameplay/combat/combat_med_sit_regen_combat_exit_test.cs:126`.
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1-COMBAT-07` done, records 7/13 Sprint 1 stories done, and surfaces `T1-COMBAT-08` as next active.
- Tech debt logged: None.
- Carried forward: HUD presentation (`T1-COMBAT-08`), kill credit chain (`T1-COMBAT-09a`/`T1-COMBAT-09b`/`T1-COMBAT-09c`), profiled feel evidence (`T1-COMBAT-10`), architecture scan tooling (`T1-COMBAT-11`), and future forced/external sitting-as-interrupt behavior remain owned by later explicit stories.
- Next recommended: `/dev-story T1-COMBAT-08-attack-on-hud-state-signal-hookup`.

## Session Extract - /story-done 2026-04-30 (T1-COMBAT-08)

- Story: [production/stories/t1-combat-08-attack-on-hud-state-signal-hookup.md](../stories/t1-combat-08-attack-on-hud-state-signal-hookup.md) - T1-COMBAT-08 Attack ON HUD State Signal Hookup.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 4/4 story checks covered; `H-CCOM-HUD-01`, `H-CCOM-HUD-02`, `H-CCOM-HUD-03`, and `H-CCOM-HUD-04` all have file:line evidence in the story AC trace. `H-CCOM-HUD-04` coverage includes the 9-case table: Attack on signal, Attack off signal, target death, successful sit/med, combat exit, player death, zone transition, current-state accessor matching event history, and no no-target pulse.
- Evidence: `tests/evidence/T1-COMBAT-08/t1-combat-08-stage2.trx:654` 106/106 PASS; verification summary at [tests/evidence/T1-COMBAT-08/verification.md](../../tests/evidence/T1-COMBAT-08/verification.md).
- UI seam guards: held. `src/gameplay/combat/presentation/` has no UnityEngine, MonoBehaviour, UI Toolkit, UGUI, rendering, styling, layout, animation, color, image, sprite, canvas, or TextMeshPro dependency; HUD threat output is categorical only through `CombatHudThreatCategory`; the no-target no-op emits no misleading transient Attack ON pulse at `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:110`.
- Composition verification: `CombatHudStateProjection` reads existing `CombatActorState` resource/current-state, target/threat-table, `CombatAttackStateMachine` snapshot/signals, and `CombatCastStateMachine` state/progress surfaces only. Threat thresholds are supplied through the request DTO instead of hardcoded constants or fixture-loaded production coupling, opening a third architectural path for pure projection code that is neither gameplay tuning nor fixture-owned config.
- Footprint: strictly additive new-file-only implementation footprint in commit `5f5782cbb1bd04e8167a1f017c031cd1198dcb86`, the first such implementation footprint since `T1-COMBAT-03`.
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1-COMBAT-08` done, records 8/13 Sprint 1 stories done, and surfaces `T1-COMBAT-09a` as next active.
- Tech debt logged: None.
- Carried forward: kill-credit chain (`T1-COMBAT-09a`/`T1-COMBAT-09b`/`T1-COMBAT-09c`), save barriers (`T1-COMBAT-09b`), death payloads (`T1-COMBAT-09c`), profiled feel evidence (`T1-COMBAT-10`), architecture scan tooling (`T1-COMBAT-11`), and final Layer 1 HUD visual treatment remain owned by later explicit stories. `T1-COMBAT-09b`'s 2.0d save-barrier integration remains the documented Sprint 1 unknown.
- Next recommended: `/dev-story T1-COMBAT-09a-npc-death-playerkillcreditevent-emission`.

## Session Extract - /story-done 2026-04-30 (T1-COMBAT-09b)

- Story: [production/stories/t1-combat-09b-same-frame-save-barrier-kill-credit-consistency.md](../stories/t1-combat-09b-same-frame-save-barrier-kill-credit-consistency.md) - T1-COMBAT-09b Same-Frame Save Barrier Kill-Credit Consistency.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 9/9 covered; `H-CCOM-KILL-01` acknowledgement behavior, `H-CPRO-XP-02`, `H-CPRO-XP-03`, `H-CPRO-XP-09`, `H-CPRO-XP-14`, `H-CPRO-SL-06`, `H-CPRO-CB-01`, ADR-0001, and ADR-0002 all have file:line evidence in the story AC trace.
- Evidence: `tests/evidence/T1-COMBAT-09b/t1-combat-09b-stage2.trx:762` 124/124 PASS; verification summary at [tests/evidence/T1-COMBAT-09b/verification.md](../../tests/evidence/T1-COMBAT-09b/verification.md).
- Frozen-event invariant: `PlayerKillCreditEvent` remained unchanged from the 09a baseline `b2fe66f`; `git diff --exit-code b2fe66f -- src/gameplay/combat/events/CombatDeathEvents.cs` returned zero diff, and the contract remains four fields at `src/gameplay/combat/events/CombatDeathEvents.cs:16`.
- Boundary scan: Character Progression reads only `defeated_source_ref`, `zoneId`, `faction_id`, and `kill_weight_seed` from the approved Combat kill-credit event, then uses progression-owned lookup, snapshot, and dedupe registry state.
- No-byte assertion: grouped save attempts make zero writer calls when any required barrier is unresolved, covered by `tests/integration/core/save/save_grouped_barrier_consistency_test.cs:33` and `tests/integration/gameplay/progression/progression_save_barrier_kill_credit_consistency_test.cs:58`.
- Regression check: prior 09a TRX comparison passed with `old_total=113 old_passed=113 new_total=124 missing_old_passed=0`.
- Architectural milestone: this is the first Sprint 1 implementation batch crossing `src/gameplay/progression/`, `src/gameplay/npc/`, and `src/core/save/` simultaneously while keeping Combat, Progression, NPC, and Save surfaces narrow and one-way.
- ADR ride-along: ADR-0001 and ADR-0002 status changed from Proposed to Accepted; DECISIONS.md D007 and D008 status changed from Proposed to Locked. This is metadata-only and validated by T1-COMBAT-09b implementation commit `617a431`.
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1-COMBAT-09b` done, records 9/13 Sprint 1 stories done, and surfaces `T1-COMBAT-09c` as next active.
- Tech debt logged: None.
- Carried forward: player death payload narrowing (`T1-COMBAT-09c`), profiled combat-feel evidence (`T1-COMBAT-10`), forbidden-pattern compliance scan/analyzer (`T1-COMBAT-11`), and final Layer 1 HUD visual treatment. `T1-COMBAT-09c` will introduce the first write of the held feel-review policy at `production/qa/combat/feel-review-09c-player-death.md`.
- Next recommended: `/dev-story T1-COMBAT-09c-player-death-payload-stub-reserved-integration`.

## Session Extract - /story-done 2026-04-30 (T1-COMBAT-09c)

- Story: [production/stories/t1-combat-09c-player-death-payload-stub-reserved-integration.md](../stories/t1-combat-09c-player-death-payload-stub-reserved-integration.md) - T1-COMBAT-09c Player Death Payload Stub Reserved Integration.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 5/5 covered; `H-CCOM-DEATH-01`, `H-CCOM-DEATH-02`, `H-CCOM-DEATH-03`, `H-CCOM-SL-01`, and `H-CCOM-SL-03` all have file:line evidence in the story AC trace.
- Evidence: `tests/evidence/T1-COMBAT-09c/t1-combat-09c-stage2.trx:816` 133/133 PASS; verification summary at [tests/evidence/T1-COMBAT-09c/verification.md](../../tests/evidence/T1-COMBAT-09c/verification.md).
- Frozen-event invariant: `PlayerKillCreditEvent` remained unchanged from the `b2fe66f` baseline; `git diff b2fe66f -- src\gameplay\combat\events\CombatDeathEvents.cs` showed only the additive `PlayerDeathEvent` block.
- Payload and persistence shapes: `PlayerDeathEvent` is exactly six fields (`death_context_id`, `local_character_id`, `zoneId`, `death_position`, `killer_source_ref`, `death_cause_type`); `CombatPersistenceProjection` is exactly four read-only fields (`current_health`, `current_mana`, `combat_life_state`, optional `pending_death_handoff_payload`).
- Scope guards: Death & Corpse Recovery remained stub-only with zero matches for corpse-run, respawn, resurrection, or `xp_loss`; 09b save coordinator files remained unchanged by `git diff --exit-code 617a431 -- src\core\save\SaveStabilityBarrierProtocol.cs src\core\save\GroupedSaveAttemptCoordinator.cs`.
- Held-policy artifact: [production/qa/combat/feel-review-09c-player-death.md](../qa/combat/feel-review-09c-player-death.md) exists with implementation-perspective sections, intentional-absence notes, human-prompt template, and `<!-- HUMAN PLAYTEST PENDING -->` marker at line 41. No agent verdict was issued.
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1-COMBAT-09c` done, records 10/13 Sprint 1 stories done, and surfaces `T1-COMBAT-10` as next active.
- Tech debt logged: None.
- Carried forward: human qualitative death-moment playtest pending for the slice review; `T1-COMBAT-10` owns profiled feel evidence and the Unity harness per held policy; `T1-COMBAT-11` owns forbidden-pattern scan/analyzer and is the hard-stop trigger before slice review. ADR-0004/D010 and ADR-0005/D011 remain Proposed pending their future-sprint validation gates.
- Sprint note: Sprint 1 implementation is 10/13 done. Two fresh implementation stories remain: `T1-COMBAT-10` and `T1-COMBAT-11`. After `T1-COMBAT-11` closes, no further `/dev-story` work should run until the slice review session produces `production/qa/combat/feel-review-T1-slice.md` and Brian issues the Green/Yellow/Red verdict.
- Next recommended: `/dev-story T1-COMBAT-10-smoke-profiled-evidence-loop`.

## Session Extract - /story-done 2026-05-05 (T1-COMBAT-10)

- Story: [production/stories/t1-combat-10-smoke-profiled-evidence-loop.md](../stories/t1-combat-10-smoke-profiled-evidence-loop.md) - T1-COMBAT-10 Smoke/Profiled Evidence Loop.
- Verdict: COMPLETE WITH NOTES - quantitative ACs failed; metrics surfaced for slice review.
- Criteria: `H-CCOM-FEEL-02`, `H-CCOM-FEEL-04`, `H-CCOM-ART-02`, `H-CCOM-AUD-01`, and `H-CCOM-SCOPE-01` passed. `H-CCOM-FEEL-01` failed-as-measured with `20/20` solo-trash wins vs. the `55-85%` target. `H-CCOM-FEEL-03` failed-as-measured with `5/10` dangerous two-trash outcomes vs. the `>=8/10` target.
- Evidence: JSONL at [tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl](../../tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl); verification summary at [tests/evidence/T1-COMBAT-10/verification.md](../../tests/evidence/T1-COMBAT-10/verification.md); slice-review input at [production/qa/combat/t1-combat-10-profiled-evidence-summary.md](../qa/combat/t1-combat-10-profiled-evidence-summary.md).
- Verification: `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` passed 133/133; `git diff --exit-code 6875672 -- src/ tests/Gravenspire.Combat.Tests.csproj tests/unit tests/integration` returned empty; structural scan found zero Combat-owned visual/audio matches; harness T1 scope grep returned zero matches; `git diff --check` and `bash .githooks/pre-commit` passed.
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1-COMBAT-10` done, records 11/13 Sprint 1 stories done, and surfaces the slice review session as the next active gate.
- Tech debt logged: None.
- Carried forward: slice review session must run next and produce `production/qa/combat/feel-review-T1-slice.md` with Green/Yellow/Red verdict. `T1-COMBAT-11` holds until that verdict is issued.
- Next recommended: slice review session; do not run `/dev-story T1-COMBAT-11` before the verdict.
