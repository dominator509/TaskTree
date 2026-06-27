// SPEC-DERIVED-PHASE3E  HALT #3/#4/#5
// Architecture.md Sections 4.8 and 9.2.6 live SMTP delivery.
// Gap #263/#264: live SMTP and DPAPI-wrapped SMTP config deferred to Phase 5E.

using System;
using System.Threading.Tasks;
using TaskTree.Core.Models;

namespace TaskTree.Modules.BugReporter
{
    /// <summary>HIGH-stub email delivery adapter.</summary>
    public sealed class EmailDeliveryAdapter : IBugReportDeliveryAdapter
    {
        public string Channel => "Email";
        public Task<BugReportDeliveryResult> DeliverAsync(BugReport report)
        {
            throw new NotImplementedException("HIGH: Live SMTP delivery requires credentials/runtime endpoint - Codex Phase 5E");
        }
    }
}
