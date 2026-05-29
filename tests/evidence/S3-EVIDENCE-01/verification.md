# S3-EVIDENCE-01 — Verification Summary

**Story:** [s3-evidence-integrity-patch.md](../../../production/stories/s3-evidence-integrity-patch.md)
**Worktree:** `N:/GravenSpire-codex` (D006 Codex implementer lane)
**Evidence run date:** 2026-05-28 / 2026-05-29 UTC; companion filenames preserve the `20260528` brief stamp.
**Verdict:** PASS WITH NOTES (4/4 ACs PASS; one PR-readiness gate is BLOCKED by tooling scope — see Code Style Gate).

## Acceptance Criteria → Evidence

| AC | Description | Evidence | Status |
|---|---|---|---|
| S3-EVIDENCE-01-01 | Faction negative check derived from structural absence, not unconditional `true` | `unity-m3-end-to-end-faction-negative-20260528-smoke.md:9` (Result PASS), `:40`–`:43` (structural-absence checks PASS), `:46` (`no_faction_consequence_applied` PASS) | PASS |
| S3-EVIDENCE-01-01 (negative control) | Runner-local fake faction-consequence probe trips the check to FAIL | `unity-m3-end-to-end-faction-negative-control-20260528-smoke.md:10` (Result FAIL, expected), `:47` (`no_faction_consequence_applied` FAIL), `:48` (`runner_local_fake_faction_consequence_negative_control_tripped` PASS) | PASS |
| S3-EVIDENCE-01-02 | M2 preservation evidence self-describing; preservation mode requires explicit `-gravenspireSkipBuilder` | `m2-02-preservation-skip-builder-20260528-smoke.md:7,:15,:22`–`:24`; `m2-03-…:7,:15,:22`–`:24`; `m2-04-…:7,:15,:22`–`:24` (each: Result PASS, `builder_skipped=true`, negative control FAIL without flag, normal control `builder_skipped=false`) | PASS |
| S3-EVIDENCE-01-03 | Bake-scope footprint coverage beyond center-only; per-obstacle + aggregate fields | `navmesh-bake-scope-footprint-20260528-smoke.md:7` (Result PASS), `:13` (`negative_control_mode=false`), `:15` (`expected_result=PASS`), `:133` (`all_obstacle_footprints_fully_carved` PASS) | PASS |
| S3-EVIDENCE-01-03 (negative control) | Flat-floor uncarved fixture must be flagged FAILING | `navmesh-bake-scope-flat-floor-negative-control-20260528-smoke.md:7` (Result FAIL, expected), `:13` (`negative_control_mode=true`), `:15` (`expected_result=FAIL`), `:146` (`all_obstacle_footprints_fully_carved` FAIL) | PASS |
| S3-EVIDENCE-01-04 | Reachability + soft-lock evidence emit `precondition_artifact_required: navmesh-bake-scope`, naming the artifact path; visibility stub only | `reachability-precondition-stub-20260528-smoke.md:11` and `soft-lock-precondition-stub-20260528-smoke.md:11` (`precondition_artifact_required: navmesh-bake-scope`); both `:12` name the bake-scope artifact path; both `enforcement: visibility-only` | PASS |

## Negative Controls (disease each guard detects)

| Control | File | Expected | Observed |
|---|---|---|---|
| Fake faction-consequence event injected | `unity-m3-end-to-end-faction-negative-control-20260528-smoke.md` | FAIL | FAIL (exit 1) — proves structural-absence check is not a constant |
| Flat-floor uncarved NavMesh fixture | `navmesh-bake-scope-flat-floor-negative-control-20260528-smoke.md` | FAIL | FAIL (exit 1) — proves footprint check detects flat-bake disease |
| Preservation mode without `-gravenspireSkipBuilder` | `m2-0{2,3,4}-preservation-skip-builder-20260528-smoke.md:22` | FAIL | FAIL — `preservation_mode_requires_skip_builder` trips |

## Closure Gates

| Gate | Command | Result | Notes |
|---|---|---|---|
| Combat regression baseline | `dotnet test tests/Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` | PASS 189/189 | Run 2026-05-29 in `N:/GravenSpire-codex` |
| Whitespace/diff hygiene | `git diff --check` | PASS (clean) | No whitespace errors |
| Scene discipline | `git status --porcelain` filtered for `*.unity`/`*.prefab` | PASS — `NO_SCENE_OR_PREFAB_CHANGES` | All four batchmode runs left scenes untouched; only the three runner `.cs` files modified |
| Code style gate | `dotnet format --verify-no-changes` | **BLOCKED** | See Code Style Gate below |

