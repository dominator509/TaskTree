# HANDOFF.md v1.0.21 — Additive Delta

> **Applied on top of v1.0.20 delta** (which rebases v1.0.16 + v1.0.17 + v1.0.18 + v1.0.19 + v1.0.20 onto v1.0.15 base).
> **Author:** DSW · **Date:** 2026-05-28 · **Trigger:** Phase 1F Msg 2 emitted; Sub-Phase 1F ✅ COMPLETE.

---

## §A — Document Identity Update

| Field | Old (v1.0.20) | New (v1.0.21) |
|---|---|---|
| Document Version | 1.0.20 | **1.0.21** |
| Session ID | 2026-05-28-05 | 2026-05-28-06 |
| State | Phase 1F Msg 1 ✅; Msg 2 pending | **Phase 1F ✅ COMPLETE; Phase 1G pending** |
| Current Sub-Phase | 1F — Msg 1 ✅, Msg 2 pending | **1G — ReminderDeliveryService Tier 1/2/3 (pending)** |
| Total spec-derived items | 100 across 16 registries | **115 across 17 registries** |
| LOAD-BEARING open questions | Q10, Q11 | Q10, Q11 (unchanged) |
| Progress | 28 / 35 → 80.0% | **29 / 35 → 82.9%** |

---

## §B — Files Produced Delta

### Phase 1F Msg 2 (Sub-Phase 1F ✅ closed)

| Phase.Item | FilePath | Status | Notes |
|---|---|---|---|
| P1.F.Msg2 | `tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs` | ✅ NEW | 14 tests. SPEC-DERIVED-PHASE1F-MSG2 |
| P1.F.Msg2 | `tests/TaskTree.Orchestrator.Tests/TaskTree.Orchestrator.Tests.csproj` | PATCHED | 9 ProjectReferences |
| P1.F.Msg2 | `docs/spec-derivations/PHASE1F-MSG2-DERIVATIONS-DELTA.md` | ✅ NEW | Registry |
| P1.F.Msg2 | `docs/Architecture.v1.0.2-delta.md` | UPDATED | 4 bundled changes (§3.3 + §4.3 + §4.1 + §4 NEW) — Gap #63 closure |
| P1.F.Msg2 | `tools/find-spec-derivations.ps1` | UPDATED | PHASE1F 4→5; PHASE1F-MSG2 = 1; grand total = 53 |

### Phase 1G — pending

| Phase.Item | FilePath | Status |
|---|---|---|
| P1.G | `src/TaskTree.Modules.ReminderDelivery/ReminderDeliveryService.cs` | ⬚ Pending |
| P1.G | `src/TaskTree.Modules.ReminderDelivery/ToastTier1Adapter.cs` (HIGH-stub) | ⬚ Pending |
| P1.G | `src/TaskTree.Modules.ReminderDelivery/ToastTier2Adapter.cs` | ⬚ Pending |
| P1.G | `src/TaskTree.Modules.ReminderDelivery/ToastTier3Adapter.cs` (HIGH-stub) | ⬚ Pending |

---

## §C — Decisions Log Delta

### R16 — Phase 1F Msg 2 — 15 HALT items resolved (batch-approved)

1. **#1 Test count** → 14 total (cohesive single file).
2. **#2 File structure** → Single OrchestratorTests.cs with logical regions.
3. **#3 IComplianceCore mock** → **Moq Strict** — converts Gap #66 to compile-time enforcement (Gap #72).
4. **#4 ITrayHost mock** → Strict + Initialize behavior toggle (default: throw NotImplementedException; override: no-op).
5. **#5 IReminderScheduler mock** → Strict + SetupAdd/Remove for ReminderDue event.
6. **#6 ITaskEngine mock** → Loose — future drift detector (Gap #73).
7. **#7 IClock** → **FakeClock 5th consumer** — defer Option B promotion (Gap #74).
8. **#8 IAppLogger mock** → Loose; verify only for specific tests.
9. **#9 NotImplementedException catch test** → 1 test (Gap #64 verification).
10. **#10 Other exception propagation test** → 1 anti-drift test (InvalidOperationException as proxy).
11. **#11 Chain-verify failure tests** → 2 tests (false + throws).
12. **#12 Reverse-order shutdown** → callOrder list via Callbacks.
13. **#13 E2E audit chain verification** → 2 tests (P1F-AC3).
14. **#14 CompositionRoot smoke** → 1 Integration-category test (Gap #71/#75).
15. **#15 Architecture v1.0.2 update** → 4 bundled changes (added §4 IOrchestrator) — Gap #63 closure.

---

## §D — Open Questions Delta

