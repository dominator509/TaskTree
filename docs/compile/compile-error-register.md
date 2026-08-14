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
- Package version drift
- Missing project reference
- Missing using / namespace mismatch
- Constructor signature drift
- Interface contract drift
- Enum/model mismatch
- Test support fake mismatch
- Test coverage drift
- DI registration mismatch
- Marker/verifier drift from stitched handoff deltas
- Local environment/toolchain missing
- Project inventory drift
- XAML/code-behind mismatch
- Windows-only API / target framework mismatch
- Packaging/project path mismatch
- Namespace / file inclusion check drift
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
Command: `rtk dotnet restore TaskTree.sln`; `where dotnet`; standard install-path probes for `C:\Program Files\dotnet\dotnet.exe` and `C:\Program Files (x86)\dotnet\dotnet.exe`
Configuration: N/A
Project: Solution
File: N/A
Line: N/A
Column: N/A
Compiler error code: N/A
Message: `rtk: Failed to resolve 'dotnet' via PATH, falling back to direct exec: Binary 'dotnet' not found on PATH`; `where dotnet` finds no match; neither standard Program Files `dotnet.exe` path exists in this environment.
Root cause category: Local environment/toolchain missing
Proposed fix: Re-run Phase 5B restore/build commands in an environment with the .NET 8 SDK installed and available on PATH, or install/provision the SDK with owner approval.
Status: Resolved
Resolution reference: P5B-R027
Follow-up gap: None for Phase 5B restore/build proof; live/package validation remains Phase 5E/5F-owned.

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

### P5B-E011 - Tier 2 Obsolete Window Deletion Build Gate

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
Status: Resolved
Resolution reference: P5B-R027
Follow-up gap: None for the obsolete Tier 2 file pair; live toast behavior remains Phase 5E-owned.

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

### P5B-E014 - Shared TestSupport Migration Drift

Error ID: P5B-E014
Command: Static Phase 5A/5B migration audit via `rtk rg -n FakeClock tests src`, `rtk rg -n InMemorySecureStore tests src`, and `docs/Phase5A-using-migration-manifest.md`
Configuration: Debug / Release
Project: `TaskTree.Modules.TaskEngine.Tests`; `TaskTree.Modules.ComplianceCore.Tests`; `TaskTree.TestSupport`
File: `tests/TaskTree.Modules.TaskEngine.Tests/TaskEngineTests.cs`; `tests/TaskTree.Modules.ComplianceCore.Tests/ComplianceCoreTests.cs`; `tests/TaskTree.Modules.ComplianceCore.Tests/AuditChainWriterTests.cs`; `tests/TaskTree.TestSupport/InMemorySecureStore.cs`
Line: Inline/private test double declarations and shared helper surface
Column: N/A
Compiler error code: Duplicate type/file collision risk / test support fake mismatch
Message: Phase 5A's shared TestSupport migration still had live inline `FakeClock`/`InMemorySecureStore` consumers in TaskEngine and ComplianceCore test files after the promoted `TaskTree.TestSupport` project was present. The migrated TaskEngine and ComplianceCore tests required the old by-reference storage semantics for private DTOs and tamper inspection, while the shared JSON-backed helper did not expose that mode.
Root cause category: Test support fake mismatch / duplicate type/file collision
Proposed fix: Migrate active TaskEngine and ComplianceCore test construction paths to `TaskTree.TestSupport`; extend `InMemorySecureStore` with an explicit object-reference preservation mode for migrated private-DTO/tamper tests while preserving JSON round-trip behavior by default; remove obsolete inline doubles without changing test behavior.
Status: Resolved
Resolution reference: P5B-R012
Follow-up gap: Build/test verification remains deferred to an environment with the .NET SDK available.

### P5B-E015 - Packaging WAP Generated Module Reference Drift

