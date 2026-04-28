# Agents Smoke Report - 2026-04-28

**Scope:** 48 remaining `.claude/agents/*.md` files, excluding `qa-lead.md`
because it was smoke-tested before commit `2e47f70`.

**Commit under test:** `2e47f70` on `main`; verified by
`git rev-parse --short HEAD` and `git status --short --branch`.

**Prior qa-lead report artifact:** absent at
`production/qa/qa-lead-smoke-test-result.md`; verified by `Test-Path`.

---

## Read Baseline

Session ritual and scope anchors were read from live disk.

- `AGENTS.md` defines the session-start ritual, EDIT_OK protocol, evidence rule,
  and T1 tier policy at `AGENTS.md:41`, `AGENTS.md:57`, `AGENTS.md:72`, and
  `AGENTS.md:119`.
- `.claude/docs/coordination-rules.md` defines model-tier expectations at
  `.claude/docs/coordination-rules.md:15`.
- `design/gdd/game-concept.md` reinforces T1 as single-player offline at
  `design/gdd/game-concept.md:323` and excludes networking, companions, and live
  LLM surfaces from MVP at `design/gdd/game-concept.md:327`.
- `tasks/lessons.md` includes the smoke-discipline lesson at
  `tasks/lessons.md:58`.

**Session-state note:** `production/session-state/active.md` is stale for this
checkpoint. It still points to `/sprint-plan new` at
`production/session-state/active.md:64`, while Sprint 1 now requires
`/qa-plan sprint`, then `/test-setup`, per `production/sprints/sprint-1.md:37`
and `production/sprint-status.yaml:13`.

---

## Methodology

### Tier 1 - Structural Smoke

Read-only PowerShell parser over all 48 remaining agent files:

- YAML frontmatter presence and required fields.
- Required frontmatter fields: `name`, `description`, `tools`, `model`.
- Expected optional frontmatter fields: `maxTurns`, `memory`, `skills`,
  `disallowedTools`.
- Model values restricted to `opus`, `sonnet`, or `haiku`.
- Skill references checked against `.claude/skills/`.
- Storage-path drift check for the retired
  `production/session-state/lessons.md` path.
- Bash policy check for review/design/coordination agents.
- Code fence balance.
- Behavioral-contract section presence.

Expected body sections checked:

- Mission
- Operating Principles
- Scope
- Non-Goals
- Instruction Priority
- Decision-Making Process or workflow analog
- Tool-Use Policy
- Bash Use Policy where Bash is granted
- Self-Learning Protocol
- Self-Healing Protocol
- Memory Policy
- Feedback Policy
- Safety Guardrails or Safety and Guardrails
- Output Standards
- Reflection Checklist
- Evaluation Checklist
- Example Workflows
- Delegation Map or coordination map
- Final Behavioral Rule

### Tier 2 - Workload Smoke

Codex cannot invoke Claude's named `.claude/agents/*` Task harness directly.
Three Codex subagents were spawned and required to read and follow their
corresponding agent spec:

- `producer`
- `gameplay-programmer`
- `systems-designer`

This is a valid workload simulation but not a perfect harness-level proof.

---

## Tier 1 Structural Summary

**Verdict:** NEEDS_FIX

| Result | Count |
| --- | ---: |
| PASS | 42 |
| PASS_WITH_NOTES | 4 |
| NEEDS_FIX | 2 |
| BLOCKED | 0 |

Frontmatter and storage results:

| Check | Result |
| --- | ---: |
| Required fields missing | 0 |
| Invalid model values | 0 |
| Missing skill references | 0 |
| Old `production/session-state/lessons.md` references | 0 |
| `tasks/lessons.md` references | 47 |
| Bash granted | 31 |
| `disallowedTools: Bash` | 17 |
| Agents without explicit `skills:` | 43 |
| Agents without explicit `disallowedTools:` | 31 |

The missing explicit `skills:` / `disallowedTools:` fields are not blockers
because the prompt treats them as optional, but adding `skills: []` and
`disallowedTools: []` would make future audits cleaner.

---

## Tier 1 Per-Agent Verdicts

