#nullable enable

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Shared payload required by downstream spell execution and HUD lifecycle consumers.
/// </summary>
public interface ICombatCastLifecycleEvent
{
    string CastId { get; }

    string SpellId { get; }

    string CasterCombatActorId { get; }

    string? TargetCombatActorId { get; }

    CombatTick Tick { get; }
}

/// <summary>
/// Emitted when a valid slow cast enters Casting.
/// </summary>
public sealed record CastStartedEvent(
    string CastId,
    string SpellId,
    string CasterCombatActorId,
    string? TargetCombatActorId,
    CombatTick Tick) : ICombatCastLifecycleEvent;

/// <summary>
/// Emitted when a cast reaches completion and spends mana.
/// </summary>
public sealed record CastCompletedEvent(
    string CastId,
    string SpellId,
    string CasterCombatActorId,
    string? TargetCombatActorId,
    CombatTick Tick,
    int ManaSpent) : ICombatCastLifecycleEvent;

/// <summary>
/// Emitted when eligible damage or a hard interrupt source interrupts the active cast.
/// </summary>
public sealed record CastInterruptedEvent(
    string CastId,
    string SpellId,
    string CasterCombatActorId,
    string? TargetCombatActorId,
    CombatTick Tick,
    string InterruptSource,
    double? InterruptChance,
    double? InterruptRoll) : ICombatCastLifecycleEvent;

/// <summary>
/// Emitted when the player cancels the active cast before completion.
/// </summary>
public sealed record CastCancelledEvent(
    string CastId,
    string SpellId,
    string CasterCombatActorId,
    string? TargetCombatActorId,
    CombatTick Tick,
    string CancelSource) : ICombatCastLifecycleEvent;

/// <summary>
/// Emitted when Combat Core starts post-cast, post-cancel, or post-interrupt recovery.
/// </summary>
public sealed record CastRecoveryStartedEvent(
    string CastId,
    string SpellId,
    string CasterCombatActorId,
    string? TargetCombatActorId,
    CombatTick Tick,
    double RecoverySeconds) : ICombatCastLifecycleEvent;

/// <summary>
/// Emitted when Combat-owned recovery ends and casting becomes available again.
/// </summary>
public sealed record CastRecoveryEndedEvent(
    string CastId,
    string SpellId,
    string CasterCombatActorId,
    string? TargetCombatActorId,
    CombatTick Tick) : ICombatCastLifecycleEvent;
