---
name: pre-pr-review
description: Gate check before opening a PR. Validates the PR description answers all 4 adversarial questions, invokes full RED_TEAM protocol when tier-gated (T2 netcode, T3+ risky subsystems), and runs the local code-style gate. Produces a GO / BLOCK verdict with specific blockers.
---

# /pre-pr-review

Gate check before opening a PR. Prevents "what does this claim to do"
ambiguity and enforces tier-gated RED_TEAM requirements.

## When to run

- Immediately before `gh pr create` or pushing a branch that will be PRed
- When user says "pre-pr check", "ready to open PR", or `/pre-pr-review`

## Procedure

### Phase 1 — Scope Detection

1. Run `git diff --stat main...HEAD` to list changed files.
2. Classify the change surface:
   - `src/networking/**` or `Assets/Scripts/Networking/**` → **NETCODE**
   - `src/ai/**` or `Assets/Scripts/AI/**` → **AI** (incl. faction sim, LLM)
   - `src/core/save/**` or `Assets/Scripts/**/Save/**` → **SAVE**
   - Otherwise → **GENERAL**
3. Read current tier from `DECISIONS.md` (most recent tier-defining
   D-entry).

### Phase 2 — 4-Question Check

Read the drafted PR description (passed as argument, or ask the user to
paste). Verify:

1. Claim is specific and falsifiable
2. Evidence cites file:line OR a playtest path
3. Blast radius is named (which systems, which data)
4. An untested edge case is named honestly

Any missing → **BLOCK** with a list of what's missing.

### Phase 3 — Tier-Gated RED_TEAM

Per `AGENTS.md` §6:

| Tier | Surface | RED_TEAM Required? |
|---|---|---|
| T1 | any | NO |
| T2 | NETCODE | YES |
| T2 | other | NO |
| T3+ | NETCODE / AI / SAVE | YES |
| T3+ | GENERAL | reviewer discretion |
| T4 | any risky | YES |

If required:
- Check for `docs/audits/RED_TEAM_*_[scope].md` dated within 7 days of the
  HEAD commit.
- If missing or stale → **BLOCK** and instruct the user to run the
  `RED_TEAM.md` protocol.

### Phase 4 — Code-Style Gate

Run `dotnet format --verify-no-changes`. Any output → **BLOCK** with the
diff.

## Verdict

- **GO** — all phases passed; proceed with `gh pr create`
- **BLOCK** — numbered list of blockers, each with a concrete fix action

## Output Contract

End the response with:

- **Verdict:** GO / BLOCK
- **Tier detected:** T1 / T2 / T3 / T4
- **Surface detected:** NETCODE / AI / SAVE / GENERAL
- **RED_TEAM required:** yes / no; if yes, path to existing or expected
  audit doc
- If BLOCK: numbered list of blockers

## See Also

- `AGENTS.md` §6 (tier policy), §7 (4-question check)
- `RED_TEAM.md` — adversarial protocol
- `RED_TEAM_RUBRIC.md` — scoring guide
- `.github/PULL_REQUEST_TEMPLATE.md` — the 4 questions live here too
