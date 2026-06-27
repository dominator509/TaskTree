// ============================================================================
// File: src/TaskTree.Core/Enums/ReminderReason.cs
// Module: TaskTree.Core.Enums
// Implements requirement: Architecture §4.3 (ReminderEvent reason field)
//                         Architecture §5.3 (cadence ladder)
//                         Roadmap P1D-AC3 ("ReminderDue includes node + reason")
// ----------------------------------------------------------------------------
// SPEC-DERIVED-PHASE1D
//   HALT #5 — three-value enum for Phase 1D
// Architecture amendment: see docs/Architecture.v1.0.2-delta.md (proposal).
// Escalation value intentionally omitted — Phase 2G EscalationPolicy.
// ============================================================================

namespace TaskTree.Core.Enums
{
    /// <summary>
    /// Reason a <see cref="Models.ReminderEvent"/> was raised by the
    /// <see cref="Abstractions.IReminderScheduler"/>.
    /// </summary>
    public enum ReminderReason
    {
        /// <summary>First fire for this task (Architecture §5.3 column 2).</summary>
        Initial = 0,

        /// <summary>Repeat fire per §5.3 column 3 cadence.</summary>
        Repeat = 1,

        /// <summary>Deadline has elapsed and task is not Done (§5.3 baseline detection).</summary>
        Overdue = 2,

        // NOTE: Escalation deferred to Phase 2G EscalationPolicy.
        // Adding it later is additive (enum value 3) — no breaking change.
    }
}
