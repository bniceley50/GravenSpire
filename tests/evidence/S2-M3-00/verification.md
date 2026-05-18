# S2-M3-00 Verification

**Story:** `production/stories/s2-m3-00-scenario-smoke-handoff-cleanup.md`
**Result:** PASS (closure verification complete; commit SHA pending explicit commit approval)
**Verification date:** 2026-05-17
**Implementation HEAD:** PENDING COMMIT — closure verification complete in working tree; final commit SHA assigned after explicit commit approval
**Pre-change HEAD:** `dc0f306`
**Story manifest reference:** Sprint 2, 2026-05-14 (`production/stories/s2-m3-00-scenario-smoke-handoff-cleanup.md:9`)

## Summary

This is a refactor/extraction story. It introduces a shared editor-runner helper class for cross-runner scenario-smoke utilities and backports the diagnosed `UnityEditor.Search.SearchInit` editor-startup noise filter (previously local to the S2-M2-04 runner per `production/sprint-status.yaml:44`) into a single source of truth that all five existing scenario-smoke runners now consume, and that future M3 runners can consume without copying logic.

There are no gameplay behavior changes. The five runner edits add an early `return` on a diagnosed, isolated editor-startup noise condition that is outside any Gravenspire scenario runtime/controller/runner.

## Evidence Artifacts

| Artifact | Purpose |
| --- | --- |
| `Assets/Editor/GravenspireScenarioSmokeRunnerHelpers.cs` | New shared helper class hosting `IsEditorStartupNoise`; designed to grow as further cross-runner helpers are needed by M3 and beyond. |
| `Assets/Editor/GravenspireScenarioSmokeRunnerHelpers.cs.meta` | Unity asset import companion for the new Editor script; matches the project's minimal two-line meta format. Persistent GUID `c471cec5ceec4d498fe462fcc0ea70a3` (verified unique in repo before write). |
| `Assets/Editor/GravenspireM2NamedBlockerVerificationRunner.cs` | M2-04 runner refactored to delegate the SearchInit suppression to the shared helper (behavior-preserving). |
| `Assets/Editor/GravenspireM2CombatBridgeVerificationRunner.cs` | M2-01 runner backported to consume the shared filter (was previously unfiltered against the diagnosed editor noise). |
| `Assets/Editor/GravenspireM2LinkedTrashOverpullVerificationRunner.cs` | M2-03 runner backported to consume the shared filter. |
| `Assets/Editor/GravenspireM2SingleTrashLoopVerificationRunner.cs` | M2-02 runner backported to consume the shared filter. |
| `Assets/Editor/GravenspireLaunchVerificationRunner.cs` | Launch verification runner backported to consume the shared filter. |
| `tests/evidence/S2-M3-00/verification.md` | This acceptance-criteria and gate summary. |
| `tests/evidence/S2-M3-00/m2-01-rerun-20260517-smoke.md` | Closure rerun evidence: M2-01 CombatBridge runner PASS, exit code 0. |
| `tests/evidence/S2-M3-00/m2-02-rerun-20260517-smoke.md` | Closure rerun evidence: M2-02 SingleTrashLoop runner PASS, exit code 0. |
| `tests/evidence/S2-M3-00/m2-03-rerun-20260517-smoke.md` | Closure rerun evidence: M2-03 LinkedTrashOverpull runner PASS, exit code 0. |
| `tests/evidence/S2-M3-00/m2-04-rerun-20260517-smoke.md` | Closure rerun evidence: M2-04 NamedBlocker runner PASS, exit code 0. |

## Acceptance Criteria Trace

