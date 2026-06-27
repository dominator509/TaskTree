# Phase 1C — ComplianceCore Baseline Spec-Derived Surfaces

**Status:** approved by Dominic Sarria-Wiley on 2026-05-26
**Grep marker:** `SPEC-DERIVED-PHASE1C`
**Total derived items:** 8 (1 LOAD-BEARING: §6 PHI regex policy; 1 LOAD-BEARING: §7 support-email allowlist)
**Implementation status:** Msg 1 (`ComplianceCore.cs`) ✅; Msg 2 (`PhiRedactor.cs`) ✅; Msg 3 (`AuditChainWriter.cs`) ✅ — module code-complete; Msg 4 ✅; Msg 5 (`AuditChainWriterTests.cs`) ✅ — **all 13 approved tests live; Phase 1C COMPLETE.**
**Companion docs:** Architecture.md v1.0.1, Roadmap.md v1.0.0, HANDOFF.md v1.0.14, `PHASE0-MSG2/3/4/5/6-DERIVATIONS.md`, `PHASE1A-DERIVATIONS.md`, `PHASE1B-DERIVATIONS.md`.

---

## Why this file exists

§4.6 declares `IComplianceCore` and §10.5 specifies the audit hash formula,
but the implementing class structure, audit storage representation, sequence
policy, verify-failure behavior, PHI redaction regex set, support-email
allowlist seed, and null/empty redaction contract are not specified verbatim.

**Items §6 and §7 are LOAD-BEARING for HIPAA compliance.** §6 ships a Phase 1C
minimum-viable regex set with two explicitly-flagged knowledge gaps (Q10 + Q11
in HANDOFF.md) that the owner must close before Phase 5F sign-off.

## How to use this file at handoff

1. `grep -r "SPEC-DERIVED-PHASE1C" src/` — locates `ComplianceCore.cs` (Msg 1), `PhiRedactor.cs` (Msg 2), `AuditChainWriter.cs` (Msg 3).
2. `grep -r "SPEC-DERIVED-PHASE1C" tests/` — locates the 3 test files in Msgs 4-5.
3. `grep -r "SPEC-DERIVED-PHASE1C" docs/` — locates this registry + HANDOFF.md delta.
4. Cross-check against Phase 1F DI (must wire `ComplianceCore` + concrete
   sibling `PhiRedactor` + `AuditChainWriter`; verify support-email allowlist
   is populated or explicitly empty per Q11), Phase 2B/2E (chain-verify-failed
   UI), Phase 4A `AuditChainStressTests` (10k/100k entries; validate §15 perf),
   Phase 5F (close Q10 + Q11 before sign-off).
5. Promotion path: amend Architecture §4.6 / §9.2.3 / §10.5; bump to v1.0.2.

---

## Derivation Registry

### 1. ComplianceCore class structure

| Field | Value |
| --- | --- |
| Approved by / date | Dominic Sarria-Wiley, 2026-05-26 |
| Approval message | "Approve all 8 derivations and 13-test plan. Be sure to document all gaps thoroughly for later handoffs so that they may be filled by codex/claude code" |
| Source-of-truth | §4.6 (IComplianceCore stub) + §10.5 (audit schema). |
| Spec gap | Single monolithic class vs. delegate to siblings. |
| Chosen behavior | Constructor with 5 deps: `(ISecureStore, IClock, IAppLogger, PhiRedactor, AuditChainWriter)`. Delegates `AuditAsync` / `GetAuditChainAsync` / `VerifyChainIntegrityAsync` to `AuditChainWriter`. Delegates `RedactPhi` to `PhiRedactor`. `StartIdleMonitor` throws `NotImplementedException("Deferred to Phase 2F per Roadmap 1C")` per anti-drift D5. `AutoLogoffTriggered` event declared (matches §4.6 stub) but never raised in Phase 1C. |
| Alternatives rejected | Monolithic class (untestable; violates separation of concerns); extract `IPhiRedactor`/`IAuditChainWriter` interfaces now (premature, YAGNI per PHASE1B §1 precedent). |
| Downstream consumers | Phase 1F DI (must register `ComplianceCore` + `PhiRedactor` + `AuditChainWriter` as singletons); Phase 2F idle monitor wiring (must implement `StartIdleMonitor` AND raise `AutoLogoffTriggered`); Phase 4A `AuditChainStressTests`. |
| Re-validation tasks for handoff agent | • Confirm Phase 1F `ServiceRegistrations.cs` registers all three concrete classes (`ComplianceCore` + `PhiRedactor` + `AuditChainWriter`) as singletons.<br>• Confirm Phase 2F `IdleMonitor.cs` replaces the stubbed `StartIdleMonitor` AND raises `AutoLogoffTriggered`.<br>• If a second redactor or chain impl is ever needed (e.g., cloud-backed audit), extract interfaces via §Governance; do NOT modify these classes. |
| Risk if wrong | Monolithic class becomes untestable; idle monitor stub silently succeeds in Phase 2F leaving auto-logoff broken (HIPAA technical-safeguard failure). |
| Promotion path | Amend §4.6 stub remarks. |

