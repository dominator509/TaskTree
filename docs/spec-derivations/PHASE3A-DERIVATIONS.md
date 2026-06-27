# PHASE3A-DERIVATIONS.md - AutoUpdater Core (Manifest + Verify)

> Scope: UpdateManifest schema, AutoUpdater VerifyAsync, ManifestSigner Ed25519 verification, HashVerifier SHA-256 verification, tests.
> Source: Roadmap Phase 3A; Architecture.md Sections 4.7 and 9.1.1-9.1.6.
> Owner-approved HALT batch: 22 items.

## Section 1 HALT Summary

All 22 HALT items approved as proposed. Phase 3A implements manifest schema compatibility, canonical signing payload generation, Ed25519 verification wrapper, SHA-256 hash verification, payload size check, and AutoUpdater.VerifyAsync. CheckAsync remains no-network in 3A. ApplyAsync and ImportLocalAsync remain deferred with explicit NotImplementedException reasons.

## Section 2 Files Produced

| Path | Purpose | Marker |
|---|---|---|
| src/TaskTree.Core/Models/UpdateManifest.cs | PATCH manifest schema | PHASE3A |
| src/TaskTree.Modules.AutoUpdater/TaskTree.Modules.AutoUpdater.csproj | PATCH NSec.Cryptography | csproj |
| src/TaskTree.Modules.AutoUpdater/AutoUpdater.cs | NEW/PATCH VerifyAsync | PHASE3A |
| src/TaskTree.Modules.AutoUpdater/ManifestSigner.cs | NEW Ed25519 verifier | PHASE3A |
| src/TaskTree.Modules.AutoUpdater/HashVerifier.cs | NEW SHA-256 verifier | PHASE3A |
| tests/TaskTree.Modules.AutoUpdater.Tests/TaskTree.Modules.AutoUpdater.Tests.csproj | PATCH/NEW test project | csproj |
| tests/TaskTree.Modules.AutoUpdater.Tests/AutoUpdaterTests.cs | NEW tests | PHASE3A |
| tests/TaskTree.Modules.AutoUpdater.Tests/ManifestSignerTests.cs | NEW tests | PHASE3A |
| tests/TaskTree.Modules.AutoUpdater.Tests/HashVerifierTests.cs | NEW tests | PHASE3A |
| docs/spec-derivations/PHASE3A-DERIVATIONS.md | This registry | md |
| docs/HANDOFF-v1.0.36-delta.md | Phase 3A delta | md |
| tools/find-spec-derivations.ps1 | PHASE3A=7 | script |

## Section 3 Marker Inventory

PHASE3A = 7 distinct .cs files. Grand total: 126 -> 133.

## Section 4 Cross-Phase Gaps Introduced (209-221)

| # | Gap | Target | Action |
|---|---|---|---|
| 209 | Verify/patch UpdateManifest.cs to match Architecture Section 9.1.2 | Phase 3A/5B | Confirm during repo stitching/build |
| 210 | Nested manifest DTO names are derived; Architecture lists only UpdateManifest.cs | Architecture v1.0.3/5F | Document nested DTOs |
| 211 | JSON casing compatibility tests required for exact manifest schema | Phase 5C | Add schema test vectors |
| 212 | Version comparison / monotonicity rules deferred | Phase 3B | Define before update eligibility |
| 213 | Verify exact NSec.Cryptography package version resolves | Phase 5B | Restore/build validation |
| 214 | Real Ed25519 public key material required before production updater verification | Phase 4C/5E | Provide production key |
| 215 | Manifest canonicalization not specified; Phase 3A uses canonical JSON excluding signature | Owner/5C | Approve and add vectors |
| 216 | Hash casing unspecified; Phase 3A accepts upper/lowercase HEX_64 | Phase 5C | Confirm compatibility |
| 217 | Phase 3B must re-check size/hash after download/staging | Phase 3B | Re-verify staged file |
| 218 | CheckAsync, ApplyAsync, ImportLocalAsync deferred | Phase 3B/3C/5E | Implement later phases |
| 219 | AutoUpdater audit vocabulary deferred | Phase 3B/3C/4A | Define events |
| 220 | Verify AutoUpdater test project exists after stitching | Phase 5A/5B | Create/patch if missing |
| 221 | Deterministic Ed25519 positive test vector required | Phase 3A/5C | Add real vector after key material |

## Section 5 Known Limitations

1. ManifestSigner uses placeholder public key and therefore only negative signature paths are meaningful until production/test key material is provided.
2. CheckAsync performs no live HTTP in Phase 3A.
3. ApplyAsync and ImportLocalAsync remain explicit stubs for later updater phases.
4. Canonicalization needs owner acceptance and Phase 5C test vectors.
5. Version comparison is not implemented until Phase 3B.
