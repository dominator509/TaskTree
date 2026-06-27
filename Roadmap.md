# TaskTree — Roadmap.md

> **Source of Truth Companion to `Architecture.md` — v1.0.0**
> Owner: Dominic Sarria-Wiley
> Classification: Public
> Last Updated: 2026-05-26
> Companion Documents: `Architecture.md`, `HANDOFF.md`

---

## Preamble — Agentic Coding Protocol

This roadmap governs how AI coding agents (Claude Opus in-chat for Phases 0–4, Codex / Claude Code for Phase 5) execute the TaskTree build. It is binding. Any deviation must be logged in `HANDOFF.md` and approved by the human owner.

### Source of Truth

- **Primary spec:** `Architecture.md` (21 sections, locked).
- **Execution plan:** `Roadmap.md` (this file).
- **State tracker:** `HANDOFF.md` (live; updated each sub-phase).
- If `Architecture.md` is silent on any decision, agents MUST issue a HALT.

### D1–D10 Anti-Drift Rules

| # | Rule |
|---|---|
| D1 | Every code file MUST include a header comment citing the Architecture.md section(s) it implements. |
| D2 | Never rename classes, interfaces, methods, or properties defined in Architecture.md §3.3 / §4. |
| D3 | Never invent libraries, NuGet packages, or APIs not listed in Architecture.md §12 Tech Stack. |
| D4 | Never skip a sub-phase. Phases and sub-phases execute in declared order. |
| D5 | No placeholder/TODO code. Implement fully OR throw `NotImplementedException("<reason>")` per §21 Gap policy. |
| D6 | No synthetic test data that resembles real PHI (no real-looking names, MRNs, SSNs, DOBs). |
| D7 | No hardcoded credentials, secrets, tokens, keys, or production URLs. Use config + DPAPI. |
| D8 | If Architecture.md is silent on a class/property/algorithm/library/rule, output: `HALT: I need clarification on [X]`. Do not guess. |
| D9 | Every module must be independently testable via its interface (MSTest required). |
| D10 | All public methods, properties, and types have XML doc comments. |

### Anti-Hallucination HALT Protocol (MANDATORY)

```
BEFORE writing any code, the agent MUST:
- READ the relevant Architecture.md section(s)
- LIST the exact classes, interfaces, methods to create
- CONFIRM the list matches Architecture.md — no additions, no omissions
- ONLY THEN begin writing code

IF the agent is unsure about ANY of:
- A class/property name
- An algorithm or formula
- A library or API
- A behavioral rule
- A compliance requirement

THEN the agent MUST output:
"HALT: I need clarification on [specific question].
Architecture.md Section [X.Y] does not specify [what is missing].
Please advise before I proceed."

The agent MUST NOT:
- Guess
- Use "reasonable defaults"
- Proceed with TODO comments
- Invent workarounds
```

### Mandatory Sub-Roadmap Generation Protocol

Before any sub-phase begins coding, the agent MUST emit a **Sub-Roadmap** declaring:
1. The Architecture.md sections being implemented.
2. The exact file paths to be created (from §3.3).
3. The exact class/interface/method names (from §4).
4. The acceptance criteria being targeted.
5. Any items that will be stubbed and the reason (per Gap Classification §21).

### Sub-Roadmap Prompt Template

```
You are executing Sub-Phase {ID} — {Title}.
Source of Truth:
- Architecture.md sections: {LIST}
- Roadmap.md sub-phase: {ID}
Before writing code, output a Sub-Roadmap with:
1. Files to create (full paths from §3.3)
2. Types to define (names from §4)
3. Public method signatures (from §4 stubs)
4. Acceptance criteria targeted
5. Items being stubbed + reason
Apply D1–D10. Apply HALT protocol. Do not exceed declared scope.
On completion, update HANDOFF.md §Files Produced and §Gap Summary.
```

---

## Master Roadmap Overview

```
        +------------------------------------------------+
        |              CHAT-ONLY BUILD ZONE              |
        |      (Claude Opus / GPT-class chat agent)      |
        +------------------------------------------------+
                                |
  +---------+  +----------+    |    +----------+  +----------+
  | Phase 0 |->| Phase 1  |----+--->| Phase 2  |->| Phase 3  |
  |Scaffold |  |Core MVP  |    |    |Secondary |  |Extended  |
  | (LOW)   |  |(1A-1H)   |    |    |(2A-2G)   |  |(3A-3F)   |
  +---------+  +----------+    |    +----------+  +----------+
                                |                       |
                                v                       v
                        +--------------+       +--------------+
                        |   Phase 4    |------>|   Phase 5    |
                        |  Hardening   |       |   Handoff    |
                        |  (4A-4D)     |       |  (5A-5F)     |
                        +--------------+       +--------------+
                                                       |
                            +--------------------------+---------+
                            |       CODEX / CLAUDE CODE ZONE     |
                            |   (live build, test, sign, deploy) |
                            +------------------------------------+
```

---

## Phase Gate Rules

1. No phase may begin until the previous phase's acceptance gate passes.
2. No sub-phase may begin until its input dependencies are complete.
3. Every sub-phase produces a deliverable artifact (code zip, test results, or document update).
4. `HANDOFF.md` must be updated at the end of every sub-phase.
5. Phase 5 is the only phase that runs outside the chat environment.
6. Human owner approval is required to advance past Phase 1, 3, 4, and 5 gates.

---

# Phase 0 — Project Scaffold

**Complexity:** LOW (fully chat-buildable)
**Goal:** Produce a compilable scaffold containing every interface, model, enum, and security primitive declared in Architecture.md. No business logic yet.

### Mandatory Sub-Roadmap Prompt

```
Sub-Phase: Phase 0 — Scaffold
Source of Truth: Architecture.md §3.3, §4, §10, §12
Output:
1. Solution + 11 csproj files (per §3.3)
2. 11 interfaces under TaskTree.Core/Abstractions/
3. 5 models under TaskTree.Core/Models/
4. 5 enums under TaskTree.Core/Enums/
5. AesGcmCryptoProvider + HashChain under TaskTree.Core/Security/
6. FileAppLogger under TaskTree.Core/Logging/
7. MSTest test project skeletons (8 test csproj)
Apply D1-D10. Interfaces only - no business logic.
```

### Objective
Build the scaffold that all later phases fill in. Establish namespaces, file paths, interface contracts, and security primitives.

### Architecture References

| Section | Title |
|---|---|
| §3.3 | Folder Structure |
| §4.1-4.8 | Module Specifications |
| §10.3 | Encryption (AES-256-GCM + DPAPI) |
| §10.5 | Audit Logging Schema (hash chain) |
| §12 | Tech Stack |
| §19 | Chat-First Development Strategy |

### Input Dependencies
None.

### Deliverables

```
TaskTree.sln                                            [LOW]
src/TaskTree.Core/Abstractions/    (11 interfaces)      [LOW]
src/TaskTree.Core/Models/          (5 models)           [LOW]
src/TaskTree.Core/Enums/           (5 enums)            [LOW]
src/TaskTree.Core/Security/        (AesGcm + HashChain) [LOW]
src/TaskTree.Core/Logging/         (FileAppLogger)      [LOW]
src/TaskTree.Modules.*/            (8 empty csproj)     [LOW]
tests/*.Tests/                     (8 test skeletons)   [LOW]
```

