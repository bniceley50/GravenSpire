---
paths:
  - "src/gameplay/**"
---

# Gameplay Code Rules

## Rule Set Name

Gameplay Code Rules

## Mission

These rules govern gameplay implementation under:

```text
src/gameplay/**
```

Their purpose is to ensure gameplay code is data-driven, testable, deterministic where required, frame-rate independent, interface-driven, UI-decoupled, design-traceable, and safe to evolve.

Gameplay code translates approved design documents into working mechanics. It must not silently invent design values, hardcode tuning, couple itself to UI, hide state transitions, or bypass tests.

The core gameplay-code question is:

> Does this code faithfully implement the design through configurable, testable, explicit, decoupled gameplay logic?

---

## Operating Principles

1. **Gameplay values are data**
   - All tunable gameplay values must come from external config/data files.
   - Hardcoded tuning values are prohibited in production gameplay logic.
   - Defaults may exist only as documented fallbacks, not hidden design authority.

2. **Frame-rate independence is mandatory**
   - All time-dependent gameplay calculations must use `delta`, fixed-step time, or an approved simulation clock.
   - No gameplay result may depend on frame rate unless explicitly designed and documented.

3. **Gameplay does not know UI**
   - Gameplay code must not directly reference UI classes, widgets, screens, HUDs, or visual components.
   - Gameplay emits events/state.
   - UI observes state and sends commands through approved interfaces.

4. **Every gameplay system has an interface**
   - Systems expose contracts, not concrete implementation dependencies.
   - Consumers depend on interfaces, service contracts, or typed events.

5. **State machines are explicit**
   - Every state machine must define states, allowed transitions, guards, side effects, entry behavior, exit behavior, and invalid transition handling.

6. **Logic is separate from presentation**
   - Gameplay rules, formulas, state transitions, cooldowns, scoring, damage, resources, and progression must be testable without rendering, UI, animation, audio, or scene presentation.

7. **Every gameplay feature traces to design**
   - Code must document the design document, section, feature ID, formula ID, or accepted decision it implements.
   - If no design source exists, mark the implementation as provisional and request design clarification.

8. **No static singletons for game state**
   - Static global game-state singletons are prohibited.
   - Use dependency injection, explicit context objects, service locators only if approved, or scoped runtime containers.

9. **Tests are required**
   - Gameplay logic requires automated unit tests.
   - Multi-system gameplay behavior requires integration tests or documented QA/playtest evidence according to project QA rules.

10. **Engine-version safety**
    - Engine-specific APIs, lifecycle methods, timing APIs, signals/events, resource loading, and input APIs must be verified against the project’s engine-reference docs when version-sensitive.

11. **Self-healing**
    - When hardcoded values, missing config, invalid states, UI coupling, missing tests, or design drift are detected, stop, classify the issue, repair safely, verify, and report.

12. **Bounded self-learning**
    - Durable gameplay-code lessons must be explicit, reviewable, reversible, and stored in approved project files or supported project memory.
    - Lessons must not override design documents, architecture decisions, QA gates, or current user instructions.

---

## Scope

These rules apply to production gameplay code under:

```text
src/gameplay/**
```

This includes, where present:

- player mechanics,
- combat logic,
- abilities,
- items,
- resources,
- inventory logic,
- crafting logic,
- progression logic,
- objective logic,
- scoring,
- status effects,
- interaction rules,
- cooldowns,
- buffs/debuffs,
- state machines,
- input-to-gameplay command handling,
- gameplay events,
- save-relevant gameplay state,
- gameplay service interfaces,
- runtime gameplay systems.

---

## Non-Goals

These rules do not authorize gameplay code to:

- Make game design decisions.
- Invent tuning values.
- Modify UI code.
- Own UI presentation.
- Modify engine-core systems.
- Implement networking transport.
- Implement AI behavior architecture unless delegated.
- Modify build or DevOps infrastructure.
- Modify art/audio/narrative content.
- Bypass QA evidence requirements.
- Edit files without the active agent’s approval workflow.
- Store persistent lessons without approval.

---

## Gameplay Code State Labels

Use these labels when reviewing or implementing gameplay work:

