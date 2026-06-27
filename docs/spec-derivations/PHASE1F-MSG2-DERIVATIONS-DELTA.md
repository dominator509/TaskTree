# PHASE1F-MSG2-DERIVATIONS-DELTA.md — Phase 1F Msg 2 (Tests + Architecture v1.0.2 Update)

> **Scope:** OrchestratorTests + csproj patch + Architecture v1.0.2 delta document update (Gap #63 closure).
> **Companion to:** PHASE1F-DERIVATIONS.md (Msg 1 base).
> **Companion docs:** Architecture.md v1.0.1 (v1.0.2 amendment SCOPE-FINAL pending), Roadmap.md v1.0.0, HANDOFF.md v1.0.21.
> **Owner-approved HALT batch:** 15 items, batch-resolved.

---

## §1 Summary Table

| # | Item | Resolution | Files | LOAD-BEARING? |
|---|---|---|---|---|
| 1 | Test count | 14 total | OrchestratorTests.cs | No |
| 2 | File structure | Single file with logical regions | OrchestratorTests.cs | No |
| 3 | IComplianceCore mock | **Moq Strict — Gap #66 compile-enforced** | OrchestratorTests.cs | No (Gap #72) |
| 4 | ITrayHost mock | Strict + Initialize behavior toggle | OrchestratorTests.cs | No |
| 5 | IReminderScheduler mock | Strict + SetupAdd/Remove for ReminderDue | OrchestratorTests.cs | No |
| 6 | ITaskEngine mock | Loose — future drift detector | OrchestratorTests.cs | No (Gap #73) |
| 7 | IClock test double | FakeClock 5th consumer | OrchestratorTests.cs | No (Gap #74) |
| 8 | IAppLogger mock | Loose; no verify (except specific tests) | OrchestratorTests.cs | No |
| 9 | NotImplementedException catch verify | 1 test (Gap #64) | OrchestratorTests.cs | No |
| 10 | Other exception propagation | 1 anti-drift test | OrchestratorTests.cs | No |
| 11 | Chain-verify failure tests | 2 tests (false + throws) | OrchestratorTests.cs | No |
| 12 | Reverse-order shutdown test | List<string> callOrder via Callbacks | OrchestratorTests.cs | No |
| 13 | E2E audit chain verification | 2 tests (Happy path + Start+Stop) | OrchestratorTests.cs | No |
| 14 | CompositionRoot smoke | 1 Integration-category test | OrchestratorTests.cs + csproj | No (Gap #71/#75) |
| 15 | Architecture v1.0.2 update | 4 bundled changes (added §4 IOrchestrator) | docs/Architecture.v1.0.2-delta.md | No (Gap #63 closure) |

---

## §2-§16 — Per-item Sections

### §2 — Item #1: Test Count
- **Trigger:** Owner discretion (1A=14, 1C=13, 1D=20, 1E=16).
- **Resolution:** 14 tests.
- **Gap for Handoff:** None.

### §3 — Item #2: File Structure
- **Resolution:** Single file with logical regions.
- **Rationale:** Orchestrator is one cohesive class.
- **Gap for Handoff:** None.

### §4 — Item #3: IComplianceCore Moq Strict
- **Trigger:** Gap #66 — Orchestrator audits ONLY 2 lifecycle events.
- **Resolution:** Moq Strict with explicit Setups for VerifyChainIntegrityAsync + AuditAsync.
- **Rationale:** Compile-time enforcement of lifecycle-only audit contract.
- **Gap for Handoff:** Cross-Phase Gap #72 — Phase 5E live audit may add more entries; Codex switches to Loose with Setup + new Verify assertions.

### §5 — Item #4: ITrayHost Mock + Initialize Toggle
- **Resolution:** Strict + boolean parameter to toggle Initialize() throwing NotImplementedException vs no-op.
- **Rationale:** Simulates Phase 1E stub (default) vs Phase 5E live impl (override).
- **Gap for Handoff:** None.

### §6 — Item #5: IReminderScheduler Mock
- **Resolution:** Strict + Setup StartAsync/StopAsync + SetupAdd/Remove for ReminderDue event.
- **Rationale:** Catches subscribe/unsubscribe lifecycle drift.
- **Gap for Handoff:** None.

### §7 — Item #6: ITaskEngine Mock Loose
- **Resolution:** Loose; never set up, never verified.
- **Rationale:** Phase 1F doesn't invoke ITaskEngine; Strict would surface zero-call expectations as noise.
- **Gap for Handoff:** Cross-Phase Gap #73 — Switch to Strict at Phase 1G/2B when tray events route to engine actions.

### §8 — Item #7: FakeClock 5th Consumer
- **Resolution:** Continue with Option A (existing TaskTree.Core.Tests/TestDoubles); defer Option B promotion.
- **Rationale:** Re-evaluating Option B now would block Phase 1F Msg 2.
- **Gap for Handoff:** **Cross-Phase Gap #74 — REVISIT REQUIRED at next consumer addition (Phase 2A or 2B).**

### §9 — Item #8: IAppLogger Mock Loose
- **Resolution:** Loose; verify only for specific tests (NotImplementedException catch warning, chain-verify warning/error).
- **Gap for Handoff:** None.

### §10 — Item #9: NotImplementedException Catch Test
- **Resolution:** 1 test verifies catch + log warning + continuation (RS.StartAsync still called).
- **Gap for Handoff:** Cross-Phase Gap #64 already documented in Msg 1 — Phase 5E Codex removes try/catch + this test.

### §11 — Item #10: Other Exception Propagation
- **Resolution:** 1 test verifies InvalidOperationException from TrayHost.Initialize propagates (anti-drift).
- **Rationale:** Protects narrow-catch design from future regression.
- **Gap for Handoff:** None (test guards Gap #64 design forever).

### §12 — Item #11: Chain-Verify Failure Tests
- **Resolution:** 2 tests — ReturnsFalse + Throws paths.
- **Gap for Handoff:** Phase 2B will add ChainVerifyFailed UI surfacing (carries Gap #9).

### §13 — Item #12: Reverse-Order Shutdown
- **Resolution:** List<string> callOrder via Moq Callbacks; assert IndexOf comparisons.
- **Rationale:** Moq doesn't have native call-order verification.
- **Gap for Handoff:** Test is fragile to refactoring — see §23 Known Limitations.

### §14 — Item #13: E2E Audit Verification (P1F-AC3)
- **Resolution:** 2 tests — HappyPath_AuditsStartup + StartThenStop_AuditsBothLifecycleEntries.
- **Gap for Handoff:** Phase 1H deep integration owns full E2E flow with real audit chain (Gap #70).

### §15 — Item #14: CompositionRoot Smoke (P1F-AC1)
- **Resolution:** 1 [TestCategory("Integration")] test resolves IOrchestrator from real DI container.
- **Rationale:** CompositionRoot creates real %LOCALAPPDATA% dirs; cannot run in Offline category.
- **Gap for Handoff:** Cross-Phase Gap #71 + #75 — Phase 1H deeper integration; Phase 5C must include Integration filter in CI.

### §16 — Item #15: Architecture v1.0.2 Update (Gap #63 Closure)
- **Resolution:** Updated docs/Architecture.v1.0.2-delta.md to bundle 4 changes (added §4 IOrchestrator subsection).
- **Gap for Handoff:** Owner sign-off required before Phase 5F (Gap #63).

---

## §17 Files Produced This Msg

| Path | Purpose | Marker(s) |
|---|---|---|
| `tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs` | 14 tests | `PHASE1F-MSG2` |
| `tests/TaskTree.Orchestrator.Tests/TaskTree.Orchestrator.Tests.csproj` | PATCHED 9 ProjectReferences | (csproj — no marker) |
| `docs/spec-derivations/PHASE1F-MSG2-DERIVATIONS-DELTA.md` | This registry | (md) |
| `docs/HANDOFF-v1.0.21-delta.md` | Additive delta | (md) |
| `docs/Architecture.v1.0.2-delta.md` | UPDATED — 4 bundled changes | (md) |
| `tools/find-spec-derivations.ps1` | UPDATED — 14 marker buckets | (script) |

---

## §18 Marker Inventory Updates

| Bucket | Old (Msg 1) | New (Msg 2) | Notes |
|---|---|---|---|
| `SPEC-DERIVED-PHASE1F` | 4 | **5** | OrchestratorTests.cs substring-matches PHASE1F |
| `SPEC-DERIVED-PHASE1F-MSG2` | — | **1** | NEW bucket: OrchestratorTests.cs |

`tools/find-spec-derivations.ps1` updated. Grand-total = **53** (bucket sum).

---

## §19 Cross-Phase Gaps Introduced (rows 71-75)

| # | Gap | Source | Target Phase | Action |
|---|---|---|---|---|
| 71 | CompositionRoot/SR get only smoke integration test (no unit tests) | HALT-Msg2 #2 | Phase 1H | Deeper integration tests for env-specific paths + DI graph edge cases |
| 72 | Moq Strict on IComplianceCore enforces Gap #66 (lifecycle audit only) at test-time | HALT-Msg2 #3 | Phase 5E | Codex: switch to Loose with Setup + Verify on new audit contract |
| 73 | Moq Loose on ITaskEngine — future drift detector | HALT-Msg2 #6 | Phase 1G / 2B | Switch to Strict when tray events route to engine actions |
| 74 | 5th FakeClock consumer triggers Gap #40 re-evaluation | HALT-Msg2 #7 | Phase 2A / 2B (6th consumer) | Promote to new TaskTree.TestSupport project + Architecture v1.0.2 §3.3 amendment |
| 75 | P1F-AC1 smoke test category = Integration (creates real %LOCALAPPDATA% dirs) | HALT-Msg2 #14 | Phase 5C | CI must include Integration filter; Offline filter excludes |

---

## §20 Patches to Msg 1 Production Code

**Zero patches required.** Msg 1 design was test-friendly from the start (clean ctor surface, observable side effects via mocks, no internal state requiring InternalsVisibleTo). Phase 1F Msg 2 is cleaner than Phase 1D Msg 2.

---

## §21 Test Category Usage

- **13 Offline tests** — ctor null guards, StartAsync paths, StopAsync paths, Dispose paths
- **1 Integration test** — CompositionRoot_BuildServiceProvider_ResolvesIOrchestratorSuccessfully (Gap #75)

Cumulative Offline CI cost: <100ms (mocks only). Integration test ~500ms (real DI container build + dir creation).

---

## §22 Coverage Targets (Informational)

| AC | Coverage |
|---|---|
| P1F-AC1 (container builds) | Integration test #14 |
| P1F-AC2 (E2E flow) | Tests #2 (HappyPath), #3 (NotImplementedException catch), #7 (idempotency), #10 (StartThenStop) |
| P1F-AC3 (audit chain receives entries) | Tests #2 (1 audit), #5/#6 (failure audits), #10 (2 audits exactly) |

**Estimated module coverage:** Orchestrator.cs ~90%; CompositionRoot.cs ~100% via integration; ServiceRegistrations.cs ~100% via integration. **Phase 1H gate (75%) met.**

---

## §23 Known Limitations

1. Test #9 (`StopAsync_CallsModulesInReverseOrder`) uses string-list callOrder via Callbacks — fragile to refactoring; if Phase 5E adds new shutdown steps, list assertions need update.
2. No test verifies behavior when MULTIPLE unsubscribe steps fail simultaneously — single-failure paths covered by per-step try/catch in production code.
3. Dispose live-DI teardown not verified — Phase 5D scope (carries Gap #52 pattern).
4. Integration test #14 creates real %LOCALAPPDATA%\TaskTree\{keys,store,logs} directories; these persist after test run (not cleaned up) — acceptable for Phase 5C runtime but should be cleaned in CI.
5. Test #4 (`StartAsync_TrayHostInitializeThrowsOtherException_Propagates`) uses InvalidOperationException as proxy for "any other exception type". If Phase 5E live code introduces a different exception with similar semantic, must verify catch is still narrow to NotImplementedException only.
6. FakeClock 5th consumer is intentional debt — Gap #74 defers actual Option B promotion to Phase 2A/2B (6th consumer trigger).
