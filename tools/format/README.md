# Format Verification

Run the local format gate from the repository root before opening a PR that adds or edits C#:

```powershell
.\tools\format\verify.ps1
```

The script exits `0` when no `.cs` files exist, which is the expected state before the first gameplay source lands. Once C# source exists, it runs `dotnet format --verify-no-changes` when a solution or project and .NET SDK are available; otherwise it runs standalone checks for the root `.editorconfig` naming and line-ending contract.

## What the fallback checks

When the standalone fallback runs (no `.sln`/`.csproj` or no .NET SDK), it enforces:

- CRLF line endings on `.cs` files
- Spaces-only indentation (no tabs)
- Type names -> `PascalCase`; first type in file must match the file name
- Method names -> `PascalCase`
- Public fields -> `PascalCase`
- Private fields -> `_camelCase` (with `_` prefix)
- `const` fields -> `UPPER_SNAKE_CASE`

These mirror the `.editorconfig` naming rules for the pre-SDK state.
