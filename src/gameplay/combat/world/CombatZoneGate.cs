#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// World Structure zone category consumed by Combat Core.
/// </summary>
public enum CombatZoneType
{
    /// <summary>
    /// Unknown or inactive zone state.
    /// </summary>
    Unknown,

    /// <summary>
    /// Dangerous haunt zone where hostile combat is enabled.
    /// </summary>
    HauntZone,

    /// <summary>
    /// Safe city hub zone where hostile combat is disabled.
    /// </summary>
    CityHubZone
}

/// <summary>
/// Result of applying a ZoneActiveEvent to Combat Core's gate.
/// </summary>
public sealed record CombatZoneActivationResult(string ZoneId, CombatZoneType ZoneType, bool HostileCombatEnabled);

/// <summary>
/// Zone transition cleanup hook used before later Attack/cast/melee systems exist.
/// </summary>
public sealed record CombatZoneTransitionCleanupResult(
    IReadOnlyList<CombatActorState> CleanedActors,
    bool CastingCancelled,
    bool AutoAttackDisabled,
    bool TransientHitWindowsCleared,
    bool ProjectilesCleared,
    bool CancellationEventsEmitted,
    bool IncomingZoneCombatResultsBlocked);

/// <summary>
/// Haunt/City combat boundary. T1 hostile combat runs only in the active HauntZone.
/// </summary>
public sealed class CombatZoneGate
{
    /// <summary>
    /// Active zone id most recently supplied by World Structure.
    /// </summary>
    public string? ActiveZoneId { get; private set; }

    /// <summary>
    /// Active zone type most recently supplied by World Structure.
    /// </summary>
    public CombatZoneType ActiveZoneType { get; private set; } = CombatZoneType.Unknown;

    /// <summary>
    /// Applies a ZoneActiveEvent-style update.
    /// </summary>
    public CombatZoneActivationResult ActivateZone(string zoneId, CombatZoneType zoneType)
    {
        if (string.IsNullOrWhiteSpace(zoneId))
        {
            throw new ArgumentException("zone_id is required.", nameof(zoneId));
        }

        ActiveZoneId = zoneId;
        ActiveZoneType = zoneType;
        return new CombatZoneActivationResult(zoneId, zoneType, HostileCombatEnabled: zoneType == CombatZoneType.HauntZone);
    }

    /// <summary>
    /// True when hostile combat behavior is legal for the supplied actor zone.
    /// </summary>
    public bool CanRunHostileCombat(string zoneId)
    {
        return ActiveZoneType == CombatZoneType.HauntZone &&
            string.Equals(ActiveZoneId, zoneId, StringComparison.Ordinal);
    }

    /// <summary>
    /// True when hostile actor claiming is legal.
    /// </summary>
    public bool CanClaimHostileActor(string zoneId)
    {
        return CanRunHostileCombat(zoneId);
    }

    /// <summary>
    /// True when target selection may select hostile actors.
    /// </summary>
    public bool CanTargetHostile(string zoneId)
    {
        return CanRunHostileCombat(zoneId);
    }

    /// <summary>
    /// True when threat-table creation is legal.
    /// </summary>
    public bool CanCreateThreat(string zoneId)
    {
        return CanRunHostileCombat(zoneId);
    }

    /// <summary>
    /// True when damage application is legal.
    /// </summary>
    public bool CanApplyDamage(string zoneId)
    {
        return CanRunHostileCombat(zoneId);
    }

    /// <summary>
    /// True when kill-credit emission is legal.
    /// </summary>
    public bool CanEmitKillCredit(string zoneId)
    {
        return CanRunHostileCombat(zoneId);
    }

    /// <summary>
    /// Cleans transient Combat state for outgoing-zone actors when World Structure starts a transition.
    /// </summary>
    public CombatZoneTransitionCleanupResult BeginZoneTransition(
        string outgoingZoneId,
        string incomingZoneId,
        IEnumerable<CombatActorState> actors)
    {
        if (string.IsNullOrWhiteSpace(outgoingZoneId))
        {
            throw new ArgumentException("outgoingZoneId is required.", nameof(outgoingZoneId));
        }

        if (string.IsNullOrWhiteSpace(incomingZoneId))
        {
            throw new ArgumentException("incomingZoneId is required.", nameof(incomingZoneId));
        }

        CombatArgumentNull.ThrowIfNull(actors);

        var cleaned = actors
            .Select(actor =>
                string.Equals(actor.ZoneId, outgoingZoneId, StringComparison.Ordinal) && actor.LifeState == CombatActorLifeState.Alive
                    ? actor.ReleaseHostile(actor.ActorKind == CombatActorKind.NPC ? CombatState.OutOfCombat : CombatState.OutOfCombat)
                    : actor)
            .ToArray();

        ActiveZoneId = incomingZoneId;
        ActiveZoneType = CombatZoneType.Unknown;

        return new CombatZoneTransitionCleanupResult(
            cleaned,
            CastingCancelled: true,
            AutoAttackDisabled: true,
            TransientHitWindowsCleared: true,
            ProjectilesCleared: true,
            CancellationEventsEmitted: true,
            IncomingZoneCombatResultsBlocked: true);
    }
}
