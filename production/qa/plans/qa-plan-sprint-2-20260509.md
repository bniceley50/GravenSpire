# QA Plan - Sprint 2 First District Foundation

**Date:** 2026-05-09
**Invocation:** `/qa-plan sprint`
**Scope:** Sprint 2 First District routing: completed Combat Core hotfix plus Unity project shell foundation before new feature implementation.
**Input source:** `production/sprints/sprint-2.md` at commit `9450ddf`.
**Sprint status source:** `production/sprint-status.yaml` at commit `9450ddf`.
**Confidence:** High for current two-story QA scope; medium for exact Unity-generated file footprint until `S2-FOUNDATION-01` runs in the installed editor.

## QA Scope

Sprint 2 builds toward **Gravenspire T1: The First District**, a 20-30 minute offline slice with one Cleric, one cursed-city district, three enemy types, one named NPC, one faction presence, one objective, one loot table, one vendor or stash, one save/load flow, and one visible world-state change (`production/sprints/sprint-2.md:10` through `production/sprints/sprint-2.md:23`).

The immediate QA gate is narrow: `S2-FOUNDATION-01` must create a production Unity shell before launch/menu/session smoke can pass. New feature implementation waits for this QA plan, then starts with `/dev-story S2-FOUNDATION-01-unity-project-shell` (`production/sprints/sprint-2.md:35` through `production/sprints/sprint-2.md:39`; `production/sprint-status.yaml:15` through `production/sprint-status.yaml:20`).

| Story | Status | Classification | Automated Test Required | Manual / Document Verification Required |
| --- | --- | --- | --- | --- |
| `S2-COMBAT-01` Fix init-only property preservation | Complete | Hotfix / Logic | Existing combat suite regression only; do not reopen completed implementation | Verify existing evidence remains the source of truth |
| `S2-FOUNDATION-01` Unity project shell | Ready for dev after this QA plan | Foundation / Integration | Unity shell file/config checks, optional EditMode/batchmode smoke, T1 negative-scope scan | Verify launchable dev entry scene or documented Unity-environment blocker |

## Source List

Verification method: live repository reads with `Get-Content`, `rg`, `Select-String`, and git inspection on 2026-05-09.

