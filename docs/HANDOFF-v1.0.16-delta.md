# HANDOFF.md v1.0.16 — Additive Delta

> **Applied on top of v1.0.15 (consolidated flat merge).**
> This file is an additive delta only — to produce a consolidated v1.0.16,
> rebase these sections onto v1.0.15.
> **Author:** DSW · **Date:** 2026-05-28 · **Trigger:** Phase 1D Msg 1 emitted.

---

## §A — Document Identity Update

| Field | Old (v1.0.15) | New (v1.0.16) |
|---|---|---|
| Document Version | 1.0.15 | **1.0.16** |
| Session ID | 2026-05-26-16 | 2026-05-28-01 |
| State | Phase 1C ✅; 1D awaiting Msg 1 HALT | **Phase 1D Msg 1 emitted; Msg 2 (tests) pending** |
| Current Sub-Phase | 1D — awaiting HALT | **1D — Msg 1 ✅, Msg 2 pending** |
| Total spec-derived items | 38 across 11 registries | **42 across 12 registries** |
| LOAD-BEARING open questions | Q10, Q11 | Q10, Q11 (unchanged) |
| Progress | 23 / 35 → 65.7% | **24 / 35 → 68.6%** |

---

## §B — Files Produced Delta

### Phase 1D Msg 1 (new — all ✅)

