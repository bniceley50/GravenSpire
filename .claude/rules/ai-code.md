---
paths:
  - "src/ai/**"
---

# AI Code Rules

## Rule Set Name

AI Code Rules

## Mission

These rules govern game AI implementation under:

```text
src/ai/**
```

Their purpose is to ensure AI behavior is performant, data-driven, debuggable, readable to players, testable, scalable, network-safe, and maintainable.

This rule set covers AI behavior systems, behavior trees, utility AI, AI state machines, perception, pathfinding, tactical decision-making, group behavior, role assignment, debugging hooks, profiling, and AI validation.

This file does **not** govern LLM dialogue safety. LLM prompt and moderation behavior belongs to the separate LLM moderation rules.

The core AI-code question is:

> Can this AI make believable, readable, performant decisions using data-driven rules while remaining debuggable, testable, and safe under production conditions?

---

## Operating Principles

1. **Performance budget is mandatory**
   - AI update cost must stay within the approved frame budget.
   - Default budget:

```text
AI update budget: <= 2ms per frame
```

   - Budget compliance requires profiling evidence.

2. **All AI parameters are data-driven**
   - Behavior-tree weights, utility weights, perception ranges, timers, cooldowns, aggression thresholds, role priorities, formation rules, and flanking parameters must come from data/config files.
   - Hardcoded tunable AI values are prohibited in production AI code.

3. **AI must be debuggable**
   - Every AI system must expose visualization or inspection hooks for relevant state.
   - Required debug surfaces include paths, perception cones, target selection, decision scores, behavior-tree state, state-machine state, group role, and tactical intent where applicable.

4. **AI must be readable to players**
   - AI should telegraph intentions before consequential actions.
   - Players need time and information to read, learn, and react.
   - Unreadable AI is a design and implementation failure even if it is technically optimal.

5. **Prefer structured decision systems**
   - Prefer utility AI, behavior trees, goal-oriented action planning, or explicit state machines over hardcoded if/else chains.
   - Simple finite-state machines are acceptable for simple behaviors if states and transitions are documented.
   - Hardcoded nested decision chains are allowed only for trivial or temporary cases and must be labeled.

6. **Group behavior must be data-driven**
   - Formation, flanking, role assignment, spacing, target focus, retreat, and coordination rules must be configurable from data.
   - Group AI must avoid clustering, deadlocks, and role starvation.

7. **AI state transitions must be logged**
   - All AI state machines must log transitions in debug/development builds.
   - Logs must be structured, rate-limited, and safe for high-agent-count scenarios.

8. **Never trust network-originated AI input**
   - AI commands, target selections, tactical hints, perception updates, or state changes received from clients or network sources must be validated before use.
   - Server/authoritative simulation owns consequential AI state in multiplayer.

9. **AI should be fun, not omniscient**
   - AI should make believable mistakes where design requires.
   - AI should not use hidden information unless explicitly designed and disclosed through fair telegraphs.

10. **Self-healing**
    - When AI exceeds budget, loses data, gets stuck, sees invalid targets, enters invalid states, spams logs, or receives unsafe network input, stop, contain, recover, verify, and report.

11. **Bounded self-learning**
    - Durable AI lessons must be explicit, reviewable, reversible, and stored in approved files or supported project memory.
    - Lessons must not override design docs, performance budgets, architecture decisions, QA evidence, or current user instructions.

---

## Scope

These rules apply to production AI code under:

```text
src/ai/**
```

This includes, where present:

- behavior-tree systems,
- utility AI systems,
- AI state machines,
- perception systems,
- target selection,
- pathfinding requests and steering,
- tactical movement,
- cover selection,
- flanking logic,
- formations,
- role assignment,
- squad/group coordination,
- combat AI decision logic,
- non-combat NPC AI,
- AI debug visualization,
- AI profiling hooks,
- AI data/config loading,
- AI test scaffolds,
- AI network validation boundaries.

---

## Non-Goals

These rules do not authorize AI code to:

- Design enemy archetypes or behaviors independently.
- Make game design decisions.
- Modify core engine navigation systems without engine/lead approval.
- Modify gameplay state directly outside approved gameplay interfaces.
- Implement multiplayer networking transport.
- Implement LLM dialogue generation or moderation.
- Make animation, VFX, audio, or UI decisions.
- Bypass performance profiling.
- Skip data-driven configuration.
- Hide unsafe or invalid AI behavior.
- Edit files without the active agent’s write-approval workflow.
- Store persistent lessons without approval.

---

## AI Implementation State Labels

Use these labels when reviewing or implementing AI work:

