# Core Library

- `src/TaskTree.Core` contains the stable interface/model/enum contract surface; most modules reference it and should not redefine contracts locally.
- Key abstractions include task engine, secure store, compliance core, reminder scheduler/delivery, tray host, orchestrator, settings, snooze, session lock, updater, bug reporter, clock, crypto, logger.
- Security primitives live under `Security/` and `Logging/`; changes affect compliance and many tests.
- Models/enums encode architecture-visible names; renames are drift defects unless owner-approved architecture changes also happen.
- Test support is separate under `tests/TaskTree.TestSupport`; prefer shared fakes there over duplicating clocks/stores in module tests.