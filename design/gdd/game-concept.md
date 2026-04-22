# Game Concept: Gravenspire

*Created: 2026-04-21*
*Status: Draft*

---

## Elevator Pitch

> **Gravenspire** is a small persistent gothic MMO where EverQuest-Classic combat meets a deeply simulated undead political sandbox. You earn a name in a cursed city where 4-6 undead factions scheme against each other in real time — even while you sleep — and your reputation with the lords of the dead is the real progression.

*10-second test: "It's EverQuest Classic set inside one gothic city-state where the AI factions actually play the political game with you."*

---

## Core Identity

| Aspect | Detail |
| ---- | ---- |
| **Genre** | MMORPG (gothic horror / political intrigue subgenre) |
| **Platform** | PC (Steam / Epic) |
| **Target Audience** | Explorer-Socializers: 25-50 year-olds with Classic EQ, Pantheon, P1999, VtM, and tabletop RPG DNA |
| **Player Count** | Small persistent MMO (10-50 concurrent per server) |
| **Session Length** | 60-180 minutes (EQ-native; natural stopping point is "return to the city") |
| **Monetization** | Premium / none yet (treat as passion project; revisit post-Tier-3) |
| **Estimated Scope** | Large (multi-year, solo — Tier 1 MVP 6-12 months, Tier 4 full vision 36+ months) |
| **Comparable Titles** | EverQuest Classic (Project 1999), Pantheon: Rise of the Fallen, Vampire: The Masquerade — Bloodlines, Disco Elysium |

---

## Core Fantasy

**"You earn a name in a cursed city where the undead lords know who you are."**

Gravenspire is not a power fantasy. It is an *identity* fantasy — the promise that your character will become a *specific, named, remembered person* in a small world full of other named, remembered people. Human or AI, your allies have faces. Human or AI, your enemies have grudges. The vampire court knows your reputation. The ghoul syndicate knows your face. Your patron remembers what you did last Thursday.

You can be brave in Gravenspire. You can be greedy, clever, treacherous, loyal, or reckless. The city does not tell you who to be. It just remembers what you did.

What you can do here that you can't do anywhere else: *participate in a living gothic political simulation at the pace and stakes of Classic EverQuest, alongside a small community where your reputation is real.*

---

## Unique Hook

**Like EverQuest Classic, AND ALSO the world is a deeply-simulated undead political sandbox where factions scheme against each other in real time — even while you sleep — and AI companions are fellow inhabitants of the world with names, histories, and grudges.**

This hook carries three distinguishing claims that each map to a pillar:

1. **The world simulates itself.** Faction AI has autonomous goals and schemes. Log off for a week and you may return to find your patron assassinated, your faction fractured, the political map redrawn.
2. **The dependency is on *named* people, not anonymous slots.** Your cleric is Sister Elara. Your enchanter is Mortis. They have histories, faction allegiances, and opinions of you. Whether a companion is human or AI is a question of circumstance, not hierarchy.
3. **The combat and pacing are fully Classic EQ.** Tab-target, spell memorization, med breaks, careful pulls, group dependency. Not "inspired by." *Is.*

---

## Player Experience Analysis (MDA Framework)

### Target Aesthetics (What the player FEELS)

| Aesthetic | Priority | How We Deliver It |
| ---- | ---- | ---- |
| **Discovery** (exploration, secrets) | **1 (Primary)** | Living faction map, no quest markers, LLM-driven NPC rumors, hidden lore, emergent server history, "what changed while I was gone?" |
| **Fellowship** (social connection) | **2** | Small-server reputation, forced grouping, named AI companions, faction identity, inn-at-night rumor culture |
| **Narrative** (drama, story arc) | **3** | Faction political arcs, named-NPC memory, emergent server-wide stories that are truly unique per server |
| **Challenge** (obstacle course, mastery) | **4** | Classic EQ class depth, hard pulls, permadeath-adjacent stakes, political literacy as a skill |
| **Fantasy** (make-believe, role-playing) | 5 | Gothic undead register, class identity, faction role-play |
| **Sensation** (sensory pleasure) | 6 | Atmospheric audio-forward (silence, ambient dread), painterly/gothic visuals (TBD in `/art-bible`) |
| **Expression** (self-expression, creativity) | 7 | Class build choices, faction path, reputation expression through renown |
| **Submission** (relaxation) | N/A | Not a goal. Gravenspire is not a comfort game. |

