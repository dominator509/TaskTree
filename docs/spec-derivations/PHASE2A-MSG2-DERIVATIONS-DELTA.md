# PHASE2A-MSG2-DERIVATIONS-DELTA.md - Phase 2A Msg 2 (Architecture v1.0.2 Update + Phase 5A Migration Manifest)

> **Scope:** Architecture v1.0.2 delta UPDATED to 7 bundled changes (Gap #105 closure) + Phase 5A using-migration manifest (Gap #106/#112 closure) + Architecture promotion manifest (Gap #110 closure).
> **Companion to:** PHASE2A-DERIVATIONS.md (Msg 1 base).
> **Owner-approved HALT batch:** 12 items.

## Section 1 Summary Table

| # | Item | Resolution | LOAD-BEARING? |
|---|---|---|---|
| 1 | Architecture v1.0.2 7th change wording | §3.3 tests folder addition | Closes Gap #105 |
| 2 | Diff snippet format for Change 7 | Before/After ASCII tree | No |
| 3 | Owner sign-off block expansion | 7 approval checkbox groups | No |
| 4 | Phase 5A using-migration manifest scope | NEW manifest file (Gap #106 closure) | No |
| 5 | Affected test files enumeration | Revised count 4 -> 8 (Gap #112) | No |
| 6 | Migration manifest format | Table-driven with verification grep | No |
| 7 | Old-location deletion list | Gap #103/#104 closure list | No |
| 8 | Verification commands | 5 commands listed | No |
| 9 | Final Architecture.md v1.0.2 promotion | Option B prose-only manifest (Gap #110) | Yes (Gap #113) |
| 10 | Phase 2A closure | Sub-Phase 2A COMPLETE | No |
| 11 | PowerShell PHASE2A-MSG2 marker | Documentation-only Msg; no bucket (Gap #114) | No |
| 12 | Phase 2A retrospective | HANDOFF v1.0.26 Section F | No |

## Sections 2-13 Per-Item Detail

See HANDOFF v1.0.26 section C R21 for bullet summary of each HALT resolution.

## Section 14 Files Produced

| Path | Purpose | Marker |
|---|---|---|
| docs/Architecture.v1.0.2-delta.md | UPDATED to 7 bundled changes | (md) |
| docs/Phase5A-using-migration-manifest.md | NEW 8-file migration + deletion list | (md) |
| docs/Architecture-v1.0.2-promotion-manifest.md | NEW Option B prose insertion guide | (md) |
| docs/spec-derivations/PHASE2A-MSG2-DERIVATIONS-DELTA.md | This registry | (md) |
| docs/HANDOFF-v1.0.26-delta.md | Phase 2A COMPLETE delta | (md) |
| tools/find-spec-derivations.ps1 | UNCHANGED (Gap #114) | (script) |

## Section 15 Marker Inventory

**PHASE2A-MSG2 is NOT a new marker bucket** per Gap #114 (documentation-only Msg). PHASE2A remains at 5 from Msg 1. Grand total remains 79.

## Section 16 Cross-Phase Gaps Introduced (rows 111-114)

| # | Gap | Source | Target Phase | Action |
|---|---|---|---|---|
| 111 | Phase 2B+ may add Section 3.4 test-support architecture if growth warrants | HALT-Msg2 #1 | Phase 2B+ | Add subsection if test-support grows beyond FakeClock + InMemorySecureStore |
| 112 | Gap #106 file count revised from 4 to 8 | HALT-Msg2 #5 | Phase 5A | Use Phase5A-using-migration-manifest.md authoritative list |
| 113 | Phase 5B compile gate verifies Architecture v1.0.2 promotion applied | HALT-Msg2 #9 | Phase 5B | Run Architecture-v1.0.2-promotion-manifest.md Section 6 checks |
| 114 | Phase 2A Msg 2 documentation-only; no marker bucket | HALT-Msg2 #11 | (none) | Documented; no action |

## Section 17 Phase 2A Closure Summary

Sub-Phase 2A COMPLETE after this Msg.

| Metric | Value |
|---|---|
| HALT items total resolved across 2A | **34** (22 Msg 1 + 12 Msg 2) |
| New cross-phase gaps introduced | **14** (rows #100-#113; Gap #114 adds metadata) |
| LOAD-BEARING items closed in 2A | **4** (Gap #11 Phase 1E carry-forward + Gap #97 FakeClock + Gap #98 InMemorySecureStore + Gap #107 HotkeyManager IComplianceCore) |
| Architecture v1.0.2 amendment scope | **7 bundled changes** (finalized) |
| New test infrastructure pattern | **TaskTree.TestSupport project** established |
| Tests added | 10 (HotkeyManagerTests) |
| Production files added | HotkeyManager.cs + HotkeyConfig.cs + 4 docs + manifests |

## Section 18 Known Limitations

1. Phase 5A migration manifest accuracy depends on accurate enumeration of test files using FakeClock + InMemorySecureStore; if Codex finds additional consumers, add to scope as **Gap #115**.
2. Architecture.md v1.0.2 promotion is Option B prose-only manifest, not full document rewrite; Codex Phase 5B applies per the manifest.
3. Full Architecture.md v1.0.2 promoted document not generated this Msg per Gap #110/#113.
4. Phase 1G+1H test skeletons (Gap #95) still pending Codex backfill at Phase 5C.
5. Q11 implicit empty default per Gap #102 still triggers runtime warning every startup.
6. Architecture v1.0.2 amendment (7 bundled changes) still requires explicit owner sign-off before Phase 5F.
