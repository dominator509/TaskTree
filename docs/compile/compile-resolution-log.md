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

### P5B-R012 - Shared TestSupport Consumer Reconciliation

Resolution ID: P5B-R012
Related compile error ID(s): P5B-E014
File(s) changed:
- `tests/TaskTree.TestSupport/InMemorySecureStore.cs`
- `tests/TaskTree.Modules.TaskEngine.Tests/TaskEngineTests.cs`
- `tests/TaskTree.Modules.ComplianceCore.Tests/ComplianceCoreTests.cs`
- `tests/TaskTree.Modules.ComplianceCore.Tests/AuditChainWriterTests.cs`
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
Change summary: Moved active TaskEngine and ComplianceCore test construction paths to the canonical `TaskTree.TestSupport` helpers and added an explicit by-reference mode to the shared in-memory store for migrated private-DTO/tamper tests.
Exact rationale: `docs/Phase5A-using-migration-manifest.md` requires the promoted TestSupport helpers to be used by the stitched test graph. TaskEngine and ComplianceCore tests still carried inline helpers. The shared store's default JSON-backed behavior is correct for most consumers, but the migrated TaskEngine and ComplianceCore test paths previously used by-reference storage for private DTOs and tamper inspection.
Why this is compile-closure only: The edit is test-support reconciliation only. It does not change production APIs, production storage, audit chain logic, redaction behavior, package references, or live integrations.
Why runtime behavior is unchanged or minimally changed: Default `InMemorySecureStore` behavior remains JSON-backed. Only migrated tests that explicitly request `preserveObjectReferences: true` get the old by-reference semantics from their former inline helpers.
Regression risk: Low to medium; helper behavior is shared across tests, so the default path was preserved and the special mode is opt-in.
Tests/build commands re-run:
- `rtk rg -n FakeClock tests`
- `rtk rg -n InMemorySecureStore tests`
- `rtk powershell -NoProfile -ExecutionPolicy Bypass -File tools/find-spec-derivations.ps1 -Root .`
- `rtk git diff --check`
Command output reference: Static checks should show active TaskEngine and AuditChainWriter construction using `TaskTree.TestSupport`; full Debug/Release compile proof remains unavailable locally because no .NET SDK is available on PATH.
Follow-up gap created/updated: `P5B-E002` remains the environment blocker for restore/build/test proof.
Reviewer/owner note: Re-run SDK-backed tests before final Phase 5B sign-off.

### P5B-R013 - Packaging WAP Generated Module Reference Reconciliation

Resolution ID: P5B-R013
Related compile error ID(s): P5B-E015
File(s) changed:
- `packaging/TaskTree.Installer.wapproj`
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
- `docs/compile/phase5b-compile-closure-plan.md`
- `docs/compile/project-reference-reconciliation.md`
Change summary: Added explicit WAP project references for the generated Settings, SessionLock, and Snooze modules.
Exact rationale: Phase 5B had already reconciled `TaskTree.App.csproj` and the solution to include these generated modules. Gap #319 in `docs/signing-checklist.md` requires the final `.wapproj` project references to be reconciled against stitched repo projects. The packaging scaffold still listed only the older module set.
Why this is compile-closure only: The edit only aligns the packaging project graph with existing projects already used by the app composition root. It does not change product source code, package signing, install behavior, certificates, or live packaging commands.
Why runtime behavior is unchanged or minimally changed: Runtime DI and app project references were already present; the packaging project now names the same module projects explicitly.
Regression risk: Low to medium; WAP tooling may still need Phase 5E normalization, but missing generated modules are no longer hidden in the packaging graph.
Tests/build commands re-run:
- `rtk rg -n ProjectReference packaging`
- `rtk powershell -NoProfile -ExecutionPolicy Bypass -File tools/find-spec-derivations.ps1 -Root .`
- `rtk git diff --check`
Command output reference: Static checks should show all generated module references present. Real WAP/MSIX validation remains unavailable locally without the .NET/MSIX toolchain.
Follow-up gap created/updated: Gap #319 is statically reconciled; Phase 5E gaps #318/#320 remain for Windows MSIX tooling/signing/install proof.
Reviewer/owner note: Re-run packaging validation with MSIX tooling in Phase 5E.