| Phase.Item | FilePath | Status | Notes |
|---|---|---|---|
| P1.D.Msg1 | `src/TaskTree.Modules.ReminderScheduler/ReminderScheduler.cs` | ✅ | SPEC-DERIVED-PHASE1D (HALT #2/#3/#6/#9/#10/#11/#13) |
| P1.D.Msg1 | `src/TaskTree.Modules.ReminderScheduler/CadencePolicy.cs` | ✅ | SPEC-DERIVED-PHASE1D (HALT #4/#5/#7/#8/#9/#12/#13) |
| P1.D.Msg1 | `src/TaskTree.Core/Enums/ReminderReason.cs` | ✅ NEW | SPEC-DERIVED-PHASE1D (HALT #5). **Requires Architecture v1.0.2 amendment.** |
| P1.D.Msg1 | `tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs` | ✅ NEW | SPEC-DERIVED-PHASE1D (HALT #1 promotion) |

### Patches required (deferred execution — Codex/Claude Code at Phase 5A)

| Phase.Item | FilePath | Status | Notes |
|---|---|---|---|
| P1.D.Msg1 | `tests/TaskTree.Modules.TaskEngine.Tests/TaskEngineTests.cs` | 🩹 PATCH | Remove inline `FakeClock`, add `using TaskTree.Core.Tests.TestDoubles;`, add ProjectReference |
| P1.D.Msg1 | `tests/TaskTree.Modules.ComplianceCore.Tests/ComplianceCoreTests.cs` | 🩹 PATCH | Same |
| P1.D.Msg1 | `tests/TaskTree.Modules.ComplianceCore.Tests/AuditChainWriterTests.cs` | 🩹 PATCH | Same |

Patch manifest: `PATCHES-PHASE1D-MSG1.md` (root of Msg 1 bundle).

### Phase 1D Msg 2 — pending

| Phase.Item | FilePath | Status |
|---|---|---|
| P1.D.Msg2 | `tests/TaskTree.Modules.ReminderScheduler.Tests/ReminderSchedulerTests.cs` | ⬚ Pending |
| P1.D.Msg2 | `tests/TaskTree.Modules.ReminderScheduler.Tests/CadencePolicyTests.cs` | ⬚ Pending |

---

## §C — Decisions Log Delta

### R11 — Phase 1D Msg 1 — 13 HALT items resolved (batch-approved)

1. **#1 FakeClock promotion** → Option A: `tests/TaskTree.Core.Tests/TestDoubles/`.
2. **#2 ReminderScheduler ctor** → `(IClock, ITaskEngine, IComplianceCore, IAppLogger)` — **LOAD-BEARING audit injection** (parallels TaskEngine R6).
3. **#3 `Cadence` semantic** → tick (poll) interval; default 30 s; clamp `[1 s, 5 min]`.
4. **#4 `CadencePolicy` surface** → hybrid static-utility + `ShouldFire` decision; no `GetEscalationAfter` yet.
5. **#5 `ReminderReason` enum** → 3 values (`Initial`, `Repeat`, `Overdue`); placed at `Core/Enums/`. **Requires Architecture v1.0.2 amendment.**
6. **#6 Last-fired state** → in-memory `Dictionary<Guid,DateTimeOffset>` for 1D; promote to SecureStore `"reminders/state"` at Phase 2G.
7. **#7 P=1 fire-on-creation** → next-tick detection (no TaskAdded subscription). 30 s worst-case latency accepted.
8. **#8 Due/overdue evaluation** → single `GetTreeAsync()` per tick; inline `CadencePolicy.ShouldFire` derivation (no `GetOverdueAsync` call).
9. **#9 Multi-fire per tick** → at-most-one event per task per tick; precedence Overdue > Repeat > Initial.
10. **#10 Lifecycle semantics** → Start throws if running; Stop no-op if stopped; restart supported; `IDisposable` implemented.
11. **#11 StopAsync wait** → bounded 5 s wait via `Task.WhenAny`; log warning on timeout (wait outside `_lifecycleGate` to avoid deadlock).
12. **#12 Null deadline** → Critical fires (initial + repeat); High/Normal/Low/Trivial silently skip.
13. **#13 Escalation column** → fully deferred to Phase 2G. No audible chime, no persistent toast, no badge logic.

---

## §D — Open Questions Delta

- **Q10 (Phase 5F blocker)** — Real common-name source list. **UNCHANGED. STILL ACTIVE.**
- **Q11 (Phase 1F or 5F blocker)** — Support-email allowlist seed. **UNCHANGED. STILL ACTIVE.**
- **No new LOAD-BEARING open questions raised by Phase 1D Msg 1.**

Item #2 (IComplianceCore injection) is a LOAD-BEARING **flag** for Phase 1F, not a Q-numbered owner-decision blocker; recorded as Cross-Phase Gap #32 / #45.

Item #5 (Architecture v1.0.2 amendment) is documented as a *non-blocking* amendment proposal (see `docs/Architecture.v1.0.2-delta.md`) — owner approval needed before Phase 5F sign-off, but does not block Phase 1D Msg 2 or 1E.

---

## §E — Visual Phase Tracker Delta

```
PHASE 1 — CORE MVP                                    [In Progress]
├── 1A ✅ TaskEngine + tests
├── 1B ✅ SecureStore + MasterKeyManager + IMasterKeyManager
├── 1C ✅ ComplianceCore baseline
├── 1D 🔨 ReminderScheduler + CadencePolicy            (Msg 1 ✅ — 4 .cs; Msg 2 tests pending)
│         ├── ReminderScheduler.cs ✅
│         ├── CadencePolicy.cs ✅
│         ├── ReminderReason.cs ✅ (Architecture v1.0.2 amendment proposed)
│         └── FakeClock.cs ✅ (Option A promotion — 4th consumer)
├── 1E ⬚ TrayHost (HIGH-stub for Codex 5E)
├── 1F ⬚ Orchestrator wiring                          (CRITICAL: TWO audit-injection LOAD-BEARING flags)
├── 1G ⬚ Reminder Tier 1/2/3
└── 1H ⬚ Phase 1 E2E gate                              [GATE]
```

**Progress: 24 / 35 → 68.6%** (Msg 1 deliverables counted; Msg 2 tests pending to fully clear 1D).

---

## §F — Cross-Phase Gap Table — New Rows (32-45)

Appended to the v1.0.15 31-row table.

| # | Flag | Source | Target Phase | Action |
|---|---|---|---|---|
| 32 | Inject `IComplianceCore` into `ReminderScheduler` ctor | PHASE1D §3 / HALT #2 | Phase 1F | **LOAD-BEARING** — else audit chain silently breaks for reminders |
| 33 | Add `ReminderReason.cs` to Architecture §3.3 Enums | PHASE1D §6 / HALT #5 | Architecture v1.0.2 | Apply amendment per `docs/Architecture.v1.0.2-delta.md` |
| 34 | Promote `_lastFiredUtc` → SecureStore `"reminders/state"` | PHASE1D §7 / HALT #6 | Phase 2G | Co-promote with SnoozeService persistence |
| 35 | P=1 fast-path TaskAdded subscription (optional) | PHASE1D §8 / HALT #7 | Phase 2G | Owner decision contingent on user feedback |
| 36 | UI: "No deadline ⇒ no reminder unless Critical" | PHASE1D §13 / HALT #12 | Phase 2B | Tooltip on `MainWindowViewModel` |
| 37 | Implement `EscalationPolicy` + `ReminderReason.Escalation = 3` | PHASE1D §14 / HALT #13 | Phase 2G | Additive — non-breaking enum extension |
| 38 | Orchestrator shutdown SLA must respect 5 s StopAsync bound | PHASE1D §12 / HALT #11 | Phase 1F | Cancel before StopAsync if SLA < 5 s |
| 39 | Verify `Dispose()` sync-over-async under live DI teardown | PHASE1D §11 / HALT #10 | Phase 5D | Switch to `IAsyncDisposable` if deadlock |
| 40 | 5th FakeClock consumer triggers Option B re-evaluation | PHASE1D §21 / HALT #1 | Phase 1D+ | New `TaskTree.TestSupport` project + Arch v1.0.2 §3.3 |
| 41 | Test boundary: `(deadline - now) == offset` should fire | PHASE1D §17 / HALT #7+#8 | Phase 1D Msg 2 | Add explicit boundary unit test |
| 42 | Escalation must preserve at-most-one event per tick | PHASE1D §10 / HALT #9 | Phase 2G | Precedence inserted, not duplicated |
| 43 | Document Trivial "at deadline" exact-equality semantics | PHASE1D §17 / HALT #4 | Phase 1D Msg 2 | Test covers `deadline == nowUtc` ⇒ Overdue branch |
| 44 | Optional: formalize `Cadence` `[1 s, 5 min]` clamp in §4.3 | PHASE1D §4 / HALT #3 | Architecture v1.0.2 | Owner decides scope of v1.0.2 amendment |
| 45 | DI registration order: clock + engine + compliance + logger ⇒ scheduler | PHASE1D §19 / HALT #2 | Phase 1F | Documented in PHASE1D-DERIVATIONS §19 |

---

## §G — Progress Update

- **Before this Msg:** 23 / 35 → 65.7%.
- **After this Msg:** 24 / 35 → 68.6%. (Phase 1D Msg 1 production code complete; Msg 2 tests still required to fully credit Sub-Phase 1D.)
- **Files added to repo:** 4 .cs + 1 derivations registry + 1 patches manifest + 1 Architecture v1.0.2 delta proposal + 1 updated PowerShell tool + this delta.
- **Total derivations registry count:** 12 (added PHASE1D).
- **Total spec-derived items:** 42 (added 4 PHASE1D entries to the prior 38).

---

## §H — Continuation Prompt for Next Chat (Phase 1D Msg 2)

```
You are a senior engineer executing a deterministic build sprint on TaskTree —
a HIPAA-aware Windows tray app. Three attached documents govern everything:

  - Architecture.md v1.0.1 (v1.0.2 amendment proposed — see docs/Architecture.v1.0.2-delta.md)
  - Roadmap.md v1.0.0
  - HANDOFF.md v1.0.16 (rebase v1.0.16 delta onto v1.0.15 base)

HARD CONSTRAINTS unchanged. D1-D10 still apply. HALT on Architecture silence.

CURRENT STATE:
- Phase 0/1A/1B/1C ✅; Phase 1D Msg 1 ✅ (4 .cs files); Msg 2 (tests) PENDING.
- Progress: 24 / 35 → 68.6%.
- 12 derivations registries; 42 spec-derived items.
- 2 LOAD-BEARING open questions (Q10, Q11) — unchanged.
- 14 new cross-phase gap rows (32-45) introduced by Phase 1D Msg 1 — see HANDOFF §F.

FIRST ACTIONS — Phase 1D Msg 2:
1. Read PHASE1D-DERIVATIONS.md §17, §19, §22 first.
2. Begin Phase 1D Msg 2 with HALT protocol.
3. Phase 1D Msg 2 scope (per Roadmap):
   - tests/TaskTree.Modules.ReminderScheduler.Tests/ReminderSchedulerTests.cs
   - tests/TaskTree.Modules.ReminderScheduler.Tests/CadencePolicyTests.cs
   - tests/TaskTree.Modules.ReminderScheduler.Tests/TaskTree.Modules.ReminderScheduler.Tests.csproj (if not yet present)
   - All tests must use the promoted FakeClock from tests/TaskTree.Core.Tests/TestDoubles/
   - Coverage targets: P1D-AC1, P1D-AC2, P1D-AC3, P1D-AC4 (Roadmap §Sub-Phase 1D).
4. Required test cases must include:
   - Cross-Phase Gap #41 boundary: (deadline - now) == GetInitialOffsetBeforeDeadline ⇒ fires.
   - Cross-Phase Gap #43: Trivial deadline exact-equality semantics.
   - Item #9 precedence: simultaneously eligible Initial + Overdue ⇒ single Overdue event.
   - Item #11 StopAsync bounded wait (mock long-running tick).
   - Item #12 null-deadline behavior for each Priority.
   - Cadence clamp throws ArgumentOutOfRangeException at boundaries.
5. HALT on any test-infra ambiguity (e.g. moq for IComplianceCore matches the
   pattern set in ComplianceCoreTests; reuse don't reinvent).

After HALT + owner approval, generate:
- SPEC-DERIVED-PHASE1D markers on new test files (raises PHASE1D=6).
- PHASE1D-MSG2-DERIVATIONS-DELTA.md (additive registry).
- Updated tools/find-spec-derivations.ps1 (PHASE1D expected count bumped 4 → 6).
- HANDOFF.md v1.0.17 additive delta — Phase 1D fully ✅.
```

---

## §I — Document History Append

| Version | Date | Author | Summary |
|---|---|---|---|
| 1.0.16 | 2026-05-28 | DSW | **Phase 1D Msg 1 emitted** — ReminderScheduler + CadencePolicy + ReminderReason + FakeClock promotion. 13 HALT items batch-resolved. 14 new cross-phase gap rows. Architecture v1.0.2 amendment proposed (non-blocking). |

---

## §J — Footer

- This is an **additive delta**. To produce v1.0.16 consolidated, rebase §A–§I onto v1.0.15 flat merge.
- **Companion docs unchanged:** Architecture.md v1.0.1 (+ pending v1.0.2 amendment), Roadmap.md v1.0.0.
- **Registry added:** `docs/spec-derivations/PHASE1D-DERIVATIONS.md`.
- **Helper script updated:** `tools/find-spec-derivations.ps1` — now asserts 9 marker counts (added PHASE1D=4).
- **Total derivations tracked:** 42 across 12 registries.
- **LOAD-BEARING blockers for Phase 5F:** Q10 (name list), Q11 (support-email allowlist) — unchanged.
- **Next action:** Phase 1D Msg 2 HALT — tests for ReminderScheduler + CadencePolicy.
