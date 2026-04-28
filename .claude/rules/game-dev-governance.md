---
paths:
  - "**/*"
---

# Game Dev Governance Rules

## Rule Set Name

Game Dev Governance Rules

## Mission

These project-wide rules govern cross-cutting development discipline across all files and all agents.

They exist to prevent version drift, undocumented claims, scene corruption, speculative dependencies, tier creep, weak PR evidence, and accidental scope expansion.

These rules apply to:

```text
**/*
```

They are derived from `AGENTS.md` and related project governance files. They do not replace specialized path rules; they provide baseline governance that all other rules must respect.

The core governance question is:

> Is this work appropriate for the current tier, verified against the pinned project context, supported by evidence, safe to review, and free of speculative dependency or scope drift?

---

## Operating Principles

1. **Pinned project context wins**
   - The project is pinned to Unity 6.3 LTS.
   - Treat Unity 6.1–6.3 APIs as unverified unless documented in `docs/engine-reference/unity/`.
   - Do not rely on model memory for post-Unity-6.0 API details.

2. **URP only**
   - BIRP is not an option.
   - HDRP is blocked by D001 unless a formally approved photoreal pivot occurs.
   - New rendering recommendations must assume URP unless a decision record says otherwise.

3. **Evidence beats assertion**
   - Any “done” claim requires either:
     - passing test evidence, or
     - file:line evidence.
   - “Configured” alone is not evidence.
   - PR claims must have traceable evidence paths.

4. **Tier discipline protects scope**
   - Do not implement Tier N+1 features during Tier N.
   - Scope creep must be recorded as a `[SCOPE]` lesson in `tasks/lessons.md`.
   - Tier transitions require a new D-entry in `DECISIONS.md`.

5. **Scene files are fragile**
   - Never commit dirty `.unity` scenes.
   - Save first, inspect diff, then stage.
   - Use Unity Smart Merge for scene conflicts.
   - Do not hand-edit Unity scene YAML.
   - Prefer one scene edit per PR.

6. **Style gates are real gates**
   - Tier 1: `dotnet format --verify-no-changes` must pass locally before PR.
   - Tier 2+: same gate must run in CI via GameCI.
   - Tooling failure is not a pass.

7. **No speculative dependencies**
   - New libraries are added to `.claude/docs/technical-preferences.md` only when the system needing them starts active work.
   - Do not install, allow-list, or normalize future dependencies early.
   - Named-but-not-approved dependencies remain uninstalled until active work and approval.

8. **Reviewability matters**
   - Changes should be small enough to inspect.
   - Generated files, scene files, dependency changes, and project settings require extra evidence.

9. **Temporary exceptions are not precedent**
   - Emergency bypasses, prototypes, and one-off waivers must be labeled temporary.
   - They must not become normal process unless documented and approved.

10. **Self-healing before escalation**
   - When governance evidence is missing, scope is wrong, version checks fail, or a gate breaks, stop, classify the failure, repair safely, and report remaining risk.

11. **Bounded self-learning**
   - Durable governance lessons must be explicit, reviewable, reversible, and stored in approved files.
   - Lessons must not override pinned decisions, tier rules, evidence requirements, security rules, or current user instructions.

---

## Scope

These rules apply project-wide to:

- source files,
- design documents,
- scene files,
- test files,
- CI files,
- dependency files,
- project settings,
- documentation,
- PR descriptions,
- release notes,
- governance documents,
- task plans,
- lessons files,
- decision records.

They apply to all agents and all work unless a higher-priority system rule or explicitly approved project decision overrides them.

---

## Non-Goals

These rules do not:

- Define game design direction.
- Approve scope changes.
- Approve dependency adoption by themselves.
- Replace Technical Director decisions.
- Replace Creative Director decisions.
- Replace Producer tier/milestone planning.
- Replace QA evidence requirements.
- Replace Unity-specific specialist guidance.
- Allow file edits without the active agent’s write-approval process.
- Authorize CI, Git, Unity Editor, Bash, or build operations unless the active agent has those tools and approval.

---

## Governance State Labels

Use these labels when discussing governance status:

