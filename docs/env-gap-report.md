# Environment Gap Report

**Date:** 2026-08-13
**Scope:** Roadmap Phase 5E environment-dependent implementations

## Implemented

- Tray host: `H.NotifyIcon.Wpf`, context menu, balloon delivery, message-only hotkey window, and `RegisterHotKey`/`UnregisterHotKey` wrappers.
- Compliance/session: `GetLastInputInfo` idle polling, workstation lock request, input-desktop lock detection, audit/event propagation, and disposal.
- Updater: HTTPS manifest polling, Ed25519 verification, channel/rollout checks, hash-verified download/staging when explicitly enabled, MSIX installation through `Add-AppxPackage`, and last-known-good rollback.
- Bug reporting: runtime-configured SMTP and GitHub Issues delivery. Missing configuration returns an explicit failure and never logs credentials.
- Reminder fallback: Tier 1 reports unavailable without package identity; Tier 3 uses the live tray balloon path.

## Open Environment Gates

- WPF toast activation needs an installed package identity and a real Windows notification profile.
- MSIX/WAP build, production signing, `Add-AppxPackage`, rollback, and installed-binary launch were not run here.
- SMTP requires `TASKTREE_SMTP_*` deployment settings and a safe test mailbox; GitHub requires `TASKTREE_GITHUB_REPOSITORY` and `TASKTREE_GITHUB_TOKEN` with provider approval.
- Real lock/unlock, Credential UI, crash injection, and PHI-redaction source-list validation require an owner-controlled Windows/provider test environment.

**Status:** Code paths are locally buildable and fail closed; live environment acceptance remains open.
