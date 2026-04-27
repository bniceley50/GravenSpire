# Combat Core Review Log

## Review - 2026-04-25 - Verdict: APPROVED

Scope signal: L
Specialists: none in this Codex pass; local full design-review criteria applied because subagent delegation was not explicitly requested.
Blocking items: 0 | Recommended: 0

Summary: Re-review #3 APPROVED after the second blocker revision. All seven prior blocker groups were verified resolved from disk: runtime combat ids are transient only, death handoff uses stable source refs and a `death_context_id`, Combat-owned pause is closed locally, social assist/pull contracts are data-complete and deterministic, cross-system ACs now test Combat-side boundaries, threat thresholds cover edge cases, and `Cleric_Mid_T1` fixture math is coherent at 140 HP / 180 mana.

Evidence: `design/gdd/combat-core.md:103` forbids persisting `combat_actor_id`; `design/gdd/combat-core.md:184` defines `death_context_id` creation, lifetime, dedupe purpose, and World Structure consumption; `design/gdd/combat-core.md:194` defines Combat-owned pause behavior; `design/gdd/combat-core.md:121` and `design/gdd/combat-core.md:122` define social assist fields, filters, radius, LoS, and deterministic order; `design/gdd/combat-core.md:684` verifies threat edge cases; `design/gdd/combat-core.md:393` defines `Cleric_Mid_T1` as level 5, 140 HP, 180 max mana; `design/gdd/combat-core.md:859` records 52 criteria, 46 ordinary T1-blocking, 6 fixture-gated T1-blocking, and 0 advisory-at-T1.

Prior verdict resolved: Yes - re-review #3 closed the blockers from re-review #2.

Standing follow-ups: update Save/Load to mirror Combat's persistence whitelist before save-system implementation; mirror or supersede Combat's death payload contract in World Structure / Death & Corpse Recovery work; resolve the cross-doc global pause policy in a later coordinated batch. These are downstream synchronization items, not Combat Core approval blockers.

## Review - 2026-04-27 - Verdict: APPROVED

Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, ui/ux reviewer, gameplay-programmer, creative-director synthesis.
Blocking items: 0 | Recommended: 3

Summary: D012 amendment re-review APPROVED after Combat Core was reopened only for the pinned combat-feel amendment. Six specialist passes found no blockers: Attack is now a first-class player actor state that pull does not auto-enable; tactical Cleric instants are specified as profile-contract shape rather than Combat-owned numeric constants; Attack ON state is exposed to Layer 1 HUD without presentation ownership; required Attack edge cases are covered; Class Design reverse-listing is explicit; and the approved `PlayerKillCreditEvent` / same-tick kill-resolution discipline remains unchanged.

Evidence: D012 requires Attack toggle, tactical Cleric instants, and explicit Attack ON state in `DECISIONS.md:339` and `DECISIONS.md:353`; D012 preserves the approved kill-credit payload in `DECISIONS.md:361`. Pinned validation evidence is recorded in `production/prototypes/combat-feel-report.md:196`, with JSONL runs `prototypes/combat-feel/Logs/playtest-20260426-204721.log:1` and `prototypes/combat-feel/Logs/playtest-20260426-205508.log:1` both showing Unity `6000.3.14f1`, `5/5` pulls, `5` med breaks, `0` unsafe pulls, and `0` deaths. Combat Core implements Attack as explicit state in `design/gdd/combat-core.md:113`, tactical instants as profile contract in `design/gdd/combat-core.md:148`, HUD state exposure in `design/gdd/combat-core.md:582`, required edge cases in `design/gdd/combat-core.md:436-440`, Class Design reverse-listing in `design/gdd/combat-core.md:479`, and unchanged `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)` in `design/gdd/combat-core.md:194`. Amendment landed before review in commit `88b1955`.

Recommended follow-ups: (1) clean the pre-D012 Player Fantasy residue that implied auto-attack starts from body pull; completed in the approval metadata sync. (2) make the Layer 1 HUD systems-index row explicitly name the Attack on/off plus Attack ON visual-state signal; completed in the approval metadata sync. (3) during implementation, split `H-CCOM-AA-03` and `H-CCOM-HUD-04` into table-driven subcases for each forced-off and no-misleading-HUD-pulse condition.

Prior verdict resolved: Yes - the prior 2026-04-25 Combat Core approval remained valid; this review closed the D012 amendment gate added by the pinned combat-feel validation.
