---
name: architecture-decision
description: "Creates or retrofits an Architecture Decision Record (ADR) documenting a significant technical decision, context, alternatives, consequences, engine compatibility, GDD traceability, and registry impact."
argument-hint: "[title] [--review full|lean|solo] [--dry-run] OR retrofit <docs/architecture/adr-NNNN-title.md> [--review full|lean|solo] [--dry-run]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Task, AskUserQuestion
---

When this skill is invoked, execute the workflow below.

## 0. Operating Contract

### Autonomy

- Default to maximum autonomy. Make reasonable assumptions from repository evidence and proceed.
- Do not ask broad clarification questions when the repository already provides enough context.
- Use `AskUserQuestion` only for:
  - unresolved product/design choices that materially change the ADR,
  - invalid or unsafe paths,
  - retrofit writes to existing ADRs,
  - registry updates,
  - story-status updates,
  - GDD updates,
  - explicit supersession or scoped exceptions.
- New ADR creation is the primary purpose of this skill. If the target path is valid and `--dry-run` is not active, writing the new ADR is authorized by invocation.
- `--dry-run` performs discovery, drafting, and review, but does not write files or update registry/GDD/story artifacts.

### Review modes

Resolve review mode once:

1. Explicit `--review full|lean|solo`.
2. Else `production/review-mode.txt`.
3. Else `lean`.

Mode semantics:

| Mode | Engine Specialist | Technical Director |
|------|-------------------|--------------------|
| `full` | Yes | Yes |
| `lean` | Yes | No |
| `solo` | No | No |

If the review-mode file is missing, malformed, or empty, default to `lean` and report that assumption once.

### Path safety

Before reading or writing a user-supplied path:

1. Normalize it.
2. Reject absolute paths.
3. Reject any path containing `..`.
4. Require retrofit paths to match `docs/architecture/adr-*.md`.
5. Require new ADR output paths to live under `docs/architecture/`.

---

## 1. Parse Invocation

If the first argument is `retrofit`, enter retrofit mode.

Expected format:

```text
/architecture-decision retrofit docs/architecture/adr-0001-event-system.md
```

If no title or retrofit path was provided, ask:

```text
What technical decision are you documenting? Provide a short title, for example:
- event-system-architecture
- physics-engine-choice
- save-data-schema
```

Use the response as the ADR title and continue.

---

## 2. Discover Engine Context

This phase applies to new ADRs and to retrofit mode when adding or validating `## Engine Compatibility`.

### 2.1 Determine configured engine

Determine `[engine]` in this order:

1. Read `.claude/docs/technical-preferences.md` and look for the configured engine.
2. Else inspect directories under `docs/engine-reference/`.
3. If exactly one engine directory exists, use it.
4. If multiple engine directories exist, ask which one applies.
5. If no engine reference exists, stop and ask the user to run `/setup-engine` or provide engine name and version.

### 2.2 Read engine reference files

Read:

```text
docs/engine-reference/[engine]/VERSION.md
docs/engine-reference/[engine]/breaking-changes.md
docs/engine-reference/[engine]/deprecated-apis.md
```

Identify the decision domain from the title, user description, GDD context, or related code.

Common domains:

```text
Physics
Rendering
UI
Audio
Navigation
Animation
Networking
Core
Input
Scripting
Persistence
Tools
Build
```

If a module reference exists, read:

```text
docs/engine-reference/[engine]/modules/[domain].md
```

### 2.3 Missing engine files

| Missing file | Behavior |
|-------------|----------|
| `VERSION.md` | Stop. Engine compatibility cannot be verified. |
| `breaking-changes.md` | Continue, but mark knowledge risk at least `MEDIUM`. |
| `deprecated-apis.md` | Continue, but mark deprecated API coverage as incomplete. |
| `modules/[domain].md` | Continue, but mark domain reference as unavailable. |
| `.claude/docs/technical-preferences.md` | Continue without specialist discovery unless review mode requires it. |
| `.claude/docs/director-gates.md` | In `full` mode, stop and report that TD gate config is unavailable. |

### 2.4 Knowledge-gap warning

If the domain has `MEDIUM` or `HIGH` risk, show:

```text
⚠️ ENGINE KNOWLEDGE GAP WARNING

Engine: [engine name + version]
Domain: [domain]
Risk Level: [LOW | MEDIUM | HIGH]

Relevant verified changes:
- [change 1]
- [change 2]

This ADR must use the local engine-reference docs as source of truth.
Do not rely only on training data for this decision.
```

---

## 3. Retrofit Existing ADR

Only run this phase when invocation starts with `retrofit`.

### 3.1 Read and classify the existing ADR

Read the complete ADR file.

