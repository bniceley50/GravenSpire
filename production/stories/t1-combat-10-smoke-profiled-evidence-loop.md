# T1-COMBAT-10 - Smoke/Profiled Evidence Loop

**Status:** Implemented + Verified; awaiting `/story-done`
**Sprint:** 1
**Priority:** Must Have
**Layer:** Gameplay / Combat Core
**Type:** Visual/Feel + Evidence Harness
**Estimate:** 2.0 days
**Manifest Version:** Sprint 1, 2026-04-28
**GDD:** `design/gdd/combat-core.md`
**Governing ADR:** None newly introduced; T1 offline tier discipline remains governed by `DECISIONS.md` D003 and Combat Core's approved D012 contract.
**Evidence:** `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl`; `tests/evidence/T1-COMBAT-10/verification.md`; `production/qa/combat/t1-combat-10-profiled-evidence-summary.md`

## Topline Result

`T1-COMBAT-10` generated quantitative profiled JSONL evidence per the held policy. Two ACs failed-as-measured:

- `H-CCOM-FEEL-01`: solo trash observed `20/20` Cleric wins, outside the `55-85%` target. Evidence: `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:1`.
- `H-CCOM-FEEL-03`: two-trash overpull observed `5/10` dangerous outcomes, below the `>=8/10` target. Evidence: `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:3`.

Production was not tuned to mask the failure. The slice review session owns the Green/Yellow/Red verdict.

## Scope

This story creates a deterministic, headless C# evidence runner that consumes production Combat Core source and fixture data:

- Compile production `src/gameplay/combat/**` through a sibling .NET 8 console project.
- Load `assets/data/combat/t1-combat-fixtures.json` using production fixture loading and validation.
- Emit prototype-compatible JSONL plus production-specific `command`, `build_sha`, and `result` fields.
- Run the required seeded scenarios: solo trash, named solo-block, two-trash overpull, med-break pacing, and dev-build structural smoke.
- Produce durable verification and slice-review summary markdown.

Source trace: `production/sprints/sprint-1.md:509`.

## Out Of Scope

- No production source changes.
- No edits to `tests/Gravenspire.Combat.Tests.csproj`.
- No edits to `assets/data/combat/t1-combat-fixtures.json`.
- No tuning pass to bring failed quantitative criteria into range.
- No human qualitative verdict and no Green/Yellow/Red slice judgment.
- No `T1-COMBAT-11` forbidden-pattern analyzer implementation.

## Composition Trace

Original held policy expected a Unity harness consuming production Combat Core. That path was abandoned after Unity 6.3 editor compilation required significant compatibility shimming for the .NET 8 production source shape, including file-scoped namespaces, record structs, `IsExternalInit`, and `System.Text.Json`.

The amended approach is a headless C# JSONL runner at `prototypes/combat-slice-T1/Harness/CombatSliceHarness.cs`. It compiles the same production Combat Core files used by the existing test bridge, loads the production T1 fixture package, and emits quantitative evidence without a Unity temporary project.

Held policy is reframed for this story: `T1-COMBAT-10` generates quantitative profiled evidence; the slice review session handles whatever playable setup the human wants for qualitative evaluation.

## Acceptance Criteria Trace

| AC | Status | Observed Result | Evidence |
| --- | --- | --- | --- |
| `H-CCOM-FEEL-01` solo-trash success envelope | Failed-As-Measured | Target `55-85%` wins; observed `20/20` wins and `result=fail`. | `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:1`; `tests/evidence/T1-COMBAT-10/verification.md`. |
| `H-CCOM-FEEL-02` named enemy not soloable | Passed | Target `>=8/10` loss/flee outcomes; observed `9/10` loss/flee and `result=pass`. | `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:2`; `tests/evidence/T1-COMBAT-10/verification.md`. |
| `H-CCOM-FEEL-03` two-trash overpull is dangerous | Failed-As-Measured | Target `>=8/10` loss/flee/below-threshold outcomes; observed `5/10` dangerous outcomes and `result=fail`. | `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:3`; `tests/evidence/T1-COMBAT-10/verification.md`. |
| `H-CCOM-FEEL-04` med-break pacing | Passed | Target `60-120s` to recover from below 35% mana to 70% mana; observed `72s`, `12` regen ticks, and `result=pass`. | `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:4`; `tests/evidence/T1-COMBAT-10/verification.md`. |
| `H-CCOM-ART-02` no global combat visual state | Structural-Pass | Static structural scan found `0` forbidden presentation matches. | `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:5`; `tests/evidence/T1-COMBAT-10/verification.md`. |
| `H-CCOM-AUD-01` audio hooks only | Structural-Pass | Static structural scan found `0` Combat-owned audio playback matches. | `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl:5`; `tests/evidence/T1-COMBAT-10/verification.md`. |
| `H-CCOM-SCOPE-01` T1 strict scope | Passed | Production source unchanged from `6875672`; harness scope grep returned zero matches for the T1 deny list. | `tests/evidence/T1-COMBAT-10/verification.md`. |

## Runnable Evidence

Harness command:

```powershell
dotnet run --project prototypes/combat-slice-T1/Harness/CombatSliceHarness.csproj
```

Result: command completed harness execution and wrote `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl`; process exit code was `1` because the harness correctly returns non-zero when measured ACs fail.

Regression command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"
```

Result: PASS, 133 total, 133 passed, 0 failed.

## Story Status

`T1-COMBAT-10` is implemented and verified, but not yet closed. `/story-done` should mark it `Complete` with the verdict `COMPLETE WITH NOTES - quantitative ACs failed; metrics surfaced for slice review`.

## Blockers / Carried Forward

- Slice review session triggers immediately after closure and before `T1-COMBAT-11`, per Path B decision.
- `T1-COMBAT-11` holds until the slice review verdict is issued.
- Any fixture or balance changes required by the failed quantitative criteria belong to a later tuning story or sprint-1.5 plan, not this story.

## Completion Notes

**Planned `/story-done` verdict:** COMPLETE WITH NOTES - quantitative ACs failed; metrics surfaced for slice review.
**Quantitative failures:** `H-CCOM-FEEL-01` and `H-CCOM-FEEL-03` failed-as-measured.
**Production tuning:** Not performed.
**Next discrete activity after closure:** slice review session, not `/dev-story T1-COMBAT-11`.
