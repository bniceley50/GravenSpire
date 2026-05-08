# T1-COMBAT-11 - Forbidden-Pattern Compliance Scan/Analyzer

**Status:** ready-for-dev
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

- [ ] `AC-11-01` Architecture source ingestion: the scanner reads or mirrors the
  registered forbidden-pattern list from `docs/registry/architecture.yaml:481`
  through `docs/registry/architecture.yaml:704`, and names each checked pattern
  in output.
- [ ] `AC-11-02` T1 scope scan: the gate detects forbidden networking, server,
  PvP, companion, future-class, live-LLM, and account-authority terms in T1
  production combat surfaces while avoiding test fixture false positives.
- [ ] `AC-11-03` Combat/Progression/NPC boundary scan: the gate covers
  `combat_actor_id` identity misuse, live NPC XP lookup after death, and
  Character Progression attempts to expand `PlayerKillCreditEvent`.
- [ ] `AC-11-04` Save/Load barrier scan: the gate covers direct downstream reads,
  unbounded barrier waits, partial group serialization, and unresolved barrier
  byte writes.
- [ ] `AC-11-05` Progression snapshot scan: the gate covers generic all-consumer
  baseline snapshots, illegal `visible_level` or XP/spell fields in Combat
  hydration, non-Combat reads of `CombatProgressionBaselineSnapshot`, and
  consumer mutation of progression read models.
- [ ] `AC-11-06` First-save/first-load scan: the gate covers illegal
  `local_character_id` generation or derivation, seed-only first save,
  synthesized progression state on load, and re-materialization of initialized
  records.
- [ ] `AC-11-07` Pacing fixture scan: the gate covers synthetic fixture misuse,
  profiled pacing without preflight, legal route bypass of ADR-0001 lookup, and
  ambiguous `fixture_kind`.
- [ ] `AC-11-08` Endurance scan: the gate covers ADR-0006 action-rotation,
  HUD-prominence, pulse/combo, per-ability shipping callout, and
  combat-rotation-fast regeneration forbidden patterns.
- [ ] `AC-11-09` Ability resolved-event scan: the gate inspects
  `AbilityResolvedEvent.ManaSpent`, classifies the current payload semantics,
  and fails only if shipping consumers treat the mana-only payload as universal
  Endurance spend evidence.
- [ ] `AC-11-10` Failure fixture: a deliberate forbidden-pattern sample under a
  test/tool fixture path is caught by the scanner without adding forbidden
  content to production source.
- [ ] `AC-11-11` Local gate integration: the compliance check runs either through
  `dotnet test tests\Gravenspire.Combat.Tests.csproj` or through a documented
  local command in `tests/evidence/T1-COMBAT-11/verification.md`.
- [ ] `AC-11-12` Evidence output: `tests/evidence/T1-COMBAT-11/verification.md`
  records the command, commit/build SHA, source pattern set, scanner output
  summary, failure-fixture result, and final pass/fail status.

## Acceptance Criteria Coverage Plan

| AC | Planned Evidence |
| --- | --- |
| `AC-11-01` | Scanner output lists every registered pattern name from `docs/registry/architecture.yaml:481-704`. |
| `AC-11-02` | Static scan test with at least one allowed test fixture and one caught forbidden sample for T1 scope terms. |
| `AC-11-03` | Static scan test over production `src/gameplay/combat/**`, `src/gameplay/progression/**`, and `src/gameplay/npc/**`. |
| `AC-11-04` | Static scan or analyzer test over `src/core/save/**`, progression save barrier code, and NPC lifecycle barrier code. |
| `AC-11-05` | Schema/static scan over `CombatProgressionBaselineSnapshot` and non-Combat consumer namespaces. |
| `AC-11-06` | Static scan over Save/Load and Progression paths for identity generation, synthesis, and re-materialization terms. |
| `AC-11-07` | Static scan over pacing fixture/data paths and evidence-generation paths. |
| `AC-11-08` | Static scan over gameplay HUD/presentation, ability, regen, and fixture paths for ADR-0006 quiet-Endurance terms. |
| `AC-11-09` | Analyzer row or verification table classifies `AbilityResolvedEvent.ManaSpent` as PASS, KNOWN-CARRYOVER, or FAIL with rationale. |
| `AC-11-10` | Failure-fixture test proves a deliberately banned sample is detected. |
| `AC-11-11` | TRX row or documented command output proves the local gate ran. |
| `AC-11-12` | Verification artifact includes command, SHA, pattern-set source, result summary, and pass/fail status. |

## Test Evidence Required

Preferred gate:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-11-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-11"
```

If the implementation uses a standalone tool instead of .NET tests, the tool
command must be recorded in `tests/evidence/T1-COMBAT-11/verification.md`, and
`dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"`
must still pass as the regression gate.

## Done Definition

- The compliance check names each forbidden pattern checked and whether it
  passed, failed, or is an explicitly documented known carryover.
- The deliberate failure fixture is caught.
- The local gate command is documented and reproducible.
- Any failure blocks sprint closeout.
- `T1.5-COMBAT-05` remains blocked until this story closes.

## Story Status

`T1-COMBAT-11` is ready for `/dev-story` after this recovered story file lands.