### P5B-R014 - DI Registration Test Coverage Alignment

Resolution ID: P5B-R014
Related compile error ID(s): P5B-E016
File(s) changed:
- `tests/TaskTree.Orchestrator.Tests/ServiceRegistrationsTests.cs`
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
- `docs/compile/phase5b-compile-closure-plan.md`
Change summary: Extended the DI registration test to assert Settings, SessionLock, and Snooze service registrations, and renamed the reminder delivery resolution test to remove stale placeholder wording.
Exact rationale: The app composition root now constructs `SettingsService`, `SessionLockService`, `SnoozeService`, the real `ReminderDeliveryService`, and an `Orchestrator` that depends on the newer interfaces. Phase 5B constructor/DI reconciliation should keep the container test aligned with the current graph.
Why this is compile-closure only: The edit changes only test assertions/naming to match the existing production DI graph. No production behavior, package references, or runtime code changed.
Why runtime behavior is unchanged or minimally changed: The app already registers these services. The test now checks the current registrations instead of the earlier subset.
Regression risk: Low; the test may expose missing DI registration regressions earlier, which is intended for Phase 5B.
Tests/build commands re-run:
- `rtk git grep -n ISettingsService tests/TaskTree.Orchestrator.Tests/ServiceRegistrationsTests.cs`
- `rtk git grep -n ISessionLockService tests/TaskTree.Orchestrator.Tests/ServiceRegistrationsTests.cs`
- `rtk git grep -n ISnoozeService tests/TaskTree.Orchestrator.Tests/ServiceRegistrationsTests.cs`
- `rtk git diff --check`
Command output reference: Static grep should show all three newer interfaces covered; real test execution remains unavailable without the .NET SDK.
Follow-up gap created/updated: `P5B-E002` remains the restore/build/test environment blocker.
Reviewer/owner note: Re-run `dotnet test` in an SDK environment before Phase 5B sign-off.

### P5B-R015 - Orchestrator Offline Test Backfill

Resolution ID: P5B-R015
Related compile error ID(s): P5B-E017
File(s) changed:
- `tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs`
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
- `docs/compile/phase5b-compile-closure-plan.md`
Change summary: Replaced the Phase 1H Gap #95 inconclusive orchestrator placeholder with offline tests covering constructor null guardrails, startup lifecycle calls, shutdown lifecycle calls, event unsubscription, and startup/shutdown audit entries.
Exact rationale: The current production `Orchestrator` constructor and dependency graph are stable enough for offline tests, and leaving the placeholder in place would keep a generated test artifact from exercising the reconciled constructor/DI surface.
Why this is compile-closure only: The edit changes only test code and compile audit docs. It does not change production orchestration behavior, service registrations, live Windows integrations, PHI/audit policy, or external adapters.
Why runtime behavior is unchanged or minimally changed: Production code is untouched. The tests observe existing calls using Moq and existing `TaskTree.TestSupport.FakeClock`.
Regression risk: Low to medium; strict test mocks may expose future lifecycle drift, which is intended for Phase 5B/5C verification.
Tests/build commands re-run:
- `rtk rg -n Placeholder tests\TaskTree.Orchestrator.Tests\OrchestratorTests.cs`
- `rtk rg -n Constructor_NullDependencies tests\TaskTree.Orchestrator.Tests\OrchestratorTests.cs`
- `rtk rg -n StartAsync_StartsDependencies tests\TaskTree.Orchestrator.Tests\OrchestratorTests.cs`
- `rtk rg -n StopAsync_AfterStart tests\TaskTree.Orchestrator.Tests\OrchestratorTests.cs`
- `rtk git diff --check`
Command output reference: Static grep should show the placeholder removed and the three offline tests present. Real `dotnet test` execution remains unavailable locally because no .NET SDK is available on PATH.
Follow-up gap created/updated: `P5B-E002` remains the restore/build/test environment blocker; broader Phase 1H E2E backfill continued in `P5B-R016`.
Reviewer/owner note: Re-run SDK-backed tests before Phase 5B/5C sign-off.

### P5B-R016 - EndToEnd Offline Provider Backfill

