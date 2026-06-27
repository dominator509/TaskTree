# PHASE2B-DERIVATIONS.md - Phase 2B Msg 1 (TreeViewUI + ReminderToast WPF/MVVM)

> **Scope:** TaskTree.UI project + MainWindow + MainWindowViewModel + ReminderToast + ToastViewModel + tests.
> **Companion:** PHASE2A-DERIVATIONS.md + PHASE2A-MSG2-DERIVATIONS-DELTA.md.
> **Owner-approved HALT batch:** 24 items.

## Section 1 Summary Table

| # | Item | Resolution | LOAD-BEARING? |
|---|---|---|---|
| 1 | TaskTree.UI project creation | NEW WPF class library | No (Gap #115) |
| 2 | TaskTree.UI.Tests project creation | NEW MSTest project | No |
| 3 | Folder structure | Views/ + ViewModels/ subfolders | No |
| 4 | InternalsVisibleTo | Deferred to Msg 2 if needed | No (Gap #116) |
| 5 | MainWindow.xaml shape | Window + 3-row Grid + DataGrid | No |
| 6 | Tree display control | DataGrid (flat baseline) | No (Gap #117 Phase 2C) |
| 7 | MainWindow.xaml.cs | Minimal partial; DI ctor | No |
| 8 | MainWindowViewModel class | ObservableObject + source generators | No |
| 9 | MainWindowViewModel surface | 4 ObservableProperty + 2 RelayCommand + InitializeAsync | No |
| 10 | Quick-add behavior + validation | Title required + max 200 chars + try/catch | No |
| 11 | ReminderToast.xaml shape | 380x120 transparent overlay + MVVM binding | No (Gap #80 partial closure) |
| 12 | ReminderToast.xaml.cs | Minimal partial; ShowActivated=false; bottom-right anchor | No |
| 13 | ToastViewModel class | ObservableObject + UpdateContent method | No |
| 14 | Priority color mapping | Hard-coded brushes Red/Orange/Yellow/LightBlue/Gray | No (Gap #120 Phase 2D) |
| 15 | ReasonLabel format | "(Reason)" except Trivial-Overdue empty | No |
| 16 | Auto-close timer location | Adapter (Msg 2 patch) | No (Gap #121) |
| 17 | XAML data binding mode | OneWay | No |
| 18 | MainWindowViewModelTests count | 12 tests | No |
| 19 | ToastViewModelTests count | 6 tests | No |
| 20 | ITaskEngine mock strategy | Moq Strict with AddAsync + GetTreeAsync setups | No |
| 21 | IClock test double | FakeClock from TaskTree.TestSupport | No |
| 22 | UI thread / Dispatcher | ObservableCollection direct updates | No (Gap #122 Phase 5C) |
| 23 | Msg structure | Msg 1 = production + tests; Msg 2 = adapter patch + Orchestrator wiring | No |
| 24 | PowerShell PHASE2B marker | PHASE2B = 6 | No |

## Sections 2-25 Per-Item Detail

See HANDOFF v1.0.27 Section C R22 for bullet summary of each HALT resolution.

## Section 26 Files Produced

| Path | Purpose | Marker |
|---|---|---|
| src/TaskTree.UI/TaskTree.UI.csproj | NEW WPF class library | (csproj) |
| src/TaskTree.UI/Views/MainWindow.xaml | Main window XAML | (XAML) |
| src/TaskTree.UI/Views/MainWindow.xaml.cs | Code-behind | PHASE2B |
| src/TaskTree.UI/ViewModels/MainWindowViewModel.cs | MVVM ViewModel | PHASE2B |
| src/TaskTree.UI/Views/ReminderToast.xaml | Tier 2 replacement XAML | (XAML) |
| src/TaskTree.UI/Views/ReminderToast.xaml.cs | Code-behind | PHASE2B |
| src/TaskTree.UI/ViewModels/ToastViewModel.cs | MVVM ViewModel | PHASE2B |
| tests/TaskTree.UI.Tests/TaskTree.UI.Tests.csproj | NEW test project | (csproj) |
| tests/TaskTree.UI.Tests/MainWindowViewModelTests.cs | 12 tests | PHASE2B |
| tests/TaskTree.UI.Tests/ToastViewModelTests.cs | 6 tests | PHASE2B |
| docs/spec-derivations/PHASE2B-DERIVATIONS.md | This registry | (md) |
| docs/HANDOFF-v1.0.27-delta.md | Additive delta | (md) |
| tools/find-spec-derivations.ps1 | UPDATED 19 buckets | (script) |

## Section 27 Marker Inventory

PHASE2B = 6 distinct .cs files (MainWindow.xaml.cs + MainWindowViewModel.cs + ReminderToast.xaml.cs + ToastViewModel.cs + MainWindowViewModelTests.cs + ToastViewModelTests.cs). Grand total 79 -> 85.

## Section 28 Cross-Phase Gaps Introduced (rows 115-122)

| # | Gap | Source | Target Phase | Action |
|---|---|---|---|---|
| 115 | src/TaskTree.UI project creation | HALT #1 | (closed) | Architecture v1.0.2 §3.3 already lists src/TaskTree.UI/; no amendment change needed |
| 116 | InternalsVisibleTo deferred to Msg 2 | HALT #4 | Phase 2B Msg 2 | Add if internal members need testing |
| 117 | Phase 2C nested children migration DataGrid -> TreeView | HALT #6 | Phase 2C | HierarchicalDataTemplate when subtasks ship |
| 118 | MainWindowViewModel ctor does NOT inject ITrayHost or IOrchestrator | HALT #8 | Phase 2B Msg 2 | Orchestrator subscribes to TrayHost.ShowTreeRequested and shows MainWindow instance |
| 119 | Phase 2B Msg 2 ToastTier2Adapter PATCH + Phase 5A delete Phase 1G ToastTier2Window | HALT #11 | Phase 2B Msg 2 + Phase 5A | Replace Tier 2 window instantiation in adapter + delete old XAML files |
| 120 | Phase 2D Theme + Color replaces hard-coded brushes | HALT #14 | Phase 2D | Replace Brushes.Red/Orange/etc with theme resource lookups |
| 121 | Adapter owns auto-close timer + window lifecycle | HALT #16 | Phase 2B Msg 2 | Adapter pattern preserved from Phase 1G |
| 122 | Phase 5C integration test verifies UI thread behavior under real WPF Application context | HALT #22 | Phase 5C | Integration-category test |

## Section 29 Patches Pending for Msg 2

| File | Patch |
|---|---|
| src/TaskTree.Orchestrator/ToastTier2Adapter.cs | PATCH instantiate new TaskTree.UI.Views.ReminderToast(new ToastViewModel()) instead of TaskTree.Orchestrator.Views.ToastTier2Window; call viewModel.UpdateContent(evt) instead of window.UpdateContent(evt) |
| src/TaskTree.Orchestrator/Orchestrator.cs | PATCH subscribe to TrayHost.ShowTreeRequested event and display MainWindow instance (Gap #118 closure) |
| Phase 5A deletion manifest | NEW listing src/TaskTree.Orchestrator/Views/ToastTier2Window.xaml + .xaml.cs for Phase 5A cleanup (Gap #119 closure) |

## Section 30 Known Limitations

1. DataGrid baseline is flat - Phase 2C subtasks require TreeView with HierarchicalDataTemplate (Gap #117).
2. Hard-coded brush colors will be replaced with theme resources at Phase 2D (Gap #120).
3. MainWindowViewModel does not handle window show/hide directly - Orchestrator wiring at Phase 2B Msg 2 (Gap #118).
4. Tier 2 ReminderToast XAML created but ToastTier2Adapter still references Phase 1G ToastTier2Window - patch at Phase 2B Msg 2 (Gap #119).
5. UI thread behavior under real WPF Application context verified at Phase 5C (Gap #122).
6. Phase 2B Msg 1 production code does not patch ServiceRegistrations - MainWindow/MainWindowViewModel registration deferred to Phase 2B Msg 2 alongside Orchestrator wiring.
7. ModernWpfUI theming applied at the application root in Phase 2D - Phase 2B uses default WPF styling.
8. CommunityToolkit.Mvvm source generators require .NET 8.0 SDK; if build environment uses older SDK, fallback manual property change handling required (Phase 5B verification).
