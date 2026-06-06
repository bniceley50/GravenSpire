# Art-Bible Revision Prompt - D020 EQ-Readability Pivot

Paste this into a fresh main-lane session when ready. This is a prompt for the
art-bible revision session; it is not the revision itself.

```text
Art-Bible Revision - D020 EQ-Readability Pivot (main lane)

GOAL:
Revise design/art/art-bible.md to amend pillar P1 ("The World Is Not Your
Story") and adopt more EQ-style PLAY LEGIBILITY - "readability, not cosplay" -
per DECISIONS.md D020 (Locked, 2026-05-30). This is the deliberate follow-up
D020 triggered; it is the most important creative decision left in the project.

WHY THIS EXISTS:
S3-06 assembled the Tier-1 vertical-slice loop and proved it mechanically
(AC-01-06), but the N=1 human-play attempt FAILED the feel gate: the slice read
as Unity greybox/debug scaffolding, not a playable classic-MMO-descended gothic
slice. The product owner, after playing, decided the current art bible's pure
"world does not perform for the player" stance is too austere to ship - he wants
more EQ-style legibility than the bible currently allows.

START BY READING (do not skip - look before you assert):
- DECISIONS.md D020 (the authority, exact decision, and recorded risks).
- design/art/art-bible.md - the REVISION-PENDING banner at top, then in full:
  Section 1 (Visual Identity / P1), Section 5 (Character Design - silhouette
  legibility), and Section 7 (UI/HUD Direction). These three are currently
  frozen-for-revision per the banner.
- tests/evidence/S3-06/human-play-20260530.md - the actual feel-fail verdict
  and presentation-readability classification (what specifically did not read).
- design/gdd/game-concept.md - the core EQ-classic gothic-political-sandbox
  identity this revision must NOT betray.

THE WORK (run THROUGH the art-director agent - this is its domain):
Resolve, section by section, exactly where the legibility line MOVES. The line
moves; it does not disappear. For each of Sections 1/5/7, decide and rewrite:
- What player-service legibility is now ALLOWED that P1 previously forbade?
  Candidates: target/combat readability, a structured-but-quiet HUD, clearer
  interaction feedback, readable character/faction silhouettes at play distance.
- What stays FORBIDDEN as theme-park guidance?
  Quest markers, objective arrows, minimap pins, glowing waypoints, and "go here
  / do this next" routing remain forbidden. The premortem-distinct identity is
  that the world does not route the player.
- Rewrite the amended pillar text for P1's VISUAL expression, and reconcile the
  downstream consequences in Section 5 (silhouette can now communicate more?)
  and Section 7 (HUD is no longer "invisible by design"?).

HARD CONSTRAINTS:
- "EQ READABILITY, NOT EQ COSPLAY." The pivot is about making the slice legible
  and playable, NOT about becoming EverQuest to court the EQ crowd. D020 records
  the premortem #5/#7 audience risk: the anti-performing-world identity is part
  of what makes Gravenspire distinct. Hold that line - readability serves the
  player; it does not surrender the world's indifference.
- T1 SCOPE: this is a design-doc revision only. No code, no Unity, no Sprint 4
  stories yet. Sprint 4 planning is a SEPARATE session AFTER this revision lands.
- The art bible's own author left gates against exactly this pressure (the
  "bigger dopamine signal" production-note arguments). Engage those arguments
  honestly; amend deliberately, do not bulldoze them.

PROCESS:
- Default READ-ONLY until explicit EDIT_OK (root contract). Propose the amended
  pillar text and section rewrites for review BEFORE writing.
- This is a main-lane / forbidden-zone doc (art-bible is off-limits to Codex).
- When the revision is approved and written, LIFT the REVISION-PENDING banner
  (or convert it to a "revised per D020 on [date]" record), and note that
  Sprint 4 slate authoring is now unblocked.

DEFINITION OF DONE:
- Sections 1/5/7 rewritten with the amended P1 legibility line, reviewed,
  approved, and written.
- The allowed-vs-forbidden legibility boundary is explicit and unambiguous
  (a future presentation-story author can tell what they may and may not build).
- Banner resolved; D020's first trigger satisfied; Sprint 4 planning unblocked.
- "Readability not cosplay" demonstrably held - the gothic-political identity
  survives the revision intact.
```

## Notes For The Fresh Session

- Start from evidence, not screenshots or vibes: D020, S3-06 human-play evidence,
  the frozen art-bible sections, and the game concept.
- Keep the task framed as "where does the line move?" rather than "make it look
  like EQ."
- Defer Sprint 4 story authoring until after the art-bible revision lands.
- The goal is a clearer playable read, not a new theme-park guidance system.
