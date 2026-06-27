// SPEC-DERIVED-PHASE3E  HALT #2
// Architecture.md Section 9.2.4 routing rules.
// Gap #262: module-local delivery adapter interface is derived.

using System.Threading.Tasks;
using TaskTree.Core.Models;

namespace TaskTree.Modules.BugReporter
{
    /// <summary>Module-local bug report delivery adapter contract.</summary>
    public interface IBugReportDeliveryAdapter
    {
        string Channel { get; }
        Task<BugReportDeliveryResult> DeliverAsync(BugReport report);
    }
}
