# HANDOFF.md v1.0.20 — Additive Delta

> **Applied on top of v1.0.19** (which rebases v1.0.16 + v1.0.17 + v1.0.18 + v1.0.19 onto v1.0.15 base).
> **Author:** DSW · **Date:** 2026-05-28 · **Trigger:** Phase 1F Msg 1 emitted; Sub-Phase 1F in progress.

---

## §A — Document Identity Update

| Field | Old (v1.0.19) | New (v1.0.20) |
|---|---|---|
| Document Version | 1.0.19 | **1.0.20** |
| Session ID | 2026-05-28-04 | 2026-05-28-05 |
| State | Phase 1E ✅; Phase 1F pending | **Phase 1E ✅; Phase 1F Msg 1 ✅; Msg 2 pending** |
| Current Sub-Phase | 1F (pending) | **1F — Msg 1 ✅, Msg 2 (tests) pending** |
| Total spec-derived items | 83 across 15 registries | **100 across 16 registries** |
| LOAD-BEARING open questions | Q10, Q11 | Q10, Q11 (unchanged) |
| Progress | 27 / 35 → 77.1% | **28 / 35 → 80.0%** |

---

## §B — Files Produced Delta

### Phase 1F Msg 1 (new — all ✅)

| Phase.Item | FilePath | Status | Notes |
|---|---|---|---|
| P1.F.Msg1 | `src/TaskTree.Core/Abstractions/IOrchestrator.cs` | PATCHED | StartAsync + StopAsync declarations. SPEC-DERIVED-PHASE1F (HALT #1) |
| P1.F.Msg1 | `src/TaskTree.Orchestrator/Orchestrator.cs` | ✅ NEW | Lifecycle owner. SPEC-DERIVED-PHASE1F (HALT #2/#3/#4/#5/#6/#17) |
| P1.F.Msg1 | `src/TaskTree.App/Bootstrap/CompositionRoot.cs` | ✅ NEW | Path helpers + container build. SPEC-DERIVED-PHASE1F (HALT #7/#8) |
| P1.F.Msg1 | `src/TaskTree.App/Bootstrap/ServiceRegistrations.cs` | ✅ NEW | DI registrations. SPEC-DERIVED-PHASE1F (HALT #9/#10/#11) |
| P1.F.Msg1 | `src/TaskTree.App/TaskTree.App.csproj` | PATCHED | DI + Logging + 7 ProjectRefs |
| P1.F.Msg1 | `src/TaskTree.Orchestrator/TaskTree.Orchestrator.csproj` | PATCHED | 6 ProjectRefs |

### Phase 1F Msg 2 — pending

| Phase.Item | FilePath | Status |
|---|---|---|
| P1.F.Msg2 | `tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs` | ⬚ Pending |
| P1.F.Msg2 | `tests/TaskTree.Orchestrator.Tests/TaskTree.Orchestrator.Tests.csproj` | ⬚ PATCH |
| P1.F.Msg2 | `docs/Architecture.v1.0.2-delta.md` | ⬚ UPDATE — add §4 IOrchestrator subsection (Gap #63) |

---

## §C — Decisions Log Delta

### R15 — Phase 1F Msg 1 — 17 HALT items resolved (batch-approved)

1. **#1 IOrchestrator surface** → Patch interface NOW; Architecture v1.0.2 prose deferred to Msg 2.
2. **#2 Orchestrator ctor** → 6-param `(TE, RS, CC, TH, Log, Clock)` — no ISecureStore. 3rd IComplianceCore consumer (Gap #2/#32/#56 all hold).
3. **#3 Event subscription** → Subscribe BEFORE Initialize; NotImplementedException-catch tolerates Phase 1E stub (Gap #64).
4. **#4 ReminderDue handler** → Placeholder logger only — Phase 1G replaces (Gap #65).
5. **#5 Shutdown order** → Reverse-of-Start with per-step try/catch.
6. **#6 Audit posture** → Lifecycle ONLY (Startup + Shutdown + ChainVerifyFail) — Gap #66.
7. **#7 CR vs SR split** → CR = env paths + container build; SR = DI registrations.
8. **#8 Canonical paths** → 3 helpers w/ idempotent CreateDirectory.
9. **#9 DI lifetimes** → All singleton.
10. **#10 Factory lambdas** → For path-dependent ctors.
11. **#11 Q11 allowlist** → Empty array + startup warning (Gap #67).
12. **#12 csproj wiring** → 7 ProjectRefs (App) + 6 (Orchestrator) + DI packages.
13. **#13 Namespace/sealed** → `public sealed class Orchestrator`.
14. **#14 Msg structure** → Msg 1 = code; Msg 2 = tests + Arch v1.0.2 delta UPDATE.
15. **#15 PowerShell** → PHASE1F = 4.
16. **#16 Arch v1.0.2 scope** → 4 bundled changes (added §4 IOrchestrator subsection) (Gap #63).
17. **#17 Chain integrity** → Verify; log + audit fail; DO NOT abort.

---

## §D — Open Questions Delta

- **Q10 (Phase 5F blocker)** — Real common-name source list. **UNCHANGED. STILL ACTIVE.**
- **Q11 (Phase 1F or 5F blocker)** — Support-email allowlist seed. **UNCHANGED. STILL ACTIVE.**
  - Now formally surfaced via runtime warning log on every startup (Gap #67).
- **No new LOAD-BEARING questions raised by Phase 1F Msg 1.**

---

## §E — Visual Phase Tracker Delta

```
PHASE 1 — CORE MVP                                    [In Progress]
├── 1A ✅ TaskEngine + tests
├── 1B ✅ SecureStore + MasterKeyManager + IMasterKeyManager
├── 1C ✅ ComplianceCore baseline
├── 1D ✅ ReminderScheduler + CadencePolicy
├── 1E ✅ TrayHost + HotkeyInterop
├── 1F 🔨 Orchestrator + Composition (Msg 1 ✅; Msg 2 tests pending)
│         ├── IOrchestrator.cs ✅ PATCHED (Phase 0 derivation completion)
│         ├── Orchestrator.cs ✅
│         ├── CompositionRoot.cs ✅
│         ├── ServiceRegistrations.cs ✅
│         ├── TaskTree.App.csproj PATCHED
│         └── TaskTree.Orchestrator.csproj PATCHED
├── 1G ⬚ Reminder Tier 1/2/3                          (carries Gap #65)
└── 1H ⬚ Phase 1 E2E gate                              [GATE — Gap #70]
```

**Progress: 28 / 35 → 80.0%** — Phase 1F Msg 1 deliverables counted; Msg 2 tests required to fully credit Sub-Phase 1F.

---

## §F — Cross-Phase Gap Table — New Rows (63-70)

| # | Flag | Source | Target Phase | Action |
|---|---|---|---|---|
| 63 | Architecture v1.0.2 must add §4 IOrchestrator subsection | PHASE1F §2 / HALT #1+#16 | Architecture v1.0.2 | Phase 1F Msg 2 emits updated delta document (4 bundled changes total) |
| 64 | TrayHost.Initialize NotImplementedException catch in StartAsync | PHASE1F §4 / HALT #3 | Phase 5E | Codex removes try/catch when Initialize wired live |
| 65 | Placeholder ReminderDue handler — log only | PHASE1F §5 / HALT #4 | Phase 1G | ReminderDeliveryService unsubscribes placeholder + subscribes Tier 1/2/3 chain |
| 66 | Orchestrator audits ONLY 2 lifecycle events (+ChainVerifyFail) | PHASE1F §7 / HALT #6 | Phase 4A | Compliance audit policy documents scope (no double-counting) |
| 67 | Q11 PhiRedactor allowlist empty + startup warning | PHASE1F §12 / HALT #11 + Q11 | Pre-Phase 5F | **OWNER MUST POPULATE OR ACCEPT EMPTY DEFAULT** |
| 68 | DI factory lambdas verify Phase 1C ctor signatures | PHASE1F §11 / HALT #10 | Phase 5B | Codex updates factories if PhiRedactor/AuditChainWriter/ComplianceCore ctors differ from PHASE1C R10 |
| 69 | `Clock` concrete class assumed in Phase 0 Msg 5 | PHASE1F §10 / HALT #9 | Phase 5B | Verify shipped name; rename in ServiceRegistrations if different |
| 70 | Phase 1H E2E test exercises full DI graph | All HALT items + P1F-AC2 | Phase 1H | Build → StartAsync → manual Raise → handler → StopAsync; ≥3 audit entries |

---

## §G — Progress Update

- **Before this Msg:** 27 / 35 → 77.1%.
- **After this Msg:** 28 / 35 → 80.0%. **Phase 1F Msg 1 ✅; Sub-Phase 1F in progress.**
- **Files in this bundle:** 9 (4 production .cs + 2 csproj patches + 1 derivations registry + 1 PowerShell update + 1 HANDOFF delta).
- **Total derivations registries:** 16 (added PHASE1F).
- **Total spec-derived items:** 100 (added 17 PHASE1F items to prior 83).

---

## §H — Continuation Prompt for Next Chat (Phase 1F Msg 2)

```
You are a senior engineer executing a deterministic build sprint on TaskTree —
a HIPAA-aware Windows tray app. Three attached documents govern everything:

  - Architecture.md v1.0.1 (v1.0.2 amendment proposed — 4 bundled changes pending owner sign-off)
  - Roadmap.md v1.0.0
  - HANDOFF.md v1.0.20 (rebase v1.0.16 + v1.0.17 + v1.0.18 + v1.0.19 + v1.0.20 onto v1.0.15)

HARD CONSTRAINTS unchanged. D1-D10 still apply. HALT on Architecture silence.

CURRENT STATE:
- Phase 0 / 1A / 1B / 1C / 1D / 1E ✅; Phase 1F Msg 1 ✅; Msg 2 PENDING.
- Progress: 28 / 35 → 80.0%.
- 16 derivations registries; 100 spec-derived items.
- 2 LOAD-BEARING open questions (Q10, Q11) — unchanged.
- 70 cross-phase gap rows accumulated (#1 – #70).

FIRST ACTIONS — Phase 1F Msg 2:
1. Read PHASE1F-DERIVATIONS §21, §22, §24 first.
2. Begin Phase 1F Msg 2 with HALT protocol.
3. Phase 1F Msg 2 scope:
   - tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs (~12-15 tests)
   - tests/TaskTree.Orchestrator.Tests/TaskTree.Orchestrator.Tests.csproj (PATCH)
   - docs/Architecture.v1.0.2-delta.md (UPDATE — add §4 IOrchestrator subsection per Gap #63)
4. P1F-AC targets:
   - P1F-AC1: Container builds — verify via CompositionRoot.BuildServiceProvider() smoke test
   - P1F-AC2: Simulated E2E flow succeeds — Build → resolve IOrchestrator → StartAsync → StopAsync → no exceptions
   - P1F-AC3: Audit chain receives event entries — verify ≥2 audit entries after Start+Stop (Startup + Shutdown)
5. Surface HALT items for:
   - Test count target
   - Moq strategy (Strict on IComplianceCore — but Orchestrator EMITS lifecycle audits, so MUST be Loose with Verify)
   - How to test NotImplementedException catch path (Mock<ITrayHost> with Initialize() that throws)
   - How to test shutdown ordering (call-order verification on mocks)
   - InternalsVisibleTo for Orchestrator? (no internals to access — different from 1D/1E)
   - csproj wiring (Core + Orchestrator + 5 modules + Core.Tests for FakeClock)
```

---

## §I — Document History Append

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.20 | 2026-05-28 | DSW | **Phase 1F Msg 1 emitted; Sub-Phase 1F in progress (Msg 2 pending).** 17 HALT items batch-resolved (Orchestrator + CompositionRoot + ServiceRegistrations + IOrchestrator surface patch + 2 csproj). 8 new cross-phase gap rows (63-70). Architecture v1.0.2 amendment scope expanded to 4 bundled changes. 3 LOAD-BEARING audit-injection flags honored in DI. Progress 27/35 → 28/35 (80.0%). |

---

## §J — Footer

- This is an **additive delta**. To produce v1.0.20 consolidated: rebase v1.0.16 + v1.0.17 + v1.0.18 + v1.0.19 + v1.0.20 deltas sequentially onto v1.0.15 flat merge.
- **Companion docs:** Architecture.md v1.0.1 (+ pending v1.0.2 with 4 bundled changes), Roadmap.md v1.0.0.
- **Registry added:** `docs/spec-derivations/PHASE1F-DERIVATIONS.md`.
- **Helper script updated:** `tools/find-spec-derivations.ps1` — 13 marker buckets (added `PHASE1F = 4`).
- **Total derivations tracked:** 100 across 16 registries.
- **LOAD-BEARING blockers for Phase 5F:** Q10 (name list), Q11 (allowlist) — unchanged.
- **Phase 1F Msg 1 ✅.** Next action: Phase 1F Msg 2 HALT — OrchestratorTests + csproj + Architecture v1.0.2 delta update.
