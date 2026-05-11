#nullable enable

using System;

namespace Gravenspire.Gameplay.Combat;

public sealed record CombatExitTimerRequest(
    CombatTick CurrentTick,
    CombatTick LastHostileActionTick,
    int TickRateHz,
    int ValidHostileThreatEntries,
    CombatRegenAndCombatExitTuning Tuning);

public sealed record CombatExitTimerResult(
    bool CanExitCombat,
    double SecondsSinceLastHostileAction,
    double RemainingSeconds);

public sealed class CombatExitStateMachine
{
    public CombatExitTimerResult Evaluate(CombatExitTimerRequest request)
    {
        CombatArgumentNull.ThrowIfNull(request);
        CombatArgumentNull.ThrowIfNull(request.Tuning);
        if (request.TickRateHz <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "tick_rate_hz must be positive.");
        }

        if (request.ValidHostileThreatEntries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "valid_hostile_threat_entries must not be negative.");
        }

        if (request.CurrentTick.Index < request.LastHostileActionTick.Index)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "current tick must not precede last hostile action tick.");
        }

        var secondsSinceHostileAction =
            (request.CurrentTick.Index - request.LastHostileActionTick.Index) / (double)request.TickRateHz;
        var remainingSeconds = Math.Max(0, request.Tuning.CombatExitTimerSeconds - secondsSinceHostileAction);
        var canExit = request.ValidHostileThreatEntries == 0 &&
            secondsSinceHostileAction > request.Tuning.CombatExitTimerSeconds;

        return new CombatExitTimerResult(canExit, secondsSinceHostileAction, remainingSeconds);
    }
}
