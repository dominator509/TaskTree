# project-reference-reconciliation.md - Phase 5B Project / Solution Reconciliation

> Phase 5B compile-closure plan for solution, project references, package references, namespaces, and test inclusion.

## Required Solution Checks

1. Confirm `TaskTree.sln` exists.
2. Confirm every expected product project from `docs/stitching/expected-solution-projects.md` is present or intentionally absent with rationale.
3. Confirm every generated test project is present or intentionally absent with rationale.
4. Confirm `tests/TaskTree.Perf.Tests/TaskTree.Perf.Tests.csproj` is included if performance tests are retained.
5. Confirm packaging project path is valid for Phase 5E, even if not built during normal `dotnet build`.

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
| #361 | Verify `TaskTree.sln` contains every expected project after stitching | Phase 5B |
| #362 | Reconcile project references and namespace churn from generated modules/tests | Phase 5B |
| #374 | Any new package reference must be source-approved or documented for owner review | Phase 5B / Owner |
| #377 | Namespace mismatches must be reconciled and logged if file moves are required | Phase 5B |
| #380 | WPF/Windows-specific projects must use compatible target frameworks and properties | Phase 5B |
| #381 | XAML/code-behind mismatches must be corrected or documented for UI validation | Phase 5B / 5D / 5E |
| #383 | Generated test files must be included in test projects and compile before Phase 5C execution | Phase 5B / 5C |
