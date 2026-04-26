# Cover Art Concepts: Gravenspire

> **Status**: Draft (creative exploration, not formally reviewed)
> **Created**: 2026-04-25
> **Author**: Claude Code (with `art-director` agent), session with brian
> **Related**: [art-bible.md](art-bible.md), [game-concept.md](../gdd/game-concept.md), [world-structure.md](../gdd/world-structure.md)

## Purpose

This document preserves the creative exploration of Gravenspire's front-cover identity — six distinct cover concepts authored across two passes, the prompt-engineering rules derived from the AI image-generation work, and the strategic distribution recommendation that allocates concepts across Steam surfaces (capsule / hero banner / in-game UI art).

The artifact serves three future purposes:
1. **Reference for later cover-artist commissioning** — when a real human illustrator is hired (T2+ likely), this catalog gives them a tonal anchor that has already been validated through multiple rendering iterations.
2. **Reusable prompt-engineering knowledge** for any future Gravenspire AI-generated asset work (NPC portraits, faction illustrations, marketing materials).
3. **Audit trail** of the design instinct behind the project's identity-fantasy framing, captured at pre-production for later retrospection.

---

## Strategic Distribution

After two creative passes, the recommendation is **not to pick one cover** but to allocate concepts across Steam surfaces:

| Surface | Concept | Why this surface |
|---|---|---|
| **Steam capsule (460×215px)** | C — Lamplighter (undead, civic infrastructure) | Pallor-in-wrong-light + atmospheric mood reads at thumbnail scale; encodes the differentiating claim ("undead = civil service") in one image |
| **Steam store page hero banner (full res)** | B — Court Knows Your Name (undead aristocracy) | Strongest single image artistically; multiple layered cues (period-misalignment, architecture they predate, healed-wrong wounds) need full resolution to read |
| **In-game faction board UI art** | A — Clerk's hand (undead bureaucrat) | Compositionally too specific to "waste" on a thumbnail; perfect as the actual board art the player sees in-game |
| **Promotional / mood pieces** | Concepts 1, 2, 3 (originals) | Atmospheric mood pieces; useful for trailers, screenshots, store page secondaries |

This distribution acknowledges that no single cover image can communicate everything Gravenspire is. Three images working together do the genre-signaling, tonal-establishment, and identity-fantasy framing in different registers at different scales.

---

## First Pass Concepts (Pre-Undead Iteration)

These three concepts established the *compositional language* — Friedrich-philosophical framing, Pre-Raphaelite material fidelity, "two figures not facing each other" anti-protagonist grammar — but did not visually communicate the undead political sandbox genre.

### Concept 1 — "The Lamplighter's Round" (original)

**Status**: SUPERSEDED by Concept C (same composition + visible undead lamplighter).

**Visual description**: Pre-dawn gothic street. Two figures: a small lamplighter on his round (back to viewer, ~8% of frame), and a Vampire Court silhouette walking the opposite direction past him. Neither faces viewer. Neither acknowledges the other. Single amber lamp cone overlaps the Court figure's passing shadow — that overlap is the compositional center. Cobblestones rendered with Pre-Raphaelite hyper-detail (smoothed at the center of each stone by centuries of foot traffic) are the actual subject.

**Why Gravenspire (not generic gothic)**: Direct visual expression of Pillar 1 ("The World Is Not Your Story"). Friedrich's *Monk by the Sea* compositional ancestor — small figure dwarfed not by dramatic nature but by indifferent continuity.

**What it deliberately rejects**: Center-frame hero. Player-projection entry-point. The cognitive disruption of "this doesn't look like a game cover" *is* the message.

