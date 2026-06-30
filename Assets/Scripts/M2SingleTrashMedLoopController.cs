#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Combat.Fixtures;
using Gravenspire.UnityRuntime.Interaction;
using UnityEngine;

namespace Gravenspire.UnityRuntime.Combat
{
    public sealed class M2SingleTrashMedLoopController : MonoBehaviour
    {
        private const string FixtureRelativePath = "data/combat/t1-combat-fixtures.json";
        private const string EncounterFixtureId = "SoloTrash_EvenCon_T1";
        private const string LinkedEncounterFixtureId = "TwoTrash_Overpull_T1";
        private const string ActiveZoneId = "Haunt_Prototype_T1";
        private const string PlayerActorId = "m2-player-cleric";
        private const string PlayerLocalCharacterId = "local-character-m2-dev";
        private const string HostileActorPrefix = "m2-hostile";
        private const string LinkedHostileActorPrefix = "m2-linked-hostile";
        private const string NamedEncounterFixtureId = "NamedSoloBlock_T1";
        private const string NamedHostileActorPrefix = "m2-named-hostile";
        private const string NamedBlockerSocialGroupId = "m2-named-blocker";
        private const string SmiteAbilityId = "SmiteOfAuthority_T1_Prototype";
        private const string FixtureBand = "Mid";
        private const string LinkedTrashSocialGroupId = "m2-linked-trash";
        private const string LinkedTrashEncounterGroupId = "m2-linked-trash-overpull";
        private const string BaseColorPropertyName = "_BaseColor";
        private const string ColorPropertyName = "_Color";
        private const float PullAggroRadiusMeters = 4.0f;
        private const float M2MeleeRangeMeters = 1.5f;
        private const float HostileMoveSpeedMeters = 2.2f;
        private const float PlayerMoveSpeedMeters = 4.0f;
        private const float CameraFollowHeight = 5.4f;
        private const float CameraFollowBack = 8.4f;
        private const float CameraPitchDegrees = 36.0f;
        private const float CameraFieldOfViewDegrees = 50.0f;
        private const float CameraSteerDegreesPerSecond = 82.0f;
        private const int MaxSmokeTicksPerPull = 5000;
        private const double Feel03HateWindowSeconds = 5.0d;
        private const double DangerHealthRatio = 0.20d;
        private const double DangerManaRatio = 0.10d;

        private readonly List<string> _events = new();
        private readonly List<string> _errors = new();
        private readonly List<string> _overpullEvents = new();
        private readonly List<string> _namedBlockerEvents = new();
        private MaterialPropertyBlock? _materialPropertyBlock;

        private Transform? _playerMarker;
        private Transform? _baselineTrash;
        private Transform? _linkedTrash;
        private Transform? _namedBlocker;
        private Transform? _campRestPoint;
        private Transform? _pullLane;
        private Transform? _floor;
        private Transform? _playerSword;
        private Camera? _camera;
        private GUIStyle? _labelStyle;
        private GUIStyle? _titleStyle;
        private GUIStyle? _buttonStyle;
        private GUIStyle? _statusStyle;
        private Coroutine? _playerSwordSwing;

        private CombatFixturePackage? _fixturePackage;
        private CombatTacticalAbilityProfile? _smiteProfile;
        private CombatActorState? _player;
        private CombatActorState? _hostile;
        private CombatActorState? _linkedHostile;
        private CombatActorState? _namedHostile;
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
        private readonly LoopingMeleeRandomSource _linkedHostileMeleeRandom = new(0.34d, 0.84d);
        private readonly LoopingMeleeRandomSource _namedHostileMeleeRandom = new(0.36d, 0.86d);

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
        private float _cameraYawDegrees;
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

        public bool PullBlockedWhileSittingRecorded { get; private set; }

        public bool AttackBlockedBeforeTargetRecorded { get; private set; }

        public bool SmiteBlockedBeforeAttackRecorded { get; private set; }

        public int PullsCompleted => _pullsCompleted;

        public int ManaRestoredTotal => _manaRestoredTotal;

        public IReadOnlyList<string> Events => _events;

        public IReadOnlyList<string> OverpullEvents => _overpullEvents;

        public IReadOnlyList<string> Errors => _errors;

        public bool LinkedTrashArrangementPresent => _linkedTrash is not null;

        public bool LinkedTrashEnteredHateWithinFeelWindow { get; private set; }

        public double LinkedTrashHateWindowSeconds { get; private set; } = -1d;

        public int OverpullHostilesInHate { get; private set; }

        public bool OverpullDangerousOutcomeRecorded { get; private set; }

        public bool CleanSingleTrashLoopPreservedAfterOverpull { get; private set; }

        public string OverpullOutcome { get; private set; } = "not_run";

        public int OverpullEndingHealth { get; private set; }

        public int OverpullMaxHealth { get; private set; }

        public int OverpullEndingMana { get; private set; }

        public int OverpullMaxMana { get; private set; }

        public bool NamedBlockerAnchorPresent => _namedBlocker is not null;

        public bool NamedBlockerPresentAndTargetable { get; private set; }

        public bool NamedBlockerDistinctFromTrashFixture { get; private set; }

        public bool NamedBlockerBoundaryOutcomeRecorded { get; private set; }

        public bool NamedBlockerNotFarmableTrash { get; private set; }

        public bool CleanSingleTrashLoopPreservedAfterNamedBlocker { get; private set; }

        public string NamedBlockerOutcome { get; private set; } = "not_run";

        public string NamedBlockerHostileFixtureId { get; private set; } = string.Empty;

        public double NamedBlockerTimeToDangerSeconds { get; private set; } = -1d;

        public int NamedBlockerEndingHealth { get; private set; }

        public int NamedBlockerMaxHealth { get; private set; }

        public int NamedBlockerEndingMana { get; private set; }

        public int NamedBlockerMaxMana { get; private set; }

        public int NamedBlockerEndingNamedHealth { get; private set; }

        public int NamedBlockerMaxNamedHealth { get; private set; }

        public int NamedBlockerBaselineTrashMaxHealth { get; private set; }

        public IReadOnlyList<string> NamedBlockerEvents => _namedBlockerEvents;

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
            HandleCameraSteering();
            HandleInput();
            if (!ShouldSuppressLegacyM2DuringObjectiveFreeWalk())
            {
                TryStartBodyPull();
            }

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

