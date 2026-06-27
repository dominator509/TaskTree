# App Core

- App host: `src/TaskTree.App` with `App.xaml`, `App.xaml.cs`, and `Bootstrap/` composition files.
- CompositionRoot/ServiceRegistrations are high-risk integration seams: they wire TaskEngine, SecureStore, ComplianceCore, ReminderScheduler, TrayHost, Orchestrator, UI, settings/session/snooze modules as present.
- DI lifetimes and canonical `%LOCALAPPDATA%\TaskTree\...` paths are governed by Architecture/Roadmap and Phase handoff flags; do not invent alternate storage roots.
- WPF host targets `net8.0-windows` and references Core, major modules, and Orchestrator.
- Owner-controlled Q11 support-email allowlist affects compliance/redaction DI; do not silently default away the blocker in release-readiness language.