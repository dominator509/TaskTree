# PHASE1D-DERIVATIONS-MSG2.md

**Phase:** 1D — ReminderScheduler + CadencePolicy
**Msg:** 2 of 2 (tests) — Phase 1D COMPLETE upon this delivery
**Source authority:** Architecture.md v1.0.2 §4.3, §5.3; Roadmap.md v1.0.0 P1D-AC1..AC4
**Predecessor:** PHASE1D-DERIVATIONS.md (Msg 1)
**Marker:** SPEC-DERIVED-PHASE1D (now 5 .cs files — added AssemblyInfo.cs)

---

## §11 — InternalsVisibleTo (CLOSES G1D-20)

`src/TaskTree.Modules.ReminderScheduler/Properties/AssemblyInfo.cs` declares:

`[assembly: InternalsVisibleTo("TaskTree.Modules.ReminderScheduler.Tests")]`

This grants the test assembly access to `EvaluateOnceAsync` (internal in production)
without changing its public surface. Per D2, the public `IReminderScheduler` surface
is untouched. Per D9, the module remains testable via its public interface; the
internal helper is exposed only for deterministic single-tick simulation.

**Phase 5B verification:** confirm AssemblyInfo.cs is included in the compilation
(default behavior for net8.0; no csproj change required).

---

## §12 — CadencePolicy Tests (16 tests)

All `[TestCategory("Offline")]`. Pure-static SUT; no Moq, no FakeClock instance.

| # | Test | AC |
|---|---|---|
| 1 | InitialOffset_Critical_ReturnsSentinel | P1D-AC2 |
| 2 | InitialOffset_AllPriorities_MatchesSection53 (DataRow x4) | P1D-AC2 |
| 3 | RepeatInterval_AllPriorities_MatchesSection53 (DataRow x5) | P1D-AC2 |
| 4 | EscalationThreshold_AllPriorities_MatchesSection53 | P1D-AC2 |
| 5 | ShouldFire_CriticalPriority_NeverFired_ReturnsTrue | P1D-AC2 |
| 6 | ShouldFire_NonCriticalPriority_BeforeInitialOffset_ReturnsFalse | P1D-AC2 |
| 7 | ShouldFire_NonCriticalPriority_AtInitialOffset_ReturnsTrue | P1D-AC2 |
| 8 | ShouldFire_AnyPriority_LastFiredRecently_ReturnsFalse | P1D-AC2 |
| 9 | ShouldFire_AnyPriority_RepeatIntervalElapsed_ReturnsTrue | P1D-AC2 |
| 10 | ClassifyReason_NeverFired_ReturnsInitialReminder | P1D-AC3 |
| 11 | ClassifyReason_RecentFire_ReturnsRepeatCadence | P1D-AC3 |
| 12 | ClassifyReason_OverdueBeyondEscalation_ReturnsOverdueEscalation | P1D-AC3 |
| 13 | ClassifyReason_TrivialPriority_NeverEscalates | P1D-AC2 + §5.3 footnote |
| 14 | InitialOffset_UnknownPriorityCast_Throws | defensive |
| 15 | RepeatInterval_UnknownPriorityCast_Throws | defensive |
| 16 | EscalationThreshold_UnknownPriorityCast_Throws | defensive |

---

## §13 — ReminderScheduler Tests (25 tests)

All `[TestCategory("Offline")]`. Uses Moq for ITaskEngine/IComplianceCore/IAppLogger
and promoted FakeClock for time control.

| # | Test | AC |
|---|---|---|
| 1 | Constructor_NullTaskEngine_Throws | ctor gate |
| 2 | Constructor_NullClock_Throws | ctor gate |
| 3 | Constructor_NullCompliance_Throws | ctor gate |
| 4 | Constructor_NullLogger_Throws | ctor gate |
| 5 | Constructor_ZeroTickInterval_Throws | ctor gate |
| 6 | Constructor_NegativeTickInterval_Throws | ctor gate |
| 7 | Constructor_OmittedTickInterval_UsesDefault30Seconds | P1D-AC1 |
| 8 | Cadence_SetToZero_Throws | ctor gate |
| 9 | Cadence_SetToNegative_Throws | ctor gate |
| 10 | Cadence_SetToValid_UpdatesProperty | P1D-AC1 |
| 11 | StartAsync_CalledTwice_IsIdempotent | P1D-AC1 |
| 12 | StopAsync_WithoutStart_IsIdempotent | P1D-AC1 |
| 13 | EvaluateOnceAsync_CriticalTaskNeverFired_RaisesReminderDue | P1D-AC3 |
| 14 | EvaluateOnceAsync_DoneTask_DoesNotRaise | P1D-AC3 |
| 15 | EvaluateOnceAsync_NotYetDue_DoesNotRaise | P1D-AC2 |
| 16 | EvaluateOnceAsync_AuditFailure_StillRaisesReminderDue | HALT #8A |
| 17 | EvaluateOnceAsync_RaisesPerTaskOnly_NotPerTree | P1D-AC3 |
| 18 | EvaluateOnceAsync_SubscriberThrows_LoopContinues | resilience |
| 19 | EvaluateOnceAsync_RecordsLastFiredAt_PreventsImmediateRefire | P1D-AC2 |
| 20 | EvaluateOnceAsync_AfterRepeatInterval_FiresAgain | P1D-AC2 |
| 21 | EvaluateOnceAsync_PassesCorrectAuditFields | §10.5 |
| 22 | ReminderScheduler_HasNoSnoozeMethod_ByReflection | P1D-AC4 |
| 23 | ReminderScheduler_HasNoEscalateMethod_ByReflection | P1D-AC4 |
| 24 | DisposeAsync_AfterStart_StopsCleanly | lifecycle |
| 25 | StartAsync_AfterDispose_Throws | lifecycle |

