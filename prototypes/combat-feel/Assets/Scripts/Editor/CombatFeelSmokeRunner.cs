// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can Cleric tab-target combat, slow cast cadence, mana pressure, and med-break recovery make the silence between pulls feel intentional rather than empty?
// Date: 2026-04-26

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Gravenspire.Prototypes.CombatFeel.Editor
{
    public static class CombatFeelSmokeRunner
    {
        [MenuItem("Gravenspire/Prototypes/Combat Feel/Run Scripted Smoke")]
        public static void RunScriptedSmoke()
        {
            var knobs = new CadenceKnobs();
            var loop = new CombatLoop(knobs);
            loop.Reset();

            var elapsed = 0f;
            var safetyTicks = 0;
            while (loop.State != CombatPrototypeState.Complete &&
                   loop.State != CombatPrototypeState.Dead &&
                   loop.State != CombatPrototypeState.Stopped &&
                   elapsed < 240f &&
                   safetyTicks < 2400)
            {
                DriveOneDecision(loop, knobs);
                loop.Tick(0.1f);
                elapsed += 0.1f;
                safetyTicks++;
            }

            if (loop.State != CombatPrototypeState.Complete)
            {
                throw new InvalidOperationException($"Smoke did not complete: state={loop.State}, {loop.BuildMetricsSummary()}");
            }

            if (loop.PullsCompleted < 3)
            {
                throw new InvalidOperationException($"Smoke completed too few pulls: {loop.PullsCompleted}.");
            }

            if (loop.MedBreaks < 1)
            {
                throw new InvalidOperationException("Smoke did not force a med break under default knobs.");
            }

            var averagePullSeconds = loop.TotalCombatSeconds / Mathf.Max(1, loop.PullsCompleted);
            if (averagePullSeconds < 15f || averagePullSeconds > 45f)
            {
                Debug.LogWarning($"Average scripted pull length outside target band: {averagePullSeconds:0.0}s. {loop.BuildMetricsSummary()}");
            }

            Debug.Log($"Combat feel scripted smoke passed in {elapsed:0.0}s simulated time. {loop.BuildMetricsSummary()}");
        }

        private static void DriveOneDecision(CombatLoop loop, CadenceKnobs knobs)
        {
            if (loop.State == CombatPrototypeState.BetweenPulls)
            {
                var targetMana = loop.PullsCompleted >= 2
                    ? Mathf.CeilToInt(loop.Cleric.MaxMana * 0.74f)
                    : knobs.RecommendedPullMana;
                var targetHealth = loop.PullsCompleted >= 2
                    ? Mathf.CeilToInt(loop.Cleric.MaxHealth * 0.78f)
                    : knobs.RecommendedPullHealth;
                var shouldMed = loop.Cleric.Mana < targetMana ||
                                loop.Cleric.Health < targetHealth;
                if (shouldMed)
                {
                    if (!loop.Cleric.IsSitting)
                    {
                        loop.ToggleMeditation();
                    }

                    return;
                }

                if (loop.Cleric.IsSitting)
                {
                    loop.ToggleMeditation();
                }

                loop.PullSelected();
                return;
            }

            if (loop.State != CombatPrototypeState.Fighting)
            {
                return;
            }

            if (!loop.AutoAttackEnabled)
            {
                loop.ToggleAutoAttack();
                return;
            }

            if (loop.Cleric.HealthPercent < 0.68f && loop.Cleric.Mana >= knobs.healManaCost)
            {
                loop.CastHeal();
                return;
            }

            if (loop.Cleric.HealthPercent < 0.78f && loop.CanUseDefensivePrayer)
            {
                loop.UseDefensivePrayer();
                return;
            }

            if (loop.CurrentTarget != null &&
                loop.CurrentTarget.Health > knobs.authorityMaxDamage &&
                loop.CanUseAuthority)
            {
                loop.UseSmiteOfAuthority();
                return;
            }

            if (loop.CurrentTarget != null &&
                loop.CurrentTarget.Health > knobs.bashMaxDamage &&
                loop.CanUseBash)
            {
                loop.UseBash();
                return;
            }

            if (loop.CurrentTarget != null &&
                loop.CurrentTarget.Health > knobs.smiteMaxDamage &&
                loop.Cleric.Mana >= knobs.smiteManaCost)
            {
                loop.CastSmite();
            }
        }
    }
}
#endif
