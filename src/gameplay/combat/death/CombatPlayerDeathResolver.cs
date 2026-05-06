#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Gravenspire.Gameplay.Combat;

public sealed record CombatPlayerDeathResolutionRequest(
    CombatActorState Player,
    int IncomingDamage,
    CombatPoint3 DeathPosition,
    CombatStableSourceRef KillerSourceRef,
    string DeathCauseType);

public sealed record CombatPlayerDeathResolutionResult(
    bool Processed,
    CombatActorState PlayerAfterResolution,
    PlayerDeathEvent? DeathEvent);

/// <summary>
/// Resolves the Combat-owned local-player lethal transition and emits one narrow death handoff signal.
/// </summary>
public sealed class CombatPlayerDeathResolver
{
    private readonly HashSet<string> processedDeathContextIds = new(StringComparer.Ordinal);

    public CombatPlayerDeathResolutionResult Resolve(CombatPlayerDeathResolutionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Player);
        ArgumentNullException.ThrowIfNull(request.KillerSourceRef);

        ValidateRequest(request);

        var player = request.Player;
        if (player.ActorKind != CombatActorKind.Player)
        {
            throw new InvalidOperationException("Player death resolution requires a player actor.");
        }

        var localCharacterId = RequireLocalCharacterId(player);
        if (!player.IsAlive || player.CurrentHealth <= default(int))
        {
            return new CombatPlayerDeathResolutionResult(Processed: false, player, DeathEvent: null);
        }

        var currentHealth = Math.Max(default(int), player.CurrentHealth - request.IncomingDamage);
        if (currentHealth > default(int))
        {
            return new CombatPlayerDeathResolutionResult(Processed: false, player, DeathEvent: null);
        }

        var deathCauseType = request.DeathCauseType.Trim();
        var deathContextId = CreateDeathContextId(
            localCharacterId,
            player.ZoneId,
            request.DeathPosition,
            request.KillerSourceRef,
            deathCauseType);

        if (!processedDeathContextIds.Add(deathContextId))
        {
            return new CombatPlayerDeathResolutionResult(Processed: false, player, DeathEvent: null);
        }

        var deathEvent = new PlayerDeathEvent(
            deathContextId,
            localCharacterId,
            player.ZoneId,
            request.DeathPosition,
            request.KillerSourceRef,
            deathCauseType);

        return new CombatPlayerDeathResolutionResult(
            Processed: true,
            TransitionPlayerToDead(player),
            deathEvent);
    }

    public static string CreateDeathContextId(
        string localCharacterId,
        string zoneId,
        CombatPoint3 deathPosition,
        CombatStableSourceRef killerSourceRef,
        string deathCauseType)
    {
        ArgumentNullException.ThrowIfNull(killerSourceRef);
        var canonical = string.Join(
            "|",
            RequireText(localCharacterId, nameof(localCharacterId)),
            RequireText(zoneId, nameof(zoneId)),
            Canonicalize(deathPosition),
            Canonicalize(killerSourceRef),
            RequireText(deathCauseType, nameof(deathCauseType)));

        return $"DCTX-{ToBase64Url(canonical)}";
    }

    private static CombatActorState TransitionPlayerToDead(CombatActorState player)
    {
        return new CombatActorState(
            player.CombatActorId,
            player.ActorKind,
            player.StableSourceRef,
            player.FactionId,
            player.ZoneId,
            player.Level,
            player.MaxHealth,
            currentHealth: default,
            player.MaxMana,
            player.CurrentMana,
            player.ArmorClass,
            player.AttackPower,
            player.WeaponBaseDamage,
            player.AttackSkill,
            player.DefenseSkill,
            player.WeaponDelaySeconds,
            player.MeleeRangeMeters,
            player.SpellRangeMeters,
            CombatState.Dead,
            CombatActorLifeState.Dead,
            targetCombatActorId: null,
            player.CombatSortKey,
            threatTable: new Dictionary<string, int>(StringComparer.Ordinal),
            maxEndurance: player.MaxEndurance,
            currentEndurance: player.CurrentEndurance);
    }

    private static void ValidateRequest(CombatPlayerDeathResolutionRequest request)
    {
        if (request.IncomingDamage < default(int))
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Incoming damage must not be negative.");
        }

        RequireText(request.DeathCauseType, nameof(request.DeathCauseType));
    }

    private static string RequireLocalCharacterId(CombatActorState player)
    {
        return RequireText(player.StableSourceRef.LocalCharacterId ?? string.Empty, "local_character_id");
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Required text value cannot be empty.", parameterName);
        }

        return value.Trim();
    }

    private static string Canonicalize(CombatPoint3 point)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{point.X.ToString("R", CultureInfo.InvariantCulture)},{point.Y.ToString("R", CultureInfo.InvariantCulture)},{point.Z.ToString("R", CultureInfo.InvariantCulture)}");
    }

    private static string Canonicalize(CombatStableSourceRef sourceRef)
    {
        if (!string.IsNullOrWhiteSpace(sourceRef.LocalCharacterId))
        {
            return $"local_character:{sourceRef.LocalCharacterId}";
        }

        if (!string.IsNullOrWhiteSpace(sourceRef.SourceNpcId))
        {
            return $"source_npc:{sourceRef.SourceNpcId}";
        }

        if (sourceRef.SourceSpawnRef is not null)
        {
            return string.Join(
                ":",
                "source_spawn",
                sourceRef.SourceSpawnRef.SpawnTableId,
                sourceRef.SourceSpawnRef.SpawnAnchorId,
                sourceRef.SourceSpawnRef.NpcArchetypeId);
        }

        if (!string.IsNullOrWhiteSpace(sourceRef.SourceHazardId))
        {
            return $"source_hazard:{sourceRef.SourceHazardId}";
        }

        throw new ArgumentException("Killer source reference must contain one stable identity.", nameof(sourceRef));
    }

    private static string ToBase64Url(string value)
    {
        return Convert.ToBase64String(Encoding.Unicode.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
