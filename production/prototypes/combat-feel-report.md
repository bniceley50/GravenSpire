# Combat-Feel Prototype Report

> Status: ADVISORY POSITIVE - proceed to pinned-engine validation plus one-knob casting iteration
> Date: 2026-04-26
> Prototype scaffold commit: `7add6ee`
> Heal fix + launcher commit: same bundle as this report
> Reporter: brian (single playtester)

## Question

Does a minimal Cleric tab-target loop make Classic EQ pacing feel intentional in
2026: auto-attack ticks, slow spell casts, mana pressure, med-break recovery,
and quiet time between pulls that feels like preparation rather than empty
waiting?

This report covers the first hands-on validation of the project risk called out
in `design/gdd/game-concept.md`: whether EQ-classic combat still feels good in
2026. It is not final proof. It is the first evidence-backed read.

## Run Conditions

- Engine: Unity `6000.4.1f1`, not pinned Unity `6000.3.x LTS`.
- Evidence status: advisory only until repeated under pinned `6000.3.x`.
- Run mode: Windows standalone build through `BUILD_COMBAT_FEEL.bat`.
- Session count: 2.
- Session 1: 1 pull, stopped early after HUD fallback validation.
- Session 2: 3+ pull advisory playtest after Heal fix.
- Cadence knobs: default prototype values.
- Playtester: brian, solo.

## Findings Against README Success Criteria

| Criterion | Direct evidence | Inferred read |
|---|---|---|
| Med breaks feel tense/useful | Not directly isolated in verbal feedback. | Implied positive: "everything else felt pretty smooth." |
| Pull duration 15-45s without padding | First-run metric: 19.7s/pull, inside target band. Post-fix smoke: 22.7s average pull. | Positive; playtester did not report "too slow" or "too long." |
| Downtime as preparation, not empty | Not directly isolated in verbal feedback. | Implied positive; no "empty," "boring," or "annoying" report. |
| Mana pressure as tactical choice | Not directly isolated in verbal feedback. | Positive but tuneable; playtester framed cadence as adjustable, especially casting time. |

## Bugs Caught During Playtest

- **Standalone build initially rendered black.** Fixed by adding an IMGUI
  fallback HUD in `PrototypeBootstrap.cs`. The standalone path is now usable
  without learning the Unity editor.
- **Heal button appeared non-functional.** Root cause: Heal only worked during
  `Fighting` state and silently returned in invalid states. Fixed in
  `CombatLoop.cs` so Heal can cast during combat or between pulls, and invalid
  presses explain themselves. `PrototypeBootstrap.cs` now enables Heal when the
  Cleric is missing health during combat or between pulls.
- **Post-fix smoke validation passed.** Scripted smoke completed 5/5 pulls with
  113.3s combat, 35.7s downtime, 22.7s average pull, 1 med break, 8 Smites, 5
  Heals, and 0 unsafe pulls.

Usability bugs can mask pacing reads. The smoke runner caught mechanical regen
bugs before playtest, but it did not catch the Heal usability surface. The
lesson is complementary: smoke validates machinery; hands-on playtest validates
what the player can actually understand and use.

## Manual Melee / Agency Question

After the advisory-positive read, the playtester asked whether the prototype
could have an option for manual melee instead of only auto-attack.

This should not be folded into the baseline result. Spammable manual melee would
test a different question and risks undermining the Classic EQ pacing premise.
The likely underlying need is more hands-on agency, not necessarily faster
combat.

Recommended v2 interpretation: test **tactical Cleric instant abilities** rather
than spammable manual melee. Examples: instant Smite on cooldown, short-range
Bash, or Defensive Prayer. This preserves auto-attack and med-break pacing while
adding moment-to-moment choice. If the intent is full click-to-swing action
combat instead, that is a project-direction conversation, not a small prototype
tweak.

## Verdict

**ADVISORY POSITIVE.**

The first hands-on read produced no fundamental feel objection to the
EQ-baseline loop. The playtester said the loop felt "pretty smooth" and framed
the main concern as tuning ("we can adjust settings on things like casting
time"), not as a rejection of the 2026 viability of slow tab-target combat.

This is positive evidence, but not authoritative evidence, because:

- The run used Unity `6000.4.1f1`, not pinned Unity `6000.3.x LTS`.
- The run had one playtester.
- Three of four success criteria were inferred rather than explicitly probed.
- The session exposed and fixed usability bugs during the run.

## Recommended Next Moves

1. Install or open pinned Unity `6000.3.x LTS`, reopen the scene, and rerun with
   the same default knobs. Probe all four README success criteria explicitly.
2. If the pinned-engine read still feels under-paced, run one-knob iteration on
   Smite cast time first. Document which knob moved the feel; do not tune
   multiple knobs at once.
3. If pinned-engine plus one-knob validation stays positive across 3+ pulls per
   criterion, treat T1 sprint planning as unblocked for combat-feel baseline.
4. If the second session reveals hidden failures, run a second iteration before
   closing the game-concept combat-feel risk.
5. Separately scope a v2 agency prototype around Cleric instant abilities if the
   manual-melee instinct persists after pinned-engine validation.

## Limitations Required Before T1 Sprint Planning Gate

- Pinned Unity `6000.3.x LTS` run not yet performed.
- Multi-session stability not validated.
- Other player perspectives not validated.
- Cleric prototype data is not aligned with production Combat Core fixtures.
- No game art or real haunt space; this is HUD-only, so visual feel may shift.
