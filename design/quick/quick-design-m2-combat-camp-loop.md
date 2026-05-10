# Quick Design: M2 Combat Camp Loop

**Type:** Addition
**System:** Combat Core plus Unity runtime wrapper
**Date:** 2026-05-10
**Spec path:** `design/quick/quick-design-m2-combat-camp-loop.md`
**Sprint:** 2
**Milestone:** M2 - Combat Camp Loop
**Confidence:** High for design shape and scope; medium for exact story estimates until Unity runtime adapter implementation exposes real friction.

## Change Summary

M2 turns the verified `_DevEntry.unity` shell into a temporary combat-camp proving ground: one Cleric, one safe rest point, one short pull lane, three enemy-role anchors, and a repeatable `pull -> fight -> sit/med -> repeat` loop. It does not add the later Sprint 2 objective, NPC, loot, vendor/stash, save/load, or faction-consequence work.

The point of M2 is not to redesign combat. The existing engine-agnostic Combat Core remains the gameplay source of truth; Unity wraps it with a thin runtime adapter, scene anchors, player input, and smoke evidence.

## Source List

Verification method: live repository reads with `git status --short --branch`, `git log --oneline -6`, `Get-Content`, `rg`, and `Test-Path` on 2026-05-10.

| Source | Use |
| --- | --- |
| `production/sprints/sprint-2.md:10` through `production/sprints/sprint-2.md:23` | Sprint 2 First District target. |
| `production/sprints/sprint-2.md:46` | M2 proof: three enemy types support pull, fight, med-break, and recovery pacing. |
| `production/sprints/sprint-2.md:60` through `production/sprints/sprint-2.md:78` | Operating model and Tier 2+ cuts preserved. |
| `production/sprint-status.yaml:5` and `production/sprint-status.yaml:15` through `production/sprint-status.yaml:16` | Current Sprint 2 goal and M2 story-breaking gate. |
| `production/session-state/active.md:8` through `production/session-state/active.md:12` | M2 is the current next action after Unity launch verification. |
| `production/session-state/active.md:71` through `production/session-state/active.md:75` | `_DevEntry.unity` launch verification complete; runner reusable for later Sprint 2 smoke gates. |
| `design/gdd/game-concept.md:121` through `design/gdd/game-concept.md:129` | Moment-to-moment and camp-loop pacing. |
| `design/gdd/game-concept.md:315` through `design/gdd/game-concept.md:323` | T1 core hypothesis and MVP constraints. |
| `design/gdd/combat-core.md:37` through `design/gdd/combat-core.md:49` | Combat Core source-of-truth scope, Attack toggle amendment, and Cleric-only solo-trash boundary. |
| `design/gdd/combat-core.md:113` through `design/gdd/combat-core.md:119` | Explicit Attack toggle and pull model. |
| `design/gdd/combat-core.md:174` through `design/gdd/combat-core.md:176` | Sitting and med-break regeneration rules. |
| `design/gdd/combat-core.md:429` through `design/gdd/combat-core.md:431` | Existing SoloTrash, TwoTrash, and NamedSoloBlock fixture anchors. |
| `design/gdd/combat-core.md:800` through `design/gdd/combat-core.md:817` | FEEL-01 through FEEL-04 acceptance targets. |
| `design/gdd/combat-core.md:904` through `design/gdd/combat-core.md:917` | Combat Core non-goals. |
| `design/gdd/systems-index.md:35` through `design/gdd/systems-index.md:45` | Combat Core, Enemy AI, Faction Sim, and Zone Control system positions. |
| `design/gdd/systems-index.md:111` through `design/gdd/systems-index.md:127` | Dependency layering for Combat Core, Creature / Enemy AI, Faction State Simulation, and Zone Control. |
| `DECISIONS.md:12` through `DECISIONS.md:18` | Unity 6.3 LTS + C# + URP lock. |
| `DECISIONS.md:32` through `DECISIONS.md:42` | FishNet deferred; T1 is strictly single-player offline. |
| `DECISIONS.md:48` through `DECISIONS.md:61` | T1 single-player offline, local save, no netcode/account/server/live LLM. |
| `DECISIONS.md:339` through `DECISIONS.md:361` | D012 combat-feel validation and Combat Core amendment. |
| `DECISIONS.md:415` through `DECISIONS.md:462` | D014 clean-state solo-trash target revalidation. |
| `production/stories/s2-foundation-01-unity-project-shell.md:93` through `production/stories/s2-foundation-01-unity-project-shell.md:102` | `_DevEntry.unity` shell contents and no-gameplay boundary. |
| `production/stories/s2-foundation-01-unity-project-shell.md:119` through `production/stories/s2-foundation-01-unity-project-shell.md:120` | S2-FOUNDATION-01 watch items and M2 handoff. |
| `tests/evidence/S2-FOUNDATION-01/verification.md:57` through `tests/evidence/S2-FOUNDATION-01/verification.md:62` | M1 shell acceptance evidence. |
| `tests/evidence/S2-FOUNDATION-01/verification.md:128` through `tests/evidence/S2-FOUNDATION-01/verification.md:135` | Shell-only scene scope and watch items. |
| `tests/evidence/S2-FOUNDATION-01/unity-cli-launch-verification-20260510.md:5` through `tests/evidence/S2-FOUNDATION-01/unity-cli-launch-verification-20260510.md:22` | CLI launch verification PASS checks. |
| `Assets/Editor/GravenspireLaunchVerificationRunner.cs:17` through `Assets/Editor/GravenspireLaunchVerificationRunner.cs:18` | Current runner hardcodes scene and S2 evidence path. |
| `Assets/Editor/GravenspireLaunchVerificationRunner.cs:54` through `Assets/Editor/GravenspireLaunchVerificationRunner.cs:59` | Current runner shell-object and render checks. |
| `Assets/Scenes/_DevEntry.unity:133`, `:218`, `:330`, `:429`, `:539` | Current verified scene objects. |
| `data/combat/t1-combat-fixtures.json:496` through `data/combat/t1-combat-fixtures.json:538` | Existing encounter fixture ids for runtime/story acceptance inputs. |