```text
PROPOSED — suggested but not approved.
APPROVED_RULE — accepted project governance.
ACTIVE_TIER — currently active tier constraint.
BLOCKED_BY_TIER — violates current tier boundary.
NEEDS_DECISION — requires DECISIONS.md entry or owner ruling.
NEEDS_EVIDENCE — claim lacks test or file:line evidence.
EVIDENCE_LINKED — claim has valid evidence path.
LOCAL_GATE_PASSED — local required gate passed.
CI_GATE_PASSED — CI required gate passed.
GATE_FAILED — required gate failed.
WAIVED — approved exception with owner and expiry.
TEMPORARY_EXCEPTION — one-off exception, not precedent.
SUPERSEDED — replaced by newer rule or decision.
UNVERIFIED — not yet checked against source of truth.
```

### State Rules

- Do not mark work “done” unless `EVIDENCE_LINKED`.
- Do not mark formatting compliant unless local or CI gate evidence exists.
- Do not mark Tier N+1 work acceptable during Tier N unless there is an approved tier transition or explicit waiver.
- Do not treat `TEMPORARY_EXCEPTION` as `APPROVED_RULE`.
- Do not treat `UNVERIFIED` Unity API usage as safe.

---

## Source of Truth

Primary governance files:

```text
AGENTS.md
DECISIONS.md
tasks/lessons.md
.claude/docs/technical-preferences.md
docs/engine-reference/unity/VERSION.md
docs/engine-reference/unity/deprecated-apis.md
docs/engine-reference/unity/breaking-changes.md
```

Related scoped rules:

```text
.claude/rules/netcode-conventions.md
.claude/rules/save-integrity.md
.claude/rules/llm-moderation.md
.claude/rules/network-code.md
.claude/rules/ai-code.md
```

### Source-of-Truth Rules

- Check `DECISIONS.md` for tier transitions, render-pipeline decisions, dependency decisions, and major architecture decisions.
- Check `AGENTS.md` for tier model and project-wide definitions.
- Check `.claude/docs/technical-preferences.md` before adding or recommending libraries.
- Check Unity engine-reference docs before recommending Unity APIs.
- Check specialized rule files when the domain applies.
- If sources conflict, stop and surface the conflict.

---

## Instruction Priority

When governance rules conflict, apply this hierarchy:

1. System, platform, privacy, legal, security, and safety constraints.
2. Current user instruction.
3. `DECISIONS.md` accepted decisions.
4. Active tier and milestone constraints.
5. `AGENTS.md` project governance.
6. Specialized scoped rules.
7. This cross-cutting governance file.
8. Department lead / director instructions.
9. Existing project conventions.
10. General best practices.
11. Working assumptions.

If a lower-priority rule conflicts with `DECISIONS.md`, `DECISIONS.md` wins unless superseded by a newer approved decision.

---

## Engine Version Awareness

### Current Engine Rule

```text
Unity 6.3 LTS is pinned.
```

### Unity API Verification Requirement

Before recommending, using, or documenting Unity-specific APIs, verify:

```text
docs/engine-reference/unity/VERSION.md
docs/engine-reference/unity/deprecated-apis.md
docs/engine-reference/unity/breaking-changes.md
```

For subsystem-specific work, also check the matching module reference if present:

```text
docs/engine-reference/unity/modules/rendering.md
docs/engine-reference/unity/modules/urp.md
docs/engine-reference/unity/modules/ui-toolkit.md
docs/engine-reference/unity/modules/input-system.md
docs/engine-reference/unity/modules/addressables.md
docs/engine-reference/unity/modules/physics.md
docs/engine-reference/unity/modules/build.md
```

### Post-Cutoff Rule

LLM knowledge is assumed reliable only up to approximately Unity 6.0. Unity 6.1, 6.2, and 6.3 APIs must be treated as:

```text
UNVERIFIED
```

unless they are explicitly documented in the engine reference.

### Deprecated API Examples

Do not recommend or use the following without explicit engine-reference verification and migration rationale:

```text
VisualElement.transform
URP SetupRenderPasses
URP Compatibility Mode patterns
```

### Render Pipeline Rule

```text
URP only.
BIRP is blocked.
HDRP is blocked by D001 unless a photoreal pivot is approved.
```

### Unity API Verification Record

Use this format when Unity API uncertainty matters:

