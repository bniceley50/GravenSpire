# QA Plan - Sprint 1.5 T1 Combat Feel Correction

**Date:** 2026-05-06
**Invocation:** `/qa-plan sprint`
**Scope:** Sprint 1.5 combat-feel correction: quiet Endurance, physical instant conversion, FEEL-03 overpull tuning, FEEL-01 target revalidation, T1-COMBAT-11 carryover, and profiled rerun evidence.
**Input source:** `production/sprints/sprint-1-5.md` at commit `8885d2e`.
**Sprint status source:** `production/sprint-status.yaml` at commit `a562362`.
**Confidence:** High for QA scope and discipline guards; medium for exact future test filenames until each `/dev-story` creates its story artifact and implementation batch.

## QA Scope

Sprint 1.5 responds to the Yellow T1 combat slice verdict. The sprint goal is to correct combat-feel findings without reopening the Combat Core architecture: add quiet Endurance for physical instants, restore two-trash overpull danger, revalidate the solo-trash feel target against D012, then rerun profiled evidence (`production/sprints/sprint-1-5.md:3`, `production/sprints/sprint-1-5.md:47` through `production/sprints/sprint-1-5.md:53`).

The QA plan covers all seven Sprint 1.5 Must Have stories:

| Story | Classification | Automated Test Required | Manual / Document Verification Required |
| --- | --- | --- | --- |
| `T1.5-COMBAT-00` Endurance contract lock | Design/Contract | Static grep for docs-only scope | D013 + ADR-0006 artifact verification |
| `T1.5-COMBAT-01` Endurance state, persistence, HUD signal | Integration | Unit + integration regression tests | Quiet Endurance projection review |
| `T1.5-COMBAT-02` Physical instant conversion | Logic + Integration | Unit + integration resource-gating tests | Fixture schema review |
| `T1.5-COMBAT-03` FEEL-03 overpull tuning | Config/Data + Profiled Feel | Harness rerun + regression guards | Tuning rationale review |
| `T1.5-COMBAT-04` FEEL-01 target revalidation | Design/Contract | Static grep for no fixture changes | D014 artifact verification |
| `T1-COMBAT-11` Forbidden-pattern compliance scan/analyzer | Static/Integration | Static scan + failure fixture | Missing story-file recovery verification |
| `T1.5-COMBAT-05` Profiled rerun + slice evidence summary | Profiled QA Evidence | Harness rerun + regression suite | No-agent-verdict summary review |

Story file status: all referenced Sprint 1.5 story files are forward-looking and currently absent. This QA plan uses `production/sprints/sprint-1-5.md` as the authoritative story-detail source. Test execution for each story waits for its `/dev-story` run to create the story file and implementation batch.

## Source List

Verification method: live repository reads with `Get-Content`, `Select-String`, `Test-Path`, and git inspection on 2026-05-06.

