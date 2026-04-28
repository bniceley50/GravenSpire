---
paths:
  - "src/networking/**"
---

# Network Code Rules

## Rule Set Name

Network Code Rules

## Mission

These rules govern all network code under:

```text
src/networking/**
```

Their purpose is to ensure multiplayer networking is authoritative, secure, versioned, bandwidth-conscious, prediction-safe, rollback-capable, resilient to real-world network conditions, testable, and observable.

Network code is part of the project’s trust boundary. Bugs here can create cheating vectors, desyncs, crashes, corrupted sessions, bad player experience, data leaks, and release-blocking instability.

The core network-code question is:

> Does this code preserve server authority, validate every untrusted input, replicate only what is necessary, recover from network failure, and remain playable under latency, jitter, loss, and version mismatch?

---

## Operating Principles

1. **Server authority is mandatory**
   - The server or authoritative host owns all gameplay-critical state.
   - Clients send intent, not truth.
   - Client-provided state is untrusted until validated.

2. **Never trust the client**
   - Validate incoming packet sizes, versions, ownership, field ranges, state transitions, timing, sequence numbers, and rate limits.
   - Reject invalid, stale, impossible, or unauthorized messages safely.

3. **Every message is a versioned contract**
   - All network messages must include a version or participate in a versioned protocol.
   - Breaking changes require compatibility, migration, or explicit rejection behavior.

4. **Prediction is local; truth is authoritative**
   - Clients may predict for responsiveness.
   - The server validates and corrects.
   - Reconciliation and rollback must be explicit and bounded.

5. **Replication strategy must be declared**
   - Every networked value must specify:
     - reliable or unreliable,
     - ordered or unordered,
     - send frequency,
     - interpolation behavior,
     - prediction behavior,
     - authority owner,
     - bandwidth budget.

6. **Bandwidth is a budget, not a wish**
   - Track bandwidth by message type.
   - Avoid per-frame RPCs.
   - Use relevancy, priority, delta compression, quantization, batching, and send-rate limits.

7. **Failures are normal**
   - Disconnection, reconnection, timeout, version mismatch, packet loss, packet reordering, and host migration must have defined behavior.
   - If host migration is unsupported, state that explicitly.

8. **Logging must help without flooding or leaking**
   - Network logs must be structured, rate-limited, redacted, and safe for production diagnostics.
   - Do not log secrets, tokens, private player data, raw sensitive payloads, or cheat-detection internals.

9. **LAN success is not online validation**
   - Validate under latency, jitter, packet loss, packet duplication, packet reordering, bandwidth limits, disconnects, reconnects, and version mismatch.

10. **Self-healing before release**
    - When code violates authority, validation, versioning, prediction, bandwidth, logging, or failure-recovery rules, stop, contain the issue, repair safely, verify, and report.

11. **Bounded self-learning**
    - Durable lessons from QA, bandwidth reports, desync investigations, security reviews, incident reports, and user corrections may be stored only in approved reviewable locations.
    - Lessons must be explicit, reversible, and subordinate to current project rules and architecture decisions.

---

## Scope

These rules apply to:

```text
src/networking/**
```

This includes, where present:

- transport adapters,
- packet/message schemas,
- RPC definitions,
- serialization and deserialization,
- replication systems,
- prediction systems,
- reconciliation systems,
- rollback buffers,
- interpolation systems,
- connection lifecycle,
- session lifecycle,
- lobby/matchmaking network glue,
- disconnect/reconnect logic,
- host migration logic,
- bandwidth measurement,
- network logging,
- packet validation,
- network simulation test code,
- network security validation hooks.

---

## Non-Goals

These rules do not authorize network code to:

- Design gameplay mechanics.
- Own production server infrastructure.
- Make final security architecture decisions alone.
- Make anti-cheat policy decisions.
- Modify unrelated gameplay logic.
- Store private player data in logs.
- Bypass moderation, security, or privacy policies.
- Change transport provider, relay provider, or backend vendor without Technical Director approval.
- Claim online readiness from local or LAN-only tests.
- Edit files without the active agent’s approval workflow.
- Store persistent lessons without approval.

---

## Network Code State Labels

Use these labels when reviewing or implementing network code:

```text
PROPOSED — suggested but not approved.
SPEC_READY — message/replication/lifecycle contract is documented.
IMPLEMENTED — code exists.
LOCAL_TESTED — tested locally or LAN-only.
SIMULATED_NETWORK_TESTED — tested under latency/jitter/loss simulation.
BANDWIDTH_PROFILED — bandwidth measured by message type.
SECURITY_REVIEWED — trust boundary and validation reviewed.
QA_VERIFIED — QA validated expected behavior.
LIVE_VALIDATED — validated in production/live or live-like environment.
AUTHORITY_SAFE — authority ownership verified.
VALIDATION_READY — input validation rules defined.
VERSIONED — message/protocol version defined.
ROLLBACK_READY — rollback behavior defined and testable.
RECONNECT_READY — reconnect behavior defined and testable.
HOST_MIGRATION_UNSUPPORTED — explicitly unsupported.
HOST_MIGRATION_READY — host migration defined and testable.
BLOCKED — missing spec, validation, test, owner, or approval.
SUPERSEDED — replaced by newer code/spec.
DEPRECATED — still present but not for new use.
```

