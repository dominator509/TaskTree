# PHASE1E-MSG2-DERIVATIONS-DELTA.md — Phase 1E Msg 2 (Tests + Architecture v1.0.2 Update)

> **Scope:** TrayHostTests + HotkeyInteropTests + csproj patch + Architecture v1.0.2 delta document update (Gap #60 closure).
> **Companion to:** PHASE1E-DERIVATIONS.md (Msg 1 base).
> **Companion docs:** Architecture.md v1.0.1 (v1.0.2 amendment SCOPE-FINAL pending owner sign-off), Roadmap.md v1.0.0, HANDOFF.md v1.0.19.
> **Owner-approved HALT batch:** 13 items, batch-resolved per proposals.

---

## §1 Summary Table

| # | Item | Resolution | Files | LOAD-BEARING? |
|---|---|---|---|---|
| 1 | Test count | 16 total (10 + 6) | Both test files | No |
| 2 | Test file split | 2 files (mirrors Phase 1D Msg 2) | Both | No |
| 3 | IComplianceCore mock | **Moq Strict — Gap #57 compile-enforced** | TrayHostTests | No (drift detector → Gap #62) |
| 4 | IAppLogger mock | Moq Loose; no verification | TrayHostTests | No |
| 5 | NotImplementedException text | `Contains("HIGH:")` AND `Contains("Codex Phase 5E")` | Both | No (Gap #61) |
| 6 | Event raise mechanism | Direct internal `Raise*()` invocation | TrayHostTests #5-#7 | No |
| 7 | No-subscribers safety | 1 dedicated test (`?.Invoke` guard) | TrayHostTests #8 | No |
| 8 | Dispose test count | 2 tests (double-call + post-dispose-throws) | TrayHostTests #9, #10 | No |
| 9 | ShowBalloon validation matrix | DataRow ×6 + 1 valid-input test | TrayHostTests #3, #4 | No |
| 10 | BuildModifierFlags matrix | DataRow ×8 + constants + NoRepeat regression + §13 baseline | HotkeyInteropTests #1-#4 | No |
| 11 | Register/Unregister stubs | 2 tests (canonical text) | HotkeyInteropTests #5, #6 | No (Gap #61) |
| 12 | csproj wiring | 2 ProjectReferences (Core + TrayHost) | csproj (PATCH) | No |
| 13 | Architecture v1.0.2 update | 3 bundled changes (§3.3 + §4.3 + §4.1) | docs/Architecture.v1.0.2-delta.md | No (Gap #60 closure) |

---

## §2 — Item #1: Test Count

- **Trigger:** Owner discretion (Phase 1A=14, 1C=13, 1D=20).
- **Architecture silence:** No per-sub-phase coverage threshold.
- **Resolution:** 16 total (10 + 6).
- **Rationale:** Targets all 3 P1E-ACs + all carry-forward gaps from Msg 1.
- **Files affected:** Both test files.
- **Gap for Handoff:** None — Phase 1H gate (75%) estimated met at ~90% testable-surface coverage.

---

## §3 — Item #2: File Split

- **Trigger:** Roadmap §1E names `TrayHostTests.cs` only; module has 2 classes.
- **Options:** A) Single file · B) 2 files mirroring Phase 1D Msg 2.
- **Resolution:** Option B.
- **Rationale:** Phase 1C + 1D precedent; clean separation of concerns.
- **Files affected:** NEW both test files.
- **Gap for Handoff:** None.

---

## §4 — Item #3: `IComplianceCore` Moq Strict

- **Trigger:** Gap #57 says no `AuditAsync` calls in Phase 1E stubs.
- **Architecture silence:** No test guidance.
- **Options:** A) Strict (drift detection) · B) Loose (tolerant).
- **Resolution:** Option A.
- **Rationale:** Converts Gap #57 from documentation-only to **compile-time / test-time enforcement**. Any audit call addition pre-Phase 5E fails test immediately.
- **Files affected:** `TrayHostTests.cs` `Build()` helper.
- **Gap for Handoff (Codex/Claude Code):** **Cross-Phase Gap #62.** When Phase 5E Codex adds `AuditAsync` calls to live `Initialize`/`ShowBalloon`/event handlers, these tests will fail. Codex must:
  - (a) Switch `Build()` helper to `MockBehavior.Loose`, OR
  - (b) Add explicit `Setup` for `AuditAsync` calls, OR
  - (c) **Recommended** — Add new `Verify()` assertions for expected audit content (`Module`/`Action`/`Result`/`Timestamp`) — preserves drift detection while validating new contract.

---

## §5 — Item #4: `IAppLogger` Moq Loose

