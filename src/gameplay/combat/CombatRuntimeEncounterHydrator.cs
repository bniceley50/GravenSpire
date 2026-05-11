#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravenspire.Gameplay.Combat.Fixtures;

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Hydrates a runtime encounter from approved Combat Core fixture data without owning combat formulas.
/// </summary>
public sealed class CombatRuntimeEncounterHydrator
{
    /// <summary>
    /// Loads fixture data from disk and hydrates the requested encounter actors.
    /// </summary>
    public CombatRuntimeEncounterHydrationResult HydrateFromFile(
        string fixtureFilePath,
        CombatRuntimeEncounterHydrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(fixtureFilePath))
        {
            return CombatRuntimeEncounterHydrationResult.Failure(new[] { "fixture_file_path is required." });
        }

        try
        {
            var package = new CombatFixtureLoader().LoadFromFile(fixtureFilePath);
            return Hydrate(package, request, Path.GetFullPath(fixtureFilePath));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            return CombatRuntimeEncounterHydrationResult.Failure(new[] { ex.Message });
        }
    }

    /// <summary>
    /// Hydrates the requested encounter actors from an already-loaded fixture package.
    /// </summary>
    public CombatRuntimeEncounterHydrationResult Hydrate(
        CombatFixturePackage? package,
        CombatRuntimeEncounterHydrationRequest? request,
        string fixtureFilePath = "")
    {
        var errors = new List<string>();

        if (request is null)
        {
            errors.Add("runtime encounter hydration request is required.");
        }
        else
        {
            errors.AddRange(request.Validate());
        }

        var packageValidation = new CombatFixtureValidator().Validate(package);
        if (!packageValidation.IsValid)
        {
            errors.AddRange(packageValidation.Errors);
        }

        if (errors.Count > 0 || package is null || request is null)
        {
            return CombatRuntimeEncounterHydrationResult.Failure(errors);
        }

        var encounter = package.EncounterFixtures.SingleOrDefault(
            candidate => string.Equals(candidate.Id, request.EncounterFixtureId, StringComparison.Ordinal));
        if (encounter is null)
        {
            return CombatRuntimeEncounterHydrationResult.Failure(new[]
            {
                $"encounter fixture '{request.EncounterFixtureId}' was not found."
            });
        }

        var actorFixtures = ResolveActorFixtures(package, encounter, errors);
        var playerFixture = actorFixtures.SingleOrDefault(fixture => fixture.ActorKind == CombatActorKind.Player);
        var hostileFixtures = actorFixtures
            .Where(fixture => fixture.ActorKind == CombatActorKind.NPC)
            .ToArray();

        if (playerFixture is null)
        {
            errors.Add($"{encounter.Id}: one player actor fixture is required.");
        }

        if (hostileFixtures.Length == 0)
        {
            errors.Add($"{encounter.Id}: at least one hostile actor fixture is required.");
        }

        if (errors.Count > 0 || playerFixture is null)
        {
            return CombatRuntimeEncounterHydrationResult.Failure(errors);
        }

        var playerHydration = HydratePlayerActor(playerFixture, request);
        if (!playerHydration.Succeeded || playerHydration.Actor is null)
        {
            return CombatRuntimeEncounterHydrationResult.Failure(playerHydration.Errors);
        }

        var hostileActors = HydrateHostileActors(hostileFixtures, encounter, request, errors);
        if (errors.Count > 0)
        {
            return CombatRuntimeEncounterHydrationResult.Failure(errors);
        }

        return new CombatRuntimeEncounterHydrationResult(
            true,
            request.ActiveZoneId,
            fixtureFilePath,
            package.FixtureSetVersion,
            new[] { encounter.Id },
            encounter.ActorFixtureIds.ToArray(),
            playerHydration.Actor,
            hostileActors,
            Array.Empty<string>());
    }

    private static IReadOnlyList<CombatActorFixture> ResolveActorFixtures(
        CombatFixturePackage package,
        CombatEncounterFixture encounter,
        ICollection<string> errors)
    {
        var actorFixtures = new List<CombatActorFixture>();
        foreach (var actorFixtureId in encounter.ActorFixtureIds)
        {
            var fixture = package.ActorFixtures.SingleOrDefault(
                candidate => string.Equals(candidate.Id, actorFixtureId, StringComparison.Ordinal));
            if (fixture is null)
            {
                errors.Add($"{encounter.Id}: actor fixture '{actorFixtureId}' was not found.");
                continue;
            }

            actorFixtures.Add(fixture);
        }

        return actorFixtures;
    }

    private static CombatActorHydrationResult HydratePlayerActor(
        CombatActorFixture playerFixture,
        CombatRuntimeEncounterHydrationRequest request)
    {
        var snapshot = new CombatProgressionBaselineSnapshot(
            request.PlayerLocalCharacterId,
            playerFixture.ClassId ?? string.Empty,
            playerFixture.Level,
            playerFixture.MaxHealth,
            playerFixture.MaxMana,
            1,
            0,
            CombatProgressionBaselineProducedFor.DebugValidation);
        var input = new CombatActorHydrationInput(
            request.PlayerCombatActorId,
            request.ActiveZoneId,
            $"player-{request.PlayerLocalCharacterId}",
            null,
            playerFixture.MaxEndurance);

        return new CombatActorHydrator().HydratePlayerActor(snapshot, playerFixture, input);
    }

    private static IReadOnlyList<CombatActorState> HydrateHostileActors(
        IReadOnlyList<CombatActorFixture> hostileFixtures,
        CombatEncounterFixture encounter,
        CombatRuntimeEncounterHydrationRequest request,
        ICollection<string> errors)
    {
        var hostileActors = new List<CombatActorState>();
        for (var index = 0; index < hostileFixtures.Count; index++)
        {
            var fixture = hostileFixtures[index];
            var stableSourceRef = BuildHostileStableSourceRef(fixture, encounter, errors);
            if (stableSourceRef is null)
            {
                continue;
            }

            var actor = new CombatActorState(
                $"{request.HostileCombatActorIdPrefix}-{index + 1}",
                CombatActorKind.NPC,
                stableSourceRef,
                fixture.FactionId,
                request.ActiveZoneId,
                fixture.Level,
                fixture.MaxHealth,
                fixture.MaxHealth,
                fixture.MaxMana,
                fixture.MaxMana,
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
                $"hostile-{fixture.Id}-{index + 1}",
                maxEndurance: fixture.MaxEndurance,
                currentEndurance: fixture.MaxEndurance);

            var validation = actor.Validate();
            if (!validation.IsValid)
            {
                foreach (var error in validation.Errors)
                {
                    errors.Add($"{fixture.Id}: {error}");
                }

                continue;
            }

            hostileActors.Add(actor);
        }

        return hostileActors;
    }

    private static CombatStableSourceRef? BuildHostileStableSourceRef(
        CombatActorFixture fixture,
        CombatEncounterFixture encounter,
        ICollection<string> errors)
    {
        var npcAlias = FirstAlias("source_npc_id:", fixture.StableSourceAliases, encounter.SourceRefAliases);
        if (npcAlias is not null)
        {
            return CombatStableSourceRef.ForPersistentNpc(npcAlias);
        }

        var spawnAlias = FirstAlias("source_spawn_ref:", fixture.StableSourceAliases, encounter.SourceRefAliases);
        if (spawnAlias is not null)
        {
            return CombatStableSourceRef.ForSpawn(new CombatSpawnSourceRef(encounter.Id, spawnAlias, fixture.Id));
        }

        errors.Add($"{fixture.Id}: NPC fixtures require source_npc_id or source_spawn_ref alias for runtime hydration.");
        return null;
    }

    private static string? FirstAlias(
        string prefix,
        IEnumerable<string> primaryAliases,
        IEnumerable<string> fallbackAliases)
    {
        foreach (var alias in primaryAliases.Concat(fallbackAliases))
        {
            if (!alias.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            var value = alias[prefix.Length..];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}

/// <summary>
/// Request values needed to hydrate a story-scoped runtime encounter.
/// </summary>
public sealed record CombatRuntimeEncounterHydrationRequest
{
    public string EncounterFixtureId { get; init; } = string.Empty;

    public string ActiveZoneId { get; init; } = string.Empty;

    public string PlayerCombatActorId { get; init; } = string.Empty;

    public string PlayerLocalCharacterId { get; init; } = string.Empty;

    public string HostileCombatActorIdPrefix { get; init; } = string.Empty;

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        RequireText(EncounterFixtureId, "encounter_fixture_id", errors);
        RequireText(ActiveZoneId, "active_zone_id", errors);
        RequireText(PlayerCombatActorId, "player_combat_actor_id", errors);
        RequireText(PlayerLocalCharacterId, "player_local_character_id", errors);
        RequireText(HostileCombatActorIdPrefix, "hostile_combat_actor_id_prefix", errors);
        return errors;
    }

    private static void RequireText(string value, string fieldName, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{fieldName} is required.");
        }
    }
}

/// <summary>
/// Result of hydrating a runtime encounter from Combat Core fixture data.
/// </summary>
public sealed record CombatRuntimeEncounterHydrationResult(
    bool Succeeded,
    string ActiveZoneId,
    string FixtureFilePath,
    string FixtureSetVersion,
    IReadOnlyList<string> EncounterFixtureIds,
    IReadOnlyList<string> ActorFixtureIds,
    CombatActorState? PlayerActor,
    IReadOnlyList<CombatActorState> HostileActors,
    IReadOnlyList<string> Errors)
{
    public static CombatRuntimeEncounterHydrationResult Failure(IEnumerable<string> errors)
    {
        return new CombatRuntimeEncounterHydrationResult(
            false,
            string.Empty,
            string.Empty,
            string.Empty,
            Array.Empty<string>(),
            Array.Empty<string>(),
            null,
            Array.Empty<CombatActorState>(),
            errors.ToArray());
    }
}
