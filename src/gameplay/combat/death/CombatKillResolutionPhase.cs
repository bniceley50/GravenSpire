#nullable enable

using System;
using System.Collections.Generic;

namespace Gravenspire.Gameplay.Combat;

public sealed record CombatKillResolutionRequest(
    CombatActorState DefeatedActor,
    CombatActorState? PlayerActor,
    double KillWeightSeed,
    int PlayerDamageContribution = 0);

public sealed record CombatKillResolutionResult(
    bool Processed,
    CombatActorDeathEvent? DeathEvent,
    PlayerKillCreditEvent? KillCreditEvent);

/// <summary>
/// Coordinates one Combat-owned NPC kill-resolution pass for a defeated source.
/// </summary>
public sealed class CombatKillResolutionPhase
{
    private readonly HashSet<string> processedDefeatedActorIds = new(StringComparer.Ordinal);

    public CombatKillResolutionResult Resolve(CombatKillResolutionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.DefeatedActor);

        if (request.DefeatedActor.ActorKind != CombatActorKind.NPC)
        {
            throw new InvalidOperationException("NPC kill resolution requires a defeated NPC combat actor.");
        }

        if (request.KillWeightSeed <= 0 || double.IsNaN(request.KillWeightSeed) || double.IsInfinity(request.KillWeightSeed))
        {
            throw new ArgumentOutOfRangeException(nameof(request), "kill_weight_seed must be a positive finite fixture value.");
        }

        if (request.PlayerDamageContribution < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "player damage contribution must not be negative.");
        }

        if (request.DefeatedActor.CurrentHealth > 0 || request.DefeatedActor.IsAlive)
        {
            return new CombatKillResolutionResult(Processed: false, DeathEvent: null, KillCreditEvent: null);
        }

        if (!processedDefeatedActorIds.Add(request.DefeatedActor.CombatActorId))
        {
            return new CombatKillResolutionResult(Processed: false, DeathEvent: null, KillCreditEvent: null);
        }

        var deathEvent = new CombatActorDeathEvent(
            request.DefeatedActor.CombatActorId,
            request.DefeatedActor.StableSourceRef,
            request.DefeatedActor.ZoneId);

        var killCreditEvent = HasQualifyingPlayerContribution(request)
            ? new PlayerKillCreditEvent(
                request.DefeatedActor.StableSourceRef,
                request.DefeatedActor.ZoneId,
                request.DefeatedActor.FactionId,
                request.KillWeightSeed)
            : null;

        return new CombatKillResolutionResult(Processed: true, deathEvent, killCreditEvent);
    }

    private static bool HasQualifyingPlayerContribution(CombatKillResolutionRequest request)
    {
        var player = request.PlayerActor;
        if (player is null || player.ActorKind != CombatActorKind.Player)
        {
            return false;
        }

        if (request.PlayerDamageContribution > 0)
        {
            return true;
        }

        return request.DefeatedActor.ThreatTable.TryGetValue(player.CombatActorId, out var threat) && threat > 0;
    }
}
