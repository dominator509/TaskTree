# HANDOFF.md — v1.0.16 Additive Delta

**Predecessor:** v1.0.15 (consolidated flat merge)
**Date:** 2026-05-27
**Phase advance:** Phase 1D Msg 1 ✅ (Msg 2 pending — tests)
**Author:** DSW (via owner batch approval of 11 HALT items)

ADDITIVE delta — the consolidated v1.0.15 remains valid; merge this on top.

---

## §Document Identity — UPDATE
- Document Version: **1.0.16**
- State: Phase 1D **Msg 1 ✅** (production code complete + FakeClock promoted); Msg 2 awaiting
- Architecture.md Version: **1.0.2** (amended — TaskTree.TestSupport + ReminderEvent.Reason)
- Current Sub-Phase: Phase 1D — Msg 2 (tests) pending
- Total spec-derived items: **42 across 12 registries** (was 38 / 11)
- LOAD-BEARING open questions: **3 active** (Q10, Q11, NEW Q12)
- Progress: **24 of 35 → 68.6%** (1D Msg 1 = half of 1D = 0.5 of the deliverable)

---

## §Files Produced — APPEND

### Phase 1D Msg 1 (all ✅ this delta)
| Phase.Item | FilePath | Status | Notes |
|---|---|---|---|
| P1.D.Msg1 | `src/TaskTree.Modules.ReminderScheduler/CadencePolicy.cs` | ✅ | SPEC-DERIVED-PHASE1D §4 |
| P1.D.Msg1 | `src/TaskTree.Modules.ReminderScheduler/ReminderScheduler.cs` | ✅ | SPEC-DERIVED-PHASE1D §5–§10 |
| P1.D.Msg1 | `src/TaskTree.Core/Enums/ReminderReason.cs` | ✅ NEW | SPEC-DERIVED-PHASE1D §3 (Arch v1.0.2) |
| P1.D.Msg1 | `src/TaskTree.Core/Models/ReminderEvent.cs` | ✅ AMENDED | Supersedes v1.0.0; Arch v1.0.2 |
| P1.D.Msg1 | `tests/TaskTree.TestSupport/TaskTree.TestSupport.csproj` | ✅ NEW | SPEC-DERIVED-PHASE1D §2 (Arch v1.0.2 §3.3) |
| P1.D.Msg1 | `tests/TaskTree.TestSupport/Clocks/FakeClock.cs` | ✅ PROMOTED | SPEC-DERIVED-PHASE1D §1 |
| P1.D.Msg1 | `docs/spec-derivations/PHASE1D-DERIVATIONS.md` | ✅ NEW | 10 derivations + 22 gaps + Q12 |
| P1.D.Msg1 | `docs/Architecture.v1.0.2-delta.md` | ✅ NEW | Two amendments |
| P1.D.Msg1 | `tools/find-spec-derivations.ps1` | ✅ UPDATED | Adds SPEC-DERIVED-PHASE1D = 4 |

### Phase 1D Msg 2 — pending
| Phase.Item | FilePath | Status |
|---|---|---|
| P1.D.Msg2 | `tests/TaskTree.Modules.ReminderScheduler.Tests/ReminderSchedulerTests.cs` | ⬚ |
| P1.D.Msg2 | `tests/TaskTree.Modules.ReminderScheduler.Tests/CadencePolicyTests.cs` | ⬚ |

---

## §Decisions Log — APPEND R11

### R11 (2026-05-27) — Phase 1D Msg 1 — 11-item HALT batch, all owner-approved
1. HALT #1 → **Option B**: new `TaskTree.TestSupport` project (Arch v1.0.2 §3.3)
2. HALT #2 → **Option A**: `Cadence` = master tick interval; per-priority via `CadencePolicy`
3. HALT #3 → **Option B + Arch v1.0.2**: `ReminderReason` enum + `ReminderEvent.Reason`
4. HALT #4 → **Hybrid**: `DefaultTickInterval = 30 s`, settable via `Cadence`
5. HALT #5 → **Approved as proposed**: 5-method static `CadencePolicy` + sentinel
6. HALT #6 → **Approved as proposed**: 4-dep ctor + optional `tickInterval`
7. HALT #7 → **Option A**: in-memory `lastFiredAt`; raises NEW Q12 for Phase 2G uplift
8. HALT #8 → **Option A**: synchronous raise on tick context; Orchestrator marshals to UI
9. HALT #9 → **Approved as proposed**: 3-value `ReminderReason` enum
10. HALT #10 → **Confirmed**: re-use Phase 0 Msg 6 csproj
11. HALT #11 → **Option A**: escalation values as constants only; no stubs

---

## §Open Questions — APPEND Q12 (LOAD-BEARING)

- **Q12 (Phase 2G or 5F blocker):** Should `ReminderScheduler._lastFiredAt` persist across app restarts? Phase 1D ships in-memory only per HALT #7A. Risk: after process restart, a Priority 1 task whose initial reminder already fired will fire **again** as if first-time.
  - (A) Persist to SecureStore key `"reminders/state"` — full restart fidelity; additive ISecureStore ctor dep
  - (B) Owner explicitly accepts per-restart double-fire for P1 tasks (HIPAA-acceptable; not a PHI risk)
  - **Owner decision needed by Phase 2G design.**

---

## §Visual Phase Tracker — UPDATE

