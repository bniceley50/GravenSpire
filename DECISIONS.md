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
**Status:** Locked
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
**Status:** Locked
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
**Status:** Locked
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

---

## D010 — ADR-0004 First-Save Materialization and Character Identity

**Date:** 2026-04-26
**Status:** Proposed
**Context:** Character Creation already defines
`InitialCharacterRecord.local_character_id`, but its generation location was an
open question. Character Progression requires `local_character_id` for XP dedupe
and first-save progression state, while Save/Load owns the first-run path and
must not synthesize missing progression state on first load.
**Decision:** Create
`docs/architecture/adr-0004-first-save-materialization-and-character-identity.md`.
The ADR proposes that Character Creation owns `local_character_id` generation
and validation, Save/Load owns persistence and active-record identity context,
and downstream systems consume the id read-only. Before the first successful
save, Save/Load invokes declared first-save materializers; in T1, Character
Progression must materialize `CharacterProgressionSaveState` from
`local_character_id` plus `starting_class_id = Cleric` before any bytes are
written.
**Consequences:**
- First-save materialization is separate from ADR-0002 save-stability barriers:
  materializers run only before the first successful save; barriers run before
  normal saves of runtime-owned state.
- Failed first-save materialization emits
  `SaveFailedEvent(FirstSaveMaterializationFailed)`, writes no bytes, and does
  not mark the local record initialized.
- Subsequent loads never repair missing identity or progression state by
  re-running first-save materialization; missing required persisted state fails
  loud through Save/Load.
- Follow-up ADR remains needed for progression pacing fixture contracts.
**See also:**
`docs/architecture/adr-0004-first-save-materialization-and-character-identity.md`;
`docs/architecture/adr-0001-xp-source-lifecycle-registry.md`;
`docs/architecture/adr-0002-save-stability-barrier-protocol.md`;
`docs/architecture/adr-0003-progression-baseline-snapshot-contract.md`;
`design/gdd/character-creation.md`; `design/gdd/character-progression.md`;
`design/gdd/save-load-persistence.md`; `design/gdd/systems-index.md`.

---

## D011 — ADR-0005 Progression Pacing Fixture Contracts

**Date:** 2026-04-26
**Status:** Proposed
**Context:** Character Progression pacing criteria need deterministic fixture
evidence for XP/hour, kills/level, camp-session ding cadence, and anti-farming
claims. Round-4 review also exposed that event-order edge cases can become
mathematically impossible if every fixture is forced through legal kill-credit
caps, while pacing claims become invalid if they use synthetic direct-XP
transactions.
**Decision:** Create
`docs/architecture/adr-0005-progression-pacing-fixture-contracts.md`. The ADR
proposes explicit fixture kinds: `LegalKillCreditRoute` for pacing evidence,
`FormulaOnly` for pure formula checks, `SyntheticEventTransaction` for event
ordering and cap edge cases, `InvalidDataValidation` for validator failures,
and `ProfileRunSpec` for QA profile instructions. Pacing signoff requires a
passing `PacingMathPreflight` from legal kill-credit fixtures before profiled
playtest evidence can count.
**Consequences:**
- Character Progression GDD should reference ADR-0005 for pacing fixture kinds,
  deterministic preflight, and profiled playtest evidence rules.
- Synthetic event fixtures may test multi-level event ordering, but cannot prove
  XP/hour, kills/level, time-to-ding, camp cadence, or earned progression
  fantasy.
- Legal pacing fixtures must resolve against ADR-0001 lookup rows, Combat Core
  fixture ids, expected `kill_weight_seed`, repeatability class, lifecycle
  policy, and cadence models.
- This completes the five-ADR architecture set needed before the final
  Character Progression GDD revision pass.
**See also:**
`docs/architecture/adr-0005-progression-pacing-fixture-contracts.md`;
`docs/architecture/adr-0001-xp-source-lifecycle-registry.md`;
`docs/architecture/adr-0002-save-stability-barrier-protocol.md`;
`docs/architecture/adr-0003-progression-baseline-snapshot-contract.md`;
`docs/architecture/adr-0004-first-save-materialization-and-character-identity.md`;
`design/gdd/character-progression.md`; `design/gdd/combat-core.md`;
`design/gdd/npc-system.md`; `design/gdd/systems-index.md`.