Scan headings and classify:

| Section | Severity if missing | Reason |
|---------|--------------------|--------|
| `## Status` | BLOCKING | Story readiness cannot check ADR acceptance. |
| `## ADR Dependencies` | HIGH | Dependency ordering and unblocking cannot be verified. |
| `## Engine Compatibility` | HIGH | Engine-version risk is unknown. |
| `## GDD Requirements Addressed` | MEDIUM | Requirements traceability is incomplete. |
| `## Registry Impact` | MEDIUM | Registry update intent is ambiguous. |
| `## Sources Consulted` | LOW | Audit trail is incomplete. |

### 3.2 Present retrofit preview

Show:

```text
## Retrofit: [ADR title]
File: [path]

Sections already present and will not be touched:
✓ Status: [value or "present"]
✓ [section]

Missing sections that can be appended:
✗ Status — BLOCKING
✗ ADR Dependencies — HIGH
✗ Engine Compatibility — HIGH
✗ GDD Requirements Addressed — MEDIUM
✗ Registry Impact — MEDIUM
✗ Sources Consulted — LOW
```

Ask:

```text
Shall I prepare and preview the missing sections? I will not modify existing content.
```

Options:

```text
[A] Prepare missing sections
[B] Stop here
```

### 3.3 Gather missing retrofit information

For `## Status`, ask:

```text
What is the current status of this decision?
[A] Proposed
[B] Accepted
[C] Deprecated
[D] Superseded by ADR-XXXX
```

For `## ADR Dependencies`, capture:

```text
Depends On:
Enables:
Blocks:
Ordering Note:
```

Allow `None`.

For `## Engine Compatibility`, use the engine context from Phase 2 and ask the user to confirm the domain only if uncertain.

For `## GDD Requirements Addressed`, infer candidates from GDD references and ask only if the mapping is ambiguous.

For `## Registry Impact`, infer candidates from the ADR and ask the user to confirm.

For `## Sources Consulted`, include the existing ADR path plus any engine reference, GDD, registry, and related ADR files read during the retrofit.

### 3.4 Preview exact retrofit append

Before writing, show the exact markdown sections that will be appended.

Ask:

```text
May I append these missing sections to [path]?
[A] Yes — append these sections
[B] Not yet — revise the generated sections
[C] Stop without writing
```

Only append if the user selects `[A]`.

Never edit or rewrite existing content in retrofit mode.

### 3.5 Close retrofit mode

After retrofit append, say:

```text
Retrofit complete. Run `/architecture-review` in a fresh session to re-validate coverage.
```

Do not continue into new ADR creation.

---

## 4. Create New ADR

Only run this phase when not in retrofit mode.

### 4.1 Determine next ADR number

Scan:

```text
docs/architecture/
```

Find existing files matching:

```text
adr-[NNNN]-*.md
```

Use the next available number.

### 4.2 Detect likely duplicate ADRs

Before assigning the final filename, search existing ADRs for:

- Similar title.
- Same domain.
- Same system name.
- Same state ownership.
- Same interface names.
- Same GDD systems.
- Same registry stances.

If likely duplicates are found, show the overlap. Ask only if the overlap would create duplicate or contradictory decisions.

### 4.3 Gather bounded context

Read relevant sources using this budget:

| Source type | Maximum |
|------------|---------|
| Existing ADRs | 5 most relevant |
| Source files | 5 most relevant |
| GDD files | 5 most relevant |
| Registry entries | All matching entries |
| Engine reference files | All required for domain |

If more candidates are found, summarize them and use the top matches unless the user asks to expand scope.

---

## 5. Architecture Registry Check

Read:

```text
docs/registry/architecture.yaml
```

If missing, continue and report:

```text
No architecture registry found. Registry conflict checking is unavailable.
```

Extract entries relevant to:

- Domain.
- System name.
- State ownership.
- Interface contracts.
- API decisions.
- Forbidden patterns.
- Performance budgets.

Treat only entries from `Accepted` ADRs or entries with `status: active` as binding. Treat entries from `Proposed` ADRs as non-binding but relevant.

If the proposed decision contradicts a binding stance, stop and ask:

```text
⚠️ Architecture conflict detected

This ADR appears to propose:
  [new proposal]

But ADR-[NNNN] established:
  [existing stance]

Choose one:
[A] Align this ADR with the existing stance
[B] Supersede ADR-[NNNN] with an explicit replacement
[C] Treat this as a scoped exception
[D] Stop
```

For a scoped exception, require:

```text
Exception scope:
Exception rationale:
Systems affected:
Systems explicitly not affected:
```

Record the exception in `Context`, `Alternatives Considered`, `Related Decisions`, and `Registry Impact`.

