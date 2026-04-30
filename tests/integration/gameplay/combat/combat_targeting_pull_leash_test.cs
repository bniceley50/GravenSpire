#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using NUnit.Framework;

namespace Gravenspire.Tests.Integration.Gameplay.Combat;

public sealed class CombatTargetingPullLeashTest
{
    [Test]
    public void test_haunt_zone_gate_enables_claim_targeting_and_pull()
    {
        var gate = new CombatZoneGate();
        var activation = gate.ActivateZone("Haunt_Prototype_T1", CombatZoneType.HauntZone);
        var player = CreatePlayer();
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");
        var target = TargetCandidate(hostile, 8d);
        var primary = PullCandidate(hostile, 4d, CombatSocialAssistProfile.T1Default("court-social"));

        var selection = new CombatTargetSelector().SelectNextTarget(
            player,
            new CombatPoint3(0, 0, 0),
            new[] { target },
            gate);
        var pull = new CombatPullCoordinator().ResolveBodyPull(
            player,
            new CombatPoint3(0, 0, 0),
            primary,
            Array.Empty<CombatPullCandidate>(),
            gate,
            CombatTick.Zero);

        Assert.That(activation.HostileCombatEnabled, Is.True);
        Assert.That(gate.CanClaimHostileActor("Haunt_Prototype_T1"), Is.True);
        Assert.That(gate.CanCreateThreat("Haunt_Prototype_T1"), Is.True);
        Assert.That(gate.CanEmitKillCredit("Haunt_Prototype_T1"), Is.True);
        Assert.That(selection.Target, Is.SameAs(hostile));
        Assert.That(pull.Succeeded, Is.True, string.Join(Environment.NewLine, pull.Errors));
        Assert.That(pull.PrimaryHostile!.CombatState, Is.EqualTo(CombatState.Pulling));
        Assert.That(pull.PrimaryHostile.ThreatTable[player.CombatActorId], Is.EqualTo(25));
    }

    [Test]
    public void test_city_hub_zone_disables_targeting_claim_threat_damage_and_kill_credit()
    {
        var gate = new CombatZoneGate();
        gate.ActivateZone("CityHub_T1", CombatZoneType.CityHubZone);
        var player = CreatePlayer(zoneId: "CityHub_T1");
        var hostile = CreateHostile("combat-city-npc-1", "city-npc-001", zoneId: "CityHub_T1");
        var target = TargetCandidate(hostile, 4d);
        var primary = PullCandidate(hostile, 2d, CombatSocialAssistProfile.T1Default("court-social"));

        var selection = new CombatTargetSelector().SelectNextTarget(
            player,
            new CombatPoint3(0, 0, 0),
            new[] { target },
            gate);
        var pull = new CombatPullCoordinator().ResolveBodyPull(
            player,
            new CombatPoint3(0, 0, 0),
            primary,
            Array.Empty<CombatPullCandidate>(),
            gate,
            CombatTick.Zero);

        Assert.That(gate.CanTargetHostile("CityHub_T1"), Is.False);
        Assert.That(gate.CanClaimHostileActor("CityHub_T1"), Is.False);
        Assert.That(gate.CanCreateThreat("CityHub_T1"), Is.False);
        Assert.That(gate.CanApplyDamage("CityHub_T1"), Is.False);
        Assert.That(gate.CanEmitKillCredit("CityHub_T1"), Is.False);
        Assert.That(selection.Succeeded, Is.False);
        Assert.That(pull.Succeeded, Is.False);
        Assert.That(pull.PrimaryHostile, Is.Null);
    }