```text
PROPOSED — suggested AI behavior or architecture, not approved.
DESIGN_TRACE_FOUND — linked to approved design source.
DESIGN_TRACE_MISSING — no design source identified.
DATA_SCHEMA_READY — AI config/data schema defined.
DATA_DRIVEN_VERIFIED — tunable values verified externalized.
IMPLEMENTED — code exists.
UNIT_TESTED — deterministic AI logic has passing tests.
SIMULATION_TESTED — AI tested in runtime/simulation scenario.
PROFILED — AI budget measured.
BUDGET_PASS — AI cost within budget.
BUDGET_FAIL — AI cost exceeds budget.
DEBUG_HOOKS_READY — debug hooks/visualization defined or implemented.
TELEGRAPH_REVIEWED — player-readability review completed.
NETWORK_VALIDATED — network-originated AI input validation verified.
QA_VERIFIED — QA validated behavior.
BLOCKED — missing design/data/tests/profiling/approval.
SUPERSEDED — replaced by newer implementation.
```

### State Rules

- Do not mark `BUDGET_PASS` without profiling evidence.
- Do not mark `DATA_DRIVEN_VERIFIED` without config/data evidence.
- Do not mark `TELEGRAPH_REVIEWED` without animation/audio/VFX/UX or gameplay-readability review where relevant.
- Do not mark `NETWORK_VALIDATED` without trust-boundary evidence.
- `IMPLEMENTED` is not equivalent to complete.

---

## Source of Truth

Recommended files and directories:

```text
design/gdd/ai/
design/gdd/enemies/
design/gdd/combat/
design/ai/behavior-trees.md
design/ai/utility-ai.md
design/ai/perception.md
design/ai/pathfinding.md
design/ai/group-behavior.md
design/ai/telegraphing.md
design/ai/ai-debugging.md
design/config/ai/
assets/data/ai/
src/ai/
tests/unit/ai/
tests/integration/ai/
production/qa/ai/
production/session-state/lessons.md
```

### Source-of-Truth Rules

- Read relevant AI/enemy/combat design docs before implementing behavior.
- Use design-approved behavior goals.
- Use data/config for AI tuning.
- Check existing AI architecture before adding new decision systems.
- If design docs and AI implementation conflict, flag the conflict.
- If AI config and design docs conflict, flag the conflict.
- If no design source exists, mark `DESIGN_TRACE_MISSING`.

---

## AI Performance Budget

### Default Budget

```text
AI update budget: <= 2ms per frame
```

This is an aggregate frame budget unless a Technical Director or Performance Analyst defines a more specific per-agent/per-system/platform budget.

### Budget Record

```md
## AI Performance Budget

- System:
- Platform:
- Build:
- Scenario:
- Agent count:
- AI systems active:
- Budget:
- Actual median:
- Actual p95:
- Actual p99:
- Worst frame:
- Profiler/tool:
- Verdict:
- Confidence:
```

### Budget Rules

- Profile under representative enemy/NPC counts.
- Include worst-case combat scenario if AI runs during combat.
- Track median, p95, p99, and worst-frame cost where possible.
- Separate:
  - perception,
  - pathfinding,
  - decision-making,
  - behavior-tree evaluation,
  - utility scoring,
  - group coordination,
  - steering,
  - debug visualization.
- Debug visualization cost must not be included in release-budget claims unless debug hooks are enabled in release.
- If profiling does not exist, mark `PROFILE_MISSING`, not pass.

### Budget Failure Handling

If AI exceeds budget:

1. Identify subsystem cost.
2. Reduce update frequency where safe.
3. Use LOD/throttling for distant or low-priority AI.
4. Batch perception/pathfinding.
5. Cache expensive queries.
6. Stagger updates across frames.
7. Simplify decision evaluation.
8. Profile again before claiming improvement.

---

## AI Parameter Data Policy

### Must Be Data-Driven

The following must come from data/config:

- behavior-tree weights,
- utility scores and weights,
- perception ranges,
- sight cone angles,
- hearing ranges,
- memory durations,
- search durations,
- patrol timings,
- reaction delays,
- cooldowns,
- attack windup and recovery timing,
- aggression thresholds,
- retreat thresholds,
- morale thresholds,
- formation spacing,
- flanking preference,
- role priority,
- target priority,
- path recalculation intervals,
- stuck thresholds,
- investigation timers,
- alert decay timers,
- difficulty-scaling modifiers,
- group coordination parameters.

### AI Parameter Schema

```md
## AI Config Schema: [AI System / Archetype]

- System:
- Archetype:
- Source file:
- Design source:
- Version:
- Required keys:
- Optional keys:
- Defaults:
- Safe ranges:
- Difficulty scaling:
- Validation:
- Migration behavior:
```

### Parameter Table

```md
| Key | Type | Required | Default | Range | Design Source | Description |
|---|---|---|---|---|---|---|
```

### Data Rules

- Missing required AI config is a validation failure.
- Optional defaults must be documented.
- Defaults must not become hidden design authority.
- Safe ranges must be defined.
- Invalid config must fail loudly in development.
- Runtime fallback must be explicit.
- Config schema changes need migration notes when they affect tests, saves, spawned archetypes, or balancing.

---

## AI Decision Architecture

### Acceptable Decision Models

Use one or more of:

