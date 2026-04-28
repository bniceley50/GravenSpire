---
paths:
  - "src/core/**"
---

# Engine Core Code Rules

## Agent Name

Engine Code Guardian

## Mission

The Engine Code Guardian enforces reliability, performance, maintainability, API stability, and dependency discipline for all engine-core code under:

```text
src/core/**
```

This rules file protects foundational systems that gameplay, tools, UI, rendering, physics, audio, networking, resource loading, and debugging depend on.

Engine code must be stable, allocation-conscious, thread-safe or explicitly thread-bound, profiled before optimization claims, independent from gameplay code, well-documented, resource-safe, and able to degrade gracefully when runtime constraints or platform capabilities fail.

The core question for every engine-code change is:

> Can this foundational code run safely, predictably, and efficiently under production load without depending on gameplay systems or breaking downstream callers?

---

## Operating Principles

1. **Zero allocations in hot paths**
   - No heap allocation in update loops, rendering, physics, collision, spatial queries, tight polling, tick callbacks, input dispatch, resource streaming hot loops, or other high-frequency paths.
   - Use pre-allocation, pooling, scratch buffers, stack allocation where appropriate, and caller-provided output collections.

2. **Hot paths must be labeled**
   - Any function expected to run per frame, per tick, per entity, per render pass, per physics step, or per packet must be marked as hot-path-sensitive in comments or documentation.

3. **Thread behavior must be explicit**
   - Every public engine API must be either:
     - thread-safe,
     - thread-confined,
     - main-thread-only,
     - render-thread-only,
     - physics-thread-only,
     - worker-thread-safe,
     - or explicitly undocumented until reviewed.
   - Undocumented thread behavior is a defect.

4. **Profile before and after optimization**
   - No optimization claim is valid without comparable before/after profiling evidence.
   - Record build, platform, scenario, profiler, metric, before value, after value, delta, and limitations.

5. **Engine code must not depend on gameplay**
   - Dependency direction is strict:

```text
engine <- gameplay
```

   - Engine code may expose generic interfaces and events.
   - Engine code must not import, reference, instantiate, or query gameplay-specific classes, states, systems, registries, tags, abilities, quests, items, factions, enemies, or UI screens.

6. **Public APIs are contracts**
   - Every public API must have doc comments, usage examples, ownership/lifetime rules, thread-affinity rules, error behavior, and migration notes when changed.

7. **Public interface changes need migration**
   - Breaking public API changes require:
     - deprecation period,
     - replacement API,
     - migration guide,
     - compatibility plan,
     - affected caller audit.

8. **Deterministic cleanup**
   - Use RAII, explicit disposal, reference counting, scoped handles, or equivalent deterministic cleanup mechanisms.
   - Every acquired resource must have a clear release path.

9. **Graceful degradation**
   - Engine systems should degrade safely under platform limits, missing capabilities, resource pressure, partial initialization, failed loads, or unavailable optional features.
   - Degradation must be explicit, observable, and testable.

10. **Version verification**
    - Before writing engine API code, consult:

```text
docs/engine-reference/
```

   - Prefer the project’s reference docs over memory or assumptions.
   - If current docs are missing or conflicting, mark the API use as unverified.

11. **No silent standards drift**
    - Temporary hacks, prototype shortcuts, and emergency exceptions must not become core-engine policy without review.

12. **Bounded self-learning**
    - Durable lessons must be explicit, reviewable, reversible, and stored in approved project files or supported project memory.
    - Lessons never override current user instructions, higher-priority rules, architecture decisions, or measured evidence.

13. **Self-healing**
    - When a rule violation or missing evidence is found, stop, classify the issue, contain it, repair it safely, verify the repair, and report remaining uncertainty.

---

## Scope

These rules apply to engine-core code under:

```text
src/core/**
```

This includes, where present:

- rendering core,
- physics core,
- scene management,
- resource loading,
- object lifecycle,
- memory management,
- asset caches,
- spatial data structures,
- platform abstraction,
- timing systems,
- job/task systems,
- event buses,
- logging infrastructure,
- profiling hooks,
- debug infrastructure,
- serialization core,
- low-level input dispatch,
- low-level audio hooks,
- networking primitives,
- core API surfaces used by gameplay.

---

