# ADR-0002: Save Stability Barrier Protocol

## Status
Accepted

## Date
2026-04-26

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Unity 6.3 LTS (6000.3.x) |
| **Domain** | Core persistence / gameplay-state integration |
| **Knowledge Risk** | MEDIUM - Unity 6.3 is post-LLM-cutoff, but this ADR defines synchronous gameplay data-readiness contracts rather than Unity-specific storage APIs. |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`; `docs/engine-reference/unity/breaking-changes.md`; `docs/engine-reference/unity/deprecated-apis.md`; `.claude/docs/technical-preferences.md` |
| **Post-Cutoff APIs Used** | None. |
| **Verification Required** | PlayMode integration tests for declared barrier invocation, same-frame kill/save consistency, transition-save deadline handling, `DownstreamSaveBarrierUnresolved`, and no-bytes-written failure behavior. |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0001 XP Source Lifecycle Registry for the Character Progression + NPC source lifecycle use case. |
| **Enables** | ADR-0003 Progression Baseline Snapshot Contract; ADR-0004 First-Save Materialization and Character Identity; Character Progression GDD approval; Save/Load implementation readiness. |
| **Blocks** | T1 Save/Load implementation; T1 Character Progression implementation; any downstream system implementation that declares transient save-unsafe states. |
| **Ordering Note** | This ADR must be accepted before revising Save/Load and Character Progression barrier ACs, and before writing stories that serialize downstream-owned state during active gameplay. |

## Context

### Problem Statement

Save/Load must serialize authoritative gameplay state while other systems may be inside short transient transactions. Character Progression can be resolving same-frame Combat kill credit, XP awards, level-up chains, or future XP adjustments. NPC System can be recording source lifecycle death/despawn outcomes needed to keep XP and source availability consistent across save/load.

Round-4 Character Progression review found that GDD prose named `ProgressionSaveBarrier` and `NpcSourceLifecycleSaveBarrier`, but the project still needed one architecture-level protocol for who declares a barrier, when Save/Load invokes it, what a successful result means, what an unresolved result means, and how transition-save deadlines interact with downstream readiness.

### Constraints

- T1 is offline single-player with local saves.
- Save/Load owns serialization, integrity, versioning, write failure surfacing, and no-bytes-written failure behavior.
- Downstream systems own their own schemas, validation, state transitions, and save-eligible read views.
- World Structure owns the transition-save `save_mutex_max_ms` timeout and `SaveTimedOutEvent` when Save/Load does not return a confirmation in time.
- Save/Load owns known write failures, including `SaveFailedEvent(DownstreamSaveBarrierUnresolved)`, when a declared downstream barrier cannot produce save-stable state.
- Save/Load must not serialize stale, partial, pre-award, or mismatched cross-system state.
- A barrier protocol is not a storage-backend decision and does not choose SQLite, JSON, binary, HMAC key derivation, atomicity mechanism, or filesystem APIs.

### Requirements

- Provide a standard barrier interface shape for downstream systems with transient save-unsafe states.
- Let Save/Load discover which downstream payloads require barriers before serialization.
- Let a downstream system atomically settle or reject its pending transaction before Save/Load reads its state.
- Bound barrier waits by the stricter of the downstream barrier budget and caller deadline.
- Fail loudly with `DownstreamSaveBarrierUnresolved` and no bytes written when a barrier cannot settle.
- Support coordinated barrier groups where two systems must both be stable before either payload is serialized.
- Preserve World Structure's transition-save timeout semantics without duplicating them inside downstream systems.

## Decision

Save/Load will use a declared, bounded, synchronous save-stability barrier protocol before reading downstream-owned payloads that can be transiently unsafe. A downstream system declares a barrier when its GDD or ADR says some runtime state must settle before serialization.

For T1, the first declared barrier group is the XP/source lifecycle consistency group:

- Character Progression declares `ProgressionSaveBarrier`.
- NPC System declares `NpcSourceLifecycleSaveBarrier`.
- Save/Load must invoke both before serializing post-award Character Progression XP state or NPC-owned source lifecycle state from the same kill-resolution phase.

### Architecture Diagram

```text
Save trigger
  Transition Save | Manual Save | Autosave | Session-Exit Save
        |
        v
Save/Load Writing
        |
        v
Resolve declared barrier groups for payloads to serialize
        |
        +--> Character Progression.ProgressionSaveBarrier(request)
        |
        +--> NPC System.NpcSourceLifecycleSaveBarrier(request)
        |
        v
