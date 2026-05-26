# S3-05 Verification - Navigable Greybox First District

**Story:** `production/stories/s3-05-navigable-greybox-first-district.md`
**Result:** DONE WITH NOTES
**Date:** 2026-05-26
**Implementation branch:** `codex/s3-05-navigable-greybox-first-district`
**Implementation commits:** `bf3705d` through `31ff817`, plus this Phase 8 closure commit

## Evidence Index

| Artifact | Purpose | Result |
|---|---|---|
| `Assets/Editor/GravenspireS3FirstDistrictGreyboxBuilder.cs` | Phase 1 district authoring and Phase 1 verification entrypoint. | PASS |
| `Assets/Scenes/_DevEntry.unity` | Authored greybox district scene: `FirstDistrict_Greybox` root, M3 anchors, M2 camp, player spawn, NavMesh object. | PASS |
| `tests/evidence/S3-05/reachability-20260526-smoke.md` | S3-05-T2 NavMesh reachability runner output. | PASS |
| `tests/evidence/S3-05/soft-lock-scan-20260526-smoke.md` | S3-05-T3 best-effort high-confidence soft-lock scan output. | PASS |
| `tests/evidence/S3-05/pillar-2-wayfinding-review-20260526.md` | S3-05-T4 design-aware wayfinding review checklist. | PASS WITH CONCERNS |
| `tests/evidence/S3-05/spawn-to-caretaker-discoverability-20260526.png` | S3-05-T4/AC-09 marker-free spawn-view screenshot. | CAPTURED |
| `tests/evidence/S3-05/walkthrough-log-20260526.md` | S3-05-T5 advisory walkthrough artifact. | ADVISORY-DEFERRED |
| `tests/evidence/S3-05/greybox-presentation-scan-20260526.txt` | S3-05-T6 greybox-only source/asset scan. | PASS |
| `tests/evidence/S3-05/unity-end-to-end-in-district-20260526-smoke.md` | S3-05-T7 AC-12 composite smoke with graceful-degradation envelope. | PASS_WITH_NOTES |
| `tests/evidence/S3-05/m2-02-preservation-20260526-smoke.md` | M2 single-trash med-loop preservation smoke, separate batchmode invocation. | PASS |
| `tests/evidence/S3-05/m2-03-preservation-20260526-smoke.md` | M2 linked-trash overpull preservation smoke, separate batchmode invocation. | PASS |
| `tests/evidence/S3-05/m2-04-preservation-20260526-smoke.md` | M2 named-blocker camp-boundary preservation smoke, separate batchmode invocation. | PASS |

## Acceptance Criteria Trace

