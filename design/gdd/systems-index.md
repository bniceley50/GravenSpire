# Systems Index: Gravenspire

> **Status**: Draft
> **Created**: 2026-04-22
> **Last Updated**: 2026-04-22
> **Source Concept**: [design/gdd/game-concept.md](game-concept.md)
> **Governing Art Bible**: [design/art/art-bible.md](../art/art-bible.md)

> **Phase Gates (Lean Review Mode):** `TD-SYSTEM-BOUNDARY` skipped. `PR-SCOPE` skipped. `CD-SYSTEMS` skipped. Re-run `/map-systems --review full` for formal director sign-off if required before GDD authoring begins.

---

## Overview

Gravenspire is a small persistent gothic MMO that combines Classic-EQ tab-target combat with deep autonomous faction simulation. Mechanically, the game lives at the intersection of **six core pillars**: live-simulated faction politics, EQ-native combat pacing, diegetic world information, named AI companions, LLM-driven NPC dialogue, and a 10-50 concurrent persistent server.

This decomposes into **33 systems** spanning foundation infrastructure (world streaming, persistence), core gameplay (combat, classes, progression), the game's identity systems (faction simulation, reputation, zone control), AI companion depth (named NPCs with personalities), LLM dialogue (Tier 3+), interface (two-layer UI with diegetic world information), networking (FishNet Tier 2+), and polish (audio, accessibility).

The system count is toward the small end of MMO scope (typical MMOs run 50-100+ systems) but toward the large end of indie solo-dev scope (typical solo indie projects run 15-25 systems). Ruthless prioritization against the four scope tiers from the concept document is essential: **26 systems are required for Tier 1 MVP** (single-player vertical slice, 1 class, 1 haunt, 1 faction); remaining systems accumulate across Tier 2 (co-op alpha), Tier 3 (persistent beta), and Tier 4 (full vision).

---

## Systems Enumeration

Systems marked **E** are explicit in the game concept's Core Mechanics or Technical Considerations. Systems marked **I** are inferred — required by the explicit systems but not named in the concept.

