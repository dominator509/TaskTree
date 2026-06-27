# Architecture.md v1.0.2 - Amendment Proposal (UPDATED - 7 Bundled Changes)

> **Status:** NOT YET APPLIED - owner approval required.
> **Triggered by:** Phase 1D Msg 1 HALT #5 + #3 + Phase 1E Msg 1 HALT #15 + Phase 1F Msg 1 HALT #1+#16 + Phase 1G Msg 1 HALT #9 + Phase 1G Msg 2 HALT #1+#18 + Phase 2A Msg 1 HALT #8.
> **Updated by:** Phase 2A Msg 2 per Cross-Phase Gap #105 closure.
> **Author:** DSW - **Date:** 2026-05-29
> **Scope:** 7 bundled additive non-breaking changes.

## Section 1 Change Request Summary

| # | Section | Change | Status | Source |
|---|---|---|---|---|
| 1 | Section 3.3 Enums | Add ReminderReason.cs (6th enum) | REQUIRED | Phase 1D Msg 1 HALT #5 |
| 2 | Section 4.3 ReminderScheduler | Formalize Cadence [1s, 5min] clamp prose | OPTIONAL | Phase 1D Msg 1 HALT #3 |
| 3 | Section 4.1 TrayHost | Bless internal-raise pattern via InternalsVisibleTo | REQUIRED IF PHASE 1E SHIPS | Phase 1E Msg 1 HALT #15 |
| 4 | Section 4.7 NEW IOrchestrator | Formalize 2-method surface + 4 invariants | REQUIRED IF PHASE 1F SHIPS | Phase 1F Msg 1 HALT #1+#16 |
| 5 | Section 3.3 Enums | Add ReminderDeliveryTier.cs (7th enum) | REQUIRED IF PHASE 1G SHIPS | Phase 1G Msg 1 HALT #9 |
| 6 | Section 4.9 NEW IReminderDeliveryService | Formalize 2-method + 4 invariants prose | REQUIRED IF PHASE 1G SHIPS | Phase 1G Msg 2 HALT #1+#18 |
| 7 | Section 3.3 tests folder | Add tests/TaskTree.TestSupport/ (10th test dir) | REQUIRED IF PHASE 2A SHIPS | Phase 2A Msg 1 HALT #8 |

## Section 2 Justification

### Change 1 (REQUIRED)
Roadmap P1D-AC3 mandates ReminderDue events include reason. Typed enum satisfies D8 no-string-magic. Without amendment ReminderReason.cs exists without Section 3.3 acknowledgment - Phase 5F-blocking drift.

### Change 2 (OPTIONAL)
HALT #3 resolved Cadence as tick interval default 30s clamp [1s, 5min]. Formalizing in Section 4.3 prose eliminates future drift.

### Change 3 (REQUIRED IF PHASE 1E SHIPS)
Phase 1E HALT #5 introduced internal Raise methods via InternalsVisibleTo. Architecture has no general guidance on this testing pattern - Phase 5F flags as undocumented.

### Change 4 (REQUIRED IF PHASE 1F SHIPS)
Phase 1F HALT #1 finalized IOrchestrator surface as StartAsync + StopAsync. 4 invariants are LOAD-BEARING for Phase 1F E2E correctness.

### Change 5 (REQUIRED IF PHASE 1G SHIPS)
Phase 1G HALT #9 introduced ReminderDeliveryTier enum at Core/Enums. Without Section 3.3 amendment enum exists without acknowledgment.

### Change 6 (REQUIRED IF PHASE 1G SHIPS)
Phase 1G Msg 2 HALT #1 promoted ReminderDeliveryService to implement IReminderDeliveryService interface. Architecture has no formal Section 4 subsection.

### Change 7 (REQUIRED IF PHASE 2A SHIPS) - NEW
Phase 2A Msg 1 promoted FakeClock + InMemorySecureStore to new tests/TaskTree.TestSupport/ project per Gap #97/#98. Architecture Section 3.3 currently lists 9 test directories but does not include the new project. Without amendment Phase 5F sign-off flags this as undocumented test infrastructure pattern. Gap #105 closure.