```
PHASE 1 — CORE MVP [In Progress]
├── 1A ✅ TaskEngine
├── 1B ✅ SecureStore + MasterKeyManager + IMasterKeyManager
├── 1C ✅ ComplianceCore baseline (Q10+Q11 LOAD-BEARING)
├── 1D ⏳ Msg 1 ✅ (ReminderScheduler+CadencePolicy+FakeClock promotion); Msg 2 ⬚ tests
├── 1E ⬚ TrayHost
├── 1F ⬚ Orchestrator wiring
│       [CRITICAL: wire ITaskEngine.TaskDeleted → ReminderScheduler purge (G1D-18)]
│       [CRITICAL: wire AutoLogoffTriggered → ReminderScheduler.StopAsync (G1D-14)]
│       [CRITICAL: UI subscribers MUST marshal to Dispatcher (G1D-12)]
├── 1G ⬚ Reminder Tier 1/2/3 [now consumes ReminderReason for decision routing]
└── 1H ⬚ Phase 1 E2E gate
```

---

## §Consolidated Gap Summary — APPEND 22 NEW ROWS (G1D-1..G1D-22)

| # | Flag | Source | Target Phase | Action |
|---|---|---|---|---|
| 32 | G1D-1 Priority enum names not verified | PHASE1D N2 | 5B | Diff vs Phase 0 Msg 4; rename per D2 |
| 33 | G1D-2 TaskStatus.Done member name not verified | PHASE1D §IsEligible | 5B | Confirm vs `Completed` candidate |
| 34 | G1D-3 TaskNode.Deadline may be nullable | PHASE1D N3 | 5B | Add HasValue guard |
| 35 | G1D-4 AuditEntry may require Actor at ctor | PHASE1D N4 | 5B | Inject Windows SID |
| 36 | G1D-5 IClock property name may be `Now` | PHASE1D N5 | 5B | Rename per D2 |
| 37 | G1D-6 IAppLogger method shape may differ | PHASE1D N6 | 5B | Adapt calls |
| 38 | G1D-7 ReminderScheduler csproj package refs | PHASE1D | 5A | Verify |
| 39 | G1D-8 TaskTree.sln must include TestSupport | PHASE1D HALT #1B | 5A | `dotnet sln add` |
| 40 | G1D-9 Delete 3 inline FakeClock private nested classes | PHASE1D HALT #1B | 5A | Mechanical delete + ProjectReference add |
| 41 | G1D-10 ReminderEvent.cs supersedes duplicate | PHASE1D | 5A | Resolve via newer file |
| 42 | G1D-11 lastFiredAt not persisted (Q12) | PHASE1D §8 | 2G | Owner decision |
| 43 | G1D-12 ReminderDue on threadpool — UI must marshal | PHASE1D §9 | 1F + 1G | Dispatcher.Invoke wiring |
| 44 | G1D-13 Cadence setter has no observability | PHASE1D | 2E | Add CadenceChanged event |
| 45 | G1D-14 Scheduler not paused on auto-logoff | PHASE1D | 2F | Wire StopAsync on AutoLogoffTriggered |
| 46 | G1D-15 Tree fetch every tick — perf budget | PHASE1D §15 | 4B | Benchmark; optional cache |
| 47 | G1D-16 Audit failure does not block raise | PHASE1D §10.5 | 4A | Compliance re-verify |
| 48 | G1D-17 OverdueEscalation classified but not acted upon | PHASE1D | 2G | EscalationPolicy branches |
| 49 | G1D-18 No purge of lastFiredAt on TaskDeleted | PHASE1D §8 | 1F or 2G | Wire TaskDeleted → purge |
| 50 | G1D-19 IsEligibleForReminder only filters Done | PHASE1D | 2C/2E | Feature wiring |
| 51 | G1D-20 EvaluateOnceAsync internal — needs InternalsVisibleTo | PHASE1D §test design | 5B | Add assembly attr |
| 52 | G1D-21 PeriodicTimer behavior under sleep/resume unverified | PHASE1D | 5E | Live test |
| 53 | G1D-22 Tier 1/2/3 routing not exercised live | PHASE1D | 5E | Live integration |

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
SPEC-DERIVED-PHASE1D  = 4   # NEW
# Total: 33 distinct .cs files
```

---

## §Continuation Prompt — UPDATE for next chat

Replace the CURRENT STATE block in HANDOFF.md continuation prompt with:

> CURRENT STATE:
> - Phase 0 ✅ + Phase 1A ✅ + Phase 1B ✅ + Phase 1C ✅ + Phase 1D Msg 1 ✅
> - Phase 1D Msg 2 (tests) pending
> - 42 spec-derived items across 12 registries
> - 3 LOAD-BEARING open questions (Q10, Q11, Q12) — Phase 5F blockers (Q12 → Phase 2G design)
> - Progress: 24/35 → 68.6%
> - Architecture.md is now v1.0.2 (TestSupport + ReminderEvent.Reason)
>
> FIRST ACTION FOR NEXT SESSION: Phase 1D Msg 2 — produce ReminderSchedulerTests.cs + CadencePolicyTests.cs against PHASE1D-DERIVATIONS.md §Acceptance Criteria Wire-Up.

---

## §Document History — APPEND

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.16 | 2026-05-27 | DSW | Phase 1D Msg 1 ✅ — ReminderScheduler + CadencePolicy + Arch v1.0.2 amendment + FakeClock promotion + 22 new gaps + Q12 raised |

---

## End of HANDOFF.v1.0.16-delta.md
