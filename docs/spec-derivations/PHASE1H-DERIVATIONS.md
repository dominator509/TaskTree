# PHASE1H-DERIVATIONS.md - Phase 1H Final E2E Gate

> Scope: EndToEndOfflineTests + InMemorySecureStore promotion + Gap #95 backfill skeletons + Phase 1 closure.
> Owner-approved HALT batch: 20 items.

## Section 1 Summary Table

| # | Item | Resolution | LOAD-BEARING? |
|---|---|---|---|
| 1 | Gap #95 backfill scope | Skeleton emission per Gap #95 pattern | Closes #95 |
| 2 | EndToEnd test count | 8 tests | No |
| 3 | DI graph construction | Custom IServiceCollection (offline) | No (Gap #96) |
| 4 | IClock strategy | FakeClock (Gap #89 escalates to #97) | Yes #97 |
| 5 | ISecureStore strategy | InMemorySecureStore promoted | Yes #98 |
| 6 | IComplianceCore strategy | Real + InMemoryStore = real audit E2E | No |
| 7 | ITrayHost strategy | Real (NotImplementedException catch verified) | No |
| 8 | IReminderScheduler | Real with Cadence=100ms | No |
| 9 | Cascade simulation | DeliveryFailedAllTiers | No |
| 10 | Test file split | 3 test files | No |
| 11 | Audit chain integrity test | Dedicated test | No |
| 12 | Coverage measurement | Deferred to Phase 5C (Gap #99) | No |
| 13 | Msg structure | Single Msg | No |
| 14 | csproj patches | Zero | No |
| 15 | PowerShell | PHASE1H = 4 | No |
| 16 | HANDOFF v1.0.24 = Phase 1 COMPLETE | Owner gate activated | Yes |
| 17 | Phase 1 owner approval gate | HARD GATE per Roadmap | Yes |
| 18 | Q10/Q11 status | Q11 LOAD-BEARING for Phase 2 | Yes Q11 |
| 19 | Architecture v1.0.2 | 6 changes pending | Yes |
| 20 | Final state docs | 99-gap status + Phase 5 manifest | Yes |

## Sections 2-21 Per-Item Detail

See HANDOFF v1.0.24 section C R19 for bullet summary.

## Section 22 Files Produced

| Path | Purpose | Marker |
|---|---|---|
| tests/TaskTree.Orchestrator.Tests/EndToEndOfflineTests.cs | SKELETON 8-test plan | PHASE1H |
| tests/TaskTree.Core.Tests/TestDoubles/InMemorySecureStore.cs | NEW promoted test double | PHASE1H |
| tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs | SKELETON 15-test plan | PHASE1F-MSG2 + PHASE1G-MSG2 + PHASE1H |
| tests/TaskTree.Orchestrator.Tests/ReminderDeliveryServiceTests.cs | SKELETON 14-test plan | PHASE1G-MSG2 + PHASE1H |
| docs/spec-derivations/PHASE1H-DERIVATIONS.md | This registry | (md) |
| docs/HANDOFF-v1.0.24-delta.md | Phase 1 COMPLETE delta | (md) |
| tools/find-spec-derivations.ps1 | UPDATED 17 buckets | (script) |

## Section 23 Marker Inventory

PHASE1H = 4 distinct .cs files. Grand total: 70 -> 74.

## Section 24 Cross-Phase Gaps Introduced (rows 96-99)

| # | Gap | Source | Target Phase | Status |
|---|---|---|---|---|
| 96 | Phase 5C real CompositionRoot integration | HALT #3 | Phase 5C | DEFERRED |
| 97 | **FakeClock Option B promotion** | HALT #4 (escalates #89) | Phase 2A | **ACTIVE LOAD-BEARING** |
| 98 | InMemorySecureStore promoted (8th consumer) | HALT #5 | Phase 2A | **ACTIVE LOAD-BEARING** |
| 99 | Phase 5C coverage measurement via coverlet | HALT #12 | Phase 5C | DEFERRED |

## Section 25 Patches Applied (Gap #95 backfill via skeleton emission)

### OrchestratorTests.cs (15 tests)

Build helper: `TestHarness Build(bool initializeThrowsNotImplemented = true, bool chainValid = true, bool chainVerifyThrows = false)` returning Orchestrator + FakeClock + Mock<ITaskEngine>(Loose) + Mock<IReminderScheduler>(Strict + SetupAdd/Remove ReminderDue + StartAsync/StopAsync) + Mock<IComplianceCore>(Strict + VerifyChainIntegrityAsync + AuditAsync) + Mock<ITrayHost>(Strict + SetupAdd/Remove 3 events + Dispose + Initialize toggleable) + Mock<IReminderDeliveryService>(Strict + StartAsync/StopAsync) + Mock<IAppLogger>(Loose).

15 tests: Constructor_NullArgs_Throw (7 paths), StartAsync_HappyPath_AuditsStartup, StartAsync_TrayHostInitializeThrowsNotImplementedException_LogsWarningAndContinues, StartAsync_TrayHostInitializeThrowsOtherException_Propagates, StartAsync_ChainVerifyReturnsFalse_LogsAndAuditsFailure_ContinuesStartup, StartAsync_ChainVerifyThrows_LogsErrorAndAuditsFailure_ContinuesStartup, StartAsync_CalledTwice_ThrowsInvalidOperationException, StopAsync_WhenNotRunning_IsNoOp, StopAsync_CallsModulesInReverseOrder (callOrder: unsub < RDS.Stop < RS.Stop < TH.Dispose < Audit.Shutdown), StartThenStop_HappyPath_AuditsBothLifecycleEntries, Dispose_CalledTwice_DoesNotThrow, Dispose_ThenAnyPublicMethod_ThrowsObjectDisposedException, [Integration] CompositionRoot_BuildServiceProvider_ResolvesIOrchestratorSuccessfully, StopAsync_CallsReminderDeliveryServiceBeforeReminderScheduler (Gap #83 #1), StartAsync_HappyPath_VerifiesReminderDeliveryServiceStartAsyncCalled.

### ReminderDeliveryServiceTests.cs (14 tests)

Build helper: FakeClock pinned 2026-06-01T12:00:00Z (Gap #89 6th consumer), Mock<IReminderScheduler>(Strict + SetupAdd/SetupRemove ReminderDue), Mock<IComplianceCore>(Strict + Setup AuditAsync), Mock<IAppLogger>(Loose), Mock<ITrayHost>(Loose), real ToastTier1Adapter(logger), real ToastTier2Adapter(logger), real ToastTier3Adapter(trayHost, logger), real ReminderDeliveryService(7 args). MakeEvent: new ReminderEvent { TaskId = Guid.NewGuid(), FiredAtUtc = TestEpoch, Reason = ReminderReason.Initial, Priority = Priority.Normal }.

14 tests: Constructor_NullArgs_Throw (7), SelectTier_ToastAvailableAndFocusAssistOff_ReturnsTier1, [DataTestMethod] SelectTier_AllOtherCombos_ReturnsTier2 (3 DataRows), StartAsync_SubscribesToReminderDue, StopAsync_UnsubscribesFromReminderDue, StartAsync_CalledTwice_ThrowsInvalidOperationException, StopAsync_WhenNotRunning_IsNoOp, Dispose_CalledTwice_DoesNotThrow, Dispose_ThenStartAsync_ThrowsObjectDisposedException, OnReminderDue_AllTiersStubbed_AuditsDeliveryFailedAllTiers (Mock.Raise + Task.Delay(50) + Verify), OnReminderDue_Tier1ThrowsLoggedAsInfo, OnReminderDue_Tier3ThrowsLoggedAsError, OnReminderDue_Tier2NoApplicationContextLoggedAsWarning, OnReminderDue_HandlerExceptionsDoNotCrashService.

### EndToEndOfflineTests.cs (8 tests)

Build helper: BuildOfflineProvider(FakeClock clock, InMemorySecureStore store, Mock<IAppLogger>? loggerMock = null) creates IServiceCollection with IClock=clock, ICryptoProvider=AesGcmCryptoProvider, IAppLogger=loggerMock?.Object ?? Loose Mock, ISecureStore=store, IMasterKeyManager=Mock.Of<IMasterKeyManager>(), IComplianceCore via factory wiring AuditChainWriter + PhiRedactor(empty), ITaskEngine, IReminderScheduler, ITrayHost, ToastTier1/2/3, IReminderDeliveryService factory with 7 args, IOrchestrator factory with 7 args. Returns BuildServiceProvider(validateScopes: true). TestEpoch = new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero).

8 tests as listed in Section 1.

## Section 26 Phase 1 Complete State Summary

All 8 Phase 1 sub-phases delivered. Architecture v1.0.2: 6 bundled changes pending. 99 cross-phase gaps tracked.

## Section 27 Known Limitations

1. Phase 1H test backfill emitted as SKELETONS per Gap #95; Codex implements at Phase 5C.
2. Coverage measurement deferred to Phase 5C (Gap #99).
3. Architecture v1.0.2 owner sign-off pending; 5 REQUIRED changes block Phase 5F.
4. Q11 PhiRedactor allowlist owner decision REQUIRED at Phase 1 gate for Phase 2.
5. FakeClock + InMemorySecureStore Option B promotion REQUIRED at Phase 2A.
6. Real CompositionRoot integration test deferred to Phase 5C (Gap #96).
7. Live tier delivery (Tier 1 + Tier 3) deferred to Phase 5E.
8. Phase 1 GATE per Roadmap requires explicit owner approval before Phase 2A.
