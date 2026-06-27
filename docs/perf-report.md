# perf-report.md - Phase 4B Performance Optimization Report

> Source: Roadmap Phase 4B; Architecture.md Section 15.  
> Status: Preliminary benchmark/report scaffold. No performance pass/fail is claimed until Codex/Windows execution evidence is attached.

## 1. Executive Summary

Phase 4B adds an MSTest/Stopwatch-based performance benchmark scaffold and maps every Architecture Section 15 performance target to generated evidence, required execution environment, and Codex/Claude follow-up. No speculative production tuning patches were generated because the repo has not yet been stitched, built, or measured.

## 2. Evidence Status Labels

- Pending Codex execution
- Requires live Windows desktop
- Requires installed MSIX
- Requires Phase 5E live validation
- Requires Phase 5F final validation
- Measured in generated smoke benchmark only

## 3. Architecture Section 15 Performance Target Matrix

| Target | Architecture Threshold | Generated Benchmark / Evidence | Current Status | Gap |
|---|---:|---|---|---|
| Tray click -> GUI visible warm | < 200 ms | None in chat; live WPF/tray required | Requires Phase 5E/5F | #311 |
| Tray click -> event raised | < 50 ms | None in chat; live tray/event required | Requires Phase 5E/5F | #311 |
| TaskEngine CRUD | < 50 ms | `TaskEngine_AddUpdateDelete_SmokeUnderTarget` placeholder | Pending stitched benchmark | #305 |
| Full tree fetch <=1000 nodes | < 100 ms | `TaskEngine_Fetch1000Nodes_SmokeUnderTarget` placeholder | Pending stitched benchmark | #306 |
| ReminderScheduler tick eval | < 10 ms | `ReminderScheduler_TickEvaluation1000Tasks_SmokeUnderTarget` placeholder | Pending test seam / stitched benchmark | #307 |
| SecureStore read/write <=10 MB | < 100 ms | small payload smoke + 10 MB elapsed recording | Pending real filesystem/crypto benchmark | #308 |
| Audit write | < 20 ms | `Audit_Write1000_AverageUnder20Ms_Smoke`; Phase 4A stress tests | Pending real AuditChainWriter benchmark | #295 |
| Audit-chain verify 10k | < 500 ms | Phase 4A `AuditChain_Append10000_VerifyIntegrity_UnderTarget` | Pending Codex/Windows measurement | #295/#296 |
| Updater manifest check | < 2 s network | No live HTTP in chat | Requires Phase 5E live HTTP | #309 |
| Bug submit queued | < 50 ms | `BugReporter_SubmitQueued_SmokeUnderTarget` placeholder | Pending stitched benchmark | #310 |
| Idle RAM | < 80 MB | No chat measurement | Requires installed app | #302 |
| CPU at idle | < 0.5% | No chat measurement | Requires installed app | #302 |

## 4. Benchmarking Approach

Architecture Section 12 does not permit BenchmarkDotNet, so Phase 4B uses MSTest and `System.Diagnostics.Stopwatch`. The generated benchmarks are deliberately smoke-level scaffolds where stitched dependencies are unstable or unavailable. Codex/Claude must replace placeholders with real module calls where constructor/factory reconciliation permits.

## 5. Category Policy

Generated tests use:

- `Performance` for performance smoke tests.
- `Stress` for heavier workloads such as 10 MB or 100k audit-chain tests.
- `Live` for installed-app, UI, RAM, CPU, tray, and network tests.

Policy gap: Phase 5C/5F must define which categories run in CI versus release validation.

## 6. Tuning Patch Policy

No production tuning patches were emitted in Phase 4B. Tuning must be evidence-driven after the stitched repo is built and measured.

## 7. New Phase 4B Gaps

| Gap | Description | Target |
|---|---|---|
| #300 | Phase 4B cannot claim pass/fail until benchmarks run on stitched repo | Phase 5C/5F |
| #301 | Performance report preliminary until Codex/Windows execution evidence is attached | Phase 5C/5F |
| #302 | Idle RAM and CPU targets require live installed-app measurement | Phase 5E/5F |
| #303 | Architecture Section 3.3 must add tests/TaskTree.Perf.Tests | Architecture v1.0.3/5F |
| #304 | MSTest/Stopwatch used because BenchmarkDotNet is outside tech stack | Phase 5F |
| #305 | TaskEngine benchmark may require constructor/fake reconciliation | Phase 5B |
| #306 | 1000-node tree benchmark must run against stitched TaskEngine | Phase 5C/5F |
| #307 | ReminderScheduler tick benchmark may need a public/internal test seam | Phase 5B |
| #308 | SecureStore 10 MB benchmark requires real filesystem/crypto validation | Phase 5C/5F |
| #309 | Updater manifest check performance requires live HTTP measurement | Phase 5E/5F |
| #310 | BugReporter submit benchmark requires stitched dependency setup | Phase 5B/5C |
| #311 | Tray/UI latency targets require live WPF/tray validation | Phase 5E/5F |
| #312 | Phase 5F must enforce final performance thresholds with release-candidate evidence | Phase 5F |
| #313 | Define Performance/Stress/Live category execution policy | Phase 5C/5F |
| #314 | Tuning patches deferred until measured evidence exists | Phase 5B/5C/5F |

## 8. Codex/Claude Action Plan

1. Stitch repo in Phase 5A.
2. Reconcile constructors/fakes in Phase 5B.
3. Replace placeholder smoke benchmarks with real module calls where possible.
4. Run `dotnet test --filter TestCategory=Performance` after compile closure.
5. Run `Stress` category during release validation or dedicated CI lane.
6. Run `Live` category on installed Windows desktop in Phase 5E/5F.
7. Attach measured timings to this report before release sign-off.

## 9. Conclusion

Phase 4B provides performance benchmark scaffolding and an evidence map. It does not certify performance. Final pass/fail belongs to Phase 5F using release-candidate evidence.
