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
**Context:** Gravenspire concept includes "LLM-driven NPC dialogue" as a pillar.
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
+ lessons ritual) under HIPAA pressure. Gravenspire has a smaller risk surface
but benefits from the same structure, right-sized to game-dev.
**Decision:** Port the clinic-notes governance shape to Gravenspire, adapted
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
- `.codex/` workflow (Gravenspire is Claude Code only)
- `.github/workflows/ci.yml` (T1 has no CI per AGENTS.md §6; add at T2)
**Supersedes:** none.
**See also:** `AGENTS.md` (the new behavioral contract), migration-source
parent document (`docs/brian-system-prompt-v4-6.md`).

---

## D006 — Codex Added as Parallel Implementer (Partially Supersedes D005)

**Date:** 2026-04-22
**Status:** Locked
**Context:** D005 (2026-04-21) excluded the `.codex/` workflow under the
framing "Gravenspire is Claude Code only." One day later (2026-04-22) we
reversed that framing: Codex is being onboarded as a parallel implementer,
operating from a dedicated worktree `N:\GravenSpire-codex`. `AGENTS.md` §0
was rewritten in commit `486f0a0` to include Codex worktree rules; that
rewrite was ahead of the decision log. This entry closes the gap.
**Decision:** Codex is a sanctioned parallel agent on Gravenspire:
- **Role:** Parallel coder / implementer. Claude Code remains the
  design/architecture authoring partner. Codex is not a reviewer.
- **Authority:** Write only inside its own branch/worktree. Never push to
  `main`. Never force-push.
- **Worktree:** `N:\GravenSpire-codex`, created from `origin/main`, branch
  naming `codex/<feature-name>`. One worktree per feature branch.
- **Governance:** Codex honors EDIT_OK (`AGENTS.md` §2), evidence rule
  (§3), source-of-truth table (§4), tier discipline (§6), and the pre-PR
  4-question check (§7) — identical to Claude Code.
- **Forbidden zones** (no edits without explicit per-file user approval):
  `design/gdd/**`, `design/art/art-bible.md`, `DECISIONS.md`, `AGENTS.md`,
  `CLAUDE.md`, `docs/engine-reference/**`, `.claude/agents/**`,
  `.claude/skills/**`, `.claude/rules/**`.
- **PR flow:** Codex opens PRs from `codex/<feature>` → `main`; user +
  Claude Code review.
**Consequences:**
- `docs/brian-system-prompt-v4-6.md` placeholder line naming Codex as "not
  used on Gravenspire" is obsolete; update when the placeholder is
  populated.
- D005's "excluded: `.codex/` workflow" refers specifically to the
  clinic-notes-ai `.codex/` directory shape (tooling pattern), not to
  Codex as an agent. That narrow exclusion remains correct; this entry
  opens a new scope.
**Supersedes (partial):** D005 — the "Claude Code only" framing only; all
other D005 consequences (governance stack port, new top-level files, new
rules, new skills) remain locked.
**Related:** `AGENTS.md` §0 worktree rules (commit `486f0a0`); Codex
onboarding brief (delivered 2026-04-22).

---

## D007 — ADR-0001 XP Source Lifecycle Registry

**Date:** 2026-04-26
**Status:** Proposed
**Context:** Character Progression round-4 review exposed that XP source
lookup, lifecycle tokens, repeatability classes, and NPC-owned source
lifecycle durability were being designed inline in GDD prose without an
architecture lock. Repeated GDD-only revision rounds were expanding the blast
radius across Character Progression, Save/Load, NPC System, systems-index, and
Character Creation.
**Decision:** Create the first Gravenspire ADR:
`docs/architecture/adr-0001-xp-source-lifecycle-registry.md`. The ADR proposes
that Character Progression owns authored XP metadata, transient source
registry entries, immutable award snapshots, and session-local processed-award
dedupe, while NPC System owns durable source lifecycle state through
`NpcSourceLifecycleRecord`. Combat Core's approved narrow kill-credit payload
remains unchanged.
**Consequences:**
- Character Progression GDD should reference ADR-0001 instead of redefining XP
  source lifecycle architecture inline.
- `NonRepeatableFirstKill` is future-reserved and invalid for T1 shipping rows
  until a later ADR defines durable per-character claim persistence.
