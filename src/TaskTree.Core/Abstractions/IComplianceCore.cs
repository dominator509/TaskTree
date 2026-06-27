// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Abstractions/IComplianceCore.cs
//  Purpose: HIPAA controls — hash-chained audit log, auto-logoff, PHI redaction per Architecture §4.6.
//  Architecture.md References: §4.6, §10.4, §10.5, §10.7, §9.2.3
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 2 — Interfaces)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: signatures match Architecture.md verbatim (or SPEC-DERIVED with owner approval).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTree.Core.Models;

namespace TaskTree.Core.Abstractions;

/// <summary>
/// Aggregates the HIPAA technical-safeguards surface: append-only hash-chained
/// audit log (§10.5), idle-detection / auto-logoff (§10.4), and PHI redaction
/// helpers (§9.2.3).
/// </summary>
public interface IComplianceCore
{
    /// <summary>Appends an entry to the hash-chained audit log.</summary>
    /// <param name="entry">
    /// The audit entry. Its <c>Hash</c> is computed from
    /// <c>SHA256(prevHash + canonicalJson(entryWithoutHash))</c> per §10.5.
    /// </param>
    Task AuditAsync(AuditEntry entry);

    /// <summary>Returns the full audit chain in append order.</summary>
    Task<IReadOnlyList<AuditEntry>> GetAuditChainAsync();

    /// <summary>
    /// Verifies that the audit chain is unbroken — every entry's hash matches
    /// <c>SHA256(prevHash + canonicalJson(entryWithoutHash))</c>.
    /// </summary>
    /// <returns><c>true</c> if every link is valid; otherwise <c>false</c>.</returns>
    Task<bool> VerifyChainIntegrityAsync();

    /// <summary>Raised when the idle monitor exceeds the configured timeout.</summary>
    event EventHandler AutoLogoffTriggered;

    /// <summary>
    /// Starts the OS idle monitor with the supplied inactivity threshold.
    /// </summary>
    /// <param name="timeout">Inactivity threshold (default 15 minutes per §10.4).</param>
    void StartIdleMonitor(TimeSpan timeout);

    /// <summary>
    /// Returns a copy of <paramref name="text"/> with PHI patterns masked per
    /// the balanced-strictness policy described in §9.2.3.
    /// </summary>
    /// <param name="text">Input text (may be <c>null</c> or empty).</param>
    /// <returns>The redacted text; never <c>null</c>.</returns>
    string RedactPhi(string text);
}
