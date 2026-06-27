# PHASE2C-MSG2-DERIVATIONS-DELTA.md - Phase 2C Msg 2 (MainWindow Integration + TaskBuilder Wiring)

> Scope: Patch MainWindow XAML and MainWindowViewModel to host TaskBuilderViewModel and wire TaskCreated into the task list.
> Owner-approved HALT batch: 18 items.

## Section 1 Summary Table

| # | Item | Resolution | Gap |
|---|---|---|---|
| 1 | Builder UI location | Inline panel in MainWindow | #134 |
| 2 | Builder ownership | MainWindowViewModel owns direct-created Builder | #135 |
| 3 | TaskCreated update | Append directly to Tasks | #136 |
| 4 | DataGrid -> TreeView timing | Keep DataGrid flat baseline | #117 remains |
| 5 | Builder layout | Functional GroupBox panel | #137 |
| 6 | Lab due input | Deferred visible input | #138 |
| 7 | Builder property | Added Builder property | #133 closed |
| 8 | TaskCreated handler | Appends node + status | #133 closed |
| 9 | Duplicate prevention | Lightweight Id check | #139 |
| 10 | QuickAdd coexistence | Both paths remain | - |
| 11 | Tests | 4 integration tests | - |
| 12 | XAML tests | Deferred | #140 |
| 13 | Test count | Phase 2C total 18 tests | - |
| 14 | Marker count | PHASE2C 4 -> 6 | - |
| 15 | Closure criteria | Phase 2C complete after Msg 2 | - |
| 16 | Gaps closed | #133 closed | - |
| 17 | New gaps | #134-#140 | - |
| 18 | Artifact manifest | 6 files emitted | - |

## Section 2 Files Produced

| Path | Purpose | Marker |
|---|---|---|
| src/TaskTree.UI/Views/MainWindow.xaml | PATCH inline builder panel | XAML |
| src/TaskTree.UI/ViewModels/MainWindowViewModel.cs | PATCH Builder property + TaskCreated wiring | PHASE2B + PHASE2C |
| tests/TaskTree.UI.Tests/MainWindowViewModelTests.cs | PATCH 4 tests | PHASE2B + PHASE2C |
| docs/spec-derivations/PHASE2C-MSG2-DERIVATIONS-DELTA.md | This registry | md |
| docs/HANDOFF-v1.0.30-delta.md | Phase 2C complete | md |
| tools/find-spec-derivations.ps1 | PHASE2C = 6 | script |

## Section 3 Marker Inventory

PHASE2C = 6 distinct .cs files after Msg 2. Grand total: 91 -> 93.

## Section 4 Cross-Phase Gaps Introduced (134-140)

| # | Gap | Target | Action |
|---|---|---|---|
| 134 | Inline builder panel should be styled/refactored | Phase 2D | Convert to themed modal/dialog if desired |
| 135 | Direct TaskBuilderViewModel construction should be evaluated for DI replacement | Phase 5C | Replace with DI if UI composition grows |
| 136 | Direct append after TaskCreated must match TaskEngine sort/order semantics | Phase 5C | Verify after ordering finalized |
| 137 | TaskBuilder UI needs Theme + Color restyle | Phase 2D | Apply resources/styles |
| 138 | LabDueAtUtc UI input deferred | Phase 2E/4A | Add DatePicker/converter or workflow policy |
| 139 | Duplicate display after builder create + refresh must be verified | Phase 5C | Integration test |
| 140 | WPF Application integration test for builder panel | Phase 5C | Verify MainWindow loads + builder panel binds |

## Section 5 Phase 2C Closure

Phase 2C COMPLETE. TaskMetadata exists, TaskNode.Metadata exists, TaskBuilderViewModel exists and is tested, MainWindow exposes builder surface, MainWindowViewModel wires TaskCreated, and HANDOFF v1.0.30 emitted.

## Section 6 Known Limitations

1. DataGrid remains flat; Gap #117 remains deferred.
2. Inline UI is functional but not styled; Gaps #134/#137 deferred to Phase 2D.
3. LabDueAtUtc not exposed in MainWindow XAML; Gap #138.
4. Direct ViewModel construction should be re-evaluated; Gap #135.
5. Duplicate/sort behavior requires integration verification; Gap #136/#139.
6. Real WPF Application test deferred to Phase 5C; Gap #140.
