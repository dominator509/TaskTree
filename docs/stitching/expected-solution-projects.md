# expected-solution-projects.md - Phase 5A Expected Solution Inventory

> Phase 5A expected project list. Phase 5B must verify the actual `TaskTree.sln` contents after stitching.

## Expected Product Projects

- `src/TaskTree.Core/TaskTree.Core.csproj`
- `src/TaskTree.App/TaskTree.App.csproj`
- `src/TaskTree.UI/TaskTree.UI.csproj`
- `src/TaskTree.Orchestrator/TaskTree.Orchestrator.csproj`
- `src/TaskTree.Modules.TaskEngine/TaskTree.Modules.TaskEngine.csproj`
- `src/TaskTree.Modules.ReminderScheduler/TaskTree.Modules.ReminderScheduler.csproj`
- `src/TaskTree.Modules.SecureStore/TaskTree.Modules.SecureStore.csproj`
- `src/TaskTree.Modules.ComplianceCore/TaskTree.Modules.ComplianceCore.csproj`
- `src/TaskTree.Modules.TrayHost/TaskTree.Modules.TrayHost.csproj`
- `src/TaskTree.Modules.AutoUpdater/TaskTree.Modules.AutoUpdater.csproj`
- `src/TaskTree.Modules.BugReporter/TaskTree.Modules.BugReporter.csproj`

## Expected Phase 2 Delta Projects If Present

- `src/TaskTree.Modules.Settings/TaskTree.Modules.Settings.csproj`
- `src/TaskTree.Modules.SessionLock/TaskTree.Modules.SessionLock.csproj`
- `src/TaskTree.Modules.Snooze/TaskTree.Modules.Snooze.csproj`

No standalone ReminderDelivery module project is present in the stitched checkout; reminder delivery types live under `src/TaskTree.Orchestrator/` and are included through `src/TaskTree.Orchestrator/TaskTree.Orchestrator.csproj`.

## Expected Test Projects

- `tests/TaskTree.Core.Tests/TaskTree.Core.Tests.csproj`
- `tests/TaskTree.Modules.TaskEngine.Tests/TaskTree.Modules.TaskEngine.Tests.csproj`
- `tests/TaskTree.Modules.ReminderScheduler.Tests/TaskTree.Modules.ReminderScheduler.Tests.csproj`
- `tests/TaskTree.Modules.SecureStore.Tests/TaskTree.Modules.SecureStore.Tests.csproj`
- `tests/TaskTree.Modules.ComplianceCore.Tests/TaskTree.Modules.ComplianceCore.Tests.csproj`
- `tests/TaskTree.Modules.TrayHost.Tests/TaskTree.Modules.TrayHost.Tests.csproj`
- `tests/TaskTree.Modules.AutoUpdater.Tests/TaskTree.Modules.AutoUpdater.Tests.csproj`
- `tests/TaskTree.Modules.BugReporter.Tests/TaskTree.Modules.BugReporter.Tests.csproj`
- `tests/TaskTree.Modules.Settings.Tests/TaskTree.Modules.Settings.Tests.csproj`
- `tests/TaskTree.Modules.SessionLock.Tests/TaskTree.Modules.SessionLock.Tests.csproj`
- `tests/TaskTree.Modules.Snooze.Tests/TaskTree.Modules.Snooze.Tests.csproj`
- `tests/TaskTree.Orchestrator.Tests/TaskTree.Orchestrator.Tests.csproj`
- `tests/TaskTree.UI.Tests/TaskTree.UI.Tests.csproj`
- `tests/TaskTree.Perf.Tests/TaskTree.Perf.Tests.csproj`
- `tests/TaskTree.TestSupport/TaskTree.TestSupport.csproj`

## Packaging Project

- `packaging/TaskTree.Installer.wapproj`

## Known Project Reference Risks

- Phase 2 generated modules may not exist in original Architecture §3.3, but the stitched checkout now contains Settings, SessionLock, and Snooze projects and their tests.
- Phase 3 AutoUpdater / BugReporter constructors changed multiple times.
- Phase 4 packaging references may not match final stitched project paths.
- Test support fakes may require namespace reconciliation.

## Verification Commands for Later Phases

```powershell
pwsh -File tools/find-spec-derivations.ps1 -Root .
dotnet restore TaskTree.sln
dotnet build TaskTree.sln -c Debug
dotnet test TaskTree.sln --no-build
```

Build/test output logs must be attached during Phase 5B/5C, not Phase 5A.

## Claude/Codex Gaps

| Gap | Description | Target |
|---|---|---|
| #361 | Phase 5B must verify `TaskTree.sln` contains every expected project after stitching | Verified statically; build proof deferred |
| #362 | Phase 5B must reconcile project references and namespace churn introduced by generated modules and tests | Reconciled statically; build proof deferred |
| #363 | Phase 5B/5C must attach actual command output logs after restore/build/test execution | Phase 5B / 5C |
