#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Gravenspire.Gameplay.Combat;

public sealed record CombatSimulationStepRequest(
    ICombatClock Clock,
    int TickBudget,
    bool IsPaused,
    Func<CombatTick, CombatMeleeTickResult?>? ResolveMeleeTick = null);

public sealed record CombatSimulationStepResult(
    CombatTick StartTick,
    CombatTick EndTick,
    IReadOnlyList<CombatTick> ResolvedTicks,
    IReadOnlyList<CombatMeleeTickResult> MeleeResults,
    bool WasPaused);

/// <summary>
/// Advances a caller-owned fixed combat clock by an explicit tick budget.
/// </summary>
public sealed class CombatSimulationStepper
{
    public CombatSimulationStepResult Step(CombatSimulationStepRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Clock);

        if (request.TickBudget < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "tick_budget must not be negative.");
        }

        var startTick = request.Clock.Snapshot();
        if (request.IsPaused || request.TickBudget == 0)
        {
            return new CombatSimulationStepResult(
                startTick,
                request.Clock.Snapshot(),
                Array.Empty<CombatTick>(),
                Array.Empty<CombatMeleeTickResult>(),
                request.IsPaused);
        }

        var resolvedTicks = new List<CombatTick>();
        var meleeResults = new List<CombatMeleeTickResult>();

        for (var tick = 0; tick < request.TickBudget; tick++)
        {
            var currentTick = request.Clock.AdvanceTicks(1);
            resolvedTicks.Add(currentTick);

            var meleeResult = request.ResolveMeleeTick?.Invoke(currentTick);
            if (meleeResult is not null)
            {
                meleeResults.Add(meleeResult);
            }
        }

        return new CombatSimulationStepResult(
            startTick,
            request.Clock.Snapshot(),
            new ReadOnlyCollection<CombatTick>(resolvedTicks),
            new ReadOnlyCollection<CombatMeleeTickResult>(meleeResults),
            WasPaused: false);
    }
}