**Image-gen prompt**:
```
A narrow gothic city street at pre-dawn, desaturated stone gray cobblestones
worn smooth at the center by centuries of foot traffic, rendered with Pre-Raphaelite
hyper-detailed surface texture. In the far midground, a small lamplighter figure
(back to viewer, occupying 8 percent of frame height) on his morning round carries
a warm amber lantern at 2200K. Passing him in the opposite direction, a figure in
tall formal gothic court attire (high flat trapezoid collar, layered hem at mid-calf)
walks toward the viewer with face turned away. Neither figure acknowledges the other.
Buildings close overhead at 3:1 ratio, pre-dawn charcoal sky barely visible in a
narrow strip at top. Warm amber lamplight overlaps the court figure's shadow — this
overlap is the compositional center. Medieval Italian city-state architecture, Florence
register, 400 years of accumulated maintenance and modification. Oil painting style,
Millais surface quality, Caspar David Friedrich compositional philosophy. Vertical
2:3 format. Color: desaturated gray dominant, single amber warm source, no color
saturation anywhere else. No glow, no hero lighting, no magic effects.

--ar 2:3 --style raw --no glow, magic effects, hero lighting, dramatic sky,
protagonist framing, skulls, bones, generic gothic, fog machine mist,
center-framed hero, weapon showcase
```

### Concept 2 — "The Faction Board at Dawn"

