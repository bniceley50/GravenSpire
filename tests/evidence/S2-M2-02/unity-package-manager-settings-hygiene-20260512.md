# Unity PackageManagerSettings Tracking - 2026-05-12

## Context

`ProjectSettings/PackageManagerSettings.asset` was Unity-generated as a side
effect of M2-02 editor sessions and surfaced as untracked after commit
`93b460e`. This hygiene batch tracks it as project-canonical Unity settings.

## Project-Canonical Determination

Content inspection confirms:

- Standard Unity YAML for `UnityEditor.PackageManager.UI.Internal.PackageManagerProjectSettings`
- Single registry entry: default `https://packages.unity.com`, marked `m_IsDefault: 1`
- No scoped registries beyond the default
- No local paths, including no `file:` URLs or local install references
- No account data, auth tokens, or user-identifying values
- No environment-specific paths

The file is appropriate to track as project configuration.

## Whitespace Exception

Unity emits canonical trailing whitespace on blank-value lines:

- `m_Name: ` on lines 13 and 24
- `m_UserSelectedRegistryName: ` on line 33
- `m_ErrorMessage: ` on line 37

These match the established `ProjectSettings/TagManager.asset` pattern: Unity's
serializer treats the trailing space as a blank-value indicator. The
`.gitattributes` entry mirrors the `da4e177` precedent, keeping the per-file
exception model rather than relaxing trailing-whitespace hygiene globally.

## Known Minor Risk

Lines 38-39, `m_UserModificationsInstanceId` and `m_OriginalInstanceId`,
contain Unity-internal session IDs as negative values. These may or may not be
stable across Unity sessions. If future commits surface diff churn on these
specific fields without other settings changes, mitigation options are:

1. `git update-index --skip-worktree ProjectSettings/PackageManagerSettings.asset` to ignore working-tree changes for this file
2. Accept the churn as cosmetic if the values shift but no semantic settings change
3. Investigate Unity's behavior for these fields and apply a more targeted fix

No action is needed today; this is documented as a watch item for future
hygiene work.

## Verification

- `git diff --check` should pass after the `.gitattributes` entry takes effect.
- `.githooks/pre-commit` should pass.
- File scope: this hygiene batch tracks only `PackageManagerSettings.asset`; no
  other Unity-generated settings are affected.
