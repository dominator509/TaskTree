# PHASE1D-DERIVATIONS.md — Phase 1D Msg 1

> **Scope:** ReminderScheduler + CadencePolicy + ReminderReason + FakeClock promotion.
> **Companion docs:** `Architecture.md` v1.0.1, `Roadmap.md` v1.0.0, `HANDOFF.md` v1.0.16.
> **Owner-approved HALT batch:** 13 items, batch-resolved per proposals.

---

## §1 Summary Table

| # | Item | Resolution | Files | LOAD-BEARING? |
|---|---|---|---|---|
| 1 | FakeClock promotion target | Option A — `tests/TaskTree.Core.Tests/TestDoubles/` | FakeClock.cs + 3 patched tests | No |
| 2 | ReminderScheduler ctor deps | `(IClock, ITaskEngine, IComplianceCore, IAppLogger)` | ReminderScheduler.cs | **YES (Phase 1F)** |
| 3 | `Cadence` property semantic | Tick (poll) interval; default 30 s; clamp [1 s, 5 min] | ReminderScheduler.cs | No |
| 4 | CadencePolicy public surface | Hybrid: static getters + `ShouldFire` | CadencePolicy.cs | No |
| 5 | ReminderReason enum values | `Initial`, `Repeat`, `Overdue` (Escalation deferred) | ReminderReason.cs | **Architecture amendment** |
| 6 | Last-fired state storage | In-memory `Dictionary<Guid, DateTimeOffset>` | ReminderScheduler.cs | No (Phase 2G promote) |
| 7 | P=1 "On creation" trigger | Option B — next-tick detection (no TaskAdded subscription) | CadencePolicy.cs | No |
| 8 | "Due / overdue" definition | Inline derivation; single `GetTreeAsync()` per tick | ReminderScheduler.cs + CadencePolicy.cs | No |
| 9 | Multiple firings per task/tick | Option A — at-most-one with precedence Overdue > Repeat > Initial | CadencePolicy.cs | No |
| 10 | Start/Stop lifecycle semantics | Start throws if running; Stop is no-op if stopped; restart allowed | ReminderScheduler.cs | No |
| 11 | StopAsync in-flight wait | Bounded 5 s wait; log warning on timeout | ReminderScheduler.cs | No (Phase 1F coord) |
| 12 | Null-deadline tasks | Critical fires (initial + repeat); High/Normal/Low/Trivial silently skip | CadencePolicy.cs | No (Phase 2B UI) |
| 13 | Escalation scope confirmation | Fully deferred to Phase 2G; no escalation logic in 1D | (negative — confirmation only) | No |

---

## §2 Item #1 — FakeClock Promotion Target

- **Trigger:** ReminderScheduler tests will be the 4th consumer of FakeClock (after TaskEngineTests, ComplianceCoreTests, AuditChainWriterTests). Per PHASE1A §8, this is the canonical promotion trigger.
- **Architecture silence:** §3.3 lists no shared TestSupport project; §12 doesn't constrain test infrastructure beyond MSTest 3.x + Moq 4.x.
- **Options considered:**
  - **A.** Promote into existing `tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs` (no Architecture amendment; smallest diff).
  - **B.** New `tests/TaskTree.TestSupport/` project (cleaner long-term; **requires Architecture v1.0.2 §3.3 amendment**).
- **Resolution:** Option A.
- **Rationale:** Smallest diff principle. The 4-consumer threshold is satisfied without paying the cost of a new project + Architecture amendment. Option B remains the right answer once a 5th consumer (or a non-test project) appears.
- **Files affected:** Created `tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs`. Patches required to remove inline copies — see `PATCHES-PHASE1D-MSG1.md`.
- **Gap for Handoff (Codex/Claude Code):**
  - During Phase 5A repo stitching, verify `grep -rn "class FakeClock" src/ tests/` returns exactly **one** match (the promoted file).
  - Each existing test csproj (`TaskTree.Modules.TaskEngine.Tests.csproj`, `TaskTree.Modules.ComplianceCore.Tests.csproj`, and the future `TaskTree.Modules.ReminderScheduler.Tests.csproj` from Msg 2) must add `<ProjectReference Include="..\TaskTree.Core.Tests\TaskTree.Core.Tests.csproj" />`.
  - If a 5th consumer appears, raise a HALT to re-evaluate Option B promotion (new project + Architecture v1.0.2 §3.3 amendment).