```text
PROPOSED — suggested implementation approach, not approved.
DESIGN_TRACE_FOUND — linked to approved design source.
DESIGN_TRACE_MISSING — no design source identified.
CONFIG_SCHEMA_READY — external config schema defined.
IMPLEMENTED — code exists.
UNIT_TESTED — gameplay logic has passing unit tests.
INTEGRATION_TESTED — cross-system behavior has evidence.
QA_VERIFIED — validated by QA.
FRAME_RATE_SAFE — time-dependent behavior verified against delta/fixed-step.
DATA_DRIVEN — tunable values verified externalized.
UI_DECOUPLED — no direct UI references.
STATE_TABLE_VERIFIED — state machine has valid transition table.
BLOCKED — cannot proceed due to missing design/config/tests/approval.
SUPERSEDED — replaced by newer implementation.
```

### State Rules

- Do not mark `DATA_DRIVEN` without config evidence.
- Do not mark `FRAME_RATE_SAFE` without reviewing time-dependent calculations.
- Do not mark `UNIT_TESTED` without passing test evidence.
- Do not mark `DESIGN_TRACE_FOUND` without a design doc path, section, feature ID, or decision record.
- `IMPLEMENTED` is not equivalent to done.

---

## Source of Truth

Recommended files and directories:

```text
design/gdd/
design/systems/
design/registry/entities.yaml
design/config/
assets/data/
src/gameplay/
tests/unit/gameplay/
tests/integration/gameplay/
production/qa/gameplay/
docs/architecture/
DECISIONS.md
```

### Source-of-Truth Rules

- Read the relevant design doc before implementation.
- Check architecture decisions before creating new system boundaries.
- Check entity registry for cross-system items, resources, abilities, enemies, currencies, and status effects.
- Check existing config schemas before adding new gameplay values.
- If design docs and code conflict, flag the conflict.
- If config and design docs conflict, flag the conflict.
- If no design source exists, mark `DESIGN_TRACE_MISSING`.

---

## Gameplay Value Policy

### Gameplay Value Categories

Classify values before deciding whether they belong in config:

```text
TUNABLE_DESIGN_VALUE — must be external config/data.
FORMULA_CONSTANT — may be code if mathematically intrinsic and documented.
ENGINE_CONSTANT — may be code if engine-defined and documented.
IDENTIFIER — stable key/name/id; may be code if not tunable.
DEFAULT_FALLBACK — allowed only when documented and not authoritative.
TEST_FIXTURE_VALUE — allowed in tests only.
DEBUG_ONLY_VALUE — allowed only in debug/prototype scope and clearly labeled.
GENERATED_CONSTANT — allowed if generated from approved data source.
```

### Must Be Externalized

Externalize:

- damage,
- speed,
- acceleration,
- jump height,
- cooldowns,
- durations,
- ranges,
- spawn rates,
- resource amounts,
- XP values,
- level thresholds,
- score values,
- drop weights,
- status effect magnitudes,
- costs,
- enemy stats,
- item stats,
- ability parameters,
- tuning thresholds,
- economy values,
- difficulty values,
- input buffer windows,
- invulnerability windows,
- combo timing,
- AI gameplay parameters.

### May Be Code Constants

May remain in code if documented:

- zero,
- one,
- unit conversion constants,
- mathematical constants,
- enum values,
- array bounds derived from schema,
- protocol identifiers,
- stable localization/config keys,
- test-only sample values,
- non-tunable algorithmic constants.

### Gameplay Value Record

```md
## Gameplay Value Record

- Value:
- Category:
- Config key / source:
- Default fallback:
- Design source:
- Safe range:
- Owner:
- Validation:
```

### Hardcoded Value Review

```md
## Hardcoded Value Review

- File:
- Line:
- Value:
- Context:
- Category:
- Verdict:
  - OK_CONSTANT
  - MUST_EXTERNALIZE
  - TEST_ONLY
  - DEBUG_ONLY
  - NEEDS_DESIGN_REVIEW
- Recommendation:
```

---

## Config and Data Rules

### Config Schema Standard

Every gameplay config should define:

```md
## Gameplay Config Schema: [System]

- System:
- Source file:
- Owner:
- Design source:
- Version:
- Required keys:
- Optional keys:
- Defaults:
- Safe ranges:
- Validation:
- Migration behavior:
```

### Config Entry Format

```md
| Key | Type | Required | Default | Range | Design Source | Description |
|---|---|---|---|---|---|---|
```

### Config Rules

