---
paths:
  - "design/narrative/**"
---

# Narrative Rules

## Rule Set Name

Narrative Rules

## Mission

These rules govern all narrative design documents under:

```text
design/narrative/**
```

Their purpose is to keep the game’s story, lore, character voices, world rules, mysteries, factions, timelines, and player-facing narrative text internally consistent, localization-ready, spoiler-safe, and reviewable.

Narrative documents are creative source-of-truth records. They must distinguish approved canon from provisional ideas, hidden truth from player-facing knowledge, intentional ambiguity from accidental contradiction, and brainstorms from production narrative.

The core narrative question is:

> Does this narrative content fit established canon, preserve character voice, obey world rules, localize safely, and clearly state what is known, hidden, provisional, or under review?

---

## Operating Principles

1. **Canon is tracked explicitly**
   - Every lore entry must specify canon level:
     - Established
     - Provisional
     - Under Review

2. **Established canon is hard to change**
   - Established canon may only change through approved retcon or supersession.
   - Do not silently rewrite established lore.

3. **Provisional lore is not production truth**
   - Provisional lore can guide drafts.
   - It must not be treated as final canon.

4. **Under-review lore blocks downstream finalization**
   - Under-review lore must not be used as final dialogue, quest text, item text, or shipped narrative without approval.

5. **Cross-reference before adding lore**
   - New lore must be checked against existing lore, world rules, timelines, character profiles, faction docs, and mystery records.
   - If the check cannot be performed, mark the content `UNVERIFIED`.

6. **Contradictions must be intentional or fixed**
   - Accidental contradictions must be resolved.
   - Intentional contradictions must be labeled as:
     - unreliable narrator,
     - misinformation,
     - limited perspective,
     - mystery clue,
     - cultural belief,
     - propaganda,
     - or open question.

7. **World rules are binding**
   - What is possible, impossible, rare, forbidden, supernatural, technological, cultural, or exceptional must be explicitly documented.
   - Exceptions must have causes and consequences.

8. **Mysteries require true answers**
   - Every mystery must have a documented true answer, even if players never learn it.
   - Clues, red herrings, reveal timing, and spoiler levels must be tracked.

9. **Factions must be internally logical**
   - Faction motivations, resources, constraints, relationships, leadership, internal conflicts, and power structures must make sense.

10. **Character voice is a constraint**
    - Dialogue must match the character’s approved voice profile.
    - If the profile is missing, dialogue remains draft or blocked.

11. **Localization readiness is mandatory**
    - Avoid idioms that do not translate.
    - Use named placeholders for variables.
    - Provide translator context.
    - Keep dialogue lines within UI constraints.

12. **Dialogue line limit**
    - No line of dialogue should exceed:

```text
120 characters
```

   - Longer lines require split, rewrite, or explicit UI/layout exception.

13. **Self-healing before approval**
    - When content contradicts canon, lacks canon level, violates voice, exceeds line length, lacks mystery truth, or is not localization-ready, stop, classify, repair, verify, and report.

14. **Bounded self-learning**
    - Durable narrative lessons may be stored only in approved, reviewable locations.
    - Lessons must be evidence-backed, reversible, and subordinate to current creative direction and narrative-director approval.

---

## Scope

These rules apply to:

```text
design/narrative/**
```

This includes, where present:

- story bible,
- lore entries,
- world rules,
- character profiles,
- voice profiles,
- faction documents,
- timeline documents,
- mystery documents,
- quest narrative outlines,
- environmental storytelling plans,
- dialogue strategy documents,
- hidden-truth records,
- player-facing lore drafts,
- glossary and terminology documents,
- narrative localization notes,
- narrative QA records.

---

## Non-Goals

These rules do not authorize narrative documents to:

- make final story arc decisions without Narrative Director approval,
- override Creative Director vision,
- define gameplay mechanics,
- make UI layout decisions,
- write final localized translations,
- alter established canon silently,
- introduce new factions, cultures, or world rules without review,
- expose hidden spoilers to player-facing files without approval,
- edit files without the active agent’s approval workflow,
- store persistent lessons without approval.

---

## Narrative Lifecycle State Labels

Use these labels when reviewing or authoring narrative content:

```text
BRAINSTORM — exploratory idea, not canon.
DRAFT — written but not approved.
PROVISIONAL_CANON — usable for exploration, not final.
UNDER_REVIEW — blocked pending Narrative Director or owner review.
ESTABLISHED_CANON — approved canon.
PLAYER_VISIBLE — safe for player-facing text.
DISCOVERABLE — hidden initially but can be discovered by players.
HIDDEN_TRUTH — true in canon but not directly player-facing.
UNRELIABLE_ACCOUNT — in-world claim may be false or biased.
CONTRADICTION_FOUND — conflicts with existing canon/rules.
CONTRADICTION_RESOLVED — conflict reviewed and resolved.
RETCON_PROPOSED — change to established canon requested.
RETCON_APPROVED — retcon accepted and documented.
LOCALIZATION_READY — text has placeholders/context and avoids localization hazards.
VOICE_APPROVED — dialogue matches voice profile.
LINE_LENGTH_VERIFIED — dialogue lines pass length constraint.
BLOCKED — missing canon level, voice profile, truth answer, approval, or consistency check.
SUPERSEDED — replaced by newer narrative source.
DEPRECATED — no longer active but retained for reference.
```

### State Rules

- Do not mark content `ESTABLISHED_CANON` without approval.
- Do not mark content `PLAYER_VISIBLE` if it contains hidden truth not meant for players.
- Do not mark dialogue `VOICE_APPROVED` without a voice-profile check.
- Do not mark text `LOCALIZATION_READY` if it contains unnamed variables, fragile idioms, or missing context.
- Do not mark a mystery ready if the true answer is missing.
- `BRAINSTORM` and `DRAFT` are not canon.

---

## Source of Truth

Recommended narrative source files:

```text
design/narrative/story-bible.md
design/narrative/world-rules.md
design/narrative/timeline.md
design/narrative/characters/
design/narrative/voice-profiles/
design/narrative/factions/
design/narrative/mysteries/
design/narrative/lore/
design/narrative/glossary.md
design/narrative/localization-notes.md
design/narrative/contradictions.md
design/narrative/retcons.md
production/qa/narrative/
production/session-state/lessons.md
```

### Source-of-Truth Rules

- Check existing lore before adding new lore.
- Check world rules before adding supernatural, technological, political, historical, or cultural claims.
- Check timeline before adding historical events, ages, travel times, reigns, wars, migrations, or disasters.
- Check character profile before writing dialogue.
- Check voice profile before approving dialogue.
- Check faction docs before changing motivations, alliances, rivalries, power structures, or territories.
- Check mystery records before adding clues or reveals.
- If source files conflict, mark content `CONTRADICTION_FOUND`.

---

## Canon Levels

### Canon Level Definitions

```text
Established
Approved canon. Downstream teams may rely on it. Changes require retcon or supersession approval.

Provisional
Likely direction, but not final. May guide drafts and exploration. Must not be treated as final shipped truth.

Under Review
Open issue, contradiction, uncertain decision, or pending approval. Blocks finalization.
```

### Canon Level Record

```md
## Canon Status

- Canon level:
  - Established
  - Provisional
  - Under Review
- Approved by:
- Source:
- Date/session:
- Review trigger:
- Supersedes:
- Notes:
```

### Canon Rules

- Every lore entry requires canon level.
- Canon level must be visible near the top of the document.
- Established canon needs source and approval.
- Provisional canon needs review trigger.
- Under-review canon needs owner and open question.
- Do not cite provisional or under-review content as final truth.

---

## Lore Entry Standard

Every lore entry must include:

```md
# Lore Entry: [Name]

## Status

- Canon level:
- Player visibility:
  - Player Visible
  - Discoverable
  - Hidden Truth
  - Unreliable Account
- Owner:
- Source:
- Last updated:
- Related entries:
- Contradictions check:

## Summary

## Established Facts

## Provisional / Under Review Facts

## Player-Facing Knowledge

## Hidden Truth

## Cross-References

## World Rule Implications

## Timeline Implications

## Faction / Character Implications

## Localization Notes

## Open Questions
```

### Lore Entry Rules

- Do not mix objective canon with in-world belief without labeling.
- Separate player-facing knowledge from hidden truth.
- Cross-reference related entries.
- State whether contradictions were checked.
- If a lore claim changes a world rule, update world rules.
- If a lore claim affects timeline, update timeline.
- If a lore claim affects factions or characters, update those docs.

---

## Cross-Reference and Contradiction Check

### Cross-Reference Record

```md
## Narrative Cross-Reference Check

- New/changed content:
- Files checked:
- Related characters:
- Related factions:
- Related world rules:
- Related timeline events:
- Related mysteries:
- Related quests:
- Related glossary terms:
- Contradictions found:
- Verdict:
```

### Verdicts

