# perf-report.md - Phase 4B Performance Optimization Report

> Source: Roadmap Phase 4B; Architecture.md Section 15.  
> Status: Module-backed Release measurements captured on the current Windows host; installed-app and live network metrics remain open.

## 1. Executive Summary

Phase 4B now contains module-backed MSTest/Stopwatch measurements for TaskEngine, ReminderScheduler, SecureStore, AuditChainWriter, and BugReporter. The current Release run passed every local measurable case. Installed WPF/tray latency, idle RAM/CPU, and live updater/network timings still require the Phase 5E/5F environment.

## 2. Evidence Status Labels

- Pending Codex execution
- Requires live Windows desktop
- Requires installed MSIX
- Requires Phase 5E live validation
- Requires Phase 5F final validation
- Measured in module-backed Release benchmark

## 3. Architecture Section 15 Performance Target Matrix

| Target | Architecture Threshold | Generated Benchmark / Evidence | Current Status | Gap |
|---|---:|---|---|---|
| Tray click -> GUI visible warm | < 200 ms | None in chat; live WPF/tray required | Requires Phase 5E/5F | #311 |
| Tray click -> event raised | < 50 ms | None in chat; live tray/event required | Requires Phase 5E/5F | #311 |
| TaskEngine CRUD | < 50 ms | `TaskEngine_AddUpdateDelete_SmokeUnderTarget`: 0.3075 ms average | Passed local Release measurement | #305 closed locally |
| Full tree fetch <=1000 nodes | < 100 ms | `TaskEngine_Fetch1000Nodes_SmokeUnderTarget`: 0.3593 ms | Passed local Release measurement | #306 closed locally |
| ReminderScheduler tick eval | < 10 ms | `ReminderScheduler_TickEvaluation1000Tasks_SmokeUnderTarget`: 4.5148 ms | Passed local Release measurement | #307 closed locally |
| SecureStore read/write <=10 MB | < 100 ms each | 10 MB save: 53.4829 ms; load: 65.6757 ms | Passed local Release measurement | #308 closed locally |
| Audit write | < 20 ms | `Audit_Write1000_AverageUnder20Ms_Smoke`: 0.0229 ms average | Passed local Release measurement | #295 partial |
| Audit-chain verify 10k | < 500 ms | `AuditChain_Append10000_VerifyIntegrity_UnderTarget`: 107 ms | Passed local Release measurement | #295/#296 partial |
| Updater manifest check | < 2 s network | No live HTTP in chat | Requires Phase 5E live HTTP | #309 |
| Bug submit queued | < 50 ms | `BugReporter_SubmitQueued_SmokeUnderTarget`: 2.2137 ms average | Passed local Release measurement | #310 closed locally |
| Idle RAM | < 80 MB | No chat measurement | Requires installed app | #302 |
| CPU at idle | < 0.5% | No chat measurement | Requires installed app | #302 |

## 4. Captured Release Evidence

Run on 2026-08-16 with the installed .NET 8 SDK:

```text
dotnet test tests/TaskTree.Perf.Tests/TaskTree.Perf.Tests.csproj -c Release --no-build --filter TestCategory=Performance
  7 passed, 1 skipped (live RAM/CPU/tray/UI case)

dotnet test tests/TaskTree.Modules.ComplianceCore.Tests/TaskTree.Modules.ComplianceCore.Tests.csproj -c Release --no-build --filter FullyQualifiedName~AuditChain_Append10000_VerifyIntegrity_UnderTarget
  10k append+verify: 107 ms, passed

dotnet test TaskTree.sln -c Release --no-build --filter TestCategory=Stress
  100k append+verify: 1370 ms, passed
  10 MB SecureStore save: 53.4829 ms; load: 65.6757 ms, passed
```

The benchmark tests use the real module implementations with deterministic, non-PHI test doubles and interface-backed snapshots where the target is specifically an isolated evaluation loop. They do not claim installed-app behavior.

## 5. Benchmarking Approach

Architecture Section 12 does not permit BenchmarkDotNet, so Phase 4B uses MSTest and `System.Diagnostics.Stopwatch`. Module-backed cases call the real production classes; deterministic doubles isolate the contract being measured. Live UI, resource, network, and installed-package measurements remain environment gates.

## 6. Category Policy

Generated tests use:

- `Performance` for module-backed performance tests.
- `Stress` for heavier workloads such as 10 MB and 100k audit-chain tests.
- `Live` for installed-app, UI, RAM, CPU, tray, and network tests.

The local Release evidence runs `Performance` and `Stress`; `Live` remains a release-environment lane.

## 7. Tuning Patch Policy

No speculative production tuning patches were emitted. The module-backed measurements pass their local thresholds, so remaining tuning belongs after installed-app evidence rather than before it.

## 8. Remaining Phase 4B/5F Gaps

| Gap | Description | Target |
|---|---|---|
| #300 | Module-backed local pass is captured; release-candidate evidence still requires final environment validation | Phase 5F |
| #301 | Installed-app evidence remains open after local Windows-host benchmark execution | Phase 5E/5F |
| #302 | Idle RAM and CPU targets require live installed-app measurement | Phase 5E/5F |
| #303 | Architecture Section 3.3 must add tests/TaskTree.Perf.Tests | Architecture v1.0.3/5F |
| #304 | MSTest/Stopwatch used because BenchmarkDotNet is outside tech stack | Phase 5F |
| #305 | TaskEngine benchmark now uses the real TaskEngine with deterministic test support | Closed locally; retain final RC check |
| #306 | 1000-node tree benchmark now runs against the real TaskEngine | Closed locally; retain final RC check |
| #307 | ReminderScheduler benchmark now uses its internal deterministic tick seam | Closed locally; retain final RC check |
| #308 | SecureStore 10 MB benchmark now uses real DPAPI/AES-GCM/filesystem save and load | Closed locally; retain final RC check |
| #309 | Updater manifest check performance requires live HTTP measurement | Phase 5E/5F |
| #310 | BugReporter benchmark now uses the real queue/redaction/submit path | Closed locally; retain final RC check |
| #311 | Tray/UI latency targets require live WPF/tray validation | Phase 5E/5F |
| #312 | Phase 5F must enforce final performance thresholds with release-candidate evidence | Phase 5F |
| #313 | Define Performance/Stress/Live category execution policy | Phase 5C/5F |
| #314 | Tuning patches deferred until measured evidence exists | Phase 5B/5C/5F |

## 9. Codex/Claude Action Plan

1. Run the module-backed Performance category in the Release candidate.
2. Run the Stress category during release validation or a dedicated CI lane.
3. Run the Live category on an installed Windows desktop in Phase 5E/5F.
4. Attach RAM/CPU, tray/WPF latency, package, network, and compliance evidence before release sign-off.

## 10. Conclusion

Phase 4B now provides local module-backed performance evidence and an explicit remaining live-evidence map. It does not certify production or installed-binary performance; final pass/fail still belongs to Phase 5F using release-candidate evidence.
