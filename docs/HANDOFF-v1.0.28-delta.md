# HANDOFF.md v1.0.28 - Phase 2B COMPLETE Delta

> Applied on top of v1.0.27 delta. Author: DSW. Date: 2026-05-29.
> Trigger: Phase 2B Msg 2 emitted; Sub-Phase 2B COMPLETE.

## Section A Identity Update

| Field | Old (v1.0.27) | New (v1.0.28) |
|---|---|---|
| Document Version | 1.0.27 | **1.0.28** |
| Session ID | 2026-05-29-03 | 2026-05-29-04 |
| State | Phase 2B Msg 1 done; Msg 2 pending | **Phase 2A COMPLETE; Phase 2B COMPLETE; Phase 2C pending** |
| Current Sub-Phase | Phase 2B Msg 2 pending | **Phase 2B done (2 of 7 Phase 2 sub-phases); Phase 2C NEXT** |
| Total spec-derived items | 224 / 23 registries | **232 / 24 registries** |
| LOAD-BEARING open questions | Q10 active + Q11 implicit | Same |
| Progress | 33/35 + 1.5 of 7 Phase 2 | **33/35 + 2 of 7 Phase 2 sub-phases done** |

## Section B Files Produced Delta

### Phase 2B Msg 2

| Phase.Item | FilePath | Status |
|---|---|---|
| P2.B.Msg2 | src/TaskTree.Orchestrator/ToastTier2Adapter.cs | PATCH (ReminderToast + ToastViewModel) |
| P2.B.Msg2 | src/TaskTree.Orchestrator/Orchestrator.cs | PATCH (`ShowTreeRequested` now shows MainWindow) |
| P2.B.Msg2 | src/TaskTree.Orchestrator/TaskTree.Orchestrator.csproj | PATCH add `TaskTree.UI` ProjectReference |
| P2.B.Msg2 | docs/Phase5A-tier2-deletion-manifest.md | NEW |
| P2.B.Msg2 | docs/spec-derivations/PHASE2B-MSG2-DERIVATIONS-DELTA.md | NEW |
| P2.B.Msg2 | docs/HANDOFF-v1.0.28-delta.md | THIS FILE |
| P2.B.Msg2 | tools/find-spec-derivations.ps1 | UPDATED PHASE2B 6 -> 8, grand total 87 |

### Phase 2C pending

| Phase.Item | FilePath | Status |
|---|---|---|
| P2.C | src/TaskTree.Core/Models/TaskMetadata.cs | Pending |
| P2.C | src/TaskTree.UI/ViewModels/TaskBuilderViewModel.cs | Pending |
| P2.C | tests/TaskTree.UI.Tests/TaskBuilderViewModelTests.cs | Pending |

## Section C Decisions Log Delta R23 - Phase 2B Msg 2

