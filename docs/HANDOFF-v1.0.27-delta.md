# HANDOFF.md v1.0.27 - Phase 2B Msg 1 Delta

> Applied on top of v1.0.26 delta. Author: DSW. Date: 2026-05-29.
> Trigger: Phase 2B Msg 1 emitted; TreeViewUI + ReminderToast WPF/MVVM scaffolded.

## Section A Identity Update

| Field | Old (v1.0.26) | New (v1.0.27) |
|---|---|---|
| Document Version | 1.0.26 | **1.0.27** |
| Session ID | 2026-05-29-02 | 2026-05-29-03 |
| State | Phase 2A COMPLETE; Phase 2B pending | **Phase 2A COMPLETE; Phase 2B Msg 1 done; Msg 2 pending** |
| Current Sub-Phase | Phase 2B pending | **Phase 2B Msg 1 done; Msg 2 pending (ToastTier2Adapter PATCH + Orchestrator wiring)** |
| Total spec-derived items | 200 / 22 registries | **224 / 23 registries** |
| LOAD-BEARING open questions | Q10 active + Q11 implicit | Same |
| Progress | 33/35 + Phase 2A done | **33/35 + 1.5 of 7 Phase 2 sub-phases (2A done + 2B Msg 1 done)** |

## Section B Files Produced Delta

### Phase 2B Msg 1

