# PHASE2F-DERIVATIONS.md - SessionLock

> Scope: ISessionLockService + SessionLockService + Orchestrator/MainWindow privacy behavior + Tier2 suppression while locked.
> Owner-approved HALT batch: 22 items.

## Section 1 Summary

All 22 HALT items approved as proposed. SessionLockService is implemented with deferred live OS hook, internal test triggers, audit events, Orchestrator hide-on-lock behavior, and Tier 2 toast suppression while locked.

## Section 2 Files Produced

| Path | Purpose | Marker |
|---|---|---|
| src/TaskTree.Core/Abstractions/ISessionLockService.cs | NEW abstraction + event args | PHASE2F |
| src/TaskTree.Modules.SessionLock/TaskTree.Modules.SessionLock.csproj | NEW module | csproj |
| src/TaskTree.Modules.SessionLock/SessionLockService.cs | NEW service | PHASE2F |
| src/TaskTree.Modules.SessionLock/Properties/AssemblyInfo.cs | InternalsVisibleTo | PHASE2F |
| src/TaskTree.Orchestrator/ToastTier2Adapter.cs | PATCH lock suppression | PHASE1G/2B/2F |
| src/TaskTree.Orchestrator/Orchestrator.cs | PATCH session lock wiring | PHASE1F/1G/2B/2E/2F |
| src/TaskTree.App/Bootstrap/ServiceRegistrations.cs | PATCH registrations | PHASE1F/1G/2A/2E/2F |
| tests/TaskTree.Modules.SessionLock.Tests/TaskTree.Modules.SessionLock.Tests.csproj | NEW test project | csproj |
| tests/TaskTree.Modules.SessionLock.Tests/SessionLockServiceTests.cs | NEW 12 tests | PHASE2F |
| tests/TaskTree.Orchestrator.Tests/ToastTier2AdapterTests.cs | NEW/patch headless-safe test | PHASE2F |
| docs/spec-derivations/PHASE2F-DERIVATIONS.md | This registry | md |
| docs/HANDOFF-v1.0.34-delta.md | Phase 2F delta | md |
| tools/find-spec-derivations.ps1 | PHASE2F=8 | script |

## Section 3 Marker Inventory

PHASE2F = 8 distinct .cs files. Grand total: 107 -> 115.

## Section 4 Cross-Phase Gaps Introduced (164-179)

| # | Gap | Target | Action |
|---|---|---|---|
| 164 | Add ISessionLockService Architecture Section 4 subsection | Architecture v1.0.3 | Document interface/lifecycle |
| 165 | Move event args to Core.Models if session state grows | Future refactor | Reassess if more detail needed |
| 166 | Add src/TaskTree.Modules.SessionLock/ to Architecture Section 3.3 | Architecture v1.0.3/Phase 5F | Amend folder tree |
| 167 | Add formal SessionState enum if richer states needed | Future HALT | Only bool IsLocked now |
| 168 | SessionLockService audit injection consumer registration | Closed by Msg 1 | ServiceRegistrations patch |
| 169 | Real Windows session lock/unlock hook deferred | Phase 5E | Wire SystemEvents.SessionSwitch or equivalent |
| 170 | Document SessionLock internal-raise test pattern | Architecture v1.0.3 | Add prose |
| 171 | Compliance policy documents SessionLockStarted/Stopped | Phase 4A | Audit vocabulary |
| 172 | Compliance policy documents SessionLocked/Unlocked | Phase 4A | Audit vocabulary |
| 173 | Orchestrator constructor tests update for ISessionLockService | Phase 5B | Patch mocks/builders |
| 174 | Shutdown order with SessionLockService verified | Phase 5C | Integration/unit backfill |
| 175 | WPF integration: MainWindow hides on lock/no auto-show | Phase 5C | Real WPF test |
| 176 | ToastTier2Adapter constructor changes update tests/factories | Phase 5B | Patch factories/tests |
| 177 | Full reminder suppression policy while locked | Phase 4A/5E | Decide policy for all tiers |
| 178 | WPF integration: visible ReminderToast hides on lock | Phase 5C | Real WPF test |
| 179 | Orchestrator test backfill includes SessionLock start/stop + lock handling | Phase 5C | Extend Gap #95 backfill |

## Section 5 Existing Gaps Carried

#141, #142, #158, #160, #161, #162, #163 remain active. Constructor churn from #160/#161 is compounded by #173/#176.

## Section 6 Known Limitations

1. Live OS lock/unlock hook is deferred; StartAsync logs a HIGH warning but does not subscribe to Windows events.
2. MainWindow hide-on-lock behavior requires Phase 5C real WPF verification.
3. Tier 2 visible-toast hide-on-lock requires Phase 5C real WPF verification.
4. Full all-tier reminder suppression policy is not defined yet.
5. Architecture amendments required for new module/interface/internal-raise pattern.
