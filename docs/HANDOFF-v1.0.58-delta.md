# HANDOFF.md v1.0.58 - Updater Integrity Delta

**Date:** 2026-08-16  
**Scope:** Local updater staging and apply-boundary hardening

## Change

- `StagingService` validates the manifest version as a safe filename component, writes to a unique temporary file, promotes with overwrite, rereads the promoted package, and cleans temporary/promoted artifacts on failure.
- `AutoUpdater.ApplyAsync` rechecks staged package existence, size, SHA-256, and manifest signature before invoking the MSIX installer from a non-idle staged state.
- Added focused regression coverage for safe version rejection, replacement without temporary artifacts, apply-boundary revalidation, and state reset after rejection.

## Evidence

- Release build: 0 warnings, 0 errors.
- Full Release solution suite: 380 passed, 1 intentionally skipped Live desktop-metrics test.
- Non-live contract suite: 373 passed, 0 failed, 0 skipped.
- AutoUpdater focused lane: 16 passed; updater assembly total: 76 passed.
- Performance lane: 7 passed.
- Serena diagnostics: no diagnostics for changed source/test files.
- Spec derivation registry: 178 expected, 178 found.
- Packaging PowerShell AST and four Obsidian JSON files: parsed successfully.

## Carry-forward

Real WPF/tray/session E2E, live HTTP/provider checks, MSIX/WAP build/sign/install/rollback evidence, owner-controlled Q10/Q11 inputs, production updater signing key, and Phase 5F owner sign-off remain open. The last fully converted SDK production coverage report remains 1,651/2,137 lines (77.26%); the current collector rerun produced an ignored artifact but no repo-local Cobertura converter is checked in.
