#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Gravenspire.Gameplay.Combat;

namespace Gravenspire.Gameplay.Combat.Fixtures;

/// <summary>
/// Result of validating the T1 Combat Core fixture package.
/// </summary>
public sealed record CombatFixtureValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    /// <summary>
    /// Creates a passing validation result.
    /// </summary>
    public static CombatFixtureValidationResult Valid { get; } = new(true, Array.Empty<string>());

    /// <summary>
    /// Creates a failing validation result.
    /// </summary>
    public static CombatFixtureValidationResult Invalid(IEnumerable<string> errors)
    {
        return new CombatFixtureValidationResult(false, errors.ToArray());
    }
}

/// <summary>
/// Validates fixture completeness and generic safe ranges without embedding production tuning values.
/// </summary>
public sealed class CombatFixtureValidator
{
    private static readonly string[] RequiredActorFixtures =
    {
        "Cleric_Low_T1",
        "Cleric_Mid_T1",
        "Cleric_Top_T1",
        "Trash_Low_T1",
        "Trash_Mid_T1",
        "Trash_Top_T1",
        "Named_Top_T1"
    };

    private static readonly string[] RequiredSpellFixtures =
    {
        "Smite_T1_Prototype",
        "LesserHeal_T1_Prototype"
    };

    private static readonly string[] RequiredTacticalInstantFixtures =
    {
        "SmiteOfAuthority_T1_Prototype",
        "Bash_T1_Prototype",
        "DefensivePrayer_T1_Prototype"
    };

    private static readonly string[] RequiredTacticalInstantAbilityProfiles =
    {
        "SmiteOfAuthority_T1_Prototype",
        "Bash_T1_Prototype",
        "DefensivePrayer_T1_Prototype"
    };

    private static readonly string[] RequiredEncounterFixtures =
    {
        "SoloTrash_EvenCon_T1",
        "TwoTrash_Overpull_T1",
        "NamedSoloBlock_T1"
    };

    /// <summary>
    /// Validates the T1 combat fixture package.
    /// </summary>
    public CombatFixtureValidationResult Validate(CombatFixturePackage? package)
    {
        if (package is null)
        {
            return CombatFixtureValidationResult.Invalid(new[] { "Combat fixture package is required." });
        }

        var errors = new List<string>();

        RequireText(package.FixtureSetId, "fixture_set_id", errors);
        RequireText(package.FixtureSetVersion, "fixture_set_version", errors);
        RequireText(package.SourceDocument, "source_document", errors);
        RequireText(package.PrototypeHauntBand, "prototype_haunt_band", errors);

        if (package.CombatTickRateHz <= 0)
        {
            errors.Add("combat_tick_rate_hz must be positive.");
        }

        ValidateTargetingTuning(package.TargetingTuning, errors);
        ValidatePullTuning(package.PullTuning, errors);
        ValidateLeashTuning(package.LeashTuning, errors);
        ValidateSocialAssistProfiles(package.SocialAssistProfiles, package.PullTuning, errors);
        ValidateActors(package.ActorFixtures, errors);
        ValidateSpellRows(package.SpellFixtures, "spell", requireZeroCast: false, errors);
        ValidateSpellRows(package.TacticalInstantFixtures, "tactical instant", requireZeroCast: true, errors);
        ValidateTacticalInstantAbilityProfiles(package.TacticalInstantAbilityProfiles, errors);
        ValidateEncounters(package.EncounterFixtures, package.ActorFixtures, errors);
        ValidateRegenAndCombatExitTuning(package.RegenAndCombatExitTuning, errors);

        RequireIds(package.ActorFixtures.Select(actor => actor.Id), RequiredActorFixtures, "actor fixture", errors);
        RequireIds(package.SpellFixtures.Select(spell => spell.Id), RequiredSpellFixtures, "spell fixture", errors);
        RequireIds(package.TacticalInstantFixtures.Select(spell => spell.Id), RequiredTacticalInstantFixtures, "tactical instant fixture", errors);
        RequireIds(package.TacticalInstantAbilityProfiles.Select(profile => profile.Id), RequiredTacticalInstantAbilityProfiles, "tactical instant ability profile", errors);
        RequireIds(package.EncounterFixtures.Select(encounter => encounter.Id), RequiredEncounterFixtures, "encounter fixture", errors);

        return errors.Count == 0 ? CombatFixtureValidationResult.Valid : CombatFixtureValidationResult.Invalid(errors);
    }

