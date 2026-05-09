# T1.5-COMBAT-05 Verification

## Story

`production/stories/t1-5-combat-05-profiled-rerun-evidence-summary.md`

## Evidence Provenance

Command:

```powershell
dotnet run --project prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj
```

Result: PASS. The rerun wrote
`tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl` with five rows.

Commit / evidence chain:

| Commit | Role | Verification status |
| --- | --- | --- |
| `be1c3ed31bbe4be44c3923ba44e03b4ac6fb62cd` | Parent and evidence-capture SHA recorded in JSONL `build_sha` | Intentional Approach A state: the JSONL was generated before the implementation commit and names the pre-existing head. This is not SHA drift. |
| `960a1481840f8c178cf840b0de52481cd4cff493` | T1.5-COMBAT-05 implementation commit | Lands harness evaluator, fixture label, regression test, story, JSONL, verification, and QA summary in one batch. This commit intentionally differs from JSONL `build_sha`; the table records the distinction instead of regenerating JSONL solely to chase commit identity. |

JSONL provenance proof:

| Row | Scenario | JSONL `build_sha` | Evidence |
| --- | --- | --- | --- |
| 1 | `SoloTrash_EvenCon_T1` | `be1c3ed` | `tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl:1` |
| 2 | `NamedSoloBlock_T1` | `be1c3ed` | `tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl:2` |
| 3 | `TwoTrash_Overpull_T1` | `be1c3ed` | `tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl:3` |
| 4 | `MedBreak_Pacing_T1` | `be1c3ed` | `tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl:4` |
| 5 | `DevBuild_StructuralSmoke_T1` | `be1c3ed` | `tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl:5` |

## Implementation Evidence

| Surface | Status | Evidence |
| --- | --- | --- |
| Harness story/timestamp defaults | Updated | `prototypes/combat-slice-T1/Harness/CombatSliceHarness.cs:20` and `prototypes/combat-slice-T1/Harness/CombatSliceHarness.cs:21` set `T1.5-COMBAT-05` and `2026-05-09T00:00:00-04:00`. |
| FEEL-01 evaluator | Updated | `prototypes/combat-slice-T1/Harness/CombatSliceHarness.cs:110` requires `18-20` wins out of `20` plus ending pressure below either `80%` health or `60%` mana. |
| Fixture metadata | Updated | `assets/data/combat/t1-combat-fixtures.json:3` preserves the T1.5-COMBAT-03 fixture-set identity pinned by the overpull isolation regression, `assets/data/combat/t1-combat-fixtures.json:4` expands the source span through Sprint 1.5 capstone scope, and `assets/data/combat/t1-combat-fixtures.json:511` records the `90-100%` clean-state target. |
| Regression guard | Added | `tests/integration/gameplay/combat/combat_fixture_loading_test.cs:60` locks `SoloTrash_EvenCon_T1.requiredOutcome`; `tests/integration/gameplay/combat/combat_fixture_loading_test.cs:66` requires `90-100%`; `tests/integration/gameplay/combat/combat_fixture_loading_test.cs:69` rejects `55-85%`. |
| Story artifact | Added | `production/stories/t1-5-combat-05-profiled-rerun-evidence-summary.md:3` marks the implementation state; `production/stories/t1-5-combat-05-profiled-rerun-evidence-summary.md:20` records the D014 deferral resolution; `production/stories/t1-5-combat-05-profiled-rerun-evidence-summary.md:40` records the no-SHA-chase rule. |

## Profiled Rerun Results

