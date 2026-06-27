# HANDOFF.md - v1.0.21 Additive Delta

**Predecessor:** v1.0.20 (Phase 1F Msg 1 delta)
**Date:** 2026-05-27
**Phase advance:** Phase 1F Msg 2 ✅ (Msg 3 tests pending)
**Author:** DSW (via owner batch approval of 7 Msg-2 HALT items)

ADDITIVE delta - merges on top of v1.0.15 + v1.0.16 + v1.0.17 + v1.0.18 + v1.0.19 + v1.0.20.

---

## §Document Identity - UPDATE

- Document Version: **1.0.21**
- State: **Phase 1F Msg 2 ✅** (Bootstrap complete); Msg 3 (OrchestratorTests) pending
- Architecture.md Version: **1.0.3** (no further amendments)
- Current Sub-Phase: Phase 1F - Msg 3 (tests) pending
- Total spec-derived items: **68 across 14 registries** (+4 PHASE1F Msg-2 derivations)
- LOAD-BEARING open questions: **3 active** (Q10, Q11, Q12) - Q11 still partial
- Closed cross-phase gaps this msg: **5 fully closed** + 1 still partial
- Progress: **26.67 of 35 -> 76.2%** (1F Msg 2 = 2/3 of 1F = 0.67)

---

## §Files Produced - APPEND (Phase 1F Msg 2)

| Phase.Item | FilePath | Status | Notes |
|---|---|---|---|
| P1.F.Msg2 | src/TaskTree.App/Bootstrap/ServiceRegistrations.cs | ✅ NEW | SPEC-DERIVED-PHASE1F §11, §13 |
| P1.F.Msg2 | src/TaskTree.App/Bootstrap/CompositionRoot.cs | ✅ NEW | SPEC-DERIVED-PHASE1F §12 |
| P1.F.Msg2 | src/TaskTree.App/App.xaml | ✅ NEW | OnExplicitShutdown, no StartupUri |
| P1.F.Msg2 | src/TaskTree.App/App.xaml.cs | ✅ NEW | OnStartup/OnExit wiring |
| P1.F.Msg2 | src/TaskTree.App/TaskTree.App.csproj | ✅ AMENDED | WinExe + WPF + 7 ProjectReferences + 2 PackageReferences |
| P1.F.Msg2 | docs/spec-derivations/PHASE1F-DERIVATIONS-MSG2.md | ✅ NEW | 4 derivations + 14 gaps + 5 closures |
| P1.F.Msg2 | tools/find-spec-derivations.ps1 | ✅ UPDATED | PHASE1F = 7 |

### Phase 1F Msg 3 - pending
| Phase.Item | FilePath | Status |
|---|---|---|
| P1.F.Msg3 | tests/TaskTree.Orchestrator.Tests/TaskTree.Orchestrator.Tests.csproj | ⬚ |
| P1.F.Msg3 | tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs | ⬚ |
| P1.F.Msg3 | tests/TaskTree.Orchestrator.Tests/ServiceRegistrationsTests.cs (maybe split) | ⬚ |

---

## §Decisions Log - APPEND R16

### R16 (2026-05-27) - Phase 1F Msg 2 - 7-item HALT batch, all owner-approved

1. HALT #1 Msg 2 -> Option B: EnsureDirectoriesExist BEFORE container build
2. HALT #2 Msg 2 -> Option A: try/catch in OnStartup; MessageBox + Application.Shutdown
3. HALT #3 Msg 2 -> Option A: CompositionRoot IAsyncDisposable; OnExit awaits
4. HALT #4 Msg 2 -> Approved 12-step top-down registration order per §3.2
5. HALT #5 Msg 2 -> Confirmed concrete impl names; G1F-H8..H12 flagged for Phase 5B
6. HALT #6 Msg 2 -> Option A combined: omit StartupUri + ShutdownMode=OnExplicitShutdown
7. HALT #7 Msg 2 -> 7 ProjectReferences + MS.Ext.DI 8.0.0 runtime + WPF flags

---

## §Open Questions - STATE

No new open questions. Q10, Q11, Q12 remain LOAD-BEARING. Q11 still partial (PhiRedactor wired with Array.Empty<string>()).

---

## §Visual Phase Tracker - UPDATE

```
PHASE 1 - CORE MVP [In Progress]
|-- 1A OK TaskEngine
|-- 1B OK SecureStore + MasterKeyManager + IMasterKeyManager
|-- 1C OK ComplianceCore baseline
|-- 1D OK ReminderScheduler + CadencePolicy + tests (41 tests)
|-- 1E OK TrayHost HIGH-stub + HotkeyInterop + 18 tests
|-- 1F PARTIAL Msg 1 ✅ + Msg 2 ✅ (Orchestrator + Bootstrap)
|       Msg 3 PENDING OrchestratorTests (P1F-AC1/AC2/AC3 verification)
|       [G1F-H1: MS.Ext.DI 8.0.0 - Phase 5A restore]
|       [G1F-H4: TaskEngine ctor IComplianceCore - Phase 5B]
|       [G1F-H8..H14: 7 concrete-name assumptions - Phase 5B]
|       [G1F-S4: §10.9 breach-response - Phase 2B UI]
|-- 1G PENDING Reminder Tier 1/2/3 [PlaceholderReminderDeliveryService is the swap point]
+-- 1H PENDING Phase 1 E2E gate
```

