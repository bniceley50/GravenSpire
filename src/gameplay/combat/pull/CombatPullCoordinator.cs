#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Faction filter applied by social assist profiles.
/// </summary>
public enum CombatAssistFactionFilter
{
    /// <summary>
    /// Candidate must share faction or be supplied by a later explicit ally map.
    /// </summary>
    SameFactionOrExplicitAlly
}

/// <summary>
/// Encounter filter applied by social assist profiles.
/// </summary>
public enum CombatAssistEncounterFilter
{
    /// <summary>
    /// Candidate must share encounter group or social-link group.
    /// </summary>
    SameEncounterOrSharedSocialGroup
}

/// <summary>
/// Non-warning presentation signal allowed for body-pull feedback.
/// </summary>
public enum CombatPullPresentationSignal
{
    /// <summary>
    /// Enemy pivot or stance shift is the visible signal.
    /// </summary>
    EnemyPivotOrStanceShift
}

/// <summary>
/// Authored social assist profile consumed by pull logic.
/// </summary>
public sealed record CombatSocialAssistProfile(
    string SocialLinkGroupId,
    string? EncounterGroupId,
    bool AssistEnabled,
    double AssistRadiusMeters,
    int AssistThreatInitial,
    bool AssistRequiresLosToPrimary,
    bool AssistRequiresLosToTarget,
    CombatAssistFactionFilter AssistFactionFilter,
    CombatAssistEncounterFilter AssistEncounterFilter,
    int AssistOrderIndex)
{
    /// <summary>
    /// Default T1 profile values from Combat Core.
    /// </summary>
    public static CombatSocialAssistProfile T1Default(string socialLinkGroupId, string? encounterGroupId = null, int assistOrderIndex = 0)
    {
        return new CombatSocialAssistProfile(
            socialLinkGroupId,
            encounterGroupId,
            AssistEnabled: true,
            AssistRadiusMeters: 12d,
            AssistThreatInitial: 25,
            AssistRequiresLosToPrimary: true,
            AssistRequiresLosToTarget: true,
            CombatAssistFactionFilter.SameFactionOrExplicitAlly,
            CombatAssistEncounterFilter.SameEncounterOrSharedSocialGroup,
            assistOrderIndex);
    }
}

/// <summary>
/// Pull tuning owned by authored combat data.
/// </summary>
public sealed record CombatPullTuning(int ProximityThreatInitial, double SocialAssistPulseSeconds)
{
    /// <summary>
    /// Approved T1 defaults from Combat Core.
    /// </summary>
    public static CombatPullTuning T1Default { get; } = new(25, 2d);
}

/// <summary>
/// Hostile actor plus authored placement/profile data needed by pull logic.
/// </summary>
public sealed record CombatPullCandidate(
    CombatActorState Actor,
    CombatPoint3 Position,
    CombatSpatialAnchorSet Anchors,
    double AggroRadiusMeters,
    CombatSocialAssistProfile? SocialAssistProfile,
    IReadOnlyList<CombatLosLayer> LosBlockingLayersToPlayer,
    IReadOnlyList<CombatLosLayer> LosBlockingLayersToPrimary,
    int AuthoredColliderIndex);

/// <summary>
/// Immutable pull-episode state used to keep social assist bounded.
/// </summary>
public sealed record CombatPullEpisodeState(
    string EpisodeId,
    string PlayerCombatActorId,
    string PrimaryHostileCombatActorId,
    IReadOnlyList<string> JoinedHostileActorIds,
    double StartedElapsedSeconds,
    double LastAssistPulseElapsedSeconds)
{
    /// <summary>
    /// Returns a copy with newly joined hostile ids and an updated pulse timestamp.
    /// </summary>
    public CombatPullEpisodeState WithJoinedActors(IEnumerable<string> joinedActorIds, double lastAssistPulseElapsedSeconds)
    {
        var joined = JoinedHostileActorIds
            .Concat(joinedActorIds)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return this with
        {
            JoinedHostileActorIds = joined,
            LastAssistPulseElapsedSeconds = lastAssistPulseElapsedSeconds
        };
    }
}

/// <summary>
/// Body-pull or social-assist result.
/// </summary>
public sealed record CombatPullResult(
    bool Succeeded,
    CombatActorState? PrimaryHostile,
    IReadOnlyList<CombatActorState> AssistingHostiles,
    CombatPullEpisodeState? Episode,
    bool PlayerAttackEnabled,
    IReadOnlyList<CombatPullPresentationSignal> PresentationSignals,
    IReadOnlyList<string> ForbiddenWarningSignals,
    bool ScriptedEncounterTriggerStarted,
    IReadOnlyList<string> Errors);

