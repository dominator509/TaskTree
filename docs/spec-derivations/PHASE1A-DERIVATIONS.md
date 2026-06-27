# Phase 1A — TaskEngine Spec-Derived Behaviors

**Status:** approved by Dominic Sarria-Wiley on 2026-05-26
**Grep marker:** `SPEC-DERIVED-PHASE1A`
**Total derived behaviors:** 7 (+ 1 test infrastructure convention recorded as §8)
**Companion docs:** Architecture.md v1.0.1, Roadmap.md v1.0.0, HANDOFF.md v1.0.7, `PHASE0-MSG2/3/4/5/6-DERIVATIONS.md`.

---

## Why this file exists

Architecture §4.2 declares the `ITaskEngine` surface but does not specify
several behaviors that the implementation must commit to (delete cascade,
missing-parent handling, id auto-generation, cross-cutting audit dependency,
storage layout, concurrency model, timestamp policy). The test file additionally
introduces test-infrastructure conventions (FakeClock, InMemorySecureStore, Moq
choice). All 8 items are recorded here for handoff traceability.

## How to use this file at handoff

1. `grep -r "SPEC-DERIVED-PHASE1A" src/` — locates `TaskEngine.cs`.
2. `grep -r "SPEC-DERIVED-PHASE1A" tests/` — locates `TaskEngineTests.cs`.
3. `grep -r "SPEC-DERIVED-PHASE1A" docs/` — locates this registry + HANDOFF.md delta.
4. Cross-check against Phase 1F Orchestrator wiring (DI MUST inject `IComplianceCore` into `TaskEngine`), Phase 1H E2E tests, Phase 5C coverage gate.
5. Promotion path: amend Architecture.md (§3.2 graph, §4.2 stub remarks), bump to v1.0.2.

---

## Derivation Registry

### 1. Delete cascade

| Field | Value |
| --- | --- |
| Approved by / date | Dominic Sarria-Wiley, 2026-05-26 |
| Source-of-truth | Roadmap P1A-AC4 "Tree integrity maintained on delete." Architecture §4.2 silent. |
| Spec gap | What happens to children when a parent is deleted? |
| Chosen behavior | Cascade-delete: remove the node AND all descendants atomically; one audit entry per deleted node. |
| Alternatives rejected | Orphan-promote (dangling reminders, surprising UX); refuse-if-children (bad sticky-note replacement UX per §1.1). |
| Downstream consumers | ReminderScheduler (Phase 1D), TreeViewUI (Phase 2B), DragDropBehavior (Phase 2C). |
| Re-validation tasks | • Confirm Phase 1D scheduler iterates current tree.<br>• Confirm Phase 2B UI refreshes after `DeleteAsync`.<br>• If a "soft delete" feature is ever added, revisit via Architecture amendment. |
| Risk if wrong | Dangling reminder firings; ghost UI nodes; HIPAA traceability gap. |
| Promotion path | Amend §4.2 stub remarks + §13 step 10 to specify cascade. |

### 2. `AddAsync` with non-existent `parentId`

| Field | Value |
| --- | --- |
| Source-of-truth | §4.2 stub accepts `Guid? parentId`; behavior on missing parent silent. |
| Chosen behavior | Throw `InvalidOperationException` with clear message. |
| Alternatives rejected | Silent fallback to root (masks caller bugs, creates orphans). |
| Re-validation | • Confirm Phase 2C drag-drop catches and surfaces user-friendly toast.<br>• Confirm Phase 1F Orchestrator does not silently swallow this exception. |
| Risk if wrong | Orphan nodes; corrupted tree on replay. |
| Promotion path | Amend §4.2 stub remarks. |

### 3. Id auto-generation

| Field | Value |
| --- | --- |
| Source-of-truth | §4.2 stub passes `TaskNode`; id-assignment policy silent. |
| Chosen behavior | If `node.Id == Guid.Empty` → assign `Guid.NewGuid()`; else use caller-supplied Id. |
| Alternatives rejected | Always overwrite (breaks fixture replay); always require caller (worse DX). |
| Re-validation | • Confirm test fixtures consistently supply explicit Ids when determinism matters.<br>• Confirm `System.Text.Json` round-trip preserves Id. |
| Risk if wrong | Collisions on replay; broken test fixtures. |
| Promotion path | Amend §4.2 stub remarks. |

### 4. Cross-cutting audit dependency (LOAD-BEARING)