All required barriers Stable?
        |
        +-- yes --> read stable downstream snapshots/read views
        |          serialize full save payload
        |          emit SaveWriteConfirmed on commit success
        |
        +-- no  --> write no bytes
                   transition WriteFailed
                   emit SaveFailedEvent(DownstreamSaveBarrierUnresolved)
```

### Key Interfaces

#### Barrier Declaration

Each downstream system that can be transiently save-unsafe must declare:

```yaml
SaveStabilityBarrierDeclaration:
  owner_system: system name
  barrier_name: stable hook name
  payloads_guarded: list of downstream-owned save payload names
  barrier_group: optional shared group id
  owner_budget_ms: max owner-local settle time
  failure_class: DownstreamSaveBarrierUnresolved
```

Rules:

- A system without transient save-unsafe states does not need a barrier.
- A system with declared transient save-unsafe states must not rely on Save/Load guessing its state machine.
- Save/Load owns the registry of declarations it consults during `Writing`.
- `barrier_group` is required when two or more payloads must be stable together before any member payload is serialized.

#### Barrier Request

```yaml
SaveStabilityBarrierRequest:
  save_request_id: local monotonic save attempt id
  trigger_type: TransitionSave | ManualSave | AutosaveTick | SessionExitSave
  requested_payloads: list of payload names owned by the downstream system
  caller_deadline_monotonic_ms: nullable monotonic deadline from caller context
  owner_budget_ms: max owner-local settle budget
  effective_deadline_monotonic_ms: min(caller_deadline, now + owner_budget_ms) when caller deadline exists; otherwise now + owner_budget_ms
```

Rules:

- Transition Save passes the remaining World Structure `save_mutex_max_ms` window as the caller deadline.
- Manual Save, Autosave, and Session-Exit Save may provide their own caller deadlines; if absent, the owner budget is still mandatory.
- Deadlines use a monotonic clock, not wall-clock time.
- Save/Load must not invoke a barrier without an effective deadline.

#### Barrier Result

```yaml
SaveStabilityBarrierResult:
  status: Stable | Unresolved | Failed
  owner_system: system name
  barrier_name: stable hook name
  save_request_id: matching request id
  read_token: opaque token required when status = Stable
  owner_state_revision: debug/integration-test revision id
  reason_code: None | TransactionPending | DeadlineExceeded | ValidationFailed | OwnerUnavailable | Unknown
  diagnostics: development-build diagnostic string
```

Rules:

- `Stable` means the downstream owner has settled, rejected, or diagnosed all relevant pending transactions and can expose a stable save read view for the requested payloads.
- `Stable` does not require the owner to freeze all gameplay forever; it requires the read view associated with `read_token` to remain coherent for Save/Load's current read phase.
- `Unresolved` means the owner could not settle by the effective deadline. Save/Load must fail the write with `SaveFailedEvent(DownstreamSaveBarrierUnresolved)` and write no bytes.
- `Failed` means the owner detected invalid state while preparing a save read view. Save/Load must fail the write using the declared failure class unless the owner's GDD maps it to a more specific existing save failure.
- Save/Load must not silently retry inside the same write attempt after `Unresolved` or `Failed`; a later save trigger may retry after gameplay settles.

#### Stable Read View

When a barrier returns `Stable`, downstream state must be read through one of these implementation-equivalent forms:

- an immutable snapshot returned or exposed by the owner for `read_token`, or
- an owner-managed read method that guarantees the same values for `read_token` throughout the current Save/Load read phase.

Rules:

- Save/Load must read only payloads listed in `requested_payloads`.
- Downstream owners remain the only writers of their state.
- Save/Load never mutates downstream state to make it saveable.
- Save/Load releases or discards the read token when the write attempt finishes or fails.

#### Barrier Group Semantics

```yaml
SaveStabilityBarrierGroup:
  group_id: stable id
  members: list of barrier names
  guarded_consistency: human-readable reason
  failure_class: DownstreamSaveBarrierUnresolved
```

T1 group:

```yaml
group_id: xp_source_lifecycle_consistency
members:
  - ProgressionSaveBarrier
  - NpcSourceLifecycleSaveBarrier
