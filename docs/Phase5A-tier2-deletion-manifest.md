# Phase 5A Tier 2 Deletion Manifest (Gap #119 closure path)

> **Status:** DEFERRED - preconditions 1-2 verified in stitched repo; deletion still requires successful Release build.
> **Trigger:** Phase 2B Msg 2 patched `ToastTier2Adapter.cs` to consume `TaskTree.UI.Views.ReminderToast` instead of the Phase 1G programmatic `ToastTier2Window`.
> **Author:** DSW - **Date:** 2026-05-29

## Section 1 Scope

Delete the obsolete Phase 1G Tier 2 WPF window files after Codex verifies the new `ReminderToast.xaml` path compiles and `ToastTier2Adapter` no longer references the old type.

## Section 2 Files To Delete

- `src/TaskTree.Orchestrator/Views/ToastTier2Window.xaml`
- `src/TaskTree.Orchestrator/Views/ToastTier2Window.xaml.cs`

## Section 3 Preconditions

1. `ToastTier2Adapter.cs` resolves `TaskTree.UI.Views.ReminderToast` and `TaskTree.UI.ViewModels.ToastViewModel` successfully.
2. `TaskTree.Orchestrator.csproj` includes a `ProjectReference` to `TaskTree.UI.csproj`.
3. `dotnet build TaskTree.sln -c Release` succeeds before deletion.

## Section 4 Verification Commands

1. `grep -rn "ToastTier2Window" src/` must return only the two files listed in Section 2 before deletion.
2. `grep -rn "ToastTier2Window" src/` must return zero matches after deletion.
3. `grep -rn "ReminderToast" src/TaskTree.Orchestrator/ToastTier2Adapter.cs` must return at least one match.
4. `dotnet build TaskTree.sln -c Release` must succeed after deletion.
5. `dotnet test TaskTree.sln --filter "TestCategory!=Live"` must pass (or honor SKELETON Assert.Inconclusive results per Gap #95 if not yet backfilled).

## Section 5 Execution Order

1. Apply Phase 2B Msg 2 `ToastTier2Adapter.cs` patch and `TaskTree.Orchestrator.csproj` ProjectReference patch.
2. Verify build success.
3. Delete the two files in Section 2.
4. Re-run build.
5. Re-run tests.

## Section 6 Known Limitations

1. This manifest deletes only the old XAML window files. Any stale `using TaskTree.Orchestrator.Views;` statements elsewhere must also be removed if Codex finds them.
2. The new `ReminderToast` window still uses hard-coded brushes in `ToastViewModel` until Phase 2D (Gap #120).
3. Adapter-owned auto-close timer remains in `ToastTier2Adapter` by design (Gap #121).

## Section 7 Codex Precondition Notes

- Precondition 1 verified: `src/TaskTree.Orchestrator/ToastTier2Adapter.cs` references `TaskTree.UI.Views.ReminderToast` and `TaskTree.UI.ViewModels.ToastViewModel`.
- Precondition 2 verified: `src/TaskTree.Orchestrator/TaskTree.Orchestrator.csproj` references `..\TaskTree.UI\TaskTree.UI.csproj`.
- Precondition 3 not yet verified locally: `dotnet build TaskTree.sln -c Release` cannot run until a .NET SDK is available in the shell; see `docs/compile/compile-error-register.md` entry `P5B-E002`.
- Deletion remains intentionally unapplied until the build-before-delete gate is satisfied.
