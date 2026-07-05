# project-reference-reconciliation.md - Phase 5B Project / Solution Reconciliation

> Phase 5B compile-closure plan for solution, project references, package references, namespaces, and test inclusion.

## Required Solution Checks

1. Confirm `TaskTree.sln` exists.
2. Confirm every expected product project from `docs/stitching/expected-solution-projects.md` is present or intentionally absent with rationale.
3. Confirm every generated test project is present or intentionally absent with rationale.
4. Confirm `tests/TaskTree.Perf.Tests/TaskTree.Perf.Tests.csproj` is included if performance tests are retained.
5. Confirm packaging project path is valid for Phase 5E, even if not built during normal `dotnet build`.

## Verified Static Status

| Check | Status | Evidence |
|---|---|---|
| `TaskTree.sln` exists | Verified | `rtk git diff --name-only` and direct solution inspection. |
| Expected product projects | Verified statically | `src/**/*.csproj` contains Core, App, UI, Orchestrator, TaskEngine, ReminderScheduler, SecureStore, ComplianceCore, TrayHost, AutoUpdater, BugReporter, Settings, SessionLock, and Snooze. |
| Generated test projects | Verified statically | `tests/**/*.csproj` contains Core/module tests, UI/Orchestrator tests, generated Settings/SessionLock/Snooze tests, perf tests, and TestSupport. |
| `TaskTree.Perf.Tests` solution inclusion | Verified statically | `TaskTree.sln` includes `tests\TaskTree.Perf.Tests\TaskTree.Perf.Tests.csproj`. |
| `TaskTree.TestSupport` solution inclusion | Verified statically | `TaskTree.sln` includes `tests\TaskTree.TestSupport\TaskTree.TestSupport.csproj`. |
| Packaging project path | Verified path only | `packaging/TaskTree.Installer.wapproj`, `Package.appxmanifest`, and `build-msix.ps1` exist; packaging build is Phase 5E-scoped. |
| Phase 2 generated module references from app | Verified statically | `TaskTree.App.csproj` references Settings, SessionLock, and Snooze after Phase 5B reconciliation. |
| WPF project properties | Verified statically | App, UI, and Orchestrator projects use `net8.0-windows`/`UseWPF`. |
| XAML/code-behind class names | Verified statically | `x:Class` and partial classes were checked for App, MainWindow, ReminderToast, and ToastTier2Window. |
| Restore/build proof | Deferred | `rtk dotnet restore TaskTree.sln` cannot run because `dotnet` is unavailable on PATH (`P5B-E002`). |

No standalone `ReminderDelivery` project exists in this checkout; reminder delivery types live in `src/TaskTree.Orchestrator/` and are included through `TaskTree.Orchestrator.csproj`.

## Project Reference Reconciliation

High-risk projects:

- `TaskTree.App`
- `TaskTree.Orchestrator`
- `TaskTree.UI`
- `TaskTree.Modules.AutoUpdater`
- `TaskTree.Modules.BugReporter`
- `TaskTree.Modules.ComplianceCore`
- `TaskTree.Modules.SecureStore`
- `TaskTree.Perf.Tests`
- `TaskTree.TestSupport`

## Package Reference Policy

Allowed references must be source-approved. Expected approved categories include:

- MSTest packages for test projects.
- Moq for tests.
- Microsoft.Extensions.DependencyInjection for DI where already used.
- BCL/System.Text.Json.
- WPF/Windows App SDK references where applicable.
- NSec.Cryptography only if already source-approved for Ed25519 verification path.

Any new package must be recorded for owner review if not already source-approved.

## Namespace / File Inclusion Checks

- Verify SDK-style globbing includes generated `.cs` files.
- Verify generated test files compile in intended test project.
- Verify namespace matches project conventions.
- Prefer namespace correction or project inclusion correction before moving files.
- Any file move must be logged in `compile-resolution-log.md`.

## WPF / XAML Checks

- WPF projects should use Windows TFM and WPF properties, for example:

```xml
<TargetFramework>net8.0-windows</TargetFramework>
<UseWPF>true</UseWPF>
```

- Validate XAML class names, namespaces, partial classes, resources, commands, and properties.

## Phase 5B Gaps

| Gap | Description | Target |
|---|---|---|
| #361 | Verify `TaskTree.sln` contains every expected project after stitching | Verified statically; build proof deferred |
| #362 | Reconcile project references and namespace churn from generated modules/tests | Partially applied; build proof deferred |
| #374 | Any new package reference must be source-approved or documented for owner review | No new unapproved package added during current Codex pass |
| #377 | Namespace mismatches must be reconciled and logged if file moves are required | TestSupport namespace migration applied/logged |
| #380 | WPF/Windows-specific projects must use compatible target frameworks and properties | Verified statically |
| #381 | XAML/code-behind mismatches must be corrected or documented for UI validation | Verified statically; runtime UI validation deferred |
| #383 | Generated test files must be included in test projects and compile before Phase 5C execution | Included statically; compile proof deferred |