| AC | Verification method | Evidence | Status |
|---|---|---|---|
| S3-05-01 | Phase 1 builder replaced the old shell with the `FirstDistrict_Greybox` hierarchy and the Phase 1 verifier asserted required greybox objects. | `Assets/Editor/GravenspireS3FirstDistrictGreyboxBuilder.cs:21`, `:23`, `:56-61`; `Assets/Scenes/_DevEntry.unity:715` | PASS |
| S3-05-02 | Existing M3 anchor GameObjects remain present after scene authoring and are reachable on the baked NavMesh. | `Assets/Scenes/_DevEntry.unity:758`, `:1384`, `:2058`; `tests/evidence/S3-05/reachability-20260526-smoke.md:28-38` | PASS |
| S3-05-03 | M2 preservation reruns passed against the authored district, with each smoke run in a separate Unity invocation. | `tests/evidence/S3-05/m2-02-preservation-20260526-smoke.md:7`, `tests/evidence/S3-05/m2-03-preservation-20260526-smoke.md:7`, `tests/evidence/S3-05/m2-04-preservation-20260526-smoke.md:7` | PASS |
| S3-05-04 | Player spawn is deliberate and stable at `ClericShellMarker`; spawn-view capture and Phase 1 verification use it as the district vantage. | `Assets/Scenes/_DevEntry.unity:1043`; `tests/evidence/S3-05/pillar-2-wayfinding-review-20260526.md:19`, `:93-101` | PASS |
| S3-05-05 | Baked NavMesh covers all M3 anchor paths and documents the agent profile. | `tests/evidence/S3-05/reachability-20260526-smoke.md:10-18`, `:40-46` | PASS |
| S3-05-06 | Reachability runner proves spawn-to-M3 paths are complete and records path length/elevation values. | `Assets/Editor/GravenspireS3DistrictReachabilityVerificationRunner.cs`; `tests/evidence/S3-05/reachability-20260526-smoke.md:28-46` | PASS |
| S3-05-07 | Soft-lock scan grid-samples the district at 1 m spacing and detects zero trapped samples, with non-exhaustive framing preserved. | `Assets/Editor/GravenspireS3DistrictSoftLockScanVerificationRunner.cs`; `tests/evidence/S3-05/soft-lock-scan-20260526-smoke.md:10-19`, `:31-36`, `:49` | PASS, best-effort high-confidence |
| S3-05-08 | Pillar-2 wayfinding review applies the four reject criteria, explicit absence scan, and allowed-pattern checklist. | `tests/evidence/S3-05/pillar-2-wayfinding-review-20260526.md:28-66`, `:88` | PASS WITH CONCERNS |
| S3-05-09 | Spawn-to-Caretaker discoverability is verified by builder sightline evidence plus marker-free screenshot; second-landmark readability is documented as a concern. | `tests/evidence/S3-05/spawn-to-caretaker-discoverability-20260526.png`; `tests/evidence/S3-05/pillar-2-wayfinding-review-20260526.md:68-73`, `:90` | PASS WITH CONCERNS |
| S3-05-10 | Greybox-only presentation scan reports no Light or AudioSource components, only approved primitive meshes and five approved greybox materials. | `tests/evidence/S3-05/greybox-presentation-scan-20260526.txt:7`, `:15-26`, `:28-33` | PASS |
| S3-05-11 | M2 combat camp preservation smokes all pass after district authoring, each in its own batchmode invocation. | `tests/evidence/S3-05/m2-02-preservation-20260526-smoke.md:7`, `tests/evidence/S3-05/m2-03-preservation-20260526-smoke.md:7`, `tests/evidence/S3-05/m2-04-preservation-20260526-smoke.md:7` | PASS |
| S3-05-12 | Composite smoke asserts the closed S3-01 harness dispatch portion in district context and records downstream S3-02/03/04 adapter portions as deferred to S3-06. | `tests/evidence/S3-05/unity-end-to-end-in-district-20260526-smoke.md:7`, `:18-49`, `:55-60` | PASS_WITH_NOTES; full-chain assertion rolls forward |

## AC-11 Reconciliation

The S3-05 story contains an internal stale label around AC-11. The authoritative AC list defines S3-05-11 as M2 combat camp preservation at `production/stories/s3-05-navigable-greybox-first-district.md:101`. Later, the manual evidence section labels the advisory walkthrough as "AC-11" at `production/stories/s3-05-navigable-greybox-first-district.md:171`.

This verification treats line 101 as authoritative. The walkthrough log is S3-05-T5 advisory evidence that complements AC-07's soft-lock methodology gaps; it has no primary acceptance criterion of its own. Earlier Phase 3 and Phase 4 forward references to an "AC-11 walkthrough" inherited the stale story label and should be read as references to the advisory T5 walkthrough artifact.

## AC-12 Partial-Pass Closure

S3-05 closes DONE WITH NOTES because the AC-12 composite smoke could only assert the portion of the chain that existed at S3-05 implementation time. At closure, S3-02 was ready but not closed, and S3-03/S3-04 were blocked behind the dependency chain. The Phase 6 runner therefore asserted the S3-01 harness dispatch path inside the district and did not fake downstream adapter behavior.

The full S3-02/03/04 telemetry vocabulary remains the canonical S3-06 responsibility. `production/sprint-status.yaml` carries `s3_05_ac12_partial_rollforward_to_s3_06`, and `production/stories/s3-06-playable-end-to-end-and-human-play.md:71`, `:108`, and `:234` already name S3-06 as the resolution point.

## S3-01 M2 Preservation Retrospective

Phase 7 exposed that the existing M2 preservation runners called `GravenspireM2SingleTrashLoopBuilder.Build()` before verification. That builder mutates and saves `_DevEntry.unity`, including M2 baseline reconstruction. As a result, S3-01's M2 preservation evidence proved that the M2 baseline still worked after the baseline builder restored the scene, not that M2 preserved behavior in a harness-present authored-scene context.

This does not rewrite S3-01 history. It records the discovered limitation and the S3-05 correction: the M2 runners now accept `-gravenspireSkipBuilder`, defaulting to the old behavior when absent and skipping only the builder call when the flag is present. Future authored-district preservation checks should use the flag so scene-authoring stories test the scene they actually authored.

## M2 Evidence Header Note

