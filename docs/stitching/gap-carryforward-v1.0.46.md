# gap-carryforward-v1.0.46.md - Phase 5A Gap Carry-Forward

> Phase 5A authoritative gap carry-forward scaffold. All gaps #1-#348 remain active until explicitly closed. Phase 5A adds #349-#366.

## Source-of-Truth Rule

After bundle application, the stitched repository is the source of truth. Prior bundle fragments are historical evidence only.

## Phase 5A New Gaps

| Gap | Description | Target |
|---|---|---|
| #349 | The stitched repository, not individual chat bundles, must become authoritative source of truth after Phase 5A | Phase 5A |
| #350 | Phase 5A must avoid opportunistic compile fixes; compile/constructor/DI work belongs to Phase 5B | Phase 5A / 5B |
| #351 | Any file modified beyond copy/select/report must be explicitly logged for Phase 5B review | Phase 5A / 5B |
| #352 | Verify exact available bundle filenames and resolve missing/regenerated bundle ambiguity | Phase 5A |
| #353 | Document superseded bundles and avoid applying obsolete artifacts | Phase 5A |
| #354 | Record all duplicate file paths with selected and superseded source | Phase 5A |
| #355 | Latest-phase-wins applies only to explicitly patched files; accidental collisions require review | Phase 5A |
| #356 | Preserve all HANDOFF deltas for traceability; consolidated HANDOFF deferred | Phase 5A / Future |
| #357 | Produce complete bundle inventory | Phase 5A |
| #358 | Produce stitched file manifest for Phase 5B project inclusion review | Phase 5A / 5B |
| #359 | Preserve and classify all gaps by next closure phase | Phase 5A |
| #360 | Log bundle application order and results for reproducibility | Phase 5A |
| #361 | Verify `TaskTree.sln` contains every expected project after stitching | Phase 5B |
| #362 | Reconcile project references and namespace churn from generated modules/tests | Phase 5B |
| #363 | Attach actual restore/build/test command output logs | Phase 5B / 5C |
| #364 | Generate file-level hash manifest after stitching if release traceability requires it | Phase 5A / 5F |
| #365 | Maintain compile-error register and resolution log | Phase 5B |
| #366 | Declare stitched repo source-of-truth hierarchy | Phase 5A |

## Current Codex Status Overlay

| Gap | Current status | Evidence / remaining gate |
|---|---|---|
| #349 | Applied for current checkout | `docs/stitching/bundle-inventory.md` and `docs/stitching/stitched-file-manifest.md` now treat the stitched repository as authoritative. |
| #350 | Preserved | Compile/constructor/DI changes are logged as Phase 5B work in `docs/compile/compile-resolution-log.md`. |
| #351 | Applied for current Codex modifications | `docs/stitching/bundle-application-log.md` records Codex continuation entries. |
| #352 | Partially blocked | Original bundle filenames are unavailable in this checkout; `docs/stitching/bundle-inventory.md` records this boundary. |
| #353 | Partially applied | Superseded TestSupport and Tier 2 window state recorded in `docs/stitching/file-collision-register.md`. |
| #354 | Partially applied | Known duplicate/superseded paths recorded; more entries require original bundle evidence if available. |
| #355 | Preserved | No accidental overwrite rule was weakened; current selected/superseded paths are logged. |
| #356 | Preserved | HANDOFF deltas remain separate files. |
| #357 | Partially applied | Current evidence inventory added; original bundle filenames remain unavailable. |
| #358 | Applied statically | `docs/stitching/stitched-file-manifest.md` now lists verified source/test/tooling/packaging inventory. |
| #359 | Applied for Phase 5A rows | This overlay classifies current Phase 5A gap state; earlier gaps remain carried forward unless explicitly closed elsewhere. |
| #360 | Partially applied | `docs/stitching/bundle-application-log.md` records Codex continuation entries; original bundle application order cannot be reconstructed from absent bundle files. |
| #361 | Verified statically | `TaskTree.sln` includes current source/test projects; build proof remains blocked by missing .NET SDK. |
| #362 | Partially applied | Project references and TestSupport namespace migration were reconciled statically; compile proof remains SDK-blocked. |
| #363 | Deferred | Restore/build/test command outputs cannot be attached locally because `dotnet` is unavailable on PATH. |
| #364 | Deferred | No file-level hash manifest generated; target remains Phase 5F if release traceability requires it. |
| #365 | Applied and ongoing | Compile-error register and resolution log contain current Phase 5B entries through `P5B-E012` / `P5B-R010`. |
| #366 | Applied | Source-of-truth hierarchy is reflected in `REPO_BRIEF.md`, latest HANDOFF delta, and the stitching inventory docs. |

## Closure Phase Classification

### Phase 5B Compile Closure

- Constructor churn
- DI registration churn
- Namespace mismatches
- Project references
- Missing projects
- Package references
- Test project inclusion
- Compile-error register and resolution log

### Phase 5C Offline Validation

- Full test suite
- Compliance/security tests
- PHI-leak review
- JSON schema tests
- Audit vocabulary reconciliation
- Performance/stress category policy
- No-live-network test enforcement

### Phase 5D Documentation / Integration Reconciliation

- User/admin guide final behavior matching
- Release notes updates
- Known limitations review
- Architecture/Roadmap amendments
- Consolidated HANDOFF if desired

### Phase 5E Live Environment Validation

- MSIX build/sign/install
- First launch
- WPF/tray/hotkey behavior
- DPAPI persistence
- Credential UI/session lock
- Live updater HTTP/MSIX apply/rollback
- SMTP/GitHub delivery
- Crash injection
- RAM/CPU/UI performance

### Phase 5F Final Validation

- Compliance reconciliation
- Performance threshold reconciliation
- Package hash/signing evidence archive
- Owner release approval
- Final gap closure/acceptance register

## Claude/Codex Gap

Gap #359: Phase 5A must preserve all prior gaps and classify them by next closure phase.
