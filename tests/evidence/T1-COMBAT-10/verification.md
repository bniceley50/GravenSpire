# T1-COMBAT-10 Verification

## Harness Command

```powershell
dotnet run --project prototypes/combat-slice-T1/Harness/CombatSliceHarness.csproj
```

Result: harness executed and wrote `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl`. Exit code was `1` because the harness intentionally returns non-zero when measured quantitative ACs fail.

## Do Not Tune

Do Not Tune: production was not tuned to mask the failure; AC failures are intentional surfaced evidence, not bugs to fix in this story.

## JSONL Schema

JSONL path: `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl`.

Sample record field list:

```text
timestamp
engine_version
fixture_set_version
build_sha
test_scenario
final_state
stopped_via
pulls_completed
pulls_target
total_combat_seconds
total_downtime_seconds
avg_pull_seconds
med_breaks
auto_swings
hostile_swings
smites_channeled
heals_used
smite_of_authority_uses
bash_uses
defensive_prayer_uses
defensive_prayer_damage_prevented
unsafe_pulls
deaths
command
result
wins
losses
flees
dangerous_outcomes
mean_ending_health_ratio
mean_ending_mana_ratio
mean_win_ending_health_ratio
mean_win_ending_mana_ratio
seconds_to_70_mana
regen_ticks
structural_matches
```

## Acceptance Evidence

| AC | Status | Target | Observed | JSONL Evidence |
| --- | --- | --- | --- | --- |
| `H-CCOM-FEEL-01` | Failed-As-Measured | 20 seeded solo-trash trials produce `55-85%` Cleric wins; mean ending state below either `80%` health or `60%` mana. | `20/20` wins, `100%`; mean ending health `0.819`, mean ending mana `0.486`; `result=fail`. | `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:1` |
| `H-CCOM-FEEL-02` | Passed | 10 named solo-block trials produce at least `8/10` losses or forced flees. | `5` losses + `4` flees = `9/10`; `result=pass`. | `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:2` |
| `H-CCOM-FEEL-03` | Failed-As-Measured | 10 two-trash overpull trials produce at least `8/10` loss/flee/below-threshold outcomes. | `5/10` dangerous outcomes; `9` wins, `1` death, `0` flees; `result=fail`. | `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:3` |
| `H-CCOM-FEEL-04` | Passed | Mana recovery from below `35%` to `70%` in `60-120s`, only on regen ticks. | `72s`, `12` regen ticks, `result=pass`. | `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:4` |
| `H-CCOM-ART-02` | Structural-Pass | No Combat Core global combat visual state. | Structural scan found `0` forbidden presentation matches. | `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:5` |
| `H-CCOM-AUD-01` | Structural-Pass | No Combat-owned audio playback objects. | Structural scan found `0` audio playback matches. | `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:5` |
| `H-CCOM-SCOPE-01` | Passed | No T1 scope creep into networking, PvP, live LLM, companions, future classes, account identity, server authority, or server combat state. | Production source unchanged from `6875672`; harness scope grep returned zero matches. | Commands below |

## Production Regression Check

Command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"
```

Result:

```text
Passed!  - Failed:     0, Passed:   133, Skipped:     0, Total:   133
```

## No Production Modification Check

Command:

```powershell
git diff --exit-code 6875672 -- src/ tests/Gravenspire.Combat.Tests.csproj tests/unit tests/integration
```

Result: PASS, empty diff.

## Dev-Build Smoke Structural Scan

Command:

```powershell
rg -n "Camera\.|AudioSource|Animator|MonoBehaviour|UnityEngine\.UI|UnityEngine\.UIElements" src/gameplay/combat
```

Result: PASS, zero matches.

## Harness Scope Grep

Command:

```powershell
rg -n -i "FishNet|\bnetworking\b|server authority|\bPvP\b|companion|\bWarrior\b|\bEnchanter\b|OpenAI|Anthropic|Time\.deltaTime|DateTime\.Now|DateTime\.UtcNow" prototypes/combat-slice-T1/Harness/CombatSliceHarness.cs prototypes/combat-slice-T1/Harness/CombatSliceHarness.csproj
```

Result: PASS, zero matches.

## Hook Smoke

Command:

```powershell
bash .githooks/pre-commit
```

Result:

```text
[pre-commit] OK
```

## Hygiene

Command:

```powershell
git diff --check
```

Result: PASS.

## Closure Readiness

The story is ready for `/story-done` with a `COMPLETE WITH NOTES` verdict that explicitly preserves the failed quantitative ACs for slice review. It is not a tuning story.
