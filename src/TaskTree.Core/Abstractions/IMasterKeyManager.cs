// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Abstractions/IMasterKeyManager.cs
//  Purpose: Master-key-provider abstraction per Architecture §10.3 (added Phase 1B Msg 2 via PHASE1B-DERIVATIONS §7).
//  Architecture.md References: §10.3, §4.5, §3.3
//  Roadmap.md References: Phase 1B — Msg 2 (test-driven extraction)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: promotes MasterKeyManager (sealed concrete from Msg 1) per the registry's anticipated path.
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────
using System.Threading.Tasks;

namespace TaskTree.Core.Abstractions;

/// <summary>
/// Master-key provider abstraction extracted from <c>MasterKeyManager</c>
/// (Phase 1B Msg 1) so that <c>SecureStore</c> can be tested in isolation
/// without exercising real DPAPI.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-PHASE1B: this interface was promoted during Phase 1B Msg 2
/// (Option A) per the path explicitly anticipated in
/// docs/spec-derivations/PHASE1B-DERIVATIONS.md §1: "If a second key impl is
/// ever needed (e.g., HSM-backed), extract IMasterKeyManager interface via
/// §Governance; do NOT modify this class." Approved by human owner on
/// 2026-05-26. The "second impl" is the in-test FakeMasterKeyManager / Moq
/// double used by SecureStoreTests.
/// </remarks>
public interface IMasterKeyManager
{
    /// <summary>
    /// Returns the unwrapped 32-byte AES-256 master key. Implementations are
    /// expected to cache the unwrapped key for the process lifetime per
    /// docs/spec-derivations/PHASE1B-DERIVATIONS.md §6.
    /// </summary>
    Task<byte[]> GetOrCreateMasterKeyAsync();
}