    [Test]
    public void test_zone_transition_clears_transient_combat_state_and_blocks_incoming_zone_results()
    {
        var gate = new CombatZoneGate();
        gate.ActivateZone("Haunt_Prototype_T1", CombatZoneType.HauntZone);
        var player = CreatePlayer()
            .WithTarget("combat-hostile-1")
            .SetThreat("combat-hostile-1", 25)
            .WithCombatState(CombatState.Casting);
        var hostile = CreateHostile("combat-hostile-1", "hostile-001")
            .ClaimHostile(player.CombatActorId, 25, CombatState.InCombat);

        var cleanup = gate.BeginZoneTransition(
            "Haunt_Prototype_T1",
            "CityHub_T1",
            new[] { player, hostile });

        Assert.That(cleanup.CastingCancelled, Is.True);
        Assert.That(cleanup.AutoAttackDisabled, Is.True);
        Assert.That(cleanup.TransientHitWindowsCleared, Is.True);
        Assert.That(cleanup.ProjectilesCleared, Is.True);
        Assert.That(cleanup.CancellationEventsEmitted, Is.True);
        Assert.That(cleanup.IncomingZoneCombatResultsBlocked, Is.True);
        Assert.That(cleanup.CleanedActors.Single(actor => actor.CombatActorId == player.CombatActorId).TargetCombatActorId, Is.Null);
        Assert.That(cleanup.CleanedActors.Single(actor => actor.CombatActorId == player.CombatActorId).ThreatTable, Is.Empty);
        Assert.That(cleanup.CleanedActors.Single(actor => actor.CombatActorId == hostile.CombatActorId).ThreatTable, Is.Empty);
    }

    [Test]
    public void test_target_selection_filters_dead_out_of_radius_blocked_los_and_orders_deterministically()
    {
        var gate = new CombatZoneGate();
        gate.ActivateZone("Haunt_Prototype_T1", CombatZoneType.HauntZone);
        var player = CreatePlayer();
        var selected = CreateHostile("combat-hostile-selected", "hostile-001");
        var laterTie = CreateHostile("combat-hostile-later", "hostile-002");
        var dead = CreateHostile("combat-hostile-dead", "hostile-000", currentHealth: 0, lifeState: CombatActorLifeState.Dead);
        var outside = CreateHostile("combat-hostile-outside", "hostile-003");
        var blocked = CreateHostile("combat-hostile-blocked", "hostile-004");

        var result = new CombatTargetSelector().SelectNextTarget(
            player,
            new CombatPoint3(0, 0, 0),
            new[]
            {
                TargetCandidate(laterTie, 10d, colliderIndex: 1),
                TargetCandidate(selected, 10d, colliderIndex: 0),
                TargetCandidate(dead, 1d),
                TargetCandidate(outside, 40d),
                TargetCandidate(blocked, 2d, new[] { CombatLosLayer.WorldSolid })
            },
            gate);

        Assert.That(result.Target, Is.SameAs(selected));
        Assert.That(result.RejectionReasons, Has.Some.Contains("actor is not alive"));
        Assert.That(result.RejectionReasons, Has.Some.Contains("outside target_acquire_radius_meters"));
        Assert.That(result.RejectionReasons, Has.Some.Contains("T1 line of sight is blocked"));
    }

    [Test]
    public void test_body_pull_initializes_threat_and_pivot_signal_without_warning_affordances()
    {
        var gate = new CombatZoneGate();
        gate.ActivateZone("Haunt_Prototype_T1", CombatZoneType.HauntZone);
        var player = CreatePlayer();
        var hostile = CreateHostile("combat-hostile-1", "hostile-001");

        var result = new CombatPullCoordinator().ResolveBodyPull(
            player,
            new CombatPoint3(0, 0, 0),
            PullCandidate(hostile, 4d, CombatSocialAssistProfile.T1Default("court-social")),
            Array.Empty<CombatPullCandidate>(),
            gate,
            CombatTick.Zero);

        Assert.That(result.Succeeded, Is.True, string.Join(Environment.NewLine, result.Errors));
        Assert.That(result.PrimaryHostile!.ThreatTable[player.CombatActorId], Is.EqualTo(25));
        Assert.That(result.PlayerAttackEnabled, Is.False);
        Assert.That(result.PresentationSignals, Is.EquivalentTo(new[] { CombatPullPresentationSignal.EnemyPivotOrStanceShift }));
        Assert.That(result.ForbiddenWarningSignals, Is.Empty);
        Assert.That(result.ScriptedEncounterTriggerStarted, Is.False);
    }

