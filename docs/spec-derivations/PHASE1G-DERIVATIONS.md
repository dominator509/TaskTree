# PHASE1G-DERIVATIONS.md - Phase 1G Msg 1 (ReminderDeliveryService + Tier 1/2/3 Adapters)

> **Scope:** ReminderDeliveryTier enum + ReminderDeliveryService + ToastTier1Adapter + ToastTier2Adapter + ToastTier2Window.xaml/.cs + ToastTier3Adapter + csproj patch.
> **Companion docs:** Architecture.md v1.0.1 (v1.0.2 amendment expanding to 5 changes), Roadmap.md v1.0.0, HANDOFF.md v1.0.22.
> **Owner-approved HALT batch:** 18 items, batch-resolved.

---

## §1 Summary Table

| # | Item | Resolution | Files | LOAD-BEARING? |
|---|---|---|---|---|
| 1 | Folder placement | `src/TaskTree.Orchestrator/` per Roadmap | All 4 | No (Gap #76) |
| 2 | IReminderDeliveryService interface | None - concrete class only | ReminderDeliveryService.cs | No (Gap #77) |
| 3 | Service ctor deps | 7-param (RS + 3 adapters + Clock + Logger + Compliance) | ReminderDeliveryService.cs | **YES Gap #78** |
| 4 | Tier 1/2/3 decision tree | Cascade via `bool TryDeliver` | ReminderDeliveryService.cs | No |
| 5 | Tier 1+3 HIGH-stub | Throw NotImplementedException; narrow-catch in service | All 3 adapters | No (Gap #79) |
| 6 | Tier 2 implementation | Programmatic WPF + XAML window | Tier2 + XAML + .xaml.cs | No (Gap #80) |
| 7 | Tier 2 UI library | Vanilla WPF (no ModernWpfUI in stub) | XAML | No |
| 8 | Adapter ctor deps | Tier1/2=(IAppLogger); Tier3=(ITrayHost, IAppLogger) | All 3 adapters | No |
| 9 | Tier enum location | `src/TaskTree.Core/Enums/ReminderDeliveryTier.cs` | NEW enum | No (Gap #81) |
| 10 | Audit posture | DeliveredViaTier1/2/3 + DeliveryFailedAllTiers | ReminderDeliveryService.cs | No (Gap #82) |
| 11 | Subscription mechanism | Service owns; Orchestrator placeholder removed in Msg 2 | ReminderDeliveryService.cs | **YES Gap #65/#83** |
| 12 | Lifecycle (Start/Stop) | Mirrors ReminderScheduler pattern | ReminderDeliveryService.cs | No |
| 13 | Public surface | `public sealed class` for all 4 | All | No |
| 14 | Trivial-Overdue UX | Soften title; full copy 2B | ToastTier2Window.xaml.cs | No (Gap #84) |
| 15 | Tier 2 reuse + auto-close | Single window + 10s timer | ToastTier2Adapter.cs | No (Gap #85) |
| 16 | Adapter Dispose | All 3 IDisposable | All 3 adapters | No |
| 17 | Msg structure | Msg 1 code; Msg 2 tests + patches | (process) | No |
| 18 | PowerShell update | Add `PHASE1G = 6` | tools/find-spec-derivations.ps1 | No |

---

## §2 Item #1 - Folder Placement
**Trigger:** Roadmap §1G places files under `src/TaskTree.Orchestrator/`. My Phase 1F Msg 2 §H prompt suggested a new module folder.
**Resolution:** Follow Roadmap (D4 authority).
**Files:** All 4 production files under `TaskTree.Orchestrator/`.
**Gap for Handoff:** Cross-Phase Gap #76 - informational only; drift documented, no action.

## §3 Item #2 - IReminderDeliveryService Interface
**Trigger:** No Phase 0 stub; not in §4 module specs.
**Resolution:** Concrete class only.
**Files:** ReminderDeliveryService.cs.
**Gap for Handoff:** Cross-Phase Gap #77 - Phase 2B+ may promote to interface if mocking needed.

## §4 Item #3 - Service Constructor Dependencies
**Trigger:** Service needs scheduler + adapters + audit + logger + clock.
**Resolution:** 7-param `(IReminderScheduler, ToastTier1Adapter, ToastTier2Adapter, ToastTier3Adapter, IClock, IAppLogger, IComplianceCore)`.
**Files:** ReminderDeliveryService.cs.
**Gap for Handoff:** Cross-Phase Gap #78 - Phase 1G Msg 2 MUST patch ServiceRegistrations.cs to register 4 new singletons.

## §5 Item #4 - Decision Tree
**Trigger:** P1G-AC1.
**Resolution:** Each adapter has `bool TryDeliver(ReminderEvent)`. Service cascades 1->2->3.
**Files:** ReminderDeliveryService.cs DeliverAsync.
**Gap for Handoff:** None. Phase 1G Msg 2 tests verify 4 input combos.

## §6 Item #5 - HIGH-Stub Throw Behavior
**Trigger:** P1G-AC3 mandates Tier 1+3 throw NotImplementedException.
**Resolution:** Tier 1+3 throw; service catches ONLY NotImplementedException (narrow).
**Files:** All 3 adapters + ReminderDeliveryService.cs.
**Gap for Handoff:** Cross-Phase Gap #79 - Phase 5E Codex removes Tier 1+3 catches.

## §7 Item #6 - Tier 2 Implementation
**Trigger:** P1G-AC2.
**Resolution:** `ToastTier2Window.xaml` + `.xaml.cs` (vanilla WPF) + Tier 2 adapter.
**Files:** XAML + code-behind + adapter + csproj UseWPF=true.
**Gap for Handoff:** Cross-Phase Gap #80 - Phase 2B refactor replaces with TaskTree.UI/Views/ReminderToast.xaml.

## §8 Item #7 - UI Library
**Resolution:** Vanilla WPF only.
**Files:** XAML.
**Gap for Handoff:** None.

## §9 Item #8 - Adapter Constructor Dependencies
**Resolution:** Tier1=(IAppLogger); Tier2=(IAppLogger); Tier3=(ITrayHost, IAppLogger).
**Files:** All 3 adapters.
**Gap for Handoff:** None.

## §10 Item #9 - Tier Enum Location
**Resolution:** NEW `src/TaskTree.Core/Enums/ReminderDeliveryTier.cs`.
**Files:** Enum file.
**Gap for Handoff:** Cross-Phase Gap #81 - Architecture v1.0.2 5th bundled change (§3.3 ReminderDeliveryTier.cs add); Phase 1G Msg 2 updates delta.

## §11 Item #10 - Audit Posture
**Resolution:** DeliveredViaTier1/2/3 (success) or DeliveryFailedAllTiers (failure).
**Files:** ReminderDeliveryService.cs.
**Gap for Handoff:** Cross-Phase Gap #82 - Phase 4A audit policy documents vocabulary.

## §12 Item #11 - Subscription Mechanism
**Trigger:** Gap #65.
**Resolution:** Service owns subscription in StartAsync.
**Files:** ReminderDeliveryService.cs.
**Gap for Handoff:** Cross-Phase Gap #83 - LOAD-BEARING Phase 1G Msg 2 patches required: Orchestrator.cs ctor 6->7 + remove placeholder + Start/Stop wiring; OrchestratorTests.cs Build helper extends 7-param; ServiceRegistrations.cs (#78); Architecture v1.0.2 delta (#81).

## §13 Item #12 - Lifecycle
**Resolution:** StartAsync(ct) + StopAsync() + IDisposable.
**Files:** ReminderDeliveryService.cs.
**Gap for Handoff:** None.

## §14 Item #13 - Public Surface
**Resolution:** `public sealed class` for all 4.
**Files:** All.
**Gap for Handoff:** None.

## §15 Item #14 - Trivial-Overdue UX
**Trigger:** Gap #50/#55.
**Resolution:** Soften title for Trivial+Overdue; full UX 2B.
**Files:** ToastTier2Window.xaml.cs UpdateContent.
**Gap for Handoff:** Cross-Phase Gap #84 - Phase 2B UX copy + localization.

## §16 Item #15 - Tier 2 Reuse + Auto-Close
**Resolution:** Single reusable window + Timer 10s.
**Files:** ToastTier2Adapter.cs.
**Gap for Handoff:** Cross-Phase Gap #85 - Phase 2E Settings panel configurable.

## §17 Item #16 - Dispose Contract
**Resolution:** All 3 IDisposable. Tier 2 closes window + timer.
**Files:** All 3 adapters.
**Gap for Handoff:** None.

## §18 Item #17 - Msg Structure
**Resolution:** Msg 1 code; Msg 2 tests + 4 patches.
**Files:** None (process).
**Gap for Handoff:** None.

## §19 Item #18 - PowerShell
**Resolution:** Add `PHASE1G = 6`. Grand total 53->59.
**Files:** tools/find-spec-derivations.ps1.
**Gap for Handoff:** None.

---

## §20 Files Produced This Msg

| Path | Purpose | Marker |
|---|---|---|
| `src/TaskTree.Core/Enums/ReminderDeliveryTier.cs` | NEW enum | `PHASE1G` |
| `src/TaskTree.Orchestrator/ReminderDeliveryService.cs` | Cascade service + audit | `PHASE1G` |
| `src/TaskTree.Orchestrator/ToastTier1Adapter.cs` | HIGH-stub | `PHASE1G` |
| `src/TaskTree.Orchestrator/ToastTier2Adapter.cs` | WPF window adapter | `PHASE1G` |
| `src/TaskTree.Orchestrator/Views/ToastTier2Window.xaml` | XAML markup | (XAML - no marker) |
| `src/TaskTree.Orchestrator/Views/ToastTier2Window.xaml.cs` | Code-behind | `PHASE1G` |
| `src/TaskTree.Orchestrator/ToastTier3Adapter.cs` | HIGH-stub | `PHASE1G` |
| `src/TaskTree.Orchestrator/TaskTree.Orchestrator.csproj` | PATCHED UseWPF=true | (csproj - no marker) |

---

## §21 SPEC-DERIVED-PHASE1G Marker Inventory

PHASE1G = **6 distinct .cs files**. Grand total 53 -> **59**.

---

## §22 Cross-Phase Gaps Introduced (rows 76-85)

| # | Gap | Source | Target Phase | Action |
|---|---|---|---|---|
| 76 | Folder placement drift in Phase 1F Msg 2 §H | Roadmap enforcement | (informational) | Documented |
| 77 | No IReminderDeliveryService interface | HALT #2 | Phase 2B+ | Promote if mocking needed |
| 78 | ServiceRegistrations.cs register 4 new singletons | HALT #3 | Phase 1G Msg 2 | Patch SR |
| 79 | NotImplementedException narrow-catch in cascade | HALT #5 | Phase 5E | Codex removes Tier 1+3 catches |
| 80 | Phase 2B ReminderToast.xaml replaces Tier 2 window | HALT #6 | Phase 2B | Refactor to UI project XAML |
| 81 | Architecture v1.0.2 5th change ReminderDeliveryTier | HALT #9 | Architecture v1.0.2 | Phase 1G Msg 2 delta UPDATE |
| 82 | Phase 4A audit policy DeliveredVia/Failed vocab | HALT #10 | Phase 4A | Document audit vocabulary |
| 83 | **Phase 1F Orchestrator+tests+SR+Arch v1.0.2 4-patch batch** | HALT #11 | Phase 1G Msg 2 | **LOAD-BEARING** |
| 84 | Phase 2B UX copy + localization | HALT #14 | Phase 2B | UX copy review |
| 85 | Tier 2 auto-close 10s hard-coded | HALT #15 | Phase 2E | Settings panel configurable |

---

## §23 Phase 1G Msg 2 Patch Manifest

| # | File | Patch |
|---|---|---|
| 1 | `src/TaskTree.Orchestrator/Orchestrator.cs` | Extend ctor 6->7 params (add ReminderDeliveryService); remove OnReminderDuePlaceholder + subscribe/unsubscribe; call `_reminderDeliveryService.StartAsync(ct)` after `_reminderScheduler.StartAsync(ct)`; call `_reminderDeliveryService.StopAsync()` before `_reminderScheduler.StopAsync()` |
| 2 | `tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs` | All 14 tests Build helper extends to 7-param ctor with mock; shutdown-order verifies RDS.Stop before RS.Stop |
| 3 | `src/TaskTree.App/Bootstrap/ServiceRegistrations.cs` | Add 4 singletons (ToastTier1Adapter + ToastTier2Adapter + ToastTier3Adapter + ReminderDeliveryService); order: after ITrayHost, before IOrchestrator |
| 4 | `docs/Architecture.v1.0.2-delta.md` | Add Change 5: §3.3 Enums adds ReminderDeliveryTier.cs (REQUIRED IF PHASE 1G SHIPS) |

---

## §24 Known Limitations

1. Tier 2 WPF testability constrained - requires [STAThread] + Application context; integration tests preferred over unit tests.
2. Tier 1+3 stub coverage gap until Phase 5E (only NotImplementedException-throw verifiable).
3. Programmatic Tier 2 window has no animation; Phase 2B XAML may add fade-in/out.
4. Audit vocabulary (DeliveredViaTier1/2/3, DeliveryFailedAllTiers) not yet documented in §10.5; Phase 4A scope.
5. ReminderDeliveryService is the 4th IComplianceCore consumer; SR patch required (Gap #78/#83).
6. Hard-coded 10s auto-close - Gap #85 defers to Phase 2E.
7. No snooze/escalation in Phase 1G (Phase 2G owns SnoozeService + EscalationPolicy).
8. Trivial-Overdue copy is minimum-viable until Phase 2B localization (Gap #84).
