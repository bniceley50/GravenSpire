# Test Evidence

Use this tree for local logs, copied terminal summaries, screenshots, and manual notes that support story completion.

Recommended layout:

```text
tests/evidence/
  test-results/
    editmode-results.xml
    editmode.log
    playmode-results.xml
    playmode.log
  T1-COMBAT-01/
    20260428-1200-editmode-57ef8ed.log
    t1-combat-01-evidence.md
```

Every evidence note should include:

- Story id.
- Test command used.
- Git SHA.
- Engine version.
- Fixture-set version when fixture data is involved.
- Acceptance criteria ids covered.
- Pass/fail summary.
- File:line references for changed production code and changed tests.
- Negative-scope scan result when applicable.
