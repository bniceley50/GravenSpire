---
paths:
  - "design/gdd/**"
---

# Design Document Rules

## Rule Set Name

Design Document Rules

## Mission

These rules govern all game design documents under:

```text
design/gdd/**
```

Their purpose is to ensure every mechanic, system, feature, loop, economy, encounter-facing rule, and player-facing design is precise, implementable, testable, traceable, reviewable, and safe to evolve.

A design document is not complete because it sounds good. It is complete when a programmer can implement it, a QA tester can verify it, a designer can tune it, a producer can scope it, and downstream teams can understand its dependencies.

The core design-document question is:

> Can this document be implemented, tested, tuned, and maintained without hidden assumptions, vague language, missing edge cases, or unsupported balance values?

---

## Operating Principles

1. **Eight required sections**
   - Every design document must contain:
     1. Overview
     2. Player Fantasy
     3. Detailed Rules
     4. Formulas
     5. Edge Cases
     6. Dependencies
     7. Tuning Knobs
     8. Acceptance Criteria

2. **No hand-waving**
   - “Should feel good,” “handle gracefully,” “tune later,” “works as expected,” and similar phrases are not valid specifications unless translated into concrete behavior and validation criteria.

3. **Implementation-ready detail**
   - The document must define rules, inputs, outputs, states, triggers, values, formulas, failure behavior, and dependencies with enough precision for implementation.

4. **Formulas must be complete**
   - Every formula must include:
     - named expression,
     - variable definitions,
     - units,
     - expected value ranges,
     - clamps or bounds,
     - worked example,
     - source/rationale.

5. **Edge cases must say what happens**
   - Every edge case must define actual behavior, not intent.
   - If behavior is unresolved, mark it `UNRESOLVED`, not “handle gracefully.”

6. **Dependencies are bidirectional**
   - If system A depends on system B, A’s document must mention B and B’s document must mention A.
   - Dependency documents must describe what data, events, state, ownership, and validation cross the boundary.

7. **Tuning knobs must be safe**
   - Every tuning knob must specify:
     - safe range,
     - default value,
     - gameplay aspect affected,
     - category,
     - owner,
     - source/rationale.

8. **Acceptance criteria must be testable**
   - A QA tester must be able to verify pass/fail.
   - Experiential goals require measurable playtest, telemetry, or review criteria.

9. **Balance values require provenance**
   - Every balance value must link to:
     - source formula,
     - playtest result,
     - comparable reference,
     - design rationale,
     - simulation result,
     - or explicit owner decision.

10. **Incremental writing is mandatory**
    - Create skeleton first.
    - Fill one section at a time.
    - Get user approval between sections.
    - Write each approved section immediately to persist decisions and manage context.

11. **Self-healing before completion**
    - When a section is vague, missing, contradictory, stale, untestable, or dependency-incomplete, stop, classify the issue, repair it safely, and report remaining risk.

12. **Bounded self-learning**
    - Lessons from approved design reviews, QA findings, playtests, balance changes, dependency conflicts, and user corrections may be stored only in reviewable locations.
    - Persistent lessons must be explicit, reversible, and subordinate to current user instructions and approved project decisions.

---

## Scope

These rules apply to:

```text
design/gdd/**
```

This includes, where present:

- core loop documents,
- combat system documents,
- movement system documents,
- progression documents,
- crafting documents,
- economy documents,
- reward documents,
- inventory documents,
- ability documents,
- item documents,
- enemy behavior documents,
- encounter-system documents,
- status-effect documents,
- save-relevant system designs,
- multiplayer-facing design rules,
- tutorial/onboarding mechanics,
- player-facing rule documents,
- system interaction documents.

---

## Non-Goals

These rules do not authorize:

- implementation code,
- engine architecture decisions,
- production scheduling decisions,
- final creative direction changes,
- narrative canon changes,
- UI visual design decisions,
- art/audio direction decisions,
- monetization decisions,
- dependency installation,
- file writes without the active agent’s approval workflow,
- persistent memory updates without approval.

---

## Design Document State Labels

Use these labels for GDD sections and documents:

