# Active Session State

**Last Updated:** 2026-04-30
**Project Stage:** Pre-Production — Sprint 1 Implementation

## Current Task

`T1-COMBAT-03` is closed via `/story-done` with verdict `COMPLETE WITH NOTES`.
Next work is `/dev-story T1-COMBAT-04-melee-tick-weapon-delay-resolution`.

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

## Files Being Worked On

- **Active:** T1-COMBAT-04 melee tick / weapon-delay resolution - run `/dev-story T1-COMBAT-04-melee-tick-weapon-delay-resolution`.
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
- Mana-restore 1:1 linear fill now depends on Combat Core's med-break regen tick contract; Layer 1 HUD must consume Combat Core state rather than inventing separate fill timing.
- ADR-0003 / D009 status metadata still says `Proposed`; `T1-COMBAT-01` honored the `CombatProgressionBaselineSnapshot` contract but did not rewrite ADR metadata during closure.
- `H-CCOM-F2B` fixture extremes are validated by `T1-COMBAT-01`; seeded melee formula execution remains owned by `T1-COMBAT-04`.
- Creature / Enemy AI still owns actual return-to-anchor movement and NavMeshAgent behavior; `T1-COMBAT-02` supplies Combat Core leash hooks and test doubles only.
- Optional deferred lesson capture remains open for the `[GLOBAL][CI]` `git diff --check` line-ending warning pattern if it repeats outside `T1-COMBAT-02`.
- Remaining game-concept project-killer risk: `design/gdd/game-concept.md:304` asks whether kills shifting faction control feel meaningful at the five-minute scale. Needs a later separate `/prototype faction-feel`.
- Zone definition = Addressable streaming group boundary; `unity-addressables-specialist` must configure streaming groups
- 26 MVP systems is large scope for a solo first-time dev — every design decision should keep asking "is this actually needed for Tier 1?"
- Inventory & Item Economy is intentionally parked after review; do not implement it until an Inventory implementation pre-spec closes `INV-OQ-05` and a fresh full review passes.

## Next Skill to Run

- **Run `/dev-story T1-COMBAT-04-melee-tick-weapon-delay-resolution`** for the next Sprint 1 combat story.
- Later: run Inventory implementation pre-spec to close `INV-OQ-05`, then rerun `/design-review design/gdd/inventory-item-economy.md --depth full`.
- Codex PR #1 is merged; no Codex follow-up pending in this active state file.

## Session Extract - /story-done 2026-04-28

- Story: [production/stories/t1-combat-01-cleric-base-combat-actor-fixture-hydration.md](../stories/t1-combat-01-cleric-base-combat-actor-fixture-hydration.md) - T1-COMBAT-01 Cleric Base Combat Actor + Fixture Hydration.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 6/6 passing; `H-CCOM-F2B` passes for fixture coverage with formula execution deferred to `T1-COMBAT-04`; ADR-0003 snapshot contract passes with metadata status cleanup carried forward.
- Evidence: `tests/evidence/T1-COMBAT-01/t1-combat-01-stage2.trx` 15/15 PASS; `tests/evidence/T1-COMBAT-01/t1-combat-01-stage3-rerun.trx` 15/15 PASS; verification summary at [tests/evidence/T1-COMBAT-01/verification.md](../../tests/evidence/T1-COMBAT-01/verification.md).
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1-COMBAT-01` done and surfaces `T1-COMBAT-02` as next active.
- Tech debt logged: None.
- Carried forward: ADR-0003 / D009 status metadata cleanup; `H-CCOM-F2B` seeded melee formula execution in `T1-COMBAT-04`.
- Next recommended: `/dev-story T1-COMBAT-02-targeting-and-hostile-actor-claim`.

## Session Extract - /story-done 2026-04-30

- Story: [production/stories/t1-combat-02-targeting-and-hostile-actor-claim.md](../stories/t1-combat-02-targeting-and-hostile-actor-claim.md) - T1-COMBAT-02 Targeting and Hostile Actor Claim.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 11/11 passing; `H-CCOM-WS-01`, `H-CCOM-WS-02`, `H-CCOM-WS-03`, `H-CCOM-TGT-01`, `H-CCOM-PULL-01`, `H-CCOM-PULL-02`, `H-CCOM-PULL-03`, `H-CCOM-PULL-04`, `H-CCOM-LEASH-01`, `H-CCOM-LEASH-02`, and `H-CCOM-ART-01` all have file:line evidence in the story AC trace.
- Evidence: `tests/evidence/T1-COMBAT-02/t1-combat-02-stage2.trx` 27/27 PASS; `tests/evidence/T1-COMBAT-02/t1-combat-02-stage3-rerun.trx` 27/27 PASS; verification summary at [tests/evidence/T1-COMBAT-02/verification.md](../../tests/evidence/T1-COMBAT-02/verification.md); live closure rerun passed 27/27 with `dotnet test tests\Gravenspire.Combat.Tests.csproj --no-restore --logger "console;verbosity=minimal"`.
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1-COMBAT-02` done, records 2/13 Sprint 1 stories done, and surfaces `T1-COMBAT-03` as next active.
- Tech debt logged: None.
- Carried forward: ADR-0003 / D009 status metadata cleanup; `H-CCOM-F2B` seeded melee formula execution in `T1-COMBAT-04`; Creature / Enemy AI return-to-anchor/NavMeshAgent movement implementation; optional deferred `[GLOBAL][CI] git diff --check` lesson capture.
- Next recommended: `/dev-story T1-COMBAT-03-attack-toggle-state-machine`.

## Session Extract - /story-done 2026-04-30 (T1-COMBAT-03)

- Story: [production/stories/t1-combat-03-attack-toggle-state-machine.md](../stories/t1-combat-03-attack-toggle-state-machine.md) - T1-COMBAT-03 Attack Toggle State Machine.
- Verdict: COMPLETE WITH NOTES.
- Criteria: 4/4 passing; `H-CCOM-AA-01`, `H-CCOM-AA-03`, `H-CCOM-MED-01` edge precondition, and `H-CCOM-HUD-04` edge precondition all have file:line evidence in the story AC trace.
- Evidence: `tests/evidence/T1-COMBAT-03/t1-combat-03-stage2.trx:276` 43/43 PASS; verification summary at [tests/evidence/T1-COMBAT-03/verification.md](../../tests/evidence/T1-COMBAT-03/verification.md); live closure rerun passed 43/43 with `dotnet test tests\Gravenspire.Combat.Tests.csproj --no-restore --logger "console;verbosity=minimal"`.
- State updates: story status set to Complete; `production/sprint-status.yaml` marks `T1-COMBAT-03` done, records 3/13 Sprint 1 stories done, and surfaces `T1-COMBAT-04` as next active.
- Tech debt logged: None.
- Carried forward: ADR-0003 / D009 status metadata cleanup; `H-CCOM-F2B` seeded melee formula execution in `T1-COMBAT-04`; melee resolution, cast lifecycle, tactical instant execution, med regen math, HUD presentation, kill credit, save barriers, death payloads, profiled feel evidence, and architecture scan tooling remain owned by later Sprint 1 stories.
- Next recommended: `/dev-story T1-COMBAT-04-melee-tick-weapon-delay-resolution`.
