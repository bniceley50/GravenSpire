# tasks/lessons.md — Accumulated Lessons

Append-only log of lessons learned. Tag with the taxonomy in `AGENTS.md` §9.
Promote repeating lessons to `CLAUDE-patterns.md` (cross-cutting) or a
`.claude/rules/*.md` file (path-scoped). Mark promoted entries with
`[PROMOTED to <target> YYYY-MM-DD]`.

---

## Format

```
### YYYY-MM-DD — [TAG][TAG2] one-line title
**Context:** what happened, one paragraph
**Lesson:** the generalized rule this teaches
**Evidence:** file path(s) or commit SHA
**Promotion status:** open / promoted to <target> on YYYY-MM-DD
```

---

## Entries (newest first)

### 2026-05-20 — [GLOBAL][SCOPE][TEST] Feel-gates belong only on playable stories; playability is a milestone entry condition

**Context:** The 2026-05-14 lesson "convert gameplay-feel acceptance into telemetry so blockout builds can still gate" was correct for M2's *mechanical* questions, but it was over-applied across the M3 slate: telemetry quietly substituted for player-facing assembly everywhere. Every M3 story (`S2-M3-01` through `S2-M3-04`) was typed Integration / Visual-Feel yet implemented as a runner-driven verification harness with no player input path — `Input.GetKey` appears in exactly one file in `Assets/Scripts` (`M2SingleTrashMedLoopController.cs`, the M2 combat loop). The M3 objective layer (named NPC, objective state, relic, loot, vendor) was proven by automated runners but never wired so a human could drive it. `S2-M3-04`'s AC-06 then asked the project lead to human-play and feel-judge a loop that had no input path and no navigable world; the lead opened the build and correctly reported it was not playable as a loop. A producer + creative-director reassessment confirmed the pillars and systems are sound; what failed is sequencing and the definition of "done" (D016).

**Lesson:** Telemetry proves a *system functions*; only a human-driven build proves a *loop is playable and worth playing* — and the two are different deliverables. A human-play / "feel" acceptance criterion may sit only on a story that is actually human-playable end to end; never bolt a human-play AC onto a runner-only story. Playability is a milestone *entry* condition, not a final-story acceptance criterion: a milestone whose purpose is "a reason to play" must own, as early work, a player-facing assembly (input + navigable space), and feel-test continuously from there rather than once at the end. If a feel AC's prerequisite playable state does not exist, the AC blocks loudly — it is not downgraded to a "qualified supplement." This refines and bounds the 2026-05-14 telemetry lesson: telemetry-as-gate is correct for mechanical correctness, wrong as a substitute for assembling something a person can play.

**Evidence:** `DECISIONS.md` D016; `tests/evidence/S2-M3-04/verification.md`; `tests/evidence/S2-M3-04/human-play-20260520.md`; the 2026-05-14 `[GLOBAL][TEST]` telemetry lesson below; producer + creative-director reassessments (2026-05-20 session).

**Promotion status:** open — promote to `.claude/rules/test-standards.md` (feel-gating rule) and the universal system prompt; this entry is the correction the 2026-05-14 telemetry lesson's "promote if it survives the M3+ presentation pass" clause anticipated — it did not fully survive.

### 2026-05-14 — [GLOBAL][TEST] Convert gameplay-feel acceptance into telemetry so blockout builds can still gate

**Context:** Sprint 2 M2 stories carry human-play acceptance criteria, but `S2-M2-02` human-play evidence found blockout-quality presentation (capsule actors, flat floor, debug HUD) too thin to validate "feel" via the "did you want one more pull?" bar (`m2_presentation_threshold_gap` carryover). `S2-M2-03` resolved this for its danger criterion not by improving presentation but by instrumenting intent: the overpull smoke recorded `overpull_outcome=forced_flee_threshold` and `ending_health=14/140` as mechanical proof, so AC-03 passed on telemetry while human-play stayed a qualified supplement. At `S2-M2-04` readiness the routing decision made this standing policy: named-blocker pacing must be proven through telemetry (discovery, time-to-danger, boundary pressure, clean-loop preservation, no farm-through), with human-play qualified by blockout visuals rather than blocking on them.