| Source | Use |
| --- | --- |
| `production/sprints/sprint-2.md:10` through `production/sprints/sprint-2.md:23` | First District slice target. |
| `production/sprints/sprint-2.md:35` through `production/sprints/sprint-2.md:39` | Post-QA plan next gate and foundation scope guard. |
| `production/sprints/sprint-2.md:45` | M1 Player In World proof target. |
| `production/sprints/sprint-2.md:55` through `production/sprints/sprint-2.md:58` | Story ledger and current next gate. |
| `production/sprints/sprint-2.md:70` through `production/sprints/sprint-2.md:78` | Tier 2+ cuts preserved. |
| `production/sprints/sprint-2.md:80` through `production/sprints/sprint-2.md:87` | Known findings and carryovers. |
| `production/sprints/sprint-2.md:94` through `production/sprints/sprint-2.md:98` | Minimum shell shape. |
| `production/sprint-status.yaml:22` through `production/sprint-status.yaml:32` | Carryover dictionary feeding Sprint 2 QA. |
| `production/sprint-status.yaml:35` through `production/sprint-status.yaml:52` | Current Sprint 2 story list and ready-for-dev foundation row. |
| `production/stories/s2-combat-01-fix-init-only-property-preservation.md:1` through `production/stories/s2-combat-01-fix-init-only-property-preservation.md:12` | Completed hotfix metadata. |
| `production/stories/s2-combat-01-fix-init-only-property-preservation.md:35` through `production/stories/s2-combat-01-fix-init-only-property-preservation.md:43` | Completed hotfix acceptance criteria. |
| `tests/evidence/S2-COMBAT-01/verification.md:27` through `tests/evidence/S2-COMBAT-01/verification.md:34` | S2-COMBAT-01 implementation evidence. |
| `tests/evidence/S2-COMBAT-01/verification.md:38` through `tests/evidence/S2-COMBAT-01/verification.md:44` | S2-COMBAT-01 AC pass evidence. |
| `production/stories/s2-foundation-01-unity-project-shell.md:20` through `production/stories/s2-foundation-01-unity-project-shell.md:29` | Foundation source trace. |
| `production/stories/s2-foundation-01-unity-project-shell.md:35` through `production/stories/s2-foundation-01-unity-project-shell.md:45` | Planned implementation surface. |
| `production/stories/s2-foundation-01-unity-project-shell.md:47` through `production/stories/s2-foundation-01-unity-project-shell.md:62` | Foundation out-of-scope guardrails. |
| `production/stories/s2-foundation-01-unity-project-shell.md:66` through `production/stories/s2-foundation-01-unity-project-shell.md:73` | `S2-FND-01` through `S2-FND-06`. |
| `DECISIONS.md:12` through `DECISIONS.md:26` | D001 Unity 6.3 LTS + C# + URP lock. |
| `DECISIONS.md:48` through `DECISIONS.md:64` | D003 T1 single-player offline scope. |
| `DECISIONS.md:68` through `DECISIONS.md:82` | D004 T1 templated dialogue / no live LLM dependency. |
| `.claude/docs/technical-preferences.md:7` through `.claude/docs/technical-preferences.md:15` | Engine/language/rendering baseline. |
| `docs/engine-reference/unity/VERSION.md:5` through `docs/engine-reference/unity/VERSION.md:18` | Unity 6.3 LTS version and API-risk warning. |
| `tests/README.md:43` through `tests/README.md:63` | Unity EditMode/PlayMode command surfaces and current missing-shell note. |
| `tests/smoke/critical-paths.md:8` through `tests/smoke/critical-paths.md:21` | Setup and core-stability smoke gates. |
| `tests/smoke/critical-paths.md:39` through `tests/smoke/critical-paths.md:41` | T1 negative-scope smoke guard. |
| `production/qa/smoke-sprint-20260509.md:51` through `production/qa/smoke-sprint-20260509.md:63` | Prior launch/menu/session smoke warning caused by missing Unity shell. |
| `production/gate-checks/gate-check-2026-05-09-sprint-1-5-closeout.md:62` through `production/gate-checks/gate-check-2026-05-09-sprint-1-5-closeout.md:70` | Gate-check recommendation to add the Sprint 2 Unity shell foundation story. |
| `docs/registry/architecture.yaml:481` through `docs/registry/architecture.yaml:686` | Forbidden-pattern registry used when `docs/architecture/control-manifest.md` is absent. |

## Live-State Corrections

- `S2-COMBAT-01` is already complete and verified. Sprint 2 QA does not reopen it; it remains a regression input.
- `S2-FOUNDATION-01` exists and was intentionally blocked only by the missing QA plan. The accompanying routing sync moves it to `ready-for-dev`.
- `docs/architecture/control-manifest.md` is absent. Architecture forbidden-pattern QA uses `docs/registry/architecture.yaml` until a later story creates or supersedes a separate control manifest.
- Production Unity shell is absent. This is the accepted Sprint 1.5 close-out warning and the direct scope of `S2-FOUNDATION-01`.
- Carryovers into Sprint 2 QA remain: human death-moment playtest, QA-02-01 wording, `AbilityResolvedEvent.ManaSpent`-only semantics, evidence provenance conventions, Save/Load metadata drift, README template-facing drift, and game-concept engine wording drift.
- Save/Load metadata cleanup, README rewrite, and `design/gdd/game-concept.md` engine wording cleanup are explicitly out of scope for `S2-FOUNDATION-01`.

## Regression Gates

