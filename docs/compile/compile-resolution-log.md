# compile-resolution-log.md - Phase 5B Compile Resolution Log

> Every compile fix must be recorded here by Claude/Codex. This log is the audit trail proving Phase 5B stayed inside compile-closure scope.

## Resolution Entry Template

```text
Resolution ID:
Related compile error ID(s):
File(s) changed:
Change summary:
Exact rationale:
Why this is compile-closure only:
Why runtime behavior is unchanged or minimally changed:
Regression risk:
Tests/build commands re-run:
Command output reference:
Follow-up gap created/updated:
Reviewer/owner note:
```

## Allowed Compile-Closure Fix Types

- Add missing using directive.
- Correct namespace to match project/folder convention.
- Add missing project reference.
- Add approved package reference.
- Reconcile constructor arguments to latest generated constructor.
- Update TestSupport fake to match final interface.
- Resolve duplicate type by selecting latest approved source and logging superseded source.
- Align WPF target framework/project properties.
- Fix XAML/code-behind namespace or partial class mismatch.

## Not Allowed Without New Approval

- Redesigning module behavior.
- Replacing stubs with fake live success.
- Adding external libraries not approved by Architecture/Roadmap.
- Removing audit/redaction/security checks to make tests compile.
- Hardcoding secrets, tokens, cert passwords, private keys, production URLs, or PHI-like data.

## Deletion / Exclusion Rule

Any file deleted or excluded from compile must include:

```text
Path:
Reason:
Proof duplicate/superseded:
Selected replacement path:
Downstream impact:
Owner review needed: yes/no
```

## Phase 5B Gaps

| Gap | Description | Target |
|---|---|---|
| #372 | Every compile fix must be recorded with rationale, changed files, and regression risk | Phase 5B |
| #374 | Any new package reference must be source-approved or documented for owner review | Phase 5B / Owner |
| #365 | Maintain compile-error register and resolution log | Phase 5B |

## Resolution Entries

### P5B-R001 - Marker Inventory Reconciliation

Resolution ID: P5B-R001
Related compile error ID(s): P5B-E001
File(s) changed:
- `tools/find-spec-derivations.ps1`
- `src/TaskTree.Core/Abstractions/IOrchestrator.cs`
- `src/TaskTree.Core/Models/TaskNode.cs`
- `src/TaskTree.Core/Models/UpdateManifest.cs`
- `src/TaskTree.Core/Models/BugReport.cs`
- `src/TaskTree.Core/Models/AuditEntry.cs`
- `src/TaskTree.Core/Models/ReminderEvent.cs`
- `src/TaskTree.Core/Enums/Priority.cs`
- `src/TaskTree.Core/Enums/UpdateChannel.cs`
- `src/TaskTree.Core/Enums/BugSeverity.cs`
- `src/TaskTree.App/Bootstrap/ServiceRegistrations.cs`
- `tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs`
Change summary: Repaired stitched-repo SPEC-DERIVED marker drift so the Phase 5B marker gate reflects canonical derived files and no longer counts explanatory marker-like text in verbatim files.
Exact rationale: `tools/find-spec-derivations.ps1` failed with stale/misleading counts (`Grand total: 184 expected 177`). Inspection showed missing Phase 0 markers on canonical derived files, marker-like explanatory text in verbatim enum/model files, stale duplicate Phase 1D test-double marker text, and a stale PHASE1F count after later handoff deltas.
Why this is compile-closure only: The edits only affect comments and the verification script's expected marker inventory; no runtime code paths, public APIs, project references, package references, or tests were changed.
Why runtime behavior is unchanged or minimally changed: All changed C# files received comment-only edits.
Regression risk: Low; risk is limited to marker accounting. Mitigated by rerunning the marker script.
Tests/build commands re-run:
- `rtk powershell -NoProfile -ExecutionPolicy Bypass -File tools/find-spec-derivations.ps1 -Root .`
Command output reference: Marker script passed with all buckets OK and `Grand total: 179 expected 179`.
Follow-up gap created/updated: None.
Reviewer/owner note: Debug/Release compile remains unverified in this environment because no .NET SDK is available on PATH.

### P5B-R003 - Solution Project Inventory Reconciliation

