// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Security/HashChain.cs
//  Purpose: SHA-256 hash-chain primitive per Architecture §10.5 / §10.7.
//  Architecture.md References: §10.5, §10.7, §4.6
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 5 — Security + Logging Primitives)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: behavior matches Architecture.md verbatim where specified (or SPEC-DERIVED-MSG5).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskTree.Core.Models;

namespace TaskTree.Core.Security;

/// <summary>
/// Stateless SHA-256 hash-chain primitive for the append-only audit log per
/// Architecture §10.5 and §10.7.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-MSG5: surface not specified verbatim in Architecture.md;
/// derived from documented usage and approved by human owner on 2026-05-26.
/// See docs/spec-derivations/PHASE0-MSG5-DERIVATIONS.md.
/// <para>
/// Architecture §10.5 specifies the hash FORMULA
/// <c>SHA256(prevHash + canonicalJson(entryWithoutHash))</c> but does not
/// declare a C# surface. This primitive exposes:
/// (1) <see cref="ComputeHash"/> — pure SHA-256 over
///     UTF-8(prevHash) ‖ UTF-8(canonicalJson(entry without Hash));
/// (2) <see cref="VerifyChain"/> — iterates the list, confirms every link,
///     and enforces the writer-assigned 1..N sequence invariant;
/// (3) <see cref="GenesisPrevHash"/> = empty string — the very first entry
///     has no predecessor; the empty-string sentinel keeps the SHA-256 input
///     deterministic and any hex-decoder unambiguous (no hex string ever
///     decodes to empty).
/// </para>
/// <para>
/// Canonical JSON convention: properties serialized in declared C# order
/// (matches §10.5 JSON schema field order), no extra whitespace, omitting the
/// <c>Hash</c> property of the entry. Callers MUST NOT mutate
/// <c>AuditEntry.Hash</c> before passing into <see cref="ComputeHash"/>.
/// </para>
/// </remarks>
public static class HashChain
{
    /// <summary>
    /// Sentinel value used as the <c>prevHash</c> for the genesis (first)
    /// entry per Architecture §10.5 hash formula. Documented in
    /// PHASE0-MSG5-DERIVATIONS.md §2.
    /// </summary>
    public const string GenesisPrevHash = "";

    /// <summary>
    /// Computes the SHA-256 hash of
    /// <c>UTF-8(prevHash) || UTF-8(canonicalJson(entryWithoutHash))</c>
    /// per Architecture §10.5 hash formula.
    /// </summary>
    /// <param name="prevHash">
    /// Hex-encoded SHA-256 of the previous chain entry, or
    /// <see cref="GenesisPrevHash"/> for the first entry.
    /// </param>
    /// <param name="entry">
    /// The entry to hash. Its <see cref="AuditEntry.Hash"/> property is
    /// ignored (the formula omits it).
    /// </param>
    /// <returns>Hex-encoded SHA-256 (lowercase, 64 chars).</returns>
    public static string ComputeHash(string prevHash, AuditEntry entry)
    {
        ArgumentNullException.ThrowIfNull(prevHash);
        ArgumentNullException.ThrowIfNull(entry);

        string canonical = CanonicalJsonExcludingHash(entry);

        var prevBytes = Encoding.UTF8.GetBytes(prevHash);
        var jsonBytes = Encoding.UTF8.GetBytes(canonical);

        var combined = new byte[prevBytes.Length + jsonBytes.Length];
        Buffer.BlockCopy(prevBytes, 0, combined, 0,                prevBytes.Length);
        Buffer.BlockCopy(jsonBytes, 0, combined, prevBytes.Length, jsonBytes.Length);

        var digest = SHA256.HashData(combined);
        return Convert.ToHexString(digest).ToLowerInvariant();
    }

    /// <summary>
    /// Verifies that <paramref name="entries"/> form an unbroken hash chain
    /// per Architecture §10.5 / §10.7. Returns <c>false</c> on any mismatch.
    /// An empty list is treated as a valid (empty) chain.
    /// </summary>
    /// <param name="entries">Audit entries in append order.</param>
    public static bool VerifyChain(IReadOnlyList<AuditEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        if (entries.Count == 0) return true;

        string expectedPrev = GenesisPrevHash;
        long expectedSeq = 1L;
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            if (e is null)
                return false;

            if (e.Seq != expectedSeq++)
                return false;

            if (!string.Equals(e.PrevHash, expectedPrev, StringComparison.Ordinal))
                return false;

            var recomputed = ComputeHash(e.PrevHash, e);
            if (!string.Equals(e.Hash, recomputed, StringComparison.Ordinal))
                return false;

            expectedPrev = e.Hash;
        }
        return true;
    }

    private static string CanonicalJsonExcludingHash(AuditEntry e)
    {
        var dto = new EntryWithoutHash
        {
            Seq = e.Seq,
            Timestamp = e.Timestamp,
            Actor = e.Actor,
            Module = e.Module,
            Action = e.Action,
            TargetId = e.TargetId,
            Result = e.Result,
            PrevHash = e.PrevHash,
        };
        return JsonSerializer.Serialize(dto, CanonicalJsonOptions);
    }

    private static readonly JsonSerializerOptions CanonicalJsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    // Mirror of AuditEntry property order WITHOUT the Hash property.
    // If AuditEntry is ever reordered or has fields added/removed, this DTO
    // MUST be updated in lockstep — otherwise every previously-written chain
    // entry will fail verification.
    private sealed class EntryWithoutHash
    {
        public long Seq { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Actor { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public Guid TargetId { get; set; }
        public string Result { get; set; } = string.Empty;
        public string PrevHash { get; set; } = string.Empty;
    }
}
