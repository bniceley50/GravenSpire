#nullable enable

using System;
using Gravenspire.Gameplay.Combat;
using NUnit.Framework;

namespace Gravenspire.Tests.Unit.Gameplay.Combat;

public sealed class CombatClockTest
{
    [Test]
    public void test_combat_clock_advance_ticks_uses_fixed_duration()
    {
        var clock = new FixedCombatClock(tickRateHz: 50);

        var tick = clock.AdvanceTicks(25);

        Assert.That(tick.Index, Is.EqualTo(25));
        Assert.That(tick.ElapsedSeconds, Is.EqualTo(0.5d).Within(0.000001d));
        Assert.That(clock.TickDurationSeconds, Is.EqualTo(0.02d).Within(0.000001d));
    }

    [Test]
    public void test_combat_clock_rejects_invalid_tick_rate()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FixedCombatClock(0));
    }

    [Test]
    public void test_combat_clock_reset_returns_known_tick()
    {
        var clock = new FixedCombatClock(tickRateHz: 50);
        clock.AdvanceTicks(100);

        var tick = clock.Reset(10);

        Assert.That(tick.Index, Is.EqualTo(10));
        Assert.That(clock.CurrentTick, Is.EqualTo(10));
        Assert.That(clock.ElapsedSeconds, Is.EqualTo(0.2d).Within(0.000001d));
    }
}
