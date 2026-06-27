# Phase 0 — Msg 5 Spec-Derived Security + Logging Primitives

**Status:** approved by Dominic Sarria-Wiley on 2026-05-26
**Grep marker:** `SPEC-DERIVED-MSG5`
**Total derived primitives:** 3 of 3
**Companion docs:** Architecture.md v1.0.0, Roadmap.md v1.0.0, HANDOFF.md v1.0.4, `PHASE0-MSG2-DERIVATIONS.md`, `PHASE0-MSG3-DERIVATIONS.md`, `PHASE0-MSG4-DERIVATIONS.md`.

---

## Why this file exists

Architecture.md is the single source of truth for primitive-class surfaces.
For all three Phase-0 Msg-5 primitives, Architecture.md names the file in
§3.3 and describes its *role* (with §10.3/§10.5 supplying formula-level
detail), but the C# class layouts — constructor signatures, internal
conventions, defaults, and rotation/canonicalization policies — are not
specified verbatim. Per Roadmap D8 (HALT protocol), the agent stopped, asked,
and received owner approval before generating these primitives. This file
records each derivation so any future agent (Codex, Claude Code, human
reviewer) can locate, audit, and re-validate every approved-but-not-spec'd
primitive.

## How to use this file at handoff

1. `grep -r "SPEC-DERIVED-MSG5" src/` — locates the 3 primitive `.cs` files.
2. `grep -r "SPEC-DERIVED-MSG5" docs/` — locates this registry.
3. Cross-check against Phase 1B (`SecureStore` uses `AesGcmCryptoProvider`),
   Phase 1C (`AuditChainWriter` uses `HashChain`), every module + Phase 1F
   bootstrap (`FileAppLogger` via `IAppLogger` DI).
4. Promotion path: amend Architecture.md, bump to v1.0.x, record a
   Superseded-by row.

---

## Derivation Registry

### 1. AesGcmCryptoProvider (minor — nonce source + key-length guard + tag-length constant)

| Field | Value |
| --- | --- |
| File | `src/TaskTree.Core/Security/AesGcmCryptoProvider.cs` |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "Approve all 3 derivations as-is, thoroughly log any gaps for handoff later on" |
| Source-of-truth section(s) | §10.3 (AES-256-GCM + DPAPI), §10.7 (integrity), §3.3 (`AesGcmCryptoProvider.cs` declared), `ICryptoProvider` registry (`PHASE0-MSG2-DERIVATIONS.md` §2). |
| Spec gap | §10.3 mandates AES-256-GCM but does not specify the random nonce source, the key-length validation behavior, or the tag length. |
| Derivation rationale | • Nonce source: `RandomNumberGenerator.Fill` — the canonical CSPRNG in .NET. Reuse would catastrophically break GCM confidentiality and integrity guarantees.<br>• Key-length guard: explicit `ArgumentException` if `key.Length != 32` — fails loud at the API boundary rather than letting `AesGcm` throw a less specific exception.<br>• Tag length: 16 bytes (`AesGcm.TagByteSizes.MaxSize`) — strongest available; matches the normative wire format documented in the `ICryptoProvider` registry. |
| Approved surface | `ctor()` + `Encrypt` + `Decrypt` + public consts `NonceSize=12`, `TagSize=16`, `KeySize=32`. |
| Downstream consumers | `SecureStore` (Phase 1B), `MasterKeyManager` (Phase 1B), `AuditChainWriter` integrity via GCM tag (Phase 1C), `BugReport` encryption-at-rest (Phase 3D). |
| Re-validation tasks for handoff agent | - Confirm `RandomNumberGenerator.Fill` is invoked per call (no nonce reuse).<br>- Confirm `ArgumentException` fires before any `AesGcm` allocation for wrong-length keys.<br>- Confirm `Decrypt` throws `CryptographicException` (from `AesGcm.Decrypt`) on tag mismatch — DO NOT catch this exception; let it propagate per §10.7.<br>- If a streaming variant for >10 MB payloads is needed later, add a NEW interface (`ICryptoStreamProvider`) — do NOT silently change this one. |
| Risk if behavior is wrong | Nonce reuse → catastrophic confidentiality + integrity break; missing key-length guard → confusing downstream failures; smaller tag → reduced integrity assurance. |
| Promotion path | Amend §10.3 per §Governance. |

