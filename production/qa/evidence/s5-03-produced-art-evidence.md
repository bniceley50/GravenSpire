# S5-03 Produced-Art Evidence — The Sexton's Court (First-Pass)

> **Story**: S5-03 — Produced-Art Production (Representative Area)
> **Date**: 2026-06-09
> **Status**: Manifest COMPLETE — closure pending M2 preservation reruns + creative-director gate
> **Channel**: Unity editor via MCP bridge (all scene mutations editor-serialized; zero hand-edited YAML)
> **Authority**: D021; S5-00 design brief (LOCKED); S5-01 manifest; S5-02 asset-spec + budgets

## 1. Manifest Completion

| S5-01 manifest item | Delivered | Commit |
|---|---|---|
| mat_01 street cobble | Applied to district floor, 2m tile, **1024 import (spec-corrected from 2048)** | `d7863c5` / `32f6451` |
| mat_02 Caretaker-face Court ashlar | **Ground-floor band on the office +Z face ONLY (R1)** — door niche at x=3.6 (off Morrvik's axis), timber lintel, marble threshold | `2e84a25` |
| mat_03 residential facade stone | Hall massings 6×6; boundary walls 6×2 (city-fabric read) | `d7863c5` / `32f6451` |
| mat_04 timber trim | Lintel + faction board + handcart + debris crate | `2e84a25` |
| Practical lighting | Flat pewter overcast ambient + dim cool directional (soft, 0.45 strength) + **3 civic lantern practicals** (~2400K, Candlefall Amber) | `d7863c5` |
| Hero props (cap 5) | lantern posts ×3 (one prop type) · faction board (3 blank notices) · marble threshold · handcart (west wall) · corner debris (south wall) = **5 types, cap respected** | `d7863c5` / `2e84a25` |
| NPC placeholder palette (added scope, product-owner directed) | Morrvik Wick-Gray / vendor Quarry-Stone / player Bone-Pale; M2 combat capsules untouched | `443db2f` |

All set dressing is **collider-free** (visual-only): zero NavMesh/agent impact, no rebake
required, no soft-lock surface change. Set dressing lives under `S5_SetDressing`;
lighting under `S5_PracticalLighting`. The S3-05 geometry was reused unmodified.

## 2. [F1] Artifact-Identity Tuple

| Artifact | Value |
|---|---|
| `Assets/Scenes/_DevEntry.unity` content SHA | `bae11334e62cc72b39ffdd20d6cb10836891dda9` |
| `Assets/Scenes/_DevEntry/FirstDistrict_Greybox_NavMesh.asset` SHA | `5c20605e530996245a7061c01e82243063ee8dda` |
| NavMesh asset size | 9,704 bytes |
| Repo state | `95a0bb9` (origin/main; scene final state incl. the CD-gate lantern move at `1d92daf`) |

**Reproduce**: `git hash-object Assets/Scenes/_DevEntry.unity` and
`git hash-object Assets/Scenes/_DevEntry/FirstDistrict_Greybox_NavMesh.asset` at `95a0bb9`.
The S5-05 gate compares the played build against this tuple. *(Tuple re-hashed after the
CD-gate scene adjustment — the original 2e84a25 hash was superseded by the lantern move;
a stale identity anchor is the F1 failure mode this tuple exists to prevent.)*

## 3. Perf Snapshot vs S5-02 Budget (editor capture, 2026-06-09)

| Metric | Measured | S5-02 budget | Verdict |
|---|---|---|---|
| Scene renderers | 42 | — | — |
| Total triangles | 5,540 | 4,000–8,000 *per facade span* | PASS (entire scene under one span's allowance) |
| Lights | 3 practicals + 1 overcast directional | ≤4 authored practicals | PASS |
| Runtime texture memory (4 albedos) | ~11.1 MB (4× 2.77 MB, 1024 DXT5) | ≤64 MB new (sub-slice) | PASS |
| Cobble import | 1024, sRGB, mips, DXT5 | 1024 max (mat_01) | PASS (corrected from 2048 intake deviation) |

Capture method: in-editor object/mesh census + `Profiler.GetRuntimeMemorySizeLong` per
texture. The **provisional budget framework locks against a real play profile at S5-05**
(this snapshot is the pre-play sanity check, not the final lock).

## 4. Fence Compliance (D021 / §7.11 / S5-00 rejections)

- Practical-source light only: every point light sits in a lantern fixture; the directional is the overcast sun. No light placed for the player's benefit without an emitter.
- **CD gate (2026-06-09): PASS WITH ADJUSTMENTS — both applied.** The gate caught that the
  original Caretaker-corner lantern (4.5,0,-5.3) co-lit Morrvik and the door niche (the
  scene's only warm pool on the objective area = soft routing), and that lantern parity was
  asserted, not evidenced. **Adjustment:** the lantern moved to (-1.5,0,-3.5)
  (`S5_LanternPost_CourtWest` — 3.57m from Morrvik, 5.3m from the niche); post-move frame
  `s5-03-cdgate-lantern-moved-spawn-view.png` shows the warm pool on the residential mass,
  the niche/NPC in neutral cool light, both lantern posts visible, and **neither hall
  carrying a warm pool from spawn** (parity shown, not stated).
- Door niche at x=3.6 — **off Morrvik's standing axis (x=2)**, honoring the S5-00 "no entrance center-frames Morrvik" rejection; post-adjustment, the niche is no longer the lit feature of the face.
- Faction board notices are **blank pale quads** — no readable text, no routing.
- No emissive materials, rarity color, atmosphere-as-warning, or objective-framing composition anywhere in the produced set.
- Warm/cool greybox landmark tinting retired (both halls wear the same residential stone; Court read is material-precision on one face only).

## 5. Scene Discipline Record

- All `_DevEntry.unity` mutations were made by the Unity editor (MCP bridge `execute_code` / editor tools) and saved via the editor — **no hand-edited scene YAML**.
- One scene-edit batch per commit: `d7863c5` (materials+lighting), `443db2f` (NPC tint), `32f6451` (review adjust), `2e84a25` (set dressing).
- ProjectSettings drift (Graphics/Quality/URPProjectSettings) observed during the editor session: **unstaged, decision deferred until the editor closes** (no write-race with a live editor; 2026-05-26 lesson).
- The flagged S5-02 reference check: `:8` and `:20` cite the real committed evidence file — no stale reference found.

## 6. Visual Evidence (tests/evidence/S5-03/)

| File | Shows |
|---|---|
| `s5-03-before-greybox-spawn-view.png` | Baseline: grey tinted boxes, debug scaffolding |
| `s5-03-after-midpass-cobble-facade-spawn-view.png` | First materials (mid-pass GO) |
| `s5-03-after-lighting-spawn-view.png` | Practical lighting — first "reads as place" frame |
| `s5-03-after-npc-tint-spawn-view.png` | NPC placeholder palette |
| `s5-03-adjusted-1024cobble-6x6facade-spawn-view.png` | Post-review spec alignment |
| `s5-03-setdressing-spawn-view.png` | Manifest complete, spawn view (ashlar quiet from spawn) |
| `s5-03-setdressing-caretaker-face.png` | The two-register read: Court ashlar vs residential stone |

## 7. Open Before S5-03 Closure

1. ~~M2 preservation reruns (RG-02, BLOCKING)~~ **DONE 2026-06-09: 3/3 PASS** —
   single-trash / linked-overpull / named-blocker in separate batchmode invocations,
   preservation mode + builder skipped; evidence at
   `tests/evidence/S5-03/m2-0{2,3,4}-preservation-20260609-smoke.md` (commit `95a0bb9`).
2. ~~Creative-director gate (S5-03-04)~~ **DONE 2026-06-09: PASS WITH ADJUSTMENTS**
   (all 9 elements gated; 7 PASS, 2 ADJUST — lantern re-placement + parity evidence —
   **both adjustments applied and re-captured**, see §4). CD on-vision note retained for
   the next pass: place-read clears the D021 bar; "cursed gothic" tone (sky, grime) is a
   deepening item, not a gate failure.
3. ~~ProjectSettings drift restore-or-adopt~~ **SETTLED 2026-06-09: deliberate ADOPT**
   (`95a0bb9`) — GraphicsSettings linear-intensity/color-temperature 0→1 is the
   URP-correct lighting math the gated lantern pass was authored under; legacy
   QualitySettings AA zeroed (URP owns MSAA); URPProjectSettings whitespace noise.
4. Final budget lock happens at the S5-05 play profile.

**Closure state: all S5-03 evidence requirements satisfied — `/story-done`-ready.**

---
*All work editor-mediated via the Unity MCP channel (D021 pipeline; dependency recorded
in technical-preferences). Commits: `993edb7`, `d7863c5`, `443db2f`, `32f6451`, `2e84a25`
(pushed). Generated 2026-06-09.*
