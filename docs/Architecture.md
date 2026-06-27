# TaskTree — Architecture.md

> **Source of Truth Document — v1.0.1**
> Owner: Dominic Sarria-Wiley
> Classification: Public
> Last Updated: 2026-05-26 (amended Phase 0 Msg 6 — added TrayHost.Tests + UI.Tests to §3.3)
> Companion Documents: `Roadmap.md`, `HANDOFF.md`

---

## Section 0 — Disaster Recovery / Rebuild Preamble

> **This document + `Roadmap.md` = full project rebuild.**
> If the codebase is lost, these two files contain everything needed to regenerate TaskTree from scratch using a chat-only LLM environment (Claude Opus) plus a final gap-closure pass in Codex / Claude Code.

### Rebuild Priority Order
1. **Read `Architecture.md` end-to-end.** Lock in module names, namespaces, file paths (§3.3), and interface contracts (§4).
2. **Read `Roadmap.md` Phase 0–5.** Execute phases in strict dependency order; no skipping.
3. **Regenerate Phase 0 scaffold first** (solution, interfaces, models, enums, security & logging primitives).
4. **Regenerate modules in dependency order** per §3.2 module graph.
5. **Hand off to Codex/Claude Code** for environment-dependent gaps (tray hooks, global hotkeys, MSIX signing, live updater HTTP).

### Metadata

| Field | Value |
|---|---|
| Owner | Dominic Sarria-Wiley |
| Classification | Public |
| Compliance Framework | HIPAA |
| Sensitive Data | Possible PHI |
| Document Version | 1.0.1 |

---

## Section 1 — Project Overview

### 1.1 What is TaskTree?

TaskTree is a lightweight, resource-efficient Windows desktop application that runs in the background and lives in the system tray. It replaces paper sticky notes and ad-hoc reminders with a structured, prioritized, deadline-aware task tree. Users add tasks via tray click or hotkeys, assign priorities (1–5) and deadlines, and receive periodic toast reminders until tasks are completed or dismissed. A maximizable GUI exposes the full tree for review, drag-drop reorganization, and bulk operations.

TaskTree is HIPAA-aware: all task content is treated as potential PHI and stored encrypted at rest (AES-256-GCM), with hash-chained audit logging, configurable auto-logoff, and a PHI-safe bug reporter.

### 1.2 Who Is It For?

| Audience | Primary Need |
|---|---|
| Healthcare workers (nurses, pharmacists, techs) | PHI-safe task tracking without sticky notes |
| Clinicians | Prioritized reminders during patient care |
| Knowledge workers | Sticky-note replacement with deadlines |
| Anyone managing multi-task workloads | Lightweight, distraction-free reminders |

### 1.3 Target Platforms

- Windows 10 (build 1809+)
- Windows 11 (all builds)
- x64 + ARM64

### 1.4 Design Philosophy — Four Pillars

1. **Quiet by Default** — lives in tray, surfaces only when needed; never steals focus aggressively.
2. **Secure by Default** — assume PHI; encrypt everything; no plaintext at rest, ever.
3. **Deterministic by Design** — predictable file paths, namespaces, behavior; chat-buildable end-to-end.
4. **Simple, Clean, Functional UI** — modern minimal aesthetic, zero noise, instant comprehension.

---

## Section 2 — Locked-In Features

| # | Feature | Status | Phase |
|---|---|---|---|
| F1 | System tray icon with context menu | LOCKED | 1 |
| F2 | Add/edit/delete tasks with priority 1–5 | LOCKED | 1 |
| F3 | Hierarchical task tree (parent/child) | LOCKED | 1 |
| F4 | Deadlines with date + time | LOCKED | 1 |
| F5 | Periodic toast reminders (configurable interval) | LOCKED | 1 |
| F6 | Global hotkeys (configurable) | LOCKED | 2 |
| F7 | Maximized tree GUI with drag-drop | LOCKED | 2 |
| F8 | AES-256-GCM encrypted local persistence | LOCKED | 1 |
| F9 | Auto-reload encrypted store on app restart | LOCKED | 1 |
| F10 | Hash-chained audit logging | LOCKED | 1 |
| F11 | Auto-logoff (configurable, default 15 min) | LOCKED | 2 |
| F12 | Snooze + escalation on reminders | LOCKED | 2 |
| F13 | Color-coded priority indicators | LOCKED | 2 |
| F14 | Auto-updater (toggle, signed, offline import) | LOCKED | 3 |
| F15 | Bug reporter (email + GitHub Issues, redacted) | LOCKED | 3 |
| F16 | Settings panel (intervals, hotkeys, logoff timer) | LOCKED | 2 |

---

## Section 3 — System Architecture Overview

### 3.1 High-Level Diagram

