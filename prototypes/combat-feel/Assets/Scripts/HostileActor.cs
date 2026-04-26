// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can Cleric tab-target combat, slow cast cadence, mana pressure, and med-break recovery make the silence between pulls feel intentional rather than empty?
// Date: 2026-04-26

using System;
using UnityEngine;

namespace Gravenspire.Prototypes.CombatFeel
{
    [Serializable]
    public sealed class HostileActor
    {
        public string Name { get; }
        public string HauntCue { get; }
        public int MaxHealth { get; }
        public int Health { get; private set; }
        public int BaseDamage { get; }
        public bool IsDead => Health <= 0;
        public float HealthPercent => MaxHealth <= 0 ? 0f : Mathf.Clamp01((float)Health / MaxHealth);

        public HostileActor(string name, string hauntCue, int maxHealth, int baseDamage)
        {
            Name = name;
            HauntCue = hauntCue;
            MaxHealth = Mathf.Max(1, maxHealth);
            Health = MaxHealth;
            BaseDamage = Mathf.Max(0, baseDamage);
        }

        public int ReceiveDamage(int amount)
        {
            if (amount <= 0 || IsDead)
            {
                return 0;
            }

            var before = Health;
            Health = Mathf.Max(0, Health - amount);
            return before - Health;
        }

        public int RollDamage(CadenceKnobs knobs)
        {
            var variance = Mathf.Max(0, knobs.hostileDamageVariance);
            return BaseDamage + UnityEngine.Random.Range(-variance, variance + 1);
        }

        public static HostileActor[] CreateDefaultPulls()
        {
            return new[]
            {
                new HostileActor("Bone Footpad", "A bare heel scrapes behind the chapel rail.", 78, 6),
                new HostileActor("Lantern Thrall", "A dead lantern swings without wind.", 98, 7),
                new HostileActor("Choir Wight", "A cracked hymn gathers in the west hall.", 112, 8),
                new HostileActor("Hollow Acolyte", "Wax runs upward along a black candle.", 126, 9),
                new HostileActor("Sepulcher Deacon", "Keys knock softly against a sealed door.", 140, 10)
            };
        }
    }
}
