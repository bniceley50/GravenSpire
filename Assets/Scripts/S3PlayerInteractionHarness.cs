#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gravenspire.UnityRuntime.Interaction
{
    public enum InteractFeedbackOutcome
    {
        None,
        Fired,
        Missed,
        Blocked
    }

    public interface IPlayerInteractTarget
    {
        bool TryInteract(string playerActorId, float distanceMeters, out InteractContext context);
    }

    public interface IPlayerInteractTelemetryTarget
    {
        IReadOnlyList<InteractContext> LastInteractTelemetryEvents { get; }
    }

    public readonly struct InteractContext
    {
        public InteractContext(
            string telemetryEvent,
            string playerActorId,
            string targetId,
            string actionLabel,
            string resultLabel,
            float distanceMeters,
            string payloadKind = "",
            string primaryPayload = "",
            string secondaryPayload = "",
            int amount = 0)
        {
            TelemetryEvent = telemetryEvent;
            PlayerActorId = playerActorId;
            TargetId = targetId;
            ActionLabel = actionLabel;
            ResultLabel = resultLabel;
            DistanceMeters = distanceMeters;
            PayloadKind = payloadKind;
            PrimaryPayload = primaryPayload;
            SecondaryPayload = secondaryPayload;
            Amount = amount;
        }

        public string TelemetryEvent { get; }

        public string PlayerActorId { get; }

        public string TargetId { get; }

        public string ActionLabel { get; }

        public string ResultLabel { get; }

        public float DistanceMeters { get; }

        public string PayloadKind { get; }

        public string PrimaryPayload { get; }

        public string SecondaryPayload { get; }

        public int Amount { get; }

        public InteractContext WithFeedbackEvent(
            string telemetryEvent,
            string resultLabel,
            float distanceMeters)
        {
            return new InteractContext(
                telemetryEvent,
                PlayerActorId,
                TargetId,
                ActionLabel,
                resultLabel,
                distanceMeters,
                PayloadKind,
                PrimaryPayload,
                SecondaryPayload,
                Amount);
        }
    }

    /// <summary>
    /// Standalone Sprint 3 player-interaction front-end. It reuses the existing
    /// ClericShellMarker transform and performs a nearest-target distance check
    /// against registered <see cref="IPlayerInteractTarget"/> instances when
    /// the single legacy input verb (<see cref="KeyCode.E"/> by default) fires.
    /// It never moves the player marker and never implements objective, loot,
    /// or vendor state; it only dispatches the player verb and records
    /// fired/missed/blocked acknowledgements.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class S3PlayerInteractionHarness : MonoBehaviour
    {
        public const string HarnessRootName = "S3_PlayerInteractionHarnessRoot";
        public const string ClericMarkerObjectName = "ClericShellMarker";
        public const string DefaultPlayerActorId = "m3-player-cleric";
        public const string FiredTelemetryEvent = "interact_fired";
        public const string MissedTelemetryEvent = "interact_missed";
        public const string BlockedTelemetryEvent = "interact_blocked";
        public const float DefaultInteractRangeMeters = 2.0f;

        private const string DefaultPromptText = "Press E";
        private const string FiredFeedbackText = "Interacted";
        private const string MissedFeedbackText = "Interact missed";
        private const string BlockedFeedbackText = "Interact blocked";
        private const float DefaultFeedbackDurationSeconds = 1.2f;

        private readonly List<RegisteredInteractTarget> _registeredTargets = new();
        private readonly List<InteractContext> _telemetryEvents = new();

        [SerializeField] private Transform? _playerMarker;
        [SerializeField] private string _playerActorId = DefaultPlayerActorId;
        [SerializeField] private KeyCode _interactKey = KeyCode.E;
        [SerializeField] private float _interactRangeMeters = DefaultInteractRangeMeters;
        [SerializeField] private string _promptText = DefaultPromptText;
        [SerializeField] private float _feedbackDurationSeconds = DefaultFeedbackDurationSeconds;
        [SerializeField] private bool _autoDiscoverTargetsOnStart = true;

        private GUIStyle? _promptStyle;
        private GUIStyle? _feedbackStyle;
        private bool _isRefreshingTargetsFromScene;
        private float _feedbackExpiresAtSeconds;

        public Transform? PlayerMarker => _playerMarker;

        public string PlayerActorId => string.IsNullOrWhiteSpace(_playerActorId)
            ? DefaultPlayerActorId
            : _playerActorId;

        public KeyCode InteractKey => _interactKey;

        public float ConfiguredInteractRangeMeters => Mathf.Max(0.1f, _interactRangeMeters);

        public bool PromptVisible { get; private set; }

        public string CurrentPromptText => PromptVisible ? _promptText : string.Empty;

        public InteractFeedbackOutcome LastOutcome { get; private set; } = InteractFeedbackOutcome.None;

        public string LastFeedbackText { get; private set; } = string.Empty;

        public IReadOnlyList<InteractContext> TelemetryEvents => _telemetryEvents;

        public int RegisteredTargetCount => _registeredTargets.Count;

        private void Awake()
        {
            BindPlayerMarkerIfNeeded();
        }

        private void Start()
        {
            if (_autoDiscoverTargetsOnStart)
            {
                RefreshRegisteredTargetsFromScene();
            }

            RefreshPromptState();
        }

        private void Update()
        {
            BindPlayerMarkerIfNeeded();
            RefreshPromptState();

            if (Input.GetKeyDown(_interactKey))
            {
                TryDispatchInteract();
            }

            if (!string.IsNullOrEmpty(LastFeedbackText) && Time.time >= _feedbackExpiresAtSeconds)
            {
                LastFeedbackText = string.Empty;
            }
        }

        private void OnGUI()
        {
            EnsureGuiStyles();

            if (PromptVisible)
            {
                GUI.Label(new Rect(24.0f, 388.0f, 220.0f, 28.0f), _promptText, _promptStyle);
            }

            if (!string.IsNullOrEmpty(LastFeedbackText))
            {
                GUI.Label(new Rect(24.0f, 418.0f, 260.0f, 28.0f), LastFeedbackText, _feedbackStyle);
            }
        }

        public void Configure(
            Transform playerMarker,
            float interactRangeMeters = DefaultInteractRangeMeters,
            string playerActorId = DefaultPlayerActorId)
        {
            _playerMarker = playerMarker;
            _interactRangeMeters = Mathf.Max(0.1f, interactRangeMeters);
            _playerActorId = string.IsNullOrWhiteSpace(playerActorId) ? DefaultPlayerActorId : playerActorId;
            RefreshPromptState();
        }

        public void RegisterTarget(IPlayerInteractTarget target)
        {
            if (target is not Component component)
            {
                throw new ArgumentException(
                    "Scene targets must be Components or must register with an explicit Transform.",
                    nameof(target));
            }

            RegisterTarget(target, component.transform);
        }

        public void RegisterTarget(IPlayerInteractTarget target, Transform targetTransform)
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (targetTransform == null)
            {
                throw new ArgumentNullException(nameof(targetTransform));
            }

            PruneInvalidTargets();
            for (var i = 0; i < _registeredTargets.Count; i++)
            {
                var registered = _registeredTargets[i];
                if (ReferenceEquals(registered.Target, target) || registered.TargetTransform == targetTransform)
                {
                    registered.Target = target;
                    registered.TargetTransform = targetTransform;
                    if (!_isRefreshingTargetsFromScene)
                    {
                        RefreshPromptState();
                    }

                    return;
                }
            }

            _registeredTargets.Add(new RegisteredInteractTarget(target, targetTransform));
            if (!_isRefreshingTargetsFromScene)
            {
                RefreshPromptState();
            }
        }

        public void UnregisterTarget(IPlayerInteractTarget target)
        {
            for (var i = _registeredTargets.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(_registeredTargets[i].Target, target))
                {
                    _registeredTargets.RemoveAt(i);
                }
            }

            RefreshPromptState();
        }

        public void ClearRegisteredTargets()
        {
            _registeredTargets.Clear();
            RefreshPromptState();
        }

        public void RefreshRegisteredTargetsFromScene()
        {
            RefreshRegisteredTargetsFromScene(refreshPromptAfter: true);
        }

        private void RefreshRegisteredTargetsFromScene(bool refreshPromptAfter)
        {
            _isRefreshingTargetsFromScene = true;
            foreach (var behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (behaviour == this || !behaviour.isActiveAndEnabled || behaviour is not IPlayerInteractTarget target)
                {
                    continue;
                }

                RegisterTarget(target, behaviour.transform);
            }

            _isRefreshingTargetsFromScene = false;
            if (refreshPromptAfter)
            {
                RefreshPromptState();
            }
        }

        public void ClearTelemetry()
        {
            _telemetryEvents.Clear();
            LastOutcome = InteractFeedbackOutcome.None;
            LastFeedbackText = string.Empty;
            _feedbackExpiresAtSeconds = 0.0f;
        }

        public bool RefreshPromptState()
        {
            PromptVisible = TryFindNearestTarget(requireWithinRange: true, out _, out _);
            return PromptVisible;
        }

        public bool TryDispatchInteract()
        {
            if (!TryFindNearestTarget(requireWithinRange: true, out var nearest, out var distanceMeters))
            {
                RecordOutcome(
                    InteractFeedbackOutcome.Missed,
                    CreateHarnessContext(MissedTelemetryEvent, string.Empty, MissedFeedbackText, 0.0f),
                    MissedFeedbackText);
                return false;
            }

            if (nearest.Target.TryInteract(PlayerActorId, distanceMeters, out var targetContext))
            {
                var targetTelemetryContexts = ResolveTargetTelemetryContexts(nearest.Target, targetContext);
                var primaryTelemetryContext = NormalizeTargetContext(targetTelemetryContexts[0], nearest, distanceMeters);
                for (var i = 0; i < targetTelemetryContexts.Count; i++)
                {
                    _telemetryEvents.Add(NormalizeTargetContext(targetTelemetryContexts[i], nearest, distanceMeters));
                }

                var firedContext = primaryTelemetryContext
                    .WithFeedbackEvent(FiredTelemetryEvent, NonBlank(targetContext.ResultLabel, FiredFeedbackText), distanceMeters);
                RecordOutcome(InteractFeedbackOutcome.Fired, firedContext, FiredFeedbackText);
                return true;
            }

            RecordOutcome(
                InteractFeedbackOutcome.Blocked,
                CreateHarnessContext(BlockedTelemetryEvent, TargetName(nearest), BlockedFeedbackText, distanceMeters),
                BlockedFeedbackText);
            return false;
        }

        private void BindPlayerMarkerIfNeeded()
        {
            if (_playerMarker != null)
            {
                return;
            }

            var markerObject = GameObject.Find(ClericMarkerObjectName);
            if (markerObject != null)
            {
                _playerMarker = markerObject.transform;
            }
        }

        private bool TryFindNearestTarget(
            bool requireWithinRange,
            out RegisteredInteractTarget nearest,
            out float distanceMeters)
        {
            nearest = default!;
            distanceMeters = 0.0f;

            BindPlayerMarkerIfNeeded();
            if (_playerMarker == null)
            {
                return false;
            }

            if (_autoDiscoverTargetsOnStart)
            {
                RefreshRegisteredTargetsFromScene(refreshPromptAfter: false);
            }

            PruneInvalidTargets();
            var nearestDistance = float.MaxValue;
            var found = false;
            var playerPosition = _playerMarker.position;
            for (var i = 0; i < _registeredTargets.Count; i++)
            {
                var registered = _registeredTargets[i];
                var candidateDistance = Vector3.Distance(playerPosition, registered.TargetTransform.position);
                if (requireWithinRange && candidateDistance > ConfiguredInteractRangeMeters)
                {
                    continue;
                }

                if (candidateDistance >= nearestDistance)
                {
                    continue;
                }

                nearest = registered;
                nearestDistance = candidateDistance;
                found = true;
            }

            if (!found)
            {
                return false;
            }

            distanceMeters = nearestDistance;
            return true;
        }

        private static IReadOnlyList<InteractContext> ResolveTargetTelemetryContexts(
            IPlayerInteractTarget target,
            InteractContext fallbackContext)
        {
            if (target is IPlayerInteractTelemetryTarget telemetryTarget &&
                telemetryTarget.LastInteractTelemetryEvents.Count > 0)
            {
                return telemetryTarget.LastInteractTelemetryEvents;
            }

            return new[] { fallbackContext };
        }

        private void PruneInvalidTargets()
        {
            for (var i = _registeredTargets.Count - 1; i >= 0; i--)
            {
                var registered = _registeredTargets[i];
                if (registered.TargetTransform == null ||
                    registered.Target is Component component &&
                    (component == null ||
                        !component.gameObject.activeInHierarchy ||
                        component is Behaviour behaviour && !behaviour.isActiveAndEnabled))
                {
                    _registeredTargets.RemoveAt(i);
                }
            }
        }

        private InteractContext NormalizeTargetContext(
            InteractContext context,
            RegisteredInteractTarget target,
            float distanceMeters)
        {
            return new InteractContext(
                NonBlank(context.TelemetryEvent, FiredTelemetryEvent),
                NonBlank(context.PlayerActorId, PlayerActorId),
                NonBlank(context.TargetId, TargetName(target)),
                NonBlank(context.ActionLabel, "interacted"),
                NonBlank(context.ResultLabel, FiredFeedbackText),
                distanceMeters,
                NonBlank(context.PayloadKind, string.Empty),
                NonBlank(context.PrimaryPayload, string.Empty),
                NonBlank(context.SecondaryPayload, string.Empty),
                context.Amount);
        }

        private InteractContext CreateHarnessContext(
            string telemetryEvent,
            string targetId,
            string resultLabel,
            float distanceMeters)
        {
            return new InteractContext(
                telemetryEvent,
                PlayerActorId,
                targetId,
                "interacted",
                resultLabel,
                distanceMeters);
        }

        private void RecordOutcome(
            InteractFeedbackOutcome outcome,
            InteractContext context,
            string feedbackText)
        {
            LastOutcome = outcome;
            LastFeedbackText = feedbackText;
            _feedbackExpiresAtSeconds = Time.time + Mathf.Max(0.1f, _feedbackDurationSeconds);
            _telemetryEvents.Add(context);
        }

        private void EnsureGuiStyles()
        {
            if (_promptStyle is not null && _feedbackStyle is not null)
            {
                return;
            }

            _promptStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
            _feedbackStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                wordWrap = false
            };
        }

        private static string TargetName(RegisteredInteractTarget target)
        {
            return target.TargetTransform == null ? string.Empty : target.TargetTransform.name;
        }

        private static string NonBlank(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private sealed class RegisteredInteractTarget
        {
            public RegisteredInteractTarget(IPlayerInteractTarget target, Transform targetTransform)
            {
                Target = target;
                TargetTransform = targetTransform;
            }

            public IPlayerInteractTarget Target { get; set; }

            public Transform TargetTransform { get; set; }
        }
    }
}
