# Test Gap Report

**Date:** 2026-08-16
**Scope:** Roadmap Phase 5C, SDK-backed local validation

## Evidence

- `rtk C:\Users\domin\.dotnet\dotnet.exe build TaskTree.sln -c Release --no-restore`: passed, 0 warnings, 0 errors.
- `rtk C:\Users\domin\.dotnet\dotnet.exe test TaskTree.sln -c Release --no-build --filter "TestCategory!=Live&TestCategory!=Performance"`: passed, 14 assemblies, 357 tests, 0 failures, 0 skips.
- `rtk C:\Users\domin\.dotnet\dotnet.exe test tests\TaskTree.Perf.Tests\TaskTree.Perf.Tests.csproj -c Release --no-build --filter "TestCategory=Performance&TestCategory!=Live"`: passed, 7 measurable performance tests, 0 failures, 0 skips.
- Built-in SDK coverage: `rtk C:\Users\domin\.dotnet\dotnet.exe test TaskTree.sln -c Release --no-build --filter "TestCategory!=Live&TestCategory!=Performance" --collect:"Code Coverage"`: passed, 357 tests, and produced a `.coverage` artifact under ignored `TestResults/`. SDK conversion to Cobertura reports 1,466/1,937 production source lines covered (75.68%), meeting the 75% target after dispatcher hardening.
- `git diff --check`: passed.
- Release Performance category: 7 module-backed tests passed, 1 Live desktop metrics test skipped by design. Captured timings include TaskEngine CRUD 0.3075 ms average, 1000-node fetch 0.3593 ms, ReminderScheduler 1000-task tick 4.5148 ms, SecureStore 10 MB save/load 53.4829/65.6757 ms, AuditChainWriter append 0.0229 ms average, and BugReporter submit+queue 2.2137 ms average.
- Release Stress category: 100k audit-chain append+verify passed in 1370 ms; the 10 MB SecureStore save/load case passed with 53.4829/65.6757 ms.
- Dedicated 10k audit-chain verification passed in 107 ms.
- Live/provider tests were intentionally excluded and were not represented as green.

## Remaining Gaps

Phase 5C local coverage and offline test evidence are closed. Remaining gaps belong to the later environment and final-validation gates: interactive WPF/tray/session E2E, MSIX/WAP build/sign/install, live SMTP/GitHub/updater validation, CredUI/lock testing, owner inputs Q10/Q11, and Phase 5F owner sign-off.
