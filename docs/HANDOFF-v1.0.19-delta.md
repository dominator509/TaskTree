# HANDOFF.md v1.0.19 — Additive Delta

> **Applied on top of v1.0.18 delta** (which was applied on top of v1.0.17 + v1.0.16 + v1.0.15 consolidated).
> **Author:** DSW · **Date:** 2026-05-28 · **Trigger:** Phase 1E Msg 2 emitted; Sub-Phase 1E ✅ COMPLETE.

---

## §A — Document Identity Update

| Field | Old (v1.0.18) | New (v1.0.19) |
|---|---|---|
| Document Version | 1.0.18 | **1.0.19** |
| Session ID | 2026-05-28-03 | 2026-05-28-04 |
| State | Phase 1E Msg 1 ✅; Msg 2 pending | **Phase 1E ✅ COMPLETE; Phase 1F pending** |
| Current Sub-Phase | 1E Msg 2 pending | **1F — Orchestrator wiring (pending)** |
| Total spec-derived items | 70 across 14 registries | **83 across 15 registries** |
| LOAD-BEARING open questions | Q10, Q11 | Q10, Q11 (unchanged) |
| Progress | 26 / 35 → 74.3% | **27 / 35 → 77.1%** |

---

## §B — Files Produced Delta

### Phase 1E Msg 2 (Sub-Phase 1E ✅ closed)

| Phase.Item | FilePath | Status | Notes |
|---|---|---|---|
| P1.E.Msg2 | `tests/TaskTree.Modules.TrayHost.Tests/TrayHostTests.cs` | ✅ NEW | 10 tests. SPEC-DERIVED-PHASE1E-MSG2 |
| P1.E.Msg2 | `tests/TaskTree.Modules.TrayHost.Tests/HotkeyInteropTests.cs` | ✅ NEW | 6 tests. SPEC-DERIVED-PHASE1E-MSG2 |
| P1.E.Msg2 | `tests/TaskTree.Modules.TrayHost.Tests/TaskTree.Modules.TrayHost.Tests.csproj` | 🪙 PATCHED | 2 ProjectReferences (Core + TrayHost) |
| P1.E.Msg2 | `docs/Architecture.v1.0.2-delta.md` | 🪙 UPDATED | 3 bundled changes (§3.3 + §4.3 + §4.1 NEW) — Gap #60 closure |
| P1.E.Msg2 | `tools/find-spec-derivations.ps1` | 🪙 UPDATED | PHASE1E 3→5; PHASE1E-MSG2 = 2; grand total = 47 |

### Phase 1F — pending

| Phase.Item | FilePath | Status |
|---|---|---|
| P1.F | `src/TaskTree.Orchestrator/Orchestrator.cs` | ⬚ Pending |
| P1.F | `src/TaskTree.App/Bootstrap/CompositionRoot.cs` | ⬚ Pending |
| P1.F | `src/TaskTree.App/Bootstrap/ServiceRegistrations.cs` | ⬚ Pending |
| P1.F | `tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs` | ⬚ Pending |

---

## §C — Decisions Log Delta

### R14 — Phase 1E Msg 2 — 13 HALT items resolved (batch-approved)

1. **#1 Test count** → 16 total (10 + 6).
2. **#2 File split** → 2 files (mirrors Phase 1D Msg 2).
3. **#3 IComplianceCore mock** → **Moq Strict** — converts Gap #57 to compile-time enforcement (Gap #62).
4. **#4 IAppLogger mock** → Moq Loose; no verification.
5. **#5 NotImplementedException text** → `Contains("HIGH:")` AND `Contains("Codex Phase 5E")` (Gap #61).
6. **#6 Event raise** → Direct internal `Raise*()` invocation.
7. **#7 No-subscribers safety** → 1 dedicated test.
8. **#8 Dispose** → 2 tests (double-call + post-dispose-throws).
9. **#9 ShowBalloon validation** → DataRow ×6 + 1 valid-input test.
10. **#10 BuildModifierFlags** → DataRow ×8 + constants + NoRepeat regression + §13 baseline.
11. **#11 Register/Unregister stubs** → 2 tests (canonical text).
12. **#12 csproj** → 2 ProjectReferences (Core + TrayHost; no Core.Tests).
13. **#13 Architecture v1.0.2 update** → 3 bundled changes (§3.3 + §4.3 + §4.1 NEW) — Gap #60 closure.

