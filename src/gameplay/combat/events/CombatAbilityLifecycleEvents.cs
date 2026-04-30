#nullable enable

using System.Collections.Generic;

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Shared payload for tactical ability lifecycle and result consumers.
/// </summary>
public interface ICombatAbilityLifecycleEvent
{
    string ActivationId { get; }

    string AbilityId { get; }

    string CasterCombatActorId { get; }

    string? TargetCombatActorId { get; }

    CombatTick Tick { get; }
}

/// <summary>
/// Emitted when a zero-cast-time tactical ability request passes validation and begins same-tick resolution.
/// </summary>
public sealed record AbilityActivatedEvent(
    string ActivationId,
    string AbilityId,
    string CasterCombatActorId,
    string? TargetCombatActorId,
    CombatTick Tick) : ICombatAbilityLifecycleEvent;

/// <summary>
/// Emitted when a tactical ability resolves and starts its transient cooldown.
/// </summary>
public sealed record AbilityResolvedEvent(
    string ActivationId,
    string AbilityId,
    string CasterCombatActorId,
    string? TargetCombatActorId,
    CombatTick Tick,
    int ManaSpent,
    long CooldownEndsTick,
    IReadOnlyList<CombatAppliedAbilityEffect> AppliedEffects) : ICombatAbilityLifecycleEvent;

/// <summary>
/// Emitted when Combat Core rejects a tactical ability request.
/// </summary>
public sealed record AbilityRejectedEvent(
    string ActivationId,
    string AbilityId,
    string CasterCombatActorId,
    string? TargetCombatActorId,
    CombatTick Tick,
    IReadOnlyList<string> RejectionReasons) : ICombatAbilityLifecycleEvent;
