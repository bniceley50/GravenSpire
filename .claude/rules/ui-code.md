---
paths:
  - "src/ui/**"
---

# UI Code Rules

## Rule Set Name

UI Code Rules

## Mission

These rules govern all UI implementation under:

```text
src/ui/**
```

Their purpose is to ensure UI code is presentation-focused, localized, accessible, input-complete, performant, non-blocking, audio-system-compliant, resolution-safe, and testable.

UI is the player’s primary interface with the game. It must communicate game state clearly without owning that state, support all required input methods, respect accessibility settings, and remain responsive under real runtime conditions.

The core UI-code question is:

> Does this UI display state clearly, request changes safely, support every required player input and accessibility path, and avoid blocking or corrupting the game?

---

## Operating Principles

1. **UI does not own game state**
   - UI must never directly own or mutate gameplay, save, inventory, economy, quest, combat, or progression state.
   - UI displays state and sends commands/events requesting changes.

2. **UI is presentation, not authority**
   - UI may show health.
   - UI may not reduce health.
   - UI may show inventory.
   - UI may not directly add/remove items.
   - UI may request an action through an approved command/event path.

3. **Localization is mandatory**
   - All player-facing text must go through the localization system.
   - No hardcoded display strings in UI code.
   - Variable text must use named placeholders.

4. **Input parity is mandatory**
   - Every interactive element must be usable with:
     - keyboard/mouse,
     - gamepad.
   - Touch support is required only if the target platform requires it.

5. **Focus must be explicit**
   - Gamepad and keyboard navigation require clear focus order, initial focus, focus restoration, modal focus trapping, and disabled-state handling.

6. **Accessibility is not optional**
   - Scalable text and colorblind modes are mandatory.
   - UI must not rely on color alone.
   - Motion-sensitive UI must support reduced motion or skipping.
   - UI must support the project’s accessibility requirements.

7. **Animations must not trap players**
   - UI animations must be skippable.
   - UI animations must respect reduced-motion preferences.
   - UI must remain usable if animation is interrupted.

8. **UI sounds go through audio events**
   - UI code must not play raw audio files directly.
   - UI sounds must trigger approved audio events and respect audio mixer/bus/volume settings.

9. **UI must not block the game thread**
   - No synchronous asset loading, heavy layout rebuilding, blocking network calls, blocking save/load, expensive formatting loops, or long-running work on the game thread.

10. **Resolution coverage is required**
    - Every screen must be tested at minimum and maximum supported resolutions.
    - Aspect ratios, safe areas, text scale, and localization expansion must be considered.

11. **UI states must be explicit**
    - Loading, empty, disabled, unavailable, error, confirmation, success, failure, offline, locked, and unknown states must be defined.

12. **Self-healing**
    - When UI owns state, loses focus, hardcodes text, blocks the game thread, breaks layout, ignores accessibility, or fails input coverage, stop, classify, repair, verify, and report.

13. **Bounded self-learning**
    - Durable UI lessons may be stored only in approved reviewable locations.
    - Lessons must be evidence-backed, reversible, and subordinate to UX specs, accessibility requirements, localization rules, and current user instructions.

---

## Scope

These rules apply to production UI code under:

```text
src/ui/**
```

This includes, where present:

- menus,
- HUD,
- inventory screens,
- settings screens,
- pause screens,
- dialogue boxes,
- tooltips,
- popups,
- modals,
- notifications,
- loading screens,
- map screens,
- quest/objective UI,
- accessibility settings UI,
- options UI,
- input prompt UI,
- UI data-binding code,
- UI command/event code,
- UI animation code,
- UI localization helpers,
- UI audio trigger code,
- UI tests and test helpers colocated under UI paths.

---

## Non-Goals

These rules do not authorize UI code to:

- own gameplay state,
- implement gameplay logic,
- directly mutate save/progression/economy/combat state,
- define UX flows without UX Designer approval,
- define visual style without Art Director approval,
- create final localization strings without Localization Lead / Writer coordination,
- create final UI sound assets,
- bypass accessibility requirements,
- block the game thread,
- edit files without the active agent’s approval workflow,
- store persistent lessons without approval.

---

## UI Lifecycle State Labels

Use these labels when reviewing or implementing UI work:

```text
PROPOSED — suggested UI change, not approved.
SPEC_READY — UX/art/accessibility/localization requirements are documented.
IMPLEMENTED — code or screen exists.
STATE_BOUND — UI reads from ViewModel/state snapshot, not raw mutable game state.
COMMAND_BOUND — UI sends approved command/event for mutations.
LOCALIZED — all user-facing text uses localization keys.
INPUT_COMPLETE — keyboard/mouse and gamepad support verified.
FOCUS_VERIFIED — focus order, initial focus, restoration, and modal behavior verified.
ACCESSIBILITY_REVIEWED — text scale, contrast/colorblind/reduced motion reviewed.
AUDIO_EVENT_READY — UI sounds route through audio event system.
NON_BLOCKING_VERIFIED — no blocking game-thread work detected.
RESOLUTION_TESTED — min/max supported resolutions tested.
PSEUDOLOCALIZED_TESTED — pseudolocalization or text expansion tested.
PERFORMANCE_PROFILED — UI cost measured.
QA_VERIFIED — QA has verified required behavior.
BLOCKED — missing spec, localization, input, accessibility, performance, or state-boundary requirement.
SUPERSEDED — replaced by newer UI.
DEPRECATED — still present but not for new use.
```

### State Rules

- Do not mark `LOCALIZED` if display text is hardcoded.
- Do not mark `INPUT_COMPLETE` without keyboard/mouse and gamepad coverage.
- Do not mark `FOCUS_VERIFIED` without focus-path evidence.
- Do not mark `RESOLUTION_TESTED` without min/max resolution evidence.
- Do not mark `NON_BLOCKING_VERIFIED` without code review or profiling evidence.
- `IMPLEMENTED` is not equivalent to complete.

---

## Source of Truth

Recommended project sources:

```text
design/ux/
design/ui/
design/accessibility/
design/localization/
design/audio/
src/ui/
src/gameplay/
assets/data/
locales/
production/qa/ui/
production/qa/accessibility/
production/session-state/lessons.md
```

### Source-of-Truth Rules

- Check UX flow specs before implementing screen flow.
- Check Art Director mockups/style guide before visual implementation.
- Check localization files before adding user-facing text.
- Check accessibility standards before approving UI.
- Check audio event docs before triggering UI sound.
- Check gameplay command/event contracts before wiring UI actions.
- If UI implementation conflicts with UX, art, accessibility, localization, or gameplay contracts, flag the conflict.

---

## UI / Game-State Boundary

### Required Pattern

```text
Game State
  -> ViewModel / State Snapshot / Read Model
  -> UI Display

UI Input
  -> Command / Event / Request
  -> Game System
  -> Game State
```

### Forbidden Pattern

```text
UI Widget
  -> directly mutates Game State
```

### Boundary Rules

UI may:

- read immutable state snapshots,
- bind to ViewModels,
- subscribe to state-change events,
- display values,
- show derived presentation-only values,
- send commands/events,
- request actions,
- show results returned by game systems.

UI must not:

- directly set player health,
- directly add/remove inventory items,
- directly change currency,
- directly complete quests,
- directly mutate save data,
- directly alter combat state,
- directly change progression,
- directly own authoritative settings unless the setting is UI-local and approved.

### UI Boundary Contract

```md
## UI / Game-State Boundary Contract: [Screen / Feature]

- UI screen:
- Game system:
- State read:
- ViewModel / read model:
- Commands emitted:
- Events subscribed:
- Result payloads:
- Error payloads:
- Ownership:
- Lifecycle:
- Tests:
```

---

## Command and Event Standards

### UI Command Record

```md
## UI Command: [CommandName]

- Source screen:
- Target system:
- User intent:
- Payload:
- Validation owner:
- Success result:
- Failure result:
- Loading behavior:
- Disabled-state behavior:
- Analytics event, if any:
- Tests:
```

### UI Event Record

```md
## UI Event Subscription: [EventName]

- Source system:
- UI consumer:
- Payload:
- Update behavior:
- Frequency:
- Debounce/throttle:
- Null/invalid payload behavior:
- Lifecycle cleanup:
- Tests:
```

### Command/Event Rules

- Commands represent user intent, not direct mutation.
- UI must handle success and failure results.
- UI must show loading state for long-running commands.
- Commands must be disabled or guarded when invalid.
- Event subscriptions must be cleaned up when screens close.
- High-frequency events must be throttled or batched where needed.
- UI events must not carry mutable internal game-state references.

---

## Data Binding and ViewModel Rules

### ViewModel Record

```md
## UI ViewModel: [Name]

- Screen:
- Source systems:
- Fields:
- Update triggers:
- Derived presentation values:
- Localization keys:
- Empty state:
- Error state:
- Lifetime:
- Tests:
```

### Binding Rules

- UI reads through ViewModel/state snapshot.
- ViewModels expose presentation-ready values where appropriate.
- UI should not query game systems every frame.
- Bindings must handle missing/null state.
- ViewModels must not mutate gameplay state directly.
- ViewModels may derive display-only values.
- Data refresh must be event-driven where possible.
- Polling must be justified and budgeted.

---

## Localization Rules

### Localization Requirements

All user-facing UI text must use localization keys.

Hardcoded display strings are prohibited in:

- labels,
- buttons,
- tooltips,
- dialogue boxes,
- menus,
- HUD,
- notifications,
- error messages,
- confirmation dialogs,
- loading messages,
- settings descriptions,
- accessibility labels,
- input prompts.

### Localization Key Record

```md
## UI Localization Key

- Key:
- Default/source text:
- Screen:
- Context:
- Character limit:
- Variables:
- Pluralization:
- Gender/grammar notes:
- Fallback:
- Owner:
```

### Key Naming Guidance

Use hierarchical dot notation unless the project defines another convention:

```text
ui.settings.audio.volume_label
ui.inventory.empty_message
ui.confirm.delete_save.title
ui.input.prompt.interact
```

### Placeholder Rules

Use named placeholders:

```text
{itemName}
{itemCount}
{playerName}
```

Do not concatenate localized strings from fragments unless approved by Localization Lead.

### Localization Failure Rules

If a key is missing:

- do not display raw key to players,
- use approved fallback,
- log safe missing-key diagnostic,
- mark localization gap.

If text overflows:

- adjust layout,
- shorten source text only with Writer/UX approval,
- use scalable/wrapping layout where possible.

---

## Text Fitting and Pseudolocalization

### Text Test Matrix

Test important screens with:

- default locale,
- pseudolocalized expanded strings,
- longest supported locale if known,
- minimum text scale,
- maximum text scale,
- minimum supported resolution,
- maximum supported resolution.

### Text Fitting Record

```md
## UI Text Fitting Test

- Screen:
- Locale:
- Text scale:
- Resolution:
- Overflow:
- Truncation:
- Clipping:
- Wrapping:
- Pass/fail:
- Evidence:
```

### Text Rules

- UI must support scalable text.
- Text must not clip at supported scales.
- Critical text must not be hidden behind ellipses unless explicitly designed.
- Buttons must handle translated label length.
- Tooltips must wrap safely.
- Variable insertion must not break grammar or layout.
- Font coverage must support supported locales.

---

## Input Support and Focus Management

### Required Input Support

Every interactive UI element must support:

- keyboard/mouse,
- gamepad.

Touch support applies if the target platform includes touch.

### Focus Rules

Every screen must define:

- initial focus,
- focus order,
- disabled-item behavior,
- focus restoration when returning,
- modal focus trap,
- back/cancel behavior,
- controller prompt behavior,
- mouse/gamepad switching behavior.

### Focus Map Record

```md
## UI Focus Map: [Screen]

- Screen:
- Initial focus:
- Primary action:
- Back/cancel action:
- Focus order:
- Modal focus trap:
- Disabled-state behavior:
- Focus restoration:
- Keyboard path:
- Gamepad path:
- Tests:
```

### Input Rules

- Keyboard-only navigation must reach every interactive element.
- Gamepad-only navigation must reach every interactive element.
- Focus must always be visible.
- Opening a screen must set logical initial focus.
- Closing a modal must restore focus to the invoking element.
- Disabled elements must be skipped or explained.
- Back/cancel must behave consistently.
- Input prompts must match active input device.
- UI must not consume gameplay input when not focused or active.

---

## Accessibility Rules

### Required Accessibility Support

At minimum:

- scalable text,
- colorblind modes or equivalent redundant encoding,
- non-color indicators,
- reduced motion / skip animations,
- keyboard/gamepad navigation,
- readable contrast,
- safe focus indicators,
- subtitle/caption support where relevant,
- screen-reader metadata where project targets it.

### Accessibility Review Record

```md
## UI Accessibility Review: [Screen / Feature]

- Screen/feature:
- Text scaling:
- Contrast:
- Colorblind safety:
- Color-only information:
- Focus visibility:
- Keyboard navigation:
- Gamepad navigation:
- Reduced motion:
- Animation skip:
- Screen reader metadata:
- Subtitles/captions:
- Severity:
- Recommendation:
```

### Colorblind Rules

- Do not encode information through color alone.
- Pair color with:
  - icon,
  - shape,
  - text,
  - pattern,
  - position,
  - animation,
  - sound/haptic where appropriate.
- Colorblind mode must preserve gameplay meaning.
- Test UI in supported colorblind modes.

### Text Scale Rules

- UI must support project-approved text scale levels.
- Scaling must not break layout.
- Text scale must apply consistently.
- Critical UI must remain readable at maximum scale.

---

## Motion and Animation Policy

### Animation Rules

- All UI animations must be skippable where they block interaction or information access.
- All UI animations must respect reduced-motion preferences.
- Screen transitions must have reduced-motion alternatives.
- Long animations must not delay critical interaction.
- Repeated motion must not be required to understand UI.
- Flashing or high-intensity effects must follow accessibility policy.

### Animation Record

```md
## UI Animation Review

- Screen/element:
- Animation:
- Purpose:
- Duration:
- Blocks interaction:
- Skippable:
- Reduced-motion behavior:
- Interrupt behavior:
- Accessibility risk:
- Tests:
```

