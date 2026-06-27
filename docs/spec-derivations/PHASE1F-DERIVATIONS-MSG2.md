# PHASE1F-DERIVATIONS-MSG2.md

**Phase:** 1F - Orchestrator + CompositionRoot + ServiceRegistrations
**Msg:** 2 of 3 (Msg 1 = Orchestrator + Core; Msg 2 = Bootstrap; Msg 3 = tests)
**Source authority:** Architecture.md v1.0.3 §3.2, §3.3, §12; Roadmap.md v1.0.0 Sub-Phase 1F
**HALT log:** 7 Msg-2 items resolved 2026-05-27 (owner batched approval)
**Marker:** SPEC-DERIVED-PHASE1F now totals 7 .cs files (5 Msg 1 + 2 Msg 2)

---

## Registry - 4 Spec-Derived Items (Msg 2 additions)

| # | Derivation | Type | Spec citation | LOAD-BEARING? |
|---|---|---|---|---|
| §11 | ServiceRegistrations.AddTaskTreeServices extension method (12 singletons in §3.2 order) | DI surface | §3.2 + §12 + HALT #4 Msg 2 | YES - P1F-AC1 |
| §12 | CompositionRoot IAsyncDisposable lifecycle + App.xaml.cs integration | App lifecycle | WPF idiom | YES - closes G1E-S1 + G1F-E1/E2/E3 |
| §13 | All-singleton DI lifetime matrix wired | Lifetime contract | Roadmap 1F | YES |
| §14 | TaskTreePaths singleton ctor-resolves canonical paths into all dependent services | Path injection | §10.3 + PHASE0-MSG5 §3 + PHASE1B §1, §3 | YES - closes Cross-phase Flags #4/#5 |

---

## Implementation Notes

### N1 - Double-registration of TaskTreePaths (G1F-H13)
CompositionRoot pre-registers a TaskTreePaths instance, then AddTaskTreeServices adds its own factory. MS.Ext.DI last-registration-wins, so the AddTaskTreeServices factory creates a second TaskTreePaths. Both instances reference identical paths, so behavior is benign in Phase 1F, but Phase 5B should refactor (one of):
- (a) Remove TaskTreePaths registration from AddTaskTreeServices; require external registration
- (b) Use services.TryAddSingleton<TaskTreePaths> inside AddTaskTreeServices
- (c) Inject Func<TaskTreePaths> for test override

### N2 - Concrete-name assumptions wired in this msg
| Concrete | Used in | Gap ID |
|---|---|---|
| SystemClock | IClock registration | G1F-H8 |
| FileAppLogger ctor(logDir) | IAppLogger factory | G1F-H9 |
| AesGcmCryptoProvider | ICryptoProvider | confirmed PHASE0-MSG5 |
| MasterKeyManager ctor(keyDir, IAppLogger, fileName) | factory | G1F-H10 |
| SecureStore ctor(storageDir, IMasterKeyManager, ICryptoProvider, IAppLogger) | factory | G1F-H11 |
| ComplianceCore 5-dep ctor + inline PhiRedactor/AuditChainWriter | factory | G1F-H12 |
| PhiRedactor ctor(IReadOnlyCollection<string>) | factory | confirmed PHASE1C §7 + Q11 |
| TaskEngine (no factory; ctor-injected) | depends on G1F-H4 | PHASE1A §4 |
| ReminderScheduler / TrayHost / Placeholder / Orchestrator | no-factory ctor-injected | confirmed |

### N3 - WPF partial class App requires UseWPF
App.xaml.cs declares partial class App : Application. The Msg 2 csproj sets <UseWPF>true</UseWPF>. **G1F-H14** flags this for Phase 5A scaffold-vs-amendment verification.

### N4 - MessageBox requires UI thread
App.OnStartup is on WPF Application STA, so MessageBox.Show is safe. Phase 5C tests do NOT exercise this path. **G1F-S12** flags Phase 5D live integration verification.

### N5 - Composition fault-injection seam
CompositionRoot.StartAsync has no test-friendly fault-injection point. **G1F-S13** flags Phase 5C refactor.

---

## Phase 1F Msg 2 Gap Ledger

### Hard Gaps - Phase 5A/5B compile risk

| ID | Gap | Likely fix | Phase |
|---|---|---|---|
| G1F-H8 | SystemClock concrete name assumed | Verify vs Phase 0 Msg 5; rename per D2 | 5B |
| G1F-H9 | FileAppLogger ctor(string logDir) signature assumed | Verify; adjust factory | 5B |
| G1F-H10 | MasterKeyManager ctor (keyDir, IAppLogger, fileName) | Verify vs PHASE1B R8 | 5B |
| G1F-H11 | SecureStore ctor order | Verify vs PHASE1B R8 | 5B |
| G1F-H12 | ComplianceCore 5-dep ctor + inline PhiRedactor/AuditChainWriter | Verify; consider DI promotion | 5B |
| G1F-H13 | Double-registration of TaskTreePaths | Apply N1 resolution (a/b/c) | 5B |
| G1F-H14 | <UseWPF>true</UseWPF> verify against Phase 0 Msg 1 scaffold | Verify; ensure Application base resolves | 5A |

