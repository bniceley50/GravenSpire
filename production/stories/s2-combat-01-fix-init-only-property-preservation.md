# S2-COMBAT-01 - Fix Init-Only Property Preservation in CombatActorState Transitions

**Status:** Complete
**Sprint:** 2
**Priority:** Must Have
**Layer:** Gameplay / Combat Core
**Type:** Hotfix / Logic
**Estimate:** 0.5 days
**Manifest Version:** Sprint 2, 2026-05-09
**GDD:** `design/gdd/combat-core.md`
**Governing Decisions:** `DECISIONS.md` D003, D012, D013, D014
**Evidence:** `tests/evidence/S2-COMBAT-01/verification.md`

## Scope

Fix the cross-vendor review finding that `CombatActorStateTransitions` manual
copy paths can drop `CombatActorState` init-only runtime fields. The story is
limited to the transition helper surface and regression coverage for the bug
class.

Affected production paths:

- Shared copy path used by `WithCombatState`, `WithTarget`, `SetThreat`,
  `AddThreat`, `ClaimHostile`, `ReleaseHostile`, and `ClearTargetAndThreat`.
- Resource copy path used by `WithCurrentMana` and `WithCurrentEndurance`.
- Tactical ability damage copy path used by `WithCurrentHealthAfterAbilityDamage`.

## Out Of Scope

- No tuning, fixture, harness, GDD, ADR, or DECISIONS edits.
- No Sprint 1.5 evidence changes.
- No networking, FishNet, server authority, PvP, live LLM, or Tier 2 runtime
  feature work.
- No resolver-lifetime, thread-safety, allocation, or result-pattern refactor
  from the wider Gemini review.
- No player-death transition behavior change; death resolution intentionally
  clears target, threat, and cast runtime per existing tests.

## Acceptance Criteria

| ID | Criterion | Evidence |
| --- | --- | --- |
| `S2-01-01` | Shared combat-state, target, and threat transitions preserve all current init-only runtime properties. | `tests/unit/gameplay/combat/combat_actor_state_transitions_test.cs` |
| `S2-01-02` | Resource transitions preserve the same init-only runtime properties while changing only mana or Endurance. | `tests/unit/gameplay/combat/combat_actor_state_transitions_test.cs` |
| `S2-01-03` | Tactical ability damage copy preserves the same init-only runtime properties while changing health/life state as before. | `tests/unit/gameplay/combat/combat_actor_state_transitions_test.cs` |
| `S2-01-04` | Regression coverage fails when a future init-only `CombatActorState` property is added without updating transition preservation coverage. | `tests/unit/gameplay/combat/combat_actor_state_transitions_test.cs` |
| `S2-01-05` | Existing combat regression suite and pre-commit gate pass. | `tests/evidence/S2-COMBAT-01/verification.md` |

## Implementation Notes

The production fix keeps existing constructor validation and threat-table copy
behavior, then funnels each manual constructor result through one preservation
helper. The helper copies all current init-only runtime fields:

- `CastRuntimeState`
- `ActiveCastId`
- `ActiveCastSpellId`
- `ActiveCastTargetCombatActorId`
- `CastProgressSeconds`
- `CastRecoveryRemainingSeconds`
- `PostureState`
- `NextRegenTickIndex`
- `LastHostileActionTickIndex`
- `CombatExitRemainingSeconds`

## Review Notes

This story originates from the Gemini cross-vendor full-codebase review on
2026-05-09. The specific "cast wipe during threat update" scenario is valid for
the shared copy path; resource and ability-damage copy paths also needed the
same non-cast runtime preservation treatment.

## Completion Notes

**Completed:** 2026-05-09
**Verdict:** COMPLETE WITH NOTES
**Criteria:** 5/5 passing
**Deferred/Untested Criteria:** None
**Test Evidence:** `tests/unit/gameplay/combat/combat_actor_state_transitions_test.cs`; `tests/evidence/S2-COMBAT-01/verification.md`
**GDD/ADR Deviations:** None
**Code Review:** `/code-review 5b8a017` completed with APPROVED WITH SUGGESTIONS; no blocking findings.
**Notes:** Review P3 suggestions are non-blocking: evidence wording was cleaned during closure, and future polish may make the preservation assertion reflection-driven from the allowlist. `production/sprint-status.yaml` is intentionally unchanged because Sprint 1.5 is closed and no Sprint 2 plan/status file exists yet; when Sprint 2 planning lands, this hotfix should be listed retrospectively as story #1.
