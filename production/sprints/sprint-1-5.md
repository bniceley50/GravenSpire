# Sprint 1.5 - T1 Combat Feel Correction

## Sprint Goal

Correct the Yellow slice-review findings without reopening the Combat Core architecture: add quiet Endurance for physical instants, restore two-trash overpull danger, revalidate the solo-trash feel target against D012, then rerun profiled evidence.

## Source Baseline

- Current HEAD: `4edf2f9` (`main` matching `origin/main` when drafted).
- Slice review verdict: `production/qa/combat/feel-review-T1-slice.md` - Yellow.
- Sprint 1 closure: 11/13 done; `T1-COMBAT-11` held; `T1-COMBAT-09c` human death-moment playtest still pending.
- Locked design contracts: D001-D003, D005-D009, D012 Locked; D004 Provisional; D010-D011 Proposed.
- ADR state: ADR-0001 through ADR-0003 Accepted; ADR-0004 and ADR-0005 Proposed.
- ADR-0003 governs `CombatProgressionBaselineSnapshot` only; it does not constrain `CombatPersistenceProjection`. Adding Endurance to the combat persistence whitelist does not amend ADR-0003.
- Quantitative evidence:
  - `tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl`
  - `production/qa/combat/t1-combat-10-profiled-evidence-summary.md`
  - `tests/evidence/T1-COMBAT-10/verification.md`
- Qualitative evidence:
  - Brian's Yellow rationale in `production/qa/combat/feel-review-T1-slice.md`.
  - `production/qa/combat/feel-review-09c-player-death.md` remains HUMAN PLAYTEST PENDING.
- Regression baseline: Combat test suite passed `133/133` as of `4edf2f9`.

## Capacity

- Total days: 10.0
- Buffer: 2.0
- Available: 8.0
- Planned Must Have: 7.0

## Required Pre-Implementation Gates

- Run `/qa-plan sprint` for Sprint 1.5 before implementation.
- Verify baseline with `dotnet test tests/Gravenspire.Combat.Tests.csproj`.
- Confirm no implementation starts before `T1.5-COMBAT-00` locks the Endurance contract.

## Parallelization Point

After `T1.5-COMBAT-02`, `T1.5-COMBAT-03` and `T1-COMBAT-11` can run in parallel. `T1.5-COMBAT-04` depends only on `T1.5-COMBAT-00` and can run in parallel with implementation. `T1.5-COMBAT-05` waits on `03`, `04`, and `11`.

If Codex parallel work is used under D006, `T1.5-COMBAT-04` and `T1-COMBAT-11` are the cleanest candidates for separate branches because they have low source-file write overlap with the Endurance implementation stories.

## Must Have

| ID | Task | Owner | Est. | Dependencies | Acceptance Criteria |
|---|---|---|---:|---|---|
| T1.5-COMBAT-00 | Endurance contract lock | systems-designer + architect | 1.0d | Slice verdict | D013 + ADR-0006 lock quiet Endurance, physical-only scope, and Smite of Authority / Defensive Prayer carveout |
| T1.5-COMBAT-01 | Endurance state, persistence, HUD signal | gameplay-programmer + qa-tester | 1.25d | 00, QA plan | Combat actor state, persistence projection, and Layer 1 HUD projection expose quiet Endurance without breaking 133-test baseline |
| T1.5-COMBAT-02 | Physical instant conversion | gameplay-programmer + qa-tester | 1.25d | 01 | Bash consumes Endurance; Smite of Authority and Defensive Prayer remain mana-based; fixtures validate both resource kinds |
| T1.5-COMBAT-03 | FEEL-03 overpull tuning | systems-designer + qa-tester | 1.25d | 02 | Two-trash overpull danger restored without treating FEEL-01 as a fixture-only tuning task |
| T1.5-COMBAT-04 | FEEL-01 target revalidation | game-designer + qa-tester | 1.0d | 00 | D014 in DECISIONS.md is preferred for the Keep / Move / Caveats target decision; GDD revision may accompany it if AC text changes |
| T1-COMBAT-11 | Forbidden-pattern compliance scan/analyzer | gameplay-programmer + qa-tester | 1.0d | 02 | Sprint-1 carryover scan lands and expands coverage to Endurance forbidden patterns |
| T1.5-COMBAT-05 | Profiled rerun + slice evidence summary | qa-tester + systems-designer | 1.25d | 03, 04, 11 | JSONL rerun and summary show post-correction FEEL-03 and revalidated FEEL-01 status; regression suite and pre-commit pass |

## Key Story Guards

