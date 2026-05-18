# S2-M3-01 - Named NPC Objective Frame

**Status:** Complete
**Sprint:** 2
**Priority:** Must Have
**Layer:** Gameplay / Unity Runtime
**Type:** Integration / Visual-Feel
**Estimate:** 1.0 days
**Manifest Version:** Sprint 2, 2026-05-14
**GDD:** `design/gdd/npc-system.md`; `design/gdd/game-concept.md`
**Quick Design:** `design/quick/quick-design-m3-objective-npc-loot.md`
**Governing Decisions:** `DECISIONS.md` D001, D002, D003, D004
**Evidence:** `tests/evidence/S2-M3-01/verification.md`

## Routing Status

Ready for `/story-readiness` after `S2-M3-00` completed. This story adds the
first M3 player-facing authored reason: one named NPC anchor and intentional
templated interaction, without quest markers, Dialogue System UI, persistence,
faction reaction, or live LLM behavior.

## Source Trace

- `design/quick/quick-design-m3-objective-npc-loot.md:254` through `:259`
  define M3-01 Named NPC Objective Frame acceptance candidates.
- `design/quick/quick-design-m3-objective-npc-loot.md:126` through `:140`
  define the working named NPC sketch and constraints.
- `design/gdd/npc-system.md:104` through `:105` define intentional interaction
  through `ActiveInZone` -> `Interacting`.
- `design/gdd/npc-system.md:388` through `:407` require no marker affordances
  and T1 templated dialogue context only.
- `DECISIONS.md:68` through `:79` keeps T1 dialogue templated and live LLM out
  of scope.

## Scope

Add one visible named NPC anchor/marker near the safe side of `_DevEntry.unity`.
Intentional player interaction records an `NpcInteractionContext`-shaped event
and a templated dialogue id or text key that frames the M3 objective.

Planned implementation surface:

- `M3_Caretaker` or equivalent scene anchor under `_DevEntry.unity`.
- Minimal runtime interaction component or controller hook.
- Story-specific Unity runner evidence under `tests/evidence/S2-M3-01/`.

## Out Of Scope

- No final Dialogue System UI, quest log, minimap, marker, overhead name, glow,
  outline, proximity bark, camera emphasis, live LLM, LLM memory, moderation,
  NPC schedule, NPC persistence, companion behavior, faction reaction, loot,
  vendor, Save/Load, or visible faction consequence.

## Acceptance Criteria

| ID | Criterion | Evidence |
| --- | --- | --- |
| `S2-M3-01-01` | `_DevEntry.unity` contains one visible named NPC anchor/marker for the M3 objective frame. | `tests/evidence/S2-M3-01/verification.md` |
| `S2-M3-01-02` | Intentional interaction with the NPC records an `NpcInteractionContext`-shaped event and a templated dialogue id or text key. | `tests/evidence/S2-M3-01/verification.md` |
| `S2-M3-01-03` | The NPC frames the objective without quest markers, overhead names, glow, outline, minimap dots, auto-pathing, proximity barks, or live LLM calls. | `tests/evidence/S2-M3-01/verification.md` |
| `S2-M3-01-04` | The implementation is session-local and does not claim full NPC schedule, persistence, faction reaction, companion, or Dialogue System ownership. | `tests/evidence/S2-M3-01/verification.md` |
| `S2-M3-01-05` | M2 clean-loop, overpull, and named-blocker preservation checks still pass or are explicitly reverified through the shared M3 smoke path. | `tests/evidence/S2-M3-01/verification.md` |

## QA Test Cases

- **S2-M3-01-01**: NPC anchor exists
  - Given: `_DevEntry.unity` is opened.
  - When: the M3 scene anchors are inspected.
  - Then: exactly one M3 named NPC anchor/marker is present and distinguishable at blockout fidelity.
- **S2-M3-01-02**: Intentional interaction
  - Given: the player is in allowed range.
  - When: interaction is triggered intentionally.
  - Then: evidence records NPC interaction context and templated dialogue handle.
- **S2-M3-01-03**: No markers or LLM
  - Given: the NPC is visible and interactable.
  - When: gameplay is inspected before and after interaction.
  - Then: no marker affordance or live LLM dependency is present.
- **S2-M3-01-04**: Narrow NPC surface
  - Given: changed files are inspected.
  - When: NPC runtime code is reviewed.
  - Then: no schedule, persistence, faction reaction, companion, or Dialogue UI system is introduced.
- **S2-M3-01-05**: M2 preservation
  - Given: M3 NPC code is present.
  - When: preservation smoke runs.
  - Then: M2 loop proofs still pass.

## Test Evidence

Required evidence:

- `tests/evidence/S2-M3-01/verification.md`
- Story-specific Unity Play Mode or batchmode runner output for NPC anchor and interaction.
- M2 preservation evidence through the M3 smoke handoff.
- T1 negative-scope scan over changed files.
- `git diff --check`
- `.githooks/pre-commit`

## Performance Budget

This story adds one blockout NPC anchor and intentional interaction check only.
It must not add per-frame NPC search, schedule ticking, pathfinding behavior,
Dialogue UI lifecycle, LLM calls, or broad scene polling.

## Dependencies

- Depends on: `S2-M3-00` complete.
- Unlocks: `S2-M3-02` Objective State + Relic Hand-In.

## Completion Notes

- Closed via `/story-done` 2026-05-18 with verdict **COMPLETE WITH NOTES** (5/5 AC passing).
- Implementation files: `Assets/Scripts/M3NamedNpcObjectiveFrame.cs`, `Assets/Editor/GravenspireM3NamedNpcObjectiveFrameBuilder.cs`, `Assets/Editor/GravenspireM3NamedNpcObjectiveFrameVerificationRunner.cs`, `Assets/Scenes/_DevEntry.unity` (NPC anchor + transform).
- Evidence: `tests/evidence/S2-M3-01/verification.md` (PASS); story-specific Unity smoke `unity-named-npc-objective-frame-20260518-smoke.md`; M2 preservation reruns `m2-02-preservation-20260518-smoke.md`, `m2-03-preservation-20260518-smoke.md`, `m2-04-preservation-20260518-smoke.md`.
- Local gates: dotnet Combat Core 175/175 PASS; four Unity batchmode runners exit 0 PASS; `git diff --check` clean; T1 negative-scope scan PASS WITH CLASSIFIED DOC HIT (evidence-only, no runtime/scene/runner hits — convention matches `tests/evidence/S2-M2-03/verification.md:43`); `.githooks/pre-commit` PASS.
- Carryover added at closure: `m2_02_runner_date_hardcoded` — `Assets/Editor/GravenspireM2SingleTrashLoopVerificationRunner.cs:192` hardcodes `**Date:** 2026-05-12`; pre-existing runner-hygiene gap in the same class as `s2_bridge_runner_evidence_path_hardcoded` and `launch_runner_evidence_path_hardcoded`; non-blocking for S2-M3-01 closure.
- Next gate: `/story-readiness production/stories/s2-m3-02-objective-state-relic-hand-in.md`.