## Section 3 Diff Snippets

### Change 1 + Change 5 - Section 3.3 Enums folder

**Before (v1.0.1):**
```
Enums/
  Priority.cs
  TaskStatus.cs
  ReminderCadence.cs
  UpdateChannel.cs
  BugSeverity.cs
```

**After (v1.0.2):**
```
Enums/
  Priority.cs
  TaskStatus.cs
  ReminderCadence.cs
  UpdateChannel.cs
  BugSeverity.cs
  ReminderReason.cs        # added v1.0.2 Change 1
  ReminderDeliveryTier.cs  # added v1.0.2 Change 5
```

### Change 2 - Section 4.3 ReminderScheduler prose append (OPTIONAL)

> **Cadence semantic:** The Cadence property is the tick (poll) interval. Default = 30s. Allowed range = [1s, 5min] inclusive; setter throws ArgumentOutOfRangeException on out-of-range values. Per-priority repeat cadences are Section 5.3-driven and live in CadencePolicy - not Cadence.

### Change 3 - Section 4.1 TrayHost prose append

> **Internal-raise testing pattern:** The TrayHost module MAY expose internal Raise*() methods accessible only via [InternalsVisibleTo("TaskTree.Modules.TrayHost.Tests")] to its Tests assembly for P1E-AC2 satisfaction. No production caller may use these methods. This pattern is also used by ReminderScheduler (Section 4.3) and Orchestrator (Section 4.7) modules.

### Change 4 - Section 4.7 IOrchestrator subsection (NEW)

> **Section 4.7 IOrchestrator** - coordinates lifecycle and event wiring across all TaskTree modules per Section 3.2 dependency graph.
>
> Interface declares: `Task StartAsync(CancellationToken ct)` and `Task StopAsync()`.
>
> Concrete implementations MUST:
> 1. **(Chain integrity)** Verify audit chain integrity at startup per Section 10.7, logging+auditing on failure but not aborting.
> 2. **(Subscribe-before-Initialize)** Subscribe to TrayHost + ReminderScheduler events BEFORE invoking TrayHost.Initialize.
> 3. **(Lifecycle-audit-only)** Emit audit entries ONLY for Startup, Shutdown, ChainVerifyFailedAtStartup - never per-event.
> 4. **(Reverse-order shutdown)** Unsubscribe handlers - stop ReminderDeliveryService - stop ReminderScheduler - dispose TrayHost - audit Shutdown.

### Change 6 - Section 4.9 IReminderDeliveryService subsection (NEW)

> **Section 4.9 IReminderDeliveryService** - Routes ReminderDue events from IReminderScheduler through Section 7 Tier 1/2/3 fallback chain.
>
> Interface declares: `Task StartAsync(CancellationToken ct)` and `Task StopAsync()`.
>
> Concrete implementations MUST:
> 1. Subscribe to IReminderScheduler.ReminderDue in StartAsync.
> 2. Unsubscribe in StopAsync.
> 3. Implement Section 7 Tier 1 -> Tier 2 -> Tier 3 cascade via per-adapter `bool TryDeliver(ReminderEvent)`; catch NotImplementedException narrowly (Phase 5E removes catches when live tiers wired).
> 4. Emit audit Module="ReminderDelivery" Action="DeliveredViaTier1|2|3" on success, "DeliveryFailedAllTiers" on failure. Always TargetId=evt.TaskId, Timestamp=clock.UtcNow.

### Change 7 - Section 3.3 tests folder (NEW)

**Before (v1.0.1):**
```
tests/
  TaskTree.Core.Tests/
  TaskTree.Modules.TaskEngine.Tests/
  TaskTree.Modules.SecureStore.Tests/
  TaskTree.Modules.ComplianceCore.Tests/
  TaskTree.Modules.ReminderScheduler.Tests/
  TaskTree.Modules.TrayHost.Tests/
  TaskTree.Orchestrator.Tests/
  TaskTree.UI.Tests/
  TaskTree.AutoUpdater.Tests/
```

