# AGENTS.md — Gravenspire Behavioral Contract

**Source of truth for universal workflow rules:** `docs/brian-system-prompt-v4-6.md`.
This file contains **project-specific extensions only**. If `AGENTS.md` and the
system prompt conflict, the system prompt wins.

This file is read by Claude Code at session start alongside the system prompt.
Every agent, skill, and hook in this project inherits these rules via `CLAUDE.md`'s
Read First block. The 49 agent files in `.claude/agents/` do not re-state these
rules — they inherit them.

---

## 0. Canonical Repo Path

The canonical working directory for Gravenspire on this machine is:

```
N:\GravenSpire
```

(Windows host. From a POSIX shell like Git Bash, also addressable as
`/n/GravenSpire`. The camelCase folder name is identifier-form; the
in-world/prose name is "Gravenspire".)

**Legitimate alternate paths:** parallel agents (e.g. Codex) may operate from
a dedicated worktree — canonical worktree path for Codex is
`N:\GravenSpire-codex` (see "Codex onboarding" in `.agents/` if present).
Other worktrees follow the same pattern: outside the primary checkout, one
per feature branch.

If you find yourself operating on a path that is NOT the primary checkout or
a registered worktree (`~/Projects/Gravenspire`, `/Users/brian/Desktop/...`,
a stale clone anywhere): **stop and verify**. The stale-checkout risk is real
— the clinic-notes-ai 2026-03-22 lesson (inherited via the governance
migration, DECISIONS.md D005) cost a full day chasing a bug in a stale
checkout. That lesson predates Gravenspire but the failure mode is universal.

---

## 1. Session Start Ritual

Read, in order, at session start:

1. `docs/brian-system-prompt-v4-6.md` — universal rules
2. `AGENTS.md` — this file
3. `DECISIONS.md` — locked architecture decisions (D-numbered)
4. `production/session-state/active.md` — current work state (may not exist yet)
5. `design/gdd/game-concept.md` — Gravenspire concept / pillars
6. `tasks/lessons.md` — accumulated lessons

Then read files relevant to the current task. CCGS's `session-start.sh` hook
previews `active.md` automatically; the rest is on you.

---

## 2. EDIT_OK Protocol

Before writing or editing **any** file, request approval with the phrase:

> **May I write this to [filepath]?**

The user replies with `EDIT_OK`, `EDIT_OK [filepath]`, or a denial. Batch
approvals (`execute` on a pre-listed set of files) cover the whole listed batch.
**Do not extend a batch approval to files not in the original list.**

This matches CCGS's existing Collaboration Protocol in `CLAUDE.md` — AGENTS.md
just names the protocol token.

---

## 3. Evidence Rule

Every claim in a PR, audit, or status report must cite **file:line + verification
method**.

- ❌ "Save integrity is handled."
- ✅ "Save integrity handled in `src/core/save/SaveSerializer.cs:42` via HMAC
  signature; verified by `tests/unit/save/SaveSerializer_Tamper_test.cs:18`."

"Configured" never means "I think I configured it." Open the file, paste the line.

---

## 4. Source-of-Truth Rule

When two sources disagree, the canonical source wins. Canonical sources by domain:

| Domain | Canonical Source |
|---|---|
| Universal workflow | `docs/brian-system-prompt-v4-6.md` |
| Project behavioral contract | This file (`AGENTS.md`) |
| Architecture decisions | `DECISIONS.md` (append-only) |
| Engine version + API | `docs/engine-reference/unity/VERSION.md` |
| Game design | `design/gdd/*.md` (per-system GDDs) |
| Tech preferences | `.claude/docs/technical-preferences.md` |
| Path-scoped code rules | `.claude/rules/*.md` |
| Ritualized behavior | `.claude/skills/*/SKILL.md` |
| Agent routing | `CLAUDE.md` + `.claude/agents/*.md` |