```md
## Unity API Verification

- API / feature:
- Unity version:
- Reference files checked:
- Status:
  - VERIFIED
  - DEPRECATED
  - BLOCKED
  - UNVERIFIED
  - CONFLICTING_DOCS
- Decision impact:
- Recommendation:
- Evidence:
```

### Engine Version Self-Healing

If an API is unverified:

1. Stop treating it as safe.
2. Mark `UNVERIFIED`.
3. Check engine-reference docs.
4. If docs are missing, ask for verification or escalate to Unity specialist / Technical Director.
5. Prefer documented alternatives.
6. Record the finding if reusable.

---

## Code Style Gate

### Tier 1

Before any PR in Tier 1:

```bash
dotnet format --verify-no-changes
```

must pass locally.

### Tier 2+

The same gate must run in CI via GameCI.

### Style Gate Record

```md
## Code Style Gate Evidence

- Tier:
- Command:
- Run location:
  - Local
  - CI
- Result:
  - PASS
  - FAIL
  - BLOCKED
  - NOT_RUN
- Evidence:
- Notes:
```

### Style Gate Rules

- `NOT_RUN` is not acceptable for PR readiness.
- Tooling unavailable is `BLOCKED`, not pass.
- Generated files must be handled by documented project policy.
- Do not suppress formatting failures without approved waiver.
- If Tier 2+ CI is expected but missing, report governance gap.

### Style Gate Self-Healing

If formatting fails:

1. Classify failure:
   - code formatting,
   - generated file,
   - missing tool,
   - wrong .NET SDK,
   - CI configuration issue.
2. Fix formatting or identify owner.
3. Re-run gate.
4. Link evidence.
5. Do not claim PR readiness until gate passes or waiver exists.

---

## Scene Discipline

### Scene File Rules

For `.unity` scene files:

- Never commit unsaved dirty scene state.
- Save first.
- Inspect diff before staging.
- Prefer one scene edit per PR.
- Use Unity Smart Merge for conflicts.
- Do not hand-edit scene YAML.
- Do not resolve scene merge conflicts by guessing.
- Scene metadata changes require review.

### Scene Change Review Record

```md
## Scene Change Review

- Scene file:
- Dirty state checked:
- Saved before staging:
- Diff inspected:
- Number of scenes changed:
- Smart Merge required:
- Conflict status:
- Hand-edited YAML:
  - Yes / No
- Reviewer notes:
- Verdict:
```

### Scene Verdicts

```text
SCENE_OK
SCENE_NEEDS_REVIEW
SCENE_BLOCKED
SCENE_UNKNOWN
```

### Scene Self-Healing

If scene status is unsafe:

1. Stop staging/approval.
2. Save scene in Unity.
3. Inspect diff.
4. If conflict exists, use Unity Smart Merge.
5. If YAML was hand-edited, mark `SCENE_BLOCKED` until reviewed.
6. Split multi-scene PR if reviewability is poor.
7. Record exception if multiple scenes are necessary.

---

## Tier Discipline

### Core Tier Rules

- Do not implement Tier N+1 features during Tier N.
- Tier transitions require a new D-entry in `DECISIONS.md`.
- Cross-tier creep must be recorded as a `[SCOPE]` lesson in:

```text
tasks/lessons.md
```

### Tier Status Record

```md
## Tier Status

- Active tier:
- Work item:
- Claimed tier:
- Evidence:
- Cross-tier dependency:
- Tier risk:
- Verdict:
```

### Tier Verdicts

```text
IN_TIER
TIER_RISK
BLOCKED_BY_TIER
NEEDS_TIER_DECISION
```

### Scope Creep Lesson Format

```md
## [SCOPE] [Short title]

- Date:
- Active tier:
- Work item:
- Observed creep:
- Tier N+1 behavior:
- Impact:
- Recommendation:
- Owner:
- Status:
```

### Tier Transition Record

In `DECISIONS.md`, tier transitions must include:

```md
## D[NNN] — Tier Transition: [Tier N] to [Tier N+1]

- Status:
- Date:
- Previous tier:
- New tier:
- Reason:
- Required gates passed:
- Scope unlocked:
- Scope still blocked:
- Risks:
- Owners:
```

### Tier Self-Healing

If Tier N+1 work appears during Tier N:

1. Stop implementation.
2. Classify the work as tier creep.
3. Decide whether to:
   - defer,
   - stub,
   - document as future,
   - request tier transition,
   - request explicit waiver.
4. Record `[SCOPE]` lesson.
5. Do not merge Tier N+1 implementation without decision or waiver.

---

## Evidence Discipline

### Evidence Rule

Any “done” claim requires one of:

```text
Passing test evidence.
File:line reference.
```

“Configured” is not evidence unless the relevant file and line are cited.

Before a PR lands, every claim in the PR description must have a traceable evidence path.

### Evidence Types

Use:

```text
TEST_PASS
FILE_LINE
CI_PASS
LOCAL_GATE_PASS
QA_EVIDENCE
SCREENSHOT_EVIDENCE
PROFILE_EVIDENCE
CONFIG_LINE
DECISION_RECORD
WAIVER_RECORD
```

### Evidence Register Format

```md
## Evidence Register: [PR / Task / Feature]

| Claim ID | Claim | Evidence Type | Evidence Path | Status | Notes |
|---|---|---|---|---|---|
| C1 | dotnet format passes | LOCAL_GATE_PASS | [command output / CI link] | LINKED | |
| C2 | URP is configured | CONFIG_LINE | ProjectSettings/...:L12-L18 | LINKED | |
```

### Evidence Status

```text
LINKED
MISSING
INVALID
STALE
BLOCKED
NOT_APPLICABLE
```

### File:Line Evidence Rules

- Use file path and line range where possible.
- If line numbers are unavailable, quote a stable heading or config key.
- Generated files need generator source or artifact evidence.
- “I checked it” is not evidence.
- “Configured” must cite the config file line.

### Test Evidence Rules

Test evidence should include:

- command,
- run location,
- result,
- build/branch if relevant,
- date or CI run,
- failure notes if not pass.

### Evidence Self-Healing

If a claim lacks evidence:

1. Mark `NEEDS_EVIDENCE`.
2. Identify the needed evidence type.
3. Link file:line or test result.
4. Remove or weaken the claim if evidence cannot be produced.
5. Do not leave PR claims unsupported.

---

## Dependency Discipline

### Core Rule

New libraries are added to:

```text
.claude/docs/technical-preferences.md
```

Allowed Libraries list only when the system that needs them starts active work.

No speculative installs.

### Dependency Request Record

```md
## Dependency Request: [Library / Package]

- Status:
  - PROPOSED
  - ACTIVE_WORK_NEEDED
  - APPROVED_FOR_INSTALL
  - INSTALLED
  - REJECTED
  - DEFERRED
- Library:
- Version:
- System needing it:
- Active work evidence:
- Problem solved:
- Alternatives considered:
- Install location:
- Runtime impact:
- Build impact:
- Security/privacy impact:
- License:
- Owner:
- Approval:
```

### Dependency Rules

- Do not install libraries for future tiers.
- Do not add libraries to allowed list speculatively.
- Do not treat named future dependencies as approved.
- FishNet is named but not approved for install unless active work and approval exist.
- Dependency additions require Technical Director review where they affect architecture.
- Dependencies with security, privacy, build, platform, or licensing risk need additional owner review.
- Dependency updates are dependency changes and need evidence.

### Dependency Self-Healing

If a dependency appears without approval:

1. Stop.
2. Identify where it was added:
   - package file,
   - lock file,
   - project settings,
   - allowed libraries doc.
3. Check active work evidence.
4. If speculative, remove or mark deferred.
5. If needed, create dependency request.
6. Record lesson if repeated.

---

## PR Governance

### PR Evidence Checklist

Before a PR is ready:

```md
## PR Governance Checklist

- [ ] Work is in active tier.
- [ ] Tier creep checked.
- [ ] All claims have evidence paths.
- [ ] dotnet format gate passed where applicable.
- [ ] Unity APIs verified where applicable.
- [ ] No BIRP/HDRP drift.
- [ ] Scene files saved and diff-inspected if changed.
- [ ] No hand-edited scene YAML.
- [ ] Dependency changes approved.
- [ ] New libraries are not speculative.
- [ ] Required tests or QA evidence linked.
- [ ] Waivers documented if any.
```

### PR Claim Format

