# PHASE1F-DERIVATIONS.md — Phase 1F Msg 1 (Orchestrator + Composition)

> **Scope:** IOrchestrator surface patch + Orchestrator implementation + CompositionRoot + ServiceRegistrations + 2 csproj patches.
> **Companion docs:** Architecture.md v1.0.1 (v1.0.2 amendment pending — scope expanding to 4 changes), Roadmap.md v1.0.0, HANDOFF.md v1.0.20.
> **Owner-approved HALT batch:** 17 items, batch-resolved.

---

## §1 Summary Table

| # | Item | Resolution | Files | LOAD-BEARING? |
|---|---|---|---|---|
| 1 | `IOrchestrator` surface | Patch interface NOW (StartAsync + StopAsync); v1.0.2 prose later | IOrchestrator.cs | No (Gap #63) |
| 2 | Orchestrator ctor | 6-param: (TE, RS, CC, TH, Log, Clock) — no ISecureStore | Orchestrator.cs | **YES — 3rd CC consumer** |
| 3 | Event subscription order | Subscribe BEFORE Initialize; NotImplementedException-catch | Orchestrator.cs | No (Gap #64) |
| 4 | ReminderDue handler | Placeholder logger only — Phase 1G replaces | Orchestrator.cs | No (Gap #65) |
| 5 | Shutdown order | Reverse-of-Start with per-step try/catch | Orchestrator.cs | No |
| 6 | Audit posture | Lifecycle ONLY (Startup + Shutdown + chain-verify fail) | Orchestrator.cs | No (Gap #66) |
| 7 | CR vs SR split | CR = env paths + container build; SR = DI registrations | CompositionRoot + ServiceRegistrations | No |
| 8 | Canonical paths | 3 helpers w/ idempotent Directory.CreateDirectory | CompositionRoot.cs | No |
| 9 | DI lifetimes | All Phase 1 modules = singleton | ServiceRegistrations.cs | No |
| 10 | Factory lambdas | For path-dependent ctors (FileAppLogger / MKM / SecureStore) | ServiceRegistrations.cs | No |
| 11 | Q11 allowlist | Empty array + startup warning log | ServiceRegistrations.cs | No (Gap #67) |
| 12 | csproj wiring | 7 ProjectRefs (App) + 6 (Orchestrator) + DI/Logging packages | 2 csproj patches | No |
| 13 | Namespace/sealed | public sealed class Orchestrator in TaskTree.Orchestrator | Orchestrator.cs | No |
| 14 | Msg structure | Msg 1 = code; Msg 2 = tests + Architecture v1.0.2 delta UPDATE | (process) | No |
| 15 | PowerShell update | PHASE1F = 4 distinct .cs | tools/find-spec-derivations.ps1 | No |
| 16 | Arch v1.0.2 scope | 4 bundled changes (added §4 IOrchestrator) | docs/Architecture.v1.0.2-delta.md (Msg 2) | No (Gap #63) |
| 17 | Chain integrity at startup | Verify; log + audit on fail; DO NOT abort | Orchestrator.cs | No |

---

## §2 — Item #1: `IOrchestrator` Surface

- **Trigger:** Phase 0 Msg 2 stub was empty; Phase 1F DI resolution requires public methods visible through the interface.
- **Architecture silence:** §3.2 says "coordinates"; no method enumeration.
- **Options:** A) minimal lifecycle 2-method · B) plus event subscription `IObservable` · C) plus delegation methods.
- **Resolution:** Option A — `StartAsync(CancellationToken ct)` + `StopAsync()`.
- **Rationale:** Coordination via internal event subscriptions, not request/response API.
- **Files:** `IOrchestrator.cs` PATCHED.
- **Gap for Handoff:** **Cross-Phase Gap #63 — Architecture v1.0.2 must add §4 IOrchestrator subsection.** Phase 1F Msg 2 emits the updated v1.0.2 delta document (now 4 bundled changes).

---

## §3 — Item #2: Orchestrator Constructor

- **Trigger:** Wires 5 modules per §3.2.
- **Architecture silence:** No ctor shape.
- **Options:** A) 7 deps (incl. ISecureStore) · B) IServiceProvider (anti-pattern) · C) grouped record.
- **Resolution:** 6-param: `(ITaskEngine, IReminderScheduler, IComplianceCore, ITrayHost, IAppLogger, IClock)`. No `ISecureStore` (TaskEngine owns).
- **Rationale:** Constructor injection per §3.2 + D9. IClock for lifecycle audit timestamps.
- **Files:** `Orchestrator.cs` ctor.
- **Gap for Handoff:** **LOAD-BEARING — already covered by Gaps #2, #32, #56** (TaskEngine + ReminderScheduler + TrayHost all receive IComplianceCore). Orchestrator is the 4th IComplianceCore consumer.

