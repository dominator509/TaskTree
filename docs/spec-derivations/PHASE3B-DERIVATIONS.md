# PHASE3B-DERIVATIONS.md - AutoUpdater State Machine + Staging

> Scope: UpdaterState enum, UpdaterStateMachine, VersionEligibilityEvaluator, StagingService, AutoUpdater collaborator patch, tests.
> Source: Roadmap Phase 3B; Architecture.md Sections 9.1.1 and 9.1.4.
> Owner-approved HALT batch: 24 items.

## Summary

All 24 HALT items approved as proposed. Phase 3B implements deterministic updater state transitions, version/rollout eligibility, file staging under a testable staging root, post-write size/hash verification, and AutoUpdater collaborator wiring without live HTTP.

## Files Produced

- src/TaskTree.Core/Enums/UpdaterState.cs
- src/TaskTree.Modules.AutoUpdater/UpdaterStateMachine.cs
- src/TaskTree.Modules.AutoUpdater/VersionEligibilityEvaluator.cs
- src/TaskTree.Modules.AutoUpdater/StagingService.cs
- src/TaskTree.Modules.AutoUpdater/AutoUpdater.cs
- tests/TaskTree.Modules.AutoUpdater.Tests/UpdaterStateMachineTests.cs
- tests/TaskTree.Modules.AutoUpdater.Tests/StagingServiceTests.cs
- tests/TaskTree.Modules.AutoUpdater.Tests/VersionEligibilityEvaluatorTests.cs
- tests/TaskTree.Modules.AutoUpdater.Tests/AutoUpdaterTests.cs
- docs/Phase3B-updater-state-transition-manifest.md
- docs/spec-derivations/PHASE3B-DERIVATIONS.md
- docs/HANDOFF-v1.0.37-delta.md
- tools/find-spec-derivations.ps1

## Marker Inventory

PHASE3B = 9 distinct .cs files. Grand total: 133 -> 142.

## Cross-Phase Gaps Introduced (222-231)

| # | Gap | Target | Action |
|---|---|---|---|
| 222 | Architecture Section 3.3 Enums must add UpdaterState.cs | Architecture v1.0.3/5F | Amend enum list |
| 223 | UpdaterStateMachine public surface is derived | Architecture v1.0.3/5F | Document surface |
| 224 | Move UpdaterStateChangedEventArgs to Core.Models if cross-module/UI-visible | Future refactor | Reassess usage |
| 225 | Transition graph should be validated against full updater flow | Phase 5C | Integration tests |
| 226 | System.Version comparison lacks SemVer prerelease/build metadata support | Future HALT/5C | Decide if SemVer needed |
| 227 | Rollout bucket derivation undefined; caller-supplied bucket used | Phase 3C/5E | Define install identity/bucket |
| 228 | Verify staging path resolves under real Windows profile | Phase 5C | Real FS test |
| 229 | Staged file naming unspecified; using TaskTree-{version}.msix | Architecture v1.0.3/5F | Document convention |
| 230 | Test staged file tamper/mismatch behavior with real filesystem permissions | Phase 5C | Real FS permissions test |
| 231 | AutoUpdater orchestration partially wired; live check/download deferred | Phase 3C/5E | Implement later flow |

## Carry-Forward

Gaps #214/#215/#218/#219/#220/#221 remain active. ApplyAsync remains Phase 5E stub. ImportLocalAsync remains Phase 3C stub. Audit vocabulary remains deferred.
