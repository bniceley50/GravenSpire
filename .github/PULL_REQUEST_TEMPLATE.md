## Summary

One sentence. What does this PR claim to do?

## Type of Change

- [ ] New agent
- [ ] New skill
- [ ] New hook or rule
- [ ] Game code / feature
- [ ] Bug fix
- [ ] Documentation
- [ ] Governance / process
- [ ] Other:

## Changes

-
-
-

## Pre-PR Adversarial Review (AGENTS.md §7)

Answer all four. Specificity is the point.

1. **What does this PR claim to do?** (one sentence)
2. **What evidence proves it works?** (test file:line, playtest note path,
   screenshot path)
3. **What's the blast radius if this is wrong?** (which systems break, which
   data corrupts)
4. **What did I not test?** (name the untested edge case honestly)

## RED_TEAM Signoff (tier-gated — AGENTS.md §6)

Check the one that applies.

- [ ] **T1 / Not required** — 4-question review above is sufficient
- [ ] **T2** — PR does not touch `src/networking/**`; 4-question review sufficient
- [ ] **T2** — PR touches `src/networking/**`; RED_TEAM attached at
      `docs/audits/RED_TEAM_YYYY-MM-DD_netcode.md`
- [ ] **T3+** — PR touches risky subsystem (netcode / faction sim / save /
      LLM); RED_TEAM attached at `docs/audits/RED_TEAM_YYYY-MM-DD_[scope].md`

## Checklist

- [ ] `dotnet format --verify-no-changes` passes locally
- [ ] Tests added or updated (or WHY not is stated in the 4-question review)
- [ ] `DECISIONS.md` updated if an architecture decision was made
- [ ] `tasks/lessons.md` updated if a lesson was learned
- [ ] CCGS meta: if a new agent/skill/hook/rule landed, reference docs updated
      (agent-roster, skills-reference, hooks-reference, rules-reference)
- [ ] New agents include the Collaboration Protocol section
- [ ] New skills use the subdirectory format (`.claude/skills/<name>/SKILL.md`)
- [ ] Hooks use `grep -E` (POSIX) and fail gracefully without jq/python
- [ ] No hardcoded paths or platform-specific assumptions
