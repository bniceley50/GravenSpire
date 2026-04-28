---
paths:
  - "prototypes/**"
---

# Prototype Code Standards

## Rule Set Name

Prototype Code Standards

## Mission

These rules govern all prototype work under:

```text
prototypes/**
```

Their purpose is to let the team test ideas quickly while preventing prototype code from contaminating production systems.

Prototype code is disposable. Prototype learning is valuable. The code should be fast to create, easy to run, easy to judge, and safe to throw away.

The core prototype question is:

> What are we trying to learn, what did we build to test it, what evidence did we collect, and what decision does that evidence support?

---

## Operating Principles

1. **Learning over quality**
   - Prototype code exists to validate an idea, not to become production software.
   - Optimize for speed, clarity of experiment, and evidence.

2. **Isolation is mandatory**
   - All prototype code lives under:

```text
prototypes/[prototype-name]/
```

   - Production code must never import, reference, depend on, or ship prototype code.

3. **Relaxed standards are scoped**
   - Hardcoded values, copy-paste, global state, debug output, placeholder assets, and quick architecture are allowed only inside `prototypes/**`.

4. **Safety standards are not relaxed**
   - Prototypes must not leak secrets, mutate production files, access private data, bypass security controls, or ship to players.

5. **Every prototype has a hypothesis**
   - A prototype without a question is not a prototype; it is unfocused development.
   - Every prototype must state what it is trying to prove, disprove, or learn.

6. **Every prototype has a README**
   - Each prototype must include:

```text
prototypes/[prototype-name]/README.md
```

   - The README is the source of truth for purpose, run instructions, status, findings, and decision.

7. **Prototype code is never migrated directly**
   - If a prototype succeeds, the production feature is rewritten from scratch under production rules.
   - Prototype findings inform production design and architecture; prototype code does not become production code.

8. **Conclude prototypes explicitly**
   - Every prototype must end as:
     - proceed,
     - pivot,
     - kill,
     - inconclusive,
     - archived,
     - deleted.

9. **Capture findings before cleanup**
   - Do not delete or archive a prototype until findings are captured.
   - Findings should be linked into the relevant production design document if the concept proceeds.

10. **Do not extend concluded prototypes**
   - A concluded prototype is reference material, not a working branch.
   - New questions require a new prototype or production implementation.

11. **Self-healing**
   - When prototype scope creeps, isolation breaks, README is missing, findings are absent, or production references appear, stop, contain, repair, verify, and report.

12. **Bounded self-learning**
   - Lessons from prototypes may inform future work only when captured in reviewable files.
   - Prototype results must not become production rules without owner review.

---

## Scope

These rules apply to:

```text
prototypes/**
```

This includes:

- prototype source files,
- prototype scenes,
- prototype assets,
- prototype data,
- prototype scripts,
- prototype README files,
- prototype reports,
- prototype screenshots or notes,
- prototype-local test harnesses,
- prototype-only debug tools.

---

## Non-Goals

These rules do not authorize prototypes to:

- modify production source files,
- modify production data files,
- change project-wide settings,
- change build or CI infrastructure,
- add production dependencies,
- ship to players,
- deploy to live environments,
- bypass security, privacy, or legal requirements,
- become production implementation through cleanup,
- overwrite approved design documents without review,
- store persistent lessons without approval.

---

## Prototype Lifecycle States

Use these labels for every prototype:

```text
PROPOSED — idea suggested but not approved.
APPROVED — prototype approved for creation.
IN_PROGRESS — prototype is being built or tested.
PLAYABLE — prototype can be run and evaluated.
MEASURED — evidence or observations have been captured.
CONCLUDED_PROCEED — concept validated; production rewrite recommended.
CONCLUDED_PIVOT — concept partly valid; new direction recommended.
CONCLUDED_KILL — concept not worth pursuing.
INCONCLUSIVE — insufficient evidence; decision cannot be made.
FINDINGS_CAPTURED — README/report contains final findings.
DESIGN_DOC_UPDATED — findings reflected in production design documentation.
ARCHIVED — preserved for reference, not active.
DELETED — removed after findings are captured.
BLOCKED — cannot proceed due to missing hypothesis, safety issue, or isolation failure.
CONTAMINATED — prototype has leaked into production or production references it.
SUPERSEDED — replaced by newer prototype or production implementation.
```

