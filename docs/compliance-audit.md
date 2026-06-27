# compliance-audit.md - Phase 4A Compliance Audit

> Source: Roadmap Phase 4A; Architecture.md Section 10.  
> Status: Preliminary evidence-based compliance audit. Final compliance status requires Phase 5A-5F repo stitching, build, tests, live validation, installed-binary review, and owner sign-off.

## 1. Executive Summary

TaskTree is designed as a HIPAA-aware local Windows application that treats task content as possible PHI. Phase 4A maps Architecture Section 10 controls to generated artifacts and explicitly records unresolved compliance, security, audit, and environment gaps. This audit is not a production compliance certification.

## 2. Scope and Limitations

This audit reviews chat-generated artifacts through Phase 3F and Phase 4A stress-test scaffolding. The repo has not been stitched, compiled, executed, installed, or validated in a real Windows desktop environment in this chat context.

Status labels used:

- Implemented in chat-generated artifact
- Partially implemented
- Stubbed / environment gap
- Deferred to Phase 5E
- Requires Phase 5B compile verification
- Requires Phase 5C test verification
- Requires Phase 5F final validation

## 3. Architecture Section 10 Control Matrix

| Section | Control | Preliminary Status | Evidence | Required Follow-up |
|---|---|---|---|---|
| 10.1 | HIPAA technical safeguard basis | Partially implemented | SecureStore, ComplianceCore, BugReporter, SessionLock design artifacts | Gap #285: validate safeguard intent in Phase 5F |
| 10.2 | All task content treated as PHI; no PHI in logs/telemetry/bug reports | Partially implemented | RedactionPipeline, FileDrop unredacted rejection, audit payload posture | Gap #286: repo-wide PHI leak review |
| 10.3 | AES-256-GCM at rest, DPAPI-wrapped master key | Implemented by design; needs live verification | SecureStore / MasterKeyManager generated earlier | Gap #287: live DPAPI validation |
| 10.4 | DPAPI user binding, auto-logoff, re-auth | Partially implemented / environment deferred | SessionLock artifacts, settings/lock behavior | Gap #288: GetLastInputInfo + CredUI live validation |
| 10.5 | Hash-chained audit schema | Implemented by design; stress tests added | ComplianceCore/AuditChainWriter/HashChain and Phase 4A tests | Gap #289: action vocabulary + PHI-safety validation |
| 10.6 | TLS 1.2+, cert pinning, secure outbound transport | Deferred / stubbed | Live HTTP/SMTP/GitHub intentionally stubbed | Gap #290: live transport implementation/validation |
| 10.7 | Integrity controls and startup chain verification | Partially implemented | Audit chain and SecureStore authenticated encryption design | Gap #291: startup verify/warning/export validation |
| 10.8 | BAA not required; EULA consent recorded | Mostly deferred | No full EULA flow found in generated deltas | Gap #292: EULA capture/audit verify or implement |
| 10.9 | Breach/tamper warning and export readiness | Deferred / partially designed | Audit-chain verification pieces exist | Gap #293: warning UI/export workflow validation |
| 10.10 | HIPAA mode default ON; strict redaction/logoff/audit | Partially implemented; policy confirmation needed | Settings, BugReporter redaction posture, SessionLock | Gap #294: confirm Compliance Mode semantics |

## 4. Evidence File Map

### Encryption and persistence

- `src/TaskTree.Modules.SecureStore/SecureStore.cs`
- `src/TaskTree.Modules.SecureStore/MasterKeyManager.cs`
- `src/TaskTree.Core/Security/AesGcmCryptoProvider.cs`

### Audit logging and integrity

- `src/TaskTree.Modules.ComplianceCore/ComplianceCore.cs`
- `src/TaskTree.Modules.ComplianceCore/AuditChainWriter.cs`
- `src/TaskTree.Core/Security/HashChain.cs`
- `tests/TaskTree.Modules.ComplianceCore.Tests/AuditChainStressTests.cs`

### Access control and session lock

- `src/TaskTree.Modules.SessionLock/*`
- `src/TaskTree.UI/Views/SessionLockView.xaml`
- `src/TaskTree.UI/ViewModels/SessionLockViewModel.cs`

### PHI-safe bug reporting

- `src/TaskTree.Modules.BugReporter/RedactionPipeline.cs`
- `src/TaskTree.Modules.BugReporter/BugReportQueue.cs`
- `src/TaskTree.Modules.BugReporter/FileDropAdapter.cs`
- `src/TaskTree.Modules.BugReporter/DeliveryRouter.cs`

### Updater integrity and transport

- `src/TaskTree.Modules.AutoUpdater/ManifestSigner.cs`
- `src/TaskTree.Modules.AutoUpdater/HashVerifier.cs`
- `src/TaskTree.Modules.AutoUpdater/StagingService.cs`
- `src/TaskTree.Modules.AutoUpdater/OfflineImportService.cs`

## 5. Audit Logging and Hash-Chain Review

Architecture Section 10.5 requires hash-chained audit entries. Earlier generated artifacts provide a ComplianceCore and AuditChainWriter foundation. Phase 4A adds stress tests for 10k and 100k chain operations and tamper detection. Performance assertions are intentionally smoke-level in generated tests; strict timing must be measured on Codex/Windows host.

