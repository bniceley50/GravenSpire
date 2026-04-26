// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can Cleric tab-target combat, slow cast cadence, mana pressure, and med-break recovery make the silence between pulls feel intentional rather than empty?
// Date: 2026-04-26

using System;
using UnityEngine;

namespace Gravenspire.Prototypes.CombatFeel
{
    [Serializable]
    public sealed class ClericActor
    {
        private float healthValue;
        private float manaValue;

        public int Health => Mathf.FloorToInt(healthValue);
        public int MaxHealth { get; private set; }
        public int Mana => Mathf.FloorToInt(manaValue);
        public int MaxMana { get; private set; }
        public bool IsSitting { get; private set; }
        public bool IsDead => healthValue <= 0f;

        public float HealthPercent => MaxHealth <= 0 ? 0f : Mathf.Clamp01(healthValue / MaxHealth);
        public float ManaPercent => MaxMana <= 0 ? 0f : Mathf.Clamp01(manaValue / MaxMana);

        public void Reset(CadenceKnobs knobs)
        {
            MaxHealth = Mathf.Max(1, knobs.clericMaxHealth);
            MaxMana = Mathf.Max(1, knobs.clericMaxMana);
            healthValue = MaxHealth;
            manaValue = MaxMana;
            IsSitting = false;
        }

        public void SetSitting(bool sitting)
        {
            if (!IsDead)
            {
                IsSitting = sitting;
            }
        }

        public bool SpendMana(int amount)
        {
            if (amount < 0 || manaValue < amount)
            {
                return false;
            }

            manaValue -= amount;
            return true;
        }

        public int RestoreMana(float amount)
        {
            if (amount <= 0f || IsDead)
            {
                return 0;
            }

            var before = Mana;
            manaValue = Mathf.Min(MaxMana, manaValue + amount);
            return Mana - before;
        }

        public int Heal(int amount)
        {
            if (amount <= 0 || IsDead)
            {
                return 0;
            }

            var before = Health;
            healthValue = Mathf.Min(MaxHealth, healthValue + amount);
            return Health - before;
        }

        public int RestoreHealth(float amount)
        {
            if (amount <= 0f || IsDead)
            {
                return 0;
            }

            var before = Health;
            healthValue = Mathf.Min(MaxHealth, healthValue + amount);
            return Health - before;
        }

        public int ReceiveDamage(int amount)
        {
            if (amount <= 0 || IsDead)
            {
                return 0;
            }

            var before = Health;
            healthValue = Mathf.Max(0f, healthValue - amount);
            return before - Health;
        }
    }
}