Resolution ID: P5B-R003
Related compile error ID(s): P5B-E004
File(s) changed:
- `TaskTree.sln`
Change summary: Added present generated Phase 2 module projects and retained generated test/perf/support projects to `TaskTree.sln` with Debug/Release build configuration rows.
Exact rationale: `docs/stitching/expected-solution-projects.md` requires Phase 5B to verify the solution contains expected projects, generated test projects, and `TaskTree.Perf.Tests` if retained. The repo contained `.csproj` files for Settings, SessionLock, Snooze, their test projects, `TaskTree.Perf.Tests`, and `TaskTree.TestSupport`, but `TaskTree.sln` did not include them.
Why this is compile-closure only: The edit only reconciles the solution inventory so existing projects are restored/build/test participants; no source code, package reference, project reference, or runtime behavior changed.
Why runtime behavior is unchanged or minimally changed: Application runtime code is untouched. Build graph coverage is broadened to include already-present projects.
Regression risk: Medium; including previously omitted projects may surface compile errors that were hidden by the incomplete solution. That is intentional for Phase 5B compile closure.
Tests/build commands re-run:
- `rtk git grep -n TaskTree.Modules.SessionLock TaskTree.sln`
- `rtk git grep -n TaskTree.Modules.Settings TaskTree.sln`
- `rtk git grep -n TaskTree.Modules.Snooze TaskTree.sln`
- `rtk git grep -n TaskTree.Perf.Tests TaskTree.sln`
- `rtk git grep -n TaskTree.TestSupport TaskTree.sln`
Command output reference: Grep confirmed all added project entries are present in `TaskTree.sln`; Debug/Release build remains unverified locally because no .NET SDK is available on PATH.
Follow-up gap created/updated: None for `.csproj` solution inventory. Packaging WAP remains Phase 5E-scoped and path-valid at `packaging/TaskTree.Installer.wapproj`.
Reviewer/owner note: A real `dotnet restore/build` run may expose additional hidden compile errors now that omitted projects are included.

### P5B-R004 - App Generated Module Project References

Resolution ID: P5B-R004
Related compile error ID(s): P5B-E005
File(s) changed:
- `src/TaskTree.App/TaskTree.App.csproj`
Change summary: Added `ProjectReference` entries from `TaskTree.App` to the generated Settings, SessionLock, and Snooze modules used by the DI composition root.
Exact rationale: `src/TaskTree.App/Bootstrap/ServiceRegistrations.cs` directly constructs `SettingsService`, `SessionLockService`, and `SnoozeService`. Without project references, `TaskTree.App` cannot resolve those concrete implementation namespaces during compile.
Why this is compile-closure only: The edit only connects already-present generated module projects into the compile graph for the app project.
Why runtime behavior is unchanged or minimally changed: DI registrations already intended these services to exist; this change makes the project graph match that existing source code.
Regression risk: Low; it may reveal compile errors in the referenced modules, which is desired during Phase 5B.
Tests/build commands re-run:
- `type src\TaskTree.App\TaskTree.App.csproj`
- `type src\TaskTree.App\Bootstrap\ServiceRegistrations.cs`
Command output reference: Static inspection confirmed the app project now references the generated modules it constructs. Debug/Release build remains unverified locally because no .NET SDK is available on PATH.
Follow-up gap created/updated: None.
Reviewer/owner note: Re-run restore/build in an environment with .NET 8 SDK to verify transitive compile closure.

### P5B-R005 - TestSupport Project References For Generated Tests

Resolution ID: P5B-R005
Related compile error ID(s): P5B-E006
File(s) changed:
- `tests/TaskTree.Modules.AutoUpdater.Tests/TaskTree.Modules.AutoUpdater.Tests.csproj`
- `tests/TaskTree.Modules.BugReporter.Tests/TaskTree.Modules.BugReporter.Tests.csproj`
Change summary: Added `ProjectReference` entries to `tests/TaskTree.TestSupport/TaskTree.TestSupport.csproj` for test projects whose source imports `TaskTree.TestSupport`.
Exact rationale: Static grep showed AutoUpdater and BugReporter test sources using shared `FakeClock`/`InMemorySecureStore` helpers from `TaskTree.TestSupport`, while their project files lacked the shared test-support reference.
Why this is compile-closure only: The edit only makes already-written test sources resolvable by their owning test projects.
Why runtime behavior is unchanged or minimally changed: Production code is untouched; test graph inclusion now matches existing test imports.
Regression risk: Low; it may expose additional compile errors in shared test helpers, which is desired during Phase 5B/5C.
Tests/build commands re-run:
- `rtk git grep -n TaskTree.TestSupport tests/TaskTree.Modules.AutoUpdater.Tests`
- `rtk git grep -n TaskTree.TestSupport tests/TaskTree.Modules.BugReporter.Tests`
Command output reference: Grep confirmed both test projects contain sources importing `TaskTree.TestSupport`; project files now reference the shared support project. Debug/Release build remains unverified locally because no .NET SDK is available on PATH.
Follow-up gap created/updated: None.
Reviewer/owner note: Re-run restore/build/test in an environment with .NET 8 SDK to verify full graph closure.

