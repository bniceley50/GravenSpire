# S2-M2-01 Verification

**Story:** `production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md`
**Status:** PASS
**Date:** 2026-05-11

## Scope Verified

`S2-M2-01` implements the first Unity runtime bridge over the existing
engine-agnostic Combat Core package. The bridge consumes the approved T1
fixture file, hydrates one player actor and one hostile actor, and records M2
smoke evidence without modifying `_DevEntry.unity`.

## Implementation Evidence

| Area | Evidence |
| --- | --- |
| Local Combat Core Unity package | `Packages/manifest.json` includes `com.gravenspire.gameplay.combat`; `src/gameplay/combat/package.json:8` declares `com.unity.nuget.newtonsoft-json` for Unity fixture deserialization. |
| Unity C# compatibility | `src/gameplay/combat/csc.rsp:1` pins `-langversion:10.0`; `src/gameplay/combat/compat/IsExternalInit.cs:9` supplies the Unity netstandard marker for records/init-only setters. |
| .NET / Unity JSON split | `src/gameplay/combat/fixtures/CombatFixtureLoader.cs:5` branches on `UNITY_5_3_OR_NEWER`; Unity uses `JsonConvert` at `src/gameplay/combat/fixtures/CombatFixtureLoader.cs:55`, while .NET tests keep `System.Text.Json` at `src/gameplay/combat/fixtures/CombatFixtureLoader.cs:57`. |
| Code-review fix | `/code-review` identified a structured-failure gap: malformed JSON parser exceptions were not wrapped, so they could escape the hydrator's `InvalidDataException` catch. `src/gameplay/combat/fixtures/CombatFixtureLoader.cs:51` through `:63` now wraps both Unity/Newtonsoft and .NET/System.Text.Json parse calls as `InvalidDataException`; existing upper-layer structured failure handling remains the translation boundary. |
| Combat guard compatibility | `src/gameplay/combat/compat/CombatArgumentNull.cs:10` replaces .NET 6 `ArgumentNullException.ThrowIfNull` usage so the same Combat Core source compiles under Unity 6000.3. |
| Runtime hydrator | `src/gameplay/combat/CombatRuntimeEncounterHydrator.cs:19` loads fixture data and hydrates runtime actors from Combat Core fixtures. |
| Unity bridge | `Assets/Scripts/M2CombatCoreRuntimeBridge.cs:78` calls `CombatRuntimeEncounterHydrator.HydrateFromFile`; `Assets/Scripts/M2CombatCoreRuntimeBridgeBootstrap.cs:12` auto-bootstraps the bridge after scene load. |
| Story runner | `Assets/Editor/GravenspireM2CombatBridgeVerificationRunner.cs:21` writes M2 evidence under `tests/evidence/S2-M2-01/`; runner checks include `bridge_hydrated` at line `128` and `fixture_file_from_data_directory` at line `135`. |
| Automated tests | `tests/integration/gameplay/combat/combat_runtime_encounter_hydration_test.cs:15`, `:35`, `:53`, `:64`, and `:75` cover success hydration, fixture-value preservation, wrong fixture id, missing fixture file, and malformed fixture JSON. New regression test names: `test_runtime_encounter_hydrator_fails_loud_on_missing_fixture_file` and `test_runtime_encounter_hydrator_fails_loud_on_malformed_fixture_json`. |

## Acceptance Criteria