| Field | Value |
| --- | --- |
| Source-of-truth | §4.6 "All modules emit audit events" vs §3.2 module graph showing `TaskEngine→SecureStore` only; Roadmap 1A Verification "Audit events raised on Add/Update/Delete/Complete"; §4.2 declares only 2 events. |
| Chosen behavior | `TaskEngine` constructor takes `IComplianceCore` in addition to `ISecureStore`/`IClock`/`IAppLogger`. All 4 operations (Add/Update/Delete/Complete) call `IComplianceCore.AuditAsync` directly. The 2 public events remain exactly as §4.2 declared (for UI/scheduler subscribers, NOT for audit). |
| Alternatives rejected | Orchestrator-wraps-and-audits — fragile because callers could bypass Orchestrator; would violate "All modules emit audit events." |
| Downstream consumers | Phase 1C `ComplianceCore` (receives `AuditAsync` calls); Phase 1F `Orchestrator` DI (MUST inject `IComplianceCore` into `TaskEngine`). |
| Re-validation | • **CRITICAL:** Phase 1F `ServiceRegistrations.cs` MUST register `IComplianceCore` as a singleton and inject it into `TaskEngine`. Without this, the entire HIPAA audit story breaks silently.<br>• Confirm Phase 1H E2E test asserts audit entries are written for each operation.<br>• If Architecture §3.2 module graph is ever updated, add the `TaskEngine→ComplianceCore` edge explicitly. |
| Risk if wrong | HIPAA technical-safeguard failure (§10.5 audit log incomplete); silent compliance violation. |
| Promotion path | Amend §3.2 module graph to show `TaskEngine→ComplianceCore` cross-cutting edge. |

### 5. Storage representation

| Field | Value |
| --- | --- |
| Source-of-truth | §4.5 `ISecureStore` is keyed string→object; §4.2 TaskEngine usage silent. |
| Chosen behavior | Single key `"tasks/tree"` (`TaskEngine.StorageKey`); value is a private DTO `TaskMap` wrapping `Dictionary<Guid, TaskNode>`; each stored node has `Children` empty (tree reconstructed from `ParentId` at read time). |
| Alternatives rejected | Persist nested tree — every mutation rewrites the entire blob; bad for §15 CRUD perf target. |
| Downstream consumers | Phase 1B `SecureStore` (must support `TaskMap` round-trip — `System.Text.Json` default policy). |
| Re-validation | • Confirm Phase 1B `SecureStore` uses `System.Text.Json` defaults.<br>• Confirm 1000-node fetch meets §15 < 100 ms target.<br>• If a different storage-key namespace emerges, document HERE before changing. |
| Risk if wrong | Perf regression on growing trees; `ISecureStore` key collision. |
| Promotion path | Amend §4.5 storage-key conventions or §4.2 storage layout per §Governance. |

### 6. Concurrency model

| Field | Value |
| --- | --- |
| Source-of-truth | Architecture silent. `ITaskEngine` methods are async; concurrent calls possible. |
| Chosen behavior | Single `SemaphoreSlim(1, 1)` gate guarding all public mutation AND read operations. |
| Alternatives rejected | Lock-free `ImmutableDictionary` (over-engineered for single-user app); reader-writer lock (negligible benefit). |
| Re-validation | • Confirm §15 CRUD < 50 ms still holds under the gate.<br>• If multi-user / cloud-sync ever lands (§6 "DEFERRED"), revisit this gate. |
| Risk if wrong | Read-while-write inconsistency; perf degradation. |
| Promotion path | Amend §4.2 stub remarks. |

### 7. Timestamp assignment

| Field | Value |
| --- | --- |
| Source-of-truth | `TaskNode` declares `CreatedAt`/`ModifiedAt`; exact policy silent. |
| Chosen behavior | `AddAsync`: `CreatedAt = clock.UtcNow` IF default else preserve caller value; `ModifiedAt` always refreshed. `UpdateAsync`: preserve stored `CreatedAt`; refresh `ModifiedAt`. `DeleteAsync`: no timestamp update. |
| Alternatives rejected | Always overwrite `CreatedAt`; never refresh `ModifiedAt`. |
| Re-validation | • Confirm Phase 2B UI binds to `ModifiedAt` for "last edited".<br>• Confirm Phase 1D scheduler uses `Deadline` only (not `ModifiedAt`) for cadence. |
| Risk if wrong | Misleading audit timestamps; stale UI; cadence drift. |
| Promotion path | Amend `TaskNode` XML doc + §4.2 stub remarks. |

### 8. Test infrastructure conventions (Phase 1A Msg 2)

