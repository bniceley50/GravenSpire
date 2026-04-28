---
paths:
  - "assets/data/**"
---

# Data File Rules

## Rule Set Name

Data File Rules

## Mission

These rules govern all data files under:

```text
assets/data/**
```

Their purpose is to ensure project data is valid, schema-backed, consistently named, numerically explainable, reference-safe, versioned, migration-ready, build-safe, and maintainable.

Data files are part of the game’s source of truth. They drive gameplay, economy, AI, content, tuning, localization references, rewards, encounters, and other runtime or authoring systems. Data errors can break builds, corrupt balance, invalidate tests, or ship broken content.

The core data-file question is:

> Can this data be parsed, validated, understood, referenced, migrated, and used safely by code, tools, designers, and QA?

---

## Operating Principles

1. **Valid JSON only**
   - All `.json` files must be valid strict JSON.
   - Broken JSON blocks the build pipeline.
   - Comments are not allowed inside `.json` files.

2. **Explain numbers outside raw JSON**
   - Because strict JSON does not allow comments, numeric explanations must live in:
     - JSON Schema `description` fields,
     - companion Markdown documentation,
     - design documents,
     - sidecar metadata files,
     - or approved tooling documentation.
   - Do not insert comments into `.json`.

3. **Consistent file naming**
   - Data file names must be lowercase with underscores only.
   - File names must follow:

```text
[system]_[name].json
```

   - Example:

```text
combat_enemies.json
loot_goblin_common.json
ai_guard_behavior.json
```

4. **Consistent key naming**
   - Keys inside JSON files use `camelCase`.
   - Entry IDs should be stable and lowercase snake_case unless the schema explicitly defines another ID format.

5. **Every data file needs a schema**
   - Each data file must have:
     - JSON Schema, or
     - documented schema in the corresponding design document.
   - Schema location must be traceable.

6. **No orphaned data**
   - Every data entry must be referenced by code, another data file, a registry, a design document, a test fixture, or an approved generated artifact.
   - Orphan status must be explicit.

7. **Version breaking changes**
   - Breaking schema changes require versioning.
   - Migration behavior must be documented.
   - Old data must not be silently interpreted as new schema.

8. **Optional fields need defaults**
   - Every optional field must have a sensible default.
   - Defaults must be documented in schema or companion docs.
   - Defaults must not hide missing required design decisions.

9. **Data changes require rationale**
   - Balance values, economy numbers, damage values, probabilities, weights, timers, and other tuning fields must link to design rationale, formula, playtest, simulation, or owner decision.

10. **Generated data is not hand-authored**
    - Generated data must be marked as generated.
    - Do not manually edit generated files unless the generator workflow explicitly permits it.

11. **Self-healing before merge**
    - When JSON is invalid, schema is missing, keys are inconsistent, defaults are absent, or references are broken, stop, classify, repair, validate, and report.

12. **Bounded self-learning**
    - Durable lessons from data validation failures, schema migrations, orphan findings, build breaks, and user corrections may be stored only in reviewable locations.
    - Lessons must be explicit, reversible, and subordinate to current project rules and approved design decisions.

---

## Scope

These rules apply to files under:

```text
assets/data/**
```

This includes, where present:

- gameplay tuning data,
- combat data,
- enemy data,
- item data,
- loot tables,
- economy data,
- AI parameters,
- encounter data,
- progression data,
- crafting data,
- ability data,
- status effect data,
- reward data,
- registry-like data files,
- generated runtime data,
- authored content data,
- schema-adjacent sidecar docs.

---

## Non-Goals

These rules do not authorize:

- changing game design values without design approval,
- changing balance without rationale,
- changing schemas without migration review,
- installing validation dependencies,
- editing generated data manually,
- writing implementation code,
- bypassing build validation,
- treating invalid JSON as acceptable,
- file edits without the active agent’s approval workflow,
- persistent memory updates without approval.

---

## Data File State Labels

Use these labels when reviewing or authoring data files:

```text
PROPOSED — data file or entry suggested but not approved.
DRAFT — file exists but is incomplete or unvalidated.
VALID_JSON — strict JSON parse succeeds.
INVALID_JSON — parse failure.
SCHEMA_DOCUMENTED — schema exists and is traceable.
SCHEMA_MISSING — no schema found.
SCHEMA_VALIDATED — data validates against schema.
SCHEMA_FAILED — data fails schema validation.
REFERENCED — entry is referenced by an approved source.
ORPHANED — entry has no valid reference.
DEFAULTS_DOCUMENTED — optional defaults are documented.
DEFAULTS_MISSING — optional defaults are missing.
VERSIONED — schema/data version is recorded.
MIGRATION_REQUIRED — breaking change requires migration.
MIGRATION_DOCUMENTED — migration path is documented.
GENERATED — file is produced by a generator.
HAND_AUTHORED — file is manually authored.
BLOCKED — cannot proceed due to validation, schema, reference, or approval issue.
SUPERSEDED — replaced by newer data/schema.
DEPRECATED — still present but should not be used for new references.
```

### State Rules

- Do not mark data usable unless it is `VALID_JSON`.
- Do not mark schema-compliant unless it is `SCHEMA_VALIDATED`.
- Do not mark optional defaults complete unless they are `DEFAULTS_DOCUMENTED`.
- Do not mark entry complete unless it is `REFERENCED` or explicitly approved as reserved/future data.
- Do not mark breaking changes complete unless migration is documented or explicitly waived.

---

## Source of Truth

Recommended locations:

```text
assets/data/
assets/data/schemas/
assets/data/docs/
design/gdd/
design/registry/entities.yaml
docs/data/
production/qa/data/
```

### Source-of-Truth Rules

- Look for schema before editing data.
- Look for design doc before changing balance values.
- Look for registry entry before adding cross-system IDs.
- Look for references before deleting or renaming entries.
- If schema and data conflict, treat data as blocked until resolved.
- If design docs and data conflict, escalate to relevant owner.
- If code expects fields not in schema, update schema or code contract after approval.

---

## File Naming Standard

### Required Pattern

```text
[system]_[name].json
```

### Allowed Characters

```text
a-z
0-9
_
```

### Examples

Valid:

```text
combat_enemies.json
combat_status_effects.json
loot_goblin_common.json
economy_currency_sources.json
ai_guard_behavior.json
progression_level_curve.json
```

Invalid:

```text
EnemyData.json
combat-enemies.json
combat enemies.json
CombatEnemies.json
combat.enemies.json
```

### File Naming Record

```md
## Data File Naming Review

- File:
- Pattern:
- System:
- Name:
- Valid:
- Recommendation:
```

---

## JSON Validity Policy

### Strict JSON Rules

All `.json` files must:

- parse as strict JSON,
- use double quotes for strings and keys,
- contain no comments,
- contain no trailing commas,
- contain no unquoted keys,
- contain no `NaN`, `Infinity`, or non-JSON numeric values,
- use arrays/objects intentionally,
- be encoded consistently, preferably UTF-8.

### Invalid JSON Handling

If JSON is invalid:

1. Mark file `INVALID_JSON`.
2. Identify parse error.
3. Do not continue schema or orphan validation until parse is fixed.
4. Repair syntax.
5. Re-validate parse.
6. Report exact file and issue.

### JSON Validity Record

```md
## JSON Validity Check

- File:
- Parser/tool:
- Result:
  - PASS
  - FAIL
  - NOT_RUN
- Error:
- Evidence:
```

---

## Schema Policy

### Schema Requirement

Every data file must have a documented schema through one of:

```text
JSON Schema file
Design document schema section
Companion schema documentation
Approved generator schema
```

### Preferred Schema File Location

```text
assets/data/schemas/[system]_[name].schema.json
```

Example:

```text
assets/data/combat_enemies.json
assets/data/schemas/combat_enemies.schema.json
```

### Schema Record

```md
## Data Schema: [File / System]

- Data file:
- Schema file:
- Schema version:
- Owner:
- Design source:
- Required fields:
- Optional fields:
- Defaults:
- Field types:
- Safe ranges:
- ID format:
- Reference fields:
- Migration behavior:
- Validation tool:
```

### JSON Schema Requirements

Schema should define:

- `$schema`,
- `$id` where useful,
- `title`,
- `description`,
- `type`,
- `required`,
- `properties`,
- `additionalProperties` policy,
- field descriptions,
- numeric ranges,
- default values for optional fields,
- enum values where applicable,
- ID/key patterns,
- schema version.

### Schema Validation Record

```md
## Schema Validation

- Data file:
- Schema:
- Schema version:
- Result:
  - PASS
  - FAIL
  - NOT_RUN
- Errors:
- Evidence:
```

---

## Key Naming Policy

### JSON Object Keys

Within JSON files:

```text
camelCase
```

Examples:

```json
{
  "baseHealth": 50,
  "baseDamage": 8,
  "moveSpeed": 3.5,
  "lootTable": "loot_goblin_common"
}
```

### Entry IDs

Top-level entry IDs should be stable IDs. Default style:

```text
lowercase_snake_case
```

Examples:

```json
{
  "goblin": {},
  "goblin_chief": {},
  "forest_bandit_elite": {}
}
```

### Key Naming Exceptions

Exceptions require schema documentation.

Possible exceptions:

- external vendor data format,
- generated data,
- localization keys,
- platform-required field names,
- imported third-party JSON,
- legacy migration data.

### Key Naming Review

```md
## Key Naming Review

- File:
- Key:
- Location:
- Expected style:
- Actual style:
- Exception documented:
- Verdict:
```

---

## Numeric Value Documentation

### Numeric Provenance Requirement

Every numeric value that affects gameplay, economy, AI, progression, rewards, timing, or balance must have a documented meaning.

Documentation may live in:

- JSON Schema `description`,
- corresponding GDD,
- companion Markdown file,
- data dictionary,
- tuning guide,
- balance model,
- simulation report.

### Numeric Value Record

```md
## Numeric Value Record

- File:
- Entry:
- Field:
- Value:
- Unit:
- Meaning:
- Safe range:
- Default:
- Source:
  - Formula
  - Design rationale
  - Playtest
  - Simulation
  - Owner decision
  - Placeholder
- Review trigger:
```

### Numeric Rules

- Define units where applicable.
- Define safe range.
- Define whether value is tunable.
- Define whether value is balance-critical.
- Placeholder numbers must be labeled.
- Do not use unexplained magic numbers.
- Do not hide numeric meaning in code only.

---

## Optional Defaults Policy

### Default Record

```md
## Optional Field Default

- Field:
- Type:
- Default:
- Reason:
- Applies when:
- Safe range:
- Runtime behavior:
- Schema location:
```

### Default Rules

- Every optional field must have a documented default.
- Defaults should be safe and conservative.
- Defaults must not conceal missing required data.
- Defaults must be consistent across schema, loader, and design docs.
- Runtime fallback behavior must match schema defaults.
- Changing a default may be a balance or compatibility change and requires review.

---

## Referential Integrity and Orphan Policy

### Valid Reference Sources

A data entry is not orphaned if referenced by at least one approved source:

- runtime code,
- another data file,
- entity registry,
- design document,
- schema,
- test fixture,
- generated artifact,
- approved future/reserved-data list.

### Reference Record

```md
## Data Reference Record

- Data file:
- Entry ID:
- Referenced by:
- Reference type:
  - Code
  - Data
  - Registry
  - Design Doc
  - Test
  - Generated Artifact
  - Reserved
- Evidence:
- Status:
```

### Orphan Review

```md
## Orphan Data Review

| File | Entry ID | References Found | Status | Recommendation |
|---|---|---|---|---|
```

### Orphan Rules

- Orphaned production entries are not allowed.
- Reserved future entries must be explicitly labeled with owner and review trigger.
- Test-only entries must live in test data or be labeled test-only.
- Deleting an entry requires checking all references first.
- Renaming an ID is a breaking reference change unless migration is provided.

---

## Schema Versioning and Migration

### Data Versioning

Breaking schema changes require a version field or schema version association.

Recommended field:

```json
{
  "schemaVersion": 1,
  "entries": {}
}
```

For files where a wrapper object is not appropriate, versioning may be documented in the companion schema file.

### Breaking Changes Include

- removing required field,
- renaming field,
- changing field type,
- changing ID format,
- changing default behavior,
- changing required reference semantics,
- changing units,
- changing value interpretation,
- changing top-level structure,
- changing enum values in a way that breaks existing data.