### Key Dynamics (Emergent player behaviors)

- Players will **share rumors and clues in guild and inn chat**, trading knowledge about the current faction state the way EQ players traded camp info.
- Players will **form long-term factional loyalties** and develop grudges with rival-faction players and NPCs.
- Players will **return at odd hours to recruit AI companions** when humans aren't online, knowing the roster will be richer off-peak.
- Players will **talk about "what happened on my server" as folklore** — "remember when the Ghoul King got assassinated and the Vampire Court seized the docks?"
- Players will **camp specific spots for weeks** working on faction reputation and rare-spawn farming.
- Players will **hedge political bets** — quietly building reputation with a secondary faction in case their primary patron falls.

### Core Mechanics (Systems we build)

1. **Classic-EQ tab-target combat** — auto-attack + spell memorization + hate management + med breaks + mandatory group composition.
2. **Deep autonomous faction simulation** — AI factions with independent goals, resources, agents, and schemes that run whether players are online or not.
3. **Faction reputation progression** — the long-term ladder; a multi-dimensional standing with each faction; no gear treadmill.
4. **Named AI companion system** — a shared-world pool of specific named NPC adventurers that scales inversely with server population and carries persistent reputation.
5. **LLM-driven NPC dialogue** (scoped to key NPCs at scale; templated elsewhere) — players can *talk* to named NPCs, gather rumors, solicit quests, and have those NPCs remember them.

---

## Player Motivation Profile

### Primary Psychological Needs Served

| Need | How This Game Satisfies It | Strength |
| ---- | ---- | ---- |
| **Autonomy** | Faction choice is meaningful. Betrayal is real and supported. Which camp to work, which patron to cultivate, whether to hedge — all player-driven. No mandatory questline. | **Core** |
| **Competence** | Classic EQ class mastery has genuine depth and takes months to master. Layered on top: political literacy — reading the faction board is its own skill. | **Core** |
| **Relatedness** | Small-server player community (10-50 concurrent). Forced grouping. Named AI companions who remember you. Named NPCs who know you. Every relationship in the game has a name and a history. | **Core (dominant)** |

### Player Type Appeal (Bartle Taxonomy)

- [x] **Achievers** (slow-burn variety) — Faction reputation ladder, class mastery, named-figure-in-server-history goal. Gravenspire is *rich* with achievement tracks but none are gear-score based.
- [x] **Explorers** — *Primary audience.* Living world + no handholding + hidden faction lore + emergent server history is their dream game.
- [x] **Socializers** — *Primary audience.* Small server + forced grouping + named NPC drama + faction identity. Every relationship matters.
- [ ] **Killers / Competitors** — Not the primary target. Faction raiding may provide periodic PvP in later tiers, but the game is not designed around domination.

**Primary archetype:** The **Explorer-Socializer** — a player who wants to *discover and participate in* a living world alongside a small community.

### Flow State Design

- **Onboarding curve:** The hardest design problem. First hour must teach a new player the *value* of EQ slowness (not hide it). Approach: a heavily-curated first-hour tutorial experience with an AI companion mentor (Sister Elara or equivalent) who models the pace. Details designed in dedicated GDD later.
- **Difficulty scaling:** Zone-by-zone, EQ-native. Each zone has a recommended level band. Political complexity scales naturally with reputation tier.
- **Feedback clarity:** XP bar, mana bar, combat feedback, faction rep meters (precise numbers hidden; faceted reputation shown qualitatively — "respected," "trusted inner circle," etc.).
- **Recovery from failure:** Corpse runs + XP loss on death (EQ-native stakes). AI companions can be temporarily injured/unavailable but not permalost. Faction reputation damaged by death is recoverable through play.

