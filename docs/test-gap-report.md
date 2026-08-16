# Test Gap Report

**Date:** 2026-08-16
**Scope:** Roadmap Phase 5C, SDK-backed local validation

## Evidence

- `rtk C:\Users\domin\.dotnet\dotnet.exe build TaskTree.sln -c Release --no-restore`: passed, 0 warnings, 0 errors.
- `rtk C:\Users\domin\.dotnet\dotnet.exe test TaskTree.sln -c Release --no-build --no-restore --filter "TestCategory!=Live&TestCategory!=Performance"`: passed, 14 assemblies, 386 tests, 0 failures, 0 skips. The full solution run was 398 total tests, with 397 passed and 1 intentionally skipped Live desktop-metrics test.
- `rtk C:\Users\domin\.dotnet\dotnet.exe test tests\TaskTree.Perf.Tests\TaskTree.Perf.Tests.csproj -c Release --no-build --filter "TestCategory=Performance&TestCategory!=Live"`: passed, 7 measurable performance tests, 0 failures, 0 skips.
- Built-in SDK coverage: `rtk C:\Users\domin\.dotnet\dotnet.exe test TaskTree.sln -c Release --no-build --no-restore --filter "TestCategory!=Live&TestCategory!=Performance" --collect:"Code Coverage"`: passed 386 tests and produced a fresh `.coverage` artifact under ignored `TestResults/`. The last fully converted production-source report is 1,651/2,137 lines covered (77.26%); no repo-local Cobertura converter is checked in.
- `git diff --check`: passed.
- Release Performance category: 7 module-backed tests passed, 1 Live desktop metrics test skipped by design. Captured timings include TaskEngine CRUD 0.3075 ms average, 1000-node fetch 0.3593 ms, ReminderScheduler 1000-task tick 4.5148 ms, SecureStore 10 MB save/load 53.4829/65.6757 ms, AuditChainWriter append 0.0229 ms average, and BugReporter submit+queue 2.2137 ms average.
- Release Stress category: 100k audit-chain append+verify passed in 1370 ms; the 10 MB SecureStore save/load case passed with 53.4829/65.6757 ms.
- Dedicated 10k audit-chain verification passed in 107 ms.
- Live/provider tests were intentionally excluded and were not represented as green.
- Updater integrity tests cover temporary-file promotion, unsafe manifest-version rejection, and staged-package revalidation before install.
- BugReporter concurrency tests cover transactional queue submissions and single-delivery behavior for overlapping flush calls.
- BugReporter delivery tests cover atomic file-drop replacement, concurrent limiter recording, router/adapter delivery behavior, and 7/30-day retention; the focused BugReporter lane passed 74 tests.
- Additional hardening tests cover concurrent snooze writes, reminder stop draining and post-stop callback suppression, same-second logger rotation, missing offline package metadata, and atomic sentinel replacement.
- Additional hardening tests cover updater check serialization around the shared state machine and rejection of idle-monitor restart after compliance disposal.
- Additional hardening tests cover serialized hotkey persistence, repeated crash-hook registration, and session disposal during an in-flight start audit.
- Retention tests cover pending-versus-delivered state, delivery-time anchoring, failed-report expiry, and encrypted metadata persistence across queue instances.

## Remaining Gaps

Phase 5C local coverage and offline test evidence are closed. Remaining gaps belong to the later environment and final-validation gates: interactive WPF/tray/session E2E, MSIX/WAP build/sign/install, live SMTP/GitHub/updater validation, CredUI/lock testing, owner inputs Q10/Q11, and Phase 5F owner sign-off.
