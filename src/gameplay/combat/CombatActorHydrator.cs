#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Gravenspire.Gameplay.Combat.Fixtures;

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Combat-owned current resource state supplied during actor hydration.
/// </summary>
public sealed record CombatResourceHydrationState(
    int CurrentHealth,
    int CurrentMana,
    int CurrentEndurance = 0);

/// <summary>
/// Runtime context required to build a player combat actor from fixtures and progression baseline data.
/// </summary>
public sealed record CombatActorHydrationInput(
    string CombatActorId,
    string ZoneId,
    string CombatSortKey,
    CombatResourceHydrationState? CurrentResources = null,
    int MaxEndurance = 0);

/// <summary>
/// Result of a combat actor hydration attempt.
/// </summary>
public sealed record CombatActorHydrationResult(bool Succeeded, CombatActorState? Actor, IReadOnlyList<string> Errors)
{
    /// <summary>
    /// Creates a successful hydration result.
    /// </summary>
    public static CombatActorHydrationResult Success(CombatActorState actor)
    {
        return new CombatActorHydrationResult(true, actor, Array.Empty<string>());
    }

    /// <summary>
    /// Creates a failed hydration result.
    /// </summary>
    public static CombatActorHydrationResult Failure(IEnumerable<string> errors)
    {
        return new CombatActorHydrationResult(false, null, errors.ToArray());
    }
}

/// <summary>
/// Builds player combat actor state from the ADR-0003 combat baseline and fixture-owned combat tunables.
/// </summary>
public sealed class CombatActorHydrator
{
    /// <summary>
    /// Hydrates the T1 player combat actor from a CombatProgressionBaselineSnapshot and a Cleric actor fixture.
    /// </summary>
    public CombatActorHydrationResult HydratePlayerActor(
        CombatProgressionBaselineSnapshot? snapshot,
        CombatActorFixture? fixture,
        CombatActorHydrationInput? input)
    {
        var errors = new List<string>();

        if (snapshot is null)
        {
            errors.Add("CombatProgressionBaselineSnapshot is required before Combat actor hydration.");
        }
        else
        {
            errors.AddRange(snapshot.ValidateForT1CombatHydration());
        }

        if (fixture is null)
        {
            errors.Add("Combat actor fixture is required before Combat actor hydration.");
        }
        else
        {
            ValidatePlayerFixture(snapshot, fixture, errors);
        }

        if (input is null)
        {
            errors.Add("Combat actor hydration input is required.");
        }
        else
        {
            ValidateHydrationInput(input, errors);
        }

        if (errors.Count > 0 || snapshot is null || fixture is null || input is null)
        {
            return CombatActorHydrationResult.Failure(errors);
        }

        var resources = input.CurrentResources ?? new CombatResourceHydrationState(
            snapshot.PermanentMaxHealth,
            snapshot.PermanentMaxMana,
            input.MaxEndurance);

        ValidateCurrentResources(resources, snapshot, input.MaxEndurance, errors);

        if (errors.Count > 0)
        {
            return CombatActorHydrationResult.Failure(errors);
        }

        var actor = new CombatActorState(
            input.CombatActorId,
            CombatActorKind.Player,
            CombatStableSourceRef.ForPlayer(snapshot.LocalCharacterId),
            fixture.FactionId,
            input.ZoneId,
            snapshot.CombatActorLevel,
            snapshot.PermanentMaxHealth,
            resources.CurrentHealth,
            snapshot.PermanentMaxMana,
            resources.CurrentMana,
            fixture.ArmorClass,
            fixture.AttackPower,
            fixture.WeaponBaseDamage,
            fixture.AttackSkill,
            fixture.DefenseSkill,
            fixture.WeaponDelaySeconds,
            fixture.MeleeRangeMeters,
            fixture.SpellRangeMeters,
            CombatState.OutOfCombat,
            CombatActorLifeState.Alive,
            null,
            input.CombatSortKey,
            maxEndurance: input.MaxEndurance,
            currentEndurance: resources.CurrentEndurance);

        var actorValidation = actor.Validate();
        if (!actorValidation.IsValid)
        {
            return CombatActorHydrationResult.Failure(actorValidation.Errors);
        }

        return CombatActorHydrationResult.Success(actor);
    }

    private static void ValidatePlayerFixture(
        CombatProgressionBaselineSnapshot? snapshot,
        CombatActorFixture fixture,
        ICollection<string> errors)
    {
        if (fixture.ActorKind != CombatActorKind.Player)
        {
            errors.Add("T1 player hydration requires a Player actor fixture.");
        }

        if (!string.Equals(fixture.ClassId, "Cleric", StringComparison.Ordinal))
        {
            errors.Add("T1 player hydration requires a Cleric actor fixture.");
        }

        if (snapshot is null)
        {
            return;
        }

        if (fixture.Level != snapshot.CombatActorLevel)
        {
            errors.Add("Player actor fixture level must match CombatProgressionBaselineSnapshot.combat_actor_level.");
        }

        if (fixture.MaxHealth != snapshot.PermanentMaxHealth)
        {
            errors.Add("Player actor fixture max health must match CombatProgressionBaselineSnapshot.permanent_max_health.");
        }

        if (fixture.MaxMana != snapshot.PermanentMaxMana)
        {
            errors.Add("Player actor fixture max mana must match CombatProgressionBaselineSnapshot.permanent_max_mana.");
        }
    }

    private static void ValidateHydrationInput(CombatActorHydrationInput input, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(input.CombatActorId))
        {
            errors.Add("combat_actor_id is required.");
        }

        if (string.IsNullOrWhiteSpace(input.ZoneId))
        {
            errors.Add("zone_id is required.");
        }

        if (string.IsNullOrWhiteSpace(input.CombatSortKey))
        {
            errors.Add("combat_sort_key is required.");
        }

        if (input.MaxEndurance < 0)
        {
            errors.Add("max_endurance must not be negative.");
        }
    }

    private static void ValidateCurrentResources(
        CombatResourceHydrationState resources,
        CombatProgressionBaselineSnapshot snapshot,
        int maxEndurance,
        ICollection<string> errors)
    {
        if (resources.CurrentHealth <= 0)
        {
            errors.Add("current_health <= 0 without death handoff is invalid combat hydration.");
        }

        if (resources.CurrentHealth > snapshot.PermanentMaxHealth)
        {
            errors.Add("current_health must not exceed CombatProgressionBaselineSnapshot.permanent_max_health.");
        }

        if (resources.CurrentMana < 0)
        {
            errors.Add("current_mana must not be negative.");
        }

        if (resources.CurrentMana > snapshot.PermanentMaxMana)
        {
            errors.Add("current_mana must not exceed CombatProgressionBaselineSnapshot.permanent_max_mana.");
        }

        if (resources.CurrentEndurance < 0)
        {
            errors.Add("current_endurance must not be negative.");
        }

        if (resources.CurrentEndurance > maxEndurance)
        {
            errors.Add("current_endurance must not exceed max_endurance.");
        }
    }
}