```text
CONSISTENT
CONSISTENT_WITH_NOTES
CONTRADICTION_FOUND
UNVERIFIED
BLOCKED
```

### Contradiction Record

```md
## Narrative Contradiction

- ID:
- Status:
  - Found
  - Intentional
  - Resolved
  - Waived
  - Superseded
- Source A:
- Source B:
- Conflict:
- Impact:
- Proposed resolution:
- Owner:
- Approval:
```

### Contradiction Rules

- Accidental contradictions block approval.
- Intentional contradictions require explanation.
- If unreliable narration explains the conflict, mark it clearly.
- If a retcon resolves the conflict, document retcon.
- If conflict remains unresolved, mark affected content `UNDER_REVIEW`.

---

## Retcon and Supersession Policy

### Retcon Record

```md
## Retcon Record

- Retcon ID:
- Status:
  - Proposed
  - Approved
  - Rejected
  - Superseded
- Original canon:
- New canon:
- Reason:
- Affected files:
- Affected characters:
- Affected factions:
- Affected quests:
- Affected dialogue:
- Affected localization:
- Player-facing impact:
- Approval:
```

### Retcon Rules

- Retcons require Narrative Director approval.
- Retcons affecting pillars or game identity require Creative Director review.
- Retcons affecting gameplay, quests, or progression require Game Designer / Producer coordination.
- Retcons affecting shipped content require player-facing continuity review.
- Do not silently overwrite established canon.
- Preserve deprecated canon for traceability when useful.

---

## World Rule Standard

World rules define what is possible, impossible, rare, forbidden, or exceptional.

### World Rule Record

```md
## World Rule: [Rule Name]

- Canon level:
- Rule type:
  - Physical
  - Magical
  - Technological
  - Cultural
  - Political
  - Ecological
  - Religious
  - Economic
  - Metaphysical
- Statement:
- What is possible:
- What is impossible:
- Known exceptions:
- Cost/consequence:
- Evidence/source:
- Affected systems:
- Player-facing explanation:
- Hidden truth:
- Contradictions check:
```

### World Rule Rules

- Every world rule must define boundaries.
- Exceptions must be documented.
- Exceptions should have cost, rarity, cause, or consequence.
- World rules must not change casually to solve plot problems.
- If a scene violates a world rule, either revise the scene or approve an exception.
- Player-facing explanations can be incomplete, but hidden truth must be coherent.

---

## Mystery Design Standard

Mysteries must have documented true answers.

### Mystery Record

```md
## Mystery: [Mystery Name]

- Status:
- Canon level:
- Player-facing question:
- True answer:
- Hidden truth:
- Player-known facts:
- Clues:
- Red herrings:
- Unreliable accounts:
- Reveal timing:
- Reveal method:
- Spoiler level:
- Related entries:
- Contradiction risk:
- Owner:
```

### Clue Table

```md
| Clue | Location / Source | What player may infer | True relevance | Spoiler risk |
|---|---|---|---|---|
```

### Mystery Rules

- Every mystery has a true answer.
- Red herrings must be intentional.
- Clues must be consistent with the true answer.
- Unreliable accounts must be labeled.
- Reveal timing must be tracked.
- If the true answer changes, update all clues and red herrings.
- A mystery without a true answer is `BLOCKED`.

---

## Character and Voice Profile Standard

### Character Profile

```md
# Character: [Name]

## Status

- Canon level:
- Player visibility:
- Narrative function:
- First appearance:
- Owner:

## Core Identity

## Motivation

## Fear / Wound

## Desire

## Internal Conflict

## External Conflict

## Relationships

## Faction Links

## Timeline

## Secrets

## Player-Facing Knowledge

## Voice Profile Link

## Open Questions
```

### Voice Profile

```md
# Voice Profile: [Character Name]

## Status

- Canon level:
- Approved by:
- Last updated:

## Voice Summary

## Diction

## Rhythm

## Sentence Length

## Vocabulary

## Emotional Range

## Humor Style

## Formality Level

## Topics They Avoid

## Words / Phrases They Use

## Words / Phrases They Never Use

## Example Lines

## Anti-Example Lines

## Localization Notes

## Voice Actor Notes, if applicable
```

### Voice Rules

- Dialogue must match the voice profile.
- If voice profile is missing, dialogue remains draft.
- Characters should not all share the same rhythm or vocabulary.
- Voice exceptions require context:
  - stress,
  - deception,
  - injury,
  - altered state,
  - formal setting,
  - character growth.
- Voice drift must be flagged.

---

## Dialogue Standard

### Dialogue Line Record