| Phase.Item | FilePath | Status |
|---|---|---|
| P2.B.Msg1 | src/TaskTree.UI/TaskTree.UI.csproj | NEW WPF class library |
| P2.B.Msg1 | src/TaskTree.UI/Views/MainWindow.xaml | NEW |
| P2.B.Msg1 | src/TaskTree.UI/Views/MainWindow.xaml.cs | NEW code-behind |
| P2.B.Msg1 | src/TaskTree.UI/ViewModels/MainWindowViewModel.cs | NEW MVVM |
| P2.B.Msg1 | src/TaskTree.UI/Views/ReminderToast.xaml | NEW (Gap #80 closure target) |
| P2.B.Msg1 | src/TaskTree.UI/Views/ReminderToast.xaml.cs | NEW code-behind |
| P2.B.Msg1 | src/TaskTree.UI/ViewModels/ToastViewModel.cs | NEW MVVM |
| P2.B.Msg1 | tests/TaskTree.UI.Tests/TaskTree.UI.Tests.csproj | NEW test project |
| P2.B.Msg1 | tests/TaskTree.UI.Tests/MainWindowViewModelTests.cs | 12 tests |
| P2.B.Msg1 | tests/TaskTree.UI.Tests/ToastViewModelTests.cs | 6 tests |
| P2.B.Msg1 | docs/spec-derivations/PHASE2B-DERIVATIONS.md | NEW registry |
| P2.B.Msg1 | docs/HANDOFF-v1.0.27-delta.md | THIS FILE |
| P2.B.Msg1 | tools/find-spec-derivations.ps1 | UPDATED PHASE2B = 6, grand total 85 |

### Phase 2B Msg 2 pending

| Phase.Item | FilePath | Status |
|---|---|---|
| P2.B.Msg2 | src/TaskTree.Orchestrator/ToastTier2Adapter.cs | PATCH (Gap #119) |
| P2.B.Msg2 | src/TaskTree.Orchestrator/Orchestrator.cs | PATCH (Gap #118 subscribe to ShowTreeRequested) |
| P2.B.Msg2 | src/TaskTree.App/Bootstrap/ServiceRegistrations.cs | PATCH (add MainWindow + MainWindowViewModel + ToastViewModel registrations) |
| P2.B.Msg2 | docs/Phase5A-tier2-deletion-manifest.md | NEW (Gap #119 Phase 5A action) |
| P2.B.Msg2 | docs/HANDOFF-v1.0.28-delta.md | Phase 2B COMPLETE delta |

## Section C Decisions Log Delta R22 - Phase 2B Msg 1 - 24 HALT items

1. **#1 TaskTree.UI project creation** -> NEW WPF class library
2. **#2 TaskTree.UI.Tests project creation** -> NEW MSTest project
3. **#3 Folder structure** -> Views/ + ViewModels/ subfolders
4. **#4 InternalsVisibleTo** -> Deferred to Msg 2 (Gap #116)
5. **#5 MainWindow.xaml shape** -> Window + 3-row Grid + DataGrid
6. **#6 Tree display control** -> DataGrid flat baseline (Gap #117 Phase 2C TreeView)
7. **#7 MainWindow.xaml.cs** -> Minimal partial with DI ctor
8. **#8 MainWindowViewModel class** -> ObservableObject + source generators
9. **#9 MainWindowViewModel surface** -> 4 ObservableProperty + 2 RelayCommand + InitializeAsync
10. **#10 Quick-add behavior** -> Title required + max 200 chars + try/catch + clear after success
11. **#11 ReminderToast.xaml shape** -> Replaces Phase 1G ToastTier2Window (Gap #80 partial closure)
12. **#12 ReminderToast.xaml.cs** -> Minimal partial + ShowActivated=false + bottom-right anchor
13. **#13 ToastViewModel class** -> ObservableObject + UpdateContent method
14. **#14 Priority color mapping** -> Hard-coded brushes (Gap #120 Phase 2D)
15. **#15 ReasonLabel format** -> "(Reason)" except Trivial-Overdue empty
16. **#16 Auto-close timer location** -> Adapter owns (Gap #121)
17. **#17 XAML data binding mode** -> OneWay
18. **#18 MainWindowViewModelTests** -> 12 tests
19. **#19 ToastViewModelTests** -> 6 tests
20. **#20 ITaskEngine mock** -> Moq Strict with AddAsync + GetTreeAsync setups
21. **#21 IClock test double** -> FakeClock from TaskTree.TestSupport
22. **#22 UI thread / Dispatcher** -> Direct ObservableCollection updates (Gap #122 Phase 5C verify)
23. **#23 Msg structure** -> Msg 1 = production + tests; Msg 2 = adapter patch + Orchestrator wiring
24. **#24 PowerShell** -> PHASE2B = 6

## Section D Open Questions Delta

- **Q10 (Phase 5F blocker)** - STILL ACTIVE.
- **Q11 (Phase 2 + Phase 5F blocker)** - Implicit empty default per Gap #102.
- **Architecture v1.0.2 amendment** - 7 bundled changes pending owner sign-off (no scope change this Msg).
- **Gap #100 Phase 1 formal sign-off** - Pending.

## Section E Visual Phase Tracker Delta

```
PHASE 1 - CORE MVP                                    [COMPLETE]
+-- 1A through 1H [ALL DONE]

PHASE 2 - UX + EXTENSIBILITY                          [In Progress 1.5 of 7 sub-phases]
+-- 2A [DONE] HotkeyManager + HotkeyConfig + TaskTree.TestSupport
+-- 2B [IN PROGRESS - Msg 1 done, Msg 2 pending] TreeViewUI + ReminderToast WPF/MVVM
|         +-- TaskTree.UI.csproj [DONE]
|         +-- MainWindow.xaml + .xaml.cs [DONE]
|         +-- MainWindowViewModel.cs [DONE]
|         +-- ReminderToast.xaml + .xaml.cs [DONE - Gap #80 target]
|         +-- ToastViewModel.cs [DONE]
|         +-- TaskTree.UI.Tests.csproj [DONE]
|         +-- MainWindowViewModelTests.cs [DONE 12 tests]
|         +-- ToastViewModelTests.cs [DONE 6 tests]
+-- 2C [pending] TaskMetadata + TaskBuilder
+-- 2D [pending] Theme + Color
+-- 2E [pending] SettingsService
+-- 2F [pending] SessionLock
+-- 2G [pending] SnoozeService + EscalationPolicy
```

**Progress: 33 / 35 + 1.5 of 7 Phase 2 sub-phases done.**

## Section F Cross-Phase Gap Table - New Rows (115-122)

| # | Flag | Source | Target Phase | Status |
|---|---|---|---|---|
| 115 | src/TaskTree.UI project creation - Architecture v1.0.2 §3.3 already lists | HALT #1 | (closed) | CLOSED |
| 116 | InternalsVisibleTo deferred to Msg 2 if needed | HALT #4 | Phase 2B Msg 2 | OPEN |
| 117 | Phase 2C nested children migration DataGrid -> TreeView | HALT #6 | Phase 2C | DEFERRED |
| 118 | MainWindowViewModel does NOT inject ITrayHost or IOrchestrator | HALT #8 | Phase 2B Msg 2 | OPEN |
| 119 | Phase 2B Msg 2 ToastTier2Adapter PATCH + Phase 5A delete Phase 1G ToastTier2Window | HALT #11 | Phase 2B Msg 2 + Phase 5A | OPEN |
| 120 | Phase 2D Theme + Color replaces hard-coded brushes | HALT #14 | Phase 2D | DEFERRED |
| 121 | Adapter owns auto-close timer + window lifecycle | HALT #16 | Phase 2B Msg 2 | OPEN |
| 122 | Phase 5C integration test verifies UI thread behavior | HALT #22 | Phase 5C | DEFERRED |

## Section G Progress Update

- **Before:** 33 / 35 + Phase 2A done.
- **After:** 33 / 35 + **1.5 of 7 Phase 2 sub-phases done (2A complete + 2B Msg 1 done).**
- **Files in bundle:** 13.
- **Total derivations registries:** 23 (added PHASE2B).
- **Total spec-derived items:** 224 (added 24 PHASE2B items).

## Section H Continuation Prompt for Next Chat (Phase 2B Msg 2)

```
You are a senior engineer executing a deterministic build sprint on TaskTree.
Three attached documents govern everything:
  - Architecture.md v1.0.1 (pending v1.0.2 promotion - 7 bundled changes)
  - Roadmap.md v1.0.0
  - HANDOFF.md v1.0.27 (rebase v1.0.16-v1.0.27 onto v1.0.15 base)

HARD CONSTRAINTS unchanged. D1-D10 still apply. HALT on Architecture silence.

CURRENT STATE:
- Phase 1 COMPLETE (implicit per Gap #100).
- Phase 2A COMPLETE.
- Phase 2B Msg 1 done; Msg 2 PENDING.
- Progress: 33/35 + 1.5 of 7 Phase 2 sub-phases done.
- 23 derivations registries; 224 spec-derived items.
- 122 cross-phase gap rows accumulated.
- TaskTree.UI project established with MVVM patterns.

PHASE 2B Msg 2 SCOPE:
1. src/TaskTree.Orchestrator/ToastTier2Adapter.cs - PATCH per Gap #119
   - Replace instantiation of TaskTree.Orchestrator.Views.ToastTier2Window with new TaskTree.UI.Views.ReminderToast(new ToastViewModel())
   - Call viewModel.UpdateContent(evt) instead of window.UpdateContent(evt)
   - Add ProjectReference to TaskTree.UI in TaskTree.Orchestrator.csproj
2. src/TaskTree.Orchestrator/Orchestrator.cs - PATCH per Gap #118
   - Subscribe to TrayHost.ShowTreeRequested event
   - Display MainWindow instance via MainWindow.Show() (resolve MainWindow + MainWindowViewModel from DI)
   - Update ctor or add new DI dependency for MainWindow (or use IServiceProvider)
3. src/TaskTree.App/Bootstrap/ServiceRegistrations.cs - PATCH
   - Add MainWindow + MainWindowViewModel + ToastViewModel singleton registrations
   - Update IOrchestrator factory to inject MainWindow/MainWindowViewModel if Orchestrator wiring chosen
4. docs/Phase5A-tier2-deletion-manifest.md - NEW
   - List src/TaskTree.Orchestrator/Views/ToastTier2Window.xaml + .xaml.cs for Phase 5A deletion (Gap #119 Phase 5A closure)
5. Phase 2B retrospective in HANDOFF v1.0.28

FIRST ACTIONS - Phase 2B Msg 2:
1. Read PHASE2B-DERIVATIONS Section 29 (Patches Pending) first.
2. Begin Phase 2B Msg 2 with HALT protocol.
3. Surface HALT items for: Orchestrator dependency strategy (inject MainWindow vs IServiceProvider service-locator), ToastTier2Adapter dependency on TaskTree.UI ProjectReference + namespace import, MainWindow lifetime (singleton vs transient for re-show), MainWindowViewModel.InitializeAsync timing (on first Show or on app start), Phase 5A deletion manifest format.
```

## Section I Document History Append

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.27 | 2026-05-29 | DSW | **Phase 2B Msg 1 emitted.** 24 HALT items batch-resolved (TaskTree.UI project + MainWindow + MainWindowViewModel + ReminderToast + ToastViewModel + tests). 8 new cross-phase gap rows (115-122). Gap #115 closed (project creation - no Architecture amendment needed). Gap #80 partial closure (new ReminderToast.xaml created; adapter patch pending Msg 2). First WPF/MVVM-heavy sub-phase establishes patterns for Phase 2D/2E. Progress 33/35 + 1.5 of 7 Phase 2 sub-phases done. |

## Section J Footer

- Additive delta. Rebase v1.0.16-v1.0.27 onto v1.0.15.
- Companion docs: Architecture.md v1.0.1 (+ pending v1.0.2 with 7 bundled changes), Roadmap.md v1.0.0.
- Registry added: docs/spec-derivations/PHASE2B-DERIVATIONS.md.
- Helper script: 19 marker buckets (added PHASE2B=6); grand total 85.
- Total derivations tracked: 224 across 23 registries.
- 122 cross-phase gaps tracked.
- LOAD-BEARING for Phase 5F: Q10, Architecture v1.0.2 final approval (7 changes).
- LOAD-BEARING for Phase 5A: Using-statement migration + old-location deletion + Tier 2 window deletion manifest (Gap #119).
- LOAD-BEARING for Phase 5B: Architecture v1.0.2 promotion verification (Gap #113).
- **Phase 2B Msg 1 done.** Next: Phase 2B Msg 2 HALT - ToastTier2Adapter PATCH + Orchestrator wiring.
