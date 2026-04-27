// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can Cleric tab-target combat, slow cast cadence, mana pressure, and med-break recovery make the silence between pulls feel intentional rather than empty?
// Date: 2026-04-26

using System;
using UnityEngine;

namespace Gravenspire.Prototypes.CombatFeel
{
    [Serializable]
    public sealed class CadenceKnobs
    {
        [Header("Loop")]
        [Min(3)] public int pullsToComplete = 5;
        [Min(0f)] public float globalCooldownSeconds = 1.25f;
        [Min(0f)] public float pullWindupSeconds = 1.1f;
        [Min(0f)] public float betweenPullAmbientSeconds = 7.5f;

        [Header("Readiness Gates")]
        [Range(0f, 1f)] public float recommendedPullManaPercent = 0.42f;
        [Range(0f, 1f)] public float recommendedPullHealthPercent = 0.55f;
        public bool blockUnsafePulls = true;

        [Header("Cleric")]
        [Min(1)] public int clericMaxHealth = 115;
        [Min(1)] public int clericMaxMana = 132;
        [Min(0f)] public float clericAutoAttackSeconds = 2.6f;
        [Min(0)] public int clericAutoAttackMinDamage = 5;
        [Min(0)] public int clericAutoAttackMaxDamage = 8;
        [Min(0f)] public float standingManaRegenPerSecond = 0.35f;
        [Min(0f)] public float betweenPullManaRegenPerSecond = 0.8f;
        [Min(0f)] public float medManaRegenPerSecond = 7.2f;
        [Min(0f)] public float medHealthRegenPerSecond = 1.6f;

        [Header("Smite")]
        [Min(0f)] public float smiteCastSeconds = 4.1f;
        [Min(0)] public int smiteManaCost = 34;
        [Min(0)] public int smiteMinDamage = 32;
        [Min(0)] public int smiteMaxDamage = 39;

        [Header("Heal")]
        [Min(0f)] public float healCastSeconds = 4.6f;
        [Min(0)] public int healManaCost = 29;
        [Min(0)] public int healMinAmount = 28;
        [Min(0)] public int healMaxAmount = 36;

        [Header("Smite of Authority")]
        [Min(0f)] public float authorityCooldownSeconds = 7.0f;
        [Min(0)] public int authorityManaCost = 10;
        [Min(0)] public int authorityMinDamage = 13;
        [Min(0)] public int authorityMaxDamage = 16;

        [Header("Bash")]
        [Min(0f)] public float bashCooldownSeconds = 10.0f;
        [Min(0)] public int bashManaCost = 10;
        [Min(0)] public int bashMinDamage = 7;
        [Min(0)] public int bashMaxDamage = 11;
        [Min(0f)] public float bashInterruptSeconds = 1.0f;

        [Header("Defensive Prayer")]
        [Min(0f)] public float defensivePrayerCooldownSeconds = 30.0f;
        [Min(0)] public int defensivePrayerManaCost = 25;
        [Min(0f)] public float defensivePrayerDurationSeconds = 8.0f;
        [Range(0f, 0.9f)] public float defensivePrayerDamageReduction = 0.2f;

        [Header("Hostiles")]
        [Min(0f)] public float hostileAttackSeconds = 2.9f;
        [Min(0)] public int hostileDamageVariance = 2;
        [Min(0f)] public float hostilePressureGraceSeconds = 2.0f;

        public int RecommendedPullMana => Mathf.CeilToInt(clericMaxMana * recommendedPullManaPercent);
        public int RecommendedPullHealth => Mathf.CeilToInt(clericMaxHealth * recommendedPullHealthPercent);
    }
}
