# S2-M2-03 Verification

**Story:** `production/stories/s2-m2-03-linked-trash-overpull.md`
**Result:** PASS
**Verification date:** 2026-05-13
**Current HEAD:** `745d65e`
**Story manifest reference:** Sprint 2, 2026-05-10 (`production/stories/s2-m2-03-linked-trash-overpull.md:9`)

## Evidence Artifacts

| Artifact | Purpose |
| --- | --- |
| `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513-smoke.md` | Unity Play Mode smoke output for linked-trash overpull and clean-loop preservation. |
| `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513.log` | Unity batchmode runner log; records generated scene objects and exit code. |
| `tests/evidence/S2-M2-03/verification.md` | This acceptance-criteria and gate summary. |

## Acceptance Criteria Trace

| AC | Status | Evidence |
| --- | --- | --- |
| `S2-M2-03-01` | PASS | The story requires a linked/patrol trash arrangement at `production/stories/s2-m2-03-linked-trash-overpull.md:78`. The builder creates `M2_LinkedTrash` at `Assets/Editor/GravenspireM2SingleTrashLoopBuilder.cs:36`; the scene contains `M2_LinkedTrash` at `Assets/Scenes/_DevEntry.unity:266`; the smoke recorded `linked_trash_anchor_exists` and `linked_trash_arrangement_present` at `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513-smoke.md:16` and `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513-smoke.md:20`. |
| `S2-M2-03-02` | PASS | The story requires two same-band trash enemies entering hate inside FEEL-03 at `production/stories/s2-m2-03-linked-trash-overpull.md:79`. The smoke recorded `two_hostiles_entered_hate` and `feel03_hate_window_met` at `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513-smoke.md:21` through `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513-smoke.md:22`, with `hostiles_in_hate=2` and `hate_window_seconds=0.0` at `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513-smoke.md:29` through `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513-smoke.md:30`. Integration coverage asserts the same conditions at `tests/integration/gameplay/combat/combat_runtime_linked_trash_overpull_test.cs:15` through `tests/integration/gameplay/combat/combat_runtime_linked_trash_overpull_test.cs:24`. |
| `S2-M2-03-03` | PASS | The story requires a dangerous overpull outcome at `production/stories/s2-m2-03-linked-trash-overpull.md:80`. The smoke recorded `dangerous_outcome_recorded` at `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513-smoke.md:23`, `overpull_outcome=forced_flee_threshold` and `ending_health=14/140` at `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513-smoke.md:31` through `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513-smoke.md:32`, and the final ending-health event at `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513-smoke.md:63`. Integration coverage requires `player_lost` or `forced_flee_threshold` plus low health/mana/death at `tests/integration/gameplay/combat/combat_runtime_linked_trash_overpull_test.cs:28` through `tests/integration/gameplay/combat/combat_runtime_linked_trash_overpull_test.cs:41`; runtime danger capture is implemented at `Assets/Scripts/M2SingleTrashMedLoopController.cs:1412` through `Assets/Scripts/M2SingleTrashMedLoopController.cs:1430`. |
| `S2-M2-03-04` | PASS | The story requires the clean S2-M2-02 single-trash loop to remain passing at `production/stories/s2-m2-03-linked-trash-overpull.md:81`. The smoke recorded `clean_single_trash_loop_preserved` at `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513-smoke.md:24` and `clean_loop_preserved=True` at `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513-smoke.md:34`. |
| `S2-M2-03-05` | PASS | The story requires dotnet regression, T1 negative-scope scan, `git diff --check`, and `.githooks/pre-commit` before closure at `production/stories/s2-m2-03-linked-trash-overpull.md:82` and `production/stories/s2-m2-03-linked-trash-overpull.md:120`. Gate results are recorded below. |

## Manifest Absence

`docs/architecture/control-manifest.md` is absent in this checkout.

Verification method:
- `Test-Path -LiteralPath 'docs/architecture/control-manifest.md'` returned `False`.
- `git ls-files 'docs/architecture/control-manifest.md'` returned no tracked file.
- `rg --files docs/architecture | rg 'control-manifest\.md$'` returned no path.

The active Sprint 2 QA plan explicitly uses the architecture registry fallback when the control manifest is absent at `production/qa/plans/qa-plan-sprint-2-20260509.md:54` and `production/qa/plans/qa-plan-sprint-2-20260509.md:60`. This verification is recorded against HEAD `745d65e` and the story manifest reference `Sprint 2, 2026-05-10`.

## Local Gates

| Gate | Result | Evidence |
| --- | --- | --- |
| Combat regression | PASS | `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` passed `172/172` on 2026-05-13. |
| T1 negative-scope scan | PASS WITH CLASSIFIED DOC HITS | `rg -n -i "FishNet|\bnetworking\b|server authority|server-authority|\bPvP\b|\baccounts?\b|cloud saves?|OpenAI|Anthropic|live LLM|multiplayer|\bWarrior\b|\bEnchanter\b" ...changed S2-M2-03 files...` returned the explicit story out-of-scope line at `production/stories/s2-m2-03-linked-trash-overpull.md:67` plus this verification row's quoted command text; no runtime, test, scene, runner, or smoke evidence hits were returned. |
| Diff hygiene | PASS | `git diff --check` initially found trailing whitespace in `Assets/Scenes/_DevEntry.unity:232`; that in-scope scene whitespace was removed and the rerun passed with no whitespace or conflict-marker findings. |
| Pre-commit hook | PASS | `.githooks/pre-commit` was run with a temporary index staging the S2-M2-03 in-scope files. The hook performs staged `git diff --cached --check` at `.githooks/pre-commit:10`, scans staged C# files with the T1 deny pattern at `.githooks/pre-commit:12` through `.githooks/pre-commit:26`, and returned `[pre-commit] OK`. |

## Runtime Notes

- Unity smoke result is PASS at `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513-smoke.md:7`.
- The smoke captured no runner warnings or errors at `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513-smoke.md:134` through `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513-smoke.md:140`.
- The Unity log includes a local `Licensing::Client` handshake message at `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513.log:76`, but the runner generated the scene objects at `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513.log:337` and wrote smoke evidence with exit code `0` at `tests/evidence/S2-M2-03/unity-linked-trash-overpull-runner-20260513.log:461`.