- `T1.5-COMBAT-00`: design lock only; no implementation, fixtures, or tuning.
- `T1.5-COMBAT-01`: Endurance must stay quiet; no action-rotation HUD treatment.
- `T1.5-COMBAT-02`: Bash and physical instants move off mana; Smite of Authority and Defensive Prayer stay mana.
- `T1.5-COMBAT-03`: FEEL-03 is the stronger tuning warning.
- `T1.5-COMBAT-04`: FEEL-01 is a design revalidation, not a fixture tweak.
- `T1-COMBAT-11`: inherits from sprint-1 plan/status; current referenced story file is absent and must be created or recovered before implementation.
- `T1.5-COMBAT-05`: evidence only; do not issue a new Green/Yellow/Red verdict.

## Story Details

### T1.5-COMBAT-00 - Endurance Contract Lock

Scope:
- Append D013 to `DECISIONS.md` with status `Locked` if Brian approves the design text, otherwise `Proposed - pending implementation validation`.
- Create `docs/architecture/adr-0006-endurance-resource-model.md`.
- ADR-0006 section structure: Status, Date, Context, Decision, Consequences, See Also.
- Encode physical-only Endurance scope, quiet HUD/save discipline, and Smite of Authority / Defensive Prayer mana carveout.
- Define banned patterns: Endurance as action-rotation bar, Endurance HUD prominence above mana, pulse/combo treatment, per-ability Endurance callouts unless QA/debug-only.

Likely files touched:
- `DECISIONS.md`
- `docs/architecture/adr-0006-endurance-resource-model.md`

Acceptance criteria trace:
- Slice verdict rationale: `production/qa/combat/feel-review-T1-slice.md:80`.
- Endurance finding: `production/qa/combat/feel-review-T1-slice.md:54-58`.
- Implementation surface note: `production/qa/combat/feel-review-T1-slice.md:62`.
- Current mana-cost contract to amend: `design/gdd/combat-core.md:148-152`, `design/gdd/combat-core.md:746-747`.

Test plan:
- No code tests.
- Review D013 + ADR-0006 against slice-review rationale.
- Grep for accidental implementation edits in source/fixtures; this story must be docs-only.

Dependencies:
- Slice review verdict committed at `4edf2f9`.

Done definition:
- D013 and ADR-0006 exist with status fields.
- ADR-0006 names the carveout artifacts: Smite of Authority and Defensive Prayer.
- Stories `01` and `02` can cite ADR-0006 sections for scope guards.

### T1.5-COMBAT-01 - Endurance State, Persistence, HUD Signal

Scope:
- Add Endurance to Combat Core player actor state with max/current values and validation.
- Extend combat persistence whitelist beyond current health, mana, life state, and death handoff.
- Extend Layer 1 HUD projection with a quiet Endurance signal.
- Preserve T1 offline/local scope and avoid UI styling ownership.
- Add tests for state clamp/hydration, persistence projection, and HUD projection.

Likely files touched:
- `src/gameplay/combat/state/**`
- `src/gameplay/combat/persistence/CombatPersistenceProjection.cs`
- `src/gameplay/combat/presentation/CombatHudStateProjection.cs`
- `tests/unit/gameplay/combat/combat_actor_state_test.cs`
- `tests/unit/gameplay/combat/combat_persistence_projection_test.cs`
- `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs`

Acceptance criteria trace:
- Endurance T1 surface: `production/qa/combat/feel-review-T1-slice.md:58`.
- Persistence surface note: `production/qa/combat/feel-review-T1-slice.md:62`.
- Current persistence whitelist: `design/gdd/combat-core.md:786-787`.
- Current HUD resource projection: `src/gameplay/combat/presentation/CombatHudStateProjection.cs:53-54`, `src/gameplay/combat/presentation/CombatHudStateProjection.cs:98-99`.
- Current persistence projection: `src/gameplay/combat/persistence/CombatPersistenceProjection.cs:10-44`.
- ADR-0003 governs `CombatProgressionBaselineSnapshot` only; it does not constrain `CombatPersistenceProjection`. Adding Endurance to the combat persistence whitelist does not amend ADR-0003.

Test plan:
- `dotnet test tests/Gravenspire.Combat.Tests.csproj`
- New/updated unit tests for Endurance state clamp and invalid hydration.
- New/updated persistence tests proving allowed Endurance field and still-banned transient fields.
- New/updated HUD projection tests proving quiet categorical signal without threat/raw-number leaks.

Dependencies:
- `T1.5-COMBAT-00`
- Sprint 1.5 QA plan.

