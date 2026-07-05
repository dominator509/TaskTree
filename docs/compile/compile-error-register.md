# compile-error-register.md - Phase 5B Compile Error Register

> Phase 5B Compile Closure. This register must be completed by Claude/Codex against the actual stitched repository. Debug and Release builds must both pass before this register can be closed.

## Required Baseline Commands

```powershell
pwsh -File tools/find-spec-derivations.ps1 -Root .
dotnet restore TaskTree.sln
dotnet build TaskTree.sln -c Debug
dotnet build TaskTree.sln -c Release
```

Attach real command output logs during Claude/Codex execution. Do not invent results.

## Register Template

```text
Error ID:
Command:
Configuration: Debug / Release
Project:
File:
Line:
Column:
Compiler error code:
Message:
Root cause category:
Proposed fix:
Status: Open / In Progress / Resolved / Deferred
Resolution reference:
Follow-up gap:
```

## Root Cause Categories

- Missing project
- Missing package reference
- Missing using / namespace mismatch
- Constructor signature drift
- Interface contract drift
- Enum/model mismatch
- Test support fake mismatch
- XAML/code-behind mismatch
- Windows-only API / target framework mismatch
- Packaging/project path mismatch
- Duplicate type/file collision
- Nullable/reference-type issue
- Access modifier / internal test seam issue

## Compile Closure Rules

1. Fix only build/compile issues required to make the repo compile.
2. Do not redesign features or expand product scope.
3. Do not delete/exclude files unless proven duplicate or superseded.
4. Log all deletions/exclusions with rationale and downstream impact.
5. Preserve explicit Phase 5E stubs; do not fake live success.
6. New package references require source approval or owner-review documentation.

## Phase 5B Gaps

| Gap | Description | Target |
|---|---|---|
| #367 | Phase 5B compile fixes must not redesign features or expand scope beyond buildability | Phase 5B |
| #368 | Any deleted/excluded file during compile closure must be logged with rationale and downstream impact | Phase 5B |
| #369 | Marker/restore/build command outputs must be attached to compile closure artifacts | Phase 5B |
| #370 | Release configuration compile closure must be verified separately from Debug | Phase 5B |
| #371 | Maintain complete compile-error register until Debug and Release builds pass | Phase 5B |
| #373 | Compile errors must be categorized consistently | Phase 5B |
| #384 | Actual compile closure requires Claude/Codex on stitched repo with real command outputs | Claude/Codex Phase 5B |

## Register Entries

### P5B-E001 - Marker Inventory Drift

Error ID: P5B-E001
Command: `rtk powershell -NoProfile -ExecutionPolicy Bypass -File tools/find-spec-derivations.ps1 -Root .`
Configuration: N/A
Project: N/A
File: `tools/find-spec-derivations.ps1`
Line: expected marker table
Column: N/A
Compiler error code: N/A
Message: Initial marker verification failed with mismatched SPEC-DERIVED buckets and `Grand total: 184 expected 177`.
Root cause category: Marker/verifier drift from stitched handoff deltas
Proposed fix: Reconcile marker comments and script expectations against current canonical derived files.
Status: Resolved
Resolution reference: P5B-R001
Follow-up gap: None.

### P5B-E002 - .NET SDK Unavailable In Current Environment

Error ID: P5B-E002
Command: `rtk dotnet restore TaskTree.sln`; `rtk dotnet --info`
Configuration: N/A
Project: Solution
File: N/A
Line: N/A
Column: N/A
Compiler error code: N/A
Message: `rtk: Failed to resolve 'dotnet' via PATH, falling back to direct exec: Binary 'dotnet' not found on PATH`
Root cause category: Local environment/toolchain missing
Proposed fix: Re-run Phase 5B restore/build commands in an environment with the .NET 8 SDK installed and available on PATH, or install/provision the SDK with owner approval.
Status: Deferred
Resolution reference: None in this environment.
Follow-up gap: #384 remains open for actual Claude/Codex compile execution with real restore/build outputs.

