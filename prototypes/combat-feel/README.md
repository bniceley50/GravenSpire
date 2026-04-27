// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can Cleric tab-target combat, slow cast cadence, mana pressure, and med-break recovery make the silence between pulls feel intentional rather than empty?
// Date: 2026-04-26

# Combat Feel Prototype

## Prototype Question

Can a minimal Cleric-only tab-target combat loop make Classic EQ pacing feel
good in 2026: auto-attack ticks, slow spell casts, mana pressure, med-break
recovery, and quiet time between pulls that feels like preparation rather than
dead air?

## Tech Choice

Chosen option: Unity 6.3 LTS standalone prototype scene.

Rationale: the question is about feel, not only timing math. The prototype must
exercise visual cadence: cast bar fill rate, target frame readability, combat
log pacing, mana bar pressure, and downtime presentation. Web or terminal
versions would be faster, but their findings would overstate confidence because
they would not test Unity runtime UI/input/timing behavior.

## Minimum Build

- One Cleric actor with health, mana, auto-attack, Smite, Heal, sit, and med.
- Three to five sequential hostile pulls in a haunt-like space.
- Default cadence knobs force at least one med break between pulls 2 and 3.
- Player can pull again if ready, or sit and med if mana/health are low.
- Pull starts combat without auto-swinging. Press Attack to begin melee.
- Loop ends when five pulls are complete, the player stops, or the player dies.
- Runtime HUD uses UI Toolkit generated from code: player frame, target frame,
  cast bar, controls, combat log, and prototype metrics.

## Success Criteria

Project unblocked toward T1 sprint planning if self-playtest finds all of:

- Med breaks read as tense or useful in 3+ pulls.
- Pull duration lands in the 15-45 second band without feeling padded.
- Downtime between pulls reads as preparation: choosing next target, watching
  risk, or deciding whether to spend time medding.
- Mana pressure is real: spell budgets create tactical choices rather than
  passive number watching.

## Failure Criteria

Project requires reframing if self-playtest finds any of:

- Med breaks consistently feel like punishment or empty time after 5+ pulls.
- Cadence must be sped up substantially to feel engaging, undermining Pillar 2:
  The Silence Is Sacred.
- The between-pull rhythm feels like 2026 friction rather than 1999 patience.

## Inconclusive Criteria

If feel is mixed, run one second iteration with exactly one changed knob. The
first candidate knob is auto-attack interval; the second is mana drain/recovery
rate. Document which knob moved the feel.

## Controls

- Tab: cycle preview target while between pulls.
- 1: pull selected hostile.
- A: toggle auto-attack on or off during combat.
- Q: cast Smite.
- E: cast Heal during combat or between pulls.
- 2: Smite of Authority instant damage.
- 3: Bash instant melee damage and brief interrupt.
- 4: Defensive Prayer instant self-buff.
- R: sit or stand to meditate.
- X: stop the prototype loop.

The same actions are available as HUD buttons.

## V2 Tactical Instants Iteration

This iteration tests the Read A agency question: does the EQ-baseline loop need
more tactical buttons, rather than faster manual melee?

- Smite of Authority: instant ranged filler, light damage, 7s cooldown, low
  mana cost.
- Bash: instant melee hit, low damage, 10s cooldown, low mana cost, briefly
  delays the next hostile swing.
- Defensive Prayer: instant self-buff, 20% damage reduction for 8s, 30s
  cooldown, meaningful mana cost.

Keep all baseline success criteria and add:

- Tactical instants criterion: choosing between channeled Smite, instant filler,
  Bash, and Defensive Prayer should make the loop feel intentional, not just
  busier.
- Pacing preservation criterion: med breaks must still feel necessary after 3-5
  pulls with full ability use. If instants erase sitting, the iteration failed.

Comparison protocol:

1. Run the baseline build from commit `3a49df9` if available and note feel.
2. Run this v2 build with instants and the same default knobs.
3. Compare whether v2 is meaningfully more intentional or merely more busy.

## Easiest Run Path

Use the batch files from Windows Explorer:

1. Double-click `BUILD_COMBAT_FEEL.bat`.
2. Double-click `RUN_COMBAT_FEEL.bat`.

The build script prefers Unity 6000.3.x if installed, then falls back to
6000.4.x for advisory practice builds.

Manual playtest metrics are written on Stop (`X`) or five-pull completion to
`Logs/playtest-<timestamp>.log` as one JSON line. `Logs/` is ignored by git; copy
the relevant line into `production/prototypes/combat-feel-report.md` when a run
is evidence-bearing.

## File Map

- `Assets/Scripts/PrototypeBootstrap.cs` - Unity entry point and UI bootstrap.
- `Assets/Scripts/CadenceKnobs.cs` - default timing, mana, damage, and pull
  requirements exposed in the Inspector.
- `Assets/Scripts/ClericActor.cs` - minimal Cleric state and resource methods.
- `Assets/Scripts/HostileActor.cs` - haunt hostile definitions and damage.
- `Assets/Scripts/CombatLoop.cs` - pull sequence, casting, medding, metrics,
  and combat log events.
- `Assets/Scripts/Editor/CombatFeelSceneBuilder.cs` - editor-only menu command
  that creates a blank scene with the bootstrap object attached.
- `Assets/Scripts/Editor/CombatFeelBuildRunner.cs` - editor-only Windows
  standalone build command used by `BUILD_COMBAT_FEEL.bat`.
- `Assets/Scripts/Editor/CombatFeelSmokeRunner.cs` - editor-only scripted
  mechanics smoke check for multi-pull completion and forced med break.
- `BUILD_COMBAT_FEEL.bat` - command-line Windows build helper.
- `RUN_COMBAT_FEEL.bat` - launches the generated Windows player.

## Run Notes

This folder is throwaway prototype code. It does not import from production
source files and production code must not import from it.

To run in Unity, open or create a throwaway Unity 6.3 LTS project from this
folder, then use `Gravenspire/Prototypes/Combat Feel/Create Prototype Scene`.
If Unity regenerates project metadata, keep that metadata inside this prototype
folder and do not move any files into production `src`.

Engine version note: Scaffold and scene generation were verified on Unity
6000.4.1f1 because the pinned editor, Unity 6000.3.x LTS, was not available on
the scaffolding machine. Open the scene in pinned Unity 6000.3.x for
self-playtest findings to count against the project engine pin per
`docs/engine-reference/unity/VERSION.md` and `DECISIONS.md` D001. Findings
under any other Unity version are advisory only.

## Report Target

Durable findings should be written after self-playtest to:

`production/prototypes/combat-feel-report.md`