            if (ShouldSuppressLegacyM2DuringObjectiveFreeWalk())
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
            var canSmite = _pullActive && hostileAlive && _targetSelected && _playerAttackState.IsAttackOn && IsSmiteReady();
            var canSitStand = playerSitting || (!_pullActive && !hostileAlive);

            DrawHudButton(new Rect(24, 292, 100, 34), "Pull V", ApproachAndPull, canPull);
            DrawHudButton(new Rect(134, 292, 112, 34), "Target Tab", SelectBaselineTarget, canTarget);
            DrawHudButton(new Rect(256, 292, 104, 34), "Attack F", ToggleAttack, canAttack);
            DrawHudButton(new Rect(370, 292, 112, 34), SmiteButtonText(), UseSmite, canSmite);
            DrawHudButton(new Rect(492, 292, 132, 34), "Sit/Stand X", ToggleSitStand, canSitStand);
            DrawHudButton(new Rect(24, 334, 100, 34), "Reset R", ResetLoop, enabled: true);
            GUI.matrix = previousMatrix;
        }

        // CLIENT-LOCAL: suppress legacy M2 presentation and proximity aggro during human objective free-walk.
        private bool ShouldSuppressLegacyM2DuringObjectiveFreeWalk()
        {
            if (Application.isBatchMode || _smokeRunning)
            {
                return false;
            }

            if (GameObject.Find(S3PlayerInteractionHarness.HarnessRootName) == null)
            {
                return false;
            }

            if (_pullActive || _pullsCompleted > 0 || _targetSelected || _playerAttackState.IsAttackOn)
            {
                return false;
            }

            if (_player is not null && _player.CombatState != CombatState.OutOfCombat)
            {
                return false;
            }

            if (_hostile is not null && _hostile.CombatState != CombatState.OutOfCombat)
            {
                return false;
            }

            return true;
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
                    if (pull == 0)
                    {
                        _player = _player! with { PostureState = CombatPostureState.Sitting };
                        if (TryStartBodyPull())
                        {
                            AddError("pull_1: body pull started while the player was sitting.");
                            return false;
                        }

                        _player = _player with { PostureState = CombatPostureState.Standing };
                    }

                    if (!TryStartBodyPull())
                    {
                        AddError($"pull_{pull + 1}: body pull did not start.");
                        return false;
                    }

                    if (pull == 0)
                    {
                        ToggleAttack();
                        if (_playerAttackState.IsAttackOn)
                        {
                            AddError("pull_1: Attack toggled on before explicit target selection.");
                            return false;
                        }
                    }

                    SelectBaselineTarget();
                    if (pull == 0)
                    {
                        var manaBeforeSmiteGuard = _player!.CurrentMana;
                        var hostileHealthBeforeSmiteGuard = _hostile!.CurrentHealth;
                        UseSmite();
                        if (_player.CurrentMana != manaBeforeSmiteGuard ||
                            _hostile.CurrentHealth != hostileHealthBeforeSmiteGuard ||
                            _smiteCooldownEndsTick is not null)
                        {
                            AddError("pull_1: Smite resolved before Attack was ON.");
                            return false;
                        }
                    }

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
                    PullBlockedWhileSittingRecorded &&
                    AttackBlockedBeforeTargetRecorded &&
                    SmiteBlockedBeforeAttackRecorded &&
                    TwoPullLoopComplete &&
                    Errors.Count == 0;
            }
            finally
            {
                _smokeRunning = false;
                ApplySceneVisualState();
            }
        }

        public bool RunAutomatedLinkedTrashOverpullSmoke()
        {
            _smokeRunning = true;
            try
            {
                ResetOverpullMetrics();
                if (!RunBadPullOverpullSmoke())
                {
                    return false;
                }

                CleanSingleTrashLoopPreservedAfterOverpull = RunAutomatedTwoPullSmoke();
                return LinkedTrashArrangementPresent &&
                    LinkedTrashEnteredHateWithinFeelWindow &&
                    OverpullDangerousOutcomeRecorded &&
                    CleanSingleTrashLoopPreservedAfterOverpull &&
                    Errors.Count == 0;
            }
            finally
            {
                _smokeRunning = false;
                ApplySceneVisualState();
            }
        }

        public bool RunAutomatedNamedBlockerBoundarySmoke()
        {
            _smokeRunning = true;
            try
            {
                ResetNamedBlockerMetrics();
                if (!RunNamedBlockerBoundarySmoke())
                {
                    return false;
                }

                CleanSingleTrashLoopPreservedAfterNamedBlocker = RunAutomatedTwoPullSmoke();
                return NamedBlockerAnchorPresent &&
                    NamedBlockerPresentAndTargetable &&
                    NamedBlockerDistinctFromTrashFixture &&
                    NamedBlockerBoundaryOutcomeRecorded &&
                    NamedBlockerNotFarmableTrash &&
                    CleanSingleTrashLoopPreservedAfterNamedBlocker &&
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
            PullBlockedWhileSittingRecorded = false;
            AttackBlockedBeforeTargetRecorded = false;
            SmiteBlockedBeforeAttackRecorded = false;
            _combatExitRecorded = false;
            _sitMedStarted = false;
            _smiteCooldownEndsTick = null;
            _pullActive = false;
            _targetSelected = false;
            _playerAttackState = new CombatAttackStateSnapshot(CombatAttackMode.Off, null, null, null);
            _hostileAttackState = new CombatAttackStateSnapshot(CombatAttackMode.Off, null, null, null);
            _linkedHostile = null;
            InitializeLoop();
            RecordEvent("loop_reset");
        }

        private void ResetOverpullMetrics()
        {
            _overpullEvents.Clear();
            LinkedTrashEnteredHateWithinFeelWindow = false;
            LinkedTrashHateWindowSeconds = -1d;
            OverpullHostilesInHate = 0;
            OverpullDangerousOutcomeRecorded = false;
            CleanSingleTrashLoopPreservedAfterOverpull = false;
            OverpullOutcome = "not_run";
            OverpullEndingHealth = 0;
            OverpullMaxHealth = 0;
            OverpullEndingMana = 0;
            OverpullMaxMana = 0;
        }

        private void ResetNamedBlockerMetrics()
        {
            _namedBlockerEvents.Clear();
            NamedBlockerPresentAndTargetable = false;
            NamedBlockerDistinctFromTrashFixture = false;
            NamedBlockerBoundaryOutcomeRecorded = false;
            NamedBlockerNotFarmableTrash = false;
            CleanSingleTrashLoopPreservedAfterNamedBlocker = false;
            NamedBlockerOutcome = "not_run";
            NamedBlockerHostileFixtureId = string.Empty;
            NamedBlockerTimeToDangerSeconds = -1d;
            NamedBlockerEndingHealth = 0;
            NamedBlockerMaxHealth = 0;
            NamedBlockerEndingMana = 0;
            NamedBlockerMaxMana = 0;
            NamedBlockerEndingNamedHealth = 0;
            NamedBlockerMaxNamedHealth = 0;
            NamedBlockerBaselineTrashMaxHealth = 0;
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
                HydrateNamedBlockerPresence();
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

        // CLIENT-LOCAL: local camera orbit only; never pulls toward NPCs, relics, vendors, or other POIs.
        private void HandleCameraSteering()
        {
            var yawInput = Axis(KeyCode.E, KeyCode.Q);
            if (Mathf.Abs(yawInput) <= 0.0001f)
            {
                return;
            }

            var frameSeconds = Mathf.Clamp(Time.unscaledDeltaTime, 0.0f, 0.05f);
            _cameraYawDegrees = Mathf.Repeat(
                _cameraYawDegrees + yawInput * CameraSteerDegreesPerSecond * frameSeconds,
                360.0f);
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

            if (_player is not null && _player.PostureState == CombatPostureState.Sitting)
            {
                _lastStatus = "Stand before pulling. Med break ends before the next body pull.";
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

            if (_player.PostureState == CombatPostureState.Sitting)
            {
                PullBlockedWhileSittingRecorded = true;
                _lastStatus = "Stand before pulling. Med break ends before the next body pull.";
                RecordEvent("pull_blocked_while_sitting");
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

            if (!_pullActive)
            {
                _lastStatus = "Start the body pull before targeting this trash.";
                RecordEvent("target_blocked_no_pull");
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

            if (!_pullActive)
            {
                _lastStatus = "Start the body pull before toggling Attack.";
                RecordEvent("attack_blocked_no_pull");
                return;
            }

            if (!_targetSelected)
            {
                AttackBlockedBeforeTargetRecorded = true;
                _lastStatus = "Target the baseline trash before toggling Attack.";
                RecordEvent("attack_blocked_without_target");
                return;
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

            if (!_pullActive)
            {
                _lastStatus = "Start the body pull before casting Smite.";
                RecordEvent("smite_blocked_no_pull");
                return;
            }

            if (!_targetSelected)
            {
                _lastStatus = "Target the baseline trash before casting Smite.";
                RecordEvent("smite_blocked_without_target");
                return;
            }

            if (!_playerAttackState.IsAttackOn)
            {
                SmiteBlockedBeforeAttackRecorded = true;
                _lastStatus = "Toggle Attack ON before casting Smite in this loop.";
                RecordEvent("smite_blocked_without_attack");
                return;
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

                int smiteDamage = 0;
                foreach (var effect in result.AppliedEffects)
                {
                    if (effect.Damage.HasValue)
                    {
                        smiteDamage += effect.Damage.Value;
                    }
                }
                if (_baselineTrash != null)
                {
                    CombatJuice.Instance.SpawnDamageText($"SMITE -{smiteDamage}!", _baselineTrash.position, Color.yellow);
                    if (_baselineTrash.TryGetComponent<Renderer>(out var r))
                    {
                        CombatJuice.Instance.Flash(r, Color.yellow, 0.25f);
                    }
                }
                CombatJuice.Instance.TriggerCameraShake(0.28f, 0.32f);
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
                        if (_baselineTrash != null)
                        {
                            CombatJuice.Instance.SpawnDamageText($"-{playerResult.Damage}", _baselineTrash.position, Color.white);
                            if (_baselineTrash.TryGetComponent<Renderer>(out var r))
                            {
                                CombatJuice.Instance.Flash(r, Color.white);
                            }
                        }

                        TriggerPlayerSwordSwing();
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
                        if (_playerMarker != null)
                        {
                            CombatJuice.Instance.SpawnDamageText($"-{hostileResult.Damage}", _playerMarker.position, Color.red);
                            if (_playerMarker.TryGetComponent<Renderer>(out var r))
                            {
                                CombatJuice.Instance.Flash(r, Color.red);
                            }
                        }
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
            ICombatMeleeRandomSource random,
            double? distanceMetersToTarget = null)
        {
            return new CombatMeleeTickRequest(
                attacker,
                target,
                attackState,
                _zoneGate!,
                distanceMetersToTarget ?? DistanceToHostile(),
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

        private bool RunBadPullOverpullSmoke()
        {
            BindSceneObjects();
            if (_linkedTrash is null)
            {
                AddError("M2 linked trash scene marker was not found.");
                return false;
            }

            var hydration = new CombatRuntimeEncounterHydrator().HydrateFromFile(
                ResolveFixturePath(FixtureRelativePath),
                new CombatRuntimeEncounterHydrationRequest
                {
                    EncounterFixtureId = LinkedEncounterFixtureId,
                    ActiveZoneId = ActiveZoneId,
                    PlayerCombatActorId = PlayerActorId,
                    PlayerLocalCharacterId = PlayerLocalCharacterId,
                    HostileCombatActorIdPrefix = LinkedHostileActorPrefix
                });

            if (!hydration.Succeeded || hydration.PlayerActor is null || hydration.HostileActors.Count < 2)
            {
                AddError("Linked-trash encounter hydration failed: " + string.Join("; ", hydration.Errors));
                return false;
            }

            _player = hydration.PlayerActor;
            _hostile = hydration.HostileActors[0];
            _linkedHostile = hydration.HostileActors[1];
            _zoneGate = new CombatZoneGate();
            _zoneGate.ActivateZone(ActiveZoneId, CombatZoneType.HauntZone);
            _clock = new FixedCombatClock(_fixturePackage?.CombatTickRateHz ?? 50);
            _pullActive = false;
            _targetSelected = false;
            _playerAttackState = new CombatAttackStateSnapshot(CombatAttackMode.Off, null, null, null);
            _hostileAttackState = new CombatAttackStateSnapshot(CombatAttackMode.Off, null, null, null);
            PositionMarkersForBadPull();

            var playerPoint = ToCombatPoint(_playerMarker!.position);
            var primaryPoint = ToCombatPoint(_baselineTrash!.position);
            var linkedPoint = ToCombatPoint(_linkedTrash.position);
            var primaryCandidate = LinkedPullCandidate(
                _hostile,
                primaryPoint,
                "M2_BaselineTrash",
                assistOrderIndex: 0);
            var linkedCandidate = LinkedPullCandidate(
                _linkedHostile,
                linkedPoint,
                "M2_LinkedTrash",
                assistOrderIndex: 1);

            var pull = new CombatPullCoordinator().ResolveBodyPull(
                _player,
                playerPoint,
                primaryCandidate,
                new[] { linkedCandidate },
                _zoneGate,
                CurrentTick());

            if (!pull.Succeeded || pull.PrimaryHostile is null || pull.AssistingHostiles.Count == 0)
            {
                AddError("Linked-trash body pull failed: " + string.Join("; ", pull.Errors));
                return false;
            }

            var primary = pull.PrimaryHostile.WithCombatState(CombatState.InCombat);
            var linked = pull.AssistingHostiles[0].WithCombatState(CombatState.InCombat);
            _pullActive = true;
            _pullStartedBeforeAttack = true;
            _pullDidNotAutoEnableAttack = !pull.PlayerAttackEnabled;
            OverpullHostilesInHate = 1 + pull.AssistingHostiles.Count;
            LinkedTrashHateWindowSeconds = 0d;
            LinkedTrashEnteredHateWithinFeelWindow = OverpullHostilesInHate >= 2 &&
                LinkedTrashHateWindowSeconds <= Feel03HateWindowSeconds;
            RecordOverpullEvent($"bad_pull_primary_hate:{primary.CombatActorId}");
            RecordOverpullEvent($"bad_pull_linked_hate:{linked.CombatActorId}");
            RecordOverpullEvent($"hate_window_seconds:{LinkedTrashHateWindowSeconds:0.0}");

            _player = _player.WithTarget(primary.CombatActorId).WithCombatState(CombatState.InCombat);
            _targetSelected = true;
            _hostile = primary;
            _linkedHostile = linked;
            _hostileAttackState = new CombatAttackStateSnapshot(
                CombatAttackMode.On,
                _player.CombatActorId,
                NextWeaponTick(primary),
                CombatAttackTransitionPath.PlayerToggleOn);
            var linkedAttackState = new CombatAttackStateSnapshot(
                CombatAttackMode.On,
                _player.CombatActorId,
                NextWeaponTick(linked),
                CombatAttackTransitionPath.PlayerToggleOn);

            var attack = new CombatAttackStateMachine().ToggleOn(new CombatAttackToggleOnRequest(
                _player,
                primary,
                _zoneGate,
                M2MeleeRangeMeters,
                CurrentTick(),
                TickRateHz));
            if (!attack.Succeeded)
            {
                AddError("Linked-trash player Attack toggle failed: " + string.Join("; ", attack.RejectionReasons));
                return false;
            }

            _playerAttackState = attack.Snapshot;
            var instantResolver = new CombatInstantAbilityResolver();
            long? overpullSmiteCooldownEndsTick = null;
            var targetingPrimary = true;
            ResolveOverpullSmite(instantResolver, ref primary, ref linked, targetingPrimary, ref overpullSmiteCooldownEndsTick);

            for (var tickIndex = 0; tickIndex < MaxSmokeTicksPerPull; tickIndex++)
            {
                var tick = _clock.AdvanceTicks(1);
                if (targetingPrimary && !primary.IsAlive && linked.IsAlive)
                {
                    targetingPrimary = false;
                    _player = _player!.WithTarget(linked.CombatActorId);
                    _playerAttackState = new CombatAttackStateSnapshot(
                        CombatAttackMode.On,
                        linked.CombatActorId,
                        NextWeaponTick(_player),
                        CombatAttackTransitionPath.PlayerToggleOn);
                    RecordOverpullEvent("player_retargeted_linked_trash");
                }

                if (overpullSmiteCooldownEndsTick is not null &&
                    tick.Index >= overpullSmiteCooldownEndsTick.Value &&
                    (targetingPrimary ? primary.IsAlive : linked.IsAlive))
                {
                    ResolveOverpullSmite(
                        instantResolver,
                        ref primary,
                        ref linked,
                        targetingPrimary,
                        ref overpullSmiteCooldownEndsTick);
                }

                ResolvePlayerOverpullMelee(tick, ref primary, ref linked, targetingPrimary);
                ResolveHostileOverpullMelee(tick, ref primary, ref _hostileAttackState, _hostileMeleeRandom, "primary");
                ResolveHostileOverpullMelee(tick, ref linked, ref linkedAttackState, _linkedHostileMeleeRandom, "linked");
                _hostile = primary;
                _linkedHostile = linked;
                UpdateHostileScale();
                UpdateLinkedHostileScale();

                if (RecordDangerIfReached(primary, linked))
                {
                    return true;
                }

                if (!primary.IsAlive && !linked.IsAlive)
                {
                    CaptureOverpullTelemetry("comfortable_two_trash_win");
                    return false;
                }
            }

            CaptureOverpullTelemetry("unresolved_tick_budget");
            return false;
        }

        private CombatPullCandidate LinkedPullCandidate(
            CombatActorState actor,
            CombatPoint3 position,
            string anchorId,
            int assistOrderIndex)
        {
            return new CombatPullCandidate(
                actor,
                position,
                new CombatSpatialAnchorSet(position, ToCombatPoint(_playerMarker!.position), anchorId, "ClericShellMarker"),
                PullAggroRadiusMeters,
                CombatSocialAssistProfile.T1Default(LinkedTrashSocialGroupId, LinkedTrashEncounterGroupId, assistOrderIndex),
                Array.Empty<CombatLosLayer>(),
                Array.Empty<CombatLosLayer>(),
                assistOrderIndex);
        }

        private void ResolvePlayerOverpullMelee(
            CombatTick tick,
            ref CombatActorState primary,
            ref CombatActorState linked,
            bool targetingPrimary)
        {
            if (_player is null || !_playerAttackState.IsAttackOn)
            {
                return;
            }

            var target = targetingPrimary ? primary : linked;
            if (!target.IsAlive)
            {
                return;
            }

            var result = _meleeResolver.ResolveTick(MeleeRequest(
                _player,
                target,
                _playerAttackState,
                tick,
                _playerMeleeRandom,
                M2MeleeRangeMeters));
            if (result.Outcome == CombatMeleeTickOutcome.NotDue)
            {
                return;
            }

            _playerAttackState = _playerAttackState with { NextSwingDueTick = result.NextSwingDueTick };
            if (result.TargetAfterResolution is not null)
            {
                if (targetingPrimary)
                {
                    primary = result.TargetAfterResolution;
                }
                else
                {
                    linked = result.TargetAfterResolution;
                }
            }

            if (result.AppliedDamage)
            {
                RecordOverpullEvent($"player_melee_hit_{(targetingPrimary ? "primary" : "linked")}:{result.Damage}");
            }
        }

        private void ResolveHostileOverpullMelee(
            CombatTick tick,
            ref CombatActorState hostile,
            ref CombatAttackStateSnapshot attackState,
            ICombatMeleeRandomSource random,
            string label)
        {
            if (_player is null || !hostile.IsAlive)
            {
                return;
            }

            var result = _meleeResolver.ResolveTick(MeleeRequest(
                hostile,
                _player,
                attackState,
                tick,
                random,
                M2MeleeRangeMeters));
            if (result.Outcome == CombatMeleeTickOutcome.NotDue)
            {
                return;
            }

            attackState = attackState with { NextSwingDueTick = result.NextSwingDueTick };
            if (result.TargetAfterResolution is not null)
            {
                _player = result.TargetAfterResolution;
            }

            if (result.AppliedDamage)
            {
                RecordOverpullEvent($"{label}_trash_melee_hit:{result.Damage}");
            }
        }

        private void ResolveOverpullSmite(
            CombatInstantAbilityResolver instantResolver,
            ref CombatActorState primary,
            ref CombatActorState linked,
            bool targetingPrimary,
            ref long? cooldownEndsTick)
        {
            if (_player is null || _zoneGate is null || _smiteProfile is null)
            {
                return;
            }

            var target = targetingPrimary ? primary : linked;
            if (!target.IsAlive)
            {
                return;
            }

            var result = instantResolver.Resolve(new CombatInstantAbilityRequest(
                $"m2-overpull-smite-{CurrentTick().Index}",
                _player,
                target,
                _zoneGate,
                M2MeleeRangeMeters,
                Array.Empty<CombatLosLayer>(),
                CurrentTick(),
                TickRateHz,
                _smiteProfile));

            if (!result.Succeeded)
            {
                return;
            }

            _player = result.Caster;
            cooldownEndsTick = result.CooldownEndsTick;
            if (result.TargetAfterResolution is not null)
            {
                if (targetingPrimary)
                {
                    primary = result.TargetAfterResolution;
                }
                else
                {
                    linked = result.TargetAfterResolution;
                }
            }

            RecordOverpullEvent($"smite_resolved_{(targetingPrimary ? "primary" : "linked")}");
        }

        private bool RecordDangerIfReached(CombatActorState primary, CombatActorState linked)
        {
            if (_player is null)
            {
                return false;
            }

            if (!_player.IsAlive)
            {
                CaptureOverpullTelemetry("player_lost");
                OverpullDangerousOutcomeRecorded = true;
                return true;
            }

            var healthRatio = _player.CurrentHealth / (double)_player.MaxHealth;
            var manaRatio = _player.MaxMana <= 0 ? 0d : _player.CurrentMana / (double)_player.MaxMana;
            if ((primary.IsAlive || linked.IsAlive) && (healthRatio < DangerHealthRatio || manaRatio < DangerManaRatio))
            {
                CaptureOverpullTelemetry("forced_flee_threshold");
                OverpullDangerousOutcomeRecorded = true;
                return true;
            }

            return false;
        }

        private void CaptureOverpullTelemetry(string outcome)
        {
            OverpullOutcome = outcome;
            if (_player is not null)
            {
                OverpullEndingHealth = _player.CurrentHealth;
                OverpullMaxHealth = _player.MaxHealth;
                OverpullEndingMana = _player.CurrentMana;
                OverpullMaxMana = _player.MaxMana;
            }

            RecordOverpullEvent($"outcome:{outcome}");
            RecordOverpullEvent($"ending_health:{OverpullEndingHealth}/{OverpullMaxHealth}");
            RecordOverpullEvent($"ending_mana:{OverpullEndingMana}/{OverpullMaxMana}");
        }

        private bool RunNamedBlockerBoundarySmoke()
        {
            BindSceneObjects();
            if (_namedBlocker is null)
            {
                AddError("M2 named blocker scene marker was not found.");
                return false;
            }

            var baselineTrashHydration = new CombatRuntimeEncounterHydrator().HydrateFromFile(
                ResolveFixturePath(FixtureRelativePath),
                new CombatRuntimeEncounterHydrationRequest
                {
                    EncounterFixtureId = EncounterFixtureId,
                    ActiveZoneId = ActiveZoneId,
                    PlayerCombatActorId = PlayerActorId,
                    PlayerLocalCharacterId = PlayerLocalCharacterId,
                    HostileCombatActorIdPrefix = HostileActorPrefix
                });
            if (!baselineTrashHydration.Succeeded || baselineTrashHydration.HostileActors.Count == 0)
            {
                AddError("Baseline-trash reference hydration failed: " + string.Join("; ", baselineTrashHydration.Errors));
                return false;
            }

            NamedBlockerBaselineTrashMaxHealth = baselineTrashHydration.HostileActors[0].MaxHealth;

            var hydration = new CombatRuntimeEncounterHydrator().HydrateFromFile(
                ResolveFixturePath(FixtureRelativePath),
                new CombatRuntimeEncounterHydrationRequest
                {
                    EncounterFixtureId = NamedEncounterFixtureId,
                    ActiveZoneId = ActiveZoneId,
                    PlayerCombatActorId = PlayerActorId,
                    PlayerLocalCharacterId = PlayerLocalCharacterId,
                    HostileCombatActorIdPrefix = NamedHostileActorPrefix
                });
            if (!hydration.Succeeded || hydration.PlayerActor is null || hydration.HostileActors.Count == 0)
            {
                AddError("Named-blocker encounter hydration failed: " + string.Join("; ", hydration.Errors));
                return false;
            }

            _player = hydration.PlayerActor;
            _namedHostile = hydration.HostileActors[0];
            _zoneGate = new CombatZoneGate();
            _zoneGate.ActivateZone(ActiveZoneId, CombatZoneType.HauntZone);
            _clock = new FixedCombatClock(_fixturePackage?.CombatTickRateHz ?? 50);
            _pullActive = false;
            _targetSelected = false;
            _playerAttackState = new CombatAttackStateSnapshot(CombatAttackMode.Off, null, null, null);
            _hostileAttackState = new CombatAttackStateSnapshot(CombatAttackMode.Off, null, null, null);
            PositionMarkersForNamedBlocker();

            NamedBlockerHostileFixtureId = NamedEncounterFixtureId;
            NamedBlockerMaxNamedHealth = _namedHostile.MaxHealth;
            NamedBlockerDistinctFromTrashFixture =
                !string.Equals(NamedEncounterFixtureId, EncounterFixtureId, StringComparison.Ordinal) &&
                _namedHostile.MaxHealth > NamedBlockerBaselineTrashMaxHealth;
            RecordNamedBlockerEvent($"named_fixture:{NamedEncounterFixtureId}");
            RecordNamedBlockerEvent($"named_max_health:{_namedHostile.MaxHealth} baseline_trash_max_health:{NamedBlockerBaselineTrashMaxHealth}");

            var playerPoint = ToCombatPoint(_playerMarker!.position);
            var namedPoint = ToCombatPoint(_namedBlocker.position);
            var pull = new CombatPullCoordinator().ResolveBodyPull(
                _player,
                playerPoint,
                NamedPullCandidate(_namedHostile, namedPoint, "M2_NamedBlocker"),
                Array.Empty<CombatPullCandidate>(),
                _zoneGate,
                CurrentTick());

            if (!pull.Succeeded || pull.PrimaryHostile is null)
            {
                AddError("Named-blocker body pull failed: " + string.Join("; ", pull.Errors));
                return false;
            }

            var named = pull.PrimaryHostile.WithCombatState(CombatState.InCombat);
            _pullActive = true;
            _pullStartedBeforeAttack = true;
            _pullDidNotAutoEnableAttack = !pull.PlayerAttackEnabled;
            _player = _player.WithTarget(named.CombatActorId).WithCombatState(CombatState.InCombat);
            _targetSelected = true;
            _namedHostile = named;
            NamedBlockerPresentAndTargetable = _namedBlocker is not null && _targetSelected && named.IsAlive;
            RecordNamedBlockerEvent($"named_present_targetable:{named.CombatActorId}");

            _hostileAttackState = new CombatAttackStateSnapshot(
                CombatAttackMode.On,
                _player.CombatActorId,
                NextWeaponTick(named),
                CombatAttackTransitionPath.PlayerToggleOn);

            var attack = new CombatAttackStateMachine().ToggleOn(new CombatAttackToggleOnRequest(
                _player,
                named,
                _zoneGate,
                M2MeleeRangeMeters,
                CurrentTick(),
                TickRateHz));
            if (!attack.Succeeded)
            {
                AddError("Named-blocker player Attack toggle failed: " + string.Join("; ", attack.RejectionReasons));
                return false;
            }

            _playerAttackState = attack.Snapshot;
            var instantResolver = new CombatInstantAbilityResolver();
            long? namedSmiteCooldownEndsTick = null;
            ResolveNamedBlockerSmite(instantResolver, ref named, ref namedSmiteCooldownEndsTick);

            for (var tickIndex = 0; tickIndex < MaxSmokeTicksPerPull; tickIndex++)
            {
                var tick = _clock.AdvanceTicks(1);
                if (namedSmiteCooldownEndsTick is not null &&
                    tick.Index >= namedSmiteCooldownEndsTick.Value &&
                    named.IsAlive)
                {
                    ResolveNamedBlockerSmite(instantResolver, ref named, ref namedSmiteCooldownEndsTick);
                }

                ResolvePlayerNamedBlockerMelee(tick, ref named);
                ResolveHostileNamedBlockerMelee(tick, ref named, ref _hostileAttackState, _namedHostileMeleeRandom);
                _namedHostile = named;
                UpdateNamedBlockerScale();

                if (RecordNamedBlockerDangerIfReached(named, tick))
                {
                    return true;
                }

                if (!named.IsAlive)
                {
                    CaptureNamedBlockerTelemetry("named_solo_killed", named, tick);
                    NamedBlockerNotFarmableTrash = false;
                    AddError("Named blocker was solo-killed; FEEL-02 tuning defect unless flagged exploit-under-investigation.");
                    return false;
                }
            }

            CaptureNamedBlockerTelemetry("unresolved_tick_budget", named, CurrentTick());
            return false;
        }

        private CombatPullCandidate NamedPullCandidate(
            CombatActorState actor,
            CombatPoint3 position,
            string anchorId)
        {
            return new CombatPullCandidate(
                actor,
                position,
                new CombatSpatialAnchorSet(position, ToCombatPoint(_playerMarker!.position), anchorId, "ClericShellMarker"),
                PullAggroRadiusMeters,
                CombatSocialAssistProfile.T1Default(NamedBlockerSocialGroupId),
                Array.Empty<CombatLosLayer>(),
                Array.Empty<CombatLosLayer>(),
                AuthoredColliderIndex: 0);
        }

        private void ResolvePlayerNamedBlockerMelee(CombatTick tick, ref CombatActorState named)
        {
            if (_player is null || !_playerAttackState.IsAttackOn || !named.IsAlive)
            {
                return;
            }

            var result = _meleeResolver.ResolveTick(MeleeRequest(
                _player,
                named,
                _playerAttackState,
                tick,
                _playerMeleeRandom,
                M2MeleeRangeMeters));
            if (result.Outcome == CombatMeleeTickOutcome.NotDue)
            {
                return;
            }

            _playerAttackState = _playerAttackState with { NextSwingDueTick = result.NextSwingDueTick };
            if (result.TargetAfterResolution is not null)
            {
                named = result.TargetAfterResolution;
            }

            if (result.AppliedDamage)
            {
                RecordNamedBlockerEvent($"player_melee_hit_named:{result.Damage}");
            }
        }

        private void ResolveHostileNamedBlockerMelee(
            CombatTick tick,
            ref CombatActorState named,
            ref CombatAttackStateSnapshot attackState,
            ICombatMeleeRandomSource random)
        {
            if (_player is null || !named.IsAlive)
            {
                return;
            }

            var result = _meleeResolver.ResolveTick(MeleeRequest(
                named,
                _player,
                attackState,
                tick,
                random,
                M2MeleeRangeMeters));
            if (result.Outcome == CombatMeleeTickOutcome.NotDue)
            {
                return;
            }

            attackState = attackState with { NextSwingDueTick = result.NextSwingDueTick };
            if (result.TargetAfterResolution is not null)
            {
                _player = result.TargetAfterResolution;
            }

            if (result.AppliedDamage)
            {
                RecordNamedBlockerEvent($"named_melee_hit:{result.Damage}");
            }
        }

        private void ResolveNamedBlockerSmite(
            CombatInstantAbilityResolver instantResolver,
            ref CombatActorState named,
            ref long? cooldownEndsTick)
        {
            if (_player is null || _zoneGate is null || _smiteProfile is null || !named.IsAlive)
            {
                return;
            }

            var result = instantResolver.Resolve(new CombatInstantAbilityRequest(
                $"m2-named-blocker-smite-{CurrentTick().Index}",
                _player,
                named,
                _zoneGate,
                M2MeleeRangeMeters,
                Array.Empty<CombatLosLayer>(),
                CurrentTick(),
                TickRateHz,
                _smiteProfile));

            if (!result.Succeeded)
            {
                return;
            }

            _player = result.Caster;
            cooldownEndsTick = result.CooldownEndsTick;
            if (result.TargetAfterResolution is not null)
            {
                named = result.TargetAfterResolution;
            }

            RecordNamedBlockerEvent("smite_resolved_named");
        }

        private bool RecordNamedBlockerDangerIfReached(CombatActorState named, CombatTick tick)
        {
            if (_player is null)
            {
                return false;
            }

            if (!_player.IsAlive)
            {
                CaptureNamedBlockerTelemetry("player_lost", named, tick);
                NamedBlockerBoundaryOutcomeRecorded = true;
                NamedBlockerNotFarmableTrash = true;
                return true;
            }

            var healthRatio = _player.CurrentHealth / (double)_player.MaxHealth;
            var manaRatio = _player.MaxMana <= 0 ? 0d : _player.CurrentMana / (double)_player.MaxMana;
            if (named.IsAlive && (healthRatio < DangerHealthRatio || manaRatio < DangerManaRatio))
            {
                CaptureNamedBlockerTelemetry("forced_flee_threshold", named, tick);
                NamedBlockerBoundaryOutcomeRecorded = true;
                NamedBlockerNotFarmableTrash = true;
                return true;
            }

            return false;
        }

        private void CaptureNamedBlockerTelemetry(string outcome, CombatActorState named, CombatTick tick)
        {
            NamedBlockerOutcome = outcome;
            NamedBlockerTimeToDangerSeconds = tick.Index / (double)TickRateHz;
            if (_player is not null)
            {
                NamedBlockerEndingHealth = _player.CurrentHealth;
                NamedBlockerMaxHealth = _player.MaxHealth;
                NamedBlockerEndingMana = _player.CurrentMana;
                NamedBlockerMaxMana = _player.MaxMana;
            }

            NamedBlockerEndingNamedHealth = named.CurrentHealth;
            NamedBlockerMaxNamedHealth = named.MaxHealth;

            RecordNamedBlockerEvent($"outcome:{outcome}");
            RecordNamedBlockerEvent($"time_to_danger_seconds:{NamedBlockerTimeToDangerSeconds:0.00}");
            RecordNamedBlockerEvent($"ending_health:{NamedBlockerEndingHealth}/{NamedBlockerMaxHealth}");
            RecordNamedBlockerEvent($"ending_mana:{NamedBlockerEndingMana}/{NamedBlockerMaxMana}");
            RecordNamedBlockerEvent($"named_ending_health:{NamedBlockerEndingNamedHealth}/{NamedBlockerMaxNamedHealth}");
        }

        private void HydrateNamedBlockerPresence()
        {
            var hydration = new CombatRuntimeEncounterHydrator().HydrateFromFile(
                ResolveFixturePath(FixtureRelativePath),
                new CombatRuntimeEncounterHydrationRequest
                {
                    EncounterFixtureId = NamedEncounterFixtureId,
                    ActiveZoneId = ActiveZoneId,
                    PlayerCombatActorId = PlayerActorId,
                    PlayerLocalCharacterId = PlayerLocalCharacterId,
                    HostileCombatActorIdPrefix = NamedHostileActorPrefix
                });
            if (hydration.Succeeded && hydration.HostileActors.Count > 0)
            {
                _namedHostile = hydration.HostileActors[0];
            }
        }

        private void PositionMarkersForNamedBlocker()
        {
            if (_namedBlocker is not null)
            {
                _namedBlocker.position = NamedBlockerAnchorPosition();
            }

            if (_playerMarker is not null && _namedBlocker is not null)
            {
                _playerMarker.position = _namedBlocker.position + new Vector3(0.0f, -0.4f, -M2MeleeRangeMeters);
            }

            FollowCamera();
            ApplySceneVisualState();
        }

        private static Vector3 NamedBlockerAnchorPosition()
        {
            return new Vector3(-2.8f, 1.4f, 5.6f);
        }

        private void UpdateNamedBlockerScale()
        {
            if (_namedBlocker is null || _namedHostile is null || _namedHostile.MaxHealth <= 0)
            {
                return;
            }

            var healthRatio = Mathf.Clamp01((float)_namedHostile.CurrentHealth / _namedHostile.MaxHealth);
            _namedBlocker.localScale = new Vector3(1.25f, Mathf.Lerp(0.7f, 1.4f, healthRatio), 1.25f);
        }

        private void RecordNamedBlockerEvent(string eventName)
        {
            _namedBlockerEvents.Add($"{CurrentTick().Index}:{eventName}");
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

            if (_linkedTrash is not null)
            {
                _linkedTrash.position = LinkedTrashAnchorPosition();
            }

            if (_namedBlocker is not null)
            {
                _namedBlocker.position = NamedBlockerAnchorPosition();
            }

            FollowCamera();
            ApplySceneVisualState();
        }

        private void PositionMarkersForBadPull()
        {
            if (_baselineTrash is not null)
            {
                _baselineTrash.position = TrashAnchorPosition();
            }

            if (_linkedTrash is not null)
            {
                _linkedTrash.position = LinkedTrashAnchorPosition();
            }

            if (_playerMarker is not null && _baselineTrash is not null)
            {
                _playerMarker.position = _baselineTrash.position + new Vector3(0.0f, 0.0f, -M2MeleeRangeMeters);
            }

            FollowCamera();
            ApplySceneVisualState();
        }

        private static Vector3 TrashAnchorPosition()
        {
            return new Vector3(0.0f, 1.0f, 4.0f);
        }

        private static Vector3 LinkedTrashAnchorPosition()
        {
            return new Vector3(2.3f, 1.0f, 4.8f);
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
            _linkedTrash ??= FindTransform("M2_LinkedTrash");
            _namedBlocker ??= FindTransform("M2_NamedBlocker");
            _campRestPoint ??= FindTransform("M2_CampRestPoint");
            _pullLane ??= FindTransform("M2_PullLane");
            _floor ??= FindTransform("DevEntry_DistrictBlockout_Floor");
            _camera ??= Camera.main ?? FindFirstObjectByType<Camera>();
            EnsurePlayerSword();
        }

        private static Transform? FindTransform(string objectName)
        {
            var found = GameObject.Find(objectName);
            return found == null ? null : found.transform;
        }

        private void EnsurePlayerSword()
        {
            if (_playerSword is not null || _playerMarker is null)
            {
                return;
            }

            var swordObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            swordObject.name = "M2_PlayerSlashSword";
            swordObject.transform.SetParent(_playerMarker, worldPositionStays: false);
            swordObject.transform.localPosition = new Vector3(0.55f, 0.30f, 0.45f);
            swordObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, -75.0f);
            swordObject.transform.localScale = new Vector3(0.08f, 0.85f, 0.08f);

            if (swordObject.TryGetComponent<Collider>(out var collider))
            {
                collider.enabled = false;
            }

            if (swordObject.TryGetComponent<Renderer>(out var renderer))
            {
                var block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);
                block.SetColor(BaseColorPropertyName, new Color(0.95f, 0.86f, 0.55f));
                block.SetColor(ColorPropertyName, new Color(0.95f, 0.86f, 0.55f));
                renderer.SetPropertyBlock(block);
            }

            swordObject.SetActive(false);
            _playerSword = swordObject.transform;
        }

        private void TriggerPlayerSwordSwing()
        {
            EnsurePlayerSword();
            if (_playerSword is null)
            {
                return;
            }

            if (_playerSwordSwing is not null)
            {
                StopCoroutine(_playerSwordSwing);
            }

            _playerSwordSwing = StartCoroutine(SwingPlayerSword());
        }

        private System.Collections.IEnumerator SwingPlayerSword()
        {
            if (_playerSword is null)
            {
                yield break;
            }

            var sword = _playerSword;
            var start = Quaternion.Euler(0.0f, 0.0f, -75.0f);
            var end = Quaternion.Euler(0.0f, 0.0f, 105.0f);
            const float duration = 0.15f;

            sword.gameObject.SetActive(true);
            sword.localRotation = start;

            var elapsed = 0.0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                t = 1.0f - Mathf.Pow(1.0f - t, 3.0f);
                sword.localRotation = Quaternion.Slerp(start, end, t);
                yield return null;
            }

            sword.localRotation = start;
            sword.gameObject.SetActive(false);
            _playerSwordSwing = null;
        }

        private void FollowCamera()
        {
            if (_camera is null || _playerMarker is null)
            {
                return;
            }

            var yawRotation = Quaternion.Euler(0.0f, _cameraYawDegrees, 0.0f);
            var followOffset = yawRotation * new Vector3(0.0f, CameraFollowHeight, -CameraFollowBack);
            _camera.transform.SetPositionAndRotation(
                _playerMarker.position + followOffset,
                Quaternion.Euler(CameraPitchDegrees, _cameraYawDegrees, 0.0f));
        }

        private void ApplySceneVisualState()
        {
            if (ShouldSuppressLegacyM2DuringObjectiveFreeWalk())
            {
                return;
            }

            ApplyColor(_floor, new Color(0.19f, 0.20f, 0.21f));
            ApplyColor(_playerMarker, new Color(0.25f, 0.70f, 1.0f));
            ApplyColor(_campRestPoint, new Color(0.12f, 0.55f, 0.32f));
            ApplyColor(_pullLane, new Color(0.16f, 0.16f, 0.18f));
            ApplyColor(_baselineTrash, _hostile is not null && _hostile.IsAlive
                ? new Color(0.75f, 0.16f, 0.16f)
                : new Color(0.22f, 0.22f, 0.24f));
            ApplyColor(_linkedTrash, _linkedHostile is not null && _linkedHostile.IsAlive
                ? new Color(0.78f, 0.24f, 0.12f)
                : new Color(0.22f, 0.22f, 0.24f));
            ApplyColor(_namedBlocker, _namedHostile is not null && _namedHostile.IsAlive
                ? new Color(0.42f, 0.10f, 0.46f)
                : new Color(0.22f, 0.22f, 0.24f));
        }

        private void ApplyPresentationSettings()
        {
            if (ShouldSuppressLegacyM2DuringObjectiveFreeWalk())
            {
                return;
            }

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
            _camera.fieldOfView = CameraFieldOfViewDegrees;
        }

        private void ApplyColor(Transform? target, Color color)
        {
            if (target is null || !target.TryGetComponent<Renderer>(out var renderer))
            {
                return;
            }

            if (renderer.sharedMaterial is null)
            {
                return;
            }

            _materialPropertyBlock ??= new MaterialPropertyBlock();
            renderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetColor(BaseColorPropertyName, color);
            _materialPropertyBlock.SetColor(ColorPropertyName, color);
            renderer.SetPropertyBlock(_materialPropertyBlock);
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

        private void UpdateLinkedHostileScale()
        {
            if (_linkedTrash is null || _linkedHostile is null || _linkedHostile.MaxHealth <= 0)
            {
                return;
            }

            var healthRatio = Mathf.Clamp01((float)_linkedHostile.CurrentHealth / _linkedHostile.MaxHealth);
            _linkedTrash.localScale = new Vector3(0.8f, Mathf.Lerp(0.35f, 1.0f, healthRatio), 0.8f);
        }

        private void RecordEvent(string eventName)
        {
            _events.Add($"{CurrentTick().Index}:{eventName}");
        }

        private void RecordOverpullEvent(string eventName)
        {
            _overpullEvents.Add($"{CurrentTick().Index}:{eventName}");
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
