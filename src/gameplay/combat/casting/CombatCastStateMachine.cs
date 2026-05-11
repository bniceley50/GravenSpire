#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gravenspire.Gameplay.Combat;

public enum CombatCastTransitionOutcome
{
    Accepted,
    Rejected,
    NotDue,
    Completed,
    Cancelled,
    Interrupted,
    NoInterruptRoll,
    RecoveryStarted,
    RecoveryEnded
}

public interface ICombatCastInterruptRandomSource
{
    double NextInterruptRoll();
}

public sealed record CombatCastProfile(
    string SpellId,
    double CastTimeSeconds,
    int ManaCost,
    double SpellRangeMeters,
    double RecoverySeconds,
    double InterruptResistance = 0d,
    bool RequiresTarget = true,
    bool RequiresLineOfSight = true);

public sealed record CombatCastStartRequest(
    string CastId,
    CombatActorState Caster,
    CombatActorState? Target,
    CombatZoneGate ZoneGate,
    double DistanceMetersToTarget,
    IReadOnlyList<CombatLosLayer> LosBlockingLayers,
    CombatTick Tick,
    int TickRateHz,
    CombatCastProfile Profile);

public sealed record CombatDamageInterruptRequest(
    int DamageTaken,
    bool DamageWasBlockedOrAbsorbed,
    CombatTick Tick,
    CombatInterruptFormulaTuning InterruptTuning,
    ICombatCastInterruptRandomSource RandomSource,
    string InterruptSource = "damage");

public sealed record CombatCastProgressSnapshot(
    string CastId,
    string SpellId,
    string CasterCombatActorId,
    string? TargetCombatActorId,
    CombatTick Tick,
    double ProgressSeconds,
    double CastTimeSeconds,
    double NormalizedProgress,
    bool IsComplete);

public sealed record CombatActiveCastSnapshot(
    string CastId,
    string SpellId,
    string CasterCombatActorId,
    string? TargetCombatActorId,
    long StartedTick,
    long CompletionTick,
    long? RecoveryEndTick);

public sealed record CombatCastTransitionResult(
    CombatActorState Caster,
    CombatCastTransitionOutcome Outcome,
    bool Changed,
    IReadOnlyList<string> RejectionReasons,
    IReadOnlyList<ICombatCastLifecycleEvent> Events,
    CombatCastProgressSnapshot? Progress,
    double? InterruptChance,
    double? InterruptRoll)
{
    public bool Succeeded => RejectionReasons.Count == 0;
}

/// <summary>
/// Fixed-tick slow-cast state machine for Combat-owned cast execution.
/// </summary>
public sealed class CombatCastStateMachine
{
    private readonly List<ICombatCastLifecycleEvent> lifecycleEvents = new();
    private ActiveCast? activeCast;

    public IReadOnlyList<ICombatCastLifecycleEvent> LifecycleEvents =>
        new ReadOnlyCollection<ICombatCastLifecycleEvent>(lifecycleEvents);

    public CombatActiveCastSnapshot? CurrentCast => activeCast?.ToSnapshot();

    public CombatCastTransitionResult StartCast(CombatCastStartRequest request)
    {
        CombatArgumentNull.ThrowIfNull(request);
        var errors = ValidateStartRequest(request);
        if (errors.Count > 0)
        {
            return Result(request.Caster, CombatCastTransitionOutcome.Rejected, Changed: false, errors, Array.Empty<ICombatCastLifecycleEvent>(), null, null, null);
        }

        var completionTick = checked(request.Tick.Index + CombatCastFormulas.SecondsToTicksCeiling(request.Profile.CastTimeSeconds, request.TickRateHz));
        activeCast = new ActiveCast(
            request.CastId,
            request.Profile,
            request.Caster.CombatActorId,
            request.Target?.CombatActorId,
            request.Tick.Index,
            completionTick,
            request.TickRateHz,
            RecoveryEndTick: null);

        var caster = request.Caster.BeginCast(request.CastId, request.Profile.SpellId, request.Target?.CombatActorId);
        var started = new CastStartedEvent(request.CastId, request.Profile.SpellId, request.Caster.CombatActorId, request.Target?.CombatActorId, request.Tick);
        Add(started);

        return Result(caster, CombatCastTransitionOutcome.Accepted, Changed: true, Array.Empty<string>(), new ICombatCastLifecycleEvent[] { started }, ProgressAt(request.Tick), null, null);
    }