```text
SKELETON_CREATED — required section headers exist.
DRAFT — written but not approved.
SECTION_APPROVED — section approved by user/owner.
SPEC_READY — all required sections complete and internally consistent.
IMPLEMENTATION_READY — spec has enough detail for engineering handoff.
QA_READY — acceptance criteria are testable and QA-reviewed.
IMPLEMENTED — behavior exists in code/content.
PLAYTESTED — validated through playtest.
BALANCE_REVIEWED — balance values reviewed against rationale/evidence.
SUPERSEDED — replaced by newer design.
DEPRECATED — no longer intended for implementation.
BLOCKED — missing decision, dependency, formula, owner, or evidence.
UNRESOLVED — explicitly open design question.
```

### State Rules

- Do not mark a document `SPEC_READY` unless all eight required sections are complete.
- Do not mark a document `IMPLEMENTATION_READY` if formulas, edge cases, or dependencies are missing.
- Do not mark a section `SECTION_APPROVED` without user/owner approval.
- Do not mark a feature `IMPLEMENTED` without file/build evidence.
- Do not mark experiential acceptance criteria validated without playtest, telemetry, or review evidence.
- `DRAFT` is not approval.

---

## Required Design Document Skeleton

Every design document under `design/gdd/**` must start with this structure:

```md
# [System / Mechanic Name]

## Document Status

- Status:
- Owner:
- Last updated:
- Source decision:
- Related documents:
- Implementation owner:
- QA owner:
- Open questions:

## 1. Overview

## 2. Player Fantasy

## 3. Detailed Rules

## 4. Formulas

## 5. Edge Cases

## 6. Dependencies

## 7. Tuning Knobs

## 8. Acceptance Criteria
```

---

## Section Quality Standards

### 1. Overview

The Overview must explain:

- what the system is,
- why it exists,
- what player behavior it supports,
- what design pillar or player fantasy it serves,
- what systems it touches,
- what is explicitly out of scope.

Required format:

```md
## 1. Overview

### Summary

### Design Purpose

### Player-Facing Role

### In Scope

### Out of Scope

### Related Systems
```

Invalid overview examples:

```text
This system handles combat.
```

```text
This should feel satisfying.
```

Valid overview style:

```text
This system resolves melee attacks by converting player attack commands into validated hit attempts, damage results, hit reactions, and combat feedback. It supports the player fantasy of deliberate, readable close-range mastery.
```

---

### 2. Player Fantasy

The Player Fantasy section must define what the player should feel, understand, and be able to do.

Required format:

```md
## 2. Player Fantasy

### Core Fantasy

### Target Emotions

### Player Motivation Served

### Skill / Mastery Promise

### Failure Fantasy

### Anti-Fantasy

### Validation
```

Rules:

- Define the emotional promise, not just the feature.
- Use concrete language.
- Explain what the system must never make the player feel if that would violate the design.
- Tie fantasy to mechanics and feedback.

Invalid:

```text
The player should feel good.
```

Valid:

```text
The player should feel like they are reading enemy intent, committing to deliberate attacks, and winning through timing rather than button mashing.
```

---

### 3. Detailed Rules

Detailed Rules must be precise enough for implementation.

Required format:

```md
## 3. Detailed Rules

### Inputs

### Outputs

### States

### Triggers

### Rules

| Rule ID | Rule | Source | Notes |
|---|---|---|---|

### State / Flow Description

### Failure Behavior

### Player Feedback
```

Rules:

- Use numbered rule IDs.
- Avoid hidden assumptions.
- Define what happens on invalid input.
- Define ownership of state.
- Define feedback.
- Define timing if timing matters.
- Define multiplayer/save/UI implications if relevant.

Invalid:

```text
Enemies react naturally.
```

Valid:

```text
R-COMBAT-014: If an enemy is hit while in `Windup`, the attack is interrupted unless the enemy has `uninterruptible=true`.
```

---

### 4. Formulas

Formulas must include variable definitions, expected value ranges, and example calculations.

Required format:

```md
## 4. Formulas

### Formula: [Formula Name]

**Expression**

```text
[result] = [expression]
```

### Variables

| Symbol | Name | Type | Unit | Range | Description |
|---|---|---|---|---|---|

### Output

- Type:
- Unit:
- Range:
- Clamp:
- Rounding:
- Failure behavior:

### Worked Example

### Source / Rationale

### Balance Notes
```

Formula rules:

- Define every variable.
- Define units.
- Define valid input ranges.
- Define output range.
- Define clamp or no clamp.
- Define rounding.
- Define zero, negative, maximum, overflow, and invalid input behavior.
- Provide at least one worked example.
- Link every balance value to rationale or source.

For probability or loot systems, include:

```md
### Probability Table

| Outcome | Weight / Probability | Conditions | Expected Attempts | Notes |
|---|---:|---|---:|---|
```

For curves, include:

```md
### Curve Definition

- Curve type:
- Anchor points:
- Interpolation:
- Clamp:
- Reason:
```

For interaction matrices, include:

```md
### Interaction Matrix

| A \ B | Type 1 | Type 2 | Type 3 |
|---|---|---|---|
```

---

### 5. Edge Cases

Edge cases must explicitly state what happens.

Required format:

```md
## 5. Edge Cases

| Edge Case ID | Scenario | Expected Behavior | Reason | Owner | Test Coverage |
|---|---|---|---|---|---|
```

Required edge-case categories where relevant:

```text
ZERO_VALUE
NEGATIVE_VALUE
MAX_VALUE
OVERFLOW
UNDERFLOW
NULL_OR_MISSING_DATA
INVALID_STATE
CONFLICTING_STATES
SIMULTANEOUS_EVENTS
ORDER_OF_OPERATIONS
SAVE_LOAD_INTERRUPTION
NETWORK_DESYNC
PLAYER_DISCONNECT
RESOURCE_EXHAUSTION
DUPLICATE_INPUT
RAPID_REPEAT_INPUT
COOLDOWN_BOUNDARY
FRAME_RATE_VARIANCE
LOCALIZATION_TEXT_OVERFLOW
ACCESSIBILITY_ALTERNATIVE
UI_UNAVAILABLE
AUDIO_VISUAL_FEEDBACK_MISSING
```

Rules:

- Do not write “handle gracefully.”
- State exact behavior.
- State whether behavior is fail-fast, fallback, ignore, clamp, retry, queue, or block.
- State whether QA can test it.
- Mark unresolved cases explicitly.

Invalid:

```text
Handle missing data gracefully.
```

Valid:

```text
If required config key `combat.base_damage` is missing, the system fails validation at load and the feature remains disabled in development builds. In release, it uses the last known valid config if available; otherwise combat starts blocked and reports a safe error.
```

---

### 6. Dependencies

Dependencies must be bidirectional.

Required format:

```md
## 6. Dependencies

### Dependency Summary

| System | Direction | This System Needs | This System Provides | Required In Other Doc | Status |
|---|---|---|---|---|---|

### Dependency Contracts

#### [System Name]

- Depends on:
- Provides:
- Data exchanged:
- Events/commands:
- Timing:
- Failure behavior:
- Owner:
- Linked document:
- Reverse dependency confirmed:
```

Dependency directions:

```text
INBOUND — another system feeds this system.
OUTBOUND — this system feeds another system.
BIDIRECTIONAL — both systems exchange data/events.
SHARED_STATE — both systems depend on a shared source of truth.
```

Rules:

- If this document mentions another system, the other system’s document must mention this one.
- Missing reverse reference is a documentation defect.
- Dependencies must include data flow and failure behavior.
- Do not hide cross-system requirements in prose only.

---

### 7. Tuning Knobs

Tuning knobs must specify safe ranges and what gameplay aspect they affect.

Required format:

```md
## 7. Tuning Knobs

| Knob | Category | Default | Safe Range | Affects | Source / Rationale | Owner | Notes |
|---|---|---:|---|---|---|---|---|
```

Tuning categories:

```text
FEEL — moment-to-moment sensation, responsiveness, timing.
CURVE — progression, scaling, pacing shape.
GATE — unlocks, thresholds, access, cooldowns.
ECONOMY — sources, sinks, prices, drop rates.
DIFFICULTY — challenge, enemy pressure, failure rate.
ACCESSIBILITY — timing, readability, assistive settings.
UX — feedback, clarity, interaction pacing.
LIVE_OPS — seasonal/event-specific tuning.
```

Rules:

- Every knob needs a default.
- Every knob needs a safe range.
- Every knob must say what it affects.
- Every balance value must link to a formula, rationale, playtest, simulation, or owner decision.
- Unsafe values must be documented.
- Tuning knobs must be externalizable by implementation.

Invalid:

```text
Tune damage later.
```

Valid:

```text
base_damage | FEEL/DIFFICULTY | 20 | 5–60 | Time-to-kill and perceived weapon weight | Derived from target TTK of 6s against baseline enemy
```

---

### 8. Acceptance Criteria

Acceptance criteria must be testable.

Required format:

```md
## 8. Acceptance Criteria

### Functional Criteria

| ID | Criterion | Test Method | Pass / Fail Definition | Evidence |
|---|---|---|---|---|

### Experiential Criteria

| ID | Criterion | Validation Method | Target | Evidence |
|---|---|---|---|---|

### QA Notes

### Telemetry / Playtest Notes

### Implementation Evidence Needed
```

Acceptance criteria types:

```text
FUNCTIONAL — binary behavior.
FORMULA — numeric output.
EDGE_CASE — unusual state.
INTEGRATION — cross-system behavior.
PERFORMANCE — budget or timing.
ACCESSIBILITY — accessibility requirement.
LOCALIZATION — locale/text behavior.
UX — player interaction clarity.
PLAYTEST — observed player behavior.
TELEMETRY — measured production/playtest data.
```

Rules:

- QA must be able to verify pass/fail.
- Functional criteria need exact expected behavior.
- Experiential criteria need measurable validation, not vibes.
- “Feels good” must be converted into observable criteria.
- If a criterion cannot yet be measured, mark it `NEEDS_VALIDATION_DESIGN`.

Invalid:

```text
Combat feels responsive.
```

Valid:

```text
AC-COMBAT-004: After pressing attack, the first active attack frame begins within 150ms at target framerate, unless the player is in recovery state.
```

---

## Balance Value Provenance

Every numeric balance value must include provenance.

### Balance Value Record

```md
## Balance Value: [Name]

- Value:
- Unit:
- Used in:
- Source:
  - Formula
  - Simulation
  - Playtest
  - Reference
  - Designer decision
  - Temporary placeholder
- Rationale:
- Safe range:
- Review trigger:
- Owner:
```

### Provenance Rules

- Temporary placeholder values must be labeled.
- Placeholder values cannot be treated as approved balance.
- If a value is copied from a reference game, state it as an inspiration point, not authoritative balance.
- If a value comes from a formula, cite the formula.
- If a value comes from playtest, cite the playtest.
- If a value is an owner decision, name the owner/decision record.

---

## Incremental Writing Protocol

Design documents must be written incrementally.

### Required Workflow

1. Create skeleton first.
2. Ask for approval to write skeleton.
3. Draft one section.
4. Review section with user.
5. Revise if needed.
6. Ask for approval to write that section.
7. Write approved section immediately.
8. Update session state if the project uses session-state tracking.
9. Continue to next section.
10. If a later section invalidates an earlier section, open a revision note.

### Section Approval Record

```md
## Section Approval

- Document:
- Section:
- Status:
- Approved by:
- Date/session:
- Open issues:
- Revision needed:
```

### Session State Format

If session state exists, update:

```text
production/session-state/active.md
```

with:

```md
## Active Design Document Task

- Document:
- Current section:
- Completed sections:
- Approved decisions:
- Open questions:
- Next section:
- Risks:
```

### Incremental Writing Rules

- Do not fill all sections at once unless user explicitly approves bulk drafting.
- Do not write unapproved sections to file.
- Do not leave approved decisions only in chat if file persistence is requested.
- If context is long, preserve approved decisions in the document before compacting.
- If the user rejects a section, revise before writing.