### Reduced-Motion Examples

Allowed alternatives:

- instant transition,
- crossfade,
- shorter duration,
- no parallax,
- no camera shake,
- no repeated bounce,
- static state change.

---

## UI Audio Rules

### Audio Event Requirement

UI sounds must trigger through the audio event system.

Forbidden:

```text
UI directly plays raw audio file.
UI creates audio source ad hoc.
UI bypasses UI/audio volume category.
```

Allowed:

```text
UI emits approved audio event ID.
Audio system handles routing, bus, volume, cooldown, and playback.
```

### UI Audio Event Record

```md
## UI Audio Event: [EventID]

- UI trigger:
- Audio event ID:
- Category/bus:
- Cooldown:
- Concurrency:
- Volume slider:
- Accessibility note:
- Owner:
- Tests:
```

### UI Audio Rules

- UI sounds must respect UI volume slider.
- UI sounds must not spam on rapid focus movement.
- Error, confirm, cancel, hover/focus, select, and reward sounds should be distinct where needed.
- UI code does not decide sonic palette.
- Missing audio event should fail gracefully or use silent fallback.

---

## Non-Blocking UI Policy

### UI Must Not Block The Game Thread

Forbidden in UI path:

- synchronous asset loading,
- blocking file I/O,
- blocking network calls,
- blocking save/load,
- heavy layout rebuilds in hot paths,
- creating hundreds of widgets at once without virtualization,
- per-frame localization string formatting,
- per-frame scene/game-object queries,
- long-running loops,
- shader/material compilation during interaction,
- sleep/wait calls on game thread.

### Non-Blocking Review Record

```md
## UI Non-Blocking Review

- Screen/system:
- Potential blocking operation:
- Trigger:
- Runtime frequency:
- Async strategy:
- Loading state:
- Timeout/failure behavior:
- Evidence:
```

### Non-Blocking Rules

- Use async loading for UI assets.
- Show loading state for long operations.
- Virtualize large lists/grids.
- Pool repeated UI elements.
- Batch updates.
- Cache localization lookups where appropriate.
- Defer heavy computation to game/system layer or background task when safe.
- UI should remain responsive during data refresh.

---

## UI Performance Standards

### Performance Record

```md
## UI Performance Review: [Screen]

- Screen:
- Build/platform:
- Scenario:
- Widget/element count:
- CPU frame cost:
- GPU/draw cost:
- Layout rebuilds:
- Allocations:
- List virtualization:
- Pooling:
- Bottlenecks:
- Verdict:
```

### Performance Rules

- UI must not exceed project UI frame budget.
- If no UI budget exists, mark budget status `UNKNOWN` and escalate to Technical Director / Performance Analyst.
- Inventory, maps, settings, and large list screens must use virtualization or pooling where needed.
- Avoid per-frame layout invalidation.
- Avoid per-frame text measurement/formatting.
- Avoid creating/destroying UI elements repeatedly during common flows.
- Profile before claiming performance success.

---

## Resolution, Aspect Ratio, and Safe Area Testing

### Required Coverage

Test all screens at:

- minimum supported resolution,
- maximum supported resolution,
- common aspect ratios:
  - 16:9,
  - 16:10,
  - 21:9 if supported,
  - 4:3 if supported,
  - platform-specific handheld/mobile aspect ratios if supported,
- maximum text scale,
- colorblind mode,
- reduced-motion mode.

### Resolution Test Record

```md
## UI Resolution Test: [Screen]

- Screen:
- Resolution:
- Aspect ratio:
- Text scale:
- Locale:
- Safe area:
- Input method:
- Result:
- Issues:
- Evidence:
```

### Layout Rules

- Critical UI must remain visible in safe area.
- UI must not depend on a single monitor aspect ratio.
- HUD must adapt to resolution and safe area.
- Popups must remain centered or intentionally anchored.
- Large text must not hide interactive controls.
- Scroll containers must expose all content.

---

## Screen State Standards

Every screen must define:

- loading state,
- empty state,
- disabled state,
- unavailable/locked state,
- error state,
- success state,
- confirmation state,
- offline state where relevant,
- stale data state where relevant.

### Screen State Record

```md
## UI Screen State Spec: [Screen]

| State | Trigger | UI Behavior | Player Action Available | Localization Key | Notes |
|---|---|---|---|---|---|
```

### State Rules

- Do not leave screens blank without explanation.
- Errors must be player-comprehensible.
- Disabled controls must explain why when not obvious.
- Loading states must not spin forever without timeout or fallback.
- Confirm destructive actions.
- Offline/unavailable states must avoid technical jargon unless appropriate.

---

## UI Testing Requirements

### Required Test Types

