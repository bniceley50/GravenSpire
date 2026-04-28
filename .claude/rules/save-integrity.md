---
paths:
  - "src/core/save/**"
  - "Assets/Scripts/Core/Save/**"
  - "Assets/Scripts/Persistence/**"
---

# Save Integrity Rules

## Rule Set Name

Save Integrity Rules

## Mission

These rules govern save-file integrity, versioning, migration, validation, and recovery for:

```text
src/core/save/**
Assets/Scripts/Core/Save/**
Assets/Scripts/Persistence/**
```

Their purpose is to ensure save data is tamper-evident, version-safe, migration-tested, privacy-aware, recoverable where possible, and never silently trusted when integrity or compatibility is unknown.

Save files are player-facing persistence artifacts and bug-report evidence. A tampered save can look like a real bug. A corrupt save can destroy player trust. A local save that later syncs to a server can become a security incident.

The core save-integrity question is:

> Can this save be authenticated, version-checked, validated, migrated, loaded, rejected, backed up, or synced without silently corrupting gameplay state or hiding tampering?

---

## Active Tier

```text
Active tier: T1+
```

Save integrity matters from day one.

Reasons:

- Playtesters may share save files to reproduce bugs.
- Community save-editor posts are expected.
- Local tampering that later syncs to a server at T3+ can become a server-side integrity incident.
- Migration mistakes compound over time if early versions are not preserved and tested.

---

## Operating Principles

1. **Tamper evidence is mandatory**
   - Every production save file must carry an HMAC signature.
   - Signature mismatch rejects the save loudly.
   - Missing signature rejects the save unless explicitly classified as pre-integrity legacy data with an approved migration path.

2. **HMAC before gameplay-state deserialization**
   - The loader may read only bounded envelope/header metadata required to locate version and signature fields.
   - The loader must not deserialize or materialize gameplay state until HMAC verification passes.

3. **Derived per-install key**
   - The HMAC key is derived, not stored in plaintext next to the save file.
   - The derivation strategy is a T1 design decision and must be documented in the save-system GDD or security design record.

4. **Local integrity is not server authority**
   - HMAC makes local tampering detectable.
   - HMAC does not make local save data authoritative for T3+ server state.
   - Server sync must revalidate consequential state.

5. **Version first**
   - Save format carries a version stamp as the first field or first bounded header value.
   - Loader checks version before attempting to parse later gameplay state.

6. **Fail loud**
   - Version mismatch, HMAC mismatch, malformed envelope, failed migration, and invalid required fields must fail loudly.
   - Never silently fall through to default state.

7. **No partial load**
   - Older loader + newer save: reject with version-mismatch error.
   - Newer loader + older save: migrate if a complete migration path exists.
   - If migration is unavailable or fails, reject. Never partial-load.

8. **Migrations are forward-only**
   - Never edit prior-version deserialization paths after release except under explicit security/critical bug review.
   - Add new migrations from old version to current version.
   - Preserve canonical prior-version saves.

9. **Every version bump needs a real fixture test**
   - Every version bump ships with a migration test:

```text
real prior-version save -> current loader -> expected current-version state
```

10. **Canonical save fixtures are protected evidence**
    - Store canonical test saves in:

```text
tests/fixtures/saves/v[N]/
```

    - Do not casually regenerate them.
    - Changes require explanation.

11. **Only save reproducible persistent state**
    - Save gameplay state that must persist.
    - Do not save engine internals, derived/cache values, runtime-only handles, connection state, temp paths, or non-reproducible state.

12. **Player-authored strings are bounded and sanitized**
    - Character names and similar fields must be length-capped and sanitized on load.
    - Strip control characters.
    - Reject or normalize invalid text according to policy.

13. **Atomic writes**
    - Save writes must not leave half-written saves as the active slot.
    - Use temp-write, verify, then atomic replace where the platform supports it.

14. **Self-healing before recovery**
    - If a save is invalid, tampered, corrupt, partially written, or migration-blocked, contain the issue, preserve safe evidence, avoid mutation, and recover only through approved paths.

15. **Bounded self-learning**
    - Lessons from migration failures, tamper reports, corruption incidents, QA findings, and user corrections may be stored only in approved, reviewable locations.
    - Lessons must not store secrets, keys, raw private saves, or exploit details.

---

## Scope

These rules apply to:

- save serializers,
- save deserializers,
- save headers/envelopes,
- HMAC signing,
- HMAC verification,
- per-install key derivation,
- save file versioning,
- save migrations,
- migration tests,
- canonical fixture saves,
- save validation,
- player-authored string sanitation,
- save backups,
- save slot metadata,
- cloud-save preparation,
- server-sync preparation,
- save failure logging,
- corruption/tamper incident handling.

---

## Non-Goals

These rules do not authorize save code to:

- treat local saves as server-authoritative,
- store secrets or credentials in saves,
- store the HMAC key next to the save,
- silently reset tampered saves,
- partially load incompatible saves,
- alter prior-version deserialization paths casually,
- skip migration tests,
- sync unvalidated local state to server,
- expose private player data in logs,
- guarantee confidentiality unless encryption is separately implemented,
- edit files without the active agent’s approval workflow,
- store persistent lessons without approval.

---

## Save Integrity State Labels

Use these labels when reviewing or implementing save behavior:

```text
PROPOSED — save behavior suggested but not approved.
SPEC_READY — save format/signature/migration behavior documented.
IMPLEMENTED — code exists.
HEADER_READ_ONLY — loader reads only bounded metadata before verification.
HMAC_REQUIRED — save requires HMAC signature.
HMAC_VERIFIED — signature verified before gameplay-state deserialization.
HMAC_FAILED — signature mismatch or missing signature.
KEY_DERIVATION_DOCUMENTED — derivation strategy is documented and approved.
KEY_UNAVAILABLE — derived key cannot be obtained.
VERSION_PRESENT — version stamp exists as first field/header value.
VERSION_CHECKED — loader checks version before gameplay-state deserialization.
VERSION_MISMATCH — save version incompatible with loader.
MIGRATION_REQUIRED — save version older than current and requires migration.
MIGRATION_AVAILABLE — complete migration path exists.
MIGRATION_TESTED — real prior-version fixture test passes.
MIGRATION_FAILED — migration failed or produced invalid state.
FIXTURE_READY — canonical fixture exists for version.
FIXTURE_MISSING — required canonical fixture is missing.
SAVE_VALIDATED — field-level validation passed.
SAVE_REJECTED — save rejected loudly.
SAVE_QUARANTINED — invalid save preserved safely for review.
BACKUP_AVAILABLE — prior safe save exists.
ATOMIC_WRITE_VERIFIED — write path prevents active partial saves.
SERVER_SYNC_BLOCKED — local save cannot sync until server validation passes.
BLOCKED — missing HMAC, version, migration, validation, fixture, or decision.
SUPERSEDED — replaced by newer save format or rule.
DEPRECATED — still supported only for migration.
```

### State Rules

- Do not mark `HMAC_VERIFIED` unless signature comparison passed.
- Do not mark `VERSION_CHECKED` unless version is checked before gameplay-state deserialization.
- Do not mark `MIGRATION_TESTED` without fixture-based test evidence.
- Do not mark `ATOMIC_WRITE_VERIFIED` without write-path evidence.
- Do not mark `SAVE_VALIDATED` without field-level validation.
- `IMPLEMENTED` is not equivalent to safe.

---

## Source of Truth

Recommended project files:

```text
design/gdd/save-system.md
design/security/save-integrity.md
docs/security/save-integrity.md
docs/persistence/save-format.md
src/core/save/
Assets/Scripts/Core/Save/
Assets/Scripts/Persistence/
tests/fixtures/saves/v[N]/
tests/unit/save/
tests/integration/save/
production/qa/save/
SECURITY.md
RED_TEAM.md
AGENTS.md
```

### Source-of-Truth Rules

- Check the save-system GDD before changing saved fields.
- Check `SECURITY.md` before changing integrity, privacy, or key handling.
- Check `RED_TEAM.md` before claiming save-integrity readiness.
- Check fixture directories before changing migrations.
- If design docs, save schema, and code conflict, mark the save system `BLOCKED`.
- If key derivation is not documented, mark `KEY_DERIVATION_DOCUMENTED` as false.

---

## Save Envelope Standard

### Required Envelope Goals

A save envelope must support:

- version-first compatibility check,
- bounded metadata read,
- signature verification,
- format identification,
- safe rejection,
- migration routing,
- future extension.

### Example Conceptual Envelope

For JSON-like formats:

```json
{
  "version": 3,
  "format": "game_save",
  "schemaVersion": 3,
  "createdUtc": "2026-04-28T00:00:00Z",
  "payload": {
    "inventory": {},
    "questProgress": {},
    "reputation": {},
    "unlocks": {},
    "seeds": {}
  },
  "signature": {
    "algorithm": "HMAC-SHA-256",
    "keyId": "install",
    "value": "[base64]"
  }
}
```

For binary formats:

```text
[version][format_id][metadata_length][bounded_metadata][payload_length][payload][signature]
```

### Envelope Rules

- Version must be first field or first bounded header value.
- Signature metadata may be read before gameplay-state deserialization.
- Payload must not be materialized into gameplay objects until signature passes.
- Header and payload sizes must be bounded.
- Unknown format ID rejects loudly.
- Unknown required metadata rejects loudly.
- Signature field is excluded from the signed bytes.
- Signed bytes must be explicitly defined and stable.

---

## HMAC Signature Policy

### HMAC Requirements

Every production save must include:

- algorithm,
- key derivation reference,
- signed data definition,
- signature value,
- signature version if policy changes.

Use an approved HMAC algorithm such as HMAC-SHA-256 unless the project security spec chooses another.

### HMAC Record

```md
## Save HMAC Policy

- Save format:
- Algorithm:
- Key source:
- Key derivation:
- Signed fields/bytes:
- Excluded fields:
- Canonicalization:
- Comparison:
- Failure behavior:
- Tests:
```

### HMAC Rules

- HMAC covers the gameplay payload and integrity-relevant metadata.
- HMAC excludes the signature value itself.
- HMAC verification occurs before gameplay-state deserialization.
- Signature comparison must use constant-time comparison where available.
- Missing signature is a loud failure unless explicitly handled as legacy migration.
- Invalid signature is a loud failure.
- Do not display cryptographic details to players.
- Do not log raw signature values unless explicitly approved for secure diagnostics.
- HMAC provides tamper evidence, not encryption.

---

## Canonical Serialization

HMAC requires stable bytes.

### Canonicalization Record

```md
## Save Canonicalization Policy

- Format:
- Field ordering:
- Whitespace handling:
- Numeric formatting:
- String encoding:
- Date/time format:
- Null/default handling:
- Signature exclusion:
- Tests:
```

### Canonicalization Rules

