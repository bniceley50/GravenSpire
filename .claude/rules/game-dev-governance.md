---
paths:
  - "**/*"
---

# Game Dev Governance Rules

Cross-cutting governance rules derived from `AGENTS.md` that don't fit any
single path scope. Path set to `**/*` because these apply project-wide.

---

## Engine Version Awareness

- **Unity 6.3 LTS is pinned.** Do not suggest APIs that are deprecated in
  6.2+ (e.g., `VisualElement.transform`, URP `SetupRenderPasses`) without
  cross-referencing `docs/engine-reference/unity/VERSION.md`.
- LLM knowledge cuts off around Unity 6.0; treat 6.1–6.3 APIs as
  **unverified** unless explicitly documented in the engine reference.
- **BIRP is not an option.** URP only (HDRP blocked by D001 unless a photoreal
  pivot occurs, which is not planned).

## Code Style Gate

- **Tier 1:** `dotnet format --verify-no-changes` must pass locally before any
  PR. No CI yet (see `AGENTS.md` §6).
- **Tier 2+:** same gate, run in CI (GameCI).

## Scene Discipline

- Never commit a `.unity` scene with unsaved `Scene is dirty` state — save
  first, then inspect the diff before staging.
- Scene merge conflicts: use Unity Smart Merge; do **not** hand-edit the
  YAML.
- One scene edit per PR when possible — scene diffs are hard to review and
  easy to corrupt.

## Tier Discipline

See `AGENTS.md` §6. Core rules:

- **Do not implement Tier N+1 features during Tier N.** If you notice
  cross-tier creep in a PR, file it as a `[SCOPE]` lesson in
  `tasks/lessons.md`.
- Tier transitions require a new D-entry in `DECISIONS.md`.

## Evidence Discipline

- Any "done" claim requires either a passing test or a file:line reference
  (see `AGENTS.md` §3).
- "Configured" alone is not evidence. Cite the line.
- Before a PR lands: every claim in the PR description has a traceable
  evidence path.

## Dependency Discipline

- New libraries are added to `.claude/docs/technical-preferences.md`
  Allowed Libraries list **only** when the system that needs them starts
  active work. No speculative installs. (See D002 for the canonical
  example — FishNet is named but not yet approved for install.)

## See Also

- `.claude/rules/netcode-conventions.md` — T2+ networking integrity
  (security, server authority)
- `.claude/rules/save-integrity.md` — T1+ save format rules (HMAC, versioning)
- `.claude/rules/llm-moderation.md` — T3+ LLM output policy
- `.claude/rules/network-code.md` — network code style (existing; orthogonal
  to `netcode-conventions.md`)
- `.claude/rules/ai-code.md` — AI code style (existing; orthogonal to
  `llm-moderation.md`)