---

## §4 — Item #3: Event Subscription Order + NotImplementedException Catch

- **Trigger:** P1F-AC2 requires E2E flow; Phase 1E `TrayHost.Initialize` throws NotImplementedException.
- **Options:** A) Subscribe in StartAsync after Initialize (won't reach due to throw) · B) Subscribe BEFORE Initialize · C) Catch NotImplementedException to allow container build.
- **Resolution:** Hybrid B + C. Subscribe FIRST, then try Initialize wrapped in catch for `NotImplementedException` ONLY.
- **Rationale:** Container builds in Phase 1F without Phase 5E. Other exception types propagate (anti-drift — never swallow real errors).
- **Files:** `Orchestrator.cs` StartAsync.
- **Gap for Handoff:** **Cross-Phase Gap #64 — Phase 5E Codex removes the try/catch when TrayHost.Initialize is wired live.** Grep `catch (NotImplementedException` in Orchestrator.cs at Phase 5E for closure.

---

## §5 — Item #4: ReminderDue Placeholder Handler

- **Trigger:** §3.2 ReminderDue needs a subscriber; Phase 1G ReminderDeliveryService is separate sub-phase.
- **Options:** A) Log only · B) Forward to TrayHost.ShowBalloon (also stubbed) · C) Defer to 1G.
- **Resolution:** Option A.
- **Rationale:** Roadmap Sub-Phase 1F line: "placeholder delivery until 1G". Format: `"[1F-PLACEHOLDER] ReminderDue: TaskId={evt.TaskId} Reason={evt.Reason} Priority={evt.Priority}"`.
- **Files:** `Orchestrator.cs` `OnReminderDuePlaceholder`.
- **Gap for Handoff:** **Cross-Phase Gap #65 — Phase 1G ReminderDeliveryService unsubscribes this placeholder + subscribes Tier 1/2/3 chain.** Code change: in Orchestrator.StartAsync, replace `_reminderScheduler.ReminderDue += OnReminderDuePlaceholder` with subscription wiring through Phase 1G service. Alternative: keep both, but ensure Phase 1G handler runs before placeholder (subscription order in C# events is registration order).

---

## §6 — Item #5: Reverse-Order Shutdown