Done definition:
- Prior 133-test baseline still passes plus new Endurance tests.
- No `src/ui/**` dependency introduced.
- Persistence projection includes Endurance and still excludes cooldowns, cast progress, threat tables, target selection, and runtime ids.

### T1.5-COMBAT-02 - Physical Instant Conversion

Scope:
- Add fixture schema support for physical instant Endurance cost.
- Convert Bash from mana cost to Endurance cost.
- Keep Smite of Authority and Defensive Prayer mana-based.
- Update instant resolver so resource gating branches by ability/resource kind.
- Add tests proving physical and magical/holy instant resource paths are separate.

Likely files touched:
- `assets/data/combat/t1-combat-fixtures.json`
- `src/gameplay/combat/fixtures/**`
- `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs`
- `tests/unit/gameplay/combat/combat_tactical_ability_profile_test.cs`
- `tests/unit/gameplay/combat/combat_fixture_validation_test.cs`
- `tests/integration/gameplay/combat/combat_tactical_cleric_instants_test.cs`

Acceptance criteria trace:
- Bash/physical finding: `production/qa/combat/feel-review-T1-slice.md:54`.
- Specific carveout: `production/qa/combat/feel-review-T1-slice.md:56`.
- Current Bash fixture has `cost_mana`: `assets/data/combat/t1-combat-fixtures.json:416-418`.
- Current Smite of Authority and Defensive Prayer costs: `assets/data/combat/t1-combat-fixtures.json:388-390`, `assets/data/combat/t1-combat-fixtures.json:448-450`.
- Current resolver spends mana: `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:118`, validates mana at `src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs:222-224`.
- Current GDD instant AC requires mana and must be amended or superseded: `design/gdd/combat-core.md:746-747`.

Test plan:
- `dotnet test tests/Gravenspire.Combat.Tests.csproj`
- Bash insufficient-Endurance failure.
- Bash sufficient-Endurance success with mana unchanged.
- Smite of Authority and Defensive Prayer still spend mana and ignore Endurance.
- Fixture validator rejects physical instant with mana cost and rejects magical/holy instant without legal mana cost.

Dependencies:
- `T1.5-COMBAT-01`

Done definition:
- Bash uses Endurance in fixture data and resolver behavior.
- Smite of Authority and Defensive Prayer remain mana-based.
- Future Cleric magical/holy instants are documented under the same mana rule.
- Regression suite passes.

### T1.5-COMBAT-03 - FEEL-03 Overpull Tuning

Scope:
- Tune only the fixture/formula surface needed to restore two-trash overpull danger.
- Treat FEEL-03 as stronger than FEEL-01 per Brian's verdict rationale.
- Preserve named solo-block pass and med-break pass.
- Avoid tuning production to mask evidence without recording the tuning rationale.

Likely files touched:
- `assets/data/combat/t1-combat-fixtures.json`
- `prototypes/combat-slice-T1/Harness/**`
- `tests/evidence/T1.5-COMBAT-03/**`
- Potentially `production/stories/t1-5-combat-03-feel-03-overpull-tuning.md`

Acceptance criteria trace:
- FEEL-03 GDD target: `design/gdd/combat-core.md:808-810`.
- Failed-as-measured row: `tests/evidence/T1-COMBAT-10/verification.md:66`.
- Summary evidence: `production/qa/combat/t1-combat-10-profiled-evidence-summary.md:16`, `production/qa/combat/t1-combat-10-profiled-evidence-summary.md:24`.
- Slice rationale prioritizes FEEL-03: `production/qa/combat/feel-review-T1-slice.md:80`.
- T1-COMBAT-10 explicitly prohibited tuning in the evidence story: `production/stories/t1-combat-10-smoke-profiled-evidence-loop.md:21`, `production/stories/t1-combat-10-smoke-profiled-evidence-loop.md:88-90`.

Test plan:
- Rerun harness scenario for `TwoTrash_Overpull_T1`.
- Run named solo-block and med-break scenarios as regression guards.
- `dotnet test tests/Gravenspire.Combat.Tests.csproj`
- `bash .githooks/pre-commit`

Dependencies:
- `T1.5-COMBAT-02`

Done definition:
- FEEL-03 meets the current or updated target.
- Evidence file records before/after tuning inputs.
- FEEL-02 and FEEL-04 do not regress.
- FEEL-01 is not silently tuned in this story.

### T1.5-COMBAT-04 - FEEL-01 Target Revalidation

Scope:
- Decide whether the `55-85%` solo-trash target remains valid, changes, or gains caveats.
- Use D012 prototype feel plus 2026-05-06 prototype rerun and T1-COMBAT-10 harness data.
- Produce a design conversation output, not fixture tuning.
- Prefer a D014 entry in `DECISIONS.md` that names the FEEL-01 target decision as Keep, Move, or Caveats, with rationale citing D012, the prototype rerun, and harness data.
- If the target changes, update the design/AC source with rationale and link it to D014.