### State Rules

- Do not mark `PLAYABLE` unless run instructions exist.
- Do not mark `MEASURED` without evidence, observation notes, or metrics.
- Do not mark `CONCLUDED_*` without findings.
- Do not mark `DESIGN_DOC_UPDATED` without a linked production design doc or approved note.
- Do not archive or delete before `FINDINGS_CAPTURED`.
- `CONTAMINATED` blocks production readiness until repaired.

---

## What Is Allowed in Prototypes

Inside `prototypes/[name]/`, the following are allowed:

- hardcoded values,
- placeholder art,
- placeholder audio,
- temporary input mappings,
- temporary UI,
- debug output,
- copy-pasted code,
- simple scripts,
- global state,
- singletons,
- minimal architecture,
- minimal or no doc comments,
- manual testing,
- crude visualizations,
- local-only test data,
- simplified physics,
- mocked dependencies,
- fake content,
- throwaway scenes,
- quick-and-dirty solutions.

### Allowed Does Not Mean Production-Safe

Any allowed shortcut must remain prototype-local. It must not be copied into production without production rewrite.

---

## What Is Still Required

Every prototype must:

- live in its own subdirectory:

```text
prototypes/[name]/
```

- include a README:

```text
prototypes/[name]/README.md
```

- state the hypothesis,
- state how to run it,
- state current status,
- capture findings when concluded,
- avoid modifying files outside `prototypes/`,
- avoid production imports/references,
- avoid deployment/shipping,
- avoid secrets/private data,
- avoid unsafe destructive scripts,
- preserve findings before archive/delete.

---

## Prototype README Standard

Every prototype README must include:

```md
# Prototype: [Name]

## Status

- Status:
- Owner:
- Created:
- Last updated:
- Timebox:
- Decision:
  - Undecided
  - Proceed
  - Pivot
  - Kill
  - Inconclusive

## Hypothesis

## Question Being Tested

## Scope

### In Scope

### Out of Scope

## How to Run

## Controls / Interaction

## What Was Built

## Evidence / Metrics

## Findings

## Decision Rationale

## If Proceeding to Production

## Cleanup / Archive Status

## Links

- Related design doc:
- Related production issue/task:
- Related follow-up prototype:
```

### README Rules

- README must exist before prototype is considered valid.
- Hypothesis must be specific.
- Run instructions must be executable by another team member.
- Findings must be updated when prototype concludes.
- Decision must be recorded.
- If proceeding, production implications must be captured.
- If killed, reason must be captured.

---

## Hypothesis Standard

A good hypothesis is specific, testable, and timeboxed.

### Hypothesis Format

```md
## Hypothesis

We believe that [idea/mechanic/approach] will produce [desired result] for [target player/user/context].

We will know this is true if [observable evidence].

We will reject or pivot if [failure signal].
```

### Examples

Good:

```text
We believe a dash with 150ms startup and 300ms cooldown will make melee combat feel more responsive without letting players bypass enemy positioning. We will know this is true if testers use dash defensively and offensively without trivializing the baseline encounter.
```

Bad:

```text
Try dash mechanic.
```

### Hypothesis Rules

- One primary question per prototype.
- Secondary questions are allowed but must not drive scope.
- If the question changes materially, create a new prototype or revise the README.

---

## Scope and Timebox Discipline

### Timebox Record

```md
## Timebox

- Start:
- Intended duration:
- Hard stop:
- Extension requested:
- Extension approved by:
- Reason:
```

### Scope Rules

- Build only what answers the hypothesis.
- Do not add menus unless UI is the question.
- Do not add art polish unless visual feel is the question.
- Do not add save/load unless persistence is the question.
- Do not add production architecture unless architecture feasibility is the question.
- Do not continue past the timebox without explicit approval.

### Scope Creep Signals

Watch for:

- “Just one more feature.”
- “This is almost production-ready.”
- “Let’s clean it up and ship it.”
- “The prototype needs a full menu.”
- “We need reusable architecture.”
- “Let’s hook it into production so we can test it properly.”
- “Let’s keep extending this instead of rewriting.”

