# T1-COMBAT-11 - Forbidden-Pattern Compliance Scan/Analyzer

**Status:** Complete
**Sprint:** 1 carryover into Sprint 1.5
**Priority:** Must Have
**Layer:** Architecture / Static Compliance
**Type:** Static + Integration
**Estimate:** 1.0 day
**Manifest Version:** Sprint 1 recovery, 2026-05-08
**GDD:** `design/gdd/combat-core.md`; `design/gdd/character-progression.md`; `design/gdd/npc-system.md`; `design/gdd/save-load-persistence.md`
**Governing ADRs:** `docs/architecture/adr-0001-xp-source-lifecycle-registry.md`; `docs/architecture/adr-0002-save-stability-barrier-protocol.md`; `docs/architecture/adr-0003-progression-baseline-snapshot-contract.md`; `docs/architecture/adr-0004-first-save-materialization-and-character-identity.md`; `docs/architecture/adr-0005-progression-pacing-fixture-contracts.md`; `docs/architecture/adr-0006-endurance-resource-model.md`
**Evidence:** `tests/evidence/T1-COMBAT-11/verification.md`

## Scope

Recover the Sprint 1 forbidden-pattern compliance story as the handoff artifact
that `/dev-story` can execute.

Implementation-time scope:

- Add static compliance checks for architectural forbidden patterns registered
  under `docs/registry/architecture.yaml:481` through
  `docs/registry/architecture.yaml:704`.
- Start with deterministic grep/static scans if sufficient; promote to a
  Roslyn analyzer only if simple scans cannot reliably catch the patterns in
  the current codebase.
- Include the Sprint 1 scope-creep negative checks already described in
  `tests/architecture/README.md:5` through `tests/architecture/README.md:16`.
- Include Combat/Progression identity and snapshot boundary checks from
  ADR-0001, ADR-0002, and ADR-0003.
- Include the ADR-0006 quiet Endurance forbidden patterns at
  `docs/architecture/adr-0006-endurance-resource-model.md:79` through
  `docs/architecture/adr-0006-endurance-resource-model.md:90`.
- Include `AbilityResolvedEvent.ManaSpent` payload semantics as an explicit scan
  input. T1.5-COMBAT-02 carried this forward at
  `production/stories/t1-5-combat-02-physical-instant-conversion.md:108`
  through `production/stories/t1-5-combat-02-physical-instant-conversion.md:110`
  and again at
  `production/stories/t1-5-combat-02-physical-instant-conversion.md:145`
  through `production/stories/t1-5-combat-02-physical-instant-conversion.md:147`.

Source trace: `production/sprints/sprint-1.md:577` through
`production/sprints/sprint-1.md:617`.

## Out Of Scope

- No Combat, Progression, NPC, Save/Load, fixture, GDD, ADR, DECISIONS.md, or
  sprint-plan behavior changes as part of this story file recovery.
- No tuning of FEEL-01, FEEL-03, med-break pacing, XP formulas, or fixture
  values.
- No expansion of `PlayerKillCreditEvent`, `PlayerDeathEvent`,
  `CombatActorDeathEvent`, or `CombatProgressionBaselineSnapshot`.
- No FishNet, networking, server authority, PvP, companions, Warrior,
  Enchanter, live LLM, account identity, prediction, lag compensation, or T2+
  architecture.
- No rewrite of `docs/registry/architecture.yaml`; it is a read-only source for
  scanner coverage unless a later architecture decision explicitly updates it.
- No production source mutation from deliberate forbidden-pattern failure
  samples; samples must live under test/tool fixture paths only.

## Dependencies

- `T1-COMBAT-09b` completed the same-frame kill-credit/save-barrier consistency
  seam and preserved the approved four-field `PlayerKillCreditEvent`.
- `T1.5-COMBAT-02` completed the physical-instant resource split and made
  `AbilityResolvedEvent.ManaSpent` payload semantics a required scan input.
- ADR-0006 is Accepted, so the Endurance forbidden patterns are now part of the
  compliance surface.
- `T1.5-COMBAT-05` remains blocked until this story is implemented and closed.

## Likely Files Touched During Implementation