- **Resolution:** Steps: unsubscribe handlers → ReminderScheduler.StopAsync → TrayHost.Dispose → AuditAsync Shutdown. Each wrapped in try/catch.
- **Rationale:** Reverse of Start to avoid losing in-flight ReminderDue events.
- **Files:** `Orchestrator.cs` StopAsync.
- **Gap for Handoff:** None for Phase 1F. Phase 5D verifies under live DI teardown (carries Gap #52 pattern).

---

## §7 — Item #6: Orchestrator Audit Posture

- **Resolution:** Audit ONLY Startup + Shutdown + ChainVerifyFailedAtStartup (3 entries possible per session).
- **Rationale:** Avoids double-counting with TrayHost (Gap #57) and ReminderScheduler (already audits each fire).
- **Files:** `Orchestrator.cs`.
- **Gap for Handoff:** **Cross-Phase Gap #66 — Phase 4A Compliance audit policy must document this scope.** Specifically: "Orchestrator emits lifecycle audit entries only (Startup, Shutdown, ChainVerifyFailedAtStartup). Per-module audit is the responsibility of each emitting module."

---

## §8 — Item #7: CompositionRoot vs ServiceRegistrations

- **Resolution:** CR = environment-specific (paths + container build); SR = DI registration extension method.
- **Rationale:** Idiomatic .NET 8 pattern. SR is testable in isolation.
- **Files:** `CompositionRoot.cs` + `ServiceRegistrations.cs`.
- **Gap for Handoff:** None.

---

## §9 — Item #8: Canonical Paths

- **Resolution:** 3 static helpers in CompositionRoot returning `%LOCALAPPDATA%\TaskTree\{keys,store,logs}\` with idempotent `Directory.CreateDirectory`.
- **Files:** `CompositionRoot.cs`.
- **Gap for Handoff:** Carries Gaps #4 + #5 to closure for Phase 1F.

---

## §10 — Item #9: DI Lifetimes

- **Resolution:** All 11 services = `AddSingleton`.
- **Rationale:** Stateful modules per §3.2 (in-memory tree, last-fired dict, NotifyIcon handle).
- **Files:** `ServiceRegistrations.cs`.
- **Gap for Handoff:** PhiRedactor + AuditChainWriter are NOT registered (consumed only by ComplianceCore factory). If Phase 2A or later needs direct access, register at that time.

---

## §11 — Item #10: Factory Lambdas

- **Resolution:** `AddSingleton<T>(sp => new T(...))` with path strings from CompositionRoot helpers.
- **Rationale:** No `Microsoft.Extensions.Options` dependency added.
- **Files:** `ServiceRegistrations.cs`.
- **Gap for Handoff:** **Cross-Phase Gap #68 — Phase 5B compile gate verifies actual Phase 1C ctor signatures match factory lambdas.** If `PhiRedactor`, `AuditChainWriter`, or `ComplianceCore` ctors differ from PHASE1C-DERIVATIONS R10 documentation, Codex updates factory lambdas at Phase 5B.

---

## §12 — Item #11: Q11 PhiRedactor Allowlist

- **Resolution:** `Array.Empty<string>()` + `IAppLogger.LogWarning` on startup citing Q11.
- **Rationale:** No synthetic data hard-coded (D7). Owner-pending allowlist.
- **Files:** `ServiceRegistrations.cs` IComplianceCore factory.
- **Gap for Handoff:** **Cross-Phase Gap #67 — Owner MUST populate Q11 allowlist OR explicitly accept empty default before Phase 5F sign-off.** Currently logged every startup; Phase 5F sign-off process surfaces this for decision.

---

## §13 — Item #12: csproj Wiring

- **Resolution:** TaskTree.App.csproj patched with 3 PackageReferences + 7 ProjectReferences. TaskTree.Orchestrator.csproj patched with 6 ProjectReferences.
- **Files:** Both csproj.
- **Gap for Handoff:** None — `dotnet build` verifies at Phase 5B.

---

## §14 — Item #13: Namespace/Sealed

- **Resolution:** `public sealed class Orchestrator : IOrchestrator, IDisposable` in `namespace TaskTree.Orchestrator`.
- **Rationale:** Consistent with all Phase 1 modules.
- **Files:** `Orchestrator.cs`.
- **Gap for Handoff:** None.

---

## §15 — Item #14: Msg Structure

- **Resolution:** Msg 1 = production + 2 csproj + derivations + HANDOFF v1.0.20 + tools update. Msg 2 = OrchestratorTests + csproj patch + Architecture v1.0.2 delta UPDATE (Gap #63) + PHASE1F-MSG2 delta + HANDOFF v1.0.21.
- **Rationale:** Mirrors Phase 1D/1E precedent.
- **Files:** None (process).
- **Gap for Handoff:** None.

---

## §16 — Item #15: PowerShell Update

- **Resolution:** Add `'SPEC-DERIVED-PHASE1F' = 4` to expected hashtable.
- **Rationale:** 4 distinct .cs files carry the marker (IOrchestrator patched + 3 new).
- **Files:** `tools/find-spec-derivations.ps1`.
- **Gap for Handoff:** None. Phase 1F Msg 2 adds PHASE1F-MSG2 bucket for test files.

---

## §17 — Item #16: Architecture v1.0.2 Scope Expansion (again)

- **Trigger:** Item #1 raises IOrchestrator formalization need.
- **Resolution:** Architecture v1.0.2 delta document grows to **4 bundled additive non-breaking changes** in Phase 1F Msg 2:
  1. §3.3 ReminderReason add (REQUIRED — Phase 1D Msg 1)
  2. §4.3 Cadence clamp prose (OPTIONAL — Phase 1D Msg 1)
  3. §4.1 TrayHost internal-raise blessing (REQUIRED — Phase 1E Msg 1)
  4. **§4 IOrchestrator subsection** (NEW — Phase 1F Msg 1)
- **Files:** `docs/Architecture.v1.0.2-delta.md` (UPDATED in Msg 2).
- **Gap for Handoff:** **Cross-Phase Gap #63 — Phase 1F Msg 2 emits updated v1.0.2 delta document.** Owner sign-off required pre-Phase 5F.

---

## §18 — Item #17: Chain Integrity at Startup

- **Trigger:** §10.7 "Chain verify on app startup; alert + audit on mismatch."
- **Architecture silence:** No module ownership.
- **Options:** A) Orchestrator verifies first · B) ComplianceCore self-verifies on construction · C) Defer to Phase 2F.
- **Resolution:** Option A. Log + audit failure but DO NOT abort (first-run state has empty chain).
- **Rationale:** Orchestrator is lifecycle owner.
- **Files:** `Orchestrator.cs` StartAsync.
- **Gap for Handoff:** Phase 2B will add ChainVerifyFailed event surfacing (carries Cross-Phase Gap #9). Phase 1F audits the failure but doesn't propagate to UI/user.

---

## §19 Files Produced This Msg

| Path | Purpose | Marker |
|---|---|---|
| `src/TaskTree.Core/Abstractions/IOrchestrator.cs` | PATCHED — 2 method declarations | `PHASE1F` |
| `src/TaskTree.Orchestrator/Orchestrator.cs` | Lifecycle coordinator | `PHASE1F` |
| `src/TaskTree.App/Bootstrap/CompositionRoot.cs` | Path helpers + container build | `PHASE1F` |
| `src/TaskTree.App/Bootstrap/ServiceRegistrations.cs` | DI singleton registrations | `PHASE1F` |
| `src/TaskTree.App/TaskTree.App.csproj` | PATCHED — DI + Logging + 7 ProjectRefs | (csproj) |
| `src/TaskTree.Orchestrator/TaskTree.Orchestrator.csproj` | PATCHED — 6 ProjectRefs | (csproj) |

Test file deferred to Msg 2. Architecture v1.0.2 delta UPDATE deferred to Msg 2.

---

## §20 SPEC-DERIVED-PHASE1F Marker Inventory

| File | Marker count |
|---|---|
| `IOrchestrator.cs` (patched) | 1 |
| `Orchestrator.cs` | 1 |
| `CompositionRoot.cs` | 1 |
| `ServiceRegistrations.cs` | 1 |
| **Total distinct .cs files** | **4** |

`tools/find-spec-derivations.ps1` updated to assert `PHASE1F = 4`. Msg 2 introduces `PHASE1F-MSG2` bucket for test files.

---

## §21 Cross-Phase Gaps Introduced (rows 63-70)

| # | Gap | Source | Target Phase | Action |
|---|---|---|---|---|
| 63 | Architecture v1.0.2 must add §4 IOrchestrator subsection | HALT #1 + #16 | Architecture v1.0.2 | Phase 1F Msg 2 emits updated delta document |
| 64 | TrayHost.Initialize NotImplementedException catch in StartAsync | HALT #3 | Phase 5E | Codex removes try/catch when Initialize wired live |
| 65 | Placeholder ReminderDue handler — log only | HALT #4 | Phase 1G | ReminderDeliveryService unsubscribes placeholder + subscribes Tier 1/2/3 chain |
| 66 | Orchestrator audits ONLY 2 lifecycle events (+ChainVerifyFail) | HALT #6 | Phase 4A | Compliance audit policy documents scope |
| 67 | Q11 PhiRedactor allowlist empty + startup warning | HALT #11 / Q11 | Pre-Phase 5F | **OWNER MUST POPULATE OR ACCEPT EMPTY** |
| 68 | DI factory lambdas verify Phase 1C ctor signatures | HALT #10 | Phase 5B | Codex updates factories if PhiRedactor/AuditChainWriter/ComplianceCore ctors differ from PHASE1C R10 |
| 69 | `Clock` concrete class assumed in Phase 0 Msg 5 | HALT #9 | Phase 5B | Verify shipped name; rename in SR if different |
| 70 | Phase 1H E2E test exercises full DI graph | All HALT items + P1F-AC2 | Phase 1H | Confirm Build → StartAsync → manual Raise → handler → StopAsync; ≥3 audit entries (chain, Startup, Shutdown) |

---

## §22 Phase 5B / 5E Verification Additions

**Phase 5B:**
- Verify Phase 1C ctor signatures match ServiceRegistrations factory lambdas (Gap #68).
- Verify Phase 0 Msg 5 shipped `Clock` class (Gap #69).
- Verify `Microsoft.Extensions.DependencyInjection` 8.0.0 + `Microsoft.Extensions.Logging` 8.0.0 NuGet restore succeeds.
- Verify CompositionRoot.BuildServiceProvider(validateScopes:true) does NOT throw lifetime mismatch.

**Phase 5E:**
- Replace TrayHost.Initialize NotImplementedException catch (Gap #64).
- Replace placeholder ReminderDue handler with Phase 1G ReminderDeliveryService (Gap #65 + Phase 1G scope).
- Verify live audit posture: Orchestrator emits only lifecycle events (Gap #66).

---

## §23 Phase 1G Handoff Notes

Phase 1G `ReminderDeliveryService` will need to:
- Ctor: `(IReminderScheduler, ITrayHost, IClock, IAppLogger)`.
- Subscribe to `ReminderScheduler.ReminderDue`.
- Apply Tier 1/2/3 decision per §7 (Toast API → WPF custom → Tray balloon).
- Either: (a) Phase 1F placeholder handler must be UNSUBSCRIBED first (modify Orchestrator), OR (b) Phase 1G handler runs as additional subscriber (placeholder log is harmless duplicate).
- DI registration: register AFTER ReminderScheduler; can be before or after Orchestrator depending on subscription approach (a vs b).
- Recommended approach (a) — clean ownership; Phase 1G owns ReminderDue subscription, Orchestrator just owns the scheduler lifecycle.

---

## §24 Known Limitations

1. ServiceRegistrations factory lambdas assume Phase 1C ctor signatures from PHASE1C-DERIVATIONS R10 documentation; Phase 5B verifies (Gap #68).
2. `Clock` concrete class name assumed (Phase 0 may have shipped `SystemClock` or similar) (Gap #69).
3. Orchestrator.Dispose uses sync-over-async `StopAsync().GetAwaiter().GetResult()` pattern (matches Phase 1D ReminderScheduler precedent); Phase 5D verifies under live DI teardown.
4. Q11 empty allowlist triggers warning log every startup until owner populates (Gap #67).
5. Architecture v1.0.2 amendment now bundles 4 changes — owner sign-off may stall on partial approval; Phase 5F enforces.
6. NotImplementedException catch in StartAsync is narrowly scoped (only NotImplementedException; not parent Exception) — preserves anti-drift; any OTHER exception aborts startup as expected (Gap #64).
7. Orchestrator does NOT wire UI (TreeViewUI deferred to Phase 2B); ShowTreeRequested handler logs only and does nothing visible.