### 2. AuditChainWriter C# surface

| Field | Value |
| --- | --- |
| Source-of-truth | §10.5 (hash formula) + PHASE0-MSG5 §2 (`HashChain` primitive). Class surface silent. |
| Spec gap | Class member surface. |
| Chosen behavior | `public sealed class AuditChainWriter`: `public const string StorageKey = "audit/chain"`; ctor `(ISecureStore, IClock, IAppLogger)`; methods `Task AppendAsync(AuditEntry)`, `Task<IReadOnlyList<AuditEntry>> GetAllAsync()`, `Task<bool> VerifyAsync()`. Single `SemaphoreSlim(1,1)` gate matching PHASE1A §6 + PHASE1B §5 conventions. |
| Alternatives rejected | Static class (Seq counter requires instance state); extracting `IAuditChainWriter` now (YAGNI per PHASE1B §1 precedent). |
| Downstream consumers | `ComplianceCore` (Msg 1); Phase 4A `AuditChainStressTests`. |
| Re-validation | • Confirm Phase 4A 100k stress test meets §15 verify < 5 s; if it fails, storage format may need chunking (see §3).<br>• Confirm `SemaphoreSlim` gate doesn't deadlock under reentry from `ComplianceCore` synchronous callbacks. |
| Risk if wrong | Audit chain integrity gap; HIPAA technical-safeguard failure. |
| Promotion path | Amend §10.5 to declare `AuditChainWriter` surface; v1.0.2 candidate. |

### 3. Audit chain SecureStore key + storage representation

| Field | Value |
| --- | --- |
| Source-of-truth | §4.5 silent on key; PHASE1B §2 sanitization rule applies. |
| Spec gap | Storage key + on-disk format. |
| Chosen behavior | Key `"audit/chain"` (sanitized to `audit__chain.bin` per PHASE1B §2). Format: flat `List<AuditEntry>` wrapped in a private DTO `AuditChainDto` with a single property `Entries` of type `List<AuditEntry>` — same pattern as `TaskMap` from PHASE1A §5. **Entire chain rewritten on every append** (ISecureStore is key/value, not append-only). |
| Alternatives rejected | Append-only file (ISecureStore is key/value, not append-stream); per-entry files (key-collision risk; per-key gate overhead). |
| Downstream consumers | `AuditChainWriter` (Msg 3); Phase 4A perf tests; Phase 5C coverage gate. |
| Re-validation | • **CRITICAL:** If Phase 4A 100k stress test exceeds §15 < 5 s verify OR < 20 ms append, storage format MUST change to chunked or append-only via §Governance amendment.<br>• Confirm no other module reuses the `"audit/chain"` key. |
| Risk if wrong | Perf regression on growing chains; chain corruption on partial-write (mitigated by PHASE1B §2 atomic temp→rename). |
| Promotion path | Amend §4.5 storage-key conventions + §10.5 storage format. |

### 4. Sequence numbering policy

| Field | Value |
| --- | --- |
| Source-of-truth | §10.5 schema says `seq` is "monotonically increasing" but no policy. |
| Spec gap | Caller-assigned vs writer-assigned; starting value. |
| Chosen behavior | `AuditChainWriter.AppendAsync` **overwrites** `entry.Seq` with `lastSeq + 1` even if caller pre-populates it. First entry `Seq = 1`; `PrevHash = HashChain.GenesisPrevHash = ""`. Stricter than PHASE1A §3 Id auto-generation because monotonicity is a security invariant. |
| Alternatives rejected | Honor caller-supplied `Seq` (security risk: caller could inject out-of-order entry); start `Seq = 0` (off-by-one inconsistency with §10.5 `"seq": 12345` example which implies 1-based). |
| Downstream consumers | `AuditChainWriter`; Phase 4A stress tests. |
| Re-validation | • Confirm Phase 4A tests assert `Seq` monotonicity over 10k entries.<br>• Confirm no caller (TaskEngine PHASE1A §4 audit pipeline; future modules) relies on pre-setting `Seq`. |
| Risk if wrong | Chain integrity holes; replay-attack surface. |
| Promotion path | Amend §10.5 stub remarks. |
| Verified by | `AuditChainWriterTests.AppendAsync_AssignsMonotonicallyIncreasingSeq` + `AppendAsync_FirstEntry_UsesGenesisPrevHash` (Msg 5). |