/// <summary>
/// Coordinates body/LoS pulls and bounded social assist without Creature AI movement implementation.
/// </summary>
public sealed class CombatPullCoordinator
{
    /// <summary>
    /// Resolves a body/LoS pull and the immediate social-assist pass.
    /// </summary>
    public CombatPullResult ResolveBodyPull(
        CombatActorState player,
        CombatPoint3 playerPosition,
        CombatPullCandidate primary,
        IEnumerable<CombatPullCandidate> linkedCandidates,
        CombatZoneGate zoneGate,
        CombatTick tick,
        CombatPullTuning? tuning = null)
    {
        CombatArgumentNull.ThrowIfNull(player);
        CombatArgumentNull.ThrowIfNull(primary);
        CombatArgumentNull.ThrowIfNull(linkedCandidates);
        CombatArgumentNull.ThrowIfNull(zoneGate);

        var actualTuning = tuning ?? CombatPullTuning.T1Default;
        var errors = ValidateBodyPull(player, playerPosition, primary, zoneGate, actualTuning);
        if (errors.Count > 0)
        {
            return Failure(errors);
        }

        var claimedPrimary = primary.Actor.ClaimHostile(player.CombatActorId, actualTuning.ProximityThreatInitial);
        var episode = new CombatPullEpisodeState(
            $"pull:{tick.Index}:{primary.Actor.CombatSortKey}",
            player.CombatActorId,
            primary.Actor.CombatActorId,
            new[] { primary.Actor.CombatActorId },
            tick.ElapsedSeconds,
            tick.ElapsedSeconds);

        var assists = ResolveEligibleAssists(player, primary, linkedCandidates, zoneGate, episode)
            .Select(candidate => candidate.Actor.ClaimHostile(
                player.CombatActorId,
                candidate.SocialAssistProfile!.AssistThreatInitial,
                CombatState.InCombat))
            .ToArray();

        episode = episode.WithJoinedActors(assists.Select(actor => actor.CombatActorId), tick.ElapsedSeconds);

        return new CombatPullResult(
            Succeeded: true,
            claimedPrimary,
            assists,
            episode,
            PlayerAttackEnabled: false,
            PresentationSignals: new[] { CombatPullPresentationSignal.EnemyPivotOrStanceShift },
            ForbiddenWarningSignals: Array.Empty<string>(),
            ScriptedEncounterTriggerStarted: false,
            Errors: Array.Empty<string>());
    }

    /// <summary>
    /// Resolves a later social-assist pulse if the pulse interval has elapsed.
    /// </summary>
    public CombatPullResult ResolveAssistPulse(
        CombatActorState player,
        CombatPullCandidate primary,
        IEnumerable<CombatPullCandidate> linkedCandidates,
        CombatZoneGate zoneGate,
        CombatPullEpisodeState episode,
        CombatTick tick,
        CombatPullTuning? tuning = null)
    {
        CombatArgumentNull.ThrowIfNull(player);
        CombatArgumentNull.ThrowIfNull(primary);
        CombatArgumentNull.ThrowIfNull(linkedCandidates);
        CombatArgumentNull.ThrowIfNull(zoneGate);
        CombatArgumentNull.ThrowIfNull(episode);

        var actualTuning = tuning ?? CombatPullTuning.T1Default;
        if (tick.ElapsedSeconds - episode.LastAssistPulseElapsedSeconds < actualTuning.SocialAssistPulseSeconds)
        {
            return new CombatPullResult(
                Succeeded: true,
                primary.Actor,
                Array.Empty<CombatActorState>(),
                episode,
                PlayerAttackEnabled: false,
                PresentationSignals: Array.Empty<CombatPullPresentationSignal>(),
                ForbiddenWarningSignals: Array.Empty<string>(),
                ScriptedEncounterTriggerStarted: false,
                Errors: Array.Empty<string>());
        }

        var assists = ResolveEligibleAssists(player, primary, linkedCandidates, zoneGate, episode)
            .Select(candidate => candidate.Actor.ClaimHostile(
                player.CombatActorId,
                candidate.SocialAssistProfile!.AssistThreatInitial,
                CombatState.InCombat))
            .ToArray();

        return new CombatPullResult(
            Succeeded: true,
            primary.Actor,
            assists,
            episode.WithJoinedActors(assists.Select(actor => actor.CombatActorId), tick.ElapsedSeconds),
            PlayerAttackEnabled: false,
            PresentationSignals: Array.Empty<CombatPullPresentationSignal>(),
            ForbiddenWarningSignals: Array.Empty<string>(),
            ScriptedEncounterTriggerStarted: false,
            Errors: Array.Empty<string>());
    }

