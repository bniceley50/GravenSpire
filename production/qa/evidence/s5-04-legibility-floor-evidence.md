# S5-04 Legibility Floor Evidence

> **Story**: S5-04 - Land S4-01 Legibility Floor (Camera + Debug-HUD Isolation)
> **Status**: PASS WITH NOTES - manual Play Mode and post-fix automated preservation complete
> **Date**: 2026-06-09
> **Branch**: `codex/s4-01-play-camera-debug-hud-isolation`
> **Base**: `origin/main` at `1be9ee3`

## 1. Scope

S5-04 carries the parked S4-01 camera and legacy M2 debug-HUD isolation work
onto the current Sprint 5 base. It does not change `_DevEntry.unity`; the
legibility floor is code-only in `Assets/Scripts/M2SingleTrashMedLoopController.cs`
and `Assets/Scripts/S3PlayerInteractionHarness.cs`.

No optional vitals readout or focused interaction-prompt text was added in this
Codex pass. Those levers remain deferred because the story can land the minimum
S4-01 floor without expanding into the deferred S4-02/S4-03/S4-04 HUD scope.

## 2. Code Evidence

| Claim | Evidence | Verification method |
|---|---|---|
| Branch is rebased onto current `origin/main`. | `git rebase origin/main` completed successfully; `git status --branch` reports the branch up to date with `origin/main`. | Git command output in Codex session. |
| Camera steering is player-controlled and local-only. | `Assets/Scripts/M2SingleTrashMedLoopController.cs:203`, `:632-644` call and implement `HandleCameraSteering()`. | Source read after objective free-walk fix. |
| Camera does not pull toward NPCs, relics, vendors, blockers, or other POIs. | `Assets/Scripts/M2SingleTrashMedLoopController.cs:2113-2124` derives camera position only from `_playerMarker` plus `_cameraYawDegrees`. | Source read after objective free-walk fix. |
| Legacy M2 debug HUD, proximity body-pull, floor repaint, fog override, ambient-light override, and black camera background are suppressed during human objective free-walk. | `Assets/Scripts/M2SingleTrashMedLoopController.cs:205-207`, `:222-225`, `:275-299`, `:2127-2132`, `:2149-2154`. | Source read after the objective free-walk fix. |
| Explicit combat/debug entry remains available. | `Assets/Scripts/M2SingleTrashMedLoopController.cs:667`, `:696-718` still routes `V` to `ApproachAndPull()` and through `TryStartBodyPull()`. | Source read after the objective free-walk fix. |
| Unity batchmode preservation scenarios are not suppressed by the human-only objective guard. | `Assets/Scripts/M2SingleTrashMedLoopController.cs:277-280` returns false for `Application.isBatchMode` or `_smokeRunning`. | Source read after the objective free-walk fix. |
| The S3 prompt keeps its range gate, but renders through the same QHD/editor GUI scaling pattern as the M2 HUD. | `Assets/Scripts/S3PlayerInteractionHarness.cs:218-238` scales `GUI.matrix`; `Assets/Scripts/S3PlayerInteractionHarness.cs:358` still sets `PromptVisible` from the nearest in-range target. | Source read after the prompt-scaling fix. |
| CLIENT-LOCAL annotation is present for both M2 presentation/camera changes. | `Assets/Scripts/M2SingleTrashMedLoopController.cs:274` and `:631`. | Source read after the objective free-walk fix. |

## 3. Manual Failure Diagnosis Captured 2026-06-09

| Finding | Evidence | Resolution in this diff |
|---|---|---|
| F-A: Play Mode was stomping the produced court presentation. | `Assets/Scripts/M2SingleTrashMedLoopController.cs:2127-2154` now shows the legacy floor repaint and presentation override behind the objective free-walk guard; before this diff those methods ran unguarded. | Objective free-walk now preserves authored S5-03 lighting/materials. |
| F-B: The observed "camera auto-pull" was M2 proximity body-pull, not camera logic. | `Assets/Scripts/M2SingleTrashMedLoopController.cs:205-207` now blocks the per-frame `TryStartBodyPull()` call during objective free-walk; `Assets/Scripts/M2SingleTrashMedLoopController.cs:2113-2124` keeps the camera following only `_playerMarker`. | Walking near Morrvik should no longer start M2 combat unless the player explicitly presses `V`. |
| F-C: The S3 prompt was fixed-pixel and too easy to miss at QHD. | `Assets/Scripts/S3PlayerInteractionHarness.cs:218-238` now scales the prompt/feedback GUI; `Assets/Scripts/S3PlayerInteractionHarness.cs:358` leaves the 2 m prompt range intact. | Prompt visibility becomes a rendering/readability question, not a tiny-label question. |

