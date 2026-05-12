#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Combat.Fixtures;
using UnityEngine;

namespace Gravenspire.UnityRuntime.Combat
{
    public sealed class M2SingleTrashMedLoopController : MonoBehaviour
    {
        private const string FixtureRelativePath = "data/combat/t1-combat-fixtures.json";
        private const string EncounterFixtureId = "SoloTrash_EvenCon_T1";
        private const string ActiveZoneId = "Haunt_Prototype_T1";
        private const string PlayerActorId = "m2-player-cleric";
        private const string PlayerLocalCharacterId = "local-character-m2-dev";
        private const string HostileActorPrefix = "m2-hostile";
        private const string SmiteAbilityId = "SmiteOfAuthority_T1_Prototype";
        private const string FixtureBand = "Mid";
        private const float PullAggroRadiusMeters = 4.0f;
        private const float M2MeleeRangeMeters = 1.5f;
        private const float HostileMoveSpeedMeters = 2.2f;
        private const float PlayerMoveSpeedMeters = 4.0f;
        private const float CameraFollowHeight = 7.0f;
        private const float CameraFollowBack = 8.0f;
        private const int MaxSmokeTicksPerPull = 5000;

        private readonly List<string> _events = new();
        private readonly List<string> _errors = new();

        private Transform? _playerMarker;
        private Transform? _baselineTrash;
        private Transform? _campRestPoint;
        private Transform? _pullLane;
        private Transform? _floor;
        private Camera? _camera;
        private GUIStyle? _labelStyle;
        private GUIStyle? _titleStyle;
        private GUIStyle? _buttonStyle;
        private GUIStyle? _statusStyle;

        private CombatFixturePackage? _fixturePackage;
        private CombatTacticalAbilityProfile? _smiteProfile;
        private CombatActorState? _player;
        private CombatActorState? _hostile;
        private CombatZoneGate? _zoneGate;
        private FixedCombatClock? _clock;
        private CombatAttackStateSnapshot _playerAttackState = new(CombatAttackMode.Off, null, null, null);
        private CombatAttackStateSnapshot _hostileAttackState = new(CombatAttackMode.Off, null, null, null);
        private readonly CombatMeleeResolver _meleeResolver = new();
        private readonly CombatRegenResolver _regenResolver = new();
        private readonly CombatPostureStateMachine _postureStateMachine = new();
        private readonly CombatInstantAbilityResolver _instantAbilityResolver = new();
        private readonly LoopingMeleeRandomSource _playerMeleeRandom = new(0.12d, 1.0d);
        private readonly LoopingMeleeRandomSource _hostileMeleeRandom = new(0.38d, 0.82d);

        private double _tickAccumulatorSeconds;
        private bool _pullActive;
        private bool _targetSelected;
        private bool _pullStartedBeforeAttack;
        private bool _pullDidNotAutoEnableAttack;
        private bool _sitMedStarted;
        private bool _combatExitRecorded;
        private bool _smokeRunning;
        private int _pullsCompleted;
        private int _manaRestoredTotal;
        private long? _smiteCooldownEndsTick;
        private string _lastStatus = "Ready: approach the baseline trash to body-pull.";

        public bool IsInitialized { get; private set; }

        public bool PullStarted => _pullStartedBeforeAttack;

        public bool PullDidNotAutoEnableAttack => _pullDidNotAutoEnableAttack;

        public bool AttackTransitionRecorded { get; private set; }

        public bool HostileDefeatRecorded { get; private set; }

        public bool CombatExitRecorded => _combatExitRecorded;

        public bool SitMedStartRecorded => _sitMedStarted;

        public bool ManaRestorationRecorded => _manaRestoredTotal > 0;

        public bool TwoPullLoopComplete => _pullsCompleted >= 2;

        public int PullsCompleted => _pullsCompleted;

        public int ManaRestoredTotal => _manaRestoredTotal;

        public IReadOnlyList<string> Events => _events;

        public IReadOnlyList<string> Errors => _errors;

        private void Awake()
        {
            BindSceneObjects();
            InitializeLoop();
        }

        private void Update()
        {
            if (!IsInitialized || _smokeRunning)
            {
                return;
            }

            HandlePlayerMovement();
            HandleInput();
            TryStartBodyPull();
            MoveHostileTowardPlayer();
            AdvanceFixedTime(Time.deltaTime);
            ApplySceneVisualState();
        }