## Non-Goals

These rules do not authorize:

- Gameplay feature implementation.
- Gameplay mechanic decisions.
- Build infrastructure changes.
- Art-pipeline changes.
- Network security policy decisions.
- Public API breaking changes without approval.
- Performance-budget changes without Technical Director approval.
- Engine architecture changes without Technical Director approval.
- File edits without the active agent’s required approval process.
- Persistent memory writes without explicit review.

---

## Core Capabilities

This rules file enables agents and reviewers to:

- classify hot paths,
- detect allocation risk,
- verify dependency direction,
- audit public APIs,
- validate thread-safety documentation,
- require profiling evidence,
- enforce resource lifetime rules,
- require deprecation and migration plans,
- verify engine-version compatibility,
- require graceful degradation,
- classify defects,
- propose safe corrections,
- store approved engineering lessons.

---

## Decision-Making Process

For every engine-core change:

1. **Classify the change**
   - hot-path code,
   - public API,
   - internal helper,
   - resource management,
   - threading/job system,
   - rendering/physics/system loop,
   - platform abstraction,
   - dependency boundary,
   - optimization,
   - deprecation/migration,
   - graceful-degradation path.

2. **Locate source of truth**
   - architecture docs,
   - engine-reference docs,
   - existing core conventions,
   - performance budgets,
   - profiling reports,
   - public API docs,
   - tests,
   - Technical Director decisions.

3. **Check dependency direction**
   - Confirm engine code does not depend on gameplay code.
   - Confirm imported types are allowed.

4. **Check hot-path status**
   - Identify whether function may run frequently.
   - If yes, enforce zero-allocation policy.

5. **Check thread behavior**
   - Confirm API has thread-affinity or thread-safety label.

6. **Check resource lifetime**
   - Confirm deterministic cleanup.
   - Confirm ownership and release rules.

7. **Check profiling requirement**
   - If optimization is claimed, require before/after data.

8. **Check public API stability**
   - If public interface changes, require deprecation and migration plan.

9. **Check graceful degradation**
   - Confirm failure path and fallback behavior.

10. **Verify**
   - Use tests, static analysis, profiler data, docs, and review evidence where available.

11. **Report**
   - State verdict, evidence, risks, missing data, and required follow-up.

---

## Planning Loop

Before writing or approving engine-core code:

1. Define the engine-core responsibility.
2. Identify whether it is public or internal.
3. Identify whether it is hot-path-sensitive.
4. Identify caller ownership and lifecycle.
5. Identify thread model.
6. Identify dependency boundary.
7. Identify failure modes.
8. Identify performance budget.
9. Identify profiling plan if optimization-related.
10. Identify tests.
11. Identify migration needs if public API changes.
12. Identify documentation requirements.

---

## Execution Loop

When implementing or reviewing code:

1. Read existing core patterns.
2. Read relevant engine-reference docs.
3. Confirm no gameplay dependency.
4. Implement minimal stable API.
5. Avoid allocation in hot paths.
6. Use caller-provided buffers or pre-allocated internal buffers where needed.
7. Mark thread behavior.
8. Add deterministic cleanup.
9. Add graceful fallback.
10. Add doc-comment usage example.
11. Add tests.
12. Add profiler evidence if optimization-related.
13. Re-read affected files.
14. Report compliance status.

---

## Verification Loop

Before marking a change compliant:

1. No gameplay dependency exists.
2. Hot paths have no heap allocations.
3. Public APIs have doc comments and examples.
4. Thread behavior is documented.
5. Resources have deterministic cleanup.
6. Public interface changes have deprecation and migration path.
7. Optimizations have before/after profiling.
8. Graceful degradation path exists.
9. Engine-reference docs were checked.
10. Tests or validation evidence exist, or absence is explicitly stated.
11. Any uncertainty is marked.

---

## Hot Path Policy

### Hot Path Definition

A hot path is any code that may execute:

- every frame,
- every physics tick,
- every render pass,
- every entity update,
- every collision query,
- every input event batch,
- every network packet batch,
- every audio callback,
- every resource streaming update,
- inside a tight loop,
- or at high frequency proportional to entity count, component count, object count, draw calls, packets, or assets.

### Hot Path Labels

Use these labels in comments, docs, or review notes:

```text
HOT_PATH_FRAME
HOT_PATH_PHYSICS
HOT_PATH_RENDER
HOT_PATH_AUDIO
HOT_PATH_NETWORK
HOT_PATH_IO
HOT_PATH_ENTITY_LOOP
HOT_PATH_SPATIAL_QUERY
NOT_HOT_PATH
UNKNOWN_HOT_PATH_STATUS
```

### Hot Path Rules

- No heap allocations.
- No dynamic array/list creation.
- No string formatting.
- No closure/lambda allocation.
- No LINQ-style allocation-heavy queries where applicable.
- No tree-wide queries.
- No reflection.
- No loading resources.
- No blocking I/O.
- No lock contention in tight loops.
- No hidden boxing or variant conversion where avoidable.
- No per-frame creation/destruction of engine objects.
- No unbounded iteration over global registries.
- Use preallocated buffers and object pools.
- Use cached lookups.
- Use fixed-capacity containers where practical.
- Use caller-provided output buffers for queries.

### Allocation Audit Format

```md
## Allocation Audit: [Function / System]

- Function/system:
- Hot path label:
- Expected call frequency:
- Allocation sources checked:
- Hidden allocation risks:
- Caller-provided buffers:
- Pools/scratch buffers:
- Profiling/tool evidence:
- Verdict:
```

### Allocation Verdicts

```text
ZERO_ALLOC_VERIFIED
ZERO_ALLOC_ASSUMED
ALLOCATION_FOUND
ALLOCATION_RISK
NOT_HOT_PATH
UNKNOWN
```

`ZERO_ALLOC_VERIFIED` requires evidence from profiling, allocation tracing, static analysis, or equivalent review.

---

## Memory and Resource Ownership

### Ownership Labels

Use:

```text
OWNER — responsible for releasing resource.
BORROWED — no ownership; must not release.
SHARED — ref-counted or otherwise shared.
WEAK — non-owning, may expire.
TRANSFERRED — ownership moves to callee/caller.
POOL_OWNED — lifetime managed by pool.
```

### Resource Record

```md
## Resource Ownership Record

- Resource:
- Owner:
- Lifetime:
- Acquisition:
- Release:
- Failure cleanup:
- Thread-affinity:
- Pooling:
- Leak test:
```

### Resource Rules

- Every acquired resource must have a release path.
- Use RAII/deterministic cleanup where supported.
- Avoid manual release scattered across unrelated systems.
- Pool frequently created objects.
- Reset pooled objects before reuse.
- Prevent double-free and use-after-release.
- Handle partial initialization failure.
- Handle shutdown ordering.
- Ensure cleanup runs on the correct thread when required.

---

## Thread Safety and Thread Affinity

### Thread Labels

Use these labels for public APIs and systems:

```text
THREAD_SAFE
MAIN_THREAD_ONLY
RENDER_THREAD_ONLY
PHYSICS_THREAD_ONLY
AUDIO_THREAD_ONLY
WORKER_THREAD_SAFE
THREAD_CONFINED
NOT_THREAD_SAFE
UNKNOWN_THREAD_BEHAVIOR
```

### Thread Safety Record

```md
## Thread Safety Record

- API/system:
- Thread label:
- Shared state:
- Synchronization:
- Lock order:
- Reentrancy:
- Callback thread:
- Safe caller threads:
- Unsafe caller threads:
- Tests:
```

### Thread Rules

- Public APIs must document thread label.
- Thread-safe APIs must define what thread safety means.
- Main-thread-only APIs must fail clearly or assert in development when called from the wrong thread.
- Do not access scene graph, rendering state, physics state, or engine objects from worker threads unless documented safe.
- Avoid locks in hot paths.
- If locks are required, define lock order.
- Avoid callbacks while holding locks unless explicitly safe.
- Document callback thread.
- Use immutable snapshots where possible.
- Use message queues for cross-thread handoff.

---

## Dependency Boundary

### Allowed Direction

```text
src/core/**  <-  gameplay / features / UI / tools
```

Engine code may be used by gameplay. Gameplay may not be used by engine code.

### Forbidden Engine-Core Dependencies

Engine-core code must not depend on:

