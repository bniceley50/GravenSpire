---
paths:
  - "src/networking/**"
  - "Assets/Scripts/Networking/**"
---

# Netcode Conventions: Security & Integrity

## Rule Set Name

Netcode Conventions: Security & Integrity

## Mission

These rules govern multiplayer security, server authority, anti-cheat integrity, validation, and audit logging for:

```text
src/networking/**
Assets/Scripts/Networking/**
```

This file is orthogonal to:

```text
.claude/rules/network-code.md
```

`network-code.md` governs network code style, replication strategy, prediction/reconciliation mechanics, bandwidth budgets, and general networking implementation. This file governs the **trust boundary**: what the server may trust, what clients may request, what must be validated, what must be audit-logged, and what must never enter authoritative state without proof.

The core security question for every networked feature is:

> Can a hostile or faulty client send this message, and if so, can the server reject it without corrupting state, leaking information, flooding logs, or harming legitimate players?

---

## Active Tier

```text
Active tier: T2+
```

This rule is inert during T1.

During T1:

- Networking code does not exist per `DECISIONS.md` D003.
- Any networking implementation must remain stubbed, documented as future work, or blocked by tier.
- Do not introduce production network trust-boundary behavior during T1.
- Do not install or normalize networking dependencies unless the relevant T2 decision allows it.

At T2+:

- Server authority is mandatory.
- Inbound validation is mandatory.
- Anti-cheat surfaces must be reviewed.
- High-value state changes must be audit-logged.
- Client-authored consequential state is prohibited.

---

## Operating Principles

1. **Server authority is non-negotiable**
   - All gameplay-critical state lives on the server or authoritative host.
   - The client is a render, input, prediction, and presentation target.
   - Clients send intent, not truth.

2. **Client prediction is local only**
   - Clients may predict for responsiveness.
   - Predicted state must reconcile with server authority.
   - Prediction must never grant durable rewards, inventory, reputation, currency, unlocks, or progression.

3. **Client-authored values are untrusted**
   - No client-authored value may enter server state without validation.
   - Validation must check range, rate, context, ownership, authority, sequence, timing, and action legality.

4. **Reject safely**
   - Invalid inbound messages are logged and dropped.
   - Do not echo validation failure details to the client.
   - Player-facing errors must be generic and non-informative from a cheat perspective.

5. **Anti-cheat signals are not punishment decisions**
   - Suspicious behavior should produce evidence.
   - Enforcement actions require approved policy, severity classification, and owner review.
   - Do not reveal detection logic to clients.

6. **Audit high-value state changes**
   - Item grants, reputation changes, unlock grants, currency changes, match outcomes, progression changes, and security-sensitive state changes must have append-only audit records.

7. **Logs must be useful and safe**
   - Logs must be structured, rate-limited, redacted, and privacy-classified.
   - Do not log secrets, tokens, private player data, raw sensitive payloads, or cheat-detection internals.

8. **Rate limits are part of validation**
   - Every client-to-server action requires a rate-limit policy.
   - Missing rate limit is a trust-boundary defect.

9. **Security evidence must be reviewable**
   - Validation rules, thresholds, audit fields, and review outcomes must live in reviewable project files or approved security systems.
   - Hidden memory is not enough.

10. **Self-healing before release**
    - If authority, validation, logging, audit, or anti-cheat integrity fails, stop, contain, fallback where possible, repair, verify, and report.

11. **Bounded self-learning**
    - Lessons from security reviews, incidents, QA, red-team tests, audit findings, and user corrections may be stored only in approved, reviewable locations.
    - Lessons must not expose exploit instructions, sensitive payloads, or private player data.

---

## Scope

These rules apply to:

- client-to-server messages,
- server-to-client authoritative results,
- RPC validation,
- packet validation,
- action legality checks,
- client prediction trust boundaries,
- hit registration validation,
- movement validation,
- item grants,
- reputation changes,
- unlocks,
- rewards,
- currency changes,
- progression changes,
- anti-cheat signal generation,
- suspicious behavior logging,
- audit logging,
- validation error handling,
- replay/spoofing protection,
- rate limits,
- T2+ networking security reviews.

---

## Non-Goals

These rules do not authorize:

- network protocol architecture changes without Technical Director approval,
- anti-cheat punishment policy decisions without Security Engineer approval,
- production infrastructure changes,
- storing sensitive player data casually,
- exposing validation internals to clients,
- trusting client-reported gameplay outcomes,
- collecting unnecessary personal data,
- bypassing validation for speed,
- weakening server authority for convenience,
- file edits without the active agent’s approval workflow,
- persistent lesson storage without approval.

---

## Instruction Priority

When these rules conflict with other instructions, apply this priority:

1. System, platform, privacy, legal, security, and safety constraints.
2. Current user instruction.
3. `SECURITY.md`.
4. `RED_TEAM.md`.
5. `DECISIONS.md`.
6. Technical Director architecture decisions.
7. Security Engineer trust-boundary decisions.
8. This netcode integrity file.
9. `.claude/rules/network-code.md`.
10. Network Programmer implementation contracts.
11. Gameplay Programmer integration contracts.
12. Existing implementation.
13. Working assumptions.

If a lower-priority instruction asks to trust client gameplay state, expose validation details, skip validation, or store unsafe logs, reject that part and propose a safe alternative.

---

## Netcode Integrity State Labels

Use these labels when reviewing or implementing networking security behavior:

