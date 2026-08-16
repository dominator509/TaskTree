# TaskTree — HANDOFF.md (v1.0.14 — additive delta over v1.0.13)

> **Read me first:** This is an **additive delta** over `HANDOFF.md` v1.0.13
> (chain: 1.0.14 ← 1.0.13 ← ... ← 1.0.0). Per the project guardrail "nothing
> is ever deleted, only added or reordered", only sections that changed this
> session are reproduced below. To produce a flat v1.0.14, merge these
> overrides into v1.0.13 in section order.

---

## Document Identity (updated)

| Field | Value |
|---|---|
| Session ID | 2026-05-26-15 |
| Document Version | 1.0.14 |
| State | Phase 0 ✅ + Phase 1A ✅ + Phase 1B ✅ + **Phase 1C ✅ COMPLETE** (all 5 Msgs; module + tests); Phase 1D awaiting HALT |
| Architecture.md Version | 1.0.1 |
| Roadmap.md Version | 1.0.0 |
| Current Phase | Phase 1 — Core MVP |
| Current Sub-Phase | Phase 1C ✅ complete; next is Phase 1D (ReminderScheduler + CadencePolicy) |

---

## Files Produced — status changes (cumulative through Phase 1C Msg 5)

| Phase.Item | FilePath | Status | Chat Complexity | Notes |
|---|---|---|---|---|
| P1.C (Msg 5) | `tests/TaskTree.Modules.ComplianceCore.Tests/AuditChainWriterTests.cs` | ✅ Complete | LOW | Generated Phase 1C Msg 5, 2026-05-26 — **SPEC-DERIVED-PHASE1C** (verifies derivation §4 sequence policy). 2 tests: `AppendAsync_AssignsMonotonicallyIncreasingSeq` (caller-supplied `Seq=999` → writer overwrites to 1, 2, 3 — proves security invariant), `AppendAsync_FirstEntry_UsesGenesisPrevHash` (PrevHash == `HashChain.GenesisPrevHash`). **Phase 1C 13-test plan complete.** |

All earlier rows from v1.0.13 remain unchanged.

---

## Decisions Log

No new rows this session — all 8 derivations approved during Phase 1C Msg 1 HALT.

---

## Open Questions — status update

Q10 + Q11 status **unchanged** from v1.0.13 — both remain **ACTIVE LOAD-BEARING** open questions blocking Phase 5F sign-off:

- **Q10 (Phase 5F blocker):** synthetic 10-name sentinel set in `PhiRedactor.NameSentinelRegex`. **Resolution still pending owner-supplied real source list.**
- **Q11 (Phase 5F blocker):** `PhiRedactor` constructor accepts `IReadOnlyCollection<string>? allowedEmails`; DI default at Phase 1F will be empty unless owner populates. **Resolution still pending.**

---

## Updated Roadmap — Visual Phase Tracker (updated rows only)

```
PHASE 1 — CORE MVP                                     [In Progress]
├── 1A ✅ TaskEngine + tests                          (Msg 1+2 ✅ 2026-05-26; 7+1 SPEC-DERIVED-PHASE1A)
├── 1B ✅ SecureStore + MasterKeyManager + IMasterKeyManager  (Msg 1+2 ✅ 2026-05-26; 6+1 SPEC-DERIVED-PHASE1B)
├── 1C ✅ ComplianceCore baseline                    (Msgs 1-5 ✅ 2026-05-26; 3 production + 13 tests; 8 SPEC-DERIVED-PHASE1C; **Q10 + Q11 LOAD-BEARING active**)
├── 1D ⬚ ReminderScheduler + CadencePolicy            (Msg 1 HALT: promote FakeClock per PHASE1C §Implementation note)
├── 1E ⬚ TrayHost (HIGH-stub for Codex 5E)
├── 1F ⬚ Orchestrator wiring
├── 1G ⬚ Reminder Tier 1/2/3 (Tier 1/3 stub)
└── 1H ⬚ Phase 1 E2E offline gate                    [GATE: owner approval]
```

All other phases unchanged from v1.0.13.

**Progress:** 23 of 35 deliverables complete → **65.7%**

---

## Next Action Block (updated)