Resolution ID: P5B-R016
Related compile error ID(s): P5B-E018
File(s) changed:
- `tests/TaskTree.Orchestrator.Tests/EndToEndOfflineTests.cs`
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
- `docs/compile/phase5b-compile-closure-plan.md`
Change summary: Replaced the Phase 1H Gap #95 E2E placeholder with offline provider tests for graph resolution, task persistence with audit-chain integrity, and Orchestrator start/stop lifecycle through mocked live integration boundaries.
Exact rationale: `docs/spec-derivations/PHASE1H-DERIVATIONS.md` explicitly identifies `EndToEndOfflineTests.cs` as an 8-test skeleton for Phase 5C implementation. The current constructor graph supports a smaller verified offline provider seam without changing production code.
Why this is compile-closure only: The edit changes only test code and compile audit docs. It does not alter production DI registrations, runtime services, live tray/toast behavior, storage encryption, PHI redaction, or external adapters.
Why runtime behavior is unchanged or minimally changed: Production code is untouched. The test provider uses existing real module implementations where safe and Moq boundaries for live tray/session/settings/snooze integrations.
Regression risk: Medium; DI-style tests are sensitive to constructor churn, which is exactly the drift they are intended to expose during Phase 5B/5C.
Tests/build commands re-run:
- `rtk rg -n Placeholder tests\TaskTree.Orchestrator.Tests\EndToEndOfflineTests.cs`
- `rtk rg -n OfflineProvider tests\TaskTree.Orchestrator.Tests\EndToEndOfflineTests.cs`
- `rtk rg -n BuildOfflineProvider tests\TaskTree.Orchestrator.Tests\EndToEndOfflineTests.cs`
- `rtk git diff --check`
Command output reference: Static grep should show the placeholder removed and the offline provider helper/tests present. Real `dotnet test` execution remains unavailable locally because no .NET SDK is available on PATH.
Follow-up gap created/updated: `P5B-E002` remains the restore/build/test environment blocker; live integration proof remains Phase 5E-owned.
Reviewer/owner note: Re-run SDK-backed tests before Phase 5B/5C sign-off.

### P5B-R017 - ReminderDelivery Snooze Skip Test Backfill

Resolution ID: P5B-R017
Related compile error ID(s): P5B-E019
File(s) changed:
- `tests/TaskTree.Orchestrator.Tests/ReminderDeliveryServiceTests.cs`
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
- `docs/compile/phase5b-compile-closure-plan.md`
Change summary: Replaced the Phase 2G Gap #193 inconclusive skeleton with an offline event-driven test for the snoozed reminder skip path.
Exact rationale: `ReminderDeliveryService` now has a concrete snooze check before the tier cascade. The test can verify this behavior without live toast/tray APIs by raising `IReminderScheduler.ReminderDue`, returning an active `SnoozeState`, and observing the `DeliverySkippedSnoozed` audit entry.
Why this is compile-closure only: The edit changes only test code and compile audit docs. It does not change production reminder delivery behavior, tier adapters, snooze behavior, audit vocabulary, or live Windows integrations.
Why runtime behavior is unchanged or minimally changed: Production code is untouched. The test exercises existing behavior through mocks and concrete adapters that are bypassed by the snooze short-circuit.
Regression risk: Low to medium; async event-handler tests can be timing-sensitive, mitigated here with a `TaskCompletionSource` tied to the audit call rather than fixed sleep.
Tests/build commands re-run:
- `rtk rg -n Inconclusive tests\TaskTree.Orchestrator.Tests`
- `rtk rg -n SKELETON tests\TaskTree.Orchestrator.Tests`
- `rtk git diff --check`
Command output reference: Static grep should show no remaining inconclusive/skeleton tests under `tests\TaskTree.Orchestrator.Tests`. Real `dotnet test` execution remains unavailable locally because no .NET SDK is available on PATH.
Follow-up gap created/updated: `P5B-E002` remains the restore/build/test environment blocker; live tier delivery proof remains Phase 5E-owned.
Reviewer/owner note: Re-run SDK-backed tests before Phase 5B/5C sign-off.

### P5B-R018 - Offline Import Valid Bundle Test Backfill

