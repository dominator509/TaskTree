# HANDOFF.md — v1.0.18 Additive Delta

**Predecessor:** v1.0.17 (Phase 1D COMPLETE delta)
**Date:** 2026-05-27
**Phase advance:** Phase 1E Msg 1 ✅ (Msg 2 pending — tests)
**Author:** DSW (via owner batch approval of 9 HALT items)

ADDITIVE delta — merges on top of v1.0.15 consolidated + v1.0.16 + v1.0.17.

---

## §Document Identity — UPDATE

- Document Version: **1.0.18**
- State: **Phase 1E Msg 1 ✅** (HIGH-stub code complete); Msg 2 (tests) pending
- Architecture.md Version: **1.0.2** (no further amendments this msg)
- Current Sub-Phase: Phase 1E — Msg 2 (tests) pending
- Total spec-derived items: **54 across 13 registries** (+11 PHASE1E derivations)
- LOAD-BEARING open questions: **3 active** (Q10, Q11, Q12)
- Progress: **25.5 of 35 → 72.9%** (1E Msg 1 counted as half of 1E)

---

## §Files Produced — APPEND (Phase 1E Msg 1)

| Phase.Item | FilePath | Status | Notes |
|---|---|---|---|
| P1.E.Msg1 | src/TaskTree.Modules.TrayHost/TrayHost.cs | ✅ | SPEC-DERIVED-PHASE1E §1, §3, §4, §8, §9 |
| P1.E.Msg1 | src/TaskTree.Modules.TrayHost/HotkeyInterop.cs | ✅ | SPEC-DERIVED-PHASE1E §2, §5, §6, §7 |
| P1.E.Msg1 | src/TaskTree.Modules.TrayHost/Properties/AssemblyInfo.cs | ✅ | SPEC-DERIVED-PHASE1E §10 |
| P1.E.Msg1 | src/TaskTree.Modules.TrayHost/TaskTree.Modules.TrayHost.csproj | ✅ AMENDED | H.NotifyIcon.Wpf 2.0.108 PackageReference (G1E-H1 flag) |
| P1.E.Msg1 | docs/spec-derivations/PHASE1E-DERIVATIONS.md | ✅ NEW | 11 derivations + 20 gaps |
| P1.E.Msg1 | tools/find-spec-derivations.ps1 | ✅ UPDATED | Adds SPEC-DERIVED-PHASE1E = 3 |

### Phase 1E Msg 2 — pending
| Phase.Item | FilePath | Status |
|---|---|---|
| P1.E.Msg2 | tests/TaskTree.Modules.TrayHost.Tests/TrayHostTests.cs | ⬚ |

---

## §Decisions Log — APPEND R13

### R13 (2026-05-27) — Phase 1E Msg 1 — 9-item HALT batch, all owner-approved

1. HALT #1 → ctor `TrayHost(IAppLogger logger)` only
2. HALT #2 → HotkeyInterop API (WM_HOTKEY + HotkeyModifiers flags + 2 DllImports + HotkeyId enum)
3. HALT #3 → Option A: internal `Raise*()` helpers + `[InternalsVisibleTo]` (mirrors G1D-20 pattern)
4. HALT #4 → sync `void Dispose()` per §4.1 verbatim
5. HALT #5 → Option A: pin `H.NotifyIcon.Wpf 2.0.108` now; flag G1E-H1 for Phase 5A restore validation
6. HALT #6 → HotkeyId enum: ShowTree=0x9001, AddTask=0x9002 reserved
7. HALT #7 → Option A: strict 1E/2A boundary; no chord literals in 1E
8. HALT #8 → `public sealed class TrayHost` for pattern consistency
9. HALT #9 → Option A: ShowBalloon throws NotImplementedException with explicit Codex 5E reason

---

## §Open Questions — STATE

No new open questions in Msg 1. Q10, Q11, Q12 remain LOAD-BEARING.

---

## §Visual Phase Tracker — UPDATE

```
PHASE 1 — CORE MVP [In Progress]
├── 1A ✅ TaskEngine
├── 1B ✅ SecureStore + MasterKeyManager + IMasterKeyManager
├── 1C ✅ ComplianceCore baseline
├── 1D ✅ ReminderScheduler + CadencePolicy + tests (41 tests)
├── 1E ⏳ Msg 1 ✅ TrayHost HIGH-stub + HotkeyInterop; Msg 2 ⬚ tests
│       [G1E-H1: H.NotifyIcon.Wpf 2.0.108 — Phase 5A restore validates]
│       [G1E-E1..E8: all live PInvoke + NotifyIcon body — Codex 5E]
├── 1F ⬚ Orchestrator wiring
│       [G1D-12: Dispatcher.Invoke on ReminderDue]
│       [G1D-14: AutoLogoffTriggered → ReminderScheduler.StopAsync]
│       [G1D-18: TaskDeleted → ReminderScheduler purge]
│       [G1E-S1: STA thread for TrayHost.Initialize]
│       [G1E-S4: TrayHost events marshal as needed]
├── 1G ⬚ Reminder Tier 1/2/3 [consumes ReminderReason; G1E-S3 Focus Assist detection]
└── 1H ⬚ Phase 1 E2E gate
```

