#nullable enable

using System.Collections.Generic;
using Gravenspire.Gameplay.Npc.M3Objective;
using Gravenspire.UnityRuntime.Interaction;
using UnityEngine;

namespace Gravenspire.UnityRuntime.Npc
{
    [DisallowMultipleComponent]
    public sealed class M3RelicInteractTarget : MonoBehaviour, IPlayerInteractTarget, IPlayerInteractTelemetryTarget
    {
        public const string RelicRecoveredTelemetryEvent = "relic_recovered";
        public const string ObjectiveLootResolvedTelemetryEvent = "objective_loot_resolved";
        public const string ObjectiveLootResolutionFailedTelemetryEvent = "objective_loot_resolution_failed";
        public const string RelicPayloadKind = "relic_recovery:player_driven";
        public const string LootPayloadKind = "objective_loot:player_driven";
        public const string SourceAttribution = "player_driven";

        private readonly List<InteractContext> _lastInteractTelemetryEvents = new();

        [SerializeField] private M3ObjectiveStateRelicHandIn? _objectiveState;
        [SerializeField] private M3LootTableFixedProfileVendor? _lootVendor;

        private bool _missingStateLogged;
        private bool _missingVendorLogged;

        public IReadOnlyList<InteractContext> LastInteractTelemetryEvents => _lastInteractTelemetryEvents;

        public M3ObjectiveStateRelicHandIn? ObjectiveState => ResolveObjectiveState(logIfMissing: false);

        public M3LootTableFixedProfileVendor? LootVendor => ResolveLootVendor(logIfMissing: false);

        public int InteractionAttemptCount { get; private set; }

        public int SuccessfulRecoveryCount { get; private set; }

        public bool LastLootResolutionSucceeded { get; private set; }

        public bool LastLootResolutionPartialFailure { get; private set; }

        public string LastLootResolutionFailureReason { get; private set; } = string.Empty;

        private void Awake()
        {
            ResolveObjectiveState(logIfMissing: true);
            ResolveLootVendor(logIfMissing: false);
        }

        public void Configure(M3ObjectiveStateRelicHandIn objectiveState, M3LootTableFixedProfileVendor? lootVendor)
        {
            _objectiveState = objectiveState;
            _lootVendor = lootVendor;
            _missingStateLogged = false;
            _missingVendorLogged = false;
        }

        public bool TryInteract(string playerActorId, float distanceMeters, out InteractContext context)
        {
            context = default;
            InteractionAttemptCount++;
            LastLootResolutionSucceeded = false;
            LastLootResolutionPartialFailure = false;
            LastLootResolutionFailureReason = string.Empty;
            _lastInteractTelemetryEvents.Clear();

            var objectiveState = ResolveObjectiveState(logIfMissing: true);
            if (objectiveState == null)
            {
                return false;
            }

            var fromState = objectiveState.State;
            if (!objectiveState.TryRecoverRelic())
            {
                return false;
            }

            SuccessfulRecoveryCount++;
            context = MapRelicRecoveredContext(playerActorId, gameObject.name, fromState, objectiveState.State, distanceMeters);
            _lastInteractTelemetryEvents.Add(context);

            var lootVendor = ResolveLootVendor(logIfMissing: false);
            if (lootVendor == null)
            {
                RecordLootFailure(playerActorId, distanceMeters, "M3 loot/vendor component is missing.");
                return true;
            }

            if (!lootVendor.TryResolveObjectiveLoot())
            {
                RecordLootFailure(playerActorId, distanceMeters, lootVendor.LastRejectionReason);
                return true;
            }

            LastLootResolutionSucceeded = true;
            _lastInteractTelemetryEvents.Add(MapLootResolvedContext(playerActorId, lootVendor, distanceMeters));
            return true;
        }

        private void RecordLootFailure(string playerActorId, float distanceMeters, string rejectionReason)
        {
            LastLootResolutionPartialFailure = true;
            LastLootResolutionFailureReason = string.IsNullOrWhiteSpace(rejectionReason)
                ? "M3 objective loot resolution failed."
                : rejectionReason;
            _lastInteractTelemetryEvents.Add(MapLootFailureContext(playerActorId, distanceMeters, LastLootResolutionFailureReason));
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
                Debug.LogError(
                    $"{nameof(M3RelicInteractTarget)} on {name} is missing its {nameof(M3ObjectiveStateRelicHandIn)} reference.");
                _missingStateLogged = true;
            }

            return null;
        }

        private M3LootTableFixedProfileVendor? ResolveLootVendor(bool logIfMissing)
        {
            if (_lootVendor != null)
            {
                return _lootVendor;
            }

            _lootVendor = FindFirstObjectByType<M3LootTableFixedProfileVendor>();
            if (_lootVendor != null)
            {
                return _lootVendor;
            }

            if (logIfMissing && !_missingVendorLogged)
            {
                Debug.LogWarning(
                    $"{nameof(M3RelicInteractTarget)} on {name} is missing its {nameof(M3LootTableFixedProfileVendor)} reference.");
                _missingVendorLogged = true;
            }

            return null;
        }

        private static InteractContext MapRelicRecoveredContext(
            string playerActorId,
            string relicObjectName,
            M3ObjectiveState fromState,
            M3ObjectiveState toState,
            float distanceMeters)
        {
            return new InteractContext(
                RelicRecoveredTelemetryEvent,
                playerActorId,
                relicObjectName,
                "relic_recover",
                toState.ToString(),
                distanceMeters,
                RelicPayloadKind,
                M3ObjectiveStateRelicHandInSession.RelicItemId,
                $"{fromState}->{toState}");
        }

        private static InteractContext MapLootResolvedContext(
            string playerActorId,
            M3LootTableFixedProfileVendor lootVendor,
            float distanceMeters)
        {
            return new InteractContext(
                ObjectiveLootResolvedTelemetryEvent,
                playerActorId,
                lootVendor.ConfiguredVendorId,
                "objective_loot_resolve",
                "Resolved",
                distanceMeters,
                LootPayloadKind,
                lootVendor.ConfiguredLootTableId,
                $"{M3ObjectiveStateRelicHandInSession.RelicItemId}|{M3LootTableFixedProfileVendorData.SalvageItemId}");
        }

        private static InteractContext MapLootFailureContext(
            string playerActorId,
            float distanceMeters,
            string rejectionReason)
        {
            return new InteractContext(
                ObjectiveLootResolutionFailedTelemetryEvent,
                playerActorId,
                M3LootTableFixedProfileVendorData.DefaultVendorId,
                "objective_loot_resolve",
                "Failed",
                distanceMeters,
                LootPayloadKind,
                M3LootTableFixedProfileVendorData.DefaultLootTableId,
                rejectionReason);
        }
    }
}
