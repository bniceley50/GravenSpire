# S2-M3-04 Human-Play Notes — End-To-End Objective Loop

**Story:** `production/stories/s2-m3-04-end-to-end-objective-loop.md`
**Acceptance Criterion:** `S2-M3-04-06`
**Date played:** 2026-05-20
**Played by:** project lead

## What was attempted

The lead opened `Assets/Scenes/_DevEntry.unity` in the Unity Editor, entered
Play Mode, and attempted the human-play: play the M3 objective loop end to end
and judge "did the objective give a real reason to do one more pull?"

## What was found

The M2 combat loop is interactively playable — the lead played a trash pull
(pull -> target -> attack -> Smite -> kill -> med-break; the loop advanced to
Pulls 1/2). The M3 objective layer is **not player-interactive**: the named
NPC, objective acceptance, relic recovery, hand-in, looting, and the vendor
are driven only by the verification runner — no player input path exists
(`Input.GetKey` appears in exactly one file, `M2SingleTrashMedLoopController.cs`).
There is no navigable world (only a `FirstDistrict_ShellOnly_NoGameplay`
blockout shell + flat floor) and no art.

## The one-more-pull question

**Could not be validated by human play.** The objective loop the question is
about is not something a human can drive in this build. The honest answer to
"did the objective give a real reason to do one more pull?" is **not
determinable in the current blockout** — the objective is neither communicated
to nor driven by the player.

## Presentation limitations (classified, not hidden)

| Limitation | Effect on feel-validation |
| --- | --- |
| M3 objective layer has no player input (NPC / relic / hand-in / loot / vendor are runner-only) | BLOCKING — the loop cannot be experienced as a loop |
| No navigable world — only a blockout shell + flat floor | BLOCKING — no space to traverse |
| No art (capsule actors, untextured surfaces, debug-text HUD) | Tolerable in principle, compounds the above |
| The objective is never shown to the player (no objective HUD, no NPC dialogue surface) | BLOCKING — the player cannot know the objective exists |

## Lead's verbatim feedback

> "hard to play and see if things work when it looks like this? why is there
> no world to play in yet? where is the artwork, where is the npc that gives
> quests how do you loot etc... cant test garbage like this and expect any
> kindve feedback from me its not even playable in this form"

## Verdict

Human-play could not validate the one-more-pull question because the M3
objective loop is not player-interactive in the blockout build. Per lead
decision (2026-05-20), AC-06's feel-validation **transfers to the Sprint 3
"Playable Vertical-Slice Assembly" milestone**, where the M3 systems will be
wired behind player input inside a navigable greybox district. AC-06's
documentation requirement — notes captured, limitations classified rather
than hidden — is satisfied by this record.
