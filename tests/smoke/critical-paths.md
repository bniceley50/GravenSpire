# Smoke Test: Critical Paths

**Purpose**: Run these checks before QA hand-off, story completion, or sprint closeout.
**Run via**: `/smoke-check` once implementation exists.
**Expected Duration**: 10-15 minutes for setup-only checks; longer once combat profile scenarios exist.
**Update Rule**: Add entries when a Sprint 1 story creates a new release-critical path.

## Setup Gate

1. Unity editor exists at `C:\Program Files\Unity\Hub\Editor\6000.3.14f1\Editor\Unity.exe`.
2. EditMode command is documented in `tests/README.md`.
3. PlayMode command is documented in `tests/README.md`.
4. Story evidence paths are documented in `tests/README.md`.
5. No `.github/workflows/tests.yml` exists during T1 unless a later tier or CI decision approves it.

## Core Stability

6. Game launches without crash once a Unity project shell exists.
7. Main menu or temporary dev entry scene loads once implemented.
8. New local session can start once Save/Load and World Structure entry points exist.
9. Keyboard/mouse input responds without freezing once a playable path exists.

## T1 Combat Core

10. `T1-COMBAT-01` can build a Cleric combat actor from valid fixture data.
11. `Cleric_Mid_T1` resolves as level 5, 140 HP, and 180 mana.
12. Targeting, body pull, spell pull, and threat initialization do not turn Attack on.
13. Attack turns off on target death, successful sit/med, combat exit, death, and zone transition.
14. `PlayerKillCreditEvent` contains exactly `defeated_source_ref`, `zoneId`, `faction_id`, and `kill_weight_seed`.
15. Same-frame kill credit plus save invokes both grouped save barriers before serialization.

## Profiled Evidence

16. `SoloTrash_EvenCon_T1` writes prototype-compatible JSONL plus production fields.
17. `NamedSoloBlock_T1` writes prototype-compatible JSONL plus production fields.
18. `TwoTrash_Overpull_T1` writes prototype-compatible JSONL plus production fields.
19. Med-break recovery evidence records elapsed seconds, regen ticks, and Attack forced off.

## Scope Negative Pass

20. Static scan finds no FishNet, networking placeholder, PvP, live LLM, companion, Warrior, Enchanter, account identity, server authority, or server combat state in T1 Combat Core code.