Likely files touched:
- `DECISIONS.md`
- `design/gdd/combat-core.md` if AC text changes.
- `production/qa/combat/feel-review-T1-slice.md` should not be rewritten; cite it.
- `production/stories/t1-5-combat-04-feel-01-target-revalidation.md`

Acceptance criteria trace:
- FEEL-01 GDD target: `design/gdd/combat-core.md:800-802`.
- Harness failure: `tests/evidence/T1-COMBAT-10/verification.md:64`.
- Summary evidence: `production/qa/combat/t1-combat-10-profiled-evidence-summary.md:14`, `production/qa/combat/t1-combat-10-profiled-evidence-summary.md:22`.
- Prototype rerun citation: `production/qa/combat/feel-review-T1-slice.md:32`.
- Brian rationale says FEEL-01 is softer and needs revalidation: `production/qa/combat/feel-review-T1-slice.md:80`.
- D012 baseline: `DECISIONS.md:339-369`.

Test plan:
- No fixture tuning.
- Document review against D012 and slice verdict.
- If AC text changes, run grep/checks to confirm all references to the old target are either updated or intentionally preserved as historical evidence.

Dependencies:
- `T1.5-COMBAT-00`
- Can run in parallel with `01/02/03`.

Done definition:
- Durable design output is filed as a D014 entry in `DECISIONS.md` (preferred) or as a GDD revision with linking rationale comment.
- If target changes, rationale cites both harness and prototype evidence.
- If target remains, rationale explains why prototype solo-trash wins do not invalidate it.
- No fixture values changed by this story.

### T1-COMBAT-11 - Forbidden-Pattern Compliance Scan/Analyzer

Scope:
- Carry over sprint-1 forbidden-pattern scan/analyzer.
- Use sprint-1 plan/status as the source because the referenced story file is currently absent.
- Add or recover the story handoff artifact before implementation.
- Expand scan coverage to include Endurance forbidden patterns after `T1.5-COMBAT-02`.
- Start grep/static scan; promote to analyzer only if grep cannot reliably enforce current patterns.

Likely files touched:
- `production/stories/t1-combat-11-forbidden-pattern-compliance-scan-analyzer.md` (create or recover)
- `docs/registry/architecture.yaml`
- `tools/**` or `tests/**` scan helper path selected by implementation
- `tests/evidence/T1-COMBAT-11/**`

Acceptance criteria trace:
- Sprint-1 story detail: `production/sprints/sprint-1.md:577-617`.
- Sprint-status carryover path: `production/sprint-status.yaml:129-131`.
- Architecture forbidden patterns registry: `docs/registry/architecture.yaml:481-708`.
- Existing forbidden-pattern examples include `combat_actor_id_as_xp_identity`: `docs/registry/architecture.yaml:482-484`.
- Endurance banned patterns from ADR-0006 after `T1.5-COMBAT-00`.
- T1 scope remains offline/local: `DECISIONS.md:48-59`.

Test plan:
- Static scan command outputs each forbidden pattern checked and pass/fail.
- Failure fixture proves one deliberate forbidden pattern is caught.
- DTO/schema scan checks event/snapshot/persistence shapes.
- `dotnet test tests/Gravenspire.Combat.Tests.csproj`
- `bash .githooks/pre-commit`

Dependencies:
- `T1.5-COMBAT-02`

Done definition:
- Carryover story file exists on disk.
- Scan covers sprint-1 forbidden patterns plus Endurance-specific banned patterns.
- Evidence names every pattern checked.
- No false claim that `combat_actor_id` is durable identity anywhere in XP/progression/save surfaces.

### T1.5-COMBAT-05 - Profiled Rerun and Evidence Summary

Scope:
- Rerun the profiled harness after Endurance, FEEL-03 tuning, FEEL-01 revalidation, and T1-COMBAT-11.
- Preserve quantitative evidence only; no new Green/Yellow/Red verdict.
- Produce JSONL evidence and a concise QA summary.
- Record whether each revalidated target passes, fails-as-measured, or is superseded by design decision.

Likely files touched:
- `tests/evidence/T1.5-COMBAT-05/profiled-combat-slice.jsonl`
- `tests/evidence/T1.5-COMBAT-05/verification.md`
- `production/qa/combat/t1-5-combat-profiled-evidence-summary.md`
- `production/stories/t1-5-combat-05-profiled-rerun-evidence-summary.md`