### State Rules

- Do not mark `AUTHORITY_SAFE` without authority review.
- Do not mark `VERSIONED` without schema/protocol evidence.
- Do not mark `SIMULATED_NETWORK_TESTED` without network-condition evidence.
- Do not mark `BANDWIDTH_PROFILED` without measured per-message data.
- Do not mark `HOST_MIGRATION_READY` without test evidence.
- `LOCAL_TESTED` is not production multiplayer validation.

---

## Source of Truth

Recommended files and directories:

```text
src/networking/
design/network/
design/network/message-schemas.md
design/network/replication-strategy.md
design/network/authority-model.md
design/network/prediction-reconciliation.md
design/network/session-lifecycle.md
design/network/bandwidth-budgets.md
design/network/network-security.md
tests/unit/networking/
tests/integration/networking/
production/qa/networking/
production/session-state/lessons.md
docs/architecture/
DECISIONS.md
```

### Source-of-Truth Rules

- Check architecture decisions before changing authority, protocol, transport, or session lifecycle.
- Check message schema docs before adding or changing messages.
- Check replication strategy docs before adding replicated values.
- Check security docs before changing client-to-server validation.
- Check QA/network test evidence before claiming network readiness.
- If source docs and implementation conflict, flag the conflict.

---

## Authority Model

### Gameplay-Critical State

Server-authoritative by default:

- health,
- damage,
- hit results,
- player position where fairness-sensitive,
- inventory,
- currency,
- rewards,
- cooldowns,
- ability state,
- objective state,
- quest state,
- progression,
- match state,
- match results,
- ranked/competitive outcomes,
- loot grants,
- economy transactions,
- save-relevant multiplayer state.

### Client Intent

Clients may send intent, such as:

- movement input,
- aim direction,
- interact request,
- ability activation request,
- emote request,
- ready-state request,
- cosmetic selection request,
- UI-driven session request.

### Authority Record

```md
## Authority Record: [State / System]

- State/system:
- Authoritative owner:
  - Server
  - Dedicated server
  - Listen host
  - Platform service
  - Client, cosmetic-only
- Client input allowed:
- Server validation:
- Replication target:
- Prediction allowed:
- Rollback required:
- Failure behavior:
- Security risk:
- Evidence:
```

### Authority Rules

- Clients never authoritatively report gameplay-critical outcomes.
- Client requests must be validated by the server.
- Cosmetic-only client authority must be explicitly labeled.
- Reconnect restores state from server-authoritative snapshot, not client cache.
- Authority exceptions require Technical Director and Security Engineer review.

---

## Trust Boundary and Packet Validation

### Packet Validation Checklist

Every incoming packet/message must validate:

- packet size,
- message type,
- message version,
- required fields,
- field types,
- field ranges,
- enum values,
- sequence number,
- timestamp/window,
- sender identity,
- session membership,
- ownership,
- authority,
- current game/session state,
- cooldown/timing,
- rate limit,
- replay/spoofing risk,
- payload length,
- string length and encoding,
- reference IDs exist,
- target is relevant/visible/valid where applicable.

### Packet Validation Record

```md
## Packet Validation: [Message Name]

- Message:
- Direction:
- Max packet size:
- Required fields:
- Field ranges:
- Sender validation:
- Ownership validation:
- State validation:
- Rate limit:
- Replay protection:
- Failure behavior:
- Logging:
- Tests:
```

### Validation Rules

- Reject invalid packets before applying state.
- Never allocate unbounded memory based on packet contents.
- Never trust payload length without bounds.
- Never deserialize into live state before validation.
- Invalid packets must not crash the session.
- Suspicious packets should be logged safely and rate-limited.
- Validation failures must not reveal cheat-detection internals.

---

## Network Message Schema

### Message Contract Format

```md
## Network Message: [MessageName]

- Status:
- Message ID:
- Version:
- Direction:
  - Client -> Server
  - Server -> Client
  - Server -> All
  - Peer -> Peer
- Purpose:
- Authority:
- Reliability:
  - Reliable
  - Unreliable
  - Sequenced
  - Ordered
  - Unordered
- Channel:
- Send frequency:
- Max size:
- Rate limit:
- Prediction:
- Reconciliation:
- Rollback:
- Interpolation:
- Validation:
- Failure behavior:
- Logging:
- Bandwidth budget:
- Tests:

### Payload

| Field | Type | Required | Range / Allowed Values | Default | Notes |
|---|---|---|---|---|---|
```

### Message Rules

- Every message must have a purpose.
- Every message must have a version.
- Every client-to-server message must define validation.
- Every repeated message must define rate limits.
- Every high-frequency message must define bandwidth budget.
- Every state-bearing message must define authority.
- Frequent cosmetic messages should usually be unreliable or locally derived.
- Reliable messages must be used deliberately.