| AC | Status | Evidence |
| --- | --- | --- |
| `S2-M3-00-01` | PASS | No fourth parallel scenario-smoke block was added to the M2 controller. `Assets/Scripts/M2SingleTrashMedLoopController.cs` is unchanged in this story (no diff against `dc0f306`). All cleanup work landed in editor runners and a new editor-only helper, not in the M2 controller. The story-required negative property (`production/stories/s2-m3-00-scenario-smoke-handoff-cleanup.md:63`) holds. |
| `S2-M3-00-02` | PASS | Shared scenario-smoke runner helper exists at `Assets/Editor/GravenspireScenarioSmokeRunnerHelpers.cs:14` (class) and `:37` (`IsEditorStartupNoise`). All five existing scenario-smoke runners now consume it: `Assets/Editor/GravenspireLaunchVerificationRunner.cs:183`, `Assets/Editor/GravenspireM2CombatBridgeVerificationRunner.cs:161`, `Assets/Editor/GravenspireM2LinkedTrashOverpullVerificationRunner.cs:164`, `Assets/Editor/GravenspireM2NamedBlockerVerificationRunner.cs:169`, `Assets/Editor/GravenspireM2SingleTrashLoopVerificationRunner.cs:150`. The class is documented as a growth point (`Assets/Editor/GravenspireScenarioSmokeRunnerHelpers.cs:9` through `:13` and `:30` through `:34`). |
| `S2-M3-00-03` | PASS | Behavior-preserving by construction and empirically verified at closure: (a) the M2-04 named-blocker runner originally suppressed the SearchInit stack inside its Error/Exception/Assert branch only; the helper preserves the same gate (`Assets/Editor/GravenspireScenarioSmokeRunnerHelpers.cs:39` through `:47`) so M2-04 runner behavior is bit-for-bit identical against any non-SearchInit log. (b) The four sibling runners (M2-01/02/03 + Launch) gain only the SearchInit suppression, which is the same diagnosed editor-startup noise condition recorded as a known false-fail in `production/sprint-status.yaml:44`; any pre-change PASS remains a post-change PASS because the change only removes one false-fail axis. (c) Combat Core regression: 175/175 passed (see Local Gates below) — no formula or state change. (d) Unity batchmode reruns of the four non-Launch affected runners passed at closure: `m2-01-rerun-20260517-smoke.md`, `m2-02-rerun-20260517-smoke.md`, `m2-03-rerun-20260517-smoke.md`, and `m2-04-rerun-20260517-smoke.md` all show `**Result:** PASS`, exit code 0, and no captured runner errors. The fifth modified runner (Launch) remains exempt from batchmode rerun and is covered by the logical proof alone — see **Closure Evidence** below for details. |
| `S2-M3-00-04` | PASS | The SearchInit editor-startup noise filter is shared and explicitly available to M3 runner log capture via `GravenspireScenarioSmokeRunnerHelpers.IsEditorStartupNoise(condition, stackTrace, type)` (`Assets/Editor/GravenspireScenarioSmokeRunnerHelpers.cs:37`). Any future M3 runner that subscribes to `Application.logMessageReceived` can call this helper at the top of its `CaptureLog` body and `return` when it returns true, matching the pattern established at `Assets/Editor/GravenspireM2NamedBlockerVerificationRunner.cs:167` through `:170`. |
| `S2-M3-00-05` | PASS | No gameplay behavior changes: zero diff in `Assets/Scripts/**` (Combat Core, M2 controller, runtime bridge, bootstrap). All edits are in `Assets/Editor/**` (editor-time scenario-smoke verification runners + a new editor-only helper). The change adds suppression of one diagnosed, named editor-startup noise condition; it does not alter scenario logic, scoring, evidence schema, or scene generation. Combat regression at 175/175 confirms no formula/state regression. |
| `S2-M3-00-06` | PASS | Local gates passed on 2026-05-17 (see Local Gates table below): dotnet Combat Core 175/175 PASS, four Unity batchmode reruns PASS, `git diff --check` PASS, T1 negative-scope scan PASS (with classified pre-existing hits), and `.githooks/pre-commit` PASS against the staged closure index. |

## Manifest Absence

`docs/architecture/control-manifest.md` is absent in this checkout, consistent with the pre-existing project-wide governance gap recorded at `production/sprint-status.yaml:42` (`control_manifest_absence_pre_existing`). The active Sprint 2 QA plan uses architecture-registry fallback when the manifest is absent (`production/qa/plans/qa-plan-sprint-2-20260509.md:54`, `:60`). This verification is recorded against pre-change HEAD `dc0f306` and the story manifest reference `Sprint 2, 2026-05-14`.