- Config keys must be stable.
- Config values must have safe ranges.
- Missing required config is a validation failure.
- Optional defaults must be documented.
- Defaults should be conservative and safe.
- Config load failure must not silently change gameplay balance.
- Config schema changes require migration notes if save data, content, or tests depend on them.
- Config should be validated at load time.
- Tests should cover missing, invalid, boundary, and typical config values.

### Config Failure Handling

If config is missing or invalid:

1. Identify key and source file.
2. Use documented fallback only if allowed.
3. Log or report validation failure.
4. Avoid silently using hidden code value.
5. Mark feature `BLOCKED` if required config is unavailable.
6. Add test for the failure case.

---

## Delta Time and Simulation Time

### Time-Dependent Systems

Time-dependent gameplay includes:

- movement,
- acceleration,
- cooldowns,
- timers,
- regeneration,
- damage over time,
- buffs/debuffs,
- status expiry,
- animation-driven gameplay windows,
- input buffering,
- combo timing,
- invulnerability frames,
- spawn timing,
- AI decision cooldowns,
- resource production over time,
- physics-influenced gameplay.

### Time Source Labels

Use:

```text
FRAME_DELTA — frame delta from update loop.
FIXED_DELTA — fixed-step simulation delta.
GAME_CLOCK — game-controlled simulation clock.
SERVER_TIME — authoritative network/server time.
PAUSED_TIME_AWARE — respects pause.
REAL_TIME — wall-clock time, only when intentionally independent of pause.
```

### Time Calculation Record

```md
## Time Calculation Review

- System:
- Calculation:
- Time source:
- Pauses respected:
- Frame-rate independent:
- Fixed-step required:
- Network/server authority needed:
- Tests:
```

### Time Rules

- Movement and continuous simulation multiply rates by delta.
- Cooldowns and timers decrement by approved time source.
- Fixed-step systems use fixed delta, not variable frame delta.
- Paused gameplay must not advance gameplay timers unless explicitly designed.
- Real-time timers require design approval.
- Networked gameplay may require server-authoritative time.
- Tests should compare behavior under different frame rates where feasible.

---

## UI Boundary Rules

### Forbidden

Gameplay code must not:

- import UI namespaces,
- instantiate UI widgets,
- call HUD/screen methods,
- read UI control state directly,
- store references to UI objects,
- format display strings for UI,
- trigger UI animations directly,
- play UI sounds directly,
- depend on UI scene hierarchy.

### Allowed

Gameplay code may:

- expose read-only state through interfaces,
- emit typed gameplay events,
- accept commands from input/UI/gameplay controller layer,
- provide query methods for gameplay state,
- return domain result objects,
- expose state snapshots,
- raise errors/failures through typed result channels.

### Boundary Pattern

```text
Gameplay State -> Gameplay Event / State Snapshot -> UI ViewModel -> UI
UI Action -> Gameplay Command -> Gameplay System -> Gameplay State
```

### UI Boundary Contract

```md
## Gameplay/UI Boundary Contract

- Gameplay system:
- UI consumer:
- State exposed:
- Events emitted:
- Commands accepted:
- Error/result payload:
- Ownership:
- Thread/lifecycle notes:
- Tests:
```

---

## Event / Signal / Command Contracts

### Event Contract

```md
## Gameplay Event Contract: [EventName]

- Source system:
- Consumers:
- Trigger:
- Payload:
- Required fields:
- Ordering guarantees:
- Frequency:
- Replay/save behavior:
- UI-safe:
- Network relevance:
- Tests:
```

### Command Contract

```md
## Gameplay Command Contract: [CommandName]

- Caller:
- Target system:
- Player intent:
- Payload:
- Validation:
- Success result:
- Failure result:
- Side effects:
- Cooldown/rate limit:
- Tests:
```

### Event Rules

- Events must be typed where the language/runtime allows.
- Payloads must be explicit.
- Events should not carry UI objects.
- Events should not expose mutable internal state.
- High-frequency events must be budgeted and tested.
- Gameplay events that affect save/network/analytics need explicit routing notes.
- Event subscriptions must be cleaned up to avoid leaks.

### Command Rules

- Commands represent intent.
- Gameplay systems validate commands.
- Commands return or emit clear success/failure.
- Invalid commands must not partially mutate state.
- Repeated commands should handle cooldowns, deduplication, or idempotency where needed.

---

## Interface Standards

### Gameplay System Interface Record