---

## §3 Item #2 — ReminderScheduler Constructor Dependencies

- **Trigger:** Module must be instantiable.
- **Architecture silence:** §4.3 declares the interface contract but is silent on the implementation's ctor signature.
- **Options considered:**
  - **A.** `(IClock, ITaskEngine, IAppLogger)` only — minimal.
  - **B.** Add `IComplianceCore` for per-fire audit, mirroring TaskEngine R6 LOAD-BEARING pattern.
  - **C.** Add `ISecureStore` for last-fired persistence (would also require Phase 2G's promotion to be implemented now).
- **Resolution:** Option B.
- **Rationale:** Auditing every ReminderFired event is the HIPAA-correct posture (§10.5 schema already supports `Action="ReminderFired"`). Mirrors the existing TaskEngine R6 ctor pattern (`IComplianceCore` is the second LOAD-BEARING audit injection in the codebase). `ISecureStore` deferred per Item #6.
- **Files affected:** `ReminderScheduler.cs` ctor signature.
- **Gap for Handoff (Codex/Claude Code):**
  - **LOAD-BEARING — Phase 1F.** `ServiceRegistrations.cs` MUST register `ReminderScheduler` with `IComplianceCore` injected as the 3rd ctor parameter. Failure to do so will surface as a DI resolution error at composition root build, but the safer path is to add an integration test in `OrchestratorTests` confirming `ReminderScheduler` audits on fire.
  - Add to the Phase 1F LOAD-BEARING checklist (existing #2 row in HANDOFF Cross-Phase Gap table). Phase 1F now has **two** audit-injection LOAD-BEARING flags (TaskEngine + ReminderScheduler).

---

## §4 Item #3 — `IReminderScheduler.Cadence` Semantic

- **Trigger:** Interface declares `TimeSpan Cadence { get; set; }` (singular) but §5.3 cadence is per-priority.
- **Architecture silence:** No prose binds the singular property to either tick interval or per-priority repeat.
- **Options considered:**
  - **A.** `Cadence` = tick (poll) interval; per-priority repeat handled by CadencePolicy.
  - **B.** `Cadence` = per-priority default override (conflicts with §5.3).
- **Resolution:** Option A. Default `TimeSpan.FromSeconds(30)`. Setter clamps to `[1 s, 5 min]` and throws `ArgumentOutOfRangeException`.
- **Rationale:** Roadmap Sub-Phase 1D narrative explicitly names a "30 s tick." Option A is the only interpretation consistent with both the interface shape and the Roadmap narrative.
- **Files affected:** `ReminderScheduler.cs` (`Cadence` property + constants).
- **Gap for Handoff (Codex/Claude Code):**
  - The clamp range `[1 s, 5 min]` is implementation-defined, not Architecture-normative. Flag for **Architecture v1.0.2 §4.3 amendment** if the owner wants the bounds formally locked.
  - See HANDOFF Cross-Phase Gap #44.

---

## §5 Item #4 — `CadencePolicy.cs` Public Surface

- **Trigger:** Roadmap 1D names the file; Architecture §4.3 doesn't enumerate its members.
- **Architecture silence:** No interface declared; no methods declared.
- **Options considered:**
  - **A.** Static utility class with three getters returning `TimeSpan`.
  - **B.** Stateful `sealed class` implementing a future `ICadencePolicy` (over-engineered for 1D).
  - **C.** Static `Decide(...)` returning a `(bool, ReminderReason)` tuple.
- **Resolution:** Hybrid A + C.
  - `GetInitialOffsetBeforeDeadline(Priority)` → §5.3 col 2.
  - `GetRepeatCadence(Priority)` → §5.3 col 3.
  - `ShouldFire(TaskNode, lastFiredUtc, nowUtc, out ReminderReason)` → decision.
  - **No** escalation getter (deferred to Phase 2G).
- **Rationale:** Static class keeps the policy pure and trivially testable, while `ShouldFire` is the single decision call ReminderScheduler invokes per node per tick. Avoids inventing an `ICadencePolicy` interface that has no second implementer in the system.
- **Files affected:** `CadencePolicy.cs`.
- **Gap for Handoff (Codex/Claude Code):**
  - If Phase 2G EscalationPolicy needs the escalation-after windows from §5.3 col 4, the natural place is to add `GetEscalationAfter(Priority)` to `CadencePolicy`. Mark in PHASE2G derivations.

---

## §6 Item #5 — `ReminderReason` Enum Values

- **Trigger:** P1D-AC3 ("ReminderDue includes node + reason"). `ReminderEvent` model carries the `Reason` field per PHASE0-MSG3.
- **Architecture silence:** No enumeration values declared in §4.3 or §5.3. Existing enums list (§3.3): Priority, TaskStatus, ReminderCadence, UpdateChannel, BugSeverity — **5 enums**.
- **Options considered:**
  - **A.** String literals on `ReminderEvent.Reason`.
  - **B.** Typed enum `ReminderReason` placed at `src/TaskTree.Core/Enums/ReminderReason.cs` (6th enum).
  - **C.** Nested enum inside `ReminderEvent` model (no §3.3 amendment but cross-cuts responsibility).
- **Resolution:** Option B with values `Initial`, `Repeat`, `Overdue` (Escalation deferred to Phase 2G).
- **Rationale:** Typed enum is idiomatic .NET, enables exhaustive switch handling in delivery adapters (Phase 1G), and Option B placement matches the existing §3.3 pattern. Adding `Escalation` later is additive (enum value 3, no breaking change).
- **Files affected:** `src/TaskTree.Core/Enums/ReminderReason.cs` (new).
- **Gap for Handoff (Codex/Claude Code):**
  - **REQUIRES Architecture v1.0.2 amendment.** See `docs/Architecture.v1.0.2-delta.md`. The amendment is small: §3.3 enums folder gains a 6th file. No interface signature changes; no other module impact.
  - Owner must approve v1.0.2 before Phase 5F sign-off (otherwise a marker in `find-spec-derivations.ps1` will reference a file that isn't accounted for in Architecture §3.3).
  - See HANDOFF Cross-Phase Gap #33.

---

## §7 Item #6 — Last-Fired State Storage

- **Trigger:** §5.3 repeat cadences require per-task last-fired tracking.
- **Architecture silence:** §4.3 + §5.3 silent on persistence; ISecureStore not listed in §4.3 ReminderScheduler tech stack.
- **Options considered:**
  - **A.** In-memory `Dictionary<Guid, DateTimeOffset>` — transient; restart resets.
  - **B.** Persist to SecureStore under `"reminders/state"` — durable; adds ISecureStore dep + canonical key.
- **Resolution:** Option A for Phase 1D.
- **Rationale:** Smallest diff. Restart cost is a single over-fire per task whose repeat window has already elapsed — acceptable for v1.0. Audit chain records both fires honestly (no data corruption). Phase 2G SnoozeService will also need persistence — both will be promoted together in a single Architecture v1.0.x amendment.
- **Files affected:** `ReminderScheduler.cs` (`_lastFiredUtc` field).
- **Gap for Handoff (Codex/Claude Code):**
  - **Phase 2G amendment:** introduce `"reminders/state"` SecureStore key. Schema candidate: `Dictionary<Guid, ReminderState>` where `ReminderState { DateTimeOffset LastFiredUtc; DateTimeOffset? SnoozedUntilUtc; }`.
  - Add `ISecureStore` to `ReminderScheduler` ctor at that time (5th param) — represents a non-breaking ctor expansion if registered via DI.
  - See HANDOFF Cross-Phase Gap #34.

---

## §8 Item #7 — P=1 "On Creation" Trigger Mechanism

- **Trigger:** §5.3 row P1 specifies "Initial Reminder: On creation."
- **Architecture silence:** No subscription model specified.
- **Options considered:**
  - **A.** Subscribe to `ITaskEngine.TaskAdded` and fire `ReminderDue` immediately.
  - **B.** Detect on next tick (`CadencePolicy.ShouldFire` returns Initial when `lastFiredUtc == null && Priority == Critical`).
- **Resolution:** Option B. Latency bounded by `Cadence` (30 s).
- **Rationale:** Keeps scheduler poll-only (no reactive event bus coupling), avoids re-entrancy risk on TaskAdded, simplifies testing. 30 s worst-case is acceptable for a v1.0 sticky-note replacement.
- **Files affected:** `CadencePolicy.cs` initial-fire branch for Critical.
- **Gap for Handoff (Codex/Claude Code):**
  - If real-world user feedback shows 30 s is too lax for Critical tasks, Phase 2G can add Option A as an opt-in fast-path (TaskAdded subscription emitting an immediate `Initial` event), while leaving the poll loop as the source of truth.
  - See HANDOFF Cross-Phase Gap #35.

---

## §9 Item #8 — "Due / Overdue" Definition

- **Trigger:** Multiple §5.3 rows reference deadline-relative timing.
- **Architecture silence:** No formal "due window" definition.
- **Options considered:**
  - **A.** Single `GetTreeAsync()` call; inline `CadencePolicy.ShouldFire(...)` filter.
  - **B.** Mix of `GetTreeAsync()` for pre-deadline + `GetOverdueAsync(now)` for overdue.
- **Resolution:** Option A.
- **Rationale:** One call honors P1A-AC5 (`< 100 ms for 1000-node fetch`) and Architecture §15 (`< 10 ms tick eval ≤ 1000 tasks`). Combining two engine calls per tick risks doubling I/O for no behavioral gain.
- **Files affected:** `ReminderScheduler.cs` `TickOnceAsync`.
- **Gap for Handoff (Codex/Claude Code):**
  - Phase 1D Msg 2 tests must include a 1000-node perf test stub (asserts tick < 10 ms warm). Defer the hard assertion to Phase 4B perf suite.

---

## §10 Item #9 — Multiple Firings per Task per Tick (Coalescing)

- **Trigger:** A long-paused process could resume with both Initial and Repeat eligible simultaneously.
- **Architecture silence:** Not addressed.
- **Options considered:**
  - **A.** At-most-one `ReminderDue` per task per tick (precedence Overdue > Repeat > Initial).
  - **B.** One event per eligible reason.
- **Resolution:** Option A.
- **Rationale:** Matches Pillar §1.4 "Quiet by Default." Avoids notification storms after sleep/wake.
- **Files affected:** `CadencePolicy.ShouldFire` precedence ladder.
- **Gap for Handoff (Codex/Claude Code):**
  - Phase 2G `EscalationPolicy` must preserve the at-most-one contract — escalation logic extends, not duplicates, the single per-tick event.
  - See HANDOFF Cross-Phase Gap #42.

---

## §11 Item #10 — StartAsync / StopAsync Lifecycle Semantics

- **Trigger:** Interface declares `StartAsync(ct)` + `StopAsync()`.
- **Architecture silence:** Idempotency, restart, disposal.
- **Options considered:**
  - **A.** Repeat-Start throws; Repeat-Stop is no-op; Start-after-Stop allowed.
  - **B.** Fully idempotent.
- **Resolution:** Option A. Single `_running` field guarded by `SemaphoreSlim(1,1)` (parallels TaskEngine R6 gate pattern).
- **Rationale:** Throw-on-repeat-Start surfaces caller bugs early. Idempotent stop is friendly to graceful-shutdown call chains. Restart support is required for settings changes (e.g. owner edits Cadence and toggles the scheduler).
- **Files affected:** `ReminderScheduler.cs` (`_lifecycleGate`, `_running`, lifecycle methods, `IDisposable`).
- **Gap for Handoff (Codex/Claude Code):**
  - Phase 5D should verify under real DI container teardown that `Dispose()`'s sync-over-async `StopAsync().GetAwaiter().GetResult()` does not deadlock when called from the UI thread context. If a deadlock surfaces, switch to `IAsyncDisposable.DisposeAsync` (additive interface — non-breaking).
  - See HANDOFF Cross-Phase Gap #39.

---

## §12 Item #11 — StopAsync Wait-for-In-Flight-Tick

- **Trigger:** Tick handler may be mid-execution at Stop.
- **Architecture silence:** No grace period.
- **Options considered:**
  - **A.** Cancel via internal CTS; await with bounded 5 s wait; log warning on timeout.
  - **B.** Hard abort (not idiomatic for PeriodicTimer).
- **Resolution:** Option A. Wait is performed **outside** `_lifecycleGate` to avoid deadlock (gate released before `Task.WhenAny`).
- **Files affected:** `ReminderScheduler.cs` `StopAsync`.
- **Gap for Handoff (Codex/Claude Code):**
  - Phase 1F orchestrator graceful-shutdown coordination must observe the 5 s bound — if total orchestrator shutdown SLA is < 5 s, the orchestrator should cancel before calling `StopAsync` so the in-flight tick aborts faster.
  - See HANDOFF Cross-Phase Gap #38.

---

## §13 Item #12 — Tasks with Null Deadline

- **Trigger:** Most §5.3 rows are deadline-relative.
- **Architecture silence:** Undefined for `Deadline == null`.
- **Options considered:**
  - **A.** Skip entirely.
  - **B.** P1-only fire-on-creation regardless of priority (no deadline binding).
  - **C.** P1 fires (on-creation + repeat ladder); P2–P5 skip silently.
- **Resolution:** Option C.
- **Rationale:** Preserves Critical safety net. Spam-free for non-critical untimed tasks.
- **Files affected:** `CadencePolicy.ShouldFire` null-deadline branch.
- **Gap for Handoff (Codex/Claude Code):**
  - Phase 2B Settings panel / UI tree should display an affordance: "Tasks without a deadline are not reminded unless Priority = Critical." Add to `MainWindowViewModel` tooltip set.
  - See HANDOFF Cross-Phase Gap #36.

---

## §14 Item #13 — Escalation Scope Confirmation

- **Trigger:** §5.3 column 4 specifies escalation; Roadmap 1D explicitly defers it.
- **Architecture silence:** None — explicit confirmation for handoff log.
- **Resolution:** Full escalation column deferred to Phase 2G `EscalationPolicy`. Phase 1D produces only `Initial / Repeat / Overdue` events. No audible chime, no persistent toast, no badge logic.
- **Files affected:** None (negative). `ReminderReason` enum intentionally omits `Escalation`.
- **Gap for Handoff (Codex/Claude Code):**
  - Phase 2G must:
    - Add `ReminderReason.Escalation = 3` (additive).
    - Add `CadencePolicy.GetEscalationAfter(Priority)`.
    - Add escalation precedence (likely above Overdue in `ShouldFire`).
    - Add persistent-toast / audible-chime branches to `ToastTier2Adapter` and `ToastTier1Adapter`.
  - See HANDOFF Cross-Phase Gap #37.

---

## §15 Files Produced This Msg

| Path | Purpose | SPEC-DERIVED-PHASE1D |
|---|---|---|
| `src/TaskTree.Modules.ReminderScheduler/ReminderScheduler.cs` | `IReminderScheduler` implementation | ✅ |
| `src/TaskTree.Modules.ReminderScheduler/CadencePolicy.cs` | Static §5.3 binding + `ShouldFire` | ✅ |
| `src/TaskTree.Core/Enums/ReminderReason.cs` | 3-value reason enum | ✅ |
| `tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs` | Promoted shared test double | ✅ |

Tests for ReminderScheduler + CadencePolicy: **deferred to Phase 1D Msg 2**.

---

## §16 SPEC-DERIVED-PHASE1D Marker Inventory

| File | Marker count |
|---|---|
| `ReminderScheduler.cs` | 1 |
| `CadencePolicy.cs` | 1 |
| `ReminderReason.cs` | 1 |
| `FakeClock.cs` | 1 |
| **Total distinct .cs files** | **4** |

`tools/find-spec-derivations.ps1` will assert `SPEC-DERIVED-PHASE1D = 4`.

---

## §17 Cross-Phase Gaps Introduced

| HANDOFF Row | Source HALT | Target Phase | Action Required |
|---|---|---|---|
| 32 | #2 | Phase 1F | Inject `IComplianceCore` into `ReminderScheduler` ctor (LOAD-BEARING) |
| 33 | #5 | Architecture v1.0.2 | Add `ReminderReason.cs` to §3.3 Enums |
| 34 | #6 | Phase 2G | Promote `_lastFiredUtc` → SecureStore `"reminders/state"` |
| 35 | #7 | Phase 2G | Optional fast-path `TaskAdded` subscription for P=1 latency |
| 36 | #12 | Phase 2B | UI tooltip "no deadline ⇒ no reminder unless Critical" |
| 37 | #13 | Phase 2G | Implement `EscalationPolicy` + `ReminderReason.Escalation` |
| 38 | #11 | Phase 1F | Orchestrator shutdown SLA must respect 5 s bound |
| 39 | #10 | Phase 5D | Verify `Dispose()` sync-over-async under real DI teardown |
| 40 | #1 | Phase 1D+ | 5th FakeClock consumer triggers Option B re-eval |
| 41 | #7 + #8 | Phase 1D Msg 2 | Test boundary: `(deadline - now) == offset` should fire |
| 42 | #9 | Phase 2G | Escalation must preserve at-most-one-per-tick |
| 43 | #4 | Phase 1D Msg 2 | Document Trivial "at deadline" exact-equality semantics |
| 44 | #3 | Architecture v1.0.2 | Optional: formalize `Cadence` `[1 s, 5 min]` clamp |
| 45 | #2 | Phase 1F | DI registration order: clock + engine + compliance + logger ⇒ scheduler |

---

## §18 Architecture Amendment Required (v1.0.2)

`ReminderReason` enum addition to §3.3:

```
src/TaskTree.Core/Enums/
├── Priority.cs
├── TaskStatus.cs
├── ReminderCadence.cs
├── UpdateChannel.cs
├── BugSeverity.cs
└── ReminderReason.cs   # NEW — Phase 1D Msg 1
```

Full delta in `docs/Architecture.v1.0.2-delta.md`. Owner approval required before Phase 5F sign-off.

Optional secondary v1.0.2 change: formalize `Cadence` `[1 s, 5 min]` clamp range in §4.3 prose (HANDOFF Gap #44).

---

## §19 Phase 1F Composition Root Checklist Additions

When Phase 1F begins authoring `ServiceRegistrations.cs`, the following must be true for `ReminderScheduler`:

1. ✅ `IClock` registered as singleton (already required for `TaskEngine`).
2. ✅ `ITaskEngine` registered as singleton.
3. ✅ `IComplianceCore` registered as singleton.
4. ✅ `IAppLogger` registered as singleton.
5. ✅ `IReminderScheduler` → `ReminderScheduler` registered as **singleton**.
6. ✅ Orchestrator startup invokes `IReminderScheduler.StartAsync(appShutdownToken)`.
7. ✅ Orchestrator shutdown invokes `IReminderScheduler.StopAsync()` (before disposing audit chain).
8. ✅ `Cadence` is left at default (30 s) for v1.0; Settings panel binding is Phase 2E concern.

**LOAD-BEARING:** Phase 1F now has **two** audit-injection flags (TaskEngine + ReminderScheduler). Both must hold or HIPAA audit chain silently breaks for the affected module.

---

## §20 Phase 2G Handoff Notes

Phase 2G `SnoozeService` + `EscalationPolicy` will need to:

- Add `SnoozedUntilUtc` to per-task state alongside `LastFiredUtc`.
- Promote both to SecureStore `"reminders/state"` (Architecture amendment).
- Add `ReminderReason.Escalation = 3` (additive enum value).
- Add `CadencePolicy.GetEscalationAfter(Priority)` returning §5.3 col 4 values.
- Update `ShouldFire` to insert escalation precedence (above Overdue when threshold passed).
- Coordinate with Phase 1G delivery adapters for audible chime (Critical) and persistent toast (High).

The single per-tick contract (Item #9 / Gap #42) MUST be preserved.

---

## §21 FakeClock Promotion — Implementation Note

**Chosen:** Option A — promotion into existing `tests/TaskTree.Core.Tests/TestDoubles/` namespace.

**Why:** smallest diff, zero Architecture amendment, satisfies PHASE1A §8.

**Option B (deferred):** new `tests/TaskTree.TestSupport/` project. Would require:
1. New csproj.
2. `TaskTree.sln` addition.
3. `Architecture.md` v1.0.2 §3.3 amendment listing the new project.
4. ProjectReferences from every test project to the new TestSupport project.

**Trigger to switch to Option B:** any of the following — 5th FakeClock consumer, a non-test project ever needs FakeClock, or shared mock infrastructure (e.g. `InMemorySecureStore`) reaches 3+ consumers (PHASE1A R7 already raised this candidate). Track as HANDOFF Gap #40.

---

## §22 Known Limitations

1. **Restart over-fire (Item #6):** A task whose repeat cadence has fully elapsed during process downtime will fire **once** on the next tick after restart. Audit chain records this honestly. Acceptable for v1.0 sticky-note semantics; resolved by Phase 2G persistence promotion.
2. **30 s Critical latency (Item #7):** Newly created Critical tasks may wait up to 30 s for first reminder. Acceptable per Roadmap 1D narrative; revisitable in Phase 2G.
3. **Null-deadline non-Critical silent skip (Item #12):** Documented; surfaced in UI per Gap #36.
4. **Sync `Dispose` over async `StopAsync` (Item #10):** Best-effort; verified during Phase 5D under live DI teardown.
5. **No escalation column behavior (Item #13):** No audible chime, no persistent toast, no badge — Phase 2G owns the full column.
