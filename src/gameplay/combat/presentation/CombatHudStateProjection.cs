#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gravenspire.Gameplay.Combat;

public enum CombatHudThreatCategory
{
    NoThreat,
    ThreatListed,
    ThreatClose,
    HasAggroStable,
    HasAggroContested
}

public enum CombatHudSwingReadinessCategory
{
    AttackOff,
    NoTarget,
    WaitingForWeaponDelay,
    Ready
}

public enum CombatHudCastCategory
{
    NotCasting,
    Casting,
    Interrupted,
    Recovery
}

public enum CombatHudEnduranceCategory
{
    Unavailable,
    Empty,
    Available,
    Full
}

public sealed record CombatHudResourceSnapshot(
    int Current,
    int Max);

public sealed record CombatHudTargetSnapshot(
    string TargetCombatActorId,
    int CurrentHealth,
    int MaxHealth,
    bool IsAlive,
    bool IsHostile);

public sealed record CombatHudCastSnapshot(
    CombatHudCastCategory Category,
    string? SpellId,
    double NormalizedProgress,
    double RecoverySecondsRemaining);

public sealed record CombatHudStateSnapshot(
    CombatHudResourceSnapshot Health,
    CombatHudResourceSnapshot Mana,
    CombatHudEnduranceCategory Endurance,
    CombatHudTargetSnapshot? Target,
    CombatHudCastSnapshot Cast,
    bool AttackOn,
    CombatHudSwingReadinessCategory NextSwingReadiness,
    CombatHudThreatCategory ThreatCategory,
    CombatState CombatState);

public sealed record CombatHudAttackStateSignal(
    bool AttackOn,
    string? TargetCombatActorId,
    CombatAttackTransitionPath TransitionPath,
    CombatTick Tick);

public sealed record CombatHudThreatCategoryTuning(
    double ThreatCloseRatio,
    double AggroContestedRatio);

public sealed record CombatHudThreatCategoryRequest(
    CombatActorState Actor,
    CombatActorState? HostileActor,
    IReadOnlyList<CombatActorState> CandidateThreatActors,
    CombatHudThreatCategoryTuning Tuning);

public sealed record CombatHudProjectionRequest(
    CombatActorState Player,
    CombatActorState? Target,
    CombatAttackStateSnapshot AttackState,
    CombatTick Tick,
    CombatHudThreatCategoryRequest? ThreatCategoryRequest = null,
    CombatCastProgressSnapshot? CastProgress = null);

/// <summary>
/// Projects Combat Core runtime state into HUD-safe primitives without owning any UI rendering.
/// </summary>
public static class CombatHudStateProjection
{
    public static CombatHudStateSnapshot Project(CombatHudProjectionRequest request)
    {
        CombatArgumentNull.ThrowIfNull(request);
        CombatArgumentNull.ThrowIfNull(request.Player);
        CombatArgumentNull.ThrowIfNull(request.AttackState);

        return new CombatHudStateSnapshot(
            new CombatHudResourceSnapshot(request.Player.CurrentHealth, request.Player.MaxHealth),
            new CombatHudResourceSnapshot(request.Player.CurrentMana, request.Player.MaxMana),
            ProjectEndurance(request.Player),
            ProjectTarget(request.Target),
            ProjectCast(request.Player, request.CastProgress),
            request.AttackState.IsAttackOn,
            ProjectSwingReadiness(request.AttackState, request.Tick),
            request.ThreatCategoryRequest is null
                ? CombatHudThreatCategory.NoThreat
                : EvaluateThreatCategory(request.ThreatCategoryRequest),
            request.Player.CombatState);
    }

    public static CombatHudAttackStateSignal? ProjectAttackSignal(CombatAttackTransitionResult transition)
    {
        CombatArgumentNull.ThrowIfNull(transition);
        return transition.StateChangedSignal is null
            ? null
            : ProjectAttackSignal(transition.StateChangedSignal);
    }

    public static IReadOnlyList<CombatHudAttackStateSignal> ProjectAttackSignals(
        IEnumerable<CombatAttackStateChangedSignal> signals)
    {
        CombatArgumentNull.ThrowIfNull(signals);
        return new ReadOnlyCollection<CombatHudAttackStateSignal>(
            signals.Select(ProjectAttackSignal).ToArray());
    }

