# Test Fixtures

Use this tree for test-only data fixtures. Production data belongs under `assets/data/**`; these fixture files should either mirror approved production rows for validation or hold deliberately invalid rows for negative tests.

Expected Sprint 1 fixture paths:

```text
tests/fixtures/combat/
tests/fixtures/progression/
tests/fixtures/save/
```

Fixture rules:

- Include fixture-set version metadata.
- Include a source document reference or story id.
- Keep valid fixtures and deliberately invalid fixtures clearly separated.
- Use stable source refs and lifecycle tokens for XP and save-barrier tests.
- Do not use runtime `combat_actor_id` as a persisted identity.
- Do not include network, server, account, PvP, live LLM, companion, Warrior, or Enchanter fixture rows in T1 Combat Core tests.