| # | System | Category | Priority | Status | Design Doc | Depends On |
|---|---|---|---|---|---|---|
| 1 | World Structure (I) | Core | **MVP** | Not Started | — | — |
| 2 | Save / Load & Persistence (I) | Persistence | **MVP** | Not Started | — | — |
| 3 | Menus & Settings (I) | UI | **MVP** | Not Started | — | — |
| 4 | NPC System (I) | Core | **MVP** | Not Started | — | World Structure |
| 5 | Day/Night Cycle (I) | Core | **MVP** | Not Started | — | World Structure |
| 6 | Character Creation (I) | Core | **MVP** | Not Started | — | Save/Load |
| 7 | Combat Core (E) | Gameplay | **MVP** | Not Started | — | World Structure, NPC System, Save/Load |
| 8 | Character Progression (I) | Progression | **MVP** | Not Started | — | Character Creation, Save/Load |
| 9 | Inventory & Item Economy (I) | Economy | **MVP** | Not Started | — | Save/Load, Character Creation |
| 10 | Class Design (I) | Gameplay | **MVP** (Cleric) / **T2** (Warrior, Enchanter) | Not Started | — | Combat Core, Character Progression |
| 11 | Spell Memorization (I) | Gameplay | **MVP** | Not Started | — | Combat Core, Class Design |
| 12 | Status Effects & Buffs (I) | Gameplay | **MVP** | Not Started | — | Combat Core |
| 13 | Creature / Enemy AI (I) | Gameplay | **MVP** | Not Started | — | NPC System, Combat Core |
| 14 | Death & Corpse Recovery (I) | Gameplay | **MVP** | Not Started | — | Combat Core, Character Progression, Save/Load |
| 15 | Faction State Simulation (E) | Gameplay | **MVP** (1 faction reactive) / **T3** (3 factions) / **T4** (6 factions autonomous) | Not Started | — | World Structure, NPC System, Save/Load |
| 16 | Faction Reputation (E) | Progression | **MVP** (1 faction) | Not Started | — | Faction State Simulation, Save/Load, Character Progression |
| 17 | Zone Control (I) | Gameplay | **MVP** (1 zone) | Not Started | — | Faction State Simulation, Combat Core |
| 18 | Faction Events (I) | Narrative | **MVP** (templated, 1 faction) | Not Started | — | Faction State Simulation, NPC System |
| 19 | Named AI Companion Core (E) | Gameplay | **MVP** (Elara only) / **T2** (expanded roster) | Not Started | — | NPC System, Combat Core, Class Design, Enemy AI (patterns) |
| 20 | Hiring Hall (I) | Gameplay | **Tier 2** | Not Started | — | Named AI Companion Core, Faction State Simulation |
| 21 | Companion Relationships (I) | Gameplay | **Tier 2** | Not Started | — | Named AI Companion Core, Save/Load |
| 22 | Sister Elara Mentor (I) | Meta | **MVP** | Not Started | — | Named AI Companion Core |
| 23 | Dialogue System (E) | Narrative | **MVP** (templated) / **T3** (LLM for key NPCs) | Not Started | — | NPC System, Faction State Simulation |
| 24 | Moderation & Safety (I) | Meta | **Tier 3** | Not Started | — | Dialogue System |
| 25 | Layer 1 HUD (I) | UI | **MVP** | Not Started | — | Combat Core, Status Effects |
| 26 | Dialogue UI Panel (I) | UI | **MVP** | Not Started | — | Dialogue System |
| 27 | Personal Journal (I) | UI | **MVP** | Not Started | — | Faction Reputation |
| 28 | Faction Board UI (I) | UI | **MVP** | Not Started | — | Faction Events, Faction State Simulation |
| 29 | Network Architecture (E) | Core | **Tier 2** (co-op) / **T3** (persistent server) | Not Started | — | Combat Core, Save/Load |
| 30 | Authentication & Accounts (I) | Persistence | **Tier 2** | Not Started | — | Network Architecture, Save/Load |
| 31 | Social Systems (I) | Gameplay | **Tier 2** (chat + party) / **T4** (guilds/cabals) | Not Started | — | Network Architecture |
| 32 | Audio System (I) | Audio | **MVP** | Not Started | — | Combat Core, Dialogue, World Structure |
| 33 | Accessibility (I) | Meta | **Tier 3** (proper pass; keybinding in MVP via Menus) | Not Started | — | UI systems, Audio, Input |

---

## Categories

| Category | Gravenspire Systems |
|---|---|
| **Core** | World Structure, NPC System, Day/Night Cycle, Network Architecture |
| **Gameplay** | Combat Core, Class Design, Spell Memorization, Status Effects, Creature AI, Death/Corpse, Faction Sim, Zone Control, Named AI Companions, Hiring Hall, Companion Relationships, Social Systems |
| **Progression** | Character Progression, Faction Reputation |
| **Economy** | Inventory & Item Economy |
| **Persistence** | Save/Load, Authentication & Accounts |
| **UI** | Menus & Settings, Layer 1 HUD, Dialogue UI Panel, Personal Journal, Faction Board UI |
| **Audio** | Audio System |
| **Narrative** | Faction Events, Dialogue System |
| **Meta** | Sister Elara Mentor, Moderation & Safety, Accessibility |

---

## Priority Tiers

Priority tiers map directly to Gravenspire's 4-tier scope plan from the concept document. Each tier is independently shippable.

