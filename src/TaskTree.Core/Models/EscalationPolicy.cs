// SPEC-DERIVED-PHASE2G  HALT #6
// Gap #184: final escalation semantics require clinical/operational review.

using System;

namespace TaskTree.Core.Models
{
    public sealed record EscalationPolicy(TimeSpan InitialDelay, TimeSpan RepeatInterval, int MaxRepeats, bool EscalateWhenSessionLocked)
    {
        public static EscalationPolicy Default => new(TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(15), 3, false);
    }
}
