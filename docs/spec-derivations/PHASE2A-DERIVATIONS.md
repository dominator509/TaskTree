# PHASE2A-DERIVATIONS.md - Phase 2A Msg 1 (HotkeyManager + HotkeyConfig + TaskTree.TestSupport Promotion)

> Scope: HotkeyManager.cs (HIGH) + HotkeyConfig.cs + TaskTree.TestSupport project + FakeClock/InMemorySecureStore relocation + 5 csproj patches + 10 tests.
> Companion: All prior PHASE registries. Owner-approved HALT batch: 22 items.

## Section 1 Summary Table

| # | Item | Resolution | LOAD-BEARING? |
|---|---|---|---|
| 1 | Phase 1 owner approval status | Implicit per "Proceed" directive (Gap #100) | Yes (rollback path) |
| 2 | Architecture v1.0.2 sign-off | 5 REQUIRED implicitly accepted; Change 2 deferred to v1.0.3 (Gap #101) | Yes |
| 3 | Q11 status at Phase 2A start | Empty default implicit (Gap #102) | Q11 |
| 4 | Q10 status | Remains DEFERRED to Phase 5F | No (Q10 carries) |
| 5 | TaskTree.TestSupport project creation | NEW csproj net8.0-windows | Closes Gap #97/#98 |
| 6 | FakeClock relocation | Moved + namespace change (Gap #103) | No |
| 7 | InMemorySecureStore relocation | Moved + namespace change (Gap #104) | No |
| 8 | Architecture v1.0.2 7th change | Add tests/TaskTree.TestSupport/ to section 3.3 (Gap #105) | No |
| 9 | Existing test csproj patches | 5 csproj add TestSupport ProjectReference + using migration manifest (Gap #106) | No |
| 10 | HotkeyManager class shape | public sealed class : IDisposable; 4-param ctor | Yes Gap #107 |
| 11 | HotkeyManager public surface | 5 methods + 1 event | No |
| 12 | HotkeyConfig record shape | record with Default static | No |
| 13 | HotkeyRegistrationResult enum | Nested public enum in HotkeyManager.cs | No (Gap #108 corrected) |
| 14 | HotkeyChangedEventArgs class | Nested public class | No |
| 15 | Default hotkey constant | HotkeyConfig.Default (closes Phase 1E Gap #11) | No |
| 16 | Persistence storage key | "hotkeys/config" | No |
| 17 | Audit posture | HotkeyConfigChanged + HotkeyConflictDetected (Gap #109) | No |
| 18 | InitializeAsync HIGH-stub canonical text | "HIGH: RegisterHotKey + message-only HWND require live env - Codex Phase 5E" | No |
| 19 | HotkeyManagerTests test count | 10 tests | No |
| 20 | csproj for HotkeyManagerTests | TrayHost.Tests.csproj PATCHED | No |
| 21 | Phase 2A Msg structure | Msg 1 = production + tests + project promotion; Msg 2 = Architecture v1.0.2 update (Gap #110) | No |
| 22 | PowerShell update | PHASE2A = 5 | No |

## Sections 2-23 Per-Item Detail

See HANDOFF v1.0.25 section C R20 for bullet summary of each HALT resolution.

## Section 24 Files Produced

| Path | Purpose | Marker |
|---|---|---|
| src/TaskTree.Modules.TrayHost/HotkeyManager.cs | HIGH-stub HotkeyManager | PHASE2A |
| src/TaskTree.Modules.TrayHost/HotkeyConfig.cs | record + Default | PHASE2A |
| src/TaskTree.App/Bootstrap/ServiceRegistrations.cs | PATCHED (HotkeyManager singleton) | PHASE1F + PHASE1G-MSG2 + PHASE2A |
| tests/TaskTree.TestSupport/TaskTree.TestSupport.csproj | NEW project | (csproj) |
| tests/TaskTree.TestSupport/FakeClock.cs | RELOCATED (Gap #97) | PHASE2A |
| tests/TaskTree.TestSupport/InMemorySecureStore.cs | RELOCATED (Gap #98) | PHASE2A |
| tests/TaskTree.Modules.TrayHost.Tests/HotkeyManagerTests.cs | 10 tests | PHASE2A |
| 5 test csproj files | PATCHED with TestSupport ref | (csproj) |
| docs/spec-derivations/PHASE2A-DERIVATIONS.md | This registry | (md) |
| docs/HANDOFF-v1.0.25-delta.md | Additive delta | (md) |
| tools/find-spec-derivations.ps1 | UPDATED 18 buckets | (script) |

## Section 25 Marker Inventory

PHASE2A = 5 distinct .cs files (HotkeyManager + HotkeyConfig + FakeClock + InMemorySecureStore + HotkeyManagerTests). Grand total: 74 -> 79.

## Section 26 Cross-Phase Gaps Introduced (rows 100-110)

| # | Gap | Source | Target Phase | Action |
|---|---|---|---|---|
| 100 | Implicit Phase 1 approval; explicit sign-off pending | HALT #1 | Pre-Phase 5F | Owner formally signs off Phase 1 deliverables |
| 101 | Architecture v1.0.2 implicit acceptance of 5 REQUIRED; Change 2 deferred to v1.0.3 | HALT #2 | Pre-Phase 5F | Owner sign-off final |
| 102 | Q11 empty default implicit resolution | HALT #3 | Pre-Phase 5F | Owner overrides if desired |
| 103 | FakeClock old location Phase 5A cleanup | HALT #6 | Phase 5A | Delete tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs |
| 104 | InMemorySecureStore old location Phase 5A cleanup | HALT #7 | Phase 5A | Delete tests/TaskTree.Core.Tests/TestDoubles/InMemorySecureStore.cs |
| 105 | **Architecture v1.0.2 7th change (TaskTree.TestSupport in section 3.3)** | HALT #8 | Architecture v1.0.2 | Phase 2A Msg 2 emits updated delta document |
| 106 | Using-statement migration manifest for 4 existing test files | HALT #9 | Phase 5A | Replace using TaskTree.Core.Tests.TestDoubles with using TaskTree.TestSupport in: ReminderSchedulerTests.cs, ComplianceCoreTests.cs, AuditChainWriterTests.cs, OrchestratorTests.cs |
| 107 | HotkeyManager 5th IComplianceCore audit injection consumer | HALT #10 | (closed in Phase 2A Msg 1 SR patch) | None |
| 108 | HotkeyRegistrationResult nested enum decision (avoids 8th Architecture v1.0.2 change) | HALT #13 | (closed) | None |
| 109 | Phase 4A compliance policy must document HotkeyConfigChanged + HotkeyConflictDetected vocabulary | HALT #17 | Phase 4A | Document audit vocab |
| 110 | Phase 2A Msg 2 Architecture v1.0.2 final promotion document | HALT #21 | Architecture v1.0.2 | Upon owner sign-off promote Architecture.md to v1.0.2 |

## Section 27 Patches Applied

| File | Patch |
|---|---|
| src/TaskTree.App/Bootstrap/ServiceRegistrations.cs | Add HotkeyManager singleton + Gap #102 Q11 warning text update |
| tests/TaskTree.Modules.ReminderScheduler.Tests/TaskTree.Modules.ReminderScheduler.Tests.csproj | Add TestSupport ProjectReference |
| tests/TaskTree.Modules.ComplianceCore.Tests/TaskTree.Modules.ComplianceCore.Tests.csproj | Same |
| tests/TaskTree.Modules.TaskEngine.Tests/TaskTree.Modules.TaskEngine.Tests.csproj | Same |
| tests/TaskTree.Orchestrator.Tests/TaskTree.Orchestrator.Tests.csproj | Same |
| tests/TaskTree.Modules.TrayHost.Tests/TaskTree.Modules.TrayHost.Tests.csproj | Same |

## Section 28 Known Limitations

1. InitializeAsync HIGH-stub - real RegisterHotKey wiring deferred to Codex Phase 5E.
2. Conflict detection at runtime - architecture (HotkeyRegistrationResult.ConflictDetected) supports it but live detection deferred to Phase 5E.
3. Audit chain integrity for hotkey events verified at Phase 5C via coverlet coverage measurement (Gap #99).
4. FakeClock + InMemorySecureStore relocation requires Phase 5A old-location cleanup (Gap #103/#104) to avoid duplicate types.
5. Using-statement migration manifest for 4 test files required at Phase 5A (Gap #106) before existing tests compile against new namespace.
6. HotkeyConfig record persistence depends on System.Text.Json round-trip - verified at Phase 5C with real ISecureStore.
7. Q11 empty default still triggers runtime warning every startup until owner overrides (Gap #102 implicit acceptance).
8. Architecture v1.0.2 amendment now 7 bundled changes (Gap #105 added) pending owner sign-off; Phase 2A Msg 2 emits final promotion document.
