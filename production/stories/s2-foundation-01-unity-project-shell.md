# S2-FOUNDATION-01 - Unity Project Shell

**Status:** Complete
**Sprint:** 2
**Priority:** Must Have
**Layer:** Foundation / Unity Shell
**Type:** Foundation / Integration
**Estimate:** 1.0 days
**Manifest Version:** Sprint 2, 2026-05-09
**GDD:** `design/gdd/game-concept.md`
**Governing Decisions:** `DECISIONS.md` D001, D003
**Evidence:** `tests/evidence/S2-FOUNDATION-01/verification.md`

## Routing Status

This story is open for Sprint 2 routing visibility. It is intentionally
blocked until Sprint 2 `/qa-plan sprint` exists. Sprint 2 QA plan now exists
at `production/qa/plans/qa-plan-sprint-2-20260509.md`, so implementation ran
under `/dev-story S2-FOUNDATION-01-unity-project-shell`.

## Source Trace

- `production/sprints/sprint-2.md:45` defines M1 Player In World: create the
  Unity project shell and launchable dev entry path, proven by launching into a
  controlled district blockout as the Cleric.
- `production/sprints/sprint-2.md:94-98` defines the minimum shell shape:
  create the production Unity shell, add `ProjectSettings/` and
  `Packages/manifest.json`, add one launchable dev entry scene or equivalent
  temporary entry path, keep scope to shell / launchability / smoke-testability,
  and do not implement hub, faction, Save/Load, NPC, loot, vendor, or objective
  gameplay in this story.

## Scope

Create the minimum production Unity project shell required for Sprint 2 to
begin playable First District implementation work. The shell should make the
repo launchable and smoke-testable without claiming gameplay systems that
belong to later Sprint 2 stories.

Planned implementation surface:

- `Assets/` as the canonical Unity asset root.
- `ProjectSettings/` for the production Unity project.
- `Packages/manifest.json` and any minimal package lock/config files Unity
  requires for the selected project shell.
- One launchable dev entry scene or equivalent temporary entry path.
- Verification evidence under `tests/evidence/S2-FOUNDATION-01/`.
- Move existing non-Unity production fixture data from `assets/data/**` to
  `data/**` so the lowercase folder does not collide with Unity's canonical
  `Assets/` root on case-sensitive platforms.

## Out Of Scope

- No hub gameplay, faction gameplay, Save/Load gameplay, NPC, loot, vendor,
  stash, objective, or visible world-state consequence implementation.
- No multiplayer, FishNet, server authority, PvP, accounts, cloud saves, live
  LLM dialogue, or Tier 2+ runtime feature work.
- No extra playable classes or broad content import.
- No README landing-page rewrite; README remains a known template-facing
  carryover outside this shell story.
- No Save/Load metadata cleanup; `design/gdd/save-load-persistence.md` header
  drift remains deferred until M4 Save/Load story-breaking naturally touches
  the file.
- No `design/gdd/game-concept.md` engine wording cleanup; D001 already governs
  Unity 6.3 LTS + C# + URP, and the known wording drift stays deferred outside
  this shell story.

## Acceptance Criteria

| ID | Criterion | Evidence |
| --- | --- | --- |
| `S2-FND-01` | Production Unity shell exists at the repo root with `ProjectSettings/` and `Packages/manifest.json` committed. | `tests/evidence/S2-FOUNDATION-01/verification.md` |
| `S2-FND-02` | Shell configuration aligns with D001: Unity 6.3 LTS, C#, and URP; no superseded engine choice is encoded in the new shell. | `tests/evidence/S2-FOUNDATION-01/verification.md` |
| `S2-FND-03` | One launchable dev entry scene or equivalent temporary entry path exists and is scoped to shell launchability, not full gameplay. | `tests/evidence/S2-FOUNDATION-01/verification.md` |
| `S2-FND-04` | Unity launch or batchmode smoke is attempted; if the local editor path blocks execution, the blocker is recorded with the attempted command and environment evidence. | `tests/evidence/S2-FOUNDATION-01/verification.md` |
| `S2-FND-05` | T1 negative-scope scan shows no FishNet, networking, server authority, PvP, live LLM, account, or cloud-save implementation introduced by the shell. | `tests/evidence/S2-FOUNDATION-01/verification.md` |
| `S2-FND-06` | Verification artifact records the source trace, changed-file footprint, local gates, and any environment-dependent smoke limitations. | `tests/evidence/S2-FOUNDATION-01/verification.md` |

## Implementation Notes

- Keep the project shell boring and minimal. The goal is to unblock launch,
  smoke, and later First District story work, not to sneak gameplay into the
  foundation batch.
- If Unity generates required project metadata beyond the listed paths, include
  only the files needed for deterministic project load and document the reason
  in verification.
- If local Unity batchmode cannot run from this machine, preserve the attempted
  command and exact blocker in `tests/evidence/S2-FOUNDATION-01/verification.md`
  instead of treating the story as silently verified.

## Dev Implementation Notes

- Dev entry scene path: `Assets/Scenes/_DevEntry.unity`.
- Unity asset root decision: production Unity assets use canonical `Assets/`;
  non-Unity production fixture data now lives under `data/**`.
- Unity-generated metadata policy: commit deterministic shell metadata under
  `Assets/**`, `ProjectSettings/**`, `Packages/manifest.json`, and
  `Packages/packages-lock.json`; keep local Unity output ignored.
- Empty generated `Assets/Resources/` output was removed; this shell does not
  introduce `Resources` folder usage.
- The shell scene contains only a camera, light, floor blockout, Cleric marker,
  and shell-only district marker. It does not implement hub, faction,
  Save/Load, NPC, loot, vendor, objective, or world-state gameplay.

## Blockers

- None for implementation. Code review returned approved with notes; awaiting `/story-done`.

## Completion Notes

**Completed:** 2026-05-09
**Verdict:** COMPLETE WITH NOTES
**Criteria:** 6/6 passing
**Deferred/Untested Criteria:** None
**Test Evidence:** `tests/evidence/S2-FOUNDATION-01/verification.md`
**GDD/ADR Deviations:** None
**Code Review:** Code/engine review returned APPROVED WITH SUGGESTIONS; QA/testability review returned APPROVED WITH NOTES. No blocking findings.
**Scope Notes:** M1 Unity project shell is complete. This story created the shell only; hub, faction, Save/Load, NPC, loot, vendor, objective, and world-state gameplay remain outside this story.
**Watch Items:** Build-settings GUID parity remains a later build-pipeline hardening item; Unity EditMode smoke exits `0` but has no results XML until Unity Test Runner assemblies exist; the combat test bridge still preserves legacy copied output under `assets/data/combat/**` while sourcing production data from `data/combat/**`.
**Next Recommended:** Human-in-the-loop Unity launch verification before M2 story-breaking: open Unity `6000.3.14f1`, load `Assets/Scenes/_DevEntry.unity`, enter Play mode, confirm the camera, Cleric marker, blockout floor, and `FirstDistrict_ShellOnly_NoGameplay` marker render/stabilize, and capture findings before choosing `/quick-design M2-combat-camp-loop` or `/create-stories`.
