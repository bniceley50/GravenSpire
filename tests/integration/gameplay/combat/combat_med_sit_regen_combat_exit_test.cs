#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Combat.Fixtures;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Gameplay.Combat;

public sealed class CombatMedSitRegenCombatExitTest
{
    [Test]
    public void test_successful_sit_forces_attack_off_before_regen_tick_resolves()
    {
        var package = LoadPackage();
        var player = CreatePlayer(package, currentMana: 100).WithTarget("combat-hostile-1");
        var hostile = CreateHostile(package, "combat-hostile-1", "hostile-001");
        var attack = EnabledAttackMachine(player, hostile, package);
        var posture = new CombatPostureStateMachine();
        var regen = new CombatRegenResolver();

        var sit = posture.TrySit(new CombatSitRequest(
            player,
            attack,
            new CombatTick(160, 3.2d),
            IsGrounded: true,
            IsMoving: false,
            IsZoneLoadingCommitLocked: false));
        var tick = regen.ResolveTick(new CombatRegenTickRequest(
            sit.Player,
            package.RegenAndCombatExitTuning,
            new CombatTick(300, 6.0d),
            package.CombatTickRateHz));

        Assert.That(sit.Succeeded, Is.True, string.Join(Environment.NewLine, sit.RejectionReasons));
        Assert.That(sit.AttackTransition.StateChangedSignal!.TransitionPath, Is.EqualTo(CombatAttackTransitionPath.SuccessfulSitOrMed));
        Assert.That(attack.StateChangedSignals.Last().AttackOn, Is.False);
        Assert.That(tick.ManaRestored, Is.EqualTo(8));
        Assert.That(tick.Actor.CurrentMana, Is.EqualTo(108));
    }

    [Test]
    public void test_out_of_combat_sitting_mana_regen_clamps_to_max_mana()
    {
        var package = LoadPackage();
        var player = CreatePlayer(package, currentMana: 176) with
        {
            PostureState = CombatPostureState.Sitting
        };
        var regen = new CombatRegenResolver();

        var result = regen.ResolveTick(new CombatRegenTickRequest(
            player,
            package.RegenAndCombatExitTuning,
            new CombatTick(300, 6.0d),
            package.CombatTickRateHz));

        Assert.That(result.ManaRestored, Is.EqualTo(4));
        Assert.That(result.Actor.CurrentMana, Is.EqualTo(player.MaxMana));
    }

    [Test]
    public void test_sitting_in_combat_applies_threat_bonus_and_no_mana_med_boost()
    {
        var package = LoadPackage();
        var player = CreatePlayer(package, currentMana: 100).WithCombatState(CombatState.InCombat) with
        {
            PostureState = CombatPostureState.Sitting
        };
        var hostile = CreateHostile(package, "combat-hostile-1", "hostile-001")
            .SetThreat(player.CombatActorId, 25);
        var threat = new CombatThreatResolver();
        var regen = new CombatRegenResolver();

        var threatResult = threat.ApplySittingThreatBonus(new CombatSittingThreatRequest(
            player,
            new[] { hostile },
            package.RegenAndCombatExitTuning));
        var regenResult = regen.ResolveTick(new CombatRegenTickRequest(
            player,
            package.RegenAndCombatExitTuning,
            new CombatTick(300, 6.0d),
            package.CombatTickRateHz));

        Assert.That(threatResult.UpdatedThreatEntries, Is.EqualTo(1));
        Assert.That(threatResult.HostileActors.Single().ThreatTable[player.CombatActorId], Is.EqualTo(75));
        Assert.That(regenResult.ManaRestored, Is.EqualTo(0));
        Assert.That(regenResult.Actor.CurrentMana, Is.EqualTo(100));
    }

