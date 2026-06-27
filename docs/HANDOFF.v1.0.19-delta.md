# HANDOFF.md - v1.0.19 Additive Delta

**Predecessor:** v1.0.18 (Phase 1E Msg 1 delta)
**Date:** 2026-05-27
**Phase advance:** Phase 1E COMPLETE (Msg 1 + Msg 2 delivered)
**Author:** DSW

ADDITIVE delta - merges on top of v1.0.15 consolidated + v1.0.16 + v1.0.17 + v1.0.18.

---

## §Document Identity - UPDATE

- Document Version: **1.0.19**
- State: **Phase 1E COMPLETE**; Phase 1F pending
- Architecture.md Version: **1.0.2** (no further amendments this msg)
- Current Sub-Phase: Phase 1F - Orchestrator wiring (next)
- Total spec-derived items: **54 across 13 registries** (Msg 2 added no new SPEC-DERIVED markers; test files only)
- LOAD-BEARING open questions: **3 active** (Q10, Q11, Q12)
- Progress: **26 of 35 -> 74.3%**

---

## §Files Produced - APPEND (Phase 1E Msg 2)

| Phase.Item | FilePath | Status | Notes |
|---|---|---|---|
| P1.E.Msg2 | tests/TaskTree.Modules.TrayHost.Tests/TaskTree.Modules.TrayHost.Tests.csproj | AMENDED | 2 NEW ProjectReferences |
| P1.E.Msg2 | tests/TaskTree.Modules.TrayHost.Tests/TrayHostTests.cs | NEW | 18 tests |
| P1.E.Msg2 | docs/spec-derivations/PHASE1E-DERIVATIONS-MSG2.md | NEW | §12+§13+§14 + AC matrix + 8 TG1E gaps |

---

## §Decisions Log - APPEND R14

### R14 (2026-05-27) - Phase 1E Msg 2 - 18 tests authored + 8 TG1E-* test gaps

1. Test csproj AMENDS Phase 0 Msg 6 scaffold with 2 ProjectReferences (Core + production); no TestSupport ref (FakeClock not needed)
2. 18 tests organized: 1 ctor gate, 3 event-raise verifications, 3 no-subscriber safety, 1 Initialize throw, 3 ShowBalloon (1 throw + 2 null-guards), 2 Dispose lifecycle, 3 post-dispose Raise gates, 1 Initialize-after-dispose, 1 interface conformance via reflection
3. Pattern reused: internal Raise() seams (from G1D-20 / PHASE1D §11 precedent); reflection-based negative-surface gates (from PHASE1D scheduler tests 22/23)
4. 8 test-side gaps (TG1E-1..TG1E-8) logged for Phase 5 handoff with explicit Phase 5A/5B/5C/5E routing
5. By-design exclusions documented: no live PInvoke, no H.NotifyIcon.Wpf API, no STA thread fixture (all Phase 5E concerns)

---

## §Open Questions - STATE

No new open questions in Msg 2. Q10, Q11, Q12 remain LOAD-BEARING.

---

## §Visual Phase Tracker - UPDATE

```
PHASE 1 - CORE MVP [In Progress]
|-- 1A OK TaskEngine
|-- 1B OK SecureStore + MasterKeyManager + IMasterKeyManager
|-- 1C OK ComplianceCore baseline
|-- 1D OK ReminderScheduler + CadencePolicy + tests (41 tests)
|-- 1E OK TrayHost HIGH-stub + HotkeyInterop + AssemblyInfo + 18 tests
|-- 1F PENDING Orchestrator wiring
|       [G1D-12: Dispatcher.Invoke on ReminderDue]
|       [G1D-14: AutoLogoffTriggered -> ReminderScheduler.StopAsync]
|       [G1D-18: TaskDeleted -> ReminderScheduler purge]
|       [G1E-S1: STA thread for TrayHost.Initialize]
|       [G1E-S4: TrayHost events marshal as needed]
|       [#2: inject IComplianceCore into TaskEngine]
|       [#3: register MasterKeyManager as singleton IMasterKeyManager]
|       [#6: populate Q11 allowlist or accept empty default]
|-- 1G PENDING Reminder Tier 1/2/3 [consumes ReminderReason; G1E-S3 Focus Assist]
+-- 1H PENDING Phase 1 E2E gate
```

---

## §Consolidated Gap Summary - APPEND 8 NEW ROWS (TG1E-1..TG1E-8)

| # | Flag | Source | Target Phase | Action |
|---|---|---|---|---|
| 89 | TG1E-1 IAppLogger Moq shape assumed | PHASE1E-MSG2 §14 | 5B | Verify + adapt mock |
| 90 | TG1E-2 Test csproj NEW ProjectReferences | PHASE1E-MSG2 §14 | 5A | Verify supersedes scaffold |
| 91 | TG1E-3 Interface conformance reflection assumes §4.1 stable | PHASE1E-MSG2 §14 | 5B | Update test if §4.1 evolves |
| 92 | TG1E-4 Moq 4.20.72 + MSTest 3.6.1 versions | PHASE1E-MSG2 §14 | 5A | Align packages |
| 93 | TG1E-5 HotkeyInterop internal access via InternalsVisibleTo | PHASE1E-MSG2 §14 | 5A | Verify grant included |
| 94 | TG1E-6 No live PInvoke in tests | PHASE1E-MSG2 §14 | by-design | Codex 5E adds Live category |
| 95 | TG1E-7 No H.NotifyIcon.Wpf API in tests | PHASE1E-MSG2 §14 | by-design | Codex 5E adds Live category |
| 96 | TG1E-8 No STA thread fixture | PHASE1E-MSG2 §14 | 5C/5D | Codex Phase 5D integration verifies |

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
# Total: 37 distinct .cs files (test files do not carry markers)
```

---

## §Continuation Prompt - UPDATE for next chat

> CURRENT STATE:
> - Phase 0 OK + Phase 1A OK + Phase 1B OK + Phase 1C OK + Phase 1D OK + Phase 1E OK COMPLETE
> - Phase 1F (Orchestrator + CompositionRoot + ServiceRegistrations) pending - next action
> - 54 spec-derived items across 13 registries (+ 23 TG-* test gaps tracked)
> - 3 LOAD-BEARING open questions (Q10, Q11, Q12) - Phase 5F blockers
> - Progress: 26/35 -> 74.3%
> - Architecture.md is v1.0.2
>
> FIRST ACTION FOR NEXT SESSION: Phase 1F Msg 1 - HALT protocol for Orchestrator/CompositionRoot/ServiceRegistrations. CRITICAL wiring flags MUST be addressed:
>   - #2 (cross-phase): inject IComplianceCore into TaskEngine ctor
>   - #3 (cross-phase): register MasterKeyManager as singleton IMasterKeyManager
>   - #6 (cross-phase): populate Q11 allowlist or accept empty default
>   - G1D-12 / G1D-14 / G1D-18 (Phase 1D wiring)
>   - G1E-S1 / G1E-S4 (Phase 1E STA thread + marshaling)
>   - Canonical paths from HANDOFF Architecture Context Snapshot

---

## §Document History - APPEND

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.19 | 2026-05-27 | DSW | Phase 1E COMPLETE - 18 TrayHost tests + 8 TG1E-* gaps logged + Phase 1F now blocking |

---

## End of HANDOFF.v1.0.19-delta.md