    private static void ValidateTargetingTuning(CombatTargetingTuningFixture tuning, ICollection<string> errors)
    {
        if (tuning.TargetAcquireRadiusMeters <= 0)
        {
            errors.Add("target_acquire_radius_meters must be positive.");
        }

        if (tuning.CombatQueryBufferSize != 64)
        {
            errors.Add("combat_query_buffer_size must be 64 for the approved T1 query contract.");
        }

        var blockers = new HashSet<CombatLosLayer>(tuning.LosOccluderLayerMaskT1);
        if (!blockers.SetEquals(T1CombatLineOfSight.BlockingLayers))
        {
            errors.Add("los_occluder_layer_mask_t1 must contain exactly WorldSolid, ClosedDoor, and LargeProp.");
        }

        foreach (var layer in tuning.NonBlockingLayersT1)
        {
            if (T1CombatLineOfSight.BlocksLineOfSight(layer))
            {
                errors.Add($"{layer}: non-blocking LoS layer cannot also be in los_occluder_layer_mask_t1.");
            }
        }
    }

    private static void ValidatePullTuning(CombatPullTuningFixture tuning, ICollection<string> errors)
    {
        if (tuning.ProximityThreatInitial <= 0)
        {
            errors.Add("proximity_threat_initial must be positive.");
        }

        if (tuning.SocialAssistPulseSeconds <= 0)
        {
            errors.Add("social_assist_pulse_seconds must be positive.");
        }

        if (tuning.SocialAssistRadiusMeters <= 0)
        {
            errors.Add("social_assist_radius_meters must be positive.");
        }

        if (tuning.AssistThreatInitial <= 0)
        {
            errors.Add("assist_threat_initial must be positive.");
        }
    }

    private static void ValidateLeashTuning(CombatLeashTuningFixture tuning, ICollection<string> errors)
    {
        if (tuning.LeashDistanceMeters <= 0 ||
            tuning.PathFailureGraceSeconds <= 0 ||
            tuning.PathPendingGraceSeconds <= 0 ||
            tuning.PathStatusSampleSeconds <= 0 ||
            tuning.LeashThreatMemorySeconds <= 0 ||
            tuning.LeashReAggroDistanceMeters <= 0)
        {
            errors.Add("leash tuning values must be positive.");
        }
    }

    private static void ValidateSocialAssistProfiles(
        IEnumerable<CombatSocialAssistProfileFixture> profiles,
        CombatPullTuningFixture pullTuning,
        ICollection<string> errors)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var hasDefault = false;

        foreach (var profile in profiles)
        {
            RequireUniqueId(profile.Id, "social assist profile", seen, errors);
            RequireText(profile.SocialLinkGroupId, $"{profile.Id}.social_link_group_id", errors);
            RequireText(profile.AssistFactionFilter, $"{profile.Id}.assist_faction_filter", errors);
            RequireText(profile.AssistEncounterFilter, $"{profile.Id}.assist_encounter_filter", errors);

            if (profile.AssistRadiusMeters <= 0)
            {
                errors.Add($"{profile.Id}: assist_radius_meters must be positive.");
            }

            if (profile.AssistThreatInitial <= 0)
            {
                errors.Add($"{profile.Id}: assist_threat_initial must be positive.");
            }

            if (profile.AssistOrderIndex < 0)
            {
                errors.Add($"{profile.Id}: assist_order_index must not be negative.");
            }

            if (string.Equals(profile.Id, "VampireCourt_T1_DefaultSocial", StringComparison.Ordinal))
            {
                hasDefault = true;
                if (profile.AssistThreatInitial != pullTuning.AssistThreatInitial ||
                    Math.Abs(profile.AssistRadiusMeters - pullTuning.SocialAssistRadiusMeters) > 0.000001d)
                {
                    errors.Add("VampireCourt_T1_DefaultSocial must mirror default social assist radius and threat tuning.");
                }
            }
        }

