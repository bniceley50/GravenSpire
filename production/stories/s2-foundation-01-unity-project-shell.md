# S2-FOUNDATION-01 - Unity Project Shell

**Status:** Blocked - pending Sprint 2 QA plan
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
blocked until Sprint 2 `/qa-plan sprint` exists. Do not run
`/dev-story S2-FOUNDATION-01` or start new Sprint 2 feature implementation
before that QA plan is written.

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

- `ProjectSettings/` for the production Unity project.
- `Packages/manifest.json` and any minimal package lock/config files Unity
  requires for the selected project shell.
- One launchable dev entry scene or equivalent temporary entry path.
- Verification evidence under `tests/evidence/S2-FOUNDATION-01/`.

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

## Blockers

- Sprint 2 `/qa-plan sprint` has not been run. This story remains blocked for
  `/dev-story` until that QA plan exists.
