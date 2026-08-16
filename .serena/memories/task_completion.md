# Task Completion

- Read REPO_BRIEF plus Architecture/Roadmap/latest HANDOFF before edits; reconcile source/tests against authority.
- Local code gate: SDK-backed Debug/Release build with `--no-restore`, then `dotnet test ... --filter TestCategory!=Live`; report exact assembly/test counts.
- Run marker/static checks when spec-derived files change; run `git diff --check` and final `git diff --name-only` + `git status --short`.
- Packaging uses documented `packaging/build-msix.ps1`; do not install packages or treat missing certificate/MSIX/provider prerequisites as repo failures.
- Final acceptance requires evidence for coverage, interactive Windows E2E, MSIX signing/install, provider delivery, owner PHI/support-email inputs, and owner sign-off.
- Updater invariant: `AutoUpdater.CheckAsync` and `ApplyAsync` share one operation gate around `UpdaterStateMachine`; preserve this serialization when adding updater entrypoints.
- Compliance invariant: `ComplianceCore` idle-monitor start/replacement, disposal, and timer callback handling share a lifecycle gate; disposed instances reject restart and callbacks must stop before workstation locking.
- Hotkey invariant: `HotkeyManager` serializes config reads, initialization, replacement, and disposal; if native replacement persistence fails, restore the prior binding before surfacing the failure.
- Bug-report invariant: SMTP and GitHub delivery calls are bounded to 5 seconds; crash-hook subscription is idempotent at both the hook and reporter layers.
- Bug-report queue invariant: encrypted retention metadata tracks pending/delivered state; flush retries pending reports only, successful reports purge after 7 days, failed reports purge after 30 days, and metadata survives queue-instance recreation.
- Session/scheduler invariant: session lifecycle operations serialize through disposal, and `ReminderScheduler` loops own their `PeriodicTimer` reference rather than rereading a field that stop can clear.