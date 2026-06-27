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

### Deliverables
- TaskTree.sln + 11 csproj
- 11 interfaces under TaskTree.Core/Abstractions/
- 5 models under TaskTree.Core/Models/
- 5 enums under TaskTree.Core/Enums/
- AesGcmCryptoProvider + HashChain under TaskTree.Core/Security/
- FileAppLogger under TaskTree.Core/Logging/
- 8 MSTest test project skeletons + 5 primitive tests

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P0-AC1 | Solution builds 0 errors |
| P0-AC2 | All interfaces match §4 |
| P0-AC3 | AES-256-GCM round-trip works |
| P0-AC4 | Hash chain integrity verified |
| P0-AC5 | Logger writes valid JSON |

### Chat Strategy
6 sequential messages: solution+csproj, interfaces, models, enums, security+logging primitives, test skeletons.

### Phase 0 Gate
All checkpoints + ACs pass.

---

# Phase 1 — Core MVP

**Goal:** TaskTree functions end-to-end at minimum: add a task, persist encrypted, schedule + fire reminders, surface via tray (stubbed).

## Sub-Phase 1A — TaskEngine

**Objective:** Implement hierarchical CRUD with priority (1-5) and deadlines; persist via ISecureStore; raise events.

### Deliverables
- src/TaskTree.Modules.TaskEngine/TaskEngine.cs
- tests/TaskTree.Modules.TaskEngine.Tests/TaskEngineTests.cs

### Anti-Drift Constraints
- Class name `TaskEngine` only.
- Method signatures match ITaskEngine exactly.
- Use `IClock` — never `DateTime.Now`.
- Persist after every mutation.

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

## Sub-Phase 1B — SecureStore

**Objective:** Encrypted local persistence; DPAPI-wrapped master key; AES-256-GCM JSON storage.