### P5B-R006 - TaskBuilder Regex Source Sanitation

Resolution ID: P5B-R006
Related compile error ID(s): P5B-E007
File(s) changed:
- `src/TaskTree.UI/ViewModels/TaskBuilderViewModel.cs`
Change summary: Replaced non-portable rendered control characters in the `DateLike` regex literal with explicit `\b` word-boundary escapes.
Exact rationale: Static source inspection showed the regex intended to reject date-like PHI patterns, but the source contained control-character glyphs around the pattern. Using explicit ASCII regex escapes preserves the intended boundary semantics and avoids parser/encoding fragility.
Why this is compile-closure only: The edit only normalizes a malformed source literal to the intended regex syntax.
Why runtime behavior is unchanged or minimally changed: The date-like PHI heuristic remains the same intended word-boundary date pattern.
Regression risk: Low; affected path is validation-only and covered by existing TaskBuilder PHI-like rejection tests for adjacent patterns.
Tests/build commands re-run:
- `rtk git diff --check`
Command output reference: Whitespace/source diff validation passed. Debug/Release build remains unverified locally because no .NET SDK is available on PATH.
Follow-up gap created/updated: None.
Reviewer/owner note: Re-run restore/build/test in an environment with .NET 8 SDK to verify full compile closure.

### P5B-R008 - TaskTreePaths DI Registration Alignment

Resolution ID: P5B-R008
Related compile error ID(s): P5B-E009
File(s) changed:
- `src/TaskTree.App/Bootstrap/ServiceRegistrations.cs`
Change summary: Registered `TaskTreePaths` via `TryAddSingleton` and updated logger, master-key, and secure-store factories to use the injected path model.
Exact rationale: `TaskTreePaths` states it is registered as a singleton, and `ServiceRegistrationsTests` asserts that `AddTaskTreeServices` registers it. The composition root was still using static path helpers directly, which bypassed test overrides and contradicted the documented DI graph.
Why this is compile-closure only: The edit reconciles DI registration/factory construction with existing project contracts and tests. It does not add new services, external dependencies, or live behavior.
Why runtime behavior is unchanged or minimally changed: The default `TaskTreePaths` constructor resolves the same `%LOCALAPPDATA%\TaskTree` root used by the previous static helpers; tests can now supply an isolated root as intended.
Regression risk: Low to medium; service construction now depends on `TaskTreePaths`, but it is registered by default and can be overridden before `AddTaskTreeServices`.
Tests/build commands re-run:
- `rtk git grep -n TaskTreePaths src tests`
Command output reference: Static inspection confirmed `TaskTreePaths` is now registered/consumed by `ServiceRegistrations`; Debug/Release build remains unverified locally because no .NET SDK is available on PATH.
Follow-up gap created/updated: None.
Reviewer/owner note: Re-run restore/build/test in an environment with .NET 8 SDK to verify full DI graph closure.

### P5B-R007 - Snooze Audit TargetId Model Alignment

Resolution ID: P5B-R007
Related compile error ID(s): P5B-E008
File(s) changed:
- `src/TaskTree.Modules.Snooze/SnoozeService.cs`
Change summary: Changed `AuditEntry.TargetId` assignment in `SnoozeService` from a stringified task id to the `Guid` task id.
Exact rationale: Phase 5B model reconciliation selected `AuditEntry.TargetId` as `Guid`. `SnoozeService` still used `taskId.ToString()`, which would not compile against the current model.
Why this is compile-closure only: The edit aligns the assignment type with the existing Core model without changing snooze lifecycle, audit action names, storage, or event behavior.
Why runtime behavior is unchanged or minimally changed: The same task identity is audited, now using the model's typed `Guid` field.
Regression risk: Low; adjacent audit call sites already assign `Guid` values to `TargetId`.
Tests/build commands re-run:
- `rtk git grep -n TargetId src tests`
Command output reference: Follow-up grep showed production `AuditEntry.TargetId` assignments now use `Guid` values. Debug/Release build remains unverified locally because no .NET SDK is available on PATH.
Follow-up gap created/updated: None.
Reviewer/owner note: Re-run restore/build/test in an environment with .NET 8 SDK to verify full compile closure.

