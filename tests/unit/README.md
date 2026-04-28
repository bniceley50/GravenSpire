# Unit Tests

Use this tree for isolated NUnit tests that do not need scenes, physics, file I/O, network access, live LLM calls, or external services.

Expected Sprint 1 paths:

```text
tests/unit/gameplay/combat/
tests/unit/gameplay/progression/
tests/unit/core/save/
```

Sprint 1 unit coverage should include:

- Combat actor schema.
- Combat formula examples and clamps.
- Fixture validation for combat and progression data.
- Tactical instant profile validation.
- Kill-credit DTO schema exactness.
- Progression XP award, duplicate, missing lookup, and stale lifecycle cases that do not need runtime scenes.
- Save/Load payload whitelist and first-save materializer result mapping.

Boundary value tests may include exact numeric constants when the number itself is the requirement. Production combat logic must still load tunable gameplay values from approved data/config.
