# PHASE1G-MSG2-DERIVATIONS-DELTA.md - Phase 1G Msg 2

> **Scope:** 5 production patches + 2 test skeletons (full tests deferred per Gap #95) + 4 docs.
> **Companion:** PHASE1G-DERIVATIONS.md (Msg 1 base).
> **Owner-approved HALT batch:** 18 items.

## Section 1 Summary Table

| # | Item | Resolution | Files | LOAD-BEARING? |
|---|---|---|---|---|
| 1 | RDS interface promotion | New IReminderDeliveryService (reverses Msg 1 #2) | IRDS.cs + RDS.cs patched | Yes Gap #86 |
| 2 | Test file structure | Single ReminderDeliveryServiceTests.cs | (deferred) | No |
| 3 | Test count | 14 tests planned | (deferred) | No |
| 4 | IComplianceCore mock | Strict + Setup AuditAsync | (deferred) | No |
| 5 | IReminderScheduler mock | Strict + SetupAdd/Remove | (deferred) | No |
| 6 | Adapter mock strategy | Real instances; Tier 1/3 throw NIE | (deferred) | No Gap #88 |
| 7 | IClock | FakeClock 6th consumer | (deferred) | No Gap #89 |
| 8 | IAppLogger mock | Loose | (deferred) | No |
| 9 | Cascade tests | 4 input combos via SelectTier | RDS.cs SelectTier | No Gap #91 |
| 10 | InternalsVisibleTo | AssemblyInfo.cs | NEW | No Gap #92 |
| 11 | Null-arg test | 7 paths | (deferred) | No |
| 12 | Lifecycle tests | StartAsync/StopAsync | (deferred) | No |
| 13 | Dispose tests | 2 patterns | (deferred) | No |
| 14 | Audit verification | Tier1/3 log paths + audit entries | (deferred) | No |
| 15 | Manual event raise | Moq.Raise pattern | (deferred) | No |
| 16 | Orchestrator patch | 7-param ctor + RDS wiring | Orchestrator.cs | Yes Gap #83 |
| 17 | Phase 1F tests patch | 14 tests deferred | (deferred) | Yes Gap #93 |
| 18 | Architecture v1.0.2 | 6 bundled changes | Architecture.v1.0.2-delta.md | No Gap #94 |

## Sections 2-19 Per-Item

Each item per HALT batch documented. See HANDOFF v1.0.23 §C Decisions Log R18 for resolutions.

## Section 20 Files Produced

| Path | Purpose | Marker |
|---|---|---|
| src/TaskTree.Core/Abstractions/IReminderDeliveryService.cs | NEW interface | PHASE1G-MSG2 |
| src/TaskTree.Orchestrator/ReminderDeliveryService.cs | PATCHED unsealed + interface + SelectTier | PHASE1G + PHASE1G-MSG2 |
| src/TaskTree.Orchestrator/Properties/AssemblyInfo.cs | NEW InternalsVisibleTo | PHASE1G-MSG2 |
| src/TaskTree.Orchestrator/Orchestrator.cs | PATCHED 7-param + RDS wiring | PHASE1F + PHASE1G-MSG2 |
| src/TaskTree.App/Bootstrap/ServiceRegistrations.cs | PATCHED 4 new singletons | PHASE1F + PHASE1G-MSG2 |
| tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs | SKELETON (Gap #93/#95 deferred) | PHASE1G-MSG2 |
| tests/TaskTree.Orchestrator.Tests/ReminderDeliveryServiceTests.cs | SKELETON (Gap #95 deferred) | PHASE1G-MSG2 |
| docs/spec-derivations/PHASE1G-MSG2-DERIVATIONS-DELTA.md | This registry | (md) |
| docs/HANDOFF-v1.0.23-delta.md | Additive delta | (md) |
| docs/Architecture.v1.0.2-delta.md | UPDATED 6 changes | (md) |
| tools/find-spec-derivations.ps1 | UPDATED 16 buckets | (script) |

## Section 21 Marker Inventory

PHASE1G bumped 6 -> 11 distinct .cs files (substring match catches PHASE1G-MSG2 files).
PHASE1G-MSG2 new = 6 distinct .cs files.
Grand total bucket sum: 70.

## Section 22 Cross-Phase Gaps Introduced (rows 86-95)

| # | Gap | Source | Target Phase | Action |
|---|---|---|---|---|
| 86 | IReminderDeliveryService interface (reverses Msg 1 HALT #2) | HALT-Msg2 #1 | Architecture v1.0.2 | 6th bundled change |
| 87 | Tier 2 WPF tests deferred to integration | HALT-Msg2 #2 | Phase 5E | Live env tests |
| 88 | Tier 1/3 success-path tests deferred | HALT-Msg2 #6 | Phase 5E | Interface promotion + mocks |
| 89 | 6th FakeClock consumer - Option B promotion REQUIRED | HALT-Msg2 #7 | Phase 2A/2B | New TaskTree.TestSupport project |
| 90 | Cascade success-path tests deferred | HALT-Msg2 #9 | Phase 5E | Tier interface mocks needed |
| 91 | SelectTier helper reserved for Phase 5E | HALT-Msg2 #9 | Phase 5E | Wire real Toast/FocusAssist detection |
| 92 | Orchestrator InternalsVisibleTo pattern | HALT-Msg2 #10 | Phase 5B | Compile gate verifies 1 attribute line |
| 93 | Phase 1F R16 retroactive 7-param refactor | HALT-Msg2 #17 | HANDOFF v1.0.23 §I | Documented |
| 94 | Architecture v1.0.2 6 bundled changes | HALT-Msg2 #18 | Architecture v1.0.2 | Owner sign-off 6 groups |
| 95 | Phase 1G Msg 2 14+14 tests deferred to Codex | (skeleton emission) | Codex/Claude Code | Implement per plan |

## Section 23 Patches Applied to Msg 1 Production Code

| # | File | Patch |
|---|---|---|
| 1 | src/TaskTree.Orchestrator/ReminderDeliveryService.cs | Removed sealed; added IReminderDeliveryService interface; added internal Tier enum + SelectTier static helper |
| 2 | (none — interface is NEW file, not patch) | — |
| 3 | src/TaskTree.Orchestrator/Properties/AssemblyInfo.cs | NEW file |

## Section 24 Phase 1F Patches Applied

| # | File | Patch |
|---|---|---|
| 1 | src/TaskTree.Orchestrator/Orchestrator.cs | Ctor 6 -> 7 params; remove OnReminderDuePlaceholder; add RDS.Start after RS.Start; add RDS.Stop before RS.Stop in reverse-order shutdown |
| 2 | src/TaskTree.App/Bootstrap/ServiceRegistrations.cs | Add 4 new singletons (3 adapters + IRDS); update IOrchestrator to 7-param |
| 3 | tests/TaskTree.Orchestrator.Tests/OrchestratorTests.cs | DEFERRED to Codex per Gap #95 (skeleton emitted) |

## Section 25 Known Limitations

1. 14+14 ReminderDeliveryServiceTests + OrchestratorTests test execution deferred to Codex/Claude Code (Gap #95). Test plan is fully specified in HALT-Msg2 batch + section 19 above.
2. Tier 2 WPF adapter unit tests deferred (Gap #87) - requires STAThread Application context.
3. Tier 1/3 success-path tests deferred (Gap #88) - requires interface promotion or unsealing of adapters.
4. FakeClock 6th consumer (Gap #89) - Option B promotion REQUIRED at Phase 2A/2B.
5. SelectTier helper is pure §7 reference reserved for Phase 5E wiring (Gap #91).
6. Architecture v1.0.2 amendment now 6 bundled changes - owner sign-off block expanded.
7. Phase 1F R16 decisions retroactively updated for 7-param Orchestrator ctor (Gap #93).
8. Phase 1H E2E gate (Gap #70) must verify the full DI graph including new ReminderDeliveryService registrations.
