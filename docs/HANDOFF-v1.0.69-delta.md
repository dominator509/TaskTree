# HANDOFF v1.0.69 Delta

**Date:** 2026-08-16  
**Scope:** Updater first-launch recovery wiring and local validation refresh.

## Delivered

- `AutoUpdater.ApplyAsync` now creates the existing atomic update sentinel before either MSIX installation path and clears it if installation fails; successful installation leaves it for the next package launch.
- `SentinelService` now owns an atomic `.started` launch-attempt marker without changing the manifest JSON format. The first post-update launch records the marker; a surviving marker on the next launch selects rollback.
- `App.OnStartup` records the first attempt, clears sentinel state only after successful Orchestrator startup, or invokes the registered `RollbackService` and exits after a failed-attempt retry.
- Application DI registers `SentinelService` and `RollbackService` against the existing `TaskTreePaths` roots. `IAutoUpdater` remains unchanged.
- Regression coverage verifies updater success/failure sentinel behavior, launch-attempt state, and concrete DI resolution.

## Validation

- AutoUpdater tests: 85 passed, 0 failed, 0 skipped.
- Orchestrator composition tests: 29 passed, 0 failed, 0 skipped.
- Release solution build: 0 warnings, 0 errors.
- Full offline suite: 401 non-live/non-performance tests passed; full solution: 413 passed and 1 intentional Live desktop-metrics test skipped; isolated performance: 7 passed.
- SDK coverage collection passed the same 401-test offline lane and produced an ignored `.coverage` artifact; the last converted production-source report remains 77.26% (1,651/2,137 lines).

## Open Gates

- Real WPF/tray/session/reminder/CredUI E2E.
- MSIX assets, WAP DesktopBridge targets, signing, install, rollback, and installed-binary launch.
- Live SMTP, GitHub, updater-provider checks; Q10/Q11 owner inputs; production updater key; Phase 5F owner sign-off.
