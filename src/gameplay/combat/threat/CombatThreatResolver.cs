#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Gravenspire.Gameplay.Combat;

public sealed record CombatSittingThreatRequest(
    CombatActorState Sitter,
    IReadOnlyList<CombatActorState> HostileActors,
    CombatRegenAndCombatExitTuning Tuning);

public sealed record CombatSittingThreatResult(
    IReadOnlyList<CombatActorState> HostileActors,
    int UpdatedThreatEntries);

public sealed class CombatThreatResolver
{
    public int CountValidHostileThreatEntries(
        CombatActorState actor,
        IEnumerable<CombatActorState> hostileActors)
    {
        CombatArgumentNull.ThrowIfNull(actor);
        CombatArgumentNull.ThrowIfNull(hostileActors);

        var count = 0;
        foreach (var hostile in hostileActors)
        {
            if (hostile.ActorKind == CombatActorKind.NPC &&
                hostile.IsAlive &&
                hostile.ThreatTable.TryGetValue(actor.CombatActorId, out var threat) &&
                threat > 0)
            {
                count++;
            }
        }

        return count;
    }

    public CombatSittingThreatResult ApplySittingThreatBonus(CombatSittingThreatRequest request)
    {
        CombatArgumentNull.ThrowIfNull(request);
        CombatArgumentNull.ThrowIfNull(request.Sitter);
        CombatArgumentNull.ThrowIfNull(request.HostileActors);
        CombatArgumentNull.ThrowIfNull(request.Tuning);
        if (request.Tuning.SittingThreatBonus < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "sitting_threat_bonus must not be negative.");
        }

        var updatedActors = new List<CombatActorState>();
        var updatedEntries = 0;
        foreach (var hostile in request.HostileActors)
        {
            if (hostile.ActorKind == CombatActorKind.NPC &&
                hostile.IsAlive &&
                hostile.ThreatTable.TryGetValue(request.Sitter.CombatActorId, out var threat) &&
                threat > 0)
            {
                updatedActors.Add(hostile.AddThreat(request.Sitter.CombatActorId, request.Tuning.SittingThreatBonus));
                updatedEntries++;
                continue;
            }

            updatedActors.Add(hostile);
        }

        return new CombatSittingThreatResult(
            new ReadOnlyCollection<CombatActorState>(updatedActors),
            updatedEntries);
    }
}