- **Q10 (Phase 5F blocker)** — Real common-name source list. **UNCHANGED. STILL ACTIVE.**
- **Q11 (Phase 1F or 5F blocker)** — Support-email allowlist seed. **UNCHANGED. STILL ACTIVE.** Surfaced via runtime warning log every startup.
- **No new LOAD-BEARING questions raised by Phase 1F Msg 2.**
- **Architecture v1.0.2 owner sign-off block now expanded to 4 changes** — Phase 5F-blocking if not approved.

---

## §E — Visual Phase Tracker Delta

```
PHASE 1 — CORE MVP                                    [In Progress]
├── 1A ✅ TaskEngine + tests
├── 1B ✅ SecureStore + MasterKeyManager + IMasterKeyManager
├── 1C ✅ ComplianceCore baseline
├── 1D ✅ ReminderScheduler + CadencePolicy
├── 1E ✅ TrayHost + HotkeyInterop
├── 1F ✅ Orchestrator + Composition (Msg 1 ✅ code; Msg 2 ✅ tests)
│         ├── IOrchestrator.cs ✅ PATCHED (Phase 0 derivation completion)
│         ├── Orchestrator.cs ✅
│         ├── CompositionRoot.cs ✅
│         ├── ServiceRegistrations.cs ✅
│         └── OrchestratorTests.cs ✅ (Msg 2 — 14 tests; 13 Offline + 1 Integration)
├── 1G ⬚ Reminder Tier 1/2/3                          — NEXT (Gap #65 placeholder unsubscribe)
└── 1H ⬚ Phase 1 E2E gate                              [GATE — Gap #70]
```

**Progress: 29 / 35 → 82.9%** — Phase 1F fully closed.

---

## §F — Cross-Phase Gap Table — New Rows (71-75)

Appended to v1.0.20 70-row table.

| # | Flag | Source | Target Phase | Action |
|---|---|---|---|---|
| 71 | CompositionRoot/SR get only smoke integration test | PHASE1F-MSG2 §15 / HALT-Msg2 #14 | Phase 1H | Deeper integration tests for env-specific paths |
| 72 | Moq Strict on IComplianceCore enforces lifecycle-only audit contract | PHASE1F-MSG2 §4 / HALT-Msg2 #3 | Phase 5E | Codex: switch to Loose + new Setup/Verify on real audit |
| 73 | Moq Loose on ITaskEngine — future drift detector | PHASE1F-MSG2 §7 / HALT-Msg2 #6 | Phase 1G / 2B | Switch to Strict when engine wired into tray events |
| 74 | **5th FakeClock consumer — Gap #40 re-evaluation REQUIRED** | PHASE1F-MSG2 §8 / HALT-Msg2 #7 | Phase 2A / 2B (6th consumer) | Promote to TaskTree.TestSupport project + Architecture v1.0.2 §3.3 amendment |
| 75 | P1F-AC1 smoke = Integration category | PHASE1F-MSG2 §15 / HALT-Msg2 #14 | Phase 5C | CI must include Integration filter |

---

## §G — Progress Update

- **Before this Msg:** 28 / 35 → 80.0%.
- **After this Msg:** 29 / 35 → 82.9%. **Phase 1F ✅ COMPLETE.**
- **Files in this bundle:** 6 (1 test file + 1 csproj patch + 1 derivations registry + 1 Architecture v1.0.2 delta UPDATE + 1 HANDOFF delta + 1 PowerShell tool UPDATE).
- **Total derivations registries:** 17 (added PHASE1F-MSG2).
- **Total spec-derived items:** 115 (added 15 PHASE1F-MSG2 items to prior 100).

---

## §H — Continuation Prompt for Next Chat (Phase 1G)

