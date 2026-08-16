# UI Core

- UI projects: `src/TaskTree.UI` for WPF views/viewmodels/themes and `src/TaskTree.Orchestrator` for orchestration plus Tier 2 toast window/adapter surface.
- WPF/ModernWpfUI/CommunityToolkit.Mvvm are the expected UI stack; keep UI changes aligned with architecture, not marketing-page patterns.
- Views include MainWindow and ReminderToast; viewmodels include main window, task builder, settings, toast.
- Tray/hotkey surfaces live in `TaskTree.Modules.TrayHost`; reminder delivery decision logic lives in Orchestrator adapters/services.
- `SettingsViewModel.ThemePreference` drives app-level Light/Dark/System dictionaries at runtime; System reads Windows `AppsUseLightTheme` and falls back to Light. Theme changes are dispatcher-safe and covered by STA tests.
- Tests mirror UI and orchestrator behavior under `tests/TaskTree.UI.Tests` and `tests/TaskTree.Orchestrator.Tests`; avoid weakening UI assertions to bypass WPF environment gaps.