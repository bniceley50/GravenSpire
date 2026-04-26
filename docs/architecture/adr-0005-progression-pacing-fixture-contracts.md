# ADR-0005: Progression Pacing Fixture Contracts

## Status
Proposed

## Date
2026-04-26

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Unity 6.3 LTS (6000.3.x) |
| **Domain** | Core gameplay test fixtures / progression balance validation |
| **Knowledge Risk** | MEDIUM - Unity 6.3 is post-LLM-cutoff, but this ADR defines deterministic test-data contracts and validation rules rather than Unity-specific APIs. |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`; `docs/engine-reference/unity/breaking-changes.md`; `docs/engine-reference/unity/deprecated-apis.md`; `.claude/docs/technical-preferences.md` |
| **Post-Cutoff APIs Used** | None. |
| **Verification Required** | Editor validators for fixture schema/feasibility; Unit tests for deterministic XP math; PlayMode/profile harness tests proving profiled pacing reports are based on legal fixture rows. |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0001 XP Source Lifecycle Registry for legal kill-credit source metadata and repeatability semantics; ADR-0003 Progression Baseline Snapshot Contract for `current_level`/`visible_level` vocabulary; ADR-0004 First-Save Materialization and Character Identity for active `local_character_id` context in dedupe fixtures. |
| **Enables** | Character Progression GDD approval; Character Progression tuning-story readiness; Combat + Progression fixture integration readiness. |
| **Blocks** | T1 Character Progression implementation closeout; any T1 pacing signoff that claims XP/hour, kills/level, or time-to-ding; any GDD acceptance criterion that uses mathematically impossible progression fixtures. |
| **Ordering Note** | This ADR is the final architectural prerequisite for revising Character Progression after ADRs 0001-0004. |

## Context

### Problem Statement

Character Progression needs to preserve an EQ-slow, earned progression fantasy without allowing undercon farming, named farming, or dense-camp loops to bypass pacing. The GDD already contains pacing acceptance criteria for XP/hour, kills/level, med-break cadence, and time-to-ding. It also contains event-order criteria that require specific threshold crossings.

Round-4 review exposed that some acceptance criteria can become structurally untestable if they are written as if every event scenario must come from a legal kill-credit route. For example, a multi-level event-order test may require an XP transaction larger than any legal per-kill cap allows. Conversely, pacing claims are invalid if they come from synthetic direct-XP fixtures that bypass Combat Core kill credit, ADR-0001 source lookup, repeatability, lockout, and cadence constraints.

The project needs one fixture architecture that separates legal pacing fixtures from synthetic event/formula fixtures and requires a deterministic math preflight before any profiled playtest can count as pacing evidence.

### Constraints

- T1 is offline single-player, one local Cleric, one haunt, one city hub.
- Character Progression can award gameplay XP only from Combat Core's approved `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)`.
- Combat Core's approved kill-credit payload remains unchanged.
- Legal XP fixtures must obey ADR-0001 source lookup, source lifecycle token, repeatability, and Combat fixture `kill_weight_seed` matching rules.
- T1 shipping rows may not use `NonRepeatableFirstKill` per ADR-0001.
- Character Progression's XP formula owns thresholds, level-difference modifier, per-kill caps, cap clamp, and `xp_award_minimum_nontrivial`.
- Profiled playtests are evidence of observed timing, not substitutes for deterministic formula feasibility.
- Synthetic test fixtures may exist only for Unit/Integration edge cases and must not be used as pacing or fantasy evidence.

### Requirements

- Define fixture classes with explicit legal use: pacing, formula, event-order, invalid-data, and profile harness.
- Require pacing fixture math preflight for XP per kill, kills-to-level, XP/hour, projected time-to-ding, repeatability, and lockout handling.
- Prevent profiled pacing ACs from claiming impossible kill counts or threshold crossings.
- Allow event-order tests to use synthetic transaction fixtures when legal kill-credit routes cannot reach the edge case.
- Ensure synthetic fixtures are impossible to mistake for shipping XP sources or pacing evidence.
- Keep fixture data aligned with Combat Core fixture ids, ADR-0001 lookup rows, and Character Progression tuning knobs.

## Decision

Character Progression will use a progression-owned deterministic fixture contract with explicit fixture kinds. Pacing signoff requires `LegalKillCreditRoute` fixtures and a passing `PacingMathPreflight`. Synthetic fixtures may test formulas and event ordering, but they cannot prove XP/hour, kills/level, time-to-ding, camp-session cadence, or earned progression fantasy.

### Architecture Diagram

```text
Combat Core fixtures
  SoloTrash_EvenCon_T1
  TwoTrash_Overpull_T1
  NamedSoloBlock_T1
        |
        v
