# Art Bible: Gravenspire

## Document Status

- **Version**: 1.0 (complete draft)
- **Created**: 2026-04-22
- **Last Updated**: 2026-04-22
- **Owned By**: art-director
- **Status**: Complete draft — all 9 sections authored
- **Related documents**: [design/gdd/game-concept.md](../gdd/game-concept.md), [.claude/docs/technical-preferences.md](../../.claude/docs/technical-preferences.md), [docs/engine-reference/unity/VERSION.md](../../docs/engine-reference/unity/VERSION.md)

> **Art Director Sign-Off (AD-ART-BIBLE)**: RATIFY WITH NOTES — 2026-05-15.
> The art bible is approved as the visual identity source-of-truth for Gravenspire
> production. Internal consistency, pillar alignment, and forbidden-pattern
> discipline are all production-ready. Ratification carries the following bound
> conditions, to be resolved before `/asset-spec` runs the M3 playable surface:
>
> 1. **Sister Elara tier conflict (S7.5 vs. D003)** — RESOLVED 2026-05-15 by F-05:
>    S7.5 revised to "Tier 1 templated mentor, T2+ full AI-companion." D003/D004
>    stand.
> 2. **T1 M3 scope clarification** — RESOLVED 2026-05-15 by companion file
>    [`design/art/art-bible-t1-scope.md`](art-bible-t1-scope.md) covering
>    postural compression (Court Tier 3+ per S5.5), garment-wear-pattern
>    transfer (T2+ static variants in T1), per-faction Layer 2 (Vampire Court
>    only at M3), and ambient stilling (animator-infrastructure-gated with
>    surface-resolution fallback).
> 3. **Engine coupling — VERDICT 2026-05-15** by `technical-artist` and
>    `unity-shader-specialist` convergent subagent passes:
>    - **(a) Corpse-run per-camera desat (S2 State 7 / S4.4)** —
>      **CONDITIONALLY VERIFIED.** URP Volume Layer Mask is the documented
>      camera-stack isolation mechanism. Single-player path LOW risk;
>      multiplayer per-player isolation deferred to T2 ADR per the bible's
>      own production note. Proof-of-concept in Unity 6.3 LTS required
>      before T1 commits.
>    - **(b) URP SSS cost model (S8.7)** — **RESOLVED-WITH-NOTES 2026-05-16,
>      recommend Option 2.** PoC at `N:\GravenSpire-sss-poc\`; verdict
>      report at [`tests/evidence/SSS-POC/sss-poc-2026-05-16-verification.md`](../../tests/evidence/SSS-POC/sss-poc-2026-05-16-verification.md).
>      Option 1 (custom URP screen-space SSS) is implementable but URP-forward
>      composites diffuse + specular per fragment, making screen-space SSS
>      bleed specular into the scatter — structural problem, not tuning.
>      Option 2 (per-material Shader Graph pre-integrated skin LUT) recommended
>      for named-NPC skin. §S8.6 named-NPC budget needs per-draw SSS cost revision.
>    - **(c) Mipmap bias for 30m faction-silhouette legibility (S6.2 / S8.5)**
>      — **UNVERIFIED, hardware-dependent.** `-0.5` bias is reasonable as
>      starting value; validate against the locked hardware spec before
>      pipeline commits.
>    - **`/asset-spec` gating:** environment + ambient-NPC asset specs may
>      proceed. Skin shaders / named-NPC material slot counts **UNBLOCKED
>      2026-05-16** against Option 2 working assumption. See companion
>      [`art-bible-t1-scope.md`](art-bible-t1-scope.md) SSS section and
>      [`tests/evidence/SSS-POC/sss-poc-2026-05-16-verification.md`](../../tests/evidence/SSS-POC/sss-poc-2026-05-16-verification.md).
> 4. **Editorial reconciliation** — RESOLVED 2026-05-15 by F-02 (S2 State 3
>    cross-referenced to D012 combat-feel validation) and F-03 (S3.5 tension 3
>    superseded by S4.4 HUD-exempt resolution).
> 5. **Hardware-target governance drift (F-09)** — IDENTIFIED 2026-05-15.
>    Bible S8.4 (line 1510) and S8.6 (line 1539) assert polygon budgets are
>    "tech-validated against Unity 6.3 URP + GTX 1070 min-spec / RTX 4070+
>    target / 1080p/60fps" but `.claude/docs/technical-preferences.md`
>    performance section states all targets are `[TO BE CONFIGURED]`.
>    Technical Director + producer decision required: either lock the hardware
>    spec in technical-preferences.md and profile to back the bible's claim,
>    or soften the bible's "tech-validated" language to "estimated." Either
>    path needs a new D-entry in `DECISIONS.md`. Tracked as carryover
>    `art_bible_hardware_target_drift` in `production/sprint-status.yaml`
>    (added separately at next /story-done or sprint-status sync).
>
> All other sections (S1 Visual Identity Statement, S3 Shape Language, S4 Color
> System, S5 Character Design, S6 Environment, S8 Asset Standards within bound
> conditions, S9 Reference Direction) are production-ready as written.
>
> Ratification subject to review trigger: any future revision of pillars
> (`design/gdd/game-concept.md`), tier strategy (`DECISIONS.md`), or render
> pipeline (`DECISIONS.md` D001) requires a fresh AD-ART-BIBLE pass.

> **Technical-artist validation flags** (accumulated across sections; require resolution before or during Tier 1 prototype):
> - S2 State 7: per-camera desat pass for corpse run — URP camera-stacking isolation
> - S2 State 8: ambient NPC stilling animation — animator state-machine coordination
> - S5 postural compression: rigging system for per-faction idle blend tree (MVP: Court only at Tier 3+)
> - S5 garment-wear-pattern transfer: dynamic texture modification system (may need to start as static variants)
> - S6.2 mipmap behavior for 1024² exterior facades at 30m silhouette-legibility range
> - S6.2 URP Decal Projector perf budget (confirmed: projectors interior-only; exteriors bake history)
> - S7.9 mana-restore 1:1 fill depends on med-break mechanics (not yet specced)
> - S8 Unity 6.3 LTS API verification (SSS cost model, GPU instancing, occlusion culling bake) against engine-reference docs
> - S8.9 unity-addressables-specialist: per-zone texture streaming groups, ≤350MB resident target

---

## Section 1 — Visual Identity Statement

### The One-Line Visual Rule

> **"Every visual element earns its place through weight and age, not spectacle."**

**Why this phrasing:** It targets the exact failure mode of gothic games — mistaking ornamentation for atmosphere. "Weight and age" is *operational*: an artist can ask "does this object look like it has been here for a hundred years and carries that history in its surfaces?" and get a yes or no. It excludes glowing runes, particle-density VFX, and hero-lighting drama. The word *earns* puts the burden on the artist to justify presence.

### Supporting Principles

#### Principle 1 — Stillness Is The Signal
*Serves pillar: **The Silence Is Sacred** (P2)*

**Definition:** Atmosphere is communicated through what does NOT move — static light, resting shadow, held poses — rather than through animation, particle density, or environmental loops.

**Design test:** When a scene feels tonally flat, add darkness and structural shadow before adding any moving element. When a VFX pass is proposed, cut it by half before reviewing.

**Why this protects the pillar:** EQ Classic's pacing worked because the world did not perform for the player. A torch that flickers on a physics simulation, a banner that loops a wind cycle, a mist cloud that pulses — each one whispers "this world is attending to you." Gravenspire's world does not attend to the player. Stillness communicates indifference, and indifference is where dread lives.

#### Principle 2 — Faction Before Fantasy
*Serves pillar: **Reputation Is The Progression** (P3)*

**Definition:** Every character and environment communicates its factional allegiance through material, color, and silhouette *before* it communicates anything about power level or lore significance.

**Design test:** When an NPC visual is ambiguous, choose the reading that locates them socially within the city rather than the reading that makes them visually impressive as a solo design. A Vampire Court attendant should read Vampire Court before they read "powerful undead."

**Why this protects the pillar:** If gear-level is not the progression system, then visual power-scaling (larger, more elaborate, more luminous) becomes actively misleading. Players who learn to read factions instead of power tiers are playing the correct game. The visual system must train the correct literacy. This means the art department must resist the pull toward making "boss" characters look like bosses — a Ghoul Syndicate fence with no combat role should still read louder as Syndicate than as combatant.

#### Principle 3 — The Beautiful And The Wrong
*Serves pillar: **The World Is Not Your Story** (P1)*

**Definition:** Gravenspire is beautiful in the way that a sleeping face is beautiful — composed, specific, and carrying a suggestion that something is not right. Nothing should look grotesque; nothing should look unambiguously safe.

**Design test:** A character or environment design should produce mild unease on *second* look, not the first. If it reads as threatening immediately, pull back. If it reads as purely decorative, add one element that doesn't settle.

**Why this protects the pillar:** The standard gothic game signals threat through monstrous aesthetics: the villain has a skull motif, the dangerous zone is red and black, the enemy reads hostile from across the room. This is player-service aesthetics — the world decorates itself to warn you. Gravenspire's world does not warn you. The Vampire Court is genuinely beautiful. The Pale King's cathedral is genuinely devotional. The wrongness is subtle and requires attention — which is exactly the attention the simulation rewards. Atmosphere-as-warning is the visual equivalent of quest markers.

### Productive Tensions Between Principles

- **Stillness ↔ Beautiful-And-Wrong.** Stillness produces unease through *absence*; Beautiful-And-Wrong demands a specific *present* element that doesn't settle. A chapel interior: leave the candles unlit (stillness) or include the half-burned candle that shouldn't still be lit (beautiful-and-wrong)? Usually both — the tension forces the question.
- **Faction Before Fantasy ↔ the Weight-and-Age rule.** A Vampire Court costume covered in insignia is obviously factional, but the rule also demands the insignia itself has earned its place through age — no crisp heraldry, no clean livery. Faction read must come from silhouette and material, not from a freshly-pressed badge.

### What This Rule FORBIDS (the teeth check)

- **Glowing loot and rarity colors** — the immediate-readability shortcut every RPG uses. In Gravenspire, item value is social and contextual, not visual. A ring does not glow gold because it's rare; it looks like it belongs to someone specific.
- **Particle-dense ambient VFX** — floating motes, magical shimmer, drifting soul-light. These are the visual equivalent of ambient music that tells you how to feel. Mist in a haunt behaves like mist, not like a mood system.
- **High-contrast hero silhouettes for player characters** — the temptation to make player characters read as protagonists (broader shoulders, better-lit than the world, agency-communicating posture). The world is not their story; the art refuses to grant them visual priority.
- **Clean surfaces on anything old** — stone that looks freshly textured, wood grain that reads as an asset, cloth with no weight. The gothic shortcut is to add a dark color overlay and call it aged. Weight-and-age means texture budget goes into surface *history*, not surface complexity.
- **Jump-cut lighting between zones** — the Bloodborne instinct: each zone has its own color grade applied like a filter. Gravenspire's factions bleed into each other geographically and politically; color language shifts gradually through material and light-source logic, not through a post-process override at a loading seam.
- **Skeletal / bone-motif decoration as generic "undead" signaling** — skulls on the architecture, bones in the trim. This collapses factional distinction — if everything in the undead city uses death-iconography uniformly, the Vampire Court and the Ghoul Syndicate look like the same faction with different hats. Each faction's death-relationship is specific; the visual language must honor that specificity.

---

## Section 2 — Mood & Atmosphere

### Atmospheric Philosophy

Gravenspire does not perform atmosphere. Where most gothic games announce their tone through constant environmental theater — drifting mist, pulsing shadows, orchestrated dread — Gravenspire's atmosphere is made of **intervals**: the silence between a bell toll and its echo, the stillness of a room that was recently occupied. Every game state shares one underlying quality: things here have been happening for a long time without you, and will continue happening after you leave. The emotional register ranges from contemplative to suspended but never tips into spectacle. The player is always a witness, never the subject.

### The Nine Game States

#### State 1 — Exploration

- **Primary emotion:** The alertness of moving through a familiar city at an hour when you should not be there — not fear, but a heightened proprioception of where your feet land.
- **Lighting character:** Overcast ambient daylight — flat ~6000K diffuse sky dome, shadows compressed to ~1:2 contrast. No hard directional sun. Practical warm sources (oil lanterns, hearth-glow through doorways) at 2200-2600K assert weakly but specifically. The world is lit from everywhere and nowhere; shadows exist without specific owners.
- **Palette skew:** Desaturated stone gray dominant. Muted umber/oxide brown in trim and woodwork. Faction color appears only in small material accents (a door in Vampire Court slate-violet, a Syndicate post stained ochre). Absent: true black, true white, any saturated warm hue.
- **Atmospheric descriptors:** Pewter-weighted, architecturally dense, unhurried, inhabited without being populated, self-contained.
- **Energy level:** Contemplative. The player is reading the city between destinations. Nothing demands focus; many things reward lateral attention.
- **The ONE visual element that carries this mood:** **Wear patterns on stone** — high-traffic cobblestone polished smooth at the center of each stone by centuries of foot traffic; low-traffic alleys show moss encroachment at the grout lines. The city's social geography is written in surfaces, not signage.
- **Distinguished from neighbors:** The Camp is quieter and enclosed (small field, intimate low-light). City Hub Day shares the diffuse exterior light but is populated and socially legible. Exploration is threshold space — reading, not yet engaged.
- **Pillar alignment:** P1 (The World Is Not Your Story) — the world is legible through attention, not prompts.

#### State 2 — The Camp (Med Break)

- **Primary emotion:** The specific ease of a rest that was earned rather than chosen — setting down a weight you had forgotten you were carrying, among people who have set down the same one.
- **Lighting character:** A single practical source owned by the group — campfire, lantern, candle stub. 2000-2400K. Light radius is tight; faces of the party are lit; the zone beyond is indeterminate (shapes present but unresolved). Contrast ~1:3 inside the lit circle. Ambient fill beyond is a cool 4800-5200K scattered light suggesting the larger world without revealing it. Total illuminated area is the radius of a careful throw.
- **Palette skew:** Warm amber of the source against a cool-dark surround resolving toward near-black blue-gray. Party faces and equipment read within the warm band; faction colors present but low-key. Absent: any competing light source. The outside world does not offer light back.
- **Atmospheric descriptors:** Gathered, provisionally warm, small-radius, patient, wick-lit.
- **Energy level:** Suspended. Med break is one of the few moments in any game where designed inactivity is the correct state. No ambient animation. No distant events to watch. The camp circle is complete.
- **The ONE visual element that carries this mood:** **Multiple overlapping soft shadows of seated figures** cast on the wall or ground behind the party. No single shadow dominates. The visual grammar of gathered people reads as warmth and fragility simultaneously — one step back into the dark and these shadows vanish.
- **Distinguished from neighbors:** Combat is the direct neighbor. The camp is closed, warm, contained. Combat is open, cool, directional. The shift in temperature and shadow direction must communicate that rest is over before the first spell fires.
- **Pillar alignment:** P2 (The Silence Is Sacred) — the camp **is** the silence.

#### State 3 — The Pull / Combat

- **Primary emotion:** The focused narrowing of attention in the first seconds of a confrontation you cannot leave — not panic, but the particular clarity of knowing your decisions now will determine whether you get to rest again.
- **Lighting character:** The ambient environment does not change. No combat-state post-process, no desaturation, no red vignette. What changes is focal: spell VFX introduce brief localized light events at 3200-4500K (cool fire, not warm), very short duration (<0.4 seconds at full intensity, rapid falloff). Enemy targets may carry weak practical light sources that become visually relevant once the pull is active. No global lighting state change.
- **Palette skew:** The environment's existing gray-brown. Spell effects skew desaturated-cool (ice, shadow, bone-white). The only warm color in combat is biological (blood, wound) — handled per biological accuracy, not stylization. Magic in Gravenspire costs something; it should not look like a celebration.
- **Atmospheric descriptors:** Compressed, consequential, fractionally brighter at specific points, staccato, load-bearing.
- **Energy level:** Kinetic, but slow kinetic — this is EQ tab-target. The player's attention is on the hate list and the spell queue, not spectacle. Kinetic here means stakes are live, not that the camera shakes.
- **The ONE visual element that carries this mood:** **The pivot** — the moment an enemy mesh rotates from facing-away (set dressing) to facing-toward (encounter). No particle effect signals this. The mesh rotation is the signal.
- **Distinguished from neighbors:** The Camp is the direct predecessor — the pull-begin moment ends the camp's enclosed warmth. The camp-to-combat transition is deliberately asymmetric: camp is slow and warm; combat introduces cool, brief, localized light events with no global warmup.
- **Pillar alignment:** P2 (The Silence Is Sacred) — combat earns its disruption of the silence rather than filling the world with it by default.

> **Production note (flagged by art-director):** This state is deliberately under-powered visually. If it reads as thin in playtest, the approved tension-break is to add **enemy stance/silhouette shift** (idle → combat-ready) as an animation-layer behavior, NOT a lighting change or VFX addition. Defer unless playtesting shows need.
>
> **Cross-reference D012 (Combat Feel Prototype validated 2026-04-26):** The Cleric's Attack-ON visual state and tactical instant abilities validated in D012 are **inside this State 3 envelope**, not outside it. D012's brief localized impact events on Smite cast and Attack-ON toggle (3200-4500K cool, <0.4s duration, mesh+localized-light pairing — not particle systems) match the lighting character specified above. Combat does not need additional VFX to feel kinetic; the validated tactical instants are sufficient. Resolved 2026-05-15 by AD-ART-BIBLE sign-off pass finding F-02.

#### State 4 — City Hub (Day Cycle — "Inn Hours")

- **Primary emotion:** The negotiated ease of a public space where everyone's allegiances are known but not activated — the careful sociability of a city where every stranger is someone's agent.
- **Lighting character:** Flat overcast exterior as in Exploration, but interior hub spaces have functional daytime lighting: hearth fires, oil lanterns in windows, practical warm sources of a working space at 2600-3200K medium intensity, competing against the 6000K ambient coming through windows. Net interior quality is **working warmth** — not intimate, not theatrical. Shadows are medium-contrast (~1:2.5) and specific: you can see whose shadow this is.
- **Palette skew:** Stone gray base with architectural warmth from wood tones and open flame. Faction colors are socially present — NPCs wear their allegiance, boards bear faction insignia, doors carry colors. But worn, aged, quiet. No hue at full saturation. The palette is a room repainted several times without being stripped first; every layer shows through.
- **Atmospheric descriptors:** Politically textured, inhabited, negotiated, warm-lit-but-cool-in-the-corners, legible.
- **Energy level:** Measured. Social navigation is the activity; the environment supports reading NPCs and space rather than experiencing it aesthetically.
- **The ONE visual element that carries this mood:** **The faction board itself** — not as UI, but as a physical object. Corkboard, notice wall, stone ledge heavy with papers and seals. Wear of paper, crookedness of pins, visible handwriting. The presence of recent activity on a surface showing old activity underneath.
- **Distinguished from neighbors:** Exploration is the same exterior light without interior warmth. Night Cycle shares the space but inverts the logic — interior sources now dominate rather than compete.
- **Pillar alignment:** P3 (Reputation Is The Progression) — the hub is where the political game is played; the visual environment must make faction legibility easy without making it simple.

#### State 5 — City Hub (Night Cycle — "Court Hours")

- **Primary emotion:** The sensation of being admitted somewhere you were not expected, into a gathering that has been in progress longer than your lifetime — not menace, but the quality of being younger than the room.
- **Lighting character:** All exterior ambient is gone. Interior practical sources now own the space completely: candelabras, wall sconces, a fireplace operating as theatrical centerpiece rather than heat source. Temperature 2200-2600K throughout. No ambient fill from sky. Exterior windows are black mirrors. Shadow density is high (~1:4 contrast), but warm source coverage means faces are well-lit *where people choose to stand near light* — the choice of where to stand is a social act. Unlit areas of the room exist and have depth.
- **Palette skew:** Deep warm amber dominant. Grays and browns recede into unified dark. Faction colors worn by higher-status NPCs (who attend court hours) are now the only chromatic variety — but they glow slightly in firelight in a way that reads as material (silk, polished leather, worked metal), not emission. Absent: any cool hue. Moonlight, if any, is blocked by curtains drawn by someone who made that choice.
- **Atmospheric descriptors:** Candle-thick, deliberate, older-than-present, close-aired, structurally warm but socially cold.
- **Energy level:** Contemplative with an undertow of measured. The Vampire Court's actual members attend during court hours; what was a working space by day is now a hall of political weight.
- **The ONE visual element that carries this mood:** **Candlelight reflected in the eyes and small jewelry of NPCs who are watching the player.** Not particle effects — actual specular highlights on mesh surfaces moving with the candle simulation. The room is full of small lights that are watching.
- **Distinguished from neighbors:** Day Cycle is about the competition between interior and exterior light. Night is sealed — interior is the whole world. The Camp is also warm and intimate, but small and provisional. Court hours is ancient and architecturally intentional — safety built by exhausted people vs. space built to contain power.
- **Pillar alignment:** P1 (The World Is Not Your Story) — the strongest expression of a world that predates the player and does not require them.

#### State 6 — The Haunt Interior

- **Primary emotion:** The absorption of someone reading a very old letter — the certainty that you are receiving information meant for someone who no longer exists to receive it.
- **Lighting character:** No ambient sky fill. Every source is practical, architectural, aged: wall sconces burning low, a chandelier with two-thirds of its candles unlit, daylight through shuttered or boarded windows as thin blades of 6500K cool white at 10-20° angles — these cutting lines of dusty light are the only directional source. Warm practicals at 1800-2200K (dying fire, nearly-depleted candle). Total ambient very low; scene lives in ~1:5 contrast range. The difference between lit sconce and unlit hallway is navigable but not dramatic — eyes adjust.
- **Palette skew:** The stone-and-bone interior coloration of the faction that owns this haunt — not generic dungeon brown, but architecturally specific. A Vampire Court haunt reads through polished black marble and tarnished silver fixtures differently than a Ghoul Syndicate undercrypt (crude excavated stone, stained timber, iron hinges). Absent: any warm ambient fill. The warmth has been local and dying for a long time.
- **Atmospheric descriptors:** Architecturally specific, historically inhabited, cool-with-dead-warmth, compressed-vertical, specific.
- **Energy level:** Contemplative with sudden kinetic interruptions — EQ haunt pacing. Long corridors of quiet attention; the encounter begins without warning.
- **The ONE visual element that carries this mood:** **The blade of dusty exterior light cutting across a room lit by dying interior sources** — light that has traveled a long way and arrived somewhere it was not expected. The dust particles it reveals are the only animation in the room, and they are too slow to read as alive.
- **Distinguished from neighbors:** The Camp is warm and bounded by party presence; Haunt Interior is cool and bounded by architecture. Combat shares the environment but adds directed enemy attention. Corpse Run is the same space with practicals further reduced — the haunt seen without the fiction of warmth.
- **Pillar alignment:** P1 (The World Is Not Your Story) — the haunt is a history, not a level.

#### State 7 — Death & Corpse Run

- **Primary emotion:** The particular self-knowledge of having overestimated yourself — not despair, but the steady, corrective weight of consequence meeting intention on an open road.
- **Lighting character:** Death state inverts the scarcity logic. The player returns to the world as a spirit or penalized state; world's light sources no longer apply warmth to them. **Per-player camera only:** practicals present but desaturated — the 2400K campfire is perceived by the returning player as ~4000K cool white, global saturation reduced ~-40% on a post-process layer that applies ONLY to this player's camera. The world is the same; the player's relationship to it has shifted.
- **Palette skew:** Existing environment palette pulled toward gray-blue. Warm sources lose their warmth. Color exists but is muted, seen through a scrim of exhaustion. Absent: any palette warmth for the player. They are temporarily outside the hospitality of light.
- **Atmospheric descriptors:** Gray-witnessed, continuous-but-changed, corrective, patient, unaccompanied.
- **Energy level:** Measured — the corpse run is a walk. Not frenetic, not contemplative (too much at stake for full contemplation), but the specific measured pace of someone doing what they did not want to have to do. The environment does not rush them. It has seen this before.
- **The ONE visual element that carries this mood:** **Your corpse visible from a distance before you reach it** — a still, specific object in the middle of a space you navigated alive. Your equipment visible on it. Visual confirmation that the death was real and the world holds the record.
- **Distinguished from neighbors:** Haunt Interior shares the space, but in normal state the player has warmth and belonging; in corpse run state they are a desaturated witness. The transition is not a screen fade — a persistent visual register shift applied to the dead player's camera only. Groupmates see the world normally.
- **Pillar alignment:** P2 (The Silence Is Sacred) — death has no music sting, no red screen, no UI drama. The punishment is delivered quietly, which is worse.

> **Production note (flagged by art-director):** Per-camera isolation of the desaturation pass in URP for a multiplayer context needs technical-artist confirmation. Flagged for `/design-system` networking or a later ADR.

#### State 8 — Rare Spawn / Named Boss Appearance

- **Primary emotion:** The specific attention of a room going quiet when someone enters who everyone already knows — recognition without celebration, significance without fanfare.
- **Lighting character:** No new light source added. No environmental change triggered. The rare spawn or named entity walks or manifests within the existing environmental lighting. What changes is behavioral: the entity's mesh occupies light differently than standard enemies — higher surface quality means specular response is more specific, ambient occlusion is denser at joints and face structure. The entity is lit by the same lights as everything else; it simply responds to those lights as if it has more physical substance. Presence through physical weight, not through emission.
- **Palette skew:** No color shift. The named entity may carry faction colors at higher material fidelity — older, more specific, more historically convincing surfaces — but they are not chromatically elevated above the world. The Vampire Court ancient in her silver-black gown reads louder not because she is brighter but because every surface on her is more resolved.
- **Atmospheric descriptors:** Weight-concentrated, unhurried, specific, self-contained, historically dense.
- **Energy level:** Suspended — the moment a named appears, the pull has not yet begun. This is the suspended breath before the decision to engage. The visual environment does not accelerate; it remains exactly what it was. Significance is carried by the entity, not by the world's reaction to it.
- **The ONE visual element that carries this mood:** **Ambient non-combat entities briefly stilling** — set-dressing creatures, ambient NPCs, background figures — briefly cease their idle animation loops when the named entity is within a certain radius. Not staring at it. Simply stilling. The world has noticed, and the only sign is the cessation of motion that was previously unremarkable.
- **Distinguished from neighbors:** Combat is what this becomes once the pull initiates — the named-appearance moment is pre-pull and communicates significance without combat cues. Haunt Interior is the ambient context; the named appearance is the interruption of that ambient by something specific.
- **Pillar alignment:** P1 (The World Is Not Your Story) — the world does not celebrate the appearance of a notable entity for the player's benefit. The world responds the way the world would respond.

> **Production note (flagged by art-director — preserve this argument):** This state will be contested in production by stakeholders who want a bigger dopamine signal for rare spawns. The pillar-alignment argument is deliberately baked in here as a gate: the entire premise of Gravenspire is that the world does not perform for the player. Adding glow, sting, or VFX to the rare-spawn moment would violate P1 at the most visible point. If future production pressure pushes against this, the approved tension-break is to add **weight** (even higher surface resolution, denser shadow response), never emission or celebration.

#### State 9 — Faction Board / Political Inflection

- **Primary emotion:** The disorienting recognition that the map has changed while you were sleeping — not alarm, but the settled comprehension of consequence arriving from a direction you were watching but could not fully anticipate.
- **Lighting character:** This is a UI state, but it lives in the world. The faction board physical object becomes the focal point: a localized narrow spot from an overhead practical source (400-600K warmer than ambient, very tight radius, 30-40° cone) illuminates the board without illuminating the space around it. A **reading-light**, not a dramatic beam. The player's environment does not change; the board's legibility increases slightly. Should read as "someone turned up the lamp over the notice board," not "the UI is activating."
- **Palette skew:** The board's content carries the palette shift. When a major political event fires — border change, assassination, war declaration — the new notice is on different paper than the old ones. Not white, not red: the specific color of the dominant faction's communications. Vampire Court dispatches are written on gray-blue vellum with a specific seal. Ghoul Syndicate notices are on rough parchment the color of old wax. The chromatic shift is diegetic: different organizations use different materials.
- **Atmospheric descriptors:** Document-weighted, specific, consequence-adjacent, irreversible-feeling, socially legible.
- **Energy level:** Measured, with an undertow. The visual register is quiet; the stakes are the highest in the game (server-wide political events are Gravenspire's macro-progression). The restraint of the visual treatment is proportional to the permanence of what's being communicated.
- **The ONE visual element that carries this mood:** **The new notice on faction-specific material, slightly less worn than the layers beneath it** — fresh but not crisp, as if posted this morning rather than new from the press. The handwriting or seal is faction-specific. The information is the environment; the environment is only its container.
- **Distinguished from neighbors:** City Hub Day is the ambient context; the political inflection is when that context acquires a specific, irreversible event. City Hub Night shares the board but the lamp-over-board effect reads differently against the candlelit dark — it becomes even more focused because the surround is lower.
- **Pillar alignment:** P3 (Reputation Is The Progression) — the faction board is the game's macro-progression system made visible. Its visual treatment must communicate consequence and permanence without celebration.

### Cohesion DNA — what runs through every state

The single visual DNA of Gravenspire is this: **light is always practical, localized, and earned**. It comes from objects that exist in the world for reasons the world has, not for reasons the player has. A campfire gives light because fire gives light. A chandelier is unlit in two-thirds of its arms because no one has tended it recently. A spotlight does not exist because a designer decided this moment needs emphasis. Every transition between states is a change in **which practical sources are present and how they behave**, not a change in the world's fundamental relationship to light. The player moves through a world that has been lit this way for centuries; they do not carry their own light with them, and the world does not adjust its light for their arrival.

Cohesion in Gravenspire is not a visual style — it is a physical logic applied without exception.

---

## Section 3 — Shape Language

### Shape Philosophy (overall)

Gravenspire's shapes are the shapes of things that have been under pressure for a long time. Not broken — settled. The architectural vocabulary is load-bearing verticality compressed by centuries into horizontal spread: arches that have flattened, towers that have thickened at the base, doorframes that have shifted into slight trapezoids. Character silhouettes communicate social position through the cumulative weight of what they carry — layers of garment, tools of role, the posture of someone who has been doing this specific thing for a very long time. Nothing is designed to be legible at a glance to a stranger; everything is designed to be legible to someone who has learned to read it. Shape in Gravenspire is a literacy, not a signal.

### 3.1 — Character Silhouettes

**Philosophy.** A character in Gravenspire reads as a social position before it reads as a power level, and as a faction role before it reads as a named individual. Silhouette is the earliest information a player receives at distance. The vocabulary is built around occupation and accumulated material, not heroic anatomy.

**Readability target (hard constraint).** Faction role must be identifiable at **20-30m in-world distance (~80-120px at 1080p)** on the default third-person camera, under overcast ambient conditions with no compensating hero lighting. Named individuals require proximity (<10m / <50px) and cannot rely on silhouette alone — they require material and face. **If a silhouette is not faction-role-legible at 80px without lighting aids, the design has failed.** Test every character design at this pixel height before approving.

**Hierarchy.** Silhouette alone communicates, in descending order: Faction > occupational role within faction > approximate age/experience weight. Silhouette does **not** communicate: threat level, power tier, named-individual identity, player vs. NPC, or alive-vs-undead status. Those distinctions are carried by material, movement, and proximity.

**Signature faction silhouette shapes:**

- **Vampire Court** — formal vertical silhouette. High flat-topped collar geometry (specific trapezoid, not ruffled edge). Layered garment hem creating horizontal line at mid-calf. Controlled arm position with elbows close to body.
- **Ghoul Syndicate** — horizontal-weighted asymmetry. Tool or satchel mass on one side. Uneven layer count (one shoulder heavier). Lower center of gravity in overall shape.
- **Necromancer Academy** — gathered-vertical shape of academic dress with distinctive forward head-lean created by a weighted hood (not decorative — the hood is weighted and reads as such).
- **Cult of the Pale King** — deliberate restriction. Bound-silhouette shapes. Limb-close posture. Wrapped rather than draped fabric creating narrow, compressed verticality.
- **Haunt Collective** — dissolution-at-edges. Layered translucent material creating an indeterminate outer silhouette; the character's boundary is not a clean line.
- **Living Resistance** — improvised horizontal mass. Strapped-on gear, non-uniform layer count, shapes that do not belong to any single costume tradition.

The player character's silhouette obeys the same rules as NPC silhouettes. The engine must not apply any protagonist framing, hero-lighting rim, or silhouette-enhancement shader to player characters that does not equally apply to equivalently-dressed NPCs.

**Forbidden silhouette shapes:**

- No character silhouette exceeds **1.8× shoulder-width** of an equivalent-height human figure in idle pose.
- No weapon extends more than **0.4m beyond body silhouette** in resting carry. Weapons in active combat are exempt but must return to resting silhouette between engagements.
- No decorative cape, cloak, or mantle may be rigged to billow, flare, or create a heroic silhouette in idle or walk animation. Fabric falls under gravity with realistic lag.
- No isolated apex landmark element (single horn, spike, towering headpiece) on non-named characters. Landmark silhouette elements are reserved for named, historically significant NPCs and are documented per-character in that character's design note.
- No faction insignia placed at a size readable at the 80px silhouette target. Faction reads through **shape and proportion**, not through logo.

**Emotional communication.** The player learns to read a city where nothing is labeled. Characters are distinguished by shape before name. A thirty-hour player reads silhouettes the way a resident reads faces; a new player reads them the way a tourist reads a city. Both experiences are valid; neither is cheated.

**Pillar anchor.** Principle 2 (Faction Before Fantasy) → Pillar 3 (Reputation Is The Progression). Silhouette trains the player to read the political geography as a visual literacy — the precondition for engaging with faction reputation as a progression system.

### 3.2 — Environment Geometry

**Philosophy.** Medieval Italian city-state vertical compression — streets 3:1+ height-to-width ratio, buildings sharing walls and load with neighbors — plus **400 years of additional settling**. Streets are not narrow by plan but by accumulation. Buildings have grown additional stories and shed them; cantilevered additions encroach from both sides. The emotional register is **enclosure without claustrophobia** — a city older than anyone who could have designed it.

**Architectural stance.** Primarily pointed-arch gothic over round-arch Romanesque, but crucially: **the pointed arches have been filled, modified, subdivided, and repaired with Romanesque round arches** where the pointed arch cracked or the lintel was replaced. The city does not have a unified architectural period — it has the geometry of a place maintained and modified by whoever was in charge across 400 years, which means competing geometric vocabularies coexisting. The art department's job is not to design consistent gothic architecture but to design architecture that looks like different centuries, different factions, made to coexist.

**Scale specifications:**

| Element | Value |
|---|---|
| Player character eye height | 1.65m |
| Interior ground-floor ceiling | 3.8-4.2m (generous/institutional) |
| Upper floors | 3.0-3.2m (added later, lower priority) |
| Main street width | 6-8m |
| Secondary street | 3.5-5m |
| Alley | 1.8-2.5m (two people can pass but not comfortably) |
| Building heights | 3-6 stories, averaging 4 |

Sky visible at the end of a street, rarely from the middle. The geometry communicates the world predates the player by doing the opposite of modern game environments: instead of vast scale to make the player feel small, Gravenspire uses **dense, accumulated, indifferent geometry** to make the player feel recent.

**Wear and settling specifications:**

- Stone walls bow outward at mid-height under floor weight — **max visible bow 4-8cm at 3m span**. Perceptible, not structural-failure-level.
- Floors slope toward drainage points — **0.5-1.5°** — enough to notice on marble/rolling objects, not enough to feel drunk.
- Doorframes racked out of square by **1-3°**. Doors that once fit perfectly now stick or gap.
- Stair treads worn at center — **8-12mm below edge at nosing** from foot traffic.
- **Wear is directional and tells history.** High-traffic = smoother; low-traffic = moss, lichen, settled-dust in corners. The geometry is a social document.

**Per-faction geometry:**

- **Vampire Court** — highest ceilings, most regular geometry, most vertical and most resolved, but most frozen (hasn't changed in 200 years and knows it).
- **Ghoul Syndicate** — most accumulated. Original building geometry overlaid with additions, subtractions, tunneled passages. Horizontal-weighted (low ceilings, wide spans, ground-level access).
- **Necromancer Academy** — formally religious architecture adapted for academic use. Nave-scale spaces subdivided with mezzanine additions that break original verticality into working floors.
- **Cult of the Pale King** — oldest architectural strata. Pre-city structures, foundations-level spaces. Pre-gothic crude heavy massive stonework.
- **Haunt Collective** — interstitial architecture between factions. Never fully claimed, never fully maintained. Highest delta from ideal-form.
- **Living Resistance** — repurposed non-monumental architecture. Tradespeople's buildings, lower-district residential, warehousing. Designed for function, modified for concealment.

**Emotional communication.** The pleasure is archaeological — reading layers of history in a surface. The dread is the same: this city's history is very long; the player's part is very short.

**Pillar anchor.** Principle 3 (The Beautiful And The Wrong) → Pillar 1 (The World Is Not Your Story).

### 3.3 — UI Shape Grammar

**Decision: UI echoes the world, but not fully.** A fully diegetic UI fails accessibility — a player reading a shrinking mana bar during corpse run cannot parse a handwritten scroll. The abstract UI layer is **a concession to legibility, not a design choice to be celebrated.** It is kept minimal, low-saturation, architecturally humble.

**Two-layer approach:**

| Layer | Content | Treatment |
|---|---|---|
| **Layer 1 — Practical HUD** | Health, mana, hate, spell queue, combat timers | Abstract, geometric, peripheral. Time-critical information only. |
| **Layer 2 — World Information** | Faction standing, political events, NPC relationships, notices, quest/task info | Fully diegetic. Lives in physical world objects (faction board, posted notices, pinned documents). |

The rule: **time-critical = abstract; not-time-critical = diegetic.** No third category. No "pretty abstract UI" for Layer 2.

**Layer 1 shape vocabulary:**

- Panels framed by compressed pointed arch (not decorative gothic tracery — the functional structural arch from haunt architecture). Line weight 1px at standard resolution.
- No rounded corners. No drop shadows. No gradient fills. Solid single-color fills at **40-60% opacity** against dark surround.
- Health/mana bars: horizontal bars, pointed-arch terminus on right end, **3px height**, full tracking-panel width, no glow.
- Buttons: trapezoidal form (wider at base, narrower at top) mirroring Vampire Court door-arch geometry.
- Icon frames: square with 45° chamfer all corners (diamond rotated to square, carrying rotational symmetry of floor tile patterns).

**Layer 2 shape vocabulary:**

- Every document, notice, faction-board entry is a physical object with material history.
- Paper stock is **faction-specific** (defined in Section 4 Color System).
- Handwriting is faction-specific: Vampire Court uses formal chancery hand; Ghoul Syndicate uses abbreviated cramped cipher; Pale King Cult uses backward-leaning devotional script.
- Seals and insignia are **worn, not crisp**.
- Pinning and mounting methods are faction-specific and show use history (Syndicate notice folded twice, stabbed with iron pin rusted into board; Court dispatch rolled, wax-sealed, opened and resealed at least once).

**The diegetic tension rule.** Test: *"Would this information exist in this world if there were no player?"* Faction news exists — it is on a board. Reputation standing exists — NPCs know it and express it through dialogue. The abstract UI representations of this information are what betray the world.

**Emotional communication.** The HUD is invisible in practice — quiet enough that players stop seeing it after a few sessions. The world-information layer is the game's information system; reading it produces the pleasure of reading a city.

**Pillar anchor.** Principle 1 (Stillness Is The Signal) + Principle 2 (Faction Before Fantasy) → Pillar 2 (Silence Is Sacred).

### 3.4 — Hero Shapes vs. Supporting Shapes

**What draws the eye:** **the specific** (a candle that is the only warm source in a cool-lit room) and **the still** (a figure completely still among figures with low-amplitude idle motion). Neither uses brightness, scale, saturation, or motion emphasis. The visual hierarchy of a Gravenspire scene operates through the inverse of most game design — what is most **resolved and most specific** receives attention, not what is most prominent.

**What recedes:** set-dressing entities (ambient NPCs, background figures, environmental inhabitants) are visually quiet through:
- Lower material resolution (fewer specific surface details)
- Lower-amplitude idle animation (motion that does not differentiate them from ambient environmental movement like fabric physics)
- Lower-contrast placement against surfaces behind them

**This is not an LOD system** (though LOD exists). This is **designed quietness maintained even at close range** for entities whose purpose is to populate without directing attention. A background merchant at 5m is designed to be less resolved than a named NPC at the same distance. **Material quality is the volume knob for attention.**

**Grammar of significance without performance.** Everything that matters is specific and old. A named NPC has more surface history than an ambient NPC — not more surface complexity, but more resolved history. Wear patterns corresponding to their specific occupation; posture specific to their long-term role (a Syndicate fence who has spent years bending over a desk has a specific thoracic curve; a Court attendant who has spent years standing at formal functions has a different postural set). The literacy develops over time and cannot be shortcut.

**Critical prohibition.** There is no visual grammar element that says "this is important" directly. **No** colored outline on interactable objects. **No** floating nameplate in differentiated color for named entities. **No** particle halo, **no** emission ring, **no** audio cue tied to visual emphasis. The game offers the player a world and trusts them to develop the reading. A new player will walk past named NPCs without recognizing them. A thirty-hour player will not. This is **the social contract of the game design expressed visually.**

**Production test.** Before releasing any scene: (1) remove all NPCs and review environment, (2) reintroduce ambient NPCs — they should add inhabited density without directing attention, (3) introduce named NPCs — they should read as more specific against the inhabited background without emitting emphasis. If any named NPC requires a UI marker to distinguish them from ambient NPCs at 15m, the model or material has failed and must be revised.

**Emotional communication.** The pleasure of becoming a citizen of the city. The player who has developed the literacy feels a specific pride in recognition — not "the game showed me that matters" but "I know that matters because I have been here long enough."

**Pillar anchor.** Principle 2 + Principle 3 → Pillar 1.

### 3.5 — Productive Tensions (production-level)

1. **Silhouette readability target ↔ no-hero-silhouette rule.** The 80px faction-legibility target requires distinctive silhouettes; the no-hero rule forbids player visual priority. **Resolution:** test silhouettes by asking "does this read the same whether a player or an NPC wears it?" If the player version reads as more protagonist-shaped, the design fails. Distinction lives in faction geometry, not individual character geometry.

2. **Geometry wear ↔ navigation legibility.** Bowing walls, racked doorframes, sloped floors communicate age but obscure nav affordances. **Resolution:** wear language holds everywhere EXCEPT within **1m of a ledge, step, or hazard threshold**, where geometry must be readable as navigation affordance.

3. **Layer 1 HUD minimalism ↔ corpse-run desaturation.** The HUD uses 40-60% opacity; the corpse-run state applies -40% global desat to the dead player's camera. HUD designed for full-saturation viewing may lose contrast under corpse-run filter. **Resolution (per Section 4.4):** HUD is **EXEMPT** from the -40% desaturation pass. Implementation: URP camera-stacking isolates HUD (Overlay camera) from the world's post-process volume (Base camera). See Section 4.4 for full rationale and the technical-artist verification flag at line 483. The "pending" status is closed; production validation of the URP camera-stack pattern itself remains a tech-artist deliverable but is not blocking on art direction. Resolved 2026-05-15 by AD-ART-BIBLE sign-off pass finding F-03.

### 3.6 — What This Section FORBIDS

- Isolated apex landmark elements (spikes, horns, towering headpieces) on non-named characters. Named characters with such elements must have it documented in their individual design note with pillar-aligned justification.
- Silhouettes > **1.8× shoulder-width** of equivalent-height human in idle pose.
- Weapons extending > **0.4m beyond body silhouette** in resting carry.
- Decorative fabric (cape, cloak, mantle) rigged to billow, flare, or create heroic silhouette in idle or walk.
- Any player-character silhouette enhancement (rim light, shader pass) not equally applied to NPCs in equivalent faction dress.
- Faction insignia at 80px silhouette-readable scale. Faction reads through shape, proportion, and material — not logo.
- Zero-delta surfaces (fresh-textured stone, plumb walls, level floors) outside specifically justified newly-constructed set pieces.
- Global post-process color grade override tied to zone load boundary. Palette shifts are material and light-source driven, continuous, gradual.
- Colored outlines, differentiated nameplates, particle/emission emphasis on interactables, significant entities, or named NPCs.
- Layer 2 world-information (faction standing, political events, NPC relationships) presented through abstract MMO UI conventions (progress bars, colored tier labels, floating iconography). Lives only in diegetic world objects.
- Ambient NPCs at the same material resolution as named NPCs. Ambient stays visually quieter at all distances, including close proximity. Maintained even under art-quality pressure to raise ambient fidelity.
- **The word "boss" is not a valid visual brief.** Named significant entities are more specific and more historically resolved than ambient enemies; they are never larger, brighter, more elaborately decorated, or visually foregrounded by the environment.

---

## Section 4 — Color System

### Color Philosophy

Color in Gravenspire is not applied. It is what remains after 400 years of weather, soot, human contact, and factional occupation have worked on specific materials. Every color decision must have a material cause rooted in the world's physical history. "The city uses this palette" is not a justification. "This stone weathers to this temperature in this climate after this many decades of foot traffic and candle smoke" is.

The Pre-Raphaelite reference establishes the operative standard. Rossetti and Millais do not paint beauty through saturation — they paint surfaces that are specifically themselves. Every color is load-bearing. No color exists as filler. **A small palette applied with specificity produces more visual richness than a large palette applied democratically.**

RPG convention uses color semiotically (red = danger, gold = reward, purple = rare). Gravenspire's vocabulary is the opposite: colors mean what the materials they live on mean, within the world's history. Gold in Gravenspire is not "valuable" — it is the specific color of tallow candle wax after extended burning, or aged brass, or the ochre-brown of old leather. The player learns to read it not because a legend explained it, but because they have spent time in the city.

### 4.1 — The Master Palette

Colors of Gravenspire as a city, independent of the factions that occupy it. These are the base against which all faction color reads.

| Name | Hex | Temperature/Saturation | Primary Role | Forbidden Uses |
|---|---|---|---|---|
| **Quarry Stone** | `#8A8478` | Cool neutral, desaturated warm (~4800-5200K paint equivalent) | Dominant building material — exterior stone walls, paving, architectural mass. The default read of the city at distance. | Not for fabric or character materials. Not for UI fills. Not for sky or ambient fill color. |
| **Wick Gray** | `#5C5650` | Slightly warm dark, near-neutral | Aged timber, weathered iron, old lacquer, shadow-register architectural surfaces. The color of things that were once other colors. | Not for skin. Not for UI background. Not for state/condition signaling. |
| **Candlefall Amber** | `#C48B3A` | Warm, low-mid saturation | Practical light sources only — candle, lantern, hearth. Present only where a physical source exists. | Not as a tint over ambient surfaces. Not for UI. Not for magic VFX. Not for gold-as-reward. |
| **Pewter Rain** | `#9EA4A8` | Cool, desaturated blue-gray | Overcast sky ambient, wet stone reflection, slate rooftiles, cool diffuse daylight when no practical source competes. | Not as a tonal grade over scenes. Not for water/fluid effects. Not as a faction color. |
| **Iron Seam** | `#3D3A38` | Very dark warm near-black | Deep shadow, unlit corridor depth, sub-threshold darkness that retains material identity. **Not black** — shadow on stone that still has surface information. | Not as true black (pure `#000000` is forbidden in all ambient contexts). Not as silhouette fill. Not for UI. |
| **Render Umber** | `#7A6248` | Warm mid-tone, low saturation | Interior plaster, worn timber flooring, old ceiling beams, smoke-absorbed fabric. The warm-neutral of everything interior and old. | Not as skin tone. Not as substitute for faction color. Not for surfaces newer than 30 in-world years. |
| **Bone Pale** | `#D4CCBC` | Warm-cool transitional, low sat, high value | Aged paper, bleached linen, very old plaster past cream. Lightest the city gets without direct practical light on a surface. **Never white** — always yellowed, stained, specific. | Not for purity symbolism. Not for UI background. Not for healing/safety association. Not as true white. |