## Local Gates

| Gate | Result | Evidence |
| --- | --- | --- |
| Combat regression | PASS | `dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` reported `Passed: 175, Failed: 0, Skipped: 0, Total: 175, Duration: 552 ms` on 2026-05-17. The S2-M3-00 diff touches only editor-only `Assets/Editor/**` files; Combat Core (`Assets/Scripts/**`, `tests/**`) is untouched. Test count grew from S2-M2-03's reported 172/172 (`tests/evidence/S2-M2-03/verification.md:42`) to 175/175 due to coverage added in later M2 stories; all 175 pass under this story's changes. Editor-only code in this story's diff surface is not exercised by this headless suite; that coverage is provided by the closure batchmode reruns documented under **Closure Evidence** below. |
| T1 negative-scope scan | PASS WITH CLASSIFIED PRE-EXISTING HITS | `Grep` over the six changed files for `FishNet\|\bnetworking\b\|server authority\|server-authority\|\bPvP\b\|\baccounts?\b\|cloud saves?\|OpenAI\|Anthropic\|live LLM\|multiplayer\|\bWarrior\b\|\bEnchanter\b\|Time\.deltaTime\|DateTime\.Now\|DateTime\.UtcNow` returned only pre-existing `DateTime.UtcNow` calls in editor-time evidence timestamping (e.g., `Assets/Editor/GravenspireM2NamedBlockerVerificationRunner.cs:102`, `:112`, `:211`, `:267`, and identical patterns in the four sibling runners). These calls are unchanged by this story (each appears in the unaltered portions of each runner). The new helper file `Assets/Editor/GravenspireScenarioSmokeRunnerHelpers.cs` has no matches. No T1 deny term was introduced by this story. |
| Diff hygiene | PASS | `git diff --check` returned no whitespace or conflict-marker findings against `dc0f306` on 2026-05-17. |
| Unity batchmode reruns | PASS | Four closure reruns used Unity `6000.3.14f1` without `-quit` (the runners own `EditorApplication.Exit(exitCode)`) and wrote S2-M3-00 smoke evidence: `m2-01-rerun-20260517-smoke.md` PASS, `m2-02-rerun-20260517-smoke.md` PASS, `m2-03-rerun-20260517-smoke.md` PASS, `m2-04-rerun-20260517-smoke.md` PASS. Each runner exited code 0 and recorded `- None captured during runner execution.` under Errors. The earlier `-quit` attempt exited 0 before evidence was written and is intentionally not counted as a closure pass. |
| `.githooks/pre-commit` | PASS | `.githooks/pre-commit` returned `[pre-commit] OK` against the staged closure index on 2026-05-17 after closure documentation edits were complete. |

## Refactor Verification Note

Two independent statements justify AC-03 / AC-05 behavior preservation:

1. **M2-04 runner (named-blocker) — bit-equivalent refactor.** The helper's logic body (`Assets/Editor/GravenspireScenarioSmokeRunnerHelpers.cs:39` through `:47`) implements exactly the same two-step gate as the original inline block: (a) type must be `LogType.Error`, `LogType.Exception`, or `LogType.Assert`; (b) `stackTrace` must be non-empty and `Contains("UnityEditor.Search.SearchInit")`. The refactored caller (`Assets/Editor/GravenspireM2NamedBlockerVerificationRunner.cs:167` through `:182`) calls the helper at the top of `CaptureLog` and returns when true; the original caller wrapped the same logic inside the Error/Exception/Assert branch. Because the helper itself gates on type, calling it first and returning is equivalent to nesting it inside the original gate. Warning-level SearchInit logs (helper returns false for `LogType.Warning`) flow through to `AppendSessionLine(WarningsKey, …)` exactly as before; the M2-04 original inline block was also scoped to `LogType.Error / Exception / Assert` only, so Warning-path behavior was identical before this story and remains so after. Edge case: if a SearchInit-tagged condition were ever logged with a null or empty `stackTrace`, the helper would return `false` (the stack-trace check at `:44` requires a non-empty string) and the condition would be captured to `ErrorsKey` as before — this is conservative (no false-pass risk) and matches pre-change behavior. There is no other behavior change to that runner.

