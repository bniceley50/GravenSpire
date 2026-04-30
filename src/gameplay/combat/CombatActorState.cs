#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Broad runtime category of a combat participant.
/// </summary>
public enum CombatActorKind
{
    /// <summary>
    /// The active local player combat actor.
    /// </summary>
    Player,

    /// <summary>
    /// A non-player combat actor claimed from NPC or spawn authoring.
    /// </summary>
    NPC,

    /// <summary>
    /// A non-character environmental damage source.
    /// </summary>
    EnvironmentalCombatSource
}

/// <summary>
/// Runtime combat state owned by Combat Core.
/// </summary>
public enum CombatState
{
    /// <summary>
    /// Actor is not currently in combat.
    /// </summary>
    OutOfCombat,

    /// <summary>
    /// Actor has entered pull initialization before full combat state.
    /// </summary>
    Pulling,

    /// <summary>
    /// Actor is actively participating in combat.
    /// </summary>
    InCombat,

    /// <summary>
    /// Actor is performing a slow cast.
    /// </summary>
    Casting,

    /// <summary>
    /// Actor is returning to an anchor or leaving combat through leash rules.
    /// </summary>
    Leashing,

    /// <summary>
    /// Actor is dead and cannot perform combat actions.
    /// </summary>
    Dead,

    /// <summary>
    /// Actor has had a cast interrupted and is waiting for recovery routing.
    /// </summary>
    Interrupted,

    /// <summary>
    /// Actor is in post-cast or post-cancel recovery.
    /// </summary>
    Recovery
}

/// <summary>
/// Combat-owned cast runtime substate carried on the actor without owning spellbook slots.
/// </summary>
public enum CombatCastRuntimeState
{
    /// <summary>
    /// Actor has no active Combat-owned cast runtime.
    /// </summary>
    None,

    /// <summary>
    /// Actor is channeling a slow cast.
    /// </summary>
    Casting,

    /// <summary>
    /// Actor has had the current cast interrupted.
    /// </summary>
    Interrupted,

    /// <summary>
    /// Actor is blocked by post-cast, post-cancel, or post-interrupt recovery.
    /// </summary>
    Recovery
}

/// <summary>
/// Life-state boundary used by Combat and later Save/Load handoff rules.
/// </summary>
public enum CombatActorLifeState
{
    /// <summary>
    /// Actor can participate in combat.
    /// </summary>
    Alive,

    /// <summary>
    /// Player death occurred and awaits a future corpse-run handoff.
    /// </summary>
    DeadPendingCorpseRunHandoff,

    /// <summary>
    /// Actor death is fully resolved for Combat-owned runtime purposes.
    /// </summary>
    Dead
}

/// <summary>
/// Stable authored spawn identity for non-persistent hostile fixtures.
/// </summary>
public sealed record CombatSpawnSourceRef(
    string SpawnTableId,
    string SpawnAnchorId,
    string NpcArchetypeId);

/// <summary>
/// Stable source reference used for handoffs outside the transient combat session.
/// </summary>
public sealed record CombatStableSourceRef
{
    private CombatStableSourceRef(
        string? localCharacterId,
        string? sourceNpcId,
        CombatSpawnSourceRef? sourceSpawnRef,
        string? sourceHazardId)
    {
        LocalCharacterId = localCharacterId;
        SourceNpcId = sourceNpcId;
        SourceSpawnRef = sourceSpawnRef;
        SourceHazardId = sourceHazardId;
    }

    /// <summary>
    /// Player-local identity supplied by Character Creation and Save/Load context.
    /// </summary>
    public string? LocalCharacterId { get; }

    /// <summary>
    /// Persistent NPC identity supplied by NPC System.
    /// </summary>
    public string? SourceNpcId { get; }

    /// <summary>
    /// Authored spawn identity for non-persistent hostile actors.
    /// </summary>
    public CombatSpawnSourceRef? SourceSpawnRef { get; }

    /// <summary>
    /// Authored environmental combat source identity.
    /// </summary>
    public string? SourceHazardId { get; }

    /// <summary>
    /// Creates a stable player source reference.
    /// </summary>
    public static CombatStableSourceRef ForPlayer(string localCharacterId)
    {
        return new CombatStableSourceRef(RequireText(localCharacterId, nameof(localCharacterId)), null, null, null);
    }

    /// <summary>
    /// Creates a stable persistent NPC source reference.
    /// </summary>
    public static CombatStableSourceRef ForPersistentNpc(string sourceNpcId)
    {
        return new CombatStableSourceRef(null, RequireText(sourceNpcId, nameof(sourceNpcId)), null, null);
    }

