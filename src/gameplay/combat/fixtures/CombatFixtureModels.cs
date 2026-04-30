#nullable enable

using System.Collections.Generic;
using Gravenspire.Gameplay.Combat;

namespace Gravenspire.Gameplay.Combat.Fixtures;

/// <summary>
/// Root fixture package for T1 Combat Core actor, spell, tactical instant, and encounter data.
/// </summary>
public sealed record CombatFixturePackage
{
    /// <summary>
    /// Stable package id.
    /// </summary>
    public string FixtureSetId { get; init; } = string.Empty;

    /// <summary>
    /// Versioned fixture-set id for evidence and test logs.
    /// </summary>
    public string FixtureSetVersion { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable source documents for this fixture package.
    /// </summary>
    public string SourceDocument { get; init; } = string.Empty;

    /// <summary>
    /// Prototype haunt band currently mapped by these fixture rows.
    /// </summary>
    public string PrototypeHauntBand { get; init; } = string.Empty;

    /// <summary>
    /// Combat tick rate loaded from fixture/config data.
    /// </summary>
    public int CombatTickRateHz { get; init; }

    /// <summary>
    /// T1 targeting and line-of-sight query tuning.
    /// </summary>
    public CombatTargetingTuningFixture TargetingTuning { get; init; } = new();

    /// <summary>
    /// T1 body-pull and social-assist tuning.
    /// </summary>
    public CombatPullTuningFixture PullTuning { get; init; } = new();

    /// <summary>
    /// T1 leash and path-probe tuning.
    /// </summary>
    public CombatLeashTuningFixture LeashTuning { get; init; } = new();

    /// <summary>
    /// Social assist profiles available to hostile placement fixtures.
    /// </summary>
    public List<CombatSocialAssistProfileFixture> SocialAssistProfiles { get; init; } = new();

    /// <summary>
    /// Actor fixtures available for T1 Combat Core tests.
    /// </summary>
    public List<CombatActorFixture> ActorFixtures { get; init; } = new();

    /// <summary>
    /// Slow spell fixtures available for T1 Combat Core tests.
    /// </summary>
    public List<CombatSpellFixture> SpellFixtures { get; init; } = new();

    /// <summary>
    /// D012 tactical instant fixtures available for T1 Combat Core tests.
    /// </summary>
    public List<CombatSpellFixture> TacticalInstantFixtures { get; init; } = new();

    /// <summary>
    /// Strict tactical instant ability profiles used by the T1 execution path.
    /// </summary>
    public List<CombatTacticalInstantAbilityProfileFixture> TacticalInstantAbilityProfiles { get; init; } = new();

    /// <summary>
    /// Encounter fixtures available for T1 Combat Core tests.
    /// </summary>
    public List<CombatEncounterFixture> EncounterFixtures { get; init; } = new();

    /// <summary>
    /// Fixture-owned med-break, regen, unsafe-sit threat, and combat-exit tuning.
    /// </summary>
    public CombatRegenAndCombatExitTuning RegenAndCombatExitTuning { get; init; } = new();
}

/// <summary>
/// Actor fixture row for player and hostile combat actor construction.
/// </summary>
public sealed record CombatActorFixture
{
    /// <summary>
    /// Stable fixture id.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Runtime actor kind represented by this fixture.
    /// </summary>
    public CombatActorKind ActorKind { get; init; }

    /// <summary>
    /// Optional class id for player fixtures.
    /// </summary>
    public string? ClassId { get; init; }

    /// <summary>
    /// Encounter role for hostile fixture classification.
    /// </summary>
    public string? EncounterRole { get; init; }

    /// <summary>
    /// Fixture level.
    /// </summary>
    public int Level { get; init; }

    /// <summary>
    /// Fixture maximum health.
    /// </summary>
    public int MaxHealth { get; init; }

    /// <summary>
    /// Fixture maximum mana.
    /// </summary>
    public int MaxMana { get; init; }

    /// <summary>
    /// Fixture armor class.
    /// </summary>
    public int ArmorClass { get; init; }

    /// <summary>
    /// Fixture attack power.
    /// </summary>
    public int AttackPower { get; init; }

    /// <summary>
    /// Fixture weapon or natural attack base damage.
    /// </summary>
    public int WeaponBaseDamage { get; init; }

    /// <summary>
    /// Fixture attack skill.
    /// </summary>
    public int AttackSkill { get; init; }

    /// <summary>
    /// Fixture defense skill.
    /// </summary>
    public int DefenseSkill { get; init; }

    /// <summary>
    /// Fixture weapon delay in seconds.
    /// </summary>
    public double WeaponDelaySeconds { get; init; }

    /// <summary>
    /// Fixture melee range in meters.
    /// </summary>
    public double MeleeRangeMeters { get; init; }

