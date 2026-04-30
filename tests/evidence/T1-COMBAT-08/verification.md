# T1-COMBAT-08 Verification

## Targeted Test Command

```powershell
dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-08-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-08"
```

Result: PASS, 106 total, 106 passed, 0 failed.

TRX counter: `tests/evidence/T1-COMBAT-08/t1-combat-08-stage2.trx:654`.

## Acceptance Coverage Anchors

- `H-CCOM-HUD-01`: `CombatHudStateSnapshot` exposes health, mana, target, cast, Attack ON/OFF, next swing readiness, categorical threat, and combat state at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:52`; `Project` fills those fields at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:91`; integration coverage starts at `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:14`.
- `H-CCOM-HUD-02`: the projection file is gameplay-side only and imports no UI/rendering namespaces; snapshot coverage starts at `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:14`, and event-stream coverage starts at `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:52`.
- `H-CCOM-HUD-03`: threat output is categorical through `CombatHudThreatCategory` at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:10` and `src/gameplay/combat/presentation/CombatHudStateProjection.cs:59`; category evaluation starts at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:126`; categorical assertions start at `tests/unit/gameplay/combat/combat_hud_threat_category_test.cs:13`, with a sample `CombatHudThreatCategory.ThreatClose` assertion at `tests/unit/gameplay/combat/combat_hud_threat_category_test.cs:50`.
- `H-CCOM-HUD-04`: Attack signal projection starts at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:110`; event-stream projection starts at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:118`; table-driven transition coverage starts at `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:52`, `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:84`, `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:98`, and `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:110`.

## UI Seam Grep

Command:

```powershell
rg -n "using UnityEngine|MonoBehaviour|VisualElement|Color|Image|Sprite|TextMeshPro|Canvas|RectTransform|Animator|UnityEngine\.UI|UnityEngine\.UIElements" src\gameplay\combat\presentation\CombatHudStateProjection.cs
```

Result: PASS, zero matches. `CombatHudStateProjection.cs` imports only `System` namespaces and defines records/enums, not UI/rendering objects.

## Raw-threat Exposure Grep

Command:

```powershell
rg -n "RawThreat|ThreatValue|ThreatPercent|ThreatFill|ThreatBar|DisplayedThreat|ThreatNumber" src\gameplay\combat\presentation\CombatHudStateProjection.cs tests\unit\gameplay\combat\combat_hud_threat_category_test.cs tests\integration\gameplay\combat\combat_hud_state_signal_test.cs
```

Result: PASS, zero matches. HUD output carries `CombatHudThreatCategory`, not raw numeric threat. Sample non-numeric assertion: `Assert.That(close, Is.EqualTo(CombatHudThreatCategory.ThreatClose));` at `tests/unit/gameplay/combat/combat_hud_threat_category_test.cs:50`.

## Hardcoded-tuning Gate

Command:

```powershell
rg -n "[0-9]+(\.[0-9]+)?[dDfFlLmM]?" src\gameplay\combat\presentation\CombatHudStateProjection.cs
```

Result: PASS. Numeric literal hits are limited to zero/one guards, neutral defaults, and collection index access:

- `src/gameplay/combat/presentation/CombatHudStateProjection.cs:139`
- `src/gameplay/combat/presentation/CombatHudStateProjection.cs:151`
- `src/gameplay/combat/presentation/CombatHudStateProjection.cs:157`
- `src/gameplay/combat/presentation/CombatHudStateProjection.cs:159`
- `src/gameplay/combat/presentation/CombatHudStateProjection.cs:165`
- `src/gameplay/combat/presentation/CombatHudStateProjection.cs:166`
- `src/gameplay/combat/presentation/CombatHudStateProjection.cs:181`
- `src/gameplay/combat/presentation/CombatHudStateProjection.cs:231`
- `src/gameplay/combat/presentation/CombatHudStateProjection.cs:258`
- `src/gameplay/combat/presentation/CombatHudStateProjection.cs:263`