---

## D012 — T1 Combat-Feel Validated; Combat Core Revision Required Before /sprint-plan new

**Date:** 2026-04-26
**Status:** Locked
**Context:** The combat-feel prototype cycle ran v1 baseline, v2 tactical
instants plus Attack toggle, and pinned Unity `6000.3.14f1` validation. All six
README success criteria passed with disk-captured JSONL evidence recorded in
`prototypes/combat-feel/Logs/playtest-20260426-204721.log` and
`prototypes/combat-feel/Logs/playtest-20260426-205508.log`; those metrics are
summarized durably in `production/prototypes/combat-feel-report.md`. The
project's biggest documented core-hypothesis risk in
`design/gdd/game-concept.md:303` is answered affirmatively at prototype-grade
evidence level.
**Decision:** T1 combat-feel is the project's first prototype-validated pillar.
Combat Core GDD must revise before `/sprint-plan new` is run, adding:
- Attack toggle as a first-class player actor state
- Tactical Cleric instants as a first-class T1 ability surface
- Explicit Attack ON visual state requirement

T1 sprint planning is unblocked once Combat Core revision passes
`/design-review`.
**Consequences:**
- Combat Core's currently approved `PlayerKillCreditEvent` payload is preserved;
  the validation requires scoped combat-feel additions, not an architectural
  pivot.
- Attack toggle and tactical instants must be designed into production Combat
  Core rather than copied from throwaway prototype code.
- The faction-control prototype risk in `design/gdd/game-concept.md:304`
  remains open; run a separate `/prototype faction-feel` later.
**See also:** `production/prototypes/combat-feel-report.md`;
`prototypes/combat-feel/README.md`; `design/gdd/combat-core.md`;
`design/gdd/game-concept.md`; commits `7add6ee` through `83598de`.

---

## D013 — ADR-0006 Endurance Resource Model

**Date:** 2026-05-06
**Status:** Locked
**Context:** The T1 combat slice review committed at `4edf2f9` recorded Brian's
Yellow verdict and identified a harder resource-model finding: Bash and future
physical instants should move off mana onto a quiet Endurance resource, while
`Smite of Authority` and `Defensive Prayer` remain mana-based Cleric abilities.
The finding is captured in `production/qa/combat/feel-review-T1-slice.md:54`
through `production/qa/combat/feel-review-T1-slice.md:58`, and Brian's verdict
prose requires Endurance to support physical pacing without becoming an
action-rotation bar at `production/qa/combat/feel-review-T1-slice.md:80`.
Combat Core's current tactical instant contract still describes authored mana
costs for all tactical instants in `design/gdd/combat-core.md:148` through
`design/gdd/combat-core.md:152` and `design/gdd/combat-core.md:746` through
`design/gdd/combat-core.md:747`.
**Decision:** Create
`docs/architecture/adr-0006-endurance-resource-model.md` as the proposed
Endurance contract for Sprint 1.5. ADR-0006 defines physical-only Endurance
scope, quiet HUD/save discipline, resource split rules for Bash/future physical
instants, and explicit carveouts keeping `Smite of Authority` and
`Defensive Prayer` mana-based.
**Consequences:**
- T1.5-COMBAT-00 authors the contract only; it does not implement Endurance,
  change fixtures, amend GDD acceptance criteria, or tune combat feel.
- T1.5-COMBAT-01 should validate the Combat Core actor-state, persistence, and
  HUD-projection portions of ADR-0006 while keeping the ADR/D-entry proposed.
- T1.5-COMBAT-02 should validate the physical-instant resource split. If that
  implementation holds, the closure batch may move ADR-0006 from Proposed to
  Accepted and this D013 entry from Proposed to Locked.
- T1-COMBAT-11 should treat ADR-0006's banned Endurance patterns as scan input
  after the contract and implementation stories land.
**See also:**
`docs/architecture/adr-0006-endurance-resource-model.md`;
`production/qa/combat/feel-review-T1-slice.md`;
`production/sprints/sprint-1-5.md`;
`production/qa/plans/qa-plan-sprint-1-5-20260506.md`;
`design/gdd/combat-core.md`.

