# Phase 0 — Msg 2 Spec-Derived Interface Surfaces

**Status:** approved by Dominic Sarria-Wiley on 2026-05-26
**Grep marker:** `SPEC-DERIVED-MSG2`
**Total derived interfaces:** 4 of 11
**Companion docs:** Architecture.md v1.0.0, Roadmap.md v1.0.0, HANDOFF.md v1.0.1

---

## Why this file exists

Architecture.md is the single source of truth for interface surfaces. For four
cross-cutting primitives (IClock, ICryptoProvider, IAppLogger, IOrchestrator),
Architecture.md describes their *role* but not their *signatures*. Per Roadmap
D8 (HALT protocol), the agent stopped, asked, and received owner approval
before generating signatures. This file records each derivation so that any
future agent (Codex, Claude Code, human reviewer) can locate, audit, and
re-validate every approved-but-not-spec'd surface.

## How to use this file at handoff

1. `grep -r "SPEC-DERIVED-MSG2" src/` — finds every interface file carrying a derived surface.
2. `grep -r "SPEC-DERIVED-MSG2" docs/` — locates this registry.
3. Cross-check each entry below against the implementations produced in later
   phases (esp. Msg 5 AesGcmCryptoProvider, Msg 5 FileAppLogger, Phase 1F
   Orchestrator, Phase 1A's IClock-using TaskEngine).
4. If any derivation conflicts with real runtime needs found during Codex
   Phase 5B/5D, promote the change to Architecture.md v1.0.2 via the
   §Governance amendment protocol — do NOT silently rename.

---

## Derivation Registry

### 1. IClock

| Field | Value |
| --- | --- |
| File | `src/TaskTree.Core/Abstractions/IClock.cs` |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "This whole repo must be easily trackable for gaps when it gets handed off eventually.  It is fine to approve them but be sure to record thoroughly for when they need to be wired and gaps filled later on after handoff" |
| Source-of-truth section(s) | Architecture §3.2 (cross-cutting), §4.2, §4.3; Roadmap 1A anti-drift line "Use `IClock` — never `DateTime.Now`" |
| Spec gap | Architecture names IClock as a cross-cutting primitive and mandates its use for time, but no member surface is provided. |
| Derivation rationale | Only a UTC-now reader is required by the documented callers (TaskEngine deadline checks, ReminderScheduler tick comparisons). `DateTimeOffset` preferred over `DateTime` for unambiguous timezone semantics; matches §10.5 audit timestamp `"2026-06-01T12:34:56.789Z"`. |
| Approved surface | ```csharp
DateTimeOffset UtcNow { get; }
``` |
| Downstream consumers | TaskEngine (Phase 1A), ReminderScheduler (Phase 1D), IdleMonitor (Phase 2F), AutoUpdater poll timer (Phase 3A), AuditChainWriter timestamp field (Phase 1C). |
| Re-validation tasks for handoff agent | - Confirm no caller needs monotonic/elapsed time — if yes, add `Elapsed` property or migrate to a dedicated `IStopwatch` primitive (NOT silently extend).<br>- Confirm test doubles (`FakeClock`) can advance time deterministically for cadence tests in Phase 1D. |
| Risk if surface is wrong | Time-zone bugs in audit log; reminder cadence drift; non-deterministic tests. |
| Promotion path | Amend Architecture.md §3.2 or §4.x per §Governance; bump to v1.0.2; add a Superseded-by row here. |

### 2. ICryptoProvider

| Field | Value |
| --- | --- |
| File | `src/TaskTree.Core/Abstractions/ICryptoProvider.cs` |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "This whole repo must be easily trackable for gaps when it gets handed off eventually.  It is fine to approve them but be sure to record thoroughly for when they need to be wired and gaps filled later on after handoff" |
| Source-of-truth section(s) | Architecture §10.3, §10.7; §3.3 (`AesGcmCryptoProvider.cs` exists); §4.5 (SecureStore uses it). |
| Spec gap | Architecture mandates AES-256-GCM and references `AesGcmCryptoProvider` but does not declare method signatures or the on-disk byte layout. |
| Derivation rationale | A single-blob `byte[]` in/out keeps SecureStore writes atomic per §10.7. 12-byte nonce is the AES-GCM standard. Optional `associatedData` allows authenticated header binding without forcing all callers to provide it. Packing nonce ‖ ciphertext ‖ tag into one `byte[]` is the canonical .NET AES-GCM pattern. |
| Approved surface | ```csharp
byte[] Encrypt(byte[] plaintext, byte[] key, byte[]? associatedData = null);
byte[] Decrypt(byte[] ciphertextWithNonceAndTag, byte[] key, byte[]? associatedData = null);
``` |
| Wire format (NORMATIVE) | `12-byte nonce ‖ ciphertext ‖ 16-byte GCM tag`, contiguous. |
| Downstream consumers | AesGcmCryptoProvider (Phase 0 Msg 5), SecureStore (Phase 1B), MasterKeyManager (Phase 1B), AuditChainWriter integrity HMAC (Phase 1C — uses GCM tag), BugReport encryption-at-rest (Phase 3D). |
| Re-validation tasks for handoff agent | - Confirm Phase 1B SecureStore reads/writes match the 12+ct+16 layout byte-for-byte.<br>- Confirm Phase 0 Msg 5 AesGcmCryptoProvider uses `RandomNumberGenerator` for nonce per call (no nonce reuse — GCM security catastrophe otherwise).<br>- Confirm key length is always 32 bytes (AES-256); add `ArgumentException` guard at impl time.<br>- If a streaming variant is needed for >10 MB payloads (§4.5 perf target is for ≤10 MB), add a NEW interface (e.g., `ICryptoStreamProvider`) — do NOT silently change this one. |
| Risk if surface is wrong | Data-at-rest corruption; integrity tag mismatch on every read; HIPAA encryption control failure (§10.3). |
| Promotion path | Amend Architecture.md §10.3 or §4.5 per §Governance; bump to v1.0.2; add a Superseded-by row here. |

### 3. IAppLogger

| Field | Value |
| --- | --- |
| File | `src/TaskTree.Core/Abstractions/IAppLogger.cs` |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "This whole repo must be easily trackable for gaps when it gets handed off eventually.  It is fine to approve them but be sure to record thoroughly for when they need to be wired and gaps filled later on after handoff" |
| Source-of-truth section(s) | Architecture §3.3 (`FileAppLogger.cs` exists), §10.5 (audit JSON schema), §12 (`Microsoft.Extensions.Logging.Abstractions` in tech stack), §9.2.3 (PHI redaction contract). |
| Spec gap | A logger primitive is declared in the file layout but no public method surface is provided. |
| Derivation rationale | Mirrors `Microsoft.Extensions.Logging.ILogger` log-level vocabulary (Debug/Info/Warn/Error) since §12 pins that package. Owning the abstraction in `TaskTree.Core` keeps the codebase decoupled from a specific logger backend (per §1.4 "Deterministic by Design"). Error overload accepts `Exception`. `params object?[]` enables structured-logging argument forwarding without binding to a specific message-template library. |
| Approved surface | ```csharp
void LogDebug(string message, params object?[] args);
void LogInformation(string message, params object?[] args);
void LogWarning(string message, params object?[] args);
void LogError(Exception? exception, string message, params object?[] args);
``` |
| Downstream consumers | Every module (universal cross-cutting), FileAppLogger (Phase 0 Msg 5), all 8 module impls in Phase 1, BugReporter crash capture (Phase 3D). |
| Re-validation tasks for handoff agent | - Confirm FileAppLogger writes valid JSON lines per P0-AC5.<br>- Confirm NO module logs raw task-content strings before passing through `IComplianceCore.RedactPhi` (per §9.2.3, §10.2).<br>- Confirm test code does not inject PHI-like strings into log calls (D6).<br>- If `LogTrace` or `LogCritical` levels are needed later, extend the interface — do NOT repurpose existing methods. |
| Risk if surface is wrong | PHI leakage into logs (HIPAA breach); logger-backend lock-in; tests miss un-redacted log paths. |
| Promotion path | Amend Architecture.md §3.3 per §Governance; bump to v1.0.2; add a Superseded-by row here. |

### 4. IOrchestrator

| Field | Value |
| --- | --- |
| File | `src/TaskTree.Core/Abstractions/IOrchestrator.cs` |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "This whole repo must be easily trackable for gaps when it gets handed off eventually.  It is fine to approve them but be sure to record thoroughly for when they need to be wired and gaps filled later on after handoff" |
| Source-of-truth section(s) | Architecture §3.1 (diagram), §3.2 (dependency graph), §7 (tier fallback decision lives here); Roadmap 1F. |
| Spec gap | Architecture names the Orchestrator and shows its position in the dependency graph but does not declare a method surface. Internal event-wiring is documented; public API is not. |
| Derivation rationale | All cross-module coordination per §3.1 is event-driven and happens during construction/start-up subscription. The only PUBLIC surface a host process (`TaskTree.App`) needs is lifecycle: start the runtime and stop it cleanly. Keeping the surface minimal preserves §1.4 "Deterministic by Design" and avoids accidentally exposing internal state. |
| Approved surface | ```csharp
Task StartAsync(CancellationToken cancellationToken = default);
Task StopAsync(CancellationToken cancellationToken = default);
``` |
| Downstream consumers | `TaskTree.App` composition root (Phase 1F), service-host integration tests (Phase 1H, Phase 5D). |
| Re-validation tasks for handoff agent | - Confirm Phase 1F Orchestrator implementation subscribes events in `StartAsync` and unsubscribes in `StopAsync` (lifecycle symmetry).<br>- Confirm `StopAsync` is idempotent and safe to call during app shutdown.<br>- If runtime needs introspection (e.g., "is running" status for the tray menu), add `IsRunning` bool getter — do NOT add a setter; lifecycle is method-only. |
| Risk if surface is wrong | Orphaned event subscriptions causing memory leaks; double-start race conditions; ungraceful shutdown losing in-flight audit writes. |
| Promotion path | Amend Architecture.md §3.1 or §3.2 per §Governance; bump to v1.0.2; add a Superseded-by row here. |

---

## Cross-reference table — every file containing a SPEC-DERIVED-MSG2 surface

| Interface | Path | Concrete impl path (future) | Phase that wires it |
| --- | --- | --- | --- |
| `IClock` | `src/TaskTree.Core/Abstractions/IClock.cs` | (concrete is system clock; injected — `TaskTree.App/Bootstrap/ServiceRegistrations.cs` registers `DateTimeOffset.UtcNow` source in Phase 1F) | Phase 1F |
| `ICryptoProvider` | `src/TaskTree.Core/Abstractions/ICryptoProvider.cs` | `src/TaskTree.Core/Security/AesGcmCryptoProvider.cs` | Phase 0 Msg 5 |
| `IAppLogger` | `src/TaskTree.Core/Abstractions/IAppLogger.cs` | `src/TaskTree.Core/Logging/FileAppLogger.cs` | Phase 0 Msg 5 |
| `IOrchestrator` | `src/TaskTree.Core/Abstractions/IOrchestrator.cs` | `src/TaskTree.Orchestrator/Orchestrator.cs` | Phase 1F |

---

## Promotion / change procedure

If any of these 4 surfaces needs to change after handoff:

1. Open a §Governance amendment against Architecture.md (bump to v1.0.x).
2. Add the formal stub to the appropriate §4.x or §3.2 section.
3. Update this file with a "Superseded by Architecture.md v1.0.x §y.z on YYYY-MM-DD" row.
4. Update HANDOFF.md §Decisions Log + §Gap Summary.
5. Re-run the grep helper script (`tools/find-spec-derivations.ps1`) to confirm no orphan `SPEC-DERIVED-MSG2` markers remain.

---

## Document history

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 1.0.0 | 2026-05-26 | Dominic Sarria-Wiley (via Claude Opus) | Initial registry — 4 derivations approved during Phase 0 Msg 2. |