## Assumptions

- M2 may add temporary dev-scene gameplay objects to `_DevEntry.unity`, because Sprint 2 has already selected `_DevEntry.unity` as the verified runtime foundation.
- M2 should use existing Combat Core domain code and fixture data rather than authoring Unity-only combat math.
- M2 can use simple placeholders for enemy visuals, animation, and interaction affordances as long as the playable loop is real and evidence-producing.
- Story files and routing state are intentionally not updated by this quick design; those belong to the later `/create-stories` pass.

## Facts

- Sprint 2 targets a 20-30 minute offline First District slice with one Cleric, one district, three enemy types, one named NPC, one faction presence, one objective, one loot table, one vendor or stash, one save/load flow, and one visible world-state change after player action (`production/sprints/sprint-2.md:10` through `production/sprints/sprint-2.md:23`).
- M2 specifically proves that three enemy types support pull, fight, med-break, and recovery pacing (`production/sprints/sprint-2.md:46`).
- `_DevEntry.unity` is the verified runtime foundation. The CLI launch verification passed scene load, required shell objects, nonblank camera render, 30-second Play Mode stability, post-play object checks, and no captured errors (`tests/evidence/S2-FOUNDATION-01/unity-cli-launch-verification-20260510.md:11` through `tests/evidence/S2-FOUNDATION-01/unity-cli-launch-verification-20260510.md:22`).
- Combat Core is already engine-agnostic gameplay source for actor state, targeting, auto-attack, casts, hate, resources, events, and HUD-facing projection (`design/gdd/combat-core.md:47`).
- Combat Core explicitly allows T1 Cleric solo trash but not named enemies, camps, or normal multi-pull farming (`design/gdd/combat-core.md:49` and `design/gdd/combat-core.md:57`).
- T1 remains offline, single-player, Cleric-only, and no-networking/no-live-LLM by D002, D003, and Sprint 2 cuts (`DECISIONS.md:32` through `DECISIONS.md:61`; `production/sprints/sprint-2.md:72` through `production/sprints/sprint-2.md:78`).

## M2 Loop Definition

The M2 player loop is:

1. Start in `_DevEntry.unity` at a safe camp/rest point.
2. Acquire or approach a hostile in a short pull lane.
3. Body/LoS pull one hostile. Pulling initializes threat but never turns Attack on automatically.
4. Target the hostile and explicitly toggle Attack on.
5. Resolve the fight through the existing Combat Core mechanics: auto-attack ticks, slow cast, tactical instant use, resource spend, threat, death/kill event, and combat exit.
6. Return to the camp/rest point, sit, med, and watch mana recover through the Combat Core regeneration rules.
7. Repeat the loop against another spawned/respawned trash target, then test overpull and named-blocker boundaries.

The minimum complete feel target is not "win one fight." It is at least two sequential clean pulls with an intentional med break between them, plus one overpull or named-blocker attempt that proves the player cannot treat the camp as a modern solo-cleave arena.

## Enemy Type Sketch

### 1. Baseline Trash

Role: clean single-pull enemy.

Behavior:

- Starts idle at a single anchor.
- Aggros through body/LoS pull.
- Uses simple melee pressure only for M2 unless an existing Combat Core fixture already supplies a legal spell/instant behavior.
- Is tuned through the existing `SoloTrash_EvenCon_T1` fixture family.

Purpose:

- Proves the first playable pull, fight, med-break, repeat loop.
- Validates FEEL-01 as runtime-facing behavior: clean-state Cleric wins should be reliable but leave meaningful health or mana pressure.

Evidence anchor:

- `design/gdd/combat-core.md:429`
- `design/gdd/combat-core.md:800` through `design/gdd/combat-core.md:805`
- `data/combat/t1-combat-fixtures.json:496` through `data/combat/t1-combat-fixtures.json:509`

### 2. Linked / Patrol Trash

Role: overpull pressure enemy.

Behavior:

- Exists as either a second nearby linked trash anchor or a simple patrol across the pull lane.
- Can enter the fight within the existing two-trash overpull window when the player pulls badly.
- Does not require broad Creature / Enemy AI yet; simple anchor movement or static linked placement is enough for M2 if it proves the danger.

Purpose:

- Teaches that careful pulling is the game.
- Proves normal two-trash farming is not viable.

Evidence anchor:

- `design/gdd/combat-core.md:430`
- `design/gdd/combat-core.md:812` through `design/gdd/combat-core.md:814`
- `data/combat/t1-combat-fixtures.json:514` through `data/combat/t1-combat-fixtures.json:523`

### 3. Named Blocker

Role: visible camp boundary.

Behavior:

- Present in the space as a targetable named/camp anchor.
- May be stationary for M2.
- Must not be balanced as normal solo-farm content.
- Does not drop loot, start objectives, or trigger faction consequence in M2.

Purpose:

- Shows the absence of a group without adding companions or party classes.
- Gives the camp shape: trash is for the loop; named is pressure and future content.

Evidence anchor:

- `design/gdd/combat-core.md:431`
- `design/gdd/combat-core.md:808` through `design/gdd/combat-core.md:810`
- `data/combat/t1-combat-fixtures.json:528` through `data/combat/t1-combat-fixtures.json:538`

## Unity Integration Approach

M2 should build a thin Unity runtime wrapper around Combat Core:

- `Assets/**` owns scene objects, MonoBehaviours, input plumbing, camera-facing markers, and smoke-runner entry points.
- `src/gameplay/combat/**` remains the owner of formulas, state machines, events, and fixture interpretation.
- Unity runtime code may hydrate actors from `data/combat/t1-combat-fixtures.json` through existing fixture models/loaders or a narrow adapter, but it must not copy combat formulas into Unity scripts.
- Unity runtime code may project Combat Core state into simple transforms, material states, labels, and debug logs.
- Unity runtime code may record smoke results, but story evidence belongs under story-specific `tests/evidence/S2-M2-*/` paths, not the S2-FOUNDATION-01 evidence file.

M2 should prefer boring implementation surfaces:

- A dev-only combat-loop root in `_DevEntry.unity`.
- A scene-local runtime coordinator for the M2 loop.
- A fixture-backed actor spawner/hydrator.
- A minimal player input adapter for target, Attack toggle, cast/instant, sit/stand.
- A simple enemy anchor/patrol/assist adapter.
- A story-specific editor verification runner or generalized launch runner successor.

Promotion warning:

- If implementation requires a new Creature / Enemy AI GDD, a new Class Design GDD, a new Spell Memorization GDD, a new Zone Control implementation contract, or broad persistence rules, M2 has exceeded quick-design scope and should stop for `/design-system` or `/architecture-decision`.