```md
## Gameplay System Interface: [SystemName]

- Interface name:
- Implementations:
- Consumers:
- Design source:
- Responsibilities:
- Inputs:
- Outputs:
- Commands:
- Events:
- State exposed:
- Error behavior:
- Test doubles:
```

### Interface Rules

- Depend on interfaces, not concrete classes.
- Keep interfaces focused.
- Avoid god interfaces.
- Define error/result behavior.
- Provide fake/test implementations for unit tests where useful.
- Interfaces should not leak presentation details.
- Interfaces should not expose mutable internal collections unless controlled.

---

## Dependency Injection Rules

### Dependency Record

```md
## Gameplay Dependency Record

- System:
- Dependency:
- Type:
  - Interface
  - Config
  - Data source
  - Event bus
  - Clock
  - RNG
  - Save gateway
  - Network gateway
- Injection method:
- Lifetime:
- Test substitute:
- Notes:
```

### DI Rules

- Inject dependencies through constructors, initialization methods, context objects, or approved runtime containers.
- Do not access static global game state.
- Do not create hard dependencies inside gameplay logic.
- Inject clocks for testable time behavior.
- Inject RNG for deterministic tests where randomness matters.
- Inject config providers rather than reading config files from deep gameplay logic.
- Avoid service locators unless explicitly approved and documented.

---

## Static Singleton Policy

### Prohibited

Static singletons must not hold mutable game state such as:

- player health,
- inventory,
- currency,
- progression,
- quest state,
- combat state,
- level state,
- current run state,
- save state,
- difficulty state,
- matchmaking/session state.

### Allowed With Review

Static access may be allowed for:

- immutable constants,
- pure utility functions,
- generated identifiers,
- logging facade,
- static factories with no mutable state,
- engine-provided singletons when wrapped or documented,
- test helpers in test scope only.

### Static State Review

```md
## Static State Review

- File:
- Static member:
- Mutable:
- Holds gameplay state:
- Lifetime:
- Test pollution risk:
- Replacement:
- Verdict:
```

---

## State Machine Standards

### State Machine Record

```md
## State Machine: [Name]

- System:
- Design source:
- Current state owner:
- Initial state:
- Terminal states:
- State enum/type:
- Transition trigger source:
- Invalid transition behavior:
- Tests:
```

### State Definition Table

```md
| State | Meaning | Entry Action | Exit Action | Allowed Inputs | Notes |
|---|---|---|---|---|---|
```

### Transition Table

```md
| From | Trigger / Condition | Guard | To | Side Effects | Failure Behavior |
|---|---|---|---|---|---|
```

### State Machine Rules

- Every state is documented.
- Every allowed transition is documented.
- Invalid transitions have explicit behavior:
  - reject,
  - ignore,
  - log,
  - error,
  - recover.
- State entry/exit side effects are explicit.
- State changes go through one transition function.
- No direct assignment from arbitrary callers.
- Transition guards must be testable.
- Tests cover valid transitions, invalid transitions, initial state, terminal state, and edge cases.

---

## Gameplay Formula Standards

When gameplay code implements a formula, the formula must be traceable to design.

### Formula Implementation Record

```md
## Formula Implementation Record

- Formula:
- Design doc:
- Code path:
- Config keys:
- Inputs:
- Output:
- Clamping:
- Edge cases:
- Unit tests:
```

### Formula Rules

- Do not invent formulas in code.
- Do not hide formula constants in implementation.
- Clamp only when design specifies clamping.
- Handle zero, negative, max, overflow, and invalid inputs.
- Test typical, boundary, and edge cases.
- If formula design is missing, request Systems Designer / Game Designer clarification.

---

## Randomness and Determinism

### RNG Rules

- Gameplay randomness must use an injected or approved RNG source.
- Do not call global random functions directly in gameplay logic if the result affects tests, save, networking, or replay.
- Random outcomes must be traceable to design probabilities.
- Loot/reward randomness must use approved tables.
- Tests should control RNG seed or use deterministic fake RNG.
- Networked deterministic systems require authority rules.

### RNG Record

```md
## RNG Review

- System:
- Random outcome:
- RNG source:
- Seed behavior:
- Design probability source:
- Save/replay/network relevance:
- Tests:
```

---

## Save-Relevant Gameplay State

### Save State Rules

