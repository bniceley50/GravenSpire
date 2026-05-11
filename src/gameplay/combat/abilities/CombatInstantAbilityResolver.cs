#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gravenspire.Gameplay.Combat;

public enum CombatInstantAbilityOutcome
{
    Resolved,
    Rejected,
    OnCooldown
}

public sealed record CombatInstantAbilityRequest(
    string ActivationId,
    CombatActorState Caster,
    CombatActorState? Target,
    CombatZoneGate ZoneGate,
    double DistanceMetersToTarget,
    IReadOnlyList<CombatLosLayer> LosBlockingLayers,
    CombatTick Tick,
    int TickRateHz,
    CombatTacticalAbilityProfile Profile);

public sealed record CombatAppliedAbilityEffect(
    CombatTacticalAbilityEffectType EffectType,
    int? Damage,
    double? DurationSeconds,
    double? DamageReduction,
    double? InterruptSeconds);

public sealed record CombatInstantAbilityResult(
    CombatActorState Caster,
    CombatActorState? TargetAfterResolution,
    CombatInstantAbilityOutcome Outcome,
    IReadOnlyList<string> RejectionReasons,
    IReadOnlyList<ICombatAbilityLifecycleEvent> AbilityEvents,
    IReadOnlyList<ICombatCastLifecycleEvent> CastEvents,
    IReadOnlyList<CombatAppliedAbilityEffect> AppliedEffects,
    long? CooldownEndsTick,
    long? CooldownRemainingTicks)
{
    public bool Succeeded => Outcome == CombatInstantAbilityOutcome.Resolved;
}

/// <summary>
/// Resolves explicit zero-cast-time tactical ability activations against fixture-loaded profiles.
/// </summary>
public sealed class CombatInstantAbilityResolver
{
    private readonly Dictionary<CooldownKey, long> cooldownEndTicks = new();

