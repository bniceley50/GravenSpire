#nullable enable

using System;
using System.IO;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Combat.Fixtures;
using NUnit.Framework;

namespace Gravenspire.Tests.Unit.Gameplay.Combat;

public sealed class CombatMeleeFormulasTest
{
    [Test]
    public void test_melee_hit_chance_equal_level_equal_skills_returns_design_example()
    {
        var attacker = CreateActor("combat-attacker", "attacker", level: 5, attackSkill: 30, defenseSkill: 30);
        var defender = CreateActor("combat-defender", "defender", level: 5, attackSkill: 30, defenseSkill: 30);

        var result = CombatMeleeFormulas.CalculateHitChance(attacker, defender, DefaultHitTuning());

        Assert.That(result, Is.EqualTo(0.72d).Within(0.000001d));
    }

    [TestCase(1, 60, 0, 300, 0.10d)]
    [TestCase(60, 1, 300, 0, 0.92d)]
    [TestCase(5, 5, 30, 30, 0.72d)]
    public void test_melee_hit_chance_clamps_and_honors_boundary_inputs(
        int attackerLevel,
        int defenderLevel,
        int attackerSkill,
        int defenderSkill,
        double expected)
    {
        var attacker = CreateActor("combat-attacker", "attacker", attackerLevel, attackerSkill, defenseSkill: 30);
        var defender = CreateActor("combat-defender", "defender", defenderLevel, attackSkill: 30, defenderSkill);

        var result = CombatMeleeFormulas.CalculateHitChance(attacker, defender, DefaultHitTuning());

        Assert.That(result, Is.EqualTo(expected).Within(0.000001d));
    }

    [Test]
    public void test_melee_damage_design_example_returns_nine_damage()
    {
        var attacker = CreateActor(
            "combat-attacker",
            "attacker",
            level: 5,
            attackSkill: 30,
            defenseSkill: 30,
            attackPower: 20,
            weaponBaseDamage: 8);
        var defender = CreateActor(
            "combat-defender",
            "defender",
            level: 5,
            attackSkill: 30,
            defenseSkill: 30,
            armorClass: 30);

        var result = CombatMeleeFormulas.CalculateDamage(attacker, defender, DefaultDamageTuning(), damageRollScalar: 1.0d);

        Assert.That(result, Is.EqualTo(9));
    }

