# T1.5-COMBAT-00 Verification

## Do Not Implement

T1.5-COMBAT-00 is contract-only; no implementation surface is touched.
Implementation begins at T1.5-COMBAT-01 once this contract is on main.

## Baseline Regression

Command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"
```

Result:

```text
Passed!  - Failed:     0, Passed:   133, Skipped:     0, Total:   133
```

## QA Case Evidence

| QA Case | Status | Evidence |
| --- | --- | --- |
| `QA-00-01` D013 Endurance decision exists | PASS | D013 is present at `DECISIONS.md:374` and has `Status: Proposed` at `DECISIONS.md:377`. |
| `QA-00-02` D013 cites slice review | PASS | D013 cites the slice-review Endurance finding at `DECISIONS.md:382` through `DECISIONS.md:385`, including `production/qa/combat/feel-review-T1-slice.md:54` through `production/qa/combat/feel-review-T1-slice.md:58` and `production/qa/combat/feel-review-T1-slice.md:80`. |
| `QA-00-03` Specific carveout artifacts are named | PASS | ADR-0006 Decision section names `Smite of Authority` at `docs/architecture/adr-0006-endurance-resource-model.md:55` and `Defensive Prayer` at `docs/architecture/adr-0006-endurance-resource-model.md:56`. |
| `QA-00-04` Quiet Endurance banned patterns are explicit | PASS | ADR-0006 enumerates five forbidden Endurance patterns at `docs/architecture/adr-0006-endurance-resource-model.md:81` through `docs/architecture/adr-0006-endurance-resource-model.md:90`. |
| `QA-00-05` Docs-only story boundary holds | PASS | No source, fixture, production-test, or test-bridge files are modified; see the empty diff command at `tests/evidence/T1.5-COMBAT-00/verification.md:67` through `tests/evidence/T1.5-COMBAT-00/verification.md:71`. |

## ADR Section Structure

Command:

```powershell
rg -n "^## (Status|Date|Context|Decision|Consequences|See Also)$" docs/architecture/adr-0006-endurance-resource-model.md
```

Result: PASS, all six required standard sections are present.

## Carveout Naming

Command:

```powershell
rg -n "Smite of Authority|Defensive Prayer" docs/architecture/adr-0006-endurance-resource-model.md
```

Result: PASS, both carveout abilities are named verbatim in the ADR-0006 Decision section.

## Banned Patterns

Command:

```powershell
rg -n "^- Endurance as an action-rotation bar|^- Endurance HUD prominence above mana|^- Pulse, combo, animation|^- Per-ability Endurance callouts|^- Combat-rotation-fast Endurance regeneration" docs/architecture/adr-0006-endurance-resource-model.md
```

Result: PASS, five explicit banned Endurance patterns are enumerated.

## No Implementation Modification Check

Command:

```powershell
git diff --name-only baec9a6 -- src/ tests/Gravenspire.Combat.Tests.csproj tests/unit tests/integration assets/
```

Result: PASS, empty diff. No production source, fixture data, production test,
or test bridge files are modified.

## DECISIONS.md Append-Only Check

Command:

```powershell
git diff baec9a6 -- DECISIONS.md
```

Result: PASS, diff adds D013 only; no prior D-entry is modified.

## Story Boundary

Tracked files changed by this batch:

```text
DECISIONS.md
docs/architecture/adr-0006-endurance-resource-model.md
production/stories/t1-5-combat-00-endurance-contract-lock.md
tests/evidence/T1.5-COMBAT-00/verification.md
```

No story files for T1.5-COMBAT-01 through T1.5-COMBAT-05 or T1-COMBAT-11 were
created.

## Hygiene

Command:

```powershell
git diff --check
```

Result: PASS.

## Hook Smoke

Command:

```powershell
bash .githooks/pre-commit
```

Result:

```text
[pre-commit] OK
```

## Staging Area

Command:

```powershell
git diff --cached --name-only
```

Result: PASS, empty output.
