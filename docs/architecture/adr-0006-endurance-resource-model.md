# ADR-0006: Endurance Resource Model

## Status

Accepted

## Date

2026-05-06

## Context

The T1 combat slice review produced Brian's Yellow verdict after the Combat Core
architecture held but the feel evidence surfaced real T1 correction work. The
quantitative evidence showed `H-CCOM-FEEL-03` failed-as-measured with only
`5/10` dangerous two-trash overpull outcomes against the `>=8/10` target, while
Brian's qualitative finding identified a harder resource-model problem:
physical instants such as Bash should not cost mana. Evidence:
`production/qa/combat/feel-review-T1-slice.md:20`,
`production/qa/combat/feel-review-T1-slice.md:54`, and
`production/qa/combat/feel-review-T1-slice.md:80`.

The slice review states that Bash and future physical instants should consume a
separate Endurance/Stamina resource, remain gated by cooldown/global recovery,
and not apply to Cleric magical/holy abilities. It names `Smite of Authority`
and `Defensive Prayer` as mana-based carveouts. It also says Endurance is a real
T1 combat resource that belongs in Combat Core state, Layer 1 HUD signaling, and
save/load persistence. Evidence:
`production/qa/combat/feel-review-T1-slice.md:54` through
`production/qa/combat/feel-review-T1-slice.md:58`.

Combat Core's current D012-era tactical instant contract describes tactical
instants as `cast_time_seconds = 0` profiles with authored mana-cost fields and
requires `H-CCOM-INST-01` to spend mana through Rule 13. That was correct for
the approved D012 combat-feel baseline, but the slice review now requires an
Endurance contract before implementation can safely split physical and
magical/holy instant resource paths. Evidence: `design/gdd/combat-core.md:148`
through `design/gdd/combat-core.md:152` and `design/gdd/combat-core.md:746`
through `design/gdd/combat-core.md:747`.

This ADR is contract-only. It does not implement Endurance, change fixture data,
amend the Combat Core GDD, add HUD styling, or tune FEEL-01/FEEL-03.

## Decision

Combat Core will add a quiet Endurance resource for T1 physical instant pacing.
Endurance is a real combat resource, but it is not a new action-combat rotation
system.

The T1 resource split is:

- Physical instants, starting with Bash, consume Endurance rather than mana.
- Future Warrior-style physical abilities consume Endurance unless a later ADR
  creates a different class-specific resource contract.
- `Smite of Authority` remains mana-based.
- `Defensive Prayer` remains mana-based.
- Magical, holy, healing, and buff Cleric abilities continue to use mana unless
  a later Class Design or Spell Memorization decision explicitly supersedes
  this carveout.

Endurance implementation stories derive from this contract across these
surfaces:

- Combat Core player actor state: current/max Endurance and validation.
- Combat persistence whitelist: Endurance may be added as explicit combat state.
- Layer 1 HUD projection: Endurance may be exposed only as a quiet practical
  signal.
- Fixture schema/data: physical instant rows require an Endurance cost field
  and must not retain mana costs.
- Instant resolver: resource validation/spend branches by ability resource kind.
- Profiled harness resource tracking: physical pacing can be measured without
  treating Endurance as a mana replacement.

Endurance HUD/save discipline is intentionally quiet. Mana remains the foreground
resource for Cleric attention, med-break pacing, and magical/holy ability cost.
Endurance must support physical pacing without competing with mana/med-break
attention.

Forbidden Endurance patterns:

- Endurance as an action-rotation bar, priority bar, combo meter, or GCD-like
  resource loop.
- Endurance HUD prominence above mana; mana stays the foreground attention
  budget, while Endurance stays peripheral.
- Pulse, combo, animation, or celebratory treatment that implies tactical
  cycling intent.
- Per-ability Endurance callouts in the shipping HUD unless the surface is
  explicitly QA/debug-only.
- Combat-rotation-fast Endurance regeneration; regeneration must support
  physical pacing, not compete with mana recovery or med-break attention.

ADR-0003 is not amended by this decision. ADR-0003 governs
`CombatProgressionBaselineSnapshot` as a progression-to-combat hydration
contract. Combat persistence projection may grow to include Endurance without
changing the progression baseline snapshot contract.

## Consequences

### Positive

- Physical-instant pacing can be tuned without making Bash spend the same mana
  budget as Cleric magical/holy abilities.
- Cleric mana remains meaningful for med breaks, heals, buffs, and magical/holy
  tactical instants.
- Downstream implementation stories have a single contract for actor state,
  persistence, HUD projection, fixture schema, resolver behavior, and harness
  tracking.
- T1-COMBAT-11 can scan for explicit Endurance forbidden patterns instead of
  relying on prose memory.

### Negative

- Combat Core must grow its player resource model beyond health and mana.
- Existing D012-era instant fixture schema and GDD wording will need amendment
  or supersession in later Sprint 1.5 work.
- HUD projection tests must guard an intentionally subtle output, which is
  easier to overbuild than a simple visible bar.
- Profiled harness metrics must distinguish mana pressure from physical
  Endurance pressure.

### Validation State

This ADR remains Proposed until implementation validates it. T1.5-COMBAT-01 is
expected to validate actor state, persistence, and HUD projection. T1.5-COMBAT-02
is expected to validate the physical-instant resource split. If both hold,
ADR-0006 can move from Proposed to Accepted and DECISIONS.md D013 can move from
Proposed to Locked during the relevant closure batch.

## See Also

- `DECISIONS.md` D013 - ADR-0006 Endurance Resource Model.
- `DECISIONS.md` D012 - T1 Combat-Feel Validated; Combat Core Revision Required
  Before /sprint-plan new.
- `production/qa/combat/feel-review-T1-slice.md`.
- `production/sprints/sprint-1-5.md`.
- `production/qa/plans/qa-plan-sprint-1-5-20260506.md`.
- `design/gdd/combat-core.md`.