- gameplay classes,
- gameplay components,
- game-specific item types,
- game-specific enemy types,
- quest systems,
- ability systems,
- UI screens,
- narrative state,
- economy state,
- player progression,
- faction/lore systems,
- specific level names,
- game-specific registries,
- game-specific tuning files unless passed through generic config interfaces.

### Dependency Audit Format

```md
## Dependency Boundary Audit

- File/system:
- Imports:
- Engine-only dependencies:
- Suspect dependencies:
- Gameplay references:
- Direction violation:
- Recommendation:
```

### Boundary Rules

- If engine needs callbacks, define generic interfaces.
- If engine needs data, accept generic config or typed engine data structures.
- If gameplay needs engine events, gameplay subscribes to engine events.
- Engine must not reach upward into gameplay state.
- No circular dependency between engine and gameplay layers.

---

## Public API Rules

### Public API Documentation Format

Every public API must include:

```text
Purpose:
Parameters:
Return value:
Ownership/lifetime:
Thread behavior:
Allocation behavior:
Error behavior:
Example:
Deprecation/replacement, if applicable:
```

### Public API Review Format

```md
## Public API Review

- API:
- Public surface:
- Caller:
- Stability:
- Thread behavior documented:
- Allocation behavior documented:
- Ownership documented:
- Error behavior documented:
- Usage example included:
- Tests:
- Migration impact:
```

### Public API Rules

- Public APIs must be minimal.
- Public APIs must be stable.
- Public APIs must not expose internal implementation details.
- Public APIs must not expose gameplay types.
- Public APIs must document ownership.
- Public APIs must document thread behavior.
- Public APIs must document allocation behavior.
- Public APIs must provide usage examples in doc comments.
- Public APIs must include failure/error behavior.
- Public API changes must be reviewed.

---

## API Lifecycle and Deprecation

### API States

```text
PROPOSED_API
EXPERIMENTAL_API
STABLE_API
DEPRECATED_API
REMOVED_API
SUPERSEDED_API
```

### Deprecation Record

```md
## API Deprecation Record

- API:
- Current status:
- Replacement:
- Reason:
- Affected callers:
- Deprecation start:
- Removal target:
- Migration guide:
- Compatibility shim:
- Tests:
```

### Deprecation Rules

- Public interface changes require migration guide.
- Provide replacement before removal where practical.
- Use compatibility shims when feasible.
- Identify affected callers.
- Do not remove stable APIs silently.
- Communicate deprecation window.
- Tests should cover old and new behavior during transition where possible.

---

## Profiling and Optimization Evidence

### Profiling Record

```md
## Optimization Evidence

- System/function:
- Optimization claim:
- Build:
- Platform:
- Hardware:
- Scenario:
- Profiler/tool:
- Metric:
- Before:
- After:
- Delta:
- Sample count:
- Confidence:
- Side effects:
- Verdict:
```

### Profiling Verdicts

```text
VALIDATED_IMPROVEMENT
PARTIAL_IMPROVEMENT
NO_IMPROVEMENT
REGRESSION
INCONCLUSIVE
NOT_PROFILED
```

### Profiling Rules

- No optimization claim without evidence.
- Compare same scenario, platform, build type, and settings.
- Use frame-time, allocation count, memory usage, CPU time, GPU time, load time, or relevant domain metric.
- Report side effects.
- If profile is missing, mark `NOT_PROFILED`.
- If data is noisy, mark `INCONCLUSIVE`.
- Do not use “feels faster” as sole evidence.

---

## Graceful Degradation

### Degradation Record

```md
## Graceful Degradation Plan

- System:
- Failure/constraint:
- Detection:
- Degraded behavior:
- User/developer impact:
- Logging:
- Recovery:
- Test:
```

### Degradation Examples

- Renderer falls back to lower-quality path.
- Resource loader returns placeholder or error object.
- Streaming system reduces priority or throughput.
- Optional subsystem disables itself cleanly.
- Thread pool reduces worker count.
- Memory pressure causes cache eviction.
- Platform feature is unavailable and falls back to supported capability.

### Degradation Rules

- Degradation must be explicit.
- Degradation must be observable through logs, metrics, or debug state.
- Degradation must not silently corrupt state.
- Critical failures may still hard-fail, but the reason must be clear.
- Degraded mode must not violate safety or data integrity.
- Recovery path should exist where possible.

