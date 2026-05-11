#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Target acquisition tuning owned by authored combat data.
/// </summary>
public sealed record CombatTargetingTuning(double TargetAcquireRadiusMeters, int CombatQueryBufferSize)
{
    /// <summary>
    /// Approved T1 defaults from Combat Core.
    /// </summary>
    public static CombatTargetingTuning T1Default { get; } = new(35d, 64);
}

/// <summary>
/// One hostile actor returned from a bounded spatial query.
/// </summary>
public sealed record CombatTargetCandidate(
    CombatActorState Actor,
    CombatPoint3 Position,
    CombatSpatialAnchorSet Anchors,
    IReadOnlyList<CombatLosLayer> LosBlockingLayers,
    int AuthoredColliderIndex);

/// <summary>
/// Result of target selection.
/// </summary>
public sealed record CombatTargetSelectionResult(
    CombatActorState? Target,
    IReadOnlyList<CombatTargetCandidate> OrderedCandidates,
    CombatQueryBufferOverflowDiagnostic? OverflowDiagnostic,
    IReadOnlyList<string> RejectionReasons)
{
    /// <summary>
    /// True when a legal target was selected.
    /// </summary>
    public bool Succeeded => Target is not null;
}

/// <summary>
/// Deterministic target selection over bounded query results.
/// </summary>
public sealed class CombatTargetSelector
{
    /// <summary>
    /// Selects the first or next legal hostile target by radius, LoS, and deterministic ordering.
    /// </summary>
    public CombatTargetSelectionResult SelectNextTarget(
        CombatActorState player,
        CombatPoint3 queryOrigin,
        IEnumerable<CombatTargetCandidate> candidates,
        CombatZoneGate zoneGate,
        CombatTargetingTuning? tuning = null,
        string? currentTargetCombatActorId = null)
    {
        CombatArgumentNull.ThrowIfNull(player);
        CombatArgumentNull.ThrowIfNull(candidates);
        CombatArgumentNull.ThrowIfNull(zoneGate);

        var actualTuning = tuning ?? CombatTargetingTuning.T1Default;
        if (actualTuning.TargetAcquireRadiusMeters <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tuning), "target_acquire_radius_meters must be positive.");
        }

        if (actualTuning.CombatQueryBufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tuning), "combat_query_buffer_size must be positive.");
        }

        var rawCandidates = candidates.ToArray();
        var overflow = rawCandidates.Length >= actualTuning.CombatQueryBufferSize
            ? T1CombatLineOfSight.CreateOverflowDiagnostic(actualTuning.CombatQueryBufferSize, rawCandidates.Length)
            : null;

        var ordered = rawCandidates
            .OrderBy(candidate => queryOrigin.DistanceMillimetersTo(candidate.Position))
            .ThenBy(candidate => candidate.Actor.CombatSortKey, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.AuthoredColliderIndex)
            .Take(actualTuning.CombatQueryBufferSize)
            .ToArray();

        var rejectionReasons = new List<string>();
        var valid = ordered
            .Where(candidate => IsCandidateValid(player, queryOrigin, zoneGate, actualTuning, candidate, rejectionReasons))
            .ToArray();

        if (valid.Length == 0)
        {
            return new CombatTargetSelectionResult(null, ordered, overflow, rejectionReasons);
        }

        var selectedIndex = 0;
        if (!string.IsNullOrWhiteSpace(currentTargetCombatActorId))
        {
            var currentIndex = Array.FindIndex(
                valid,
                candidate => string.Equals(candidate.Actor.CombatActorId, currentTargetCombatActorId, StringComparison.Ordinal));

            if (currentIndex >= 0)
            {
                selectedIndex = (currentIndex + 1) % valid.Length;
            }
        }

        return new CombatTargetSelectionResult(valid[selectedIndex].Actor, ordered, overflow, rejectionReasons);
    }

    private static bool IsCandidateValid(
        CombatActorState player,
        CombatPoint3 queryOrigin,
        CombatZoneGate zoneGate,
        CombatTargetingTuning tuning,
        CombatTargetCandidate candidate,
        ICollection<string> rejectionReasons)
    {
        if (candidate.Actor.ActorKind != CombatActorKind.NPC)
        {
            rejectionReasons.Add($"{candidate.Actor.CombatActorId}: only hostile NPC actors are targetable in T1.");
            return false;
        }

        if (!candidate.Actor.IsAlive)
        {
            rejectionReasons.Add($"{candidate.Actor.CombatActorId}: actor is not alive.");
            return false;
        }

        if (!string.Equals(player.ZoneId, candidate.Actor.ZoneId, StringComparison.Ordinal))
        {
            rejectionReasons.Add($"{candidate.Actor.CombatActorId}: actor is not in the player's active zone.");
            return false;
        }

        if (!zoneGate.CanTargetHostile(candidate.Actor.ZoneId))
        {
            rejectionReasons.Add($"{candidate.Actor.CombatActorId}: active zone does not allow hostile targeting.");
            return false;
        }

        if (queryOrigin.DistanceTo(candidate.Position) > tuning.TargetAcquireRadiusMeters)
        {
            rejectionReasons.Add($"{candidate.Actor.CombatActorId}: actor is outside target_acquire_radius_meters.");
            return false;
        }

        if (!T1CombatLineOfSight.HasLineOfSight(candidate.LosBlockingLayers))
        {
            rejectionReasons.Add($"{candidate.Actor.CombatActorId}: T1 line of sight is blocked.");
            return false;
        }

        return true;
    }
}
