# Phase 1B — SecureStore + MasterKeyManager Spec-Derived Surfaces

**Status:** approved by Dominic Sarria-Wiley on 2026-05-26
**Grep marker:** `SPEC-DERIVED-PHASE1B`
**Total derived items:** 6 (+ 1 promotion recorded as §7)
**Companion docs:** Architecture.md v1.0.1, Roadmap.md v1.0.0, HANDOFF.md v1.0.9, `PHASE0-MSG2/3/4/5/6-DERIVATIONS.md`, `PHASE1A-DERIVATIONS.md`.

---

## Why this file exists

§4.5 declares the `ISecureStore` surface and §10.3 mandates AES-256-GCM + DPAPI
master key, but the concrete C# class surfaces (`MasterKeyManager`) and the
on-disk file layout (per-key file naming, atomic writes, key sanitization) are
not specified verbatim. All items recorded here for Codex / Claude Code
handoff.

## How to use this file at handoff

1. `grep -r "SPEC-DERIVED-PHASE1B" src/` — locates `IMasterKeyManager.cs`, `SecureStore.cs`, `MasterKeyManager.cs`.
2. `grep -r "SPEC-DERIVED-PHASE1B" tests/` — locates `SecureStoreTests.cs` + `MasterKeyManagerTests.cs`.
3. `grep -r "SPEC-DERIVED-PHASE1B" docs/` — locates this registry + HANDOFF.md delta.
4. Cross-check against Phase 1F DI wiring (must register `MasterKeyManager` as singleton `IMasterKeyManager` + pass injected dirs), Phase 5E live DPAPI verification, Phase 5C coverage gate.
5. Promotion path: amend Architecture.md §4.5 / §10.3, bump to v1.0.2.

---

## Derivation Registry

### 1. `MasterKeyManager` C# surface

