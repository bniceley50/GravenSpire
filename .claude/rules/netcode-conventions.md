---
paths:
  - "src/networking/**"
  - "Assets/Scripts/Networking/**"
---

# Netcode Conventions (Security & Integrity)

**Orthogonal to** `.claude/rules/network-code.md`, which covers code style,
replication strategy, and bandwidth budgets. **This file covers server
authority and anti-cheat integrity.**

**Active tier:** T2+. This rule is inert during T1 (no networking code exists
yet per `DECISIONS.md` D003).

---

## Server Authority (Non-Negotiable at T2+)

- All gameplay-critical state lives on the server. The client is a render +
  input target.
- Client may **predict** (for local responsiveness) but **must reconcile**
  with server state.
- **No client-authored values enter server state without validation** —
  range, rate, and context must all check out.

## Validation

- Every inbound packet validates:
  - **Size** (bounded max)
  - **Field ranges** (no NaN, no out-of-domain)
  - **Action legality** (can this player do this right now — alive, in
    range, not stunned, has resource)
  - **Rate limit** (how many of this action per second per player)
- Rejection path: log + drop. Do **not** echo validation failure details to
  the client (information leak).

## Anti-Cheat Surfaces

- **Hit registration:** server replays the shot using server-side state;
  client-reported hits are suggestions, not facts.
- **Movement:** server rate-caps position delta; teleport-style jumps are
  flagged and logged.
- **Item grants:** server-side only. Client cannot add to inventory.
- **Reputation changes:** server-side only. Every change is audit-logged
  (see `RED_TEAM.md` §8).

## Audit Logging

- High-value actions (item grants, rep changes above threshold, state
  changes that grant unlocks) write to an append-only audit log with:
  `actor, target, action, delta, cause, server_timestamp, server_tick`.
- Log retention policy is a design-level decision at T3 entry.

## See Also

- `.claude/rules/network-code.md` — network code style (existing)
- `RED_TEAM.md` §7 — server-authoritative validation rubric
- `SECURITY.md` — threats #2, #4, #5, #6
- `DECISIONS.md` D002 (FishNet deferred to T2)
