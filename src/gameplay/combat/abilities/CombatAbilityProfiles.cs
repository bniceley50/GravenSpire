#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Gravenspire.Gameplay.Combat.Fixtures;

namespace Gravenspire.Gameplay.Combat;

public static class CombatTacticalAbilityEffectTypes
{
    public const string DirectDamage = "direct_damage";
    public const string SelfBuff = "self_buff";
    public const string InterruptCurrentChannel = "interrupt_current_channel";
}

public enum CombatTacticalAbilityEffectType
{
    DirectDamage,
    SelfBuff,
    InterruptCurrentChannel
}

public enum CombatTacticalAbilityResourceKind
{
    Physical,
    Magical
}

public sealed record CombatTacticalAbilityProfile(
    string AbilityId,
    double CastTimeSeconds,
    CombatTacticalAbilityResourceKind ResourceKind,
    int CostMana,
    int CostEndurance,
    double CooldownSeconds,
    double RangeMeters,
    bool RequiresTarget,
    bool RequiresLineOfSight,
    IReadOnlyList<CombatTacticalAbilityEffectProfile> Effects)
{
    public static CombatTacticalAbilityProfile FromFixture(
        CombatTacticalInstantAbilityProfileFixture fixture,
        string band)
    {
        CombatArgumentNull.ThrowIfNull(fixture);
        if (string.IsNullOrWhiteSpace(fixture.Id))
        {
            throw new ArgumentException("ability profile id is required.", nameof(fixture));
        }

        if (string.IsNullOrWhiteSpace(band))
        {
            throw new ArgumentException("fixture band is required.", nameof(band));
        }

        var effects = fixture.Effects
            .Select(effect => CombatTacticalAbilityEffectProfile.FromFixture(effect, band))
            .ToArray();

        if (effects.Length == 0)
        {
            throw new ArgumentException("ability profile requires at least one declared effect.", nameof(fixture));
        }

        return new CombatTacticalAbilityProfile(
            fixture.Id,
            fixture.CastTimeSeconds,
            fixture.ResourceKind ?? throw new ArgumentException("ability profile resource_kind is required.", nameof(fixture)),
            fixture.CostMana,
            fixture.CostEndurance,
            fixture.CooldownSeconds,
            fixture.RangeMeters,
            fixture.RequiresTarget,
            fixture.RequiresLineOfSight,
            effects);
    }
}

public sealed record CombatTacticalAbilityEffectProfile(
    CombatTacticalAbilityEffectType EffectType,
    int? Damage,
    double? DurationSeconds,
    double? DamageReduction,
    double? InterruptSeconds)
{
    public static CombatTacticalAbilityEffectProfile FromFixture(
        CombatTacticalInstantAbilityEffectFixture fixture,
        string band)
    {
        CombatArgumentNull.ThrowIfNull(fixture);

        return fixture.EffectType switch
        {
            CombatTacticalAbilityEffectTypes.DirectDamage => new CombatTacticalAbilityEffectProfile(
                CombatTacticalAbilityEffectType.DirectDamage,
                RequiredBandValue(fixture.DamageByBand, band, fixture.EffectType),
                DurationSeconds: null,
                DamageReduction: null,
                InterruptSeconds: null),
            CombatTacticalAbilityEffectTypes.SelfBuff => new CombatTacticalAbilityEffectProfile(
                CombatTacticalAbilityEffectType.SelfBuff,
                Damage: null,
                RequiredTiming(fixture.DurationSeconds, fixture.EffectType),
                fixture.DamageReduction,
                InterruptSeconds: null),
            CombatTacticalAbilityEffectTypes.InterruptCurrentChannel => new CombatTacticalAbilityEffectProfile(
                CombatTacticalAbilityEffectType.InterruptCurrentChannel,
                Damage: null,
                DurationSeconds: null,
                DamageReduction: null,
                RequiredTiming(fixture.InterruptSeconds, fixture.EffectType)),
            _ => throw new ArgumentException($"Unsupported tactical ability effect type: {fixture.EffectType}.", nameof(fixture))
        };
    }

    private static int RequiredBandValue(
        IEnumerable<CombatBandValue> values,
        string band,
        string effectType)
    {
        foreach (var value in values)
        {
            if (string.Equals(value.Band, band, StringComparison.Ordinal))
            {
                return value.Value;
            }
        }

        throw new ArgumentException($"{effectType} is missing a value for fixture band {band}.");
    }

    private static double RequiredTiming(double? value, string effectType)
    {
        return value ?? throw new ArgumentException($"{effectType} is missing required authored timing.");
    }
}
