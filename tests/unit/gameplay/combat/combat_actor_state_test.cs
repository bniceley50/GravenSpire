#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using NUnit.Framework;

namespace Gravenspire.Tests.Unit.Gameplay.Combat;

public sealed class CombatActorStateTest
{
    [Test]
    public void test_combat_actor_state_contains_required_runtime_and_stable_fields()
    {
        var actor = CreateValidPlayerActor();

        var validation = actor.Validate();

        Assert.That(validation.IsValid, Is.True, string.Join(Environment.NewLine, validation.Errors));
        Assert.That(actor.CombatActorId, Is.EqualTo("combat-player-1"));
        Assert.That(actor.StableSourceRef.LocalCharacterId, Is.EqualTo("local-character-1"));
        Assert.That(actor.ActorKind, Is.EqualTo(CombatActorKind.Player));
        Assert.That(actor.ZoneId, Is.EqualTo("Haunt_Prototype_T1"));
        Assert.That(actor.TargetCombatActorId, Is.Null);
        Assert.That(actor.ThreatTable, Is.Empty);
    }

    [Test]
    public void test_combat_actor_state_rejects_transient_threat_table_negative_values()
    {
        var actor = CreateValidPlayerActor(new Dictionary<string, int>
        {
            ["combat-hostile-1"] = -1
        });

        var validation = actor.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors, Has.Some.Contains("threat_table values must not be negative"));
    }

    [Test]
    public void test_combat_actor_state_has_no_unity_scene_object_fields()
    {
        var forbiddenTypeNames = typeof(CombatActorState)
            .GetProperties()
            .Select(property => property.PropertyType.FullName ?? property.PropertyType.Name)
            .Where(name =>
                name.Contains("UnityEngine", StringComparison.Ordinal) ||
                name.Contains("GameObject", StringComparison.Ordinal) ||
                name.Contains("Transform", StringComparison.Ordinal) ||
                name.Contains("Animator", StringComparison.Ordinal) ||
                name.Contains("Material", StringComparison.Ordinal) ||
                name.Contains("Texture", StringComparison.Ordinal))
            .ToArray();

        Assert.That(forbiddenTypeNames, Is.Empty);
    }

    private static CombatActorState CreateValidPlayerActor(IReadOnlyDictionary<string, int>? threatTable = null)
    {
        return new CombatActorState(
            "combat-player-1",
            CombatActorKind.Player,
            CombatStableSourceRef.ForPlayer("local-character-1"),
            "PlayerLocal_T1",
            "Haunt_Prototype_T1",
            5,
            140,
            140,
            180,
            180,
            35,
            25,
            8,
            30,
            30,
            2.8d,
            2.0d,
            30.0d,
            CombatState.OutOfCombat,
            CombatActorLifeState.Alive,
            null,
            "player-local-character-1",
            threatTable);
    }
}