**Lesson:** When presentation fidelity is below the bar needed to validate "feel," do not defer the criterion and do not hand-wave it — instrument the design intent as telemetry. A blockout build cannot tell you if a mechanic feels good, but it can prove the mechanic does the thing (gates, escalates, resists trivialization). This decouples "is the mechanic correct?" (falsifiable now) from "does it feel right?" (needs art), keeps mechanical AC binding at low fidelity, and keeps human-play in the loop as a qualified supplement, not a removed step or a blocker.

**Evidence:** `production/stories/s2-m2-03-linked-trash-overpull.md` Completion Notes (Scope Notes); `tests/evidence/S2-M2-03/verification.md` AC `S2-M2-03-03` row; `production/sprint-status.yaml` carryover `m2_presentation_threshold_gap`; `tests/evidence/S2-M2-02/human-play-20260512.md`.

**Promotion status:** open — refines the standing "human play as first-class acceptance for gameplay-runtime stories from M2-02 onward" memory note; promote to `.claude/rules/test-standards.md` or the universal prompt if it survives the M3+ presentation pass.

### 2026-05-14 — [GLOBAL][CI][SCOPE] The T1 negative-scope scanner matches its own forbidden-term documentation

**Context:** The `S2-M2-03` closure ran the T1 negative-scope scan over changed files. Every hit was the scanner matching documentation that names forbidden terms in order to forbid them: the story's own "Out Of Scope" line (`s2-m2-03-linked-trash-overpull.md:67`) and the verification row's own quoted scan command. No runtime, test, scene, runner, or smoke file hit. The same pattern appeared at `S2-M2-01` closure ("only classified story/test/comment hits").

**Lesson:** A negative-scope scanner that greps for forbidden vocabulary will always self-match the documentation written to exclude that vocabulary. This is expected, not a violation — but it must be handled explicitly every time: classify each hit as doc-reference vs. real implementation hit, record the classification in verification evidence (the "PASS WITH CLASSIFIED DOC HITS" convention), and never let a future reader mistake a self-match for a scope breach. A hit on a runtime/scene/runner/smoke implementation file is the real signal.

**Evidence:** `tests/evidence/S2-M2-03/verification.md` Local Gates table (T1 negative-scope scan row); `production/stories/s2-m2-03-linked-trash-overpull.md:67`; `production/session-state/active.md` S2-M2-01 `/story-done` extract.

**Promotion status:** open — promote to `.githooks/pre-commit` docs or `game-dev-governance.md` if the classify-doc-hits step is formalized into the hook itself.

### 2026-05-14 — [GLOBAL][CI] Redact local-machine identifiers from Unity logs before committing them as evidence

**Context:** `S2-M2-03` commits the Unity batchmode runner log as required test evidence (`unity-linked-trash-overpull-runner-20260513.log`). That log contains a Unity `Licensing::Client` handshake message and other local machine identifiers from the local Editor environment. The log was redacted before commit and the redaction recorded in the story Completion Notes. Every Unity-runtime story in Sprint 2 (`S2-M2-01/02/03`) commits a Unity log as evidence.

**Lesson:** Unity batchmode logs are legitimate, required evidence — but they leak local environment data (licensing handshakes, machine names, absolute user paths). Redaction of local-machine identifiers is required before committing any Unity log that contains local identifiers, not optional cleanup, and the redaction itself should be recorded in verification evidence so a reviewer knows the log was sanitized, not truncated. Aligns with the governance rule against storing sensitive logs.

**Evidence:** `production/stories/s2-m2-03-linked-trash-overpull.md` Completion Notes (Scope Notes); `tests/evidence/S2-M2-03/verification.md` Runtime Notes (`Licensing::Client` handshake); `.claude/rules/game-dev-governance.md` Memory Policy ("Never Store: sensitive logs").

**Promotion status:** open — promote to a Unity evidence checklist on the next Unity-runtime closure.

### 2026-05-14 — [GLOBAL] MaterialPropertyBlock and other Unity engine-backed objects should not be created in MonoBehaviour field initializers

