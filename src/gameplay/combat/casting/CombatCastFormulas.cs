#nullable enable

using System;

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Injected tuning for the Combat Core damage interrupt formula.
/// </summary>
public sealed record CombatInterruptFormulaTuning(
    double BaseInterruptChance,
    double DamageInterruptScalar,
    double EarlyCastInterruptScalar,
    double InterruptChanceMin,
    double InterruptChanceMax);

/// <summary>
/// Deterministic cast and interrupt formulas used by slow spell casting.
/// </summary>
public static class CombatCastFormulas
{
    /// <summary>
    /// Calculates the chance that eligible post-mitigation damage interrupts a cast.
    /// </summary>
    public static double CalculateInterruptChance(
        int damageTaken,
        int maxHealth,
        double castTimeRemainingSeconds,
        double castTimeTotalSeconds,
        double interruptResistance,
        CombatInterruptFormulaTuning tuning)
    {
        CombatArgumentNull.ThrowIfNull(tuning);

        if (damageTaken < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(damageTaken), "Damage taken cannot be negative.");
        }

        if (maxHealth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxHealth), "Max health must be positive.");
        }

        if (castTimeTotalSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(castTimeTotalSeconds), "Cast total time must be positive.");
        }

        if (castTimeRemainingSeconds < 0 || castTimeRemainingSeconds > castTimeTotalSeconds)
        {
            throw new ArgumentOutOfRangeException(nameof(castTimeRemainingSeconds), "Remaining cast time must be inside the cast duration.");
        }

        if (tuning.InterruptChanceMin < 0 ||
            tuning.InterruptChanceMax > 1 ||
            tuning.InterruptChanceMin > tuning.InterruptChanceMax)
        {
            throw new ArgumentOutOfRangeException(nameof(tuning), "Interrupt chance clamp bounds must be an ordered ratio range.");
        }

        var pressure = damageTaken / (double)maxHealth * tuning.DamageInterruptScalar;
        var remainingFraction = castTimeRemainingSeconds / castTimeTotalSeconds;
        var earlyCastPressure = remainingFraction * tuning.EarlyCastInterruptScalar;
        var unclamped = tuning.BaseInterruptChance + pressure + earlyCastPressure - interruptResistance;

        return Clamp(unclamped, tuning.InterruptChanceMin, tuning.InterruptChanceMax);
    }

    /// <summary>
    /// Converts authored seconds into a deterministic Combat Simulation Tick count.
    /// </summary>
    public static long SecondsToTicksCeiling(double seconds, int tickRateHz)
    {
        if (seconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds cannot be negative.");
        }

        if (tickRateHz <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tickRateHz), "Tick rate must be positive.");
        }

        return Math.Max(1L, (long)Math.Ceiling(seconds * tickRateHz));
    }

    private static double Clamp(double value, double minimum, double maximum)
    {
        return Math.Min(maximum, Math.Max(minimum, value));
    }
}
