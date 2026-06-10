# S5-04 Objective Free-Walk Fix Gates

> **Date**: 2026-06-09
> **Branch**: `codex/s4-01-play-camera-debug-hud-isolation`
> **Status**: PASS WITH NOTES - code gates, manual Play Mode, and Unity preservation passed
> **Scope**: Code-only fix for M2 legacy presentation/proximity pull during S3/S5 objective free-walk plus S3 prompt GUI scaling.

## Code Fix Evidence

| Claim | Evidence | Verification |
|---|---|---|
| Human objective free-walk suppresses per-frame M2 proximity body-pull. | `Assets/Scripts/M2SingleTrashMedLoopController.cs:205-207`, `:275-299` | Source read after patch. |
| Explicit M2 combat entry remains available. | `Assets/Scripts/M2SingleTrashMedLoopController.cs:656-659`, `:707-710` | Source read after patch. |
| M2 floor repaint and presentation override no longer stomp objective free-walk. | `Assets/Scripts/M2SingleTrashMedLoopController.cs:2127-2154` | Source read after patch. |
| Batchmode/smoke scenarios are outside the human free-walk guard. | `Assets/Scripts/M2SingleTrashMedLoopController.cs:277-280` | Source read after patch. |
| S3 prompt and feedback labels now use scaled GUI matrix. | `Assets/Scripts/S3PlayerInteractionHarness.cs:218-238` | Source read after patch. |

## Local Gates

| Command | Result | Evidence |
|---|---|---|
| `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` | PASS | 189/189 tests passed. |
| `dotnet format tests\Gravenspire.Combat.Tests.csproj --verify-no-changes --exclude-diagnostics IDE1006` | PASS | Exit code 0. |
| `dotnet format prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj --verify-no-changes --exclude-diagnostics IDE1006` | PASS | Exit code 0. |
| `git diff --check` | PASS | Exit code 0. |
| `git diff --name-only -- Assets/Scenes/_DevEntry.unity ProjectSettings Packages` | PASS | Returned no paths. |

## Manual Play Mode Check

Manual Play Mode check reported by Brian on 2026-06-09:

| Check | Result |
|---|---|
| Camera readable/player-steered with WASD + Q/E. | PASS WITH NOTES - needs a real character body/orientation read. |
| No auto-pull walking up to Morrvik. | PASS |
| `Press E` prompt visible within about 2 m of the Caretaker. | PASS |
| M2 debug HUD absent while free-walking. | PASS |

Follow-up notes outside S5-04 closure: player/body orientation should route to
the character/NPC body pass; ability breadth should route to a combat/class
surface story because this harness exposes auto-attack plus Smite only.

## Unity Preservation Status

Post-fix M2 preservation reruns completed after the Unity editor closed. Each
runner used a separate Unity batchmode invocation with
`-gravenspirePreservationMode` and `-gravenspireSkipBuilder`, and without
`-quit`.

| Runner | Result | Evidence |
|---|---|---|
| `Gravenspire.Editor.GravenspireM2SingleTrashLoopVerificationRunner.Run` | PASS | `tests/evidence/S5-04/m2-02-preservation-20260609-smoke.md:7-10`, `:36` |
| `Gravenspire.Editor.GravenspireM2LinkedTrashOverpullVerificationRunner.Run` | PASS | `tests/evidence/S5-04/m2-03-preservation-20260609-smoke.md:7-10`, `:35`, `:44` |
| `Gravenspire.Editor.GravenspireM2NamedBlockerVerificationRunner.Run` | PASS | `tests/evidence/S5-04/m2-04-preservation-20260609-smoke.md:7-10`, `:37`, `:49` |

This confirms the human objective free-walk guard did not suppress the automated
M2 preservation scenarios.
