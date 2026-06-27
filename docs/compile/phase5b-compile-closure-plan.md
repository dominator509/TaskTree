# phase5b-compile-closure-plan.md - Phase 5B Compile Closure Plan

> Phase 5B is compile closure only. Actual code fixes must be performed by Claude/Codex in the stitched repository and logged in the compile registers.

## Scope Boundary

Phase 5B may fix only what is required to make Debug and Release builds compile. It must not redesign features, remove safety controls, fake live behavior, or expand product scope.

## Execution Sequence

1. Confirm stitched repo is authoritative.
2. Run marker verification.
3. Run restore.
4. Run Debug build.
5. Populate compile-error register.
6. Categorize errors.
7. Apply compile-only fixes in small batches.
8. Log every fix in compile-resolution log.
9. Re-run Debug build until clean.
10. Run Release build.
11. Resolve Release-only errors.
12. Attach command outputs.
13. Prepare Phase 5C handoff.

## Required Commands

```powershell
pwsh -File tools/find-spec-derivations.ps1 -Root .
dotnet restore TaskTree.sln
dotnet build TaskTree.sln -c Debug
dotnet build TaskTree.sln -c Release
```

## Constructor Churn Reconciliation

Prefer this order:

1. Latest generated production class constructor.
2. Interface contract requirements.
3. Existing DI registration conventions.
4. TestSupport fakes updated to match production constructors.

High-risk constructor areas:

- AutoUpdater collaborators.
- BugReporter queue/redaction/delivery/router.
- SettingsService.
- SessionLock.
- ReminderDelivery.
- SecureStore / ComplianceCore dependencies.

## DI Registration Reconciliation

Update DI/service registrations only enough to compile and instantiate the expected runtime graph. Document unresolved runtime graph questions as Phase 5C/5E gaps.

## Model / Enum Conflict Reconciliation

High-risk types:

- `UpdaterState`
- `BugReportType`
- `BugReport`
- `UpdateManifest`
- `BugSeverity`
- update package/signature nested models

If duplicates exist, select the latest Architecture-aligned version and document the superseded version.

## Interface Contract Reconciliation

Ensure implementations match interfaces:

- `IAutoUpdater`
- `IBugReporter`
- `IComplianceCore`
- `ISecureStore`
- `IClock`
- `IAppLogger`
- reminder/settings/session abstractions

If an interface must evolve for compile closure, update all implementers/tests and record rationale.

## Explicit Stubs to Preserve

Do not fake success for:

- live HTTP update
- `Add-AppxPackage`
- MSIX rollback restore
- SMTP
- GitHub Issues
- live crash injection
- CredUI / idle detection if stubbed

## Phase 5C Handoff Criteria

Phase 5B is complete only when:

- Marker script passes.
- `dotnet restore` succeeds.
- Debug build succeeds.
- Release build succeeds.
- Compile-error register has no open compile-blocking errors.
- Compile-resolution log records all fixes.
- Remaining issues are classified for Phase 5C/5D/5E/5F.

## Phase 5B Gaps

| Gap | Description | Target |
|---|---|---|
| #367 | Compile fixes must not redesign features or expand scope beyond buildability | Phase 5B |
| #375 | Constructor churn must be reconciled without changing runtime behavior | Phase 5B |
| #376 | DI registrations must be reconciled and unresolved runtime graph gaps documented | Phase 5B |
| #378 | Duplicate/conflicting models/enums must be resolved with selected source and compatibility rationale | Phase 5B |
| #379 | Interface/implementation drift must be closed systematically and recorded | Phase 5B |
| #382 | TestSupport fakes must be reconciled with final interfaces and constructors | Phase 5B |
| #384 | Actual compile closure requires Claude/Codex on stitched repo with real command outputs | Claude/Codex Phase 5B |