### P5B-E003 - IAppLogger Error Contract Drift

Error ID: P5B-E003
Command: Static Phase 5B contract audit via `rtk git grep -n LogError`
Configuration: N/A
Project: Multiple production and test projects
File: `src/TaskTree.Modules.*`, `src/TaskTree.Orchestrator`, `src/TaskTree.UI`, `tests/TaskTree.Modules.*`
Line: Multiple call sites
Column: N/A
Compiler error code: N/A
Message: Several production call sites and test/null loggers still used the stale `LogError(string)` shape while `IAppLogger` declares `LogError(Exception? exception, string message, params object?[] args)`.
Root cause category: Interface contract drift
Proposed fix: Pass caught exceptions into `LogError` at call sites and update fake/null loggers to implement the current full `IAppLogger` interface.
Status: Resolved
Resolution reference: P5B-R002
Follow-up gap: None.

### P5B-E004 - Solution Missing Present Generated Projects

Error ID: P5B-E004
Command: Static Phase 5B project reconciliation via `dir /s /b *.csproj`, `type TaskTree.sln`, and `docs/stitching/expected-solution-projects.md`
Configuration: Debug / Release
Project: `TaskTree.sln`
File: `TaskTree.sln`
Line: Project inventory / ProjectConfigurationPlatforms
Column: N/A
Compiler error code: N/A
Message: The stitched repo contained generated Phase 2 module projects plus retained perf/test-support projects that were absent from `TaskTree.sln`.
Root cause category: Missing project
Proposed fix: Add present generated projects to `TaskTree.sln` with Debug/Release configuration rows.
Status: Resolved
Resolution reference: P5B-R003
Follow-up gap: None for the `.csproj` solution inventory. Packaging WAP remains Phase 5E-scoped and path-valid at `packaging/TaskTree.Installer.wapproj`.

### P5B-E005 - App Missing Generated Module References

Error ID: P5B-E005
Command: Static Phase 5B project-reference audit via `type src\TaskTree.App\TaskTree.App.csproj` and `type src\TaskTree.App\Bootstrap\ServiceRegistrations.cs`
Configuration: Debug / Release
Project: `TaskTree.App`
File: `src/TaskTree.App/TaskTree.App.csproj`
Line: ProjectReference item group
Column: N/A
Compiler error code: N/A
Message: `ServiceRegistrations` constructs `SettingsService`, `SessionLockService`, and `SnoozeService`, but `TaskTree.App.csproj` did not reference the corresponding generated module projects.
Root cause category: Missing project reference
Proposed fix: Add project references for `TaskTree.Modules.Settings`, `TaskTree.Modules.SessionLock`, and `TaskTree.Modules.Snooze`.
Status: Resolved
Resolution reference: P5B-R004
Follow-up gap: None.

### P5B-E006 - Test Projects Missing TestSupport Reference

Error ID: P5B-E006
Command: Static Phase 5B project-reference audit via `rtk git grep -n TaskTree.TestSupport tests/TaskTree.Modules.AutoUpdater.Tests` and `rtk git grep -n TaskTree.TestSupport tests/TaskTree.Modules.BugReporter.Tests`
Configuration: Debug / Release
Project: `TaskTree.Modules.AutoUpdater.Tests`; `TaskTree.Modules.BugReporter.Tests`
File: `tests/TaskTree.Modules.AutoUpdater.Tests/TaskTree.Modules.AutoUpdater.Tests.csproj`; `tests/TaskTree.Modules.BugReporter.Tests/TaskTree.Modules.BugReporter.Tests.csproj`
Line: ProjectReference item group
Column: N/A
Compiler error code: N/A
Message: Test source files in both projects import `TaskTree.TestSupport`, but the projects did not reference `tests/TaskTree.TestSupport/TaskTree.TestSupport.csproj`.
Root cause category: Test support fake mismatch / missing project reference
Proposed fix: Add `ProjectReference` entries to `TaskTree.TestSupport`.
Status: Resolved
Resolution reference: P5B-R005
Follow-up gap: None.

