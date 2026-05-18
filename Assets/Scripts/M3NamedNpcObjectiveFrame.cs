#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gravenspire.UnityRuntime.Npc
{
    public readonly struct NpcInteractionContext
    {
        public NpcInteractionContext(
            string npcId,
            string playerActorId,
            string activeZoneId,
            string interactionState,
            string interactionKind,
            string dialogueTemplateSetId,
            string objectiveFrameTextKey,
            bool wasIntentional,
            float distanceMeters)
        {
            NpcId = npcId;
            PlayerActorId = playerActorId;
            ActiveZoneId = activeZoneId;
            InteractionState = interactionState;
            InteractionKind = interactionKind;
            DialogueTemplateSetId = dialogueTemplateSetId;
            ObjectiveFrameTextKey = objectiveFrameTextKey;
            WasIntentional = wasIntentional;
            DistanceMeters = distanceMeters;
        }

        public string NpcId { get; }

        public string PlayerActorId { get; }

        public string ActiveZoneId { get; }

        public string InteractionState { get; }

        public string InteractionKind { get; }

        public string DialogueTemplateSetId { get; }

        public string ObjectiveFrameTextKey { get; }

        public bool WasIntentional { get; }

        public float DistanceMeters { get; }
    }

    [DisallowMultipleComponent]
    public sealed class M3NamedNpcObjectiveFrame : MonoBehaviour
    {
        public const string AnchorObjectName = "M3_Caretaker";
        public const string NpcId = "M3_Caretaker_T1";
        public const string PlayerActorId = "m3-player-cleric";
        public const string ActiveZoneId = "Haunt_Prototype_T1";
        public const string DialogueTemplateSetId = "dialogue.m3.caretaker.objective_frame_t1";
        public const string ObjectiveFrameTextKey = "m3.objective.recover_marked_relic.frame";
        public const string InteractionState = "Interacting";
        public const string InteractionKind = "IntentionalPlayerInteraction";
        public const float InteractionRangeMeters = 2.0f;

        private readonly List<NpcInteractionContext> _recordedInteractions = new();

        [SerializeField] private string _npcId = NpcId;
        [SerializeField] private string _activeZoneId = ActiveZoneId;
        [SerializeField] private string _dialogueTemplateSetId = DialogueTemplateSetId;
        [SerializeField] private string _objectiveFrameTextKey = ObjectiveFrameTextKey;
        [SerializeField] private float _interactionRangeMeters = InteractionRangeMeters;

        public event Action<NpcInteractionContext>? NpcInteractionRecorded;

        public string ConfiguredNpcId => _npcId;

        public string ConfiguredActiveZoneId => _activeZoneId;

        public string ConfiguredDialogueTemplateSetId => _dialogueTemplateSetId;

        public string ConfiguredObjectiveFrameTextKey => _objectiveFrameTextKey;

        public float ConfiguredInteractionRangeMeters => Mathf.Max(0.1f, _interactionRangeMeters);

        public bool SessionLocalOnly => true;

        public bool UsesTemplatedDialogueOnly =>
            !string.IsNullOrWhiteSpace(_dialogueTemplateSetId) &&
            !string.IsNullOrWhiteSpace(_objectiveFrameTextKey);

        public IReadOnlyList<NpcInteractionContext> RecordedInteractions => _recordedInteractions;

        public bool HasRecordedInteraction => _recordedInteractions.Count > 0;

        public NpcInteractionContext LastInteraction =>
            _recordedInteractions.Count == 0 ? default : _recordedInteractions[^1];

        public void ConfigureForM3ObjectiveFrame()
        {
            _npcId = NpcId;
            _activeZoneId = ActiveZoneId;
            _dialogueTemplateSetId = DialogueTemplateSetId;
            _objectiveFrameTextKey = ObjectiveFrameTextKey;
            _interactionRangeMeters = InteractionRangeMeters;
        }

        public void ClearSessionInteractions()
        {
            _recordedInteractions.Clear();
        }

        public bool TryRecordIntentionalInteraction(
            string playerActorId,
            float distanceMeters,
            out NpcInteractionContext context)
        {
            context = default;

            if (string.IsNullOrWhiteSpace(playerActorId))
            {
                return false;
            }

            if (distanceMeters > ConfiguredInteractionRangeMeters)
            {
                return false;
            }

            context = new NpcInteractionContext(
                _npcId,
                playerActorId,
                _activeZoneId,
                InteractionState,
                InteractionKind,
                _dialogueTemplateSetId,
                _objectiveFrameTextKey,
                wasIntentional: true,
                distanceMeters);
            _recordedInteractions.Add(context);
            NpcInteractionRecorded?.Invoke(context);
            return true;
        }
    }
}