        private void OnGUI()
        {
            if (!IsInitialized)
            {
                return;
            }

            var previousMatrix = GUI.matrix;
            var portraitSimulatorScale = Screen.height > Screen.width * 1.35f ? 2.7f : 1.0f;
            var landscapeEditorScale = Screen.width > Screen.height ? Screen.width / 1000.0f : 1.0f;
            var hudScale = Mathf.Clamp(Mathf.Max(Mathf.Max(Screen.height / 900.0f, portraitSimulatorScale), landscapeEditorScale), 1.0f, 3.2f);
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(hudScale, hudScale, 1.0f));
            EnsureHudStyles();

            var player = _player;
            var hostile = _hostile;
            GUI.Box(new Rect(12, 12, 650, 364), string.Empty);
            GUI.Label(new Rect(24, 24, 520, 24), "S2-M2-02 Single Trash Pull + Med Loop", _titleStyle);
            GUI.Label(new Rect(24, 54, 604, 48), _lastStatus, _statusStyle);
            if (player is not null)
            {
                GUI.Label(new Rect(24, 108, 220, 24), "Cleric", _labelStyle);
                DrawBar(new Rect(90, 112, 220, 12), player.CurrentHealth, player.MaxHealth, new Color(0.55f, 0.05f, 0.05f));
                DrawBar(new Rect(90, 130, 220, 12), player.CurrentMana, player.MaxMana, new Color(0.05f, 0.20f, 0.75f));
                GUI.Label(new Rect(326, 108, 300, 24), $"HP {player.CurrentHealth}/{player.MaxHealth}   Mana {player.CurrentMana}/{player.MaxMana}", _labelStyle);
                GUI.Label(new Rect(24, 154, 604, 24), $"Posture {player.PostureState}   Attack {(_playerAttackState.IsAttackOn ? "ON" : "OFF")}   Pulls {_pullsCompleted}/2   Smite {SmiteHudText()}", _labelStyle);
            }

            if (hostile is not null)
            {
                GUI.Label(new Rect(24, 186, 220, 24), "Trash", _labelStyle);
                DrawBar(new Rect(90, 190, 220, 12), hostile.CurrentHealth, hostile.MaxHealth, new Color(0.65f, 0.08f, 0.08f));
                GUI.Label(new Rect(326, 182, 300, 24), $"HP {hostile.CurrentHealth}/{hostile.MaxHealth}   State {hostile.CombatState}", _labelStyle);
            }

            GUI.Label(new Rect(24, 226, 604, 24), "Use the buttons. Auto-attack keeps ticking after Attack is ON.", _labelStyle);
            GUI.Label(new Rect(24, 250, 604, 24), "When Smite shows cooldown, wait for it to light up instead of clicking through rejection text.", _labelStyle);
            var hostileAlive = hostile is not null && hostile.IsAlive;
            var playerSitting = player is not null && player.PostureState == CombatPostureState.Sitting;
            var canPull = !_pullActive && hostileAlive && _pullsCompleted < 2 && !playerSitting;
            var canTarget = _pullActive && hostileAlive && !_targetSelected;
            var canAttack = _pullActive && hostileAlive && _targetSelected && !_playerAttackState.IsAttackOn;
            var canSmite = _pullActive && hostileAlive && _targetSelected && IsSmiteReady();
            var canSitStand = playerSitting || (!_pullActive && !hostileAlive);

            DrawHudButton(new Rect(24, 292, 100, 34), "Pull V", ApproachAndPull, canPull);
            DrawHudButton(new Rect(134, 292, 112, 34), "Target Tab", SelectBaselineTarget, canTarget);
            DrawHudButton(new Rect(256, 292, 104, 34), "Attack F", ToggleAttack, canAttack);
            DrawHudButton(new Rect(370, 292, 112, 34), SmiteButtonText(), UseSmite, canSmite);
            DrawHudButton(new Rect(492, 292, 132, 34), "Sit/Stand X", ToggleSitStand, canSitStand);
            DrawHudButton(new Rect(24, 334, 100, 34), "Reset R", ResetLoop, enabled: true);
            GUI.matrix = previousMatrix;
        }