```text
INERT_T1 — networking security rule inactive because T1 has no networking implementation.
T2_REQUIRED — networking security controls required.
PROPOSED — security rule or validation path suggested but not approved.
SPEC_READY — authority/validation/audit spec documented.
IMPLEMENTED — code exists.
AUTHORITY_SAFE — authoritative owner verified.
CLIENT_TRUST_RISK — client value may influence server state unsafely.
VALIDATION_READY — validation rules documented.
VALIDATION_IMPLEMENTED — validation code exists.
RATE_LIMITED — rate limit exists and is enforced.
REJECTION_SAFE — invalid messages log/drop without leaking details.
AUDIT_REQUIRED — high-value action requires audit record.
AUDIT_LOGGED — append-only audit log path exists.
AUDIT_RETENTION_UNRESOLVED — T3 retention decision missing.
AUDIT_REDACTED — audit fields are privacy-safe or pseudonymized.
SECURITY_REVIEWED — Security Engineer or approved owner reviewed.
RED_TEAM_TESTED — red-team/abuse tests executed.
QA_VERIFIED — QA validated expected behavior.
BLOCKED — unsafe or missing required trust-boundary control.
SUPERSEDED — replaced by newer convention.
DEPRECATED — still present but not for new use.
```

### State Rules

- Do not mark `AUTHORITY_SAFE` without authority evidence.
- Do not mark `VALIDATION_IMPLEMENTED` without validation code or test evidence.
- Do not mark `RATE_LIMITED` without rate-limit threshold and enforcement path.
- Do not mark `AUDIT_LOGGED` unless log is append-only or otherwise tamper-resistant.
- Do not mark `AUDIT_REDACTED` without redaction/privacy review.
- Do not mark `SECURITY_REVIEWED` without review evidence.
- `IMPLEMENTED` is not equivalent to safe.

---

## T2 Activation Gate

Before any T2+ networking security work is considered active, confirm:

```md
## T2 Netcode Security Activation Gate

- Active tier:
- D003 networking status:
- Network model:
  - Dedicated server
  - Listen server
  - Peer-to-peer
  - Relay
  - Hybrid
- Authoritative owner:
- Client prediction allowed:
- Security owner:
- Validation owner:
- Audit logging owner:
- Required high-value audit actions:
- Open decisions:
- Status:
```

### T2 Gate Rules

- If the project is still T1, mark networking security as `INERT_T1`.
- If networking begins at T2, apply these rules before accepting client-to-server messages.
- If network model is unknown, mark trust-boundary design `BLOCKED`.
- If authoritative owner is unknown, mark gameplay-critical network code `BLOCKED`.

---

## T3 Audit Retention Gate

Audit log retention policy is a design-level decision at T3 entry.

Before long-term audit logging is enabled, define:

```md
## T3 Audit Retention Gate

- Audit log categories:
- Retention duration:
- Access roles:
- Redaction policy:
- Pseudonymous ID policy:
- Deletion policy:
- Export policy:
- Privacy review:
- Security review:
- Legal/compliance review, if required:
- Owner:
- Status:
```

### T3 Retention Rules

- Before T3 retention is defined, audit logging may be implemented but must be marked `AUDIT_RETENTION_UNRESOLVED`.
- Do not store raw private player data in audit logs.
- Do not retain sensitive audit logs indefinitely without approval.
- Retention must support security review while respecting privacy constraints.

---

## Gameplay-Critical State Taxonomy

Server-authoritative by default:

```text
health
damage
hit results
player position where fairness-sensitive
movement authority
inventory
item grants
currency
rewards
loot grants
reputation
progression
unlock state
quest state
objective state
cooldowns
ability state
match state
match results
ranked outcomes
save-relevant multiplayer state
economy transactions
```

### Gameplay-Critical State Record

```md
## Gameplay-Critical State Record

- State:
- Authoritative owner:
- Client input allowed:
- Client prediction allowed:
- Server validation:
- Audit required:
- Anti-cheat risk:
- Evidence:
```

### Rules

- Client may display or predict.
- Client may request.
- Client may not decide.
- Server computes or verifies all consequential outcomes.
- Any exception requires Technical Director and Security Engineer review.

---

## Client Intent vs Client Fact

### Allowed Client Intent

Clients may send requests such as:

```text
move input
aim direction
attack request
ability activation request
interact request
ready-state request
emote request
cosmetic selection request
chat/message input
menu/session request
```

### Forbidden Client Facts

Clients must not authoritatively send:

```text
I hit this target.
I dealt this damage.
I gained this item.
I changed my reputation.
I earned this currency.
I completed this objective.
I am no longer stunned.
I teleported here.
My cooldown is reset.
My inventory now contains this.
My rank/match result is this.
```

### Client Message Classification

```md
## Client Message Classification

- Message:
- Classification:
  - Intent
  - Suggested observation
  - Cosmetic request
  - Gameplay-critical claim
  - Forbidden authoritative claim
- Server response:
- Validation:
- Audit:
- Verdict:
```

### Rules

- Suggested observations are not facts.
- Gameplay-critical claims are rejected unless converted into validated intent.
- Forbidden authoritative claims must not mutate state.
- Cosmetic requests still require validation and rate limits.

---

## Inbound Validation Standard

Every inbound packet/message validates:

```text
size
message type
message version
required fields
field types
field ranges
no NaN / Infinity / invalid numeric values
enum domain
string length and encoding
payload length
sender identity
session membership
ownership
authority
current game/session state
action legality
target validity
resource/cooldown availability
rate limit
sequence number
timestamp/window
replay/spoofing risk
context legality
```

### Inbound Validation Record

```md
## Inbound Validation: [Message / RPC]

- Message:
- Direction:
- Max size:
- Version:
- Required fields:
- Field ranges:
- Sender identity check:
- Session membership check:
- Ownership check:
- Authority check:
- Action legality:
- Resource/cooldown check:
- Target validity:
- Rate limit:
- Sequence/timestamp validation:
- Replay protection:
- Rejection behavior:
- Logging:
- Tests:
```

### Validation Rules

- Validate before state mutation.
- Validate before expensive processing where possible.
- Bound all sizes before deserialization into dynamic structures.
- Reject invalid payloads safely.
- Drop invalid messages.
- Do not echo detailed rejection reasons to the client.
- Log validation category safely.
- Rate-limit validation-failure logs.

---

## Rejection Path

### Required Behavior

When validation fails:

```text
log + drop
```

### Rejection Record

```md
## Rejection Path: [Message / Action]

- Validation failure:
- Client-visible response:
- Server behavior:
- Log category:
- Log fields:
- Rate limit:
- Audit needed:
- Security escalation:
```

### Rejection Rules

- Do not send detailed validation failure to client.
- Do not reveal thresholds.
- Do not reveal cheat-detection categories.
- Do not reveal server state.
- Do not mutate gameplay state.
- Do not crash session.
- Do not flood logs.
- Use generic client response only if needed:
  - request rejected,
  - action unavailable,
  - action failed,
  - resync required.

---

## Rate-Limit Policy

Every client-originated action needs a rate limit.

### Rate Limit Record

```md
## Rate Limit: [Action / Message]

- Action/message:
- Scope:
  - per player
  - per account
  - per session
  - per IP / connection
  - global
- Sustained limit:
- Burst limit:
- Window:
- Exceeded behavior:
- Cooldown:
- Logging:
- Security escalation:
- Tests:
```

### Rate-Limit Rules

- High-frequency gameplay inputs need carefully tuned limits.
- Chat/string inputs need anti-spam limits.
- Item/economy/reputation actions need strict limits and audit.
- Exceeding limit should drop or reject safely.
- Repeated violations may produce anti-cheat signal.
- Do not reveal exact limits to clients.

---

## Anti-Cheat Surface Standards

### Anti-Cheat Surface Record

```md
## Anti-Cheat Surface: [Surface]

- Surface:
  - Hit Registration
  - Movement
  - Item Grant
  - Reputation Change
  - Currency Change
  - Unlock Grant
  - Ability Activation
  - Cooldown Reset
  - Objective Completion
- Client input:
- Server authority:
- Validation checks:
- Suspicious signals:
- Audit required:
- Rejection behavior:
- False-positive risk:
- Review owner:
- Tests:
```

### Anti-Cheat Rules

- Treat anti-cheat events as evidence, not automatic punishment.
- Separate validation failure from enforcement.
- Do not reveal detection logic to clients.
- Avoid false positives by considering latency, packet loss, reconciliation, and server bugs.
- Security Engineer owns anti-cheat escalation policy.

---

## Hit Registration

### Rule

Server replays or validates the shot/action using server-side state. Client-reported hits are suggestions, not facts.

### Hit Registration Record

```md
## Hit Registration Validation

- Attack/action:
- Client input:
- Server replay state:
- Lag compensation:
- Max rewind:
- Target validation:
- Line-of-sight validation:
- Range validation:
- Cooldown validation:
- Damage authority:
- Suspicious signals:
- Audit required:
- Tests:
```

### Hit Registration Rules

- Client may report aim, input, timestamp, or perceived target.
- Server validates target, range, line of sight, weapon/ability state, cooldown, and timing.
- Server computes hit and damage.
- Client-reported damage is rejected.
- Client-reported critical hit is rejected.
- Repeated impossible hit claims are logged as suspicious.
- Lag compensation must be bounded.

---

## Movement Validation

### Rule

Server rate-caps position delta and flags teleport-style jumps.

### Movement Validation Record

```md
## Movement Validation

- Movement mode:
- Client input:
- Server authority:
- Max speed:
- Max acceleration:
- Max position delta:
- Teleport allowance:
- Ability modifiers:
- Stun/root/slow checks:
- Terrain/nav validity:
- Sequence/timestamp:
- Suspicious threshold:
- Reconciliation behavior:
- Tests:
```

### Movement Rules

- Client movement input may be predicted.
- Server validates motion against state, max speed, acceleration, status effects, collision/nav rules, and time window.
- Teleport-style jumps are flagged and logged unless caused by approved ability/server event.
- Server correction must reconcile client state.
- Movement validation must account for latency and packet loss to avoid false positives.
- Do not trust client-reported position as authoritative.

---

## Item Grants

### Rule

Item grants are server-side only. Client cannot add to inventory.

### Item Grant Record

```md
## Item Grant Validation

- Item:
- Grant source:
- Actor:
- Target:
- Quantity:
- Cause:
  - loot drop
  - quest reward
  - purchase
  - admin grant
  - progression unlock
  - event reward
- Server validation:
- Inventory capacity behavior:
- Duplicate behavior:
- Audit log:
- Tests:
```

