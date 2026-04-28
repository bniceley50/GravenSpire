# Architecture Boundary Tests

Use this tree for static checks and lightweight analyzers that enforce ADR and T1 scope boundaries.

Sprint 1 forbidden-pattern coverage should include:

- No FishNet, networking placeholders, replicated combat authority, server validation, account identity, prediction, or lag compensation in T1 Combat Core code.
- No PvP, duels, friendly fire, companions, Sister Elara combat behavior, Warrior, Enchanter, live LLM calls, or server combat state.
- No `combat_actor_id` used as XP identity, save identity, dedupe key, or source lifecycle identity.
- No Character Progression dependency on Combat-expanded XP metadata.
- No Save/Load direct read of guarded downstream state before declared barriers.
- No generic all-consumer `ProgressionBaselineSnapshot` handoff to Combat.
- No `visible_level`, XP progress, `spell_eligibility_tier`, spell ids, or UI read-model fields inside `CombatProgressionBaselineSnapshot`.
- No first-load synthesis or re-materialization of missing required first-save progression state.

Start with deterministic grep/static scans if sufficient. Promote to a Roslyn analyzer only if simple scans cannot reliably catch the forbidden pattern.
