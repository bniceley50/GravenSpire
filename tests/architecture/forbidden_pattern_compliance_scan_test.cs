#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Gravenspire.Gameplay.Combat;
using NUnit.Framework;

namespace Gravenspire.Tests.Architecture;

public sealed class ForbiddenPatternComplianceScanTest
{
    private static readonly string[] ExpectedRegistryPatterns =
    {
        "combat_actor_id_as_xp_identity",
        "live_npc_state_xp_lookup_after_death",
        "t1_nonrepeatable_firstkill_shipping_rows",
        "direct_save_read_of_transient_downstream_state",
        "unbounded_downstream_save_wait",
        "partial_group_payload_serialization",
        "generic_all_consumer_progression_baseline_snapshot",
        "combat_consuming_visible_level",
        "ui_or_spell_consumer_reading_combat_snapshot",
        "consumer_mutating_progression_snapshot",
        "save_load_generating_local_character_id",
        "first_save_seed_only_without_required_materialization",
        "first_load_synthesizing_progression_state",
        "rematerializing_existing_record_on_load",
        "local_character_id_derived_from_player_authored_data",
        "synthetic_fixture_as_pacing_evidence",
        "profiled_pacing_without_preflight",
        "legal_pacing_fixture_without_adr0001_lookup",
        "lockout_route_projected_as_repeatable",
        "pacing_fixture_with_ambiguous_kind"
    };

    private static readonly string[] Adr0006AddendumPatterns =
    {
        "endurance_action_rotation_bar",
        "endurance_hud_prominence_above_mana",
        "endurance_pulse_combo_celebratory_treatment",
        "shipping_per_ability_endurance_callout",
        "combat_rotation_fast_endurance_regeneration"
    };