---

## Core Loop

### Moment-to-Moment (30 seconds)

Classic EQ combat: auto-attack ticking, hate management, timed spell casts every 6-10 seconds, group coordination in chat. Between pulls: sit, med, regen, re-buff. *The silence between pulls is a feature.*

**Gravenspire twist:** every mob carries faction weight. Killing a vampire servant is a tiny contribution to the political board. Individual kills don't matter; a night's work might.

### Short-Term (5-15 minutes — The Camp)

Camped at a specific spot in a specific zone. Pulls every 1-3 minutes. Camp goals: next rare spawn, clear to the named, fill faction tokens, hit the next XP level. "One more pull" psychology lives here.

**Gravenspire twist:** camps have *shifting faction control*. The mobs here this week are Vampire Court forward agents. Farm hard and the Court retreats; Ghouls move in. Next week: different mobs, different loot, different named spawn.

### Session-Level (60-180 minutes)

1. **Arrive in Gravenspire.** Hit the inn. Talk to NPCs (rumor, gossip). Check the faction board — who's up, who's down.
2. **Talk to your patron.** Faction contact has work. Maybe specific targets. Maybe strategic context.
3. **Travel + camp.** Most of the session. 1-3 hours in the field.
4. **Return to city.** Sell, bank, deposit faction tokens. See the faction board tilt. See if your patron survived.

**Natural stopping point:** the return to the city.

**"Thinking-about-it-offline" hook:** *"My patron is losing ground to the Necromancer Academy. What if they assassinate her before I log back in? Should I hedge and start earning Ghoul rep now?"*

### Long-Term Progression

Three parallel tracks:

| Track | Type | What It Is |
| ---- | ---- | ---- |
| **Class** | Vertical | EQ-classic. Levels, spells, equipment, keyed zones. Your cleric is mechanically better at 40 than at 20. |
| **Faction Reputation** | Horizontal | Rank in your chosen faction(s). Named NPCs know you. Titles, inner circles, political influence. *This is the story of your character.* |
| **Server State** | Meta | The political map of Gravenspire evolves over server weeks. Some of this you cause. Some happens while you sleep. |

**Long-term goal:** Become a **named figure in the server's history**. A personal story arc runs ~6-12 months of play: nobody → faction member → inner circle → power broker.

### Retention Hooks

- **Curiosity:** The world state changes while you're offline. What happened? What's different? What do the rumors say?
- **Investment:** Reputation built over weeks. Characters (human and AI) you care about. A political position on the board.
- **Social:** The small server. Your guild. Your AI companion who has a grudge against your rival's AI companion. The inn culture.
- **Mastery:** EQ class depth. Political literacy. Camp optimization. Economy. Faction strategy.

---

## Game Pillars

### Pillar 1: The World Is Not Your Story

Gravenspire's factions, NPCs, and politics exist **independently** of any player. The world doesn't pause when you log off. The player is a participant in the city's story, not its protagonist.

*Design test:* When debating "should the player feel like the chosen one?" — we choose **no**. Named NPCs outrank named players. The simulation wins ties. There is no Dragonborn.

### Pillar 2: The Silence Is Sacred

EQ-Classic's pacing — med breaks, careful pulls, slow travel, no markers — is preserved as a **feature, not a bug**. Tension and stillness are what make the payoffs matter.

*Design test:* When debating "should we speed this up for modern players?" — we choose **no**. Fast travel, auto-path, map markers, quest logs — rejected unless they *preserve* tension rather than eliminate it.

### Pillar 3: Reputation Is The Progression

The deepest long-term progression in Gravenspire is **faction reputation and political standing**, not item level. Your character is a *name*, not a *number*.

*Design test:* When debating "what's the reward for this activity?" — we choose **reputation and relationship** over **gear**. Gear plateaus early. Rep has no ceiling.

### Pillar 4: Every Companion Is A Person