## Allowed `_DevEntry.unity` Changes

Allowed for M2:

- Add `M2_CombatCampLoopRoot`.
- Add `M2_CampRestPoint`.
- Add `M2_PullLane`.
- Add one or more simple LoS/pull-shaping blockers if needed.
- Add enemy anchors for `M2_BaselineTrash`, `M2_LinkedOrPatrolTrash`, and `M2_NamedBlocker`.
- Add temporary marker meshes/materials that make the loop readable in Play Mode.
- Add runtime-only scripts/components needed to drive and verify the combat loop.

Not allowed for M2:

- Objective giver or named friendly NPC.
- Loot table, item pickup, vendor, stash, or inventory economy.
- Save/load flow.
- Visible faction consequence or Zone Control state mutation beyond temporary debug labels.
- Full hub, second district, or broad First District content pass.
- Networking, FishNet, server authority, PvP, accounts, cloud saves, live LLM, extra classes, or broad companion behavior.

## Acceptance Criteria Candidates

### M2-01 Unity Combat Core Runtime Bridge

- [ ] Unity runtime compiles with a thin adapter over `src/gameplay/combat/**`; no Unity-only duplicate combat formulas are introduced.
- [ ] Runtime can load or receive the existing T1 combat fixtures from `data/combat/t1-combat-fixtures.json`.
- [ ] `_DevEntry.unity` can enter Play Mode with the adapter enabled and no captured errors/exceptions.
- [ ] Runtime smoke records the player actor, at least one hostile actor, fixture ids, and active zone id.
- [ ] Dotnet combat regression still passes.

### M2-02 Single Trash Pull + Med Loop

- [ ] `_DevEntry.unity` contains the camp rest point, pull lane, Cleric marker, and baseline trash anchor.
- [ ] Player can body/LoS pull one baseline trash enemy.
- [ ] Pull does not automatically enable Attack.
- [ ] Player can target, toggle Attack, resolve a clean single-trash fight, exit combat, sit, recover mana, and repeat a second pull.
- [ ] Runtime smoke records pull start, Attack on/off transitions, hostile defeat, combat exit, sit/med start, mana restoration, and no errors/exceptions.

### M2-03 Linked Trash Overpull

- [ ] `_DevEntry.unity` contains a linked or patrol trash arrangement that can create a two-trash overpull.
- [ ] Bad pull smoke records two same-band trash enemies entering hate within the FEEL-03 window.
- [ ] Outcome is dangerous per Combat Core intent: player loses, flees, or survives below the health/mana danger threshold.
- [ ] Clean single-trash loop from M2-02 still passes after this addition.

### M2-04 Named Blocker + Camp Boundary

- [ ] `_DevEntry.unity` contains a visible named blocker anchor using the existing named fixture family.
- [ ] Runtime smoke verifies the named blocker is targetable/present but not treated as normal solo-trash farm content.
- [ ] Named attempt evidence records either loss/flee/failed solo attempt or a blocked attempt consistent with FEEL-02.
- [ ] M2 does not add loot, objective, faction consequence, Save/Load, companion, or extra-class behavior.

## Proposed Story Split

| Story | Purpose | Primary evidence |
| --- | --- | --- |
| M2-01 Unity Combat Core Runtime Bridge | Make Unity consume existing Combat Core through a thin runtime adapter. | Compile/Play Mode smoke, fixture load/hydration smoke, dotnet regression. |
| M2-02 Single Trash Pull + Med Loop | First playable pull, fight, sit/med, repeat loop. | Story-specific Unity runner proving sequential clean pulls and med break. |
| M2-03 Linked Trash Overpull | Add second enemy role and prove bad pulls are dangerous. | Unity runner plus Combat Core overpull metric/log evidence. |
| M2-04 Named Blocker + Camp Boundary | Add third enemy role and prove named/camp boundary. | Unity runner proving named presence and non-solo/future-content boundary. |

## Evidence Plan

Each M2 story should produce:

- `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` result.
- Unity Play Mode or batchmode runner evidence under a story-specific path such as `tests/evidence/S2-M2-01/verification.md`.
- Negative-scope scan for FishNet, networking, server authority, PvP, accounts, cloud saves, live LLM, extra classes, and broad companion implementation.
- `git diff --check`.
- `.githooks/pre-commit`.
- For code-bearing Unity stories: code review focused on authored adapter/runner code, not Unity-generated config noise.