    [Test]
    public void test_melee_damage_successful_hit_never_drops_below_minimum_damage()
    {
        var attacker = CreateActor(
            "combat-attacker",
            "attacker",
            level: 1,
            attackSkill: 1,
            defenseSkill: 1,
            attackPower: 0,
            weaponBaseDamage: 1);
        var defender = CreateActor(
            "combat-defender",
            "defender",
            level: 10,
            attackSkill: 60,
            defenseSkill: 60,
            armorClass: 500);

        var result = CombatMeleeFormulas.CalculateDamage(attacker, defender, DefaultDamageTuning(), damageRollScalar: 0.70d);

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void test_seeded_fixture_melee_damage_executes_low_and_top_extremes_inside_trash_band()
    {
        var package = LoadPackage();
        var clericLow = Actor(package, "Cleric_Low_T1", "combat-cleric-low", "cleric-low");
        var clericTop = Actor(package, "Cleric_Top_T1", "combat-cleric-top", "cleric-top");
        var trashLow = Actor(package, "Trash_Low_T1", "combat-trash-low", "trash-low");
        var trashTop = Actor(package, "Trash_Top_T1", "combat-trash-top", "trash-top");

        var lowDefault = CombatMeleeFormulas.CalculateDamage(clericLow, trashLow, DefaultDamageTuning(), damageRollScalar: 1.0d);
        var topLowRoll = CombatMeleeFormulas.CalculateDamage(clericTop, trashTop, DefaultDamageTuning(), damageRollScalar: 0.85d);
        var topHighRoll = CombatMeleeFormulas.CalculateDamage(clericTop, trashTop, DefaultDamageTuning(), damageRollScalar: 1.15d);
        var armorClamp = CombatMeleeFormulas.CalculateDamage(clericLow, CopyWithArmorClass(trashTop, armorClass: 500), DefaultDamageTuning(), damageRollScalar: 0.70d);

        Assert.That(lowDefault, Is.InRange(2, 20));
        Assert.That(topLowRoll, Is.InRange(2, 20));
        Assert.That(topHighRoll, Is.InRange(2, 20));
        Assert.That(armorClamp, Is.EqualTo(1));
    }

    private static CombatMeleeHitChanceTuning DefaultHitTuning()
    {
        return new CombatMeleeHitChanceTuning(
            BaseHitChance: 0.72d,
            LevelHitDelta: 0.03d,
            SkillHitDelta: 0.001d,
            HitChanceMin: 0.10d,
            HitChanceMax: 0.92d);
    }

    private static CombatMeleeDamageTuning DefaultDamageTuning()
    {
        return new CombatMeleeDamageTuning(
            AttackPowerScalar: 0.20d,
            ArmorMitigationScalar: 0.10d);
    }

    private static CombatActorState Actor(
        CombatFixturePackage package,
        string fixtureId,
        string combatActorId,
        string sortKey)
    {
        var fixture = package.ActorFixtures.Single(actor => actor.Id == fixtureId);
        return new CombatActorState(
            combatActorId,
            fixture.ActorKind,
            fixture.ActorKind == CombatActorKind.Player
                ? CombatStableSourceRef.ForPlayer($"{sortKey}-local")
                : CombatStableSourceRef.ForSpawn(new CombatSpawnSourceRef("VampireCourt_T1", $"{sortKey}-anchor", fixture.Id)),
            fixture.FactionId,
            "Haunt_Prototype_T1",
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
            sortKey);
    }

    private static CombatActorState CreateActor(
        string combatActorId,
        string sortKey,
        int level,
        int attackSkill,
        int defenseSkill,
        int attackPower = 25,
        int weaponBaseDamage = 8,
        int armorClass = 30)
    {
        return new CombatActorState(
            combatActorId,
            CombatActorKind.NPC,
            CombatStableSourceRef.ForSpawn(new CombatSpawnSourceRef("VampireCourt_T1", $"{sortKey}-anchor", "FormulaFixture_T1")),
            "VampireCourt_T1",
            "Haunt_Prototype_T1",
            level,
            120,
            120,
            0,
            0,
            armorClass,
            attackPower,
            weaponBaseDamage,
            attackSkill,
            defenseSkill,
            3.0d,
            2.0d,
            0.0d,
            CombatState.OutOfCombat,
            CombatActorLifeState.Alive,
            null,
            sortKey);
    }

    private static CombatActorState CopyWithArmorClass(CombatActorState actor, int armorClass)
    {
        return new CombatActorState(
            actor.CombatActorId,
            actor.ActorKind,
            actor.StableSourceRef,
            actor.FactionId,
            actor.ZoneId,
            actor.Level,
            actor.MaxHealth,
            actor.CurrentHealth,
            actor.MaxMana,
            actor.CurrentMana,
            armorClass,
            actor.AttackPower,
            actor.WeaponBaseDamage,
            actor.AttackSkill,
            actor.DefenseSkill,
            actor.WeaponDelaySeconds,
            actor.MeleeRangeMeters,
            actor.SpellRangeMeters,
            actor.CombatState,
            actor.LifeState,
            actor.TargetCombatActorId,
            actor.CombatSortKey,
            actor.ThreatTable);
    }

    private static CombatFixturePackage LoadPackage()
    {
        var path = Path.Combine(FindRepoRoot(), "assets", "data", "combat", "t1-combat-fixtures.json");
        return new CombatFixtureLoader().LoadFromFile(path);
    }

    private static string FindRepoRoot()
    {
        var candidates = new[]
        {
            new DirectoryInfo(TestContext.CurrentContext.TestDirectory),
            new DirectoryInfo(Directory.GetCurrentDirectory())
        };

        foreach (var candidate in candidates)
        {
            for (var directory = candidate; directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")) &&
                    Directory.Exists(Path.Combine(directory.FullName, "assets")))
                {
                    return directory.FullName;
                }
            }
        }

        throw new DirectoryNotFoundException("Unable to locate repository root for combat formula tests.");
    }
}