## Code Style Gate Evidence

- Tier: 1
- Command: `dotnet format --verify-no-changes`
- Run location: Local (`N:/GravenSpire-codex`)
- Result: **BLOCKED**
- Evidence / Notes:
  - The three changed files are Unity Editor scripts under `Assets/Editor/` (`GravenspireS3DistrictNavMeshBakeScopeVerificationRunner.cs`, `GravenspireS3DistrictReachabilityVerificationRunner.cs`, `GravenspireS3DistrictSoftLockScanVerificationRunner.cs`). They are compiled by the Unity-generated `Assembly-CSharp-Editor.csproj`, which is gitignored and not present in the tracked tree.
  - The only tracked `.csproj` files are `tests/Gravenspire.Combat.Tests.csproj` and `prototypes/combat-slice-T1/Harness/CombatSliceHarness.csproj`; neither references `Assets/Editor/`, so `dotnet format` has no tracked workspace covering the changed files (bare invocation errors: "Could not find a MSBuild project file or solution file").
  - Running `dotnet format` against the combat tests project surfaces only pre-existing `IDE1006` naming on `test_*` methods, which are intentional per `.claude/rules/test-standards.md` (`test_[system]_[scenario]_[expected_result]`); these are not introduced by this story.
  - Compensating evidence: all four Unity 6.3 LTS (`6000.3.14f1`) batchmode runs compiled and executed the changed scripts cleanly (exit 0 for the two PASS runs; expected exit 1 for the flat-floor negative control), confirming the changed C# compiles under the pinned engine.
- Classification: `STYLE_GATE — BLOCKED` (tooling scope, not a formatting failure). Same situation as prior Unity-Editor-only stories (e.g. S3-01, which closed without a runner-targeted format gate). **Surface to user for waiver or owner ruling before PR.**

## Unity API Verification (carried from AC-3)

- API / feature: `NavMeshBuilder.BuildNavMeshData`, `NavMeshBuildSource` (Box), `NavMesh.AddNavMeshData`, `NavMeshDataInstance` (flat-floor negative-control fixture only)
- Unity version: 6.3 LTS (`6000.3.14f1`)
- Reference files checked: `docs/engine-reference/unity/modules/navigation.md` (does not document `NavMeshBuilder.BuildNavMeshData`)
- Status: UNVERIFIED in engine-reference → verified EMPIRICALLY by the batchmode run recorded in `navmesh-bake-scope-flat-floor-negative-control-20260528-smoke.md:7` (Result FAIL, expected) and its embedded `## Unity API Verification` block. (Raw Unity `.log` files are intentionally NOT committed: they contain local/licensing identifiers — `LicenseClient-brian`, Machine Id, local paths — so they are local byproducts only.)
- Decision impact: fixture-only; never touches the committed `surface.navMeshData` asset. Added additively via `NavMesh.AddNavMeshData` and removed in `ClearState`.
- Recommendation: keep the Unity API Verification block in the negative-control evidence; consider documenting `NavMeshBuilder.BuildNavMeshData` in `navigation.md` if reused.

## Carryovers Honored

- `m2_02_runner_date_hardcoded` — all evidence dates derive from `DateTimeOffset.UtcNow`; no hardcoded dates were introduced in the modified runners.
- `m2_melee_rng_not_reset` — out of scope; this patch adds preservation builder-skip evidence only and does not reset M2 RNG cursors.
- `control_manifest_absence_pre_existing` — story Manifest Version remains `Unavailable` under the documented fallback; no control-manifest artifact introduced.

## Scope Discipline

- AC-1 added no real M5 faction-consequence infrastructure (structural absence + runner-local negative control only).
- AC-4 is a visibility-only stub: emits the precondition field, names the artifact path, and explicitly does **not** enforce artifact freshness, file hashes, scene hashes, or NavMesh asset identity (D003 — Tier 1 single-player offline; no Tier 2 composability framework).

## Open Items Before Merge

1. Code style gate is BLOCKED by tooling scope (Unity-Editor-only changes, no tracked csproj). Needs user/owner waiver or an approved approach (e.g. generate the Unity sln for a one-off format pass).
2. `.githooks/pre-commit` (`[pre-commit] OK`) and a T1 negative-scope scan over changed files remain to be run as part of `/story-done` / commit staging.