Error ID: P5B-E015
Command: Static packaging project-reference audit via `rtk rg -n ProjectReference packaging src tests`, `rtk rg -n Settings docs packaging src\TaskTree.App`, `rtk rg -n SessionLock docs packaging src\TaskTree.App`, and `rtk rg -n Snooze docs packaging src\TaskTree.App`
Configuration: Release / Packaging
Project: `TaskTree.Installer`
File: `packaging/TaskTree.Installer.wapproj`
Line: ProjectReference item group
Column: N/A
Compiler error code: Packaging/project path mismatch
Message: `TaskTree.App.csproj` and `ServiceRegistrations` now include generated Settings, SessionLock, and Snooze modules, and Gap #319 requires final `.wapproj` project references to be reconciled against stitched repo projects. The packaging project still listed the earlier module set and omitted these generated modules.
Root cause category: Packaging/project path mismatch
Proposed fix: Add explicit `ProjectReference` entries for `TaskTree.Modules.Settings`, `TaskTree.Modules.SessionLock`, and `TaskTree.Modules.Snooze` to `packaging/TaskTree.Installer.wapproj` without changing signing or live packaging behavior.
Status: Resolved
Resolution reference: P5B-R013
Follow-up gap: Phase 5E still owns MSIX/WAP tooling validation, signing, install, and package execution proof.

### P5B-E016 - DI Registration Test Coverage Drift

Error ID: P5B-E016
Command: Static DI/test audit via `rtk git grep -n Orchestrator -- tests src`, `rtk git grep -n ReminderDeliveryService -- tests src`, and source inspection
Configuration: Debug / Release
Project: `TaskTree.Orchestrator.Tests`
File: `tests/TaskTree.Orchestrator.Tests/ServiceRegistrationsTests.cs`
Line: `AddTaskTreeServices_RegistersAllRequiredInterfaces`; `BuildServiceProvider_ResolvesIReminderDeliveryService_*`
Column: N/A
Compiler error code: N/A
Message: The DI registration test still validated the earlier Phase 1F/1G service set and named reminder delivery as a placeholder even after Settings, SessionLock, Snooze, and the real `ReminderDeliveryService` became constructor dependencies in the app graph.
Root cause category: Constructor signature drift / DI registration mismatch
Proposed fix: Assert that `ISettingsService`, `ISessionLockService`, and `ISnoozeService` are registered, and rename the reminder-delivery provider test away from the stale placeholder wording.
Status: Resolved
Resolution reference: P5B-R014
Follow-up gap: Build/test execution remains deferred until a .NET SDK is available.

### P5B-E017 - Orchestrator Placeholder Test Drift

Error ID: P5B-E017
Command: Static Phase 1H/5B test audit via `rtk rg -n Placeholder tests\TaskTree.Orchestrator.Tests` and source inspection
Configuration: Debug / Release
Project: `TaskTree.Orchestrator.Tests`
File: `tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs`
Line: `Placeholder_PendingCodexBackfill`
Column: N/A
Compiler error code: N/A
Message: `OrchestratorTests` still contained the Phase 1H Gap #95 inconclusive placeholder after the production `Orchestrator` constructor and lifecycle dependencies had stabilized to the current TaskEngine, ReminderScheduler, ComplianceCore, TrayHost, ReminderDelivery, Settings, SessionLock, logger, and clock graph.
Root cause category: Constructor signature drift / test coverage drift
Proposed fix: Replace the inconclusive placeholder with offline tests for constructor null guardrails, startup dependency calls, shutdown dependency calls, event unsubscription, and startup/shutdown audit entries using existing Moq and `TaskTree.TestSupport` helpers.
Status: Resolved
Resolution reference: P5B-R015
Follow-up gap: Test execution remains deferred until a .NET SDK is available; broader Phase 1H end-to-end placeholder coverage remains separate in `EndToEndOfflineTests.cs`.

### P5B-E018 - EndToEnd Offline Placeholder Test Drift