### Soft Gaps - Phase 5C/5D integration risk

| ID | Gap | Closure point | Notes |
|---|---|---|---|
| G1F-S10 | Composition failure routes only to MessageBox | Phase 3D | BugReporter capture wiring |
| G1F-S11 | TaskTreePaths default %LOCALAPPDATA% non-overridable | Phase 5E | Override registration in integration |
| G1F-S12 | MessageBox not exercised in offline tests | Phase 5D | Live integration verifies |
| G1F-S13 | No fault-injection seam in CompositionRoot.StartAsync | Phase 5C | Add BuildProviderAsync extension point |

### Environment Gaps - Phase 5D/5E

| ID | Gap | Phase | Notes |
|---|---|---|---|
| G1F-E4 | WPF Application STA confirmed at live runtime | 5D | Live integration |
| G1F-E5 | ShutdownMode=OnExplicitShutdown in tray-only mode | 5D | Verify tray Exit triggers Application.Shutdown |
| G1F-E6 | Application.Current.Dispatcher for UI marshaling | 5D | Phase 1G ToastTier2Adapter uses it |

---

## Closed Gaps (Msg 2 specifically)

| Prior gap | Closure mechanism in Msg 2 |
|---|---|
| Cross-phase Flag #2 (IComplianceCore -> TaskEngine) | ✅ CLOSED - TaskEngine singleton; ctor-injects IComplianceCore via container |
| Cross-phase Flag #3 (IMasterKeyManager singleton) | ✅ CLOSED - AddSingleton<IMasterKeyManager, MasterKeyManager> factory |
| Cross-phase Flag #4 (canonical key/store paths) | ✅ CLOSED - TaskTreePaths factory injects KeyDir + StorageDir |
| Cross-phase Flag #5 (canonical log directory) | ✅ CLOSED - TaskTreePaths.LogDir injected into FileAppLogger |
| G1E-S1 (STA thread for TrayHost) | ✅ CLOSED - WPF App.OnStartup runs on STA; CompositionRoot inherits |

### Partial closures (still LOAD-BEARING)
| Prior gap | Status | Reason |
|---|---|---|
| Cross-phase Flag #6 (Q11 allowlist) | ⚠️ PARTIAL - Array.Empty<string>() passed to PhiRedactor; Q11 stays LOAD-BEARING for Phase 5F |

### Deferred
| Prior gap | Deferred to | Reason |
|---|---|---|
| G1D-14 (AutoLogoffTriggered -> StopAsync) | Phase 2F | Event not yet emitted (Phase 1C stub) |
| G1D-18 (TaskDeleted -> Scheduler purge) | Phase 2C/2G | Event not yet declared on ITaskEngine (G1F-H5) |

---

## Anti-Hallucination Re-Check (per D8)

| Item | Authority | Outcome |
|---|---|---|
| Constructor injection only | Roadmap 1F | OK - 0 service-locator calls |
| Singletons for stateful modules | Roadmap 1F | OK - all 12 services singleton |
| MS.Ext.DI 8.0 only | §12 | OK |
| BuildServiceProvider(validateScopes: true) | best practice | OK |
| App.xaml.cs uses partial class | WPF | OK |
| Header citations D1 | D1 | OK |
| XML doc on publics D10 | D10 | OK |

---

## Acceptance Criteria Wire-Up (Msg 3)

| Criterion | Implementation site (Msg 2) | Test (Msg 3) |
|---|---|---|
| P1F-AC1 (Container builds) | ServiceRegistrations + CompositionRoot.StartAsync | ServiceRegistrations_ResolvesAllInterfaces_NoMissingDeps |
| P1F-AC2 (Simulated E2E flow) | Full pipeline wired | EndToEnd_TrayShowTreeRequested_WritesAudit |
| P1F-AC3 (Audit chain receives event entries) | Orchestrator.SafeAuditAsync + ComplianceCore registration | StartAsync_WritesAppStartAudit |

---

## Files Produced This Msg (Msg 2 of 3)

| Path | Purpose |
|---|---|
| src/TaskTree.App/Bootstrap/ServiceRegistrations.cs | 12-singleton extension method |
| src/TaskTree.App/Bootstrap/CompositionRoot.cs | IAsyncDisposable container owner |
| src/TaskTree.App/App.xaml | OnExplicitShutdown WPF Application |
| src/TaskTree.App/App.xaml.cs | OnStartup/OnExit wiring |
| src/TaskTree.App/TaskTree.App.csproj | AMENDED - WinExe, WPF, 7 ProjectReferences |
| docs/spec-derivations/PHASE1F-DERIVATIONS-MSG2.md | This file - 4 derivations + 14 new gaps + 5 closures |
| docs/HANDOFF.v1.0.21-delta.md | State tracker delta |
| tools/find-spec-derivations.ps1 | UPDATED - SPEC-DERIVED-PHASE1F = 7 |

---

## End of PHASE1F-DERIVATIONS-MSG2.md
