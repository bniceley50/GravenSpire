# NPC System Review Log

## Review — 2026-04-24 (re-entry #2, same day) — Verdict: APPROVED
Scope signal: none required for close-out
Specialists: none this pass (targeted re-review against updated repo state)
Blocking items: 0 | Recommended: 0
Summary: All three prior findings resolved. H-NPC-SL-04 now covers valid NPC hydration, pre-SessionResume readiness, hydrated-record catch-up, fixture-value accessor checks, and no ZoneActiveEvent gameplay enable before readiness. Dependency drift resolved in npc-system.md quick reference and systems-index.md dependency map. H-NPC-F1 now tests both default 60s and safe-range-minimum 30s quanta, matching the registry [0, 20160] output range.
Prior verdict resolved: Yes — re-review #1 findings closed by the approved NPC close-out patch.
Next steps: Continue with Day/Night Cycle as the next MVP Core system per systems-index.md.

## Review — 2026-04-24 (re-entry #1, same day) — Verdict: NEEDS REVISION
Scope signal: S
Specialists: none this pass (targeted re-review)
Blocking items: 1 | Recommended: 2
Summary: The major World Structure and Save/Load contracts were coherent, but three targeted issues remained. P1: NPC System was a hard Save/Load client yet lacked a positive valid-hydration/readiness acceptance criterion. P2: dependency summary text still omitted Save/Load in the NPC quick reference and systems-index dependency map prose. P3: npc_schedule_catchup_steps had registry output_range [0, 20160], but the GDD wording and H-NPC-F1 only tested the default 60s quantum.
Prior verdict resolved: Partial — prior structural blockers were resolved; new acceptance-coverage and drift findings surfaced.

## Review — 2026-04-24 — Verdict: NEEDS REVISION
Scope signal: M
Specialists: none this pass (independent Codex review against approved design stack)
Blocking items: 2 | Recommended: 3
Summary: First NPC System review found the design direction sound but not ready for approval. Blocking items were ADR-tba-5 resolution coherence with World Structure and NPC persistence contract coherence with approved Save/Load. Advisory items covered dependency/index drift, registry/formula range clarity, and active.md staleness. Patch 1 resolved the two blockers plus two advisories; active.md staleness remained explicitly out of scope under the standing guardrail.
Prior verdict resolved: No — first review required revision before approval.
