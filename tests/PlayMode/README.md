# PlayMode Tests

Use PlayMode tests for runtime integration behavior:

- Unity lifecycle, scene, coroutine, or physics behavior.
- Targeting, pull, social assist, leash, and combat-zone gates.
- Fixed combat tick and pause behavior.
- Slow casts, interruptions, recovery, and tactical instant lifecycle.
- Save/Load hydration ordering and grouped save-barrier behavior.
- HUD-facing event/accessor behavior without final HUD presentation ownership.
- Profile and smoke harnesses that require runtime GameObjects.

Recommended future assembly definition: `PlayModeTests.asmdef`.

Run:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.14f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "$PWD" -runTests -testPlatform PlayMode -testResults "tests/evidence/test-results/playmode-results.xml" -logFile "tests/evidence/test-results/playmode.log"
```

Do not create scenes or production source from `/test-setup`. Add PlayMode tests only as each `/dev-story` creates the corresponding runtime surface.
