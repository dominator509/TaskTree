# TaskTree Repo Brief

Compact context for Codex, Serena, and Obsidian. Link here instead of copying large repo content into notes.

## Purpose

TaskTree is a Windows-only, HIPAA-aware desktop task/reminder app. It lives in the system tray, manages prioritized hierarchical tasks, stores task data encrypted at rest, emits reminders, and records hash-chained audit events.

## Authority Surface

- `Architecture.md` and `docs/Architecture.md`: primary architecture, module contracts, tech stack, security/compliance constraints, and rebuild protocol.
- `Roadmap.md` and `docs/Roadmap.md`: phase order, anti-drift rules, HALT protocol, acceptance criteria, and owner-approval gates.
- `docs/HANDOFF.md` plus versioned `docs/HANDOFF*.md`: live/additive handoff state. Note: root code currently contains modules beyond the early Phase 1C state described in the top of `docs/HANDOFF.md`; verify current files before assuming phase status.
- `docs/spec-derivations/`: phase-specific derivation registries.

## Stack

- Language/runtime: C# 12 on .NET 8, `net8.0-windows`.
- Local SDK used for validation: `C:\Users\domin\.dotnet\dotnet.exe` (8.0.422).
- UI/app: WPF, ModernWpfUI, CommunityToolkit.Mvvm, H.NotifyIcon.Wpf.
- Tests: MSTest, Moq, `Microsoft.NET.Test.Sdk`.
- Packaging: MSIX/WAP project under `packaging/`.
- Crypto/storage: `System.Security.Cryptography`, AES-GCM, DPAPI-wrapped local master key, JSON payloads, local `%LOCALAPPDATA%\TaskTree\...` paths.
- External integrations: HTTPS updater manifest/download, Ed25519 verification via NSec.Cryptography, SMTP, GitHub Issues, Windows tray/hotkey/session APIs, and WPF toast fallback. Runtime credentials/package identity/signing remain environment-owned.

## Entrypoints

- Solution: `TaskTree.sln`.
- App host: `src/TaskTree.App/App.xaml`, `src/TaskTree.App/App.xaml.cs`.
- DI/composition: `src/TaskTree.App/Bootstrap/CompositionRoot.cs`, `src/TaskTree.App/Bootstrap/ServiceRegistrations.cs`.
- App startup resolves the existing AutoUpdater and BugReporter modules, injects TaskTreePaths-backed storage roots, handles the updater first-launch sentinel/rollback attempt, installs the global crash hook, then starts the Orchestrator. Orchestrator startup flushes queued bug reports and owns the bounded 24-hour updater poll lifecycle.
- Core contracts/models: `src/TaskTree.Core/`.
- Module implementations: `src/TaskTree.Modules.*`, `src/TaskTree.Orchestrator/`, `src/TaskTree.UI/`.
- Test support: `tests/TaskTree.TestSupport/`.
- Packaging: `packaging/TaskTree.Installer.wapproj`, `packaging/build-msix.ps1`.

## Commands

Run shell commands through the inherited RTK rule from `AGENTS.md`.

```powershell
rtk dotnet restore TaskTree.sln
rtk dotnet build TaskTree.sln -c Release
rtk dotnet test TaskTree.sln -c Release --filter "TestCategory!=Live&TestCategory!=Performance"
rtk dotnet test tests/TaskTree.Perf.Tests/TaskTree.Perf.Tests.csproj -c Release --filter "TestCategory=Performance&TestCategory!=Live"
rtk powershell -NoProfile -File tools/find-spec-derivations.ps1 -Root .
rtk powershell -NoProfile -File packaging/build-msix.ps1
```

Use documented command paths when validating scripts. Do not install packages during repo-tooling tasks.

## Important Directories

- `src/TaskTree.Core`: abstractions, models, enums, crypto/logging primitives.
- `src/TaskTree.Modules.TaskEngine`: task tree CRUD and persistence integration.
- `src/TaskTree.Modules.ReminderScheduler`: reminder cadence/tick evaluation.
- `src/TaskTree.Modules.SecureStore`: encrypted local persistence and key handling.
- `src/TaskTree.Modules.ComplianceCore`: PHI redaction and audit chain.
- `src/TaskTree.Modules.TrayHost`: Windows tray/hotkey integration.
- `src/TaskTree.Modules.AutoUpdater`: update manifest/signature/staging/rollback logic, atomic apply sentinels, and first-launch recovery state.
- `src/TaskTree.Modules.BugReporter`: crash/report queue, redaction, routing adapters.
- BugReporter queue state is encrypted through `ISecureStore`; unredacted reports cannot be persisted, delivered reports remain queryable for 7 days, failed reports for 30 days, and only pending reports are retried.
- `src/TaskTree.Modules.Settings`, `src/TaskTree.Modules.Snooze`, `src/TaskTree.Modules.SessionLock`: later-phase feature modules present in the current tree.
- `tests/`: MSTest projects mirroring modules plus perf and UI tests.
- `docs/`: handoff, architecture, roadmap, derivation, compile/stitching, release, compliance, and user/admin docs.
- Phase evidence: `docs/test-gap-report.md`, `docs/integration-gap-report.md`, `docs/env-gap-report.md`, `docs/final-validation-report.md`.
- `.obsidian/`: existing local Obsidian vault settings only; do not copy large repo content into notes.

