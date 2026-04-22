---
paths:
  - "src/ai/dialogue/**"
  - "Assets/Scripts/AI/Dialogue/**"
---

# LLM Moderation Rules

**Active tier:** T3+. Inert during T1–T2 (all NPC dialogue is templated per
`DECISIONS.md` D004).

**Orthogonal to** `.claude/rules/ai-code.md`, which covers AI code style —
AI update budget, behavior trees, debuggability. **This file covers LLM
output safety and prompt integrity.**

---

## Prompt Hardening

- All prompts that incorporate player input use a **fixed template with
  placeholder slots**. Player input **never** concatenates directly into
  the system prompt.
- Strip control characters and cap length (e.g., 200 chars) on player input
  **before** inserting into prompt.
- **Never include** secrets, server state, or out-of-character world data
  in prompts sent to the LLM.
- Never include other players' identifying information in a prompt shared
  with the LLM vendor.

## Output Moderation

- Every LLM response passes through a **moderation API call** (vendor TBD
  at T3 entry gate — see D004 revisit triggers) **before** display to the
  player.
- **Moderation failure path:** fall back to templated response; log the
  event (prompt + response + reject reason) for offline review.
- Retention of moderated-reject events is a design-level decision at T3
  entry (see `SECURITY.md` Open Decisions).

## Fallback

- **LLM call timeout** (e.g., 3s): fall back to templated response; queue
  LLM retry for background refresh.
- **LLM vendor outage:** 100% templated fallback; feature-flag switch to
  disable LLM dialogue entirely if outage persists beyond threshold.
- Fallback must be **indistinguishable** from normal templated dialogue to
  the player — no "LLM failed" visible error.

## Cost & Latency Guardrails

- Per-session LLM call budget (hard cap; design-level value set at T3 entry).
- **Cache LLM responses** keyed on `(npc_id, quest_state, player_rep_bucket)`
  to avoid regenerating the same response for the same context.
- Cache invalidation on quest-state transition or rep-bucket change only —
  do not invalidate on minor player inputs.

## See Also

- `.claude/rules/ai-code.md` — AI code style (existing)
- `RED_TEAM.md` §3 (moderation evidence), §6 (LLM as client-authority risk)
- `SECURITY.md` threat #3
- `DECISIONS.md` D004 (LLM scope: templated default, 5–10 NPCs at T3)