No player can solo endgame — classes and roles are mutually necessary. The cleric sitting next to you is a **named, persistent, consequential person** whose identity matters. *Whether they are human or AI is a question of circumstance, not hierarchy.*

*Design test:*
- When debating "should class X be able to do Y alone?" — we choose **no — roles are mutually necessary**.
- When debating "should AI companions be easier, more disposable, or less real than humans?" — we choose **no — they're fellow inhabitants of the world, not a utility**.

**Supporting rules baked into this pillar:**
- AI companions are **specific named NPCs** with histories, preferences, reputations, and faction allegiances.
- **Availability scales inversely with server population** — off-peak = packed hiring hall; prime time = sparse hall. The system nudges human grouping when humans are available.
- AI companions can be **injured, temporarily unavailable, or hold grudges** — they won't permadie, but they're not disposable either.
- AI companions carry their own **faction allegiances** — recruiting a Vampire Court cleric for a Ghoul Syndicate raid has consequences for both of you.

### Pillar 5: Stakes Are Honest

Death in Gravenspire carries **real consequences** — corpse runs, XP loss, faction reputation impact. Failure is always possible, but always *legible* and *learnable*.

*Design test:* When debating "should we add a safety net for Y?" — we choose **consequences over convenience**, but we also ensure every death can be explained. Mystery is good; unfairness is not.

### Pillar Tension Map

| Tension | Why It Matters |
| ---- | ---- |
| **P1 ↔ P3** | If rep is *your* progression, how is the world not *your* story? Resolution: the world has its own story; your slice matters *locally*. |
| **P1 ↔ P4** | *Reinforcing.* AI companions are inhabitants of the world with their own lives, supporting P1. |
| **P2 ↔ P5** | Slow pacing + hard death = potentially brutal. Tension forces pacing feel *intentional*, not punishing by accident. |
| **P2 ↔ onboarding** | Modern players will bounce off slow pacing. Tension forces us to teach the *value* of slowness, not hide it. |
| **P4 ↔ dev scope** | Making AI companions this deep is expensive. Tension forces careful MVP scoping. |

### Anti-Pillars (What This Game Is NOT)

- **NOT Skyrim.** We will **not** add main-quest power fantasies that put the player at the center of cosmic events — it would compromise **P1**.
- **NOT modern WoW.** We will **not** add map markers, quest arrows, auto-path, dungeon finders, or convenience features that eliminate friction — they compromise **P2**.
- **NOT a solo RPG with online bolted on.** We will **not** design content optimally experienced alone. AI party fills gaps; it never replaces the social core — that would compromise **P4**.
- **NOT a gear-treadmill MMO.** We will **not** ship raid-tier vertical progression with new gear tiers every content patch — it would compromise **P3**.
- **NOT built for a mass-market audience.** We will **not** soften stakes, pacing, or dependency to broaden appeal. A small, dedicated community is the explicit target; broadening would compromise **P2, P4, and P5 simultaneously**.

---

## Inspiration and References

| Reference | What We Take From It | What We Do Differently | Why It Matters |
| ---- | ---- | ---- | ---- |
| **EverQuest Classic / Project 1999** | Combat feel, pacing, dependency, stakes, class identity, no-handholding | Single city-state instead of whole Norrath; deep faction sim; AI companions; LLM NPC dialogue | Core design DNA. Proven audience still active 25 years later. |
| **Pantheon: Rise of the Fallen** | Audience validation — 15 years of fundraising proves the market | Smaller scope (one city vs. whole world); staged tiers; AI-first design | Shows demand exists *and* serves as cautionary tale about scope. |
| **Vampire: The Masquerade — Bloodlines** | Gothic urban faction politics, tone, named NPC density | Persistent multiplayer; EQ combat instead of action; simulation instead of scripted | Proves gothic faction RPG has a real audience. |
| **Disco Elysium** | NPC dialogue depth, investigation-driven play, city-as-character | MMO with combat instead of single-player detective; LLM-powered instead of hand-written | Proves dialogue-forward RPG works commercially. |
| **Dwarf Fortress / Crusader Kings** | Deep autonomous simulation, emergent history, faction AI | Player-perspective instead of God-view; real-time MMO instead of turn-based | Proves audience appetite for simulation-first design. |
| **Classic WoW / Season of Discovery** | Slow MMO audience validation; forced grouping culture | Smaller server (10-50 vs 1000s); political simulation; AI companions | Shows Blizzard's own data that classic MMO audience is real. |
| **World of Warcraft (modern)** | What *not* to do (convenience creep, dungeon finder) | Explicit inverse | Our anti-pillar case study. |
| **Skyrim** | Open-world exploration feel; single-player respect | Reject Dragonborn power fantasy; persistent multiplayer; dependency-core | What we steal vs. what we reject. |