### Anti-Drift Constraints
- All file paths MUST match Architecture §3.3 exactly.
- All interface names MUST match §4 exactly.
- Tech stack restricted to §12.

### Verification Checkpoint
- [ ] `dotnet restore` succeeds.
- [ ] `dotnet build` succeeds with 0 errors.
- [ ] All 11 interfaces, 5 models, 5 enums present.
- [ ] AES-256-GCM round-trip passes.
- [ ] HashChain SHA-256 chaining passes.
- [ ] FileAppLogger writes valid JSON lines.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P0-AC1 | Solution builds 0 errors |
| P0-AC2 | All interfaces match §4 |
| P0-AC3 | AES-256-GCM round-trip works |
| P0-AC4 | Hash chain integrity verified |
| P0-AC5 | Logger writes valid JSON |

### Chat Strategy
- Msg 1: solution + csproj. Msg 2: 11 interfaces. Msg 3: 5 models. Msg 4: 5 enums. Msg 5: security + logging primitives. Msg 6: test skeletons + 5 primitive tests.

### Codex Handoff Notes
None for Phase 0.

### Phase 0 Gate
All checkpoints + ACs pass.

---

# Phase 1 — Core MVP

**Goal:** TaskTree functions end-to-end at minimum: add a task, persist encrypted, schedule + fire reminders, surface via tray (stubbed).

## Sub-Phase 1A — TaskEngine

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 1A — TaskEngine
Source of Truth: Architecture.md §4.2, §3.3
Output: TaskEngine.cs + tests (>=12 cases). No external deps beyond ISecureStore + IClock + IAppLogger.
Apply D1-D10.
```

### Objective
Implement hierarchical CRUD with priority (1-5) and deadlines; persist via ISecureStore; raise events.

### Architecture References
| Section | Title |
|---|---|
| §4.2 | TaskEngine |
| §5.3 | Cadence Timing Table |
| §10.5 | Audit Logging Schema |

### Input Dependencies
Phase 0 (ITaskEngine, TaskNode, Priority, TaskStatus).

### Deliverables
```
src/TaskTree.Modules.TaskEngine/TaskEngine.cs              [LOW]
tests/TaskTree.Modules.TaskEngine.Tests/TaskEngineTests.cs [LOW]
```

### Anti-Drift Constraints
- Class name `TaskEngine` only.
- Method signatures match ITaskEngine exactly.
- Use `IClock` — never `DateTime.Now`.
- Persist after every mutation.

### Verification Checkpoint
- [ ] All ITaskEngine methods implemented.
- [ ] No `DateTime.Now` usage.
- [ ] Audit events raised on Add/Update/Delete/Complete.
- [ ] >=12 unit tests pass.
- [ ] CRUD < 50 ms.

### Acceptance Criteria
| ID | Criterion |
|---|---|
| P1A-AC1 | AddAsync persists and raises TaskAdded |
| P1A-AC2 | UpdateAsync raises TaskCompleted when status = Done |
| P1A-AC3 | GetOverdueAsync returns past-deadline non-Done nodes |
| P1A-AC4 | Tree integrity maintained on delete |
| P1A-AC5 | 1000-node fetch < 100 ms |

### Chat Strategy
One message TaskEngine.cs (<=400 lines), one message tests.

### Codex Handoff Notes
None.

---

## Sub-Phase 1B — SecureStore

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 1B — SecureStore
Source of Truth: Architecture.md §4.5, §10.3, §10.7
Output: SecureStore.cs + MasterKeyManager.cs + tests.
Master key path: %LOCALAPPDATA%\TaskTree\keys\master.bin. Apply D1-D10.
```

### Objective
Encrypted local persistence; DPAPI-wrapped master key; AES-256-GCM JSON storage.

### Architecture References
| Section | Title |
|---|---|
| §4.5 | SecureStore |
| §10.3 | Encryption at rest |
| §10.7 | Integrity Controls |

### Input Dependencies
Phase 0 (AesGcmCryptoProvider, ISecureStore, IAppLogger).

### Deliverables
```
src/TaskTree.Modules.SecureStore/SecureStore.cs         [LOW]
src/TaskTree.Modules.SecureStore/MasterKeyManager.cs    [MEDIUM]
tests/TaskTree.Modules.SecureStore.Tests/*              [LOW]
```

### Anti-Drift Constraints
- Use only `System.Security.Cryptography.AesGcm` + `ProtectedData`.
- Tag verified on every read; mismatch throws `CryptographicException`.

### Verification Checkpoint
- [ ] AES-256-GCM round-trip with random nonce.
- [ ] DPAPI wrap/unwrap succeeds.
- [ ] Byte tamper -> CryptographicException.
- [ ] R/W < 100 ms for <=10 MB.

### Acceptance Criteria
| ID | Criterion |
|---|---|
| P1B-AC1 | LoadAsync returns null for missing key |
| P1B-AC2 | SaveAsync -> LoadAsync returns equal payload |
| P1B-AC3 | Tampered ciphertext -> Load throws |
| P1B-AC4 | Master key persists across restarts |
| P1B-AC5 | DeleteAsync removes data + tag |

### Chat Strategy
One message: store + key manager. One message: tests.

### Codex Handoff Notes
- DPAPI live tests marked `[TestCategory("Live")]`, run in Phase 5E.

---

