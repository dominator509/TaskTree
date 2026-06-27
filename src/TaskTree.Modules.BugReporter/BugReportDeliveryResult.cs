// SPEC-DERIVED-PHASE3E  HALT #1
// Architecture.md Section 9.2.4 routing rules.
// Gap #261: delivery result contract is derived.

namespace TaskTree.Modules.BugReporter
{
    /// <summary>Result from a bug report delivery channel.</summary>
    public sealed record BugReportDeliveryResult(bool Success, string Channel, string Message);
}