---

## §D — Open Questions Delta

- **Q10 (Phase 5F blocker)** — Real common-name source list. **UNCHANGED. STILL ACTIVE.**
- **Q11 (Phase 1F or 5F blocker)** — Support-email allowlist seed. **UNCHANGED. STILL ACTIVE.**
- **No new LOAD-BEARING questions raised by Phase 1E Msg 2.**
- **Architecture v1.0.2 owner sign-off block now expanded to 3 changes** — non-blocking now, but Phase 5F-blocking if not approved.

---

## §E — Visual Phase Tracker Delta

```
PHASE 1 — CORE MVP                                    [In Progress]
├── 1A ✅ TaskEngine + tests
├── 1B ✅ SecureStore + MasterKeyManager + IMasterKeyManager
├── 1C ✅ ComplianceCore baseline
├── 1D ✅ ReminderScheduler + CadencePolicy
├── 1E ✅ TrayHost + HotkeyInterop (Msg 1 ✅ stubs; Msg 2 ✅ tests)
│         ├── TrayHost.cs ✅ HIGH-stub
│         ├── HotkeyInterop.cs ✅ HIGH-stub
│         ├── Properties/AssemblyInfo.cs ✅
│         ├── TrayHostTests.cs ✅ (Msg 2 — 10 tests)
│         └── HotkeyInteropTests.cs ✅ (Msg 2 — 6 tests)
├── 1F ⬚ Orchestrator wiring                           — NEXT  [CRITICAL: 3 audit-injection LOAD-BEARING flags]
├── 1G ⬚ Reminder Tier 1/2/3
└── 1H ⬚ Phase 1 E2E gate                              [GATE]
```

**Progress: 27 / 35 → 77.1%** — Phase 1E fully closed.

---

## §F — Cross-Phase Gap Table — New Rows (61-62)

Appended to v1.0.18 60-row table.

| # | Flag | Source | Target Phase | Action |
|---|---|---|---|---|
| 61 | Canonical NotImplementedException text contract enforced via `Contains()` | PHASE1E-MSG2 §6 / HALT-Msg2 #5 | Phase 5E | Codex replaces/removes 4 text-verification tests when implementing live methods |
| 62 | TrayHostTests Strict `IComplianceCore` mock breaks when audit wired | PHASE1E-MSG2 §4 / HALT-Msg2 #3 | Phase 5E | Codex: switch to Loose with `Setup` + `Verify` on new audit contract |

---

## §G — Progress Update

- **Before this Msg:** 26 / 35 → 74.3%.
- **After this Msg:** 27 / 35 → 77.1%. **Phase 1E ✅ COMPLETE.**
- **Files in this bundle:** 7 (2 test files + 1 patched csproj + 1 derivations registry + 1 updated Architecture v1.0.2 delta + 1 HANDOFF delta + 1 updated PowerShell tool).
- **Total derivations registries:** 15 (added PHASE1E-MSG2).
- **Total spec-derived items:** 83 (added 13 PHASE1E-MSG2 items to prior 70).

---

## §H — Continuation Prompt for Next Chat (Phase 1F)

