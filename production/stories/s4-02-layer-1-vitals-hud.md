# S4-02: Layer 1 Vitals HUD

> **Sprint**: Sprint 4 — EQ-Readable Presentation Slice
> **Sprint Plan**: `production/sprints/sprint-4.md` (Story Ledger, S4-02)
> **Status**: Blocked (depends on S4-00)
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: 1.0 day
> **Manifest Version**: Unavailable (control-manifest absent project-wide; documented fallback applies)
> **Generated**: 2026-06-07
> **Owner**: Codex

## Context

**Authority**: `DECISIONS.md` D020; revised art bible §7.1 (recessive but legible),
§7.1.1 (Layer 1 State-Report elements), §4.4 (UI palette).

**Requirement Summary**: Implement the player vitals readout (health / mana /
endurance / hate) as recessive-but-legible Layer 1 HUD in Gravenspire's Iron Seam
architectural vocabulary, at the legibility floor set by S4-00. This is the first of
the three HUD implementation stories and the base the target frame (S4-03) and cast
bar / interaction prompt (S4-04) build alongside. It directly serves §7.12 criterion 1
(combat-state read).

**Governing decisions**:

| D-entry | Status | Usage |
|---|---|---|
| D020 | Locked | EQ-readability pivot; the vitals legibility this executes |
| D016 | Locked | Greybox; legibility floor, not maximum polish |

**Art-bible authority**: §7.1 (Layer 1 character: recessive but legible — quiet is the
aesthetic, unreadable is a bug); §7.1.1; §7.9 (animation feel — snap-to-value bars,
the one permitted low-amplitude death-approach pulse); §7.11 (forbidden list);
§4.4 (UI palette: health Render Umber, mana Pewter Rain, hate Academic blue-black ->
Rust Iron).

**Surfaces consumed**: the S4-00 resolved numbers (bar height, panel-fill opacity
floor). This story does NOT set those numbers — it builds to them.

**Engine**: Unity 6.3 LTS. **Engine Risk**: MEDIUM (UI Toolkit / UGUI HUD rendering;
verify UI Toolkit APIs against `docs/engine-reference/unity/` — §6.2 deprecations
noted in VERSION.md).

## Acceptance Criteria

- [ ] **S4-02-01**: Health / mana / endurance / hate bars are implemented in the Iron Seam Layer 1 vocabulary (compressed architectural forms, §4.4 palette per channel) at the **S4-00 legibility floor** (bar height, panel-fill opacity).
- [ ] **S4-02-02**: The vitals HUD is **recessive but legible** (§7.1): readable under combat stress at a peripheral glance, without pulling the player's aesthetic attention. Satisfies §7.12 criterion 1 (a player 10s into a pull can read their approximate health without studying the screen).
- [ ] **S4-02-03**: No forbidden Layer 1 treatments (§7.11): no glow / bloom / emission, no gradient fills, no rounded corners, no drop shadows, no true black / true white, no red or green as signaling colors.
- [ ] **S4-02-04**: Bars **snap to value** (§7.9); the only permitted animation is the low-amplitude death-approach pulse (<20% health, in-combat, 0.7Hz) and the linear med-break mana fill. No tween-for-feel.
- [ ] **S4-02-05**: HUD coherence (§7.12 criterion 5): the vitals share the city's material vocabulary (Iron Seam, arch terminus, chamfered icon frames) — it does not look imported from another game.
- [ ] **S4-02-06**: The HUD is **State Report only** (§7.11 boundary): it reports the player's own vitals. It does not route, advertise, or perform world content.

## Implementation Notes

- Build strictly to the S4-00 numbers; if S4-00 left a number as an open question, escalate rather than guess (the evidence-honesty discipline — a guessed HUD number is the S3-06 failure repeating).
- Verify Unity 6.3 UI Toolkit / UGUI APIs against `docs/engine-reference/unity/` (post-6.0 UI Toolkit has documented deprecations — e.g. `VisualElement.transform`).
- HUD lives on an isolated overlay (per S4-01's debug-HUD isolation); it is exempt from the world-desat pass (§7.9 corpse-run note).

## Out of Scope

- Target frame + con indicator (S4-03).
- Cast bar + interaction prompt (S4-04).
- Group/party frames (not in the T1 slice).
- The HUD threshold numbers themselves (S4-00 owns those).

## QA Test Cases

**Manual check (S4-02-02 combat-state read)**
- Setup: enter a pull; take damage to ~50% then <20% health.
- Verify: health is readable at a peripheral glance throughout; the <20% death-approach pulse is felt-not-distracting.
- Pass: §7.12 criterion 1 satisfied; pulse at 0.7Hz, low amplitude.

**Manual check (S4-02-03 forbidden treatments)**
- Setup: inspect the rendered HUD + the implementation.
- Verify: no glow/gradient/rounded/shadow/true-black/true-white/red/green.
- Pass: zero §7.11 violations.

## Test Evidence

**Required evidence**: `production/qa/evidence/s4-02-vitals-hud-evidence.md`
(screenshots at full/low health; confirmation of §7.11 compliance and the S4-00 floor
values used).

**Evidence status**: Not started

## Dependencies

| Depends On | Reason | Required Status |
|---|---|---|
| `S4-00` | The bar height + panel-fill opacity floor this HUD builds to | Done |

## Blockers

Blocked on S4-00 (the deferred HUD numbers). Cannot start with real values until
S4-00 resolves them. D020 + the revised bible are otherwise the full authority.
