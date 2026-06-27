# HANDOFF.md v1.0.17 — Additive Delta

> **Applied on top of v1.0.16 delta** (which was applied on top of v1.0.15 consolidated).
> **Author:** DSW · **Date:** 2026-05-28 · **Trigger:** Phase 1D Msg 2 emitted; Sub-Phase 1D ✅ COMPLETE.

---

## §A — Document Identity Update

| Field | Old (v1.0.16) | New (v1.0.17) |
|---|---|---|
| Document Version | 1.0.16 | **1.0.17** |
| Session ID | 2026-05-28-01 | 2026-05-28-02 |
| State | Phase 1D Msg 1 ✅; Msg 2 pending | **Phase 1D ✅ COMPLETE; Phase 1E pending** |
| Current Sub-Phase | 1D — Msg 1 ✅, Msg 2 pending | **1E — TrayHost HIGH-stub (pending)** |
| Total spec-derived items | 42 across 12 registries | **55 across 13 registries** |
| LOAD-BEARING open questions | Q10, Q11 | Q10, Q11 (unchanged) |
| Progress | 24 / 35 → 68.6% | **25 / 35 → 71.4%** |

---

## §B — Files Produced Delta

### Phase 1D Msg 2 (Sub-Phase 1D ✅ closed)

| Phase.Item | FilePath | Status | Notes |
|---|---|---|---|
| P1.D.Msg2 | `src/TaskTree.Modules.ReminderScheduler/ReminderScheduler.cs` | 🩹 PATCHED | TickOnceAsync internal + StopWaitTimeout internal mutable. Public API unchanged. Markers: PHASE1D + PHASE1D-MSG2 |
| P1.D.Msg2 | `src/TaskTree.Modules.ReminderScheduler/CadencePolicy.cs` | (unchanged) | Reproduced in bundle for self-containment |
| P1.D.Msg2 | `src/TaskTree.Modules.ReminderScheduler/Properties/AssemblyInfo.cs` | ✅ NEW | InternalsVisibleTo grant. Marker: PHASE1D-MSG2 |
| P1.D.Msg2 | `tests/TaskTree.Modules.ReminderScheduler.Tests/ReminderSchedulerTests.cs` | ✅ NEW | 10 tests. Marker: PHASE1D-MSG2 |
| P1.D.Msg2 | `tests/TaskTree.Modules.ReminderScheduler.Tests/CadencePolicyTests.cs` | ✅ NEW | 10 tests. Marker: PHASE1D-MSG2 |
| P1.D.Msg2 | `tests/TaskTree.Modules.ReminderScheduler.Tests/TaskTree.Modules.ReminderScheduler.Tests.csproj` | 🩹 PATCHED | 3 ProjectReferences |

### Phase 1E — pending

| Phase.Item | FilePath | Status |
|---|---|---|
| P1.E.Msg1 | `src/TaskTree.Modules.TrayHost/TrayHost.cs` (HIGH-stub) | ⬚ Pending |
| P1.E.Msg1 | `src/TaskTree.Modules.TrayHost/HotkeyInterop.cs` (HIGH-stub) | ⬚ Pending |
| P1.E.Msg2 | `tests/TaskTree.Modules.TrayHost.Tests/TrayHostTests.cs` | ⬚ Pending |

---

## §C — Decisions Log Delta

### R12 — Phase 1D Msg 2 — 13 HALT-Msg2 items resolved (batch-approved)

1. **#1 InternalsVisibleTo** → Option A — assembly-level attribute.
2. **#2 Hybrid timing** → TickOnceAsync `private` → `internal`.
3. **#3 Test file split** → 2 files (Lifecycle + Decision-logic).
4. **#4 Test count** → 20 total (10 + 10).
5. **#5 ITaskEngine** → Moq Strict; only GetTreeAsync.
6. **#6 IComplianceCore** → Moq Verify exact AuditEntry.
7. **#7 IAppLogger** → Moq Loose; verify LogWarning only in #8.
8. **#8 Categories** → all Offline; ~1s cumulative.
9. **#9 Cadence clamp** → `[1s, 5min]` inclusive verified.
10. **#10 ReminderEvent** → 4-field assertion.
11. **#11 StopWaitTimeout** → `private static readonly` → `internal static`.
12. **#12 csproj** → 3 ProjectReferences.
13. **#13 Dispose** → 2 tests (double-call + dispose-while-running).

