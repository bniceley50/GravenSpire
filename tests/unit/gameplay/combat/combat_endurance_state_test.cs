#nullable enable

using System;
using System.Linq;
using System.Reflection;
using Gravenspire.Gameplay.Combat;
using NUnit.Framework;

namespace Gravenspire.Tests.Unit.Gameplay.Combat;

public sealed class CombatEnduranceStateTest
{
    [Test]
    public void test_qa_01_01_endurance_actor_state_validates_clamps_and_round_trips()
    {
        var actor = CreatePlayer(maxEndurance: 80, currentEndurance: 55);

        var validation = actor.Validate();
        var updated = actor.WithCurrentEndurance(25);
        var invalidMax = CreatePlayer(maxEndurance: -1, currentEndurance: 0).Validate();
        var invalidCurrent = CreatePlayer(maxEndurance: 80, currentEndurance: -1).Validate();
        var overMax = CreatePlayer(maxEndurance: 80, currentEndurance: 81).Validate();

        Assert.That(validation.IsValid, Is.True, string.Join(Environment.NewLine, validation.Errors));
        Assert.That(actor.MaxEndurance, Is.EqualTo(80));
        Assert.That(actor.CurrentEndurance, Is.EqualTo(55));
        Assert.That(updated.MaxEndurance, Is.EqualTo(80));
        Assert.That(updated.CurrentEndurance, Is.EqualTo(25));
        Assert.That(invalidMax.Errors, Has.Some.Contains("max_endurance must not be negative"));
        Assert.That(invalidCurrent.Errors, Has.Some.Contains("current_endurance must not be negative"));
        Assert.That(overMax.Errors, Has.Some.Contains("current_endurance must not exceed max_endurance"));
    }

    [Test]
    public void test_qa_01_02_persistence_projection_exposes_prior_four_fields_plus_current_endurance()
    {
        var properties = typeof(CombatPersistenceProjection)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
        var names = properties.Select(property => property.Name).ToArray();

        Assert.That(properties, Has.Length.EqualTo(5));
        Assert.That(names, Is.EqualTo(new[]
        {
            "combat_life_state",
            "current_endurance",
            "current_health",
            "current_mana",
            "pending_death_handoff_payload"
        }.OrderBy(name => name, StringComparer.Ordinal).ToArray()));
        Assert.That(properties, Has.All.Property(nameof(PropertyInfo.CanWrite)).False);
    }

    [Test]
    public void test_qa_01_03_persistence_projection_still_excludes_transient_combat_state()
    {
        var names = typeof(CombatPersistenceProjection)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .ToArray();

        Assert.That(names, Has.No.Member("max_endurance"));
        Assert.That(names, Has.No.Member("combat_" + "actor_id"));
        Assert.That(names, Has.No.Member("ThreatTable"));
        Assert.That(names, Has.No.Member("TargetCombatActorId"));
        Assert.That(names, Has.No.Member("CastRuntimeState"));
        Assert.That(names, Has.No.Member("PostureState"));
        Assert.That(names, Has.No.Member("NextRegenTickIndex"));
        Assert.That(names, Has.No.Member("ActiveCastId"));
        Assert.That(names, Has.No.Member("WeaponDelaySeconds"));
    }

    [Test]
    public void test_qa_01_04_hud_projection_exposes_categorical_endurance_signal()
    {
        var player = CreatePlayer(maxEndurance: 80, currentEndurance: 40);

        var snapshot = CombatHudStateProjection.Project(new CombatHudProjectionRequest(
            player,
            Target: null,
            new CombatAttackStateSnapshot(CombatAttackMode.Off, null, null, null),
            CombatTick.Zero));
        var enduranceProperty = typeof(CombatHudStateSnapshot).GetProperty(nameof(CombatHudStateSnapshot.Endurance));

        Assert.That(snapshot.Endurance, Is.EqualTo(CombatHudEnduranceCategory.Available));
        Assert.That(enduranceProperty, Is.Not.Null);
        Assert.That(enduranceProperty!.PropertyType, Is.EqualTo(typeof(CombatHudEnduranceCategory)));
        Assert.That(enduranceProperty.PropertyType, Is.Not.EqualTo(typeof(CombatHudResourceSnapshot)));
        Assert.That(enduranceProperty.PropertyType, Is.Not.EqualTo(typeof(int)));
    }

    [Test]
    public void test_qa_01_05_hud_projection_has_no_ui_or_unity_dependency()
    {
        var hudTypes = typeof(CombatHudStateProjection).Assembly.GetTypes()
            .Where(type => string.Equals(type.Namespace, "Gravenspire.Gameplay.Combat", StringComparison.Ordinal))
            .Where(type => type.Name.StartsWith("CombatHud", StringComparison.Ordinal))
            .ToArray();
        var forbiddenMatches = hudTypes
            .SelectMany(type => type.GetProperties().Select(property => property.PropertyType.FullName ?? property.PropertyType.Name).Append(type.FullName ?? type.Name))
            .Where(name =>
                name.Contains("UnityEngine", StringComparison.Ordinal) ||
                name.Contains("MonoBehaviour", StringComparison.Ordinal) ||
                name.Contains("VisualElement", StringComparison.Ordinal) ||
                name.Contains("UnityEngine.UI", StringComparison.Ordinal) ||
                name.Contains("UnityEngine.UIElements", StringComparison.Ordinal))
            .ToArray();

        Assert.That(hudTypes, Is.Not.Empty);
        Assert.That(forbiddenMatches, Is.Empty);
    }

    [Test]
    public void test_qa_01_06_baseline_snapshot_remains_endurance_free_while_persistence_adds_endurance()
    {
        var baselineNames = typeof(CombatProgressionBaselineSnapshot)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .ToArray();
        var persistenceNames = typeof(CombatPersistenceProjection)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .ToArray();

        Assert.That(baselineNames.Any(name => name.Contains("Endurance", StringComparison.Ordinal)), Is.False);
        Assert.That(persistenceNames, Has.Member("current_endurance"));
        Assert.That(persistenceNames, Has.No.Member("max_endurance"));
    }

    private static CombatActorState CreatePlayer(int maxEndurance, int currentEndurance)
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
            maxEndurance: maxEndurance,
            currentEndurance: currentEndurance);
    }
}
