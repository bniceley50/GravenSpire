#nullable enable

using Gravenspire.UnityRuntime.Interaction;
using UnityEngine;

namespace Gravenspire.UnityRuntime.Npc
{
    [DisallowMultipleComponent]
    public sealed class M3NamedNpcInteractTarget : MonoBehaviour, IPlayerInteractTarget
    {
        public const string TelemetryEvent = "npc_interaction_intentional";
        public const string PayloadKind = "npc_interaction_context:player_driven";
        public const string SourceAttribution = "player_driven";

        [SerializeField] private M3NamedNpcObjectiveFrame? _objectiveFrame;

        private bool _missingFrameLogged;

        public M3NamedNpcObjectiveFrame? ObjectiveFrame => ResolveObjectiveFrame(logIfMissing: false);

        public int InteractionAttemptCount { get; private set; }

        public int SuccessfulInteractionCount { get; private set; }

        public NpcInteractionContext LastNpcInteractionContext { get; private set; }

        private void Awake()
        {
            ResolveObjectiveFrame(logIfMissing: true);
        }

        public void Configure(M3NamedNpcObjectiveFrame objectiveFrame)
        {
            _objectiveFrame = objectiveFrame;
            _missingFrameLogged = false;
        }

        public bool TryInteract(string playerActorId, float distanceMeters, out InteractContext context)
        {
            context = default;
            InteractionAttemptCount++;

            var objectiveFrame = ResolveObjectiveFrame(logIfMissing: true);
            if (objectiveFrame == null)
            {
                return false;
            }

            var recorded = objectiveFrame.TryRecordIntentionalInteraction(
                playerActorId,
                distanceMeters,
                out var npcContext);
            if (!recorded)
            {
                return false;
            }

            SuccessfulInteractionCount++;
            LastNpcInteractionContext = npcContext;
            context = MapToInteractContext(npcContext);
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
    }
}