ADR-0001 legal source lookup rows
  defeated_source_ref
  source_lifecycle_token_policy
  repeatability_class
  expected_kill_weight_seed_t1
        |
        v
ProgressionPacingFixtureSet_T1
        |
        +--> LegalKillCreditRoute fixtures
        |       -> PacingMathPreflight
        |       -> Profiled playtest evidence
        |
        +--> SyntheticTransaction fixtures
        |       -> formula/event-order tests only
        |
        +--> InvalidData fixtures
                -> validator/failure tests only
```

### Key Interfaces

#### Fixture Set

`ProgressionPacingFixtureSet_T1` is the top-level authored/test-data package for Character Progression pacing and edge-case tests.

```yaml
ProgressionPacingFixtureSet_T1:
  fixture_set_id: stable id
  schema_version: int
  progression_schema_version: int
  combat_fixture_package_ref: CombatCorePrototypeFixtures_T1
  xp_source_lookup_ref: ProgressionXpSourceRefLookup_T1
  tuning_profile_ref: CharacterProgressionDefaults_T1
  formula_fixtures: list of FormulaFixture ids
  event_transaction_fixtures: list of EventTransactionFixture ids
  legal_kill_credit_route_fixtures: list of LegalKillCreditRouteFixture ids
  profile_run_specs: list of ProfiledPacingRunSpec ids
```

Rules:

- Fixture set ids are stable and versioned.
- Fixture set validation is build-blocking for T1 fixture-gated criteria.
- The fixture set may contain synthetic fixtures only when `fixture_kind` explicitly marks them as synthetic.
- A fixture set with any ambiguous fixture kind is invalid.
- Fixture data must not be inferred from prose at runtime; the values used by tests must live in inspectable fixture data.

#### Fixture Kinds

```yaml
fixture_kind:
  - LegalKillCreditRoute
  - FormulaOnly
  - SyntheticEventTransaction
  - InvalidDataValidation
  - ProfileRunSpec
```

Rules:

- `LegalKillCreditRoute` is the only fixture kind that can support pacing fantasy acceptance, XP/hour, kills/level, or time-to-ding claims.
- `FormulaOnly` can test pure formula outputs without Combat/NPC lifecycle setup.
- `SyntheticEventTransaction` can test event ordering, multi-level sequencing, cap-crossing order, and transaction guards when legal kill-credit caps cannot produce the edge case.
- `InvalidDataValidation` can test failure paths, malformed rows, mismatched kill weights, and forbidden fixture states.
- `ProfileRunSpec` references one or more legal route fixtures and records how QA should run or simulate the profile. It is not itself proof that the route is legal.

#### Legal Kill-Credit Route Fixture

```yaml
LegalKillCreditRouteFixture:
  fixture_id: stable id
  fixture_kind: LegalKillCreditRoute
  local_character_id_source: ADR-0004 active-record context or deterministic test id
  starting_total_xp: int
  starting_current_level: int
  target_level: int
  combat_fixture_ref: Combat Core fixture id
  zoneId: stable zone id
  defeated_source_ref: stable source ref
  defeated_level: int
  encounter_role: Trash | Named | Camp
  encounter_role_multiplier: float
  xp_weight_seed_t1: float
  expected_kill_weight_seed_t1: float
  repeatability_class: Repeatable | RespawnLockout
  source_lifecycle_token_policy: PersistentNpcEpisode | SpawnCycle
  xp_eligible: bool
  cadence_model_ref: PacingCadenceModel id
  expected_xp_per_qualifying_kill: int
  expected_kills_to_level: int
  expected_projected_time_to_ding_seconds: int
```

Rules:

- The fixture must resolve against a valid ADR-0001 lookup row.
- `expected_kill_weight_seed_t1` must match the referenced Combat fixture's `kill_weight_seed`.
- `NonRepeatableFirstKill` is invalid for T1 legal route fixtures.
- `xp_eligible = false` fixtures may validate zero-XP behavior but cannot be used for time-to-ding projections.
- The route must define repeatability and lifecycle token behavior before it can be profiled.
- Named or camp fixtures must include lockout, respawn, and med-break assumptions; they cannot be projected as continuous repeatable routes unless repeatability makes that legal.

#### Pacing Cadence Model

```yaml
PacingCadenceModel:
  cadence_model_id: stable id
  pull_kill_seconds: int
  med_break_after_kills: int
  med_break_seconds: int
  route_reset_seconds: int
  lockout_seconds: int
  safety_notes: text