```text
Behavior Tree
Utility AI
Finite State Machine
Hierarchical State Machine
Goal-Oriented Action Planning
Scripted Sequence
Simple Rule Set
```

### Decision Model Selection Record

```md
## AI Decision Model Selection

- AI type:
- Behavior complexity:
- Required reactivity:
- Required designer control:
- Performance risk:
- Debugging needs:
- Selected model:
- Alternatives considered:
- Reason:
```

### Selection Guidance

- Use **behavior trees** for hierarchical, readable, designer-authored behaviors.
- Use **utility AI** for continuous prioritization among competing goals.
- Use **finite-state machines** for small, discrete behavior modes.
- Use **GOAP/planning** only when planning complexity is justified and budgeted.
- Use **scripted sequences** only for authored moments, tutorials, or cinematic behavior.
- Avoid deeply nested if/else chains except for trivial glue logic.

---

## Behavior Tree Standards

### Behavior Tree Record

```md
## Behavior Tree: [Name]

- AI archetype:
- Design source:
- Root goal:
- Blackboard keys:
- Services:
- Decorators/conditions:
- Tasks/actions:
- Abort rules:
- Update frequency:
- Debug visualization:
- Tests:
```

### Blackboard Key Table

```md
| Key | Type | Source | Lifetime | Required | Notes |
|---|---|---|---|---|---|
```

### Behavior Tree Rules

- Behavior-tree values are data-driven.
- Blackboard keys must be documented.
- Conditions/decorators must be testable.
- Tasks should be small and focused.
- Long-running tasks need cancellation behavior.
- Abort rules must be explicit.
- Behavior-tree transitions must be visible in debug mode.
- Avoid expensive tree-wide evaluation every frame unless profiled.
- Avoid behavior trees that encode hidden gameplay design without a design trace.

---

## Utility AI Standards

### Utility AI Record

```md
## Utility AI: [System / Archetype]

- AI archetype:
- Design source:
- Decisions scored:
- Considerations:
- Curves:
- Weights:
- Tie-break rules:
- Minimum score threshold:
- Update frequency:
- Debug visualization:
- Tests:
```

### Utility Score Table

```md
| Option | Consideration | Input | Curve | Weight | Range | Design Source |
|---|---|---|---|---:|---|---|
```

### Utility Rules

- Utility weights must be data-driven.
- Utility inputs must be explicit.
- Score ranges must be normalized or documented.
- Tie-break rules must be deterministic unless randomness is designed.
- Scores must be inspectable in debug mode.
- Avoid overfitting utility weights to one scenario.
- Tests should cover typical, boundary, and conflicting-goal cases.

---

## AI State Machine Standards

### State Machine Record

```md
## AI State Machine: [Name]

- AI archetype/system:
- Design source:
- Initial state:
- Terminal states:
- Current state owner:
- Transition trigger source:
- Invalid transition behavior:
- Debug log:
- Tests:
```

### State Table

```md
| State | Meaning | Entry Action | Exit Action | Allowed Inputs | Telegraph Needed |
|---|---|---|---|---|---|
```

### Transition Table

```md
| From | Trigger / Condition | Guard | To | Side Effects | Debug Log | Failure Behavior |
|---|---|---|---|---|---|---|
```

### State Machine Rules

- Every state must be documented.
- Every valid transition must be documented.
- Invalid transitions must be handled explicitly.
- State transitions must go through one transition function.
- Direct arbitrary state assignment is prohibited.
- Entry/exit side effects must be clear.
- Transitions must be logged in debug/development builds.
- Tests must cover valid transitions, invalid transitions, initial state, and terminal states.

---

## AI State Transition Logging

### Transition Log Format

```md
## AI Transition Log Event

- AI entity ID:
- Archetype:
- From state:
- To state:
- Trigger:
- Guard result:
- Timestamp/frame:
- Position:
- Target ID, if safe:
- Reason:
```

### Logging Rules

- Transition logs must be structured.
- Transition logs must be rate-limited.
- Logs must be disabled or reduced in release unless explicitly required.
- Do not log private player data.
- Do not log secret anti-cheat details.
- Use stable debug IDs where possible.
- Logging must not push AI over budget.

---

## Perception System Standards

### Perception Record

```md
## AI Perception Spec: [System / Archetype]

- Sense type:
  - Sight
  - Hearing
  - Damage
  - Smell
  - Proximity
  - Scripted stimulus
- Range:
- Field of view:
- Occlusion rules:
- Memory duration:
- Target priority:
- Update frequency:
- Debug visualization:
- Data source:
- Tests:
```

### Perception Rules

- Perception ranges and angles must be data-driven.
- Line-of-sight and occlusion behavior must be explicit.
- Perception memory duration must be data-driven.
- Perception update frequency must be budgeted.
- Perception should not query every possible target every frame.
- Use spatial partitioning, broadphase, or event-driven stimuli where appropriate.
- Debug views must show cones/ranges and perceived targets.
- AI must not use hidden information unless design-approved.

---

## Pathfinding and Steering Standards

### Pathfinding Record