        if (!hasDefault)
        {
            errors.Add("Missing required social assist profile: VampireCourt_T1_DefaultSocial.");
        }
    }

    private static void ValidateActors(IEnumerable<CombatActorFixture> actors, ICollection<string> errors)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var actor in actors)
        {
            RequireUniqueId(actor.Id, "actor fixture", seen, errors);

            if (actor.Level is < 1 or > 10)
            {
                errors.Add($"{actor.Id}: level must be inside the T1 fixture band.");
            }

            if (actor.MaxHealth <= 0)
            {
                errors.Add($"{actor.Id}: max_health must be positive.");
            }

            if (actor.MaxMana < 0)
            {
                errors.Add($"{actor.Id}: max_mana must not be negative.");
            }

            if (actor.ActorKind == CombatActorKind.Player && string.IsNullOrWhiteSpace(actor.ClassId))
            {
                errors.Add($"{actor.Id}: player fixtures require class_id.");
            }

            if (actor.ActorKind == CombatActorKind.NPC && string.IsNullOrWhiteSpace(actor.EncounterRole))
            {
                errors.Add($"{actor.Id}: hostile fixtures require encounter_role.");
            }

            if (actor.ArmorClass < 0 || actor.AttackPower < 0 || actor.WeaponBaseDamage < 0 || actor.AttackSkill < 0 || actor.DefenseSkill < 0)
            {
                errors.Add($"{actor.Id}: combat stats must not be negative.");
            }

            if (actor.WeaponDelaySeconds <= 0 || actor.MeleeRangeMeters <= 0 || actor.SpellRangeMeters < 0)
            {
                errors.Add($"{actor.Id}: delay and range values must be valid.");
            }

            if (actor.StableSourceAliases.Count == 0)
            {
                errors.Add($"{actor.Id}: at least one stable source alias is required.");
            }
        }
    }

    private static void ValidateSpellRows(
        IEnumerable<CombatSpellFixture> spells,
        string label,
        bool requireZeroCast,
        ICollection<string> errors)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var spell in spells)
        {
            RequireUniqueId(spell.Id, label, seen, errors);
            RequireText(spell.FixtureKind, $"{spell.Id}.fixture_kind", errors);
            RequireText(spell.EffectType, $"{spell.Id}.effect_type", errors);

            if (spell.CastTimeSeconds < 0 || spell.RecoverySeconds < 0)
            {
                errors.Add($"{spell.Id}: cast and recovery seconds must not be negative.");
            }

            if (requireZeroCast && spell.CastTimeSeconds != 0)
            {
                errors.Add($"{spell.Id}: tactical instant fixtures must declare zero cast time.");
            }

            ValidateBandValues(spell.Id, "mana cost", spell.ManaCostByBand, errors);
            ValidateBandValues(spell.Id, "effect value", spell.EffectValueByBand, errors);

            if (spell.CooldownSeconds is < 0 || spell.DurationSeconds is < 0 || spell.InterruptSeconds is < 0)
            {
                errors.Add($"{spell.Id}: optional timing values must not be negative.");
            }

            if (spell.DamageReduction is < 0 or > 1)
            {
                errors.Add($"{spell.Id}: damage reduction must be a ratio from zero to one.");
            }
        }
    }

    private static void ValidateTacticalInstantAbilityProfiles(
        IEnumerable<CombatTacticalInstantAbilityProfileFixture> profiles,
        ICollection<string> errors)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var profile in profiles)
        {
            RequireUniqueId(profile.Id, "tactical instant ability profile", seen, errors);

            if (profile.CastTimeSeconds != 0)
            {
                errors.Add($"{profile.Id}: tactical instant ability profile must declare zero cast time.");
            }

            if (profile.CostMana <= 0)
            {
                errors.Add($"{profile.Id}: cost_mana must be positive.");
            }

            if (profile.CooldownSeconds <= 0)
            {
                errors.Add($"{profile.Id}: cooldown_seconds must be positive.");
            }

            if (profile.RangeMeters < 0)
            {
                errors.Add($"{profile.Id}: range_meters must not be negative.");
            }

            if (profile.RequiresTarget && profile.RangeMeters <= 0)
            {
                errors.Add($"{profile.Id}: targeted tactical instant ability profiles require positive range_meters.");
            }

            if (profile.Effects.Count == 0)
            {
                errors.Add($"{profile.Id}: at least one declared ability effect is required.");
            }

            foreach (var effect in profile.Effects)
            {
                ValidateTacticalInstantAbilityEffect(profile.Id, effect, errors);
            }
        }
    }

    private static void ValidateTacticalInstantAbilityEffect(
        string profileId,
        CombatTacticalInstantAbilityEffectFixture effect,
        ICollection<string> errors)
    {
        RequireText(effect.EffectType, $"{profileId}.effect_type", errors);

        switch (effect.EffectType)
        {
            case CombatTacticalAbilityEffectTypes.DirectDamage:
                ValidateBandValues(profileId, "direct damage", effect.DamageByBand, errors);
                if (effect.DamageByBand.Any(value => value.Value <= 0))
                {
                    errors.Add($"{profileId}: direct_damage values must be positive.");
                }

                break;
            case CombatTacticalAbilityEffectTypes.SelfBuff:
                if (effect.DurationSeconds is null or <= 0)
                {
                    errors.Add($"{profileId}: self_buff requires positive duration_seconds.");
                }

                if (effect.DamageReduction is null or < 0 or > 1)
                {
                    errors.Add($"{profileId}: self_buff damage_reduction must be a ratio from zero to one.");
                }

                break;
            case CombatTacticalAbilityEffectTypes.InterruptCurrentChannel:
                if (effect.InterruptSeconds is null or <= 0)
                {
                    errors.Add($"{profileId}: interrupt_current_channel requires positive interrupt_seconds.");
                }

                break;
            default:
                errors.Add($"{profileId}: unsupported tactical instant ability effect_type {effect.EffectType}.");
                break;
        }
    }

    private static void ValidateRegenAndCombatExitTuning(
        CombatRegenAndCombatExitTuning tuning,
        ICollection<string> errors)
    {
        if (tuning.RegenTickIntervalSeconds <= 0)
        {
            errors.Add("regen_tick_interval_seconds must be positive.");
        }

        if (tuning.CombatExitTimerSeconds <= 0)
        {
            errors.Add("combat_exit_timer_seconds must be positive.");
        }

        if (tuning.SittingThreatBonus < 0)
        {
            errors.Add("sitting_threat_bonus must not be negative.");
        }

        ValidateResourceRegenTuning("health_regen", tuning.HealthRegen, errors);
        ValidateResourceRegenTuning("mana_regen", tuning.ManaRegen, errors);
    }

    private static void ValidateResourceRegenTuning(
        string label,
        CombatResourceRegenTuning tuning,
        ICollection<string> errors)
    {
        if (tuning.BaseRegen < 0 ||
            tuning.LevelRegenScalar < 0 ||
            tuning.PercentRegenScalar < 0 ||
            tuning.StandingPostureMultiplier <= 0 ||
            tuning.SittingPostureMultiplier <= 0 ||
            tuning.OutOfCombatMultiplier < 0 ||
            tuning.InCombatMultiplier < 0)
        {
            errors.Add($"{label} values must be non-negative, and posture multipliers must be positive.");
        }
    }

    private static void ValidateEncounters(
        IEnumerable<CombatEncounterFixture> encounters,
        IEnumerable<CombatActorFixture> actors,
        ICollection<string> errors)
    {
        var actorIds = new HashSet<string>(actors.Select(actor => actor.Id), StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var encounter in encounters)
        {
            RequireUniqueId(encounter.Id, "encounter fixture", seen, errors);

            if (encounter.ActorFixtureIds.Count == 0)
            {
                errors.Add($"{encounter.Id}: at least one actor fixture id is required.");
            }

            foreach (var actorFixtureId in encounter.ActorFixtureIds)
            {
                if (!actorIds.Contains(actorFixtureId))
                {
                    errors.Add($"{encounter.Id}: unknown actor fixture id {actorFixtureId}.");
                }
            }

            if (encounter.KillWeightSeed <= 0)
            {
                errors.Add($"{encounter.Id}: kill_weight_seed must be positive.");
            }

            if (encounter.SourceRefAliases.Count == 0)
            {
                errors.Add($"{encounter.Id}: source-ref aliases are required.");
            }

            RequireText(encounter.RequiredOutcome, $"{encounter.Id}.required_outcome", errors);
        }
    }

    private static void ValidateBandValues(
        string fixtureId,
        string label,
        IReadOnlyCollection<CombatBandValue> values,
        ICollection<string> errors)
    {
        if (values.Count == 0)
        {
            errors.Add($"{fixtureId}: {label} values are required.");
            return;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in values)
        {
            RequireUniqueId(value.Band, $"{fixtureId}.{label}.band", seen, errors);
            if (value.Value < 0)
            {
                errors.Add($"{fixtureId}: {label} values must not be negative.");
            }
        }
    }

    private static void RequireIds(
        IEnumerable<string> actualIds,
        IEnumerable<string> requiredIds,
        string label,
        ICollection<string> errors)
    {
        var actual = new HashSet<string>(actualIds, StringComparer.Ordinal);
        foreach (var requiredId in requiredIds)
        {
            if (!actual.Contains(requiredId))
            {
                errors.Add($"Missing required {label}: {requiredId}.");
            }
        }
    }

    private static void RequireUniqueId(
        string id,
        string label,
        ISet<string> seen,
        ICollection<string> errors)
    {
        RequireText(id, label, errors);
        if (!string.IsNullOrWhiteSpace(id) && !seen.Add(id))
        {
            errors.Add($"Duplicate {label} id: {id}.");
        }
    }

    private static void RequireText(string? value, string fieldName, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{fieldName} is required.");
        }
    }
}