```
You are a senior engineer executing a deterministic build sprint on TaskTree —
a HIPAA-aware Windows tray app. Three attached documents govern everything:

  - Architecture.md v1.0.1 (v1.0.2 amendment proposed — 4 bundled changes pending owner sign-off)
  - Roadmap.md v1.0.0
  - HANDOFF.md v1.0.21 (rebase v1.0.16 through v1.0.21 deltas onto v1.0.15 base)

HARD CONSTRAINTS unchanged. D1-D10 still apply. HALT on Architecture silence.

CURRENT STATE:
- Phase 0 / 1A / 1B / 1C / 1D / 1E / 1F ✅; Phase 1G PENDING.
- Progress: 29 / 35 → 82.9%.
- 17 derivations registries; 115 spec-derived items.
- 2 LOAD-BEARING open questions (Q10, Q11) — unchanged.
- 75 cross-phase gap rows accumulated (#1 – #75).

CRITICAL Phase 1G integration points:
- Gap #65: Phase 1F placeholder ReminderDue handler must be unsubscribed when
  Phase 1G ReminderDeliveryService subscribes. Recommended: Phase 1G owns
  ReminderDue subscription; modify Orchestrator to skip placeholder when
  ReminderDeliveryService is registered, OR keep both (placeholder log is
  harmless duplicate diagnostic).
- Gap #50: Trivial-at-deadline UX semantic — adapter must surface "Overdue"
  reason for Trivial tasks at exact deadline.
- Gap #51: High/Normal/Low pre-deadline boundary fires at exact offset — UI
  tooltip must reflect "fires AT or before offset".
- Gap #55: UX copy must document Trivial-Overdue-at-deadline semantic.

FIRST ACTIONS — Phase 1G:
1. Read Architecture §7 Tier 1/2/3 fallback chain decision tree first.
2. Read PHASE1F-MSG2-DERIVATIONS-DELTA §19 (gap rows 71-75).
3. Begin Phase 1G with HALT protocol.
4. Phase 1G scope per Roadmap Sub-Phase 1G:
   - src/TaskTree.Modules.ReminderDelivery/ReminderDeliveryService.cs (subscribes
     to IReminderScheduler.ReminderDue; applies §7 Tier 1/2/3 decision tree)
   - src/TaskTree.Modules.ReminderDelivery/ToastTier1Adapter.cs (Windows Toast
     API — HIGH-stub per Roadmap; throws NotImplementedException for Codex Phase 5E)
   - src/TaskTree.Modules.ReminderDelivery/ToastTier2Adapter.cs (WPF custom
     window fallback — testable in chat env)
   - src/TaskTree.Modules.ReminderDelivery/ToastTier3Adapter.cs (NotifyIcon
     balloon via TrayHost.ShowBalloon — HIGH-stub since TrayHost.ShowBalloon
     is itself HIGH-stubbed)
   - tests deferred to Phase 1G Msg 2
5. Acceptance criteria:
   - P1G-AC1: ReminderDeliveryService compiles and implements
     IReminderDeliveryService interface (Phase 0 stub)
   - P1G-AC2: Tier 1/2/3 decision tree per Architecture §7
   - P1G-AC3: Live tiers stubbed per D5 (Tier 1 + Tier 3 throw
     NotImplementedException with canonical "HIGH:" prefix)
6. Surface HALT items for:
   - ReminderDeliveryService ctor deps (IReminderScheduler? + IClock + ITrayHost
     + IAppLogger + IComplianceCore?)
   - Tier 1/2/3 decision tree implementation (priority-driven per §5.3 + §7?)
   - Audit posture (delivery success/failure per fire?)
   - Phase 1F placeholder handler relationship (Gap #65 resolution)
   - Architecture v1.0.2 amendment scope (5th bundled change for ToastTier
     adapter pattern?)
   - csproj wiring + InternalsVisibleTo if needed

After HALT + owner approval, generate:
- SPEC-DERIVED-PHASE1G markers
- docs/spec-derivations/PHASE1G-DERIVATIONS.md
- Updated tools/find-spec-derivations.ps1
- HANDOFF.md v1.0.22 additive delta
```

---

## §I — Document History Append

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.21 | 2026-05-28 | DSW | **Phase 1F Msg 2 emitted; Sub-Phase 1F ✅ COMPLETE.** 15 HALT items batch-resolved (14 OrchestratorTests + csproj patch + Architecture v1.0.2 delta UPDATE with 4 bundled changes). 5 new cross-phase gap rows (71-75). Zero patches to Msg 1 production code. FakeClock 5th consumer (Gap #74 escalates Gap #40). Progress 28/35 → 29/35 (82.9%). |

---

## §J — Footer

- This is an **additive delta**. To produce v1.0.21 consolidated: rebase v1.0.16 through v1.0.21 deltas sequentially onto v1.0.15 flat merge.
- **Companion docs:** Architecture.md v1.0.1 (+ pending v1.0.2 amendment with 4 bundled changes), Roadmap.md v1.0.0.
- **Registry added:** `docs/spec-derivations/PHASE1F-MSG2-DERIVATIONS-DELTA.md`.
- **Architecture v1.0.2 delta document UPDATED** — 4 bundled changes (§3.3 + §4.3 + §4.1 + §4 NEW IOrchestrator subsection).
- **Helper script updated:** `tools/find-spec-derivations.ps1` — 14 marker buckets (PHASE1F bumped 4→5; PHASE1F-MSG2 = 1).
- **Total derivations tracked:** 115 across 17 registries.
- **LOAD-BEARING blockers for Phase 5F:** Q10, Q11 — unchanged. Architecture v1.0.2 owner sign-off (4 changes).
- **Phase 1F ✅.** Next action: Phase 1G HALT — ReminderDeliveryService + Toast Tier 1/2/3 Adapters.
