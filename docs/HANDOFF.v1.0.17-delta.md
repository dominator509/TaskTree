# HANDOFF.md — v1.0.17 Additive Delta

**Predecessor:** v1.0.16 (Phase 1D Msg 1 delta)
**Date:** 2026-05-27
**Phase advance:** Phase 1D COMPLETE (Msg 1 + Msg 2 delivered + verified)
**Author:** DSW

ADDITIVE delta — merges on top of v1.0.15 consolidated + v1.0.16.

---

## §Document Identity — UPDATE

- Document Version: **1.0.17**
- State: **Phase 1D COMPLETE**; Phase 1E pending
- Architecture.md Version: **1.0.2** (no further amendments this msg)
- Current Sub-Phase: Phase 1E — TrayHost (HIGH-stub) pending
- Total spec-derived items: **43 across 12 registries** (+1 from AssemblyInfo.cs §11)
- LOAD-BEARING open questions: **3 active** (Q10, Q11, Q12)
- Progress: **25 of 35 → 71.4%**

---

## §Files Produced — APPEND (Phase 1D Msg 2)

| Phase.Item | FilePath | Status | Notes |
|---|---|---|---|
| P1.D.Msg2 | tests/TaskTree.Modules.ReminderScheduler.Tests/TaskTree.Modules.ReminderScheduler.Tests.csproj | AMENDED | Adds 3 ProjectReferences |
| P1.D.Msg2 | tests/TaskTree.Modules.ReminderScheduler.Tests/CadencePolicyTests.cs | NEW | 16 tests |
| P1.D.Msg2 | tests/TaskTree.Modules.ReminderScheduler.Tests/ReminderSchedulerTests.cs | NEW | 25 tests |
| P1.D.Msg2 | src/TaskTree.Modules.ReminderScheduler/Properties/AssemblyInfo.cs | NEW | SPEC-DERIVED-PHASE1D §11 — closes G1D-20 |
| P1.D.Msg2 | docs/spec-derivations/PHASE1D-DERIVATIONS-MSG2.md | NEW | §11 + §12 + §13 + §14 + AC matrix |
| P1.D.Msg2 | tools/find-spec-derivations.ps1 | UPDATED | PHASE1D expected = 5 |

---

## §Decisions Log — APPEND R12

### R12 (2026-05-27) — Phase 1D Msg 2 — 5 test-side derivations + 15 test gaps

1. AssemblyInfo.cs with InternalsVisibleTo (closes G1D-20) — added as SPEC-DERIVED-PHASE1D §11
2. Test csproj amends Phase 0 Msg 6 scaffold with 3 ProjectReferences (TestSupport + production + Core)
3. 16 CadencePolicy tests covering §5.3 verbatim across all 5 priorities x 3 cadence values
4. 25 ReminderScheduler tests including: 10 ctor gates, 2 lifecycle, 9 EvaluateOnceAsync behaviors, 2 reflection-based AC4 enforcement, 2 disposal
5. 15 test-side gaps (TG1D-1..TG1D-15) logged for Phase 5 handoff with explicit Phase 5A/5B/5C routing

---

## §Open Questions — STATE

No new open questions raised in Msg 2. Q10, Q11, Q12 remain LOAD-BEARING.

---

## §Visual Phase Tracker — UPDATE

```
PHASE 1 — CORE MVP [In Progress]
├── 1A OK TaskEngine
├── 1B OK SecureStore + MasterKeyManager + IMasterKeyManager
├── 1C OK ComplianceCore baseline
├── 1D OK ReminderScheduler + CadencePolicy + tests (41 tests; 5 SPEC-DERIVED .cs files)
├── 1E PENDING TrayHost (HIGH-stub for Codex 5E)
├── 1F PENDING Orchestrator wiring
│       [G1D-12: UI must Dispatcher.Invoke on ReminderDue]
│       [G1D-14: wire AutoLogoffTriggered → ReminderScheduler.StopAsync]
│       [G1D-18: wire TaskDeleted → ReminderScheduler purge]
│       [#2: inject IComplianceCore into TaskEngine]
│       [#3: register MasterKeyManager as singleton IMasterKeyManager]
│       [#6: populate Q11 allowlist or accept empty default]
├── 1G PENDING Reminder Tier 1/2/3 [consumes ReminderReason]
└── 1H PENDING Phase 1 E2E gate
```

