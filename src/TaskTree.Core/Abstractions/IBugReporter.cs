// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Abstractions/IBugReporter.cs
//  Purpose: Crash + user bug capture, redaction, queue, delivery per Architecture §4.8 / §9.2.
//  Architecture.md References: §4.8, §9.2
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 2 — Interfaces)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: signatures match Architecture.md verbatim (or SPEC-DERIVED with owner approval).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Threading.Tasks;
using TaskTree.Core.Models;

namespace TaskTree.Core.Abstractions;

/// <summary>
/// Captures crash and user-submitted bug reports, redacts PHI per §9.2.3, queues
/// them locally per §9.2.5, and routes them to email + GitHub Issues per §9.2.4.
/// </summary>
public interface IBugReporter
{
    /// <summary>Submits a bug report (queued locally; transmission is asynchronous).</summary>
    /// <param name="report">The <see cref="BugReport"/> to submit.</param>
    /// <returns>The persistent identifier assigned to the queued report.</returns>
    Task<Guid> SubmitAsync(BugReport report);

    /// <summary>Attempts to flush queued reports to their delivery channels.</summary>
    /// <returns>The number of reports successfully delivered.</returns>
    Task<int> FlushQueueAsync();

    /// <summary>Hooks <c>AppDomain.UnhandledException</c> and related global crash sources.</summary>
    void HookGlobalCrashHandler();

    /// <summary>Gets or sets whether PHI redaction is enforced before transmission.</summary>
    bool RedactionEnabled { get; set; }
}
