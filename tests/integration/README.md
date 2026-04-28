# Integration Tests

Use this tree for tests crossing systems, Unity runtime behavior, event buses, hydration order, and save/load boundaries.

Expected Sprint 1 paths:

```text
tests/integration/gameplay/combat/
tests/integration/gameplay/progression/
tests/integration/gameplay/npc/
tests/integration/core/save/
```

Sprint 1 integration coverage should include:

- `T1-COMBAT-01`: valid and invalid `CombatProgressionBaselineSnapshot` hydration.
- `T1-COMBAT-02`: HauntZone/CityHub gates, targeting, pull, social assist, leash, and cleanup.
- `T1-COMBAT-03`: Attack toggle never auto-enables from target, tab cycle, body pull, spell pull, or threat initialization.
- `T1-COMBAT-04`: fixed-tick melee, pause resume, out-of-range skip, and same-tick death priority.
- `T1-COMBAT-05/06`: cast lifecycle, interrupt/cancel, recovery, tactical instant profile resolution.
- `T1-COMBAT-07`: sit/med, combat exit timer, and seated threat behavior.
- `T1-COMBAT-08`: HUD-safe Attack ON/OFF signal and accessor without final presentation ownership.
- `T1-COMBAT-09a/b/c`: death events, unchanged kill credit, XP award/dedupe/rejection, grouped save barriers, and narrow player death payload.

Use test doubles for systems that are not implemented yet, but keep ownership boundaries honest: Combat must not mutate Character Progression, NPC lifecycle, or Save/Load state directly.
