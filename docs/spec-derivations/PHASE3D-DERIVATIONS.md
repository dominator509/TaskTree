# PHASE3D-DERIVATIONS.md - BugReporter Capture + Queue + Redaction

> Scope: BugReport schema, BugReportType enum, RedactionPipeline, BugReportQueue, CrashCaptureHook, BugReporter, tests.
> Source: Roadmap Phase 3D; Architecture.md Sections 4.8 and 9.2.1-9.2.3.
> Owner-approved HALT batch: 24 items.

## Summary

All 24 HALT items approved as proposed. Phase 3D implements local bug report capture, redaction, encrypted queue persistence via ISecureStore, fingerprint deduplication, and crash hook registration surface. Delivery remains deferred to Phase 3E.

## Files Produced

- src/TaskTree.Core/Enums/BugReportType.cs
- src/TaskTree.Core/Models/BugReport.cs
- src/TaskTree.Modules.BugReporter/RedactionPipeline.cs
- src/TaskTree.Modules.BugReporter/BugReportQueue.cs
- src/TaskTree.Modules.BugReporter/CrashCaptureHook.cs
- src/TaskTree.Modules.BugReporter/BugReporter.cs
- tests/TaskTree.Modules.BugReporter.Tests/RedactionPipelineTests.cs
- tests/TaskTree.Modules.BugReporter.Tests/BugReportQueueTests.cs
- tests/TaskTree.Modules.BugReporter.Tests/BugReporterTests.cs
- tests/TaskTree.Modules.BugReporter.Tests/CrashCaptureHookTests.cs
- docs/spec-derivations/PHASE3D-DERIVATIONS.md
- docs/HANDOFF-v1.0.39-delta.md
- tools/find-spec-derivations.ps1

## Marker Inventory

PHASE3D = 10 distinct .cs files. Grand total: 149 -> 159.

## Cross-Phase Gaps Introduced (244-260)

| # | Gap | Target | Action |
|---|---|---|---|
| 244 | Verify/patch BugReport.cs to match Architecture Section 9.2.1 | Phase 3D/5B | Confirm build/schema |
| 245 | Add BugReportType.cs to Architecture Section 3.3 Enums | Architecture v1.0.3/5F | Amend enum list |
| 246 | Add exact BugReport JSON schema compatibility tests | Phase 5C | Schema vectors |
| 247 | Validate no unredacted free text is persisted | Phase 5C/4A | Persistence inspection |
| 248 | BugReport null-normalization behavior is derived | Architecture v1.0.3/5F | Document behavior |
| 249 | BugReport queue SecureStore key bugreports/queue is derived | Architecture v1.0.3/5F | Document key |
| 250 | BugReportQueue public surface is derived | Architecture v1.0.3/5F | Document methods |
| 251 | Dedup by fingerprint at queue boundary needs flush/delivery validation | Phase 5C | Delivery integration tests |
| 252 | BugReporter constructor/dependency surface is derived; DI later | Phase 3E/5B | Wire DI after delivery |
| 253 | BugReport normalization rules are derived | Architecture v1.0.3/5F | Document rules |
| 254 | Fingerprint formula should be validated | Phase 5C | Collision/dedup tests |
| 255 | FlushQueueAsync delivery behavior deferred | Phase 3E | Implement router flush |
| 256 | RedactionEnabled=false still redacts; confirm policy | Phase 4A | Owner/compliance decision |
| 257 | Real crash injection validation deferred | Phase 5E | Live crash test |
| 258 | Default crash severity set to High; confirm severity policy | Phase 4A | Owner/compliance decision |
| 259 | CrashCaptureHook tests limited until live crash injection | Phase 5E | Live tests |
| 260 | BugReporter DI registration deferred | Phase 3E/5B | Wire after dependencies stabilize |

## Carry-Forward

Phase 3E must implement delivery, routing, file drop, rate limiting, and live-stub adapters. Phase 4A must confirm redaction toggle and crash severity policy.