1. **ToastTier2Adapter PATCH** -> `ReminderToast` + `ToastViewModel` now used instead of Phase 1G `ToastTier2Window` (Gap #119 path).
2. **Auto-close timer ownership** -> stays in adapter (Gap #121).
3. **Orchestrator ShowTreeRequested wiring** -> `OnShowTreeRequested` now creates/reuses `MainWindow` with `MainWindowViewModel` (Gap #118 closure).
4. **MainWindow lifetime** -> single reused window instance inside Orchestrator (Gap #125).
5. **TaskTree.Orchestrator.csproj** -> add `TaskTree.UI` ProjectReference (Gap #124 closed).
6. **Phase 5A deletion manifest** -> emitted for old `ToastTier2Window` files (Gap #126).

## Section D Open Questions Delta

- **Q10 (Phase 5F blocker)** - STILL ACTIVE.
- **Q11 (Phase 2 + Phase 5F blocker)** - Implicit empty default per Gap #102.
- **Architecture v1.0.2 amendment** - 7 changes pending owner sign-off.
- **Gap #100 Phase 1 formal sign-off** - Pending.
- **Gap #123 direct-construction pattern** - Phase 5C verify under real WPF/DI context.

## Section E Visual Phase Tracker Delta

```
PHASE 1 - CORE MVP                                    [COMPLETE]
+-- 1A through 1H [ALL DONE]

PHASE 2 - UX + EXTENSIBILITY                          [In Progress 2 of 7 sub-phases]
+-- 2A [DONE] HotkeyManager + HotkeyConfig + TestSupport
+-- 2B [DONE] TreeViewUI + ReminderToast WPF/MVVM
|         +-- TaskTree.UI project [DONE]
|         +-- MainWindow + ViewModel [DONE]
|         +-- ReminderToast + ToastViewModel [DONE]
|         +-- ToastTier2Adapter PATCH [DONE]
|         +-- Orchestrator ShowTreeRequested PATCH [DONE]
|         +-- Phase5A Tier 2 deletion manifest [DONE]
+-- 2C [pending NEXT] TaskMetadata + TaskBuilder
+-- 2D [pending] Theme + Color
+-- 2E [pending] SettingsService
+-- 2F [pending] SessionLock
+-- 2G [pending] SnoozeService + EscalationPolicy
```

**Progress: 33 / 35 + 2 of 7 Phase 2 sub-phases done.**

## Section F Cross-Phase Gap Table - New Rows (123-126)

| # | Flag | Source | Target Phase | Status |
|---|---|---|---|---|
| 123 | Orchestrator directly constructs MainWindowViewModel/MainWindow | Phase 2B Msg 2 | Phase 5C | OPEN |
| 124 | TaskTree.Orchestrator.csproj add TaskTree.UI ProjectReference | Phase 2B Msg 2 | (closed) | CLOSED |
| 125 | MainWindow singleton lifetime managed inside Orchestrator | Phase 2B Msg 2 | Phase 5C | OPEN |
| 126 | Phase 5A delete legacy ToastTier2Window files | Phase 2B Msg 2 | Phase 5A | OPEN |

## Section G Progress Update

- **Before:** 33 / 35 + 1.5 of 7 Phase 2 sub-phases.
- **After:** 33 / 35 + **2 of 7 Phase 2 sub-phases done (2A + 2B complete).**
- **Files in this Msg bundle:** 7.
- **Total derivations registries:** 24 (added PHASE2B-MSG2).
- **Total spec-derived items:** 232.

## Section H Continuation Prompt for Next Chat (Phase 2C)

```
You are a senior engineer executing a deterministic build sprint on TaskTree.
Three attached documents govern everything:
  - Architecture.md v1.0.1 (pending v1.0.2 promotion - 7 bundled changes)
  - Roadmap.md v1.0.0
  - HANDOFF.md v1.0.28 (rebase v1.0.16-v1.0.28 onto v1.0.15 base)

CURRENT STATE:
- Phase 1 COMPLETE.
- Phase 2A COMPLETE.
- Phase 2B COMPLETE.
- Phase 2C PENDING.
- Progress: 33/35 + 2 of 7 Phase 2 sub-phases done.
- 24 derivations registries; 232 spec-derived items.
- 126 cross-phase gap rows accumulated.

PHASE 2C SCOPE per Roadmap Sub-Phase 2C:
1. src/TaskTree.Core/Models/TaskMetadata.cs
2. src/TaskTree.UI/ViewModels/TaskBuilderViewModel.cs
3. tests/TaskTree.UI.Tests/TaskBuilderViewModelTests.cs

Acceptance criteria:
- P2C-AC1: metadata carries patient text + labs + delivery hints without PHI leakage
- P2C-AC2: TaskBuilderViewModel validates required fields before push to TaskEngine
- P2C-AC3: MainWindow integration path to TaskBuilder ready

PRE-FLIGHT CHECKLIST:
1. Q11 still implicit empty default (Gap #102) - OK for 2C
2. Architecture v1.0.2 pending sign-off - does not block 2C
3. Phase 5A using-migration manifest available (Gap #106/#112)
4. Phase 5A old-location deletion manifests available (Gap #103/#104/#126)
5. Phase 1G+1H skeleton tests still pending Codex backfill (Gap #95)
6. Gap #117 DataGrid->TreeView migration deferred to 2C with nested children metadata

FIRST ACTIONS - Phase 2C Msg 1:
1. Read PHASE2B-DERIVATIONS Section 28 + PHASE2B-MSG2-DERIVATIONS Section 11 first.
2. Begin Phase 2C with HALT protocol.
3. Surface HALT items for:
   - TaskMetadata field set and PHI-safe scope
   - TaskBuilderViewModel ctor dependencies
   - Validation rules and status reporting
   - Integration point from MainWindow to TaskBuilder
   - Whether TaskNode grows child collections now or later
```

## Section I Document History Append

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.28 | 2026-05-29 | DSW | **Phase 2B COMPLETE.** `ToastTier2Adapter.cs` patched to use `ReminderToast` + `ToastViewModel`; `Orchestrator.cs` patched to show `MainWindow` on `ShowTreeRequested`; `TaskTree.Orchestrator.csproj` patched with `TaskTree.UI` ProjectReference; Phase 5A Tier 2 deletion manifest emitted. 4 new cross-phase gaps (123-126). Gap #80 closed; Gap #118 closed; Gap #119 patch path emitted; Gap #124 closed. Progress 33/35 + 2 of 7 Phase 2 sub-phases done. |

## Section J Footer

- Additive delta. Rebase v1.0.16-v1.0.28 onto v1.0.15.
- Companion docs: Architecture.md v1.0.1 (+ pending v1.0.2 with 7 bundled changes), Roadmap.md v1.0.0.
- Registry added: docs/spec-derivations/PHASE2B-MSG2-DERIVATIONS-DELTA.md.
- Helper script: PHASE2B 6 -> 8; grand total 87.
- Total derivations tracked: 232 across 24 registries.
- 126 cross-phase gaps tracked.
- LOAD-BEARING for Phase 5F: Q10, Architecture v1.0.2 final approval.
- LOAD-BEARING for Phase 5A: using-migration + old-location deletion + Tier 2 deletion manifest.
- **Phase 2B COMPLETE.** Next: Phase 2C HALT.
