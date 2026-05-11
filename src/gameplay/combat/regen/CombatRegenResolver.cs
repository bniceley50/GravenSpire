#nullable enable

using System;

namespace Gravenspire.Gameplay.Combat;

public sealed record CombatRegenTickRequest(
    CombatActorState Actor,
    CombatRegenAndCombatExitTuning Tuning,
    CombatTick Tick,
    int TickRateHz);

public sealed record CombatRegenTickResult(
    CombatActorState Actor,
    int ManaRestored,
    long NextRegenTickIndex,
    bool TickResolved);

public sealed class CombatRegenResolver
{
    public CombatRegenTickResult ResolveTick(CombatRegenTickRequest request)
    {
        CombatArgumentNull.ThrowIfNull(request);
        CombatArgumentNull.ThrowIfNull(request.Actor);
        CombatArgumentNull.ThrowIfNull(request.Tuning);
        if (request.TickRateHz <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "tick_rate_hz must be positive.");
        }

        var intervalTicks = CombatRegenFormulas.CalculateRegenTickIntervalTicks(request.Tuning, request.TickRateHz);
        var nextDueTick = request.Actor.NextRegenTickIndex ?? request.Tick.Index;
        if (request.Tick.Index < nextDueTick)
        {
            return new CombatRegenTickResult(
                request.Actor,
                ManaRestored: 0,
                nextDueTick,
                TickResolved: false);
        }

        var manaRegen = CombatRegenFormulas.CalculateResourceRegenPerTick(new CombatResourceRegenRequest(
            request.Actor,
            CombatResourceKind.Mana,
            request.Tuning,
            request.Actor.PostureState,
            request.Actor.CombatState));
        var currentMana = Math.Min(request.Actor.MaxMana, checked(request.Actor.CurrentMana + manaRegen));
        var restored = currentMana - request.Actor.CurrentMana;
        var updatedActor = request.Actor.WithCurrentMana(currentMana) with
        {
            PostureState = request.Actor.PostureState,
            NextRegenTickIndex = checked(request.Tick.Index + intervalTicks),
            LastHostileActionTickIndex = request.Actor.LastHostileActionTickIndex,
            CombatExitRemainingSeconds = request.Actor.CombatExitRemainingSeconds
        };

        return new CombatRegenTickResult(
            updatedActor,
            restored,
            updatedActor.NextRegenTickIndex!.Value,
            TickResolved: true);
    }
}
