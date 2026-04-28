---
paths:
  - "tests/**"
---

# Test Standards

## Rule Set Name

Test Standards

## Mission

These rules govern all automated and documented tests under:

```text
tests/**
```

Their purpose is to ensure tests are readable, deterministic, isolated, meaningful, maintainable, fast enough for their gate, and capable of catching real regressions.

Tests are production evidence. A passing test should mean something. A failing test should tell the team what broke, where, and why.

The core test question is:

> Does this test clearly prove a specific behavior under controlled conditions, without hidden external state or ambiguous assertions?

---

## Operating Principles

1. **Tests must be named by behavior**
   - Test names must communicate:
     - system,
     - scenario,
     - expected result.

2. **Arrange / Act / Assert is mandatory**
   - Every test must have a clear setup, a single behavior under test, and precise assertions.

3. **Unit tests are isolated**
   - Unit tests must not depend on filesystem, network, database, live services, engine scenes, global state, real time, or uncontrolled randomness.

4. **Integration tests clean up**
   - Integration tests may touch broader systems, but they must restore state after execution.

5. **Performance tests enforce thresholds**
   - Performance tests must define acceptable thresholds and fail when exceeded.
   - A performance test without a threshold is a measurement, not a gate.

6. **Test data is controlled**
   - Test data must be defined in the test or in dedicated immutable fixtures.
   - Shared mutable test data is prohibited.

7. **External dependencies are mocked or simulated**
   - Tests should be fast and deterministic.
   - External systems should be replaced with mocks, fakes, stubs, test doubles, local fixtures, or approved test harnesses.

8. **Every bug fix needs a regression test**
   - The regression test must fail before the fix and pass after the fix.
   - It must reproduce the original failure mode, not merely test adjacent behavior.

9. **Test failures are evidence**
   - Do not dismiss failing tests as noise.
   - Classify, contain, repair, and verify.

10. **Skipped tests are visible debt**
    - Skips, quarantines, and expected failures must have a reason, owner, and review trigger.

11. **Self-healing**
    - When a test is flaky, ambiguous, order-dependent, non-isolated, misnamed, or missing cleanup, stop, diagnose, repair safely, verify, and report.

12. **Bounded self-learning**
    - Durable testing lessons may be stored only in approved reviewable locations.
    - Lessons must be evidence-backed, reversible, and subordinate to current project rules and quality gates.

---

## Scope

These rules apply to:

```text
tests/**
```

This includes, where present:

- unit tests,
- integration tests,
- regression tests,
- smoke tests,
- performance tests,
- acceptance tests,
- data/config validation tests,
- localization tests,
- accessibility test stubs,
- network tests,
- AI simulation tests,
- UI interaction tests,
- test fixtures,
- test helpers,
- test doubles,
- test reports,
- test evidence files.

---

## Non-Goals

These rules do not authorize tests to:

- call production services,
- mutate production data,
- require private credentials,
- depend on external network availability,
- depend on test execution order,
- rely on uncontrolled randomness,
- hide failing tests,
- skip tests indefinitely,
- lower performance thresholds without approval,
- mark stories complete without required evidence,
- edit files without the active agent’s approval workflow,
- store persistent lessons without approval.

---

## Test Lifecycle State Labels

Use these labels when reviewing, writing, or reporting tests:

```text
PROPOSED — test idea exists but is not written.
SCAFFOLDED — test file/function exists but is incomplete.
IMPLEMENTED — test has executable logic.
AAA_VERIFIED — arrange/act/assert structure is clear.
ISOLATED — test has no forbidden external state dependency.
DETERMINISTIC — test result is stable under repeated runs.
FIXTURE_READY — fixture is immutable, scoped, and documented.
CLEANUP_VERIFIED — integration test cleanup is confirmed.
THRESHOLD_DEFINED — performance threshold exists.
PERFORMANCE_VALIDATED — performance test executed and threshold evaluated.
REGRESSION_LINKED — regression test links to bug/failure mode.
PASSING — test passed in stated environment.
FAILING — test failed.
FLAKY — test has inconsistent result.
QUARANTINED — isolated from gate with owner and expiry.
SKIPPED — intentionally not run with reason and review trigger.
OBSOLETE — no longer tests active behavior.
SUPERSEDED — replaced by better test.
BLOCKED — cannot run due to missing dependency, environment, fixture, or tool.
```