```md
## Pathfinding Spec: [AI / System]

- Navigation type:
  - Navmesh
  - Grid
  - A*
  - Flow field
  - Steering-only
  - Scripted path
- Request frequency:
- Repath conditions:
- Dynamic obstacle handling:
- Stuck detection:
- Stuck recovery:
- Path smoothing:
- Debug visualization:
- Budget:
- Tests:
```

### Pathfinding Rules

- Path requests must be throttled.
- Repath conditions must be explicit.
- AI must detect and recover from being stuck.
- Debug visualization must show current path and failed path requests.
- Dynamic obstacles require clear behavior.
- Avoid all agents recalculating paths on the same frame.
- Group movement should avoid crowd collapse and path contention.
- Pathfinding failures must produce fallback behavior, not idle silently forever.

### Stuck Recovery

If AI is stuck:

1. Detect using distance moved over time.
2. Repath once under cooldown.
3. Try local steering adjustment.
4. Escalate to fallback position or regroup.
5. If still stuck, enter safe fallback state.
6. Log debug event with rate limit.

---

## Player Telegraphing and Readability

### Telegraph Record

```md
## AI Telegraph Spec: [Action / State]

- AI action:
- Player-facing meaning:
- Consequence if missed:
- Telegraph type:
  - Animation
  - Audio
  - VFX
  - UI
  - Movement
  - Positioning
  - Dialogue/bark
- Lead time:
- Cancel/commit point:
- Accessibility alternative:
- QA/playtest validation:
```

### Telegraph Rules

- Consequential actions need readable anticipation.
- Attack windup, charge, flee, flank, call-for-help, heal, buff, and special abilities should be telegraphed where relevant.
- Telegraph timing must be long enough for intended reaction.
- Telegraphs must not rely on color alone.
- Audio-only telegraphs need visual/haptic/subtitle support where accessibility requires.
- Animation, audio, VFX, and AI state must align.
- AI should not instantly execute high-consequence actions without warning unless explicitly designed.

---

## Group AI Standards

### Group Behavior Record

```md
## Group AI Spec: [Group / Encounter Type]

- Group type:
- Design source:
- Formation:
- Role types:
- Role assignment rules:
- Flanking rules:
- Spacing rules:
- Target focus rules:
- Retreat/regroup rules:
- Communication events:
- Update frequency:
- Data source:
- Debug visualization:
- Tests:
```

### Role Assignment Table

```md
| Role | Purpose | Max Count | Priority | Assignment Conditions | Release Conditions |
|---|---|---:|---:|---|---|
```

### Group AI Rules

- Formation rules must be data-driven.
- Flanking rules must be data-driven.
- Role assignment must be data-driven.
- Role assignment should avoid starvation.
- Group AI must handle member death, spawn, retreat, and separation.
- Group AI must prevent all agents choosing the same role unless designed.
- Debug visualization should show role, formation slot, target, and group intent.
- Group coordination must not exceed AI budget.

---

## Tactical Fairness and Hidden Information

### Fairness Rules

- AI may know hidden information only if design-approved.
- AI should not react to player actions it could not plausibly perceive.
- AI should not perfectly track hidden player position unless designed.
- Search behavior should use last-known position and memory.
- Difficulty should not come from cheating unless intentionally stylized and disclosed through design.

### Hidden Information Review

```md
## AI Hidden Information Review

- AI behavior:
- Hidden data used:
- Design source:
- Player fairness impact:
- Telegraph/feedback:
- Verdict:
```

---

## Network Safety for AI

### Network Trust Rule

Never trust AI input from the network without validation.

Network-originated AI inputs may include:

- target hints,
- player position,
- noise events,
- damage events,
- command requests,
- squad commands,
- replicated perception,
- replicated AI state.

### Network AI Validation Record

```md
## Network AI Validation

- Input:
- Source:
- Authority:
- Validation:
- Rate limit:
- Replay/spoofing risk:
- Failure behavior:
- Owner:
- Tests:
```

### Network AI Rules

- Server/authoritative simulation owns consequential AI state.
- Clients may send intent or stimuli only if validated.
- AI must not trust client-reported:
  - target visibility,
  - damage,
  - player position,
  - ability state,
  - stealth state,
  - objective state.
- Networked AI behavior requires coordination with Network Programmer and Security Engineer.

---

## Debug Visualization Hooks

### Required Debug Hooks

Where applicable, AI must expose:

- current state,
- behavior-tree active node,
- utility scores,
- target selection,
- path,
- path status,
- stuck status,
- perception cone,
- perceived targets,
- last-known positions,
- group role,
- formation slot,
- current intent,
- cooldown/timer state,
- blackboard values,
- failed transition reason.

### Debug Hook Record

```md
## AI Debug Hook Record

- AI system:
- Debug data exposed:
- Visualization:
- Runtime toggle:
- Release behavior:
- Performance impact:
- Privacy/security notes:
- Owner:
```

### Debug Rules

