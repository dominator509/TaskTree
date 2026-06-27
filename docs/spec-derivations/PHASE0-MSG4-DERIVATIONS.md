# Phase 0 — Msg 4 Spec-Derived Enums

**Status:** approved by Dominic Sarria-Wiley on 2026-05-26
**Grep marker:** `SPEC-DERIVED-MSG4`
**Total derived enums:** 2 of 5
**Companion docs:** Architecture.md v1.0.0, Roadmap.md v1.0.0, HANDOFF.md v1.0.3, `PHASE0-MSG2-DERIVATIONS.md`, `PHASE0-MSG3-DERIVATIONS.md`

---

## Why this file exists

Architecture.md is the single source of truth for enum enumerations. For two
of the five Phase-0 enums (`TaskStatus`, `ReminderCadence`), Architecture.md
references the *type* (or its location in §3.3) and describes its *use* but
does not enumerate values. Per Roadmap D8 (HALT protocol), the agent stopped,
asked, and received owner approval before generating enumerations. This file
records each derivation so any future agent (Codex, Claude Code, human
reviewer) can locate, audit, and re-validate every approved-but-not-spec'd
enum.

## How to use this file at handoff

1. `grep -r "SPEC-DERIVED-MSG4" src/` — locates every enum file with a derived enumeration.
2. `grep -r "SPEC-DERIVED-MSG4" docs/` — locates this registry.
3. Cross-check entries below against Phase 1A (TaskEngine using `TaskStatus`),
   Phase 1D (ReminderScheduler firing `ReminderCadence`),
   Phase 2G (SnoozeService + EscalationPolicy referencing both).
4. Promotion path: amend Architecture.md, bump to v1.0.x, record a
   Superseded-by row.

---

## Derivation Registry

### 1. TaskStatus

| Field | Value |
| --- | --- |
| File | `src/TaskTree.Core/Enums/TaskStatus.cs` |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "Approve both enums as-is and record any and all gaps thoroughly for later handoffs" |
| Source-of-truth section(s) | §1.1 ("until tasks are completed or dismissed"), §2 F2 (add/edit/delete tasks), §13 step 10 ("user marks complete"), Roadmap P1A-AC2 (`UpdateAsync raises TaskCompleted when status = Done`), Roadmap 1A AC (`GetOverdueAsync returns past-deadline non-Done nodes`) |
| Spec gap | Architecture.md references `TaskStatus` enum location (§3.3) and uses the literal value `Done` in Roadmap but never enumerates all statuses. |
| Derivation rationale | • Binary `Active`/`Done` covers every documented behavior. The phrase "completed or dismissed" in §1.1 maps to `Done` (completed) + `DeleteAsync` (dismissed-as-deletion per §4.2).<br>• `Done=1` chosen so the default-init value (0) is `Active` — newly constructed nodes are correctly "in flight" without explicit initialization.<br>• No `Snoozed` status: §2G `SnoozeService` is a scheduler-side concern (defers `ReminderDue` firings); the task itself remains `Active` during snooze. |
| Approved surface | `Active = 0`, `Done = 1` |
| Downstream consumers | TaskEngine (Phase 1A), ReminderScheduler (Phase 1D — skip `Done` nodes when computing overdue), TreeViewUI (Phase 2B — visual strikethrough binding), SnoozeService (Phase 2G — only `Active` tasks can be snoozed) |
| Re-validation tasks for handoff agent | - Confirm Phase 1A `UpdateAsync` raises `TaskCompleted` ONLY when transitioning `Active→Done` (not on `Done→Done` idempotent updates).<br>- Confirm `GetOverdueAsync` filters on `Status != Done` (not just `== Active`) so future status additions don't silently flip overdue inclusion.<br>- If a `Cancelled` / `InProgress` / `Snoozed` status becomes required by UI or workflow, add via §Governance amendment — do NOT silently add.<br>- Confirm `System.Text.Json` round-trip preserves int value (0/1), not member name (default `System.Text.Json` serializes enums as numbers — confirm or pin via `JsonStringEnumConverter` at composition root). |
| Risk if surface is wrong | Overdue detection skips legitimately overdue tasks; `TaskCompleted` event missed; UI fails to gray out completed tasks. |
| Promotion path | Amend §4.2 or §3.3 per §Governance; bump to v1.0.x; add Superseded-by row. |

### 2. ReminderCadence

