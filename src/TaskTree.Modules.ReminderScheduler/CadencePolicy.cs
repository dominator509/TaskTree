// ============================================================================
// File: src/TaskTree.Modules.ReminderScheduler/CadencePolicy.cs
// Binds: Architecture §5.3 Cadence Timing Table
// ----------------------------------------------------------------------------
// SPEC-DERIVED-PHASE1D (Msg 1 HALT #4/#5/#7/#8/#9/#12/#13)
// Msg 2: granted test-only access via InternalsVisibleTo. No body changes.
// ============================================================================

using System;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;

namespace TaskTree.Modules.ReminderScheduler
{
    internal static class CadencePolicy
    {
        public static TimeSpan GetInitialOffsetBeforeDeadline(Priority p) => p switch
        {
            Priority.Critical => TimeSpan.Zero,
            Priority.High     => TimeSpan.FromMinutes(30),
            Priority.Normal   => TimeSpan.FromHours(1),
            Priority.Low      => TimeSpan.FromHours(4),
            Priority.Trivial  => TimeSpan.Zero,
            _ => throw new ArgumentOutOfRangeException(nameof(p), p, "Unknown priority."),
        };

        public static TimeSpan GetRepeatCadence(Priority p) => p switch
        {
            Priority.Critical => TimeSpan.FromMinutes(5),
            Priority.High     => TimeSpan.FromMinutes(15),
            Priority.Normal   => TimeSpan.FromMinutes(30),
            Priority.Low      => TimeSpan.FromHours(2),
            Priority.Trivial  => TimeSpan.FromHours(8),
            _ => throw new ArgumentOutOfRangeException(nameof(p), p, "Unknown priority."),
        };

        public static bool ShouldFire(TaskNode node, DateTimeOffset? lastFiredUtc, DateTimeOffset nowUtc, out ReminderReason reason)
        {
            reason = ReminderReason.Initial;
            if (node is null) return false;

            var priority = node.Priority;

            if (!node.Deadline.HasValue)
            {
                var repeat = GetRepeatCadence(priority);
                if (priority != Priority.Critical) return false;
                if (lastFiredUtc is null) { reason = ReminderReason.Initial; return true; }
                if ((nowUtc - lastFiredUtc.Value) >= repeat) { reason = ReminderReason.Repeat; return true; }
                return false;
            }

            var deadline = node.Deadline.Value;

            if (deadline <= nowUtc)
            {
                var repeat = GetRepeatCadence(priority);
                if (lastFiredUtc is null || (nowUtc - lastFiredUtc.Value) >= repeat)
                { reason = ReminderReason.Overdue; return true; }
                return false;
            }

            if (lastFiredUtc.HasValue && (nowUtc - lastFiredUtc.Value) >= GetRepeatCadence(priority))
            { reason = ReminderReason.Repeat; return true; }

            if (lastFiredUtc is null)
            {
                switch (priority)
                {
                    case Priority.Critical: reason = ReminderReason.Initial; return true;
                    case Priority.Trivial: return false;
                    case Priority.High:
                    case Priority.Normal:
                    case Priority.Low:
                        var offset = GetInitialOffsetBeforeDeadline(priority);
                        if ((deadline - nowUtc) <= offset) { reason = ReminderReason.Initial; return true; }
                        return false;
                    default: throw new ArgumentOutOfRangeException(nameof(node.Priority), priority, "Unknown priority.");
                }
            }
            return false;
        }
    }
}