Resolution ID: P5B-R018
Related compile error ID(s): P5B-E020
File(s) changed:
- `src/TaskTree.Modules.AutoUpdater/ManifestSigner.cs`
- `tests/TaskTree.Modules.AutoUpdater.Tests/OfflineImportServiceTests.cs`
- `tests/TaskTree.Modules.AutoUpdater.Tests/ManifestSignerTests.cs`
- `tests/TaskTree.Modules.AutoUpdater.Tests/Phase3IntegrationTests.cs`
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
- `docs/compile/phase5b-compile-closure-plan.md`
Change summary: Added a backward-compatible public-key override constructor to `ManifestSigner` and replaced the inconclusive positive offline-import test with a fixed Ed25519 vector test that verifies staged package bytes.
Exact rationale: The production embedded public key is intentionally an owner-owned placeholder, but tests need a safe key seam to prove the signature/hash/staging path. The new constructor keeps default production behavior unchanged while allowing tests to supply a deterministic Ed25519 public key for a fixed test vector.
Why this is compile-closure only: The change is a testability seam plus test backfill for an already-approved Ed25519 verification path. It does not add a production key, fake signature success, bypass hash verification, change staging behavior, or introduce any package dependency.
Why runtime behavior is unchanged or minimally changed: `new ManifestSigner()` still uses the existing embedded placeholder key. Only callers that explicitly pass a public key use the override.
Regression risk: Medium; the positive test depends on the manifest canonical JSON remaining stable and must be verified in an SDK environment.
Tests/build commands re-run:
- `rtk rg -n Inconclusive tests docs\compile`
- `rtk rg -n ImportAsync_ValidBundle tests\TaskTree.Modules.AutoUpdater.Tests\OfflineImportServiceTests.cs`
- `rtk git diff --check`
Command output reference: Static grep should show the updater inconclusive removed and the positive test present. Real `dotnet test` execution remains unavailable locally because no .NET SDK is available on PATH.
Follow-up gap created/updated: `P5B-E002` remains the restore/build/test environment blocker; real production signing key remains Phase 5F/release owner input.
Reviewer/owner note: Re-run SDK-backed AutoUpdater tests before Phase 5B/5C sign-off.

### P5B-R019 - TestSupport FakeClock Canonicalization

Resolution ID: P5B-R019
Related compile error ID(s): P5B-E021
File(s) changed:
- `tests/TaskTree.TestSupport/FakeClock.cs`
- `tests/TaskTree.TestSupport/Clocks/FakeClock.cs`
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
Change summary: Made the root `TaskTree.TestSupport.FakeClock` a compatibility alias over the promoted `TaskTree.TestSupport.Clocks.FakeClock` implementation, while preserving the settable `UtcNow` surface used by existing migrated tests and documenting the changed public constructors.
Exact rationale: Phase 1D documentation identifies the `Clocks` namespace helper as the promoted canonical fake, but the active migrated tests use the root namespace helper. Keeping two independent implementations would leave TestSupport behavior forked during compile closure.
Why this is compile-closure only: The edit only reconciles shared test-support helper implementation and XML documentation. Production code, public product APIs, package references, live integrations, and test assertions are unchanged.
Why runtime behavior is unchanged or minimally changed: Existing root-namespace consumers still construct `FakeClock` with the same constructors and can still set `UtcNow`; the underlying behavior now comes from the promoted helper.
Regression risk: Low to medium; the promoted helper rejects backwards time movement. Current static search shows the active setter use advances time forward.
Tests/build commands re-run:
- `rtk rg -n TaskTree.TestSupport.Clocks docs tests src`
- `rtk rg -n FakeClock tests\TaskTree.TestSupport tests\TaskTree.Modules.TaskEngine.Tests`
- `rtk git diff --check`
Command output reference: Static grep should show the promoted helper retained and the root helper forwarding to it. Real `dotnet test` execution remains unavailable locally because no .NET SDK is available on PATH.
Follow-up gap created/updated: `P5B-E002` remains the restore/build/test environment blocker.
Reviewer/owner note: Re-run SDK-backed tests before Phase 5B/5C sign-off, especially `TaskTree.Modules.TaskEngine.Tests`.

### P5B-R020 - Expected Solution Inventory Reconciliation