### 2. HashChain (entire surface)

| Field | Value |
| --- | --- |
| File | `src/TaskTree.Core/Security/HashChain.cs` |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "Approve all 3 derivations as-is, thoroughly log any gaps for handoff later on" |
| Source-of-truth section(s) | §10.5 (hash formula), §10.7 (integrity controls), §4.6 (`ComplianceCore` uses it). |
| Spec gap | §10.5 specifies the formula but no C# class members. No genesis-sentinel convention. |
| Derivation rationale | • Static class: stateless primitive matching the pure-function style of `AesGcmCryptoProvider`'s per-call API.<br>• `ComputeHash(prevHash, entry)`: pure function applying §10.5 formula. Caller (Phase 1C) is the chain owner.<br>• `VerifyChain(entries)`: iterates, recomputes, asserts equality. Returns `bool` per Roadmap P1C-AC2.<br>• `GenesisPrevHash = ""`: empty string is unambiguous because no hex-encoded SHA-256 ever equals empty. Architecture is silent; this is the doc-faithful minimum.<br>• Canonical JSON: properties in declared C# order (matches §10.5 JSON field order), no whitespace, omitting `Hash`. System.Text.Json defaults satisfy this when properties are declared in schema order. |
| Approved surface | `GenesisPrevHash` const + `ComputeHash` + `VerifyChain` (both static). |
| Downstream consumers | `ComplianceCore.AuditAsync` (Phase 1C), `AuditChainWriter` (Phase 1C), `AuditChainStressTests` 10k/100k (Phase 4A). |
| Re-validation tasks for handoff agent | - Confirm Phase 1C writes `AuditEntry.Hash` AFTER computing via `HashChain.ComputeHash` (NOT before — the formula omits `Hash`).<br>- Confirm canonical JSON serializer keeps property order identical to `AuditEntry` declaration. If a future PR adds a property to `AuditEntry`, BOTH the model and the private `EntryWithoutHash` DTO must be updated in lockstep.<br>- Verify performance target: §15 chain verify < 500 ms on 10k entries.<br>- If §10.5 schema gains a field, append to `EntryWithoutHash` in matching position; do NOT reorder existing fields (would break every stored chain). |
| Risk if surface is wrong | Every chain entry hash mismatches on read; HIPAA integrity control (§10.7) silently broken. |
| Promotion path | Amend §10.5 per §Governance. |

### 3. FileAppLogger (entire surface + log location + rotation + JSON-line schema)

