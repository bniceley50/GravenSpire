#nullable enable

using System;
using System.Collections.Generic;

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Copy helpers for immutable CombatActorState transitions used by early T1 domain systems.
/// </summary>
public static class CombatActorStateTransitions
{
    /// <summary>
    /// Returns a copy with a new combat state.
    /// </summary>
    public static CombatActorState WithCombatState(this CombatActorState actor, CombatState combatState)
    {
        ArgumentNullException.ThrowIfNull(actor);
        return Copy(actor, combatState: combatState);
    }

    /// <summary>
    /// Returns a copy with a new transient target id.
    /// </summary>
    public static CombatActorState WithTarget(this CombatActorState actor, string? targetCombatActorId)
    {
        ArgumentNullException.ThrowIfNull(actor);
        return Copy(actor, targetCombatActorId: targetCombatActorId, replaceTarget: true);
    }

    /// <summary>
    /// Returns a copy with an exact threat value for the supplied runtime actor id.
    /// </summary>
    public static CombatActorState SetThreat(this CombatActorState actor, string sourceCombatActorId, int threat)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (string.IsNullOrWhiteSpace(sourceCombatActorId))
        {
            throw new ArgumentException("Threat source combat actor id is required.", nameof(sourceCombatActorId));
        }

        if (threat < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(threat), "Threat cannot be negative.");
        }

        var threatTable = new Dictionary<string, int>(actor.ThreatTable, StringComparer.Ordinal)
        {
            [sourceCombatActorId] = threat
        };

        return Copy(actor, threatTable: threatTable);
    }

    /// <summary>
    /// Returns a copy with added threat for the supplied runtime actor id.
    /// </summary>
    public static CombatActorState AddThreat(this CombatActorState actor, string sourceCombatActorId, int threatDelta)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (string.IsNullOrWhiteSpace(sourceCombatActorId))
        {
            throw new ArgumentException("Threat source combat actor id is required.", nameof(sourceCombatActorId));
        }

        if (threatDelta < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(threatDelta), "Threat delta cannot be negative.");
        }

        var current = actor.ThreatTable.TryGetValue(sourceCombatActorId, out var existingThreat) ? existingThreat : 0;
        return actor.SetThreat(sourceCombatActorId, checked(current + threatDelta));
    }

    /// <summary>
    /// Claims a hostile actor for combat against a target without enabling player Attack.
    /// </summary>
    public static CombatActorState ClaimHostile(
        this CombatActorState actor,
        string targetCombatActorId,
        int initialThreat,
        CombatState claimState = CombatState.Pulling)
    {
        if (actor.ActorKind != CombatActorKind.NPC)
        {
            throw new InvalidOperationException("Only hostile NPC combat actors can be claimed by pull/assist logic.");
        }

        return actor
            .WithTarget(targetCombatActorId)
            .WithCombatState(claimState)
            .SetThreat(targetCombatActorId, initialThreat);
    }

    /// <summary>
    /// Releases a hostile actor from the current pull episode and clears transient targeting/threat.
    /// </summary>
    public static CombatActorState ReleaseHostile(this CombatActorState actor, CombatState releaseState = CombatState.OutOfCombat)
    {
        ArgumentNullException.ThrowIfNull(actor);
        return Copy(actor, combatState: releaseState, targetCombatActorId: null, replaceTarget: true, threatTable: new Dictionary<string, int>(StringComparer.Ordinal));
    }

    /// <summary>
    /// Clears transient target and threat state without changing life state or authored identity.
    /// </summary>
    public static CombatActorState ClearTargetAndThreat(this CombatActorState actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        return Copy(actor, targetCombatActorId: null, replaceTarget: true, threatTable: new Dictionary<string, int>(StringComparer.Ordinal));
    }

    /// <summary>
    /// Begins a Combat-owned slow cast on the actor.
    /// </summary>
    public static CombatActorState BeginCast(
        this CombatActorState actor,
        string activeCastId,
        string spellId,
        string? targetCombatActorId)
    {
        ArgumentNullException.ThrowIfNull(actor);
        RequireText(activeCastId, nameof(activeCastId));
        RequireText(spellId, nameof(spellId));

        return actor.WithCombatState(CombatState.Casting) with
        {
            CastRuntimeState = CombatCastRuntimeState.Casting,
            ActiveCastId = activeCastId,
            ActiveCastSpellId = spellId,
            ActiveCastTargetCombatActorId = targetCombatActorId,
            CastProgressSeconds = 0d,
            CastRecoveryRemainingSeconds = 0d
        };
    }

    /// <summary>
    /// Updates cast progress in Combat-owned simulation seconds.
    /// </summary>
    public static CombatActorState WithCastProgress(this CombatActorState actor, double castProgressSeconds)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (castProgressSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(castProgressSeconds), "Cast progress cannot be negative.");
        }

        return actor with { CastProgressSeconds = castProgressSeconds };
    }

    /// <summary>
    /// Marks the active cast as interrupted before recovery begins.
    /// </summary>
    public static CombatActorState MarkCastInterrupted(this CombatActorState actor, double recoveryRemainingSeconds)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (recoveryRemainingSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(recoveryRemainingSeconds), "Recovery remaining cannot be negative.");
        }

        return actor.WithCombatState(CombatState.Interrupted) with
        {
            CastRuntimeState = CombatCastRuntimeState.Interrupted,
            ActiveCastId = actor.ActiveCastId,
            ActiveCastSpellId = actor.ActiveCastSpellId,
            ActiveCastTargetCombatActorId = actor.ActiveCastTargetCombatActorId,
            CastProgressSeconds = actor.CastProgressSeconds,
            CastRecoveryRemainingSeconds = recoveryRemainingSeconds
        };
    }

    /// <summary>
    /// Moves the active cast runtime into post-cast, post-cancel, or post-interrupt recovery.
    /// </summary>
    public static CombatActorState BeginCastRecovery(this CombatActorState actor, double recoveryRemainingSeconds)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (recoveryRemainingSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(recoveryRemainingSeconds), "Recovery remaining cannot be negative.");
        }

        return actor.WithCombatState(CombatState.Recovery) with
        {
            CastRuntimeState = CombatCastRuntimeState.Recovery,
            ActiveCastId = actor.ActiveCastId,
            ActiveCastSpellId = actor.ActiveCastSpellId,
            ActiveCastTargetCombatActorId = actor.ActiveCastTargetCombatActorId,
            CastProgressSeconds = actor.CastProgressSeconds,
            CastRecoveryRemainingSeconds = recoveryRemainingSeconds
        };
    }

    /// <summary>
    /// Updates current mana while preserving any Combat-owned cast runtime fields.
    /// </summary>
    public static CombatActorState WithCurrentMana(this CombatActorState actor, int currentMana)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (currentMana < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentMana), "Current mana cannot be negative.");
        }

        if (currentMana > actor.MaxMana)
        {
            throw new ArgumentOutOfRangeException(nameof(currentMana), "Current mana cannot exceed max mana.");
        }

        return CopyWithCastRuntime(actor, currentMana: currentMana);
    }

    /// <summary>
    /// Clears Combat-owned cast runtime fields after recovery has ended.
    /// </summary>
    public static CombatActorState ClearCastRuntime(this CombatActorState actor, CombatState nextCombatState)
    {
        ArgumentNullException.ThrowIfNull(actor);
        return actor.WithCombatState(nextCombatState) with
        {
            CastRuntimeState = CombatCastRuntimeState.None,
            ActiveCastId = null,
            ActiveCastSpellId = null,
            ActiveCastTargetCombatActorId = null,
            CastProgressSeconds = 0d,
            CastRecoveryRemainingSeconds = 0d
        };
    }

    private static CombatActorState CopyWithCastRuntime(CombatActorState actor, int? currentMana = null)
    {
        return new CombatActorState(
            actor.CombatActorId,
            actor.ActorKind,
            actor.StableSourceRef,
            actor.FactionId,
            actor.ZoneId,
            actor.Level,
            actor.MaxHealth,
            actor.CurrentHealth,
            actor.MaxMana,
            currentMana ?? actor.CurrentMana,
            actor.ArmorClass,
            actor.AttackPower,
            actor.WeaponBaseDamage,
            actor.AttackSkill,
            actor.DefenseSkill,
            actor.WeaponDelaySeconds,
            actor.MeleeRangeMeters,
            actor.SpellRangeMeters,
            actor.CombatState,
            actor.LifeState,
            actor.TargetCombatActorId,
            actor.CombatSortKey,
            actor.ThreatTable) with
        {
            CastRuntimeState = actor.CastRuntimeState,
            ActiveCastId = actor.ActiveCastId,
            ActiveCastSpellId = actor.ActiveCastSpellId,
            ActiveCastTargetCombatActorId = actor.ActiveCastTargetCombatActorId,
            CastProgressSeconds = actor.CastProgressSeconds,
            CastRecoveryRemainingSeconds = actor.CastRecoveryRemainingSeconds
        };
    }

    private static void RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Cast runtime identifiers cannot be empty.", parameterName);
        }
    }

    private static CombatActorState Copy(
        CombatActorState actor,
        CombatState? combatState = null,
        string? targetCombatActorId = null,
        bool replaceTarget = false,
        IReadOnlyDictionary<string, int>? threatTable = null)
    {
        return new CombatActorState(
            actor.CombatActorId,
            actor.ActorKind,
            actor.StableSourceRef,
            actor.FactionId,
            actor.ZoneId,
            actor.Level,
            actor.MaxHealth,
            actor.CurrentHealth,
            actor.MaxMana,
            actor.CurrentMana,
            actor.ArmorClass,
            actor.AttackPower,
            actor.WeaponBaseDamage,
            actor.AttackSkill,
            actor.DefenseSkill,
            actor.WeaponDelaySeconds,
            actor.MeleeRangeMeters,
            actor.SpellRangeMeters,
            combatState ?? actor.CombatState,
            actor.LifeState,
            replaceTarget ? targetCombatActorId : actor.TargetCombatActorId,
            actor.CombatSortKey,
            threatTable ?? actor.ThreatTable);
    }
}
