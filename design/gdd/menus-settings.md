# Menus & Settings

> **Status**: Designed (pending review)
> **Author**: Brian + Codex
> **Last Updated**: 2026-04-24
> **Last Verified**: 2026-04-24 — Phase 5 self-check readback
> **Implements Pillar**: Primary — **P5 Stakes Are Honest**. Supports — **P2 The Silence Is Sacred**.

## Summary

Menus & Settings is Gravenspire's Foundation-layer interface shell: title flow, pause flow, settings, input remapping, and fail-loud system messages. It makes save/load and world-structure failures legible to the player without turning normal transitions or session resume into UI theater.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `World Structure; Save / Load & Persistence`

## Overview

Menus & Settings owns the player-facing control surfaces that sit outside moment-to-moment play: title menu, pause menu, settings, input remapping, manual save entry point, quit confirmation, and system failure presentation. It consumes Foundation-layer events from Save / Load & Persistence and World Structure, but it does not own their state machines or redesign their contracts. Save/Load owns save integrity, write outcomes, loader rejection classes, and Session-Exit Save gating; World Structure owns zone loading, zone errors, transition timing, and the silent `SessionResumeEvent` ordering contract. Menus & Settings turns those contracts into restrained, legible UI: optional save-in-progress affordances, loud save/load failure modals, a non-dismissible `ZoneError` surface, a non-blocking zone-load overrun indicator, and the Session-Exit Save failure modal with `Retry` / `Quit Without Saving`. Normal play remains quiet: routine `ZoneLoading` does not become a loading-screen ritual, successful saves do not celebrate themselves, and `SessionResumeEvent` never produces a welcome-back banner.

## Player Fantasy

Menus in Gravenspire should feel like stepping into the vestibule of a working institution: quiet, legible, and unsentimental. The player is not being celebrated or guided by a theme-park shell; they are opening a ledger, checking their controls, deciding whether to continue, and accepting the consequences of leaving. Settings are a practical compact with the game: every key can be rebound, audio can be quieted, readability can be adjusted, and the interface gets out of the way when the world is speaking.

Most of the time, Menus & Settings should be forgettable in the best sense. A successful save does not congratulate the player. A normal zone transition does not become a loading-screen ceremony. Returning after days away does not produce a welcome-back banner, because `SessionResumeEvent` is a silent system signal and the world should reveal its changes through downstream systems. When something is wrong, the tone changes: save/load failure, missing-zone state, corrupt bundles, or an exit-save failure are stated plainly and blockingly. The fantasy is not comfort; it is trust. The player believes the record because the game is quiet on success and blunt on failure.

## Detailed Design

### Core Rules

1. **Presentation shell only.** Menus & Settings owns title flow, pause flow, settings, input remapping, manual-save entry, quit confirmation, and system-message presentation. It does not own Save/Load state, World Structure state, combat HUD state, dialogue state, journal state, or faction-board content.

2. **No silent failure.** Any `SaveFailedEvent`, `LoadRejected(failureClass)`, or `ZoneErrorEvent(reason, zoneId)` reaching Menus must produce a player-visible blocking modal unless a more specific rule below marks the surface non-blocking. Success may be quiet; failure may not.

3. **Session-Exit Save failure modal.** On Session-Exit Save `SaveFailedEvent`, Menus blocks the quit flow and presents exactly two choices: `Retry` and `Quit Without Saving`. `Retry` re-queues the Session-Exit Save through Save/Load. `Quit Without Saving` exits without another save attempt. Menus must not add a third "continue anyway" path that returns to gameplay after a failed exit save.

4. **Engine-level fallback is mandatory.** The Session-Exit Save failure modal cannot depend solely on Menus being present. If the Menus subscriber is absent, disabled, or not yet initialized, the engine-level quit-flow hook surfaces the same `Retry` / `Quit Without Saving` choice.

5. **Manual Save routes through Save/Load.** Manual Save is invoked from the pause menu, but Menus never writes save data directly. It dispatches a manual-save request through the Save/Load / World Structure `SaveCheckpointing` path defined by the approved Save/Load GDD.

6. **Save status is restrained.** `SaveInProgress` may show a small non-blocking indicator. `SaveWriteConfirmed` hides the indicator and may update a timestamp in the pause menu, but it must not produce a celebratory toast or interrupt play. `SaveFailedEvent` follows Rule 2 or Rule 3 depending on trigger context.

7. **Load rejection carries the failure class.** `LoadRejected(failureClass)` must surface the distinct failure class from Save/Load's failure-mode matrix. The UI may translate the class into player-safe prose, but logs and test hooks retain the exact class.

8. **World Structure errors are terminal for the current session.** `ZoneErrorEvent(reason, zoneId)` produces a non-dismissible modal with a return-to-title action. Menus does not offer retry, relocation, or continue options unless a future ADR explicitly changes the World Structure contract.

9. **Zone loading is normally invisible.** Routine `ZoneLoading` does not show a loading screen, banner, or progress bar. `ZoneLoadOverrunEvent(elapsed_ms)` may show a low-weight non-blocking indicator, hidden on `ZoneActiveEvent`.

10. **Session resume is silent.** Menus must not subscribe to `SessionResumeEvent` for a welcome-back banner, recap modal, elapsed-time toast, or any other player-facing surface. Downstream world systems reveal resume effects organically.

11. **Input remapping is first-class.** Every gameplay action exposed through input must be bindable through Settings using Unity's Input System binding override model. Keyboard/mouse is the primary scheme; gamepad bindings are permitted for accessibility but do not displace keyboard/mouse.

12. **Chat/input protection.** Once chat exists, typing into chat or text fields must suppress gameplay hotkeys and menu shortcuts except explicit text-field escape/cancel behavior. Menus establishes this rule now so future Social Systems and Dialogue UI do not fight the keybinding layer.

13. **Settings apply without corrupting play state.** Audio, display, accessibility, and input settings may be changed from title or pause contexts. Applying settings must not trigger save/load writes except for settings persistence itself, which is separate from gameplay save integrity.

14. **No convenience breach of Pillar 2.** Menus must not introduce quest markers, auto-pathing, fast-travel shortcuts, zone relocation, resume recaps, or other convenience surfaces that contradict the approved World Structure and game-concept silence/no-marker posture.

15. **Implementation substrate.** Runtime menus should target Unity UI Toolkit unless a later UI architecture ADR chooses otherwise. Any UI Toolkit motion or transform behavior must avoid deprecated `VisualElement.transform` usage in Unity 6.3.

