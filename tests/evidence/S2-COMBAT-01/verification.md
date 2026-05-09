# S2-COMBAT-01 Verification

**Story:** `production/stories/s2-combat-01-fix-init-only-property-preservation.md`
**Captured:** 2026-05-09
**Evidence-capture SHA:** `4bb4d7b2ab5e8c770ec2197b1bf329cbcdc8f385`
**Verdict:** IMPLEMENTED; pre-commit pair review completed with QA whitespace fix; awaiting commit/push authorization

## Provenance

| SHA | Role | Verification status |
| --- | --- | --- |
| `4bb4d7b2ab5e8c770ec2197b1bf329cbcdc8f385` | Parent/evidence-capture SHA for the worktree diff | `164/164` tests passed against the approved S2-COMBAT-01 batch on top of this SHA. |

No forward-reference placeholder rows are recorded. If a later approved batch
reopens this artifact after a commit exists, append the actual commit SHA as a
new row.

## Implementation Evidence

| Area | Evidence |
| --- | --- |
| Tactical ability damage copy path | `src/gameplay/combat/CombatActorStateTransitions.cs:253` enters `WithCurrentHealthAfterAbilityDamage`; the constructor result is routed through `PreserveInitOnlyRuntimeFields` at `src/gameplay/combat/CombatActorStateTransitions.cs:265`. |
| Resource copy path | `WithCurrentMana` and `WithCurrentEndurance` route into `CopyWithCastRuntime` at `src/gameplay/combat/CombatActorStateTransitions.cs:211` and `src/gameplay/combat/CombatActorStateTransitions.cs:230`; that constructor result is preserved at `src/gameplay/combat/CombatActorStateTransitions.cs:314`. |
| Shared copy path | `Copy` begins at `src/gameplay/combat/CombatActorStateTransitions.cs:350`; the shared constructor result is preserved at `src/gameplay/combat/CombatActorStateTransitions.cs:357`. |
| Preservation helper | `PreserveInitOnlyRuntimeFields` starts at `src/gameplay/combat/CombatActorStateTransitions.cs:385` and copies all ten current init-only runtime properties. |

## Acceptance Criteria

| ID | Result | Evidence |
| --- | --- | --- |
| `S2-01-01` | PASS | `test_shared_copy_transitions_preserve_init_only_runtime_properties` covers target, threat, and combat-state transitions at `tests/unit/gameplay/combat/combat_actor_state_transitions_test.cs:42`. |
| `S2-01-02` | PASS | `test_resource_copy_transitions_preserve_init_only_runtime_properties` covers mana and Endurance copy paths at `tests/unit/gameplay/combat/combat_actor_state_transitions_test.cs:58`. |
| `S2-01-03` | PASS | `test_ability_damage_copy_preserves_init_only_runtime_properties` covers tactical ability damage at `tests/unit/gameplay/combat/combat_actor_state_transitions_test.cs:72`. |
| `S2-01-04` | PASS | `test_transition_guard_covers_all_init_only_runtime_properties` enumerates every public settable `CombatActorState` property and compares it against the preservation list at `tests/unit/gameplay/combat/combat_actor_state_transitions_test.cs:29`. |
| `S2-01-05` | PASS | Local test, hygiene, pre-commit, and T1 negative-scope gates passed below. |

## Verification Commands

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"
```

Result: `Passed! - Failed: 0, Passed: 164, Skipped: 0, Total: 164`.

```powershell
git diff --check
```

Result: clean. Git emitted only line-ending normalization warnings for touched
files.

```powershell
rg -n "[ \t]+$" production\stories\s2-combat-01-fix-init-only-property-preservation.md tests\unit\gameplay\combat\combat_actor_state_transitions_test.cs tests\evidence\S2-COMBAT-01\verification.md production\session-state\active.md src\gameplay\combat\CombatActorStateTransitions.cs
```

Result: no matches.

```powershell
bash .githooks/pre-commit
```

Result: `[pre-commit] OK`.

```powershell
rg -n -i "FishNet|networking|server authority|PvP|OpenAI|Anthropic|live LLM|System\.Random|DateTime\.(UtcNow|Now)|Time\.deltaTime" src\gameplay\combat\CombatActorStateTransitions.cs tests\unit\gameplay\combat\combat_actor_state_transitions_test.cs
```

Result: no matches.

## Deferred

The wider Gemini review items remain outside this hotfix:

- `CombatInstantAbilityResolver` cooldown lifecycle audit.
- Threat-table allocation profiling.
- T2+ thread-safety review.
- Cast-tick exception/result-pattern refactor.
