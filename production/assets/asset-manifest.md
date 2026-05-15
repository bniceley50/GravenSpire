# Gravenspire Asset Manifest

**Owned by:** art-director
**Created:** 2026-05-15
**Updated:** 2026-05-15
**Authoritative source:** This file is the master inventory of all Gravenspire production assets. Per-asset specs live at `production/assets/specs/[asset-slug].md`.
**Source-of-truth refs:**
- `design/art/art-bible.md` — visual identity (Sections 1-9)
- `design/art/art-bible-t1-scope.md` — T1 M3 phasing
- `DECISIONS.md` — engine + tier locks (D001 URP only; D003 T1 single-player offline; D012 Combat Feel Prototype validation)

## Status legend

- **SPEC** — visual specification drafted; ready for production handoff
- **PROD** — in art production
- **READY** — production complete; ready for engine import
- **LIVE** — imported into Unity and validated
- **BLOCKED** — gated on upstream resolution (see Notes)

## Sprint 2 M3 — Unblocked Surface (specs generated 2026-05-15)

| # | Asset Id | Type | Tier | Faction | Status | Spec | Notes |
|---|----------|------|------|---------|--------|------|-------|
| 1 | env_arch_stone_ashlar_s3_vc_yr200 | Architecture tile | Primary | Vampire Court | SPEC | [spec](specs/env_arch_stone_ashlar_s3_vc_yr200.md) | Cemetery + district wall primary facade |
| 2 | env_arch_stone_rough_s1_neu_yr400 | Architecture tile | Primary | Neutral (pre-city) | SPEC | [spec](specs/env_arch_stone_rough_s1_neu_yr400.md) | Visible base course stratum below VC walls |
| 3 | env_arch_marble_polished_s4_vc_yr150 | Architecture tile | Unique (capped 3-4/zone) | Vampire Court | SPEC | [spec](specs/env_arch_marble_polished_s4_vc_yr150.md) | Court signature surface (black marble dado panels) |
| 4 | env_ground_cobble_street_neu_yr200 | Ground tile | Primary | Neutral | SPEC | [spec](specs/env_ground_cobble_street_neu_yr200.md) | Cobble street with foot-traffic wear |
| 5 | env_mesh_modular_s3_vc | Mesh set (wall + arch + doorframe) | Primary | Vampire Court | SPEC | [spec](specs/env_mesh_modular_s3_vc.md) | Modular kit per S6.1 |
| 6 | prop_maj_neu_gravestone_set_01 | Prop (Major) | — | Neutral | SPEC | [spec](specs/prop_maj_neu_gravestone_set_01.md) | Cemetery prop set (5-8 variants in one FBX) |
| 7 | prop_maj_vc_lantern_practical_01 | Prop (Major) | — | Vampire Court | SPEC | [spec](specs/prop_maj_vc_lantern_practical_01.md) | Court wrought-iron practical light source |
| 8 | env_lighting_district_camp | Lighting setup (Volume + lights) | — | Mixed | SPEC | [spec](specs/env_lighting_district_camp.md) | District (S2 State 1) + Camp (S2 State 2) |
| 9 | char_npc_amb_neu_feral_undead_trash | Ambient NPC (creature) | Ambient | Neutral (creature-register, no garments above faction threshold) | SPEC | [spec](specs/char_npc_amb_neu_feral_undead_trash.md) | M2 SoloTrash_EvenCon_T1 visual |
| 10 | char_npc_amb_neu_feral_undead_linked | Ambient NPC (creature) | Ambient | Neutral | SPEC | [spec](specs/char_npc_amb_neu_feral_undead_linked.md) | M2 LinkedTrash_T1 variant (paired-coupling visual) |
| 11 | char_npc_amb_neu_feral_undead_block | Ambient NPC (creature) | Ambient (named-in-combat-sense, NOT named-NPC tier) | Neutral | SPEC | [spec](specs/char_npc_amb_neu_feral_undead_block.md) | M2 NamedSoloBlock_T1; larger silhouette, distinctness via scale |
| 12 | char_pc_neu_cleric_t1 | Player Character | T1 onboarding (Rep Tier 0-1) | Neutral (pre-faction) | SPEC | [spec](specs/char_pc_neu_cleric_t1.md) | Body + onboarding garment; no face spec (player camera doesn't see face) |
| 13 | ui_l1_hud_layout | HUD (Layer 1) | — | — | SPEC | [spec](specs/ui_l1_hud_layout.md) | Full S4.4 palette; replaces current debug-IMGUI placeholder |
| 14 | doc_vc_paper_vellum_stock | Layer 2 paper | — | Vampire Court | SPEC | [spec](specs/doc_vc_paper_vellum_stock.md) | Gray-blue vellum, smooth/thin/formal |
| 15 | doc_vc_handwriting_chancery | Layer 2 typography | — | Vampire Court | SPEC | [spec](specs/doc_vc_handwriting_chancery.md) | Vampire Court chancery hand |
| 16 | doc_vc_seal_wax_01 | Layer 2 seal | — | Vampire Court | SPEC | [spec](specs/doc_vc_seal_wax_01.md) | Aged wax seal (no crisp pressed surface) |
| 17 | prop_maj_vc_court_marked_relic_01 | Prop (M3 objective) | — | Vampire Court | SPEC | [spec](specs/prop_maj_vc_court_marked_relic_01.md) | CourtMarkedRelic_T1 |
| 18 | prop_maj_neu_grave_dust_salvage_01 | Prop (M3 salvage) | — | Neutral | SPEC | [spec](specs/prop_maj_neu_grave_dust_salvage_01.md) | GraveDust_Salvage_T1 |

## Sprint 2 M3 — Blocked (gated on SSS proof-of-concept)

Per AD-ART-BIBLE sign-off (commit `0bcce46`) bound condition #3(b): URP SSS cost model is UNVERIFIED. Named-NPC tier (S5.2) requires SSS shader pass per S8.7. Until SSS POC resolves, no named-NPC material slot count / skin shader work can spec. POC task spawned 2026-05-15 (separate session); evidence will land at `tests/evidence/SSS-POC/`.

| # | Asset Id | Reason | Unblock Trigger |
|---|----------|--------|-----------------|
| B1 | char_npc_named_vc_caretaker | Named NPC tier (S5.2); face SSS pass dependent | SSS POC verdict; spec via `/asset-spec character:caretaker` |
| B2 | char_npc_named_vc_sister_elara | Named NPC tier per F-05 (T1 templated mentor, no LLM); same skin shader gating | SSS POC verdict |
| B3 | char_npc_named_vc_court_vendor | Named NPC tier (M3 fixed-profile vendor per `design/quick/quick-design-m3-objective-npc-loot.md:184`); same gating | SSS POC verdict |

## Open art decisions

1. **Cleric class baseline silhouette specifics** — S5.1 names "Cleric: layered mid-length vertical emphasis" but the exact garment cut, layered fold count, and drape needs AD ratification on the first-pass concept before spec #12 moves SPEC → PROD. Treat current spec as ready for first-pass concept generation; final geometry requires AD pass.
2. **NamedSoloBlock narrative identity** — currently spec'd as creature-tier with larger silhouette + faction-baseline material. If M3 design or future GDD reveals NamedSoloBlock as a faction-affiliated entity (e.g., a Court attendant gone feral), spec #11 moves to named-NPC tier and becomes BLOCKED.
3. **F-09 hardware target** — polygon budgets reference the bible's GTX 1070 min-spec claim; `.claude/docs/technical-preferences.md` performance section still says `[TO BE CONFIGURED]`. Technical Director + producer decision needed: lock spec in technical-preferences.md (and profile to back it) or soften the bible's "tech-validated" language. Either path requires a new D-entry in `DECISIONS.md`. Tracked as `art_bible_hardware_target_drift` carryover.
4. **Lichguard/Hand of the Compact garment vocabulary** — Cleric's progression-tier garment vocabulary (Rep Tier 2-4 per S5.1's Faction Reputation Visual Tiers table) is not specified in T1 because the player starts at Rep Tier 0-1 (onboarding). When the player accumulates Vampire Court rep, garments shift per S5.1 vocabulary — that's T2 spec scope, not T1.