- Define exact bytes used for HMAC.
- JSON-like formats must define canonical field order or sign original serialized bytes.
- Numeric formats must be stable.
- Strings must use consistent encoding, preferably UTF-8.
- Date/time values must use a stable format.
- Do not rely on unspecified dictionary iteration order.
- Canonicalization changes are save-format changes and may require versioning.

---

## Per-Install Key Derivation

### Key Derivation Requirement

The HMAC key is derived, not stored plaintext next to the save file.

The derivation strategy is a T1 design decision and must be documented.

### Key Derivation Record

```md
## Save Key Derivation Decision

- Status:
- Owner:
- Platform(s):
- Key source:
- Derivation method:
- Salt/nonce policy:
- Storage boundary:
- Reinstall behavior:
- Device migration behavior:
- Cloud-save behavior:
- Rotation behavior:
- Failure behavior:
- Security review:
```

### Key Rules

- Do not hardcode the HMAC key.
- Do not store key material next to save files.
- Prefer platform-protected storage where available.
- If install-specific key changes, old local saves may become unreadable unless migration/recovery is designed.
- Cloud saves need explicit portability policy.
- Server sync cannot trust local HMAC as server authority.
- Key derivation failure must fail loudly and preserve player-safe recovery options where available.

---

## Load Path Standard

### Load Sequence

```text
1. Open save file safely.
2. Bound file size.
3. Read version/header metadata only.
4. Check format/version compatibility.
5. Locate signature metadata.
6. Derive or obtain HMAC key.
7. Verify HMAC over defined signed bytes.
8. If HMAC fails, reject loudly and do not deserialize gameplay state.
9. Deserialize payload into neutral data object.
10. Validate fields and ranges.
11. If old version, run migration.
12. Validate migrated state.
13. Commit loaded state into runtime only after full validation.
```

### Load Path Record

```md
## Save Load Path Review

- Save format:
- Header read bounded:
- Version checked before payload:
- HMAC checked before gameplay deserialization:
- Field validation:
- Migration path:
- Commit point:
- Failure behavior:
- Tests:
```

### Load Rules

- Do not instantiate gameplay objects before integrity passes.
- Do not partially load fields.
- Do not silently default missing required fields.
- Do not commit runtime state until validation completes.
- Do not mutate original save during load.
- Invalid save remains outside active runtime state.

---

## Save Write Standard

### Atomic Save Sequence

```text
1. Build save payload from approved persistent state.
2. Validate payload before serialization.
3. Serialize deterministically.
4. Compute HMAC over signed bytes.
5. Write to temporary file.
6. Flush/close according to platform capability.
7. Verify temp file if practical.
8. Atomically replace active save.
9. Preserve previous backup if policy requires.
```

### Save Write Record

```md
## Save Write Path Review

- Save slot:
- Payload source:
- Validation before write:
- Serialization:
- HMAC generation:
- Temporary file:
- Atomic replace:
- Backup:
- Failure behavior:
- Tests:
```

### Write Rules

- Never write unsigned active saves.
- Never expose half-written saves as active saves.
- Do not overwrite last known good save until replacement is complete.
- Save write failure must not corrupt the previous valid save.
- Derived/cached fields must not be written unless explicitly allowed.

---

## Versioning Policy

### Required Version Behavior

- Version stamp is the first field or first bounded header value.
- Loader checks version before attempting to deserialize subsequent gameplay fields.
- Newer loader + older save:
  - attempt migration path;
  - if unavailable, fail loud;
  - never partial-load.
- Older loader + newer save:
  - reject with version-mismatch error;
  - never partial-load.

### Version Record

```md
## Save Format Version

- Version:
- Introduced in:
- Schema summary:
- Required fields:
- Optional fields:
- Removed fields:
- Changed fields:
- Migration from previous:
- Migration to current:
- Fixture path:
- Tests:
```

### Version Rules

- Version changes when field meaning, structure, type, requiredness, or canonicalization changes.
- Removing a field is a versioned change.
- Renaming a field is a versioned change.
- Changing units is a versioned change.
- Changing default behavior may be a versioned change.
- Save versioning is separate from game build version, though both may be recorded.

---

## Migration Policy

### Migration Requirements

Every version bump ships with:

```text
real prior-version save -> current loader -> expected current-version state
```

Canonical test saves live in:

```text
tests/fixtures/saves/v[N]/
```

### Migration Record

```md
## Save Migration: v[Old] -> v[New]

- Old version:
- New version:
- Reason:
- Added fields:
- Removed fields:
- Renamed fields:
- Type changes:
- Defaulted fields:
- Data transformations:
- Failure behavior:
- Fixture path:
- Expected state:
- Test path:
- Owner:
```

### Migration Rules

- Migrations are forward-only.
- Never edit prior versions’ deserialization paths casually.
- Add migration steps instead of rewriting history.
- Migrations must not partial-load.
- Migrations must validate output.
- Migration failure preserves original save.
- Migration should write a current-version save only after successful validation.
- Migration code should be deterministic.

---

## Canonical Fixture Saves

### Fixture Directory

```text
tests/fixtures/saves/v[N]/
```

### Fixture Record

```md
## Canonical Save Fixture

- Version:
- Path:
- Scenario:
- Contains:
- Expected migrated state:
- Created from:
- HMAC status:
- Owner:
- Review trigger:
```

### Fixture Rules

- Fixtures are real prior-version saves.
- Fixtures must not contain private player data.
- Fixtures must not contain secrets.
- Fixtures should include representative gameplay state.
- Edge-case fixtures are required for important migrations.
- Do not regenerate fixtures casually.
- If fixture must change, document why.

---

## Save Content Policy