```md
## Dialogue Line

- Line ID:
- Speaker:
- Context:
- Canon level:
- Player visibility:
- Conditions:
- Emotional direction:
- Line:
- Character count:
- Voice profile check:
- Localization notes:
- Variables:
- Follow-up / branch:
```

### Dialogue Rules

- Every line has speaker and context.
- Every line must match voice profile.
- No line should exceed 120 characters.
- Lines over 120 characters must be split, revised, or flagged for UI exception.
- Use named placeholders for variables:
  - `{player_name}`
  - `{item_count}`
  - `{location_name}`
- Do not use positional placeholders like `{0}` unless localization system requires it and context is documented.
- Avoid idioms that do not translate.
- Avoid grammar that depends on English-only word order when variables are inserted.
- Mechanical information must be unambiguous.
- Player-facing dialogue must not reveal hidden truth unless reveal timing allows it.

---

## Dialogue Line-Length Review

### Line-Length Record

```md
## Dialogue Line-Length Review

| Line ID | Speaker | Character Count | Limit | Status | Recommendation |
|---|---|---:|---:|---|---|
```

### Line-Length Rules

- Target maximum:

```text
120 characters
```

- Count visible characters after placeholder notation.
- If localized text may expand, shorter source lines are safer.
- UI exceptions require UI/UX review.
- Long lore prose may use different constraints if not shown in dialogue boxes, but must document display context.

---

## Faction Standard

### Faction Record

```md
# Faction: [Name]

## Status

- Canon level:
- Player visibility:
- Owner:
- Source:

## Core Identity

## Motivation

## Ideology

## Resources

## Territory

## Leadership

## Power Structure

## Internal Conflicts

## Allies

## Rivals / Enemies

## Relationship Map

## Methods

## Constraints

## Public Face

## Hidden Truth

## Player Relevance

## Timeline

## Contradictions Check

## Open Questions
```

### Faction Logic Rules

- Factions need understandable motivations.
- Power structures must explain who can make decisions.
- Resources must support faction behavior.
- Internal conflicts should be documented if they affect story.
- Relationships should be reciprocal unless asymmetry is intentional.
- Factions should not act against their interests without explanation.
- Sudden relationship changes require cause.

---

## Timeline and Continuity Rules

### Timeline Event Record

```md
## Timeline Event: [Event Name]

- Date / era:
- Canon level:
- Player visibility:
- Summary:
- Causes:
- Consequences:
- Characters involved:
- Factions involved:
- Locations involved:
- Related mysteries:
- Source:
- Contradictions check:
```

### Timeline Rules

- Events must have relative or absolute placement.
- Character ages, reigns, wars, travel times, and succession must remain coherent.
- Hidden events may exist, but they must still fit timeline logic.
- If timeline is intentionally uncertain, state why:
  - lost records,
  - propaganda,
  - mythic time,
  - unreliable calendar,
  - cultural disagreement.
- Player-facing uncertainty must not mean authorial uncertainty unless marked `UNDER_REVIEW`.

---

## Player Visibility and Spoiler Policy

### Visibility Levels

```text
Player Visible — can appear directly in player-facing text.
Discoverable — can be learned through play.
Hidden Truth — true in canon but not directly revealed yet.
Spoiler Restricted — should not appear outside approved spoiler-safe docs.
Internal Only — production context, not player-facing.
```

### Spoiler Record

```md
## Spoiler Classification

- Content:
- Visibility:
- Reveal timing:
- Approved player-facing use:
- Restricted files:
- Notes:
```

### Spoiler Rules

- Hidden truth must not leak into early player-facing text.
- Clues may hint at hidden truth if approved.
- Marketing/community copy must not reveal restricted spoilers.
- Localization context can include spoiler notes if access-controlled and necessary for translation quality.
- If reveal timing changes, update affected dialogue, lore, quests, and localization notes.

---

## Localization Readiness

### Narrative Localization Rules

Narrative text must:

- avoid idioms that do not translate,
- avoid culture-specific jokes unless intentional,
- use named placeholders,
- provide translator context,
- avoid concatenated sentence fragments,
- avoid hardcoded gender/number assumptions where variables appear,
- avoid line lengths that break UI,
- document invented terms in glossary,
- document pronunciation if voiceover is relevant.

### Localization Record

```md
## Narrative Localization Record

- Text ID:
- Source text:
- Context:
- Speaker:
- Character limit:
- Variables:
- Grammar notes:
- Cultural risk:
- Glossary terms:
- Pronunciation:
- Translator notes:
```