### State Rules

- Do not mark `PASSING` without execution evidence.
- Do not mark `ISOLATED` without checking external dependencies.
- Do not mark `DETERMINISTIC` without stable repeatability or deterministic design.
- Do not mark `REGRESSION_LINKED` without bug ID or original failure description.
- Do not mark `PERFORMANCE_VALIDATED` without threshold and measured result.
- `SKIPPED` and `QUARANTINED` require owner and review trigger.

---

## Test Type Taxonomy

Classify every test.

```text
UNIT — tests one logic unit in isolation.
INTEGRATION — tests interaction between multiple systems.
REGRESSION — proves a specific previous bug does not recur.
SMOKE — checks critical path build/playability.
PERFORMANCE — checks speed, memory, frame time, throughput, or latency against threshold.
ACCEPTANCE — validates story or feature acceptance criteria.
DATA_VALIDATION — validates config/data/schema correctness.
UI_INTERACTION — validates screen flow, input, focus, or UI behavior.
NETWORK — validates multiplayer/network behavior.
AI_SIMULATION — validates AI decision/perception/path behavior.
LOCALIZATION — validates locale strings, layout, fallback, or formatting.
ACCESSIBILITY — validates accessibility requirement.
MANUAL_EVIDENCE — documented human-run test or playtest.
```

### Test Type Record

```md
## Test Type Record

- Test:
- Type:
- System:
- Story / bug / feature:
- Gate level:
  - BLOCKING
  - ADVISORY
  - INFO
- Evidence required:
- Owner:
```

---

## Test Naming Standard

### Default Function Name Pattern

```text
test_[system]_[scenario]_[expected_result]
```

Examples:

```text
test_health_system_take_damage_reduces_health
test_inventory_add_item_when_slot_available_increases_stack
test_save_loader_invalid_checksum_rejects_save
test_ai_guard_loses_los_enters_search_state
```

### File Naming Pattern

Use a clear system-oriented pattern:

```text
[system]_[test_type]_test.[ext]
```

Examples:

```text
health_unit_test.gd
inventory_integration_test.cs
save_regression_test.cpp
ai_perception_performance_test.gd
```

If project-specific engine conventions differ, use the approved project convention, but keep names descriptive.

### Regression Test Naming

```text
test_regression_[bug_id]_[scenario]_[expected_result]
```

Example:

```text
test_regression_bug_142_inventory_remove_last_item_clears_slot
```

### Performance Test Naming

```text
test_perf_[system]_[scenario]_[threshold_result]
```

Example:

```text
test_perf_ai_50_agents_update_under_2ms
```

### Naming Rules

- Avoid vague names:
  - `test1`
  - `test_damage`
  - `test_system`
  - `test_bugfix`
- Include expected result.
- Use behavior language, not implementation details, unless implementation is the behavior under test.
- Parameterized tests must include case labels.
- Regression tests must include bug ID or failure ID where available.

---

## Arrange / Act / Assert Standard

Every test must clearly separate:

```text
Arrange — set up subject, dependencies, fixtures, and expected values.
Act — perform exactly the behavior under test.
Assert — verify precise, meaningful outcomes.
```

### Required Structure

```text
# Arrange
[setup]

# Act
[one behavior]

# Assert
[precise assertions]
```

### AAA Rules

- Arrange should not contain assertions except setup validation when necessary.
- Act should be as small as possible.
- Assert should verify exact behavior.
- Avoid multiple unrelated acts in one test.
- Avoid vague assertions.
- Avoid testing multiple behaviors unless the test is explicitly integration/acceptance.
- Use failure messages when assertion meaning is not obvious.

### Assertion Quality

Bad:

```text
assert_true(health.current_health < 100)
```

Better:

```text
assert_eq(health.current_health, 75)
```

Bad:

```text
assert_not_null(result)
```

Better:

```text
assert_eq(result.status, "rejected")
assert_eq(result.reason, "invalid_checksum")
```

---

## Unit Test Isolation

### Unit Tests Must Not Depend On

