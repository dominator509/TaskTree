# PHASE2C-DERIVATIONS.md - Phase 2C Msg 1 (TaskMetadata + TaskBuilderViewModel)

> Scope: TaskMetadata core model + TaskNode metadata patch + TaskBuilderViewModel + tests.
> Owner-approved HALT batch: 20 items.

## Section 1 Summary Table

| # | Item | Resolution | Gap |
|---|---|---|---|
| 1 | TaskMetadata on TaskNode | Add nullable TaskMetadata? Metadata | #127 |
| 2 | TaskMetadata shape | sealed record with 6 fields + Empty | - |
| 3 | PHI-safe scope | No name/MRN/DOB/phone/email/address/blob/dictionary | #128 |
| 4 | String caps | 160/120/120 | - |
| 5 | Minimal PHI heuristic | @, 7+ digits, DOB-like slashes | #129 |
| 6 | Defaults | TaskMetadata.Empty | - |
| 7 | Audit posture | No metadata-specific audit in 2C | #130 |
| 8 | Serialization compatibility | Additive nullable property | #131 |
| 9 | VM dependencies | ITaskEngine + IClock + IAppLogger | - |
| 10 | Builder command behavior | Validate then AddAsync with Metadata | - |
| 11 | Defaults | Priority.Normal, Deadline null | - |
| 12 | Lab due time rule | Warning only; not blocking | #132 |
| 13 | MainWindow path | TaskCreated event args | #133 |
| 14 | Reset after create | Reset fields + Task created | - |
| 15 | MainWindow patch timing | Defer to Msg 2 | #133 |
| 16 | Status text | Fixed strings | - |
| 17 | Tests | 14 tests | - |
| 18 | Test project reuse | Reuse TaskTree.UI.Tests | - |
| 19 | Marker count | PHASE2C = 4 | - |
| 20 | Msg structure | Msg 1 core + builder; Msg 2 integration | - |

## Section 2 Files Produced

| Path | Purpose | Marker |
|---|---|---|
| src/TaskTree.Core/Models/TaskMetadata.cs | NEW PHI-minimal metadata record | PHASE2C |
| src/TaskTree.Core/Models/TaskNode.cs | PATCH add Metadata property | PHASE1A + PHASE2C |
| src/TaskTree.UI/ViewModels/TaskBuilderViewModel.cs | NEW builder VM | PHASE2C |
| tests/TaskTree.UI.Tests/TaskBuilderViewModelTests.cs | NEW 14 tests | PHASE2C |
| docs/spec-derivations/PHASE2C-DERIVATIONS.md | This registry | md |
| docs/HANDOFF-v1.0.29-delta.md | Additive delta | md |
| tools/find-spec-derivations.ps1 | UPDATED PHASE2C=4 | script |

## Section 3 Marker Inventory

PHASE2C = 4 distinct .cs files. Grand total: 87 -> 91.

## Section 4 Cross-Phase Gaps Introduced (rows 127-133)

| # | Gap | Target | Action |
|---|---|---|---|
| 127 | TaskNode model surface expanded with Metadata | Phase 5B | Verify compile/serialization/test compatibility |
| 128 | Model is PHI-minimal but not formal PHI scanner | Phase 4A/5F | Formal PHI rules |
| 129 | Minimal 2C PHI-like heuristic is not formal scanner | Phase 4A | Replace/augment validation |
| 130 | No metadata-specific audit vocabulary in 2C | Phase 4A | Add if product requires |
| 131 | TaskNode JSON backward/forward compatibility | Phase 5C | Verify old tasks deserialize with Metadata null |
| 132 | Lab review without due time is warning not blocker | Phase 2E/4A | Add stricter lab policy if required |
| 133 | MainWindow integration with TaskBuilder deferred | Phase 2C Msg 2 | Wire MainWindow/MainWindowViewModel to TaskBuilderViewModel.TaskCreated |

## Section 5 Patches Required in Msg 2

- Patch MainWindow.xaml or add TaskBuilder view surface.
- Patch MainWindowViewModel to launch or host TaskBuilderViewModel.
- Wire TaskBuilderViewModel.TaskCreated event to refresh or append Tasks.
- Consider DataGrid -> TreeView migration for nested metadata/subtasks per Gap #117.

## Section 6 Known Limitations

1. PHI-like validation is heuristic only; Phase 4A must formalize scanner (Gap #128/#129).
2. Metadata-specific audit vocabulary deferred (Gap #130).
3. Serialization compatibility must be verified with real SecureStore JSON (Gap #131).
4. MainWindow integration deferred to Msg 2 (Gap #133).
5. Lab due-time policy may tighten later (Gap #132).
6. TaskMetadata field set is intentionally narrow; future fields require HALT.