---

## 6. Collaborative Decision Framing

Before asking questions, derive assumptions from gathered context.

Use `AskUserQuestion` only if needed:

```text
Here is what I am assuming before drafting:

Problem:
[one-sentence problem statement]

Decision domain:
[domain]

Alternatives to evaluate:
A) [option derived from engine reference]
B) [option derived from GDD requirements]
C) [option derived from project architecture]

GDD systems driving this:
- [system]

Known constraints:
- [constraint]

Dependencies:
- Depends On: [ADR or None]
- Enables: [ADR/Epic or None]
- Blocks: [Epic/Story or None]

Initial status:
Proposed

Proceed?
[A] Draft with these assumptions
[B] Change alternatives
[C] Adjust GDD linkage
[D] Add performance budget constraint
[E] Change dependencies
[F] Something else needs changing first
```

If no material ambiguity exists, proceed and record assumptions in the ADR.

---

## 7. Draft ADR

Use this exact structure.

```markdown
# ADR-[NNNN]: [Title]

## Status

[Proposed | Accepted | Deprecated | Superseded by ADR-XXXX]

## Date

[YYYY-MM-DD]

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | [engine name + version] |
| **Domain** | [domain] |
| **Knowledge Risk** | [LOW / MEDIUM / HIGH] |
| **Engine Reference Version** | [from VERSION.md, or "Unknown"] |
| **References Consulted** | [engine-reference files read] |
| **Breaking Changes Checked** | [Yes / Partial / No] |
| **Deprecated APIs Checked** | [Yes / Partial / No] |
| **Post-Cutoff APIs Used** | [APIs or "None"] |
| **Deprecated APIs Avoided** | [APIs or "None"] |
| **Verification Required** | [specific behavior to test, or "None"] |
| **Test Harness / Scene Required** | [specific test harness, scene, or "None"] |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | [ADR-NNNN or "None"] |
| **Enables** | [ADR-NNNN, Epic, Story, or "None"] |
| **Blocks** | [Epic, Story, or "None"] |
| **Ordering Note** | [sequencing constraint or "None"] |

## Context

### Problem Statement

[What problem is being solved and why this decision is needed now.]

### Constraints

- [Technical constraint]
- [Engine/version constraint]
- [GDD requirement]
- [Performance constraint]
- [Schedule/resource constraint]

### Architectural Exception

[Only include if this ADR intentionally diverges from an existing stance. Otherwise write "None."]

## Decision

[The specific decision in implementable terms.]

### Architecture Diagram

[ASCII diagram or concise structural description.]

### Key Interfaces

[Signals, methods, data contracts, components, ownership boundaries, or APIs.]

## Alternatives Considered

### Alternative 1: [Name]

- **Description**: [how it works]
- **Pros**: [advantages]
- **Cons**: [costs]
- **Rejection Reason**: [why not chosen]

### Alternative 2: [Name]

- **Description**: [how it works]
- **Pros**: [advantages]
- **Cons**: [costs]
- **Rejection Reason**: [why not chosen]

### Alternative 3: [Name]

- **Description**: [how it works]
- **Pros**: [advantages]
- **Cons**: [costs]
- **Rejection Reason**: [why not chosen]

## Consequences

### Positive

- [benefit]

### Negative

- [trade-off]

### Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| [risk] | [impact] | [mitigation] |

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|------------|-------------|----------------------------|
| [system-name].md | [specific rule, formula, or constraint] | [how the ADR satisfies it] |

## Registry Impact

### State Ownership

- [state] → [owner], or "None"

### Interface Contracts

- [interface/signal/API] → [contract], or "None"

### Performance Budgets

- [system] → [budget], or "None"

### API Decisions

- [API/pattern] → [decision], or "None"

### Forbidden Patterns

- [pattern] → [reason], or "None"

### Supersedes Existing Registry Entries

- [entry] → [reason], or "None"

## Performance Implications

- **CPU**: [expected impact]
- **Memory**: [expected impact]
- **Load Time**: [expected impact]
- **Network**: [expected impact, or "Not applicable"]

## Migration Plan

[How to move from current implementation to this decision. Write "None" if greenfield.]

## Validation Criteria

[Tests, metrics, review checks, runtime behavior, or acceptance criteria.]

## Sources Consulted

| Source | Purpose |
|--------|---------|
| [path] | [why it was read] |

## Related Decisions

- [ADR links]
- [GDD links]
- [Registry links]
```

---

## 8. Engine Specialist Validation

Skip in `solo`.

Run in `lean` and `full`.

Read `.claude/docs/technical-preferences.md` and use the primary specialist under `Engine Specialists`.