Error ID: P5B-E018
Command: Static Phase 1H/5B E2E audit via `rtk rg -n Placeholder tests\TaskTree.Orchestrator.Tests` and `docs/spec-derivations/PHASE1H-DERIVATIONS.md`
Configuration: Debug / Release
Project: `TaskTree.Orchestrator.Tests`
File: `tests/TaskTree.Orchestrator.Tests/EndToEndOfflineTests.cs`
Line: `Placeholder_PendingCodexBackfill`
Column: N/A
Compiler error code: N/A
Message: `EndToEndOfflineTests` still contained the Phase 1H Gap #95 inconclusive placeholder even though the current offline constructor graph can be built with real in-memory Core, ComplianceCore, TaskEngine, ReminderScheduler, ReminderDeliveryService, and mocked live integration boundaries.
Root cause category: Test coverage drift / constructor signature drift
Proposed fix: Replace the placeholder with offline provider tests that resolve the core runtime graph, verify TaskEngine persistence plus audit-chain integrity, and start/stop the Orchestrator through the offline graph without invoking live tray/toast/provider behavior.
Status: Resolved
Resolution reference: P5B-R016
Follow-up gap: Test execution remains deferred until a .NET SDK is available; live tray/toast/MSIX/provider paths remain Phase 5E-owned.

### P5B-E019 - ReminderDelivery Snooze Skeleton Drift

Error ID: P5B-E019
Command: Static orchestrator-test audit via `rtk rg -n Inconclusive tests\TaskTree.Orchestrator.Tests` and `rtk rg -n SKELETON tests\TaskTree.Orchestrator.Tests`
Configuration: Debug / Release
Project: `TaskTree.Orchestrator.Tests`
File: `tests/TaskTree.Orchestrator.Tests/ReminderDeliveryServiceTests.cs`
Line: `OnReminderDue_WhenSnoozed_SkipsTierCascade_AuditsDeliverySkippedSnoozed_SKELETON`
Column: N/A
Compiler error code: N/A
Message: The Phase 2G Gap #193 reminder delivery test remained an inconclusive skeleton even though the current `ReminderDeliveryService` constructor and snooze skip behavior are concrete enough for an offline event-driven test.
Root cause category: Test coverage drift / interface contract drift
Proposed fix: Replace the inconclusive skeleton with a real offline test that starts `ReminderDeliveryService`, raises `IReminderScheduler.ReminderDue`, returns an active snooze from `ISnoozeService`, and verifies `DeliverySkippedSnoozed` audit output without invoking live toast/tray behavior.
Status: Resolved
Resolution reference: P5B-R017
Follow-up gap: Test execution remains deferred until a .NET SDK is available; live tier delivery behavior remains Phase 5E-owned.

### P5B-E020 - Offline Import Positive Test Key Drift

Error ID: P5B-E020
Command: Static updater-test audit via `rtk rg -n Inconclusive tests docs\compile` and source inspection
Configuration: Debug / Release
Project: `TaskTree.Modules.AutoUpdater.Tests`; `TaskTree.Modules.AutoUpdater`
File: `tests/TaskTree.Modules.AutoUpdater.Tests/OfflineImportServiceTests.cs`; `src/TaskTree.Modules.AutoUpdater/ManifestSigner.cs`
Line: `ImportAsync_ValidBundle_StagesPackage_PENDING_KEY`; `ManifestSigner.EmbeddedPublicKeyBase64`
Column: N/A
Compiler error code: N/A
Message: The positive offline-import test remained inconclusive because `ManifestSigner` only verified against the owner-owned embedded production-key placeholder, leaving no safe test key seam for a valid Ed25519 bundle.
Root cause category: Test coverage drift / constructor signature drift
Proposed fix: Preserve the default embedded-key path while adding a public-key override constructor for tests, then use a fixed Ed25519 public key/signature vector for the manifest canonical payload, import the ZIP bundle, and verify the staged MSIX bytes.
Status: Resolved
Resolution reference: P5B-R018
Follow-up gap: Test execution remains deferred until a .NET SDK is available; the real production signing public key remains owner-owned for Phase 5F/release.

### P5B-E021 - TestSupport FakeClock Canonicalization Drift

Error ID: P5B-E021
Command: Static TestSupport audit via `rtk rg -n TaskTree.TestSupport.Clocks docs tests src`, `rtk rg -n TaskTree.TestSupport tests src docs\compile`, and source inspection
Configuration: Debug / Release
Project: `TaskTree.TestSupport`
File: `tests/TaskTree.TestSupport/FakeClock.cs`; `tests/TaskTree.TestSupport/Clocks/FakeClock.cs`
Line: Class declarations
Column: N/A
Compiler error code: N/A
Message: Phase 1D handoff/derivations identify `TaskTree.TestSupport.Clocks.FakeClock` as the promoted canonical helper, but later Phase 5B migrated active tests to a second root `TaskTree.TestSupport.FakeClock` implementation with independent behavior and missing XML documentation.
Root cause category: Test support fake mismatch / duplicate type/file collision
Proposed fix: Preserve the root namespace for active test compatibility, but make it a thin compatibility alias over the promoted `TaskTree.TestSupport.Clocks.FakeClock` implementation and add XML documentation on the public wrapper surface.
Status: Resolved
Resolution reference: P5B-R019
Follow-up gap: Test execution remains deferred until a .NET SDK is available.