---

## §Consolidated Gap Summary - APPEND 14 NEW ROWS (G1F-H8..H14 + S10..S13 + E4..E6)

| # | Flag | Source | Target Phase | Action |
|---|---|---|---|---|
| 116 | G1F-H8 SystemClock concrete name assumed | PHASE1F-MSG2 N2 | 5B | Verify vs Phase 0 Msg 5 |
| 117 | G1F-H9 FileAppLogger ctor(logDir) assumed | PHASE1F-MSG2 N2 | 5B | Verify; adapt factory |
| 118 | G1F-H10 MasterKeyManager ctor order | PHASE1F-MSG2 N2 | 5B | Verify vs PHASE1B R8 |
| 119 | G1F-H11 SecureStore ctor order | PHASE1F-MSG2 N2 | 5B | Verify vs PHASE1B R8 |
| 120 | G1F-H12 ComplianceCore 5-dep ctor + inline PhiRedactor/AuditChainWriter | PHASE1F-MSG2 N2 | 5B | Verify; consider DI promotion |
| 121 | G1F-H13 Double-registration of TaskTreePaths | PHASE1F-MSG2 N1 | 5B | Apply N1 resolution (a/b/c) |
| 122 | G1F-H14 <UseWPF>true</UseWPF> on Phase 0 Msg 1 scaffold | PHASE1F-MSG2 N3 | 5A | Verify; ensure Application base resolves |
| 123 | G1F-S10 Composition failure routes only to MessageBox | PHASE1F-MSG2 | 3D | BugReporter capture wiring |
| 124 | G1F-S11 TaskTreePaths default %LOCALAPPDATA% non-overridable | PHASE1F-MSG2 | 5E | Override registration in integration |
| 125 | G1F-S12 MessageBox not exercised in offline tests | PHASE1F-MSG2 N4 | 5D | Live integration verifies |
| 126 | G1F-S13 No fault-injection seam in CompositionRoot.StartAsync | PHASE1F-MSG2 N5 | 5C | Add BuildProviderAsync extension point |
| 127 | G1F-E4 WPF Application STA confirmed at live runtime | PHASE1F-MSG2 | 5D | Live integration |
| 128 | G1F-E5 ShutdownMode=OnExplicitShutdown in tray-only | PHASE1F-MSG2 | 5D | Verify tray Exit triggers Application.Shutdown |
| 129 | G1F-E6 Application.Current.Dispatcher for UI marshaling | PHASE1F-MSG2 | 5D | Phase 1G ToastTier2Adapter uses it |

---

## §Build Verification - UPDATE expected marker counts

```
SPEC-DERIVED-MSG2     = 4
SPEC-DERIVED-MSG3     = 4
SPEC-DERIVED-MSG4     = 2
SPEC-DERIVED-MSG5     = 3
SPEC-DERIVED-MSG6     = 3
SPEC-DERIVED-PHASE1A  = 2
SPEC-DERIVED-PHASE1B  = 5
SPEC-DERIVED-PHASE1C  = 6
SPEC-DERIVED-PHASE1D  = 5
SPEC-DERIVED-PHASE1E  = 3
SPEC-DERIVED-PHASE1F  = 7   # UPDATED (was 5; +ServiceRegistrations.cs +CompositionRoot.cs)
# Total: 44 distinct .cs files
```

---

## §Continuation Prompt - UPDATE for next chat

> CURRENT STATE:
> - Phase 0 OK + Phase 1A OK + Phase 1B OK + Phase 1C OK + Phase 1D OK + Phase 1E OK + Phase 1F Msg 1 OK + Msg 2 OK
> - Phase 1F Msg 3 (OrchestratorTests + ServiceRegistrations container-build tests) pending - next action
> - 68 spec-derived items across 14 registries
> - 3 LOAD-BEARING open questions (Q10, Q11, Q12)
> - Progress: 26.67/35 -> 76.2%
> - Architecture.md is v1.0.3
>
> FIRST ACTION FOR NEXT SESSION: Phase 1F Msg 3 - HALT protocol for OrchestratorTests. Tests verify P1F-AC1 (container builds), P1F-AC2 (simulated E2E flow), P1F-AC3 (audit chain receives entries). Use Moq for module mocks where possible; use real ServiceCollection.BuildServiceProvider for container tests. InternalsVisibleTo grant from Msg 1 enables Orchestrator handler tests.

---

## §Document History - APPEND

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.21 | 2026-05-27 | DSW | Phase 1F Msg 2 ✅ - Bootstrap complete + 14 new G1F-* gaps + 5 fully-closed cross-phase flags |

---

## End of HANDOFF.v1.0.21-delta.md