| Tier | Definition | Milestone | Scope |
|---|---|---|---|
| **MVP (Tier 1)** | Single-player vertical slice that tests: "does EQ combat still feel good, and does faction-shifts-camp-control feel meaningful?" | ~6-12 months solo | 1 class (Cleric), 1 haunt zone, 1 faction (Vampire Court), offline |
| **Tier 2 (Alpha)** | Co-op multiplayer with holy trinity classes | ~12-18 months cumulative | + Warrior + Enchanter, P2P/lobby 2-6 player co-op, AI companion roster |
| **Tier 3 (Beta)** | Persistent small-server MMO with LLM dialogue | ~18-30 months cumulative | + 2nd haunt, 3 factions, 10-person persistent server, LLM for 5-10 key NPCs |
| **Tier 4 (Full Vision)** | Complete design with deep autonomous simulation | 36+ months cumulative, open-ended | + 3-5 zones, all 6 factions, deep autonomous faction sim, 50-person server |

**Shipping philosophy:** every tier is shippable on its own. If life intervenes at Tier 2, Gravenspire exists as a small co-op gothic RPG. At Tier 3, it is a cult-classic small MMO. At Tier 4, it is the full vision. No tier is a stepping stone that's worthless without the next one.

---

## Dependency Map

Systems sorted by dependency order. Design and build from top (Foundation) to bottom (Polish).

### Layer 1 — Foundation *(no dependencies on other game systems)*

1. **World Structure** — zone architecture, Addressables streaming groups, zone transition mechanics
2. **Save / Load & Persistence** — character data, world state, faction state; SQLite Tier 1, scales later
3. **Menus & Settings** — pause, options, input remapping, accessibility controls framework

### Layer 2 — Core *(depend only on Foundation)*

4. **NPC System** — depends on: World Structure. ⚠ **Bottleneck — 6 downstream systems depend on this.** Ambient + named behavioral framework, occupation postures, idle loops.
5. **Day/Night Cycle** — depends on: World Structure. Court hours vs. inn hours; world state shifts per art bible S2.
6. **Character Creation** — depends on: Save/Load. Class selection, starting faction-neutral state.
7. **Combat Core** — depends on: World Structure, NPC System, Save/Load. ⚠ **Bottleneck — 9 downstream systems.** The core hypothesis.
8. **Character Progression** — depends on: Character Creation, Save/Load. EQ-native XP, levels, spell unlocks.
9. **Inventory & Item Economy** — depends on: Save/Load, Character Creation. Items, gear, faction tokens, Syndicate transactions.

### Layer 3 — Core Extensions *(depend on Layer 2)*

10. **Class Design** — depends on: Combat Core, Character Progression. Cleric (Tier 1), Warrior/Enchanter (Tier 2).
11. **Spell Memorization** — depends on: Combat Core, Class Design. EQ-style spellbook mechanics.
12. **Status Effects & Buffs** — depends on: Combat Core. Buffs, debuffs, crowd control.
13. **Creature / Enemy AI** — depends on: NPC System, Combat Core. The pivot; hostile creature combat state machine.
14. **Death & Corpse Recovery** — depends on: Combat Core, Character Progression, Save/Load. Corpse runs, XP loss, rez.

### Layer 4 — Faction Systems *(Gravenspire's soul)*

15. **Faction State Simulation** — depends on: World Structure, NPC System, Save/Load. ⚠ **Bottleneck — 4 downstream systems.** Autonomous faction AI; reactive at MVP, autonomous at Tier 4.
16. **Faction Reputation** — depends on: Faction State Simulation, Save/Load, Character Progression. Per-player per-faction rep, 5-tier progression.
17. **Zone Control** — depends on: Faction State Simulation, Combat Core. **The load-bearing bridge** — kills shift faction camp ownership.
18. **Faction Events** — depends on: Faction State Simulation, NPC System. Assassinations, wars, leader changes.

### Layer 5 — AI Companions

19. **Named AI Companion Core** — depends on: NPC System, Combat Core, Class Design, Enemy AI (patterns reused). Class AI (cleric, enchanter), hire/dismiss, identity persistence.
20. **Hiring Hall** — depends on: Named AI Companion Core, Faction State Simulation. Inverse-population scaling (Tier 2+).
21. **Companion Relationships** — depends on: Named AI Companion Core, Save/Load. Grudges, preferences, faction allegiances.
22. **Sister Elara Mentor** — depends on: Named AI Companion Core. Onboarding-specific AI companion.

