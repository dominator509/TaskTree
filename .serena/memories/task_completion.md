# Task Completion

- Read REPO_BRIEF plus Architecture/Roadmap/latest HANDOFF before edits; reconcile source/tests against authority.
- Local code gate: SDK-backed Debug/Release build with `--no-restore`, then `dotnet test ... --filter TestCategory!=Live`; report exact assembly/test counts.
- Run marker/static checks when spec-derived files change; run `git diff --check` and final `git diff --name-only` + `git status --short`.
- Packaging uses documented `packaging/build-msix.ps1`; do not install packages or treat missing certificate/MSIX/provider prerequisites as repo failures.
- Final acceptance requires evidence for coverage, interactive Windows E2E, MSIX signing/install, provider delivery, owner PHI/support-email inputs, and owner sign-off.
- Updater invariant: `AutoUpdater.CheckAsync` and `ApplyAsync` share one operation gate around `UpdaterStateMachine`; preserve this serialization when adding updater entrypoints. Both check and apply enforce channel/version/minimum-version/rollout eligibility; app DI supplies the existing TaskTreePaths updates root.
- Compliance invariant: `ComplianceCore` idle-monitor start/replacement, disposal, and timer callback handling share a lifecycle gate; disposed instances reject restart and callbacks must stop before workstation locking.
- PHI invariant: `PhiRedactor.AllowedEmails` returns a snapshot; callers cannot mutate the backing allowlist through a read-only reference. Keep Q10/Q11 owner inputs explicit.
- Hotkey invariant: `HotkeyManager` serializes config reads, initialization, replacement, and disposal; if native replacement persistence fails, restore the prior binding before surfacing the failure.
- Bug-report invariant: SMTP and GitHub delivery calls are bounded to 5 seconds; SMTP TLS is mandatory; GitHub repository owner/name segments must be safe; crash-hook subscription is idempotent at both the hook and reporter layers. App startup installs the global hook before Orchestrator start, and DI supplies the TaskTreePaths bug-report root.
- Bug-report queue invariant: encrypted retention metadata tracks pending/delivered state; `Redacted=false` reports are rejected before persistence; flush retries pending reports only, successful reports purge after 7 days, failed reports purge after 30 days, and metadata survives queue-instance recreation.
- Settings invariant: `SettingsService` serializes reads, saves, resets, audit writes, and post-persistence change notifications through one gate; release the gate before invoking observers.
- Session invariant: `SessionLockService.StartAsync` rolls back its timer/running state when the startup audit fails, allowing a clean retry; lifecycle operations serialize through disposal.
- Scheduler invariant: `ReminderScheduler` owns its `PeriodicTimer` reference across stop and fully cancels/disposes/drains the loop when startup logging fails, allowing a clean retry.
- TaskEngine invariant: `TaskNode.Metadata` must be copied by storage and defensive tree/overdue snapshot clones.
- Offline integrity invariant: `OfflineImportService` verifies manifest package size before reading package bytes; `MasterKeyManager` confines key filenames to its configured directory, caches the DPAPI-unwrapped key internally, and returns defensive copies so callers cannot mutate the cache.
- Orchestrator startup invariant: verify the compliance audit chain before subscribing to events; invalid or failed verification logs and audits `ChainVerifyFailedAtStartup` but does not abort startup. Lifecycle audits remain limited to Startup, Shutdown, and the failure action when applicable.