### P5B-R002 - IAppLogger Error Contract Alignment

Resolution ID: P5B-R002
Related compile error ID(s): P5B-E003
File(s) changed:
- `src/TaskTree.Modules.AutoUpdater/AutoUpdater.cs`
- `src/TaskTree.Modules.AutoUpdater/OfflineImportService.cs`
- `src/TaskTree.Modules.BugReporter/BugReporter.cs`
- `src/TaskTree.Modules.ReminderScheduler/ReminderScheduler.cs`
- `src/TaskTree.Modules.Settings/SettingsService.cs`
- `src/TaskTree.Modules.TrayHost/HotkeyManager.cs`
- `src/TaskTree.Orchestrator/Orchestrator.cs`
- `src/TaskTree.Orchestrator/ToastTier2Adapter.cs`
- `src/TaskTree.UI/ViewModels/MainWindowViewModel.cs`
- `src/TaskTree.UI/ViewModels/SettingsViewModel.cs`
- `src/TaskTree.UI/ViewModels/TaskBuilderViewModel.cs`
- `tests/TaskTree.Modules.AutoUpdater.Tests/Phase3IntegrationTests.cs`
- `tests/TaskTree.Modules.BugReporter.Tests/Phase3IntegrationTests.cs`
Change summary: Updated stale `LogError(string)` production call sites and test/null loggers to the current `IAppLogger.LogError(Exception? exception, string message, params object?[] args)` contract.
Exact rationale: `docs/HANDOFF.v1.0.20-delta.md` tracks G1F-H3 for `IAppLogger.LogError` shape verification. The current interface has an exception-bearing signature, but several stitched-repo call sites and nested fake loggers still used the older string-only shape.
Why this is compile-closure only: The edit only reconciles method signatures with the existing interface. It does not change control flow, exception handling outcomes, live integrations, audit policy, redaction policy, or user-visible status messages.
Why runtime behavior is unchanged or minimally changed: Existing log messages retain the same semantic content; caught exceptions are now passed through the logger's intended exception parameter.
Regression risk: Low; affected paths already catch exceptions and either continue, return a failure value, or preserve existing status text. Test fakes remain no-op.
Tests/build commands re-run:
- `rtk git grep -n LogError`
Command output reference: Follow-up grep showed production and test call sites using the current exception-bearing `LogError` shape; only docs mention the old shape as historical handoff/spec context.
Follow-up gap created/updated: None.
Reviewer/owner note: Debug/Release compile remains unverified in this environment because no .NET SDK is available on PATH.

### P5B-R009 - Phase 5A TestSupport Migration Completion

Resolution ID: P5B-R009
Related compile error ID(s): P5B-E010
File(s) changed:
- `tests/TaskTree.Modules.ReminderScheduler.Tests/ReminderSchedulerTests.cs`
- `tests/TaskTree.Modules.ReminderScheduler.Tests/TaskTree.Modules.ReminderScheduler.Tests.csproj`
- `tests/TaskTree.Modules.ComplianceCore.Tests/TaskTree.Modules.ComplianceCore.Tests.csproj`
- `tests/TaskTree.Modules.TaskEngine.Tests/TaskTree.Modules.TaskEngine.Tests.csproj`
- `tests/TaskTree.Modules.TrayHost.Tests/TaskTree.Modules.TrayHost.Tests.csproj`
- `tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs` (deleted)
- `tests/TaskTree.Core.Tests/TestDoubles/InMemorySecureStore.cs` (deleted)
- `docs/Phase5A-using-migration-manifest.md`
Change summary: Completed the Phase 5A shared TestSupport migration by moving the remaining ReminderScheduler test import to `TaskTree.TestSupport`, clearing stale old-namespace manifest comments, and deleting superseded old-location test double files.
Exact rationale: `docs/Phase5A-using-migration-manifest.md` explicitly requires old `TaskTree.Core.Tests.TestDoubles` consumers to migrate to `TaskTree.TestSupport` and old duplicate files to be deleted. Static audit showed the promoted `TaskTree.TestSupport` files and project references already existed, but one old namespace import and the old files remained.
Why this is compile-closure only: The edit reconciles test project namespace/file inclusion with the existing promoted support project. It does not change production behavior, test assertions, package references, or runtime services.
Why runtime behavior is unchanged or minimally changed: Production code is untouched; test code now uses the canonical promoted helpers with the same public helper surface.
Regression risk: Low; the migrated test project already referenced `tests/TaskTree.TestSupport/TaskTree.TestSupport.csproj`.
Tests/build commands re-run:
- `rtk rg -n TaskTree.Core.Tests.TestDoubles tests src`
- `rtk rg --files tests/TaskTree.Core.Tests/TestDoubles`
- `rtk git diff --check`
Command output reference: Static verification should show no live old namespace imports or old duplicate files; Debug/Release build remains unverified locally because no .NET SDK is available on PATH.
Follow-up gap created/updated: `P5B-E002` remains the environment blocker for restore/build/test proof.
Reviewer/owner note: Deletion rationale: old files were superseded by `tests/TaskTree.TestSupport/FakeClock.cs` and `tests/TaskTree.TestSupport/InMemorySecureStore.cs`; downstream impact is expected to be limited to removing duplicate stale namespace surface.