**Notes:**
- Quarry Stone and Pewter Rain bracket the neutral range. The city reads differently under diffuse daylight (Pewter Rain dominant) vs. overcast shade (Quarry Stone warmer by contrast). This oscillation is natural limestone behavior, not a zone grade.
- **Iron Seam is the shadow anchor.** It must retain surface identity — you can still see what material is in shadow. Target for URP ambient occlusion and shadow color: warm dark, not black.
- **Bone Pale is the ceiling of lightness.** Nothing reads brighter without a direct practical source illuminating it.

### 4.2 — Per-Faction Color Systems

All six factions share the city. Their distinguishing colors are not imposed — they are what their specific materials and practices produce over time, read against the shared Quarry Stone and Wick Gray base.

#### Vampire Court

| Attribute | Specification |
|---|---|
| **Primary** | Tarnished silver-blue `#8A9BA8` — polished black marble under cold indirect light, aged mirror silver, candlelight-absorbing velvet |
| **Secondary** | Deep slate violet `#4A4058` — undercolor of black garments in candlelight, aged velvet backing; visible only in candlelight, not overcast daylight |
| **Forbidden pairing** | No warm amber as Court accent. Court may contain candlelight but does not warm — Court surfaces read cool against the warm source |
| **Material expression** | Polished black marble floors, dado panels. Tarnished silver fixture hardware. Formal black velvet/fine wool absorbing rather than scattering light. Aged mirror glass (mercury-backed, oxidized at edges) reflecting imperfectly |
| **Age evolution** | **~0-20 years:** true black + hard silver, cold and controlled, high contrast constrained. **~50 years:** green-gray surface oxidation in damp areas; silver tarnished to blue-gray; unified quiet, high contrast replaced by tonal compression. **~200 years:** black marble polished by centuries reflects cold ambient as pools of diffuse light; silver almost pewter; architecturally inevitable |