| Source | Use |
| --- | --- |
| `production/sprints/sprint-1-5.md:31` | Required pre-implementation gates. |
| `production/sprints/sprint-1-5.md:47` through `production/sprints/sprint-1-5.md:53` | Sprint 1.5 story list and acceptance summary. |
| `production/sprints/sprint-1-5.md:55` through `production/sprints/sprint-1-5.md:63` | Key story guards. |
| `production/sprints/sprint-1-5.md:67` through `production/sprints/sprint-1-5.md:331` | Per-story scope, AC trace, test plans, dependencies, and done definitions. |
| `production/sprints/sprint-1-5.md:338` through `production/sprints/sprint-1-5.md:346` | Explicit out-of-scope guardrails. |
| `production/sprints/sprint-1-5.md:370` through `production/sprints/sprint-1-5.md:381` | Sprint definition of done. |
| `production/qa/combat/feel-review-T1-slice.md:19` through `production/qa/combat/feel-review-T1-slice.md:20` | FEEL-01 and FEEL-03 failed-as-measured evidence summary. |
| `production/qa/combat/feel-review-T1-slice.md:54` through `production/qa/combat/feel-review-T1-slice.md:62` | Brian's Endurance finding and implementation surface. |
| `production/qa/combat/feel-review-T1-slice.md:74` through `production/qa/combat/feel-review-T1-slice.md:80` | Brian's Yellow verdict and rationale. |
| `tests/evidence/T1-COMBAT-10/verification.md:64` through `tests/evidence/T1-COMBAT-10/verification.md:67` | Prior profiled evidence rows for FEEL-01/03/04. |
| `tests/evidence/T1-COMBAT-10/verification.md:121` through `tests/evidence/T1-COMBAT-10/verification.md:127` | Prior pre-commit proof pattern. |
| `design/gdd/combat-core.md:148` through `design/gdd/combat-core.md:152` | Current tactical instant mana-cost contract and no action-combat rotation guard. |
| `design/gdd/combat-core.md:746` through `design/gdd/combat-core.md:747` | Current `H-CCOM-INST-01` instant mana-cost AC to amend/supersede. |
| `design/gdd/combat-core.md:786` through `design/gdd/combat-core.md:787` | Current combat persistence whitelist. |
| `design/gdd/combat-core.md:800` through `design/gdd/combat-core.md:813` | FEEL-01/02/03/04 profile acceptance criteria. |
| `design/gdd/combat-core.md:842` through `design/gdd/combat-core.md:896` | Combat Core acceptance matrix. |
| `DECISIONS.md:48` through `DECISIONS.md:59` | D003 T1 offline/single-player scope. |
| `DECISIONS.md:339` through `DECISIONS.md:369` | D012 combat-feel baseline and tactical instant decision. |
| `docs/registry/architecture.yaml:481` through `docs/registry/architecture.yaml:708` | Current forbidden-pattern source. |
| `tests/Gravenspire.Combat.Tests.csproj:3` and `tests/Gravenspire.Combat.Tests.csproj:17` through `tests/Gravenspire.Combat.Tests.csproj:25` | `net8.0` test project and included production/test source surfaces. |

## Live-State Corrections

- All Sprint 1.5 story files referenced by `production/sprint-status.yaml` are currently missing. This is expected until `/dev-story` creates the story handoff artifact for each implementation batch.
- `production/stories/t1-combat-11-forbidden-pattern-compliance-scan-analyzer.md` is also missing even though it was referenced by Sprint 1 planning and status. Sprint 1.5 QA treats this as a specific precondition for `T1-COMBAT-11`.
- `docs/architecture/control-manifest.md` is absent. Forbidden-pattern QA uses `docs/registry/architecture.yaml` until `T1-COMBAT-11` creates, supersedes, or explicitly rejects a separate control manifest.
- `production/sprint-status.yaml` already points at `/qa-plan sprint`, but its `qa_plan` field remains blank until this QA plan is committed and a later bookkeeping batch updates the pointer.

## Regression Gates

| Gate | Timing | Command / Method | Pass Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| RG-00 Baseline before implementation | Before any `/dev-story` | `dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` | Existing Combat test suite passes at `133/133`; record any count change if test discovery changes before code work | Story 00 evidence or first Sprint 1.5 verification note |
| RG-01 Endurance state regression | After `T1.5-COMBAT-01` | `dotnet test tests/Gravenspire.Combat.Tests.csproj` | Prior 133-test baseline still passes plus Endurance state/persistence/HUD tests | `tests/evidence/T1.5-COMBAT-01/verification.md` |
| RG-02 Physical instant regression | After `T1.5-COMBAT-02` | `dotnet test tests/Gravenspire.Combat.Tests.csproj` | Bash Endurance tests pass; Smite of Authority and Defensive Prayer mana tests pass; frozen event schema tests still pass | `tests/evidence/T1.5-COMBAT-02/verification.md` |
| RG-03 Final profiled rerun regression | During `T1.5-COMBAT-05` | `dotnet test tests/Gravenspire.Combat.Tests.csproj`; `bash .githooks/pre-commit`; harness command | Unit/integration suite passes; hook passes; JSONL evidence generated | `tests/evidence/T1.5-COMBAT-05/verification.md` |

