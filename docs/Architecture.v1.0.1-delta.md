# Architecture.md v1.0.1 — Additive Delta

> **Read me first:** This is an **additive delta** over Architecture.md v1.0.0. Per §Governance ("append/reorder-only; content is never deleted"), this file documents the surgical edits that produce v1.0.1 from v1.0.0. Only the changed sections are reproduced below; every unchanged section from v1.0.0 remains authoritative.

---

## Reason for amendment

Roadmap.md sub-phases 1E, 2A reference `tests/TaskTree.Modules.TrayHost.Tests/` and sub-phases 2B, 2C, 2D, 2E reference `tests/TaskTree.UI.Tests/`. Neither was enumerated in §3.3 v1.0.0. To unblock those sub-phases and Phase 0 Msg 6, §3.3 is expanded to include the two omitted test projects.

## Owner approval

- **Approved by:** Dominic Sarria-Wiley
- **Approval date:** 2026-05-26
- **Approval message:** "Issue 1: B, and log all gaps thoroughly along with an updated Architecture.md. Issue 2: Approve all four minor items as proposed with thorough logging of gaps for later phases and handoff"

---

## Section 3.3 — Folder Structure (`tests/` subsection, updated)

The full `tests/` block of §3.3 in v1.0.1 (added projects flagged for reviewer reference; in v1.0.1 these are plain text):

```
├── tests/
│   ├── TaskTree.Core.Tests/
│   ├── TaskTree.Modules.TaskEngine.Tests/
│   ├── TaskTree.Modules.ReminderScheduler.Tests/
│   ├── TaskTree.Modules.SecureStore.Tests/
│   ├── TaskTree.Modules.ComplianceCore.Tests/
│   ├── TaskTree.Modules.AutoUpdater.Tests/
│   ├── TaskTree.Modules.BugReporter.Tests/
│   ├── TaskTree.Orchestrator.Tests/
│   ├── TaskTree.Modules.TrayHost.Tests/    ← added v1.0.1
│   └── TaskTree.UI.Tests/                   ← added v1.0.1
```

---

## Note on `TaskTree.Perf.Tests` (deferred)

Roadmap Phase 4B references `tests/TaskTree.Perf.Tests/PerfBenchmarks.cs`. **Intentionally NOT added to §3.3 in v1.0.1** — deferred to a future Architecture amendment at the start of Phase 4B per §Governance append-only rule. Tracked as Knowledge Gap **Q9** in `HANDOFF.md` v1.0.5 §Open Questions.

---

## §Document History — new row

| Version | Date | Author | Changes |
|---|---|---|---|
| 1.0.1 | 2026-05-26 | Dominic Sarria-Wiley | Expanded §3.3 `tests/` list to add `TaskTree.Modules.TrayHost.Tests` and `TaskTree.UI.Tests` (referenced by Roadmap 1E / 2A / 2B / 2C / 2D / 2E). `TaskTree.Perf.Tests` intentionally deferred to Phase 4B amendment. Front-matter Last Updated set to 2026-05-26. |

---

## Merge instructions

To produce a flat `Architecture.md` v1.0.1 from v1.0.0:

1. In §3.3, in the `tests/` subsection, append two lines after `TaskTree.Orchestrator.Tests/`:
   - `├── TaskTree.Modules.TrayHost.Tests/`
   - `└── TaskTree.UI.Tests/`

   Re-bracket the previous final `└── TaskTree.Orchestrator.Tests/` to `├──`.

2. Append the new row to §Document History.

3. Update the front-matter `Last Updated` to 2026-05-26 and `Document Version` to `1.0.1`.

---

**Note for handoff agent:** This is an additive delta over `Architecture.md` v1.0.0; all unchanged sections from v1.0.0 remain authoritative.