```
┌──────────────────────────────────────────────┐
│              USER INTERACTIONS               │
│  (tray click / hotkey / scheduled tick)      │
└──────────┬───────────────────────────────────┘
           │
┌──────────▼──────────────────────┐
│          TrayHost               │
│  (NotifyIcon + Hotkey Hook)     │
└──────────┬──────────────────────┘
           │
┌──────────▼──────────────────────┐
│         Orchestrator            │
│  (Coordinates all modules)      │
└─┬──────────┬──────────┬─────────┘
  │          │          │
┌─▼──────────┐  ┌──▼─────┐  ┌─▼─────────────┐
│  TaskEngine│  │Reminder│  │ TreeViewUI    │
│  (CRUD/Tree)  │Scheduler  │ (WPF Window)  │
└─────────┬──┘  └────┬───┘  └────────┬──────┘
          │         │                │
          │     ┌───▼────────────────▼────┐
          │     │     ComplianceCore       │
          │     │  (Audit / Logoff / Lock) │
          │     └────────────┬─────────────┘
          │                  │
          └──────────────────▼──┐
                                │
┌───────────────────────────────▼─┐
│         SecureStore             │
│  (AES-256-GCM, JSON-on-disk)    │
└─────────────────────────────────┘

┌──────────────────┐                ┌──────────────────┐
│  AutoUpdater     │                │  BugReporter     │
│ (poll + verify)  │                │ (crash + form)   │
└──────────────────┘                └──────────────────┘
```

### 3.2 Module Dependency Graph

```
TrayHost ──▶ Orchestrator ──▶ TaskEngine ──▶ SecureStore
                       ├──▶ ReminderScheduler ──▶ TaskEngine
                       ├──▶ TreeViewUI ──▶ TaskEngine
                       ├──▶ ComplianceCore ──▶ SecureStore
                       ├──▶ AutoUpdater
                       └──▶ BugReporter ──▶ ComplianceCore (redaction)

Cross-cutting: ILogger, IClock, ICryptoProvider (Phase 0 primitives)
```

### 3.3 Folder Structure (Declared Upfront — Deterministic)

```
TaskTree/
├── TaskTree.sln
├── src/
│   ├── TaskTree.App/                       # WPF host + DI composition
│   │   ├── App.xaml
│   │   ├── App.xaml.cs
│   │   ├── Bootstrap/
│   │   │   ├── CompositionRoot.cs
│   │   │   └── ServiceRegistrations.cs
│   │   ├── Resources/
│   │   │   ├── Icons/
│   │   │   └── Themes/
│   │   └── TaskTree.App.csproj
│   │
│   ├── TaskTree.Core/                      # Interfaces, models, enums, primitives
│   │   ├── Abstractions/
│   │   │   ├── ITaskEngine.cs
│   │   │   ├── IReminderScheduler.cs
│   │   │   ├── ISecureStore.cs
│   │   │   ├── IComplianceCore.cs
│   │   │   ├── IAutoUpdater.cs
│   │   │   ├── IBugReporter.cs
│   │   │   ├── ITrayHost.cs
│   │   │   ├── IOrchestrator.cs
│   │   │   ├── ICryptoProvider.cs
│   │   │   ├── IClock.cs
│   │   │   └── IAppLogger.cs
│   │   ├── Models/
│   │   │   ├── TaskNode.cs
│   │   │   ├── ReminderEvent.cs
│   │   │   ├── AuditEntry.cs
│   │   │   ├── UpdateManifest.cs
│   │   │   └── BugReport.cs
│   │   ├── Enums/
│   │   │   ├── Priority.cs
│   │   │   ├── TaskStatus.cs
│   │   │   ├── ReminderCadence.cs
│   │   │   ├── UpdateChannel.cs
│   │   │   └── BugSeverity.cs
│   │   ├── Security/
│   │   │   ├── AesGcmCryptoProvider.cs
│   │   │   └── HashChain.cs
│   │   ├── Logging/
│   │   │   └── FileAppLogger.cs
│   │   └── TaskTree.Core.csproj
│   │
│   ├── TaskTree.Modules.TaskEngine/
│   ├── TaskTree.Modules.ReminderScheduler/
│   ├── TaskTree.Modules.SecureStore/
│   ├── TaskTree.Modules.ComplianceCore/
│   ├── TaskTree.Modules.TrayHost/          # NotifyIcon + Hotkey PInvoke
│   ├── TaskTree.Modules.AutoUpdater/
│   ├── TaskTree.Modules.BugReporter/
│   ├── TaskTree.UI/                        # WPF views, viewmodels
│   │   ├── Views/
│   │   │   ├── MainWindow.xaml
│   │   │   ├── SettingsView.xaml
│   │   │   └── ReminderToast.xaml
│   │   ├── ViewModels/
│   │   └── TaskTree.UI.csproj
│   └── TaskTree.Orchestrator/
│
├── tests/
│   ├── TaskTree.Core.Tests/
│   ├── TaskTree.Modules.TaskEngine.Tests/
│   ├── TaskTree.Modules.ReminderScheduler.Tests/
│   ├── TaskTree.Modules.SecureStore.Tests/
│   ├── TaskTree.Modules.ComplianceCore.Tests/
│   ├── TaskTree.Modules.AutoUpdater.Tests/
│   ├── TaskTree.Modules.BugReporter.Tests/
│   ├── TaskTree.Orchestrator.Tests/
│   ├── TaskTree.Modules.TrayHost.Tests/    # added v1.0.1
│   └── TaskTree.UI.Tests/                   # added v1.0.1
│
├── docs/
│   ├── Architecture.md
│   ├── Roadmap.md
│   └── HANDOFF.md
│
├── tools/
│   ├── assemble-repo.ps1
│   └── verify-namespaces.ps1
│
├── packaging/
│   ├── TaskTree.Installer.wapproj          # MSIX
│   └── manifest/
│       └── update.manifest.json
│
└── README.md
```