    public CombatInstantAbilityResult Resolve(CombatInstantAbilityRequest request)
    {
        CombatArgumentNull.ThrowIfNull(request);
        CombatArgumentNull.ThrowIfNull(request.Caster);
        CombatArgumentNull.ThrowIfNull(request.ZoneGate);
        CombatArgumentNull.ThrowIfNull(request.LosBlockingLayers);
        CombatArgumentNull.ThrowIfNull(request.Profile);

        var errors = ValidateRequest(request);
        if (errors.Count > 0)
        {
            var rejected = new AbilityRejectedEvent(
                request.ActivationId,
                request.Profile.AbilityId,
                request.Caster.CombatActorId,
                request.Target?.CombatActorId,
                request.Tick,
                errors);
            return new CombatInstantAbilityResult(
                request.Caster,
                request.Target,
                CombatInstantAbilityOutcome.Rejected,
                errors,
                new ICombatAbilityLifecycleEvent[] { rejected },
                Array.Empty<ICombatCastLifecycleEvent>(),
                Array.Empty<CombatAppliedAbilityEffect>(),
                CooldownEndsTick: null,
                CooldownRemainingTicks: null);
        }

        var cooldownKey = new CooldownKey(request.Caster.CombatActorId, request.Profile.AbilityId);
        if (cooldownEndTicks.TryGetValue(cooldownKey, out var existingCooldownEndTick) &&
            request.Tick.Index < existingCooldownEndTick)
        {
            var remainingTicks = existingCooldownEndTick - request.Tick.Index;
            var rejected = new AbilityRejectedEvent(
                request.ActivationId,
                request.Profile.AbilityId,
                request.Caster.CombatActorId,
                request.Target?.CombatActorId,
                request.Tick,
                new[] { "Ability cooldown is still active." });
            return new CombatInstantAbilityResult(
                request.Caster,
                request.Target,
                CombatInstantAbilityOutcome.OnCooldown,
                new[] { "Ability cooldown is still active." },
                new ICombatAbilityLifecycleEvent[] { rejected },
                Array.Empty<ICombatCastLifecycleEvent>(),
                Array.Empty<CombatAppliedAbilityEffect>(),
                existingCooldownEndTick,
                remainingTicks);
        }

        var activated = new AbilityActivatedEvent(
            request.ActivationId,
            request.Profile.AbilityId,
            request.Caster.CombatActorId,
            request.Target?.CombatActorId,
            request.Tick);
        var abilityEvents = new List<ICombatAbilityLifecycleEvent> { activated };
        var castEvents = new List<ICombatCastLifecycleEvent>();
        var appliedEffects = new List<CombatAppliedAbilityEffect>();
        var caster = SpendAbilityResource(request.Caster, request.Profile);
        var target = request.Target;

        foreach (var effect in request.Profile.Effects)
        {
            switch (effect.EffectType)
            {
                case CombatTacticalAbilityEffectType.DirectDamage:
                    target = ApplyDirectDamage(target, effect);
                    appliedEffects.Add(new CombatAppliedAbilityEffect(effect.EffectType, effect.Damage, null, null, null));
                    break;
                case CombatTacticalAbilityEffectType.SelfBuff:
                    appliedEffects.Add(new CombatAppliedAbilityEffect(effect.EffectType, null, effect.DurationSeconds, effect.DamageReduction, null));
                    break;
                case CombatTacticalAbilityEffectType.InterruptCurrentChannel:
                    if (target is not null && target.CastRuntimeState == CombatCastRuntimeState.Casting)
                    {
                        var interruptSeconds = RequiredInterruptSeconds(effect);
                        var cancelled = new CastCancelledEvent(
                            target.ActiveCastId!,
                            target.ActiveCastSpellId!,
                            target.CombatActorId,
                            target.ActiveCastTargetCombatActorId,
                            request.Tick,
                            request.Profile.AbilityId);
                        target = target.CancelActiveChannelByAbility(interruptSeconds);
                        var recoveryStarted = new CastRecoveryStartedEvent(
                            target.ActiveCastId!,
                            target.ActiveCastSpellId!,
                            target.CombatActorId,
                            target.ActiveCastTargetCombatActorId,
                            request.Tick,
                            interruptSeconds);
                        castEvents.Add(cancelled);
                        castEvents.Add(recoveryStarted);
                    }

                    appliedEffects.Add(new CombatAppliedAbilityEffect(effect.EffectType, null, null, null, effect.InterruptSeconds));
                    break;
            }
        }

        var cooldownEndsTick = checked(
            request.Tick.Index + CombatCastFormulas.SecondsToTicksCeiling(request.Profile.CooldownSeconds, request.TickRateHz));
        cooldownEndTicks[cooldownKey] = cooldownEndsTick;
        var resolved = new AbilityResolvedEvent(
            request.ActivationId,
            request.Profile.AbilityId,
            request.Caster.CombatActorId,
            request.Target?.CombatActorId,
            request.Tick,
            request.Profile.CostMana,
            cooldownEndsTick,
            appliedEffects);
        abilityEvents.Add(resolved);

        return new CombatInstantAbilityResult(
            caster,
            target,
            CombatInstantAbilityOutcome.Resolved,
            Array.Empty<string>(),
            abilityEvents,
            castEvents,
            appliedEffects,
            cooldownEndsTick,
            CooldownRemainingTicks: null);
    }

