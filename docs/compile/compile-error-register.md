# compile-error-register.md - Phase 5B Compile Error Register

> Phase 5B Compile Closure. This register must be completed by Claude/Codex against the actual stitched repository. Debug and Release builds must both pass before this register can be closed.

## Required Baseline Commands

```powershell
pwsh -File tools/find-spec-derivations.ps1 -Root .
dotnet restore TaskTree.sln
dotnet build TaskTree.sln -c Debug
dotnet build TaskTree.sln -c Release
```

Attach real command output logs during Claude/Codex execution. Do not invent results.

## Register Template

```text
Error ID:
Command:
Configuration: Debug / Release
Project:
File:
Line:
Column:
Compiler error code:
Message:
Root cause category:
Proposed fix:
Status: Open / In Progress / Resolved / Deferred
Resolution reference:
Follow-up gap:
```

## Root Cause Categories

- Missing project
- Missing package reference
- Missing using / namespace mismatch
- Constructor signature drift
- Interface contract drift
- Enum/model mismatch
- Test support fake mismatch
- XAML/code-behind mismatch
- Windows-only API / target framework mismatch
- Packaging/project path mismatch
- Duplicate type/file collision
- Nullable/reference-type issue
- Access modifier / internal test seam issue

## Compile Closure Rules

1. Fix only build/compile issues required to make the repo compile.
2. Do not redesign features or expand product scope.
3. Do not delete/exclude files unless proven duplicate or superseded.
4. Log all deletions/exclusions with rationale and downstream impact.
5. Preserve explicit Phase 5E stubs; do not fake live success.
6. New package references require source approval or owner-review documentation.

## Phase 5B Gaps

| Gap | Description | Target |
|---|---|---|
| #367 | Phase 5B compile fixes must not redesign features or expand scope beyond buildability | Phase 5B |
| #368 | Any deleted/excluded file during compile closure must be logged with rationale and downstream impact | Phase 5B |
| #369 | Marker/restore/build command outputs must be attached to compile closure artifacts | Phase 5B |
| #370 | Release configuration compile closure must be verified separately from Debug | Phase 5B |
| #371 | Maintain complete compile-error register until Debug and Release builds pass | Phase 5B |
| #373 | Compile errors must be categorized consistently | Phase 5B |
| #384 | Actual compile closure requires Claude/Codex on stitched repo with real command outputs | Claude/Codex Phase 5B |

## Register Entries

### P5B-E001 - Marker Inventory Drift

Error ID: P5B-E001
Command: `rtk powershell -NoProfile -ExecutionPolicy Bypass -File tools/find-spec-derivations.ps1 -Root .`
Configuration: N/A
Project: N/A
File: `tools/find-spec-derivations.ps1`
Line: expected marker table
Column: N/A
Compiler error code: N/A
Message: Initial marker verification failed with mismatched SPEC-DERIVED buckets and `Grand total: 184 expected 177`.
Root cause category: Marker/verifier drift from stitched handoff deltas
Proposed fix: Reconcile marker comments and script expectations against current canonical derived files.
Status: Resolved
Resolution reference: P5B-R001
Follow-up gap: None.

### P5B-E002 - .NET SDK Unavailable In Current Environment

Error ID: P5B-E002
Command: `rtk dotnet restore TaskTree.sln`; `rtk dotnet --info`
Configuration: N/A
Project: Solution
File: N/A
Line: N/A
Column: N/A
Compiler error code: N/A
Message: `rtk: Failed to resolve 'dotnet' via PATH, falling back to direct exec: Binary 'dotnet' not found on PATH`
Root cause category: Local environment/toolchain missing
Proposed fix: Re-run Phase 5B restore/build commands in an environment with the .NET 8 SDK installed and available on PATH, or install/provision the SDK with owner approval.
Status: Deferred
Resolution reference: None in this environment.
Follow-up gap: #384 remains open for actual Claude/Codex compile execution with real restore/build outputs.
