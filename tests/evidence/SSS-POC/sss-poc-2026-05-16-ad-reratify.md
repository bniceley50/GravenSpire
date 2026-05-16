# AD Re-Ratification — SSS Bound #3(b) Closure (2026-05-16)

**Verdict:** RATIFY WITH NOTES
**Ratifying agent:** `art-director`
**Commit under review:** `fa240fe` ("Resolve SSS cost-model bound condition; unblock skin shader /asset-spec.")
**Originating ratification:** `0bcce46` (AD-ART-BIBLE sign-off, 2026-05-15)
**Bound condition reviewed:** #3(b) URP SSS cost model — see [`design/art/art-bible.md`](../../../design/art/art-bible.md) sign-off footer

## Evidence summary

- **F-3 structural argument sound and file-backed.** URP forward rendering's per-fragment composition of diffuse + specular before any screen-space post-pass is the documented mechanism (`tests/evidence/SSS-POC/sss-poc-2026-05-16-verification.md:106-108`). Not a tuning issue. Structural.
- **Option 2 alignment with §5.4 / §8.4 named-NPC SSS expectations is defensible.** §5.4 Range Bands at `design/art/art-bible.md:807-808` require named-NPC SSS / Pre-Raphaelite portrait-grade skin at intimate range and portrait-quality face at inspection range. §8.4 Character Budgets at `design/art/art-bible.md:1579-1580` identifies SSS as the real cost (not polys) and constrains named-NPC LOD0 to inside 5m. Pre-integrated skin LUT (Option 2) operates in light-domain before per-fragment compositing — separates the diffuse scatter response that these targets require. Closed by F-4 (`tests/evidence/SSS-POC/sss-poc-2026-05-16-verification.md:110-113`).
- **§8.7 original claim correctly superseded, not silently buried.** Original "Named NPC SSS Verification" text at `design/art/art-bible.md:1669-1671` asserted "flat 1-2ms full-screen pass." Sign-off footer condition #3(b) at `design/art/art-bible.md:35-42` explicitly governs the revision; body text remains unaltered but sign-off block is authoritative.
- **§8.6 budget revision honestly deferred, not silently embedded.** Verdict at `tests/evidence/SSS-POC/sss-poc-2026-05-16-verification.md:122-123` explicitly flags §S8.6 line-item swap as Recommendation #2, not fait accompli. Sign-off footer at `design/art/art-bible.md:42` flags as pending. `/asset-spec` unblocking is scoped narrowly to skin shaders and named-NPC material slot counts.
- **Evidence honesty correct.** Verdict at `tests/evidence/SSS-POC/sss-poc-2026-05-16-verification.md:90` states "Per-pass GPU timings (the decisive question for #3(b) cost model): NOT OBTAINED" in plain text. Closure label is RESOLVED-WITH-NOTES, not VERIFIED. F-2 at `:102-104` correctly attributes the gap to `NotSupportedWithNativeGfxJobs`.

## Reasoning

Bound condition #3(b) was opened because the original §S8.7 cost-model claim was unverified and gated skin shader authoring. The PoC resolves it via a structural architectural finding rather than empirical GPU timing — a legitimate closure path: if Option 1 cannot produce correct visuals in URP forward regardless of cost, the cost model debate is moot and Option 2 is required on fidelity grounds alone. Per-pass GPU cost remaining unmeasured is honestly disclosed and does not undermine the architectural conclusion. Closure at RESOLVED-WITH-NOTES is the correct evidence-commensurate label.

## Notes (non-blocking — for next art-bible amendment pass)

- **§S8.7 body alignment:** `design/art/art-bible.md:1669-1671` still asserts the original "flat 1-2ms" cost claim. Sign-off footer governs, but a future author reading §S8.7 in isolation might miss the resolution. Next amendment should bring §S8.7 body into alignment with sign-off language.
- **Two-document split navigability:** Sign-off footer points to companion `art-bible-t1-scope.md` SSS section; companion carries full resolution rationale. Pointer is correct but author-miss risk exists. Consolidation is advisory at next amendment, not blocking now.

## Open items acknowledged

- **F-09 hardware-target governance drift:** separate Technical Director + Producer decision, tracked as `art_bible_hardware_target_drift` carryover in `production/sprint-status.yaml`. NOT a blocker for this ratification.
- **§S8.6 named-NPC budget revision:** pending next art-bible amendment pass per `tests/evidence/SSS-POC/sss-poc-2026-05-16-verification.md:122-123` Recommendation #2.

## Downstream effect

- `/asset-spec` named-NPC tier unblocked: **yes** — for `M3_Caretaker_T1`, `Sister Elara`, and `M3_CourtVendor_T1` against Option 2 as working assumption, per `design/art/art-bible.md:47-51` and `design/art/art-bible-t1-scope.md:144-145`.
- Bound condition #3(b) status: **RESOLVED-WITH-NOTES** — no revert to UNVERIFIED-BLOCKING.