### Deliverables
- src/TaskTree.Modules.SecureStore/SecureStore.cs
- src/TaskTree.Modules.SecureStore/MasterKeyManager.cs
- tests/TaskTree.Modules.SecureStore.Tests/*

### Anti-Drift Constraints
- Use only `System.Security.Cryptography.AesGcm` + `ProtectedData`.
- Tag verified on every read; mismatch throws `CryptographicException`.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P1B-AC1 | LoadAsync returns null for missing key |
| P1B-AC2 | SaveAsync -> LoadAsync returns equal payload |
| P1B-AC3 | Tampered ciphertext -> Load throws |
| P1B-AC4 | Master key persists across restarts |
| P1B-AC5 | DeleteAsync removes data + tag |

### Codex Handoff Notes
- DPAPI live tests marked `[TestCategory("Live")]`, run in Phase 5E.

## Sub-Phase 1C — ComplianceCore (Baseline)

**Objective:** Hash-chained audit log + PHI redactor; idle monitor stubbed for 2F.

### Deliverables
- src/TaskTree.Modules.ComplianceCore/ComplianceCore.cs
- src/TaskTree.Modules.ComplianceCore/PhiRedactor.cs
- src/TaskTree.Modules.ComplianceCore/AuditChainWriter.cs
- tests/TaskTree.Modules.ComplianceCore.Tests/*

### Anti-Drift Constraints
- Hash: `SHA256(prevHash || canonicalJson(entryWithoutHash))`.
- Synthetic test inputs only (D6).
- `StartIdleMonitor` throws `NotImplementedException("Deferred to 2F")`.

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

## Sub-Phase 1D — ReminderScheduler

**Objective:** Periodic 30s tick evaluates tree; raises `ReminderDue` per §5.3 cadence.

### Deliverables
- src/TaskTree.Modules.ReminderScheduler/ReminderScheduler.cs
- src/TaskTree.Modules.ReminderScheduler/CadencePolicy.cs
- tests/TaskTree.Modules.ReminderScheduler.Tests/*

### Anti-Drift Constraints
- Cadence values match §5.3 exactly.
- `System.Threading.PeriodicTimer` only.
- Time source = injected `IClock`.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P1D-AC1 | StartAsync ticks; StopAsync halts |
| P1D-AC2 | Cadence respects §5.3 for all priorities |
| P1D-AC3 | ReminderDue includes node + reason |
| P1D-AC4 | Snooze/escalation NOT implemented (deferred to 2G) |

## Sub-Phase 1E — TrayHost (Stub)

**Objective:** Stub TrayHost that compiles, exposes correct event surface, marks Win32 work for Codex 5E.

### Deliverables
- src/TaskTree.Modules.TrayHost/TrayHost.cs (HIGH-stub)
- src/TaskTree.Modules.TrayHost/HotkeyInterop.cs (HIGH-stub)
- tests/TaskTree.Modules.TrayHost.Tests/TrayHostTests.cs

### Anti-Drift Constraints
- `Initialize()` throws `NotImplementedException("HIGH: NotifyIcon + RegisterHotKey require live env - Codex Phase 5E")`.
- Events declared and manually raisable.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P1E-AC1 | TrayHost compiles |
| P1E-AC2 | Events raise correctly via reflection test |
| P1E-AC3 | Live methods stubbed per D5 |

## Sub-Phase 1F — Orchestrator Wiring

**Objective:** DI composition root; subscribe TrayHost -> TaskEngine -> ReminderScheduler; placeholder delivery until 1G.

### Deliverables
- src/TaskTree.Orchestrator/Orchestrator.cs
- src/TaskTree.App/Bootstrap/CompositionRoot.cs
- src/TaskTree.App/Bootstrap/ServiceRegistrations.cs
- tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs

### Anti-Drift Constraints
- Constructor injection only.
- DI lifetimes per Architecture (singletons for stateful modules).

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P1F-AC1 | Container builds |
| P1F-AC2 | Simulated E2E flow succeeds |
| P1F-AC3 | Audit chain receives event entries |

## Sub-Phase 1G — Reminder Delivery Tier Chain

**Objective:** Decision logic chooses Tier 1/2/3 based on Toast API availability and Focus Assist state.

### Deliverables
- src/TaskTree.Orchestrator/ReminderDeliveryService.cs
- src/TaskTree.Orchestrator/ToastTier1Adapter.cs (HIGH-stub)
- src/TaskTree.Orchestrator/ToastTier2Adapter.cs
- src/TaskTree.Orchestrator/ToastTier3Adapter.cs (HIGH-stub)
- tests/TaskTree.Orchestrator.Tests/ReminderDeliveryTests.cs

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P1G-AC1 | Decision tree correct across 4 input combos |
| P1G-AC2 | Tier 2 WPF XAML present |
| P1G-AC3 | Tier 1+3 throw NotImplementedException |

## Sub-Phase 1H — Phase 1 Integration Test Gate

**Objective:** Confirm Phase 1 deliverables work together offline. Data + event spine only.

### Deliverables
- tests/TaskTree.Orchestrator.Tests/EndToEndOfflineTests.cs

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P1H-AC1 | E2E offline tests 100% pass |
| P1H-AC2 | Coverage >= 75% |
| P1H-AC3 | HANDOFF.md updated |

### Phase 1 Gate
All P1A-P1H pass + **human owner approval**.

---

# Phase 2 — Secondary Features

**Goal:** Make TaskTree usable daily — visible UI, hotkeys, settings, auto-logoff, snooze/escalation.

## Sub-Phase 2A — Global Hotkey Manager

### Deliverables
- src/TaskTree.Modules.TrayHost/HotkeyManager.cs (HIGH)
- src/TaskTree.Modules.TrayHost/HotkeyConfig.cs
- tests/TaskTree.Modules.TrayHost.Tests/HotkeyManagerTests.cs

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P2A-AC1 | Default Ctrl+Alt+T configurable |
| P2A-AC2 | Bindings persist across restart |
| P2A-AC3 | Conflicts surface gracefully |

## Sub-Phase 2B — TreeViewUI (Main Window)

### Deliverables
- src/TaskTree.UI/Views/MainWindow.xaml + .cs
- src/TaskTree.UI/ViewModels/MainWindowViewModel.cs
- src/TaskTree.UI/Views/ReminderToast.xaml
- src/TaskTree.UI/ViewModels/ToastViewModel.cs
- tests/TaskTree.UI.Tests/MainWindowViewModelTests.cs

### Anti-Drift Constraints
- MVVM strict: no logic in code-behind.
- `CommunityToolkit.Mvvm` only (D3).

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P2B-AC1 | ViewModel 100% unit-tested |
| P2B-AC2 | Tree shows priority + deadline columns |
| P2B-AC3 | Quick-add appends to TaskEngine |

## Sub-Phase 2C — Drag-Drop Reordering / Reparenting

### Deliverables
- src/TaskTree.UI/Behaviors/DragDropBehavior.cs
- tests/TaskTree.UI.Tests/DragDropBehaviorTests.cs

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P2C-AC1 | Drop reorders siblings |
| P2C-AC2 | Drop reparents to new parent |
| P2C-AC3 | Cycle attempt -> toast warning |

## Sub-Phase 2D — Color-Coded Priority + Theme

### Deliverables
- src/TaskTree.UI/Converters/PriorityColorConverter.cs
- src/TaskTree.App/Resources/Themes/Light.xaml
- src/TaskTree.App/Resources/Themes/Dark.xaml
- tests/TaskTree.UI.Tests/PriorityColorConverterTests.cs

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P2D-AC1 | Converter returns correct brush per priority |
| P2D-AC2 | Theme persists across restart |

## Sub-Phase 2E — Settings Panel

### Deliverables
- src/TaskTree.UI/Views/SettingsView.xaml
- src/TaskTree.UI/ViewModels/SettingsViewModel.cs
- tests/TaskTree.UI.Tests/SettingsViewModelTests.cs

### Anti-Drift Constraints
- Logoff timer 1-60 min.
- Toggles emit audit events.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P2E-AC1 | Settings persist across restart |
| P2E-AC2 | Invalid values rejected |
| P2E-AC3 | Audit entry on toggle changes |

## Sub-Phase 2F — Auto-Logoff + Session Lock

### Deliverables
- src/TaskTree.Modules.ComplianceCore/IdleMonitor.cs
- src/TaskTree.UI/Views/SessionLockView.xaml
- src/TaskTree.UI/ViewModels/SessionLockViewModel.cs
- tests/TaskTree.Modules.ComplianceCore.Tests/IdleMonitorTests.cs

### Anti-Drift Constraints
- Re-auth cannot be skipped.
- Audit on lock + unlock.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P2F-AC1 | 15-min default fires correctly (mocked) |
| P2F-AC2 | Lock blocks other interaction |
| P2F-AC3 | Unlock writes audit entry |

### Codex Handoff Notes
- Phase 5E: real `GetLastInputInfo` + CredUI verification.

## Sub-Phase 2G — Snooze + Escalation

### Deliverables
- src/TaskTree.Modules.ReminderScheduler/SnoozeService.cs
- src/TaskTree.Modules.ReminderScheduler/EscalationPolicy.cs

### Anti-Drift Constraints
- Snooze max 1 hour.
- Escalation thresholds match §5.3.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P2G-AC1 | Snooze options 5/15/30/60 min |
| P2G-AC2 | Escalation flips persistence flag |
| P2G-AC3 | Audit entry on snooze + escalation |

### Phase 2 Gate
All P2A-P2G pass.

---

# Phase 3 — Extended Integration

**Goal:** Ship-ready secondary features — updater and bug reporter.

## Sub-Phase 3A — AutoUpdater Core (Manifest + Verify)

### Deliverables
- src/TaskTree.Modules.AutoUpdater/AutoUpdater.cs
- src/TaskTree.Modules.AutoUpdater/ManifestSigner.cs
- src/TaskTree.Modules.AutoUpdater/HashVerifier.cs
- tests/TaskTree.Modules.AutoUpdater.Tests/*

### Anti-Drift Constraints
- NSec.Cryptography only for Ed25519.
- Public key embedded as compile-time constant.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P3A-AC1 | Manifest parses per §9.1.2 |
| P3A-AC2 | Signature verification rejects tampering |
| P3A-AC3 | Hash verification rejects mismatch |

## Sub-Phase 3B — AutoUpdater State Machine + Staging

### Deliverables
- src/TaskTree.Modules.AutoUpdater/UpdaterStateMachine.cs
- src/TaskTree.Modules.AutoUpdater/StagingService.cs

### Anti-Drift Constraints
- Transitions match §9.1.1 diagram.
- `ApplyAsync` throws `NotImplementedException("HIGH: Add-AppxPackage - Codex 5E")`.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P3B-AC1 | State machine matches §9.1.1 |
| P3B-AC2 | Staged file present after DOWNLOADING |
| P3B-AC3 | ApplyAsync stub throws clear reason |

## Sub-Phase 3C — Offline Import + Rollback Sentinel

### Deliverables
- src/TaskTree.Modules.AutoUpdater/OfflineImportService.cs
- src/TaskTree.Modules.AutoUpdater/SentinelService.cs
- src/TaskTree.Modules.AutoUpdater/RollbackService.cs

### Anti-Drift Constraints
- Local import uses same verify pipeline as remote (T7).
- Sentinel = `%LOCALAPPDATA%\TaskTree\sentinel.lock`.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P3C-AC1 | Local import enforces sig + hash |
| P3C-AC2 | Sentinel triggers rollback on crash |
| P3C-AC3 | Rollback restores prior version |

## Sub-Phase 3D — BugReporter Capture + Queue + Redaction

### Deliverables
- src/TaskTree.Modules.BugReporter/BugReporter.cs
- src/TaskTree.Modules.BugReporter/CrashCaptureHook.cs
- src/TaskTree.Modules.BugReporter/BugReportQueue.cs
- src/TaskTree.Modules.BugReporter/RedactionPipeline.cs

### Anti-Drift Constraints
- Never store unredacted payload.
- Fingerprint = SHA-256(stack || title).

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P3D-AC1 | Crash captured < 200 ms |
| P3D-AC2 | All 5 PHI patterns redacted |
| P3D-AC3 | Dedup by fingerprint works |

## Sub-Phase 3E — BugReporter Delivery (Email + GitHub)

### Deliverables
- src/TaskTree.Modules.BugReporter/EmailDeliveryAdapter.cs (HIGH-stub)
- src/TaskTree.Modules.BugReporter/GitHubIssueAdapter.cs (HIGH-stub)
- src/TaskTree.Modules.BugReporter/FileDropAdapter.cs
- src/TaskTree.Modules.BugReporter/DeliveryRouter.cs

### Anti-Drift Constraints
- Credentials from DPAPI-wrapped config only.
- Rate limit 5/min, 50/day.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P3E-AC1 | Severity 1 -> email + GitHub |
| P3E-AC2 | Severity 5 -> file drop only |
| P3E-AC3 | Rate limit enforced |

## Sub-Phase 3F — Phase 3 Integration Test Gate

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P3F-AC1 | Phase 3 offline tests 100% pass |
| P3F-AC2 | HANDOFF.md updated |

### Phase 3 Gate
All P3A-P3F pass + **human owner approval**.

---

# Phase 4 — Hardening & Release

**Goal:** Ship-quality build.

## Sub-Phase 4A — Compliance Audit

### Deliverables
- docs/compliance-audit.md
- tests/TaskTree.Modules.ComplianceCore.Tests/AuditChainStressTests.cs

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P4A-AC1 | All §10 controls mapped |
| P4A-AC2 | Stress tests pass (10k + 100k entries) |
| P4A-AC3 | No PHI leaks in code review |

## Sub-Phase 4B — Performance Optimization

### Deliverables
- tests/TaskTree.Perf.Tests/PerfBenchmarks.cs
- docs/perf-report.md

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P4B-AC1 | All measurable targets pass |
| P4B-AC2 | Misses logged with mitigation plan |

## Sub-Phase 4C — Packaging (MSIX)

### Deliverables
- packaging/TaskTree.Installer.wapproj
- packaging/Package.appxmanifest
- packaging/build-msix.ps1
- docs/signing-checklist.md

### Anti-Drift Constraints
- Self-contained .NET 8 publish.
- Authenticode signing required.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P4C-AC1 | MSIX produced by build-msix.ps1 in Codex |
| P4C-AC2 | Signed binary installs cleanly |
| P4C-AC3 | First launch succeeds |

## Sub-Phase 4D — Documentation & Deployment

### Deliverables
- README.md
- docs/user-guide.md
- docs/ops-runbook.md
- docs/release-checklist.md

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P4D-AC1 | All four docs present |
| P4D-AC2 | Cross-links accurate |
| P4D-AC3 | Release checklist signed off |

### Phase 4 Gate
All P4A-P4D pass + **human owner approval**.

---

# Phase 5 — Handoff & Gap Closure (Codex / Claude Code)

> Runs **entirely outside the chat environment.** Codex / Claude Code take over with the full repo, live OS, live network, and a Windows desktop.

## Sub-Phase 5A — Repo Stitching

Tasks: run assemble-repo.ps1; verify file count vs §3.3; run verify-namespaces.ps1; resolve dupes; dotnet restore.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P5A-AC1 | Repo assembles 0 errors |
| P5A-AC2 | All §3.3 files present |

## Sub-Phase 5B — Compile Gap Closure

Tasks: dotnet build; address every error per D1-D10; update CHANGELOG.md.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P5B-AC1 | Build succeeds Release config |

## Sub-Phase 5C — Test Gap Closure

Tasks: `dotnet test --filter Category!=Live`; 100% pass; coverage >= 75%.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P5C-AC1 | Offline suite 100% green |
| P5C-AC2 | Coverage targets met |

## Sub-Phase 5D — Integration Gap Closure

Tasks: Wire Orchestrator real-runtime; Wire UI live; Wire DI lifetimes; verify E2E happy path.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P5D-AC1 | E2E verified on real machine |

## Sub-Phase 5E — Environment Gap Closure

Tasks:
1. NotifyIcon + RegisterHotKey
2. Windows Toast Tier 1
3. NotifyIcon balloon Tier 3
4. GetLastInputInfo + CredUI
5. Add-AppxPackage + rollback
6. Real SMTP + GitHub Issue API
7. MSIX build + signing

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P5E-AC1 | All env gaps closed |
| P5E-AC2 | Live integration tests pass |

## Sub-Phase 5F — Final Validation Gate

Tasks: full test suite; re-run compliance audit on installed binary; owner sign-off.

### Acceptance Criteria

| ID | Criterion |
|---|---|
| P5F-AC1 | Owner sign-off recorded |
| P5F-AC2 | All previous ACs verified |
| P5F-AC3 | Release artifacts archived |

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
| 1.0.0 | 2026-05-26 | Dominic Sarria-Wiley | Initial Roadmap.md (6 phases, 32 sub-phases) |

## Closing Drift-Prevention Statement

> Every line of TaskTree code is a contract with `Architecture.md`. Every phase is a contract with `Roadmap.md`. Every handoff is a contract with `HANDOFF.md`. Agents may fill in implementations; they may not invent architecture. When in doubt, HALT and ask the owner — never guess, never default, never drift. This is how a solo builder ships a HIPAA-grade app without losing control.
