# signing-checklist.md - Phase 4C MSIX + Update Signing Checklist

> Source: Roadmap Phase 4C; Architecture.md §12 and §9.1.3.  
> Status: Checklist scaffold. Final certificate/key decisions require owner and Codex/Claude Code validation.

## 1. Scope

This checklist covers MSIX Authenticode signing and updater Ed25519 manifest signing. No private keys, certificate passwords, production URLs, tokens, or secrets are committed by Phase 4C artifacts.

## 2. Authenticode / MSIX Signing Checklist

- [ ] Decide certificate source: self-signed dev, internal enterprise CA, public OV/EV code-signing certificate, or organizational certificate.
- [ ] Confirm final MSIX Publisher DN and ensure it matches the certificate subject exactly.
- [ ] Replace manifest placeholder `CN=TaskTree` if needed.
- [ ] Confirm package identity name and publisher display metadata.
- [ ] Confirm package versioning policy and synchronization with updater manifests.
- [ ] Confirm `signtool.exe` availability from Windows SDK.
- [ ] Confirm timestamp server URL and organizational timestamping policy.
- [ ] Store private keys securely; do not commit PFX/passwords to repo.
- [ ] Validate package signature on a clean Windows machine.
- [ ] Validate install trust chain for intended deployment environment.
- [ ] Archive signing evidence: package hash, signing certificate thumbprint, timestamp evidence, build log.

## 3. MSIX Packaging Checklist

- [ ] Validate `packaging/TaskTree.Installer.wapproj` on Windows with MSIX packaging tooling.
- [ ] Confirm Windows App SDK / Desktop Bridge tooling version compatibility.
- [ ] Confirm `TaskTree.App.exe` is the actual published executable name.
- [ ] Confirm `runFullTrust` capability is accepted and sufficient.
- [ ] Provide MSIX visual assets/logos.
- [ ] Run `packaging/build-msix.ps1` from a stitched repo.
- [ ] Install generated MSIX on a clean Windows user profile.
- [ ] Validate first launch.
- [ ] Validate tray icon, hotkeys, WPF window, local storage paths, updater paths, and bug reporter paths under packaged context.

## 4. Ed25519 Update Manifest Signing Checklist

- [ ] Generate Ed25519 key pair using approved tooling.
- [ ] Store Ed25519 private key offline or in approved secure storage.
- [ ] Embed Ed25519 public key in app binary.
- [ ] Assign stable `publicKeyId`, e.g. `tasktree-stable-2026`.
- [ ] Produce canonical JSON test vector excluding `signature` object.
- [ ] Add deterministic positive signature verification test vector.
- [ ] Define key rotation process and public-key ID transition strategy.
- [ ] Compute SHA-256 package hash for each MSIX release artifact.
- [ ] Sign manifest with Ed25519 private key.
- [ ] Verify manifest signature and package hash before release publication.

## 5. Gap Register for Claude/Codex Handoff

| Gap | Description | Target |
|---|---|---|
| #315 | Packaging artifacts require Phase 5E live Windows validation before MSIX build/install success can be claimed | Phase 5E |
| #316 | Secure cert/password handling for MSIX signing must be provided without committing secrets | Phase 5E |
| #317 | Authenticode cert source and Ed25519 manifest signing key process remain owner/Codex decisions | Phase 5E / Owner |
| #318 | `.wapproj` must be validated/normalized on Windows with MSIX tooling | Phase 5E |
| #319 | Final `.wapproj` project references must be reconciled against stitched repo projects | Phase 5A / 5B |
| #320 | Windows App SDK / MSIX packaging tooling version compatibility must be confirmed | Phase 5E |
| #321 | Final MSIX package identity name must be confirmed | Owner / Phase 5E |
| #322 | MSIX Publisher must match actual signing certificate subject | Phase 5E |
| #323 | Package versioning must synchronize with updater manifest and release process | Phase 4D / 5E |
| #324 | Publisher display/app metadata require owner review | Owner / Phase 4D |
| #325 | Published executable and manifest executable must match | Phase 5B / 5E |
| #326 | MSIX capabilities must be validated against WPF/tray/hotkey/runtime needs | Phase 5E |
| #327 | MSIX visual assets/logos must be created or mapped | Phase 4D / 5E |
| #328 | `build-msix.ps1` requires live Windows packaging tools and may need Codex normalization | Phase 5E |
| #329 | ARM64 packaging target should be added/validated if wanted | Owner / Phase 5E |
| #330 | Authenticode certificate/timestamp policy must be finalized | Owner / Phase 5E |
| #331 | Updater Ed25519 signing procedure and rotation policy must be finalized | Owner / Phase 5E |
| #332 | Marker script must support non-C# phases without changing grand total | Phase 4C / 5A |

## 6. Release Evidence to Archive

- Build log from `build-msix.ps1`.
- `dotnet publish` output path and hash.
- MSIX file path and SHA-256 hash.
- Authenticode signature verification output.
- Timestamp verification output.
- Manifest Ed25519 signature verification output.
- First-launch validation evidence.
- Owner sign-off.