    /// <summary>
    /// Creates a stable non-persistent spawn source reference.
    /// </summary>
    public static CombatStableSourceRef ForSpawn(CombatSpawnSourceRef sourceSpawnRef)
    {
        ArgumentNullException.ThrowIfNull(sourceSpawnRef);
        return new CombatStableSourceRef(null, null, sourceSpawnRef, null);
    }

    /// <summary>
    /// Creates a stable environmental source reference.
    /// </summary>
    public static CombatStableSourceRef ForHazard(string sourceHazardId)
    {
        return new CombatStableSourceRef(null, null, null, RequireText(sourceHazardId, nameof(sourceHazardId)));
    }

    /// <summary>
    /// Validates that this stable source matches the actor kind.
    /// </summary>
    public IReadOnlyList<string> ValidateFor(CombatActorKind actorKind)
    {
        var errors = new List<string>();
        var populated = CountPopulated();

        if (populated != 1)
        {
            errors.Add("stable_source_ref must contain exactly one stable identity.");
        }

        switch (actorKind)
        {
            case CombatActorKind.Player when string.IsNullOrWhiteSpace(LocalCharacterId):
                errors.Add("Player actors require stable_source_ref.local_character_id.");
                break;
            case CombatActorKind.NPC when string.IsNullOrWhiteSpace(SourceNpcId) && SourceSpawnRef is null:
                errors.Add("NPC actors require stable_source_ref.source_npc_id or source_spawn_ref.");
                break;
            case CombatActorKind.EnvironmentalCombatSource when string.IsNullOrWhiteSpace(SourceHazardId):
                errors.Add("Environmental combat sources require stable_source_ref.source_hazard_id.");
                break;
        }

        if (SourceSpawnRef is not null)
        {
            if (string.IsNullOrWhiteSpace(SourceSpawnRef.SpawnTableId))
            {
                errors.Add("source_spawn_ref.spawn_table_id is required.");
            }

            if (string.IsNullOrWhiteSpace(SourceSpawnRef.SpawnAnchorId))
            {
                errors.Add("source_spawn_ref.spawn_anchor_id is required.");
            }

            if (string.IsNullOrWhiteSpace(SourceSpawnRef.NpcArchetypeId))
            {
                errors.Add("source_spawn_ref.npc_archetype_id is required.");
            }
        }

        return errors;
    }

    private int CountPopulated()
    {
        var count = 0;
        if (!string.IsNullOrWhiteSpace(LocalCharacterId))
        {
            count++;
        }

        if (!string.IsNullOrWhiteSpace(SourceNpcId))
        {
            count++;
        }

        if (SourceSpawnRef is not null)
        {
            count++;
        }

        if (!string.IsNullOrWhiteSpace(SourceHazardId))
        {
            count++;
        }

        return count;
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Stable source identifiers cannot be empty.", parameterName);
        }

        return value;
    }
}

/// <summary>
/// Result of validating a combat actor record.
/// </summary>
public sealed record CombatActorValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    /// <summary>
    /// Creates a passing validation result.
    /// </summary>
    public static CombatActorValidationResult Valid { get; } = new(true, Array.Empty<string>());

    /// <summary>
    /// Creates a failing validation result.
    /// </summary>
    public static CombatActorValidationResult Invalid(IEnumerable<string> errors)
    {
        return new CombatActorValidationResult(false, errors.ToArray());
    }
}