- Debug visualization must be gated behind debug/developer mode.
- Debug hooks must not materially affect release performance.
- Debug output must be safe and rate-limited.
- Debug visualization should be stable enough for QA and designers.
- Do not expose anti-cheat-sensitive internals to players.

---

## AI Testing Requirements

### Unit Tests

Unit tests are required for:

- utility scoring,
- behavior-tree condition functions,
- state-machine transitions,
- perception filtering,
- target priority,
- role assignment,
- path request throttling logic,
- stuck detection,
- cooldown/timer logic,
- config validation,
- network input validation.

### Integration / Simulation Tests

Integration or simulation tests are required for:

- multi-agent combat scenarios,
- group formation behavior,
- flanking behavior,
- perception with obstacles,
- pathfinding through level geometry,
- telegraph timing,
- AI under high agent counts,
- AI plus animation/audio/VFX feedback,
- network-adapted AI behavior.

### AI Test Evidence Record

```md
## AI Test Evidence

- System:
- Test type:
  - Unit
  - Integration
  - Simulation
  - QA playtest
  - Performance profile
- Test path:
- Scenario:
- Agent count:
- Expected result:
- Actual result:
- Status:
- Missing coverage:
```

### Test Rules

- Logic tests should not require full rendering or UI.
- Use fake clocks, fake perception inputs, and fake path results where useful.
- Tests should cover normal, boundary, invalid, and stress cases.
- Performance-sensitive AI requires profiling, not only functional tests.
- If test infrastructure is missing, mark `TEST_INFRA_MISSING`, not done.

---

## AI Profiling Requirements

### Profiling Record

```md
## AI Profiling Evidence

- System:
- Build:
- Platform:
- Scenario:
- Agent count:
- Profiler/tool:
- Capture duration:
- Median frame cost:
- p95 frame cost:
- p99 frame cost:
- Worst frame:
- Budget:
- Verdict:
- Bottleneck:
- Recommendation:
```

### Profiling Rules

- Profile representative scenes.
- Profile worst-case supported enemy count.
- Profile with debug visualization off for release budget.
- Profile with debug visualization on separately if debug mode matters.
- Identify top AI bottlenecks.
- Before/after evidence is required for optimization claims.

---

## Design Traceability

### AI Design Trace Comment

AI feature entry points should cite design source:

```text
Implements: design/gdd/ai/[system].md#[section-or-feature-id]
Feature ID: [optional]
Decision: DECISIONS.md#D[NNN] if applicable
```

### Design Trace Record

```md
## AI Design Trace

- AI feature/system:
- Code path:
- Design doc:
- Section/feature ID:
- Status:
  - FOUND
  - MISSING
  - STALE
  - CONFLICTING
```

### Traceability Rules

- Do not implement major AI behavior without design source.
- If no source exists, mark `DESIGN_TRACE_MISSING`.
- If implementation deviates from design, flag it.
- If design changed, update config, tests, and AI behavior together.

---

## AI Review Format

Use this for AI code reviews:

```md
## AI Code Review: [System/File]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Scope

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Performance Budget Status

### Data-Driven Status

### Decision Model Status

### Perception Status

### Pathfinding Status

### Group AI Status

### Telegraph / Readability Status

### Debug Hook Status

### Network Validation Status

### Test Evidence

### Required Follow-Up
```

### Severity

```text
AI-S1 — Critical
Can break core gameplay, cause major unfairness, trust network input unsafely, or create severe performance failure.

AI-S2 — High
Violates AI architecture rule: budget overrun, hardcoded AI parameters, missing validation, unreadable consequential AI, missing debug hooks for core AI, invalid state transitions.

AI-S3 — Medium
Partial debug gaps, weak data validation, missing edge-case tests, unclear telegraph timing, pathfinding fallback gaps.

AI-S4 — Low
Documentation, naming, debug polish, minor readability improvement.
```

---

## Self-Learning Protocol

Self-learning means controlled improvement from approved AI reviews, profiling evidence, QA findings, playtest findings, red-team/security findings, state-machine bugs, pathfinding bugs, group-AI issues, and user corrections.

It does not mean autonomous behavior mutation, hidden memory updates, or turning prototype behavior into production policy.

### What May Be Learned

The AI code rule system may learn:

- approved AI architecture patterns,
- approved behavior-tree conventions,
- approved utility-scoring conventions,
- approved AI config key conventions,
- known performance bottlenecks,
- known perception failures,
- known pathfinding failures,
- known stuck-recovery rules,
- known telegraph timing findings,
- known group-role assignment fixes,
- known debug hook requirements,
- network validation findings,
- rejected approaches and reasons.

### What Must Not Be Learned or Stored

Do not store:

- private user data,
- private chain-of-thought,
- secrets,
- credentials,
- sensitive logs,
- anti-cheat internals outside approved security docs,
- raw player data,
- unapproved enemy behavior as design rule,
- prototype if/else behavior as production standard,
- one-off playtest observations as universal rules without review,
- unsupported performance claims.

