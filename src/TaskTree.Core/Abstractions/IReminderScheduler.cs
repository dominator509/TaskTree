// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Abstractions/IReminderScheduler.cs
//  Purpose: Periodic-tick reminder evaluator per Architecture §4.3 and §5.3.
//  Architecture.md References: §4.3, §5.3
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 2 — Interfaces)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: signatures match Architecture.md verbatim (or SPEC-DERIVED with owner approval).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Models;

namespace TaskTree.Core.Abstractions;

/// <summary>
/// Drives the periodic evaluation tick that inspects the task tree and raises
/// <see cref="ReminderDue"/> events according to the priority-weighted cadence
/// table in Architecture.md §5.3.
/// </summary>
/// <remarks>
/// Implementations MUST use <see cref="System.Threading.PeriodicTimer"/> and an
/// injected <see cref="IClock"/> (per Roadmap 1D anti-drift constraints).
/// </remarks>
public interface IReminderScheduler
{
    /// <summary>Starts the scheduler's periodic tick loop.</summary>
    /// <param name="ct">Token used to cooperatively stop the scheduler.</param>
    Task StartAsync(CancellationToken ct);

    /// <summary>Stops the scheduler and releases its timer resources.</summary>
    Task StopAsync();

    /// <summary>
    /// Raised when a task qualifies for a reminder per §5.3 cadence rules.
    /// </summary>
    event EventHandler<ReminderEvent> ReminderDue;

    /// <summary>
    /// Gets or sets the base evaluation cadence (typically 30 seconds; see §4.3).
    /// </summary>
    TimeSpan Cadence { get; set; }
}
