# S2-M3-03 Verification - Loot Table + Fixed-Profile Vendor

**Story:** `production/stories/s2-m3-03-loot-table-fixed-profile-vendor.md`
**Result:** COMPLETE WITH NOTES — /code-review re-review PASS_WITH_NOTES; /story-done closure passed locally
**Date:** 2026-05-20
**Implementation Branch:** `claude/s2-m3-03-loot-table-fixed-profile-vendor`
**Implementation HEAD:** PENDING COMMIT — set in sync commit

## Evidence Index

| Artifact | Purpose |
| --- | --- |
| `data/first-district/m3-objective-npc-loot.json` | Authored M3 loot/vendor data. |
| `src/gameplay/npc/m3-objective/M3LootTableFixedProfileVendorSession.cs` | Engine-agnostic session model for loot resolution and fixed-profile vendor transactions. |
| `tests/unit/gameplay/npc/m3_loot_table_fixed_profile_vendor_test.cs` | NUnit coverage for authored data, F4, fixed-profile constraints, capacity prevalidation, atomicity, and session-local state. |
| `Assets/Scripts/M3LootTableFixedProfileVendor.cs` | Unity runtime bridge for the session model. |
| `Assets/Editor/GravenspireM3LootTableFixedProfileVendorBuilder.cs` | Editor builder adding the vendor marker to `_DevEntry.unity`. |
| `Assets/Editor/GravenspireM3LootTableFixedProfileVendorVerificationRunner.cs` | Unity batchmode smoke runner for objective recovery -> loot resolution -> salvage sale. |
| `tests/evidence/S2-M3-03/unity-loot-table-fixed-profile-vendor-20260519-smoke.md` | Unity smoke result and telemetry. |

## Acceptance Criteria

| AC | Result | Evidence |
| --- | --- | --- |
| `S2-M3-03-01` | PASS | Authored JSON defines `CourtMarkedRelic_T1` and `GraveDust_Salvage_T1`; `test_authored_loot_table_resolves_relic_and_salvage_rows` passes through the shared loader; Unity smoke records `authored_data_file_valid`, `vendor_loaded_authored_data_file`, `vendor_not_using_fallback_data`, `authored_loot_table_resolved`, `resolved_relic_carried`, and `resolved_salvage_carried` as PASS. |
| `S2-M3-03-02` | PASS | `test_loot_data_uses_stable_authored_ids_and_no_combat_runtime_fields` validates stable ids and raw authored-data absence for forbidden runtime/combat fields. |
| `S2-M3-03-03` | PASS | `test_loot_resolution_does_not_reuse_kill_weight_seed` passes; Unity smoke records `loot_rng_seed_boundary_preserved` as PASS. |
| `S2-M3-03-04` | PASS | `test_default_table_contains_no_currency_container_entry` passes; session model reports no coin-faucet projection claim. |
| `S2-M3-03-05` | PASS | `test_fixed_profile_vendor_applies_f4_salvage_formula` passes with 50 nominal copper -> 7 copper; Unity smoke records `salvage_sale_credited_copper=7`. |
| `S2-M3-03-06` | PASS | `test_vendor_prevalidates_capacity_before_any_currency_debit` and `test_vendor_profile_exposes_no_dynamic_economy_hooks` pass; Unity smoke records fixed-profile purchase and all no-hook checks as PASS. |
| `S2-M3-03-07` | PASS | `test_vendor_transactions_are_synchronous_and_atomic` proves failed sale and successful purchase leave no partial state. |
| `S2-M3-03-08` | PASS | `test_vendor_state_is_session_local_and_makes_no_tuned_economy_or_persistence_claim` passes; Unity smoke records no coin-faucet projection claim and no currency-at-rest persistence. |

## Local Gates

| Gate | Result | Evidence |
| --- | --- | --- |
| `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"` | PASS | 189/189 passed after adding 9 S2-M3-03 unit tests. |
| Unity batchmode smoke | PASS | `tests/evidence/S2-M3-03/unity-loot-table-fixed-profile-vendor-20260519-smoke.md` records 39/39 checks PASS, no warnings, no errors. |
| T1 negative-scope scan | PASS | Project deny-pattern scan over the new S2-M3-03 `.cs` implementation/test files returned zero matches. Story-specific forbidden loot-scope scan over production code and authored JSON also returned zero matches. |
| `git diff --check` | PASS | Final hygiene gate returned exit 0. |
| `.githooks/pre-commit` | PASS | Temporary-index staged footprint containing only the S2-M3-03 batch returned `[pre-commit] OK`; real git index was left untouched. |

## Scope Notes

- Vendor and currency state are session-local only.
- The Unity runtime vendor path loads `data/first-district/m3-objective-npc-loot.json` as the primary data source; the in-code default is retained only as a documented missing-file fallback and was not used by the passing smoke.
- No save/load hook, persisted inventory schema, tuned economy claim, coin pacing claim, dynamic vendor profile, stock ticking, reputation discount, or broad inventory system was added.
- The Unity scene change adds only `M3_LootVendorRoot`, `M3_CourtVendor`, and `M3_CourtVendor_SalvageCounter` on top of the existing M3 objective setup.
- The 2026-05-20 Unity rerun overwrote the existing S2-M3-03 smoke path so the evidence set stays single-file for this story.
- Closure: `/code-review` (initial pass + corrected re-review) verdict is PASS_WITH_NOTES. The initial verdict missed a HIGH finding — the runtime used a hardcoded factory instead of the authored JSON. The fix made the Unity runtime load `data/first-district/m3-objective-npc-loot.json` as primary data with `CreateAuthoredM3Default()` demoted to a documented missing-file fallback, verified by smoke checks `vendor_loaded_authored_data_file` and `vendor_not_using_fallback_data`. Three non-blocking LOW notes are deferred to the runner-hygiene cleanup story per the `m3_03_low_review_notes` carryover: smoke-filename date drift, `Enum.Parse` outside the JSON catch block, and Editor-context-only path resolution (acceptable T1 boundary; built-player data delivery is INV-OQ-04/T2 scope).