## Story Test Plans

### T1.5-COMBAT-00 - Endurance Contract Lock

**Classification:** Design/Contract
**Story file status:** `production/stories/t1-5-combat-00-endurance-contract-lock.md` does not exist yet.
**Sprint-plan source:** `production/sprints/sprint-1-5.md:67` through `production/sprints/sprint-1-5.md:97`.

| Case ID | Scenario | Method | Pass / Fail Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| QA-00-01 | D013 Endurance decision exists | Inspect `DECISIONS.md` after the story | D013 exists, has a status field, cites the slice review, and states physical-only quiet Endurance scope | `production/stories/t1-5-combat-00-endurance-contract-lock.md` AC trace; story verification |
| QA-00-02 | ADR-0006 exists with standard sections | Inspect `docs/architecture/adr-0006-endurance-resource-model.md` | ADR includes Status, Date, Context, Decision, Consequences, and See Also | `docs/architecture/adr-0006-endurance-resource-model.md` |
| QA-00-03 | Specific carveout artifacts are named | Inspect ADR-0006 Decision section | Smite of Authority and Defensive Prayer are named explicitly as mana-based carveouts, not only described by category | ADR-0006 line evidence |
| QA-00-04 | Quiet Endurance banned patterns are explicit | Inspect ADR-0006 | Banned patterns include Endurance as action-rotation bar, HUD prominence above mana, pulse/combo treatment, and per-ability callouts unless QA/debug-only | ADR-0006 line evidence |
| QA-00-05 | Docs-only story boundary holds | `git diff --name-only` for the story commit; grep for source/fixture/test edits | Story changes only `DECISIONS.md` and ADR-0006 unless Brian explicitly widens scope | Story verification |

Needs clarification before `/dev-story`: D013 status is allowed to be `Locked` or `Proposed - pending implementation validation` per the sprint plan. The story must choose one explicitly and explain why.

### T1.5-COMBAT-01 - Endurance State, Persistence, HUD Signal

**Classification:** Integration
**Story file status:** `production/stories/t1-5-combat-01-endurance-state-persistence-hud-signal.md` does not exist yet.
**Sprint-plan source:** `production/sprints/sprint-1-5.md:99` through `production/sprints/sprint-1-5.md:137`.

| Case ID | Scenario | Method | Pass / Fail Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| QA-01-01 | Endurance actor state validates and clamps | Unit tests in `tests/unit/gameplay/combat/combat_actor_state_test.cs` or story-specific equivalent | Current/max Endurance cannot hydrate below zero, above max, or with invalid max; valid state round-trips | `tests/evidence/T1.5-COMBAT-01/verification.md` |
| QA-01-02 | Combat persistence whitelist adds exactly Endurance | Unit tests in `tests/unit/gameplay/combat/combat_persistence_projection_test.cs` | `CombatPersistenceProjection` exposes prior four persisted fields plus Endurance and no additional transient fields | `tests/evidence/T1.5-COMBAT-01/verification.md` |
| QA-01-03 | Persistence still excludes transient combat state | Unit tests / reflection assertions | Threat tables, target selection, cast progress, cooldowns, swing timers, runtime ids, hit rolls, formula outputs, and regen rates remain absent | `tests/evidence/T1.5-COMBAT-01/verification.md` |
| QA-01-04 | HUD projection exposes quiet Endurance | Integration test in `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs` or equivalent | HUD projection exposes Endurance as a quiet resource/signal without raw numeric pulse, combo, priority-above-mana, or action-rotation behavior | `tests/evidence/T1.5-COMBAT-01/verification.md` |
| QA-01-05 | No `src/ui/**` dependency introduced | Static scan | Combat HUD projection remains gameplay-side; no `src/ui/**` or final styling ownership added | Story verification |
| QA-01-06 | ADR-0003 non-constraint preserved | Document/reference check | Story cites that ADR-0003 governs `CombatProgressionBaselineSnapshot` only and does not constrain `CombatPersistenceProjection` shape | Story AC trace |