    public CombatCastProgressSnapshot? GetProgress(CombatTick tick)
    {
        return ProgressAt(tick);
    }

    public CombatCastTransitionResult ResolveCompletion(CombatActorState caster, CombatActorState? target, CombatTick tick)
    {
        var active = activeCast;
        if (active is null)
        {
            return Rejected(caster, "No active cast is available.");
        }

        if (tick.Index < active.CompletionTick)
        {
            return Result(caster.WithCastProgress(ProgressAt(tick)!.ProgressSeconds), CombatCastTransitionOutcome.NotDue, Changed: false, Array.Empty<string>(), Array.Empty<ICombatCastLifecycleEvent>(), ProgressAt(tick), null, null);
        }

        var targetError = ValidateCompletionTarget(active, caster, target);
        if (targetError is not null)
        {
            return Result(caster, CombatCastTransitionOutcome.Rejected, Changed: false, new[] { targetError }, Array.Empty<ICombatCastLifecycleEvent>(), ProgressAt(tick), null, null);
        }

        var manaAfterCast = checked(caster.CurrentMana - active.Profile.ManaCost);
        var completedCaster = caster
            .WithCastProgress(active.Profile.CastTimeSeconds)
            .WithCurrentMana(manaAfterCast)
            .BeginCastRecovery(active.Profile.RecoverySeconds);

        activeCast = active with { RecoveryEndTick = RecoveryEndTick(tick, active.Profile.RecoverySeconds, active.TickRateHz) };

        var completed = new CastCompletedEvent(active.CastId, active.Profile.SpellId, active.CasterCombatActorId, active.TargetCombatActorId, tick, active.Profile.ManaCost);
        var recoveryStarted = new CastRecoveryStartedEvent(active.CastId, active.Profile.SpellId, active.CasterCombatActorId, active.TargetCombatActorId, tick, active.Profile.RecoverySeconds);
        Add(completed);
        Add(recoveryStarted);

        return Result(
            completedCaster,
            CombatCastTransitionOutcome.Completed,
            Changed: true,
            Array.Empty<string>(),
            new ICombatCastLifecycleEvent[] { completed, recoveryStarted },
            ProgressAt(tick),
            null,
            null);
    }

    public CombatCastTransitionResult CancelCast(CombatActorState caster, CombatTick tick, string cancelSource = "manual_cancel")
    {
        var active = activeCast;
        if (active is null)
        {
            return Rejected(caster, "No active cast is available.");
        }

        if (tick.Index >= active.CompletionTick)
        {
            return Result(caster, CombatCastTransitionOutcome.Rejected, Changed: false, new[] { "Cast has already reached completion priority." }, Array.Empty<ICombatCastLifecycleEvent>(), ProgressAt(tick), null, null);
        }

        var progress = ProgressAt(tick);
        var cancelledCaster = caster
            .WithCastProgress(progress!.ProgressSeconds)
            .BeginCastRecovery(active.Profile.RecoverySeconds);

        activeCast = active with { RecoveryEndTick = RecoveryEndTick(tick, active.Profile.RecoverySeconds, active.TickRateHz) };

        var cancelled = new CastCancelledEvent(active.CastId, active.Profile.SpellId, active.CasterCombatActorId, active.TargetCombatActorId, tick, cancelSource);
        var recoveryStarted = new CastRecoveryStartedEvent(active.CastId, active.Profile.SpellId, active.CasterCombatActorId, active.TargetCombatActorId, tick, active.Profile.RecoverySeconds);
        Add(cancelled);
        Add(recoveryStarted);

        return Result(
            cancelledCaster,
            CombatCastTransitionOutcome.Cancelled,
            Changed: true,
            Array.Empty<string>(),
            new ICombatCastLifecycleEvent[] { cancelled, recoveryStarted },
            progress,
            null,
            null);
    }