### Lesson Classification

Use:

```text
Confirmed Rule
Approved AI Code Standard
Behavior Tree Finding
Utility AI Finding
State Machine Finding
Perception Finding
Pathfinding Finding
Stuck Recovery Finding
Performance Finding
Debugging Finding
Telegraph Finding
Group AI Finding
Network Validation Finding
QA Finding
Playtest Finding
Security Finding
Validated Fix
Rejected Approach
Working Assumption
Temporary Context
Superseded
```

### Lesson Storage

Store durable lessons only in approved, reviewable locations such as:

```text
docs/ai/ai-code-standards.md
docs/ai/behavior-tree-lessons.md
docs/ai/utility-ai-lessons.md
docs/ai/perception-lessons.md
docs/ai/group-ai-lessons.md
docs/ai/performance-findings.md
tasks/lessons.md
production/qa/ai/
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
- it applies to AI code,
- it does not include sensitive data,
- it is not overgeneralized,
- it does not conflict with design documents or architecture decisions,
- it has a review trigger where appropriate.

### Lesson Expiry

Review or expire lessons when:

- AI design changes,
- enemy archetype changes,
- behavior-tree architecture changes,
- utility scoring changes,
- navigation system changes,
- performance budget changes,
- engine version changes,
- network authority model changes,
- QA/playtest evidence contradicts the lesson,
- Lead Programmer or Game Designer supersedes it,
- the lesson was temporary,
- the lesson is too broad.

---

## Self-Healing Protocol

Self-healing means detecting an AI-code rule failure, containing the risk, repairing safely, verifying the repair, and reporting what changed.

### Failure Types

Monitor for:

- AI budget overrun,
- hardcoded AI parameter,
- missing AI config,
- invalid AI config,
- missing debug hook,
- behavior-tree condition failure,
- utility scoring instability,
- invalid state transition,
- state transition not logged,
- stuck pathfinding,
- path request spam,
- perception seeing invalid targets,
- perception missing valid targets,
- unreadable telegraph,
- unfair hidden information use,
- group-role starvation,
- formation collapse,
- flanking deadlock,
- network input trusted without validation,
- missing tests,
- missing design trace,
- profiling missing.

### Recovery Loop

When failure occurs:

1. **Stop**
   - Do not mark AI implementation complete.

2. **Identify**
   - State the exact violation.

3. **Classify**
   - Performance, data/config, decision model, perception, pathfinding, state machine, telegraph, group AI, debug, network, test, or traceability.

4. **Contain**
   - Mark status as `BLOCKED`, `NEEDS_FIX`, `BUDGET_FAIL`, `DATA_DRIVEN_UNVERIFIED`, `DEBUG_HOOKS_MISSING`, or `PROFILE_MISSING`.

5. **Recover**
   - externalize data,
   - add config validation,
   - add debug hook,
   - reduce update frequency,
   - add AI LOD,
   - add transition table/log,
   - add stuck recovery,
   - fix perception filtering,
   - add telegraph,
   - add role-assignment guard,
   - add network validation,
   - add tests,
   - add profiling plan.

6. **Verify**
   - Re-run or request tests/profiling.
   - Re-check config, design trace, and debug output.

7. **Report**
   - Summarize issue, fix, remaining risk, and required owner review.

8. **Learn**
   - Propose durable lesson only if validated and approved.

---

## Error Recovery

### Budget Overrun

If AI exceeds 2ms/frame:

- identify bottleneck,
- profile by subsystem,
- reduce update frequency,
- stagger expensive work,
- add AI LOD,
- cache or batch queries,
- simplify scoring/tree evaluation,
- profile again.

### Hardcoded AI Parameter

If an AI parameter is hardcoded:

- classify as tunable or intrinsic,
- externalize if tunable,
- add config key,
- define safe range,
- add config validation test.

### Missing Debug Hook

If core AI cannot be inspected:

- add state/debug data exposure,
- add path/perception/decision visualization where relevant,
- gate debug output,
- ensure debug cost is controlled.

### Invalid State Transition

If AI transitions illegally:

- route through transition table,
- add guard,
- log failed transition,
- add transition test.

### Stuck Pathfinding

If AI gets stuck:

- detect stuck condition,
- throttle repath,
- attempt local steering,
- fallback to safe state,
- log rate-limited stuck event,
- test stuck scenario.

### Perception Failure

If AI sees invalid targets or misses valid targets:

- validate line-of-sight/occlusion rules,
- validate range/FOV data,
- verify spatial query,
- add debug visualization,
- add tests for occluded/visible/edge targets.

### Unreadable Telegraph

If player cannot read AI intent:

- add anticipation state,
- coordinate animation/audio/VFX,
- define lead time,
- add accessibility alternative,
- validate through QA/playtest.

### Group AI Failure

If group roles collapse or starve:

- add role caps,
- add assignment/release rules,
- add fallback roles,
- visualize roles,
- test member death/spawn/separation cases.

### Network Validation Failure

If network-originated AI input is trusted:

- identify authority,
- validate source and payload,
- rate-limit input,
- reject invalid state,
- coordinate with Network Programmer and Security Engineer.

### Missing Test Evidence

If AI logic lacks tests:

- classify logic type,
- add unit test for decision/scoring/transition/config,
- add integration/simulation test for runtime behavior,
- mark blocked until evidence exists.

---

## Memory Policy

### Short-Term Task Memory

Track during current task:

- AI system,
- design source,
- decision model,
- config keys,
- performance budget,
- perception rules,
- pathfinding rules,
- group behavior rules,
- telegraph requirements,
- debug hooks,
- network validation,
- tests,
- profiling evidence,
- open questions,
- approvals needed.

Short-term memory expires after task completion unless explicitly stored.

### Project Memory

Project memory may store:

- approved AI code standards,
- AI config conventions,
- behavior-tree patterns,
- utility scoring patterns,
- state-machine conventions,
- perception findings,
- pathfinding findings,
- performance findings,
- debug hook requirements,
- telegraph lessons,
- group AI lessons,
- network validation findings,
- validated fixes,
- rejected approaches.

### Never Store

Never store:

- secrets,
- credentials,
- private user/player data,
- private chain-of-thought,
- sensitive logs,
- anti-cheat internals outside approved security docs,
- unapproved design behavior as rule,
- temporary prototype behavior as production standard,
- unsupported profiling claims.

---

## Feedback Policy

When the user, Lead Programmer, Game Designer, Level Designer, Systems Designer, Network Programmer, Security Engineer, Performance Analyst, QA Lead, or AI Programmer corrects AI behavior:

1. Accept the correction.
2. Identify whether it affects:
   - AI architecture,
   - data/config,
   - behavior tree,
   - utility scoring,
   - state machine,
   - perception,
   - pathfinding,
   - group AI,
   - telegraphing,
   - debugging,
   - performance,
   - network validation,
   - tests.
3. Revise current output.
4. Ask whether the correction should become durable AI-code guidance if reusable.
5. Store only if approved and evidence-backed.

---

## Tool-Use Policy

This rules file does not grant tools by itself. Agents applying it must follow their own tool permissions.

General guidance:

- Use file-reading tools to inspect AI code, design docs, config files, tests, profiling reports, and QA evidence.
- Use search tools to find hardcoded AI values, behavior-tree keys, utility weights, state transitions, debug hooks, network input paths, and pathfinding calls.
- Use write/edit tools only after approval under the active agent’s workflow.
- Use Bash only if the active agent allows it and only under that agent’s safety policy.
- Do not run tests, profilers, simulations, builds, or file mutations without required approval.
- Do not use Bash to bypass Write/Edit approval.

---

## Safety Guardrails

Never allow production AI code under `src/ai/**` to:

- exceed AI budget without visible failure status,
- hardcode tunable AI parameters,
- omit required debug hooks,
- hide AI state transitions,
- use opaque nested if/else decision chains for complex behavior,
- skip telegraphs for consequential actions,
- use hidden information unfairly,
- trust network-originated AI input,
- omit config validation,
- omit tests for deterministic AI logic,
- omit profiling for budget claims,
- claim done without test or file:line evidence.

---

## Output Standards

AI-code reviews should be:

- performance-aware,
- data-driven,
- design-trace-aware,
- decision-model-aware,
- player-readability-aware,
- debug-friendly,
- network-safe,
- test-evidence-backed,
- clear about uncertainty,
- specific about required follow-up.

### Review Output Format

```md
## AI Code Review: [System/File]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Design Trace

### Performance Budget

### Data/Config Status

### Decision Model

### Perception

### Pathfinding

### Group Behavior

### Telegraphing

### Debugging

### Network Safety

### Tests / Profiling

### Required Follow-Up
```

---

## Reflection Checklist

After reviewing or drafting AI code, privately check:

- Did I verify the design source?
- Did I identify all tunable AI parameters?
- Did I confirm AI parameters come from config/data?
- Did I check the 2ms/frame AI budget?
- Did I require profiling evidence?
- Did I check decision-model structure?
- Did I check debug visualization hooks?
- Did I check perception rules?
- Did I check pathfinding recovery?
- Did I check player telegraphing?
- Did I check group role/formation/flanking data?
- Did I check state transition logs?
- Did I check network input validation?
- Did I require tests?
- Did I state uncertainty honestly?

Do not expose private chain-of-thought. Report only conclusions, evidence, and recommendations.

---

## Evaluation Checklist

Before final approval of AI code:

### Performance

- [ ] AI update budget considered.
- [ ] Representative agent count identified.
- [ ] Profiling evidence exists or missing evidence is stated.
- [ ] AI cost is within budget or marked `BUDGET_FAIL`.
- [ ] Debug visualization cost is separately considered.

### Data and Config

- [ ] Behavior weights are data-driven.
- [ ] Perception ranges are data-driven.
- [ ] Timers/cooldowns are data-driven.
- [ ] Group AI values are data-driven.
- [ ] Config schema and safe ranges exist.
- [ ] Missing/invalid config behavior is defined.

### Decision Logic

- [ ] Behavior tree, utility AI, or state machine is documented.
- [ ] Complex AI is not encoded as opaque if/else chains.
- [ ] State transitions are logged.
- [ ] Invalid transitions are handled.
- [ ] Decision scores or active nodes are debuggable.

### Perception and Pathfinding

- [ ] Perception range/FOV/occlusion rules are explicit.
- [ ] Perception debug visualization exists.
- [ ] Pathfinding requests are throttled.
- [ ] Stuck detection exists.
- [ ] Stuck recovery exists.
- [ ] Path visualization exists.

### Player Readability

- [ ] Consequential actions are telegraphed.
- [ ] Telegraph lead time is defined.
- [ ] Animation/audio/VFX/UX dependencies are identified.
- [ ] Accessibility alternatives are considered.

### Group AI

- [ ] Formation rules are data-driven.
- [ ] Flanking rules are data-driven.
- [ ] Role assignment is data-driven.
- [ ] Role starvation and clustering are handled.
- [ ] Group debug visualization exists where needed.

### Network Safety

- [ ] Network-originated AI input is validated.
- [ ] Client data is not trusted for consequential AI state.
- [ ] Rate limits are considered.
- [ ] Security/network owner review is flagged where relevant.

### Tests and Traceability

- [ ] Design source is cited.
- [ ] Unit tests cover deterministic AI logic.
- [ ] Integration/simulation tests cover runtime behavior where needed.
- [ ] QA/playtest evidence exists for readability where needed.
- [ ] No unsupported “done” claims.

---

## Example Workflows

### Example 1: Hardcoded Perception Range

Issue:

```text
sight_range = 20.0
```

Response:

```md
Finding: Hardcoded tunable AI parameter.

Fix:
- Add config key such as `ai.guard.perception.sight_range`.
- Define safe range.
- Link to AI design doc.
- Add config validation test.
```

---

### Example 2: Budget Overrun

Issue:

```text
AI update costs 5.8ms in 20-enemy combat scenario.
```

Response:

```md
Finding: AI budget failure.

Fix:
- Profile by subsystem.
- Stagger perception updates.
- Throttle path recalculation.
- Add AI LOD for distant agents.
- Profile again before marking improvement validated.
```

---

### Example 3: Missing Debug Hooks

Issue:

```text
AI chooses targets but exposes no debug info.
```

Response:

```md
Finding: AI debuggability failure.

Fix:
- Expose current target, target score, target reason, and perception state.
- Add debug overlay or inspector hook.
- Gate visualization behind debug mode.
```

---

### Example 4: Opaque If/Else Chain

Issue:

```text
AI behavior is a 200-line nested if/else block.
```

Response:

```md
Finding: Decision architecture risk.

Fix:
- Convert to behavior tree, utility scorer, or documented state machine.
- Externalize thresholds and weights.
- Add debug output for active decision path.
```

---

### Example 5: Missing Telegraph

Issue:

```text
Enemy instantly performs high-damage lunge.
```

Response:

```md
Finding: Player-readability failure.

Fix:
- Add anticipation state.
- Define windup duration from data.
- Coordinate animation/audio/VFX telegraph.
- Add QA/playtest validation.
```

---

### Example 6: Pathfinding Stuck

Issue:

```text
AI stands forever against obstacle after path fails.
```

Response:

```md
Finding: Missing stuck recovery.

Fix:
- Add stuck detection.
- Repath under cooldown.
- Try local steering.
- Fall back to regroup/search state.
- Log stuck recovery event.
```

---

### Example 7: Network Input Trusted

Issue:

```text
Client sends AI target override directly.
```

Response:

```md
Finding: Network trust-boundary violation.

Fix:
- Treat client message as request or hint only.
- Server validates visibility, authority, range, and rate.
- Reject invalid input.
- Coordinate with Network Programmer and Security Engineer.
```

---

### Example 8: User Correction

User says:

```text
Enemy reaction delays must be data-driven by difficulty tier.
```

Response pattern:

```text
Understood. I’ll treat enemy reaction delay as a difficulty-tiered AI config value, not a code constant. Store this as a durable AI-code rule only if approved.
```

---

### Example 9: Case Where the Rule Must Not Learn

User says during prototyping:

```text
Just hardcode this enemy’s behavior chain for now.
```

Response pattern:

```text
That can be treated as prototype-only. Production AI under `src/ai/**` still needs data-driven parameters, debuggable decision structure, profiling, and tests.
```

---

## Final AI Code Rule

AI code must be:

- profiled against the 2ms/frame budget,
- data-driven,
- debuggable,
- readable to players,
- structured through behavior trees, utility AI, or explicit state machines,
- safe under group coordination,
- robust under pathfinding/perception failure,
- network-validated,
- testable,
- design-traceable,
- and honest about uncertainty.