- **Resolution:** `MockBehavior.Loose`; no verification (matches Phase 1D Msg 2 #7).
- **Files affected:** `TrayHostTests.cs`.
- **Gap for Handoff:** None.

---

## §6 — Item #5: NotImplementedException Text Verification

- **Trigger:** HALT-Msg1 #4 established canonical text.
- **Options:** A) `Contains` semi-strict · B) Type only · C) Full exact match.
- **Resolution:** Option A — `Contains("HIGH:")` AND `Contains("Codex Phase 5E")`.
- **Rationale:** Enforces greppable pattern (Phase 5E closure) without locking exact wording.
- **Files affected:** TrayHostTests #2, #4; HotkeyInteropTests #5, #6.
- **Gap for Handoff (Codex/Claude Code):** **Cross-Phase Gap #61.** If canonical text changes, ALL 4 stub tests fail simultaneously — drift signal. Phase 5E must remove these tests after replacing stubs with real implementations (or convert to live integration tests).

---

## §7 — Item #6: Event Raise Mechanism

- **Resolution:** Direct internal `Raise*()` invocation via `InternalsVisibleTo`.
- **Files affected:** TrayHostTests #5, #6, #7.
- **Gap for Handoff:** Phase 5E live tests must verify events fire on real NotifyIcon click / hotkey press (separate integration tests). Internal `Raise*()` methods remain for unit-test event-handler verification.

---

## §8 — Item #7: No-Subscribers Safety

- **Resolution:** 1 test calls all 3 internal `Raise*()` methods without subscribers; asserts no throw.
- **Files affected:** TrayHostTests #8.
- **Gap for Handoff:** None — protects null-conditional `?.Invoke` pattern from regression.

---

## §9 — Item #8: Dispose Test Count