**Namespace convention:** `TaskTree.<Layer>.<Module>` (e.g., `TaskTree.Modules.TaskEngine`).

---

## Section 4 — Module Specifications

### 4.1 TrayHost

| Property | Value |
|---|---|
| Purpose | System tray icon, context menu, global hotkey registration |
| Trigger | App startup |
| Technology | `NotifyIcon` (WPF-NotifyIcon NuGet) + `RegisterHotKey` PInvoke |
| Chat Complexity | **HIGH** (PInvoke + tray behavior requires live verification) |

**Input/Output:**
- Input: User clicks, hotkey presses
- Output: Events raised to Orchestrator (`ShowTreeRequested`, `AddTaskRequested`, `ExitRequested`)

**Stub Example (C#):**
```csharp
public interface ITrayHost
{
    event EventHandler ShowTreeRequested;
    event EventHandler AddTaskRequested;
    event EventHandler ExitRequested;
    void Initialize();
    void ShowBalloon(string title, string message);
    void Dispose();
}
```

**Performance Target:** < 50ms tray click → event raised
**Handoff Notes:** Stub `Initialize()` and hotkey registration with `NotImplementedException("HIGH: PInvoke + NotifyIcon requires live verification — Codex Phase 5E")`. Models and event signatures fully chat-buildable.

### 4.2 TaskEngine

| Property | Value |
|---|---|
| Purpose | CRUD operations on hierarchical task tree, priority and deadline management |
| Trigger | Orchestrator calls |
| Technology | Pure C# .NET 8 (in-memory tree + persistence via ISecureStore) |
| Chat Complexity | **LOW** |

**Stub Example:**
```csharp
public interface ITaskEngine
{
    Task<TaskNode> AddAsync(TaskNode node, Guid? parentId = null);
    Task<TaskNode> UpdateAsync(TaskNode node);
    Task DeleteAsync(Guid id);
    Task<IReadOnlyList<TaskNode>> GetTreeAsync();
    Task<IReadOnlyList<TaskNode>> GetOverdueAsync(DateTimeOffset now);
    event EventHandler<TaskNode> TaskAdded;
    event EventHandler<TaskNode> TaskCompleted;
}
```

**Performance Target:** CRUD < 50ms; full tree fetch (≤1000 nodes) < 100ms

### 4.3 ReminderScheduler

| Property | Value |
|---|---|
| Purpose | Periodic ticks → evaluate due/overdue tasks → fire reminder events |
| Trigger | Internal timer (System.Threading.PeriodicTimer) |
| Technology | C# .NET 8 PeriodicTimer + IClock |
| Chat Complexity | **LOW** |

**Stub Example:**
```csharp
public interface IReminderScheduler
{
    Task StartAsync(CancellationToken ct);
    Task StopAsync();
    event EventHandler<ReminderEvent> ReminderDue;
    TimeSpan Cadence { get; set; }
}
```

**Performance Target:** Tick evaluation < 10ms for ≤1000 tasks

### 4.4 TreeViewUI

| Property | Value |
|---|---|
| Purpose | Maximized GUI: tree view, drag-drop, color-coded priority, toast popups |
| Trigger | TrayHost `ShowTreeRequested` event |
| Technology | WPF + ModernWpfUI; MVVM with CommunityToolkit.Mvvm |
| Chat Complexity | **MEDIUM** |

**Performance Target:** Window show < 200ms warm

### 4.5 SecureStore

| Property | Value |
|---|---|
| Purpose | Encrypted persistence of task tree, settings, audit log |
| Trigger | TaskEngine + ComplianceCore writes |
| Technology | AES-256-GCM (System.Security.Cryptography), JSON serialization, DPAPI-wrapped master key |
| Chat Complexity | **LOW** |

**Stub Example:**
```csharp
public interface ISecureStore
{
    Task<T?> LoadAsync<T>(string key) where T : class;
    Task SaveAsync<T>(string key, T value) where T : class;
    Task<bool> ExistsAsync(string key);
    Task DeleteAsync(string key);
}
```

**Performance Target:** Read/write < 100ms for stores ≤ 10 MB

### 4.6 ComplianceCore

| Property | Value |
|---|---|
| Purpose | HIPAA controls: hash-chained audit log, auto-logoff, session lock, PHI redaction helpers |
| Trigger | All modules emit audit events |
| Technology | SHA-256 hash chain, idle-detection timer |
| Chat Complexity | **MEDIUM** (idle detection requires Win32 `GetLastInputInfo`) |

**Stub Example:**
```csharp
public interface IComplianceCore
{
    Task AuditAsync(AuditEntry entry);
    Task<IReadOnlyList<AuditEntry>> GetAuditChainAsync();
    Task<bool> VerifyChainIntegrityAsync();
    event EventHandler AutoLogoffTriggered;
    void StartIdleMonitor(TimeSpan timeout);
    string RedactPhi(string text);
}
```

**Performance Target:** Audit write < 20ms; chain verify < 500ms for 10k entries

### 4.7 AutoUpdater

| Property | Value |
|---|---|
| Purpose | Poll manifest, verify signature + hash, download + stage update, support offline manual import |
| Trigger | Timer (24h default) or manual user action |
| Technology | HttpClient, Ed25519 signature verification, SHA-256 hash |
| Chat Complexity | **MEDIUM** |

**Stub Example:**
```csharp
public interface IAutoUpdater
{
    Task<UpdateManifest?> CheckAsync();
    Task<bool> VerifyAsync(UpdateManifest manifest, byte[] payload);
    Task ApplyAsync(UpdateManifest manifest);
    Task<UpdateManifest> ImportLocalAsync(string filePath);
    UpdateChannel Channel { get; set; }
    bool Enabled { get; set; }
}
```

**Performance Target:** Manifest check < 2s on network; verify < 500ms

### 4.8 BugReporter

| Property | Value |
|---|---|
| Purpose | Capture crashes + user-submitted reports; redact PHI; send to email + GitHub Issues |
| Trigger | `AppDomain.UnhandledException`, user form submission |
| Technology | SMTP client (System.Net.Mail), GitHub REST API, local queue |
| Chat Complexity | **MEDIUM** |

**Stub Example:**
```csharp
public interface IBugReporter
{
    Task<Guid> SubmitAsync(BugReport report);
    Task<int> FlushQueueAsync();
    void HookGlobalCrashHandler();
    bool RedactionEnabled { get; set; }
}
```

**Performance Target:** Submit (queued) < 50ms; flush < 5s per report

---

## Section 5 — Domain-Specific Architecture

### 5.1 Core Concept — Priority-Weighted Reminder Cadence

TaskTree's reminder system is **priority-weighted**: higher-priority tasks fire reminders more frequently and escalate visually. This is the system's domain differentiator vs. a generic to-do app.

### 5.2 Why Superior

- Paper sticky notes have **no escalation** — they sit until torn down.
- Generic to-do apps fire one reminder and stop.
- TaskTree's cadence ladder ensures the most urgent task gets the most attention without spam for low-priority items.

### 5.3 Cadence Timing Table

| Priority | Initial Reminder | Repeat Cadence | Escalation After |
|---|---|---|---|
| 1 (Critical) | On creation | Every 5 min | Toast + audible chime after 15 min overdue |
| 2 (High) | 30 min before deadline | Every 15 min | Toast persistent after 30 min overdue |
| 3 (Normal) | 1 hour before deadline | Every 30 min | Toast after 1 hour overdue |
| 4 (Low) | 4 hours before deadline | Every 2 hours | Toast after 4 hours overdue |
| 5 (Trivial) | At deadline | Every 8 hours | Silent badge only |

### 5.4 Timing Diagram

```
T-deadline    T-30m    T-0    T+5m    T+15m    T+30m
│          │        │       │        │        │
P=1: │          │        ▼─────▶▼──────▶▼ESCALATE▶│
P=2: │          ▼────────▼──────▶▼─────▶▼─ESCAL──▶│
P=3: ▼──────────▼────────▼──────────────────────▶│
```

---

## Section 6 — Provider / Options Matrix

**TaskTree has NO external API integrations.** This section is preserved for future extensibility.

| Option | Status | Rationale |
|---|---|---|
| Local-only (default) | ACTIVE | HIPAA-safe; no PHI leaves device |
| Cloud sync (future) | DEFERRED | Would require BAA + extensive design |
| MDM-managed deployment | DEFERRED | Enterprise feature, Phase 6+ |

---

## Section 7 — Core Engine (3-Tier Reminder Fallback Chain)

### Tier 1 — Preferred: Windows Toast Notification
- Modern, native Action Center integration
- Survives focus loss
- Respects Focus Assist

### Tier 2 — Fallback: WPF Custom Toast Window
- Used if Toast API fails or Focus Assist suppresses
- Always-on-top, click-through-aware

### Tier 3 — Universal Fallback: Tray Balloon + Icon Flash
- NotifyIcon balloon with priority color
- Icon flash via `NIIF_NOSOUND`

### Decision Flow

```
┌─────────────────────┐
│ ReminderDue fired   │
└──────────┬──────────┘
           ▼
┌─────────────────────┐
│ Toast API available?│──no──▶ Tier 2
└──────────┬──────────┘
       yes
           ▼
┌─────────────────────┐
│ Focus Assist off?   │──no──▶ Tier 2
└──────────┬──────────┘
       yes
           ▼
        Tier 1
```

---

## Section 8 — External Integration Cascade (Default, Unused)

| Tier | Method | TaskTree v1.0 Status |
|---|---|---|
| 1 — Native API | OS-native APIs | N/A |
| 2 — Power User API | App-specific SDKs | N/A |
| 3 — Companion / Extension | Browser extension or plugin | N/A |
| 4 — Universal Fallback | OCR / accessibility tree | N/A |

---

## Section 9 — Auto-Updater + Bug Reporting (LOCKED IN)

### 9.1 Auto-Updater — Chat-Friendly Design

**Properties:**
- Manifest-based (JSON, static-hosted)
- Polling-based (24h default, configurable)
- SHA-256 hash + Ed25519 signature verification
- Remote source + local file import (offline)
- Channels: stable, beta
- Staged rollout via percentage rings (10% → 50% → 100%)
- Rollback via previous MSIX retained on disk

#### 9.1.1 Updater State Machine

```
        ┌──────────┐
   ┌───▶│   IDLE   │◀────────────┐
   │    └────┬─────┘              │
   │         │ poll timer / manual│
   │         ▼                    │
   │    ┌──────────┐              │
   │    │ CHECKING │              │
   │    └────┬─────┘              │
   │         │                    │
   │   no update                  │
   │         │                    │
   │         ├────────────────────┘
   │   update found
   │         ▼
   │    ┌──────────────┐
   │    │ DOWNLOADING  │
   │    └────┬─────────┘
   │         ▼
   │    ┌──────────────┐
   │    │  VERIFYING   │──fail──▶ ┌──────────┐
   │    └────┬─────────┘          │  FAILED  │──▶ rollback
   │      pass                    └──────────┘
   │         ▼
   │    ┌──────────────┐
   │    │   STAGING    │
   │    └────┬─────────┘
   │         ▼
   │    ┌──────────────┐
   │    │   APPLYING   │
   │    └────┬─────────┘
   │         ▼
   │    ┌──────────────┐
   └────│   APPLIED    │
        └──────────────┘
```

#### 9.1.2 Update Manifest Schema

```json
{
  "version": "1.0.1",
  "channel": "stable",
  "released": "2026-06-01T00:00:00Z",
  "minPreviousVersion": "1.0.0",
  "rolloutPercent": 100,
  "package": {
    "url": "https://updates.example.com/tasktree/1.0.1.msix",
    "sha256": "{HEX_64}",
    "sizeBytes": 12345678
  },
  "signature": {
    "alg": "Ed25519",
    "publicKeyId": "tasktree-stable-2026",
    "value": "{BASE64}"
  },
  "notes": "Bug fixes and reminder cadence improvements."
}
```

#### 9.1.3 Signature & Integrity Model
- Manifest signed with Ed25519; public key embedded in app at build time.
- Package SHA-256 computed and compared to manifest entry.
- Authenticode signature on MSIX verified by OS at install time.

#### 9.1.4 Update Flow
1. Timer fires → fetch manifest from configured URL.
2. Parse manifest; verify Ed25519 signature on manifest body.
3. Check `version > current && rolloutPercent covers this install`.
4. Download package; compute SHA-256; compare to manifest.
5. Stage in `%LOCALAPPDATA%\TaskTree\updates\`.
6. Prompt user; on accept, invoke `Add-AppxPackage`.
7. Audit log entry written; previous version retained for 7 days.

#### 9.1.5 Rollback Strategy
- Last-known-good MSIX retained on disk.
- User-triggered rollback via Settings → "Roll back to previous version".
- Automatic rollback if new version crashes on first launch (sentinel file).

#### 9.1.6 Threat Model

| # | Threat | Mitigation |
|---|---|---|
| T1 | Manifest tampering | Ed25519 signature verification |
| T2 | Package tampering | SHA-256 hash + Authenticode |
| T3 | Downgrade attack | `minPreviousVersion` enforcement |
| T4 | MITM during download | TLS 1.2+ pinning to update domain |
| T5 | Replay of old manifest | Timestamp + version monotonicity check |
| T6 | Rogue update server | Public key pinning in app binary |
| T7 | Malicious local-import file | Same signature + hash verification as remote |
| T8 | Crash loop after update | Sentinel file → auto-rollback |

#### 9.1.7 Chat Complexity
**MEDIUM** — manifest parsing, signature verification, hash check, state machine, manual import all chat-buildable. `Add-AppxPackage` invocation and live HTTPS calls finalized in Codex.

#### 9.1.8 Acceptance Criteria
- Detect new version within 24h of publication
- Reject tampered manifest (signature fail)
- Reject tampered package (hash fail)
- Support offline local file import
- Rollback works after sentinel-detected crash
- User can disable updater entirely

### 9.2 Bug Reporting System — Small-Team Optimized

**Properties:**
- Zero-infrastructure: routes to email + GitHub Issues
- Crash capture + user-submitted form
- PHI-safe by default (balanced redaction)
- Local queue with retry on offline
- Deduplication via fingerprint hash
- Correlation IDs

#### 9.2.1 Payload Schema

```json
{
  "id": "{UUID}",
  "timestamp": "2026-06-01T12:34:56Z",
  "type": "crash | user_submitted | regression",
  "severity": 3,
  "title": "Reminder failed to fire",
  "description": {
    "expected": "Toast appears at deadline",
    "actual": "No toast; tray icon did not flash"
  },
  "environment": {
    "os": "Windows 11 23H2",
    "appVersion": "1.0.0",
    "build": "2026.05.26.1",
    "channel": "stable"
  },
  "correlationId": "{UUID}",
  "fingerprint": "{SHA256_OF_STACK_OR_TITLE}",
  "attachments": [
    { "name": "log.txt", "redacted": true, "sizeBytes": 4096 }
  ],
  "redacted": true
}
```

#### 9.2.2 Crash Capture Policy
- **Captured:** stack trace (redacted), thread ID, OS version, app version, last 100 log lines (redacted), correlation ID.
- **NOT captured:** task content, user names beyond Windows username initials, file paths beyond `%LOCALAPPDATA%\TaskTree`, network identifiers, audit log content.

#### 9.2.3 Redaction & Data Minimization
- All free-text fields pass through `IComplianceCore.RedactPhi()`.
- Regex patterns: SSN, MRN-like 6–10 digit strings, names from common-name list, dates, phone numbers, emails (except support email).
- Strictness: **balanced** — over-redact rather than under.

#### 9.2.4 Routing Rules

| Severity | Channel |
|---|---|
| 1 (Critical) | Email + GitHub Issue (label: `critical`) |
| 2 (High) | Email + GitHub Issue (label: `high`) |
| 3 (Normal) | GitHub Issue (label: `bug`) |
| 4 (Low) | GitHub Issue (label: `enhancement`) |
| 5 (Trivial) | Local file drop only |

#### 9.2.5 Storage & Retention
- Local queue in `%LOCALAPPDATA%\TaskTree\bugreports\` (encrypted via SecureStore).
- Successful submissions: deleted after 7 days.
- Failed submissions: retained 30 days then purged.

#### 9.2.6 Security Controls
- Outbound TLS 1.2+ only.
- GitHub PAT stored DPAPI-wrapped.
- Rate limit: max 5 reports/minute, 50/day.
- Tamper detection: payload SHA-256 stored alongside; mismatch → flag in audit log.

#### 9.2.7 Chat Complexity
**MEDIUM** — schema, redaction, queue, routing all chat-buildable. Live SMTP + GitHub API calls finalized in Codex.

#### 9.2.8 Acceptance Criteria
- Crash auto-captures within 200ms
- PHI never leaves device unredacted
- Offline queue retries on next online tick
- Dedup via fingerprint prevents duplicate Issues
- User can opt out entirely

---

## Section 10 — Compliance / Security Hardening

### 10.1 Regulatory Basis
HIPAA Security Rule (45 CFR §164.308, §164.312) — administrative, physical, and technical safeguards. TaskTree implements **technical safeguards** end-to-end.

### 10.2 Data Handling Policy
- All task content treated as **potential PHI**.
- No PHI in logs, telemetry, or bug reports without redaction.
- No PHI transmitted off-device in v1.0.

### 10.3 Encryption
- **At rest:** AES-256-GCM with per-install 256-bit master key.
- **Master key:** generated on first run, wrapped via DPAPI (user scope) and stored at `%LOCALAPPDATA%\TaskTree\keys\master.bin`.
- **In transit:** TLS 1.2+ for updater and bug reporter only; cert pinning for update domain.

### 10.4 Access Controls
- Single-user app, but enforces Windows user-scope DPAPI binding (key inaccessible to other Windows users on same machine).
- Auto-logoff requires re-auth on resume (Windows password via Credential UI prompt).

### 10.5 Audit Logging — JSON Schema

```json
{
  "seq": 12345,
  "timestamp": "2026-06-01T12:34:56.789Z",
  "actor": "windowsUserSid",
  "module": "TaskEngine",
  "action": "TaskAdded",
  "targetId": "{UUID}",
  "result": "success",
  "prevHash": "{HEX64}",
  "hash": "{HEX64}"
}
```

`hash = SHA256(prevHash + canonicalJson(entryWithoutHash))`

### 10.6 Transmission Security
- TLS 1.2+ minimum; TLS 1.3 preferred.
- Cert pinning on update domain.
- No HTTP fallback ever.

### 10.7 Integrity Controls
- Hash-chained audit log (§10.5).
- SecureStore payloads include HMAC tag (built into GCM).
- Chain verify on app startup; alert + audit on mismatch.

### 10.8 Legal / Agreement Requirements
- BAA NOT required for v1.0 (no PHI transmitted).
- EULA presented on first run; consent recorded in audit.

### 10.9 Breach Notification Readiness
- If chain verify fails or audit log tampered → user-visible warning + export of last-known-good audit chain for incident review.

### 10.10 Compliance Mode Toggle
- **HIPAA mode (default ON):** redaction strict, auto-logoff enabled, audit chain enforced.
- **Personal mode (opt-in):** redaction balanced, auto-logoff configurable, audit chain still enforced.

---

## Section 11 — Compatibility Matrix

| Component | Min Version | Recommended |
|---|---|---|
| Windows | 10 build 1809 | 11 23H2+ |
| .NET Runtime | bundled (self-contained) | bundled |
| RAM | 256 MB free | 512 MB |
| Disk | 150 MB | 300 MB |
| Architecture | x64 | x64 or ARM64 |
| Display | 1024×768 | 1920×1080+ |

---

## Section 12 — Complete Tech Stack

| Layer | Technology | Version |
|---|---|---|
| Language | C# | 12 |
| Runtime | .NET | 8 (LTS) |
| UI | WPF | .NET 8 |
| UI Toolkit | ModernWpfUI | 0.9.6+ |
| MVVM | CommunityToolkit.Mvvm | 8.2+ |
| Tray | H.NotifyIcon.Wpf | 2.0+ |
| Crypto | System.Security.Cryptography | built-in |
| JSON | System.Text.Json | built-in |
| Logging | Microsoft.Extensions.Logging | 8.0 |
| DI | Microsoft.Extensions.DependencyInjection | 8.0 |
| Tests | MSTest | 3.x |
| Mocking | Moq | 4.x |
| HTTP | HttpClient (built-in) | — |
| Packaging | MSIX (Windows App SDK) | 1.5+ |
| Signing | signtool.exe + Ed25519 (NSec.Cryptography) | — |

**No other libraries permitted (Rule D3).**

---

## Section 13 — End-to-End User Flow

1. User installs TaskTree MSIX.
2. First run: master key generated, DPAPI-wrapped, audit chain initialized, EULA accepted.
3. App minimizes to tray.
4. User presses configured hotkey (default `Ctrl+Alt+T`).
5. Tree GUI appears warm (< 200ms).
6. User adds task: title, priority, deadline, optional parent.
7. TaskEngine writes to SecureStore (encrypted).
8. ReminderScheduler picks up next tick.
9. At cadence time, toast fires via Tier 1/2/3 chain.
10. User clicks toast → tree GUI focuses → user marks complete.
11. Audit log entry written for every state change.
12. After 15 min idle: app locks; re-auth required.
13. Daily: updater polls manifest; user prompted on new version.
14. On crash: bug reporter captures, redacts, queues, sends.

---

## Section 14 — Phase Plan

| Phase | Name | DoD |
|---|---|---|
| 0 | Scaffold | Solution builds; all interfaces + models + enums + security primitives present |
| 1 | Core MVP | Tray + TaskEngine + ReminderScheduler + SecureStore + ComplianceCore baseline; reminders fire |
| 2 | Secondary Features | Hotkeys, full tree GUI, drag-drop, settings, auto-logoff, snooze/escalation |
| 3 | Extended Integration | AutoUpdater + BugReporter end-to-end; MSIX packaging |
| 4 | Hardening & Release | Compliance audit, perf optimization, packaging signing, docs |
| 5 | Handoff & Gap Closure | Codex/Claude Code closes compile/integration/runtime/environment gaps |

(Detailed sub-phases in `Roadmap.md`.)

---

## Section 15 — Performance Targets

| Metric | Target |
|---|---|
| Tray click → GUI visible (warm) | < 200 ms |
| Tray click → event raised | < 50 ms |
| TaskEngine CRUD | < 50 ms |
| Full tree fetch (≤1000 nodes) | < 100 ms |
| ReminderScheduler tick eval | < 10 ms |
| SecureStore read/write (≤10 MB) | < 100 ms |
| Audit write | < 20 ms |
| Audit chain verify (10k entries) | < 500 ms |
| Updater manifest check | < 2 s |
| Bug submit queued | < 50 ms |
| Idle RAM | < 80 MB |
| CPU at idle | < 0.5% |

---

## Section 16 — Honest Limitations

- **Single user, single device** — no sync, no multi-user, no cloud.
- **Windows only** — no macOS/Linux in v1.0.
- **No mobile companion** — tray is desktop-only.
- **No team sharing** — tasks are private to Windows user account.
- **Hotkey conflicts** possible — user must reconfigure if collision.
- **Toast suppression by Focus Assist** — falls back to Tier 2/3 but cannot override Do-Not-Disturb policy.
- **No screenshot attach in bug reports** — too high a PHI leak risk in v1.0.

---

## Section 17 — Glossary

| Term | Definition |
|---|---|
| PHI | Protected Health Information |
| Tray | Windows notification area (bottom-right) |
| Toast | Windows native notification popup |
| Cadence | Frequency of reminder repetition by priority |
| Hash chain | Audit log where each entry's hash includes the previous entry's hash |
| DPAPI | Windows Data Protection API |
| MSIX | Modern Windows app package format |
| GCM | Galois/Counter Mode (authenticated encryption) |

---

## Section 18 — Version History / Change Log

| Version | Date | Author | Changes |
|---|---|---|---|
| 1.0.0 | 2026-05-26 | Dominic Sarria-Wiley | Initial Architecture.md |
| 1.0.1 | 2026-05-26 | Dominic Sarria-Wiley | Expanded §3.3 tests/ list to add TaskTree.Modules.TrayHost.Tests and TaskTree.UI.Tests (referenced by Roadmap 1E / 2A / 2B / 2C / 2D / 2E). TaskTree.Perf.Tests intentionally deferred to Phase 4B amendment. |

---

## Section 19 — Chat-First Development Strategy

### 19.1 Token-Aware Chunking
- Max ~4,500 tokens output per chat message.
- One file per message for HIGH complexity; up to 3 files per message for LOW complexity (interfaces, simple models).

### 19.2 File Grouping for Single-Message Generation
- **Group A:** All interfaces (one message)
- **Group B:** All models + enums (one message)
- **Group C:** Per-module implementation (one message each: TaskEngine, ReminderScheduler, etc.)
- **Group D:** Per-module test class (one message each)

### 19.3 Naming Conventions (Drift Prevention)
- Interfaces: `I{Name}` PascalCase
- Implementations: `{Name}` (no prefix)
- Async methods: suffix `Async`
- Events: past-tense (`TaskAdded`) or imperative-Requested (`ShowTreeRequested`)
- Namespaces: `TaskTree.<Layer>.<Module>`

### 19.4 Context Recovery
If chat session expires:
1. Re-attach `Architecture.md` + `Roadmap.md` + `HANDOFF.md` to new chat.
2. New chat reads `HANDOFF.md` "Next Action Block" to resume.

### 19.5 Max File Size per Chat Message
- 600 lines or 4,500 tokens, whichever is smaller.

### 19.6 Sequential Generation Rules
- Never reference a type not yet generated in a prior phase.
- Phase 0 must complete fully before any Phase 1 module begins.

---

## Section 20 — Repo Stitching Protocol

### 20.1 Assembly Process
1. User downloads all phase zips from chat.
2. Run `tools/assemble-repo.ps1`.
3. Script extracts each zip into the canonical folder per §3.3.
4. Script runs namespace validation (`verify-namespaces.ps1`).
5. Script runs duplicate detection.
6. Script invokes `dotnet restore` + `dotnet build`.

### 20.2 File Naming Normalization
- Strip any chat-added prefixes (e.g., `Phase1A_`).
- Enforce PascalCase for `.cs` files.
- Enforce match between class name and file name.

### 20.3 Namespace Validation Checklist
- Every `.cs` file declares a namespace.
- Namespace matches folder path under `src/`.
- No duplicate type names across namespaces.
- All `using` statements resolve.

### 20.4 Duplicate Detection
- Hash file contents (SHA-256); duplicates flagged.
- If two files claim same class name → halt assembly, prompt user.

### 20.5 Order of Assembly
1. `.sln` and `.csproj` files
2. Interfaces (TaskTree.Core/Abstractions)
3. Models + Enums
4. Security + Logging primitives
5. Module implementations (dependency order)
6. UI
7. Orchestrator
8. App composition root
9. Tests
10. Tools + packaging

### 20.6 Build Verification Commands
```powershell
dotnet restore TaskTree.sln
dotnet build TaskTree.sln -c Release
dotnet test TaskTree.sln -c Release --filter Category!=Live
```

### 20.7 Stitching Helper Script (PowerShell Template)

```powershell
# tools/assemble-repo.ps1
param(
    [Parameter(Mandatory=$true)][string]$ZipDir,
    [Parameter(Mandatory=$true)][string]$OutDir
)
$ErrorActionPreference = 'Stop'
Write-Host "Assembling TaskTree from $ZipDir into $OutDir"
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
Get-ChildItem -Path $ZipDir -Filter *.zip | Sort-Object Name | ForEach-Object {
    Write-Host "Extracting $($_.Name)"
    Expand-Archive -Path $_.FullName -DestinationPath $OutDir -Force
}
& "$PSScriptRoot\verify-namespaces.ps1" -Root $OutDir
Write-Host "Done. Run: dotnet restore; dotnet build"
```

---

## Section 21 — Gap Classification System

| Gap Type | Definition | Who Fixes | Example |
|---|---|---|---|
| Compile Gap | Code won't build | Codex | Missing `using`, type not found |
| Integration Gap | Modules don't connect | Codex | Orchestrator DI wiring missing |
| Runtime Gap | Behavior incorrect | Codex + Human | Edge case in reminder cadence |
| Environment Gap | Requires OS/API at runtime | Human + Codex | Global hotkey PInvoke, `Add-AppxPackage`, live SMTP |
| Knowledge Gap | Spec ambiguous | Human only | Architecture.md silent on a behavior |

Every gap encountered MUST be logged in `HANDOFF.md` §Gap Summary with type, location, and remediation owner.

---

## Document Integrity Footer

- **SHA-256 (of this file):** `{COMPUTE_AT_COMMIT}`
- **Total Sections:** 21
- **Total Modules:** 8
- **Locked Features:** 16
- **Companion Docs:** `Roadmap.md`, `HANDOFF.md`

> *"Build it so any module can be torn out and rebuilt without disrupting the rest. Modularity is not a luxury — it's the only way a solo builder ships something this opinionated."*
> — Dominic Sarria-Wiley, Owner