- `tests/architecture/**`
- `tools/architecture/**`
- `tests/Gravenspire.Combat.Tests.csproj` if new C# architecture tests must be
  compiled by the existing .NET test bridge.
- `tests/evidence/T1-COMBAT-11/**`
- `production/qa/combat/**` only if the implementation needs a human-readable
  compliance summary in addition to `tests/evidence/T1-COMBAT-11/verification.md`.

Read-only implementation sources:

- `docs/registry/architecture.yaml`
- `docs/architecture/adr-0001-xp-source-lifecycle-registry.md`
- `docs/architecture/adr-0002-save-stability-barrier-protocol.md`
- `docs/architecture/adr-0003-progression-baseline-snapshot-contract.md`
- `docs/architecture/adr-0004-first-save-materialization-and-character-identity.md`
- `docs/architecture/adr-0005-progression-pacing-fixture-contracts.md`
- `docs/architecture/adr-0006-endurance-resource-model.md`
- `tests/architecture/README.md`
- `src/**`
- `assets/data/combat/**`

## Composition Trace

This recovered story combines three source streams:

- Original Sprint 1 plan lines for `T1-COMBAT-11` at
  `production/sprints/sprint-1.md:577` through
  `production/sprints/sprint-1.md:617`.
- Registered forbidden patterns from `docs/registry/architecture.yaml:481`
  through `docs/registry/architecture.yaml:704`.
- Sprint 1.5 carryovers from the Endurance resource split:
  ADR-0006's forbidden Endurance patterns and the
  `AbilityResolvedEvent.ManaSpent` payload scan input.

The implementation should produce a durable local gate, not a one-off manual
grep transcript. It may be grep/static-scan based if the checks are
deterministic, named, and tested. It should promote to a Roslyn analyzer only
for patterns where text scanning cannot reliably avoid false passes.

## Implementation Trace

- `tests/architecture/forbidden_pattern_compliance_scan_test.cs` implements the
  T1-COMBAT-11 compliance gate as deterministic NUnit architecture tests.
- `tests/Gravenspire.Combat.Tests.csproj` explicitly includes
  `tests/architecture/*.cs`, keeping the scanner inside the existing local
  combat test bridge.
- The scanner ingests every registered forbidden pattern from
  `docs/registry/architecture.yaml`, maps each registry id to an explicit
  evaluator, then adds the ADR-0006 Endurance addendum because the registry has
  not yet absorbed those patterns.
- Registry status drift is reported as `KNOWN-CARRYOVER` when a registry row is
  still `proposed` but the governing ADR is already `Accepted`; the scanner
  does not downgrade the pattern.
- Real violation scans are path-scoped to production source/data surfaces.
  Documentation, evidence, and prototype text are not treated as production
  failures; production data scans cover `assets/data/**/*.json` rather than only
  current combat fixture JSON.
- Frozen event and snapshot contracts use reflection checks where possible.
- `AbilityResolvedEvent.ManaSpent` is classified as `KNOWN-CARRYOVER` in the
  current state. It becomes a failure only if shipping production consumers
  treat the mana-only payload as universal resource-spend evidence or if the
  physical-resource guard / Bash carryover coverage disappears.

## Minimum Forbidden-Pattern Coverage

The compliance gate must name and evaluate each pattern below.

### T1 Scope Creep

- No FishNet, networking placeholders, replicated combat authority, server
  validation, account identity, prediction, lag compensation, or server combat
  state in T1 Combat Core code.
- No PvP, duels, friendly fire, companions, Sister Elara combat behavior,
  Warrior, Enchanter, live LLM calls, or server combat state.

### Combat / Progression / NPC Identity Boundaries

- `combat_actor_id` must not be used as XP identity, persistence identity,
  dedupe key, save identity, or source lifecycle identity.
- Character Progression must not resolve XP metadata by querying mutable live
  NPC or spawn runtime state after Combat kill credit fires.
- Character Progression must not demand expanded Combat kill-credit payload
  fields such as defeated level, encounter role, repeatability, source
  lifecycle metadata, XP values, spell data, or progression transaction ids.
- Legal kill-credit pacing fixtures must not bypass ADR-0001 source lookup
  rows, expected Combat kill weight matching, lifecycle policy, repeatability
  class, or XP eligibility.