The existing `GravenspireLaunchVerificationRunner` should not be reused as-is because it hardcodes both the S2-FOUNDATION-01 evidence path and shell-only checks (`Assets/Editor/GravenspireLaunchVerificationRunner.cs:17` through `Assets/Editor/GravenspireLaunchVerificationRunner.cs:18`). M2 should either refactor it into a parameterized base runner or add a successor such as `GravenspireCombatCampVerificationRunner` that writes to story-specific evidence paths.

Minimum M2 runner checks:

- scene loaded;
- M2 root/rest point/pull lane/enemy anchors exist;
- camera render nonblank;
- Play Mode stable for at least 30 seconds or story-specific loop duration;
- no captured errors/exceptions;
- story-specific combat assertion passes;
- S2 shell objects still exist unless intentionally superseded.

## Deferred / Non-Goals

Deferred watch items:

- Build-settings GUID parity stays deferred unless M2 touches build settings. It was accepted as non-blocking after S2-FOUNDATION-01 because Unity batchmode generated and reopened the project successfully (`tests/evidence/S2-FOUNDATION-01/verification.md:135`).
- Unity Test Runner results XML remains deferred unless M2 creates Unity test assemblies. S2-FOUNDATION-01 documented that EditMode smoke returned `0` without results XML because no Unity Test Runner assemblies existed (`tests/evidence/S2-FOUNDATION-01/verification.md:88`).
- Save/Load metadata drift stays deferred until M4 Save/Load story-breaking naturally touches it (`production/sprint-status.yaml:32`).
- README and game-concept engine wording drift stay deferred outside M2.
- Human death-moment playtest remains a Sprint 2 planning input, not an M2 blocker unless player death feel is reopened.

M2 story candidates from S2-FOUNDATION-01 watch items:

- Runner successor/refactor, because M2 needs story-specific Unity runtime smoke evidence.
- Data bridge/scanner alignment, if Unity runtime consumes `data/combat/**`.
- Unity Test Runner assembly creation only if it becomes the cleanest way to capture M2 Play Mode evidence.

Explicit M2 non-goals:

- No objective, quest, named friendly NPC, loot, vendor/stash, Save/Load, or visible faction consequence.
- No new Combat Core architecture.
- No class expansion beyond Cleric.
- No broad Creature / Enemy AI beyond the minimum hostile anchor/pull/patrol behavior required to prove M2.
- No networking, FishNet, server authority, PvP, accounts, cloud saves, live LLM, or companion roster behavior.

## Affected Systems

| System | Impact | Action required |
| --- | --- | --- |
| Combat Core | Source of truth for loop mechanics. | Consume existing APIs/fixtures; do not duplicate formulas. |
| World Structure | `_DevEntry.unity` remains temporary dev foundation. | Keep scene changes local and shell-compatible. |
| Creature / Enemy AI | Minimum anchor/pull/patrol behavior may be needed. | Keep M2 behavior narrow; promote if real AI contract expands. |
| Class Design / Spell Memorization | Not authored yet. | Use existing prototype Cleric spell/instant fixture equivalents only. |
| Zone Control / Faction State | Not implemented in M2. | Preserve kill-credit/faction hooks; do not mutate real faction state yet. |
| Layer 1 HUD | Not implemented in M2. | Runtime may expose debug-readable state; do not build final HUD. |

## QA Checks

- Verify the quick-design source remains in `design/quick/quick-design-m2-combat-camp-loop.md`.
- Before `/create-stories`, ensure the M2 story files cite this quick design plus the source list above.
- During implementation, require story-specific Unity runtime evidence and combat regression evidence for every M2 story.
- Preserve the Sprint 2 operating model: implement one small feature, play it immediately, write down what felt bad, fix the worst thing, commit, repeat (`production/sprints/sprint-2.md:62` through `production/sprints/sprint-2.md:63`).

## Next Action

Run:

```text
/create-stories M2-combat-camp-loop
```

Use this quick design as the design input for the M2-01 through M2-04 story split. Do not update sprint status or `production/session-state/active.md` until stories are actually opened.