If these appear, mark `SCOPE_CREEP_RISK`.

---

## Isolation Requirements

### Directory Rule

Every prototype lives here:

```text
prototypes/[prototype-name]/
```

No prototype may modify files outside its own directory unless explicitly approved as a prototype-local reference copy.

### Import / Reference Rules

Production code must not import from:

```text
prototypes/**
```

Prototype code should not import from production code unless explicitly allowed for a technical feasibility test. If it does, mark the README clearly:

```text
Production dependency used for test only. Do not migrate directly.
```

Preferred pattern:

```text
Copy minimal needed behavior into the prototype.
Mock production systems.
Use local prototype-only fixtures.
```

### Isolation Review

```md
## Prototype Isolation Review

- Prototype path:
- Own subdirectory:
- README present:
- Files outside prototype modified:
- Production imports prototype:
- Prototype imports production:
- Production assets referenced:
- Production data mutated:
- Verdict:
```

### Isolation Verdicts

```text
ISOLATED
ISOLATED_WITH_NOTES
CONTAMINATED
UNKNOWN
```

---

## Prototype Header

Each source file in a prototype should include a short header where practical:

```text
// PROTOTYPE - NOT FOR PRODUCTION
// Prototype: [name]
// Question: [what this tests]
// Created: [date/session]
```

For formats that do not support comments, document file purpose in the README.

### Header Rules

- Header is strongly recommended for source files.
- Header is required for files that look similar to production code.
- Do not add invalid comments to formats that prohibit them, such as strict JSON.

---

## Evidence and Metrics

### Evidence Types

Use any appropriate evidence:

```text
PLAYTEST_NOTES
FRAME_TIME_METRICS
VIDEO_CAPTURE
SCREENSHOT
DESIGNER_OBSERVATION
USER_FEEDBACK
COMPARISON_TABLE
TIMING_MEASUREMENT
FAILURE_CASE
TECHNICAL_FEASIBILITY_RESULT
```

### Evidence Record

```md
## Evidence / Metrics

| Evidence | Method | Result | Confidence | Notes |
|---|---|---|---|---|
```

### Evidence Rules

- Evidence can be lightweight.
- Manual observations are acceptable if labeled.
- Metrics should be captured when measurable.
- Do not overstate evidence.
- One prototype result is evidence, not proof of universal design truth.
- If no evidence exists, mark decision `INCONCLUSIVE`.

---

## Prototype Report

If the prototype is substantial, add:

```text
prototypes/[name]/REPORT.md
```

### Report Format

```md
# Prototype Report: [Concept Name]

## Hypothesis

## Approach

## What Was Built

## Result

## Metrics

## Findings

## Recommendation

Use one:

- PROCEED
- PIVOT
- KILL
- INCONCLUSIVE

## Production Implications

## What Must Be Rewritten

## Risks Discovered

## Follow-Up Questions

## Lessons Learned

## Archive / Cleanup Recommendation
```

### Report Rules

- Report should focus on what was learned, not how polished the prototype is.
- Recommendations must be evidence-based.
- If proceeding, list what must change for production.
- If killed, capture why to avoid repeating the same experiment.

---

## Production Promotion Gate

When a prototype validates a concept and the feature moves to production:

1. Do not migrate prototype code directly.
2. Extract findings into the relevant production design document.
3. Create or update a production task/story.
4. Rebuild from scratch under production rules.
5. Treat prototype hardcoded values as placeholders unless validated.
6. Identify production requirements:
   - architecture,
   - data-driven values,
   - tests,
   - accessibility,
   - performance,
   - security,
   - localization,
   - QA.
7. Preserve prototype directory for reference or archive/delete after findings are captured.

### Promotion Record

```md
## Prototype Promotion Review

- Prototype:
- Decision:
- Production feature:
- Related design doc:
- Findings captured:
- Prototype code migrated:
  - MUST BE NO
- Production rewrite required:
- Production rules that apply:
- Owner:
- Approval:
```

### Promotion Rules

- Prototype code is not copied into production.
- Prototype assets are not shipped unless relicensed/reviewed and productionized.
- Prototype dependencies are not adopted without Technical Director review.
- Prototype tuning values are not production balance.
- Prototype UI/UX is not final unless reviewed by UX/UI/Art owners.
- Prototype performance results need production validation.

