#nullable enable

using System;
using System.Collections.Generic;

namespace Gravenspire.Gameplay.Npc.M3Objective;

public enum M3ObjectiveState
{
    NotIntroduced,
    Accepted,
    RelicRecovered,
    Complete
}

public readonly struct M3ObjectiveTransition
{
    public M3ObjectiveTransition(
        M3ObjectiveState from,
        M3ObjectiveState to,
        string actionId,
        string actorId,
        string itemId)
    {
        From = from;
        To = to;
        ActionId = actionId;
        ActorId = actorId;
        ItemId = itemId;
    }

    public M3ObjectiveState From { get; }

    public M3ObjectiveState To { get; }

    public string ActionId { get; }

    public string ActorId { get; }

    public string ItemId { get; }
}

public sealed class M3ObjectiveStateRelicHandInSession
{
    public const string ObjectiveId = "m3.objective.recover_marked_relic";
    public const string CaretakerNpcId = "M3_Caretaker_T1";
    public const string RelicItemId = "CourtMarkedRelic_T1";
    public const string RelicObjectName = "M3_ObjectiveRelic";
    public const string AcceptActionId = "m3.objective.accept";
    public const string RecoverActionId = "m3.objective.relic_recovered";
    public const string HandInActionId = "m3.objective.relic_handed_in";

    private readonly List<M3ObjectiveTransition> _transitions = new();
    private readonly List<M3ObjectiveState> _stateSequence = new();

    public M3ObjectiveStateRelicHandInSession()
    {
        State = M3ObjectiveState.NotIntroduced;
        _stateSequence.Add(State);
    }

    public M3ObjectiveState State { get; private set; }

    public bool RelicAvailable => State == M3ObjectiveState.Accepted;

    public bool CarriesRelic => State == M3ObjectiveState.RelicRecovered;

    public bool IsComplete => State == M3ObjectiveState.Complete;

    public bool SessionLocalOnly => true;

    public IReadOnlyList<M3ObjectiveTransition> Transitions => _transitions;

    public IReadOnlyList<M3ObjectiveState> StateSequence => _stateSequence;

    public bool TryAcceptObjective(string npcId, string playerActorId, out string rejectionReason)
    {
        rejectionReason = string.Empty;

        if (!string.Equals(npcId, CaretakerNpcId, StringComparison.Ordinal))
        {
            rejectionReason = "Objective can only be accepted from the M3 caretaker.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(playerActorId))
        {
            rejectionReason = "Objective acceptance requires a player actor id.";
            return false;
        }

        return TryTransition(
            M3ObjectiveState.NotIntroduced,
            M3ObjectiveState.Accepted,
            AcceptActionId,
            playerActorId,
            itemId: string.Empty,
            out rejectionReason);
    }

    public bool TryRecoverRelic(string relicObjectName, string itemId, out string rejectionReason)
    {
        rejectionReason = string.Empty;

        if (!string.Equals(relicObjectName, RelicObjectName, StringComparison.Ordinal))
        {
            rejectionReason = "Recovery requires the authored M3 relic object.";
            return false;
        }

        if (!string.Equals(itemId, RelicItemId, StringComparison.Ordinal))
        {
            rejectionReason = "Recovery requires the Court marked relic.";
            return false;
        }

        return TryTransition(
            M3ObjectiveState.Accepted,
            M3ObjectiveState.RelicRecovered,
            RecoverActionId,
            actorId: string.Empty,
            RelicItemId,
            out rejectionReason);
    }

    public bool TryHandInRelic(string npcId, string itemId, out string rejectionReason)
    {
        rejectionReason = string.Empty;

        if (!string.Equals(npcId, CaretakerNpcId, StringComparison.Ordinal))
        {
            rejectionReason = "Relic hand-in requires the M3 caretaker.";
            return false;
        }

        if (!string.Equals(itemId, RelicItemId, StringComparison.Ordinal))
        {
            rejectionReason = "Relic hand-in requires the Court marked relic.";
            return false;
        }

        return TryTransition(
            M3ObjectiveState.RelicRecovered,
            M3ObjectiveState.Complete,
            HandInActionId,
            CaretakerNpcId,
            RelicItemId,
            out rejectionReason);
    }

    public bool HasExactStateSequence(params M3ObjectiveState[] expectedStates)
    {
        if (expectedStates == null)
        {
            throw new ArgumentNullException(nameof(expectedStates));
        }

        if (expectedStates.Length != _stateSequence.Count)
        {
            return false;
        }

        for (var index = 0; index < expectedStates.Length; index++)
        {
            if (expectedStates[index] != _stateSequence[index])
            {
                return false;
            }
        }

        return true;
    }

    public string FormatStateSequence()
    {
        return string.Join(" -> ", _stateSequence);
    }

    private bool TryTransition(
        M3ObjectiveState requiredState,
        M3ObjectiveState nextState,
        string actionId,
        string actorId,
        string itemId,
        out string rejectionReason)
    {
        rejectionReason = string.Empty;

        if (State != requiredState)
        {
            rejectionReason = $"Objective state {State} cannot run {actionId}; expected {requiredState}.";
            return false;
        }

        var previousState = State;
        State = nextState;
        _transitions.Add(new M3ObjectiveTransition(previousState, nextState, actionId, actorId, itemId));
        _stateSequence.Add(State);
        return true;
    }
}