- Save-relevant state must be explicit.
- Gameplay state that affects progression, inventory, economy, quests, unlocks, or world changes must have save/load behavior.
- Save data should reference stable IDs, not transient object references.
- Config schema changes that affect saved gameplay require migration notes.
- Load should validate state and recover from invalid/corrupt data where possible.

### Save-Relevant State Record

```md
## Save-Relevant Gameplay State

- State:
- Owner:
- Stable ID:
- Save field:
- Load validation:
- Migration behavior:
- Corruption fallback:
- Tests:
```

---

## Networking-Relevant Gameplay State

If gameplay state may become multiplayer-relevant:

- Mark authority assumptions.
- Do not design client-authoritative critical state.
- Avoid direct dependency on network transport.
- Expose commands/state suitable for a Network Programmer to adapt.
- Coordinate with Network Programmer before implementing network-specific code.

### Network Relevance Record

```md
## Network-Relevant Gameplay State

- System:
- State:
- Authority assumption:
- Replication need:
- Prediction need:
- Security risk:
- Owner:
```

---

## Test Requirements

### Unit Test Requirements

Unit tests are required for:

- formulas,
- state machines,
- validation rules,
- resource calculations,
- damage/healing rules,
- cooldown logic,
- timers,
- command validation,
- config parsing,
- inventory/resource operations,
- progression thresholds,
- status effect interactions,
- any gameplay rule with deterministic output.

### Integration Test Requirements

Integration tests or documented playtests are required for:

- multi-system interactions,
- input-to-gameplay command flow,
- save/load gameplay state,
- UI event consumption,
- audio/visual feedback triggers,
- network-adapted gameplay behavior,
- cross-system economy/progression flows.

### Test Evidence Record

```md
## Gameplay Test Evidence

- System:
- Story type:
- Test path:
- Test command:
- Result:
- Coverage:
- Edge cases covered:
- Missing tests:
- Gate level:
```

### Test Rules

- Tests must not require UI or presentation unless testing integration.
- Use fake config, fake clock, fake RNG, and fake event bus where useful.
- Tests must cover boundary cases.
- Logic with no tests is not complete.
- If test infrastructure is missing, mark `BLOCKED` or `TEST_INFRA_MISSING`, not done.

---

## Design Traceability

### Code Comment Standard

Every gameplay feature entry point should include a design trace comment:

```text
Implements: design/gdd/[system].md#[section-or-feature-id]
Feature ID: [optional]
Formula ID: [optional]
Decision: DECISIONS.md#D[NNN] if applicable
```

### Design Trace Record

```md
## Design Trace

- Feature/system:
- Code path:
- Design doc:
- Section/feature ID:
- Formula ID:
- Decision record:
- Status:
  - FOUND
  - MISSING
  - STALE
  - CONFLICTING
```

### Traceability Rules

- Do not implement major gameplay features without a design source.
- If design source is missing, mark provisional.
- If implementation differs from design, flag discrepancy.
- If design changed, update tests/config/code together.
- If code comment cites obsolete design, mark `STALE`.

---

## Gameplay Review Format

Use this for review output:

```md
## Gameplay Code Review: [System/File]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Scope

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Data-Driven Status

### Delta-Time Status

### UI Boundary Status

### Interface Status

### State Machine Status

### Dependency Injection Status

### Test Status

### Design Trace Status

### Required Follow-Up
```

### Severity

```text
GP-S1 — Critical
Can corrupt progression/save/economy, break core mechanic, introduce unsafe client authority, or block release.

GP-S2 — High
Violates gameplay architecture rule: hardcoded tuning, UI coupling, missing state table, missing tests for logic, static game state.

GP-S3 — Medium
Incomplete traceability, weak interface, partial config validation, missing edge-case tests.

GP-S4 — Low
Documentation, naming, minor cleanup, or non-blocking polish.
```

---

## Self-Learning Protocol

Self-learning means controlled improvement from approved design traces, config failures, gameplay test failures, state-machine bugs, code reviews, QA findings, and user corrections.

It does not mean autonomous design changes, hidden memory updates, or treating temporary hacks as production rules.

### What May Be Learned

The gameplay-code rule system may learn:

- approved config key conventions,
- approved interface patterns,
- approved state-machine patterns,
- approved event/command conventions,
- known hardcoded-value pitfalls,
- known frame-rate bugs,
- known UI-coupling problems,
- known static-state risks,
- validated fixes,
- recurring config failure modes,
- testing patterns that caught regressions,
- rejected approaches and why.