**Status**: KEEP. Strongest first-pass render. Concept A (clerk's hand) is an *addition* to this image, not a replacement.

**Visual description**: Close interior. A physical faction board on a stone wall — layered notices, writs, seals, and pinned papers. The game's macro-progression system rendered as document archaeology. Bottom layers ancient and brown; topmost notice is gray-blue Vampire Court vellum, this morning's, with a visible wax seal and a court secretary's working hand. Single overhead practical lamp cone. At the extreme left edge: silhouette of someone who has just stopped to read. Not the subject. The board is.

**Why Gravenspire (not generic gothic)**: Most novel of the three. Presents the game's *actual central system* — faction reputation and political reading — as the subject, without showing combat, hero, or monster. The cover teaches you that progression here is political reading, not combat achievement, before you've launched the game.

**What it deliberately rejects**: Spectacle. No enemy reveal, no confrontation, no power demonstration. Every guild RPG cover shows what you'll be fighting. This shows what you'll be reading.

**Image-gen prompt**:
```
A medieval stone wall covered in layered faction notices, writs, and pinned papers --
a physical political board seen in close interior composition, waist-to-eye-height
filling the frame. Layers of paper accumulating over years: bottom layers ancient,
brown, pressed flat; middle layers spanning years of faction politics; topmost notice
is gray-blue vellum with a visible wax seal and a secretary's working handwriting,
slightly less worn than the layers beneath. Single overhead practical lamp (warm,
tight 30-40 degree cone) illuminates the board. The room around it recedes into
ambient shadow. At the extreme left edge of frame, cut off at chest height, is the
silhouette of a figure in tall formal gothic court attire reading the board -- only
the edge of them is visible, they are NOT the subject. The board is the subject.
Medieval Italian city-state documentary texture: aged paper, dried wax, stone wall
with centuries of soot and marks. Pre-Raphaelite material fidelity. Oil painting
style, Millais surface quality. Vertical 2:3 format. Palette: stone gray, aged
brown, the one lamp-cone, gray-blue vellum as the only cool note.

--ar 2:3 --style raw --no hero figure, protagonist, combat, magic glow, skulls,
generic gothic, wide shot, monster reveal, dramatic lighting, saturated colors,
fantasy UI elements
```

### Concept 3 — "The Court Knows Your Name" (original)

**Status**: SUPERSEDED by Concept B (rebuild with locked figure orientation). Original render failed because multiple Court figures faced the viewer, collapsing the social-disquiet payload.

**Visual description (original brief)**: Vampire Court interior at night, candelabras at 2200K, eight Court NPCs in mid-conversation — none looking at the viewer. Center-left foreground: a player character (back to viewer, slightly less resolved silhouette). Deep in the room, **one Court figure turned 15° more toward the player than the others** — not looking at them, but the orientation is fractionally off. Their jewelry catches the candlelight differently. One of them has noticed you.

**What rendered instead**: Multiple figures oriented toward player; the precise "one turned 15°" detail did not transfer; player character rendered as fully-resolved adventurer rather than the unresolved-newcomer the brief asked for.

**Lesson**: When a brief depends on a specific compositional event (one figure subtly off, jewelry catching light differently), AI image-gen may render the spirit but not the specific. See "Prompt-Engineering Rules" below for the technique used in Concept B to lock the orientation.

---

## Second Pass Concepts (With Visible Undead Citizenry)

The first pass succeeded compositionally but failed at genre-signaling — the rendered images read as "gothic costume drama" rather than "undead political simulation." The second pass solves this by making the undead-ness visually legible without crossing into Hollywood horror.

### Concept A — "The Clerk Who Has Always Been Here"

**An iteration on Concept 2 (Faction Board) that adds one undead figure at maximum leverage.**

**Visual description**: The Faction Board composition is preserved. The change: the partial figure at the left edge becomes the **undead Court clerk pinning the morning's notice**. What's visible:
- One pale hand and wrist
- The cuff of a sleeve (mid-17th century court formal — pressed, immaculate, 200 years out of period with the architectural setting)
- Maybe one quarter of his torso and jaw-line in the lamp's peripheral spill

The undead-ness concentrates in three specific cues:
- **Pallor against warmth**: Skin reads corpse-pale in 2200K candlelight that should warm everything. The math is wrong.
- **Period misalignment in the sleeve**: Cuff style discontinued 200 years before the architectural period of the surrounding building. He has been maintaining this board longer than the building's current stone has stood.
- **Stillness in the hand**: Fingers closed completely around the pin. No tension suggesting motion. The flat quality of a hand at rest between tasks.

**Why Gravenspire**: Le Fanu's *Carmilla* register — the houseguest who is slightly wrong, ageless, beautiful and dangerous. The viewer reads it the way they read Carmilla's unnaturally perfect stillness: nothing is wrong with any individual element, but the sum is impossible. The Piranesi parallel is precise: what makes the Piranesi register eerie is not threat but that entities are *more real and more settled than you are*.

**What it deliberately rejects**: The horror convention is the full reveal (vampire turns, you see the teeth, the red eyes). This image refuses that grammar entirely. The wrongness is in the wrist and the cuff. You never get to see if it would be worse with a face. Le Fanu technique — what *isn't* shown carries more than what is.

**Image-gen prompt**:
```
Dimly lit medieval hall, faction notice board covering the left two thirds, layered
papers and documents pinned in overlapping years of accumulation, a narrow brass
reading lamp casting 2200K amber light over the board from above right. Dawn cold
light at 5500K visible through a narrow stone window at far left, deep shadows in
the architectural surround.

At the very left edge of frame, a single pale hand in a formal lace-cuffed sleeve
pins a folded gray-blue vellum document to the board. The cuff style is mid-17th
century court formal — pressed, immaculate, and seventy years out of period with
the architectural setting. The skin of the hand is corpse-pale, unnaturally bright
in the amber lamplight, as if the warm source cannot warm it.

Pre-Raphaelite material fidelity. Candlelit documentary space. Medieval Italian
city-state administrative interior. The hand is completely still, grip complete
on the pin, no tension suggesting motion.

--no glowing eyes, no bared teeth, no supernatural aura, no visible wound, no
skeletal features, no horror lighting, no fog machine atmosphere, no dramatic
pose, no modern clothing, no bright colors, no fantasy runes, no emotional
expression visible on any face
```

### Concept B — "The Court Knows Your Name — Revised for Undead Register"

**A rebuild of Concept 3 with locked figure orientation and explicit undead cues.**

**Visual description**: Court interior — high ceilings, candlelit dark, polished black marble floor, tarnished silver hardware, deep shadow at room perimeter. **Three undead figures, all in Vampire Court formal dress, all facing away from or perpendicular to viewer:**

- **Figure A** (center-left, male, ~40s apparent age): Stands at a tall window, back mostly to viewer, face in three-quarter profile facing left — looking at nothing. Conversation-pause posture: hands clasped behind him. Court attire from early 19th century, slightly wrong for the architectural period. **Undead cue**: His one partially visible eye has the specular quality of a taxidermied specimen — not red, not glowing, but reflecting from the wrong depth.
- **Figure B** (far right, partial, female): Only shoulder and side of face visible, turning away. Her dress is older than the architectural period. **Undead cue (architecture they predate)**: Her high vertical collar silhouette **matches an architectural carving on the limestone wall behind her** — same formal vertical geometry. The visual rhyme says she's the same age as the carving.
- **Figure C** (foreground, back to viewer): Seated at a writing desk, writing by single candle, does not look up. **Undead cue (old wounds healed wrong)**: Visible writing hand has a finger shortened by half a joint — long healed, but proportions wrong on inspection.

**Why Gravenspire**: *Bloodlines* Toreador register — Vampire Court as genuine aristocracy, beautiful and ancient and specific, inhabiting their own social world with indifference to the player. The locked figure orientation (none facing viewer) restores the social-disquiet payload that the original render lost. The period-mismatch matching the architectural carving (Figure B) is the Friedrich compositional technique applied to character design.

**What it deliberately rejects**: The Nosferatu framing (cadaverous faces, threatening postures, supernatural intensity). These figures are beautiful. That is the trap of the image — you should look at it and think "beautiful gothic interior" until the second look finds the wrong-reflecting eye, the missing finger, the collar matching a four-century-old carving. The horror is not in the image. It is in the viewer's recognition.

Also rejects the "vampires hosting a party for the player" convention. Nobody is hosting anything. These people are doing their own work in their own room.

**Image-gen prompt**:
```
Gothic interior, Vampire Court hall, high ceilings with tarnished silver candleabras,
polished black marble floor reflecting warm candlelight, architectural limestone relief
panels carved with court insignia on the far wall. Candlelit 2200K warm amber dominant,
deep shadow at room perimeter and ceiling.

Three figures in formal court attire, all facing away from or perpendicular to the
viewer. Left figure: man in early-19th century formal dress standing at a tall window,
back to viewer, head in three-quarter profile facing left — in conversation pause,
hands clasped behind him. Far right: partial female figure in very high vertical
formal collar, centuries-older dress than the architectural period suggests, only
shoulder and near-profile visible. Foreground: a figure seated at a writing desk,
back fully to viewer, writing by a single candle, right hand holding a pen, the
ring finger visibly shorter than normal by half a joint, old healed injury.

The left figure's one partially visible eye has the reflective quality of an
animal eye or taxidermied specimen in candlelight — not glowing, not red,
but reflecting from the wrong depth.

The female figure's high vertical collar silhouette echoes a carved court insignia
on the limestone panel directly behind her, the same formal vertical geometry.

Pre-Raphaelite material fidelity. Caspar David Friedrich compositional weight.
No figure faces toward viewer. Administrative aristocratic space. Occupied by
people attending to their own concerns.

--no glowing red eyes, no fangs showing, no dramatic pose toward camera, no
theatrical expression, no supernatural aura, no modern elements, no blood,
no violence, no horror lighting, no fog, no silhouette glow, no magical effects,
no cosplay-register costumes
```

### Concept C — "The Lamplighter's Round — The Witness Who Is Not Alive"

**The Steam capsule pick. New concept that supersedes Concept 1.**

**Visual description**: Pre-dawn. Narrow city street, 3:1 height-to-width ratio per art bible. High walls, overhanging cantilevered upper stories, sky a narrow strip of 5500K blue-grey dawn above. A string of oil lanterns at 4m height along the right wall, several already lit further down the street. Morning mist at knee height, static not swirling.

**A figure on a short step-ladder is lighting the nearest lantern. He is undead.** Compositionally central though facing away from viewer. The undead cues:

- **Pallor in wrong context**: His face, in partial profile, is not warmed by the 2200K amber lantern he just lit. His skin reads cool grey of overcast stone in a light that should amber-tint everything. The math is wrong.
- **Period misalignment in livery**: He wears municipal lamplighter livery from a coat style 200 years prior — buttons archaic, collar discontinued in living memory. He has been on this round longer than the city's current municipal charter.
- **The lanterns ahead are already lit**: He is lighting the nearest now. But the string further down is already glowing. He has already been here. He is completing a round that was already complete before the player arrived. **He is not performing for the player's arrival; he is on his own schedule.**
- **Two ambient figures further back, very small, crossing the far end of the street.** They are not looking at the lamplighter. He is not an event. He is part of the city's morning routine.

**Why Gravenspire**: This concept solves the genre-signaling problem most legibly for a Steam thumbnail because it encodes the game's core identity claim in a single visual: **the undead run the civic infrastructure.** The lamplighter is not a monster. He is the civil service. He has been lighting these same lanterns for 200 years, and the city needs its lanterns lit, so the city employs him to light them.

The pallor-in-wrong-light cue is the most visually legible of the undead vocabulary for a three-second Steam scroll: warm light not warming the subject is an immediate read for anyone who has looked at portrait photography or painting, even without knowing why. It is the visual equivalent of a chord that contains a wrong note.

**What it deliberately rejects**: Making the lamplighter's undead-ness the drama of the image. He is not the protagonist of his own image. The drama is the city at dawn — the lantern-lit street receding in perspective, the narrow sky above, the scale of accumulated architecture. He is an element of the city's morning, not the horror in the middle of it. Also rejects "ancient vampire surveys his domain" composition. No survey gaze, no dramatic horizon, no implied sovereignty. This figure is on a ladder doing a job.

**Image-gen prompt**:
```
Narrow pre-dawn gothic city street, walls 3-to-1 height-to-width ratio, overhanging
stone upper stories, sky a narrow strip of steel blue at the vanishing point. A string
of oil lanterns hung at 4-meter height receding in perspective along the right wall.
Several lanterns already lit further down the street. Morning mist at knee height,
static not swirling.

A figure on a short step-ladder is lighting the nearest lantern. He faces away from
viewer at three-quarter angle. He wears the formal livery of a municipal lamplighter,
coat style early-18th century — archaic compared to the architectural period of the
surrounding buildings. His exposed profile and wrist are corpse-pale, the amber
lamplight of the newly-lit lantern failing to warm his skin — his face reads the
cool grey of overcast stone in a light that should amber-tint everything.

Far back on the street, two tiny figures cross. Neither is looking at the lamplighter.
He is unremarkable. He is part of the morning.

Pre-Raphaelite material fidelity — stone surface texture, lamp metalwork aged and
specific, worn cobblestones with centuries of traffic wear at stone centers. Caspar
David Friedrich atmospheric scale — figure small against dense vertical architecture.
Morning civic routine. No drama.

--no skeleton features, no exposed bone, no bared teeth, no glowing eyes, no
supernatural emanation, no fog machine, no horror framing, no dramatic pose,
no action, no threat display, no blood, no gore, no shroud, no tattered clothing,
no zombie appearance
```

---

## Prompt-Engineering Rules (Reusable for Future Asset Work)

These rules emerged from running the cover concepts through ChatGPT image generation. They apply to any future Gravenspire AI-generated asset that needs to express undead-ness, period-mismatch, stillness, or material specificity in the project's tonal register.

### 1. Color-temperature physics over skin-color descriptions

"Corpse-pale in amber light that should warm him" beats "pale skin" beats "undead." The model is being given an *optical contradiction* to solve, not a category to render. Optical contradictions render reliably; categories drift.

### 2. Period misalignment specified by decade + dress element

"Mid-17th century court formal — pressed, immaculate, 70 years out of period with the architectural setting" is concrete and image-gen-tractable. "Ancient vampire costume" is a prompt for Hollywood. The model needs the precise temporal vocabulary, not a vague antiquity gesture.

### 3. Stillness through anatomy, not mood

"Fingers closed completely around the pin, no tension in the gesture, wrist at rest, no implied movement out of frame" beats "very still" beats "eerily still." Anatomical instructions render; mood instructions get smoothed into convention.

### 4. Eye-reflection through optical physics, not color

"Reflective quality of a taxidermied specimen, reflecting from the wrong depth, not from the surface" beats "glowing eyes" (which you are negative-prompting anyway) beats "unsettling eyes." A useful fallback anchor: *"the way eyes look in old oil paintings, where the reflective surface seems to be behind the surface of the eye"* — gives the model a painterly reference it can index on.

### 5. Avoid horror-pattern-matching words in positive prompts

Words that activate horror-genre pattern-matching — `undead`, `vampire`, `dead`, `ghoul`, `monster`, `creature`, `horror`, `supernatural` — should be **rare or absent** in the positive prompt. Describe the *effect* (pallor, stillness, period-wrong clothing) not the *category*. Put the category in the negative prompt as a guard.

### 6. Avoid costume-drama words in positive prompts

Words that activate "elegant gothic" — `beautiful`, `gorgeous`, `elegant`, `mysterious`, `atmospheric`, `romantic` — tell the model to produce a fashion editorial. Replace with material-fidelity anchors: `Pre-Raphaelite material fidelity`, `worn stone`, `tarnished silver`, `aged`, `worn`.

### Bonus rule: Override sympathetic-rendering bias

AI image-gen models have a trained bias toward *sympathetic figure rendering* — most reference images of "old man holding lantern" feature warm, dignified, well-rendered faces. To break out of that, give the model an *anti-sympathetic painterly mode* anchor:

- *"Marble or bleached bone in overcast daylight"*
- *"Surface honestly without flattering the subject"*
- *"Like a portrait painted from a corpse studied in candlelight"*

These override the trained sympathy without crossing into horror territory.

---

## Render History

| Date | Concept | Result | Notes |
|---|---|---|---|
| 2026-04-25 | Concept 1 (original Lamplighter) | Rendered: atmospheric mood landed; "two figures in opposite directions" payload did not transfer; figures appeared to walk same direction | Superseded by Concept C |
| 2026-04-25 | Concept 2 (Faction Board) | Rendered: strongest first-pass result; layered papers, wax seal, partial cloaked figure all rendered; "BANDO" Italian decree word visible — historically correct | Keep — base for Concept A iteration |
| 2026-04-25 | Concept 3 (original Court) | Rendered: mood landed; "one figure turned 15°" detail did not transfer; multiple figures oriented toward viewer; player character rendered as fully-resolved adventurer | Superseded by Concept B (locked orientation in prompt) |
| 2026-04-25 | Concept C (Lamplighter, undead) | Rendered: composition + mood + civic-continuity payload all landed cleanly; pallor-in-wrong-light cue partially landed but figure's face still has some warmth in amber light; two distant figures appeared as a walking pair, reinforcing civic-continuity | **PICKED for Steam capsule. Not iterating further; risk of losing strong composition outweighs gain of pushing pallor.** |
| TBD | Concept B (Court, undead) | Not yet rendered | Next render target — for Steam store page hero banner |
| TBD | Concept A (Clerk hand) | Not yet rendered | Add to existing Concept 2 render via inpainting at left edge — do not re-render from scratch |

---

## Notes for Future Cover-Artist Commissioning

When a real human illustrator is hired (likely T2+):

1. **Show them this document first** — particularly the Strategic Distribution table and the prompt-engineering rules. The rules are AI-specific but the *underlying tonal anchors* (color-temperature physics, period misalignment, anatomical stillness) translate to brief-language a human illustrator can act on.

2. **Reference the rendered images that worked** — particularly Concept 2 (Faction Board) and Concept C (Lamplighter). These are tonally validated. A human illustrator can match their register without re-deriving it from first principles.

3. **The three "what it deliberately rejects" sections per concept are the briefing gold** — they tell the illustrator what NOT to do, which is harder to specify than what TO do. Defending the absence of conventional gothic-cover spectacle is the hardest part of the brief.

4. **Pillars-aligned reference list**: Caspar David Friedrich (composition), Pre-Raphaelite painters Millais/Hunt/Rossetti (material specificity, doomed beauty), *Vampire: The Masquerade — Bloodlines* concept art (urban gothic intimacy), Susanna Clarke's *Piranesi* prose imagery (entities more inhabited than the viewer), Le Fanu's *Carmilla* (the slightly-wrong houseguest).

---

## Promotion Candidates

If patterns from this document repeat across other Gravenspire art commissions, candidates for promotion to a project-wide rules file:

- The **6 prompt-engineering rules** above could promote to `.claude/rules/asset-prompt-engineering.md` if they prove useful for future AI-generated asset work (NPC portraits, faction illustrations, marketing materials)
- The **"what it deliberately rejects" briefing pattern** could promote to `design/art/asset-spec-template.md` as a required section for any future asset spec — it's the same defensive-craft pattern Combat Core's revision adopted with explicit "Combat Core does NOT do X" boundary clauses

Both promotions are deferred until at least one second use case confirms the pattern works beyond cover art.