**Non-game inspirations:**
- **Gothic literature:** Le Fanu's *Carmilla*, Stoker's *Dracula*, Shirley Jackson's *The Haunting of Hill House*, Susanna Clarke's *Piranesi* — tone, atmosphere, dread, the feeling of a place with its own memory.
- **Pre-Raphaelite painting** (Millais, Rossetti) and Caspar David Friedrich — possible visual reference for `/art-bible`.
- **Medieval Italian city-state politics** (Florence, Venice) — faction structures, patronage networks.
- **Small private MUD communities of the late 90s/early 2000s** — the social texture of 50-person online communities where everyone knows everyone.

---

## Target Player Profile

| Attribute | Detail |
| ---- | ---- |
| **Age range** | 25-50 |
| **Gaming experience** | Mid-core to hardcore |
| **Time availability** | 1-2 hour weeknight sessions, 3-5 hour weekend sessions. Plays in bursts, not marathons. |
| **Platform preference** | PC (Windows/Mac) |
| **Current games they play** | Project 1999, Classic WoW / Season of Discovery, Pantheon demos, Vampire: The Masquerade — Bloodlines (still, 20 years later), Disco Elysium, BG3, Dwarf Fortress |
| **What they're looking for** | An MMO where grouping matters, where death has consequences, where their name means something, where the world isn't built around them. They've been waiting for this since 2003. |
| **What would turn them away** | Dungeon finders, raid gear treadmills, "quest markers everywhere," power fantasies, mass-market MMO design, game systems that ignore their time investment |

---

## Technical Considerations

| Consideration | Assessment |
| ---- | ---- |
| **Recommended Engine** | **TBD — defer to `/setup-engine`.** Godot 4.6 is currently pinned in the template. All three major engines (Godot, Unity, Unreal) are viable; the decision depends on MMO netcode approach, AI/LLM integration strategy, and dev background. |
| **Key Technical Challenges** | 1) Dedicated-server MMO netcode for 10-50 concurrent with persistent world state; 2) deep autonomous faction AI simulation; 3) LLM NPC dialogue with coherent memory across sessions and bounded token cost; 4) named AI companion class AI competent enough to fill tank/heal/crowd-control roles in group combat |
| **Art Style** | Gothic atmosphere — stylized 3D or painterly 2.5D; specifics to be decided in `/art-bible`. Reference register: Unrest + Mistmoore + VtM Bloodlines + Pre-Raphaelite painting. |
| **Art Pipeline Complexity** | Medium-High. Style choice in `/art-bible` will determine pipeline investment. Likely: stylized 3D environments + hand-painted character portraits for named NPCs (the portraits are load-bearing). |
| **Audio Needs** | Moderate to Music-heavy. Atmosphere-driven. Silence and ambient dread are design elements. Faction themes. Event audio. Eventually adaptive for political tension shifts. |
| **Networking** | Dedicated server (single rented VPS per server, small player cap). No horizontally scaled MMO infra planned. |
| **Content Volume** | Tier 1 MVP: 1 zone + 1 city hub. Tier 4 full: 3-5 zones + full city + 6 factions + 6-10 classes. Measured by *depth* of faction simulation, not by raw content count. |
| **Procedural Systems** | Faction simulation itself is procedural/emergent. Environments are hand-authored (no procgen zones). NPC dialogue is LLM-generated within authored personality constraints (not fully free-form). |