| Field | Value |
| --- | --- |
| File | `src/TaskTree.Core/Enums/ReminderCadence.cs` |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "Approve both enums as-is and record any and all gaps thoroughly for later handoffs" |
| Source-of-truth section(s) | §5.3 (cadence table), §4.3 (`IReminderScheduler.ReminderDue` raises `ReminderEvent` which carries this), §2G (EscalationPolicy thresholds), Roadmap P1D ("Cadence values match §5.3 exactly") |
| Spec gap | §5.3 binds cadence to priority bands but never declares the enum's member names. |
| Derivation rationale | • 1:1 mirror of `Priority` (`Critical`/`High`/`Normal`/`Low`/`Trivial` with values 1..5) reflects §5.3's direct binding of cadence-to-priority.<br>• Distinct type from `Priority` (not a type alias): `Priority` describes a task attribute; `ReminderCadence` describes a scheduler output on `ReminderEvent`. Conflating them would force the UI to inspect `Priority` to know which band fired (it already does via `ReminderEvent.Node.Priority` — but the explicit `Cadence` field makes audit/log entries self-describing per §10.5).<br>• Explicit ints (1..5) chosen for parity with `Priority` — makes `ReminderCadence` ↔ `Priority` casts unambiguous if a Phase-1D adapter performs them. |
| Approved surface | `Critical=1, High=2, Normal=3, Low=4, Trivial=5` |
| Downstream consumers | ReminderScheduler (Phase 1D), `ReminderEvent.Cadence` (already shipped Msg 3), ReminderDeliveryService (Phase 1G — uses cadence to choose Tier 1/2/3 escalation), SnoozeService + EscalationPolicy (Phase 2G), Tier 2 Toast XAML binding (Phase 2B) |
| Re-validation tasks for handoff agent | - Confirm Phase 1D `CadencePolicy` maps `Priority` → `ReminderCadence` as identity (`Priority.Critical` → `ReminderCadence.Critical`). If a non-identity mapping is ever introduced (e.g., compliance-mode demotes), document HERE before changing.<br>- Confirm Phase 2G `EscalationPolicy` thresholds (§5.3) read both `ReminderCadence` band AND a wall-clock overdue duration — not just band alone.<br>- If a `Snoozed` cadence band is added later for §2G visual distinction, add via §Governance amendment AND extend `ReminderEvent.Reason` allowed values in `PHASE0-MSG3-DERIVATIONS.md` §2.<br>- Confirm `System.Text.Json` round-trip: same `JsonStringEnumConverter` decision applies as for `Priority`/`TaskStatus` — pick once at composition root (Phase 1F) and apply globally. |
| Risk if surface is wrong | Wrong band fires (P1 task gets P5 cadence → critical reminders delayed 8 hours); escalation logic mis-triggers; audit log loses cadence context. |
| Promotion path | Amend §5.3 or §3.3 per §Governance. |

---

## Cross-reference table

| Enum | Path | Derived elements | Phases that consume it |
| --- | --- | --- | --- |
| `TaskStatus` | `src/TaskTree.Core/Enums/TaskStatus.cs` | full enumeration | Phase 1A, 1D, 2B, 2G |
| `ReminderCadence` | `src/TaskTree.Core/Enums/ReminderCadence.cs` | value labels | Phase 1D, 1G, 2G, 2B |
| `Priority` | `src/TaskTree.Core/Enums/Priority.cs` | (none — verbatim §5.3) | Phase 1A, 1D, 2B, 2D |
| `UpdateChannel` | `src/TaskTree.Core/Enums/UpdateChannel.cs` | (none — verbatim §9.1/§9.1.2) | Phase 3A, 3B, 3C |
| `BugSeverity` | `src/TaskTree.Core/Enums/BugSeverity.cs` | (none — verbatim §9.2.4) | Phase 3D, 3E |

---

## Cross-derivation interaction notes

- **`ReminderCadence` ↔ `Priority` value parity is intentional and must be preserved.** If §5.3 ever changes the priority count or banding, BOTH enums must be updated in a single Architecture.md amendment.
- **`TaskStatus` ↔ §2G SnoozeService interaction:** snoozed tasks remain `Active`. The "snoozed" state lives on the scheduler, not on the task. Phase 2G implementation must respect this — do not introduce a `Snoozed` task status without §Governance amendment.
- **`UpdateChannel` string ↔ enum boundary:** §9.1.2 JSON channel = string (`"stable"`/`"beta"`). `UpdateManifest.Channel` (Msg 3) = `string`. `IAutoUpdater.Channel` (Msg 2, §4.7) = `UpdateChannel` enum. Conversion happens at the AutoUpdater boundary in Phase 3A. Verify the boundary handles unknown channel values gracefully (reject manifest, do not crash).

---

## Promotion / change procedure

If any of these 2 enumerations needs to change after handoff:

1. Open a §Governance amendment against Architecture.md (bump to v1.0.x).
2. Add the formal enum enumeration to the appropriate §4.x or §3.3 section.
3. Update this file with a "Superseded by Architecture.md v1.0.x §y.z on
   YYYY-MM-DD" row.
4. Update HANDOFF.md §Decisions Log + §Gap Summary.
5. Re-run the grep helper script (`tools/find-spec-derivations.ps1`) to
   confirm no orphan `SPEC-DERIVED-MSG4` markers remain.

---

## Document history

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 1.0.0 | 2026-05-26 | Dominic Sarria-Wiley (via Claude Opus) | Initial registry — 2 enums derived during Phase 0 Msg 4. |