```
You are a senior engineer executing a deterministic build sprint on TaskTree —
a HIPAA-aware Windows tray app. Three attached documents govern everything:

  - Architecture.md v1.0.1 (v1.0.2 amendment proposed — 3 bundled changes pending owner sign-off)
  - Roadmap.md v1.0.0
  - HANDOFF.md v1.0.19 (rebase v1.0.16 + v1.0.17 + v1.0.18 + v1.0.19 deltas onto v1.0.15 base)

HARD CONSTRAINTS unchanged. D1-D10 still apply. HALT on Architecture silence.

CURRENT STATE:
- Phase 0 / 1A / 1B / 1C / 1D / 1E ✅; Phase 1F PENDING.
- Progress: 27 / 35 → 77.1%.
- 15 derivations registries; 83 spec-derived items.
- 2 LOAD-BEARING open questions (Q10, Q11) — unchanged.
- 62 cross-phase gap rows accumulated (#1 – #62).
- **3 LOAD-BEARING audit-injection flags** for Phase 1F: TaskEngine + ReminderScheduler + TrayHost
  (all must register IComplianceCore in ctor).

FIRST ACTIONS — Phase 1F:
1. Read PHASE1E-DERIVATIONS §20 + PHASE1E-MSG2-DERIVATIONS-DELTA §19 first.
2. Read all 3 ctor LOAD-BEARING flags:
   - TaskEngine R6 (PHASE1A §4) — needs IComplianceCore
   - ReminderScheduler HALT-Msg2 #2 — needs IComplianceCore (3rd param of 4)
   - TrayHost HALT-Msg1 #2 — needs IComplianceCore (2nd param of 2)
3. Begin Phase 1F with HALT protocol.
4. Phase 1F scope per Roadmap Sub-Phase 1F:
   - src/TaskTree.Orchestrator/Orchestrator.cs (coordinates all modules per §3.2)
   - src/TaskTree.App/Bootstrap/CompositionRoot.cs (composition root)
   - src/TaskTree.App/Bootstrap/ServiceRegistrations.cs (DI registrations with 3 audit-injection flags)
   - tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs (deferred to Phase 1F Msg 2)
5. CRITICAL Phase 1F flags from accumulated gaps:
   - Gap #2 + #32 + #56: 3 audit-injection LOAD-BEARING
   - Gap #3: Register MasterKeyManager as singleton IMasterKeyManager
   - Gap #4 + #5: Canonical paths %LOCALAPPDATA%\TaskTree\{keys,store,logs}\
   - Gap #6 / Q11: Populate Q11 allowlist OR accept empty
   - Gap #38: Orchestrator shutdown SLA must respect 5s ReminderScheduler StopWaitTimeout
   - Gap #45: DI registration order
   - Gap #47: Orchestrator MUST use StartAsync/StopAsync only (never TickOnceAsync)
   - Gap #48: Production MUST NOT mutate StopWaitTimeout
   - Gap #57: Live TrayHost audit posture deferred to Phase 5E
6. Surface HALT items for:
   - DI container choice (Microsoft.Extensions.DependencyInjection 8.0 per §12)
   - Composition root vs ServiceRegistrations split
   - Singleton lifetimes (per §3.2 dependency graph)
   - Orchestrator role: event hub + lifecycle coordinator + shutdown sequencer
   - Q11 allowlist population (owner decision)
   - Phase 1G ReminderDelivery deferred? (Roadmap §1G is separate sub-phase)

After HALT + owner approval, generate:
- SPEC-DERIVED-PHASE1F markers
- docs/spec-derivations/PHASE1F-DERIVATIONS.md
- Updated tools/find-spec-derivations.ps1
- HANDOFF.md v1.0.20 additive delta
```

---

## §I — Document History Append

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.19 | 2026-05-28 | DSW | **Phase 1E Msg 2 emitted; Sub-Phase 1E ✅ COMPLETE.** 13 HALT items batch-resolved (16 tests + csproj patch + Architecture v1.0.2 delta update with 3 bundled changes). 2 new cross-phase gap rows (61-62). Zero patches to Msg 1 production code (test-friendly design from Msg 1). Progress 26/35 → 27/35 (77.1%). |

---

## §J — Footer

- This is an **additive delta**. To produce v1.0.19 consolidated: rebase v1.0.16 + v1.0.17 + v1.0.18 + v1.0.19 deltas sequentially onto v1.0.15 flat merge.
- **Companion docs:** Architecture.md v1.0.1 (+ pending v1.0.2 amendment with 3 bundled changes), Roadmap.md v1.0.0.
- **Registry added:** `docs/spec-derivations/PHASE1E-MSG2-DERIVATIONS-DELTA.md`.
- **Architecture v1.0.2 delta document UPDATED** — 3 bundled changes (§3.3 + §4.3 + §4.1).
- **Helper script updated:** `tools/find-spec-derivations.ps1` — 12 marker buckets (PHASE1E bumped 3→5; PHASE1E-MSG2 = 2).
- **Total derivations tracked:** 83 across 15 registries.
- **LOAD-BEARING blockers for Phase 5F:** Q10, Q11 — unchanged.
- **Phase 1E ✅.** Next action: Phase 1F HALT — Orchestrator + CompositionRoot + ServiceRegistrations.
