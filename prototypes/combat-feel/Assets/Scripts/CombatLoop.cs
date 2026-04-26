// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can Cleric tab-target combat, slow cast cadence, mana pressure, and med-break recovery make the silence between pulls feel intentional rather than empty?
// Date: 2026-04-26

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Gravenspire.Prototypes.CombatFeel
{
    public enum CombatPrototypeState
    {
        BetweenPulls,
        Pulling,
        Fighting,
        Casting,
        Complete,
        Stopped,
        Dead
    }

    public enum SpellKind
    {
        None,
        Smite,
        Heal
    }

    public sealed class CombatLoop
    {
        private readonly CadenceKnobs knobs;
        private readonly List<string> logLines = new();
        private HostileActor[] pulls = Array.Empty<HostileActor>();
        private int selectedPullIndex;
        private int activePullIndex = -1;
        private float stateTimer;
        private float clericSwingTimer;
        private float hostileSwingTimer;
        private float globalCooldownTimer;
        private float ambientTimer;
        private float castTimer;
        private float castDuration;
        private SpellKind castingSpell;
        private CombatPrototypeState stateBeforeCast;
        private bool lastMeditating;

        public CombatLoop(CadenceKnobs knobs)
        {
            this.knobs = knobs;
            Cleric = new ClericActor();
        }

        public ClericActor Cleric { get; }
        public HostileActor CurrentTarget { get; private set; }
        public CombatPrototypeState State { get; private set; }
        public int PullsCompleted { get; private set; }
        public int PullsGoal => pulls.Length == 0
            ? Mathf.Max(3, knobs.pullsToComplete)
            : Mathf.Clamp(knobs.pullsToComplete, 3, pulls.Length);
        public IReadOnlyList<string> LogLines => logLines;
        public SpellKind CastingSpell => castingSpell;
        public float CastProgress => castDuration <= 0f ? 0f : Mathf.Clamp01(castTimer / castDuration);
        public float TotalCombatSeconds { get; private set; }
        public float TotalDowntimeSeconds { get; private set; }
        public int MedBreaks { get; private set; }
        public int SmiteCasts { get; private set; }
        public int HealCasts { get; private set; }
        public int AutoAttackSwings { get; private set; }
        public int HostileSwings { get; private set; }
        public int UnsafePullAttempts { get; private set; }
        public int Deaths { get; private set; }

        public HostileActor PreviewTarget
        {
            get
            {
                if (pulls.Length == 0 || selectedPullIndex >= pulls.Length)
                {
                    return null;
                }

                return pulls[selectedPullIndex];
            }
        }

        public void Reset()
        {
            pulls = HostileActor.CreateDefaultPulls();
            selectedPullIndex = 0;
            activePullIndex = -1;
            CurrentTarget = null;
            State = CombatPrototypeState.BetweenPulls;
            stateTimer = 0f;
            clericSwingTimer = knobs.clericAutoAttackSeconds;
            hostileSwingTimer = knobs.hostilePressureGraceSeconds;
            globalCooldownTimer = 0f;
            ambientTimer = 1.5f;
            castTimer = 0f;
            castDuration = 0f;
            castingSpell = SpellKind.None;
            stateBeforeCast = CombatPrototypeState.BetweenPulls;
            lastMeditating = false;
            PullsCompleted = 0;
            TotalCombatSeconds = 0f;
            TotalDowntimeSeconds = 0f;
            MedBreaks = 0;
            SmiteCasts = 0;
            HealCasts = 0;
            AutoAttackSwings = 0;
            HostileSwings = 0;
            UnsafePullAttempts = 0;
            Deaths = 0;
            Cleric.Reset(knobs);
            logLines.Clear();
            Log("The chapel hall settles. Tab the next target, pull when ready.");
            Log("Success question: does this quiet feel like preparation?");
        }

        public void Tick(float deltaTime)
        {
            if (State == CombatPrototypeState.Complete || State == CombatPrototypeState.Stopped || State == CombatPrototypeState.Dead)
            {
                return;
            }

            deltaTime = Mathf.Max(0f, deltaTime);
            stateTimer += deltaTime;
            globalCooldownTimer = Mathf.Max(0f, globalCooldownTimer - deltaTime);

            if (State == CombatPrototypeState.BetweenPulls)
            {
                TickBetweenPulls(deltaTime);
                return;
            }

            TotalCombatSeconds += deltaTime;

            if (State == CombatPrototypeState.Pulling)
            {
                if (stateTimer >= knobs.pullWindupSeconds)
                {
                    State = CombatPrototypeState.Fighting;
                    stateTimer = 0f;
                    clericSwingTimer = Mathf.Min(clericSwingTimer, 0.4f);
                    hostileSwingTimer = knobs.hostilePressureGraceSeconds;
                    Log($"{CurrentTarget.Name} reaches melee range.");
                }

                return;
            }

            TickStandingRegen(deltaTime);

            if (State == CombatPrototypeState.Casting)
            {
                TickCast(deltaTime);
            }

            if (State == CombatPrototypeState.Fighting || State == CombatPrototypeState.Casting)
            {
                TickAutoAttacks(deltaTime);
                TickHostileAttacks(deltaTime);
                CheckCombatEnd();
            }
        }

        public void CycleTarget()
        {
            if (State != CombatPrototypeState.BetweenPulls || pulls.Length == 0)
            {
                return;
            }

            var maxIndex = Mathf.Min(PullsGoal, pulls.Length) - 1;
            selectedPullIndex++;
            if (selectedPullIndex > maxIndex)
            {
                selectedPullIndex = PullsCompleted;
            }

            var preview = PreviewTarget;
            if (preview != null)
            {
                Log($"You listen toward {preview.Name}. {preview.HauntCue}");
            }
        }

        public bool PullSelected()
        {
            if (State != CombatPrototypeState.BetweenPulls)
            {
                return false;
            }

            if (PullsCompleted >= PullsGoal)
            {
                Complete();
                return false;
            }

            if (!CanSafelyPull(out var reason))
            {
                UnsafePullAttempts++;
                Log(reason);
                return false;
            }

            Cleric.SetSitting(false);
            activePullIndex = Mathf.Clamp(selectedPullIndex, PullsCompleted, Mathf.Min(PullsGoal, pulls.Length) - 1);
            CurrentTarget = pulls[activePullIndex];
            State = CombatPrototypeState.Pulling;
            stateTimer = 0f;
            ambientTimer = knobs.betweenPullAmbientSeconds;
            Log($"Pull {PullsCompleted + 1}/{PullsGoal}: {CurrentTarget.Name}. {CurrentTarget.HauntCue}");
            return true;
        }

        public bool ToggleMeditation()
        {
            if (State != CombatPrototypeState.BetweenPulls)
            {
                Log("You cannot settle into meditation while engaged.");
                return false;
            }

            Cleric.SetSitting(!Cleric.IsSitting);
            Log(Cleric.IsSitting ? "You sit. The med break begins." : "You stand and ready your holy symbol.");
            return true;
        }

        public bool CastSmite()
        {
            if (CurrentTarget == null || CurrentTarget.IsDead)
            {
                Log("No hostile target is in range.");
                return false;
            }

            if (State != CombatPrototypeState.Fighting)
            {
                Log("Smite needs an active hostile target in melee range.");
                return false;
            }

            if (globalCooldownTimer > 0f)
            {
                Log("You are still recovering from the last action.");
                return false;
            }

            if (!Cleric.SpendMana(knobs.smiteManaCost))
            {
                Log($"Not enough mana for Smite. Need {knobs.smiteManaCost}, have {Cleric.Mana}.");
                return false;
            }

            BeginCast(SpellKind.Smite, knobs.smiteCastSeconds);
            Log($"You begin Smite ({knobs.smiteCastSeconds:0.0}s). Mana {Cleric.Mana}/{Cleric.MaxMana}.");
            return true;
        }

        public bool CastHeal()
        {
            if (State == CombatPrototypeState.Casting)
            {
                Log($"Already casting {castingSpell}. Wait for the cast bar.");
                return false;
            }

            if (State != CombatPrototypeState.Fighting && State != CombatPrototypeState.BetweenPulls)
            {
                Log("Heal can be cast during combat or between pulls.");
                return false;
            }

            if (Cleric.Health >= Cleric.MaxHealth)
            {
                Log("You are already at full health.");
                return false;
            }

            if (globalCooldownTimer > 0f)
            {
                Log("You are still recovering from the last action.");
                return false;
            }

            if (!Cleric.SpendMana(knobs.healManaCost))
            {
                Log($"Not enough mana for Heal. Need {knobs.healManaCost}, have {Cleric.Mana}.");
                return false;
            }

            BeginCast(SpellKind.Heal, knobs.healCastSeconds);
            Log($"You begin Heal ({knobs.healCastSeconds:0.0}s). Mana {Cleric.Mana}/{Cleric.MaxMana}.");
            return true;
        }

        public void Stop()
        {
            if (State == CombatPrototypeState.Stopped)
            {
                return;
            }

            State = CombatPrototypeState.Stopped;
            Log("Prototype stopped by player.");
            Log(BuildMetricsSummary());
        }

        public string BuildMetricsSummary()
        {
            var pullAverage = PullsCompleted <= 0 ? 0f : TotalCombatSeconds / PullsCompleted;
            return $"Metrics: pulls={PullsCompleted}/{PullsGoal}, combat={TotalCombatSeconds:0.0}s, downtime={TotalDowntimeSeconds:0.0}s, avgPull={pullAverage:0.0}s, medBreaks={MedBreaks}, smites={SmiteCasts}, heals={HealCasts}, unsafePulls={UnsafePullAttempts}.";
        }

        public string BuildLogText(int maxLines = 12)
        {
            var builder = new StringBuilder();
            var start = Mathf.Max(0, logLines.Count - maxLines);
            for (var i = start; i < logLines.Count; i++)
            {
                builder.AppendLine(logLines[i]);
            }

            return builder.ToString();
        }

        private void TickBetweenPulls(float deltaTime)
        {
            TotalDowntimeSeconds += deltaTime;
            var manaRate = Cleric.IsSitting ? knobs.medManaRegenPerSecond : knobs.betweenPullManaRegenPerSecond;
            var healthRate = Cleric.IsSitting ? knobs.medHealthRegenPerSecond : 0f;
            Cleric.RestoreMana(manaRate * deltaTime);
            Cleric.RestoreHealth(healthRate * deltaTime);

            if (Cleric.IsSitting && !lastMeditating)
            {
                MedBreaks++;
                lastMeditating = true;
            }
            else if (!Cleric.IsSitting)
            {
                lastMeditating = false;
            }

            ambientTimer -= deltaTime;
            if (ambientTimer <= 0f)
            {
                ambientTimer = knobs.betweenPullAmbientSeconds;
                var preview = PreviewTarget;
                if (preview != null)
                {
                    Log($"Quiet beat: {preview.HauntCue} Mana {Cleric.Mana}/{Cleric.MaxMana}.");
                }
            }
        }

        private void TickStandingRegen(float deltaTime)
        {
            Cleric.SetSitting(false);
            Cleric.RestoreMana(knobs.standingManaRegenPerSecond * deltaTime);
        }

        private void TickCast(float deltaTime)
        {
            castTimer += deltaTime;
            if (castTimer < castDuration)
            {
                return;
            }

            FinishCast();
        }

        private void TickAutoAttacks(float deltaTime)
        {
            if (CurrentTarget == null || CurrentTarget.IsDead)
            {
                return;
            }

            clericSwingTimer -= deltaTime;
            if (clericSwingTimer > 0f)
            {
                return;
            }

            clericSwingTimer = knobs.clericAutoAttackSeconds;
            var damage = UnityEngine.Random.Range(knobs.clericAutoAttackMinDamage, knobs.clericAutoAttackMaxDamage + 1);
            CurrentTarget.ReceiveDamage(damage);
            AutoAttackSwings++;
            Log($"Auto-attack hits {CurrentTarget.Name} for {damage}.");
        }

        private void TickHostileAttacks(float deltaTime)
        {
            if (CurrentTarget == null || CurrentTarget.IsDead)
            {
                return;
            }

            hostileSwingTimer -= deltaTime;
            if (hostileSwingTimer > 0f)
            {
                return;
            }

            hostileSwingTimer = knobs.hostileAttackSeconds;
            var damage = CurrentTarget.RollDamage(knobs);
            Cleric.ReceiveDamage(damage);
            HostileSwings++;
            Log($"{CurrentTarget.Name} hits you for {damage}.");

            if (Cleric.IsDead)
            {
                Deaths++;
                State = CombatPrototypeState.Dead;
                Log("You die before the rhythm stabilizes.");
                Log(BuildMetricsSummary());
            }
        }

        private void BeginCast(SpellKind spell, float duration)
        {
            stateBeforeCast = State;
            castingSpell = spell;
            castDuration = Mathf.Max(0.01f, duration);
            castTimer = 0f;
            State = CombatPrototypeState.Casting;
            globalCooldownTimer = knobs.globalCooldownSeconds;
        }

        private void FinishCast()
        {
            switch (castingSpell)
            {
                case SpellKind.Smite:
                {
                    var damage = UnityEngine.Random.Range(knobs.smiteMinDamage, knobs.smiteMaxDamage + 1);
                    CurrentTarget?.ReceiveDamage(damage);
                    SmiteCasts++;
                    Log($"Smite lands for {damage}.");
                    break;
                }
                case SpellKind.Heal:
                {
                    var amount = UnityEngine.Random.Range(knobs.healMinAmount, knobs.healMaxAmount + 1);
                    var restored = Cleric.Heal(amount);
                    HealCasts++;
                    Log($"Heal restores {restored} health.");
                    break;
                }
            }

            castingSpell = SpellKind.None;
            castTimer = 0f;
            castDuration = 0f;
            if (State != CombatPrototypeState.Dead)
            {
                State = stateBeforeCast == CombatPrototypeState.BetweenPulls
                    ? CombatPrototypeState.BetweenPulls
                    : CombatPrototypeState.Fighting;
            }
        }

        private void CheckCombatEnd()
        {
            if (CurrentTarget == null || !CurrentTarget.IsDead)
            {
                return;
            }

            PullsCompleted++;
            Log($"{CurrentTarget.Name} falls. Mana ended at {Cleric.Mana}/{Cleric.MaxMana}.");
            CurrentTarget = null;
            activePullIndex = -1;
            selectedPullIndex = Mathf.Clamp(PullsCompleted, 0, Mathf.Min(PullsGoal, pulls.Length) - 1);
            castingSpell = SpellKind.None;
            castTimer = 0f;
            castDuration = 0f;

            if (PullsCompleted >= PullsGoal)
            {
                Complete();
                return;
            }

            State = CombatPrototypeState.BetweenPulls;
            stateTimer = 0f;
            ambientTimer = 1.0f;
            Log("Between pulls. Pull now, or sit until the next choice feels earned.");
        }

        private bool CanSafelyPull(out string reason)
        {
            if (!knobs.blockUnsafePulls)
            {
                reason = string.Empty;
                return true;
            }

            if (Cleric.Mana < knobs.RecommendedPullMana)
            {
                reason = $"Too little mana to make this pull meaningful. Need {knobs.RecommendedPullMana}, have {Cleric.Mana}. Sit and med.";
                return false;
            }

            if (Cleric.Health < knobs.RecommendedPullHealth)
            {
                reason = $"Too wounded to pull cleanly. Need {knobs.RecommendedPullHealth} health, have {Cleric.Health}. Sit and recover.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private void Complete()
        {
            State = CombatPrototypeState.Complete;
            Log("Five-pull rhythm complete. Stop and write the feel notes now.");
            Log(BuildMetricsSummary());
        }

        private void Log(string message)
        {
            logLines.Add($"[{Time.time:0.0}] {message}");
            if (logLines.Count > 80)
            {
                logLines.RemoveAt(0);
            }
        }
    }
}
