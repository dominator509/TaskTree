# TaskTree Deployment Checklist

> Phase 4D deployment checklist scaffold. This document must become an evidence checklist during Phase 5F.

## Pre-Build Validation

- [ ] Apply all generated phase bundles in correct order.
- [ ] Confirm latest HANDOFF delta is applied.
- [ ] Confirm no stale file overwrite occurred.
- [ ] Run marker script.
- [ ] Confirm no secrets are committed.
- [ ] Review `docs/compliance-audit.md`.
- [ ] Review `docs/perf-report.md`.
- [ ] Review `docs/signing-checklist.md`.

## Build Validation

- [ ] Restore solution.
- [ ] Build all projects.
- [ ] Resolve constructor churn.
- [ ] Resolve DI registration churn.
- [ ] Resolve namespace/project reference issues.
- [ ] Resolve package reference issues.

## MSIX Packaging and Signing

- [ ] Validate `packaging/TaskTree.Installer.wapproj`.
- [ ] Validate `packaging/Package.appxmanifest`.
- [ ] Run `packaging/build-msix.ps1`.
- [ ] Confirm MSIX output path.
- [ ] Compute MSIX SHA-256 hash.
- [ ] Confirm certificate source.
- [ ] Confirm Publisher DN matches certificate subject.
- [ ] Sign package.
- [ ] Verify Authenticode signature.
- [ ] Verify timestamp.
- [ ] Archive signing evidence.

## Install and First-Launch Validation

- [ ] Install MSIX on clean Windows profile.
- [ ] Verify install trust chain.
- [ ] Verify application identity.
- [ ] Launch app.
- [ ] Confirm tray icon.
- [ ] Confirm WPF window.
- [ ] Confirm hotkeys.
- [ ] Confirm local storage path creation.
- [ ] Confirm DPAPI key creation.
- [ ] Confirm no first-launch crash.
- [ ] Verify uninstall behavior.

## Compliance Validation

- [ ] Repository-wide PHI leak review.
- [ ] Audit vocabulary reconciliation.
- [ ] Compliance Mode semantics review.
- [ ] Redaction behavior validation.
- [ ] Session lock / idle / CredUI validation.
- [ ] Audit-chain tamper validation.

## Performance Validation

- [ ] Run `Performance` category tests.
- [ ] Run `Stress` category tests according to policy.
- [ ] Run `Live` category tests on installed app.
- [ ] Attach timings to `docs/perf-report.md`.
- [ ] Confirm final Architecture §15 thresholds.

## Update Validation

- [ ] Generate Ed25519 key/test vector.
- [ ] Embed public key.
- [ ] Sign update manifest.
- [ ] Verify manifest signature.
- [ ] Verify package SHA-256.
- [ ] Validate live HTTP check.
- [ ] Validate MSIX apply.
- [ ] Validate rollback flow.

## BugReporter Validation

- [ ] Validate redaction before queue.
- [ ] Validate FileDrop rejects unredacted reports.
- [ ] Validate SMTP delivery if enabled.
- [ ] Validate GitHub delivery if enabled.
- [ ] Validate rate limits.
- [ ] Validate crash capture with live crash injection.

## Phase 5A Repo Stitching Handoff

- [ ] Unzip/apply all phase bundles in order.
- [ ] Reconcile duplicate files.
- [ ] Apply latest HANDOFF deltas.
- [ ] Verify project paths.
- [ ] Verify solution references.
- [ ] Verify marker script.

## Phase 5B Compile Closure

- [ ] Restore solution.
- [ ] Compile all projects.
- [ ] Reconcile constructors.
- [ ] Reconcile DI registrations.
- [ ] Fix namespace/project-reference issues.
- [ ] Update test projects.

## Phase 5C Offline Validation

- [ ] Run unit tests.
- [ ] Run integration tests.
- [ ] Run compliance/security tests.
- [ ] Run performance smoke tests.
- [ ] Run stress tests according to policy.
- [ ] Verify no live network in offline tests.
- [ ] Verify no PHI-like test data.

## Phase 5E Live Validation

- [ ] MSIX build/sign/install.
- [ ] First launch.
- [ ] Tray/WPF/hotkey behavior.
- [ ] DPAPI persistence.
- [ ] CredUI/session lock.
- [ ] Updater live HTTP.
- [ ] MSIX apply/rollback.
- [ ] SMTP/GitHub delivery.
- [ ] Crash injection.
- [ ] RAM/CPU/UI performance.

## Phase 5F Final Validation

- [ ] Compliance status reconciled.
- [ ] Performance thresholds reconciled.
- [ ] Release notes updated.
- [ ] Known limitations approved.
- [ ] Package hash archived.
- [ ] Signing evidence archived.
- [ ] Owner release approval recorded.

## Claude/Codex Handoff Gaps

- #338: Deployment checklist must be converted into actual release evidence during Phase 5F.
- #343: Phase 5A must verify all generated bundles are applied in correct order.
- #344: Phase 5B compile closure must reconcile constructor/namespace/DI/project churn.
- #345: Phase 5C must define and execute final offline validation matrix.
- #346: Phase 5E must execute live Windows/environment validation matrix and attach evidence.
- #347: Phase 5F must collect final owner sign-off and release evidence archive.
- #348: Phase 5A must become authoritative source for applying all bundles to actual repo.