```

Rules:

- Cadence values used in acceptance criteria must come from this model.
- Profiled runs may record observed cadence, but the deterministic preflight uses declared cadence values.
- A route with `RespawnLockout` must include `lockout_seconds` and cannot be projected as continuous repeatable XP/hour unless the projection includes the lockout.
- Med-break assumptions must align with Combat Core's med-break fixture envelope when a Combat fixture is referenced.

#### Pacing Math Preflight

Before any pacing fixture can be used for a profiled playtest claim, the Editor validator computes:

```yaml
PacingMathPreflight:
  fixture_id: LegalKillCreditRouteFixture id
  xp_per_qualifying_kill: int
  xp_needed_to_target: int
  kills_to_level: ceil(xp_needed_to_target / xp_per_qualifying_kill)
  med_break_count: floor((kills_to_level - 1) / med_break_after_kills)
  projected_time_to_ding_seconds: kills_to_level * pull_kill_seconds
    + med_break_count * med_break_seconds
    + included lockout / route reset time
  projected_xp_per_hour: floor((kills_to_level * xp_per_qualifying_kill) / projected_time_to_ding_seconds * 3600)
  feasibility_status: Feasible | Infeasible
  failure_reasons: list
```

Rules:

- `xp_per_qualifying_kill` must be produced by the actual Character Progression formula and legal fixture inputs.
- A fixture with `xp_per_qualifying_kill <= 0` is infeasible for time-to-ding.
- A fixture whose projected time-to-ding falls outside its authored acceptance window is infeasible.
- A fixture whose requested level crossing requires more XP than legal per-kill caps can supply in the stated route is infeasible.
- Profiled playtest criteria may not run as T1-blocking evidence until preflight is `Feasible`.
- Preflight failure is a design/test-data failure, not a runtime fallback.

#### Synthetic Event Transaction Fixture

```yaml
SyntheticEventTransactionFixture:
  fixture_id: stable id
  fixture_kind: SyntheticEventTransaction
  starting_total_xp: int
  transaction_xp_delta: int
  expected_level_events: ordered list
  expected_spell_eligibility_events: ordered list
  expected_cap_event: optional
  bypasses_legal_kill_credit: true
  valid_for:
    - EventOrder
    - TransactionGuard
    - CapClamp
```

Rules:

- Synthetic event transaction fixtures must never reference Combat fixture ids, `defeated_source_ref`, `kill_weight_seed`, or NPC lifecycle rows.
- They must carry `bypasses_legal_kill_credit = true`.
- They may test event sequencing that legal kill-credit routes cannot produce, such as multi-level transaction emission.
- They may not be used for XP/hour, kills/level, camp-session ding cadence, anti-farming, named-route, or pacing-fantasy acceptance.
- GDD acceptance criteria using this fixture kind must say they are synthetic transaction tests, not kill-credit or pacing tests.

#### Invalid Data Fixture

```yaml
InvalidDataValidationFixture:
  fixture_id: stable id
  fixture_kind: InvalidDataValidation
  invalid_condition: enum
  expected_validator_failure: enum
```

Rules:

- Invalid fixtures exist only to prove validation failures.
- They must not be profiled.
- They must not be included in route projections.
- They must not be shipped as lookup rows.

#### Profiled Pacing Run Spec

```yaml
ProfiledPacingRunSpec:
  run_spec_id: stable id
  legal_route_fixture_id: LegalKillCreditRouteFixture id
  required_preflight_status: Feasible
  profile_duration_minutes: range
  measured_fields:
    - xp_per_hour
    - kills_to_level
    - pull_kill_seconds_observed
    - med_break_seconds_observed
    - time_to_ding_observed
    - death_penalty_occurrences