    private static IReadOnlyList<string> ValidateRequest(CombatInstantAbilityRequest request)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.ActivationId))
        {
            errors.Add("activation_id is required.");
        }

        if (request.TickRateHz <= 0)
        {
            errors.Add("tick_rate_hz must be positive.");
        }

        if (request.Profile.CastTimeSeconds != 0)
        {
            errors.Add("Instant ability profile must declare zero cast time.");
        }

        if (request.Profile.CostMana < 0 ||
            request.Profile.CostEndurance < 0 ||
            request.Profile.CooldownSeconds <= 0 ||
            request.Profile.RangeMeters < 0)
        {
            errors.Add("ability cost, cooldown, and range must be valid.");
        }

        if (!request.Caster.IsAlive)
        {
            errors.Add("Caster must be alive.");
        }

        if (request.Caster.CastRuntimeState != CombatCastRuntimeState.None ||
            request.Caster.CombatState is CombatState.Casting or CombatState.Interrupted or CombatState.Recovery)
        {
            errors.Add("Caster is already casting or recovering.");
        }

        if (request.Profile.ResourceKind == CombatTacticalAbilityResourceKind.Magical &&
            request.Caster.CurrentMana < request.Profile.CostMana)
        {
            errors.Add("Caster does not have enough mana.");
        }

        if (request.Profile.ResourceKind == CombatTacticalAbilityResourceKind.Physical &&
            request.Caster.CurrentEndurance < request.Profile.CostEndurance)
        {
            errors.Add("Caster does not have enough Endurance.");
        }

        if (!request.ZoneGate.CanRunHostileCombat(request.Caster.ZoneId))
        {
            errors.Add("Active zone does not allow tactical abilities.");
        }

        if (request.Profile.RequiresTarget)
        {
            ValidateTarget(request, errors);
        }

        if (request.Profile.Effects.Count == 0)
        {
            errors.Add("Ability profile requires at least one declared effect.");
        }

        return errors;
    }

    private static void ValidateTarget(CombatInstantAbilityRequest request, ICollection<string> errors)
    {
        var target = request.Target;
        if (target is null)
        {
            errors.Add("Ability requires a valid target.");
            return;
        }

        if (!target.IsAlive)
        {
            errors.Add("Ability target must be alive.");
        }

        if (!string.Equals(request.Caster.ZoneId, target.ZoneId, StringComparison.Ordinal))
        {
            errors.Add("Ability target must be in the caster's zone.");
        }

        if (!string.Equals(request.Caster.TargetCombatActorId, target.CombatActorId, StringComparison.Ordinal))
        {
            errors.Add("Ability target must match the caster's selected target.");
        }

        if (!request.ZoneGate.CanRunHostileCombat(target.ZoneId))
        {
            errors.Add("Active zone does not allow tactical ability targets.");
        }

        if (request.DistanceMetersToTarget > request.Profile.RangeMeters)
        {
            errors.Add("Ability target is outside range_meters.");
        }

        if (request.Profile.RequiresLineOfSight &&
            !T1CombatLineOfSight.HasLineOfSight(request.LosBlockingLayers))
        {
            errors.Add("Target is not line-of-sight valid for tactical ability.");
        }
    }

    private static CombatActorState? ApplyDirectDamage(
        CombatActorState? target,
        CombatTacticalAbilityEffectProfile effect)
    {
        if (target is null)
        {
            return null;
        }

        return target.WithCurrentHealthAfterAbilityDamage(RequiredDamage(effect));
    }

    private static CombatActorState SpendAbilityResource(
        CombatActorState caster,
        CombatTacticalAbilityProfile profile)
    {
        return profile.ResourceKind == CombatTacticalAbilityResourceKind.Physical
            ? caster.WithCurrentEndurance(caster.CurrentEndurance - profile.CostEndurance)
            : caster.WithCurrentMana(caster.CurrentMana - profile.CostMana);
    }

    private static int RequiredDamage(CombatTacticalAbilityEffectProfile effect)
    {
        return effect.Damage
            ?? throw new InvalidOperationException("direct_damage effect is missing authored damage.");
    }

    private static double RequiredInterruptSeconds(CombatTacticalAbilityEffectProfile effect)
    {
        return effect.InterruptSeconds
            ?? throw new InvalidOperationException("interrupt_current_channel effect is missing authored interrupt timing.");
    }

    private readonly record struct CooldownKey(string CasterCombatActorId, string AbilityId);
}