### Layer 6 — Dialogue

23. **Dialogue System** — depends on: NPC System, Faction State Simulation. Templated (Tier 1-2) → LLM (Tier 3+).
24. **Moderation & Safety** — depends on: Dialogue System. LLM output filtering.

### Layer 7 — Presentation (UI)

25. **Layer 1 HUD** — depends on: Combat Core, Status Effects. Health/mana/hate/spell queue per art bible S7.
26. **Dialogue UI Panel** — depends on: Dialogue System. Diegetic faction-specific paper panel.
27. **Personal Journal** — depends on: Faction Reputation. Diegetic carried object tracking faction standings.
28. **Faction Board UI** — depends on: Faction Events, Faction State Simulation. Diegetic world-object bulletin.

### Layer 8 — Networking *(Tier 2+)*

29. **Network Architecture** — depends on: Combat Core, Save/Load. FishNet-based client-server.
30. **Authentication & Accounts** — depends on: Network Architecture, Save/Load.

### Layer 9 — Social *(Tier 2+)*

31. **Social Systems** — depends on: Network Architecture. Text chat, party, guilds (Tier 4).

### Layer 10 — Polish

32. **Audio System** — depends on: Combat, Dialogue, World Structure. Ambient zones, combat audio, Silence-Is-Sacred discipline.
33. **Accessibility** — depends on: UI systems, Audio, Input. Cross-cutting; colorblind, remapping, low-vision, reduced motion.

---

## Recommended Design Order

Combining dependency sort + priority tier. **Write the GDDs in this order.** Independent systems at the same layer can be designed in parallel if capacity allows.

