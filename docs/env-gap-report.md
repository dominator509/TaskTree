# Environment Gap Report

**Date:** 2026-08-16
**Scope:** Roadmap Phase 5E environment-dependent implementations

## Implemented

- Tray host: `H.NotifyIcon.Wpf`, context menu, balloon delivery, message-only hotkey window, persisted `HotkeyManager` configuration, and `RegisterHotKey`/`UnregisterHotKey` wrappers.
- Compliance/session: `GetLastInputInfo` idle polling, workstation lock request, input-desktop lock detection, audit/event propagation, and disposal.
- Updater: HTTPS manifest polling, Ed25519 verification, channel/rollout checks, hash-verified download/staging when explicitly enabled, MSIX installation through `Add-AppxPackage`, last-known-good rollback, atomic first-launch sentinel promotion, serialized check/apply operations, and synchronized state transitions. Offline import rejects missing or size-mismatched package metadata before hash processing.
- Bug reporting: runtime-configured SMTP and GitHub Issues delivery. Missing configuration returns an explicit failure and never logs credentials; SMTP TLS is required and GitHub repository owner/name segments are constrained to safe path characters.
- Bug-report queue updates use a serialized load-modify-save gate, overlapping queue flushes are serialized, encrypted retention metadata keeps delivered reports for 7 days and failed reports for 30 days, file-drop reports promote through temporary files, and limiter/router delivery state is serialized to prevent lost submissions, partial files, duplicate local delivery, or rate-limit races.
- Reminder fallback: Tier 1 reports unavailable without package identity; Tier 3 uses the live tray balloon path.
- Dispatcher safety: scheduler-thread reminder delivery and session-lock callbacks marshal WPF windows and tray notifications onto the owning dispatcher.
- Reminder lifecycle safety: delivery callbacks are tracked, cancellation-aware, and drained before `StopAsync` returns; callbacks arriving after stop are ignored.
- Session-state safety: overlapping lock/unlock observations serialize audit and event publication, and timer callbacks do not publish a new UI transition after disposal.
- Compliance idle-monitor safety: start/replacement/disposal and timer callback handling are serialized; disposed instances reject restart and do not continue into workstation locking.
- Provider safety: SMTP and GitHub delivery calls are bounded to five seconds per report; timeout failures return redacted status text without credentials.
- Crash-capture safety: app startup installs the global hook before Orchestrator start; both global hook installation and BugReporter event subscription are idempotent, preventing duplicate queued crash reports.
- Packaging preflight: `build-msix.ps1` resolves the installed Windows SDK tools by architecture, uses the repo-local .NET 8 executable when present, stages `Package.appxmanifest` beside the published output, and fails closed before restore when required assets are absent.

## Open Environment Gates

- WPF toast activation needs an installed package identity and a real Windows notification profile.
- MSIX/WAP build, production signing, `Add-AppxPackage`, rollback, and installed-binary launch were not run here. The documented packaging command reaches the missing `packaging/Assets/` gate before restore; the WAP DesktopBridge targets are also absent.
- SMTP requires `TASKTREE_SMTP_*` deployment settings and a safe test mailbox; GitHub requires `TASKTREE_GITHUB_REPOSITORY` and `TASKTREE_GITHUB_TOKEN` with provider approval.
- Real lock/unlock, Credential UI, crash injection, and PHI-redaction source-list validation require an owner-controlled Windows/provider test environment.

**Status:** Code paths are locally buildable and fail closed; live environment acceptance remains open.