Use where relevant:

- interaction tests,
- focus navigation tests,
- localization tests,
- resolution tests,
- accessibility audits,
- reduced-motion tests,
- input method tests,
- command/event boundary tests,
- UI performance tests,
- regression tests for UI bugs.

### UI Test Evidence Record

```md
## UI Test Evidence

- Screen:
- Test type:
- Input method:
- Resolution:
- Locale:
- Accessibility setting:
- Steps:
- Expected result:
- Actual result:
- Status:
- Evidence:
```

### UI Regression Record

```md
## UI Regression Test

- Bug ID:
- Original failure:
- Screen:
- Input method:
- Resolution/locale/accessibility setting:
- Test path:
- Pass/fail:
- Evidence:
```

### Test Rules

- UI state-boundary bugs require command/event tests.
- Localization bugs require locale or pseudolocalization tests.
- Focus bugs require keyboard/gamepad tests.
- Resolution bugs require resolution test evidence.
- Animation accessibility bugs require reduced-motion test.
- Performance bugs require profile or benchmark evidence.

---

## UI Review Format

Use this for reviews:

```md
## UI Code Review: [Screen/File]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Game-State Boundary

### Localization

### Input / Focus

### Accessibility

### Motion / Animation

### UI Audio

### Non-Blocking / Performance

### Resolution / Layout

### Screen States

### Test Evidence

### Required Follow-Up
```

### Severity

```text
UI-S1 — Critical
UI directly mutates gameplay/save/economy/progression state, blocks the game thread in a critical flow, or prevents required player interaction.

UI-S2 — High
Hardcoded player-facing strings, missing gamepad/keyboard support, inaccessible critical UI, broken focus trap, missing localization, or layout broken at supported resolution.

UI-S3 — Medium
Weak focus restoration, missing reduced-motion behavior, missing empty/error states, text clipping in non-default locale, performance risk without evidence.

UI-S4 — Low
Minor polish, naming, redundant layout, documentation gap, non-blocking style issue.
```

---

## Self-Learning Protocol

Self-learning means controlled improvement from approved UI reviews, accessibility audits, localization findings, QA reports, input/focus bugs, performance profiles, resolution tests, and user corrections.

It does not mean hidden memory updates, autonomous UI pattern changes, or treating one-off UI workarounds as permanent rules.

### What May Be Learned

The UI rule system may learn:

- approved UI/game-state boundary patterns,
- approved ViewModel patterns,
- approved command/event conventions,
- approved localization key conventions,
- known focus-navigation pitfalls,
- known text-fitting failures,
- known colorblind/contrast findings,
- known reduced-motion requirements,
- known UI audio-event conventions,
- known performance bottlenecks,
- known resolution/safe-area issues,
- rejected UI implementation patterns and why.

### What Must Not Be Learned or Stored

Do not store:

- private user data,
- private chain-of-thought,
- secrets,
- credentials,
- private player data,
- raw player telemetry,
- temporary UI hacks as production rules,
- one-off layout bugs as universal rules without review,
- hardcoded placeholder strings as final localization,
- unapproved UX or art-direction decisions,
- unsupported accessibility claims.

### Lesson Classification

Use:

```text
Confirmed Rule
Approved UI Standard
Game-State Boundary Finding
Command/Event Finding
ViewModel Finding
Localization Finding
Input Finding
Focus Finding
Accessibility Finding
Colorblind Finding
Motion Finding
UI Audio Finding
Performance Finding
Resolution Finding
Screen State Finding
QA Finding
Regression Finding
Validated Fix
Rejected Approach
Working Assumption
Temporary Context
Superseded
```

### Lesson Storage

Store durable lessons only in approved, reviewable locations such as:

```text
docs/ui/ui-code-standards.md
docs/ui/input-focus-lessons.md
docs/ui/localization-lessons.md
docs/ui/accessibility-lessons.md
docs/ui/performance-findings.md
production/qa/ui/
production/qa/accessibility/
tasks/lessons.md
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
- it applies to UI implementation,
- it does not include sensitive data,
- it is not overgeneralized,
- it does not conflict with UX, art, accessibility, or localization authority,
- it has a review trigger where appropriate.

### Lesson Expiry

Review or expire lessons when:

- UI framework changes,
- UX flow changes,
- art direction changes,
- localization architecture changes,
- accessibility requirements change,
- supported platforms change,
- supported resolutions change,
- input requirements change,
- QA evidence contradicts the lesson,
- owner supersedes it,
- the lesson was temporary,
- the lesson is too broad.

---

## Self-Healing Protocol

Self-healing means detecting a UI-code rule failure, containing risk, repairing safely, verifying the repair, and reporting what changed.

### Failure Types

Monitor for:

- UI directly mutates game state,
- UI owns gameplay state,
- hardcoded user-facing string,
- missing localization key,
- missing keyboard path,
- missing gamepad path,
- invisible or lost focus,
- modal focus leak,
- missing reduced-motion behavior,
- unskippable blocking animation,
- UI sound played directly,
- UI blocks game thread,
- synchronous loading,
- large list without virtualization,
- layout broken at supported resolution,
- text clipped at maximum scale,
- color-only information,
- missing empty/error/loading state,
- missing cleanup of event subscriptions,
- missing test evidence.

### Recovery Loop

When failure occurs:

1. **Stop**
   - Do not mark UI complete.

2. **Identify**
   - State the exact violation.

3. **Classify**
   - State boundary, localization, input, focus, accessibility, animation, audio, performance, resolution, screen state, or test evidence.

4. **Contain**
   - Mark status:
     - `BLOCKED`,
     - `LOCALIZATION_MISSING`,
     - `INPUT_INCOMPLETE`,
     - `FOCUS_BROKEN`,
     - `ACCESSIBILITY_GAP`,
     - `BLOCKING_RISK`,
     - `RESOLUTION_UNVERIFIED`.

5. **Recover**
   - replace direct mutation with command/event,
   - move text to localization key,
   - add focus path,
   - add gamepad/keyboard support,
   - add reduced-motion fallback,
   - route sound through audio event,
   - make loading async,
   - virtualize/pool large UI,
   - fix layout/safe area,
   - add screen states,
   - add tests/evidence.

6. **Verify**
   - Re-check UI boundary.
   - Re-check localization.
   - Re-check input/focus.
   - Re-check accessibility.
   - Re-check resolution/layout.
   - Run or request tests/profiles where needed.

7. **Report**
   - Summarize issue, fix, remaining risk, and owner.

8. **Learn**
   - Propose durable lesson only if validated and approved.

---

## Error Recovery

### UI Mutates Game State

If UI directly modifies game state:

- remove mutation,
- define command/event,
- route request to game system,
- handle success/failure response,
- add boundary test.

### Hardcoded UI String

If text is hardcoded:

- create localization key,
- move source text to base locale,
- add context/character limit,
- replace hardcoded text with key lookup,
- test fallback behavior.

### Missing Keyboard/Gamepad Path

If element is unreachable:

- add focus target,
- define navigation path,
- ensure visible focus state,
- test with keyboard only and gamepad only.

### Focus Lost

If focus becomes null or invisible:

- define initial focus,
- restore focus on modal close,
- skip disabled controls,
- trap focus in modal,
- add focus regression test.

### Unskippable or Motion-Unsafe Animation

If animation blocks or violates motion settings:

- add skip path,
- add reduced-motion alternative,
- remove unnecessary motion,
- preserve information with static state,
- test reduced-motion mode.

### UI Sound Bypasses Audio Event

If UI plays audio directly:

- replace direct playback with audio event trigger,
- route to UI audio bus/category,
- respect UI volume slider,
- add cooldown if rapid-triggered.

### Game Thread Block

If UI blocks the game thread:

- make operation async,
- defer heavy work,
- add loading state,
- virtualize lists,
- batch updates,
- profile again.

### Resolution Layout Failure

If layout breaks:

- identify resolution/aspect/text-scale/locale,
- adjust anchors/layout constraints,
- support safe area,
- add wrapping/scrolling,
- retest matrix.

### Color-Only Information

If information is conveyed only by color:

- add icon/shape/text/pattern,
- test colorblind modes,
- update accessibility review.

### Missing Screen State

If screen lacks empty/error/loading state:

- define state,
- add localization keys,
- add UI behavior,
- add test case.

---

## Memory Policy

### Short-Term Task Memory

Track during current UI task:

- screen/feature,
- UX source,
- art source,
- localization keys,
- ViewModel/state source,
- commands/events,
- input paths,
- focus map,
- accessibility requirements,
- animation behavior,
- audio event IDs,
- resolution matrix,
- performance concerns,
- tests/evidence,
- open questions.

Short-term memory expires after task completion unless explicitly stored.

### Project Memory

Project memory may store:

- approved UI-code standards,
- ViewModel conventions,
- command/event patterns,
- localization key patterns,
- focus-navigation lessons,
- accessibility findings,
- reduced-motion rules,
- UI audio-event mappings,
- UI performance findings,
- resolution/safe-area findings,
- validated fixes,
- rejected approaches.

### Never Store

Never store:

- secrets,
- credentials,
- private user/player data,
- private chain-of-thought,
- sensitive logs,
- raw telemetry with personal data,
- temporary UI hacks as production standards,
- unapproved UX/art/localization decisions,
- unsupported accessibility claims.

---

## Feedback Policy

When the user, UX Designer, Art Director, Accessibility Specialist, Localization Lead, Audio Director, Gameplay Programmer, Lead Programmer, QA Lead, or Technical Director corrects UI behavior:

1. Accept the correction.
2. Identify whether it affects:
   - game-state boundary,
   - ViewModel/data binding,
   - command/event contract,
   - localization,
   - input handling,
   - focus management,
   - accessibility,
   - animation/motion,
   - UI audio,
   - performance,
   - resolution/layout,
   - screen states,
   - tests.
3. Revise current output.
4. Ask whether the correction should become durable UI-code guidance if reusable.
5. Store only if approved and evidence-backed.

---

## Tool-Use Policy

This rules file does not grant tools by itself. Agents applying it must follow their own tool permissions.

General guidance:

- Use file-reading tools to inspect UI code, UX specs, localization keys, accessibility docs, tests, QA evidence, and audio event docs.
- Use search tools to find hardcoded strings, direct game-state mutations, raw audio playback, synchronous loading, missing focus handlers, and event subscriptions.
- Use write/edit tools only after approval under the active agent’s workflow.
- Use Bash only if the active agent allows it and only under that agent’s safety policy.
- Do not run UI tests, profilers, builds, localization tools, or file mutations without required approval.
- Do not use Bash to bypass write/edit approval.

---

## Safety Guardrails

Never allow production UI code under `src/ui/**` to:

- own or directly modify game state,
- hardcode player-facing strings,
- omit keyboard/mouse support,
- omit gamepad support,
- lose focus or trap users without escape,
- use color as the only information carrier,
- ignore scalable text,
- ignore colorblind modes,
- force unskippable blocking animations,
- ignore reduced-motion settings,
- play UI audio directly outside audio event system,
- block the game thread,
- synchronously load UI assets during interaction,
- ship screens untested at min/max supported resolutions,
- claim accessibility or resolution support without evidence.

---

## Output Standards

UI code reviews should be:

- state-boundary-aware,
- localization-aware,
- input-complete,
- focus-aware,
- accessibility-aware,
- motion-safe,
- audio-event-compliant,
- non-blocking,
- resolution-tested,
- evidence-backed,
- honest about uncertainty.

### Review Output Format

```md
## UI Code Review: [Screen/File]

### Verdict

PASS | PASS_WITH_NOTES | NEEDS_FIX | BLOCKED | UNKNOWN

### Findings

| Finding | Severity | Evidence | Recommendation |
|---|---|---|---|

### Game-State Boundary

### Localization

### Input and Focus

### Accessibility

### Motion / Animation

### UI Audio

### Non-Blocking / Performance

### Resolution / Layout

### Screen States

### Tests / Evidence

### Required Follow-Up
```

---

## Reflection Checklist

After reviewing or drafting UI code, privately check:

- Does UI only display state?
- Are mutations routed through commands/events?
- Is every user-facing string localized?
- Are variable placeholders named?
- Does keyboard/mouse work?
- Does gamepad work?
- Is focus visible and recoverable?
- Are modals focus-trapped?
- Does UI support scalable text?
- Is color not the only signal?
- Are colorblind modes respected?
- Are animations skippable and reduced-motion-safe?
- Are UI sounds routed through audio events?
- Is there any blocking work on the game thread?
- Are min/max resolutions tested?
- Are loading/empty/error/disabled states defined?
- Is evidence available?

Do not expose private chain-of-thought. Report only findings, evidence, and recommendations.

---

## Evaluation Checklist

Before final approval of UI code:

### State Boundary

- [ ] UI does not own game state.
- [ ] UI does not directly mutate game state.
- [ ] UI reads from ViewModel/state snapshot.
- [ ] UI sends commands/events for changes.
- [ ] Success/failure results are handled.
- [ ] Event subscriptions are cleaned up.

### Localization

- [ ] No hardcoded user-facing strings.
- [ ] Localization keys exist.
- [ ] Variables use named placeholders.
- [ ] Missing-key fallback is defined.
- [ ] Text fits at max text scale.
- [ ] Pseudolocalization or expansion test exists where relevant.

### Input and Focus

- [ ] Keyboard/mouse path exists for every interactive element.
- [ ] Gamepad path exists for every interactive element.
- [ ] Initial focus is defined.
- [ ] Focus order is defined.
- [ ] Focus restoration is defined.
- [ ] Modal focus trap works.
- [ ] Disabled-state behavior is defined.
- [ ] Input prompts match active input device.

### Accessibility

- [ ] Text scaling works.
- [ ] Colorblind mode works.
- [ ] Color is not sole information carrier.
- [ ] Contrast/readability is reviewed.
- [ ] Reduced-motion setting is respected.
- [ ] Screen-reader metadata exists where required.
- [ ] Subtitles/captions are supported where relevant.

### Animation and Audio

- [ ] Blocking animations are skippable.
- [ ] Reduced-motion alternative exists.
- [ ] Animation interruption is handled.
- [ ] UI sounds use audio events.
- [ ] UI audio respects volume/bus settings.
- [ ] UI sound spam is controlled.

### Performance and Layout

- [ ] No synchronous loading on interaction path.
- [ ] No blocking network/file/save work on game thread.
- [ ] Large lists are virtualized or pooled.
- [ ] Layout does not rebuild excessively.
- [ ] UI performance is profiled where relevant.
- [ ] Minimum resolution tested.
- [ ] Maximum resolution tested.
- [ ] Safe area and aspect ratio are considered.

### Screen States and Evidence

- [ ] Loading state exists.
- [ ] Empty state exists.
- [ ] Error state exists.
- [ ] Disabled/unavailable state exists.
- [ ] Confirmation state exists for destructive actions.
- [ ] UI test evidence exists or missing evidence is stated.

---

## Example Workflows

### Example 1: UI Directly Mutates Health

Issue:

```text
health_bar.on_click sets player.health = 0
```

Response:

```md
Finding: UI directly mutates gameplay state.

Severity: UI-S1.

Fix:
- UI emits `RequestDebugDamageCommand` or approved gameplay command.
- Gameplay system validates and mutates health.
- UI receives updated health through ViewModel/state event.
```

---

### Example 2: Hardcoded Button Text

Issue:

```text
button.text = "Start Game"
```

Response:

```md
Finding: Hardcoded user-facing string.

Fix:
- Add localization key: `ui.main_menu.start_game`.
- Replace text assignment with localization lookup.
- Add context and character limit for translators.
```

---

### Example 3: Gamepad Focus Missing

Issue:

```text
Settings menu works with mouse but gamepad cannot reach the reset button.
```

Response:

```md
Finding: Input parity failure.

Fix:
- Add reset button to focus order.
- Ensure visible focus state.
- Test gamepad-only navigation.
```

---

### Example 4: Modal Focus Leak

Issue:

```text
Confirmation dialog opens, but gamepad focus can move to inventory behind it.
```

Response:

```md
Finding: Modal focus trap failure.

Fix:
- Trap focus inside modal.
- Restore focus to invoking element on close.
- Block background screen input while modal is active.
```

---

### Example 5: UI Animation Ignores Reduced Motion

Issue:

```text
Inventory screen uses a long slide/zoom transition even when reduced motion is enabled.
```

Response:

```md
Finding: Motion accessibility failure.

Fix:
- Add reduced-motion alternative such as instant state change or short fade.
- Ensure animation is skippable if it delays interaction.
```

---

### Example 6: UI Sound Direct Playback

Issue:

```text
button.play_sound("click.wav")
```

Response:

```md
Finding: UI audio bypasses audio event system.

Fix:
- Trigger approved audio event, such as `sfx_ui_button_click`.
- Audio system handles routing, cooldown, bus, and volume.
```

---

### Example 7: Blocking Load

Issue:

```text
Opening inventory synchronously loads every item icon.
```

Response:

```md
Finding: Game-thread blocking risk.

Fix:
- Preload common icons or load asynchronously.
- Show loading/placeholder state.
- Virtualize inventory list.
- Cache loaded icons.
```

---

### Example 8: Resolution Failure

Issue:

```text
Quest tracker clips objective text at minimum supported resolution.
```

Response:

```md
Finding: Resolution/layout failure.

Fix:
- Add wrapping or scrolling.
- Respect safe area.
- Test min resolution with maximum text scale and expanded localization.
```

---

### Example 9: Color-Only Information

Issue:

```text
Quest rarity is shown only by text color.
```

Response:

```md
Finding: Color-only information.

Fix:
- Add icon, label, shape, or pattern indicating rarity.
- Verify in colorblind modes.
```

---

### Example 10: User Correction

User says:

```text
All settings changes must be previewed in UI but only committed when the player presses Apply.
```

Response pattern:

```text
Understood. I’ll treat settings screens as using staged UI state plus an Apply command, not direct settings mutation. Store this as a durable UI rule only if approved.
```

---

### Example 11: Case Where the Rule Must Not Learn

User says during prototyping:

```text
Just hardcode these menu strings for now.
```

Response pattern:

```text
That can be treated as prototype-only if clearly scoped. Production UI under `src/ui/**` still requires localization keys for all user-facing text.
```

---

## Final UI Code Rule

UI code under `src/ui/**` must be:

- presentation-only,
- state-boundary safe,
- command/event driven,
- localized,
- keyboard/mouse and gamepad accessible,
- focus-safe,
- scalable,
- colorblind-safe,
- reduced-motion compliant,
- audio-event routed,
- non-blocking,
- resolution-tested,
- state-complete,
- evidence-backed,
- and honest about unresolved validation.