### Item Grant Rules

- Client cannot create inventory items.
- Client may request pickup, purchase, claim, or use.
- Server validates source, ownership, proximity, eligibility, quantity, inventory capacity, and transaction state.
- Every high-value item grant is audit-logged.
- Duplicate grant prevention must be explicit.
- Failed grant must not partially mutate inventory.

---

## Reputation Changes

### Rule

Reputation changes are server-side only and audit-logged.

### Reputation Change Record

```md
## Reputation Change Validation

- Reputation system:
- Actor:
- Target/player:
- Delta:
- Cause:
- Threshold:
- Server validation:
- Anti-abuse checks:
- Audit log:
- RED_TEAM reference:
- Tests:
```

### Reputation Rules

- Client cannot directly set reputation.
- Reputation changes require server-side cause.
- Large changes require audit.
- Changes above threshold require append-only audit record.
- Reputation changes must be idempotent where repeated messages are possible.
- Repeated suspicious reputation-change requests are logged.

---

## Currency, Rewards, and Unlocks

### Currency / Reward / Unlock Record

```md
## High-Value State Change

- State changed:
- Actor:
- Target:
- Action:
- Delta/value:
- Cause:
- Eligibility:
- Server validation:
- Duplicate prevention:
- Audit log:
- Rollback/reversal behavior:
- Tests:
```

### Rules

- Client cannot grant currency, rewards, unlocks, achievements, progression, or ranked outcomes.
- Server validates eligibility and cause.
- High-value state changes are audit-logged.
- Duplicate grants must be prevented.
- Failure behavior must avoid partial reward state.

---

## Audit Logging

### Required High-Value Audit Actions

Audit these by default:

```text
item grants
currency changes
reputation changes above threshold
unlock grants
achievement grants
rank/match result changes
economy transactions
admin/manual grants
state changes granting progression
security-sensitive state corrections
```

### Append-Only Audit Log Schema

Required fields:

```text
actor
target
action
delta
cause
server_timestamp
server_tick
```

Recommended expanded schema:

```md
## Audit Log Event

- event_id:
- actor_id_hash:
- target_id_hash:
- actor_role:
- target_type:
- action:
- delta:
- cause:
- source_system:
- session_id_hash:
- match_id_hash:
- item_or_state_id:
- server_timestamp:
- server_tick:
- request_id:
- idempotency_key:
- validation_result:
- security_flags:
- build_version:
- schema_version:
```

### Audit Log Rules

- Audit logs must be append-only or tamper-evident.
- Use pseudonymous IDs where possible.
- Do not log raw secrets, tokens, private profile data, payment data, or raw packet payloads.
- Audit log access must be role-restricted.
- Audit records should support investigation without revealing cheat-detection internals.
- Retention policy is a T3 design-level decision.
- Audit log schema changes require versioning.
- Audit failure for high-value actions must be handled explicitly.

---

## Audit Log Integrity

### Audit Integrity Record

```md
## Audit Log Integrity

- Audit stream:
- Append-only mechanism:
- Tamper-evidence:
- Access roles:
- Redaction:
- Retention:
- Backup:
- Failure behavior:
- Owner:
```

### Integrity Rules

- Normal gameplay systems should not be able to mutate past audit records.
- Audit writes should be resilient to transient failure.
- If audit write fails for a high-value action, behavior must be defined:
  - block action,
  - queue audit event,
  - retry,
  - fallback to local secure buffer,
  - or owner-approved degraded mode.
- Do not silently drop high-value audit events.

---

## Log Redaction and Privacy

### Privacy Classes

```text
PUBLIC_DIAGNOSTIC
PSEUDONYMOUS_SECURITY_EVENT
PRIVATE_PLAYER_DATA
SENSITIVE_SECURITY_SIGNAL
SECRET
```

### Redaction Rules

- Never log `SECRET`.
- Avoid logging `PRIVATE_PLAYER_DATA`.
- Use hashes or pseudonymous IDs for player/account/session identifiers.
- Do not log raw packet payloads unless explicitly approved for secure debug capture.
- Do not log exact anti-cheat thresholds in player-facing or low-trust logs.
- Do not include validation details in client responses.

### Log Review Record

```md
## Netcode Log Review

- Log event:
- Privacy class:
- Fields:
- Redaction:
- Rate limit:
- Retention:
- Access:
- Verdict:
```

---

## Suspicious Behavior Severity

Use severity labels for suspicious network/anti-cheat events:

```text
NC-S1 — Critical
Confirmed or highly likely exploit path affecting authoritative state, high-value economy/reputation/inventory, private data, or production integrity.

NC-S2 — High
Repeated impossible actions, validation bypass attempt, suspicious item/reputation/currency request, or severe movement/hit anomaly.

NC-S3 — Medium
Single suspicious anomaly, repeated rate-limit exceedance, malformed packet patterns, or possible false-positive candidate.

NC-S4 — Low
Benign invalid input, stale packet, normal packet loss/reordering side effect, or debug-only anomaly.
```

### Severity Rules

- High-value state mutation attempts are at least `NC-S2`.
- Secret/private data exposure is `NC-S1`.
- Unvalidated client value entering server state is `NC-S1`.
- Repeated impossible movement or hit claims are at least `NC-S2`.
- Severity can be downgraded only after review.

---

## Enforcement Boundary

### Enforcement Policy

Validation and logging do not automatically imply punishment.

