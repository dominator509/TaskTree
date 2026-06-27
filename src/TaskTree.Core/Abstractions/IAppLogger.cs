// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Abstractions/IAppLogger.cs
//  Purpose: Structured logger abstraction owned by Core per §3.3. Concrete: FileAppLogger (Msg 5).
//  Architecture.md References: §3.3, §10.5, §12, §9.2.3
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 2 — Interfaces)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: signatures match Architecture.md verbatim (or SPEC-DERIVED with owner approval).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

using System;

namespace TaskTree.Core.Abstractions;

/// <summary>
/// Application-wide structured logger. Backed at runtime by
/// <c>FileAppLogger</c> (Phase 0 Msg 5) and bridged to
/// <c>Microsoft.Extensions.Logging</c> per Architecture §12.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-MSG2: surface not specified verbatim in Architecture.md; derived from
/// documented usage and approved by human owner on 2026-05-26. See
/// docs/spec-derivations/PHASE0-MSG2-DERIVATIONS.md.
/// <para>
/// PHI redaction MUST occur via
/// <see cref="IComplianceCore.RedactPhi(string)"/> BEFORE values are passed to
/// any logger method (Architecture §9.2.3, §10.2). This logger does not redact
/// on its own.
/// </para>
/// </remarks>
public interface IAppLogger
{
    /// <summary>Writes a debug-level log line.</summary>
    /// <param name="message">Message template.</param>
    /// <param name="args">Structured arguments for the template.</param>
    void LogDebug(string message, params object?[] args);

    /// <summary>Writes an informational log line.</summary>
    /// <param name="message">Message template.</param>
    /// <param name="args">Structured arguments for the template.</param>
    void LogInformation(string message, params object?[] args);

    /// <summary>Writes a warning log line.</summary>
    /// <param name="message">Message template.</param>
    /// <param name="args">Structured arguments for the template.</param>
    void LogWarning(string message, params object?[] args);

    /// <summary>Writes an error log line, optionally with an exception.</summary>
    /// <param name="exception">Optional exception to include.</param>
    /// <param name="message">Message template.</param>
    /// <param name="args">Structured arguments for the template.</param>
    void LogError(Exception? exception, string message, params object?[] args);
}