```md
## PR Claim

- Claim:
- Evidence:
- File:line or test:
- Status:
- Owner:
```

### PR Self-Healing

If PR claims are unsupported:

1. Convert claims into an evidence register.
2. Link each claim.
3. Remove claims that cannot be supported.
4. Mark missing gates.
5. Do not approve readiness until evidence is linked or waived.

---

## Waiver and Exception Policy

### Waiver Record

```md
## Governance Waiver

- Rule waived:
- Reason:
- Scope:
- Risk:
- Expiry / review trigger:
- Approved by:
- Alternative safeguard:
- Evidence:
```

### Waiver Rules

- Waivers are explicit.
- Waivers are scoped.
- Waivers expire or have review triggers.
- Waivers do not become precedent.
- Waivers for security, dependency, tier transition, scene corruption, or release gates require owner approval.
- Waived evidence remains visible.

### Temporary Exception Rules

Use `TEMPORARY_EXCEPTION` when:

- emergency work bypasses normal process,
- prototype work knowingly violates production standards,
- toolchain failure blocks a non-critical gate,
- one-off manual recovery is performed.

Temporary exceptions must be reviewed before becoming rules.

---

## Governance Incident Handling

### Incident Types

Use:

```text
ENGINE_VERSION_DRIFT
DEPRECATED_API_USED
RENDER_PIPELINE_DRIFT
STYLE_GATE_FAILURE
SCENE_CORRUPTION_RISK
SCENE_MERGE_FAILURE
TIER_CREEP
MISSING_EVIDENCE
SPECULATIVE_DEPENDENCY
UNAPPROVED_LIBRARY
PR_CLAIM_UNSUPPORTED
CONFIG_CLAIM_UNVERIFIED
WAIVER_MISSING
```

### Incident Record

```md
## Governance Incident

- Type:
- Severity:
- Detected in:
- Description:
- Evidence:
- Impact:
- Containment:
- Correction:
- Owner:
- Follow-up lesson:
```

### Severity

```text
GOV-S1 — Critical
Could corrupt project, violate security/privacy, break release, or invalidate governance.

GOV-S2 — High
Blocks PR/release readiness, introduces unapproved dependency, tier creep, or unsupported API risk.

GOV-S3 — Medium
Reviewability or evidence problem that can be corrected before merge.

GOV-S4 — Low
Documentation or process cleanup.
```

### Incident Rules

- Scene corruption risk is at least `GOV-S2`.
- Secret/dependency/security risk escalates to Security Engineer / Technical Director.
- Unsupported Unity API in production code is at least `GOV-S2`.
- Missing PR evidence is at least `GOV-S3`.
- Tier creep is at least `GOV-S2` unless explicitly deferred.

---

## Self-Learning Protocol

Self-learning means controlled improvement from approved decisions, PR failures, CI failures, formatting failures, scene incidents, dependency reviews, tier lessons, and user corrections.

It does not mean autonomous rule changes, hidden memory updates, or turning exceptions into policy.

### What May Be Learned

The governance system may learn:

- approved tier transition decisions,
- approved render-pipeline constraints,
- Unity API pitfalls,
- deprecated Unity API findings,
- code-style gate failure patterns,
- scene-diff review lessons,
- Smart Merge failure modes,
- recurring evidence gaps,
- dependency-request outcomes,
- approved library timing rules,
- repeated scope-creep patterns,
- PR evidence checklist improvements,
- rejected practices and why.

### What Must Not Be Learned or Stored

Do not store:

- secrets,
- credentials,
- private user data,
- private chain-of-thought,
- sensitive logs,
- raw telemetry containing personal data,
- unapproved exceptions as normal policy,
- speculative dependencies as approved,
- Tier N+1 ideas as active work,
- unsupported Unity API assumptions,
- one-off PR mistakes as global rules without evidence.

### Lesson Classification

Use:

```text
Confirmed Rule
Approved Governance Rule
Tier Finding
Scope Finding
Unity Version Finding
Deprecated API Finding
Render Pipeline Finding
Code Style Gate Finding
Scene Discipline Finding
Evidence Finding
Dependency Finding
PR Governance Finding
Waiver Finding
Incident Finding
Rejected Approach
Working Assumption
Temporary Context
Superseded
```

### Lesson Storage

Durable governance lessons should live in reviewable project files such as:

```text
tasks/lessons.md
devops/pipeline-lessons.md
docs/governance/governance-lessons.md
docs/governance/pr-evidence.md
docs/governance/dependency-lessons.md
docs/governance/scene-discipline.md
DECISIONS.md
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
- it is project-relevant,
- it does not include sensitive data,
- it is not overgeneralized,
- it has a review trigger where appropriate,
- it does not conflict with `DECISIONS.md`.

### Lesson Expiry

Review or expire lessons when:

- tier changes,
- Unity version changes,
- render pipeline decision changes,
- CI becomes available,
- GameCI gates change,
- dependency policy changes,
- scene workflow changes,
- evidence policy changes,
- owner supersedes the rule,
- lesson was temporary,
- lesson is too broad.

---

## Self-Healing Protocol

Self-healing means detecting a governance failure, containing the risk, repairing safely, verifying the repair, and documenting the learning if reusable.

### Failure Types

Monitor for:

- Unity API unverified,
- deprecated API recommended,
- BIRP/HDRP drift,
- format gate not run,
- format gate failed,
- scene dirty state,
- scene YAML hand-edit,
- multi-scene PR without rationale,
- Tier N+1 work during Tier N,
- missing `[SCOPE]` lesson,
- missing D-entry for tier transition,
- unsupported “done” claim,
- “configured” without file:line evidence,
- speculative dependency,
- unapproved package install,
- allowed library updated too early,
- missing waiver,
- conflicting governance sources.

### Recovery Loop

When failure occurs:

1. **Stop**
   - Do not continue as if governance is satisfied.

2. **Identify**
   - State the governance failure.

3. **Classify**
   - Engine version, style gate, scene, tier, evidence, dependency, waiver, or PR governance.

4. **Contain**
   - Mark status:
     - `UNVERIFIED`,
     - `NEEDS_EVIDENCE`,
     - `GATE_FAILED`,
     - `BLOCKED_BY_TIER`,
     - `SCENE_BLOCKED`,
     - `NEEDS_DECISION`.

5. **Recover**
   - verify Unity API,
   - run or request style gate,
   - save scene and inspect diff,
   - create `[SCOPE]` lesson,
   - add D-entry requirement,
   - link file:line evidence,
   - remove speculative dependency,
   - create waiver record.

6. **Verify**
   - Re-check source of truth.
   - Confirm evidence exists.
   - Confirm state label changed appropriately.

7. **Report**
   - Summarize issue, correction, remaining risk, and owner.

8. **Learn**
   - Propose durable lesson only if validated and approved.

---

## Error Recovery

### Unity API Unverified

If an API is suggested or used without verification:

- mark `UNVERIFIED`,
- check Unity reference docs,
- if deprecated, replace with documented current API,
- if docs are missing, escalate to Unity specialist or Technical Director,
- do not claim compatibility.

### Deprecated Unity API

If a deprecated API appears:

- identify deprecated API,
- cite reference docs or local rule if available,
- propose replacement,
- mark affected PR blocked until fixed or waived.

### Render Pipeline Drift

If BIRP, HDRP, or compatibility-mode assumptions appear:

- check `DECISIONS.md`,
- confirm URP-only rule,
- block drift unless approved D-entry exists,
- escalate to Technical Director for pipeline pivot.

### Style Gate Failure

If `dotnet format --verify-no-changes` fails:

- do not mark PR ready,
- fix formatting or assign owner,
- rerun locally or in CI depending on tier,
- attach evidence.

### Scene Discipline Failure

If a `.unity` scene is dirty, conflicted, or hand-edited:

- stop,
- save in Unity,
- use Smart Merge for conflicts,
- inspect diff,
- split PR if needed,
- block hand-edited YAML until reviewed.

### Tier Creep

If Tier N+1 work appears in Tier N:

- identify exact Tier N+1 behavior,
- defer or stub,
- record `[SCOPE]` lesson,
- request D-entry only if transition is intended.

### Missing Evidence

If a claim lacks evidence:

- convert claim to evidence register row,
- link file:line or test pass,
- remove/soften unsupported claim,
- mark PR not ready until fixed.

### Speculative Dependency

If a dependency is proposed too early:

- mark `DEFERRED`,
- do not install,
- do not add to allowed list,
- record active work trigger needed,
- create dependency request if appropriate.

---

## Memory Policy

### Short-Term Task Memory

Track during current task:

- active tier,
- files affected,
- Unity API references,
- render-pipeline assumptions,
- formatting gate status,
- scene-file status,
- PR claims,
- evidence paths,
- dependency changes,
- waivers,
- open decisions.

Short-term memory expires after the task unless explicitly stored.

### Project Memory

Project memory may store:

- approved governance rules,
- tier-transition decisions,
- Unity API pitfalls,
- render-pipeline constraints,
- style-gate lessons,
- scene-discipline lessons,
- dependency-review outcomes,
- recurring evidence failures,
- waiver patterns,
- rejected practices.

### Never Store

Never store:

- secrets,
- credentials,
- tokens,
- private keys,
- private user data,
- private chain-of-thought,
- sensitive logs,
- unapproved temporary exceptions as policy,
- unsupported Unity API claims,
- speculative dependencies as approved libraries.

---

## Feedback Policy

When the user, Technical Director, Producer, Lead Programmer, QA Lead, DevOps Engineer, Unity Specialist, Security Engineer, or Release Manager corrects governance behavior:

1. Accept the correction.
2. Identify whether it affects:
   - Unity version policy,
   - render pipeline,
   - code style gate,
   - scene workflow,
   - tier policy,
   - evidence policy,
   - dependency policy,
   - waiver policy,
   - memory.
3. Revise current output.
4. Ask whether the correction should become durable governance guidance if reusable.
5. Store only if approved and evidence-backed.

---

## Safety Guardrails

Do not:

- recommend unverified Unity 6.1–6.3 APIs,
- recommend BIRP,
- recommend HDRP without approved pivot,
- claim `dotnet format` passed without evidence,
- mark PR work done without test or file:line evidence,
- accept “configured” without file:line evidence,
- hand-edit Unity scene YAML,
- merge dirty scenes,
- implement Tier N+1 during Tier N,
- install speculative libraries,
- add dependencies to allowed list before active work,
- treat temporary exceptions as precedent,
- silently update persistent memory.

---

## Output Standards

Governance responses should be:

- source-of-truth-aware,
- tier-aware,
- evidence-driven,
- Unity-version-aware,
- scene-safety-aware,
- dependency-disciplined,
- explicit about unknowns,
- clear about required owner approval,
- actionable.

### Governance Review Output Format

```md
## Governance Review: [Task / PR / Change]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Scope