```text
Validation failure -> reject/drop.
Suspicious signal -> log/evidence.
Repeated or severe suspicious signal -> review/escalation.
Enforcement action -> approved anti-cheat policy.
```

### Enforcement Review Record

```md
## Anti-Cheat Enforcement Review

- Signal:
- Severity:
- Evidence:
- False-positive risk:
- Recommended action:
  - none
  - monitor
  - warn
  - soft restriction
  - temporary suspension
  - permanent ban
  - manual review
- Approved by:
- Player-facing communication:
```

### Enforcement Rules

- Do not auto-ban from one ambiguous signal.
- Do not reveal detection method.
- Manual/security review required for severe enforcement.
- Player-facing communication must be generic and policy-approved.

---

## Security Review Checklist

For each client-to-server message or high-value action:

```md
## Netcode Security Review: [Message / Feature]

- Active tier:
- Authority owner:
- Client input classified:
- Gameplay-critical state affected:
- Validation checks:
- Rate limit:
- Rejection path:
- Anti-cheat surface:
- Audit required:
- Audit schema:
- Redaction/privacy:
- Replay/spoofing protection:
- False-positive risk:
- Tests:
- Review owner:
- Verdict:
```

### Verdicts

```text
SECURITY_PASS
SECURITY_PASS_WITH_RISKS
SECURITY_BLOCKED
SECURITY_UNKNOWN
```

### Blocking Conditions

Mark `SECURITY_BLOCKED` if:

- client can mutate gameplay-critical state,
- inbound validation is missing,
- rate limit is missing,
- high-value action lacks audit plan,
- validation details are exposed to client,
- audit logs expose secrets/private data,
- suspicious behavior cannot be detected for a major anti-cheat surface.

---

## Red-Team and QA Testing

### Required Test Categories

At T2+ where relevant:

- malformed packet size,
- invalid field range,
- NaN/Infinity numeric value,
- impossible action while dead/stunned/out of range,
- rate-limit exceedance,
- repeated action replay,
- stale sequence/timestamp,
- client-reported hit fraud,
- impossible movement delta,
- client item grant attempt,
- client reputation change attempt,
- duplicate reward claim,
- audit log write,
- audit log redaction,
- rejection response does not reveal validation detail.

### Test Record

```md
## Netcode Integrity Test

- Test ID:
- Category:
- Message/action:
- Input:
- Expected server behavior:
- Expected client-visible behavior:
- Expected log/audit behavior:
- Actual result:
- Status:
- Evidence:
```

### Test Rules

- Invalid inputs must be tested.
- High-value audit actions must be tested.
- Client-visible failure responses must be checked for information leakage.
- Red-team tests should not include real secrets or private player data.
- Do not claim security readiness without test or review evidence.

---

## Incident Response

### Incident Types

```text
CLIENT_AUTHORITY_BYPASS
VALIDATION_MISSING
VALIDATION_DETAIL_LEAK
RATE_LIMIT_MISSING
HIT_REGISTRATION_BYPASS
MOVEMENT_TELEPORT_BYPASS
ITEM_GRANT_BYPASS
REPUTATION_CHANGE_BYPASS
AUDIT_LOG_MISSING
AUDIT_LOG_MUTABLE
AUDIT_LOG_PRIVACY_LEAK
LOG_FLOODING
REPLAY_OR_SPOOFING_RISK
FALSE_POSITIVE_SPIKE
TIER_GATE_BYPASS
```

### Incident Record

```md
## Netcode Integrity Incident

- Incident type:
- Severity:
- Detected by:
- Affected message/system:
- Player exposure:
- State corruption:
- Data exposure:
- Containment:
- Correction:
- Audit/log impact:
- Owner:
- Review outcome:
- Follow-up lesson:
```

### Incident Rules

- Unvalidated client value entering server state is `NC-S1`.
- Private data or secret logging is `NC-S1`.
- Missing audit log for high-value shipped action is at least `NC-S2`.
- Do not delete evidence without security owner approval.
- Redact sensitive data in incident reports.

---

## Self-Learning Protocol

Self-learning means controlled improvement from security reviews, red-team tests, QA findings, suspicious activity reviews, audit-log reviews, incident reports, and user corrections.

It does not mean hidden memory updates, autonomous anti-cheat policy changes, model training, or automatic punishment-rule changes.

### What May Be Learned

The netcode integrity rule system may learn:

- approved authority rules,
- approved validation patterns,
- approved rate-limit classes,
- known malformed packet patterns,
- known hit-registration abuse patterns,
- known movement anomaly patterns,
- item grant validation lessons,
- reputation audit lessons,
- audit schema improvements,
- redaction findings,
- false-positive findings,
- incident lessons,
- rejected unsafe approaches and why.

### What Must Not Be Learned or Stored

Do not store:

- secrets,
- credentials,
- tokens,
- private keys,
- private player data,
- raw sensitive packet payloads,
- raw private logs,
- private chain-of-thought,
- detailed exploit recipes outside approved security docs,
- anti-cheat internals in low-trust documentation,
- one-off false positives as universal rules,
- temporary prototype shortcuts as production rules.

### Lesson Classification

Use:

```text
Confirmed Rule
Approved Netcode Integrity Standard
Authority Finding
Validation Finding
Rate Limit Finding
Hit Registration Finding
Movement Finding
Item Grant Finding
Reputation Finding
Audit Log Finding
Redaction Finding
Replay/Spoofing Finding
False Positive Finding
Security Review Finding
Red-Team Finding
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
docs/networking/netcode-integrity.md
docs/networking/authority-lessons.md
docs/networking/validation-lessons.md
docs/networking/audit-log-policy.md
docs/security/netcode-findings.md
RED_TEAM.md
SECURITY.md
tasks/lessons.md
production/qa/networking/
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
- it applies to netcode integrity,
- it does not include sensitive data,
- it does not expose exploit instructions,
- it is not overgeneralized,
- it does not conflict with current security policy,
- it has an owner or review trigger.

### Lesson Expiry

Review or expire lessons when:

- network model changes,
- transport changes,
- authority model changes,
- anti-cheat policy changes,
- audit retention policy changes,
- privacy requirements change,
- red-team findings contradict the lesson,
- incident review supersedes it,
- Security Engineer supersedes it,
- the lesson was temporary,
- the lesson is too broad.

---

## Self-Healing Protocol

Self-healing means detecting a netcode integrity failure, containing risk, repairing safely, verifying the repair, and reporting what changed.

### Failure Types

Monitor for:

- client-authored gameplay-critical state,
- missing validation,
- missing range check,
- missing action legality check,
- missing rate limit,
- validation detail leaked to client,
- missing hit replay/validation,
- movement delta not capped,
- teleport anomaly not logged,
- client item grant path,
- client reputation mutation path,
- missing audit log,
- mutable audit log,
- audit log privacy leak,
- log flooding,
- missing T2 activation gate,
- missing T3 retention decision,
- missing red-team/QA evidence.

### Recovery Loop

When failure occurs:

1. **Stop**
   - Do not mark netcode safe.

2. **Identify**
   - State the exact trust-boundary failure.

3. **Classify**
   - Authority, validation, rate limit, anti-cheat surface, audit log, privacy, tier gate, or evidence issue.

4. **Contain**
   - Mark status:
     - `BLOCKED`,
     - `CLIENT_TRUST_RISK`,
     - `VALIDATION_MISSING`,
     - `RATE_LIMIT_MISSING`,
     - `AUDIT_REQUIRED`,
     - `AUDIT_RETENTION_UNRESOLVED`,
     - `SECURITY_REVIEW_REQUIRED`.

5. **Recover**
   - convert client fact to intent,
   - add server validation,
   - add rate limit,
   - change rejection path to log/drop,
   - remove detailed client response,
   - add audit log,
   - redact audit fields,
   - add append-only/tamper-evident behavior,
   - add tests,
   - escalate to Security Engineer.

6. **Verify**
   - Re-run or request unit/integration/red-team tests.
   - Review authority and audit evidence.
   - Confirm no information leak.

7. **Report**
   - Summarize issue, fix, remaining risk, and owner.

8. **Learn**
   - Propose durable lesson only if validated and approved.

---

## Error Recovery

### Client-Authored State

If client-authored value enters server state:

- block the path,
- convert message to intent,
- compute result server-side,
- validate all inputs,
- add test proving client cannot author state,
- create incident record if production-exposed.

### Missing Validation

If validation is missing or incomplete:

- add validation record,
- validate size, range, context, ownership, state, rate, sequence, and replay risk,
- reject invalid messages before mutation,
- add malformed input tests.

### Validation Detail Leak

If server returns detailed rejection reasons:

- replace with generic client-visible response,
- keep detailed reason in safe server-side logs only,
- review for information leakage.

### Missing Rate Limit

If client action has no rate limit:

- define sustained and burst limits,
- implement enforcement,
- log excess safely,
- test repeated request behavior.

### Hit Registration Risk

If client-reported hits are trusted:

- make client hit report a suggestion,
- replay or validate on server,
- compute authoritative damage server-side,
- log impossible claims.

### Movement Risk

If movement delta is not capped:

- add server delta/speed/acceleration checks,
- account for latency and allowed abilities,
- flag teleport-like jumps,
- reconcile client state.

### Item Grant Risk

If client can grant or duplicate items:

- move grant authority server-side,
- validate source and eligibility,
- add idempotency/deduplication,
- audit high-value grants.

### Reputation Risk

If client can change reputation:

- reject client mutation,
- require server-side cause,
- audit threshold changes,
- test replay/duplicate requests.

### Audit Log Failure

If high-value action lacks audit:

- add audit event,
- include required fields,
- ensure append-only/tamper-evident path,
- define failure behavior,
- mark retention unresolved until T3 policy exists.

### Sensitive Audit Logging

If audit/log output includes private or sensitive data:

- stop logging unsafe fields,
- redact or hash identifiers,
- escalate Security/Privacy review,
- update log schema,
- add redaction test.

### Log Flooding

If invalid packets flood logs:

- add rate limits,
- aggregate repeated events,
- log summary counts,
- preserve high-value incident signals.

---

## Memory Policy

### Short-Term Task Memory

Track during current task:

- active tier,
- network model,
- message/action,
- authority owner,
- client input classification,
- validation requirements,
- rate limit,
- rejection behavior,
- anti-cheat surface,
- audit requirement,
- audit fields,
- privacy class,
- tests,
- open decisions,
- approvals needed.

Short-term memory expires after the task unless explicitly stored.

### Project Memory

Project memory may store:

- approved authority rules,
- validation conventions,
- rate-limit classes,
- anti-cheat review findings,
- audit log schema decisions,
- redaction policies,
- false-positive lessons,
- incident lessons,
- rejected approaches.

### Never Store

Never store:

- secrets,
- credentials,
- tokens,
- private keys,
- private player data,
- raw sensitive payloads,
- private chain-of-thought,
- anti-cheat internals outside approved security docs,
- detailed exploit steps in general docs,
- temporary prototype trust shortcuts as production standards.

---

## Feedback Policy

When the user, Security Engineer, Technical Director, Network Programmer, Lead Programmer, QA Lead, Producer, or Privacy/Legal owner corrects netcode integrity behavior:

1. Accept the correction.
2. Identify whether it affects:
   - authority,
   - validation,
   - rate limits,
   - rejection path,
   - hit registration,
   - movement validation,
   - item grants,
   - reputation changes,
   - audit logging,
   - redaction,
   - retention,
   - anti-cheat signals,
   - enforcement boundary,
   - tests.
3. Revise current output.
4. Ask whether the correction should become durable netcode-integrity guidance if reusable.
5. Store only if approved and evidence-backed.

---

## Tool-Use Policy

This rules file does not grant tools by itself. Agents applying it must follow their own tool permissions.

General guidance:

- Use file-reading tools to inspect networking code, security docs, red-team docs, audit schemas, validation code, tests, and QA evidence.
- Use search tools to find client-to-server messages, validation paths, rate limits, audit writes, item grants, reputation changes, hit registration, and movement validation.
- Use write/edit tools only after approval under the active agent’s workflow.
- Use Bash only if the active agent allows it and only under that agent’s safety policy.
- Do not run servers, packet fuzzing, red-team scripts, production log queries, external network commands, or audit exports without explicit approval.
- Do not use Bash to bypass write/edit approval.

---

## Safety Guardrails

Never allow T2+ production netcode to:

- let clients author gameplay-critical state,
- trust client-reported hits,
- trust client-reported damage,
- trust client-granted items,
- trust client reputation changes,
- accept unbounded packet sizes,
- accept NaN/Infinity/out-of-domain fields,
- omit action-legality checks,
- omit rate limits,
- echo validation failure details to clients,
- log secrets or private data,
- omit audit logs for high-value state changes,
- store mutable audit logs without review,
- turn anti-cheat signals into punishment without approved policy,
- claim security readiness without review/test evidence.

---

## Output Standards

Netcode integrity reviews should be:

- tier-aware,
- authority-aware,
- trust-boundary-specific,
- validation-specific,
- anti-cheat-surface-aware,
- audit-aware,
- privacy-aware,
- evidence-backed,
- explicit about unresolved decisions.

### Review Output Format

```md
## Netcode Integrity Review: [Message / Feature / System]

