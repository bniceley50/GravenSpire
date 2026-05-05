#nullable enable

using System;

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Save-safe read projection for Combat-owned player state.
/// </summary>
public sealed class CombatPersistenceProjection
{
    private CombatPersistenceProjection(
        int currentHealth,
        int currentMana,
        CombatActorLifeState combatLifeState,
        PlayerDeathEvent? pendingDeathHandoffPayload)
    {
        current_health = currentHealth;
        current_mana = currentMana;
        combat_life_state = combatLifeState;
        pending_death_handoff_payload = pendingDeathHandoffPayload;
    }

    public int current_health { get; }

    public int current_mana { get; }

    public CombatActorLifeState combat_life_state { get; }

    public PlayerDeathEvent? pending_death_handoff_payload { get; }

    public static CombatPersistenceProjection FromPlayer(
        CombatActorState player,
        PlayerDeathEvent? pendingDeathHandoffPayload = null)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (player.ActorKind != CombatActorKind.Player)
        {
            throw new InvalidOperationException("Combat persistence projection requires a player actor.");
        }

        return new CombatPersistenceProjection(
            player.CurrentHealth,
            player.CurrentMana,
            player.LifeState,
            pendingDeathHandoffPayload);
    }
}