Known gaps:

- #289: Validate all modules emit approved audit actions without PHI payloads.
- #295: Measure audit-chain performance targets on Codex/Windows host.
- #296: Decide whether Stress-category tests run by default or release-only.
- #297: Reconcile stress-test constructors after repo stitching.
- #298: Audit vocabulary appendix is preliminary.
- #299: AutoUpdater and BugReporter audit vocabularies need final names.

## 6. Encryption and Key Management Review

SecureStore and master-key design target AES-256-GCM and DPAPI user-scope key wrapping. Because DPAPI is Windows-environment dependent, final verification is deferred to Phase 5E.

Known gap:

- #287: Validate DPAPI master-key wrap/unwrap and restart persistence under a real Windows profile.

## 7. Access Control and Auto-Logoff Review

Session lock and privacy behaviors were generated in Phase 2F, but real Windows idle detection and Credential UI re-authentication remain environment-dependent.

Known gap:

- #288: Implement and validate real idle detection and Credential UI re-authentication in Phase 5E.

## 8. PHI Redaction and BugReporter Review

BugReporter is designed to redact reports before queueing or file drop. FileDrop rejects reports not marked redacted. However, this must be validated by repo-wide tests and code review.

Known gaps:

- #247: Validate no unredacted free text is persisted.
- #256: Confirm RedactionEnabled=false still-redacts policy.
- #258: Confirm default crash severity policy.
- #269: Verify FileDrop never writes unredacted reports.
- #286: Perform repository-wide PHI leak review.

## 9. Transmission Security Review

Live network integrations are intentionally stubbed in chat-generated code. TLS, certificate pinning, SMTP security, GitHub REST security, and DPAPI-wrapped credentials are Phase 5E work.

Known gaps:

- #263: Live SMTP delivery remains Phase 5E environment gap.
- #264: SMTP configuration and DPAPI credential storage deferred.
- #265: Live GitHub Issues delivery deferred.
- #290: TLS/cert pinning/transport security validation deferred.

## 10. Integrity Controls and Tamper Readiness

Audit chain, SecureStore authenticated encryption, update manifest verification, package hash validation, and offline import rejection paths are represented in generated artifacts. Startup verification and user-visible tamper response still require integration/live validation.

Known gaps:

- #291: Startup audit-chain verification and warning/export behavior.
- #293: Breach/tamper warning UI and last-known-good export workflow.

## 11. Compliance Mode Behavior

Compliance mode semantics span settings, audit enforcement, redaction, and auto-logoff. Generated artifacts imply HIPAA-safe defaults, but final policy must be confirmed.

Known gap:

- #294: Confirm Compliance Mode semantics across SettingsService, BugReporter.RedactionEnabled, auto-logoff, and audit enforcement.

## 12. Preliminary Audit Vocabulary Appendix

Known generated or planned audit actions include:

- TaskEngine: TaskAdded, TaskUpdated, TaskDeleted, TaskCompleted
- Settings: SettingsSaved, SettingsReset
- SessionLock: SessionLockStarted, SessionLockStopped, SessionLocked, SessionUnlocked
- Snooze: SnoozeCreated, SnoozeCleared, SnoozeExpired
- ReminderDelivery: DeliveredViaTier1, DeliveredViaTier2, DeliveredViaTier3, DeliveryFailedAllTiers, DeliverySkippedSnoozed
- AutoUpdater: Pending final vocabulary
- BugReporter: Pending final vocabulary

This appendix is preliminary and must be reconciled with actual `AuditEntry.Action` values after repo stitching.

## 13. Known Gaps and Codex/Claude Handoff

New Phase 4A gaps:

- #282: Compliance audit preliminary until Phase 5A-5F validation.
- #283: Phase 5F must reconcile preliminary statuses with installed binary evidence.
- #284: Compliance audit consolidates cross-phase compliance-relevant gaps.
- #285: Validate HIPAA safeguard implementation intent.
- #286: Repository-wide PHI-leak review required.
- #287: DPAPI validation required.
- #288: Real idle detection and CredUI required.
- #289: Audit vocabulary and PHI safety validation required.
- #290: TLS/cert pinning/SMTP/GitHub transport security deferred.
- #291: Startup chain verification/warning/export validation required.
- #292: EULA consent capture/audit remains unverified or unimplemented.
- #293: Breach/tamper warning and export workflow validation required.
- #294: Compliance Mode semantics require confirmation.
- #295: Audit-chain performance targets must be measured on Codex/Windows host.
- #296: Stress-category test execution policy required.
- #297: AuditChainStressTests may need constructor reconciliation.
- #298: Audit vocabulary appendix is preliminary.
- #299: AutoUpdater and BugReporter audit vocabularies require final action names.

## 14. Phase 4A Conclusion

Phase 4A provides a preliminary compliance audit and stress-test scaffold. It does not certify production compliance. Final release readiness requires Phase 5A repo stitching, Phase 5B compile closure, Phase 5C offline test closure, Phase 5D integration validation, Phase 5E live environment closure, and Phase 5F final validation with owner sign-off.
