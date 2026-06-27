# stitched-file-manifest.md - Phase 5A Stitched File Manifest

> Phase 5A manifest scaffold. Claude/Codex must update this after applying actual bundles to the repository.

## Source Files Expected

### `src/`

- TaskTree.App
- TaskTree.Core
- TaskTree.UI
- TaskTree.Orchestrator
- TaskTree.Modules.TaskEngine
- TaskTree.Modules.ReminderScheduler
- TaskTree.Modules.SecureStore
- TaskTree.Modules.ComplianceCore
- TaskTree.Modules.TrayHost
- TaskTree.Modules.AutoUpdater
- TaskTree.Modules.BugReporter
- Phase 2 delta modules such as Settings, SessionLock, Snooze, and ReminderDelivery if present in generated bundles

### `tests/`

- Core/module unit tests
- AutoUpdater tests
- BugReporter tests
- ComplianceCore tests
- Performance tests
- TestSupport

### `docs/`

- Architecture/Roadmap/HANDOFF source docs
- Phase derivation docs
- Compliance audit
- Performance report
- Signing checklist
- User/admin/deployment/release docs
- Stitching docs

### `tools/`

- `find-spec-derivations.ps1`

### `packaging/`

- `TaskTree.Installer.wapproj`
- `Package.appxmanifest`
- `build-msix.ps1`

## Phase 5B Review Targets

- Confirm every generated `.cs` file is included in a project or intentionally excluded.
- Confirm every `.csproj` is included in `TaskTree.sln` where appropriate.
- Confirm packaging paths match the stitched source tree.
- Confirm test support fakes compile and namespaces resolve.

## Claude/Codex Gap

Gap #358: Phase 5A must create a stitched file manifest so Phase 5B can verify project inclusion and missing files.