/// <summary>
/// Data-only runtime combat actor state. Runtime ids and threat tables are transient.
/// </summary>
public sealed record CombatActorState
{
    /// <summary>
    /// Creates a combat actor state record.
    /// </summary>
    public CombatActorState(
        string combatActorId,
        CombatActorKind actorKind,
        CombatStableSourceRef stableSourceRef,
        string? factionId,
        string zoneId,
        int level,
        int maxHealth,
        int currentHealth,
        int maxMana,
        int currentMana,
        int armorClass,
        int attackPower,
        int weaponBaseDamage,
        int attackSkill,
        int defenseSkill,
        double weaponDelaySeconds,
        double meleeRangeMeters,
        double spellRangeMeters,
        CombatState combatState,
        CombatActorLifeState lifeState,
        string? targetCombatActorId,
        string combatSortKey,
        IReadOnlyDictionary<string, int>? threatTable = null)
    {
        CombatActorId = combatActorId;
        ActorKind = actorKind;
        StableSourceRef = stableSourceRef;
        FactionId = factionId;
        ZoneId = zoneId;
        Level = level;
        MaxHealth = maxHealth;
        CurrentHealth = currentHealth;
        MaxMana = maxMana;
        CurrentMana = currentMana;
        ArmorClass = armorClass;
        AttackPower = attackPower;
        WeaponBaseDamage = weaponBaseDamage;
        AttackSkill = attackSkill;
        DefenseSkill = defenseSkill;
        WeaponDelaySeconds = weaponDelaySeconds;
        MeleeRangeMeters = meleeRangeMeters;
        SpellRangeMeters = spellRangeMeters;
        CombatState = combatState;
        LifeState = lifeState;
        TargetCombatActorId = targetCombatActorId;
        CombatSortKey = combatSortKey;
        ThreatTable = new ReadOnlyDictionary<string, int>(
            new Dictionary<string, int>(threatTable ?? new Dictionary<string, int>(), StringComparer.Ordinal));
    }

    /// <summary>
    /// Transient runtime combat actor id. Never use as save or XP identity.
    /// </summary>
    public string CombatActorId { get; }

    /// <summary>
    /// Runtime actor category.
    /// </summary>
    public CombatActorKind ActorKind { get; }

    /// <summary>
    /// Stable handoff identity for persistence and downstream contracts.
    /// </summary>
    public CombatStableSourceRef StableSourceRef { get; }

    /// <summary>
    /// Optional faction identity for downstream systems.
    /// </summary>
    public string? FactionId { get; }

    /// <summary>
    /// Active zone id for the actor.
    /// </summary>
    public string ZoneId { get; }

    /// <summary>
    /// Combat-facing level used by Combat Core formulas.
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// Combat-owned maximum health for the current actor state.
    /// </summary>
    public int MaxHealth { get; }

    /// <summary>
    /// Combat-owned current health for the current actor state.
    /// </summary>
    public int CurrentHealth { get; }

    /// <summary>
    /// Combat-owned maximum mana for the current actor state.
    /// </summary>
    public int MaxMana { get; }

    /// <summary>
    /// Combat-owned current mana for the current actor state.
    /// </summary>
    public int CurrentMana { get; }

    /// <summary>
    /// Actor armor class fixture value.
    /// </summary>
    public int ArmorClass { get; }

    /// <summary>
    /// Actor attack power fixture value.
    /// </summary>
    public int AttackPower { get; }

    /// <summary>
    /// Actor weapon or natural attack base damage fixture value.
    /// </summary>
    public int WeaponBaseDamage { get; }

    /// <summary>
    /// Actor attack skill fixture value.
    /// </summary>
    public int AttackSkill { get; }

    /// <summary>
    /// Actor defense skill fixture value.
    /// </summary>
    public int DefenseSkill { get; }

    /// <summary>
    /// Actor weapon delay in seconds, loaded from fixture data.
    /// </summary>
    public double WeaponDelaySeconds { get; }

    /// <summary>
    /// Actor melee range in meters, loaded from fixture data.
    /// </summary>
    public double MeleeRangeMeters { get; }

    /// <summary>
    /// Actor spell range in meters, loaded from fixture data.
    /// </summary>
    public double SpellRangeMeters { get; }

    /// <summary>
    /// Combat-owned runtime state.
    /// </summary>
    public CombatState CombatState { get; }

    /// <summary>
    /// Combat-owned life state.
    /// </summary>
    public CombatActorLifeState LifeState { get; }

    /// <summary>
    /// Optional transient target combat actor id.
    /// </summary>
    public string? TargetCombatActorId { get; }

    /// <summary>
    /// Stable deterministic sort key derived from stable source and authored spawn context.
    /// </summary>
    public string CombatSortKey { get; }

    /// <summary>
    /// Transient threat table keyed by runtime combat actor id.
    /// </summary>
    public IReadOnlyDictionary<string, int> ThreatTable { get; }

    /// <summary>
    /// Combat-owned cast runtime substate. Spellbook and memorization ownership remain downstream.
    /// </summary>
    public CombatCastRuntimeState CastRuntimeState { get; init; } = CombatCastRuntimeState.None;

    /// <summary>
    /// Transient active cast id supplied by the caller for deterministic lifecycle correlation.
    /// </summary>
    public string? ActiveCastId { get; init; }

    /// <summary>
    /// Active spell profile id being executed by Combat Core, when any.
    /// </summary>
    public string? ActiveCastSpellId { get; init; }