---

## §D — Open Questions Delta

- **Q10 (Phase 5F blocker)** — Real common-name source list. **UNCHANGED. STILL ACTIVE.**
- **Q11 (Phase 1F or 5F blocker)** — Support-email allowlist seed. **UNCHANGED. STILL ACTIVE.**
- **No new LOAD-BEARING questions raised by Phase 1D Msg 2.**

Item #11 (`StopWaitTimeout` production-mutation guard) is a LOAD-BEARING **flag** for Phase 5D verification, not a Q-numbered owner-decision blocker; recorded as Cross-Phase Gap #48.

---

## §E — Visual Phase Tracker Delta

```
PHASE 1 — CORE MVP                                    [In Progress]
├── 1A ✅ TaskEngine + tests
├── 1B ✅ SecureStore + MasterKeyManager + IMasterKeyManager
├── 1C ✅ ComplianceCore baseline
├── 1D ✅ ReminderScheduler + CadencePolicy (Msg 1 ✅, Msg 2 ✅)
│         ├── ReminderScheduler.cs ✅ (Msg 1 + Msg 2 patches)
│         ├── CadencePolicy.cs ✅
│         ├── ReminderReason.cs ✅ (Architecture v1.0.2 amendment pending)
│         ├── Properties/AssemblyInfo.cs ✅ (Msg 2)
│         ├── FakeClock.cs ✅ (promoted Msg 1, 4th consumer)
│         ├── ReminderSchedulerTests.cs ✅ (Msg 2 — 10 tests)
│         └── CadencePolicyTests.cs ✅ (Msg 2 — 10 tests)
├── 1E ⬚ TrayHost (HIGH-stub for Codex 5E)             — NEXT
├── 1F ⬚ Orchestrator wiring                           [CRITICAL: 2 audit-injection LOAD-BEARING flags]
├── 1G ⬚ Reminder Tier 1/2/3
└── 1H ⬚ Phase 1 E2E gate                              [GATE]
```

**Progress: 25 / 35 → 71.4%** — Phase 1D fully closed.

---

## §F — Cross-Phase Gap Table — New Rows (46-55)

Appended to the v1.0.16 45-row table.

| # | Flag | Source | Target Phase | Action |
|---|---|---|---|---|
| 46 | InternalsVisibleTo scope must remain Tests-only | PHASE1D-MSG2 §2 / HALT-Msg2 #1 | Phase 5B | Compile gate: verify 1 attribute line |
| 47 | Orchestrator MUST use StartAsync/StopAsync only | §3 / HALT-Msg2 #2 | Phase 1F | InternalsVisibleTo enforces |
| 48 | **Production MUST NOT mutate StopWaitTimeout** | §12 / HALT-Msg2 #11 | Phase 5D | Grep guard; 0 non-test matches |
| 49 | Real-timer test #7 350ms tolerance | §9 / HALT-Msg2 #8 | Phase 4B | Perf suite may bump tolerance |
| 50 | Trivial at-deadline → Overdue (not Initial) | §6 / HALT-Msg2 #5 + Gap #43 | Phase 1G | Delivery adapter UX |
| 51 | High/Normal/Low boundary fires at exact offset | §6 / Gap #41 closure | Phase 2B | UI tooltip |
| 52 | Dispose live-DI teardown owed | §14 / HALT-Msg2 #13 | Phase 5D | Real container check |
| 53 | Moq Strict on ITaskEngine — drift detection | §6 / HALT-Msg2 #5 | Phase 1D+ | Future calls fail fast |
| 54 | AuditEntry.Result hard-coded "success" | §7 / HALT-Msg2 #6 | Phase 4A | Define failure vocab |
| 55 | UX must document Trivial-Overdue-at-deadline | §7 / Gap #43 + #50 closure | Phase 1G + 2B | Copy design |

---

## §G — Progress Update