| Field | Value |
| --- | --- |
| Conventions | (a) Inline `FakeClock` private nested class with settable `UtcNow`; (b) Inline `InMemorySecureStore` private nested class implementing `ISecureStore` against an in-memory `Dictionary<string, object?>`; (c) `Moq` mocks for `IComplianceCore` (with `AuditAsync` callback capturing entries into a `List<AuditEntry>`) and `IAppLogger` (no setups). |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval signal | "proceed" — interpreted as implicit approval for non-architectural test infrastructure choices following the explicit preview at the end of Phase 1A Msg 1. |
| Source-of-truth | Architecture.md silent on test-double conventions. §12 pins Moq 4.x as the mocking library; everything else is implementation latitude. |
| Spec gap | Where do test doubles live? Mock library used for which collaborators? |
| Chosen behavior | Test doubles live as **private nested classes inside `TaskEngineTests`** for Phase 1A. They are NOT promoted to a shared `tests/TestDoubles/` location until a second test class needs them. Moq is used only for collaborators where call-count verification matters (`IComplianceCore` → audit entry capture; `IAppLogger` → no verifies). |
| Alternatives rejected | Promote `FakeClock` + `InMemorySecureStore` to a shared `tests/TestDoubles/` library now — premature; YAGNI until second consumer arrives in Phase 1D / 1C tests. Use Moq for `ISecureStore` — Moq's generic-method setup is verbose; a real in-memory impl is clearer and faster. |
| Downstream consumers | Phase 1B `SecureStore.Tests` will need its OWN test for the real `SecureStore`; this `InMemorySecureStore` is test-only and lives inside `TaskTree.Modules.TaskEngine.Tests`. Phase 1C `ComplianceCore.Tests` may want to reuse `FakeClock`. Phase 1D `ReminderScheduler.Tests` definitely needs `FakeClock` for cadence tick simulation. |
| Re-validation tasks | • When Phase 1C/1D needs `FakeClock`, promote to a shared location and update this registry §8 with a Superseded-by-shared-location row.<br>• Confirm Phase 5C coverage gate counts both branches of `UpdateAsync_DoneToDone_DoesNotRaiseTaskCompleted`. |
| Risk if wrong | Code duplication across test projects if FakeClock isn't promoted promptly; over-coupling if promoted too early. |
| Promotion path | When a second consumer arrives, create `tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs` (or a dedicated `tests/TaskTree.TestSupport/` project) per §Governance. No Architecture.md change needed unless the location is canonical. |

---

## Cross-derivation interaction notes

- **Derivation 4 (audit) + Derivation 1 (cascade):** cascade-delete writes ONE audit entry per descendant. Phase 1C `ComplianceCore.AuditAsync` is responsible for atomicity guarantees against the chain (not TaskEngine's concern).
- **Derivation 5 (flat storage) + Derivation 6 (gate):** the gate covers Load+Mutate+Save as one critical section. Future move to optimistic concurrency must replace BOTH derivations together.
- **Derivation 7 (timestamps) + Derivation 3 (Id):** both use the "default-means-engine-assigns" pattern. If Id policy changes, review timestamp policy for parity.
- **Derivation 8 (test infrastructure) + Derivation 4 (audit dependency):** the `InMemorySecureStore` does not validate the `IComplianceCore` audit chain — tests assert audit entries by capturing them from the Moq callback, not by verifying chain integrity. That coverage belongs to Phase 1C tests against the real `ComplianceCore`.

---

## Cross-reference table

| Derivation | Affects (later phases) |
| --- | --- |
| 1. Cascade delete | Phase 1D, 2B, 2C |
| 2. Missing-parent exception | Phase 2C, 1F |
| 3. Id auto-generation | All TaskNode consumers; Phase 5B test fixtures |
| 4. Cross-cutting audit dep | Phase 1C, 1F (DI), 1H (E2E gate), 5C |
| 5. Flat-dictionary storage | Phase 1B (SecureStore), 4B (perf), 5C |
| 6. SemaphoreSlim gate | Phase 4B (perf), 5D (E2E concurrency) |
| 7. Timestamp policy | Phase 2B (UI binding), 1D (scheduler), 1C (audit timestamps) |
| 8. Test infrastructure conventions | Phase 1A Msg 2 test file; will affect Phase 1C, 1D when test doubles are promoted to a shared location. |

---

## Promotion / change procedure

Same as Msg 2–6 registries: amend Architecture, bump to v1.0.x, add Superseded-by row, update `HANDOFF.md`, re-run `tools/find-spec-derivations.ps1`.

---

## Document history

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 1.0.0 | 2026-05-26 | Dominic Sarria-Wiley (via Claude Opus) | Initial registry — 7 TaskEngine behavioral derivations approved during Phase 1A Msg 1. |
| 1.0.1 | 2026-05-26 | Dominic Sarria-Wiley (via Claude Opus) | Added §8 test infrastructure conventions during Phase 1A Msg 2. Marker count bumped from 1 to 2 (TaskEngine.cs + TaskEngineTests.cs). |