    /// <summary>
    /// Fixture spell range in meters.
    /// </summary>
    public double SpellRangeMeters { get; init; }

    /// <summary>
    /// Optional faction id used by downstream fixtures.
    /// </summary>
    public string? FactionId { get; init; }

    /// <summary>
    /// Stable source aliases available to downstream fixtures.
    /// </summary>
    public List<string> StableSourceAliases { get; init; } = new();

    /// <summary>
    /// Optional named-enemy solo block profile id.
    /// </summary>
    public string? SoloBlockProfileId { get; init; }
}

/// <summary>
/// Targeting and line-of-sight fixture tuning.
/// </summary>
public sealed record CombatTargetingTuningFixture
{
    /// <summary>
    /// Maximum target acquisition radius in meters.
    /// </summary>
    public double TargetAcquireRadiusMeters { get; init; }

    /// <summary>
    /// Non-alloc query buffer size expected by T1 combat queries.
    /// </summary>
    public int CombatQueryBufferSize { get; init; }

    /// <summary>
    /// Layers that block T1 combat line of sight.
    /// </summary>
    public List<CombatLosLayer> LosOccluderLayerMaskT1 { get; init; } = new();

    /// <summary>
    /// Layers explicitly documented as non-blocking for T1 combat line of sight.
    /// </summary>
    public List<CombatLosLayer> NonBlockingLayersT1 { get; init; } = new();
}

/// <summary>
/// Body-pull and social-assist fixture tuning.
/// </summary>
public sealed record CombatPullTuningFixture
{
    /// <summary>
    /// Threat assigned by a proximity/body pull.
    /// </summary>
    public int ProximityThreatInitial { get; init; }

    /// <summary>
    /// Social assist pulse interval in seconds.
    /// </summary>
    public double SocialAssistPulseSeconds { get; init; }

    /// <summary>
    /// Default social assist radius in meters.
    /// </summary>
    public double SocialAssistRadiusMeters { get; init; }

    /// <summary>
    /// Default threat assigned to assisting hostiles.
    /// </summary>
    public int AssistThreatInitial { get; init; }
}

/// <summary>
/// Leash and path-probe fixture tuning.
/// </summary>
public sealed record CombatLeashTuningFixture
{
    /// <summary>
    /// Distance from anchor that forces leashing.
    /// </summary>
    public double LeashDistanceMeters { get; init; }

    /// <summary>
    /// Continuous path failure grace period.
    /// </summary>
    public double PathFailureGraceSeconds { get; init; }

    /// <summary>
    /// Continuous path-pending grace period.
    /// </summary>
    public double PathPendingGraceSeconds { get; init; }

    /// <summary>
    /// Expected minimum path-status sample cadence.
    /// </summary>
    public double PathStatusSampleSeconds { get; init; }

    /// <summary>
    /// Threat memory duration while leashing.
    /// </summary>
    public double LeashThreatMemorySeconds { get; init; }

    /// <summary>
    /// Distance in which a leashing hostile may re-aggro before anchor return or expiry.
    /// </summary>
    public double LeashReAggroDistanceMeters { get; init; }
}

/// <summary>
/// Authored social assist profile fixture.
/// </summary>
public sealed record CombatSocialAssistProfileFixture
{
    /// <summary>
    /// Stable profile id.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Social-link group shared by eligible hostiles.
    /// </summary>
    public string SocialLinkGroupId { get; init; } = string.Empty;

    /// <summary>
    /// Optional encounter group id.
    /// </summary>
    public string? EncounterGroupId { get; init; }

    /// <summary>
    /// True when assist is enabled.
    /// </summary>
    public bool AssistEnabled { get; init; }

    /// <summary>
    /// Assist radius in meters.
    /// </summary>
    public double AssistRadiusMeters { get; init; }

    /// <summary>
    /// Initial assist threat.
    /// </summary>
    public int AssistThreatInitial { get; init; }

    /// <summary>
    /// True when candidate must have LoS to the primary hostile.
    /// </summary>
    public bool AssistRequiresLosToPrimary { get; init; }

    /// <summary>
    /// True when candidate must have LoS to the player target point.
    /// </summary>
    public bool AssistRequiresLosToTarget { get; init; }

    /// <summary>
    /// Faction filter id.
    /// </summary>
    public string AssistFactionFilter { get; init; } = string.Empty;

    /// <summary>
    /// Encounter filter id.
    /// </summary>
    public string AssistEncounterFilter { get; init; } = string.Empty;

    /// <summary>
    /// Authored assist ordering tie-breaker.
    /// </summary>
    public int AssistOrderIndex { get; init; }
}

/// <summary>
/// Low, mid, or top fixture value for a spell or tactical instant profile.
/// </summary>
public sealed record CombatBandValue
{
    /// <summary>
    /// Fixture band label.
    /// </summary>
    public string Band { get; init; } = string.Empty;