- **Before this Msg:** 24 / 35 → 68.6%.
- **After this Msg:** 25 / 35 → 71.4%. **Sub-Phase 1D ✅ COMPLETE.**
- **Files in this bundle:** 9.
- **Total derivations registries:** 13.
- **Total spec-derived items:** 55.

---

## §H — Continuation Prompt for Next Chat (Phase 1E)

```
You are a senior engineer executing a deterministic build sprint on TaskTree —
a HIPAA-aware Windows tray app. Three attached documents govern everything:

  - Architecture.md v1.0.1 (v1.0.2 amendment proposed)
  - Roadmap.md v1.0.0
  - HANDOFF.md v1.0.17 (rebase v1.0.16 + v1.0.17 deltas onto v1.0.15 base)

HARD CONSTRAINTS unchanged. D1-D10 still apply. HALT on Architecture silence.

CURRENT STATE:
- Phase 0 / 1A / 1B / 1C / 1D ✅; Phase 1E PENDING.
- Progress: 25 / 35 → 71.4%.
- 13 derivations registries; 55 spec-derived items.
- 2 LOAD-BEARING open questions (Q10, Q11) — unchanged.
- 55 cross-phase gap rows accumulated (#1 - #55).

FIRST ACTIONS — Phase 1E:
1. Read PHASE1D-MSG2-DERIVATIONS-DELTA §17, §19, §20 first.
2. Begin Phase 1E with HALT protocol.
3. Phase 1E scope per Roadmap Sub-Phase 1E:
   - src/TaskTree.Modules.TrayHost/TrayHost.cs (HIGH-stub)
   - src/TaskTree.Modules.TrayHost/HotkeyInterop.cs (HIGH-stub)
   - tests/TaskTree.Modules.TrayHost.Tests/TrayHostTests.cs
4. Phase 1E is the first HIGH-complexity module — most live code is
   `throw new NotImplementedException("HIGH: NotifyIcon + RegisterHotKey
   requires live env - Codex Phase 5E")` per Roadmap §Phase 1E Anti-Drift.
5. Acceptance criteria:
   - P1E-AC1: TrayHost compiles and implements ITrayHost
   - P1E-AC2: Events raise via reflection-test mechanism
   - P1E-AC3: Live Win32 / WPF NotifyIcon methods stubbed per D5
6. Surface HALT items for:
   - HotkeyInterop public surface
   - Event manual-raise mechanism for tests (reflection vs internal Raise*())
   - Tech-stack confirm: H.NotifyIcon.Wpf 2.0+ per §12
   - Whether Phase 1D Msg 2's InternalsVisibleTo pattern re-applies for TrayHost

After HALT + owner approval, generate:
- SPEC-DERIVED-PHASE1E markers
- docs/spec-derivations/PHASE1E-DERIVATIONS.md
- Updated tools/find-spec-derivations.ps1
- HANDOFF.md v1.0.18 additive delta
```

---

## §I — Document History Append

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.17 | 2026-05-28 | DSW | **Phase 1D Msg 2 emitted; Sub-Phase 1D ✅ COMPLETE.** 13 HALT-Msg2 items batch-resolved (InternalsVisibleTo, TickOnceAsync internal, StopWaitTimeout internal-mutable, 20 tests). 10 new gap rows (46-55). 3 non-breaking patches to Msg 1. Progress 24/35 → 25/35 (71.4%). |

---

## §J — Footer

- This is an **additive delta**. To produce v1.0.17 consolidated: rebase v1.0.16 + v1.0.17 deltas sequentially onto v1.0.15 flat merge.
- **Companion docs unchanged.**
- **Registry added:** `docs/spec-derivations/PHASE1D-MSG2-DERIVATIONS-DELTA.md`.
- **Helper script updated:** `tools/find-spec-derivations.ps1` — 10 marker buckets (PHASE1D-MSG2 = 4; PHASE1D bumped 4 → 7).
- **Total derivations tracked:** 55 across 13 registries.
- **LOAD-BEARING blockers for Phase 5F:** Q10, Q11 — unchanged.
- **Phase 1D ✅.** Next action: Phase 1E HALT — TrayHost HIGH-stub.