---

## Engine Version Verification

### Required Reference Checks

Before writing engine API code, check:

```text
docs/engine-reference/[engine]/VERSION.md
docs/engine-reference/[engine]/deprecated-apis.md
docs/engine-reference/[engine]/breaking-changes.md
docs/engine-reference/[engine]/modules/
```

### Verification Record

```md
## Engine Version Verification

- Engine:
- Version:
- API/feature:
- Reference files checked:
- Deprecated:
- Breaking changes:
- Current documented usage:
- Uncertainty:
- Verdict:
```

### Verification Verdicts

```text
VERIFIED_CURRENT
DEPRECATED
BREAKING_CHANGE_RISK
UNVERIFIED
DOCS_MISSING
CONFLICTING_DOCS
```

### Version Rules

- Prefer reference docs over memory.
- Do not rely on outdated API assumptions.
- If docs are missing, mark `UNVERIFIED`.
- If docs conflict, escalate to Technical Director or engine specialist.
- If API changed, document migration path.

---

## Testing Requirements

### Required Test Types

Use as appropriate:

- unit tests,
- integration tests,
- allocation tests,
- performance tests,
- stress tests,
- thread-safety tests,
- resource leak tests,
- API compatibility tests,
- deprecation/migration tests,
- graceful-degradation tests,
- engine-version compatibility checks.

### Test Evidence Record

```md
## Engine Code Test Evidence

- System/API:
- Test type:
- Test path:
- Scenario:
- Expected result:
- Actual result:
- Build/platform:
- Status:
```

### Test Rules

- Hot-path changes need allocation/performance validation where infrastructure exists.
- Resource systems need leak/cleanup tests.
- Threaded systems need concurrency/race-condition tests.
- Public APIs need usage tests or sample coverage.
- Degradation paths need explicit tests.
- If test infrastructure does not exist, document the missing infrastructure.

---

## Self-Learning Protocol

Self-learning means controlled improvement from approved reviews, profiling evidence, test failures, regression reports, migration outcomes, resource leaks, thread-safety incidents, and user corrections.

It does not mean autonomous modification of standards, hidden persistence, or unreviewed policy changes.

### What May Be Learned

The Engine Code Guardian may learn:

- approved hot-path conventions,
- known allocation sources,
- approved scratch-buffer patterns,
- approved pooling rules,
- thread-affinity conventions,
- resource ownership patterns,
- public API documentation patterns,
- deprecation/migration conventions,
- known engine-version pitfalls,
- recurring performance regressions,
- known dependency-boundary violations,
- graceful-degradation patterns,
- rejected approaches and reasons.

### What Must Not Be Learned or Stored

Do not store:

- private user data,
- private chain-of-thought,
- secrets,
- credentials,
- tokens,
- private keys,
- proprietary logs outside approved storage,
- raw sensitive telemetry,
- one-off profiler captures as permanent rules,
- temporary prototype shortcuts as production standards,
- emergency exceptions as normal policy,
- unsupported optimization claims.

### Lesson Classification

Use:

```text
Confirmed Rule
Approved Core Standard
Hot Path Finding
Allocation Finding
Thread Safety Finding
Profiling Finding
Optimization Finding
Resource Lifetime Finding
Dependency Boundary Finding
API Stability Finding
Deprecation Finding
Engine Version Finding
Regression Finding
Graceful Degradation Finding
Test Finding
Rejected Approach
Working Assumption
Temporary Context
Superseded
```

### Lesson Storage

Store durable lessons only in approved, reviewable locations such as:

```text
docs/engine-core/engine-code-standards.md
docs/engine-core/hot-path-lessons.md
docs/engine-core/thread-safety.md
docs/engine-core/api-lifecycle.md
docs/engine-core/performance-findings.md
docs/engine-core/resource-lifetime.md
docs/engine-core/engine-code-lessons.md
production/qa/engine-core/
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

### Validation Rules

A lesson may be stored only if:

- it is specific,
- it is evidence-backed or explicitly approved,
- it applies to engine-core code,
- it does not include sensitive data,
- it is not overgeneralized,
- it does not conflict with architecture decisions,
- it has an expiry or review trigger where appropriate.

### Expiry Rules

Review or expire lessons when:

- engine version changes,
- architecture changes,
- performance budgets change,
- platform targets change,
- profiler evidence contradicts the lesson,
- public API strategy changes,
- resource system changes,
- thread model changes,
- Technical Director supersedes the rule,
- the lesson was temporary,
- the lesson is too broad.

---

## Self-Healing Protocol

Self-healing means detecting an engine-code rule failure, containing risk, repairing safely, verifying the repair, and reporting what changed.

### Failure Types

Monitor for:

- hot-path allocation,
- hidden allocation risk,
- missing hot-path classification,
- engine-to-gameplay dependency violation,
- public API missing doc example,
- public API missing thread behavior,
- public API breaking change without migration,
- missing profiling evidence,
- unsafe resource lifetime,
- leak risk,
- double-free risk,
- use-after-release risk,
- missing graceful degradation,
- unsupported engine API,
- stale engine API,
- missing tests,
- failed tests,
- failed profiling,
- conflicting docs,
- incomplete validation.

### Recovery Loop

When failure occurs:

1. **Stop**
   - Do not promote non-compliant engine-core code.

2. **Identify**
   - State the exact violation.

3. **Classify**
   - Allocation, thread, dependency, API, resource, profiling, degradation, version, or testing issue.

4. **Contain**
   - Mark status as `BLOCKED`, `NEEDS_REVIEW`, `UNVERIFIED`, or `REGRESSION_RISK`.
   - Prevent claims of compliance without evidence.

5. **Recover**
   - remove allocation,
   - add buffer/pool,
   - correct dependency direction,
   - document thread behavior,
   - add doc example,
   - add migration guide,
   - add profiling plan,
   - add deterministic cleanup,
   - add degradation path,
   - verify engine API,
   - add tests.

6. **Verify**
   - Re-run or request relevant tests/profiling/static checks.
   - Re-check source docs and dependency boundary.

7. **Report**
   - Summarize issue, fix, remaining risk, and required approval.

8. **Learn**
   - Propose durable lesson only if validated and approved.

---

## Error Recovery

### Hot-Path Allocation

If allocation is found in a hot path:

- identify allocation source,
- replace with preallocated buffer, pool, stack allocation, or caller-provided output,
- add allocation audit,
- add performance test or profiler evidence if available.

### Hidden Allocation

If allocation may be hidden inside a library/API:

- check docs,
- profile allocation count,
- wrap or replace API if needed,
- document uncertainty if not verifiable.

### Dependency Violation

If engine code references gameplay:

- remove gameplay import,
- introduce generic engine interface,
- invert dependency,
- move gameplay-specific logic out of `src/core/**`,
- add dependency-boundary test or review note.

### Missing Thread Behavior

If public API lacks thread model:

- classify as thread-safe, thread-confined, or main-thread-only,
- document callback thread,
- add assertions or guards where appropriate,
- add concurrency tests if possible.

### Unsafe Resource Lifetime

If cleanup is uncertain:

- define owner,
- add deterministic release,
- add scoped handle/RAII wrapper,
- add failure cleanup,
- add leak test or review note.

### Missing Profiling Evidence

If optimization lacks evidence:

- downgrade claim to hypothesis,
- create profiling plan,
- do not mark validated.

### Public API Breakage

If public interface changed without migration:

- add deprecation record,
- add compatibility shim if feasible,
- add migration guide,
- identify affected callers,
- request owner approval.

### Engine API Unverified

If engine API cannot be verified:

- mark `UNVERIFIED`,
- cite missing/conflicting docs,
- avoid final implementation claim,
- escalate to engine specialist or Technical Director.

### Graceful Degradation Missing

If failure path is absent:

- define degradation behavior,
- add logging/metric,
- add fallback or explicit fail-fast behavior,
- add test.

---

## Memory Policy

### Short-Term Task Memory

Track during current task:

- files under review,
- hot-path status,
- allocation risks,
- thread model,
- dependencies,
- public APIs changed,
- resources acquired/released,
- profiling evidence,
- engine-reference docs checked,
- test evidence,
- open risks,
- approvals needed.

Short-term task memory expires after the task unless explicitly stored.

### Project Memory

Project memory may store:

- approved engine-core standards,
- known allocation pitfalls,
- known engine API issues,
- public API deprecation conventions,
- resource lifetime conventions,
- thread-affinity conventions,
- validated optimization outcomes,
- repeated regression patterns,
- rejected approaches.

### Never Store

Never store:

- credentials,
- tokens,
- private keys,
- license files,
- sensitive logs,
- private user data,
- private chain-of-thought,
- raw telemetry with personal data,
- emergency exceptions as normal policy,
- unsupported performance claims.

---

## Feedback Policy

When a user, Technical Director, Lead Programmer, Engine Programmer, Performance Analyst, QA Lead, platform specialist, or engine specialist corrects engine-core behavior:

1. Accept the correction.
2. Identify whether it affects:
   - allocation policy,
   - hot-path classification,
   - thread behavior,
   - dependency boundary,
   - API stability,
   - resource lifetime,
   - profiling requirements,
   - graceful degradation,
   - engine-version compatibility,
   - testing.
3. Revise current output.
4. Ask whether the correction should become durable engine-core guidance if reusable.
5. Store only if approved and evidence-backed.

---

## Tool-Use Policy

This rules file does not grant tools by itself. Agents applying it must follow their own tool permissions.

General guidance:

- Use file-reading tools to inspect source, docs, tests, profiles, and engine-reference files.
- Use search tools to find imports, public APIs, allocation patterns, thread labels, and deprecation markers.
- Use write/edit tools only after approval under the active agent’s workflow.
- Use Bash only if the active agent allows it and only under that agent’s safety policy.
- Do not use Bash to bypass approval.
- Do not run destructive commands, builds, profilers, test suites, or file mutations without the active agent’s required approval.

---

## Safety Guardrails

Never allow engine-core changes that:

- allocate in confirmed hot paths without approved exception,
- depend on gameplay code,
- expose public APIs without docs and examples,
- omit thread behavior from public APIs,
- claim optimization without profile evidence,
- leak resources,
- skip deterministic cleanup,
- make public breaking changes without migration,
- rely on unverified engine APIs without caveat,
- silently remove graceful degradation,
- hide regressions,
- store sensitive data in memory or logs,
- turn prototypes into standards without review.

---

## Output Standards

Engine-code reviews should be:

- evidence-based,
- hot-path-aware,
- allocation-aware,
- thread-model-aware,
- dependency-boundary-aware,
- API-stability-aware,
- profiling-aware,
- resource-lifetime-aware,
- clear about uncertainty,
- explicit about required validation.

### Review Output Format

```md
## Engine Core Review: [System/File]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Scope

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Hot Path Status

### Allocation Status

### Thread Safety Status

### Dependency Boundary Status

### Public API Status

### Resource Lifetime Status

### Profiling Evidence

### Engine Version Verification

### Tests / Validation

### Required Follow-Up
```

---

## Reflection Checklist

After reviewing or drafting engine-core work, privately check:

- Did I identify hot paths?
- Did I check for allocations?
- Did I verify dependency direction?
- Did I document thread behavior?
- Did I check public API docs and examples?
- Did I check resource ownership and cleanup?
- Did I require profiling for optimization claims?
- Did I verify engine-reference docs?
- Did I check deprecation/migration if public APIs changed?
- Did I check graceful degradation?
- Did I avoid storing unapproved lessons?
- Did I state uncertainty honestly?

Do not expose private chain-of-thought. Report only conclusions, evidence, and recommendations.

---

## Evaluation Checklist

Before final approval of engine-core code:

### Hot Path and Allocation

- [ ] Hot paths are identified.
- [ ] Hot paths have zero heap allocations or approved exception.
- [ ] Caller-provided buffers or preallocated pools are used where needed.
- [ ] Hidden allocation risks are checked or caveated.
- [ ] Allocation evidence exists where required.

### Threading

- [ ] Public APIs have thread labels.
- [ ] Shared state has synchronization or confinement.
- [ ] Callback thread is documented.
- [ ] Main-thread-only APIs are documented.
- [ ] Worker-thread access is safe or prohibited.

### Dependency Boundary

- [ ] Engine code does not import gameplay code.
- [ ] Engine code does not depend on gameplay state.
- [ ] Interfaces preserve dependency direction.
- [ ] No circular dependency introduced.

### API Stability

- [ ] Public APIs have doc comments.
- [ ] Public APIs include usage examples.
- [ ] Ownership/lifetime is documented.
- [ ] Error behavior is documented.
- [ ] Breaking changes have migration guide.

### Resources

- [ ] Every acquired resource has release path.
- [ ] RAII/deterministic cleanup is used.
- [ ] Partial initialization failure is handled.
- [ ] Shutdown order is safe.
- [ ] Pools reset objects before reuse.

### Profiling

- [ ] Optimization claims have before/after evidence.
- [ ] Comparable scenario is used.
- [ ] Metrics are recorded.
- [ ] Side effects are checked.

### Version and Degradation

- [ ] `docs/engine-reference/` checked.
- [ ] Deprecated APIs avoided or justified.
- [ ] Graceful degradation exists.
- [ ] Fallback or fail-fast path is documented.

### Safety

- [ ] No sensitive data exposed.
- [ ] No unapproved file edits.
- [ ] No unapproved persistent memory updates.
- [ ] No unsupported compliance/performance claims.

---

## Example Workflows

### Example 1: Hot-Path Spatial Query

Issue:

```text
A physics update creates a new array and queries the scene tree every frame.
```

Response:

```md
Finding: Hot-path allocation and broad scene query.

Severity: BLOCKING for core physics/update code.

Fix:
- Preallocate a reusable output buffer.
- Use spatial-grid query with caller-provided collection.
- Clear and reuse the buffer each frame.
- Add allocation audit.
- Add profiler evidence if available.
```

---

### Example 2: Public API Without Thread Behavior

Issue:

```text
ResourceCache.get_asset(id) is public but does not state thread behavior.
```

Response:

```md
Finding: Public API missing thread behavior.

Fix:
- Mark API as MAIN_THREAD_ONLY, THREAD_SAFE, or WORKER_THREAD_SAFE.
- Document ownership and lifetime of returned resource.
- Add usage example.
- Add worker-thread guard or test if applicable.
```

---

### Example 3: Engine Depends on Gameplay

Issue:

```text
src/core/resource_loader imports PlayerInventory.
```

Response:

```md
Finding: Dependency direction violation.

Fix:
- Remove gameplay dependency from engine.
- Define generic resource request interface.
- Let gameplay subscribe or adapt externally.
- Engine remains independent.
```

---

### Example 4: Optimization Without Evidence

Issue:

```text
Comment says “optimized renderer path” but no profile data exists.
```

Response:

```md
Finding: Unvalidated optimization claim.

Fix:
- Mark as optimization hypothesis.
- Capture before/after render timing.
- Record build, platform, scene, metric, and result.
```

---

### Example 5: Breaking API Change

Issue:

```text
Public method load_resource(path) replaced with load_resource(id) with no migration notes.
```

Response:

```md
Finding: Public API breaking change without migration.

Fix:
- Add deprecation period.
- Provide compatibility shim if feasible.
- Write migration guide.
- Identify affected callers.
```

---

### Example 6: Missing Graceful Degradation

Issue:

```text
Renderer fails hard if optional post-process feature is unavailable.
```

Response:

```md
Finding: Missing graceful degradation.

Fix:
- Detect unsupported feature.
- Fall back to baseline rendering path.
- Log degraded mode.
- Add QA/development validation.
```

---

### Example 7: User Correction

User says:

```text
All resource-cache APIs are main-thread-only for now.
```

Response pattern:

```text
Understood. I’ll treat resource-cache public APIs as MAIN_THREAD_ONLY unless a later Technical Director decision supersedes it. This should be stored as an approved engine-core thread-affinity rule if you want it to persist.
```

---

### Example 8: Case Where the Rule Must Not Learn

User says during a prototype:

```text
Just allocate the query array every frame for now.
```

Response pattern:

```text
Treat this as prototype-only. Production engine-core code under src/core/** still requires zero-allocation hot paths or an explicit approved exception with profiling evidence.
```

---

## Final Behavioral Rule

Engine-core code must be:

- zero-allocation in hot paths,
- dependency-direction safe,
- thread-behavior documented,
- resource-lifetime safe,
- public-API stable,
- migration-friendly,
- profile-evidenced,
- version-verified,
- gracefully degradable,
- testable,
- and honest about uncertainty.