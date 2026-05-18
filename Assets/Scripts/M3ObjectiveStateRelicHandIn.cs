#nullable enable

using Gravenspire.Gameplay.Npc.M3Objective;
using Gravenspire.UnityRuntime.Npc;
using UnityEngine;

namespace Gravenspire.UnityRuntime.Npc
{
    [DisallowMultipleComponent]
    public sealed class M3ObjectiveStateRelicHandIn : MonoBehaviour
    {
        private M3ObjectiveStateRelicHandInSession _session = new();

        [SerializeField] private GameObject? _relicObject;

        public M3ObjectiveState State => _session.State;

        public bool RelicAvailable => _session.RelicAvailable;

        public bool CarriesRelic => _session.CarriesRelic;

        public bool IsComplete => _session.IsComplete;

        public bool SessionLocalOnly => _session.SessionLocalOnly;

        public int TransitionCount => _session.Transitions.Count;

        public bool RelicObjectConfigured => _relicObject != null;

        public bool RelicObjectActive => _relicObject != null && _relicObject.activeSelf;

        public string LastRejectionReason { get; private set; } = string.Empty;

        public string StateSequence => _session.FormatStateSequence();

        private void Awake()
        {
            ApplyRelicAvailability();
        }

        public void ConfigureForM3ObjectiveStateRelicHandIn(GameObject relicObject)
        {
            _relicObject = relicObject;
            ResetSessionObjective();
        }

        public void ResetSessionObjective()
        {
            _session = new M3ObjectiveStateRelicHandInSession();
            LastRejectionReason = string.Empty;
            ApplyRelicAvailability();
        }

        public bool TryAcceptObjectiveFromNpc(
            M3NamedNpcObjectiveFrame npcFrame,
            string playerActorId,
            float distanceMeters)
        {
            if (npcFrame == null)
            {
                LastRejectionReason = "M3 caretaker frame is missing.";
                return false;
            }

            if (!npcFrame.TryRecordIntentionalInteraction(playerActorId, distanceMeters, out var context))
            {
                LastRejectionReason = "M3 caretaker interaction was not recorded.";
                return false;
            }

            if (!_session.TryAcceptObjective(context.NpcId, context.PlayerActorId, out var rejectionReason))
            {
                LastRejectionReason = rejectionReason;
                return false;
            }

            LastRejectionReason = string.Empty;
            ApplyRelicAvailability();
            return true;
        }

        public bool TryRecoverRelic()
        {
            if (_relicObject == null)
            {
                LastRejectionReason = "M3 relic object is missing.";
                return false;
            }

            if (!_relicObject.activeSelf)
            {
                LastRejectionReason = "M3 relic object is not available.";
                return false;
            }

            if (!_session.TryRecoverRelic(
                    _relicObject.name,
                    M3ObjectiveStateRelicHandInSession.RelicItemId,
                    out var rejectionReason))
            {
                LastRejectionReason = rejectionReason;
                return false;
            }

            LastRejectionReason = string.Empty;
            ApplyRelicAvailability();
            return true;
        }

        public bool TryReturnRelicToNpc(M3NamedNpcObjectiveFrame npcFrame)
        {
            if (npcFrame == null)
            {
                LastRejectionReason = "M3 caretaker frame is missing.";
                return false;
            }

            if (!_session.TryHandInRelic(
                    npcFrame.ConfiguredNpcId,
                    M3ObjectiveStateRelicHandInSession.RelicItemId,
                    out var rejectionReason))
            {
                LastRejectionReason = rejectionReason;
                return false;
            }

            LastRejectionReason = string.Empty;
            ApplyRelicAvailability();
            return true;
        }

        private void ApplyRelicAvailability()
        {
            if (_relicObject != null)
            {
                _relicObject.SetActive(_session.RelicAvailable);
            }
        }
    }
}
