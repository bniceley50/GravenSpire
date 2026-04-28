# T1 Combat Fixture Data

This directory holds production Combat Core fixture data for Sprint 1.

`t1-combat-fixtures.json` materializes the approved Combat Core prototype fixture package for `T1-COMBAT-01`. It is implementation and test data, not final Class Design or Encounter Design balance.

Source contracts:

- `design/gdd/combat-core.md` actor, spell, tactical instant, and encounter fixture tables.
- `production/sprints/sprint-1.md` story `T1-COMBAT-01`.
- ADR-0003 `CombatProgressionBaselineSnapshot` hydration boundary.

Rules:

- Gameplay tunables used by production Combat Core must resolve from fixture/config data.
- `CombatActorState.combat_actor_id` is transient and must not appear as persisted identity.
- T1 data must not add FishNet, networking, server authority, PvP, live LLM, companions, Warrior, or Enchanter rows.
- The current haunt band is `PrototypeHauntBand_T1 = 1-10`; Level / Encounter Design must remap fixture ids when the final haunt band locks.
