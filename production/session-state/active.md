# Active Session State

**Last Updated:** 2026-04-22
**Project Stage:** Pre-Production — Design

## Current Task

Author **World Structure GDD** (first MVP Foundation system). Skeleton created; Phase 4 Section A (Overview) next.

## Status

- ✓ Brainstorm complete → [design/gdd/game-concept.md](../../design/gdd/game-concept.md)
- ✓ Engine configured → Unity 6.3 LTS + C# + URP (FishNet planned for Tier 2+)
- ✓ Art bible complete → [design/art/art-bible.md](../../design/art/art-bible.md) (9/9 sections, Lean mode, AD-ART-BIBLE skipped)
- ✓ Systems mapped → [design/gdd/systems-index.md](../../design/gdd/systems-index.md) (33 systems, 26 MVP)
- ✓ Spelling normalized Gravespire → Gravenspire, AGENTS.md §0 canonical path fixed (commit `486f0a0`)
- ✓ D006 committed — Codex onboarded as parallel implementer (commit `fae8c8c`)
- ✓ Codex onboarding brief delivered; Codex has completed read-only onboarding and surfaced 4 clarifying questions (all answered)
- ✓ Codex Assignment #1 drafted — `dotnet format` setup on branch `codex/dotnet-format-setup`
- ✅ World Structure GDD — **complete.** All 8 required sections + 4 optional sections populated. Section C amended mid-process (Rule 12 save-timeout after Section E edge A4; Day/Night named event after Section H qa-lead flag). 26 acceptance criteria (23 T1-blocking, 3 advisory). Entity registry updated (5 formulas + 3 constants). Systems-index flipped to Designed (1/26 MVP).
- ✅ Commit `4801d8a` on `main` (GDD + registry + systems-index + active.md bundle; rebased onto PR #1 merge).
- Next: run `/design-review design/gdd/world-structure.md` in a **fresh session** for independent validation; then `/consistency-check` before next GDD; then Save/Load & Persistence (next in Foundation order).
- 🔄 Codex PR #1 (`codex/dotnet-format-setup` → `main`): Claude Code review posted as COMMENTED (GitHub blocks request-changes on self-owned PRs). 3 polish items requested before merge: (1) expand traversal exclusions for Unity paths, (2) attribute-decorated field regex fix, (3) document fallback rules in README. Awaiting Codex revisions.

## Files Being Worked On

- **Active:** [design/gdd/world-structure.md](../../design/gdd/world-structure.md) — skeleton; Section A next
- Game concept: [design/gdd/game-concept.md](../../design/gdd/game-concept.md)
- Art bible: [design/art/art-bible.md](../../design/art/art-bible.md) — hard constraints for this GDD (§8.7 zone=Addressable group, §8.9 ≤350MB, §4.4 no zone-boundary LUT, §6.2 ≤3-4 unique 2K surfaces/group)
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

## Next Skill to Run

- **Continue `/design-system world-structure`** — Phase 4 Section A (Overview). Skeleton is at [design/gdd/world-structure.md](../../design/gdd/world-structure.md). Feasibility brief already delivered in-session; jump straight to the Section A framing widget (Framing / ADR ref / Fantasy tabs).
- After World Structure: Save/Load & Persistence → Menus & Settings → NPC System → Combat Core.
- Codex in parallel: [Assignment #1 dotnet format setup] — waiting for brian to relay.