2. **M2-01 / M2-02 / M2-03 / Launch runners — additive suppression of one named condition.** These four runners previously appended every `LogType.Error / Exception / Assert` to `ErrorsKey`, including the diagnosed `UnityEditor.Search.SearchInit` editor-startup noise. After this story, they suppress that single named condition only. Any prior run that did not surface SearchInit produces an identical evidence record. Any prior run that surfaced SearchInit and consequently false-failed will now correctly pass — matching the carryover at `production/sprint-status.yaml:44`. No other log entries are suppressed. No gameplay code, controller code, scene generation, evidence writer, or runner orchestration is changed.

## Closure Evidence

The following items moved from implementation-gate deferral to closure evidence during `/story-done`:

1. **Unity batchmode rerun of four affected runners** (M2-01 CombatBridge, M2-02 SingleTrashLoop, M2-03 LinkedTrashOverpull, M2-04 NamedBlocker). All four runners exited code 0 and wrote S2-M3-00 evidence files with `**Result:** PASS`. AC-03's named preservation set (S2-M2-02, S2-M2-03, S2-M2-04) maps to the M2-02, M2-03, and M2-04 runners in this list; M2-01 is included so every modified scenario-smoke runner accepting `-gravenspireEvidencePath` is empirically reverified.

2. **`.githooks/pre-commit` execution against the staged closure index** (AC-06). At implementation gate, the hook's pattern-scan scope was analyzed and the working-tree equivalent of its whitespace gate (`git diff --check`) ran clean. At closure, `.githooks/pre-commit` returned `[pre-commit] OK` against the staged closure index after closure documentation edits were complete.

3. **Implementation HEAD lock**. The final commit SHA cannot exist until Brian approves the commit. This file therefore records the verified working-tree closure state as `PENDING COMMIT`; after the approved commit, the final response records the closure SHA. If a future audit requires the SHA inside this file, use a follow-up metadata-only evidence patch rather than pretending a pre-commit file can self-reference its own commit hash.

### Launch runner exemption

The fifth modified runner — `Assets/Editor/GravenspireLaunchVerificationRunner.cs` — is exempt from the closure batchmode rerun. Its evidence-output path is hardcoded at `Assets/Editor/GravenspireLaunchVerificationRunner.cs:18` to `tests/evidence/S2-FOUNDATION-01/unity-cli-launch-verification-20260510.md` and the runner has no `-gravenspireEvidencePath` argument-parsing implementation (compare to the four M2 runners — e.g., `Assets/Editor/GravenspireM2CombatBridgeVerificationRunner.cs:30` and `:251`). Running it in batchmode under S2-M3-00 would silently overwrite the closed S2-FOUNDATION-01 evidence rather than produce an S2-M3-00 artifact. The Launch backport is covered by the logical proof in the Refactor Verification Note (one-way additive suppression of a single named editor-startup noise condition; any pre-change PASS remains a post-change PASS). Harmonizing Launch's evidence-path argument parsing with the four M2 runners is tracked as a separate carryover (`launch_runner_evidence_path_hardcoded`, see Carryover Status below); it is out of scope for S2-M3-00's behavior-preserving cleanup constraint at `production/stories/s2-m3-00-scenario-smoke-handoff-cleanup.md:50` through `:57`.

### Closure rerun protocol