The three Phase 7 preservation files intentionally retain their S2-M2-0X titles and story references because the existing M2 runners author their own headers and S3-05 only redirects the evidence path with `-gravenspireEvidencePath`. This matches the S3-01 precedent and is not a wrong-story authorship signal; the file location under `tests/evidence/S3-05/` and this verification document identify the S3-05 preservation use.

## Pattern Notes

- **Reachability runner:** `GravenspireS3DistrictReachabilityVerificationRunner` is the district/zone precedent for proving spawn-to-anchor NavMesh paths and recording path length, sample distance, and elevation deltas.
- **Soft-lock scan:** `GravenspireS3DistrictSoftLockScanVerificationRunner` is the precedent for best-effort high-confidence grid sampling. Claims must remain "zero soft-lock zones detected at sampled density," not "no soft-locks exist."
- **Pillar-2 review checklist:** `pillar-2-wayfinding-review-20260526.md` separates factual scene observations from design-aware verdicts and preserves PASS WITH CONCERNS when visual readability is real but not crisp.
- **Walkthrough log:** `walkthrough-log-20260526.md` exists as the advisory T5 artifact but honestly defers human qualitative play because no interactive human walkthrough was conducted in Phase 8.
- **M2 skip-builder pattern:** `-gravenspireSkipBuilder` on the M2 runners preserves backward compatibility while allowing future authored-scene stories to rerun M2 smokes without clobbering their scene state.
- **NavMesh agent profile:** Until a later story changes it, the T1 First District profile is `agent_radius=0.5`, `agent_height=2`, `agent_slope=45`, and `agent_climb=0.75`, documented in the reachability and soft-lock evidence.

## Local Closure Gates

| Gate | Result | Evidence |
|---|---|---|
| Unity reachability smoke | PASS | `tests/evidence/S3-05/reachability-20260526-smoke.md:7` |
| Unity soft-lock scan | PASS | `tests/evidence/S3-05/soft-lock-scan-20260526-smoke.md:7` |
| Pillar-2 wayfinding review | PASS WITH CONCERNS | `tests/evidence/S3-05/pillar-2-wayfinding-review-20260526.md:8`, `:88-90` |
| Greybox-only scan | PASS | `tests/evidence/S3-05/greybox-presentation-scan-20260526.txt:7` |
| AC-12 composite smoke | PASS_WITH_NOTES | `tests/evidence/S3-05/unity-end-to-end-in-district-20260526-smoke.md:7` |
| M2 preservation reruns | PASS | `tests/evidence/S3-05/m2-02-preservation-20260526-smoke.md:7`, `tests/evidence/S3-05/m2-03-preservation-20260526-smoke.md:7`, `tests/evidence/S3-05/m2-04-preservation-20260526-smoke.md:7` |
| Combat regression | PASS | `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` returned 189/189 PASS during Phase 8 closure verification. |
| Format gate projects | PASS | `dotnet format tests\Gravenspire.Combat.Tests.csproj --verify-no-changes --exclude-diagnostics IDE1006` and `dotnet format prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj --verify-no-changes --exclude-diagnostics IDE1006` both returned exit 0. |
| `.githooks/pre-commit` | PASS | Running with Git-for-Windows Bash (`C:\Program Files\Git\bin\bash.exe .githooks/pre-commit`) returned `[pre-commit] OK`. |
| Hygiene | PASS | `git diff --check` and trailing-whitespace scan over the five Phase 8 files returned clean before staging. |
| Sprint status structure | PASS | Node-based structure check verified the S3 story IDs remained in order, S3-02 stayed `ready`, S3-03/S3-04 stayed `blocked`, and S3-05 changed to `done with notes` with the AC-12 carryover key present. |
| Scene/settings preservation | PASS | Phase 7 post-rerun checks found no `_DevEntry.unity`, `ProjectSettings`, or `Packages` drift. |

## Known Notes

- AC-08 and AC-09 are PASS WITH CONCERNS, not unqualified PASS. The marker-free screenshot is dark in gamma-mode rendering and the second landmark is less visually distinct than the builder sightline check proves mechanically.
- The Phase 4 screenshot perspective limitation is retained: the first capture read like an oblique scout view and was replaced with a 70-degree player-perspective capture before the review verdict.
- Unity batchmode ProjectSettings drift was observed during Phase 4 and restored from `HEAD`; it did not ship as S3-05 scope.
- The soft-lock scan is non-exhaustive by design. Human qualitative walkthrough remains advisory-deferred until an interactive play session occurs.
