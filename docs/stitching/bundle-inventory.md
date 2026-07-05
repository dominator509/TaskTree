# bundle-inventory.md - Phase 5A Bundle Inventory

> Phase 5A Repo Stitching / Bundle Application.
> Status: current stitched repository is authoritative; original bundle filenames are not present in this checkout.

## Application Order

Apply bundles in chronological phase order. If multiple bundles exist for one phase, use the latest explicitly regenerated / owner-approved bundle and document superseded bundles.

```text
Initial Architecture/Roadmap/HANDOFF source files
Phase Msg2-Msg6 base architecture bundles
Phase 1A
Phase 1B
Phase 1C
Phase 1D Msg1
Phase 1D Msg2
Phase 1E Msg1
Phase 1E Msg2
Phase 1F Msg1
Phase 1F Msg2
Phase 1G Msg1
Phase 1G Msg2
Phase 1H final/v2 bundle
Phase 2A Msg1
Phase 2A Msg2
Phase 2B Msg1
Phase 2B Msg2
Phase 2C Msg1
Phase 2C Msg2
Phase 2D
Phase 2E Msg1
Phase 2E Msg2
Phase 2F
Phase 2G
Phase 3A
Phase 3B
Phase 3C
Phase 3D
Phase 3E
Phase 3F
Phase 4A
Phase 4B
Phase 4C
Phase 4D
Phase 5A
```

## Current Repo Evidence

The checkout contains stitched outputs and phase evidence, not the original external bundle files. Current evidence includes:

- authority docs: `Architecture.md`, `Roadmap.md`, `docs/HANDOFF*.md`
- derivation registries: `docs/spec-derivations/PHASE0-MSG2` through `PHASE3E`
- phase summaries/manifests: Phase 1H, Phase 2D, Phase 3, Phase 4, and Phase 5A manifests
- stitching artifacts: `docs/stitching/*.md`
- compile closure artifacts: `docs/compile/*.md`
- source/test project inventory: see `docs/stitching/stitched-file-manifest.md`
- marker verifier: `tools/find-spec-derivations.ps1`, currently green at `Grand total: 179 expected 179`

## Inventory Boundary

| Item | Current status | Evidence / note |
|---|---|---|
| Exact original bundle filenames | Unavailable in checkout | Do not invent filenames; use current repo files as source of truth. |
| Bundle application order | Preserved as planned order | See Application Order above and `docs/HANDOFF-v1.0.46-delta.md`. |
| Stitched file inventory | Applied | See `docs/stitching/stitched-file-manifest.md`. |
| Collision register | Partially applied | TestSupport duplicate and obsolete Tier 2 window pair are recorded in `docs/stitching/file-collision-register.md`. |
| Application log | Partially applied | Codex continuation sequences `C-001` through `C-003` are recorded in `docs/stitching/bundle-application-log.md`; original bundle order remains unreconstructed without the source bundles. |
| Restore/build/test outputs | Deferred | Blocked locally by missing .NET SDK; see `docs/compile/compile-error-register.md` entry `P5B-E002`. |

## Known Regeneration / Supersession Notes

- Phase 1H had regenerated/final artifacts; prefer the latest explicitly regenerated owner-approved bundle.
- `tools/find-spec-derivations.ps1` is regenerated repeatedly; latest accepted version wins.
- HANDOFF deltas are retained as separate files and not collapsed during Phase 5A.
- `TaskTree.TestSupport` supersedes old `tests/TaskTree.Core.Tests/TestDoubles/` files.
- Phase 2B `ReminderToast` supersedes the Phase 1G Tier 2 WPF window, but deletion remains gated on a successful Release build.

## Claude/Codex Gaps

| Gap | Description | Target |
|---|---|---|
| #352 | Verify exact available bundle filenames and resolve missing/regenerated bundle ambiguity | Partially blocked: original bundles unavailable in checkout |
| #353 | Document superseded bundles and avoid applying obsolete artifacts | Partially applied via collision register |
| #357 | Produce complete bundle inventory showing applied, missing, superseded, and review-required bundles | Partially applied via current repo evidence inventory |