### Placeholder Rules

Use named placeholders:

```text
{player_name}
{item_count}
{faction_name}
```

Avoid:

```text
{0}
%s
[name]
```

unless the localization system requires that format and context is documented.

---

## Glossary and Terminology

### Glossary Entry

```md
## Glossary Term: [Term]

- Canon level:
- Definition:
- Player-facing:
- Pronunciation:
- Translation guidance:
- Related terms:
- Do not translate:
  - Yes / No
- Notes:
```

### Glossary Rules

- Proper nouns, faction names, invented terms, magic/tech terms, and cultural concepts require glossary entries.
- Terms must be used consistently.
- If a term changes, update all affected files or create a retcon/supersession record.
- Translation guidance must state whether a term is translated, transliterated, or preserved.

---

## Narrative Dependency Map

Narrative content may depend on:

- gameplay mechanics,
- quests,
- level design,
- environment art,
- audio cues,
- UI text,
- cinematics,
- localization,
- accessibility,
- live-ops events,
- marketing/community copy.

### Dependency Record

```md
## Narrative Dependency

- Narrative element:
- Depends on:
- Provides to:
- Data / text / context exchanged:
- Player-facing impact:
- Spoiler risk:
- Owner:
- Status:
```

### Dependency Rules

- If narrative changes affect quests, levels, UI, or dialogue, notify relevant owner.
- If gameplay changes invalidate narrative, update affected narrative docs.
- If a dependency is unresolved, mark content `UNDER_REVIEW`.

---

## Narrative Review Format

Use this for reviews:

```md
## Narrative Review: [Document / Entry]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Canon Status

### Cross-Reference Status

### Contradiction Status

### World Rule Status

### Character Voice Status

### Mystery / Hidden Truth Status

### Faction Logic Status

### Timeline / Continuity Status

### Localization Readiness

### Dialogue Line-Length Status

### Required Follow-Up
```

### Severity

```text
NARR-S1 — Critical
Breaks established canon, leaks major hidden truth, contradicts core world rule, or invalidates major story structure.

NARR-S2 — High
Missing canon level, unresolved contradiction, missing mystery answer, major voice mismatch, faction logic failure, or player-facing spoiler risk.

NARR-S3 — Medium
Weak cross-reference, localization issue, line-length violation, unclear visibility level, incomplete timeline implication.

NARR-S4 — Low
Terminology polish, minor style inconsistency, formatting, minor context note.
```

---

## Self-Learning Protocol

Self-learning means controlled improvement from approved narrative reviews, contradiction resolutions, retcon records, voice-profile corrections, localization findings, QA reports, and user corrections.

It does not mean autonomous canon changes, hidden memory updates, or treating brainstorms as established lore.

### What May Be Learned

The narrative rule system may learn:

- approved canon facts,
- approved world rules,
- approved retcons,
- established character voice patterns,
- known contradiction patterns,
- faction relationship corrections,
- mystery truth clarifications,
- localization hazards,
- line-length failure patterns,
- glossary and terminology decisions,
- rejected lore directions and why.

### What Must Not Be Learned or Stored

Do not store:

- private user data,
- private chain-of-thought,
- secrets or credentials,
- unapproved brainstorms as canon,
- provisional lore as established canon,
- rejected lore as active direction,
- hidden truth in player-facing memory,
- one-off writing comments as global voice rules without approval,
- localization exceptions as general policy without review.

### Lesson Classification

Use:

```text
Confirmed Rule
Approved Canon
Approved World Rule
Approved Retcon
Voice Finding
Contradiction Finding
Mystery Finding
Faction Finding
Timeline Finding
Localization Finding
Line-Length Finding
Glossary Finding
Spoiler Finding
QA Finding
Implementation Feedback
Rejected Approach
Working Assumption
Temporary Context
Superseded
```

### Lesson Storage

Store durable lessons only in approved, reviewable locations such as:

```text
design/narrative/story-bible.md
design/narrative/world-rules.md
design/narrative/contradictions.md
design/narrative/retcons.md
design/narrative/glossary.md
design/narrative/localization-notes.md
docs/narrative/narrative-standards.md
tasks/lessons.md
production/qa/narrative/
production/session-state/lessons.md
```

### Lesson Format

```md
## Lesson: [Short Name]

- Status:
- Source:
- Applies to:
- Lesson:
- Evidence:
- Date/session:
- Expiry/review trigger:
- Conflicts:
```

### Lesson Validation Rules

A lesson may be stored only if:

- it is specific,
- it is approved or evidence-backed,
- it applies to narrative consistency or writing governance,
- it does not include sensitive data,
- it is not overgeneralized,
- it does not conflict with established canon,
- it has owner/review trigger where appropriate.

### Lesson Expiry

Review or expire lessons when:

- canon changes,
- world rules change,
- character arcs change,
- voice profiles change,
- timeline changes,
- faction relationships change,
- localization pipeline changes,
- narrative-director supersedes the rule,
- creative-director supersedes the direction,
- the lesson was temporary,
- the lesson is too broad.

---

## Self-Healing Protocol

Self-healing means detecting a narrative-rule failure, containing the risk, repairing safely, verifying the repair, and reporting what changed.

### Failure Types

Monitor for:

- missing canon level,
- contradiction with established lore,
- unverified cross-reference,
- world-rule violation,
- undocumented world-rule exception,
- mystery without true answer,
- clue inconsistent with mystery truth,
- faction motivation illogical,
- relationship asymmetry unexplained,
- power structure unclear,
- voice profile missing,
- dialogue voice mismatch,
- line over 120 characters,
- unnamed placeholder,
- idiom/localization hazard,
- hidden truth leaking to player-facing text,
- timeline inconsistency,
- glossary term missing,
- retcon attempted silently.

### Recovery Loop

When failure occurs:

1. **Stop**
   - Do not mark narrative content approved.

2. **Identify**
   - State the exact narrative failure.

3. **Classify**
   - Canon, contradiction, world rule, mystery, faction, voice, localization, line length, spoiler, timeline, glossary, or retcon issue.

4. **Contain**
   - Mark status:
     - `BLOCKED`,
     - `UNDER_REVIEW`,
     - `CONTRADICTION_FOUND`,
     - `UNVERIFIED`,
     - `LOCALIZATION_NOT_READY`,
     - `VOICE_MISMATCH`.

5. **Recover**
   - add canon level,
   - cross-reference related lore,
   - resolve contradiction,
   - document exception,
   - add mystery true answer,
   - revise clue,
   - repair faction logic,
   - check voice profile,
   - split/rewrite long line,
   - replace idiom,
   - add named placeholders,
   - hide spoiler,
   - update timeline,
   - add glossary term,
   - create retcon record.

6. **Verify**
   - Re-check source-of-truth files.
   - Re-check voice profile.
   - Re-check line length.
   - Re-check localization notes.
   - Re-check dependencies.

7. **Report**
   - Summarize issue, repair, remaining risk, and owner.

8. **Learn**
   - Propose durable lesson only if validated and approved.

---

## Error Recovery

### Missing Canon Level

If a lore entry lacks canon level:

- mark `BLOCKED`,
- classify as Established, Provisional, or Under Review,
- identify owner and source,
- do not use downstream until classified.

### Lore Contradiction

If new lore contradicts established lore:

- create contradiction record,
- determine whether contradiction is accidental or intentional,
- fix, label unreliable account, or propose retcon,
- keep affected content under review until resolved.

### World Rule Violation

If content violates a world rule:

- identify the rule,
- revise content or document exception,
- define cost/consequence of exception,
- request Narrative Director approval.

### Mystery Missing True Answer

If mystery lacks true answer:

- mark `BLOCKED`,
- define hidden truth,
- check clues against truth,
- document reveal timing.

### Voice Mismatch

If dialogue does not match character voice:

- compare against voice profile,
- revise diction/rhythm/vocabulary,
- document intentional exception if character state justifies it,
- request owner review if profile is missing.

### Line Over 120 Characters

If dialogue exceeds 120 characters:

- split into multiple lines,
- rewrite shorter,
- move exposition out of dialogue,
- or request UI/layout exception.

### Localization Hazard

If text contains idiom, unnamed variable, concatenated phrase, or grammar-fragile wording:

- rewrite in localization-safe language,
- add named placeholders,
- add translator notes,
- update glossary.

### Faction Logic Failure

If faction action does not match motivation/resources/relationships:

- update faction motivation or action,
- document internal conflict or external pressure,
- revise relationship map,
- mark under review if unresolved.

### Timeline Inconsistency

If timeline conflicts:

- identify affected dates/events,
- correct chronology,
- document unreliable date source if intentional,
- update related entries.

### Silent Retcon

If established canon is changed without record:

- stop,
- restore original or create retcon proposal,
- list affected files,
- request approval.

---

## Memory Policy

### Short-Term Task Memory

Track during current narrative task:

- document path,
- canon level,
- player visibility,
- related lore,
- world rules,
- timeline references,
- character voice profile,
- faction relationships,
- mystery records,
- localization notes,
- line-length issues,
- contradictions,
- open questions,
- approvals needed.

Short-term memory expires after task completion unless explicitly stored.

### Project Memory

Project memory may store:

- approved canon facts,
- approved world rules,
- approved retcons,
- voice-profile rules,
- faction relationship rules,
- mystery truth records,
- glossary decisions,
- localization hazards,
- contradiction resolutions,
- rejected directions.

### Never Store

Never store:

- secrets,
- credentials,
- private user data,
- private chain-of-thought,
- unapproved brainstorms as canon,
- provisional lore as established truth,
- hidden truth in player-facing summaries,
- rejected lore as active direction,
- unsupported claims of approval.

---

## Feedback Policy

When the user, Creative Director, Narrative Director, Writer, World Builder, Localization Lead, Level Designer, Game Designer, Producer, or QA Lead corrects narrative content:

1. Accept the correction.
2. Identify whether it affects:
   - canon level,
   - world rules,
   - timeline,
   - character voice,
   - faction logic,
   - mystery truth,
   - player visibility,
   - localization,
   - line length,
   - glossary,
   - retcon status.
3. Revise current output.
4. Ask whether the correction should become durable narrative guidance if reusable.
5. Store only if approved and evidence-backed.

---

## Tool-Use Policy

This rules file does not grant tools by itself. Agents applying it must follow their own tool permissions.

General guidance:

- Use file-reading tools to inspect lore, character profiles, voice profiles, faction docs, mystery docs, world rules, timeline, glossary, and localization notes.
- Use search tools to find names, terms, events, faction references, mystery clues, and possible contradictions.
- Use write/edit tools only after approval under the active agent’s workflow.
- Use Bash only if the active agent allows it and only under that agent’s safety policy.
- Do not use Bash to bypass write/edit approval.
- Do not claim cross-reference, localization, or line-length validation without evidence.

---

## Safety Guardrails

Never allow narrative content under `design/narrative/**` to:

- omit canon level,
- treat brainstorm as canon,
- change established canon silently,
- introduce contradictions without explanation,
- violate world rules without approved exception,
- create mysteries without true answers,
- use faction behavior that lacks motivation or power logic,
- write character dialogue without voice-profile check,
- exceed 120 characters per dialogue line without review,
- use unnamed placeholders,
- use localization-hostile idioms in production text,
- reveal hidden truth too early,
- omit glossary entries for invented terms,
- claim approval without evidence.

---

## Output Standards

Narrative reviews and drafts should be:

- canon-aware,
- cross-referenced,
- contradiction-conscious,
- world-rule consistent,
- voice-profile aligned,
- mystery-truth backed,
- faction-logical,
- timeline-safe,
- localization-ready,
- spoiler-aware,
- line-length aware,
- clear about unresolved issues.

### Review Output Format

```md
## Narrative Review: [Document / Entry]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Canon Level

### Cross-References

### Contradictions

### World Rules

### Character Voice

### Mysteries / Hidden Truth

### Factions

### Timeline

### Localization

### Dialogue Line Length

### Required Follow-Up
```

---

## Reflection Checklist

After reviewing or drafting narrative content, privately check:

- Does every lore entry have canon level?
- Did I cross-reference existing lore?
- Did I check for contradictions?
- Did I separate established facts from provisional facts?
- Did I separate player-visible knowledge from hidden truth?
- Did I check world rules?
- Did I document mystery true answers?
- Did I check faction motivation and power logic?
- Did I check character voice profile?
- Did I verify dialogue line length?
- Did I use named placeholders?
- Did I avoid localization-hostile idioms?
- Did I avoid leaking spoilers?
- Did I avoid silent retcons?
- Did I avoid storing unapproved lessons?

Do not expose private chain-of-thought. Report only findings, evidence, and recommendations.

---

## Evaluation Checklist

Before final approval of narrative content:

### Canon and Consistency

- [ ] Canon level is specified.
- [ ] Source and owner are specified.
- [ ] Existing lore was checked.
- [ ] Contradictions are resolved or documented.
- [ ] Retcons are recorded if needed.
- [ ] Provisional content is not treated as established.

### World Rules and Timeline

- [ ] Relevant world rules are checked.
- [ ] Exceptions are documented.
- [ ] Timeline placement is coherent.
- [ ] Character ages/durations/travel times are plausible where relevant.
- [ ] Hidden truth fits world logic.

### Mysteries

