#nullable enable

using Gravenspire.Gameplay.Npc.M3Objective;
using NUnit.Framework;

namespace Gravenspire.Tests.Unit.Gameplay.Npc;

public sealed class M3ObjectiveStateRelicHandInTest
{
    [Test]
    public void test_accepting_objective_transitions_not_introduced_to_accepted()
    {
        var session = new M3ObjectiveStateRelicHandInSession();

        var accepted = session.TryAcceptObjective(
            M3ObjectiveStateRelicHandInSession.CaretakerNpcId,
            "m3-player-cleric",
            out var rejectionReason);

        Assert.That(accepted, Is.True, rejectionReason);
        Assert.That(session.State, Is.EqualTo(M3ObjectiveState.Accepted));
        Assert.That(session.RelicAvailable, Is.True);
        Assert.That(session.HasExactStateSequence(M3ObjectiveState.NotIntroduced, M3ObjectiveState.Accepted), Is.True);
        Assert.That(session.Transitions[0].ActionId, Is.EqualTo(M3ObjectiveStateRelicHandInSession.AcceptActionId));
    }

    [Test]
    public void test_relic_marker_is_available_only_after_objective_acceptance()
    {
        var session = new M3ObjectiveStateRelicHandInSession();

        Assert.That(session.RelicAvailable, Is.False);

        session.TryAcceptObjective(
            M3ObjectiveStateRelicHandInSession.CaretakerNpcId,
            "m3-player-cleric",
            out _);

        Assert.That(session.RelicAvailable, Is.True);
        Assert.That(session.CarriesRelic, Is.False);
        Assert.That(session.IsComplete, Is.False);
    }

    [Test]
    public void test_recovering_relic_transitions_to_relic_recovered_and_records_carried_state()
    {
        var session = AcceptedSession();

        var recovered = session.TryRecoverRelic(
            M3ObjectiveStateRelicHandInSession.RelicObjectName,
            M3ObjectiveStateRelicHandInSession.RelicItemId,
            out var rejectionReason);

        Assert.That(recovered, Is.True, rejectionReason);
        Assert.That(session.State, Is.EqualTo(M3ObjectiveState.RelicRecovered));
        Assert.That(session.RelicAvailable, Is.False);
        Assert.That(session.CarriesRelic, Is.True);
        Assert.That(session.HasExactStateSequence(
            M3ObjectiveState.NotIntroduced,
            M3ObjectiveState.Accepted,
            M3ObjectiveState.RelicRecovered), Is.True);
    }

    [Test]
    public void test_returning_relic_to_named_npc_transitions_to_complete()
    {
        var session = RecoveredSession();

        var completed = session.TryHandInRelic(
            M3ObjectiveStateRelicHandInSession.CaretakerNpcId,
            M3ObjectiveStateRelicHandInSession.RelicItemId,
            out var rejectionReason);

        Assert.That(completed, Is.True, rejectionReason);
        Assert.That(session.State, Is.EqualTo(M3ObjectiveState.Complete));
        Assert.That(session.RelicAvailable, Is.False);
        Assert.That(session.CarriesRelic, Is.False);
        Assert.That(session.IsComplete, Is.True);
        Assert.That(session.FormatStateSequence(), Is.EqualTo("NotIntroduced -> Accepted -> RelicRecovered -> Complete"));
    }

    [Test]
    public void test_invalid_order_does_not_skip_or_duplicate_objective_state()
    {
        var session = new M3ObjectiveStateRelicHandInSession();

        var recoveredTooEarly = session.TryRecoverRelic(
            M3ObjectiveStateRelicHandInSession.RelicObjectName,
            M3ObjectiveStateRelicHandInSession.RelicItemId,
            out var recoveryRejection);
        var completedTooEarly = session.TryHandInRelic(
            M3ObjectiveStateRelicHandInSession.CaretakerNpcId,
            M3ObjectiveStateRelicHandInSession.RelicItemId,
            out var completionRejection);

        Assert.That(recoveredTooEarly, Is.False);
        Assert.That(completedTooEarly, Is.False);
        Assert.That(recoveryRejection, Does.Contain("expected Accepted"));
        Assert.That(completionRejection, Does.Contain("expected RelicRecovered"));
        Assert.That(session.State, Is.EqualTo(M3ObjectiveState.NotIntroduced));
        Assert.That(session.StateSequence, Has.Count.EqualTo(1));
        Assert.That(session.Transitions, Is.Empty);
    }

    private static M3ObjectiveStateRelicHandInSession AcceptedSession()
    {
        var session = new M3ObjectiveStateRelicHandInSession();
        session.TryAcceptObjective(M3ObjectiveStateRelicHandInSession.CaretakerNpcId, "m3-player-cleric", out _);
        return session;
    }

    private static M3ObjectiveStateRelicHandInSession RecoveredSession()
    {
        var session = AcceptedSession();
        session.TryRecoverRelic(
            M3ObjectiveStateRelicHandInSession.RelicObjectName,
            M3ObjectiveStateRelicHandInSession.RelicItemId,
            out _);
        return session;
    }
}
