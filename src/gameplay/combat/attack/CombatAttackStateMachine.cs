#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gravenspire.Gameplay.Combat;

/// <summary>
/// Player-controlled Attack toggle state.
/// </summary>
public enum CombatAttackMode
{
    Off,
    On
}

/// <summary>
/// Approved paths that may change Attack state.
/// </summary>
public enum CombatAttackTransitionPath
{
    PlayerToggleOn,
    PlayerToggleOff,
    TargetDeath,
    SuccessfulSitOrMed,
    CombatExit,
    PlayerDeath,
    ZoneTransition
}

/// <summary>
/// Combat paths that must not enable Attack by side effect.
/// </summary>
public enum CombatAttackPassivePath
{
    TargetSelection,
    TabCycle,
    BodyPull,
    SocialAssist,
    SpellPull,
    SpellCast
}

/// <summary>
/// HUD-safe current Attack state.
/// </summary>
public sealed record CombatAttackStateSnapshot(
    CombatAttackMode Mode,
    string? TargetCombatActorId,
    long? NextSwingDueTick,
    CombatAttackTransitionPath? LastTransitionPath)
{
    public bool IsAttackOn => Mode == CombatAttackMode.On;
}

/// <summary>
/// HUD-facing signal emitted when Attack changes on or off.
/// </summary>
public sealed record CombatAttackStateChangedSignal(
    bool AttackOn,
    string? TargetCombatActorId,
    CombatAttackTransitionPath TransitionPath,
    CombatTick Tick);

/// <summary>
/// Toggle-on request context used to validate the explicit player command.
/// </summary>
public sealed record CombatAttackToggleOnRequest(
    CombatActorState Player,
    CombatActorState? Target,
    CombatZoneGate ZoneGate,
    double DistanceMetersToTarget,
    CombatTick Tick,
    int TickRateHz);

/// <summary>
/// Result of an Attack state-machine operation.
/// </summary>
public sealed record CombatAttackTransitionResult(
    CombatAttackStateSnapshot Snapshot,
    bool Changed,
    CombatAttackStateChangedSignal? StateChangedSignal,
    IReadOnlyList<string> RejectionReasons)
{
    public bool Succeeded => RejectionReasons.Count == 0;
}

/// <summary>
/// T1 Attack toggle state machine. It owns intent only; melee hit resolution belongs to a later story.
/// </summary>
public sealed class CombatAttackStateMachine
{
    private readonly List<CombatAttackStateChangedSignal> stateChangedSignals = new();

    public CombatAttackStateSnapshot CurrentState { get; private set; } =
        new(CombatAttackMode.Off, null, null, null);

    public IReadOnlyList<CombatAttackStateChangedSignal> StateChangedSignals =>
        new ReadOnlyCollection<CombatAttackStateChangedSignal>(stateChangedSignals);

    public CombatAttackTransitionResult ToggleOn(CombatAttackToggleOnRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Player);
        ArgumentNullException.ThrowIfNull(request.ZoneGate);

        if (request.TickRateHz <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "tick_rate_hz must be positive.");
        }

        var rejections = ValidateToggleOn(request);
        if (rejections.Count > 0)
        {
            return NoChange(rejections);
        }

        var target = request.Target!;
        var ticksUntilSwing = Math.Max(1L, (long)Math.Ceiling(request.Player.WeaponDelaySeconds * request.TickRateHz));
        var nextSwingDueTick = checked(request.Tick.Index + ticksUntilSwing);

        return Change(
            CombatAttackMode.On,
            target.CombatActorId,
            nextSwingDueTick,
            CombatAttackTransitionPath.PlayerToggleOn,
            request.Tick);
    }

    public CombatAttackTransitionResult ToggleOff(CombatAttackTransitionPath transitionPath, CombatTick tick)
    {
        if (!IsApprovedOffPath(transitionPath))
        {
            throw new ArgumentException("Attack off transition path is not approved.", nameof(transitionPath));
        }

        return CurrentState.IsAttackOn
            ? Change(CombatAttackMode.Off, null, null, transitionPath, tick)
            : NoChange(Array.Empty<string>());
    }

    public CombatAttackTransitionResult ForceOff(CombatAttackTransitionPath transitionPath, CombatTick tick)
    {
        if (transitionPath == CombatAttackTransitionPath.PlayerToggleOff)
        {
            throw new ArgumentException("Use ToggleOff for the explicit player off command.", nameof(transitionPath));
        }

        return ToggleOff(transitionPath, tick);
    }

    public CombatAttackTransitionResult ObservePassivePath(CombatAttackPassivePath passivePath, CombatTick tick)
    {
        _ = passivePath;
        _ = tick;
        return NoChange(Array.Empty<string>());
    }

    private static IReadOnlyList<string> ValidateToggleOn(CombatAttackToggleOnRequest request)
    {
        var errors = new List<string>();
        var player = request.Player;
        var target = request.Target;

        if (player.ActorKind != CombatActorKind.Player)
        {
            errors.Add("Attack is player-controlled and requires a player actor.");
        }

        if (!player.IsAlive)
        {
            errors.Add("Player must be alive to enable Attack.");
        }

        if (target is null)
        {
            errors.Add("Attack requires a valid hostile target.");
            return errors;
        }

        if (target.ActorKind != CombatActorKind.NPC)
        {
            errors.Add("Attack target must be a hostile actor.");
        }

        if (!target.IsAlive)
        {
            errors.Add("Attack target must be alive.");
        }

        if (!string.Equals(player.ZoneId, target.ZoneId, StringComparison.Ordinal))
        {
            errors.Add("Attack target must be in the player's active zone.");
        }

        if (!string.Equals(player.TargetCombatActorId, target.CombatActorId, StringComparison.Ordinal))
        {
            errors.Add("Attack target must match the player's selected target.");
        }

        if (!request.ZoneGate.CanRunHostileCombat(player.ZoneId) ||
            !request.ZoneGate.CanRunHostileCombat(target.ZoneId))
        {
            errors.Add("Active zone does not allow hostile Attack.");
        }

        if (request.DistanceMetersToTarget > player.MeleeRangeMeters)
        {
            errors.Add("Attack target is outside melee_range_meters.");
        }

        return errors;
    }

    private static bool IsApprovedOffPath(CombatAttackTransitionPath transitionPath)
    {
        return transitionPath is
            CombatAttackTransitionPath.PlayerToggleOff or
            CombatAttackTransitionPath.TargetDeath or
            CombatAttackTransitionPath.SuccessfulSitOrMed or
            CombatAttackTransitionPath.CombatExit or
            CombatAttackTransitionPath.PlayerDeath or
            CombatAttackTransitionPath.ZoneTransition;
    }

    private CombatAttackTransitionResult Change(
        CombatAttackMode mode,
        string? targetCombatActorId,
        long? nextSwingDueTick,
        CombatAttackTransitionPath transitionPath,
        CombatTick tick)
    {
        CurrentState = new CombatAttackStateSnapshot(mode, targetCombatActorId, nextSwingDueTick, transitionPath);
        var signal = new CombatAttackStateChangedSignal(mode == CombatAttackMode.On, targetCombatActorId, transitionPath, tick);
        stateChangedSignals.Add(signal);
        return new CombatAttackTransitionResult(CurrentState, Changed: true, signal, Array.Empty<string>());
    }

    private CombatAttackTransitionResult NoChange(IReadOnlyList<string> rejectionReasons)
    {
        return new CombatAttackTransitionResult(
            CurrentState,
            Changed: false,
            StateChangedSignal: null,
            RejectionReasons: rejectionReasons.ToArray());
    }
}