No threat threshold tuning value is hardcoded in production. `CombatHudThreatCategoryTuning` is supplied by the caller at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:68`, and the projection validates only ratio bounds at `src/gameplay/combat/presentation/CombatHudStateProjection.cs:255`.

## Negative T1 Scope Grep

Command:

```powershell
rg -n -i "FishNet|networking|network|server authority|server|PvP|companion|Warrior|Enchanter|OpenAI|Anthropic|DateTime|UtcNow|DateTime\.Now|Time\.deltaTime|deltaTime|System\.Random" src\gameplay\combat\presentation\CombatHudStateProjection.cs tests\unit\gameplay\combat\combat_hud_threat_category_test.cs tests\integration\gameplay\combat\combat_hud_state_signal_test.cs
```

Result: PASS, zero matches across the three approved new `.cs` files.

## Composition Verification

- Snapshot projection reads existing actor resource/current-state surfaces: `CurrentHealth`, `MaxHealth`, `CurrentMana`, `MaxMana`, and `CombatState` at `src/gameplay/combat/CombatActorState.cs:397`, `src/gameplay/combat/CombatActorState.cs:402`, `src/gameplay/combat/CombatActorState.cs:407`, `src/gameplay/combat/CombatActorState.cs:412`, and `src/gameplay/combat/CombatActorState.cs:457`; projection read sites are `src/gameplay/combat/presentation/CombatHudStateProjection.cs:98`, `src/gameplay/combat/presentation/CombatHudStateProjection.cs:99`, and `src/gameplay/combat/presentation/CombatHudStateProjection.cs:107`.
- Target and threat category projection reads existing transient target and threat table state: `TargetCombatActorId` and `ThreatTable` at `src/gameplay/combat/CombatActorState.cs:467` and `src/gameplay/combat/CombatActorState.cs:477`; read sites are `src/gameplay/combat/presentation/CombatHudStateProjection.cs:100`, `src/gameplay/combat/presentation/CombatHudStateProjection.cs:139`, and `src/gameplay/combat/presentation/CombatHudStateProjection.cs:150`.
- Attack state projection reuses existing Attack state-machine outputs: `CombatAttackStateSnapshot`, `CombatAttackStateChangedSignal`, `CurrentState`, and `StateChangedSignals` at `src/gameplay/combat/attack/CombatAttackStateMachine.cs:49`, `src/gameplay/combat/attack/CombatAttackStateMachine.cs:61`, `src/gameplay/combat/attack/CombatAttackStateMachine.cs:97`, and `src/gameplay/combat/attack/CombatAttackStateMachine.cs:100`; projection read sites are `src/gameplay/combat/presentation/CombatHudStateProjection.cs:102`, `src/gameplay/combat/presentation/CombatHudStateProjection.cs:103`, `src/gameplay/combat/presentation/CombatHudStateProjection.cs:110`, `src/gameplay/combat/presentation/CombatHudStateProjection.cs:118`, and `src/gameplay/combat/presentation/CombatHudStateProjection.cs:235`.
- Cast state projection composes with existing actor cast fields and cast progress snapshots: `CastRuntimeState`, `ActiveCastSpellId`, and `CastRecoveryRemainingSeconds` at `src/gameplay/combat/CombatActorState.cs:482`, `src/gameplay/combat/CombatActorState.cs:492`, and `src/gameplay/combat/CombatActorState.cs:507`; `CombatCastProgressSnapshot` and `GetProgress` are at `src/gameplay/combat/casting/CombatCastStateMachine.cs:57` and `src/gameplay/combat/casting/CombatCastStateMachine.cs:130`; projection read sites are `src/gameplay/combat/presentation/CombatHudStateProjection.cs:101` and `src/gameplay/combat/presentation/CombatHudStateProjection.cs:216`.

## H-CCOM-HUD-04 Table Coverage

- Attack on signal emits: `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:52`.
- Attack off signal emits: `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:65`.
- Target death, successful sit, combat exit, player death, and zone transition reflect/emit off: table-driven test starts at `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:79`.
- Current-state accessor matches event history: `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:98`.
- No misleading transient Attack ON pulse on no-target no-op: `test_no_target_toggle_noop_emits_no_misleading_attack_on_pulse` at `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:110`, with null/empty signal assertions at `tests/integration/gameplay/combat/combat_hud_state_signal_test.cs:125`.

## Hygiene

- `bash .githooks/pre-commit`: PASS, `[pre-commit] OK`.
- `git diff --check`: PASS.
- `git diff --check --no-index -- /dev/null <new-approved-file>`: PASS for the five approved new text files. Git emitted LF/CRLF working-copy warnings only; no whitespace/conflict-marker errors.
- Staging area: empty during final verification.
