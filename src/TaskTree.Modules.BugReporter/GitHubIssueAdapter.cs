// SPEC-DERIVED-PHASE3E  HALT #6/#7/#8
// Architecture.md Sections 4.8, 9.2.4, and 9.2.6 live GitHub Issues delivery.
// Gap #265/#266: live GitHub Issues delivery and remote label validation deferred to Phase 5E.

using System;
using System.Threading.Tasks;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;

namespace TaskTree.Modules.BugReporter
{
    /// <summary>HIGH-stub GitHub Issues delivery adapter.</summary>
    public sealed class GitHubIssueAdapter : IBugReportDeliveryAdapter
    {
        public string Channel => "GitHub";
        public string GetLabel(BugSeverity severity) => severity switch
        {
            BugSeverity.Critical => "critical",
            BugSeverity.High => "high",
            BugSeverity.Normal => "bug",
            BugSeverity.Low => "enhancement",
            _ => "bug",
        };
        public Task<BugReportDeliveryResult> DeliverAsync(BugReport report)
        {
            throw new NotImplementedException("HIGH: Live GitHub Issue delivery requires REST API + DPAPI-wrapped PAT - Codex Phase 5E");
        }
    }
}
