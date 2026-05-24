# S3-03: Player Relic Recovery + Looting (Accept, Recover, Hand-In, Loot)

> **Sprint**: Sprint 3 — Playable Vertical-Slice Assembly
> **Sprint Plan**: `production/sprints/sprint-3.md` (Story Ledger row, line 68)
> **Status**: Ready (depends on S3-02 being Done)
> **Layer**: Core
> **Type**: Integration
> **Estimate**: 1.0 day (MEDIUM confidence — per `sprint-3.md:157`; densest wiring of the slate, 1.5-day actuals would not be a deviation)
> **Manifest Version**: Unavailable (control-manifest absent project-wide per `production/qa/plans/qa-plan-sprint-2-20260509.md:54,60`; documented fallback applies)
> **Generated**: 2026-05-23
> **Owner**: Codex

## Context

**Sprint 3 plan**: `production/sprints/sprint-3.md`
**Quick-design source**: `design/quick/quick-design-m3-objective-npc-loot.md`
**Story Ledger row**: `production/sprints/sprint-3.md:68`
**Requirement IDs (story-local ACs)**: `S3-03-01` through `S3-03-12`

**Requirement Summary**: Wire the full M3 objective lifecycle behind player input — Accept (NotIntroduced→Accepted), Recover (Accepted→RelicRecovered), Hand-In (RelicRecovered→Complete) — plus objective loot resolution at relic recovery. Expand S3-02's NPC adapter to be objective-state-routing; introduce a new relic adapter on `M3_ObjectiveRelic` that calls relic recovery atomically with objective loot resolution. All four M3 state transitions become player-driven; the existing M3 systems (`M3ObjectiveStateRelicHandIn`, `M3LootTableFixedProfileVendor`) have zero diff. The S2-M3-02 and S2-M3-03 closures (commits `fb77f83`, `25c94ee`) are preserved verbatim. Telemetry records each transition with `player_driven` source attribution.

**Plan row scope note (not a gap, but worth surfacing)**: the Story Ledger row at `sprint-3.md:68` names `TryRecoverRelic`, `TryAcceptObjectiveFromNpc`, and `TryResolveObjectiveLoot`. It does not enumerate `TryReturnRelicToNpc` (the hand-in transition). The M3 state machine has four states and three transitions; without hand-in, the objective never reaches Complete, and S3-06's end-to-end loop has nothing to compose. **Hand-in is therefore in S3-03's scope** as the third state transition, consistent with the plan's purpose statement that the loop be playable end-to-end.

**Governing decisions** (DECISIONS.md):

| D-entry | Status | Usage |
|---|---|---|
| D001 (`DECISIONS.md:14`) | Locked | Unity 6.3 LTS + C# + URP |
| D003 (`DECISIONS.md:51`) | Locked | Tier 1 single-player offline — no Save/Load of objective state (M4); the session-local invariant from S2-M3-02 holds |
| D004 (`DECISIONS.md:73`) | Provisional | Templated dialogue boundary — same as S3-02; no rendering |
| D012 (`DECISIONS.md:342`) | Locked | T1 combat-feel validated |
| D016 (`DECISIONS.md:554`) | Locked | No new systems, reuse-not-rebuild — D016 RED FLAG for any rewrite of objective state or loot resolution |

**Sprint 3 feedback rule** (`sprint-3.md:85`): every feedback element passes the trigger + direction tests. The objective loop is emotionally close to M5 (faction consequence) — this story must NOT introduce route-hints, quest-arrows, or "next step" UI. The relic appears in the scene when objective is Accepted (M3 system auto-handles via `ApplyRelicAvailability`); that scene change IS the only diegetic cue, and it is the M3 system's existing behavior, not new advertising.

**Architecture Module**: Gameplay / Objective Lifecycle Wiring (NPC adapter expansion + new relic adapter)
**Engine**: Unity 6.3 LTS
**Engine Risk**: LOW (only core MonoBehaviour + already-tested M3 API surface)