| Scenario | Target | Observed | Result |
| --- | --- | --- | --- |
| `SoloTrash_EvenCon_T1` | `18-20/20` wins and mean ending pressure below either `0.80` health or `0.60` mana. | `20/20` wins; mean ending health `0.819`; mean ending mana `0.544`; dangerous outcomes `0`. | Pass |
| `NamedSoloBlock_T1` | At least `8/10` losses or flees. | `5` losses + `3` flees = `8/10`; `2` wins; dangerous outcomes `8`. | Pass |
| `TwoTrash_Overpull_T1` | At least `8/10` dangerous outcomes. | `5` losses/deaths + `0` flees + low-threshold endings = `9/10` dangerous outcomes; `5` wins. | Pass |
| `MedBreak_Pacing_T1` | Recover from below `35%` mana to `70%` mana in `60-120s`, only on regen ticks. | `72s`; `12` regen ticks. | Pass |
| `DevBuild_StructuralSmoke_T1` | No Combat Core global visual state or Combat-owned audio playback objects. | `0` structural matches. | Pass |

## Acceptance Criteria Evidence

| QA Case | Status | Evidence |
| --- | --- | --- |
| `QA-05-01` Full profiled harness rerun | PASS | Rerun command above exited `0`; JSONL rows at `tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl:1` through `tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl:5` cover all five required scenarios. |
| `QA-05-02` Regression suite after profiled rerun | PASS | `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` passed `160/160` on 2026-05-09. |
| `QA-05-03` Pre-commit hook proof | PASS | After staging the approved seven-file batch, `bash .githooks/pre-commit` returned `[pre-commit] OK`. |
| `QA-05-04` Summary labels quantitative outcomes only | PASS | `production/qa/combat/t1-5-combat-profiled-evidence-summary.md:10` through `production/qa/combat/t1-5-combat-profiled-evidence-summary.md:18` uses `Pass` and `Structural-Pass` result labels only. |
| `QA-05-05` No-agent-verdict guard | PASS | `rg -n "Green|Yellow|Red|verdict" tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl production/qa/combat/t1-5-combat-profiled-evidence-summary.md` returned no matches. |
| `QA-05-06` T1 scope negative pass | PASS | `rg -n -i "FishNet|\bnetworking\b|server authority|\bPvP\b|OpenAI|Anthropic|Time\.deltaTime|DateTime\.Now|DateTime\.UtcNow|endurance.*action|action-rotation|endurance.*combo|endurance.*pulse|per-ability endurance" prototypes\combat-slice-T1\Harness\CombatSliceHarness.cs prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj assets\data\combat\t1-combat-fixtures.json tests\integration\gameplay\combat\combat_fixture_loading_test.cs` returned no matches. |

## Final Hygiene

Commands:

```powershell
dotnet run --project prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"
git diff --check
rg -n "Green|Yellow|Red|verdict" tests\evidence\T1.5-COMBAT-05\profiled-combat-slice.jsonl production\qa\combat\t1-5-combat-profiled-evidence-summary.md
rg -n -i "FishNet|\bnetworking\b|server authority|\bPvP\b|OpenAI|Anthropic|Time\.deltaTime|DateTime\.Now|DateTime\.UtcNow|endurance.*action|action-rotation|endurance.*combo|endurance.*pulse|per-ability endurance" prototypes\combat-slice-T1\Harness\CombatSliceHarness.cs prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj assets\data\combat\t1-combat-fixtures.json tests\integration\gameplay\combat\combat_fixture_loading_test.cs
git add -- production/stories/t1-5-combat-05-profiled-rerun-evidence-summary.md prototypes/combat-slice-T1/Harness/CombatSliceHarness.cs assets/data/combat/t1-combat-fixtures.json tests/integration/gameplay/combat/combat_fixture_loading_test.cs tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl tests/evidence/T1.5-COMBAT-05/verification.md production/qa/combat/t1-5-combat-profiled-evidence-summary.md
git diff --cached --name-only
git diff --cached --check
bash .githooks/pre-commit
```

Result: PASS. Harness rerun exited `0`; regression suite passed `160/160`;
`git diff --check` returned no whitespace errors, with Git CRLF warnings only;
no-agent-verdict and T1 scope negative scans returned no matches;
`git diff --cached --name-only` returned exactly the seven approved files;
`git diff --cached --check` returned no whitespace errors; pre-commit returned
`[pre-commit] OK`.
