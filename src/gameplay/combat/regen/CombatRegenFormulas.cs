#nullable enable

using System;

namespace Gravenspire.Gameplay.Combat;

public enum CombatResourceKind
{
    Health,
    Mana
}

public sealed record CombatResourceRegenTuning
{
    public int BaseRegen { get; init; }

    public double LevelRegenScalar { get; init; }

    public double PercentRegenScalar { get; init; }

    public double StandingPostureMultiplier { get; init; }

    public double SittingPostureMultiplier { get; init; }

    public double OutOfCombatMultiplier { get; init; }

    public double InCombatMultiplier { get; init; }
}

public sealed record CombatRegenAndCombatExitTuning
{
    public double RegenTickIntervalSeconds { get; init; }

    public double CombatExitTimerSeconds { get; init; }

    public int SittingThreatBonus { get; init; }

    public CombatResourceRegenTuning HealthRegen { get; init; } = new();

    public CombatResourceRegenTuning ManaRegen { get; init; } = new();
}

public sealed record CombatResourceRegenRequest(
    CombatActorState Actor,
    CombatResourceKind Resource,
    CombatRegenAndCombatExitTuning Tuning,
    CombatPostureState PostureState,
    CombatState CombatState);

public static class CombatRegenFormulas
{
    public static int CalculateResourceRegenPerTick(CombatResourceRegenRequest request)
    {
        CombatArgumentNull.ThrowIfNull(request);
        CombatArgumentNull.ThrowIfNull(request.Actor);
        CombatArgumentNull.ThrowIfNull(request.Tuning);

        var maxResource = request.Resource == CombatResourceKind.Mana
            ? request.Actor.MaxMana
            : request.Actor.MaxHealth;
        if (maxResource <= 0)
        {
            return 0;
        }

        var profile = request.Resource == CombatResourceKind.Mana
            ? request.Tuning.ManaRegen
            : request.Tuning.HealthRegen;

        var baseAmount = Math.Floor(
            profile.BaseRegen +
            (request.Actor.Level * profile.LevelRegenScalar) +
            (maxResource * profile.PercentRegenScalar));
        var postureMultiplier = request.PostureState == CombatPostureState.Sitting &&
            request.CombatState == CombatState.OutOfCombat
                ? profile.SittingPostureMultiplier
                : profile.StandingPostureMultiplier;
        var combatMultiplier = request.CombatState == CombatState.OutOfCombat
            ? profile.OutOfCombatMultiplier
            : profile.InCombatMultiplier;
        var regen = Math.Floor(baseAmount * postureMultiplier * combatMultiplier);

        return Math.Max(0, checked((int)regen));
    }

    public static long CalculateRegenTickIntervalTicks(
        CombatRegenAndCombatExitTuning tuning,
        int tickRateHz)
    {
        CombatArgumentNull.ThrowIfNull(tuning);
        if (tickRateHz <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tickRateHz), "tick_rate_hz must be positive.");
        }

        if (tuning.RegenTickIntervalSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tuning), "regen_tick_interval_seconds must be positive.");
        }

        return Math.Max(1L, checked((long)Math.Ceiling(tuning.RegenTickIntervalSeconds * tickRateHz)));
    }
}