| Agent | Verdict | Notes |
| --- | --- | --- |
| accessibility-specialist | PASS |  |
| ai-programmer | NEEDS_FIX | Truncated behavioral contract; unbalanced code fence opens at `.claude/agents/ai-programmer.md:586` and the file ends without closing it. Missing governance tail after `.claude/agents/ai-programmer.md:525`. |
| analytics-engineer | PASS |  |
| art-director | NEEDS_FIX | Truncated behavioral contract; unbalanced code fence opens at `.claude/agents/art-director.md:159` and the file ends at line 160. Missing most governance sections after `.claude/agents/art-director.md:115`. |
| audio-director | PASS |  |
| community-manager | PASS |  |
| creative-director | PASS |  |
| devops-engineer | PASS |  |
| economy-designer | PASS |  |
| engine-programmer | PASS_WITH_NOTES | Missing or renamed `Instruction Priority`; otherwise full governance tail present, including `.claude/agents/engine-programmer.md:1011` and `.claude/agents/engine-programmer.md:2183`. |
| game-designer | PASS | Uses `Safety and Guardrails` analog at `.claude/agents/game-designer.md:1024`. |
| gameplay-programmer | PASS_WITH_NOTES | Missing or renamed `Instruction Priority`; full tail otherwise present through `.claude/agents/gameplay-programmer.md:1945`. |
| godot-csharp-specialist | PASS |  |
| godot-gdextension-specialist | PASS |  |
| godot-gdscript-specialist | PASS |  |
| godot-shader-specialist | PASS |  |
| godot-specialist | PASS |  |
| lead-programmer | PASS |  |
| level-designer | PASS |  |
| live-ops-designer | PASS |  |
| localization-lead | PASS_WITH_NOTES | Bash remains a policy choice for string extraction tooling; has translation-memory policy but no generic `Memory Policy` heading. |
| narrative-director | PASS |  |
| network-programmer | PASS |  |
| performance-analyst | PASS |  |
| producer | PASS |  |
| prototyper | PASS_WITH_NOTES | Missing or renamed `Memory Policy`; self-learning/self-healing tail is present. |
| qa-tester | PASS |  |
| release-manager | PASS |  |
| security-engineer | PASS |  |
| sound-designer | PASS |  |
| systems-designer | PASS |  |
| technical-artist | PASS |  |
| technical-director | PASS |  |
| tools-programmer | PASS |  |
| ue-blueprint-specialist | PASS |  |
| ue-gas-specialist | PASS |  |
| ue-replication-specialist | PASS |  |
| ue-umg-specialist | PASS |  |
| ui-programmer | PASS |  |
| unity-addressables-specialist | PASS |  |
| unity-dots-specialist | PASS |  |
| unity-shader-specialist | PASS |  |
| unity-specialist | PASS |  |
| unity-ui-specialist | PASS |  |
| unreal-specialist | PASS |  |
| ux-designer | PASS |  |
| world-builder | PASS |  |
| writer | PASS |  |

---

## Tier 2 Workload Smokes

| Agent | Workload Verdict | Result |
| --- | --- | --- |
| producer | PASS_WITH_NOTES | Scope-respecting, evidence-cited sprint readiness. Correctly found no story should start before `/qa-plan sprint` and `/test-setup`, citing `production/sprints/sprint-1.md:35` and `production/sprint-status.yaml:13`. Note: producer disallows Bash; Codex subagent used shell-backed read inspection. |
| gameplay-programmer | PASS_WITH_NOTES | Scope-respecting, evidence-cited T1-COMBAT-01 readiness. Correctly identified fixture hydration, `CombatActorState`, ADR-0003 snapshot, and test scaffold needs from `production/sprints/sprint-1.md:72`, `design/gdd/combat-core.md:630`, and `design/gdd/character-progression.md:853`. |
| systems-designer | PASS_WITH_NOTES | Scope-respecting integration audit. Found no blocking Combat Core / Character Progression / ADR-0003 contract inconsistency. Non-blocking metadata tension: Character Progression treats ADR-0003 as authoritative at `design/gdd/character-progression.md:22`, while ADR-0003/D009 are still `Proposed` at `docs/architecture/adr-0003-progression-baseline-snapshot-contract.md:3` and `DECISIONS.md:222`. |

---

## Workload Findings

### Producer

The producer workload correctly reported that Sprint 1 is not ready for
`/dev-story` execution yet. The sprint plan requires:

1. `/qa-plan sprint`
2. `/test-setup`
3. `/dev-story T1-COMBAT-01-cleric-base-combat-actor-fixture-hydration`

Evidence:

- `production/sprints/sprint-1.md:35`
- `production/sprints/sprint-1.md:37`
- `production/sprints/sprint-1.md:698`
- `production/sprint-status.yaml:13`

The producer smoke also verified that `tests/`, `production/stories/`,
`ProjectSettings/`, and previously `production/qa/` were absent before this
report write. That absence supports the `/test-setup` gate.

### Gameplay Programmer

The gameplay-programmer workload correctly identified `T1-COMBAT-01` as pure
actor + fixture work, not a place to absorb targeting, Attack toggle, casting,
or kill-credit integration.

Load-bearing evidence:

- `production/sprints/sprint-1.md:72` defines the story.
- `production/sprints/sprint-1.md:82` names expected write surfaces.
- `production/sprints/sprint-1.md:106` names the `Cleric_Mid_T1` fixture.
- `design/gdd/combat-core.md:630` defines `H-CCOM-ACTOR-01`.
- `design/gdd/combat-core.md:658` defines `H-CCOM-FIXTURE-01`.
- `design/gdd/combat-core.md:662` defines `H-CCOM-F2B`.
- `design/gdd/combat-core.md:790` defines `H-CCOM-SL-02`.
- `design/gdd/character-progression.md:853` defines `H-CPRO-SL-05`.

### Systems Designer

The systems-designer workload found the Combat Core / Character Progression
contract coherent:

- Combat -> Progression remains
  `PlayerKillCreditEvent(defeated_source_ref, zoneId, faction_id, kill_weight_seed)`;
  see `design/gdd/combat-core.md:194` and
  `design/gdd/character-progression.md:57`.
- Progression -> Combat remains ADR-0003
  `CombatProgressionBaselineSnapshot`; see
  `docs/architecture/adr-0003-progression-baseline-snapshot-contract.md:60`
  and `design/gdd/character-progression.md:77`.
- Combat must not consume `visible_level`, XP progress, or spell eligibility
  fields; see `design/gdd/combat-core.md:478`,
  `design/gdd/character-progression.md:77`, and
  `docs/architecture/adr-0003-progression-baseline-snapshot-contract.md:129`.

Non-blocking drift:

- `design/gdd/character-progression.md:22` calls ADR-0003 authoritative.
- `docs/architecture/adr-0003-progression-baseline-snapshot-contract.md:3`
  and `DECISIONS.md:222` still mark ADR-0003 / D009 as Proposed.

---

## Follow-Up Patch Recommendations

1. **Fix `.claude/agents/ai-programmer.md` before relying on that agent.**
   Restore the missing governance tail, close the open code fence, and add the
   missing sections: Instruction Priority, Self-Learning Protocol,
   Self-Healing Protocol, Memory Policy, Feedback Policy, Safety Guardrails,
   Output Standards, Reflection Checklist, Evaluation Checklist, Example
   Workflows, Delegation Map, and Final Behavioral Rule.

2. **Fix `.claude/agents/art-director.md` before relying on that agent.**
   The file appears cut off at line 160. Restore the behavioral-contract body
   from the upgraded template and close the open code fence.

3. **Use the untracked patch files as first-choice source material.**
   Check `all-skills-claude.patch` and `all-skills-claude-rebased.patch` for
   the full `ai-programmer.md` and `art-director.md` sections. If intact, copy
   the complete versions from the patch into the two agent files. If the patch
   sections are also truncated, reconstruct from sister-agent templates.

4. **Optional normalization:** add explicit `skills: []` to agents with no
   skills and explicit `disallowedTools: []` where Bash is intentionally
   allowed. This is not required for runtime, but it would make frontmatter
   audits unambiguous.

5. **Policy choice:** decide whether `localization-lead` keeps Bash for string
   extraction tooling or moves to the design/review no-Bash pattern.

6. **Non-agent metadata cleanup:** decide whether ADR-0003 / D009 should remain
   `Proposed` while Character Progression calls ADR-0003 authoritative. This is
   not blocking the agent smoke result.

---

## Overall Verdict

**NEEDS_FIX**

The governance upgrade is broadly functional: 46 of 48 remaining agents are
usable at structural smoke level, and all three critical-path workload smokes
produced scope-respecting, evidence-cited output. The two exceptions are real
truncation defects in `.claude/agents/ai-programmer.md` and
`.claude/agents/art-director.md`.

---

## Gate Recommendation

Do not treat the 48-agent upgrade as fully smoke-clean until
`.claude/agents/ai-programmer.md` and `.claude/agents/art-director.md` are
patched and re-audited.

It is safe to proceed with `/qa-plan sprint` because the critical-path agents
for that sequence (`producer`, `gameplay-programmer`, `systems-designer`, plus
previously smoke-tested `qa-lead`) are not blocked by these two truncations.

Recommended sequence:

1. Commit this smoke report as a standalone durable evidence artifact.
2. Patch `.claude/agents/ai-programmer.md` and `.claude/agents/art-director.md`
   under a separate EDIT_OK batch.
3. Re-run Tier 1 structural smoke on those two agents.
4. Commit the two-agent fix.
5. Resume Sprint 1 with `/qa-plan sprint`.

---

## What Changed

- Created `production/qa/`.
- Wrote `production/qa/agents-smoke-report-2026-04-28.md`.

## What's Next

Patch and re-audit the two truncated agent files, then resume Sprint 1 with
`/qa-plan sprint`.

## Blockers

The report is written, but the smoke verdict remains NEEDS_FIX until
`.claude/agents/ai-programmer.md` and `.claude/agents/art-director.md` are
repaired.
