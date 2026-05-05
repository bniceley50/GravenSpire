#nullable enable

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Runtime NPC death signal for immediate Combat Core subscribers.
/// </summary>
public sealed record CombatActorDeathEvent(
    string combat_actor_id,
    CombatStableSourceRef defeated_source_ref,
    string zoneId);

/// <summary>
/// Approved narrow player kill-credit signal emitted from Combat Core.
/// </summary>
public sealed record PlayerKillCreditEvent(
    CombatStableSourceRef defeated_source_ref,
    string zoneId,
    string? faction_id,
    double kill_weight_seed);

/// <summary>
/// Narrow local-player death signal for a future downstream handoff.
/// </summary>
public sealed record PlayerDeathEvent(
    string death_context_id,
    string local_character_id,
    string zoneId,
    CombatPoint3 death_position,
    CombatStableSourceRef killer_source_ref,
    string death_cause_type);
