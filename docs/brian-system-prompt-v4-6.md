# Brian's Universal System Prompt v4.6

> **PLACEHOLDER — content pending.**
>
> This file is the true root of the governance stack for all of Brian's projects.
> `AGENTS.md` extends it. **When `AGENTS.md` and this file conflict, this file wins.**
>
> **TO POPULATE:** Paste the full content from `N:\Clinic Notes AI\docs\brian-system-prompt-v4-6.md`
> into this file, replacing this placeholder entirely.
>
> The Gravenspire governance files (`AGENTS.md`, `DECISIONS.md`, `RED_TEAM.md`,
> `SECURITY.md`, `tasks/lessons.md`) all assume this file is present and complete.
> If you open a new session before this is populated, the session-start ritual will
> be incomplete — agents will still function via CCGS's `CLAUDE.md`, but the
> universal rules (EDIT_OK protocol, Build Protocol modes, Stop Digging Rule, etc.)
> will not be loaded.
>
> ## Expected contents (summary, per clinic-notes-ai precedent)
>
> - Session-start ritual: read `AGENTS.md` → `DECISIONS.md` → `PLAN.md` →
>   `tasks/lessons.md` in that order
> - 4-line Session State block at the top of every substantial response
> - EDIT_OK protocol for file writes (approval token before any Write/Edit)
> - Build Protocol with three modes: **Rapid v0** / **Beta** / **Production**
> - Decision Gates, Stop Digging Rule, Rollback Rule
> - Secrets and PII Protection (always-on)
> - Always-On Quality Gates
> - Toolchain Roles table: Claude.ai chat (planning) / Claude Code (execution) /
>   Codex (autonomous scoped tasks — **not used on Gravenspire**, per D005)
>
> ## Gravenspire-specific notes about the universal prompt
>
> - Gravenspire is **Claude Code only**. The `.codex/` workflow from clinic-notes
>   is not ported (see `DECISIONS.md` D005).
> - Build Protocol modes map to Gravenspire tiers: Rapid v0 ≈ T1 prototype, Beta ≈
>   T2 co-op, Production ≈ T3+ persistent server (see `AGENTS.md` §6).
> - The universal prompt's PII rules apply but at lower stakes than clinic-notes
>   (no HIPAA surface). See `SECURITY.md` for the Gravenspire-specific threat table.
