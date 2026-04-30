# T1 Combat Fixture Data

This directory holds production Combat Core fixture data for Sprint 1.

`t1-combat-fixtures.json` materializes the approved Combat Core prototype fixture package for `T1-COMBAT-01` and the targeting/pull/leash tuning required by `T1-COMBAT-02`. It is implementation and test data, not final Class Design or Encounter Design balance.

Source contracts:

- `design/gdd/combat-core.md` actor, spell, tactical instant, and encounter fixture tables.
- `production/sprints/sprint-1.md` story `T1-COMBAT-01`.
- ADR-0003 `CombatProgressionBaselineSnapshot` hydration boundary.

Rules:

- Gameplay tunables used by production Combat Core must resolve from fixture/config data.
- Targeting uses `target_acquire_radius_meters = 35` and `combat_query_buffer_size = 64`; query results must be sorted by distance, `combat_sort_key`, then authored collider index before use.
- T1 LoS blockers are exactly `WorldSolid`, `ClosedDoor`, and `LargeProp`; `CombatActor`, `TriggerOnly`, `InteractableSoft`, and VFX layers must not block Combat Core LoS.
- Body pulls initialize `proximity_threat_initial = 25`, use pivot/stance shift as the visible signal, and must not create alert markers, barks, UI warnings, warning stingers, or scripted encounter triggers.
- Default social assist uses `social_assist_pulse_seconds = 2.0`, `social_assist_radius_meters = 12`, and `assist_threat_initial = 25`; each eligible actor may join only once per pull episode.
- Leash/path hooks are data-driven: `leash_distance_meters = 35`, `path_failure_grace_seconds = 1.0`, `path_pending_grace_seconds = 1.0`, `path_status_sample_seconds = 0.25`, `leash_threat_memory_seconds = 30`, and `leash_reaggro_distance_meters = 20`.
- `CombatActorState.combat_actor_id` is transient and must not appear as persisted identity.
- T1 data must not add FishNet, networking, server authority, PvP, live LLM, companions, Warrior, or Enchanter rows.
- The current haunt band is `PrototypeHauntBand_T1 = 1-10`; Level / Encounter Design must remap fixture ids when the final haunt band locks.