**Context:** `S2-M2-03` moved camp visual-state code toward the `MaterialPropertyBlock` + `GetPropertyBlock`/`SetPropertyBlock` pattern (the recommended fix for the `m2_renderer_material_property_access` carryover). In `M2SingleTrashMedLoopController.cs` the `MaterialPropertyBlock` is declared as a nullable field with no initializer (`private MaterialPropertyBlock? _materialPropertyBlock;`, line 43) and created lazily at first use (`_materialPropertyBlock ??= new MaterialPropertyBlock();`, line 1646) — deliberately not a field initializer, because C# field initializers run inside the constructor and Unity invokes MonoBehaviour constructors outside the normal engine lifecycle.

**Lesson:** Unity engine-backed objects — `MaterialPropertyBlock`, `Material`, `Texture`, `Mesh`, and similar — should not be constructed in a MonoBehaviour C# field initializer or constructor unless Unity documentation explicitly marks the type safe for constructor-time allocation. Initialize them lazily (`??=` at point of use) or in a lifecycle method (`Awake`/`OnEnable`). The lazy `??=` pattern also pairs correctly with nullable reference types: one honestly-`?`-typed field, one guaranteed construction site. A field initializer on a Unity-backed type is a latent main-thread / lifecycle-order bug even when it appears to work.

**Evidence:** `Assets/Scripts/M2SingleTrashMedLoopController.cs:43` and `:1646-1650` (commit `bb6deab`); `production/sprint-status.yaml` carryover `m2_renderer_material_property_access`.

**Promotion status:** open — promote to `.claude/rules/` Unity code-standards if the field-initializer trap recurs on another Unity-backed type.

### 2026-04-30 — [GLOBAL][CI][SCOPE] Promote staged hygiene and T1 negative-scope checks to hook

**Context:** Sprint 1 Combat Core implementation repeated the same local gates across `T1-COMBAT-01` through `T1-COMBAT-05`: `git diff --check` caught or confirmed whitespace/conflict-marker hygiene, and manual negative T1 scope greps repeatedly protected the offline single-player tier from FishNet, networking, server authority, PvP, companion, future-class, live-LLM, Unity frame-time, and wall-clock-time drift. The pattern has crossed the threshold where relying on manual recall is weaker than making the gate structural.

**Lesson:** Repeated pre-commit hygiene that protects project scope should become a local hook once it is stable and low-noise. The hook must run on staged files only, avoid broad tree scans, and stay tier-aware so T1 exclusions can be revised when a future DECISIONS.md tier-transition entry changes the allowed surface.

**Evidence:** `.githooks/pre-commit`; `.githooks/README.md`; `tests/evidence/T1-COMBAT-01/verification.md`; `tests/evidence/T1-COMBAT-02/verification.md`; `tests/evidence/T1-COMBAT-03/verification.md`; `tests/evidence/T1-COMBAT-04/verification.md`; `tests/evidence/T1-COMBAT-05/verification.md`.

**Promotion status:** [PROMOTED to .githooks/pre-commit 2026-04-30]

### 2026-04-28 — [GLOBAL][TEST] Use the narrowest real runner for runnable domain tests

**Context:** `T1-COMBAT-01` Stage 1 created pure C# Combat Core domain code and NUnit-style test intent before a Unity project shell existed. Stage 2 needed runnable verification without absorbing Unity bootstrap scope into the story. The selected harness was a small `tests/Gravenspire.Combat.Tests.csproj` bridge that compiles `src/gameplay/combat/**` plus the existing unit/integration tests and runs them with `dotnet test`, while Unity Test Runner remains the correct path for later MonoBehaviour, scene, and PlayMode-dependent code.

**Lesson:** Match the runner to the code under test, but still require real execution. Pure domain code should use the cheapest runnable local harness that exercises the actual production files and tests. Unity shell creation belongs at the point where Unity APIs, MonoBehaviours, scenes, or PlayMode behavior are required; it should not be smuggled into an unrelated domain story just to satisfy a test-run checkbox. The durable split is: domain code can run under `dotnet test`; Unity-dependent code runs under Unity Test Runner.

