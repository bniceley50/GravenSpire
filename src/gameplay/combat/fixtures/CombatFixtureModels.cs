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
    /// Encounter fixtures available for T1 Combat Core tests.
    /// </summary>
    public List<CombatEncounterFixture> EncounterFixtures { get; init; } = new();
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
