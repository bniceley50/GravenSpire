#nullable enable

using System;

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Creature / Enemy AI path probe status consumed by Combat Core leash logic.
/// </summary>
public enum CombatPathStatus
{
    /// <summary>
    /// Path probe reached a complete path.
    /// </summary>
    PathComplete,

    /// <summary>
    /// Path probe reached only a partial path.
    /// </summary>
    PathPartial,

    /// <summary>
    /// Path probe found no valid path.
    /// </summary>
    PathInvalid
}

/// <summary>
/// Path probe result published by Creature / Enemy AI or a test fake.
/// </summary>
public sealed record CombatPathProbeResult(
    string CombatActorId,
    CombatPathStatus PathStatus,
    bool PathPending,
    long SampledTick);

/// <summary>
/// Leash tuning owned by authored combat data.
/// </summary>
public sealed record CombatLeashTuning(
    double LeashDistanceMeters,
    double PathFailureGraceSeconds,
    double PathPendingGraceSeconds,
    double PathStatusSampleSeconds,
    double LeashThreatMemorySeconds,
    double LeashReAggroDistanceMeters)
{
    /// <summary>
    /// Approved T1 defaults from Combat Core.
    /// </summary>
    public static CombatLeashTuning T1Default { get; } = new(35d, 1d, 1d, 0.25d, 30d, 20d);
}

/// <summary>
/// Runtime leash timer state. Threat table remains CombatActorState-owned.
/// </summary>
public sealed record CombatLeashRuntimeState(
    double? ContinuousPathFailureStartedSeconds,
    double? PathPendingStartedSeconds,
    double? LeashingStartedSeconds,
    double? ThreatMemoryExpiresSeconds,
    bool AnchorReached)
{
    /// <summary>
    /// Empty leash runtime state.
    /// </summary>
    public static CombatLeashRuntimeState None { get; } = new(null, null, null, null, AnchorReached: false);
}

/// <summary>
/// Result of evaluating leash entry.
/// </summary>
public sealed record CombatLeashEvaluationResult(
    CombatActorState Actor,
    CombatLeashRuntimeState State,
    bool EnteredLeashing,
    bool ReturnToAnchorRequested,
    bool NewAttacksAndCastsStopped,
    bool ActiveAttackIntentCleared);

/// <summary>
/// Result of evaluating re-aggro or threat-memory expiry.
/// </summary>
public sealed record CombatReAggroResult(
    CombatActorState Actor,
    CombatLeashRuntimeState State,
    bool ReAggroed,
    bool ThreatMemoryExpired,
    bool ThreatCleared);

/// <summary>
/// Combat-owned leash state coordinator. Creature AI remains responsible for actual movement.
/// </summary>
public sealed class CombatLeashCoordinator
{
    /// <summary>
    /// Evaluates path failure, pending probes, and leash distance for one hostile actor.
    /// </summary>
    public CombatLeashEvaluationResult EvaluatePathAndDistance(
        CombatActorState hostile,
        CombatPoint3 hostilePosition,
        CombatPoint3 anchorPosition,
        CombatPathProbeResult pathProbe,
        CombatTick tick,
        CombatLeashRuntimeState? previousState = null,
        CombatLeashTuning? tuning = null)
    {
        CombatArgumentNull.ThrowIfNull(hostile);
        CombatArgumentNull.ThrowIfNull(pathProbe);

        var actualTuning = tuning ?? CombatLeashTuning.T1Default;
        var state = previousState ?? CombatLeashRuntimeState.None;
        if (!string.Equals(hostile.CombatActorId, pathProbe.CombatActorId, StringComparison.Ordinal))
        {
            throw new ArgumentException("Path probe combat actor id must match hostile actor id.", nameof(pathProbe));
        }

        double? failureStart = pathProbe.PathStatus is CombatPathStatus.PathPartial or CombatPathStatus.PathInvalid
            ? state.ContinuousPathFailureStartedSeconds ?? tick.ElapsedSeconds
            : null;
        double? pendingStart = pathProbe.PathPending
            ? state.PathPendingStartedSeconds ?? tick.ElapsedSeconds
            : null;

        var leashByDistance = hostilePosition.DistanceTo(anchorPosition) > actualTuning.LeashDistanceMeters;
        var leashByFailure = failureStart is not null &&
            tick.ElapsedSeconds - failureStart.Value > actualTuning.PathFailureGraceSeconds;
        var leashByPending = pendingStart is not null &&
            tick.ElapsedSeconds - pendingStart.Value > actualTuning.PathPendingGraceSeconds;

        var shouldLeash = leashByDistance || leashByFailure || leashByPending;
        if (!shouldLeash)
        {
            return new CombatLeashEvaluationResult(
                hostile,
                state with
                {
                    ContinuousPathFailureStartedSeconds = failureStart,
                    PathPendingStartedSeconds = pendingStart
                },
                EnteredLeashing: false,
                ReturnToAnchorRequested: false,
                NewAttacksAndCastsStopped: false,
                ActiveAttackIntentCleared: false);
        }

        var leashingStarted = state.LeashingStartedSeconds ?? tick.ElapsedSeconds;
        var memoryExpires = state.ThreatMemoryExpiresSeconds ?? tick.ElapsedSeconds + actualTuning.LeashThreatMemorySeconds;
        var leashingActor = hostile.WithCombatState(CombatState.Leashing);

        return new CombatLeashEvaluationResult(
            leashingActor,
            state with
            {
                ContinuousPathFailureStartedSeconds = failureStart,
                PathPendingStartedSeconds = pendingStart,
                LeashingStartedSeconds = leashingStarted,
                ThreatMemoryExpiresSeconds = memoryExpires
            },
            EnteredLeashing: hostile.CombatState != CombatState.Leashing,
            ReturnToAnchorRequested: true,
            NewAttacksAndCastsStopped: true,
            ActiveAttackIntentCleared: true);
    }

    /// <summary>
    /// Evaluates re-aggro while threat memory is active or clears threat after anchor/expiry.
    /// </summary>
    public CombatReAggroResult EvaluateReAggro(
        CombatActorState hostile,
        CombatLeashRuntimeState state,
        CombatPoint3 hostilePosition,
        CombatPoint3 targetPosition,
        bool hasLineOfSightToTarget,
        CombatTick tick,
        CombatLeashTuning? tuning = null)
    {
        CombatArgumentNull.ThrowIfNull(hostile);
        CombatArgumentNull.ThrowIfNull(state);

        var actualTuning = tuning ?? CombatLeashTuning.T1Default;
        var memoryExpired = state.ThreatMemoryExpiresSeconds is not null &&
            tick.ElapsedSeconds >= state.ThreatMemoryExpiresSeconds.Value;

        if (state.AnchorReached || memoryExpired)
        {
            return new CombatReAggroResult(
                hostile.ReleaseHostile(),
                CombatLeashRuntimeState.None,
                ReAggroed: false,
                ThreatMemoryExpired: memoryExpired,
                ThreatCleared: true);
        }

        var canReAggro = hostile.CombatState == CombatState.Leashing &&
            hasLineOfSightToTarget &&
            hostilePosition.DistanceTo(targetPosition) <= actualTuning.LeashReAggroDistanceMeters;

        if (!canReAggro)
        {
            return new CombatReAggroResult(hostile, state, ReAggroed: false, ThreatMemoryExpired: false, ThreatCleared: false);
        }

        return new CombatReAggroResult(
            hostile.WithCombatState(CombatState.InCombat),
            state,
            ReAggroed: true,
            ThreatMemoryExpired: false,
            ThreatCleared: false);
    }
}
