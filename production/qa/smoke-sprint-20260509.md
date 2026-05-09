# Smoke Check Report — Sprint 1.5

**Date**: 2026-05-09
**Mode**: sprint
**Platform**: none
**Scope**: Sprint close-out smoke gate
**Sprint**: Sprint 1.5 — T1 Combat Feel Correction
**Repo Head During Smoke**: `7620c28`
**Sprint Status Source**: `production/sprint-status.yaml` remains Sprint 1.5 at `head: "caea662"` by rollover convention
**Verdict**: PASS WITH WARNINGS

## Environment

| Check | Result | Evidence |
|---|---|---|
| Test directory | FOUND | `tests/` exists. |
| CI workflow | NOT CONFIGURED | `.github/workflows/` is absent; expected for T1 local-gate work. |
| Engine | Unity 6.3 LTS + C#/.NET 8 | `.claude/docs/technical-preferences.md` reports Unity 6.3 LTS and C#/.NET 8. |
| Unity editor | FOUND | `C:\Program Files\Unity\Hub\Editor\6000.3.14f1\Editor\Unity.exe` exists. |
| Production Unity shell | NOT IMPLEMENTED | No `ProjectSettings/`, no `Packages/manifest.json`, and no production `Assets/**/*.unity`. `tests/README.md:63` says the Unity runner surface cannot pass until the Unity project shell exists. |
| Smoke checklist | FOUND | `tests/smoke/critical-paths.md`. |
| QA plan | FOUND | `production/qa/plans/qa-plan-sprint-1-5-20260506.md`. |

## Automated Tests

| Gate | Status | Evidence |
|---|---|---|
| Combat test suite | PASS | `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` passed `164/164`. |
| Pre-commit | PASS | `bash .githooks/pre-commit` returned `[pre-commit] OK`. |
| Profiled harness rerun | PASS | `dotnet run --project prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj` exited `0`; all five scenarios reported `result=pass`. |
| Unity EditMode / PlayMode | NOT CHECKED | Unity project shell is not present; `tests/README.md:63` documents this as not runnable yet. |

## Evidence Coverage

| Story | Type | Evidence | Coverage Status |
|---|---|---|---|
| `T1.5-COMBAT-00` Endurance contract lock | Design/Contract | `tests/evidence/T1.5-COMBAT-00/verification.md` | COVERED |
| `T1.5-COMBAT-01` Endurance state, persistence, HUD signal | Integration | `tests/evidence/T1.5-COMBAT-01/verification.md` | COVERED |
| `T1.5-COMBAT-02` Physical instant conversion | Logic + Integration | `tests/evidence/T1.5-COMBAT-02/verification.md` | COVERED |
| `T1.5-COMBAT-03` FEEL-03 overpull tuning | Config/Data + Profiled Feel | `tests/evidence/T1.5-COMBAT-03/verification.md` | COVERED |
| `T1.5-COMBAT-04` FEEL-01 target revalidation | Design/Contract | `tests/evidence/T1.5-COMBAT-04/verification.md` | COVERED |
| `T1-COMBAT-11` Forbidden-pattern compliance scan/analyzer | Static/Integration | `tests/evidence/T1-COMBAT-11/verification.md` | COVERED |
| `T1.5-COMBAT-05` Profiled rerun + slice evidence summary | Profiled QA Evidence | `tests/evidence/T1.5-COMBAT-05/verification.md` | COVERED |

**Coverage Summary**: 7 covered, 0 missing.

## Manual Smoke Checks

| Batch | Check | Status | Evidence / Reason |
|---|---|---|---|
| Batch 1 — Core stability | Game launches without crash | NOT CHECKED | Production Unity project shell is not implemented. `tests/smoke/critical-paths.md:18` scopes this to "once a Unity project shell exists." |
| Batch 1 — Core stability | Main menu or temporary dev entry scene loads | NOT CHECKED | `tests/smoke/critical-paths.md:19` scopes this to "once implemented." |
| Batch 1 — Core stability | New local session starts | NOT CHECKED | `tests/smoke/critical-paths.md:20` scopes this to "once Save/Load and World Structure entry points exist." |
| Batch 1 — Core stability | Keyboard/mouse input responds | NOT CHECKED | No playable production path exists yet. |
| Batch 2 — Sprint critical path | Sprint 1.5 combat mechanics regression | PASS | `164/164` combat tests passed; Sprint 1.5 story evidence is complete. |
| Batch 2 — Sprint critical path | Profiled combat scenarios | PASS | Harness rerun reported PASS for `SoloTrash_EvenCon_T1`, `NamedSoloBlock_T1`, `TwoTrash_Overpull_T1`, `MedBreak_Pacing_T1`, and `DevBuild_StructuralSmoke_T1`. |
| Batch 2 — Sprint critical path | Unity runtime launch/manual play path | NOT CHECKED | No production Unity shell yet. |
| Batch 3 — Persistence/performance | Save/load completes without data loss | NOT APPLICABLE | Save/Load implementation is not in this sprint. |
| Batch 3 — Persistence/performance | No new frame drops or hitches | NOT APPLICABLE | No production runtime shell or performance-sensitive scene path exists yet. |

## Warnings

- Production Unity launch/menu/session smoke checks are **NOT CHECKED** because the Unity project shell is not present. This is documented in `tests/README.md:63` and scoped in `tests/smoke/critical-paths.md:18` through `tests/smoke/critical-paths.md:20`.
- CI is not configured. This is expected under T1 local-gate policy.
- The profiled harness rerun attempted to rewrite `tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl` with `build_sha=7620c28`. That generated diff was restored because keeping it would violate the established Approach A provenance convention: the existing JSONL intentionally records the parent evidence-capture SHA, while verification artifacts document later implementation/provenance commits.
- Static negative-scope scan returned allowed-context hits only: `assets/data/combat/README.md` names forbidden T1 expansion terms as banned, and `pulse` appears in social-assist mechanics rather than Endurance action-rotation language.

## Failures

None.

## Gate Decision

### Verdict: PASS WITH WARNINGS

Sprint 1.5 is ready to proceed to `/team-qa sprint`.

To graduate this smoke gate from PASS WITH WARNINGS to PASS in a future sprint, the project needs a production Unity shell with `ProjectSettings/`, `Packages/manifest.json`, and at least one launchable scene or temporary dev entry path so Batch 1 manual launch/menu/session checks can be executed instead of marked NOT CHECKED.
