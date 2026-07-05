# compile-resolution-log.md - Phase 5B Compile Resolution Log

> Every compile fix must be recorded here by Claude/Codex. This log is the audit trail proving Phase 5B stayed inside compile-closure scope.

## Resolution Entry Template

```text
Resolution ID:
Related compile error ID(s):
File(s) changed:
Change summary:
Exact rationale:
Why this is compile-closure only:
Why runtime behavior is unchanged or minimally changed:
Regression risk:
Tests/build commands re-run:
Command output reference:
Follow-up gap created/updated:
Reviewer/owner note:
```

## Allowed Compile-Closure Fix Types

- Add missing using directive.
- Correct namespace to match project/folder convention.
- Add missing project reference.
- Add approved package reference.
- Reconcile constructor arguments to latest generated constructor.
- Update TestSupport fake to match final interface.
- Resolve duplicate type by selecting latest approved source and logging superseded source.
- Align WPF target framework/project properties.
- Fix XAML/code-behind namespace or partial class mismatch.

## Not Allowed Without New Approval

- Redesigning module behavior.
- Replacing stubs with fake live success.
- Adding external libraries not approved by Architecture/Roadmap.
- Removing audit/redaction/security checks to make tests compile.
- Hardcoding secrets, tokens, cert passwords, private keys, production URLs, or PHI-like data.

## Deletion / Exclusion Rule

Any file deleted or excluded from compile must include:

```text
Path:
Reason:
Proof duplicate/superseded:
Selected replacement path:
Downstream impact:
Owner review needed: yes/no
```

## Phase 5B Gaps

| Gap | Description | Target |
|---|---|---|
| #372 | Every compile fix must be recorded with rationale, changed files, and regression risk | Phase 5B |
| #374 | Any new package reference must be source-approved or documented for owner review | Phase 5B / Owner |
| #365 | Maintain compile-error register and resolution log | Phase 5B |

## Resolution Entries

### P5B-R001 - Marker Inventory Reconciliation

Resolution ID: P5B-R001
Related compile error ID(s): P5B-E001
File(s) changed:
- `tools/find-spec-derivations.ps1`
- `src/TaskTree.Core/Abstractions/IOrchestrator.cs`
- `src/TaskTree.Core/Models/TaskNode.cs`
- `src/TaskTree.Core/Models/UpdateManifest.cs`
- `src/TaskTree.Core/Models/BugReport.cs`
- `src/TaskTree.Core/Models/AuditEntry.cs`
- `src/TaskTree.Core/Models/ReminderEvent.cs`
- `src/TaskTree.Core/Enums/Priority.cs`
- `src/TaskTree.Core/Enums/UpdateChannel.cs`
- `src/TaskTree.Core/Enums/BugSeverity.cs`
- `src/TaskTree.App/Bootstrap/ServiceRegistrations.cs`
- `tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs`
Change summary: Repaired stitched-repo SPEC-DERIVED marker drift so the Phase 5B marker gate reflects canonical derived files and no longer counts explanatory marker-like text in verbatim files.
Exact rationale: `tools/find-spec-derivations.ps1` failed with stale/misleading counts (`Grand total: 184 expected 177`). Inspection showed missing Phase 0 markers on canonical derived files, marker-like explanatory text in verbatim enum/model files, stale duplicate Phase 1D test-double marker text, and a stale PHASE1F count after later handoff deltas.
Why this is compile-closure only: The edits only affect comments and the verification script's expected marker inventory; no runtime code paths, public APIs, project references, package references, or tests were changed.
Why runtime behavior is unchanged or minimally changed: All changed C# files received comment-only edits.
Regression risk: Low; risk is limited to marker accounting. Mitigated by rerunning the marker script.
Tests/build commands re-run:
- `rtk powershell -NoProfile -ExecutionPolicy Bypass -File tools/find-spec-derivations.ps1 -Root .`
Command output reference: Marker script passed with all buckets OK and `Grand total: 179 expected 179`.
Follow-up gap created/updated: None.
Reviewer/owner note: Debug/Release compile remains unverified in this environment because no .NET SDK is available on PATH.