        private void EnsureHudStyles()
        {
            if (_labelStyle is not null && _titleStyle is not null && _buttonStyle is not null && _statusStyle is not null)
            {
                return;
            }

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                wordWrap = false
            };
            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            _statusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                wordWrap = true
            };
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15
            };
        }

        private void DrawHudButton(Rect rect, string label, Action action, bool enabled)
        {
            var previousEnabled = GUI.enabled;
            GUI.enabled = enabled;
            if (GUI.Button(rect, label, _buttonStyle))
            {
                action();
            }

            GUI.enabled = previousEnabled;
        }

        private static void DrawBar(Rect rect, int current, int max, Color fill)
        {
            GUI.Box(rect, string.Empty);
            if (max <= 0)
            {
                return;
            }

            var previousColor = GUI.color;
            GUI.color = fill;
            var filled = rect;
            filled.width = Mathf.Max(0.0f, rect.width * Mathf.Clamp01(current / (float)max));
            GUI.DrawTexture(filled, Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        public bool RunAutomatedTwoPullSmoke()
        {
            _smokeRunning = true;
            try
            {
                ResetLoop();
                for (var pull = 0; pull < 2; pull++)
                {
                    MovePlayerToMeleeRange();
                    if (!TryStartBodyPull())
                    {
                        AddError($"pull_{pull + 1}: body pull did not start.");
                        return false;
                    }

                    SelectBaselineTarget();
                    ToggleAttack();
                    UseSmite();
                    ResolveCombatUntilHostileDefeated();
                    if (!HostileDefeatRecorded)
                    {
                        AddError($"pull_{pull + 1}: hostile was not defeated.");
                        return false;
                    }

                    Sit();
                    AdvanceFixedTicks(400);
                    Stand();

                    if (pull == 0)
                    {
                        if (_hostile is null || !_hostile.IsAlive)
                        {
                            RespawnBaselineTrash();
                        }
                    }
                }

                return PullStarted &&
                    PullDidNotAutoEnableAttack &&
                    AttackTransitionRecorded &&
                    HostileDefeatRecorded &&
                    CombatExitRecorded &&
                    SitMedStartRecorded &&
                    ManaRestorationRecorded &&
                    TwoPullLoopComplete &&
                    Errors.Count == 0;
            }
            finally
            {
                _smokeRunning = false;
                ApplySceneVisualState();
            }
        }

        public void ResetLoop()
        {
            _events.Clear();
            _errors.Clear();
            _pullsCompleted = 0;
            _manaRestoredTotal = 0;
            _tickAccumulatorSeconds = 0d;
            _pullStartedBeforeAttack = false;
            _pullDidNotAutoEnableAttack = false;
            AttackTransitionRecorded = false;
            HostileDefeatRecorded = false;
            _combatExitRecorded = false;
            _sitMedStarted = false;
            _smiteCooldownEndsTick = null;
            _pullActive = false;
            _targetSelected = false;
            _playerAttackState = new CombatAttackStateSnapshot(CombatAttackMode.Off, null, null, null);
            _hostileAttackState = new CombatAttackStateSnapshot(CombatAttackMode.Off, null, null, null);
            InitializeLoop();
            RecordEvent("loop_reset");
        }

        private void InitializeLoop()
        {
            try
            {
                BindSceneObjects();
                _fixturePackage = new CombatFixtureLoader().LoadFromFile(ResolveFixturePath(FixtureRelativePath));
                _smiteProfile = LoadSmiteProfile(_fixturePackage);
                var hydration = new CombatRuntimeEncounterHydrator().HydrateFromFile(
                    ResolveFixturePath(FixtureRelativePath),
                    new CombatRuntimeEncounterHydrationRequest
                    {
                        EncounterFixtureId = EncounterFixtureId,
                        ActiveZoneId = ActiveZoneId,
                        PlayerCombatActorId = PlayerActorId,
                        PlayerLocalCharacterId = PlayerLocalCharacterId,
                        HostileCombatActorIdPrefix = HostileActorPrefix
                    });

                if (!hydration.Succeeded || hydration.PlayerActor is null || hydration.HostileActors.Count == 0)
                {
                    AddError("Runtime encounter hydration failed: " + string.Join("; ", hydration.Errors));
                    return;
                }

                _player = hydration.PlayerActor;
                _hostile = hydration.HostileActors[0];
                _zoneGate = new CombatZoneGate();
                _zoneGate.ActivateZone(ActiveZoneId, CombatZoneType.HauntZone);
                _clock = new FixedCombatClock(_fixturePackage.CombatTickRateHz);
                ApplyPresentationSettings();
                PositionMarkersForFreshLoop();
                IsInitialized = true;
                _lastStatus = "Ready: approach trash, then Tab target and F Attack.";
                RecordEvent("loop_initialized");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or InvalidOperationException)
            {
                AddError(ex.Message);
            }
        }

        private void HandlePlayerMovement()
        {
            var playerMarker = _playerMarker;
            if (playerMarker is null || _player is null || _player.PostureState == CombatPostureState.Sitting)
            {
                return;
            }

            var input = new Vector3(
                Axis(KeyCode.D, KeyCode.A),
                0.0f,
                Axis(KeyCode.W, KeyCode.S));
            if (input.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            playerMarker.position += input.normalized * PlayerMoveSpeedMeters * Time.deltaTime;
            FollowCamera();
        }

        private static float Axis(KeyCode positive, KeyCode negative)
        {
            var value = 0.0f;
            if (Input.GetKey(positive))
            {
                value += 1.0f;
            }

            if (Input.GetKey(negative))
            {
                value -= 1.0f;
            }

            return value;
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                ApproachAndPull();
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SelectBaselineTarget();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleAttack();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UseSmite();
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                ToggleSitStand();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetLoop();
            }
        }

        private void ApproachAndPull()
        {
            if (_pullsCompleted >= 2)
            {
                _lastStatus = "Two-pull loop complete. Press R to reset.";
                return;
            }

            if (_hostile is null || !_hostile.IsAlive)
            {
                _lastStatus = "Trash is down. Sit, wait for mana, then stand to reset the next pull.";
                return;
            }

            MovePlayerToMeleeRange();
            FollowCamera();
            if (!TryStartBodyPull())
            {
                _lastStatus = "Move closer to the red trash or press Pull again.";
            }
        }

        private void ToggleSitStand()
        {
            if (_player is not null && _player.PostureState == CombatPostureState.Sitting)
            {
                Stand();
            }
            else
            {
                Sit();
            }
        }

        private bool TryStartBodyPull()
        {
            if (_pullActive || _player is null || _hostile is null || _zoneGate is null || !_hostile.IsAlive)
            {
                return false;
            }

            var playerMarker = _playerMarker;
            var hostileMarker = _baselineTrash;
            if (playerMarker is null || hostileMarker is null)
            {
                AddError("Scene markers are not bound.");
                return false;
            }

            if (Vector3.Distance(playerMarker.position, hostileMarker.position) > PullAggroRadiusMeters)
            {
                return false;
            }

            var playerPoint = ToCombatPoint(playerMarker.position);
            var hostilePoint = ToCombatPoint(hostileMarker.position);
            var result = new CombatPullCoordinator().ResolveBodyPull(
                _player,
                playerPoint,
                new CombatPullCandidate(
                    _hostile,
                    hostilePoint,
                    new CombatSpatialAnchorSet(hostilePoint, playerPoint, "M2_BaselineTrash", "ClericShellMarker"),
                    PullAggroRadiusMeters,
                    CombatSocialAssistProfile.T1Default("m2-solo-trash"),
                    Array.Empty<CombatLosLayer>(),
                    Array.Empty<CombatLosLayer>(),
                    AuthoredColliderIndex: 0),
                Array.Empty<CombatPullCandidate>(),
                _zoneGate,
                CurrentTick());

            if (!result.Succeeded || result.PrimaryHostile is null)
            {
                AddError("Body pull failed: " + string.Join("; ", result.Errors));
                return false;
            }

            _pullActive = true;
            _pullStartedBeforeAttack = true;
            _pullDidNotAutoEnableAttack = !_playerAttackState.IsAttackOn && !result.PlayerAttackEnabled;
            _player = _player.WithCombatState(CombatState.InCombat);
            _hostile = result.PrimaryHostile.WithCombatState(CombatState.InCombat);
            _hostileAttackState = new CombatAttackStateSnapshot(
                CombatAttackMode.On,
                _player.CombatActorId,
                NextWeaponTick(_hostile),
                CombatAttackTransitionPath.PlayerToggleOn);
            _lastStatus = "Body pull started. Attack is still OFF until you press F.";
            RecordEvent("pull_start");
            return true;
        }

        private void SelectBaselineTarget()
        {
            if (_player is null || _hostile is null || !_hostile.IsAlive)
            {
                return;
            }

            _player = _player.WithTarget(_hostile.CombatActorId);
            _targetSelected = true;
            _lastStatus = "Baseline trash targeted. Press F to toggle Attack.";
            RecordEvent("target_selected");
        }

        private void ToggleAttack()
        {
            if (_player is null || _hostile is null || _zoneGate is null)
            {
                return;
            }

            if (_playerAttackState.IsAttackOn)
            {
                ForceAttackOff(CombatAttackTransitionPath.PlayerToggleOff, "attack_off");
                return;
            }

            if (!_targetSelected)
            {
                SelectBaselineTarget();
            }

            var machine = new CombatAttackStateMachine();
            var result = machine.ToggleOn(new CombatAttackToggleOnRequest(
                _player,
                _hostile,
                _zoneGate,
                DistanceToHostile(),
                CurrentTick(),
                TickRateHz));

            if (!result.Succeeded)
            {
                _lastStatus = "Attack rejected: " + string.Join("; ", result.RejectionReasons);
                return;
            }

            _playerAttackState = result.Snapshot;
            AttackTransitionRecorded = true;
            _lastStatus = "Attack ON. Stay in melee range; press 1 for Smite.";
            RecordEvent("attack_on");
        }

        private void UseSmite()
        {
            if (_player is null || _hostile is null || _zoneGate is null || _smiteProfile is null || !_hostile.IsAlive)
            {
                return;
            }

            if (!_targetSelected)
            {
                SelectBaselineTarget();
            }

            if (!IsSmiteReady())
            {
                _lastStatus = "Smite cooling down. Auto-attack is still ticking; wait for the Smite button to light up.";
                return;
            }

            var result = _instantAbilityResolver.Resolve(new CombatInstantAbilityRequest(
                $"m2-smite-{CurrentTick().Index}",
                _player,
                _hostile,
                _zoneGate,
                DistanceToHostile(),
                Array.Empty<CombatLosLayer>(),
                CurrentTick(),
                TickRateHz,
                _smiteProfile));

            if (!result.Succeeded)
            {
                _lastStatus = result.Outcome == CombatInstantAbilityOutcome.OnCooldown
                    ? "Smite cooling down. Auto-attack is still ticking; wait for the Smite button to light up."
                    : "Smite rejected: " + string.Join("; ", result.RejectionReasons);
                if (result.CooldownEndsTick is not null)
                {
                    _smiteCooldownEndsTick = result.CooldownEndsTick;
                }

                return;
            }

            _player = result.Caster;
            _smiteCooldownEndsTick = result.CooldownEndsTick;
            if (result.TargetAfterResolution is not null)
            {
                _hostile = result.TargetAfterResolution;
                UpdateHostileScale();
            }

            _lastStatus = "Smite resolved.";
            RecordEvent("smite_resolved");
            CheckHostileDefeat();
        }

        private bool IsSmiteReady()
        {
            return _smiteCooldownEndsTick is null || CurrentTick().Index >= _smiteCooldownEndsTick.Value;
        }

        private string SmiteButtonText()
        {
            return IsSmiteReady() ? "Smite 1" : $"Smite {SmiteCooldownRemainingSeconds():0.0}s";
        }

        private string SmiteHudText()
        {
            return IsSmiteReady() ? "ready" : $"cooldown {SmiteCooldownRemainingSeconds():0.0}s";
        }

        private float SmiteCooldownRemainingSeconds()
        {
            if (_smiteCooldownEndsTick is null || _fixturePackage is null)
            {
                return 0.0f;
            }

            var remainingTicks = Math.Max(0, _smiteCooldownEndsTick.Value - CurrentTick().Index);
            return remainingTicks / (float)_fixturePackage.CombatTickRateHz;
        }

        private void Sit()
        {
            if (_player is null || _clock is null)
            {
                return;
            }

            if (_pullActive && _hostile is not null && _hostile.IsAlive)
            {
                _lastStatus = "Defeat the trash before sitting. Med break starts after combat exit.";
                RecordEvent("sit_blocked_active_combat");
                return;
            }

            var result = _postureStateMachine.TrySit(new CombatSitRequest(
                _player,
                new CombatAttackStateMachine(),
                CurrentTick(),
                IsGrounded: true,
                IsMoving: false,
                IsZoneLoadingCommitLocked: false));
            if (!result.Succeeded)
            {
                _lastStatus = "Sit rejected: " + string.Join("; ", result.RejectionReasons);
                return;
            }

            _player = result.Player.WithCombatState(_player.CombatState);
            ForceAttackOff(CombatAttackTransitionPath.SuccessfulSitOrMed, "attack_off_sit");
            _sitMedStarted = true;
            _lastStatus = "Sitting. Mana restores on Combat Core regen ticks.";
            RecordEvent("sit_med_start");
        }

        private void Stand()
        {
            if (_player is null)
            {
                return;
            }

            _player = _player with { PostureState = CombatPostureState.Standing };
            RecordEvent("stand");
            if (_pullsCompleted == 1 && (_hostile is null || !_hostile.IsAlive))
            {
                RespawnBaselineTrash();
                _lastStatus = "Standing. Baseline trash reset; pull one more.";
                RecordEvent("manual_repeat_ready");
                return;
            }

            _lastStatus = _pullsCompleted < 2
                ? "Standing. Pull again when ready."
                : "Two-pull loop complete.";
        }

        private void MoveHostileTowardPlayer()
        {
            if (!_pullActive || _baselineTrash is null || _playerMarker is null || _hostile is null || !_hostile.IsAlive)
            {
                return;
            }

            if (Vector3.Distance(_baselineTrash.position, _playerMarker.position) <= M2MeleeRangeMeters)
            {
                return;
            }

            _baselineTrash.position = Vector3.MoveTowards(
                _baselineTrash.position,
                _playerMarker.position,
                HostileMoveSpeedMeters * Time.deltaTime);
        }

        private void AdvanceFixedTime(float deltaSeconds)
        {
            if (_clock is null || _fixturePackage is null || deltaSeconds <= 0.0f)
            {
                return;
            }

            _tickAccumulatorSeconds += deltaSeconds;
            var tickBudget = (int)Math.Floor(_tickAccumulatorSeconds * _clock.TickRateHz);
            if (tickBudget <= 0)
            {
                return;
            }

            _tickAccumulatorSeconds -= tickBudget * _clock.TickDurationSeconds;
            AdvanceFixedTicks(tickBudget);
        }

        private void AdvanceFixedTicks(int tickBudget)
        {
            if (_clock is null || _fixturePackage is null)
            {
                return;
            }

            for (var i = 0; i < tickBudget; i++)
            {
                var tick = _clock.AdvanceTicks(1);
                ResolveMelee(tick);
                ResolveRegen(tick);
                if (_player is not null && !_player.IsAlive)
                {
                    AddError("Player died during S2-M2-02 loop.");
                    return;
                }
            }
        }

        private void ResolveMelee(CombatTick tick)
        {
            if (_player is null || _hostile is null || _zoneGate is null || !_pullActive || !_hostile.IsAlive)
            {
                return;
            }

            if (_playerAttackState.IsAttackOn)
            {
                var playerResult = _meleeResolver.ResolveTick(MeleeRequest(
                    _player,
                    _hostile,
                    _playerAttackState,
                    tick,
                    _playerMeleeRandom));
                if (playerResult.Outcome != CombatMeleeTickOutcome.NotDue)
                {
                    _playerAttackState = _playerAttackState with { NextSwingDueTick = playerResult.NextSwingDueTick };
                    if (playerResult.TargetAfterResolution is not null)
                    {
                        _hostile = playerResult.TargetAfterResolution;
                    }

                    if (playerResult.AppliedDamage)
                    {
                        RecordEvent($"player_melee_hit:{playerResult.Damage}");
                    }
                }
            }

            if (_hostile.IsAlive)
            {
                var hostileResult = _meleeResolver.ResolveTick(MeleeRequest(
                    _hostile,
                    _player,
                    _hostileAttackState,
                    tick,
                    _hostileMeleeRandom));
                if (hostileResult.Outcome != CombatMeleeTickOutcome.NotDue)
                {
                    _hostileAttackState = _hostileAttackState with { NextSwingDueTick = hostileResult.NextSwingDueTick };
                    if (hostileResult.TargetAfterResolution is not null)
                    {
                        _player = hostileResult.TargetAfterResolution;
                    }

                    if (hostileResult.AppliedDamage)
                    {
                        RecordEvent($"hostile_melee_hit:{hostileResult.Damage}");
                    }
                }
            }

            UpdateHostileScale();
            CheckHostileDefeat();
        }

        private CombatMeleeTickRequest MeleeRequest(
            CombatActorState attacker,
            CombatActorState target,
            CombatAttackStateSnapshot attackState,
            CombatTick tick,
            ICombatMeleeRandomSource random)
        {
            return new CombatMeleeTickRequest(
                attacker,
                target,
                attackState,
                _zoneGate!,
                DistanceToHostile(),
                FacingDegreesToTarget: 0.0d,
                FacingToleranceDegrees: 90.0d,
                Array.Empty<CombatLosLayer>(),
                tick,
                TickRateHz,
                new CombatMeleeHitChanceTuning(0.72d, 0.03d, 0.001d, 0.10d, 0.92d),
                new CombatMeleeDamageTuning(0.20d, 0.10d),
                random);
        }

        private void ResolveRegen(CombatTick tick)
        {
            if (_player is null || _fixturePackage is null || _player.PostureState != CombatPostureState.Sitting)
            {
                return;
            }

            var result = _regenResolver.ResolveTick(new CombatRegenTickRequest(
                _player,
                _fixturePackage.RegenAndCombatExitTuning,
                tick,
                TickRateHz));
            _player = result.Actor;
            if (result.ManaRestored > 0)
            {
                _manaRestoredTotal += result.ManaRestored;
                RecordEvent($"mana_restored:{result.ManaRestored}");
            }
        }

        private void CheckHostileDefeat()
        {
            if (_hostile is null || _hostile.IsAlive)
            {
                return;
            }

            _pullsCompleted++;
            _pullActive = false;
            _targetSelected = false;
            HostileDefeatRecorded = true;
            _combatExitRecorded = true;
            _player = _player?.ClearTargetAndThreat().WithCombatState(CombatState.OutOfCombat);
            _hostileAttackState = new CombatAttackStateSnapshot(CombatAttackMode.Off, null, null, CombatAttackTransitionPath.TargetDeath);
            ForceAttackOff(CombatAttackTransitionPath.TargetDeath, "attack_off_target_death");
            _lastStatus = _pullsCompleted < 2
                ? "Trash defeated. Return to camp, sit/med, then pull again."
                : "Two-pull med loop complete.";
            RecordEvent("hostile_defeat");
            RecordEvent("combat_exit");
        }

        private void ForceAttackOff(CombatAttackTransitionPath path, string eventName)
        {
            if (!_playerAttackState.IsAttackOn && path != CombatAttackTransitionPath.TargetDeath)
            {
                return;
            }

            _playerAttackState = new CombatAttackStateSnapshot(CombatAttackMode.Off, null, null, path);
            RecordEvent(eventName);
        }

        private void RespawnBaselineTrash()
        {
            if (_fixturePackage is null)
            {
                return;
            }

            var hydration = new CombatRuntimeEncounterHydrator().HydrateFromFile(
                ResolveFixturePath(FixtureRelativePath),
                new CombatRuntimeEncounterHydrationRequest
                {
                    EncounterFixtureId = EncounterFixtureId,
                    ActiveZoneId = ActiveZoneId,
                    PlayerCombatActorId = PlayerActorId,
                    PlayerLocalCharacterId = PlayerLocalCharacterId,
                    HostileCombatActorIdPrefix = HostileActorPrefix
                });
            if (!hydration.Succeeded || hydration.HostileActors.Count == 0)
            {
                AddError("Respawn hydration failed: " + string.Join("; ", hydration.Errors));
                return;
            }

            _hostile = hydration.HostileActors[0];
            _hostileAttackState = new CombatAttackStateSnapshot(CombatAttackMode.Off, null, null, null);
            _pullActive = false;
            _targetSelected = false;
            _baselineTrash!.position = TrashAnchorPosition();
            RecordEvent("baseline_trash_respawned");
        }

        private void ResolveCombatUntilHostileDefeated()
        {
            for (var tick = 0; tick < MaxSmokeTicksPerPull; tick++)
            {
                if (_hostile is not null && !_hostile.IsAlive)
                {
                    return;
                }

                if (tick == 400)
                {
                    UseSmite();
                }

                AdvanceFixedTicks(1);
            }
        }

        private void MovePlayerToMeleeRange()
        {
            if (_playerMarker is not null && _baselineTrash is not null)
            {
                _playerMarker.position = _baselineTrash.position + new Vector3(0.0f, 0.0f, -M2MeleeRangeMeters);
            }
        }

        private void PositionMarkersForFreshLoop()
        {
            if (_playerMarker is not null)
            {
                _playerMarker.position = new Vector3(0.0f, 1.0f, -5.0f);
            }

            if (_baselineTrash is not null)
            {
                _baselineTrash.position = TrashAnchorPosition();
            }

            FollowCamera();
            ApplySceneVisualState();
        }

        private static Vector3 TrashAnchorPosition()
        {
            return new Vector3(0.0f, 1.0f, 4.0f);
        }

        private long NextWeaponTick(CombatActorState actor)
        {
            return checked(CurrentTick().Index + (long)Math.Ceiling(actor.WeaponDelaySeconds * TickRateHz));
        }

        private CombatTick CurrentTick()
        {
            return _clock?.Snapshot() ?? CombatTick.Zero;
        }

        private int TickRateHz => _fixturePackage?.CombatTickRateHz ?? 50;

        private double DistanceToHostile()
        {
            if (_playerMarker is null || _baselineTrash is null)
            {
                return 999d;
            }

            return Vector3.Distance(_playerMarker.position, _baselineTrash.position);
        }

        private static CombatPoint3 ToCombatPoint(Vector3 point)
        {
            return new CombatPoint3(point.x, point.y, point.z);
        }

        private static string ResolveFixturePath(string relativePath)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static CombatTacticalAbilityProfile LoadSmiteProfile(CombatFixturePackage package)
        {
            var fixture = package.TacticalInstantAbilityProfiles.Single(profile => profile.Id == SmiteAbilityId);
            return CombatTacticalAbilityProfile.FromFixture(fixture, FixtureBand);
        }

        private void BindSceneObjects()
        {
            _playerMarker ??= FindTransform("ClericShellMarker");
            _baselineTrash ??= FindTransform("M2_BaselineTrash");
            _campRestPoint ??= FindTransform("M2_CampRestPoint");
            _pullLane ??= FindTransform("M2_PullLane");
            _floor ??= FindTransform("DevEntry_DistrictBlockout_Floor");
            _camera ??= Camera.main ?? FindFirstObjectByType<Camera>();
        }

        private static Transform? FindTransform(string objectName)
        {
            var found = GameObject.Find(objectName);
            return found == null ? null : found.transform;
        }

        private void FollowCamera()
        {
            if (_camera is null || _playerMarker is null)
            {
                return;
            }

            _camera.transform.SetPositionAndRotation(
                _playerMarker.position + new Vector3(0.0f, CameraFollowHeight, -CameraFollowBack),
                Quaternion.Euler(48.0f, 0.0f, 0.0f));
        }

        private void ApplySceneVisualState()
        {
            ApplyColor(_floor, new Color(0.19f, 0.20f, 0.21f));
            ApplyColor(_playerMarker, new Color(0.25f, 0.70f, 1.0f));
            ApplyColor(_campRestPoint, new Color(0.12f, 0.55f, 0.32f));
            ApplyColor(_pullLane, new Color(0.16f, 0.16f, 0.18f));
            ApplyColor(_baselineTrash, _hostile is not null && _hostile.IsAlive
                ? new Color(0.75f, 0.16f, 0.16f)
                : new Color(0.22f, 0.22f, 0.24f));
        }

        private void ApplyPresentationSettings()
        {
            RenderSettings.ambientLight = new Color(0.28f, 0.30f, 0.34f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.04f, 0.045f, 0.05f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 10.0f;
            RenderSettings.fogEndDistance = 30.0f;

            if (_camera is null)
            {
                return;
            }

            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.035f, 0.038f, 0.043f);
            _camera.orthographic = false;
            _camera.fieldOfView = 44.0f;
        }

        private static void ApplyColor(Transform? target, Color color)
        {
            if (target is null || !target.TryGetComponent<Renderer>(out var renderer))
            {
                return;
            }

            renderer.material.color = color;
        }

        private void UpdateHostileScale()
        {
            if (_baselineTrash is null || _hostile is null || _hostile.MaxHealth <= 0)
            {
                return;
            }

            var healthRatio = Mathf.Clamp01((float)_hostile.CurrentHealth / _hostile.MaxHealth);
            _baselineTrash.localScale = new Vector3(0.8f, Mathf.Lerp(0.35f, 1.0f, healthRatio), 0.8f);
        }

        private void RecordEvent(string eventName)
        {
            _events.Add($"{CurrentTick().Index}:{eventName}");
        }

        private void AddError(string error)
        {
            _errors.Add(error);
            _lastStatus = error;
        }

        private sealed class LoopingMeleeRandomSource : ICombatMeleeRandomSource
        {
            private readonly double _hitRoll;
            private readonly double _damageRollScalar;

            public LoopingMeleeRandomSource(double hitRoll, double damageRollScalar)
            {
                _hitRoll = hitRoll;
                _damageRollScalar = damageRollScalar;
            }

            public double NextHitRoll()
            {
                return _hitRoll;
            }

            public double NextDamageRollScalar()
            {
                return _damageRollScalar;
            }
        }
    }
}