| Gate | Timing | Command / Method | Pass Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| RG-00 Current combat baseline | Before `/dev-story S2-FOUNDATION-01` | `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` | Existing combat suite passes; expected count from latest evidence is `164/164`, but record any legitimate discovery-count drift | `tests/evidence/S2-FOUNDATION-01/verification.md` |
| RG-01 Unity shell file/config check | During `S2-FOUNDATION-01` | Inspect `ProjectSettings/`, `Packages/manifest.json`, package lock/config files, and scene/dev-entry files | Required shell files exist; generated metadata is documented; no superseded engine choice is encoded | `tests/evidence/S2-FOUNDATION-01/verification.md` |
| RG-02 Unity launch / batchmode smoke | During `S2-FOUNDATION-01` | Attempt Unity batchmode/EditMode or launch smoke using the installed editor path from `tests/README.md` | Command passes, or blocker is recorded with exact command, editor path, log path, and environment evidence | `tests/evidence/S2-FOUNDATION-01/verification.md` |
| RG-03 T1 negative scope | Before story closure | `rg -n -i "FishNet|networking|server authority|PvP|account|cloud save|OpenAI|Anthropic|live LLM" ProjectSettings Packages Assets src tests production/stories/s2-foundation-01-unity-project-shell.md` scoped to changed/created shell files where possible | No T1-forbidden runtime implementation introduced; allowed-context documentation hits are classified | `tests/evidence/S2-FOUNDATION-01/verification.md` |
| RG-04 Hygiene and hook | Before commit | `git diff --check`; `bash .githooks/pre-commit` | Diff hygiene clean; hook reports `[pre-commit] OK` | `tests/evidence/S2-FOUNDATION-01/verification.md` |

## Story Test Plans

### S2-COMBAT-01 - Fix Init-Only Property Preservation

**Classification:** Hotfix / Logic
**Status:** Complete. No reopening.
**Story:** `production/stories/s2-combat-01-fix-init-only-property-preservation.md`
**Evidence:** `tests/evidence/S2-COMBAT-01/verification.md`

| Case ID | Scenario | Method | Pass / Fail Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| QA-S2-01-RG | Completed hotfix remains covered by regression suite | Run current combat suite before foundation work | Suite passes; if count changes from `164/164`, verification explains why | `tests/evidence/S2-FOUNDATION-01/verification.md` |
| QA-S2-01-EV | Existing S2-COMBAT-01 evidence remains source of truth | Read prior verification and story completion notes | S2-COMBAT-01 remains complete; no code, evidence, or story edits are made by Sprint 2 QA-plan routing | `tests/evidence/S2-COMBAT-01/verification.md` |

### S2-FOUNDATION-01 - Unity Project Shell

**Classification:** Foundation / Integration
**Status after this QA plan:** Ready for `/dev-story`
**Story:** `production/stories/s2-foundation-01-unity-project-shell.md`
**Evidence target:** `tests/evidence/S2-FOUNDATION-01/verification.md`

| Case ID | Scenario | Method | Pass / Fail Criteria | Evidence Path |
| --- | --- | --- | --- | --- |
| QA-FND-01 | Production Unity shell exists | File existence check for `ProjectSettings/` and `Packages/manifest.json`; inspect any generated project metadata included in the batch | Required shell files are committed; any additional Unity-generated files are justified as required for deterministic project load | `tests/evidence/S2-FOUNDATION-01/verification.md` |
| QA-FND-02 | Shell configuration aligns with D001 | Inspect `ProjectSettings/ProjectVersion.txt`, `ProjectSettings/ProjectSettings.asset`, `Packages/manifest.json`, URP package/asset references, and graphics settings where present | Unity 6.3 LTS / `6000.3.x`, C#, and URP are encoded; no Godot, BIRP, HDRP, or superseded engine baseline is encoded | `tests/evidence/S2-FOUNDATION-01/verification.md` |
| QA-FND-03 | Launchable dev entry exists | Inspect scene/build settings or equivalent temporary dev-entry path | One launchable dev scene or equivalent temporary entry path exists; it is shell-only and does not claim hub/faction/Save/Load/NPC/loot/vendor/objective gameplay | `tests/evidence/S2-FOUNDATION-01/verification.md` |
| QA-FND-04 | Unity smoke attempted | Attempt EditMode/batchmode or launch smoke using `C:\Program Files\Unity\Hub\Editor\6000.3.14f1\Editor\Unity.exe` where available | Smoke passes, or the verification records the exact command, log path, editor-path result, and reason it is blocked | `tests/evidence/S2-FOUNDATION-01/verification.md` |
| QA-FND-05 | T1 negative-scope scan passes | Static grep over changed/created shell files and relevant project config | No FishNet, networking placeholders, server authority, PvP, accounts, cloud saves, live LLM, extra classes, broad content import, hub/faction/Save/Load/NPC/loot/vendor/objective gameplay implementation | `tests/evidence/S2-FOUNDATION-01/verification.md` |
| QA-FND-06 | Verification artifact is complete | Review `tests/evidence/S2-FOUNDATION-01/verification.md` before `/story-done` | Artifact records source trace, changed-file footprint, commands, pass/fail output, negative-scope results, environment limitations, and any Unity-generated metadata rationale | `tests/evidence/S2-FOUNDATION-01/verification.md` |