**Reading:** Not the color of death or threat — the color of sustained formality, of a social order maintained long past the conditions that created it. The unease is in the perfection of preservation, not in the darkness.

#### Ghoul Syndicate

| Attribute | Specification |
|---|---|
| **Primary** | Old-wax ochre `#9A7B42` — tallow candle wax, lard-oil-sealed wood, fat-preserved leather; things preserved through function, not aesthetics |
| **Secondary** | Rust iron `#7A4A38` — ferrous hardware in damp conditions, hinges and pins left to oxidize; most chromatic note in Syndicate space, readable at distance |
| **Forbidden pairing** | No polished/refined surfaces. No silver. No vertical formal geometry (reads as Court contamination) |
| **Material expression** | Stained structural timber (lap joints, added brackets, different wood species from repairs). Iron hardware oxidized rust-brown on all exposed surfaces. Old-wax leather on portable objects. Rough-cut stone in excavated undercrypts with visible iron-mineral staining |
| **Age evolution** | **~0-20 years:** bright yellow-brown ochre, dark gray iron pre-oxidation. **~50 years:** ochre deepened and homogenized; rust full range; reads as lived-in functional space. **~200 years:** ochre almost indistinguishable from Render Umber in some light; rust progressed past brown toward dull dark red; can be mistaken for abandoned — often intentional |

**Reading:** Working material, not statement material. The palette of things kept functional rather than maintained beautifully. The warmest interior read in the city — **productive tension:** the Syndicate's most welcoming-looking spaces are often its most operationally dangerous.

#### Necromancer Academy

| Attribute | Specification |
|---|---|
| **Primary** | Aged parchment `#C4B48A` — warm ivory-yellow of academic documents over generations; the Academy's primary output bleeding into spatial vocabulary |
| **Secondary** | Academic blue-black `#2A3040` — iron gall ink after 100 years on parchment; writing instruments, note labels, diagram borders; present in detail rather than mass |
| **Forbidden pairing** | No decorative ossuary elements. The Academy's relationship to death is scholarly, not devotional — any bone-motif collapses it into generic undead aesthetic and loses the critical distinction from Pale King Cult |
| **Material expression** | Parchment and paper in layered horizontal accumulation. Academic black ink on writing surfaces. Formal undyed linen/wool academic dress aged to parchment tone. Brass measuring instruments (verdigris-green on copper elements, yellow-brass on newer pieces) |
| **Age evolution** | **Fresh:** near-cream parchment, near-black ink; almost formal and clean — can look like Court for first 20 years. **~50 years:** deepened mid-ochre parchment; ink faded to blue-gray at surface; distinction from Court clear, distinction from Syndicate now requires silhouette. **~200 years:** deep amber-ochre of very old parchment in bulk; oldest wings look built of compressed documents — not entirely inaccurate |

**Reading:** Warmth is intellectual rather than occupational. Parchment-ink vocabulary consistent enough for new players to learn, old enough to never read as fresh or deliberate.

#### Cult of the Pale King

