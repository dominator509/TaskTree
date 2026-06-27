# Modules Core

- Module folders under `src/TaskTree.Modules.*` mirror architecture feature seams: TaskEngine, ReminderScheduler, SecureStore, ComplianceCore, TrayHost, AutoUpdater, BugReporter, Settings, Snooze, SessionLock.
- TaskEngine/ReminderScheduler/SecureStore/ComplianceCore are core MVP modules; TrayHost/UI/SessionLock/AutoUpdater/BugReporter touch live Windows/provider boundaries.
- Environment stubs must keep clear NotImplemented/gap reasons until live verification is approved and performed.
- AutoUpdater has signature/hash/staging/sentinel/rollback code; live package install and signing remain environment-sensitive.
- BugReporter must route only redacted payloads; SMTP/GitHub delivery and credentials are production-risk surfaces.
- Module tests are mirrored in `tests/TaskTree.Modules.*.Tests`; perf gates live in `tests/TaskTree.Perf.Tests`.