### What Must Not Be Learned or Stored

Do not store:

- private user data,
- private chain-of-thought,
- secrets,
- credentials,
- sensitive logs,
- unapproved design ideas as gameplay rules,
- prototype hardcoded values as production config,
- temporary test fixtures as balance data,
- one-off bugs as universal rules without evidence,
- unsupported design claims.

### Lesson Classification

Use:

```text
Confirmed Rule
Approved Gameplay Code Standard
Config Finding
Hardcoded Value Finding
Delta-Time Finding
UI Boundary Finding
Interface Finding
State Machine Finding
Dependency Injection Finding
Static State Finding
Design Trace Finding
Test Finding
QA Finding
Validated Fix
Rejected Approach
Working Assumption
Temporary Context
Superseded
```

### Lesson Storage

Store durable lessons only in reviewable approved locations such as:

```text
docs/gameplay/gameplay-code-standards.md
docs/gameplay/config-lessons.md
docs/gameplay/state-machine-lessons.md
docs/gameplay/testing-lessons.md
tasks/lessons.md
production/qa/gameplay/
production/session-state/lessons.md
```

### Lesson Format

```md
## Lesson: [Short Name]

- Status:
- Source:
- Applies to:
- Lesson:
- Evidence:
- Date/session:
- Expiry/review trigger:
- Conflicts:
```

### Lesson Validation Rules

A lesson may be stored only if:

- it is specific,
- it is approved or evidence-backed,
- it applies to gameplay code,
- it does not include sensitive data,
- it is not overgeneralized,
- it does not conflict with design documents or architecture decisions,
- it has a review trigger where appropriate.

### Lesson Expiry

Review or expire lessons when:

- design docs change,
- config schemas change,
- architecture changes,
- engine version changes,
- test strategy changes,
- QA evidence contradicts the lesson,
- Lead Programmer supersedes the rule,
- Game Designer supersedes the design,
- the lesson was temporary,
- the lesson is too broad.

---

## Self-Healing Protocol

Self-healing means detecting a gameplay-code rule failure, containing the risk, repairing safely, verifying the repair, and reporting what changed.

### Failure Types

Monitor for:

- hardcoded gameplay value,
- missing config key,
- invalid config value,
- default fallback used as hidden tuning,
- time-dependent calculation without delta/fixed time,
- direct UI dependency,
- concrete dependency where interface is required,
- static game-state singleton,
- state machine without transition table,
- invalid transition path,
- gameplay logic coupled to presentation,
- missing unit test,
- missing design trace,
- stale design trace,
- command without validation,
- event without payload contract,
- untestable code,
- ambiguous design source,
- failed tests.

### Recovery Loop

When failure occurs:

1. **Stop**
   - Do not mark the gameplay implementation complete.

2. **Identify**
   - State the exact violation.

3. **Classify**
   - Data/config, delta-time, UI boundary, interface, state machine, DI, test, traceability, or design ambiguity.

4. **Contain**
   - Mark state as `BLOCKED`, `NEEDS_FIX`, `DESIGN_TRACE_MISSING`, `DATA_DRIVEN_UNVERIFIED`, or `UNIT_TEST_MISSING`.

5. **Recover**
   - externalize values,
   - add config schema,
   - use delta/fixed time,
   - replace UI reference with event/command,
   - add interface,
   - inject dependency,
   - remove static game state,
   - add transition table,
   - separate logic from presentation,
   - add tests,
   - add design trace,
   - escalate design ambiguity.

6. **Verify**
   - Re-run or request tests.
   - Re-check config, references, state tables, and design trace.

7. **Report**
   - Summarize issue, fix, remaining risk, and required owner review.

8. **Learn**
   - Propose durable lesson only if validated and approved.

---

## Error Recovery

### Hardcoded Gameplay Value

If a gameplay value is hardcoded:

- classify value,
- externalize if tunable,
- add config key,
- add safe range,
- update design trace,
- add config validation test.

### Missing Config

If required config is missing:

- mark feature blocked,
- add schema or config entry,
- use documented fallback only if approved,
- add missing-config test.

### Delta-Time Failure

If time-dependent logic does not use delta/fixed time:

- identify time source,
- replace frame-count or fixed literal decrement with approved time source,
- test at multiple frame rates if feasible.

### UI Coupling

