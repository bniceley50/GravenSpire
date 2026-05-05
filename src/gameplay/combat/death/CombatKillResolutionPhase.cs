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

public enum CombatKillCreditAcknowledgementStatus
{
    Acknowledged,
    Pending,
    Rejected
}

public sealed record CombatKillCreditAcknowledgement(
    string ConsumerName,
    CombatKillCreditAcknowledgementStatus Status,
    string? Diagnostic = null)
{
    public bool IsTerminal => Status is CombatKillCreditAcknowledgementStatus.Acknowledged or CombatKillCreditAcknowledgementStatus.Rejected;

    public static CombatKillCreditAcknowledgement Acknowledged(string consumerName, string? diagnostic = null)
    {
        return new CombatKillCreditAcknowledgement(consumerName, CombatKillCreditAcknowledgementStatus.Acknowledged, diagnostic);
    }

    public static CombatKillCreditAcknowledgement Pending(string consumerName, string? diagnostic = null)
    {
        return new CombatKillCreditAcknowledgement(consumerName, CombatKillCreditAcknowledgementStatus.Pending, diagnostic);
    }

    public static CombatKillCreditAcknowledgement Rejected(string consumerName, string diagnostic)
    {
        return new CombatKillCreditAcknowledgement(consumerName, CombatKillCreditAcknowledgementStatus.Rejected, diagnostic);
    }
}

public interface ICombatKillCreditAcknowledgementSink
{
    string ConsumerName { get; }

    CombatKillCreditAcknowledgement Acknowledge(PlayerKillCreditEvent killCreditEvent);
}

public sealed record CombatKillResolutionHoldStatus(
    string? HoldId,
    bool HasHeldKillCreditEvent,
    bool IsAcknowledged,
    IReadOnlyList<string> PendingAcknowledgements,
    PlayerKillCreditEvent? HeldKillCreditEvent)
{
    public static CombatKillResolutionHoldStatus NoHold { get; } = new(
        HoldId: null,
        HasHeldKillCreditEvent: false,
        IsAcknowledged: true,
        PendingAcknowledgements: Array.Empty<string>(),
        HeldKillCreditEvent: null);
}

public sealed record CombatKillResolutionAcknowledgedResult(
    CombatKillResolutionResult Resolution,
    CombatKillResolutionHoldStatus HoldStatus);

internal sealed class CombatKillResolutionHold
{
    public CombatKillResolutionHold(PlayerKillCreditEvent killCreditEvent, IEnumerable<string> pendingAcknowledgements)
    {
        KillCreditEvent = killCreditEvent;
        PendingAcknowledgements = new HashSet<string>(pendingAcknowledgements, StringComparer.Ordinal);
    }

    public PlayerKillCreditEvent KillCreditEvent { get; }

    public HashSet<string> PendingAcknowledgements { get; }
}

/// <summary>
/// Coordinates one Combat-owned NPC kill-resolution pass for a defeated source.
/// </summary>
public sealed class CombatKillResolutionPhase
{
    private readonly HashSet<string> processedDefeatedActorIds = new(StringComparer.Ordinal);
    private readonly Dictionary<string, CombatKillResolutionHold> heldKillCreditEmissions = new(StringComparer.Ordinal);

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