---

## Archive and Deletion Policy

### Archive Criteria

Archive when:

- findings are useful for future reference,
- evidence is non-trivial,
- prototype demonstrates a rejected idea worth remembering,
- production implementation is pending,
- technical feasibility findings may be reused.

### Delete Criteria

Delete when:

- findings are captured elsewhere,
- code is misleading or unsafe,
- prototype is obsolete,
- prototype contains risky artifacts,
- prototype clutters the repo,
- prototype is trivial and no longer useful.

### Archive Record

```md
## Prototype Archive Record

- Prototype:
- Status:
- Findings captured:
- Related design doc:
- Archive reason:
- Delete after:
- Owner:
```

### Cleanup Rules

- Do not archive/delete until findings are captured.
- Do not keep abandoned prototypes without status.
- Concluded prototypes should not be extended.
- New questions require a new prototype.

---

## Safety Guardrails That Remain Active

Even in prototypes, never allow:

- secrets,
- credentials,
- private keys,
- tokens,
- private player data,
- production data mutation,
- unsafe file deletion outside prototype path,
- deployment to live environments,
- shipping to players,
- production imports from prototype code,
- legal/licensing violations,
- harmful external network calls,
- malware-like behavior,
- code that bypasses security/privacy controls,
- unreviewed third-party dependency installation,
- destructive scripts that affect the repository outside `prototypes/`.

---

## Dependency and Asset Policy

### Prototype Dependencies

Prototype-only dependencies may be used only if:

- local to the prototype,
- documented in README,
- not added to production allowed libraries,
- not required by production build,
- not installed globally without approval,
- license risk is checked if assets/code may influence production.

### Prototype Assets

Placeholder assets are allowed, but:

- mark them as placeholders,
- do not ship them,
- do not assume licenses are production-safe,
- document source if using external assets,
- replace or review before production use.

### Dependency Record

```md
## Prototype Dependency / Asset Record

- Name:
- Source:
- Purpose:
- Prototype-only:
- License known:
- Production-safe:
- Notes:
```

---

## Prototype Review Format

Use this for reviews:

```md
## Prototype Review: [Prototype Name]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Hypothesis Status

### README Status

### Isolation Status

### Scope / Timebox Status

### Evidence Status

### Production Contamination Status

### Safety Status

### Decision Status

### Cleanup Status

### Required Follow-Up
```

### Severity

```text
PROTO-S1 — Critical
Prototype contaminates production, can ship accidentally, modifies files outside prototypes, exposes secrets/private data, or creates unsafe external side effects.

PROTO-S2 — High
Missing README, missing hypothesis, production references prototype, prototype lacks findings after conclusion, or production migration is attempted directly.

PROTO-S3 — Medium
Scope creep, unclear run instructions, weak evidence, missing status, stale prototype.

PROTO-S4 — Low
Documentation cleanup, naming issue, minor README polish.
```

---

## Self-Learning Protocol

Self-learning means controlled improvement from prototype findings, failed experiments, evidence, production promotion reviews, cleanup reviews, and user corrections.

It does not mean hidden memory updates, automatic production adoption, or treating a prototype result as universal truth.

### What May Be Learned

The prototype rule system may learn:

- which prototype questions produced useful evidence,
- recurring prototype scope-creep patterns,
- common isolation failures,
- common missing README sections,
- useful evidence metrics,
- failed assumptions,
- production rewrite requirements,
- prototype cleanup practices,
- rejected prototype directions and why.

### What Must Not Be Learned or Stored

Do not store:

- private user data,
- private chain-of-thought,
- secrets,
- credentials,
- private keys,
- raw private logs,
- prototype hardcoded values as production balance,
- prototype architecture as production architecture,
- one-off prototype result as universal rule,
- placeholder asset choices as final art direction,
- temporary debug behavior as production behavior.

### Lesson Classification

Use:

```text
Confirmed Rule
Approved Prototype Standard
Hypothesis Finding
Scope Finding
Isolation Finding
README Finding
Evidence Finding
Metrics Finding
Promotion Finding
Cleanup Finding
Prototype Failure Finding
Production Rewrite Finding
Rejected Approach
Working Assumption
Temporary Context
Superseded
```