16. **Title-menu actions are gated by save-record state.** Under Save/Load's T1 single-record constraint (Save/Load Rule 2 + D003), title-menu actions respect save-record existence: (a) Continue with no record is disabled with a peripheral "no saved character" note, or collapses to first-run bootstrap with inline explanation; no `LoadRejected` event is emitted, because Save/Load Rule 14's first-run path is not a failure. (b) New Game with an existing record requires a blocking destructive-overwrite confirmation modal naming the existing character; silent overwrite is forbidden. (c) Continue with an existing record invokes the normal load path. (d) New Game with no record invokes the first-run path directly. Menus consumes a save-record-existence query from Save/Load whose mechanism is implementation-level per Save/Load Rule 14 slot-initialised status and the storage-backend ADR.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|-----------------|----------------|----------|
| `TitleMenu` | App launch; return from gameplay; acknowledged fatal/load failure | Gated Continue/New Game selected; Quit selected | Shows Continue/New Game, Settings, Quit. Does not run gameplay. Continue and New Game are gated by Rule 16 save-record state: existing-record Continue invokes the normal load path; no-record New Game invokes first-run path; existing-record New Game requires destructive-overwrite confirmation; no-record Continue must not emit `LoadRejected`. |
| `SessionLoadingSurface` | Continue/New Game selected from `TitleMenu` | Successful resume reaches gameplay; `LoadRejected(failureClass)` enters `LoadRejectedModal` | Minimal loading surface only. Does not display `SessionResumeEvent` recap or elapsed-time messaging. |
| `GameplayMenusHidden` | Playable session active; no menu focus | Pause/menu input; blocking system event; quit request | Default in-play state. Routine `ZoneLoading` stays invisible here unless `ZoneLoadOverrunEvent` fires. |
| `PauseMenu` | Player opens menu during active gameplay | Resume selected; Settings selected; Manual Save selected; Quit selected; fatal event interrupts | UI focus state, not a World Structure state. Menus may request local T1 input pause, but it must not claim ownership of world simulation time. |
| `SettingsMenu` | Settings selected from `TitleMenu` or `PauseMenu` | Back/Apply selected; rebind action selected | Edits audio, display, accessibility, and input settings. Gameplay save is not triggered by settings changes. |
| `RebindingCapture` | Player selects an input binding to change | Binding accepted; cancel selected; invalid duplicate rejected | Captures exactly one binding through Unity Input System override flow. Text input fields suppress gameplay hotkeys. |
| `SaveStatusIndicator` | `SaveInProgress` received | `SaveWriteConfirmed` or `SaveFailedEvent` received | Non-blocking, low-weight status indicator. Hidden on success/failure; success does not toast. |
| `ManualSaveFailureModal` | Manual Save emits `SaveFailedEvent` outside Session-Exit flow | Player acknowledges | Blocking fail-loud modal over previous menu context. Does not pretend save succeeded. |
| `LoadRejectedModal` | `LoadRejected(failureClass)` received | Player acknowledges return to title | Blocking modal with player-safe prose and exact failure class retained for logs/tests. No partial session becomes playable. |
| `ZoneOverrunIndicator` | `ZoneLoadOverrunEvent(elapsed_ms)` received | `ZoneActiveEvent` or `ZoneErrorEvent` received | Non-blocking low-weight loading indicator. No progress bar for normal transitions. |
| `ZoneErrorModal` | `ZoneErrorEvent(reason, zoneId)` received | Player returns to title | Non-dismissible terminal-session modal. No retry, relocation, or continue action. |
| `QuitConfirm` | Quit selected from title or pause | Cancel selected; quit confirmed | Confirms intentional clean shutdown. Confirmed quit fires Session-Exit Save when a playable session exists. |
| `SessionExitSavePending` | Clean in-app quit confirmed while gameplay save state exists | `SaveWriteConfirmed`; `SaveFailedEvent` | Blocks shutdown while final save is attempted. On success, application exits. |
| `SessionExitSaveFailedModal` | Session-Exit Save emits `SaveFailedEvent` | `Retry`; `Quit Without Saving` | Exactly two choices. Retry re-queues Session-Exit Save. Quit Without Saving exits without another save attempt. Same behavior must exist through engine-level fallback if Menus is absent. |

### Interactions with Other Systems

| System | Inputs Consumed by Menus | Outputs Published by Menus | Ownership Boundary | Dependency |
|--------|--------------------------|-----------------------------|--------------------|------------|
| **Save / Load & Persistence** | `SaveInProgress`, `SaveWriteConfirmed`, `SaveFailedEvent`, `LoadRejected(failureClass)`; Session-Exit Save gate result; save-record-existence query for title-menu action gating (mechanism per Save/Load Rule 14 slot-initialised status) | Manual Save request; Session-Exit Save retry request; player acknowledgement for `Quit Without Saving` | Save/Load owns save validity, write ordering, failure classes, atomicity, and slot-initialised status. Menus owns presentation, player acknowledgement, and destructive-overwrite confirmation. | **Soft consumer / hard presentation obligation when present** |
| **World Structure** | `ZoneLoadOverrunEvent(elapsed_ms)`, `ZoneErrorEvent(reason, zoneId)`, `ZoneActiveEvent` for hiding overrun UI | No zone-state writes. Manual Save request is routed through Save/Load/WS save checkpointing, not direct WS mutation. | World Structure owns zone loading, zone errors, transition state, and `SessionResumeEvent` ordering. Menus owns only UI surfaces for WS events. | **Soft consumer** |
| **Engine-level Quit Flow** | Session-Exit Save `SaveFailedEvent` when Menus is absent or disabled | Retry / Quit Without Saving acknowledgement via fallback modal | Engine fallback duplicates only the exit-save failure choice. It does not become a general menu system. | **Hard fallback** |
| **Input System** | Action maps, binding metadata, current control scheme, text-field focus state | Binding override changes; input-focus suppression requests while menus/text fields are active | Input System owns device polling and binding mechanics. Menus owns settings UI and conflict presentation. | **Hard** |
| **Settings Persistence** | Current non-gameplay settings payload | Audio/display/accessibility/input settings updates | Settings persistence is separate from gameplay Save/Load integrity. Gameplay save is not triggered by settings changes. | **Hard** |
| **Layer 1 HUD** | None directly required at MVP | Menu-open/menu-close focus state; optional suppression of HUD hotkeys while modal is active | HUD owns combat/status presentation. Menus owns pause/title/settings/failure modals. | **Soft** |
| **Dialogue UI Panel** | Text-field focus and modal-stack status once authored | Input suppression while dialogue text entry is active | Dialogue owns conversation presentation/content. Menus owns global keybinding and focus rules. | **Future soft** |
| **Personal Journal** | None at MVP | None at MVP | Journal is a diegetic information surface, not part of Menus. Menus may link to it later only by explicit GDD/UX decision. | **No direct T1 dependency** |
| **Faction Board UI** | None | None | Faction Board is a world/diegetic surface. Menus must not duplicate board state into a recap or convenience screen. | **No direct T1 dependency** |
| **Audio System** | Current volume/mute settings; optional UI warning tone hooks for blocking failures | Volume settings changes; UI event hints for modal open/close | Audio owns sound playback. Menus owns settings controls and may request UI tones without owning mix logic. | **Soft** |
| **Accessibility** | Accessibility settings schema once authored | Settings changes for contrast, text size, subtitle/caption preferences, reduced motion | Menus provides the MVP settings surface; the Accessibility GDD owns full standards when authored. | **Hard for settings surface; full pass T3** |