---

## §14 — Test-Side Gap Ledger (15 TG1D-* gaps)

Every test-side assumption that the chat environment cannot verify against the
assembled repo is recorded here. Phase 5 must walk this list end-to-end.

### Hard test gaps — block Phase 5B compile or 5C run

| ID | Gap | Likely fix | Phase |
|---|---|---|---|
| TG1D-1  | `TaskStatus.Active` assumed as default non-Done | Verify enum; if `Pending`/`New`/`Open`, rename per D2 | 5B |
| TG1D-2  | `TaskNode` init properties (Id/Priority/Deadline/Status/Title) | Diff vs Phase 0 Msg 3; adjust init | 5B |
| TG1D-3  | `ITaskEngine.GetTreeAsync` returns `IReadOnlyList<TaskNode>` | Verify cast in Moq setup | 5B |
| TG1D-4  | `IComplianceCore.AuditAsync(AuditEntry)` signature | Verify; adjust mock | 5B |
| TG1D-5  | `IAppLogger` mocked with `It.IsAny`; method shape duck-typed | Adapt mock setup to actual interface | 5B |
| TG1D-6  | Moq 4.20.72 may not match repo-wide pinned version | Align with Phase 0 Msg 6 packages | 5A |
| TG1D-7  | MSTest 3.6.1 may not match repo-wide pinned version | Align with Phase 0 Msg 6 packages | 5A |
| TG1D-8  | Test csproj adds NEW ProjectReferences (TestSupport + production) | Verify supersedes scaffold; update sln | 5A |
| TG1D-9  | `TaskTree.TestSupport.Clocks.FakeClock` dependency | Verify Msg 1 promotion intact | 5A |
| TG1D-11 | `AuditEntry` init shape Module/Action/TargetId/Result/Timestamp | Verify; adjust ctor or init | 5B |
| TG1D-12 | `ReminderEvent.Reason` field per Arch v1.0.2 | Verify amendment applied | 5A |
| TG1D-13 | `Priority` enum names — same N2 from Msg 1 | Verify; rename per D2 | 5B |
| TG1D-14 | `IClock.UtcNow` property name — same N5 from Msg 1 | Verify; rename per D2 | 5B |

### Soft test gaps — Phase 5C/5E concerns

| ID | Gap | Phase | Notes |
|---|---|---|---|
| TG1D-10 | Reflection-based AC4 enforcement (Snooze/Escalate absence) | by-design | When Phase 2G adds these methods, tests 22 + 23 MUST be updated/removed |
| TG1D-15 | Tests do not capture SyncContext (consistent with HALT #8A) | 5C | If 1F Orchestrator marshals to Dispatcher, integration tests verify there |

---

## §Acceptance Criteria Coverage Matrix

| Acceptance criterion | Tests covering | Confidence |
|---|---|---|
| P1D-AC1 (Start ticks; Stop halts) | Sched 7, 8, 9, 10, 11, 12, 24, 25 | HIGH |
| P1D-AC2 (Cadence per §5.3 all priorities) | All 16 CadencePolicy tests + Sched 15, 19, 20 | HIGH |
| P1D-AC3 (ReminderDue includes node + reason) | Sched 13, 14, 17, 21 + Cadence 10, 11, 12 | HIGH |
| P1D-AC4 (Snooze/Escalation NOT impl) | Sched 22, 23 (reflection) | HIGH (by-design negative gate) |

---

## §Anti-Drift Re-Check (per D1–D10)

| Rule | Result |
|---|---|
| D1 — header citations | OK all files |
| D2 — no renames | OK |
| D3 — no invented libs (Moq, MSTest, FakeClock all approved) | OK |
| D5 — no TODO/placeholder | OK |
| D6 — synthetic test data only ("synthetic-task" title) | OK |
| D9 — testable via interface | OK public StartAsync/StopAsync + interface mocks |
| D10 — XML doc on public test classes | OK |

---

## §Files Produced This Msg

| Path | Status |
|---|---|
| tests/TaskTree.Modules.ReminderScheduler.Tests/TaskTree.Modules.ReminderScheduler.Tests.csproj | AMENDED |
| tests/TaskTree.Modules.ReminderScheduler.Tests/CadencePolicyTests.cs (16 tests) | NEW |
| tests/TaskTree.Modules.ReminderScheduler.Tests/ReminderSchedulerTests.cs (25 tests) | NEW |
| src/TaskTree.Modules.ReminderScheduler/Properties/AssemblyInfo.cs | NEW (closes G1D-20) |
| docs/spec-derivations/PHASE1D-DERIVATIONS-MSG2.md | NEW (this file) |
| docs/HANDOFF.v1.0.17-delta.md | NEW |
| tools/find-spec-derivations.ps1 | UPDATED (PHASE1D = 5) |

## End of PHASE1D-DERIVATIONS-MSG2.md