| Attribute | Specification |
|---|---|
| **Primary** | Mortared chalk `#C8C4B8` — bleached ancient stone in oldest architectural strata; pre-city stonework before centuries of smoke have grayed it; aggressive lightness reading as age, not cleanliness |
| **Secondary** | Devotional gray-purple `#7A7488` — aged liturgical cloth left without dye-fixing for decades, faded past original purple toward cool mid-gray that still carries the hue; fabric and textile only, never architectural |
| **Forbidden pairing** | No black. No silver. No Court vocabulary in any form — the Cult predates the Court by centuries; formal-cool aesthetic reads as Court imitation. Not to be confused with Bone Pale (which is a surface neutral; the Cult's chalk-white is deliberate material choice) |
| **Material expression** | Oldest stone in the city — pre-cut massive blocks, mortar visible, no surface finish. Devotional textile in faded liturgical colors. Crude candle-holders, offering-bowls in unfinished bronze (not polished brass, not silver — pre-craft-guild bronze from before the city's trade infrastructure) |
| **Age evolution** | **Fresh (~0-20 years Cult occupation):** chalk-white reads alien — too light, too bare. Communicates deliberate archaism, insistence on oldest forms. **~50 years:** absorbed city air but lightness remains distinctive; gray-purple deepened; archaeological. **~200 years:** same stone as 400 years ago, absorbing at rate of dense old stone; reads as geological fact — "this stone is the oldest thing here" is the Cult's entire argument |

**Reading:** Lightness is the uncanny element. In a city trending warm-dark, pale cold massive stone reads immediately as pre-city. Unease is in the exposure — Cult spaces feel like they were never finished by any civilization that would recognize the city above them.

#### Haunt Collective

| Attribute | Specification |
|---|---|
| **Primary** | Interstitial rust-green `#687058` — iron oxidizing through mortar in spaces with water infiltration; a surface belonging to no specific material category because environmental degradation has collapsed distinctions; the exact color between iron-rust-brown and stone-gray-green |
| **Secondary** | Faded limewash `#A4A898` — walls painted with faction colors by previous occupants, then abandoned, then limewashed, then abandoned again; cool-gray-green of limewash over pigment that bled through |
| **Forbidden pairing** | No strong single-hue accent. Any strong accent reads as actual faction claim — the one thing Collective space does not make |
| **Material expression** | Transitional wall surfaces where one faction's material ends and another's begins without clean junction. Temporary structures (fabric dividers, wooden screens) carrying multiple prior-factions' partial colors. Iron hardware painted over and repainted, all layers visible at stress points |
| **Age evolution** | **~0-20 years:** still readable as whatever-it-used-to-be with Collective modifications on top. **~50 years:** limewash and rust-green begin merging prior occupant colors into characteristic indeterminacy. **~200 years:** fully resolved interstitial — a Collective space of great age looks like a cross-section of the city's political history with labels removed |

**Reading:** Not a style — the residue of contested occupation. Visual identity defined by **absence** of singular identity. Harder to maintain consistently than a positive-color system; the interstitial rust-green anchor gives artists a material starting point before layering complexity.

#### Living Resistance

| Attribute | Specification |
|---|---|
| **Primary** | Trade-district ochre `#A08058` — warm-brown of commercial building materials: local un-refined clay brick, pine-tar-preserved pine timber, rough-milled wooden planking; color of the city's pre-undead working population's built environment |
| **Secondary** | Undyed linen `#C8B898` — off-white of functional textile never finished with luxury dye; workwear, domestic textile, improvised fabric; color of things made to be used, not seen |
| **Forbidden pairing** | No faction-formal elements from any of the five undead factions. Any slip into Court verticality, Syndicate ochre-rust, Academy parchment-warmth, or Cult archaism collapses the factional read |
| **Material expression** | Brick and rough timber framing (not gothic stone). Pine-tar-preserved wood (specific blue-gray-black surface). Undyed linen and rough wool. Repurposed objects with original faction context visible but modified (a Syndicate satchel with Court-livery buckles replaced by wooden toggles) |
| **Age evolution** | **Fresh:** reads closer to Syndicate Old-wax ochre than comfortable — accurate, because Resistance often operates out of Syndicate-adjacent working-class buildings; distinction via silhouette + material, not hue. **~50 years:** pine-tar blue-black secondary patinas more visible; undyed linen deepens; working-class warmth more specific. **~200 years:** oldest Resistance buildings look like the city's pre-political substratum — the buildings that were here before the factions |

**Faction identity under corpse-run desaturation:** All six primaries specified in low-to-mid saturation. Vampire Court silver-blue, Pale King chalk-white, and Haunt Collective rust-green are most vulnerable under -40% desat. **Backup cue: silhouette** (Section 3.1) — color system does not need to carry faction identity alone under corpse-run conditions. These interact as a designed system.

### 4.3 — Semantic Color Vocabulary

Color carries meaning only when it carries material. These rules replace RPG convention with world-grounded reading.

**Red.** In Gravenspire, red means **biological** — blood, wound, fresh animal tissue — and appears in the world only as such, never as stylized danger signaling. Bloodstains on cold limestone dry to a dark red-brown closer to Rust Iron than to signaling red. Fresh blood in combat reads at biological saturation. Magic is not red. Danger indicators are not red. When a player encounters red in the world, their nervous system responds to its biological specificity, not to a trained game-convention reflex.

**Gold/yellow.** In Gravenspire, gold means **age and practice** — tallow wax, worked leather, brass that have been handled for a long time by someone with a specific occupation. The forbidden-glowing-loot rule removes gold's RPG function. A truly gold-colored object reads as an aged formal object of specific origin (Court or Academy provenance), communicating institutional age, not monetary value.

**Blue/green.** In Gravenspire, blue means **cold-source light** (exterior sky, overcast ambient, uninhabited temperature of a space not warmed by recent occupation), and green means **biological surface time** (lichen, moss, verdigris — surfaces undisturbed long enough for organisms to colonize). Green means time has passed without human intervention at this location — a navigational read as much as atmospheric.

**White.** In Gravenspire, white means **very old and specifically exposed** — stone predating the city's centuries of occupation, not yet absorbed enough soot to gray down. Anything reading as clean white is a flag: either narratively justified as newly constructed (within 5 in-world years), or the texture asset needs an age-plausibility revision pass. **White is not in the Gravenspire visual vocabulary as a positive value.**

**Black.** In Gravenspire, black means **sustained formal attention** — surfaces maintained and polished by an organization that has the resources and cultural mandate to prevent wear from showing, i.e., the Vampire Court's specific form of power. What looks black in the world is always Iron Seam or deeper. The one faction that approaches true black (Court polished marble) makes it work by polishing, not by surface neutrality — the surface reflects, containing ambient light around it.

### 4.4 — UI Palette (Layer 1 HUD)

**Corpse-run strategy — HUD is EXEMPT from the -40% desaturation pass.** Rationale: designing the HUD to survive -40% desat would require starting saturation 66% higher than needed, violating "architecturally humble." Camera-stacking in URP (HUD on Overlay camera; world on Base camera) naturally isolates the post-process volume — HUD does not receive world's desat. This also correctly models fiction: the HUD is the player's own perceptual state (body, resources), which has not changed; the world is what looks different.

> **Flag for technical-artist:** Confirm URP post-process volumes on Overlay camera stack are correctly isolated from Base camera post-process pass in Unity 6.3 LTS. Production validation required before Tier 1 implementation.

| Element | Color | Hex | Why |
|---|---|---|---|
| **Health bar** | Render Umber | `#7A6248` | Health is the warmth of the body — interior heat, organic material. Biological register, not conventional red. Full health reads warm and material-present. |
| **Health bar — low state (<20%)** | Bone Pale, 75-80% opacity | `#D4CCBC` | **Counter-intuitive:** drains LIGHTER and COOLER. The body becoming cold, not bleeding — warning is the warmth draining, not a danger signal firing. A quiet alarm, not a loud one. Paired with a slow-pulse animation in combat contexts only (combat is the kinetic exception to Stillness Is The Signal). |
| **Mana bar** | Pewter Rain | `#9EA4A8` | Resource drawn from the environment, the cold of the world, not from the body's warmth. Resource is the city; health is the body. |
| **Mana bar — depleted (<20%)** | Wick Gray | `#5C5650` | Depletion toward dark-neutral, not toward warm. Running out of mana reads as the world becoming less available, not as danger. |
| **Hate / Threat indicator** | Academic blue-black, 50% opacity | `#2A3040` | Hate is attention directed at the player — cool and directed. Blue-black is the Academy's notation color, suggesting being written down, indexed. The enemy tracks you the way a record-keeper tracks a debt. |
| **Hate — maximum (pulling aggro)** | Rust Iron | `#7A4A38` | The HUD's loudest moment — the one higher-saturation warm, borrowed from Syndicate material vocabulary. Rust iron reads as **functional alarm** without conventional red. Color of something ferrous under stress. |
| **Background panel** | Iron Seam, 45% opacity | `#3D3A38` | Shadow-anchor at sub-50% creates a panel that is there-but-not-there, the way architectural framing is present without demanding attention. Meets the 40-60% opacity mandate from Section 3.3. |

All bars read as architectural materials under the compressed-arch framing from Section 3.3. The Umber health bar reads as a material strip inside a stone frame, not a conventional game UI bar. The HUD uses **city colors, not UI colors.**

### 4.5 — Color Temperature Rules Per Zone

**The standing rule (Section 1):** No jump-cut color grades between zones. No post-process override at a zone boundary. Every temperature shift is material-driven and light-source-logical.

**Between-zone transition mechanism:** The player moves from city street into haunt via an architectural threshold (door, passage arch, stairway descent). The light-source inventory changes through that threshold — exterior city has competing interior-practical and exterior-ambient; haunt interior has only dying practicals with no exterior ambient input. The temperature shift is a **consequence of geometry,** not a filter. The art team specifies where the last exterior-sky-contribution point is (typically the threshold itself or within 3-4m of it), and beyond that point the scene is lit solely by what exists inside it.

| Zone | Temperature & Palette Rule |
|---|---|
| **City Hub — Day** | Working-warmth. Quarry Stone under 6000K overcast exterior; 2600-3200K competing interior practicals. Net interior ~4000-4500K neutral-warm. Shadow temperature Pewter Rain. Faction colors present as material accents. Most chromatic variety in the city. |
| **City Hub — Night** | Candlelight-sealed. 2200-2600K dominant throughout. Exterior ambient gone; windows reflect candle-warmth back as dark mirrors. Shadows under candlelight are warm-dark, not cool-dark. Grays/browns recede into unified warm-dark field; faction material colors emerge as the only chromatic variety. Hub reads smaller at night — illuminated space defined by candle-throw radius. |
| **Vampire Court Haunt** | Cold ambient, specific warmth. No exterior ambient. Dying/controlled practicals at 1800-2200K. Polished black marble reads cool under this light — does not warm in candlelight the way matte surfaces do; mirrors the candle as point-source reflection. Dominant read: Iron Seam with thin silver-blue highlights and warm-point candle-spots that do not extend their warmth into surrounding stone. Warmth exists but is not absorbed. |
| **Ghoul Syndicate Undercrypt** | Warmth by accumulation. Tallow candles and oil lanterns in functional density. 2200-2600K with higher source density than other faction spaces — warmer interior despite lower individual source quality. Old-wax ochre surfaces reflect warmth broadly. Rust-iron hardware creates warm-dark mid-tones. **The warmest interior space in the game.** |
| **Necromancer Academy Haunt** | Academic-cool with directed warmth. Reading light — functional overhead sources positioned for task, not atmosphere. 3200-4000K (oil lamps with white-hot wicks). Neutral-working temperature — the temperature of attention. Parchment-warm mid-register; academic blue-black ink-detail at contrast points. Lightest-reading interior after the Cult. |
| **Cult of the Pale King Pre-Gothic** | Cold and exposed. Oldest spaces have no practical source logic except devotional flame — single candles, offering-lamps, low-duration open flame at 1800-2000K with limited throw. Chalk-white stone absorbs very little warmth; reflects candle as near-white against warm-dark ambient. **Temperature inversion** — light source is warmer than in any other zone, but surfaces are coldest-reading because the stone was cut before architecture that traps warm air. Not candlelit warmth — the specific discomfort of warm-source light in a space that will not warm up. |
| **Haunt Collective Interstitials** | Transitional — no consistent temperature because no consistent light-source inventory. A Collective passage may pass from a previously Court-held section (cool, brass wall sconce) through a previously Syndicate-held section (warm tallow in iron holder) into an unclaimed corridor (ambient blue-dark from exterior crack). **The only zone where color temperature is deliberately unstable.** Not a zone-grade change — a practical-source-inventory change. Source types must be physically placed, not post-process imposed. |
| **Living Resistance Buildings** | Working-warm with functional source logic. Trade-district buildings designed for human occupation — hearth-fires (2200-2600K wide radius), oil lamps on work surfaces (2800-3200K directed), natural light through relatively un-blocked windows (6000K diffuse, more exterior access than faction haunts). Resistance buildings breathe — windows not shuttered, doors open to streets, sources serve occupants not atmosphere. **Closest the game gets to a "normal" interior.** The city's former baseline. |

### 4.6 — Colorblind Accessibility

**Redundancy system:** Faction identification = Silhouette (Section 3.1) + Color (this section) + Material Texture. Any single channel can fail and the other two carry the identification. Color is **by design the weakest channel** — it depends on ambient conditions, colorblindness, and desaturation states. **The game is not accessible through color alone and does not need to be.**

#### Deuteranopia (red-green deficiency — most common, ~6% of males)

| Failure Pair | Why It Fails | Backup Cue |
|---|---|---|
| Health bar Umber vs. Mana bar Pewter Rain | Red-green channel collapse may reduce distinction | **Position** (Health left / Mana right, consistent HUD placement) + **value contrast** (Umber darker than Pewter Rain — luminance survives deut) |
| Rust Iron hate-maximum vs. Umber health | Both read as mid-dark tones under deut | **Shape and position** — hate indicator is a distinct geometric element, never co-located with health bar |
| Syndicate Rust-Iron vs. Resistance Trade-Ochre | Both in orange-brown register | **Silhouette** (Syndicate horizontal-weighted; Resistance improvised) + **material texture** (iron-oxide surface vs. pine-tar timber) |
| Biological red (blood) vs. ambient stone | Red-brown blood may not distinguish at wound sites | **Animation** (blood has drip/pool behavior) + **surface specificity** (wet-material sheen stone doesn't have) |

#### Protanopia (red deficiency — ~1% of males)

| Failure Pair | Why It Fails | Backup Cue |
|---|---|---|
| Rust Iron hate at maximum | Red-brown signal grays out entirely | **Shape pulse** — hate indicator outline pulses at 1.5Hz in combat context only. Explicitly permitted by Stillness Is The Signal because combat is the kinetic exception. Outside combat, static. |
| Vampire Court silver-blue vs. Haunt Collective faded limewash | Both may read as similar gray | **Silhouette** (Court vertical formal; Collective dissolution-at-edges) + **pattern** (Court regular polished; Collective irregular layered) |
| Old-wax ochre (Syndicate) vs. academic parchment (Academy) | Both yellow-warm, may merge | **Silhouette** (Syndicate horizontal; Academy vertical-with-hood) + **material texture** (smooth aged leather vs. stacked paper edges) |

#### Tritanopia (blue-yellow deficiency — ~0.01%, rarest)

| Failure Pair | Why It Fails | Backup Cue |
|---|---|---|
| Mana bar Pewter Rain vs. panel Iron Seam | Blue channel collapse flattens against background | **Luminance contrast** — Pewter Rain must maintain ≥3:1 contrast against Iron Seam panel. Achievable; confirm in HUD implementation. |
| Court vellum (gray-blue) vs. Syndicate parchment (old-wax) | Blue-yellow axis is exactly this distinction's axis | **Texture pattern** (Court smooth/thin/formal; Syndicate rough/thick/fibrous) + **handwriting** (Court chancery; Syndicate cramped cipher) — both survive greyscale |
| Pewter Rain ambient vs. Candlefall Amber warm | Zone-transition temperature signal may lose clarity | **Light source visibility** — practicals are physically present and identifiable by object. You can see the candle, the lantern, the hearth. Temperature is an effect of a visible cause; the source-object carries the information |

### 4.7 — Productive Tensions

1. **Faction warmth inversion vs. emotional intuition.** Ghoul Syndicate occupies the warmest interior in the city; Vampire Court (most associated with formal power) occupies the coldest. Materially accurate but inverts the intuition that power spaces feel warm. **Load-bearing:** teaches the player that warmth in Gravenspire is functional, not hierarchical — the correct lesson for a world where undead power-hierarchy does not map to human comfort logic.

2. **Corpse-run exemption vs. HUD-as-world.** Exempting the HUD from desat is technically correct and argument-supported, but means the HUD is the ONLY element retaining full saturation during the desaturated death state — inverse of the design intention (HUD invisible in normal play). May actually be appropriate: player is in high-stress navigational task (recover corpse); slightly increased HUD legibility is contextually correct even if aesthetically not ideal. **Monitor in playtest.**

3. **Living Resistance readability vs. faction distinctness.** Trade-District Ochre sits within the warm-brown band with Syndicate Old-Wax Ochre and Academy Parchment-Amber. Under overcast ambient at distance, the three are in proximity. **Silhouette + color are not independent systems — they are co-dependent.** Must be tested as a system, not independently.

### 4.8 — What This Section FORBIDS

- **Rarity colors on any object.** No golden glow, no purple shimmer, no tier color system. Item significance is social and contextual, readable only through material quality and faction-specific design.
- **Warm-colored magic VFX.** Spell effects are cool-desaturated (ice, shadow, bone-white). The one exception is biological-temperature fire — which behaves as fire behaves (localized, physically motivated, short duration), not as magic signal. Fire is fire. Magic is not fire.
- **Post-process zone-grade overrides.** No LUT swap at a zone boundary. Color temperature shifts produced by light-source inventory changes and geometry.
- **True black (`#000000`) in any ambient surface, shadow, or fill.** Iron Seam is the shadow anchor. Shadows have material identity. Voids are not black — they are dark with surface information.
- **True white (`#FFFFFF`) on any surface older than 5 in-world years.** Bone Pale is the ceiling.
- **Gold as reward signaling.** Yellow-amber reads as material (tallow, brass, parchment-age, ochre dye) or practical light. Never valuable. Never rare.
- **Green as safety or health.** Green means biological colonization time. Never safety. Never "this way."
- **Blue as cooldown or mana-energy.** Mana is Pewter Rain because Pewter Rain is the world's cool ambient. The distinction matters when onboarding UI artists.
- **Red as stylized danger signaling.** Red is biological. Blood, wound, tissue at biological accuracy. Not enemy highlights, damage overlays, warning vignettes, or faction colors.
- **The Haunt Collective assigned a "strong" faction color.** The Collective's visual identity is indeterminacy. Any attempt to simplify is a misunderstanding of the faction's political character.
- **Faction colors at full saturation.** Every faction primary is weathered, aged, low-to-mid saturation. No version of any faction's color should appear at full digital saturation — not in the world, not in the UI, not in marketing.
- **Using ambient color temperature as emotional shorthand.** Warm does not mean safe. Cool does not mean threat. These conventions are legible but player-service and flatten the world. The Syndicate undercrypt is warm and you are in significant danger there.

---

## Section 5 — Character Design Direction

### Character Philosophy

Every player character in Gravenspire is a newcomer to an ancient social order, and the art must enforce that legibility gap. A character who has just arrived reads as someone who has just arrived — garments ungiven by faction, materials unweather-stained by specific occupation, posture not yet settled. A character who has spent sixty hours accumulating Vampire Court reputation reads as someone who has been admitted somewhere, slowly, across many transactions — not promoted, but **admitted**. The visual grammar of character progression is the grammar of social absorption: faction materials accumulating on the body, garment wear patterns corresponding to where you have been standing and for how long, the specific postural compression of someone who has learned the correct ways to hold themselves in certain rooms. The weight-and-age rule does not grant players a grace period. From character creation forward, every visual decision on the player character earns its place through weight and age, not through the desire to read as a protagonist.

### 5.1 — Player Character Visual Archetype

#### The Onboarding Player

A new character arrives in the visual register of someone who does not yet belong anywhere. Garments read as **pre-faction**: undyed linen and rough wool in Bone Pale and Render Umber, materials available to anyone in the city's trade district before allegiance has been declared. No faction color, no material quality implying membership. Silhouette is occupation-baseline — the player-selected class contributes the first layer of occupational shape (Cleric: layered mid-length vertical emphasis; Warrior: padded-layer horizontal shoulder mass; Enchanter: weighted-hood forward-lean suggesting Academy-adjacent formation but not Academy membership).

The new character does not read as a hero. They read as **a new resident** — someone wearing what they could afford before knowing which faction they would be spending time with.

**Onboarding visual constraints (negative):**
1. No faction-primary color present anywhere at > 5% surface area
2. All materials in the Bone Pale / Render Umber / Wick Gray neutral band
3. No material quality token (polished, refined, embroidered) associated with any faction vocabulary

#### The Veteran Player — Faction Reputation Visual Tiers

| Rep Tier | Visual State | Faction Color | Material Quality |
|---|---|---|---|
| **0 — Unknown** | Neutral undyed. Pre-faction baseline. | None | Bone Pale / Render Umber / Wick Gray only |
| **1 — Recognized** | Single faction-material accent (a Court lapel pin, a Syndicate carrying satchel, an Academy ink-stained cuff) | <10% surface area | Faction material at 0-20 year age tier |
| **2 — Affiliated** | Multiple garment pieces in faction vocabulary. Silhouette begins shifting toward faction signature. | 20-30% surface area | 20-50 year age tier materials |
| **3 — Trusted** | Dominant faction vocabulary. Silhouette matches faction signature in form. Secondary faction history visible in residual material from previous affiliations (*the world can read where you have been*). | 40-60% surface area | 50-year tier materials |
| **4 — Integral** | Near-full faction vocabulary. **Player character visually indistinguishable from a faction NPC at 80px silhouette distance.** Distinction requires proximity and material read. | 60-80% surface area | 50-200 year tier materials in faction's primary expression |

**Critical design decision:** Tier 4 integration IS the visual design goal. When a player achieves full faction integration, they stop reading as "player" and start reading as "faction member" at distance — the correct experience for a game where reputation is progression.

#### Progression-Visible Elements

**1. Garments (primary carrier).** Class silhouette baseline stays stable; faction vocabulary accumulates in layers, folds, and material quality. Garments show wear patterns corresponding to how the character has been spending their time: a character who has done heavy Haunt Collective exploration shows Collective interstitial rust-green transfer on lower garment hems (contact with Collective wall surfaces). A Cleric who has spent time in the Academy shows ink-staining on the right cuff consistent with document-handling.

**2. Carried objects (secondary carrier).** Weapons, tools, and satchels accumulate material history. A weapon used in Syndicate undercrypt encounters develops ochre-brown staining in the blade fuller consistent with that stone's iron-mineral content. A Cleric's focus item shows grip wear and thumb-polish. **Objects tell occupation history.**

**3. Postural compression (tertiary carrier — rigging system).** Long-term faction participation produces faction-specific postural tendency in idle and walk animation: Court characters develop slight chin-up carriage; Academy characters the weighted-hood forward lean; Syndicate characters a low-center-of-gravity walk weight. *Rigging and animation system — flagged for technical-artist and animator coordination.* The body learns the world it has been inhabiting.

**What does NOT change with progression:**
- Silhouette scale (no size increase)
- Luminosity, emission, particle effects of any kind
- Face (character's own history, not faction history)
- Class baseline anatomy and proportion

#### Group Read

In a camp of five players and AI companions, the player reads as one member of the group. No protagonist framing. The group reads as a group through proximity and arrangement, not through any individual being visually elevated. The player recognizes their own character through: position (camera anchor) + motion (their input) + their specific accumulated material/faction combination. No other distinguishing visual element is permitted.

**Production test:** Render the player character at identical distance and lighting conditions as AI companions and other player characters. If the player character reads louder than equivalently-dressed NPCs at the same distance, the implementation has introduced protagonist framing and must be corrected.

### 5.2 — Character Type Distinguishing Rules

#### Player vs. Named NPC: No Visual Marker System

**There is no marker.** The player learns to identify player vs. NPC through accumulated literacy, not a UI layer.

- **Behavior under player control** is the primary tell — player characters respond to input with player-characteristic jank (stop-start movement, menu-open pauses, non-occupational behavior). NPCs move with occupational purposefulness or stand with postural specificity.
- **Faction vocabulary vs. neutral baseline** — new players may be in neutral garments; NPCs are always in faction vocabulary.
- **Postural compression specificity** — NPCs have occupation-specific compression; players have faction-averaged compression from rep accumulation.

The player who cannot tell a player from an NPC is getting the new-player experience the design intends. The player who can tell has developed appropriate literacy. Neither is a failure state.

#### Named NPC vs. Ambient NPC — Production Material Resolution Tiers

| Attribute | Ambient NPC | Named NPC |
|---|---|---|
| **Polygon budget** | 6,000-8,000 tri (body + clothing) | 14,000-18,000 tri. Named characters occupy the scene with physical density. |
| **Texture resolution** | 512×512 diffuse / 256×256 normal. Single material set. | 1024×1024 diffuse / 512×512 normal body. **2048×2048 face albedo + normal** — portrait resolution because proximity occurs during dialogue. |
| **Material complexity** | 1-2 materials. Faction-correct surface and age tier but limited surface event variety. | 3-5 materials with surface-event specificity. Edge-wear at cuffs from occupation, repaired seams, chemical staining from what they actually carry. |
| **Rigging** | Standard biped, 1 idle blend, 1 walk, 1 combat. Jaw only facial rig. | Full biped with occupation-adjusted bone weights for postural compression. Facial rig with faction-range expressive capacity. Named-character signature pose blend tree. |
| **Shader passes** | 1 pass: standard PBR. | 2 passes: standard PBR + per-character SSS on biological surfaces (face, hands). Skin responds to practical-source light the way materials in the world respond. |
| **LOD steps** | 3 LODs: LOD0 <10m, LOD1 (50% tri) 10-25m, LOD2 (25% tri) 25-50m, cull beyond 50m. | 4 LODs: LOD0 <5m, LOD1 (75% tri) 5-10m, LOD2 (50% tri) 10-25m, LOD3 (25% tri) 25-50m, cull beyond 50m. Extra LOD preserves portrait-grade through social range. |

> **Requires technical-artist validation.** Polygon budgets are targets for Unity 6.3 URP with 10-50 concurrent clients in a dense city environment. Actual budgets depend on scene character density, GPU tier targets, and batching strategy. **The ratios (named ~2× ambient poly count; named 4× ambient face resolution) should be preserved even if absolute numbers shift.**

#### Enemy Creature vs. Civilian Person — Garments Are Civilization

In Gravenspire, a Ghoul Syndicate member is an undead *person* operating within a social structure. A feral ghoul in a haunt is an undead *creature* with no faction affiliation. The distinction is among the most important in the game's design.

**Primary cue — behavioral:**
- Civilian/person NPCs occupy space with social purposefulness. Occupational postures at occupational positions. Social response to proximity (turning to face, not pivoting to combat orientation). Idle-loop behavior appropriate to role.
- Creature enemies do not have social postures because they have no social roles.

**Secondary cue — garment vs. biology:**
- Faction-affiliated undead *persons* are dressed. Their undead biology is present but managed within their social role.
- Creature enemies are either undressed (bare biological forms) or in garments destroyed past faction-legibility (shredded, soiled, absent deliberate dress construction).

**The hard rule:** A character is visually a civilian as long as their garment vocabulary communicates faction membership at or above the lowest faction recognition threshold (Rep Tier 1 equivalent material). Below that threshold — garments too damaged, too absent, too far from any faction vocabulary — the character is in creature-read territory regardless of undead biology. **Garments are civilization.**

**Exceptional case:** Named hostile NPCs in full faction garments are intentional — the distinction between combat-hostile and civilian faction members is **behavioral state** (the pivot), not visual differentiation. You fight the Court attendant sent to stop you; you converse with the Court attendant who has not. They look the same. This is the world not warning you.

#### Allied Companion (AI Party) vs. Stranger NPC

No floating fellowship marker. No group color ring. No overhead icon. The player learns to identify AI companions through repeated exposure — the mechanism by which you learn to identify people in any social setting.

**Identification mechanisms:**
1. **Spatial positioning during camp** — AI companions are the characters seated in the light radius of the party's fire.
2. **Behavioral responsiveness** — named companions respond to player actions within a response window (choosing the same corridor, sitting when player sits to med). Strangers have no behavioral responsiveness.
3. **Character-specific visual fingerprint** — named AI companions are Named-NPC-tier resolution with specific visual signatures players learn over time (Korrath's Academy garment with repaired left shoulder; Vesna's Resistance kit with repurposed Syndicate satchel).
4. **Combat proximity** — companions direct behavior toward shared enemies; strangers flee or disengage.

### 5.3 — Expression & Pose Style

#### Facial Animation — Sculpted-Specific Register

Not realistic (requires render fidelity we don't have). Not theatrical (performs emotion for the viewer). The **sculpted-specific** mid-register of portrait painting: faces carry expression in their particularity of form rather than in the range of motion they perform.

**Reference is Pre-Raphaelite portraiture as an animation philosophy** — the expression is in the face at rest as much as in motion. Rossetti's Elizabeth Siddal does not need to move her face to communicate weight of interior life — the sculpt carries it. **Neutral mesh IS an expression.** An NPC who is habitually watchful has that watchfulness in the orbital set. An NPC worn by long occupation has the wear in nasolabial depth, not just added when emoting.

**Facial rig spec for Named NPCs:**
- Jaw open/close for dialogue vocalization
- Brow complex: inner raise / outer raise / compress (3 shapes)
- Eye aperture: squint-light / open-attention
- Mouth corners: compress / slightly part — does not smile widely, does not sneer openly
- **Total: 8-12 blend shapes maximum**

**Cannot express:** open-mouthed surprise, wide smile, theatrical anger, despair. An NPC who would recoil in another game instead performs a very slight brow compress and a fraction-second stillness. **Restraint IS the expression.**

**Ambient NPCs:** Jaw only. No facial rig beyond ambient vocalization support.

#### Idle Poses — Present But Not Performing

**What does a person look like when doing their specific occupation in a moment of non-activity?** Not "what does an idle loop look like."

- **Amplitude:** Very low. Weight-shifting micro-movement (0.5-1.5° trunk rotation per breath cycle, 0.2-0.4s period). Breathing visible as rib-cage expansion. Hand micro-movement at class-appropriate rest position.
- **No performance idle.** No looking-around. No stretching. No sighing-visible. A character who has been standing at their faction-position for 200 years does not look bored — they look like they have been there for 200 years.
- **Idle loop length:** 8-12 seconds minimum before repeat. No hard-cut on restart. Blend tree allows 3-4 randomized variation sub-loops.
- **Stillness test:** idle motion must be indistinguishable from subtle environmental animation (fabric physics, ambient particles) when viewed peripherally. **If idle motion directs attention to the NPC, amplitude is too high.**

#### Combat Poses — The Pivot and Its Follow-Through

The pivot (Section 2, State 3) is the one visual event that communicates combat engagement. Everything else follows.

- **Pre-pull state:** ambient occupation posture. No combat anticipation. No pre-battle pacing or weapon-readying. The enemy does not signal combat intent in advance. Not a player-friendly affordance — world behavior.
- **The pivot:** mesh rotation ambient → facing-toward in 0.3-0.5s. Simultaneous: occupation posture → low-center-of-gravity weight-shift (a person who has done this many times finding a ready position, not a hero adopting a heroic pose). Weapon draw 0.6-0.9s during which the character is already pivoting and closing distance.
- **Ready position is character-type specific** — Syndicate enforcer finds forward-lean with low shoulders; Court guard finds upright-but-weighted stance with controlled arm carriage.
- **Combat hold/idle** during tab-target: weight distributed forward, head orientation tracking target, micro-adjustment of footing. Not a loop of dramatic aggressive gestures. **Focused attention under physical readiness.**
- **Post-combat/disengage:** No victory pose. A character who has just killed an enemy returns to ambient occupation posture at normal walk speed, without acknowledgment. **The world has seen this before.**

#### Dialogue Poses — Minimal Body, Maximum Voice

**Head-and-upper-body system. No full-body acting. No stills.** Rationale: full-body acting telegraphs emotional state too broadly. Stills break presence. Head-and-upper-body maintains physical presence with small movement vocabulary.

**4-state posture vocabulary:**

| State | Head | Upper Torso | Hands | When Used |
|---|---|---|---|---|
| **Engaged** | Slight chin-down, direct orientation | Micro-turn toward player | Occupational rest | Baseline dialogue state |
| **Considering** | Slight tilt (3-5°), brief stillness | No change | One hand near chin, no pointing | Processing question/decision |
| **Dismissive** | Chin-level or fractionally above | Micro-rotation away, weight-shift back | Both hands at occupation rests | Declining or ending exchange |
| **Cautious** | Chin-level, scan micro-movement (2-3° arc) | Slight shoulder-inward | Hands lower, tighter to body | Unsafe or politically sensitive context |

Transitions: 0.3-0.5s cross-blend. No snapping.

**Mouth sync for LLM voice:** jaw-open phoneme approximation only (open/closed/mid-open). No viseme system. Voice carries the expression; jaw sync prevents visible puppeting without lip-sync accuracy.

#### Named NPC Signature Behaviors

1-2 very low-amplitude occupation-specific micro-signatures per character, documented in the character design note and reviewed by art director before rigging implementation. Examples:
- The fence who performs a specific right-weight-shift before concluding transactions (a tell visible only to someone who has dealt with them multiple times)
- The Court attendant who keeps their right hand at a specific position against their thigh (the position of someone trained to reach a weapon they aren't currently wearing)
- The Academy archivist who tracks the player's hands rather than their face during dialogue (years of watching for theft)

**Undocumented micro-behaviors introduced in animation will be removed.**

### 5.4 — LOD / Detail Philosophy at Camera Distance

**Camera default:** 4-6m in tight spaces, 6-9m in open environments.

#### Range Bands

| Range | Usage | Primary Read |
|---|---|---|
| **Silhouette (30m+)** | Ambient city navigation | Faction silhouette only — per S3.1 rule |
| **Social (5-15m)** | NPC approach, group identification | Material differential — named vs. ambient resolution visible |
| **Intimate (<5m)** | Active dialogue, combat proximity, camp med break | Full LOD0 activates. Named NPCs with SSS, cloth sim, facial rig. Pre-Raphaelite portrait-grade skin on named NPCs. |
| **Inspection (<2m)** | Corpse inspection, UI portrait context | Portrait-quality face required. **The mesh face at LOD0 under portrait-appropriate light IS the portrait** — no separate asset. This forces face mesh quality to be portrait-quality from the start. |

#### Unity URP Production Targets *(requires technical-artist validation)*

| Tier | LOD0 Tri | LOD1 (75%) | LOD2 (50%) | LOD3 (25%) | Cull | Face Albedo | Body Albedo | Body Normal | Shader |
|---|---|---|---|---|---|---|---|---|---|
| **Player** | 18,000 | 13,500 | 9,000 | 4,500 | 80m | 2048² | 1024² | 512² | PBR + SSS on skin |
| **Named NPC** | 16,000 | 12,000 | 8,000 | 4,000 | 60m | 2048² | 1024² | 512² | PBR + SSS on skin |
| **Ambient NPC** | 6,000 | 3,000 | 1,500 | 600 | 50m | 512² | 512² | 256² | Standard PBR |
| **Creature enemy** | 8-12k | 6-9k | 3-4.5k | 1-1.5k | 60m | 512² | 512² | 256² | Standard PBR |

**LOD step distances:**

| Tier | LOD0→1 | LOD1→2 | LOD2→3 | LOD3→Cull |
|---|---|---|---|---|
| Player | 5m | 12m | 25m | 80m |
| Named NPC | 5m | 10m | 25m | 60m |
| Ambient NPC | 10m | 20m | 35m | 50m |
| Creature | 8m | 18m | 30m | 60m |

> **Critical ratios to preserve through any budget adjustment:** Named NPC ~2× ambient poly count. Named NPC 4× ambient face resolution. LOD3 must preserve faction-distinctive silhouette geometry (Court's high flat-topped collar, Syndicate's asymmetric satchel mass, Academy's weighted-hood forward-lean survive aggressive LOD reduction).

### 5.5 — Productive Tensions

1. **Companion identification vs. no-indicator rule.** Tractable at MVP with low companion count (2-3 named AI companions at Cleric Tier 1). Grows harder as roster expands. Either companion count is held low enough that learning is tractable, or the no-indicator rule becomes a design problem. **Design and production coordination issue** — flagged before it becomes a user complaint.
2. **Postural compression vs. animation budget.** Per-faction postural blend tree across 6 factions × 3+ classes × 4 rep tiers is significant rigging time. **MVP scope:** implement for one faction (Court) at Tier 3+ rep, validate in playtest, expand in later tiers. Design principle documented now; full implementation is phased.
3. **Garment-wear-pattern transfer vs. texture system complexity.** Per-character dynamic texture modification for location-specific surface marks is compelling but a meaningful technical system. **Flag for technical-artist before implementation** — may need to be static garment variants at MVP, with dynamic transfer added later if Unity 6.3 URP supports it at acceptable perf cost.

### 5.6 — What This Section FORBIDS

- Any visual marker, glow, outline, particle effect, shader highlight, or floating indicator distinguishing a player character from an NPC at equivalent faction dress
- Any marker, ring, overhead icon, or shader distinguishing AI companions from stranger NPCs
- Idle animation that directs the viewer's attention toward the idle character — must fail the stillness test or be reduced
- Victory poses, defeat poses, or emotive animations following combat resolution
- Named NPC facial animation exceeding the 8-12 blend-shape budget
- Ambient NPCs at named-NPC-tier material resolution, polygon budget, or shader complexity, even under milestone pressure
- Characters reading as protagonists at any rep tier. Tier 4 full integration = faction-indistinguishable at silhouette range. **This is the goal, not a failure.**
- True black or true white on any character surface
- Faction insignia at 80px silhouette-readable scale
- Creature enemies wearing undamaged faction garments (the garment rule is the line between civilizational and non-civilizational read)
- Signature poses or micro-behavioral signatures on named NPCs without documented approved character design note

---

## Section 6 — Environment Design Language

### Environment Philosophy

The city of Gravenspire is not a set. It is the physical residue of 400 years of occupation, contestation, rebuilding, and managed decline by entities who do not die and therefore do not hurry. Every surface carries legible physical history; every room has been used in ways that slightly precede its current use. The environment does not perform atmosphere — it accumulates evidence.

This means two things operationally. First, environment art earns its place through *causal specificity* (every crack, stain, and modification has a material cause in the world's history), not through aesthetic density. Second, the artist's hardest discipline is restraint — stopping before the scene tips from *inhabited* into *staged*. A tavern with nineteen props reads as a movie set. A tavern with eleven props, each one placed where an actual person would actually put it, reads as someone's livelihood.

### 6.1 — Architectural Style and World Culture/History

#### How a First-Time Visitor Reads the City's Age

Gravenspire's age is not communicated through ruin or decay — both are theatrical shortcuts. The city is not ruined. It is *continuously inhabited* by an extremely old population that has been maintaining it to their own standards across multiple political regimes. Age reads through **geological-scale** persistence: street widths encoding pre-city property boundaries that no living authority remembers establishing. Second-floor overhangs extending progressively further out over a main street until they nearly touch the buildings opposite, because each was built by a different owner to capture slightly more interior space and no one ever said no. A pointed-arch doorway infilled with a round-arch insert because the original surround cracked and the repair was done two centuries later by someone with different formal training.

Visitors read age through **strata** — visible geological layering of architectural decision-making. The bottom register of any building tends to be the oldest and most massive (pre-gothic crude stonework, Romanesque round arches, load-bearing walls almost a meter thick). As the building rises, each floor may show a different architectural period of modification.

**Five primary age-legibility cues in architectural first read:**

1. **Threshold geometry contradiction.** Every old doorway has been modified at least once. The frame carries evidence: a gothic pointed arch with a flat lintel inserted at mid-height when someone needed the door shorter. Hinges from three periods visible in filled and relocated hinge-pintles. Door material does not match frame material.

2. **Vertical accretion in exterior walls.** Buildings grew up, not out. Base course is rough-cut massive stone; first addition shows chamfered ashlar; second shows smaller brick with different mortar chemistry. **The building's history is a stratigraphy visible from the street, provided the player looks up.**

3. **Infrastructure interruption.** Drainage gutters cut across older decorative courses without regard for ornament. A carved corbel removed and replaced with a functional iron bracket. A window bricked in (different brick from wall); a new window cut through older solid wall (raw stone cut edges, never dressed). The city's function has repeatedly overridden its aesthetics.

4. **Wear directionality tells social history.** Stone steps worn smooth at the center, rough at edges, records centuries of foot traffic at this specific social choke point. A doorway threshold worn into a bowl by foot pattern. A wall face abraded at shoulder-height for six meters along a narrow alley — where everyone has put their right hand for 300 years. **The wear pattern is the building's diary.**

5. **Repair materials preserve political record.** After a faction changes control of a district, they rebuild using their materials — but selectively. Load-bearing infrastructure stays; visible surface elements (window surrounds, door hardware, wall facing) shift to new-faction vocabulary. The result is buildings that read like contested documents — the original author still visible under the revision.

#### Architecture as Factional History

**Wherever factional control has changed, the architecture has a visible seam.**

A **Court-dominant** building shows: original 200-year-old pre-Court structure; Court modification phase (~180 years ago) — black marble dado panels applied, silver-then-tarnished hardware replacement, windows enlarged with Court surround treatment; Court maintenance phase (ongoing) — mortar repointed, marble polished annually, no vegetation encroachment on lower register.

A **Syndicate-dominant** building shows: original Resistance-era trade-district brick and timber; Syndicate first-occupation excavation (passage driven through original basement, rough-cut stone revealed beneath brick foundation, iron pit-props not architectural-grade); Syndicate modification layer — windows boarded or solid-shuttered, iron hardware on all closures, staining from oil-lamp burn.

A **Haunt Collective** building is a palimpsest — Court molding over Syndicate patching over original construction, limewash applied over all of it, limewash worn through to previous layers at corners and high-traffic points, revealing competing histories at once.

**Production discipline:** every building must have a documented occupation history of 2-3 sentences specifying which factions held it and when, before modeling begins. **The artist cannot know which details to layer without knowing the history being layered.**

#### The Five Architectural Tells of an Un-Living City

Specific signals communicating Gravenspire's nature — not "undead" generically, but *this city with this history*:

1. **Ergonomic calibration for undead physiology.** Court stair risers 2-3cm taller than a living city; doorframes 2.1-2.3m clear height (formal presentation requires heads-up clearance). Syndicate undercroft passages 1.8m — efficient working access, ghouls don't need headroom. A player begins to read spatial calibration as faction vocabulary.

2. **No organic accumulation in Court and Cult zones.** Organic process requires organic presence and heat. The absence of biological mess in high-Court zones is not a design choice — it is environmental physics. Resistance zones show all the organic accumulation of any human neighborhood.

3. **Preserved interiors at civilizational scale.** A room furnished 150 years ago, unchanged because the occupants do not change with new lifetimes of preference. **Preserved in use** — not a museum, because objects are still actively used, but with no generational turnover of taste. Specifically uncanny: a room that looks antique but shows daily use by someone with very old taste.

4. **Light management for predatory function.** Upper-quarter windows shuttered on a schedule that has nothing to do with weather or sleep — shuttered at dawn, opened at dusk. Windows architecturally modified: deep reveals, angled to avoid direct sun penetration even when open.

5. **Infrastructure scaled for political rather than practical use.** Court-quarter streets wide enough for formal processions that occur twice a year — monumentally oversized for daily traffic. Academy corridors sized for scholars to pass without breaking stride carrying stacked documents. **Scale reveals faction values more reliably than surface decoration.**

#### Chronological Architectural Strata

| Stratum | Period | Character | Faction Association |
|---|---|---|---|
| **1 — Pre-City Foundation** | Pre-historic | Massive rough-cut stone/sandstone, crude lime mortar, round arches, walls 0.9-1.2m thick, no ornament, windows are slots | Pale King Cult (theology IS the pre-history) |
| **2 — Early City / Living Foundation** | ~400-250 years ago | Pointed gothic arches, dressed stone, ornamental corbels, civic identity through carved emblems | Original medieval Italian city-state character — the structural bones under subsequent occupation |
| **3 — The Transition Period** | ~250-150 years ago | Original building forms maintained for legitimacy, surface-level modifications to assert factional identity | The period when undead factions achieved political dominance — most visible seams of occupation-change |
| **4 — Factional Consolidation** | ~150-50 years ago | Each faction building or modifying architecture confidently in its own vocabulary | Where most recognizable factional architectural signatures originated |
| **5 — Current State** | Past 50 years | Newest visible strata, reflects current factional balance of power | Where a faction lost ground shows unrepaired damage; where it gained shows current-vocabulary construction |

**Production requirement:** every environment must be assigned a primary stratum and at least one secondary stratum visible in the final asset. A building exclusively Stratum 4 without earlier stratum visible is implausible and must be revised.

#### Per-Faction Architectural Expression

| Faction | Primary Stratum | Maintenance | Key Architectural Tell |
|---|---|---|---|
| **Vampire Court** | 3-4 | Actively maintained to Stratum 4 standard; decay visible only in surface chemistry | Largest window reveals; geometric precision of doorway surrounds that has not degraded in 150 years |
| **Ghoul Syndicate** | 2 + own Stratum 4 excavations | Functional maintenance; surface repairs indefinitely deferred | Visible excavation cuts in existing foundations; iron pit-props in passages supposed to be temporary |
| **Necromancer Academy** | 3 (religious adapted) | Structural integrity prioritized; ornamental elements defaced where they conflict with academic use | Nave-scale spaces subdivided by wood-and-iron mezzanines; reading-lamp hardware on pre-gothic columns |
| **Cult of the Pale King** | 1 | No maintenance except structural emergency; deliberate anti-maintenance (intervention is interference with Pale King's will) | Vegetation deliberately introduced into cracks; no mortar repointing for 200+ years |
| **Haunt Collective** | All in transition | No maintenance; each previous occupant's repair history archaeologically visible | No consistent language; transition seams between modifications without bridging |
| **Living Resistance** | 2 | Active human-biological maintenance; cleanliness is survival | All windows operable; fresh mortar in recent repairs; organic modifications (window-box growth, salvaged hardware in working condition) |

#### Section 6.1 — Unity URP Production Guidance

- **Modular architecture kit requires strata tagging.** Every modular piece (wall, arch, doorframe, floor) carries metadata indicating stratum and faction attribution. Enables layering strata without redesigning geometry; seams come from assembling differently-tagged pieces.
- **Trim-sheet strategy with strata variation.** Trim sheets authored in pairs: clean-register (Stratum 2-3 first construction) and age-modified (mortar loss, surface accretion, repair patches). UV-tiling at matching scale for seam-free transitions.
- **Organic absence in undead zones.** Do NOT simulate organic debris in Court/Cult zones through particle systems or scattered meshes. **The absence is the statement.**

#### Section 6.1 — Forbidden Shortcuts

- Applying a single "old and damaged" mesh pass to all buildings without stratum differentiation
- "Gothic" as a uniform architectural style — Gravenspire is specific to its history; gothic coexists with Romanesque, crude pre-city, and functional Resistance
- Fresh or uniform mortar on any pre-Stratum 5 construction
- Faction insignia carved or applied to architecture as primary faction-identification — faction reads through material, geometry, and maintenance pattern, not logos

### 6.2 — Texture and Material Philosophy

#### The Decision: Grounded PBR with Hand-Authored Surface History

Not painterly-cartoon, not photorealistic PBR. **Grounded PBR with hand-authored surface history.**

Standard PBR captures material properties with physical accuracy but its failure mode is *ahistorical precision* — a fresh PBR surface looks like the day it was created. Pure handpainted (Borderlands / Wind Waker) achieves aesthetic identity but sacrifices the material specificity weight-and-age demands.

**The Gravenspire hybrid:** PBR physically-based properties (roughness, normal, metallic) authored for material accuracy, then **hand-authored age and history layered on top** in diffuse/albedo and roughness channels. Physically convincing material + artistically controlled history. Aligns with Pre-Raphaelite reference: not photorealistic, not illustrative, but *specifically itself*.

**Every albedo texture must pass the "cause test":** every patch of discoloration, every worn edge, every mineral staining deposit must have an identifiable physical cause. **"Variation texture" added to break visual monotony is forbidden.** The texture carries history, not randomness.

#### Texture Budgets by Surface Category

*(Character budgets are in Section 5.4, independent from these environment budgets.)*

| Surface Category | Albedo | Normal | Roughness | Metallic | Notes |
|---|---|---|---|---|---|
| **Architecture — Primary (facade, major structural)** | 1024×1024 | 1024×1024 | 512×512 | 256×256 where applicable | Tileable at 1:2m scale. History in dedicated decal layer, not baked into tile base. |
| **Architecture — Secondary (interior wall, ceiling)** | 512×512 | 512×512 | 256×256 | 128×128 | Same tileability target. Lower priority for unique surface events. |
| **Architecture — Unique Surface (landmark facade, boss haunt feature wall)** | 2048×2048 | 2048×2048 | 1024×1024 | 512×512 | Non-tileable. Reserved for spaces players spend significant time at close range. **Budget capped at 3-4 per zone.** |
| **Environment Props — Major (furniture, structural fixture)** | 512×512 | 256×256 | 256×256 | 128×128 | Individual UV per object. No tiling. |
| **Environment Props — Minor (candle, book, implement)** | 256×256 | 128×128 | 128×128 | — | Atlas-packed in sets of 8-16 similar props. |
| **Ground surface — Primary (street, courtyard)** | 1024×1024 | 1024×1024 | 512×512 | — | Tileable at 1:1.5m scale. Wear variation via vertex-painted overlay. |
| **Ground surface — Interior floor** | 512×512 (or 1024×1024 for unique material like Court marble) | 512×512 | 256×256 | 256×256 for polished | Marble surfaces (Court) require metallic/smoothness for specular response. |
| **Sky / Exterior dome** | 1024×512 panoramic | — | — | — | Overcast 6000K ambient per Section 2. No dramatic sky; no stars (perpetual overcast is the city's weather state). |

> **Flag for technical-artist:** Confirm texture streaming behavior in Unity 6.3 LTS for city-density environment at 10-50 concurrent. Mipmap budgets must accommodate 30m faction-silhouette-legibility (Section 3.1). If 1024² exterior facades mip too aggressively at 30m, either increase base resolution or author dedicated LOD texture.

#### Material Reuse Strategy — The 80/20 Rule

- **~80% of visible surface area** uses tileable base materials from a managed library of ~30-40 tile sets (organized by stratum and faction)
- **~20% of visible surface area** uses unique surface-event assets (decals, overlays, unique-UV'd props) to differentiate specific locations

**Tile set naming:** `[material-type]_[stratum]_[faction-or-neutral]_[age-tier]`
- Examples: `stone_ashlar_court_200yr`, `brick_coursed_resistance_50yr`, `stone_rough_precity_400yr`, `timber_lap_syndicate_80yr`

This ensures any two tile sets from the same stratum/faction/age-tier mix without jarring discontinuity.

**Unique surface events via decal layer:** history detail (staining, moss/lichen, mortar-loss, scorch marks, liquid tracks) handled in a dedicated decal layer on top of tileable bases. Separates reusable base from specific history. Decal library grows incrementally; specific causes documented per decal (a rust-stain always comes from iron hardware above it).

> **Flag for technical-artist:** Unity 6.3 URP decal projector system required. Confirm performance budget per scene for decal projector count at city-exterior density. If projector count is prohibitive outdoors, explore baked-decal approach for static outdoor geometry with projectors reserved for interiors.

#### "History in the Surface" — Implementing Section 3 Wear Specs

Section 3.2 defines physical wear values. These require texture implementation:

- **Wall bow (4-8cm at 3m span):** mortar lines don't run perfectly horizontal on a bowed wall. Hand-author normal map to show mortar cracking in tension on outer face (horizontal crack lines), compression on inner face (vertical squeeze lines). Plaster/limewash overlays show cracking consistent with bow direction.
- **Sloped floors (0.5-1.5°):** water and particulate have settled toward drainage for centuries. Drainage-low-point shows mineral staining and organic growth absent at high point. Roughness at low point slightly higher (particulate accumulation) UNLESS high-traffic drainage (tavern floors, stable floors) where low point is smoother from liquid cleaning. **Drainage direction tells who keeps the floor and how.**
- **Racked doorframes (1-3°):** the door fitted for a square frame has been rehung multiple times. Multiple hinge-pintle positions visible (filled holes from previous positions, different rust staining). Door edge contact wear where frame is now closer. Wood shows diagonal compression mark where it rubs frame under load.
- **Worn stair treads (8-12mm depression at nosing):** worn center is *smoother* than unworn edges — centuries of foot-polish. Roughness: worn-center low (polished), raised-nosing high (exposed aggregate). Faction variation — Court treads polished consistently across full width (formal processions use full stair); Syndicate treads worn only at center (single-file functional traffic); Resistance shows repair patches (replacement stone tread set slightly high).

#### Section 6.2 — Unity URP Production Guidance

- **PBR workflow with hand-authored history** is compatible with URP's lit shader without custom shader work.
- **Decal projector system:** URP Decal Projector component. Validate per-scene count in Unity 6.3 LTS against frame budget. *(Flag for technical-artist.)*
- **Vertex color as wear map:** architecture meshes with baked geometry wear can use vertex color for material blending (high vertex alpha = worn smooth; low = rough original). Requires shader graph blending node. *(Flag for technical-artist.)*
- **No emissive on architecture.** No architectural surface carries emissive — light comes from sources, not materials. Iron Seam shadows retain surface identity through AO and roughness, not emissive fills.

#### Per-Faction Material Expression

| Faction | Surface Material Priority | Age/Wear Character | Material Tell |
|---|---|---|---|
| **Vampire Court** | Polished black marble, tarnished silver, candlelight-absorbing velvet | *Maintained* deterioration — oxidation is real but not causing structural decay; surfaces show age in chemistry not damage | Mirror-surface reflectivity on marble must respond correctly to practical light sources (reflection only, no ambient emission) |
| **Ghoul Syndicate** | Stained structural timber, oxidized iron, sealed-fat surfaces, rough-excavated stone | *Functional* deterioration — use-wear at high-contact points, deferred maintenance everywhere else; rust present but arrested, not progressing | Iron hardware shows multi-stage rust (fresh orange, older dark brown, oldest near-black); no single-color-rust surfaces |
| **Necromancer Academy** | Parchment accumulation, formal academic stone/wood, brass instruments | *Use*-deterioration — worn at functional contact points, pristine where untouched; contrast IS the texture signature | Document surface texture shows age stratigraphy — oldest documents at bottom of piles darker, more fragile-looking than newer on top |
| **Cult of the Pale King** | Massive rough-cut pre-city limestone, crude bronze, faded liturgical textile | *Deliberate non-maintenance* — surface is as old as first construction plus only what time itself has done | Chalk-white limestone must maintain lightness even in shadow (high albedo even at low roughness) |
| **Haunt Collective** | Interstitial accumulation — no single-faction material dominant | *Palimpsest* deterioration — every layer visible simultaneously | Color complexity higher than any other faction; requires more decal layers for correct contested-history read |
| **Living Resistance** | Trade-district brick and timber, pine-tar preserved wood, undyed functional textile | *Practical living* wear — high-touch surfaces worn smooth, maintained for function, organic accumulation present | The only faction where organic-growth on lower walls is NOT neglect — it IS living occupation |

#### Section 6.2 — Forbidden Shortcuts

- **"Dirt overlay"** applied uniformly without specific physical cause. Dirt has sources (water run-off, foot traffic, smoke deposition, biological growth). Each dirt mark needs a cause.
- **Fully procedural or algorithmic texture variation** as substitute for hand-authored history. Noise-based aging looks random; history-based aging looks like history. They feel different even if the player can't articulate why.
- **Emissive on any non-light-source architectural surface.**
- **Fresh or uniform mortar** in architectural textures older than Stratum 5.
- **Roughness uniformity** across any surface older than 30 in-world years.

### 6.3 — Prop Density Rules Per Area

#### The Anti-Theater Principle

**Every prop must have an answer to "why is this object here and not somewhere else?"** If the answer is "for atmosphere" or "to fill the space" or "to make it feel lived-in," the prop is theater. If the answer is "because whoever uses this room would have put it here for this reason," the prop is inhabited.

**Theater density fails** not because it has too many objects but because its objects are *decorative in aggregate* — each individually plausible, but collectively arranged to produce aesthetic effect rather than reflect actual use. The specific tell is **uniform distribution** — props spread at aesthetically even intervals are theater; props clustered near function (candles near work surfaces, not centered on empty shelves) are inhabited.

**Placement test:** before placing any prop, ask: "Who put this here, and what were they doing when they put it here?" If the answer requires imagining a person in the middle of an activity (a Syndicate fence who just finished tallying and set the ledger on the left corner of the desk as she stood up) — the prop is inhabited. If the answer is "an environment artist who thought this space needed more visual interest" — the prop is theater.

#### City Street

- **Prop count:** 8-14 unique placements per 10m street section (excluding architectural furniture like lantern posts, gutter infrastructure, mounted signage)
- **Placement logic:** Cluster at social activity nodes — doorsteps, corner wait-positions, markets-adjacent. Middle-of-street is near-zero; the street's purpose is traversal.
- **Crowd density:** 3-8 active ambient NPCs visible in hub streets during day cycle. Lower in secondary streets (1-3). Near-zero in alleys.
- **Props by context node:**
  - *Doorstep:* 1-2 props (boot-scraper, delivered object not yet taken in, recently-placed in-use object) — threshold deposit, not display.
  - *Corner wait-position:* 0-1 props (leaned object, discarded container from regular visitor).
  - *Loading/delivery point:* 3-5 props in functional cluster (crates, bundles, rope coil) positioned as if work was interrupted.
  - *Market-adjacent:* 4-7 props reflecting active-transaction, not display-quality, arrangements.

**Theater vs. inhabited streets:** theater has even distribution and uniform height. Inhabited has prop clusters at function nodes with clear negative space between; objects at the height they were put at.

#### Inn Interior

- **Prop count:** 12-20 unique placements in a typical taproom (4-6 tables, 8-12 seating spaces)
- **The innkeeper's density test:** surfaces an innkeeper would clean are sparsely propped (swept, maintained). Surfaces they don't clean (shelf behind bar, storage alcove, under-table) are moderately propped. **Near-zero-prop places are the evidence of occupation** — a swept floor is maintained by someone here.
- **Table arrangement:** 2-3 props max per table, and only if recently or currently occupied. Empty tables have zero props unless structurally a work surface. **The empty table is NOT an opportunity for atmospheric prop dressing.**
- **Dense clusters permitted:** bar surface (working tools in functional arrangement), kitchen threshold (overflow), single patron-specific table where an NPC with established location-habit has gathered materials.
- **Prohibited:** evenly distributed atmosphere props on every horizontal surface; candles serving no functional purpose; framed objects on walls unless faction-board-class information objects.

#### Haunt Interior — Faction-Specific Density

The haunt density principle: haunts are spaces with *specific use history*, not *aesthetic atmosphere*. Props are the material residue of what has been happening there. Spatially, haunts are sequences of function nodes — storage, meeting, sleeping, access — each with the props of its function, separated by low-prop transition corridors.

| Faction | Density | Density Principle | Character |
|---|---|---|---|
| **Vampire Court** | **Low (6-10/room)** | Maintained sparse formality — negative space as political statement | Each prop is specific object with specific provenance. New player reads under-propped; 30-hour player reads the specific objects loud |
| **Ghoul Syndicate** | **High (18-30/room)** | Functional accumulation at working surfaces; cleared paths (min 1m) to every functional point; no aesthetic organization | Dense but organized by function, not display. Visual complexity communicates active use |
| **Necromancer Academy** | **Very high (25-40/horizontal room)** | Academic density — horizontal surfaces covered in documents and instruments; vertical heavily used; specific organization system requiring occupant-knowledge to navigate | Density is cognitive load, not aesthetic choice. Chaos is expertise-specific, not atmospheric |
| **Cult of the Pale King** | **Very low (4-8/room)** | Devotional minimum — props as offerings; specific, intentional, sparse | Sparseness more unsettling than density. Massive chamber with four candles and an offering bowl |
| **Haunt Collective** | **Medium-high (14-22/room, layered)** | Residual accumulation; objects from previous occupants mixed with current + temporary structures; no organization | Objects from different factions in same space without connecting logic. Density of history with no organizing use |
| **Living Resistance** | **Medium (12-18/room)** | Practical working density — most functional organization; most-used closest at hand; survival-influenced prioritization | Denser than Court, less than Syndicate; more organized than Collective, less formal than Academy. Improvised home |

**Collective interstitial compositional rule:** every prop cluster must include objects with *incompatible faction provenance*. A Syndicate iron tool-box next to a Court formal document-box next to an improvised Resistance sleeping roll.

**When does density become theater?**
1. Objects at aesthetically even distribution rather than functional clustering
2. Density uniform across a space (inhabited has variation — dense at function nodes, sparse in traversal paths)
3. Objects positioned for visibility rather than use (eye level facing out, vs. height and orientation of last use)
4. Objects belong to no specific person or regime of use — generic "atmosphere objects" (candles, bottles, skulls) rather than specific-faction, specific-function

#### Section 6.3 — Unity URP Production Guidance

- **Prop atlas packing:** minor props atlas-packed by faction to minimize draw calls in faction-heavy scenes. Court on one atlas; Syndicate on another; etc. Faction-mixing in Collective requires cross-faction atlas or performance decision about draw calls.
- **GPU instancing:** ambient architecture props that repeat (identical candle holders, identical book spines) should use GPU instancing. **Not optional for Syndicate and Academy high-density scenes.**
- **Occlusion culling critical in haunt interiors.** Academy and Syndicate high-density rooms require strong occlusion setup. *(Flag for technical-artist to validate Unity 6.3 LTS occlusion in faction-specific room configurations.)*

#### Section 6.3 — Forbidden Shortcuts

- **"Scatter"** — randomly distributed props used to break visual monotony. Every prop placement is a decision, not a brush stroke.
- **Skulls, bones, or death-iconography** as generic atmospheric props. Communicates "this is an undead space" without communicating *which* undead faction.
- **Evenly distributed candles** on every horizontal surface. Candles are present where function requires light.
- **Generic "medieval atmosphere" props** (wine jugs, stacked barrels, scattered parchment) in scenes without specific use-narrative. Every prop must belong to specific Gravenspire faction's material vocabulary.

### 6.4 — Environmental Storytelling

#### What Tells the Story

Gravenspire's vocabulary is the vocabulary of **physical consequence.** Not narrative illustration — consequence.

- **Narrative illustration** shows you a story (the bloodstain tells you a fight happened here).
- **Physical consequence** is the residue of systems operating (the drainage pattern on this floor tells you this room has been regularly hosed down — which tells you something about what regularly needs cleaning in this room).

**The player who reads Gravenspire's environment is doing forensics, not narrative consumption.** The environment offers residue of systems and events; the player assembles history from evidence. This is the game's deepest literacy — the visual equivalent of its political simulation.

**Specific details that tell the story:**

- **Use-pattern wear at function positions** — the flagstone in front of the Syndicate fence's desk more worn than beside it, the specific oval of a person who stands at a specific position for years. More specific than a bloodstain, more durable, less theatrical.
- **Organizational systems showing revision history** — the Academy's shelves showing a classification applied, reconsidered, and partially reclassified; two competing shelf-label systems visible; some books filed under both.
- **Ownership marks telling social relationship** — a Syndicate tool with a Court seal partially obscured. Not conspiracy — probably the tool was owned by a Court servant who sold it or had it taken.
- **Temporary measures become permanent** — a timber shore-post supporting a cracked Court arch, put there 80 years ago as temporary pending a real repair, never replaced because the arch isn't actually failing and the Court sees no reason.
- **Maintenance schedules made visible** — a wall in a Resistance building whitewashed recently, new whitewash margin slightly whiter and less cracked; adjacent section showing older limewash through. Someone is maintaining this building for a reason specific to the current period.
- **The object in the wrong position** — a Court formal dispatch-box in a Syndicate undercroft passage. Not narrative planted — residue of a transaction that happened here; the box arrived and was never removed because removal would require Court entry to Syndicate territory.

#### What Must NOT Be Added

The gothic-game shortcuts Gravenspire categorically excludes:

- **Readable text in the environment as environmental storytelling.** No journal pages, no inscribed notes on walls, no carved dedications as player information. Writing exists because that kind of document would exist in the space; not to tell the player a story.
- **Skeleton-pose tableaux** — arranging bones in suggestive poses to imply a last moment. Dead remains in Gravenspire look like dead remains: collapsed, settled, distributed by physics and scavenging, not arranged for narrative.
- **Object arrangements designed to tell a single legible story** — three objects arranged to imply "a family gathered here" or "a last meal was eaten here" is theater. Objects present for their own reasons; player assembles what they imply.
- **Narrative arrows** — prop placement functioning as spatial guide ("look at this, this is important"). Faction boards and notices are exempt (their diegetic function IS directing attention). Everything else must never be placed to guide player attention.
- **Contrast-emphasis props** — a single candle in an otherwise dark room placed to light a specific point of interest. Light comes from where light sources exist in the world, not where the art department wants the player to look.
- **"Shrine" arrangements** — an object isolated on a surface with clear space around it and appropriate lighting, implying special significance. Objects have significance by virtue of what they are and where they are in the world's history.

#### How Players Learn Environmental Literacy

Gravenspire does not explain its environmental literacy. It trusts repeated exposure to environments where physical consequence is consistent builds pattern recognition without instruction.

- **Through repetition of consistent rules** — every Syndicate undercroft has use-pattern wear at the same functional positions because Syndicate function is consistent. A third Syndicate visit, the player knows where to look.
- **Through the delayed payoff of attention** — the detail that doesn't matter on first visit matters on second because context changed. The Court's temporary shore-post means nothing the first time. After the player learns the Court maintains its spaces at significant resource cost, the shore-post becomes readable as an anomaly — which IS story.
- **Through what the player can verify** — the Resistance building with recent whitewash promises something changed. If investigation finds evidence of recent Collective occupation (Collective material left behind, limewash applied to cover it), the environmental story has been confirmed by forensic investigation.
- **Through NPC behavior as environmental context** — an NPC behaving at a specific function position in a space is the strongest confirmation that the environmental storytelling of that position is correct.

#### Faction-Specific Storytelling Vocabulary

| Faction | Reads Through |
|---|---|
| **Vampire Court** | Precise geometry of maintenance (which surfaces are maintained vs. not — political significance). Interrupted formal process (dispatch-box opened but not sealed). Age contrast at specific positions — replacement of same object type over time |
| **Ghoul Syndicate** | Transaction residue (weight-measurement wear, fold-pattern of documents kept at specific size). Infrastructure improvisation history (temporary-becoming-permanent). Operational marks without explanation (deliberate scoring records activity without identifying it) |
| **Necromancer Academy** | Academic revision history on physical surfaces (palimpsest of ideas chalked and erased, iron-gall ink bleeding through 50-year-old pages). Abandoned inquiry (dust pattern specific to equipment, missing components, result notes cut off mid-notation). Citation chains via physical proximity |
| **Cult of the Pale King** | Devotional deposit accumulation at theologically significant positions (offerings mapping the Pale King's geography). Inscription in oldest stone (marks for reasons legible only within Cult practice, recurring patterns becoming visual language for long-term players). Absence of expected maintenance — the devotional significance of NOT repairing |
| **Haunt Collective** | Palimpsest occupation evidence — physical stratigraphy of successive occupants. Temporary habitation artifacts (cord between points hanging a drying cloth; broken implement repurposed as door prop). The specific ABSENCE of faction ownership marks |
| **Living Resistance** | Human biological evidence (ash in recently-used hearth, food-storage deposits, evidence of children in lower wear patterns and marks at lower heights). Modification for concealment (hidden compartments, false walls visible as false to spatial literacy). Faction material repurposed with original provenance still visible |

#### Anti-Examples — Environmental Storytelling Moves That Violate Pillars

1. **The shrine to a past player's death** — other players' deaths commemorated with items and a note. *Violates P1: the world does not memorialize player events.*
2. **The dungeon journal** — readable document in a haunt explaining history or purpose. *Violates P1: the world does not provide its own annotations. The haunt's history is forensically readable from its surfaces.*
3. **The curated last-moment tableau** — skeletons arranged around a campfire suggesting "a last camp." *Violates P3: faction-agnostic tableau is generic gothic world, not Gravenspire.*
4. **The color-coded corpse** — a body whose clothing communicates story through unusual color emphasis. *Violates P2: staging performs significance rather than letting the player find it.*
5. **The blood-trail to a revelation** — leading the player along a path to a discovery. *Violates P1: this is navigation, not storytelling — directing attention through a world that should be indifferent.*
6. **The single-significant-object on a pedestal** — communicating "this is important" through isolation, lighting, elevated/centered placement. *Violates weight-and-age: significance is earned through material history, not staging.*
7. **The "this faction was here" prop sprinkle** — scattering faction-specific props through non-faction spaces to indicate previous visit/ownership. *Violates inhabited-vs-theater test: if the props are there for narrative communication rather than functional historical reason, they are illustration not evidence.*

#### Section 6.4 — Unity URP Production Guidance

- Environmental storytelling detail (use-wear patterns, staining, occupation evidence) lives primarily in the decal layer (Section 6.2). Environmental storytelling is implemented during environment-dressing, not during base-mesh/texture authoring.
- **Each environment has a storytelling brief** (1-2 paragraphs) identifying: which factions have occupied this space, in what sequence, what activities conducted. The brief is the source document for decal placement and prop selection decisions.
- **Environmental storytelling review gate:** before a space is locked, art director reviews: (1) is every prop explainable by specific person/activity? (2) are there generic atmosphere props? (3) is there any element directing player attention to a specific point rather than trusting investigation?

### 6.5 — Productive Tensions

1. **Causal specificity vs. production time.** Every prop requiring documented use-narrative and every texture requiring physical cause significantly increases art production time. **Resolution:** the storytelling brief system (6.4) is the workflow tool — brief is written before dressing begins, amortizing narrative decision-making to a fast front-end. Brief authoring at scale is achievable; brief-less prop-by-prop justification during dressing is not.
2. **Faction legibility vs. interstitial authenticity.** Clear faction zones read their faction loudly; Collective interstitial is defined by factional indeterminacy. Under production pressure, Collective spaces may be under-developed. **Resolution:** Collective spaces must be explicitly scoped in production planning as requiring higher dressing time than equivalent-size faction spaces.
3. **Environmental storytelling as reward vs. accessibility.** Forensic literacy develops over hours; new players won't read environmental stories at session one — correct and intended. **The tension:** if detail is calibrated purely for veteran literacy, the first three sessions may feel visually inert. **Resolution:** environmental stories at most-trafficked early-game locations (first inn, first city exit to haunt) include at least one story legible with zero context — a physical consequence whose cause is immediate (a door with marks indicating it was forced, not picked). These are not tutorials; they are the first vocabulary lessons.

### 6.6 — What This Section FORBIDS

- **"Gothic atmosphere"** as an artistic brief. The city looks the way it does because of 400 years of specific history, not because it is a Gothic Atmosphere Game.
- **Buildings without documented occupation history.** Every environment has at minimum a 2-3 sentence history note before modeling begins.
- **Uniformly aged surfaces.** Age varies within a single building. Stratum 2 foundation and Stratum 4 addition share the building; they do not share surface treatment.
- **Uniform mortar on pre-Stratum 5 architecture.** Every 400-year-old wall shows 400 years of mortar history.
- **Atmospheric lighting rigs** — no "mood light" placed for player benefit without practical source in the world. If a space needs to be darker, remove light sources. If warmer, add a fire.
- **Atmosphere props** as a category. Cobwebs, candelabras on empty floors, wine goblets on every table. Every prop is a specific object with specific faction vocabulary and specific use history.
- **Skeleton tableau** as environment storytelling. No final-moment bone arrangements.
- **In-world text existing for the player.** Documents and inscriptions exist because the world creates them; they do not exist because the player needs information delivered.
- **Uniform prop distribution.** All prop density is function-clustered, not aesthetically even.
- **Signage functioning as player navigation.** The world does not waypoint itself. Players navigate by spatial literacy, faction-zone legibility, and NPC behavior.
- **Any environment dressing pass without a storytelling brief.** Brief is not optional and is not written after dressing.
- **The word "creepy" as an art direction note.** Gravenspire is beautiful and wrong. "Creepy" directs toward theatrical dread. Correct direction is always specific and material.

---

## Section 7 — UI / HUD Visual Direction

### 7.1 — Layer 1 Visual Style (Practical HUD)

**Character.** Layer 1 is a concession, not an asset. It exists because health and hate cannot safely live on a haunt-interior wall during a corpse run. Every decision here earns its place by being **invisible in practice** — something players stop consciously seeing after three sessions. If a Layer 1 element directs the player's aesthetic attention toward the HUD, it has failed.

**Physical analogy.** The Layer 1 panel does not read as "screen-space UI." It reads as if someone has mounted a narrow strip of architectural ironwork at the periphery of vision — a framing element in the vocabulary of the city's structural details. The eye uses it the way it would use a column or an arch terminus: as a spatial anchor, not as a readout.

**Concrete specification:**

- **Panels:** 1px border in Iron Seam `#3D3A38` at 100% opacity. Fill Iron Seam at 45% opacity. Contrast border-to-fill ~1.1:1 — barely perceivable as two elements. No drop shadow, gradient, rounded corners, or beveling.
- **Panel frames** use the compressed pointed-arch terminus (Section 3.3) at the right end of horizontal bars only. The left end is a clean vertical cut. The arch terminus references Gravenspire's Romanesque-over-gothic layering — structural, not decorative.
- **Icon frames:** square with 45° chamfer all corners, matching floor-tile rotational symmetry. Line weight 1px Iron Seam. No fill.
- **Bars:** 3px height, no glow, no gradient. Health (Render Umber), mana (Pewter Rain), hate (Academic blue-black at 50% opacity, peaking to Rust Iron at max). Bar tracks at Iron Seam 30% opacity — more transparent than the enclosing panel so the bar reads as inside the panel.
- **Spacing rule:** all Layer 1 elements live at screen periphery, minimum 48px from nearest edge at 1080p. Nothing in the center of the screen; nothing near vertical center. Default placement: health/mana lower-left; hate/threat lower-right; spell queue centered-bottom above lower margin.
- **Buttons (invoked from Layer 1 context):** trapezoidal, wider at base, narrower at top — mirrors door-arch geometry. Fill Iron Seam 45%, border 1px 100%, no rounded corners. Width proportioned to label — no fixed-width buttons.

**In a screenshot:** a slim architectural band at lower-left and lower-right — same dark-warm-gray as deep architectural shadow. In a medium-lit exterior or candlelit interior, Layer 1 blends into peripheral architecture. It reads as part of the scene's framing, not as interface superimposed.

### 7.2 — Layer 2 Visual Style (World Information)

**Character.** Layer 2 is not UI. Layer 2 is the world's information system made physically readable. A faction dispatch pinned to the board *is* a faction dispatch — not an interface element resembling one. The player walks to the board and reads the world; no abstraction layer mediates.

**Physical vocabulary — common to all factions:**

Every Layer 2 document is a physical object with: a paper stock (defined per-faction in 7.7), a handwriting style, a mounting method showing use history, and a seal/insignia that is *worn, not crisp*.

**What Layer 2 does NOT have:** UI borders, frames, glowing interaction prompts, "click to read" affordances. The affordance is spatial — the player walks to the board and looks. Close enough to read, the handwriting resolves. Not close enough, it reads as a textured physical object.

**The board as composed object:**
- Base layer of old notices, stained and partially obscured by subsequent layers
- Intermediate notices partially removed, leaving pin-hole clusters and torn corners
- Current notices: the freshest layer, still legibly newer without being crisp
- **Actual physical layering** of document meshes, not flat texture with painted depth. Paper edges, overlapping sheets, different thicknesses — achievable with flat collider plane + layered mesh cards.

**Lighting:** the lamp-over-board state (Section 2 State 9) — slightly warmer, tighter overhead source. 400-600K warmer than ambient; 30-40° cone. Not triggered by player approach. Lamp is always on; the player enters its light.

**Reading resolution:** documents authored so handwriting is legible at 1080p native when player is within 1.5-2m in-world. Beyond 3m, letters don't resolve; the document reads as textured paper. **The game rewards proximity.**

### 7.3 — Dialogue Text Display (UX Resolution — Diegetic Text-Box Panel)

*Resolved decision: Layer-2-style diegetic panel, faction-specific paper.*

During active LLM-driven NPC dialogue (Tier 3+):

- **A faction-appropriate paper-stock panel appears at lower-third of screen** — Vampire Court gray-blue vellum when conversing with a Court NPC; Ghoul Syndicate old-wax parchment when conversing with Syndicate; etc.
- **The panel reads as a document being written as the NPC speaks** — text appears in the faction's handwriting style (Section 7.4) at reading-resolution rate, as if the scribe is recording the exchange.
- **Paper behavior:** slight hand-drawn quality at edges (torn, slightly curled). Mounted appearance — e.g., pinned to an implied board at upper corners when Syndicate; rolled-and-unrolled visual cue for Court dispatches.
- **Response selection:** three or four possible responses as separate sheets or list entries on the same paper, written in the player character's own faction-accumulated hand (Section 5.1 progression — the player's hand develops over time as faction rep accumulates).
- **Exit:** the panel does not slide away. It fades as if dismissed — paper lifted away by a hand at the panel's edge. No UI transitions in the engine sense.

**Why this works:** preserves diegetic rule (panel is paper, not abstract UI) while solving the "where does text live" problem. The panel IS the Layer 2 register applied to a new context.

**Production note:** because the paper panel obstructs ~30% of screen real estate during active dialogue, the NPC must still be visible above it. Camera framing during dialogue holds the NPC head-and-upper-body in the upper 2/3 of frame; the paper panel occupies the lower 1/3. Consistent with Section 5.3's head-and-upper-body dialogue system.

### 7.4 — Faction Standing Display (UX Resolution — Diegetic Personal Journal)

*Resolved decision: A carried journal/ledger that the world writes in on the player's behalf.*

Every player character carries a small leather-bound journal — a physical object in the world, not a character-sheet panel.

- **Opening the journal** is a physical action triggered by a hotkey (default: `J`). The character pulls the journal from a belt pouch; the journal appears in screen-space at reading resolution, held in the player character's hands (animated — the journal is a held object, not a panel).
- **What's written in the journal:**
  - Per-faction pages, one page per faction the player has encountered. Each page is entirely in that faction's handwriting style — the *faction* has written in the journal.
  - Current rep standing is not a number — it's a **descriptive entry**. "You are known to the Vampire Court; Chamberlain Vessik has mentioned your name approvingly" (Rep Tier 1 — Recognized). "Lady Duvessa has admitted you to her inner circle" (Rep Tier 3 — Trusted). The text changes as rep changes.
  - Historical entries remain — when you advance from Recognized to Affiliated, the earlier entry is not erased; it's crossed through with a new entry beneath.
- **The journal's own material state** progresses with the player — as the player accumulates faction rep, the journal accumulates faction marks, stains, additional loose documents slipped between pages. By Rep Tier 4 in a single faction, the journal is physically transformed by that faction's influence.
- **Faction-specific entries do NOT appear until the player encounters that faction** — the journal is empty of a faction until the first interaction.
- **When closed,** the journal returns to the belt pouch; the visual action is brief, reinforcing that the object exists in the world.

**Why this works:** the journal is a Layer 2 diegetic object (paper + handwriting, faction-specific), not a character sheet UI. The player reads their own standing as if reading a document the world has written about them — which they are. This extends the Layer 2 vocabulary rather than breaking it.

**Production note:** requires journal UI implementation with per-faction page templates (6 factions × ~4 rep tiers = 24 authored entry templates minimum) and dynamic text substitution for named-NPC references. Not trivial; flagged as a real production investment but within scope.

### 7.5 — New Player Onboarding (UX Resolution — Veteran AI Companion)

*Resolved decision: A named AI companion (provisionally Sister Elara the Cleric) is pre-assigned to every new player for their first several sessions. Her behavior models correct pacing; no tutorial text.*

**How it works:**

- **First login:** the new player character wakes in an inn room. Sister Elara is already present — a named AI Cleric companion. Her visual register is Rep-Tier-1 Vampire Court affiliation (she's been at this for a long time; her garments show Court material accumulation and specific occupation wear; her postural compression reads Court).
- **She initiates the first expedition through behavior, not dialogue** — she stands, collects her kit, and walks toward the inn exit. The player can follow or not. If they do, she proceeds to a low-stakes entry haunt.
- **In the haunt, she models correct pacing:**
  - Pulls one mob at a time with careful positioning
  - Sits to med between pulls at the natural rest points
  - Casts her buffs and heals in correct sequence
  - Does not rush the player; matches the player's pace but never exceeds the pacing the design calls for
- **She does NOT explain** — no dialogue balloons with tutorial text, no "press X to sit and med." She just sits and meds, and the player observes what happens.
- **After 3-5 sessions,** Sister Elara begins to spend time away from the player — she has her own faction obligations in the world. The player is now expected to navigate alone or with other companions. The training wheels come off without ceremony.

**Why this works:** the pillars prohibit explicit tutorials but permit observation-and-imitation learning. A named AI companion with authentic Gravenspire identity teaches by doing — which is how a new resident would actually learn from a mentor in this world. The mechanism is 100% diegetic.

**Production note:** Sister Elara (or the equivalent named companion) must be implemented as a named-NPC-tier AI (Section 5.2 material resolution) with expanded behavioral logic for the mentoring context. She is the single most important AI companion in the game's onboarding. Her implementation is **Tier 1 templated** (named-NPC visual register, behavior-driven, no LLM dependency per `DECISIONS.md` D003 and D004). The full AI-companion surface — autonomous decision-making, LLM-driven dialogue, and persistent companion state across sessions — is deferred to Tier 2+. The T1 templated mentor is load-bearing for the onboarding feel; the T2+ promotion is load-bearing for the wider companion system. Resolved 2026-05-15 by AD-ART-BIBLE sign-off pass finding F-05.

**Explicit corollary:** Sister Elara's departure from the player's side after 3-5 sessions is *the ending of the onboarding phase*, marked by her behavior (not by a UI message). The player notices she's no longer in the hiring hall when they return — she's been recruited by another player, or she's pursuing her own Court business. The onboarding ending is narrative consequence, not a progression gate.

### 7.6 — Typography

**Primary Interface Font (Layer 1 HUD labels, numeric readouts, system text):**

Compressed, geometric, narrow proportions consistent with pointed-arch structural logic. Working inscriptional face — the nearest historical model is medieval lapidary inscription (text cut into stone by someone who had to plan letter spacing in advance because there was no undo). Letter-forms resolved and specific; nothing casual, handwritten, ornate, or display-weight.

**Personality test:** this font is what you would find carved into a threshold stone or cut into a metal instrument-plate — identifying function, not communicating personality. Reads as "the readout of a system the city already had before the player arrived," not as "the UI of this video game."

**Weight hierarchy:** two weights only.
- Regular (labels, descriptors, context text): 30-32px at 1080p, compressed proportions, 1.1-1.2 cap-to-x-height ratio.
- Medium (numeric values, critical state labels like "DEAD", "PULLING"): same face, one step heavier, 34-36px. Heavier weight signals live readout vs. static label — no color change, no size jump.

**No italic. No oblique.** Italic communicates rhetorical emphasis, which the interface does not take.

**Sizing discipline:** max Layer 1 font size 36px at 1080p; min 24px. Scale linearly with viewport height.

**Secondary Display Font (Layer 2 documents, faction notices, diegetic text):**

Not a single font — a family of handwriting traditions. Each faction's writing is defined in 7.7. **Institutional age determines formality, not hierarchy.** No faction writes casually except Living Resistance. Handwriting expresses each faction's relationship to time, permanence, and audience.

**Body text vs. display text hierarchy for Layer 2:**
- Display (names, notice headers, NPC names): full faction hand, 16-20pt equivalent, fully resolved letterforms.
- Body (notice content, communique body): same hand, 12-14pt equivalent, letter-forms slightly compressed (same person writing faster, not a different font).
- Marginalia (annotations, dates, cross-references added by subsequent readers): 8-10pt equivalent, variable pressure — lighter where uncertain, heavier where correcting.

**Production scope flag:** faction handwriting can be executed as custom font assets, hand-lettered texture assets per document, or hybrid. Spec works for all three approaches. Producer/technical-artist conversation required at implementation time.

### 7.7 — Per-Faction Layer 2 Visual Variation

Executable spec per faction — paper stock, handwriting, seal, mounting. An artist should be able to execute documents from this spec without art director present.

#### Vampire Court
- **Paper stock:** gray-blue **vellum** (animal-skin-derived, not paper) — thinnest in the city; almost translucent at thinnest sections, warmer at natural thicker areas. Base color: Court primary `#8A9BA8` at very low saturation. Dimensions consistent (cut to a specific rectangle used for 150 years with a knife guided by a metal rule). No torn edges. Age staining only at corners and fold-lines.
- **Handwriting:** formal chancery hand — 15th-century Italian cancellaresca formale. Controlled pen angle (~45°), thin strokes exactly 1/4 of thick. Ascenders and descenders extended but controlled. No corrections visible. Ink: cool near-black approaching `#2A3040`, slightly oxidized toward blue.
- **Seal:** square wax impression (not round — Court geometry is rectilinear). Arch-and-lintel motif compressed into 15×15mm. Gray-blue wax, oxidized at edges. Age in wax craze-cracking, not motif degradation.
- **Mounting:** arrives rolled and wax-sealed. On the board: unrolled, secured with two small iron pins at upper corners, consistent pin size/spacing (board handler follows a protocol). Edges curl slightly from having been rolled.

#### Ghoul Syndicate
- **Paper stock:** old-wax **parchment** — rough-fiber, heavily handled with oily/tallow-stained hands, slightly waxy to touch, slightly translucent in places from lipid absorption. Color: Syndicate Old-wax ochre `#9A7B42` at low saturation. Dimensions vary. Corners torn or cut without rule. Fold-history visible.
- **Handwriting:** cramped operational cipher — 14th-century Anglicana cursive crossed with merchant cipher abbreviation. Tight letter-spacing, abbreviated word-forms, heavy pen pressure (thick uniform strokes without hairlines). Corrections struck through with single hard horizontal line, amended above.
- **Seal:** stamped iron die pressed directly into parchment without wax. Motif: a horizontal ruled line with tally-mark count beneath. Never quite level — tool shows use wear. Placed at lower-left (hand-rest position).
- **Mounting:** stabbed with a single heavy iron pin, rusted from board exposure, at upper center or offset. New notices layered over old; Syndicate doesn't retire old notices.

#### Cult of the Pale King
- **Paper stock:** rough unbleached **linen paper** — oldest, crudest in the city, deliberately chosen. Rough fiber without significant bleaching. Base color between Bone Pale `#D4CCBC` and Mortared Chalk `#C8C4B8`. Fiber direction visible. Dimensions irregular — Cult sheets are not ruled into commercial dimensions.
- **Handwriting:** backward-leaning devotional script — pre-Caroline minuscule with deliberate archaizing. Backward lean 5-10° (against normal right-handed writing). Uniform weight (reed pen, not broad-edge — the Cult doesn't use craft-guild tools). Letters individually formed, no ligatures. Marginal prayer-mark repetition.
- **Seal:** stone stamp impressed multiple times (three or four impressions). No ink — impression alone. Circle-bisected-vertical motif (pre-writing symbol). Pressure varies between impressions.
- **Mounting:** **no pins.** Cult documents are placed, not pinned — rest against vertical surfaces held by fold or tuck; laid flat on horizontal surfaces. If fallen, lie where they fell. No Cult member re-posts fallen documents.

#### Necromancer Academy
- **Paper stock:** formal academic **parchment** — second highest quality in the city. Opaque, substantial. Color: Aged parchment `#C4B48A`. Surface: smooth, slight tooth. Standardized dimensions (Academy archives everything; consistent size simplifies storage). Corners square, maintained. Progressive yellowing toward warm, darker at handled margins.
- **Handwriting:** academic iron-gall precision hand — 15th-century humanistic minuscule. Upright, regular, clear letterforms. Most legible hand in the city. Standard scholarly abbreviation only (`&c.`, `p-with-stroke`). Systematic annotation in margins — numbered cross-references, underscoring of key terms, insertion marks. **Light graphite or stylus guidelines visible beneath text** (not erased — erasure would damage parchment).
- **Seal:** academic blue-black ink stamp on raised paper wafer (not wax). Stamp level and precisely placed — double-rule line enclosing abbreviated Roman-numeral citation (the Academy's own archival reference). Ink oxidizes blue-gray over time.
- **Mounting:** two small iron pins at upper corners, smaller than Syndicate pins (Academy uses a consistent small-pin standard). Single horizontal fold-mark at mid-document (from envelope transport).

#### Haunt Collective
- **Paper stock:** **no consistent stock.** Communications produced on whatever is available — Syndicate parchment fragment, reverse of Academy document, Resistance linen paper scrap. What IS consistent: the paper shows prior use (read-through text from the other side, previous fold-lines, staining not from current content).
- **Handwriting:** **no consistent hand.** At least two hands visible on most Collective communications — the Collective is plural. Ghost layer of prior hand bleeds through under active layer.
- **Seal:** none, or **defaced** (prior seal broken but present; prior stamp over-stamped with a second different impression). The defacement is visible and intentional.
- **Mounting:** improvised and inconsistent. Wedged under another notice's pin. Adhered with candle wax (not seal wax — a candle smeared on the board as adhesive). Folded into a triangle and wedged into a crack.

*Production note:* Collective is deliberately harder to produce than the other five factions. Collective's identity IS indeterminacy — specified as a feature, not a bug. This is the right production call; the Collective spaces are explicitly scoped in production planning as requiring higher dressing time.

#### Living Resistance
- **Paper stock:** rough linen **paper** — commercial grade available to trade-district residents pre-faction specialization. Color between Undyed linen `#C8B898` and Bone Pale `#D4CCBC`. Texture rougher than Academy, smoother than Cult. Commercial dimensions but torn/cut with varying care. Some notices on the inside of unfolded commercial envelopes — reverse shows original printed commercial lettering.
- **Handwriting:** practical working hand. Multiple contributors common; variation between writers unashamed. Direct, without headings or formality. Inline corrections. The notice communicates and stops.
- **Seal:** **none.** Authentication by recognition. May carry a small personal mark — not a seal, but a specific pen-mark or initial that Resistance members in the same cell recognize. 1-2cm, simple, meaningless to outsiders.
- **Mounting:** any available method. Standard iron pins; folded corner tucked under another notice; cord through a hole punched in upper corner. Pragmatism, not protocol.

### 7.8 — Iconography

**Register:** outlined monochrome illustration at instrument-plate line weight. Not flat filled (too contemporary — no material analog in the world). Not photorealistic (unachievable at icon size). Not cartoon-outlined (communicates lightness the tone doesn't support).

**Correct register:** line illustration of a 15th-century instrument-plate or manuscript marginal diagram. Single-line-weight outline (1.5-2px at 100% size, normalized to equivalent weight at icon resolution). Interior detail in shorter secondary lines at the same weight, not fills. **The icon is a drawing, not a silhouette** — information lives in the lines, not in the shape's mass.

**Line weight language:** all icons have outer stroke 1.5px at 64×64, interior detail 1px same size. No variable weight within a stroke (no calligraphic swell — that belongs to handwriting registers). Consistent monochrome: Iron Seam lines on transparent ground.

**No colored outlines. No faction-colored fill. No emission halo. No glow on any icon state.**

**Icon content:**
- **Spell queue icons** identify spell type through the physical analog — vertical hand-position for arrested gesture; crossed hands for a ward; open horizontal palm for projectile. No elemental color conventions. No rarity shine.
- **Status effect icons** identify condition through physical symptom — tilted figure-outline for slow/root; horizontal line through icon's middle for silence. Compressed illustrations, not invented ideograms.
- **Faction icons** for hate/threat context carry faction-inflected frame modification (below).

**Faction-specific icon frame modifications** (the only faction variation in Layer 1 iconography):
- **Court:** 45° chamfer frame + hairline secondary border (0.5px) set 2px inside outer border — recessed-surround effect
- **Syndicate:** chamfer frame + notch at lower-right corner (ledger-page corner-clip)
- **Academy:** chamfer frame + ruled baseline (1px horizontal) at lower 1/4 of frame interior — as if the icon was written on lined paper
- **Cult:** chamfer replaced by slightly irregular, slightly heavier-weight hand-drawn-approximation border (2px, not perfectly ruled)
- **Collective:** no consistent modification — frame shows damage (missing segments, partial frame only)
- **Resistance:** chamfer frame + single notch at upper-left corner only — corner-fold mark

**Faction seals** (Layer 2, not Layer 1 — per 7.7).

### 7.9 — Animation Feel

**Governing constraint: Stillness Is The Signal.** Every animation is accounted for by a specific mechanical need. **The operative test:** "What information does this motion carry that the static state does not?" If the answer is "none — it just feels more alive," the animation is cut.

**Health bar change:**
- **On damage:** snap to new value on same frame as damage event. No damage-flash, no shake, no elastic overshoot. The bar is shorter. That is all.
- **Why snap, not tween:** a tweening bar implies damage still in transit — the animation performs the hit. Gravenspire's damage is done; the bar reports the result.
- **On death approach (<20% health, Bone Pale transition):** in active combat only — a very low-amplitude pulse animation, opacity oscillates between 75% and 80% at **0.7Hz**. Almost-imperceptible — felt as much as seen, below threshold of conscious noticing but above threshold of invisibility. The one animation explicitly permitted to reference biological urgency without performing alarm. Outside combat: static Bone Pale, no animation.
- **Corpse-run state:** bar at zero, static. HUD exempt from world-desat pass (Section 4.4) — bar retains Render Umber hue against desaturated world. The static HUD against gray-witnessed world is itself a visual statement.

**Mana drain:** snap-to-value on spend. No tween, no sparkle. At depletion: static gray, no animation. **During med break meditation:** 1:1 linear fill — bar grows from current to full over the actual duration. Not animation for feel — literal readout of the med process. If meditation takes 45 seconds, the bar takes 45 seconds. No ease. No satisfying ping.

> **Production dependency flag:** mana-restore linear fill depends on med-break mechanics (duration, restore curve) which are not yet specced. May need placeholder until combat GDD resolves this.

**Hate indicator:** snap-to-value on aggro change. **At maximum (Rust Iron transition):** the hate indicator shape pulses at **1.5Hz** per colorblind accessibility (Section 4.6). **Shape-outline pulse** (icon frame border expands 1px and contracts), not a brightness pulse. The one place where animation is justified because information — you are pulling aggro — has high enough stakes to warrant conscious interruption of Stillness. Below max: pulse stops immediately (no transition out).

**Panels and windows (Layer 1 secondary):**
- **On open:** 120ms linear reveal — panel frame draws from top-left corner to full dimensions, as if being ruled. Not a fade, not a scale-from-center. **A ruled drawing motion**, because the panel is an architectural element being inscribed into space.
- **On close:** reverse — frame erases from bottom-right to top-left (retracing the rule). 100ms. No ease-in/out. Mechanical, not organic.

**Layer 2 documents:** no UI animation. Player approaches; document is there. Player leaves; document remains. **Layer 2 is the world; it does not animate for the player.**

**Menu transitions:** same 120ms ruled construction applied to full screen frame. Previous screen immediately replaced (no dissolve, slide, wipe). All typography at full opacity at end of frame construction — text is inscribed once the surface exists.

**Hover:** element border weight increases from 1px to 1.5px. No color change, no glow, no scale. Immediate (0ms) on entry and exit — hover is a current-state readout, not an animation.

**Selection (committed interaction):** fill opacity increases from 45% to 65% for 80ms, then returns to 45%. Not a flash — brief density increase distinguishing "you acted on this" from "you looked at this."

**Active / queued state:** fill opacity holds at 65% for as long as the state persists. Returns to 45% when state clears.

**Death state — does the HUD animate differently?** **No.** HUD is static at zero health. No special death-state animation. The punishment is delivered through the world's desaturated appearance, through the corpse visible at distance, through the walk back. **The HUD does not participate in the drama.** That is the correct emotional register.

### 7.10 — Testable Thresholds *(flagged by ux-designer for validation)*

These are measurable UX requirements the visual direction must respect:

- **Group frame bar readability:** 6 simultaneous health bars (player + 5 party members) at the 40-60% HUD opacity floor must remain discriminable under combat-stress scanning. *Validate in prototype.*
- **Pewter Rain / Iron Seam contrast:** must maintain ≥3:1 luminance contrast for tritanopia accessibility (Section 4.6). Confirm in HUD implementation.
- **1px border legibility at 4K:** test the 1px architectural borders at 4K resolution — may need to scale to 1.5-2px at 4K for equivalent perceptual weight.
- **Dialogue panel obstruction test:** with the faction-specific dialogue panel occupying lower 1/3 of screen, verify NPC head-and-upper-body framing in the upper 2/3 remains cinematically readable.
- **Journal accessibility:** verify hotkey `J` is remappable, journal text is scalable for low-vision, reduced-motion option available for journal open/close animation.

### 7.11 — What This Section FORBIDS

**Layer 1 / Screen-space HUD:**
- Rounded corners on any HUD element. Zero tolerance — not even 1px radius
- Drop shadows on any Layer 1 element
- Gradient fills anywhere in Layer 1
- Glow, bloom, emission on any bar, icon, or frame
- Animation outside the specific cases defined in 7.9
- Font sizes outside 24-36px range at 1080p
- Italic or oblique type anywhere in the interface
- Color variations on icons beyond the single faction-frame modification
- Any UI element in vertical center of screen or center horizontal band (40-60% viewport height)
- Smooth-tween bar updates — all bars snap to value
- Any HUD element participating in world-desat pass (HUD on Overlay camera, isolated)
- **The word "satisfying" as a design brief for HUD animation** — satisfaction is not the register

**Layer 2 / Diegetic documents:**
- Abstract UI framing on any Layer 2 element
- "Click to read" prompts, interact-indicators, or UI affordances on documents
- Documents on consistent white/neutral paper stock — faction stock is mandatory
- Crisp wax seals or fresh-pressed stamps — every seal shows age consistent with document's inferred age
- Documents without faction attribution visible in material alone (if faction cannot be read from paper + handwriting before text content, the document fails)
- Any Layer 2 element using Layer 1 typography — the two layers have entirely separate type systems

**Dialogue (new in 7.3):**
- Floating speech bubbles
- Dialogue text in Layer 1 abstract register
- Response-selection UI in abstract button style (responses must be on the diegetic paper panel in the player's own hand)

**Journal (new in 7.4):**
- Abstract tier labels ("Tier 3," "Trusted") displayed as UI text — all tier information is narrative text in faction handwriting
- Progress bars for rep within a tier — there are no rep bars
- Character-sheet style stat grids inside the journal — it is a journal, not a stat screen

**Onboarding (new in 7.5):**
- Tutorial text overlays on the HUD
- "Press X to med" prompts
- Forced tutorial gating (player cannot leave Sister Elara's area until they complete a tutorial)
- Sister Elara speaking mechanic instructions

**Both layers:**
- True black (`#000000`) anywhere in the UI system
- True white (`#FFFFFF`) anywhere in the UI system
- Any animation without a specific mechanical information-carrying justification
- Faction colors at full digital saturation
- Red as a UI signaling color in any context
- Green as a UI signaling color in any context

---

## Section 8 — Asset Standards

Section 8 combines art-direction decisions (file formats, naming, resolution tiers, export settings) with technical-artist hard constraints (polygon budgets, texture memory, draw calls, URP importer rules, validation pipeline). Conflicts between the two have been resolved explicitly.

> **Engine version note:** Unity 6.3 LTS is beyond the LLM knowledge cutoff (~6.0). APIs in this section — URP Decal Projector performance, GPU instancing behavior, occlusion culling bake workflow, SRP Batcher compatibility, BC5/BC7 compression pipeline — must be verified against `docs/engine-reference/unity/VERSION.md` or official Unity 6.3 documentation before implementation. Budget math is stable; API implementation details are not.

### 8.1 — File Format Philosophy

Every format decision follows one principle: **preserve the information that took the most time to author.** The worst failure mode is losing 50 years of hand-authored soot accumulation to a compression artifact.

| Asset Type | Source/Working | Delivery | Notes |
|---|---|---|---|
| **3D meshes (characters, rigged)** | Blender `.blend` | FBX | Y-up, apply all transforms before export, export normals from mesh (not recomputed), embed media off. |
| **3D meshes (static props, environment)** | Blender `.blend` | FBX | Same settings. glTF 2.0 acceptable as secondary export for rigless static; revisit for primary when Unity glTF support matures. |
| **Textures — masters** | PNG 8-bit (16-bit where applicable) | PNG | Lossless. Universal. Retains full channel info. **PSD/Substance source files version-controlled alongside but never delivered to Unity.** |
| **Textures — HDR (sky dome, lightmap sources)** | EXR 32-bit float | EXR | Linear. Preserves luminance range for correct ambient contribution. **Reserved for sky dome + baked lightmap sources only — not runtime textures.** |
| **Normal maps** | PNG 8-bit | PNG | Sufficient for the wall-bow and stair-wear detail this project uses. |
| **Animations** | Blender actions within character `.blend` | FBX with baked keyframes | Bake before export — raw rig curves produce driver/constraint errors on import. |
| **Audio references** | WAV 48kHz/24-bit master | Format owned by audio system | Final audio format decisions with technical-artist. |

**Explicitly forbidden:** JPEG in any asset category (lossy artifacts corrupt surface-history textures). TGA (no production advantage over PNG). FBX with embedded textures (creates duplicate assets and version-control conflicts). Source files (PSD, Substance) delivered as Unity imports.

### 8.2 — Naming Conventions

Asset names are the production search system. Every component of a name is a filter that narrows the result set. Unreadable or ad-hoc names are a multi-year production liability for a solo dev.

**Master pattern:** `[category]_[descriptor1]_[descriptor2]_[variant].[ext]`

**Separator rules:** underscore only. No hyphens (ambiguous in path parsing), no spaces, no camelCase. All lowercase. No abbreviations beyond the established short-codes below.

**Established short-codes — and only these:**

| Short-code | Meaning |
|---|---|
| `vc` | Vampire Court |
| `gs` | Ghoul Syndicate |
| `na` | Necromancer Academy |
| `cpk` | Cult of the Pale King |
| `hc` | Haunt Collective |
| `lr` | Living Resistance |
| `neu` | Neutral (no faction) |
| `pc` | Player character |
| `npc_named` | Named NPC tier |
| `npc_amb` | Ambient NPC tier |
| `s1`–`s5` | Architectural stratum 1-5 per S6.1 |
| `yr20`, `yr50`, `yr200` | Age tiers (0-20yr, 20-50yr, 50-200yr) |

**No new short-codes without revising this table.**

#### Per-Category Naming

**Characters:** `char_[type]_[faction]_[role]_[state].[ext]`
- Examples: `char_npc_named_vc_attendant_lod0.fbx` / `char_pc_neu_cleric_lod0.fbx` / `char_npc_amb_gs_fence_lod2.fbx`
- Character textures: `char_[type]_[faction]_[role]_[map-type].[ext]` — map-type: `alb`, `nrm`, `rgh`, `mtl`, `msk`, `ems`. Face textures get explicit `_face_` insertion (e.g., `char_npc_named_vc_attendant_face_alb.png`).

**Environments — tileable architecture:** `env_arch_[material]_[stratum]_[faction]_[age-tier]_[map-type].[ext]`
- Examples: `env_arch_stone_ashlar_s3_vc_yr200_alb.png` / `env_arch_timber_lap_s4_gs_yr50_nrm.png`

**Environments — unique surfaces (non-tileable):** `env_arch_uniq_[location-slug]_[map-type].[ext]` — Budget cap enforced as searchable audit target.

**Environments — structural meshes:** `env_mesh_[type]_[stratum]_[faction]_[variant].[ext]` — type: `wall` / `arch` / `doorframe` / `floor` / `ceiling` / `stair` / `column` / `window`.

**Props — major:** `prop_maj_[faction]_[object]_[variant].[ext]` (e.g., `prop_maj_vc_dispatch_box_01.fbx`)
**Props — minor (atlas-packed):** `prop_min_[faction]_atlas_[set-slug].[ext]`

**Ground surfaces:** `env_ground_[material]_[context]_[faction]_[map-type].[ext]` — context: `street` / `courtyard` / `interior` / `haunt`.

**Decals (surface history layer):** `dcl_[cause]_[faction]_[variant].[ext]` — cause: `rust` / `soot` / `moss` / `mineral` / `waterrun` / `bloodstain` / `mortar_loss` / `wax`. **Every decal must name its physical cause — enforces the Section 6.2 "cause test."**

**VFX:** `vfx_[type]_[context]_[faction-or-neutral]_[variant].[ext]` — type: `particle` / `decal_dyn` / `ribbon` / `sprite`; context: `spell` / `combat` / `ambient` / `death` / `weather`.

**Animations:** `anim_[character-slug]_[state]_[variant].[ext]` — state: `idle_01` / `walk` / `combat_pivot` / `combat_hold` / `dialogue_engaged` / `dialogue_considering` / `dialogue_dismissive` / `dialogue_cautious` / `medbreak_sit` / `medbreak_rise`.

**UI Layer 1:** `ui_l1_[element]_[state].[ext]` — element: `panel` / `bar_health` / `bar_mana` / `bar_hate` / `btn` / `icon_[spell-slug]` / `icon_status_[effect]` / `frame_[faction]`.

**UI Icons (standard):** `ui_icon_[category]_[subject]_[size].[ext]` — category: `spell` / `status` / `faction` / `action`; size: `32` / `64`.

**Layer 2 documents (diegetic):** `doc_[faction]_[type]_[content-slug]_[age-tier].[ext]` — type: `dispatch` / `notice` / `journal_page` / `seal` / `paper_stock`.

**Audio (reference tags; formal audio system owns):** `sfx_[context]_[faction-or-neutral]_[description].[ext]` / `amb_[zone]_[faction]_[state].[ext]`.

**Concept art / reference (pipeline only — not shipped):** `ref_[category]_[subject]_[variant].[ext]`.

### 8.3 — Consolidated Texture Resolution Table

Single authoritative source. Supersedes any inconsistency in earlier sections.

| Asset Category | Albedo | Normal | Roughness | Metallic | Notes |
|---|---|---|---|---|---|
| **Player character — body** | 1024² | 512² | 512² | 256² | Per S5.4 |
| **Player character — face** | 2048² | 1024² | 512² | — | Portrait quality; SSS uses albedo channel |
| **Named NPC — body** | 1024² | 512² | 512² | 256² | Per S5.4 |
| **Named NPC — face** | 2048² | 1024² | 512² | — | Per S5.4 |
| **Ambient NPC — full** | 512² | 256² | 256² | — | Per S5.4 |
| **Creature enemy** | 512² | 256² | 256² | — | Standard PBR only |
| **Architecture — Primary (facade)** | 1024² | 1024² | 512² | 256² | Tileable 1:2m. See mipmap bias note in 8.5. |
| **Architecture — Secondary (interior wall, ceiling)** | 512² | 512² | 256² | 128² | Tileable |
| **Architecture — Unique surface** | 2048² | 2048² | 1024² | 512² | Non-tileable. **Budget cap: 3-4 per Addressable streaming group** (see 8.7 for zone definition). |
| **Ground — Street/courtyard** | 1024² | 1024² | 512² | — | Tileable 1:1.5m. Wear via vertex-painted overlay. |
| **Ground — Interior floor (standard)** | 512² | 512² | 256² | — | — |
| **Ground — Interior (polished, Court marble)** | 1024² | 1024² | 512² | 256² | Metallic channel for specular response |
| **Props — Major** | 512² | 256² | 256² | 128² | Individual UV |
| **Props — Minor (atlas-packed)** | 256² | 128² | 128² | — | Atlas-packed in sets of 8-16 |
| **Sky / Exterior dome** | 1024×512 panoramic | — | — | — | EXR 32-bit float |
| **VFX — Particle sprite sheet** | **256² maximum** | — | — | — | Gravenspire VFX is minimal by design (S1, S4). 256² cap enforces Stillness Is The Signal materially. |
| **VFX — Decal (dynamic)** | 256² | 128² | — | — | Dynamic decals are small by definition — specific physical events |
| **Layer 1 HUD — panels, bars, frames** | 128² (if rasterized) | — | — | — | Prefer vector or shader-drawn. If raster, 128² max |
| **Layer 1 HUD — Icons (standard)** | 64² | — | — | — | Iron Seam line art on transparent ground |
| **Layer 1 HUD — Icons (large/highlighted)** | 128² | — | — | — | — |
| **Layer 2 — Paper stock (faction doc base)** | 512² | — | 256² | — | Roughness required for tactile plausibility; no normal map (paper is not displacement) |
| **Layer 2 — Faction seal** | 128² | — | — | — | Read at <1.5m in-world |
| **Layer 2 — Handwriting texture (if raster)** | 512² | — | — | — | Hand-letter asset path |
| **Layer 2 — Full document (journal page, major dispatch)** | 1024² | — | 256² | — | Players read at 1.5-2m in-world |

**Ratios to preserve (non-negotiable even if budgets scale):** Named NPC face = 2× body albedo. Architecture unique surface = 2× primary facade. VFX cap at 256² (material enforcement of S1). Layer 2 full document = same budget as primary facade.

### 8.4 — Polygon Budget Validation *(tech-validated against Unity 6.3 URP + GTX 1070 min-spec / RTX 4070+ target)*

#### Character Budgets

| Tier | LOD0 Tri | Verdict |
|---|---|---|
| Player | 18,000 | **ACCEPTED** — player is one of the smallest contributors; SSS is the real cost, not polys. |
| Named NPC | 16,000 | **CONDITIONALLY ACCEPTED** — LOD0 only inside 5m. LOD transition distances (S5.4) are performance infrastructure, not artistic preference. |
| Ambient NPC | **6,000** | Revised down from 7,000 to align with S5.2 stated range and preserve the ~2× ratio (16k/6k = 2.67×). |
| Creature enemy | 8,000-12,000 | **ACCEPTED** — 8k default; reserve 10-12k for boss-tier single-visible encounters. |

#### New Environment Budgets

| Asset | LOD0 Tri | Notes |
|---|---|---|
| Architecture module — Primary (facade wall, arch, tower section) | 800-1,500 | Modular. Complexity in texture, not geometry. |
| Architecture module — Secondary (interior wall, ceiling section, floor tile) | 200-600 | Tiling expected. All detail in normal map. |
| Assembled city facade (10-15m span) | 4,000-8,000 | Assembled from primary modules |
| Assembled haunt interior wall (4-6m span) | 2,000-4,000 | Occlusion culling does the culling work |
| Hero prop (major furniture) | 500-1,200 | Individual UV |
| Standard prop (minor furniture) | 150-500 | Atlas-packed |
| Minor prop (candle, book, implement) | 30-120 | Atlas-packed in sets of 8-16 |
| VFX decal / wound decal | 2-8 | Quad or minimal mesh |
| VFX 3D particle (exceptional) | 4-16 per particle | 3D particles are the exception |

#### Scene Polygon Ceilings

| Scene | Estimated Total | Status |
|---|---|---|
| **City Hub — worst case 50 concurrent + 15 named + 40 ambient + architecture** | **~1,110,000 tri** | Within GTX 1070 sustained rendering at 1080p/60fps. Triangle count is not the bottleneck — draw calls are. |
| **Haunt Zone — 5 players + 8 named + 15 ambient + 6 creatures + props** | **~660,000 tri** | Comfortable. Draw-call count from high-density props is the real risk. |

### 8.5 — URP Importer Constraints

**Color space:** all albedo/diffuse textures flagged **sRGB**. All non-color data (normal, roughness, metallic, mask, AO) flagged **Linear**. EXR sources Linear 32-bit float. **Incorrect color-space flagging silently destroys the faction color calibration in S4 — most common and most catastrophic solo-dev import error.**

**Normal map convention:** Unity uses **DirectX convention** (Y+ up in tangent space). Blender/Substance default is OpenGL. Either flip green channel at export (Substance DirectX preset handles automatically) OR flag "Fix Now" in Unity's normal map importer. **Convention must be consistent across every tool — document the chosen approach (export-time flip preferred).**

**Compression requirements:**
- **Albedo opaque:** BC7 (DXT5 fallback if BC7 pipeline unavailable). BC7 is correct for hand-authored PBR albedo with subtle color history. DXT1 loses too much fidelity.
- **Normal maps:** BC5 (two-channel RG, reconstruct B in shader). Not BC3/DXT5 — BC5 preserves precision on fine mortar-line detail.
- **Roughness/Metallic packed mask:** BC7 packed (R=roughness, G=metallic, B=AO). Standard URP "mask map" workflow.
- **Albedo with alpha (decals, cutouts):** BC7 RGBA.
- **Face albedo (2048):** BC7 — preserves skin tone transitions DXT1 would posterize.

**Mipmap policy:**
- **ON:** architecture tileable surfaces, character textures, VFX particle sheets
- **OFF:** Layer 1 HUD elements (1px architectural borders are sharpness-critical), Layer 2 document mesh cards (consistent in-world scale), sky dome
- **Mipmap bias for faction legibility:** architecture primary albedo needs negative bias (~-0.5 starting value) to preserve faction-silhouette legibility at 30m. If -0.5 insufficient, increase base resolution to 2048 for primary exterior facade tiles, not just landmark unique surfaces.

**Texture size:** strictly power-of-two (64, 128, 256, 512, 1024, 2048, 4096). Non-POT textures cannot be compressed with BC7/DXT and will not mipmap correctly. Sky dome panoramic 1024×512 is valid (both dimensions individually POT). **Non-POT is an immediate import rejection.**

**Alpha channel policy:** only textures that genuinely require alpha should retain it. Opaque albedos imported as RGB (no alpha) then compressed BC7 RGB. Unnecessary alpha forces BC7 RGBA mode, increasing memory by ~33%.

### 8.6 — Material Slots & Draw Call Budgets

**URP on GTX 1070 sustains approximately 1,500 draw calls/frame at 60fps / 1080p** before CPU becomes the bottleneck. SRP Batcher reduces per-draw CPU cost but does not reduce draw call count.

| Character Type | Max Material Slots | Justification |
|---|---|---|
| Player character | 4 | Body (faction-shared), face (unique), hair/hood, weapon/carried. No more. |
| Named NPC | 4 (5 max with AD approval) | Body, face, signature garment/prop, optional secondary material if design requires |
| **Ambient NPC** | **2 — hard limit** | 1 body (faction-shared, GPU-instanced with same-faction ambient NPCs), 1 face (faction-variant). **This 2-material limit + faction sharing is the primary draw-call lever in city hub scenes — non-negotiable performance constraint.** |
| Creature enemy | 2-3 | Standard: 2 (body + biological). Boss: 3 max. |

**City Hub draw call breakdown (optimized):**

| Source | Draw Calls | Notes |
|---|---|---|
| Ambient NPCs (40-60, GPU instanced by faction) | **12** | 6 factions × 2 materials. Only works if ambient bodies are truly faction-shared. Unique textures break instancing. |
| Named NPCs (15, non-instanced) | 60 | Unique faces prevent face instancing; bodies may share |
| Player characters (20) | 80 | Faction-shared bodies, unique faces |
| Architecture (static batched) | 150-250 | Tileables batch aggressively; uniques don't (expect 30-40 DC for uniques) |
| Props (instanced where repeated, batched otherwise) | 100-200 | Depends on atlas quality |
| Decals (URP Decal Projector — see 8.7) | 50-150 | The variable; see zone rule |
| Shadows, depth prepass, UI | 200-300 | URP required passes |
| **Total** | **650-800** | **Within 1,500 budget with headroom** |

### 8.7 — Resolved Production Conflicts

#### Zone Definition (was undefined — now locked)

**A "zone" is defined as an Addressable streaming group boundary.** Not a haunt room, not a whole district — the memory-streaming unit. The "3-4 unique surfaces per zone" cap in S6.2 applies to Addressable groups. Flag for `unity-addressables-specialist`: configure streaming groups with explicit memory budgets; target ≤350MB texture memory resident at any point during normal play.

#### Haunt Collective — Cross-Faction Atlas

**Resolution locked: cross-faction Collective atlas.** A dedicated Collective atlas (1024×1024 sheet) packs props from multiple factions together at slightly reduced per-prop texel resolution. Preserves 14-22 prop density while halving draw-call count. Texel tradeoff is acceptable because Collective rooms are visually dense by design and forensic detail is read at close range where mip quality is highest.

**Naming exception:** Collective atlases use `prop_min_hc_atlas_crossfaction_[set-slug].[ext]` — the only atlas permitted to pack cross-faction material.

#### Exterior Architecture — Bake History Into Tile Textures

**Resolution locked:** history detail for exterior surfaces is **baked into tile albedo/roughness at authoring time**, not applied via runtime URP decal projectors. Decal Projectors are **reserved for interior spaces and close-range interaction zones** (haunt interiors, inn rooms, faction-specific indoors).

For complex curved exterior surfaces where tile baking is insufficient, **Decal Meshes** (flat quads placed against surfaces) are permitted — cheaper than Decal Projectors, work correctly on flat or gently curved geometry.

#### Named NPC SSS Verification

URP screen-space subsurface scattering is expected to be **flat cost** (1-2ms full-screen pass regardless of character count) on GTX 1070. This is favorable for Gravenspire's named-NPC density. **Flag for `unity-shader-specialist`** to verify URP 6.3 SSS implementation is screen-space and not per-draw-call before skin shader authoring begins.

### 8.8 — Asset Validation Pipeline

Automated validation runs as Unity Editor tool (AssetPostprocessor) and pre-commit step. All blocking checks must pass before asset is accepted into repository.

**Blocking checks (asset rejected on failure):**

| Check | Rule |
|---|---|
| Polygon count | LOD0 must not exceed category budget (8.4). LOD1/2/3 at or below % reductions specified |
| Texture dimensions | Power-of-two both axes. Must match category budget (8.3). |
| Texture type setting | `*_nrm` / `*_Normal.*` must be TextureImporterType.NormalMap; non-normal must not be imported as Normal type |
| Material count | Character prefabs: SkinnedMeshRenderer material count ≤ per-category limit (8.6). Prop prefabs: ≤ 1 material per minor prop. |
| LOD component | Character/major prop prefabs must have LODGroup with correct LOD count (characters 4, ambient NPCs 3) |
| Naming convention | File names must match the pattern established in 8.2. Invalid names fail immediately. |

**Advisory checks (warnings shown, import proceeds):**
- UV overlap on lightmapped geometry
- UV padding < 4px at native atlas resolution
- Texel density deviation > 2× median within an atlas
- Hard normals on meshes with expected soft shading
- Material used on 3+ identical mesh instances without Enable GPU Instancing checked
- Missing back faces on occluder-candidate meshes

### 8.9 — Texture Streaming Requirement

Target: **≤350MB texture memory resident at any point during normal play**.

The 30-40 tile set library (S6.2) must never be fully resident — only the active zone's subset loads. Per-zone streaming groups configured via Unity Addressables with explicit unload triggers on zone transition.

**Flag for `unity-addressables-specialist`:** define texture streaming groups per zone type (city district, haunt faction, outdoor transition) with explicit memory budgets. Validate streaming behavior under worst-case city-hub density before Tier 2 begins.

### 8.10 — What This Section FORBIDS

- **Non-power-of-two textures** in any runtime asset category
- **Albedo textures flagged Linear** in Unity importer (or non-color data flagged sRGB)
- **OpenGL normal maps imported without green-channel correction**
- **VFX textures above 256²** for particle sprite sheets
- **Arbitrary naming** outside the conventions in 8.2 or short-codes outside the established set
- **JPEG in any asset category** — lossy compression corrupts surface-history textures
- **Source files (PSD, Substance source) delivered as Unity imports** — source files version-controlled alongside PNG masters but never shipped
- **Compression format decisions made by art director** — format selection is technical-artist territory
- **EXR for runtime character or environment textures** — reserved for sky dome and lightmap sources only
- **FBX with embedded textures** — creates duplicate assets and version-control conflicts
- **The phrases "detail texture" or "variation texture"** — if a texture exists to break monotony rather than represent specific cause, it fails the cause-test (S6.2). A texture with no articulable `[cause]` slot in its name does not belong in the pipeline.
- **Unique-per-NPC body textures** on ambient NPCs — breaks faction GPU instancing and blows the draw-call budget
- **More than 2 materials on an ambient NPC** — hard limit; draw-call economy depends on it
- **Decal projectors at city exterior density** — bake history into tile textures instead; projectors for interiors only
- **Unique Collective atlases per faction** — Collective uses cross-faction atlas only

---

## Section 9 — Reference Direction

### Why This Section Exists

A reference list is not a mood board. Mood boards are collections of things the art direction finds beautiful; they produce imitation. This section is **production guidance** — it tells an artist, concretely, where to look when they are uncertain about a specific decision. Each reference is cited for exactly one function it serves that the other four do not. If a reference is absent from this list, it is not because it was overlooked — it is because another reference already covers that function, or because the reference would corrupt the art direction if treated as a visual target. **The list closes doors.** An artist who has read this section should be able to reject a design decision by identifying which reference rules it out.

### Reference 1 — Pre-Raphaelite Painting (Rossetti and Millais, 1848–1860)

- **Medium:** Painting
- **What to take:** The technique of building material identity through locally-specific color events rather than palette-wide tonal unification. Rossetti's surfaces — cloth, hair, skin, metal — are each individually and specifically themselves; no single atmospheric wash unifies the painting's tone. Millais's *Ophelia* (1852) is the clearest production reference: every surface has been painted as if the artist spent a week looking at it before committing paint. The lesson is not "paint lushly" — it is "**each material earns its place by being specifically what it is**." Color does not mean anything beyond what the material it represents means. Elizabeth Siddal's dress in *Ophelia* is not "blue for melancholy" — it is the specific blue of waterlogged heavy fabric with embroidery pattern losing its thread tension. That is the target. Pre-Raphaelite also establishes the **sculpted-specific portrait register**: the face at rest carries expression in its form, not in its movement. A neutral face is not a blank face.
- **What to avoid:** The saturated jewel-palette of late Pre-Raphaelite work (Burne-Jones, later Rossetti) where color becomes symbolic and decorative rather than material. Hunt's *The Light of the World* uses lantern-warm and cold-dark as emotional opposites; Gravenspire refuses that convention. Also avoid: Pre-Raphaelite narrative staging, where figures are arranged to communicate a specific literary story legibly. The art bible explicitly forbids object arrangements designed to tell a single legible story. Pre-Raphaelite environmental arrangements are theater; their surface technique is not.
- **Anchor in the art bible:** Section 4 Color Philosophy ("every color is load-bearing; a small palette applied with specificity produces more visual richness than a large palette applied democratically"). Section 6.2 (every albedo must pass the cause test). Section 5.3 (sculpted-specific portrait register; "neutral mesh IS an expression").

### Reference 2 — Medieval Italian City-State Architecture (Siena and San Gimignano, 13th–15th century)

- **Medium:** Architecture (historical)
- **What to take:** The specific geometry of **contested vertical urban space** — buildings that grew upward in competition with adjacent buildings, with no single planning authority. San Gimignano's towers demonstrate what Section 3.2 specifies: streets at 3:1+ height-to-width ratio that are not narrow by design but by accumulation. More usefully: the way different centuries of building remain legible in the same structure without any single century being restored over the others. In Siena specifically, the Palazzo Pubblico and adjacent buildings show the exact stratigraphy Gravenspire requires — Stratum 2 civic gothic underneath Stratum 3-4 factional modification, both present in the same facade. The Campo shows the ground-surface wear pattern specified in Section 3.2 (herringbone brick polished at traversal centerlines, rough at edges). Most usefully for production: the way windows and doors have been modified across centuries — 13th-century arches infilled with 15th-century windows, 14th-century doorframes with 16th-century lintels, 17th-century brick closures in gothic arch openings. **This is the threshold geometry contradiction (Section 6.1) as a real-world reference set.**
- **What to avoid:** Tourist-sanitized, restored, or digitally enhanced versions of these spaces — cleaned mortar, even stone coloring, replaced hardware. Look at the least-restored, most-archaeologically-intact portions. Also avoid: the open plaza logic of Italian civic space, which is about public gathering and visibility — the opposite of Gravenspire's enclosed street geometry. Gravenspire takes the building vocabulary, not the urban planning philosophy.
- **Anchor in the art bible:** Section 3.2 (street geometry, vertical compression). Section 6.1 (age-legibility cues, chronological strata, the five architectural tells, per-faction expression). Section 6.2 (wear/settling specifications; 80/20 tile reuse strategy with stratum tagging).

### Reference 3 — *Piranesi* by Susanna Clarke (2021, novel)

- **Medium:** Novel (literary)
- **What to take:** The specific mechanism by which the protagonist develops literacy for an environment that predates him by an unknowable span and was not built for him. Clarke's Piranesi reads the House — its tides, its statues, its skeletal remains of previous inhabitants — the way Section 6.4 describes how the Gravenspire player should read the city: **as physical consequence, not as narrative illustration**. The novel's operative lesson: a world can be fully legible, fully comprehensible, and still predate the viewer entirely — literacy does not require the world to have been designed for reading. Piranesi does not need the House to explain itself; the House's explanations are the tides, the wear patterns, the positions of things. Crucially, Clarke maintains this throughout without the protagonist becoming the world's subject — **the House is indifferent to Piranesi, and it is the most important thing in the novel**. The functional lesson for Gravenspire artists: an environment can carry complete forensic story without arranging itself to communicate.
- **What to avoid:** The House's unreality — impossible physics, portal-logic, explicitly magical architecture. Gravenspire is not surreal. Everything about the city is physically explicable given the history. The reference is for the **epistemological relationship** between viewer and environment, not for the environment's actual physics. Also avoid: the novel's mystery-narrative structure, where environmental details are clues to a thriller plot. Gravenspire's environmental stories are not clues to a detective story; they are the sediment of systems operating, not planted evidence.
- **Anchor in the art bible:** Section 6.4 (physical consequence vs. narrative illustration; "the player who reads Gravenspire's environment is doing forensics, not narrative consumption"). Section 3.4 (what draws the eye; grammar of significance without performance). Pillar 1 (The World Is Not Your Story) + Principle 3 (The Beautiful And The Wrong).

### Reference 4 — EverQuest Classic, Mistmoore and Unrest Haunts (1999)

- **Medium:** Game
- **What to take:** **Not the visual quality** (1999 low-polygon rendering is not a visual target). **The spatial grammar.** A haunt is a sequence of function nodes with inhabited material between them, not a sequence of encounters with atmospheric dressing between them. Mistmoore Castle and The Estate of Unrest are canonical because they do something almost no game before or since has replicated: **the world inside does not acknowledge the player's arrival**. No lights trigger on entry. No ambient creatures shift behavior. The space has been going about its operation before the player arrived and continues doing so. The specific pacing contribution: the pull happens when the enemy's facing changes, not when the player triggers a radius or crosses a threshold. **The world makes the decision; the player registers it.** This is the combat initiation grammar in Section 2 State 3 ("The Pivot") drawn from direct precedent. Also: the spatial relationship between inhabited low-light quiet and sudden confrontation. These haunts have long corridors of low ambient sound and minimal motion that terminate, without warning, in an encounter. **The absence of pre-encounter signaling is the point.**
- **What to avoid:** The visual design of the spaces themselves — flat stone textures, uniform-brown color field, ambient darkness that reads as technical limitation rather than material darkness. Section 2 specifies materially-motivated darkness; Section 4 forbids true black. EQ Classic's darkness is technical absence of lighting; Gravenspire's darkness is controlled practical-source-inventory logic. Also avoid: the NPC density and spawn-camp model of EQ Classic content design — irrelevant to visual direction and actively corrosive to the inhabited-space logic.
- **Anchor in the art bible:** Section 2 States 3 and 6 (combat initiation through mesh pivot; haunt pacing: "long corridors of quiet attention; the encounter begins without warning"). Section 6.3 (prop density; "haunts are spaces with specific use history, not aesthetic atmosphere"). Section 3.4 (what draws the eye; named NPCs through specificity, not prominence).

### Reference 5 — Caspar David Friedrich, Overcast Atmospheric Work (*Winter Landscape*, 1811; *Monk by the Sea*, 1810)

- **Medium:** Painting
- **What to take:** **Not the compositional logic** (human figure as protagonist in a vast landscape is the wrong reading for Gravenspire — see below). **The atmospheric physics.** Friedrich's overcast light is the only painting reference that demonstrates how a diffuse 6000K sky dome operates as a light source on stone, vegetation, and cold surfaces. In *Winter Landscape*, the light does not come from anywhere; it is everywhere; it compresses tonal range while preserving material identity. Stone reads as stone, dead grass reads as dead grass, ice reads as ice — not because the painting is lit dramatically, but because the ambient light is flat enough that material reflectance carries all the information load. This is the Gravenspire exploration-state lighting specification in Section 2 State 1 (flat ~6000K diffuse sky dome, shadows compressed to ~1:2 contrast) **demonstrated in paint**. *Monk by the Sea* shows the specific relationship between an overcast sky and a very dark foreground: the sky is the lightest element, the land is the darkest, and nothing in between is lit by a directional source. **The light does not emphasize; it simply reveals.**
- **What to avoid:** The Romantic Sublime compositional grammar: a small human figure overwhelmed by a vast natural world, positioning the viewer as both the figure's subject and the landscape's witness. This is spectacle-scale. Gravenspire's indifference operates through **density and accumulation, not through scale**. Also avoid: the specific desolate-winter bareness of Friedrich's most well-known work — stripped trees, snow, explicit ruin. Gravenspire's city is continuously inhabited, maintained, lived-in. Friedrich's landscapes are abandoned and seasonal. The lesson is the light physics, not the scene.
- **Anchor in the art bible:** Section 2 State 1 (exploration lighting spec: overcast 6000K ambient, compressed shadows, practical warm sources asserting weakly). Section 4 (Candlefall Amber only where physical source exists; Pewter Rain is overcast ambient). Section 6.1 (exterior ambient light behavior through geometry, not zone-grade).

### How to Use These References in Production

These five references are consulted at **specific decision points**, not carried as general background inspiration.

- **Consult Reference 1 (Pre-Raphaelite)** when a surface material lacks physical specificity — when a stone wall reads as "stone wall texture" rather than as this specific limestone under this specific history of weathering and occupation.
- **Consult Reference 2 (Italian city-state architecture)** when designing or assessing an architectural form, threshold modification, or building stratigraphy — if Siena doesn't have it, the question is whether Gravenspire's history justifies an exception.
- **Consult Reference 3 (*Piranesi*)** when reviewing environmental dressing for the inhabited-vs.-theater distinction — if a scene's objects feel like they're telling a story rather than being the residue of one, the Piranesi standard applies.
- **Consult Reference 4 (EQ Mistmoore/Unrest)** when assessing haunt-space layout, NPC idle behavior, or the combat initiation moment — it is the pacing reference, the grammar of what it feels like to be in a space that does not acknowledge you.
- **Consult Reference 5 (Friedrich)** only for **exterior lighting decisions** — overcast light quality, the relationship between ambient diffuse and practical warm sources in outdoor contexts.

These references **do not resolve** character design decisions (governed by Section 5), UI questions (Section 7), or production-format decisions (Section 8). They are environmental and material references only, except Reference 4's pacing grammar which extends into character and encounter design.

### References Explicitly NOT Included (and Why)

- **Bloodborne (2015)** — visually the most cited gothic game reference in contemporary development, and the most dangerous for Gravenspire. Bloodborne's art direction is zone-grade driven: each zone has a strong LUT-shift defining its atmosphere. **Section 1 explicitly names and forbids this** ("jump-cut lighting between zones — the Bloodborne instinct"). The game's VFX density and particle-heavy boss encounters are the opposite of Stillness Is The Signal. Bloodborne's atmosphere is performed for the player; Gravenspire's is not. Any artist treating Bloodborne as a reference will produce exactly the wrong outcome.
- **Castlevania / Bloodstained** — gothic ornament maximalism: skulls in architectural trim, bone iconography as generic undead signal, environments where every surface carries decorative darkness. Section 6.6 forbids "gothic atmosphere" as an artistic brief. Castlevania's visual shorthand is almost entirely composed of the specific shortcuts the art bible prohibits by name.
- **Vampire: The Masquerade — Bloodlines (2004)** — the most tempting exclusion because VtMB appears in the concept doc and its faction social complexity is relevant. However, VtMB's visual direction for faction spaces is maximalist and theatrical: the Tremere chantry is gothic-horror stagecraft, the Sabbat spaces are visceral gore-staging, Camarilla elysium spaces perform power through oppressive surface elaboration. The faction vocabulary Gravenspire requires is **legibility through material and silhouette, not spectacle**. An artist using VtMB as a visual reference for faction spaces will produce spaces that *perform* their faction, not spaces that have been inhabited by it.
- **Pre-Raphaelite painting after 1870 (Burne-Jones, late Rossetti)** — a specific callout within the movement. The early Pre-Raphaelites (1848–1865) built material specificity. The later work trends toward symbolic saturation and allegorical atmosphere — the exact decorative quality the color system forbids. Separating "early Pre-Raphaelite technique" from "late Pre-Raphaelite aesthetic" is a real production risk warranting the explicit call.
- **Dark Souls atmospheric work** — the environmental patience and consequence reading in Dark Souls is relevant, but the visual direction is not. Dark Souls environments communicate age primarily through ruin, collapse, and decay — the magnificent wreckage aesthetic. **Gravenspire is not ruined.** The city is continuously inhabited and maintained, which produces an entirely different visual problem. Dark Souls's ruins are spectacular. Gravenspire's continued occupation is the uncanny thing, not collapse.
