#nullable enable

using System;

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Authored melee hit-chance parameters supplied by fixture or test data.
/// </summary>
public sealed record CombatMeleeHitChanceTuning(
    double BaseHitChance,
    double LevelHitDelta,
    double SkillHitDelta,
    double HitChanceMin,
    double HitChanceMax)
{
    public void Validate()
    {
        if (BaseHitChance < 0 || BaseHitChance > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(BaseHitChance), "Base hit chance must be a ratio.");
        }

        if (LevelHitDelta < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(LevelHitDelta), "Level hit delta must not be negative.");
        }

        if (SkillHitDelta < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(SkillHitDelta), "Skill hit delta must not be negative.");
        }

        if (HitChanceMin < 0 || HitChanceMin > HitChanceMax || HitChanceMax > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(HitChanceMin), "Hit chance clamp bounds must be ordered ratios.");
        }
    }
}

/// <summary>
/// Authored melee damage parameters supplied by fixture or test data.
/// </summary>
public sealed record CombatMeleeDamageTuning(
    double AttackPowerScalar,
    double ArmorMitigationScalar)
{
    public void Validate()
    {
        if (AttackPowerScalar < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(AttackPowerScalar), "Attack power scalar must not be negative.");
        }

        if (ArmorMitigationScalar < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ArmorMitigationScalar), "Armor mitigation scalar must not be negative.");
        }
    }
}

/// <summary>
/// Pure Combat Core melee formulas. Coefficients are injected instead of hardcoded.
/// </summary>
public static class CombatMeleeFormulas
{
    public static double CalculateHitChance(
        CombatActorState attacker,
        CombatActorState defender,
        CombatMeleeHitChanceTuning tuning)
    {
        CombatArgumentNull.ThrowIfNull(attacker);
        CombatArgumentNull.ThrowIfNull(defender);
        CombatArgumentNull.ThrowIfNull(tuning);

        tuning.Validate();

        var levelDelta = attacker.Level - defender.Level;
        var skillDelta = attacker.AttackSkill - defender.DefenseSkill;
        var unclamped = tuning.BaseHitChance +
            (levelDelta * tuning.LevelHitDelta) +
            (skillDelta * tuning.SkillHitDelta);

        return Math.Clamp(unclamped, tuning.HitChanceMin, tuning.HitChanceMax);
    }

    public static int CalculateDamage(
        CombatActorState attacker,
        CombatActorState defender,
        CombatMeleeDamageTuning tuning,
        double damageRollScalar)
    {
        CombatArgumentNull.ThrowIfNull(attacker);
        CombatArgumentNull.ThrowIfNull(defender);
        CombatArgumentNull.ThrowIfNull(tuning);

        tuning.Validate();

        if (damageRollScalar <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(damageRollScalar), "Damage roll scalar must be positive.");
        }

        var preMitigation = (attacker.WeaponBaseDamage + (attacker.AttackPower * tuning.AttackPowerScalar)) *
            damageRollScalar;
        var mitigated = preMitigation - (defender.ArmorClass * tuning.ArmorMitigationScalar);

        return Math.Max(1, (int)Math.Floor(mitigated));
    }
}
