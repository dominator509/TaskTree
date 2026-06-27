# PHASE2E-MSG2-DERIVATIONS-DELTA.md - Settings UI Surface

> Scope: SettingsViewModel + inline MainWindow settings panel + Orchestrator/MainWindowViewModel pass-through + tests.

## Section 1 Summary

Settings UI surface emitted. SettingsViewModel persists preferences through ISettingsService and is hosted by MainWindowViewModel. MainWindow adds a simple settings panel. Runtime theme application remains deferred.

## Section 2 Files Produced

| Path | Purpose | Marker |
|---|---|---|
| src/TaskTree.UI/ViewModels/SettingsViewModel.cs | NEW settings VM | PHASE2E |
| src/TaskTree.UI/Views/MainWindow.xaml | PATCH settings panel | XAML |
| src/TaskTree.UI/ViewModels/MainWindowViewModel.cs | PATCH Settings property | PHASE2B/2C/2E |
| src/TaskTree.Orchestrator/Orchestrator.cs | PATCH pass ISettingsService | PHASE2B/2E |
| src/TaskTree.App/Bootstrap/ServiceRegistrations.cs | PATCH Orchestrator factory | PHASE2E |
| tests/TaskTree.UI.Tests/SettingsViewModelTests.cs | NEW 6 tests | PHASE2E |
| tests/TaskTree.UI.Tests/MainWindowViewModelTests.cs | PATCH settings test | PHASE2B/2C/2E |
| docs/spec-derivations/PHASE2E-MSG2-DERIVATIONS-DELTA.md | This registry | md |
| docs/HANDOFF-v1.0.33-delta.md | Phase 2E complete | md |
| tools/find-spec-derivations.ps1 | PHASE2E=10 | script |

## Section 3 Marker Inventory

PHASE2E increases from 6 to 10. Grand total: 103 -> 107.

## Section 4 Cross-Phase Gaps Introduced (158-163)

| # | Gap | Target | Action |
|---|---|---|---|
| 158 | Settings UI persists theme preference but does not apply runtime theme | Phase 2E/future | Implement after App.xaml/resource-root decision |
| 159 | Inline settings panel should be styled/refactored | Phase 2D/2E/future | Consider modal/settings window |
| 160 | Orchestrator ctor changed for ISettingsService; Phase 5B must update older tests | Phase 5B | Patch constructor tests/mocks |
| 161 | MainWindowViewModel constructor changed; Phase 5B must update older tests | Phase 5B | Patch test builders |
| 162 | ObjectDataProvider enum source in XAML must be validated under real WPF context | Phase 5C | Integration test |
| 163 | Settings UI lacks validation adorners for invalid snooze input | Phase 2E/future | Add validation style/converter if required |

## Section 5 Gap Treatment

Gap #157 is closed: a settings UI surface now exists. Gaps #141/#142/#158 remain open because runtime theme application is still deferred.