### 5. VerifyChainIntegrityAsync failure-mode behavior

| Field | Value |
| --- | --- |
| Source-of-truth | §4.6 returns `bool`; §10.9 "user-visible warning + export" is documented future behavior. |
| Spec gap | Side effects on verify-fail in Phase 1C. |
| Chosen behavior | Returns `bool` only; NO side effects in Phase 1C. The §10.9 export-on-failure is Phase 2B/2E (UI warning) + Phase 4A (export tool) scope. |
| Alternatives rejected | Side-effect-on-fail in Phase 1C (overreach for "baseline" per Roadmap 1C scope). |
| Downstream consumers | `ComplianceCore.VerifyChainIntegrityAsync` delegate; Phase 2B/2E UI warning; Phase 4A audit export tool. |
| Re-validation | • Phase 2B/2E will need to subscribe to a chain-verify-failed signal. The current `IComplianceCore` has no such event — Phase 2B start MUST add `ChainVerifyFailed` event via §Governance amendment OR Phase 2B UI polls `VerifyChainIntegrityAsync` periodically. **Flag for Phase 2B HALT.**<br>• Confirm Phase 4A export tool reads last-known-good chain. |
| Risk if wrong | Chain tampering goes unnoticed in v1.0. |
| Promotion path | Amend §4.6 to add `ChainVerifyFailed` event + §10.9 export contract; v1.0.2 likely needed before Phase 2B. |

### 6. **LOAD-BEARING** — PhiRedactor regex policy