---

## Message Versioning and Compatibility

### Versioning Record

```md
## Message Versioning: [MessageName]

- Current version:
- Previous versions:
- Compatibility:
  - Backward compatible
  - Forward compatible
  - Breaking
- Added fields:
- Removed fields:
- Renamed fields:
- Type changes:
- Default behavior:
- Migration behavior:
- Version mismatch behavior:
- Tests:
```

### Versioning Rules

- Every network message must be versioned.
- Breaking changes require migration or explicit incompatibility handling.
- Unknown optional fields may be ignored if safe.
- Unknown required fields require rejection or safe fallback.
- Version negotiation happens during handshake or session join.
- Do not silently reinterpret old payloads as new schema.
- Old clients/servers must fail safely when incompatible.

---

## Replication Strategy

### Replicated Value Record

```md
## Replicated Value: [ValueName]

- System:
- Value:
- Authority:
- Replication strategy:
  - Reliable state
  - Unreliable snapshot
  - Delta replication
  - Owner-only
  - Event-based
  - Derived locally
  - Not replicated
- Frequency:
- Reliability:
- Interpolation:
- Extrapolation:
- Prediction:
- Reconciliation:
- Quantization:
- Relevancy:
- Bandwidth budget:
- Failure behavior:
- Tests:
```

### Replication Rules

- Replicate only what is necessary.
- Prefer derived local values over replicated derived values.
- Use owner-only replication for private state.
- Use delta replication for frequently changing state.
- Use quantization where precision can be reduced safely.
- Use relevancy and priority to avoid sending irrelevant state.
- Do not replicate entire arrays when one element changed.
- Do not use reliable replication for high-frequency state unless required.

---

## Reliability, Ordering, and Channels

### Channel Record

```md
## Network Channel: [ChannelName]

- Purpose:
- Reliability:
- Ordering:
- Message types:
- Max rate:
- Queue behavior:
- Drop behavior:
- Backpressure behavior:
- Tests:
```

### Reliability Rules

Use reliable delivery for:

- handshake,
- version negotiation,
- match start/end,
- inventory/economy authoritative results,
- confirmed rewards,
- critical session state,
- one-time state changes that cannot be derived.

Use unreliable or sequenced delivery for:

- movement snapshots,
- aim updates,
- animation hints,
- footstep/cosmetic effects,
- frequent transform updates,
- non-critical momentary VFX/audio events.

Avoid:

- reliable cosmetic spam,
- per-frame reliable RPCs,
- large reliable payloads,
- ordered delivery when order does not matter.

---

## Client Prediction, Reconciliation, and Rollback

### Prediction Record

```md
## Prediction / Reconciliation: [Action / State]

- Predicted action:
- Client input command:
- Sequence number:
- Prediction key:
- Local predicted state:
- Server authoritative response:
- Reconciliation method:
- Rollback buffer:
- Correction threshold:
- Smoothing behavior:
- Snap threshold:
- Failure behavior:
- Tests:
```

### Prediction Rules

- Predicted actions must have sequence numbers or prediction keys.
- Store enough input/state history for reconciliation.
- Server corrections must be applied deterministically where required.
- Small corrections should smooth.
- Large or invalid corrections may snap.
- Rollback buffer size must be bounded.
- Client prediction must not grant rewards, inventory, currency, progression, or match results.
- Prediction must not hide server rejection.

### Rollback Rules

Rollback data must define:

- what state is stored,
- how long it is stored,
- memory budget,
- replay method,
- invalidation behavior,
- mismatch handling.

---

## Interpolation and Extrapolation

### Interpolation Record

```md
## Interpolation Strategy: [Entity / Value]

- Snapshot source:
- Snapshot frequency:
- Interpolation delay:
- Buffer size:
- Extrapolation allowed:
- Max extrapolation time:
- Missing snapshot behavior:
- Teleport/snap threshold:
- Smoothing:
- Tests:
```

### Interpolation Rules

- Remote entities normally interpolate rather than predict.
- Interpolation buffers must absorb expected jitter.
- Extrapolation must be bounded.
- Missing snapshots must not freeze forever.
- Teleports, respawns, and authority changes need explicit snap behavior.
- Gameplay-critical state must prioritize correctness over smoothness.

---

## Bandwidth Budgeting

### Message Bandwidth Budget

```md
## Bandwidth Budget: [Mode / Platform]

- Mode:
- Platform:
- Max players:
- Tick rate:
- Snapshot rate:
- Per-client upstream budget:
- Per-client downstream budget:
- Server aggregate budget:
- Budget owner:
- Validation tool:
```

### Per-Message Bandwidth Table

```md
| Message Type | Direction | Frequency | Avg Size | p95 Size | Avg Bandwidth | p95 Bandwidth | Budget | Status |
|---|---|---:|---:|---:|---:|---:|---:|---|
```

### Bandwidth Rules

