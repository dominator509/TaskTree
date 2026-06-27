# bundle-inventory.md - Phase 5A Bundle Inventory

> Phase 5A Repo Stitching / Bundle Application. Status: stitching plan artifact. The actual repository becomes authoritative only after Claude/Codex applies and verifies all bundles.

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

## Inventory Columns for Claude/Codex

For each actual bundle file, record:

- Bundle filename
- Phase
- Expected file count
- Expected marker count
- Status: Applied / Missing / Superseded / Needs Review
- Collision notes
- Selected source if regenerated

## Known Regeneration / Supersession Notes

- Phase 1H had regenerated/final artifacts; prefer the latest explicitly regenerated owner-approved bundle.
- `tools/find-spec-derivations.ps1` is regenerated repeatedly; latest accepted version wins.
- HANDOFF deltas are retained as separate files and not collapsed during Phase 5A.

## Claude/Codex Gaps

| Gap | Description | Target |
|---|---|---|
| #352 | Verify exact available bundle filenames and resolve missing/regenerated bundle ambiguity | Phase 5A |
| #353 | Document superseded bundles and avoid applying obsolete artifacts | Phase 5A |
| #357 | Produce complete bundle inventory showing applied, missing, superseded, and review-required bundles | Phase 5A |
