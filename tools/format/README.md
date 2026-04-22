# Format Verification

Run the local format gate from the repository root before opening a PR that adds or edits C#:

```powershell
.\tools\format\verify.ps1
```

The script exits `0` when no `.cs` files exist, which is the expected state before the first gameplay source lands. Once C# source exists, it runs `dotnet format --verify-no-changes` when a solution or project and .NET SDK are available; otherwise it runs standalone checks for the root `.editorconfig` naming and line-ending contract.