## Do-Not-Touch / Risk Areas

- Do not weaken Roadmap D1-D10, HALT, owner-approval, or gap-classification rules.
- Treat all task content and bug-report text paths as potential PHI. Do not add real-looking names, MRNs, SSNs, DOBs, credentials, tokens, production URLs, or unredacted report examples.
- Do not delete or flatten additive handoff/versioned docs unless the owner explicitly approves a consolidation.
- Do not edit generated/build/local output: `bin/`, `obj/`, `TestResults/`, coverage, publish/package output, `.vs/`, NuGet/package caches, and local workspace state.
- Live Windows integrations, MSIX signing/install, SMTP, GitHub Issues, and updater network flows may require real environment/provider validation; report local-only checks honestly.

## Current Unknowns / TODOs

- Phase 5C offline validation is green with 411 non-live, non-performance contract tests plus 7 isolated measurable performance tests; the last fully converted SDK report measured 77.26% of production source (1,651/2,137 lines), and the current collector rerun produced a fresh ignored artifact. Phase 5D composition now resolves the persisted HotkeyManager, AutoUpdater, and BugReporter modules with TaskTreePaths-backed roots; the app installs the crash hook, handles first-launch recovery before the Orchestrator graph, flushes queued bug reports at startup, owns the bounded 24-hour updater poll lifecycle, verifies the audit chain before subscriptions and records failure without aborting startup, resolves empty audit actors to the current Windows user SID, exports the last-known-good audit prefix on invalid startup chains, fails closed on null audit entries and invalid persisted settings, validates virtual-key bounds before native registration, preserves TaskNode metadata across storage snapshots, serializes settings, hotkey/session-lock/compliance idle-monitor lifecycles, rolls back SessionLock and ReminderScheduler partial starts, unwinds failed orchestrator starts, completes best-effort shutdown, drains in-flight reminder delivery before stop, owns the scheduler timer reference across stop, marshals scheduler/session callbacks onto the WPF dispatcher, applies persisted Light/Dark/System themes at runtime, and enforces audit-chain sequence plus SecureStore key-path invariants, but real interactive E2E remains open. Phase 5E code paths now atomically promote staged packages and first-launch sentinels, distinguish the first post-update launch from a surviving crash attempt, fail closed into the registered last-known-good rollback service, reject unsafe manifest versions and incomplete offline package metadata, enforce channel/version/rollout eligibility again at apply time, verify offline package size before import, serialize updater checks/applies and state transitions, bound SMTP/GitHub delivery calls, require SMTP TLS, validate GitHub repository segments, keep crash-hook subscription idempotent, revalidate staged package integrity at apply time, promote file-drop reports through temporary files, serialize bug-report queue, limiter, and router delivery state, expose the PHI email allowlist as a snapshot, confine SecureStore key filenames, keep the DPAPI-unwrapped master key behind defensive copies, and retain delivered/failed queue records on the documented 7/30-day schedule; packaging preflight is hardened but stops on absent owner-supplied visual assets, while provider/package validation remains open. See the linked phase reports.
- The Phase 2B/2E UI surface now includes the declared reusable `SettingsView`, a hierarchical task `TreeView` with priority colors and deadlines, and a delete command backed by the existing `ITaskEngine.DeleteAsync` contract; STA XAML smoke coverage is green. Drag-drop sibling ordering and real desktop interaction remain environment/spec gated.
- Post-v1.0.78 validation rerun: 411 non-live, non-performance tests passed; 7 performance tests passed; the last Live-inclusive baseline remains 415 passed plus 1 intentional desktop-metrics skip.
- Compliance auto-logoff now reaches the orchestrator privacy path: an open main window is dispatcher-safely hidden as soon as `IComplianceCore.AutoLogoffTriggered` fires, and the subscription is removed during normal and failed-start cleanup. Real lock/re-auth remains desktop-gated.
- Runtime theme application now swaps the app-level light/dark resource dictionaries from persisted `ThemePreference`; `System` follows the Windows `AppsUseLightTheme` setting and falls back to light when unavailable. Invalid startup audit chains now emit a user-visible warning when a WPF application is present and atomically export the validated last-known-good prefix under `%LOCALAPPDATA%\TaskTree\audit-incidents`.
- Real WPF/tray E2E, MSIX/WAP build/sign/install, and installed-binary validation require an interactive Windows environment.
- Crash capture now synchronously commits the redacted report to the encrypted queue before the unhandled-exception callback returns, bounded to 200 ms; queue persistence remains local and provider-independent.
- Q10: owner must provide the real PHI common-name source list before Phase 5F sign-off.
- Q11: owner must provide or explicitly accept the support-email allowlist default before Phase 5F sign-off.
- GitHub remote currently exists at `https://github.com/dominator509/TaskTree.git`; `main` has been pushed. Continue to verify current `git status`/remote state before publish or recovery work.