Needs clarification before `/dev-story`: the sprint plan says Endurance should be a "categorical signal" and "quiet", but exact API names and whether the HUD resource snapshot exposes current/max or category-only are not locked until D013/ADR-0006.

### T1.5-COMBAT-02 - Physical Instant Conversion

**Classification:** Logic + Integration
**Story file status:** `production/stories/t1-5-combat-02-physical-instant-conversion.md` does not exist yet.
**Sprint-plan source:** `production/sprints/sprint-1-5.md:139` through `production/sprints/sprint-1-5.md:178`.

| Case ID | Scenario | Method | Pass / Fail Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| QA-02-01 | Bash uses Endurance, not mana | Integration test in `tests/integration/gameplay/combat/combat_tactical_cleric_instants_test.cs` or equivalent | Bash succeeds with sufficient Endurance, leaves mana unchanged, starts cooldown/global recovery, and applies declared effects | `tests/evidence/T1.5-COMBAT-02/verification.md` |
| QA-02-02 | Bash fails on insufficient Endurance | Integration test | Bash fails loud with insufficient Endurance, spends no mana, applies no effect, and does not start cooldown unless ADR-0006 explicitly says otherwise | `tests/evidence/T1.5-COMBAT-02/verification.md` |
| QA-02-03 | Smite of Authority remains mana-based | Integration test | Smite of Authority spends mana, ignores Endurance, fails loud on insufficient mana, and keeps existing cooldown/recovery behavior | `tests/evidence/T1.5-COMBAT-02/verification.md` |
| QA-02-04 | Defensive Prayer remains mana-based | Integration test | Defensive Prayer spends mana, ignores Endurance, fails loud on insufficient mana, and keeps existing self-buff behavior | `tests/evidence/T1.5-COMBAT-02/verification.md` |
| QA-02-05 | Fixture validator rejects physical instant with `cost_mana` | Unit/data validation test | Physical instant rows such as Bash are invalid if they retain `cost_mana` instead of the Endurance cost field | `tests/evidence/T1.5-COMBAT-02/verification.md` |
| QA-02-06 | Fixture validator rejects magical/holy instant without legal mana cost | Unit/data validation test | Smite of Authority, Defensive Prayer, and future Cleric magical/holy instants require a legal mana cost unless a later contract changes this | `tests/evidence/T1.5-COMBAT-02/verification.md` |
| QA-02-07 | No `combat_actor_id` leak into Endurance events | Reflection/static schema test | Endurance-related event/result DTOs do not add durable `combat_actor_id` identity or persistence fields | `tests/evidence/T1.5-COMBAT-02/verification.md` |

Needs clarification before `/dev-story`: the sprint plan does not name the final Endurance fixture field (`cost_endurance`, `enduranceCostByBand`, etc.). The implementation story must lock the exact schema name and update validator expectations.

### T1.5-COMBAT-03 - FEEL-03 Overpull Tuning

**Classification:** Config/Data + Profiled Feel
**Story file status:** `production/stories/t1-5-combat-03-feel-03-overpull-tuning.md` does not exist yet.
**Sprint-plan source:** `production/sprints/sprint-1-5.md:180` through `production/sprints/sprint-1-5.md:214`.

| Case ID | Scenario | Method | Pass / Fail Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| QA-03-01 | Two-trash overpull rerun | Harness/profiled run for `TwoTrash_Overpull_T1` | `H-CCOM-FEEL-03` meets current or updated target; if it fails, evidence is recorded as failed-as-measured, not masked | `tests/evidence/T1.5-COMBAT-03/verification.md` |
| QA-03-02 | Named solo-block regression | Harness/profiled run for `NamedSoloBlock_T1` | `H-CCOM-FEEL-02` remains pass or any failure is recorded as a regression | `tests/evidence/T1.5-COMBAT-03/verification.md` |
| QA-03-03 | Med-break pacing regression | Harness/profiled run for `MedBreak_Pacing_T1` | `H-CCOM-FEEL-04` remains in the `60-120s` target band unless changed by a later explicit design decision | `tests/evidence/T1.5-COMBAT-03/verification.md` |
| QA-03-04 | FEEL-01 untouched by this story | Diff review | No FEEL-01-only fixture retargeting occurs in this story; FEEL-01 remains owned by `T1.5-COMBAT-04` | Story verification |
| QA-03-05 | Tuning rationale captured | Story verification | Before/after tuned inputs and rationale are documented; no production tuning is hidden as a non-evidence change | Story verification |