### Allowed Save Data

Save persistent gameplay state such as:

```text
inventory
quest progress
reputation
unlocks
seeds
player-authored names/labels
persistent world flags
settings that belong in save scope
progression milestones
discovered locations
crafted/owned items
persistent choices
```

### Prohibited Save Data

Do not save:

```text
engine internals
derived values
cached values
network connections
file handles
temporary paths
runtime-only object references
raw scene object pointers
thread/task handles
open transactions
session tokens
credentials
private keys
debug-only state
non-reproducible transient state
```

### Save Field Record

```md
## Save Field Record

- Field:
- Category:
  - Persistent Gameplay
  - Player Authored
  - Derived
  - Runtime Only
  - Engine Internal
  - Sensitive
- Saved:
  - Yes / No
- Reason:
- Validation:
- Migration impact:
- Privacy impact:
```

### Save Content Rules

- Save stable IDs, not transient object references.
- Save source state, not derived display values.
- Save deterministic seeds if needed to reproduce procedural state.
- Do not save server-only authoritative state as trusted local truth.
- Save data should be minimal but sufficient to reconstruct persistent state.

---

## Player-Authored String Policy

### Player-Authored Fields

Examples:

```text
character name
save slot name
custom marker label
pet name
loadout name
user-created note
```

### String Sanitation Record

```md
## Player-Authored Save String

- Field:
- Max length:
- Encoding:
- Unicode normalization:
- Control character handling:
- Whitespace handling:
- Profanity/moderation dependency:
- Display fallback:
- Privacy class:
- Tests:
```

### String Rules

- Bound length.
- Strip control characters.
- Normalize or reject invalid encoding.
- Avoid storing private personal data where possible.
- Sanitize on load.
- Sanitize before display if needed.
- Do not log full raw player-authored strings without approval.
- If strings may sync to server or be shared, apply stricter moderation/privacy policy.

---

## Save Validation

### Validation Requirements

After HMAC verification and neutral deserialization, validate:

- required fields,
- field types,
- field ranges,
- enum values,
- array lengths,
- string lengths,
- ID references,
- duplicate IDs,
- impossible states,
- inventory capacity,
- quest-state consistency,
- reputation bounds,
- unlock dependencies,
- seed validity,
- version-specific invariants.

### Save Validation Record

```md
## Save Validation Rule

- Field/system:
- Rule:
- Applies to version(s):
- Failure behavior:
- Migration interaction:
- Tests:
```

### Validation Rules

- Invalid required state rejects loudly.
- Optional defaults must be documented.
- Validation failure must not silently create a new game.
- Validation failure should preserve the invalid save for safe review if policy allows.
- Validate again after migration.

---

## Backup, Quarantine, and Recovery

### Backup Policy

Use backups to protect players from write failures and migration failures.

```md
## Save Backup Policy

- Backup count:
- Backup trigger:
- Backup location:
- Backup HMAC:
- Restore behavior:
- Retention:
- Privacy:
```

### Quarantine Policy

Invalid/tampered saves may be moved or copied into a quarantine location for review.

```md
## Save Quarantine Record

- Save slot:
- Reason:
  - HMAC mismatch
  - Version mismatch
  - Migration failure
  - Validation failure
  - Corruption
- Player-visible behavior:
- Quarantine path:
- Privacy class:
- Retention:
- Review owner:
```

### Recovery Rules

- Do not load tampered saves.
- Do not overwrite invalid saves before preserving safe evidence when policy allows.
- Offer player-safe recovery only from last known good save or approved backup.
- Quarantined saves must not be auto-loaded.
- Quarantine logs must not expose private data.

---

## Cloud Saves and T3+ Server Sync

### Local Save Trust Rule

Local HMAC validates local integrity only. It does not make the save server-authoritative.

### Sync Record

```md
## Save Sync Integrity Review

- Sync tier:
- Local HMAC checked:
- Server validation:
- Conflicting save behavior:
- Tamper mismatch behavior:
- Migration before sync:
- Player-authored strings:
- Audit needed:
- Owner:
```

### Sync Rules

- Server must validate consequential state before accepting local save data.
- Local tamper detection failure blocks sync.
- Migrated saves must validate before sync.
- Player-authored strings must be sanitized before sync.
- Sync conflicts must not merge invalid/tampered data.
- High-value state changes may require audit at T3+.

---

## Save Failure Logging

### Log Categories

```text
SAVE_HMAC_MISMATCH
SAVE_SIGNATURE_MISSING
SAVE_KEY_UNAVAILABLE
SAVE_VERSION_MISMATCH
SAVE_MIGRATION_REQUIRED
SAVE_MIGRATION_FAILED
SAVE_VALIDATION_FAILED
SAVE_CORRUPT
SAVE_WRITE_FAILED
SAVE_ATOMIC_REPLACE_FAILED
SAVE_BACKUP_RESTORED
SAVE_SYNC_BLOCKED
```

### Log Record

```md
## Save Integrity Log Event

- Event:
- Save slot:
- Save version:
- Build version:
- Failure category:
- Player-visible message key:
- Privacy class:
- Redaction:
- Rate limit:
- Owner:
```

### Logging Rules

- Do not log HMAC keys.
- Do not log raw save payload.
- Do not log raw player-authored strings without approval.
- Do not log secrets or credentials.
- Use save slot IDs and hashes where possible.
- Player-visible errors must be clear but not expose implementation details.
- Security-relevant events should be reviewable.

---

## Player-Facing Failure Behavior

### Error Message Goals

Player-facing save failure messages should:

- be clear,
- avoid technical exploit details,
- explain whether the save can be recovered,
- avoid silently overwriting progress,
- avoid blaming the player without evidence,
- provide next safe action.

### Failure Behavior Record

```md
## Save Failure Behavior

- Failure:
- Player-visible message:
- Recovery option:
- Backup restore:
- Quarantine:
- Support/debug info:
- Tests:
```

### Failure Behavior Rules

- HMAC mismatch: reject and show loud integrity error.
- Version mismatch: show incompatible save version message.
- Migration unavailable: show migration unavailable message.
- Migration failure: preserve original save and show migration failed message.
- Corruption: reject and offer backup restore if available.
- Never silently create default state as substitute for failed load.

---

## Save Integrity Test Requirements

### Required Test Categories

At T1+:

- valid save loads,
- missing signature rejects,
- modified payload rejects,
- modified metadata rejects if metadata is signed,
- wrong key rejects,
- malformed signature rejects,
- version-first check works,
- older save migrates when path exists,
- older save rejects when path missing,
- newer save rejects on older loader,
- migration fixture test passes,
- migration failure does not mutate original save,
- invalid required field rejects,
- optional defaults behave as documented,
- player-authored string sanitation works,
- derived/runtime-only fields are not saved,
- atomic write protects previous save,
- corrupt/truncated save rejects,
- backup restore works if implemented.

### Test Record

```md
## Save Integrity Test

- Test ID:
- Category:
- Save version:
- Fixture:
- Input condition:
- Expected behavior:
- Actual behavior:
- Status:
- Evidence:
```

### Test Rules

- Migration tests use real prior-version save fixtures.
- Tamper tests modify signed bytes and expect rejection.
- Corrupt tests truncate or malformed-save fixtures and expect rejection.
- Tests must prove no partial load.
- Tests must verify loud failure behavior.
- Tests must not contain private player data.

---

## Save Integrity Review Format

Use this for reviews:

```md
## Save Integrity Review: [System / File / Change]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Active Tier

### HMAC Status

### Key Derivation Status

### Versioning Status

### Load Path Status

### Write Path Status

### Migration Status

### Fixture Status

### Save Content Status

### Player-Authored String Status

### Backup / Quarantine / Recovery Status

### Server Sync Status

### Logging / Privacy Status

### Test Evidence

### Required Follow-Up
```

### Severity

```text
SAVE-S1 — Critical
Tampered save can load, incompatible save can partial-load, key is stored plaintext, migration corrupts player data, or local tampered state can sync as trusted server state.

SAVE-S2 — High
Missing HMAC, missing version-first check, missing migration test, missing fixture, invalid string sanitation, unsafe save content, or failed loud-rejection path.

SAVE-S3 — Medium
Weak backup policy, incomplete logging/redaction, missing optional default validation, incomplete migration documentation, unclear cloud-save behavior.

SAVE-S4 — Low
Documentation gap, naming issue, minor fixture metadata issue, non-blocking cleanup.
```

---

## Self-Learning Protocol

Self-learning means controlled improvement from save migration failures, tamper reports, QA findings, support reports, fixture failures, server-sync incidents, security reviews, and user corrections.

It does not mean hidden memory updates, autonomous key policy changes, or treating one corrupted save as universal truth.

### What May Be Learned

The save-integrity rule system may learn:

- approved save envelope structure,
- approved HMAC coverage rules,
- approved key derivation policy,
- known canonicalization pitfalls,
- known migration failure modes,
- known fixture requirements,
- known player-authored string sanitation rules,
- known save-content allow/deny decisions,
- backup/restore findings,
- cloud-save conflict findings,
- server-sync integrity findings,
- rejected unsafe approaches and why.

### What Must Not Be Learned or Stored

Do not store:

- HMAC keys,
- key derivation secrets,
- salts/nonces if sensitive,
- private player data,
- raw save payloads with personal data,
- credentials,
- tokens,
- private keys,
- raw support saves without approved handling,
- private chain-of-thought,
- detailed tampering instructions in general docs,
- one-off corrupt saves as global policy,
- prototype save shortcuts as production rules.

### Lesson Classification

Use:

```text
Confirmed Rule
Approved Save Standard
HMAC Finding
Key Derivation Finding
Canonicalization Finding
Versioning Finding
Migration Finding
Fixture Finding
Validation Finding
Player String Finding
Save Content Finding
Backup Finding
Quarantine Finding
Cloud Sync Finding
Server Sync Finding
Security Review Finding
QA Finding
Incident Finding
Rejected Approach
Working Assumption
Temporary Context
Superseded
```

### Lesson Storage

Store durable lessons only in approved, reviewable locations such as:

```text
docs/security/save-integrity.md
docs/persistence/save-format.md
docs/persistence/save-migration-lessons.md
design/gdd/save-system.md
SECURITY.md
RED_TEAM.md
tasks/lessons.md
production/qa/save/
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
- it applies to save integrity,
- it does not include sensitive data,
- it does not expose keys or private save payloads,
- it is not overgeneralized,
- it does not conflict with security policy,
- it has owner/review trigger where appropriate.

### Lesson Expiry

Review or expire lessons when:

- save format changes,
- key derivation changes,
- platform storage changes,
- cloud-save policy changes,
- server-sync architecture changes,
- migration strategy changes,
- privacy requirements change,
- fixture evidence contradicts the lesson,
- security review supersedes it,
- the lesson was temporary,
- the lesson is too broad.

---

## Self-Healing Protocol

Self-healing means detecting a save-integrity failure, containing the risk, repairing safely, verifying the repair, and reporting what changed.

### Failure Types

Monitor for:

- missing HMAC,
- HMAC mismatch,
- missing signature,
- key stored plaintext,
- key derivation undocumented,
- key unavailable,
- gameplay state deserialized before HMAC verification,
- version not first,
- version not checked before payload,
- newer save partial-load risk,
- older save migration missing,
- migration failure,
- fixture missing,
- prior-version deserialization path changed,
- invalid save field accepted,
- player-authored string unsanitized,
- runtime-only value saved,
- derived value saved,
- non-atomic save write,
- corrupt save silently reset,
- tampered save synced to server,
- sensitive data logged.

### Recovery Loop

When failure occurs:

1. **Stop**
   - Do not load, migrate, sync, or mark the save path safe.

2. **Identify**
   - State the exact save-integrity failure.

3. **Classify**
   - HMAC, key, version, migration, fixture, validation, content, string, write, recovery, sync, or logging issue.

4. **Contain**
   - Mark status:
     - `BLOCKED`,
     - `HMAC_FAILED`,
     - `KEY_UNAVAILABLE`,
     - `VERSION_MISMATCH`,
     - `MIGRATION_REQUIRED`,
     - `MIGRATION_FAILED`,
     - `FIXTURE_MISSING`,
     - `SAVE_REJECTED`,
     - `SERVER_SYNC_BLOCKED`.

5. **Recover**
   - reject save,
   - preserve original save,
   - restore last known good backup if available,
   - quarantine invalid save if policy allows,
   - add missing validation,
   - document key derivation,
   - add migration path,
   - add fixture test,
   - remove forbidden saved field,
   - sanitize string,
   - fix atomic write path,
   - block server sync.

6. **Verify**
   - Run or request relevant tests.
   - Confirm no partial load.
   - Confirm failure is loud.
   - Confirm logs are redacted.

7. **Report**
   - Summarize issue, fix, remaining risk, and owner.

8. **Learn**
   - Propose durable lesson only if validated and approved.

---

## Error Recovery

### HMAC Mismatch

If HMAC verification fails:

- reject save before gameplay-state deserialization,
- do not silently create default state,
- do not partially load,
- preserve/quarantine safe evidence if policy allows,
- offer backup restore if available,
- log redacted integrity event.

### Missing Signature

If signature is absent:

- reject as invalid production save,
- or route through approved legacy migration only if explicitly supported,
- do not assume unsigned save is trustworthy.

### Key Unavailable

If derived key cannot be obtained:

- do not load save,
- show player-safe failure,
- preserve save,
- use documented recovery or backup path,
- do not generate a new key and silently invalidate existing saves unless policy allows.

### Version Missing or Not First

If version is missing or not first:

- reject or route through explicitly approved legacy migration,
- do not attempt full deserialization,
- update save format or migration docs.

### Older Save With No Migration

If current loader sees older save and no migration exists:

- fail loud,
- preserve original save,
- do not partial-load,
- add migration path or mark unsupported with owner approval.

### Older Loader With Newer Save

If older loader sees newer save:

- reject with version-mismatch error,
- do not partial-load,
- do not attempt fallback defaults.

### Migration Failure

If migration fails:

- preserve original save,
- do not write current-version replacement,
- do not partial-load,
- record failure category,
- add regression fixture if not already present.

### Missing Fixture

If a version bump lacks canonical fixture:

- block migration readiness,
- add real prior-version save fixture,
- add expected-state assertion,
- preserve fixture in `tests/fixtures/saves/v[N]/`.

### Invalid Save Field

If deserialized field is out of range or impossible:

- reject or migrate according to version-specific policy,
- do not clamp silently unless schema says so,
- add validation test.

### Unsanitized Player String

If player-authored string is not sanitized:

- strip control characters,
- cap length,
- normalize or reject invalid encoding,
- avoid raw logging,
- add load-time sanitation test.

### Forbidden Saved Value

If save includes engine internals, derived values, handles, connections, or temp paths:

- remove field from save schema,
- reconstruct derived value at runtime,
- add schema validation test,
- migrate old saves if needed.

### Non-Atomic Write

If save write can leave active partial file:

- write to temp file,
- validate temp file,
- replace atomically where supported,
- preserve backup,
- add interrupted-write test if infrastructure exists.

### Sensitive Logging

If save logs expose raw payload, player-authored private text, keys, or signatures:

- stop logging unsafe fields,
- redact or hash identifiers,
- escalate Security/Privacy review,
- update logging rules/tests.

### Server Sync Risk

If tampered or unvalidated save could sync:

- block sync,
- revalidate server-side,
- audit high-value changes if T3+,
- do not treat local HMAC as server trust.

---

## Memory Policy

### Short-Term Task Memory

Track during current task:

- save format version,
- save path,
- HMAC status,
- key derivation status,
- signed bytes,
- version handling,
- migration path,
- fixture path,
- validation rules,
- player-authored fields,
- backup/quarantine behavior,
- server sync implications,
- test evidence,
- open decisions,
- approvals needed.

Short-term memory expires after the task unless explicitly stored.

### Project Memory

Project memory may store:

- approved save format decisions,
- approved HMAC policy,
- key derivation decision summary without secrets,
- migration rules,
- fixture standards,
- validation rules,
- backup/quarantine findings,
- cloud-save policy,
- server-sync rules,
- security review findings,
- rejected approaches.

### Never Store

Never store:

- HMAC keys,
- derived key material,
- plaintext secrets,
- credentials,
- private keys,
- tokens,
- private player data,
- raw sensitive save payloads,
- support saves without approved handling,
- private chain-of-thought,
- detailed tampering recipes in general-purpose memory,
- unsupported claims that saves are secure.

---

## Feedback Policy

When the user, Security Engineer, Technical Director, Lead Programmer, Gameplay Programmer, QA Lead, DevOps Engineer, Release Manager, or Privacy/Legal owner corrects save behavior:

1. Accept the correction.
2. Identify whether it affects:
   - HMAC,
   - key derivation,
   - envelope format,
   - versioning,
   - migration,
   - fixtures,
   - save content,
   - player-authored strings,
   - backup/restore,
   - quarantine,
   - logging,
   - cloud sync,
   - server sync,
   - tests.
3. Revise current output.
4. Ask whether the correction should become durable save-integrity guidance if reusable.
5. Store only if approved and evidence-backed.

---

## Tool-Use Policy

This rules file does not grant tools by itself. Agents applying it must follow their own tool permissions.

General guidance:

- Use file-reading tools to inspect save code, save GDDs, security docs, migration code, fixtures, tests, and QA evidence.
- Use search tools to find save fields, HMAC verification, key derivation, version checks, migration paths, fixture references, and failure logs.
- Use write/edit tools only after approval under the active agent’s workflow.
- Use Bash only if the active agent allows it and only under that agent’s safety policy.
- Do not run destructive save tests, delete fixtures, mutate canonical saves, or inspect private player saves without explicit approval.
- Do not use Bash to bypass write/edit approval.

---

## Safety Guardrails

Never allow production save code to:

- load unsigned saves,
- deserialize gameplay state before HMAC verification,
- store HMAC key plaintext next to save,
- silently fall back to default state on HMAC mismatch,
- partial-load incompatible saves,
- edit prior-version deserialization paths casually,
- bump save version without migration test,
- omit canonical prior-version fixture,
- save runtime-only/engine-internal state,
- save derived/cache state as authoritative,
- accept unbounded player-authored strings,
- sync tampered local saves as trusted server state,
- log keys, raw private saves, or sensitive player data,
- claim save-integrity readiness without evidence.

---

## Output Standards

Save-integrity reviews should be:

- tier-aware,
- HMAC-aware,
- key-derivation-aware,
- version-first,
- migration-safe,
- fixture-backed,
- validation-specific,
- privacy-aware,
- server-sync-aware,
- evidence-backed,
- explicit about unresolved decisions.

### Review Output Format

```md
## Save Integrity Review: [System / File / Change]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### HMAC / Signature

