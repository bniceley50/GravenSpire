# EditMode Tests

Use EditMode tests for logic that does not require a live scene:

- Combat formulas.
- Actor schema validation.
- Fixture and data validation.
- Attack state transition tables that can run without GameObjects.
- Kill-credit DTO schema checks.
- `CombatProgressionBaselineSnapshot` schema checks.
- Forbidden-pattern scan wrappers that run inside the Unity Test Runner.

Recommended future assembly definition: `EditModeTests.asmdef`.

Run:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.14f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "$PWD" -runTests -testPlatform EditMode -testResults "tests/evidence/test-results/editmode-results.xml" -logFile "tests/evidence/test-results/editmode.log"
```

Do not add fake passing tests. The first EditMode test should be tied to `T1-COMBAT-01` actor schema, fixture validation, or ADR-0003 hydration behavior.