---

## Revision and Contradiction Handling

### Contradiction Record

```md
## Design Contradiction

- Document:
- Section A:
- Section B:
- Conflict:
- Impact:
- Proposed resolution:
- Owner:
- Status:
```

### Contradiction Rules

- Later sections may reveal earlier mistakes.
- Do not silently rewrite approved sections.
- Flag contradiction and ask for approval to revise.
- If dependency docs conflict, mark both documents needing review.
- If implementation contradicts design, escalate to Game Designer / Lead Programmer / relevant owner.

---

## Dependency Integrity Check

### Bidirectional Dependency Check

```md
## Bidirectional Dependency Check

| Source Doc | Depends On | Reverse Mention Found | Reverse Path | Status |
|---|---|---|---|---|
```

### Dependency Status

```text
CONFIRMED
MISSING_REVERSE
CONFLICTING
UNVERIFIED
NOT_REQUIRED
```

Rules:

- `MISSING_REVERSE` blocks `SPEC_READY` for cross-system dependencies.
- Dependency records must include failure behavior.
- If dependency is speculative, mark it `PROPOSED`, not confirmed.

---

## Design Quality Review

Use this format for reviewing GDDs:

```md
## Design Document Review: [Document]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Required Section Status

| Section | Status | Notes |
|---|---|---|

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Formula Status

### Edge Case Status

### Dependency Status

### Tuning Status

### Acceptance Criteria Status

### Required Follow-Up
```

### Severity

```text
GDD-S1 — Critical
Spec cannot be implemented safely or contradicts core project decision.

GDD-S2 — High
Missing required section, untestable acceptance criteria, missing formula detail, missing edge-case behavior, or broken dependency.

GDD-S3 — Medium
Weak rationale, incomplete tuning range, unclear player fantasy, partial dependency notes, missing example calculation.

GDD-S4 — Low
Formatting, naming, minor clarity, or documentation polish.
```

---

## Self-Learning Protocol

Self-learning means controlled improvement from approved design reviews, QA findings, playtests, balance reviews, dependency conflicts, implementation feedback, and user corrections.

It does not mean autonomous design changes, hidden memory updates, or treating one-off comments as permanent design policy.

### What May Be Learned

The design-document system may learn:

- approved section templates,
- recurring formula omissions,
- recurring edge-case categories,
- dependency documentation patterns,
- acceptance-criteria patterns,
- balance-provenance standards,
- tuning-knob safe-range conventions,
- QA findings,
- playtest validation patterns,
- implementation handoff failures,
- rejected vague language,
- rejected design approaches and why.

### What Must Not Be Learned or Stored

Do not store:

- private user data,
- private chain-of-thought,
- secrets or credentials,
- unapproved brainstorms as approved design,
- temporary placeholder values as balance rules,
- one-off playtest comments as universal rules,
- rejected options as active direction,
- speculative dependencies as confirmed,
- unsupported balance claims.

### Lesson Classification

Use:

```text
Confirmed Rule
Approved Design Standard
Section Template Finding
Formula Finding
Edge Case Finding
Dependency Finding
Tuning Finding
Acceptance Criteria Finding
Balance Finding
QA Finding
Playtest Finding
Implementation Feedback
Rejected Approach
Working Assumption
Temporary Context
Superseded
```

### Lesson Storage

Store durable lessons only in approved, reviewable locations such as:

```text
docs/design/design-document-standards.md
docs/design/formula-lessons.md
docs/design/edge-case-lessons.md
docs/design/dependency-lessons.md
docs/design/acceptance-criteria-lessons.md
tasks/lessons.md
production/qa/design/
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
- it applies to design-document quality,
- it does not include sensitive data,
- it is not overgeneralized,
- it does not conflict with accepted project decisions,
- it has a review trigger where appropriate.

### Lesson Expiry

Review or expire lessons when:

- design-document standards change,
- project tier changes,
- system architecture changes,
- QA process changes,
- balance methodology changes,
- playtest evidence contradicts the lesson,
- user or owner supersedes it,
- the lesson was temporary,
- the lesson is too broad.

---

## Self-Healing Protocol

Self-healing means detecting a design-document failure, containing the risk, repairing safely, verifying the repair, and reporting what changed.

### Failure Types

Monitor for:

- missing required section,
- vague overview,
- unclear player fantasy,
- hand-wavy detailed rules,
- missing formula,
- formula missing variable table,
- formula missing range,
- formula missing example,
- balance value missing rationale,
- edge case says “handle gracefully,”
- edge case missing behavior,
- dependency not bidirectional,
- tuning knob missing safe range,
- tuning knob missing gameplay impact,
- acceptance criterion not testable,
- contradiction between sections,
- stale design source,
- speculative dependency,
- implementation feedback conflict,
- missing approval,
- incomplete incremental writing.

### Recovery Loop

When failure occurs:

1. **Stop**
   - Do not mark document complete.

2. **Identify**
   - State the exact design-document failure.

3. **Classify**
   - Section, formula, edge case, dependency, tuning, acceptance criteria, contradiction, approval, or traceability.

4. **Contain**
   - Mark status:
     - `BLOCKED`,
     - `UNRESOLVED`,
     - `NEEDS_FIX`,
     - `NEEDS_EVIDENCE`,
     - `MISSING_REVERSE`,
     - `DRAFT`.

5. **Recover**
   - add missing section,
   - rewrite vague language,
   - add formula table,
   - add worked example,
   - define edge-case behavior,
   - add reverse dependency,
   - add safe tuning range,
   - make acceptance criteria testable,
   - resolve contradiction with approval,
   - update session state.

6. **Verify**
   - Re-run document checklist.
   - Confirm all eight sections.
   - Confirm dependencies.
   - Confirm criteria are testable.

7. **Report**
   - Summarize issue, repair, remaining risk, and owner.

8. **Learn**
   - Propose durable lesson only if validated and approved.

---

## Error Recovery

### Missing Required Section

If any of the eight sections are missing:

- add skeleton heading,
- mark section `DRAFT` or `UNRESOLVED`,
- do not mark document complete.

### Formula Incomplete

If formula lacks variables, ranges, or example:

- add named expression,
- add variable table,
- define ranges,
- define clamp/rounding,
- add worked example,
- add rationale.

### Edge Case Hand-Waving

If edge case says “handle gracefully”:

- replace with explicit behavior,
- define fallback/reject/clamp/retry/block behavior,
- add QA test method.

### Missing Bidirectional Dependency

If dependency appears in one doc only:

- mark `MISSING_REVERSE`,
- update or request update to the dependent doc,
- block `SPEC_READY` until resolved or waived.

### Untestable Acceptance Criteria

If acceptance criterion is subjective:

- convert to binary functional criterion,
- or define playtest/telemetry/review validation target,
- mark unresolved if no measurement exists.

### Missing Tuning Range

If tuning knob lacks safe range:

- define minimum,
- define maximum,
- define unsafe values,
- define gameplay impact,
- link formula/rationale.

### Contradiction Between Sections

If sections disagree:

- create contradiction record,
- propose resolution,
- request approval,
- update affected sections after approval.

### Approved Section Invalidated

If later work invalidates an approved section:

- do not silently overwrite,
- mark revision needed,
- ask for approval to revise,
- record the change.

---

## Memory Policy

### Short-Term Task Memory

Track during current design-document task:

- document path,
- active section,
- completed sections,
- approved sections,
- open questions,
- formulas,
- edge cases,
- dependencies,
- tuning knobs,
- acceptance criteria,
- contradictions,
- owner decisions.

Short-term memory expires after task completion unless explicitly stored.

### Project Memory

Project memory may store:

- approved design-document templates,
- recurring section-quality lessons,
- formula-format decisions,
- edge-case taxonomies,
- dependency documentation patterns,
- acceptance-criteria examples,
- balance-rationale standards,
- rejected vague phrases,
- validated review findings.

### Never Store

Never store:

- private user data,
- private chain-of-thought,
- secrets or credentials,
- unapproved brainstorms as approved design,
- temporary placeholder values as final balance,
- one-off comments as universal rules,
- unsupported balance claims.

---

## Feedback Policy

When the user, Game Designer, Systems Designer, Economy Designer, Level Designer, Narrative Director, UX Designer, Lead Programmer, QA Lead, Producer, or Technical Director corrects a design document:

1. Accept the correction.
2. Identify whether it affects:
   - overview,
   - player fantasy,
   - detailed rules,
   - formulas,
   - edge cases,
   - dependencies,
   - tuning knobs,
   - acceptance criteria,
   - balance provenance,
   - document state.
3. Revise the current section.
4. Ask whether the correction should become durable design-document guidance if reusable.
5. Store only if approved and evidence-backed.

---

## Tool-Use Policy

This rules file does not grant tools by itself. Agents applying it must follow their own tool permissions.

General guidance:

- Use file-reading tools to inspect GDDs, related docs, dependency docs, formulas, acceptance criteria, and session state.
- Use search tools to find dependencies, referenced systems, formulas, tuning keys, and unresolved markers.
- Use write/edit tools only after approval under the active agent’s workflow.
- Use Bash only if the active agent allows it and only under that agent’s safety policy.
- Do not use Bash to bypass write approval.
- Do not claim tests, playtests, telemetry, or QA validation were performed unless evidence exists.

---

## Safety Guardrails

Never allow GDDs under `design/gdd/**` to:

- omit required sections,
- use vague hand-waving as specification,
- include formulas without variable definitions,
- include formulas without value ranges,
- include formulas without examples,
- define edge cases without explicit behavior,
- document dependencies in only one direction,
- list tuning knobs without safe ranges,
- list tuning knobs without gameplay impact,
- include acceptance criteria that QA cannot verify,
- include balance values without rationale,
- treat draft sections as approved,
- write unapproved sections to file,
- silently overwrite approved decisions,
- store persistent lessons without approval.

---

## Output Standards

Design-document reviews and drafts should be:

- section-complete,
- implementation-ready,
- formula-complete,
- edge-case-explicit,
- dependency-bidirectional,
- tuning-safe,
- acceptance-testable,
- rationale-backed,
- approval-aware,
- clear about unresolved issues.

### Review Output Format

```md
## Design Document Review: [Document]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Required Sections

### Formula Quality

### Edge Case Quality

### Dependency Integrity

### Tuning Knobs

### Acceptance Criteria

### Approval / Incremental Writing Status

### Required Follow-Up
```

---

## Reflection Checklist

After drafting or reviewing a GDD section, privately check:

- Does the document have all eight required sections?
- Is this section approved or still draft?
- Are all formulas complete?
- Are all edge cases explicit?
- Are dependencies bidirectional?
- Are tuning knobs safe and sourced?
- Are acceptance criteria testable?
- Are balance values linked to rationale?
- Is any language vague?
- Did a later section contradict an earlier one?
- Did I preserve approved decisions?
- Did I avoid storing unapproved lessons?

Do not expose private chain-of-thought. Report conclusions, evidence, and recommendations.

---

## Evaluation Checklist

Before final approval of a design document:

### Required Structure

- [ ] Overview exists.
- [ ] Player Fantasy exists.
- [ ] Detailed Rules exists.
- [ ] Formulas exists.
- [ ] Edge Cases exists.
- [ ] Dependencies exists.
- [ ] Tuning Knobs exists.
- [ ] Acceptance Criteria exists.

### Formula Quality

- [ ] Every formula has a named expression.
- [ ] Every variable is defined.
- [ ] Every variable has type, unit, and range.
- [ ] Output range is defined.
- [ ] Clamping/rounding behavior is defined.
- [ ] Worked example exists.
- [ ] Balance values cite source/rationale.

### Edge Cases

- [ ] Edge cases state exact behavior.
- [ ] No “handle gracefully” without behavior.
- [ ] Invalid input behavior is defined.
- [ ] Zero/min/max behavior is defined where relevant.
- [ ] Failure states are defined.

### Dependencies

- [ ] All dependencies are listed.
- [ ] Dependency direction is defined.
- [ ] Reverse dependency is confirmed.
- [ ] Data/events exchanged are defined.
- [ ] Failure behavior is defined.

### Tuning

- [ ] Every tuning knob has default.
- [ ] Every tuning knob has safe range.
- [ ] Every tuning knob names gameplay impact.
- [ ] Every tuning knob has owner/rationale.
- [ ] Unsafe ranges are documented.

### Acceptance Criteria

- [ ] Criteria are pass/fail or measurable.
- [ ] QA can verify functional criteria.
- [ ] Experiential criteria have playtest/telemetry/review target.
- [ ] Edge cases have criteria where relevant.
- [ ] Performance/accessibility/localization criteria exist where relevant.

### Process

- [ ] Skeleton was created first.
- [ ] Sections were approved incrementally.
- [ ] Approved sections were persisted.
- [ ] Open questions are marked.
- [ ] No unapproved sections are treated as final.

---

## Example Workflows

### Example 1: Creating a New Combat GDD

Correct workflow:

1. Create skeleton with eight required sections.
2. Ask approval to write skeleton.
3. Draft Overview.
4. Get approval.
5. Write Overview.
6. Draft Player Fantasy.
7. Continue section by section.

---

### Example 2: Incomplete Formula

Issue:

```text
Damage = attack power times multiplier.
```

Response:

```md
Finding: Formula incomplete.

Fix:
- Add symbolic expression.
- Define `attack_power`, `damage_multiplier`, and `final_damage`.
- Define ranges and units.
- Define clamp/rounding.
- Add worked example.
- Link multiplier to balance rationale.
```

---

### Example 3: Hand-Wavy Edge Case

Issue:

```text
If stamina runs out, handle gracefully.
```

Response:

```md
Finding: Edge case is not specified.

Fix:
Define exact behavior, such as:
- Current action continues or cancels?
- Is player locked out?
- Is cooldown applied?
- What feedback appears?
- What can QA verify?
```

---

### Example 4: Missing Reverse Dependency

Issue:

```text
Combat GDD depends on Stamina GDD, but Stamina GDD does not mention Combat.
```

Response:

```md
Finding: Bidirectional dependency missing.

Fix:
- Add Combat dependency to Stamina GDD.
- Define stamina cost consumers.
- Define behavior if stamina API fails or returns insufficient stamina.
```

---

### Example 5: Untestable Acceptance Criteria

Issue:

```text
The crafting system feels rewarding.
```

Response:

```md
Finding: Untestable criterion.

Possible replacements:
- Player receives visible/audio confirmation within 500ms of successful craft.
- 80% of first-time playtesters understand the crafted item result without facilitator explanation.
- Crafting result screen displays item name, quantity, and one-line function summary.
```

---

### Example 6: Tuning Knob Missing Safe Range

Issue:

```text
enemy_spawn_rate: tune as needed
```

Response:

```md
Finding: Missing safe range and gameplay impact.

Fix:
- Define default.
- Define min/max.
- Define what happens outside safe range.
- Link to encounter pacing or performance rationale.
```

---

### Example 7: User Correction

User says:

```text
All cooldown formulas need both real-time and paused-time behavior.
```

Response pattern:

```text
Understood. I’ll treat cooldown formulas as incomplete unless they define pause behavior. Store this as a durable design-document rule only if approved.
```

---

### Example 8: Case Where the Rule Must Not Learn

User says during brainstorming:

```text
Maybe all weapons should have 1-second cooldowns.
```

Response pattern:

```text
I’ll treat that as a brainstorm value, not approved balance. If it becomes a real tuning value, it needs formula rationale, safe range, and acceptance criteria.
```

---

## Final Design Document Rule

Every design document under `design/gdd/**` must be:

- section-complete,
- precise,
- formula-backed,
- edge-case-explicit,
- dependency-bidirectional,
- tuning-safe,
- balance-rationale-linked,
- acceptance-testable,
- incrementally approved,
- persistently recorded,
- and honest about unresolved decisions.