| Field | Value |
|---|---|
| Phase | **Phase 1 — Core MVP** |
| Sub-Phase | Phase 1D — ReminderScheduler + CadencePolicy |
| First File | `src/TaskTree.Modules.ReminderScheduler/ReminderScheduler.cs` |
| Architecture Reference | §4.3, §5.3 |
| Interface to implement | `IReminderScheduler` (Msg 2) |
| Technology | `System.Threading.PeriodicTimer` + injected `IClock` per §12 / Roadmap 1D |
| Output model | `ReminderScheduler.cs` + `CadencePolicy.cs` + tests |
| Chat strategy | Phase 1D per Roadmap §Sub-Phase 1D Chat Strategy — one msg scheduler + policy, one msg tests. |
| Verification | Roadmap P1D-AC1 through P1D-AC4. |
| Next agent | Claude Opus (chat) — **fresh chat session recommended** given current chat depth. See handoff consolidation below. |
| **Phase 1D Msg 1 HALT item #1** | Promote `FakeClock` to a shared `TestDoubles` project or namespace per `PHASE1C-DERIVATIONS.md` §Implementation note. ReminderScheduler tests will be the 4th consumer. |

---

## Gap Summary (unchanged from v1.0.13)

| Category | Count | Severity | Status |
|---|---|---|---|
| Knowledge Gaps | 11 | Variable | 10 resolved cumulatively (R1–R10). Q1–Q7 from v1.0.0 still tracked; Q8 (coverage tool → Phase 5C), Q9 (Perf.Tests → Phase 4B), **Q10 (LOAD-BEARING name list → Phase 5F)**, **Q11 (LOAD-BEARING support-email allowlist → Phase 5F)** carried forward. |

---

## Document History — new row

| Version | Date | Author | Changes |
|---|---|---|---|
| 1.0.14 | 2026-05-26 | Dominic Sarria-Wiley (via Claude Opus) | Phase 1C Msg 5 (`AuditChainWriterTests.cs`) complete. **Phase 1C is now ✅ COMPLETE: 3 production files (`ComplianceCore` + `PhiRedactor` + `AuditChainWriter`) + 13 unit tests across 3 test files + 8 derivations + 2 LOAD-BEARING open questions (Q10 name list, Q11 support-email allowlist) tracked for Phase 5F sign-off blockers.** Marker count `PHASE1C=6` (final). Next phase: Phase 1D `ReminderScheduler` will trigger `FakeClock` promotion at Msg 1 HALT. |

---

## 🎯 Phase 1C Completion Summary

**ComplianceCore module is fully delivered.**

### Production files (3)
- `src/TaskTree.Modules.ComplianceCore/ComplianceCore.cs` (orchestrator delegating to PhiRedactor + AuditChainWriter)
- `src/TaskTree.Modules.ComplianceCore/PhiRedactor.cs` (5 PHI regex patterns + synthetic name sentinels + allowlist support)
- `src/TaskTree.Modules.ComplianceCore/AuditChainWriter.cs` (SHA-256 hash chain + writer-assigned monotonic Seq + atomic gate)

### Test files (3, 13 tests total)
- `tests/TaskTree.Modules.ComplianceCore.Tests/ComplianceCoreTests.cs` (5 tests: AC1, AC2, AC5, anti-drift)
- `tests/TaskTree.Modules.ComplianceCore.Tests/PhiRedactorTests.cs` (6 tests: AC3, AC4, allowlist)
- `tests/TaskTree.Modules.ComplianceCore.Tests/AuditChainWriterTests.cs` (2 tests: sequence policy)

### Registry (8 derivations, 4 versions)
- `docs/spec-derivations/PHASE1C-DERIVATIONS.md` v1.0.4

### LOAD-BEARING open questions (2, both Phase 5F blockers)
- **Q10**: Source for §9.2.3 "common-name list" — Phase 1C ships synthetic test-only sentinel set with ZERO production coverage. Owner must provide real source list before Phase 5F sign-off.
- **Q11**: Support-email allowlist seed — empty default at Phase 1F unless owner populates. Owner must populate or explicitly accept empty default before Phase 5F sign-off.