**Evidence:** `tests/Gravenspire.Combat.Tests.csproj`; `tests/evidence/T1-COMBAT-01/verification.md`; `tests/evidence/T1-COMBAT-01/t1-combat-01-stage2.trx`; command `dotnet test tests\Gravenspire.Combat.Tests.csproj --logger "trx;LogFileName=t1-combat-01-stage2.trx" --results-directory "tests\evidence\T1-COMBAT-01"` passed 15/15 tests.

**Promotion status:** open — promote to `.claude/rules/test-standards.md` or the universal prompt after this runner split repeats outside the first Combat Core domain slice.

### 2026-04-26 — [GLOBAL][SCOPE] Prototype-validated pillar beats speculative pillar

**Context:** The combat-feel prototype cycle (commits `7add6ee` through
`83598de`) ran v1 baseline, v2 tactical instants plus Attack toggle, and pinned
Unity `6000.3.14f1` validation in one day. The session arc was: speculation
("does EQ-Classic combat still feel good in 2026?") → playtest → playtester
instinct ("can we have manual melee?") → explicit design-tension surfacing
(Read A/B/C) → playtester chose Read A → v2 implementation → re-playtest →
pinned-engine validation gate → headline milestone. The result moved the
project's biggest documented core-hypothesis risk from speculation to
prototype-grade evidence on the pinned engine.

**Lesson:** Pillars locked by speculation are fragile until prototype-validated.
The pillar discipline that survived this session — "EQ-Classic IS, not inspired
by" plus P2 Silence Is Sacred — was preserved by surfacing the design tension
when playtester instinct conflicted with it, not by silently implementing the
first requested mechanic. The Read A/B/C framing let the player choose an
agency direction while keeping the pillar-fit tradeoff visible, then the chosen
path was revalidated to confirm it did not dilute med breaks. General rule:
when playtester instinct conflicts with a pillar, surface alternatives with
pillar-fit notes; do not silently choose either capitulation or rejection.

**Evidence:** Commits `7add6ee` through `83598de`;
`production/prototypes/combat-feel-report.md` v1, v2, and pinned-validation
sections; pinned-engine metrics captured in
`prototypes/combat-feel/Logs/playtest-20260426-204721.log` and
`prototypes/combat-feel/Logs/playtest-20260426-205508.log`; D012 in
`DECISIONS.md`.

**Promotion status:** open — promote to universal system prompt
(`docs/brian-system-prompt-v4-6.md`) when that placeholder is populated; this
discipline is universal across design-prototype cycles, not Gravenspire-
specific. Tag remains `[GLOBAL]` for that propagation pathway.

### 2026-04-26 — [GLOBAL][TEST] Prototype smoke checks catch cadence bugs before human playtest

**Context:** During combat-feel prototype scaffolding (commit `7add6ee`), a
scripted smoke runner was added alongside the human-playtest scene to validate
the multi-pull loop mechanically before any qualitative playtest. The smoke
run caught two real bugs that would have invalidated human feel-test findings:
per-frame integer-floored mana regen, where med breaks would never restore
mana under normal frame rates, and per-frame integer-floored health regen,
where post-pull recovery would stall forever. Both bugs were fixed before the
prototype was opened for self-playtest.

**Lesson:** Prototypes that test *feel* still benefit from mechanical smoke
validation. The qualitative question being asked is hostage to the quantitative
machinery actually working. A five-minute scripted loop check before human
playtest prevents the "playtested for 30 minutes, found med break broken, threw
out the playtest data" failure mode. Smoke discipline is not just for
production code; it is for any artifact whose purpose is to produce evidence.

**Evidence:** `prototypes/combat-feel/Assets/Scripts/Editor/CombatFeelSmokeRunner.cs`;
commit `7add6ee`; bugs caught in iterative `ClericActor.cs` regen-as-float fix.

**Promotion status:** open — promote to universal system prompt
(`docs/brian-system-prompt-v4-6.md`) when that placeholder is populated; this
discipline is universal across all prototype work, not Gravenspire-specific.
Tag remains `[GLOBAL]` for that propagation pathway. Could also promote to
`.claude/rules/prototype-discipline.md` if a rules file is ever created for
prototype work specifically.