Resolution ID: P5B-R020
Related compile error ID(s): P5B-E022
File(s) changed:
- `docs/stitching/expected-solution-projects.md`
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
- `docs/compile/phase5b-compile-closure-plan.md`
- `docs/stitching/gap-carryforward-v1.0.46.md`
- `docs/stitching/bundle-application-log.md`
- `docs/HANDOFF-v1.0.48-delta.md`
Change summary: Updated the expected solution inventory to name present generated Settings, SessionLock, and Snooze projects; added present UI, Orchestrator, TrayHost, Settings, SessionLock, and Snooze test projects; and documented that ReminderDelivery is included through Orchestrator rather than a standalone project.
Exact rationale: Static `*.csproj` and `TaskTree.sln` inspection showed the solution inventory was already reconciled beyond the older Phase 5A scaffold, but `docs/stitching/expected-solution-projects.md` still omitted current expected projects and retained a stale possible standalone ReminderDelivery project.
Why this is compile-closure only: The edit changes only stitching/compile documentation so the expected inventory matches the actual solution graph. No product code, project files, package references, or runtime behavior changed.
Why runtime behavior is unchanged or minimally changed: Documentation-only change.
Regression risk: Low; risk is limited to documentation accuracy. Restore/build proof remains the authoritative final gate.
Tests/build commands re-run:
- `rtk rg --files -g *.csproj src tests`
- `type TaskTree.sln`
- `rtk git diff --check`
Command output reference: Static inspection shows all named source/test projects are present in `TaskTree.sln`; real restore/build proof remains unavailable locally because no .NET SDK is available on PATH.
Follow-up gap created/updated: `P5B-E002` remains the restore/build/test environment blocker.
Reviewer/owner note: Re-run restore, Debug build, and Release build with .NET SDK before Phase 5B sign-off.

### P5B-R021 - Compile Register Category Taxonomy Reconciliation

Resolution ID: P5B-R021
Related compile error ID(s): P5B-E023
File(s) changed:
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
- `docs/compile/phase5b-compile-closure-plan.md`
- `docs/stitching/gap-carryforward-v1.0.46.md`
- `docs/stitching/bundle-application-log.md`
- `docs/HANDOFF-v1.0.48-delta.md`
Change summary: Extended the compile register root-cause category table so it includes categories already used by current Phase 5B entries.
Exact rationale: Gap #373 requires consistent compile-error categorization. The register entries had evolved beyond the original category list as Phase 5B added marker, environment, project-inventory, and test-coverage findings.
Why this is compile-closure only: Documentation taxonomy only; no product code, project files, package references, or runtime behavior changed.
Why runtime behavior is unchanged or minimally changed: Documentation-only change.
Regression risk: Low; the change makes the register more internally consistent.
Tests/build commands re-run:
- `rtk rg -n "Root cause category:" docs\compile\compile-error-register.md`
- `rtk git diff --check`
Command output reference: Static inspection should show all current root-cause categories represented in the table. Real restore/build proof remains unavailable locally because no .NET SDK is available.
Follow-up gap created/updated: `P5B-E002` remains the restore/build/test environment blocker.
Reviewer/owner note: Re-run restore, Debug build, and Release build with .NET SDK before Phase 5B sign-off.

### P5B-R022 - MSTest Package Version Alignment

Resolution ID: P5B-R022
Related compile error ID(s): P5B-E024
File(s) changed:
- `tests/TaskTree.Core.Tests/TaskTree.Core.Tests.csproj`
- `tests/TaskTree.Modules.SecureStore.Tests/TaskTree.Modules.SecureStore.Tests.csproj`
- `tests/TaskTree.Modules.BugReporter.Tests/TaskTree.Modules.BugReporter.Tests.csproj`
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
- `docs/compile/phase5b-compile-closure-plan.md`
- `docs/stitching/gap-carryforward-v1.0.46.md`
- `docs/stitching/bundle-application-log.md`
- `docs/HANDOFF-v1.0.48-delta.md`
Change summary: Aligned the three remaining MSTest `3.6.0` test projects to the repo's current `3.6.1` test package baseline.
Exact rationale: Static package audit showed no missing project references and no solution omissions, but it did show a split MSTest package graph. Aligning patch-level MSTest versions reduces restore/test discovery drift without adding new dependencies.
Why this is compile-closure only: The edit changes test project package versions and compile documentation only; no production code or runtime behavior changed.
Why runtime behavior is unchanged or minimally changed: Test-package alignment only.
Regression risk: Low; `3.6.1` was already the dominant MSTest version in the stitched test graph.
Tests/build commands re-run:
- `rtk node C:\tmp\tasktree-static-check.js`
- `findstr /n /c:"MSTest.TestAdapter" /c:"MSTest.TestFramework" tests\TaskTree.Core.Tests\TaskTree.Core.Tests.csproj tests\TaskTree.Modules.SecureStore.Tests\TaskTree.Modules.SecureStore.Tests.csproj tests\TaskTree.Modules.BugReporter.Tests\TaskTree.Modules.BugReporter.Tests.csproj`
- `rtk git diff --check`
Command output reference: Static audit should show no mixed package versions after alignment. Real restore/build/test proof remains unavailable locally because no .NET SDK is available.
Follow-up gap created/updated: `P5B-E002` remains the restore/build/test environment blocker.
Reviewer/owner note: Re-run restore, Debug build, and Release build with .NET SDK before Phase 5B sign-off.