---

## §Consolidated Gap Summary — APPEND 20 NEW ROWS (G1E-E1..E8 + H1..H4 + S1..S8)

| # | Flag | Source | Target Phase | Action |
|---|---|---|---|---|
| 69 | G1E-E1 Live TaskbarIcon construction | PHASE1E §Ledger | 5E | Body of Initialize() — 5E.4 |
| 70 | G1E-E2 Message-only HWND via CreateWindowEx(HWND_MESSAGE) | PHASE1E §Ledger | 5E | Body of Initialize() — 5E.1 |
| 71 | G1E-E3 WndProc subclassing for WM_HOTKEY routing | PHASE1E §Ledger | 5E | Body of Initialize() — 5E.2 |
| 72 | G1E-E4 Live RegisterHotKey/UnregisterHotKey calls | PHASE1E §Ledger | 5E | Initialize + Dispose bodies |
| 73 | G1E-E5 Live ShowBalloon body | PHASE1E §Ledger | 5E | ShowBalloon body |
| 74 | G1E-E6 Dispose ordering | PHASE1E §Ledger | 5E | Dispose body |
| 75 | G1E-E7 VK_T = 0x54 constant for Ctrl+Alt+T | PHASE1E §Ledger | 5E | Initialize body |
| 76 | G1E-E8 Icon resource .ico file | PHASE1E §Ledger | 4D or 5E | Asset production |
| 77 | G1E-H1 H.NotifyIcon.Wpf 2.0.108 may not resolve | PHASE1E N2 | 5A | Bump or float per D3 |
| 78 | G1E-H2 IAppLogger.LogInformation shape (G1D-6) | PHASE1E N3 | 5B | Verify + adapt |
| 79 | G1E-H3 ITrayHost EventHandler non-generic shape | PHASE1E §Ledger | 5B | Verify §4.1 + document for 1F |
| 80 | G1E-H4 net8.0-windows TFM required for UseWPF | PHASE1E §Ledger | 5A | Verify Phase 0 Msg 1 csproj |
| 81 | G1E-S1 STA thread for NotifyIcon + msg-only HWND | PHASE1E N4 | 1F | Orchestrator STA init |
| 82 | G1E-S2 Hotkey conflict detection | PHASE1E §Ledger | 2A | Surface RegisterHotKey false |
| 83 | G1E-S3 Focus Assist detection for Tier routing | PHASE1E §Ledger | 1G | Routing logic in Orchestrator |
| 84 | G1E-S4 Tray click marshaling | PHASE1E §Ledger | 1F | Document in 1F derivations |
| 85 | G1E-S5 NotifyIcon orphan recovery on crash | PHASE1E §Ledger | 4A | OS-level limitation; doc §16 |
| 86 | G1E-S6 HotkeyConfig validation | PHASE1E §Ledger | 2A | Defensive validation |
| 87 | G1E-S7 Context menu strings — Architecture silent | PHASE1E §Ledger | 2B/4D | Hard-code English; i18n future |
| 88 | G1E-S8 Hotkey scope: message-only window | PHASE1E §Ledger | 5E | Use msg window for clean lifecycle |

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
SPEC-DERIVED-PHASE1D  = 5
SPEC-DERIVED-PHASE1E  = 3   # NEW: TrayHost.cs + HotkeyInterop.cs + AssemblyInfo.cs
# Total: 37 distinct .cs files (test files do NOT carry markers per Msg 6 convention)
```

---

## §Continuation Prompt — UPDATE for next chat

> CURRENT STATE:
> - Phase 0 ✅ + Phase 1A ✅ + Phase 1B ✅ + Phase 1C ✅ + Phase 1D ✅ + Phase 1E Msg 1 ✅
> - Phase 1E Msg 2 (TrayHostTests reflection-based event verification) pending
> - 54 spec-derived items across 13 registries
> - 3 LOAD-BEARING open questions (Q10, Q11, Q12)
> - Progress: 25.5/35 → 72.9%
> - Architecture.md is v1.0.2
>
> FIRST ACTION FOR NEXT SESSION: Phase 1E Msg 2 — produce TrayHostTests.cs verifying RaiseShowTreeRequested/RaiseAddTaskRequested/RaiseExitRequested + Initialize/ShowBalloon NotImplementedException + Dispose idempotence + null-arg guards.

---

## §Document History — APPEND

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.18 | 2026-05-27 | DSW | Phase 1E Msg 1 ✅ — TrayHost HIGH-stub + HotkeyInterop + AssemblyInfo + csproj amendment + 20 G1E-* gaps logged |

---

## End of HANDOFF.v1.0.18-delta.md
