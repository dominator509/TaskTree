// =============================================================================
// TaskTree — ReminderEvent.cs (AMENDED — Architecture v1.0.2)
// Implements: Architecture.md §4.3 + v1.0.2 amendment (added Reason)
// Phase:      1D Msg 1 (HALT #3)
// SPEC-DERIVED-MSG3 §2 (original); Phase 1D §3 added Reason.
// CHANGE NOTE: This file SUPERSEDES the Phase 0 Msg 3 version. At Phase 5A repo
// stitching, the assemble-repo.ps1 duplicate detector MUST resolve to this newer
// copy. The original v1.0.0 ReminderEvent did NOT have Reason; the v1.0.2 spec
// requires it for P1D-AC3 compliance. See G1D-10.
// =============================================================================
using System;
using TaskTree.Core.Enums;

namespace TaskTree.Core.Models;

/// <summary>
/// Payload for <see cref="Abstractions.IReminderScheduler.ReminderDue"/>.
/// Identifies the task, when the reminder fired, and (v1.0.2+) why.
/// </summary>
public sealed class ReminderEvent
{
    /// <summary>Identifier of the <see cref="TaskNode"/> for which the reminder fired.</summary>
    public Guid TaskId { get; init; }

    /// <summary>Priority of the task at time of firing (1–5 per §5.3).</summary>
    public Priority Priority { get; init; }

    /// <summary>Original task deadline (immutable copy at fire time).</summary>
    public DateTimeOffset Deadline { get; init; }

    /// <summary>Time the reminder was fired (from injected <see cref="Abstractions.IClock"/>).</summary>
    public DateTimeOffset FiredAtUtc { get; init; }

    /// <summary>
    /// Why this reminder is firing.
    /// Added in Architecture v1.0.2 to satisfy Roadmap P1D-AC3.
    /// </summary>
    public ReminderReason Reason { get; init; }
}
