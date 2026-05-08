# T1.5-COMBAT-04 Verification

## Story

`production/stories/t1-5-combat-04-feel-01-target-revalidation.md`

## Decision Summary

D014 moves `H-CCOM-FEEL-01` from `55-85%` to `90-100%` Cleric wins for
clean-state `SoloTrash_EvenCon_T1` trials while preserving the mean ending-state
pressure clause. FEEL-01 now covers clean single-trash reliability; FEEL-03
remains the overpull danger surface.

## QA Case Evidence

| QA Case | Status | Evidence |
| --- | --- | --- |
| `QA-04-01` D014 decision exists | PASS | D014 is appended at `DECISIONS.md:415` and names the decision as Move at `DECISIONS.md:447`. |
| `QA-04-02` Rationale cites all required evidence | PASS | D014 cites T1-COMBAT-10 evidence at `DECISIONS.md:420` through `DECISIONS.md:424`, T1.5-COMBAT-02 resource-split evidence at `DECISIONS.md:430` through `DECISIONS.md:431`, D012/prototype evidence at `DECISIONS.md:433` through `DECISIONS.md:437`, and T1.5-COMBAT-03 / FEEL-03 separation evidence at `DECISIONS.md:441` through `DECISIONS.md:445`. |
| `QA-04-03` No fixture data modified | PASS | `git diff --name-only -- assets/data/combat` returned empty output. |
| `QA-04-04` Old target references handled | PASS | Broad target sweep below classifies the remaining `55-85` references as updated, historical, superseded-rationale, or explicitly deferred. |
| `QA-04-05` FEEL-01 remains distinct from FEEL-03 tuning | PASS | D014 states the separation at `DECISIONS.md:454` through `DECISIONS.md:456`; story rationale carries it at `production/stories/t1-5-combat-04-feel-01-target-revalidation.md:29` through `production/stories/t1-5-combat-04-feel-01-target-revalidation.md:33`. |

## Current Target Updates

| File | Status | Evidence |
| --- | --- | --- |
| `design/gdd/combat-core.md` encounter fixture table | Updated | `design/gdd/combat-core.md:429` now says clean-state solo trash wins `90-100%` and preserves ending pressure. |
| `design/gdd/combat-core.md` tuning knob | Updated | `design/gdd/combat-core.md:559` now uses `trash_clean_solo_target_success_rate`, default `0.95`, range `0.90-1.00`. |
| `design/gdd/combat-core.md` FEEL-01 AC | Updated | `design/gdd/combat-core.md:801` now requires `90-100%` clean-state wins and preserves the ending-state pressure clause; `design/gdd/combat-core.md:804` through `design/gdd/combat-core.md:806` reserve disadvantage starts for a future fixture/AC. |
| `tests/performance/README.md` | Updated | `tests/performance/README.md:7` now says `90-100 percent`, catching the spelled-out percent form. |

## Old Target Reference Classification

Command:

```powershell
rg -n "55-85|0\.55-0\.85|55 percent|90-100|0\.90-1\.00" . --glob "!production/session-logs/**"
```

Classification:

| Reference | Classification | Rationale |
| --- | --- | --- |
| `DECISIONS.md:420`, `DECISIONS.md:447`, `DECISIONS.md:466` | Superseded-rationale | D014 must name the old target to document what changed and why. |
| `design/gdd/combat-core.md:429`, `design/gdd/combat-core.md:559`, `design/gdd/combat-core.md:801` | Updated current target | These are the design-source lines changed to `90-100%` / `0.90-1.00`. |
| `tests/performance/README.md:7` | Updated current target | The spelled-out `55-85 percent` reference is now `90-100 percent`. |
| `assets/data/combat/t1-combat-fixtures.json:511` | Deferred to `T1.5-COMBAT-05` | Fixture/harness labels are outside this Design/Contract batch; D014 assigns machine-readable target/output-label updates to the profiled rerun story. |
| `production/sprints/sprint-1.md:563` | Historical sprint plan | Sprint 1 plan remains historical; not a current target source after D014. |
| `production/sprints/sprint-1-5.md:219` | Historical task framing | Sprint 1.5 correctly records that this story decides whether the old target remains, changes, or gains caveats. |
| `production/stories/t1-combat-10-smoke-profiled-evidence-loop.md:18`, `production/stories/t1-combat-10-smoke-profiled-evidence-loop.md:56` | Historical story evidence | T1-COMBAT-10 failed-as-measured against the old target; preserving that evidence is required. |
| `production/stories/t1-5-combat-04-feel-01-target-revalidation.md:25`, `production/stories/t1-5-combat-04-feel-01-target-revalidation.md:86` | Superseded-rationale / verification pointer | The new story names the old target only as the superseded target and QA-04-04 proof obligation. |
| `production/session-state/active.md:72`, `production/session-state/active.md:324` | Historical session extract | Session-state history preserves the T1-COMBAT-10 failure description; closure routing is intentionally not part of this batch. |
| `production/qa/plans/qa-plan-sprint-20260428.md:221` | Historical QA plan | Original Sprint 1 QA plan remains historical and explains the old measurement. |
| `production/qa/plans/qa-plan-sprint-1-5-20260506.md:151` | QA instruction | This is the QA-04-04 drift check itself, not a current FEEL-01 target. |
| `production/qa/combat/t1-combat-10-profiled-evidence-summary.md:14`, `production/qa/combat/t1-combat-10-profiled-evidence-summary.md:22` | Immutable evidence summary | The summary records what T1-COMBAT-10 measured at the time. |
| `production/qa/combat/feel-review-T1-slice.md:19`, `production/qa/combat/feel-review-T1-slice.md:80` | Slice-review evidence/rationale | The sprint plan explicitly says to cite, not rewrite, this artifact. |
| `tests/evidence/T1-COMBAT-10/verification.md:64` | Immutable evidence | The old target remains necessary to explain the T1-COMBAT-10 failed-as-measured row. |

No `0.55-0.85` target remains outside historical/superseded rationale; the
current GDD tuning range is `0.90-1.00`.

## No Fixture Modification Check

Command:

```powershell
git diff --name-only -- assets/data/combat
```

Result: PASS, empty output.

## Regression Check

Command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"
```

Result:

```text
Passed!  - Failed:     0, Passed:   149, Skipped:     0, Total:   149
```

## Hygiene

Command:

```powershell
git diff --check
```

Result: PASS, exit code `0`. Git printed CRLF normalization warnings for
existing tracked markdown files, but no whitespace errors.

Command:

```powershell
bash .githooks/pre-commit
```

Result:

```text
[pre-commit] OK
```

## Story Boundary

Approved files in this implementation batch:

```text
DECISIONS.md
design/gdd/combat-core.md
tests/performance/README.md
production/stories/t1-5-combat-04-feel-01-target-revalidation.md
tests/evidence/T1.5-COMBAT-04/verification.md
```

No fixture data, harness code, sprint-status, or active-session routing files
are modified by this batch.

## Carry Forward

- `T1.5-COMBAT-05` owns the profiled rerun and any harness/output-label update
  needed for the new FEEL-01 target.
- Disadvantage-state solo-trash vulnerability remains a future design target,
  not part of clean-state FEEL-01.
- `/story-done` still owns the closure-only status/routing update for this
  story.