### P5B-R010 - Tier 2 Deletion Grep Precondition Cleanup

Resolution ID: P5B-R010
Related compile error ID(s): P5B-E012
File(s) changed:
- `src/TaskTree.UI/Views/ReminderToast.xaml.cs`
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
Change summary: Removed the obsolete `ToastTier2Window` type name from the `ReminderToast` replacement header comment.
Exact rationale: `docs/Phase5A-tier2-deletion-manifest.md` uses a grep precondition requiring `ToastTier2Window` source hits to identify only the obsolete files slated for deletion. A replacement-file comment produced a false positive.
Why this is compile-closure only: The edit is comment-only and keeps the manifest verification command meaningful.
Why runtime behavior is unchanged or minimally changed: No executable code, XAML, project references, resources, or test assertions changed.
Regression risk: Low; the affected file remains marked with the same Phase 2B derivation marker.
Tests/build commands re-run:
- `rtk rg -n ToastTier2Window src`
- `rtk powershell -NoProfile -ExecutionPolicy Bypass -File tools/find-spec-derivations.ps1 -Root .`
- `rtk git diff --check`
Command output reference: Static grep should now show only the two obsolete `ToastTier2Window` files under `src/TaskTree.Orchestrator/Views`; Debug/Release build remains unverified locally because no .NET SDK is available on PATH.
Follow-up gap created/updated: `P5B-E011` remains deferred for the build-before-delete gate.
Reviewer/owner note: The actual obsolete file deletion is still intentionally unapplied until Release build succeeds.

### P5B-R011 - Warning-As-Error Unused Member Cleanup

Resolution ID: P5B-R011
Related compile error ID(s): P5B-E013
File(s) changed:
- `src/TaskTree.Modules.ComplianceCore/ComplianceCore.cs`
- `src/TaskTree.Modules.Settings/SettingsService.cs`
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
Change summary: Removed unused stored constructor dependency fields from `ComplianceCore` while preserving null validation, and removed an unused `oldSettings` local from `SettingsService.SaveAsync`.
Exact rationale: `TaskTree.Modules.ComplianceCore.csproj` treats warnings as errors. Private fields that are assigned but never read can block compile under warning-as-error settings. The Settings local was also dead code from a previous drift pass.
Why this is compile-closure only: The edit removes unused state and a dead local without changing public APIs, DI constructor shape, audit behavior, redaction behavior, settings persistence, or event behavior.
Why runtime behavior is unchanged or minimally changed: Constructor null checks still run for all dependencies; `ComplianceCore` continues delegating to `PhiRedactor` and `AuditChainWriter`; `SettingsService.SaveAsync` never used the removed snapshot.
Regression risk: Low; removed values had no read sites.
Tests/build commands re-run:
- `rtk rg -n TreatWarningsAsErrors src tests`
- `rtk powershell -NoProfile -ExecutionPolicy Bypass -File tools/find-spec-derivations.ps1 -Root .`
- `rtk git diff --check`
Command output reference: Static checks passed; Debug/Release build remains unverified locally because no .NET SDK is available on PATH.
Follow-up gap created/updated: `P5B-E002` remains the environment blocker for restore/build/test proof.
Reviewer/owner note: Re-run restore/build in a .NET SDK environment to confirm no additional warning-as-error issues remain.