### Lesson Storage

Store durable lessons only in approved, reviewable locations such as:

```text
docs/prototypes/prototype-standards.md
docs/prototypes/prototype-lessons.md
tasks/lessons.md
design/gdd/[related-system].md
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
- it is evidence-backed or approved,
- it does not include sensitive data,
- it does not overgeneralize from one prototype,
- it does not conflict with production rules,
- it has an owner or review trigger where appropriate.

### Lesson Expiry

Review or expire lessons when:

- prototype is superseded,
- production implementation contradicts prototype findings,
- design direction changes,
- architecture changes,
- performance evidence changes,
- playtest evidence changes,
- owner supersedes the lesson,
- the lesson was temporary,
- the lesson is too broad.

---

## Self-Healing Protocol

Self-healing means detecting a prototype governance failure, containing the risk, repairing safely, verifying the repair, and reporting what changed.

### Failure Types

Monitor for:

- missing README,
- missing hypothesis,
- missing run instructions,
- missing status,
- missing findings,
- missing decision,
- prototype outside `prototypes/[name]/`,
- prototype modifies files outside `prototypes/`,
- production imports prototype,
- prototype imports production without disclosure,
- prototype is deployed or shipped,
- prototype contains secrets,
- prototype contains private data,
- prototype grows past scope,
- prototype continues past timebox,
- concluded prototype is extended,
- direct production migration attempted,
- findings not captured before archive/delete.

### Recovery Loop

When failure occurs:

1. **Stop**
   - Do not continue as if prototype is valid or production-safe.

2. **Identify**
   - State the exact issue.

3. **Classify**
   - README, hypothesis, isolation, scope, safety, promotion, evidence, or cleanup failure.

4. **Contain**
   - Mark status:
     - `BLOCKED`,
     - `CONTAMINATED`,
     - `SCOPE_CREEP_RISK`,
     - `FINDINGS_MISSING`,
     - `PROMOTION_BLOCKED`.

5. **Recover**
   - add README,
   - write hypothesis,
   - add run instructions,
   - move files into proper prototype directory,
   - remove production references,
   - disclose prototype imports,
   - capture findings,
   - stop extending concluded prototype,
   - create production rewrite task,
   - archive/delete after findings.

6. **Verify**
   - Re-check isolation.
   - Re-check README.
   - Re-check production reference direction.
   - Re-check decision/finding status.

7. **Report**
   - Summarize issue, fix, remaining risk, and owner.

8. **Learn**
   - Propose durable lesson only if validated and approved.

---

## Error Recovery

### Missing README

If a prototype lacks README:

- mark `BLOCKED`,
- create `README.md`,
- add hypothesis,
- add run instructions,
- add status,
- add findings section,
- do not consider prototype valid until complete.

### Missing Hypothesis

If hypothesis is missing:

- stop adding features,
- define the single question,
- document success/failure signals,
- cut scope that does not serve the question.

### Prototype Outside Directory

If prototype files are outside `prototypes/[name]/`:

- move them into proper subdirectory after approval,
- check whether production files were modified,
- revert unintended production modifications,
- update README.

### Production Imports Prototype

If production code imports prototype:

- mark `CONTAMINATED`,
- remove dependency,
- replace with production implementation or approved abstraction,
- review build/deploy risk.

### Prototype Imports Production

If prototype imports production code:

- document dependency in README,
- confirm it is for test only,
- avoid modifying production code,
- consider copying/mock minimal behavior instead.

### Prototype Modifies Production Files

If prototype changes files outside `prototypes/`:

- stop,
- list modified files,
- revert or isolate changes after approval,
- document contamination risk.

### Direct Migration Attempt

If prototype code is being moved into production:

- block migration,
- extract findings,
- create production design/task,
- rewrite from scratch under production standards.

### Missing Findings

If prototype is concluded but findings are absent:

- mark `FINDINGS_MISSING`,
- add result, evidence, decision, and production implications,
- do not archive/delete until complete.

### Scope Creep

If prototype expands beyond hypothesis:

- identify new scope,
- decide whether to cut, split, or create a new prototype,
- update timebox only with approval.

### Stale Prototype

If prototype remains in progress indefinitely:

- mark stale,
- request conclusion,
- capture current findings,
- archive/delete or re-scope.

---

## Memory Policy

### Short-Term Task Memory

Track during current prototype task:

- prototype name,
- hypothesis,
- directory,
- README status,
- run instructions,
- timebox,
- scope,
- files created,
- evidence,
- findings,
- decision,
- production implications,
- cleanup status.

Short-term memory expires after task completion unless explicitly stored.

### Project Memory

Project memory may store:

- approved prototype standards,
- useful hypothesis formats,
- evidence patterns,
- scope-creep lessons,
- isolation failure lessons,
- production rewrite lessons,
- cleanup decisions,
- rejected prototype approaches.

### Never Store

Never store:

- secrets,
- credentials,
- private keys,
- private user/player data,
- private chain-of-thought,
- prototype code as production rule,
- hardcoded prototype values as approved balance,
- placeholder assets as approved art direction,
- one-off results as universal truths.

---

## Feedback Policy

When the user, Creative Director, Technical Director, Producer, Game Designer, Lead Programmer, QA Lead, or relevant domain owner corrects prototype behavior:

1. Accept the correction.
2. Identify whether it affects:
   - hypothesis,
   - scope,
   - isolation,
   - run instructions,
   - evidence,
   - findings,
   - decision,
   - production rewrite,
   - archive/delete,
   - safety.
3. Revise the README/report or review output.
4. Ask whether the correction should become durable prototype guidance if reusable.
5. Store only if approved and evidence-backed.

---

## Tool-Use Policy

This rules file does not grant tools by itself. Agents applying it must follow their own tool permissions.

General guidance:

- Use file-reading tools to inspect prototype README files, reports, and source.
- Use search tools to find production references to `prototypes/`.
- Use write/edit tools only after approval under the active agent’s workflow.
- Use Bash only if the active agent allows it and only under that agent’s safety policy.
- Do not run destructive cleanup, delete prototypes, or modify production files without explicit approval.
- Do not use Bash to bypass write/edit approval.

---

## Safety Guardrails

Never allow prototype work under `prototypes/**` to:

- ship to players,
- deploy to live environments,
- modify production source without approval,
- be imported by production code,
- contain secrets,
- contain private player data,
- become production code through cleanup,
- continue indefinitely without conclusion,
- lack a README,
- lack findings after conclusion,
- be promoted without production rewrite,
- install production dependencies speculatively,
- erase findings before cleanup.

---

## Output Standards

Prototype reviews and summaries should be:

- hypothesis-focused,
- timebox-aware,
- isolation-aware,
- evidence-based,
- honest about uncertainty,
- explicit about production implications,
- clear about proceed/pivot/kill decisions,
- clear about cleanup status.

### Review Output Format

```md
## Prototype Review: [Prototype]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Hypothesis

### README Status

### Isolation Status

### Scope / Timebox

### Evidence

### Decision

### Production Implications

### Cleanup Status

### Required Follow-Up
```

---

## Reflection Checklist

After reviewing or drafting prototype work, privately check:

- Is the prototype inside `prototypes/[name]/`?
- Does the README exist?
- Is the hypothesis specific?
- Are run instructions clear?
- Is status current?
- Is scope limited to the question?
- Are findings captured?
- Is there a proceed/pivot/kill/inconclusive decision?
- Did production code reference the prototype?
- Did the prototype modify files outside `prototypes/`?
- Is direct migration blocked?
- Is cleanup/archive status defined?
- Did I avoid storing unapproved lessons?

Do not expose private chain-of-thought. Report findings, evidence, and recommendations.

---

## Evaluation Checklist

Before a prototype is considered valid:

### Structure

- [ ] Prototype lives in `prototypes/[name]/`.
- [ ] README exists.
- [ ] README includes hypothesis.
- [ ] README includes how to run.
- [ ] README includes status.
- [ ] README includes findings section.
- [ ] README includes decision field.

### Isolation

- [ ] No production code imports prototype.
- [ ] Prototype does not modify files outside `prototypes/`.
- [ ] Prototype dependencies are prototype-local or documented.
- [ ] Prototype assets are placeholder-labeled.
- [ ] Prototype is not included in shipping/deployment path.

### Evidence

- [ ] Evidence or observation method is defined.
- [ ] Metrics are captured if relevant.
- [ ] Findings are recorded when concluded.
- [ ] Decision is evidence-based.
- [ ] Uncertainty is stated honestly.

### Promotion / Cleanup

- [ ] Production rewrite is required if proceeding.
- [ ] Findings are linked to design doc if proceeding.
- [ ] Prototype code is not migrated directly.
- [ ] Archive/delete decision exists after conclusion.
- [ ] Concluded prototype is not extended.

### Safety

- [ ] No secrets.
- [ ] No private data.
- [ ] No unsafe external side effects.
- [ ] No speculative production dependency.
- [ ] No unapproved persistent lesson storage.

---

## Example Workflows

### Example 1: New Movement Prototype

Correct setup:

```text
prototypes/dash_feel/
  README.md
  dash_feel_scene
  dash_feel_controller
```

README excerpt:

```md
## Hypothesis

We believe a short dash with a brief recovery window will make melee combat feel more responsive without allowing players to ignore enemy spacing.

## How to Run

Open `prototypes/dash_feel/dash_feel_scene` and press Play.

## Status

IN_PROGRESS
```

---

### Example 2: Missing README

Issue:

```text
prototypes/combat_test/ exists but has no README.md.
```

Response:

```md
Finding: Missing prototype README.

Severity: PROTO-S2.

Fix:
Create `prototypes/combat_test/README.md` with hypothesis, how to run, status, and findings section before continuing.
```

---

### Example 3: Production Imports Prototype

Issue:

```text
src/gameplay/player_controller imports prototypes/dash_feel/dash_logic.
```

Response:

```md
Finding: Production contamination.

Severity: PROTO-S1.

Fix:
Remove production dependency on prototype code. If dash proceeds, create a production implementation from scratch under gameplay-code standards.
```

---

### Example 4: Prototype Modifies Production Data

Issue:

```text
Prototype edits `assets/data/combat_enemies.json`.
```

Response:

```md
Finding: Prototype modified production data.

Severity: PROTO-S1 or PROTO-S2 depending on impact.

Fix:
Move test data into `prototypes/[name]/data/` or use mock data. Revert or review production data change separately under data-file rules.
```

---

### Example 5: Prototype Succeeds

Issue:

```text
Dash prototype validated the concept.
```

Response:

```md
Correct next step:
- Capture findings in README or REPORT.md.
- Update relevant GDD with lessons.
- Create production task.
- Rewrite dash under production gameplay standards.
- Preserve prototype for reference or archive it.
```

---

### Example 6: Prototype Scope Creep

Issue:

```text
A combat-feel prototype now includes inventory, save/load, and menus.
```

Response:

```md
Finding: Scope creep.

Fix:
Cut everything not needed to answer the combat-feel question, or split those into separate prototypes with their own hypotheses and README files.
```

---

### Example 7: Prototype Concluded but Findings Missing

Issue:

```text
README status says concluded, but Findings is empty.
```

Response:

```md
Finding: Findings missing.

Fix:
Add result, evidence, decision, production implications, and cleanup recommendation before archive/delete.
```

---

### Example 8: User Correction

User says:

```text
Prototype README decisions should use proceed / pivot / kill / inconclusive only.
```

Response pattern:

```text
Understood. I’ll treat those four labels as the approved prototype decision vocabulary. Store this as a durable prototype rule only if approved.
```

---

### Example 9: Case Where the Rule Must Not Learn

User says during a prototype:

```text
This hardcoded dash speed feels great, use it everywhere.
```

Response pattern:

```text
Treat that as prototype evidence, not approved production balance. If the dash proceeds, the value needs design rationale, config exposure, tests, and production tuning review.
```

---

## Final Prototype Rule

Prototype work under `prototypes/**` must be:

- isolated,
- hypothesis-driven,
- fast,
- disposable,
- README-documented,
- evidence-capturing,
- safe,
- never shipped,
- never imported by production,
- never migrated directly,
- concluded explicitly,
- cleaned up deliberately,
- and honest about what was actually learned.