    [Test]
    public void test_social_assist_joins_eligible_actors_in_deterministic_order_with_default_threat()
    {
        var gate = new CombatZoneGate();
        gate.ActivateZone("Haunt_Prototype_T1", CombatZoneType.HauntZone);
        var player = CreatePlayer();
        var primary = PullCandidate(
            CreateHostile("combat-primary", "primary"),
            4d,
            CombatSocialAssistProfile.T1Default("court-social", "encounter"));
        var later = PullCandidate(
            CreateHostile("combat-assist-later", "assist-later"),
            7d,
            CombatSocialAssistProfile.T1Default("court-social", "encounter", assistOrderIndex: 1));
        var first = PullCandidate(
            CreateHostile("combat-assist-first", "assist-first"),
            7d,
            CombatSocialAssistProfile.T1Default("court-social", "encounter", assistOrderIndex: 0));

        var result = new CombatPullCoordinator().ResolveBodyPull(
            player,
            new CombatPoint3(0, 0, 0),
            primary,
            new[] { later, first },
            gate,
            CombatTick.Zero);

        Assert.That(result.Succeeded, Is.True, string.Join(Environment.NewLine, result.Errors));
        Assert.That(result.AssistingHostiles.Select(actor => actor.CombatActorId), Is.EqualTo(new[]
        {
            "combat-assist-first",
            "combat-assist-later"
        }));
        foreach (var actor in result.AssistingHostiles)
        {
            Assert.That(actor.ThreatTable[player.CombatActorId], Is.EqualTo(25));
            Assert.That(actor.CombatState, Is.EqualTo(CombatState.InCombat));
        }
    }

    [Test]
    public void test_los_query_contract_blocks_only_t1_occluder_layers_and_reports_buffer_overflow()
    {
        var gate = new CombatZoneGate();
        gate.ActivateZone("Haunt_Prototype_T1", CombatZoneType.HauntZone);
        var player = CreatePlayer();
        var candidates = Enumerable.Range(0, 65)
            .Select(index => TargetCandidate(
                CreateHostile($"combat-hostile-{index:00}", $"hostile-{index:00}"),
                5d,
                colliderIndex: index))
            .ToArray();

        var result = new CombatTargetSelector().SelectNextTarget(
            player,
            new CombatPoint3(0, 0, 0),
            candidates,
            gate);

        Assert.That(T1CombatLineOfSight.HasLineOfSight(new[] { CombatLosLayer.CombatActor, CombatLosLayer.TriggerOnly, CombatLosLayer.InteractableSoft, CombatLosLayer.Vfx }), Is.True);
        Assert.That(T1CombatLineOfSight.HasLineOfSight(new[] { CombatLosLayer.WorldSolid }), Is.False);
        Assert.That(T1CombatLineOfSight.HasLineOfSight(new[] { CombatLosLayer.ClosedDoor }), Is.False);
        Assert.That(T1CombatLineOfSight.HasLineOfSight(new[] { CombatLosLayer.LargeProp }), Is.False);
        Assert.That(result.OverflowDiagnostic, Is.Not.Null);
        Assert.That(result.OverflowDiagnostic!.Code, Is.EqualTo("CombatQueryBufferOverflow"));
        Assert.That(result.OverflowDiagnostic.CombatQueryBufferSize, Is.EqualTo(64));
        Assert.That(result.OverflowDiagnostic.ReturnedCandidateCount, Is.EqualTo(65));
        Assert.That(result.OrderedCandidates, Has.Count.EqualTo(64));
    }

