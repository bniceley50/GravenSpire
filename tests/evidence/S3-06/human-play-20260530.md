# S3-06 Human-Play Feel Check

**Story:** `production/stories/s3-06-playable-end-to-end-and-human-play.md`
**AC refs:** S3-06-07, S3-06-08, S3-06-09, S3-06-10
**Date:** 2026-05-30
**Played by:** Brian (project lead), N=1 self-test
**Build SHA:** `codex/s3-06-playable-end-to-end-and-human-play` working tree after the S3-06 mechanical pass; branch head before local evidence updates was based on `cc7da60`
**Verdict:** NOT PASSED - human-play feel gate failed for presentation-readability reasons

## What Was Attempted

The project lead opened the assembled S3-06 branch after AC-01 through AC-06 had mechanical runner evidence and attempted the Tier-1 objective loop as a real player:

1. Spawn in the S3-05 district.
2. Find and interact with `M3_Caretaker` to accept the objective.
3. Find and interact with `M3_ObjectiveRelic` to recover the relic and resolve objective loot.
4. Find and interact with `M3_CourtVendor` to sell salvage.
5. Return to `M3_Caretaker` and hand in the relic.

## What Was Found

The mechanical loop existed, but the playable read failed. The slice read as Unity greybox/debug scaffolding rather than a playable classic-MMO-descended gothic slice. A concrete defect contributed to that read: the legacy M2 combat debug HUD was visible over the S3 objective-loop view. That HUD bleed is kept as a scoped S3-06 presentation-readiness bug fix, but it does not solve the broader presentation-readability gap recorded by the playtest and main-lane D020.

**Protocol question:** Would you do that again right now?

**Answer:** No - not as a feel-pass verdict for the current presentation state.

**If yes, why? If no, what would change that?**

> No full verbatim transcript was captured in the playtest evidence before this record was filled. The contemporaneous session record describes the reaction as the slice looking like Unity greybox/debug scaffolding rather than a playable classic-MMO-descended gothic slice, with the M2 combat debug HUD bleed called out as a real bug.

## Re-Engagement Attribution Test

PASS requires both:

- The playtester answers yes to immediate re-engagement.
- The verbatim reason names the objective, NPC, relic, or other world element as the reason, not raw XP, copper, testing, completionism, or "the game told me to."

FAIL if either:

- The answer is no.
- The answer is yes but the reason is mechanical reward, meta testing, completionism, or an external obligation.

**Computed verdict:** FAIL / NOT PASSED. The answer does not satisfy AC-08's immediate re-engagement requirement, and the reason does not name the objective, NPC, relic, or another world element as the pull to continue. The blocking issue is presentation-readability, not missing AC-01 through AC-06 mechanical proof.

## Presentation Limitations

| Finding | Classification | Reason | Counts against AC-08? |
|---|---|---|---|
| Overall slice reads as Unity greybox/debug scaffolding, not a playable classic-MMO-descended gothic slice | BLOCKING presentation-readability gap | This is broader than missing finished art: camera/HUD/interaction/environment read are not yet carrying the intended EQ-readable gothic slice. Main-lane D020 routes the answer to art-bible revision plus Sprint 4, not in-story polish. | Yes |
| Legacy M2 combat debug HUD bleeds over the S3 objective-loop view | Scoped bug fix / presentation-readiness defect | This was a concrete defect contaminating the playtest read. The fix is kept in `M2SingleTrashMedLoopController.cs` as a narrow S3-06 presentation-readiness bug fix. | Yes for the failed attempt; no as a future pass claim without a new human-play run |
| Produced-art fidelity is still below final target | Tolerable by itself | Greybox visuals alone would not fail AC-08 under D016; the failure here is the stronger presentation-legibility/readability gap recorded above. | No by itself |

R-P2-FEEL-MISATTRIBUTION rule: greybox aesthetic deficits are tolerable under D016 and do not count against the loop-feel verdict. Loop-mechanical deficits such as getting lost, anticlimactic relic recovery, unclear agency, or lack of world-investment do count.

2026-05-30 clarification: this failure is not mere "the relic looked bad" polish. It is a presentation-readability failure strong enough that D020 now routes the project through an art-bible revision and Sprint 4 EQ-readable presentation slice before Save/Load or faction-consequence work.

## Methodological Limit

On a solo project, the playtester may be the designer and implementer - an N=1 self-test with known selection bias. The verdict is recorded under this constraint.

## Playtester Verbatim Feedback

> No full verbatim transcript was captured. Session summary: the slice read as Unity greybox/debug scaffolding rather than a playable classic-MMO-descended gothic slice; the legacy M2 combat debug HUD bleeding over S3 objective play was a real bug and is kept as a scoped S3-06 presentation-readiness fix. The HUD fix does not turn the feel gate into PASS.

## Second-Playtester Read

Absent. No second playtester read was captured for this S3-06 verdict.