Acceptance criteria trace:
- Prior harness command: `tests/evidence/T1-COMBAT-10/verification.md:6`.
- Prior evidence rows: `tests/evidence/T1-COMBAT-10/verification.md:64-67`.
- Prior non-tuning discipline: `tests/evidence/T1-COMBAT-10/verification.md:11-13`.
- Slice verdict recommendation path: `production/qa/combat/feel-review-T1-slice.md:82-88`.
- Regression/pre-commit proof pattern: `tests/evidence/T1-COMBAT-10/verification.md:83`, `tests/evidence/T1-COMBAT-10/verification.md:121-127`.

Test plan:
- `dotnet run --project prototypes/combat-slice-T1/Harness/CombatSliceHarness.csproj`
- `dotnet test tests/Gravenspire.Combat.Tests.csproj`
- `bash .githooks/pre-commit`
- Negative scope grep for networking, PvP, live LLM, server authority, and action-rotation Endurance language.

Dependencies:
- `T1.5-COMBAT-03`
- `T1.5-COMBAT-04`
- `T1-COMBAT-11`

Done definition:
- New JSONL and verification artifacts exist.
- Summary distinguishes pass, failed-as-measured, and superseded target outcomes.
- Evidence is ready for sprint-1.5 closeout.
- No agent-authored feel verdict is added.

## Carryover From Previous Sprint

| Task | Reason | New Estimate |
|---|---|---:|
| T1-COMBAT-11 | Held in sprint-1 per slice review; should run after Endurance lands so scan covers the new resource model | 1.0d |
| 09c human playtest sections | `production/qa/combat/feel-review-09c-player-death.md` still has HUMAN PLAYTEST PENDING | 0.25d, deferral acceptable |

## Explicit Out Of Scope

- No Tier 2 networking, FishNet, server authority, PvP, accounts, cloud saves, live LLM, or multiplayer.
- No action-combat rotation.
- Endurance must not become a prominent player-facing rotation bar.
- No fixture tuning in `T1.5-COMBAT-00`, `01`, `02`, or `04`.
- FEEL-01 retargeting is design revalidation, not fixture tuning.
- Smite of Authority and Defensive Prayer stay mana-based.
- No new Green/Yellow/Red verdict in sprint-1.5 evidence artifacts unless Brian explicitly requests another human verdict artifact.

## Risks

| Risk | Probability | Impact | Mitigation |
|---|---:|---:|---|
| Endurance becomes an action-rotation bar | Medium | High | ADR-0006 banned patterns; HUD tests check quiet projection |
| FEEL-01 gets treated as fixture tuning | Medium | Medium | Separate `T1.5-COMBAT-04` design-output story |
| FEEL-03 tuning regresses solo-trash, named, or med-break behavior | Medium | High | Rerun FEEL-02 and FEEL-04 as guard scenarios |
| Endurance breaks 133-test baseline | Medium | High | Baseline regression before and after each implementation story |
| Missing T1-COMBAT-11 story file causes execution drift | Medium | Medium | Create or recover story artifact before implementation |

## Dependencies On External Factors

- Unity 6.3.14f1 local install for optional prototype rerun evidence.
- Slice review verdict commit `4edf2f9` as immutable input baseline.
- Existing `133/133` Combat test baseline.
- Sprint-1.5 QA plan before implementation.
- Brian owns any additional human qualitative verdict or death-moment playtest notes.

## QA Plan Status

No Sprint 1.5 QA plan exists yet. Run `/qa-plan sprint` before `T1.5-COMBAT-01`.

## Definition Of Done

- [ ] Sprint 1.5 QA plan exists.
- [ ] `T1.5-COMBAT-00` locks Endurance contract.
- [ ] Endurance state/persistence/HUD implementation passes regression.
- [ ] Bash/physical instants consume Endurance; Smite of Authority and Defensive Prayer remain mana-based.
- [ ] FEEL-03 retest meets target or records failed-as-measured with no masking.
- [ ] FEEL-01 target is explicitly revalidated.
- [ ] T1-COMBAT-11 scan/analyzer passes and includes Endurance forbidden patterns.
- [ ] Profiled rerun evidence and summary exist.
- [ ] `dotnet test tests/Gravenspire.Combat.Tests.csproj` passes.
- [ ] `bash .githooks/pre-commit` passes.
- [ ] No T1 scope creep found.

## Recommended First Dev Story

After `/qa-plan sprint` and baseline regression:

`/dev-story T1.5-COMBAT-00-endurance-contract-lock`

This first story is pure design-contract work. It must not include code, fixture, HUD/save implementation, or tuning.