    public static CombatHudThreatCategory EvaluateThreatCategory(CombatHudThreatCategoryRequest request)
    {
        CombatArgumentNull.ThrowIfNull(request);
        CombatArgumentNull.ThrowIfNull(request.Actor);
        CombatArgumentNull.ThrowIfNull(request.CandidateThreatActors);
        ValidateTuning(request.Tuning);

        var hostile = request.HostileActor;
        if (hostile is null || hostile.ActorKind != CombatActorKind.NPC || !hostile.IsAlive)
        {
            return CombatHudThreatCategory.NoThreat;
        }

        if (hostile.ThreatTable.Any(entry => entry.Value < 0))
        {
            throw new InvalidOperationException("HUD threat category cannot be projected from negative threat entries.");
        }

        var validActorIds = request.CandidateThreatActors
            .Where(actor => actor.IsAlive)
            .Where(actor => string.Equals(actor.ZoneId, hostile.ZoneId, StringComparison.Ordinal))
            .Select(actor => actor.CombatActorId)
            .ToHashSet(StringComparer.Ordinal);

        var validEntries = hostile.ThreatTable
            .Where(entry => entry.Value > 0)
            .Where(entry => validActorIds.Contains(entry.Key))
            .OrderByDescending(entry => entry.Value)
            .ThenBy(entry => entry.Key, StringComparer.Ordinal)
            .ToArray();

        if (validEntries.Length == 0 ||
            !hostile.ThreatTable.TryGetValue(request.Actor.CombatActorId, out var actorThreat) ||
            actorThreat <= 0 ||
            !validActorIds.Contains(request.Actor.CombatActorId))
        {
            return CombatHudThreatCategory.NoThreat;
        }

        var topThreat = validEntries[0].Value;
        if (topThreat <= 0)
        {
            return CombatHudThreatCategory.NoThreat;
        }

        var actorIsCurrentTarget = string.Equals(
            hostile.TargetCombatActorId,
            request.Actor.CombatActorId,
            StringComparison.Ordinal);

        if (actorIsCurrentTarget && actorThreat >= topThreat)
        {
            var secondHighest = validEntries
                .Where(entry => !string.Equals(entry.Key, request.Actor.CombatActorId, StringComparison.Ordinal))
                .Select(entry => entry.Value)
                .DefaultIfEmpty(0)
                .Max();
            var contestedRatio = secondHighest / (double)topThreat;
            return contestedRatio >= request.Tuning.AggroContestedRatio
                ? CombatHudThreatCategory.HasAggroContested
                : CombatHudThreatCategory.HasAggroStable;
        }

        var listedRatio = actorThreat / (double)topThreat;
        return listedRatio >= request.Tuning.ThreatCloseRatio
            ? CombatHudThreatCategory.ThreatClose
            : CombatHudThreatCategory.ThreatListed;
    }

    private static CombatHudAttackStateSignal ProjectAttackSignal(CombatAttackStateChangedSignal signal)
    {
        return new CombatHudAttackStateSignal(
            signal.AttackOn,
            signal.TargetCombatActorId,
            signal.TransitionPath,
            signal.Tick);
    }

    private static CombatHudTargetSnapshot? ProjectTarget(CombatActorState? target)
    {
        return target is null
            ? null
            : new CombatHudTargetSnapshot(
                target.CombatActorId,
                target.CurrentHealth,
                target.MaxHealth,
                target.IsAlive,
                target.ActorKind == CombatActorKind.NPC);
    }

    private static CombatHudEnduranceCategory ProjectEndurance(CombatActorState player)
    {
        if (player.MaxEndurance <= 0)
        {
            return CombatHudEnduranceCategory.Unavailable;
        }

        if (player.CurrentEndurance <= 0)
        {
            return CombatHudEnduranceCategory.Empty;
        }

        return player.CurrentEndurance >= player.MaxEndurance
            ? CombatHudEnduranceCategory.Full
            : CombatHudEnduranceCategory.Available;
    }

    private static CombatHudCastSnapshot ProjectCast(
        CombatActorState player,
        CombatCastProgressSnapshot? progress)
    {
        var category = player.CastRuntimeState switch
        {
            CombatCastRuntimeState.Casting => CombatHudCastCategory.Casting,
            CombatCastRuntimeState.Interrupted => CombatHudCastCategory.Interrupted,
            CombatCastRuntimeState.Recovery => CombatHudCastCategory.Recovery,
            _ => CombatHudCastCategory.NotCasting
        };

        return new CombatHudCastSnapshot(
            category,
            progress?.SpellId ?? player.ActiveCastSpellId,
            progress?.NormalizedProgress ?? 0d,
            player.CastRecoveryRemainingSeconds);
    }

    private static CombatHudSwingReadinessCategory ProjectSwingReadiness(
        CombatAttackStateSnapshot attackState,
        CombatTick tick)
    {
        if (!attackState.IsAttackOn)
        {
            return CombatHudSwingReadinessCategory.AttackOff;
        }

        if (string.IsNullOrWhiteSpace(attackState.TargetCombatActorId))
        {
            return CombatHudSwingReadinessCategory.NoTarget;
        }

        return attackState.NextSwingDueTick is not null &&
            tick.Index >= attackState.NextSwingDueTick.Value
                ? CombatHudSwingReadinessCategory.Ready
                : CombatHudSwingReadinessCategory.WaitingForWeaponDelay;
    }

    private static void ValidateTuning(CombatHudThreatCategoryTuning tuning)
    {
        CombatArgumentNull.ThrowIfNull(tuning);
        if (tuning.ThreatCloseRatio <= 0d || tuning.ThreatCloseRatio > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(tuning), "threat_close_ratio must be a ratio.");
        }

        if (tuning.AggroContestedRatio <= 0d || tuning.AggroContestedRatio > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(tuning), "aggro_contested_ratio must be a ratio.");
        }
    }
}
