# Handoff Delta v1.0.81

**Date:** 2026-08-16
**Scope:** AutoUpdater state cleanup after observer failure

## Changes

- `AutoUpdater.ApplyAsync` now attempts to transition through `Failed` and reset to `Idle` for every non-idle failure state.
- Cleanup remains effective when a `StateChanged` observer throws after the state machine has already assigned `Applied` or `Failed`.
- Public contracts and architecture remain unchanged; the original operation/observer exception is still propagated.

## Evidence

- AutoUpdater focused lane: 86 non-live tests passed.
- Matching .NET 8 validation is recorded in `docs/test-gap-report.md` and `docs/final-validation-report.md`.

## Remaining Gates

- Interactive WPF/session/CredUI validation, MSIX assets/signing/install, live providers, EULA/Compliance Mode owner policy, Q10/Q11 inputs, production updater signing, and Phase 5F sign-off remain open.
