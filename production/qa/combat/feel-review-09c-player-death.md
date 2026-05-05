# T1-COMBAT-09c Player Death Feel Review

**Story:** `T1-COMBAT-09c`
**Date:** 2026-04-30
**Status:** Implementation perspective + human playtest pending.

## Implementation Perspective

The death moment now has a narrow Combat-owned state transition: lethal player damage produces `combat_life_state == Dead`, clamps current health to zero, clears transient target/threat/cast interaction state, and emits one `PlayerDeathEvent` with stable local identity, zone, position, killer source ref, cause type, and deterministic context id.

From the player's perspective, the code now has enough state for the game to stop accepting combat actions and preserve the exact death handoff payload. It does not yet provide a full playable death sequence. The only event that fires from this story is the narrow `PlayerDeathEvent`; NPC kill credit remains a separate frozen sibling event.

The persistence seam is active only as a typed read projection. It exposes current health, current mana, combat life state, and optional pending death handoff payload. Threat, target selection, cast progress, cooldown state, regen state, formula tuning, penalty data, item outcomes, narrative text, and visual treatment remain internal or absent.

## What Is Intentionally Absent

Death & Corpse Recovery is stub-only in this story. There is no respawn flow, no corpse-run flow, no resurrection mechanic, no death narrative, no penalty calculation, no item-drop behavior, no recovery interaction, and no "you died" UI treatment.

This story reserves the integration point. It makes the death moment auditable and save-safe, but it does not make death playable end-to-end.

## Human Playtest Prompts

**What the death moment felt like (one sentence):**


**What the player read from the moment without any UI text:**


**What's missing that you noticed:**


**What's missing that's intentional but worth flagging for later (`tasks/gameplay-debt.md` candidates):**


**Death-cause clarity (did the player understand who/what killed them):**


**Pre-fix-required (Loop-Threat) vs. defer (Open):**


<!-- HUMAN PLAYTEST PENDING -->
