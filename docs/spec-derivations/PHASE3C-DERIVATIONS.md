# PHASE3C-DERIVATIONS.md - Offline Import + Rollback Sentinel

> Scope: OfflineImportService, SentinelService, RollbackService, AutoUpdater.ImportLocalAsync patch, tests.
> Source: Roadmap Phase 3C; Architecture.md Sections 9.1.4-9.1.6.
> Owner-approved HALT batch: 24 items.

## Summary

All 24 HALT items approved as proposed. Phase 3C implements the offline ZIP import foundation, sentinel file management, rollback package discovery, and AutoUpdater.ImportLocalAsync integration. Live MSIX rollback/apply remains deferred to Codex Phase 5E.

## Files Produced

- src/TaskTree.Modules.AutoUpdater/OfflineImportService.cs
- src/TaskTree.Modules.AutoUpdater/SentinelService.cs
- src/TaskTree.Modules.AutoUpdater/RollbackService.cs
- src/TaskTree.Modules.AutoUpdater/AutoUpdater.cs
- tests/TaskTree.Modules.AutoUpdater.Tests/OfflineImportServiceTests.cs
- tests/TaskTree.Modules.AutoUpdater.Tests/SentinelServiceTests.cs
- tests/TaskTree.Modules.AutoUpdater.Tests/RollbackServiceTests.cs
- docs/spec-derivations/PHASE3C-DERIVATIONS.md
- docs/HANDOFF-v1.0.38-delta.md
- tools/find-spec-derivations.ps1

## Marker Inventory

PHASE3C = 7 distinct .cs files. Grand total: 142 -> 149.

## Cross-Phase Gaps Introduced (232-243)

| # | Gap | Target | Action |
|---|---|---|---|
| 232 | Offline import bundle format unspecified; ZIP with update.manifest.json + package.msix used | Architecture v1.0.3/5F | Document format |
| 233 | OfflineImportResult is derived | Architecture v1.0.3/5F | Document result type |
| 234 | OfflineImportService constructor/factory updates if injected later | Phase 5B | Update DI/tests if needed |
| 235 | ZIP entry names derived | Architecture v1.0.3/5F | Document entry names |
| 236 | AutoUpdater constructor overloads changed again | Phase 5B | Update factories/tests |
| 237 | Offline import not represented in Phase 3B state graph | Phase 5C/Architecture v1.0.3 | Decide local state path |
| 238 | Verify sentinel path under real Windows profile/install context | Phase 5C | Real FS integration |
| 239 | SentinelService public surface is derived | Architecture v1.0.3/5F | Document methods |
| 240 | Sentinel file content format is derived | Phase 5C | Validate rollback compatibility |
| 241 | Live MSIX rollback restore remains environment gap | Phase 5E | Implement Add-AppxPackage rollback |
| 242 | Rollback directory unspecified; using updates/rollback | Architecture v1.0.3/5F | Document directory |
| 243 | Rollback selection by modified time may be replaced by manifest sidecar | Phase 5E/future | Reassess selection rule |

## Carry-Forward

Gaps #214/#215/#219/#221/#225/#227/#228/#230/#231 remain active. ImportLocalAsync is now implemented against OfflineImportService, but positive import remains blocked by Ed25519 test vector/key material.