| Field | Value |
| --- | --- |
| File | `src/TaskTree.Core/Logging/FileAppLogger.cs` |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "Approve all 3 derivations as-is, thoroughly log any gaps for handoff later on" |
| Source-of-truth section(s) | §3.3 (`FileAppLogger.cs` declared), §12 (`Microsoft.Extensions.Logging.Abstractions` binding), §10.2 (no PHI in logs without redaction), §10.5 (audit JSON style precedent), Roadmap P0-AC5 ("Logger writes valid JSON"). |
| Spec gap | Architecture says only that `FileAppLogger` exists and writes valid JSON. Constructor signature, log file path, rotation policy, JSON-line schema, and thread-safety model are all silent. |
| Derivation rationale | • Constructor params: injected directory + filename + size threshold — keeps the class testable (no hard-coded path) and matches the §3.3 / §9.2.5 / §10.3 convention of `%LOCALAPPDATA%\TaskTree\<subdir>\`.<br>• Default log directory: `%LOCALAPPDATA%\TaskTree\logs\` (set by DI registration in Phase 1F — NOT hardcoded in this class).<br>• Rotation: size-based at 10 MB threshold. Rename to `tasktree.log.YYYYMMDD-HHmmss`. Simple, deterministic, no external dependency.<br>• JSON-line schema: `{ timestamp, level, message, args, exception }` — one object per line (JSON Lines convention). Satisfies P0-AC5.<br>• Thread safety: lock on append. Sufficient for §15 perf targets given expected log volume.<br>• PHI redaction: NOT performed here per `IAppLogger` XML doc (`PHASE0-MSG2-DERIVATIONS.md` §3). Caller's responsibility.<br>• No purge: explicit limitation. v1.0 retains rotated logs indefinitely. §16 honest-limitation candidate. |
| Approved surface | `ctor(logDirectory, logFileName=DefaultLogFileName, maxFileSizeBytes=DefaultMaxFileSizeBytes)` + `LogFilePath` getter + 4 `IAppLogger` methods + 2 public consts. |
| Downstream consumers | Every module via `IAppLogger` (Phase 1A–3E), Phase 1F DI registration, `BugReporter` (Phase 3D — log lines may end up in bug reports per §9.2.2 "last 100 log lines redacted"). |
| Re-validation tasks for handoff agent | - Confirm Phase 1F `ServiceRegistrations.cs` passes the canonical path `%LOCALAPPDATA%\TaskTree\logs\` into the constructor.<br>- Confirm bug-report log capture (Phase 3D §9.2.2) reads the last 100 lines AFTER PHI redaction passes — verify the `BugReporter` never includes raw log lines.<br>- If rotation collides on the same second (rare), `Move(overwrite:false)` will throw — acceptable for v1.0; if encountered in practice, add a uniqueness suffix.<br>- Add log-purge policy if disk-fill becomes a real concern (deferred — document as §16 limitation in Phase 4D docs).<br>- Consider async appending if §15 perf targets regress under heavy log volume. |
| Risk if surface is wrong | Lost log lines (race conditions); PHI leakage if a caller forgets to redact (mitigated by `IAppLogger` XML doc reminder); disk fill from un-rotated logs. |
| Promotion path | Amend §3.3 per §Governance. |

---

## Cross-reference table

| Primitive | Path | Derived elements | Phases that consume it |
| --- | --- | --- | --- |
| `AesGcmCryptoProvider` | `src/TaskTree.Core/Security/AesGcmCryptoProvider.cs` | nonce source, key-length guard, tag-length constant | Phase 1B, 1C, 3D |
| `HashChain` | `src/TaskTree.Core/Security/HashChain.cs` | entire surface + genesis sentinel + canonical JSON | Phase 1C, 4A |
| `FileAppLogger` | `src/TaskTree.Core/Logging/FileAppLogger.cs` | entire surface + log path + rotation + JSON-line schema | every module (Phase 1A-3E) |

---

## Cross-derivation interaction notes

- **`HashChain` canonical JSON depends on `AuditEntry` property order (Msg 3).** If `AuditEntry` is ever reordered or has fields added, the private `EntryWithoutHash` DTO MUST be updated in lockstep — otherwise every previously-written chain entry will fail verification.
- **`FileAppLogger` ↔ `BugReporter` (Phase 3D) interaction.** `BugReporter` captures "last 100 log lines redacted" per §9.2.2. Redaction MUST happen at `BugReporter` capture time, not at logger write time, because `IAppLogger` doesn't redact.
- **`AesGcmCryptoProvider.TagSize = 16` is referenced by the `SecureStore` wire-format contract (`PHASE0-MSG2-DERIVATIONS.md` §2).** If `TagSize` ever changes, both this file and that registry must be updated together.

---

## Promotion / change procedure

Same as Msg 2–4 registries: open a §Governance amendment against Architecture.md, bump to v1.0.x, add a Superseded-by row, update HANDOFF.md §Decisions Log + §Gap Summary, and re-run `tools/find-spec-derivations.ps1` to confirm no orphan markers remain.

---

## Document history

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 1.0.0 | 2026-05-26 | Dominic Sarria-Wiley (via Claude Opus) | Initial registry — 3 primitives derived during Phase 0 Msg 5. |