- filesystem,
- network,
- database,
- live services,
- real clock time,
- uncontrolled randomness,
- test execution order,
- global mutable state,
- static caches,
- real engine scenes,
- real player profiles,
- environment-specific paths,
- external assets unless provided as immutable test fixtures.

### Unit Test Isolation Record

```md
## Unit Test Isolation Review

- Test:
- Filesystem dependency:
- Network dependency:
- Database dependency:
- Real time dependency:
- Randomness dependency:
- Global state dependency:
- External service dependency:
- Fixture dependency:
- Verdict:
```

### Isolation Rules

- Inject clocks instead of using real time.
- Inject RNG or use fixed seed.
- Use in-memory fakes instead of filesystem.
- Use mock/stub services instead of network.
- Reset static state after each test if unavoidable.
- Do not share mutable fixture objects between tests.
- Do not assume prior tests ran.

---

## Integration Test Cleanup

Integration tests may touch real subsystems, but they must clean up.

### Cleanup Requirements

Clean up:

- created files,
- temporary directories,
- database records,
- mock server state,
- network sessions,
- engine scene objects,
- spawned entities,
- subscriptions/event listeners,
- static/global state,
- caches,
- timers,
- coroutines/tasks,
- temporary config,
- generated artifacts.

### Cleanup Record

```md
## Integration Test Cleanup Record

- Test:
- Resources created:
- Cleanup method:
- Cleanup verified:
- Failure cleanup:
- Residual state risk:
```

### Cleanup Rules

- Cleanup should run even if assertions fail.
- Use test framework setup/teardown hooks where available.
- Prefer unique test IDs/namespaces for created resources.
- Do not leave test data in production-like locations.
- Integration tests must be order-independent after cleanup.
- Cleanup failure is a test failure or blocker.

---

## Test Data and Fixture Policy

### Allowed Test Data Sources

- inline test data,
- immutable dedicated fixtures,
- generated per-test data,
- builder/factory helpers,
- approved golden files,
- local mock responses.

### Prohibited

- shared mutable fixtures,
- production player data,
- production save data unless sanitized and approved,
- live database rows,
- external service state,
- hidden test-order dependencies.

### Fixture Record

```md
## Test Fixture Record

- Fixture:
- Used by:
- Mutable:
- Scope:
  - Per test
  - Per file
  - Per suite
- Reset behavior:
- Owner:
- Notes:
```

### Fixture Rules

- Fixture data must be dedicated to tests.
- Mutable fixtures must be copied per test.
- Golden files need clear update procedure.
- Randomized fixtures must use seed and print seed on failure.
- Fixtures must not encode production balance unless explicitly intended.

---

## Mock / Fake / Stub Policy

### Definitions

```text
Mock — verifies interaction expectations.
Fake — lightweight working implementation.
Stub — returns predefined responses.
Spy — records calls for assertion.
Dummy — placeholder object not used by test.
```

### Test Double Record

```md
## Test Double Record

- Dependency:
- Double type:
- Behavior:
- Assertions:
- Reset:
- Reason:
```

### Mocking Rules

- Mock external dependencies.
- Prefer fakes for stateful domain behavior when clearer.
- Avoid over-mocking implementation details.
- Mock boundaries, not the system under test.
- Do not mock what the test is supposed to verify.
- Test doubles must be deterministic.
- Mocks must be reset between tests.

---

## Determinism and Flake Policy

### Determinism Requirements

Tests should produce the same result when run:

- alone,
- with the full suite,
- repeatedly,
- in different order,
- on supported CI runners,
- without network access.

### Flaky Test Record

```md
## Flaky Test Record

- Test:
- Failure pattern:
- Frequency:
- Suspected cause:
- Impacted gate:
- Owner:
- Mitigation:
- Review trigger:
```

### Flaky Test Rules

- Flaky tests are defects.
- Do not ignore flaky tests.
- Do not silently loosen assertions.
- Do not quarantine without owner and expiry.
- Fix root cause:
  - time dependency,
  - order dependency,
  - shared state,
  - resource leak,
  - race condition,
  - external dependency,
  - insufficient waiting condition,
  - noisy performance threshold.

### Quarantine Rules

