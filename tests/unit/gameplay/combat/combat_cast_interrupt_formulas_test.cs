#nullable enable

using System;
using Gravenspire.Gameplay.Combat;
using NUnit.Framework;

namespace Gravenspire.Tests.Unit.Gameplay.Combat;

public sealed class CombatCastInterruptFormulasTest
{
    [Test]
    public void test_interrupt_formula_matches_combat_core_worked_example()
    {
        var chance = CombatCastFormulas.CalculateInterruptChance(
            damageTaken: 10,
            maxHealth: 100,
            castTimeRemainingSeconds: 2.0d,
            castTimeTotalSeconds: 4.0d,
            interruptResistance: 0d,
            DefaultTuning());

        Assert.That(chance, Is.EqualTo(0.65d).Within(0.000001d));
    }

    [TestCase(90, 100, 4.0d, 4.0d, 0.85d)]
    [TestCase(1, 1000, 0.0d, 4.0d, 0.05d)]
    public void test_interrupt_formula_clamps_to_injected_bounds(
        int damageTaken,
        int maxHealth,
        double remaining,
        double total,
        double expected)
    {
        var chance = CombatCastFormulas.CalculateInterruptChance(
            damageTaken,
            maxHealth,
            remaining,
            total,
            interruptResistance: 0.40d,
            DefaultTuning());

        Assert.That(chance, Is.EqualTo(expected).Within(0.000001d));
    }

    [Test]
    public void test_interrupt_formula_rejects_invalid_ratio_bounds()
    {
        var invalid = new CombatInterruptFormulaTuning(
            BaseInterruptChance: 0.20d,
            DamageInterruptScalar: 4.0d,
            EarlyCastInterruptScalar: 0.10d,
            InterruptChanceMin: 0.90d,
            InterruptChanceMax: 0.10d);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CombatCastFormulas.CalculateInterruptChance(
                damageTaken: 10,
                maxHealth: 100,
                castTimeRemainingSeconds: 2.0d,
                castTimeTotalSeconds: 4.0d,
                interruptResistance: 0d,
                invalid));
    }

    [Test]
    public void test_seconds_to_ticks_uses_ceiling_against_injected_tick_rate()
    {
        Assert.That(CombatCastFormulas.SecondsToTicksCeiling(6.0d, 50), Is.EqualTo(300));
        Assert.That(CombatCastFormulas.SecondsToTicksCeiling(1.5d, 50), Is.EqualTo(75));
    }

    private static CombatInterruptFormulaTuning DefaultTuning()
    {
        return new CombatInterruptFormulaTuning(
            BaseInterruptChance: 0.20d,
            DamageInterruptScalar: 4.0d,
            EarlyCastInterruptScalar: 0.10d,
            InterruptChanceMin: 0.05d,
            InterruptChanceMax: 0.85d);
    }
}