## Production handoff checklist

Before any spec moves SPEC → PROD:

- [ ] AD reviews spec for faithful translation of bible principles ("cause test," "weight and age," "no spectacle")
- [ ] Tech-artist confirms texture/poly budgets are achievable on locked hardware target (currently blocked by F-09)
- [ ] AI generation prompt produces an output the AD approves as a starting concept
- [ ] Artist (or AI-assisted pipeline) executes against spec
- [ ] Output validated against the bible's "cause test" — every patch of discoloration, every worn edge, every wear pattern has an identifiable physical cause
- [ ] Output imported into Unity 6.3 LTS + URP and passes S8.2 naming-convention validation
- [ ] Output preserves the 80px silhouette legibility target (S3.1)
- [ ] Output does NOT introduce any S3.6 / S4.8 / S5 / S6.1 / S7.11 / S8 forbidden patterns

## Source traceability

This manifest and the 18 specs draw from:

- `design/art/art-bible.md` v1.0 (2026-04-22; RATIFIED WITH NOTES 2026-05-15 by commit `0bcce46`) — visual identity, all 9 sections
- `design/art/art-bible-t1-scope.md` — T1 M3 phasing (just landed in commit `0bcce46`)
- `design/gdd/game-concept.md:340` — T1 MVP: 1 class (Cleric), 1 haunt zone, 1 faction (Vampire Court), 1 city hub skeleton
- `design/quick/quick-design-m3-objective-npc-loot.md` — M3 enemy types (`SoloTrash_EvenCon_T1`, `LinkedTrash_T1`, `NamedSoloBlock_T1`), objective props (`CourtMarkedRelic_T1`, `GraveDust_Salvage_T1`), named NPCs (`M3_Caretaker_T1`, `M3_CourtVendor_T1` — both BLOCKED)
- `production/sprints/sprint-2.md` — Sprint 2 First District target (Mournwall Cemetery District working name)
- `DECISIONS.md` D001 (URP only), D003 (T1 single-player offline), D012 (Combat Feel Prototype validation)

## Next recommended

1. **AD review** of all 18 specs before any move to PROD. Suggested batched review: env (1-8) first, then characters (9-12), then HUD (13), then Layer 2 + props (14-18).
2. **Tech-artist + Technical Director resolve F-09** hardware target governance drift.
3. **First production pass** — recommend starter asset `env_arch_stone_ashlar_s3_vc_yr200` (primary tileable facade): represents 80% of the district's surface area per the 80/20 rule, validates the cause-test pipeline, and produces immediately-visible art improvement vs the current flat-color floor.
4. **SSS POC** (separate task, spawned 2026-05-15) unblocks B1-B3 named NPCs.
