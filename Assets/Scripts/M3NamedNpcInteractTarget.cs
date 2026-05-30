#nullable enable

using System.Collections.Generic;
using Gravenspire.Gameplay.Npc.M3Objective;
using Gravenspire.UnityRuntime.Interaction;
using UnityEngine;

namespace Gravenspire.UnityRuntime.Npc
{
    [DisallowMultipleComponent]
    public sealed class M3NamedNpcInteractTarget : MonoBehaviour, IPlayerInteractTarget, IPlayerInteractTelemetryTarget
    {
        public const string TelemetryEvent = "npc_interaction_intentional";
        public const string ObjectiveAcceptedTelemetryEvent = "objective_accepted";
        public const string RelicHandedInTelemetryEvent = "relic_handed_in";
        public const string PayloadKind = "npc_interaction_context:player_driven";
        public const string ObjectiveTransitionPayloadKind = "objective_transition:player_driven";
        public const string SourceAttribution = "player_driven";

        [SerializeField] private M3NamedNpcObjectiveFrame? _objectiveFrame;
        [SerializeField] private M3ObjectiveStateRelicHandIn? _objectiveState;

        private readonly List<InteractContext> _lastInteractTelemetryEvents = new();
        private bool _missingFrameLogged;
        private bool _missingStateLogged;

        public M3NamedNpcObjectiveFrame? ObjectiveFrame => ResolveObjectiveFrame(logIfMissing: false);

        public M3ObjectiveStateRelicHandIn? ObjectiveState => ResolveObjectiveState(logIfMissing: false);

        public IReadOnlyList<InteractContext> LastInteractTelemetryEvents => _lastInteractTelemetryEvents;

        public int InteractionAttemptCount { get; private set; }

        public int SuccessfulInteractionCount { get; private set; }

        public NpcInteractionContext LastNpcInteractionContext { get; private set; }

        private void Awake()
        {
            ResolveObjectiveFrame(logIfMissing: true);
            ResolveObjectiveState(logIfMissing: false);
        }

        public void Configure(M3NamedNpcObjectiveFrame objectiveFrame)
        {
            Configure(objectiveFrame, objectiveState: null);
        }

        public void Configure(M3NamedNpcObjectiveFrame objectiveFrame, M3ObjectiveStateRelicHandIn? objectiveState)
        {
            _objectiveFrame = objectiveFrame;
            _objectiveState = objectiveState;
            _missingFrameLogged = false;
            _missingStateLogged = false;
        }

        public bool TryInteract(string playerActorId, float distanceMeters, out InteractContext context)
        {
            context = default;
            InteractionAttemptCount++;
            _lastInteractTelemetryEvents.Clear();

            var objectiveFrame = ResolveObjectiveFrame(logIfMissing: true);
            if (objectiveFrame == null)
            {
                return false;
            }

            var objectiveState = ResolveObjectiveState(logIfMissing: false);
            if (objectiveState == null)
            {
                return TryRecordNpcInteractionOnly(
                    objectiveFrame,
                    playerActorId,
                    distanceMeters,
                    out context);
            }

            return objectiveState.State switch
            {
                M3ObjectiveState.NotIntroduced => TryAcceptObjectiveFromNpc(
                    objectiveState,
                    objectiveFrame,
                    playerActorId,
                    distanceMeters,
                    out context),
                M3ObjectiveState.Accepted => TryRecordNpcInteractionOnly(
                    objectiveFrame,
                    playerActorId,
                    distanceMeters,
                    out context),
                M3ObjectiveState.RelicRecovered => TryHandInRecoveredRelic(
                    objectiveState,
                    objectiveFrame,
                    playerActorId,
                    distanceMeters,
                    out context),
                M3ObjectiveState.Complete => TryRecordNpcInteractionOnly(
                    objectiveFrame,
                    playerActorId,
                    distanceMeters,
                    out context),
                _ => false
            };
        }