    [Test]
    public void test_ac_11_01_registry_and_adr0006_addendum_patterns_are_named_and_evaluated()
    {
        var registryPatterns = LoadForbiddenPatternsFromRegistry();
        var evaluators = RegistryPatternEvaluators();
        var checkedPatternIds = BuildComplianceReport()
            .Select(result => result.PatternId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();

        Assert.That(
            registryPatterns.Select(pattern => pattern.Id).OrderBy(id => id, StringComparer.Ordinal).ToArray(),
            Is.EqualTo(ExpectedRegistryPatterns.OrderBy(id => id, StringComparer.Ordinal).ToArray()));
        Assert.That(
            Adr0006AddendumPatterns,
            Has.All.Matches<string>(pattern => checkedPatternIds.Contains(pattern, StringComparer.Ordinal)));
        Assert.That(
            ExpectedRegistryPatterns,
            Has.All.Matches<string>(pattern => checkedPatternIds.Contains(pattern, StringComparer.Ordinal)));
        Assert.That(
            evaluators.Keys.OrderBy(id => id, StringComparer.Ordinal).ToArray(),
            Is.EqualTo(ExpectedRegistryPatterns.OrderBy(id => id, StringComparer.Ordinal).ToArray()));

        var acceptedAdrPatternIds = registryPatterns
            .Where(pattern => string.Equals(ReadAdrStatus(pattern.AdrPath), "Accepted", StringComparison.Ordinal))
            .Select(pattern => pattern.Id)
            .ToArray();
        var acceptedRegistryDrift = registryPatterns
            .Where(pattern => string.Equals(pattern.RegistryStatus, "proposed", StringComparison.OrdinalIgnoreCase))
            .Where(pattern => string.Equals(ReadAdrStatus(pattern.AdrPath), "Accepted", StringComparison.Ordinal))
            .Select(pattern => pattern.Id)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
        var report = BuildComplianceReport();

        Assert.That(acceptedRegistryDrift, Is.SubsetOf(acceptedAdrPatternIds));
        Assert.That(acceptedRegistryDrift, Has.All.Matches<string>(pattern =>
        {
            var result = report.Single(entry => string.Equals(entry.PatternId, pattern, StringComparison.Ordinal));
            return result.State == ComplianceState.KnownCarryover;
        }));

        var failures = report
            .Where(result => result.State == ComplianceState.Fail)
            .ToArray();
        Assert.That(failures, Is.Empty, FormatComplianceResults(failures));
    }

    [Test]
    public void test_ac_11_02_t1_scope_terms_are_absent_from_production_surfaces_and_failure_sample_is_caught()
    {
        var productionFiles = ProductionCombatFiles()
            .Concat(ProductionDataFiles())
            .ToArray();
        var forbiddenTerms = T1ScopeForbiddenTerms();

        AssertNoMatches("T1 scope creep", productionFiles, forbiddenTerms);

        var sample = Literal("Fish", "Net") + " replicated combat authority with " +
            Literal("Pv", "P") + " " + Literal("comp", "anion") + " prediction";
        Assert.That(ContainsAny(sample, forbiddenTerms), Is.True);
    }

    [Test]
    public void test_ac_11_03_combat_progression_npc_identity_boundaries_hold()
    {
        AssertRecordProperties<PlayerKillCreditEvent>(new[]
        {
            "defeated_source_ref",
            "zoneId",
            "faction_id",
            "kill_weight_seed"
        });

        var nonCombatGameplayFiles = SourceFiles(
            "src/gameplay/progression",
            "src/gameplay/npc",
            "src/core/save");
        AssertNoMatches("combat actor id outside Combat-owned identity surfaces", nonCombatGameplayFiles, new[]
        {
            @"\bcombat_actor_id\b",
            @"\bCombatActorId\b"
        });

        var progressionSource = ReadText("src/gameplay/progression/CharacterProgressionKillCreditProcessor.cs");
        var killCreditFields = Regex.Matches(progressionSource, @"killCreditEvent\.([A-Za-z_][A-Za-z0-9_]*)")
            .Cast<Match>()
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.That(killCreditFields, Is.EqualTo(new[]
        {
            "defeated_source_ref",
            "faction_id",
            "kill_weight_seed",
            "zoneId"
        }.OrderBy(name => name, StringComparer.Ordinal).ToArray()));
        Assert.That(progressionSource, Does.Not.Contain("Gravenspire.Gameplay.Npc"));
        Assert.That(progressionSource, Does.Not.Contain("NpcSourceLifecycleService"));

        var deathEventSource = ReadText("src/gameplay/combat/events/CombatDeathEvents.cs");
        Assert.That(deathEventSource, Does.Not.Contain("defeated_level"));
        Assert.That(deathEventSource, Does.Not.Contain("encounter_role"));
        Assert.That(deathEventSource, Does.Not.Contain("repeatability"));
        Assert.That(deathEventSource, Does.Not.Contain("source_lifecycle"));
        Assert.That(deathEventSource, Does.Not.Contain("xp_"));
        Assert.That(deathEventSource, Does.Not.Contain("progression_transaction"));
    }

    [Test]
    public void test_ac_11_04_save_load_barrier_boundaries_hold()
    {
        var coordinator = ReadText("src/core/save/GroupedSaveAttemptCoordinator.cs");
        var unresolvedReturnIndex = coordinator.IndexOf("return new SaveAttemptResult(SaveAttemptStatus.Failed", StringComparison.Ordinal);
        var writerIndex = coordinator.IndexOf("writer.Write", StringComparison.Ordinal);

        Assert.That(unresolvedReturnIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(writerIndex, Is.GreaterThan(unresolvedReturnIndex));
        Assert.That(coordinator, Does.Contain("SaveStabilityBarrierStatus.Stable"));
        Assert.That(coordinator, Does.Contain("foreach (var barrier in barriers)"));

        var saveCoreFiles = SourceFiles("src/core/save");
        AssertNoMatches("unbounded barrier wait", saveCoreFiles, new[]
        {
            @"Thread\.Sleep\s*\(",
            @"Task\.Delay\s*\(",
            @"\.Wait\s*\(",
            @"WaitOne\s*\("
        });

        var groupedBarrierTest = ReadText("tests/integration/core/save/save_grouped_barrier_consistency_test.cs");
        Assert.That(groupedBarrierTest, Does.Contain("Writer.WriteCount, Is.EqualTo(0)").IgnoreCase);
    }

    [Test]
    public void test_ac_11_05_progression_snapshot_boundaries_hold()
    {
        AssertRecordProperties<CombatProgressionBaselineSnapshot>(new[]
        {
            "ClassId",
            "CombatActorLevel",
            "LocalCharacterId",
            "PermanentMaxHealth",
            "PermanentMaxMana",
            "ProducedFor",
            "ProgressionSchemaVersion",
            "ProgressionStateRevision"
        });

        var snapshotProperties = typeof(CombatProgressionBaselineSnapshot)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .ToArray();
        Assert.That(snapshotProperties, Has.No.Member("visible_level"));
        Assert.That(snapshotProperties, Has.No.Member("VisibleLevel"));
        Assert.That(snapshotProperties, Has.No.Member("total_xp"));
        Assert.That(snapshotProperties, Has.No.Member("TotalXp"));
        Assert.That(snapshotProperties, Has.No.Member("spell_eligibility_tier"));
        Assert.That(snapshotProperties, Has.No.Member("SpellEligibilityTier"));
        Assert.That(snapshotProperties.Any(name => name.Contains("Endurance", StringComparison.Ordinal)), Is.False);
        Assert.That(snapshotProperties.Any(name => name.Contains("Current", StringComparison.Ordinal)), Is.False);

        var sourceFiles = SourceFiles("src");
        var genericSnapshotMatches = FindMatches(
            sourceFiles,
            new[] { @"(?<!Combat)\bProgressionBaselineSnapshot\b" });
        Assert.That(genericSnapshotMatches, Is.Empty, FormatMatches(genericSnapshotMatches));

        var nonCombatSource = SourceFiles(
            "src/core/save",
            "src/gameplay/progression",
            "src/gameplay/npc");
        AssertNoMatches("non-Combat consumer reading Combat snapshot", nonCombatSource, new[]
        {
            @"\bCombatProgressionBaselineSnapshot\b"
        });
    }

    [Test]
    public void test_ac_11_06_first_save_and_identity_boundaries_hold()
    {
        var files = SourceFiles("src/core/save", "src/gameplay/progression");

        AssertNoMatches("local character id generated by Save/Load or Progression", files, new[]
        {
            @"Guid\.NewGuid\s*\(",
            @"new\s+Guid\s*\(",
            @"RandomNumberGenerator",
            @"\bRandom\b.*local_character_id",
            @"local_character_id.*(?:character_name|save_slot|account|device|path|CombatActorId|combat_actor_id)"
        });
        AssertNoMatches("first load synthesizing progression state", files, new[]
        {
            @"(?:first\s+load|continue|load).{0,120}starting_class_id",
            @"starting_class_id.{0,120}(?:first\s+load|continue|load)",
            @"Synthesiz(?:e|ing).*CharacterProgressionSaveState",
            @"Re[-]?materializ(?:e|ing)"
        });
        AssertNoWholeFileMatches("seed-only first-save materialization bypass", files, FirstSaveSeedOnlyForbiddenTerms());
    }

    [Test]
    public void test_ac_11_07_progression_pacing_fixture_boundaries_hold()
    {
        var productionFiles = ProductionSourceAndDataFiles();

        AssertNoMatches("synthetic pacing evidence misuse", productionFiles, PacingForbiddenTerms());
        AssertNoWholeFileMatches("ambiguous pacing fixture kind", ProductionDataFiles(), PacingFixtureKindForbiddenTerms());

        Assert.That(ContainsAny("SyntheticEventTransaction used as XP/hour evidence", new[] { @"\bSyntheticEventTransaction\b" }), Is.True);
    }

    [Test]
    public void test_ac_11_08_quiet_endurance_boundaries_hold()
    {
        var files = ProductionCombatFiles()
            .Concat(ProductionDataFiles())
            .ToArray();

        AssertNoMatches("quiet Endurance forbidden pattern", files, QuietEnduranceForbiddenTerms());
    }

    [Test]
    public void test_ac_11_09_ability_resolved_event_payload_is_known_carryover_not_universal_spend()
    {
        var abilityProperties = typeof(AbilityResolvedEvent)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.That(abilityProperties, Does.Contain("ManaSpent"));
        Assert.That(abilityProperties, Does.Not.Contain("EnduranceSpent"));
        Assert.That(abilityProperties, Does.Not.Contain("ResourceSpent"));

        var productionConsumers = SourceFiles("src")
            .Where(file => !file.RelativePath.EndsWith("CombatAbilityLifecycleEvents.cs", StringComparison.Ordinal))
            .Where(file => !file.RelativePath.EndsWith("CombatCastLifecycleEvents.cs", StringComparison.Ordinal))
            .Where(file => !file.RelativePath.EndsWith("CombatInstantAbilityResolver.cs", StringComparison.Ordinal))
            .ToArray();
        AssertNoMatches("shipping consumer treating ManaSpent as universal spend", productionConsumers, new[]
        {
            @"\.ManaSpent\b"
        });

        var resolver = ReadText("src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs");
        Assert.That(resolver, Does.Contain("profile.ResourceKind == CombatTacticalAbilityResourceKind.Physical"));
        Assert.That(resolver, Does.Contain("caster.WithCurrentEndurance"));
        Assert.That(PhysicalResourceSpendIsGuarded(), Is.True);
        Assert.That(BashManaSpentCarryoverIsCoveredByTests(), Is.True);
        Assert.That(ClassifyAbilityResolvedEventPayload(), Is.EqualTo(ComplianceState.KnownCarryover));
    }

    [Test]
    public void test_ac_11_10_deliberate_failure_samples_are_caught_without_production_mutation()
    {
        var samples = FailureFixtureSamples();
        var samplesById = samples.ToDictionary(
            sample => Path.GetFileNameWithoutExtension(sample.RelativePath),
            sample => sample,
            StringComparer.Ordinal);

        Assert.That(samples, Has.All.Matches<SourceFile>(sample =>
            sample.RelativePath.StartsWith("tests/architecture/fixtures/", StringComparison.Ordinal)));
        Assert.That(FindMatches(new[] { samplesById["t1_scope"] }, T1ScopeForbiddenTerms()), Is.Not.Empty);
        Assert.That(FindMatches(new[] { samplesById["combat_actor_id_identity"] }, new[] { @"combat_actor_id" }), Is.Not.Empty);
        Assert.That(FindMatches(new[] { samplesById["progression_snapshot"] }, new[] { @"(?<!Combat)\bProgressionBaselineSnapshot\b", @"visible_level", @"spell_eligibility_tier" }), Is.Not.Empty);
        Assert.That(FindMatches(new[] { samplesById["quiet_endurance"] }, QuietEnduranceForbiddenTerms()), Is.Not.Empty);
        Assert.That(FindMatches(new[] { samplesById["pacing"] }, PacingForbiddenTerms()), Is.Not.Empty);
        Assert.That(FindWholeFileMatches(new[] { samplesById["first_save_seed_only"] }, FirstSaveSeedOnlyForbiddenTerms()), Is.Not.Empty);
        Assert.That(FindWholeFileMatches(new[] { samplesById["ambiguous_fixture_kind"] }, PacingFixtureKindForbiddenTerms()), Is.Not.Empty);

        var productionFiles = ProductionSourceAndDataFiles();
        var productionText = string.Join(Environment.NewLine, productionFiles.Select(file => file.Text));
        foreach (var sample in samples)
        {
            Assert.That(productionText, Does.Not.Contain(sample.Text));
        }
    }

    private static IReadOnlyList<ComplianceResult> BuildComplianceReport()
    {
        var results = new List<ComplianceResult>();
        var evaluators = RegistryPatternEvaluators();
        foreach (var pattern in LoadForbiddenPatternsFromRegistry())
        {
            var adrStatus = ReadAdrStatus(pattern.AdrPath);
            var state = evaluators.TryGetValue(pattern.Id, out var evaluator)
                ? evaluator()
                : ComplianceState.Fail;
            if (state == ComplianceState.Pass &&
                string.Equals(pattern.RegistryStatus, "proposed", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(adrStatus, "Accepted", StringComparison.Ordinal))
            {
                state = ComplianceState.KnownCarryover;
            }

            results.Add(new ComplianceResult(pattern.Id, state, $"registry={pattern.RegistryStatus}; adr={adrStatus}"));
        }

        results.AddRange(Adr0006AddendumPatterns.Select(pattern =>
            new ComplianceResult(pattern, EvaluateAdr0006AddendumPattern(pattern), "ADR-0006 addendum")));
        results.Add(new ComplianceResult("ability_resolved_event_mana_spent_payload", ClassifyAbilityResolvedEventPayload(), "current payload classification"));
        return results;
    }

    private static IReadOnlyDictionary<string, Func<ComplianceState>> RegistryPatternEvaluators()
    {
        return new Dictionary<string, Func<ComplianceState>>(StringComparer.Ordinal)
        {
            ["combat_actor_id_as_xp_identity"] = EvaluateCombatActorIdAsXpIdentity,
            ["live_npc_state_xp_lookup_after_death"] = EvaluateLiveNpcStateXpLookupAfterDeath,
            ["t1_nonrepeatable_firstkill_shipping_rows"] = EvaluateT1NonrepeatableFirstKillShippingRows,
            ["direct_save_read_of_transient_downstream_state"] = EvaluateDirectSaveReadOfTransientDownstreamState,
            ["unbounded_downstream_save_wait"] = EvaluateUnboundedDownstreamSaveWait,
            ["partial_group_payload_serialization"] = EvaluatePartialGroupPayloadSerialization,
            ["generic_all_consumer_progression_baseline_snapshot"] = EvaluateGenericAllConsumerProgressionBaselineSnapshot,
            ["combat_consuming_visible_level"] = EvaluateCombatConsumingVisibleLevel,
            ["ui_or_spell_consumer_reading_combat_snapshot"] = EvaluateUiOrSpellConsumerReadingCombatSnapshot,
            ["consumer_mutating_progression_snapshot"] = EvaluateConsumerMutatingProgressionSnapshot,
            ["save_load_generating_local_character_id"] = EvaluateSaveLoadGeneratingLocalCharacterId,
            ["first_save_seed_only_without_required_materialization"] = EvaluateFirstSaveSeedOnlyWithoutRequiredMaterialization,
            ["first_load_synthesizing_progression_state"] = EvaluateFirstLoadSynthesizingProgressionState,
            ["rematerializing_existing_record_on_load"] = EvaluateRematerializingExistingRecordOnLoad,
            ["local_character_id_derived_from_player_authored_data"] = EvaluateLocalCharacterIdDerivedFromPlayerAuthoredData,
            ["synthetic_fixture_as_pacing_evidence"] = EvaluateSyntheticFixtureAsPacingEvidence,
            ["profiled_pacing_without_preflight"] = EvaluateProfiledPacingWithoutPreflight,
            ["legal_pacing_fixture_without_adr0001_lookup"] = EvaluateLegalPacingFixtureWithoutAdr0001Lookup,
            ["lockout_route_projected_as_repeatable"] = EvaluateLockoutRouteProjectedAsRepeatable,
            ["pacing_fixture_with_ambiguous_kind"] = EvaluatePacingFixtureWithAmbiguousKind
        };
    }

    private static ComplianceState EvaluateCombatActorIdAsXpIdentity()
    {
        var killCreditProperties = typeof(PlayerKillCreditEvent)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .ToArray();

        if (killCreditProperties.Contains("combat_actor_id", StringComparer.Ordinal) ||
            killCreditProperties.Contains("CombatActorId", StringComparer.Ordinal))
        {
            return ComplianceState.Fail;
        }

        return PassIfNoMatches(SourceFiles("src/gameplay/progression", "src/gameplay/npc", "src/core/save"), new[]
        {
            @"\bcombat_actor_id\b",
            @"\bCombatActorId\b"
        });
    }

    private static ComplianceState EvaluateLiveNpcStateXpLookupAfterDeath()
    {
        return PassIfNoMatches(SourceFiles("src/gameplay/progression"), new[]
        {
            @"Gravenspire\.Gameplay\.Npc",
            @"NpcSourceLifecycleService",
            @"NpcSourceLifecycleRecord",
            @"LiveNpc"
        });
    }

    private static ComplianceState EvaluateT1NonrepeatableFirstKillShippingRows()
    {
        return PassIfNoMatches(ProductionSourceAndDataFiles(), new[]
        {
            @"\bNonRepeatableFirstKill\b"
        });
    }

    private static ComplianceState EvaluateDirectSaveReadOfTransientDownstreamState()
    {
        return PassIfNoMatches(SourceFiles("src/core/save"), new[]
        {
            @"CharacterProgressionSaveState",
            @"NpcSourceLifecycleRecord"
        });
    }

    private static ComplianceState EvaluateUnboundedDownstreamSaveWait()
    {
        return PassIfNoMatches(SourceFiles("src/core/save"), new[]
        {
            @"Thread\.Sleep\s*\(",
            @"Task\.Delay\s*\(",
            @"\.Wait\s*\(",
            @"WaitOne\s*\("
        });
    }

    private static ComplianceState EvaluatePartialGroupPayloadSerialization()
    {
        var coordinator = ReadText("src/core/save/GroupedSaveAttemptCoordinator.cs");
        var unresolvedReturnIndex = coordinator.IndexOf("return new SaveAttemptResult(SaveAttemptStatus.Failed", StringComparison.Ordinal);
        var writerIndex = coordinator.IndexOf("writer.Write", StringComparison.Ordinal);
        var groupedBarrierTest = ReadText("tests/integration/core/save/save_grouped_barrier_consistency_test.cs");

        return PassIf(
            unresolvedReturnIndex >= 0 &&
            writerIndex > unresolvedReturnIndex &&
            groupedBarrierTest.Contains("Writer.WriteCount, Is.EqualTo(0)", StringComparison.OrdinalIgnoreCase));
    }

    private static ComplianceState EvaluateGenericAllConsumerProgressionBaselineSnapshot()
    {
        return PassIfNoMatches(SourceFiles("src"), new[]
        {
            @"(?<!Combat)\bProgressionBaselineSnapshot\b"
        });
    }

    private static ComplianceState EvaluateCombatConsumingVisibleLevel()
    {
        return PassIfNoMatches(SourceFiles("src/gameplay/combat"), new[]
        {
            @"\bvisible_level\b",
            @"\bVisibleLevel\b",
            @"\btotal_xp\b",
            @"\bTotalXp\b",
            @"\bspell_eligibility_tier\b",
            @"\bSpellEligibilityTier\b"
        });
    }

    private static ComplianceState EvaluateUiOrSpellConsumerReadingCombatSnapshot()
    {
        return PassIfNoMatches(SourceFiles("src/core/save", "src/gameplay/progression", "src/gameplay/npc"), new[]
        {
            @"\bCombatProgressionBaselineSnapshot\b"
        });
    }

    private static ComplianceState EvaluateConsumerMutatingProgressionSnapshot()
    {
        var snapshotSource = ReadText("src/gameplay/combat/CombatProgressionBaselineSnapshot.cs");
        if (!snapshotSource.Contains("public sealed record CombatProgressionBaselineSnapshot", StringComparison.Ordinal) ||
            snapshotSource.Contains(" set;", StringComparison.Ordinal))
        {
            return ComplianceState.Fail;
        }

        return PassIfNoMatches(SourceFiles("src/core/save", "src/gameplay/progression", "src/gameplay/npc"), new[]
        {
            @"\bCombatProgressionBaselineSnapshot\b"
        });
    }

    private static ComplianceState EvaluateSaveLoadGeneratingLocalCharacterId()
    {
        return PassIfNoMatches(SourceFiles("src/core/save", "src/gameplay/progression"), new[]
        {
            @"Guid\.NewGuid\s*\(",
            @"new\s+Guid\s*\(",
            @"RandomNumberGenerator"
        });
    }

    private static ComplianceState EvaluateFirstSaveSeedOnlyWithoutRequiredMaterialization()
    {
        return PassIfNoWholeFileMatches(
            SourceFiles("src/core/save", "src/gameplay/progression"),
            FirstSaveSeedOnlyForbiddenTerms());
    }

    private static ComplianceState EvaluateFirstLoadSynthesizingProgressionState()
    {
        return PassIfNoMatches(SourceFiles("src/core/save", "src/gameplay/progression"), new[]
        {
            @"(?:first\s+load|continue|load).{0,120}starting_class_id",
            @"starting_class_id.{0,120}(?:first\s+load|continue|load)",
            @"Synthesiz(?:e|ing).*CharacterProgressionSaveState"
        });
    }

    private static ComplianceState EvaluateRematerializingExistingRecordOnLoad()
    {
        return PassIfNoMatches(SourceFiles("src/core/save", "src/gameplay/progression"), new[]
        {
            @"Re[-]?materializ(?:e|ing)"
        });
    }

    private static ComplianceState EvaluateLocalCharacterIdDerivedFromPlayerAuthoredData()
    {
        return PassIfNoMatches(SourceFiles("src/core/save", "src/gameplay/progression"), new[]
        {
            @"local_character_id.*(?:character_name|save_slot|account|device|path|CombatActorId|combat_actor_id)"
        });
    }

    private static ComplianceState EvaluateSyntheticFixtureAsPacingEvidence()
    {
        return PassIfNoMatches(ProductionSourceAndDataFiles(), new[]
        {
            @"\bSyntheticEventTransaction\b",
            @"\bFormulaOnly\b",
            @"\bInvalidDataValidation\b"
        });
    }

    private static ComplianceState EvaluateProfiledPacingWithoutPreflight()
    {
        return PassIfNoMatches(ProductionSourceAndDataFiles(), new[]
        {
            @"ProfiledPacing.*WithoutPreflight",
            @"ProfiledPacingRunSpec.{0,160}(?:SyntheticEventTransaction|FormulaOnly|InvalidDataValidation)",
            @"PacingMathPreflight.*missing"
        });
    }

    private static ComplianceState EvaluateLegalPacingFixtureWithoutAdr0001Lookup()
    {
        return PassIfNoMatches(ProductionSourceAndDataFiles(), new[]
        {
            @"LegalKillCreditRoute.*bypass",
            @"LegalKillCreditRoute.{0,160}(?:missing|without).{0,80}(?:ADR-0001|source lookup|source_ref|lifecycle)"
        });
    }

    private static ComplianceState EvaluateLockoutRouteProjectedAsRepeatable()
    {
        return PassIfNoMatches(ProductionSourceAndDataFiles(), new[]
        {
            @"RespawnLockout.{0,160}(?:continuous|repeatable|XP/hour|xp_per_hour)",
            @"lockout.{0,160}continuous.{0,80}repeatable"
        });
    }

    private static ComplianceState EvaluatePacingFixtureWithAmbiguousKind()
    {
        return PassIfNoWholeFileMatches(ProductionDataFiles(), PacingFixtureKindForbiddenTerms());
    }

    private static ComplianceState EvaluateAdr0006AddendumPattern(string patternId)
    {
        return patternId switch
        {
            "endurance_action_rotation_bar" => PassIfNoMatches(ProductionCombatFiles().Concat(ProductionDataFiles()).ToArray(), new[]
            {
                @"Endurance.{0,80}(?:action[- ]rotation|priority\s+bar|combo\s+meter|GCD|rotation\s+loop)",
                @"(?:action[- ]rotation|priority\s+bar|combo\s+meter|GCD|rotation\s+loop).{0,80}Endurance"
            }),
            "endurance_hud_prominence_above_mana" => PassIfNoMatches(ProductionCombatFiles().Concat(ProductionDataFiles()).ToArray(), new[]
            {
                @"Endurance.{0,80}(?:above|prominen|larger|primary).{0,80}Mana",
                @"Mana.{0,80}(?:below|secondary).{0,80}Endurance"
            }),
            "endurance_pulse_combo_celebratory_treatment" => PassIfNoMatches(ProductionCombatFiles().Concat(ProductionDataFiles()).ToArray(), new[]
            {
                @"Endurance.{0,80}(?:pulse|combo|celebrat|animation)",
                @"(?:pulse|combo|celebrat|animation).{0,80}Endurance"
            }),
            "shipping_per_ability_endurance_callout" => PassIfNoMatches(ProductionCombatFiles().Concat(ProductionDataFiles()).ToArray(), new[]
            {
                @"per[- ]ability\s+Endurance\s+callout",
                @"Endurance(?:Callout|Preview|Popup)"
            }),
            "combat_rotation_fast_endurance_regeneration" => PassIfNoMatches(ProductionCombatFiles().Concat(ProductionDataFiles()).ToArray(), new[]
            {
                @"Endurance.{0,80}(?:fast|rapid).{0,80}regen",
                @"(?:fast|rapid).{0,80}Endurance.{0,80}regen"
            }),
            _ => ComplianceState.Fail
        };
    }

    private static ComplianceState ClassifyAbilityResolvedEventPayload()
    {
        var properties = typeof(AbilityResolvedEvent)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .ToArray();

        if (!properties.Contains("ManaSpent", StringComparer.Ordinal))
        {
            return ComplianceState.Pass;
        }

        var consumers = SourceFiles("src")
            .Where(file => !file.RelativePath.EndsWith("CombatAbilityLifecycleEvents.cs", StringComparison.Ordinal))
            .Where(file => !file.RelativePath.EndsWith("CombatCastLifecycleEvents.cs", StringComparison.Ordinal))
            .Where(file => !file.RelativePath.EndsWith("CombatInstantAbilityResolver.cs", StringComparison.Ordinal))
            .ToArray();
        var consumerMatches = FindMatches(consumers, new[] { @"\.ManaSpent\b" });
        return consumerMatches.Count == 0 &&
            PhysicalResourceSpendIsGuarded() &&
            BashManaSpentCarryoverIsCoveredByTests()
                ? ComplianceState.KnownCarryover
                : ComplianceState.Fail;
    }

    private static void AssertRecordProperties<T>(IReadOnlyList<string> expected)
    {
        var actual = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.That(actual, Is.EqualTo(expected.OrderBy(name => name, StringComparer.Ordinal).ToArray()));
    }

    private static IReadOnlyList<RegistryPattern> LoadForbiddenPatternsFromRegistry()
    {
        var lines = File.ReadAllLines(RepoPath("docs/registry/architecture.yaml"));
        var inForbiddenPatterns = false;
        var patterns = new List<RegistryPatternBuilder>();
        RegistryPatternBuilder? current = null;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line == "forbidden_patterns:")
            {
                inForbiddenPatterns = true;
                continue;
            }

            if (!inForbiddenPatterns || line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            if (line.StartsWith("- pattern:", StringComparison.Ordinal))
            {
                if (current is not null)
                {
                    patterns.Add(current);
                }

                current = new RegistryPatternBuilder
                {
                    Id = line["- pattern:".Length..].Trim()
                };
                continue;
            }

            if (current is null)
            {
                continue;
            }

            if (line.StartsWith("status:", StringComparison.Ordinal))
            {
                current.RegistryStatus = line["status:".Length..].Trim();
            }
            else if (line.StartsWith("adr:", StringComparison.Ordinal))
            {
                current.AdrPath = line["adr:".Length..].Trim();
            }
        }

        if (current is not null)
        {
            patterns.Add(current);
        }

        return patterns
            .Where(pattern => !string.IsNullOrWhiteSpace(pattern.Id))
            .Select(pattern => new RegistryPattern(
                pattern.Id,
                string.IsNullOrWhiteSpace(pattern.RegistryStatus) ? "unknown" : pattern.RegistryStatus,
                string.IsNullOrWhiteSpace(pattern.AdrPath) ? string.Empty : pattern.AdrPath))
            .ToArray();
    }

    private static string ReadAdrStatus(string relativeAdrPath)
    {
        if (string.IsNullOrWhiteSpace(relativeAdrPath))
        {
            return "unknown";
        }

        var lines = File.ReadAllLines(RepoPath(relativeAdrPath));
        for (var index = 0; index < lines.Length; index++)
        {
            if (!string.Equals(lines[index].Trim(), "## Status", StringComparison.Ordinal))
            {
                continue;
            }

            for (var valueIndex = index + 1; valueIndex < lines.Length; valueIndex++)
            {
                var value = lines[valueIndex].Trim();
                if (value.Length > 0)
                {
                    return value;
                }
            }
        }

        return "unknown";
    }

    private static void AssertNoMatches(string label, IReadOnlyList<SourceFile> files, IReadOnlyList<string> patterns)
    {
        var matches = FindMatches(files, patterns);
        Assert.That(matches, Is.Empty, $"{label}:{Environment.NewLine}{FormatMatches(matches)}");
    }

    private static void AssertNoWholeFileMatches(string label, IReadOnlyList<SourceFile> files, IReadOnlyList<string> patterns)
    {
        var matches = FindWholeFileMatches(files, patterns);
        Assert.That(matches, Is.Empty, $"{label}:{Environment.NewLine}{FormatMatches(matches)}");
    }

    private static IReadOnlyList<SourceMatch> FindMatches(IReadOnlyList<SourceFile> files, IReadOnlyList<string> patterns)
    {
        return files
            .SelectMany(file => patterns.SelectMany(pattern => FindMatches(file, pattern)))
            .ToArray();
    }

    private static IEnumerable<SourceMatch> FindMatches(SourceFile file, string pattern)
    {
        var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        var lines = file.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        for (var index = 0; index < lines.Length; index++)
        {
            if (regex.IsMatch(lines[index]))
            {
                yield return new SourceMatch(file.RelativePath, index + 1, pattern, lines[index].Trim());
            }
        }
    }

    private static IReadOnlyList<SourceMatch> FindWholeFileMatches(IReadOnlyList<SourceFile> files, IReadOnlyList<string> patterns)
    {
        return files
            .SelectMany(file => patterns.SelectMany(pattern => FindWholeFileMatches(file, pattern)))
            .ToArray();
    }

    private static IEnumerable<SourceMatch> FindWholeFileMatches(SourceFile file, string pattern)
    {
        var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        foreach (Match match in regex.Matches(file.Text))
        {
            var lineNumber = file.Text.Take(match.Index).Count(character => character == '\n') + 1;
            var lineText = file.Text
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .ElementAtOrDefault(lineNumber - 1)?
                .Trim() ?? string.Empty;
            yield return new SourceMatch(file.RelativePath, lineNumber, pattern, lineText);
        }
    }

    private static bool ContainsAny(string text, IReadOnlyList<string> patterns)
    {
        return patterns.Any(pattern => Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
    }

    private static ComplianceState PassIf(bool condition)
    {
        return condition ? ComplianceState.Pass : ComplianceState.Fail;
    }

    private static ComplianceState PassIfNoMatches(IReadOnlyList<SourceFile> files, IReadOnlyList<string> patterns)
    {
        return FindMatches(files, patterns).Count == 0 ? ComplianceState.Pass : ComplianceState.Fail;
    }

    private static ComplianceState PassIfNoWholeFileMatches(IReadOnlyList<SourceFile> files, IReadOnlyList<string> patterns)
    {
        return FindWholeFileMatches(files, patterns).Count == 0 ? ComplianceState.Pass : ComplianceState.Fail;
    }

    private static string FormatMatches(IReadOnlyList<SourceMatch> matches)
    {
        return string.Join(
            Environment.NewLine,
            matches.Select(match => $"{match.RelativePath}:{match.LineNumber}: {match.LineText} [{match.Pattern}]"));
    }

    private static string FormatComplianceResults(IReadOnlyList<ComplianceResult> results)
    {
        return string.Join(
            Environment.NewLine,
            results.Select(result => $"{result.PatternId}: {result.State} ({result.Detail})"));
    }

    private static string[] T1ScopeForbiddenTerms()
    {
        return new[]
        {
            @"\b" + Literal("Fish", "Net") + @"\b",
            @"\bnetwork" + @"ing?\b",
            @"\breplicat(?:e|ed|ion)\b",
            @"\bserver\s+(?:authority|validation|combat|state)\b",
            @"\baccount\s+identity\b",
            @"\bprediction\b",
            @"\blag\s+compensation\b",
            @"\b" + Literal("Pv", "P") + @"\b",
            @"\bduels?\b",
            @"\bfriendly\s+fire\b",
            @"\bcompan" + @"ions?\b",
            @"\bSister\s+Elara\b",
            @"\b" + Literal("War", "rior") + @"\b",
            @"\b" + Literal("Enchant", "er") + @"\b",
            @"\blive\s+LLM\b"
        };
    }

    private static string[] PacingForbiddenTerms()
    {
        return new[]
        {
            @"\bSyntheticEventTransaction\b",
            @"\bFormulaOnly\b",
            @"\bInvalidDataValidation\b",
            @"\bNonRepeatableFirstKill\b",
            @"ProfiledPacing.*WithoutPreflight",
            @"PacingMathPreflight.*missing",
            @"LegalKillCreditRoute.*bypass",
            @"RespawnLockout.{0,160}(?:continuous|repeatable|XP/hour|xp_per_hour)"
        };
    }

    private static string[] FirstSaveSeedOnlyForbiddenTerms()
    {
        return new[]
        {
            @"FirstSave.{0,240}(?:seed[-_ ]?only|InitialCharacterRecord\s+only|without\s+.*CharacterProgressionSaveState)",
            @"SaveWriteConfirmed.{0,240}(?:without|before).{0,120}(?:FirstSaveMaterialization|CharacterProgressionSaveState)",
            @"FirstSaveMaterialization.{0,160}(?:bypass|skip|disabled|not\s+required)",
            @"CharacterProgressionSaveState.{0,160}(?:synthesiz|materializ).{0,160}(?:first\s+load|continue|load)",
            @"starting_class_id.{0,160}(?:synthesiz|materializ).{0,160}CharacterProgressionSaveState"
        };
    }

    private static string[] PacingFixtureKindForbiddenTerms()
    {
        return new[]
        {
            @"ProgressionPacingFixtureSet_T1[\s\S]{0,400}(?:[""']?fixture_kind[""']?\s*[:=]\s*(?:null|[""']{2})|fixture_kind_missing|missing_fixture_kind|ambiguous_fixture_kind)",
            @"ProgressionPacingFixtureSet_T1[\s\S]{0,400}[""']?fixture_kind[""']?\s*[:=]\s*[""']?(?:LegalKillCreditRoute|FormulaOnly|SyntheticEventTransaction|InvalidDataValidation)[""']?[\s\S]{0,160}[""']?fixture_kind[""']?\s*[:=]",
            @"ProgressionPacingFixtureSet_T1[\s\S]{0,400}(?:LegalKillCreditRoute\s*\|\s*FormulaOnly|SyntheticEventTransaction\s*\|\s*InvalidDataValidation)"
        };
    }

    private static string[] QuietEnduranceForbiddenTerms()
    {
        return new[]
        {
            @"Endurance.{0,80}(?:action[- ]rotation|priority\s+bar|combo\s+meter|GCD|rotation\s+loop)",
            @"(?:action[- ]rotation|priority\s+bar|combo\s+meter|GCD|rotation\s+loop).{0,80}Endurance",
            @"Endurance.{0,80}(?:above|prominen|larger|primary).{0,80}Mana",
            @"Mana.{0,80}(?:below|secondary).{0,80}Endurance",
            @"Endurance.{0,80}(?:pulse|combo|celebrat|animation)",
            @"(?:pulse|combo|celebrat|animation).{0,80}Endurance",
            @"per[- ]ability\s+Endurance\s+callout",
            @"Endurance(?:Callout|Preview|Popup)",
            @"Endurance.{0,80}(?:fast|rapid).{0,80}regen",
            @"(?:fast|rapid).{0,80}Endurance.{0,80}regen"
        };
    }

    private static IReadOnlyList<SourceFile> FailureFixtureSamples()
    {
        return new[]
        {
            new SourceFile(
                "tests/architecture/fixtures/t1_scope.txt",
                Literal("Fish", "Net") + " replicated server combat authority with " + Literal("Pv", "P") + " prediction"),
            new SourceFile(
                "tests/architecture/fixtures/combat_actor_id_identity.txt",
                "XpAwardDedupeKey(local_character_id, zoneId, combat_actor_id, token)"),
            new SourceFile(
                "tests/architecture/fixtures/progression_snapshot.txt",
                "ProgressionBaselineSnapshot(visible_level, spell_eligibility_tier, total_xp)"),
            new SourceFile(
                "tests/architecture/fixtures/quiet_endurance.txt",
                "Endurance combo meter with per-ability Endurance callout"),
            new SourceFile(
                "tests/architecture/fixtures/pacing.txt",
                "SyntheticEventTransaction used as XP/hour evidence"),
            new SourceFile(
                "tests/architecture/fixtures/first_save_seed_only.txt",
                "FirstSave writes InitialCharacterRecord only without CharacterProgressionSaveState"),
            new SourceFile(
                "tests/architecture/fixtures/ambiguous_fixture_kind.json",
                """
                {
                  "schema": "ProgressionPacingFixtureSet_T1",
                  "rows": [
                    { "id": "bad", "fixture_kind": "LegalKillCreditRoute", "fixture_kind": "FormulaOnly" }
                  ]
                }
                """)
        };
    }

    private static bool PhysicalResourceSpendIsGuarded()
    {
        var resolver = ReadText("src/gameplay/combat/abilities/CombatInstantAbilityResolver.cs");
        return resolver.Contains("request.Profile.ResourceKind == CombatTacticalAbilityResourceKind.Physical", StringComparison.Ordinal) &&
            resolver.Contains("request.Caster.CurrentEndurance < request.Profile.CostEndurance", StringComparison.Ordinal) &&
            resolver.Contains("profile.ResourceKind == CombatTacticalAbilityResourceKind.Physical", StringComparison.Ordinal) &&
            resolver.Contains("caster.WithCurrentEndurance(caster.CurrentEndurance - profile.CostEndurance)", StringComparison.Ordinal);
    }

    private static bool BashManaSpentCarryoverIsCoveredByTests()
    {
        var instantTests = ReadText("tests/integration/gameplay/combat/combat_tactical_cleric_instants_test.cs");
        return instantTests.Contains("bash.ResourceKind, Is.EqualTo(CombatTacticalAbilityResourceKind.Physical)", StringComparison.Ordinal) &&
            instantTests.Contains("result.AbilityEvents.OfType<AbilityResolvedEvent>().Single().ManaSpent, Is.EqualTo(0)", StringComparison.Ordinal);
    }

    private static string Literal(params string[] parts)
    {
        return string.Concat(parts);
    }

    private static IReadOnlyList<SourceFile> ProductionCombatFiles()
    {
        return SourceFiles("src/gameplay/combat");
    }

    private static IReadOnlyList<SourceFile> ProductionSourceAndDataFiles()
    {
        return SourceFiles("src")
            .Concat(ProductionDataFiles())
            .ToArray();
    }

    private static IReadOnlyList<SourceFile> ProductionDataFiles()
    {
        var dataRoot = RepoPath("assets/data");
        if (!Directory.Exists(dataRoot))
        {
            return Array.Empty<SourceFile>();
        }

        return Directory.GetFiles(dataRoot, "*.json", SearchOption.AllDirectories)
            .Select(ReadSourceFile)
            .ToArray();
    }

    private static IReadOnlyList<SourceFile> SourceFiles(params string[] relativeRoots)
    {
        return relativeRoots
            .Select(RepoPath)
            .Where(Directory.Exists)
            .SelectMany(root => Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Select(ReadSourceFile)
            .ToArray();
    }

    private static SourceFile ReadSourceFile(string fullPath)
    {
        return new SourceFile(Path.GetRelativePath(FindRepoRoot(), fullPath).Replace('\\', '/'), File.ReadAllText(fullPath));
    }

    private static string ReadText(string relativePath)
    {
        return File.ReadAllText(RepoPath(relativePath));
    }

    private static string RepoPath(string relativePath)
    {
        return Path.Combine(FindRepoRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static string FindRepoRoot()
    {
        var candidates = new[]
        {
            new DirectoryInfo(TestContext.CurrentContext.TestDirectory),
            new DirectoryInfo(Directory.GetCurrentDirectory())
        };

        foreach (var candidate in candidates)
        {
            for (var directory = candidate; directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")) &&
                    Directory.Exists(Path.Combine(directory.FullName, "src")) &&
                    Directory.Exists(Path.Combine(directory.FullName, "tests")))
                {
                    return directory.FullName;
                }
            }
        }

        throw new DirectoryNotFoundException("Unable to locate repository root for architecture compliance tests.");
    }

    private sealed record SourceFile(string RelativePath, string Text);

    private sealed record SourceMatch(string RelativePath, int LineNumber, string Pattern, string LineText);

    private sealed record RegistryPattern(string Id, string RegistryStatus, string AdrPath);

    private sealed record ComplianceResult(string PatternId, ComplianceState State, string Detail);

    private sealed class RegistryPatternBuilder
    {
        public string Id { get; init; } = string.Empty;

        public string RegistryStatus { get; set; } = string.Empty;

        public string AdrPath { get; set; } = string.Empty;
    }

    private enum ComplianceState
    {
        Pass,
        KnownCarryover,
        Fail
    }
}