A test may be quarantined only if:

- it blocks unrelated work,
- the failure is documented,
- owner is assigned,
- review date is set,
- gate impact is documented,
- replacement coverage is considered.

---

## Regression Test Policy

Every bug fix must include a regression test.

### Regression Test Requirements

A regression test must:

- reference the bug ID or failure description,
- reproduce the original failure mode,
- fail before the fix,
- pass after the fix,
- assert the exact corrected behavior,
- cover the minimal scenario that caused the bug,
- include related edge cases where cheap.

### Regression Test Record

```md
## Regression Test Record

- Bug ID:
- Original failure:
- Root cause:
- Test path:
- Test name:
- Fails before fix:
  - Yes / No / Unknown
- Passes after fix:
  - Yes / No / Unknown
- Edge cases covered:
- Owner:
```

### Regression Rules

- No bug fix is complete without a regression test unless explicitly waived.
- Regression tests must not only test the happy path.
- A bug fix that cannot be tested must explain why and provide alternate evidence.
- Regression tests should remain after the bug is fixed.

---

## Performance Test Standards

### Performance Test Record

```md
## Performance Test: [System / Scenario]

- Test name:
- System:
- Scenario:
- Metric:
- Threshold:
- Unit:
- Platform:
- Hardware:
- Build configuration:
- Warm-up:
- Sample count:
- Measurement method:
- Pass condition:
- Fail condition:
- Variance handling:
- Evidence:
```

### Performance Metrics

Use where relevant:

- frame time,
- CPU time,
- GPU time,
- memory usage,
- allocation count,
- GC allocation,
- load time,
- network bandwidth,
- latency,
- throughput,
- query time,
- entity count,
- operation count per second.

### Performance Rules

- Threshold must be explicit.
- Test must fail if threshold is exceeded.
- Include warm-up when needed.
- Account for noisy measurements.
- Use representative build/platform.
- Do not compare debug and release results without caveat.
- Avoid overly tight thresholds on noisy environments.
- Track median and p95/p99 when possible.
- If performance is measured but not gated, label it as benchmark, not test.

---

## Smoke Test Standards

Smoke tests verify critical build readiness.

### Smoke Test Record

```md
## Smoke Test: [Scenario]

- Scenario:
- Preconditions:
- Steps:
- Expected result:
- Pass criteria:
- Owner:
- Gate:
```

### Smoke Test Rules

- Smoke tests should cover critical path only.
- Smoke tests should be fast.
- Smoke test failure blocks QA handoff or build promotion.
- Smoke tests should not become full regression suites.
- Smoke tests must be updated when critical path changes.

---

## Acceptance Test Standards

Acceptance tests validate story or feature completion.

### Acceptance Test Record

```md
## Acceptance Test: [Feature / Story]

- Story / feature:
- Acceptance criterion:
- Test type:
- Preconditions:
- Steps:
- Expected result:
- Pass criteria:
- Evidence:
```

### Acceptance Rules

- Acceptance criteria must be testable.
- Subjective criteria require measurable proxy or review method.
- A feature is not complete if required acceptance tests are missing or failing.
- Acceptance tests should map one-to-one with important criteria where practical.

---

## Test Evidence

### Test Evidence Record

```md
## Test Evidence

- Test path:
- Test name:
- Test type:
- Command:
- Run location:
  - Local
  - CI
  - Manual
- Result:
  - PASS
  - FAIL
  - SKIPPED
  - BLOCKED
  - NOT_RUN
- Date/session:
- Build/commit:
- Environment:
- Notes:
```

### Evidence Rules

- Do not claim tests passed without evidence.
- `NOT_RUN` is not pass.
- Local pass is not CI pass.
- Manual evidence must include steps and result.
- Performance evidence must include threshold and measurement.
- Failed tests should include failure summary.

---

## CI and Gate Behavior

### Gate Levels

```text
BLOCKING — failure prevents merge, handoff, or release.
ADVISORY — failure warns and requires owner review.
INFO — diagnostic only.
```

### Gate Record

```md
## Test Gate

- Gate:
- Test suite:
- Trigger:
- Required tests:
- Pass criteria:
- Failure behavior:
- Waiver allowed:
- Waiver approver:
- Evidence path:
```

