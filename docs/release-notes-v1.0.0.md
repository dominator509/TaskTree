# TaskTree Release Notes v1.0.0 Draft

> Phase 4D release-notes draft. Final release notes must be updated after Phase 5 validation with actual build number, package hash, signing evidence, and tested limitations.

## Release Summary

TaskTree v1.0.0 is planned as a local Windows task reminder application with privacy-aware storage, audit, reminders, session lock behavior, update scaffolding, and bug-report capture/delivery scaffolding.

## New Features

- Local task management.
- Reminder scheduling and multi-tier delivery scaffolding.
- Snooze behavior.
- Secure local storage design.
- Audit-chain integrity design.
- Session lock and privacy-mode scaffolding.
- AutoUpdater manifest/signature/hash/staging/offline import scaffolding.
- BugReporter redaction, queue, file-drop, and delivery routing scaffolding.
- MSIX packaging scaffold.

## Security and Compliance Notes

TaskTree is designed as a local HIPAA-aware tool. Final compliance depends on deployment configuration, validation, organizational policy, and Phase 5F owner sign-off.

## Known Limitations Before Phase 5 Validation

- Live MSIX build/sign/install not yet validated.
- Live updater HTTP, MSIX apply, and rollback not yet validated.
- Live SMTP/GitHub delivery not yet implemented.
- Live crash injection not yet validated.
- Tray/UI/RAM/CPU performance not yet measured.
- DPAPI and Credential UI behavior require live Windows validation.
- Release signing and Ed25519 update signing require final owner/Codex decisions.

## Deferred Live Integrations

- MSIX signing and installation.
- Update download and package apply.
- MSIX rollback restore.
- SMTP delivery.
- GitHub issue delivery.
- Live crash capture validation.
- Live tray/hotkey/WPF validation.

## Validation Status

Current status: generated scaffolds and documentation complete through Phase 4D after approval. Phase 5 remains mandatory before release readiness.

## Upgrade / Update Notes

Updater manifest signing, package hash verification, and offline import scaffolding exist. Production update release flow requires Ed25519 key procedure, package hash generation, manifest signing, and live update validation.

## Support / Troubleshooting Notes

See `docs/admin-guide.md` and `docs/deployment-checklist.md`.

## Claude/Codex Handoff Gap

Gap #339: Release notes must be updated after Phase 5 validation with actual build number, package hash, signing evidence, and tested limitations.
