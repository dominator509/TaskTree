# PHASE1E-DERIVATIONS-MSG2.md

**Phase:** 1E - TrayHost (HIGH-stub for Codex Phase 5E)
**Msg:** 2 of 2 (tests) - Phase 1E COMPLETE upon this delivery
**Source authority:** Architecture.md v1.0.2 §4.1; Roadmap.md v1.0.0 P1E-AC1..AC3
**Predecessor:** PHASE1E-DERIVATIONS.md (Msg 1)
**Marker:** SPEC-DERIVED-PHASE1E unchanged at 3 (test files do not carry markers per Msg 6 convention)

---

## §12 - TrayHost Tests (18 tests)

All `[TestCategory("Offline")]`. Uses Moq for IAppLogger. Relies on
InternalsVisibleTo grant from Msg 1 AssemblyInfo.cs (PHASE1E §10).

| # | Test | AC |
|---|---|---|
| 1 | Constructor_NullLogger_Throws | ctor gate |
| 2 | RaiseShowTreeRequested_WithSubscriber_DeliversEvent | P1E-AC2 |
| 3 | RaiseAddTaskRequested_WithSubscriber_DeliversEvent | P1E-AC2 |
| 4 | RaiseExitRequested_WithSubscriber_DeliversEvent | P1E-AC2 |
| 5 | RaiseShowTreeRequested_NoSubscribers_DoesNotThrow | defensive (null-safe ?.Invoke) |
| 6 | RaiseAddTaskRequested_NoSubscribers_DoesNotThrow | defensive |
| 7 | RaiseExitRequested_NoSubscribers_DoesNotThrow | defensive |
| 8 | Initialize_Throws_WithExplicitCodex5EReason | P1E-AC3 + D5 |
| 9 | ShowBalloon_ValidArgs_ThrowsNotImplementedWithCodex5EReason | P1E-AC3 + D5 |
| 10 | ShowBalloon_NullTitle_Throws | defensive ArgumentNull guard |
| 11 | ShowBalloon_NullMessage_Throws | defensive ArgumentNull guard |
| 12 | Dispose_CalledTwice_IsIdempotent | lifecycle |
| 13 | Dispose_AfterInitializeThrows_StillCleansCleanly | resilience |
| 14 | RaiseShowTreeRequested_AfterDispose_Throws | lifecycle (ObjectDisposed) |
| 15 | RaiseAddTaskRequested_AfterDispose_Throws | lifecycle |
| 16 | RaiseExitRequested_AfterDispose_Throws | lifecycle |
| 17 | Initialize_AfterDispose_Throws | lifecycle |
| 18 | TrayHost_ImplementsITrayHost_With_ExpectedSurface | §4.1 conformance via reflection |

---

## §13 - Test Patterns Confirmed

- **Internal Raise() pattern** - consistent with G1D-20 precedent (Phase 1D ReminderScheduler EvaluateOnceAsync).
- **Reflection-based interface conformance** - generalizes the negative-surface pattern from Phase 1D Scheduler tests 22/23.
- **No PInvoke exercised** - by design. All Win32 work is a Phase 5E environment gap.
- **No H.NotifyIcon.Wpf API exercised** - by design. Live tray work is Phase 5E.
- **No STA thread fixture** - see TG1E-8 below.

---

## §14 - Test-Side Gap Ledger (8 TG1E-* gaps)

### Hard test gaps - block Phase 5B compile or 5C run

| ID | Gap | Likely fix | Phase |
|---|---|---|---|
| TG1E-1 | `IAppLogger.LogInformation(string)` Moq shape assumed | Verify interface; adapt mock setup (same root as G1D-6/G1E-H2) | 5B |
| TG1E-2 | Test csproj adds NEW ProjectReferences (Core + production); supersedes Phase 0 Msg 6 scaffold | Verify supersedes; update sln if needed | 5A |
| TG1E-3 | Interface conformance test depends on §4.1 surface stability | If §4.1 evolves, update test in lockstep per D2 | 5B |
| TG1E-4 | Moq 4.20.72 + MSTest 3.6.1 may not match repo-wide pinned versions | Align with Phase 0 Msg 6 packages at 5A | 5A |
| TG1E-5 | `HotkeyInterop` internal access via `InternalsVisibleTo` grant from PHASE1E §10 | Verify AssemblyInfo.cs is included in compilation | 5A |

### Soft test gaps - Phase 5C/5E concerns

| ID | Gap | Phase | Notes |
|---|---|---|---|
| TG1E-6 | Tests do NOT exercise live PInvoke | by-design | Codex 5E adds live HotkeyManager tests with `[TestCategory("Live")]` |
| TG1E-7 | Tests do NOT exercise H.NotifyIcon.Wpf API | by-design | Codex 5E adds live tray icon tests with `[TestCategory("Live")]` |
| TG1E-8 | No STA thread fixture - G1E-S1 UNVERIFIED here | 5C/5D | Codex Phase 5D wires Orchestrator on STA; integration test verifies |

---

## §Acceptance Criteria Coverage Matrix

| Acceptance criterion | Tests covering | Confidence |
|---|---|---|
| P1E-AC1 (TrayHost compiles) | All 18 tests implicitly (file builds against production) | HIGH |
| P1E-AC2 (Events raise correctly via reflection test) | Tests 2, 3, 4, 5, 6, 7, 14, 15, 16, 18 | HIGH |
| P1E-AC3 (Live methods stubbed per D5) | Tests 8, 9, 10, 11 | HIGH |

---

## §Anti-Drift Re-Check (per D1-D10)

| Rule | Result |
|---|---|
| D1 - header citations | OK |
| D2 - no renames; verifies §4.1 surface verbatim via reflection | OK |
| D3 - no invented libs (MSTest + Moq only) | OK |
| D5 - no TODO/placeholder | OK |
| D6 - synthetic test data only | OK |
| D9 - testable via interface + internal seams | OK |
| D10 - XML doc on public test class | OK |

---

## §Files Produced This Msg

| Path | Status |
|---|---|
| tests/TaskTree.Modules.TrayHost.Tests/TaskTree.Modules.TrayHost.Tests.csproj | AMENDED |
| tests/TaskTree.Modules.TrayHost.Tests/TrayHostTests.cs (18 tests) | NEW |
| docs/spec-derivations/PHASE1E-DERIVATIONS-MSG2.md | NEW (this file) |
| docs/HANDOFF.v1.0.19-delta.md | NEW |

## End of PHASE1E-DERIVATIONS-MSG2.md