### Active Tier

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Unity Version Status

### Render Pipeline Status

### Code Style Gate Status

### Scene Discipline Status

### Evidence Register Status

### Dependency Status

### Tier / Scope Status

### Waivers

### Required Follow-Up
```

---

## Reflection Checklist

After any governance-sensitive review, privately check:

- Did I identify the active tier?
- Did I check whether the work belongs to this tier?
- Did I check Unity API version risk?
- Did I enforce URP-only / no BIRP?
- Did I check style-gate evidence?
- Did I check scene-file safety if relevant?
- Did I require file:line or test evidence for “done” claims?
- Did I reject “configured” without line evidence?
- Did I detect speculative dependencies?
- Did I avoid treating exceptions as precedent?
- Did I avoid silent memory updates?

Report only findings, evidence, and recommendations.

---

## Evaluation Checklist

Before final approval of governance-sensitive work:

### Engine Version

- [ ] Unity 6.3 LTS assumption respected.
- [ ] Unity 6.1–6.3 APIs verified or marked unverified.
- [ ] Deprecated APIs checked.
- [ ] URP-only rule respected.
- [ ] BIRP not recommended.
- [ ] HDRP not recommended unless approved D-entry exists.

### Code Style

- [ ] Tier 1 local `dotnet format --verify-no-changes` evidence exists.
- [ ] Tier 2+ CI GameCI formatting evidence exists.
- [ ] Formatting failures are fixed or waived.
- [ ] Generated file handling is documented.

### Scene Discipline

- [ ] `.unity` scene dirty state checked.
- [ ] Scene diff inspected.
- [ ] Unity Smart Merge used for conflicts.
- [ ] No hand-edited YAML.
- [ ] Multi-scene PR has rationale.

### Tier Discipline

- [ ] Active tier identified.
- [ ] Tier N+1 work absent, deferred, or approved.
- [ ] Scope creep recorded as `[SCOPE]` lesson if found.
- [ ] Tier transition has or requires D-entry in `DECISIONS.md`.

### Evidence

- [ ] Every “done” claim has test or file:line evidence.
- [ ] Every “configured” claim cites relevant line.
- [ ] PR description claims have evidence paths.
- [ ] Evidence is current, not stale.

### Dependency Discipline

- [ ] No speculative installs.
- [ ] New library tied to active work.
- [ ] Allowed Libraries updated only with approval.
- [ ] Dependency request exists where needed.
- [ ] Named future dependencies are not treated as approved.

### Safety

- [ ] Waivers are explicit and scoped.
- [ ] No hidden exceptions.
- [ ] No sensitive data stored.
- [ ] Persistent lessons require approval.

---

## Example Workflows

### Example 1: Unity API Recommendation

Issue:

```text
A task recommends VisualElement.transform for UI Toolkit animation.
```

Governance response:

```md
Finding: Unity API verification risk.

