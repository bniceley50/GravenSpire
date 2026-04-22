# SECURITY.md — Gravenspire Threat Model

Game-dev adapted from clinic-notes-ai's HIPAA threat table. Rows marked
**Active** apply to the current tier. Rows marked **Tier N** become active at
that tier.

**Current tier: T1** (per `DECISIONS.md` D003).

---

## Threat Table

| # | Threat | Mitigation | Status | Tier |
|---|---|---|---|---|
| 1 | Client-side save file tampering for reputation/inventory | Server-authoritative rep state at T2+; HMAC-signed saves at T1–T2 | Designed | **Active (T1: signed save)** |
| 2 | Faction sim determinism drift across clients | Server runs sim authoritatively; clients render state only | Designed | T2+ |
| 3 | LLM NPC dialogue generating policy-violating content | Prompt hardening + output moderation API before display | **Open decision** | T3+ |
| 4 | Aimbot / speed hack on combat | Server validates hit registration; client reports are suggestions | Designed | T2+ |
| 5 | Cheat clients spawning items | All item grants server-authoritative; client cannot add to inventory | Designed | T2+ |
| 6 | Exploiting faction reputation via item duplication | Server-side audit log on rep changes (actor/target/delta/cause/ts/tick) | Designed | T3+ |
| 7 | Griefing via corpse camping | Design-level rule (safe zones, respawn timers, teleport options) | **Open decision** | T3+ |
| 8 | Account sharing / boosting | Out of scope | Deferred | T4+ |

---

## T1 Active Surfaces

**Only Threat #1 is live at T1.** Save files are local, but tamper-resistance
matters because:

- Playtesters share save files to reproduce bugs — a tampered save looks like
  a real bug report and wastes diagnostic time
- The Steam community is creative; expect save-editor forum posts on day one
  of any public playtest
- A locally-tampered save that later syncs to server (T3+) becomes a
  server-side integrity incident

**T1 mitigation:** HMAC-signed save file using a per-install key. Load path
rejects invalid signatures with a loud error; no silent fall-through to default
state. Implementation lands in `src/core/save/**` per
`.claude/rules/save-integrity.md`.

---

## Escalation Path

Any suspected exploit or vulnerability:

1. File as an issue tagged `security`
2. If HIGH severity (per `RED_TEAM_RUBRIC.md`), write an incident doc at
   `docs/audits/SECURITY_INCIDENT_[YYYY-MM-DD]_[scope].md`
3. Decide: hotfix path (`/hotfix` skill) or next-sprint fix
4. Update this table if the mitigation changes
5. File a `[GLOBAL]` or scoped lesson in `tasks/lessons.md` if the class of
   bug could recur

---

## Open Decisions (block tier entry)

- **Threat #3 (LLM moderation):** vendor, latency budget, moderated-reject
  retention policy — required at T3 entry gate
- **Threat #7 (griefing):** design-level rules for safe zones, corpse-camp
  timers, player blocking — required at T3 entry gate

---

## Related Docs

- `RED_TEAM.md` — adversarial review protocol (§6–§9 cover the security
  surfaces)
- `RED_TEAM_RUBRIC.md` — scoring guide
- `.claude/rules/save-integrity.md` — save format rules (T1+)
- `.claude/rules/netcode-conventions.md` — server-authority rules (T2+)
- `.claude/rules/llm-moderation.md` — LLM output policy (T3+)
- `DECISIONS.md` D003 (tier scope), D004 (LLM scope)