## 4. Manual Play Mode Confirmation

Manual Play Mode check reported by Brian on 2026-06-09 against the open
`GravenSpire-codex - _DevEntry` Unity editor, after script recompilation.

| Check | Result | Notes |
|---|---|---|
| Camera readable / player-steered with WASD + Q/E. | PASS WITH NOTES | Camera behavior works, but the player still needs a real character body/orientation read to know which way they are turning. Route to character/NPC body work, not S5-04 camera code. |
| No auto-pull walking up to Caretaker Morrvik. | PASS | Confirms the M2 proximity body-pull no longer fires during objective free-walk. |
| `Press E` prompt visible within about 2 m of the Caretaker. | PASS | Confirms the prompt is now readable enough for this gate. |
| M2 debug HUD absent while free-walking. | PASS | Expected behavior: the M2 HUD may still appear after explicit `V` pull. |

Additional play-read note: the current M2 harness exposes auto-attack plus Smite
only. Combat design already names `LesserHeal_T1_Prototype`, `Bash_T1_Prototype`,
and `DefensivePrayer_T1_Prototype` in `design/gdd/combat-core.md:415` and
`:422-423`, and fixture-package inclusion at `design/gdd/combat-core.md:659`;
the Play Mode harness UI currently exposes only Smite through
`Assets/Scripts/M2SingleTrashMedLoopController.cs:262-268`, `:872-939`.
This is a downstream combat ability-surface/backlog item, not part of S5-04's
camera/HUD isolation closure.

## 5. Automated Gates

| Gate | Result | Notes |
|---|---|---|
| `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` | PASS | 189/189 tests passed after the objective free-walk fix. |
| `dotnet format tests\Gravenspire.Combat.Tests.csproj --verify-no-changes --exclude-diagnostics IDE1006` | PASS | No formatting changes required after the fix. |
| `dotnet format prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj --verify-no-changes --exclude-diagnostics IDE1006` | PASS | Matches local pre-commit hook target after the fix. |
| `git diff --check` | PASS | No whitespace/conflict-marker issues in the current diff after the fix. |
| `git diff --name-only -- Assets/Scenes/_DevEntry.unity ProjectSettings Packages` | PASS | Returned no paths after the code fix. |

Detailed fix-gate notes: `tests/evidence/S5-04/s5-04-objective-freewalk-fix-20260609.md`.

## 6. M2 Preservation Reruns

Each preservation runner ran in a separate Unity `6000.3.14f1` batchmode
invocation, without `-quit`, because these runners enter Play Mode
asynchronously and call `EditorApplication.Exit()` themselves.

| Runner | Evidence | Result |
|---|---|---|
| M2-02 single-trash med-loop | `tests/evidence/S5-04/m2-02-preservation-20260609-smoke.md` | PASS; preservation mode true; builder skipped true; builder invoked false. |
| M2-03 linked-trash overpull | `tests/evidence/S5-04/m2-03-preservation-20260609-smoke.md` | PASS; preservation mode true; builder skipped true; builder invoked false. |
| M2-04 named-blocker camp boundary | `tests/evidence/S5-04/m2-04-preservation-20260609-smoke.md` | PASS; preservation mode true; builder skipped true; builder invoked false. |

**Post-fix rerun status:** complete. All three preservation smokes reran after
the objective free-walk fix with preservation mode true, builder skipped true,
builder invoked false, and no controller errors:

- `tests/evidence/S5-04/m2-02-preservation-20260609-smoke.md:7-10`, `:36`
- `tests/evidence/S5-04/m2-03-preservation-20260609-smoke.md:7-10`, `:35`, `:44`
- `tests/evidence/S5-04/m2-04-preservation-20260609-smoke.md:7-10`, `:37`, `:49`

## 7. Scene Discipline

`git diff --name-only -- Assets/Scenes/_DevEntry.unity ProjectSettings Packages`
returned no paths after the objective free-walk fix. This supports the S5-04
no-scene-delta acceptance: no scene, ProjectSettings, or package file is part
of the S5-04 diff.

## 8. Closure Verdict

S5-04 acceptance evidence is complete: manual Play Mode confirmation passed with
two non-blocking notes, .NET/format/diff gates passed, post-fix M2 preservation
reruns passed 3/3 with builders skipped, and no scene/settings/package diff was
introduced.

Follow-up notes to route outside S5-04:

- Player orientation needs a real character body/readable facing cue.
- Ability breadth needs a separate combat/class surface story; the current M2
  harness exposes auto-attack plus Smite only.