Needs clarification before `/dev-story`: the sprint plan does not identify which fixture/formula knobs are legal for FEEL-03 tuning. The story should list the candidate knobs before implementation.

### T1.5-COMBAT-04 - FEEL-01 Target Revalidation

**Classification:** Design/Contract
**Story file status:** `production/stories/t1-5-combat-04-feel-01-target-revalidation.md` does not exist yet.
**Sprint-plan source:** `production/sprints/sprint-1-5.md:216` through `production/sprints/sprint-1-5.md:252`.

| Case ID | Scenario | Method | Pass / Fail Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| QA-04-01 | D014 decision exists | Inspect `DECISIONS.md` after the story | D014 exists or a companion GDD revision explicitly links to D014; D014 names FEEL-01 decision as Keep, Move, or Caveats | Story verification |
| QA-04-02 | Rationale cites all required evidence | Document review | D014 rationale cites D012, `prototypes/combat-feel/Logs/playtest-20260506-093105.log`, and `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl` | Story verification |
| QA-04-03 | No fixture data modified | `git diff --name-only` and focused diff on `assets/data/combat/**` | This story does not tune fixture data or harness behavior | Story verification |
| QA-04-04 | Old target references handled | Grep for `55-85%`, `SoloTrash_EvenCon_T1`, and FEEL-01 references after any GDD/AC change | References are either updated to the new target or intentionally preserved as historical evidence with rationale | Story verification |
| QA-04-05 | FEEL-01 remains distinct from FEEL-03 tuning | Document review | D014 does not reframe FEEL-03 as a target-revalidation problem and does not silently move FEEL-03 tuning scope | Story verification |

Needs clarification before `/dev-story`: if Brian chooses "Caveats" rather than Keep or Move, the story must define whether harness pass/fail status uses the old target, a new conditional target, or a superseded-target label.

### T1-COMBAT-11 - Forbidden-Pattern Compliance Scan/Analyzer

**Classification:** Static/Integration
**Story file status:** `production/stories/t1-combat-11-forbidden-pattern-compliance-scan-analyzer.md` does not exist yet.
**Sprint-plan source:** `production/sprints/sprint-1-5.md:254` through `production/sprints/sprint-1-5.md:291`; Sprint 1 source `production/sprints/sprint-1.md:577` through `production/sprints/sprint-1.md:617`.

| Case ID | Scenario | Method | Pass / Fail Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| QA-11-01 | Missing story artifact recovered | File existence check | `production/stories/t1-combat-11-forbidden-pattern-compliance-scan-analyzer.md` exists before implementation closes | Story verification |
| QA-11-02 | Forbidden-pattern source resolved | Static scan setup review | Scan uses `docs/registry/architecture.yaml` unless the story creates/supersedes `docs/architecture/control-manifest.md` with rationale | `tests/evidence/T1-COMBAT-11/verification.md` |
| QA-11-03 | Existing architecture forbidden patterns covered | Static scan | Patterns from `docs/registry/architecture.yaml:481-708` are named in output and each reports pass/fail | `tests/evidence/T1-COMBAT-11/verification.md` |
| QA-11-04 | Endurance forbidden patterns covered | Static scan | Endurance-specific banned patterns from ADR-0006 are included after `T1.5-COMBAT-00`/`02` land | `tests/evidence/T1-COMBAT-11/verification.md` |
| QA-11-05 | Failure fixture proves scanner works | Deliberate forbidden-pattern sample | Scanner fails when a deliberate forbidden-pattern fixture is present and passes when it is removed | `tests/evidence/T1-COMBAT-11/verification.md` |
| QA-11-06 | Frozen event invariants remain unchanged | Reflection/schema tests | `PlayerKillCreditEvent` remains 4 fields; `PlayerDeathEvent` remains 6 fields; `CombatActorDeathEvent` remains 3 fields and treats `combat_actor_id` as transient | `tests/evidence/T1-COMBAT-11/verification.md` |

