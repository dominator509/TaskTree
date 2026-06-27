# PHASE2B-MSG2-DERIVATIONS-DELTA.md - Phase 2B Msg 2 (Tier 2 Adapter Patch + Orchestrator MainWindow Wiring)

> **Scope:** Patch `ToastTier2Adapter.cs` to consume `ReminderToast` + patch `Orchestrator.cs` to show `MainWindow` on `ShowTreeRequested` + patch `TaskTree.Orchestrator.csproj` + emit Phase 5A Tier 2 deletion manifest.
> **Companion:** PHASE2B-DERIVATIONS.md (Msg 1 base).
> **Owner-approved HALT batch:** executed directly per proceed directive.

## Section 1 Summary Table

| # | Item | Resolution | LOAD-BEARING? |
|---|---|---|---|
| 1 | Tier 2 adapter replacement | `ReminderToast` + `ToastViewModel` now used | Closes Gap #80 / #119 partial |
| 2 | Auto-close timer ownership | Stays in adapter | No (Gap #121) |
| 3 | Orchestrator show-tree wiring | `OnShowTreeRequested` now instantiates/shows `MainWindow` | Closes Gap #118 |
| 4 | MainWindow lifetime | Reused singleton instance inside Orchestrator | No (Gap #125) |
| 5 | TaskTree.Orchestrator project refs | Add `TaskTree.UI` ProjectReference | No (Gap #124) |
| 6 | Phase 5A old Tier 2 window deletion | Manifest emitted | No (Gap #126) |

## Sections 2-7 Detail

See HANDOFF v1.0.28 Section C for exact decisions.

## Section 8 Files Produced

| Path | Purpose | Marker |
|---|---|---|
| src/TaskTree.Orchestrator/ToastTier2Adapter.cs | PATCHED to use ReminderToast + ToastViewModel | PHASE1G + PHASE2B |
| src/TaskTree.Orchestrator/Orchestrator.cs | PATCHED to show MainWindow on tray event | PHASE1F + PHASE1G-MSG2 + PHASE2B |
| src/TaskTree.Orchestrator/TaskTree.Orchestrator.csproj | PATCHED add TaskTree.UI ProjectReference | (csproj) |
| docs/Phase5A-tier2-deletion-manifest.md | NEW manifest | (md) |
| docs/spec-derivations/PHASE2B-MSG2-DERIVATIONS-DELTA.md | This registry | (md) |
| docs/HANDOFF-v1.0.28-delta.md | Phase 2B COMPLETE delta | (md) |
| tools/find-spec-derivations.ps1 | UPDATED PHASE2B 6 -> 8 | (script) |

## Section 9 Marker Inventory

PHASE2B gains 2 additional .cs files via patched existing members (`ToastTier2Adapter.cs`, `Orchestrator.cs`).
PHASE2B count: 6 -> 8. Grand total bucket sum: 85 -> 87.

## Section 10 Cross-Phase Gaps Introduced (rows 123-126)

| # | Gap | Source | Target Phase | Action |
|---|---|---|---|---|
| 123 | Orchestrator directly constructs MainWindowViewModel/MainWindow to avoid ctor churn + test breakage | Phase 2B Msg 2 | Phase 5C | Verify this direct-construction path under real DI/WPF context; if rejected, replace with DI singleton injection |
| 124 | TaskTree.Orchestrator.csproj add TaskTree.UI ProjectReference | Phase 2B Msg 2 | (closed in Msg 2) | None |
| 125 | MainWindow singleton lifetime managed inside Orchestrator | Phase 2B Msg 2 | Phase 5C | Verify no duplicate windows or stale DataContext under repeated ShowTreeRequested cycles |
| 126 | Phase 5A delete legacy ToastTier2Window files after adapter patch | Phase 2B Msg 2 | Phase 5A | Follow Phase5A-tier2-deletion-manifest.md |

## Section 11 Phase 2B Closure Summary

Sub-Phase 2B COMPLETE after this Msg.

| Metric | Value |
|---|---|
| HALT items total resolved across 2B | 24 (Msg 1) + adapter/wiring patch decisions (Msg 2) |
| New cross-phase gaps introduced | 4 (rows #123-#126) |
| LOAD-BEARING items closed in 2B | Gap #80 partial -> complete, Gap #118 closed, Gap #119 patch emitted |
| New WPF/MVVM pattern | TaskTree.UI project established |
| Tests added in 2B | 18 (12 MainWindowViewModel + 6 ToastViewModel) |

## Section 12 Known Limitations

1. Orchestrator currently constructs `MainWindowViewModel` + `MainWindow` directly rather than resolving from DI (Gap #123) to avoid constructor churn in Phase 1F tests. Phase 5C verifies or replaces.
2. MainWindow singleton reuse inside Orchestrator must be verified under repeated show/hide cycles (Gap #125).
3. Phase 5A still must delete the old `ToastTier2Window` files (Gap #126).
4. `TaskTree.Orchestrator.csproj` ProjectReference to `TaskTree.UI` must be preserved through repo stitching.
5. Theme resources still deferred to Phase 2D (Gap #120).
6. Phase 1G+1H skeleton tests (Gap #95) still pending Codex backfill.