| Order | System | Priority | Layer | Primary Agent | Est. Effort | Notes |
|---|---|---|---|---|---|---|
| 1 | World Structure | MVP | Foundation | game-designer + engine-programmer | M | Foundation for everything. Scoped to 1 zone + 1 hub at MVP. |
| 2 | Save / Load & Persistence | MVP | Foundation | engine-programmer | M | SQLite schema + character/world state serialization. |
| 3 | Menus & Settings | MVP | Foundation | ui-programmer + ux-designer | S | Input remapping is MMO-critical per technical-preferences. |
| 4 | NPC System | MVP | Core | ai-programmer + game-designer | L | **Bottleneck — high-quality design critical.** |
| 5 | Day/Night Cycle | MVP | Core | game-designer | S | Coupled with World Structure scene state. |
| 6 | Character Creation | MVP | Core | game-designer + systems-designer | S | Single class at MVP (Cleric) — scope is small. |
| 7 | **Combat Core** | MVP | Core | **game-designer + systems-designer** | **L** | **THE core hypothesis. Prototype this earliest.** |
| 8 | Character Progression | MVP | Progression | systems-designer | M | EQ-native XP curves; formula-heavy. |
| 9 | Inventory & Item Economy | MVP | Economy | economy-designer + systems-designer | M | Faction tokens, rep-gated items, currency. |
| 10 | Class Design — Cleric | MVP | Gameplay | game-designer + systems-designer | L | Cleric is hardest class — group-dependency pivots here. |
| 11 | Spell Memorization | MVP | Gameplay | game-designer | M | Signature mechanic; EQ-specific. |
| 12 | Status Effects & Buffs | MVP | Gameplay | systems-designer | M | Interaction matrix is formula-heavy. |
| 13 | Creature / Enemy AI | MVP | Gameplay | ai-programmer | M | The pivot behavior is load-bearing. |
| 14 | Death & Corpse Recovery | MVP | Gameplay | game-designer + systems-designer | S | Corpse run + XP loss + rez. |
| 15 | **Faction State Simulation** | MVP | Gameplay | **game-designer + ai-programmer + systems-designer** | **L** | **Bottleneck. Reactive at MVP.** |
| 16 | Faction Reputation | MVP | Progression | systems-designer + economy-designer | M | 5-tier progression; trigger events. |
| 17 | **Zone Control** | MVP | Gameplay | **game-designer + systems-designer** | **M** | **The combat ↔ politics bridge — prototype alongside Combat Core.** |
| 18 | Faction Events | MVP | Narrative | narrative-director + game-designer | M | Templated at MVP; LLM at T3. |
| 19 | Named AI Companion Core | MVP | Gameplay | ai-programmer + game-designer | L | Class AI competence is hard. |
| 22 | Sister Elara Mentor | MVP | Meta | ai-programmer + narrative-director | M | Onboarding-critical. |
| 23 | Dialogue System (templated) | MVP | Narrative | narrative-director + writer | M | Templated at MVP; LLM at T3. |
| 25 | Layer 1 HUD | MVP | UI | unity-ui-specialist + ui-programmer | M | Art bible S7 spec is detailed — execution from spec. |
| 26 | Dialogue UI Panel | MVP | UI | unity-ui-specialist | M | Diegetic paper panel per art bible S7.3. |
| 27 | Personal Journal | MVP | UI | unity-ui-specialist + narrative-director | M | Diegetic held object; world-written. |
| 28 | Faction Board UI | MVP | UI | unity-ui-specialist | S | Physical world object per art bible. |
| 32 | Audio System | MVP | Audio | audio-director + sound-designer | M | Silence Is Sacred discipline in practice. |
| — | *End of MVP — 26 systems complete* | | | | | |
| 27 | Class Design — Warrior + Enchanter | Tier 2 | Gameplay | game-designer + systems-designer | L | Holy trinity completion. |
| 28 | Named AI Companion Core — expanded roster | Tier 2 | Gameplay | ai-programmer | M | Beyond Elara. |
| 29 | Hiring Hall | Tier 2 | Gameplay | game-designer + systems-designer | M | Inverse-population scaling. |
| 30 | Companion Relationships | Tier 2 | Gameplay | ai-programmer + narrative-director | M | Grudges, preferences. |
| 31 | Network Architecture | Tier 2 | Core | network-programmer | L | FishNet P2P/lobby. |
| 32 | Authentication & Accounts | Tier 2 | Persistence | network-programmer + security-engineer | M | Login + character slots. |
| 33 | Social Systems (chat + party) | Tier 2 | Gameplay | ui-programmer + network-programmer | M | Text chat + group formation. |
| — | *End of Tier 2 Alpha* | | | | | |
| 34 | Faction State Simulation — 3 factions | Tier 3 | Gameplay | game-designer + ai-programmer | M | Expansion, not new GDD. |
| 35 | Network Architecture — persistent server | Tier 3 | Core | network-programmer | L | VPS deployment + persistent world state. |
| 36 | Dialogue System — LLM | Tier 3 | Narrative | narrative-director + ai-programmer | L | OpenAI/Anthropic/local integration; NPC memory. |
| 37 | Moderation & Safety | Tier 3 | Meta | security-engineer + narrative-director | M | LLM output filtering. |
| 38 | Accessibility | Tier 3 | Meta | accessibility-specialist | M | Proper pass. |
| — | *End of Tier 3 Beta* | | | | | |
| 39+ | Tier 4 expansions | T4 | Various | Various | M-L each | Deep autonomous sim, full LLM coverage, 6 factions, Guilds/Cabals. |

**Effort legend:** S = 1 design session (1-2 days). M = 2-3 sessions (3-6 days). L = 4+ sessions (1+ weeks).

**MVP design effort total:** ~25 systems × ~2 sessions average = **~50 design sessions**, roughly **10-15 weeks of focused GDD authoring** before any MVP implementation begins. Solo dev schedules will vary — this is why prototyping Combat Core early (in parallel with authoring the remaining GDDs) is the correct move.

---

## Circular Dependencies

