# T1-COMBAT-11 Verification

## Baseline And Test Gate

Reviewed follow-up commit:

```powershell
git rev-parse f4f191a
```

Result: `f4f191a7c5587696067d5b3bc9a9e85560bfc21d`.

Reviewed implementation commit:

```powershell
git rev-parse 0e603a7
```

Result: `0e603a7f5aa87d19d9ddc6b25f78e104ac48ff75`.

Implementation parent baseline:

```powershell
git rev-parse 0e603a7^
```

Result: `015d417daa449e1c1f51fa7b53051fbfbab621cb`.

Follow-up parent:

```powershell
git rev-parse f4f191a^
```

Result: `0e603a7f5aa87d19d9ddc6b25f78e104ac48ff75`.

Engine version: Unity 6.3 LTS per `.claude/docs/technical-preferences.md`.
Fixture-set version: N/A; this story does not tune or execute combat fixture
data. The scanner reads production source and JSON data as static text only.

Post-implementation command:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"
```

Result: PASS, `159/159` on 2026-05-08. Previous gate was `149/149`; the
additional `10` tests are the T1-COMBAT-11 architecture compliance scanner.

`tests/Gravenspire.Combat.Tests.csproj:21` explicitly includes
`tests/architecture/*.cs`, so the scanner runs inside the existing local test
bridge rather than as a separate CLI or Roslyn analyzer.

## Pattern Source Set

| Source | Status | Evidence |
| --- | --- | --- |
| `docs/registry/architecture.yaml:481` through `docs/registry/architecture.yaml:704` | Ingested + evaluated | `tests/architecture/forbidden_pattern_compliance_scan_test.cs:16` through `tests/architecture/forbidden_pattern_compliance_scan_test.cs:38` lists the expected registry IDs; `tests/architecture/forbidden_pattern_compliance_scan_test.cs:358` through `tests/architecture/forbidden_pattern_compliance_scan_test.cs:381` maps each id to an explicit evaluator; parser starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:656`. |
| `docs/architecture/adr-0006-endurance-resource-model.md:79` through `docs/architecture/adr-0006-endurance-resource-model.md:90` | Addendum | `tests/architecture/forbidden_pattern_compliance_scan_test.cs:40` through `tests/architecture/forbidden_pattern_compliance_scan_test.cs:47` names the ADR-0006 quiet-Endurance addendum patterns; evaluator dispatch is at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:587` through `tests/architecture/forbidden_pattern_compliance_scan_test.cs:618`. |
| `tests/architecture/README.md:5` through `tests/architecture/README.md:16` | Scope policy | `tests/architecture/forbidden_pattern_compliance_scan_test.cs:99` through `tests/architecture/forbidden_pattern_compliance_scan_test.cs:110` implements the T1 scope negative scan over production source/data only; line-match helpers start at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:760`, whole-file helpers start at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:780`, and `ContainsAny` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:801`. |
| Registry status drift | Known carryover | `tests/architecture/forbidden_pattern_compliance_scan_test.cs:73` through `tests/architecture/forbidden_pattern_compliance_scan_test.cs:95` reports `proposed` registry rows whose governing ADRs are already `Accepted`; drift does not downgrade enforcement. |

## Per-Pattern Scanner Output

The scanner builds `ComplianceResult` rows at
`tests/architecture/forbidden_pattern_compliance_scan_test.cs:332` through
`tests/architecture/forbidden_pattern_compliance_scan_test.cs:355`. Current
post-review scanner states in this follow-up diff for the implementation chain
reviewed at `f4f191a7c5587696067d5b3bc9a9e85560bfc21d`:

| Pattern | State | Evaluator evidence |
| --- | --- | --- |
| `combat_actor_id_as_xp_identity` | KNOWN-CARRYOVER | `EvaluateCombatActorIdAsXpIdentity`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:385` |
| `live_npc_state_xp_lookup_after_death` | KNOWN-CARRYOVER | `EvaluateLiveNpcStateXpLookupAfterDeath`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:405` |
| `t1_nonrepeatable_firstkill_shipping_rows` | KNOWN-CARRYOVER | `EvaluateT1NonrepeatableFirstKillShippingRows`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:416` |
| `direct_save_read_of_transient_downstream_state` | KNOWN-CARRYOVER | `EvaluateDirectSaveReadOfTransientDownstreamState`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:424` |
| `unbounded_downstream_save_wait` | KNOWN-CARRYOVER | `EvaluateUnboundedDownstreamSaveWait`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:433` |
| `partial_group_payload_serialization` | KNOWN-CARRYOVER | `EvaluatePartialGroupPayloadSerialization`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:444` |
| `generic_all_consumer_progression_baseline_snapshot` | KNOWN-CARRYOVER | `EvaluateGenericAllConsumerProgressionBaselineSnapshot`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:457` |
| `combat_consuming_visible_level` | KNOWN-CARRYOVER | `EvaluateCombatConsumingVisibleLevel`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:465` |
| `ui_or_spell_consumer_reading_combat_snapshot` | KNOWN-CARRYOVER | `EvaluateUiOrSpellConsumerReadingCombatSnapshot`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:478` |
| `consumer_mutating_progression_snapshot` | KNOWN-CARRYOVER | `EvaluateConsumerMutatingProgressionSnapshot`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:486` |
| `save_load_generating_local_character_id` | PASS | `EvaluateSaveLoadGeneratingLocalCharacterId`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:501` |
| `first_save_seed_only_without_required_materialization` | PASS | `EvaluateFirstSaveSeedOnlyWithoutRequiredMaterialization`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:511`; whole-file forbidden terms at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:872` |
| `first_load_synthesizing_progression_state` | PASS | `EvaluateFirstLoadSynthesizingProgressionState`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:518` |
| `rematerializing_existing_record_on_load` | PASS | `EvaluateRematerializingExistingRecordOnLoad`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:528` |
| `local_character_id_derived_from_player_authored_data` | PASS | `EvaluateLocalCharacterIdDerivedFromPlayerAuthoredData`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:536` |
| `synthetic_fixture_as_pacing_evidence` | PASS | `EvaluateSyntheticFixtureAsPacingEvidence`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:544` |
| `profiled_pacing_without_preflight` | PASS | `EvaluateProfiledPacingWithoutPreflight`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:554` |
| `legal_pacing_fixture_without_adr0001_lookup` | PASS | `EvaluateLegalPacingFixtureWithoutAdr0001Lookup`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:564` |
| `lockout_route_projected_as_repeatable` | PASS | `EvaluateLockoutRouteProjectedAsRepeatable`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:573` |
| `pacing_fixture_with_ambiguous_kind` | PASS | `EvaluatePacingFixtureWithAmbiguousKind`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:582`; whole-file forbidden terms at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:884` |
| `endurance_action_rotation_bar` | PASS | ADR-0006 addendum evaluator, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:591` |
| `endurance_hud_prominence_above_mana` | PASS | ADR-0006 addendum evaluator, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:596` |
| `endurance_pulse_combo_celebratory_treatment` | PASS | ADR-0006 addendum evaluator, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:601` |
| `shipping_per_ability_endurance_callout` | PASS | ADR-0006 addendum evaluator, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:606` |
| `combat_rotation_fast_endurance_regeneration` | PASS | ADR-0006 addendum evaluator, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:611` |
| `ability_resolved_event_mana_spent_payload` | KNOWN-CARRYOVER | `ClassifyAbilityResolvedEventPayload`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:620` |

## Acceptance Criteria Evidence

| AC | Status | Evidence |
| --- | --- | --- |
| `AC-11-01` Architecture source ingestion | PASS | Registry + ADR-0006 addendum are named and evaluated by `test_ac_11_01_registry_and_adr0006_addendum_patterns_are_named_and_evaluated` at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:50`; compliance report construction is at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:332`; per-pattern output table is recorded above. |
| `AC-11-02` T1 scope scan | PASS | `test_ac_11_02_t1_scope_terms_are_absent_from_production_surfaces_and_failure_sample_is_caught` scans production Combat source and production JSON data at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:99`; path helpers start at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:967`, `tests/architecture/forbidden_pattern_compliance_scan_test.cs:979`, and `tests/architecture/forbidden_pattern_compliance_scan_test.cs:992`. |
| `AC-11-03` Combat/Progression/NPC boundary scan | PASS | `test_ac_11_03_combat_progression_npc_identity_boundaries_hold` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:114`; it verifies `PlayerKillCreditEvent` shape, approved Progression field reads, no `combat_actor_id` leak outside Combat-owned surfaces, and no live NPC dependency. |
| `AC-11-04` Save/Load barrier scan | PASS | `test_ac_11_04_save_load_barrier_boundaries_hold` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:162`; it verifies grouped save failure precedes writer calls and scans `src/core/save/**` for unbounded wait calls. |
| `AC-11-05` Progression snapshot scan | PASS | `test_ac_11_05_progression_snapshot_boundaries_hold` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:187`; reflection verifies `CombatProgressionBaselineSnapshot` fields and static scans prevent generic/non-Combat snapshot consumers. |
| `AC-11-06` First-save/first-load scan | PASS | `test_ac_11_06_first_save_and_identity_boundaries_hold` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:231`; it scans Save/Load and Progression paths for illegal local-character-id generation, first-load synthesis, re-materialization, and seed-only first-save materialization bypass via whole-file matching at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:250`. |
| `AC-11-07` Pacing fixture scan | PASS | `test_ac_11_07_progression_pacing_fixture_boundaries_hold` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:254`; it scans production source plus `assets/data/**/*.json` for synthetic pacing evidence misuse and checks ambiguous `fixture_kind` patterns with whole-file matching at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:259`. |
| `AC-11-08` Endurance scan | PASS | `test_ac_11_08_quiet_endurance_boundaries_hold` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:265`; it scans production Combat source/data for ADR-0006 action-rotation, HUD prominence, pulse/combo, callout, and fast-regeneration patterns. |
| `AC-11-09` Ability resolved-event scan | PASS WITH KNOWN-CARRYOVER | `test_ac_11_09_ability_resolved_event_payload_is_known_carryover_not_universal_spend` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:275`; helper classification at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:620` returns `KnownCarryover` only while no shipping production consumer treats `ManaSpent` as universal resource-spend evidence and the physical-resource guard / Bash carryover tests remain present. |
| `AC-11-10` Failure fixture | PASS | `test_ac_11_10_deliberate_failure_samples_are_caught_without_production_mutation` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:306`; path-labelled bad samples are defined at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:911` and asserted absent from production source/data. |
| `AC-11-11` Local gate integration | PASS | `tests/Gravenspire.Combat.Tests.csproj:21` includes architecture tests; the local gate command above passed `159/159`. |
| `AC-11-12` Evidence output | PASS | This file records command, implementation commit SHA, source pattern set, scanner summary, per-pattern pass/fail/known-carryover output, failure-fixture result, and final pass/fail status. |

## Negative-Scope Results

- No production Combat source, Progression source, NPC source, Save/Load source,
  fixture data, GDD, ADR, DECISIONS.md, sprint-status, or session-state files
  were modified.
- Real violation scans are path-scoped to production source/data. Documentation,
  historical evidence, and prototypes are intentionally excluded from production
  failure classification.
- Failure samples are path-labelled `tests/architecture/fixtures/**` source
  samples inside `tests/architecture/forbidden_pattern_compliance_scan_test.cs:911`
  through `tests/architecture/forbidden_pattern_compliance_scan_test.cs:942`;
  no deliberate bad pattern was added to production source.

## Final Hygiene

Commands:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"
```

Result: PASS, `159/159`.

Additional hygiene commands:

```powershell
git add -- production/stories/t1-combat-11-forbidden-pattern-compliance-scan-analyzer.md tests/architecture/forbidden_pattern_compliance_scan_test.cs tests/evidence/T1-COMBAT-11/verification.md
git diff --cached --name-only
bash .githooks/pre-commit
git diff --check
git diff --cached --check
git diff --name-only HEAD -- src assets/data/combat design/gdd docs/architecture DECISIONS.md production/sprint-status.yaml production/session-state/active.md production/qa prototypes tools
git ls-files --others --exclude-standard -- src assets/data/combat design/gdd docs/architecture DECISIONS.md production/sprint-status.yaml production/session-state/active.md production/qa prototypes tools
```

Result: PASS. `git diff --cached --name-only` returned exactly the three
approved follow-up files; `bash .githooks/pre-commit` returned `[pre-commit] OK`;
`git diff --check` and `git diff --cached --check` returned no whitespace
errors, with Git CRLF warnings only. Forbidden-zone tracked and untracked scans
returned empty output. `git status --short -uall` still reports unrelated
untracked `.claude/**` and `all-skills-claude*.patch` files outside this batch;
none are staged.