If gameplay references UI:

- remove UI import/reference,
- define gameplay event/state snapshot,
- define UI command or observer contract,
- add boundary test or review note.

### Missing Interface

If systems depend on concrete classes:

- extract interface,
- define responsibilities,
- inject dependency,
- add test double.

### Static Game State

If static singleton holds gameplay state:

- move state into scoped runtime object,
- inject dependency,
- reset state through lifecycle,
- add test for isolation between runs.

### Missing State Table

If a state machine has implicit transitions:

- define state table,
- define transition table,
- centralize transition method,
- add tests for valid and invalid transitions.

### Missing Unit Test

If gameplay logic lacks tests:

- classify story type,
- add unit test or mark blocked,
- test normal, boundary, invalid, and edge cases.

### Missing Design Trace

If implementation lacks design source:

- locate relevant GDD/decision,
- add trace comment,
- if none exists, mark provisional and escalate.

---

## Memory Policy

### Short-Term Task Memory

Track during current task:

- gameplay system,
- design source,
- config keys,
- values externalized,
- time source,
- event/command contracts,
- interfaces,
- dependencies,
- state machines,
- tests,
- open design questions,
- validation status.

Short-term memory expires after task completion unless explicitly stored.

### Project Memory

Project memory may store:

- approved gameplay code standards,
- config key conventions,
- state-machine conventions,
- event/command patterns,
- test patterns,
- recurring design trace issues,
- recurring hardcoded-value failures,
- validated fixes,
- rejected approaches.

### Never Store

Never store:

- secrets,
- credentials,
- private user data,
- private chain-of-thought,
- sensitive logs,
- unapproved design ideas as implementation rules,
- temporary prototype values as production config,
- unsupported gameplay claims.

---

## Feedback Policy

When the user, Lead Programmer, Game Designer, Systems Designer, QA Lead, Technical Director, or Gameplay Programmer corrects gameplay-code behavior:

1. Accept the correction.
2. Identify whether it affects:
   - config/data,
   - time calculation,
   - interface,
   - UI boundary,
   - dependency injection,
   - state machine,
   - design trace,
   - tests,
   - architecture.
3. Revise current output.
4. Ask whether the correction should become durable gameplay-code guidance if reusable.
5. Store only if approved and evidence-backed.

---

## Tool-Use Policy

This rules file does not grant tools by itself. Agents applying it must follow their own tool permissions.

General guidance:

- Use file-reading tools to inspect gameplay code, design docs, config files, tests, and architecture decisions.
- Use search tools to find hardcoded values, UI imports, static state, state assignments, config keys, and design trace comments.
- Use write/edit tools only after approval under the active agent’s workflow.
- Use Bash only if the active agent allows it and only under that agent’s safety policy.
- Do not run tests, formatters, builds, or file mutations without required approval.
- Do not use Bash to bypass Write/Edit approval.

---

## Safety Guardrails

Never allow production gameplay code under `src/gameplay/**` to:

- hardcode tunable gameplay values,
- ignore delta/fixed time for time-dependent behavior,
- directly reference UI code,
- hide gameplay state in static singletons,
- skip interfaces for gameplay systems,
- implement state machines without transition tables,
- couple logic to presentation,
- lack unit tests for gameplay logic,
- lack design trace for implemented features,
- silently use missing config defaults,
- invent design behavior not found in approved docs,
- claim done without test or file:line evidence.

---

## Output Standards

Gameplay-code reviews should be:

- design-trace-aware,
- data-driven,
- delta-time-aware,
- UI-boundary-safe,
- interface-driven,
- state-machine-explicit,
- test-evidence-backed,
- clear about uncertainty,
- specific about required follow-up.

### Review Output Format

```md
## Gameplay Code Review: [System/File]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Design Trace

### Config/Data Status

### Time/Delta Status

### UI Boundary Status

### Interface/DI Status

### State Machine Status

### Test Evidence

### Required Follow-Up
```

---

## Reflection Checklist

After reviewing or drafting gameplay code, privately check:

- Did I verify the design source?
- Did I identify all tunable values?
- Did I confirm values come from config/data?
- Did I check delta/fixed time use?
- Did I check for direct UI references?
- Did I verify interface boundaries?
- Did I check dependency injection?
- Did I check state-machine transition tables?
- Did I separate logic from presentation?
- Did I require unit tests?
- Did I avoid treating prototype shortcuts as production rules?
- Did I state uncertainty honestly?

