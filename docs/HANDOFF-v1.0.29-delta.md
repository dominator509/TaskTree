# HANDOFF.md v1.0.29 - Phase 2C Msg 1 Delta

> Applied on top of v1.0.28 delta. Author: DSW. Date: 2026-05-29.
> Trigger: Phase 2C Msg 1 emitted; TaskMetadata + TaskBuilderViewModel created.

## Section A Identity Update

| Field | Old | New |
|---|---|---|
| Document Version | 1.0.28 | 1.0.29 |
| State | Phase 2B COMPLETE; Phase 2C pending | Phase 2C Msg 1 done; Msg 2 pending |
| Total spec-derived items | 232 / 24 registries | 252 / 25 registries |
| Progress | 33/35 + 2 of 7 Phase 2 | 33/35 + 2.5 of 7 Phase 2 |

## Section B Files Produced

| File | Status |
|---|---|
| src/TaskTree.Core/Models/TaskMetadata.cs | NEW |
| src/TaskTree.Core/Models/TaskNode.cs | PATCHED |
| src/TaskTree.UI/ViewModels/TaskBuilderViewModel.cs | NEW |
| tests/TaskTree.UI.Tests/TaskBuilderViewModelTests.cs | NEW 14 tests |
| docs/spec-derivations/PHASE2C-DERIVATIONS.md | NEW |
| docs/HANDOFF-v1.0.29-delta.md | THIS FILE |
| tools/find-spec-derivations.ps1 | UPDATED PHASE2C=4 |

## Section C Decisions Log R24 - 20 HALT items

All 20 approved as proposed. Key decisions: TaskNode gets nullable Metadata; TaskMetadata is narrow and PHI-minimal; 2C uses simple PHI-like heuristics; no metadata-specific audit vocabulary yet; TaskBuilderViewModel exposes TaskCreated event; MainWindow integration deferred to Msg 2.

## Section D Cross-Phase Gaps 127-133

See PHASE2C-DERIVATIONS Section 4. Codex/Claude must specifically close or carry #127-#133 in future phases.

## Section E Visual Phase Tracker

```
PHASE 2 - UX + EXTENSIBILITY
+-- 2A [DONE]
+-- 2B [DONE]
+-- 2C [IN PROGRESS - Msg 1 done, Msg 2 pending] TaskMetadata + TaskBuilder
+-- 2D [pending]
+-- 2E [pending]
+-- 2F [pending]
+-- 2G [pending]
```

## Section F Continuation Prompt for Phase 2C Msg 2

```
Begin Phase 2C Msg 2 with HALT protocol.
Read PHASE2C-DERIVATIONS Section 5 first.
Scope:
1. Patch MainWindow.xaml or add builder UI surface.
2. Patch MainWindowViewModel to open/host TaskBuilderViewModel.
3. Wire TaskBuilderViewModel.TaskCreated event to append/refresh Tasks.
4. Decide whether DataGrid -> TreeView migration happens now or remains Phase 2C/2D carry-forward per Gap #117.
5. Emit HANDOFF v1.0.30 and retrospective marking Phase 2C complete if integration wiring is done.
```

## Section G Footer

- Registry added: PHASE2C-DERIVATIONS.md.
- Marker script: PHASE2C=4; grand total 91.
- Total gaps tracked now: 133.
- Q10, Q11 implicit, Architecture v1.0.2 sign-off, and Phase 5A/5B/5C carries remain active.