## Sub-Phase 1C — ComplianceCore (Baseline)

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 1C — ComplianceCore (audit + redactor)
Source of Truth: Architecture.md §4.6, §10.5, §10.7, §9.2.3
Output: ComplianceCore.cs + PhiRedactor.cs + AuditChainWriter.cs + tests.
Idle monitor deferred to 2F. Apply D1-D10.
```

### Objective
Hash-chained audit log + PHI redactor; idle monitor stubbed for 2F.

### Architecture References
| Section | Title |
|---|---|
| §4.6 | ComplianceCore |
| §10.5 | Audit Schema |
| §10.7 | Integrity |
| §9.2.3 | Redaction |

### Input Dependencies
Phase 0 (HashChain, IAppLogger, ISecureStore).

### Deliverables
```
src/TaskTree.Modules.ComplianceCore/ComplianceCore.cs   [LOW]
src/TaskTree.Modules.ComplianceCore/PhiRedactor.cs      [LOW]
src/TaskTree.Modules.ComplianceCore/AuditChainWriter.cs [LOW]
tests/TaskTree.Modules.ComplianceCore.Tests/*           [LOW]
```

### Anti-Drift Constraints
- Hash: `SHA256(prevHash || canonicalJson(entryWithoutHash))`.
- Synthetic test inputs only (D6).
- `StartIdleMonitor` throws `NotImplementedException("Deferred to 2F")`.

### Verification Checkpoint
- [ ] Append-only audit log; tamper invalidates chain.
- [ ] Redactor masks SSN, phone, email, MRN-like, dates.
- [ ] Audit write < 20 ms; chain verify on 10k < 500 ms.

### Acceptance Criteria
| ID | Criterion |
|---|---|
| P1C-AC1 | AuditAsync appends valid chain entry |
| P1C-AC2 | Byte change -> VerifyChainIntegrity false |
| P1C-AC3 | Redactor handles 5 PHI patterns |
| P1C-AC4 | RedactPhi never throws on null/empty |
| P1C-AC5 | Entry includes actor, module, action, result |

### Chat Strategy
Three source messages + two test messages.

### Codex Handoff Notes
- Idle monitor full impl in 2F (Win32 PInvoke).

---

## Sub-Phase 1D — ReminderScheduler

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 1D — ReminderScheduler
Source of Truth: Architecture.md §4.3, §5.3, §5.4
Output: ReminderScheduler.cs + CadencePolicy.cs + tests.
Snooze + escalation deferred to 2G. Apply D1-D10.
```

### Objective
Periodic 30s tick evaluates tree; raises `ReminderDue` per §5.3 cadence.

### Architecture References
| Section | Title |
|---|---|
| §4.3 | ReminderScheduler |
| §5.3 | Cadence Timing Table |
| §5.4 | Timing Diagram |

### Input Dependencies
1A.

### Deliverables
```
src/TaskTree.Modules.ReminderScheduler/ReminderScheduler.cs  [LOW]
src/TaskTree.Modules.ReminderScheduler/CadencePolicy.cs      [LOW]
tests/TaskTree.Modules.ReminderScheduler.Tests/*             [LOW]
```

### Anti-Drift Constraints
- Cadence values match §5.3 exactly.
- `System.Threading.PeriodicTimer` only.
- Time source = injected `IClock`.

### Verification Checkpoint
- [ ] P1 fires every 5 min once active.
- [ ] P5 silent unless past deadline.
- [ ] Tick eval < 10 ms for <=1000 tasks.
- [ ] No double-fire in window.

### Acceptance Criteria
| ID | Criterion |
|---|---|
| P1D-AC1 | StartAsync ticks; StopAsync halts |
| P1D-AC2 | Cadence respects §5.3 for all priorities |
| P1D-AC3 | ReminderDue includes node + reason |
| P1D-AC4 | Snooze/escalation NOT implemented (deferred) |

### Chat Strategy
One message scheduler + policy; one message tests.

### Codex Handoff Notes
None.

---

## Sub-Phase 1E — TrayHost (Stub)

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 1E — TrayHost (HIGH, stubbed)
Source of Truth: Architecture.md §4.1, §21
Output: TrayHost.cs + HotkeyInterop.cs (stubs) + event-wiring tests.
Initialize() throws NotImplementedException with Codex 5E reason. Apply D1-D10.
```

### Objective
Stub TrayHost that compiles, exposes correct event surface, marks Win32 work for Codex 5E.

### Architecture References
| Section | Title |
|---|---|
| §4.1 | TrayHost |
| §7 | Tier Fallback Chain |
| §21 | Gap Classification |

### Input Dependencies
Phase 0 (ITrayHost).

### Deliverables
```
src/TaskTree.Modules.TrayHost/TrayHost.cs               [HIGH-stub]
src/TaskTree.Modules.TrayHost/HotkeyInterop.cs          [HIGH-stub]
tests/TaskTree.Modules.TrayHost.Tests/TrayHostTests.cs  [LOW]
```

### Anti-Drift Constraints
- `Initialize()` throws `NotImplementedException("HIGH: NotifyIcon + RegisterHotKey require live env - Codex Phase 5E")`.
- Events declared and manually raisable.

### Verification Checkpoint
- [ ] Compiles.
- [ ] Events declared per §4.1.
- [ ] Stubs throw with clear reason.
- [ ] HANDOFF.md gap entries logged (Environment Gap x2).

### Acceptance Criteria
| ID | Criterion |
|---|---|
| P1E-AC1 | TrayHost compiles |
| P1E-AC2 | Events raise correctly via reflection test |
| P1E-AC3 | Live methods stubbed per D5 |

### Chat Strategy
One message both files; one message tests.

### Codex Handoff Notes
- Phase 5E: implement `H.NotifyIcon.Wpf` + `RegisterHotKey`/`UnregisterHotKey` PInvoke bound to message-only window.

---

## Sub-Phase 1F — Orchestrator Wiring

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 1F — Orchestrator
Source of Truth: Architecture.md §3.1, §3.2, §7
Output: Orchestrator.cs + CompositionRoot.cs + ServiceRegistrations.cs + tests.
Apply D1-D10.
```

### Objective
DI composition root; subscribe TrayHost -> TaskEngine -> ReminderScheduler; placeholder delivery until 1G.

### Architecture References
| Section | Title |
|---|---|
| §3.1 | High-Level Diagram |
| §3.2 | Module Dependency Graph |
| §7 | Tier Fallback Chain |

### Input Dependencies
1A-1E.

### Deliverables
```
src/TaskTree.Orchestrator/Orchestrator.cs               [LOW]
src/TaskTree.App/Bootstrap/CompositionRoot.cs           [LOW]
src/TaskTree.App/Bootstrap/ServiceRegistrations.cs      [LOW]
tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs  [LOW]
```

### Anti-Drift Constraints
- Constructor injection only.
- DI lifetimes per Architecture (singletons for stateful modules).

### Verification Checkpoint
- [ ] DI container resolves Orchestrator.
- [ ] Simulated flow: tray click -> task add -> reminder due -> log.
- [ ] No circular deps.

### Acceptance Criteria
| ID | Criterion |
|---|---|
| P1F-AC1 | Container builds |
| P1F-AC2 | Simulated E2E flow succeeds |
| P1F-AC3 | Audit chain receives event entries |

### Chat Strategy
One message Orchestrator + bootstrap; one message tests.

### Codex Handoff Notes
- Live wiring verified in Phase 5D after TrayHost replaced.

---

## Sub-Phase 1G — Reminder Delivery Tier Chain

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 1G — Reminder Delivery Tier 1/2/3
Source of Truth: Architecture.md §7
Output: ReminderDeliveryService.cs + ToastTier{1,2,3}Adapter.cs + tests.
Tier 1 + Tier 3 stubbed. Apply D1-D10.
```

### Objective
Decision logic chooses Tier 1/2/3 based on Toast API availability and Focus Assist state.

### Architecture References
| Section | Title |
|---|---|
| §7 | Tier Fallback Chain |
| §4.4 | TreeViewUI (toast renderer in 2B) |

### Input Dependencies
1F.

### Deliverables
```
src/TaskTree.Orchestrator/ReminderDeliveryService.cs       [MEDIUM]
src/TaskTree.Orchestrator/ToastTier1Adapter.cs             [HIGH-stub]
src/TaskTree.Orchestrator/ToastTier2Adapter.cs             [MEDIUM]
src/TaskTree.Orchestrator/ToastTier3Adapter.cs             [HIGH-stub]
tests/TaskTree.Orchestrator.Tests/ReminderDeliveryTests.cs [LOW]
```

### Anti-Drift Constraints
- Decision tree matches §7 exactly.
- Tier 1/3 stub with D5 reason.

### Verification Checkpoint
- [ ] Toast avail + FA off -> Tier 1.
- [ ] FA on -> Tier 2.
- [ ] Toast API unavail -> Tier 2 then Tier 3.

### Acceptance Criteria
| ID | Criterion |
|---|---|
| P1G-AC1 | Decision tree correct across 4 input combos |
| P1G-AC2 | Tier 2 WPF XAML present |
| P1G-AC3 | Tier 1+3 throw NotImplementedException |

### Chat Strategy
Two messages: service + adapters, then tests.

### Codex Handoff Notes
- Phase 5E: implement `Windows.UI.Notifications` Tier 1 + NotifyIcon balloon Tier 3.

---

## Sub-Phase 1H — Phase 1 Integration Test Gate

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 1H — Phase 1 Gate
Source of Truth: Architecture.md §13, §15
Output: EndToEndOfflineTests.cs + HANDOFF.md update. No live tests. Apply D1-D10.
```

### Objective
Confirm Phase 1 deliverables work together offline. Data + event spine only.

### Architecture References
| Section | Title |
|---|---|
| §13 | End-to-End User Flow |
| §15 | Performance Targets |

### Input Dependencies
1A-1G.

### Deliverables
```
tests/TaskTree.Orchestrator.Tests/EndToEndOfflineTests.cs [LOW]
```

### Anti-Drift Constraints
- Assertions tie to §13 numbered steps.
- Perf assertions match §15.

### Verification Checkpoint
- [ ] AddTask -> Persist -> Schedule -> ReminderDue -> Audit entry verified.
- [ ] All §15 offline-measurable targets pass.
- [ ] Coverage >= 75% on Phase 1 modules.

### Acceptance Criteria
| ID | Criterion |
|---|---|
| P1H-AC1 | E2E offline tests 100% pass |
| P1H-AC2 | Coverage >= 75% |
| P1H-AC3 | HANDOFF.md updated |

### Chat Strategy
One message: E2E tests; one message: HANDOFF.md update.

### Codex Handoff Notes
- Live E2E in Phase 5C.

### Phase 1 Gate
All P1A-P1H pass + **human owner approval**.

---

# Phase 2 — Secondary Features

**Complexity:** MEDIUM with two HIGH stubs.
**Goal:** Make TaskTree usable daily — visible UI, hotkeys, settings, auto-logoff, snooze/escalation.

## Sub-Phase 2A — Global Hotkey Manager

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 2A — Hotkey Manager
Source of Truth: Architecture.md §4.1
Output: HotkeyManager.cs + HotkeyConfig.cs + tests (mocked PInvoke).
Apply D1-D10.
```

### Objective
Manage hotkey registration lifecycle; surface `HotkeyPressed` event per binding.

### Architecture References

| Section | Title |
|---|---|
| §4.1 | TrayHost |
| §10.4 | Access Controls |

### Input Dependencies
1E (TrayHost stub), 1B (SecureStore).

### Deliverables
```
src/TaskTree.Modules.TrayHost/HotkeyManager.cs                [HIGH]
src/TaskTree.Modules.TrayHost/HotkeyConfig.cs                 [LOW]
tests/TaskTree.Modules.TrayHost.Tests/HotkeyManagerTests.cs   [LOW]
```

### Anti-Drift Constraints
- PInvoke signatures match Win32 exactly.
- Hotkey IDs scoped per process.

### Verification Checkpoint
- [ ] Binding round-trips through SecureStore.
- [ ] Conflict detection returns user-facing error.
- [ ] Mocked Win32 unit tests pass.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P2A-AC1 | Default Ctrl+Alt+T configurable |
| P2A-AC2 | Bindings persist across restart |
| P2A-AC3 | Conflicts surface gracefully |

### Chat Strategy
One message manager+config; one message tests.

### Codex Handoff Notes
- Phase 5E: real `User32.dll` calls bound to message-only HWND.

---

## Sub-Phase 2B — TreeViewUI (Main Window)

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 2B — TreeViewUI Main Window
Source of Truth: Architecture.md §4.4, §12, §1.4
Output: MainWindow.xaml/.cs + MainWindowViewModel.cs + ReminderToast.xaml + ToastViewModel.cs + tests.
Apply D1-D10.
```

### Objective
Render task tree with priority color + deadline + quick-add; ModernWpfUI theming.

### Architecture References

| Section | Title |
|---|---|
| §4.4 | TreeViewUI |
| §1.4 | Design Pillars |
| §12 | Tech Stack |

### Input Dependencies
1A (ITaskEngine), 1F (Orchestrator).

### Deliverables
```
src/TaskTree.UI/Views/MainWindow.xaml                    [MEDIUM]
src/TaskTree.UI/Views/MainWindow.xaml.cs                 [LOW]
src/TaskTree.UI/ViewModels/MainWindowViewModel.cs        [LOW]
src/TaskTree.UI/Views/ReminderToast.xaml                 [MEDIUM]
src/TaskTree.UI/ViewModels/ToastViewModel.cs             [LOW]
tests/TaskTree.UI.Tests/MainWindowViewModelTests.cs      [LOW]
```

### Anti-Drift Constraints
- MVVM strict: no logic in code-behind.
- `CommunityToolkit.Mvvm` only (D3).

### Verification Checkpoint
- [ ] ViewModel exposes ObservableCollection.
- [ ] Add/Delete commands wired.
- [ ] XAML compiles.
- [ ] Window show < 200 ms warm.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P2B-AC1 | ViewModel 100% unit-tested |
| P2B-AC2 | Tree shows priority + deadline columns |
| P2B-AC3 | Quick-add appends to TaskEngine |

### Chat Strategy
Msg1: XAML. Msg2: ViewModels. Msg3: Toast XAML+VM. Msg4: tests.

### Codex Handoff Notes
- Pixel rendering, animations, focus verified live in 5E.

---

## Sub-Phase 2C — Drag-Drop Reordering / Reparenting

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 2C — Drag-Drop
Source of Truth: Architecture.md §4.4
Output: DragDropBehavior.cs + VM commands + tests.
Apply D1-D10.
```

### Objective
Reorder siblings and reparent nodes via drag-drop with cycle detection.

### Architecture References

| Section | Title |
|---|---|
| §4.4 | TreeViewUI |

### Input Dependencies
2B.

### Deliverables
```
src/TaskTree.UI/Behaviors/DragDropBehavior.cs            [MEDIUM]
tests/TaskTree.UI.Tests/DragDropBehaviorTests.cs         [LOW]
```

### Anti-Drift Constraints
- Cycle prevention before commit.
- Persistence updated after drop.

### Verification Checkpoint
- [ ] Cycle attempt rejected.
- [ ] Persistence updated post-drop.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P2C-AC1 | Drop reorders siblings |
| P2C-AC2 | Drop reparents to new parent |
| P2C-AC3 | Cycle attempt -> toast warning |

### Chat Strategy
One message behavior; one message tests.

### Codex Handoff Notes
- Live drag-drop verified in 5E.

---

## Sub-Phase 2D — Color-Coded Priority + Theme

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 2D — Priority Color + Theme
Source of Truth: Architecture.md §1.4, §5.3
Output: PriorityColorConverter.cs + Light/Dark.xaml + tests.
Apply D1-D10.
```

### Objective
Map priority 1-5 to color tokens; light/dark theme toggle.

### Architecture References

| Section | Title |
|---|---|
| §1.4 | Design Pillars |
| §5.3 | Cadence Table |

### Input Dependencies
2B.

### Deliverables
```
src/TaskTree.UI/Converters/PriorityColorConverter.cs       [LOW]
src/TaskTree.App/Resources/Themes/Light.xaml               [LOW]
src/TaskTree.App/Resources/Themes/Dark.xaml                [LOW]
tests/TaskTree.UI.Tests/PriorityColorConverterTests.cs     [LOW]
```

### Anti-Drift Constraints
- Color tokens AA-contrast minimum.

### Verification Checkpoint
- [ ] 5 priorities distinct tokens.
- [ ] Theme persists via SecureStore.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P2D-AC1 | Converter returns correct brush per priority |
| P2D-AC2 | Theme persists across restart |

### Chat Strategy
One message converter+themes; one message tests.

### Codex Handoff Notes
None.

---

## Sub-Phase 2E — Settings Panel

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 2E — Settings Panel
Source of Truth: Architecture.md §2, §9.1, §9.2, §10.10
Output: SettingsView.xaml/.cs + SettingsViewModel.cs + tests.
Apply D1-D10.
```

### Objective
UI for cadence overrides, hotkey rebinding, auto-logoff, theme, updater/bug-reporter toggles.

### Architecture References

| Section | Title |
|---|---|
| §2 | Locked Features (F11, F16) |
| §9.1, §9.2 | Updater + BugReporter toggles |
| §10.10 | Compliance Mode Toggle |

### Input Dependencies
2B, 2A, 1B.

### Deliverables
```
src/TaskTree.UI/Views/SettingsView.xaml                   [MEDIUM]
src/TaskTree.UI/ViewModels/SettingsViewModel.cs           [LOW]
tests/TaskTree.UI.Tests/SettingsViewModelTests.cs         [LOW]
```

### Anti-Drift Constraints
- Logoff timer 1-60 min.
- Toggles emit audit events.

### Verification Checkpoint
- [ ] Settings persist via SecureStore.
- [ ] Hotkey rebind integrates with 2A.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P2E-AC1 | Settings persist across restart |
| P2E-AC2 | Invalid values rejected |
| P2E-AC3 | Audit entry on toggle changes |

### Chat Strategy
Two messages: view+VM, tests.

### Codex Handoff Notes
- Visual polish in 5E.

---

## Sub-Phase 2F — Auto-Logoff + Session Lock

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 2F — Auto-Logoff
Source of Truth: Architecture.md §4.6, §10.4
Output: IdleMonitor.cs + SessionLockView.xaml/VM + tests.
Apply D1-D10.
```

### Objective
Detect idle >= timeout; lock app; require Windows re-auth.

### Architecture References

| Section | Title |
|---|---|
| §4.6 | ComplianceCore |
| §10.4 | Access Controls |

### Input Dependencies
1C (baseline), 2E (settings).

### Deliverables
```
src/TaskTree.Modules.ComplianceCore/IdleMonitor.cs        [MEDIUM]
src/TaskTree.UI/Views/SessionLockView.xaml                [MEDIUM]
src/TaskTree.UI/ViewModels/SessionLockViewModel.cs        [LOW]
tests/TaskTree.Modules.ComplianceCore.Tests/IdleMonitorTests.cs [LOW]
```

### Anti-Drift Constraints
- Re-auth cannot be skipped.
- Audit on lock + unlock.

### Verification Checkpoint
- [ ] Idle >= timer -> AutoLogoffTriggered.
- [ ] Unlock requires re-auth.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P2F-AC1 | 15-min default fires correctly (mocked) |
| P2F-AC2 | Lock blocks other interaction |
| P2F-AC3 | Unlock writes audit entry |

### Chat Strategy
One message monitor+VM; one message XAML+tests.

### Codex Handoff Notes
- Phase 5E: real `GetLastInputInfo` + CredUI verification.

---

## Sub-Phase 2G — Snooze + Escalation

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 2G — Snooze + Escalation
Source of Truth: Architecture.md §5.3
Output: SnoozeService.cs + EscalationPolicy.cs + scheduler updates + tests.
Apply D1-D10.
```

### Objective
User snoozes reminders; if past threshold, escalate tier + visual style.

### Architecture References

| Section | Title |
|---|---|
| §5.3 | Cadence + Escalation |

### Input Dependencies
1D, 1G.

### Deliverables
```
src/TaskTree.Modules.ReminderScheduler/SnoozeService.cs      [LOW]
src/TaskTree.Modules.ReminderScheduler/EscalationPolicy.cs   [LOW]
tests/TaskTree.Modules.ReminderScheduler.Tests/*             [LOW]
```

### Anti-Drift Constraints
- Snooze max 1 hour.
- Escalation thresholds match §5.3.

### Verification Checkpoint
- [ ] Snooze defers next ReminderDue.
- [ ] Escalation triggers per priority.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P2G-AC1 | Snooze options 5/15/30/60 min |
| P2G-AC2 | Escalation flips persistence flag |
| P2G-AC3 | Audit entry on snooze + escalation |

### Chat Strategy
One message services; one message tests.

### Codex Handoff Notes
None.

### Phase 2 Gate
All P2A-P2G pass.

---

# Phase 3 — Extended Integration

**Goal:** Ship-ready secondary features — updater and bug reporter.

## Sub-Phase 3A — AutoUpdater Core (Manifest + Verify)

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 3A — AutoUpdater Core
Source of Truth: Architecture.md §9.1.1-9.1.3
Output: AutoUpdater.cs + ManifestSigner.cs + HashVerifier.cs + tests.
No live HTTP in tests. Apply D1-D10.
```

### Objective
Fetch + parse manifest; verify Ed25519 signature + SHA-256 hash.

### Architecture References

| Section | Title |
|---|---|
| §9.1.1 | State Machine |
| §9.1.2 | Manifest Schema |
| §9.1.3 | Signature & Integrity |
| §9.1.6 | Threat Model |

### Input Dependencies
Phase 0 (IAutoUpdater, UpdateManifest, UpdateChannel).

### Deliverables
```
src/TaskTree.Modules.AutoUpdater/AutoUpdater.cs            [MEDIUM]
src/TaskTree.Modules.AutoUpdater/ManifestSigner.cs         [LOW]
src/TaskTree.Modules.AutoUpdater/HashVerifier.cs           [LOW]
tests/TaskTree.Modules.AutoUpdater.Tests/*                 [LOW]
```

### Anti-Drift Constraints
- NSec.Cryptography only for Ed25519.
- Public key embedded as compile-time constant.

### Verification Checkpoint
- [ ] Tampered manifest -> verify fails.
- [ ] Tampered hash -> verify fails.
- [ ] Valid pair -> verify succeeds.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P3A-AC1 | Manifest parses per §9.1.2 |
| P3A-AC2 | Signature verification rejects tampering |
| P3A-AC3 | Hash verification rejects mismatch |

### Chat Strategy
One message per file; one message tests.

### Codex Handoff Notes
- Live HTTP fetches verified in 5E.

---

## Sub-Phase 3B — AutoUpdater State Machine + Staging

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 3B — Updater State Machine
Source of Truth: Architecture.md §9.1.1, §9.1.4
Output: UpdaterStateMachine.cs + StagingService.cs + tests.
ApplyAsync stubbed for MSIX install. Apply D1-D10.
```

### Objective
Full state machine; stage packages on disk; defer MSIX install to Codex.

### Architecture References

| Section | Title |
|---|---|
| §9.1.1 | State Machine |
| §9.1.4 | Update Flow |

### Input Dependencies
3A.

### Deliverables
```
src/TaskTree.Modules.AutoUpdater/UpdaterStateMachine.cs    [MEDIUM]
src/TaskTree.Modules.AutoUpdater/StagingService.cs         [MEDIUM]
tests/TaskTree.Modules.AutoUpdater.Tests/UpdaterStateMachineTests.cs [LOW]
```

### Anti-Drift Constraints
- Transitions match §9.1.1 diagram.
- `ApplyAsync` throws `NotImplementedException("HIGH: Add-AppxPackage - Codex 5E")`.

### Verification Checkpoint
- [ ] Happy path 7 states.
- [ ] Verify-fail -> FAILED -> rollback.
- [ ] Staging writes binary.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P3B-AC1 | State machine matches §9.1.1 |
| P3B-AC2 | Staged file present after DOWNLOADING |
| P3B-AC3 | ApplyAsync stub throws clear reason |

### Chat Strategy
Two messages: machine+staging, tests.

### Codex Handoff Notes
- Phase 5E: `Add-AppxPackage` via PowerShell runspace.

---

## Sub-Phase 3C — Offline Import + Rollback Sentinel

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 3C — Offline Import + Rollback
Source of Truth: Architecture.md §9.1.4-9.1.5, §9.1.6 (T7)
Output: OfflineImportService.cs + SentinelService.cs + RollbackService.cs + tests.
Apply D1-D10.
```

### Objective
Manual air-gapped updates + auto-rollback on first-launch crash.

### Architecture References

| Section | Title |
|---|---|
| §9.1.4 | Update Flow (manual import) |
| §9.1.5 | Rollback Strategy |
| §9.1.6 | T7 mitigation |

### Input Dependencies
3A, 3B.

### Deliverables
```
src/TaskTree.Modules.AutoUpdater/OfflineImportService.cs   [MEDIUM]
src/TaskTree.Modules.AutoUpdater/SentinelService.cs        [LOW]
src/TaskTree.Modules.AutoUpdater/RollbackService.cs        [MEDIUM]
tests/TaskTree.Modules.AutoUpdater.Tests/*                 [LOW]
```

### Anti-Drift Constraints
- Local import uses same verify pipeline as remote (T7).
- Sentinel = `%LOCALAPPDATA%\TaskTree\sentinel.lock`.

### Verification Checkpoint
- [ ] Malicious manifest -> rejected.
- [ ] Sentinel cleared on successful launch.
- [ ] Rollback restores prior MSIX.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P3C-AC1 | Local import enforces sig + hash |
| P3C-AC2 | Sentinel triggers rollback on crash |
| P3C-AC3 | Rollback restores prior version |

### Chat Strategy
One message services; one message tests.

### Codex Handoff Notes
- Live rollback in 5E.

---

## Sub-Phase 3D — BugReporter Capture + Queue + Redaction

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 3D — BugReporter Capture
Source of Truth: Architecture.md §9.2.1-9.2.3
Output: BugReporter.cs + CrashCaptureHook.cs + BugReportQueue.cs + RedactionPipeline.cs + tests.
Apply D1-D10.
```

### Objective
Capture crashes + user submissions; redact PHI; persist locally.

### Architecture References

| Section | Title |
|---|---|
| §9.2.1 | Payload Schema |
| §9.2.2 | Crash Capture Policy |
| §9.2.3 | Redaction |

### Input Dependencies
1C, 1B.

### Deliverables
```
src/TaskTree.Modules.BugReporter/BugReporter.cs            [MEDIUM]
src/TaskTree.Modules.BugReporter/CrashCaptureHook.cs       [MEDIUM]
src/TaskTree.Modules.BugReporter/BugReportQueue.cs         [LOW]
src/TaskTree.Modules.BugReporter/RedactionPipeline.cs      [LOW]
tests/TaskTree.Modules.BugReporter.Tests/BugReporterTests.cs [LOW]
```

### Anti-Drift Constraints
- Never store unredacted payload.
- Fingerprint = SHA-256(stack || title).

### Verification Checkpoint
- [ ] Synthetic PHI fully redacted.
- [ ] Queue survives restart.
- [ ] Duplicates suppressed.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P3D-AC1 | Crash captured < 200 ms |
| P3D-AC2 | All 5 PHI patterns redacted |
| P3D-AC3 | Dedup by fingerprint works |

### Chat Strategy
Two messages: capture+queue, tests.

### Codex Handoff Notes
- Real crash injection in 5E.

---

## Sub-Phase 3E — BugReporter Delivery (Email + GitHub)

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 3E — BugReporter Delivery
Source of Truth: Architecture.md §9.2.4-9.2.6
Output: EmailDeliveryAdapter.cs (stub) + GitHubIssueAdapter.cs (stub) + FileDropAdapter.cs + DeliveryRouter.cs + tests.
Apply D1-D10.
```

### Objective
Route reports by severity per §9.2.4; retry on failure.

### Architecture References

| Section | Title |
|---|---|
| §9.2.4 | Routing Rules |
| §9.2.5 | Storage & Retention |
| §9.2.6 | Security Controls |

### Input Dependencies
3D.

### Deliverables
```
src/TaskTree.Modules.BugReporter/EmailDeliveryAdapter.cs     [HIGH-stub]
src/TaskTree.Modules.BugReporter/GitHubIssueAdapter.cs       [HIGH-stub]
src/TaskTree.Modules.BugReporter/FileDropAdapter.cs          [LOW]
src/TaskTree.Modules.BugReporter/DeliveryRouter.cs           [LOW]
tests/TaskTree.Modules.BugReporter.Tests/*                   [LOW]
```

### Anti-Drift Constraints
- Credentials from DPAPI-wrapped config only.
- Rate limit 5/min, 50/day.

### Verification Checkpoint
- [ ] Routing per severity matches §9.2.4.
- [ ] File drop writes to `bugreports\out\`.
- [ ] Rate limiter blocks excess.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P3E-AC1 | Severity 1 -> email + GitHub |
| P3E-AC2 | Severity 5 -> file drop only |
| P3E-AC3 | Rate limit enforced |

### Chat Strategy
One message adapters+router; one message tests.

### Codex Handoff Notes
- Phase 5E: live SMTP + GitHub PAT.

---

## Sub-Phase 3F — Phase 3 Integration Test Gate

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 3F — Phase 3 Gate
Source of Truth: Architecture.md §9
Output: Phase3IntegrationTests + HANDOFF.md update. Apply D1-D10.
```

### Objective
Updater + bug reporter integrate without affecting Phases 1/2.

### Architecture References

| Section | Title |
|---|---|
| §9 | Updater + Bug Reporter |

### Input Dependencies
3A-3E.

### Deliverables
```
tests/TaskTree.Modules.AutoUpdater.Tests/Phase3IntegrationTests.cs   [LOW]
tests/TaskTree.Modules.BugReporter.Tests/Phase3IntegrationTests.cs   [LOW]
```

### Anti-Drift Constraints
- No live network this sub-phase.

### Verification Checkpoint
- [ ] No module side-effects.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P3F-AC1 | Phase 3 offline tests 100% pass |
| P3F-AC2 | HANDOFF.md updated |

### Chat Strategy
One message tests.

### Codex Handoff Notes
- Live network in 5E.

### Phase 3 Gate
All P3A-P3F pass + **human owner approval**.

---

# Phase 4 — Hardening & Release

**Goal:** Ship-quality build.

## Sub-Phase 4A — Compliance Audit

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 4A — Compliance Audit
Source of Truth: Architecture.md §10
Output: compliance-audit.md + AuditChainStressTests.cs (10k + 100k). Apply D1-D10.
```

### Objective
Written audit confirming HIPAA controls implementation + integrity stress tests.

### Architecture References

| Section | Title |
|---|---|
| §10.1-10.10 | Compliance / Security |

### Input Dependencies
All prior phases.

### Deliverables
```
docs/compliance-audit.md                                      [LOW]
tests/TaskTree.Modules.ComplianceCore.Tests/AuditChainStressTests.cs [LOW]
```

### Anti-Drift Constraints
- Audit references real implementations only.

### Verification Checkpoint
- [ ] Each §10 control mapped to code file + test.
- [ ] 100k stress verify < 5 s.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P4A-AC1 | All §10 controls mapped |
| P4A-AC2 | Stress tests pass |
| P4A-AC3 | No PHI leaks in code review |

### Chat Strategy
One message report; one message stress tests.

### Codex Handoff Notes
- Live audit verification in 5F.

---

## Sub-Phase 4B — Performance Optimization

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 4B — Performance
Source of Truth: Architecture.md §15
Output: PerfBenchmarks.cs + perf-report.md + tuning patches if needed. Apply D1-D10.
```

### Objective
Confirm every §15 target met; tune if not.

### Architecture References

| Section | Title |
|---|---|
| §15 | Performance Targets |

### Input Dependencies
All prior code complete.

### Deliverables
```
tests/TaskTree.Perf.Tests/PerfBenchmarks.cs                   [LOW]
docs/perf-report.md                                           [LOW]
```

### Anti-Drift Constraints
- Benchmarks tied to §15 numeric targets.

### Verification Checkpoint
- [ ] All §15 targets pass or have documented gap.
- [ ] Idle RAM <= 80 MB (Codex 5E).

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P4B-AC1 | All measurable targets pass |
| P4B-AC2 | Misses logged with mitigation plan |

### Chat Strategy
Two messages: benchmarks + report.

### Codex Handoff Notes
- Live RAM/CPU measured in 5E.

---

## Sub-Phase 4C — Packaging (MSIX)

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 4C — MSIX Packaging
Source of Truth: Architecture.md §12, §9.1.3
Output: TaskTree.Installer.wapproj + Package.appxmanifest + build-msix.ps1 + signing-checklist.md. Apply D1-D10.
```

### Objective
MSIX project + manifest + build/sign script.

### Architecture References

| Section | Title |
|---|---|
| §12 | Tech Stack (MSIX) |
| §9.1.3 | Signature Model |

### Input Dependencies
All code phases.

### Deliverables
```
packaging/TaskTree.Installer.wapproj                          [LOW]
packaging/Package.appxmanifest                                [LOW]
packaging/build-msix.ps1                                      [LOW]
docs/signing-checklist.md                                     [LOW]
```

### Anti-Drift Constraints
- Self-contained .NET 8 publish.
- Authenticode signing required.

### Verification Checkpoint
- [ ] wapproj references all 11 module csproj.
- [ ] Manifest declares correct capabilities.
- [ ] Script runs end-to-end on Codex host.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P4C-AC1 | MSIX produced by build-msix.ps1 in Codex |
| P4C-AC2 | Signed binary installs cleanly |
| P4C-AC3 | First launch succeeds |

### Chat Strategy
One message wapproj+manifest; one message scripts+checklist.

### Codex Handoff Notes
- Phase 5E: build + sign + install verification.

---

## Sub-Phase 4D — Documentation & Deployment

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 4D — Docs + Deployment
Source of Truth: Architecture.md (all)
Output: README.md + user-guide.md + ops-runbook.md + release-checklist.md. Apply D1-D10.
```

### Objective
Owner-facing docs: install, configure, troubleshoot, operate.

### Architecture References

| Section | Title |
|---|---|
| All | (cross-reference) |

### Input Dependencies
All prior phases.

### Deliverables
```
README.md                                                     [LOW]
docs/user-guide.md                                            [LOW]
docs/ops-runbook.md                                           [LOW]
docs/release-checklist.md                                     [LOW]
```

### Anti-Drift Constraints
- No invented features.
- Cross-link to Architecture.md sections.

### Verification Checkpoint
- [ ] User guide covers all 16 locked features (§2).
- [ ] Ops runbook covers backup, restore, rollback, audit export.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P4D-AC1 | All four docs present |
| P4D-AC2 | Cross-links accurate |
| P4D-AC3 | Release checklist signed off |

### Chat Strategy
One doc per message.

### Codex Handoff Notes
None.

### Phase 4 Gate
All P4A-P4D pass + **human owner approval**.

---

# Phase 5 — Handoff & Gap Closure (Codex / Claude Code)

> Runs **entirely outside the chat environment.** Codex / Claude Code take over with the full repo, live OS, live network, and a Windows desktop.

## Sub-Phase 5A — Repo Stitching

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 5A — Repo Stitching
Source of Truth: Architecture.md §20
Tasks: run assemble-repo.ps1; verify file count vs §3.3; run verify-namespaces.ps1; resolve dupes; dotnet restore.
Output: stitch-report.md
```

### Objective
Assemble chat-produced zips into a single buildable repo.

### Architecture References

| Section | Title |
|---|---|
| §20 | Repo Stitching Protocol |

### Input Dependencies
All Phase 0-4 zip artifacts.

### Deliverables
```
stitch-report.md
```

### Anti-Drift Constraints
- No file renames during stitching.
- Duplicates resolved by inspection.

### Verification Checkpoint
- [ ] File count matches §3.3.
- [ ] Namespaces resolve.
- [ ] dotnet restore succeeds.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P5A-AC1 | Repo assembles 0 errors |
| P5A-AC2 | All §3.3 files present |

### Chat Strategy
N/A (Codex zone).

### Codex Handoff Notes
- This IS the Codex zone.

---

## Sub-Phase 5B — Compile Gap Closure

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 5B — Compile Gaps
Source of Truth: HANDOFF.md §Gap Summary
Tasks: dotnet build; address every error per D1-D10; update CHANGELOG.md.
Output: compile-gap-report.md
```

### Objective
Zero compile errors.

### Architecture References

| Section | Title |
|---|---|
| §21 | Gap Classification |

### Input Dependencies
5A.

### Deliverables
```
compile-gap-report.md
CHANGELOG.md (updated)
```

### Anti-Drift Constraints
- May add usings/fix typos.
- May NOT rename Architecture types.

### Verification Checkpoint
- [ ] dotnet build = 0 errors.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P5B-AC1 | Build succeeds Release config |

### Chat Strategy
N/A.

### Codex Handoff Notes
- Codex authority: bug fixes + missing impls only.

---

## Sub-Phase 5C — Test Gap Closure

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 5C — Test Gaps
Tasks: dotnet test --filter Category!=Live; 100% pass; coverage >= 75%.
Output: test-gap-report.md
```

### Objective
100% pass on offline test suite.

### Architecture References

| Section | Title |
|---|---|
| All | Phase 0-3 ACs |

### Input Dependencies
5B.

### Deliverables
```
test-gap-report.md
```

### Anti-Drift Constraints
- Tests may not weaken assertions to pass.

### Verification Checkpoint
- [ ] 100% offline tests pass.
- [ ] Coverage >= 75%.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P5C-AC1 | Offline suite 100% green |
| P5C-AC2 | Coverage targets met |

### Chat Strategy
N/A.

### Codex Handoff Notes
- Codex zone.

---

## Sub-Phase 5D — Integration Gap Closure

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 5D — Integration Gaps
Source of Truth: Architecture.md §3.1, §3.2, §13
Tasks: Wire Orchestrator real-runtime; Wire UI live; Wire DI lifetimes; verify E2E happy path.
Output: integration-gap-report.md
```

### Objective
End-to-end flow runs in real desktop session.

### Architecture References

| Section | Title |
|---|---|
| §3.1, §3.2, §13 | Wiring + E2E flow |

### Input Dependencies
5C.

### Deliverables
```
integration-gap-report.md
```

### Anti-Drift Constraints
- No architecture changes; wiring only.

### Verification Checkpoint
- [ ] App launches.
- [ ] Add -> persist -> remind -> display works.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P5D-AC1 | E2E verified on real machine |

### Chat Strategy
N/A.

### Codex Handoff Notes
- Codex zone.

---

## Sub-Phase 5E — Environment Gap Closure

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 5E — Environment Gaps
Source of Truth: HANDOFF.md (Env Gaps from 1E, 1G, 2A, 2F, 3B, 3C, 3E, 4C)
Tasks:
1. NotifyIcon + RegisterHotKey
2. Windows Toast Tier 1
3. NotifyIcon balloon Tier 3
4. GetLastInputInfo + CredUI
5. Add-AppxPackage + rollback
6. Real SMTP + GitHub Issue API
7. MSIX build + signing
Output: env-gap-report.md
```

### Objective
Replace all `NotImplementedException` stubs with live, verified implementations.

### Architecture References

| Section | Title |
|---|---|
| §4.1, §4.4, §4.6, §9.1, §9.2 | Live OS/API integrations |

### Input Dependencies
5D.

### Deliverables
```
env-gap-report.md
```

### Anti-Drift Constraints
- D2 - never rename interfaces; fill bodies only.

### Verification Checkpoint
- [ ] Tray click opens window.
- [ ] Hotkey opens window.
- [ ] Toast fires and is interactive.
- [ ] Idle lock triggers + re-auth required.
- [ ] Updater installs new MSIX.
- [ ] Bug report sends email + opens GitHub Issue.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P5E-AC1 | All env gaps closed |
| P5E-AC2 | Live integration tests pass |

### Chat Strategy
N/A.

### Codex Handoff Notes
- Largest Codex task. Allow generous time.

---

## Sub-Phase 5F — Final Validation Gate

### Mandatory Sub-Roadmap Prompt
```
Sub-Phase: 5F — Final Validation
Source of Truth: All §ACs + §15 perf + §10 compliance
Tasks: full test suite; re-run compliance audit on installed binary; owner sign-off.
Output: final-validation-report.md
```

### Objective
Owner sign-off and release readiness.

### Architecture References

| Section | Title |
|---|---|
| All | (full audit) |

### Input Dependencies
5E.

### Deliverables
```
final-validation-report.md
```

### Anti-Drift Constraints
- Sign-off requires verified evidence per criterion.

### Verification Checkpoint
- [ ] All ACs verified.
- [ ] All §15 perf targets met.
- [ ] All §10 compliance controls verified live.
- [ ] Human owner signs off.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P5F-AC1 | Owner sign-off recorded |
| P5F-AC2 | All previous ACs verified |
| P5F-AC3 | Release artifacts archived |

### Chat Strategy
N/A.

### Codex Handoff Notes
- End of Codex zone.

### Phase 5 Gate
Owner sign-off + all gaps closed.

---

# Governance

## Modification Rules

1. `Architecture.md` and `Roadmap.md` are the **source of truth**. They are append/reorder-only; content is **never deleted**.
2. Every change increments the document patch version (1.0.0 -> 1.0.1) and appends a row in §Document History.
3. Owner approval is required for any change to: locked features (§2), tech stack (§12), compliance controls (§10), or D1-D10 rules.
4. Drift between code and Architecture.md is a defect - code is updated, not Architecture.md, unless §3 Architecture change is owner-approved.
5. `HANDOFF.md` is **live state**; updated every sub-phase but never overwrites Architecture.md / Roadmap.md.

## Agent Handoff Protocol (6 Steps)

1. **Read** the current `Architecture.md` + `Roadmap.md` end-to-end.
2. **Read** the current `HANDOFF.md` to determine state.
3. **Identify** the Next Action Block in `HANDOFF.md`.
4. **Verify** all input dependencies for the next sub-phase are complete.
5. **Execute** the sub-phase under the HALT protocol and D1-D10. Emit the Sub-Roadmap before code.
6. **Update** `HANDOFF.md` §Files Produced, §Gap Summary, §Next Action Block, and visual phase tracker.

## Document History

| Version | Date | Author | Changes |
|---|---|---|---|
| 1.0.0 | 2026-05-26 | Dominic Sarria-Wiley | Initial Roadmap.md (6 phases, 33 sub-phases) |

## Closing Drift-Prevention Statement

> Every line of TaskTree code is a contract with `Architecture.md`. Every phase is a contract with `Roadmap.md`. Every handoff is a contract with `HANDOFF.md`. Agents may fill in implementations; they may not invent architecture. When in doubt, HALT and ask the owner - never guess, never default, never drift. This is how a solo builder ships a HIPAA-grade app without losing control.