## Formulas

Menus & Settings owns no gameplay progression, save-integrity, zone-loading, or simulation formulas. Those remain owned by their source GDDs. This section defines only UI/settings derivations used by Menus itself.

### UI Scale Clamp

```text
ui_scale_effective = clamp(ui_scale_setting, ui_scale_min, ui_scale_max)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `ui_scale_setting` | float | 0.85-1.50 | Settings UI | Player-selected UI scale. |
| `ui_scale_min` | float | 0.85 | Menus tuning | Minimum supported menu scale. |
| `ui_scale_max` | float | 1.50 | Menus tuning | Maximum supported menu scale before layout reflow must be redesigned. |
| `ui_scale_effective` | float | 0.85-1.50 | Calculated | Scale applied to Menus UI Toolkit root classes. |

**Expected output range**: 0.85 to 1.50
**Edge case**: Any value loaded outside range clamps before layout applies.

### Audio Bus Effective Volume

```text
effective_bus_volume = is_muted ? 0 : master_volume * bus_volume
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `is_muted` | bool | true/false | Settings UI | Per-bus or master mute state. |
| `master_volume` | float | 0.0-1.0 | Settings UI | Master output scalar. |
| `bus_volume` | float | 0.0-1.0 | Settings UI | Music, ambience, SFX, dialogue, or UI bus scalar. |
| `effective_bus_volume` | float | 0.0-1.0 | Calculated | Scalar sent to Audio System for that bus. |

**Expected output range**: 0.0 to 1.0
**Edge case**: Muted buses always output 0 regardless of slider positions.

### Zone Overrun Visibility Gate