    public CombatCastTransitionResult InterruptFromDamage(
        CombatActorState caster,
        CombatActorState? target,
        CombatDamageInterruptRequest request)
    {
        CombatArgumentNull.ThrowIfNull(request);
        CombatArgumentNull.ThrowIfNull(request.InterruptTuning);
        CombatArgumentNull.ThrowIfNull(request.RandomSource);

        var active = activeCast;
        if (active is null)
        {
            return Rejected(caster, "No active cast is available.");
        }

        if (request.Tick.Index >= active.CompletionTick)
        {
            return ResolveCompletion(caster, target, request.Tick);
        }

        var progress = ProgressAt(request.Tick);
        var progressedCaster = caster.WithCastProgress(progress!.ProgressSeconds);
        if (request.DamageWasBlockedOrAbsorbed || request.DamageTaken <= 0)
        {
            return Result(progressedCaster, CombatCastTransitionOutcome.NoInterruptRoll, Changed: true, Array.Empty<string>(), Array.Empty<ICombatCastLifecycleEvent>(), progress, null, null);
        }

        var remainingSeconds = RemainingCastSeconds(active, request.Tick);
        var interruptChance = CombatCastFormulas.CalculateInterruptChance(
            request.DamageTaken,
            caster.MaxHealth,
            remainingSeconds,
            active.Profile.CastTimeSeconds,
            active.Profile.InterruptResistance,
            request.InterruptTuning);
        var interruptRoll = request.RandomSource.NextInterruptRoll();
        if (interruptRoll < 0 || interruptRoll > 1)
        {
            throw new InvalidOperationException("Injected interrupt roll must be a ratio.");
        }

        if (interruptRoll >= interruptChance)
        {
            return Result(progressedCaster, CombatCastTransitionOutcome.NoInterruptRoll, Changed: true, Array.Empty<string>(), Array.Empty<ICombatCastLifecycleEvent>(), progress, interruptChance, interruptRoll);
        }

        var interruptedCaster = progressedCaster.MarkCastInterrupted(active.Profile.RecoverySeconds);
        var interrupted = new CastInterruptedEvent(
            active.CastId,
            active.Profile.SpellId,
            active.CasterCombatActorId,
            active.TargetCombatActorId,
            request.Tick,
            request.InterruptSource,
            interruptChance,
            interruptRoll);
        Add(interrupted);

        return Result(
            interruptedCaster,
            CombatCastTransitionOutcome.Interrupted,
            Changed: true,
            Array.Empty<string>(),
            new ICombatCastLifecycleEvent[] { interrupted },
            progress,
            interruptChance,
            interruptRoll);
    }

    public CombatCastTransitionResult BeginRecoveryAfterInterrupt(CombatActorState caster, CombatTick tick)
    {
        var active = activeCast;
        if (active is null)
        {
            return Rejected(caster, "No active cast is available.");
        }

        if (caster.CastRuntimeState != CombatCastRuntimeState.Interrupted)
        {
            return Rejected(caster, "Caster is not in Interrupted cast state.");
        }

        activeCast = active with { RecoveryEndTick = RecoveryEndTick(tick, active.Profile.RecoverySeconds, active.TickRateHz) };
        var recoveryCaster = caster.BeginCastRecovery(active.Profile.RecoverySeconds);
        var recoveryStarted = new CastRecoveryStartedEvent(active.CastId, active.Profile.SpellId, active.CasterCombatActorId, active.TargetCombatActorId, tick, active.Profile.RecoverySeconds);
        Add(recoveryStarted);

        return Result(
            recoveryCaster,
            CombatCastTransitionOutcome.RecoveryStarted,
            Changed: true,
            Array.Empty<string>(),
            new ICombatCastLifecycleEvent[] { recoveryStarted },
            ProgressAt(tick),
            null,
            null);
    }

