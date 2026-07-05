# Phase 5A Using-Statement Migration Manifest (Gap #106 + Gap #112 closure)

> **Status:** APPLIED in stitched repo by Codex during Phase 5B compile/stitching closure.
> **Trigger:** Phase 2A Msg 1 promoted FakeClock + InMemorySecureStore to TaskTree.TestSupport project per Gap #97/#98.
> **Author:** DSW - **Date:** 2026-05-29

## Section 1 Scope

This manifest enumerates all test files requiring `using` statement migration from `TaskTree.Core.Tests.TestDoubles` to `TaskTree.TestSupport` namespace after Phase 2A Msg 1 promotion.

Gap #106 originally estimated 4 affected files. **Gap #112 revises count to 8** after enumeration of Phase 1G Msg 2 backfill + Phase 1H E2E test + Phase 1A baseline consumers.

## Section 2 Affected Files Table

| File Path | Remove | Add | Types | Verification |
|---|---|---|---|---|
| tests/TaskTree.Modules.ReminderScheduler.Tests/ReminderSchedulerTests.cs | `using TaskTree.Core.Tests.TestDoubles;` | `using TaskTree.TestSupport;` | FakeClock | `grep -n FakeClock` |
| tests/TaskTree.Modules.ReminderScheduler.Tests/CadencePolicyTests.cs | same | same | FakeClock | `grep -n FakeClock` |
| tests/TaskTree.Modules.ComplianceCore.Tests/ComplianceCoreTests.cs | same | same | FakeClock | `grep -n FakeClock` |
| tests/TaskTree.Modules.ComplianceCore.Tests/AuditChainWriterTests.cs | same | same | FakeClock | `grep -n FakeClock` |
| tests/TaskTree.Modules.TaskEngine.Tests/TaskEngineTests.cs | same | same | FakeClock | `grep -n FakeClock` |
| tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs | same | same | FakeClock | `grep -n FakeClock` |
| tests/TaskTree.Orchestrator.Tests/ReminderDeliveryServiceTests.cs | same | same | FakeClock | `grep -n FakeClock` |
| tests/TaskTree.Orchestrator.Tests/EndToEndOfflineTests.cs | same | same | FakeClock + InMemorySecureStore | `grep -n "FakeClock\|InMemorySecureStore"` |

## Section 3 Old-Location Deletion List (Gap #103/#104 closure)

After migration is complete, delete the following obsolete files:

- `tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs` - DELETED
- `tests/TaskTree.Core.Tests/TestDoubles/InMemorySecureStore.cs` - DELETED
- Optionally delete entire `tests/TaskTree.Core.Tests/TestDoubles/` directory if empty after the two file deletions

**Verification:** `grep -r FakeClock tests/TaskTree.Core.Tests/` must return zero matches.

## Section 4 Verification Commands

Phase 5A action sequence (run from repo root):

1. **Pre-check old namespace:** `grep -rn "using TaskTree.Core.Tests.TestDoubles" tests/` must return zero matches after migration.
2. **Verify new namespace adoption:** `grep -rn "using TaskTree.TestSupport" tests/` must return at least 9 matches (8 migrated files + HotkeyManagerTests.cs which already uses TaskTree.TestSupport from Phase 2A Msg 1).
3. **NuGet restore:** `dotnet restore TaskTree.sln` must succeed.
4. **Build verification:** `dotnet build TaskTree.sln -c Release` must succeed with zero errors.
5. **Test execution:** `dotnet test TaskTree.sln --filter "TestCategory!=Live"` must pass (or honor SKELETON Assert.Inconclusive results per Gap #95 if Codex has not yet backfilled them).

## Section 5 Migration Execution Order

1. Update `using` statements in 8 files (Section 2 table).
2. Delete old locations (Section 3 list).
3. Run `dotnet restore` to refresh ProjectReference resolution.
4. Run `dotnet build` to verify zero errors.
5. Run `dotnet test` to verify behavioral preservation.

## Section 6 Known Limitations

1. Phase 1G+1H skeleton tests (Gap #95) will return Assert.Inconclusive post-migration - acceptable per HANDOFF v1.0.24 Section H; full test backfill is Codex Phase 5C scope.
2. If Codex finds additional consumers of `TaskTree.Core.Tests.TestDoubles` beyond this 8-file list (e.g., PhiRedactorTests.cs if updated during Phase 1C work not captured in derivations), Codex MUST add them to migration scope and log as **Gap #115**.
3. Migration is purely text-substitution; no behavioral changes expected.
4. If old-location deletion leaves `TestDoubles/` folder empty, Phase 5A may also delete the empty folder for cleanliness.

## Section 7 Codex Application Notes

- Remaining old namespace consumer found in `tests/TaskTree.Modules.ReminderScheduler.Tests/ReminderSchedulerTests.cs` and migrated to `TaskTree.TestSupport`.
- Obsolete old-location files were deleted after confirming `tests/TaskTree.TestSupport/` contains the promoted replacements and dependent test projects already reference `TaskTree.TestSupport`.
- Build/test verification is still pending a local .NET SDK; see `docs/compile/compile-error-register.md` entry `P5B-E002`.