**Surfaces reused** (do not re-author):
- S3-01 harness dispatch interface: `IPlayerInteractTarget.TryInteract(playerActorId, distanceMeters, out InteractContext context)`
- S3-02 NPC adapter: `M3NamedNpcInteractTarget` on `M3_Caretaker` (this story MODIFIES that adapter to add state-routing — the adapter is new-code-from-S3-02, not an S2 boundary, so modification is in-scope; S3-02's tests must continue to pass for the in-Accepted/Complete-state re-talk path)
- M3 objective state: `Assets/Scripts/M3ObjectiveStateRelicHandIn.cs`
  - `TryAcceptObjectiveFromNpc(M3NamedNpcObjectiveFrame npcFrame, string playerActorId, float distanceMeters)` at :54 — **internally calls** `npcFrame.TryRecordIntentionalInteraction(...)` at :65 then `_session.TryAcceptObjective(...)` at :71. Returns `false` on null frame, failed NPC record, or session rejection. **The internal call order is NPC-record (line 65) BEFORE session-accept (line 71)** — this fixes the telemetry event emission order, see AC-03.
  - `TryRecoverRelic()` at :82 — requires `_relicObject != null` and `_relicObject.activeSelf`; calls `_session.TryRecoverRelic(...)`; side effect: `ApplyRelicAvailability()` deactivates the relic GameObject post-recovery (line 132-138).
  - `TryReturnRelicToNpc(M3NamedNpcObjectiveFrame npcFrame)` at :110 — does NOT internally record NPC interaction; only transitions state. Returns `false` on null frame or session rejection.
- M3 loot/vendor: `Assets/Scripts/M3LootTableFixedProfileVendor.cs`
  - `TryResolveObjectiveLoot()` at :89 — calls `Session.TryResolveDefaultLoot(out _, out rejection)`; yields the authored M3 objective loot (`CourtMarkedRelic_T1` + `GraveDust_Salvage_T1`) into the vendor's carried inventory.
- M3 scene anchors in `Assets/Scenes/_DevEntry.unity`: `M3_Caretaker` (:881), `M3_ObjectiveRelic` (:1556)
- S2-M3-02 closure baseline: commit `fb77f83`
- S2-M3-03 closure baseline: commit `25c94ee`

## Acceptance Criteria

### NPC adapter state-routing expansion

- [ ] **S3-03-01**: The S3-02 NPC adapter `M3NamedNpcInteractTarget` is modified to route based on the current objective state (read from `M3ObjectiveStateRelicHandIn.State`):
  - `NotIntroduced` → call `state.TryAcceptObjectiveFromNpc(frame, playerActorId, distance)` (which internally records NPC interaction + transitions to Accepted — no separate `frame.TryRecordIntentionalInteraction` call to avoid double-recording)
  - `Accepted` → call `frame.TryRecordIntentionalInteraction(playerActorId, distance, out context)` only (re-talk; no state change)
  - `RelicRecovered` → call `state.TryReturnRelicToNpc(frame)` (transitions to Complete)
  - `Complete` → call `frame.TryRecordIntentionalInteraction(...)` only (post-completion re-talk; no state change)
- [ ] **S3-03-02**: S3-02's player-driven NPC interaction test (S3-02-T2) continues to pass after this modification — when the objective state is `Accepted` or `Complete`, the adapter still records intentional interaction and the `npc_interaction_intentional` telemetry event still fires with `player_driven` source. Regression discipline.
- [ ] **S3-03-03**: On a successful `TryAcceptObjectiveFromNpc` dispatch, telemetry records a new `objective_accepted` event with payload `{ npcId, playerActorId, fromState: "NotIntroduced", toState: "Accepted", source: "player_driven", distanceMeters }`. The `npc_interaction_intentional` event ALSO fires (since `TryAcceptObjectiveFromNpc` internally calls `TryRecordIntentionalInteraction` at `M3ObjectiveStateRelicHandIn.cs:65` BEFORE `_session.TryAcceptObjective` at `:71`). Event order is **`npc_interaction_intentional` first, then `objective_accepted`** — forced by the M3 system's internal call sequence, not a separate scheduling decision. The test asserts both events present in that order.
- [ ] **S3-03-04**: On a successful `TryReturnRelicToNpc` dispatch, telemetry records a new `relic_handed_in` event with payload `{ npcId, playerActorId, fromState: "RelicRecovered", toState: "Complete", source: "player_driven" }`. No `npc_interaction_intentional` event fires (hand-in does not internally record).

### Relic adapter (new) + loot resolution

- [ ] **S3-03-05**: A new MonoBehaviour `M3RelicInteractTarget` is created under `Assets/Scripts/`, implements `IPlayerInteractTarget` (from S3-01), and is attached to the `M3_ObjectiveRelic` GameObject in `_DevEntry.unity:1556`.
- [ ] **S3-03-06**: The relic adapter holds serialized references to a `M3ObjectiveStateRelicHandIn` instance and a `M3LootTableFixedProfileVendor` instance. On `TryInteract(...)`, it calls `state.TryRecoverRelic()`. If that returns `false`, the adapter returns `false` immediately and the harness's interact-blocked feedback fires.
- [ ] **S3-03-07**: On a successful `TryRecoverRelic` (returns `true`), the adapter **atomically** calls `vendor.TryResolveObjectiveLoot()`. Both calls succeed or the adapter records the partial-success edge case (`TryRecoverRelic` true, `TryResolveObjectiveLoot` false) explicitly in telemetry as `objective_loot_resolution_failed` with the rejection reason. The relic recovery transition is NOT rolled back (the M3 systems do not expose rollback); the partial state is recorded honestly.
- [ ] **S3-03-08**: On a successful atomic dispatch (both calls returned `true`), telemetry records two events: `relic_recovered` with payload `{ relicObjectName, relicItemId, fromState: "Accepted", toState: "RelicRecovered", source: "player_driven" }` and `objective_loot_resolved` with payload `{ lootTableId, resolvedItemIds, source: "player_driven" }`. The relic GameObject becomes inactive (M3's `ApplyRelicAvailability` handles this automatically; the test asserts the GameObject is inactive post-recovery).

### Cross-cutting invariants

- [ ] **S3-03-09**: `M3ObjectiveStateRelicHandIn.cs` has **zero diff** in this story. S2-M3-02 closure (`fb77f83`) is preserved verbatim. The same applies to `M3ObjectiveStateRelicHandIn.cs.meta`.
- [ ] **S3-03-10**: `M3LootTableFixedProfileVendor.cs` has **zero diff** in this story. S2-M3-03 closure (`25c94ee`) is preserved verbatim. Same for `.meta`.
- [ ] **S3-03-11**: All feedback at each transition obeys S3-01's contract (interact-fired on success, interact-blocked on failure) AND the Sprint 3 feedback rule. No "objective accepted, now go to the relic" routing text. No "you got the relic, return to the caretaker" hint. No quest log entry, minimap pin, or arrow. The relic appearing/disappearing in the scene is the M3 system's existing diegetic cue and is not a new advertising element.
- [ ] **S3-03-12**: Full Tier-1 objective loop is end-to-end player-driven through this story's wiring: player interacts with `M3_Caretaker` (state goes NotIntroduced → Accepted; relic appears in scene); player walks to the now-visible relic and interacts (state goes Accepted → RelicRecovered, relic vanishes from scene, objective loot resolves into vendor inventory); player walks back to `M3_Caretaker` and interacts (state goes RelicRecovered → Complete). This loop is exercised end-to-end in a single test scenario (T7 below).

## Implementation Notes

- **S3-02 dependency**: this story cannot start until S3-02 lands the NPC adapter. S3-03 modifies that adapter in place rather than creating a new one — the adapter class is new code from S3-02, not an S2 boundary, so modification is in-scope. S3-02's tests must continue to pass (S3-03-02 is the regression AC for this).
- **NPC adapter state read**: the adapter needs a reference to `M3ObjectiveStateRelicHandIn` to read `state.State` before deciding which Try* to call. The reference can be serialized or looked up at registration time. Both adapter components (NPC + relic) need the state reference; consider a shared registration pattern or a `[SerializeField]` on each. Implementer's choice.
- **Atomic dispatch caveat (S3-03-07)**: the M3 systems do not expose transaction rollback. If `TryRecoverRelic` succeeds but `TryResolveObjectiveLoot` fails, the state is already RelicRecovered and the relic GameObject is already deactivated. The adapter records the partial-success failure mode in telemetry but does NOT attempt to undo the recovery. This is the honest behavior for greybox wiring; a future M4/M5 story would address rollback if it ever becomes necessary. In practice, `TryResolveObjectiveLoot` failure modes are session-init bugs (e.g., authored data file missing entirely — `Session.TryResolveDefaultLoot` rejection), not runtime races.
- **Relic adapter has three terminal telemetry shapes per dispatch** (matters for S3-06's end-to-end fixture):
  1. **Full success** — `relic_recovered` + `objective_loot_resolved` both fire (T3 success path, AC-08); harness interact-fired feedback plays.
  2. **Partial success** — `relic_recovered` fires + `objective_loot_resolution_failed` fires with rejection reason (AC-07 caveat path); the state has already transitioned to RelicRecovered and the relic GameObject has already deactivated; harness interact-fired feedback plays (the harness sees `TryInteract` return true because the relic adapter returns true on `TryRecoverRelic` success regardless of the subsequent loot resolution result — verify this contract decision in implementation review).
  3. **Blocked** — no events fire, only the harness's interact-blocked feedback (AC-06 path, e.g., state was NotIntroduced or Complete when player interacted with the relic, or the M3 relic GameObject was inactive because `_session.RelicAvailable` is false).

  S3-06's end-to-end telemetry fixture must account for all three branches when asserting on the relic dispatch event vocabulary. The S3-03 test suite covers (1) and (3) explicitly (T3 success and T3 edge case); (2) is covered as an edge case under T3 but should also surface explicitly in the verification.md evidence so the S3-06 fixture author doesn't have to re-derive it.
- **No CarriedCurrencyCopper change in this story**: relic recovery yields items into the vendor's carried inventory (per `TryResolveObjectiveLoot` → `Session.TryResolveDefaultLoot`), but no currency is credited. Currency only changes on `TrySellRecoveredSalvage` (S3-04) or `TryPurchaseFixedVendorGood` (out of scope). Verify in T6 that `vendor.CarriedCurrencyCopper` is unchanged post-recovery.
- **Densest wiring of the slate**: 12 ACs is more than S3-01 (9) or S3-02 (7). The plan's 1.0-day estimate (MEDIUM confidence) is honored but the implementer should expect this to be the most wire-intensive story. Sequence: NPC adapter modification first (extends existing code, regression test S3-02 path), then relic adapter (new, simpler shape), then end-to-end T7.
- **Telemetry event vocabulary**: this story introduces five new event names (`objective_accepted`, `relic_handed_in`, `relic_recovered`, `objective_loot_resolved`, `objective_loot_resolution_failed`) and reuses `npc_interaction_intentional`. S3-06 will compose these into the end-to-end telemetry assertion vocabulary. Keep names stable to avoid S3-06 churn.
- **No DateTime.UtcNow**, scene discipline (touches `_DevEntry.unity` for relic adapter attachment), style gate — same as S3-01/S3-02.

## Out of Scope

- No rewrite of `M3ObjectiveStateRelicHandIn.cs` (S2-M3-02 zero-diff invariant; D016 red flag)
- No rewrite of `M3LootTableFixedProfileVendor.cs` (S2-M3-03 zero-diff invariant; D016 red flag)
- No vendor sell or buy in this story (`TrySellRecoveredSalvage` is S3-04; `TryPurchaseFixedVendorGood` is not in the slate)
- No `CoinFaucetProjection_T1`, coin-pacing claim, tuned economy, currency persistence, or progression seed routing (S2-M3-03 boundaries hold)
- No quest log, minimap, route-hint, "now go to X" UI, glow effect on the relic, outlined NPC, or any feedback element advertising/locating/routing (Pillar 2, Sprint 3 feedback rule, especially given proximity to M5 emotional payoff)
- No Save/Load of objective state (M4 deferred; the existing session-local invariant from S2-M3-02 holds — restart of Play Mode resets the objective to NotIntroduced)
- No faction reaction or consequence on objective completion (M5 deferred behind Sprint 3 — Complete state is its own reward in S3-03, no faction stand-up)
- No multi-objective support (one objective, one relic, one NPC; multi-objective is post-Sprint-3 scope)
- No dialogue rendering for accept or hand-in transitions (D004 templated boundary holds)
- No transactional rollback if `TryResolveObjectiveLoot` fails after `TryRecoverRelic` succeeds (see Implementation Notes; greybox-acceptable)
- No human-play feel acceptance criterion on this story (sits only on S3-06 per `sprint-3.md:141`)

## QA Test Cases

### Integration test (Unity Play Mode / batchmode runner)

**Test setup** (shared): Unity 6.3 LTS (`6000.3.14f1`) editor batchmode; `_DevEntry.unity` loaded; S3-01 harness, S3-02 NPC adapter (with state-routing applied per S3-03-01), and S3-03 relic adapter all wired; player marker positioned at known coordinates relative to `M3_Caretaker` (:881) and `M3_ObjectiveRelic` (:1556).

**Test S3-03-T1: NPC adapter state-routing — NotIntroduced → Accepted (AC-01, AC-03)**
- Given: fresh `_DevEntry.unity` load; `state.State == NotIntroduced`.
- When: player marker positioned at `M3_Caretaker` within range; harness keycode fires.
- Then: adapter calls `state.TryAcceptObjectiveFromNpc(frame, playerActorId, distance)`; `frame.TryRecordIntentionalInteraction` is called internally (line 65); `_session.TryAcceptObjective` is called internally (line 71); session transitions to Accepted; `state.State == Accepted` post-dispatch; **telemetry event order is `npc_interaction_intentional` first then `objective_accepted` second** (forced by the M3 system's internal call sequence at lines 65 → 71); `objective_accepted` payload includes `fromState: "NotIntroduced", toState: "Accepted", source: "player_driven"`; relic GameObject becomes active (`ApplyRelicAvailability` line 134-137).
- Edge cases: dispatch when frame reference is null on adapter → adapter returns false at the M3 layer (line 59-63), interact-blocked feedback fires; rapid double-tap of interact key → first dispatch transitions to Accepted, second dispatch routes to Accepted-state path (re-talk), no double-transition.

**Test S3-03-T2: NPC adapter Accepted-state re-talk (AC-01, AC-02 regression)**
- Given: state is Accepted (after T1 or pre-set).
- When: player re-interacts with NPC.
- Then: adapter calls `frame.TryRecordIntentionalInteraction(...)` only (no state.TryAcceptObjectiveFromNpc); `npc_interaction_intentional` records with `player_driven` source; state remains Accepted; no `objective_accepted` event fires.
- Edge cases: S3-02-T2 regression — the S3-02 test for player-driven NPC interaction at in-range distance must continue to PASS unchanged (re-run as part of this story's evidence).

**Test S3-03-T3: relic adapter — Accept→Recover atomic dispatch (AC-05, AC-06, AC-07, AC-08)**
- Given: state is Accepted; relic GameObject is active in scene.
- When: player marker positioned at `M3_ObjectiveRelic` within harness threshold; harness keycode fires.
- Then: relic adapter calls `state.TryRecoverRelic()` → returns true; adapter atomically calls `vendor.TryResolveObjectiveLoot()` → returns true; state transitions to RelicRecovered; relic GameObject becomes inactive; vendor's `CarriesCourtMarkedRelic` becomes true (`CourtMarkedRelic_T1` resolved into inventory); two telemetry events fire — `relic_recovered` and `objective_loot_resolved`; harness interact-fired feedback plays. This is **telemetry shape (1) — full success** per Implementation Notes.
- Edge cases:
  - State is NotIntroduced or Complete when player interacts with relic → `state.TryRecoverRelic` returns false (state-machine rejects), adapter returns false, interact-blocked feedback fires, no loot resolution attempted. **Telemetry shape (3) — blocked.**
  - Vendor.TryResolveObjectiveLoot fails after recovery succeeds (synthesized in test via session-init bug, e.g., authored data file path mocked to missing) → state has already transitioned to RelicRecovered, relic GameObject is already inactive, `relic_recovered` fires, `objective_loot_resolution_failed` fires with rejection reason. **Telemetry shape (2) — partial success.** Harness interact-fired feedback still plays (the relic adapter returns true because `TryRecoverRelic` succeeded; the loot resolution failure does not change the harness-layer outcome).

**Test S3-03-T4: NPC adapter — RelicRecovered → Complete hand-in (AC-04)**
- Given: state is RelicRecovered (after T3 succeeded).
- When: player walks back to `M3_Caretaker` and interacts.
- Then: adapter routes to `state.TryReturnRelicToNpc(frame)`; session transitions to Complete; `state.State == Complete`; `relic_handed_in` telemetry event records with `fromState: "RelicRecovered", toState: "Complete", source: "player_driven"`; no `npc_interaction_intentional` event fires for this dispatch (hand-in does not internally record).
- Edge cases: rapid double-tap → first dispatch transitions to Complete, second dispatch routes to Complete-state path (re-talk via `frame.TryRecordIntentionalInteraction`).

**Test S3-03-T5: NPC adapter — Complete-state re-talk (AC-01)**
- Given: state is Complete (after T4).
- When: player re-interacts with NPC.
- Then: adapter calls `frame.TryRecordIntentionalInteraction(...)` only; `npc_interaction_intentional` records; state remains Complete; no `relic_handed_in` event re-fires.

**Test S3-03-T6: M3 zero-diff invariants (AC-09, AC-10)**
- Given: pre- and post-implementation source-tree snapshots of `Assets/Scripts/M3ObjectiveStateRelicHandIn.cs`, `Assets/Scripts/M3ObjectiveStateRelicHandIn.cs.meta`, `Assets/Scripts/M3LootTableFixedProfileVendor.cs`, `Assets/Scripts/M3LootTableFixedProfileVendor.cs.meta`.
- When: `git diff --stat` is run.
- Then: zero lines changed across all four files. Diff stat output committed as evidence.
- Edge cases: none (binary invariant).

**Test S3-03-T7: end-to-end objective loop player-driven (AC-12)**
- Given: fresh `_DevEntry.unity` Play Mode session; state at NotIntroduced; relic inactive.
- When: scripted player input sequence — (a) walk to NPC, interact; (b) walk to relic location (now visible), interact; (c) walk back to NPC, interact.
- Then: state transitions NotIntroduced → Accepted → RelicRecovered → Complete in that order; relic appears in scene at step (a), disappears at step (b); telemetry sequence includes `npc_interaction_intentional` then `objective_accepted` (from step a, in that order per AC-03), `relic_recovered` then `objective_loot_resolved` (from step b), `relic_handed_in` (from step c); no diagnostic UI, no quest log entry, no route hint visible at any point; final `state.State == Complete`, final `vendor.CarriesCourtMarkedRelic == true`.
- Edge cases: this test validates the AC-12 "full Tier-1 objective loop player-driven" invariant; it is the closest mechanical proof of the S3-06 end-to-end loop short of the human-play check.

**Test S3-03-T8: feedback rule compliance (AC-11)**
- Given: any of T1, T3, T4 successful or blocked dispatch.
- When: telemetry and runner screen-capture are inspected.
- Then: no rendered text matches the deny-pattern list inherited from S3-01-T3 (`"quest"`, `"go to"`, `"objective located"`, `"nearest"`, `"track"`, plus S3-03 additions: `"next step"`, `"return to"`, `"now go"`, `"head to"`) — finalized in the story's verification fixture. The relic GameObject appearing in scene at Accept and disappearing at Recover is the M3 system's existing diegetic cue (line 134-137 of `M3ObjectiveStateRelicHandIn.cs`) and is NOT a violation.

### M2 preservation reruns (additional required evidence)

- `M2SingleTrashLoop` smoke — PASS, exit 0
- `M2LinkedTrashOverpull` smoke — PASS, exit 0
- `M2NamedBlockerBoundary` smoke — PASS, exit 0

See "M2 preservation rerun execution note" below.

## Test Evidence

**Required evidence**: `tests/evidence/S3-03/verification.md`

Companion artifacts:
- `tests/evidence/S3-03/unity-player-relic-recovery-and-looting-[YYYYMMDD]-smoke.md` (story-specific Unity batchmode runner output covering T1–T8, end-to-end loop)
- `tests/evidence/S3-03/m2-02-preservation-[YYYYMMDD]-smoke.md`
- `tests/evidence/S3-03/m2-03-preservation-[YYYYMMDD]-smoke.md`
- `tests/evidence/S3-03/m2-04-preservation-[YYYYMMDD]-smoke.md`
- `tests/evidence/S3-03/m3-objective-state-zero-diff-[YYYYMMDD].txt` (proves `M3ObjectiveStateRelicHandIn.cs` + .meta zero diff)
- `tests/evidence/S3-03/m3-loot-vendor-zero-diff-[YYYYMMDD].txt` (proves `M3LootTableFixedProfileVendor.cs` + .meta zero diff)
- `tests/evidence/S3-03/s3-02-regression-[YYYYMMDD]-smoke.md` (S3-02 player-driven NPC interaction test re-run, confirming regression hold for S3-03-02)
- `dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` — Combat regression baseline must hold (current: 189/189)
- T1 negative-scope scan over changed files — zero matches expected
- `git diff --check` — clean
- `.githooks/pre-commit` — `[pre-commit] OK`
- `dotnet format --verify-no-changes` — PASS

**Evidence status**: Not started

**Relic adapter telemetry-shape summary in verification.md**: the verification.md must explicitly enumerate the three terminal telemetry shapes (full success / partial success / blocked) with example payloads, so the S3-06 end-to-end fixture author does not have to re-derive them. See Implementation Notes for the shape catalog.

**M2 preservation rerun execution note** (per `m2_melee_rng_not_reset`): three M2 preservation smokes cannot be chained in a single Unity batchmode invocation. Run each in its own `Unity.exe -batchmode -executeMethod ...` invocation with its own `-gravenspireEvidencePath` override. Pattern established in S2-M3-04 closure.

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| `S3-01` | Harness + `IPlayerInteractTarget` interface | Done |
| `S3-02` | NPC adapter `M3NamedNpcInteractTarget` that this story modifies in place | Done |

**Sprint-level pre-condition (tracked):** `dotnet format` setup — same as S3-01/S3-02.

## Blockers

S3-01 and S3-02 must both close before this story enters `/dev-story`. No design blockers; all governing D-entries Locked (D004 Provisional but its T1 boundary is explicit and respected). The plan row's omission of `TryReturnRelicToNpc` is treated as a row-text gap, not a scope blocker — the full state machine is required to close the loop, and the story owns all three transitions.

Watch items (not blockers):
- `m2_melee_rng_not_reset` — three M2 smokes require separate invocations
- `m2_02_runner_date_hardcoded` — no new hardcoded dates in runners
- `m2_renderer_material_property_access` — adapter feedback renderer hot-path discipline
- `control_manifest_absence_pre_existing` — Manifest Version `Unavailable` per fallback
- Format Gate — see Dependencies
- **Partial-success edge case (S3-03-07 / telemetry shape (2))**: if `TryResolveObjectiveLoot` fails after `TryRecoverRelic` succeeds, the state is RelicRecovered but loot is not in vendor inventory. The greybox-acceptable behavior is to record the failure and continue; a future story can address rollback if it becomes necessary. Flag for review consideration.