| Field | Value |
| --- | --- |
| Source-of-truth | §9.2.3 lists 5 PHI categories ("SSN, MRN-like 6–10 digit strings, names from common-name list, dates, phone numbers, emails (except support email)") without regex specs. |
| Spec gap | Regex specifics + "common-name list" source + "dates" format scope + "support email" address. |
| Chosen behavior | See `PhiRedactor.cs` for the canonical regex literals (Msg 2 ships them verbatim in C# verbatim-string form). Pattern order (most-specific first to minimize double-redaction): 1. SSN → `[REDACTED:SSN]`; 2. Phone → `[REDACTED:PHONE]`; 3. MRN-like 6–10 digit → `[REDACTED:MRN]`; 4. ISO date → `[REDACTED:DATE]`; 5. Email → `[REDACTED:EMAIL]` (allowlist consulted first — see §7). **Strictness** = "balanced — over-redact rather than under" per §9.2.3 + HANDOFF v1.0.0 Decisions Log. **Names from common-name list** = **DEFERRED.** Phase 1C ships a synthetic test-only sentinel set (10 obviously-fake names like `"Pat Test"`, `"Sam Sample"`) for test coverage only; the regex compiles a case-insensitive alternation against these tokens. **Owner must provide real source list before Phase 5F sign-off — see HANDOFF Q10.** |
| Alternatives rejected | NER ML library (D3 violation, not in §12 tech stack); skip name redaction entirely (leaves known PHI hole in v1.0); use Microsoft Presidio (D3 violation). |
| Downstream consumers | `ComplianceCore.RedactPhi`; BugReporter `RedactionPipeline` (Phase 3D); every module's logger calls per §10.2. |
| Re-validation tasks for handoff agent | • **CRITICAL:** Owner must close Q10 (real name source list) before Phase 5F sign-off; current synthetic sentinel list provides ZERO production coverage for names.<br>• Confirm Phase 5C coverage gate exercises all 5 PHI categories.<br>• Confirm Phase 5E live test never logs un-redacted PHI in any module's logger output (HIPAA breach risk).<br>• If patterns must change (e.g., international phone number support), test fixture updates must come in lockstep with regex changes.<br>• If a 6th PHI category emerges (e.g., addresses), add via §Governance amendment. |
| Risk if wrong | **HIPAA breach via unredacted PHI in logs, bug reports, or audit log.** Over-redaction also harms debugging. |
| Promotion path | Amend §9.2.3 to declare regex set + name-list source + supported date formats; v1.0.2 candidate. |

### 7. **LOAD-BEARING** — Support-email allowlist seed

| Field | Value |
| --- | --- |
| Source-of-truth | §9.2.3 says "emails (except support email)" but never names the support email. |
| Spec gap | Address(es) of the allowlisted support email(s). |
| Chosen behavior | `PhiRedactor` constructor takes `IReadOnlyCollection<string> allowedEmails` (constructor-injected, NOT hardcoded per D7); DI default in Phase 1F = empty collection; email regex consults allowlist before redacting. |
| Alternatives rejected | Hardcode `"support@tasktree.example.com"` placeholder (D7 violation; misleading at Phase 5F sign-off). |
| Downstream consumers | `PhiRedactor`; Phase 1F DI `ServiceRegistrations.cs`; Phase 3E `BugReporter` (must respect allowlist when redacting outgoing report destinations). |
| Re-validation | • **CRITICAL:** Owner must populate the allowlist at Phase 1F composition root OR explicitly accept no allowlist (logged as Q11).<br>• Confirm Phase 3E `BugReporter` does NOT bypass the redactor for outgoing email destinations.<br>• Phase 5F sign-off cannot complete until Q11 is closed. |
| Risk if wrong | Support email leaks PHI through redaction loophole; over-redaction blocks legitimate support emails. |
| Promotion path | Amend §9.2.3 to name the canonical support email OR document the empty-allowlist contract. |

### 8. RedactPhi null/empty contract

| Field | Value |
| --- | --- |
| Source-of-truth | §4.6 stub silent; Roadmap P1C-AC4 "Redactor never throws on null/empty". |
| Spec gap | Exact return for `null` and `""`. |
| Chosen behavior | `null` → `string.Empty`; `""` → `""`; all other inputs → pipeline. |
| Alternatives rejected | Throw `ArgumentNullException` (violates P1C-AC4); return the input unchanged (caller can't distinguish "no PHI found" from "null input"). |
| Downstream consumers | Every module calling `RedactPhi`. |
| Re-validation | • Confirm Phase 3D `BugReporter` null-safety asserts.<br>• Phase 5C coverage gate exercises both `null` and `""` branches. |
| Risk if wrong | `NullReferenceException` in logging hot paths. |
| Promotion path | Amend §4.6 stub remarks. |

---

## Implementation note — FakeClock promotion deferral (Phase 1C Msg 4)

PHASE1A-DERIVATIONS §8 re-validation tasks state: *"When Phase 1C/1D needs `FakeClock`, promote it to `tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs`."*

Phase 1C Msg 4 was the **2nd consumer** of `FakeClock` (after `TaskEngineTests`); Phase 1C Msg 5 is the **3rd consumer** (using its own inline copy for symmetry). Per the precedent-vs-YAGNI tradeoff:

- **2 consumers** = minor duplication (~5 lines per file); manageable.
- **3+ consumers** = clear win for shared location.

Phase 1D `ReminderScheduler` tests will be the **4th consumer** and definitely need `FakeClock` for cadence simulation. That makes **Phase 1D Msg 1 (test scaffolding)** the canonical promotion point.

**Action item for Phase 1D HALT (item #1):** promote `FakeClock` to a shared `TestDoubles` project or namespace; update `PHASE1A-DERIVATIONS.md` §8 with a Superseded-by row; update this note with the promotion link.

---

## Cross-derivation interaction notes

- **Derivation 6 + Derivation 7:** the email regex (§6) defers to the allowlist (§7) — they MUST be reasoned about together. If the allowlist is empty (current default), ALL emails are redacted including the support email if it ever ships with that empty default.
- **Derivation 4 + Derivation 5:** monotonic `Seq` + bool-only `Verify` means tamper detection is silent in v1.0. Phase 2B/2E (UI warning) + Phase 4A (export) close the §10.9 loop. **Phase 2B will likely require a `ChainVerifyFailed` event on `IComplianceCore` — flag for §Governance amendment at Phase 2B start.**
- **Derivation 3 + Phase 4A:** storage format decision is bounded by perf — 100k stress test is the validating gate; if it fails, chunking/append-only must be retrofitted via §Governance.
- **Derivation 1 + PhiRedactor (Msg 2) + AuditChainWriter (Msg 3):** `ComplianceCore` is the public boundary; the two collaborators are concrete sibling classes per PHASE1B §1 YAGNI precedent. If a second redactor or chain impl is ever needed, extract interfaces via §Governance — do NOT modify these classes.
- **Derivation 4 + PHASE1A §4 + PHASE1A §7:** AuditChainWriter's defensive timestamp fallback (`if entry.Timestamp == default → clock.UtcNow`) is an inline implementation detail, NOT a new derivation. It mirrors the PHASE1A §7 pattern of "engine-assigns-only-when-default-supplied" and protects against forgetful callers without overriding properly-set values from TaskEngine (PHASE1A §4) or future audit-emitting modules.

---

## Cross-reference table

| Derivation | Affects (later phases) | Verified by |
| --- | --- | --- |
| 1. `ComplianceCore` structure | Phase 1F (DI), 2F (idle monitor), 4A (stress) | `ComplianceCoreTests.StartIdleMonitor_ThrowsNotImplementedException` (Msg 4) |
| 2. `AuditChainWriter` surface | Phase 4A (stress), 5C (coverage) | `ComplianceCoreTests.AuditAsync_AppendsValidChainEntry` (Msg 4) |
| 3. Audit storage representation | Phase 4A (perf), 5C | `ComplianceCoreTests` end-to-end (Msg 4) |
| 4. Sequence policy | Phase 4A (monotonicity); every audit-emitting module | `AuditChainWriterTests.AppendAsync_AssignsMonotonicallyIncreasingSeq` + `AppendAsync_FirstEntry_UsesGenesisPrevHash` (Msg 5) |
| 5. `VerifyChainIntegrityAsync` side effects | Phase 2B/2E (UI), 4A (export) | `ComplianceCoreTests.VerifyChainIntegrityAsync_*` (Msg 4) |
| 6. PHI regex set + name list | Phase 3D (BugReporter), 5C, 5E, 5F (Q10) | `PhiRedactorTests` 5 pattern tests (Msg 4) |
| 7. Support-email allowlist | Phase 1F (DI default), 3E (BugReporter), 5F (Q11) | `PhiRedactorTests.Redact_Email_RespectsAllowlist` (Msg 4) |
| 8. Null/empty contract | Every redactor caller; Phase 5C | `PhiRedactorTests.Redact_NullAndEmpty_ReturnsEmpty` (Msg 4) |

---

## Promotion / change procedure

Same as prior registries. Amend Architecture, bump to v1.0.2, add Superseded-by row, update `HANDOFF.md`, re-run grep helper.

---

## Document history

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 1.0.0 | 2026-05-26 | Dominic Sarria-Wiley (via Claude Opus) | Initial registry — 8 ComplianceCore baseline derivations approved during Phase 1C Msg 1. Two LOAD-BEARING items (§6 PHI regex set with Q10 name-list gap; §7 support-email allowlist Q11) flagged for owner closure before Phase 5F sign-off. |
| 1.0.1 | 2026-05-26 | Dominic Sarria-Wiley (via Claude Opus) | Phase 1C Msg 2 — `PhiRedactor.cs` shipped. Derivations §6 (regex policy), §7 (allowlist surface), §8 (null/empty contract) now have in-code references. Marker count bumped from 1 to 2. Q10 + Q11 remain LOAD-BEARING. |
| 1.0.2 | 2026-05-26 | Dominic Sarria-Wiley (via Claude Opus) | Phase 1C Msg 3 — `AuditChainWriter.cs` shipped. Derivations §2, §3, §4, §5 now have in-code references. Marker count bumped from 2 to 3. **ComplianceCore module is now code-complete.** Defensive timestamp fallback (PHASE1A §7 pattern) added inline. Q10 + Q11 remain LOAD-BEARING. |
| 1.0.3 | 2026-05-26 | Dominic Sarria-Wiley (via Claude Opus) | Phase 1C Msg 4 — `ComplianceCoreTests.cs` (5 tests) + `PhiRedactorTests.cs` (6 tests) shipped. 11 of 13 approved tests now live. Test infrastructure (`InMemorySecureStore` + `FakeClock`) inlined per PHASE1A §8 convention; **FakeClock promotion deferred to Phase 1D Msg 1** (3rd consumer is the canonical trigger — see Implementation note above). Marker count bumped from 3 to 5. Q10 + Q11 remain LOAD-BEARING active. |
| 1.0.4 | 2026-05-26 | Dominic Sarria-Wiley (via Claude Opus) | Phase 1C Msg 5 — `AuditChainWriterTests.cs` (2 tests) shipped. **Phase 1C is now complete: 3 production files + 13 unit tests + 8 derivations + 2 LOAD-BEARING open questions (Q10, Q11) tracked.** Marker count bumped from 5 to 6 (final Phase 1C count). Next phase: Phase 1D (ReminderScheduler) will trigger FakeClock promotion at Msg 1 HALT. |