- Every message type needs bandwidth estimate or measurement.
- High-frequency messages need profiling evidence.
- Track average and p95 where possible.
- Budget failures require optimization or owner-approved waiver.
- Optimize using:
  - lower frequency,
  - smaller payload,
  - quantization,
  - delta compression,
  - batching,
  - relevancy,
  - priority,
  - local derivation,
  - unreliable/sequenced delivery.

---

## Connection Lifecycle

### Connection States

```text
DISCONNECTED
RESOLVING
CONNECTING
AUTHENTICATING
HANDSHAKING
VERSION_CHECK
JOINING_SESSION
SYNCING_STATE
READY
IN_MATCH
DISCONNECTING
RECONNECTING
TIMED_OUT
KICKED
FAILED
```

### Connection Lifecycle Record

```md
## Connection Lifecycle

- Entry point:
- Auth/session requirement:
- Handshake messages:
- Version negotiation:
- Timeout:
- Retry policy:
- Reconnect policy:
- Failure behavior:
- Player-facing fallback:
- Logging:
- Tests:
```

### Connection Rules

- Every blocking state needs timeout.
- Every retry path needs limit/backoff.
- Version mismatch must fail safely.
- Reconnect must restore server-authoritative state.
- Disconnect cleanup must release session resources.
- Player-facing errors must not expose internal protocol details.

---

## Reconnection Policy

### Reconnect Record

```md
## Reconnect Policy

- Mode:
- Reconnect window:
- Identity validation:
- State restored:
- State discarded:
- Authority source:
- Match impact:
- Timeout:
- Failure behavior:
- Tests:
```

### Reconnect Rules

- Reconnect uses server-authoritative state.
- Client cache is not trusted.
- Reconnect identity must be validated.
- Reconnect window must be bounded.
- Reconnect failure must leave session consistent.
- Reconnect must not duplicate inventory, rewards, or match results.

---

## Host Migration Policy

### Host Migration Status

Every networked mode must declare one of:

```text
HOST_MIGRATION_UNSUPPORTED
HOST_MIGRATION_NOT_APPLICABLE
HOST_MIGRATION_SUPPORTED
```

### Host Migration Record

```md
## Host Migration Policy

- Status:
- Applies to:
- Trigger:
- New host selection:
- State transfer:
- Authoritative state source:
- Timeout:
- Failure behavior:
- Anti-cheat/security risk:
- Tests:
```

### Host Migration Rules

- Do not imply host migration exists unless implemented and tested.
- Unsupported host migration must have player/session failure behavior.
- Migrated state must come from trusted authority.
- Host selection must be deterministic or service-controlled.
- Host migration requires security review.
- Host migration is not required for dedicated-server-only architectures, but unsupported/not-applicable status must be explicit.

---

## Network Logging

### Log Event Record

```md
## Network Log Event: [EventName]

- Event:
- Severity:
- Trigger:
- Fields:
- Redaction:
- Rate limit:
- Privacy class:
- Release behavior:
- Owner:
```

### Log Categories

Use:

```text
CONNECTION
HANDSHAKE
VERSION_MISMATCH
PACKET_VALIDATION_FAILED
RATE_LIMIT_EXCEEDED
AUTHORITY_REJECTED
REPLAY_SUSPECTED
DESYNC_DETECTED
RECONCILIATION_CORRECTION
BANDWIDTH_BUDGET_EXCEEDED
RECONNECT_ATTEMPT
HOST_MIGRATION_EVENT
```

### Logging Rules

- Rate-limit all network anomaly logs.
- Logs must not flood under packet spam.
- Logs must not expose secrets, auth tokens, private player data, or cheat-detection internals.
- Logs should include enough non-sensitive context for diagnosis:
  - message type,
  - session ID hash,
  - player ID hash if approved,
  - sequence number,
  - version,
  - validation category,
  - timestamp/frame.
- Release logs should be lower-detail than debug logs unless explicitly approved.

---

## Security and Abuse Controls

### Security Review Record

```md
## Network Security Review: [Message / System]

- Trust boundary:
- Client inputs:
- Server validation:
- Rate limits:
- Replay protection:
- Spoofing risk:
- Payload-size risk:
- Field-range risk:
- Sensitive data risk:
- Logging:
- Owner:
- Tests:
```

### Security Rules

- Validate every client-to-server message.
- Rate-limit every client-to-server message.
- Reject impossible state transitions.
- Detect repeated invalid messages.
- Do not reveal validation internals to clients.
- Coordinate with Security Engineer for:
  - replay/spoofing,
  - anti-cheat,
  - suspicious behavior logging,
  - privacy-sensitive telemetry,
  - punishment/escalation policy.

---

## Network Simulation Test Profiles

### Default Profiles

```text
LAN_BASELINE
- 0–10ms latency
- 0% packet loss
- negligible jitter

NORMAL_ONLINE
- 50ms latency
- 5–10ms jitter
- 0–1% packet loss

HIGH_LATENCY
- 150ms latency
- 20ms jitter
- 1–2% packet loss

BAD_WIFI
- 100ms latency
- 50ms jitter
- 3–5% packet loss
- occasional packet reordering

PACKET_REORDERING
- reordering enabled
- low/moderate latency
- validate sequence handling

BANDWIDTH_CONSTRAINED
- capped upstream/downstream
- validate priority and drop behavior

DISCONNECT_RECONNECT
- forced disconnect
- reconnect within approved window

VERSION_MISMATCH
- incompatible client/server message versions
```

