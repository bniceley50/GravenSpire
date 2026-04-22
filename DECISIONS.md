# DECISIONS.md — Architecture Decisions Log

Append-only. Every entry gets a unique D-number. Never renumber. If a decision
is superseded, add a new D-entry that supersedes it and link both ways.

This log **complements** (does not replace) `docs/architecture/` ADRs created
by `/architecture-decision`. Rule of thumb: **ADRs are chapters, DECISIONS.md
is the index.** Every DECISIONS.md entry that has an ADR links to it.

---

## D001 — Stack Lock: Unity 6.3 LTS + C# (.NET 8+) + URP

**Date:** 2026-04-21
**Status:** Locked
**Context:** Pre-production engine selection. Ruled out Godot (C# support
maturity), Unreal (team size mismatch), and custom engine (scope).
**Decision:** Unity 6.3 LTS, C# (.NET 8+), URP, Unity Test Framework + Moq,
PhysX.
**Consequences:**
- Engine reference pinned at `docs/engine-reference/unity/VERSION.md`
- BIRP explicitly excluded
- HDRP explicitly excluded unless a photoreal pivot occurs (not planned)
- Addressables for asset management (no `Resources/` folder usage)
- Expect LLM knowledge gap for 6.1–6.3 APIs — cross-reference engine docs
  before any URP or UI Toolkit code
**See also:** `.claude/docs/technical-preferences.md`,
`docs/engine-reference/unity/VERSION.md`.

---

## D002 — FishNet Deferred to Tier 2

**Date:** 2026-04-21
**Status:** Locked
**Context:** Gravenspire is planned as a small persistent MMO. Netcode library
selection was made during `/brainstorm`, but installation is deferred.
**Decision:** FishNet is the planned netcode library. It is **NOT** added to
Allowed Libraries in `.claude/docs/technical-preferences.md` until Tier 2
sprint work actively begins. No speculative installation. No placeholder
`src/networking/**` code during Tier 1.
**Consequences:** Tier 1 work is strictly single-player offline. Any networking
code, even placeholder, requires a tier-transition decision appended here.
**Supersedes:** none.

---

## D003 — Single-Player Offline Through Tier 1

**Date:** 2026-04-21
**Status:** Locked
**Context:** Vertical-slice scope definition. Multiplayer pressure is real but
premature. Validate core gameplay (combat, faction sim, save/load) offline
before adding network complexity.
**Decision:** T1 vertical slice is single-player, offline, local saves. Faction
simulation runs in-process. Combat, inventory, save/load, and one biome area
are the T1 surfaces.
**Consequences:**
- No netcode, no account system, no server backend, no LLM calls live in T1
- Save files are local only (but HMAC-signed — see `SECURITY.md` threat #1)
- LLM dialogue (if any) is stubbed with templated responses
- Faction sim runs authoritatively in the client — no determinism/replication
  requirements until T2
**Related:** D002 (FishNet deferred), D004 (LLM scope).

---

## D004 — LLM Dialogue Scope: Templated Default, 5–10 NPCs at T3

**Date:** 2026-04-21
**Status:** Provisional (revisit at T3 entry gate)
**Context:** Gravespire concept includes "LLM-driven NPC dialogue" as a pillar.
Full LLM-per-NPC is infeasible at MMO scale (cost + latency + moderation).
**Decision:** Default NPC dialogue is **templated** (static + slot-filled).
LLM dialogue is reserved for **5–10 named faction NPCs** and only lights up in
**Tier 3**. All LLM output passes a moderation layer before display.
**Consequences:**
- T1 dialogue system is fully templated — no LLM dependency
- `.claude/rules/llm-moderation.md` applies only when LLM code lands in T3
- Cost model, vendor selection, and fallback behavior are T3-entry decisions
**Revisit triggers:** Tier 3 entry gate — confirm cost model, moderation
vendor, fallback when LLM call fails, cache strategy.

---

## D005 — Governance Migration from clinic-notes-ai

**Date:** 2026-04-21
**Status:** Locked
**Context:** clinic-notes-ai proved the value of a tight governance stack
(system prompt + `AGENTS.md` + `DECISIONS.md` + `RED_TEAM` + `.claude/rules`
+ lessons ritual) under HIPAA pressure. Gravespire has a smaller risk surface
but benefits from the same structure, right-sized to game-dev.
**Decision:** Port the clinic-notes governance shape to Gravespire, adapted
for the game-dev threat model. **Tier-gate rigor:** T1 skips RED_TEAM, T2
narrows it to netcode, T3+ full. Keep CCGS's 49 agents / 72 skills / 11 rules
/ 12 hooks untouched; the governance files layer **on top of** CCGS, not
replacing it.
**Consequences:**
- New top-level files: `AGENTS.md`, `DECISIONS.md`, `RED_TEAM.md`,
  `RED_TEAM_RUBRIC.md`, `SECURITY.md`, `CLAUDE-patterns.md`
- New `tasks/` directory with `lessons.md`
- 4 new security-focused rules in `.claude/rules/`
- Pre-PR 4-question check added to PR template (overwrote existing)
- Dependabot added for NuGet + GitHub Actions
- New skills: `/update-memory-bank`, `/pre-pr-review`
- `CLAUDE.md` gets a "Read First" block; `@import` chain preserved
**Explicitly excluded:**
- ULTRATHINKING adversarial prompt (clinic-notes aspirational, never run)
- `pre-tool.sh` hook (clinic-notes stubbed no-op)
- `.codex/` workflow (Gravespire is Claude Code only)
- `.github/workflows/ci.yml` (T1 has no CI per AGENTS.md §6; add at T2)
**Supersedes:** none.
**See also:** `AGENTS.md` (the new behavioral contract), migration-source
parent document (`docs/brian-system-prompt-v4-6.md`).
