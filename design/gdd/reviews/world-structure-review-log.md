# World Structure Review Log

## Review — 2026-04-23 (re-entry #5, same day) — Verdict: APPROVED
Scope signal: none required for content
Specialists: none this pass (senior gate review against updated repo state)
Blocking items: 0 | Recommended: 0
Summary: All prior blocker groups closed. Completeness 8/8. Dependency graph internally consistent (world-structure.md §Dependencies line 317) and reverse-synced in systems-index.md (lines 46, 51, 128, 139). Zone/group contract coherent across Rule 1, Rule 3, and ADR-tba-4. Rule 13 subscriber coverage mirrored across Interactions/Dependencies surfaces and systems-index. T1 acceptance suite gateable: two rows genuinely ADR-gated (H-F1-peak unlocks on ADR-tba-4(i); H-EC-A1-addressables unlocks on ADR-tba-4(b)(c)); four C2-sensitive rows (H-F1.1a, H-F1.1b, H-F4a, H-EC-C2) carry pre-specified Form L (load-side) and Form U (unload-side) fallback methodologies authored in the shared C2 caveat — T1-runnable on day one without further ADR-naming step. Summary table mechanically correct: 37 criteria / 32 T1-blocking / 2 ADR-gated / 2 advisory / 1 T4-deferred, verified directly from the table rows.
Prior verdict resolved: Yes — round-4 follow-up batch (world-structure.md + systems-index.md) closed all round-3 and round-4 residual items. Non-blocking follow-up captured separately as metadata sync (this batch).
Next steps: `/consistency-check` across approved GDDs; Save/Load & Persistence GDD as the next Foundation-layer system. Downstream GDDs (NPC System, Day/Night Cycle, Faction State Simulation, Faction Events, Dialogue System) each owe a Rule-13 `SessionResumeEvent` handler AC plus an organic-discovery design AC when authored (per D3 amendment). ADR-tba-1 through ADR-tba-5 remain open in §Open Questions — ADR-tba-4 (zone transition mechanism) and ADR-tba-5 (hub NPC schedule tick semantics) are T1-implementation-blocking prototypes.

## Review — 2026-04-23 (re-entry #4, same day) — Verdict: NEEDS REVISION
Scope signal: S
Specialists: none this pass (senior gate review)
Blocking items: 3 | Recommended: 0
Summary: Round-3 closed most content-level blockers but three small residuals remained. (1) Zone/group wording leaks in Rule 3 and ADR-tba-4(f) still used pre-round-3 "streaming group" / "zone=group binding" phrasing. (2) Reverse dependency map in systems-index.md omitted World Structure from Faction Events (§18) and Dialogue System (§23) — now canonical Rule-13 subscribers in world-structure.md. (3) Acceptance-gate accounting: the shared C2 caveat effectively made H-F1.1a, H-F1.1b, H-F4a, H-EC-C2 ADR-gated while the summary table still counted them as T1-blocking. Resolution required either promoting those rows into the ADR-gated bucket or pre-specifying a fallback methodology so they remain genuinely T1-runnable; plan opted for Option B with criterion-specific fallback forms (load-side for active-zone budget gates, unload-side for post-unload leak gates).
Prior verdict resolved: Partial — round-3 closed most content, but repo-wide dependency sync and acceptance-gate accounting still open.

## Review — 2026-04-23 (re-entry #3, same day) — Verdict: NEEDS REVISION
Scope signal: M
Specialists: game-designer, systems-designer, qa-lead, performance-analyst, unity/addressables specialist; senior synthesis
Blocking items: 3 grouped | Recommended: as embedded in blocker text | Disagreements: 0 (all specialists converged)
Summary: Round-2 cleanup resolved criterion-level and fantasy-language defects, but full review surfaced three contract-coherence blockers. (A) Zone-definition contract still contradictory — Rule 1 defined zone as ZoneManifest-entry-binding logical group-set, but Summary/Overview/Cross-References still taught "zone = separate/discrete Addressable streaming group," and Rule 1 itself framed the binding mechanism as settled while ADR-tba-4(d)(g) left field-shape and cross-group dependency open. (B) Rule 13 subscriber contract incomplete — Rule 13 named five downstream subscribers (NPC, Faction Sim, Day/Night, Faction Events, Dialogue) but Interactions/Dependencies tables modeled only three, and Day/Night's SessionResumeEvent subscription was prose-only. (C) T1 acceptance suite not implementation-safe — H-F1 used undefined timing aliases and treated the peak seam as resolved while ADR-tba-4(i) left it open; H-F1.1/H-F4a/H-EC-C2 assumed stable per-zone bundle-label attribution while ADR-tba-4(d)(e) left it open; H-F4b bundled schema and cadence and used debug-watch (outside declared taxonomy); H-EC-A1 remained overpacked around prototype-dependent behavior.
Prior verdict resolved: Partial — round-2 closed criterion-level items, but contract-coherence and T1-gateability blockers surfaced in full review.