---

## Risks and Open Questions

### Design Risks

- **EQ-Classic pacing may bounce modern players in the first 15 minutes.** Onboarding that teaches the *value* of slowness is the hardest design problem in the project.
- **Faction simulation may feel chaotic (random events) or static (nothing actually happens).** The sweet spot — "it feels alive and it feels legible" — is narrow and unproven in solo-dev scope.
- **Small-server culture may devolve into cliques or burnout.** The social engineering of the server community is a real design surface, not just a technical concern.
- **Three progression tracks (class / rep / server state) may confuse rather than deepen** if any one is under-delivered.

### Technical Risks

- **MMO netcode as a first-time solo dev is the #1 project-killer.** Mitigation: Tiers 1-2 have no networking; Tier 3 introduces single-VPS dedicated server; no horizontal scaling planned.
- **Deep autonomous faction AI is unproven as a solo-dev system.** Mitigation: Tier 3 starts with reactive simulation; escalate to autonomous only if reactive feels alive.
- **LLM NPC dialogue at scale has real cost and coherence problems in 2026.** Mitigation: templated dialogue default; LLM for ~5-10 key NPCs in Tier 3; full-coverage LLM only in Tier 4 if tech matures or local model on 5090 is viable.
- **Named AI companion class AI that's competent enough to fill group roles is a hard AI problem.** Mitigation: Tier 2 ships with a single companion class (Cleric) before expanding.

### Market Risks

- **Pantheon has been in development ~15 years with a funded team and hasn't shipped.** Both validation and cautionary tale. Mitigation: Tier 1 ships as single-player; we don't need Pantheon's full vision to have shipped something.
- **Small audience (5K-50K global).** Commercial viability depends on staying cheap. Mitigation: passion project framing; if it earns, great, but don't design around revenue.
- **Audience skews older** (25-50). Lower pool of potential players, but typically higher engagement and willingness to pay for a fitting game.

### Scope Risks

- **Full vision is multi-year; first-time dev curve adds 30-50% on top of all estimates.**
- **Feature creep on the AI systems** (every AI idea is a whole subsystem) is a realistic threat. Pillars and anti-pillars are primary mitigation.
- **LLM API cost at Tier 3+ scale is an unknown.** Local model on 5090 is a viable hedge.

### Open Questions (require prototyping or research)

- **Does EQ-Classic combat still feel good in 2026?** Answer via `/prototype combat-feel` — build a single-zone, single-class vertical slice and playtest.
- **Does the "kills shift faction control" bridge feel meaningful at the 5-minute scale?** Prototype at same time as combat feel; add simulated faction state.
- **What's the minimum viable depth of LLM NPC dialogue that feels alive?** Small prototype — 1-3 NPCs, short memory, bounded topic scope — before committing to full system.
- **Can a single rented VPS actually host a 10-50 concurrent persistent server with full faction sim?** Performance prototype in Tier 3 pre-work.
- **What's the first-hour tutorial that teaches EQ pacing as a feature?** Dedicated design sprint in Tier 1.

---

## MVP Definition

**The absolute minimum version that validates the core hypothesis:**

**Core hypothesis:** *"Does Classic-EQ combat still feel good in 2026, and does the 'kills shift faction control' bridge make the political layer feel connected to moment-to-moment play?"*

**Required for MVP (Tier 1 — Vertical Slice, 6-12 months solo):**

1. **One class: Cleric** — the hardest class to build right, because group-dependency systems (heals, buffs, rez mechanics) pivot around it. If the Cleric feels right, the rest of the class design has a template.
2. **One haunt zone** — Unrest-scale haunted mansion. 2-3 floors, 10-15 mob types, 1 named boss, hand-authored layout with real spatial personality.
3. **One faction: Vampire Court** — reputation tracker, ~10 templated NPC dialogues, faction board UI, kill-weight attribution.
4. **Classic-EQ tab-target combat** — auto-attack, mana, hate, spell memorization, med breaks, sitting, corpse runs, XP loss.
5. **Single-player, offline** — no networking at this tier.