    [Test]
    public void test_social_assist_pulses_are_bounded_and_join_once_per_pull_episode()
    {
        var gate = new CombatZoneGate();
        gate.ActivateZone("Haunt_Prototype_T1", CombatZoneType.HauntZone);
        var player = CreatePlayer();
        var primary = PullCandidate(
            CreateHostile("combat-primary", "primary"),
            4d,
            CombatSocialAssistProfile.T1Default("court-social", "encounter"));
        var assistA = PullCandidate(
            CreateHostile("combat-assist-a", "assist-a"),
            8d,
            CombatSocialAssistProfile.T1Default("court-social", "encounter", assistOrderIndex: 0));
        var assistB = PullCandidate(
            CreateHostile("combat-assist-b", "assist-b"),
            9d,
            CombatSocialAssistProfile.T1Default("court-social", "encounter", assistOrderIndex: 1));
        var coordinator = new CombatPullCoordinator();
        var started = coordinator.ResolveBodyPull(
            player,
            new CombatPoint3(0, 0, 0),
            primary,
            Array.Empty<CombatPullCandidate>(),
            gate,
            CombatTick.Zero);

        var earlyPulse = coordinator.ResolveAssistPulse(
            player,
            primary,
            new[] { assistA },
            gate,
            started.Episode!,
            new CombatTick(50, 1d));
        var firstPulse = coordinator.ResolveAssistPulse(
            player,
            primary,
            new[] { assistA },
            gate,
            earlyPulse.Episode!,
            new CombatTick(100, 2d));
        var secondPulse = coordinator.ResolveAssistPulse(
            player,
            primary,
            new[] { assistA, assistB },
            gate,
            firstPulse.Episode!,
            new CombatTick(200, 4d));

        Assert.That(earlyPulse.AssistingHostiles, Is.Empty);
        Assert.That(firstPulse.AssistingHostiles.Select(actor => actor.CombatActorId), Is.EqualTo(new[] { "combat-assist-a" }));
        Assert.That(secondPulse.AssistingHostiles.Select(actor => actor.CombatActorId), Is.EqualTo(new[] { "combat-assist-b" }));
        Assert.That(secondPulse.Episode!.JoinedHostileActorIds, Is.EquivalentTo(new[]
        {
            "combat-primary",
            "combat-assist-a",
            "combat-assist-b"
        }));
    }

    [Test]
    public void test_path_failure_enters_leashing_after_grace_and_requests_return_to_anchor()
    {
        var player = CreatePlayer();
        var hostile = CreateHostile("combat-hostile-1", "hostile-001")
            .ClaimHostile(player.CombatActorId, 25, CombatState.InCombat);
        var coordinator = new CombatLeashCoordinator();
        var first = coordinator.EvaluatePathAndDistance(
            hostile,
            new CombatPoint3(0, 0, 0),
            new CombatPoint3(0, 0, 0),
            new CombatPathProbeResult(hostile.CombatActorId, CombatPathStatus.PathPartial, PathPending: false, SampledTick: 0),
            CombatTick.Zero);

        var afterGrace = coordinator.EvaluatePathAndDistance(
            hostile,
            new CombatPoint3(0, 0, 0),
            new CombatPoint3(0, 0, 0),
            new CombatPathProbeResult(hostile.CombatActorId, CombatPathStatus.PathPartial, PathPending: false, SampledTick: 51),
            new CombatTick(51, 1.02d),
            first.State);

        Assert.That(first.EnteredLeashing, Is.False);
        Assert.That(afterGrace.EnteredLeashing, Is.True);
        Assert.That(afterGrace.Actor.CombatState, Is.EqualTo(CombatState.Leashing));
        Assert.That(afterGrace.ReturnToAnchorRequested, Is.True);
        Assert.That(afterGrace.NewAttacksAndCastsStopped, Is.True);
        Assert.That(afterGrace.ActiveAttackIntentCleared, Is.True);
        Assert.That(afterGrace.Actor.ThreatTable[player.CombatActorId], Is.EqualTo(25));
    }