### Save / Load Barrier Boundaries

- Save/Load must not directly read guarded downstream payloads while the owner
  can be in a transient save-unsafe state.
- Save/Load and downstream barrier owners must not wait indefinitely for a
  save-stability barrier to settle.
- Partial group payload serialization must not occur for declared barrier
  groups.
- Unresolved save barriers must not write bytes.

### Progression Baseline Snapshot Boundaries

- Generic all-consumer `ProgressionBaselineSnapshot` must not appear as a
  shared handoff to Combat, UI, spells, or future consumers.
- Combat Core must not consume `visible_level` as a combat actor level input.
- UI, Menus, Spell Memorization, Class Design, vendors, drops, and other
  non-Combat consumers must not read `CombatProgressionBaselineSnapshot`.
- Consumers must not mutate Character Progression state through snapshots or
  read models.
- `visible_level`, XP progress, `spell_eligibility_tier`, spell ids, spell
  content, UI fields, or Combat current resources must not enter
  `CombatProgressionBaselineSnapshot`, whose current legal production shape is
  anchored at `src/gameplay/combat/CombatProgressionBaselineSnapshot.cs:37`
  through `src/gameplay/combat/CombatProgressionBaselineSnapshot.cs:45`.

### First-Save / First-Load Identity Boundaries

- Save/Load must not generate, regenerate, repair, replace, or derive
  `local_character_id`.
- First successful save must not persist only seed data when required
  downstream materializers have not produced their payloads.
- Character Progression must not synthesize missing
  `CharacterProgressionSaveState` from `starting_class_id` during first load or
  Continue.
- No system may re-run first-save materialization for an already initialized
  record.
- `local_character_id` must not be derived from player-authored data, save
  paths, account names, device usernames, or Combat runtime ids.

### Progression Pacing Fixture Boundaries

- Synthetic event, formula-only, and invalid-data fixtures must not be used as
  evidence for XP/hour, kills/level, time-to-ding, camp-session cadence, or
  pacing fantasy acceptance.
- Profiled pacing must not count as T1-blocking evidence unless the referenced
  legal kill-credit route has a passing pacing-math preflight.
- Respawn lockout, named, camp, or future first-kill routes must not be
  projected as continuous repeatable XP/hour routes unless lockout and one-time
  limits are explicit.
- Every `ProgressionPacingFixtureSet_T1` row must declare exactly one legal
  `fixture_kind`.

### Quiet Endurance Boundaries

- Endurance must not become an action-rotation bar, priority bar, combo meter,
  or GCD-like resource loop.
- Endurance HUD treatment must not gain prominence above mana.
- Pulse, combo, animation, or celebratory treatment must not imply tactical
  cycling intent.
- Per-ability Endurance callouts must not appear in shipping HUD surfaces unless
  explicitly QA/debug-only.
- Combat-rotation-fast Endurance regeneration must not appear.

### AbilityResolvedEvent Payload Semantics

- The scanner must inspect `AbilityResolvedEvent` at
  `src/gameplay/combat/events/CombatAbilityLifecycleEvents.cs:36` through
  `src/gameplay/combat/events/CombatAbilityLifecycleEvents.cs:44`.
- Current `ManaSpent`-only payload semantics are a known Sprint 1.5 carryover,
  not an automatic implementation failure by themselves.
- The gate must fail if a shipping consumer treats `ManaSpent` as a universal
  resource-spend payload for Endurance or hides Bash Endurance spend behind a
  mana-only assertion.
- The gate output must classify this scan result explicitly as PASS,
  KNOWN-CARRYOVER, or FAIL.

## Frozen Contracts

- `PlayerKillCreditEvent` stays the four-field event at
  `src/gameplay/combat/events/CombatDeathEvents.cs:16` through
  `src/gameplay/combat/events/CombatDeathEvents.cs:20`.
- `PlayerDeathEvent` remains additive and unchanged at
  `src/gameplay/combat/events/CombatDeathEvents.cs:25` through
  `src/gameplay/combat/events/CombatDeathEvents.cs:31`.
- `CombatActorDeathEvent` remains unchanged at
  `src/gameplay/combat/events/CombatDeathEvents.cs:8` through
  `src/gameplay/combat/events/CombatDeathEvents.cs:11`.