### Migration Record

```md
## Data Migration Record

- Data file:
- Old schema version:
- New schema version:
- Breaking changes:
- Migration method:
- Backward compatibility:
- Affected references:
- Validation:
- Owner:
```

### Versioning Rules

- Do not silently reinterpret old data.
- Migration must be documented before breaking schema changes ship.
- If migration script/tool exists, document command and validation evidence.
- If manual migration is required, document exact steps.
- Old schema may be deprecated but should remain traceable until all data is migrated.

---

## Generated Data Policy

### Generated Data Marker

Generated files should include either:

- a top-level metadata field if schema allows,
- a companion metadata file,
- or repository convention documenting generated status.

Example:

```json
{
  "schemaVersion": 1,
  "generated": true,
  "generator": "tools/data/build_loot_tables",
  "entries": {}
}
```

### Generated Data Rules

- Do not manually edit generated data.
- Generated files must identify generator source.
- Generated output must be deterministic where possible.
- Generated output must validate against schema.
- Generator changes may require data migration.
- Generated data should not contain stale entries.

---

## Data Review Format

Use this for data file review:

```md
## Data File Review: [File]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### JSON Validity

### File Naming

### Schema Status

### Key Naming

### Numeric Documentation

### Optional Defaults

### Reference / Orphan Status

### Version / Migration Status

### Required Follow-Up
```

### Severity

```text
DATA-S1 — Critical
Invalid JSON, build-blocking parse failure, schema-breaking change without migration, or corrupted generated data.

DATA-S2 — High
Missing schema, orphaned production entry, missing required field, invalid reference, or unexplained balance-critical number.

DATA-S3 — Medium
Key naming inconsistency, missing default documentation, weak numeric rationale, incomplete companion docs.

DATA-S4 — Low
Formatting, naming polish, non-blocking documentation cleanup.
```

---

## Data Validation Report

```md
## Data Validation Report

- Scope:
- Date/session:
- Validator:
- Files checked:
- JSON validity:
- Schema validation:
- Naming validation:
- Reference validation:
- Version validation:
- Numeric documentation validation:
- Verdict:
- Blockers:
- Warnings:
- Required fixes:
```

### Validation Verdicts

```text
DATA_VALIDATION_PASS
DATA_VALIDATION_PASS_WITH_WARNINGS
DATA_VALIDATION_BLOCKED
DATA_VALIDATION_UNKNOWN
```

---

## Self-Learning Protocol

Self-learning means controlled improvement from data validation failures, schema migrations, build breaks, orphan reviews, QA findings, implementation feedback, and user corrections.

It does not mean hidden memory updates, automatic schema changes, or turning one-off data mistakes into global policy.

### What May Be Learned

The data-file rule system may learn:

- approved data file naming conventions,
- approved schema locations,
- approved key naming exceptions,
- known invalid JSON failure modes,
- numeric documentation patterns,
- default value conventions,
- schema migration patterns,
- common orphan causes,
- reference integrity findings,
- generated-data conventions,
- rejected data structures and why.

### What Must Not Be Learned or Stored

Do not store:

- private user data,
- private chain-of-thought,
- secrets or credentials,
- sensitive logs,
- unapproved placeholder values as balance rules,
- speculative data entries as approved content,
- one-off validation failures as universal rules without evidence,
- generated output as source-of-truth unless approved.

### Lesson Classification

Use:

```text
Confirmed Rule
Approved Data Standard
Naming Finding
Schema Finding
JSON Validity Finding
Key Naming Finding
Numeric Documentation Finding
Default Value Finding
Reference Integrity Finding
Orphan Finding
Versioning Finding
Migration Finding
Generated Data Finding
Build Pipeline Finding
QA Finding
Implementation Feedback
Rejected Approach
Working Assumption
Temporary Context
Superseded
```

### Lesson Storage

Store durable lessons only in approved, reviewable locations such as:

```text
docs/data/data-file-standards.md
docs/data/schema-lessons.md
docs/data/migration-lessons.md
docs/data/reference-integrity.md
tasks/lessons.md
production/qa/data/
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
- it applies to data files,
- it does not include sensitive data,
- it is not overgeneralized,
- it does not conflict with design docs or schema authority,
- it has a review trigger where appropriate.

### Lesson Expiry

Review or expire lessons when:

- schema standards change,
- data directory structure changes,
- validation tooling changes,
- build pipeline changes,
- design docs change,
- entity registry changes,
- migration evidence contradicts the lesson,
- owner supersedes it,
- the lesson was temporary,
- the lesson is too broad.

---

## Self-Healing Protocol

Self-healing means detecting a data-file failure, containing the risk, repairing safely, verifying the repair, and reporting what changed.

### Failure Types

Monitor for:

- invalid JSON,
- wrong file name,
- wrong key style,
- missing schema,
- schema validation failure,
- undocumented numeric value,
- missing optional default,
- orphaned data entry,
- invalid reference,
- renamed ID without migration,
- breaking schema change without version,
- generated file manually edited,
- stale generated data,
- design/data mismatch,
- code/schema mismatch,
- placeholder value treated as final.

### Recovery Loop

When failure occurs:

1. **Stop**
   - Do not mark data valid or build-ready.

2. **Identify**
   - State the exact data failure.

3. **Classify**
   - JSON, naming, schema, key style, numeric documentation, default, reference, orphan, versioning, migration, generated-data, or design mismatch.

4. **Contain**
   - Mark status:
     - `INVALID_JSON`,
     - `SCHEMA_MISSING`,
     - `SCHEMA_FAILED`,
     - `ORPHANED`,
     - `MIGRATION_REQUIRED`,
     - `BLOCKED`.

5. **Recover**
   - fix JSON syntax,
   - rename file after reference review,
   - correct key names or document exception,
   - add or link schema,
   - document numeric meaning,
   - add optional defaults,
   - update references,
   - mark reserved/test-only entries,
   - version schema,
   - write migration record,
   - regenerate generated data.

6. **Verify**
   - Re-run parse validation.
   - Re-run schema validation.
   - Re-run reference/orphan check.
   - Re-check defaults and numeric documentation.

7. **Report**
   - Summarize issue, fix, remaining risk, and owner.

8. **Learn**
   - Propose durable lesson only if validated and approved.

---

## Error Recovery

### Invalid JSON

If a JSON file fails to parse:

- mark `INVALID_JSON`,
- fix syntax,
- do not run downstream validation until parse passes,
- add validation evidence.

### Comments in JSON

If comments appear in `.json`:

- remove comments,
- move explanation to schema `description`, sidecar docs, or GDD,
- re-validate strict JSON.

### Wrong File Name

If file naming violates pattern:

- propose valid lowercase underscore name,
- check references before renaming,
- update references after approval,
- treat rename as potentially breaking.

### Missing Schema

If schema is missing:

- add JSON Schema or document schema in corresponding design doc,
- identify owner,
- mark data `SCHEMA_MISSING` until complete.

### Schema Failure

If data fails schema validation:

- identify field path,
- identify expected type/range/enum,
- correct data or update schema after owner approval,
- re-run validation.

### Unexplained Numeric Value

If numeric value lacks meaning:

- add schema description or companion doc,
- define unit, safe range, rationale, and owner,
- flag design review for balance-critical values.

### Missing Optional Default

If optional field lacks default:

- define default in schema/docs,
- verify runtime loader uses the same default,
- add test if available.

### Orphaned Entry

If entry has no reference:

- search code, data, registry, design docs, and tests,
- if unused, remove or mark reserved/test-only after approval,
- do not leave production orphan unclassified.

### Breaking Schema Change

If schema change is breaking:

- increment schema version,
- document migration,
- update affected data,
- update tests/validators,
- block merge until migration is clear.

### Generated File Edited

If generated file was hand-edited:

- mark blocked,
- identify generator,
- apply change to source/generator instead,
- regenerate,
- validate output.

---

## Memory Policy

### Short-Term Task Memory

Track during current task:

- data file path,
- schema path,
- schema version,
- parse status,
- validation status,
- key naming issues,
- numeric documentation gaps,
- optional defaults,
- references,
- orphan status,
- migration needs,
- approvals needed.

Short-term memory expires after task completion unless explicitly stored.

### Project Memory

Project memory may store:

- approved data naming conventions,
- schema locations,
- key naming exceptions,
- numeric documentation patterns,
- default value conventions,
- migration patterns,
- common orphan causes,
- generated data rules,
- validation findings,
- rejected approaches.

### Never Store

Never store:

- secrets,
- credentials,
- private user data,
- private chain-of-thought,
- sensitive logs,
- unapproved placeholder balance values,
- speculative data entries as approved facts,
- unsupported validation claims.

---

## Feedback Policy

When the user, Game Designer, Systems Designer, Economy Designer, Gameplay Programmer, Tools Programmer, QA Lead, Technical Director, or Build/DevOps owner corrects data-file behavior:

1. Accept the correction.
2. Identify whether it affects:
   - JSON validity,
   - file naming,
   - schema,
   - key naming,
   - numeric documentation,
   - defaults,
   - references,
   - versioning,
   - migration,
   - generated data,
   - validation.
3. Revise current output.
4. Ask whether the correction should become durable data-file guidance if reusable.
5. Store only if approved and evidence-backed.

---

## Tool-Use Policy

This rules file does not grant tools by itself. Agents applying it must follow their own tool permissions.

General guidance:

- Use file-reading tools to inspect data files, schemas, companion docs, design docs, registries, tests, and generated metadata.
- Use search tools to find references to entry IDs, file names, schema versions, and data keys.
- Use write/edit tools only after approval under the active agent’s workflow.
- Use Bash only if the active agent allows it and only under that agent’s safety policy.
- Do not run validators, generators, builds, or file mutations without required approval.
- Do not use Bash to bypass write/edit approval.

---

## Safety Guardrails

Never allow production data under `assets/data/**` to:

- contain invalid JSON,
- contain comments in `.json`,
- use inconsistent file naming,
- lack a documented schema,
- contain unexplained balance-critical numbers,
- mix key naming styles without documented exception,
- contain orphaned production entries,
- make breaking schema changes without versioning/migration,
- lack defaults for optional fields,
- manually edit generated data without generator update,
- claim validation without evidence,
- store secrets or credentials.

---

## Output Standards

Data-file reviews should be:

- parse-aware,
- schema-aware,
- naming-aware,
- reference-aware,
- version-aware,
- migration-aware,
- numeric-rationale-aware,
- explicit about validation evidence,
- clear about unresolved issues.

### Review Output Format

```md
## Data File Review: [File]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### JSON Validity

### Schema Status

### Naming Status

### Key Naming Status

### Numeric Documentation

### Defaults

### References / Orphans

### Versioning / Migration

### Required Follow-Up
```

---

## Reflection Checklist

After reviewing or drafting data files, privately check:

- Is the file valid strict JSON?
- Does the file name match `[system]_[name].json`?
- Are keys camelCase where required?
- Are entry IDs stable and consistently styled?
- Is there a schema?
- Does data validate against schema?
- Are numeric values documented?
- Are optional defaults documented?
- Are entries referenced?
- Are breaking changes versioned?
- Is migration documented?
- Is generated data clearly marked?
- Did I avoid storing unapproved lessons?

Do not expose private chain-of-thought. Report only conclusions, evidence, and recommendations.

---

## Evaluation Checklist

Before final approval of a data file:

### JSON Validity

- [ ] Strict JSON parse succeeds.
- [ ] No comments.
- [ ] No trailing commas.
- [ ] No invalid numeric values.
- [ ] Encoding is acceptable.

### Naming

- [ ] File name is lowercase underscore.
- [ ] File name follows `[system]_[name].json`.
- [ ] Keys use camelCase.
- [ ] Entry IDs follow approved ID format.

### Schema

- [ ] Schema exists or is documented in design doc.
- [ ] Schema version is defined.
- [ ] Required fields are listed.
- [ ] Optional defaults are listed.
- [ ] Types and ranges are defined.
- [ ] Data validates against schema.

### Numeric Values

- [ ] Numeric values have meaning documented.
- [ ] Units are defined where applicable.
- [ ] Safe ranges are defined.
- [ ] Balance values have rationale/source.
- [ ] Placeholder values are labeled.

### References

- [ ] Entries are referenced by code/data/registry/design/test or marked reserved.
- [ ] No orphaned production entries.
- [ ] Renamed IDs have migration/reference update plan.
- [ ] Cross-file references are valid.

### Versioning and Migration

- [ ] Breaking changes are versioned.
- [ ] Migration path is documented.
- [ ] Old schema compatibility is addressed.
- [ ] Tests or validation are updated where relevant.

### Safety

- [ ] No secrets or credentials in data.
- [ ] No generated data manually edited.
- [ ] Validation evidence exists or missing evidence is stated.
- [ ] File edits require approval.

---

## Example Workflows

### Example 1: Invalid JSON

Issue:

```json
{
  "goblin": {
    "baseHealth": 50,
  }
}
```

Response:

```md
Finding: Invalid JSON.

Reason:
Trailing comma after `baseHealth` makes this invalid strict JSON and can block the build pipeline.

Fix:
Remove the trailing comma and re-run JSON validation.
```

---

### Example 2: Comments in JSON

Issue:

```json
{
  "baseDamage": 8 // starter enemy damage
}
```

Response:

```md
Finding: Comments are not allowed in strict JSON.

Fix:
Move explanation to schema description or companion docs. Keep JSON valid:

{
  "baseDamage": 8
}
```

---

### Example 3: Bad File Name

Issue:

```text
EnemyData.json
```

Response:

```md
Finding: File naming violation.

Fix:
Rename to a lowercase underscore name following `[system]_[name].json`, such as:

combat_enemies.json
```

---

### Example 4: Missing Schema

Issue:

```text
assets/data/combat_enemies.json has no JSON Schema or design-doc schema section.
```

Response:

```md
Finding: Schema missing.

Fix:
Add `assets/data/schemas/combat_enemies.schema.json` or document the schema in the combat design doc before treating this data as production-ready.
```

---

### Example 5: Unexplained Numeric Value

Issue:

```json
{
  "goblin": {
    "baseHealth": 50
  }
}
```

Response:

```md
Finding: Numeric value lacks provenance.

Fix:
Document:
- unit: hit points,
- safe range,
- rationale,
- design source,
- whether this is a placeholder or approved balance value.
```

---

### Example 6: Missing Optional Default

Issue:

```json
{
  "goblin": {
    "baseHealth": 50,
    "baseDamage": 8
  }
}
```

Schema has optional `armorRating` but no default.

Response:

```md
Finding: Optional field default missing.

Fix:
Document default, such as `armorRating = 0`, in schema and verify runtime loader uses that same default.
```

---

### Example 7: Orphaned Entry

Issue:

```json
{
  "unused_enemy_test": {
    "baseHealth": 999
  }
}
```

No code, data, registry, test, or design reference exists.

Response:

```md
Finding: Orphaned production data entry.

Fix:
Remove it, move it to test data, or mark it as reserved with owner and review trigger.
```

---

### Example 8: Breaking Schema Change

Issue:

```text
`baseHealth` renamed to `health` in existing production data.
```

Response:

```md
Finding: Breaking schema change.

Fix:
Increment schema version, document migration from `baseHealth` to `health`, update references/tests, and avoid silently reinterpreting old data.
```

---

### Example 9: User Correction

User says:

```text
Top-level entry IDs should use lowercase snake_case, but nested JSON keys stay camelCase.
```

Response pattern:

```text
Understood. I’ll treat top-level entry IDs as lowercase snake_case and nested JSON fields as camelCase. Store this as a durable data-file rule only if approved.
```

---

### Example 10: Case Where the Rule Must Not Learn

User says during prototyping:

```text
Just put test enemies in combat_enemies.json for now.
```

Response pattern:

```text
That can be treated as prototype-only if clearly labeled, but production data under `assets/data/**` still cannot contain orphaned or unexplained entries. Test enemies should live in test data or be marked reserved/test-only with a review trigger.
```

---

## Final Data File Rule

Data files under `assets/data/**` must be:

- valid strict JSON,
- consistently named,
- schema-documented,
- camelCase internally,
- numerically explainable,
- reference-safe,
- default-aware,
- versioned for breaking changes,
- migration-ready,
- validated before build,
- and honest about unresolved data assumptions.