# T1.5-COMBAT-05 - Profiled Rerun + Slice Evidence Summary

**Status:** Implemented + Verified; awaiting `/story-done`
**Sprint:** 1.5
**Priority:** Must Have
**Layer:** Gameplay / Combat Feel Evidence
**Type:** Profiled QA Evidence
**Estimate:** 1.25 days
**Manifest Version:** Sprint 1.5, 2026-05-09
**GDD:** `design/gdd/combat-core.md`
**Governing Decisions:** `DECISIONS.md` D014; `docs/architecture/adr-0006-endurance-resource-model.md`
**Evidence:** `tests/evidence/T1.5-COMBAT-05/verification.md`

## Scope

Rerun the profiled combat-slice harness after Endurance, FEEL-03 tuning,
FEEL-01 target revalidation, and T1-COMBAT-11. Preserve quantitative evidence
only; no agent-authored qualitative feel call is added by this story.

This story also resolves the D014 machine-readable target deferral from
`T1.5-COMBAT-04`: the harness evaluator and the
`SoloTrash_EvenCon_T1.requiredOutcome` fixture metadata now use the current
`90-100%` clean-state FEEL-01 target while preserving the mean ending-state
pressure clause.

Source trace: `production/sprints/sprint-1-5.md:293` through
`production/sprints/sprint-1-5.md:331`; QA trace:
`production/qa/plans/qa-plan-sprint-1-5-20260506.md:173` through
`production/qa/plans/qa-plan-sprint-1-5-20260506.md:188`.

## Out Of Scope

- No fixture or formula tuning to make results pass.
- No Green/Yellow/Red or equivalent human qualitative sprint verdict.
- No changes to Combat Core production logic.
- No changes to GDD, ADR, or DECISIONS status.
- No sprint-status or active-session closure routing updates during the
  implementation batch.
- No post-commit JSONL regeneration solely to chase the implementation commit
  SHA. The verification artifact records JSONL build SHA provenance explicitly.

## Implementation Trace

- `prototypes/combat-slice-T1/Harness/CombatSliceHarness.cs` now evaluates
  `SoloTrash_EvenCon_T1` against D014's clean-state FEEL-01 target: `18-20`
  wins out of `20` and mean ending pressure below either `80%` health or `60%`
  mana.
- `assets/data/combat/t1-combat-fixtures.json` updates the source-document span
  and `SoloTrash_EvenCon_T1.requiredOutcome` label to the current D014 target.
  The fixture-set version remains the T1.5-COMBAT-03 set identity because the
  overpull isolation regression pins that version while this story changes
  labels and evidence, not the actor/encounter fixture topology.
- `tests/integration/gameplay/combat/combat_fixture_loading_test.cs` locks the
  machine-readable fixture label to the D014 target and rejects the superseded
  `55-85%` label.
- `tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl` records the
  profiled rerun.
- `production/qa/combat/t1-5-combat-profiled-evidence-summary.md` summarizes
  quantitative outcomes only.

## Acceptance Criteria Coverage

| QA Case | Status | Evidence |
| --- | --- | --- |
| `QA-05-01` Full profiled harness rerun | Covered | `tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl` records solo trash, named solo-block, two-trash overpull, med-break pacing, and structural smoke rows. |
| `QA-05-02` Regression suite after profiled rerun | Covered | `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` passes; count is recorded in verification. |
| `QA-05-03` Pre-commit hook proof | Covered | `.githooks/pre-commit` output is recorded in verification. |
| `QA-05-04` Summary labels quantitative outcomes only | Covered | `production/qa/combat/t1-5-combat-profiled-evidence-summary.md` uses pass / failed-as-measured / structural-pass labels only. |
| `QA-05-05` No-agent-verdict guard | Covered | Verification records a grep guard over JSONL and summary for prohibited verdict words. |
| `QA-05-06` T1 scope negative pass | Covered | Verification records a focused static grep for T1-excluded networking, PvP, live LLM, server-authority, and action-rotation Endurance language. |

## Runnable Evidence

```powershell
dotnet run --project prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"
bash .githooks/pre-commit
```

## Story Status

`T1.5-COMBAT-05` is implemented and verified, awaiting `/story-done`.

## Blockers / Carried Forward

- Any future qualitative sprint verdict remains Brian-owned and out of this
  implementation batch.
- If later review finds that the quantitative metrics require another tuning
  pass, remediation belongs in a separate follow-up story rather than this
  evidence story.
