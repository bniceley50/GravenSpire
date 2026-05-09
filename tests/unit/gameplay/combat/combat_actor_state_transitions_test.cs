#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gravenspire.Gameplay.Combat;
using NUnit.Framework;

namespace Gravenspire.Tests.Unit.Gameplay.Combat;

public sealed class CombatActorStateTransitionsTest
{
    private static readonly string[] ExpectedInitOnlyRuntimeProperties =
    {
        nameof(CombatActorState.ActiveCastId),
        nameof(CombatActorState.ActiveCastSpellId),
        nameof(CombatActorState.ActiveCastTargetCombatActorId),
        nameof(CombatActorState.CastProgressSeconds),
        nameof(CombatActorState.CastRecoveryRemainingSeconds),
        nameof(CombatActorState.CastRuntimeState),
        nameof(CombatActorState.CombatExitRemainingSeconds),
        nameof(CombatActorState.LastHostileActionTickIndex),
        nameof(CombatActorState.NextRegenTickIndex),
        nameof(CombatActorState.PostureState)
    };

    [Test]
    public void test_transition_guard_covers_all_init_only_runtime_properties()
    {
        var initOnlyRuntimeProperties = typeof(CombatActorState)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.SetMethod is not null)
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.That(initOnlyRuntimeProperties, Is.EqualTo(ExpectedInitOnlyRuntimeProperties.OrderBy(name => name, StringComparer.Ordinal).ToArray()));
    }

    [Test]
    public void test_shared_copy_transitions_preserve_init_only_runtime_properties()
    {
        var actor = RuntimeRichActor();

        var updated = actor
            .WithTarget("combat-hostile-2")
            .SetThreat("combat-hostile-2", 75)
            .WithCombatState(CombatState.InCombat);

        Assert.That(updated.TargetCombatActorId, Is.EqualTo("combat-hostile-2"));
        Assert.That(updated.ThreatTable["combat-hostile-2"], Is.EqualTo(75));
        Assert.That(updated.CombatState, Is.EqualTo(CombatState.InCombat));
        AssertInitOnlyRuntimePropertiesPreserved(updated, actor);
    }

    [Test]
    public void test_resource_copy_transitions_preserve_init_only_runtime_properties()
    {
        var actor = RuntimeRichActor();

        var updated = actor
            .WithCurrentMana(150)
            .WithCurrentEndurance(65);

        Assert.That(updated.CurrentMana, Is.EqualTo(150));
        Assert.That(updated.CurrentEndurance, Is.EqualTo(65));
        AssertInitOnlyRuntimePropertiesPreserved(updated, actor);
    }

    [Test]
    public void test_ability_damage_copy_preserves_init_only_runtime_properties()
    {
        var actor = RuntimeRichActor(currentHealth: 120);

        var damaged = actor.WithCurrentHealthAfterAbilityDamage(35);

        Assert.That(damaged.CurrentHealth, Is.EqualTo(85));
        Assert.That(damaged.LifeState, Is.EqualTo(CombatActorLifeState.Alive));
        Assert.That(damaged.CombatState, Is.EqualTo(actor.CombatState));
        AssertInitOnlyRuntimePropertiesPreserved(damaged, actor);
    }

    private static CombatActorState RuntimeRichActor(int currentHealth = 140)
    {
        return new CombatActorState(
            "combat-player-1",
            CombatActorKind.Player,
            CombatStableSourceRef.ForPlayer("local-character-1"),
            "PlayerLocal_T1",
            "Haunt_Prototype_T1",
            5,
            140,
            currentHealth,
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
            CombatState.Casting,
            CombatActorLifeState.Alive,
            "combat-hostile-1",
            "player-local-character-1",
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["combat-hostile-1"] = 50
            },
            maxEndurance: 80,
            currentEndurance: 80) with
        {
            CastRuntimeState = CombatCastRuntimeState.Casting,
            ActiveCastId = "cast-preserve-001",
            ActiveCastSpellId = "Smite_T1_Prototype",
            ActiveCastTargetCombatActorId = "combat-hostile-1",
            CastProgressSeconds = 2.5d,
            CastRecoveryRemainingSeconds = 0.75d,
            PostureState = CombatPostureState.Sitting,
            NextRegenTickIndex = 240,
            LastHostileActionTickIndex = 120,
            CombatExitRemainingSeconds = 8.5d
        };
    }

    private static void AssertInitOnlyRuntimePropertiesPreserved(CombatActorState actual, CombatActorState expected)
    {
        Assert.That(actual.CastRuntimeState, Is.EqualTo(expected.CastRuntimeState));
        Assert.That(actual.ActiveCastId, Is.EqualTo(expected.ActiveCastId));
        Assert.That(actual.ActiveCastSpellId, Is.EqualTo(expected.ActiveCastSpellId));
        Assert.That(actual.ActiveCastTargetCombatActorId, Is.EqualTo(expected.ActiveCastTargetCombatActorId));
        Assert.That(actual.CastProgressSeconds, Is.EqualTo(expected.CastProgressSeconds));
        Assert.That(actual.CastRecoveryRemainingSeconds, Is.EqualTo(expected.CastRecoveryRemainingSeconds));
        Assert.That(actual.PostureState, Is.EqualTo(expected.PostureState));
        Assert.That(actual.NextRegenTickIndex, Is.EqualTo(expected.NextRegenTickIndex));
        Assert.That(actual.LastHostileActionTickIndex, Is.EqualTo(expected.LastHostileActionTickIndex));
        Assert.That(actual.CombatExitRemainingSeconds, Is.EqualTo(expected.CombatExitRemainingSeconds));
    }
}