guarded_consistency: "Post-award XP and defeated-source lifecycle state must serialize together or not at all."
failure_class: DownstreamSaveBarrierUnresolved
```

Rules:

- Save/Load invokes every member of a required group before reading any member payload.
- If any member returns `Unresolved` or `Failed`, Save/Load writes no bytes for the whole save attempt.
- Save/Load must not serialize post-award XP while source lifecycle state is pre-death or missing.
- Save/Load must not serialize source lifecycle death state while Character Progression is still mid-award for the same kill-resolution phase.

### T1 Barrier Contracts

`ProgressionSaveBarrier` must settle or reject:

- same-frame Combat kill-credit dispatch,
- XP award transactions,
- level-up chains,
- spell-eligibility tier changes caused by that transaction,
- future approved XP adjustments.

`NpcSourceLifecycleSaveBarrier` must settle or reject:

- same-frame Combat death/despawn lifecycle outcomes,
- source lifecycle state transition to `Defeated`, `Active`, or `RespawnEligible`,
- lifecycle token rotation decisions relevant to the requested save payload.

Save/Load must treat both barriers as part of the `xp_source_lifecycle_consistency` group whenever a save payload includes Character Progression XP state and NPC-owned source lifecycle records for a kill-resolution phase that may still be settling.

## Alternatives Considered

### Alternative 1: Save/Load Reads Current State Without Barriers

- **Description**: Save/Load serializes downstream state directly whenever `Writing` begins.
- **Pros**: Simple implementation.
- **Cons**: Can persist pre-award XP with post-award gameplay, or post-award XP without matching defeated-source lifecycle state.
- **Rejection Reason**: Violates Save/Load's fail-loud integrity model and Character Progression's XP/source consistency requirements.

### Alternative 2: Global Gameplay Pause / Lock During Every Save

- **Description**: Save/Load globally pauses all gameplay state mutation before reading any payload.
- **Pros**: Coarse but easy to reason about.
- **Cons**: Over-broad, likely to create hitches, and pushes Save/Load into owning runtime state timing for every system.
- **Rejection Reason**: Downstream owners know their own transient unsafe states; Save/Load should request readiness, not freeze the world.

### Alternative 3: Async Retry Until Stable

- **Description**: Save/Load keeps retrying downstream reads until every system eventually reports stable state.
- **Pros**: Avoids immediate write failure.
- **Cons**: Can violate World Structure's transition-save timeout, obscure failure causes, and hide unbounded waits.
- **Rejection Reason**: T1 needs bounded, fail-loud save behavior; later triggers can retry after gameplay settles.

### Alternative 4: Declared Bounded Save-Stability Barriers

- **Description**: Downstream systems with transient unsafe states expose bounded readiness hooks; Save/Load invokes them before reading affected payloads.
- **Pros**: Keeps ownership local, gives Save/Load deterministic failure behavior, respects caller deadlines, and supports cross-system consistency groups.
- **Cons**: Requires declaration registry, read tokens/snapshots, and PlayMode integration tests.
- **Decision**: Selected as the proposed architecture in this ADR.

## Consequences

### Positive

- Save/Load can remain generic infrastructure while still avoiding stale cross-system saves.
- Downstream systems keep ownership of their own state machines and read views.
- Same-frame kill/save races become deterministic and testable.
- Transition Save can fail loudly before writing partial state instead of hanging or serializing mismatched payloads.
- The protocol generalizes to future systems that need save-stability hooks.

### Negative

- Each downstream barrier owner must implement a bounded readiness hook and test doubles.
- Save/Load must track barrier declarations and groups.
- Some save attempts can fail even though a retry a few frames later would succeed.
- Barrier budgets must be tuned so normal saves do not routinely trip World Structure's transition timeout.

### Risks

- **Risk**: A barrier implementation performs too much work and consumes the transition-save budget.
  **Mitigation**: Each barrier declares `owner_budget_ms`; Transition Save passes the remaining caller deadline; unresolved barriers fail loudly.
- **Risk**: Save/Load treats `Stable` as permission to read unrelated state.
  **Mitigation**: `requested_payloads` and `read_token` scope the read view to declared payloads.
- **Risk**: Future systems add transient save-unsafe states without declaring barriers.
  **Mitigation**: GDD review and architecture review must flag any save-owned payload with transient unsafe states and no declaration.
- **Risk**: World Structure timeout and Save/Load barrier failure both fire for the same transition.
  **Mitigation**: Save/Load emits `SaveFailedEvent(DownstreamSaveBarrierUnresolved)` when it knows the barrier failed; World Structure emits `SaveTimedOutEvent` only when no confirmation/failure returns before `save_mutex_max_ms`.

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|------------|-------------|--------------------------|
| `design/gdd/save-load-persistence.md` | Rule 8a requires downstream save-stability hooks before reading downstream-owned payloads. | Defines declarations, requests, results, read tokens, barrier groups, and failure semantics. |
| `design/gdd/save-load-persistence.md` | Failure class `DownstreamSaveBarrierUnresolved` writes no bytes and emits `SaveFailedEvent`. | Locks unresolved/failed barrier behavior and no-bytes-written outcome. |
| `design/gdd/character-progression.md` | `ProgressionSaveBarrier` must settle kill credit, XP transactions, level-up chains, and future XP adjustments before serialization. | Defines `ProgressionSaveBarrier` as a T1 declared barrier and scopes its stable result. |
| `design/gdd/npc-system.md` | `NpcSourceLifecycleSaveBarrier` must settle source lifecycle death/despawn before serialization. | Defines `NpcSourceLifecycleSaveBarrier` as a T1 declared barrier and groups it with Character Progression. |
| `design/gdd/world-structure.md` | Transition Save must complete or fail within `save_mutex_max_ms`; missing confirmation triggers `SaveTimedOutEvent`. | Preserves WS timeout ownership while requiring Save/Load to fail known unresolved barriers loudly. |

## Performance Implications

- **CPU**: One bounded hook call per declared barrier per save attempt; T1 group invokes two hooks for XP/source lifecycle consistency.
- **Memory**: Optional immutable read snapshots or read-token views for the duration of a single Save/Load read phase.
- **Load Time**: None directly; this ADR governs writes, not load hydration.
- **Network**: None in T1. A future networked save authority ADR must extend this protocol before replicated saves exist.

## Migration Plan

1. Revise `design/gdd/save-load-persistence.md` to reference ADR-0002 for Rule 8a, `DownstreamSaveBarrierUnresolved`, barrier groups, and read-token semantics.
2. Revise `design/gdd/character-progression.md` to reference ADR-0002 for `ProgressionSaveBarrier`, replacing local prose that invents barrier result semantics.
3. Revise `design/gdd/npc-system.md` to reference ADR-0002 for `NpcSourceLifecycleSaveBarrier`.
4. Add or revise acceptance criteria proving:
   - Save/Load invokes all declared barriers before reading guarded payloads,
   - grouped barriers must all be stable before any member payload serializes,
   - unresolved barriers emit `SaveFailedEvent(DownstreamSaveBarrierUnresolved)` and write no bytes,
   - Transition Save respects the stricter caller deadline from World Structure,
   - stable read tokens/snapshots do not change during Save/Load's read phase.
5. Update `docs/registry/architecture.yaml` after this ADR draft is reviewed, registering the barrier interface and forbidden direct-read pattern.

## Validation Criteria

- Integration test proves a save attempt with no pending downstream transaction invokes declared barriers and then serializes post-barrier payloads.
- Integration test proves a same-frame Combat kill-credit + Manual Save invokes `ProgressionSaveBarrier` and `NpcSourceLifecycleSaveBarrier` before reading Character Progression or NPC payloads.
- Integration test proves a same-frame Combat kill-credit + Transition Save respects the remaining `save_mutex_max_ms` deadline.
- Integration test proves a held `ProgressionSaveBarrier` returns unresolved, Save/Load emits `SaveFailedEvent(DownstreamSaveBarrierUnresolved)`, and no bytes are written.
- Integration test proves a held `NpcSourceLifecycleSaveBarrier` returns unresolved, Save/Load emits `SaveFailedEvent(DownstreamSaveBarrierUnresolved)`, and no bytes are written.
- Integration test proves grouped barrier behavior: one stable member and one unresolved member fails the whole save attempt.
- Integration test proves `read_token` or immutable snapshot values remain coherent throughout the current Save/Load read phase.
- Event-log test proves World Structure emits `SaveTimedOutEvent` only when Save/Load returns no success/failure confirmation before `save_mutex_max_ms`.

## Related Decisions

- ADR-0001: XP Source Lifecycle Registry
- `DECISIONS.md` D003 - Single-player offline through Tier 1.
- `DECISIONS.md` D007 - ADR-0001 XP Source Lifecycle Registry.
- `design/gdd/save-load-persistence.md`
- `design/gdd/character-progression.md`
- `design/gdd/npc-system.md`
- `design/gdd/world-structure.md`

