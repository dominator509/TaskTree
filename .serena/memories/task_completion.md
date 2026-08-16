# Task Completion

- Read REPO_BRIEF plus Architecture/Roadmap/latest HANDOFF before edits; reconcile source/tests against authority.
- Local code gate: SDK-backed Debug/Release build with `--no-restore`, then `dotnet test ... --filter TestCategory!=Live`; report exact assembly/test counts.
- Run marker/static checks when spec-derived files change; run `git diff --check` and final `git diff --name-only` + `git status --short`.
- Packaging uses documented `packaging/build-msix.ps1`; do not install packages or treat missing certificate/MSIX/provider prerequisites as repo failures.
- Final acceptance requires evidence for coverage, interactive Windows E2E, MSIX signing/install, provider delivery, owner PHI/support-email inputs, and owner sign-off.
- Updater invariant: `AutoUpdater.CheckAsync` and `ApplyAsync` share one operation gate around `UpdaterStateMachine`; preserve this serialization when adding updater entrypoints.
- Compliance invariant: `ComplianceCore` idle-monitor start/replacement, disposal, and timer callback handling share a lifecycle gate; disposed instances reject restart and callbacks must stop before workstation locking.