# Active Session State

**Last Updated:** 2026-04-26
**Project Stage:** Pre-Production — Design

## Current Task

Park **Inventory & Item Economy GDD** after full review and pivot to `/prototype combat-feel`.

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
- ✅ Codex PR #1 (`codex/dotnet-format-setup` → `main`) merged into `main`; follow-up cleanup commit `ce634c3` recorded.

## Files Being Worked On

- **Active:** `/prototype combat-feel` — Unity standalone prototype scaffold in [prototypes/combat-feel/](../../prototypes/combat-feel/); advisory report in [production/prototypes/combat-feel-report.md](../prototypes/combat-feel-report.md); Inventory & Item Economy remains parked pending implementation pre-spec
- Combat Feel Prototype: [prototypes/combat-feel/README.md](../../prototypes/combat-feel/README.md) — prototype question, success/failure criteria, loop spec, controls, run notes
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
- Mana-restore 1:1 linear fill depends on med-break mechanics (not yet specced in combat GDD)
- Zone definition = Addressable streaming group boundary; `unity-addressables-specialist` must configure streaming groups
- 26 MVP systems is large scope for a solo first-time dev — every design decision should keep asking "is this actually needed for Tier 1?"
- Inventory & Item Economy is intentionally parked after review; do not implement it until an Inventory implementation pre-spec closes `INV-OQ-05` and a fresh full review passes.

## Next Skill to Run

- **Continue `/prototype combat-feel`** — rerun the standalone/scene in pinned Unity `6000.3.x LTS`, explicitly probe all four README success criteria, then decide whether one-knob Smite cast-time iteration is needed.
- Later: run Inventory implementation pre-spec to close `INV-OQ-05`, then rerun `/design-review design/gdd/inventory-item-economy.md --depth full`.
- Codex PR #1 is merged; no Codex follow-up pending in this active state file.
