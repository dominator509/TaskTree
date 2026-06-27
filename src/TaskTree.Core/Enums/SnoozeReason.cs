// SPEC-DERIVED-PHASE2G  HALT #4
// Gap #182: Architecture Section 3.3 Enums must add SnoozeReason.cs.

namespace TaskTree.Core.Enums
{
    public enum SnoozeReason
    {
        UserRequested = 0,
        SessionLocked = 1,
        QuietHours = 2,
        EscalationDeferred = 3,
    }
}