- Follow-up ADRs remain needed for save-stability barriers, progression
  baseline snapshots, first-save identity/materialization, and pacing fixtures.
**See also:** `docs/architecture/adr-0001-xp-source-lifecycle-registry.md`;
`design/gdd/character-progression.md`; `design/gdd/npc-system.md`;
`design/gdd/save-load-persistence.md`; `design/gdd/combat-core.md`.

---

## D008 — ADR-0002 Save Stability Barrier Protocol

**Date:** 2026-04-26
**Status:** Proposed
**Context:** Character Progression and Save/Load round-4 review exposed that
same-frame kill/save races need one architecture-level save-readiness protocol,
not per-GDD prose. `ProgressionSaveBarrier` and
`NpcSourceLifecycleSaveBarrier` were named, but their shared request/result
shape, deadline behavior, grouped consistency semantics, and failure behavior
needed a project-level lock.
**Decision:** Create `docs/architecture/adr-0002-save-stability-barrier-protocol.md`.
The ADR proposes a declared, bounded, synchronous save-stability barrier
protocol. Save/Load invokes declared downstream barriers before reading guarded
payloads; downstream owners return stable read views or unresolved/failed
results; grouped barriers must all be stable before any member payload is
serialized; unresolved barriers fail the write loudly with
`SaveFailedEvent(DownstreamSaveBarrierUnresolved)` and no bytes written.
**Consequences:**
- Save/Load GDD should reference ADR-0002 for Rule 8a downstream barriers,
  grouped barrier semantics, and the `DownstreamSaveBarrierUnresolved` failure.
- Character Progression and NPC System should reference ADR-0002 for
  `ProgressionSaveBarrier` and `NpcSourceLifecycleSaveBarrier`.
- Follow-up ADRs remain needed for progression baseline snapshots,
  first-save identity/materialization, and pacing fixtures.
**See also:** `docs/architecture/adr-0002-save-stability-barrier-protocol.md`;
`docs/architecture/adr-0001-xp-source-lifecycle-registry.md`;
`design/gdd/save-load-persistence.md`; `design/gdd/character-progression.md`;
`design/gdd/npc-system.md`; `design/gdd/world-structure.md`.

---

## D009 — ADR-0003 Progression Baseline Snapshot Contract

**Date:** 2026-04-26
**Status:** Proposed
**Context:** Character Progression round-4 review exposed that the current
`ProgressionBaselineSnapshot(current_level, permanent_max_health,
permanent_max_mana, spell_eligibility_tier)` wording was too broad and
internally ambiguous. Save/Load and Character Progression said Combat used only
health/mana maxima, while Combat Core still needed explicit player actor level
input for its own formulas.
**Decision:** Create
`docs/architecture/adr-0003-progression-baseline-snapshot-contract.md`. The ADR
proposes consumer-scoped immutable progression snapshots. Combat Core consumes
only `CombatProgressionBaselineSnapshot`, which carries
`combat_actor_level = current_level`, permanent max health, permanent max mana,
class/character ids, schema, and revision metadata. UI/Menu and spell systems
must use separate read models and may not receive the Combat hydration payload
as a generic progression snapshot.
**Consequences:**
- Character Progression and Save/Load GDDs should replace generic
  `ProgressionBaselineSnapshot` handoff wording with
  `CombatProgressionBaselineSnapshot`.
- Combat Core gets an explicit level/max-resource input contract while keeping
  ownership of combat formulas, current resources, hydration clamp/reject
  behavior, threat, casting, regen, and death.
- `visible_level`, XP progress fields, `spell_eligibility_tier`, spell content,
  and UI presentation data are banned from the Combat baseline handoff.
- Follow-up ADRs remain needed for first-save identity/materialization and
  progression pacing fixtures.
**See also:**
`docs/architecture/adr-0003-progression-baseline-snapshot-contract.md`;
`docs/architecture/adr-0001-xp-source-lifecycle-registry.md`;
`docs/architecture/adr-0002-save-stability-barrier-protocol.md`;
`design/gdd/character-progression.md`; `design/gdd/save-load-persistence.md`;
`design/gdd/combat-core.md`; `design/gdd/systems-index.md`.
