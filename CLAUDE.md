# Claude Code Game Studios -- Game Studio Agent Architecture

## Read First (session start ritual)

Before any task, read these in order — they define behavior, decisions, and state:

1. `docs/brian-system-prompt-v4-6.md` — universal cross-project system prompt (source of truth)
2. `AGENTS.md` — Gravenspire behavioral contract (extends the system prompt)
3. `DECISIONS.md` — locked architecture decisions (D-numbered, append-only)
4. `production/session-state/active.md` — current work state (may not exist yet)
5. `design/gdd/game-concept.md` — Gravenspire concept / pillars
6. `tasks/lessons.md` — accumulated lessons

`.claude/docs/technical-preferences.md` (loaded below via `@import`) is the
tech-spec source of truth. `AGENTS.md` governs **behavior**; technical-preferences
governs **tech choices**.

See `AGENTS.md` §4 for conflict resolution when sources disagree, §14 for
the agent's commit & push prompting obligation, and §15 for AGENTS.md vs
system-prompt precedence.

---

Indie game development managed through 48 coordinated Claude Code subagents.
Each agent owns a specific domain, enforcing separation of concerns and quality.

## Technology Stack

- **Engine**: Unity 6.3 LTS
- **Language**: C# (.NET 8+)
- **Version Control**: Git with trunk-based development
- **Build System**: Unity Build Pipeline
- **Asset Pipeline**: Unity Asset Import Pipeline + Addressables

> **Note**: Engine-specialist agents exist for Godot, Unity, and Unreal with
> dedicated sub-specialists. Use the set matching your engine.

## Project Structure

@.claude/docs/directory-structure.md

## Engine Version Reference

@docs/engine-reference/unity/VERSION.md

## Technical Preferences

@.claude/docs/technical-preferences.md

## Coordination Rules

@.claude/docs/coordination-rules.md

## Collaboration Protocol

**User-driven collaboration, not autonomous execution.**
Every task follows: **Question -> Options -> Decision -> Draft -> Approval**

- Agents MUST ask "May I write this to [filepath]?" before using Write/Edit tools
- Agents MUST show drafts or summaries before requesting approval
- Multi-file changes require explicit approval for the full changeset
- No commits without user instruction
- See `AGENTS.md` §14 for the agent's prompting obligation at approval checkpoints

See `docs/COLLABORATIVE-DESIGN-PRINCIPLE.md` for full protocol and examples.

> **First session?** If the project has no engine configured and no game concept,
> run `/start` to begin the guided onboarding flow.

## Coding Standards

@.claude/docs/coding-standards.md

## Context Management

@.claude/docs/context-management.md