    public CombatKillResolutionAcknowledgedResult ResolveWithAcknowledgements(
        CombatKillResolutionRequest request,
        IEnumerable<ICombatKillCreditAcknowledgementSink> acknowledgementSinks)
    {
        ArgumentNullException.ThrowIfNull(acknowledgementSinks);

        var resolution = Resolve(request);
        if (resolution.KillCreditEvent is null)
        {
            return new CombatKillResolutionAcknowledgedResult(resolution, CombatKillResolutionHoldStatus.NoHold);
        }

        var holdId = CreateHoldId(resolution.KillCreditEvent);
        var pendingAcknowledgements = new List<string>();

        foreach (var sink in acknowledgementSinks)
        {
            ArgumentNullException.ThrowIfNull(sink);

            var acknowledgement = sink.Acknowledge(resolution.KillCreditEvent);
            var consumerName = string.IsNullOrWhiteSpace(acknowledgement.ConsumerName)
                ? sink.ConsumerName
                : acknowledgement.ConsumerName;

            if (!acknowledgement.IsTerminal)
            {
                pendingAcknowledgements.Add(consumerName);
            }
        }

        if (pendingAcknowledgements.Count == 0)
        {
            heldKillCreditEmissions.Remove(holdId);
            return new CombatKillResolutionAcknowledgedResult(
                resolution,
                new CombatKillResolutionHoldStatus(
                    holdId,
                    HasHeldKillCreditEvent: false,
                    IsAcknowledged: true,
                    PendingAcknowledgements: Array.Empty<string>(),
                    HeldKillCreditEvent: null));
        }

        var hold = new CombatKillResolutionHold(resolution.KillCreditEvent, pendingAcknowledgements);
        heldKillCreditEmissions[holdId] = hold;

        return new CombatKillResolutionAcknowledgedResult(resolution, CreateHoldStatus(holdId, hold));
    }

    public bool AcknowledgeHeldKillCredit(string holdId, string consumerName)
    {
        if (string.IsNullOrWhiteSpace(holdId))
        {
            throw new ArgumentException("hold_id is required.", nameof(holdId));
        }

        if (string.IsNullOrWhiteSpace(consumerName))
        {
            throw new ArgumentException("consumer_name is required.", nameof(consumerName));
        }

        if (!heldKillCreditEmissions.TryGetValue(holdId, out var hold))
        {
            return false;
        }

        hold.PendingAcknowledgements.Remove(consumerName);
        if (hold.PendingAcknowledgements.Count == 0)
        {
            heldKillCreditEmissions.Remove(holdId);
        }

        return true;
    }

    public CombatKillResolutionHoldStatus GetHeldKillCreditStatus(string holdId)
    {
        if (string.IsNullOrWhiteSpace(holdId))
        {
            throw new ArgumentException("hold_id is required.", nameof(holdId));
        }

        return heldKillCreditEmissions.TryGetValue(holdId, out var hold)
            ? CreateHoldStatus(holdId, hold)
            : new CombatKillResolutionHoldStatus(
                holdId,
                HasHeldKillCreditEvent: false,
                IsAcknowledged: true,
                PendingAcknowledgements: Array.Empty<string>(),
                HeldKillCreditEvent: null);
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

    private static CombatKillResolutionHoldStatus CreateHoldStatus(string holdId, CombatKillResolutionHold hold)
    {
        var pending = new List<string>(hold.PendingAcknowledgements);
        pending.Sort(StringComparer.Ordinal);

        return new CombatKillResolutionHoldStatus(
            holdId,
            HasHeldKillCreditEvent: true,
            IsAcknowledged: false,
            PendingAcknowledgements: pending,
            HeldKillCreditEvent: hold.KillCreditEvent);
    }

    private static string CreateHoldId(PlayerKillCreditEvent killCreditEvent)
    {
        var sourceRef = killCreditEvent.defeated_source_ref;
        if (!string.IsNullOrWhiteSpace(sourceRef.SourceNpcId))
        {
            return $"{killCreditEvent.zoneId}|source_npc_id:{sourceRef.SourceNpcId}";
        }

        if (sourceRef.SourceSpawnRef is not null)
        {
            return string.Join(
                "|",
                killCreditEvent.zoneId,
                "source_spawn_ref",
                sourceRef.SourceSpawnRef.SpawnTableId,
                sourceRef.SourceSpawnRef.SpawnAnchorId,
                sourceRef.SourceSpawnRef.NpcArchetypeId);
        }

        if (!string.IsNullOrWhiteSpace(sourceRef.SourceHazardId))
        {
            return $"{killCreditEvent.zoneId}|source_hazard_id:{sourceRef.SourceHazardId}";
        }

        return $"{killCreditEvent.zoneId}|local_character_id:{sourceRef.LocalCharacterId}";
    }
}