- `CombatProgressionBaselineSnapshot` remains consumer-scoped and limited to
  its current legal fields at
  `src/gameplay/combat/CombatProgressionBaselineSnapshot.cs:37` through
  `src/gameplay/combat/CombatProgressionBaselineSnapshot.cs:45`.
- ADR-0006 Endurance scan coverage must not mutate ADR-0003's progression
  baseline contract.

## Acceptance Criteria

- [x] `AC-11-01` Architecture source ingestion: the scanner reads or mirrors the
  registered forbidden-pattern list from `docs/registry/architecture.yaml:481`
  through `docs/registry/architecture.yaml:704`, and names each checked pattern
  in output.
- [x] `AC-11-02` T1 scope scan: the gate detects forbidden networking, server,
  PvP, companion, future-class, live-LLM, and account-authority terms in T1
  production combat surfaces while avoiding test fixture false positives.
- [x] `AC-11-03` Combat/Progression/NPC boundary scan: the gate covers
  `combat_actor_id` identity misuse, live NPC XP lookup after death, and
  Character Progression attempts to expand `PlayerKillCreditEvent`.
- [x] `AC-11-04` Save/Load barrier scan: the gate covers direct downstream reads,
  unbounded barrier waits, partial group serialization, and unresolved barrier
  byte writes.
- [x] `AC-11-05` Progression snapshot scan: the gate covers generic all-consumer
  baseline snapshots, illegal `visible_level` or XP/spell fields in Combat
  hydration, non-Combat reads of `CombatProgressionBaselineSnapshot`, and
  consumer mutation of progression read models.
- [x] `AC-11-06` First-save/first-load scan: the gate covers illegal
  `local_character_id` generation or derivation, seed-only first save,
  synthesized progression state on load, and re-materialization of initialized
  records.
- [x] `AC-11-07` Pacing fixture scan: the gate covers synthetic fixture misuse,
  profiled pacing without preflight, legal route bypass of ADR-0001 lookup, and
  ambiguous `fixture_kind`.
- [x] `AC-11-08` Endurance scan: the gate covers ADR-0006 action-rotation,
  HUD-prominence, pulse/combo, per-ability shipping callout, and
  combat-rotation-fast regeneration forbidden patterns.
- [x] `AC-11-09` Ability resolved-event scan: the gate inspects
  `AbilityResolvedEvent.ManaSpent`, classifies the current payload semantics,
  and fails only if shipping consumers treat the mana-only payload as universal
  Endurance spend evidence.
- [x] `AC-11-10` Failure fixture: a deliberate forbidden-pattern sample under a
  test/tool fixture path is caught by the scanner without adding forbidden
  content to production source.
- [x] `AC-11-11` Local gate integration: the compliance check runs either through
  `dotnet test tests\Gravenspire.Combat.Tests.csproj` or through a documented
  local command in `tests/evidence/T1-COMBAT-11/verification.md`.
- [x] `AC-11-12` Evidence output: `tests/evidence/T1-COMBAT-11/verification.md`
  records the command, implementation/commit SHA, source pattern set, scanner
  output summary, failure-fixture result, and final pass/fail status.

## Acceptance Criteria Coverage