### Key Derivation

### Versioning

### Load Path

### Write Path

### Migration

### Fixtures

### Saved Fields

### Player-Authored Strings

### Backup / Recovery

### Logging / Privacy

### Server Sync

### Tests / Evidence

### Required Follow-Up
```

---

## Reflection Checklist

After reviewing or drafting save-integrity work, privately check:

- Is this T1+ save code?
- Does every production save carry an HMAC?
- Is the HMAC key derived, not stored next to the save?
- Is key derivation documented?
- Does load verify HMAC before gameplay-state deserialization?
- Is version the first field or first bounded header value?
- Does loader check version before payload?
- Are newer saves rejected by older loaders?
- Are older saves migrated only through complete migration paths?
- Are migrations forward-only?
- Are canonical fixtures present in `tests/fixtures/saves/v[N]/`?
- Is field-level validation defined?
- Are player-authored strings bounded and sanitized?
- Are runtime-only and derived values excluded?
- Is write path atomic?
- Is failure loud, not silent?
- Are logs redacted?
- Is T3+ server sync prevented from trusting local saves blindly?
- Did I avoid storing sensitive lessons?

Do not expose private chain-of-thought. Report only findings, evidence, and recommendations.

---

## Evaluation Checklist

Before final approval of save code:

### HMAC and Key Handling

- [ ] Production saves include HMAC.
- [ ] HMAC covers payload and integrity-relevant metadata.
- [ ] Signature field is excluded from signed bytes.
- [ ] Constant-time comparison is used where available.
- [ ] Key is derived.
- [ ] Key is not stored plaintext next to save.
- [ ] Key derivation strategy is documented.
- [ ] HMAC mismatch rejects loudly.

### Load Path

- [ ] File size/header reads are bounded.
- [ ] Version is first field/header value.
- [ ] Version is checked before payload deserialization.
- [ ] HMAC is verified before gameplay-state deserialization.
- [ ] Save is deserialized into neutral data before runtime commit.
- [ ] Field validation runs before commit.
- [ ] No partial load exists.

### Versioning and Migration

- [ ] Save format has version record.
- [ ] Older save migration path exists or fails loud.
- [ ] Newer save is rejected by older loader.
- [ ] Migration is forward-only.
- [ ] Prior deserialization paths are preserved.
- [ ] Canonical fixture exists for prior version.
- [ ] Migration test uses real prior-version save.
- [ ] Migration failure preserves original save.

### Save Contents

- [ ] Inventory, quest progress, reputation, unlocks, and seeds are saved where required.
- [ ] Engine internals are not saved.
- [ ] Derived/cached values are not saved as authoritative.
- [ ] Network connections, file handles, and temp paths are not saved.
- [ ] Stable IDs are used instead of transient references.
- [ ] Optional defaults are documented.

### Player-Authored Strings

- [ ] Length cap exists.
- [ ] Control characters are stripped.
- [ ] Encoding/normalization policy exists.
- [ ] Raw private text is not logged.
- [ ] Server-sync path sanitizes again if needed.

### Write / Recovery

- [ ] Save writes use temp file and atomic replace where supported.
- [ ] Previous valid save is protected.
- [ ] Backup policy exists where needed.
- [ ] Quarantine policy exists for invalid saves.
- [ ] Player-facing failure messages are clear and safe.

### Evidence

- [ ] Tamper test exists.
- [ ] Missing signature test exists.
- [ ] Version mismatch tests exist.
- [ ] Migration tests exist.
- [ ] Corrupt/truncated save test exists.
- [ ] Player string sanitation test exists.
- [ ] No unsupported readiness claims are made.

---

## Example Workflows

### Example 1: Tampered Save

Issue:

```text
Save payload changes after signature was computed.
```

Response:

```md
Finding: HMAC mismatch.