### Gate Rules

- Required tests must not be skipped for speed.
- Broken required test infrastructure is `BLOCKED`, not pass.
- Gate waivers require reason, owner, and expiry.
- If test suite cannot run, report `BLOCKED`.
- Do not downgrade a blocking gate without approval.

---

## Coverage Policy

### Coverage Record

```md
## Coverage Review

- System:
- Required behaviors:
- Covered behaviors:
- Missing coverage:
- Risk:
- Recommendation:
```

### Coverage Rules

- Coverage percentage alone is not sufficient.
- Critical behavior coverage matters more than raw line coverage.
- Logic, formulas, state machines, bug fixes, and edge cases need targeted tests.
- Untestable code is a design problem.
- Missing coverage must be visible.

---

## Test Review Format

Use this format for reviewing tests:

```md
## Test Review: [Test File / Suite]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Naming Status

### Arrange / Act / Assert Status

### Isolation Status

### Fixture Status

### Mocking Status

### Cleanup Status

### Determinism Status

### Regression Coverage

### Performance Thresholds

### Evidence Status

### Required Follow-Up
```

### Severity

```text
TEST-S1 — Critical
Test suite gives false confidence, required gate cannot run, cleanup corrupts state, or regression/production-blocking behavior is untested.

TEST-S2 — High
Missing regression test for bug fix, unit test depends on external state, integration test lacks cleanup, performance test lacks threshold, or flaky blocking test.

TEST-S3 — Medium
Weak naming, ambiguous assertion, shared mutable fixture, missing edge-case test, insufficient evidence.

TEST-S4 — Low
Formatting, minor documentation, non-blocking cleanup improvement.
```

---

## Self-Learning Protocol

Self-learning means controlled improvement from failed tests, flaky-test investigations, regression misses, CI failures, QA findings, and user corrections.

It does not mean hidden memory updates, automatic gate relaxation, or turning one-off failures into global rules.

### What May Be Learned

The test-standard system may learn:

- approved test naming conventions,
- recurring isolation failures,
- fixture patterns,
- mocking patterns,
- cleanup failure modes,
- flaky-test causes,
- regression-test gaps,
- performance threshold patterns,
- CI gate findings,
- coverage gaps,
- rejected test approaches and why.

### What Must Not Be Learned or Stored

Do not store:

- private user data,
- private chain-of-thought,
- secrets,
- credentials,
- private logs,
- production player data,
- one-off local failures as global rules,
- flaky results as product truth,
- temporary skips as permanent policy,
- unsupported test-pass claims.

### Lesson Classification

Use:

```text
Confirmed Rule
Approved Test Standard
Naming Finding
AAA Finding
Isolation Finding
Fixture Finding
Mocking Finding
Cleanup Finding
Determinism Finding
Flaky Test Finding
Regression Finding
Performance Threshold Finding
CI Gate Finding
Coverage Finding
QA Finding
Validated Fix
Rejected Approach
Working Assumption
Temporary Context
Superseded
```

### Lesson Storage

Store durable lessons only in approved, reviewable locations such as:

```text
docs/testing/test-standards.md
docs/testing/fixture-standards.md
docs/testing/regression-lessons.md
docs/testing/flaky-test-findings.md
docs/testing/performance-test-standards.md
tasks/lessons.md
production/qa/testing/
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
- it applies to tests or quality gates,
- it does not include sensitive data,
- it is not overgeneralized,
- it does not conflict with QA policy or gate requirements,
- it has a review trigger where appropriate.

### Lesson Expiry

Review or expire lessons when:

- test framework changes,
- engine version changes,
- CI environment changes,
- QA gate policy changes,
- test directory structure changes,
- performance budgets change,
- flaky-test evidence contradicts the lesson,
- QA Lead supersedes it,
- the lesson was temporary,
- the lesson is too broad.

---

## Self-Healing Protocol

Self-healing means detecting a test-standard failure, containing risk, repairing safely, verifying the repair, and reporting what changed.

### Failure Types

Monitor for:

- vague test name,
- missing arrange/act/assert structure,
- imprecise assertion,
- unit test uses filesystem,
- unit test uses network,
- unit test uses database,
- unit test depends on real time,
- unit test uses uncontrolled randomness,
- test depends on execution order,
- shared mutable fixture,
- integration test lacks cleanup,
- performance test lacks threshold,
- performance threshold too vague,
- test is flaky,
- test is skipped without owner,
- bug fix lacks regression test,
- regression test does not reproduce original bug,
- test pass claimed without evidence.

### Recovery Loop

When failure occurs:

1. **Stop**
   - Do not treat test coverage as valid.

2. **Identify**
   - State the exact test-standard failure.

3. **Classify**
   - Naming, structure, isolation, fixture, cleanup, determinism, regression, performance, evidence, or gate issue.

4. **Contain**
   - Mark status:
     - `NEEDS_FIX`,
     - `BLOCKED`,
     - `FLAKY`,
     - `SKIPPED`,
     - `REGRESSION_MISSING`,
     - `EVIDENCE_MISSING`.

5. **Recover**
   - rename test,
   - restructure into AAA,
   - replace external dependency with fake/mock,
   - inject clock/RNG,
   - isolate fixture,
   - add cleanup,
   - add threshold,
   - add regression test,
   - remove or justify skip,
   - rerun or request evidence.

6. **Verify**
   - Re-run test or confirm structure.
   - Confirm deterministic design.
   - Confirm evidence record.

7. **Report**
   - Summarize issue, fix, remaining risk, and owner.

8. **Learn**
   - Propose durable lesson only if validated and approved.

---

## Error Recovery

### Vague Test Name

If test name is vague:

- rename to `test_[system]_[scenario]_[expected_result]`,
- include bug ID for regression tests,
- include threshold in performance tests where practical.

### Missing AAA

If structure is unclear:

- split setup into Arrange,
- isolate the behavior into Act,
- replace broad assertions with precise Assert,
- split test if multiple unrelated behaviors are tested.

### Unit Test Uses External State

If unit test uses filesystem/network/database:

- replace with fake, mock, stub, or in-memory equivalent,
- move to integration test if external behavior is actually under test,
- isolate external test resources.

### Real Time Dependency

If test depends on real clock:

- inject fake clock,
- simulate time progression,
- avoid sleeps where possible,
- use event-driven waits with timeout for integration tests.

### Randomness Dependency

If test uses randomness:

- inject seeded RNG,
- record seed on failure,
- test deterministic outcomes where possible,
- use property-style tests only with controlled seeds and reproducible failure cases.

### Shared Mutable Fixture

If fixture is shared and mutable:

- clone per test,
- build fresh fixture in Arrange,
- reset in teardown,
- make shared fixture immutable.

### Integration Cleanup Missing

If integration test leaves state:

- add teardown,
- use unique temporary resources,
- delete created files/records/entities,
- verify cleanup even on failure.

### Performance Threshold Missing

If performance test has no threshold:

- define metric,
- define threshold,
- define environment,
- define sample count,
- make test fail when exceeded,
- or relabel as benchmark.

### Flaky Test

If test is flaky:

- identify source,
- fix determinism,
- quarantine only with owner and expiry,
- do not loosen assertion without understanding failure.

### Missing Regression Test

If bug fix lacks regression test:

- recreate original failure scenario,
- write test that fails before fix,
- link bug ID,
- assert corrected behavior.

### Missing Evidence

If pass is claimed without evidence:

- add command, run location, result, environment, and date/session,
- downgrade claim to unverified if evidence is unavailable.

---

## Memory Policy

### Short-Term Task Memory

Track during current test task:

- test file,
- test type,
- system,
- scenario,
- expected result,
- dependencies,
- fixtures,
- mocks/fakes/stubs,
- cleanup obligations,
- thresholds,
- bug IDs,
- execution evidence,
- open risks.

Short-term memory expires after task completion unless explicitly stored.

### Project Memory

Project memory may store:

- approved naming conventions,
- fixture standards,
- mocking standards,
- known flaky-test causes,
- performance threshold conventions,
- regression-test findings,
- gate evidence requirements,
- validated fixes,
- rejected approaches.

### Never Store

Never store:

- secrets,
- credentials,
- private user/player data,
- private chain-of-thought,
- sensitive logs,
- production data,
- unsupported claims that tests passed,
- temporary skips as permanent policy.

---

## Feedback Policy

When the user, QA Lead, QA Tester, Lead Programmer, Technical Director, DevOps Engineer, or domain owner corrects testing behavior:

1. Accept the correction.
2. Identify whether it affects:
   - naming,
   - AAA structure,
   - isolation,
   - fixture policy,
   - mocks/fakes/stubs,
   - integration cleanup,
   - regression requirements,
   - performance thresholds,
   - gate behavior,
   - evidence policy.
3. Revise current output.
4. Ask whether the correction should become durable test guidance if reusable.
5. Store only if approved and evidence-backed.

---

## Tool-Use Policy

This rules file does not grant tools by itself. Agents applying it must follow their own tool permissions.

General guidance:

- Use file-reading tools to inspect tests, fixtures, helpers, reports, and evidence.
- Use search tools to find test names, skips, flaky markers, fixtures, external dependency usage, and regression links.
- Use write/edit tools only after approval under the active agent’s workflow.
- Use Bash only if the active agent allows it and only under that agent’s safety policy.
- Do not run test suites, performance benchmarks, builds, or file mutations without required approval.
- Do not use Bash to bypass write/edit approval.

---

## Safety Guardrails

Never allow tests under `tests/**` to:

- use vague names,
- omit arrange/act/assert structure,
- rely on external state in unit tests,
- depend on test order,
- use shared mutable fixtures,
- leave integration-test residue,
- use real network/database/filesystem in unit tests,
- claim performance success without threshold,
- keep skipped tests without reason/owner/review trigger,
- merge bug fixes without regression tests,
- claim tests passed without evidence,
- hide failing or flaky tests.

---

## Output Standards

Test reviews and test-writing recommendations should be:

- behavior-specific,
- deterministic,
- fixture-aware,
- isolation-aware,
- cleanup-aware,
- regression-aware,
- threshold-aware,
- evidence-backed,
- explicit about uncertainty.

### Review Output Format

```md
## Test Review: [Test File / Suite]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Naming

### Arrange / Act / Assert

### Isolation

### Fixtures / Test Data

### Mocks / External Dependencies

### Cleanup

### Determinism / Flakiness

### Regression Coverage

### Performance Thresholds

### Evidence

### Required Follow-Up
```

---

## Reflection Checklist

After reviewing or drafting tests, privately check:

- Does the test name describe system, scenario, and expected result?
- Is Arrange / Act / Assert clear?
- Is the assertion precise?
- Is the test type correctly classified?
- Is the test isolated if unit-level?
- Are external dependencies mocked or faked?
- Is test data local or immutable?
- Is cleanup guaranteed for integration tests?
- Is randomness/time controlled?
- Does performance test have a threshold?
- Does bug fix have a regression test?
- Is test evidence available?
- Did I avoid treating skipped/flaky tests as acceptable?

Do not expose private chain-of-thought. Report only findings, evidence, and recommendations.

---

## Evaluation Checklist

Before final approval of test code:

### Naming

- [ ] Test function follows `test_[system]_[scenario]_[expected_result]` or approved equivalent.
- [ ] Test file name identifies system and test type.
- [ ] Regression test includes bug/failure reference where available.
- [ ] Performance test name identifies measured threshold or scenario.

### Structure

- [ ] Arrange section is clear.
- [ ] Act section tests one behavior.
- [ ] Assert section is precise.
- [ ] Test is not testing multiple unrelated behaviors.
- [ ] Failure message is clear where needed.

### Isolation and Determinism

- [ ] Unit test has no filesystem dependency.
- [ ] Unit test has no network dependency.
- [ ] Unit test has no database dependency.
- [ ] Time is controlled.
- [ ] Randomness is controlled.
- [ ] Global/static state is reset or avoided.
- [ ] Test does not depend on order.

### Fixtures and Dependencies

- [ ] Test data is local or in dedicated fixture.
- [ ] Shared mutable fixtures are avoided.
- [ ] External dependencies are mocked/faked/stubbed.
- [ ] Mocks are reset between tests.

### Integration Cleanup

- [ ] Created files are removed.
- [ ] Spawned entities/objects are cleaned.
- [ ] Event subscriptions are removed.
- [ ] Temporary database/network/mock-server state is cleared.
- [ ] Cleanup runs on failure.

### Regression and Performance

- [ ] Bug fixes include regression tests.
- [ ] Regression test would have caught original bug.
- [ ] Performance tests define thresholds.
- [ ] Performance tests fail when thresholds are exceeded.
- [ ] Environment and sample count are documented.

### Evidence

- [ ] Test command is recorded.
- [ ] Result is recorded.
- [ ] Run location is recorded.
- [ ] Build/commit/environment is recorded where relevant.
- [ ] `NOT_RUN` is not treated as pass.

---

## Example Workflows

### Example 1: Correct Unit Test

```gdscript
func test_health_system_take_damage_reduces_health() -> void:
    # Arrange
    var health := HealthComponent.new()
    health.max_health = 100
    health.current_health = 100

    # Act
    health.take_damage(25)

    # Assert
    assert_eq(health.current_health, 75)
```

Why this passes:

- Name identifies system, scenario, and expected result.
- Arrange / Act / Assert is clear.
- Assertion is precise.
- No external state is used.

---

### Example 2: Bad Test Name

Issue:

```gdscript
func test1() -> void:
```

Response:

```md
Finding: Test name is not descriptive.

Fix:
Rename using the required pattern:

test_health_system_take_damage_reduces_health
```

---

### Example 3: Imprecise Assertion

Issue:

```gdscript
assert_true(h.current_health < 100)
```

Response:

```md
Finding: Assertion is imprecise.

Fix:
Assert the exact expected behavior:

assert_eq(h.current_health, 75)
```

---

### Example 4: Unit Test Uses Filesystem

Issue:

```text
Unit test reads `assets/data/combat_enemies.json`.
```

Response:

```md
Finding: Unit test depends on filesystem.

Fix:
Use inline fixture data or an in-memory fake config provider. If the purpose is validating real data file loading, move this to integration or data-validation test.
```

---

### Example 5: Integration Test Missing Cleanup

Issue:

```text
Integration test creates temp save file but does not delete it.
```

Response:

```md
Finding: Integration cleanup missing.

Fix:
Create file in unique temp path and delete it in teardown/finally block. Verify cleanup even when assertion fails.
```

---

### Example 6: Performance Test Without Threshold

Issue:

```text
Test records AI update time but never fails.
```

Response:

```md
Finding: Benchmark mislabeled as performance test.

Fix:
Define threshold, such as `AI update <= 2ms p95 for 50 agents on target build`, and fail when exceeded.
```

---

### Example 7: Bug Fix Without Regression Test

Issue:

```text
Bug fix for inventory duplicate-stack issue has no test.
```

Response:

```md
Finding: Missing regression test.

Fix:
Add `test_regression_[bug_id]_inventory_add_duplicate_item_merges_stack_once` that fails before the fix and passes after.
```

---

### Example 8: Flaky Test

Issue:

```text
Test passes locally but fails intermittently in CI.
```

Response:

```md
Finding: Flaky test.

Fix:
Investigate order dependency, timing, randomness, external state, and cleanup. Quarantine only with owner and review trigger if blocking unrelated work.
```

---

### Example 9: User Correction

User says:

```text
Parameterized tests should include the case name in the test output.
```

Response pattern:

```text
Understood. I’ll treat parameterized tests as incomplete unless each case has a descriptive case label in the output. Store this as a durable test-standard rule only if approved.
```

---

### Example 10: Case Where the Rule Must Not Learn

User says during debugging:

```text
Skip this flaky test for now.
```

Response pattern:

```text
That can be a temporary quarantine only with reason, owner, and review trigger. It should not become permanent policy, and the root cause still needs investigation.
```

---

## Final Test Standard

Tests under `tests/**` must be:

- descriptively named,
- arranged clearly,
- isolated where unit-level,
- cleaned up where integration-level,
- deterministic,
- fixture-safe,
- dependency-mocked,
- threshold-enforced for performance,
- regression-linked for bug fixes,
- evidence-backed,
- and honest about skipped, flaky, blocked, or unrun status.