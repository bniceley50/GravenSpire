#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Gravenspire.Gameplay.Combat;
using UnityEngine;

namespace Gravenspire.UnityRuntime.Combat
{
    public sealed class M2CombatCoreRuntimeBridge : MonoBehaviour
    {
        private const string DefaultFixtureRelativePath = "data/combat/t1-combat-fixtures.json";
        private const string DefaultEncounterFixtureId = "SoloTrash_EvenCon_T1";
        private const string DefaultActiveZoneId = "Haunt_Prototype_T1";
        private const string DefaultPlayerCombatActorId = "m2-player-cleric";
        private const string DefaultPlayerLocalCharacterId = "local-character-m2-dev";
        private const string DefaultHostileCombatActorIdPrefix = "m2-hostile";

        private readonly List<string> _encounterFixtureIds = new();
        private readonly List<string> _actorFixtureIds = new();
        private readonly List<string> _hostileActorIds = new();
        private readonly List<string> _errors = new();

        [SerializeField] private string _fixtureRelativePath = DefaultFixtureRelativePath;
        [SerializeField] private string _encounterFixtureId = DefaultEncounterFixtureId;
        [SerializeField] private string _activeZoneId = DefaultActiveZoneId;

        public bool IsHydrated { get; private set; }

        public string ActiveZoneId { get; private set; } = string.Empty;

        public string FixtureFilePath { get; private set; } = string.Empty;

        public string FixtureSetVersion { get; private set; } = string.Empty;

        public string PlayerActorId { get; private set; } = string.Empty;

        public string ActiveSceneName { get; private set; } = string.Empty;

        public IReadOnlyList<string> EncounterFixtureIds => _encounterFixtureIds;

        public IReadOnlyList<string> ActorFixtureIds => _actorFixtureIds;

        public IReadOnlyList<string> HostileActorIds => _hostileActorIds;

        public IReadOnlyList<string> Errors => _errors;

        private void Awake()
        {
            HydrateBridge();
        }

        public void HydrateBridge()
        {
            _encounterFixtureIds.Clear();
            _actorFixtureIds.Clear();
            _hostileActorIds.Clear();
            _errors.Clear();
            IsHydrated = false;
            ActiveZoneId = string.Empty;
            FixtureFilePath = string.Empty;
            FixtureSetVersion = string.Empty;
            PlayerActorId = string.Empty;
            ActiveSceneName = gameObject.scene.name;

            try
            {
                var fixturePath = ResolveFixturePath(_fixtureRelativePath);
                var request = new CombatRuntimeEncounterHydrationRequest
                {
                    EncounterFixtureId = _encounterFixtureId,
                    ActiveZoneId = _activeZoneId,
                    PlayerCombatActorId = DefaultPlayerCombatActorId,
                    PlayerLocalCharacterId = DefaultPlayerLocalCharacterId,
                    HostileCombatActorIdPrefix = DefaultHostileCombatActorIdPrefix
                };
                var result = new CombatRuntimeEncounterHydrator().HydrateFromFile(fixturePath, request);

                FixtureFilePath = result.FixtureFilePath;
                FixtureSetVersion = result.FixtureSetVersion;
                ActiveZoneId = result.ActiveZoneId;
                AddRange(_encounterFixtureIds, result.EncounterFixtureIds);
                AddRange(_actorFixtureIds, result.ActorFixtureIds);
                AddRange(_hostileActorIds, ExtractHostileActorIds(result));
                AddRange(_errors, result.Errors);

                if (result.PlayerActor is not null)
                {
                    PlayerActorId = result.PlayerActor.CombatActorId;
                }

                IsHydrated = result.Succeeded;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
            {
                _errors.Add(ex.Message);
            }
        }

        private static string ResolveFixturePath(string relativePath)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static IEnumerable<string> ExtractHostileActorIds(CombatRuntimeEncounterHydrationResult result)
        {
            foreach (var hostileActor in result.HostileActors)
            {
                yield return hostileActor.CombatActorId;
            }
        }

        private static void AddRange(List<string> target, IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                target.Add(value);
            }
        }
    }
}