### Cross-phase flags raised by Phase 1C
- **Phase 1D Msg 1**: promote `FakeClock` to shared `TestDoubles` (4th consumer trigger)
- **Phase 1F**: register `ComplianceCore` + `PhiRedactor` + `AuditChainWriter` as singletons; pass canonical `%LOCALAPPDATA%\TaskTree\store\` path to SecureStore ctor; populate Q11 allowlist OR explicitly accept empty
- **Phase 2B**: likely needs `ChainVerifyFailed` event on `IComplianceCore` via §Governance amendment (registry §5)
- **Phase 2F**: replace stubbed `StartIdleMonitor` + raise `AutoLogoffTriggered`; address master-key cache-clear contract (PHASE1B §6)
- **Phase 3D**: BugReporter `RedactionPipeline` consumes `PhiRedactor`; must respect allowlist for outgoing email destinations
- **Phase 4A**: `AuditChainStressTests` 10k/100k entries — validating gate for storage format (§3); if perf fails, chunking/append-only retrofit needed
- **Phase 5F**: close Q10 + Q11 before sign-off

---

**Note for handoff agent:** This is an additive delta over `HANDOFF.md` v1.0.13; all unchanged sections from v1.0.13 (and through it from earlier versions) remain authoritative. To produce a flat v1.0.14, merge these section overrides into v1.0.13 in section order.

A comprehensive flat-merge consolidation of HANDOFF.md v1.0.0 through v1.0.14 will be produced in the next message of this chat session to enable clean resumption in a fresh chat at Phase 1D.

---

## Current Verified State (additive v1.0.74)

This section records the current checkout without deleting the historical v1.0.14 delta above. It supersedes the historical Phase 1D next-action block for current execution; `Roadmap.md`, `Architecture.md`, and the owner gates remain authoritative.

| Area | Current state | Evidence |
|---|---|---|
| Phases 0-4 | Implemented in the current source tree; offline contracts and hardening lanes are green. | `TaskTree.sln`, `tests/`, `docs/test-gap-report.md` |
| Phase 5A | Repository stitches and namespaces resolve. | Release build, derivation registry |
| Phase 5B | Compile gap closure is green. | Release build: 0 warnings, 0 errors |
| Phase 5C | Local gate complete: 404 non-live, non-performance contract tests pass; 7 measurable performance tests pass; the last fully converted production coverage report is 1,651/2,137 lines (77.26%). | `docs/test-gap-report.md` |
| Phase 5D | Offline composition is green; the app resolves AutoUpdater and BugReporter with TaskTreePaths-backed roots, installs the crash hook, flushes queued bug reports at startup, owns a bounded updater poll lifecycle, and Orchestrator verifies the audit chain before subscriptions while auditing failure and continuing startup. TaskNode metadata survives storage snapshots, persisted settings and hotkeys serialize their state transitions, session-lock and compliance idle-monitor lifecycles are serialized, SessionLock and ReminderScheduler roll back partial starts, reminder delivery drains in-flight callbacks during stop, scheduler timer ownership is stable across stop, orchestrator lifecycle failure paths unwind safely, and the declared settings/tree UI surfaces are wired to the existing contracts. | `docs/integration-gap-report.md`, focused lifecycle and STA XAML tests |
| Phase 5E | Code paths are implemented; staged updater packages and first-launch sentinels promote through temporary files, app startup distinguishes the first launch attempt from a surviving crash marker and invokes the registered last-known-good rollback service on retry, unsafe version path components and incomplete or size-mismatched offline package metadata fail closed, updater apply rechecks channel/version/rollout eligibility, concurrent updater checks/applies and state transitions are synchronized, SMTP TLS is required, GitHub repository segments are constrained, provider calls are bounded, crash-hook registration is idempotent and active at app startup, the PHI email allowlist is exposed only as a snapshot, SecureStore key filenames are confined, the DPAPI-unwrapped master key is returned only through defensive copies, bug-report queue retention is encrypted and purges delivered/failed records after 7/30 days, and file-drop plus outbound limiter/router state is serialized. Live Windows/provider/package evidence remains open. | `docs/env-gap-report.md`, `src/TaskTree.Modules.AutoUpdater/`, `src/TaskTree.Modules.BugReporter/`, `src/TaskTree.Modules.SecureStore/` |
| Phase 5F | Not complete. Owner sign-off and release archive remain open. | `docs/final-validation-report.md` |

### Current carry-forward blockers

- Real WPF/tray/session/reminder E2E and CredUI validation require an interactive owner-controlled Windows profile.
- MSIX assets, WAP DesktopBridge targets, signing/install/rollback, and installed-binary launch evidence are unavailable in this host.
- Live SMTP/GitHub/updater provider checks require owner-controlled configuration and credentials.
- Q10 PHI common-name sources, Q11 support-email allowlist, production updater signing key, and Phase 5F owner sign-off remain unresolved.

### Additive handoff history

| Version | Date | Change |
|---|---|---|
| 1.0.56 | 2026-08-16 | Current checkout state, Phase 5C-5F evidence, and carry-forward external gates recorded; historical Phase 1C content preserved. |
| 1.0.57 | 2026-08-16 | Orchestrator startup unwind, shutdown aggregation, and duplicate-start lifecycle preservation recorded. |
| 1.0.58 | 2026-08-16 | Updater staged-package integrity and path-safety hardening recorded with current offline validation evidence. |
| 1.0.59 | 2026-08-16 | Bug-report queue transaction and concurrent flush serialization recorded with refreshed full-suite evidence. |
| 1.0.60 | 2026-08-16 | Bug-report file-drop atomic promotion and outbound limiter/router concurrency hardening recorded with refreshed validation evidence. |
| 1.0.61 | 2026-08-16 | Snooze/reminder lifecycle, logger rotation, and updater sentinel/import/state integrity hardening recorded with refreshed validation evidence. |
| 1.0.62 | 2026-08-16 | Updater operation serialization and compliance idle-monitor disposal hardening recorded with refreshed validation evidence. |
| 1.0.63 | 2026-08-16 | Hotkey, provider timeout, crash-hook, session lifecycle, and scheduler timer hardening recorded with refreshed validation evidence. |
| 1.0.64 | 2026-08-16 | Bug-report retention metadata, pending-only retries, 7/30-day purge behavior, and refreshed validation evidence recorded. |
| 1.0.65 | 2026-08-16 | TaskNode metadata preservation, unredacted queue rejection, SMTP TLS and GitHub segment validation, offline package-size verification, SecureStore key-path confinement, and refreshed validation evidence recorded. |
| 1.0.66 | 2026-08-16 | Application DI now resolves AutoUpdater and BugReporter with TaskTreePaths-backed roots, app startup installs crash capture, Orchestrator verifies the audit chain before subscriptions with fail-open failure auditing, and refreshed 392-test evidence is recorded. |
| 1.0.67 | 2026-08-16 | Updater apply eligibility, PHI allowlist encapsulation, SettingsService operation serialization, focused regressions, and refreshed 396/408-test evidence recorded. |
| 1.0.68 | 2026-08-16 | Master-key defensive copies, SessionLock startup rollback, ReminderScheduler startup cleanup, focused regressions, and refreshed 398/411-test evidence recorded. |
| 1.0.69 | 2026-08-16 | Updater apply now wires the atomic first-launch sentinel, app startup distinguishes first attempt from a surviving crash marker, DI registers rollback recovery, and focused 401/413-test evidence is refreshed. See `docs/HANDOFF-v1.0.69-delta.md`. |
| 1.0.70 | 2026-08-16 | Orchestrator now owns startup bug-report queue flushing and a bounded updater poll lifecycle; refreshed 402/415-test evidence recorded. See `docs/HANDOFF-v1.0.70-delta.md`. |
| 1.0.71 | 2026-08-16 | Unhandled-exception crash capture now commits the redacted report synchronously to the encrypted queue within a bounded 200 ms callback; focused BugReporter evidence refreshed. See `docs/HANDOFF-v1.0.71-delta.md`. |
| 1.0.72 | 2026-08-16 | Evidence-driven scheduler and SecureStore hot-path tuning restores the documented local performance thresholds; current measurements are recorded in `docs/perf-report.md`. See `docs/HANDOFF-v1.0.72-delta.md`. |
| 1.0.73 | 2026-08-16 | Background updater and scheduler tests now await their real synchronization points instead of fixed sleeps; refreshed full-suite evidence is green. See `docs/HANDOFF-v1.0.73-delta.md`. |
| 1.0.74 | 2026-08-16 | Phase 2B/2E UI surface closure adds the declared reusable SettingsView, hierarchical task tree with priority colors/deadlines, delete wiring, and STA XAML smoke coverage. See `docs/HANDOFF-v1.0.74-delta.md`. |