    [Test]
    public void test_reaggro_requires_active_memory_distance_los_and_expiry_clears_threat()
    {
        var player = CreatePlayer();
        var hostile = CreateHostile("combat-hostile-1", "hostile-001")
            .ClaimHostile(player.CombatActorId, 25, CombatState.InCombat);
        var coordinator = new CombatLeashCoordinator();
        var leashing = coordinator.EvaluatePathAndDistance(
            hostile,
            new CombatPoint3(40, 0, 0),
            new CombatPoint3(0, 0, 0),
            new CombatPathProbeResult(hostile.CombatActorId, CombatPathStatus.PathComplete, PathPending: false, SampledTick: 0),
            CombatTick.Zero);

        var blocked = coordinator.EvaluateReAggro(
            leashing.Actor,
            leashing.State,
            new CombatPoint3(10, 0, 0),
            new CombatPoint3(15, 0, 0),
            hasLineOfSightToTarget: false,
            new CombatTick(100, 2d));
        var reaggro = coordinator.EvaluateReAggro(
            leashing.Actor,
            leashing.State,
            new CombatPoint3(10, 0, 0),
            new CombatPoint3(15, 0, 0),
            hasLineOfSightToTarget: true,
            new CombatTick(100, 2d));
        var expired = coordinator.EvaluateReAggro(
            leashing.Actor,
            leashing.State,
            new CombatPoint3(10, 0, 0),
            new CombatPoint3(15, 0, 0),
            hasLineOfSightToTarget: true,
            new CombatTick(1500, 30d));

        Assert.That(blocked.ReAggroed, Is.False);
        Assert.That(blocked.Actor.CombatState, Is.EqualTo(CombatState.Leashing));
        Assert.That(reaggro.ReAggroed, Is.True);
        Assert.That(reaggro.Actor.CombatState, Is.EqualTo(CombatState.InCombat));
        Assert.That(reaggro.Actor.ThreatTable[player.CombatActorId], Is.EqualTo(25));
        Assert.That(expired.ThreatMemoryExpired, Is.True);
        Assert.That(expired.ThreatCleared, Is.True);
        Assert.That(expired.Actor.ThreatTable, Is.Empty);
        Assert.That(expired.Actor.CombatState, Is.EqualTo(CombatState.OutOfCombat));
    }

    private static CombatActorState CreatePlayer(string zoneId = "Haunt_Prototype_T1")
    {
        return new CombatActorState(
            "combat-player-1",
            CombatActorKind.Player,
            CombatStableSourceRef.ForPlayer("local-character-1"),
            "PlayerLocal_T1",
            zoneId,
            5,
            140,
            140,
            180,
            180,
            35,
            25,
            8,
            30,
            30,
            2.8d,
            2.0d,
            30.0d,
            CombatState.OutOfCombat,
            CombatActorLifeState.Alive,
            null,
            "player-local-character-1");
    }

    private static CombatActorState CreateHostile(
        string combatActorId,
        string sortKey,
        string zoneId = "Haunt_Prototype_T1",
        int currentHealth = 120,
        CombatActorLifeState lifeState = CombatActorLifeState.Alive)
    {
        return new CombatActorState(
            combatActorId,
            CombatActorKind.NPC,
            CombatStableSourceRef.ForSpawn(new CombatSpawnSourceRef("VampireCourt_T1", $"{combatActorId}-anchor", "VampireThrall_T1")),
            "VampireCourt_T1",
            zoneId,
            5,
            120,
            currentHealth,
            0,
            0,
            30,
            25,
            8,
            30,
            30,
            3.0d,
            2.0d,
            0.0d,
            lifeState == CombatActorLifeState.Alive ? CombatState.OutOfCombat : CombatState.Dead,
            lifeState,
            null,
            sortKey);
    }

    private static CombatTargetCandidate TargetCandidate(
        CombatActorState actor,
        double x,
        IReadOnlyList<CombatLosLayer>? losBlockingLayers = null,
        int colliderIndex = 0)
    {
        return new CombatTargetCandidate(
            actor,
            new CombatPoint3(x, 0, 0),
            Anchors(x),
            losBlockingLayers ?? Array.Empty<CombatLosLayer>(),
            colliderIndex);
    }

    private static CombatPullCandidate PullCandidate(
        CombatActorState actor,
        double x,
        CombatSocialAssistProfile? profile,
        IReadOnlyList<CombatLosLayer>? losToPlayer = null,
        IReadOnlyList<CombatLosLayer>? losToPrimary = null,
        int colliderIndex = 0)
    {
        return new CombatPullCandidate(
            actor,
            new CombatPoint3(x, 0, 0),
            Anchors(x),
            AggroRadiusMeters: 6d,
            profile,
            losToPlayer ?? Array.Empty<CombatLosLayer>(),
            losToPrimary ?? Array.Empty<CombatLosLayer>(),
            colliderIndex);
    }

    private static CombatSpatialAnchorSet Anchors(double x)
    {
        return new CombatSpatialAnchorSet(
            new CombatPoint3(x, 1.6d, 0),
            new CombatPoint3(x, 1.4d, 0),
            "authored_aggro_eye_anchor",
            "authored_player_los_anchor");
    }
}