    /// <summary>
    /// Value for the fixture band.
    /// </summary>
    public int Value { get; init; }
}

/// <summary>
/// Spell or tactical instant profile row.
/// </summary>
public sealed record CombatSpellFixture
{
    /// <summary>
    /// Stable fixture id.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Fixture kind, such as slow_spell or tactical_instant.
    /// </summary>
    public string FixtureKind { get; init; } = string.Empty;

    /// <summary>
    /// Cast time in seconds.
    /// </summary>
    public double CastTimeSeconds { get; init; }

    /// <summary>
    /// Recovery time in seconds.
    /// </summary>
    public double RecoverySeconds { get; init; }

    /// <summary>
    /// Effect type declared by fixture data.
    /// </summary>
    public string EffectType { get; init; } = string.Empty;

    /// <summary>
    /// Mana cost values by fixture band.
    /// </summary>
    public List<CombatBandValue> ManaCostByBand { get; init; } = new();

    /// <summary>
    /// Primary effect values by fixture band.
    /// </summary>
    public List<CombatBandValue> EffectValueByBand { get; init; } = new();

    /// <summary>
    /// Optional fixture-owned cooldown in seconds.
    /// </summary>
    public double? CooldownSeconds { get; init; }

    /// <summary>
    /// Optional fixture-owned duration in seconds.
    /// </summary>
    public double? DurationSeconds { get; init; }

    /// <summary>
    /// Optional fixture-owned interrupt pressure in seconds.
    /// </summary>
    public double? InterruptSeconds { get; init; }

    /// <summary>
    /// Optional fixture-owned damage reduction ratio.
    /// </summary>
    public double? DamageReduction { get; init; }
}

/// <summary>
/// Executable zero-cast-time tactical instant ability profile row.
/// </summary>
public sealed record CombatTacticalInstantAbilityProfileFixture
{
    /// <summary>
    /// Stable ability profile id.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Cast-time contract; tactical instant profiles must declare zero.
    /// </summary>
    public double CastTimeSeconds { get; init; }

    /// <summary>
    /// Fixture-owned mana cost.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("cost_mana")]
    public int CostMana { get; init; }

    /// <summary>
    /// Fixture-owned transient cooldown length.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("cooldown_seconds")]
    public double CooldownSeconds { get; init; }

    /// <summary>
    /// Fixture-owned ability range.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("range_meters")]
    public double RangeMeters { get; init; }

    /// <summary>
    /// True when this profile requires the caller to provide a selected target.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("requires_target")]
    public bool RequiresTarget { get; init; } = true;

    /// <summary>
    /// True when line of sight must be valid for target resolution.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("requires_line_of_sight")]
    public bool RequiresLineOfSight { get; init; } = true;

    /// <summary>
    /// Declared effect profiles, resolved in authored order.
    /// </summary>
    public List<CombatTacticalInstantAbilityEffectFixture> Effects { get; init; } = new();
}

/// <summary>
/// Effect declaration for a tactical instant ability profile.
/// </summary>
public sealed record CombatTacticalInstantAbilityEffectFixture
{
    /// <summary>
    /// Declared effect type.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("effect_type")]
    public string EffectType { get; init; } = string.Empty;

    /// <summary>
    /// Fixture-owned damage values for direct damage effects.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("damage_by_band")]
    public List<CombatBandValue> DamageByBand { get; init; } = new();

    /// <summary>
    /// Fixture-owned duration for self-buff effects.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("duration_seconds")]
    public double? DurationSeconds { get; init; }

    /// <summary>
    /// Fixture-owned damage reduction ratio for self-buff effects.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("damage_reduction")]
    public double? DamageReduction { get; init; }

    /// <summary>
    /// Fixture-owned recovery pressure for interrupt effects.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("interrupt_seconds")]
    public double? InterruptSeconds { get; init; }
}

/// <summary>
/// Encounter fixture row used by later smoke/profile harnesses.
/// </summary>
public sealed record CombatEncounterFixture
{
    /// <summary>
    /// Stable fixture id.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Actor fixture ids participating in this encounter.
    /// </summary>
    public List<string> ActorFixtureIds { get; init; } = new();

    /// <summary>
    /// T1 kill-weight seed emitted for downstream validation.
    /// </summary>
    public double KillWeightSeed { get; init; }

    /// <summary>
    /// Source-ref aliases available to downstream fixtures.
    /// </summary>
    public List<string> SourceRefAliases { get; init; } = new();

    /// <summary>
    /// Human-readable required outcome from the GDD.
    /// </summary>
    public string RequiredOutcome { get; init; } = string.Empty;

    /// <summary>
    /// Optional solo block profile id for named encounters.
    /// </summary>
    public string? SoloBlockProfileId { get; init; }
}