### P5B-E022 - Expected Solution Inventory Drift

Error ID: P5B-E022
Command: Static solution inventory audit via `rtk rg --files -g *.csproj src tests`, `type TaskTree.sln`, and `type docs\stitching\expected-solution-projects.md`
Configuration: Debug / Release
Project: `TaskTree.sln`; stitching docs
File: `docs/stitching/expected-solution-projects.md`
Line: Expected Phase 2 Delta Projects; Expected Test Projects; Claude/Codex Gaps
Column: N/A
Compiler error code: N/A
Message: The expected-solution inventory remained at the Phase 5A scaffold state: generated Settings/SessionLock/Snooze project paths were described generically, present UI/Orchestrator/TrayHost and generated module test projects were omitted, and a possible standalone ReminderDelivery module was still listed even though the stitched repo includes reminder delivery through `TaskTree.Orchestrator`.
Root cause category: Missing project / project inventory drift
Proposed fix: Reconcile the expected-solution inventory to the current stitched checkout and static `TaskTree.sln` evidence, while keeping restore/build proof deferred to the .NET SDK gate.
Status: Resolved
Resolution reference: P5B-R020
Follow-up gap: Restore/build/test proof remains deferred until a .NET SDK is available.

### P5B-E023 - Compile Register Category Taxonomy Drift

Error ID: P5B-E023
Command: Static compile-register audit via root-cause category inspection in `docs/compile/compile-error-register.md`
Configuration: N/A
Project: Compile documentation
File: `docs/compile/compile-error-register.md`
Line: Root Cause Categories
Column: N/A
Compiler error code: N/A
Message: Several resolved register entries used root-cause categories that were not listed in the canonical category table, including marker/verifier drift, local environment/toolchain missing, test coverage drift, project inventory drift, missing project reference, and namespace/file inclusion check drift.
Root cause category: Project inventory drift
Proposed fix: Extend the root-cause category table to include categories already used by the current Phase 5B register entries, preserving Gap #373 category consistency.
Status: Resolved
Resolution reference: P5B-R021
Follow-up gap: Restore/build/test proof remains deferred until a .NET SDK is available.

### P5B-E024 - MSTest Package Version Drift

Error ID: P5B-E024
Command: Static package audit via `rtk node C:\tmp\tasktree-static-check.js` and `findstr /n /c:"MSTest.TestAdapter" /c:"MSTest.TestFramework" tests\TaskTree.Core.Tests\TaskTree.Core.Tests.csproj tests\TaskTree.Modules.SecureStore.Tests\TaskTree.Modules.SecureStore.Tests.csproj tests\TaskTree.Modules.BugReporter.Tests\TaskTree.Modules.BugReporter.Tests.csproj`
Configuration: Debug / Release
Project: `TaskTree.Core.Tests`; `TaskTree.Modules.SecureStore.Tests`; `TaskTree.Modules.BugReporter.Tests`
File: `tests/TaskTree.Core.Tests/TaskTree.Core.Tests.csproj`; `tests/TaskTree.Modules.SecureStore.Tests/TaskTree.Modules.SecureStore.Tests.csproj`; `tests/TaskTree.Modules.BugReporter.Tests/TaskTree.Modules.BugReporter.Tests.csproj`
Line: MSTest package references
Column: N/A
Compiler error code: N/A
Message: Three older test projects still referenced `MSTest.TestAdapter` and `MSTest.TestFramework` `3.6.0` while the rest of the stitched test graph had already moved to `3.6.1`, leaving the test package graph split during restore/test discovery.
Root cause category: Package version drift
Proposed fix: Align the older MSTest package references to the repo's current `3.6.1` test baseline without adding new packages or changing test behavior.
Status: Resolved
Resolution reference: P5B-R022
Follow-up gap: Restore/build/test proof remains deferred until a .NET SDK is available.