### Verdict

SECURITY_PASS | SECURITY_PASS_WITH_RISKS | SECURITY_BLOCKED | SECURITY_UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Active Tier

### Authority Status

### Client Input Classification

### Validation Status

### Rate Limit Status

### Rejection Path

### Anti-Cheat Surface

### Audit Logging

### Redaction / Privacy

### Tests / Red-Team Evidence

### Required Follow-Up
```

---

## Reflection Checklist

After reviewing or drafting netcode integrity work, privately check:

- Is this T2+ or inert T1?
- Did I identify authoritative owner?
- Is client input intent, not truth?
- Are size and field ranges bounded?
- Are NaN/Infinity rejected?
- Is action legality checked?
- Is ownership/session state checked?
- Is rate limit defined?
- Does rejection log and drop?
- Are detailed validation reasons hidden from client?
- Are hit registration, movement, item grants, and reputation changes server-authoritative?
- Are high-value actions audit-logged?
- Are logs redacted and rate-limited?
- Is retention policy resolved or marked unresolved?
- Are tests/red-team evidence required?
- Did I avoid storing sensitive lessons?

Do not expose private chain-of-thought. Report only findings, evidence, and recommendations.

---

## Evaluation Checklist

Before final approval of T2+ netcode integrity behavior:

### Authority

- [ ] Gameplay-critical state is server-authoritative.
- [ ] Client messages are classified as intent/suggestion/cosmetic/forbidden claim.
- [ ] Client-authored values do not enter server state without validation.
- [ ] Client prediction reconciles with server state.

### Validation

- [ ] Packet size bounded.
- [ ] Message version checked.
- [ ] Required fields checked.
- [ ] Field ranges checked.
- [ ] NaN/Infinity rejected.
- [ ] Action legality checked.
- [ ] Ownership checked.
- [ ] Session membership checked.
- [ ] Cooldown/resource/status checked.
- [ ] Rate limit enforced.
- [ ] Replay/spoofing risk considered.

### Rejection

- [ ] Invalid messages are logged and dropped.
- [ ] Client-visible response is generic.
- [ ] Validation details are not echoed.
- [ ] Logs are rate-limited.
- [ ] Logs are redacted.

### Anti-Cheat Surfaces

- [ ] Hit registration is server-validated/replayed.
- [ ] Movement deltas are capped.
- [ ] Teleport-style jumps are flagged.
- [ ] Item grants are server-side only.
- [ ] Reputation changes are server-side only.
- [ ] Suspicious signals are evidence, not automatic punishment.

### Audit Logging

- [ ] High-value actions have audit records.
- [ ] Required fields exist.
- [ ] Audit log is append-only or tamper-evident.
- [ ] Privacy class is assigned.
- [ ] IDs are pseudonymized where appropriate.
- [ ] Retention policy is resolved or marked unresolved.
- [ ] Audit failure behavior is defined.

### Evidence

- [ ] Security review evidence exists or is requested.
- [ ] Red-team tests exist or are planned.
- [ ] QA validation exists or is planned.
- [ ] Missing evidence is clearly marked.
- [ ] No unsupported security-readiness claim is made.

---

## Example Workflows

### Example 1: Client-Reported Hit

Issue:

```text
Client sends `targetId` and `hit=true`; server applies damage.
```

Response:

```md
Finding: Client-reported hit is treated as fact.

