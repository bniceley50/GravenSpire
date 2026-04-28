#nullable enable

using System;

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Identifies a resolved combat simulation tick.
/// </summary>
public readonly record struct CombatTick(long Index, double ElapsedSeconds)
{
    /// <summary>
    /// The initial combat tick before any simulation steps run.
    /// </summary>
    public static CombatTick Zero { get; } = new(0, 0d);
}

/// <summary>
/// Provides fixed-step combat time without depending on Unity scene or frame state.
/// </summary>
public interface ICombatClock
{
    /// <summary>
    /// Number of combat simulation ticks resolved per second.
    /// </summary>
    int TickRateHz { get; }

    /// <summary>
    /// Seconds represented by one combat simulation tick.
    /// </summary>
    double TickDurationSeconds { get; }

    /// <summary>
    /// Current combat simulation tick index.
    /// </summary>
    long CurrentTick { get; }

    /// <summary>
    /// Seconds elapsed through the current combat simulation tick.
    /// </summary>
    double ElapsedSeconds { get; }

    /// <summary>
    /// Advances by a deterministic number of ticks and returns the resulting tick.
    /// </summary>
    CombatTick AdvanceTicks(int tickCount);

    /// <summary>
    /// Resets the clock to a known tick and returns the resulting tick.
    /// </summary>
    CombatTick Reset(long tickIndex = 0);

    /// <summary>
    /// Returns the current tick without changing clock state.
    /// </summary>
    CombatTick Snapshot();
}

/// <summary>
/// Fixed-step combat clock for T1 offline combat logic.
/// </summary>
public sealed class FixedCombatClock : ICombatClock
{
    /// <summary>
    /// Creates a fixed combat clock with an explicit tick rate.
    /// </summary>
    public FixedCombatClock(int tickRateHz, long initialTick = 0)
    {
        if (tickRateHz <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tickRateHz), "Combat tick rate must be positive.");
        }

        if (initialTick < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialTick), "Combat tick cannot be negative.");
        }

        TickRateHz = tickRateHz;
        CurrentTick = initialTick;
    }

    /// <inheritdoc />
    public int TickRateHz { get; }

    /// <inheritdoc />
    public double TickDurationSeconds => 1d / TickRateHz;

    /// <inheritdoc />
    public long CurrentTick { get; private set; }

    /// <inheritdoc />
    public double ElapsedSeconds => CurrentTick * TickDurationSeconds;

    /// <inheritdoc />
    public CombatTick AdvanceTicks(int tickCount)
    {
        if (tickCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tickCount), "Combat clock cannot advance by negative ticks.");
        }

        checked
        {
            CurrentTick += tickCount;
        }

        return Snapshot();
    }

    /// <inheritdoc />
    public CombatTick Reset(long tickIndex = 0)
    {
        if (tickIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tickIndex), "Combat tick cannot be negative.");
        }

        CurrentTick = tickIndex;
        return Snapshot();
    }

    /// <inheritdoc />
    public CombatTick Snapshot()
    {
        return new CombatTick(CurrentTick, ElapsedSeconds);
    }
}
