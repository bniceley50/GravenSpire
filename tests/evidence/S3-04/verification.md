# S3-04 Verification

**Story:** `production/stories/s3-04-player-driven-vendor.md`
**Branch:** `codex/s3-04-player-driven-vendor`
**Date:** 2026-05-30
**Verdict:** PASS

S3-04 wires the existing M3 fixed-profile vendor sell-side path behind the S3 player interaction harness. The new adapter is sell-only, carries the D017 `SERVER-AUTH-INTENT` annotation, and leaves the protected M3 vendor runtime source zero-diff.

## Implementation Surface

| File | Purpose |
|---|---|
| `Assets/Scripts/M3VendorInteractTarget.cs` | New thin vendor interaction adapter. Implements `IPlayerInteractTarget`, calls `TrySellRecoveredSalvage`, emits `vendor_salvage_sold` then `vendor_sell_copper_applied`, and exposes no buy-side route. |
| `Assets/Scripts/S3PlayerInteractionHarness.cs` | Adds optional `InteractContext.FeedbackText`; existing targets keep default fired feedback, while the vendor adapter can surface `+N copper` as result-only feedback. |
| `Assets/Editor/GravenspireS3PlayerDrivenVendorBuilder.cs` | Adapter-only scene wiring builder; attaches `M3VendorInteractTarget` to the existing `M3_CourtVendor` object. |
| `Assets/Editor/GravenspireS3PlayerDrivenVendorVerificationRunner.cs` | Unity batchmode runner covering S3-04 T1-T6: blocked sale, success sale, post-sale state, feedback rule, buy-side absence, and end-to-end harness path. |
| `Assets/Scenes/_DevEntry.unity` | Scene wiring only: `M3_CourtVendor` gains `M3VendorInteractTarget` with `_vendor` pointing at the existing `M3LootTableFixedProfileVendor`. |

## Source Evidence

| Claim | Evidence |
|---|---|
| Adapter exists and is D017-annotated | `Assets/Scripts/M3VendorInteractTarget.cs:12` records `SERVER-AUTH-INTENT`; `Assets/Scripts/M3VendorInteractTarget.cs:17` declares `M3VendorInteractTarget : MonoBehaviour, IPlayerInteractTarget, IPlayerInteractTelemetryTarget`. |
| Adapter uses sell-side M3 API only | `Assets/Scripts/M3VendorInteractTarget.cs:58` starts `TryInteract`; `Assets/Scripts/M3VendorInteractTarget.cs:74` calls `TrySellRecoveredSalvage`; `tests/evidence/S3-04/buy-side-absence-scan-20260530.txt:8` and `:14` record no buy-side matches in the adapter. |
| Two vendor telemetry events are stable | `Assets/Scripts/M3VendorInteractTarget.cs:19` defines `vendor_salvage_sold`; `Assets/Scripts/M3VendorInteractTarget.cs:20` defines `vendor_sell_copper_applied`; `Assets/Scripts/M3VendorInteractTarget.cs:85` and `:87` add those events in order. |
| Result-only feedback is explicit | `Assets/Scripts/S3PlayerInteractionHarness.cs:101` adds optional `FeedbackText`; `Assets/Scripts/S3PlayerInteractionHarness.cs:371` through `:375` uses it for fired feedback only when supplied. |
| Scene wiring is adapter-only | `Assets/Scenes/_DevEntry.unity:668` references the new adapter script GUID, `:670` identifies `M3VendorInteractTarget`, and `:671` binds `_vendor` to the existing M3 vendor component. |

## Acceptance Criteria

