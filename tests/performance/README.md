# Performance and Profiled Evidence Tests

Use this tree for deterministic harness code that supports profiled combat-feel evidence. Human-readable evidence and JSONL outputs should be written under `production/qa/combat/**` or `production/playtests/combat/**` when the relevant story creates those approved production paths.

Sprint 1 profile scenarios:

- `SoloTrash_EvenCon_T1`: 20 seeded clean-state trials; Cleric wins 90-100 percent and ends below either 80 percent health or 60 percent mana on mean result.
- `NamedSoloBlock_T1`: 10 seeded trials; Cleric loses or must flee at least 8 out of 10.
- `TwoTrash_Overpull_T1`: 10 seeded trials; Cleric loses, flees, or survives below threshold at least 8 out of 10.
- `MedBreakRecovery_T1`: below 35 percent mana to 70 percent mana within 60-120 seconds after combat exit.

Profile JSONL records must include engine version, fixture-set version, build SHA, scenario, completion state, pull counts, combat/downtime seconds, med breaks, tactical instant usage, unsafe pulls, and deaths.