If no specialist is configured, skip and note:

```text
Engine specialist skipped — no primary specialist configured.
```

Spawn `Task` with:

- ADR draft content.
- Engine Compatibility section.
- Decision section.
- Key Interfaces section.
- Relevant engine reference paths.
- Domain.
- Engine version.

Ask the specialist to verify:

1. Idiomatic approach for the pinned engine version.
2. Deprecated APIs or changed patterns.
3. Post-cutoff engine changes.
4. Implementability of Key Interfaces.
5. Missing engine-specific risks.

If feedback requires material changes, revise once and rerun the specialist once. Do not loop indefinitely.

---

## 9. Technical Director Review

Run only in `full`.

Spawn `technical-director` using gate `TD-ADR` from:

```text
.claude/docs/director-gates.md
```

Pass:

- ADR draft content.
- Engine version.
- Domain.
- Existing ADRs in the same domain.
- Relevant registry stances.
- Known conflicts or scoped exceptions.

If TD returns `APPROVE`, continue.

If TD returns `CONCERNS`, revise once and continue.

If TD returns `REJECT`, stop and show blocking issues plus recommended next action. Do not save rejected ADRs unless `--dry-run` is active.

---

## 10. GDD Sync Check

Before writing, inspect every GDD referenced in `## GDD Requirements Addressed`.

Check naming inconsistencies with:

- Signals.
- Methods.
- Components.
- Data types.
- State names.
- API contracts.
- Performance budgets.

If inconsistencies are found, show:

```text
⚠️ GDD SYNC REQUIRED

[gdd-filename].md uses names or contracts that differ from this ADR:

  [old_name] → [new_name_from_adr]

Developers reading the GDD may implement the wrong interface unless the GDD is updated.
```

GDD updates are protected writes. Ask before applying them.

---

## 11. Write New ADR

Generate target path:

```text
docs/architecture/adr-[NNNN]-[slug].md
```

If `--dry-run` is active, show the draft and stop without writing.

If not dry-run:

1. Create `docs/architecture/` if needed.
2. Write the ADR file.
3. Do not ask for a separate confirmation for this new ADR write.
4. If the target file already exists, do not overwrite. Create the next safe filename or ask the user which file should be replaced.

---

## 12. Registry Update

Run only after the ADR is written. Skip in `--dry-run`.

Extract candidates from `## Registry Impact` first.

Show:

```text
Registry candidates from ADR-[NNNN]:

NEW state ownership:
  [state] → [owner]

NEW interface contract:
  [interface] → [contract]

NEW performance budget:
  [system] → [budget]

NEW API decision:
  [API/pattern] → [decision]

NEW forbidden pattern:
  [pattern] → [reason]

SUPERSEDES:
  [old entry] → [new entry]
```

Ask:

```text
May I update docs/registry/architecture.yaml with these registry entries?
[A] Yes — update registry
[B] Not yet — review candidates
[C] Skip registry update
```

Only proceed on `[A]`.

Registry write rules:

1. Read the current registry immediately before editing.
2. Append after the last existing entry in the correct section.
3. Do not assume placeholder arrays like `[]` still exist.
4. Do not overwrite unrelated entries.
5. Existing entries may be modified only when supersession was explicitly approved.

Status discipline:

- `Accepted` ADR → registry entries may be `active`.
- `Proposed` ADR → registry entries must be `proposed`.
- Proposed entries are not binding constraints for future ADRs.

---

## 13. Story Status Updates

Run only after the ADR is written. Skip in `--dry-run`.

Search stories for references to this ADR or `Status: Blocked`.

Rules:

- If ADR status is `Proposed`, do not mark stories `Ready`.
- If ADR status is `Accepted`, ask before changing story status.
- Never update stories automatically.

Ask:

```text
How should I handle these story statuses?
[A] Mark listed stories Ready
[B] Leave stories unchanged
[C] Review individually
```

Only update stories if explicitly approved.

---

## 14. Closing Next Steps

After the ADR is written and optional updates are complete, inspect:

```text
docs/registry/architecture.yaml
.claude/docs/technical-preferences.md
systems-index.md
```

If these files exist, identify remaining priority ADRs or prerequisite decisions.

Show:

```text
ADR-[NNNN] written.

Registry update:
[completed | skipped | not applicable]

GDD sync:
[completed | skipped | not applicable]

Story updates:
[completed | skipped | not applicable]

Next best action:
[one recommended action]
```

Always include:

```text
To validate ADR coverage against your GDDs, open a fresh Claude Code session and run:

/architecture-review

Never run /architecture-review in the same session as /architecture-decision.
The reviewing agent must be independent of the authoring context.
```