### P5B-E007 - Non-Portable Regex Control Characters

Error ID: P5B-E007
Command: Static source inspection of `src\TaskTree.UI\ViewModels\TaskBuilderViewModel.cs`
Configuration: Debug / Release
Project: `TaskTree.UI`
File: `src/TaskTree.UI/ViewModels/TaskBuilderViewModel.cs`
Line: `DateLike` regex declaration
Column: N/A
Compiler error code: N/A
Message: The date-like PHI heuristic regex contained rendered control characters where regex word-boundary escapes were intended.
Root cause category: Nullable/reference-type issue
Proposed fix: Replace the control characters with explicit regex `\b` word-boundary escapes.
Status: Resolved
Resolution reference: P5B-R006
Follow-up gap: None.

### P5B-E008 - Snooze Audit TargetId Type Drift

Error ID: P5B-E008
Command: Static Phase 5B model contract audit via `rtk git grep -n TargetId src tests`
Configuration: Debug / Release
Project: `TaskTree.Modules.Snooze`
File: `src/TaskTree.Modules.Snooze/SnoozeService.cs`
Line: `AuditAsync` helper
Column: N/A
Compiler error code: N/A
Message: `SnoozeService` assigned `AuditEntry.TargetId` from `taskId.ToString()` while the reconciled `AuditEntry.TargetId` property is `Guid`.
Root cause category: Enum/model mismatch
Proposed fix: Assign the `Guid` task id directly to `AuditEntry.TargetId`.
Status: Resolved
Resolution reference: P5B-R007
Follow-up gap: None.

### P5B-E009 - TaskTreePaths DI Registration Drift

Error ID: P5B-E009
Command: Static Phase 5B DI audit via `type src\TaskTree.Core\Models\TaskTreePaths.cs`, `type src\TaskTree.App\Bootstrap\ServiceRegistrations.cs`, and `type tests\TaskTree.Orchestrator.Tests\ServiceRegistrationsTests.cs`
Configuration: Debug / Release
Project: `TaskTree.App`
File: `src/TaskTree.App/Bootstrap/ServiceRegistrations.cs`
Line: `AddTaskTreeServices`
Column: N/A
Compiler error code: N/A
Message: `TaskTreePaths` documentation and ServiceRegistrations tests expect a singleton path model, but `AddTaskTreeServices` did not register or consume it; module factories used static `CompositionRoot` path helpers instead.
Root cause category: Constructor signature drift / DI registration mismatch
Proposed fix: Register `TaskTreePaths` with `TryAddSingleton` and make logger, key-manager, and secure-store factories use the injected paths while preserving default `%LOCALAPPDATA%\TaskTree` behavior.
Status: Resolved
Resolution reference: P5B-R008
Follow-up gap: None.

### P5B-E010 - Phase 5A TestSupport Migration Incomplete

Error ID: P5B-E010
Command: Static Phase 5A/5B migration audit via `rtk rg -n TaskTree.Core.Tests.TestDoubles tests src` and `rtk rg --files tests/TaskTree.Core.Tests/TestDoubles`
Configuration: Debug / Release
Project: `TaskTree.Modules.ReminderScheduler.Tests`; `TaskTree.Core.Tests`
File: `tests/TaskTree.Modules.ReminderScheduler.Tests/ReminderSchedulerTests.cs`; `tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs`; `tests/TaskTree.Core.Tests/TestDoubles/InMemorySecureStore.cs`
Line: Remaining old using plus obsolete duplicate files
Column: N/A
Compiler error code: N/A
Message: Phase 5A's TestSupport promotion manifest was not fully applied: one test still imported `TaskTree.Core.Tests.TestDoubles`, and obsolete old-location test double files remained after the promoted `TaskTree.TestSupport` project was present.
Root cause category: Test support fake mismatch / duplicate type/file collision
Proposed fix: Migrate the remaining test import to `TaskTree.TestSupport`, remove stale old-namespace comments that violate the manifest verification, and delete the superseded old-location test double files.
Status: Resolved
Resolution reference: P5B-R009
Follow-up gap: Build/test verification remains deferred to an environment with the .NET SDK available.

