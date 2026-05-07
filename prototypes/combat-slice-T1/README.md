# T1 Combat Slice Profile Harness

This directory contains the headless C# runner first created for
`T1-COMBAT-10` and reused by Sprint 1.5 profiled combat-feel stories.

## Purpose

The runner produces quantitative JSONL evidence for Combat Core feel gates without booting a Unity temporary project. It compiles the same production Combat Core source consumed by the existing test bridge, loads `assets/data/combat/t1-combat-fixtures.json`, runs seeded scenario loops, and writes story-scoped evidence under:

```text
tests/evidence/<story-id>/profiled-combat-slice.jsonl
```

Historical `T1-COMBAT-10` evidence remains in `tests/evidence/T1-COMBAT-10/`.
The no-args default now targets the current `T1.5-COMBAT-03` evidence path so a
default rerun cannot overwrite the original Sprint 1 profile. Human
qualitative evaluation remains owned by the later slice review session.

## Run

```powershell
dotnet run --project prototypes/combat-slice-T1/Harness/CombatSliceHarness.csproj
```

Story-specific evidence runs can target a different evidence folder explicitly:

```powershell
dotnet run --project prototypes/combat-slice-T1/Harness/CombatSliceHarness.csproj -- --evidence-story T1.5-COMBAT-03 --timestamp 2026-05-07T00:00:00-04:00
```

Expected behavior:

- `SoloTrash_EvenCon_T1`: 20 seeded trials.
- `NamedSoloBlock_T1`: 10 seeded trials.
- `TwoTrash_Overpull_T1`: 10 seeded trials.
- `MedBreak_Pacing_T1`: deterministic post-combat sitting regen check.
- `DevBuild_StructuralSmoke_T1`: static Combat Core presentation/audio boundary scan.

The runner exits non-zero if any quantitative criterion fails.

For `T1.5-COMBAT-03`, the runner still records `SoloTrash_EvenCon_T1` exactly
as measured, but it does not fail the process for that scenario. FEEL-01 target
revalidation is owned by `T1.5-COMBAT-04`; T1.5-COMBAT-03 gates on FEEL-03,
named solo-block, med-break pacing, and structural smoke.

## Boundary

This is a metrics harness, not a playable prototype. It does not own presentation, audio playback, final tuning, or a slice verdict. It exists only to produce deterministic evidence that the current production Combat Core source satisfies the story's quantitative gates.