### 2026-04-26 — [GLOBAL][SCOPE] Repair contract drift, park implementation detail

**Context:** Inventory & Item Economy full design review found six legitimate
blocker groups after authoring was complete. Only one blocker created active
cross-document drift: Inventory claimed Save/Load invoked
`InventoryFirstSaveMaterializer`, while Save/Load did not reverse-list it.
The remaining blockers were implementation-pre-spec work (schema identity,
partial-stack math, currency/vendor transaction closure, UI result handoff,
and future-system fixture gating). We repaired the Save/Load drift and parked
Inventory behind `INV-OQ-05` instead of running another large design round
before validating combat feel.

**Lesson:** When review uncovers many valid issues, separate false committed
claims from honest future work. Fix real cross-document drift immediately; do
not let broad implementation detail pull the project into over-design before
the current strategic risk is validated. A parked pre-spec entry is better
than pretending a system is approved, and better than burning a prototype
window on non-blocking precision.

**Evidence:** Commit `294a365` (Save/Load reverse-listing repair + Inventory
park); `design/gdd/inventory-item-economy.md` `INV-OQ-05`;
`production/session-state/active.md` prototype pivot entry.

**Promotion status:** open — promote to universal system prompt once repeated
across another project or another major Gravenspire review cycle.

---

### 2026-04-25 — [GLOBAL] Approved work hoarding without prompt protocol

**Context:** Across multiple sessions of the Pre-Production design pass,
~5,000 lines of `/design-review`-APPROVED design work (7 GDDs + 7 review
logs + systems-index updates + entities.yaml registry sync) accumulated
uncommitted in the working tree. The discipline "no commit without user
instruction" was honored, but no complementary discipline forced the agent
to *prompt* for commit at the moment of approval. The accumulation was
recovered by a single catch-up commit `f1df1c5` (2026-04-25), but a worktree
corruption or hard reset before the catch-up would have lost weeks of work.

**Lesson:** "No commits without user instruction" is necessary but
insufficient. It must be paired with an agent prompting obligation at every
approval checkpoint (`/design-review APPROVED`, `EDIT_OK` + verified batch,
test-passing implementation milestone, end-of-session). Approved work that
sits uncommitted across session boundaries is forgotten work and lost work
waiting to happen. The fix is structural protocol, not vigilance — quiet
competence reliably forgets the commit question; explicit prompts at
approval moments do not.

**Evidence:** `AGENTS.md` §14 (Commit & Push Cadence, added 2026-04-25);
`CLAUDE.md` Collaboration Protocol cross-reference; commit `f1df1c5`
(catch-up batch); commit `32e13a6` (the prior commit before the
accumulation, showing the gap window).

**Promotion status:** open — promote to universal system prompt
(`docs/brian-system-prompt-v4-6.md`) when that placeholder is populated;
this failure mode is universal across all Brian projects, not Gravenspire-
specific. Tag remains `[GLOBAL]` for that propagation pathway.

---

### 2026-04-21 — [GLOBAL][CI] Pre-build/pre-deploy config verification is not automatic

**Context:** Inherited lesson from clinic-notes-ai 2026-04-10. A production
outage occurred because `UPSTASH_REDIS_REST_TOKEN` was missing from the Vercel
Production env. The governance had no pre-deploy env-vars check — the team
assumed "configured" meant configured, with no verification step that
produced evidence. Same class of failure will hit Gravenspire when a Steam
build goes out with a broken Addressables group, a stale version stamp, or
(T3+) a missing server config.

**Lesson:** Every deploy/build surface needs a verification checklist that
produces evidence (file:line, screenshot, console output), **not self-report**.
"I checked" is not evidence. "Here's the line" is evidence. For Gravenspire,
the checklist is `production/pre-build-checklist.md`.

**Evidence:** `production/pre-build-checklist.md` (created 2026-04-21 as part
of the D005 governance migration); `AGENTS.md` §12.

**Promotion status:** open — promote to `.claude/rules/game-dev-governance.md`
if the same class of near-miss recurs during T1.

---

<!-- Add new lessons above this line (newest first). -->
