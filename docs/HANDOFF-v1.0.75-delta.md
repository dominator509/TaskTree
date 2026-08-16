# HANDOFF v1.0.75 Delta

**Date:** 2026-08-16  
**Scope:** Compliance auto-logoff UI lifecycle closure within existing contracts.

## Delivered

- `Orchestrator` now subscribes to `IComplianceCore.AutoLogoffTriggered` during startup.
- An open `MainWindow` is hidden on its owning WPF dispatcher as soon as compliance idle monitoring fires, before the OS session-lock callback arrives.
- The handler is removed during normal shutdown and failed-start cleanup; the existing tray reopen and session-lock behavior remains unchanged.
- Added offline lifecycle coverage for subscription attach/detach and existing startup cleanup behavior.

## Validation

- Focused orchestrator regression: 1 passed.
- Release build: 0 warnings, 0 errors.
- Offline suite: 405 passed, 0 failed, 0 skipped.
- Performance lane: 7 passed, 0 failed, 0 skipped.
- Derivation registry: `179/179` expected markers.
- JSON/Markdown and XAML/XML checks: passed.

## Open Gates

- Real WPF/tray/session/re-authentication E2E remains environment-gated.
- Drag-drop sibling ordering still lacks an architecture-defined persisted ordering field.
- EULA/consent, tamper warning/export, and HIPAA/Personal mode semantics remain policy-surface gaps requiring owner-approved contracts.