Severity: NC-S1.

Fix:
- Treat client report as aim/input suggestion.
- Server replays or validates shot using server-side state.
- Validate target, range, line of sight, cooldown, weapon state, and timestamp.
- Server computes hit and damage.
- Log repeated impossible hit claims safely.
```

---

### Example 2: Client Damage Value

Issue:

```text
Client sends `damageAmount=999`.
```

Response:

```md
Finding: Client-authored gameplay-critical value.

Severity: NC-S1.

Fix:
- Reject client damage value.
- Server computes damage from authoritative weapon/ability/config state.
- Log validation failure category without echoing details to client.
```

---

### Example 3: Movement Teleport

Issue:

```text
Client position jumps 50 meters in one tick without approved ability.
```

Response:

```md
Finding: Movement anomaly.

Fix:
- Server checks max speed, max acceleration, ability modifiers, and time window.
- If invalid, reject or correct movement.
- Log teleport-style jump as suspicious with rate limit.
```

---

### Example 4: Client Item Grant

Issue:

```text
Client sends `addItem("rare_sword")`.
```

Response:

```md
Finding: Forbidden authoritative item grant.

Severity: NC-S1 or NC-S2 depending on exposure.

Fix:
- Replace with request such as `claimLoot(lootDropId)`.
- Server validates loot source, ownership, eligibility, quantity, inventory capacity, and duplicate state.
- Server grants item and writes audit event.
```

---

### Example 5: Reputation Change

Issue:

```text
Client sends `reputationDelta=+100`.
```

Response:

```md
Finding: Client-authored reputation change.

Fix:
- Reject direct reputation mutation.
- Reputation changes require server-side cause.
- Changes above threshold are append-only audit logged with actor, target, action, delta, cause, server_timestamp, and server_tick.
```

---

### Example 6: Validation Detail Leak

Issue:

```text
Server responds: “Rejected because speed delta exceeded 7.5m/s threshold.”
```

Response:

```md
Finding: Validation detail leak.

Fix:
- Replace with generic client response.
- Keep detailed reason only in safe server logs.
- Review logs for anti-cheat threshold exposure.
```

---

### Example 7: Missing Rate Limit

Issue:

```text
Ability activation RPC has legality checks but no rate limit.
```

Response:

```md
Finding: Rate-limit gap.

Fix:
- Define sustained and burst limits per player/session.
- Drop or reject excess requests safely.
- Log repeated exceedance as suspicious with rate limit.
```

---

### Example 8: Audit Retention Unresolved

Issue:

```text
Audit logging stores reputation events indefinitely, but no retention policy exists.
```

Response:

```md
Finding: Audit retention unresolved.

Fix:
- Mark `AUDIT_RETENTION_UNRESOLVED`.
- Define T3 retention, access, redaction, and deletion policy before long-term retention is enabled.
```

---

### Example 9: Unsafe Audit Log

Issue:

```text
Audit log stores raw account email and IP address for item grants.
```

Response:

```md
Finding: Audit privacy risk.

Fix:
- Replace with approved pseudonymous player/account/session hashes.
- Escalate Security/Privacy review.
- Define retention and access roles.
```

---

### Example 10: User Correction

User says:

```text
Reputation changes above 25 points require audit, below that only security telemetry.
```

Response pattern:

```text
Understood. I’ll treat reputation deltas above 25 as audit-required and smaller deltas as security telemetry unless superseded by T3 retention or security policy. Store this as durable netcode-integrity guidance only if approved.
```

---

### Example 11: Case Where the Rule Must Not Learn

User says during a prototype:

```text
Trust client position for now so movement feels better.
```

Response pattern:

```text
Treat that as prototype-only. Production T2+ netcode must keep movement server-authoritative or server-validated, with reconciliation and suspicious delta logging.
```

---

## See Also

- `.claude/rules/network-code.md` — network implementation, replication strategy, bandwidth budgets.
- `RED_TEAM.md` §7 — server-authoritative validation rubric.
- `RED_TEAM.md` §8 — reputation and audit review.
- `SECURITY.md` — threats #2, #4, #5, #6.
- `DECISIONS.md` D002 — FishNet deferred to T2.
- `DECISIONS.md` D003 — networking absent during T1.
- `docs/security/netcode-findings.md` — recommended security finding archive.
- `docs/networking/netcode-integrity.md` — recommended durable netcode integrity standard.

---

## Final Netcode Integrity Rule

T2+ netcode must be:

- server-authoritative,
- hostile to untrusted client input,
- validation-complete,
- rate-limited,
- rejection-safe,
- anti-cheat-aware,
- audit-backed for high-value actions,
- redacted and privacy-aware,
- non-leaking in client responses,
- reviewable,
- tested against abuse cases,
- and honest about unresolved security evidence.