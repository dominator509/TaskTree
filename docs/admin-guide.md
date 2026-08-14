# TaskTree Admin Guide

> Phase 4D admin/deployment documentation scaffold. Final admin guidance must be validated after MSIX install on a clean Windows profile.

## System Requirements

- Windows desktop environment capable of running .NET 8 Windows applications.
- MSIX installation support.
- User profile with local app data storage.
- Windows SDK tooling for build/sign validation during release operations.

## Installation Prerequisites

1. Stitch all generated phase bundles into the repository.
2. Restore and build the solution.
3. Run the offline test suite.
4. Build the MSIX package.
5. Sign the package with an appropriate certificate.
6. Verify certificate trust on the target machine.

## MSIX Installation Overview

MSIX packaging artifacts are under `packaging/`:

- `packaging/TaskTree.Installer.wapproj`
- `packaging/Package.appxmanifest`
- `packaging/build-msix.ps1`

The packaging scaffold must be validated with live Windows packaging tooling in Phase 5E.

## Signing and Trust Requirements

Review `docs/signing-checklist.md` before release packaging. The MSIX Publisher DN must match the signing certificate subject. Do not commit certificate private keys, PFX passwords, access tokens, or other secrets.

## Local Storage Paths

Expected local paths include:

```text
%LOCALAPPDATA%\TaskTree\
%LOCALAPPDATA%\TaskTree\updates\
%LOCALAPPDATA%\TaskTree\updates\rollback\
%LOCALAPPDATA%\TaskTree\bugreports\
%LOCALAPPDATA%\TaskTree\bugreports\out\
%LOCALAPPDATA%\TaskTree\sentinel.lock
%LOCALAPPDATA%\TaskTree\keys\master.bin
```

These paths must be validated under installed MSIX context in Phase 5E.

## DPAPI and User Profile Considerations

TaskTree is designed to use user-bound key protection for local encrypted storage. Validation must confirm key persistence across restarts and proper failure behavior under a different user profile.

## Compliance Mode Defaults

TaskTree is designed to be HIPAA-aware by default. Compliance Mode semantics must be reconciled across settings, redaction, auto-logoff, and audit enforcement before release.

## Update Process

Updater code includes HTTPS manifest verification, opt-in package download, hash-verified staging, offline import, MSIX apply, and rollback. Provider/package execution still requires Phase 5E environment validation.

## BugReporter Delivery Modes

- FileDrop: local redacted JSON output.
- Email: runtime-configured SMTP; missing or incomplete settings fail closed.
- GitHub: runtime-configured GitHub Issues REST delivery; missing settings fail closed.

## Troubleshooting

### Package install failure

- Verify MSIX signature.
- Verify certificate trust.
- Verify manifest Publisher matches certificate subject.
- Verify `runFullTrust` capability acceptance.

### Certificate trust failure

- Confirm certificate chain and timestamp.
- Confirm target machine trusts the issuer.

### First launch failure

- Check local app data permissions.
- Check MSIX package identity and executable name.
- Validate WPF/tray startup logs.

### Local storage permission issue

- Confirm `%LOCALAPPDATA%\TaskTree\` is writable for the user.
- Confirm packaged app context does not redirect unexpectedly.

### DPAPI/key issue

- Confirm the same Windows user profile is used.
- Validate key creation and persistence.

### Update import failure

- Verify ZIP format and entry names.
- Verify manifest signature and SHA-256 hash.

### Bug report delivery/file drop issue

- Confirm report is redacted.
- Confirm file-drop output path is writable.

### Session lock/re-auth issue

- Validate Windows idle detection and Credential UI behavior in Phase 5E.

## Logs and Audit Data

Audit and logs must avoid PHI payloads. Audit vocabulary must be reconciled during Phase 5C/5F.

## Uninstall and Cleanup

Uninstall behavior must be validated in Phase 5E, including whether local encrypted data, audit data, update staging data, and bug report outputs remain or are cleaned up according to policy.

## Claude/Codex Handoff Gaps

- #337: Admin guide must be validated on a clean Windows profile after MSIX install in Phase 5E.
- #341: Local storage paths must be validated under installed MSIX context in Phase 5E.
- #342: Troubleshooting guidance must be updated with actual Phase 5E findings.
