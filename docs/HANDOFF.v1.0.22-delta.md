# HANDOFF.md - v1.0.22 Additive Delta

**Predecessor:** v1.0.21 (Phase 1F Msg 2 delta)
**Date:** 2026-05-27
**Phase advance:** Phase 1F COMPLETE (Msg 1 + Msg 2 + Msg 3 delivered)
**Author:** DSW

ADDITIVE delta - merges on top of v1.0.15 + v1.0.16 + v1.0.17 + v1.0.18 + v1.0.19 + v1.0.20 + v1.0.21.

---

## §Document Identity - UPDATE

- Document Version: **1.0.22**
- State: **Phase 1F COMPLETE**; Phase 1G next blocking
- Architecture.md Version: **1.0.3** (no further amendments)
- Current Sub-Phase: Phase 1G - Reminder Tier 1/2/3 (next)
- Total spec-derived items: **68 across 14 registries** (no new SPEC-DERIVED markers; test files only)
- LOAD-BEARING open questions: **3 active** (Q10, Q11, Q12)
- Progress: **27 of 35 → 77.1%**

---

## §Files Produced - APPEND (Phase 1F Msg 3)

| Phase.Item | FilePath | Status | Notes |
|---|---|---|---|
| P1.F.Msg3 | tests/TaskTree.Orchestrator.Tests/TaskTree.Orchestrator.Tests.csproj | AMENDED | 9 ProjectReferences + 5 PackageReferences |
| P1.F.Msg3 | tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs | NEW | 19 tests (8 ctor + 11 behavioral) |
| P1.F.Msg3 | tests/TaskTree.Orchestrator.Tests/ServiceRegistrationsTests.cs | NEW | 6 tests (hybrid real-DI + temp dir) |
| P1.F.Msg3 | docs/spec-derivations/PHASE1F-DERIVATIONS-MSG3.md | NEW | §15+§16+§17 + 11 TG1F gaps + closure summary |

---

## §Decisions Log - APPEND R17

### R17 (2026-05-27) - Phase 1F Msg 3 - 7-item HALT batch + 25 tests + 11 TG1F-* gaps

1. HALT #1 Msg 3 → Option B: split into OrchestratorTests + ServiceRegistrationsTests
2. HALT #2 Msg 3 → Option C: Hybrid real-DI + per-test temp directory
3. HALT #3 Msg 3 → Direct internal handler invocation via InternalsVisibleTo
4. HALT #4 Msg 3 → Option C: handler routing (Moq) + smoke test (real container)
5. HALT #5 Msg 3 → Option B for unit (Callback capture) + Verify for routing
6. HALT #6 Msg 3 → Option B (Moq) for unit + Option A acknowledged for future smoke
7. HALT #7 Msg 3 → 25 tests (6 ServiceRegistrations + 19 Orchestrator)

---

## §Open Questions - STATE

No new open questions. Q10, Q11, Q12 remain LOAD-BEARING (Q11 still partial via Array.Empty<string>()).

---

## §Visual Phase Tracker - UPDATE

```
PHASE 1 - CORE MVP [In Progress]
|-- 1A OK TaskEngine
|-- 1B OK SecureStore + MasterKeyManager + IMasterKeyManager
|-- 1C OK ComplianceCore baseline
|-- 1D OK ReminderScheduler + CadencePolicy + tests (41 tests)
|-- 1E OK TrayHost HIGH-stub + HotkeyInterop + 18 tests
|-- 1F OK Orchestrator + Bootstrap + 25 tests (Phase 1F COMPLETE)
|       [G1F-H1..H14: 14 hard gaps deferred to Phase 5A/5B]
|       [G1F-S1..S13: 13 soft gaps distributed across 2B/3D/5C/5D/5E]
|       [TG1F-1..TG1F-11: 11 test gaps for Phase 5]
|       [Closed: Cross-phase #2/#3/#4/#5, G1E-S1, G1E-S4, G1D-12, G1F-S5, G1F-S6]
|       [Partial: Cross-phase #6 Q11 allowlist - LOAD-BEARING for 5F]
|-- 1G PENDING Reminder Tier 1/2/3 [PlaceholderReminderDeliveryService swap point]
+-- 1H PENDING Phase 1 E2E gate
```

---

## §Consolidated Gap Summary - APPEND 11 NEW ROWS (TG1F-1..TG1F-11)