---

## §Consolidated Gap Summary — APPEND 15 NEW ROWS (TG1D-1..TG1D-15)

| # | Flag | Source | Target Phase | Action |
|---|---|---|---|---|
| 54 | TG1D-1 TaskStatus.Active default | PHASE1D-MSG2 §14 | 5B | Verify vs enum actual |
| 55 | TG1D-2 TaskNode init shape | PHASE1D-MSG2 §14 | 5B | Diff vs Phase 0 Msg 3 |
| 56 | TG1D-3 GetTreeAsync return type | PHASE1D-MSG2 §14 | 5B | Verify Moq setup |
| 57 | TG1D-4 AuditAsync signature | PHASE1D-MSG2 §14 | 5B | Verify mock |
| 58 | TG1D-5 IAppLogger duck-typed mock | PHASE1D-MSG2 §14 | 5B | Adapt mock |
| 59 | TG1D-6 Moq 4.20.72 version | PHASE1D-MSG2 §14 | 5A | Align packages |
| 60 | TG1D-7 MSTest 3.6.1 version | PHASE1D-MSG2 §14 | 5A | Align packages |
| 61 | TG1D-8 Test csproj NEW ProjectReferences | PHASE1D-MSG2 §14 | 5A | Verify sln + supersedes |
| 62 | TG1D-9 FakeClock namespace dep | PHASE1D-MSG2 §14 | 5A | Verify Msg 1 intact |
| 63 | TG1D-10 Reflection-based AC4 enforcement | PHASE1D-MSG2 §14 | by-design | Update at Phase 2G |
| 64 | TG1D-11 AuditEntry init shape | PHASE1D-MSG2 §14 | 5B | Verify field shape |
| 65 | TG1D-12 ReminderEvent.Reason dep | PHASE1D-MSG2 §14 | 5A | Verify amendment |
| 66 | TG1D-13 Priority enum names | PHASE1D-MSG2 §14 | 5B | Same as N2 |
| 67 | TG1D-14 IClock.UtcNow name | PHASE1D-MSG2 §14 | 5B | Same as N5 |
| 68 | TG1D-15 No SyncContext capture | PHASE1D-MSG2 §14 | 5C | Verify at 1F integration |

---

## §Build Verification — UPDATE expected marker counts

```
SPEC-DERIVED-MSG2     = 4
SPEC-DERIVED-MSG3     = 4
SPEC-DERIVED-MSG4     = 2
SPEC-DERIVED-MSG5     = 3
SPEC-DERIVED-MSG6     = 3
SPEC-DERIVED-PHASE1A  = 2
SPEC-DERIVED-PHASE1B  = 5
SPEC-DERIVED-PHASE1C  = 6
SPEC-DERIVED-PHASE1D  = 5   # UPDATED (was 4; now includes AssemblyInfo.cs)
# Total: 34 distinct .cs files
```

---

## §Continuation Prompt — UPDATE for next chat

> CURRENT STATE:
> - Phase 0 OK + Phase 1A OK + Phase 1B OK + Phase 1C OK + Phase 1D OK COMPLETE
> - Phase 1E (TrayHost HIGH-stub) pending — next action
> - 43 spec-derived items across 12 registries
> - 3 LOAD-BEARING open questions (Q10, Q11, Q12) — Phase 5F blockers
> - Progress: 25/35 → 71.4%
> - Architecture.md is v1.0.2
>
> FIRST ACTION FOR NEXT SESSION: Phase 1E Msg 1 — TrayHost HIGH-stub per Roadmap Sub-Phase 1E. Initialize() throws NotImplementedException("HIGH: NotifyIcon + RegisterHotKey require live env — Codex Phase 5E"). Events declared and manually raisable. Apply HALT protocol for any silence.

---

## §Document History — APPEND

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.17 | 2026-05-27 | DSW | Phase 1D COMPLETE — 41 tests + InternalsVisibleTo + 15 TG1D-* gaps logged |

---

## End of HANDOFF.v1.0.17-delta.md