| AC | Result | Evidence |
|---|---|---|
| S3-04-01 | PASS | `M3VendorInteractTarget` exists in `Assets/Scripts/M3VendorInteractTarget.cs`; scene wiring is present at `_DevEntry.unity:668` through `:671`; smoke check `vendor_adapter_present_on_m3_court_vendor` PASS at `unity-player-driven-vendor-20260530-smoke.md:16`. |
| S3-04-02 | PASS | Adapter holds the serialized `_vendor` reference in `_DevEntry.unity:671`; `TryInteract` calls `TrySellRecoveredSalvage` at `M3VendorInteractTarget.cs:74`; smoke checks `vendor_adapter_reference_resolves` and `t2_sale_dispatch_returns_true` PASS at smoke `:29` and `:48`. |
| S3-04-03 | PASS | `m3-loot-vendor-zero-diff-20260530.txt:8` and `:14` record zero diff and zero changed file output for `M3LootTableFixedProfileVendor.cs` and `.meta`. |
| S3-04-04 | PASS | Smoke checks `t2_sale_event_order`, `t2_sale_event_payload_*`, and `t2_copper_event_payload_*` PASS at smoke `:55` through `:64`; telemetry shape records `vendor_salvage_sold>vendor_sell_copper_applied>interact_fired` at smoke `:111`. |
| S3-04-05 | PASS | Smoke checks credited copper, exact currency delta, slot decrement, salvage decrement, and single-salvage absence PASS at smoke `:51` through `:55`. |
| S3-04-06 | PASS | Fresh blocked path checks PASS at smoke `:34` through `:42`; post-sell-all blocked path checks PASS at smoke `:70` through `:74`; blocked feedback remains generic at smoke `:101` and `:119`. |
| S3-04-07 | PASS | `t2_sale_feedback_mentions_copper_result` and `t2_sale_feedback_has_no_buy_side_hint` PASS at smoke `:66` and `:67`; telemetry shows feedback `+7 copper` at smoke `:110`. |
| S3-04-08 | PASS | End-to-end accept -> recover -> sell checks PASS at smoke `:75` through `:82`; full vocabulary order is recorded at smoke `:123`. |

## Companion Evidence

| Artifact | Result |
|---|---|
| `unity-player-driven-vendor-20260530-smoke.md` | PASS at line 7; warnings and errors sections record none at lines 129 through 135. |
| `s3-01-harness-regression-20260530-smoke.md` | PASS at line 7; run because `S3PlayerInteractionHarness.cs` gained optional feedback text. |
| `m2-02-preservation-20260530-smoke.md` | PASS at line 7; `Builder Invoked: false` at line 10. |
| `m2-03-preservation-20260530-smoke.md` | PASS at line 7; `Builder Invoked: false` at line 10. |
| `m2-04-preservation-20260530-smoke.md` | PASS at line 7; `Builder Invoked: false` at line 10. |
| `m3-loot-vendor-zero-diff-20260530.txt` | PASS at lines 8 and 14. |
| `buy-side-absence-scan-20260530.txt` | PASS at lines 8 and 14. |

## Local Gates

| Gate | Result |
|---|---|
| `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` | PASS: 189/189. |
| `dotnet format tests\Gravenspire.Combat.Tests.csproj --verify-no-changes --exclude-diagnostics IDE1006` | PASS. |
| `dotnet format prototypes\combat-slice-T1\Harness\CombatSliceHarness.csproj --verify-no-changes --exclude-diagnostics IDE1006` | PASS. |
| `git diff --check` | PASS after Unity serialization cleanup. |
| Trailing-whitespace scan over changed and untracked S3-04 files | PASS: zero matches after cleanup. |
| T1 negative-scope scan over changed code/scene files | PASS: no FishNet/networking/server-backend/account-system/live-LLM/DateTime.UtcNow matches. `SERVER-AUTH-INTENT` is an intentional D017 annotation, not a runtime server implementation. |
| `.githooks/pre-commit` via `C:\Program Files\Git\bin\bash.exe` | PASS: `[pre-commit] OK`. |

## Notes

- The S3-01 regression runner reintroduced legacy scene-builder drift locally (`FirstDistrict_ShellOnly_NoGameplay`, directional light, camera color). The committed `_DevEntry.unity` diff was cleaned back to adapter-only wiring after the regression/preservation evidence was generated.
- Raw Unity `.log` files were written to `%TEMP%` and are not committed; Markdown smoke artifacts are the committed evidence.