## Review — 2026-04-23 (re-review, same day) — Verdict: NEEDS REVISION
Scope signal: M
Specialists: game-designer, systems-designer, qa-lead, performance-analyst, unity-specialist, unity-addressables-specialist, creative-director (senior synthesis)
Blocking items: 13 | Recommended: ~20 | Disagreements: 0 (all specialists converged)
Summary: All 6 prior blocker groups resolved at document level. Targeted-cleanup revision needed: four qa-lead criterion-level defects (H-F1.1 mis-classified, H-CR-13b vacuously true at T1, H-F4 compound 4-condition, H-CR-13c missing boundary cases), three game-designer fantasy-language issues (anchor-1 Pillar 2 fallback anti-fantasy, anchor-3 overstated T1 delivery, anchor-4 mechanism mis-attribution), two unity-addressables HIGH findings (N7 cross-group serialized-dependency risk, N3 UnloadSceneAsync vs Release for unactivated handles), two performance-analyst HIGH findings (P1 R_always zone-count drift, P2 H-F1 snapshot capture trigger), ADR-tba-4 prototype scope expansion to name unverified Unity 6.3 APIs explicitly, plus CD D3 binding amendment (silent-signal contract reaffirmed; downstream subscriber GDDs owe organic discovery design). No redesign required.
Prior verdict resolved: Partial — structural blockers (6 groups) resolved; new criterion-level, fantasy-language, and API-framing issues surfaced by adversarial re-review.

## Review — 2026-04-23 — Verdict: MAJOR REVISION NEEDED

Scope signal: XL
Specialists: game-designer, systems-designer, Unity specialist, performance analyst, QA lead, senior synthesis
Blocking items: 6 grouped | Recommended: 8
Summary: Full review found strong system intent and broadly correct boundaries, but the GDD is not implementation-ready. Blocking groups are transition semantics, memory residency, save identity, formula/registry contracts, T1-gateable acceptance criteria, and the T1 offline "world kept moving" bridge.
Prior verdict resolved: No — prior lean-mode concerns were revised, but full review found new implementation blockers.

### Blocking Groups

1. **Transition contract not implementable** — Rule 5 promises full locomotion during pre-load, while `ZoneLoading` locks the player and F3 defines felt latency until control returns; Edge A1 also assumes immediate Addressables load abort.
2. **Memory model internally contradictory** — Rule 3 assumes one fully resident streaming group, while `ZoneIdle` and corpse retention keep non-active zones resident without corresponding F1/F1.1 budget terms.
3. **Save/zone identity contract contradictory** — Rule 4 says no cached zone ID exists, while Save/Load and H-CS-01 require `zoneId`.
4. **Formula/registry contracts not validator-safe** — `T_load` uses seconds and milliseconds, `T_save` range conflicts with `save_mutex_max_ms`, registry ranges encode soft/unbounded outputs as hard values, and F4 declares a clamp the formula does not implement.
5. **Acceptance criteria not T1-gateable** — several T1-blocking criteria require CI/headless gates despite T1 being local-gate only; H-CR-01 is a T4 scalability test; H-CR-08 asserts Combat-owned behavior.
6. **T1 offline world-kept-moving bridge underspecified** — Section B promises real-world-day changes, but T1 is offline/local and hub/NPC tick semantics remain deferred.

### Recommended Revisions

- Define `ZoneIdle` as metadata/low-res retained state or add `R_idle_retained` to formulas and acceptance criteria.
- Rename formula units (`T_load_s` for F2, `T_load_ms` for F3).
- Change H-F1.1 from Addressable bundle-size validation to resident texture memory validation.
- Pin a reproducible min-spec profile including CPU, RAM, storage, and VRAM.
- Reframe "zone = Addressable group" as an authoring/build-validation rule using `ZoneManifest` scene keys/labels.
- Replace automated Frame Debugger parsing with static validation plus PlayMode instrumentation; keep Frame Debugger as manual evidence.
- Add one World-Structure-owned inter-zone anchor moment.
- Fix the acceptance summary count.