        private bool TryAcceptObjectiveFromNpc(
            M3ObjectiveStateRelicHandIn objectiveState,
            M3NamedNpcObjectiveFrame objectiveFrame,
            string playerActorId,
            float distanceMeters,
            out InteractContext context)
        {
            context = default;
            var fromState = objectiveState.State;
            if (!objectiveState.TryAcceptObjectiveFromNpc(objectiveFrame, playerActorId, distanceMeters))
            {
                return false;
            }

            var npcContext = objectiveFrame.LastInteraction;
            SuccessfulInteractionCount++;
            LastNpcInteractionContext = npcContext;
            context = MapToInteractContext(npcContext);
            _lastInteractTelemetryEvents.Add(context);
            _lastInteractTelemetryEvents.Add(MapObjectiveTransitionContext(
                ObjectiveAcceptedTelemetryEvent,
                playerActorId,
                objectiveFrame.ConfiguredNpcId,
                "objective_accept",
                fromState,
                objectiveState.State,
                distanceMeters));
            return true;
        }

        private bool TryHandInRecoveredRelic(
            M3ObjectiveStateRelicHandIn objectiveState,
            M3NamedNpcObjectiveFrame objectiveFrame,
            string playerActorId,
            float distanceMeters,
            out InteractContext context)
        {
            context = default;
            var fromState = objectiveState.State;
            if (!objectiveState.TryReturnRelicToNpc(objectiveFrame))
            {
                return false;
            }

            SuccessfulInteractionCount++;
            context = MapObjectiveTransitionContext(
                RelicHandedInTelemetryEvent,
                playerActorId,
                objectiveFrame.ConfiguredNpcId,
                "relic_hand_in",
                fromState,
                objectiveState.State,
                distanceMeters);
            _lastInteractTelemetryEvents.Add(context);
            return true;
        }

        private bool TryRecordNpcInteractionOnly(
            M3NamedNpcObjectiveFrame objectiveFrame,
            string playerActorId,
            float distanceMeters,
            out InteractContext context)
        {
            context = default;
            if (!objectiveFrame.TryRecordIntentionalInteraction(playerActorId, distanceMeters, out var npcContext))
            {
                return false;
            }

            SuccessfulInteractionCount++;
            LastNpcInteractionContext = npcContext;
            context = MapToInteractContext(npcContext);
            _lastInteractTelemetryEvents.Add(context);
            return true;
        }

        private M3NamedNpcObjectiveFrame? ResolveObjectiveFrame(bool logIfMissing)
        {
            if (_objectiveFrame != null)
            {
                return _objectiveFrame;
            }

            _objectiveFrame = GetComponent<M3NamedNpcObjectiveFrame>();
            if (_objectiveFrame != null)
            {
                return _objectiveFrame;
            }

            if (logIfMissing && !_missingFrameLogged)
            {
                Debug.LogError(
                    $"{nameof(M3NamedNpcInteractTarget)} on {name} is missing its {nameof(M3NamedNpcObjectiveFrame)} reference.");
                _missingFrameLogged = true;
            }

            return null;
        }

        private M3ObjectiveStateRelicHandIn? ResolveObjectiveState(bool logIfMissing)
        {
            if (_objectiveState != null)
            {
                return _objectiveState;
            }

            _objectiveState = FindFirstObjectByType<M3ObjectiveStateRelicHandIn>();
            if (_objectiveState != null)
            {
                return _objectiveState;
            }

            if (logIfMissing && !_missingStateLogged)
            {
                Debug.LogWarning(
                    $"{nameof(M3NamedNpcInteractTarget)} on {name} has no {nameof(M3ObjectiveStateRelicHandIn)} reference; falling back to NPC interaction-only routing.");
                _missingStateLogged = true;
            }

            return null;
        }

        private static InteractContext MapToInteractContext(NpcInteractionContext npcContext)
        {
            return new InteractContext(
                TelemetryEvent,
                npcContext.PlayerActorId,
                npcContext.NpcId,
                npcContext.InteractionKind,
                npcContext.InteractionState,
                npcContext.DistanceMeters,
                PayloadKind,
                npcContext.DialogueTemplateSetId,
                npcContext.ObjectiveFrameTextKey,
                npcContext.WasIntentional ? 1 : 0);
        }

        private static InteractContext MapObjectiveTransitionContext(
            string telemetryEvent,
            string playerActorId,
            string npcId,
            string actionLabel,
            M3ObjectiveState fromState,
            M3ObjectiveState toState,
            float distanceMeters)
        {
            return new InteractContext(
                telemetryEvent,
                playerActorId,
                npcId,
                actionLabel,
                toState.ToString(),
                distanceMeters,
                ObjectiveTransitionPayloadKind,
                fromState.ToString(),
                toState.ToString());
        }
    }
}
