# HANDOFF.md v1.0.25 - Phase 2A Msg 1 Delta

> Applied on top of v1.0.24 delta. Author: DSW. Date: 2026-05-29.
> Trigger: Phase 2A Msg 1 emitted; FakeClock + InMemorySecureStore promoted to TaskTree.TestSupport per Gap #97/#98.

## Section A Identity Update

| Field | Old (v1.0.24) | New (v1.0.25) |
|---|---|---|
| Document Version | 1.0.24 | **1.0.25** |
| Session ID | 2026-05-28-09 | 2026-05-29-01 |
| State | Phase 1 COMPLETE; owner gate pending | **Phase 1 COMPLETE (implicit approval Gap #100); Phase 2A Msg 1 done; Msg 2 pending; TestSupport project created** |
| Current Sub-Phase | Phase 1 GATE | **Phase 2A Msg 1 done; Phase 2A Msg 2 pending (Architecture v1.0.2 update)** |
| Total spec-derived items | 172 / 20 registries | **194 / 21 registries** |
| LOAD-BEARING open questions | Q10, Q11 (active) | Q10 active + **Q11 implicit empty default (Gap #102)** |
| Progress | 33/35 (94.3%) | **33/35 (94.3%); Phase 2A Msg 1 contributes to Phase 2 tracking** |

## Section B Files Produced Delta

### Phase 2A Msg 1

| Phase.Item | FilePath | Status |
|---|---|---|
| P2.A.Msg1 | src/TaskTree.Modules.TrayHost/HotkeyManager.cs | NEW HIGH-stub |
| P2.A.Msg1 | src/TaskTree.Modules.TrayHost/HotkeyConfig.cs | NEW record |
| P2.A.Msg1 | src/TaskTree.App/Bootstrap/ServiceRegistrations.cs | PATCHED add HotkeyManager singleton |
| P2.A.Msg1 | tests/TaskTree.TestSupport/TaskTree.TestSupport.csproj | NEW project (Gap #97/#98) |
| P2.A.Msg1 | tests/TaskTree.TestSupport/FakeClock.cs | RELOCATED (Gap #97) |
| P2.A.Msg1 | tests/TaskTree.TestSupport/InMemorySecureStore.cs | RELOCATED (Gap #98) |
| P2.A.Msg1 | tests/TaskTree.Modules.TrayHost.Tests/HotkeyManagerTests.cs | NEW 10 tests |
| P2.A.Msg1 | 5 test csproj files | PATCHED with TestSupport ref |
| P2.A.Msg1 | docs/spec-derivations/PHASE2A-DERIVATIONS.md | NEW registry |
| P2.A.Msg1 | docs/HANDOFF-v1.0.25-delta.md | THIS FILE |
| P2.A.Msg1 | tools/find-spec-derivations.ps1 | UPDATED 18 buckets |

### Phase 2A Msg 2 pending

| Phase.Item | FilePath | Status |
|---|---|---|
| P2.A.Msg2 | docs/Architecture.v1.0.2-delta.md | UPDATE to 7 bundled changes (Gap #105) |
| P2.A.Msg2 | Using-statement migration manifest | NEW per Gap #106 (Phase 5A action) |
| P2.A.Msg2 | docs/Architecture.md | Final v1.0.2 promotion upon owner sign-off (Gap #110) |

## Section C Decisions Log Delta R20 - Phase 2A Msg 1 - 22 HALT items

1. **#1 Phase 1 owner approval** -> Implicit per directive (Gap #100 rollback path documented)
2. **#2 Architecture v1.0.2 sign-off** -> 5 REQUIRED implicitly accepted; Change 2 OPTIONAL deferred to v1.0.3 (Gap #101)
3. **#3 Q11 status** -> Empty default implicit (Gap #102)
4. **#4 Q10** -> Remains DEFERRED to Phase 5F
5. **#5 TaskTree.TestSupport project** -> NEW csproj
6. **#6 FakeClock relocation** -> Moved + namespace change (Gap #103)
7. **#7 InMemorySecureStore relocation** -> Moved + namespace change (Gap #104)
8. **#8 Architecture v1.0.2 7th change** -> Add TaskTree.TestSupport to section 3.3 (Gap #105)
9. **#9 5 test csproj patches** -> Add TestSupport ProjectReference + using migration manifest (Gap #106)
10. **#10 HotkeyManager class shape** -> public sealed : IDisposable; 4-param ctor (Gap #107 5th audit consumer)
11. **#11 HotkeyManager surface** -> 5 methods + 1 event
12. **#12 HotkeyConfig record** -> record with Default static
13. **#13 HotkeyRegistrationResult** -> Nested enum (Gap #108 avoids 8th Arch v1.0.2 change)
14. **#14 HotkeyChangedEventArgs** -> Nested class
15. **#15 Default hotkey constant** -> Resolved via HotkeyConfig.Default (closes Phase 1E Gap #11)
16. **#16 Storage key** -> "hotkeys/config"
17. **#17 Audit posture** -> HotkeyConfigChanged + HotkeyConflictDetected (Gap #109)
18. **#18 InitializeAsync canonical text** -> "HIGH: RegisterHotKey + message-only HWND require live env - Codex Phase 5E"
19. **#19 HotkeyManagerTests count** -> 10 tests
20. **#20 csproj** -> TrayHost.Tests.csproj PATCHED
21. **#21 Msg structure** -> Msg 1 = production + tests; Msg 2 = Architecture v1.0.2 update + final promotion (Gap #110)
22. **#22 PowerShell** -> PHASE2A = 5

## Section D Open Questions Delta

- **Q10 (Phase 5F blocker)** - Real common-name source list. STILL ACTIVE.
- **Q11 (Phase 2 + Phase 5F blocker)** - **Implicit empty default per Gap #102.** Owner can override pre-Phase 5F.
- **Gap #97 + #98 LOAD-BEARING for Phase 2A start** - CLOSED in this Msg via TaskTree.TestSupport promotion.
- **Architecture v1.0.2 amendment** - 5 REQUIRED implicitly accepted; expanded to 7 bundled changes (added TaskTree.TestSupport section 3.3 per Gap #105); final promotion in Phase 2A Msg 2 (Gap #110).

## Section E Visual Phase Tracker Delta

```
PHASE 1 - CORE MVP                                    [COMPLETE - implicit approval Gap #100]
+-- 1A [DONE] TaskEngine + 14 tests
+-- 1B [DONE] SecureStore + MasterKeyManager + IMasterKeyManager
+-- 1C [DONE] ComplianceCore baseline
+-- 1D [DONE] ReminderScheduler + CadencePolicy
+-- 1E [DONE] TrayHost + HotkeyInterop
+-- 1F [DONE] Orchestrator + Composition
+-- 1G [DONE] Reminder Tier 1/2/3
+-- 1H [DONE] E2E Final Gate

PHASE 2 - UX + EXTENSIBILITY                          [In Progress]
+-- 2A [IN PROGRESS - Msg 1 done, Msg 2 pending] HotkeyManager + HotkeyConfig + TaskTree.TestSupport
|         +-- HotkeyManager.cs [DONE HIGH-stub]
|         +-- HotkeyConfig.cs [DONE]
|         +-- TaskTree.TestSupport.csproj [DONE]
|         +-- FakeClock.cs [DONE relocated Gap #97]
|         +-- InMemorySecureStore.cs [DONE relocated Gap #98]
|         +-- HotkeyManagerTests.cs [DONE 10 tests]
|         +-- 5 test csproj [PATCHED]
|         +-- ServiceRegistrations.cs [PATCHED]
+-- 2B [pending] TreeViewUI
+-- 2C [pending] TaskMetadata + TaskBuilder
+-- 2D [pending] Theme + Color
+-- 2E [pending] SettingsService
+-- 2F [pending] SessionLock
+-- 2G [pending] SnoozeService + EscalationPolicy
```

**Progress: 33 / 35 -> 94.3% + Phase 2A Msg 1 done.**

## Section F Cross-Phase Gap Table - New Rows (100-110)

| # | Flag | Source | Target Phase | Status |
|---|---|---|---|---|
| 100 | Implicit Phase 1 approval; explicit sign-off pending | HALT #1 | Pre-Phase 5F | DOCUMENTED |
| 101 | Architecture v1.0.2 implicit 5 REQUIRED; Change 2 deferred to v1.0.3 | HALT #2 | Pre-Phase 5F | IMPLICIT |
| 102 | Q11 empty default implicit | HALT #3 | Pre-Phase 5F | IMPLICIT |
| 103 | FakeClock old location Phase 5A cleanup | HALT #6 | Phase 5A | DEFERRED |
| 104 | InMemorySecureStore old location Phase 5A cleanup | HALT #7 | Phase 5A | DEFERRED |
| 105 | Architecture v1.0.2 7th change (TaskTree.TestSupport in section 3.3) | HALT #8 | Architecture v1.0.2 | Msg 2 emits |
| 106 | Using-statement migration manifest (4 test files) | HALT #9 | Phase 5A | DEFERRED to Msg 2 |
| 107 | HotkeyManager 5th audit injection consumer | HALT #10 | (closed in SR patch) | CLOSED |
| 108 | HotkeyRegistrationResult nested enum decision | HALT #13 | (closed) | CLOSED |
| 109 | Phase 4A audit vocab HotkeyConfigChanged + HotkeyConflictDetected | HALT #17 | Phase 4A | DEFERRED |
| 110 | Phase 2A Msg 2 Architecture v1.0.2 final promotion | HALT #21 | Architecture v1.0.2 | Msg 2 emits |

### LOAD-BEARING gaps now active

- Q10 (Phase 5F blocker) - unchanged
- Gap #48 (StopWaitTimeout production mutation guard) - Phase 5C
- Gap #56 (TrayHost IComplianceCore injection LOAD-BEARING already satisfied by Phase 1F SR)
- Architecture v1.0.2 amendment (7 bundled changes) - Phase 5F gate
- Gap #100 (Phase 1 formal sign-off) - Pre-Phase 5F
- Gap #105 (Architecture v1.0.2 7th change) - Phase 2A Msg 2

## Section G Progress Update

- **Before:** 33 / 35 (94.3%) Phase 1 only tracking.
- **After:** 33 / 35 (94.3%) Phase 1 + Phase 2A Msg 1 done. Phase 2 sub-phase tracking will start with HANDOFF v1.0.26.
- **Files in bundle:** 15.
- **Total derivations registries:** 21.
- **Total spec-derived items:** 194 (added 22 PHASE2A items).

## Section H Continuation Prompt for Next Chat (Phase 2A Msg 2)

```
You are a senior engineer executing a deterministic build sprint on TaskTree.
Three attached documents govern everything:
  - Architecture.md v1.0.1 (pending v1.0.2 promotion - 7 bundled changes per Gap #105)
  - Roadmap.md v1.0.0
  - HANDOFF.md v1.0.25 (rebase v1.0.16-v1.0.25 onto v1.0.15)

HARD CONSTRAINTS unchanged. D1-D10 still apply. HALT on Architecture silence.

CURRENT STATE:
- Phase 1 COMPLETE (implicit approval Gap #100).
- Phase 2A Msg 1 done; Msg 2 PENDING.
- Progress: 33/35 (94.3%) + Phase 2A Msg 1 contribution.
- 21 derivations registries; 194 spec-derived items.
- 2 LOAD-BEARING open questions (Q10 active, Q11 implicit empty default Gap #102).
- 110 cross-phase gap rows accumulated.
- TaskTree.TestSupport project created (Gap #97/#98 CLOSED).

PHASE 2A Msg 2 SCOPE:
1. docs/Architecture.v1.0.2-delta.md - UPDATE to 7 bundled changes (add Change 7: section 3.3 add TaskTree.TestSupport)
2. docs/Phase5A-using-migration-manifest.md - NEW per Gap #106 listing 4 test files needing using statement migration (ReminderSchedulerTests.cs, ComplianceCoreTests.cs, AuditChainWriterTests.cs, OrchestratorTests.cs - replace using TaskTree.Core.Tests.TestDoubles with using TaskTree.TestSupport)
3. docs/Architecture.md - Final v1.0.2 promotion document upon owner sign-off (Gap #110) - this is a stretch goal if owner provides sign-off; otherwise Msg 2 just emits the updated delta + migration manifest

FIRST ACTIONS:
1. Read PHASE2A-DERIVATIONS section 26 (Gap rows 100-110) first.
2. Begin Phase 2A Msg 2 with HALT protocol.
3. Surface HALT items: Architecture v1.0.2 7th change wording, using-statement migration manifest format, retroactive Architecture.md promotion timing.

After HALT + owner approval:
- Generate Architecture v1.0.2 delta UPDATE (7 changes)
- Generate Phase 5A using-statement migration manifest
- HANDOFF.md v1.0.26 - Phase 2A COMPLETE
- Begin Phase 2B HALT batch (TreeViewUI + ToastUI - first WPF/MVVM-heavy sub-phase)
```

## Section I Document History Append

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.25 | 2026-05-29 | DSW | **Phase 2A Msg 1 emitted.** 22 HALT items batch-resolved (HotkeyManager HIGH-stub + HotkeyConfig record + TaskTree.TestSupport project promotion + 5 csproj patches + 10 tests). 11 new cross-phase gap rows (100-110). Gap #97/#98 CLOSED (FakeClock + InMemorySecureStore relocated). Architecture v1.0.2 amendment now 7 bundled changes (Gap #105). Phase 1 implicit approval per Gap #100. Q11 implicit empty default per Gap #102. Total 194 items / 21 registries / 110 gaps. |

## Section J Footer

- Additive delta. Rebase v1.0.16-v1.0.25 onto v1.0.15.
- Companion docs: Architecture.md v1.0.1 (+ pending v1.0.2 with 7 bundled changes per Gap #105), Roadmap.md v1.0.0.
- Registry added: docs/spec-derivations/PHASE2A-DERIVATIONS.md.
- Helper script: 18 marker buckets (added PHASE2A=5); grand total 79.
- 194 derivations / 21 registries / 110 gaps.
- LOAD-BEARING for Phase 5F: Q10, Architecture v1.0.2 final.
- LOAD-BEARING for Phase 2A Msg 2: Architecture v1.0.2 7th change wording + Phase 5A using-statement migration manifest.
- **Phase 2A Msg 1 done.** Next: Phase 2A Msg 2 HALT - Architecture v1.0.2 update + using migration manifest.