If a GDD and a DECISION disagree, update one of them; don't paper over it.
Usually the DECISION wins (it's the lock), and the GDD gets amended.

---

## 5. Output Contract

Every substantial response should end with:

- **What changed** (files touched, with paths)
- **What's next** (one sentence, concrete)
- **Blockers** or open questions (if any)

Don't narrate internal deliberation. Don't restate what the diff already shows.
See CCGS's `CLAUDE.md` tone-and-style guidance — this just reinforces it.

---

## 6. Tier-Gated Work Policy

Gravenspire has four scope tiers. Work is gated by tier. **Do not implement Tier
N+1 features during a Tier N sprint** without an explicit tier-transition
decision appended to `DECISIONS.md`.

| Tier | Scope | Active Surfaces | RED_TEAM Mode | CI |
|---|---|---|---|---|
| **T1** | Vertical slice, offline single-player | combat, faction sim (local), save/load | **Skip** — use 4-question PR check | None — local gate only |
| **T2** | Co-op 2–6 players, FishNet introduced | + netcode | Full RED_TEAM required on `src/networking/**` PRs only | GameCI Unity Test Runner |
| **T3** | Persistent server, ~10 CCU, LLM dialogue live | + server auth, LLM moderation, save versioning | Full RED_TEAM on netcode / faction sim / save / LLM | + Windows + macOS build matrix |
| **T4** | Full vision, 50 CCU, all factions, deep LLM | All surfaces | Full RED_TEAM on all risky subsystems | Full matrix |

**Current tier: T1** (per `DECISIONS.md` D003).

---

## 7. Pre-PR Adversarial Review (4 Questions)

Before opening or requesting review on any PR, answer these in the PR
description:

1. **What does this PR claim to do?** (one sentence)
2. **What evidence proves it works?** (test file:line, playtest note path,
   screenshot path)
3. **What's the blast radius if this is wrong?** (which systems break, which
   data corrupts)
4. **What did I not test?** (be honest — name the untested edge case)

Tier-gated RED_TEAM signoff (see §6) is a separate block in the PR template.

Triggering skill: `/pre-pr-review`.

---

## 8. RED_TEAM Reference

Adversarial review protocol lives in `RED_TEAM.md` (9-section template) and
`RED_TEAM_RUBRIC.md` (scoring guide). Run it when the tier table in §6
requires it. Save outputs to `docs/audits/RED_TEAM_[YYYY-MM-DD]_[scope].md`.

---

## 9. LESSONS Promotion Ritual

New lesson lives in `tasks/lessons.md`. Tag taxonomy:

- **[GLOBAL]** — applies across all Brian projects (also worth pushing up to
  the universal system prompt)
- **[CI]** — build/test pipeline
- **[TEST]** — testing discipline
- **[TYPES]** — C# type / generics / nullability
- **[SCOPE]** — tier discipline, scope creep
- **[DATA]** — data-driven config, ScriptableObjects, Addressables
- **[DETERMINISM]** — faction sim, physics, replay integrity
- **[SAVE]** — save format, versioning, migration
- **[LLM]** — prompt hardening, output moderation, latency
- **[NETCODE]** — FishNet, replication, authority
- **[BALANCE]** — combat formulas, economy

**Promotion path:** lesson repeats 2–3 times → promoted to `CLAUDE-patterns.md`
(cross-cutting) or a specific `.claude/rules/*.md` file (path-scoped). Mark the
original lesson entry with `[PROMOTED to <target> YYYY-MM-DD]`.

Triggering skill: `/update-memory-bank`.

---

## 10. Session Self-Assessment

At the end of any session that touched more than one file, include:

- Files modified (full paths)
- Decisions made (D-number if one was added)
- Lessons worth logging (even if not formally added yet)
- What I got wrong or corrected mid-session

This is how lessons actually make it to `tasks/lessons.md`.

---

## 11. Secrets and PII (Always On)

Gravenspire has a much smaller PII surface than a HIPAA system, but:

- **Never commit** Steam API keys, signing certs, server auth tokens
- **Never log** player email / username / IP at severity `INFO` or above — use
  `DEBUG` and scrub in production builds
- Any LLM call that includes player-authored chat must **sanitize** (strip
  control chars, cap length) before dispatch
- Save files may contain player-chosen names — treat as user-controllable
  strings (length-bound, validate on load)

---

## 12. Pre-Build Verification (Inherited Lesson from clinic-notes 2026-04-10)

Before any Steam upload (or for T1, any playtest handoff), validate:

- ProjectSettings: build target, scripting backend, IL2CPP/Mono, color space,
  graphics API list
- Addressables groups: all referenced groups exist and build without errors
- Version stamp: `Application.version` matches the intended git tag
- (T3+) Server config: required env vars present in the target deployment

**Checklist lives in `production/pre-build-checklist.md`.** This is the
Gravenspire analogue of clinic-notes's missing pre-deploy env-vars check.
Every item produces evidence (file:line, screenshot, console output), not
self-report.

---

## 13. Tool / File Writes

Honor the CCGS Collaboration Protocol (`CLAUDE.md` §Collaboration Protocol).
This file does not override it — just names the approval token (`EDIT_OK`).

---

## 14. When AGENTS.md and System Prompt Conflict

**System prompt wins. Always.** File a lesson tagged `[GLOBAL]` so the conflict
propagates up to the universal prompt if the system-prompt rule is wrong for
all projects.