### P5B-E025 - Test Project Direct Core Reference Drift

Error ID: P5B-E025
Command: Static using/project-reference audit via `rtk node C:\tmp\tasktree-using-reference-check.js`
Configuration: Debug / Release
Project: `TaskTree.Modules.BugReporter.Tests`; `TaskTree.Modules.SecureStore.Tests`
File: `tests/TaskTree.Modules.BugReporter.Tests/TaskTree.Modules.BugReporter.Tests.csproj`; `tests/TaskTree.Modules.SecureStore.Tests/TaskTree.Modules.SecureStore.Tests.csproj`
Line: ProjectReference item group
Column: N/A
Compiler error code: N/A
Message: BugReporter and SecureStore test source files import `TaskTree.Core.*` namespaces directly, but their test project files relied on transitive module references instead of direct `TaskTree.Core` references.
Root cause category: Missing project reference
Proposed fix: Add explicit `ProjectReference` entries to `src/TaskTree.Core/TaskTree.Core.csproj` in the affected test projects.
Status: Resolved
Resolution reference: P5B-R023
Follow-up gap: Restore/build/test proof remains deferred until a .NET SDK is available.

### P5B-E026 - Compile Register DI Category Drift

Error ID: P5B-E026
Command: Static compile-register category audit via `rtk node C:\tmp\tasktree-category-check.js`
Configuration: N/A
Project: Compile documentation
File: `docs/compile/compile-error-register.md`
Line: Root Cause Categories
Column: N/A
Compiler error code: N/A
Message: Current Phase 5B entries used `DI registration mismatch` as a root-cause category, but the canonical category table did not list that category.
Root cause category: Project inventory drift
Proposed fix: Add `DI registration mismatch` to the compile-register root-cause category table to preserve Gap #373 category consistency.
Status: Resolved
Resolution reference: P5B-R024
Follow-up gap: Restore/build/test proof remains deferred until a .NET SDK is available.

### P5B-E027 - AutoUpdater ImportLocalAsync Test Drift

Error ID: P5B-E027
Command: Static updater test audit via `rtk rg -n ImportLocalAsync src tests docs\spec-derivations docs\compile Roadmap.md Architecture.md`
Configuration: Debug / Release
Project: `TaskTree.Modules.AutoUpdater.Tests`
File: `tests/TaskTree.Modules.AutoUpdater.Tests/AutoUpdaterTests.cs`
Line: `ImportLocalAsync_MissingBundle_ThrowsFileNotFound`
Column: N/A
Compiler error code: N/A
Message: `AutoUpdater.ImportLocalAsync` is now implemented against `OfflineImportService`, but an older test still expected a Phase 3C deferred `NotImplementedException`.
Root cause category: Test coverage drift
Proposed fix: Update the stale test to assert current offline import behavior for a missing local bundle (`FileNotFoundException`) while preserving the Phase 5E `ApplyAsync` live MSIX stub expectation.
Status: Resolved
Resolution reference: P5B-R025
Follow-up gap: Restore/build/test proof remains deferred until a .NET SDK is available.

### P5B-E028 - Phase 5B Static Gap Status Drift

Error ID: P5B-E028
Command: Static gap-status audit via `rtk rg -n Partially docs\compile docs\stitching docs\HANDOFF-v1.0.48-delta.md REPO_BRIEF.md`, `rtk node C:\tmp\tasktree-static-check.js`, `rtk node C:\tmp\tasktree-using-reference-check.js`, `rtk node C:\tmp\tasktree-package-import-check.js`, `rtk node C:\tmp\tasktree-duplicate-type-check.js`, `rtk node C:\tmp\tasktree-xaml-check.js`, and `rtk node C:\tmp\tasktree-sln-check.js`
Configuration: N/A
Project: Compile documentation
File: `docs/compile/project-reference-reconciliation.md`; `docs/compile/phase5b-compile-closure-plan.md`; `docs/stitching/expected-solution-projects.md`; `docs/stitching/gap-carryforward-v1.0.46.md`
Line: Phase 5B gap status tables
Column: N/A
Compiler error code: N/A
Message: Several Phase 5B gap rows still said `Partially applied` even though current static audits now prove project/reference, namespace, package/import, XAML/code-behind, duplicate full-type, DI status, and solution Debug/Release configuration reconciliation. Build proof was separately gated at the time and is now closed by `P5B-R027`.
Root cause category: Project inventory drift
Proposed fix: Update the affected status rows to `Reconciled statically; build proof deferred` or equivalent precise wording, without claiming Phase 5B acceptance.
Status: Resolved
Resolution reference: P5B-R026
Follow-up gap: None; SDK-backed restore/build/test proof was later closed by `P5B-R027` and `P5B-R028`.