- [ ] True answer exists.
- [ ] Clues align with true answer.
- [ ] Red herrings are intentional.
- [ ] Reveal timing is documented.
- [ ] Spoiler level is assigned.

### Characters and Factions

- [ ] Character voice profile exists.
- [ ] Dialogue matches voice profile.
- [ ] Faction motivation is clear.
- [ ] Faction resources and constraints are clear.
- [ ] Relationships are reciprocal or asymmetry is explained.
- [ ] Power structure is internally logical.

### Localization and UI Constraints

- [ ] Text avoids fragile idioms.
- [ ] Variables use named placeholders.
- [ ] Translator context is provided.
- [ ] Glossary terms are defined.
- [ ] Dialogue lines are 120 characters or less.
- [ ] Longer lines have approved exception or are rewritten.

### Process

- [ ] Player visibility is assigned.
- [ ] Hidden truth is protected.
- [ ] Open questions are marked.
- [ ] Approval status is clear.
- [ ] File edits require approval.
- [ ] Persistent lessons require approval.

---

## Example Workflows

### Example 1: New Lore Entry

Issue:

```text
A new lore entry says the city was founded after the Flood, but the timeline says the city existed before the Flood.
```

Response:

```md
Finding: Timeline contradiction.

Fix:
- Determine whether the new entry is wrong, the timeline is wrong, or one source is an unreliable account.
- Create a contradiction record.
- Keep entry Under Review until resolved.
```

---

### Example 2: Missing Canon Level

Issue:

```text
A lore entry has no canon status.
```

Response:

```md
Finding: Canon level missing.

Fix:
Add one:
- Established
- Provisional
- Under Review

Do not use this entry downstream until canon level is assigned.
```

---

### Example 3: Mystery Without True Answer

Issue:

```text
Document says “No one knows who opened the gate,” but no hidden answer is recorded.
```

Response:

```md
Finding: Mystery has no true answer.

Fix:
Document the hidden truth, even if players never learn it. Then align clues, red herrings, and reveal timing.
```

---

### Example 4: Voice Mismatch

Issue:

```text
A terse soldier character suddenly speaks in poetic metaphors.
```

Response:

```md
Finding: Voice-profile mismatch.

Fix:
Compare against the character voice profile. Either revise to match established diction/rhythm or document the unusual speech as a justified state change.
```

---

### Example 5: Faction Logic Failure

Issue:

```text
A faction gives away its only strategic resource with no stated reason.
```

Response:

```md
Finding: Faction motivation/resource logic failure.

Fix:
Define why:
- coercion,
- desperation,
- deception,
- religious obligation,
- internal coup,
- hidden surplus,
- or revise the action.
```

---

### Example 6: Localization Hazard

Issue:

```text
Dialogue says: “That plan is a slam dunk.”
```

Response:

```md
Finding: Localization-hostile idiom.

Fix:
Rewrite with direct meaning, such as:
“This plan is certain to work.”
```

---

### Example 7: Dialogue Too Long

Issue:

```text
A dialogue line is 168 characters.
```

Response:

```md
Finding: Dialogue line exceeds 120-character constraint.

Fix:
Split into two lines or rewrite. If long line is required, request UI/dialogue-box exception.
```

---

### Example 8: Hidden Truth Leak

Issue:

```text
An early item description reveals the villain’s true identity before the reveal.
```

Response:

```md
Finding: Spoiler leak.

Fix:
Classify the identity as Hidden Truth / Spoiler Restricted. Rewrite early text as clue or ambiguity, not confirmation.
```

---

### Example 9: User Correction

User says:

```text
The Flood is Provisional, not Established.
```

Response pattern:

```text
Understood. I’ll treat the Flood as Provisional canon unless a later Narrative Director approval establishes it. Store this as a durable canon-status update only if approved.
```

---

### Example 10: Case Where the Rule Must Not Learn

User says during brainstorming:

```text
Maybe the queen secretly caused the plague.
```

Response pattern:

```text
I’ll treat that as brainstorm only, not canon. If it becomes an active mystery, it needs canon level, hidden truth, clue plan, timeline check, and contradiction review.
```

---

## Final Narrative Rule

Narrative content under `design/narrative/**` must be:

- canon-labeled,
- cross-referenced,
- contradiction-checked,
- world-rule consistent,
- mystery-truth backed,
- faction-logical,
- voice-profile aligned,
- timeline-safe,
- player-visibility aware,
- localization-ready,
- line-length compliant,
- spoiler-safe,
- reviewable,
- and honest about unresolved canon.