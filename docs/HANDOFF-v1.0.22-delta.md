# HANDOFF.md v1.0.22 - Additive Delta

> **Applied on top of v1.0.21 delta.**
> **Author:** DSW · **Date:** 2026-05-28 · **Trigger:** Phase 1G Msg 1 emitted; Sub-Phase 1G in progress.

---

## §A - Document Identity Update

| Field | Old (v1.0.21) | New (v1.0.22) |
|---|---|---|
| Document Version | 1.0.21 | **1.0.22** |
| Session ID | 2026-05-28-06 | 2026-05-28-07 |
| State | Phase 1F COMPLETE; Phase 1G pending | **Phase 1F COMPLETE; Phase 1G Msg 1 emitted; Msg 2 pending** |
| Current Sub-Phase | 1G (pending) | **1G - Msg 1 done, Msg 2 pending** |
| Total spec-derived items | 115 across 17 registries | **133 across 18 registries** |
| LOAD-BEARING open questions | Q10, Q11 | Q10, Q11 (unchanged) |
| Progress | 29 / 35 -> 82.9% | **30 / 35 -> 85.7%** |

---

## §B - Files Produced Delta

### Phase 1G Msg 1 (new - all done)

| Phase.Item | FilePath | Status | Notes |
|---|---|---|---|
| P1.G.Msg1 | `src/TaskTree.Core/Enums/ReminderDeliveryTier.cs` | NEW | SPEC-DERIVED-PHASE1G (HALT #9) |
| P1.G.Msg1 | `src/TaskTree.Orchestrator/ReminderDeliveryService.cs` | NEW | SPEC-DERIVED-PHASE1G (HALT #2/#3/#4/#5/#10/#11/#12) |
| P1.G.Msg1 | `src/TaskTree.Orchestrator/ToastTier1Adapter.cs` | NEW | HIGH-stub. SPEC-DERIVED-PHASE1G (HALT #5/#8) |
| P1.G.Msg1 | `src/TaskTree.Orchestrator/ToastTier2Adapter.cs` | NEW | SPEC-DERIVED-PHASE1G (HALT #4/#6/#7/#8/#13/#14/#15/#16) |
| P1.G.Msg1 | `src/TaskTree.Orchestrator/Views/ToastTier2Window.xaml` | NEW | Vanilla WPF XAML |
| P1.G.Msg1 | `src/TaskTree.Orchestrator/Views/ToastTier2Window.xaml.cs` | NEW | SPEC-DERIVED-PHASE1G (HALT #6/#7/#14) |
| P1.G.Msg1 | `src/TaskTree.Orchestrator/ToastTier3Adapter.cs` | NEW | HIGH-stub. SPEC-DERIVED-PHASE1G (HALT #5/#8) |
| P1.G.Msg1 | `src/TaskTree.Orchestrator/TaskTree.Orchestrator.csproj` | PATCHED | UseWPF=true |

### Phase 1G Msg 2 - pending

| Phase.Item | FilePath | Status |
|---|---|---|
| P1.G.Msg2 | `tests/TaskTree.Orchestrator.Tests/ReminderDeliveryServiceTests.cs` | Pending |
| P1.G.Msg2 | `tests/TaskTree.Orchestrator.Tests/ToastTier2AdapterTests.cs` (Integration) | Pending |
| P1.G.Msg2 | `src/TaskTree.Orchestrator/Orchestrator.cs` | PATCH (Gap #83) |
| P1.G.Msg2 | `tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs` | PATCH (Gap #83) |
| P1.G.Msg2 | `src/TaskTree.App/Bootstrap/ServiceRegistrations.cs` | PATCH (Gap #78) |
| P1.G.Msg2 | `docs/Architecture.v1.0.2-delta.md` | UPDATE - 5th bundled change (Gap #81) |

---

## §C - Decisions Log Delta

### R17 - Phase 1G Msg 1 - 18 HALT items resolved (batch-approved)

1. **#1 Folder placement** -> `src/TaskTree.Orchestrator/` per Roadmap (Gap #76).
2. **#2 IReminderDeliveryService** -> No interface; concrete class only (Gap #77).
3. **#3 Service ctor** -> 7-param `(RS, Tier1, Tier2, Tier3, Clock, Logger, Compliance)` - 4th audit-injection (Gap #78).
4. **#4 Decision tree** -> `bool TryDeliver` cascade fallthrough.
5. **#5 Tier 1+3 HIGH-stub** -> Throw NotImplementedException; narrow-catch in service (Gap #79).
6. **#6 Tier 2 impl** -> `ToastTier2Window.xaml` + `.xaml.cs` vanilla WPF (Gap #80).
7. **#7 UI library** -> Vanilla WPF (no ModernWpfUI in stub).
8. **#8 Adapter ctors** -> Tier1/2=(IAppLogger); Tier3=(ITrayHost, IAppLogger).
9. **#9 Tier enum** -> New `src/TaskTree.Core/Enums/ReminderDeliveryTier.cs` (Gap #81).
10. **#10 Audit posture** -> DeliveredViaTier1/2/3 + DeliveryFailedAllTiers (Gap #82).
11. **#11 Subscription** -> Service owns; Orchestrator placeholder removed in Msg 2 (Gap #65/#83).
12. **#12 Lifecycle** -> Start/Stop mirrors ReminderScheduler.
13. **#13 Public surface** -> `public sealed class` for all.
14. **#14 Trivial-Overdue UX** -> Soften title; full copy 2B (Gap #84).
15. **#15 Tier 2 reuse + auto-close** -> Single window + 10s timer (Gap #85).
16. **#16 Adapter Dispose** -> All 3 IDisposable.
17. **#17 Msg structure** -> Msg 1 code; Msg 2 tests + 4 patches.
18. **#18 PowerShell** -> PHASE1G = 6.

---

## §D - Open Questions Delta

- **Q10 (Phase 5F blocker)** - UNCHANGED. STILL ACTIVE.
- **Q11 (Phase 1F/5F blocker)** - UNCHANGED. STILL ACTIVE.
- **No new LOAD-BEARING questions raised.**
- **Architecture v1.0.2 amendment scope:** now expanding to 5 bundled changes (Gap #81); document UPDATED in Phase 1G Msg 2.

---

## §E - Visual Phase Tracker Delta

```
PHASE 1 - CORE MVP                                    [In Progress]
+-- 1A [done] TaskEngine + tests
+-- 1B [done] SecureStore + MasterKeyManager + IMasterKeyManager
+-- 1C [done] ComplianceCore baseline
+-- 1D [done] ReminderScheduler + CadencePolicy
+-- 1E [done] TrayHost + HotkeyInterop
+-- 1F [done] Orchestrator + Composition
+-- 1G [in-progress] Reminder Tier 1/2/3 (Msg 1 done; Msg 2 tests + 4 patches pending)
|         +-- ReminderDeliveryTier.cs [done]
|         +-- ReminderDeliveryService.cs [done]
|         +-- ToastTier1Adapter.cs [done] HIGH-stub
|         +-- ToastTier2Adapter.cs [done]
|         +-- Views/ToastTier2Window.xaml [done]
|         +-- Views/ToastTier2Window.xaml.cs [done]
|         +-- ToastTier3Adapter.cs [done] HIGH-stub
|         +-- TaskTree.Orchestrator.csproj PATCHED (UseWPF=true)
+-- 1H [pending] Phase 1 E2E gate [GATE - Gap #70 approaching]
```

**Progress: 30 / 35 -> 85.7%** - Phase 1G Msg 1 deliverables counted; Msg 2 tests + 4 patches required.

---

## §F - Cross-Phase Gap Table - New Rows (76-85)

Appended to v1.0.21 75-row table.

| # | Flag | Source | Target Phase | Action |
|---|---|---|---|---|
| 76 | Folder placement drift documentation | Roadmap §1G enforcement | (informational) | Documented; no action |
| 77 | No IReminderDeliveryService interface | HALT #2 | Phase 2B+ | Promote if mocking needed |
| 78 | ServiceRegistrations.cs register 4 new singletons | HALT #3 | Phase 1G Msg 2 | Patch SR (Gap #83) |
| 79 | NotImplementedException narrow-catch in cascade | HALT #5 | Phase 5E | Codex removes Tier 1+3 catches |
| 80 | Phase 2B ReminderToast.xaml replaces Tier 2 window | HALT #6 | Phase 2B | Refactor to UI project XAML |
| 81 | Architecture v1.0.2 5th change ReminderDeliveryTier | HALT #9 | Architecture v1.0.2 | Phase 1G Msg 2 delta UPDATE |
| 82 | Phase 4A audit policy DeliveredVia/Failed vocab | HALT #10 | Phase 4A | Document audit vocabulary |
| 83 | **Phase 1F Orchestrator + tests + SR + Arch v1.0.2 4-patch batch** | HALT #11 | Phase 1G Msg 2 | **LOAD-BEARING** |
| 84 | Phase 2B UX copy + localization | HALT #14 | Phase 2B | UX copy review |
| 85 | Tier 2 auto-close 10s hard-coded | HALT #15 | Phase 2E | Settings panel configurable |

---

## §G - Progress Update

- **Before this Msg:** 29 / 35 -> 82.9%.
- **After this Msg:** 30 / 35 -> 85.7%. **Phase 1G Msg 1 emitted.**
- **Files in this bundle:** 11 (6 .cs production + 1 XAML + 1 csproj patch + 1 derivations registry + 1 HANDOFF delta + 1 PowerShell tool update).
- **Total derivations registries:** 18 (added PHASE1G).
- **Total spec-derived items:** 133 (added 18 PHASE1G items to prior 115).

---

## §H - Continuation Prompt for Next Chat (Phase 1G Msg 2)

```
You are a senior engineer executing a deterministic build sprint on TaskTree -
a HIPAA-aware Windows tray app. Three attached documents govern everything:

  - Architecture.md v1.0.1 (v1.0.2 amendment proposed - 5 bundled changes pending owner sign-off)
  - Roadmap.md v1.0.0
  - HANDOFF.md v1.0.22

HARD CONSTRAINTS unchanged. D1-D10 still apply. HALT on Architecture silence.

CURRENT STATE:
- Phase 0 / 1A / 1B / 1C / 1D / 1E / 1F COMPLETE; Phase 1G Msg 1 done; Msg 2 PENDING.
- Progress: 30 / 35 -> 85.7%.
- 18 derivations registries; 133 spec-derived items.
- 2 LOAD-BEARING open questions (Q10, Q11) - unchanged.
- 85 cross-phase gap rows accumulated (#1 - #85).

CRITICAL Phase 1G Msg 2 patch batch (Gap #83):
1. src/TaskTree.Orchestrator/Orchestrator.cs - extend ctor 6->7 params + remove
   OnReminderDuePlaceholder + call _reminderDeliveryService.StartAsync(ct) after
   _reminderScheduler.StartAsync(ct) + call _reminderDeliveryService.StopAsync()
   before _reminderScheduler.StopAsync() in reverse-order shutdown
2. tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs - all 14 tests Build helper
   extends to 7-param ctor; shutdown-order test verifies RDS.Stop before RS.Stop
3. src/TaskTree.App/Bootstrap/ServiceRegistrations.cs - add 4 new singletons
   (ToastTier1Adapter, ToastTier2Adapter, ToastTier3Adapter, ReminderDeliveryService)
   in correct order (after ITrayHost, before IOrchestrator)
4. docs/Architecture.v1.0.2-delta.md - add Change 5 (§3.3 ReminderDeliveryTier.cs)
   REQUIRED IF PHASE 1G SHIPS

FIRST ACTIONS - Phase 1G Msg 2:
1. Read PHASE1G-DERIVATIONS §22 (Gap rows 76-85) + §23 (Msg 2 Patch Manifest) first.
2. Begin Phase 1G Msg 2 with HALT protocol.
3. Scope: ~12 tests + integration tests for Tier 2 WPF + 4 patches.
4. Surface HALT items: test count, Moq strategy (Strict on IComplianceCore per Gap
   #72 pattern), cascade 4-input-combo tests for P1G-AC1, Tier 2 [STAThread] strategy,
   audit verification, csproj wiring verification, Architecture v1.0.2 5th change wording.

After HALT + owner approval:
- Generate test files + 4 patch files
- Update tools/find-spec-derivations.ps1 (PHASE1G bump 6 to higher; PHASE1G-MSG2 new)
- HANDOFF.md v1.0.23 - Phase 1G COMPLETE
- Phase 1H E2E gate approaches (Gap #70)
```

---

## §I - Document History Append

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.22 | 2026-05-28 | DSW | **Phase 1G Msg 1 emitted; Sub-Phase 1G in progress.** 18 HALT items batch-resolved (ReminderDeliveryTier enum + ReminderDeliveryService cascade + 3 adapters + Tier 2 WPF XAML + code-behind + csproj UseWPF patch). 10 new cross-phase gap rows (76-85). Architecture v1.0.2 amendment scope expanded to 5 bundled changes. **Cross-Phase Gap #83 LOAD-BEARING - Phase 1G Msg 2 must apply 4-patch batch.** Progress 29/35 -> 30/35 (85.7%). |

---

## §J - Footer

- This is an **additive delta**. To produce v1.0.22 consolidated: rebase v1.0.16 through v1.0.22 deltas sequentially onto v1.0.15 flat merge.
- **Companion docs:** Architecture.md v1.0.1 (+ pending v1.0.2 with 5 bundled changes), Roadmap.md v1.0.0.
- **Registry added:** `docs/spec-derivations/PHASE1G-DERIVATIONS.md`.
- **Helper script updated:** `tools/find-spec-derivations.ps1` - 15 marker buckets (added `PHASE1G = 6`).
- **Total derivations tracked:** 133 across 18 registries.
- **LOAD-BEARING blockers for Phase 5F:** Q10, Q11 - unchanged. Architecture v1.0.2 owner sign-off (5 changes).
- **Phase 1G Msg 1 done.** Next action: Phase 1G Msg 2 HALT - tests + 4-patch batch (Gap #83) + Architecture v1.0.2 delta UPDATE (Gap #81).
