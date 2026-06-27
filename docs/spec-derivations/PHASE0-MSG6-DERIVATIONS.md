# Phase 0 — Msg 6 Spec-Derived Test Conventions

**Status:** approved by Dominic Sarria-Wiley on 2026-05-26
**Grep marker:** `SPEC-DERIVED-MSG6`
**Total derived items:** 4
**Companion docs:** Architecture.md **v1.0.1** (amended this Msg via `docs/Architecture.v1.0.1-delta.md`), Roadmap.md v1.0.0, HANDOFF.md v1.0.5, `PHASE0-MSG2/3/4/5-DERIVATIONS.md`.

---

## Why this file exists

Architecture.md v1.0.0 is silent on test naming, test category vocabulary, and the MSTest infrastructure adapter. Architecture.md v1.0.1 (this Msg) expands §3.3 to add two test projects (`TaskTree.Modules.TrayHost.Tests`, `TaskTree.UI.Tests`). All four items are recorded here for handoff traceability.

## How to use this file at handoff

1. `grep -r "SPEC-DERIVED-MSG6" tests/` — locates every test file using the derived naming / category convention.
2. `grep -r "SPEC-DERIVED-MSG6" docs/` — locates this registry.
3. Cross-check entries below against Phase 1A-3F test files and Phase 5C coverage gate.
4. Promotion path: amend Architecture.md, bump to v1.0.x, record Superseded-by row.

---

## Derivation Registry

### 1. Test naming convention

| Field | Value |
| --- | --- |
| Convention | `MethodName_Scenario_ExpectedOutcome` |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "Issue 1: B, and log all gaps thoroughly along with an updated Architecture.md. Issue 2: Approve all four minor items as proposed with thorough logging of gaps for later phases and handoff" |
| Source-of-truth | Architecture is silent. Roadmap shows test file names but not method names. |
| Rationale | Standard MSTest idiom; self-documenting; survives renaming without losing intent. |
| Where applied | All test methods in all 10 test projects, starting with Msg 6's 5 primitives. |
| Re-validation tasks for handoff agent | • If a different convention is preferred at scale (e.g., Given_When_Then BDD style), promote via Architecture amendment.<br>• Confirm test runner reports surface the convention cleanly in CI output. |
| Risk if convention is wrong | Inconsistent naming → harder to navigate test failures at scale; no functional risk. |
| Promotion path | Amend Architecture §12 (or a new §22) per §Governance. |

### 2. Test category vocabulary

| Field | Value |
| --- | --- |
| Categories | `Offline` (default, no `[TestCategory]` attribute needed); `Live` (requires real Windows OS APIs — finalized in Codex Phase 5E); `Integration` (cross-module but still offline). |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "Issue 1: B, and log all gaps thoroughly along with an updated Architecture.md. Issue 2: Approve all four minor items as proposed with thorough logging of gaps for later phases and handoff" |
| Source-of-truth | Roadmap §20.6 references `--filter Category!=Live`; Roadmap 1B notes "DPAPI tests marked `[TestCategory(\"Live\")]`". Architecture does not define the full vocabulary. |
| Rationale | Three categories cover the documented split (offline-default + opt-in Live). `Integration` added so cross-module tests (Phase 1H, 3F) can be filtered separately from pure unit tests. |
| Where applied | Test methods touching real OS APIs (DPAPI, NotifyIcon, hotkey PInvoke, SMTP, GitHub HTTP) MUST carry `[TestCategory("Live")]`. Cross-module end-to-end tests MUST carry `[TestCategory("Integration")]`. Default (no attribute) = `Offline`. |
| Re-validation tasks for handoff agent | • Confirm all Live tests are correctly attributed before Phase 5C runs `dotnet test --filter Category!=Live`.<br>• If a fourth category emerges (e.g., `Perf` for Phase 4B), document HERE before adding. |
| Risk if vocabulary is wrong | Live tests run during offline-only CI → spurious failures; Integration tests skipped during gates → silent regressions. |
| Promotion path | Amend Architecture §12 (or new §22) per §Governance. |

### 3. §3.3 expansion (`TaskTree.Modules.TrayHost.Tests` + `TaskTree.UI.Tests`)

| Field | Value |
| --- | --- |
| Files added | `tests/TaskTree.Modules.TrayHost.Tests/`, `tests/TaskTree.UI.Tests/` |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "Issue 1: B, and log all gaps thoroughly along with an updated Architecture.md. Issue 2: Approve all four minor items as proposed with thorough logging of gaps for later phases and handoff" |
| Source-of-truth | Architecture §3.3 v1.0.0 listed 8 test projects; Roadmap 1E/2A reference `TrayHost.Tests`; Roadmap 2B/2C/2D/2E reference `UI.Tests`. |
| Spec gap | Inter-document inconsistency between Architecture §3.3 and Roadmap test file references. |
| Resolution | Promoted to Architecture.md **v1.0.1** via §Governance amendment (see `docs/Architecture.v1.0.1-delta.md`). |
| Risk if expansion is wrong | Phase 1E/2A/2B/2C/2D/2E sub-phases blocked at handoff. |
| Promotion path | Already promoted (v1.0.1). Future Perf.Tests promotion will be v1.0.2 at Phase 4B start. |

### 4. `Microsoft.NET.Test.Sdk` infrastructure dependency

| Field | Value |
| --- | --- |
| Package | `Microsoft.NET.Test.Sdk` 17.11.1 |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "Issue 1: B, and log all gaps thoroughly along with an updated Architecture.md. Issue 2: Approve all four minor items as proposed with thorough logging of gaps for later phases and handoff" |
| Source-of-truth | §12 lists MSTest 3.x but not the test-runner adapter that hosts MSTest under `dotnet test`. |
| Rationale | `Microsoft.NET.Test.Sdk` is MSTest's standard host on .NET. Without it, `dotnet test` cannot discover or execute tests. Treated as required infrastructure for MSTest (not as an app library subject to D3). |
| Where applied | Every test csproj (10 of 10). |
| Re-validation tasks for handoff agent | • Confirm the 17.11.x version is current at handoff (Phase 5B). If MSTest 3.x has a newer matching adapter, upgrade in lockstep — adapter and framework versions should track. |
| Risk if missing | `dotnet test` reports zero tests found. |
| Promotion path | Add an explicit row to Architecture §12 if formal documentation is preferred. |

---

## Cross-reference table

| Convention / Item | In-code marker file(s) | Affects |
| --- | --- | --- |
| Test naming convention | `tests/TaskTree.Core.Tests/Security/AesGcmCryptoProviderTests.cs`, `HashChainTests.cs`, `Logging/FileAppLoggerTests.cs` | All test methods, all phases |
| Test category vocabulary | (same 3 files) | All test methods, all phases |
| §3.3 expansion (Architecture v1.0.1) | `docs/Architecture.v1.0.1-delta.md` | Phase 1E, 2A, 2B, 2C, 2D, 2E |
| `Microsoft.NET.Test.Sdk` dependency | Every test csproj | Phase 5B build, Phase 5C test runs |

---

## Promotion / change procedure

Same as Msg 2–5 registries: open a §Governance amendment against Architecture.md, bump to v1.0.x, add Superseded-by row, update HANDOFF.md §Decisions Log + §Gap Summary, re-run `tools/find-spec-derivations.ps1`.

---

## Document history

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 1.0.0 | 2026-05-26 | Dominic Sarria-Wiley (via Claude Opus) | Initial registry — 4 conventions/items recorded during Phase 0 Msg 6. |
