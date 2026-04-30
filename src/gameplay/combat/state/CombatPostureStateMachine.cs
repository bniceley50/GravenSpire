#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gravenspire.Gameplay.Combat;

public enum CombatPostureState
{
    Standing,
    Sitting
}

public sealed record CombatSitRequest(
    CombatActorState Player,
    CombatAttackStateMachine AttackStateMachine,
    CombatTick Tick,
    bool IsGrounded,
    bool IsMoving,
    bool IsZoneLoadingCommitLocked);

public sealed record CombatSitResult(
    CombatActorState Player,
    CombatAttackTransitionResult AttackTransition,
    IReadOnlyList<string> RejectionReasons)
{
    public bool Succeeded => RejectionReasons.Count == 0;
}

public sealed class CombatPostureStateMachine
{
    public CombatSitResult TrySit(CombatSitRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Player);
        ArgumentNullException.ThrowIfNull(request.AttackStateMachine);

        var rejections = ValidateSit(request);
        if (rejections.Count > 0)
        {
            return new CombatSitResult(
                request.Player,
                request.AttackStateMachine.ObservePassivePath(CombatAttackPassivePath.TargetSelection, request.Tick),
                rejections);
        }

        var attackTransition = request.AttackStateMachine.ForceOff(
            CombatAttackTransitionPath.SuccessfulSitOrMed,
            request.Tick);
        var seated = request.Player with { PostureState = CombatPostureState.Sitting };

        return new CombatSitResult(
            seated,
            attackTransition,
            Array.Empty<string>());
    }

    private static IReadOnlyList<string> ValidateSit(CombatSitRequest request)
    {
        var errors = new List<string>();
        if (request.Player.ActorKind != CombatActorKind.Player)
        {
            errors.Add("Only the player actor may enter the sit/med posture.");
        }

        if (!request.Player.IsAlive)
        {
            errors.Add("Player must be alive to sit.");
        }

        if (!request.IsGrounded)
        {
            errors.Add("Player must be grounded to sit.");
        }

        if (request.IsMoving)
        {
            errors.Add("Player must be stationary to sit.");
        }

        if (request.IsZoneLoadingCommitLocked)
        {
            errors.Add("Player cannot sit during ZoneLoading commit lock.");
        }

        if (request.Player.CastRuntimeState != CombatCastRuntimeState.None ||
            request.Player.CombatState is CombatState.Casting or CombatState.Interrupted or CombatState.Recovery)
        {
            errors.Add("Player cannot sit while casting, interrupted, or in recovery.");
        }

        return new ReadOnlyCollection<string>(errors);
    }
}