```text
show_zone_overrun_indicator = zone_load_elapsed_ms >= (zone_overrun_window_seconds * 1000) AND zone_active == false AND zone_error == false
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `zone_load_elapsed_ms` | int | >= 0 | `ZoneLoadOverrunEvent(elapsed_ms)` / WS telemetry | Elapsed loading time for current zone load. |
| `zone_overrun_window_seconds` | int | 3-10; default 5 | World Structure tuning knob | The approved overrun threshold. |
| `zone_active` | bool | true/false | `ZoneActiveEvent` | True once normal transition completes. |
| `zone_error` | bool | true/false | `ZoneErrorEvent` | True when transition enters terminal error. |
| `show_zone_overrun_indicator` | bool | true/false | Calculated | Whether Menus shows the low-weight overrun indicator. |

**Expected output range**: boolean
**Edge case**: `ZoneErrorEvent` hides the overrun indicator and replaces it with `ZoneErrorModal`.

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|-------------------|-----------|
| Menus subscriber absent during Session-Exit Save failure | Engine-level quit-flow hook surfaces the same `Retry` / `Quit Without Saving` modal. | Save/Load requires this fallback; exit-save acknowledgement cannot depend on Menus being alive. |
| Menus subscriber absent during non-exit `SaveFailedEvent` or `LoadRejected` | Event is logged; Save/Load and World Structure keep their own consequences. Menus makes no claim if absent. | Menus is a soft consumer for normal UI presentation, not a correctness dependency. |
| `SaveFailedEvent` and `ZoneErrorEvent` arrive for the same transition failure | Menus coalesces presentation into one terminal `ZoneErrorModal`, preserving save-failure details in logs/test hooks. | Prevents modal stacking while respecting fail-loud behavior. |
| Duplicate failure event while a blocking failure modal is already open | Existing modal remains; details update only if the new event is higher severity or different class. No stacked duplicates. | Avoids UI deadlocks and repeated acknowledgement loops. |
| `LoadRejected(failureClass)` fires before root menu UI is fully initialized | Boot shell holds the rejection payload and displays `LoadRejectedModal` as soon as the root document exists. Gameplay never enables. | Load rejection must be visible and must not allow partial play. |
| `ZoneLoadOverrunEvent` followed by `ZoneActiveEvent` | Hide `ZoneOverrunIndicator`; do not show success toast. | Overrun is advisory and non-blocking; normal transition completion should return to quiet. |
| `ZoneLoadOverrunEvent` followed by `ZoneErrorEvent` | Hide `ZoneOverrunIndicator`; show `ZoneErrorModal`. | Terminal error supersedes advisory loading feedback. |
| `SessionResumeEvent` is accidentally routed to Menus | Menus ignores it and emits a development warning in non-shipping builds. No player-facing UI. | World Structure explicitly makes session resume silent; downstream systems reveal effects organically. |
| Manual Save selected during `ZoneLoading` | Menus disables the Manual Save command or sends the request knowing WS/SaveLoad may discard it per the approved transition-save contract. No second save indicator is stacked. | World Structure already captures transition state; duplicate manual saves during loading create inconsistency risk. |
| New Game selected while an existing T1 character record exists | Menus presents a blocking destructive-overwrite confirmation naming the existing character. Proceeding is explicit; cancel returns to TitleMenu with no save mutation. | T1 has one active local character record; silent overwrite would hide destructive state loss. |
| Continue selected while no T1 character record exists | Menus disables Continue with a peripheral explanatory note, or routes to first-run bootstrap with inline explanation. No `LoadRejected` event is emitted. | Save/Load Rule 14 distinguishes no-save first-run from missing-file data loss; Menus must not turn expected bootstrap into a failure. |
| Player opens Settings while `SaveStatusIndicator` is visible | Settings may open; save indicator remains non-blocking and follows its normal hide-on-confirm/fail behavior. | Settings edits must not interfere with gameplay save completion. |
| Rebind captures a key already bound to another action | Menus blocks acceptance and asks the player to clear or replace the conflicting binding. No silent overwrite. | MMO controls need reliable keybinding; silent conflicts are input loss. |
| Text field focused while gameplay hotkey fires | Text input wins; gameplay hotkey and menu shortcut are suppressed except explicit cancel/escape behavior. | Prevents chat/dialogue/settings text from triggering gameplay actions. |
| Settings payload loads with out-of-range UI scale or volume values | Clamp to the formula ranges before applying; log the correction. | Corrupt settings must not break menu usability. |
| Player selects Quit Without Saving after exit-save failure | Application exits without another save attempt; last successful save remains recovery baseline. | This is the explicit player acknowledgement required by Save/Load. |

## Dependencies

| System | Direction | Nature of Dependency |
|--------|-----------|----------------------|
| World Structure | Menus depends on World Structure | Consumes `ZoneLoadOverrunEvent`, `ZoneErrorEvent`, and `ZoneActiveEvent`; must preserve normal invisible `ZoneLoading` and silent `SessionResumeEvent` posture. |
| Save / Load & Persistence | Menus depends on Save/Load | Consumes `SaveInProgress`, `SaveWriteConfirmed`, `SaveFailedEvent`, and `LoadRejected(failureClass)`; publishes Manual Save and Session-Exit Save retry requests. |
| Engine-level Quit Flow | Menus has required fallback peer | Provides Retry / Quit Without Saving fallback when Menus is absent during Session-Exit Save failure. |
| Unity Input System | Menus depends on Input System | Provides bindable action maps and binding override persistence for keyboard/mouse primary controls and optional gamepad accessibility bindings. |
| Settings Persistence | Menus depends on settings storage | Stores non-gameplay settings separately from gameplay save data. |
| Audio System | Audio depends on Menus settings | Receives master/music/ambience/SFX/dialogue/UI volume and mute changes; owns playback/mix application. |
| Layer 1 HUD | HUD depends on Menus focus state | Suppresses or restores HUD hotkeys and input while menus or blocking modals have focus. |
| Dialogue UI Panel | Future mutual focus dependency | Dialogue text fields and Menus keybinding layer share text-input suppression rules once Dialogue UI is authored. |
| Accessibility | Accessibility depends on Menus for MVP settings surface | Menus exposes baseline UI scale, contrast/readability, reduced motion, and subtitle/caption preferences; full standards wait for Accessibility GDD. |
| Personal Journal | No direct dependency at MVP | Journal remains a diegetic surface; Menus does not duplicate journal content. |
| Faction Board UI | No direct dependency at MVP | Faction Board remains a diegetic world surface; Menus does not provide faction recap or event-board convenience UI. |

## Tuning Knobs

| Parameter | Current Value | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|---------------|------------|--------------------|--------------------|
| `ui_scale_default` | 1.00 | 0.85-1.50 | Larger menus/text; risks layout crowding at the high end. | Smaller menus/text; risks readability loss at the low end. |
| `ui_scale_min` | 0.85 | 0.75-1.00 | Protects readability by preventing tiny UI. | Allows denser UI but risks accessibility failures. |
| `ui_scale_max` | 1.50 | 1.25-2.00 | Improves low-vision access; increases reflow burden. | Reduces layout risk but limits accessibility usefulness. |
| `master_volume_default` | 0.80 | 0.00-1.00 | Louder global mix. | Quieter global mix. |
| `music_volume_default` | 0.65 | 0.00-1.00 | Stronger score presence; may violate silence-forward tone. | More silence/ambience dominance. |
| `ambience_volume_default` | 0.75 | 0.00-1.00 | Stronger environmental read. | Flatter world presence. |
| `sfx_volume_default` | 0.80 | 0.00-1.00 | Clearer feedback. | Softer interaction/combat feedback. |
| `dialogue_volume_default` | 0.85 | 0.00-1.00 | Clearer spoken dialogue once voiced content exists. | Dialogue may lose clarity. |
| `ui_volume_default` | 0.55 | 0.00-1.00 | Stronger menu/modal tones. | Quieter UI, closer to P2 restraint. |
| `save_indicator_min_visible_ms` | 300 ms | 0-1000 ms | More visible save feedback. | Save indicator may flash too quickly to read. |
| `modal_input_lockout_ms` | 150 ms | 0-500 ms | Reduces accidental double-confirm. | More responsive, but easier to misclick through critical modals. |
| `settings_apply_debounce_ms` | 250 ms | 0-1000 ms | Fewer repeated settings writes. | More immediate settings persistence, more write churn. |
| `zone_overrun_indicator_min_visible_ms` | 500 ms | 0-1500 ms | Makes overrun notice readable. | Keeps transient overrun quieter. |
| `rebind_duplicate_policy` | BlockAndAsk | BlockAndAsk / ReplaceWithConfirm | Safer binding edits with explicit player choice. | Faster rebinding if replacement is confirmed. |

## Visual/Audio Requirements

Menus & Settings uses the art bible's Layer 1 practical UI register: minimal, low-saturation, geometric, and peripheral where possible. It must not use true black, true white, red/green danger signaling, gradients, rounded corners, drop shadows, glowing prompts, or animated flourish. Audio is sparse; silence remains the default.

| Event / Surface | Visual Feedback | Audio Feedback | Priority |
|-----------------|-----------------|----------------|----------|
| Title menu idle | Static Layer 1-style menu panel over title/world background; no looping animation beyond background scene if one exists. | Ambient title bed only if Audio System provides one; otherwise silence. | MVP |
| Pause menu opened | Quiet architectural panel, Iron Seam/Bone Pale register, no center-screen spectacle beyond practical menu layout. | Optional single low UI tick. | MVP |
| Settings menu opened | Same menu register; controls grouped by Audio, Display, Accessibility, Input. | Optional low UI tick. | MVP |
| Rebinding capture started | Focused binding row gains restrained outline/frame state; no flashing prompt. | Optional soft tick. | MVP |
| Duplicate/invalid binding | Inline validation text plus blocked accept state; no red/green signaling. | Optional low warning tone. | MVP |
| Save in progress | Small peripheral indicator, bottom-right or pause-menu status row; obeys `save_indicator_min_visible_ms`. | None. | MVP |
| Save confirmed | Indicator hides; optional timestamp update in pause menu only. No toast. | None. | MVP |
| Manual Save failed | Blocking modal over prior menu context; player-safe error text and acknowledgement. | One restrained warning tone, if UI audio exists. | MVP |
| `LoadRejected(failureClass)` | Blocking modal before gameplay; player-safe prose plus exact class retained in logs/tests. | One restrained warning tone. | MVP |
| `ZoneLoadOverrunEvent` | Low-weight peripheral loading indicator; hidden on `ZoneActiveEvent` or replaced by `ZoneErrorModal`. | None. | MVP |
| `ZoneErrorEvent` | Non-dismissible terminal-session modal with return-to-title action. No retry/relocate/continue controls. | One restrained warning tone; no sting. | MVP |
| Quit confirmation | Plain confirmation modal. If gameplay session exists, communicate final save attempt will run. | Optional low UI tick. | MVP |
| Session-Exit Save pending | Blocking "saving before exit" state; no progress promise unless implementation can report real progress. | None. | MVP |
| Session-Exit Save failed | Blocking modal with exactly `Retry` and `Quit Without Saving`. Same presentation for engine fallback if Menus absent. | One restrained warning tone. | MVP |
| Settings applied | Control value visibly updates; no success toast. | Optional soft tick only for direct interaction. | MVP |

## UI Requirements

Menus & Settings is a Layer 1 practical UI surface. It may frame access to diegetic systems later, but it must not duplicate Layer 2 world-information surfaces such as the Personal Journal or Faction Board.

| Information / Surface | Display Location | Update Frequency | Condition |
|-----------------------|------------------|------------------|-----------|
| Title actions: Continue, New Game, Settings, Quit | Title menu | On title entry | App launch or return to title |
| Pause actions: Resume, Manual Save, Settings, Quit | Pause menu | On pause open | Playable session active and no blocking modal open |
| Settings categories: Audio, Display, Accessibility, Input | Settings menu tabs/sections | On settings open and value change | Title or pause context |
| Keybinding list | Settings / Input | On settings open; after each binding change | Input settings selected |
| Rebinding capture prompt | Focused binding row/modal | While capture active | Player selects an action binding |
| Binding conflict message | Inline validation row | On attempted duplicate/invalid bind | Conflict detected |
| Save in progress | Peripheral indicator or pause status row | On `SaveInProgress`; hidden on outcome | Any save write begins |
| Last confirmed save timestamp | Pause status row only | On `SaveWriteConfirmed` | Pause menu visible or opened later |
| Manual Save failure | Blocking modal over current menu context | Once per failure event | Manual Save emits `SaveFailedEvent` |
| Load rejection | Blocking modal before gameplay | Once per rejection | `LoadRejected(failureClass)` received |
| Zone overrun | Peripheral non-blocking indicator | On `ZoneLoadOverrunEvent`; hidden on `ZoneActiveEvent` or `ZoneErrorEvent` | Load exceeds approved overrun window |
| Zone error | Non-dismissible terminal modal | Once per terminal event | `ZoneErrorEvent(reason, zoneId)` received |
| Quit confirmation | Modal | On quit request | Player selects Quit from title or pause |
| Session-exit save pending | Blocking quit-flow modal/state | While final save is in progress | Clean quit confirmed with gameplay state present |
| Session-exit save failure | Blocking quit-flow modal | On failed exit save | Session-Exit Save emits `SaveFailedEvent` |
| Engine fallback exit-save failure | Engine modal outside Menus | On failed exit save when Menus absent | Menus subscriber absent/disabled/uninitialized |
| `SessionResumeEvent` | No display | Never | Must remain silent |
| Routine `ZoneLoading` | No display | Never, unless overrun fires | Normal transition in progress |

### Input and Focus Rules

- Keyboard/mouse is the primary navigation scheme.
- Every menu command, modal choice, tab change, cancel/back action, and binding action must be keybindable.
- Focus order must be deterministic and testable for keyboard-only use.
- Blocking modals trap focus until resolved.
- `Escape` / back behavior is consistent: close current non-blocking screen, cancel rebinding, or decline confirmation; it must not dismiss terminal failure modals unless an explicit safe action is focused.
- Text-field focus suppresses gameplay hotkeys and global menu shortcuts except explicit cancel behavior.
- Routine `ZoneLoading` must not steal focus.
- `ZoneLoadOverrunEvent` must not steal focus.
- `ZoneErrorModal`, `LoadRejectedModal`, and `SessionExitSaveFailedModal` must steal and trap focus.

## Cross-References

| This Document References | Target GDD / Source | Specific Element Referenced | Nature |
|--------------------------|---------------------|-----------------------------|--------|
| Menus consumes Save/Load status events | `design/gdd/save-load-persistence.md` | `SaveInProgress`, `SaveWriteConfirmed`, `SaveFailedEvent`, `LoadRejected(failureClass)` | Data dependency |
| Manual Save routing | `design/gdd/save-load-persistence.md` | Rule 5 Manual Save trigger through Save/Load / WS `SaveCheckpointing` path | State trigger |
| T1 single-record title gating | `design/gdd/save-load-persistence.md` | Rule 2 single active local character record at T1 | Rule dependency |
| First-run vs missing-file distinction | `design/gdd/save-load-persistence.md` | Rule 14 first-run path is not a `LoadRejected` failure; missing initialized slot emits `SaveMissing` | Rule dependency |
| Session-Exit Save failure modal | `design/gdd/save-load-persistence.md` | Rule 13: Retry / Quit Without Saving | Rule dependency |
| Engine fallback for exit-save failure | `design/gdd/save-load-persistence.md` | Edge D2 / H-SL-CS-Menus-Absent | Rule dependency |
| Load failure classes | `design/gdd/save-load-persistence.md` | Failure-mode matrix and `LoadRejected(failureClass)` | Data dependency |
| Menus consumes World Structure UI events | `design/gdd/world-structure.md` | `ZoneLoadOverrunEvent`, `ZoneErrorEvent`, `ZoneActiveEvent` | State trigger |
| Routine zone loading remains invisible | `design/gdd/world-structure.md` | `ZoneLoading` two-phase transition; UI only on overrun/error | Rule dependency |
| Session resume remains silent | `design/gdd/world-structure.md` | Rule 13 `SessionResumeEvent` silent signal; H-CR-13b ordering | Rule dependency |
| Zone error is terminal | `design/gdd/world-structure.md` | `ZoneError` state and `ZoneErrorEvent(reason, zoneId)` | Rule dependency |
| Zone overrun threshold | `design/registry/entities.yaml` | `zone_overrun_window_seconds` default 5s, safe range 3-10s from World Structure | Data dependency |
| Input remapping requirement | `.claude/docs/technical-preferences.md` | Keyboard/mouse primary; every action keybindable; chat input must not be swallowed | Rule dependency |
| Unity runtime UI substrate | `docs/engine-reference/unity/modules/ui.md` | UI Toolkit recommended for new runtime UI | Rule dependency |
| Unity input substrate | `docs/engine-reference/unity/modules/input.md` | Input System package recommended; legacy input deprecated | Rule dependency |
| Unity 6.3 UI transform caveat | `docs/engine-reference/unity/VERSION.md` | `VisualElement.transform` deprecated; use style translate/rotate/scale | Rule dependency |
| Layer 1 practical UI styling | `design/art/art-bible.md` | Layer 1 visual style, button geometry, typography, forbidden UI colors/effects | Rule dependency |
| No convenience UI | `design/gdd/game-concept.md` | Pillar 2 / anti-pillar rejection of markers, auto-pathing, convenience friction removal | Rule dependency |

## Acceptance Criteria

### Save / Load UI Contract

**H-MS-SL-01 — Save-in-progress indicator**
**GIVEN** any save write emits `SaveInProgress`, **WHEN** Menus is present, **THEN** a small non-blocking indicator may appear and gameplay/menu navigation remains usable.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-MS-SL-02 — Save confirmation remains quiet**
**GIVEN** `SaveWriteConfirmed` is received, **WHEN** a save indicator is visible, **THEN** the indicator hides and no success toast, banner, modal, audio sting, or gameplay interruption occurs.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-MS-SL-03 — Pause-menu save timestamp placement (conditional)**
**GIVEN** the implementation surfaces a last-confirmed-save timestamp, **WHEN** `SaveWriteConfirmed` is received while Pause Menu is open, **THEN** the timestamp appears in the pause status row and is not duplicated in the title menu, HUD, toast, banner, save indicator, or any other UI surface.
*Integration | ui-programmer + qa-tester | advisory at T1 (vacuous when timestamp is not surfaced; promotes to T1-blocking when implementation surfaces a timestamp)*

**H-MS-SL-04 — Manual Save failure modal**
**GIVEN** a non-exit Manual Save emits `SaveFailedEvent`, **WHEN** Menus is present, **THEN** a blocking failure modal appears and no UI implies the save succeeded.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-MS-SL-05 — LoadRejected preserves failure class**
**GIVEN** `LoadRejected(failureClass)` is received, **WHEN** Menus displays the rejection, **THEN** gameplay never becomes playable; a blocking modal appears; player-facing copy may be translated; logs/test hooks retain the exact `failureClass`.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-MS-SL-06 — Session-Exit Save failure choices**
**GIVEN** Session-Exit Save emits `SaveFailedEvent`, **WHEN** the quit-flow modal appears, **THEN** shutdown is blocked and the modal contains exactly two actions: `Retry` and `Quit Without Saving`.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-MS-SL-07 — Retry requeues exit save**
**GIVEN** the Session-Exit Save failure modal is shown, **WHEN** `Retry` is selected, **THEN** Menus re-queues Session-Exit Save through Save/Load without exiting.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-MS-SL-08 — Quit Without Saving exits**
**GIVEN** the Session-Exit Save failure modal is shown, **WHEN** `Quit Without Saving` is selected, **THEN** the application exits without another save attempt and the last successful save remains the recovery baseline.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-MS-SL-09 — Settings do not dirty gameplay save**
**GIVEN** settings are changed from Title or Pause, **WHEN** the setting applies, **THEN** only settings persistence writes; gameplay save data is not written, mutated, or marked dirty.
*Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-MS-SL-10 — New Game overwrite confirmation**
**GIVEN** Save/Load reports an existing T1 active local character record, **WHEN** the player selects New Game from TitleMenu, **THEN** Menus presents a blocking destructive-overwrite confirmation naming the existing character; cancelling returns to TitleMenu without mutating gameplay save data.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-MS-SL-11 — Continue with no record**
**GIVEN** Save/Load reports no T1 active local character record, **WHEN** the player selects or inspects Continue from TitleMenu, **THEN** Continue is disabled with a peripheral explanatory note OR routes to first-run bootstrap with inline explanation; no `LoadRejected` event is emitted.
*Integration | ui-programmer + qa-tester | T1-blocking*

### World Structure UI Contract

**H-MS-WS-01 — Routine ZoneLoading invisible**
**GIVEN** routine `ZoneLoading` begins, **WHEN** no overrun or error event fires, **THEN** Menus shows no loading screen, banner, progress bar, modal, focus steal, or transition ceremony.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-MS-WS-02 — Zone overrun is non-blocking**
**GIVEN** `ZoneLoadOverrunEvent(elapsed_ms)` is received, **WHEN** Menus is present, **THEN** only a low-weight non-blocking indicator may show; it does not block movement, steal focus, or offer cancel/retry.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-MS-WS-03 — ZoneActive hides overrun**
**GIVEN** `ZoneActiveEvent` follows `ZoneLoadOverrunEvent`, **WHEN** Menus receives `ZoneActiveEvent`, **THEN** the overrun indicator hides and no success confirmation appears.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-MS-WS-04 — ZoneError terminal modal**
**GIVEN** `ZoneErrorEvent(reason, zoneId)` is received, **WHEN** Menus is present, **THEN** any overrun indicator hides and a non-dismissible terminal-session modal appears with only a return-to-title path.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-MS-WS-05 — ZoneError offers no recovery shortcut**
**GIVEN** `ZoneErrorEvent(reason, zoneId)` is shown, **WHEN** the modal is inspected, **THEN** Menus offers no retry, relocation, continue, safe-zone fallback, or load-anyway action.
*Integration | qa-tester | T1-blocking*

**H-MS-WS-06 — SessionResumeEvent silent**
**GIVEN** `SessionResumeEvent(real_elapsed_seconds, last_exit_timestamp_utc)` is published, **WHEN** Menus is present, **THEN** Menus produces no player-facing UI, audio, recap, elapsed-time message, welcome-back banner, focus change, or notification.
*Integration | ui-programmer + qa-tester | T1-blocking*

### Input / Settings / UI Compliance

**H-MS-IN-01 — UI Toolkit substrate**
**GIVEN** Unity 6.3 runtime menus are implemented, **WHEN** title, pause, settings, modal, and rebind screens are inspected, **THEN** they use Unity UI Toolkit unless a later UI ADR explicitly overrides it.
*Editor-validation + Dev-build smoke | unity-ui-specialist + qa-tester | T1-blocking*

**H-MS-IN-02 — Input System only**
**GIVEN** input is implemented, **WHEN** menu/input code is inspected, **THEN** Unity Input System action maps and binding overrides are used; legacy `Input.GetKey`, `Input.GetAxis`, and direct mouse polling APIs are absent.
*Editor-validation | unity-specialist | T1-blocking*

**H-MS-IN-03 — Keyboard/mouse complete**
**GIVEN** keyboard/mouse is the primary control scheme, **WHEN** the menus are tested keyboard-only, **THEN** every gameplay action, menu command, modal choice, tab change, back/cancel action, and rebind action is reachable.
*Integration | qa-tester | T1-blocking*

**H-MS-IN-04 — T1 actions keybindable**
**GIVEN** any gameplay action exists in T1, **WHEN** Settings / Input is opened, **THEN** that action is exposed and keybindable.
*Integration | qa-tester | T1-blocking*

**H-MS-IN-05 — Rebind capture**
**GIVEN** a player rebinds an action, **WHEN** capture begins, **THEN** exactly one binding capture is active; cancel restores the prior binding; accepted overrides persist through settings persistence.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-MS-IN-06 — Binding conflicts are explicit**
**GIVEN** a proposed binding conflicts with an existing binding, **WHEN** the player attempts to accept it, **THEN** Menus blocks silent overwrite and requires explicit clear or replace confirmation.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-MS-IN-07 — Text focus suppresses gameplay hotkeys**
**GIVEN** a text field or future chat field has focus, **WHEN** gameplay hotkeys or global menu shortcuts are pressed, **THEN** they are suppressed except explicit escape/cancel behavior.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-MS-IN-08 — Modal focus trap**
**GIVEN** a blocking modal is open, **WHEN** keyboard/mouse navigation occurs, **THEN** focus remains trapped inside the modal until an allowed action is selected.
*Integration | qa-tester | T1-blocking*

**H-MS-IN-09 — Settings clamp before applying**
**GIVEN** settings payload values load outside supported ranges, **WHEN** settings are applied, **THEN** UI scale and volume values clamp before application and the correction is logged.
*Unit + Integration | gameplay-programmer + qa-tester | T1-blocking*

**H-MS-ART-01 — Layer 1 practical UI compliance**
**GIVEN** any Menus UI surface is visible, **WHEN** it is reviewed against the art bible, **THEN** it follows Layer 1 practical UI: restrained, low-saturation, geometric, quiet, and functional.
*Dev-build smoke | ui-programmer + art-lead | T1-blocking*

**H-MS-ART-02 — Forbidden UI styling absent**
**GIVEN** a menu, modal, or indicator is rendered, **WHEN** UI styles are inspected, **THEN** there is no true black, true white, red/green danger coding, gradients, glow, drop shadows, rounded flourish, or celebratory animation.
*Editor-validation + Dev-build smoke | unity-ui-specialist + qa-tester | T1-blocking*

**H-MS-ART-03 — No convenience UI**
**GIVEN** Menus exposes navigation or information, **WHEN** the surface is inspected, **THEN** it does not duplicate diegetic world surfaces or add quest markers, auto-pathing, fast travel, safe relocation, resume recap, event digest, or convenience friction removal.
*Dev-build smoke | game-designer + qa-tester | T1-blocking*

**H-MS-ART-04 — Unity 6.3 UI transform caveat**
**GIVEN** UI Toolkit styling is implemented in Unity 6.3, **WHEN** UI code/styles are inspected, **THEN** deprecated `VisualElement.transform` usage is avoided in favor of supported style translate/rotate/scale behavior.
*Editor-validation | unity-ui-specialist | T1-blocking*

### Fallback Behavior

**H-MS-FB-01 — Engine fallback modal**
**GIVEN** Menus subscriber is absent, disabled, or not initialized during Session-Exit Save failure, **WHEN** Save/Load emits `SaveFailedEvent`, **THEN** an engine-level quit-flow fallback surfaces the same two choices: `Retry` and `Quit Without Saving`.
*Integration | engine-programmer + qa-tester | T1-blocking*

**H-MS-FB-02 — Engine fallback Retry**
**GIVEN** the engine-level fallback receives `Retry`, **WHEN** the action is selected, **THEN** it re-queues Session-Exit Save through Save/Load without exiting.
*Integration | engine-programmer + qa-tester | T1-blocking*

**H-MS-FB-03 — Engine fallback Quit Without Saving**
**GIVEN** the engine-level fallback receives `Quit Without Saving`, **WHEN** the action is selected, **THEN** it exits without another save attempt.
*Integration | engine-programmer + qa-tester | T1-blocking*

**H-MS-FB-04 — Menus absent cannot create playable partial load**
**GIVEN** Menus is absent for non-exit `SaveFailedEvent` or `LoadRejected(failureClass)`, **WHEN** the event fires, **THEN** the event is logged and Save/Load/World Structure retain their own consequences; Menus absence does not create a playable partial-load path.
*Integration | engine-programmer + qa-tester | T1-blocking*

**H-MS-FB-05 — Transition failure modal coalescing**
**GIVEN** both `SaveFailedEvent` and `ZoneErrorEvent` arrive for the same transition failure, **WHEN** Menus presents the failure, **THEN** it shows one terminal `ZoneError` modal and preserves save-failure details in logs/test hooks.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-MS-FB-06 — Duplicate failure events do not stack modals**
**GIVEN** duplicate failure events arrive while a blocking failure modal is open, **WHEN** Menus processes them, **THEN** it does not stack duplicate modals; it preserves or updates the existing modal only when severity/class changes.
*Integration | ui-programmer + qa-tester | T1-blocking*

**H-MS-FB-07 — Load rejection before root UI**
**GIVEN** `LoadRejected(failureClass)` fires before root UI is initialized, **WHEN** the boot shell receives it, **THEN** the payload is buffered and displayed once UI exists; gameplay remains disabled throughout.
*Integration | ui-programmer + qa-tester | T1-blocking*

### Summary Table

| ID | Covers | Test Type | Owner | T1-Blocking |
|---|---|---|---|---|
| H-MS-SL-01 | Save-in-progress indicator | Integration | ui-programmer, qa-tester | Yes |
| H-MS-SL-02 | Save confirmation remains quiet | Integration | ui-programmer, qa-tester | Yes |
| H-MS-SL-03 | Pause-menu save timestamp placement (conditional) | Integration | ui-programmer, qa-tester | **advisory** |
| H-MS-SL-04 | Manual Save failure modal | Integration | ui-programmer, qa-tester | Yes |
| H-MS-SL-05 | LoadRejected preserves failure class | Integration | ui-programmer, qa-tester | Yes |
| H-MS-SL-06 | Session-Exit Save failure choices | Integration | ui-programmer, qa-tester | Yes |
| H-MS-SL-07 | Retry requeues exit save | Integration | gameplay-programmer, qa-tester | Yes |
| H-MS-SL-08 | Quit Without Saving exits | Integration | gameplay-programmer, qa-tester | Yes |
| H-MS-SL-09 | Settings do not dirty gameplay save | Integration | gameplay-programmer, qa-tester | Yes |
| H-MS-SL-10 | New Game overwrite confirmation | Integration | ui-programmer, qa-tester | Yes |
| H-MS-SL-11 | Continue with no record | Integration | ui-programmer, qa-tester | Yes |
| H-MS-WS-01 | Routine ZoneLoading invisible | Integration | ui-programmer, qa-tester | Yes |
| H-MS-WS-02 | Zone overrun is non-blocking | Integration | ui-programmer, qa-tester | Yes |
| H-MS-WS-03 | ZoneActive hides overrun | Integration | ui-programmer, qa-tester | Yes |
| H-MS-WS-04 | ZoneError terminal modal | Integration | ui-programmer, qa-tester | Yes |
| H-MS-WS-05 | ZoneError offers no recovery shortcut | Integration | qa-tester | Yes |
| H-MS-WS-06 | SessionResumeEvent silent | Integration | ui-programmer, qa-tester | Yes |
| H-MS-IN-01 | UI Toolkit substrate | Editor-validation + Dev-build smoke | unity-ui-specialist, qa-tester | Yes |
| H-MS-IN-02 | Input System only | Editor-validation | unity-specialist | Yes |
| H-MS-IN-03 | Keyboard/mouse complete | Integration | qa-tester | Yes |
| H-MS-IN-04 | T1 actions keybindable | Integration | qa-tester | Yes |
| H-MS-IN-05 | Rebind capture | Integration | ui-programmer, qa-tester | Yes |
| H-MS-IN-06 | Binding conflicts are explicit | Integration | ui-programmer, qa-tester | Yes |
| H-MS-IN-07 | Text focus suppresses gameplay hotkeys | Integration | ui-programmer, qa-tester | Yes |
| H-MS-IN-08 | Modal focus trap | Integration | qa-tester | Yes |
| H-MS-IN-09 | Settings clamp before applying | Unit + Integration | gameplay-programmer, qa-tester | Yes |
| H-MS-ART-01 | Layer 1 practical UI compliance | Dev-build smoke | ui-programmer, art-lead | Yes |
| H-MS-ART-02 | Forbidden UI styling absent | Editor-validation + Dev-build smoke | unity-ui-specialist, qa-tester | Yes |
| H-MS-ART-03 | No convenience UI | Dev-build smoke | game-designer, qa-tester | Yes |
| H-MS-ART-04 | Unity 6.3 UI transform caveat | Editor-validation | unity-ui-specialist | Yes |
| H-MS-FB-01 | Engine fallback modal | Integration | engine-programmer, qa-tester | Yes |
| H-MS-FB-02 | Engine fallback Retry | Integration | engine-programmer, qa-tester | Yes |
| H-MS-FB-03 | Engine fallback Quit Without Saving | Integration | engine-programmer, qa-tester | Yes |
| H-MS-FB-04 | Menus absent cannot create playable partial load | Integration | engine-programmer, qa-tester | Yes |
| H-MS-FB-05 | Transition failure modal coalescing | Integration | ui-programmer, qa-tester | Yes |
| H-MS-FB-06 | Duplicate failure events do not stack modals | Integration | ui-programmer, qa-tester | Yes |
| H-MS-FB-07 | Load rejection before root UI | Integration | ui-programmer, qa-tester | Yes |

**Total: 37 criteria. 36 T1-blocking, 1 advisory-at-T1 (H-MS-SL-03 — vacuous when timestamp is not surfaced; promotes to T1-blocking when implementation surfaces a timestamp).**

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|------------|
| **ADR-tba — Engine-level quit-flow fallback seam.** What exact Unity/application lifecycle hook owns the Session-Exit Save failure fallback when Menus is absent, and how does it requeue `Retry` without depending on menu UI? | `engine-programmer` + `ui-programmer` | Before T1 quit-flow implementation | Open — T1-blocking |
| **ADR-tba — UI Toolkit vs. UGUI override policy.** This GDD assumes UI Toolkit for runtime menus; document the override threshold if any specific screen must use UGUI. | `unity-ui-specialist` + `ui-programmer` | Before T1 menu implementation | Open — T1-blocking unless UI Toolkit remains uncontested |
| **ADR-tba — Settings persistence backend.** Where are non-gameplay settings stored, how are corrupt settings recovered, and how is this kept separate from gameplay Save/Load integrity? | `gameplay-programmer` + `engine-programmer` | Before Settings implementation | Open — T1-blocking |
| **ADR-tba — Input binding conflict policy detail.** `rebind_duplicate_policy` is pinned to `BlockAndAsk`; decide exact UX copy and whether ReplaceWithConfirm is allowed later. | `ux-designer` + `ui-programmer` | Before Input Settings implementation | Open |
| **ADR-tba — Menu pause semantics in T1.** Does opening Pause Menu stop local simulation time in offline T1, or only capture input? This GDD intentionally does not claim ownership of world simulation time. | `game-designer` + `engine-programmer` | Before Pause Menu implementation | Open — T1-blocking |
| **Failure-copy style guide.** Define player-safe copy for each `LoadRejected(failureClass)` and `ZoneErrorEvent(reason)` without hiding the exact class from logs/tests. | `writer` + `ux-designer` + `qa-tester` | Before first failure modal implementation | Open |
| **Accessibility baseline before full Accessibility GDD.** Confirm MVP minimums for UI scale, contrast/readability, reduced motion, subtitle/caption placeholders, and keyboard-only navigation. | `accessibility-specialist` + `ui-programmer` | Before menu art/UX signoff | Open |
| **Audio UI tone library.** Decide whether Menus ships with UI ticks/warning tones at T1 or remains silent until Audio System implementation. | `audio-director` + `sound-designer` | Before T1 menu polish | Open |
| **Registry candidates.** `ui_scale_min`, `ui_scale_max`, and audio bus defaults may need registry entries if referenced by Accessibility, Audio, HUD, or future UX specs. | `systems-designer` | Phase 5 registry update | Open |
