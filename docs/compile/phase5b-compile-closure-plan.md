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

## Current Execution Status

| Step | Status | Evidence / next gate |
|---|---|---|
| Confirm stitched repo is authoritative | Applied | `docs/stitching/*.md` now records current repo evidence and source-of-truth boundaries. |
| Marker verification | Passing | `rtk powershell -NoProfile -ExecutionPolicy Bypass -File tools/find-spec-derivations.ps1 -Root .` reports `Grand total: 179 expected 179`. |
| Restore | Deferred | `rtk dotnet restore TaskTree.sln` fails locally because `dotnet` is unavailable on PATH (`P5B-E002`). |
| Debug build | Blocked by restore/toolchain | Requires .NET SDK. |
| Release build | Blocked by restore/toolchain | Requires .NET SDK; also gates Tier 2 obsolete window deletion. |
| Compile-error register | Populated/current | Entries `P5B-E001` through `P5B-E020`; `P5B-E002` and `P5B-E011` remain deferred. |
| Compile-resolution log | Populated/current | Resolutions through `P5B-R018`; all applied static fixes are logged. |
| Phase 5C handoff | Not ready | Phase 5C requires successful restore, Debug build, and Release build first. |

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
| #367 | Compile fixes must not redesign features or expand scope beyond buildability | Preserved; static fixes only |
| #375 | Constructor churn must be reconciled without changing runtime behavior | Further applied via orchestrator and E2E offline test backfills; build proof deferred |
| #376 | DI registrations must be reconciled and unresolved runtime graph gaps documented | Partially applied via `TaskTreePaths`; build proof deferred |
| #378 | Duplicate/conflicting models/enums must be resolved with selected source and compatibility rationale | Partially applied via model/enum static audits and TestSupport duplicate deletion |
| #379 | Interface/implementation drift must be closed systematically and recorded | Further applied via reminder delivery snooze test backfill; build proof deferred |
| #221 | Positive offline import requires a safe Ed25519 test-key seam without adding a production key | Applied via `ManifestSigner` public-key override and signed-bundle test; build proof deferred |
| #382 | TestSupport fakes must be reconciled with final interfaces and constructors | Further reconciled via shared TestSupport consumer migration; compile proof deferred |
| #384 | Actual compile closure requires Claude/Codex on stitched repo with real command outputs | Still open; restore/build blocked by missing .NET SDK |
| #319 | Final `.wapproj` project references must be reconciled against stitched repo projects | Applied statically; MSIX tooling proof deferred to Phase 5E |
