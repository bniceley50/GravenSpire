# Sprint 2 - Gravenspire T1: The First District

> **Status**: Active implementation gate / foundation story ready
> **Generated**: 2026-05-09
> **Current Head**: aa785a0
> **Prior Sprint Gate**: Sprint 1.5 close-out gates complete 3/3

## Goal

Build toward **Gravenspire T1: The First District**: a 20-30 minute offline slice that proves the game can put a player in one cursed-city district, let them act through Classic-EQ Cleric pacing, and show that the world visibly changes because of what they did.

The first playable build target is deliberately narrow:

- one playable character archetype: Cleric
- one district of the cursed city
- three enemy types
- one named NPC
- one faction presence
- one quest/objective
- one loot table
- one vendor or stash
- one save/load flow
- one visible world-state change after player action

Working example, not a final naming lock: the **Mournwall Cemetery District**, a local undead faction presence, a named cemetery caretaker, a relic choice, and a visible consequence such as changed patrols, opened access, or altered faction signage after the player resolves the objective.

## Gate State

Sprint 1.5 close-out is complete:

1. `/smoke-check sprint` recorded `PASS WITH WARNINGS`.
2. `/team-qa sprint` recorded `APPROVED WITH CONDITIONS`.
3. `/gate-check` recorded `PASS`.

Sprint 2 has rolled forward. The QA plan exists and M1 shell implementation is complete. Before M2:

1. Run human Unity launch verification for `Assets/Scenes/_DevEntry.unity`.
2. Capture findings about shell visibility/stability before combat-camp-loop story-breaking.
3. Use the findings to choose `/quick-design M2-combat-camp-loop` or direct `/create-stories`.

## Milestone Structure

| Milestone | Name | Purpose | First Proof |
|---|---|---|---|
| M1 | Player In World | Create the Unity project shell and launchable dev entry path. | Player can launch into a controlled district blockout as the Cleric. |
| M2 | Combat Camp Loop | Put the existing Combat Core into a playable encounter loop. | Three enemy types support pull, fight, med-break, and recovery pacing. |
| M3 | Objective + NPC + Loot | Add the minimum authored reason to play the district. | One named NPC gives or frames an objective; one loot table and one vendor or stash close the loop. |
| M4 | Save/Load Flow | Preserve player and district progress locally. | The player can save, exit, reload, and retain core state without repair-by-load behavior. |
| M5 | Faction Consequence | Connect moment-to-moment action to visible world state. | Completing the objective visibly changes the district or faction presence. |

## Story Ledger

| Story | Status | Commits | Evidence |
|---|---|---:|---|
| `S2-COMBAT-01` Fix init-only property preservation in CombatActorState transitions | Complete | `5b8a017` -> `a7269cb` | `tests/evidence/S2-COMBAT-01/verification.md` |
| `S2-FOUNDATION-01` Unity project shell | Complete | `f5f74dc` | `tests/evidence/S2-FOUNDATION-01/verification.md` |

Next gate: human Unity launch verification before M2 story-breaking.

## Operating Model Calibration

- Prioritization test: **Does this make the first 10 minutes of playable Gravenspire better?** If no, defer it.
- Development loop: implement one small feature, play it immediately, write down what felt bad, fix the worst thing, commit, repeat.
- Cleric-only lock: T1 proves one playable archetype. Extra classes are deferred.
- Use lighter ceremony for bounded documentation, evidence, provenance, and closure fixes.
- Keep full rigor for cross-contract code, persistence/state transitions, frozen contracts, and fixture/harness logic.
- Chain tables contain only actual full SHAs and grow append-only; no pending placeholder rows.
- External whole-codebase review is valuable once per sprint or tier transition; findings enter the next sprint unless they are immediate ship-blockers.

## Tier 2+ Cuts Preserved

- No multiplayer, FishNet, server authority, PvP, accounts, or cloud saves.
- No live LLM dialogue.
- No extra playable classes.
- No huge world or second district.
- No deep economy.
- No broad AI companion system.
- No faction simulation beyond the local visible consequence needed for the First District slice.

## Known Findings

- `production/session-state/active.md` previously carried stale World Structure review wording from the first MAJOR REVISION round; latest source is APPROVED in `design/gdd/reviews/world-structure-review-log.md`.
- `design/gdd/save-load-persistence.md` header still says `Status: In Design`; its review log says APPROVED. Defer cleanup until M4 Save/Load story-breaking naturally touches the file.
- `design/gdd/game-concept.md` still says engine TBD / Godot pinned; D001 locks Unity 6.3 LTS + C# + URP.
- README remains template-facing and should later become a Gravenspire landing page.
- Sprint 1.5 carryovers remain inputs to Sprint 2 QA planning, especially human death-moment playtest, QA-02-01 wording, and evidence-authoring norms.
- Production Unity shell is present after `S2-FOUNDATION-01`; human launch verification remains before M2 story-breaking.

## Next Gate

M1 complete at `f5f74dc`; next gate is human Unity launch verification before M2 story-breaking.
Minimum story shape:

- Create the Unity project shell at the production root.
- Add `ProjectSettings/` and `Packages/manifest.json`.
- Add one launchable dev entry scene or equivalent temporary entry path.
- Keep scope to shell, launchability, and smoke-testability.
- Do not implement hub, faction, Save/Load, NPC, loot, vendor, or objective gameplay in the shell story.

## Definition Of Done For Sprint 2 Planning

- [x] `S2-FOUNDATION-01` story file exists.
- [x] Sprint 2 `/qa-plan sprint` exists.
- [x] QA plan names the First District target and accepted Sprint 1.5 carryovers.
- [x] No new Sprint 2 feature implementation started before the QA plan.
