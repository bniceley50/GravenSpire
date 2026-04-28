#nullable enable

using System;
using System.Collections.Generic;

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Reason Character Progression produced a combat-facing baseline snapshot.
/// </summary>
public enum CombatProgressionBaselineProducedFor
{
    /// <summary>
    /// Snapshot was produced during normal load hydration.
    /// </summary>
    InitialHydration,

    /// <summary>
    /// Snapshot was produced for a newly materialized T1 profile.
    /// </summary>
    NewProfileMaterialization,

    /// <summary>
    /// Snapshot was produced after a progression level change.
    /// </summary>
    LevelChanged,

    /// <summary>
    /// Snapshot was produced for a controlled debug or validation harness.
    /// </summary>
    DebugValidation
}

/// <summary>
/// Combat-scoped Character Progression baseline. It is the only T1 progression baseline Combat may consume.
/// </summary>
public sealed record CombatProgressionBaselineSnapshot(
    string LocalCharacterId,
    string ClassId,
    int CombatActorLevel,
    int PermanentMaxHealth,
    int PermanentMaxMana,
    int ProgressionSchemaVersion,
    long ProgressionStateRevision,
    CombatProgressionBaselineProducedFor ProducedFor)
{
    /// <summary>
    /// Validates the snapshot for the T1 Cleric combat hydration path.
    /// </summary>
    public IReadOnlyList<string> ValidateForT1CombatHydration()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(LocalCharacterId))
        {
            errors.Add("local_character_id is required.");
        }

        if (!string.Equals(ClassId, "Cleric", StringComparison.Ordinal))
        {
            errors.Add("class_id must be Cleric for T1 combat hydration.");
        }

        if (CombatActorLevel is < 1 or > 10)
        {
            errors.Add("combat_actor_level must be inside the T1 level band.");
        }

        if (PermanentMaxHealth <= 0)
        {
            errors.Add("permanent_max_health must be positive.");
        }

        if (PermanentMaxMana <= 0)
        {
            errors.Add("permanent_max_mana must be positive.");
        }

        if (ProgressionSchemaVersion <= 0)
        {
            errors.Add("progression_schema_version must be positive.");
        }

        if (ProgressionStateRevision < 0)
        {
            errors.Add("progression_state_revision must not be negative.");
        }

        return errors;
    }
}