### P5B-R023 - Test Project Direct Core Reference Reconciliation

Resolution ID: P5B-R023
Related compile error ID(s): P5B-E025
File(s) changed:
- `tests/TaskTree.Modules.BugReporter.Tests/TaskTree.Modules.BugReporter.Tests.csproj`
- `tests/TaskTree.Modules.SecureStore.Tests/TaskTree.Modules.SecureStore.Tests.csproj`
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
- `docs/compile/phase5b-compile-closure-plan.md`
- `docs/stitching/gap-carryforward-v1.0.46.md`
- `docs/stitching/bundle-application-log.md`
- `docs/HANDOFF-v1.0.48-delta.md`
Change summary: Added explicit `TaskTree.Core` project references to test projects whose source files import Core abstractions, enums, models, or security namespaces directly.
Exact rationale: Static using/reference audit found direct `TaskTree.Core.*` imports in BugReporter and SecureStore tests while their project files depended on Core only through module project transitivity. Direct references make the stitched test graph explicit and reduce hidden compile/restore coupling.
Why this is compile-closure only: The edit changes test project references and compile documentation only; production runtime behavior is unchanged.
Why runtime behavior is unchanged or minimally changed: Test-project graph only.
Regression risk: Low; `TaskTree.Core` is already a dependency of the modules under test.
Tests/build commands re-run:
- `rtk node C:\tmp\tasktree-using-reference-check.js`
- `rtk node C:\tmp\tasktree-static-check.js`
- `rtk powershell -NoProfile -ExecutionPolicy Bypass -File C:\tmp\tasktree-xml-check.ps1`
- `rtk git diff --check`
Command output reference: Static audits should show no missing project references inferred from using directives, no missing `ProjectReference` targets, and no malformed XML. Real restore/build/test proof remains unavailable locally because no .NET SDK is available.
Follow-up gap created/updated: `P5B-E002` remains the restore/build/test environment blocker.
Reviewer/owner note: Re-run restore, Debug build, and Release build with .NET SDK before Phase 5B sign-off.
### P5B-R024 - Compile Register DI Category Reconciliation

Resolution ID: P5B-R024
Related compile error ID(s): P5B-E026
File(s) changed:
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
- `docs/compile/phase5b-compile-closure-plan.md`
- `docs/stitching/gap-carryforward-v1.0.46.md`
- `docs/stitching/bundle-application-log.md`
- `docs/HANDOFF-v1.0.48-delta.md`
Change summary: Added `DI registration mismatch` to the compile-register root-cause category table.
Exact rationale: Static category audit found the category was used by resolved Phase 5B entries but not declared in the canonical table, weakening Gap #373 category consistency.
Why this is compile-closure only: Documentation taxonomy only; no product code, project files, package references, or runtime behavior changed.
Why runtime behavior is unchanged or minimally changed: Documentation-only change.
Regression risk: Low; the change makes the register internally consistent.
Tests/build commands re-run:
- `rtk node C:\tmp\tasktree-category-check.js`
- `rtk node C:\tmp\tasktree-register-check.js`
- `rtk git diff --check`
Command output reference: Static audits should show no undeclared used categories and no register consistency problems. Real restore/build/test proof remains unavailable locally because no .NET SDK is available.
Follow-up gap created/updated: `P5B-E002` remains the restore/build/test environment blocker.
Reviewer/owner note: Re-run restore, Debug build, and Release build with .NET SDK before Phase 5B sign-off.

