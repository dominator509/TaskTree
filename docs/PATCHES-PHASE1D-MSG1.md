# PATCHES-PHASE1D-MSG1.md — Patch Manifest

> **Audience:** Codex / Claude Code at Phase 5A Repo Stitching.
> **Purpose:** Apply three structural patches to existing test files that
> currently each contain a private nested `FakeClock` class. Per HALT #1
> resolution (Phase 1D Msg 1), `FakeClock` is promoted to
> `tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs` and the inline copies
> must be removed.
> **SPEC-DERIVED-PHASE1D** — referenced here for cross-link; the marker itself
> lives on the promoted FakeClock.cs file, not in this document.

---

## Patch Targets

| # | File | csproj |
|---|---|---|
| 1 | `tests/TaskTree.Modules.TaskEngine.Tests/TaskEngineTests.cs` | `tests/TaskTree.Modules.TaskEngine.Tests/TaskTree.Modules.TaskEngine.Tests.csproj` |
| 2 | `tests/TaskTree.Modules.ComplianceCore.Tests/ComplianceCoreTests.cs` | `tests/TaskTree.Modules.ComplianceCore.Tests/TaskTree.Modules.ComplianceCore.Tests.csproj` |
| 3 | `tests/TaskTree.Modules.ComplianceCore.Tests/AuditChainWriterTests.cs` | (same csproj as #2) |

For Phase 1D Msg 2, the new file at `tests/TaskTree.Modules.ReminderScheduler.Tests/ReminderSchedulerTests.cs` will also need the same `using` + ProjectReference pattern from day one.

---

## Per-File Patch Recipe

For each target `.cs` file:

### Patch #1 — REMOVE the inline `FakeClock` nested class

Locate and delete the block matching:

```csharp
private sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; }
    // ... (any helper methods such as Advance(TimeSpan))
}
```

(The exact form may vary slightly between the three files; locate by the literal type name `FakeClock`.)

### Patch #2 — ADD `using` directive

Insert near the existing `using` block at the top of the file:

```csharp
using TaskTree.Core.Tests.TestDoubles;
```

### Patch #3 — Update test method bodies (if needed)

If any test method instantiates `FakeClock()` and previously relied on a parameterless default of `DateTimeOffset.UtcNow`, note that the promoted `FakeClock()` defaults to `2026-01-01T00:00:00Z`. Inspect each `new FakeClock()` usage; if the test asserts on relative time only, no change is needed. If the test asserted on a near-current time, switch to `new FakeClock(DateTimeOffset.UtcNow)`.

---

## Per-csproj Patch — Add ProjectReference

For both `TaskTree.Modules.TaskEngine.Tests.csproj` and `TaskTree.Modules.ComplianceCore.Tests.csproj`, ensure the following `<ItemGroup>` exists (or append to an existing one):

```xml
<ItemGroup>
  <ProjectReference Include="..\TaskTree.Core.Tests\TaskTree.Core.Tests.csproj" />
</ItemGroup>
```

Note: this introduces an inter-test-project reference. This is acceptable because `TaskTree.Core.Tests` is the canonical home for shared test infrastructure per HALT #1 Option A.

For Phase 1D Msg 2, add the same `<ProjectReference>` to the new `TaskTree.Modules.ReminderScheduler.Tests.csproj`.

---

## Verification (post-patch)

Run from repo root:

```powershell
# 1. Exactly one FakeClock class definition remains in the repo:
Get-ChildItem -Recurse -Include *.cs |
    Select-String -Pattern 'class\s+FakeClock\b' |
    Measure-Object | Select-Object -ExpandProperty Count
# Expected output: 1

# 2. The single match is in the promoted file:
Get-ChildItem -Recurse -Include *.cs |
    Select-String -Pattern 'class\s+FakeClock\b' |
    Select-Object Path
# Expected: tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs

# 3. Solution still builds:
dotnet restore TaskTree.sln
dotnet build  TaskTree.sln -c Release

# 4. Offline tests still pass:
dotnet test TaskTree.sln -c Release --filter "TestCategory!=Live"

# 5. Marker registry still valid:
pwsh -File tools/find-spec-derivations.ps1 -Root .
# Expected: PASS, including SPEC-DERIVED-PHASE1D = 4
```

---

## Failure Modes & Mitigation

| Symptom | Cause | Fix |
|---|---|---|
| `CS0246: 'FakeClock' type not found` | Patch #2 (`using`) omitted | Add `using TaskTree.Core.Tests.TestDoubles;` |
| `CS0234: namespace 'TestDoubles' not found` | ProjectReference omitted | Add `<ProjectReference>` to the test csproj |
| Multiple `FakeClock` definitions | Patch #1 incomplete | Re-run verification step 1; delete remaining inline copies |
| Test asserts on absolute time fail | Default epoch shifted to 2026-01-01 | Use `new FakeClock(DateTimeOffset.UtcNow)` in those specific tests |
| `find-spec-derivations.ps1` FAIL on PHASE1D=4 | A promoted-file marker was accidentally lost | Restore the `SPEC-DERIVED-PHASE1D` block in the FakeClock.cs header |

---

## Why this is a manifest, not a code patch

Phase 1D Msg 1 generation was performed in a chat-only environment without access to the existing test file bodies. Per Roadmap D5 ("No placeholder code"), Msg 1 emits a precise manifest rather than guessing at the existing files' contents. Codex / Claude Code applies the patches deterministically at Phase 5A, with the verification steps above ensuring drift-free results.

Cross-reference: `docs/spec-derivations/PHASE1D-DERIVATIONS.md` §2 (Item #1) and `HANDOFF.md` v1.0.16 §B.