| AC | Result | Evidence |
| --- | --- | --- |
| `S2-M2-01-01` thin Unity adapter, no duplicate combat formulas | PASS | Unity script compilation succeeded in `tests/evidence/S2-M2-01/unity-combat-bridge-smoke-post-fix-20260511.log:235`; Unity bridge delegates to Combat Core at `Assets/Scripts/M2CombatCoreRuntimeBridge.cs:78`. |
| `S2-M2-01-02` existing T1 combat fixture handoff | PASS | Runner evidence records fixture path from `data/combat/t1-combat-fixtures.json` and encounter `SoloTrash_EvenCon_T1` in `tests/evidence/S2-M2-01/unity-combat-bridge-smoke-20260510.md:24` and `:34`. |
| `S2-M2-01-03` `_DevEntry.unity` Play Mode bridge smoke | PASS | Unity smoke result is PASS at `tests/evidence/S2-M2-01/unity-combat-bridge-smoke-20260510.md:7`; player and hostile actors are recorded at lines `36` and `37`; no errors are recorded at line `51`. |
| `S2-M2-01-04` M2 runner successor evidence isolation | PASS | Runner writes `tests/evidence/S2-M2-01/unity-combat-bridge-smoke-20260510.md`; final Unity log records the write at `tests/evidence/S2-M2-01/unity-combat-bridge-smoke-post-fix-20260511.log:470`. No S2-FOUNDATION-01 evidence file was modified. |
| `S2-M2-01-05` regression, scope, hygiene gates | PASS | `dotnet test` passed `169/169`; T1 negative-scope scan has only classified story/test/comment hits; `git diff --check` returned `0`; `.githooks/pre-commit` returned `[pre-commit] OK`. |

## Commands Run

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"
```

Result: PASS. `169` passed, `0` failed, `0` skipped.

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.14f1\Editor\Unity.exe" -batchmode -nographics -projectPath "N:\GravenSpire" -executeMethod Gravenspire.Editor.GravenspireM2CombatBridgeVerificationRunner.Run -logFile "N:\GravenSpire\tests\evidence\S2-M2-01\unity-combat-bridge-smoke-post-fix-20260511.log"
```

Result: PASS. The runner wrote
`tests/evidence/S2-M2-01/unity-combat-bridge-smoke-20260510.md` with exit code
`0`.

```powershell
rg -n -i "FishNet|\bnetworking\b|server authority|\bPvP\b|account|cloud save|OpenAI|Anthropic|live LLM|extra playable classes|second district|deep economy|\bcompanion\b|\bquest\b|\bobjective\b|\bloot\b|\bvendor\b|\bstash\b|Save/Load" Packages Assets src/gameplay/combat tests/integration/gameplay/combat production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md
```

Result: PASS WITH CLASSIFIED HITS. Hits are the story's explicit out-of-scope
ban text, one existing anti-loot assertion in
`tests/integration/gameplay/combat/combat_npc_death_kill_credit_test.cs:127`,
and existing Combat Core comments about future Save/Load handoff context in
`src/gameplay/combat/CombatActorState.cs:104` and `:150`. No FishNet,
networking implementation, server authority, PvP, accounts, cloud saves,
OpenAI, Anthropic, live LLM, extra playable classes, companion behavior,
second district, or deep economy implementation was introduced.

```powershell
git diff --check
```

Result: PASS. Git emitted CRLF conversion warnings only; no whitespace or
conflict-marker errors remained after normalizing Unity-generated evidence.

```powershell
bash .githooks/pre-commit
```

Result: PASS. Output: `[pre-commit] OK`.

## Warnings

- The Play Mode runner must be invoked without `-quit`; the runner exits Unity
  itself after writing evidence. A `-quit` invocation compiles the project but
  exits before the asynchronous Play Mode checks can write the M2 smoke file.

## Blockers

None. The `/code-review` follow-up gap is closed by the structured malformed-JSON
failure fix and the two new regression tests named above. The next gate is:

```text
/code-review production/stories/s2-m2-01-unity-combat-core-runtime-bridge.md Assets/Scripts/M2CombatCoreRuntimeBridge.cs Assets/Scripts/M2CombatCoreRuntimeBridgeBootstrap.cs Assets/Editor/GravenspireM2CombatBridgeVerificationRunner.cs src/gameplay/combat/CombatRuntimeEncounterHydrator.cs tests/integration/gameplay/combat/combat_runtime_encounter_hydration_test.cs tests/evidence/S2-M2-01/verification.md
```
