# production/pre-build-checklist.md — Pre-Steam-Upload Verification

Answer to the 2026-04-10 clinic-notes lesson: **"configured" is never
evidence.** Every item below produces a file:line citation, a screenshot,
or a console output — not a self-report.

Run before ANY Steam upload. For **T1**, run before any playtest handoff
(the handoff is our closest T1 analogue to "deploy").

---

## ProjectSettings

- [ ] Build target matches intended platform (Windows Standalone / macOS
      Standalone)
      **Evidence:** `ProjectSettings/ProjectSettings.asset` field
      `activeBuildTarget`
- [ ] Scripting backend: **IL2CPP** for release builds
      **Evidence:** `ProjectSettings/ProjectSettings.asset` field
      `scriptingBackend`
- [ ] Color space: **Linear** (URP requires it)
      **Evidence:** `ProjectSettings/ProjectSettings.asset` field
      `colorSpace`
- [ ] Graphics APIs: **DirectX 11 + DirectX 12** on Windows; **Metal** on
      macOS
      **Evidence:** `ProjectSettings/ProjectSettings.asset` field
      `m_BuildTargetGraphicsAPIs`
- [ ] Managed stripping level: **Low** (safer until profiled)
      **Evidence:** `ProjectSettings/ProjectSettings.asset` field
      `managedStrippingLevel`

## Version Stamp

- [ ] `Application.version` matches the intended git tag
      **Evidence:** `git describe --tags --dirty` output matches
      `ProjectSettings.asset` `bundleVersion`
- [ ] No `-dirty` suffix on the `git describe` output
      **Evidence:** `git status` reports clean working tree

## Addressables

- [ ] All Addressables groups build without errors
      **Evidence:** build output console log, zero errors
- [ ] All scene references resolved (no missing asset warnings)
      **Evidence:** Addressables Profile report
- [ ] (T3+) Remote catalog URL matches target environment
      **Evidence:** `AddressableAssetSettings.asset` field
      `RemoteCatalogBuildPath`

## Scenes

- [ ] Build scene list matches intended release scene set
      **Evidence:** `EditorBuildSettings.asset` scenes list
- [ ] No test scenes in release build
      **Evidence:** grep build scene list for `Test`, `Debug`, `Sandbox`
      — zero matches

## Save Integrity (per `.claude/rules/save-integrity.md`)

- [ ] Save format version number bumped if any save schema changed
      **Evidence:** `SaveFormat.Version` constant diff in the PR
- [ ] Migration path for previous version tested
      **Evidence:**
      `tests/integration/save/Migration_V[n-1]_to_V[n]_test.cs` passes

## Server Config (T3+)

- [ ] Required env vars present in target environment
      **Evidence:** deployment manifest or env inspector screenshot
- [ ] Server build version matches client build version
      **Evidence:** both report the same `Application.version`

## Final Gate

- [ ] All items above have evidence cited (not "yes I checked")
- [ ] Upload proceeds only if **100% of applicable items** are checked

---

## Related

- `tasks/lessons.md` 2026-04-21 entry (the lesson that motivated this file)
- `AGENTS.md` §12 (pre-build verification policy)
- `DECISIONS.md` D005 (governance migration including this checklist)