    /// <summary>
    /// Optional transient target combat actor id for the active cast.
    /// </summary>
    public string? ActiveCastTargetCombatActorId { get; init; }

    /// <summary>
    /// Cast progress in combat-clock seconds for HUD and lifecycle consumers.
    /// </summary>
    public double CastProgressSeconds { get; init; }

    /// <summary>
    /// Remaining recovery time in combat-clock seconds after cast completion, cancellation, or interruption.
    /// </summary>
    public double CastRecoveryRemainingSeconds { get; init; }

    /// <summary>
    /// Combat-owned player posture for med-break and unsafe-sit transitions.
    /// </summary>
    public CombatPostureState PostureState { get; init; } = CombatPostureState.Standing;

    /// <summary>
    /// Optional next Combat Simulation Tick index at which regen may resolve.
    /// </summary>
    public long? NextRegenTickIndex { get; init; }

    /// <summary>
    /// Optional Combat Simulation Tick index of the actor's last hostile action.
    /// </summary>
    public long? LastHostileActionTickIndex { get; init; }

    /// <summary>
    /// Remaining combat-exit time in Combat-owned simulation seconds for HUD/debug consumers.
    /// </summary>
    public double CombatExitRemainingSeconds { get; init; }

    /// <summary>
    /// True when Combat may treat the actor as alive for runtime transitions.
    /// </summary>
    public bool IsAlive => LifeState == CombatActorLifeState.Alive && CurrentHealth > 0;

    /// <summary>
    /// Validates the actor state shape without invoking Unity scene objects.
    /// </summary>
    public CombatActorValidationResult Validate()
    {
        var errors = new List<string>();

        RequireNonEmpty(CombatActorId, "combat_actor_id", errors);
        RequireNonEmpty(ZoneId, "zone_id", errors);
        RequireNonEmpty(CombatSortKey, "combat_sort_key", errors);
        errors.AddRange(StableSourceRef.ValidateFor(ActorKind));

        if (Level <= 0)
        {
            errors.Add("level must be positive.");
        }

        if (MaxHealth <= 0)
        {
            errors.Add("max_health must be positive.");
        }

        if (CurrentHealth <= 0 && LifeState == CombatActorLifeState.Alive)
        {
            errors.Add("current_health must be positive for a live actor.");
        }

        if (CurrentHealth > MaxHealth)
        {
            errors.Add("current_health must not exceed max_health.");
        }

        if (MaxMana < 0)
        {
            errors.Add("max_mana must not be negative.");
        }

        if (CurrentMana < 0)
        {
            errors.Add("current_mana must not be negative.");
        }

        if (CurrentMana > MaxMana)
        {
            errors.Add("current_mana must not exceed max_mana.");
        }

        if (ArmorClass < 0 || AttackPower < 0 || WeaponBaseDamage < 0 || AttackSkill < 0 || DefenseSkill < 0)
        {
            errors.Add("combat stats must not be negative.");
        }

        if (WeaponDelaySeconds <= 0)
        {
            errors.Add("weapon_delay_seconds must be positive.");
        }

        if (MeleeRangeMeters <= 0 || SpellRangeMeters < 0)
        {
            errors.Add("combat ranges must be non-negative and melee range must be positive.");
        }

        foreach (var threatEntry in ThreatTable)
        {
            if (string.IsNullOrWhiteSpace(threatEntry.Key))
            {
                errors.Add("threat_table keys must be non-empty runtime combat actor ids.");
            }

            if (threatEntry.Value < 0)
            {
                errors.Add("threat_table values must not be negative.");
            }
        }

        if (CastProgressSeconds < 0 || CastRecoveryRemainingSeconds < 0)
        {
            errors.Add("cast progress and recovery remaining seconds must not be negative.");
        }

        if (NextRegenTickIndex is < 0 || LastHostileActionTickIndex is < 0 || CombatExitRemainingSeconds < 0)
        {
            errors.Add("regen and combat-exit tick state must not be negative.");
        }

        if (CastRuntimeState != CombatCastRuntimeState.None)
        {
            RequireNonEmpty(ActiveCastId ?? string.Empty, "active_cast_id", errors);
            RequireNonEmpty(ActiveCastSpellId ?? string.Empty, "active_cast_spell_id", errors);
        }

        return errors.Count == 0 ? CombatActorValidationResult.Valid : CombatActorValidationResult.Invalid(errors);
    }

    private static void RequireNonEmpty(string value, string fieldName, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{fieldName} is required.");
        }
    }
}