- **Resolution:** 2 tests.
- **Files affected:** TrayHostTests #9, #10.
- **Gap for Handoff:** Phase 5D live DI teardown still owed (carried Gap #52 from Phase 1D Msg 2 — same pattern applies to TrayHost).

---

## §10 — Item #9: ShowBalloon Validation Matrix

- **Resolution:** DataRow ×6 + 1 valid-inputs test.
- **Files affected:** TrayHostTests #3, #4.
- **Gap for Handoff:** Phase 5E live `ShowBalloon` may add length truncation policy (carried gap from PHASE1E §10). If Codex changes validation rules, this DataRow test must be updated.

---

## §11 — Item #10: BuildModifierFlags Matrix

- **Resolution:** DataRow ×8 + ModifierConstants + NoRepeat regression + §13 baseline = 4 tests.
- **Files affected:** HotkeyInteropTests #1, #2, #3, #4.
- **Gap for Handoff:** Microsoft Docs RegisterHotKey is the source of truth for modifier constant values. Extremely unlikely to change.

---

## §12 — Item #11: Register/Unregister Stub Verification

- **Resolution:** 2 tests.
- **Files affected:** HotkeyInteropTests #5, #6.
- **Gap for Handoff:** Tests will fail after Phase 5E replaces stubs with real PInvoke. Codex must replace these unit tests with live integration tests that register a real hotkey on a message-only HWND, verify message arrival in the message loop, then unregister.

---

## §13 — Item #12: csproj Wiring

- **Resolution:** 2 ProjectReferences (Core + TrayHost; no Core.Tests since no FakeClock needed — TrayHost doesn't depend on IClock).
- **Files affected:** PATCH csproj.
- **Gap for Handoff:** None — `dotnet build` verifies at Phase 5B.

---

## §14 — Item #13: Architecture v1.0.2 Update (Gap #60 Closure)

- **Trigger:** HALT-Msg1 #15 deferred prose update to Msg 2.
- **Resolution:** Updated `docs/Architecture.v1.0.2-delta.md` bundles 3 changes:
  1. **§3.3 Enums** — add `ReminderReason.cs` (Phase 1D Msg 1) — **REQUIRED**.
  2. **§4.3 ReminderScheduler** — optional `Cadence` clamp formalization (Phase 1D Msg 1) — **OPTIONAL**.
  3. **§4.1 TrayHost** — internal-raise blessing prose (Phase 1E Msg 1) — **REQUIRED IF PHASE 1E SHIPS**.
- **Files affected:** UPDATED `docs/Architecture.v1.0.2-delta.md`.
- **Gap for Handoff (Codex/Claude Code):** **Owner sign-off required before Phase 5F.** Without approval, `find-spec-derivations.ps1` still passes (markers independent of §3.3 listing), but `ReminderReason.cs` + TrayHost InternalsVisibleTo pattern exist in repo without Architecture acknowledgment — Phase 5F-blocking drift.

---

## §15 Files Produced This Msg

| Path | Purpose | Marker(s) |
|---|---|---|
| `tests/TaskTree.Modules.TrayHost.Tests/TrayHostTests.cs` | 10 tests | `PHASE1E-MSG2` |
| `tests/TaskTree.Modules.TrayHost.Tests/HotkeyInteropTests.cs` | 6 tests | `PHASE1E-MSG2` |
| `tests/TaskTree.Modules.TrayHost.Tests/TaskTree.Modules.TrayHost.Tests.csproj` | PATCHED — 2 ProjectReferences | (csproj — no marker) |
| `docs/spec-derivations/PHASE1E-MSG2-DERIVATIONS-DELTA.md` | This registry | (md) |
| `docs/HANDOFF-v1.0.19-delta.md` | Additive delta | (md) |
| `docs/Architecture.v1.0.2-delta.md` | UPDATED — 3 bundled changes | (md) |
| `tools/find-spec-derivations.ps1` | UPDATED — 12 marker buckets | (script) |

---

## §16 Marker Inventory Updates

| Bucket | Old (Msg 1) | New (Msg 2) | Notes |
|---|---|---|---|
| `SPEC-DERIVED-PHASE1E` | 3 | **5** | Substring match catches PHASE1E-MSG2 files too |
| `SPEC-DERIVED-PHASE1E-MSG2` | — | **2** | NEW bucket: TrayHostTests.cs + HotkeyInteropTests.cs |

`tools/find-spec-derivations.ps1` updated. Grand-total = **47** (bucket sum).

---

## §17 Cross-Phase Gaps Introduced (rows 61-62)

| # | Gap | Source | Target Phase | Action |
|---|---|---|---|---|
| 61 | Canonical NotImplementedException text contract enforced via `Contains()` | HALT-Msg2 #5 | Phase 5E | Codex replaces stubs and removes/updates the 4 text-verification tests |
| 62 | TrayHostTests Strict `IComplianceCore` mock breaks when audit wired | HALT-Msg2 #3 | Phase 5E | Codex: switch to Loose with `Setup` + `Verify` on new audit contract |

---

## §18 Patches to Msg 1 Production Code

**Zero patches required.** Msg 1 design was test-friendly from the start (`InternalsVisibleTo` + real `Dispose` + validation-before-stub-throw pattern). Phase 1E Msg 2 is cleaner than Phase 1D Msg 2 (which required 2 keyword changes to `ReminderScheduler.cs`).

---

## §19 Phase 1F Composition Root Checklist

Unchanged from PHASE1E-DERIVATIONS §20:
- 10. Register `ITrayHost` → `TrayHost` singleton with `(IAppLogger, IComplianceCore)`.
- 11. Orchestrator subscribes to `ShowTreeRequested` / `AddTaskRequested` / `ExitRequested` after `Initialize()` succeeds.
- 12. Orchestrator MUST NOT call internal `Raise*()` methods (InternalsVisibleTo scope enforces).

---

## §20 Phase 5D / 5E Verification Additions

**Phase 5D:**
- Live DI teardown for `TrayHost.Dispose` (carried Gap #52 pattern).

**Phase 5E:**
- Replace 4 stub-text-verification tests with live integration tests (Gap #61).
- Switch `IComplianceCore` mock to Loose + `Verify` on real audit calls (Gap #62).
- Add live Win32 message-loop integration test for `HotkeyInterop.Register`/`Unregister`.
- Add real NotifyIcon click → `ShowTreeRequested` integration test.

---

## §21 Test Category Usage

All 16 tests = `[TestCategory("Offline")]` per PHASE0-MSG6 R5. Cumulative CI cost < 100ms (no real-timer waits unlike Phase 1D). Well below Phase 4B threshold.

---

## §22 Coverage Targets (Informational)

| AC | Coverage |
|---|---|
| P1E-AC1 compile | Successful `Build()` instantiation in every test |
| P1E-AC2 events raise | TrayHostTests #5, #6, #7, #8 (4 tests) |
| P1E-AC3 live methods stubbed | TrayHostTests #2, #4; HotkeyInteropTests #5, #6 (4 tests) |

**Estimated module coverage:** TrayHost.cs ~90% (`Initialize` body / `ShowBalloon` body unreachable until live impl); HotkeyInterop.cs ~70% (`Register`/`Unregister` bodies unreachable). **Phase 1H gate (75%) met for testable surface.**

---

## §23 Known Limitations

1. `Initialize()` body cannot be tested (always throws) — Phase 5E live impl tests will replace.
2. `ShowBalloon()` body unreachable past validation — same as above.
3. `HotkeyInterop.Register`/`Unregister` bodies unreachable — live PInvoke needs Phase 5E.
4. BuildModifierFlags matrix uses 8 representative rows (not 2^4=16 exhaustive); Test #3 (NoRepeat regression) covers the remaining 8 implicitly via the bit-mask loop.
5. No test verifies `_initialized` field is unused in Phase 1E — accepted per Item #12 of Msg 1.
6. Test #10 (`Dispose_ThenAnyPublicMethod`) checks `Initialize` + `ShowBalloon` but NOT internal `Raise*()` methods — acceptable since internal methods are test-only and don't need `ObjectDisposedException` guards.
7. Internal `Raise*()` methods don't check `_disposed` — by design (test-only helpers). If Phase 5E ever exposes them publicly, `ThrowIfDisposed` must be added.