**After (v1.0.2):**
```
tests/
  TaskTree.Core.Tests/
  TaskTree.Modules.TaskEngine.Tests/
  TaskTree.Modules.SecureStore.Tests/
  TaskTree.Modules.ComplianceCore.Tests/
  TaskTree.Modules.ReminderScheduler.Tests/
  TaskTree.Modules.TrayHost.Tests/
  TaskTree.Orchestrator.Tests/
  TaskTree.UI.Tests/
  TaskTree.AutoUpdater.Tests/
  TaskTree.TestSupport/        # added v1.0.2 (Change 7 - shared test helpers FakeClock + InMemorySecureStore)
```

## Section 4 Version History Append

| Version | Date | Author | Changes |
|---|---|---|---|
| 1.0.2 | TBD | Dominic Sarria-Wiley | Section 3.3 Enums folder gains ReminderReason.cs + ReminderDeliveryTier.cs per Phase 1D/1G Msg 1. OPTIONAL Section 4.3 ReminderScheduler formalizes Cadence clamp per HALT #3. Section 4.1 TrayHost blesses internal-raise pattern per Phase 1E HALT #15. NEW Section 4.7 IOrchestrator subsection (4 invariants) per Phase 1F. NEW Section 4.9 IReminderDeliveryService subsection (4 invariants) per Phase 1G Msg 2. Section 3.3 tests folder adds TaskTree.TestSupport/ shared test helpers project per Phase 2A Msg 1 HALT #8. No module API surface impact; purely additive across all 7 changes. |

## Section 5 Owner Sign-Off Block

```
Approval - Change 1 - Section 3.3 ReminderReason.cs (REQUIRED for Phase 5F):
[ ] Approved as written
[ ] Approved with modifications: _________________________________
[ ] Rejected - alternative: ____________________________________

Approval - Change 2 - Section 4.3 Cadence clamp prose (OPTIONAL):
[ ] Approve and include in v1.0.2
[ ] Defer to v1.0.3
[ ] Reject - alternative: _____________________________________

Approval - Change 3 - Section 4.1 TrayHost internal-raise blessing (REQUIRED IF PHASE 1E SHIPS):
[ ] Approved as written
[ ] Approved with modifications: _________________________________
[ ] Rejected - alternative: ____________________________________

Approval - Change 4 - Section 4.7 IOrchestrator subsection (REQUIRED IF PHASE 1F SHIPS):
[ ] Approved as written
[ ] Approved with modifications: _________________________________
[ ] Rejected - alternative: ____________________________________

Approval - Change 5 - Section 3.3 ReminderDeliveryTier.cs (REQUIRED IF PHASE 1G SHIPS):
[ ] Approved as written
[ ] Approved with modifications: _________________________________
[ ] Rejected - alternative: ____________________________________

Approval - Change 6 - Section 4.9 IReminderDeliveryService subsection (REQUIRED IF PHASE 1G SHIPS):
[ ] Approved as written
[ ] Approved with modifications: _________________________________
[ ] Rejected - alternative: ____________________________________

Approval - Change 7 - Section 3.3 tests/TaskTree.TestSupport/ project (REQUIRED IF PHASE 2A SHIPS):
[ ] Approved as written
[ ] Approved with modifications: _________________________________
[ ] Rejected - alternative: ____________________________________

Signed: ____________________________      Date: __________________
        Dominic Sarria-Wiley
```

Upon approval: bump Architecture.md Document Version to 1.0.2; apply per Architecture-v1.0.2-promotion-manifest.md; append Section 18 Version History row.

Phase 5F sign-off gate enforces: ALL REQUIRED changes (1, 3, 4, 5, 6, 7) approved; OPTIONAL Change 2 at owner discretion (currently implicit-deferred to v1.0.3 per Gap #101).