**None found.** Tight couplings noted (NPC System ↔ Faction State Simulation, Combat Core ↔ Zone Control), but directionality is clear in all cases:
- NPC System provides the container; Faction State Simulation provides the state.
- Combat Core produces events (kills); Zone Control consumes them to update faction ownership.

---

## High-Risk Systems

Systems flagged for early prototyping regardless of priority tier — get these wrong and everything downstream suffers.

| System | Risk Type | Risk Description | Mitigation |
|---|---|---|---|
| **Combat Core** | Technical + Design | EQ-classic tab-target may not feel good to modern players in 2026; this is the core hypothesis of the entire project. | **`/prototype combat-feel` should be the first implementation work** after Foundation GDDs are written. Validate before building anything else on top. |
| **Faction State Simulation** | Design | "Alive but legible" sweet spot is narrow. Too chaotic = meaningless; too static = nothing happens. Unproven at solo-dev scale. | MVP is **reactive sim only** (faction state responds to player actions). Autonomous autonomous sim deferred to Tier 4 so mechanism can evolve from a working foundation. |
| **Zone Control — kills shift faction control** | Design | The load-bearing bridge between 30-sec combat and macro faction politics. If it doesn't feel meaningful, the game has no soul. | **Prototype alongside Combat Core** — cannot be validated in isolation. |
| **Named AI Companion Core** | Technical | Class AI competent enough to fill tank/heal/CC roles in a tab-target group is genuinely hard. | MVP scopes to ONE companion (Sister Elara as Cleric). Validate class AI quality before expanding roster. |
| **Network Architecture (persistent server)** | Technical | #1 project-killer risk per the concept doc. Tier 3 requirement — a solo first-time dev has never done this. | **Tier 1-2 skip networking entirely** (offline → P2P co-op). Tier 3 uses single-VPS dedicated server with 10-person cap; no horizontally scaled MMO infra planned. |
| **Dialogue System — LLM at scale** | Technical + Cost | LLM cost per conversation, latency, coherence of memory across sessions. 2026 tech barely handles this at small scale. | Templated in Tier 1-2. LLM scoped to 5-10 key NPCs in Tier 3. Full coverage only in Tier 4 if tech and cost improve, or if running local models on 5090 is viable. |
| **Sister Elara onboarding** | Design | The pillars prohibit tutorials, but modern players need to learn EQ-pacing mechanics. Elara's behavior-teaching is unproven. | Must be playtest-validated in Tier 1. If it fails, onboarding has to be rethought — this is the hardest new-player problem. |
| **First-hour accessibility** | Design | EQ-classic pacing is abrasive; the slow onboarding through Elara may push players away in 15 minutes before they develop the literacy. | Playtest-driven iteration on the first-hour experience during Tier 1. |

---

## Progress Tracker

| Metric | Count |
|---|---|
| Total systems identified | 33 |
| Design docs started | 0 |
| Design docs reviewed | 0 |
| Design docs approved | 0 |
| MVP systems designed | 0 / 26 |
| Tier 2 systems designed | 0 / 7 |
| Tier 3 systems designed | 0 / 5 |
| Tier 4 systems designed | 0 / — |

---

## Next Steps

- [ ] Start with **Combat Core** as the MVP hypothesis system, but design the Foundation layer first (World Structure → Save/Load → NPC System). Run `/design-system [system-name]` to begin.
- [ ] Run `/map-systems next` to always pick the highest-priority undesigned system automatically.
- [ ] Run `/design-review design/gdd/[system].md` in a fresh session after each GDD is authored.
- [ ] **Prototype Combat Core + Zone Control early** — the concept's core hypothesis cannot be validated in design docs alone. Run `/prototype combat-feel` after enough GDDs are written to inform the prototype.
- [ ] Run `/create-architecture` after MVP GDDs (or a strategic subset) are authored — architecture translates GDDs into technical blueprints.
- [ ] Run `/gate-check pre-production` when MVP GDDs are designed, prototype-validated, and architecture is drafted.
