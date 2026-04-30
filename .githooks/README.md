# Gravenspire Pre-Commit Hooks

This directory contains tracked, reviewable local Git hooks for Gravenspire. The current hook promotes repeated T1 implementation lessons into a staged-file gate: whitespace/conflict-marker hygiene and the negative T1 scope scan that has been run manually across Sprint 1 Combat Core stories.

## Install

```powershell
git config core.hooksPath .githooks
```

This is per-checkout configuration and is not auto-applied by the repository.

## What It Checks

- Runs `git diff --cached --check` against staged content to catch whitespace errors and conflict markers before commit.
- Scans staged `src/**/*.cs` and `tests/**/*.cs` files for T1-forbidden scope terms. Markdown, YAML, TRX, and other non-C# files are exempt because sprint plans, stories, and evidence files legitimately mention deferred terms in scope notes.

## Bypass Policy

AGENTS.md §13 requires honoring project hooks; do not use `--no-verify` unless the user explicitly instructs it. Any emergency bypass needs a follow-up `[GLOBAL]` lesson entry explaining why the structural gate had to be skipped.

## Tier-Transition Maintenance

The deny list is T1-specific. When DECISIONS.md D003 is superseded by a T2 transition D-entry, update `.githooks/pre-commit` to remove `FishNet`, `networking`, and `server authority` from the deny-list pattern. Use the source-of-truth table in AGENTS.md §4 to decide whether any new tier rule also requires a hook update.
