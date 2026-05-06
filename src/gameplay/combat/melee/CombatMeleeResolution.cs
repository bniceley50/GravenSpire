#nullable enable

using System;
using System.Collections.Generic;

namespace Gravenspire.Gameplay.Combat;

public enum CombatMeleeTickOutcome
{
    NotDue,
    AttackOff,
    TargetInvalid,
    TargetAlreadyDead,
    ZoneBlocked,
    OutOfRange,
    FacingBlocked,
    LineOfSightBlocked,
    Miss,
    Hit
}

public interface ICombatMeleeRandomSource
{
    double NextHitRoll();

    double NextDamageRollScalar();
}

public sealed record CombatMeleeTickRequest(
    CombatActorState Attacker,
    CombatActorState? Target,
    CombatAttackStateSnapshot AttackState,
    CombatZoneGate ZoneGate,
    double DistanceMetersToTarget,
    double FacingDegreesToTarget,
    double FacingToleranceDegrees,
    IReadOnlyList<CombatLosLayer> LosBlockingLayers,
    CombatTick Tick,
    int TickRateHz,
    CombatMeleeHitChanceTuning HitChanceTuning,
    CombatMeleeDamageTuning DamageTuning,
    ICombatMeleeRandomSource RandomSource,
    bool TargetDeathResolvedBeforeSwing = false);

public sealed record CombatMeleeTickResult(
    CombatMeleeTickOutcome Outcome,
    CombatTick Tick,
    long? NextSwingDueTick,
    double? HitChance,
    double? HitRoll,
    double? DamageRollScalar,
    int Damage,
    CombatActorState? TargetAfterResolution,
    bool ShouldForceAttackOff,
    CombatAttackTransitionPath? AttackOffPath)
{
    public bool AppliedDamage => Damage > 0;
}

/// <summary>
/// Resolves one eligible weapon-delay tick after transition-priority checks.
/// </summary>
public sealed class CombatMeleeResolver
{
    public CombatMeleeTickResult ResolveTick(CombatMeleeTickRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Attacker);
        ArgumentNullException.ThrowIfNull(request.AttackState);
        ArgumentNullException.ThrowIfNull(request.ZoneGate);
        ArgumentNullException.ThrowIfNull(request.LosBlockingLayers);
        ArgumentNullException.ThrowIfNull(request.HitChanceTuning);
        ArgumentNullException.ThrowIfNull(request.DamageTuning);
        ArgumentNullException.ThrowIfNull(request.RandomSource);