VisualElement.transform is specifically listed as deprecated-risk under the project’s Unity 6.3 governance. Verify against `docs/engine-reference/unity/deprecated-apis.md` and replace with the documented Unity 6.3-safe pattern before implementation.
```

---

### Example 2: Render Pipeline Drift

Issue:

```text
A shader task suggests BIRP fallback support.
```

Governance response:

```md
Finding: Render pipeline violation.

BIRP is not an option. The project is URP-only. Remove BIRP fallback language unless a new accepted D-entry changes the render-pipeline decision.
```

---

### Example 3: PR Done Claim Without Evidence

Issue:

```text
PR says: “Save system configured.”
```

Governance response:

```md
Finding: Missing evidence.

“Configured” alone is not evidence. Cite the exact file:line where the save config is defined, or provide a passing test that proves the configuration is active.
```

---

### Example 4: Tier Creep

Issue:

```text
During Tier 1, a PR adds multiplayer lobby scaffolding.
```

Governance response:

```md
Finding: Tier creep.

This appears to be Tier N+1 work. Defer it, replace it with a documented future hook if necessary, and record a `[SCOPE]` lesson in `tasks/lessons.md`.
```

---

### Example 5: Speculative Dependency

Issue:

```text
A package is added because networking will need it later.
```

Governance response:

```md
Finding: Speculative dependency.

Dependencies are added only when the system needing them starts active work. Remove the package or create a dependency request tied to active approved work.
```

---

### Example 6: Dirty Scene

Issue:

```text
A `.unity` scene was committed without saving and diff inspection.
```

Governance response:

```md
Finding: Scene discipline blocker.

Open the scene in Unity, save it, inspect the diff, and only then stage. If conflict exists, use Unity Smart Merge. Do not hand-edit YAML.
```

---

### Example 7: User Correction

User says:

```text
Tier 2 starts only after the save system and combat prototype are both accepted.
```

Response pattern:

```text
Understood. I’ll treat save-system acceptance plus combat-prototype acceptance as required Tier 2 entry gates unless superseded by a new D-entry. This should be recorded in `DECISIONS.md` if intended as durable governance.
```

---

### Example 8: Case Where the Rule Must Not Learn

User says during an emergency:

```text
Skip format for this PR.
```

Response pattern:

```text
This can be recorded as a temporary waiver only if explicitly approved with scope and expiry. It must not become normal policy; the default style gate remains `dotnet format --verify-no-changes`.
```

---

## See Also

- `.claude/rules/netcode-conventions.md` — Tier 2+ networking integrity: security and server authority.
- `.claude/rules/save-integrity.md` — Tier 1+ save format rules: HMAC and versioning.
- `.claude/rules/llm-moderation.md` — Tier 3+ LLM output policy.
- `.claude/rules/network-code.md` — network code style.
- `.claude/rules/ai-code.md` — AI code style.

---

## Final Governance Rule

All project work must be:

- Unity-version-aware,
- URP-aligned,
- tier-correct,
- evidence-backed,
- style-gated,
- scene-safe,
- dependency-disciplined,
- waiver-explicit,
- reviewable,
- and honest about uncertainty.