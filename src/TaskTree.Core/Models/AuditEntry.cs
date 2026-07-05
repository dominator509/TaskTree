// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Models/AuditEntry.cs
//  Purpose: Audit-log entry per Architecture §10.5 JSON schema (hash-chained).
//  Architecture.md References: §10.5, §10.7, §4.6
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 3 — Models)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: fields match Architecture.md verbatim.
//  D10 anti-drift: XML doc on every public member.
//  Forward-reference: enum types referenced here ship in Phase 0 Msg 4.
// ─────────────────────────────────────────────────────────────────────────────

using System;

namespace TaskTree.Core.Models;

/// <summary>
/// Single entry in the hash-chained audit log per §10.5.
/// </summary>
/// <remarks>
/// Hash formula (normative, §10.5):
/// <c>SHA256(prevHash + canonicalJson(entryWithoutHash))</c>.
/// </remarks>
public sealed class AuditEntry
{
    /// <summary>Monotonically increasing sequence number.</summary>
    public long Seq { get; set; }

    /// <summary>
    /// UTC timestamp of the audited action (ISO 8601, millisecond precision
    /// per §10.5 example).
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>Windows user SID of the actor.</summary>
    public string Actor { get; set; } = string.Empty;

    /// <summary>Originating module name (e.g. <c>"TaskEngine"</c>).</summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>Action verb (e.g. <c>"TaskAdded"</c>).</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Identifier of the affected entity.</summary>
    public Guid TargetId { get; set; }

    /// <summary>Result code (e.g. <c>"success"</c>, <c>"failure"</c>).</summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>Hex-encoded SHA-256 of the previous chain entry.</summary>
    public string PrevHash { get; set; } = string.Empty;

    /// <summary>Hex-encoded SHA-256 of this entry per §10.5 formula.</summary>
    public string Hash { get; set; } = string.Empty;
}
