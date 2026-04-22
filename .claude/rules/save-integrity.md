---
paths:
  - "src/core/save/**"
  - "Assets/Scripts/Core/Save/**"
  - "Assets/Scripts/Persistence/**"
---

# Save Integrity Rules

**Active tier:** T1+. Save integrity matters from day one because:

- Playtesters share save files to reproduce bugs — a tampered save looks
  like a real bug report
- Steam community save-editor posts are inevitable
- A locally-tampered save that later syncs to server (T3+) becomes a
  server-side integrity incident

---

## Tamper Resistance

- Save files **must carry an HMAC signature** computed with a per-install
  key.
- Load path **verifies** signature before deserializing gameplay state —
  mismatch rejects the save with a **loud error**, not a silent fall-through
  to default state.
- The HMAC key is **derived**, not stored in plaintext next to the save
  file (derivation strategy is a T1 design decision; document in the save
  system's GDD when authored).

## Versioning

- Save format carries a **version stamp as the first field**, before any
  gameplay state.
- Loader checks version **before** attempting to deserialize subsequent
  fields — mismatch is fail-loud.
- **Newer loader + older save:** attempt migration path; if unavailable,
  fail loud (never partial-load).
- **Older loader + newer save:** reject with version-mismatch error;
  never partial-load.

## Migration

- Every version bump ships with a migration test: a **real prior-version
  save** → current loader → expected current-version state.
- Migrations are **forward-only**; never edit prior versions' deserialization
  paths.
- Store canonical test saves in `tests/fixtures/saves/v[N]/` so migrations
  can be re-tested when new versions land.

## What Saves

- Gameplay state: inventory, quest progress, reputation, unlocks, seeds
- Player-authored strings (character name): bounded length, sanitized on
  load (strip control chars, cap length)
- **NOT:** engine internals, derived/cached values, non-reproducible
  runtime state (network connections, file handles, temp paths)

## See Also

- `RED_TEAM.md` §6, §9 — save-integrity and versioning rubrics
- `SECURITY.md` threat #1
- `AGENTS.md` §11 (PII handling for player-authored strings)