### P5B-E011 - Tier 2 Obsolete Window Deletion Build Gate Unmet

Error ID: P5B-E011
Command: Static Phase 5A deletion precondition audit via `rtk rg -n ReminderToast src/TaskTree.Orchestrator/ToastTier2Adapter.cs`, `rtk rg -n TaskTree.UI.csproj src/TaskTree.Orchestrator/TaskTree.Orchestrator.csproj`, and `rtk dotnet restore TaskTree.sln`
Configuration: Release
Project: `TaskTree.Orchestrator`
File: `docs/Phase5A-tier2-deletion-manifest.md`; `src/TaskTree.Orchestrator/Views/ToastTier2Window.xaml`; `src/TaskTree.Orchestrator/Views/ToastTier2Window.xaml.cs`
Line: N/A
Column: N/A
Compiler error code: N/A
Message: The Tier 2 deletion manifest's source-reference preconditions are satisfied, but its required build-before-delete gate cannot be executed locally because the .NET SDK is unavailable.
Root cause category: Local environment/toolchain missing
Proposed fix: Run `dotnet build TaskTree.sln -c Release` in an environment with the .NET SDK available, then delete the obsolete `ToastTier2Window` files only after that build precondition passes.
Status: Deferred
Resolution reference: None in this environment.
Follow-up gap: Obsolete Tier 2 files remain intentionally present until the Release build precondition is proven.

### P5B-E012 - Tier 2 Deletion Manifest Grep Drift

Error ID: P5B-E012
Command: Static manifest verification via `rtk rg -n ToastTier2Window src`
Configuration: N/A
Project: `TaskTree.UI`
File: `src/TaskTree.UI/Views/ReminderToast.xaml.cs`
Line: file header comment
Column: N/A
Compiler error code: N/A
Message: The Tier 2 deletion manifest expects pre-deletion `ToastTier2Window` hits to be limited to the two obsolete files, but the replacement `ReminderToast` header comment also named the obsolete type.
Root cause category: Namespace / file inclusion check drift
Proposed fix: Remove the obsolete type name from the replacement file's comment while preserving the Phase 2B/Gaps context.
Status: Resolved
Resolution reference: P5B-R010
Follow-up gap: Obsolete Tier 2 files remain deferred until Release build precondition passes.

### P5B-E013 - TreatWarningsAsErrors Unused Member Drift

Error ID: P5B-E013
Command: Static warning-as-error audit via `rtk rg -n TreatWarningsAsErrors src tests` and source inspection
Configuration: Debug / Release
Project: `TaskTree.Modules.ComplianceCore`; `TaskTree.Modules.Settings`
File: `src/TaskTree.Modules.ComplianceCore/ComplianceCore.cs`; `src/TaskTree.Modules.Settings/SettingsService.cs`
Line: private fields / `SaveAsync`
Column: N/A
Compiler error code: CS0169 / CS0414 / CS0219 risk
Message: `TaskTree.Modules.ComplianceCore` enables `TreatWarningsAsErrors` while storing constructor dependencies in private fields that are never read. `SettingsService.SaveAsync` also retained an unused local snapshot.
Root cause category: Nullable/reference-type issue
Proposed fix: Preserve constructor null validation without retaining unused private fields, and remove the unused `oldSettings` local.
Status: Resolved
Resolution reference: P5B-R011
Follow-up gap: Build verification remains deferred to an environment with the .NET SDK available.
