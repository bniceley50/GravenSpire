# S5-02: Perf-Budget Framework + Asset-Spec

> **Sprint**: Sprint 5 — First District — Designed & Produced (First-Pass)
> **Sprint Plan**: `production/sprints/sprint-5.md` (Story Ledger, S5-02)
> **Status**: Blocked (depends on S5-01)
> **Layer**: Presentation
> **Type**: Config/Data (perf-budget + asset-spec)
> **Estimate**: 1.0 day
> **Manifest Version**: Unavailable (control-manifest absent project-wide; documented fallback applies)
> **Generated**: 2026-06-07
> **Owner**: technical-director + art-director

## Context

**Authority**: `DECISIONS.md` D021 (perf-budget gate); `.claude/docs/technical-preferences.md`
(Performance Budgets — currently `TO BE CONFIGURED`); art bible §8 (asset standards /
budgets / import / naming / validation).

**Requirement Summary**: This is the first produced-art work the project has ever done —
the asset pipeline has never run and the four performance budgets are unset. Per D021,
budgets must be set before production scales. This story sets a provisional perf-budget
framework against a named target-hardware tier and writes the **asset-spec** for the
S5-01 manifest, so produced assets are authored to a ceiling rather than blindly. The
sequencing rule is recorded: **author one sub-slice → profile on target hardware → lock
measured budgets → then scale.**

**Governing decisions**:

| D-entry | Status | Usage |
|---|---|---|
| D021 | Locked | The perf-budget gate before production scales |
| D001 | Locked | URP only — no BIRP/HDRP |
| D003 | Locked | Tier-1 — no T2 pipeline pull-forward (Addressables deferred) |

**Surfaces consumed**: the S5-01 asset manifest (the assets to budget + spec).

**Engine**: Unity 6.3 LTS, URP. **Engine Risk**: MEDIUM (perf budgets on an unprofiled
project; URP material/import standards — verify §6.x post-cutoff APIs against
`docs/engine-reference/unity/`).

> **Protected write note**: this story edits `.claude/docs/technical-preferences.md` (a
> tech-spec source-of-truth governance file). That edit requires explicit per-file
> approval at execution — flag it before writing.

## Acceptance Criteria

- [ ] **S5-02-01**: A **named target-hardware tier** is set (the budgets reference it) — the minimum-spec the first-pass produced area is budgeted against.
- [ ] **S5-02-02**: The **four `TO BE CONFIGURED` budgets** — target framerate, frame budget, draw calls, memory ceiling — are set in `.claude/docs/technical-preferences.md` as a **provisional framework**, explicitly marked to be confirmed by profiling (not final until S5-03 profiles the sub-slice).
- [ ] **S5-02-03**: The **asset-spec** for the S5-01 manifest is written: per-asset texture budgets (§8.3), polygon budgets (§8.4), naming convention (§8.2), import settings, and validation requirements (§8.8) — concrete enough that S5-03 authors to spec.
- [ ] **S5-02-04**: The **sequencing rule** is recorded: author one sub-slice → profile on target hardware → lock measured budgets → then scale. The budgets are the asset-spec ceiling; over-budget in S5-03 means cut dressing, not framerate.
- [ ] **S5-02-05**: **Deferrals confirmed** (D021 / TD): Addressables (not needed for one area), character-fidelity, and the full faction material library are explicitly OUT.
- [ ] **S5-02-06**: **Tier-1 + URP-only hold** — no T2 pipeline pull-forward; no BIRP/HDRP drift; GI/lighting approach noted for the practical-light rig.

## Implementation Notes

- The budgets are provisional by design — the point is to have a ceiling before authoring, not to pretend an unprofiled project has measured numbers. S5-03 profiles the first sub-slice and the measured values lock the framework.
- Verify any post-6.0 URP material/import API against `docs/engine-reference/unity/` before writing the asset-spec (UI Toolkit/URP are UNVERIFIED beyond ~6.0).

## Out of Scope

- Producing the assets (S5-03). Art direction / material selection (S5-01). Design (S5-00).
- Installing Addressables or any deferred pipeline. Final (post-profile) budget lock — that happens in S5-03.

## QA Test Cases

**Manual check (S5-02-02 budgets set)**
- Setup: read `.claude/docs/technical-preferences.md` Performance Budgets after the edit.
- Verify: all four `TO BE CONFIGURED` fields carry provisional values + target-hardware tier + the "confirm by profiling" marker.
- Pass: no `TO BE CONFIGURED` remains; values are provisional-and-labelled, not silently final.

**Smoke check (S5-02-03 asset-spec)**
- Setup: read the asset-spec against the S5-01 manifest.
- Verify: every manifest asset has a texture/poly budget, naming, import, and validation entry per §8.
- Pass: an implementer could author each asset to spec without guessing.

## Test Evidence

**Required evidence**: `production/qa/evidence/s5-02-perf-budget-and-asset-spec.md`
(the asset-spec + the sequencing rule + deferral list) **plus** the
`.claude/docs/technical-preferences.md` Performance-Budgets edit (cited file:line).

**Evidence status**: Not started

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| `S5-01` | The asset manifest to budget + spec | Done |

## Blockers

Blocked on S5-01 (the manifest). The `technical-preferences.md` budget edit needs
explicit per-file approval at execution (protected governance file).