## Cross-Story Required Checks

### First District Target Integrity

QA must preserve the Sprint 2 target: one Cleric, one cursed-city district, three enemy types, one named NPC, one faction presence, one objective, one loot table, one vendor or stash, one save/load flow, and one visible world-state change. `S2-FOUNDATION-01` may only create the shell required to reach that target; it must not implement the target content.

### T1 Offline Scope

D003 and the Sprint 2 tier cuts require no networking, FishNet, server authority, PvP, accounts, cloud saves, live LLM, extra classes, broad AI companions, huge world, second district, or deep economy during the foundation story.

### Unity Shell Evidence

The prior Sprint 1.5 smoke/gate-check accepted launch/menu/session NOT CHECKED warnings because the Unity project shell did not exist. `S2-FOUNDATION-01` is the first story that can convert those warnings into runnable shell evidence.

### Carryover Hygiene

The QA plan carries forward known documentation drift without fixing it opportunistically: Save/Load metadata drift, README template-facing drift, and game-concept engine wording drift remain outside `S2-FOUNDATION-01`.

## Smoke Test Scope

Critical paths to verify before Sprint 2 QA handoff for the foundation story:

1. Current combat baseline still passes under `dotnet test`.
2. Unity editor path is checked and recorded.
3. `ProjectSettings/` exists.
4. `Packages/manifest.json` exists.
5. Unity 6.3 LTS / URP shell configuration is present or exact blocker is recorded.
6. Temporary dev entry scene or equivalent launch path exists.
7. Unity EditMode/batchmode or launch smoke is attempted.
8. No T1-forbidden runtime scope is introduced.
9. Verification artifact exists before `/story-done`.

## Playtest Requirements

No player feel playtest is required for `S2-FOUNDATION-01`. This story proves project launchability and smoke-testability, not gameplay feel.

| Story | Playtest Goal | Minimum Sessions | Target Player Type | Required Evidence |
| --- | --- | ---:| --- | --- |
| `S2-FOUNDATION-01` | None; shell-only foundation | 0 | N/A | Verification artifact only |
| Sprint 2 later playable-loop stories | Confirm first 10 minutes of playable Gravenspire improve | TBD per story | Brian / designer | Future story-specific playtest notes |

## Needs Clarification Before /dev-story

| Story | Clarification Needed | Why It Matters |
| --- | --- | --- |
| `S2-FOUNDATION-01` | Exact Unity-generated metadata inclusion policy after first project open | Unity may generate files beyond `ProjectSettings/` and `Packages/manifest.json`; verification must distinguish required deterministic project metadata from local noise |
| `S2-FOUNDATION-01` | Exact dev-entry scene name/path | The story requires one launchable dev entry path but has not locked the scene name |
| `S2-FOUNDATION-01` | Whether Unity smoke should prioritize EditMode batchmode or simple project launch first | Local editor availability and new shell state may determine the most useful first smoke |

## Definition of Done - Sprint 2 QA

A Sprint 2 story is DONE only when all applicable items are true:

- [ ] Story file exists and cites the Sprint 2 source lines used by this QA plan.
- [ ] All story acceptance criteria are verified via automated test, static/document review, Unity smoke, or documented environment blocker.
- [ ] Logic stories include automated regression tests in the relevant `tests/unit/**` or `tests/integration/**` surface.
- [ ] Foundation/Unity-shell stories include project file/config verification and Unity smoke attempt evidence.
- [ ] Manual evidence documents exact commands, output summaries, and blocker text when environment-dependent smoke cannot pass.
- [ ] T1 negative-scope scan is recorded.
- [ ] `git diff --check` and `.githooks/pre-commit` pass before commit.
- [ ] `/code-review` or documented peer review runs for code-bearing stories.
- [ ] `/story-done` updates story, sprint status, and active session state only after evidence is complete.

## Next Gate

Run `/dev-story S2-FOUNDATION-01-unity-project-shell` after this QA plan is written and routing files are synced.