**Explicitly NOT in MVP (defer to later tiers):**

- Networking / multiplayer (Tier 2-3)
- AI companions (Tier 2+)
- Additional classes beyond Cleric (Tier 2+)
- Additional factions (Tier 2+)
- Additional zones (Tier 2+)
- LLM-driven dialogue (Tier 3+)
- Deep autonomous faction simulation (Tier 3+)
- Full named AI companion system (Tier 3+)

### Scope Tiers

| Tier | Content | Features | Timeline |
| ---- | ---- | ---- | ---- |
| **Tier 1 — Vertical Slice / MVP** | 1 class (Cleric), 1 haunt zone, 1 faction (Vampire Court), 1 city hub skeleton | Classic-EQ combat, reputation system, basic templated dialogue, single-player offline | 6-12 months (solo, first-time dev) |
| **Tier 2 — Alpha / Co-op** | Add Warrior + Enchanter (holy trinity), peer-to-peer or lobby co-op for 2-6 players | First named AI companion (Sister Elara), templated NPC dialogue, simple rumor system | 12-18 months cumulative |
| **Tier 3 — Beta / Small Persistent Server** | Add 1 more class, 2nd haunt zone (Mistmoore register), 3 factions total (+ Ghoul Syndicate, Living Resistance) | 10-person persistent server on single VPS, reactive faction simulation, 2-3 named AI companions per class, inverse-population scaling, LLM dialogue for 5-10 key NPCs | 18-30 months cumulative |
| **Tier 4 — Full Vision** | 3-5 zones, full city, all 6 factions, 6-10 classes | Deep autonomous faction simulation, LLM dialogue throughout, 50-person server capacity, potentially multiple server communities | 36+ months cumulative, open-ended |

**Shipping philosophy:** every tier is shippable on its own. If life intervenes at Tier 2, Gravenspire exists as a small co-op gothic RPG. At Tier 3, it's a cult-classic small MMO. At Tier 4, it's the full vision. No tier is a stepping stone that's worthless without the next one.

---

## Next Steps

Recommended order. Skipping ahead compounds risk.

- [ ] **`/setup-engine`** — configure the engine and populate version-aware reference docs. (Deferred from this session; you said "help me decide.")
- [ ] **`/art-bible`** — establish visual identity *before* writing GDDs. The art bible gates asset production and shapes technical architecture (rendering, VFX, UI). For Gravenspire: gothic register, named-NPC portrait pipeline, atmospheric lighting direction.
- [ ] **`/design-review design/gdd/game-concept.md`** — validate concept completeness before going downstream.
- [ ] Discuss vision with the **`creative-director` agent** for pillar refinement (deferred under lean mode; optional before Tier 1).
- [ ] **`/map-systems`** — decompose the concept into individual systems (combat, faction simulation, reputation, AI companions, LLM dialogue, networking, etc.), map dependencies, prioritize design order.
- [ ] **`/design-system [system]`** — author per-system GDDs for every system in dependency order. Start with combat + reputation (the Tier 1 load-bearing systems).
- [ ] **`/create-architecture`** — produce the master architecture blueprint and Required ADR list.
- [ ] **`/architecture-decision` (×N)** — one ADR per decision in the Required ADR list.
- [ ] **`/gate-check pre-production`** — phase gate validation before committing to production.
- [ ] **`/prototype combat-feel`** — the highest-leverage prototype. Build a single-zone, single-class combat slice and playtest. This is where we answer "does it still feel good in 2026?"
- [ ] **`/playtest-report`** — structured feedback on the prototype. The core-hypothesis gate.
- [ ] **`/sprint-plan new`** — if prototype validates, plan the first Tier 1 sprint.