Expected behavior:
- Reject before gameplay-state deserialization.
- Do not partial-load.
- Do not silently create default state.
- Preserve/quarantine safe evidence if policy allows.
- Offer backup restore if available.
```

---

### Example 2: Missing Signature

Issue:

```text
Save file contains version and payload but no signature.
```

Response:

```md
Finding: Missing HMAC signature.

Fix:
- Reject as invalid production save.
- If this is a legacy pre-HMAC save, create an explicit legacy migration path.
- Do not treat unsigned local save as trustworthy.
```

---

### Example 3: Plaintext Key Beside Save

Issue:

```text
save_key.txt is stored in the same folder as player_save.json.
```

Response:

```md
Finding: Key storage violation.

Severity: SAVE-S1 or SAVE-S2 depending on exposure.

Fix:
- Remove plaintext adjacent key storage.
- Use approved per-install key derivation or platform-protected storage.
- Document derivation strategy in the save-system GDD/security docs.
```

---

### Example 4: Version Not First

Issue:

```json
{
  "inventory": {},
  "version": 2
}
```

Response:

```md
Finding: Version field is not first.

Fix:
- Move version to the first field/header value.
- Loader must check version before attempting to deserialize gameplay state.
```

---

### Example 5: Older Save Without Migration

Issue:

```text
Current loader sees v2 save, current version is v4, and v2→v4 migration path is missing.
```

Response:

```md
Finding: Migration unavailable.

Expected behavior:
- Fail loud.
- Preserve original save.
- Do not partial-load.
- Add migration path and fixture test before supporting v2.
```

---

### Example 6: Migration Test Missing

Issue:

```text
Save version bumped from v3 to v4, but no test fixture exists in tests/fixtures/saves/v3/.
```

Response:

```md
Finding: Migration fixture missing.

Fix:
- Add real v3 canonical save fixture.
- Add test: v3 fixture -> current loader -> expected v4 state.
- Block migration readiness until test exists.
```

---

### Example 7: Runtime State Saved

Issue:

```text
Save file contains active network connection ID and temp file path.
```

Response:

```md
Finding: Runtime-only state saved.

Fix:
- Remove network connection and temp path from save schema.
- Reconstruct runtime state after load.
- Add schema validation preventing these fields.
```

---

### Example 8: Player String Unsafe

Issue:

```text
Character name accepts 10,000 characters and control codes.
```

Response:

```md
Finding: Player-authored string validation missing.

Fix:
- Cap length.
- Strip control characters.
- Normalize or reject invalid encoding.
- Avoid raw logging.
- Add load-time sanitation test.
```

---

### Example 9: Silent Default on Failure

Issue:

```text
If save verification fails, game starts a new default save automatically.
```

Response:

```md
Finding: Silent fall-through to default state.

Severity: SAVE-S1/SAVE-S2.

Fix:
- Reject loudly.
- Show player-safe error.
- Offer backup restore or explicit new-game choice.
- Do not hide integrity failure.
```

---

### Example 10: Server Sync Risk

Issue:

```text
T3 server accepts local inventory from save after local HMAC passes.
```

Response:

```md
Finding: Local HMAC incorrectly treated as server authority.

Fix:
- Server must validate inventory grants and consequential state.
- HMAC only proves local file was not modified relative to local key.
- Server sync must audit or reject suspicious high-value changes.
```

---

### Example 11: User Correction

User says:

```text
Character names max out at 24 visible characters after normalization.
```

Response pattern:

```text
Understood. I’ll treat character names as capped at 24 visible characters after normalization and control-character stripping. Store this as durable save-integrity guidance only if approved.
```

---

### Example 12: Case Where the Rule Must Not Learn

User says during prototyping:

```text
Skip HMAC while we test save slots.
```

Response pattern:

```text
That can be treated only as a prototype/test exception if clearly labeled and isolated. Production T1+ save code still requires HMAC signing and verification before gameplay-state deserialization.
```

---

## See Also

- `RED_TEAM.md` §6 and §9 — save-integrity and versioning rubrics.
- `SECURITY.md` threat #1.
- `AGENTS.md` §11 — PII handling for player-authored strings.
- `tests/fixtures/saves/v[N]/` — canonical migration fixtures.
- `design/gdd/save-system.md` — recommended save-system design source.
- `docs/security/save-integrity.md` — recommended durable save-integrity policy.

---

## Final Save Integrity Rule

T1+ save code must be:

- HMAC-signed,
- key-derived,
- version-first,
- verification-before-deserialization,
- fail-loud,
- no-partial-load,
- forward-migrated only,
- fixture-tested,
- content-bounded,
- player-string sanitized,
- atomic-write safe,
- backup/recovery aware,
- server-sync cautious,
- privacy-safe in logs,
- and honest about unresolved integrity evidence.