```
Unity -batchmode -nographics -projectPath <repo> -executeMethod Gravenspire.Editor.GravenspireM2CombatBridgeVerificationRunner.Run -gravenspireEvidencePath tests/evidence/S2-M3-00/m2-01-rerun-<date>-smoke.md
Unity -batchmode -nographics -projectPath <repo> -executeMethod Gravenspire.Editor.GravenspireM2SingleTrashLoopVerificationRunner.Run -gravenspireEvidencePath tests/evidence/S2-M3-00/m2-02-rerun-<date>-smoke.md
Unity -batchmode -nographics -projectPath <repo> -executeMethod Gravenspire.Editor.GravenspireM2LinkedTrashOverpullVerificationRunner.Run -gravenspireEvidencePath tests/evidence/S2-M3-00/m2-03-rerun-<date>-smoke.md
Unity -batchmode -nographics -projectPath <repo> -executeMethod Gravenspire.Editor.GravenspireM2NamedBlockerVerificationRunner.Run -gravenspireEvidencePath tests/evidence/S2-M3-00/m2-04-rerun-<date>-smoke.md
```

### Closure pass condition

Each of the four runners above must exit with code 0 and write a smoke evidence file under `tests/evidence/S2-M3-00/` showing the runner's existing pass criteria. The "no new errors" condition is evaluated as follows:

- **M2-04 NamedBlocker**: compare against the prior green smoke at `tests/evidence/S2-M2-04/unity-named-blocker-runner-20260514-smoke.md`. No new errors may appear that were not present in that record.
- **M2-01 CombatBridge, M2-02 SingleTrashLoop, M2-03 LinkedTrashOverpull**: no prior filtered smoke exists (the SearchInit filter is new for these three). The pass condition is "exit code 0 and no non-SearchInit errors recorded in the runner's smoke output."

Closure may accept a manual Play Mode equivalent for any individual runner if batchmode produces no Test Runner results XML, per the pre-existing carryover at `production/sprint-status.yaml:32`.

## Carryover Status

- `m2_controller_scenario_smoke_abstraction` (`production/sprint-status.yaml:43`): PARTIALLY ADDRESSED. The story does not extract the three large parallel scenario subsystems from `M2SingleTrashMedLoopController.cs` (that would risk gameplay behavior change and exceeds the story's Out Of Scope `production/stories/s2-m3-00-scenario-smoke-handoff-cleanup.md:50` through `:57`). It establishes the shared helper class as the first piece of cross-runner abstraction and prevents M3 from adding a fourth parallel block. Further controller-side extraction remains a follow-up if M3 telemetry hooks ever require touching the controller; the story's intent is to avoid that requirement entirely by composing M3 telemetry in a separate M3 runner rather than mutating the M2 controller.
- `m2_runner_editor_noise_capture` (`production/sprint-status.yaml:44`): RESOLVED. SearchInit filter is now shared via `GravenspireScenarioSmokeRunnerHelpers.IsEditorStartupNoise` and consumed by all five existing runners. Future runners can opt in with a one-line call.
- `launch_runner_evidence_path_hardcoded` (NEW, discovered during /code-review convergent verification on 2026-05-17): `Assets/Editor/GravenspireLaunchVerificationRunner.cs:18` hardcodes its evidence output to `tests/evidence/S2-FOUNDATION-01/unity-cli-launch-verification-20260510.md` and ignores `-gravenspireEvidencePath`. The four M2 runners accept the argument (e.g., `Assets/Editor/GravenspireM2CombatBridgeVerificationRunner.cs:30` and `:251` — `EvidencePathArgumentName` constant plus `ResolveEvidencePathFromCommandLine` method). A follow-up story should harmonize Launch's evidence-path argument handling by adding `EvidencePathArgumentName`, `EvidencePathKey`, `DefaultEvidencePath`, `ResolveEvidencePathFromCommandLine`, and `CurrentEvidencePath` to Launch, matching the M2 pattern (~12 LOC). This is out of scope for S2-M3-00 per the story's behavior-preserving constraint at `production/stories/s2-m3-00-scenario-smoke-handoff-cleanup.md:50` through `:57`. Until harmonized, Launch is exempt from any closure batchmode-rerun protocol that depends on `-gravenspireEvidencePath`; see Closure Evidence above.