    public CombatCastTransitionResult EndRecovery(CombatActorState caster, CombatTick tick)
    {
        var active = activeCast;
        if (active is null || active.RecoveryEndTick is null)
        {
            return Rejected(caster, "No active cast recovery is available.");
        }

        if (tick.Index < active.RecoveryEndTick.Value)
        {
            var remainingTicks = active.RecoveryEndTick.Value - tick.Index;
            var remainingSeconds = remainingTicks / (double)active.TickRateHz;
            return Result(caster.BeginCastRecovery(remainingSeconds), CombatCastTransitionOutcome.NotDue, Changed: true, Array.Empty<string>(), Array.Empty<ICombatCastLifecycleEvent>(), ProgressAt(tick), null, null);
        }

        var nextState = caster.ThreatTable.Count > 0 ? CombatState.InCombat : CombatState.OutOfCombat;
        var endedCaster = caster.ClearCastRuntime(nextState);
        var recoveryEnded = new CastRecoveryEndedEvent(active.CastId, active.Profile.SpellId, active.CasterCombatActorId, active.TargetCombatActorId, tick);
        Add(recoveryEnded);
        activeCast = null;

        return Result(
            endedCaster,
            CombatCastTransitionOutcome.RecoveryEnded,
            Changed: true,
            Array.Empty<string>(),
            new ICombatCastLifecycleEvent[] { recoveryEnded },
            Progress: null,
            InterruptChance: null,
            InterruptRoll: null);
    }

