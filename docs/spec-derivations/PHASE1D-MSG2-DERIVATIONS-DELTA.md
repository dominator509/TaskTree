# PHASE1D-MSG2-DERIVATIONS-DELTA.md — Phase 1D Msg 2 (Tests)

> **Scope:** ReminderSchedulerTests + CadencePolicyTests + InternalsVisibleTo + 2 patches to Msg 1 production code.
> **Companion to:** PHASE1D-DERIVATIONS.md (Msg 1 base).
> **Companion docs:** Architecture.md v1.0.1 (v1.0.2 pending), Roadmap.md v1.0.0, HANDOFF.md v1.0.17.
> **Owner-approved HALT batch:** 13 items, batch-resolved.

---

## §1 Summary Table

| # | Item | Resolution | Files | LOAD-BEARING? |
|---|---|---|---|---|
| 1 | CadencePolicy test access | Option A — `[InternalsVisibleTo]` in AssemblyInfo.cs | AssemblyInfo.cs (NEW) | No |
| 2 | Tick-loop timing in tests | Option C — hybrid; TickOnceAsync `private`→`internal` | ReminderScheduler.cs (PATCH) | No |
| 3 | Test file split | 2 files — ReminderSchedulerTests + CadencePolicyTests | Both test files | No |
| 4 | Test count | **20 tests** (10 + 10) | Both test files | No |
| 5 | ITaskEngine mock | Moq Strict; only GetTreeAsync | ReminderSchedulerTests | No (drift detector) |
| 6 | IComplianceCore mock | Moq Verify exact AuditEntry | ReminderSchedulerTests #5 | No |
| 7 | IAppLogger mock | Moq Loose; verify LogWarning only in #8 | ReminderSchedulerTests | No |
| 8 | Test categories | All `[TestCategory("Offline")]` | Both | No |
| 9 | Cadence clamp inclusivity | `[1s, 5min]` inclusive | ReminderSchedulerTests #2, #3 | No |
| 10 | ReminderEvent schema | 4-field assertion | ReminderSchedulerTests #5 | No |
| 11 | StopWaitTimeout bounded-wait | `private static readonly` → `internal static` | ReminderScheduler.cs (PATCH) + #8 | **YES — Gap #48 guard** |
| 12 | csproj wiring | 3 ProjectReferences | csproj (PATCH) | No |
| 13 | Dispose edge cases | 2 tests | ReminderSchedulerTests #9, #10 | No (Phase 5D carries) |

---

## §2 — Item #1: InternalsVisibleTo

- **Trigger:** CadencePolicy is `internal static class`; test project cannot see it by default.
- **Architecture silence:** §12 silent on `InternalsVisibleTo`.
- **Options:** A) assembly-level attribute · B) make CadencePolicy public · C) test only via ReminderScheduler public surface.
- **Resolution:** Option A.
- **Rationale:** Smallest API impact; idiomatic .NET; no production consumer expansion.
- **Files:** NEW `src/TaskTree.Modules.ReminderScheduler/Properties/AssemblyInfo.cs`.
- **Gap for Handoff:** Gap #46 — Phase 5B compile gate must verify exactly one `InternalsVisibleTo` line, scoped to Tests assembly only.

## §3 — Item #2: TickOnceAsync Internal

- **Trigger:** Real PeriodicTimer with 30s default unusable for deterministic tests.
- **Options:** A) Real-timer only · B) TickOnceAsync internal · C) Hybrid.
- **Resolution:** Option C — `private` → `internal`.
- **Rationale:** Determinism for content tests; real-timer for lifecycle smoke.
- **Files:** PATCH ReminderScheduler.cs.
- **Gap for Handoff:** Gap #47 — Phase 1F orchestrator MUST use StartAsync/StopAsync only. InternalsVisibleTo scope enforces by construction.

## §4 — Item #3: Test File Split

- **Resolution:** 2 files mirrors Phase 1C precedent.
- **Files:** NEW both.
- **Gap for Handoff:** None.

## §5 — Item #4: Test Count

- **Resolution:** 20 tests (10 + 10). Revised upward from preview 18 by adding 2 Dispose tests.
- **Rationale:** Exceeds 1A=14, 1C=13 baselines.
- **Gap for Handoff:** Gap #49 — Phase 4B may bump test #7 tolerance for heavy CI.

## §6 — Item #5: ITaskEngine Mock