Do not expose private chain-of-thought. Report only conclusions, evidence, and recommendations.

---

## Evaluation Checklist

Before final approval of gameplay code:

### Data and Config

- [ ] Tunable values are externalized.
- [ ] Config keys are documented.
- [ ] Config values have ranges.
- [ ] Missing config behavior is defined.
- [ ] Tests cover invalid/missing config where relevant.

### Time

- [ ] Time-dependent calculations use delta/fixed time.
- [ ] Pause behavior is defined.
- [ ] Cooldowns/timers use approved clock.
- [ ] Frame-rate independence is tested or reviewed.

### Boundaries

- [ ] No direct UI imports or references.
- [ ] UI communication uses events/state/commands.
- [ ] Gameplay systems expose clear interfaces.
- [ ] Dependencies are injected.
- [ ] No static mutable game state.

### State Machines

- [ ] States are documented.
- [ ] Transition table exists.
- [ ] Invalid transitions are handled.
- [ ] Entry/exit side effects are documented.
- [ ] Tests cover transitions.

### Tests and Traceability

- [ ] Unit tests cover gameplay logic.
- [ ] Integration tests or QA evidence cover multi-system behavior.
- [ ] Code comments cite design docs.
- [ ] Implementation matches design.
- [ ] Deviations are approved.

### Safety

- [ ] No unapproved design changes.
- [ ] No hidden hardcoded balance.
- [ ] No untestable presentation-coupled logic.
- [ ] No unapproved persistent memory updates.
- [ ] No unsupported “done” claims.

---

## Example Workflows

### Example 1: Hardcoded Damage Value

Issue:

```text
var damage = 25.0
```

Response:

```md
Finding: Hardcoded tunable gameplay value.

Fix:
- Add config key such as `combat.base_damage`.
- Define safe range and default fallback.
- Link to combat design doc.
- Add unit test for damage calculation using config.
```

---

### Example 2: Frame-Rate-Dependent Movement

Issue:

```text
position += velocity
```

Response:

```md
Finding: Time-dependent calculation missing delta.

Fix:
- Use `position += velocity * delta` or fixed-step equivalent.
- Confirm whether this runs in frame update or fixed simulation.
- Add frame-rate independence test if feasible.
```

---

### Example 3: Gameplay References HUD

Issue:

```text
player_health_system.update_hud(health)
```

Response:

```md
Finding: Direct UI coupling.

Fix:
- Emit `health_changed` gameplay event.
- UI observes event through ViewModel/controller.
- Gameplay remains UI-independent.
```

---

### Example 4: Static Game State

Issue:

```text
GameState.Instance.CurrentGold += amount
```

Response:

```md
Finding: Static mutable game state.

Fix:
- Move game state to scoped injected economy/progression service.
- Inject interface into consumers.
- Add test proving state isolation across test runs.
```

---

### Example 5: Implicit State Transition

Issue:

```text
_current_state = ATTACKING
```

Response:

```md
Finding: State transition bypasses transition table.

Fix:
- Route through `transition_to(ATTACKING, trigger)`.
- Define guard and side effects.
- Add tests for allowed and invalid transitions.
```

---

### Example 6: Missing Design Trace

Issue:

```text
A new combo mechanic appears in code with no design-doc citation.
```

Response:

```md
Finding: Design trace missing.

Fix:
- Link approved design doc and section.
- If none exists, mark implementation provisional and request Game Designer approval.
```

---

### Example 7: User Correction

User says:

```text
Input buffer windows belong in config, not code.
```

Response pattern:

```text
Understood. I’ll treat input buffer windows as tunable gameplay values that must live in config/data files. Store this as a durable gameplay-code rule only if approved.
```

---

### Example 8: Case Where the Rule Must Not Learn

User says during prototyping:

```text
Just hardcode speed for now.
```

Response pattern:

```text
That can be treated as prototype-only, not production gameplay-code policy. Production code under `src/gameplay/**` still requires speed and other tuning values to come from external config/data.
```

---

## Final Gameplay Code Rule

Gameplay code must be:

- data-driven,
- frame-rate independent,
- UI-decoupled,
- interface-based,
- dependency-injected,
- state-machine-explicit,
- logic/presentation separated,
- unit-tested,
- design-traceable,
- and honest about uncertainty.