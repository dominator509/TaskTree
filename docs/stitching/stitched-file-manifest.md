# stitched-file-manifest.md - Phase 5A Stitched File Manifest

> Phase 5A manifest updated from the stitched repository during Phase 5B static closure.
> This is a compact inventory for project-inclusion review, not a generated hash manifest.

## Verified Source Projects

- `src/TaskTree.App/TaskTree.App.csproj`
- `src/TaskTree.Core/TaskTree.Core.csproj`
- `src/TaskTree.UI/TaskTree.UI.csproj`
- `src/TaskTree.Orchestrator/TaskTree.Orchestrator.csproj`
- `src/TaskTree.Modules.TaskEngine/TaskTree.Modules.TaskEngine.csproj`
- `src/TaskTree.Modules.ReminderScheduler/TaskTree.Modules.ReminderScheduler.csproj`
- `src/TaskTree.Modules.SecureStore/TaskTree.Modules.SecureStore.csproj`
- `src/TaskTree.Modules.ComplianceCore/TaskTree.Modules.ComplianceCore.csproj`
- `src/TaskTree.Modules.TrayHost/TaskTree.Modules.TrayHost.csproj`
- `src/TaskTree.Modules.AutoUpdater/TaskTree.Modules.AutoUpdater.csproj`
- `src/TaskTree.Modules.BugReporter/TaskTree.Modules.BugReporter.csproj`
- `src/TaskTree.Modules.Settings/TaskTree.Modules.Settings.csproj`
- `src/TaskTree.Modules.SessionLock/TaskTree.Modules.SessionLock.csproj`
- `src/TaskTree.Modules.Snooze/TaskTree.Modules.Snooze.csproj`

No standalone `ReminderDelivery` module project is present; reminder delivery lives under `src/TaskTree.Orchestrator/`.

## Verified Test Projects

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

## Verified Tooling And Packaging

- `tools/find-spec-derivations.ps1`
- `packaging/TaskTree.Installer.wapproj`
- `packaging/Package.appxmanifest`
- `packaging/build-msix.ps1`

## Verification Notes

- `TaskTree.sln` has been reconciled to include the verified source/test project set except the packaging WAP, which remains Phase 5E-scoped.
- `TaskTree.TestSupport` is the canonical shared helper project. Old `tests/TaskTree.Core.Tests/TestDoubles/` files were removed during Phase 5B static closure.
- `src/TaskTree.Orchestrator/Views/ToastTier2Window.xaml` and `.xaml.cs` remain intentionally present until the Tier 2 deletion manifest's Release-build precondition can be satisfied.
- Actual `dotnet restore`, Debug build, Release build, and test execution are still blocked locally because `dotnet` is not available on PATH.

## Phase 5B Review Targets

- Confirm every generated `.cs` file is included in a project or intentionally excluded.
- Confirm every `.csproj` is included in `TaskTree.sln` where appropriate.
- Confirm packaging paths match the stitched source tree.
- Confirm test support fakes compile and namespaces resolve.

## Claude/Codex Gap

Gap #358: Phase 5A must create a stitched file manifest so Phase 5B can verify project inclusion and missing files.