| # | Flag | Source | Target Phase | Action |
|---|---|---|---|---|
| 130 | TG1F-1 TaskEngine concrete ctor in container tests | PHASE1F-MSG3 §17 | 5B | Verify vs PHASE1A actual |
| 131 | TG1F-2 ReminderScheduler concrete ctor | PHASE1F-MSG3 §17 | 5B | Verify vs PHASE1D §7 |
| 132 | TG1F-3 ComplianceCore 5-dep ctor + inline factories | PHASE1F-MSG3 §17 | 5B | Verify vs PHASE1C R10 |
| 133 | TG1F-4 Temp directory cleanup robustness | PHASE1F-MSG3 §17 | 5C | Best-effort; investigate flakes |
| 134 | TG1F-5 DPAPI Windows + same user profile | PHASE1F-MSG3 §17 | 5C | Document non-Windows skip |
| 135 | TG1F-6 SystemClock instantiation (propagates G1F-H8) | PHASE1F-MSG3 §17 | 5B | Verify SystemClock concrete |
| 136 | TG1F-7 FileAppLogger ctor (propagates G1F-H9) | PHASE1F-MSG3 §17 | 5B | Verify ctor signature |
| 137 | TG1F-8 Mock.Raise threading semantics | PHASE1F-MSG3 §17 | 5C | Real scheduler raises on threadpool |
| 138 | TG1F-9 Mock.Verify Loose Callback capture | PHASE1F-MSG3 §17 | 5C | Pattern works; consider explicit Verify |
| 139 | TG1F-10 EventHandler<ReminderEvent> raise syntax | PHASE1F-MSG3 §17 | 5B | If shape differs, breaks |
| 140 | TG1F-11 Placeholder internal type-name skip | PHASE1F-MSG3 §17 | 5C/1G | If Phase 1G replacement breaks contract |

### Gaps CLOSED this msg

| Prior gap | Closure |
|---|---|
| G1F-S5 (Re-entrant StartAsync) | ✅ CLOSED - Orch test 10 |
| G1F-S6 (TrayHost headless) | ✅ CLOSED - Orch test 11 |

### Phase 1F cumulative closure status

| Cross-phase flag | Status |
|---|---|
| #2 IComplianceCore → TaskEngine | ✅ FULL |
| #3 IMasterKeyManager singleton | ✅ FULL |
| #4 Canonical key/store paths | ✅ FULL |
| #5 Canonical log directory | ✅ FULL |
| #6 Q11 allowlist | ⚠️ PARTIAL (LOAD-BEARING for 5F) |
| G1E-S1 STA thread | ✅ FULL |
| G1E-S4 Click marshaling docs | ✅ FULL |
| G1D-12 Dispatcher marshaling docs | ✅ FULL |
| G1D-14 AutoLogoffTriggered wiring | ⚠️ DEFERRED to 2F |
| G1D-18 TaskDeleted purge wiring | ⚠️ DEFERRED to 2C/2G |

---

## §Build Verification - marker counts unchanged

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
SPEC-DERIVED-PHASE1F  = 7
# Total: 44 distinct .cs files
```

---

## §Continuation Prompt - UPDATE for next chat

> CURRENT STATE:
> - Phase 0 OK + Phase 1A OK + Phase 1B OK + Phase 1C OK + Phase 1D OK + Phase 1E OK + Phase 1F OK COMPLETE
> - Phase 1G (Reminder Tier 1/2/3) pending - next action
> - 68 spec-derived items across 14 registries
> - Cumulative gap ledger: 140 rows tracked across all phases
> - 3 LOAD-BEARING open questions (Q10, Q11, Q12)
> - Progress: 27/35 → 77.1%
> - Architecture.md is v1.0.3
>
> FIRST ACTION FOR NEXT SESSION: Phase 1G Msg 1 - HALT protocol for ReminderDeliveryService router + Tier 1/2/3 adapters. Roadmap 1G deliverables:
>   - src/TaskTree.Orchestrator/ReminderDeliveryService.cs (router; replaces PlaceholderReminderDeliveryService)
>   - src/TaskTree.Orchestrator/ToastTier1Adapter.cs (HIGH-stub Windows Toast API)
>   - src/TaskTree.Orchestrator/ToastTier2Adapter.cs (WPF custom toast)
>   - src/TaskTree.Orchestrator/ToastTier3Adapter.cs (HIGH-stub NotifyIcon balloon)
>   - tests/TaskTree.Orchestrator.Tests/ReminderDeliveryTests.cs
>
> Architecture §7 decision tree drives the routing. Phase 5E lights up live Toast API + balloon. Expect 8-10 HALT items.

---

## §Document History - APPEND

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.22 | 2026-05-27 | DSW | Phase 1F COMPLETE - 25 tests + 11 TG1F-* gaps + 9 closures (2 in Msg 3) |

---

## End of HANDOFF.v1.0.22-delta.md