Needs clarification before `/dev-story`: the sprint plan allows grep-first or analyzer promotion. The story must state the minimum scan command and whether a Roslyn analyzer is warranted.

### T1.5-COMBAT-05 - Profiled Rerun and Evidence Summary

**Classification:** Profiled QA Evidence
**Story file status:** `production/stories/t1-5-combat-05-profiled-rerun-evidence-summary.md` does not exist yet.
**Sprint-plan source:** `production/sprints/sprint-1-5.md:293` through `production/sprints/sprint-1-5.md:331`.

| Case ID | Scenario | Method | Pass / Fail Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| QA-05-01 | Full profiled harness rerun | `dotnet run --project prototypes/combat-slice-T1/Harness/CombatSliceHarness.csproj` | JSONL evidence is produced for solo trash, named solo-block, two-trash overpull, med-break pacing, and structural smoke | `tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl` |
| QA-05-02 | Regression suite after profiled rerun | `dotnet test tests/Gravenspire.Combat.Tests.csproj` | Combat suite passes; any count change is explained in verification | `tests/evidence/T1.5-COMBAT-05/verification.md` |
| QA-05-03 | Pre-commit hook proof | `bash .githooks/pre-commit` | Hook reports `[pre-commit] OK` | `tests/evidence/T1.5-COMBAT-05/verification.md` |
| QA-05-04 | Summary labels quantitative outcomes only | Review summary markdown | Summary distinguishes pass, failed-as-measured, and superseded-target outcomes without Green/Yellow/Red verdict language from the harness or agent | `production/qa/combat/t1-5-combat-profiled-evidence-summary.md` |
| QA-05-05 | No-agent-verdict guard | Grep JSONL + summary for `Green`, `Yellow`, `Red`, `verdict` | No agent-authored feel verdict appears; any future Green/Yellow/Red call requires Brian-authored verdict artifact | `tests/evidence/T1.5-COMBAT-05/verification.md` |
| QA-05-06 | T1 scope negative pass | Static grep | No networking/FishNet, PvP, live LLM, server authority, or action-rotation Endurance language introduced | `tests/evidence/T1.5-COMBAT-05/verification.md` |

Needs clarification before `/dev-story`: if `T1.5-COMBAT-04` supersedes FEEL-01, the verification file must define the exact output label used for the superseded target.

## Cross-Story Required Checks

### Endurance Quietness

Endurance is not just "a new bar." QA must verify:

- HUD projection exposes Endurance without making it more prominent than mana.
- HUD projection does not create action-rotation, combo, pulse, or per-ability callout behavior.
- Persistence projection adds exactly Endurance beyond the prior whitelist and no other transient fields.
- Fixture schema separates physical Endurance costs from magical/holy mana costs.
- Endurance event/result surfaces do not leak durable `combat_actor_id` identity.

### Magical/Holy Mana Regression

Smite of Authority and Defensive Prayer are explicit carveout artifacts. QA must verify both:

- Spend mana and fail loud on insufficient mana.
- Ignore Endurance availability.
- Preserve current cooldown/global recovery behavior unless ADR-0006 explicitly changes it.

### Frozen Event Invariants

Sprint 1.5 must not reopen sprint-1 architectural seams. QA must verify by reflection or schema assertions:

- `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)` remains unchanged.
- `PlayerDeathEvent` remains the six-field payload established in T1-COMBAT-09c.
- `CombatActorDeathEvent` remains the three-field runtime event and treats `combat_actor_id` as transient.

## Smoke Test Scope