| Field | Value |
| --- | --- |
| Approved by / date | Dominic Sarria-Wiley, 2026-05-26 |
| Source-of-truth | §10.3 (key generation / wrap / path); class location §3.3; no member surface declared. |
| Spec gap | Constructor signature, public method surface, return type. |
| Chosen behavior | Constructor `(string keyDirectory, IAppLogger logger, string keyFileName = "master.bin")`. Single public method `Task<byte[]> GetOrCreateMasterKeyAsync()`. Public `KeyFilePath` getter. Public const `DefaultKeyFileName`. |
| Alternatives rejected | Hardcoded path (violates D7); interface `IMasterKeyManager` (premature — extract when a second impl emerges); sync API (forces sync I/O in DI hot path). |
| Downstream consumers | `SecureStore` (Phase 1B), Phase 1F DI (must pass `%LOCALAPPDATA%\TaskTree\keys\` as `keyDirectory`); Phase 5E live DPAPI test. |
| Re-validation tasks for handoff agent | • Phase 1F `ServiceRegistrations.cs` MUST pass the canonical path `%LOCALAPPDATA%\TaskTree\keys\` into the constructor — verify before Phase 1F gate.<br>• If a second key impl is ever needed (e.g., HSM-backed), extract `IMasterKeyManager` interface via §Governance; do NOT modify this class.<br>• Phase 5E live test confirms DPAPI wrap/unwrap round-trip on a real Windows session. |
| Risk if wrong | Hardcoded path breaks testability and HIPAA portability; interface missing causes DI churn later. |
| Promotion path | Amend §10.3 to declare the `MasterKeyManager` surface; v1.0.2. |
| **Superseded by** | **Phase 1B Msg 2 — `IMasterKeyManager` interface promoted per the anticipated path; this concrete class now implements `IMasterKeyManager`. See §7 below.** |

### 2. `SecureStore` on-disk file layout (one-file-per-key + slash sanitization)

| Field | Value |
| --- | --- |
| Source-of-truth | §4.5 "JSON-on-disk"; per-key file vs single blob silent. PHASE1A §5 uses key `"tasks/tree"` which contains `/`. |
| Spec gap | One file vs many; key→filename sanitization rule. |
| Chosen behavior | One file per key under `storageDirectory`; `/` replaced with `__`; extension `.bin`; file contents = AesGcm wire format from `ICryptoProvider` (`nonce ‖ ciphertext ‖ tag`). Plaintext = `System.Text.Json` default serialization. |
| Alternatives rejected | One big dictionary file (every write rewrites whole blob — bad §15 perf); subdirectory mirror (modest complexity gain, no benefit at single-user scale). |
| Downstream consumers | Phase 1A `TaskEngine` (key `"tasks/tree"` → `tasks__tree.bin`); Phase 1C `ComplianceCore` (audit chain key); Phase 2E settings store; Phase 3D bug-report queue. |
| Re-validation | • Confirm Phase 1A-style keys never contain `_` or `__` in raw form (would collide with sanitization). Current usages safe. If a future key contains `__`, extend `SlashReplacement` scheme via §Governance.<br>• Confirm Phase 4B perf benchmarks meet §15 < 100 ms read/write for ≤10 MB.<br>• Confirm Phase 5C coverage gate exercises both sanitized and non-sanitized keys. |
| Risk if wrong | Key collision under sanitization; performance regression on huge single-blob writes. |
| Promotion path | Amend §4.5 to document layout convention. |

### 3. `SecureStore` constructor + storage-directory injection

| Field | Value |
| --- | --- |
| Source-of-truth | §4.5 silent on construction. |
| Chosen behavior | Constructor `(string storageDirectory, MasterKeyManager keyManager, ICryptoProvider crypto, IAppLogger logger)`. Directory created on construction. `MasterKeyManager` is a concrete dependency (not an interface) — same module, no abstraction declared in Phase 0. |
| Alternatives rejected | Hardcoded path (violates D7); extract `IMasterKeyManager` interface now (premature — YAGNI). |
| Downstream consumers | Phase 1F DI; Phase 5C tests must pass a temp dir. |
| Re-validation | • If Phase 1F DI cannot inject `MasterKeyManager` as a singleton due to construction-order issues, extract `IMasterKeyManager` interface via §Governance and update both files.<br>• Verify Phase 1A `TaskEngine` consumes `ISecureStore` (not concrete `SecureStore`) for testability. |
| Risk if wrong | DI deadlocks on construction; test fragility. |
| Promotion path | Amend §4.5 to declare `SecureStore` ctor surface. |
| **Superseded by** | **Phase 1B Msg 2 — `SecureStore` constructor parameter type changed from concrete `MasterKeyManager` to `IMasterKeyManager`. See §7 below.** |

### 4. JSON serialization options

| Field | Value |
| --- | --- |
| Source-of-truth | Architecture silent. PHASE1A §5 said "default `System.Text.Json` options". |
| Chosen behavior | `JsonSerializer.Serialize/Deserialize` with default options (no custom converters). |
| Alternatives rejected | Pin `JsonStringEnumConverter` here (already deferred to Phase 1F composition root). |
| Downstream consumers | Every module using `SecureStore`. |
| Re-validation | • If Phase 1F adds a `JsonStringEnumConverter` globally for UI/UX, verify `SecureStore` round-trip still passes.<br>• If an enum field is added to a stored model and tests fail, this is the first place to check. |
| Risk if wrong | Enum serialization-as-int vs as-string mismatch between writers and readers. |
| Promotion path | Amend §4.5 or §12 to specify JSON options globally. |

### 5. `SecureStore` concurrency model

| Field | Value |
| --- | --- |
| Source-of-truth | Architecture silent. `ISecureStore` methods are async; concurrent calls possible. |
| Chosen behavior | Single `SemaphoreSlim(1, 1)` gate guarding all operations. Not per-key. |
| Alternatives rejected | Per-key locks (over-engineered for single-user); lock-free file I/O (no benefit on single disk). |
| Re-validation | • Confirm §15 R/W < 100 ms for ≤10 MB still holds under the gate.<br>• If multi-user/cloud-sync ever lands (§6 DEFERRED), revisit. |
| Risk if wrong | Read-while-write inconsistency; perf degradation. |
| Promotion path | Amend §4.5 stub remarks. |

### 6. Master-key caching policy

| Field | Value |
| --- | --- |
| Source-of-truth | Architecture silent on caching. |
| Chosen behavior | Cache unwrapped 32-byte key in process memory after first `GetOrCreateMasterKeyAsync()`; concurrent first-call protection via `SemaphoreSlim(1,1)` + double-check. **No purge on idle** — auto-logoff (Phase 2F) handles process-state wipe. |
| Alternatives rejected | Per-call DPAPI unwrap (perf regression); clear-on-idle (overlaps with Phase 2F responsibility). |
| Downstream consumers | Every `SecureStore` operation; Phase 2F auto-logoff (must trigger app restart or process replacement to clear cache). |
| Re-validation | • Confirm Phase 2F auto-logoff behavior either (a) restarts the process, OR (b) calls a `ClearCache` method we would add via §Governance. Currently relies on (a). Document expectation in Phase 2F derivations.<br>• Confirm Phase 5C tests verify cache hit by counting `File.ReadAllBytes` calls. |
| Risk if wrong | Key persists in memory after logoff (HIPAA technical-safeguard concern); per-call unwrap regresses §15 perf. |
| Promotion path | Amend §10.3 + §10.4 (auto-logoff) jointly to specify cache lifecycle. |

### 7. `IMasterKeyManager` interface promotion (Phase 1B Msg 2)

| Field | Value |
| --- | --- |
| Approved by / date | Dominic Sarria-Wiley, 2026-05-26 |
| Approval message | "Option A — promote IMasterKeyManager interface NOW as anticipated by PHASE1B-DERIVATIONS §1" |
| Source-of-truth | Earlier registry §1 explicitly anticipated this exact path: *"If a second key impl is ever needed (e.g., HSM-backed), extract `IMasterKeyManager` interface via §Governance; do NOT modify this class."* |
| Spec gap | None — this is a planned promotion, not a new derivation. |
| Trigger | Phase 1B Msg 2 test plan revealed that sealed concrete `MasterKeyManager` cannot be mocked by Moq, so the 12-test split (8 offline + 4 Live) was infeasible without an interface seam. |
| Chosen behavior | Add new abstraction `src/TaskTree.Core/Abstractions/IMasterKeyManager.cs` with single `Task<byte[]> GetOrCreateMasterKeyAsync()`. `MasterKeyManager` (sealed) implements it. `SecureStore` ctor parameter type changes to `IMasterKeyManager`. |
| Alternatives rejected | (B) Unseal `MasterKeyManager` (weaker isolation; ctor-inheritance baggage); (C) Mark all `SecureStore` tests Live (zero offline coverage; fails Phase 5C ≥75% gate); (D) Promote to Architecture v1.0.2 first (over-formal for a small new file that lives in `Core/Abstractions`). |
| Downstream consumers | Phase 1F DI (must register `MasterKeyManager` as singleton `IMasterKeyManager`); Phase 5C tests; Phase 5E live DPAPI verification; Phase 2F auto-logoff (still owns process-state wipe per §6). |
| Re-validation tasks for handoff agent | • Confirm Phase 1F DI registers `MasterKeyManager` as singleton `IMasterKeyManager`.<br>• Confirm no other module took a hard dependency on the concrete `MasterKeyManager` type before this Msg.<br>• If a real HSM-backed second impl is ever needed, no further interface work is required — just add a new class that implements `IMasterKeyManager`. |
| Risk if wrong | Phase 1F DI tries to inject concrete `MasterKeyManager` into `SecureStore` → compile failure. |
| Promotion path | If Architecture.md is later amended to formally declare `IMasterKeyManager` in §3.3, add a Superseded-by row here. v1.0.2 candidate. |

---

## Cross-derivation interaction notes

- **Derivation 1 + 3 + 7:** the interface promotion (§7) supersedes the "no interface" choices in §1 and §3 without invalidating their rationale — they remain correct *as of their authoring time*. The Superseded-by rows record the linear-time evolution.
- **Derivation 2 + 4:** the file layout (per-key file, AesGcm wire format) and JSON options together define the on-disk schema. Changing either invalidates every stored file. Phase 5B handoff agent must verify both before deploying.
- **Derivation 5 + 6:** `SecureStore` gate is independent of `MasterKeyManager` gate. Two separate semaphores — intentional, because key access is much hotter than store access and they have different lifecycles.
- **Derivation 6 + Phase 2F:** the cache-clear contract is currently implicit ("process restart clears it"). Phase 2F MUST document this explicitly in its own derivations, OR introduce a `ClearCache` method on `IMasterKeyManager` (via §Governance). Codex Phase 5E will catch this if missed.

---

## Cross-reference table

| Derivation | Affects (later phases) |
| --- | --- |
| 1. `MasterKeyManager` surface | Phase 1F (DI), 5E (live DPAPI), 5C (coverage) |
| 2. File layout + sanitization | Phase 1A, 1C, 2E, 3D, 4B (perf), 5C |
| 3. `SecureStore` constructor | Phase 1F (DI), 5C |
| 4. JSON default options | Every module using `SecureStore`; Phase 1F composition root |
| 5. `SemaphoreSlim` gate | Phase 4B (perf), 5D (concurrency) |
| 6. Master-key cache | Phase 2F (auto-logoff), 5E (live test), 5C |
| 7. `IMasterKeyManager` promotion | Phase 1F (DI), 5C, 5E |

---

## Promotion / change procedure

Same as prior registries. Amend Architecture, bump to v1.0.2, add Superseded-by row, update `HANDOFF.md`, re-run grep helper.

---

## Document history

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 1.0.0 | 2026-05-26 | Dominic Sarria-Wiley (via Claude Opus) | Initial registry — 6 SecureStore/MasterKeyManager derivations approved during Phase 1B Msg 1. |
| 1.0.1 | 2026-05-26 | Dominic Sarria-Wiley (via Claude Opus) | Phase 1B Msg 2 — promoted `IMasterKeyManager` interface per anticipated path in §1. Updated §1 + §3 with Superseded-by rows; added §7. `SecureStore` now depends on the interface. Marker count bumped from 2 to 5 (interface + 2 impl + 2 test files). |