| AC | Status | Evidence |
| --- | --- | --- |
| `AC-11-01` | Covered | `test_ac_11_01_registry_and_adr0006_addendum_patterns_are_named_and_evaluated` verifies the registry pattern list, explicit evaluator map, ADR-0006 addendum list, and accepted-ADR registry-status drift classification. |
| `AC-11-02` | Covered | `test_ac_11_02_t1_scope_terms_are_absent_from_production_surfaces_and_failure_sample_is_caught` scans production Combat source and production JSON data only, with a path-labelled test fixture sample proving detection. |
| `AC-11-03` | Covered | `test_ac_11_03_combat_progression_npc_identity_boundaries_hold` verifies `PlayerKillCreditEvent` shape, non-Combat absence of `combat_actor_id`, approved Progression field reads, and no live NPC dependency. |
| `AC-11-04` | Covered | `test_ac_11_04_save_load_barrier_boundaries_hold` verifies the grouped save coordinator fails before writer calls and has no unbounded wait calls in `src/core/save/**`. |
| `AC-11-05` | Covered | `test_ac_11_05_progression_snapshot_boundaries_hold` verifies `CombatProgressionBaselineSnapshot` shape and prevents generic baseline / non-Combat snapshot consumers. |
| `AC-11-06` | Covered | `test_ac_11_06_first_save_and_identity_boundaries_hold` scans Save/Load and Progression paths for illegal local-character-id generation, first-load synthesis, re-materialization terms, and seed-only first-save materialization bypass patterns. |
| `AC-11-07` | Covered | `test_ac_11_07_progression_pacing_fixture_boundaries_hold` scans production source plus all production JSON data for synthetic pacing evidence misuse, then uses whole-file matching for ambiguous multi-line `fixture_kind` cases. |
| `AC-11-08` | Covered | `test_ac_11_08_quiet_endurance_boundaries_hold` scans production Combat source/data for ADR-0006 quiet-Endurance forbidden patterns with Endurance-scoped regexes. |
| `AC-11-09` | Covered | `test_ac_11_09_ability_resolved_event_payload_is_known_carryover_not_universal_spend` classifies current `AbilityResolvedEvent.ManaSpent` semantics as `KNOWN-CARRYOVER` and fails if shipping consumers read it as universal spend or if the physical-resource guard disappears. |
| `AC-11-10` | Covered | `test_ac_11_10_deliberate_failure_samples_are_caught_without_production_mutation` proves path-labelled `tests/architecture/fixtures/**` failure samples are caught and absent from production text. |
| `AC-11-11` | Covered | `tests/Gravenspire.Combat.Tests.csproj` includes `architecture/*.cs`; `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` passed `159/159`. |
| `AC-11-12` | Covered | `tests/evidence/T1-COMBAT-11/verification.md` records the command, implementation/commit SHA, source pattern set, per-pattern pass/fail/known-carryover table, and file:line evidence. |

## Test Evidence Required

Preferred gate:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-11-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-11"
```

If the implementation uses a standalone tool instead of .NET tests, the tool
command must be recorded in `tests/evidence/T1-COMBAT-11/verification.md`, and
`dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"`
must still pass as the regression gate.

Implementation gate used:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"
```

Result: PASS, `159/159`. The `10` added architecture tests are the
T1-COMBAT-11 compliance scanner gate.

## Done Definition

- The compliance check names each forbidden pattern checked and whether it
  passed, failed, or is an explicitly documented known carryover.
- The deliberate failure fixture is caught.
- The local gate command is documented and reproducible.
- Any failure blocks sprint closeout.
- `T1.5-COMBAT-05` remains blocked until this story closes.

## Story Status

`T1-COMBAT-11` is complete.

## Completion Notes

**Completed:** 2026-05-08

**Verdict:** COMPLETE WITH NOTES

**Criteria:** 12/12 covered. `AC-11-01` through `AC-11-08` and `AC-11-10`
through `AC-11-12` pass. `AC-11-09` passes with known carryover: current
`AbilityResolvedEvent.ManaSpent` remains mana-only, while the scanner now fails
shipping universal-spend misuse and preserves Bash physical-resource coverage.

**Implementation chain:** Scanner-state commits are `015d417` (story scaffold
and implementation parent baseline), `0e603a7` (initial scanner
implementation), `f4f191a` (review fix #1), and `5bbc665` (review fix #2 and
closure-eligible scanner/evidence state). Current `496ebc6` is a
provenance-only verification restructure that documents that state in the
chain-table form; it is not a fifth scanner implementation iteration.

**Deviations:** None blocking. The only completion note is the intentional
`AC-11-09` known-carryover classification for the mana-only
`AbilityResolvedEvent.ManaSpent` payload.

**Test Evidence:** `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger
"console;verbosity=minimal"` passed `159/159`. Evidence is recorded in
`tests/evidence/T1-COMBAT-11/verification.md`, including command, source
pattern set, per-pattern pass/fail/known-carryover output, failure-fixture
result, and hygiene/staged-footprint gates.

**Code Review:** Complete after two review-fix commits; no unresolved blocking
findings remain.

**Next:** `T1.5-COMBAT-05` is unblocked for profiled rerun evidence summary.