    [Test]
    public void test_combat_exit_uses_zero_valid_hostile_threat_entries_from_existing_threat_tables()
    {
        var package = LoadPackage();
        var player = CreatePlayer(package);
        var hostile = CreateHostile(package, "combat-hostile-1", "hostile-001")
            .SetThreat(player.CombatActorId, 25);
        var threat = new CombatThreatResolver();
        var exit = new CombatExitStateMachine();

        var stillThreatenedCount = threat.CountValidHostileThreatEntries(player, new[] { hostile });
        var stillThreatened = exit.Evaluate(new CombatExitTimerRequest(
            new CombatTick(1505, 30.1d),
            CombatTick.Zero,
            package.CombatTickRateHz,
            stillThreatenedCount,
            package.RegenAndCombatExitTuning));
        var releasedCount = threat.CountValidHostileThreatEntries(player, new[] { hostile.ReleaseHostile() });
        var released = exit.Evaluate(new CombatExitTimerRequest(
            new CombatTick(1505, 30.1d),
            CombatTick.Zero,
            package.CombatTickRateHz,
            releasedCount,
            package.RegenAndCombatExitTuning));

        Assert.That(stillThreatenedCount, Is.EqualTo(1));
        Assert.That(stillThreatened.CanExitCombat, Is.False);
        Assert.That(releasedCount, Is.EqualTo(0));
        Assert.That(released.CanExitCombat, Is.True);
    }

    [Test]
    public void test_sit_command_during_active_cast_is_rejected_by_rule_19_precondition()
    {
        var package = LoadPackage();
        var player = CreatePlayer(package).BeginCast(
            "cast-001",
            "Smite_T1_Prototype",
            "combat-hostile-1");
        var attack = new CombatAttackStateMachine();
        var posture = new CombatPostureStateMachine();

        var result = posture.TrySit(new CombatSitRequest(
            player,
            attack,
            new CombatTick(10, 0.2d),
            IsGrounded: true,
            IsMoving: false,
            IsZoneLoadingCommitLocked: false));

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.RejectionReasons, Has.Some.Contains("cannot sit while casting"));
        Assert.That(result.Player.PostureState, Is.EqualTo(CombatPostureState.Standing));
    }

    private static CombatAttackStateMachine EnabledAttackMachine(
        CombatActorState player,
        CombatActorState hostile,
        CombatFixturePackage package)
    {
        var gate = new CombatZoneGate();
        gate.ActivateZone(player.ZoneId, CombatZoneType.HauntZone);
        var machine = new CombatAttackStateMachine();
        var enabled = machine.ToggleOn(new CombatAttackToggleOnRequest(
            player,
            hostile,
            gate,
            DistanceMetersToTarget: 1.5d,
            new CombatTick(10, 0.2d),
            package.CombatTickRateHz));

        Assert.That(enabled.Succeeded, Is.True, string.Join(Environment.NewLine, enabled.RejectionReasons));
        return machine;
    }

    private static CombatActorState CreatePlayer(CombatFixturePackage package, int? currentMana = null)
    {
        var fixture = package.ActorFixtures.Single(actor => actor.Id == "Cleric_Mid_T1");
        return new CombatActorState(
            "combat-player-1",
            fixture.ActorKind,
            CombatStableSourceRef.ForPlayer("local-character-1"),
            fixture.FactionId,
            "Haunt_Prototype_T1",
            fixture.Level,
            fixture.MaxHealth,
            fixture.MaxHealth,
            fixture.MaxMana,
            currentMana ?? fixture.MaxMana,
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
            "player-local-character-1");
    }

    private static CombatActorState CreateHostile(CombatFixturePackage package, string combatActorId, string sortKey)
    {
        var fixture = package.ActorFixtures.Single(actor => actor.Id == "Trash_Mid_T1");
        return new CombatActorState(
            combatActorId,
            fixture.ActorKind,
            CombatStableSourceRef.ForSpawn(new CombatSpawnSourceRef("VampireCourt_T1", $"{combatActorId}-anchor", "VampireThrall_T1")),
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
            CombatState.InCombat,
            CombatActorLifeState.Alive,
            "combat-player-1",
            sortKey);
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

        throw new DirectoryNotFoundException("Unable to locate repository root for combat med/sit tests.");
    }
}