### P5B-E029 - SDK-Backed Compile Closure Errors

Error ID: P5B-E029
Command: `rtk C:\Users\domin\.dotnet\dotnet.exe build TaskTree.sln -c Debug`; `rtk C:\Users\domin\.dotnet\dotnet.exe build TaskTree.sln -c Release`
Configuration: Debug / Release
Project: Multiple production and test projects
File: `src/TaskTree.Core/Models/TaskNode.cs`; `src/TaskTree.Core/Security/AesGcmCryptoProvider.cs`; `src/TaskTree.Modules.*`; `src/TaskTree.App`; `tests/*`
Line: Multiple compiler output lines
Column: N/A
Compiler error code: CS0104 / CS0419 / CS1674 / CS0103 / CS1061 / CS1503 / CS0854 / CS0723 / CS0246 / CS0118
Message: First SDK-backed Debug builds exposed real compile blockers hidden by the prior missing-SDK environment: `TaskStatus` ambiguity from implicit usings, ambiguous XML cref, non-disposable NSec `PublicKey`, missing DPAPI package reference, inaccessible BugReporter test seam, stale Moq optional-argument expressions, stale App host `CompositionRoot` instance use, missing production `Clock`, and Orchestrator namespace/type ambiguity.
Root cause category: Interface contract drift
Proposed fix: Apply compile-only aliases, XML-doc clarification, package/reference reconciliation, friend assembly metadata, Moq optional-argument fixes, DI/App host alignment, and production `IClock` implementation.
Status: Resolved
Resolution reference: P5B-R027
Follow-up gap: None for Debug/Release compile closure.

### P5B-E030 - SDK-Backed Non-Live Test Gate Drift

Error ID: P5B-E030
Command: `rtk C:\Users\domin\.dotnet\dotnet.exe test TaskTree.sln -c Release --filter TestCategory!=Live`
Configuration: Release
Project: `TaskTree.Core.Tests`; `TaskTree.Modules.SecureStore.Tests`; `TaskTree.Modules.ReminderScheduler.Tests`; `TaskTree.Modules.AutoUpdater.Tests`; `TaskTree.Modules.BugReporter.Tests`
File: `tests/TaskTree.Core.Tests/Security/AesGcmCryptoProviderTests.cs`; `tests/TaskTree.Modules.SecureStore.Tests/SecureStoreTests.cs`; `tests/TaskTree.Modules.ReminderScheduler.Tests/ReminderSchedulerTests.cs`; `tests/TaskTree.Modules.AutoUpdater.Tests/OfflineImportServiceTests.cs`; `tests/TaskTree.Modules.BugReporter.Tests/BugReportRateLimiterTests.cs`
Line: Multiple test failure lines
Column: N/A
Compiler error code: N/A
Message: Release non-live tests initially failed on exact .NET 8 AES-GCM exception subtype assertions, scheduler tests using cadences below the documented one-second minimum, a stale Ed25519 positive offline-import vector, and a rate-limiter test that checked the next calendar day instead of the same-day 50/day boundary.
Root cause category: Test coverage drift
Proposed fix: Align tests with .NET 8 `AuthenticationTagMismatchException`, keep scheduler tests within the documented cadence range, regenerate the test-only Ed25519 vector for the current canonical payload, and correct the same-day rate-limit boundary assertion.
Status: Resolved
Resolution reference: P5B-R028
Follow-up gap: None for the non-live Release test gate.