### Network Test Record

```md
## Network Test Evidence

- Test profile:
- Build:
- Platform:
- Mode:
- Player count:
- Scenario:
- Expected result:
- Actual result:
- Status:
- Evidence:
```

### Test Rules

- LAN testing is baseline only.
- Online readiness requires simulated adverse conditions.
- Prediction/reconciliation must be tested at target latency.
- Packet validation must be tested with invalid payloads.
- Reconnect must be tested if supported.
- Host migration must be tested if supported.
- Bandwidth must be profiled at expected player count.

---

## Network Release Gate

### Release Gate Record

```md
## Network Release Gate: [Build / Version]

- Build:
- Platform:
- Mode:
- Max players tested:
- Profiles tested:
- Authority status:
- Validation status:
- Versioning status:
- Prediction/reconciliation status:
- Bandwidth status:
- Disconnect/reconnect status:
- Host migration status:
- Logging status:
- Security review status:
- Open blockers:
- Waivers:
- Verdict:
```

### Verdicts

```text
NETWORK_PASS
NETWORK_PASS_WITH_RISKS
NETWORK_BLOCKED
NETWORK_UNKNOWN
```

### Gate Rules

- Unvalidated client-authoritative gameplay-critical state is `NETWORK_BLOCKED`.
- Unmoderated packet parsing of client data is `NETWORK_BLOCKED`.
- Missing packet validation for gameplay-critical messages is `NETWORK_BLOCKED`.
- Bandwidth budget failure is at least `NETWORK_PASS_WITH_RISKS`.
- LAN-only test evidence produces `NETWORK_UNKNOWN`, not pass.
- Host migration unsupported is acceptable only if explicitly declared and product design accepts it.

---

## Network Code Review Format

Use this for reviews:

```md
## Network Code Review: [System/File]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Scope

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Authority Status

### Packet Validation Status

### Message Versioning Status

### Replication Strategy Status

### Prediction / Reconciliation / Rollback Status

### Disconnect / Reconnect / Host Migration Status

### Bandwidth Status

### Logging Status

### Security Status

### Test Evidence

### Required Follow-Up
```

### Severity

```text
NET-S1 — Critical
Can enable cheating, corrupt gameplay-critical state, crash sessions, leak private data, or display severe desync in core gameplay.

NET-S2 — High
Missing validation, missing versioning, bandwidth budget failure, unreliable reconciliation, unsafe reconnect, or untested critical message.

NET-S3 — Medium
Incomplete logging, weak bandwidth evidence, missing adverse-network test, unclear interpolation, partial schema documentation.

NET-S4 — Low
Documentation gap, naming issue, non-blocking diagnostic improvement.
```

---

## Self-Learning Protocol

Self-learning means controlled improvement from approved network reviews, desync investigations, bandwidth profiles, security reviews, QA findings, incident reports, and user corrections.

It does not mean hidden architecture changes, autonomous protocol mutation, or treating one noisy test as permanent truth.

### What May Be Learned

The network-code rule system may learn:

- approved authority patterns,
- approved message schema conventions,
- approved versioning patterns,
- common packet validation failures,
- bandwidth bottlenecks,
- prediction/reconciliation findings,
- rollback-buffer findings,
- reconnect findings,
- host migration findings,
- logging/rate-limit findings,
- security findings,
- rejected approaches and reasons.

### What Must Not Be Learned or Stored

Do not store:

- private player data,
- secrets,
- tokens,
- private keys,
- raw sensitive packet payloads,
- private chain-of-thought,
- anti-cheat internals outside approved security docs,
- one-off local test results as production truth,
- prototype shortcuts as production network rules,
- emergency waivers as normal policy.

### Lesson Classification

Use:

```text
Confirmed Rule
Approved Network Standard
Authority Finding
Packet Validation Finding
Message Versioning Finding
Replication Finding
Prediction Finding
Reconciliation Finding
Rollback Finding
Interpolation Finding
Bandwidth Finding
Logging Finding
Reconnect Finding
Host Migration Finding
Security Finding
QA Finding
Incident Finding
Validated Fix
Rejected Approach
Working Assumption
Temporary Context
Superseded
```

### Lesson Storage

Store durable lessons only in approved, reviewable locations such as:

```text
docs/networking/network-code-standards.md
docs/networking/message-schema-lessons.md
docs/networking/authority-lessons.md
docs/networking/bandwidth-findings.md
docs/networking/desync-findings.md
docs/networking/security-findings.md
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
- it applies to network code,
- it does not include sensitive data,
- it does not expose exploit instructions,
- it is not overgeneralized,
- it does not conflict with architecture/security decisions,
- it has a review trigger where appropriate.

### Lesson Expiry

Review or expire lessons when:

- network architecture changes,
- transport changes,
- protocol version changes,
- security requirements change,
- max player count changes,
- bandwidth budget changes,
- prediction/reconciliation architecture changes,
- QA evidence contradicts the lesson,
- incident review supersedes it,
- the lesson was temporary,
- the lesson is too broad.

---

## Self-Healing Protocol

Self-healing means detecting a network-code rule failure, containing the risk, repairing safely, verifying the repair, and reporting what changed.

### Failure Types

Monitor for:

- client-authoritative gameplay-critical state,
- missing packet validation,
- missing message version,
- unbounded packet size,
- unbounded string/payload field,
- field range not validated,
- missing rate limit,
- reliable message spam,
- per-frame RPC,
- missing replication strategy,
- missing interpolation strategy,
- missing prediction sequence number,
- missing rollback buffer,
- desync,
- reconciliation snapping,
- stale cache/state after reconnect,
- host migration assumed but unsupported,
- bandwidth budget missing,
- bandwidth budget exceeded,
- log flooding,
- sensitive data in logs,
- missing adverse-network tests.

### Recovery Loop

When failure occurs:

1. **Stop**
   - Do not mark the network code safe.

2. **Identify**
   - State the exact violation.

3. **Classify**
   - Authority, validation, versioning, replication, prediction, rollback, bandwidth, lifecycle, logging, security, or test evidence.

4. **Contain**
   - Mark status:
     - `BLOCKED`,
     - `AUTHORITY_UNSAFE`,
     - `VALIDATION_MISSING`,
     - `VERSIONING_MISSING`,
     - `BANDWIDTH_UNKNOWN`,
     - `NETWORK_UNKNOWN`,
     - `SECURITY_REVIEW_REQUIRED`.

5. **Recover**
   - move authority server-side,
   - add validation,
   - version message,
   - add rate limit,
   - define replication strategy,
   - add sequence/prediction keys,
   - define rollback buffer,
   - add interpolation buffer,
   - declare host migration status,
   - add bandwidth measurement,
   - rate-limit/redact logs,
   - add network simulation tests.

6. **Verify**
   - Re-run or request unit/integration/network simulation tests.
   - Re-check authority, validation, and bandwidth evidence.

7. **Report**
   - Summarize issue, fix, remaining risk, and owner.

8. **Learn**
   - Propose durable lesson only if validated and approved.

---

## Error Recovery

### Client-Authoritative State

If client controls gameplay-critical state:

- convert client message to intent,
- validate on server,
- compute authoritative result server-side,
- replicate server result,
- add invalid-client-state tests,
- request Security Engineer review.

### Missing Packet Validation

If packet validation is incomplete:

- define max size,
- validate field ranges,
- validate ownership,
- validate session state,
- validate sequence/timing,
- add rejection behavior,
- add tests for malformed packets.

### Missing Message Version

If message is unversioned:

- add version field or protocol version mapping,
- define mismatch behavior,
- add backward/forward compatibility notes,
- add version negotiation test if applicable.

### Reliable Message Spam

If frequent reliable messages cause backlog risk:

- move cosmetic/frequent messages to unreliable/sequenced,
- reduce send frequency,
- batch or drop low-priority updates,
- profile bandwidth and queue depth.

### Prediction/Reconciliation Failure

If client correction snaps or oscillates:

- check sequence numbers,
- check rollback buffer,
- check authoritative server response,
- define correction threshold,
- smooth small corrections,
- snap only unrecoverable divergence,
- test under high latency/jitter.

### Bandwidth Failure

If bandwidth exceeds budget:

- identify top message types,
- reduce frequency,
- quantize payloads,
- add delta compression,
- add relevancy,
- remove redundant replication,
- profile again.

### Reconnect Failure

If reconnect restores incorrect state:

- restore from server-authoritative snapshot,
- validate reconnect identity,
- discard stale client cache,
- enforce reconnect window,
- test disconnect/reconnect profile.

### Host Migration Ambiguity

If host migration is unclear:

- declare supported, unsupported, or not applicable,
- if supported, define state transfer and new-host selection,
- if unsupported, define player-facing failure behavior,
- require test evidence before marking ready.

### Log Flooding

If logs can flood:

- add rate limit,
- aggregate repeated events,
- downgrade repeated duplicates,
- include safe diagnostic metadata only.

### Sensitive Logging

If logs include secrets/private data:

- stop logging,
- redact,
- escalate to Security Engineer,
- rotate secrets if exposed,
- add log-redaction test or review.

---

## Memory Policy

### Short-Term Task Memory

Track during current task:

- message/system,
- authority owner,
- packet fields,
- validation rules,
- message version,
- replication strategy,
- prediction/reconciliation behavior,
- bandwidth budget,
- connection lifecycle,
- logging behavior,
- tests,
- open questions,
- approvals needed.

Short-term memory expires after task completion unless explicitly stored.

### Project Memory

Project memory may store:

- approved authority conventions,
- message schema patterns,
- versioning rules,
- validation patterns,
- bandwidth findings,
- reconciliation findings,
- reconnect/host migration lessons,
- logging/rate-limit rules,
- security findings,
- validated fixes,
- rejected approaches.

### Never Store

Never store:

- secrets,
- credentials,
- tokens,
- private keys,
- private player data,
- raw sensitive packet payloads,
- private chain-of-thought,
- anti-cheat internals outside approved security docs,
- unsupported bandwidth or validation claims,
- prototype shortcuts as production rules.

---

## Feedback Policy

When the user, Technical Director, Lead Programmer, Network Programmer, Security Engineer, QA Lead, Performance Analyst, DevOps Engineer, or Gameplay Programmer corrects network-code behavior:

1. Accept the correction.
2. Identify whether it affects:
   - authority,
   - message schema,
   - packet validation,
   - versioning,
   - replication,
   - reliability,
   - prediction,
   - rollback,
   - interpolation,
   - bandwidth,
   - reconnect,
   - host migration,
   - logging,
   - security,
   - tests.
3. Revise current output.
4. Ask whether the correction should become durable network-code guidance if reusable.
5. Store only if approved and evidence-backed.

---

## Tool-Use Policy

This rules file does not grant tools by itself. Agents applying it must follow their own tool permissions.

General guidance:

- Use file-reading tools to inspect network code, message schemas, replication specs, tests, logs, bandwidth reports, security reviews, and architecture decisions.
- Use search tools to find message types, version fields, validation paths, packet-size checks, rate limits, reliable RPCs, logging calls, and reconnect/host migration handlers.
- Use write/edit tools only after approval under the active agent’s workflow.
- Use Bash only if the active agent allows it and only under that agent’s safety policy.
- Do not run servers, packet tests, bandwidth profilers, CI, or external network commands without required approval.
- Do not use Bash to bypass write/edit approval.

---

## Safety Guardrails

Never allow production network code under `src/networking/**` to:

- trust the client for gameplay-critical state,
- parse unbounded packets,
- skip packet-size validation,
- skip field-range validation,
- omit message versioning,
- omit rate limits for client-originated messages,
- omit replication strategy for networked values,
- use reliable spam for frequent cosmetic events,
- claim prediction/reconciliation works without test evidence,
- claim bandwidth is acceptable without measurement or explicit estimate,
- expose secrets/private data in logs,
- flood logs under packet storms,
- imply host migration exists when unsupported,
- claim online readiness from LAN-only tests.

---

## Output Standards

Network-code reviews should be:

- authority-aware,
- trust-boundary-aware,
- schema-specific,
- version-aware,
- validation-specific,
- bandwidth-aware,
- prediction/reconciliation-aware,
- lifecycle-aware,
- logging-safe,
- test-evidence-backed,
- clear about uncertainty.

### Review Output Format

```md
## Network Code Review: [System/File]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Authority

### Packet Validation

### Message Versioning

### Replication Strategy

### Prediction / Reconciliation / Rollback

### Bandwidth

### Disconnect / Reconnect / Host Migration

### Logging

### Security

### Tests / Profiling

### Required Follow-Up
```

---

## Reflection Checklist

After reviewing or drafting network code, privately check:

- Did I identify authoritative owner?
- Did I check whether client input is only intent?
- Did I validate packet size and field ranges?
- Did I check message versioning?
- Did I check ownership/state/timing validation?
- Did I check rate limits?
- Did I define replication strategy?
- Did I define reliability/frequency/interpolation?
- Did I check prediction/reconciliation/rollback?
- Did I check bandwidth budget?
- Did I check disconnect/reconnect behavior?
- Did I check host migration status?
- Did I check logging redaction and rate limiting?
- Did I require adverse-network tests?
- Did I state uncertainty honestly?

Do not expose private chain-of-thought. Report only findings, evidence, and recommendations.

---

## Evaluation Checklist

Before final approval of network code:

### Authority and Security

- [ ] Gameplay-critical state is server-authoritative.
- [ ] Client messages are intent, not truth.
- [ ] Incoming packet size is bounded.
- [ ] Field ranges are validated.
- [ ] Ownership is validated.
- [ ] Session/state is validated.
- [ ] Rate limits exist.
- [ ] Replay/spoofing risk is considered.
- [ ] Security owner review is flagged where relevant.

### Message Contracts

- [ ] Message has version.
- [ ] Payload fields are documented.
- [ ] Compatibility behavior is defined.
- [ ] Version mismatch behavior is defined.
- [ ] Failure behavior is defined.
- [ ] Tests cover invalid/mismatched versions where relevant.

### Replication

- [ ] Every networked value has replication strategy.
- [ ] Reliability is defined.
- [ ] Frequency is defined.
- [ ] Interpolation is defined.
- [ ] Prediction/reconciliation is defined where relevant.
- [ ] Bandwidth budget is defined.
- [ ] Relevancy/priority are considered.

### Prediction and Rollback

- [ ] Client prediction uses sequence/prediction keys.
- [ ] Server correction path exists.
- [ ] Rollback buffer is defined where needed.
- [ ] Smoothing/snap thresholds are defined.
- [ ] Misprediction tests exist or are planned.

### Lifecycle

- [ ] Disconnect behavior is defined.
- [ ] Reconnect behavior is defined.
- [ ] Host migration status is explicit.
- [ ] Version mismatch during join is handled.
- [ ] Session cleanup is defined.

### Observability

- [ ] Logs are rate-limited.
- [ ] Logs are redacted.
- [ ] Logs include useful diagnostic metadata.
- [ ] Log behavior differs appropriately by debug/release if needed.

### Validation Evidence

- [ ] Unit tests exist for validation logic.
- [ ] Integration tests exist for session flow where relevant.
- [ ] Simulated network tests exist for latency/loss/jitter where relevant.
- [ ] Bandwidth profile exists or missing evidence is stated.
- [ ] LAN-only status is not treated as online-ready.

---

## Example Workflows

### Example 1: Client-Reported Damage

Issue:

```text
Client sends damageAmount directly to server.
```

Response:

```md
Finding: Client-authoritative gameplay-critical state.

Severity: NET-S1.

Fix:
- Client sends attack intent and target ID.
- Server validates cooldown, range, line-of-sight, target state, and ownership.
- Server computes damage authoritatively.
- Server replicates result.
```

---

### Example 2: Unversioned Message

Issue:

```text
MoveInputMessage has no version.
```

Response:

```md
Finding: Message version missing.

Fix:
- Add message version or protocol-version mapping.
- Define compatibility behavior.
- Reject incompatible versions during handshake or message parse.
- Add version-mismatch test.
```

---

### Example 3: Missing Packet Range Validation

Issue:

```text
Client sends abilityIndex with no bounds check.
```

Response:

```md
Finding: Field-range validation missing.

Fix:
- Validate `abilityIndex` against allowed range.
- Reject invalid value before indexing.
- Log validation failure with rate limit.
- Add malformed-packet test.
```

---

### Example 4: Reliable Cosmetic Spam

Issue:

```text
Footstep events are sent as reliable messages.
```

Response:

```md
Finding: Reliable-message backlog risk.

Fix:
- Make footstep events unreliable/sequenced, locally derived, or relevance-filtered.
- Reduce frequency if needed.
- Profile message bandwidth.
```

---

### Example 5: Replicated Value Missing Strategy

Issue:

```text
`playerAimDirection` is networked but has no reliability, frequency, or interpolation policy.
```

Response:

```md
Finding: Replication strategy missing.

Fix:
- Define unreliable sequenced update.
- Define send frequency.
- Define quantization.
- Define interpolation or direct use.
- Define bandwidth budget.
```

---

### Example 6: Reconnect Trusts Client Cache

Issue:

```text
On reconnect, inventory is restored from client cache.
```

Response:

```md
Finding: Authority violation during reconnect.

Fix:
- Restore inventory from server-authoritative state.
- Validate reconnect identity.
- Discard client cache for consequential state.
- Add reconnect regression test.
```

---

### Example 7: Host Migration Assumed

Issue:

```text
Code says “migrate host later” but session depends on host migration.
```

Response:

```md
Finding: Host migration undefined.

Fix:
- Declare `HOST_MIGRATION_UNSUPPORTED`, `HOST_MIGRATION_NOT_APPLICABLE`, or `HOST_MIGRATION_SUPPORTED`.
- If supported, define host selection, state transfer, timeout, and tests.
- If unsupported, define player-facing session failure behavior.
```

---

### Example 8: Log Flooding

Issue:

```text
Every invalid packet writes a full log entry with payload.
```

Response:

```md
Finding: Log flooding and sensitive payload risk.

Fix:
- Rate-limit invalid-packet logs.
- Aggregate repeated failures.
- Do not log raw payload.
- Store message type, version, validation category, and safe player/session hash only.
```

---

### Example 9: User Correction

User says:

```text
Inventory replication is owner-only and reliable, but inventory changes are event-based, not full-state snapshots.
```

Response pattern:

```text
Understood. I’ll treat inventory replication as owner-only reliable event-based replication, not full-state snapshots. Store this as a durable network-code rule only if approved.
```

---

### Example 10: Case Where the Rule Must Not Learn

User says during prototyping:

```text
Just trust the client for position while we test movement.
```

Response pattern:

```text
That can be treated as prototype-only if clearly labeled. Production network code under `src/networking/**` must validate client movement intent and preserve authoritative correction.
```

---

## Final Network Code Rule

Network code under `src/networking/**` must be:

- server-authoritative for gameplay-critical state,
- hostile to untrusted client data,
- packet-size and field-range validated,
- message-versioned,
- replication-strategy explicit,
- bandwidth-budgeted,
- prediction/reconciliation/rollback aware,
- disconnect/reconnect/host-migration explicit,
- logging-rate-limited and redacted,
- tested under real network conditions,
- security-reviewed where relevant,
- and honest about unresolved validation.