# PHASE2D-DERIVATIONS.md - Phase 2D (Theme + Color)

> Scope: ThemeResources.xaml + PriorityBrushProvider + ToastViewModel brush refactor + XAML resource merges + tests.
> Owner-approved HALT batch: 22 items.

## Section 1 Summary

All 22 HALT items approved as proposed. ThemeResources.xaml created under src/TaskTree.UI/Themes, PriorityBrushProvider centralizes priority colors, ToastViewModel no longer uses raw Brushes.Red/Orange/etc, MainWindow/ReminderToast merge theme resources locally, and Phase 2D is complete in one message.

## Section 2 Files Produced

| Path | Purpose | Marker |
|---|---|---|
| src/TaskTree.UI/Themes/ThemeResources.xaml | NEW theme resource dictionary | XAML |
| src/TaskTree.UI/Themes/PriorityBrushProvider.cs | NEW provider | PHASE2D |
| src/TaskTree.UI/ViewModels/ToastViewModel.cs | PATCH provider brush mapping | PHASE2B + PHASE2D |
| src/TaskTree.UI/Views/MainWindow.xaml | PATCH theme merge + builder panel resources | XAML |
| src/TaskTree.UI/Views/ReminderToast.xaml | PATCH theme merge + surface resources | XAML |
| tests/TaskTree.UI.Tests/PriorityBrushProviderTests.cs | NEW 5 tests | PHASE2D |
| tests/TaskTree.UI.Tests/ToastViewModelTests.cs | PATCH provider assertions | PHASE2B + PHASE2D |
| docs/Phase2D-theme-resource-key-manifest.md | NEW resource-key handoff | md |
| docs/spec-derivations/PHASE2D-DERIVATIONS.md | This registry | md |
| docs/HANDOFF-v1.0.31-delta.md | Phase 2D complete | md |
| tools/find-spec-derivations.ps1 | PHASE2D=4 | script |

## Section 3 Marker Inventory

PHASE2D = 4 distinct .cs files. Grand total: 93 -> 97.

## Section 4 Cross-Phase Gaps Introduced (141-147)

| # | Gap | Target | Action |
|---|---|---|---|
| 141 | Split ThemeResources into LightTheme/DarkTheme if runtime switching needed | Phase 2E/future | Decide when settings/app shell exist |
| 142 | App-level merged dictionary decision deferred | Phase 5C | Decide App.xaml/app-root resource merge |
| 143 | PriorityBrushProvider and ThemeResources constants must stay synchronized | Phase 5C | Verify or replace with resource lookup/converter |
| 144 | Palette values may need design review | Phase 2D/2E | Revise values if owner provides palette |
| 145 | Compile/test gate must verify ToastViewModel provider brush semantics | Phase 5B | Run tests/build |
| 146 | DataGrid/TreeView priority styling deferred until hierarchy decision | Phase 2D/2E | Revisit after Gap #117 |
| 147 | ThemeResources.xaml WPF resource-load integration test | Phase 5C | Load dictionary in WPF Application context |

## Section 5 Existing Gap Treatment

| Gap | Treatment |
|---|---|
| #120 | CLOSED: hard-coded ToastViewModel brushes replaced by PriorityBrushProvider |
| #134 | PARTIAL: inline builder panel now resource-styled; modal/dialog refactor still deferred |
| #137 | PARTIAL/CLOSED for resource styling; deeper UX polish remains future |
| #140 | Still deferred to Phase 5C |
| #117 | Still deferred; DataGrid remains flat |

## Section 6 Known Limitations

1. ThemeResources is locally merged per view; no app-level dictionary yet (Gap #142).
2. PriorityBrushProvider duplicates hex values from XAML dictionary until resource lookup/converter exists (Gap #143).
3. Palette is deterministic but not design-reviewed (Gap #144).
4. DataGrid priority styling still deferred (Gap #146).
5. Resource dictionary load test deferred to Phase 5C (Gap #147).