---

## D014 — FEEL-01 Clean-State Solo Trash Target Revalidated

**Date:** 2026-05-08
**Status:** Locked
**Context:** `H-CCOM-FEEL-01` originally expected `SoloTrash_EvenCon_T1` to
land inside a `55-85%` Cleric win-rate band. T1-COMBAT-10 measured `20/20`
solo-trash wins against that target in
`tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:1` and summarized
the failure in `tests/evidence/T1-COMBAT-10/verification.md:64`. That evidence
was generated before D013/ADR-0006 Endurance implementation: Bash still spent
mana when the original harness result was recorded. T1.5-COMBAT-02 later
validated the physical-instant split, moving Bash to Endurance while keeping
`Smite of Authority` and `Defensive Prayer` mana-based. The post-Endurance
Cleric is therefore stronger in clean solo-trash pulls than the model that the
original FEEL-01 band measured; the resource split is recorded at
`tests/evidence/T1.5-COMBAT-02/verification.md:32` through
`tests/evidence/T1.5-COMBAT-02/verification.md:35`.

D012's prototype validation already supported reliable solo-trash play: the
direct playtest finding in `production/prototypes/combat-feel-report.md:142`
was "you nailed it! felt really smooth," and the pinned-engine rerun recorded
`5/5` pulls, `5` med breaks, `0` unsafe pulls, and `0` deaths in
`prototypes/combat-feel/Logs/playtest-20260506-093105.log:1`. The T1 slice
review also framed FEEL-01 as softer than FEEL-03 after the prototype rerun
produced clean solo-trash wins, while FEEL-03 remained the stronger pull-
discipline warning in `production/qa/combat/feel-review-T1-slice.md:80`.
T1.5-COMBAT-03 then restored FEEL-03 with `dangerous_outcomes=9` while
intentionally leaving FEEL-01 untouched, recorded at
`tests/evidence/T1.5-COMBAT-03/verification.md:24` and
`tests/evidence/T1.5-COMBAT-03/verification.md:61` through
`tests/evidence/T1.5-COMBAT-03/verification.md:63`.

**Decision:** Move `H-CCOM-FEEL-01` from `55-85%` to `90-100%` Cleric wins for
clean-state solo trash. "Clean-state" means `SoloTrash_EvenCon_T1` starts the
same-band Cleric above 80% health and above 60% mana against one same-band
`encounter_role = Trash` fixture, using intended casts, auto-attack, tactical
instants, and med breaks. Preserve the ending-state pressure clause: the mean
ending state must still fall below either 80% health or 60% mana so that
immediately chaining the same pull remains measurably riskier than sitting and
regenerating first.

FEEL-01 now owns clean solo-trash reliability. FEEL-03 owns overpull danger.
Those are distinct concerns: a Cleric should usually win a clean same-band
single-trash pull, while normal two-trash farming should remain non-viable.

**Consequences:**
- Combat Core's FEEL-01 GDD acceptance criterion and clean-solo tuning knob
  move to the new `90-100%` target.
- No fixture data, harness behavior, or profiled evidence rows are changed by
  T1.5-COMBAT-04; T1.5-COMBAT-05 owns the next profiled rerun and any
  machine-readable target/output-label updates needed after this decision.
- Historical references to the original `55-85%` target remain valid as
  evidence history when explicitly classified as historical or superseded.
- Low-resource, surprise-pull, interrupted-med-break, or other disadvantage
  solo-trash vulnerability remains a future acceptance-criteria candidate; it
  is not part of this clean-state FEEL-01 target.
**See also:**
`design/gdd/combat-core.md`;
`production/stories/t1-5-combat-04-feel-01-target-revalidation.md`;
`tests/evidence/T1.5-COMBAT-04/verification.md`;
`tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl`;
`tests/evidence/T1.5-COMBAT-03/profiled-combat-slice.jsonl`.

---

## D015 — Qwen3-Coder Onboarded as Scoped Local Implementer

