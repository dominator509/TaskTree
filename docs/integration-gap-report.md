# Integration Gap Report

**Date:** 2026-08-16
**Scope:** Roadmap Phase 5D offline composition and application wiring

## Verified Locally

- The solution builds cleanly with the installed .NET 8 SDK.
- DI registration resolves the current module graph in the offline integration tests.
- The application composition root registers the existing `HotkeyManager` singleton and injects it into `TrayHost`; the runtime path therefore loads `hotkeys/config` before registering the global binding.
- `Orchestrator.StartAsync` now initializes the tray host, starts session locking, starts reminder scheduling and delivery, and starts the 15-minute compliance idle monitor.
- `ShowTreeRequested` creates and initializes the main WPF view model/window on the application dispatcher.
- Scheduler-thread reminder delivery and session-lock callbacks now marshal Tier 2 WPF windows, main-window privacy hides, and tray balloons onto their owning WPF dispatcher.
- Session-lock state transitions are serialized before audit completion and event publication, preserving transition order when lock/unlock observations overlap.
- Compliance idle-monitor start, replacement, disposal, and timer callback handling are serialized so a disposed monitor cannot be restarted or continue into workstation locking.
- Hotkey configuration reads, initialization, replacement, and disposal are serialized; a persistence failure after native replacement attempts to restore the prior binding.
- Session-lock start/stop/dispose operations are serialized, and reminder loop instances retain their own timer reference after stop detaches shared lifecycle fields.
- Orchestrator startup failure now unwinds attempted dependency starts and event subscriptions; shutdown continues through remaining dependencies before reporting aggregate failures.
- Reminder delivery now tracks asynchronous due-event callbacks, cancels and drains them during stop, and ignores callbacks that arrive after the service is stopped.

## Not Verified Here

- A real WPF application dispatcher, tray icon, hotkey message loop, task creation, persistence, reminder escalation, and window hide/show cycle were not executed in this headless validation session.
- The offline tests do not prove end-to-end behavior against a real interactive Windows desktop profile.

**Status:** Composition is locally green; real-machine E2E acceptance remains open.