```

Rules:

- Profiled run specs cannot reference synthetic fixtures.
- Profiled results must cite the fixture set version and preflight output.
- If observed combat cadence diverges materially from the declared cadence model, the result is a tuning/input mismatch and must not silently rewrite the fixture.
- Profiled playtest can validate feel and observed timing, but it cannot legalize an infeasible fixture.

### Required T1 Fixture Gates

The Character Progression GDD must express fixture-gated pacing criteria through these gates:

1. **Schema gate**: Fixture set contains all required legal route, formula, event, invalid-data, and profile spec rows.
2. **ADR-0001 gate**: Legal route fixtures resolve to legal source lookup rows with matching Combat `kill_weight_seed`.
3. **Formula gate**: XP award, cap clamp, trivial cutoff, and min-nontrivial floor outputs match expected values.
4. **Feasibility gate**: `PacingMathPreflight` passes for every profiled pacing route.
5. **Profile gate**: QA/profile report cites the fixture version and measures the required fields.

## Alternatives Considered

### Alternative 1: Profiled Playtest Only

- **Description**: Let QA run camp loops and report observed XP/hour without deterministic fixture preflight.
- **Pros**: Captures real feel and combat cadence.
- **Cons**: Can waste time profiling mathematically impossible acceptance criteria and can hide illegal source/repeatability assumptions.
- **Rejection Reason**: Profiled playtest is necessary for feel, but deterministic feasibility must gate it.

### Alternative 2: Formula Unit Tests Only

- **Description**: Validate XP thresholds and awards with pure formula tests and skip profiled pacing fixtures.
- **Pros**: Fast and deterministic.
- **Cons**: Does not prove camp-session cadence, med-break rhythm, or route repeatability.
- **Rejection Reason**: The player fantasy depends on earned slow pacing in a route context, not only formula outputs.

### Alternative 3: One Fixture Kind For Everything

- **Description**: Use one fixture schema for formulas, event sequencing, pacing, and invalid-data tests.
- **Pros**: Simpler fixture registry.
- **Cons**: Lets synthetic event fixtures be mistaken for legal pacing evidence and lets invalid data leak into profile projections.
- **Rejection Reason**: The design needs fixture-kind separation to prevent false evidence.

### Alternative 4: Require All Event Tests To Use Legal Kill Credit

- **Description**: Even multi-level event sequencing tests must be produced through Combat kill-credit events and legal per-kill caps.
- **Pros**: Strong gameplay realism.
- **Cons**: Some event-order edge cases are intentionally unreachable through legal T1 kill-credit caps.
- **Rejection Reason**: Event systems still need edge-case coverage; synthetic event fixtures are safer when clearly banned from pacing evidence.

### Alternative 5: Fixture-Kind Separation With Math Preflight

- **Description**: Use legal route fixtures for pacing, synthetic fixtures for event/formula edge cases, and require deterministic feasibility before profile evidence.
- **Pros**: Testable, explicit, prevents impossible ACs, and preserves the earned progression fantasy.
- **Cons**: Requires fixture schema, validators, and profile-report discipline.
- **Decision**: Selected as the proposed architecture in this ADR.

## Consequences

### Positive

- Pacing acceptance criteria cannot pass on mathematically impossible fixtures.
- Profiled playtests become higher-signal because they run only after legal route feasibility is proven.
- Event-order edge cases remain testable without pretending they are legal kill routes.
- ADR-0001 lookup and repeatability rules are enforced in pacing fixtures.
- Undercon farming and named/camp XP projections become deterministic comparisons, not prose claims.
- Future tuning changes must update expected fixture math rather than silently changing pacing.

### Negative

- Character Progression needs an Editor validator for fixture kind, legal route resolution, and preflight outputs.
- QA reports must cite fixture set version and preflight output.
- Some existing GDD acceptance criteria must be rewritten to distinguish legal pacing fixtures from synthetic transaction fixtures.
- Fixture data becomes a governed artifact rather than informal test setup.

### Risks

- **Risk**: Designers treat preflight projections as final feel validation.
  **Mitigation**: Profiled playtest remains required for pacing fantasy; preflight only proves feasibility.
- **Risk**: Synthetic event fixtures become a loophole for impossible pacing claims.
  **Mitigation**: Synthetic fixtures are explicitly invalid for pacing, XP/hour, kills/level, and time-to-ding criteria.
- **Risk**: Combat fixture cadence changes after Character Progression fixtures are authored.
  **Mitigation**: Legal route fixtures cite Combat fixture ids and fail validation when referenced fixture values drift.
- **Risk**: `RespawnLockout` or future first-kill routes are projected like repeatable trash.
  **Mitigation**: Pacing preflight requires repeatability and lockout fields and rejects continuous projections that omit lockout time.

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|------------|-------------|--------------------------|
| `design/gdd/character-progression.md` | Pacing fantasy acceptance criteria require XP/hour, kills/level, camp-session cadence, and time-to-ding evidence. | Requires legal route fixtures plus deterministic preflight before profile evidence counts. |
| `design/gdd/character-progression.md` | Event criteria need deterministic multi-level and cap-crossing fixtures. | Defines `SyntheticEventTransaction` for edge-case event tests while banning it from pacing evidence. |
| `design/gdd/character-progression.md` | `ProgressionXpSourceRefLookup` rows must validate Combat fixture source refs and expected kill weights. | Requires legal route fixtures to resolve against ADR-0001 lookup rows and Combat fixture `kill_weight_seed`. |
| `design/gdd/combat-core.md` | Combat Core owns prototype fixture ids such as `SoloTrash_EvenCon_T1` and med-break feel fixtures. | Legal route fixtures reference Combat fixture ids and must fail validation on drift. |
| `docs/architecture/adr-0001-xp-source-lifecycle-registry.md` | Pacing tests must include repeatability class and lifecycle timing and must not project lockouts as continuous XP/hour routes. | Defines repeatability, lifecycle, lockout, and preflight requirements for legal pacing routes. |

## Performance Implications

- **CPU**: Editor-time fixture validation and preflight are proportional to fixture count. Runtime gameplay cost is none.
- **Memory**: Fixture data is test/editor data; no additional runtime player memory is required in shipping T1.
- **Load Time**: None for shipping builds. Editor/test fixture loading may increase validation time slightly.
- **Network**: None in T1. Future multiplayer/server pacing fixtures require a separate authority and replication ADR.

## Migration Plan

1. Revise `design/gdd/character-progression.md` to reference ADR-0005 for fixture kinds, legal pacing routes, synthetic event fixtures, and preflight requirements.
2. Rewrite `H-CPRO-EVT-02` and any similar event-order AC so impossible multi-level transactions use `SyntheticEventTransaction` and are not described as legal kill-credit pacing evidence.
3. Rewrite `H-CPRO-PACE-01` through `H-CPRO-PACE-05` to require a passing `PacingMathPreflight` before profiled playtest.
4. Add fixture schema/validation ACs for:
   - fixture kind is explicit,
   - legal routes resolve against ADR-0001 lookup rows,
   - Combat fixture kill weight matches `expected_kill_weight_seed_t1`,
   - pacing preflight computes XP per kill, kills-to-level, XP/hour, and time-to-ding,
   - synthetic fixtures are rejected for pacing/profile specs.
5. Update `docs/registry/architecture.yaml` after this ADR draft is reviewed, registering fixture state ownership, fixture loader/preflight contracts, and forbidden false-evidence patterns.

## Validation Criteria

- Editor validator fails any fixture row missing `fixture_kind`.
- Editor validator fails any `LegalKillCreditRoute` whose source ref, lifecycle policy, repeatability class, XP eligibility, or expected kill weight does not match ADR-0001 lookup data and Combat fixture data.
- Unit test proves `CampLoop_Mid_T1` preflight computes `62` XP per qualifying kill, `21` kills from level 5 to 6 under default thresholds, and projected time-to-ding inside the authored 45-120 minute window when using the declared cadence model.
- Unit test proves an undercon route at or below `trivial_cutoff` has `xp_per_qualifying_kill = 0` and is infeasible for time-to-ding.
- Editor validator fails any `ProfiledPacingRunSpec` that references a `SyntheticEventTransaction`, `FormulaOnly`, or `InvalidDataValidation` fixture.
- Integration test proves synthetic multi-level event fixtures can verify ordered per-level `LevelChangedEvent` emission without registering Combat kill-credit source refs.
- Architecture/GDD review proves all Character Progression pacing ACs distinguish deterministic preflight, legal route fixtures, synthetic transaction fixtures, and profiled playtest evidence.

## Related Decisions

- ADR-0001: XP Source Lifecycle Registry
- ADR-0002: Save Stability Barrier Protocol
- ADR-0003: Progression Baseline Snapshot Contract
- ADR-0004: First-Save Materialization and Character Identity
- `DECISIONS.md` D003 - Single-player offline through Tier 1.
- `DECISIONS.md` D007 - ADR-0001 XP Source Lifecycle Registry.
- `DECISIONS.md` D008 - ADR-0002 Save Stability Barrier Protocol.
- `DECISIONS.md` D009 - ADR-0003 Progression Baseline Snapshot Contract.
- `DECISIONS.md` D010 - ADR-0004 First-Save Materialization and Character Identity.
- `design/gdd/character-progression.md`
- `design/gdd/combat-core.md`
- `design/gdd/npc-system.md`
- `design/gdd/systems-index.md`
