// SPEC-DERIVED-PHASE3D  HALT #2
// Architecture.md Section 9.2.1 bug report payload schema.
// Gap #245: Architecture Section 3.3 Enums must add BugReportType.cs if Phase 3D ships.

namespace TaskTree.Core.Enums
{
    /// <summary>Bug report type values from Architecture.md Section 9.2.1.</summary>
    public enum BugReportType
    {
        Crash = 0,
        UserSubmitted = 1,
        Regression = 2,
    }
}
