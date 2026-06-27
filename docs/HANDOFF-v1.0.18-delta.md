# HANDOFF.md v1.0.18 — Additive Delta

> **Applied on top of v1.0.17 delta** (which was applied on top of v1.0.16 delta + v1.0.15 consolidated).
> This is an additive delta only — to produce a consolidated v1.0.18, rebase v1.0.16 + v1.0.17 + v1.0.18 deltas sequentially onto v1.0.15 flat merge.
> **Author:** DSW · **Date:** 2026-05-28 · **Trigger:** Phase 1E Msg 1 emitted; Sub-Phase 1E in progress.

---

## §A — Document Identity Update

| Field | Old (v1.0.17) | New (v1.0.18) |
|---|---|---|
| Document Version | 1.0.17 | **1.0.18** |
| Session ID | 2026-05-28-02 | 2026-05-28-03 |
| State | Phase 1D ✅; Phase 1E pending | **Phase 1D ✅; Phase 1E Msg 1 ✅; Msg 2 pending** |
| Current Sub-Phase | 1E (pending) | **1E — Msg 1 ✅, Msg 2 pending** |
| Total spec-derived items | 55 across 13 registries | **70 across 14 registries** |
| LOAD-BEARING open questions | Q10, Q11 | Q10, Q11 (unchanged) |
| Progress | 25 / 35 → 71.4% | **26 / 35 → 74.3%** |

---

## §B — Files Produced Delta

### Phase 1E Msg 1 (new — all ✅)