- **Resolution:** Moq Strict; only `GetTreeAsync` setup.
- **Gap for Handoff:** Gap #53 — if scheduler ever calls another ITaskEngine method, Strict fails fast (intentional drift detection).

## §7 — Item #6: IComplianceCore Mock

- **Resolution:** `Verify(c => c.AuditAsync(It.Is<AuditEntry>(e => e.Module=="ReminderScheduler" && e.Action=="ReminderFired" && e.TargetId==node.Id && e.Result=="success")), Times.Once)`.
- **Gap for Handoff:** Gap #54 — Result hard-coded `"success"`; failure-path value undefined. Phase 4A compliance policy must define.

## §8 — Item #7: IAppLogger Mock

- **Resolution:** Moq Loose; no verification except test #8 LogWarning.
- **Gap for Handoff:** None.

## §9 — Item #8: Test Categories

- **Resolution:** All Offline; cumulative ~1s CI cost.
- **Gap for Handoff:** Gap #49 (tolerance under heavy CI).

## §10 — Item #9: Cadence Clamp Inclusivity

- **Resolution:** `[1s, 5min]` inclusive verified via tests #2, #3.
- **Gap for Handoff:** Gap #44 (carried) — Architecture v1.0.2 §4.3 may formalize.

## §11 — Item #10: ReminderEvent 4-Field Assertion

- **Resolution:** Test #5 asserts TaskId / FiredAtUtc / Reason / Priority all populated.
- **Gap for Handoff:** None.

## §12 — Item #11: StopWaitTimeout Internal-Mutable

- **Trigger:** Gap #38 — verify 5s bound holds.
- **Resolution:** Promote `private static readonly` → `internal static`. Test #8 mutates to 200ms, restores in finally.
- **Files:** PATCH ReminderScheduler.cs; NEW test #8.
- **Gap for Handoff:** **Gap #48 — Production MUST NOT mutate.** Phase 5D grep: `StopWaitTimeout\s*=` should match only test code.

## §13 — Item #12: csproj Wiring

- **Resolution:** 3 ProjectReferences added.
- **Gap for Handoff:** None — `dotnet build` verifies at Phase 5B.

## §14 — Item #13: Dispose Edge Cases

- **Resolution:** Tests #9 (double-call) + #10 (dispose-while-running).
- **Gap for Handoff:** Gap #52 — Live DI teardown still owed at Phase 5D.

---

## §15 — Files Produced This Msg

| Path | Purpose | Marker(s) |
|---|---|---|
| `ReminderScheduler.cs` (PATCHED) | TickOnceAsync internal + StopWaitTimeout internal mutable | PHASE1D + PHASE1D-MSG2 |
| `CadencePolicy.cs` (unchanged) | §5.3 binding | PHASE1D |
| `Properties/AssemblyInfo.cs` (NEW) | InternalsVisibleTo grant | PHASE1D-MSG2 |
| `ReminderSchedulerTests.cs` (NEW) | 10 tests | PHASE1D-MSG2 |
| `CadencePolicyTests.cs` (NEW) | 10 tests | PHASE1D-MSG2 |
| `TaskTree.Modules.ReminderScheduler.Tests.csproj` (PATCHED) | 3 ProjectReferences | (no marker) |

---

## §16 — Marker Inventory Updates

| Bucket | Old (Msg 1) | New (Msg 2) | Notes |
|---|---|---|---|
| `SPEC-DERIVED-PHASE1D` | 4 | **7** | Substring match catches PHASE1D-MSG2 files too |
| `SPEC-DERIVED-PHASE1D-MSG2` | — | **4** | NEW bucket |

`tools/find-spec-derivations.ps1` updated. Grand-total = 40 (bucket sum).

---

## §17 — Cross-Phase Gaps Introduced (rows 46-55)

