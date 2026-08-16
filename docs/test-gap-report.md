# Test Gap Report

**Date:** 2026-08-16
**Scope:** Roadmap Phase 5C, SDK-backed local validation

## Evidence

- `rtk C:\Users\domin\.dotnet\dotnet.exe build TaskTree.sln -c Release --no-restore`: passed, 0 warnings, 0 errors.
- `rtk C:\Users\domin\.dotnet\dotnet.exe test TaskTree.sln -c Release --no-build --filter "TestCategory!=Live&TestCategory!=Performance"`: passed, 13 applicable assemblies, 410 tests, 0 failures, 0 skips. The last Live-inclusive baseline before this delta was 416 total tests, with 415 passed and 1 intentionally skipped Live desktop-metrics test; the current Live-inclusive lane was not repeated.
- `rtk C:\Users\domin\.dotnet\dotnet.exe test tests\TaskTree.Perf.Tests\TaskTree.Perf.Tests.csproj -c Release --no-build --filter "TestCategory=Performance&TestCategory!=Live"`: passed, 7 measurable performance tests, 0 failures, 0 skips.
- Built-in SDK coverage: the previous `rtk C:\Users\domin\.dotnet\dotnet.exe test TaskTree.sln -c Release --no-build --filter "TestCategory!=Live&TestCategory!=Performance" --collect:"Code Coverage"` run passed 404 tests and produced a fresh `.coverage` artifact under ignored `TestResults/`. The last fully converted production-source report is 1,651/2,137 lines covered (77.26%); coverage was not rerun for this lifecycle-only delta.
- `git diff --check`: passed.
- Release Performance category: 7 module-backed tests passed, 1 Live desktop metrics test skipped by design. Current captured timings include TaskEngine CRUD 0.3995 ms average, 1000-node fetch 0.4345 ms, ReminderScheduler 1000-task tick 0.2540 ms after warmup, SecureStore 10 MB save/load 41.3432/47.8530 ms, AuditChainWriter append 0.0230 ms average, and BugReporter submit+queue 2.7860 ms average.
- Release Stress category: 100k audit-chain append+verify passed in 1370 ms; the 10 MB SecureStore save/load case passed with 53.4829/65.6757 ms.
- Dedicated 10k audit-chain verification passed in 107 ms.
- Live/provider tests were intentionally excluded and were not represented as green.
- Updater integrity tests cover temporary-file promotion, unsafe manifest-version rejection, and staged-package revalidation before install.
- BugReporter concurrency tests cover transactional queue submissions and single-delivery behavior for overlapping flush calls.
- BugReporter delivery tests cover atomic file-drop replacement, concurrent limiter recording, router/adapter delivery behavior, unredacted-persistence rejection, and 7/30-day retention; the focused BugReporter lane passed 75 tests.
- Additional hardening tests cover concurrent snooze writes, reminder stop draining and post-stop callback suppression, same-second logger rotation, missing offline package metadata, and atomic sentinel replacement.
- Additional hardening tests cover updater check serialization around the shared state machine and rejection of idle-monitor restart after compliance disposal.
- Additional hardening tests cover serialized hotkey persistence, repeated crash-hook registration, and session disposal during an in-flight start audit.
- Retention tests cover pending-versus-delivered state, delivery-time anchoring, failed-report expiry, and encrypted metadata persistence across queue instances.
- TaskEngine tests cover preservation of nullable `TaskNode.Metadata` across persisted storage and defensive tree/overdue snapshots.
- Boundary tests cover required SMTP TLS, safe GitHub repository segments, offline package-size metadata, and confined SecureStore key filenames.
- Composition tests cover DI resolution of AutoUpdater/BugReporter and TaskTreePaths-backed roots; Orchestrator tests cover startup chain verification, failure auditing, and continued startup.
- New boundary tests cover updater apply-time downgrade/channel rejection, immutable PHI allowlist exposure, and serialized settings persistence/notification ordering.
- New lifecycle/security tests cover defensive master-key copies and clean retry after SessionLock audit or ReminderScheduler startup-log failure.
- New updater recovery tests cover sentinel retention on successful installation, cleanup on installer failure, first-launch marking, retry detection, and concrete DI registration.
- Orchestrator maintenance coverage verifies startup bug-report flushing and repeated updater polling with clean shutdown cancellation.
- Crash capture coverage now verifies the manual crash hook returns only after the redacted report is durably queued, while the production path remains bounded to 200 ms.
- Performance hardening coverage verifies the warmed scheduler path and UTF-8 byte-based SecureStore serialization remain below the documented thresholds.
- Background lifecycle coverage now awaits updater-poll and in-flight scheduler synchronization points, removing fixed-sleep test races under loaded parallel execution.
- UI coverage now instantiates the reusable SettingsView and MainWindow on an STA thread, validating the hierarchical task template and resource bindings at runtime.
- UI coverage now verifies runtime Light/Dark resource selection and dispatcher-safe preference changes; Orchestrator coverage verifies atomic last-known-good audit-prefix export on startup-chain failure.
- Hash-chain coverage now verifies malformed null entries fail closed; SettingsService coverage verifies invalid persisted settings return the documented defaults instead of entering the UI.
- Post-v1.0.77 rerun: 410 non-live, non-performance tests passed; 7 performance tests passed; the Live-inclusive lane was not repeated after the additive test changes.
- Orchestrator lifecycle coverage verifies compliance auto-logoff subscription cleanup, and the scheduler loop test awaits its real reminder signal with a bounded timeout instead of relying on a fixed sleep.

## Remaining Gaps

Phase 5C local coverage and offline test evidence are closed. Remaining gaps belong to the later environment and final-validation gates: interactive WPF/tray/session E2E, MSIX/WAP build/sign/install, live SMTP/GitHub/updater validation, CredUI/lock testing, owner inputs Q10/Q11, and Phase 5F owner sign-off.