    private static IReadOnlyList<string> ValidateStartRequest(CombatCastStartRequest request)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.CastId))
        {
            errors.Add("cast_id is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Profile.SpellId))
        {
            errors.Add("spell_id is required.");
        }

        if (request.TickRateHz <= 0)
        {
            errors.Add("tick_rate_hz must be positive.");
        }

        if (request.Profile.CastTimeSeconds <= 0)
        {
            errors.Add("Slow cast framework requires cast_time_seconds greater than zero.");
        }

        if (request.Profile.ManaCost < 0 || request.Profile.RecoverySeconds < 0 || request.Profile.SpellRangeMeters < 0)
        {
            errors.Add("mana cost, recovery seconds, and spell range must not be negative.");
        }

        if (!request.Caster.IsAlive)
        {
            errors.Add("Caster must be alive.");
        }

        if (request.Caster.CastRuntimeState != CombatCastRuntimeState.None ||
            request.Caster.CombatState is CombatState.Casting or CombatState.Interrupted or CombatState.Recovery)
        {
            errors.Add("Caster is already casting or recovering.");
        }

        if (request.Caster.CurrentMana < request.Profile.ManaCost)
        {
            errors.Add("Caster does not have enough mana.");
        }

        if (!request.ZoneGate.CanRunHostileCombat(request.Caster.ZoneId))
        {
            errors.Add("Active zone does not allow hostile casts.");
        }

        if (request.Profile.RequiresTarget)
        {
            ValidateTarget(request, errors);
        }

        if (request.Profile.RequiresLineOfSight &&
            !T1CombatLineOfSight.HasLineOfSight(request.LosBlockingLayers))
        {
            errors.Add("Target is not line-of-sight valid for casting.");
        }

        return errors;
    }

    private static void ValidateTarget(CombatCastStartRequest request, ICollection<string> errors)
    {
        var target = request.Target;
        if (target is null)
        {
            errors.Add("Cast requires a valid target.");
            return;
        }

        if (!target.IsAlive)
        {
            errors.Add("Cast target must be alive.");
        }

        if (!string.Equals(request.Caster.ZoneId, target.ZoneId, StringComparison.Ordinal))
        {
            errors.Add("Cast target must be in the caster's zone.");
        }

        if (!string.Equals(request.Caster.TargetCombatActorId, target.CombatActorId, StringComparison.Ordinal))
        {
            errors.Add("Cast target must match the caster's selected target.");
        }

        if (!request.ZoneGate.CanRunHostileCombat(target.ZoneId))
        {
            errors.Add("Active zone does not allow target casting.");
        }

        if (request.DistanceMetersToTarget > request.Profile.SpellRangeMeters)
        {
            errors.Add("Cast target is outside spell_range_meters.");
        }
    }

    private static string? ValidateCompletionTarget(ActiveCast active, CombatActorState caster, CombatActorState? target)
    {
        if (!caster.IsAlive)
        {
            return "Caster must be alive at completion.";
        }

        if (caster.CurrentMana < active.Profile.ManaCost)
        {
            return "Caster does not have enough mana at completion.";
        }

        if (active.Profile.RequiresTarget)
        {
            if (target is null)
            {
                return "Cast completion requires a valid target.";
            }

            if (!target.IsAlive)
            {
                return "Cast target must be alive at completion.";
            }

            if (!string.Equals(active.TargetCombatActorId, target.CombatActorId, StringComparison.Ordinal))
            {
                return "Cast completion target must match the active cast target.";
            }
        }

        return null;
    }

    private CombatCastProgressSnapshot? ProgressAt(CombatTick tick)
    {
        var active = activeCast;
        if (active is null)
        {
            return null;
        }

        var elapsedTicks = Math.Max(0L, Math.Min(tick.Index - active.StartedTick, active.CompletionTick - active.StartedTick));
        var elapsedSeconds = elapsedTicks / (double)active.TickRateHz;
        var progressSeconds = Math.Min(active.Profile.CastTimeSeconds, elapsedSeconds);
        var normalized = active.Profile.CastTimeSeconds == 0 ? 1d : Math.Min(1d, progressSeconds / active.Profile.CastTimeSeconds);

        return new CombatCastProgressSnapshot(
            active.CastId,
            active.Profile.SpellId,
            active.CasterCombatActorId,
            active.TargetCombatActorId,
            tick,
            progressSeconds,
            active.Profile.CastTimeSeconds,
            normalized,
            tick.Index >= active.CompletionTick);
    }

    private static double RemainingCastSeconds(ActiveCast active, CombatTick tick)
    {
        var remainingTicks = Math.Max(0L, active.CompletionTick - tick.Index);
        return remainingTicks / (double)active.TickRateHz;
    }

    private static long RecoveryEndTick(CombatTick tick, double recoverySeconds, int tickRateHz)
    {
        if (recoverySeconds == 0)
        {
            return tick.Index;
        }

        return checked(tick.Index + CombatCastFormulas.SecondsToTicksCeiling(recoverySeconds, tickRateHz));
    }

    private void Add(ICombatCastLifecycleEvent lifecycleEvent)
    {
        lifecycleEvents.Add(lifecycleEvent);
    }

    private static CombatCastTransitionResult Rejected(CombatActorState caster, string reason)
    {
        return Result(caster, CombatCastTransitionOutcome.Rejected, Changed: false, new[] { reason }, Array.Empty<ICombatCastLifecycleEvent>(), null, null, null);
    }

    private static CombatCastTransitionResult Result(
        CombatActorState caster,
        CombatCastTransitionOutcome outcome,
        bool Changed,
        IReadOnlyList<string> RejectionReasons,
        IReadOnlyList<ICombatCastLifecycleEvent> Events,
        CombatCastProgressSnapshot? Progress,
        double? InterruptChance,
        double? InterruptRoll)
    {
        return new CombatCastTransitionResult(caster, outcome, Changed, RejectionReasons.ToArray(), Events.ToArray(), Progress, InterruptChance, InterruptRoll);
    }

    private sealed record ActiveCast(
        string CastId,
        CombatCastProfile Profile,
        string CasterCombatActorId,
        string? TargetCombatActorId,
        long StartedTick,
        long CompletionTick,
        int TickRateHz,
        long? RecoveryEndTick)
    {
        public CombatActiveCastSnapshot ToSnapshot()
        {
            return new CombatActiveCastSnapshot(CastId, Profile.SpellId, CasterCombatActorId, TargetCombatActorId, StartedTick, CompletionTick, RecoveryEndTick);
        }
    }
}