| # | Gap | Source | Target Phase | Action |
|---|---|---|---|---|
| 46 | InternalsVisibleTo scope = Tests-only | Msg2 HALT #1 | Phase 5B | Compile gate verifies 1 attribute line |
| 47 | Orchestrator MUST use StartAsync/StopAsync only | Msg2 HALT #2 | Phase 1F | InternalsVisibleTo scope enforces |
| 48 | **Production MUST NOT mutate StopWaitTimeout** | Msg2 HALT #11 | Phase 5D | Grep guard; 0 non-test matches expected |
| 49 | Real-timer test #7 350ms tolerance | Msg2 HALT #8 | Phase 4B | Perf suite tolerance bump if needed |
| 50 | Trivial at-deadline → Overdue (not Initial) | Msg2 HALT #5 §7 + Gap #43 | Phase 1G | Delivery adapter UX must handle |
| 51 | High/Normal/Low pre-deadline boundary fires at exact offset | Gap #41 closure | Phase 2B | UI tooltip "fires AT or before offset" |
| 52 | Dispose live-DI teardown owed | Msg2 HALT #13 + Gap #39 | Phase 5D | Real container deadlock check |
| 53 | Moq Strict on ITaskEngine — drift detection | Msg2 HALT #5 | Phase 1D+ | Future engine calls fail fast |
| 54 | AuditEntry.Result hard-coded "success" | Msg2 HALT #6 | Phase 4A | Define failure-Result vocab |
| 55 | UX must document Trivial-Overdue-at-deadline | Gap #43 + #50 closure | Phase 1G + Phase 2B | User-facing copy design |

---

## §18 — Patches to Msg 1 Production Code (Drift Log)

| File | Change | Source |
|---|---|---|
| ReminderScheduler.cs | `private async Task TickOnceAsync` → `internal async Task TickOnceAsync` | HALT-Msg2 #2 |
| ReminderScheduler.cs | `private static readonly TimeSpan StopWaitTimeout = ...` → `internal static TimeSpan StopWaitTimeout = ...` | HALT-Msg2 #11 |
| Properties/AssemblyInfo.cs | NEW file with `InternalsVisibleTo` | HALT-Msg2 #1 |

**Public API surface: UNCHANGED. SemVer impact: none. Non-breaking.**

---

## §19 — Phase 1F Composition Root Checklist Additions

Append to Msg 1 §19:

8. **NEW:** Orchestrator must call `StartAsync`/`StopAsync` only — never reach for internal `TickOnceAsync` (Gap #47). Enforced by InternalsVisibleTo scope.
9. **NEW:** Production code MUST NOT mutate `ReminderScheduler.StopWaitTimeout` (Gap #48). Phase 5D grep-verifies.

---

## §20 — Phase 5D Verification Additions

1. **Live DI teardown:** `Dispose()` under runtime `IServiceProvider`; verify no deadlock (Gap #39 + #52).
2. **`StopWaitTimeout` mutation guard:** `Get-ChildItem -Recurse -Include *.cs | Select-String 'StopWaitTimeout\s*='` should match only test code; expect 1 match in `ReminderSchedulerTests.cs` test #8 (Gap #48).
3. **InternalsVisibleTo scope:** Confirm exactly 1 attribute line in module assembly (Gap #46).

---

## §21 — Test Category Usage

All 20 tests = Offline. Cumulative real-timer time ~1s. Below Phase 4B integration-suite threshold (undefined; Phase 4A will set).

---

## §22 — Coverage Targets (Informational)

| AC | Coverage |
|---|---|
| P1D-AC1 lifecycle | Tests #4, #7, #8, #10 |
| P1D-AC2 cadence | CadencePolicyTests #1, #2 + indirect via #4-#10 |
| P1D-AC3 ReminderDue payload | Test #5 (4-field assertion) |
| P1D-AC4 snooze/escalation deferred | Implicit (no escalation tests; ReminderReason omits Escalation) |

**Estimated coverage:** ReminderScheduler.cs ~85%; CadencePolicy.cs ~95%. **Phase 1H gate (75%) met.**

---

## §23 — Known Limitations

1. Real-timer test #7 has ~350ms tolerance — flakiness risk on heavy CI agents (Gap #49).
2. `StopWaitTimeout` mutation in test #8 not thread-safe; relies on MSTest serial-per-class default. If parallel execution enabled later, mark `[DoNotParallelize]`.
3. Dispose tests don't verify handle counters / leak detection — Phase 5D scope.
4. CadencePolicy tests assume `Priority` enum row-order matches §5.3; if enum reorders, all 5 DataRow blocks must be re-validated.
5. No test covers `_lastFiredUtc` Dictionary growth bound (potential leak over multi-day sessions). Phase 4B perf concern.
6. Test #8 mutation persists across test instance until `finally`; if a crash skips finally (unlikely), subsequent tests in same process see shortened timeout. Mitigated by per-test process isolation in CI.