**Date:** 2026-05-20
**Status:** Locked
**Context:** D006 (2026-04-22) onboarded Codex as a full parallel `/dev-story`
implementer. Codex and Claude both bill metered API tokens. To offload small,
low-design mechanical edits off that metered budget, a local model —
`qwen3-coder-30b-a3b`, run on LM Studio (a local OpenAI-compatible inference
server) and driven by the Aider CLI harness — was trialed on 2026-05-20. The
trial task was the `m2_02_runner_date_hardcoded` carryover fix (commit
`9035d09`). Outcome: the code output was correct and minimal, but the model
over-reached scope — it attempted to edit scene, evidence, and story files
despite an explicit "change nothing else" instruction. The Aider harness
(`--no-auto-commit` plus per-file "Add file to the chat?" gates, answered by
the user) fully contained the sprawl: only the two intended files reached disk.
This entry formalizes Qwen3-Coder as a sanctioned but tightly scoped local
implementer and records the guardrails the trial proved necessary.
**Decision:** Qwen3-Coder is a sanctioned local implementer on Gravenspire,
narrower in scope than Codex:
- **Role:** Scoped local implementer for small, low-design, mechanical code
  edits (literal cleanup, deny-pattern-safe refactors, single-function changes
  against a named pattern). It is **not** a `/dev-story` implementer, **not** a
  reviewer, **not** a PR author, and **not** a design or architecture partner.
- **Why:** Offload small mechanical edits from the metered Claude/Codex token
  budget. Cost reduction is the only rationale; it does not expand who may make
  design decisions.
- **Harness:** Aider CLI against LM Studio. Required Aider flags:
  `--no-auto-commit` (the model never commits) and `--edit-format diff` (the
  trial's `whole` format was slow and sprawl-encouraging — ~8k tokens for a
  one-line fix). Per-file "Add file to the chat?" prompts stay ON; the user
  answers them and denies any file outside the task.
- **Mandatory review:** Every Qwen3-Coder output receives a full Claude
  `/code-review` before it is staged or committed, no exception. The trial
  proved the model over-reaches scope, so its output is never trusted on the
  model's own description — it is judged only by `git diff` of what actually
  reached disk.
- **Authority:** Write only inside a dedicated branch/worktree. Never stage,
  never commit, never push to `main`, never force-push. A human or Claude Code
  performs staging and commits after review.
- **Neutral system prompt (security guardrail):** Qwen3-Coder must run for
  Gravenspire work only under a neutral or empty system prompt. The LM Studio
  model was found configured with a jailbreak / "amoral AI" / "ignore
  restrictions" persona system prompt; that persona must be cleared before any
  Gravenspire code or content task. Project work never runs under a jailbreak
  or restriction-bypassing persona.
- **Forbidden zones** (no edits, identical to D006's Codex list):
  `design/gdd/**`, `design/art/art-bible.md`, `DECISIONS.md`, `AGENTS.md`,
  `CLAUDE.md`, `docs/engine-reference/**`, `.claude/agents/**`,
  `.claude/skills/**`, `.claude/rules/**`. Given the model's demonstrated
  over-reach, the per-file add gate is the enforced control, not the
  instruction text.
- **Harness state is local:** Aider writes chat history, input history, and a
  tag cache into the working tree. `.aider*` is gitignored (commit `9035d09`);
  harness state is never pushed.
**Consequences:**
- Qwen3-Coder governance is review-gated, not trust-gated: its value depends on
  the mandatory `/code-review` step, so it only saves tokens net when the task
  is small enough that review is cheaper than implementing directly.
- It does not replace Codex. Codex remains the full `/dev-story` implementer
  under D006; Qwen3-Coder handles only sub-story mechanical edits.
- Tasks unsuitable for Qwen3-Coder (anything touching design, architecture,
  multi-file integration, or scope judgement) go to Codex or Claude.
- If the neutral-system-prompt guardrail cannot be verified for a session,
  Qwen3-Coder is not used for that session.
- `AGENTS.md` §0 worktree rules may later gain a Qwen3-Coder cross-reference;
  until then this entry is the governing record.
**Related:** D006 (Codex parallel implementer); commit `9035d09` (trial result
+ `.aider*` gitignore); `AGENTS.md` §0 worktree rules.