    private static IReadOnlyList<string> ValidateBodyPull(
        CombatActorState player,
        CombatPoint3 playerPosition,
        CombatPullCandidate primary,
        CombatZoneGate zoneGate,
        CombatPullTuning tuning)
    {
        var errors = new List<string>();

        if (tuning.ProximityThreatInitial <= 0)
        {
            errors.Add("proximity_threat_initial must be positive.");
        }

        if (primary.Actor.ActorKind != CombatActorKind.NPC || !primary.Actor.IsAlive)
        {
            errors.Add("Body pull requires a live hostile NPC actor.");
        }

        if (!string.Equals(player.ZoneId, primary.Actor.ZoneId, StringComparison.Ordinal) ||
            !zoneGate.CanClaimHostileActor(primary.Actor.ZoneId))
        {
            errors.Add("Active zone does not allow hostile claim.");
        }

        if (playerPosition.DistanceTo(primary.Position) > primary.AggroRadiusMeters)
        {
            errors.Add("Player is outside aggro_radius_meters.");
        }

        if (!T1CombatLineOfSight.HasLineOfSight(primary.LosBlockingLayersToPlayer))
        {
            errors.Add("Body pull LoS is blocked.");
        }

        return errors;
    }

    private static IEnumerable<CombatPullCandidate> ResolveEligibleAssists(
        CombatActorState player,
        CombatPullCandidate primary,
        IEnumerable<CombatPullCandidate> linkedCandidates,
        CombatZoneGate zoneGate,
        CombatPullEpisodeState episode)
    {
        var primaryProfile = primary.SocialAssistProfile;
        if (primaryProfile is null)
        {
            return Array.Empty<CombatPullCandidate>();
        }

        return linkedCandidates
            .Where(candidate => IsEligibleAssist(player, primary, primaryProfile, candidate, zoneGate, episode))
            .OrderBy(candidate => primary.Position.DistanceMillimetersTo(candidate.Position))
            .ThenBy(candidate => candidate.SocialAssistProfile!.AssistOrderIndex)
            .ThenBy(candidate => candidate.Actor.CombatSortKey, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsEligibleAssist(
        CombatActorState player,
        CombatPullCandidate primary,
        CombatSocialAssistProfile primaryProfile,
        CombatPullCandidate candidate,
        CombatZoneGate zoneGate,
        CombatPullEpisodeState episode)
    {
        var profile = candidate.SocialAssistProfile;
        if (profile is null ||
            !candidate.Actor.IsAlive ||
            candidate.Actor.ActorKind != CombatActorKind.NPC ||
            episode.JoinedHostileActorIds.Contains(candidate.Actor.CombatActorId, StringComparer.Ordinal) ||
            !zoneGate.CanClaimHostileActor(candidate.Actor.ZoneId) ||
            !profile.AssistEnabled ||
            !string.Equals(profile.SocialLinkGroupId, primaryProfile.SocialLinkGroupId, StringComparison.Ordinal))
        {
            return false;
        }

        if (profile.AssistFactionFilter == CombatAssistFactionFilter.SameFactionOrExplicitAlly &&
            !string.Equals(candidate.Actor.FactionId, primary.Actor.FactionId, StringComparison.Ordinal))
        {
            return false;
        }

        if (profile.AssistEncounterFilter == CombatAssistEncounterFilter.SameEncounterOrSharedSocialGroup &&
            !SharesEncounterOrSocialGroup(primaryProfile, profile))
        {
            return false;
        }

        if (primary.Position.DistanceTo(candidate.Position) > profile.AssistRadiusMeters)
        {
            return false;
        }

        if (profile.AssistRequiresLosToPrimary && !T1CombatLineOfSight.HasLineOfSight(candidate.LosBlockingLayersToPrimary))
        {
            return false;
        }

        if (profile.AssistRequiresLosToTarget && !T1CombatLineOfSight.HasLineOfSight(candidate.LosBlockingLayersToPlayer))
        {
            return false;
        }

        return string.Equals(player.ZoneId, candidate.Actor.ZoneId, StringComparison.Ordinal);
    }

    private static bool SharesEncounterOrSocialGroup(CombatSocialAssistProfile primary, CombatSocialAssistProfile candidate)
    {
        return string.Equals(primary.SocialLinkGroupId, candidate.SocialLinkGroupId, StringComparison.Ordinal) ||
            (!string.IsNullOrWhiteSpace(primary.EncounterGroupId) &&
             string.Equals(primary.EncounterGroupId, candidate.EncounterGroupId, StringComparison.Ordinal));
    }

    private static CombatPullResult Failure(IReadOnlyList<string> errors)
    {
        return new CombatPullResult(
            Succeeded: false,
            PrimaryHostile: null,
            AssistingHostiles: Array.Empty<CombatActorState>(),
            Episode: null,
            PlayerAttackEnabled: false,
            PresentationSignals: Array.Empty<CombatPullPresentationSignal>(),
            ForbiddenWarningSignals: Array.Empty<string>(),
            ScriptedEncounterTriggerStarted: false,
            Errors: errors);
    }
}
