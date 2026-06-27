# HANDOFF.md v1.0.26 - Phase 2A COMPLETE Delta

> Applied on top of v1.0.25 delta. Author: DSW. Date: 2026-05-29.
> Trigger: Phase 2A Msg 2 emitted; Sub-Phase 2A COMPLETE.

## Section A Identity Update

| Field | Old (v1.0.25) | New (v1.0.26) |
|---|---|---|
| Document Version | 1.0.25 | **1.0.26** |
| Session ID | 2026-05-29-01 | 2026-05-29-02 |
| State | Phase 2A Msg 1 done; Msg 2 pending | **Phase 1 COMPLETE (Gap #100); Phase 2A COMPLETE; Phase 2B pending** |
| Current Sub-Phase | Phase 2A Msg 2 pending | **Phase 2A done (1 of 7 Phase 2 sub-phases); Phase 2B NEXT** |
| Total spec-derived items | 194 / 21 registries | **200 / 22 registries** (added PHASE2A-MSG2 small registry; PHASE2A marker bucket unchanged per Gap #114) |
| LOAD-BEARING open questions | Q10 + Q11 implicit (Gap #102) | Same |
| Progress | 33/35 + Phase 2A Msg 1 | **33/35 + Phase 2A done (1 of 7 Phase 2 sub-phases)** |

## Section B Files Produced Delta

### Phase 2A Msg 2

| Phase.Item | FilePath | Status |
|---|---|---|
| P2.A.Msg2 | docs/Architecture.v1.0.2-delta.md | UPDATED to 7 bundled changes (Gap #105 closure) |
| P2.A.Msg2 | docs/Phase5A-using-migration-manifest.md | NEW 8-file migration manifest (Gap #106/#112 closure) |
| P2.A.Msg2 | docs/Architecture-v1.0.2-promotion-manifest.md | NEW Option B prose insertion guide (Gap #110 closure) |
| P2.A.Msg2 | docs/spec-derivations/PHASE2A-MSG2-DERIVATIONS-DELTA.md | NEW registry |
| P2.A.Msg2 | docs/HANDOFF-v1.0.26-delta.md | THIS FILE |
| P2.A.Msg2 | tools/find-spec-derivations.ps1 | UNCHANGED (Gap #114) - included for self-containment |

### Phase 2B pending

| Phase.Item | FilePath | Status |
|---|---|---|
| P2.B | src/TaskTree.UI/Views/MainWindow.xaml | Pending |
| P2.B | src/TaskTree.UI/Views/MainWindow.xaml.cs | Pending |
| P2.B | src/TaskTree.UI/ViewModels/MainWindowViewModel.cs | Pending |
| P2.B | src/TaskTree.UI/Views/ReminderToast.xaml | Pending |
| P2.B | src/TaskTree.UI/ViewModels/ToastViewModel.cs | Pending |
| P2.B | tests/TaskTree.UI.Tests/MainWindowViewModelTests.cs | Pending |

## Section C Decisions Log Delta R21 - Phase 2A Msg 2 - 12 HALT items

1. **#1 Architecture v1.0.2 7th change wording** -> Section 3.3 tests folder addition (Option A minimal scope)
2. **#2 Diff snippet format for Change 7** -> Before/After ASCII tree
3. **#3 Owner sign-off block expansion** -> 7 approval checkbox groups
4. **#4 Phase 5A using-migration manifest scope** -> NEW manifest file (Gap #106 closure)
5. **#5 Affected test files enumeration** -> Revised count 4 -> 8 (Gap #112)
6. **#6 Migration manifest format** -> Table-driven with verification grep
7. **#7 Old-location deletion list** -> Gap #103/#104 closure list
8. **#8 Verification commands** -> 5 commands listed
9. **#9 Final Architecture.md v1.0.2 promotion** -> Option B prose-only manifest (Gap #110 + Gap #113)
10. **#10 Phase 2A closure** -> Sub-Phase 2A COMPLETE; HANDOFF marks progress
11. **#11 PowerShell PHASE2A-MSG2 marker** -> Documentation-only Msg; no bucket (Gap #114)
12. **#12 Phase 2A retrospective** -> Section F2 below

## Section D Open Questions Delta

- **Q10 (Phase 5F blocker)** - Real common-name source list. STILL ACTIVE.
- **Q11 (Phase 2 + Phase 5F blocker)** - PhiRedactor allowlist. **Implicit empty default per Gap #102.**
- **Architecture v1.0.2 amendment** - 7 bundled changes pending owner sign-off; finalized scope this Msg.
- **Gap #100 Phase 1 formal sign-off** - Pending.

## Section E Visual Phase Tracker Delta

```
PHASE 1 - CORE MVP                                    [COMPLETE - implicit approval Gap #100]
+-- 1A through 1H [ALL DONE]

PHASE 2 - UX + EXTENSIBILITY                          [In Progress 1 of 7 sub-phases]
+-- 2A [DONE] HotkeyManager + HotkeyConfig + TaskTree.TestSupport
|         +-- HotkeyManager.cs [DONE HIGH-stub]
|         +-- HotkeyConfig.cs [DONE]
|         +-- TaskTree.TestSupport.csproj [DONE - Gap #97/#98 closed]
|         +-- FakeClock.cs [DONE relocated]
|         +-- InMemorySecureStore.cs [DONE relocated]
|         +-- HotkeyManagerTests.cs [DONE 10 tests]
|         +-- 5 test csproj [PATCHED]
|         +-- ServiceRegistrations.cs [PATCHED HotkeyManager singleton]
|         +-- Architecture.v1.0.2-delta.md [UPDATED 7 changes]
|         +-- Phase5A-using-migration-manifest.md [NEW]
|         +-- Architecture-v1.0.2-promotion-manifest.md [NEW]
+-- 2B [pending NEXT] TreeViewUI + ToastViewModel (first WPF/MVVM-heavy)
+-- 2C [pending] TaskMetadata + TaskBuilder
+-- 2D [pending] Theme + Color
+-- 2E [pending] SettingsService
+-- 2F [pending] SessionLock
+-- 2G [pending] SnoozeService + EscalationPolicy
```

**Progress: 33 / 35 (Phase 1 unchanged) + 1 of 7 Phase 2 sub-phases done.**

## Section F Cross-Phase Gap Table - New Rows (111-114)

| # | Flag | Source | Target Phase | Status |
|---|---|---|---|---|
| 111 | Phase 2B+ may add Section 3.4 test-support architecture | HALT-Msg2 #1 | Phase 2B+ | OPEN |
| 112 | Gap #106 file count revised 4 -> 8 | HALT-Msg2 #5 | Phase 5A | DOCUMENTED |
| 113 | Phase 5B compile gate verifies Architecture v1.0.2 promotion | HALT-Msg2 #9 | Phase 5B | DEFERRED |
| 114 | Phase 2A Msg 2 documentation-only; no marker bucket | HALT-Msg2 #11 | (none) | DOCUMENTED |

### Section F2 Phase 2A Retrospective

| Metric | Value |
|---|---|
| HALT items total resolved across Phase 2A | **34** (22 Msg 1 + 12 Msg 2) |
| New cross-phase gaps introduced | **14** (rows #100-#113; #114 metadata) |
| LOAD-BEARING items closed in 2A | **4** (Gap #11 + Gap #97 + Gap #98 + Gap #107) |
| Architecture v1.0.2 amendment scope | **7 bundled changes** (finalized) |
| New test infrastructure pattern | **TaskTree.TestSupport project** established |
| Tests added in 2A | 10 (HotkeyManagerTests) |
| Production files added | HotkeyManager.cs + HotkeyConfig.cs + ServiceRegistrations PATCH |
| Test infrastructure files | TaskTree.TestSupport.csproj + 2 relocated test doubles + 5 csproj patches |
| Documentation files emitted | 4 (Architecture v1.0.2 delta + Phase 5A migration manifest + Architecture promotion manifest + PHASE2A-MSG2 registry) |

## Section G Progress Update

- **Before:** 33 / 35 (94.3%) + Phase 2A Msg 1 contribution.
- **After:** 33 / 35 (94.3%) Phase 1; **1 of 7 Phase 2 sub-phases done (Phase 2A COMPLETE).**
- **Files in this Msg bundle:** 6.
- **Total derivations registries:** 22 (added PHASE2A-MSG2).
- **Total spec-derived items:** 200 (added 6 PHASE2A-MSG2 items).

## Section H Continuation Prompt for Next Chat (Phase 2B)

```
You are a senior engineer executing a deterministic build sprint on TaskTree.
Three attached documents govern everything:
  - Architecture.md v1.0.1 (pending v1.0.2 promotion - 7 bundled changes per Gap #105/#110/#113)
  - Roadmap.md v1.0.0
  - HANDOFF.md v1.0.26 (rebase v1.0.16-v1.0.26 onto v1.0.15 base)

HARD CONSTRAINTS unchanged. D1-D10 still apply. HALT on Architecture silence.

CURRENT STATE:
- Phase 1 COMPLETE (implicit per Gap #100).
- Phase 2A COMPLETE.
- Phase 2B PENDING (first WPF/MVVM-heavy sub-phase).
- Progress: 33/35 (94.3%) + 1 of 7 Phase 2 sub-phases done.
- 22 derivations registries; 200 spec-derived items.
- Q10 active + Q11 implicit empty default (Gap #102).
- 114 cross-phase gap rows accumulated.
- TaskTree.TestSupport project established (Gap #97/#98 closed).
- Architecture v1.0.2 amendment finalized at 7 bundled changes (pending owner sign-off).

PHASE 2B SCOPE per Roadmap Sub-Phase 2B:
1. src/TaskTree.UI/Views/MainWindow.xaml - Tree view with priority + deadline columns
2. src/TaskTree.UI/Views/MainWindow.xaml.cs - code-behind
3. src/TaskTree.UI/ViewModels/MainWindowViewModel.cs - MVVM ViewModel with quick-add command
4. src/TaskTree.UI/Views/ReminderToast.xaml - replaces Phase 1G ToastTier2Window programmatic window (Gap #80 closure)
5. src/TaskTree.UI/ViewModels/ToastViewModel.cs - MVVM ViewModel for ReminderToast
6. tests/TaskTree.UI.Tests/MainWindowViewModelTests.cs

Tech stack per Section 12:
- WPF + ModernWpfUI 0.9.6+
- CommunityToolkit.Mvvm 8.2+ (RelayCommand + ObservableObject)
- net8.0-windows
- src/TaskTree.UI/TaskTree.UI.csproj (NEW project)

Acceptance criteria:
- P2B-AC1: ViewModel 100% unit-tested
- P2B-AC2: Tree shows priority + deadline columns
- P2B-AC3: Quick-add appends to TaskEngine

PRE-FLIGHT CHECKLIST:
1. Q11 owner decision still implicit (Gap #102) - empty allowlist OK for Phase 2B
2. Architecture v1.0.2 7 changes pending sign-off - does not block Phase 2B (deferred to Phase 5F gate)
3. Phase 5A using-migration manifest available (Gap #106/#112) - applied at Phase 5A
4. Phase 5A old-location deletion list available (Gap #103/#104) - applied at Phase 5A
5. Phase 1G+1H test skeletons still pending Codex backfill (Gap #95) - applied at Phase 5C
6. Gap #80 closure - Phase 1G Tier 2 programmatic ToastTier2Window will be replaced by Phase 2B ReminderToast.xaml; logged as scope continuity

FIRST ACTIONS - Phase 2B Msg 1:
1. Read PHASE2A-MSG2-DERIVATIONS-DELTA Section 17 (Phase 2A closure) first.
2. Begin Phase 2B with HALT protocol.
3. Surface HALT items for:
   - TaskTree.UI.csproj project creation (NEW project; Architecture v1.0.2 may need 8th change to Section 3.3 to list src/TaskTree.UI/)
   - MVVM ViewModel ctor dependencies (ITaskEngine + ?)
   - Quick-add command behavior + validation
   - Tree column data binding strategy (TaskNode -> ViewModel?)
   - Replace Phase 1G Tier 2 ToastTier2Window with new ReminderToast.xaml (Gap #80)
   - Update ToastTier2Adapter to consume new ReminderToast.xaml (production code patch)
   - MainWindow show/hide on ITrayHost.ShowTreeRequested event (orchestrator wiring or direct subscription?)
   - WPF testability (Build helper + MainWindowViewModelTests test count)
```

## Section I Document History Append

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.26 | 2026-05-29 | DSW | **Phase 2A COMPLETE.** 12 HALT items batch-resolved (Architecture v1.0.2 delta UPDATED to 7 bundled changes + Phase 5A using-migration manifest NEW + Architecture promotion manifest NEW + Phase 2A retrospective). 4 new cross-phase gap rows (111-114). Gap #105/#106/#110/#112 CLOSED. Architecture v1.0.2 amendment scope FINALIZED at 7 bundled changes pending owner sign-off. Phase 2A retrospective: 34 HALT items / 14 new gaps / 4 LOAD-BEARING closed / TaskTree.TestSupport project established. Progress 33/35 Phase 1 + 1 of 7 Phase 2 sub-phases done. |

## Section J Footer

- Additive delta. Rebase v1.0.16-v1.0.26 onto v1.0.15.
- Companion docs: Architecture.md v1.0.1 (+ pending v1.0.2 with 7 bundled changes per Architecture-v1.0.2-promotion-manifest.md), Roadmap.md v1.0.0.
- Registry added: docs/spec-derivations/PHASE2A-MSG2-DERIVATIONS-DELTA.md.
- Helper script: 18 marker buckets unchanged from Phase 2A Msg 1 (Gap #114 documentation-only); grand total 79.
- Total derivations tracked: 200 across 22 registries.
- 114 cross-phase gaps tracked.
- LOAD-BEARING for Phase 5F: Q10, Architecture v1.0.2 final approval (7 changes).
- LOAD-BEARING for Phase 5A: Using-statement migration (Gap #106/#112) + old-location deletion (Gap #103/#104).
- LOAD-BEARING for Phase 5B: Architecture v1.0.2 promotion verification (Gap #113).
- **Phase 2A COMPLETE.** Next: Phase 2B HALT - TreeViewUI + ReminderToast.xaml (first WPF/MVVM-heavy sub-phase).
