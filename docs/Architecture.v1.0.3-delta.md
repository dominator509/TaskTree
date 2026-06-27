# Architecture.md - v1.0.3 Amendment Delta

**Predecessor:** v1.0.2 (2026-05-27 - added TaskTree.TestSupport + ReminderEvent.Reason)
**This version:** v1.0.3 (2026-05-27 - Phase 1F Msg 1)
**Author:** Dominic Sarria-Wiley (DSW)
**Type:** Append-only amendment per Roadmap §Governance

---

## Change 1 - §3.3 Abstractions list: add `IReminderDeliveryService`

ADD to `src/TaskTree.Core/Abstractions/`:

```
├── IReminderDeliveryService.cs    # NEW v1.0.3 - Phase 1F slot, Phase 1G consumer
```

**Surface:**
```csharp
public interface IReminderDeliveryService
{
    Task DeliverAsync(ReminderEvent reminder);
}
```

**Rationale (HALT #4 Option A):** Phase 1F placeholder delivery + Phase 1G Tier 1/2/3 router both need a stable contract. Defining the interface NOW lets Phase 1G slot in without refactoring Orchestrator wiring. Additive change; no API breaks.

---

## Change 2 - §3.3 Models list: add `TaskTreePaths`

ADD to `src/TaskTree.Core/Models/`:

```
├── TaskTreePaths.cs    # NEW v1.0.3 - canonical %LOCALAPPDATA%\TaskTree\* paths
```

**Surface:** 7 readonly path properties + 2 ctors + `EnsureDirectoriesExist()`.

**Rationale (HALT #7 Option B):** Centralizes canonical paths previously scattered across PHASE0-MSG5 §3 + PHASE1B §1, §3. Single point of injection for MasterKeyManager, SecureStore, FileAppLogger, AutoUpdater, BugReporter. Phase 5E integration tests override `RootDir`.

---

## Change 3 - Document History (§18) addition

| Version | Date | Author | Changes |
|---|---|---|---|
| 1.0.3 | 2026-05-27 | Dominic Sarria-Wiley | Added `IReminderDeliveryService` (§3.3 Abstractions) + `TaskTreePaths` (§3.3 Models). Both additive; no removals; no API breaks. |

---

## Out of scope for this amendment

- DI lifetime declarations - these live in `ServiceRegistrations.cs` and PHASE1F-DERIVATIONS.md §13 (forthcoming Msg 2), NOT in Architecture.md.
- `IOrchestrator` interface shape - Architecture is silent; PHASE1F-DERIVATIONS.md §5 records the assumed shape with G1F-H2 for Phase 5B reconciliation.

---

## End of Architecture.v1.0.3-delta.md