| Phase.Item | FilePath | Status | Notes |
|---|---|---|---|
| P1.E.Msg1 | `src/TaskTree.Modules.TrayHost/TrayHost.cs` | ✅ NEW | HIGH-stub. SPEC-DERIVED-PHASE1E (HALT #2/#3/#4/#5/#7/#8/#9/#10/#12) |
| P1.E.Msg1 | `src/TaskTree.Modules.TrayHost/HotkeyInterop.cs` | ✅ NEW | PInvoke sigs + `BuildModifierFlags` impl. SPEC-DERIVED-PHASE1E (HALT #1/#6) |
| P1.E.Msg1 | `src/TaskTree.Modules.TrayHost/Properties/AssemblyInfo.cs` | ✅ NEW | `InternalsVisibleTo` grant. SPEC-DERIVED-PHASE1E (HALT #5/#15) |

### Phase 1E Msg 2 — pending

| Phase.Item | FilePath | Status |
|---|---|---|
| P1.E.Msg2 | `tests/TaskTree.Modules.TrayHost.Tests/TrayHostTests.cs` | ⬚ Pending |
| P1.E.Msg2 | `tests/TaskTree.Modules.TrayHost.Tests/HotkeyInteropTests.cs` | ⬚ Pending |
| P1.E.Msg2 | `tests/TaskTree.Modules.TrayHost.Tests/TaskTree.Modules.TrayHost.Tests.csproj` | ⬚ PATCH |
| P1.E.Msg2 | `docs/Architecture.v1.0.2-delta.md` | ⬚ UPDATE (Gap #60 — §4.1 internal-raise blessing) |

---

## §C — Decisions Log Delta

### R13 — Phase 1E Msg 1 — 15 HALT items resolved (batch-approved)

1. **#1 HotkeyInterop surface** → Hybrid A+C — PInvoke sigs present, Register/Unregister stubbed, `BuildModifierFlags` implemented.
2. **#2 TrayHost ctor** → `(IAppLogger, IComplianceCore)` — **3rd audit-injection LOAD-BEARING flag** for Phase 1F (after TaskEngine R6 + ReminderScheduler HALT-Msg2 #2).
3. **#3 Audit timing in stub** → `_compliance` stored only; no `AuditAsync` calls in stub.
4. **#4 Throw text** → Canonical `"HIGH: ... — Codex Phase 5E"` format for greppable Phase 5E closure.
5. **#5 P1E-AC2 raise mechanism** → 3 internal `Raise*()` methods + `InternalsVisibleTo` (parallels Phase 1D Msg 2 HALT-Msg2 #1).
6. **#6 HotkeyInterop visibility** → `public static class`.
7. **#7 TrayHost shape** → `public sealed class TrayHost : ITrayHost, IDisposable`.
8. **#8 Dispose** → real idempotent (no Win32 owned in stub state).
9. **#9 ShowBalloon validation** → validate params first, then throw NotImplementedException.
10. **#10 Initialize idempotency** → none in stub (Gap #59 for Phase 5E).
11. **#11 Default hotkey constant** → deferred to Phase 2A `HotkeyConfig`.
12. **#12 `_initialized` field** → declared; Codex Phase 5E sets to true.
13. **#13 Msg structure** → Msg 1 = stubs + docs; Msg 2 = tests + Architecture v1.0.2 update.
14. **#14 find-spec-derivations.ps1** → add `PHASE1E = 3`.
15. **#15 Architecture v1.0.2 scope** → bundle TrayHost internal-raise blessing into existing pending amendment.

---

## §D — Open Questions Delta

- **Q10 (Phase 5F blocker)** — Real common-name source list. **UNCHANGED. STILL ACTIVE.**
- **Q11 (Phase 1F or 5F blocker)** — Support-email allowlist seed. **UNCHANGED. STILL ACTIVE.**
- **No new LOAD-BEARING questions raised by Phase 1E Msg 1.**

Item #2 (IComplianceCore injection) is a LOAD-BEARING **flag** for Phase 1F (third such flag), not a Q-numbered owner-decision blocker; recorded as Cross-Phase Gap #56.

---

## §E — Visual Phase Tracker Delta

```
PHASE 1 — CORE MVP                                    [In Progress]
├── 1A ✅ TaskEngine + tests
├── 1B ✅ SecureStore + MasterKeyManager + IMasterKeyManager
├── 1C ✅ ComplianceCore baseline
├── 1D ✅ ReminderScheduler + CadencePolicy
├── 1E 🔨 TrayHost (Msg 1 ✅ stubs; Msg 2 tests pending)
│         ├── TrayHost.cs ✅ HIGH-stub (Initialize + ShowBalloon throw NotImplementedException)
│         ├── HotkeyInterop.cs ✅ HIGH-stub (Register/Unregister stubbed; PInvoke sigs + BuildModifierFlags implemented)
│         └── Properties/AssemblyInfo.cs ✅ InternalsVisibleTo grant
├── 1F ⬚ Orchestrator wiring                           [CRITICAL: 3 audit-injection LOAD-BEARING flags]
├── 1G ⬚ Reminder Tier 1/2/3
└── 1H ⬚ Phase 1 E2E gate                              [GATE]
```

**Progress: 26 / 35 → 74.3%** — Phase 1E Msg 1 deliverables counted; Msg 2 tests required to fully credit Sub-Phase 1E.

---

## §F — Cross-Phase Gap Table — New Rows (56-60)

Appended to the v1.0.17 55-row table.

| # | Flag | Source | Target Phase | Action |
|---|---|---|---|---|
| 56 | Inject `IComplianceCore` into `TrayHost` ctor | PHASE1E §3 / HALT #2 | Phase 1F | **LOAD-BEARING — 3rd audit injection** (TaskEngine + ReminderScheduler + TrayHost) |
| 57 | Live TrayHost handlers MUST call `AuditAsync` on each event | PHASE1E §4 / HALT #3 | Phase 5E | Codex: `Module="TrayHost"`, `Action="<EventName>"`, `Result="success"` |
| 58 | `InternalsVisibleTo` scope = Tests-only | PHASE1E §6 / HALT #5 | Phase 5B | Compile gate verifies 1 attribute line |
| 59 | `Initialize` idempotency check needed | PHASE1E §11 / HALT #10 | Phase 5E | Codex adds `InvalidOperationException` guard |
| 60 | Architecture v1.0.2 must bless internal-raise pattern | PHASE1E §16 / HALT #15 | Architecture v1.0.2 | Phase 1E Msg 2 emits updated delta document |

---

## §G — Progress Update

- **Before this Msg:** 25 / 35 → 71.4%.
- **After this Msg:** 26 / 35 → 74.3%. **Phase 1E Msg 1 ✅; Sub-Phase 1E in progress.**
- **Files in this bundle:** 6 (3 production + 1 derivations registry + 1 PowerShell tool update + 1 HANDOFF delta).
- **Total derivations registries:** 14 (added PHASE1E).
- **Total spec-derived items:** 70 (added 15 PHASE1E items to prior 55).

---

## §H — Continuation Prompt for Next Chat (Phase 1E Msg 2)

```
You are a senior engineer executing a deterministic build sprint on TaskTree —
a HIPAA-aware Windows tray app. Three attached documents govern everything:

  - Architecture.md v1.0.1 (v1.0.2 amendment proposed — scope expanded per Gap #60)
  - Roadmap.md v1.0.0
  - HANDOFF.md v1.0.18 (rebase v1.0.16 + v1.0.17 + v1.0.18 deltas onto v1.0.15 base)

HARD CONSTRAINTS unchanged. D1-D10 still apply. HALT on Architecture silence.

CURRENT STATE:
- Phase 0 / 1A / 1B / 1C / 1D ✅; Phase 1E Msg 1 ✅; Msg 2 PENDING.
- Progress: 26 / 35 → 74.3%.
- 14 derivations registries; 70 spec-derived items.
- 2 LOAD-BEARING open questions (Q10, Q11) — unchanged.
- 60 cross-phase gap rows accumulated (#1 – #60).

FIRST ACTIONS — Phase 1E Msg 2:
1. Read PHASE1E-DERIVATIONS.md §17, §19, §20, §21 first.
2. Begin Phase 1E Msg 2 with HALT protocol.
3. Phase 1E Msg 2 scope:
   - tests/TaskTree.Modules.TrayHost.Tests/TrayHostTests.cs
     (null-arg ctor x2, Dispose idempotency, ShowBalloon validation matrix,
      Initialize throws NotImplementedException, ObjectDisposedException flow,
      internal Raise*() event verification x3)
   - tests/TaskTree.Modules.TrayHost.Tests/HotkeyInteropTests.cs
     (BuildModifierFlags exhaustive matrix, Register/Unregister throw NotImplementedException,
      modifier constants exposed and correct)
   - tests/TaskTree.Modules.TrayHost.Tests/TaskTree.Modules.TrayHost.Tests.csproj
     (PATCH with ProjectReferences: Core + TrayHost + Core.Tests)
   - docs/Architecture.v1.0.2-delta.md
     (UPDATE — extend §1 to add §4.1 internal-raise blessing per Gap #60)
4. Acceptance criteria:
   - P1E-AC1: TrayHost compiles ✅ (already from Msg 1)
   - P1E-AC2: Events raise correctly (internal Raise*() invoked; verify event handlers fire)
   - P1E-AC3: Live methods stubbed (verify Initialize + ShowBalloon + HotkeyInterop.Register/Unregister
              all throw NotImplementedException with canonical text format)
5. Surface HALT items for:
   - Test count target (~14 tests across 2 files?)
   - HotkeyInterop static-class testing strategy
   - Whether to verify exact NotImplementedException message text via Contains() or just exception type
   - csproj wiring (Core + TrayHost + Core.Tests for FakeClock if needed for any tests)
   - InternalsVisibleTo verification (compile-time only or runtime test?)
   - Architecture v1.0.2 delta document update — confirm §4.1 prose wording

After HALT + owner approval, generate:
- SPEC-DERIVED-PHASE1E-MSG2 markers
- docs/spec-derivations/PHASE1E-MSG2-DERIVATIONS-DELTA.md
- Updated tools/find-spec-derivations.ps1 (PHASE1E bumped 3 → ~5; PHASE1E-MSG2 = ~4)
- HANDOFF.md v1.0.19 additive delta — Phase 1E fully ✅
- Updated docs/Architecture.v1.0.2-delta.md
```

---

## §I — Document History Append

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.18 | 2026-05-28 | DSW | **Phase 1E Msg 1 emitted; Sub-Phase 1E in progress (Msg 2 pending).** 15 HALT items batch-resolved (TrayHost HIGH-stub + HotkeyInterop PInvoke + AssemblyInfo InternalsVisibleTo). 5 new cross-phase gap rows (56-60). Architecture v1.0.2 amendment scope expanded to include §4.1 internal-raise pattern blessing. Progress 25/35 → 26/35 (74.3%). |

---

## §J — Footer

- This is an **additive delta**. To produce v1.0.18 consolidated: rebase v1.0.16 + v1.0.17 + v1.0.18 deltas sequentially onto v1.0.15 flat merge.
- **Companion docs:** Architecture.md v1.0.1 (+ pending v1.0.2 amendment with expanded scope), Roadmap.md v1.0.0.
- **Registry added:** `docs/spec-derivations/PHASE1E-DERIVATIONS.md`.
- **Helper script updated:** `tools/find-spec-derivations.ps1` — 11 marker buckets (added `PHASE1E = 3`).
- **Total derivations tracked:** 70 across 14 registries.
- **LOAD-BEARING blockers for Phase 5F:** Q10 (name list), Q11 (support-email allowlist) — unchanged.
- **Phase 1E Msg 1 ✅.** Next action: Phase 1E Msg 2 HALT — TrayHostTests + HotkeyInteropTests + csproj patch + Architecture v1.0.2 delta update.
