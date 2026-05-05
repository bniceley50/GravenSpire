# T1 Combat Slice Profile Harness

This directory contains the headless C# runner for `T1-COMBAT-10`.

## Purpose

The runner produces quantitative JSONL evidence for the Sprint 1 Combat Core feel gates without booting a Unity temporary project. It compiles the same production Combat Core source consumed by the existing test bridge, loads `assets/data/combat/t1-combat-fixtures.json`, runs seeded scenario loops, and writes:

```text
tests/evidence/T1-COMBAT-10/profiled-combat-slice.jsonl
```

Human qualitative evaluation remains owned by the later slice review session.

## Run

```powershell
dotnet run --project prototypes/combat-slice-T1/Harness/CombatSliceHarness.csproj
```

Expected behavior:

- `SoloTrash_EvenCon_T1`: 20 seeded trials.
- `NamedSoloBlock_T1`: 10 seeded trials.
- `TwoTrash_Overpull_T1`: 10 seeded trials.
- `MedBreak_Pacing_T1`: deterministic post-combat sitting regen check.
- `DevBuild_StructuralSmoke_T1`: static Combat Core presentation/audio boundary scan.

The runner exits non-zero if any quantitative criterion fails.

## Boundary

This is a metrics harness, not a playable prototype. It does not own presentation, audio playback, final tuning, or a slice verdict. It exists only to produce deterministic evidence that the current production Combat Core source satisfies the story's quantitative gates.
