// SPEC-DERIVED-PHASE2G  HALT #3/#5
// Non-PHI task-scoped snooze state. Gap #183: verify serialization/backward compatibility.

using System;
using TaskTree.Core.Enums;

namespace TaskTree.Core.Models
{
    public sealed record SnoozeState(Guid TaskId, DateTimeOffset SnoozedUntilUtc, SnoozeReason Reason, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc);
}
