# T1-COMBAT-11 Verification

## Baseline And Test Gate

Implementation target baseline:

```powershell
git rev-parse HEAD
```

Result: `015d417daa449e1c1f51fa7b53051fbfbab621cb`.

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
| `docs/registry/architecture.yaml:481` through `docs/registry/architecture.yaml:704` | Ingested + evaluated | `tests/architecture/forbidden_pattern_compliance_scan_test.cs:16` through `tests/architecture/forbidden_pattern_compliance_scan_test.cs:38` lists the expected registry IDs; `tests/architecture/forbidden_pattern_compliance_scan_test.cs:354` through `tests/architecture/forbidden_pattern_compliance_scan_test.cs:378` maps each id to an explicit evaluator; parser starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:658`. |
| `docs/architecture/adr-0006-endurance-resource-model.md:79` through `docs/architecture/adr-0006-endurance-resource-model.md:90` | Addendum | `tests/architecture/forbidden_pattern_compliance_scan_test.cs:40` through `tests/architecture/forbidden_pattern_compliance_scan_test.cs:47` names the ADR-0006 quiet-Endurance addendum patterns; evaluator dispatch is at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:589` through `tests/architecture/forbidden_pattern_compliance_scan_test.cs:623`. |
| `tests/architecture/README.md:5` through `tests/architecture/README.md:16` | Scope policy | `tests/architecture/forbidden_pattern_compliance_scan_test.cs:99` through `tests/architecture/forbidden_pattern_compliance_scan_test.cs:110` implements the T1 scope negative scan over production source/data only; regex helper starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:805`. |
| Registry status drift | Known carryover | `tests/architecture/forbidden_pattern_compliance_scan_test.cs:73` through `tests/architecture/forbidden_pattern_compliance_scan_test.cs:95` reports `proposed` registry rows whose governing ADRs are already `Accepted`; drift does not downgrade enforcement. |

## Acceptance Criteria Evidence

| AC | Status | Evidence |
| --- | --- | --- |
| `AC-11-01` Architecture source ingestion | PASS | Registry + ADR-0006 addendum are named and evaluated by `test_ac_11_01_registry_and_adr0006_addendum_patterns_are_named_and_evaluated` at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:50`; compliance report construction is at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:328`. |
| `AC-11-02` T1 scope scan | PASS | `test_ac_11_02_t1_scope_terms_are_absent_from_production_surfaces_and_failure_sample_is_caught` scans production Combat source and production JSON data at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:99`; path helpers start at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:908`. |
| `AC-11-03` Combat/Progression/NPC boundary scan | PASS | `test_ac_11_03_combat_progression_npc_identity_boundaries_hold` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:114`; it verifies `PlayerKillCreditEvent` shape, approved Progression field reads, no `combat_actor_id` leak outside Combat-owned surfaces, and no live NPC dependency. |
| `AC-11-04` Save/Load barrier scan | PASS | `test_ac_11_04_save_load_barrier_boundaries_hold` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:162`; it verifies grouped save failure precedes writer calls and scans `src/core/save/**` for unbounded wait calls. |
| `AC-11-05` Progression snapshot scan | PASS | `test_ac_11_05_progression_snapshot_boundaries_hold` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:187`; reflection verifies `CombatProgressionBaselineSnapshot` fields and static scans prevent generic/non-Combat snapshot consumers. |
| `AC-11-06` First-save/first-load scan | PASS | `test_ac_11_06_first_save_and_identity_boundaries_hold` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:231`; it scans Save/Load and Progression paths for illegal local-character-id generation, first-load synthesis, and re-materialization terms. |
| `AC-11-07` Pacing fixture scan | PASS | `test_ac_11_07_progression_pacing_fixture_boundaries_hold` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:253`; it scans production source plus `assets/data/**/*.json` for synthetic pacing evidence misuse and proves a deliberate sample is caught. |
| `AC-11-08` Endurance scan | PASS | `test_ac_11_08_quiet_endurance_boundaries_hold` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:263`; it scans production Combat source/data for ADR-0006 action-rotation, HUD prominence, pulse/combo, callout, and fast-regeneration patterns. |
| `AC-11-09` Ability resolved-event scan | PASS WITH KNOWN-CARRYOVER | `test_ac_11_09_ability_resolved_event_payload_is_known_carryover_not_universal_spend` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:273`; helper classification at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:622` returns `KnownCarryover` only while no shipping production consumer treats `ManaSpent` as universal resource-spend evidence and the physical-resource guard / Bash carryover tests remain present. |
| `AC-11-10` Failure fixture | PASS | `test_ac_11_10_deliberate_failure_samples_are_caught_without_production_mutation` starts at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:304`; path-labelled bad samples are defined at `tests/architecture/forbidden_pattern_compliance_scan_test.cs:860` and asserted absent from production source/data. |
| `AC-11-11` Local gate integration | PASS | `tests/Gravenspire.Combat.Tests.csproj:21` includes architecture tests; the local gate command above passed `159/159`. |
| `AC-11-12` Evidence output | PASS | This file records command, base SHA, source pattern set, scanner summary, failure-fixture result, and pass/fail status. |

## Negative-Scope Results

- No production Combat source, Progression source, NPC source, Save/Load source,
  fixture data, GDD, ADR, DECISIONS.md, sprint-status, or session-state files
  were modified.
- Real violation scans are path-scoped to production source/data. Documentation,
  historical evidence, and prototypes are intentionally excluded from production
  failure classification.
- Failure samples are path-labelled `tests/architecture/fixtures/**` source
  samples inside `tests/architecture/forbidden_pattern_compliance_scan_test.cs:860`
  through `tests/architecture/forbidden_pattern_compliance_scan_test.cs:878`;
  no deliberate bad pattern was added to production source.

## Final Hygiene

Commands:

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "console;verbosity=minimal"
```

Result: PASS, `159/159`.

Additional hygiene commands:

```powershell
git add -- production/stories/t1-combat-11-forbidden-pattern-compliance-scan-analyzer.md tests/Gravenspire.Combat.Tests.csproj tests/architecture/forbidden_pattern_compliance_scan_test.cs tests/evidence/T1-COMBAT-11/verification.md
git diff --cached --name-only
bash .githooks/pre-commit
git diff --check
git diff --cached --check
git diff --name-only HEAD -- src assets/data/combat design/gdd docs/architecture DECISIONS.md production/sprint-status.yaml production/session-state/active.md production/qa prototypes tools
git ls-files --others --exclude-standard -- src assets/data/combat design/gdd docs/architecture DECISIONS.md production/sprint-status.yaml production/session-state/active.md production/qa prototypes tools
```

Result: PASS. `git diff --cached --name-only` returned exactly the four approved
batch files; `bash .githooks/pre-commit` returned `[pre-commit] OK`;
`git diff --check` and `git diff --cached --check` returned no whitespace
errors, with Git CRLF warnings only. Forbidden-zone tracked and untracked scans
returned empty output. `git status --short -uall` still reports unrelated
untracked `.claude/**` and `all-skills-claude*.patch` files outside this batch;
none are staged.