Critical paths to verify before Sprint 1.5 QA handoff:

1. Baseline `dotnet test tests/Gravenspire.Combat.Tests.csproj` passes before implementation begins.
2. D013 and ADR-0006 exist before Endurance implementation.
3. Endurance state, persistence, and HUD projection tests pass.
4. Bash consumes Endurance while Smite of Authority and Defensive Prayer consume mana.
5. FEEL-03 profiled rerun meets the current or updated target without regressing named solo-block or med-break pacing.
6. FEEL-01 target decision is durably recorded as D014 or linked GDD revision.
7. T1-COMBAT-11 scan covers existing architecture patterns and Endurance-specific banned patterns.
8. T1.5 profiled rerun produces JSONL + summary with no agent-authored Green/Yellow/Red verdict.

## Playtest Requirements

| Story | Playtest Goal | Minimum Sessions | Target Player Type | Required Evidence |
| --- | --- | ---:| --- | --- |
| `T1.5-COMBAT-03` | Confirm two-trash overpull feels dangerous after tuning | 1 advisory session if harness metrics pass but feel is disputed | Brian / designer | Written notes in the story verification or QA summary |
| `T1.5-COMBAT-04` | Re-anchor FEEL-01 target against D012 prototype feel if Brian wants live confirmation | Optional | Brian | D014 rationale cites whether live play was used or skipped |
| `T1.5-COMBAT-05` | Decide whether post-correction metrics require another human verdict artifact | Optional, Brian-only | Brian | Separate human-authored verdict artifact if requested |

No agent verdict replaces Brian's judgment. Quantitative reruns can surface evidence; they do not issue Green/Yellow/Red.

## Needs Clarification Before /dev-story

| Story | Clarification Needed | Why It Matters |
| --- | --- | --- |
| `T1.5-COMBAT-00` | D013 status: `Locked` vs `Proposed - pending implementation validation` | Downstream stories need to know whether the contract is final or implementation-validated later |
| `T1.5-COMBAT-01` | Exact quiet Endurance projection API shape | Tests need to assert category/current/max behavior without inventing UI styling |
| `T1.5-COMBAT-02` | Exact Endurance fixture field names | Fixture validator tests need stable schema names |
| `T1.5-COMBAT-03` | Legal fixture/formula knobs for FEEL-03 tuning | Prevents uncontrolled tuning and preserves FEEL-01 ownership |
| `T1.5-COMBAT-04` | If decision is Caveats, how harness labels FEEL-01 outcome | Prevents mismatch between D014 and profiled evidence summary |
| `T1-COMBAT-11` | Grep/static scan vs Roslyn analyzer threshold | Prevents overbuilding or underbuilding the scan |
| `T1.5-COMBAT-05` | Label for superseded targets if D014 changes FEEL-01 | Keeps JSONL/summary output machine-readable and verdict-free |

## Definition of Done - Sprint 1.5 QA

A Sprint 1.5 story is DONE only when all applicable items are true:

- [ ] Story file exists and cites the Sprint 1.5 plan lines used by this QA plan.
- [ ] All story acceptance criteria are verified via automated test, profiled evidence, or document review.
- [ ] Logic and Integration stories include automated tests in the relevant `tests/unit/**` or `tests/integration/**` surface.
- [ ] Design/Contract stories verify artifact existence, status fields, carveout wording, and no accidental implementation edits.
- [ ] Profiled stories write JSONL and verification summaries under `tests/evidence/**`.
- [ ] `dotnet test tests/Gravenspire.Combat.Tests.csproj` passes at required regression gates.
- [ ] `bash .githooks/pre-commit` reports `[pre-commit] OK` where required.
- [ ] No T1 scope creep is introduced.
- [ ] No agent-authored Green/Yellow/Red verdict is added.

## Recommended Next Step

Run the baseline regression gate:

`dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"`

Then start `/dev-story T1.5-COMBAT-00-endurance-contract-lock` with this QA plan and `production/sprints/sprint-1-5.md` as required inputs.