        if (request.TickRateHz <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "tick_rate_hz must be positive.");
        }

        if (request.FacingToleranceDegrees < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "facing_tolerance_degrees must not be negative.");
        }

        if (!request.AttackState.IsAttackOn || request.AttackState.NextSwingDueTick is null)
        {
            return NoDamage(CombatMeleeTickOutcome.AttackOff, request, nextSwingDueTick: null);
        }

        if (request.Tick.Index < request.AttackState.NextSwingDueTick.Value)
        {
            return NoDamage(CombatMeleeTickOutcome.NotDue, request, request.AttackState.NextSwingDueTick);
        }

        var target = request.Target;
        if (target is null ||
            !string.Equals(request.AttackState.TargetCombatActorId, target.CombatActorId, StringComparison.Ordinal))
        {
            return NoDamage(CombatMeleeTickOutcome.TargetInvalid, request, nextSwingDueTick: null);
        }

        if (request.TargetDeathResolvedBeforeSwing || !target.IsAlive)
        {
            return NoDamage(
                CombatMeleeTickOutcome.TargetAlreadyDead,
                request,
                nextSwingDueTick: null,
                shouldForceAttackOff: true,
                attackOffPath: CombatAttackTransitionPath.TargetDeath);
        }

        if (!request.Attacker.IsAlive)
        {
            return NoDamage(
                CombatMeleeTickOutcome.TargetInvalid,
                request,
                nextSwingDueTick: null,
                shouldForceAttackOff: true,
                attackOffPath: CombatAttackTransitionPath.PlayerDeath);
        }

        if (!string.Equals(request.Attacker.ZoneId, target.ZoneId, StringComparison.Ordinal) ||
            !request.ZoneGate.CanApplyDamage(request.Attacker.ZoneId) ||
            !request.ZoneGate.CanApplyDamage(target.ZoneId))
        {
            return NoDamage(
                CombatMeleeTickOutcome.ZoneBlocked,
                request,
                nextSwingDueTick: null,
                shouldForceAttackOff: true,
                attackOffPath: CombatAttackTransitionPath.ZoneTransition);
        }

        var nextSwingDueTick = NextSwingDueTick(request);

        if (request.DistanceMetersToTarget > request.Attacker.MeleeRangeMeters)
        {
            return NoDamage(CombatMeleeTickOutcome.OutOfRange, request, nextSwingDueTick);
        }

        if (Math.Abs(request.FacingDegreesToTarget) > request.FacingToleranceDegrees)
        {
            return NoDamage(CombatMeleeTickOutcome.FacingBlocked, request, nextSwingDueTick);
        }

        if (!T1CombatLineOfSight.HasLineOfSight(request.LosBlockingLayers))
        {
            return NoDamage(CombatMeleeTickOutcome.LineOfSightBlocked, request, nextSwingDueTick);
        }

        var hitChance = CombatMeleeFormulas.CalculateHitChance(request.Attacker, target, request.HitChanceTuning);
        var hitRoll = request.RandomSource.NextHitRoll();
        if (hitRoll < 0 || hitRoll > 1)
        {
            throw new InvalidOperationException("Injected hit roll must be a ratio.");
        }

        if (hitRoll > hitChance)
        {
            return new CombatMeleeTickResult(
                CombatMeleeTickOutcome.Miss,
                request.Tick,
                nextSwingDueTick,
                hitChance,
                hitRoll,
                DamageRollScalar: null,
                Damage: 0,
                TargetAfterResolution: target,
                ShouldForceAttackOff: false,
                AttackOffPath: null);
        }

        var damageRollScalar = request.RandomSource.NextDamageRollScalar();
        var damage = CombatMeleeFormulas.CalculateDamage(request.Attacker, target, request.DamageTuning, damageRollScalar);
        return new CombatMeleeTickResult(
            CombatMeleeTickOutcome.Hit,
            request.Tick,
            nextSwingDueTick,
            hitChance,
            hitRoll,
            damageRollScalar,
            damage,
            ApplyDamage(target, damage),
            ShouldForceAttackOff: false,
            AttackOffPath: null);
    }

    private static CombatMeleeTickResult NoDamage(
        CombatMeleeTickOutcome outcome,
        CombatMeleeTickRequest request,
        long? nextSwingDueTick,
        bool shouldForceAttackOff = false,
        CombatAttackTransitionPath? attackOffPath = null)
    {
        return new CombatMeleeTickResult(
            outcome,
            request.Tick,
            nextSwingDueTick,
            HitChance: null,
            HitRoll: null,
            DamageRollScalar: null,
            Damage: 0,
            TargetAfterResolution: request.Target,
            shouldForceAttackOff,
            attackOffPath);
    }

    private static long NextSwingDueTick(CombatMeleeTickRequest request)
    {
        var ticksUntilSwing = Math.Max(1, (long)Math.Ceiling(request.Attacker.WeaponDelaySeconds * request.TickRateHz));
        return checked(request.Tick.Index + ticksUntilSwing);
    }

    private static CombatActorState ApplyDamage(CombatActorState target, int damage)
    {
        var currentHealth = Math.Max(0, target.CurrentHealth - damage);
        var lifeState = currentHealth == 0 ? CombatActorLifeState.Dead : target.LifeState;
        var combatState = currentHealth == 0 ? CombatState.Dead : target.CombatState;

        return new CombatActorState(
            target.CombatActorId,
            target.ActorKind,
            target.StableSourceRef,
            target.FactionId,
            target.ZoneId,
            target.Level,
            target.MaxHealth,
            currentHealth,
            target.MaxMana,
            target.CurrentMana,
            target.ArmorClass,
            target.AttackPower,
            target.WeaponBaseDamage,
            target.AttackSkill,
            target.DefenseSkill,
            target.WeaponDelaySeconds,
            target.MeleeRangeMeters,
            target.SpellRangeMeters,
            combatState,
            lifeState,
            target.TargetCombatActorId,
            target.CombatSortKey,
            target.ThreatTable,
            maxEndurance: target.MaxEndurance,
            currentEndurance: target.CurrentEndurance);
    }
}