### P5B-R025 - AutoUpdater ImportLocalAsync Test Reconciliation

Resolution ID: P5B-R025
Related compile error ID(s): P5B-E027
File(s) changed:
- `tests/TaskTree.Modules.AutoUpdater.Tests/AutoUpdaterTests.cs`
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
- `docs/compile/phase5b-compile-closure-plan.md`
- `docs/stitching/gap-carryforward-v1.0.46.md`
- `docs/stitching/bundle-application-log.md`
- `docs/HANDOFF-v1.0.48-delta.md`
Change summary: Replaced the stale `ImportLocalAsync` deferred-stub test with a current missing-bundle assertion against the implemented offline-import path.
Exact rationale: Phase 3C derivations and current production code show `ImportLocalAsync` delegates to `OfflineImportService`; expecting `NotImplementedException` would fail once SDK-backed tests run.
Why this is compile-closure only: The edit changes a stale test expectation and compile documentation only; production updater behavior is unchanged.
Why runtime behavior is unchanged or minimally changed: Test-only change.
Regression risk: Low; the test now matches current production behavior while retaining `ApplyAsync` as the Phase 5E live MSIX stub.
Tests/build commands re-run:
- `rtk rg -n ImportLocalAsync src tests docs\spec-derivations docs\compile Roadmap.md Architecture.md`
- `rtk git diff --check`
Command output reference: Static inspection shows `ImportLocalAsync` implemented through `OfflineImportService`; real test execution remains unavailable locally because no .NET SDK is available.
Follow-up gap created/updated: `P5B-E002` remains the restore/build/test environment blocker.
Reviewer/owner note: Re-run SDK-backed AutoUpdater tests before Phase 5B/5C sign-off.
### P5B-R026 - Phase 5B Static Gap Status Reconciliation

Resolution ID: P5B-R026
Related compile error ID(s): P5B-E028
File(s) changed:
- `docs/compile/project-reference-reconciliation.md`
- `docs/compile/phase5b-compile-closure-plan.md`
- `docs/stitching/expected-solution-projects.md`
- `docs/stitching/gap-carryforward-v1.0.46.md`
- `docs/stitching/bundle-application-log.md`
- `docs/HANDOFF-v1.0.48-delta.md`
- `docs/compile/compile-error-register.md`
- `docs/compile/compile-resolution-log.md`
Change summary: Tightened Phase 5B gap status rows from generic partial status to static reconciliation status where current static audits prove the scoped reconciliation.
Exact rationale: The project/reference, namespace, package/import, XAML/code-behind, duplicate full-type, solution configuration, and register/category audits now pass; leaving those rows as merely partial understated the current static closure while the restore/build gate remains separately deferred.
Why this is compile-closure only: Documentation status alignment only; no product code, project files, package references, or runtime behavior changed.
Why runtime behavior is unchanged or minimally changed: Documentation-only change.
Regression risk: Low; the change preserves the SDK-backed build/test blocker and does not claim final Phase 5B acceptance.
Tests/build commands re-run:
- `rtk rg -n Partially docs\compile docs\stitching docs\HANDOFF-v1.0.48-delta.md REPO_BRIEF.md`
- `rtk node C:\tmp\tasktree-static-check.js`
- `rtk node C:\tmp\tasktree-using-reference-check.js`
- `rtk node C:\tmp\tasktree-package-import-check.js`
- `rtk node C:\tmp\tasktree-duplicate-type-check.js`
- `rtk node C:\tmp\tasktree-xaml-check.js`
- `rtk node C:\tmp\tasktree-sln-check.js`
- `rtk node C:\tmp\tasktree-register-check.js`
Command output reference: Static audits should remain green; real restore/build/test proof remains unavailable locally because no .NET SDK is available.
Follow-up gap created/updated: `P5B-E002` remains the restore/build/test environment blocker.
Reviewer/owner note: Re-run restore, Debug build, and Release build with .NET SDK before Phase 5B sign-off.

